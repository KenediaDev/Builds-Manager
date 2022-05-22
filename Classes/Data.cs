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
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Blish_HUD.Controls;

namespace Kenedia.Modules.BuildsManager
{
    public class iData : IDisposable
    {
        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                foreach(API.Stat e in Stats) { e.Dispose(); }
                foreach(API.Profession e in Professions) { e.Dispose(); }
                foreach(API.RuneItem e in Runes) { e.Dispose(); }
                foreach(API.SigilItem e in Sigils) { e.Dispose(); }
                foreach(API.ArmorItem e in Armors) { e.Dispose(); }
                foreach(API.WeaponItem e in Weapons) { e.Dispose(); }
                foreach(API.TrinketItem e in Trinkets) { e.Dispose(); }
                foreach(API.Legend e in Legends) { e.Dispose(); }

                Stats.Clear();
                Professions.Clear();
                Runes.Clear();
                Sigils.Clear();
                Armors.Clear();
                Weapons.Clear();
                Trinkets.Clear();
                Legends.Clear();

                SkillID_Pairs.Clear();

                PlaceHolder?.Dispose();
            }
        }

        public class _Legend
        {
            public string Name;
            public int Id;
            public int Skill;
            public List<int> Utilities;
            public int Heal;
            public int Elite;
            public int Swap;
            public int Specialization;
        }
        public class SkillID_Pair
        {
            public int PaletteID;
            public int ID;
        }

        public List<API.Stat> Stats = new List<API.Stat>();
        public List<API.Profession> Professions = new List<API.Profession>();
        public List<API.RuneItem> Runes = new List<API.RuneItem>();
        public List<API.SigilItem> Sigils = new List<API.SigilItem>();
        public List<API.ArmorItem> Armors = new List<API.ArmorItem>();
        public List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
        public List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();
        public List<SkillID_Pair> SkillID_Pairs = new List<SkillID_Pair>();
        public List<API.Legend> Legends = new List<API.Legend>();

        public Texture2D PlaceHolder;

        public void UpdateLanguage()
        {
            string file_path;
            var culture = BuildsManager.getCultureString();
            List<string> filesToDelete = new List<string>();

            file_path = BuildsManager.ModuleInstance.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                foreach (API.Profession entry in JsonConvert.DeserializeObject<List<API.Profession>>(LoadFile(file_path, filesToDelete)))
                {
                    var target = Professions.Find(e => e.Id == entry.Id);

                    if (target != null)
                    {
                        target.Name = entry.Name;

                        foreach (API.Specialization specialization in entry.Specializations)
                        {
                            var targetSpecialization = target.Specializations.Find(e => e.Id == specialization.Id);
                            if (targetSpecialization != null)
                            {
                                targetSpecialization.Name = specialization.Name;

                                foreach (API.Trait trait in specialization.MajorTraits)
                                {
                                    var targetTrait = targetSpecialization.MajorTraits.Find(e => e.Id == trait.Id);
                                    if (targetTrait != null)
                                    {
                                        targetTrait.Name = trait.Name;
                                        targetTrait.Description = trait.Description;
                                    }
                                }

                                foreach (API.Trait trait in specialization.MinorTraits)
                                {
                                    var targetTrait = targetSpecialization.MinorTraits.Find(e => e.Id == trait.Id);
                                    if (targetTrait != null)
                                    {
                                        targetTrait.Name = trait.Name;
                                        targetTrait.Description = trait.Description;
                                    }
                                }

                                if (specialization.WeaponTrait != null)
                                {
                                    targetSpecialization.WeaponTrait.Name = specialization.WeaponTrait.Name;
                                    targetSpecialization.WeaponTrait.Description = specialization.WeaponTrait.Description;
                                }
                            }
                        }

                        foreach (API.Skill skill in entry.Skills)
                        {
                            var targetSkill = target.Skills.Find(e => e.Id == skill.Id);
                            if (targetSkill != null)
                            {
                                targetSkill.Name = skill.Name;
                                targetSkill.Description = skill.Description;
                            }
                        }

                        foreach (API.Legend legend in entry.Legends)
                        {
                            var targetLegend = target.Legends.Find(e => e.Id == legend.Id);
                            if (targetLegend != null)
                            {
                                targetLegend.Name = legend.Name;
                            }
                        }
                    }
                }

                file_path = BuildsManager.ModuleInstance.Paths.stats + @"stats [" + culture + "].json";
                foreach (API.Stat tStat in JsonConvert.DeserializeObject<List<API.Stat>>(LoadFile(file_path, filesToDelete)))
                {
                    var stat = Stats.Find(e => e.Id == tStat.Id);
                    if (stat != null)
                    {
                        stat.Name = tStat.Name;

                        foreach(API.StatAttribute attribute in stat.Attributes)
                        {
                            attribute.Name = attribute.getLocalName;
                        }
                    }
                }

                file_path = BuildsManager.ModuleInstance.Paths.runes + @"runes [" + culture + "].json";
                foreach (API.RuneItem tRune in JsonConvert.DeserializeObject<List<API.RuneItem>>(LoadFile(file_path, filesToDelete))){
                    var rune = Runes.Find(e => e.Id == tRune.Id);

                    if(rune != null)
                    {
                        rune.Name = tRune.Name;
                        rune.Bonuses = tRune.Bonuses;
                    }
                }

                file_path = BuildsManager.ModuleInstance.Paths.sigils + @"sigils [" + culture + "].json";
                foreach (API.SigilItem tSigil in JsonConvert.DeserializeObject<List<API.SigilItem>>(LoadFile(file_path, filesToDelete))){
                    var sigil = Sigils.Find(e => e.Id == tSigil.Id);

                    if(sigil != null)
                    {
                        sigil.Name = tSigil.Name;
                        sigil.Description = tSigil.Description;
                    }
                }

                file_path = BuildsManager.ModuleInstance.Paths.armory + @"armors [" + culture + "].json";
                foreach (API.ArmorItem tArmor in JsonConvert.DeserializeObject<List<API.ArmorItem>>(LoadFile(file_path, filesToDelete))){
                    var armor = Armors.Find(e => e.Id == tArmor.Id);

                    if(armor != null)
                    {
                        armor.Name = tArmor.Name;
                    }
                }


                file_path = BuildsManager.ModuleInstance.Paths.armory + @"weapons [" + culture + "].json";
                foreach (API.WeaponItem tWeapon in JsonConvert.DeserializeObject<List<API.WeaponItem>>(LoadFile(file_path, filesToDelete)))
                {
                    var weapon = Weapons.Find(e => e.Id == tWeapon.Id);

                    if (weapon != null)
                    {
                        weapon.Name = tWeapon.Name;
                    }
                }

                file_path = BuildsManager.ModuleInstance.Paths.armory + @"trinkets [" + culture + "].json";
                foreach (API.TrinketItem tTrinket in JsonConvert.DeserializeObject<List<API.TrinketItem>>(LoadFile(file_path, filesToDelete)))
                {
                    var trinket = Trinkets.Find(e => e.Id == tTrinket.Id);

                    if (trinket != null)
                    {
                        trinket.Name = tTrinket.Name;
                    }
                }
            }
        }

        private Texture2D LoadImage(string path, GraphicsDevice graphicsDevice, List<string> filesToDelete, Rectangle region = default, Rectangle default_Bounds = default)
        {
            var texture = PlaceHolder;

            {
                try
                {
                    texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    double factor = 1;

                    if (default_Bounds != default)
                    {
                        factor = (double)texture.Width / (double)default_Bounds.Width;
                    }

                    if (region != default)
                    {
                        region = region.Scale(factor);

                        if (texture.Bounds.Contains(region))
                        {
                            texture = texture.GetRegion(region);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    if (System.IO.File.Exists(path)) filesToDelete.Add(path);
                    texture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("InvalidOperationException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
                catch (UnauthorizedAccessException)
                {
                    texture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);
                    return texture;
                }
                catch (FileNotFoundException)
                {
                    texture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("FileNotFoundException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
                catch (FileLoadException)
                {
                    texture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("FileLoadException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
            }

            return texture;
        }

        private string LoadFile(string path, List<string> filesToDelete)
        {
            var txt = "";

            {
                try
                {
                    txt = System.IO.File.ReadAllText(path);
                }
                catch (InvalidOperationException)
                {
                    if (System.IO.File.Exists(path)) filesToDelete.Add(path);
                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (FileNotFoundException)
                {
                }
                catch (FileLoadException)
                {
                }
            }

            return txt;
        }

        public iData()
        {            
            var ContentsManager = BuildsManager.ModuleInstance.ContentsManager;
            var DirectoriesManager = BuildsManager.ModuleInstance.DirectoriesManager;

            PlaceHolder = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);
            List<string> filesToDelete = new List<string>();

            string file_path;
            var culture = BuildsManager.getCultureString();
            var base_path = BuildsManager.ModuleInstance.Paths.BasePath + @"\api\";

            SkillID_Pairs = JsonConvert.DeserializeObject<List<SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

            file_path = BuildsManager.ModuleInstance.Paths.stats + @"stats [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Stats = JsonConvert.DeserializeObject<List<API.Stat>>(LoadFile(file_path, filesToDelete));
            foreach (API.Stat stat in Stats) { stat.Icon._Texture = ContentsManager.GetTexture(stat.Icon.Path); stat.Attributes.Sort((a, b) => b.Multiplier.CompareTo(a.Multiplier)); foreach (API.StatAttribute attri in stat.Attributes) {attri.Name = attri.getLocalName; attri.Icon._Texture = ContentsManager.GetTexture(attri.Icon.Path); } }

            file_path = BuildsManager.ModuleInstance.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Professions = JsonConvert.DeserializeObject<List<API.Profession>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.ModuleInstance.Paths.runes + @"runes [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Runes = JsonConvert.DeserializeObject<List<API.RuneItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.ModuleInstance.Paths.sigils + @"sigils [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Sigils = JsonConvert.DeserializeObject<List<API.SigilItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.ModuleInstance.Paths.armory + @"armors [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Armors = JsonConvert.DeserializeObject<List<API.ArmorItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.ModuleInstance.Paths.armory + @"weapons [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Weapons = JsonConvert.DeserializeObject<List<API.WeaponItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.ModuleInstance.Paths.armory + @"trinkets [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Trinkets = JsonConvert.DeserializeObject<List<API.TrinketItem>>(LoadFile(file_path, filesToDelete));

            Trinkets = Trinkets.OrderBy(e => e.TrinketType).ToList();
            Weapons = Weapons.OrderBy(e => (int)e.WeaponType).ToList();


            Texture2D texture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Bug);

            foreach (API.Profession profession in Professions)
            {
                profession.Icon.ImageRegion = new Rectangle(4, 4, 26, 26);

                foreach (API.Specialization specialization in profession.Specializations)
                {
                    specialization.Background.ImageRegion = new Rectangle(0, 123, 643, 123);

                    if (specialization.WeaponTrait != null)
                    {
                        specialization.WeaponTrait.Icon.ImageRegion = new Rectangle(3, 3, 58, 58);
                        specialization.WeaponTrait.Icon.DefaultBounds = new Rectangle(0, 0, 64, 64);
                    }


                    foreach (API.Trait trait in specialization.MajorTraits)
                    {
                        trait.Icon.ImageRegion = new Rectangle(3, 3, 58, 58);
                        trait.Icon.DefaultBounds = new Rectangle(0, 0, 64, 64);
                    }

                    foreach (API.Trait trait in specialization.MinorTraits)
                    {
                        trait.Icon.ImageRegion = new Rectangle(3, 3, 58, 58);
                        trait.Icon.DefaultBounds = new Rectangle(0, 0, 64, 64);
                    }
                }

                foreach (API.Skill skill in profession.Skills)
                {
                    skill.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                }

                if (profession.Legends.Count > 0)
                {
                    foreach (API.Legend legend in profession.Legends)
                    {
                        if (legend.Heal.Icon != null && legend.Heal.Icon.Path != "") legend.Heal.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        if (legend.Elite.Icon != null && legend.Elite.Icon.Path != "") legend.Elite.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        if (legend.Swap.Icon != null && legend.Swap.Icon.Path != "") legend.Swap.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        if (legend.Skill.Icon != null && legend.Skill.Icon.Path != "") legend.Skill.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);

                        foreach (API.Skill skill in legend.Utilities)
                        {
                            skill.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }
                    }
                }
            }

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                BuildsManager.ModuleInstance.DataLoaded = true;
            });
        }
    }
}
