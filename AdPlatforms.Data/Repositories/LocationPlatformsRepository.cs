using AdPlatforms.Data.Infrastructure;
using AdPlatforms.Domain.Entities;
using AdPlatforms.Domain.Repositories;
using System.Collections.Concurrent;

namespace AdPlatforms.Data.Repositories;

/// <summary>
/// Provides methods for managing and retrieving location node data from a data source.
/// </summary>
/// <remarks>This repository is responsible for loading location node data from a stream, parsing it into a
/// structured format, and saving it to the underlying data source. It also provides functionality to retrieve the
/// stored location node data.</remarks>
/// <param name="dataSource"></param>
public class LocationPlatformsRepository(ILocationPlatformsDataSource dataSource) : ILocationPlatformsRepository
{
    private volatile bool _isDataLoaded;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private readonly ConcurrentDictionary<string, Task<string[]>> _cache = [];

    public bool IsDataLoaded => _isDataLoaded;

    /// <inheritdoc/>
    public async Task<bool> LoadDataAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new ArgumentException("Stream must support reading.", nameof(stream));

        // Ensure only one load operation at a time
        await _loadLock.WaitAsync(cancellationToken);

        try
        {
            // Clear cache to ensure consistency with the data source
            _cache.Clear();

            // Load data into the data source
            using var reader = new StreamReader(stream);
            _isDataLoaded = await dataSource.CreateAsync(reader, cancellationToken);

            return _isDataLoaded;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<LocationPlatformsEntity?> GetPlatformsAsync(string location, CancellationToken cancellationToken = default)
    {
        // Retrieve or add the task to fetch platforms for the location
        var task = _cache.GetOrAdd(location, async loc =>
        {
            var data = await dataSource.ReadAsync(loc, cancellationToken);
            return data ?? [];
        });

        // Await the task to get the platforms
        var platforms = await task;

        // Return null if no platforms found, otherwise return the entity
        return platforms.Length == 0
            ? null
            : new LocationPlatformsEntity(location, platforms);
    }
}


