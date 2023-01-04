using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VernierMasterNode.Shared;
using Windows.UI.Xaml.Controls;

namespace VernierMasterNode.UWP;

public class Client : IRealtimeClient
{
    private HubConnection _connection;

    public Client()
    {
        _connection = new HubConnectionBuilder().WithUrl(new Uri("http://127.0.0.1:5153/Realtime"))
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
        await OnScanStarted(uid);
    }

    public virtual async Task OnScanStarted(string uid)
    {
    }

    public async Task ScanStopped(string uid)
    {
        await OnScanStopped(uid);
    }

    public virtual async Task OnScanStopped(string uid)
    {
    }

    public async Task DeviceConnectionSuccess(string uid, ulong serialId)
    {
        await OnDeviceConnectionSuccess(uid, serialId);
    }

    public virtual async Task OnDeviceConnectionSuccess(string uid, ulong serialId)
    {
    }

    public async Task DeviceConnectionFailed(string uid, ulong serialId)
    {
        await OnDeviceConnectionFailed(uid, serialId);
    }

    public virtual async Task OnDeviceConnectionFailed(string uid, ulong serialId)
    {
    }

    public async Task DeviceDisconnected(string uid, ulong serialId)
    {
        await OnDeviceDisconnected(uid, serialId);
    }

    public virtual async Task OnDeviceDisconnected(string uid, ulong serialId)
    {
    }

    public async Task DeviceFound(string uid, ulong serialId)
    {
        await OnDeviceFound(uid, serialId);
    }

    public virtual async Task OnDeviceFound(string uid, ulong serialId)
    {
    }

    public async Task SensorStarted(string uid, ulong serialId, uint sensorId)
    {
        await OnSensorStarted(uid, serialId, sensorId);
    }

    public virtual async Task OnSensorStarted(string uid, ulong serialId, uint sensorId)
    {
    }

    public async Task SensorsStopped(string uid, ulong serialId)
    {
        await OnSensorsStopped(uid, serialId);
    }

    public virtual async Task OnSensorsStopped(string uid, ulong serialId)
    {
    }

    public async Task SensorInfo(string uid, ulong serialId, VernierSensor sensor)
    {
        await OnSensorInfo(uid, serialId, sensor);
    }


    public virtual async Task OnSensorInfo(string uid, ulong serialId, VernierSensor sensor)
    {
    }

    public async Task SensorValuesUpdated(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet)
    {
        await OnSensorValuesUpdated(uid, serialId, sensorId, packet);
    }

    

    public virtual async Task OnSensorValuesUpdated(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet)
    {
    }
    
    public async Task EspDeviceConnected(string uid)
    {
        await OnEspDeviceConnected(uid);
    }
    
    public virtual async Task OnEspDeviceConnected(string uid)
    {
    }

    public async Task EspDeviceDisconnected(string uid)
    {
        await OnEspDeviceDisconnected(uid);
    }
    
    public virtual async Task OnEspDeviceDisconnected(string uid)
    {
    }

    #endregion
}