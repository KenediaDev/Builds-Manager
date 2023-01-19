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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillSelector_Control : Control
    {
        private class SelectionSkill
        {
            public API.Skill Skill;
            public Rectangle Bounds;
            public Rectangle SelectorBounds;
            public bool Hovered;
        }

        public object currentObject;
        private readonly CustomTooltip CustomTooltip;
        private readonly Texture2D _NoWaterTexture;
        private readonly int _SkillSize = 55;
        public Skill_Control Skill_Control;
        public List<API.Skill> _Skills = new();
        private List<SelectionSkill> _SelectionSkills = new();
        public bool Aquatic;

        public List<API.Skill> Skills
        {
            get => _Skills;
            set
            {
                _Skills = value;
                if (value != null)
                {
                    _SelectionSkills = new List<SelectionSkill>();
                    foreach (API.Skill skill in value)
                    {
                        _SelectionSkills.Add(new SelectionSkill()
                        {
                            Skill = skill,
                        });
                    }

                    UpdateLayout();
                }
            }
        }

        private readonly BitmapFont Font;

        public event EventHandler<SkillChangedEvent> SkillChanged;

        private void OnSkillChanged(API.Skill skill, Skill_Control skill_Control)
        {
            SkillChanged?.Invoke(this, new SkillChangedEvent(skill, skill_Control));
        }

        public SkillSelector_Control()
        {
            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            Font = GameService.Content.DefaultFont18;
            Size = new Point(20 + (4 * _SkillSize), _SkillSize * (int)Math.Ceiling(Skills.Count / (double)4));
            ClipsBounds = false;

            _NoWaterTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.NoWaterTexture).GetRegion(16, 16, 96, 96);

            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            CustomTooltip.Dispose();

            Input.Mouse.LeftMouseButtonPressed -= OnGlobalClick;
        }

        private void OnGlobalClick(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                foreach (SelectionSkill entry in _SelectionSkills)
                {
                    if (entry.Hovered)
                    {
                        bool noUnderwater = Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
                        if (!noUnderwater)
                        {
                            OnSkillChanged(entry.Skill, Skill_Control);
                            CustomTooltip.Hide();
                            Hide();
                            Thread.Sleep(100);
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateLayout()
        {
            Size = new Point(20 + (4 * _SkillSize), 40 + (_SkillSize * (int)Math.Ceiling(Skills.Count / (double)4)));
            int row = 0;
            int col = 0;

            Rectangle baseRect = new(0, 0, Width, Height);
            foreach (SelectionSkill entry in _SelectionSkills)
            {
                Rectangle rect = new(10 + (col * _SkillSize), 30 + (row * _SkillSize), _SkillSize, _SkillSize);
                if (!baseRect.Contains(rect))
                {
                    col = 0;
                    row++;
                    rect = new Rectangle(10 + (col * _SkillSize), 30 + (row * _SkillSize), _SkillSize, _SkillSize);
                }

                entry.Bounds = rect;
                entry.Hovered = entry.Bounds.Contains(RelativeMousePosition);
                col++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            UpdateLayout();

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(0, 0, 0, 230),
                0f,
                default);

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

            if (Skill_Control != null)
            {
                string text = Skill_Control.Slot switch
                {
                    SkillSlots.Heal => "Healing Skills",
                    SkillSlots.Elite => "Elite Skills",
                    SkillSlots.AquaticLegend1 or SkillSlots.AquaticLegend2 or SkillSlots.TerrestrialLegend1 or SkillSlots.TerrestrialLegend2 => "Legends",
                    _ => "Utility Skills",
                };
                MonoGame.Extended.RectangleF sRect = Font.GetStringRectangle(text);
                spriteBatch.DrawStringOnCtrl(
                    this,
                    text,
                    Font,
                    new Rectangle((bounds.Width - (int)sRect.Width) / 2, 0, (int)sRect.Width, 20),
                    Color.White,
                    false,
                    HorizontalAlignment.Left);

                CustomTooltip.Visible = false;
                foreach (SelectionSkill entry in _SelectionSkills)
                {
                    if (entry.Skill != null)
                    {
                        bool noUnderwater = Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
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
                                _NoWaterTexture,
                                entry.Bounds,
                                _NoWaterTexture.Bounds,
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

                            CustomTooltip.CurrentObject = entry.Skill;
                            CustomTooltip.Header = entry.Skill.Name;
                            CustomTooltip.TooltipContent = new List<string>() { entry.Skill.Description };
                            CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                            CustomTooltip.Visible = true;
                        }
                    }
                }
            }
        }
    }
}