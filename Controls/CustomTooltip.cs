namespace Kenedia.Modules.BuildsManager.Controls
{
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

    public class CustomTooltip : Control
    {
        private Texture2D Background;
        private int _Height;
        private object _CurrentObject;

        public object CurrentObject
        {
            get => this._CurrentObject;
            set
            {
                this._CurrentObject = value;
                this._Height = -1;
            }
        }

        private string _Header;

        public string Header
        {
            get => this._Header;
            set
            {
                this._Header = value;
                this._Height = -1;
            }
        }

        public Color HeaderColor = Color.Orange;
        public Color ContentColor = Color.Honeydew;
        private List<string> _Content;

        public List<string> TooltipContent
        {
            get => this._Content;
            set
            {
                this._Content = value;
                this._Height = -1;
            }
        }

        public CustomTooltip(Container parent)
        {
            this.Parent = GameService.Graphics.SpriteScreen;

            this.Size = new Point(225, 275);
            this.Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)Backgrounds.Tooltip];
            this.ZIndex = 1000;
            this.Visible = false;

            Input.Mouse.MouseMoved += this.Mouse_MouseMoved;
        }

        private void Mouse_MouseMoved(object sender, MouseEventArgs e)
        {
            this.Location = Input.Mouse.Position.Add(new Point(20, -10));
        }

        private void UpdateLayout()
        {
            if (this.Header == null || this.TooltipContent == null)
            {
                return;
            }

            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var ItemNameSize = headerFont.GetStringRectangle(this.Header);

            var width = (int)ItemNameSize.Width + 30;

            // var height = 10 + (int)ItemNameSize.Height;
            var height = 0;

            List<string> newStrings = new List<string>();
            foreach (string s in this.TooltipContent)
            {
                var ss = s;

                ss = Regex.Replace(ss, "<c=@reminder>", "\n\n");
                ss = Regex.Replace(ss, "<c=@abilitytype>", string.Empty);
                ss = Regex.Replace(ss, "</c>", string.Empty);
                ss = Regex.Replace(ss, "<br>", string.Empty);
                ss = ss.Replace(Environment.NewLine, "\n");
                newStrings.Add(ss);

                var rect = font.GetStringRectangle(ss);
                width = Math.Max(width, Math.Min((int)rect.Width + 20, 300));
            }

            foreach (string s in newStrings)
            {
                var yRect = font.CalculateTextRectangle(s, new Rectangle(0, 0, width, 0));
                height += (int)yRect.Height;
            }

            this.TooltipContent = newStrings;
            var hRect = headerFont.CalculateTextRectangle(this.Header, new Rectangle(0, 0, width, 0));

            this.Height = 10 + hRect.Height + 15 + height + 5;
            this.Width = width + 20;
            this._Height = this.Height;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Header == null || this.TooltipContent == null)
            {
                return;
            }

            if (this._Height == -1)
            {
                this.UpdateLayout();
            }

            // UpdateLayout();
            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var hRect = headerFont.CalculateTextRectangle(this.Header, new Rectangle(0, 0, this.Width - 20, 0));

            var rect = font.GetStringRectangle(this.Header);

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
                this.Background,
                bounds,
                bounds,
                Color.White,
                0f,
                default);

            var color = Color.Black;

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
                this.Header,
                headerFont,
                new Rectangle(10, 10, 0, (int)rect.Height),
                this.HeaderColor,
                false,
                HorizontalAlignment.Left);

            spriteBatch.DrawStringOnCtrl(
                this,
                string.Join(Environment.NewLine, this.TooltipContent),
                font,
                new Rectangle(10, 10 + (int)rect.Height + 15, this.Width - 20, this.Height - (10 + (int)rect.Height + 15)),
                this.ContentColor,
                true,
                HorizontalAlignment.Left,
                VerticalAlignment.Top);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.Background = null;
            Input.Mouse.MouseMoved -= this.Mouse_MouseMoved;
        }
    }
}
