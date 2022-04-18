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

namespace Kenedia.Modules.BuildsManager
{
    public class Build : Control
    {
        public string newToolTip;
        class ConnectorLine
        {
            public Rectangle Bounds;
            public float Rotation = 0;
        }
        public class LoadingTexture
        {
            public Texture2D Texture;
            public bool Loaded;
        }

        class TraitIcon
        {
            private const int _TraitSize = 38;

            public TraitIcon(Point p)
            {
                DefaultPoint = p;
                DefaultBounds = new Rectangle(p.X, p.Y, _TraitSize, _TraitSize);
            }

            public List<GW2API.Trait> SelectedTraits = new List<GW2API.Trait>();
            public bool Loaded = false;
            public bool Hovered;
            public bool Selected
            {
                get
                {
                    return Trait != null && SelectedTraits.Contains(Trait);
                }
            }
            public Point DefaultPoint;
            public Point Point;
            private Texture2D _Texture;
            public Texture2D Texture
            {
                get { return _Texture; }
                set { _Texture = value.GetRegion(new Rectangle(3, 3, value.Width - 6, value.Height - 6)); }
            }
            public Rectangle Bounds;
            public Rectangle DefaultBounds;

            public ConnectorLine PreLine = new ConnectorLine();
            public ConnectorLine PostLine = new ConnectorLine();

            public GW2API.Trait Trait;
        }
        public class SpecializationLine
        {
            public bool Elite;
            public Build Parent;
            public SkillBar_Element SkillBar;

            public SpecializationLine(Build c)
            {
                Parent = c;

                _SpecSideSelector_Hovered = BuildsManager.DataManager.getControlTexture(_Controls.SpecSideSelector_Hovered);
                _SpecSideSelector = BuildsManager.DataManager.getControlTexture(_Controls.SpecSideSelector);

                _EliteFrame = BuildsManager.DataManager.getControlTexture(_Controls.EliteFrame).GetRegion(0, 4, 625, 130);
                _SpecHighlightFrame = BuildsManager.DataManager.getControlTexture(_Controls.SpecHighlight).GetRegion(12, 5, 103, 116);
                _SpecFrame = BuildsManager.DataManager.getControlTexture(_Controls.SpecFrame).GetRegion(0, 0, 647, 136);
                _PlaceHolderTexture = BuildsManager.DataManager._Icons[(int)_Icons.Refresh];
                _EmptyTexture = BuildsManager.DataManager._Icons[(int)_Icons.Bug];
                _Line = BuildsManager.DataManager.getControlTexture(_Controls.Line).GetRegion(new Rectangle(22, 15, 85, 5));
                _EmptyTraitLine = BuildsManager.DataManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
                Background = _EmptyTraitLine;

                DefaultHighlightBounds = new Rectangle(_Width - _HighlightLeft, (_Height - _SpecHighlightFrame.Height) / 2, _SpecHighlightFrame.Width, _SpecHighlightFrame.Height);
            }
            private const int _FrameWidth = 1;
            private const int _LineThickness = 5;
            public const int _Width = 643;
            public const int _Height = 133;
            private const int _HighlightLeft = 570;
            private const int _TraitSize = 38;
            private int _SpecSelectSize = 64;
            private const int _MiddleRowTop = (_Height - _TraitSize) / 2;

            private LoadingTexture _Background = new LoadingTexture();
            public Texture2D Background 
            { 
                get => _Background.Texture; 
                set
                {
                    _Background.Texture = value;
                    _Background.Loaded = (value != null) && (value != _PlaceHolderTexture) && (value != _EmptyTexture);

                    if (_Background.Loaded) _Background.Texture = _Background.Texture.GetRegion(0, value.Height - _Height, value.Width - (value.Width - _Width), value.Height - (value.Height - _Height));
                }
            }
            public bool SelectorActive = false;
            public bool CanClick = true;
            public bool isActive = false;

            private Texture2D _SpecFrame;
            private Texture2D _SpecHighlightFrame;
            private Texture2D _PlaceHolderTexture;
            private Texture2D _EmptyTexture;
            private Texture2D _EmptyTraitLine;
            private Texture2D _EliteFrame;
            private Texture2D _Line;
            private Texture2D _SpecSideSelector;
            private Texture2D _SpecSideSelector_Hovered;
            private Rectangle DefaultBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private Rectangle DefaultAbsoluteBounds = new Rectangle(0,0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2));
            private Rectangle DefaultSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, 15, _Height);
            private Rectangle DefaultSpecSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private Rectangle DefaultHighlightBounds;
            public Rectangle ControlBounds { get; private set; }

            private List<Rectangle> LineBounds = new List<Rectangle>();
            private Rectangle AbsoluteBounds = new Rectangle(0,0, _Width + _FrameWidth, _Height + _FrameWidth);
            private Rectangle ContentBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private Rectangle HighlightBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private Rectangle SelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private Rectangle SpecSelectorBounds = new Rectangle(_FrameWidth, _FrameWidth, _Width, _Height);
            private ConnectorLine FirstLine = new ConnectorLine();
            public Point Location 
            {
                get => _Location;
                set 
                {
                    _Location = value;
                    UpdateLayout(Point.Zero, Scale);
                }
            }
            private Point _Location;
            private GW2API.Profession _Profession;
            public GW2API.Profession Profession 
            {
                get => _Profession;
                set
                {
                    _Profession = value;
                    Specializations.Clear();

                    if (value != null)
                    {
                        foreach (int id in value.Specializations)
                        {
                            var spec = BuildsManager.Data.Specializations.Find(e => e.Id == id);
                            if (spec != null)
                            {
                                if (spec.Elite) EliteSpecs++;

                                var sIcon = spec.getIcon();
                                var s = new SpecializationLine_Specialization()
                                {
                                    Specialization = spec,
                                    _Texture = new LoadingTexture()
                                    {
                                        Texture = sIcon,
                                        Loaded = (sIcon != null) && (sIcon != _PlaceHolderTexture) && (sIcon != _EmptyTexture),
                                    }
                                };

                                Specializations.Add(s);
                            };
                        }
                    }
                }
            }

            class SpecializationLine_Specialization
            {
                public bool Hovered;
                public Texture2D Texture { 
                    get { return _Texture != null ? _Texture.Texture : null; } 
                }
                public LoadingTexture _Texture;
                public GW2API.Specialization Specialization;
                public Rectangle Bounds;
            }
            private List<SpecializationLine_Specialization> Specializations = new List<SpecializationLine_Specialization>();
            private int EliteSpecs = 0;

            private GW2API.Specialization _Specialization;
            public GW2API.Specialization Specialization
            {
                get => _Specialization;
                set
                {
                    _Specialization = value;
                    if (value != null)
                    {
                        _Profession = BuildsManager.Data.Professions.Find(e => e.Id == value.Profession);
                        if (_Profession != null)
                        {
                            Specializations.Clear();

                            foreach (int id in _Profession.Specializations)
                            {
                                var spec = BuildsManager.Data.Specializations.Find(e => e.Id == id);
                                if (spec != null)
                                {
                                    if (spec.Elite) EliteSpecs++;

                                    var sIcon = spec.getIcon();
                                    var s = new SpecializationLine_Specialization()
                                    {
                                        Specialization = spec,
                                        _Texture = new LoadingTexture()
                                        {
                                            Texture = sIcon,
                                            Loaded = (sIcon != null) && (sIcon != _PlaceHolderTexture) && (sIcon != _EmptyTexture),
                                        }
                                    };

                                    Specializations.Add(s);
                                };
                            }
                        }

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
                            i++;
                        }
                    }
                }
            }

            private List<GW2API.Trait> _SelectedTraits = new List<GW2API.Trait>();
            public List<GW2API.Trait> SelectedTraits
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
            private List<TraitIcon> _MinorTraits = new List<TraitIcon>()
        {
            new TraitIcon( new Point(215, _MiddleRowTop)),
            new TraitIcon(new Point(360, _MiddleRowTop)),
            new TraitIcon(new Point(505, _MiddleRowTop)),
        };
            private List<TraitIcon> _MajorTraits = new List<TraitIcon>()
        {
            new TraitIcon(new Point(285, _MiddleRowTop - 3 - _TraitSize)),
            new TraitIcon(new Point(285, _MiddleRowTop)),
            new TraitIcon(new Point(285, _MiddleRowTop + 3 + _TraitSize)),
            new TraitIcon(new Point(430, _MiddleRowTop - 3 - _TraitSize)),
            new TraitIcon(new Point(430, _MiddleRowTop)),
            new TraitIcon(new Point(430, _MiddleRowTop + 3 + _TraitSize)),
            new TraitIcon(new Point(575, _MiddleRowTop - 3 - _TraitSize)),
            new TraitIcon(new Point(575, _MiddleRowTop)),
            new TraitIcon(new Point(575, _MiddleRowTop + 3 + _TraitSize)),
        };

            private double _Scale = 1;
            public double Scale
            {
                get => _Scale;
                set
                {
                    _Scale = value;
                }
            }
            public bool Click(Point p)
            {
                if (CanClick)
                {
                    TraitIcon selectedTrait = null;
                    var i = 0;
                    foreach (TraitIcon trait in _MajorTraits)
                    {
                        if (trait.Hovered)
                            if (trait.SelectedTraits.Contains(trait.Trait))
                            {
                                trait.SelectedTraits.Remove(trait.Trait);
                            }
                            else
                            {
                                trait.SelectedTraits.Add(trait.Trait);
                                selectedTrait = trait;
                            };

                        i++;
                    }

                    if (selectedTrait != null && Specialization != null)
                    {
                        foreach (TraitIcon trait in _MajorTraits)
                        {
                            if (selectedTrait != trait && selectedTrait.Trait.Tier == trait.Trait.Tier)
                            {
                                if (trait.SelectedTraits.Contains(trait.Trait))
                                {
                                    trait.SelectedTraits.Remove(trait.Trait);
                                }
                            }
                        }

                        return true;
                    }

                    if (SelectorActive)
                    {
                        SelectorActive = false;

                        foreach (SpecializationLine_Specialization spec in Specializations)
                        {
                            if (spec.Hovered)
                            {
                                if (Specialization != spec.Specialization)
                                {
                                    if(Elite)
                                    {
                                        SkillBar.EliteSpecialization = (int) spec.Specialization.Id;
                                    }

                                    Specialization = spec.Specialization;
                                    SelectedTraits.Clear();
                                }

                                return true;
                            }
                        }
                    }

                    if (SelectorBounds.Contains(p) || HighlightBounds.Contains(p))
                    {
                        SelectorActive = !SelectorActive;
                        return true;
                    }
                }

                return false;
            }
            private double CalculeAngle(Point start, Point arrival)
            {
                var deltaX = Math.Pow((arrival.X - start.X), 2);
                var deltaY = Math.Pow((arrival.Y - start.Y), 2);

                var radian = Math.Atan2((arrival.Y - start.Y), (arrival.X - start.X));
                var angle = (radian * (180 / Math.PI) + 360) % 360;

                return angle;
            }

            public void UpdateTextures()
            {
                if (_Specialization != null)
                {
                    if (!_Background.Loaded) Background = _Specialization.getBackground();
                }
                foreach (TraitIcon trait in _MinorTraits)
                {
                    if (!trait.Loaded)
                    {
                        var texture = trait.Trait.getIcon();
                        trait.Texture = texture;
                        trait.Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                    }
                }
                foreach (TraitIcon trait in _MajorTraits)
                {
                    if (!trait.Loaded)
                    {
                        var texture = trait.Trait.getIcon();
                        trait.Texture = texture;
                        trait.Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                    }
                }
                foreach (SpecializationLine_Specialization spec in Specializations)
                {
                    if (!spec._Texture.Loaded)
                    {
                        var texture = spec.Specialization.getIcon();
                        spec._Texture.Texture = texture;
                        spec._Texture.Loaded = (texture != null) && (texture != _PlaceHolderTexture) && (texture != _EmptyTexture);
                    }
                }
            }
            public void UpdateLayout(Point p, double scale = default)
            {
                if (scale != default) _Scale = scale;
                ControlBounds = DefaultAbsoluteBounds.Add(Location).Scale(_Scale);
                AbsoluteBounds = DefaultAbsoluteBounds.Add(Location).Scale(_Scale);
                ContentBounds = DefaultBounds.Add(Location).Scale(_Scale);
                HighlightBounds = DefaultHighlightBounds.Add(Location).Scale(_Scale);
                SelectorBounds = DefaultSelectorBounds.Add(Location).Scale(_Scale);
                SpecSelectorBounds = DefaultSpecSelectorBounds.Add(Location).Scale(_Scale);

                FirstLine.Bounds = new Rectangle(HighlightBounds.Right - 5.Scale(_Scale), HighlightBounds.Center.Y, new Point(HighlightBounds.Right, HighlightBounds.Center.Y).Distance2D(_MinorTraits[0].Bounds.Center), _LineThickness.Scale(_Scale));

                foreach (TraitIcon trait in _MinorTraits)
                {
                    trait.Bounds = trait.DefaultBounds.Add(Location).Scale(_Scale);
                    trait.Hovered = !SelectorActive && trait.Bounds.Contains(p);
                    if (trait.Hovered && trait.Trait != null) Parent.newToolTip = trait.Trait.Name;
                }
                foreach (TraitIcon trait in _MajorTraits)
                {
                    trait.Bounds = trait.DefaultBounds.Add(Location).Scale(_Scale);
                    trait.Hovered = !SelectorActive && trait.Bounds.Contains(p);
                    if (trait.Hovered && trait.Trait != null) Parent.newToolTip = trait.Trait.Name;

                    //Lines
                    if (trait.Selected)
                    {
                        var minor = _MinorTraits[(int)trait.Trait.Tier - 1];
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

                        trait.PreLine.Bounds = new Rectangle(minor.Bounds.Center.X, minor.Bounds.Center.Y, minor.Bounds.Center.Distance2D(trait.Bounds.Center), _LineThickness.Scale(_Scale));

                        if (trait.Selected && trait.Trait.Tier != null && trait.Trait.Tier != 3)
                        {
                            minor = _MinorTraits[(int)trait.Trait.Tier];
                            trait.PostLine.Bounds = new Rectangle(trait.Bounds.Center.X, trait.Bounds.Center.Y, trait.Bounds.Center.Distance2D(minor.Bounds.Center), _LineThickness.Scale(_Scale));
                        }
                    }
                    else
                    {
                        trait.PreLine = new ConnectorLine();
                        trait.PostLine = new ConnectorLine();
                    }
                }

                var i = 0;
                var offset = (ContentBounds.Width - (Specializations.Count * (_SpecSelectSize + 5).Scale(_Scale))) / 2;
                foreach (SpecializationLine_Specialization spec in Specializations)
                {
                    spec.Bounds = new Rectangle(offset + i * (_SpecSelectSize + 5), (_Height - _SpecSelectSize) / 2, _SpecSelectSize, _SpecSelectSize).Add(Location).Scale(_Scale);
                    spec.Hovered = (!spec.Specialization.Elite || Elite) && spec.Bounds.Contains(p);
                    if (spec.Hovered && spec.Specialization != null && SelectorActive) Parent.newToolTip = spec.Specialization.Name;
                    i++;
                }
            }
            public void Paint(SpriteBatch spriteBatch, Rectangle bounds, Point p , double scale = default)
            {
                CanClick = !SkillBar.isActive;
                UpdateTextures();
                UpdateLayout(p,  scale);

                //Black Frame
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
                                        _Background.Loaded ? Background : ContentService.Textures.Pixel,
                                        ContentBounds,
                                        Background.Bounds,
                                        _Background.Loaded ? Color.White : Color.DarkGray,
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

                    foreach (TraitIcon trait in _MajorTraits)
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

                    //Minor Traits
                    foreach (TraitIcon trait in _MinorTraits)
                    {
                        spriteBatch.DrawOnCtrl(Parent,
                                               trait.Texture,
                                               trait.Bounds,
                                               trait.Loaded ? trait.Texture.Bounds : trait.Bounds,
                                               Color.White,
                                               0f,
                                               Vector2.Zero
                                               );
                    }

                    //Major Traits
                    foreach (TraitIcon trait in _MajorTraits)
                    {
                        spriteBatch.DrawOnCtrl(Parent,
                                                trait.Texture,
                                               trait.Bounds,
                                               trait.Loaded ? trait.Texture.Bounds : trait.Bounds,
                                               trait.Selected ? Color.White : (trait.Hovered ? Color.LightGray : Color.Gray),
                                               0f,
                                               Vector2.Zero
                                               );
                    }
                }

                //Elite Frame
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
                                       Specialization != null ? Color.White : new Color(32,32,32,125),
                                       0f,
                                       Vector2.Zero
                                       );

                if (SelectorActive)
                {
                    spriteBatch.DrawOnCtrl(Parent,
                                            ContentService.Textures.Pixel,
                                            SpecSelectorBounds,
                                            SpecSelectorBounds,
                                            new Color(0, 0, 0, 205),
                                            0f,
                                            Vector2.Zero
                                            );

                    foreach (SpecializationLine_Specialization spec in Specializations)
                    {
                        if (!spec.Specialization.Elite || Elite)
                        {
                            spriteBatch.DrawOnCtrl(Parent,
                                                    spec.Texture != null ? spec.Texture : ContentService.Textures.Pixel,
                                                    spec.Bounds,
                                                    spec.Texture != null ? spec.Texture.Bounds : new Rectangle(0, 0, _SpecSelectSize, _SpecSelectSize),
                                                    spec.Specialization == Specialization ? Color.White : spec.Hovered ? Color.LightGray : Color.Gray,
                                                    0f,
                                                    Vector2.Zero
                                                    );
                        }
                    }
                }

                spriteBatch.DrawOnCtrl(Parent,
                                        SelectorBounds.Contains(p) ? _SpecSideSelector_Hovered : _SpecSideSelector,
                                        !SelectorActive ? SelectorBounds : SelectorBounds.Add(new Point(_SpecSideSelector.Width, _SpecSideSelector.Height + 5.Scale(_Scale)).Scale(_Scale)),
                                        _SpecSideSelector.Bounds,
                                        Color.LightGray,
                                        !SelectorActive ? 0f : -(float)(Math.PI),
                                        Vector2.Zero
                                        );
            }
        }

        public class SkillBar_Element
        {
            public Rectangle ControlBounds { get; private set; }
            public class SkillElement
            {
                public Rectangle SelectorBounds;
                public Rectangle IdentifierBounds;
                public Rectangle Bounds;
                public GW2API.Skill Skill;
                public LoadingTexture Texture = new LoadingTexture();
                public bool Hovered = false;
                public bool availableInWater = true;
                public int Slot;
                public string Type;
                public List<SkillElement> List = new List<SkillElement>();
            }
            public class SkillBar_Skills
            {
                public List<SkillElement> SelectableSkills = new List<SkillElement>();
                public List<SkillElement> Profession = new List<SkillElement>();
                public List<SkillElement> Terrestial = new List<SkillElement>();
                public List<SkillElement> Aquatic = new List<SkillElement>();                   
            }

            public Build Parent;
            public List<SpecializationLine> SpecializationLines;

            private int _EliteSpecialization;
            public int EliteSpecialization
            {
                get => _EliteSpecialization;
                set
                {
                    _EliteSpecialization = value;
                    Skills.SelectableSkills.Clear();

                    foreach(SkillElement skill in Skills.Profession)
                    {
                        //Heal - Utility - Elite
                        if (skill.Skill.Slot.Value == 11 || skill.Skill.Slot.Value == 12 || skill.Skill.Slot.Value == 13)
                        {
                            if (skill.Skill.Specialization == null || skill.Skill.Specialization == value)
                            {

                                skill.Slot = skill.Skill.Slot.Value;
                                skill.availableInWater = skill.Skill.Flags.Find(e => e.RawValue == "NoUnderwater") == null;

                                Skills.SelectableSkills.Add(skill);    
                            }
                        }
                    }

                    foreach (SkillElement skill in Skills.Aquatic)
                    {
                        if(skill.Skill != null && Skills.SelectableSkills.Find(e => e.Skill.Id == skill.Skill.Id) == null)
                        {
                            skill.Skill = null;
                            skill.Texture.Loaded = true;
                            skill.Texture.Texture = _SkillPlaceHolder;
                        }
                    }

                    foreach (SkillElement skill in Skills.Terrestial)
                    {
                        if (skill.Skill != null && Skills.SelectableSkills.Find(e => e.Skill.Id == skill.Skill.Id) == null)
                        {
                            skill.Skill = null;
                            skill.Texture.Loaded = true;
                            skill.Texture.Texture = _SkillPlaceHolder;
                        }
                    }
                }
            }

            private GW2API.Profession _Profession;
            public GW2API.Profession Profession
            {
                get => _Profession;
                set
                {
                    _Profession = value;
                    Skills.Profession.Clear();

                    if (value != null)
                    {
                        foreach (GW2API.ProfessionSkill skill in value.Skills)
                        {
                            var s = BuildsManager.Data.Skills.Find(e => e.Id == skill.Id);
                            if (s != null)
                            {
                                Skills.Profession.Add(new SkillElement
                                {
                                    Skill = s,
                                    Slot = s.Slot.Value,
                                });
                            }
                        }

                        Skills.SelectableSkills.Clear();
                        foreach (SkillElement skill in Skills.Profession)
                        {
                            //Heal - Utility - Elite
                            if (skill.Skill.Slot.Value == 11 || skill.Skill.Slot.Value == 12 || skill.Skill.Slot.Value == 13)
                            {
                                if (skill.Skill.Specialization == null || skill.Skill.Specialization == _EliteSpecialization)
                                {
                                    skill.Slot = skill.Skill.Slot.Value;
                                    skill.availableInWater = skill.Skill.Flags.Find(e => e.RawValue == "NoUnderwater") == null;

                                    Skills.SelectableSkills.Add(skill);
                                }
                            }
                        }
                        var i = 0;

                        i = 0;
                        foreach (GW2API.Skill skill in Terrestial_Skills)
                        {
                            var slot = 0;
                            SkillElement skillElement = Skills.Profession.Find(e => e.Skill.Id == skill.Id);

                            if (skill.Slot.Value == 11) slot = 0;
                            if (skill.Slot.Value == 13) slot = 4;
                            if (skill.Slot.Value == 12) 
                            {
                                slot = 1 + i;
                                i++;                                   
                            };

                            if (skillElement != null)
                            {
                                Skills.Terrestial[slot].Skill = skillElement.Skill;
                                Skills.Terrestial[slot].Texture = skillElement.Texture;
                            }
                        }

                        i = 0;
                        foreach (GW2API.Skill skill in Aquatic_Skills)
                        {
                            var slot = 0;
                            SkillElement skillElement = Skills.Profession.Find(e => e.Skill.Id == skill.Id);

                            if (skill.Slot.Value == 11) slot = 0;
                            if (skill.Slot.Value == 13) slot = 4;
                            if (skill.Slot.Value == 12)
                            {
                                slot = 1 + i;
                                i++;
                            };

                            if (skillElement != null)
                            {
                                Skills.Aquatic[slot].Skill = skillElement.Skill;
                                Skills.Aquatic[slot].Texture = skillElement.Texture;
                            }
                        }
                    }
                }
            }

            public SkillBar_Skills Skills = new SkillBar_Skills();
            public List<GW2API.Skill> _Terrestial_Skills = new List<GW2API.Skill>();
            public List<GW2API.Skill> Terrestial_Skills 
            {
                get => _Terrestial_Skills;
                set
                {
                    _Terrestial_Skills = value;

                    if (value == null || value.Count == 0)
                    {
                        foreach (SkillElement skill in Skills.Terrestial)
                        {
                            skill.Skill = null;
                            skill.Texture.Loaded = true;
                            skill.Texture.Texture = _SkillPlaceHolder;
                        }
                    }
                }
            }
            private List<GW2API.Skill> _Aquatic_Skills = new List<GW2API.Skill>();
            public List<GW2API.Skill> Aquatic_Skills
            {
                get => _Aquatic_Skills;
                set
                {
                    _Aquatic_Skills = value;

                    if (value == null || value.Count == 0)
                    {
                        foreach (SkillElement skill in Skills.Aquatic)
                        {
                            skill.Skill = null;
                            skill.Texture.Loaded = true;
                            skill.Texture.Texture = _SkillPlaceHolder;
                        }
                    }
                }
            }

            private Texture2D _AquaTexture;
            private Texture2D _TerrestialTexture;
            private Texture2D _PlaceHolderTexture;
            private Texture2D _EmptyTexture;
            private Texture2D _SelectorTexture;
            private Texture2D _SelectorTextureHovered;
            private Texture2D _SkillSelectorBackground;
            private Texture2D _SkillPlaceHolder;
            private Texture2D _NoWaterTexture;
            private Texture2D _Selector;

            private SkillElement SkillSelector_Active;
            private List<SkillElement> SkillSelector_List;
            
            public Point Location = Point.Zero;
            private double _Scale = 1;
            private int _IdenticatorSize = 48;

            private int _FrameWidth = 1;
            private const int _Width = 643;
            private const int _Height = 105;
            private int _SkillSize = 60;

            public Rectangle Default_Rectangle { get; private set; }

            private Rectangle Default_SkillSelectRectangle;
            private Rectangle SkillSelectRectangle;
            private Rectangle SkillSelectBackground;

            private Rectangle Default_TerrestialBar;
            private Rectangle TerrestialBar_Rectangle;
            private Rectangle TerrestialBar_IconRectangle;
            private Rectangle TerrestialBar_TextRectangle;

            private Rectangle Default_WaterBar;
            private Rectangle WaterBar_Rectangle;
            private Rectangle WaterBar_IconRectangle;
            private Rectangle WaterBar_TextRectangle;

            public bool isActive;
            public bool CanClick = true;

            public SkillBar_Element(Build control)
            {
                Parent = control;
                Default_Rectangle = new Rectangle(0, 0, _Width + (_FrameWidth * 2), _Height + (_FrameWidth * 2));

                var subBar_Width = (Default_Rectangle.Width - 15) / 2;
                Default_WaterBar = new Rectangle(0, 0, subBar_Width, Default_Rectangle.Height);
                Default_TerrestialBar = new Rectangle(subBar_Width + 22, 0, subBar_Width, Default_Rectangle.Height);
                Default_SkillSelectRectangle = new Rectangle(0, 0, 250, 350);

                _TerrestialTexture = BuildsManager.DataManager.getControlTexture(_Controls.Land);
                _AquaTexture = BuildsManager.DataManager.getControlTexture(_Controls.Water);
                _PlaceHolderTexture = BuildsManager.DataManager.getIcon(_Icons.Refresh);
                _EmptyTexture = BuildsManager.DataManager.getIcon(_Icons.Bug);
                _SelectorTexture = BuildsManager.DataManager.getControlTexture(_Controls.SkillSelector).GetRegion(0, 2, 64, 12);
                _SelectorTextureHovered = BuildsManager.DataManager.getControlTexture(_Controls.SkillSelector_Hovered);
                _SkillSelectorBackground = BuildsManager.DataManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
                _SkillPlaceHolder = BuildsManager.DataManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 128, 128);
                _NoWaterTexture = BuildsManager.DataManager.getControlTexture(_Controls.NoWaterTexture).GetRegion(16, 16, 96, 96);
                _Selector = BuildsManager.DataManager.getControlTexture(_Controls.Selector);

                _IdenticatorSize = 32;

                Skills.Profession.Add(new SkillElement
                {
                    Skill = new GW2API.Skill() 
                    {
                        Name = "Unkown",
                        Id = 0,
                        PaletteID = 0,
                        Icon = new GW2API.Icon()
                        {
                            Url = "https://assets.gw2dat.com/961372.png"
                        },
                        Slot = new GW2API.Slot()
                        {
                            Value = 0,
                        }
                    },
                    Texture = new LoadingTexture()
                    {
                        Loaded = true,
                        Texture = _SkillPlaceHolder,
                    }
                });
                for (int i = 0; i < 5; i++)
                {
                    var slot = 0;

                    switch (i)
                    {
                        case 0: //Heal
                            slot = 11;
                            break;

                        case 4: //Elite
                            slot = 13;
                            break;

                        default: //Utility
                            slot = 12;
                            break;
                    }

                    Skills.Terrestial.Add(new SkillElement()
                    {
                        Slot = slot,
                        Texture = new LoadingTexture()
                        {
                            Loaded = true,
                            Texture = _SkillPlaceHolder,
                        }
                    });
                    Skills.Aquatic.Add(new SkillElement()
                    {
                        Slot = slot,
                        Texture = new LoadingTexture()
                        {
                            Loaded = true,
                            Texture = _SkillPlaceHolder,
                        }
                    });
                }
            }
            public bool Click()
            {
                if (CanClick)
                {
                    foreach (SkillElement skill in Skills.Aquatic)
                    {
                        if (skill.Hovered)
                        {
                            if (SkillSelector_Active == skill)
                            {
                                SkillSelector_Active = null;
                            }
                            else
                            {
                                SkillSelector_Active = skill;
                                SkillSelector_List = Skills.Aquatic;

                                Skills.SelectableSkills.Clear();
                                foreach (SkillElement sSkill in Skills.Profession)
                                {
                                    //Heal - Utility - Elite
                                    if (sSkill.Skill.Slot.Value == skill.Slot)
                                    {
                                        if (sSkill.Skill.Specialization == null || sSkill.Skill.Specialization == _EliteSpecialization)
                                        {
                                            sSkill.Slot = sSkill.Skill.Slot.Value;
                                            sSkill.availableInWater = sSkill.Skill.Flags.Find(e => e.RawValue == "NoUnderwater") == null;

                                            Skills.SelectableSkills.Add(sSkill);
                                        }
                                    }
                                }
                            }
                            return true;
                        }
                    }

                    foreach (SkillElement skill in Skills.Terrestial)
                    {
                        if (skill.Hovered)
                        {
                            if (SkillSelector_Active == skill)
                            {
                                SkillSelector_Active = null;
                            }
                            else
                            {
                                SkillSelector_Active = skill;
                                SkillSelector_List = Skills.Terrestial;

                                Skills.SelectableSkills.Clear();
                                foreach (SkillElement sSkill in Skills.Profession)
                                {
                                    //Heal - Utility - Elite
                                    if (sSkill.Skill.Slot.Value == skill.Slot)
                                    {
                                        if (sSkill.Skill.Specialization == null || sSkill.Skill.Specialization == _EliteSpecialization)
                                        {
                                            sSkill.Slot = sSkill.Skill.Slot.Value;
                                            sSkill.availableInWater = sSkill.Skill.Flags.Find(e => e.RawValue == "NoUnderwater") == null;

                                            Skills.SelectableSkills.Add(sSkill);
                                        }
                                    }
                                }
                            }

                            return true;
                        }
                    }

                    if (SkillSelector_Active != null)
                    {
                        bool isLand = SkillSelector_List == Skills.Terrestial;

                        SkillElement editedSkill = null;
                        foreach (SkillElement skill in Skills.SelectableSkills)
                        {
                            if (skill.Hovered && (isLand || skill.availableInWater))
                            {
                                SkillSelector_Active.Skill = skill.Skill;
                                SkillSelector_Active.Texture = new LoadingTexture();
                                editedSkill = SkillSelector_Active;
                            }
                        }

                        SkillSelector_Active = null;

                        if (editedSkill != null)
                        {
                            foreach (SkillElement skill in SkillSelector_List)
                            {
                                if (skill != editedSkill && skill.Skill != null && skill.Skill.Id == editedSkill.Skill.Id)
                                {
                                    skill.Skill = null;
                                    skill.Texture = new LoadingTexture()
                                    {
                                        Loaded = true,
                                        Texture = _SkillPlaceHolder,
                                    };
                                }
                            }
                            return true;
                        }
                    }
                }       

                return false;
            }

            private void UpdateLayout(Point p)
            {
                int i;
                var size = _SkillSize;
                ControlBounds = Default_Rectangle.Add(Location).Scale(_Scale);

                i = 0;
                TerrestialBar_Rectangle = Default_TerrestialBar.Add(Location).Scale(_Scale);
                TerrestialBar_IconRectangle = new Rectangle(Default_TerrestialBar.X, Default_TerrestialBar.Y, _IdenticatorSize, _IdenticatorSize).Add(Location).Scale(_Scale);
                TerrestialBar_TextRectangle = new Rectangle(Default_TerrestialBar.X + _IdenticatorSize + 10, Default_TerrestialBar.Y, Default_TerrestialBar.Width - (_IdenticatorSize + 10), _IdenticatorSize).Add(Location).Scale(_Scale);

                foreach (SkillElement skill in Skills.Terrestial)
                {
                    skill.Bounds = new Rectangle(Default_TerrestialBar.X + i * (size + (_FrameWidth *2)), Default_TerrestialBar.Y + (Default_TerrestialBar.Height - size), size, size).Add(Location).Scale(_Scale);
                    skill.SelectorBounds = new Rectangle(Default_TerrestialBar.X + i * (size + (_FrameWidth * 2)), (Default_TerrestialBar.Height - size) - 10, size, 10).Add(Location).Scale(_Scale);
                    skill.Hovered = skill.Bounds.Contains(p) || skill.SelectorBounds.Contains(p);
                    if (skill.Hovered && skill.Skill != null) Parent.newToolTip = skill.Skill.Name;

                    i++;
                }

                i = 0;
                WaterBar_Rectangle = Default_WaterBar.Add(Location).Scale(_Scale);
                WaterBar_IconRectangle = new Rectangle(Default_WaterBar.X, Default_WaterBar.Y, _IdenticatorSize, _IdenticatorSize).Add(Location).Scale(_Scale);
                WaterBar_TextRectangle = new Rectangle(Default_WaterBar.X + _IdenticatorSize + 10, Default_WaterBar.Y, Default_WaterBar.Width - (_IdenticatorSize + 10), _IdenticatorSize).Add(Location).Scale(_Scale);

                foreach (SkillElement skill in Skills.Aquatic)
                {
                    skill.Bounds = new Rectangle(Default_WaterBar.X + i * (size + (_FrameWidth * 2)), Default_WaterBar.Y + (Default_WaterBar.Height - size), size, size).Add(Location).Scale(_Scale);
                    skill.SelectorBounds = new Rectangle(Default_WaterBar.X + i * (size + (_FrameWidth * 2)), (Default_WaterBar.Height - size) - 10, size, 10).Add(Location).Scale(_Scale);
                    skill.Hovered = skill.Bounds.Contains(p) || skill.SelectorBounds.Contains(p);
                    if (skill.Hovered && skill.Skill != null) Parent.newToolTip = skill.Skill.Name;
                    i++;
                }

                isActive = SkillSelector_Active != null;
                if (SkillSelector_Active != null)
                {
                    var ImgSize = _SkillSize.Scale(_Scale);
                    var ImgGap = 2.Scale(_Scale);

                    var rows = Math.Min(Skills.SelectableSkills.Count, 6);
                    var columns = (int)Math.Ceiling((double)Skills.SelectableSkills.Count / (double)rows);
                    rows = Skills.SelectableSkills.Count / columns;

                    var width = (columns)* (ImgSize + ImgGap);
                    var loc = new Point(Math.Min(SkillSelector_Active.Bounds.X, _Width.Scale(_Scale) - width), SkillSelector_Active.Bounds.Bottom);

                    //BuildsManager.Logger.Debug("X: {0}, Width - width {1}", Default_SkillSelectRectangle.Scale(_Scale).X, Default_SkillSelectRectangle.Scale(_Scale).Right - 5 - width);

                    SkillSelectRectangle = new Rectangle(Default_SkillSelectRectangle.Scale(_Scale).X, Default_SkillSelectRectangle.Scale(_Scale).Y, ImgGap + columns * (ImgSize + ImgGap), ImgGap + rows * (ImgSize + ImgGap)).Add(loc);
                    SkillSelectBackground = new Rectangle(0, _Height + 5.Scale(_Scale), _Width + 5.Scale(_Scale), 5 + SpecializationLine._Height * 3).Add(Location).Scale(_Scale);

                    var row = 0;
                    var column = -1;
                    //var pos = new Point(SkillSelector_Active.Bounds.X, SkillSelector_Active.Bounds.Y);

                    foreach (SkillElement skill in Skills.SelectableSkills)
                    {
                        column++;
                        //var rect = new Rectangle(2 + column * (_SkillSize + 2), SkillSelectRectangle.Top - 2 + (row * (_SkillSize + 2)), _SkillSize, _SkillSize).Add(Location).Add(pos).Scale(_Scale);
                        var rect = new Rectangle(SkillSelectRectangle.X + ImgGap + column * (ImgSize + ImgGap), ImgGap + SkillSelectRectangle.Y + (row * (ImgSize + ImgGap)), ImgSize, ImgSize);
                        //var rect = new Rectangle(column * (_SkillSize.Scale(_Scale)) + Location.X + pos.X, (row * _SkillSize.Scale(_Scale)) + Location.Y + pos.Y, _SkillSize.Scale(_Scale), _SkillSize.Scale(_Scale));

                        if (!SkillSelectRectangle.Contains(rect))
                        {
                            column = 0;
                            row++;

                            // rect = new Rectangle(2 + column * (_SkillSize + 2), SkillSelectRectangle.Top - 2 + (row * _SkillSize), _SkillSize, _SkillSize).Add(Location).Add(pos).Scale(_Scale);
                            rect = new Rectangle(SkillSelectRectangle.X + ImgGap + column * (ImgSize + ImgGap), ImgGap + SkillSelectRectangle.Y + (row * (ImgSize + ImgGap)), ImgSize, ImgSize);
                        }

                        skill.Bounds = rect;
                        skill.Hovered = rect.Contains(p);
                        if (skill.Hovered && skill.Skill != null) Parent.newToolTip = skill.Skill.Name;
                    }
                }
            }
            private void UpdateTextures()
            {
                foreach (SkillElement skill in Skills.Terrestial)
                {
                    if (!skill.Texture.Loaded)
                    {
                        var texture = skill.Skill.getIcon();
                        skill.Texture.Texture = texture;

                        var loaded = (texture != _PlaceHolderTexture && texture != _EmptyTexture);
                        if (loaded)
                        {
                            skill.Texture.Texture = skill.Texture.Texture.GetRegion(12, 12, texture.Width - (12 * 2), texture.Height - (12 * 2));
                        }

                        skill.Texture.Loaded = loaded;
                    }
                }

                foreach (SkillElement skill in Skills.Aquatic)
                {
                    if (!skill.Texture.Loaded)
                    {
                        var texture = skill.Skill.getIcon();
                        skill.Texture.Texture = texture;

                        var loaded = (texture != _PlaceHolderTexture && texture != _EmptyTexture);
                        if (loaded)
                        {
                            skill.Texture.Texture = skill.Texture.Texture.GetRegion(12, 12, texture.Width - (12 * 2), texture.Height - (12 * 2));
                        }

                        skill.Texture.Loaded = loaded;
                    }
                }

                if (SkillSelector_Active != null)
                {
                    foreach (SkillElement skill in Skills.SelectableSkills)
                    {
                        if (!skill.Texture.Loaded)
                        {
                            var texture = skill.Skill.getIcon();
                            skill.Texture.Texture = texture;

                            var loaded = (texture != _PlaceHolderTexture && texture != _EmptyTexture);
                            if (loaded)
                            {
                                skill.Texture.Texture = skill.Texture.Texture.GetRegion(12, 12, texture.Width - (12 * 2), texture.Height - (12 * 2));
                            }

                            skill.Texture.Loaded = loaded;
                        }
                    }
                }
            }

            public void Paint(SpriteBatch spriteBatch, Rectangle bounds, Point p, double scale = default)
            {
                CanClick = SpecializationLines.Find(e => e.isActive) == null;
                if (scale != default) _Scale = scale;
                UpdateTextures();
                UpdateLayout(p);

                MonoGame.Extended.BitmapFonts.BitmapFont font = null;
                var cnt = new ContentService();

                var fontSizes = Enum.GetValues(typeof(ContentService.FontSize));

                for (int i = fontSizes.Length - 1; i >= 0; i--)
                {
                    var size = fontSizes.GetValue(i);
                    font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)size, ContentService.FontStyle.Regular);

                  //  BuildsManager.Logger.Debug("Line Height: {0}; Font Size {1}; Rectanlge Height {2}", font.LineHeight, (int)size, TerrestialBar_TextRectangle.Height);

                    if (font.LineHeight <= TerrestialBar_TextRectangle.Height)
                    {
                        break;
                    }
                }

                // Land Skills
                spriteBatch.DrawOnCtrl(Parent,
                                       ContentService.Textures.Pixel,
                                       TerrestialBar_Rectangle,
                                       TerrestialBar_Rectangle,
                                       Color.Transparent,
                                       0f,
                                       Vector2.Zero
                                       );

                spriteBatch.DrawOnCtrl(Parent,
                                       _TerrestialTexture,
                                       TerrestialBar_IconRectangle,
                                       _TerrestialTexture.Bounds,
                                       Color.White,
                                       0f,
                                       Vector2.Zero
                                       );

                spriteBatch.DrawOnCtrl(Parent,
                                       _TerrestialTexture,
                                       TerrestialBar_IconRectangle,
                                       _TerrestialTexture.Bounds,
                                       Color.White,
                                       0f,
                                       Vector2.Zero
                                       );

                spriteBatch.DrawStringOnCtrl(Parent,
                                       "Terrestial Skills",
                                       font,
                                       TerrestialBar_TextRectangle,
                                       Color.White
                                       );



                foreach (SkillElement skill in Skills.Terrestial)
                {
                    spriteBatch.DrawOnCtrl(Parent,
                                           skill.Texture.Texture,
                                           skill.Bounds,
                                           skill.Texture.Texture.Bounds,
                                           skill.Texture.Texture == _SkillPlaceHolder ? Color.DarkGray : Color.White,
                                           0f,
                                           Vector2.Zero
                                           );

                    spriteBatch.DrawOnCtrl(Parent,
                                           skill.Hovered ? _SelectorTextureHovered : _SelectorTexture,
                                           skill.SelectorBounds,
                                           _SelectorTexture.Bounds,
                                           Color.White,
                                           0f,
                                           Vector2.Zero
                                           );

                    if (SkillSelector_Active == skill)
                    {
                        spriteBatch.DrawOnCtrl(Parent,
                                               _Selector,
                                               skill.Bounds,
                                               _Selector.Bounds,
                                               Color.White,
                                               0f,
                                               Vector2.Zero
                                               );
                    }
                }

                // Water Skills
                spriteBatch.DrawOnCtrl(Parent,
                                       ContentService.Textures.Pixel,
                                       WaterBar_Rectangle,
                                       WaterBar_Rectangle,
                                       Color.Transparent,
                                       0f,
                                       Vector2.Zero
                                       );

                spriteBatch.DrawOnCtrl(Parent,
                                       _AquaTexture,
                                       WaterBar_IconRectangle,
                                       _AquaTexture.Bounds,
                                       Color.White,
                                       0f,
                                       Vector2.Zero
                                       );

                spriteBatch.DrawStringOnCtrl(Parent,
                                       "Aquatic Skills",
                                       font,
                                       WaterBar_TextRectangle,
                                       Color.White
                                       );

                foreach (SkillElement skill in Skills.Aquatic)
                {

                    spriteBatch.DrawOnCtrl(Parent,
                                           skill.Texture.Texture,
                                           skill.Bounds,
                                           skill.Texture.Texture.Bounds,
                                           skill.Texture.Texture == _SkillPlaceHolder ? Color.DarkGray: Color.White,
                                           0f,
                                           Vector2.Zero
                                           );

                    spriteBatch.DrawOnCtrl(Parent,
                                           skill.Hovered ? _SelectorTextureHovered : _SelectorTexture,
                                           skill.SelectorBounds,
                                           _SelectorTexture.Bounds,
                                           Color.White,
                                           0f,
                                           Vector2.Zero
                                           );
                    if (SkillSelector_Active == skill)
                    {
                        spriteBatch.DrawOnCtrl(Parent,
                                               _Selector,
                                               skill.Bounds,
                                               _Selector.Bounds,
                                               Color.White,
                                               0f,
                                               Vector2.Zero
                                               );
                    }
                }

                if (SkillSelector_Active != null)
                {
                    var isLand = SkillSelector_List == Skills.Terrestial;
                    
                    spriteBatch.DrawOnCtrl(Parent,
                                           ContentService.Textures.Pixel,
                                           SkillSelectBackground,
                                           SkillSelectBackground,
                                           new Color(0, 0, 0, 175),
                                           0f,
                                           Vector2.Zero
                                           );

                    spriteBatch.DrawOnCtrl(Parent,
                                           ContentService.Textures.Pixel,
                                           SkillSelectRectangle.Add(-2, -2, 4, 4),
                                           _SkillSelectorBackground.Bounds,
                                           Color.Black,
                                           0f,
                                           Vector2.Zero
                                           );

                    spriteBatch.DrawOnCtrl(Parent,
                                           _SkillSelectorBackground,
                                           SkillSelectRectangle,
                                           _SkillSelectorBackground.Bounds,
                                           Color.DarkGray,
                                           0f,
                                           Vector2.Zero
                                           );


                    foreach (SkillElement skill in Skills.SelectableSkills)
                    {

                        spriteBatch.DrawOnCtrl(Parent,
                                               skill.Texture.Texture,
                                               skill.Bounds,
                                               skill.Texture.Texture.Bounds,
                                               (isLand || skill.availableInWater) ? Color.White : Color.LightGray,
                                               0f,
                                               Vector2.Zero
                                               );

                        if (! (isLand || skill.availableInWater))
                        {
                            spriteBatch.DrawOnCtrl(Parent,
                                                   _NoWaterTexture,
                                                   skill.Bounds,
                                                  _NoWaterTexture.Bounds,
                                                  Color.White,
                                                   0f,
                                                   Vector2.Zero
                                                   );
                        }
                    }                
                }
            }
        }

        private int FrameThickness  = 1;
        private int Gap  = 5;
        private int Build_Width = 900;
        private int Skillbar_Height = 110;
        private int Build_Height = 125 + (150 * 3);
        public Rectangle ControlBounds { get; private set; }

        private SkillBar_Element SkillBar;
        private List<SpecializationLine> SpecializationsLines;
        private Texture2D _PlaceHolderTexture;
        private Texture2D _EmptyTexture;
        private Texture2D _Line;
        private Texture2D _SpecHighlightFrame;

        private TextBox TemplateBox;

        private double _Scale = 1;
        public double Scale
        {
            get => _Scale;
            set
            {
                _Scale = value;
                Size = new Point(Build_Width.Scale(_Scale), Build_Height.Scale(_Scale));
            }
        }
        private BuildTemplate _BuildTemplate = new BuildTemplate("[&DQIEKRYqPTlwAAAAogEAAGoAAACvAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]");
        public BuildTemplate BuildTemplate
        {
            get => _BuildTemplate;
            set
            {
                _BuildTemplate = value;
                _BuildTemplateCode = value != null ? value.TemplateCode : _BuildTemplateCode;
                SetTemplate();
            }
        }
        private string _BuildTemplateCode;
        public string ParsedBuildTemplateCode = "";
        public string BuildTemplateCode
        {
            get => _BuildTemplateCode;
            set
            {
                ParsedBuildTemplateCode = value;
                _BuildTemplateCode = value;
                _BuildTemplate = new BuildTemplate(value);
                SetTemplate();
            }
        }


        public Build()
        {
            Size = new Point(Build_Width, Build_Height);
            SkillBar = new SkillBar_Element(this) { Location = new Point(5, 5)};

            SpecializationsLines = new List<SpecializationLine>();
            SpecializationsLines.AddRange(new SpecializationLine[] {
                new SpecializationLine(this){ Location = new Point(5, Skillbar_Height + Gap + 0), SkillBar = SkillBar},
                new SpecializationLine(this){ Location = new Point(5, Skillbar_Height + Gap + 134), SkillBar = SkillBar},
                new SpecializationLine(this){ Location = new Point(5, Skillbar_Height + Gap + 134 * 2), SkillBar = SkillBar},
            });
            SkillBar.SpecializationLines = SpecializationsLines;

            Click += delegate
            {
                SpecializationLine editedLine = null;

                foreach ( SpecializationLine specialization in SpecializationsLines)
                {
                    var oldSpec = specialization.Specialization;

                    if (specialization.Click(RelativeMousePosition)) OnTemplateChange();
                    if (oldSpec != specialization.Specialization)
                    {
                        editedLine = specialization;
                        if (specialization.Specialization.Elite) SkillBar.EliteSpecialization = (int) specialization.Specialization.Id;
                    }
                }

                if (editedLine != null)
                {
                    foreach (SpecializationLine specialization in SpecializationsLines)
                    {
                        if (specialization != editedLine && specialization.Specialization == editedLine.Specialization) specialization.Specialization = null;
                    }
                }

                if (SkillBar.Click()) OnTemplateChange();
            };

            ControlBounds = new Rectangle(SkillBar.Location.X, SkillBar.Location.Y, Math.Max(SpecializationsLines[0].ControlBounds.Width, SkillBar.ControlBounds.Width), SpecializationsLines[2].ControlBounds.Bottom - SkillBar.ControlBounds.Top);
        }

        public event EventHandler TemplateChanged;
        public void OnTemplateChange()
        {
            UpdateTemplate();
            this.TemplateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetTemplate()
        {
            SpecializationsLines[0].Specialization = BuildTemplate.SpecLines[0].Specialization;
            SpecializationsLines[0].SelectedTraits = BuildTemplate.SpecLines[0].Traits;
            SpecializationsLines[0].Profession = BuildsManager.Data.Professions.Find(e => e.Id == BuildTemplate.Profession.ToString());

            SpecializationsLines[1].Specialization = BuildTemplate.SpecLines[1].Specialization;
            SpecializationsLines[1].SelectedTraits = BuildTemplate.SpecLines[1].Traits;
            SpecializationsLines[1].Profession = BuildsManager.Data.Professions.Find(e => e.Id == BuildTemplate.Profession.ToString());

            SpecializationsLines[2].Specialization = BuildTemplate.SpecLines[2].Specialization;
            SpecializationsLines[2].SelectedTraits = BuildTemplate.SpecLines[2].Traits;
            SpecializationsLines[2].Profession = BuildsManager.Data.Professions.Find(e => e.Id == BuildTemplate.Profession.ToString());
            SpecializationsLines[2].Elite = true;

            SkillBar.EliteSpecialization = BuildTemplate.SpecLines[2].Specialization != null ? (int) BuildTemplate.SpecLines[2].Specialization.Id : 0;
            SkillBar.Terrestial_Skills = BuildTemplate.UtilitySkills_Land;
            SkillBar.Aquatic_Skills = BuildTemplate.UtilitySkills_Water;

            SkillBar.Profession = BuildsManager.Data.Professions.Find(e => e.Id == BuildTemplate.Profession.ToString());
        }

        public void UpdateTemplate()
        {
            BuildChatLink build = new BuildChatLink();

            build.Profession = (Gw2Sharp.Models.ProfessionType)Enum.Parse(typeof(Gw2Sharp.Models.ProfessionType), BuildTemplate.Profession.ToString());
            build.AquaticHealingSkillPaletteId = SkillBar.Skills.Aquatic[0].Skill != null && SkillBar.Skills.Aquatic[0].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Aquatic[0].Skill.PaletteID : (ushort)0;
            build.AquaticUtility1SkillPaletteId = SkillBar.Skills.Aquatic[1].Skill != null && SkillBar.Skills.Aquatic[1].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Aquatic[1].Skill.PaletteID : (ushort)0;
            build.AquaticUtility2SkillPaletteId = SkillBar.Skills.Aquatic[2].Skill != null && SkillBar.Skills.Aquatic[2].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Aquatic[2].Skill.PaletteID : (ushort)0;
            build.AquaticUtility3SkillPaletteId = SkillBar.Skills.Aquatic[3].Skill != null && SkillBar.Skills.Aquatic[3].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Aquatic[3].Skill.PaletteID : (ushort)0;
            build.AquaticEliteSkillPaletteId = SkillBar.Skills.Aquatic[4].Skill != null && SkillBar.Skills.Aquatic[4].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Aquatic[4].Skill.PaletteID : (ushort)0;

            build.TerrestrialHealingSkillPaletteId = SkillBar.Skills.Terrestial[0].Skill != null && SkillBar.Skills.Terrestial[0].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Terrestial[0].Skill.PaletteID : (ushort)0;
            build.TerrestrialUtility1SkillPaletteId = SkillBar.Skills.Terrestial[1].Skill != null && SkillBar.Skills.Terrestial[1].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Terrestial[1].Skill.PaletteID : (ushort)0;
            build.TerrestrialUtility2SkillPaletteId = SkillBar.Skills.Terrestial[2].Skill != null && SkillBar.Skills.Terrestial[2].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Terrestial[2].Skill.PaletteID : (ushort)0;
            build.TerrestrialUtility3SkillPaletteId = SkillBar.Skills.Terrestial[3].Skill != null && SkillBar.Skills.Terrestial[3].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Terrestial[3].Skill.PaletteID : (ushort)0;
            build.TerrestrialEliteSkillPaletteId = SkillBar.Skills.Terrestial[4].Skill != null && SkillBar.Skills.Terrestial[4].Skill.PaletteID != 0 ? (ushort)SkillBar.Skills.Terrestial[4].Skill.PaletteID : (ushort)0;

            SpecializationLine specLine;
            List<GW2API.Trait> selectedTraits;

            specLine = SpecializationsLines[0];
            selectedTraits = SpecializationsLines[0].SelectedTraits;
            build.Specialization1Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
            if (specLine.Specialization != null)
            {
                build.Specialization1Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                build.Specialization1Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                build.Specialization1Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
            }

            specLine = SpecializationsLines[1];
            selectedTraits = SpecializationsLines[1].SelectedTraits;
            build.Specialization2Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
            if (specLine.Specialization != null)
            {
                build.Specialization2Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                build.Specialization2Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                build.Specialization2Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
            }

            specLine = SpecializationsLines[2];
            selectedTraits = SpecializationsLines[2].SelectedTraits;
            build.Specialization3Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
            if (specLine.Specialization != null)
            {
                build.Specialization3Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                build.Specialization3Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                build.Specialization3Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
            }

            var bytes = build.ToArray();
            build.Parse(bytes);

            ParsedBuildTemplateCode = build.ToString();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            newToolTip = null;

            spriteBatch.DrawOnCtrl(Parent,
                                   ContentService.Textures.Pixel,
                                   LocalBounds,
                                   bounds,
                                   Color.Transparent,
                                   0f,
                                   Vector2.Zero
                                   );

            foreach (SpecializationLine specializationLine in SpecializationsLines)
            {
                specializationLine.Paint(spriteBatch, bounds, RelativeMousePosition, Scale);
            }

            SkillBar.Paint(spriteBatch, bounds, RelativeMousePosition, Scale);
            BasicTooltipText = newToolTip;
            ControlBounds = new Rectangle(SkillBar.Location.X, SkillBar.Location.Y, Math.Max(SpecializationsLines[0].ControlBounds.Width, SkillBar.ControlBounds.Width), SpecializationsLines[2].ControlBounds.Bottom - SkillBar.ControlBounds.Top);
        }
    }
}