using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class GeoNamesDbContext : DbContext
{
#pragma warning disable CS8618
    public GeoNamesDbContext(DbContextOptions<GeoNamesDbContext> options) : base(options)
    {
    }
#pragma warning restore CS8618
    public DbSet<GeoName> Items { get; set; }
}