using AdPlatforms.Domain.Entities;
using AdPlatforms.Domain.Repositories;
using AdPlatforms.Domain.UseCases.Abstractions;
using Microsoft.Extensions.Logging;

namespace AdPlatforms.Domain.UseCases;

public class AdPlatformSelectorUseCase(ILogger<AdPlatformSelectorUseCase> logger, ILocationPlatformsRepository repository) : IAdPlatformSelectorUseCase
{
    public async Task<(bool IsSuccess, string? Error)> LoadDataAsync(Stream stream)
    {
        try
        {
            if (!await repository.LoadDataAsync(stream))
                return (false, "Failed to load data from the provided stream");

            return (true, null);
        }
        catch (Exception ex) when (ex is FormatException)
        {
            logger.LogError(ex, "Error processing file: {Message}", ex.Message);
            return (false, $"Error processing file: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error processing file");
            return (false, "Unexpected error processing file");
        }
    }

    public async Task<(LocationPlatformsEntity? Data, string? Error)> GetPlatformsAsync(string location)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(location))
                return (null, "Query data is empty");

            // Ensure data is loaded
            if (!repository.IsDataLoaded)
                return (null, "Data not loaded");


            // Ensure data is loaded
            var locationNode = await repository.GetPlatformsAsync(location);
            if (locationNode == null)
                return (null, $"Unknown location: {location}");

            return (locationNode, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving platforms for location '{Location}': {Message}", location, ex.Message);
            return (null, "Unexpected error retrieving platforms");
        }
    }
}
