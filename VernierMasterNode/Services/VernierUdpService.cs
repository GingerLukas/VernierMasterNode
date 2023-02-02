using System.Net;
using System.Net.Sockets;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

public class VernierUdpService
{
    private Timer _timer;

    public VernierUdpService()
    {
        _timer = new Timer(TimerCallback,null, TimeSpan.Zero, TimeSpan.FromMilliseconds(EspDevice.Timeout/10));
    }

    private void TimerCallback(object? state)
    {
        SendUdpBroadcast();
    }

    public void SendUdpBroadcast()
    {
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
        {
            byte[] sendData = "VernierMasterNode_Safranek_Report to:5153"u8.ToArray();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("239.244.244.224"),  2442);
            socket.SendTo(sendData, ep);
        }
    }
    
}