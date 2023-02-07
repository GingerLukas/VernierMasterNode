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
    private Dictionary<string, Socket> _sockets = new Dictionary<string, Socket>();

    private delegate void DeviceDisconnectedHandler(string uid);

    private static event DeviceDisconnectedHandler DeviceDisconnected;


    public VernierTcpService(int port)
    {
        DeviceDisconnected += OnDeviceDisconnected;
        _tokenSource = new CancellationTokenSource();
        _token = _tokenSource.Token;
        _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
        _mainSocket.Bind(ep);
        _mainSocket.Listen();

        _acceptThread = new Thread(AcceptThreadLoop);
        _recieveThread = new Thread(ReceiveThreadLoop);

        _acceptThread.Start();
        _recieveThread.Start();
    }

    public static void DisconnectDevice(string uid)
    {
        DeviceDisconnected?.Invoke(uid);
    }

    private void OnDeviceDisconnected(string uid)
    {
        lock (_sockets)
        {
            Socket? socket = GetSocket(uid);
            if (socket == null)
            {
                return;
            }

            _sockets.Remove(uid);
            socket.Dispose();
        }
    }

    protected Socket? GetSocket(string uid)
    {
        lock (_sockets)
        {
            _sockets.TryGetValue(uid, out Socket? socket);
            return socket;
        }
    }

    private void ReceiveThreadLoop()
    {
        ParallelOptions options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        while (!_tokenSource.IsCancellationRequested)
        {
            lock (_sockets)
            {
                List<string> toRemove = new List<string>();
                Parallel.ForEach(_sockets, options, pair =>
                {
                    string uid = pair.Key;
                    Socket socket = pair.Value;

                    if (!socket.Connected)
                    {
                        lock (toRemove)
                        {
                            toRemove.Add(uid);
                        }

                        return;
                    }

                    int len = socket.Available;
                    if (len < 4)
                    {
                        return;
                    }

                    byte[] packetLenBin = new byte[4];

                    
                    SafeReceive(socket, ref packetLenBin);

                    UInt32 packetLen = BitConverter.ToUInt32(packetLenBin) - 4;
                    byte[] packet = new byte[packetLen];
                    
                    SafeReceive(socket, ref packet);
                    
                    PacketReceived(uid, packet);
                });
                lock (toRemove)
                {
                    foreach (string uid in toRemove)
                    {
                        _sockets[uid].Dispose();
                        _sockets.Remove(uid);
                    }
                }
            }
        }
    }

    private void SafeReceive(Socket socket, ref byte[] buffer)
    {
        try
        {
            while (socket.Available < buffer.Length)
            {
                Thread.Sleep(1);
            }
            socket.ReceiveAsync(buffer).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            return;
        }
    }

    protected abstract void PacketReceived(string uid, byte[] data);


    private void AcceptThreadLoop()
    {
        while (!_token.IsCancellationRequested)
        {
            Socket socket = _mainSocket.Accept();
            while (socket.Available < 12)
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
        foreach (KeyValuePair<string, Socket> pair in _sockets)
        {
            pair.Value.Dispose();
        }

        _mainSocket.Dispose();
    }
}