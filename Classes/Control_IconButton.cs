namespace Kenedia.Modules.BuildsManager
{
    using System;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class Control_AddButton : Control
    {
        public Texture2D Texture;
        public Texture2D TextureHovered;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Template_Border;
        public BitmapFont Font;

        private string _Text;

        public string Text
        {
            get => this._Text;
            set
            {
                this._Text = value;
            }
        }

        public Control_AddButton()
        {
            // BackgroundColor = Color.Red;
            this.Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Add);
            this.TextureHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Add_Hovered);
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Template_Border = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Template_Border);

            this.Font = GameService.Content.DefaultFont16;
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

            var color = Color.Black;
            var rect = bounds;

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
                this._EmptyTraitLine,
                bounds.Add(2, 2, -4, -6),
                this._EmptyTraitLine.Bounds,
                this.MouseOver ? new Color(135, 135, 135, 255) : new Color(105, 105, 105, 255),
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                this._Template_Border,
                bounds,
                this._Template_Border.Bounds,
                this.MouseOver ? Color.Gray : Color.Black,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                this.MouseOver ? this.TextureHovered : this.Texture,
                new Rectangle(6, 6, this.Height - 12, this.Height - 12),
                this.MouseOver ? this.TextureHovered.Bounds : this.Texture.Bounds,
                Color.White,
                0f,
                Vector2.Zero);

            var textBounds = new Rectangle(12 + (this.Height - 12), 6, this.Width - (12 + (this.Height - 12)), this.Height - 12);
            rect = this.Font.CalculateTextRectangle(this.Text, textBounds);

            spriteBatch.DrawStringOnCtrl(
                this,
                this.Text,
                this.Font,
                textBounds,
                this.MouseOver ? Color.White : Color.LightGray,
                false,
                rect.Height > textBounds.Height ? HorizontalAlignment.Left : HorizontalAlignment.Center);

        }
    }
}
