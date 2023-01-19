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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Control_TemplateEntry : Control
    {
        public Template Template;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Lock;
        private Texture2D _Template_Border;
        private readonly BitmapFont Font;
        private readonly BitmapFont FontItalic;
        private readonly Control_TemplateTooltip TemplateTooltip;
        private double Tick = 0;
        private string FeedbackPopup;

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (FeedbackPopup != null)
            {
                Tick += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (Tick < 350)
                {
                    // Fadeout
                }
                else
                {
                    // Hide
                    Tick = 0;
                    FeedbackPopup = null;
                }
            }
        }

        public Control_TemplateEntry(Container parent, Template template)
        {
            Parent = parent;
            Template = template;
            _EmptyTraitLine = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Template_Border = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Template_Border);
            _Lock = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Lock_Locked);

            Font = GameService.Content.DefaultFont14;
            FontItalic = GameService.Content.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Italic);

            // TemplateTooltip = new Control_TemplateTooltip();
        }

        public event EventHandler<TemplateChangedEvent> TemplateChanged;

        private void OnTemplateChangedEvent(Template template)
        {
            string code = template.Build.ParseBuildCode();
            if (code != null && code != string.Empty && Input.Keyboard.ActiveModifiers.HasFlag(Microsoft.Xna.Framework.Input.ModifierKeys.Ctrl))
            {
                try
                {
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(template.Build.ParseBuildCode());
                    FeedbackPopup = "Copied Build Code!";
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

            TemplateChanged?.Invoke(this, new TemplateChangedEvent(template));
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            OnTemplateChangedEvent(Template);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            TemplateTooltip?.Dispose();

            // Template?.Dispose();
            Template = null;
            _EmptyTraitLine = null;
            _Lock = null;
            _Template_Border = null;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplateTooltip != null)
            {
                TemplateTooltip.Visible = MouseOver;
            }

            spriteBatch.DrawOnCtrl(
                this,
                _Template_Border,
                bounds,
                _Template_Border.Bounds,
                MouseOver ? Color.Gray : Color.Gray,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                BuildsManager.s_moduleInstance.Selected_Template == Template ? new Color(0, 0, 0, 200) : new Color(0, 0, 0, 145),
                0f,
                Vector2.Zero);

            Texture2D texture = _EmptyTraitLine;
            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (Template.Specialization != null)
            {
                texture = Template.Specialization.ProfessionIconBig._AsyncTexture;
            }
            else if (Template.Build.Profession != null)
            {
                texture = Template.Build.Profession.IconBig._AsyncTexture;
            }

            spriteBatch.DrawOnCtrl(
                this,
                texture,
                new Rectangle(2, 2, bounds.Height - 4, bounds.Height - 4),
                texture.Bounds,
                Template.Profession?.Id == player.Profession.ToString() ? Color.White : Color.LightGray,
                0f,
                Vector2.Zero);

            if (Template.Path == null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _Lock,
                    new Rectangle(bounds.Height - 14, bounds.Height - 14, 12, 12),
                    _Lock.Bounds,
                    new Color(168 + 15, 143 + 15, 102 + 15, 255),
                    0f,
                    Vector2.Zero);
            }

            Rectangle textBounds = new(bounds.X + bounds.Height + 5, bounds.Y, bounds.Width - (bounds.Height + 5), bounds.Height);
            Rectangle popupBounds = new(bounds.X + bounds.Height - 10, bounds.Y, bounds.Width - (bounds.Height + 5), bounds.Height);
            Rectangle rect = Font.CalculateTextRectangle(Template.Name, textBounds);

            if (FeedbackPopup != null)
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    FeedbackPopup,
                    FontItalic,
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
                    Template.Name,
                    Font,
                    textBounds,
                    BuildsManager.s_moduleInstance.Selected_Template == Template ? Color.LimeGreen : MouseOver ? Color.White : Template.Profession?.Id == player.Profession.ToString() ? Color.LightGray : Color.Gray,
                    true,
                    HorizontalAlignment.Left,
                    rect.Height > textBounds.Height ? VerticalAlignment.Top : VerticalAlignment.Middle);
            }

            // var color = MouseOver ? Color.Honeydew : Color.Black;
            Color color = MouseOver ? Color.Honeydew : Color.Transparent;

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
