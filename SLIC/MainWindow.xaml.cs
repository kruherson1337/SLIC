using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class MainWindow : Window, IDisposable
    {
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        private Stopwatch watch;

        private Bitmap originalImage;
        private Bitmap segmentedImage;
        private Bitmap segmentedImageWithEdge;

        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            // Fill combobox with colors
            var colors = Utils.GetStaticPropertyBag(typeof(Color));

            foreach (KeyValuePair<string, object> colorPair in colors)
                comboBoxEdgeColor.Items.Add(colorPair.Value);
            comboBoxEdgeColor.SelectedItem = Color.White;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
                backgroundWorker.Dispose();
            }
            // get rid of unmanaged resources
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Parameters parameters = (Parameters)e.Argument;
                watch = Stopwatch.StartNew();

                // Load image
                originalImage = new Bitmap(parameters.filename);

                // SLIC
                Bitmap[] segmentedBitmap = ImageProcessing.SLIC(parameters.filename, parameters.numberOfCenters, parameters.m, parameters.edgeColor);

                segmentedImage = segmentedBitmap[0];
                segmentedImageWithEdge = segmentedBitmap[1];                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            watch.Stop();
            string timeTaken = String.Format("Width: {0} Height: {1} Time taken: {2} ms", originalImage.Width, originalImage.Height, watch.ElapsedMilliseconds.ToString());
            Console.WriteLine(timeTaken);
            labelTimeTaken.Content = timeTaken;

            if (e.Cancelled)
            {
            }
            else if (e.Error != null)
            {
                Console.WriteLine(e.Error);
            }
            else
            {
                // Draw image on screen
                imageOriginal.Source = Utils.getSource(originalImage);
                imageSegmented.Source = Utils.getSource(segmentedImage);
                imageSegmentedWithEdge.Source = Utils.getSource(segmentedImageWithEdge);
            }
        }

        private void buttonSLIC_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif) | *.jpg; *.jpeg; *.jpe; *.jfif",
                Title = "Prosim izberite sliko"
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int numberOfCenter = Int32.Parse(textboxNumberOfCenters.Text);
                double m = Double.Parse(textboxM.Text);
                Color edgeColor = (Color)comboBoxEdgeColor.SelectedItem;
                backgroundWorker.RunWorkerAsync(new Parameters(dialog.FileName, numberOfCenter, m, edgeColor));
            }
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
