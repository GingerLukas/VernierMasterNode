using System;
using System.Collections.Generic;
using System.IO;

namespace VernierMasterNode.Shared
{
    public class EspDevice
    {
        #region Static

        public static double Timeout = 10000;

        #endregion

        #region Event

        public delegate void SensorValuesUpdatedHandler(string uid, UInt64 serialId, UInt32 sensorId,
            SensorValuesPacket valuesPacket);

        public static event SensorValuesUpdatedHandler SensorValuesUpdated;

        #endregion

        public string Name { get; private set; }
        public Dictionary<UInt64, VernierDevice> ConnectedDevices { get; }
        public List<UInt64> SeenDevices { get; }
        public bool ScanEnabled { get; set; }
        
        
        
        private DateTime _lastHeartBeat;
        private readonly object _heartBeatLock = new object();
        
        private Queue<byte[]> _dataQueue = new Queue<byte[]>();


        public EspDevice(string name)
        {
            Name = name;
            ConnectedDevices = new Dictionary<UInt64, VernierDevice>();
            SeenDevices = new List<UInt64>();
        }

        public void HeartBeat()
        {
            lock (_heartBeatLock)
            {
                _lastHeartBeat = DateTime.Now;
            }
        }

        public bool CheckAlive()
        {
            lock (_heartBeatLock)
            {
                return ((DateTime.Now - _lastHeartBeat).TotalMilliseconds < Timeout);
            }
        }


        public void EnqueueResponse(byte[] data)
        {
            lock (_dataQueue)
            {
                _dataQueue.Enqueue(data);
            }
        }

        public void ParsePending()
        {
            while (true)
            {
                byte[] data;
                lock (_dataQueue)
                {
                    if (_dataQueue.Count == 0)
                    {
                        return;
                    }

                    data = _dataQueue.Dequeue();
                }

                ParseResponse(data);
            }
        }

        private void ParseResponse(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                stream.Position = 0;
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte deviceCount = reader.ReadByte();
                    for (int i = 0; i < deviceCount; i++)
                    {
                        UInt64 serialId = reader.ReadUInt64();

                        byte sensorCount = reader.ReadByte();
                        for (int j = 0; j < sensorCount; j++)
                        {
                            UInt32 id = reader.ReadUInt32();

                            bool isInts = reader.ReadByte() != 0;
                            byte valueCount = reader.ReadByte();
                            SensorValuesPacket packet = new SensorValuesPacket(valueCount, isInts);
                            for (int k = 0; k < valueCount; k++)
                            {
                                if (isInts)
                                {
                                    packet.AddValue(reader.ReadInt32());
                                }
                                else
                                {
                                    packet.AddValue(reader.ReadSingle());
                                }
                            }

                            if (valueCount > 0)
                            {
                                //sensor.Values.AddRange(packet.Values);
                                SensorValuesUpdated?.Invoke(this.Name, serialId, id, packet);
                            }
                        }
                    }
                }
            }
        }
    }
}