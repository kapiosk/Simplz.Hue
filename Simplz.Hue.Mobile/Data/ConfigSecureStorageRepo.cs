using Simplz.Hue.Data;

namespace Simplz.Hue.Mobile.Data
{
    internal class ConfigSecureStorageRepo : IConfigRepo
    {
        public async Task<string> GetBridgeIPAsync()
        {
            var ip = await SecureStorage.Default.GetAsync("ip");
            return ip;
        }

        public async Task<string> GetKeyAsync()
        {
            return await SecureStorage.Default.GetAsync("key");
        }

        public async Task SetBridgeIPAsync(string ip)
        {
            await SecureStorage.Default.SetAsync("ip", ip);
        }

        public async Task SetKeyAsync(string key)
        {
            await SecureStorage.Default.SetAsync("key", key);
        }
    }
}
