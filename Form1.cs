//using WebCamLib;

using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap imageB, imageA;
        int nWeightMultiplier = -1;
        //Device cam;

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            defaultForm();

            //Device[] devices = DeviceManager.GetAllDevices();
            //if (devices.Length > 0)
            //    cam = devices[0]; 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // load background

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imageA = new Bitmap(openFileDialog.FileName);
                pictureBox2.Image = imageA;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // load image

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                imageB = new Bitmap(openFileDialog.FileName);
                pictureBox1.Image = imageB;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // process image

            if (imageB == null)
            {
                MessageBox.Show("No image loaded");
                return;
            }

            switch (determineAction())
            {
                case "Copy":
                    pictureBox3.Image = basicCopy();
                    break;
                case "Greyscale":
                    pictureBox3.Image = greyscale();
                    break;
                case "Invert":
                    pictureBox3.Image = invert();
                    break;
                case "Histogram":
                    pictureBox3.Image = histogram();
                    break;
                case "Sepia":
                    pictureBox3.Image = sepia();
                    break;
                case "Subtract":
                    pictureBox3.Image = subtract();
                    break;
                case "Smoothen":
                    pictureBox3.Image = smoothen();
                    break;
                case "G Blur":
                    pictureBox3.Image = gaussianblur();
                    break;
                case "Sharpen":
                    pictureBox3.Image = sharpen();
                    break;
                case "Mean Remove":
                    pictureBox3.Image = meanremoval();
                    break;
                case "Emboss LP":
                    pictureBox3.Image = embosslaplascian();
                    break;
                case "Emboss HV":
                    pictureBox3.Image = embosshorizontalvertical();
                    break;
                case "Emboss A":
                    pictureBox3.Image = embossalldirections();
                    break;
                case "Emboss LY":
                    pictureBox3.Image = embosslossy();
                    break;
                case "Emboss HO":
                    pictureBox3.Image = embosshorizontalonly();
                    break;
                case "Emboss VO":
                    pictureBox3.Image = embossverticalonly();
                    break;
                default:
                    MessageBox.Show("No action selected");
                    break;
            }
        }

        private Bitmap basicCopy()
        {
            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixelColor = imageB.GetPixel(x, y);
                    result.SetPixel(x, y, pixelColor);
                }
            }
            return result;
        }

        private Bitmap greyscale()
        {
            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixelColor = imageB.GetPixel(x, y);

                    // get average of RGB values
                    int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color grayPixelColor = Color.FromArgb(gray, gray, gray);
                    result.SetPixel(x, y, grayPixelColor);
                }
            }
            return result;
        }

        private Bitmap invert()
        {
            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixelColor = imageB.GetPixel(x, y);

                    // subtract lightness from 255
                    Color invertedPixelColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    result.SetPixel(x, y, invertedPixelColor);
                }
            }
            return result;
        }

        private Bitmap histogram()
        {
            int[] hist = new int[256];

            // compute histogram
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixelColor = imageB.GetPixel(x, y);

                    // get grayscale value then increment histogram
                    int gray = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    hist[gray]++;
                }
            }

            // graph histogram onto bitmap
            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
                int max = hist.Max();

                for (int i = 0; i < 256; i++)
                {
                    int height = (int)((hist[i] / (float)max) * result.Height);
                    g.DrawLine(Pens.White, i, result.Height, i, result.Height - height);
                }
            }

            return result;
        }

        private Bitmap sepia()
        {
            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixelColor = imageB.GetPixel(x, y);

                    // sepia matrix
                    double sepiaR = (0.393 * pixelColor.R) + (0.769 * pixelColor.G) + (0.189 * pixelColor.B);
                    double sepiaG = (0.349 * pixelColor.R) + (0.686 * pixelColor.G) + (0.168 * pixelColor.B);
                    double sepiaB = (0.272 * pixelColor.R) + (0.534 * pixelColor.G) + (0.131 * pixelColor.B);

                    // clamp values to 0-255
                    sepiaR = Math.Min(255, sepiaR);
                    sepiaG = Math.Min(255, sepiaG);
                    sepiaB = Math.Min(255, sepiaB);

                    Color sepiaPixelColor = Color.FromArgb((int)sepiaR, (int)sepiaG, (int)sepiaB);

                    result.SetPixel(x, y, sepiaPixelColor);
                }
            }
            return result;
        }

        private Bitmap subtract()
        {
            if (imageA == null)
            {
                MessageBox.Show("No background image");
                return null;
            }
            Color mygreen = Color.FromArgb(0, 0, 255);
            int greygreen = (mygreen.R + mygreen.G + mygreen.B) / 3;
            int threshold = 5;

            Bitmap result = new Bitmap(imageB.Width, imageB.Height);
            for (int y = 0; y < imageB.Height; y++)
            {
                for (int x = 0; x < imageB.Width; x++)
                {
                    Color pixel = imageB.GetPixel(x, y);
                    Color backpixel = imageA.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractvalue = Math.Abs(grey - greygreen);

                    if (subtractvalue > threshold)
                        result.SetPixel(x, y, pixel);
                    else
                        result.SetPixel(x, y, backpixel);
                }
            }

            return result;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save image

            if (pictureBox3.Image == null)
            {
                MessageBox.Show("No image to save");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBox3.Image.Save(saveFileDialog.FileName);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // clear form

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            imageA = null;
            imageB = null;
            stopCamera();
            defaultForm();
        }


        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Copy";
            label1.Text = "Basic Copy";
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Greyscale";
            label1.Text = "Greyscale";
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Invert";
            label1.Text = "Color Inversion";
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Histogram";
            label1.Text = "Histogram";
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Sepia";
            label1.Text = "Sepia";
        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(true);
            toggleNumericUpDownTool(false);
            button3.Text = "Subtract";
            label1.Text = "Subtraction";
        }
        private void toggleOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (cam == null)
            //{
            //    MessageBox.Show("No webcam found");
            //    return;
            //}

            //cam.ShowWindow(pictureBox1);
            videoSource.Start();
        }

        private void toggleOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (cam != null)
            //{
            //    cam.Stop();
            //    pictureBox1.Image = null;
            //}
            stopCamera();
        }
        private void toggleSecondBox(Boolean visible)
        {
            pictureBox2.Visible = visible;
            button2.Visible = visible;
        }
        private String determineAction()
        {
            return button3.Text;
        }
        private void defaultForm()
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Null";
            label1.Text = "No action selected";
        }
        private void stopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = null;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No webcam found");
                return;
            }

            // pick first webcam
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += VideoSource_NewFrame;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();

            imageB = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopCamera();
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }


        // ----  CONVOLUTION MATRIX CLASS ----
        private void smoothingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(true);
            button3.Text = "Smoothen";
            label1.Text = "Smoothing";
        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(true);
            button3.Text = "G Blur";
            label1.Text = "Gaussian Blur";
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Sharpen";
            label1.Text = "Sharpen";
        }

        private void meanRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Mean Remove";
            label1.Text = "Mean Removal";
        }

        private void laplascianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss LP";
            label1.Text = "Emboss: Laplascian";
        }

        private void horizontalVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss HV";
            label1.Text = "Emboss: Horizontal/Vertical";
        }

        private void allDirectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss A";
            label1.Text = "Emboss: All Directions";
        }

        private void lossyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss LY";
            label1.Text = "Emboss: Lossy";
        }

        private void horizontalOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss HO";
            label1.Text = "Emboss: Horizontal Only";
        }

        private void verticalOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            toggleNumericUpDownTool(false);
            button3.Text = "Emboss VO";
            label1.Text = "Emboss: Vertical Only";
        }

        // convolution matrix functions

        private Bitmap smoothen(int nWeight = 1)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = nWeight * nWeightMultiplier;
            m.Factor = nWeight + 8;

            return ConvMatrix.Conv3x3(imageB, m);
        }
        private Bitmap gaussianblur(int nWeight = 4)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 1, 2, 1, 2, nWeight * nWeightMultiplier, 2, 1, 2, 1 });
            m.Factor = 16;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap sharpen(int nWeight = 11)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 0, -2, 0, -2, nWeight, -2, 0, -2, 0 });
            m.Factor = 3;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap meanremoval(int nWeight = 9)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { -1, -1, -1, -1, nWeight, -1, -1, -1, -1 });
            m.Factor = 1;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embosslaplascian(int nWeight = 4)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { -1, 0, -1, 0, nWeight, 0, -1, 0, -1 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embosshorizontalvertical(int nWeight = 4)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 0, -1, 0, -1, nWeight, -1, 0, -1, 0 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embossalldirections(int nWeight = 8)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { -1, -1, -1, -1, nWeight, -1, -1, -1, -1 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embosslossy(int nWeight = 4)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 1, 2, 1, -2, nWeight, -2, -2, 1, -2 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embosshorizontalonly(int nWeight = 2)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 0, 0, 0, -1, nWeight, -1, 0, 0, 0 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private Bitmap embossverticalonly(int nWeight = 0)
        {
            ConvMatrix m = new ConvMatrix();
            m.SetManual(new int[] { 0, -1, 0, 0, nWeight, 0, 0, 1, 0 });
            m.Factor = 1;
            m.Offset = 127;

            return ConvMatrix.Conv3x3(imageB, m);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // increase or decrease nWeight for Convolution Matrix functions
            nWeightMultiplier = (int)numericUpDown1.Value;
        }

        private void toggleNumericUpDownTool(bool enable)
        {
            numericUpDown1.Enabled = enable;
            numericUpDown1.Visible = enable;
        }
    }
}
