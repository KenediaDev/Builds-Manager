namespace Kenedia.Modules.BuildsManager.Extensions
{
    using System;
    using Point = Microsoft.Xna.Framework.Point;

    public static class PointExtension
    {
        public static Point Scale(this Point p, double factor)
        {
            return new Point((int)(p.X * factor), (int)(p.Y * factor));
        }

        public static int Distance2D(this Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        public static Point Add(this Point b, Point p)
        {
            return new Point(b.X + p.X, b.Y + p.Y);
        }
    }
}
