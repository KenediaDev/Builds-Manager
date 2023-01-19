namespace Kenedia.Modules.BuildsManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class Control_TemplateTooltip : Control
    {
        public Control_TemplateTooltip()
        {
            this.Parent = GameService.Graphics.SpriteScreen;

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
            this.Template = template;
        }
    }

    public class Control_TemplateEntry : Control
    {
        public Template Template;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Lock;
        private Texture2D _Template_Border;
        private BitmapFont Font;
        private BitmapFont FontItalic;
        private Control_TemplateTooltip TemplateTooltip;
        private double Tick = 0;
        private string FeedbackPopup;

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (this.FeedbackPopup != null)
            {
                this.Tick += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (this.Tick < 350)
                {
                    // Fadeout
                }
                else
                {
                    // Hide
                    this.Tick = 0;
                    this.FeedbackPopup = null;
                }
            }
        }

        public Control_TemplateEntry(Container parent, Template template)
        {
            this.Parent = parent;
            this.Template = template;
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Template_Border = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Template_Border);
            this._Lock = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Lock_Locked);

            this.Font = GameService.Content.DefaultFont14;
            this.FontItalic = GameService.Content.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Italic);

            // TemplateTooltip = new Control_TemplateTooltip();
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;

        private void OnTemplateChangedEvent(Template template)
        {
            var code = template.Build.ParseBuildCode();
            if (code != null && code != string.Empty && Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Ctrl))
            {
                try
                {
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(template.Build.ParseBuildCode());
                    this.FeedbackPopup = "Copied Build Code!";
                }
                catch (ArgumentException)
                {
                    ScreenNotification.ShowNotification("Failed to set the clipboard text!", ScreenNotification.NotificationType.Error);
                }
                catch
                {
                }

                return;
            }

            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(template));
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            this.OnTemplateChangedEvent(this.Template);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.TemplateTooltip?.Dispose();

            // Template?.Dispose();
            this.Template = null;
            this._EmptyTraitLine = null;
            this._Lock = null;
            this._Template_Border = null;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.TemplateTooltip != null)
            {
                this.TemplateTooltip.Visible = this.MouseOver;
            }

            spriteBatch.DrawOnCtrl(
                this,
                this._Template_Border,
                bounds,
                this._Template_Border.Bounds,
                this.MouseOver ? Color.Gray : Color.Gray,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                BuildsManager.ModuleInstance.Selected_Template == this.Template ? new Color(0, 0, 0, 200) : new Color(0, 0, 0, 145),
                0f,
                Vector2.Zero);

            Texture2D texture = this._EmptyTraitLine;
            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (this.Template.Specialization != null)
            {
                texture = this.Template.Specialization.ProfessionIconBig._AsyncTexture;
            }
            else if (this.Template.Build.Profession != null)
            {
                texture = this.Template.Build.Profession.IconBig._AsyncTexture;
            }

            spriteBatch.DrawOnCtrl(
                this,
                texture,
                new Rectangle(2, 2, bounds.Height - 4, bounds.Height - 4),
                texture.Bounds,
                this.Template.Profession?.Id == player.Profession.ToString() ? Color.White : Color.LightGray,
                0f,
                Vector2.Zero);

            if (this.Template.Path == null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this._Lock,
                    new Rectangle(bounds.Height - 14, bounds.Height - 14, 12, 12),
                    this._Lock.Bounds,
                    new Color(168 + 15, 143 + 15, 102 + 15, 255),
                    0f,
                    Vector2.Zero);
            }

            var textBounds = new Rectangle(bounds.X + bounds.Height + 5, bounds.Y, bounds.Width - (bounds.Height + 5), bounds.Height);
            var popupBounds = new Rectangle(bounds.X + bounds.Height - 10, bounds.Y, bounds.Width - (bounds.Height + 5), bounds.Height);
            var rect = this.Font.CalculateTextRectangle(this.Template.Name, textBounds);

            if (this.FeedbackPopup != null)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    this.FeedbackPopup,
                    this.FontItalic,
                    popupBounds,
                    new Color(175, 175, 175, 125),
                    false,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Middle);
            }
            else
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    this.Template.Name,
                    this.Font,
                    textBounds,
                    BuildsManager.ModuleInstance.Selected_Template == this.Template ? Color.LimeGreen : this.MouseOver ? Color.White : this.Template.Profession?.Id == player.Profession.ToString() ? Color.LightGray : Color.Gray,
                    true,
                    HorizontalAlignment.Left,
                    rect.Height > textBounds.Height ? VerticalAlignment.Top : VerticalAlignment.Middle);
            }

            // var color = MouseOver ? Color.Honeydew : Color.Black;
            var color = this.MouseOver ? Color.Honeydew : Color.Transparent;

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

        }
    }

    public class ProfessionSelection : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Profession = null;
            }
        }

        public API.Profession Profession;
        public Rectangle Bounds;
        public bool Hovered;
        public int Index;
    }

    public class Control_ProfessionSelector : Control
    {
        public List<API.Profession> Professions = new List<API.Profession>();
        public List<ProfessionSelection> _Professions;
        public Texture2D ClearTexture;
        public int IconSize;

        public Control_ProfessionSelector()
        {
            // BackgroundColor = Color.Orange;
            this._Professions = new List<ProfessionSelection>();

            for (int i = 0; i < BuildsManager.ModuleInstance.Data.Professions.Count; i++)
            {
                var profession = BuildsManager.ModuleInstance.Data.Professions[i];
                this._Professions.Add(new ProfessionSelection()
                {
                    Profession = profession,
                    Index = i,
                });
            }

            this._Professions.Add(new ProfessionSelection()
            {
                Index = this._Professions.Count,
            });

            this.IconSize = this.Height - 4;
            this.ClearTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Clear);
            this.UpdateLayout();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.IconSize = this.Height - 4;
            this.UpdateLayout();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (ProfessionSelection profession in this._Professions)
            {
                if (profession.Hovered)
                {
                    if (profession.Profession == null)
                    {
                        this.Professions.Clear();
                    }
                    else if (this.Professions.Contains(profession.Profession))
                    {
                        this.Professions.Remove(profession.Profession);
                    }
                    else
                    {
                        this.Professions.Add(profession.Profession);
                    }

                    this.OnChanged(null, EventArgs.Empty);
                }
            }
        }

        private void UpdateLayout()
        {
            foreach (ProfessionSelection profession in this._Professions)
            {
                profession.Bounds = new Rectangle(2 + (profession.Index * (this.IconSize + 2)), 2, this.IconSize, this.IconSize);
            }
        }

        public event EventHandler Changed;

        private void OnChanged(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(0, 0, 0, 145),
                0f,
                Vector2.Zero);

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

            foreach (ProfessionSelection profession in this._Professions)
            {
                profession.Hovered = profession.Bounds.Contains(this.RelativeMousePosition);

                if (profession.Profession != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        profession.Profession.Icon._AsyncTexture,
                        profession.Bounds,
                        profession.Profession.Icon._AsyncTexture.Texture.Bounds,
                        profession.Hovered ? Color.White : this.Professions.Contains(profession.Profession) ? Color.LightGray : new Color(48, 48, 48, 150),
                        0f,
                        Vector2.Zero);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        this.ClearTexture,
                        profession.Bounds,
                        this.ClearTexture.Bounds,
                        profession.Hovered ? Color.White : this.Professions.Count > 0 ? Color.LightGray : new Color(48, 48, 48, 150),
                        0f,
                        Vector2.Zero);
                }
            }

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.Professions?.Clear();
            foreach (ProfessionSelection p in this._Professions) { p.Dispose(); }
            this._Professions?.Clear();
            this.ClearTexture = null;
        }
    }

    public class Control_TemplateSelection : FlowPanel
    {
        private bool ResizeChilds = false;
        public TextBox FilterBox;
        public FlowPanel ContentPanel;
        private Control_ProfessionSelector _ProfessionSelector;
        private List<Control_TemplateEntry> Templates = new List<Control_TemplateEntry>();

        public Control_TemplateSelection(Container parent)
        {
            this.Parent = parent;
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.ControlPadding = new Vector2(0, 3);
            this.FilterBox = new TextBox()
            {
                Location = new Point(5, 0),
                Parent = this,
                Width = this.Width - 5,
                PlaceholderText = Strings.common.Search + " ...",
            };

            this._ProfessionSelector = new Control_ProfessionSelector()
            {
                Parent = this,
                Size = new Point(this.Width - 5, this.FilterBox.Height),
                Location = new Point(5, this.FilterBox.Bottom + 5),
            };
            this.ContentPanel = new FlowPanel()
            {
                Parent = this,
                Size = new Point(this.Width, this.Height - this.AbsoluteBounds.Y - 5),
                Location = new Point(5, this._ProfessionSelector.Bottom + 5),
                CanScroll = true,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            this.Location = new Point(this.FilterBox.LocalBounds.Left, this._ProfessionSelector.LocalBounds.Bottom + 5);
            this.Size = new Point(255, parent.Height - (this.AbsoluteBounds.Y - 5));

            // BackgroundColor = Color.Magenta;
            this.Refresh();
            this.FilterBox.TextChanged += this.FilterBox_TextChanged;
            this._ProfessionSelector.Changed += this._ProfessionSelector_Changed;

            BuildsManager.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Templates_Loaded += this.ModuleInstance_Templates_Loaded;
            BuildsManager.ModuleInstance.Template_Deleted += this.ModuleInstance_Template_Deleted;
            this.ContentPanel.ChildAdded += this.ContentPanel_ChildsChanged;
            this.ContentPanel.ChildRemoved += this.ContentPanel_ChildsChanged;
        }

        private void _ProfessionSelector_Changed(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        private void ModuleInstance_Template_Deleted(object sender, EventArgs e)
        {
            this.Refresh();
        }

        public void SetSelection()
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;
            this._ProfessionSelector.Professions.Clear();
            this._ProfessionSelector.Professions.Add(BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == player.Profession.ToString()));
            this.RefreshList();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void ContentPanel_ChildsChanged(object sender, ChildChangedEventArgs e)
        {
            this.ResizeChilds = true;
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (this.ResizeChilds)
            {
                var not_fitting = this.ContentPanel.Height < (this.Templates.Where(e => e.Visible).ToList().Count * 38);
                foreach (Control_TemplateEntry template in this.Templates)
                {
                    template.Width = not_fitting ? this.Width - 20 : this.Width - 5;
                }

                this.ResizeChilds = false;
            }
        }

        private void ModuleInstance_Templates_Loaded(object sender, EventArgs e)
        {
            this.Refresh();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

        }

        public void RefreshList()
        {
            this.ContentPanel.SuspendLayout();
            var filter = this.FilterBox.Text.ToLower();
            var prof = BuildsManager.ModuleInstance.CurrentProfession;

            this.ContentPanel.SortChildren<Control_TemplateEntry>((a, b) =>
            {
                var ret = (b.Template.Build.Profession == prof).CompareTo(a.Template.Build.Profession == prof);
                if (ret == 0)
                {
                    ret = a.Template.Build.Profession.Id.CompareTo(b.Template.Build.Profession.Id);
                }

                if (ret == 0 && a.Template.Specialization != null)
                {
                    ret = a.Template.Specialization.Id.CompareTo(b.Template.Specialization?.Id);
                }

                if (ret == 0)
                {
                    ret = a.Template.Name.CompareTo(b.Template.Name);
                }

                return ret;
            });

            foreach (Control_TemplateEntry template in this.Templates)
            {
                if (template.Template != null)
                {
                    var name = template.Template.Name.ToLower();

                    if ((this._ProfessionSelector.Professions.Count == 0 || this._ProfessionSelector.Professions.Contains(template.Template.Profession)) && name.Contains(filter))
                    {
                        template.Show();
                    }
                    else
                    {
                        template.Hide();
                    }
                }
            }

            this.ResizeChilds = true;
            this.ContentPanel.Invalidate();
            this.ContentPanel.ResumeLayout();
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            this.RefreshList();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.FilterBox.Width = this.Width - 5;
            this._ProfessionSelector.Width = this.Width - 5;
            this.ContentPanel.Size = new Point(this.Width, this.Height - this.LocalBounds.Y);
        }

        public void Refresh()
        {
            this.SuspendLayout();
            this.ContentPanel.SuspendLayout();

            foreach (Control_TemplateEntry template in new List<Control_TemplateEntry>(this.Templates))
            {
                if (BuildsManager.ModuleInstance.Templates.Find(e => e == template.Template) == null)
                {
                    template.Dispose();
                    this.Templates.Remove(template);
                }
            }

            foreach (Template template in BuildsManager.ModuleInstance.Templates)
            {
                if (this.Templates.Find(e => e.Template == template) == null)
                {
                    var ctrl = new Control_TemplateEntry(this.ContentPanel, template) { Size = new Point(this.Width - 20, 38) };
                    ctrl.TemplateChanged += this.OnTemplateChangedEvent;
                    this.Templates.Add(ctrl);

                    template.Deleted += this.Template_Deleted;
                }
            }

            this.ResumeLayout();
            this.ContentPanel.ResumeLayout();

            this.RefreshList();
        }

        private void Template_Deleted(object sender, EventArgs e)
        {
            var template = (Template)sender;
            var ctrl = this.Templates.Find(a => a.Template == template);
            ctrl?.Dispose();
            if (ctrl != null)
            {
                this.Templates.Remove(ctrl);
            }
        }

        public void Clear()
        {
            foreach (Control_TemplateEntry ctrl in this.Templates)
            {
                ctrl.Dispose();
            }

            this.Templates.Clear();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Template template in BuildsManager.ModuleInstance.Templates)
            {
                template.Deleted -= this.Template_Deleted;
            }

            foreach (Control_TemplateEntry template in this.Templates)
            {
                template.TemplateChanged -= this.OnTemplateChangedEvent;
            }

            this.Templates.DisposeAll();

            this.ContentPanel?.Dispose();
            this.FilterBox?.Dispose();
            this._ProfessionSelector?.Dispose();

            this.FilterBox.TextChanged -= this.FilterBox_TextChanged;
            this._ProfessionSelector.Changed -= this._ProfessionSelector_Changed;

            BuildsManager.ModuleInstance.LanguageChanged -= this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Templates_Loaded -= this.ModuleInstance_Templates_Loaded;
            BuildsManager.ModuleInstance.Template_Deleted -= this.ModuleInstance_Template_Deleted;
            this.ContentPanel.ChildAdded -= this.ContentPanel_ChildsChanged;
            this.ContentPanel.ChildRemoved -= this.ContentPanel_ChildsChanged;
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;

        private void OnTemplateChangedEvent(Object sender, TemplateChangedEvent e)
        {
            this.TemplateChanged?.Invoke(this, new TemplateChangedEvent(e.Template));
        }
    }
}
