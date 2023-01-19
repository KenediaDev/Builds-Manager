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

            this._TerrestrialTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Land);
            this._AquaTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Water);
            this._SwapTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(Icons.Refresh);

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
}