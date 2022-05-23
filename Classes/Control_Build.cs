using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.ChatLinks;
using Blish_HUD.Input;
using MonoGame.Extended.BitmapFonts;
using System.Threading;

namespace Kenedia.Modules.BuildsManager
{
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
            get => _Scale;
            set
            {
                _Scale = value;
                UpdateLayout();
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
            Parent = parent;
            CustomTooltip = customTooltip;
            Specialization_Control = specialization_Control;
            DefaultPoint = p;
            Location = p;
            DefaultBounds = new Rectangle(0, 0, _TraitSize, _TraitSize);
            _Trait = trait;
            Size = new Point(_TraitSize, _TraitSize);
            _Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            ClipsBounds = false;
            UpdateLayout();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (this.Trait != null)
            {
                CustomTooltip.Visible = true;

                if (CustomTooltip.CurrentObject != this)
                {
                    CustomTooltip.CurrentObject = this;
                    CustomTooltip.Header = this.Trait.Name;
                    CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                    CustomTooltip.Content = new List<string>() { this.Trait.Description == "" ? "No Description in API" : this.Trait.Description };
                }
            }
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            if (CustomTooltip.CurrentObject == this) CustomTooltip.Visible = false;
        }
        protected override void OnClick(MouseEventArgs mouse)
        {
            base.OnClick(mouse);

            if (this.Trait != null && Trait.Type == API.traitType.Major && Template != null && MouseOver)
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

        private API.Trait _Trait;
        public API.Trait Trait
        {
            get => TraitType != null ? TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex] : null;
            set
            {

            }
        }

        public bool Hovered;
        public bool Selected
        {
            get
            {
                var trait = TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex];

                return Template != null && trait != null && (trait.Type == API.traitType.Minor || Template.Build.SpecLines[SpecIndex].Traits.Contains(trait));
            }
        }

        public Point DefaultPoint;
        public Point Point;

        public Rectangle Bounds;
        public Rectangle DefaultBounds;

        public ConnectorLine PreLine = new ConnectorLine();
        public ConnectorLine PostLine = new ConnectorLine();
        public EventHandler Changed;

        private List<Point>_MinorTraits = new List<Point>()
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
            if (Template == null || Bounds == null) return;                        
            var trait = TraitType == API.traitType.Major ? Template.Build.SpecLines[SpecIndex].Specialization?.MajorTraits[TraitIndex] : Template.Build.SpecLines[SpecIndex].Specialization?.MinorTraits[TraitIndex];

            if (trait != null)
            {
                spriteBatch.DrawOnCtrl(this,
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
            get => _Specialization;
            set
            {
                if (value != null)
                {
                    _Specialization = value;
                }
            }
        }
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public SpecializationSelector_Control()
        {
            BuildsManager.ModuleInstance.Selected_Template_Changed += ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed += ClosePopUp;
        }

        private void ClosePopUp(object sender, EventArgs e)
        {
            if(!MouseOver) Hide();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var i = 0;
            var size = 64;
            if (Template.Build.Profession != null)
            {
                foreach (API.Specialization spec in Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || Elite)
                    {
                        var rect = new Rectangle(20 + i * size, (Height - size) / 2, size, size);

                        if (rect.Contains(RelativeMousePosition))
                        {
                            var sp = Template.Build.SpecLines.Find(x => x.Specialization != null && x.Specialization.Id == spec.Id);

                            if (sp != null && sp != Template.Build.SpecLines[Index])
                            {
                                //var traits = new List<API.Trait>(sp.Traits);
                                Template.Build.SpecLines[Index].Specialization = sp.Specialization;
                                Template.Build.SpecLines[Index].Traits = sp.Traits;
                                //Template.Build.SpecLines[Index].Control.UpdateLayout();
                                Template.SetChanged();

                                sp.Specialization = null;
                                sp.Traits = new List<API.Trait>();
                            }
                            else
                            {
                                if (Template.Build.SpecLines[Index] != null)
                                {
                                    foreach (SpecLine specLine in Template.Build.SpecLines)
                                    {
                                        if (spec != Specialization && specLine.Specialization == spec)
                                        {
                                            specLine.Specialization = null;
                                            specLine.Traits = new List<API.Trait>();
                                        }
                                    }

                                    if(Template.Build.SpecLines[Index].Specialization != spec)
                                    {
                                        Template.Build.SpecLines[Index].Specialization = spec;
                                        Template.SetChanged();
                                    }
                                }
                            }

                            Hide();
                            return;
                        }

                        i++;
                    }
                }
            }

            Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(Parent,
                                    ContentService.Textures.Pixel,
                                    bounds.Add(Location),
                                    bounds,
                                    new Color(0, 0, 0, 205),
                                    0f,
                                    Vector2.Zero
                                    );

            if (Template.Build.Profession != null)
            {
                string text = "";

                var i = 0;
                var size = 64;
                foreach (API.Specialization spec in Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || Elite)
                    {
                        var rect = new Rectangle(20 + i * size, (Height - size) / 2, size, size);
                        if (rect.Contains(RelativeMousePosition)) text = spec.Name;

                        spriteBatch.DrawOnCtrl(Parent,
                                                spec.Icon._AsyncTexture,
                                                rect.Add(Location),
                                                spec.Icon._AsyncTexture.Texture.Bounds,
                                                Specialization == spec || rect.Contains(RelativeMousePosition) ? Color.White : Color.Gray,
                                                0f,
                                                Vector2.Zero
                                                );
                        i++;
                    }
                }

                if(text != "")
                {
                    BasicTooltipText = text;
                }
                else
                {
                    BasicTooltipText = null;
                }
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            BuildsManager.ModuleInstance.Selected_Template_Changed -= ClosePopUp;
            Input.Mouse.LeftMouseButtonPressed -= ClosePopUp;
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
            get => _Scale;
            set
            {
                _Scale = value;

                foreach (Trait_Control trait in _MajorTraits)
                {
                    trait.Scale = value;
                }

                foreach (Trait_Control trait in _MinorTraits)
                {
                    trait.Scale = value;
                }

                //Width = (int)(_Width * value);
                //Height = (int)(_Height * value);
                Location = DefaultLocation.Scale(value);
                Size = new Point(_Width, _Height).Scale(value);

                UpdateLayout();
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
            get => Template != null ? Template.Build.SpecLines[Index].Specialization : null;
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
            Parent = parent;
            CustomTooltip = customTooltip;
            Index = index;
            Size = new Point(_Width, _Height);
            DefaultLocation = p;
            Location = p;

            //BackgroundColor = Color.Magenta;

            _SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            _SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            _EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            _SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            _SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            _EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            _MinorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(215, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MinorTraits[0] : null, CustomTooltip, this){ZIndex = ZIndex + 1, TraitIndex = 0, SpecIndex = Index, TraitType = API.traitType.Minor},
                    new Trait_Control(Parent, new Point(360, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MinorTraits[1]: null, CustomTooltip, this){ZIndex = ZIndex + 1, TraitIndex = 1, SpecIndex = Index, TraitType = API.traitType.Minor},
                    new Trait_Control(Parent, new Point(505, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MinorTraits[2]: null, CustomTooltip, this){ZIndex = ZIndex + 1, TraitIndex = 2, SpecIndex = Index, TraitType = API.traitType.Minor},
                };
            _MajorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization != null ? Specialization.MajorTraits[0]: null, CustomTooltip, this){ZIndex = ZIndex + 1, TraitIndex = 0, SpecIndex = Index, TraitType = API.traitType.Major },
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MajorTraits[1]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 1, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization != null ? Specialization.MajorTraits[2]: null, CustomTooltip, this){ZIndex= ZIndex + 1 , TraitIndex = 2, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization != null ? Specialization.MajorTraits[3]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 3, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MajorTraits[4]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 4, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization != null ? Specialization.MajorTraits[5]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 5, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization != null ? Specialization.MajorTraits[6]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 6, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2).Add(Location), Specialization != null ? Specialization.MajorTraits[7]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 7, SpecIndex = Index, TraitType = API.traitType.Major},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization != null ? Specialization.MajorTraits[8]: null, CustomTooltip, this){ZIndex= ZIndex + 1, TraitIndex = 8, SpecIndex = Index, TraitType = API.traitType.Major},
                };

            var TemplateSpecLine = Template.Build.SpecLines.Find(e => e.Specialization == Specialization);

            foreach (Trait_Control trait in _MajorTraits)
            {
                trait.Click += Trait_Click;
            }

            Selector = new SpecializationSelector_Control()
            {
                Index = Index,
                Specialization_Control = this,
                Parent = Parent,
                Visible = false,
                ZIndex = ZIndex + 2,
                Elite = Elite,
            };

            _Created = true;
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
                var highlight = HighlightBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);
                var selector = SelectorBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);

                if (selector || highlight)
                {
                    if (Selector.Visible)
                    {
                        Selector.Visible = false;
                    }
                    else
                    {
                        Selector.Visible = true;

                        Selector.Location = LocalBounds.Location.Add(new Point(SelectorBounds.Width, 0));
                        Selector.Size = LocalBounds.Size.Add(new Point(-SelectorBounds.Width, 0));

                        Selector.Elite = Elite;
                        Selector.Specialization = Specialization;
                    }
                }
            }
        }

        public void UpdateLayout()
        {
            if (_Created)
            {
                AbsoluteBounds = new Rectangle(0, 0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2)).Scale(Scale).Add(Location);

                ContentBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Scale(Scale).Add(Location);
                SelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, 15, _Height).Scale(Scale).Add(Location);

                HighlightBounds = new Rectangle(_Width - _HighlightLeft, (_Height - _SpecHighlightFrame.Height) / 2, _SpecHighlightFrame.Width, _SpecHighlightFrame.Height).Scale(Scale).Add(Location);
                SpecSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Scale(Scale).Add(Location);

                FirstLine.Bounds = new Rectangle(HighlightBounds.Right - 5.Scale(_Scale), HighlightBounds.Center.Y, 225 - HighlightBounds.Right, _LineThickness.Scale(_Scale));
                WeaponTraitBounds = new Rectangle(HighlightBounds.Right - _TraitSize - 6, (_Height + HighlightBounds.Height - 165), _TraitSize, _TraitSize).Scale(Scale).Add(Location);

                SpecHovered = HighlightBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition);
                if (SpecHovered)
                {
                    BasicTooltipText = Specialization?.Name;
                }
                else
                {
                    BasicTooltipText = null;
                }

                foreach (Trait_Control trait in _MajorTraits)
                {
                    if (trait.Selected)
                    {

                        var minor = _MinorTraits[trait.Trait.Tier - 1];
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
                        trait.PreLine.Bounds = new Rectangle(minor_Pos.X, minor_Pos.Y, minor.AbsoluteBounds.Center.Distance2D(trait.AbsoluteBounds.Center), _LineThickness.Scale(_Scale));

                        if (trait.Selected && trait.Trait.Tier != 3)
                        {
                            minor = _MinorTraits[trait.Trait.Tier];
                            trait.PostLine.Bounds = new Rectangle(majorPos.X, majorPos.Y, trait.AbsoluteBounds.Center.Distance2D(minor.AbsoluteBounds.Center), _LineThickness.Scale(_Scale));
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

            spriteBatch.DrawOnCtrl(Parent,
                                   ContentService.Textures.Pixel,
                                   AbsoluteBounds,
                                   AbsoluteBounds,
                                   Color.Black,
                                   0f,
                                   Vector2.Zero
                                   );

            spriteBatch.DrawOnCtrl(Parent,
                                   _EmptyTraitLine,
                                   ContentBounds,
                                   _EmptyTraitLine.Bounds,
                                    new Color(135, 135, 135, 255),
                                   0f,
                                   Vector2.Zero
                                   );


            if (Specialization != null)
            {

                //Background
                spriteBatch.DrawOnCtrl(Parent,
                                    Specialization.Background._AsyncTexture,
                                    ContentBounds,
                                    Specialization.Background._AsyncTexture.Texture.Bounds,
                                    Color.White,
                                    0f,
                                    Vector2.Zero
                                    );



                //Lines
                if (FirstLine.Bounds != null)
                {
                    spriteBatch.DrawOnCtrl(Parent,
                                           _Line,
                                           FirstLine.Bounds,
                                           _Line.Bounds,
                                           Color.White,
                                           FirstLine.Rotation,
                                           Vector2.Zero);

                }

                foreach (Trait_Control trait in _MajorTraits)
                {

                    if (trait.PreLine.Bounds != null)
                    {
                        //ScreenNotification.ShowNotification(trait.Trait.Name, ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(Parent,
                                               _Line,
                                               trait.PreLine.Bounds,
                                               _Line.Bounds,
                                               Color.White,
                                               trait.PreLine.Rotation,
                                               Vector2.Zero);

                    }

                    if (trait.PostLine.Bounds != null)
                    {
                        //ScreenNotification.ShowNotification("PostLine", ScreenNotification.NotificationType.Error);
                        spriteBatch.DrawOnCtrl(Parent,
                                               _Line,
                                               trait.PostLine.Bounds,
                                               _Line.Bounds,
                                               Color.White,
                                               trait.PostLine.Rotation,
                                               Vector2.Zero);

                    }
                }
            }

            spriteBatch.DrawOnCtrl(Parent,
                                    _SpecFrame,
                                    ContentBounds,
                                    _SpecFrame.Bounds,
                                    Color.Black,
                                    0f,
                                    Vector2.Zero
                                    );

            //Spec Highlighter
            spriteBatch.DrawOnCtrl(Parent,
                                   _SpecHighlightFrame,
                                   HighlightBounds,
                                   _SpecHighlightFrame.Bounds,
                                   Specialization != null ? Color.White : new Color(32, 32, 32, 125),
                                   0f,
                                   Vector2.Zero
                                   );

            if (Elite)
            {
                spriteBatch.DrawOnCtrl(Parent,
                                       _EliteFrame,
                                       ContentBounds,
                                       _EliteFrame.Bounds,
                                        Color.White,
                                       0f,
                                       Vector2.Zero
                                       );

                if(Specialization != null && Specialization.WeaponTrait != null)
                {
                    spriteBatch.DrawOnCtrl(Parent,
                                           Specialization.WeaponTrait.Icon._AsyncTexture,
                                           WeaponTraitBounds,
                                           Specialization.WeaponTrait.Icon._AsyncTexture.Texture.Bounds,
                                            Color.White,
                                           0f,
                                           Vector2.Zero
                                           );
                }
            }

            if (Selector.Visible)
            {
                spriteBatch.DrawOnCtrl(Parent,
                                        ContentService.Textures.Pixel,
                                        SelectorBounds,
                                        _SpecSideSelector.Bounds,
                                        new Color(0, 0, 0, 205),
                                        0f,
                                        Vector2.Zero
                                        );
            }

            spriteBatch.DrawOnCtrl(Parent,
                                    SelectorBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition) ? _SpecSideSelector_Hovered : _SpecSideSelector,
                                    SelectorBounds,
                                    _SpecSideSelector.Bounds,
                                    Color.White,
                                    0f,
                                    Vector2.Zero
                                    );
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            foreach (Trait_Control trait in _MajorTraits)
            {
                trait.Click -= Trait_Click;
            }
            _MajorTraits?.DisposeAll();
            _MajorTraits.Clear();

            _MinorTraits?.DisposeAll();
            _MinorTraits.Clear();

            Selector?.Dispose();
            _Specialization?.Dispose();
            //Specialization?.Dispose();

            _SpecSideSelector_Hovered = null;
            _SpecSideSelector = null;
            _EliteFrame = null;
            _SpecHighlightFrame = null;
            _SpecFrame = null;
            _EmptyTraitLine = null;
            _Line = null;
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
            get => _Skill;
            set
            {
                _Skill = value;

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

            _SelectorTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SkillSelector).GetRegion(0, 2, 64, 12);
            _SelectorTextureHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SkillSelector_Hovered);
            _SkillPlaceHolder = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 128, 128);

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            //BackgroundColor = Color.OldLace;
        }
        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            if (Skill != null && Skill.Id > 0)
            {
                CustomTooltip.Visible = true;
                CustomTooltip.Header = Skill.Name;
                CustomTooltip.Content = new List<string>() { Skill.Description };
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
            var skillRect = new Rectangle(new Point(0, 12), new Point(Width, Height - 12)).Scale(Scale);
            spriteBatch.DrawOnCtrl(this,
                                    MouseOver ? _SelectorTextureHovered : _SelectorTexture,
                                    new Rectangle(new Point(0, 0), new Point(Width, 12)).Scale(Scale),
                                    _SelectorTexture.Bounds,
                                    Color.White,
                                    0f,
                                    default);

            spriteBatch.DrawOnCtrl(this,
                                    (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Skill.Icon._AsyncTexture.Texture : _SkillPlaceHolder,
                                    skillRect,
                                    (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Skill.Icon._AsyncTexture.Texture.Bounds : _SkillPlaceHolder.Bounds,
                                    (Skill != null && Skill.Icon != null && Skill.Icon._AsyncTexture != null) ? Color.White : new Color(0,0,0,155),
                                    0f,
                                    default);

            if (MouseOver)
            {
                var color = Color.Honeydew;

                //Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, skillRect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, skillRect.Width, 1), Rectangle.Empty, color * 0.6f);

                //Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Bottom - 2, skillRect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Bottom - 1, skillRect.Width, 1), Rectangle.Empty, color * 0.6f);

                //Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, 2, skillRect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(skillRect.Left, skillRect.Top, 1, skillRect.Height), Rectangle.Empty, color * 0.6f);

                //Right
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

    public class SkillChangedEvent
    {
        public API.Skill Skill;
        public Skill_Control Skill_Control;
        public SkillChangedEvent(API.Skill skill, Skill_Control skill_Control)
        {
            Skill = skill;
            Skill_Control = skill_Control;
        }
    }
    public class SkillSelector_Control : Control
    {
        class SelectionSkill
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
        private BitmapFont Font;

        public event EventHandler<SkillChangedEvent> SkillChanged;

        private void OnSkillChanged(API.Skill skill, Skill_Control skill_Control)
        {
            this.SkillChanged?.Invoke(this, new SkillChangedEvent(skill, skill_Control));
        }

        public SkillSelector_Control()
        {
            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            Font = GameService.Content.DefaultFont18; 
            Size = new Point(20 + 4 * _SkillSize, _SkillSize * (int)Math.Ceiling(Skills.Count / (double)4));
            ClipsBounds = false;

            _NoWaterTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.NoWaterTexture).GetRegion(16, 16, 96, 96);

            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            CustomTooltip.Dispose();

            Input.Mouse.LeftMouseButtonPressed -= OnGlobalClick;
        }

        void OnGlobalClick(object sender, MouseEventArgs e)
        {
            if (Visible)
            {
                foreach (SelectionSkill entry in _SelectionSkills)
                {
                    if (entry.Hovered)
                    {
                        var noUnderwater = Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
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
            Size = new Point(20 + 4 * _SkillSize, 40 + _SkillSize * (int)Math.Ceiling(Skills.Count / (double)4));
            var row = 0;
            var col = 0;

            var baseRect = new Rectangle(0, 0, Width, Height);
            foreach (SelectionSkill entry in _SelectionSkills)
            {
                var rect = new Rectangle(10 + col * _SkillSize, 30 + row * _SkillSize, _SkillSize, _SkillSize);
                if (!baseRect.Contains(rect))
                {
                    col = 0;
                    row++;
                    rect = new Rectangle(10 + col * _SkillSize, 30 + row * _SkillSize, _SkillSize, _SkillSize);
                }

                entry.Bounds = rect;
                entry.Hovered = entry.Bounds.Contains(RelativeMousePosition);
                col++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            UpdateLayout();

            spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    bounds,
                                    bounds,
                                    new Color(0, 0, 0, 230),
                                    0f,
                                    default);

            var color = Color.Black;
            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            if (Skill_Control != null)
            {
                var text = "";
                switch (Skill_Control.Slot)
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


                var sRect = Font.GetStringRectangle(text);
                spriteBatch.DrawStringOnCtrl(this,
                                        text,
                                        Font,
                                        new Rectangle((bounds.Width - (int)sRect.Width) / 2, 0, (int)sRect.Width, 20),
                                        Color.White,
                                        false,
                                        HorizontalAlignment.Left
                                        );

                CustomTooltip.Visible = false;
                foreach (SelectionSkill entry in _SelectionSkills)
                {
                    if (entry.Skill != null)
                    {
                        var noUnderwater = Aquatic && entry.Skill.Flags.Contains("NoUnderwater");
                        spriteBatch.DrawOnCtrl(this,
                                                entry.Skill.Icon._AsyncTexture,
                                                entry.Bounds,
                                                entry.Skill.Icon._AsyncTexture.Texture.Bounds,
                                                noUnderwater ? Color.Gray : Color.White,
                                                0f,
                                                default);

                        if (noUnderwater)
                        {
                            spriteBatch.DrawOnCtrl(this,
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
                            //Top
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, entry.Bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, entry.Bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                            //Bottom
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Bottom - 2, entry.Bounds.Width, 2), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Bottom - 1, entry.Bounds.Width, 1), Rectangle.Empty, color * 0.6f);

                            //Left
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, 2, entry.Bounds.Height), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Left, entry.Bounds.Top, 1, entry.Bounds.Height), Rectangle.Empty, color * 0.6f);

                            //Right
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Right - 2, entry.Bounds.Top, 2, entry.Bounds.Height), Rectangle.Empty, color * 0.5f);
                            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(entry.Bounds.Right - 1, entry.Bounds.Top, 1, entry.Bounds.Height), Rectangle.Empty, color * 0.6f);

                            CustomTooltip.CurrentObject = entry.Skill;
                            CustomTooltip.Header = entry.Skill.Name;
                            CustomTooltip.Content = new List<string>() { entry.Skill.Description };
                            CustomTooltip.HeaderColor = new Color(255, 204, 119, 255);
                            CustomTooltip.Visible = true;
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
            get => _Scale;
            set
            {
                _Scale = value;
                foreach(Skill_Control skill in _Skills_Aquatic)
                {
                    skill.Scale = value;
                }
                foreach(Skill_Control skill in _Skills_Terrestrial)
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
            get => Location;
            set
            {
                if (Location == null || Location == Point.Zero) _OGLocation = value;
                Location = value;
            }
        }

        public SkillBar_Control(Container parent)
        {
            Parent = parent;
            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
        };

            _TerrestrialTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Land);
            _AquaTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Water);
            _SwapTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Refresh);

            var slots = Enum.GetValues(typeof(SkillSlots));
            //BackgroundColor = Color.Magenta;

            _Skills_Aquatic = new List<Skill_Control>();
            foreach (API.Skill skill in Template.Build.Skills_Aquatic)
            {
                _Skills_Aquatic.Add(new Skill_Control(Parent)
                {
                    Location = new Point(27 + _Skills_Aquatic.Count * (_SkillSize + 1), 51),
                    Skill = Template.Build.Skills_Aquatic[_Skills_Aquatic.Count],
                    Slot = (SkillSlots)_Skills_Aquatic.Count,
                    Aquatic = true,
                });

                var control = _Skills_Aquatic[_Skills_Aquatic.Count - 1];
                control.Click += Control_Click;
            }

            var p = _Width - (_Skills_Aquatic.Count * (_SkillSize + 1));
            _Skills_Terrestrial = new List<Skill_Control>();
            foreach (API.Skill skill in Template.Build.Skills_Terrestrial)
            {
                _Skills_Terrestrial.Add(new Skill_Control(Parent)
                {
                    Location = new Point(p + _Skills_Terrestrial.Count * (_SkillSize + 1), 51),
                    Skill = Template.Build.Skills_Terrestrial[_Skills_Terrestrial.Count],
                    Slot = (SkillSlots)_Skills_Terrestrial.Count,
                });

                var control = _Skills_Terrestrial[_Skills_Terrestrial.Count - 1];
                control.Click += Control_Click;
            }

            SkillSelector = new SkillSelector_Control()
            {
                Parent = Parent,
                //Size  =  new Point(100, 250),
                Visible = false,
                ZIndex = ZIndex + 3,
            };

            _Legends_Aquatic = new List<Skill_Control>();
            _Legends_Aquatic.Add(new Skill_Control(Parent)
            {
                Skill = Template.Build.Legends_Aquatic[0].Skill,
                Slot = SkillSlots.AquaticLegend1,
                Aquatic = true,
                Scale = 0.8,
                Location = new Point(27, 0),
            });
            _Legends_Aquatic.Add(new Skill_Control(Parent)
            {
                Skill = Template.Build.Legends_Aquatic[1].Skill,
                Slot = SkillSlots.AquaticLegend1,
                Aquatic = true,
                Scale = 0.8,
                Location = new Point(36 + 26 + 27, 0),
            });

            _Legends_Aquatic[0].Click += Legend;
            _Legends_Aquatic[1].Click += Legend;

            _Legends_Terrestrial = new List<Skill_Control>();
            _Legends_Terrestrial.Add(new Skill_Control(Parent)
            {
                Skill = Template.Build.Legends_Terrestrial[0].Skill,
                Slot = SkillSlots.TerrestrialLegend1,
                Aquatic = false,
                Scale = 0.8,
                Location = new Point(p, 0),
            });
            _Legends_Terrestrial.Add(new Skill_Control(Parent)
            {
                Skill = Template.Build.Legends_Terrestrial[1].Skill,
                Slot = SkillSlots.TerrestrialLegend1,
                Aquatic = false,
                Scale = 0.8,
                Location = new Point(p + 36 + 26, 0),
            });

            _Legends_Terrestrial[0].Click += Legend;
            _Legends_Terrestrial[1].Click += Legend;

            LegendSelector = new SkillSelector_Control()
            {
                Parent = Parent,
                //Size  =  new Point(100, 250),
                Visible = false,
                ZIndex = ZIndex + 3,
            };
            LegendSelector.SkillChanged += LegendSelector_SkillChanged;
            SkillSelector.SkillChanged += OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;
            BuildsManager.ModuleInstance.Selected_Template_Changed += ApplyBuild;
        }

        private void Control_Click(object sender, MouseEventArgs mouse)
        {
            var control = (Skill_Control)sender;

            if (CanClick)
            {
                if (!SkillSelector.Visible || SkillSelector.currentObject != control)
                {
                    SkillSelector.Visible = true;
                    SkillSelector.Skill_Control = control;
                    SkillSelector.Location = control.Location.Add(new Point(-2, control.Height));

                    List<API.Skill> Skills = new List<API.Skill>();
                    if (Template.Build.Profession != null)
                    {
                        if(Template.Build.Profession.Id == "Revenant")
                        {
                            var legend = control.Aquatic ? Template.Build.Legends_Aquatic[0] : Template.Build.Legends_Terrestrial[0];

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
                        else {
                            foreach (API.Skill iSkill in Template.Build.Profession.Skills.OrderBy(e => e.Specialization).ThenBy(e => e.Categories.Count > 0 ? e.Categories[0] : "Unkown").ToList())
                            {
                                if (iSkill.Specialization == 0 || Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == iSkill.Specialization) != null)
                                {
                                    switch (control.Slot)
                                    {
                                        case SkillSlots.Heal:
                                            if (iSkill.Slot == API.skillSlot.Heal) Skills.Add(iSkill);
                                            break;

                                        case SkillSlots.Elite:
                                            if (iSkill.Slot == API.skillSlot.Elite) Skills.Add(iSkill);
                                            break;

                                        default:
                                            if (iSkill.Slot == API.skillSlot.Utility) Skills.Add(iSkill);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    SkillSelector.Skills = Skills;
                    SkillSelector.Aquatic = control.Aquatic;
                    SkillSelector.currentObject = control;
                }
                else
                {
                    SkillSelector.Visible = false;
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var rect0 = new Rectangle(new Point(36, 15), new Point(25, 25)).Scale(Scale);
            var rect1 = new Rectangle(new Point(_Width - (_Skills_Aquatic.Count * (_SkillSize + 1) + 28) + 63, 15), new Point(25, 25)).Scale(Scale);

            if(rect0.Contains(RelativeMousePosition) || rect1.Contains(RelativeMousePosition))
            {
                Template.Build?.SwapLegends();
                SetTemplate();
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);
            if (!CanClick) CanClick = true;
        }
        private void LegendSelector_SkillChanged(object sender, SkillChangedEvent e)
        {
            var ctrl = e.Skill_Control;
            var legend = Template.Build.Legends_Terrestrial[0];

            if (ctrl == _Legends_Terrestrial[0])
            {
                Template.Build.Legends_Terrestrial[0] = Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = Template.Build.Legends_Terrestrial[0];

                if (legend != null)
                {
                    Template.Build.Skills_Terrestrial[0] = legend.Heal;
                    Template.Build.Skills_Terrestrial[1] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Terrestrial[1]?.PaletteId);
                    Template.Build.Skills_Terrestrial[2] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Terrestrial[2]?.PaletteId);
                    Template.Build.Skills_Terrestrial[3] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Terrestrial[3]?.PaletteId);
                    Template.Build.Skills_Terrestrial[4] = legend.Elite;
                }
            }
            else if (ctrl == _Legends_Terrestrial[1])
            {
                Template.Build.Legends_Terrestrial[1] = Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = Template.Build.Legends_Terrestrial[1];

                if (legend != null)
                {
                    Template.Build.InactiveSkills_Terrestrial[0] = legend.Heal;
                    Template.Build.InactiveSkills_Terrestrial[1] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Terrestrial[1]?.PaletteId);
                    Template.Build.InactiveSkills_Terrestrial[2] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Terrestrial[2]?.PaletteId);
                    Template.Build.InactiveSkills_Terrestrial[3] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Terrestrial[3]?.PaletteId);
                    Template.Build.InactiveSkills_Terrestrial[4] = legend.Elite;
                }
            }
            else if (ctrl == _Legends_Aquatic[0])
            {
                Template.Build.Legends_Aquatic[0] = Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = Template.Build.Legends_Aquatic[0];

                if (legend != null)
                {
                    Template.Build.Skills_Aquatic[0] = legend.Heal;
                    Template.Build.Skills_Aquatic[1] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Aquatic[1]?.PaletteId);
                    Template.Build.Skills_Aquatic[2] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Aquatic[2]?.PaletteId);
                    Template.Build.Skills_Aquatic[3] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.Skills_Aquatic[3]?.PaletteId);
                    Template.Build.Skills_Aquatic[4] = legend.Elite;
                }
            }
            else if (ctrl == _Legends_Aquatic[1])
            {
                Template.Build.Legends_Aquatic[1] = Template.Profession.Legends.Find(leg => leg.Skill.Id == e.Skill.Id);
                legend = Template.Build.Legends_Aquatic[1];

                if (legend != null)
                {
                    Template.Build.InactiveSkills_Aquatic[0] = legend.Heal;
                    Template.Build.InactiveSkills_Aquatic[1] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Aquatic[1]?.PaletteId);
                    Template.Build.InactiveSkills_Aquatic[2] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Aquatic[2]?.PaletteId);
                    Template.Build.InactiveSkills_Aquatic[3] = legend.Utilities.Find(skill => skill.PaletteId == Template.Build.InactiveSkills_Aquatic[3]?.PaletteId);
                    Template.Build.InactiveSkills_Aquatic[4] = legend.Elite;
                }
            } 

            ctrl.Skill = e.Skill;
            SetTemplate();
            Template.SetChanged();
            LegendSelector.Visible = false;
            CanClick = false;
        }

        private void Legend(object sender, MouseEventArgs e)
        {
            var ctrl = (Skill_Control)sender;
            if (Template.Profession?.Id == "Revenant")
            {
                List<API.Skill> legends = new List<API.Skill>();
                foreach(API.Legend legend in Template.Profession.Legends)
                {
                    if(legend.Specialization == 0 || Template.Specialization?.Id == legend.Specialization)
                    {
                        legends.Add(legend.Skill);
                    }
                }

                LegendSelector.Skills = legends;
                LegendSelector.Visible = true;
                LegendSelector.Aquatic = false;
                LegendSelector.currentObject = ctrl;

                LegendSelector.Skill_Control = ctrl;
                LegendSelector.Location = ctrl.Location.Add(new Point(-2, (int) (ctrl.Height * ctrl.Scale)));
                CanClick = false;
            }
        }

        public void ApplyBuild(object sender, EventArgs e)
        {
            SetTemplate();
        }

        private void OnSkillChanged(object sender, SkillChangedEvent e)
        {
            if (e.Skill_Control.Aquatic)
            {
                foreach (Skill_Control skill_Control in _Skills_Aquatic)
                {
                    if (skill_Control.Skill == e.Skill) skill_Control.Skill = null;
                }

                for (int i = 0; i < Template.Build.Skills_Aquatic.Count; i++)
                {
                    if (Template.Build.Skills_Aquatic[i] == e.Skill) Template.Build.Skills_Aquatic[i] = null;
                }

                Template.Build.Skills_Aquatic[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }
            else
            {
                foreach (Skill_Control skill_Control in _Skills_Terrestrial)
                {
                    if (skill_Control.Skill == e.Skill) skill_Control.Skill = null;
                }

                for (int i = 0; i < Template.Build.Skills_Terrestrial.Count; i++)
                {
                    if (Template.Build.Skills_Terrestrial[i] == e.Skill) Template.Build.Skills_Terrestrial[i] = null;
                }

                Template.Build.Skills_Terrestrial[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }

            Template.SetChanged();
        }
        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!SkillSelector.MouseOver) SkillSelector.Hide();
            if (!LegendSelector.MouseOver) LegendSelector.Hide();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.DrawOnCtrl(this,
                                    _AquaTexture,
                                    new Rectangle(new Point(0, 50), new Point(25, 25)).Scale(Scale),
                                    _AquaTexture.Bounds,
                                    Color.White,
                                    0f,
                                    default);

            if (ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(this,
                                        _SwapTexture,
                                        new Rectangle(new Point(36 + 27, 15), new Point(25, 25)).Scale(Scale),
                                        _SwapTexture.Bounds,
                                        Color.White,
                                        0f,
                                        default);
            }

            spriteBatch.DrawOnCtrl(this,
                                    _TerrestrialTexture,
                                    new Rectangle(new Point(_Width - (_Skills_Aquatic.Count * (_SkillSize + 1) + 28), 50), new Point(25, 25)).Scale(Scale),
                                    _TerrestrialTexture.Bounds,
                                    Color.White,
                                    0f,
                                    default);

            if (ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(this,
                                    _SwapTexture,
                                    new Rectangle(new Point(_Width - (_Skills_Aquatic.Count * (_SkillSize + 1) + 28) + 63, 15), new Point(25, 25)).Scale(Scale),
                                    _SwapTexture.Bounds,
                                    Color.White,
                                    0f,
                                    default);
            }
        }
        protected override void DisposeControl()
        {
            BuildsManager.ModuleInstance.Selected_Template_Changed -= ApplyBuild;
            LegendSelector.SkillChanged -= LegendSelector_SkillChanged;
            SkillSelector.SkillChanged -= OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed -= OnGlobalClick;

            _Legends_Terrestrial[0].Click -= Legend;
            _Legends_Terrestrial[1].Click -= Legend;
            _Legends_Aquatic[0].Click -= Legend;
            _Legends_Aquatic[1].Click -= Legend;

            foreach(Skill_Control skill in _Skills_Terrestrial) { skill.Click -= Control_Click; }
            foreach(Skill_Control skill in _Skills_Aquatic) { skill.Click -= Control_Click; }
            _Skills_Terrestrial.DisposeAll();
            _Skills_Aquatic.DisposeAll();

            CustomTooltip.Dispose();

            base.DisposeControl();
        }
        public void SetTemplate()
        {
            var i = 0;
            ShowProfessionSkills = Template.Profession?.Id == "Revenant";

            if (!ShowProfessionSkills)
            {
                Location = _OGLocation.Add(new Point(0, -16));
            }
            else
            {
                Location = _OGLocation;
            }

            i = 0;
            foreach(Skill_Control sCtrl in _Skills_Aquatic)
            {
                sCtrl.Skill = Template.Build.Skills_Aquatic[i];
                sCtrl.Location = new Point(sCtrl.Location.X, ShowProfessionSkills ? 51 : 51 / 2 + 5);
                i++;
            }

            i = 0;
            foreach(Skill_Control sCtrl in _Skills_Terrestrial)
            {
                sCtrl.Skill = Template.Build.Skills_Terrestrial[i];
                sCtrl.Location = new Point(sCtrl.Location.X, ShowProfessionSkills ? 51 : 51 / 2 + 5);
                i++;
            }

            _Legends_Terrestrial[0].Skill = Template.Build.Legends_Terrestrial[0]?.Skill;
            _Legends_Terrestrial[1].Skill = Template.Build.Legends_Terrestrial[1]?.Skill;

            _Legends_Aquatic[0].Skill = Template.Build.Legends_Aquatic[0]?.Skill;
            _Legends_Aquatic[1].Skill = Template.Build.Legends_Aquatic[1]?.Skill;

            _Legends_Terrestrial[0].Visible = ShowProfessionSkills;
            _Legends_Terrestrial[1].Visible = ShowProfessionSkills;

            _Legends_Aquatic[0].Visible = ShowProfessionSkills;
            _Legends_Aquatic[1].Visible = ShowProfessionSkills;

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
            get => _Scale;
            set
            {
                _Scale = value;

                var p = new Point(Location.X, Location.Y);
                var s = new Point(Size.X, Size.Y);

                Size = new Point((int)(_Width * Scale), (int)(_Height * Scale));

                foreach(Specialization_Control spec in Specializations)
                {
                    spec.Scale = value;
                }

                SkillBar.Scale = value;

                UpdateLayout();
                OnResized(new ResizedEventArgs(p, s));
            }
        }

        private List<Specialization_Control> Specializations;
        public SkillBar_Control SkillBar;
        private CustomTooltip CustomTooltip;

        public Control_Build(Container parent)
        {
            Parent = parent;
            Size = new Point((int)(_Width * Scale), (int)(_Height * Scale));

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
                HeaderColor = new Color(255, 204, 119, 255),
            };

            //BackgroundColor = Color.Honeydew;
            Click += OnClick;

            _SpecSideSelector_Hovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            _SpecSideSelector = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            _EliteFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            _SpecHighlightFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            _SpecFrame = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            _EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            _PlaceHolderTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)_Icons.Refresh];
            _EmptyTexture = BuildsManager.ModuleInstance.TextureManager._Icons[(int)_Icons.Bug];
            _Line = BuildsManager.ModuleInstance.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));
            _Background = _EmptyTraitLine;

            SkillBar = new SkillBar_Control(Parent)
            {
                _Location = new Point(0, 0),
                Size = new Point(_Width, Skillbar_Height),
            };

            Specializations = new List<Specialization_Control>();
            for (int i = 0; i < Template.Build.SpecLines.Count; i++)
            {
                Specializations.Add(new Specialization_Control(Parent, i, new Point(0, 5 + Skillbar_Height + i * 134), CustomTooltip)
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
            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        private void OnClick(object sender, MouseEventArgs m)
        {

        }

        private void UpdateTemplate()
        {
            for (int i = 0; i < Template.Build.SpecLines.Count; i++)
            {
                Template.Build.SpecLines[i].Control = Specializations[i];
                //SkillBar.SetTemplate();
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