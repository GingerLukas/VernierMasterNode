using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using VernierMasterNode.Shared;

namespace VernierMasterNode.UWP;

public class Client : IRealtimeClient
{
    private HubConnection _connection;

    public Client()
    {
        _connection = new HubConnectionBuilder().WithUrl(new Uri("http://127.0.0.1:5153/Realtime"))
            .WithAutomaticReconnect().Build();

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

    


    public Task ScanStarted(string uid)
    {
        throw new System.NotImplementedException();
    }

    public Task ScanStopped(string uid)
    {
        throw new System.NotImplementedException();
    }

    public Task DeviceConnectionSuccess(string uid, ulong serialId)
    {
        throw new System.NotImplementedException();
    }

    public Task DeviceConnectionFailed(string uid, ulong serialId)
    {
        throw new System.NotImplementedException();
    }

    public Task DeviceDisconnected(string uid, ulong serialId)
    {
        throw new System.NotImplementedException();
    }

    public Task DeviceFound(string uid, ulong serialId)
    {
        throw new System.NotImplementedException();
    }

    public Task SensorStarted(string uid, ulong serialId, uint sensorId)
    {
        throw new System.NotImplementedException();
    }

    public Task SensorsStopped(string uid, ulong serialId)
    {
        throw new System.NotImplementedException();
    }

    public Task SensorInfo(string uid, ulong serialId, VernierSensor sensor)
    {
        throw new System.NotImplementedException();
    }

    public Task SensorValuesUpdated(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
