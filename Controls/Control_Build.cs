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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Control_Build : Control
    {
        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        private readonly Texture2D _Background;
        private readonly Texture2D _SpecFrame;
        private readonly Texture2D _SpecHighlightFrame;
        private readonly Texture2D _PlaceHolderTexture;
        private readonly Texture2D _EmptyTexture;
        private readonly Texture2D _EmptyTraitLine;
        private readonly Texture2D _EliteFrame;
        private readonly Texture2D _Line;
        private readonly Texture2D _SpecSideSelector;
        private readonly Texture2D _SpecSideSelector_Hovered;

        private readonly int _FrameWidth = 1;
        private readonly int _LineThickness = 5;
        public int _Width = 643;
        public int _Height = 517 - 35;
        private readonly int _HighlightLeft = 570;
        private readonly int _TraitSize = 38;
        private readonly int _SpecSelectSize = 64;
        private readonly int _MiddleRowTop = (133 - 38) / 2; // (_Height - _TraitSize) / 2;
        private readonly int FrameThickness = 1;
        private readonly int Gap = 5;
        private readonly int Build_Width = 900;
        private readonly int Skillbar_Height = 130;
        private readonly int Build_Height = 125 + (150 * 3);

        private double _Scale = 1;

        public double Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;

                Point p = new(Location.X, Location.Y);
                Point s = new(Size.X, Size.Y);

                Size = new Point((int)(_Width * Scale), (int)(_Height * Scale));

                foreach (Specialization_Control spec in Specializations)
                {
                    spec.Scale = value;
                }

                SkillBar.Scale = value;

                UpdateLayout();
                OnResized(new ResizedEventArgs(p, s));
            }
        }

        private readonly List<Specialization_Control> Specializations;
        public SkillBar_Control SkillBar;
        private readonly CustomTooltip CustomTooltip;

        public Control_Build(Container parent)
        {
            Parent = parent;
            Size = new Point((int)(_Width * Scale), (int)(_Height * Scale));

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            // BackgroundColor = Color.Honeydew;
            Click += OnClick;

            _SpecSideSelector_Hovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector_Hovered);
            _SpecSideSelector = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector);

            _EliteFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.EliteFrame).GetRegion(0, 4, 625, 130);
            _SpecHighlightFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecHighlight).GetRegion(12, 5, 103, 116);
            _SpecFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecFrame).GetRegion(0, 0, 647, 136);
            _EmptyTraitLine = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            _PlaceHolderTexture = BuildsManager.s_moduleInstance.TextureManager._Icons[(int)Icons.Refresh];
            _EmptyTexture = BuildsManager.s_moduleInstance.TextureManager._Icons[(int)Icons.Bug];
            _Line = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));
            _Background = _EmptyTraitLine;

            SkillBar = new SkillBar_Control(Parent)
            {
                _Location = new Point(0, 0),
                Size = new Point(_Width, Skillbar_Height),
            };

            Specializations = new List<Specialization_Control>();
            for (int i = 0; i < Template.Build.SpecLines.Count; i++)
            {
                Specializations.Add(new Specialization_Control(Parent, i, new Point(0, 5 + Skillbar_Height + (i * 134)), CustomTooltip)
                {
                    ZIndex = ZIndex + 1,
                    Elite = i == 2,
                });
            }

            UpdateTemplate();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Click -= OnClick;
            CustomTooltip?.Dispose();
        }

        public EventHandler Changed;

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnClick(object sender, MouseEventArgs m)
        {
        }

        private void UpdateTemplate()
        {
            for (int i = 0; i < Template.Build.SpecLines.Count; i++)
            {
                Template.Build.SpecLines[i].Control = Specializations[i];

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
            Template template = BuildsManager.s_moduleInstance.Selected_Template;

        }
    }
}