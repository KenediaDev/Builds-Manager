using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    public class Data : IDisposable
    {
        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                Professions.DisposeAll();
                Stats.DisposeAll();
                Runes.DisposeAll();
                Sigils.DisposeAll();
                Armors.DisposeAll();
                Weapons.DisposeAll();
                Trinkets.DisposeAll();
                Legends.DisposeAll();

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

        public class Legend
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

        public List<API.Stat> Stats = new();
        public List<API.Profession> Professions = new();
        public List<API.RuneItem> Runes = new();
        public List<API.SigilItem> Sigils = new();
        public List<API.ArmorItem> Armors = new();
        public List<API.WeaponItem> Weapons = new();
        public List<API.TrinketItem> Trinkets = new();
        public List<SkillID_Pair> SkillID_Pairs = new();
        public List<API.Legend> Legends = new();

        public Texture2D PlaceHolder;

        public void UpdateLanguage()
        {
            string file_path;
            string culture = BuildsManager.getCultureString();
            List<string> filesToDelete = new();

            file_path = BuildsManager.s_moduleInstance.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                foreach (API.Profession entry in JsonConvert.DeserializeObject<List<API.Profession>>(LoadFile(file_path, filesToDelete)))
                {
                    API.Profession target = Professions.Find(e => e.Id == entry.Id);

                    if (target != null)
                    {
                        target.Name = entry.Name;

                        foreach (API.Specialization specialization in entry.Specializations)
                        {
                            API.Specialization targetSpecialization = target.Specializations.Find(e => e.Id == specialization.Id);
                            if (targetSpecialization != null)
                            {
                                targetSpecialization.Name = specialization.Name;

                                foreach (API.Trait trait in specialization.MajorTraits)
                                {
                                    API.Trait targetTrait = targetSpecialization.MajorTraits.Find(e => e.Id == trait.Id);
                                    if (targetTrait != null)
                                    {
                                        targetTrait.Name = trait.Name;
                                        targetTrait.Description = trait.Description;
                                    }
                                }

                                foreach (API.Trait trait in specialization.MinorTraits)
                                {
                                    API.Trait targetTrait = targetSpecialization.MinorTraits.Find(e => e.Id == trait.Id);
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
                            API.Skill targetSkill = target.Skills.Find(e => e.Id == skill.Id);
                            if (targetSkill != null)
                            {
                                targetSkill.Name = skill.Name;
                                targetSkill.Description = skill.Description;
                            }
                        }

                        foreach (API.Legend legend in entry.Legends)
                        {
                            API.Legend targetLegend = target.Legends.Find(e => e.Id == legend.Id);
                            if (targetLegend != null)
                            {
                                targetLegend.Name = legend.Name;
                            }
                        }
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.stats + @"stats [" + culture + "].json";
                foreach (API.Stat tStat in JsonConvert.DeserializeObject<List<API.Stat>>(LoadFile(file_path, filesToDelete)))
                {
                    API.Stat stat = Stats.Find(e => e.Id == tStat.Id);
                    if (stat != null)
                    {
                        stat.Name = tStat.Name;

                        foreach (API.StatAttribute attribute in stat.Attributes)
                        {
                            attribute.Name = attribute.getLocalName();
                        }
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.runes + @"runes [" + culture + "].json";
                foreach (API.RuneItem tRune in JsonConvert.DeserializeObject<List<API.RuneItem>>(LoadFile(file_path, filesToDelete)))
                {
                    API.RuneItem rune = Runes.Find(e => e.Id == tRune.Id);

                    if (rune != null)
                    {
                        rune.Name = tRune.Name;
                        rune.Bonuses = tRune.Bonuses;
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.sigils + @"sigils [" + culture + "].json";
                foreach (API.SigilItem tSigil in JsonConvert.DeserializeObject<List<API.SigilItem>>(LoadFile(file_path, filesToDelete)))
                {
                    API.SigilItem sigil = Sigils.Find(e => e.Id == tSigil.Id);

                    if (sigil != null)
                    {
                        sigil.Name = tSigil.Name;
                        sigil.Description = tSigil.Description;
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.armory + @"armors [" + culture + "].json";
                foreach (API.ArmorItem tArmor in JsonConvert.DeserializeObject<List<API.ArmorItem>>(LoadFile(file_path, filesToDelete)))
                {
                    API.ArmorItem armor = Armors.Find(e => e.Id == tArmor.Id);

                    if (armor != null)
                    {
                        armor.Name = tArmor.Name;
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.armory + @"weapons [" + culture + "].json";
                foreach (API.WeaponItem tWeapon in JsonConvert.DeserializeObject<List<API.WeaponItem>>(LoadFile(file_path, filesToDelete)))
                {
                    API.WeaponItem weapon = Weapons.Find(e => e.Id == tWeapon.Id);

                    if (weapon != null)
                    {
                        weapon.Name = tWeapon.Name;
                    }
                }

                file_path = BuildsManager.s_moduleInstance.Paths.armory + @"trinkets [" + culture + "].json";
                foreach (API.TrinketItem tTrinket in JsonConvert.DeserializeObject<List<API.TrinketItem>>(LoadFile(file_path, filesToDelete)))
                {
                    API.TrinketItem trinket = Trinkets.Find(e => e.Id == tTrinket.Id);

                    if (trinket != null)
                    {
                        trinket.Name = tTrinket.Name;
                    }
                }
            }
        }

        private Texture2D LoadImage(string path, GraphicsDevice graphicsDevice, List<string> filesToDelete, Rectangle region = default, Rectangle default_Bounds = default)
        {
            Texture2D texture;
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
                    if (System.IO.File.Exists(path))
                    {
                        filesToDelete.Add(path);
                    }

                    texture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);
                    BuildsManager.Logger.Debug("InvalidOperationException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
                catch (UnauthorizedAccessException)
                {
                    texture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);
                    return texture;
                }
                catch (FileNotFoundException)
                {
                    texture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);
                    BuildsManager.Logger.Debug("FileNotFoundException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
                catch (FileLoadException)
                {
                    texture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);
                    BuildsManager.Logger.Debug("FileLoadException: Failed to load {0}. Fetching the API again.", path);
                    return texture;
                }
            }

            return texture;
        }

        private string LoadFile(string path, List<string> filesToDelete)
        {
            string txt = string.Empty;

            {
                try
                {
                    txt = System.IO.File.ReadAllText(path);
                }
                catch (InvalidOperationException)
                {
                    if (System.IO.File.Exists(path))
                    {
                        filesToDelete.Add(path);
                    }
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

        public Data()
        {
            ContentsManager ContentsManager = BuildsManager.s_moduleInstance.ContentsManager;
            DirectoriesManager DirectoriesManager = BuildsManager.s_moduleInstance.DirectoriesManager;

            PlaceHolder = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);
            List<string> filesToDelete = new();

            string file_path;
            string culture = BuildsManager.getCultureString();
            string base_path = BuildsManager.s_moduleInstance.Paths.BasePath + @"\api\";

            SkillID_Pairs = JsonConvert.DeserializeObject<List<SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

            file_path = BuildsManager.s_moduleInstance.Paths.stats + @"stats [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Stats = JsonConvert.DeserializeObject<List<API.Stat>>(LoadFile(file_path, filesToDelete));
            }

            foreach (API.Stat stat in Stats) { stat.Icon._Texture = ContentsManager.GetTexture(stat.Icon.Path); stat.Attributes.Sort((a, b) => b.Multiplier.CompareTo(a.Multiplier)); foreach (API.StatAttribute attri in stat.Attributes) { attri.Name = attri.getLocalName(); attri.Icon._Texture = ContentsManager.GetTexture(attri.Icon.Path); } }

            file_path = BuildsManager.s_moduleInstance.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Professions = JsonConvert.DeserializeObject<List<API.Profession>>(LoadFile(file_path, filesToDelete));
            }

            file_path = BuildsManager.s_moduleInstance.Paths.runes + @"runes [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Runes = JsonConvert.DeserializeObject<List<API.RuneItem>>(LoadFile(file_path, filesToDelete));
            }

            file_path = BuildsManager.s_moduleInstance.Paths.sigils + @"sigils [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Sigils = JsonConvert.DeserializeObject<List<API.SigilItem>>(LoadFile(file_path, filesToDelete));
            }

            file_path = BuildsManager.s_moduleInstance.Paths.armory + @"armors [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Armors = JsonConvert.DeserializeObject<List<API.ArmorItem>>(LoadFile(file_path, filesToDelete));
            }

            file_path = BuildsManager.s_moduleInstance.Paths.armory + @"weapons [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Weapons = JsonConvert.DeserializeObject<List<API.WeaponItem>>(LoadFile(file_path, filesToDelete));
            }

            file_path = BuildsManager.s_moduleInstance.Paths.armory + @"trinkets [" + culture + "].json";
            if (System.IO.File.Exists(file_path))
            {
                Trinkets = JsonConvert.DeserializeObject<List<API.TrinketItem>>(LoadFile(file_path, filesToDelete));
            }

            Trinkets = Trinkets.OrderBy(e => e.TrinketType).ToList();
            Weapons = Weapons.OrderBy(e => (int)e.WeaponType).ToList();

            Texture2D texture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Bug);

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
                        if (legend.Heal.Icon != null && legend.Heal.Icon.Path != string.Empty)
                        {
                            legend.Heal.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }

                        if (legend.Elite.Icon != null && legend.Elite.Icon.Path != string.Empty)
                        {
                            legend.Elite.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }

                        if (legend.Swap.Icon != null && legend.Swap.Icon.Path != string.Empty)
                        {
                            legend.Swap.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }

                        if (legend.Skill.Icon != null && legend.Skill.Icon.Path != string.Empty)
                        {
                            legend.Skill.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }

                        foreach (API.Skill skill in legend.Utilities)
                        {
                            skill.Icon.ImageRegion = new Rectangle(12, 12, 104, 104);
                        }
                    }
                }
            }

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => BuildsManager.s_moduleInstance.DataLoaded = true);
        }
    }
}
