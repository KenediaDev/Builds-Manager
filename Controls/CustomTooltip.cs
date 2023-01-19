using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class CustomTooltip : Control
    {
        private Texture2D Background;
        private int _Height;
        private object _CurrentObject;

        public object CurrentObject
        {
            get => _CurrentObject;
            set
            {
                _CurrentObject = value;
                _Height = -1;
            }
        }

        private string _Header;

        public string Header
        {
            get => _Header;
            set
            {
                _Header = value;
                _Height = -1;
            }
        }

        public Color HeaderColor = Color.Orange;
        public Color ContentColor = Color.Honeydew;
        private List<string> _Content;

        public List<string> TooltipContent
        {
            get => _Content;
            set
            {
                _Content = value;
                _Height = -1;
            }
        }

        public CustomTooltip(Container parent)
        {
            Parent = GameService.Graphics.SpriteScreen;

            Size = new Point(225, 275);
            Background = BuildsManager.s_moduleInstance.TextureManager._Backgrounds[(int)Backgrounds.Tooltip];
            ZIndex = 1000;
            Visible = false;

            Input.Mouse.MouseMoved += Mouse_MouseMoved;
        }

        private void Mouse_MouseMoved(object sender, MouseEventArgs e)
        {
            Location = Input.Mouse.Position.Add(new Point(20, -10));
        }

        private void UpdateLayout()
        {
            if (Header == null || TooltipContent == null)
            {
                return;
            }

            MonoGame.Extended.BitmapFonts.BitmapFont font = GameService.Content.DefaultFont14;
            MonoGame.Extended.BitmapFonts.BitmapFont headerFont = GameService.Content.DefaultFont18;

            MonoGame.Extended.RectangleF ItemNameSize = headerFont.GetStringRectangle(Header);

            int width = (int)ItemNameSize.Width + 30;

            // var height = 10 + (int)ItemNameSize.Height;
            int height = 0;

            List<string> newStrings = new();
            foreach (string s in TooltipContent)
            {
                string ss = s;

                ss = Regex.Replace(ss, "<c=@reminder>", "\n\n");
                ss = Regex.Replace(ss, "<c=@abilitytype>", string.Empty);
                ss = Regex.Replace(ss, "</c>", string.Empty);
                ss = Regex.Replace(ss, "<br>", string.Empty);
                ss = ss.Replace(Environment.NewLine, "\n");
                newStrings.Add(ss);

                MonoGame.Extended.RectangleF rect = font.GetStringRectangle(ss);
                width = Math.Max(width, Math.Min((int)rect.Width + 20, 300));
            }

            foreach (string s in newStrings)
            {
                Rectangle yRect = font.CalculateTextRectangle(s, new Rectangle(0, 0, width, 0));
                height += (int)yRect.Height;
            }

            TooltipContent = newStrings;
            Rectangle hRect = headerFont.CalculateTextRectangle(Header, new Rectangle(0, 0, width, 0));

            Height = 10 + hRect.Height + 15 + height + 5;
            Width = width + 20;
            _Height = Height;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Header == null || TooltipContent == null)
            {
                return;
            }

            if (_Height == -1)
            {
                UpdateLayout();
            }

            // UpdateLayout();
            MonoGame.Extended.BitmapFonts.BitmapFont font = GameService.Content.DefaultFont14;
            MonoGame.Extended.BitmapFonts.BitmapFont headerFont = GameService.Content.DefaultFont18;

            Rectangle hRect = headerFont.CalculateTextRectangle(Header, new Rectangle(0, 0, Width - 20, 0));

            MonoGame.Extended.RectangleF rect = font.GetStringRectangle(Header);

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(55, 55, 55, 255),
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                Background,
                bounds,
                bounds,
                Color.White,
                0f,
                default);

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            spriteBatch.DrawStringOnCtrl(
                this,
                Header,
                headerFont,
                new Rectangle(10, 10, 0, (int)rect.Height),
                HeaderColor,
                false,
                HorizontalAlignment.Left);

            spriteBatch.DrawStringOnCtrl(
                this,
                string.Join(Environment.NewLine, TooltipContent),
                font,
                new Rectangle(10, 10 + (int)rect.Height + 15, Width - 20, Height - (10 + (int)rect.Height + 15)),
                ContentColor,
                true,
                HorizontalAlignment.Left,
                VerticalAlignment.Top);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Background = null;
            Input.Mouse.MouseMoved -= Mouse_MouseMoved;
        }
    }
}
