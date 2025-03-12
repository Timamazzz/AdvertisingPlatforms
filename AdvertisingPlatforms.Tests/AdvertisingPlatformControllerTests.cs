using AdvertisingPlatforms.API.Controllers;
using AdvertisingPlatforms.API.Contracts.Requests;
using AdvertisingPlatforms.API.Interfaces;
using AdvertisingPlatforms.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdvertisingPlatforms.Tests;

public class AdvertisingPlatformControllerTests
{
    private readonly Mock<IAdvertisingPlatformService> _serviceMock;
    private readonly Mock<IFileHelper> _fileHelperMock;
    private readonly Mock<ILogger<AdvertisingPlatformController>> _loggerMock;
    private readonly AdvertisingPlatformController _controller;

    public AdvertisingPlatformControllerTests()
    {
        _serviceMock = new Mock<IAdvertisingPlatformService>();
        _fileHelperMock = new Mock<IFileHelper>();
        _loggerMock = new Mock<ILogger<AdvertisingPlatformController>>();
        _controller =
            new AdvertisingPlatformController(_serviceMock.Object, _fileHelperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadFile_ValidFile_ReturnsOk()
    {
        var fileMock = new Mock<IFormFile>();
        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Яндекс.Директ:/ru"));
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        var request = new UploadFileRequest(fileMock.Object);
        var lines = new List<string> { "Яндекс.Директ:/ru" }.ToAsyncEnumerable();

        _fileHelperMock.Setup(f => f.ReadLinesAsync(It.IsAny<Stream>())).Returns(lines);
        _serviceMock.Setup(s => s.LoadFromFileAsync(It.IsAny<IAsyncEnumerable<string>>())).Returns(Task.CompletedTask);

        var result = await _controller.UploadFile(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Файл успешно загружен." });
    }

    [Fact]
    public async Task UploadFile_InvalidFile_ReturnsBadRequest()
    {
        var fileMock = new Mock<IFormFile>();
        var fileStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(""));
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);
        fileMock.Setup(f => f.Length).Returns(fileStream.Length);
        var request = new UploadFileRequest(fileMock.Object);

        _serviceMock.Setup(s => s.LoadFromFileAsync(It.IsAny<IAsyncEnumerable<string>>()))
            .ThrowsAsync(new InvalidDataException("Ошибка обработки файла."));

        var act = async () => await _controller.UploadFile(request);

        await act.Should().ThrowAsync<InvalidDataException>()
            .WithMessage("Ошибка обработки файла.");
    }

    [Fact]
    public async Task GetPlatforms_ExistingLocation_ReturnsOk()
    {
        var location = "/ru";
        var platforms = new List<string> { "Яндекс.Директ" };
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(location)).ReturnsAsync(platforms);

        var result = await _controller.GetPlatforms(location);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(platforms);
    }

    [Fact]
    public async Task GetPlatforms_NonExistingLocation_ReturnsNotFound()
    {
        var location = "/us";
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(location))
            .ThrowsAsync(new KeyNotFoundException("Указанная локация не существует."));

        var act = async () => await _controller.GetPlatforms(location);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Указанная локация не существует.");
    }

    [Fact]
    public async Task GetPlatforms_NoDataLoaded_Returns503()
    {
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Данные о рекламных площадках не загружены."));

        var act = async () => await _controller.GetPlatforms("/ru");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Данные о рекламных площадках не загружены.");
    }

    [Fact]
    public async Task GetPlatforms_InternalError_Returns500()
    {
        _serviceMock.Setup(s => s.GetPlatformsForLocationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Внутренняя ошибка сервера"));

        var act = async () => await _controller.GetPlatforms("/ru");

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Внутренняя ошибка сервера");
    }
}