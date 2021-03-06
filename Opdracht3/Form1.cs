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

            Image = Preprocessing(Image);
            Image = ObjectDetection(Image);
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

        #region objectDetection

        private Color[,] ObjectDetection(Color[,] image)
        {
            Color[,] newImage;



            int[,] intArray = RegionLabeling(convertColorToInts(image));
            newImage = convertIntsToColors(intArray);

           // int count = tellInts.Count;

            return newImage;
        }

        private Color[,] convertIntsToColors(int[,]array)
        { 
            Color[,] newImage = new Color[array.GetLength(0), array.GetLength(1)];

            Dictionary<int, int> tellInts = new Dictionary<int, int>();

            int totalInts = 2;

            for (int y = 0; y < InputImage.Size.Height; y++)
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    if (!tellInts.ContainsKey(array[x, y]))
                    {
                        tellInts.Add(array[x, y], totalInts);
                        totalInts++;
                    }
                }

            for (int y = 0; y < InputImage.Size.Height; y++)
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    int value = tellInts[array[x, y]];
                    newImage[x, y] = Color.FromArgb(value, value, value);
                }

                    return newImage;
        }

        private int[,] RegionLabeling(int[,] image)
        {

            //List<Tuple2> collisionList = new List<Tuple2>();
            List<GraphNode> graphList = new List<GraphNode>();
            
            int allTimeHighestLabel = 1;
            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    List<int> seenLabels = new List<int>();


                    if (image[x, y] == 0)
                        continue;

                    int highestLabel = 0;

                    //check for existing labels in top 3 blocks ignore if out of bounds
                    for (int i = -1; i < 2; i++)
                    {
                        if (y - 1 < 0)
                            break;
                        if (x + i < 0 || x + i >= InputImage.Size.Width)
                            continue;

                        // save all the seen labels in a list
                        if (image[x + i, y - 1] > 1 && !seenLabels.Contains(image[x + i, y - 1]))
                            seenLabels.Add(image[x + i, y - 1]);

                        if (image[x + i, y - 1] > highestLabel)
                            highestLabel = image[x + i, y - 1];

                    }

                    // test left for existing label, ignore if out of bounds
                    if (x - 1 > 0)
                    {
                        if(image[x -1, y ] > 1 && !seenLabels.Contains(image[x -1, y]))
                            seenLabels.Add(image[x -1, y ]);

                        if (image[x - 1, y] > highestLabel)
                            highestLabel = image[x - 1, y];
                    }
                
                // set label according found highest label

                  /*  if (highestLabel == 0)
                        continue;*/

                    // introduce new label
                    if (highestLabel == 1 || image[x,y] == 1)
                    {
                        allTimeHighestLabel++;
                        highestLabel = allTimeHighestLabel;
                        graphList.Add(new GraphNode(highestLabel));

                    }

                    foreach (var label in seenLabels)
                    {
                        if(label == highestLabel)
                            continue;

                        GraphNode node1 = graphList.Find(testnode => testnode.label == highestLabel);
                        GraphNode node2 = graphList.Find(testnode =>  testnode.label == label );

                        if (node1.label < node2.label)
                            node2.ParentNode = node1;
                        else
                            node1.ParentNode = node2;
                        //collisionList.Add(new Tuple2(highestLabel,label));
                    }

                    // set label
                    //Color newColor = Color.FromArgb(highestLabel,highestLabel,highestLabel);
                    image[x, y] = highestLabel;                
                }
            }


            /*// resolve collisions 
            foreach (var maintuple in collisionList)
            {
                foreach (var subtuple in collisionList)
                {
                    if (subtuple.Equals(maintuple))
                        continue;

                    if (maintuple.subLabel == subtuple.mainLabel)
                        subtuple.mainLabel = maintuple.mainLabel;
                }
            }
            */

           for (int y = 0; y < InputImage.Size.Height; y++)
                for (int x = 0; x < InputImage.Size.Width; x++)
                    //if there is a label
                    if (image[x, y] > 0)
                    {
                        GraphNode currentNode = graphList.Find(testnode => testnode.label == image[x,y]);
                        int g = image[x, y];
                        while (currentNode.ParentNode!= null)
                        {
                            currentNode = currentNode.ParentNode;
                        }
                        image[x, y] = currentNode.label;

                    }

            return image;
        }

        private int[,] convertColorToInts(Color[,] image)
        {
            int[,] returnArray = new int[InputImage.Size.Width,InputImage.Size.Height];

            for (int y = 0; y < InputImage.Size.Height; y++)
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    returnArray[x, y] = image[x, y].G;
                }

            return returnArray;

        }

#endregion
            
        #region preprocessing

        private Color[,] Preprocessing(Color[,] image)
        {
            Color[,] newImage;// = new Color[image.GetLength(0), image.GetLength(1)];

            newImage = ApplyGrayScale(image);
            newImage = ApplyContrastAdjustment(newImage);
            //newImage = ApplyGaussianFilter(2, 2, 2, newImage);
            newImage = ApplyMedianFilter(3, 3, newImage);
            //newImage = ApplyEdgeDetection(GetSobelEdgeFilter(), newImage);
            newImage = ApplyThreshold(FindForegroundThreshold(newImage), newImage);
            return newImage;
        }

        private int FindForegroundThreshold(Color[,] image)
        {
            Color[,] newImage = new Color[image.GetLength(0), image.GetLength(1)];

            int [] histogram = new int[256];
            int totalCount = 0;
            float totalMean = 0;


            // calculate histogram and the total mean
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    int index = image[x, y].G;
                    histogram[index]++;
                    totalCount++;
                    totalMean += index;
                }
            }

            totalMean /= totalCount;

            int highestQ = 0;
            double highestVariance = 0;

            int countBackground;
            int countForeground;


            // find the q with the highest between variance
            for (int i = 0; i < 255; i++)
            {

                // get mean and count for this particular q split
                countBackground = 0;
                countForeground = 0;

                float meanBackground = 0;
                float meanForeground = 0;

                for (int u = 0; u < i + 1; u++)
                {
                    countBackground += histogram[u];
                    meanBackground += histogram[u]*u;
                }

                meanBackground /= countBackground;

                for (int u = i + 1; u < 256; u++)
                {
                    countForeground += histogram[u];
                    meanForeground += histogram[u]*u;
                }

                meanForeground/=countForeground;

                var tussen1 = countBackground * Math.Pow(meanBackground - totalMean, 2);
                var tussen2 = countForeground * Math.Pow(meanForeground - totalMean, 2);

                double betweenVariance = (tussen1 + tussen2)/totalCount;

                if (betweenVariance > highestVariance)
                {
                    highestVariance = betweenVariance;
                    highestQ = i;
                }
            }

            return highestQ;
        }


        private Color[,] ApplyThreshold(int threshold, Color[,] image)
        {
            int width = image.GetLength(0);
            int heigth = image.GetLength(1);
            Color[,] appliedImage = new Color[width, heigth];

            for (int u = 0; u < width; u++)
                for (int v = 0; v < heigth; v++)
                {
                    int newColor = image[u, v].G > threshold ? 1 : 0;
                    appliedImage[u, v] = Color.FromArgb(newColor, newColor, newColor);
                }

            return appliedImage;
        }

        private Color[,] ApplyGrayScale(Color[,] image)
        {
            Color[,] newImage = new Color[image.GetLength(0),image.GetLength(1)];
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y]; // Get the pixel color at coordinate (x,y)
                    int newColor = (int)(pixelColor.R * 0.2125f + pixelColor.G * 0.7154 + pixelColor.B * 0.072);
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor); // Negative image
                    newImage[x, y] = updatedColor; // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep(); // Increment progress bar
                }
            }

            return newImage;
        }
        private Color[,] ApplyContrastAdjustment(Color[,] image)
        {
            Color[,] newImage = new Color[image.GetLength(0), image.GetLength(1)];
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
                    newImage[x, y] = updatedColor;
                }
            }

            return newImage;
        }

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

        // Function that turns the kernel clockwise
        private double[,] ClockwiseFilterTurn(double[,] filter)
        {
            double[,] turnedFitler = new double[3, 3];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    turnedFitler[i, j] = filter[3 - j - 1, i];

            return turnedFitler;
        }

        private Color[,] ApplyGaussianFilter(int x, int y, int sigma, Color[,] image)
        {
            double[,] filter = makeGaussianFilterBox(x, y, sigma);
            Color[,] newImage = new Color[image.GetLength(0), image.GetLength(1)];

            int centerX = (int)Math.Ceiling((double)x / 2);
            int centerY = (int)Math.Ceiling((double)y / 2);

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    double newValue = 0;
                    double weigth = 0;

                    // Loop through the filter while taking the borders of the image into account
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
                            newValue += image[tempU, tempV].G * filter[i, j];
                        }

                    newValue = newValue * (1 / weigth);
                    int intValue = (int)newValue;
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

        // Function that applies the actual edge detection on an image using the Sobel filter
        private Color[,] ApplyEdgeDetection(double[,] filter, Color[,] image)
        {
            double[,] HxValues = new double[image.GetLength(0), image.GetLength(1)];
            double[,] HyValues = new double[image.GetLength(0), image.GetLength(1)];

            double[,] turnedFilter = ClockwiseFilterTurn(filter);

            int centerX = (int)Math.Ceiling((double)filter.GetLength(0) / 2);
            int centerY = (int)Math.Ceiling((double)filter.GetLength(1) / 2);

            for (int u = 0; u < image.GetLength(0); u++)
                for (int v = 0; v < image.GetLength(1); v++)
                {
                    double weigth = 0;
                    double newValuex = 0;
                    double newValuey = 0;

                    // Loop through the filter while taking the borders of the image into account
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

                            newValuex += image[tempU, tempV].G * filter[i, j];
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

        private Color[,] ApplyMedianFilter(int x, int y,  Color[,] image)
        {
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
                            intList.Add(image[tempU, tempV].G);
                        }

                    intList.Sort(); // Sort the list of integers

                    int newValue =intList[intList.Count / 2]; // Take the highest of the values in the list
                    newImage[u, v] = Color.FromArgb(newValue, newValue, newValue); // Adjust the color value accordingly
                }

            return newImage;
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
