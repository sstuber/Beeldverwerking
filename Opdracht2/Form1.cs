﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            //==========================================================================================
            // TODO: include here your own code
            // example: create a negative image


            // var  Image2 = ApplyErosion(Image, GetstructureElement(), null);

             Image = ApplyComplement(Image);

            Image = ApplyDilation(Image, GetstructureElement(), null);

            MessageBox.Show("count is " + CountValues(Image));

            Image = ApplyComplement(Image);

            // Image = ApplyAnd(Image, ApplyComplement(Image2));


          /*  Image = ApplyOpening(Image, GetstructureElement(), null);
            Image = ApplyComplement(Image);*/
            //==========================================================================================

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            

            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }

        private double[,] GetVariableStructure(int width)
        {
            int x = width*2 + 1;
            int y = x;

            double[,] element = new double[x,y];

            return element;
        }



        private int CountValues(Color[,] image)
        {
            Dictionary<int,int> valueDictionary = new Dictionary<int, int>();

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)  
                    if(!valueDictionary.ContainsKey(image[u, v].G))
                        valueDictionary.Add(image[u,v].G,0);

            return valueDictionary.Count;
        }

        private Color[,] ApplyAnd(Color[,] firstImage, Color[,] secondImage)
        {

            if (firstImage.GetLength(0) != secondImage.GetLength(0) ||
                firstImage.GetLength(1) != secondImage.GetLength(1))
            {
                MessageBox.Show("Error images not the same size");
                return firstImage;
            }

            Color[,] newImage = new Color[firstImage.GetLength(0),firstImage.GetLength(1)];

            for (int u = 0; u < firstImage.GetLength(0); u++)
                for (int v = 0; v < firstImage.GetLength(1); v++)
                {
                    int newValue = firstImage[u, v].G == 255 && secondImage[u, v].G == 255 ? 255 : 0;
                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue);
                }

            return newImage;
        }

        private Color[,] ApplyOr(Color[,] firstImage, Color[,] secondImage)
        {

            if (firstImage.GetLength(0) != secondImage.GetLength(0) ||
                firstImage.GetLength(1) != secondImage.GetLength(1))
            {
                MessageBox.Show("Error images not the same size");
                return firstImage;
            }

            Color[,] newImage = new Color[firstImage.GetLength(0), firstImage.GetLength(1)];

            for (int u = 0; u < firstImage.GetLength(0); u++)
                for (int v = 0; v < firstImage.GetLength(1); v++)
                {
                    int newValue = firstImage[u, v].G == 255 || secondImage[u, v].G == 255 ? 255 : 0;
                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue);
                }


            return newImage;
        }

        private Color[,] ApplyComplement(Color[,] image)
        {
            Color[,] newImage = new Color[image.GetLength(0),image.GetLength(1)];
            int maxValue = 255;

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    int newValue = maxValue - image[u, v].G;
                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue);
                }

            return newImage;
        }


        private double[,] GetstructureElement()
        {
            return new double[3,3]
            {
                {0,0,0},
                {0,0,0},
                {0,0,0}
            };
        }

        private Color[,] ApplyOpening(Color[,] image, double[,] structure, Color[,] controlImage)
        {
            Color[,] newImage = ApplyErosion(image, structure, controlImage);
            newImage = ApplyDilation(newImage, structure, controlImage);

            return newImage;
        }

        private Color[,] ApplyClosing(Color[,] image, double[,] structure, Color[,] controlImage)
        {
            Color[,] newImage = ApplyDilation(image, structure, controlImage);
            newImage = ApplyErosion(newImage, structure, controlImage);

            return newImage;
        }

        // Function that applies the Median filter
        private Color[,] ApplyDilation(Color[,] image, double[,] structure, Color[,] controlImage)
        {
            if (controlImage != null && (image.GetLength(0) != controlImage.GetLength(0) ||
                image.GetLength(1) != controlImage.GetLength(1)) 
                ) 
            {
                MessageBox.Show("Error images not the same size");
                return image;
            }

            int x = structure.GetLength(0);
            int y = structure.GetLength(1);

            int centerX = x / 2;
            int centerY = y / 2;

            Color [,] newImage = new Color[image.GetLength(0),image.GetLength(1)];

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    List<int> intList = new List<int>();

                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                        {
                            int tempU = u + i - centerX;
                            int tempV = v + j - centerY;
                            if (tempU < 0)
                               continue;

                            if (tempU >= image.GetLength(0))
                                continue;

                            if (tempV < 0)
                                continue;

                            if (tempV >= image.GetLength(1))
                                continue;
                            intList.Add(image[tempU, tempV].G + (int)structure[i,j]);
                        }

                    intList.Sort(); // Sort the list of integers

                    int newValue = Math.Min( intList[intList.Count-1], 255); // Take the highest of the values in the list

                    if (controlImage != null) // clamp onto the control image 
                        newValue = Math.Min(controlImage[u, v].G, newValue);

                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue); // Adjust the color value accordingly
                }

            return newImage;
        }

        private Color[,] ApplyErosion(Color[,] image, double[,] structure, Color[,] controlImage)
        {
            if (controlImage != null && (image.GetLength(0) != controlImage.GetLength(0) ||
                image.GetLength(1) != controlImage.GetLength(1))
                )
            {
                MessageBox.Show("Error images not the same size");
                return image;
            }

            int x = structure.GetLength(0);
            int y = structure.GetLength(1);

            int centerX = x / 2;
            int centerY = y / 2;

            Color[,] newImage = new Color[image.GetLength(0), image.GetLength(1)];

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    List<int> intList = new List<int>();

                    for (int i = 0; i < x; i++)
                        for (int j = 0; j < y; j++)
                        {
                            int tempU = u + i - centerX;
                            int tempV = v + j - centerY;
                            if (tempU < 0)
                                continue;

                            if (tempU >= image.GetLength(0))
                                continue;

                            if (tempV < 0)
                                continue;

                            if (tempV >= image.GetLength(1))
                                continue;
                            intList.Add(image[tempU, tempV].G - (int)structure[i, j]);
                        }

                    intList.Sort(); // Sort the list of integers

                    int newValue = Math.Max(intList[0], 0); // Take the highest of the values in the list

                    if (controlImage != null) // clamp onto the control image 
                        newValue = Math.Max(controlImage[u, v].G, newValue);

                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue); // Adjust the color value accordingly
                }

            return newImage;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
