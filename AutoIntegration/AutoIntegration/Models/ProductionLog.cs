using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT_System.Models;

public class ProductionLog
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double TemperatureC { get; set; }
    public string? Message { get; set; }
}
