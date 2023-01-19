using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Enums;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class ProgressBar : Panel
    {
        public double _Progress = 0.33;

        public double Progress
        {
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
                Texture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.GlidingFill_Gray),
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
                Size = new Point((int)(Width * Progress), Height),
            };

            _FilledTexture = new Image()
            {
                Parent = _Bar_Done,
                Size = new Point(Size.X, Size.Y - 2),
                Texture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.GlidingFill),
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

        private void UpdateLayout()
        {
            _Bar.Size = new Point(Size.X, Size.Y - 2);
            _Label.Size = new Point(Size.X, Size.Y - 2);
            _BackgroundTexture.Size = new Point(Size.X, Size.Y - 3);
            _FilledTexture.Size = new Point(Size.X, Size.Y - 3);
            _Bar_Done.Size = new Point((int)(Width * Progress), Height);

            _Label.Text = Text;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _Bar_Done.Dispose();
            _Bar.Dispose();
            _Label.Dispose();
            _FilledTexture.Dispose();
            _BackgroundTexture.Dispose();
        }
    }
}
