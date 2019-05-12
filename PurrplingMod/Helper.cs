using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace PurrplingMod
{
    public static class Helper
    {
        public static List<Point> NearPoints(Point p, int distance)
        {
            List<Point> points = new List<Point>();
            for (int x = p.X - distance; x <= p.X + distance; x++)
            {
                for (int y = p.Y - distance; y <= p.Y + distance; y++)
                {
                    if (x == p.X && y == p.Y)
                        continue;
                    points.Add(new Point(x, y));
                }
            }

            return points;
        }

        public static List<Point> SortPointsByNearest(List<Point> nearPoints, Point startTilePoint)
        {
            List<Tuple<Point, float>> nearPointsWithDistance = Helper.MapNearPointsWithDistance(nearPoints, startTilePoint);

            nearPointsWithDistance.Sort(delegate (Tuple<Point, float> p1, Tuple<Point, float> p2) {
                if (p1.Item2 == p2.Item2)
                {
                    return 0;
                }

                return -1 * p1.Item2.CompareTo(p2.Item2);
            });

            return nearPointsWithDistance.ConvertAll<Point>(
                new Converter<Tuple<Point, float>, Point>(
                    delegate (Tuple<Point, float> tp)
                    {
                        return tp.Item1;
                    }
                )
            );
        }

        public static float Distance(Point p1, Point p2)
        {
            return Utility.distance(p1.X, p2.X, p1.Y, p2.Y);
        }
        private static List<Tuple<Point, float>> MapNearPointsWithDistance(List<Point> nearPoints, Point startTilePoint)
        {
            List<Tuple<Point, float>> nearPointsWithDistance = new List<Tuple<Point, float>>();

            foreach (Point nearPoint in nearPoints)
            {
                nearPointsWithDistance.Add(new Tuple<Point, float>(nearPoint, Utility.distance(nearPoint.X, startTilePoint.X, nearPoint.Y, startTilePoint.Y)));
            }

            return nearPointsWithDistance;
        }
    }
}
