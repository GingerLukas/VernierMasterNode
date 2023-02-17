using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace VernierMasterNode.UWP.Services;

public class HubDiscoveryService : IDisposable
{
    public delegate void HubDiscoveryHandler(string ip, DateTime time);

    public static event HubDiscoveryHandler HubFound;
    public static event HubDiscoveryHandler HubLost;

    private static Dictionary<string, DateTime> _hubs;

    private static readonly Thread _thread;
    private static bool _disposed = false;
    private static readonly UdpClient _udp;
    private static readonly Timer _aliveTimer;


    static HubDiscoveryService()
    {
        _thread = new Thread(DiscoveryLoop);
        _udp = new UdpClient();
        _hubs = new Dictionary<string, DateTime>();
        _aliveTimer = new Timer(AliveTimerTick, null, 0, 100);

        _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udp.ExclusiveAddressUse = false;

        _udp.Client.MulticastLoopback = true;
        _udp.MulticastLoopback = true;

        _udp.Client.Bind(new IPEndPoint(IPAddress.Any, 2442));
        _udp.JoinMulticastGroup(IPAddress.Parse("239.244.244.224"), IPAddress.Any);
    }

    public static void Start()
    {
        _thread.Start();
    }

    private static void AliveTimerTick(object state)
    {
        lock (_hubs)
        {
            DateTime now = DateTime.Now;
            foreach ((string ip, DateTime lastSeen) in _hubs.ToArray())
            {
                if ((now - lastSeen).TotalSeconds > 15)
                {
                    _hubs.Remove(ip);
                    HubLost?.Invoke(ip, now);
                }
            }
        }
    }


    private static void DiscoveryLoop()
    {
        while (!_disposed)
        {
            IPEndPoint sender = new IPEndPoint(0, 2442);
            byte[] buffer = _udp.Receive(ref sender);
            lock (_hubs)
            {
                string ip = sender.Address.ToString();
                var time = DateTime.Now;
                if (!_hubs.ContainsKey(ip))
                {
                    HubFound?.Invoke(ip, time);
                }

                _hubs[ip] = time;
            }
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _aliveTimer.Dispose();
    }
}