using AdPlatforms.Domain.Entities;

namespace AdPlatforms.Domain.UseCases.Abstractions;

/// <summary>
/// Platform Selector Use Case Interface
/// </summary>
public interface IAdPlatformSelectorUseCase
{
    /// <summary>
    /// Returns the platform by location
    /// </summary>
    /// <param name="location"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(LocationPlatformsEntity? Data, string? Error)> GetPlatformsAsync(string location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loading data from stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool IsSuccess, string? Error)> LoadDataAsync(Stream stream, CancellationToken cancellationToken = default);
}
