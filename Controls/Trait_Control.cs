namespace Kenedia.Modules.BuildsManager.Controls
{
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

    public class Trait_Control : Control
    {
        private double _Scale = 1;
        private int _LineThickness = 5;

        public double Scale
        {
            get => this._Scale;
            set
            {
                this._Scale = value;
                this.UpdateLayout();
            }
        }

        private const int _TraitSize = 38;
        private Texture2D _Line;

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public int Index;

        public int TraitIndex;
        public int SpecIndex;
        public API.traitType TraitType;

        private CustomTooltip CustomTooltip;
        private Specialization_Control Specialization_Control;

        public Trait_Control(Container parent, Point p, API.Trait trait, CustomTooltip customTooltip, Specialization_Control specialization_Control)
        {
            this.Parent = parent;
            this.CustomTooltip = customTooltip;
            this.Specialization_Control = specialization_Control;
            this.DefaultPoint = p;
            this.Location = p;
            this.DefaultBounds = new Rectangle(0, 0, _TraitSize, _TraitSize);
            this._Trait = trait;
            this.Size = new Point(_TraitSize, _TraitSize);
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            this.ClipsBounds = false;
            this.UpdateLayout();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (this.Trait != null)
            {
                this.CustomTooltip.Visible = true;

                if (this.CustomTooltip.CurrentObject != this)
                {
                    this.CustomTooltip.CurrentObject = this;
                    this.CustomTooltip.Header = this.Trait.Name;
                    this.CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                    this.CustomTooltip.TooltipContent = new List<string>() { this.Trait.Description == string.Empty ? "No Description in API" : this.Trait.Description };
                }
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            if (this.CustomTooltip.CurrentObject == this)
            {
                this.CustomTooltip.Visible = false;
            }
        }

        protected override void OnClick(MouseEventArgs mouse)
        {
            base.OnClick(mouse);

            if (this.Trait != null && this.Trait.Type == API.traitType.Major && this.Template != null && this.MouseOver)
            {
                if (this.Template.Build.SpecLines[this.Specialization_Control.Index].Traits.Contains(this.Trait))
                {
                    this.Template.Build.SpecLines[this.Specialization_Control.Index].Traits.Remove(this.Trait);
                }
                else
                {
                    foreach (API.Trait t in this.Template.Build.SpecLines[this.Specialization_Control.Index].Traits.Where(e => e.Tier == this.Trait.Tier).ToList())
                    {
                        this.Template.Build.SpecLines[this.Specialization_Control.Index].Traits.Remove(t);
                    }

                    this.Template.Build.SpecLines[this.Specialization_Control.Index].Traits.Add(this.Trait);
                }

                this.Template.SetChanged();
            }
        }

        private API.Trait _Trait;

        public API.Trait Trait
        {
            get => this.TraitType != null ? this.TraitType == API.traitType.Major ? this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MajorTraits[this.TraitIndex] : this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MinorTraits[this.TraitIndex] : null;
            set
            {
            }
        }

        public bool Hovered;

        public bool Selected
        {
            get
            {
                var trait = this.TraitType == API.traitType.Major ? this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MajorTraits[this.TraitIndex] : this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MinorTraits[this.TraitIndex];

                return this.Template != null && trait != null && (trait.Type == API.traitType.Minor || this.Template.Build.SpecLines[this.SpecIndex].Traits.Contains(trait));
            }
        }

        public Point DefaultPoint;
        public Point Point;

        public Rectangle Bounds;
        public Rectangle DefaultBounds;

        public ConnectorLine PreLine = new ConnectorLine();
        public ConnectorLine PostLine = new ConnectorLine();
        public EventHandler Changed;

        private List<Point> _MinorTraits = new List<Point>()
        {
            new Point(215, (133 - 38) / 2),
            new Point(360, (133 - 38) / 2),
            new Point(505, (133 - 38) / 2),
        };

        private void UpdateLayout()
        {
            this.Bounds = this.DefaultBounds.Scale(this.Scale);
            this.Location = this.DefaultPoint.Scale(this.Scale);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Template == null || this.Bounds == null)
            {
                return;
            }

            var trait = this.TraitType == API.traitType.Major ? this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MajorTraits[this.TraitIndex] : this.Template.Build.SpecLines[this.SpecIndex].Specialization?.MinorTraits[this.TraitIndex];

            if (trait != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    trait.Icon._AsyncTexture,
                    this.Bounds,
                    trait.Icon._AsyncTexture.Texture.Bounds,
                    this.Selected ? Color.White : (this.MouseOver ? Color.LightGray : Color.Gray),
                    0f,
                    default);

            }
        }

        public void SetTemplate()
        {
            var template = BuildsManager.ModuleInstance.Selected_Template;

        }
    }
}