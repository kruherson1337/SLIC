using System.Drawing;

namespace SLIC
{
    class Parameters
    {
        public string filename { get; set; }
        public int numberOfCenters { get; set; }
        public double m { get; set; }
        public Color edgeColor { get; set; }
        
        public Parameters(string filename, int numberOfCenters, double m, Color edgeColor)
        {
            this.filename = filename;
            this.numberOfCenters = numberOfCenters;
            this.m = m;
            this.edgeColor = edgeColor;
        }
    }
}