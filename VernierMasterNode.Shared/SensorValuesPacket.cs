using System;
using System.Collections.Generic;

namespace VernierMasterNode.Shared
{
    public class SensorValuesPacket
    {
        public bool IsInts { get; set; }
        public decimal[] Values { get; set; }
        private int _index = 0;

        public SensorValuesPacket()
        {
            
        }

        public SensorValuesPacket(int count, bool isInts)
        {
            Values = new decimal[count];
            IsInts = isInts;
        }

        public void AddValue(int value)
        {
            Values[_index++] = (Convert.ToDecimal(value));
        }

        public void AddValue(float value)
        {
            Values[_index++] = (Convert.ToDecimal(value));
        }
    }
}