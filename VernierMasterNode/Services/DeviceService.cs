using Microsoft.AspNetCore.SignalR;
using VernierMasterNode.Hubs;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

public class DeviceService
{
    private readonly IHubContext<RealtimeHub, IRealtimeClient> _hubContext;
    private readonly ReaderWriterLock _devicesLock = new ReaderWriterLock();
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
        _devicesLock.AcquireReaderLock(-1);
        List<EspDevice> devices = _espDevices.Values.ToList();
        _devicesLock.ReleaseReaderLock();
        return devices;
    }

    private object _infoLock = new object();

    private void EventServiceOnSensorInfoObtained(string uid, ulong serialId, VernierSensor sensor)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        lock (_infoLock)
        {
            if (!device.ConnectedDevices.ContainsKey(serialId))
            {
                device.ConnectedDevices[serialId] = new VernierDevice(serialId);
            }

            device.ConnectedDevices[serialId].Sensors[sensor.Id] = sensor;
        }
    }

    private void EventServiceOnDeviceConnectionSuccess(string uid, ulong serialId, VernierDevice vernierDevice)
    {
        EspDevice? device = GetDevice(uid);
        if (device == null)
        {
            return;
        }

        lock (_infoLock)
        {
            if (!device.ConnectedDevices.ContainsKey(serialId))
            {
                device.ConnectedDevices[serialId] = vernierDevice;
            }
        }
    }

    private void EventServiceOnDeviceFound(string uid, ulong serialId)
    {
        EspDevice? espDevice = GetDevice(uid);
        if (espDevice == null)
        {
            return;
        }

        lock (_infoLock)
        {
            espDevice.SeenDevices.Add(serialId);
        }
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
        _devicesLock.AcquireReaderLock(-1);
        EspDevice[] devices = _espDevices.Values.ToArray();
        _devicesLock.ReleaseReaderLock();
        Parallel.ForEach(devices, (device) => { device.ParsePending(); });
    }


    private void HeartBeatCallback(object? state)
    {
        _devicesLock.AcquireReaderLock(-1);
        EspDevice[] devices = _espDevices.Values.ToArray();
        _devicesLock.ReleaseReaderLock();
        Parallel.ForEach(devices, device =>
        {
            if (device.CheckAlive()) return;
            
            _devicesLock.AcquireWriterLock(-1);
            _espDevices.Remove(device.Name);
            _devicesLock.ReleaseWriterLock();
            DeviceRemoved?.Invoke(device);
        });
    }

    public void HeartBeat(string address)
    {
        if (address.Length != 12)
        {
            return;
        }

        if (!_espDevices.TryGetValue(address, out EspDevice? device))
        {
            device = new EspDevice(address);
            _devicesLock.AcquireWriterLock(-1);
            _espDevices.Add(address, device);
            _devicesLock.ReleaseWriterLock();
            DeviceAdded?.Invoke(device);
        }

        device.HeartBeat();
    }

    public void ForwardValueUpdate(string uid, byte[] packet)
    {
        EspDevice? device = GetDevice(uid);
        device?.EnqueueResponse(packet);
    }

    public EspDevice? GetDevice(string espAddress)
    {
        _devicesLock.AcquireReaderLock(-1);
        _espDevices.TryGetValue(espAddress, out EspDevice? device);
        _devicesLock.ReleaseReaderLock();

        return device;
    }
}