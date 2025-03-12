using AdvertisingPlatforms.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AdvertisingPlatforms.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для хранения рекламных площадок в оперативной памяти.
/// </summary>
/// <param name="logger">Логгер.</param>
public class AdvertisingPlatformStorage(ILogger<AdvertisingPlatformStorage> logger) : IAdvertisingPlatformRepository
{
    /// <summary>
    /// Потокобезопасное хранилище рекламных площадок, где ключ — локация, а значение — набор площадок.
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<string>> _locationToPlatforms = new();

    /// <summary>
    /// Сохраняет переданные данные в память, заменяя предыдущие записи.
    /// </summary>
    public Task SaveDataAsync(Dictionary<string, HashSet<string>> data)
    {
        logger.LogInformation("Начато сохранение данных о рекламных площадках. Локаций: {Count}", data.Count);

        _locationToPlatforms.Clear();

        foreach (var entry in data)
        {
            _locationToPlatforms[entry.Key] = entry.Value;
        }

        logger.LogInformation("Сохранение данных завершено. Доступно {LocationCount} локаций.",
            _locationToPlatforms.Count);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Возвращает список рекламных площадок для указанной локации.
    /// </summary>
    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        if (_locationToPlatforms.TryGetValue(location, out var platforms))
        {
            logger.LogInformation("Запрос рекламных площадок для локации {Location}: найдено {Count} площадок.",
                location, platforms.Count);
            return Task.FromResult(platforms.ToList());
        }

        logger.LogWarning("Запрос рекламных площадок для локации {Location}: данных нет.", location);
        return Task.FromResult(new List<string>());
    }

    /// <summary>
    /// Проверяет, загружены ли данные о рекламных площадках.
    /// </summary>
    public bool HasData()
    {
        bool hasData = !_locationToPlatforms.IsEmpty;
        logger.LogInformation("Проверка наличия данных о рекламных площадках: {Result}",
            hasData ? "данные загружены" : "данные отсутствуют");
        return hasData;
    }

    /// <summary>
    /// Проверяет, существует ли указанная локация в системе.
    /// </summary>
    public bool LocationExists(string location)
    {
        bool exists = _locationToPlatforms.ContainsKey(location);
        logger.LogInformation("Проверка существования локации {Location}: {Result}", location,
            exists ? "найдена" : "не найдена");
        return exists;
    }
}