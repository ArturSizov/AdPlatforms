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
    private readonly CancellationToken _cancellationToken = default;

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
        _dataSourceMock.Setup(ds => ds.CreateAsync(It.IsAny<StreamReader>(), _cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _repository.LoadDataAsync(stream, _cancellationToken);

        // Assert
        Assert.True(result, "LoadDataAsync should return true when CreateAsync succeeds");
        Assert.True(_repository.IsDataLoaded, "IsDataLoaded should be true after successful load");
        _dataSourceMock.Verify(ds => ds.CreateAsync(It.IsAny<StreamReader>(), _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task LoadDataAsync_NullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.LoadDataAsync(null!, _cancellationToken));
    }

    [Fact]
    public async Task LoadDataAsync_NonReadableStream_ThrowsArgumentException()
    {
        // Arrange
        using var stream = new MemoryStream();
        stream.Close(); // Make stream non-readable

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.LoadDataAsync(stream, _cancellationToken));
    }

    [Fact]
    public async Task LoadDataAsync_CreateAsyncFails_ReturnsFalseAndSetsIsDataLoadedFalse()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _dataSourceMock.Setup(ds => ds.CreateAsync(It.IsAny<StreamReader>(), _cancellationToken)).ReturnsAsync(false);

        // Act
        var result = await _repository.LoadDataAsync(stream, _cancellationToken);

        // Assert
        Assert.False(result, "LoadDataAsync should return false when CreateAsync fails");
        Assert.False(_repository.IsDataLoaded, "IsDataLoaded should be false when CreateAsync fails");
        _dataSourceMock.Verify(ds => ds.CreateAsync(It.IsAny<StreamReader>(), _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_ValidLocation_ReturnsCorrectEntity()
    {
        // Arrange
        var location = "/ru";
        var platforms = new[] { "Яндекс.Директ" };
        _dataSourceMock.Setup(ds => ds.ReadAsync(location, _cancellationToken)).ReturnsAsync(platforms);

        // Act
        var result = await _repository.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(location, result.Location);
        Assert.Equal(platforms, result.Platforms);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_NonExistingLocation_ReturnsNull()
    {
        // Arrange
        var location = "/non/existing";
        _dataSourceMock.Setup(ds => ds.ReadAsync(location, _cancellationToken)).ReturnsAsync(Array.Empty<string>());

        // Act
        var result = await _repository.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.Null(result);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_CachesResult_CallsDataSourceOnlyOnce()
    {
        // Arrange
        var location = "/ru";
        var platforms = new[] { "Яндекс.Директ" };
        _dataSourceMock.Setup(ds => ds.ReadAsync(location, _cancellationToken)).ReturnsAsync(platforms);

        // Act
        var result1 = await _repository.GetPlatformsAsync(location, _cancellationToken);
        var result2 = await _repository.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Platforms, result2.Platforms);
        _dataSourceMock.Verify(ds => ds.ReadAsync(location, _cancellationToken), Times.Once()); // Data source called only once due to caching
    }

    [Fact]
    public async Task GetPlatformsAsync_NullLocation_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetPlatformsAsync(null!, _cancellationToken));
    }
}
