namespace Kenedia.Modules.BuildsManager.Controls
{
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

    public class Skill_Control : Control
    {
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        private API.Skill _Skill;

        public API.Skill Skill
        {
            get => this._Skill;
            set
            {
                this._Skill = value;

            }
        }

        private int _SkillSize = 55;
        public SkillSlots Slot;
        private Texture2D _SelectorTexture;
        private Texture2D _SelectorTextureHovered;
        private Texture2D _SkillPlaceHolder;
        public bool Aquatic;
        private CustomTooltip CustomTooltip;

        private double _Scale = 1;

        public double Scale
        {
            get => this._Scale;
            set
            {
                this._Scale = value;
                this.Size = new Point(this._SkillSize, this._SkillSize + 15).Scale(value);
                this.Location = this.Location.Scale(value);
            }
        }

        public Skill_Control(Container parent)
        {
            this.Parent = parent;
            this.Size = new Point(this._SkillSize, this._SkillSize + 15);

            this._SelectorTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SkillSelector).GetRegion(0, 2, 64, 12);
            this._SelectorTextureHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SkillSelector_Hovered);
            this._SkillPlaceHolder = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 128, 128);

            this.CustomTooltip = new CustomTooltip(this.Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            // BackgroundColor = Color.OldLace;
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (this.Skill != null && this.Skill.Id > 0)
            {
                this.CustomTooltip.Visible = true;
                this.CustomTooltip.Header = this.Skill.Name;
                this.CustomTooltip.TooltipContent = new List<string>() { this.Skill.Description };
                this.CustomTooltip.CurrentObject = this.Skill;
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            this.CustomTooltip.Visible = false;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var skillRect = new Rectangle(new Point(0, 12), new Point(this.Width, this.Height - 12)).Scale(this.Scale);
            spriteBatch.DrawOnCtrl(
                this,
                this.MouseOver ? this._SelectorTextureHovered : this._SelectorTexture,
                new Rectangle(new Point(0, 0), new Point(this.Width, 12)).Scale(this.Scale),
                this._SelectorTexture.Bounds,
                Color.White,
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                (this.Skill != null && this.Skill.Icon != null && this.Skill.Icon._AsyncTexture != null) ? this.Skill.Icon._AsyncTexture.Texture : this._SkillPlaceHolder,
                skillRect,
                (this.Skill != null && this.Skill.Icon != null && this.Skill.Icon._AsyncTexture != null) ? this.Skill.Icon._AsyncTexture.Texture.Bounds : this._SkillPlaceHolder.Bounds,
                (this.Skill != null && this.Skill.Icon != null && this.Skill.Icon._AsyncTexture != null) ? Color.White : new Color(0, 0, 0, 155),
                0f,
                default);

            if (this.MouseOver)
            {
                var color = Color.Honeydew;

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
            this._SelectorTexture = null;
            this._SelectorTextureHovered = null;
            this._SkillPlaceHolder = null;
        }
    }
}