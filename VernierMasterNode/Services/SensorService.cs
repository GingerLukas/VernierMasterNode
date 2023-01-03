namespace VernierMasterNode.Services;

public class SensorService : VernierTcpService
{
    private readonly DeviceService _deviceService;

    public SensorService(DeviceService deviceService) : base(2224)
    {
        _deviceService = deviceService;
    }

    protected override void PacketReceived(string uid, byte[] data)
    {
        _deviceService.ForwardValueUpdate(uid, data);
    }
}