using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class TempDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder
    optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql("");
    }
}