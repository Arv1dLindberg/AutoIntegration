using IT_System.Data;
using IT_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var cs =
    config.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

var opts = new DbContextOptionsBuilder<ItDbContext>()
    .UseSqlServer(cs)
    .Options;

using var db = new ItDbContext(opts);
db.Database.Migrate();

while (true)
{
    Console.WriteLine("\nIT-System");
    Console.WriteLine("1) Lista ordrar");
    Console.WriteLine("2) Skapa ny order");
    Console.WriteLine("3) Avsluta");
    var choice = Console.ReadLine();

    if (choice == "1")
    {
        var orders = db.Orders.OrderByDescending(o => o.Id).ToList();
        foreach (var o in orders)
            Console.WriteLine($"#{o.Id} {o.Item} x{o.Quantity} [{o.Status}] {o.CreatedAt:yyyy-MM-dd HH:mm}");
    }
    else if (choice == "2")
    {
        Console.Write("Artikel: ");
        var item = Console.ReadLine() ?? "";
        Console.Write("Antal: ");
        if (!int.TryParse(Console.ReadLine(), out var qty) || qty <= 0)
        {
            Console.WriteLine("Ogiltigt antal.");
            continue;
        }

        var order = new Order { Item = item.Trim(), Quantity = qty };
        db.Orders.Add(order);
        db.SaveChanges();
        Console.WriteLine($"Skapade order #{order.Id}");
    }
    else if (choice == "3") break;
}