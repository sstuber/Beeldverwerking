using System;
using System.Collections.Generic;

namespace INFOIBV
{
    public class Contour
    {
        public List<Tuple<Coordinate, int>> Coordinates;
        public double Length;
        public double Area;
        public double Circularity;
        public List<Coordinate> ContainingPixels;
        public int Label;

        public Contour()
        {
            Coordinates = new List<Tuple<Coordinate,int>>();
            ContainingPixels = new List<Coordinate>();
        }
    }
}

