using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VernierMasterNode.Services;

public abstract class VernierTcpService : IDisposable
{
    private Socket _mainSocket;
    private Thread _acceptThread;
    private Thread _recieveThread;
    private CancellationTokenSource _tokenSource;
    private CancellationToken _token;
    private Dictionary<string,Socket> _sockets = new Dictionary<string, Socket>();


    public VernierTcpService(int port)
    {
        _tokenSource = new CancellationTokenSource();
        _token = _tokenSource.Token;
        _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress hostIP = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[1];
        IPEndPoint ep = new IPEndPoint(hostIP, port);
        _mainSocket.Bind(ep);
        _mainSocket.Listen();

        _acceptThread = new Thread(AcceptThreadLoop);
        _recieveThread = new Thread(ReceiveThreadLoop);

        _acceptThread.Start();
        _recieveThread.Start();
    }

    protected Socket? GetSocket(string uid)
    {
        lock (_sockets)
        {
            if (_sockets.TryGetValue(uid, out Socket socket))
            {
                return socket;
            }
        }

        return null;
    }

    private void ReceiveThreadLoop()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            lock (_sockets)
            {
                foreach ((string uid, Socket socket) in _sockets)
                {
                    if (!socket.Connected)
                    {
                        continue;
                    }
                    int len = socket.Available;
                    if (len < 4)
                    {
                        continue;
                    }

                    byte[] packetLenBin = new byte[4];

                    socket.ReceiveAsync(packetLenBin).GetAwaiter().GetResult();
                    UInt32 packetLen = BitConverter.ToUInt32(packetLenBin) - 4;
                    byte[] packet = new byte[packetLen];
                    while (socket.Available < packetLen)
                    {
                        Thread.Sleep(1);
                    }

                    socket.ReceiveAsync(packet).GetAwaiter().GetResult();
                    PacketReceived(uid, packet);
                    var r = 0;
                }
            }
        }
    }

    protected abstract void PacketReceived(string uid, byte[] data);


    private void AcceptThreadLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            Socket socket = _mainSocket.Accept();
            while (socket.Available<12)
            {
                Thread.Sleep(1);
            }

            byte[] uid = new byte[12];
            socket.ReceiveAsync(uid).GetAwaiter().GetResult();
            lock (_sockets)
            {
                _sockets[Encoding.ASCII.GetString(uid)] = socket;
            }
        }
    }

    public void Dispose()
    {
        _tokenSource.Cancel();
        foreach (KeyValuePair<string,Socket> pair in _sockets)
        {
            pair.Value.Dispose();
        }
        _mainSocket.Dispose();
    }
}