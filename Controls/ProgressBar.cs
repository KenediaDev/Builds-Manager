namespace Kenedia.Modules.BuildsManager.Controls
{
    using Blish_HUD.Controls;
    using Kenedia.Modules.BuildsManager.Enums;
    using Microsoft.Xna.Framework;
    using Color = Microsoft.Xna.Framework.Color;

    public class ProgressBar : Panel
    {
        public double _Progress = 0.33;

        public double Progress
        {
            get { return this._Progress; }
            set { this._Progress = value; this.UpdateLayout(); }
        }

        public string _Text;

        public string Text
        {
            get { return this._Text; }
            set { this._Text = value; this.UpdateLayout(); }
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
            this._BackgroundTexture = new Image()
            {
                Parent = this,
                Size = new Point(this.Size.X, this.Size.Y - 10),
                Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.GlidingFill_Gray),
            };

            this._Bar = new ProgressContainer()
            {
                Parent = this,
                Size = this.Size,
                FrameColor = Color.DarkOrange,
            };

            this._Bar_Done = new Panel()
            {
                Parent = this,
                Size = new Point((int)(this.Width * this.Progress), this.Height),
            };

            this._FilledTexture = new Image()
            {
                Parent = this._Bar_Done,
                Size = new Point(this.Size.X, this.Size.Y - 2),
                Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.GlidingFill),
            };

            this._Label = new Label()
            {
                Parent = this,
                Size = this.Size,
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

            this.UpdateLayout();
        }

        private void UpdateLayout()
        {
            this._Bar.Size = new Point(this.Size.X, this.Size.Y - 2);
            this._Label.Size = new Point(this.Size.X, this.Size.Y - 2);
            this._BackgroundTexture.Size = new Point(this.Size.X, this.Size.Y - 3);
            this._FilledTexture.Size = new Point(this.Size.X, this.Size.Y - 3);
            this._Bar_Done.Size = new Point((int)(this.Width * this.Progress), this.Height);

            this._Label.Text = this.Text;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this._Bar_Done.Dispose();
            this._Bar.Dispose();
            this._Label.Dispose();
            this._FilledTexture.Dispose();
            this._BackgroundTexture.Dispose();
        }
    }
}
