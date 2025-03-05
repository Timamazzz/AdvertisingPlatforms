namespace AdvertisingPlatforms.Application.Interfaces;

public interface IAdvertisingPlatformService
{
    Task LoadFromFileAsync(IAsyncEnumerable<string> lines);
    Task<List<string>> GetPlatformsForLocationAsync(string location);
}