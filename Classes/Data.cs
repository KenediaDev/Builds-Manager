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

namespace Kenedia.Modules.BuildsManager
{
   public class iData
    {
        public static ContentsManager ContentsManager;
        public static DirectoriesManager DirectoriesManager;

        public List<API.Stat> Stats = new List<API.Stat>();
        public List<API.Profession> Professions = new List<API.Profession>();
        public List<API.RuneItem> Runes = new List<API.RuneItem>();
        public List<API.SigilItem> Sigils = new List<API.SigilItem>();
        public List<API.ArmorItem> Armors = new List<API.ArmorItem>();
        public List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
        public List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();

        public iData(ContentsManager contentsManager = null, DirectoriesManager directoriesManager = null)
        {
            if (contentsManager != null) ContentsManager = contentsManager;
            if(directoriesManager != null) DirectoriesManager = directoriesManager;

            string file_path;
            var culture = BuildsManager.getCultureString();
            var base_path = BuildsManager.Paths.BasePath+ @"\api\";

            file_path = BuildsManager.Paths.stats + @"stats [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Stats = JsonConvert.DeserializeObject<List<API.Stat>>(System.IO.File.ReadAllText(file_path));
            foreach (API.Stat stat in Stats) { stat.Icon.Texture = ContentsManager.GetTexture(stat.Icon.Path); foreach (API.StatAttribute attri in stat.Attributes) { attri.Icon.Texture = ContentsManager.GetTexture(attri.Icon.Path); } }

            file_path = BuildsManager.Paths.professions + @"professions [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Professions = JsonConvert.DeserializeObject<List<API.Profession>>(System.IO.File.ReadAllText(file_path));

            file_path = BuildsManager.Paths.runes + @"runes [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Runes = JsonConvert.DeserializeObject<List<API.RuneItem>>(System.IO.File.ReadAllText(file_path));

            file_path = BuildsManager.Paths.sigils + @"sigils [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Sigils = JsonConvert.DeserializeObject<List<API.SigilItem>>(System.IO.File.ReadAllText(file_path));

            file_path = BuildsManager.Paths.armory + @"armors [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Armors = JsonConvert.DeserializeObject<List<API.ArmorItem>>(System.IO.File.ReadAllText(file_path));

            file_path = BuildsManager.Paths.armory + @"weapons [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Weapons = JsonConvert.DeserializeObject<List<API.WeaponItem>>(System.IO.File.ReadAllText(file_path));

            file_path = BuildsManager.Paths.armory + @"trinkets [" + culture + "].json";
            if (System.IO.File.Exists(file_path)) Trinkets = JsonConvert.DeserializeObject<List<API.TrinketItem>>(System.IO.File.ReadAllText(file_path));

            Trinkets = Trinkets.OrderBy(e => e.TrinketType).ToList();
            Weapons = Weapons.OrderBy(e => (int) e.WeaponType).ToList();

            Texture2D texture; 
            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                foreach(API.TrinketItem item in Trinkets) { item.Icon.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(item.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); }
                foreach(API.WeaponItem item in Weapons) { item.Icon.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(item.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); }
                foreach(API.ArmorItem item in Armors) { item.Icon.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(item.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); }
                foreach(API.SigilItem item in Sigils) { item.Icon.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(item.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); }
                foreach(API.RuneItem item in Runes) { item.Icon.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(item.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); }

                foreach (API.Profession profession in Professions)
                {
                    texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(profession.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    profession.Icon.Texture = texture;

                    texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(profession.IconBig.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    profession.IconBig.Texture = texture;

                    foreach (API.Specialization specialization in profession.Specializations)
                    {
                        texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(specialization.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        specialization.Icon.Texture = texture;

                        texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(specialization.Background.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        specialization.Background.Texture = texture.GetRegion(0, texture.Height - 133, texture.Width - (texture.Width - 643), texture.Height - (texture.Height - 133));

                        if (specialization.ProfessionIcon != null)
                        {
                            texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(specialization.ProfessionIcon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                            specialization.ProfessionIcon.Texture = texture;
                        }
                        if (specialization.ProfessionIconBig != null)
                        {
                            texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(specialization.ProfessionIconBig.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                            specialization.ProfessionIconBig.Texture = texture;
                        }
                        if (specialization.WeaponTrait != null)
                        {
                            texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(specialization.WeaponTrait.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                            specialization.WeaponTrait.Icon.Texture = texture.GetRegion(3, 3, texture.Width - 6, texture.Height - 6);
                        }


                        foreach (API.Trait trait in specialization.MajorTraits)
                        {
                            texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(trait.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                            trait.Icon.Texture = texture.GetRegion(3, 3, texture.Width - 6, texture.Height - 6);
                        }

                        foreach (API.Trait trait in specialization.MinorTraits)
                        {
                            texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(trait.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                            trait.Icon.Texture = texture.GetRegion(3, 3, texture.Width - 6, texture.Height - 6);
                        }
                    }

                    foreach (API.Skill skill in profession.Skills)
                    {
                        texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(skill.Icon.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        skill.Icon.Texture = texture.GetRegion(12, 12, texture.Width - (12 * 2), texture.Height - (12 * 2));
                    }
                }

                BuildsManager.DataLoaded = true;
            });
        }
    }
}
