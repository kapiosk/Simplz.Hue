namespace Simplz.Hue.Core.Data;

public interface IConfigRepo
{
    public Task<string?> GetKeyAsync();

    public Task SetKeyAsync(string key);

    public Task<string?> GetBridgeIPAsync();

    public Task SetBridgeIPAsync(string key);
}
