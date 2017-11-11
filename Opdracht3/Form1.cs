using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.VisualStyles;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Random random = new Random();

        private Bitmap InputImage;
        private Bitmap OutputImage;

        List<Contour> outerContours = new List<Contour>();
        List<Contour> innerContours = new List<Contour>();
        List<Contour> foundContours = new List<Contour>();

        private double sqrtOfTwo = Math.Sqrt(2);

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
            var OriginalImage = Image;
            // make the image grayscale and apply a foreground threshold
            Image = Preprocessing(Image);

            // find contours and compare circularity
            ObjectDetection(Image);

            // reject not yellow contours
            Image = FinalProcess(OriginalImage);

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

#region Colorfiltering

        private Color[,] FinalProcess(Color[,] OriginalImage)
        {
            CompareYellow(OriginalImage);

            var newImage = GetFinalImage(OriginalImage);

            // add bounding boxes for our final contours
           foreach (var contour in foundContours)
                newImage = MakeBoundingBox(newImage, contour);

            return newImage;
        }

        // Make all pixels of our found objects the original color and the other pixels black
        private Color[,] GetFinalImage(Color[,] OriginalImage)
        {
            int x = OriginalImage.GetLength(0);
            int y = OriginalImage.GetLength(1);

            Color[,] newImage = new Color[x, y];

            for(int i=0; i<x; i++)
                for (int j = 0; j < y; j++)
                {
                    newImage[i, j] = Color.FromArgb(0, 0, 0);
                }

            foreach (var contour in foundContours)
            {
                foreach (var coordinate in contour.ContainingPixels)
                {
                    newImage[coordinate.x, coordinate.y] = OriginalImage[coordinate.x, coordinate.y];
                }
            }

            return newImage;
        }

        private void CompareYellow(Color[,] image)
        {
            var newContours = new List<Contour>();

            foreach (var contour in foundContours)
            {

                double searchCount = contour.Coordinates.Count/100* 20;
                var misCount = 0;
                for(int i =0; i < searchCount; i++)
                {
                    int j = random.Next(contour.ContainingPixels.Count - 1);
                    var coordinate = contour.ContainingPixels[j];
                    var color = image[coordinate.x, coordinate.y];
                    var hue = color.GetHue();
                    // compare hue value of hsv to find the mostly yellow objects of our found objects
                    if (hue < 25 || hue > 62)
                        misCount++;
                }

                // If more then 20% is not yellow or orange we reject the object from our found objects
                if (misCount <  searchCount /100*10)
                    newContours.Add(contour);
            }

            foundContours = newContours;
        }

#endregion

        #region CombinedContourLabeling

        private void ObjectDetection(Color[,] image)
        {
            // Contourlist are globaly saved
            int[,] labelMap = RegionLabeling(outerContours, innerContours, image);

            foundContours = CompareCircularity(outerContours);

        }

        private int[,] RegionLabeling(List<Contour> outerContours, List<Contour> innerContours, Color[,] image)
        {
            // Create label map and set initial values to 0
            int[,] labelMap = new int[InputImage.Size.Width, InputImage.Size.Height];
            for (int i = 0; i < labelMap.GetLength(0); i++)
                for (int j = 0; j < labelMap.GetLength(1); j++)
                    labelMap[i, j] = 0;

            int regionCounter = 0;
            for (int y = 0; y < InputImage.Size.Height; y++)
            {
                int currentLabel = 0;
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    // Check if current pixel is foreground
                    if (image[x, y].G == 1)
                    {
                        if (currentLabel != 0)
                        {
                            labelMap[x, y] = currentLabel;
                            var contour = outerContours.Find(obj => obj.Label == currentLabel);
                            contour.ContainingPixels.Add(new Coordinate(x,y));
                        }
                        else
                        {
                            currentLabel = labelMap[x, y];
                            if (currentLabel == 0) // Create new outer contour
                            {
                                regionCounter++;
                                currentLabel = regionCounter;
                                // Trace the contour
                                var startPoint = new Coordinate(x, y);
                                var contour = TraceContour(startPoint, 0, currentLabel, image, labelMap);
                                outerContours.Add(contour); // Add to the outerContours
                                labelMap[x, y] = currentLabel;
                                contour.Label = currentLabel;
                                contour.ContainingPixels.Add(new Coordinate(x,y));
                                
                            }
                        }
                    }
                    else // Background pixel
                    {
                        if (currentLabel != 0)
                        {
                            if (labelMap[x, y] == 0)
                            {
                                var startPoint = new Coordinate(x - 1, y);
                                var contour = TraceContour(startPoint, 1, currentLabel, image, labelMap);
                                innerContours.Add(contour); // Add to the innerContours
                            }
                            currentLabel = 0;
                        }
                    }
                }
            }

            return labelMap;
        }
        
        // Calculate the area using the contour points
        private double ContourArea(Contour contour)
        {
            var coordinates = contour.Coordinates;
            int M = coordinates.Count;

            double total = 0;

            for (int i = 0; i < M; i++)
            {
                Coordinate coordinateI = coordinates[i].Item1;
                Coordinate coordinateJ = coordinates[i + 1 >= coordinates.Count  ?  0 : i +1].Item1;

                double tmp =  coordinateI.x * (coordinateJ.y % M) - (coordinateJ.x%M) * coordinateI.y;
                total += tmp;
            }

            return total/2;
        }

        private Contour TraceContour(Coordinate start, int initDirection, int label, Color[,] image, int[,] labelMap)
        {
            var contour = new Contour();
            // Find next point in contour
            var nextPoint = FindNextPoint(start, initDirection, image, labelMap);
            // Add coordinate to the contour
            contour.Coordinates.Add(nextPoint);
            int newDirection = nextPoint.Item2;
            var prevCoord = start;
            var current = nextPoint.Item1;
            bool done = start == nextPoint.Item1;

            while (!done)
             {
                labelMap[current.x, current.y] = label;
                newDirection = (newDirection + 6) % 8;
                var afterNextPoint = FindNextPoint(current, newDirection, image, labelMap);
                // Update previous and current point
                prevCoord = current;
                current = afterNextPoint.Item1;
                newDirection = afterNextPoint.Item2;

                // Check if back at start coordinate
                    done = (prevCoord == start) && (current == nextPoint.Item1);
                if (!done)
                    contour.Coordinates.Add(afterNextPoint);
            }

            return contour;
        }

        private Tuple<Coordinate, int> FindNextPoint(Coordinate start, int direction, Color[,] image, int[,] labelMap)
        {
            // Search in 7 directions
            for (int i = 0; i < 7; i++)
            {
                var delta = DeltaCoordinate(direction);
                var newCoordinate = start + delta;

                if (newCoordinate.x < 0 || newCoordinate.y < 0 || newCoordinate.x >= image.GetLength(0) ||
                    newCoordinate.y >= image.GetLength(1))
                {
                    direction = (direction + 1) % 8;
                    continue;
                }

                if (image[newCoordinate.x, newCoordinate.y].G == 0)
                {
                    labelMap[newCoordinate.x, newCoordinate.y] = -1; // Mark background as visited
                    direction = (direction + 1) % 8;
                }
                else
                    return Tuple.Create(newCoordinate, direction);
            }
            // Returning to start point because no next point was found
            return Tuple.Create(start, direction);
        }

        // Calculate the circularity for a contour and save it
        private double ContourCircularity(Contour contour)
        {
            double contourLength = ContourLength(contour);
            double contourArea = ContourArea(contour);
            double contourCircularity = contourArea/Math.Pow(contourLength, 2)*4*Math.PI;

            contour.Length = contourLength;
            contour.Area = contourArea;
            contour.Circularity = contourCircularity;

            return contourCircularity;
        }

        // Returns coordinate based on direction
        private Coordinate DeltaCoordinate(int direction)
        {
            Coordinate deltaCoordinate = new Coordinate(0, 0);

            // Set coordinate depending on direction
            switch (direction)
            {
                case 0:
                    deltaCoordinate = new Coordinate(1,0);
                    break;
                case 1:
                    deltaCoordinate = new Coordinate(1, 1);
                    break;
                case 2:
                    deltaCoordinate = new Coordinate(0, 1);
                    break;
                case 3:
                    deltaCoordinate = new Coordinate(-1, 1);
                    break;
                case 4:
                    deltaCoordinate = new Coordinate(-1, 0);
                    break;
                case 5:
                    deltaCoordinate = new Coordinate(-1, -1);
                    break;
                case 6:
                    deltaCoordinate = new Coordinate(0, -1);
                    break;
                case 7:
                    deltaCoordinate = new Coordinate(1, -1);
                    break;               
            }

            return deltaCoordinate;
        }

        // Get contour lentgh
        private double ContourLength(Contour contour)
        {
            return contour.Coordinates.Sum(obj => directionLength(obj.Item2));
        }

        // Get the length for each direction type
        private double directionLength(int direction)
        {
            double length = 0;

            switch (direction)
            {
                case 0:
                case 2:
                case 4:
                case 6:
                   // deltaCoordinate = new Coordinate(1, 0);
                    length = 1;
                    break;
                case 1:
                case 3:
                case 5:
                case 7:
           //         deltaCoordinate = new Coordinate(1, 1);
                    length = sqrtOfTwo;
                    break;
            }

            return length;
        }

        // Calculate circularity of all outercontours and only keep contours with certain circularity
        private List<Contour> CompareCircularity(List<Contour> contoursList)
        {
            List<Contour> returnList = new List<Contour>();

            foreach (var contour in contoursList)
            {
                double circularity = ContourCircularity(contour);

                if (circularity > 0.05 && circularity < 0.15)
                    returnList.Add(contour);
            }

            return returnList;
        }

        // draw a bouding box around a contour
        private Color[,] MakeBoundingBox(Color[,] image, Contour contour)
        {
            var corners = FindBoundingBox(contour, image.GetLength(0), image.GetLength(1));

            Color boxColor = Color.SpringGreen;

            var current = corners.Item1; // topleft

            while (current.x != corners.Item2.x)
            {
                image[current.x, current.y] = boxColor;

                current.x++;
            }

            while (current.y != corners.Item2.y)
            {
                image[current.x, current.y] = boxColor;
                current.y++;
            }

            while (current.x != corners.Item1.x)
            {
                image[current.x, current.y] = boxColor;
                current.x--;
            }

            while (current.y != corners.Item1.y)
            {
                image[current.x, current.y] = boxColor;
                current.y--;
            }

            return image;
        }

        private Tuple<Coordinate, Coordinate> FindBoundingBox(Contour contour, int x, int y)
        {
            Coordinate topLeft = new Coordinate(x,y);
            Coordinate bottemRight = new Coordinate(0,0);

            foreach (var cor in contour.Coordinates)
            {
                if (topLeft.x > cor.Item1.x)
                    topLeft.x = cor.Item1.x;

                if (topLeft.y > cor.Item1.y)
                    topLeft.y = cor.Item1.y;

                if (bottemRight.x < cor.Item1.x)
                    bottemRight.x = cor.Item1.x;

                if (bottemRight.y < cor.Item1.y)
                    bottemRight.y = cor.Item1.y;
            }

            return new Tuple<Coordinate, Coordinate>(topLeft,bottemRight);
        }

        #endregion

        #region preprocessing

        private Color[,] Preprocessing(Color[,] image)
        {
            Color[,] newImage;

            newImage = ApplyGrayScale(image);
            newImage = ApplyContrastAdjustment(newImage);
            newImage = ApplyMedianFilter(3, 3, newImage);
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
