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

        var rawData = new ConcurrentDictionary<string, HashSet<string>>();

        await foreach (var line in lines)
        {

            var parts = line.Split(':', 2);
            if (parts.Length != 2)
            {
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
        }
    }

    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        string decodedLocation = HttpUtility.UrlDecode(location);

        var result = _locationToPlatforms.TryGetValue(decodedLocation, out var platforms)
            ? platforms.ToList()
            : new List<string>();

        return Task.FromResult(result);
    }
}