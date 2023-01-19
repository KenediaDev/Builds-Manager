using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Skill_Control : Control
    {
        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        private API.Skill _Skill;

        public API.Skill Skill
        {
            get => _Skill;
            set
            {
                _Skill = value;

            }
        }

        private readonly int _SkillSize = 55;
        public SkillSlots Slot;
        private Texture2D _SelectorTexture;
        private Texture2D _SelectorTextureHovered;
        private Texture2D _SkillPlaceHolder;
        public bool Aquatic;
        private readonly CustomTooltip CustomTooltip;

        private double _Scale = 1;

        public double Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Size = new Point(_SkillSize, _SkillSize + 15).Scale(value);
                Location = Location.Scale(value);
            }
        }

        public Skill_Control(Container parent)
        {
            Parent = parent;
            Size = new Point(_SkillSize, _SkillSize + 15);

            _SelectorTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SkillSelector).GetRegion(0, 2, 64, 12);
            _SelectorTextureHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SkillSelector_Hovered);
            _SkillPlaceHolder = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 128, 128);

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            // BackgroundColor = Color.OldLace;
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (Skill != null && Skill.Id > 0)
            {
                CustomTooltip.Visible = true;
                CustomTooltip.Header = Skill.Name;
                CustomTooltip.TooltipContent = new List<string>() { Skill.Description };
                CustomTooltip.CurrentObject = Skill;
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            CustomTooltip.Visible = false;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Rectangle skillRect = new Rectangle(new Point(0, 12), new Point(Width, Height - 12)).Scale(Scale);
            spriteBatch.DrawOnCtrl(
                this,
                MouseOver ? _SelectorTextureHovered : _SelectorTexture,
                new Rectangle(new Point(0, 0), new Point(Width, 12)).Scale(Scale),
                _SelectorTexture.Bounds,
                Color.White,
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Skill.Icon._AsyncTexture.Texture : _SkillPlaceHolder,
                skillRect,
                (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Skill.Icon._AsyncTexture.Texture.Bounds : _SkillPlaceHolder.Bounds,
                (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Color.White : new Color(0, 0, 0, 155),
                0f,
                default);

            if (MouseOver)
            {
                Color color = Color.Honeydew;

                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, skillRect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, skillRect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Bottom - 2, skillRect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Bottom - 1, skillRect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, 2, skillRect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, 1, skillRect.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Right - 2, skillRect.Top, 2, skillRect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Right - 1, skillRect.Top, 1, skillRect.Height), Rectangle.Empty, color * 0.6f);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _SelectorTexture = null;
            _SelectorTextureHovered = null;
            _SkillPlaceHolder = null;
        }
    }
}