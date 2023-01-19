using System;
using System.Collections.Generic;
using Gw2Sharp.ChatLinks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class BuildTemplate : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                foreach (SpecLine specLine in SpecLines)
                {
                    if (specLine != null)
                    {
                        specLine.Changed -= OnChanged;
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

                Profession = null;
                SpecLines = null;
                Skills_Terrestrial = null;
                InactiveSkills_Terrestrial = null;
                Skills_Aquatic = null;
                InactiveSkills_Aquatic = null;
                Legends_Terrestrial = null;
                Legends_Aquatic = null;
            }
        }

        private string _TemplateCode;

        public string TemplateCode
        {
            private set
            {
                _TemplateCode = value;
            }

            get
            {
                _TemplateCode = ParseBuildCode();
                return _TemplateCode;
            }
        }

        public API.Profession Profession;

        public List<SpecLine> SpecLines = new()
        {
            new SpecLine() { Index = 0},
            new SpecLine() { Index = 1},
            new SpecLine() { Index = 2},
        };

        public List<API.Skill> Skills_Terrestrial = new()
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Terrestrial = new()
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> Skills_Aquatic = new()
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Aquatic = new()
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Legend> Legends_Terrestrial = new()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public List<API.Legend> Legends_Aquatic = new()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public string ParseBuildCode()
        {
            string code = string.Empty;
            if (Profession != null)
            {
                BuildChatLink build = new()
                {
                    Profession = (Gw2Sharp.Models.ProfessionType)Enum.Parse(typeof(Gw2Sharp.Models.ProfessionType), Profession.Id)
                };
                bool rev = build.Profession == Gw2Sharp.Models.ProfessionType.Revenant;

                build.RevenantActiveTerrestrialLegend = (byte)(rev && Legends_Terrestrial[0]?.Id != null ? Legends_Terrestrial[0]?.Id : 0);
                build.RevenantInactiveTerrestrialLegend = (byte)(rev && Legends_Terrestrial[1]?.Id != null ? Legends_Terrestrial[1]?.Id : 0);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = (ushort)(rev && InactiveSkills_Terrestrial[1]?.PaletteId != null ? InactiveSkills_Terrestrial[1]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = (ushort)(rev && InactiveSkills_Terrestrial[2]?.PaletteId != null ? InactiveSkills_Terrestrial[2]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = (ushort)(rev && InactiveSkills_Terrestrial[3]?.PaletteId != null ? InactiveSkills_Terrestrial[3]?.PaletteId : 0);

                build.RevenantActiveAquaticLegend = (byte)(rev && Legends_Aquatic[0]?.Id != null ? Legends_Aquatic[0]?.Id : 0);
                build.RevenantInactiveAquaticLegend = (byte)(rev && Legends_Aquatic[1]?.Id != null ? Legends_Aquatic[1]?.Id : 0);
                build.RevenantInactiveAquaticUtility1SkillPaletteId = (ushort)(rev && InactiveSkills_Aquatic[1]?.PaletteId != null ? InactiveSkills_Aquatic[1]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = (ushort)(rev && InactiveSkills_Aquatic[2]?.PaletteId != null ? InactiveSkills_Aquatic[2]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = (ushort)(rev && InactiveSkills_Aquatic[3]?.PaletteId != null ? InactiveSkills_Aquatic[3]?.PaletteId : 0);

                build.TerrestrialHealingSkillPaletteId = (ushort)(Skills_Terrestrial[0]?.Id > 0 ? Skills_Terrestrial[0]?.PaletteId : 0);
                build.TerrestrialUtility1SkillPaletteId = (ushort)(Skills_Terrestrial[1]?.Id > 0 ? Skills_Terrestrial[1]?.PaletteId : 0);
                build.TerrestrialUtility2SkillPaletteId = (ushort)(Skills_Terrestrial[2]?.Id > 0 ? Skills_Terrestrial[2]?.PaletteId : 0);
                build.TerrestrialUtility3SkillPaletteId = (ushort)(Skills_Terrestrial[3]?.Id > 0 ? Skills_Terrestrial[3]?.PaletteId : 0);
                build.TerrestrialEliteSkillPaletteId = (ushort)(Skills_Terrestrial[4]?.Id > 0 ? Skills_Terrestrial[4]?.PaletteId : 0);

                build.AquaticHealingSkillPaletteId = (ushort)(Skills_Aquatic[0]?.Id > 0 ? Skills_Aquatic[0]?.PaletteId : 0);
                build.AquaticUtility1SkillPaletteId = (ushort)(Skills_Aquatic[1]?.Id > 0 ? Skills_Aquatic[1]?.PaletteId : 0);
                build.AquaticUtility2SkillPaletteId = (ushort)(Skills_Aquatic[2]?.Id > 0 ? Skills_Aquatic[2]?.PaletteId : 0);
                build.AquaticUtility3SkillPaletteId = (ushort)(Skills_Aquatic[3]?.Id > 0 ? Skills_Aquatic[3]?.PaletteId : 0);
                build.AquaticEliteSkillPaletteId = (ushort)(Skills_Aquatic[4]?.Id > 0 ? Skills_Aquatic[4]?.PaletteId : 0);

                SpecLine specLine;
                List<API.Trait> selectedTraits;

                specLine = SpecLines[0];
                selectedTraits = SpecLines[0].Traits;
                build.Specialization1Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization1Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization1Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization1Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = SpecLines[1];
                selectedTraits = SpecLines[1].Traits;
                build.Specialization2Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization2Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization2Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization2Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = SpecLines[2];
                selectedTraits = SpecLines[2].Traits;
                build.Specialization3Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization3Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization3Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization3Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                byte[] bytes = build.ToArray();
                build.Parse(bytes);

                code = build.ToString();
            }

            return code;
        }

        public BuildTemplate(string code = default)
        {
            if (code != default)
            {
                BuildChatLink build = new();
                IGw2ChatLink chatlink = null;
                if (BuildChatLink.TryParse(code, out chatlink))
                {
                    TemplateCode = code;
                    build.Parse(chatlink.ToArray());

                    Profession = BuildsManager.s_moduleInstance.Data.Professions.Find(e => e.Id == build.Profession.ToString());
                    if (Profession != null)
                    {
                        if (build.Specialization1Id != 0)
                        {
                            SpecLines[0].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization1Id);

                            if (SpecLines[0].Specialization != null)
                            {
                                SpecLines[0].Traits = new List<API.Trait>();

                                API.Specialization spec = SpecLines[0].Specialization;
                                List<byte> traits = new(new byte[] { build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index });
                                List<API.Trait> selectedTraits = SpecLines[0].Traits;
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
                            SpecLines[1].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization2Id);

                            if (SpecLines[1].Specialization != null)
                            {
                                SpecLines[1].Traits = new List<API.Trait>();

                                API.Specialization spec = SpecLines[1].Specialization;
                                List<byte> traits = new(new byte[] { build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index });
                                List<API.Trait> selectedTraits = SpecLines[1].Traits;
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
                            SpecLines[2].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization3Id);

                            if (SpecLines[2].Specialization != null)
                            {
                                SpecLines[2].Traits = new List<API.Trait>();

                                API.Specialization spec = SpecLines[2].Specialization;
                                List<byte> traits = new(new byte[] { build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index });
                                List<API.Trait> selectedTraits = SpecLines[2].Traits;
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
                        if (Profession.Id == "Revenant")
                        {
                            Legends_Terrestrial[0] = Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveTerrestrialLegend);
                            Legends_Terrestrial[1] = Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveTerrestrialLegend);

                            if (Legends_Terrestrial[0] != null)
                            {
                                API.Legend legend = Legends_Terrestrial[0];
                                Skills_Terrestrial[0] = legend.Heal;
                                Skills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility1SkillPaletteId);
                                Skills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility2SkillPaletteId);
                                Skills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility3SkillPaletteId);
                                Skills_Terrestrial[4] = legend.Elite;
                            }

                            if (Legends_Terrestrial[1] != null)
                            {
                                API.Legend legend = Legends_Terrestrial[1];
                                InactiveSkills_Terrestrial[0] = legend.Heal;
                                InactiveSkills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility1SkillPaletteId);
                                InactiveSkills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility2SkillPaletteId);
                                InactiveSkills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility3SkillPaletteId);
                                InactiveSkills_Terrestrial[4] = legend.Elite;
                            }

                            Legends_Aquatic[0] = Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveAquaticLegend);
                            Legends_Aquatic[1] = Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveAquaticLegend);

                            if (Legends_Aquatic[0] != null)
                            {
                                API.Legend legend = Legends_Aquatic[0];
                                Skills_Aquatic[0] = legend.Heal;
                                Skills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility1SkillPaletteId);
                                Skills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility2SkillPaletteId);
                                Skills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility3SkillPaletteId);
                                Skills_Aquatic[4] = legend.Elite;
                            }

                            if (Legends_Aquatic[1] != null)
                            {
                                API.Legend legend = Legends_Aquatic[1];
                                InactiveSkills_Aquatic[0] = legend.Heal;
                                InactiveSkills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility1SkillPaletteId);
                                InactiveSkills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility2SkillPaletteId);
                                InactiveSkills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility3SkillPaletteId);
                                InactiveSkills_Aquatic[4] = legend.Elite;
                            }
                        }
                        else
                        {
                            List<ushort> Terrestrial_PaletteIds = new()
                            {
                                build.TerrestrialHealingSkillPaletteId,
                                build.TerrestrialUtility1SkillPaletteId,
                                build.TerrestrialUtility2SkillPaletteId,
                                build.TerrestrialUtility3SkillPaletteId,
                                build.TerrestrialEliteSkillPaletteId,
                            };

                            int skillindex = 0;
                            foreach (ushort pid in Terrestrial_PaletteIds)
                            {
                                API.Skill skill = Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    Skills_Terrestrial[skillindex] = skill;
                                }

                                skillindex++;
                            }

                            List<ushort> Aqua_PaletteIds = new()
                            {
                                build.AquaticHealingSkillPaletteId,
                                build.AquaticUtility1SkillPaletteId,
                                build.AquaticUtility2SkillPaletteId,
                                build.AquaticUtility3SkillPaletteId,
                                build.AquaticEliteSkillPaletteId,
                            };
                            skillindex = 0;
                            foreach (ushort pid in Aqua_PaletteIds)
                            {
                                API.Skill skill = Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    Skills_Aquatic[skillindex] = skill;
                                }

                                skillindex++;
                            }
                        }

                        foreach (SpecLine specLine in SpecLines)
                        {
                            specLine.Changed += OnChanged;
                        }
                    }
                }
            }
            else
            {
                Profession = BuildsManager.s_moduleInstance.CurrentProfession;
            }
        }

        public void SwapLegends()
        {
            if (Profession.Id == "Revenant")
            {
                API.Legend tLegend = Legends_Terrestrial[0];
                Legends_Terrestrial[0] = Legends_Terrestrial[1];
                Legends_Terrestrial[1] = tLegend;

                API.Skill tSkill1 = Skills_Terrestrial[1];
                API.Skill tSkill2 = Skills_Terrestrial[2];
                API.Skill tSkill3 = Skills_Terrestrial[3];

                Skills_Terrestrial[0] = Legends_Terrestrial[0]?.Heal;
                Skills_Terrestrial[1] = InactiveSkills_Terrestrial[1];
                Skills_Terrestrial[2] = InactiveSkills_Terrestrial[2];
                Skills_Terrestrial[3] = InactiveSkills_Terrestrial[3];
                Skills_Terrestrial[4] = Legends_Terrestrial[0]?.Elite;

                InactiveSkills_Terrestrial[1] = tSkill1;
                InactiveSkills_Terrestrial[2] = tSkill2;
                InactiveSkills_Terrestrial[3] = tSkill3;

                tLegend = Legends_Aquatic[0];
                Legends_Aquatic[0] = Legends_Aquatic[1];
                Legends_Aquatic[1] = tLegend;

                tSkill1 = Skills_Aquatic[1];
                tSkill2 = Skills_Aquatic[2];
                tSkill3 = Skills_Aquatic[3];

                Skills_Aquatic[0] = Legends_Aquatic[0]?.Heal;
                Skills_Aquatic[1] = InactiveSkills_Aquatic[1];
                Skills_Aquatic[2] = InactiveSkills_Aquatic[2];
                Skills_Aquatic[3] = InactiveSkills_Aquatic[3];
                Skills_Aquatic[4] = Legends_Aquatic[0]?.Elite;

                InactiveSkills_Aquatic[1] = tSkill1;
                InactiveSkills_Aquatic[2] = tSkill2;
                InactiveSkills_Aquatic[3] = tSkill3;
                if (BuildsManager.s_moduleInstance.Selected_Template.Build == this)
                {
                    BuildsManager.s_moduleInstance.OnSelected_Template_Edit(null, null);
                }
            }
        }

        public EventHandler Changed;

        private void OnChanged(object sender, EventArgs e)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
