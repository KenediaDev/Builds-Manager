namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.BuildsManager.Enums;
    using Kenedia.Modules.BuildsManager.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class SkillSelector_Control : Control
    {
        private class SelectionSkill
        {
            public API.Skill Skill;
            public Rectangle Bounds;
            public Rectangle SelectorBounds;
            public bool Hovered;
        }

        public Object currentObject;
        private CustomTooltip CustomTooltip;
        private Texture2D _NoWaterTexture;
        private int _SkillSize = 55;
        public Skill_Control Skill_Control;
        public List<API.Skill> _Skills = new List<API.Skill>();
        private List<SelectionSkill> _SelectionSkills = new List<SelectionSkill>();
        public bool Aquatic;

        public List<API.Skill> Skills
        {
            get => this._Skills;
            set
            {
                this._Skills = value;
                if (value != null)
                {
                    this._SelectionSkills = new List<SelectionSkill>();
                    foreach (API.Skill skill in value)
                    {
                        this._SelectionSkills.Add(new SelectionSkill()
                        {
                            Skill = skill,
                        });
                    }

                    this.UpdateLayout();
                }
            }
        }

        private BitmapFont Font;

        public event EventHandler<SkillChangedEvent> SkillChanged;

        private void OnSkillChanged(API.Skill skill, Skill_Control skill_Control)
        {
            this.SkillChanged?.Invoke(this, new SkillChangedEvent(skill, skill_Control));
        }

        public SkillSelector_Control()
        {
            this.CustomTooltip = new CustomTooltip(this.Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            this.Font = GameService.Content.DefaultFont18;
            this.Size = new Point(20 + (4 * this._SkillSize), this._SkillSize * (int)Math.Ceiling(this.Skills.Count / (double)4));
            this.ClipsBounds = false;

            this._NoWaterTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.NoWaterTexture).GetRegion(16, 16, 96, 96);

            Input.Mouse.LeftMouseButtonPressed += this.OnGlobalClick;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.CustomTooltip.Dispose();

            Input.Mouse.LeftMouseButtonPressed -= this.OnGlobalClick;
        }

        private void OnGlobalClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                foreach (SelectionSkill entry in this._SelectionSkills)
                {
                    if (entry.Hovered)
                    {
                        var noUnderwater = this.Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
                        if (!noUnderwater)
                        {
                            this.OnSkillChanged(entry.Skill, this.Skill_Control);
                            this.CustomTooltip.Hide();
                            this.Hide();
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateLayout()
        {
            this.Size = new Point(20 + (4 * this._SkillSize), 40 + (this._SkillSize * (int)Math.Ceiling(this.Skills.Count / (double)4)));
            var row = 0;
            var col = 0;

            var baseRect = new Rectangle(0, 0, this.Width, this.Height);
            foreach (SelectionSkill entry in this._SelectionSkills)
            {
                var rect = new Rectangle(10 + (col * this._SkillSize), 30 + (row * this._SkillSize), this._SkillSize, this._SkillSize);
                if (!baseRect.Contains(rect))
                {
                    col = 0;
                    row++;
                    rect = new Rectangle(10 + (col * this._SkillSize), 30 + (row * this._SkillSize), this._SkillSize, this._SkillSize);
                }

                entry.Bounds = rect;
                entry.Hovered = entry.Bounds.Contains(this.RelativeMousePosition);
                col++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            this.UpdateLayout();

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(0, 0, 0, 230),
                0f,
                default);

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

            if (this.Skill_Control != null)
            {
                var text = string.Empty;
                switch (this.Skill_Control.Slot)
                {
                    case SkillSlots.Heal:
                        text = "Healing Skills";
                        break;

                    case SkillSlots.Elite:
                        text = "Elite Skills";
                        break;

                    case SkillSlots.AquaticLegend1:
                    case SkillSlots.AquaticLegend2:
                    case SkillSlots.TerrestrialLegend1:
                    case SkillSlots.TerrestrialLegend2:
                        text = "Legends";
                        break;

                    default:
                        text = "Utility Skills";
                        break;
                }

                var sRect = this.Font.GetStringRectangle(text);
                spriteBatch.DrawStringOnCtrl(
                    this,
                    text,
                    this.Font,
                    new Rectangle((bounds.Width - (int)sRect.Width) / 2, 0, (int)sRect.Width, 20),
                    Color.White,
                    false,
                    HorizontalAlignment.Left);

                this.CustomTooltip.Visible = false;
                foreach (SelectionSkill entry in this._SelectionSkills)
                {
                    if (entry.Skill != null)
                    {
                        var noUnderwater = this.Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
                        spriteBatch.DrawOnCtrl(
                            this,
                            entry.Skill.Icon._AsyncTexture,
                            entry.Bounds,
                            entry.Skill.Icon._AsyncTexture.Texture.Bounds,
                            noUnderwater ? Color.Gray : Color.White,
                            0f,
                            default);

                        if (noUnderwater)
                        {
                            spriteBatch.DrawOnCtrl(
                                this,
                                this._NoWaterTexture,
                                entry.Bounds,
                                this._NoWaterTexture.Bounds,
                                Color.White,
                                0f,
                                default);
                        }

                        if (!noUnderwater && entry.Hovered)
                        {
                            color = Color.Honeydew;

                            // Top
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, entry.Bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, entry.Bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                            // Bottom
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Bottom - 2, entry.Bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Bottom - 1, entry.Bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                            // Left
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, 2, entry.Bounds.Height), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, 1, entry.Bounds.Height), Rectangle.Empty, color * 0.6f);

                            // Right
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Right - 2, entry.Bounds.Top, 2, entry.Bounds.Height), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Right - 1, entry.Bounds.Top, 1, entry.Bounds.Height), Rectangle.Empty, color * 0.6f);

                            this.CustomTooltip.CurrentObject = entry.Skill;
                            this.CustomTooltip.Header = entry.Skill.Name;
                            this.CustomTooltip.TooltipContent = new List<string>() { entry.Skill.Description };
                            this.CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                            this.CustomTooltip.Visible = true;
                        }
                    }
                }
            }
        }
    }
}