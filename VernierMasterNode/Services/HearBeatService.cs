namespace VernierMasterNode.Services;

public class HearBeatService : VernierTcpService
{
    private readonly DeviceService _deviceService;
    public HearBeatService(DeviceService deviceService) : base(4224)
    {
        _deviceService = deviceService;
    }

    protected override void PacketReceived(string uid, byte[] data)
    {
        _deviceService.HeartBeat(uid);
    }
}