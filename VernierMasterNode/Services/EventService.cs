using System.Text;
using Microsoft.AspNetCore.SignalR;
using VernierMasterNode.Hubs;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

enum EVernierEvent : byte
{
    Unknown = 0x00,
    ScanStarted = 0x01,
    ScanStopped = 0x02,
    DeviceConnectionSuccess = 0x03,
    DeviceConnectionFailed = 0x04,
    DeviceDisconnected = 0x05,
    DeviceFound = 0x06,
    SensorStarted = 0x07,
    SensorsStopped = 0x08,
    SensorInfo = 0x09,
};

public class EventService : VernierTcpService
{
    private readonly IHubContext<RealtimeHub, IRealtimeClient> _hubContext;

    public EventService(IHubContext<RealtimeHub, IRealtimeClient> hubContext) : base(2222)
    {
        _hubContext = hubContext;
        ScanStarted += OnScanStarted;
        ScanStopped += OnScanStopped;
        DeviceFound += OnDeviceFound;
        DeviceConnectionSuccess += OnDeviceConnectionSuccess;
        DeviceConnectionFailed += OnDeviceConnectionFailed;
        DeviceDisconnected += OnDeviceDisconnected;
        SensorInfoObtained += OnSensorInfoObtained;
        SensorStarted += OnSensorStarted;
        SensorsStopped += OnSensorsStopped;
        EspDevice.SensorValuesUpdated += EspDeviceOnSensorValuesUpdated;
        
        _parserTimer = new Timer(ParserTick, null, 0, 100);
    }

    private void OnDeviceDisconnected(string uid, ulong serialid)
    {
        _hubContext.Clients.Group(uid).DeviceDisconnected(uid, serialid);
    }

    private void OnDeviceConnectionFailed(string uid, ulong serialid)
    {
        _hubContext.Clients.Group(uid).DeviceConnectionFailed(uid, serialid);
    }

    private void EspDeviceOnSensorValuesUpdated(string uid, ulong serialid, uint sensorid, SensorValuesPacket valuespacket)
    {
        _hubContext.Clients.Group(uid).SensorValuesUpdated(uid, serialid, sensorid, valuespacket);
    }

    private void OnSensorsStopped(string uid, ulong serialid)
    {
        _hubContext.Clients.Group(uid).SensorsStopped(uid, serialid);
    }

    private void OnSensorStarted(string uid, ulong serialid, uint sensorid)
    {
        _hubContext.Clients.Group(uid).SensorStarted(uid, serialid, sensorid);
        
    }

    private void OnSensorInfoObtained(string uid, ulong serialid, VernierSensor sensor)
    {
        _hubContext.Clients.Group(uid).SensorInfo(uid, serialid, sensor);
    }

    private void OnDeviceConnectionSuccess(string uid, ulong serialid, VernierDevice vernierdevice)
    {
        _hubContext.Clients.Group(uid).DeviceConnectionSuccess(uid, serialid);
    }

    private void OnDeviceFound(string uid, ulong serialid)
    {
        _hubContext.Clients.Group(uid).DeviceFound(uid, serialid);
    }

    private void OnScanStopped(string uid)
    {
        _hubContext.Clients.Group(uid).ScanStopped(uid);
    }

    private void OnScanStarted(string uid)
    {
        _hubContext.Clients.Group(uid).ScanStarted(uid);
    }

    public delegate void EspDeviceEventHandler(string uid);

    public delegate void VernierDeviceEventHandler(string uid, UInt64 serialId);

    public delegate void VernierDeviceConnectedEventHandler(string uid, UInt64 serialId, VernierDevice vernierDevice);

    public delegate void VernierSensorEventHandler(string uid, UInt64 serialId, VernierSensor sensor);
    public delegate void VernierSensorSimpleEventHandler(string uid, UInt64 serialId, UInt32 sensorId);

    public event EspDeviceEventHandler ScanStarted;
    public event EspDeviceEventHandler ScanStopped;
    public event VernierDeviceEventHandler DeviceFound;
    public event VernierDeviceConnectedEventHandler DeviceConnectionSuccess;
    public event VernierDeviceEventHandler DeviceConnectionFailed;
    public event VernierDeviceEventHandler DeviceDisconnected;
    public event VernierSensorEventHandler SensorInfoObtained;
    public event VernierSensorSimpleEventHandler SensorStarted;
    public event VernierDeviceEventHandler SensorsStopped;


    private readonly Queue<(string, byte[])> _eventDataQueue = new Queue<(string, byte[])>();
    private readonly Timer _parserTimer;

    protected override void PacketReceived(string uid, byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            stream.Position = 0;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte count = reader.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    byte length = reader.ReadByte();
                    byte[] eventData = reader.ReadBytes(length);
                    lock (_eventDataQueue)
                    {
                        _eventDataQueue.Enqueue((uid, eventData));
                    }
                }
            }
        }
    }

    private void ParserTick(object? state)
    {
        while (true)
        {
            byte[] data;
            string uid;
            lock (_eventDataQueue)
            {
                if (_eventDataQueue.Count == 0)
                {
                    return;
                }

                (uid, data) = _eventDataQueue.Dequeue();
            }

            ParseEvent(uid, data);
        }
    }

    public void ParseEvent(string uid, byte[] data)
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            stream.Position = 0;
            UInt64 serialId;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                EVernierEvent type = (EVernierEvent)reader.ReadByte();
                //TODO handle events
                switch (type)
                {
                    case EVernierEvent.Unknown:
                        break;
                    case EVernierEvent.ScanStarted:
                        ScanStarted?.Invoke(uid);
                        break;
                    case EVernierEvent.ScanStopped:
                        ScanStopped?.Invoke(uid);
                        break;
                    case EVernierEvent.DeviceConnectionSuccess:
                        serialId = reader.ReadUInt64();

                        VernierDevice vernierDevice = new VernierDevice(serialId);
                        UInt32 mask = reader.ReadUInt32();
                        
                        for (int i = 0; i < 32; i++)
                        {
                            if (((mask >> i) & 1) == 1)
                            {
                                vernierDevice.Sensors.Add(reader.ReadUInt32(), null);
                            }
                        }
                        
                        //TODO: find use for mask

                        DeviceConnectionSuccess?.Invoke(uid, serialId, vernierDevice);

                        break;
                    case EVernierEvent.DeviceConnectionFailed:
                        serialId = reader.ReadUInt64();
                        DeviceConnectionFailed?.Invoke(uid,serialId);
                        break;
                    case EVernierEvent.DeviceDisconnected:
                        serialId = reader.ReadUInt64();
                        DeviceDisconnected?.Invoke(uid,serialId);
                        break;
                    case EVernierEvent.DeviceFound:
                        serialId = reader.ReadUInt64();
                        DeviceFound?.Invoke(uid, serialId);
                        break;
                    case EVernierEvent.SensorStarted:
                        serialId = reader.ReadUInt64();
                        uint sensorId = reader.ReadUInt32();
                        SensorStarted?.Invoke(uid, serialId, sensorId);
                        break;
                    case EVernierEvent.SensorsStopped:
                        serialId = reader.ReadUInt64();
                        SensorsStopped?.Invoke(uid, serialId);
                        break;
                    case EVernierEvent.SensorInfo:
                        serialId = reader.ReadUInt64();
                        VernierSensor sensor = new VernierSensor();
                        sensor.IsInts = reader.ReadByte() != 0;
                        sensor.Number = reader.ReadSByte();
                        sensor.SpareByte = reader.ReadByte();
                        sensor.Id = reader.ReadUInt32();
                        sensor.NumberMeasType = reader.ReadByte();
                        sensor.SamplingMode = reader.ReadByte();
                        sensor.Description = Encoding.UTF8.GetString(reader.ReadBytes(60));
                        sensor.Unit = Encoding.UTF8.GetString(reader.ReadBytes(32));
                        sensor.MeasurementUncertainty = reader.ReadDouble();
                        sensor.MinMeasurement = reader.ReadDouble();
                        sensor.MaxMeasurement = reader.ReadDouble();
                        sensor.MinMeasurementPeriod = reader.ReadUInt32();
                        sensor.MaxMeasurementPeriod = reader.ReadUInt64();
                        sensor.TypicalMeasurementPeriod = reader.ReadUInt32();
                        sensor.MeasurementPeriodGranularity = reader.ReadUInt32();
                        sensor.MutualExclusionMask = reader.ReadUInt32();

                        SensorInfoObtained?.Invoke(uid, serialId, sensor);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}