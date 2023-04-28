//TODO: Store data in Localstorage, after MAUI conversion

namespace Simplz.Hue.Data;

internal interface IConfigRepo
{
    public Task<string?> GetKeyAsync();

    public Task SetKeyAsync(string key);

    public Task<string?> GetBridgeIPAsync();

    public Task SetBridgeIPAsync(string key);
}
