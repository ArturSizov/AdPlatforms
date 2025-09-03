using AdPlatforms.Data.Infrastructure;
using AdPlatforms.Data.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPlatforms.Tests;

public class LocationPlatformsRepositoryTests
{
    private readonly Mock<ILocationPlatformsDataSource> _dataSourceMock;
    private readonly LocationPlatformsRepository _repository;

    public LocationPlatformsRepositoryTests()
    {
        _dataSourceMock = new Mock<ILocationPlatformsDataSource>();
        _repository = new LocationPlatformsRepository(_dataSourceMock.Object);
    }

    [Fact]
    public async Task LoadDataAsync_ValidStream_CallsCreateAsyncAndSetsIsDataLoaded()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _dataSourceMock.Setup(ds => ds.CreateAsync(It.IsAny<StreamReader>())).ReturnsAsync(true);

        // Act
        var result = await _repository.LoadDataAsync(stream);

        // Assert
        Assert.True(result, "LoadDataAsync should return true when CreateAsync succeeds");
        Assert.True(_repository.IsDataLoaded, "IsDataLoaded should be true after successful load");
        _dataSourceMock.Verify(ds => ds.CreateAsync(It.IsAny<StreamReader>()), Times.Once());
    }

    [Fact]
    public async Task LoadDataAsync_NullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.LoadDataAsync(null!));
    }

    [Fact]
    public async Task LoadDataAsync_NonReadableStream_ThrowsArgumentException()
    {
        // Arrange
        using var stream = new MemoryStream();
        stream.Close(); // Make stream non-readable

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.LoadDataAsync(stream));
    }

    [Fact]
    public async Task LoadDataAsync_CreateAsyncFails_ReturnsFalseAndSetsIsDataLoadedFalse()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _dataSourceMock.Setup(ds => ds.CreateAsync(It.IsAny<StreamReader>())).ReturnsAsync(false);

        // Act
        var result = await _repository.LoadDataAsync(stream);

        // Assert
        Assert.False(result, "LoadDataAsync should return false when CreateAsync fails");
        Assert.False(_repository.IsDataLoaded, "IsDataLoaded should be false when CreateAsync fails");
        _dataSourceMock.Verify(ds => ds.CreateAsync(It.IsAny<StreamReader>()), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_ValidLocation_ReturnsCorrectEntity()
    {
        // Arrange
        var location = "/ru";
        var platforms = new[] { "Яндекс.Директ" };
        _dataSourceMock.Setup(ds => ds.ReadAsync(location)).ReturnsAsync(platforms);

        // Act
        var result = await _repository.GetPlatformsAsync(location);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(location, result.Location);
        Assert.Equal(platforms, result.Platforms);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_NonExistingLocation_ReturnsNull()
    {
        // Arrange
        var location = "/non/existing";
        _dataSourceMock.Setup(ds => ds.ReadAsync(location)).ReturnsAsync(Array.Empty<string>());

        // Act
        var result = await _repository.GetPlatformsAsync(location);

        // Assert
        Assert.Null(result);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_CachesResult_CallsDataSourceOnlyOnce()
    {
        // Arrange
        var location = "/ru";
        var platforms = new[] { "Яндекс.Директ" };
        _dataSourceMock.Setup(ds => ds.ReadAsync(location)).ReturnsAsync(platforms);

        // Act
        var result1 = await _repository.GetPlatformsAsync(location);
        var result2 = await _repository.GetPlatformsAsync(location);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Platforms, result2.Platforms);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location), Times.Once()); // Data source called only once due to caching
    }

    [Fact]
    public async Task GetPlatformsAsync_NullLocation_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetPlatformsAsync(null!));
    }
}
