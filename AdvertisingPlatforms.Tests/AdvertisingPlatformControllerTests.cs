using AdvertisingPlatforms.API.Controllers;
using AdvertisingPlatforms.API.Contracts.Requests;
using AdvertisingPlatforms.API.Interfaces;
using AdvertisingPlatforms.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AdvertisingPlatforms.Tests;

public class AdvertisingPlatformControllerTests
{
    private readonly Mock<IAdvertisingPlatformService> _serviceMock;
    private readonly Mock<IFileHelper> _fileHelperMock;
    private readonly AdvertisingPlatformController _controller;

    public AdvertisingPlatformControllerTests()
    {
        _serviceMock = new Mock<IAdvertisingPlatformService>();
        _fileHelperMock = new Mock<IFileHelper>();
        _controller = new AdvertisingPlatformController(_serviceMock.Object, _fileHelperMock.Object);
    }

    [Fact]
    public async Task UploadFile_ValidFile_ReturnsOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Яндекс.Директ:/ru"));
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        var request = new UploadFileRequest(fileMock.Object);
        var lines = new List<string> { "Яндекс.Директ:/ru" }.ToAsyncEnumerable();

        _fileHelperMock.Setup(f => f.ReadLinesAsync(It.IsAny<Stream>())).Returns(lines);
        _serviceMock.Setup(s => s.LoadFromFileAsync(It.IsAny<IAsyncEnumerable<string>>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UploadFile(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Файл успешно загружен." });
    }

    [Fact]
    public async Task UploadFile_InvalidFile_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(""));
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        var request = new UploadFileRequest(fileMock.Object);

        _serviceMock.Setup(s => s.LoadFromFileAsync(It.IsAny<IAsyncEnumerable<string>>()))
            .ThrowsAsync(new InvalidDataException("Ошибка обработки файла."));

        // Act
        var result = await _controller.UploadFile(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { error = "Ошибка обработки файла." });
    }

    [Fact]
    public async Task GetPlatforms_ExistingLocation_ReturnsOk()
    {
        // Arrange
        var location = "/ru";
        var platforms = new List<string> { "Яндекс.Директ" };
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(location)).ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetPlatforms(location);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(platforms);
    }

    [Fact]
    public async Task GetPlatforms_NonExistingLocation_ReturnsNotFound()
    {
        // Arrange
        var location = "/us";
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(location))
            .ThrowsAsync(new KeyNotFoundException("Указанная локация не существует."));

        // Act
        var result = await _controller.GetPlatforms(location);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Указанная локация не существует.");
    }

    [Fact]
    public async Task GetPlatforms_NoDataLoaded_Returns503()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Данные о рекламных площадках не загружены."));

        // Act
        var result = await _controller.GetPlatforms("/ru");

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task GetPlatforms_InternalError_Returns500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        // Act
        var result = await _controller.GetPlatforms("/ru");

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}