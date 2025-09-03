using System.Collections.Concurrent;

namespace AdPlatforms.Data.Infrastructure;

/// <summary>
/// Provides an in-memory implementation of the <see cref="ILocationPlatformsDataSource"/> interface.
/// </summary>
public class InMemoryLocationPlatformsDataSource : ILocationPlatformsDataSource
{
    private readonly ConcurrentDictionary<string, string[]> _locationPlatforms = [];

    public async Task<bool> CreateAsync(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        // Clear existing data
        _locationPlatforms.Clear();

        var platformLocations = new Dictionary<string, List<string>>();
        var allLocations = new HashSet<string>();

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Split line into platform and locations
            var parts = line.Split(':', 2);

            // Check for malformed lines
            if (parts.Length < 2)
                throw new FormatException($"Invalid line format: '{line}'");

            var platform = parts[0].Trim();
            var concatLocs = parts[1].Trim();

            var locations = concatLocs
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            // Check for valid locations
            if (locations.Count == 0)
                throw new FormatException($"No valid locations for platform '{platform}' in line: '{line}'");

            // Map platform to its locations
            platformLocations[platform] = locations;

            // Add locations to the global set
            foreach (var location in locations)
            {
                if (string.IsNullOrWhiteSpace(location.Trim('/')))
                    throw new FormatException($"Invalid location '/' for platform '{platform}' in line: '{line}'");

                allLocations.Add(location);
            }
        }

        // Map each location to its suitable platforms
        foreach (var location in allLocations)
        {
            var suitable = new List<string>();

            foreach (var (platform, candidates) in platformLocations)
            {
                if (candidates.Any(location.StartsWith))
                    suitable.Add(platform);
            }

            _locationPlatforms.TryAdd(location, [.. suitable]);
        }

        return true;
    }

    public Task<string[]> ReadAsync(string location)
    {
        if (_locationPlatforms.TryGetValue(location, out var platforms))
            return Task.FromResult(platforms);

        return Task.FromResult<string[]>([]);
    }
}
