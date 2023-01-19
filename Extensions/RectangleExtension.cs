using System;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class RectangleExtension
    {
        public static int Distance2D_Center(this Rectangle p1, Rectangle p2)
        {
            return (int)Math.Sqrt(Math.Pow(p2.Left - p1.Right, 2) + Math.Pow(p2.Top + (p2.Height / 2) - (p1.Top + (p1.Height / 2)), 2));
        }

        public static int Distance2D_Middle(this Rectangle p1, Rectangle p2)
        {
            Point pp1 = new(p1.Left + (p1.Width / 2), p1.Top + (p1.Height / 2));
            Point pp2 = new(p2.Left + (p2.Width / 2), p2.Top + (p2.Height / 2));

            return (int)Math.Sqrt(Math.Pow(p2.X - pp2.X, 2) + Math.Pow(pp2.Y - pp1.Y, 2));
        }

        public static Rectangle Scale(this Rectangle b, double factor)
        {
            return new Rectangle((int)(b.X * factor), (int)(b.Y * factor), (int)(b.Width * factor), (int)(b.Height * factor));
        }

        public static Rectangle Add(this Rectangle b, Point p)
        {
            return new Rectangle(b.X + p.X, b.Y + p.Y, b.Width, b.Height);
        }
    }
}
