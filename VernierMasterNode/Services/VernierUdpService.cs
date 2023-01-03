using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Services;

public class VernierUdpService
{
    private Timer _timer;

    public VernierUdpService()
    {
        _timer = new Timer(TimerCallback,null, TimeSpan.Zero, TimeSpan.FromMilliseconds(EspDevice.Timeout/5));
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
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("224.244.244.224"),  2442);
            //IPEndPoint ep = new IPEndPoint(GetLocalBroadcast(NetworkInterfaceType.Wireless80211), 2442);
            socket.SendTo(sendData, ep);
        }
    }
    
    public IPAddress GetLocalBroadcast(NetworkInterfaceType _type)
    {
        IPAddress output = null;
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        output = GetBroadcastAddress(ip.Address,ip.IPv4Mask);
                        
                    }
                }
            }
        }
        return output;
    }

    public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
    {
        uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
        uint broadCastIpAddress = ipAddress | ~ipMaskV4;

        return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
    }
}