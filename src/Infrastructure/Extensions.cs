using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Data source to update our db
        services.AddSingleton<DataSource>();

        // Current db
        services.AddDbContextFactory<GeoNamesDbContext>(opt =>
        {
            opt.UseSqlServer(config.GetConnectionString("GeoNames"));
        });

        // Abstract db, call her in DI
        services.AddTransient<IRepository, Repository>();

        services.AddHostedService<Planner>();

        return services;
    }

    public static double GetDistance(this GeoName name, ref double latitude, ref double longitude)
    {
        const double radius = 6378.137d; // Radius of earth in KM
        const double halfPi = Math.PI / 180d;

        var dLat = Math.Abs(name.Latitude - latitude) * halfPi;
        var dLon = Math.Abs(name.Longitude - longitude) * halfPi;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(name.Latitude * halfPi) * Math.Cos(name.Latitude * halfPi) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = radius * c;

        return distance;
    }
}