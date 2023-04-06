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

    private RepositoryStatus _status;

    public Planner(ILogger<Planner> logger, DataSource dataSource, IRepository repository)
    {
        _logger = logger;
        _dataSource = dataSource;
        _repository = repository;

        _status = new RepositoryStatus();
        _tokenSource = new CancellationTokenSource();
        CheckStatus().Wait();
    }

    public async Task StartAsync(CancellationToken token = default)
    {
        try
        {
            if (_status.IsInitialized == false)
            {
                var items = await _dataSource.GetFullDbAsync(token);
                await _repository.AddRangeAsync(items, token);
                _status.UpdateDate = DateTime.UtcNow;;
            }

            CatchTime(token).RunSynchronously();
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


    private async Task CheckStatus()
    {
        var items = await _repository.GetRangeAsync(new GeoNamesSearchOptions()
        {
            Radius = 10000
        });

        if (items.Any())
        {
            _status.IsInitialized = true;
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

            UpdateDb(_tokenSource.Token).RunSynchronously();
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

                _status.UpdateDate = DateTime.UtcNow;
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