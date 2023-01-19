namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.BuildsManager.Enums;
    using Kenedia.Modules.BuildsManager.Extensions;
    using Kenedia.Modules.BuildsManager.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

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
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Template_Border = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Template_Border);
            this._Lock = BuildsManager.ModuleInstance.TextureManager.getIcon(Icons.Lock_Locked);

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
}
