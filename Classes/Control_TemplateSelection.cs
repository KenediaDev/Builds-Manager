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
using MonoGame.Extended.BitmapFonts;
using System.Threading;
using System.IO;

namespace Kenedia.Modules.BuildsManager
{
    public class TemplateChangedEvent
    {
        public Template Template;
        public TemplateChangedEvent(Template template)
        {
            Template = template;
        }
    }
    public class Control_TemplateEntry : Control
    {
        public Template Template;
        public Control_Build Control_Build;
        private Texture2D _EmptyTraitLine;
        private BitmapFont Font;

        public Control_TemplateEntry (Container parent, Template template)
        {
            Parent = parent;
            Template = template;
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            var cnt = new ContentService();
            Font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)16, ContentService.FontStyle.Regular);            
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;
        private void OnTemplateChangedEvent(Template template)
        {
            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(template));
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            OnTemplateChangedEvent(Template);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Control_Build?.Dispose();
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.DrawOnCtrl(this,
                                   _EmptyTraitLine,
                                   bounds,
                                   _EmptyTraitLine.Bounds,
                                    new Color(135, 135, 135, 255),
                                   0f,
                                   Vector2.Zero
                                   );

            spriteBatch.DrawStringOnCtrl(this,
                                    Template.Name,
                                    Font,
                                    bounds.Add(new Point(5,0)),
                                    Color.White,
                                    false,
                                    HorizontalAlignment.Left
                                    );

            var color = MouseOver ? Color.Honeydew : Color.Black;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

        }
    }

    public class Control_TemplateSelection : Control
    {
        private List<Control_TemplateEntry> Templates = new List<Control_TemplateEntry>();
        public Control_TemplateSelection(Container parent)
        {
            Parent = parent;
            //BackgroundColor = Color.Magenta;
            Refresh();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            Refresh(); 
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        public void Refresh()
        {
            foreach(Control_TemplateEntry template in Templates)
            {
                template.Dispose();
            }

            Templates = new List<Control_TemplateEntry>();
            var files = Directory.GetFiles(BuildsManager.Paths.builds, "*.json", SearchOption.AllDirectories);

            foreach(string path in files)
            {
                var ctrl = new Control_TemplateEntry(Parent, new Template(path)) { Size = new Point(Width, 50) };
                ctrl.TemplateChanged += OnTemplateChangedEvent;
                Templates.Add(ctrl);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Control_TemplateEntry template in Templates)
            {
                template.Dispose();
            }
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;
        private void OnTemplateChangedEvent(Object sender, TemplateChangedEvent e)
        {
            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(e.Template));
        }
    }
}
