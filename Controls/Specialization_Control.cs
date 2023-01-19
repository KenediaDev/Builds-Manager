namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
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

    public class Specialization_Control : Control
    {
        private const int _FrameWidth = 1;
        private const int _LineThickness = 5;
        public const int _Width = 643;
        public const int _Height = 133;
        private const int _HighlightLeft = 570;
        private const int _TraitSize = 38;
        private int _SpecSelectSize = 64;
        private const int _MiddleRowTop = (_Height - _TraitSize) / 2;

        private double _Scale = 1;
        public bool SelectorActive;
        public bool Elite;

        public double Scale
        {
            get => this._Scale;
            set
            {
                this._Scale = value;

                foreach (Trait_Control trait in this._MajorTraits)
                {
                    trait.Scale = value;
                }

                foreach (Trait_Control trait in this._MinorTraits)
                {
                    trait.Scale = value;
                }

                // Width = (int)(_Width * value);
                // Height = (int)(_Height * value);
                this.Location = this.DefaultLocation.Scale(value);
                this.Size = new Point(_Width, _Height).Scale(value);

                this.UpdateLayout();
            }
        }

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public int Index;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => this.Template != null ? this.Template.Build.SpecLines[this.Index].Specialization : null;
        }

        public EventHandler Changed;

        private Texture2D _SpecSideSelector_Hovered;
        private Texture2D _SpecSideSelector;
        private Texture2D _EliteFrame;
        private Texture2D _SpecHighlightFrame;
        private Texture2D _SpecFrame;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Line;

        private Point DefaultLocation;
        private Rectangle AbsoluteBounds;
        private Rectangle ContentBounds;
        private Rectangle HighlightBounds;
        private Rectangle SelectorBounds;
        private Rectangle SpecSelectorBounds;
        private Rectangle WeaponTraitBounds;
        private ConnectorLine FirstLine = new ConnectorLine();
        private CustomTooltip CustomTooltip;
        private SpecializationSelector_Control Selector;

        public List<Trait_Control> _MinorTraits = new List<Trait_Control>();
        public List<Trait_Control> _MajorTraits = new List<Trait_Control>();
        private bool _Created;
        private bool SpecHovered;

        public Specialization_Control(Container parent, int index, Point p, CustomTooltip customTooltip)
        {
            this.Parent = parent;
            this.CustomTooltip = customTooltip;
            this.Index = index;
            this.Size = new Point(_Width, _Height);
            this.DefaultLocation = p;
            this.Location = p;

            // BackgroundColor = Color.Magenta;

            this._SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector_Hovered);
            this._SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector);

            this._EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.EliteFrame).GetRegion(0, 4, 625, 130);
            this._SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecHighlight).GetRegion(12, 5, 103, 116);
            this._SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.SpecFrame).GetRegion(0, 0, 647, 136);
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            this._MinorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(this.Parent, new Point(215, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MinorTraits[0] : null, this.CustomTooltip, this) {ZIndex = this.ZIndex + 1, TraitIndex = 0, SpecIndex = this.Index, TraitType = API.traitType.Minor},
                    new Trait_Control(this.Parent, new Point(360, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MinorTraits[1]: null, this.CustomTooltip, this) {ZIndex = this.ZIndex + 1, TraitIndex = 1, SpecIndex = this.Index, TraitType = API.traitType.Minor},
                    new Trait_Control(this.Parent, new Point(505, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MinorTraits[2]: null, this.CustomTooltip, this) {ZIndex = this.ZIndex + 1, TraitIndex = 2, SpecIndex = this.Index, TraitType = API.traitType.Minor},
                };
            this._MajorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(this.Parent, new Point(285, ((133 - 38) / 2) - 3 - 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[0]: null, this.CustomTooltip, this) {ZIndex = this.ZIndex + 1, TraitIndex = 0, SpecIndex = this.Index, TraitType = API.traitType.Major },
                    new Trait_Control(this.Parent, new Point(285, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[1]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 1, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(285, ((133 - 38) / 2) + 3 + 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[2]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1 , TraitIndex = 2, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(430, ((133 - 38) / 2) - 3 - 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[3]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 3, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(430, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[4]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 4, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(430, ((133 - 38) / 2) + 3 + 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[5]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 5, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(575, ((133 - 38) / 2) - 3 - 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[6]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 6, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(575, (133 - 38) / 2).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[7]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 7, SpecIndex = this.Index, TraitType = API.traitType.Major},
                    new Trait_Control(this.Parent, new Point(575, ((133 - 38) / 2) + 3 + 38).Add(this.Location), this.Specialization != null ? this.Specialization.MajorTraits[8]: null, this.CustomTooltip, this) {ZIndex= this.ZIndex + 1, TraitIndex = 8, SpecIndex = this.Index, TraitType = API.traitType.Major},
                };

            var TemplateSpecLine = this.Template.Build.SpecLines.Find(e => e.Specialization == this.Specialization);

            foreach (Trait_Control trait in this._MajorTraits)
            {
                trait.Click += this.Trait_Click;
            }

            this.Selector = new SpecializationSelector_Control()
            {
                Index = this.Index,
                Specialization_Control = this,
                Parent = this.Parent,
                Visible = false,
                ZIndex = this.ZIndex + 2,
                Elite = this.Elite,
            };

            this._Created = true;
            this.UpdateLayout();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);
            this.UpdateLayout();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.UpdateLayout();
        }

        private void Trait_Click(object sender, MouseEventArgs e)
        {
            this.UpdateLayout();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (this.MouseOver)
            {
                var highlight = this.HighlightBounds.Add(new Point(-this.Location.X, -this.Location.Y)).Contains(this.RelativeMousePosition);
                var selector = this.SelectorBounds.Add(new Point(-this.Location.X, -this.Location.Y)).Contains(this.RelativeMousePosition);

                if (selector || highlight)
                {
                    if (this.Selector.Visible)
                    {
                        this.Selector.Visible = false;
                    }
                    else
                    {
                        this.Selector.Visible = true;

                        this.Selector.Location = this.LocalBounds.Location.Add(new Point(this.SelectorBounds.Width, 0));
                        this.Selector.Size = this.LocalBounds.Size.Add(new Point(-this.SelectorBounds.Width, 0));

                        this.Selector.Elite = this.Elite;
                        this.Selector.Specialization = this.Specialization;
                    }
                }
            }
        }

        public void UpdateLayout()
        {
            if (this._Created)
            {
                this.AbsoluteBounds = new Rectangle(0, 0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2)).Scale(this.Scale).Add(this.Location);

                this.ContentBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Scale(this.Scale).Add(this.Location);
                this.SelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, 15, _Height).Scale(this.Scale).Add(this.Location);

                this.HighlightBounds = new Rectangle(_Width - _HighlightLeft, (_Height - this._SpecHighlightFrame.Height) / 2, this._SpecHighlightFrame.Width, this._SpecHighlightFrame.Height).Scale(this.Scale).Add(this.Location);
                this.SpecSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Scale(this.Scale).Add(this.Location);

                this.FirstLine.Bounds = new Rectangle(this.HighlightBounds.Right - 5.Scale(this._Scale), this.HighlightBounds.Center.Y, 225 - this.HighlightBounds.Right, _LineThickness.Scale(this._Scale));
                this.WeaponTraitBounds = new Rectangle(this.HighlightBounds.Right - _TraitSize - 6, _Height + this.HighlightBounds.Height - 165, _TraitSize, _TraitSize).Scale(this.Scale).Add(this.Location);

                this.SpecHovered = this.HighlightBounds.Add(new Point(-this.Location.X, -this.Location.Y)).Contains(this.RelativeMousePosition);
                if (this.SpecHovered)
                {
                    this.BasicTooltipText = this.Specialization?.Name;
                }
                else
                {
                    this.BasicTooltipText = null;
                }

                foreach (Trait_Control trait in this._MajorTraits)
                {
                    if (trait.Selected)
                    {
                        var minor = this._MinorTraits[trait.Trait.Tier - 1];
                        float rotation = 0f;
                        switch (trait.Trait.Order)
                        {
                            case 0:
                                rotation = -(float)(Math.PI / 5.65);
                                break;

                            case 1:
                                rotation = 0f;
                                break;

                            case 2:
                                rotation = (float)(Math.PI / 5.65);
                                break;
                        }

                        trait.PreLine.Rotation = rotation;
                        trait.PostLine.Rotation = -rotation;

                        var minor_Pos = minor.LocalBounds.Center;
                        var majorPos = trait.LocalBounds.Center;
                        trait.PreLine.Bounds = new Rectangle(minor_Pos.X, minor_Pos.Y, minor.AbsoluteBounds.Center.Distance2D(trait.AbsoluteBounds.Center), _LineThickness.Scale(this._Scale));

                        if (trait.Selected && trait.Trait.Tier != 3)
                        {
                            minor = this._MinorTraits[trait.Trait.Tier];
                            trait.PostLine.Bounds = new Rectangle(majorPos.X, majorPos.Y, trait.AbsoluteBounds.Center.Distance2D(minor.AbsoluteBounds.Center), _LineThickness.Scale(this._Scale));
                        }
                    }
                    else
                    {
                        trait.PreLine = new ConnectorLine();
                        trait.PostLine = new ConnectorLine();
                    }
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            this.UpdateLayout();

            spriteBatch.DrawOnCtrl(
                this.Parent,
                ContentService.Textures.Pixel,
                this.AbsoluteBounds,
                this.AbsoluteBounds,
                Color.Black,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                this.Parent,
                this._EmptyTraitLine,
                this.ContentBounds,
                this._EmptyTraitLine.Bounds,
                new Color(135, 135, 135, 255),
                0f,
                Vector2.Zero);

            if (this.Specialization != null)
            {
                // Background
                spriteBatch.DrawOnCtrl(
                    this.Parent,
                    this.Specialization.Background._AsyncTexture,
                    this.ContentBounds,
                    this.Specialization.Background._AsyncTexture.Texture.Bounds,
                    Color.White,
                    0f,
                    Vector2.Zero);

                // Lines
                if (this.FirstLine.Bounds != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this.Parent,
                        this._Line,
                        this.FirstLine.Bounds,
                        this._Line.Bounds,
                        Color.White,
                        this.FirstLine.Rotation,
                        Vector2.Zero);

                }

                foreach (Trait_Control trait in this._MajorTraits)
                {
                    if (trait.PreLine.Bounds != null)
                    {
                        // ScreenNotification.ShowNotification(trait.Trait.Name, ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(
                            this.Parent,
                            this._Line,
                            trait.PreLine.Bounds,
                            this._Line.Bounds,
                            Color.White,
                            trait.PreLine.Rotation,
                            Vector2.Zero);

                    }

                    if (trait.PostLine.Bounds != null)
                    {
                        // ScreenNotification.ShowNotification("PostLine", ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(
                            this.Parent,
                            this._Line,
                            trait.PostLine.Bounds,
                            this._Line.Bounds,
                            Color.White,
                            trait.PostLine.Rotation,
                            Vector2.Zero);

                    }
                }
            }

            spriteBatch.DrawOnCtrl(
                this.Parent,
                this._SpecFrame,
                this.ContentBounds,
                this._SpecFrame.Bounds,
                Color.Black,
                0f,
                Vector2.Zero);

            // Spec Highlighter
            spriteBatch.DrawOnCtrl(
                this.Parent,
                this._SpecHighlightFrame,
                this.HighlightBounds,
                this._SpecHighlightFrame.Bounds,
                this.Specialization != null ? Color.White : new Color(32, 32, 32, 125),
                0f,
                Vector2.Zero);

            if (this.Elite)
            {
                spriteBatch.DrawOnCtrl(
                    this.Parent,
                    this._EliteFrame,
                    this.ContentBounds,
                    this._EliteFrame.Bounds,
                    Color.White,
                    0f,
                    Vector2.Zero);

                if (this.Specialization != null && this.Specialization.WeaponTrait != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this.Parent,
                        this.Specialization.WeaponTrait.Icon._AsyncTexture,
                        this.WeaponTraitBounds,
                        this.Specialization.WeaponTrait.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        Vector2.Zero);
                }
            }

            if (this.Selector.Visible)
            {
                spriteBatch.DrawOnCtrl(
                    this.Parent,
                    ContentService.Textures.Pixel,
                    this.SelectorBounds,
                    this._SpecSideSelector.Bounds,
                    new Color(0, 0, 0, 205),
                    0f,
                    Vector2.Zero);
            }

            spriteBatch.DrawOnCtrl(
                this.Parent,
                this.SelectorBounds.Add(new Point(-this.Location.X, -this.Location.Y)).Contains(this.RelativeMousePosition) ? this._SpecSideSelector_Hovered : this._SpecSideSelector,
                this.SelectorBounds,
                this._SpecSideSelector.Bounds,
                Color.White,
                0f,
                Vector2.Zero);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Trait_Control trait in this._MajorTraits)
            {
                trait.Click -= this.Trait_Click;
            }

            this._MajorTraits?.DisposeAll();
            this._MajorTraits.Clear();

            this._MinorTraits?.DisposeAll();
            this._MinorTraits.Clear();

            this.Selector?.Dispose();
            this._Specialization?.Dispose();

            // Specialization?.Dispose();

            this._SpecSideSelector_Hovered = null;
            this._SpecSideSelector = null;
            this._EliteFrame = null;
            this._SpecHighlightFrame = null;
            this._SpecFrame = null;
            this._EmptyTraitLine = null;
            this._Line = null;
        }
    }
}