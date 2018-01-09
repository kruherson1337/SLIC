namespace SLIC
{
    public class Center
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double L { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double COUNT { get; set; }
        
        public Center(double X, double Y, double L, double A, double B, double COUNT)
        {
            this.X = X;
            this.Y = Y;
            this.L = L;
            this.A = A;
            this.B = B;
            this.COUNT = COUNT;
        }
    }
}
