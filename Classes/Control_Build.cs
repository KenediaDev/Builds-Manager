namespace Kenedia.Modules.BuildsManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class ConnectorLine
    {
        public Rectangle Bounds;
        public float Rotation = 0;
    }

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
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

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

    public class SpecializationSelector_Control : Control
    {
        public Specialization_Control Specialization_Control;
        public int Index;
        public bool Elite;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => this._Specialization;
            set
            {
                if (value != null)
                {
                    this._Specialization = value;
                }
            }
        }

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public SpecializationSelector_Control()
        {
            BuildsManager.ModuleInstance.Selected_Template_Changed += this.ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed += this.ClosePopUp;
        }

        private void ClosePopUp(object sender, EventArgs e)
        {
            if (!this.MouseOver)
            {
                this.Hide();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var i = 0;
            var size = 64;
            if (this.Template.Build.Profession != null)
            {
                foreach (API.Specialization spec in this.Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || this.Elite)
                    {
                        var rect = new Rectangle(20 + (i * size), (this.Height - size) / 2, size, size);

                        if (rect.Contains(this.RelativeMousePosition))
                        {
                            var sp = this.Template.Build.SpecLines.Find(x => x.Specialization != null && x.Specialization.Id == spec.Id);

                            if (sp != null && sp != this.Template.Build.SpecLines[this.Index])
                            {
                                // var traits = new List<API.Trait>(sp.Traits);
                                this.Template.Build.SpecLines[this.Index].Specialization = sp.Specialization;
                                this.Template.Build.SpecLines[this.Index].Traits = sp.Traits;

                                // Template.Build.SpecLines[Index].Control.UpdateLayout();
                                this.Template.SetChanged();

                                sp.Specialization = null;
                                sp.Traits = new List<API.Trait>();
                            }
                            else
                            {
                                if (this.Template.Build.SpecLines[this.Index] != null)
                                {
                                    foreach (SpecLine specLine in this.Template.Build.SpecLines)
                                    {
                                        if (spec != this.Specialization && specLine.Specialization == spec)
                                        {
                                            specLine.Specialization = null;
                                            specLine.Traits = new List<API.Trait>();
                                        }
                                    }

                                    if (this.Template.Build.SpecLines[this.Index].Specialization != spec)
                                    {
                                        this.Template.Build.SpecLines[this.Index].Specialization = spec;
                                        this.Template.SetChanged();
                                    }
                                }
                            }

                            this.Hide();
                            return;
                        }

                        i++;
                    }
                }
            }

            this.Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this.Parent,
                ContentService.Textures.Pixel,
                bounds.Add(this.Location),
                bounds,
                new Color(0, 0, 0, 205),
                0f,
                Vector2.Zero);

            if (this.Template.Build.Profession != null)
            {
                string text = string.Empty;

                var i = 0;
                var size = 64;
                foreach (API.Specialization spec in this.Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || this.Elite)
                    {
                        var rect = new Rectangle(20 + (i * size), (this.Height - size) / 2, size, size);
                        if (rect.Contains(this.RelativeMousePosition))
                        {
                            text = spec.Name;
                        }

                        spriteBatch.DrawOnCtrl(
                            this.Parent,
                            spec.Icon._AsyncTexture,
                            rect.Add(this.Location),
                            spec.Icon._AsyncTexture.Texture.Bounds,
                            this.Specialization == spec || rect.Contains(this.RelativeMousePosition) ? Color.White : Color.Gray,
                            0f,
                            Vector2.Zero);
                        i++;
                    }
                }

                if (text != string.Empty)
                {
                    this.BasicTooltipText = text;
                }
                else
                {
                    this.BasicTooltipText = null;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            BuildsManager.ModuleInstance.Selected_Template_Changed -= this.ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed -= this.ClosePopUp;
        }
    }

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

            this._SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            this._SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            this._EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            this._SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            this._SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

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

    public enum SkillSlots
    {
        Heal,
        Utility1,
        Utility2,
        Utility3,
        Elite,
        AquaticLegend1,
        AquaticLegend2,
        TerrestrialLegend1,
        TerrestrialLegend2,
    }

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

            this._SelectorTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SkillSelector).GetRegion(0, 2, 64, 12);
            this._SelectorTextureHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SkillSelector_Hovered);
            this._SkillPlaceHolder = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 128, 128);

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

    public class SkillChangedEvent
    {
        public API.Skill Skill;
        public Skill_Control Skill_Control;

        public SkillChangedEvent(API.Skill skill, Skill_Control skill_Control)
        {
            this.Skill = skill;
            this.Skill_Control = skill_Control;
        }
    }

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

            this._NoWaterTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.NoWaterTexture).GetRegion(16, 16, 96, 96);

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

    public class SkillBar_Control : Control
    {
        private CustomTooltip CustomTooltip;

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        private List<Skill_Control> _Legends_Aquatic;
        private List<Skill_Control> _Legends_Terrestrial;

        private List<Skill_Control> _Skills_Aquatic;
        private List<Skill_Control> _Skills_Terrestrial;

        private Texture2D _AquaTexture;
        private Texture2D _TerrestrialTexture;
        private Texture2D _SwapTexture;

        public SkillSelector_Control SkillSelector;
        public SkillSelector_Control LegendSelector;
        public SkillSelector_Control PetSelector;

        private bool CanClick = true;
        private bool ShowProfessionSkills = false;

        private double _Scale = 1;

        public double Scale
        {
            get => this._Scale;
            set
            {
                this._Scale = value;
                foreach (Skill_Control skill in this._Skills_Aquatic)
                {
                    skill.Scale = value;
                }

                foreach (Skill_Control skill in this._Skills_Terrestrial)
                {
                    skill.Scale = value;
                }
            }
        }

        private int _SkillSize = 55;
        public int _Width = 643;
        private Point _OGLocation;

        public Point _Location
        {
            get => this.Location;
            set
            {
                if (this.Location == null || this.Location == Point.Zero)
                {
                    this._OGLocation = value;
                }

                this.Location = value;
            }
        }

        public SkillBar_Control(Container parent)
        {
            this.Parent = parent;
            this.CustomTooltip = new CustomTooltip(this.Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            this._TerrestrialTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Land);
            this._AquaTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Water);
            this._SwapTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Refresh);

            var slots = Enum.GetValues(typeof(SkillSlots));

            // BackgroundColor = Color.Magenta;

            this._Skills_Aquatic = new List<Skill_Control>();
            foreach (API.Skill skill in this.Template.Build.Skills_Aquatic)
            {
                this._Skills_Aquatic.Add(new Skill_Control(this.Parent)
                {
                    Location = new Point(27 + (this._Skills_Aquatic.Count * (this._SkillSize + 1)), 51),
                    Skill = this.Template.Build.Skills_Aquatic[this._Skills_Aquatic.Count],
                    Slot = (SkillSlots)this._Skills_Aquatic.Count,
                    Aquatic = true,
                });

                var control = this._Skills_Aquatic[this._Skills_Aquatic.Count - 1];
                control.Click += this.Control_Click;
            }

            var p = this._Width - (this._Skills_Aquatic.Count * (this._SkillSize + 1));
            this._Skills_Terrestrial = new List<Skill_Control>();
            foreach (API.Skill skill in this.Template.Build.Skills_Terrestrial)
            {
                this._Skills_Terrestrial.Add(new Skill_Control(this.Parent)
                {
                    Location = new Point(p + (this._Skills_Terrestrial.Count * (this._SkillSize + 1)), 51),
                    Skill = this.Template.Build.Skills_Terrestrial[this._Skills_Terrestrial.Count],
                    Slot = (SkillSlots)this._Skills_Terrestrial.Count,
                });

                var control = this._Skills_Terrestrial[this._Skills_Terrestrial.Count - 1];
                control.Click += this.Control_Click;
            }

            this.SkillSelector = new SkillSelector_Control()
            {
                Parent = this.Parent,

                // Size  =  new Point(100, 250),
                Visible = false,
                ZIndex = this.ZIndex + 3,
            };

            this._Legends_Aquatic = new List<Skill_Control>();
            this._Legends_Aquatic.Add(new Skill_Control(this.Parent)
            {
                Skill = this.Template.Build.Legends_Aquatic[0].Skill,
                Slot = SkillSlots.AquaticLegend1,
                Aquatic = true,
                Scale = 0.8,
                Location = new Point(27, 0),
            });
            this._Legends_Aquatic.Add(new Skill_Control(this.Parent)
            {
                Skill = this.Template.Build.Legends_Aquatic[1].Skill,
                Slot = SkillSlots.AquaticLegend1,
                Aquatic = true,
                Scale = 0.8,
                Location = new Point(36 + 26 + 27, 0),
            });

            this._Legends_Aquatic[0].Click += this.Legend;
            this._Legends_Aquatic[1].Click += this.Legend;

            this._Legends_Terrestrial = new List<Skill_Control>();
            this._Legends_Terrestrial.Add(new Skill_Control(this.Parent)
            {
                Skill = this.Template.Build.Legends_Terrestrial[0].Skill,
                Slot = SkillSlots.TerrestrialLegend1,
                Aquatic = false,
                Scale = 0.8,
                Location = new Point(p, 0),
            });
            this._Legends_Terrestrial.Add(new Skill_Control(this.Parent)
            {
                Skill = this.Template.Build.Legends_Terrestrial[1].Skill,
                Slot = SkillSlots.TerrestrialLegend1,
                Aquatic = false,
                Scale = 0.8,
                Location = new Point(p + 36 + 26, 0),
            });

            this._Legends_Terrestrial[0].Click += this.Legend;
            this._Legends_Terrestrial[1].Click += this.Legend;

            this.LegendSelector = new SkillSelector_Control()
            {
                Parent = this.Parent,

                // Size  =  new Point(100, 250),
                Visible = false,
                ZIndex = this.ZIndex + 3,
            };
            this.LegendSelector.SkillChanged += this.LegendSelector_SkillChanged;
            this.SkillSelector.SkillChanged += this.OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed += this.OnGlobalClick;
            BuildsManager.ModuleInstance.Selected_Template_Changed += this.ApplyBuild;
        }

        private void Control_Click(object sender, MouseEventArgs mouse)
        {
            var control = (Skill_Control)sender;

            if (this.CanClick)
            {
                if (!this.SkillSelector.Visible || this.SkillSelector.currentObject != control)
                {
                    this.SkillSelector.Visible = true;
                    this.SkillSelector.Skill_Control = control;
                    this.SkillSelector.Location = control.Location.Add(new Point(-2, control.Height));

                    List<API.Skill> Skills = new List<API.Skill>();
                    if (this.Template.Build.Profession != null)
                    {
                        if (this.Template.Build.Profession.Id == "Revenant")
                        {
                            var legend = control.Aquatic ? this.Template.Build.Legends_Aquatic[0] : this.Template.Build.Legends_Terrestrial[0];

                            if (legend != null && legend.Utilities != null)
                            {
                                switch (control.Slot)
                                {
                                    case SkillSlots.Heal:
                                        Skills.Add(legend?.Heal);
                                        break;

                                    case SkillSlots.Elite:
                                        Skills.Add(legend?.Elite);
                                        break;

                                    default:
                                        foreach (API.Skill iSkill in legend?.Utilities)
                                        {
                                            Skills.Add(iSkill);
                                        }

                                        break;
                                }
                            }
                        }
                        else
                        {
                            foreach (API.Skill iSkill in this.Template.Build.Profession.Skills.OrderBy(e => e.Specialization).ThenBy(e => e.Categories.Count > 0 ? e.Categories[0] : "Unkown").ToList())
                            {
                                if (iSkill.Specialization == 0 || this.Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == iSkill.Specialization) != null)
                                {
                                    switch (control.Slot)
                                    {
                                        case SkillSlots.Heal:
                                            if (iSkill.Slot == API.skillSlot.Heal)
                                            {
                                                Skills.Add(iSkill);
                                            }

                                            break;

                                        case SkillSlots.Elite:
                                            if (iSkill.Slot == API.skillSlot.Elite)
                                            {
                                                Skills.Add(iSkill);
                                            }

                                            break;

                                        default:
                                            if (iSkill.Slot == API.skillSlot.Utility)
                                            {
                                                Skills.Add(iSkill);
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                    }

                    this.SkillSelector.Skills = Skills;
                    this.SkillSelector.Aquatic = control.Aquatic;
                    this.SkillSelector.currentObject = control;
                }
                else
                {
                    this.SkillSelector.Visible = false;
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var rect0 = new Rectangle(new Point(36, 15), new Point(25, 25)).Scale(this.Scale);
            var rect1 = new Rectangle(new Point(this._Width - ((this._Skills_Aquatic.Count * (this._SkillSize + 1)) + 28) + 63, 15), new Point(25, 25)).Scale(this.Scale);

            if (rect0.Contains(this.RelativeMousePosition) || rect1.Contains(this.RelativeMousePosition))
            {
                this.Template.Build?.SwapLegends();
                this.SetTemplate();
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);
            if (!this.CanClick)
            {
                this.CanClick = true;
            }
        }

        private void LegendSelector_SkillChanged(object sender, SkillChangedEvent e)
        {
            var ctrl = e.Skill_Control;
            var legend = this.Template.Build.Legends_Terrestrial[0];

            if (ctrl == this._Legends_Terrestrial[0])
            {
                this.Template.Build.Legends_Terrestrial[0] = this.Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = this.Template.Build.Legends_Terrestrial[0];

                if (legend != null)
                {
                    this.Template.Build.Skills_Terrestrial[0] = legend.Heal;
                    this.Template.Build.Skills_Terrestrial[1] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Terrestrial[1]?.PaletteId);
                    this.Template.Build.Skills_Terrestrial[2] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Terrestrial[2]?.PaletteId);
                    this.Template.Build.Skills_Terrestrial[3] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Terrestrial[3]?.PaletteId);
                    this.Template.Build.Skills_Terrestrial[4] = legend.Elite;
                }
            }
            else if (ctrl == this._Legends_Terrestrial[1])
            {
                this.Template.Build.Legends_Terrestrial[1] = this.Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = this.Template.Build.Legends_Terrestrial[1];

                if (legend != null)
                {
                    this.Template.Build.InactiveSkills_Terrestrial[0] = legend.Heal;
                    this.Template.Build.InactiveSkills_Terrestrial[1] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Terrestrial[1]?.PaletteId);
                    this.Template.Build.InactiveSkills_Terrestrial[2] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Terrestrial[2]?.PaletteId);
                    this.Template.Build.InactiveSkills_Terrestrial[3] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Terrestrial[3]?.PaletteId);
                    this.Template.Build.InactiveSkills_Terrestrial[4] = legend.Elite;
                }
            }
            else if (ctrl == this._Legends_Aquatic[0])
            {
                this.Template.Build.Legends_Aquatic[0] = this.Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = this.Template.Build.Legends_Aquatic[0];

                if (legend != null)
                {
                    this.Template.Build.Skills_Aquatic[0] = legend.Heal;
                    this.Template.Build.Skills_Aquatic[1] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Aquatic[1]?.PaletteId);
                    this.Template.Build.Skills_Aquatic[2] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Aquatic[2]?.PaletteId);
                    this.Template.Build.Skills_Aquatic[3] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.Skills_Aquatic[3]?.PaletteId);
                    this.Template.Build.Skills_Aquatic[4] = legend.Elite;
                }
            }
            else if (ctrl == this._Legends_Aquatic[1])
            {
                this.Template.Build.Legends_Aquatic[1] = this.Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = this.Template.Build.Legends_Aquatic[1];

                if (legend != null)
                {
                    this.Template.Build.InactiveSkills_Aquatic[0] = legend.Heal;
                    this.Template.Build.InactiveSkills_Aquatic[1] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Aquatic[1]?.PaletteId);
                    this.Template.Build.InactiveSkills_Aquatic[2] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Aquatic[2]?.PaletteId);
                    this.Template.Build.InactiveSkills_Aquatic[3] = legend.Utilities.Find(skill => skill.PaletteId == this.Template.Build.InactiveSkills_Aquatic[3]?.PaletteId);
                    this.Template.Build.InactiveSkills_Aquatic[4] = legend.Elite;
                }
            }

            ctrl.Skill = e.Skill;
            this.SetTemplate();
            this.Template.SetChanged();
            this.LegendSelector.Visible = false;
            this.CanClick = false;
        }

        private void Legend(object sender, MouseEventArgs e)
        {
            var ctrl = (Skill_Control)sender;
            if (this.Template.Profession?.Id == "Revenant")
            {
                List<API.Skill> legends = new List<API.Skill>();
                foreach (API.Legend legend in this.Template.Profession.Legends)
                {
                    if (legend.Specialization == 0 || this.Template.Specialization?.Id == legend.Specialization)
                    {
                        legends.Add(legend.Skill);
                    }
                }

                this.LegendSelector.Skills = legends;
                this.LegendSelector.Visible = true;
                this.LegendSelector.Aquatic = false;
                this.LegendSelector.currentObject = ctrl;

                this.LegendSelector.Skill_Control = ctrl;
                this.LegendSelector.Location = ctrl.Location.Add(new Point(-2, (int)(ctrl.Height * ctrl.Scale)));
                this.CanClick = false;
            }
        }

        public void ApplyBuild(object sender, EventArgs e)
        {
            this.SetTemplate();
        }

        private void OnSkillChanged(object sender, SkillChangedEvent e)
        {
            if (e.Skill_Control.Aquatic)
            {
                foreach (Skill_Control skill_Control in this._Skills_Aquatic)
                {
                    if (skill_Control.Skill == e.Skill)
                    {
                        skill_Control.Skill = null;
                    }
                }

                for (int i = 0; i < this.Template.Build.Skills_Aquatic.Count; i++)
                {
                    if (this.Template.Build.Skills_Aquatic[i] == e.Skill)
                    {
                        this.Template.Build.Skills_Aquatic[i] = null;
                    }
                }

                this.Template.Build.Skills_Aquatic[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }
            else
            {
                foreach (Skill_Control skill_Control in this._Skills_Terrestrial)
                {
                    if (skill_Control.Skill == e.Skill)
                    {
                        skill_Control.Skill = null;
                    }
                }

                for (int i = 0; i < this.Template.Build.Skills_Terrestrial.Count; i++)
                {
                    if (this.Template.Build.Skills_Terrestrial[i] == e.Skill)
                    {
                        this.Template.Build.Skills_Terrestrial[i] = null;
                    }
                }

                this.Template.Build.Skills_Terrestrial[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }

            this.Template.SetChanged();
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!this.SkillSelector.MouseOver)
            {
                this.SkillSelector.Hide();
            }

            if (!this.LegendSelector.MouseOver)
            {
                this.LegendSelector.Hide();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this,
                this._AquaTexture,
                new Rectangle(new Point(0, 50), new Point(25, 25)).Scale(this.Scale),
                this._AquaTexture.Bounds,
                Color.White,
                0f,
                default);

            if (this.ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this._SwapTexture,
                    new Rectangle(new Point(36 + 27, 15), new Point(25, 25)).Scale(this.Scale),
                    this._SwapTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawOnCtrl(
                this,
                this._TerrestrialTexture,
                new Rectangle(new Point(this._Width - ((this._Skills_Aquatic.Count * (this._SkillSize + 1)) + 28), 50), new Point(25, 25)).Scale(this.Scale),
                this._TerrestrialTexture.Bounds,
                Color.White,
                0f,
                default);

            if (this.ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this._SwapTexture,
                    new Rectangle(new Point(this._Width - ((this._Skills_Aquatic.Count * (this._SkillSize + 1)) + 28) + 63, 15), new Point(25, 25)).Scale(this.Scale),
                    this._SwapTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }
        }

        protected override void DisposeControl()
        {
            BuildsManager.ModuleInstance.Selected_Template_Changed -= this.ApplyBuild;
            this.LegendSelector.SkillChanged -= this.LegendSelector_SkillChanged;
            this.SkillSelector.SkillChanged -= this.OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed -= this.OnGlobalClick;

            this._Legends_Terrestrial[0].Click -= this.Legend;
            this._Legends_Terrestrial[1].Click -= this.Legend;
            this._Legends_Aquatic[0].Click -= this.Legend;
            this._Legends_Aquatic[1].Click -= this.Legend;

            foreach (Skill_Control skill in this._Skills_Terrestrial) { skill.Click -= this.Control_Click; }
            foreach (Skill_Control skill in this._Skills_Aquatic) { skill.Click -= this.Control_Click; }
            this._Skills_Terrestrial.DisposeAll();
            this._Skills_Aquatic.DisposeAll();

            this.CustomTooltip.Dispose();

            base.DisposeControl();
        }

        public void SetTemplate()
        {
            var i = 0;
            this.ShowProfessionSkills = this.Template.Profession?.Id == "Revenant";

            if (!this.ShowProfessionSkills)
            {
                this.Location = this._OGLocation.Add(new Point(0, -16));
            }
            else
            {
                this.Location = this._OGLocation;
            }

            i = 0;
            foreach (Skill_Control sCtrl in this._Skills_Aquatic)
            {
                sCtrl.Skill = this.Template.Build.Skills_Aquatic[i];
                sCtrl.Location = new Point(sCtrl.Location.X, this.ShowProfessionSkills ? 51 : (51 / 2) + 5);
                i++;
            }

            i = 0;
            foreach (Skill_Control sCtrl in this._Skills_Terrestrial)
            {
                sCtrl.Skill = this.Template.Build.Skills_Terrestrial[i];
                sCtrl.Location = new Point(sCtrl.Location.X, this.ShowProfessionSkills ? 51 : (51 / 2) + 5);
                i++;
            }

            this._Legends_Terrestrial[0].Skill = this.Template.Build.Legends_Terrestrial[0]?.Skill;
            this._Legends_Terrestrial[1].Skill = this.Template.Build.Legends_Terrestrial[1]?.Skill;

            this._Legends_Aquatic[0].Skill = this.Template.Build.Legends_Aquatic[0]?.Skill;
            this._Legends_Aquatic[1].Skill = this.Template.Build.Legends_Aquatic[1]?.Skill;

            this._Legends_Terrestrial[0].Visible = this.ShowProfessionSkills;
            this._Legends_Terrestrial[1].Visible = this.ShowProfessionSkills;

            this._Legends_Aquatic[0].Visible = this.ShowProfessionSkills;
            this._Legends_Aquatic[1].Visible = this.ShowProfessionSkills;

        }
    }

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

            this._SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            this._SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            this._EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            this._SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            this._SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            this._PlaceHolderTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)_Icons.Refresh];
            this._EmptyTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)_Icons.Bug];
            this._Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));
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