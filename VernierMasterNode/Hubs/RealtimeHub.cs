using Microsoft.AspNetCore.SignalR;
using VernierMasterNode.Services;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Hubs;

public class RealtimeHub : Hub<IRealtimeClient>
{
    private readonly CommandService _commandService;
    private readonly DeviceService _deviceService;

    private static Dictionary<string, HashSet<(string uid, UInt64 device)>> _sensorsInUse =
        new Dictionary<string, HashSet<(string uid, ulong device)>>();

    public RealtimeHub(CommandService commandService, DeviceService deviceService)
    {
        _commandService = commandService;
        _deviceService = deviceService;
    }

    private void RegisterForUse(string uid, UInt64 device)
    {
        lock (_sensorsInUse)
        {
            if (!_sensorsInUse.ContainsKey(Context.ConnectionId))
            {
                _sensorsInUse[Context.ConnectionId] = new HashSet<(string uid, ulong device)>();
            }

            _sensorsInUse[Context.ConnectionId].Add((uid,device));
        }
    }

    private void DisconnectAllUsed(string connectionId)
    {
        lock (_sensorsInUse)
        {
            if (_sensorsInUse.TryGetValue(connectionId, out var devices))
            {
                foreach ((string uid, UInt64 device) in devices)
                {
                    StopSensors(uid, device);
                }
            }
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        DisconnectAllUsed(Context.ConnectionId);
        return Task.CompletedTask;
    }


    public IEnumerable<EspDevice> GetEspDevices()
    {
        return _deviceService.GetDevices().ToList();
    }

    public async void RegisterForEvents(string uid)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, uid);
    }

    public async void UnregisterFromEvents(string uid)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, uid);
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
        RegisterForUse(uid, serialId);
    }

    public void StopSensors(string uid, UInt64 serialId)
    {
        _commandService.StopSensor(uid, serialId);
    }
}