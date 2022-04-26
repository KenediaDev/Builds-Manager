using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.ChatLinks;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager
{
    public class Control_Build : Control
    {
        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    UpdateTemplate();
                }
            }
        }

        public double Scale;

        public Control_Build()
        {
            //BackgroundColor = Color.Honeydew;
            Click += OnClick;
        }

        public EventHandler Changed;
        private void OnChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnClick(object sender, MouseEventArgs m)
        {

        }

        private void UpdateTemplate()
        {

        }

        private void UpdateLayout()
        {

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_Template == null) return;
            UpdateLayout();

        }
    }
}