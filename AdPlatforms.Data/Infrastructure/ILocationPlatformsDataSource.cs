namespace AdPlatforms.Data.Infrastructure;

/// <summary>
/// Defines methods for creating and reading location platform data from a data source.
/// </summary>
public interface ILocationPlatformsDataSource
{
    /// <summary>
    /// Asynchronously creates location platform data from the provided <see cref="StreamReader"/>. 
    /// The reader must be dedicated to this call and not shared across threads to ensure thread-safety.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    Task<bool> CreateAsync(StreamReader reader, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously reads platform data for the specified location.
    /// </summary>
    /// <param name="location">Location</param>
    /// <returns></returns>
    Task<string[]> ReadAsync(string location, CancellationToken cancellationToken = default);
}
