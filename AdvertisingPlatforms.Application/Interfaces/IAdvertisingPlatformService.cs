namespace AdvertisingPlatforms.Application.Interfaces;

/// <summary>
/// Сервис для работы с рекламными площадками.
/// </summary>
public interface IAdvertisingPlatformService
{
    /// <summary>
    /// Загружает рекламные площадки из файла и обрабатывает данные.
    /// </summary>
    /// <param name="lines">Асинхронная последовательность строк из файла.</param>
    Task LoadFromFileAsync(IAsyncEnumerable<string> lines);

    /// <summary>
    /// Получает список рекламных площадок для указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru/svrd/ekb".</param>
    /// <returns>Список рекламных площадок, если они найдены.</returns>
    Task<List<string>> GetPlatformsForLocationAsync(string location);
}