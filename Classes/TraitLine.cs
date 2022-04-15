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

namespace Kenedia.Modules.BuildsManager
{
    public class TraitLineOG : Control
    {
        class TraitIcon {
            public List<int> SelectedTraits = new List<int>();
            public bool Loaded = false;
            public bool Hovered;
            public bool Selected
            {
                get
                {
                    return Trait != null && SelectedTraits.Contains((int)Trait.Id);
                }
            }
            public Point Point;
            private Texture2D _Texture;
            public Texture2D Texture
            {
                get { return _Texture; }
                set { _Texture = value.GetRegion(new Rectangle(3, 3, value.Width - 6, value.Height - 6)); }
            }
            public Rectangle Bounds;

            public GW2API.Trait Trait;
        }

        private int DEFAULT_FRAMEWIDTH = 1;
        private int DEFAULT_WIDTH = 643;
        private int DEFAULT_HEIGHT = 133;
        private int DEFAULT_HIGHLIGHTLEFT = 588;
        private int DEFAULT_MINORSIZE = 39;
        private int DEFAULT_MAJORSIZE = 38;
        public List<int> SelectedTraits 
        {
            get => _SelectedTraits;
            set
            {
                _SelectedTraits = value;
                foreach (TraitIcon trait in _MajorTraits)
                {
                    trait.SelectedTraits = SelectedTraits;
                }
            }
        }
        private List<int> _SelectedTraits = new List<int>();
        private List<TraitIcon> _MinorTraits = new List<TraitIcon>()
        {
            new TraitIcon()
            {
                Point = new Point(215, 47),
            },
            new TraitIcon()
            {
                Point = new Point(360, 47),
            },
            new TraitIcon()
            {
                Point = new Point(505, 47),
            },
        };
        private List<TraitIcon> _MajorTraits = new List<TraitIcon>()
        {
            new TraitIcon()
            {
                Point = new Point(285, 5),
            },
            new TraitIcon()
            {
                Point = new Point(285, 47),
            },
            new TraitIcon()
            {
                Point = new Point(285, 89),
            },

            new TraitIcon()
            {
                Point = new Point(430, 5),
            },
            new TraitIcon()
            {
                Point = new Point(430, 47),
            },
            new TraitIcon()
            {
                Point = new Point(430, 89),
            },

            new TraitIcon()
            {
                Point = new Point(575, 5),
            },
            new TraitIcon()
            {
                Point = new Point(575, 47),
            },
            new TraitIcon()
            {
                Point = new Point(575, 89),
            },
        };

        private double _Scale = 1;
        public double Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Size = new Point(DEFAULT_WIDTH.Scale(_Scale), DEFAULT_HEIGHT.Scale(_Scale));
            }
        }

        private GW2API.Specialization _Specialization;
        public GW2API.Specialization Specialization
        {
            get => _Specialization;
            set
            {
                SetProperty(ref _Specialization, value);

                var i = 0;
                Background = _Specialization.getBackground();

                foreach (int id in _Specialization.MinorTraits)
                {
                    GW2API.Trait trait = BuildsManager.Data.Traits.Find(e => e.Id == id);
                    if (trait != null)
                    {
                        var texture = trait.getIcon();

                        _MinorTraits[i].SelectedTraits = SelectedTraits;
                        _MinorTraits[i].Trait = trait;
                        _MinorTraits[i].Texture = texture;
                        _MinorTraits[i].Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                    }
                    BuildsManager.Logger.Debug("Minor Trait No. {0} has Trait: {1} and Name: {2}. Image is beeing loaded: {3}", i, trait != null, trait != null ? trait.Name : "None", _MinorTraits[i].Loaded);
                    i++;
                }

                i = 0;
                foreach (int id in _Specialization.MajorTraits)
                {
                    GW2API.Trait trait = BuildsManager.Data.Traits.Find(e => e.Id == id);
                    if (trait != null)
                    {
                        var texture = trait.getIcon();

                        _MajorTraits[i].SelectedTraits = SelectedTraits;
                        _MajorTraits[i].Trait = trait;
                        _MajorTraits[i].Texture = texture;
                        _MajorTraits[i].Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                    }
                    BuildsManager.Logger.Debug("Major Trait No. {0} has Trait: {1} and Name: {2}. Image is beeing loaded: {3}", i, trait != null, trait != null ? trait.Name : "None", _MajorTraits[i].Loaded);
                    i++;
                }
            }            
        }

        private Texture2D _PlaceHolderTexture;
        private Texture2D _EmptyTexture;

        private Texture2D _Line;
        private Texture2D _SpecHighlightFrame;
        private bool _BackgroundLoaded = false;
        private Texture2D _Background;
        public Texture2D Background
        {
            get => _Background;
            set 
            {
                var texture = value;
                if (texture != null && texture != _PlaceHolderTexture && texture != _EmptyTexture)
                {
                    texture = texture.GetRegion(new Rectangle(0, texture.Height - DEFAULT_HEIGHT, (texture.Width) - (texture.Width - DEFAULT_WIDTH), texture.Height - (texture.Height - DEFAULT_HEIGHT)));
                }

                //_BackgroundLoaded = !(value == _PlaceHolderTexture || value == _EmptyTexture);
                _BackgroundLoaded = false;
                SetProperty(ref _Background, texture);
            }
        }

        private List<Texture2D> _MinorTraits_Textures;
        public List<Texture2D> MinorTraits_Textures
        {
            get => _MinorTraits_Textures;
            set => SetProperty(ref _MinorTraits_Textures, value);
        }

        private List<Texture2D> _MajorTraits_Textures;
        public List<Texture2D> MajorTraits_Textures
        {
            get => _MajorTraits_Textures;
            set => SetProperty(ref _MajorTraits_Textures, value);
        }

        private Color _Tint = Color.White;
        public Color Tint
        {
            get => _Tint;
            set => SetProperty(ref _Tint, value);
        }

        public TraitLineOG()
        {
            Size = new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            _SpecHighlightFrame = BuildsManager.DataManager.getControlTexture(_Controls.SpecHighlight);
            _PlaceHolderTexture = BuildsManager.DataManager._Icons[(int) _Icons.Refresh];
            _EmptyTexture = BuildsManager.DataManager._Icons[(int) _Icons.Bug];
            _Line = BuildsManager.DataManager.getControlTexture(_Controls.Line);
            _Line = _Line.GetRegion(new Rectangle(22, 15, 85, 5));

            Click += delegate
            {
                var j = 0;
                // Major Traits
                foreach (TraitIcon major in _MajorTraits)
                {
                    if (major.Bounds.Contains(RelativeMousePosition))
                    {
                        if (major.Selected)
                        {
                           SelectedTraits.Remove((int)major.Trait.Id);

                        }
                        else
                        {
                            SelectedTraits.Add((int)major.Trait.Id);

                            for (int i = (j / 3) * 3; i < ((j / 3) + 1) * 3; i++)
                            {
                                if (_MajorTraits[i].Trait != major.Trait)
                                {
                                    BuildsManager.Logger.Debug("Selected Trait {0} - Other Trait {1}", major.Trait.Name, _MajorTraits[i].Trait.Name);
                                  if (SelectedTraits.Contains((int)_MajorTraits[i].Trait.Id)) SelectedTraits.Remove((int)_MajorTraits[i].Trait.Id);
                                }
                            }
                        }
                    };

                    j++;
                }
            };
        }

        private int Distance2D(Point p1, Point p2)
        {
            return (int) Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }
        
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Specialization == null) return;

            double factorX = ((double) Width / (double)DEFAULT_WIDTH);
            double factorY = ((double) Height / (double) DEFAULT_HEIGHT);

            if (!_BackgroundLoaded)
            {
                var texture = Specialization.getBackground();
                Background = texture;
                _BackgroundLoaded  = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
            };

            if (Background == null) return;


            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   new Rectangle(0, 0, Width, Height),
                                   _Background.Bounds,
                                   Color.Black,
                                   0f,
                                   Vector2.Zero
                                   );

            // Background
            spriteBatch.DrawOnCtrl(this,
                                   _BackgroundLoaded ? _Background : ContentService.Textures.Pixel,
                                   new Rectangle(DEFAULT_FRAMEWIDTH, DEFAULT_FRAMEWIDTH, Width - (DEFAULT_FRAMEWIDTH * 2), Height - (DEFAULT_FRAMEWIDTH * 2)),
                                   _Background.Bounds,
                                   _Tint,
                                   0f,
                                   Vector2.Zero
                                   );

            // Update Bounds and Update Images of Major & Minor Traits 
            foreach (TraitIcon minor in _MinorTraits)
            {
                minor.Bounds = new Rectangle(minor.Point.X.Scale(factorX), minor.Point.Y.Scale(factorY), DEFAULT_MAJORSIZE.Scale(factorX), DEFAULT_MAJORSIZE.Scale(factorY));
                minor.Hovered = minor.Bounds.Contains(RelativeMousePosition);

                if (!minor.Loaded && minor.Trait != null)
                {
                    BuildsManager.Logger.Debug("Loading Icon for '{0}'", minor.Trait.Name);

                    var texture = minor.Trait.getIcon();
                    minor.Texture = texture;
                    minor.Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                };
            }

            foreach (TraitIcon major in _MajorTraits)
            {
                major.Bounds = new Rectangle(major.Point.X.Scale(factorX), major.Point.Y.Scale(factorY), DEFAULT_MAJORSIZE.Scale(factorX), DEFAULT_MAJORSIZE.Scale(factorY));
                major.Hovered = major.Bounds.Contains(RelativeMousePosition);

                if (!major.Loaded && major.Trait != null)
                {
                    BuildsManager.Logger.Debug("Loading Icon for '{0}'", major.Trait.Name);

                    var texture = major.Trait.getIcon();
                    major.Texture = texture;
                    major.Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                };
            }


            // Draw Lines
            var first = _MinorTraits[0];
            spriteBatch.DrawOnCtrl(this,
                                   _Line,
                                   new Rectangle(Width -  18.Scale(factorX) -  DEFAULT_HIGHLIGHTLEFT.Scale(factorX) + _SpecHighlightFrame.Width.Scale(factorX), _SpecHighlightFrame.Bounds.Top.Scale(factorX) + (_SpecHighlightFrame.Bounds.Height / 2).Scale(factorX), Distance2D(new Point(_SpecHighlightFrame.Bounds.Right + _SpecHighlightFrame.Bounds.Left, _SpecHighlightFrame.Bounds.Top + (_SpecHighlightFrame.Bounds.Height / 2)), new Point(first.Bounds.Left - 18.Scale(factorX), first.Bounds.Top + (first.Bounds.Height / 2))).Scale(factorX), 5.Scale(factorX)),
                                   new Rectangle(0, 0, 25, 5),
                                   Color.White,
                                   0f,
                                   Vector2.Zero);

            var i = 0;
            foreach (TraitIcon major in _MajorTraits)
            {
                // Draw Line
                if (major.Selected)
                {
                    var minor = _MinorTraits[(i / 3)];
                    float rotation = 0f;
                    switch (i - ((i / 3) * 3))
                    {
                        case 0:
                            //rotation = 0.85f;
                            rotation = -(float)Math.PI / 4;
                            break;

                        case 1:
                            rotation = 0f;
                            break;

                        case 2:
                            //rotation = -0.9f;
                            rotation = (float)Math.PI / 4;
                            break;
                    }


                    spriteBatch.DrawOnCtrl(this,
                                           _Line,
                                           new Rectangle(minor.Bounds.Right, minor.Bounds.Top + (minor.Bounds.Height / 2), Distance2D(new Point(minor.Bounds.Right, minor.Bounds.Top + (minor.Bounds.Height / 2)), new Point(major.Bounds.Left, major.Bounds.Top + (major.Bounds.Height / 2))), 5.Scale(factorX)),
                                           new Rectangle(0, 0, 25, 5),
                                           Color.White,
                                           rotation,
                                           Vector2.Zero);


                    if ((i / 3) + 1 < _MinorTraits.Count)
                    {
                        minor = _MinorTraits[(i / 3) + 1];

                        spriteBatch.DrawOnCtrl(this,
                                               _Line,
                                               new Rectangle(major.Bounds.Right, major.Bounds.Top + (major.Bounds.Height / 2), Distance2D(new Point(major.Bounds.Right, major.Bounds.Top + (major.Bounds.Height / 2)), new Point(minor.Bounds.Left, minor.Bounds.Top + (minor.Bounds.Height / 2))), 5.Scale(factorX)),
                                               new Rectangle(0, 0, 25, 5),
                                               Color.White,
                                               -rotation,
                                               Vector2.Zero);

                    }
                }
                i++;
            }

            //Spec Highlight
            spriteBatch.DrawOnCtrl(this,
                                   _SpecHighlightFrame,
                                   new Rectangle(Width - DEFAULT_HIGHLIGHTLEFT.Scale(factorX), Height - _SpecHighlightFrame.Height.Scale(factorY), _SpecHighlightFrame.Width.Scale(factorX), _SpecHighlightFrame.Height.Scale(factorY)),
                                   _SpecHighlightFrame.Bounds,
                                   _Tint,
                                   0f,
                                   Vector2.Zero);

            // Minor Traits
            foreach (TraitIcon minor in _MinorTraits)
            {
                if (minor.Texture != null)
                {
                    spriteBatch.DrawOnCtrl(this,
                                           minor.Texture,
                                           minor.Bounds,
                                           new Rectangle(0, 0, minor.Texture.Width, minor.Texture.Height),
                                           Color.White,
                                           0f,
                                           Vector2.Zero);

                }
            }

            // Major Traits
            foreach (TraitIcon major in _MajorTraits)
            {
                if (major.Texture != null)
                {
                    spriteBatch.DrawOnCtrl(this,
                                       major.Texture,
                                       major.Bounds,
                                       new Rectangle(0, 0, major.Texture.Width, major.Texture.Height),
                                       major.Selected ? Color.White : major.Hovered ? Color.LightGray : Color.Gray,
                                       0f,
                                       Vector2.Zero);
                }
            }
        }
    }
}
