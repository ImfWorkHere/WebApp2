using Application;
using Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace Infrastructure;

/// <summary>
/// A <see cref="IHostedService"/> that copy data from <see cref="DataSource"/>, paste in <see cref="IRepository"/> and call GC at the middle of night.
/// </summary>
internal class Planner : IHostedService
{
    private readonly DataSource _dataSource;
    private readonly ILogger<Planner> _logger;
    private readonly IRepository _repository;
    private readonly CancellationTokenSource _tokenSource;

    public Planner(ILogger<Planner> logger, DataSource dataSource, IRepository repository)
    {
        _logger = logger;
        _dataSource = dataSource;
        _repository = repository;

        _tokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync(CancellationToken token = default)
    {
        try
        {
	        var anyCitySearchOptions = new GeoNamesSearchOptions()
	        {
		        Radius = 10000,
                MinimumPopulation = 1000000
	        };
	        var isInitialized = (await _repository.GetRangeAsync(anyCitySearchOptions, token)).Any();

			if (!isInitialized)
            {
                var items = await _dataSource.GetFullDbAsync(token);
                await _repository.AddRangeAsync(items, token);
            }

			Task.Run(() => CatchTime(token), token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Found errors during updating db.");
        }
    }

    public async Task StopAsync(CancellationToken token = default)
    {
        try
        {
            await Task.Run(() => _tokenSource.Cancel(), token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop updating db");
        }
    }

    private async Task CatchTime(CancellationToken token = default)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

            while (token.IsCancellationRequested == false)
            {
                if (DateTime.UtcNow.Hour == 4)
                    break;

                await timer.WaitForNextTickAsync(token);
            }

            UpdateDb(_tokenSource.Token).Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to catch needed time");
        }
    }

    private async Task UpdateDb(CancellationToken token = default)
    {
        try
        {
            var timer = new PeriodicTimer(TimeSpan.FromDays(1));

            while (token.IsCancellationRequested == false)
            {
                var deletes = await _dataSource.GetDeletesAsync(token);
                await _repository.DeleteRangeAsync(deletes, token);

                var items = await _dataSource.GetModificationsAsync(token);
                await _repository.UpdateRangeAsync(items, token);

                _logger.LogInformation($"Db was updated at {DateTime.Now}");
                GC.Collect();

                await timer.WaitForNextTickAsync(token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update db or get updates from data source.");
        }
    }
}