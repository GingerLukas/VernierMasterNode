namespace VernierMasterNode.UWP;

public class IndexValuePair
{
    public int Index { get; set; }
    public double Value { get; set; }

    public IndexValuePair(int index, decimal value)
    {
        Index = index;
        Value = (double)value;
    }
    public IndexValuePair(int index, double value)
    {
        Index = index;
        Value = value;
    }
}