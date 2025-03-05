using AdvertisingPlatforms.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Web;

namespace AdvertisingPlatforms.Infrastructure.Repositories;

public class AdvertisingPlatformStorage : IAdvertisingPlatformRepository
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _locationToPlatforms = new();

    public async Task LoadFromFileAsync(IAsyncEnumerable<string> lines)
    {
        _locationToPlatforms.Clear();
        Console.WriteLine("[Load] Очистка хранилища");

        var rawData = new ConcurrentDictionary<string, HashSet<string>>();

        await foreach (var line in lines)
        {
            Console.WriteLine($"[Load] Обработка строки: {line}");

            var parts = line.Split(':', 2);
            if (parts.Length != 2)
            {
                Console.WriteLine($"[Load] Пропущена некорректная строка: {line}");
                continue;
            }

            var platform = parts[0].Trim();
            var locations = parts[1].Split(',').Select(loc => loc.Trim());

            foreach (var location in locations)
            {
                rawData.AddOrUpdate(
                    location,
                    _ => new HashSet<string> { platform },
                    (_, set) =>
                    {
                        set.Add(platform);
                        return set;
                    });
            }
        }

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

            _locationToPlatforms[location] = allPlatforms;
            Console.WriteLine($"[Load] Локация '{location}' содержит площадки: {string.Join(", ", allPlatforms)}");
        }
    }

    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        // Декодируем локацию из URL-кодировки
        string decodedLocation = HttpUtility.UrlDecode(location);
        Console.WriteLine($"[Read] Запрос площадок для локации: {decodedLocation}");

        // Вывод содержимого _locationToPlatforms в читаемом виде
        Console.WriteLine("[Read] Содержимое _locationToPlatforms:");
        foreach (var entry in _locationToPlatforms)
        {
            Console.WriteLine($"  Локация: {entry.Key} -> Площадки: {string.Join(", ", entry.Value)}");
        }

        var result = _locationToPlatforms.TryGetValue(decodedLocation, out var platforms)
            ? platforms.ToList()
            : new List<string>();

        Console.WriteLine($"[Read] Найдены площадки: {string.Join(", ", result)}");
        return Task.FromResult(result);
    }
}