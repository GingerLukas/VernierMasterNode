using System.Diagnostics;
using System.Net.Sockets;

namespace VernierMasterNode.Services;

public class CommandService : VernierTcpService
{
    public CommandService() : base(4444)
    {
    }

    public void StartSensor(string uid, UInt64 serialId, UInt32 sensorId)
    {
        byte[] packet = new byte[1 + 1 + 8 + 4];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.StartSensor;

        int i = 2;
        foreach (byte b in BitConverter.GetBytes(serialId))
        {
            packet[i++] = b;
        }

        foreach (byte b in BitConverter.GetBytes(sensorId))
        {
            packet[i++] = b;
        }

        SendPacket(uid, packet);
    }

    public void StopSensor(string uid, UInt64 serialId)
    {
        byte[] packet = new byte[1 + 1 + 8];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.StopSensor;

        int i = 2;
        foreach (byte b in BitConverter.GetBytes(serialId))
        {
            packet[i++] = b;
        }

        SendPacket(uid, packet);
    }

    public void ConnectToDevice(string uid, UInt64 serialId)
    {
        byte[] packet = new byte[1 + 1 + 8];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.ConnectToDevice;

        int i = 2;
        foreach (byte b in BitConverter.GetBytes(serialId))
        {
            packet[i++] = b;
        }

        SendPacket(uid, packet);
    }

    public void DisconnectFromDevice(string uid, UInt64 serialId)
    {
        byte[] packet = new byte[1 + 1 + 8];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.DisconnectFromDevice;

        int i = 2;
        foreach (byte b in BitConverter.GetBytes(serialId))
        {
            packet[i++] = b;
        }

        SendPacket(uid, packet);
    }

    public void StartScan(string uid)
    {
        byte[] packet = new byte[1 + 1];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.StartScan;


        SendPacket(uid, packet);
    }

    public void StopScan(string uid)
    {
        byte[] packet = new byte[1 + 1];
        packet[0] = (byte)packet.Length;
        packet[1] = (byte)ECommand.StopScan;


        SendPacket(uid, packet);
    }

    private bool SendPacket(string uid, byte[] data)
    {
        Socket? socket = GetSocket(uid);
        if (socket == null)
        {
            return false;
        }

        lock (socket)
        {
            return socket.Send(data) == data.Length;
        }
    }

    protected override void PacketReceived(string uid, byte[] data)
    {
        Debugger.Break();
    }
}

public enum ECommand : byte
{
    Unknown = 0x0,
    StartScan = 0x01,
    StopScan = 0x02,
    ConnectToDevice = 0x03,
    DisconnectFromDevice = 0x04,
    StartSensor = 0x05,
    StopSensor = 0x06,
};