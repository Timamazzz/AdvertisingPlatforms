using AdvertisingPlatforms.Domain.Interfaces;
using System.Collections.Concurrent;

namespace AdvertisingPlatforms.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для хранения рекламных площадок в оперативной памяти.
/// </summary>
public class AdvertisingPlatformStorage : IAdvertisingPlatformRepository
{
    /// <summary>
    /// Потокобезопасное хранилище рекламных площадок, где ключ — локация, а значение — набор площадок.
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<string>> _locationToPlatforms = new();

    /// <summary>
    /// Сохраняет переданные данные в память, заменяя предыдущие записи.
    /// </summary>
    /// <param name="data">Словарь, где ключ — регион, значение — набор рекламных площадок.</param>
    public Task SaveDataAsync(Dictionary<string, HashSet<string>> data)
    {
        _locationToPlatforms.Clear();

        foreach (var entry in data)
        {
            _locationToPlatforms[entry.Key] = entry.Value;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Возвращает список рекламных площадок для указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>Список рекламных площадок или пустой список, если данных нет.</returns>
    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        var result = _locationToPlatforms.TryGetValue(location, out var platforms)
            ? platforms.ToList()
            : new List<string>();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Проверяет, загружены ли данные о рекламных площадках.
    /// </summary>
    public bool HasData() => !_locationToPlatforms.IsEmpty;

    /// <summary>
    /// Проверяет, существует ли указанная локация в системе.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>True, если локация существует, иначе false.</returns>
    public bool LocationExists(string location) => _locationToPlatforms.ContainsKey(location);
}