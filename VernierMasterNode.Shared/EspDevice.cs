using System;
using System.Collections.Generic;
using System.IO;

namespace VernierMasterNode.Shared
{
    public class EspDevice
    {
        public static double Timeout = 10000;
        public string Name { get; private set; }
        private DateTime _lastHeartBeat;

        public delegate void SensorValuesUpdatedHandler(string uid, UInt64 serialId,UInt32 sensorId, SensorValuesPacket valuesPacket);

        public static event SensorValuesUpdatedHandler SensorValuesUpdated;


        private Queue<byte[]> _dataQueue = new Queue<byte[]>();

        public Dictionary<UInt64, VernierDevice> ConnectedDevices { get; }
        public List<UInt64> SeenDevices { get; }

        public bool ScanEnabled { get; set; }

        public EspDevice(string name)
        {
            Name = name;
            ConnectedDevices = new Dictionary<UInt64, VernierDevice>();
            SeenDevices = new List<UInt64>();
        }

        public void HeartBeat()
        {
            _lastHeartBeat = DateTime.Now;
        }

        public bool CheckAlive()
        {
            return ((DateTime.Now - _lastHeartBeat).TotalMilliseconds < Timeout);
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

                        //TODO: remove, sensors and devices should be handled by separate socket
                        VernierDevice device;
                        if (!ConnectedDevices.TryGetValue(serialId, out device))
                        {
                            throw new Exception("device not connected");
                        }

                        byte sensorCount = reader.ReadByte();
                        for (int j = 0; j < sensorCount; j++)
                        {
                            //TODO: remove, sensors and devices should be handled by separate socket
                            VernierSensor sensor;
                            UInt32 id = reader.ReadUInt32();
                            if (!device.Sensors.TryGetValue(id, out sensor) || sensor == null)
                            {
                                throw new Exception("sensor not loaded");
                            }

                            bool isInts = reader.ReadByte() != 0;
                            byte valueCount = reader.ReadByte();
                            SensorValuesPacket packet = new SensorValuesPacket() { IsInts = isInts };
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
                                sensor.Values.AddRange(packet.Values);
                                SensorValuesUpdated?.Invoke(this.Name, serialId, sensor.Id, packet);
                            }
                        }
                    }
                }
            }
        }
    }
}