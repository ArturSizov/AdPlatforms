using AdPlatforms.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdPlatforms.Tests;

public class InMemoryLocationPlatformsDataSourceTests
{
    private readonly InMemoryLocationPlatformsDataSource _dataSource;
    private readonly CancellationToken _cancellationToken = default;

    public InMemoryLocationPlatformsDataSourceTests()
    {
        _dataSource = new InMemoryLocationPlatformsDataSource();
    }

    [Fact]
    public async Task CreateAsync_ValidInput_PopulatesCorrectPlatforms()
    {
        // Arrange
        var input = @"Яндекс.Директ:/ru
Ревдинский рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета уральских москвичей:/ru/msk,/ru/permobl,/ru/chelobl
Крутая реклама:/ru/svrd";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act
        var result = await _dataSource.CreateAsync(reader, _cancellationToken);

        // Assert
        Assert.True(result, "CreateAsync should return true for valid input");

        var ruPlatforms = await _dataSource.ReadAsync("/ru", _cancellationToken);
        Assert.Equal(new[] { "Яндекс.Директ" }, ruPlatforms);

        var mskPlatforms = await _dataSource.ReadAsync("/ru/msk", _cancellationToken);
        Assert.Equal(new[] { "Яндекс.Директ", "Газета уральских москвичей" }, mskPlatforms);

        var svrdPlatforms = await _dataSource.ReadAsync("/ru/svrd", _cancellationToken);
        Assert.Equal(new[] {"Яндекс.Директ", "Крутая реклама" }, svrdPlatforms);

        var revdaPlatforms = await _dataSource.ReadAsync("/ru/svrd/revda", _cancellationToken);
        Assert.Equal(new[] { "Яндекс.Директ", "Ревдинский рабочий", "Крутая реклама" }, revdaPlatforms);
    }

    [Fact]
    public async Task CreateAsync_EmptyInput_ReturnsTrueAndClearsData()
    {
        // Arrange
        var input = "";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act
        var result = await _dataSource.CreateAsync(reader, _cancellationToken);

        // Assert
        Assert.True(result, "CreateAsync should return true even for empty input");
        var platforms = await _dataSource.ReadAsync("/ru", _cancellationToken);
        Assert.Empty(platforms);
    }

    [Fact]
    public async Task CreateAsync_MalformedLine_ThrowsFormatException()
    {
        // Arrange
        var input = "InvalidLineNoColon";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _dataSource.CreateAsync(reader, _cancellationToken));
    }

    [Fact]
    public async Task CreateAsync_NoLocations_ThrowsFormatException()
    {
        // Arrange
        var input = "PlatformWithoutLocations:/";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _dataSource.CreateAsync(reader, _cancellationToken));
    }

    [Fact]
    public async Task CreateAsync_EmptyLines_IgnoresEmptyLines()
    {
        // Arrange
        var input = @"
Яндекс.Директ:/ru

Ревдинский рабочий:/ru/svrd/revda
";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act
        var result = await _dataSource.CreateAsync(reader, _cancellationToken);

        // Assert
        Assert.True(result, "CreateAsync should return true for valid input with empty lines");
        var ruPlatforms = await _dataSource.ReadAsync("/ru");
        Assert.Equal(new[] { "Яндекс.Директ" }, ruPlatforms);
        var revdaPlatforms = await _dataSource.ReadAsync("/ru/svrd/revda");
        Assert.Equal(new[] { "Яндекс.Директ", "Ревдинский рабочий" }, revdaPlatforms);
    }

    [Fact]
    public async Task ReadAsync_NonExistingLocation_ReturnsEmptyArray()
    {
        // Arrange
        var input = "Яндекс.Директ:/ru";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);
        await _dataSource.CreateAsync(reader, _cancellationToken);

        // Act
        var platforms = await _dataSource.ReadAsync("/non/existing", _cancellationToken);

        // Assert
        Assert.Empty(platforms);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceInPlatformAndLocations_TrimsCorrectly()
    {
        // Arrange
        var input = "  Platform One  :  /ru , /ru/msk  ";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        using var reader = new StreamReader(stream);

        // Act
        var result = await _dataSource.CreateAsync(reader, _cancellationToken);

        // Assert
        Assert.True(result, "CreateAsync should return true for input with whitespace");
        var ruPlatforms = await _dataSource.ReadAsync("/ru", _cancellationToken);
        Assert.Equal(new[] { "Platform One" }, ruPlatforms);
        var mskPlatforms = await _dataSource.ReadAsync("/ru/msk", _cancellationToken);
        Assert.Equal(new[] { "Platform One" }, mskPlatforms);
    }

    [Fact]
    public async Task CreateAsync_MultipleCalls_OverwritesPreviousData()
    {
        // Arrange
        var input1 = "OldPlatform:/ru";
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(input1));
        using var reader1 = new StreamReader(stream1);
        await _dataSource.CreateAsync(reader1, _cancellationToken);

        var input2 = "NewPlatform:/ru";
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(input2));
        using var reader2 = new StreamReader(stream2);

        // Act
        var result = await _dataSource.CreateAsync(reader2, _cancellationToken);

        // Assert
        Assert.True(result, "CreateAsync should return true for second call");
        var platforms = await _dataSource.ReadAsync("/ru");
        Assert.Equal(new[] { "NewPlatform" }, platforms);
    }

    [Fact]
    public async Task CreateAsync_NullReader_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _dataSource.CreateAsync(null!, _cancellationToken));
    }
}