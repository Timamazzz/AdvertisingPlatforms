namespace AdvertisingPlatforms.Domain.Interfaces;

/// <summary>
/// Интерфейс репозитория для хранения и получения данных о рекламных площадках.
/// </summary>
public interface IAdvertisingPlatformRepository
{
    /// <summary>
    /// Сохраняет данные о рекламных площадках в память.
    /// </summary>
    /// <param name="data">Словарь, где ключ — регион, значение — набор рекламных площадок.</param>
    Task SaveDataAsync(Dictionary<string, HashSet<string>> data);

    /// <summary>
    /// Возвращает список рекламных площадок для указанного региона.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>Список названий рекламных площадок или пустой список, если ничего не найдено.</returns>
    Task<List<string>> GetPlatformsForLocationAsync(string location);
    
    /// <summary>
    /// Возвращает, загружены ли данные о рекламных площадках
    /// </summary>
    bool HasData();
}