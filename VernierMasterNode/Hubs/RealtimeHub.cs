using Microsoft.AspNetCore.SignalR;
using VernierMasterNode.Services;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Hubs;

public class RealtimeHub : Hub<IRealtimeClient>
{
    private readonly CommandService _commandService;
    private readonly DeviceService _deviceService;

    private static readonly HashSet<string> _groups = new HashSet<string>();

    public RealtimeHub(CommandService commandService, EventService eventService, DeviceService deviceService)
    {
        _commandService = commandService;
        _deviceService = deviceService;
    }

    public async Task<List<string>> GetEspDevices()
    {
        return _deviceService.GetDevices().Select(x => x.Name).ToList();
    }

    public async void RegisterForEvents(string uid)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, uid);
        _groups.Add(uid);
    }

    public async void UnregisterFromEvents(string uid)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, uid);
        _groups.Remove(uid);
    }

    public void StartScan(string uid)
    {
        _commandService.StartScan(uid);
    }

    public void StopScan(string uid)
    {
        _commandService.StopScan(uid);
    }

    public void ConnectToDevice(string uid, UInt64 serialId)
    {
        _commandService.ConnectToDevice(uid, serialId);
    }

    public void DisconnectFromDevice(string uid, UInt64 serialId)
    {
        _commandService.DisconnectFromDevice(uid, serialId);
    }

    public void StartSensor(string uid, UInt64 serialId, UInt32 sensorId)
    {
        _commandService.StartSensor(uid, serialId, sensorId);
    }

    public void StopSensors(string uid, UInt64 serialId)
    {
        _commandService.StopSensor(uid, serialId);
    }

}