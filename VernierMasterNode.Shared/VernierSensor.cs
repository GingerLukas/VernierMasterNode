using System;
using System.Collections.Generic;
using System.Text;

namespace VernierMasterNode.Shared
{
    public class VernierSensor
    {
        public bool IsInts { get; set; }
        public sbyte Number { get; set; }
        public byte SpareByte { get; set; }
        public UInt32 Id { get; set; }
        public byte NumberMeasType { get; set; }
        public byte SamplingMode { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public double MeasurementUncertainty { get; set; }
        public double MinMeasurement { get; set; }
        public double MaxMeasurement { get; set; }
        public UInt32 MinMeasurementPeriod { get; set; }
        public UInt64 MaxMeasurementPeriod { get; set; }
        public UInt32 TypicalMeasurementPeriod { get; set; }
        public UInt32 MeasurementPeriodGranularity { get; set; }
        public UInt32 MutualExclusionMask { get; set; }


        public UInt64 DeviceId { get; set; }

        public string DeviceIdToText()
        {
            return Encoding.ASCII.GetString(BitConverter.GetBytes(DeviceId));
        }

        /*
        public List<decimal> Values { get; set; } = new List<decimal>();

        public void AddValue(int value)
        {
            Values.Add(Convert.ToDecimal(value));
        }

        public void AddValue(float value)
        {
            Values.Add(Convert.ToDecimal(value));
        }
        */
    }
}