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
    public class SkillBar_Control : Control
    {
        private readonly CustomTooltip CustomTooltip;

        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        private readonly List<Skill_Control> _Legends_Aquatic;
        private readonly List<Skill_Control> _Legends_Terrestrial;

        private readonly List<Skill_Control> _Skills_Aquatic;
        private readonly List<Skill_Control> _Skills_Terrestrial;

        private readonly Texture2D _AquaTexture;
        private readonly Texture2D _TerrestrialTexture;
        private readonly Texture2D _SwapTexture;

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
                foreach (Skill_Control skill in _Skills_Aquatic)
                {
                    skill.Scale = value;
                }

                foreach (Skill_Control skill in _Skills_Terrestrial)
                {
                    skill.Scale = value;
                }
            }
        }

        private readonly int _SkillSize = 55;
        public int _Width = 643;
        private Point _OGLocation;

        public Point _Location
        {
            get => Location;
            set
            {
                if (Location == null || Location == Point.Zero)
                {
                    _OGLocation = value;
                }

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

            _TerrestrialTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Land);
            _AquaTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Water);
            _SwapTexture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Refresh);

            Array slots = Enum.GetValues(typeof(SkillSlots));

            // BackgroundColor = Color.Magenta;

            _Skills_Aquatic = new List<Skill_Control>();
            foreach (API.Skill skill in Template.Build.Skills_Aquatic)
            {
                _Skills_Aquatic.Add(new Skill_Control(Parent)
                {
                    Location = new Point(27 + (_Skills_Aquatic.Count * (_SkillSize + 1)), 51),
                    Skill = Template.Build.Skills_Aquatic[_Skills_Aquatic.Count],
                    Slot = (SkillSlots)_Skills_Aquatic.Count,
                    Aquatic = true,
                });

                Skill_Control control = _Skills_Aquatic[_Skills_Aquatic.Count - 1];
                control.Click += Control_Click;
            }

            int p = _Width - (_Skills_Aquatic.Count * (_SkillSize + 1));
            _Skills_Terrestrial = new List<Skill_Control>();
            foreach (API.Skill skill in Template.Build.Skills_Terrestrial)
            {
                _Skills_Terrestrial.Add(new Skill_Control(Parent)
                {
                    Location = new Point(p + (_Skills_Terrestrial.Count * (_SkillSize + 1)), 51),
                    Skill = Template.Build.Skills_Terrestrial[_Skills_Terrestrial.Count],
                    Slot = (SkillSlots)_Skills_Terrestrial.Count,
                });

                Skill_Control control = _Skills_Terrestrial[_Skills_Terrestrial.Count - 1];
                control.Click += Control_Click;
            }

            SkillSelector = new SkillSelector_Control()
            {
                Parent = Parent,

                // Size  =  new Point(100, 250),
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

                // Size  =  new Point(100, 250),
                Visible = false,
                ZIndex = ZIndex + 3,
            };
            LegendSelector.SkillChanged += LegendSelector_SkillChanged;
            SkillSelector.SkillChanged += OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;
            BuildsManager.s_moduleInstance.Selected_Template_Changed += ApplyBuild;
        }

        private void Control_Click(object sender, MouseEventArgs mouse)
        {
            Skill_Control control = (Skill_Control)sender;

            if (CanClick)
            {
                if (!SkillSelector.Visible || SkillSelector.currentObject != control)
                {
                    SkillSelector.Visible = true;
                    SkillSelector.Skill_Control = control;
                    SkillSelector.Location = control.Location.Add(new Point(-2, control.Height));

                    List<API.Skill> Skills = new();
                    if (Template.Build.Profession != null)
                    {
                        if (Template.Build.Profession.Id == "Revenant")
                        {
                            API.Legend legend = control.Aquatic ? Template.Build.Legends_Aquatic[0] : Template.Build.Legends_Terrestrial[0];

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
                            foreach (API.Skill iSkill in Template.Build.Profession.Skills.OrderBy(e => e.Specialization).ThenBy(e => e.Categories.Count > 0 ? e.Categories[0] : "Unkown").ToList())
                            {
                                if (iSkill.Specialization == 0 || Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == iSkill.Specialization) != null)
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

            Rectangle rect0 = new Rectangle(new Point(36, 15), new Point(25, 25)).Scale(Scale);
            Rectangle rect1 = new Rectangle(new Point(_Width - ((_Skills_Aquatic.Count * (_SkillSize + 1)) + 28) + 63, 15), new Point(25, 25)).Scale(Scale);

            if (rect0.Contains(RelativeMousePosition) || rect1.Contains(RelativeMousePosition))
            {
                Template.Build?.SwapLegends();
                SetTemplate();
            }
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);
            if (!CanClick)
            {
                CanClick = true;
            }
        }

        private void LegendSelector_SkillChanged(object sender, SkillChangedEvent e)
        {
            Skill_Control ctrl = e.Skill_Control;
            API.Legend legend = Template.Build.Legends_Terrestrial[0];

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
            Skill_Control ctrl = (Skill_Control)sender;
            if (Template.Profession?.Id == "Revenant")
            {
                List<API.Skill> legends = new();
                foreach (API.Legend legend in Template.Profession.Legends)
                {
                    if (legend.Specialization == 0 || Template.Specialization?.Id == legend.Specialization)
                    {
                        legends.Add(legend.Skill);
                    }
                }

                LegendSelector.Skills = legends;
                LegendSelector.Visible = true;
                LegendSelector.Aquatic = false;
                LegendSelector.currentObject = ctrl;

                LegendSelector.Skill_Control = ctrl;
                LegendSelector.Location = ctrl.Location.Add(new Point(-2, (int)(ctrl.Height * ctrl.Scale)));
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
                    if (skill_Control.Skill == e.Skill)
                    {
                        skill_Control.Skill = null;
                    }
                }

                for (int i = 0; i < Template.Build.Skills_Aquatic.Count; i++)
                {
                    if (Template.Build.Skills_Aquatic[i] == e.Skill)
                    {
                        Template.Build.Skills_Aquatic[i] = null;
                    }
                }

                Template.Build.Skills_Aquatic[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }
            else
            {
                foreach (Skill_Control skill_Control in _Skills_Terrestrial)
                {
                    if (skill_Control.Skill == e.Skill)
                    {
                        skill_Control.Skill = null;
                    }
                }

                for (int i = 0; i < Template.Build.Skills_Terrestrial.Count; i++)
                {
                    if (Template.Build.Skills_Terrestrial[i] == e.Skill)
                    {
                        Template.Build.Skills_Terrestrial[i] = null;
                    }
                }

                Template.Build.Skills_Terrestrial[(int)e.Skill_Control.Slot] = e.Skill;
                e.Skill_Control.Skill = e.Skill;
            }

            Template.SetChanged();
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!SkillSelector.MouseOver)
            {
                SkillSelector.Hide();
            }

            if (!LegendSelector.MouseOver)
            {
                LegendSelector.Hide();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(
                this,
                _AquaTexture,
                new Rectangle(new Point(0, 50), new Point(25, 25)).Scale(Scale),
                _AquaTexture.Bounds,
                Color.White,
                0f,
                default);

            if (ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _SwapTexture,
                    new Rectangle(new Point(36 + 27, 15), new Point(25, 25)).Scale(Scale),
                    _SwapTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawOnCtrl(
                this,
                _TerrestrialTexture,
                new Rectangle(new Point(_Width - ((_Skills_Aquatic.Count * (_SkillSize + 1)) + 28), 50), new Point(25, 25)).Scale(Scale),
                _TerrestrialTexture.Bounds,
                Color.White,
                0f,
                default);

            if (ShowProfessionSkills)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    _SwapTexture,
                    new Rectangle(new Point(_Width - ((_Skills_Aquatic.Count * (_SkillSize + 1)) + 28) + 63, 15), new Point(25, 25)).Scale(Scale),
                    _SwapTexture.Bounds,
                    Color.White,
                    0f,
                    default);
            }
        }

        protected override void DisposeControl()
        {
            BuildsManager.s_moduleInstance.Selected_Template_Changed -= ApplyBuild;
            LegendSelector.SkillChanged -= LegendSelector_SkillChanged;
            SkillSelector.SkillChanged -= OnSkillChanged;
            Input.Mouse.LeftMouseButtonPressed -= OnGlobalClick;

            _Legends_Terrestrial[0].Click -= Legend;
            _Legends_Terrestrial[1].Click -= Legend;
            _Legends_Aquatic[0].Click -= Legend;
            _Legends_Aquatic[1].Click -= Legend;

            foreach (Skill_Control skill in _Skills_Terrestrial) { skill.Click -= Control_Click; }
            foreach (Skill_Control skill in _Skills_Aquatic) { skill.Click -= Control_Click; }
            _Skills_Terrestrial.DisposeAll();
            _Skills_Aquatic.DisposeAll();

            CustomTooltip.Dispose();

            base.DisposeControl();
        }

        public void SetTemplate()
        {
            ShowProfessionSkills = Template.Profession?.Id == "Revenant";

            if (!ShowProfessionSkills)
            {
                Location = _OGLocation.Add(new Point(0, -16));
            }
            else
            {
                Location = _OGLocation;
            }

            int i = 0;
            foreach (Skill_Control sCtrl in _Skills_Aquatic)
            {
                sCtrl.Skill = Template.Build.Skills_Aquatic[i];
                sCtrl.Location = new Point(sCtrl.Location.X, ShowProfessionSkills ? 51 : (51 / 2) + 5);
                i++;
            }

            i = 0;
            foreach (Skill_Control sCtrl in _Skills_Terrestrial)
            {
                sCtrl.Skill = Template.Build.Skills_Terrestrial[i];
                sCtrl.Location = new Point(sCtrl.Location.X, ShowProfessionSkills ? 51 : (51 / 2) + 5);
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
}