using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class ProgressContainer : Container
    {
        private const int PADDING = 2;
        public Texture2D Texture;
        public bool showBackground = true;
        public Color FrameColor = Color.Black;
        public Color TextureColor = Color.White;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (showBackground && Texture != null)
            {
                spriteBatch.DrawOnCtrl(this, Texture, bounds, new Rectangle(3, 4, _size.X, _size.Y), TextureColor * 0.98f);
            }

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, _size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, _size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), FrameColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 3, 1, 3, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 2, 1, 1, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 4, _size.X - 2, 4).Add(-PADDING, PADDING, PADDING * 2, 0), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 2, _size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0), FrameColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 3, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, 1, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.6f);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Texture = null;
        }
    }
}
