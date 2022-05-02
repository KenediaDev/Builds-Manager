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
        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    _Template.Changed += delegate
                    {

                    };
                }
            }
        }

        public int Index;

        private CustomTooltip CustomTooltip;
        private Specialization_Control Specialization_Control;
        public Trait_Control(Container parent, Point p, API.Trait trait, CustomTooltip customTooltip, Specialization_Control specialization_Control, Template template)
        {
            Parent = parent;
            Template = template;
            CustomTooltip = customTooltip;
            Specialization_Control = specialization_Control;
            DefaultPoint = p;
            Location = p;
            DefaultBounds = new Rectangle(p.X, p.Y, _TraitSize, _TraitSize);
            _Trait = trait;
            Size = new Point(_TraitSize, _TraitSize);

            Click += delegate
            {
                if (this.Trait != null && Trait.Type == API.traitType.Major && Template != null)
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
            };

            UpdateLayout();
            MouseEntered += delegate
            {
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
            };
            MouseLeft += delegate
            {
                if (CustomTooltip.CurrentObject == this) CustomTooltip.Visible = false;
            };
        }

        private API.Trait _Trait;
        public API.Trait Trait
        {
            get => _Trait;
            set
            {
                _Trait = value;
            }
        }

        public bool Hovered;
        public bool Selected
        {
            get
            {
                return Template != null && Trait != null && (Trait.Type == API.traitType.Minor || Template.Build.SpecLines[Specialization_Control.Index].Traits.Contains(Trait));
            }
        }

        public Point DefaultPoint;
        public Point Point;

        public Rectangle Bounds;
        public Rectangle DefaultBounds;

        public ConnectorLine PreLine = new ConnectorLine();
        public ConnectorLine PostLine = new ConnectorLine();
        public EventHandler Changed;

        private void UpdateLayout()
        {
            Bounds = DefaultBounds.Add(Location).Scale(Scale);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Trait == null || Bounds == null) return;

            spriteBatch.DrawOnCtrl(this,
                                    Trait.Icon.Texture,
                                    bounds,
                                    Trait.Icon.Texture.Bounds,
                                    Selected ? Color.White : (MouseOver ? Color.LightGray : Color.Gray),
                                    0f,
                                    default);
        }
    }

    public class SpecializationSelector_Control : Control
    {
        public Specialization_Control Specialization_Control;
        public int Index;
        public bool Elite;
        private Template _Template;
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
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    //Template.Changed 
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var i = 0;
            var size = 64;

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
                            Template.Build.SpecLines[Index].Control.UpdateLayout();

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

                                Template.Build.SpecLines[Index].Specialization = spec;
                            }
                        }

                        break;
                    }

                    i++;
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
                var i = 0;
                var size = 64;
                foreach (API.Specialization spec in Template.Build.Profession.Specializations)
                {
                    if (!spec.Elite || Elite)
                    {
                        var rect = new Rectangle(20 + i * size, (Height - size) / 2, size, size);
                        spriteBatch.DrawOnCtrl(Parent,
                                                spec.Icon.Texture,
                                                rect.Add(Location),
                                                spec.Icon.Texture.Bounds,
                                                Specialization == spec || rect.Contains(RelativeMousePosition) ? Color.White : Color.Gray,
                                                0f,
                                                Vector2.Zero
                                                );
                        i++;
                    }
                }
            }
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

                Width = (int)(_Width * value);
                Height = (int)(_Height * value);

                UpdateLayout();
            }
        }

        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                }
            }
        }
        public int Index;
        private API.Specialization _Specialization;
        public API.Specialization Specialization
        {
            get => _Specialization;
            set
            {
                if (_Specialization != value)
                {
                    _Specialization = value;

                    if (_Specialization != null)
                    {
                        for (int i = 0; i < _MajorTraits.Count; i++)
                        {
                            _MajorTraits[i].Trait = Specialization.MajorTraits[i];
                            _MajorTraits[i].PreLine = new ConnectorLine();
                            _MajorTraits[i].PostLine = new ConnectorLine();
                        }

                        for (int i = 0; i < _MinorTraits.Count; i++)
                        {
                            _MinorTraits[i].Trait = Specialization.MinorTraits[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _MajorTraits.Count; i++)
                        {
                            _MajorTraits[i].Trait = null;
                        }

                        for (int i = 0; i < _MinorTraits.Count; i++)
                        {
                            _MinorTraits[i].Trait = null;
                        }
                    }

                    if (_Created) UpdateLayout();
                }
            }
        }
        public EventHandler Changed;

        private Texture2D _SpecSideSelector_Hovered;
        private Texture2D _SpecSideSelector;
        private Texture2D _EliteFrame;
        private Texture2D _SpecHighlightFrame;
        private Texture2D _SpecFrame;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Line;

        private Rectangle AbsoluteBounds;
        private Rectangle ContentBounds;
        private Rectangle HighlightBounds;
        private Rectangle SelectorBounds;
        private Rectangle SpecSelectorBounds;
        private ConnectorLine FirstLine = new ConnectorLine();
        private CustomTooltip CustomTooltip;
        private SpecializationSelector_Control Selector;

        public List<Trait_Control> _MinorTraits = new List<Trait_Control>();
        public List<Trait_Control> _MajorTraits = new List<Trait_Control>();
        private bool _Created;

        public Specialization_Control(Container parent, Template template, int index, Point p, CustomTooltip customTooltip)
        {
            Parent = parent;
            _Template = template;
            CustomTooltip = customTooltip;
            Index = index;
            Specialization = template.Build.SpecLines[Index].Specialization;
            Size = new Point(_Width, _Height);
            Location = p;

            _Template.Changed += delegate
            {
                Specialization = _Template.Build.SpecLines[Index].Specialization;
            };

            _SpecSideSelector_Hovered = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            _SpecSideSelector = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            _EliteFrame = BuildsManager.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            _SpecHighlightFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            _SpecFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Line = BuildsManager.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

            _MinorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(215, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[0], CustomTooltip, this, Template){ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(360, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[1], CustomTooltip, this, Template){ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(505, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[2], CustomTooltip, this, Template){ZIndex = ZIndex + 1},
                };
            _MajorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[0], CustomTooltip, this, Template){ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[1], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[2], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[3], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[4], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[5], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[6], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[7], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[8], CustomTooltip, this, Template){ZIndex= ZIndex + 1},
                };

            var TemplateSpecLine = Template.Build.SpecLines.Find(e => e.Specialization == Specialization);

            foreach (Trait_Control trait in _MajorTraits)
            {
                //trait.Selected = Template.Build.SpecLines.Find(a => a.Traits.Contains(trait.Trait)) != null;
                trait.Click += delegate
                {
                    UpdateLayout();

                    foreach (API.Trait t in Template.Build.SpecLines[Index].Traits)
                    {
                        BuildsManager.Logger.Debug(t.Name);
                    }
                };
            }

            Selector = new SpecializationSelector_Control()
            {
                Index = Index,
                Specialization_Control = this,
                Parent = Parent,
                Visible = false,
                Template = Template,
                ZIndex = ZIndex + 2,
                Elite = Elite,
            };

            UpdateLayout();

            Moved += delegate { UpdateLayout(); };
            Resized += delegate { UpdateLayout(); };
            Click += Control_Click;
            _Created = true;
        }

        private void Control_Click(object sender, MouseEventArgs e)
        {
            if (MouseOver)
            {
                Selector.Visible = true;

                Selector.Location = LocalBounds.Location.Add(new Point(SelectorBounds.Width, 0));
                Selector.Size = LocalBounds.Size.Add(new Point(-SelectorBounds.Width, 0));

                Selector.Template = Template;
                Selector.Elite = Elite;
                Selector.Specialization = Specialization;
            }
        }

        public void UpdateLayout()
        {
            AbsoluteBounds = new Rectangle(0, 0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2)).Add(Location).Scale(Scale);

            ContentBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Add(Location).Scale(_Scale);
            SelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, 15, _Height).Add(Location).Scale(_Scale);

            HighlightBounds = new Rectangle(_Width - _HighlightLeft, (_Height - _SpecHighlightFrame.Height) / 2, _SpecHighlightFrame.Width, _SpecHighlightFrame.Height).Add(Location).Scale(_Scale);
            SpecSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height).Add(Location).Scale(_Scale);

            FirstLine.Bounds = new Rectangle(HighlightBounds.Right - 5.Scale(_Scale), HighlightBounds.Center.Y, 225 - HighlightBounds.Right, _LineThickness.Scale(_Scale));


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

                    var minor_Pos = minor.Bounds.Center.Add(new Point(-minor.Location.X, -minor.Location.Y));
                    var majorPos = trait.Bounds.Center.Add(new Point(-trait.Location.X, -trait.Location.Y));


                    trait.PreLine.Bounds = new Rectangle(minor_Pos.X, minor_Pos.Y, minor_Pos.Distance2D(majorPos), _LineThickness.Scale(_Scale));

                    if (trait.Selected && trait.Trait.Tier != 3)
                    {
                        minor = _MinorTraits[trait.Trait.Tier];
                        minor_Pos = minor.Bounds.Center.Add(new Point(-minor.Location.X, -minor.Location.Y));
                        trait.PostLine.Bounds = new Rectangle(majorPos.X, majorPos.Y, majorPos.Distance2D(minor_Pos), _LineThickness.Scale(_Scale));
                    }
                }
                else
                {
                    trait.PreLine = new ConnectorLine();
                    trait.PostLine = new ConnectorLine();
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
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
                                    Specialization.Background.Texture,
                                    ContentBounds,
                                    Specialization.Background.Texture.Bounds,
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

        public void PaintAfterChilds(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }
    }
    public class Control_Build : Control
    {
        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    Template.Changed += delegate
                    {
                        UpdateTemplate();
                        UpdateLayout();
                    };

                    UpdateTemplate();
                    UpdateLayout();
                }
            }
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
        public int _Height = 133;
        private int _HighlightLeft = 570;
        private int _TraitSize = 38;
        private int _SpecSelectSize = 64;
        private int _MiddleRowTop = (133 - 38) / 2; // (_Height - _TraitSize) / 2;
        private int FrameThickness = 1;
        private int Gap = 5;
        private int Build_Width = 900;
        private int Skillbar_Height = 110;
        private int Build_Height = 125 + (150 * 3);

        private double _Scale = 1;
        public double Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
            }
        }

        private List<Specialization_Control> Specializations;
        private CustomTooltip CustomTooltip;

        public Control_Build(Container parent, Template template)
        {
            Parent = parent;
            _Template = template;

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
            };

            //BackgroundColor = Color.Honeydew;
            Click += OnClick;

            _SpecSideSelector_Hovered = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
            _SpecSideSelector = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector);

            _EliteFrame = BuildsManager.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
            _SpecHighlightFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
            _SpecFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            _PlaceHolderTexture = BuildsManager.TextureManager._Icons[(int)_Icons.Refresh];
            _EmptyTexture = BuildsManager.TextureManager._Icons[(int)_Icons.Bug];
            _Line = BuildsManager.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));
            _Background = _EmptyTraitLine;

            Specializations = new List<Specialization_Control>();
            for (int i = 0; i < Template.Build.SpecLines.Count; i++)
            {
                Specializations.Add(new Specialization_Control(Parent, Template, i, new Point(5, Skillbar_Height + i * 134), CustomTooltip)
                {
                    ZIndex = ZIndex + 1,
                    Elite = i == 2,
                });
            }


            Disposed += delegate
            {
                CustomTooltip.Dispose();
            };

            UpdateTemplate();
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
            }
        }

        private void UpdateLayout()
        {
            if (_Template == null) return;


        }


        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_Template == null) return;
            UpdateLayout();
        }
    }
}