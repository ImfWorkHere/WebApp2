using Domain;

namespace Application;

public interface IRepository
{
    public Task<IEnumerable<GeoName>> GetRangeAsync(GeoNamesSearchOptions options,CancellationToken token = default);

    public Task AddRangeAsync(IEnumerable<GeoName> items, CancellationToken token = default);

    public Task UpdateRangeAsync(IEnumerable<GeoName> items, CancellationToken token = default);

    public Task DeleteRangeAsync(IEnumerable<DeleteGeoName> deletes, CancellationToken token = default);
}