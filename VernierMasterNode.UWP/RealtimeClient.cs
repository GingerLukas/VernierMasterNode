using System.Threading.Tasks;
using VernierMasterNode.Shared;

namespace VernierMasterNode.UWP;

public class RealtimeClient : Client
{
    public override async Task OnDeviceFound(string uid, ulong serialId)
    {
        await StopScan(uid);
        await ConnectToDevice(uid, serialId);
    }

    public override async Task OnDeviceConnectionFailed(string uid, ulong serialId)
    {
        await ConnectToDevice(uid, serialId);
    }

    public override async Task OnSensorInfo(string uid, ulong serialId, VernierSensor sensor)
    {
        await StartSensor(uid, serialId, sensor.Id);
    }

    public override async Task OnEspDeviceConnected(string uid)
    {
        await RegisterForEvents(uid);
        await StartScan(uid);
    }
}