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
    public static class Ext{
        public static void DrawSpecLine(this SpecLine SpecLine)
        {

        }
    }
    public class Control_Build : Control
    {
        class ConnectorLine
        {
            public Rectangle Bounds;
            public float Rotation = 0;
        }

        class Trait_Control : Control
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

            public Trait_Control(Container parent, Point p, API.Trait trait)
            {
                Parent = parent;
                DefaultPoint = p;
                Location = p;
                DefaultBounds = new Rectangle(p.X, p.Y, _TraitSize, _TraitSize);
                _Trait = trait;
                Size = new Point(_TraitSize, _TraitSize);

                Click += delegate { Selected = !Selected; };

                UpdateLayout();
            }

            private API.Trait _Trait;
            public API.Trait Trait
            {
                get => _Trait;
                set
                {
                    if(value != null)
                    {
                        _Trait = value;
                    }
                }
            }

            public bool Hovered;
            private bool _Selected;
            public bool Selected
            {
                get => _Selected;
                set
                {
                    if (_Selected != value) 
                    {
                        _Selected = value;
                        Changed?.Invoke(this, EventArgs.Empty);
                    }
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
            public void Removed()
            {
                _Selected = false;
            }
            public void Added()
            {
                _Selected = true;
            }

            protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
            {
                if (Trait == null || Bounds == null) return;
               //BuildsManager.Logger.Debug("SOMETHING");
                spriteBatch.DrawOnCtrl(this,
                                        Trait.Icon.Texture,
                                        bounds,
                                        Trait.Icon.Texture.Bounds,
                                        Selected ? Color.White : (MouseOver ? Color.LightGray : Color.Gray),
                                        0f,
                                        default);
            }
        }

        class Specialization_Control : Control
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
            public bool Elite;
            public double Scale
            {
                get => _Scale;
                set
                {
                    _Scale = value;

                    Width = (int) (_Width * value);
                    Height = (int) (_Height * value);

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
            public API.Specialization Specialization;
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

            private List<Trait_Control> _MinorTraits;
            private List<Trait_Control> _MajorTraits;

            public Specialization_Control(Container parent, Template template, API.Specialization specialization, Point p)
            {
                Parent = parent;
                _Template = template;
                Specialization = specialization;
                Size = new Point(_Width, _Height);
                Location = p;                

                _SpecSideSelector_Hovered = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
                _SpecSideSelector = BuildsManager.TextureManager.getControlTexture(_Controls.SpecSideSelector);

                _EliteFrame = BuildsManager.TextureManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
                _SpecHighlightFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
                _SpecFrame = BuildsManager.TextureManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
                _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
                _Line = BuildsManager.TextureManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));

                _MinorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(215, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[0]){ ZIndex = ZIndex + 1, Selected = true},
                    new Trait_Control(Parent, new Point(360, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[1]){ ZIndex = ZIndex + 1, Selected = true},
                    new Trait_Control(Parent, new Point(505, (133 - 38) / 2).Add(Location), Specialization.MinorTraits[2]){ ZIndex = ZIndex + 1, Selected = true},
                };
                _MajorTraits = new List<Trait_Control>()
                {
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[0]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[1]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(285, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[2]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[3]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[4]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(430, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[5]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 - 3 - 38).Add(Location), Specialization.MajorTraits[6]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2).Add(Location), Specialization.MajorTraits[7]){ ZIndex = ZIndex + 1},
                    new Trait_Control(Parent, new Point(575, (133 - 38) / 2 + 3 + 38).Add(Location), Specialization.MajorTraits[8]){ ZIndex = ZIndex + 1},
                };

                var TemplateSpecLine = Template.Build.SpecLines.Find(e => e.Specialization == Specialization);

                foreach(Trait_Control trait in _MajorTraits)
                {
                    trait.Changed += delegate
                    {
                        if (TemplateSpecLine.Traits.Contains(trait.Trait))
                        {
                            TemplateSpecLine.Traits.Remove(trait.Trait);
                            trait.Removed();
                        }
                        else
                        {
                            foreach (Trait_Control iTrait in _MajorTraits.Where(e => e.Trait.Tier == trait.Trait.Tier))
                            {
                                if (iTrait != trait && TemplateSpecLine.Traits.Contains(iTrait.Trait))
                                {
                                    TemplateSpecLine.Traits.Remove(iTrait.Trait);
                                    iTrait.Removed();
                                }
                            }

                            TemplateSpecLine.Traits.Add(trait.Trait);
                            trait.Added();
                        }

                        UpdateLayout();
                    };
                }


                UpdateLayout();

                Moved += delegate { UpdateLayout(); };
                Resized += delegate { UpdateLayout(); };
            }
            
            private void UpdateLayout()
            {
                AbsoluteBounds = new Rectangle(0,0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2)).Add(Location).Scale(Scale);

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

                spriteBatch.DrawOnCtrl(Parent,
                                        SelectorBounds.Add(new Point(-Location.X, -Location.Y)).Contains(RelativeMousePosition) ? _SpecSideSelector_Hovered : _SpecSideSelector,
                                        SelectorBounds,
                                        _SpecSideSelector.Bounds,
                                        Color.White,
                                        0f,
                                        Vector2.Zero
                                        );
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

        public Control_Build(Container parent, Template template)
        {
            Parent = parent;
            _Template = template;

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
                Specializations.Add(new Specialization_Control(Parent, Template, Template.Build.SpecLines[i].Specialization, new Point(5, Skillbar_Height + i * 134))
                {
                    Index = i,
                    ZIndex = ZIndex + 1,
                    Elite = i == 2,
                });
            }
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