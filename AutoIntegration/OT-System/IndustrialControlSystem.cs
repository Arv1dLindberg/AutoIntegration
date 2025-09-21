using EasyModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OT_System
{
    internal class IndustrialControlSystem
    {
        private static volatile bool isBusy = false;
        private static readonly object _lock = new();

        public void Run()
        {
            Console.WriteLine("Simulated OT system with Modbus support");

            Thread modbusThread = new Thread(StartEasyModbusTcpSlave);
            modbusThread.IsBackground = true;
            modbusThread.Start();

            while (true) Thread.Sleep(1000);
        }

        public static void StartEasyModbusTcpSlave()
        {
            int port = 502;
            ModbusServer modbusServer = new ModbusServer { Port = port };

            // --- Event Handlers for EasyModbus ---

            // Event for when Coils (Digital Outputs) are written to by a client
            modbusServer.CoilsChanged += (int startAddress, int numberOfCoils) =>
            {
                Console.WriteLine($"CoilsChanged at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Coils: {numberOfCoils}");

                const int maxCoilAddress = 1999;
                for (int i = 0; i < numberOfCoils; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address <= maxCoilAddress)
                        Console.WriteLine($"    Coil[{address}] = {modbusServer.coils[address]}");
                    else
                        Console.WriteLine($"    Warning: Coil[{address}] out of bounds.");
                }

                // Start job when coil[0] is true
                if (modbusServer.coils[0])
                {
                    lock (_lock)
                    {
                        if (isBusy) { Console.WriteLine("  Machine busy – start ignored."); return; }
                        isBusy = true;
                    }

                    int orderId = modbusServer.holdingRegisters[0];
                    int qty = modbusServer.holdingRegisters[1];
                    if (qty < 0) qty = 0;

                    Console.WriteLine($"--> START order {orderId} (qty {qty})");

                    // Reset status
                    modbusServer.inputRegisters[0] = 0;
                    modbusServer.discreteInputs[0] = false;

                    // Simulate production
                    new Thread(() =>
                    {
                        try
                        {
                            for (int n = 0; n < qty; n++)
                            {
                                Thread.Sleep(500); // unit time
                                // cast to 16-bit value
                                short produced = (short)Math.Min(n + 1, short.MaxValue);
                                modbusServer.inputRegisters[0] = produced; // progress
                            }

                            modbusServer.discreteInputs[0] = true; // done
                            Console.WriteLine($"<-- DONE order {orderId} (produced {qty})");
                        }
                        finally
                        {
                            modbusServer.coils[0] = false; // reset start coil
                            lock (_lock) isBusy = false;
                        }
                    }).Start();
                }
            };

            modbusServer.HoldingRegistersChanged += (int startAddress, int numberOfRegisters) =>
            {
                Console.WriteLine($"HoldingRegistersChanged at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Registers: {numberOfRegisters}");

                const int maxRegisterAddress = 1999;
                for (int i = 0; i < numberOfRegisters; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address <= maxRegisterAddress)
                        Console.WriteLine($"    HoldingRegister[{address}] = {modbusServer.holdingRegisters[address]}");
                    else
                        Console.WriteLine($"    Warning: HoldingRegister[{address}] out of bounds.");
                }
            };

            // Initial values
            modbusServer.inputRegisters[0] = 0;
            modbusServer.discreteInputs[0] = false;

            try
            {
                Console.WriteLine($"Starting EasyModbus TCP Slave on port {port}...");
                modbusServer.Listen();
                Console.WriteLine("EasyModbus TCP Slave started. Press any key to exit.");
                Console.ReadKey();
                Console.WriteLine("Stopping EasyModbus TCP Slave...");
                Console.WriteLine("EasyModbus TCP Slave stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}