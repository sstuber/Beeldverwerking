using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV
{
    public class ConvexHull
    {

        // For debugging.
        public static Coordinate[] g_MinMaxCorners;
        public static Rectangle g_MinMaxBox;
        public static Coordinate[] g_NonCulledCoordinates;
        public static List<Coordinate> MakeConvexHull(List<Coordinate> Coordinates)
        {
            // Cull.
            Coordinates = HullCull(Coordinates);

            // Find the remaining Coordinate with the smallest Y value.
            // if (there's a tie, take the one with the smaller X value.
            Coordinate best_pt = Coordinates[0];
            foreach (Coordinate pt in Coordinates)
            {
                if ((pt.y < best_pt.y) ||
                   ((pt.y == best_pt.y) && (pt.x < best_pt.x)))
                {
                    best_pt = pt;
                }
            }

            // Move this Coordinate to the convex hull.
            List<Coordinate> hull = new List<Coordinate>();
            hull.Add(best_pt);
            Coordinates.Remove(best_pt);

            // Start wrapping up the other Coordinates.
            float sweep_angle = 0;
            for (;;)
            {
                // Find the Coordinate with smallest AngleValue
                // from the last Coordinate.
                int X = hull[hull.Count - 1].x;
                int Y = hull[hull.Count - 1].y;
                best_pt = Coordinates[0];
                float best_angle = 3600;

                // Search the rest of the Coordinates.
                foreach (Coordinate pt in Coordinates)
                {
                    float test_angle = AngleValue(X, Y, pt.x, pt.y);
                    if ((test_angle >= sweep_angle) &&
                        (best_angle > test_angle))
                    {
                        best_angle = test_angle;
                        best_pt = pt;
                    }
                }

                // See if the first Coordinate is better.
                // If so, we are done.
                float first_angle = AngleValue(X, Y, hull[0].x, hull[0].y);
                if ((first_angle >= sweep_angle) &&
                    (best_angle >= first_angle))
                {
                    // The first Coordinate is better. We're done.
                    break;
                }

                // Add the best Coordinate to the convex hull.
                hull.Add(best_pt);
                Coordinates.Remove(best_pt);

                sweep_angle = best_angle;

                // If all of the Coordinates are on the hull, we're done.
                if (Coordinates.Count == 0) break;
            }

            return hull;
        }

        private static void GetMinMaxCorners(List<Coordinate> Coordinates, ref Coordinate ul, ref Coordinate ur, ref Coordinate ll, ref Coordinate lr)
        {
            // Start with the first Coordinate as the solution.
            ul = Coordinates[0];
            ur = ul;
            ll = ul;
            lr = ul;

            // Search the other Coordinates.
            foreach (Coordinate pt in Coordinates)
            {
                if (-pt.x - pt.y > -ul.x - ul.y) ul = pt;
                if (pt.x - pt.y > ur.x - ur.y) ur = pt;
                if (-pt.x + pt.y > -ll.x + ll.y) ll = pt;
                if (pt.x + pt.y > lr.x + lr.y) lr = pt;
            }

            g_MinMaxCorners = new Coordinate[] { ul, ur, lr, ll }; // For debugging.
        }

        // Find a box that fits inside the MinMax quadrilateral.
        private static Rectangle GetMinMaxBox(List<Coordinate> Coordinates)
        {
            // Find the MinMax quadrilateral.
            Coordinate ul = new Coordinate(0, 0), ur = ul, ll = ul, lr = ul;
            GetMinMaxCorners(Coordinates, ref ul, ref ur, ref ll, ref lr);

            // Get the coordinates of a box that lies inside this quadrilateral.
            int xmin, xmax, ymin, ymax;
            xmin = ul.x;
            ymin = ul.y;

            xmax = ur.x;
            if (ymin < ur.y) ymin = ur.y;

            if (xmax > lr.x) xmax = lr.x;
            ymax = lr.y;

            if (xmin < ll.x) xmin = ll.x;
            if (ymax > ll.y) ymax = ll.y;

            Rectangle result = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
            g_MinMaxBox = result;    // For debugging.
            return result;
        }

        // Cull Coordinates out of the convex hull that lie inside the
        // trapezoid defined by the vertices with smallest and
        // largest X and Y coordinates.
        // Return the Coordinates that are not culled.
        private static List<Coordinate> HullCull(List<Coordinate> Coordinates)
        {
            // Find a culling box.
            Rectangle culling_box = GetMinMaxBox(Coordinates);

            // Cull the Coordinates.
            List<Coordinate> results = new List<Coordinate>();
            foreach (Coordinate pt in Coordinates)
            {
                // See if (this Coordinate lies outside of the culling box.
                if (pt.x <= culling_box.Left ||
                    pt.x >= culling_box.Right ||
                    pt.y <= culling_box.Top ||
                    pt.y >= culling_box.Bottom)
                {
                    // This Coordinate cannot be culled.
                    // Add it to the results.
                    results.Add(pt);
                }
            }

            g_NonCulledCoordinates = new Coordinate[results.Count];   // For debugging.
            results.CopyTo(g_NonCulledCoordinates);              // For debugging.
            return results;
        }

        private static float AngleValue(int x1, int y1, int x2, int y2)
        {
            float dx, dy, ax, ay, t;

            dx = x2 - x1;
            ax = Math.Abs(dx);
            dy = y2 - y1;
            ay = Math.Abs(dy);
            if (ax + ay == 0)
            {
                // if (the two Coordinates are the same, return 360.
                t = 360f / 9f;
            }
            else
            {
                t = dy / (ax + ay);
            }
            if (dx < 0)
            {
                t = 2 - t;
            }
            else if (dy < 0)
            {
                t = 4 + t;
            }
            return t * 90;
        }

    }
}