using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendaryItem = Kenedia.Modules.BuildsManager.GW2API.LegendaryItem;

namespace Kenedia.Modules.BuildsManager
{
   public class iData
    {
        public List<GW2API.Item> LegendaryItems = new List<GW2API.Item>();
        public List<GW2API.Item> Runes = new List<GW2API.Item>();
        public List<GW2API.Item> Sigils = new List<GW2API.Item>();
        public List<GW2API.Skill> Skills = new List<GW2API.Skill>();
        public List<GW2API.Trait> Traits = new List<GW2API.Trait>();
        public List<GW2API.Specialization> Specializations = new List<GW2API.Specialization>();
        public List<GW2API.Profession> Professions = new List<GW2API.Profession>();

        public iData()
        {
            var culture = BuildsManager.getCultureString();
            var base_path = BuildsManager.Paths.BasePath+ @"\api\";
            var armory_path = BuildsManager.Paths.armory;
            var file_path = armory_path + @"\" + "armory [" + BuildsManager.getCultureString() + "].json";
            var SkillsByPalette = new List<KeyValuePair<int, int>>();

            file_path = BuildsManager.Paths.sigils + @"sigils [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Sigils = JsonConvert.DeserializeObject<List<GW2API.Item>>(System.IO.File.ReadAllText(file_path));
            foreach(GW2API.Item o in Sigils) { o.Icon.Path = BuildsManager.Paths.sigil_icons; };

            file_path = BuildsManager.Paths.runes + @"runes [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Runes = JsonConvert.DeserializeObject<List<GW2API.Item>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Item o in Runes) { o.Icon.Path = BuildsManager.Paths.rune_icons; };

            file_path = BuildsManager.Paths.armory + @"armory [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) LegendaryItems = JsonConvert.DeserializeObject<List<GW2API.Item>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Item o in LegendaryItems) { o.Icon.Path = BuildsManager.Paths.armory_icons; };

            file_path = BuildsManager.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Professions = JsonConvert.DeserializeObject<List<GW2API.Profession>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Profession o in Professions)
            {
                o.Icon.Path = BuildsManager.Paths.profession_icons;
                o.IconBig.Path = BuildsManager.Paths.profession_icons;

                foreach (GW2API.Profession profession in Professions)
                {
                    SkillsByPalette.AddRange(profession.SkillsByPalette);
                }
            };

            file_path = BuildsManager.Paths.skills + @"skills [" + culture + "].json";
            List<GW2API.Skill> temp_Skills = new List<GW2API.Skill>();
            if (System.IO.File.Exists(file_path)) temp_Skills = JsonConvert.DeserializeObject<List<GW2API.Skill>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Skill o in temp_Skills) {
                o.Icon.Path = BuildsManager.Paths.skill_icons;
                var ids = SkillsByPalette.Find(e => e.Value == o.Id); 
                if (ids.Value > 0)
                {
                    BuildsManager.Logger.Debug("Adding skill '{0}' ({1}) with PaletteID: {2}", o.Name, o.Id, ids.Key);
                    o.PaletteID = ids.Key;
                    Skills.Add(o);
                }
            };

            file_path = BuildsManager.Paths.traits + @"traits [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Traits = JsonConvert.DeserializeObject<List<GW2API.Trait>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Trait o in Traits) { o.Icon.Path = BuildsManager.Paths.traits_icons; };

            file_path = BuildsManager.Paths.specs + @"specializations [" + culture + "].json";
            BuildsManager.Logger.Debug("Specs Path: {0}", file_path);
            if (System.IO.File.Exists(file_path)) Specializations = JsonConvert.DeserializeObject<List<GW2API.Specialization>>(System.IO.File.ReadAllText(file_path));
            foreach (GW2API.Specialization o in Specializations) 
            { 
                o.Icon.Path = BuildsManager.Paths.spec_icons; 
                o.Background.Path = BuildsManager.Paths.spec_backgrounds; 
                foreach(int id in o.MajorTraits)
                {
                    GW2API.Trait trait = Traits.Find(e => e.Id == id);
                    o.Traits.Add(trait);
                }
            };
        }
    }
}
