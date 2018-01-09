using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SLIC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string originalFilename;

        public MainWindow()
        {
            InitializeComponent();

            // Fill combobox with colors
            var colors = Utils.GetStaticPropertyBag(typeof(Color));

            foreach (KeyValuePair<string, object> colorPair in colors)
                comboBoxEdgeColor.Items.Add(colorPair.Value);
            comboBoxEdgeColor.SelectedItem = Color.White;

            // Load default image
            string defaultImagePath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\..\\..\\Resources\\Pic6.jpg";
            processImage(defaultImagePath, Algorithm.LOAD_IMAGE);
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            processImage(originalFilename, Algorithm.LOAD_IMAGE);
        }

        private void buttonSLIC_Click(object sender, RoutedEventArgs e)
        {
            processImage(originalFilename, Algorithm.SLIC);
        }

        private void imageOriginal_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (Bitmap bmp = Utils.ImageSourceToBytes(imageOriginal))
                {
                    bmp.Save(dialog.FileName, ImageFormat.Jpeg);
                }
            }
        }

        private void imageOriginal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click
            if (e.ClickCount == 2)
                loadImage();
        }

        private void processImage(String filename, Algorithm algorithm)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            labelTimeTaken.Content = "";

            // save filename 
            originalFilename = filename;

            int numberOfCenter = Int32.Parse(textboxNumberOfCenters.Text);
            double m = Double.Parse(textboxM.Text);
            Color edgeColor = (Color)comboBoxEdgeColor.SelectedItem;

            // Load image
            using (Bitmap image = new Bitmap(filename))
            {
                Console.WriteLine("Filename: " + filename);
                Console.WriteLine("Width: " + image.Width);
                labelTimeTaken.Content += "Width: " + image.Width;
                Console.WriteLine("Height: " + image.Height);
                labelTimeTaken.Content += " Height: " + image.Height;

                // Draw image on screen
                imageOriginal.Source = Utils.getSource(image);

                if (algorithm == Algorithm.SLIC)
                {
                    Bitmap[] segmentedBitmap = ImageProcessing.SLIC(filename, numberOfCenter, m, edgeColor);

                    imageSegmented.Source = Utils.getSource(segmentedBitmap[0]);
                    imageSegmentedWithEdge.Source = Utils.getSource(segmentedBitmap[1]);

                    for(int i = 0; i < segmentedBitmap.Length; i++)
                        segmentedBitmap[i].Dispose();
                }
            }

            watch.Stop();
            string timeTaken = String.Format(" {0} {1} ms", algorithm.ToString(), watch.ElapsedMilliseconds.ToString());
            Console.WriteLine(timeTaken);
            labelTimeTaken.Content += timeTaken;
        }

        /// <summary>
        /// Load image from file
        /// </summary>
        private void loadImage()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif) | *.jpg; *.jpeg; *.jpe; *.jfif",
                Title = "Prosim izberite sliko"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                processImage(dialog.FileName, Algorithm.LOAD_IMAGE);
            }
        }

        private void imageSegmented_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (Bitmap bmp = Utils.ImageSourceToBytes(imageSegmented))
                {
                    bmp.Save(dialog.FileName, ImageFormat.Jpeg);
                }
            }
        }

        private void imageSegmentedWithEdge_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (Bitmap bmp = Utils.ImageSourceToBytes(imageSegmentedWithEdge))
                {
                    bmp.Save(dialog.FileName, ImageFormat.Jpeg);
                }
            }
        }
    }
}
