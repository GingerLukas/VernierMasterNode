using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VernierMasterNode.Shared
{
    public class ValueUpdatedPacket
    {
        public Dictionary<string, Dictionary<UInt32, List<object>>> values =
            new Dictionary<string, Dictionary<uint, List<object>>>();

        public static ValueUpdatedPacket Parse(byte[] data)
        {
            ValueUpdatedPacket packet = new ValueUpdatedPacket();
            using (MemoryStream stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
                {
                    while (reader.BaseStream.Length != reader.BaseStream.Position)
                    {
                        string address = Encoding.ASCII.GetString(reader.ReadBytes(8));
                        packet.values[address] = new Dictionary<uint, List<object>>();
                        byte count = reader.ReadByte();
                        for (int i = 0; i < count; i++)
                        {
                            UInt32 id = reader.ReadUInt32();
                            bool isInt = reader.ReadByte() != 0;
                            byte valuesCount = reader.ReadByte();
                            List<object> values = packet.values[address][id] = new List<object>();
                            for (int j = 0; j < valuesCount; j++)
                            {
                                if (isInt)
                                {
                                    values.Add(reader.ReadInt32());
                                }
                                else
                                {
                                    values.Add(reader.ReadSingle());
                                }
                            }
                        }
                    }
                }
            }

            return packet;
        }
    }
}