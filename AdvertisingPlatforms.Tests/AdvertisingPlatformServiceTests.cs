using AdvertisingPlatforms.Application.Services;
using AdvertisingPlatforms.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AdvertisingPlatforms.Tests;

/// <summary>
/// Набор тестов для класса AdvertisingPlatformService.
/// Проверяет обработку данных рекламных площадок.
/// </summary>
public class AdvertisingPlatformServiceTests
{
    private readonly Mock<IAdvertisingPlatformRepository> _mockRepository;
    private readonly AdvertisingPlatformService _service;

    public AdvertisingPlatformServiceTests()
    {
        _mockRepository = new Mock<IAdvertisingPlatformRepository>();
        _service = new AdvertisingPlatformService(_mockRepository.Object);
    }

    /// <summary>
    /// Проверяет, что при загрузке данных из файла корректные строки сохраняются в репозитории.
    /// </summary>
    [Fact]
    public async Task LoadFromFileAsync_ValidData_ShouldSaveToRepository()
    {
        // Arrange
        var lines = new List<string>
        {
            "Яндекс.Директ:/ru",
            "Газета уральских москвичей:/ru/msk,/ru/permobl"
        }.ToAsyncEnumerable();

        _mockRepository.Setup(r => r.SaveDataAsync(It.IsAny<Dictionary<string, HashSet<string>>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.LoadFromFileAsync(lines);

        // Assert
        _mockRepository.Verify(r => r.SaveDataAsync(It.IsAny<Dictionary<string, HashSet<string>>>()), Times.Once);
    }

    /// <summary>
    /// Проверяет, что при загрузке файла с некорректными строками выбрасывается исключение.
    /// </summary>
    [Fact]
    public async Task LoadFromFileAsync_InvalidData_ShouldThrowInvalidDataException()
    {
        // Arrange
        var lines = new List<string>
        {
            "Некорректная строка",
            "Еще одна плохая строка"
        }.ToAsyncEnumerable();

        // Act
        Func<Task> act = async () => await _service.LoadFromFileAsync(lines);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>()
            .WithMessage("Файл не содержит корректных данных о рекламных площадках.");
    }

    /// <summary>
    /// Проверяет, что частично загруженный файл вызывает исключение с описанием ошибок.
    /// </summary>
    [Fact]
    public async Task LoadFromFileAsync_PartialValidData_ShouldThrowPartialLoadException()
    {
        // Arrange
        var lines = new List<string>
        {
            "Яндекс.Директ:/ru",
            "Некорректная строка"
        }.ToAsyncEnumerable();

        _mockRepository.Setup(r => r.SaveDataAsync(It.IsAny<Dictionary<string, HashSet<string>>>()))
            .Returns(Task.CompletedTask);

        // Act
        Func<Task> act = async () => await _service.LoadFromFileAsync(lines);

        // Assert
        await act.Should().ThrowAsync<InvalidDataException>()
            .WithMessage("*Ошибки в строках:*");
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync выбрасывает исключение, если данные не загружены.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_NoDataLoaded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockRepository.Setup(r => r.HasData()).Returns(false);

        // Act
        Func<Task> act = async () => await _service.GetPlatformsForLocationAsync("/ru");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Данные о рекламных площадках не загружены.");
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync выбрасывает исключение, если локация не найдена.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_LocationNotExists_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.HasData()).Returns(true);
        _mockRepository.Setup(r => r.LocationExists("/ru")).Returns(false);

        // Act
        Func<Task> act = async () => await _service.GetPlatformsForLocationAsync("/ru");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Указанная локация не существует в системе.");
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync выбрасывает исключение, если у локации нет площадок.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_LocationExistsButNoPlatforms_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _mockRepository.Setup(r => r.HasData()).Returns(true);
        _mockRepository.Setup(r => r.LocationExists("/ru")).Returns(true);
        _mockRepository.Setup(r => r.GetPlatformsForLocationAsync("/ru")).ReturnsAsync(new List<string>());

        // Act
        Func<Task> act = async () => await _service.GetPlatformsForLocationAsync("/ru");

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Для данной локации не найдено рекламных площадок.");
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync возвращает корректные данные, если площадки найдены.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_ValidLocation_ShouldReturnPlatforms()
    {
        // Arrange
        var expectedPlatforms = new List<string> { "Яндекс.Директ" };

        _mockRepository.Setup(r => r.HasData()).Returns(true);
        _mockRepository.Setup(r => r.LocationExists("/ru")).Returns(true);
        _mockRepository.Setup(r => r.GetPlatformsForLocationAsync("/ru")).ReturnsAsync(expectedPlatforms);

        // Act
        var result = await _service.GetPlatformsForLocationAsync("/ru");

        // Assert
        result.Should().BeEquivalentTo(expectedPlatforms);
    }
}