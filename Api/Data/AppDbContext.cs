using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Service> Services { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> contextOptions) : base(contextOptions)
    {
    }
}