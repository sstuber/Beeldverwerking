using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.VisualStyles;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return; // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose(); // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];
                // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width*InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y); // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // TODO: include here your own code
            // example: create a negative image

            Image = ApplyGrayScale(Image);

            Image = ApplyContrastAdjustment(Image);

            //Image = ApplyMedianFilter(5, 5, Image);

           // Image = ApplyGaussianFilter(5, 5, 2, Image);

            Image = ApplyEdgeDetection(GetSobelEdgeFilter(), Image);

            Image = ApplyThreshold(10, Image);

            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]); // Set the pixel color at coordinate (x,y)
                }
            }

            pictureBox2.Image = (Image) OutputImage; // Display output image
            progressBar.Visible = false; // Hide progress bar
        }

        private Color[,] ApplyThreshold(int threshold, Color[,] image)
        {
            int width = image.GetLength(0);
            int heigth = image.GetLength(1);
            Color[,] appliedImage = new Color[width,heigth];
            
            for(int u = 0; u <width;u++)
                for (int v = 0; v < heigth; v++)
                {
                    int newColor = image[u, v].G > threshold ? 0 : 255 ;
                    appliedImage[u, v] = Color.FromArgb(newColor, newColor, newColor);
                }


            return appliedImage;
        }

        #region Edge detection

        private double[,] GetSobelEdgeFilter()
        {
            double[,] filter = new double[3, 3]
            {
                {-1,0,1},
                {-2,0,2},
                {-1,0,1}
            };

            return filter;
        }

        private double[,] ClockwiseFilterTurn(double[,] filter)
        {
            double[,] turnedFitler = new double[3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    turnedFitler[i, j] = filter[3 - j - 1, i];

            return turnedFitler;
        }

        private Color[,] ApplyEdgeDetection(double[,] filter, Color[,] image)
        {
            double[,] HxValues = new double[image.GetLength(0), image.GetLength(1)];
            double[,] HyValues = new double[image.GetLength(0), image.GetLength(1)];

            double [,] turnedFilter = ClockwiseFilterTurn(filter);

            int centerX = (int)Math.Ceiling((double)filter.GetLength(0) / 2);
            int centerY = (int)Math.Ceiling((double)filter.GetLength(1) / 2);

            for (int u = 0; u < image.GetLength(0) ; u++)
                for (int v =0; v < image.GetLength(1) ; v++)
                {
                    double weigth = 0;
                    double newValuex = 0;
                    double newValuey = 0;

                    for (int i = 0; i < filter.GetLength(0); i++)
                        for (int j = 0; j < filter.GetLength(1); j++)
                        {
                            int tempU = u + i - centerX - 1;
                            int tempV = v + j - centerY - 1;
                            if (tempU < 0)
                                tempU = 0;

                            if (tempU > image.GetLength(0))
                                tempU = image.GetLength(0) - 1;

                            if (tempV < 0)
                                tempV = 0;

                            if (tempV > image.GetLength(1))
                                tempV = image.GetLength(1) - 1;
                            weigth += filter[i, j] > 0 ? filter[i, j] : -filter[i, j];
                            
                            newValuex += image[tempU,tempV].G * filter[i, j];
                            newValuey += image[tempU, tempV].G * turnedFilter[i, j];
                        }

                    newValuex = newValuex * (1 / weigth);
                    newValuey = newValuey * (1 / weigth);
                    HxValues[u, v] = newValuex;
                    HyValues[u, v] = newValuey;
                }

            Color[,] edgeStrengthImage = new Color[image.GetLength(0), image.GetLength(1)];

            for (int u = centerX - 1; u < image.GetLength(0) - centerX; u++)
                for (int v = centerY - 1; v < image.GetLength(1) - centerY; v++)
                { 
                    var edgeStrength = (int)Math.Sqrt(Math.Pow(HxValues[u, v], 2) + Math.Pow(HyValues[u, v], 2));
                    edgeStrengthImage[u, v] = Color.FromArgb(edgeStrength, edgeStrength, edgeStrength);
                }

            return edgeStrengthImage;

        }
        #endregion

        private Color[,] ApplyGrayScale(Color[,] image) 
        {
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y]; // Get the pixel color at coordinate (x,y)
                    int newColor = (int)(pixelColor.R * 0.2125f + pixelColor.G * 0.7154 + pixelColor.B * 0.072);
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor); // Negative image
                    image[x, y] = updatedColor; // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep(); // Increment progress bar
                }
            }

            return image;
        }

        private Color[,] ApplyContrastAdjustment(Color[,] image)
        {
            int aLow = 255;
            int aHigh = 0;

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];
                    aLow = pixelColor.G < aLow ? pixelColor.G : aLow;
                    aHigh = pixelColor.G > aHigh ? pixelColor.G : aHigh;
                }
            }

            int multiplier = 255 / (aHigh - aLow);

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];
                    int newColor = (pixelColor.G - aLow) * multiplier;
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor); // Negative image
                    image[x, y] = updatedColor;
                }
            }

            return image;
        }

        /* 
           to do
           laat nieuwe image returnen
           maak apply filter function
           maak normalize filter function 

            */



        private Color[,] ApplyMedianFilter(int x, int y, Color[,] image)
        {
            int centerX = (int)Math.Ceiling((double)x / 2);
            int centerY = (int)Math.Ceiling((double)y / 2);

            for (int u = centerX - 1; u < image.GetLength(0) - centerX; u++)
                for (int v = centerY - 1; v < image.GetLength(1) - centerY; v++)
                {
                    List<int> intList = new List<int>();

                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                            intList.Add(image[u + i - (centerX - 1), v + j - (centerY - 1)].G);

                    intList.Sort();

                    int median = intList[intList.Count/2];

                    image[u, v] = Color.FromArgb(median,median,median);
                }

            return image;
        }

#region Gaussion filter

        private Color[,] ApplyGaussianFilter(int x, int y, int sigma, Color[,] image)
        {
            double[,] filter = makeGaussianFilterBox(x, y, sigma);
            Color[,] newImage = new Color[image.GetLength(0),image.GetLength(1)];

            int centerX = (int)Math.Ceiling((double)x /2);
            int centerY = (int)Math.Ceiling((double)y / 2);

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1) ; v++)
                {
                    double newValue = 0;
                    double weigth = 0;

                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                        {
                            int tempU = u + i - centerX - 1;
                            int tempV = v + j - centerY - 1;
                            if (tempU < 0)
                                tempU = 0;

                            if (tempU > image.GetLength(0))
                                tempU = image.GetLength(0) - 1;

                            if (tempV < 0)
                                tempV = 0;

                            if (tempV > image.GetLength(1))
                                tempV = image.GetLength(1) - 1;

                            weigth += filter[i, j];
                            newValue += image[tempU ,tempV].G * filter[i,j];
                        }

                    newValue = newValue*(1/weigth);
                    int intValue = (int) newValue;
                    Color newColor = Color.FromArgb(intValue, intValue, intValue);
                    newImage[u, v] = newColor;
                }

            return newImage;
        }

        private double[,] makeGaussianFilterBox(int x, int y, int sigma)
        {
            double[,] filter = new double[x, y];
            double noemer = 2 * sigma * sigma;

            int centerX = (int)Math.Ceiling((double)x / 2);
            int centerY = (int)Math.Ceiling((double)y / 2);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    double teller = Math.Pow(i - centerX, 2) + Math.Pow(j - centerY, 2);
                    filter[i, j] = Math.Pow(Math.E, -(teller / noemer));
                }

            return filter;
        }
#endregion

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
