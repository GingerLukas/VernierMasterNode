using Microsoft.AspNetCore.SignalR;
using VernierMasterNode.Hubs;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

public class DeviceService
{
    
    private readonly IHubContext<RealtimeHub, IRealtimeClient> _hubContext;
    private readonly Dictionary<string, EspDevice> _espDevices = new Dictionary<string, EspDevice>();
    private readonly Timer _heartBeatTimer;
    private readonly Timer _parserTimer;

    public delegate void DeviceEventHandler(EspDevice device);

    public event DeviceEventHandler DeviceAdded;
    public event DeviceEventHandler DeviceRemoved;

    private readonly EventService _eventService;

    public DeviceService(EventService eventService, IHubContext<RealtimeHub, IRealtimeClient> hubContext)
    {
        _eventService = eventService;
        _hubContext = hubContext;

        _eventService.ScanStarted += EventServiceOnScanStarted;
        _eventService.ScanStopped += EventServiceOnScanStopped;
        _eventService.DeviceFound += EventServiceOnDeviceFound;
        _eventService.DeviceConnectionSuccess += EventServiceOnDeviceConnectionSuccess;
        _eventService.SensorInfoObtained += EventServiceOnSensorInfoObtained;
        
        DeviceAdded += OnDeviceAdded;
        DeviceRemoved += OnDeviceRemoved;

        _heartBeatTimer = new Timer(HeartBeatCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        _parserTimer = new Timer(ParserCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    private void OnDeviceRemoved(EspDevice device)
    {
        VernierTcpService.DisconnectDevice(device.Name);
        _hubContext.Clients.All.EspDeviceDisconnected(device.Name);
    }

    private void OnDeviceAdded(EspDevice device)
    {
        _hubContext.Clients.All.EspDeviceConnected(device.Name);
    }

    public List<EspDevice> GetDevices()
    {
        return _espDevices.Values.ToList();
    }

    private void EventServiceOnSensorInfoObtained(string uid, ulong serialId, VernierSensor sensor)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        device.ConnectedDevices[serialId].Sensors[sensor.Id] = sensor;
    }

    private void EventServiceOnDeviceConnectionSuccess(string uid, ulong serialId, VernierDevice vernierDevice)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        device.ConnectedDevices[serialId] = vernierDevice;
    }

    private void EventServiceOnDeviceFound(string uid, ulong serialId)
    {
        EspDevice? espDevice = GetDevice(uid);
        if (espDevice == null)
        {
            return;
        }

        espDevice.SeenDevices.Add(serialId);
    }

    private void EventServiceOnScanStopped(string uid)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        device.ScanEnabled = false;
    }

    private void EventServiceOnScanStarted(string uid)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        device.ScanEnabled = true;
    }

    private void ParserCallback(object? state)
    {
        lock (_espDevices)
        {
            foreach (var (uid, device) in _espDevices)
            {
                device.ParsePending();
            }
        }
    }


    private void HeartBeatCallback(object? state)
    {
        lock (_espDevices)
        {
            foreach (EspDevice espDevice in _espDevices.Values)
            {
                if (!espDevice.CheckAlive())
                {
                    _espDevices.Remove(espDevice.Name);
                    DeviceRemoved?.Invoke(espDevice);
                }
            }
        }
    }

    public void HeartBeat(string address)
    {
        if (address.Length != 12)
        {
            return;
        }

        lock (_espDevices)
        {
            if (!_espDevices.TryGetValue(address, out EspDevice device))
            {
                device = new EspDevice(address);
                _espDevices.Add(address, device);
                DeviceAdded?.Invoke(device);
            }

            device.HeartBeat();
        }
    }

    public void ForwardValueUpdate(string uid, byte[] packet)
    {
        lock (_espDevices)
        {
            if (_espDevices.TryGetValue(uid, out EspDevice? device))
            {
                device.EnqueueResponse(packet);
            }
        }
    }

    public EspDevice? GetDevice(string espAddress)
    {
        EspDevice? device;
        lock (_espDevices)
        {
            _espDevices.TryGetValue(espAddress, out device);
        }

        return device;
    }
}