using System;
using System.Threading.Tasks;

namespace VernierMasterNode.Shared
{
    public interface IRealtimeClient
    {
        Task ScanStarted(string uid);
        Task ScanStopped(string uid);
        Task DeviceConnectionSuccess(string uid, ulong serialId);
        Task DeviceConnectionFailed(string uid, ulong serialId);
        Task DeviceDisconnected(string uid, ulong serialId);
        Task DeviceFound(string uid, ulong serialId);
        Task SensorStarted(string uid, ulong serialId, uint sensorId);
        Task SensorsStopped(string uid, ulong serialId);
        Task SensorInfo(string uid, ulong serialId, VernierSensor sensor);

        Task SensorValuesUpdated(string uid, ulong serialId, uint sensorId, SensorValuesPacket packet);

        Task EspDeviceConnected(string uid);
        Task EspDeviceDisconnected(string uid);
    }
}