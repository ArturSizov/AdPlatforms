using AdPlatforms.Domain.Entities;

namespace AdPlatforms.Domain.Repositories;

/// <summary>
/// Defines methods for retrieving and loading location platforms data.
/// </summary>
public interface ILocationPlatformsRepository
{
    /// <summary>
    /// Indicates whether the location platform data has been successfully loaded.
    /// </summary>
    bool IsDataLoaded { get; }

    /// <summary>
    /// Asynchronously retrieves location platform data for the specified location.
    /// </summary>
    /// <param name="location">Loaction</param>
    /// <returns></returns>
    Task<LocationPlatformsEntity?> GetPlatformsAsync(string location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously loads location platform data from the provided <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">Stream to read data</param>
    /// <returns></returns>
    Task<bool> LoadDataAsync(Stream stream, CancellationToken cancellationToken = default);
}
