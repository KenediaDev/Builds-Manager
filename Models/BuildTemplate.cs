namespace Kenedia.Modules.BuildsManager.Models
{
    using System;
    using System.Collections.Generic;
    using Gw2Sharp.ChatLinks;

    public class BuildTemplate : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                foreach (SpecLine specLine in this.SpecLines)
                {
                    if (specLine != null)
                    {
                        specLine.Changed -= this.OnChanged;
                    }
                }

                // Profession?.Dispose();
                // SpecLines?.DisposeAll();
                // Skills_Terrestrial?.DisposeAll();
                // InactiveSkills_Terrestrial?.DisposeAll();
                // Skills_Aquatic?.DisposeAll();
                // InactiveSkills_Aquatic?.DisposeAll();
                // Legends_Terrestrial?.DisposeAll();
                // Legends_Aquatic?.DisposeAll();

                this.Profession = null;
                this.SpecLines = null;
                this.Skills_Terrestrial = null;
                this.InactiveSkills_Terrestrial = null;
                this.Skills_Aquatic = null;
                this.InactiveSkills_Aquatic = null;
                this.Legends_Terrestrial = null;
                this.Legends_Aquatic = null;
            }
        }

        private string _TemplateCode;

        public string TemplateCode
        {
            private set
            {
                this._TemplateCode = value;
            }

            get
            {
                this._TemplateCode = this.ParseBuildCode();
                return this._TemplateCode;
            }
        }

        public API.Profession Profession;

        public List<SpecLine> SpecLines = new List<SpecLine>
        {
            new SpecLine() { Index = 0},
            new SpecLine() { Index = 1},
            new SpecLine() { Index = 2},
        };

        public List<API.Skill> Skills_Terrestrial = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Terrestrial = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> Skills_Aquatic = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Aquatic = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Legend> Legends_Terrestrial = new List<API.Legend>()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public List<API.Legend> Legends_Aquatic = new List<API.Legend>()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public string ParseBuildCode()
        {
            string code = string.Empty;
            if (this.Profession != null)
            {
                BuildChatLink build = new BuildChatLink();
                build.Profession = (Gw2Sharp.Models.ProfessionType)Enum.Parse(typeof(Gw2Sharp.Models.ProfessionType), this.Profession.Id);
                var rev = build.Profession == Gw2Sharp.Models.ProfessionType.Revenant;

                build.RevenantActiveTerrestrialLegend = (byte)(rev && this.Legends_Terrestrial[0]?.Id != null ? this.Legends_Terrestrial[0]?.Id : 0);
                build.RevenantInactiveTerrestrialLegend = (byte)(rev && this.Legends_Terrestrial[1]?.Id != null ? this.Legends_Terrestrial[1]?.Id : 0);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[1]?.PaletteId != null ? this.InactiveSkills_Terrestrial[1]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[2]?.PaletteId != null ? this.InactiveSkills_Terrestrial[2]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[3]?.PaletteId != null ? this.InactiveSkills_Terrestrial[3]?.PaletteId : 0);

                build.RevenantActiveAquaticLegend = (byte)(rev && this.Legends_Aquatic[0]?.Id != null ? this.Legends_Aquatic[0]?.Id : 0);
                build.RevenantInactiveAquaticLegend = (byte)(rev && this.Legends_Aquatic[1]?.Id != null ? this.Legends_Aquatic[1]?.Id : 0);
                build.RevenantInactiveAquaticUtility1SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[1]?.PaletteId != null ? this.InactiveSkills_Aquatic[1]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[2]?.PaletteId != null ? this.InactiveSkills_Aquatic[2]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[3]?.PaletteId != null ? this.InactiveSkills_Aquatic[3]?.PaletteId : 0);

                build.TerrestrialHealingSkillPaletteId = (ushort)(this.Skills_Terrestrial[0]?.Id > 0 ? this.Skills_Terrestrial[0]?.PaletteId : 0);
                build.TerrestrialUtility1SkillPaletteId = (ushort)(this.Skills_Terrestrial[1]?.Id > 0 ? this.Skills_Terrestrial[1]?.PaletteId : 0);
                build.TerrestrialUtility2SkillPaletteId = (ushort)(this.Skills_Terrestrial[2]?.Id > 0 ? this.Skills_Terrestrial[2]?.PaletteId : 0);
                build.TerrestrialUtility3SkillPaletteId = (ushort)(this.Skills_Terrestrial[3]?.Id > 0 ? this.Skills_Terrestrial[3]?.PaletteId : 0);
                build.TerrestrialEliteSkillPaletteId = (ushort)(this.Skills_Terrestrial[4]?.Id > 0 ? this.Skills_Terrestrial[4]?.PaletteId : 0);

                build.AquaticHealingSkillPaletteId = (ushort)(this.Skills_Aquatic[0]?.Id > 0 ? this.Skills_Aquatic[0]?.PaletteId : 0);
                build.AquaticUtility1SkillPaletteId = (ushort)(this.Skills_Aquatic[1]?.Id > 0 ? this.Skills_Aquatic[1]?.PaletteId : 0);
                build.AquaticUtility2SkillPaletteId = (ushort)(this.Skills_Aquatic[2]?.Id > 0 ? this.Skills_Aquatic[2]?.PaletteId : 0);
                build.AquaticUtility3SkillPaletteId = (ushort)(this.Skills_Aquatic[3]?.Id > 0 ? this.Skills_Aquatic[3]?.PaletteId : 0);
                build.AquaticEliteSkillPaletteId = (ushort)(this.Skills_Aquatic[4]?.Id > 0 ? this.Skills_Aquatic[4]?.PaletteId : 0);

                SpecLine specLine;
                List<API.Trait> selectedTraits;

                specLine = this.SpecLines[0];
                selectedTraits = this.SpecLines[0].Traits;
                build.Specialization1Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization1Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization1Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization1Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = this.SpecLines[1];
                selectedTraits = this.SpecLines[1].Traits;
                build.Specialization2Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization2Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization2Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization2Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = this.SpecLines[2];
                selectedTraits = this.SpecLines[2].Traits;
                build.Specialization3Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization3Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization3Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization3Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                var bytes = build.ToArray();
                build.Parse(bytes);

                code = build.ToString();
            }

            return code;
        }

        public BuildTemplate(string code = default)
        {
            if (code != default)
            {
                BuildChatLink build = new BuildChatLink();
                IGw2ChatLink chatlink = null;
                if (BuildChatLink.TryParse(code, out chatlink))
                {
                    this.TemplateCode = code;
                    build.Parse(chatlink.ToArray());

                    this.Profession = BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == build.Profession.ToString());
                    if (this.Profession != null)
                    {
                        if (build.Specialization1Id != 0)
                        {
                            this.SpecLines[0].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization1Id);

                            if (this.SpecLines[0].Specialization != null)
                            {
                                this.SpecLines[0].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[0].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index });
                                var selectedTraits = this.SpecLines[0].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        if (build.Specialization2Id != 0)
                        {
                            this.SpecLines[1].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization2Id);

                            if (this.SpecLines[1].Specialization != null)
                            {
                                this.SpecLines[1].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[1].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index });
                                var selectedTraits = this.SpecLines[1].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        if (build.Specialization3Id != 0)
                        {
                            this.SpecLines[2].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization3Id);

                            if (this.SpecLines[2].Specialization != null)
                            {
                                this.SpecLines[2].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[2].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index });
                                var selectedTraits = this.SpecLines[2].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        // [&DQkDJg8mPz3cEQAABhIAACsSAADUEQAAyhEAAAUCAADUESsSBhIAAAAAAAA=]
                        if (this.Profession.Id == "Revenant")
                        {
                            this.Legends_Terrestrial[0] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveTerrestrialLegend);
                            this.Legends_Terrestrial[1] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveTerrestrialLegend);

                            if (this.Legends_Terrestrial[0] != null)
                            {
                                var legend = this.Legends_Terrestrial[0];
                                this.Skills_Terrestrial[0] = legend.Heal;
                                this.Skills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility1SkillPaletteId);
                                this.Skills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility2SkillPaletteId);
                                this.Skills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility3SkillPaletteId);
                                this.Skills_Terrestrial[4] = legend.Elite;
                            }

                            if (this.Legends_Terrestrial[1] != null)
                            {
                                var legend = this.Legends_Terrestrial[1];
                                this.InactiveSkills_Terrestrial[0] = legend.Heal;
                                this.InactiveSkills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility1SkillPaletteId);
                                this.InactiveSkills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility2SkillPaletteId);
                                this.InactiveSkills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility3SkillPaletteId);
                                this.InactiveSkills_Terrestrial[4] = legend.Elite;
                            }

                            this.Legends_Aquatic[0] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveAquaticLegend);
                            this.Legends_Aquatic[1] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveAquaticLegend);

                            if (this.Legends_Aquatic[0] != null)
                            {
                                var legend = this.Legends_Aquatic[0];
                                this.Skills_Aquatic[0] = legend.Heal;
                                this.Skills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility1SkillPaletteId);
                                this.Skills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility2SkillPaletteId);
                                this.Skills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility3SkillPaletteId);
                                this.Skills_Aquatic[4] = legend.Elite;
                            }

                            if (this.Legends_Aquatic[1] != null)
                            {
                                var legend = this.Legends_Aquatic[1];
                                this.InactiveSkills_Aquatic[0] = legend.Heal;
                                this.InactiveSkills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility1SkillPaletteId);
                                this.InactiveSkills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility2SkillPaletteId);
                                this.InactiveSkills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility3SkillPaletteId);
                                this.InactiveSkills_Aquatic[4] = legend.Elite;
                            }
                        }
                        else
                        {
                            List<ushort> Terrestrial_PaletteIds = new List<ushort>{
                                build.TerrestrialHealingSkillPaletteId,
                                build.TerrestrialUtility1SkillPaletteId,
                                build.TerrestrialUtility2SkillPaletteId,
                                build.TerrestrialUtility3SkillPaletteId,
                                build.TerrestrialEliteSkillPaletteId,
                            };

                            int skillindex = 0;
                            foreach (ushort pid in Terrestrial_PaletteIds)
                            {
                                API.Skill skill = this.Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    this.Skills_Terrestrial[skillindex] = skill;
                                }

                                skillindex++;
                            }

                            List<ushort> Aqua_PaletteIds = new List<ushort>{
                                build.AquaticHealingSkillPaletteId,
                                build.AquaticUtility1SkillPaletteId,
                                build.AquaticUtility2SkillPaletteId,
                                build.AquaticUtility3SkillPaletteId,
                                build.AquaticEliteSkillPaletteId,
                            };
                            skillindex = 0;
                            foreach (ushort pid in Aqua_PaletteIds)
                            {
                                API.Skill skill = this.Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    this.Skills_Aquatic[skillindex] = skill;
                                }

                                skillindex++;
                            }
                        }

                        foreach (SpecLine specLine in this.SpecLines)
                        {
                            specLine.Changed += this.OnChanged;
                        }
                    }
                }
            }
            else
            {
                this.Profession = BuildsManager.ModuleInstance.CurrentProfession;
            }
        }

        public void SwapLegends()
        {
            if (this.Profession.Id == "Revenant")
            {
                var tLegend = this.Legends_Terrestrial[0];
                this.Legends_Terrestrial[0] = this.Legends_Terrestrial[1];
                this.Legends_Terrestrial[1] = tLegend;

                var tSkill1 = this.Skills_Terrestrial[1];
                var tSkill2 = this.Skills_Terrestrial[2];
                var tSkill3 = this.Skills_Terrestrial[3];

                this.Skills_Terrestrial[0] = this.Legends_Terrestrial[0]?.Heal;
                this.Skills_Terrestrial[1] = this.InactiveSkills_Terrestrial[1];
                this.Skills_Terrestrial[2] = this.InactiveSkills_Terrestrial[2];
                this.Skills_Terrestrial[3] = this.InactiveSkills_Terrestrial[3];
                this.Skills_Terrestrial[4] = this.Legends_Terrestrial[0]?.Elite;

                this.InactiveSkills_Terrestrial[1] = tSkill1;
                this.InactiveSkills_Terrestrial[2] = tSkill2;
                this.InactiveSkills_Terrestrial[3] = tSkill3;

                tLegend = this.Legends_Aquatic[0];
                this.Legends_Aquatic[0] = this.Legends_Aquatic[1];
                this.Legends_Aquatic[1] = tLegend;

                tSkill1 = this.Skills_Aquatic[1];
                tSkill2 = this.Skills_Aquatic[2];
                tSkill3 = this.Skills_Aquatic[3];

                this.Skills_Aquatic[0] = this.Legends_Aquatic[0]?.Heal;
                this.Skills_Aquatic[1] = this.InactiveSkills_Aquatic[1];
                this.Skills_Aquatic[2] = this.InactiveSkills_Aquatic[2];
                this.Skills_Aquatic[3] = this.InactiveSkills_Aquatic[3];
                this.Skills_Aquatic[4] = this.Legends_Aquatic[0]?.Elite;

                this.InactiveSkills_Aquatic[1] = tSkill1;
                this.InactiveSkills_Aquatic[2] = tSkill2;
                this.InactiveSkills_Aquatic[3] = tSkill3;
                if (BuildsManager.ModuleInstance.Selected_Template.Build == this)
                {
                    BuildsManager.ModuleInstance.OnSelected_Template_Edit(null, null);
                }
            }
        }

        public EventHandler Changed;

        private void OnChanged(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
