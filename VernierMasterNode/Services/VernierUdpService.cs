using System.Net;
using System.Net.Sockets;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

public class VernierUdpService : IDisposable
{
    private readonly Timer _timer;
    private readonly Socket _socket;
    private readonly IPEndPoint _endpoint;
    private readonly byte[] _sendData;

    public VernierUdpService()
    {
        _sendData = "VernierMasterNode_Safranek"u8.ToArray();

        _endpoint = new IPEndPoint(IPAddress.Parse("239.244.244.224"), 2442);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(EspDevice.Timeout / 10));
    }

    private void TimerCallback(object? state)
    {
        SendUdpBroadcast();     
    }

    private void SendUdpBroadcast()
    {
        _socket.SendTo(_sendData, _endpoint);
    }

    public void Dispose()
    {
        _timer.Dispose();
        _socket.Dispose();
    }
}