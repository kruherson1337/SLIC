using System;

namespace SLIC
{
    public class MyBitplane
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public double[,] PixelData { get; set; }

        public MyBitplane(MyBitplane bitplane)
        {
            this.Width = bitplane.Width;
            this.Height = bitplane.Height;

            for (int y = 0; y < this.Height; ++y)
                for (int x = 0; x < this.Width; ++x)
                    SetPixel(x, y, bitplane.GetPixel(x, y));
        }

        public MyBitplane(int w, int h)
        {
            Width = w;
            Height = h;

            PixelData = new double[Height, Width];
        }

        public void max()
        {
            for (int y = 0; y < this.Height; ++y)
                for (int x = 0; x < this.Width; ++x)
                    SetPixel(x, y, double.MaxValue);
        }

        public double GetPixel(int x, int y)
        {
            return PixelData[y, x];
        }

        public void SetPixel(int x, int y, double value)
        {
            PixelData[y, x] = value;
        }

        internal void setAllTo(double v)
        {
            for (int y = 0; y < this.Height; ++y)
                for (int x = 0; x < this.Width; ++x)
                    SetPixel(x, y, v);
        }
    }
}
