using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.BuildsManager
{
    public static class ClassExtensions
    {  
        public static int Distance2D(this Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }
        public static int Distance2D_Center(this Rectangle p1, Rectangle p2)
        {
            return (int)Math.Sqrt(Math.Pow((p2.Left - p1.Right), 2) + Math.Pow(((p2.Top + (p2.Height / 2)) - (p1.Top + (p1.Height / 2))), 2));
        }
        public static int Distance2D_Middle(this Rectangle p1, Rectangle p2)
        {
            var pp1 = new Point(p1.Left + (p1.Width / 2), p1.Top + (p1.Height / 2));
            var pp2 = new Point(p2.Left + (p2.Width / 2), p2.Top + (p2.Height / 2));

            return (int)Math.Sqrt(Math.Pow((p2.X - pp2.X), 2) + Math.Pow((pp2.Y - pp1.Y), 2));
        }

        public static int Scale(this int i, double factor)
        {
            return (int)(i * factor);
        }
        
        public static Point Scale(this Point p, double factor)
        {
            return new Point((int) (p.X * factor), (int) (p.Y * factor));
        }

        public static Rectangle Scale(this Rectangle b, double factor)
        {
            return new Rectangle((int)(b.X * factor), (int)(b.Y * factor), (int)(b.Width * factor), (int)(b.Height * factor));
        }
        public static Rectangle Add(this Rectangle b, Point p)
        {
            return new Rectangle(b.X + p.X, b.Y + p.Y, b.Width, b.Height);
        }
        public static Point Add(this Point b, Point p)
        {
            return new Point(b.X + p.X, b.Y + p.Y);
        }
    }
}
