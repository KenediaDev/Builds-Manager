using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Trait_Control : Control
    {
        private double _scale = 1;

        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                UpdateLayout();
            }
        }

        private const int _TraitSize = 38;
        private readonly Texture2D _Line;

        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        public int Index;

        public int TraitIndex;
        public int SpecIndex;
        public API.traitType TraitType;

        private readonly CustomTooltip CustomTooltip;
        private readonly Specialization_Control Specialization_Control;

        public Trait_Control(Container parent, Point p, API.Trait trait, CustomTooltip customTooltip, Specialization_Control specialization_Control)
        {
            Parent = parent;
            CustomTooltip = customTooltip;
            Specialization_Control = specialization_Control;
            DefaultPoint = p;
            Location = p;
            DefaultBounds = new Rectangle(0, 0, _TraitSize, _TraitSize);
            _Trait = trait;
            Size = new Point(_TraitSize, _TraitSize);
            _Line = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            ClipsBounds = false;
            UpdateLayout();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (Trait != null)
            {
                CustomTooltip.Visible = true;

                if (CustomTooltip.CurrentObject != this)
                {
                    CustomTooltip.CurrentObject = this;
                    CustomTooltip.Header = Trait.Name;
                    CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                    CustomTooltip.TooltipContent = new List<string>() { Trait.Description == string.Empty ? "No Description in API" : Trait.Description };
                }
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            if (CustomTooltip.CurrentObject == this)
            {
                CustomTooltip.Visible = false;
            }
        }

        protected override void OnClick(MouseEventArgs mouse)
        {
            base.OnClick(mouse);

            if (Trait != null && Trait.Type == API.traitType.Major && Template != null && MouseOver)
            {
                if (Template.Build.SpecLines[Specialization_Control.Index].Traits.Contains(Trait))
                {
                    Template.Build.SpecLines[Specialization_Control.Index].Traits.Remove(Trait);
                }
                else
                {
                    foreach (API.Trait t in Template.Build.SpecLines[Specialization_Control.Index].Traits.Where(e => e.Tier == Trait.Tier).ToList())
                    {
                        Template.Build.SpecLines[Specialization_Control.Index].Traits.Remove(t);
                    }

                    Template.Build.SpecLines[Specialization_Control.Index].Traits.Add(Trait);
                }

                Template.SetChanged();
            }
        }

        private readonly API.Trait _Trait;

        public API.Trait Trait
        {
            get => TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex];
            set
            {
            }
        }

        public bool Hovered;

        public bool Selected
        {
            get
            {
                API.Trait trait = TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex];

                return Template != null && trait != null && (trait.Type == API.traitType.Minor || Template.Build.SpecLines[SpecIndex].Traits.Contains(trait));
            }
        }

        public Point DefaultPoint;
        public Point Point;

        public Rectangle Bounds;
        public Rectangle DefaultBounds;

        public ConnectorLine PreLine = new();
        public ConnectorLine PostLine = new();
        public EventHandler Changed;

        private readonly List<Point> _MinorTraits = new()
        {
            new Point(215, (133 - 38) / 2),
            new Point(360, (133 - 38) / 2),
            new Point(505, (133 - 38) / 2),
        };

        private void UpdateLayout()
        {
            Bounds = DefaultBounds.Scale(Scale);
            Location = DefaultPoint.Scale(Scale);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template == null || Bounds == null)
            {
                return;
            }

            API.Trait trait = TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex];

            if (trait != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    trait.Icon._AsyncTexture,
                    Bounds,
                    trait.Icon._AsyncTexture.Texture.Bounds,
                    Selected ? Color.White : (MouseOver ? Color.LightGray : Color.Gray),
                    0f,
                    default);

            }
        }

        public void SetTemplate()
        {

        }
    }
}