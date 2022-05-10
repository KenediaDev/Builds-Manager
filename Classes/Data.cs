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

namespace Kenedia.Modules.BuildsManager
{
    public class iData
    {
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

        public static ContentsManager ContentsManager;
        public static DirectoriesManager DirectoriesManager;

        public List<API.Stat> Stats = new List<API.Stat>();
        public List<API.Profession> Professions = new List<API.Profession>();
        public List<API.RuneItem> Runes = new List<API.RuneItem>();
        public List<API.SigilItem> Sigils = new List<API.SigilItem>();
        public List<API.ArmorItem> Armors = new List<API.ArmorItem>();
        public List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
        public List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();
        public List<SkillID_Pair> SkillID_Pairs = new List<SkillID_Pair>();
        public List<API.Legend> Legends = new List<API.Legend>();

        private bool fetchAPI;
        static Texture2D PlaceHolder;

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
                    texture = BuildsManager.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("InvalidOperationException: Failed to load {0}. Fetching the API again.", path);
                    fetchAPI = true;
                    return texture;
                }
                catch (UnauthorizedAccessException)
                {
                    texture = BuildsManager.TextureManager.getIcon(_Icons.Bug);
                    return texture;
                }
                catch (FileNotFoundException)
                {
                    texture = BuildsManager.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("FileNotFoundException: Failed to load {0}. Fetching the API again.", path);
                    fetchAPI = true;
                    return texture;
                }
                catch (FileLoadException)
                {
                    texture = BuildsManager.TextureManager.getIcon(_Icons.Bug);
                    BuildsManager.Logger.Debug("FileLoadException: Failed to load {0}. Fetching the API again.", path);
                    fetchAPI = true;
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
                    fetchAPI = true;
                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (FileNotFoundException)
                {
                    fetchAPI = true;
                }
                catch (FileLoadException)
                {
                    fetchAPI = true;
                }
            }

            return txt;
        }

        public iData(ContentsManager contentsManager = null, DirectoriesManager directoriesManager = null)
        {
            if (contentsManager != null) ContentsManager = contentsManager;
            if (directoriesManager != null) DirectoriesManager = directoriesManager;

            PlaceHolder = BuildsManager.TextureManager.getIcon(_Icons.Bug);
            List<string> filesToDelete = new List<string>();

            string file_path;
            var culture = BuildsManager.getCultureString();
            var base_path = BuildsManager.Paths.BasePath + @"\api\";

            SkillID_Pairs = JsonConvert.DeserializeObject<List<SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

            file_path = BuildsManager.Paths.stats + @"stats [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Stats = JsonConvert.DeserializeObject<List<API.Stat>>(LoadFile(file_path, filesToDelete));
            foreach (API.Stat stat in Stats) { stat.Icon.Texture = ContentsManager.GetTexture(stat.Icon.Path); stat.Attributes.Sort((a, b) => b.Multiplier.CompareTo(a.Multiplier)); foreach (API.StatAttribute attri in stat.Attributes) { attri.Icon.Texture = ContentsManager.GetTexture(attri.Icon.Path); } }

            file_path = BuildsManager.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Professions = JsonConvert.DeserializeObject<List<API.Profession>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.Paths.runes + @"runes [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Runes = JsonConvert.DeserializeObject<List<API.RuneItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.Paths.sigils + @"sigils [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Sigils = JsonConvert.DeserializeObject<List<API.SigilItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.Paths.armory + @"armors [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Armors = JsonConvert.DeserializeObject<List<API.ArmorItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.Paths.armory + @"weapons [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Weapons = JsonConvert.DeserializeObject<List<API.WeaponItem>>(LoadFile(file_path, filesToDelete));

            file_path = BuildsManager.Paths.armory + @"trinkets [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Trinkets = JsonConvert.DeserializeObject<List<API.TrinketItem>>(LoadFile(file_path, filesToDelete));

            Trinkets = Trinkets.OrderBy(e => e.TrinketType).ToList();
            Weapons = Weapons.OrderBy(e => (int)e.WeaponType).ToList();


            Texture2D texture = BuildsManager.TextureManager.getIcon(_Icons.Bug);
            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                foreach (API.TrinketItem item in Trinkets)
                {
                    item.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + item.Icon.Path, graphicsDevice, filesToDelete);
                }

                foreach (API.WeaponItem item in Weapons)
                {
                    item.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + item.Icon.Path, graphicsDevice, filesToDelete);
                }
                foreach (API.ArmorItem item in Armors)
                {
                    item.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + item.Icon.Path, graphicsDevice, filesToDelete);
                }
                foreach (API.SigilItem item in Sigils)
                {
                    item.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + item.Icon.Path, graphicsDevice, filesToDelete);
                }
                foreach (API.RuneItem item in Runes)
                {
                    item.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + item.Icon.Path, graphicsDevice, filesToDelete);
                }

                foreach (API.Profession profession in Professions)
                {
                    //Icon
                    profession.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + profession.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(4, 4, 26, 26));

                    //IconBig
                    profession.IconBig.Texture = LoadImage(BuildsManager.Paths.BasePath + profession.IconBig.Path, graphicsDevice, filesToDelete);

                    foreach (API.Specialization specialization in profession.Specializations)
                    {

                        //Icon
                        specialization.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + specialization.Icon.Path, graphicsDevice, filesToDelete);

                        //Background
                        specialization.Background.Texture = LoadImage(BuildsManager.Paths.BasePath + specialization.Background.Path, graphicsDevice, filesToDelete, new Rectangle(0, 123, 643, 123));

                        if (specialization.ProfessionIcon != null)
                        {
                            //ProfessionIcon
                            specialization.ProfessionIcon.Texture = LoadImage(BuildsManager.Paths.BasePath + specialization.ProfessionIcon.Path, graphicsDevice, filesToDelete);
                        }
                        if (specialization.ProfessionIconBig != null)
                        {
                            //ProfessionIconBig
                            specialization.ProfessionIconBig.Texture = LoadImage(BuildsManager.Paths.BasePath + specialization.ProfessionIconBig.Path, graphicsDevice, filesToDelete);
                        }
                        if (specialization.WeaponTrait != null)
                        {
                            //WeaponTrait Icon
                            specialization.WeaponTrait.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + specialization.WeaponTrait.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(3, 3, 58, 58));
                        }


                        foreach (API.Trait trait in specialization.MajorTraits)
                        {
                            trait.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + trait.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(3, 3, 58, 58), new Rectangle(0, 0, 64, 64));
                        }

                        foreach (API.Trait trait in specialization.MinorTraits)
                        {
                            trait.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + trait.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(3, 3, 58, 58));
                        }
                    }

                    foreach (API.Skill skill in profession.Skills)
                    {
                        skill.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + skill.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));
                    }

                    if (profession.Legends.Count > 0)
                    {
                        foreach (API.Legend legend in profession.Legends)
                        {
                            BuildsManager.Logger.Debug("Loading " + legend.Name);

                            if(legend.Heal.Icon != null && legend.Heal.Icon.Path != "") legend.Heal.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + legend.Heal.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));
                            if (legend.Elite.Icon != null && legend.Elite.Icon.Path != "") legend.Elite.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + legend.Elite.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));
                            if (legend.Swap.Icon != null && legend.Swap.Icon.Path != "") legend.Swap.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + legend.Swap.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));
                            if (legend.Skill.Icon != null && legend.Skill.Icon.Path != "") legend.Skill.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + legend.Skill.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));

                            BuildsManager.Logger.Debug("Loading Utility of " + legend.Name);
                            foreach (API.Skill skill in legend.Utilities)
                            {
                                if (skill.Icon != null && skill.Icon.Path != "") skill.Icon.Texture = LoadImage(BuildsManager.Paths.BasePath + skill.Icon.Path, graphicsDevice, filesToDelete, new Rectangle(12, 12, 104, 104));
                            }
                        }
                    }
                }

                foreach (string path in filesToDelete)
                {
                    try
                    {
                        System.IO.File.Delete(path);
                    }
                    catch (IOException)
                    {

                    }
                }

                if (fetchAPI)
                {
                    fetchAPI = false;
                    BuildsManager.ModuleInstance.Fetch_APIData(true);
                    return;
                }

                BuildsManager.DataLoaded = true;
            });
        }
    }
}
