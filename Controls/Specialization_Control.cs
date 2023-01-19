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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Specialization_Control : Control
    {
        private const int s_frameWidth = 1;
        private const int s_lineThickness = 5;
        private const int s_highlightLeft = 570;
        private const int s_traitSize = 38;
        private readonly int _middleRowTop;

        private double _scale = 1;
        public bool SelectorActive;
        public bool Elite;

        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;

                foreach (Trait_Control trait in MajorTraits)
                {
                    trait.Scale = value;
                }

                foreach (Trait_Control trait in MinorTraits)
                {
                    trait.Scale = value;
                }

                // Width = (int)(_Width * value);
                // Height = (int)(_Height * value);
                Location = _defaultLocation.Scale(value);
                Size = new Point(Width, Height).Scale(value);

                UpdateLayout();
            }
        }

        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        public int Index;

        public API.Specialization Specialization => Template?.Build.SpecLines[Index].Specialization;

        public EventHandler Changed;

        private Texture2D _specSideSelectorHovered;
        private Texture2D _specSideSelector;
        private Texture2D _eliteFrame;
        private Texture2D _specHighlightFrame;
        private Texture2D _specFrame;
        private Texture2D _emptyTraitLine;
        private Texture2D _line;

        private Point _defaultLocation;
        private Rectangle _absoluteContentBounds;
        private Rectangle _contentBounds;
        private Rectangle _highlightBounds;
        private Rectangle _selectorBounds;
        private Rectangle _specSelectorBounds;
        private Rectangle _weaponTraitBounds;
        private readonly ConnectorLine _firstLine = new();
        private readonly CustomTooltip _customTooltip;
        private readonly SpecializationSelector_Control _selector;

        public List<Trait_Control> MinorTraits = new();
        public List<Trait_Control> MajorTraits = new();
        private readonly bool _created;
        private bool _specHovered;

        public Specialization_Control(Container parent, int index, Point p, CustomTooltip customTooltip)
        {
            Width = 643;
            Height = 133;
            _middleRowTop = (Height - s_traitSize) / 2;

            Parent = parent;
            _customTooltip = customTooltip;
            Index = index;
            Size = new Point(Width, Height);
            _defaultLocation = p;
            Location = p;

            // BackgroundColor = Color.Magenta;

            _specSideSelectorHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector_Hovered);
            _specSideSelector = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecSideSelector);

            _eliteFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.EliteFrame).GetRegion(0, 4, 625, 130);
            _specHighlightFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecHighlight).GetRegion(12, 5, 103, 116);
            _specFrame = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.SpecFrame).GetRegion(0, 0, 647, 136);
            _emptyTraitLine = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _line = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            MinorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(215, (133 - 38) / 2).Add(Location), Specialization?.MinorTraits[0], _customTooltip, this) {ZIndex = ZIndex + 1, TraitIndex = 0, SpecIndex = Index, TraitType = API.traitType.Minor},
                    new Trait_Control(Parent, new Point(360, (133 - 38) / 2).Add(Location), Specialization?.MinorTraits[1], _customTooltip, this) {ZIndex = ZIndex + 1, TraitIndex = 1, SpecIndex = Index, TraitType = API.traitType.Minor},
                    new Trait_Control(Parent, new Point(505, (133 - 38) / 2).Add(Location), Specialization?.MinorTraits[2], _customTooltip, this) {ZIndex = ZIndex + 1, TraitIndex = 2, SpecIndex = Index, TraitType = API.traitType.Minor},
                };
            MajorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(285, ((133 - 38) / 2) - 3 - 38).Add(Location), Specialization?.MajorTraits[0], _customTooltip, this) {ZIndex = ZIndex + 1, TraitIndex = 0, SpecIndex = Index, TraitType = API.traitType.Major },
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2).Add(Location), Specialization?.MajorTraits[1], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 1, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(285, ((133 - 38) / 2) + 3 + 38).Add(Location), Specialization?.MajorTraits[2], _customTooltip, this) {ZIndex= ZIndex + 1 , TraitIndex = 2, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, ((133 - 38) / 2) - 3 - 38).Add(Location), Specialization?.MajorTraits[3], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 3, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2).Add(Location), Specialization?.MajorTraits[4], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 4, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, ((133 - 38) / 2) + 3 + 38).Add(Location), Specialization?.MajorTraits[5], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 5, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, ((133 - 38) / 2) - 3 - 38).Add(Location), Specialization?.MajorTraits[6], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 6, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2).Add(Location), Specialization?.MajorTraits[7], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 7, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, ((133 - 38) / 2) + 3 + 38).Add(Location), Specialization?.MajorTraits[8], _customTooltip, this) {ZIndex= ZIndex + 1, TraitIndex = 8, SpecIndex = Index, TraitType = API.traitType.Major},
                };

            SpecLine TemplateSpecLine = Template.Build.SpecLines.Find(e => e.Specialization == Specialization);

            foreach (Trait_Control trait in MajorTraits)
            {
                trait.Click += Trait_Click;
            }

            _selector = new SpecializationSelector_Control()
            {
                Index = Index,
                Specialization_Control = this,
                Parent = Parent,
                Visible = false,
                ZIndex = ZIndex + 2,
                Elite = Elite,
            };

            _created = true;
            UpdateLayout();
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);
            UpdateLayout();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            UpdateLayout();
        }

        private void Trait_Click(object sender, MouseEventArgs e)
        {
            UpdateLayout();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (MouseOver)
            {
                bool highlight = _highlightBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);
                bool selector = _selectorBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);

                if (selector || highlight)
                {
                    if (_selector.Visible)
                    {
                        _selector.Visible = false;
                    }
                    else
                    {
                        _selector.Visible = true;

                        _selector.Location = LocalBounds.Location.Add(new Point(_selectorBounds.Width, 0));
                        _selector.Size = LocalBounds.Size.Add(new Point(-_selectorBounds.Width, 0));

                        _selector.Elite = Elite;
                        _selector.Specialization = Specialization;
                    }
                }
            }
        }

        public void UpdateLayout()
        {
            if (_created)
            {
                _absoluteContentBounds = new Rectangle(0, 0, Width + (s_frameWidth * 2), Height + (s_frameWidth * 2)).Scale(Scale).Add(Location);

                _contentBounds = new Rectangle(s_frameWidth, s_frameWidth, Width, Height).Scale(Scale).Add(Location);
                _selectorBounds = new Rectangle(s_frameWidth, s_frameWidth, 15, Height).Scale(Scale).Add(Location);

                _highlightBounds = new Rectangle(Width - s_highlightLeft, (Height - _specHighlightFrame.Height) / 2, _specHighlightFrame.Width, _specHighlightFrame.Height).Scale(Scale).Add(Location);
                _specSelectorBounds = new Rectangle(s_frameWidth, s_frameWidth, Width, Height).Scale(Scale).Add(Location);

                _firstLine.Bounds = new Rectangle(_highlightBounds.Right - 5.Scale(_scale), _highlightBounds.Center.Y, 225 - _highlightBounds.Right, s_lineThickness.Scale(_scale));
                _weaponTraitBounds = new Rectangle(_highlightBounds.Right - s_traitSize - 6, Height + _highlightBounds.Height - 165, s_traitSize, s_traitSize).Scale(Scale).Add(Location);

                _specHovered = _highlightBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);
                if (_specHovered)
                {
                    BasicTooltipText = Specialization?.Name;
                }
                else
                {
                    BasicTooltipText = null;
                }

                foreach (Trait_Control trait in MajorTraits)
                {
                    if (trait.Selected)
                    {
                        Trait_Control minor = MinorTraits[trait.Trait.Tier - 1];
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

                        Point minor_Pos = minor.LocalBounds.Center;
                        Point majorPos = trait.LocalBounds.Center;
                        trait.PreLine.Bounds = new Rectangle(minor_Pos.X, minor_Pos.Y, minor.AbsoluteBounds.Center.Distance2D(trait.AbsoluteBounds.Center), s_lineThickness.Scale(_scale));

                        if (trait.Selected && trait.Trait.Tier != 3)
                        {
                            minor = MinorTraits[trait.Trait.Tier];
                            trait.PostLine.Bounds = new Rectangle(majorPos.X, majorPos.Y, trait.AbsoluteBounds.Center.Distance2D(minor.AbsoluteBounds.Center), s_lineThickness.Scale(_scale));
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
            UpdateLayout();

            spriteBatch.DrawOnCtrl(
                Parent,
                ContentService.Textures.Pixel,
                _absoluteContentBounds,
                _absoluteContentBounds,
                Color.Black,
                0f,
                Vector2.Zero);

            spriteBatch.DrawOnCtrl(
                Parent,
                _emptyTraitLine,
                _contentBounds,
                _emptyTraitLine.Bounds,
                new Color(135, 135, 135, 255),
                0f,
                Vector2.Zero);

            if (Specialization != null)
            {
                // Background
                spriteBatch.DrawOnCtrl(
                    Parent,
                    Specialization.Background._AsyncTexture,
                    _contentBounds,
                    Specialization.Background._AsyncTexture.Texture.Bounds,
                    Color.White,
                    0f,
                    Vector2.Zero);

                // Lines
                if (_firstLine.Bounds != null)
                {
                    spriteBatch.DrawOnCtrl(
                        Parent,
                        _line,
                        _firstLine.Bounds,
                        _line.Bounds,
                        Color.White,
                        _firstLine.Rotation,
                        Vector2.Zero);

                }

                foreach (Trait_Control trait in MajorTraits)
                {
                    if (trait.PreLine.Bounds != null)
                    {
                        // ScreenNotification.ShowNotification(trait.Trait.Name, ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(
                            Parent,
                            _line,
                            trait.PreLine.Bounds,
                            _line.Bounds,
                            Color.White,
                            trait.PreLine.Rotation,
                            Vector2.Zero);

                    }

                    if (trait.PostLine.Bounds != null)
                    {
                        // ScreenNotification.ShowNotification("PostLine", ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(
                            Parent,
                            _line,
                            trait.PostLine.Bounds,
                            _line.Bounds,
                            Color.White,
                            trait.PostLine.Rotation,
                            Vector2.Zero);

                    }
                }
            }

            spriteBatch.DrawOnCtrl(
                Parent,
                _specFrame,
                _contentBounds,
                _specFrame.Bounds,
                Color.Black,
                0f,
                Vector2.Zero);

            // Spec Highlighter
            spriteBatch.DrawOnCtrl(
                Parent,
                _specHighlightFrame,
                _highlightBounds,
                _specHighlightFrame.Bounds,
                Specialization != null ? Color.White : new Color(32, 32, 32, 125),
                0f,
                Vector2.Zero);

            if (Elite)
            {
                spriteBatch.DrawOnCtrl(
                    Parent,
                    _eliteFrame,
                    _contentBounds,
                    _eliteFrame.Bounds,
                    Color.White,
                    0f,
                    Vector2.Zero);

                if (Specialization != null && Specialization.WeaponTrait != null)
                {
                    spriteBatch.DrawOnCtrl(
                        Parent,
                        Specialization.WeaponTrait.Icon._AsyncTexture,
                        _weaponTraitBounds,
                        Specialization.WeaponTrait.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        Vector2.Zero);
                }
            }

            if (_selector.Visible)
            {
                spriteBatch.DrawOnCtrl(
                    Parent,
                    ContentService.Textures.Pixel,
                    _selectorBounds,
                    _specSideSelector.Bounds,
                    new Color(0, 0, 0, 205),
                    0f,
                    Vector2.Zero);
            }

            spriteBatch.DrawOnCtrl(
                Parent,
                _selectorBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition) ? _specSideSelectorHovered : _specSideSelector,
                _selectorBounds,
                _specSideSelector.Bounds,
                Color.White,
                0f,
                Vector2.Zero);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Trait_Control trait in MajorTraits)
            {
                trait.Click -= Trait_Click;
            }

            MajorTraits?.DisposeAll();
            MajorTraits.Clear();

            MinorTraits?.DisposeAll();
            MinorTraits.Clear();

            _selector?.Dispose();

            // Specialization?.Dispose();

            _specSideSelectorHovered = null;
            _specSideSelector = null;
            _eliteFrame = null;
            _specHighlightFrame = null;
            _specFrame = null;
            _emptyTraitLine = null;
            _line = null;
        }
    }
}