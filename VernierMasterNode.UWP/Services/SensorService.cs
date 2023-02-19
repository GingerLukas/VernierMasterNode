using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VernierMasterNode.Shared;

namespace VernierMasterNode.UWP.Services;

public static class SensorService
{
    public delegate void SensorFoundHandler(VernierSensor sensor);

    public delegate void SensorValuesUpdatedHandler(string uid, ulong serialid, uint sensorid,
        SensorValuesPacket packet);

    public static event SensorFoundHandler SensorFound;
    public static event SensorValuesUpdatedHandler SensorValuesUpdated;
    private static Client _client;

    private static Dictionary<uint, List<VernierSensor>> _sensors = new Dictionary<uint, List<VernierSensor>>();

    private static HashSet<(string uid, UInt64 device, UInt32 sensor)> _toStart =
        new HashSet<(string uid, ulong device, uint sensor)>();

    public static void SetClient(Client client)
    {
        _client = client;

        _client.OnEspDeviceConnected += ClientOnOnEspDeviceConnected;
        _client.OnDeviceFound += ClientOnOnDeviceFound;
        _client.OnSensorInfo += ClientOnOnSensorInfo;
        _client.OnSensorValuesUpdated += ClientOnOnSensorValuesUpdated;
        _client.OnScanStopped += ClientOnOnScanStopped;
        _client.OnSensorStarted += ClientOnOnSensorStarted;
    }

    private static void ClientOnOnSensorStarted(string uid, ulong serialid, uint sensorid)
    {
        lock (_toStart)
        {
            _toStart.Remove((uid, serialid, sensorid));
        }
    }

    public static async void Start()
    {
        var devices = await _client.GetEspDevices();

        foreach (EspDevice espDevice in devices)
        {
            ClientOnOnEspDeviceConnected(espDevice.Name);
        }
    }

    private static void ClientOnOnSensorValuesUpdated(string uid, ulong serialid, uint sensorid,
        SensorValuesPacket packet)
    {
        SensorValuesUpdated?.Invoke(uid, serialid, sensorid, packet);
    }

    public static async Task StartSensors()
    {
        string[] array;
        lock (_toStart)
        {
            array = _toStart.GroupBy(x => x.uid).Select(x => x.Key).ToArray();
        }

        foreach (string uid in array)
        {
            await _client.StopScan(uid);
        }

        while (true)
        {
            int len = 0;
            lock (_toStart)
            {
                len = _toStart.Count;
            }


            if (len == 0)
            {
                break;
            }

            await Task.Delay(1);
        }
    }

    public static void RegisterForStart(string uid, UInt64 device, UInt32 sensor)
    {
        lock (_toStart)
        {
            _toStart.Add((uid, device, sensor));
        }
    }

    private static async void ClientOnOnScanStopped(string uid)
    {
        (string uid, ulong device, uint sensor)[] array;
        lock (_toStart)
        {
            array = _toStart.Where(x => x.uid == uid).ToArray();
        }

        foreach ((string esp, ulong device, uint sensor) in array)
        {
            await _client.StartSensor(esp, device, sensor);
        }
    }

    private static async void ClientOnOnDeviceFound(string uid, ulong serialid)
    {
        await _client.ConnectToDevice(uid, serialid);
    }

    private static async void ClientOnOnEspDeviceConnected(string uid)
    {
        await _client.RegisterForEvents(uid);
        await _client.StartScan(uid);
    }

    private static void ClientOnOnSensorInfo(string uid, ulong serialid, VernierSensor sensor)
    {
        lock (_sensors)
        {
            if (!_sensors.ContainsKey(sensor.Id))
            {
                _sensors[sensor.Id] = new List<VernierSensor>();
            }

            _sensors[sensor.Id].Add(sensor);
            SensorFound?.Invoke(sensor);
        }
    }
}