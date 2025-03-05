namespace AdvertisingPlatforms.Domain.Interfaces;

public interface IAdvertisingPlatformRepository
{
    Task LoadFromFileAsync(IAsyncEnumerable<string> lines);
    Task<List<string>> GetPlatformsForLocationAsync(string location);
}