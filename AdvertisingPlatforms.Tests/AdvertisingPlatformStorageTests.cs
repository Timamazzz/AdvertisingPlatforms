using AdvertisingPlatforms.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AdvertisingPlatforms.Tests;

/// <summary>
/// Набор тестов для класса AdvertisingPlatformStorage.
/// Проверяет корректность хранения и извлечения данных о рекламных площадках.
/// </summary>
public class AdvertisingPlatformStorageTests
{
    private readonly AdvertisingPlatformStorage _repository;
    private readonly Mock<ILogger<AdvertisingPlatformStorage>> _loggerMock;

    public AdvertisingPlatformStorageTests()
    {
        _loggerMock = new Mock<ILogger<AdvertisingPlatformStorage>>();
        _repository = new AdvertisingPlatformStorage(_loggerMock.Object);
    }

    /// <summary>
    /// Проверяет, что данные корректно сохраняются в репозитории.
    /// </summary>
    [Fact]
    public async Task SaveDataAsync_WhenCalled_ShouldStoreDataCorrectly()
    {
        var testData = new Dictionary<string, HashSet<string>>
        {
            { "/ru", new HashSet<string> { "Яндекс.Директ" } },
            { "/ru/msk", new HashSet<string> { "Газета уральских москвичей" } }
        };

        await _repository.SaveDataAsync(testData);

        _repository.HasData().Should().BeTrue();
        _repository.LocationExists("/ru").Should().BeTrue();
        _repository.LocationExists("/ru/msk").Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync возвращает корректные данные, если локация существует.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_LocationExists_ShouldReturnPlatforms()
    {
        var testData = new Dictionary<string, HashSet<string>>
        {
            { "/ru", new HashSet<string> { "Яндекс.Директ" } }
        };
        await _repository.SaveDataAsync(testData);

        var result = await _repository.GetPlatformsForLocationAsync("/ru");

        result.Should().NotBeEmpty();
        result.Should().ContainSingle().Which.Should().Be("Яндекс.Директ");
    }

    /// <summary>
    /// Проверяет, что метод GetPlatformsForLocationAsync возвращает пустой список, если локация не существует.
    /// </summary>
    [Fact]
    public async Task GetPlatformsForLocationAsync_LocationDoesNotExist_ShouldReturnEmptyList()
    {
        var testData = new Dictionary<string, HashSet<string>>
        {
            { "/ru", new HashSet<string> { "Яндекс.Директ" } }
        };
        await _repository.SaveDataAsync(testData);

        var result = await _repository.GetPlatformsForLocationAsync("/us");

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Проверяет, что метод HasData возвращает false, если данных в репозитории нет.
    /// </summary>
    [Fact]
    public void HasData_WhenNoDataSaved_ShouldReturnFalse()
    {
        _repository.HasData().Should().BeFalse();
    }

    /// <summary>
    /// Проверяет, что метод LocationExists корректно определяет наличие существующей локации.
    /// </summary>
    [Fact]
    public async Task LocationExists_LocationExists_ShouldReturnTrue()
    {
        var testData = new Dictionary<string, HashSet<string>>
        {
            { "/ru", new HashSet<string> { "Яндекс.Директ" } }
        };
        await _repository.SaveDataAsync(testData);

        _repository.LocationExists("/ru").Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что метод LocationExists возвращает false для несуществующей локации.
    /// </summary>
    [Fact]
    public void LocationExists_LocationDoesNotExist_ShouldReturnFalse()
    {
        _repository.LocationExists("/us").Should().BeFalse();
    }
}