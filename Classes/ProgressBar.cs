using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager
{
    public class ProgressBar : Panel
    {
        public double _Progress = 0.33;
        public double Progress { 
            get { return _Progress; }
            set { _Progress = value; UpdateLayout(); }
        }
        public string _Text;
        public string Text
        {
            get { return _Text; }
            set { _Text = value; UpdateLayout(); }
        }
        public Color Done_Color;
        public Color Bar_Color;
        public Image _BackgroundTexture;
        public Image _FilledTexture;
        public Label _Label;
        public ProgressContainer _Bar;
        public Panel _Bar_Done;

        public ProgressBar()
        {
            _BackgroundTexture = new Image()
            {
                Parent = this,
                Size = new Point(Size.X, Size.Y - 10),
                Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.GlidingFill_Gray),
            };

            _Bar = new ProgressContainer()
            {
                Parent = this,
                Size = Size,
                FrameColor = Color.DarkOrange,
            };

            _Bar_Done = new Panel()
            {
                Parent = this,
                Size = new Point((int) (Width * Progress), Height),
            };

            _FilledTexture = new Image()
            {
                Parent = _Bar_Done,
                Size = new Point(Size.X, Size.Y - 2),
                Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.GlidingFill),
            };

            _Label = new Label()
            {
                Parent = this,
                Size = Size,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Middle,
                Text = "Display Text",
                ShadowColor = Color.White,
                TextColor = Color.Black,
                ShowShadow = false,
            };
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            UpdateLayout(); 
        }

        void UpdateLayout()
        {
            _Bar.Size = new Point(Size.X, Size.Y - 2);
            _Label.Size = new Point(Size.X, Size.Y - 2);
            _BackgroundTexture.Size = new Point(Size.X, Size.Y - 3);
            _FilledTexture.Size = new Point(Size.X, Size.Y - 3);
            _Bar_Done.Size = new Point((int)(Width * Progress), Height);

            _Label.Text = Text;
        }
    }

    public class ProgressContainer : Container
    {
        const int PADDING = 2;
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
    }
}
