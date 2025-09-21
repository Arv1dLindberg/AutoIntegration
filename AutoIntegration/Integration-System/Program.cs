using EasyModbus;
using Integration_System.Data;
using Integration_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var cfg = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var cs = cfg.GetConnectionString("Default")
         ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Missing connection string 'Default'.");

var dbOpts = new DbContextOptionsBuilder<ItDbContext>()
    .UseSqlServer(cs)
    .Options;

string host = cfg["Ot:Host"] ?? "127.0.0.1";
int port = int.TryParse(cfg["Ot:Port"], out var p) ? p : 502;
int poll = int.TryParse(cfg["PollingSeconds"], out var s) ? s : 1;

Console.WriteLine($"Integration is running. OT={host}:{port}");

while (true)
{
    using var db = new ItDbContext(dbOpts);

    // Get the oldest new order
    var order = db.Orders
                  .Where(o => o.Status == OrderStatus.New)
                  .OrderBy(o => o.CreatedAt)
                  .FirstOrDefault();

    if (order is null)
    {
        Thread.Sleep(1000);
        continue;
    }

    try
    {
        ModbusClient client = null;
        try
        {
            client = new ModbusClient();
            client.IPAddress = host;
            client.Port = port;
            client.Connect();

            // Write order data (HR0=OrderId, HR1=Quantity)
            short orderId16 = (short)Math.Clamp(order.Id, short.MinValue, short.MaxValue);
            short qty16 = (short)Math.Clamp(order.Quantity, 0, short.MaxValue);

            client.WriteSingleRegister(0, orderId16);
            client.WriteSingleRegister(1, qty16);

            // Start job: coil[0] = true
            client.WriteSingleCoil(0, true);

            order.Status = OrderStatus.InProgress;
            db.SaveChanges();

            // Read progress (IR0) until it is done (DI0)
            int last = -1;
            while (true)
            {
                int produced = client.ReadInputRegisters(0, 1)[0];

                if (produced != last)
                {
                    last = produced;
                    db.ProductionLogs.Add(new ProductionLog
                    {
                        OrderId = order.Id,
                        ProducedCount = produced,
                        Message = "ProducedCount"
                    });
                    db.SaveChanges();

                    Console.WriteLine($"Order #{order.Id}: produced={produced}");
                }

                bool done = client.ReadDiscreteInputs(0, 1)[0];
                if (done)
                {
                    order.Status = OrderStatus.Completed;
                    db.SaveChanges();
                    Console.WriteLine($"Order #{order.Id} completed.");
                    break;
                }

                Thread.Sleep(poll * 1000);
            }
        }
        finally
        {
            // Close connection if it is open
            try { if (client != null && client.Connected) client.Disconnect(); } catch {}
        }
    }
    catch (Exception ex)
    {
        using var dbFail = new ItDbContext(dbOpts);
        var o2 = dbFail.Orders.Find(order.Id);
        if (o2 != null)
        {
            o2.Status = OrderStatus.Failed;
            o2.LastError = ex.Message;
            dbFail.SaveChanges();
        }
        Console.WriteLine($"Error while processing order #{order?.Id}: {ex.Message}");
        Thread.Sleep(1000);
    }
}