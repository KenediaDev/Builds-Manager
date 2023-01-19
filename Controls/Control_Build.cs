namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.BuildsManager.Enums;
    using Kenedia.Modules.BuildsManager.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class Control_Build : Control
    {
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        private Texture2D _Background;
        private Texture2D _SpecFrame;
        private Texture2D _SpecHighlightFrame;
        private Texture2D _PlaceHolderTexture;
        private Texture2D _EmptyTexture;
        private Texture2D _EmptyTraitLine;
        private Texture2D _EliteFrame;
        private Texture2D _Line;
        private Texture2D _SpecSideSelector;
        private Texture2D _SpecSideSelector_Hovered;

        private int _FrameWidth = 1;
        private int _LineThickness = 5;
        public int _Width = 643;
        public int _Height = 517 - 35;
        private int _HighlightLeft = 570;
        private int _TraitSize = 38;
        private int _SpecSelectSize = 64;
        private int _MiddleRowTop = (133 - 38) / 2; // (_Height - _TraitSize) / 2;
        private int FrameThickness = 1;
        private int Gap = 5;
        private int Build_Width = 900;
        private int Skillbar_Height = 130;
        private int Build_Height = 125 + (150 * 3);

        private double _Scale = 1;

        public double Scale
        {
            get => this._Scale;
            set
            {
                this._Scale = value;

                var p = new Point(this.Location.X, this.Location.Y);
                var s = new Point(this.Size.X, this.Size.Y);

                this.Size = new Point((int)(this._Width * this.Scale), (int)(this._Height * this.Scale));

                foreach (Specialization_Control spec in this.Specializations)
                {
                    spec.Scale = value;
                }

                this.SkillBar.Scale = value;

                this.UpdateLayout();
                this.OnResized(new ResizedEventArgs(p, s));
            }
        }

        private List<Specialization_Control> Specializations;
        public SkillBar_Control SkillBar;
        private CustomTooltip CustomTooltip;

        public Control_Build(Container parent)
        {
            this.Parent = parent;
            this.Size = new Point((int)(this._Width * this.Scale), (int)(this._Height * this.Scale));

            this.CustomTooltip = new CustomTooltip(this.Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            // BackgroundColor = Color.Honeydew;
            this.Click += this.OnClick;

            this._SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector_Hovered);
            this._SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector);

            this._EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.EliteFrame).GetRegion(0, 4, 625, 130);
            this._SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecHighlight).GetRegion(12, 5, 103, 116);
            this._SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecFrame).GetRegion(0, 0, 647, 136);
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            this._PlaceHolderTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)Icons.Refresh];
            this._EmptyTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)Icons.Bug];
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));
            this._Background = this._EmptyTraitLine;

            this.SkillBar = new SkillBar_Control(this.Parent)
            {
                _Location = new Point(0, 0),
                Size = new Point(this._Width, this.Skillbar_Height),
            };

            this.Specializations = new List<Specialization_Control>();
            for (int i = 0; i < this.Template.Build.SpecLines.Count; i++)
            {
                this.Specializations.Add(new Specialization_Control(this.Parent, i, new Point(0, 5 + this.Skillbar_Height + (i * 134)), this.CustomTooltip)
                {
                    ZIndex = this.ZIndex + 1,
                    Elite = i == 2,
                });
            }

            this.UpdateTemplate();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.Click -= this.OnClick;
            this.CustomTooltip?.Dispose();
        }

        public EventHandler Changed;

        private void OnChanged()
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnClick(object sender, MouseEventArgs m)
        {
        }

        private void UpdateTemplate()
        {
            for (int i = 0; i < this.Template.Build.SpecLines.Count; i++)
            {
                this.Template.Build.SpecLines[i].Control = this.Specializations[i];

                // SkillBar.SetTemplate();
            }
        }

        private void UpdateLayout()
        {
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        public void SetTemplate()
        {
            var template = BuildsManager.ModuleInstance.Selected_Template;

        }
    }
}