using IT_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT_System.Data;

public class ItDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<ProductionLog> ProductionLogs => Set<ProductionLog>();

    public ItDbContext(DbContextOptions<ItDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>().Property(o => o.Item).HasMaxLength(100).IsRequired();
        b.Entity<Order>().HasIndex(o => o.Status);
        b.Entity<ProductionLog>().HasIndex(p => p.OrderId);
    }
}