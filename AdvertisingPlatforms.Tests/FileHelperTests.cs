using AdvertisingPlatforms.API.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdvertisingPlatforms.Tests;

public class FileHelperTests
{
    private readonly FileHelper _fileHelper;
    private readonly Mock<ILogger<FileHelper>> _loggerMock;

    public FileHelperTests()
    {
        _loggerMock = new Mock<ILogger<FileHelper>>();
        _fileHelper = new FileHelper(_loggerMock.Object);
    }

    [Fact]
    public async Task ReadLinesAsync_ValidFile_ReturnsLines()
    {
        // Arrange
        var fileContent = "Яндекс.Директ:/ru\nГазета уральских москвичей:/ru/msk";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        // Act
        var lines = await _fileHelper.ReadLinesAsync(stream).ToListAsync();

        // Assert
        lines.Should().HaveCount(2);
        lines[0].Should().Be("Яндекс.Директ:/ru");
        lines[1].Should().Be("Газета уральских москвичей:/ru/msk");
    }

    [Fact]
    public async Task ReadLinesAsync_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var lines = await _fileHelper.ReadLinesAsync(stream).ToListAsync();

        // Assert
        lines.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadLinesAsync_FileWithWhitespaceLines_SkipsEmptyLines()
    {
        // Arrange
        var fileContent = "Яндекс.Директ:/ru\n\nГазета уральских москвичей:/ru/msk\n  ";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));

        // Act
        var lines = await _fileHelper.ReadLinesAsync(stream).ToListAsync();

        // Assert
        lines.Should().HaveCount(2);
        lines[0].Should().Be("Яндекс.Директ:/ru");
        lines[1].Should().Be("Газета уральских москвичей:/ru/msk");
    }
}