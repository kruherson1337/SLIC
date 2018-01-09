using ColorMine.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SLIC
{
    public class MyImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int NumCh { get; set; }
        public string ImageFileName { get; set; }

        public List<MyBitplane> Bitplane = new List<MyBitplane>();

        public MyImage(Bitmap bmp)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                  ImageLockMode.ReadOnly, bmp.PixelFormat);

            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed: NumCh = 1; break;
                case PixelFormat.Format16bppGrayScale: NumCh = 2; break;
                case PixelFormat.Format24bppRgb: NumCh = 3; break;
                case PixelFormat.Format32bppArgb: NumCh = 4; break;
                default: NumCh = 1; break;
            }

            byte[] pixels = new byte[bmp.Width * bmp.Height * NumCh];
            Marshal.Copy(bd.Scan0, pixels, 0, pixels.Length);
            bmp.UnlockBits(bd);

            Width = bmp.Width;
            Height = bmp.Height;

            for (int i = 0; i < NumCh; i++)
                Bitplane.Add(new MyBitplane(Width, Height));

            int pos = 0;
            for (int j = 0; j < Height; ++j)
                for (int i = 0; i < Width; ++i)
                    for (int ch = 0; ch < NumCh; ++ch)
                        Bitplane[ch].SetPixel(i, j, pixels[pos++]);


            bmp.Dispose();
        }

        internal void RGBtoXYZ()
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                {
                    double R = Bitplane[2].GetPixel(x, y);
                    double G = Bitplane[1].GetPixel(x, y);
                    double B = Bitplane[0].GetPixel(x, y);

                    Rgb myRgb = new Rgb(R, G, B);
                    Xyz myXYZ = myRgb.To<Xyz>();

                    Bitplane[2].SetPixel(x, y, myXYZ.X);
                    Bitplane[1].SetPixel(x, y, myXYZ.Y);
                    Bitplane[0].SetPixel(x, y, myXYZ.Z);
                }
        }

        internal void XYZtoRGB()
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                {
                    double X = Bitplane[2].GetPixel(x, y);
                    double Y = Bitplane[1].GetPixel(x, y);
                    double Z = Bitplane[0].GetPixel(x, y);

                    Xyz myXYZ = new Xyz(X, Y, Z);
                    Rgb myRGB = myXYZ.To<Rgb>();

                    Bitplane[2].SetPixel(x, y, myRGB.R);
                    Bitplane[1].SetPixel(x, y, myRGB.G);
                    Bitplane[0].SetPixel(x, y, myRGB.B);
                }
        }

        internal void LABtoRGB()
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                {
                    double L = Bitplane[2].GetPixel(x, y);
                    double A = Bitplane[1].GetPixel(x, y);
                    double B = Bitplane[0].GetPixel(x, y);

                    Lab myLAB = new Lab(L, A, B);
                    Rgb myRGB = myLAB.To<Rgb>();

                    Bitplane[2].SetPixel(x, y, myRGB.R);
                    Bitplane[1].SetPixel(x, y, myRGB.G);
                    Bitplane[0].SetPixel(x, y, myRGB.B);
                }
        }

        internal void RGBtoLAB()
        {
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                {
                    double R = Bitplane[2].GetPixel(x, y);
                    double G = Bitplane[1].GetPixel(x, y);
                    double B = Bitplane[0].GetPixel(x, y);

                    Rgb myRGB = new Rgb(R, G, B);
                    Lab myLAB = myRGB.To<Lab>();

                    Bitplane[2].SetPixel(x, y, myLAB.L);
                    Bitplane[1].SetPixel(x, y, myLAB.A);
                    Bitplane[0].SetPixel(x, y, myLAB.B);
                }
        }

        public MyImage(int w, int h, int ch)
        {
            NumCh = ch;
            Width = w;
            Height = h;
            ImageFileName = "";

            for (int i = 0; i < NumCh; i++)
                Bitplane.Add(new MyBitplane(Width, Height));
        }

        public Bitmap GetBitmap()
        {
            Bitmap bmp;
            switch (NumCh)
            {
                case 1: bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed); break;
                case 2: bmp = new Bitmap(Width, Height, PixelFormat.Format16bppGrayScale); break;
                case 3: bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb); break;
                case 4: bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb); break;
                default: bmp = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed); break;
            }
            byte[] pixels = new byte[Width * Height * NumCh];

            int pos = 0;
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    for (int ch = 0; ch < NumCh; ++ch)
                        pixels[pos++] = (byte)Bitplane[ch].GetPixel(x, y);


            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, bmp.PixelFormat);

            Marshal.Copy(pixels, 0, bd.Scan0, pixels.Length);

            bmp.UnlockBits(bd);

            return bmp;
        }
    }
}
