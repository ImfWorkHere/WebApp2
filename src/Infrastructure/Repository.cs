using Application;
using Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Infrastructure;

internal class Repository : IRepository
{
    private readonly IDbContextFactory<GeoNamesDbContext> _factory;
    private readonly ILogger _log;

    public Repository(IDbContextFactory<GeoNamesDbContext> factory)
    {
        _log = Log.ForContext<Repository>();
        _factory = factory;
    }

    public async Task<IEnumerable<GeoName>> GetRangeAsync(GeoNamesSearchOptions options, CancellationToken token = default)
    {
        try
        {
            await using var context = await _factory.CreateDbContextAsync(token);
            _log.Information("Getting range of geo names from db.");

            var border = double.Round(options.Radius / 111.3d, 5);
            var items = await context.Items
	            .Where(i => i.Name.Contains(options.Name))
	            .Where(i => Math.Abs(options.Latitude - i.Latitude) <= border)
                .Where(i => Math.Abs(options.Longitude - i.Longitude) <= border)
                .Where(i => i.Population >= options.MinimumPopulation)
                .Where(i => i.Population <= options.MaximumPopulation)
                .ToListAsync(token);

            var lat = options.Latitude;
            var lon = options.Longitude;

            items = items
                .Where(i => i.GetDistance(ref lat, ref lon) <= options.Radius)
                .OrderBy(i => i.GetDistance(ref lat, ref lon))
                .Take(options.Count)
                .ToList() ;

            return items.Any() ? items : Enumerable.Empty<GeoName>();
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get items from db.", ex);
        }
    }

    public async Task AddRangeAsync(IEnumerable<GeoName> items, CancellationToken token = default)
    {
        try
        {
            await using var context = await _factory.CreateDbContextAsync(token);
            _log.Information("Adding range of geonames to db.");

            var entities = items as GeoName[] ?? items.ToArray();
            if (entities.Any())
            {
                await context.Items.AddRangeAsync(entities, token);
                await context.SaveChangesAsync(token);
            }
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to add items to db.", ex);
        }
    }

    // TODO : Repair update
    public async Task UpdateRangeAsync(IEnumerable<GeoName> items, CancellationToken token = default)
    {
        try
        {
            _log.Information("Updating range of geo names in db.");
            var deletes = new List<GeoName>();
            var entities = items.ToArray();

            await using (var context = await _factory.CreateDbContextAsync(token))
            {
                // Temp part, coz update is unusable this case
                foreach (var entity in entities)
                {
                    var item = await context.Items.FirstOrDefaultAsync(
                        i => i.Name.ToLower().Trim()
                            .Equals(entity.Name.ToLower().Trim()), token);

                    if (item != null)
                        deletes.Add(item);
                }
            }

            await using (var context = await _factory.CreateDbContextAsync(token))
            {
                if (deletes.Any()) context.Items.RemoveRange(deletes);

                if (entities.Any())
                {
                    await context.Items.AddRangeAsync(entities, token);
                    await context.SaveChangesAsync(token);
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new DbUpdateException("Failed to update db.", ex);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<DeleteGeoName> deletes, CancellationToken token = default)
    {
        try
        {
            await using var context = await _factory.CreateDbContextAsync(token);
            _log.Information("Deleting range of geo names from db.");
            var items = new List<GeoName>();

            foreach (var delete in deletes)
            {
                var item = await context.Items.FirstOrDefaultAsync(
                    i => i.Name.ToLower().Trim()
                        .Equals(delete.Name.ToLower().Trim()), token);

                if (item != null)
                    items.Add(item);
            }

            if (items.Any())
            {
                context.Items.RemoveRange(items);
                await context.SaveChangesAsync(token);
            }
        }
        catch (TaskCanceledException ex)
        {
            throw new TaskCanceledException("A task was cancelled.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to delete item from db.", ex);
        }
    }
}