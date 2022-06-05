using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.BuildsManager
{
    internal static class StatAttributeExtension
    {
        public static string getLocalName(this API.StatAttribute attribute)
        {
            string text = attribute.Name;

            switch (attribute.Id)
            {
                case (int)_Stats.Power:
                    return Strings.common.Power;

                case (int)_Stats.Precision:
                    return Strings.common.Precision;

                case (int)_Stats.Toughness:
                    return Strings.common.Toughness;

                case (int)_Stats.Vitality:
                    return Strings.common.Vitality;

                case (int)_Stats.Ferocity:
                    return Strings.common.Ferocity;

                case (int)_Stats.HealingPower:
                    return Strings.common.HealingPower;

                case (int)_Stats.ConditionDamage:
                    return Strings.common.ConditionDamage;

                case (int)_Stats.Concentration:
                    return Strings.common.Concentration;

                case (int)_Stats.Expertise:
                    return Strings.common.Expertise;
            }

            return text;
        }
    }


    internal static class weaponTypeExtension
    {
        public static string getLocalName(this API.weaponType weaponType)
        {
            var text = weaponType.ToString();

            switch (weaponType)
            {
                case API.weaponType.Axe:
                    return Strings.common.Axe;
                case API.weaponType.Dagger:
                    return Strings.common.Dagger;
                case API.weaponType.Mace:
                    return Strings.common.Mace;
                case API.weaponType.Pistol:
                    return Strings.common.Pistol;
                case API.weaponType.Scepter:
                    return Strings.common.Scepter;
                case API.weaponType.Sword:
                    return Strings.common.Sword;
                case API.weaponType.Focus:
                    return Strings.common.Focus;
                case API.weaponType.Shield:
                    return Strings.common.Shield;
                case API.weaponType.Torch:
                    return Strings.common.Torch;
                case API.weaponType.Warhorn:
                    return Strings.common.Warhorn;
                case API.weaponType.Greatsword:
                    return Strings.common.Greatsword;
                case API.weaponType.Hammer:
                    return Strings.common.Hammer;
                case API.weaponType.Longbow:
                //case API.weaponType.LongBow:
                    return Strings.common.Longbow;
                case API.weaponType.Rifle:
                    return Strings.common.Rifle;
                case API.weaponType.Shortbow:
                //case API.weaponType.ShortBow:
                    return Strings.common.Shortbow;
                case API.weaponType.Staff:
                    return Strings.common.Staff;
                case API.weaponType.Spear:
                //case API.weaponType.Harpoon:
                    return Strings.common.Spear;
                case API.weaponType.Speargun:
                    return Strings.common.Speargun;
                case API.weaponType.Trident:
                    return Strings.common.Trident;
            }

            return text;
        }
    }

    internal static class DisposableExtensions
    {
        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (var d in disposables)
                d?.Dispose();
        }
    }

    public static class ClassExtensions
    {
        public static Rectangle CalculateTextRectangle(this Rectangle rect, string text, BitmapFont font)
        {
            int rows = 1;
            int width = 0;
            var placeholder = font.GetCharacterRegion(' ');

            foreach (char c in text)
            {
                var region = font.GetCharacterRegion(c);

                if (region != null && width + region.Width > rect.Width)
                {
                    rows++;
                    width = 0;
                }

                width += region != null ? region.Width : placeholder.Width;
            }

            return new Rectangle(rect.Location, new Point(rect.Width, rows * font.LineHeight));
        }
        public static Rectangle CalculateTextRectangle(this BitmapFont font, string text, Rectangle rect)
        {
            int rows = 1;
            int width = 0;
            var placeholder = font.GetCharacterRegion(' ');

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    rows++;
                    width = 0;
                    continue;
                }

                var region = font.GetCharacterRegion(c);
                var cWidth = region != null ? region.Width : placeholder.Width;

                if (width + cWidth > rect.Width)
                {
                    rows++;
                    width = 0;
                }

                width += cWidth;
            }

            return new Rectangle(rect.Location, new Point(rect.Width, rows * font.LineHeight));
        }

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
            return new Point((int)(p.X * factor), (int)(p.Y * factor));
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
