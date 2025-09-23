using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap imageB, imageA;
        Device cam;

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

            Device[] devices = DeviceManager.GetAllDevices();
            if (devices.Length > 0)
                cam = devices[0]; 
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
                    Color invertedPixelColor = Color.FromArgb(255 -pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
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

                    Color sepiaPixelColor = Color.FromArgb((int) sepiaR, (int) sepiaG, (int) sepiaB);

                    result.SetPixel(x, y, sepiaPixelColor);
                }
            }
            return result;
        }

        private Bitmap subtract()
        {
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
            defaultForm();
        }
        

        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            button3.Text = "Copy";
            label1.Text = "Basic Copy";
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            button3.Text = "Greyscale";
            label1.Text = "Greyscale";
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            button3.Text = "Invert";
            label1.Text = "Color Inversion";
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            button3.Text = "Histogram";
            label1.Text = "Histogram";
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(false);
            button3.Text = "Sepia";
            label1.Text = "Sepia";
        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleSecondBox(true);
            button3.Text = "Subtract";
            label1.Text = "Subtraction";
        }
        private void toggleOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cam == null)
            {
                MessageBox.Show("No webcam found");
                return;
            }

            cam.ShowWindow(pictureBox1);
        }

        private void toggleOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cam != null)
            {
                cam.Stop();
                pictureBox1.Image = null;
            }
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
            button3.Text = "Null";
            label1.Text = "No action selected";
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

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cam != null)
            {
                cam.Stop();
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
