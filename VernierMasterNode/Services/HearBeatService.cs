namespace VernierMasterNode.Services;

public class HearBeatService : VernierTcpService
{
    private readonly DeviceService _deviceService;
    private readonly CommandService _commandService;
    public HearBeatService(DeviceService deviceService, CommandService commandService) : base(4224)
    {
        _deviceService = deviceService;
        _commandService = commandService;
    }

    protected override void PacketReceived(string uid, byte[] data)
    {
        _deviceService.HeartBeat(uid);
    }
}