using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot = Gw2Sharp.WebApi.V2.Models.ItemEquipmentSlotType;
using Gw2Sharp.ChatLinks;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager
{
    public class Template
    {
        public string TemplateCode = "[&DQIEKRYqPTlwAAAAogEAAGoAAACvAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]";
        public GearTemplate Gear;
        public BuildTemplate Build;

        public Template() { }
        public Template(string templateCode)
        {
            TemplateCode = templateCode;
            Build = new BuildTemplate(templateCode);
        }

        public Template(GearTemplate gear, BuildTemplate build )
        {
            Gear = gear;
            Build = build;
        }
    }

    public class GearTemplate
    {
        List<GW2API.Item> Equipment = new List<GW2API.Item>();
    }
    public class SpecLine
    {
        public GW2API.Specialization Specialization;
        public List<GW2API.Trait> Traits = new List<GW2API.Trait>();
    }

    public class BuildTemplate
    {
        public string TemplateCode = "";
        public Gw2Sharp.Models.ProfessionType Profession;
        public List<SpecLine> SpecLines = new List<SpecLine>();
        public List<GW2API.Skill> UtilitySkills_Land = new List<GW2API.Skill>();
        public List<GW2API.Skill> UtilitySkills_Water = new List<GW2API.Skill>();

        public BuildTemplate(string code)
        {
            TemplateCode = code;
            BuildChatLink build = new BuildChatLink();
            IGw2ChatLink chatlink = null;
            BuildChatLink.TryParse(code, out chatlink);
            if (chatlink != null ) build.Parse(chatlink.ToArray());

            if(build != null)
            {
                Profession = build.Profession;

                var specline = new SpecLine();
                var spec = BuildsManager.Data.Specializations.Find(e => e.Id == build.Specialization1Id);
                List<byte> Traits = null;
                var offset = -1;

                if (spec != null)
                {
                    specline.Specialization = spec;
                    Traits = new List<byte>(new byte[] { build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index });
                    foreach (byte bit in Traits)
                    {
                        if (bit > 0) specline.Traits.Add(spec.Traits[(specline.Traits.Count * 3) + bit - 1]);
                    }
                }
                SpecLines.Add(specline);

                specline = new SpecLine();
                spec = BuildsManager.Data.Specializations.Find(e => e.Id == build.Specialization2Id);
                if (spec != null)
                {
                    specline.Specialization = spec;
                    Traits = new List<byte>(new byte[] { build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index });
                    offset = 2;
                    foreach (byte bit in Traits)
                    {
                        if (bit > 0) specline.Traits.Add(spec.Traits[(specline.Traits.Count * 3) + bit - 1]);
                    }

                    BuildsManager.Logger.Debug("Added Specline: {0}", specline.Specialization.Name);
                }
                SpecLines.Add(specline);

                specline = new SpecLine();
                spec = BuildsManager.Data.Specializations.Find(e => e.Id == build.Specialization3Id);
                if (spec != null)
                {
                    specline.Specialization = spec;
                    Traits = new List<byte>(new byte[] { build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index });
                    offset = 4;
                    foreach (byte bit in Traits)
                    {
                        if (bit > 0) specline.Traits.Add(spec.Traits[(specline.Traits.Count * 3) + bit - 1]);
                    }
                    BuildsManager.Logger.Debug("Added Specline: {0}", specline.Specialization.Name);
                }
                SpecLines.Add(specline);

                List<ushort> Terrestrial_PaletteIds = new List<ushort>();
                Terrestrial_PaletteIds.AddRange(new ushort[] {
                    build.TerrestrialHealingSkillPaletteId,
                    build.TerrestrialUtility1SkillPaletteId,
                    build.TerrestrialUtility2SkillPaletteId,
                    build.TerrestrialUtility3SkillPaletteId,
                    build.TerrestrialEliteSkillPaletteId,
                });

                foreach(ushort pid in Terrestrial_PaletteIds)
                {
                    GW2API.Skill skill = BuildsManager.Data.Skills.Find(e => e.PaletteID == pid);
                    if (skill != null) UtilitySkills_Land.Add(skill);
                }


                List<ushort> Aqua_PaletteIds = new List<ushort>();
                Aqua_PaletteIds.AddRange(new ushort[] {
                    build.AquaticHealingSkillPaletteId,
                    build.AquaticUtility1SkillPaletteId,
                    build.AquaticUtility2SkillPaletteId,
                    build.AquaticUtility3SkillPaletteId,
                    build.AquaticEliteSkillPaletteId,
                });
                foreach (ushort pid in Aqua_PaletteIds)
                {
                    GW2API.Skill skill = BuildsManager.Data.Skills.Find(e => e.PaletteID == pid);
                    if (skill != null) UtilitySkills_Water.Add(skill);
                }

                foreach (GW2API.Skill skill in UtilitySkills_Land)
                {
                    BuildsManager.Logger.Debug("Build uses Skill: " + skill.Name);
                }
            }
        }        
    }
}
