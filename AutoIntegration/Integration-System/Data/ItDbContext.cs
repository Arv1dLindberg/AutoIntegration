using Integration_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration_System.Data;

public class ItDbContext : DbContext
{
    public ItDbContext(DbContextOptions<ItDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();
}
