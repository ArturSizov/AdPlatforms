using AdPlatforms.Domain.Entities;

namespace AdPlatforms.Domain.UseCases.Abstractions;

public interface IAdPlatformSelectorUseCase
{
    Task<(LocationPlatformsEntity? Data, string? Error)> GetPlatformsAsync(string location);

    Task<(bool IsSuccess, string? Error)> LoadDataAsync(Stream stream);
}
