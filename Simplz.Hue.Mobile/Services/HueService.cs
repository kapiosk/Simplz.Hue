namespace Simplz.Hue.Services;

using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models;
using HueApi.Models.Exceptions;
using HueApi.Models.Requests;
using Simplz.Hue.Data;
using System.Diagnostics;

internal sealed class HueService
{
    private LocalHueApi? _localHueApi;

    private readonly IConfigRepo _configRepo;
    public HueService(IConfigRepo configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task<bool> IsRegisteredAsync()
    {
        try
        {
            await GetHueApiAsync();
            return true;
        }
        catch (LinkButtonNotPressedException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            throw;
        }
    }

    private async Task<LocalHueApi> GetHueApiAsync()
    {
        string? bridgeIP = await _configRepo.GetBridgeIPAsync();
        bridgeIP ??= await DiscoverBridgeIPAsync();
        await _configRepo.SetBridgeIPAsync(bridgeIP);

        string? appKey = await _configRepo.GetKeyAsync();
        appKey ??= await RegisterAppAsync(bridgeIP);
        await _configRepo.SetKeyAsync(appKey);

        return new(bridgeIP, appKey);
    }

    private static async Task<string> DiscoverBridgeIPAsync()
    {
        HttpBridgeLocator locator = new();
        var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(30));

        var _bridge = bridges.FirstOrDefault();

        return _bridge is null ? throw new Exception("No bridges found.") : _bridge.IpAddress;
    }

    private static async Task<string> RegisterAppAsync(string ip)
    {
        var regResult = await LocalHueApi.RegisterAsync(ip, "simplz.hue", Environment.MachineName);
        if (string.IsNullOrEmpty(regResult?.Username))
            throw new Exception();
        return regResult.Username;
    }

    internal async Task<HueResponse<Light>> GetLightsAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        return await _localHueApi.GetLightsAsync();
    }

    internal async Task ApplyDimmerAsync(Guid roomId, double brightness)
    {
        _localHueApi ??= await GetHueApiAsync();
        var req = new UpdateLight()
        {
            Dimming = new() { Brightness = brightness }
        };
        var lightIds = await GetLightIdsAsync(roomId);
        foreach (var lightId in lightIds)
            await _localHueApi.UpdateLightAsync(lightId, req);
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

    internal async Task<List<Models.LightStatus>> GetRoomLightsStatus(Guid roomId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var room = await _localHueApi.GetRoomAsync(roomId);
        var lights = await _localHueApi.GetLightsAsync();
        var roomLights = lights.Data.Where(l => room.Data.First().Children.Any(c => c.Rid == l.Owner.Rid));
        return roomLights.Select(l => new Models.LightStatus(l.Id, l.Dimming?.Brightness ?? 0, l.On.IsOn)).ToList();
    }

    internal async Task<List<Models.Room>> GetGroupsAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        var roomResponse = await _localHueApi.GetRoomsAsync();
        return roomResponse.Data.Select(r => new Models.Room(r.Id, r.Metadata?.Name ?? string.Empty)).ToList() ?? new();
    }

    internal async Task<List<Models.Scene>> GetScenesAsync()
    {
        _localHueApi ??= await GetHueApiAsync();
        var scenes = await _localHueApi.GetScenesAsync();
        return scenes.Data.Select(s => new Models.Scene(s.Id, s.Metadata?.Name ?? string.Empty)).ToList();
    }

    internal async Task ActivateSceneAsync(Guid sceneId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var scene = await _localHueApi.GetSceneAsync(sceneId);
        var lights = await _localHueApi.GetLightsAsync();
        foreach (var action in scene.Data.First().Actions)
        {
            var req = new UpdateLight()
            {
                On = action.Action.On,
                Dimming = action.Action.Dimming,
                ColorTemperature = action.Action.ColorTemperature,
                Color = action.Action.Color,
                Dynamics = action.Action.Dynamics,
                Gradient = action.Action.Gradient,
                Effects = action.Action.Effects,
            };
            var light = lights.Data.First(l => l.Owner.Rid == action.Target.Rid);
            await _localHueApi.UpdateLightAsync(light.Id, req);
        }
    }

    internal async Task ApplySceneAsync(Guid sceneId, Guid roomId)
    {
        _localHueApi ??= await GetHueApiAsync();
        var scene = await _localHueApi.GetSceneAsync(sceneId);
        var lightIds = await GetLightIdsAsync(roomId);
        foreach (var (action, lightId) in scene.Data.First().Actions.Zip(lightIds))
        {
            var req = new UpdateLight()
            {
                On = action.Action.On,
                Dimming = action.Action.Dimming,
                ColorTemperature = action.Action.ColorTemperature,
                Color = action.Action.Color,
                Dynamics = action.Action.Dynamics,
                Gradient = action.Action.Gradient,
                Effects = action.Action.Effects,
            };
            await _localHueApi.UpdateLightAsync(lightId, req);
        }
    }
}
