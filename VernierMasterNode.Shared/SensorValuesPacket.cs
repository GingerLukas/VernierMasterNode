using System;
using System.Collections.Generic;

namespace VernierMasterNode.Shared
{
    public class SensorValuesPacket
    {
        public bool IsInts { get; set; }
        public List<decimal> Values { get; set; } = new List<decimal>();

        public void AddValue(int value)
        {
            Values.Add(Convert.ToDecimal(value));
        }

        public void AddValue(float value)
        {
            Values.Add(Convert.ToDecimal(value));
        }
    }
}