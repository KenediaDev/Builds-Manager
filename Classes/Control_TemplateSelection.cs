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
    public class Control_TemplateTooltip : Control
    {
        public Control_TemplateTooltip()
        {
            Parent = GameService.Graphics.SpriteScreen;


        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }
    }

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
        private Texture2D _Lock;
        private Texture2D _Template_Border;
        private BitmapFont Font;
        private Control_TemplateTooltip TemplateTooltip;

        public Control_TemplateEntry(Container parent, Template template)
        {
            Parent = parent;
            Template = template;
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Template_Border = BuildsManager.TextureManager.getControlTexture(_Controls.Template_Border);
            _Lock = BuildsManager.TextureManager.getIcon(_Icons.Lock_Locked);

            var cnt = new ContentService();
            Font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular);
            //TemplateTooltip = new Control_TemplateTooltip();
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;
        private void OnTemplateChangedEvent(Template template)
        {

            var code = template.Build.ParseBuildCode();
            if (code != null && code != "" && Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Ctrl))
            {
                try
                {
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(template.Build.ParseBuildCode());
                }
                catch (ArgumentException)
                {
                    ScreenNotification.ShowNotification("Failed to set the clipboard text!", ScreenNotification.NotificationType.Error);
                }
                catch
                {

                }
            }
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
            if (TemplateTooltip != null) TemplateTooltip.Visible = MouseOver;

            spriteBatch.DrawOnCtrl(this,
                                   _Template_Border,
                                   bounds,
                                   _Template_Border.Bounds,
                                   MouseOver ? Color.Gray : Color.Gray,
                                   0f,
                                   Vector2.Zero
                                   );

            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   bounds,
                                   bounds,
                                    BuildsManager.ModuleInstance.Selected_Template == Template ? new Color(0, 0, 0, 200) : new Color(0, 0, 0, 145),
                                   0f,
                                   Vector2.Zero
                                   );

            Texture2D texture = _EmptyTraitLine;

            if (Template.Specialization != null)
            {
                texture = Template.Specialization.ProfessionIconBig.Texture;
            }
            else if (Template.Build.Profession != null)
            {
                texture = Template.Build.Profession.IconBig.Texture;
            }

            spriteBatch.DrawOnCtrl(this,
                                   texture,
                                   new Rectangle(2, 2, bounds.Height - 4, bounds.Height - 4),
                                   texture.Bounds,
                                    Color.White,
                                   0f,
                                   Vector2.Zero
                                   );

            if (Template.Path == null)
            {
                spriteBatch.DrawOnCtrl(this,
                                       _Lock,
                                       new Rectangle(bounds.Height - 14, bounds.Height - 14, 12, 12),
                                       _Lock.Bounds,
                                        new Color(168 + 15, 143 + 15, 102 + 15, 255),
                                       0f,
                                       Vector2.Zero
                                       );
            }

            var textBounds = new Rectangle(bounds.X + bounds.Height + 5, bounds.Y, bounds.Width - (bounds.Height + 5), bounds.Height);
            var rect = Font.CalculateTextRectangle(Template.Name, textBounds);

            spriteBatch.DrawStringOnCtrl(this,
                                    Template.Name,
                                    Font,
                                    textBounds,
                                    BuildsManager.ModuleInstance.Selected_Template == Template ? Color.LimeGreen : MouseOver ? Color.White : Color.LightGray,
                                    true,
                                    HorizontalAlignment.Left,
                                    rect.Height > textBounds.Height ? VerticalAlignment.Top : VerticalAlignment.Middle
                                    );

            //var color = MouseOver ? Color.Honeydew : Color.Black;
            var color = MouseOver ? Color.Honeydew : Color.Transparent;

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

    public class Control_ProfessionSelector : Control
    {
        class ProfessionSelection
        {
            public API.Profession Profession;
            public Rectangle Bounds;
            public bool Hovered;
            public int Index;
        }

        public List<API.Profession> Professions = new List<API.Profession>();
        private List<ProfessionSelection> _Professions;
        public Texture2D ClearTexture;
        public int IconSize;
        public Control_ProfessionSelector()
        {
            //BackgroundColor = Color.Orange;
            _Professions = new List<ProfessionSelection>();

            for (int i = 0; i < BuildsManager.Data.Professions.Count; i++)
            {
                var profession = BuildsManager.Data.Professions[i];
                _Professions.Add(new ProfessionSelection()
                {
                    Profession = profession,
                    Index = i,
                });
            }

            _Professions.Add(new ProfessionSelection()
            {
                Index = _Professions.Count,
            });

            IconSize = Height - 4;
            ClearTexture = BuildsManager.TextureManager.getControlTexture(_Controls.Clear);
            UpdateLayout();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            IconSize = Height - 4;
            UpdateLayout();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (ProfessionSelection profession in _Professions)
            {
                if (profession.Hovered)
                {

                    if (profession.Profession == null)
                    {
                        Professions.Clear();
                    }
                    else if (Professions.Contains(profession.Profession))
                    {
                        Professions.Remove(profession.Profession);
                    }
                    else
                    {
                        Professions.Add(profession.Profession);
                    }

                    OnChanged(null, EventArgs.Empty);
                }
            }
        }

        void UpdateLayout()
        {
            foreach (ProfessionSelection profession in _Professions)
            {
                profession.Bounds = new Rectangle(2 + profession.Index * (IconSize + 2), 2, IconSize, IconSize);
            }
        }

        public EventHandler Changed;
        private void OnChanged(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   bounds,
                                   bounds,
                                   new Color(0, 0, 0, 145),
                                   0f,
                                   Vector2.Zero
                                   );

            var color = Color.Black;

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

            foreach (ProfessionSelection profession in _Professions)
            {
                profession.Hovered = profession.Bounds.Contains(RelativeMousePosition);

                if (profession.Profession != null)
                {

                    spriteBatch.DrawOnCtrl(this,
                                           profession.Profession.Icon.Texture,
                                           profession.Bounds,
                                           profession.Profession.Icon.Texture.Bounds,
                                           profession.Hovered ? Color.White : Professions.Contains(profession.Profession) ? Color.LightGray : new Color(48, 48, 48, 150),
                                           0f,
                                           Vector2.Zero
                                           );
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this,
                                           ClearTexture,
                                           profession.Bounds,
                                           ClearTexture.Bounds,
                                           profession.Hovered ? Color.White : Professions.Count > 0 ? Color.LightGray : new Color(48, 48, 48, 150),
                                           0f,
                                           Vector2.Zero
                                           );
                }
            }

        }
    }
    public class Control_TemplateSelection : FlowPanel
    {
        public TextBox FilterBox;
        public FlowPanel ContentPanel;
        private Control_ProfessionSelector _ProfessionSelector;
        private List<Control_TemplateEntry> Templates = new List<Control_TemplateEntry>();
        public Control_TemplateSelection(Container parent)
        {
            Parent = parent;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            ControlPadding = new Vector2(0, 3);
            FilterBox = new TextBox()
            {
                Location = new Point(5, 0),
                Parent = this,
                Width = Width - 5,
                PlaceholderText = Strings.common.Search + " ..."
            };
            BuildsManager.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;
            _ProfessionSelector = new Control_ProfessionSelector()
            {
                Parent = this,
                Size = new Point(Width - 5, FilterBox.Height),
                Location = new Point(5, FilterBox.Bottom + 5),
            };
            ContentPanel = new FlowPanel()
            {
                Parent = this,
                Size = new Point(Width, Height - AbsoluteBounds.Y - 5),
                Location = new Point(5, _ProfessionSelector.Bottom + 5),
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            Location = new Point(FilterBox.LocalBounds.Left, _ProfessionSelector.LocalBounds.Bottom + 5);
            Size = new Point(255, parent.Height - AbsoluteBounds.Y - 5);

            //BackgroundColor = Color.Magenta;
            Refresh();
            FilterBox.TextChanged += FilterBox_TextChanged;

            _ProfessionSelector.Changed += delegate
            {
                RefreshList();
            };

            BuildsManager.ModuleInstance.Templates_Loaded += ModuleInstance_Templates_Loaded;
            BuildsManager.ModuleInstance.Template_Deleted += ModuleInstance_Templates_Loaded;
            ContentPanel.ChildAdded += ContentPanel_ChildsChanged;
            ContentPanel.ChildRemoved += ContentPanel_ChildsChanged;
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void ContentPanel_ChildsChanged(object sender, ChildChangedEventArgs e)
        {
            var not_fitting = ContentPanel.Height < (Templates.Count * 38);

            foreach (Control_TemplateEntry template in Templates)
            {
                template.Width = not_fitting ? Width - 20 : Width - 5;
            }
        }

        private void ModuleInstance_Templates_Loaded(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

        }

        private void RefreshList()
        {
            var filter = FilterBox.Text.ToLower();

            foreach (Control_TemplateEntry template in Templates)
            {
                var name = template.Template.Name.ToLower();


                if ((_ProfessionSelector.Professions.Count == 0 || _ProfessionSelector.Professions.Contains(template.Template.Profession)) && name.Contains(filter))
                {
                    template.Show();
                }
                else
                {
                    template.Hide();
                }
            }

            ContentPanel.Invalidate();
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            FilterBox.Width = Width - 5;
            _ProfessionSelector.Width = Width - 5;
            ContentPanel.Size = new Point(Width, Height - AbsoluteBounds.Y - 5);
        }

        public void Refresh()
        {
            foreach (Control_TemplateEntry template in Templates)
            {
                template.Dispose();
            }

            Templates.Clear();

            Templates = new List<Control_TemplateEntry>();

            BuildsManager.ModuleInstance.Templates = BuildsManager.ModuleInstance.Templates.OrderBy(a => a.Profession.Id).ThenBy(b => b.Specialization?.Id).ThenBy(b => b.Name).ToList();
            foreach (Template template in BuildsManager.ModuleInstance.Templates)
            {
                var ctrl = new Control_TemplateEntry(ContentPanel, template) { Size = new Point(Width - 20, 38) };
                ctrl.TemplateChanged += OnTemplateChangedEvent;

                Templates.Add(ctrl);
            }

            ContentPanel_ChildsChanged(null, null);
        }

        protected override void DisposeControl()
        {
            foreach (Control_TemplateEntry template in Templates)
            {
                template.Dispose();
            }

            Templates.Clear();

            FilterBox.Dispose();
            _ProfessionSelector.Dispose();

            base.DisposeControl();
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;
        private void OnTemplateChangedEvent(Object sender, TemplateChangedEvent e)
        {
            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(e.Template));
        }
    }
}
