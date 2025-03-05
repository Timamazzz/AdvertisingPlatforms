using AdvertisingPlatforms.Application.Interfaces;
using AdvertisingPlatforms.Domain.Interfaces;

namespace AdvertisingPlatforms.Application.Services;

public class AdvertisingPlatformService(IAdvertisingPlatformRepository repository) : IAdvertisingPlatformService
{
    public async Task LoadFromFileAsync(IAsyncEnumerable<string> lines)
    {
        await repository.LoadFromFileAsync(lines);
    }

    public Task<List<string>> GetPlatformsForLocationAsync(string location)
    {
        return repository.GetPlatformsForLocationAsync(location);
    }
}