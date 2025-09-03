namespace AdPlatforms.Domain.Entities;

/// <summary>
/// Represents a location with its associated platforms.
/// </summary>
/// <param name="Location">Location</param>
/// <param name="Platforms">Associated platforms</param>
public record LocationPlatformsEntity(string Location, string[] Platforms);
