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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Control_ProfessionSelector : Control
    {
        public List<API.Profession> Professions = new();
        public List<ProfessionSelection> _Professions;
        public Texture2D ClearTexture;
        public int IconSize;

        public Control_ProfessionSelector()
        {
            // BackgroundColor = Color.Orange;
            _Professions = new List<ProfessionSelection>();

            for (int i = 0; i < BuildsManager.s_moduleInstance.Data.Professions.Count; i++)
            {
                API.Profession profession = BuildsManager.s_moduleInstance.Data.Professions[i];
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
            ClearTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Clear);
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

        private void UpdateLayout()
        {
            foreach (ProfessionSelection profession in _Professions)
            {
                profession.Bounds = new Rectangle(2 + (profession.Index * (IconSize + 2)), 2, IconSize, IconSize);
            }
        }

        public event EventHandler Changed;

        private void OnChanged(object sender, EventArgs e)
        {
            Changed?.Invoke(this, EventArgs.Empty);
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

            Color color = Color.Black;

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

            foreach (ProfessionSelection profession in _Professions)
            {
                profession.Hovered = profession.Bounds.Contains(RelativeMousePosition);

                if (profession.Profession != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        profession.Profession.Icon._AsyncTexture,
                        profession.Bounds,
                        profession.Profession.Icon._AsyncTexture.Texture.Bounds,
                        profession.Hovered ? Color.White : Professions.Contains(profession.Profession) ? Color.LightGray : new Color(48, 48, 48, 150),
                        0f,
                        Vector2.Zero);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ClearTexture,
                        profession.Bounds,
                        ClearTexture.Bounds,
                        profession.Hovered ? Color.White : Professions.Count > 0 ? Color.LightGray : new Color(48, 48, 48, 150),
                        0f,
                        Vector2.Zero);
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Professions?.Clear();
            foreach (ProfessionSelection p in _Professions) { p.Dispose(); }
            _Professions?.Clear();
            ClearTexture = null;
        }
    }
}
