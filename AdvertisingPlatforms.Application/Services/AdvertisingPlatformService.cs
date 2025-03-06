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
        var rawData = new Dictionary<string, HashSet<string>>();

        await foreach (var line in lines)
        {
            var parts = line.Split(':', 2);

            if (parts.Length != 2)
            {
                continue;
            }

            var platform = parts[0].Trim();
            var locations = parts[1].Split(',').Select(loc => loc.Trim()).ToList();

            foreach (var location in locations)
            {
                if (!rawData.ContainsKey(location))
                {
                    rawData[location] = new HashSet<string>();
                }

                rawData[location].Add(platform);
            }
        }

        var processedData = new Dictionary<string, HashSet<string>>();

        foreach (var location in rawData.Keys)
        {
            var allPlatforms = new HashSet<string>();

            string currentLocation = location;
            while (!string.IsNullOrEmpty(currentLocation))
            {
                if (rawData.TryGetValue(currentLocation, out var platforms))
                {
                    allPlatforms.UnionWith(platforms);
                }

                var lastSlashIndex = currentLocation.LastIndexOf('/');
                currentLocation = lastSlashIndex > 0 ? currentLocation[..lastSlashIndex] : string.Empty;
            }

            processedData[location] = allPlatforms;
        }

        await repository.SaveDataAsync(processedData);
    }

    /// <summary>
    /// Возвращает список рекламных площадок для указанной локации.
    /// </summary>
    /// <param name="location">Локация в формате "/ru".</param>
    /// <returns>Список рекламных площадок.</returns>
    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        string decodedLocation = HttpUtility.UrlDecode(location).Trim();
        return repository.GetPlatformsForLocationAsync(decodedLocation);
    }
    
    /// <summary>
    /// Возвращает, загружены ли данные о рекламных площадках
    /// </summary>
    public bool HasData() => repository.HasData();
}