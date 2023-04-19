namespace Simplz.Hue.Services;

using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models;
using HueApi.Models.Exceptions;
using HueApi.Models.Requests;
using Simplz.Hue.Data;

internal sealed class HueService
{
    private LocalHueApi? _localHueApi;

    private readonly IConfigRepo _configRepo;
    public HueService(IConfigRepo configRepo)
    {
        _configRepo = configRepo;
    }

    private async Task<LocalHueApi> GetHueApiAsync()
    {
        string? bridgeIP = await _configRepo.GetBridgeIPAsync();
        bridgeIP ??= await DiscoverBridgeIPAsync();

        string? appKey = await _configRepo.GetKeyAsync();
        appKey ??= await RegisterAppAsync(bridgeIP);

        return new(bridgeIP, appKey);
    }

    private async Task<string> DiscoverBridgeIPAsync()
    {
        HttpBridgeLocator locator = new();
        var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));

        var _bridge = bridges.FirstOrDefault();

        if (_bridge is null)
            throw new Exception("No bridges found.");

        return _bridge.IpAddress;
    }

    private async Task<string> RegisterAppAsync(string ip)
    {
        try
        {
            var regResult = await LocalHueApi.RegisterAsync(ip, "simplz.hue", Environment.MachineName);
            if (string.IsNullOrEmpty(regResult?.Username))
                throw new Exception();
            return regResult.Username;
        }
        catch (LinkButtonNotPressedException)
        {
            return string.Empty;
        }
    }

    internal async Task<HueResponse<Light>> GetLightsAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        return await _localHueApi.GetLightsAsync();
    }

    internal async Task TurnOnAsync(string groupName)
    {
        _localHueApi ??= await GetHueApiAsync();
        var req = new UpdateLight().TurnOn();
        foreach (var lightId in await _configRepo.GetGroupLightsAsync(groupName))
            await _localHueApi.UpdateLightAsync(lightId, req);
    }

    internal async Task TurnOffAsync(string groupName)
    {
        _localHueApi ??= await GetHueApiAsync();
        var req = new UpdateLight().TurnOff();
        foreach (var lightId in await _configRepo.GetGroupLightsAsync(groupName))
            await _localHueApi.UpdateLightAsync(lightId, req);
    }

}