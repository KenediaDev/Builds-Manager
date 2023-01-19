﻿using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Control_AddButton : Control
    {
        public Texture2D Texture;
        public Texture2D TextureHovered;
        private readonly Texture2D _EmptyTraitLine;
        private readonly Texture2D _Template_Border;
        public BitmapFont Font;

        private string _Text;

        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
            }
        }

        public Control_AddButton()
        {
            // BackgroundColor = Color.Red;
            Texture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Add);
            TextureHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Add_Hovered);
            _EmptyTraitLine = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Template_Border = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Template_Border);

            Font = GameService.Content.DefaultFont16;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(0, 0, 0, 125),
                0f,
                Vector2.Zero);

            Color color = Color.Black;
            Rectangle rect = bounds;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            bounds = bounds.Add(1, 1, -2, -2);

            spriteBatch.DrawOnCtrl(
                this,
                _EmptyTraitLine,
                bounds.Add(2, 2, -4, -6),
                _EmptyTraitLine.Bounds,
                MouseOver ? new Color(135, 135, 135, 255) : new Color(105, 105, 105, 255),
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                _Template_Border,
                bounds,
                _Template_Border.Bounds,
                MouseOver ? Color.Gray : Color.Black,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                MouseOver ? TextureHovered : Texture,
                new Rectangle(6, 6, Height - 12, Height - 12),
                MouseOver ? TextureHovered.Bounds : Texture.Bounds,
                Color.White,
                0f,
                Vector2.Zero);

            Rectangle textBounds = new(12 + (Height - 12), 6, Width - (12 + (Height - 12)), Height - 12);
            rect = Font.CalculateTextRectangle(Text, textBounds);

            spriteBatch.DrawStringOnCtrl(
                this,
                Text,
                Font,
                textBounds,
                MouseOver ? Color.White : Color.LightGray,
                false,
                rect.Height > textBounds.Height ? HorizontalAlignment.Left : HorizontalAlignment.Center);

        }
    }
}
