using System;
using System.Collections.Generic;

namespace VernierMasterNode.Shared
{
    public class VernierDevice
    {
        public string Name { get; set; }
        public UInt64 Id { get; set; }
        public Dictionary<UInt32, VernierSensor> Sensors { get; }

        public VernierDevice(ulong serialId)
        {
            Sensors = new Dictionary<uint, VernierSensor>();
            Id = serialId;
        }
    }
}