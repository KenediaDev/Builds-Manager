using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Blish_HUD;
using Blish_HUD.Controls;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Blish_HUD.Modules.Managers;

namespace Kenedia.Modules.BuildsManager
{
    public class LocalTexture : Texture2D
    {
        public LocalTexture(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
        }
        public LocalTexture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format)
        {
        }
        public LocalTexture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize)
        {
        }
        protected LocalTexture(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) : base(graphicsDevice, width, height, mipmap, format, type, shared, arraySize)
        {

        }

        public static DirectoriesManager DirectoriesManager;

        public API_ImageStates ImageState = API_ImageStates.Unkown;
        public string Path;

    }

    public enum API_ImageStates
    {
        Unkown,
        Loaded,
        Downloaded,
        Download_Queued,
        Load_Queued,
    }
    public enum IconTargets
    {
        Icon,
        Background,
        ProfessionIcon,
        ProfessionIconBig,
    }
    public class WebDownload_Image
    {
        public string Url;
        public string Path;
        public Action OnDownloadComplete;
    }
    public class Load_Image
    {
        public subtExtension Target;
        public string Path;
        public Action OnLoadComplete;
    }
    public class ObjectExtension
    {
        public subtExtension Icon = new subtExtension();
        public subtExtension Background = new subtExtension();
        public subtExtension ProfessionIcon = new subtExtension();
        public subtExtension ProfessionIconBig = new subtExtension();
    }
    public class subtExtension
    {
        public API_ImageStates ImageState = API_ImageStates.Unkown;
        public Texture2D Texture;
        public string Path;
        public string FileName;
        public string Url;
        public List<Control> Controls = new List<Control>();
        public List<Texture2D> Textures = new List<Texture2D>();

    }

    public static class Image_Extension
    {
        public static void setTexture(this Image ctrl, GW2API.Item item, string path)
        {
            ctrl.Texture = item.getIcon(path, ctrl);
            ctrl.BasicTooltipText = item.Name;
        }

    }
    public static class API_Extension
    {
        public static readonly ConditionalWeakTable<GW2API.BaseObject, ObjectExtension> Flags = new ConditionalWeakTable<GW2API.BaseObject, ObjectExtension>();

        public static Texture2D GetTextureFile(GW2API.BaseObject obj, string path, Object targetControl = null, IconTargets iconTarget = IconTargets.Icon)
        {
            if (path != null && path != "")
            {
                path = path.Replace(@"/", @"\");
                var objectExtension = Flags.GetOrCreateValue(obj);
                subtExtension oExt = objectExtension.Icon;

                switch (iconTarget)
                {
                    case IconTargets.Icon:
                        oExt = objectExtension.Icon;
                        break;

                    case IconTargets.Background:
                        oExt = objectExtension.Background;
                        break;

                    case IconTargets.ProfessionIcon:
                        oExt = objectExtension.ProfessionIcon;
                        break;

                    case IconTargets.ProfessionIconBig:
                        oExt = objectExtension.ProfessionIconBig;
                        break;
                }

                switch (targetControl)
                {
                    case Control control:
                        if (!oExt.Controls.Contains(targetControl)) oExt.Controls.Add(control);
                        break;

                    case Texture2D texture:
                        if (!oExt.Controls.Contains(targetControl)) oExt.Textures.Add(texture);
                        break;
                }

                if (oExt.FileName == null || oExt.FileName == "")
                {
                    switch (obj)
                    {
                        case GW2API.Item entry:
                            oExt.Url = entry.Icon.Url;
                            oExt.FileName = Regex.Match(entry.Icon.Url, "[0-9]*.png").ToString();
                            break;

                        case GW2API.Trait entry:
                            oExt.Url = entry.Icon.Url;
                            oExt.FileName = Regex.Match(entry.Icon.Url, "[0-9]*.png").ToString();
                            break;

                        case GW2API.Skill entry:
                            oExt.Url = entry.Icon.Url;
                            oExt.FileName = Regex.Match(entry.Icon.Url, "[0-9]*.png").ToString();
                            break;

                        case GW2API.Specialization entry:
                            oExt.Url = iconTarget == IconTargets.Background ? entry.Background.Url : entry.Icon.Url;
                            oExt.FileName = iconTarget == IconTargets.Background ? Regex.Match(entry.Background.Url, "[0-9]*.png").ToString() : Regex.Match(entry.Icon.Url, "[0-9]*.png").ToString();
                            break;
                    }

                    oExt.Path = path + (path.EndsWith(@"\") ? "" : @"\") + oExt.FileName;
                }

                if (oExt.Path != null && oExt.Path != "")
                {
                    if (oExt.ImageState == API_ImageStates.Unkown)
                    {
                        if (System.IO.File.Exists(oExt.Path))
                        {
                            if (!BuildsManager.load_ImagePaths.Contains(oExt.Path))
                            {
                                // Add Image to load queue
                                var img = new Load_Image()
                                {
                                    Target = oExt,
                                    Path = oExt.Path,
                                    OnLoadComplete = delegate
                                    {
                                        BuildsManager.Logger.Debug("Loaded Image from '{0}'.", oExt.Path);
                                        oExt.ImageState = API_ImageStates.Loaded;
                                        foreach (Image image in oExt.Controls)
                                        {
                                            if (image != null) image.Texture = oExt.Texture;
                                        }

                                        for (int i = 0; i < oExt.Textures.Count; i++)
                                        {
                                            oExt.Textures[i] = oExt.Texture;
                                        }
                                    },
                                };

                                BuildsManager.load_Images.Add(img);
                                oExt.ImageState = API_ImageStates.Load_Queued;
                            }
                        }
                        else
                        {
                            BuildsManager.download_Images.Add(new WebDownload_Image()
                            {
                                Url = oExt.Url,
                                Path = oExt.Path,
                                OnDownloadComplete = delegate
                                {
                                    BuildsManager.Logger.Debug("Download completed. Saved to '{0}'.", oExt.Path);
                                    oExt.ImageState = API_ImageStates.Downloaded;

                                    var img = new Load_Image()
                                    {
                                        Target = oExt,
                                        Path = oExt.Path,
                                        OnLoadComplete = delegate
                                        {
                                            BuildsManager.Logger.Debug("Loaded Image from '{0}'.", oExt.Path);
                                            oExt.ImageState = API_ImageStates.Loaded;
                                        },
                                    };

                                    BuildsManager.load_Images.Add(img);
                                    oExt.ImageState = API_ImageStates.Load_Queued;
                                },
                            });

                            oExt.ImageState = API_ImageStates.Download_Queued;
                        }
                    }

                    switch (oExt.ImageState)
                    {
                        case API_ImageStates.Loaded:
                            return oExt.Texture;

                        case API_ImageStates.Load_Queued:
                            return BuildsManager.DataManager._Icons[1];

                        case API_ImageStates.Download_Queued:
                            return BuildsManager.DataManager._Icons[1];

                        case API_ImageStates.Downloaded:
                            var img = new Load_Image()
                            {
                                Target = oExt,
                                Path = oExt.Path,
                                OnLoadComplete = delegate
                                {
                                    BuildsManager.Logger.Debug("Loaded Image from '{0}'.", oExt.Path);
                                    oExt.ImageState = API_ImageStates.Loaded;
                                },
                            };

                            BuildsManager.load_Images.Add(img);
                            oExt.ImageState = API_ImageStates.Load_Queued;
                            return BuildsManager.DataManager._Icons[1];

                        default:
                            return BuildsManager.DataManager._Icons[0];
                    }
                }
            }

            return BuildsManager.DataManager._Icons[0];
        }

        //Item
        public static Texture2D getIcon(this GW2API.Item o, string path = null, Object targetControl = null)
        {
            return GetTextureFile(o, path == null && o != null && o.Icon != null ? o.Icon.Path : path, targetControl);
        }
        public static Texture2D getIcon(this GW2API.Trait o, string path = null, Object targetControl = null)
        {
            return GetTextureFile(o, path == null && o != null && o.Icon != null ? o.Icon.Path : path, targetControl);
        }
        public static Texture2D getIcon(this GW2API.Skill o, string path = null, Object targetControl = null)
        {
            return GetTextureFile(o, path == null  && o != null && o.Icon != null ? o.Icon.Path : path, targetControl);
        }
        public static Texture2D getIcon(this GW2API.Specialization o, string path = null, Object targetControl = null)
        {
            return GetTextureFile(o, path == null && o != null && o.Icon != null ? o.Icon.Path : path, targetControl);
        }
        public static Texture2D getBackground(this GW2API.Specialization o, string path = null, Object targetControl = null)
        {
            return GetTextureFile(o, path == null && o != null && o.Background != null ? o.Background.Path : path, targetControl, IconTargets.Background);
        }
    }

    public class Armory
    {
        public List<ItemWeapon> Weapons;
        public List<ItemArmor> Armors;
        public List<ItemTrinket> Trinkets;
        public List<ItemBack> Backpacks;
        public List<ItemUpgradeComponent> Upgrades;
    }

    public class APIDownload_Image
    {
        public API_Image Parent;
        public string display_text = "";
        public string url;
        public string path;
    }

    public class API_Image
    {
        public List<Control> connectedControls = new List<Control>();
        public string Url;
        public bool fileChecked;
        public bool fileFetched;
        public bool fileLoaded;

        public string folderPath;

        private string _iconPath;
        private string _fileName;
        public string fileName
        {
            get
            {
                if (_fileName != null) return _fileName;
                _fileName = Regex.Match(Url, "[0-9]*.png").ToString();

                return _fileName;
            }
        }

        public string iconPath
        {
            get
            {
                return folderPath + @"/" + fileName;
            }
        }

        public Texture2D _Texture;
        public Texture2D Texture;
        bool fetchImage()
        {
            return false;
        }
    }

    public class GW2API
    {
        public static string BasePath;
        public class BaseObject
        {
            public int? Id;
            public string Name;
        }

        public class LegendaryItem
        {
            public class iType
            {
                public bool IsUnknown;
                public int Value;
                public string RawValue;
            }

            public class iDetails
            {
                public List<int> StatChoices;
                public iType Type;
            }
            public class iIcon
            {
                public string Url;
            }
            public class iRarity
            {
                public bool isSet;
                public bool IsUnknown;
                public int Value;
                public string RawValue;
            }

            public string Name;
            public iIcon Icon;
            public string ChatLink;
            public int Id;
            public iDetails Details;
            public iType Type;
            public iRarity Rarity;
            public ItemRarity __Rarity;
            public ItemRarity _Rarity
            {
                get
                {
                    if (Rarity.isSet) return __Rarity;

                    if (Rarity != null) 
                    {
                        switch (Rarity.Value)
                        {
                            case 0: __Rarity = ItemRarity.Unknown; break; 
                            case 1: __Rarity = ItemRarity.Junk; break; 
                            case 2: __Rarity = ItemRarity.Basic; break; 
                            case 3: __Rarity = ItemRarity.Fine; break; 
                            case 4: __Rarity = ItemRarity.Masterwork; break; 
                            case 5: __Rarity = ItemRarity.Rare; break; 
                            case 6: __Rarity = ItemRarity.Exotic; break; 
                            case 7: __Rarity = ItemRarity.Ascended; break; 
                            case 8: __Rarity = ItemRarity.Legendary; break; 
                        }
                    }

                    Rarity.isSet = true;
                    return __Rarity;
                }
            }

            //public API_Image api_Image;

            public Texture2D Texture;
        }

        public class Type
        {
            public string Value;
            public string RawValue;
        }
        public class Slot
        {
            public int Value;
            public string RawValue;
        }
        
        public class Rarity
        {
            public string Value;
            public string RawValue;
        }

        public class Fact
        {
            public double? Percent;
            public string Text;
            public string Description;
            public string Status;
            public Icon Icon;
            public Type Type;
            public int? RequiresTrait;
            public int? Overrides;
            public int? ApplyCount;
            public int? Duration;
        }
        public class Flag
        {
            public string Value;
            public string RawValue;
        }

        public class Details
        {
            public Type Type;
            public List<Flag> Flags;
            public List<string> Bonuses;
            public List<int> StatChoices;
            public double? AttributeAdjustment;
            public string Description;
        }

        public class Icon
        {
            public string Path;
            public string Url;
        }
                
        public class ProfessionSkill
        {
            public int Id;
        }

        public class Profession : BaseObject
        {
            public string Id;
            public IReadOnlyDictionary<int, int> SkillsByPalette;
            public List<ProfessionSkill> Skills;
            public List<Flag> Flags;
            public List<int> Specializations;
            public Icon IconBig;
            public Icon Icon;
            public int Code;
            public IReadOnlyDictionary<string, ProfessionWeapon> Weapons;
        }
        public class Item : BaseObject
        {
            public string Description;
            public Details Details;
            public Icon Icon;
            public List<Flag> Flags;
            public string ChatLink;
            public Rarity Rarity;
            public Type Type;
        }

        public class Skill : BaseObject
        {
            public int? Specialization;
            public string Description;
            public Slot Slot;
            public Type Type;
            public List<string> Categories;
            public Icon Icon;
            public int PaletteID;

            public List<Flag> Flags;
            public string ChatLink;
        }

        public class Trait : BaseObject
        {
            public string Description;
            public int? Specialization;
            public int? Tier;
            public int? Order;
            public Slot Slot;
            public List<Fact> Facts;
            public List<Fact> TraitedFacts;

            public List<string> Categories;
            public Icon Icon;

            public List<Flag> Flags;
            public string ChatLink;
        }

        public class Specialization : BaseObject
        {
            public string Profession;
            public bool Elite;
            public List<int> MinorTraits;
            public List<int> MajorTraits;
            public int? WeaponTrait;

            public Icon Icon;
            public Icon Background;
            public Icon ProfessionIconBig;
            public Icon ProfessionIcon;
            public List<Trait> Traits = new List<Trait>();
        }
    }
}
