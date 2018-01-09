using System;
using System.Collections.Generic;
using System.Drawing;

namespace SLIC
{
    public class ImageProcessing
    {
        public static Bitmap[] SLIC(String filename, double numberOfCenters, double m, Color edgeColor)
        {
            MyImage image;
            Bitmap[] processedImages = new Bitmap[2];

            using (Bitmap bmp = new Bitmap(filename))
            {
                image = new MyImage(bmp);

                // Convert RGB TO LAB
                image.RGBtoLAB();

                // Create centers
                double S = Math.Sqrt((image.Width * image.Height) / numberOfCenters);
                Center[] centers = createCenters(image, numberOfCenters, S);
                MyBitplane labels = new MyBitplane(image.Width, image.Height);
                labels.setAllTo(-1);

                for (int iteration = 0; iteration < 10; iteration++)
                {
                    MyBitplane lenghts = new MyBitplane(image.Width, image.Height);
                    lenghts.max();

                    int i = 0;
                    foreach (Center center in centers)
                    {
                        for (int l = (int)Math.Round(center.Y - S); l < (int)Math.Round(center.Y + S); l++)
                            for (int k = (int)Math.Round(center.X - S); k < (int)Math.Round(center.X + S); k++)
                                if (k >= 0 && k < image.Width && l >= 0 && l < image.Height)
                                {
                                    double L = image.Bitplane[2].GetPixel(k, l);
                                    double A = image.Bitplane[1].GetPixel(k, l);
                                    double B = image.Bitplane[0].GetPixel(k, l);

                                    double Dc = Math.Sqrt(Math.Pow(L - center.L, 2) + Math.Pow(A - center.A, 2) + Math.Pow(B - center.B, 2));
                                    double Ds = Math.Sqrt(Math.Pow(l - center.Y, 2) + Math.Pow(k - center.X, 2));
                                    double lenght = Math.Sqrt(Math.Pow(Dc, 2) + Math.Pow(Ds / 2, 2) * Math.Pow(m, 2));

                                    if (lenght < lenghts.GetPixel(k, l))
                                    {
                                        lenghts.SetPixel(k, l, lenght);
                                        labels.SetPixel(k, l, i);
                                    }
                                }
                        i++;
                    }
                    centers = calculateNewCenters(image, centers, labels);
                }

                image = drawAverage(image, centers, labels);
                image.LABtoRGB();
                processedImages[0] = image.GetBitmap(); // Segmented

                image = drawEdges(image, centers, labels, edgeColor);
                processedImages[1] = image.GetBitmap(); // Segmented with Edge
            }

            return processedImages;
        }

        private static MyImage drawEdges(MyImage image, Center[] centers, MyBitplane labels, Color edgeColor)
        {
            MyBitplane edges = new MyBitplane(image.Width, image.Height);
            MyImage newImage = new MyImage(image.Width, image.Height, image.NumCh);

            for (int y = 1; y < image.Height; ++y)
                for (int x = 1; x < image.Width; ++x)
                {
                    double p = labels.GetPixel(x, y);
                    double p1 = labels.GetPixel(x, y - 1);
                    double p2 = labels.GetPixel(x - 1, y - 1);
                    double p3 = labels.GetPixel(x - 1, y);

                    bool edge = false;

                    if (((p1 != p) && (edges.GetPixel(x, y - 1) != 1)) || ((p2 != p) && (edges.GetPixel(x - 1, y - 1) != 1)) || ((p3 != p) && (edges.GetPixel(x - 1, y) != 1)))
                    {
                        edge = true;
                        edges.SetPixel(x, y, 1.0);
                    }

                    if (edge)
                    {
                        newImage.Bitplane[2].SetPixel(x, y, edgeColor.R);
                        newImage.Bitplane[1].SetPixel(x, y, edgeColor.G);
                        newImage.Bitplane[0].SetPixel(x, y, edgeColor.B);
                    }
                    else
                    {
                        newImage.Bitplane[2].SetPixel(x, y, (int)Math.Round(image.Bitplane[2].GetPixel(x, y)));
                        newImage.Bitplane[1].SetPixel(x, y, (int)Math.Round(image.Bitplane[1].GetPixel(x, y)));
                        newImage.Bitplane[0].SetPixel(x, y, (int)Math.Round(image.Bitplane[0].GetPixel(x, y)));
                    }
                }

            return newImage;
        }

        private static MyImage drawAverage(MyImage image, Center[] centers, MyBitplane labels)
        {
            MyImage newImage = new MyImage(image.Width, image.Height, image.NumCh);

            for (int y = 0; y < image.Height; ++y)
                for (int x = 0; x < image.Width; ++x)
                    if (labels.GetPixel(x, y) != -1)
                    {
                        newImage.Bitplane[2].SetPixel(x, y, centers[(int)Math.Round(labels.GetPixel(x, y))].L);
                        newImage.Bitplane[1].SetPixel(x, y, centers[(int)Math.Round(labels.GetPixel(x, y))].A);
                        newImage.Bitplane[0].SetPixel(x, y, centers[(int)Math.Round(labels.GetPixel(x, y))].B);
                    }

            return newImage;
        }

        private static Center[] calculateNewCenters(MyImage image, Center[] centers, MyBitplane labels)
        {
            Center[] newCenters = new Center[centers.Length];

            for (int i = 0; i < centers.Length; i++)
                newCenters[i] = new Center(0, 0, 0, 0, 0, 0);

            for (int y = 0; y < image.Height; ++y)
                for (int x = 0; x < image.Width; ++x)
                {
                    int centerIndex = (int)Math.Round(labels.GetPixel(x, y));
                    if (centerIndex != -1)
                    {
                        double L = image.Bitplane[2].GetPixel(x, y);
                        double A = image.Bitplane[1].GetPixel(x, y);
                        double B = image.Bitplane[0].GetPixel(x, y);

                        newCenters[centerIndex].X += x;
                        newCenters[centerIndex].Y += y;
                        newCenters[centerIndex].L += L;
                        newCenters[centerIndex].A += A;
                        newCenters[centerIndex].B += B;
                        newCenters[centerIndex].COUNT += 1;
                    }
                }

            // Normalize
            foreach (Center center in newCenters)
            {
                if (center.COUNT != 0)
                {
                    center.X = Math.Round(center.X / center.COUNT);
                    center.Y = Math.Round(center.Y / center.COUNT);
                    center.L /= center.COUNT;
                    center.A /= center.COUNT;
                    center.B /= center.COUNT;
                }
            }

            return newCenters;
        }

        private static Center[] createCenters(MyImage image, double numberOfCenters, double S)
        {
            List<Center> centers = new List<Center>();
            for (double y = S; y < image.Height - S / 2; y += S)
                for (double x = S; x < image.Width - S / 2; x += S)
                {
                    double L = image.Bitplane[2].GetPixel((int)Math.Round(x), (int)Math.Round(y));
                    double A = image.Bitplane[1].GetPixel((int)Math.Round(x), (int)Math.Round(y));
                    double B = image.Bitplane[0].GetPixel((int)Math.Round(x), (int)Math.Round(y));

                    centers.Add(new Center(x, y, L, A, B, 0));
                }
            return centers.ToArray();
        }


    }
}
