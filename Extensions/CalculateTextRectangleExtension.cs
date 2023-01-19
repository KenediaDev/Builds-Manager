namespace Kenedia.Modules.BuildsManager.Extensions
{
    using MonoGame.Extended.BitmapFonts;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public static class CalculateTextRectangleExtension
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
    }
}
