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

        public Contour()
        {
            Coordinates = new List<Tuple<Coordinate,int>>();
        }
    }
}

