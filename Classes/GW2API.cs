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
using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager
{
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
            return BuildsManager.ModuleInstance.TextureManager._Icons[0];
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
            return GetTextureFile(o, path == null && o != null && o.Icon != null ? o.Icon.Path : path, targetControl);
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
        public class intType
        {
            public int Value;
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
        public class intFlag
        {
            public int Value;
            public string RawValue;
        }

        public class Attribute
        {
            public int Value;
            public string RawValue;
        }

        public class WeightClass
        {
            public int Value;
            public string RawValue;
        }

        public class Stat
        {
            public Attribute Attribute;
            public string Value;
            public double Multiplier;
        }

        public class Stats : BaseObject
        {
            public List<Stat> Attributes;
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
        public class intDetails
        {
            public intType Type;
            public List<Flag> Flags;
            public List<string> Bonuses;
            public List<int> StatChoices;
            public double? AttributeAdjustment;
            public string Description;
            public WeightClass WeightClass;
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
        public class ProfessionWeapon
        {
            public int Specialization;
            public List<intFlag> Flags;
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
            public intDetails Details;
            public Icon Icon;
            public List<Flag> Flags;
            public string ChatLink;
            public Rarity Rarity;
            public intType Type;
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

    public class API
    {
        public static string UniformAttributeName(string statName)
        {
            switch (statName)
            {
                case "ConditionDamage":
                    return "Condition Damage";

                case "BoonDuration":
                    return "Concentration";

                case "ConditionDuration":
                    return "Expertise";

                case "Healing":
                    return "Healing Power";

                case "CritDamage":
                    return "Ferocity";
            }

            return statName;
        }
        public enum traitType
        {
            Minor = 1,
            Major = 2,
        }
        public enum skillSlot
        {
            Weapon_1 = 1,
            Weapon_2 = 2,
            Weapon_3 = 3,
            Weapon_4 = 4,
            Weapon_5 = 5,
            Profession_1 = 6,
            Profession_2 = 7,
            Profession_3 = 8,
            Profession_4 = 9,
            Profession_5 = 10,
            Heal = 11,
            Utility = 12,
            Elite = 13,
        }
        public enum armorSlot
        {
            Helm,
            Shoulders,
            Coat,
            Gloves,
            Leggings,
            Boots,
        }
        public enum weaponHand
        {
            Mainhand,
            TwoHand,
            DualWielded,
            Offhand,
            Aquatic,
        }
        public enum weaponSlot
        {
            Axe = weaponHand.DualWielded,
            Dagger = weaponHand.DualWielded,
            Mace = weaponHand.DualWielded,
            Pistol = weaponHand.DualWielded,
            Scepter = weaponHand.Mainhand,
            Sword = weaponHand.DualWielded,
            Focus = weaponHand.Offhand,
            Shield = weaponHand.Offhand,
            Torch = weaponHand.Offhand,
            Warhorn = weaponHand.Offhand,
            Greatsword = weaponHand.TwoHand,
            Hammer = weaponHand.TwoHand,
            Longbow = weaponHand.TwoHand,
            Rifle = weaponHand.TwoHand,
            Shortbow = weaponHand.TwoHand,
            Staff = weaponHand.TwoHand,
            Harpoon = weaponHand.Aquatic,
            Speargun = weaponHand.Aquatic,
            Trident = weaponHand.Aquatic,
        }
        public enum trinketType
        {
            Back,
            Amulet,
            Accessory,
            Ring,
        }
        public enum armorWeight
        {
            Heavy = 1,
            Medium,
            Light,
        }
        public enum upgradeType
        {
            Rune = 3,
            Sigil,
        }
        public enum weaponType
        {
            Unkown = -1,
            Axe,
            Dagger,
            Mace,
            Pistol,
            Scepter,
            Sword,
            Focus,
            Shield,
            Torch,
            Warhorn,
            Greatsword,
            Hammer,
            Longbow,
            Rifle,
            Shortbow,
            Staff,
            Harpoon,
            Speargun,
            Trident,

            Spear = 16,
            ShortBow = 14,
            LongBow = 12,
        }

        public class JsonIcon
        {
            public string Path;
            public string Url;
        }

        public class Icon : IDisposable
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    _Texture?.Dispose();
                    //_AsyncTexture?.Dispose();
                }
            }

            public string Path;
            public string Url;

            public AsyncTexture2D _Texture;
            public Microsoft.Xna.Framework.Rectangle ImageRegion;
            public Microsoft.Xna.Framework.Rectangle DefaultBounds;

            public AsyncTexture2D _AsyncTexture
            {
                get
                {
                    if (_Texture == null && !BuildsManager.ModuleInstance.FetchingAPI)
                    {
                        _Texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);

                        Task.Run(() =>
                        {
                            var fs = new FileStream(BuildsManager.ModuleInstance.Paths.BasePath + Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                            {
                                var texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, fs);

                                if (ImageRegion != null)
                                {
                                    double factor = 1;

                                    if (DefaultBounds != default)
                                    {
                                        factor = (double)texture.Width / (double)DefaultBounds.Width;
                                    }

                                    ImageRegion = ImageRegion.Scale(factor);

                                    if (texture.Bounds.Width > 0 && ImageRegion.Width > 0 && texture.Bounds.Contains(ImageRegion))
                                    {
                                        texture = texture.GetRegion(ImageRegion);
                                    }
                                }

                                _Texture.SwapTexture(texture);
                                fs.Close();
                            });
                        });
                    }

                    return _Texture;
                }
            }
        }

        #region Items
        public class Item
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon?.Dispose();
                }
            }

            public string Name;
            public int Id;
            public Icon Icon;
            public string ChatLink;
        }
        public class EquipmentItem : Item
        {
            public double AttributeAdjustment;
        }
        public class ArmorItem : EquipmentItem
        {
            public armorSlot Slot;
            public armorWeight ArmorWeight;

        }
        public class WeaponItem : EquipmentItem
        {
            public weaponType WeaponType;
            public weaponSlot Slot;
        }
        public class TrinketItem : EquipmentItem
        {
            public trinketType TrinketType;
        }
        public class RuneItem : Item
        {
            public upgradeType Type = upgradeType.Rune;
            public List<string> Bonuses;
        }
        public class SigilItem : Item
        {
            public upgradeType Type = upgradeType.Sigil;
            public string Description; //InfixUpgrade.Buff.Description
        }
        #endregion

        public class Skill
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon?.Dispose();
                }
            }

            public string Name;
            public int Id;
            public int Specialization;
            public int PaletteId;
            public Icon Icon;
            public string ChatLink;
            public string Description;
            public skillSlot Slot;
            public List<string> Flags;
            public List<string> Categories;
        }
        public class Legend
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Heal?.Dispose();
                    Elite?.Dispose();
                    Swap?.Dispose();
                    Skill?.Dispose();

                    foreach (Skill e in Utilities) { e.Dispose(); }
                }
            }

            public string Name; 
            public int Id;
            public List<Skill> Utilities;
            public Skill Heal;
            public Skill Elite;
            public Skill Swap;
            public Skill Skill;
            public int Specialization;
        }

        public class Trait
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon?.Dispose();
                }
            }

            public string Name;
            public int Id;
            public Icon Icon;
            public int Specialization;
            public string Description;
            public int Tier;
            public int Order;
            public traitType Type;
        }

        public class Specialization
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon?.Dispose();
                    Background?.Dispose();
                    ProfessionIcon?.Dispose();
                    ProfessionIconBig?.Dispose();
                    WeaponTrait?.Dispose();

                    foreach (Trait e in MinorTraits) { e.Dispose(); }
                    foreach (Trait e in MajorTraits) { e.Dispose(); }
                }
            }

            public string Name;
            public int Id;
            public Icon Icon;
            public Icon Background;
            public Icon ProfessionIcon;
            public Icon ProfessionIconBig;
            public string Profession;
            public bool Elite;

            public Trait WeaponTrait;
            public List<Trait> MinorTraits = new List<Trait>();
            public List<Trait> MajorTraits = new List<Trait>();
        }
        public class ProfessionWeapon
        {
            public int Specialization;
            public weaponType Weapon;
            public List<weaponHand> Wielded;
        }
        public class Profession
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon?.Dispose();
                    IconBig?.Dispose();

                    foreach(Specialization e in Specializations) { e.Dispose(); }
                    foreach(Skill e in Skills) { e.Dispose(); }
                    foreach(Legend e in Legends) { e.Dispose(); }
                }
            }

            public string Name;
            public string Id;
            public Icon Icon;
            public Icon IconBig;
            public List<Specialization> Specializations = new List<Specialization>();
            public List<ProfessionWeapon> Weapons = new List<ProfessionWeapon>();
            public List<Skill> Skills = new List<Skill>();
            public List<Legend> Legends = new List<Legend>();
        }

        public class StatAttribute
        {
            public int Id;
            public string Name;
            public double Multiplier;
            public int Value;
            public Icon Icon;

            public string getLocalName
            {
                get
                {
                    string text = Name;

                    switch (Id)
                    {
                        case (int) _Stats.Power:
                            return Strings.common.Power;

                        case (int) _Stats.Precision:
                            return Strings.common.Precision;

                        case (int) _Stats.Toughness:
                            return Strings.common.Toughness;

                        case (int) _Stats.Vitality:
                            return Strings.common.Vitality;

                        case (int) _Stats.Ferocity:
                            return Strings.common.Ferocity;

                        case (int) _Stats.HealingPower:
                            return Strings.common.HealingPower;

                        case (int) _Stats.ConditionDamage:
                            return Strings.common.ConditionDamage;

                        case (int) _Stats.Concentration:
                            return Strings.common.Concentration;

                        case (int) _Stats.Expertise:
                            return Strings.common.Expertise;
                    }

                    return text;
                }
            }
        }
        public class Stat : IDisposable
        {
            private bool disposed = false;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Icon.Dispose();
                }
            }

            public int Id;
            public string Name;
            public List<StatAttribute> Attributes = new List<StatAttribute>();
            public Icon Icon;
        }
    }
}
