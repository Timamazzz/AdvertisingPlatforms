using System.Web;
using AdvertisingPlatforms.Application.Interfaces;
using AdvertisingPlatforms.Domain.Interfaces;

namespace AdvertisingPlatforms.Application.Services;

/// <summary>
/// Сервис для обработки данных рекламных площадок.
/// </summary>
/// <param name="repository">Репозиторий для хранения данных.</param>
public class AdvertisingPlatformService(IAdvertisingPlatformRepository repository) : IAdvertisingPlatformService
{
    /// <summary>
    /// Загружает рекламные площадки из файла, обрабатывает данные и сохраняет в хранилище.
    /// </summary>
    /// <param name="lines">Асинхронная последовательность строк из файла.</param>
    public async Task LoadFromFileAsync(IAsyncEnumerable<string> lines)
    {
        var tempData = new Dictionary<string, HashSet<string>>();
        var failedLines = new List<string>();
        var validLines = 0;

        await foreach (var line in lines)
        {
            var parts = line.Split(':', 2);
            if (parts.Length != 2)
            {
                failedLines.Add(line);
                continue;
            }

            var platform = parts[0].Trim();
            var locations = parts[1].Split(',').Select(loc => loc.Trim()).ToList();

            if (string.IsNullOrEmpty(platform) || locations.Any(string.IsNullOrEmpty))
            {
                failedLines.Add(line);
                continue;
            }

            validLines++;

            foreach (var location in locations)
            {
                if (!tempData.TryGetValue(location, out var platforms))
                {
                    platforms = new HashSet<string>();
                    tempData[location] = platforms;
                }

                platforms.Add(platform);
            }
        }

        if (tempData.Count == 0)
        {
            throw new InvalidDataException("Файл не содержит корректных данных о рекламных площадках.");
        }

        var processedData = new Dictionary<string, HashSet<string>>(tempData.Count);

        foreach (var location in tempData.Keys)
        {
            var allPlatforms = new HashSet<string>();

            string currentLocation = location;
            while (!string.IsNullOrEmpty(currentLocation))
            {
                if (tempData.TryGetValue(currentLocation, out var platforms))
                {
                    allPlatforms.UnionWith(platforms);
                }

                var lastSlashIndex = currentLocation.LastIndexOf('/');
                currentLocation = lastSlashIndex > 0 ? currentLocation[..lastSlashIndex] : string.Empty;
            }

            processedData[location] = allPlatforms;
        }
        
        await repository.SaveDataAsync(processedData);

        if (failedLines.Count > 0)
        {
            throw new InvalidDataException(
                $"Файл загружен частично. Успешно обработано {validLines} строк, ошибки в строках:\n{string.Join("\n", failedLines)}"
            );
        }
    }


    /// <summary>
    /// Возвращает список рекламных площадок для указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>Список рекламных площадок.</returns>
    public async Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        string decodedLocation = HttpUtility.UrlDecode(location).Trim();

        if (!repository.HasData())
        {
            throw new InvalidOperationException("Данные о рекламных площадках не загружены.");
        }

        if (!repository.LocationExists(decodedLocation))
        {
            throw new KeyNotFoundException("Указанная локация не существует в системе.");
        }

        var platforms = await repository.GetPlatformsForLocationAsync(decodedLocation);

        if (platforms.Count == 0)
        {
            throw new KeyNotFoundException("Для данной локации не найдено рекламных площадок.");
        }

        return platforms;
    }
}