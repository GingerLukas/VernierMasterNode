using System.Linq;

namespace VernierMasterNode.UWP.Services;

public class LinerRegression
{
    public double Slope { get; set; }
    public double Intercept { get; set; }

    public void Fit(IndexValuePair[] values)
    {
        int n = values.Length;

        double sumX = values.Sum(x => x.Index);
        double sumY = values.Sum(x => x.Value);
        double sumXY = values.Sum(x => x.Index * x.Value);
        double sumXSquare = values.Sum(x => x.Index * x.Index);
        double meanX = sumX / n;
        double meanY = sumY / n;

        Slope = (n * sumXY - sumX * sumY) / (n * sumXSquare - sumX * sumX);
        Intercept = meanY - Slope * meanX;
    }

    public double Predict(double x)
    {
        return Slope * x + Intercept;
    }

    public (double x, double y) Intersection(LinerRegression other)
    {
        double x = (other.Intercept - Intercept) / (Slope - other.Slope);
        double y = Slope * x + Intercept;
        return (x, y);
    }
}