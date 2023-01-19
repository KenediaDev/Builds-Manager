namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.BuildsManager.Enums;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

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
            this.ClearTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Clear);
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
}
