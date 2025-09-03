using AdPlatforms.Domain.Entities;
using AdPlatforms.Domain.Repositories;
using AdPlatforms.Domain.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPlatforms.Tests;

public class AdPlatformSelectorUseCaseTests
{
    private readonly Mock<ILogger<AdPlatformSelectorUseCase>> _loggerMock;
    private readonly Mock<ILocationPlatformsRepository> _repositoryMock;
    private readonly AdPlatformSelectorUseCase _useCase;
    private readonly CancellationToken _cancellationToken = default;

    public AdPlatformSelectorUseCaseTests()
    {
        _loggerMock = new Mock<ILogger<AdPlatformSelectorUseCase>>();
        _repositoryMock = new Mock<ILocationPlatformsRepository>();
        _useCase = new AdPlatformSelectorUseCase(_loggerMock.Object, _repositoryMock.Object);
    }

    [Fact]
    public async Task LoadDataAsync_ValidStream_ReturnsSuccess()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _repositoryMock.Setup(r => r.LoadDataAsync(stream, _cancellationToken)).ReturnsAsync(true);

        // Act
        var (isSuccess, error) = await _useCase.LoadDataAsync(stream);

        // Assert
        Assert.True(isSuccess);
        Assert.Null(error);
        _repositoryMock.Verify(r => r.LoadDataAsync(stream, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task LoadDataAsync_RepositoryFails_ReturnsFailure()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _repositoryMock.Setup(r => r.LoadDataAsync(stream, _cancellationToken)).ReturnsAsync(false);

        // Act
        var (isSuccess, error) = await _useCase.LoadDataAsync(stream);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("Failed to load data from the provided stream", error);
        _repositoryMock.Verify(r => r.LoadDataAsync(stream, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task LoadDataAsync_FormatException_ReturnsErrorAndLogs()
    {
        // Arrange
        var input = "Invalid";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _repositoryMock.Setup(r => r.LoadDataAsync(stream, _cancellationToken)).ThrowsAsync(new FormatException("Invalid format"));

        // Act
        var (isSuccess, error) = await _useCase.LoadDataAsync(stream, _cancellationToken);

        // Assert
        Assert.False(isSuccess);
        Assert.StartsWith("Error processing file: Invalid format", error);
        _repositoryMock.Verify(r => r.LoadDataAsync(stream, _cancellationToken), Times.Once());
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error processing file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadDataAsync_UnexpectedException_ReturnsErrorAndLogs()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        _repositoryMock.Setup(r => r.LoadDataAsync(stream, _cancellationToken)).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var (isSuccess, error) = await _useCase.LoadDataAsync(stream);

        // Assert
        Assert.False(isSuccess);
        Assert.Equal("Unexpected error processing file", error);
        _repositoryMock.Verify(r => r.LoadDataAsync(stream, _cancellationToken), Times.Once());
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Unexpected error processing file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPlatformsAsync_EmptyLocation_ReturnsError()
    {
        // Arrange
        var location = "";

        // Act
        var (data, error) = await _useCase.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.Null(data);
        Assert.Equal("Query data is empty", error);
        _repositoryMock.Verify(r => r.GetPlatformsAsync(It.IsAny<string>(), _cancellationToken), Times.Never());
    }

    [Fact]
    public async Task GetPlatformsAsync_DataNotLoaded_ReturnsError()
    {
        // Arrange
        var location = "/ru";
        _repositoryMock.Setup(r => r.IsDataLoaded).Returns(false);

        // Act
        var (data, error) = await _useCase.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.Null(data);
        Assert.Equal("Data not loaded", error);
        _repositoryMock.Verify(r => r.GetPlatformsAsync(It.IsAny<string>(), _cancellationToken), Times.Never());
    }

    [Fact]
    public async Task GetPlatformsAsync_UnknownLocation_ReturnsError()
    {
        // Arrange
        var location = "/unknown";
        _repositoryMock.Setup(r => r.IsDataLoaded).Returns(true);
        _repositoryMock.Setup(r => r.GetPlatformsAsync(location, _cancellationToken)).ReturnsAsync((LocationPlatformsEntity?)null);

        // Act
        var (data, error) = await _useCase.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.Null(data);
        Assert.Equal($"Unknown location: {location}", error);
        _repositoryMock.Verify(r => r.GetPlatformsAsync(location, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_ValidLocation_ReturnsData()
    {
        // Arrange
        var location = "/ru";
        var entity = new LocationPlatformsEntity(location, new[] { "Яндекс.Директ" });
        _repositoryMock.Setup(r => r.IsDataLoaded).Returns(true);
        _repositoryMock.Setup(r => r.GetPlatformsAsync(location, _cancellationToken)).ReturnsAsync(entity);

        // Act
        var (data, error) = await _useCase.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.NotNull(data);
        Assert.Equal(entity, data);
        Assert.Null(error);
        _repositoryMock.Verify(r => r.GetPlatformsAsync(location, _cancellationToken), Times.Once());
    }

    [Fact]
    public async Task GetPlatformsAsync_Exception_ReturnsErrorAndLogs()
    {
        // Arrange
        var location = "/ru";
        _repositoryMock.Setup(r => r.IsDataLoaded).Returns(true);
        _repositoryMock.Setup(r => r.GetPlatformsAsync(location, _cancellationToken)).ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var (data, error) = await _useCase.GetPlatformsAsync(location, _cancellationToken);

        // Assert
        Assert.Null(data);
        Assert.Equal("Unexpected error retrieving platforms", error);
        _repositoryMock.Verify(r => r.GetPlatformsAsync(location, _cancellationToken), Times.Once());
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error retrieving platforms")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}