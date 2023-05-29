using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VernierMasterNode.Shared;
using Windows.UI.Xaml.Controls;

namespace VernierMasterNode.UWP;

public class Client : IRealtimeClient
{
    private HubConnection _connection;

    public Client(string address)
    {
        _connection = new HubConnectionBuilder().WithUrl(new Uri($"http://{address}:5153/Realtime"))
            .WithAutomaticReconnect().Build();
        
        _connection.On<string>(nameof(IRealtimeClient.EspDeviceConnected), EspDeviceConnected);
        _connection.On<string>(nameof(IRealtimeClient.EspDeviceDisconnected), EspDeviceDisconnected);

        _connection.On<string>(nameof(IRealtimeClient.ScanStarted), ScanStarted);
        _connection.On<string>(nameof(IRealtimeClient.ScanStopped), ScanStopped);

        _connection.On<string, ulong>(nameof(IRealtimeClient.DeviceConnectionSuccess), DeviceConnectionSuccess);
        _connection.On<string, ulong>(nameof(IRealtimeClient.DeviceConnectionFailed), DeviceConnectionFailed);
        _connection.On<string, ulong>(nameof(IRealtimeClient.DeviceDisconnected), DeviceDisconnected);
        _connection.On<string, ulong>(nameof(IRealtimeClient.DeviceFound), DeviceFound);

        _connection.On<string, ulong, uint>(nameof(IRealtimeClient.SensorStarted), SensorStarted);
        _connection.On<string, ulong>(nameof(IRealtimeClient.SensorsStopped), SensorsStopped);
        _connection.On<string, ulong, VernierSensor>(nameof(IRealtimeClient.SensorInfo), SensorInfo);
        _connection.On<string, ulong, uint, SensorValuesPacket>(nameof(IRealtimeClient.SensorValuesUpdated),
            SensorValuesUpdated);

        _connection.StartAsync().GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<EspDevice>> GetEspDevices()
    {
        return await _connection.InvokeAsync<IEnumerable<EspDevice>>(nameof(GetEspDevices));
    }

    public async Task RegisterForEvents(string uid)
    {
        await _connection.InvokeAsync(nameof(RegisterForEvents), uid);
    }


    public async Task UnregisterFromEvents(string uid)
    {
        await _connection.InvokeAsync(nameof(UnregisterFromEvents), uid);
    }

    public async Task StartScan(string uid)
    {
        await _connection.InvokeAsync(nameof(StartScan), uid);
    }

    public async Task StopScan(string uid)
    {
        await _connection.InvokeAsync(nameof(StopScan), uid);
    }

    public async Task ConnectToDevice(string uid, UInt64 serialId)
    {
        await _connection.InvokeAsync(nameof(ConnectToDevice), uid, serialId);
    }

    public async Task DisconnectFromDevice(string uid, UInt64 serialId)
    {
        await _connection.InvokeAsync(nameof(DisconnectFromDevice), uid, serialId);
    }

    public async Task StartSensor(string uid, UInt64 serialId, UInt32 sensorId)
    {
        await _connection.InvokeAsync(nameof(StartSensor), uid, serialId, sensorId);
    }

    public async Task StopSensors(string uid, UInt64 serialId)
    {
        await _connection.InvokeAsync(nameof(StopSensors), uid, serialId);
    }


    #region Remote

    public async Task ScanStarted(string uid)
    {
        OnScanStarted?.Invoke(uid);
    }

    public delegate void OnScanStartedHandler(string uid);
    public event OnScanStartedHandler OnScanStarted;

    public async Task ScanStopped(string uid)
    {
        OnScanStopped?.Invoke(uid);
    }

    public delegate void OnScanStoppedHandler(string uid);
    public event OnScanStoppedHandler OnScanStopped;

    public async Task DeviceConnectionSuccess(string uid, ulong serialId)
    {
        OnDeviceConnectionSuccess?.Invoke(uid, serialId);
    }

    public delegate void OnDeviceConnectionSuccessHandler(string uid, ulong serialId);
    public event OnDeviceConnectionSuccessHandler OnDeviceConnectionSuccess;

    public async Task DeviceConnectionFailed(string uid, ulong serialId)
    {
        OnDeviceConnectionFailed?.Invoke(uid, serialId);
    }

    public delegate void OnDeviceConnectionFailedHandler(string uid, ulong serialId);

    public event OnDeviceConnectionFailedHandler OnDeviceConnectionFailed;

    public async Task DeviceDisconnected(string uid, ulong serialId)
    {
        OnDeviceDisconnected?.Invoke(uid, serialId);
    }

    public delegate void OnDeviceDisconnectedHandler(string uid, ulong serialId);

    public event OnDeviceDisconnectedHandler OnDeviceDisconnected;

    public async Task DeviceFound(string uid, ulong serialId)
    {
        OnDeviceFound?.Invoke(uid, serialId);
    }

    public delegate void OnDeviceFoundHandler(string uid, ulong serialId);

    public event OnDeviceFoundHandler OnDeviceFound;

    public async Task SensorStarted(string uid, ulong serialId, uint sensorId)
    {
        OnSensorStarted?.Invoke(uid, serialId, sensorId);
    }

    public delegate void OnSensorStartedHandler(string uid, ulong serialId, uint sensorId);

    public event OnSensorStartedHandler OnSensorStarted;

    public async Task SensorsStopped(string uid, ulong serialId)
    {
        OnSensorsStopped?.Invoke(uid, serialId);
    }

    public delegate void OnSensorsStoppedHandler(string uid, ulong serialId);

    public event OnSensorsStoppedHandler OnSensorsStopped;

    public async Task SensorInfo(string uid, ulong serialId, VernierSensor sensor)
    {
        OnSensorInfo?.Invoke(uid, serialId, sensor);
    }


    public delegate void OnSensorInfoHandler(string uid, ulong serialId, VernierSensor sensor);

    public event OnSensorInfoHandler OnSensorInfo;

    public async Task SensorValuesUpdated(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet)
    {
        OnSensorValuesUpdated?.Invoke(uid, serialId, sensorId, packet);
    }



    public delegate void OnSensorValuesUpdatedHandler(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet);

    public event OnSensorValuesUpdatedHandler OnSensorValuesUpdated;
    
    public async Task EspDeviceConnected(string uid)
    {
        OnEspDeviceConnected?.Invoke(uid);
    }

    public delegate void OnEspDeviceConnectedHandler(string uid);

    public event OnEspDeviceConnectedHandler OnEspDeviceConnected;

    public async Task EspDeviceDisconnected(string uid)
    {
        OnEspDeviceDisconnected?.Invoke(uid);
    }

    public delegate void OnEspDeviceDisconnectedHandler(string uid);
    public event OnEspDeviceDisconnectedHandler OnEspDeviceDisconnected;

    #endregion
}