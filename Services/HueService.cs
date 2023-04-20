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

    internal async Task TurnOnAsync(Guid roomId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var req = new UpdateLight().TurnOn();
        var lightIds = await GetLightIdsAsync(roomId);
        foreach (var lightId in lightIds)
            await _localHueApi.UpdateLightAsync(lightId, req);
    }

    internal async Task TurnOffAsync(Guid roomId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var req = new UpdateLight().TurnOff();
        var lightIds = await GetLightIdsAsync(roomId);
        foreach (var lightId in lightIds)
            await _localHueApi.UpdateLightAsync(lightId, req);
    }

    private async Task<IEnumerable<Guid>> GetLightIdsAsync(Guid roomId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var room = await _localHueApi.GetRoomAsync(roomId);
        var lights = await _localHueApi.GetLightsAsync();
        return lights.Data.Where(l => room.Data.First().Children.Any(c => c.Rid == l.Owner.Rid)).Select(l => l.Id);
    }

    internal async Task<List<Models.Room>> GetGroupsAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        var roomResponse = await _localHueApi.GetRoomsAsync();
        return roomResponse.Data.Select(r => new Models.Room(r.Id, r.Metadata?.Name ?? string.Empty, r.Children.Select(l => l.Rid).ToList())).ToList() ?? new();
    }

    internal async Task GetScenesAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        //var scenes = await _localHueApi.GetScenesAsync();
    }
}
