using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.ChatLinks;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager
{
    public class TemplateItem
    {
        public GW2API.Item API_Item;
        public _EquipmentSlots Slot;
        public Stat _SelectedStat;
        public Stat SelectedStat { 
            get => _SelectedStat; 
            set {
                _SelectedStat = value;
                Stats = new List<int>();

                if (value != null)
                {
                    Stat = value.Id;

                    foreach (GW2API.Stat stat in value.Attributes)
                    {
                        Stats.Add(stat.Attribute.Value);
                    }
                }
            } 
        }

        public int Stat;
        public List<int> Stats = new List<int>();
        public Texture2D Texture;
        public Rectangle Bounds;
        public Rectangle StatBounds;
        public List<TemplateItem> Upgrades = new List<TemplateItem>();
        public bool Hovered;
        public bool TextureLoaded;
        public bool Visible = true;

        public TemplateItem()
        {

        }
    }
    public class GearTemplate
    {
        public List<TemplateItem> Equipment = new List<TemplateItem>(new TemplateItem[19]);
    }
    public class Stat
    {
        public int Id;
        public string Name;
        public Texture2D Texture;
        public List<GW2API.Stat> Attributes;
        public string MainStat;
        public Rectangle Bounds;
        public bool Hovered;
    }

    public class Equipment : Control
    {
        private Build _Build;
        public Build Build 
        {
            get => _Build;
            set
            {
                if (value != null)
                {
                    _Build = value;
                    _Build.TemplateChanged += delegate { ApplyTemplate(Build);
                    };
                    ApplyTemplate(value);
                }
            }
        }

        private Gw2Sharp.Models.ProfessionType _Profession;
        public Gw2Sharp.Models.ProfessionType Profession 
        {
            get => _Profession;
            set
            {
                _Profession = value;
            }
        }

        public _ArmorWeight ArmorWeight;
        public double Scale;
        public GearTemplate Gear;
        private Texture2D _RuneTexture;
        private TemplateItem SigilSelector;
        private TemplateItem RuneSelector;
        private TemplateItem ItemSelector;
        private TemplateItem StatSelector;
        private Rectangle ItemSelectorBounds;
        private Rectangle RuneSelectorBounds;
        private Rectangle SigilSelectorBounds;
        private int UpgradeIndex;
        private List<TemplateItem> SelectionItems;
        private List<TemplateItem> Runes = new List<TemplateItem>();
        private List<TemplateItem> Sigils = new List<TemplateItem>();
        private List<Stat> Stats;

        public TextBox FilterBox;

        private int ItemSize
        {
            get { return (int) (_RuneTexture.Width * Scale); }
        }
        public Equipment(GearTemplate gear)
        {
            ClipsBounds = false;
            Stats = new List<Stat>();

            var values = Enum.GetValues(typeof(_EquipmentStats));
            foreach (_EquipmentStats stat in values)
            {
                var info = BuildsManager.DataManager.getStat_Info(stat);

                var temp = new Stat()
                {
                    Name = info.Name,
                    Id = (int)info.Id,
                    Attributes = info.Attributes,
                    Texture = BuildsManager.DataManager.getStat_Texture(stat),
                    MainStat = info.Attributes.Aggregate((a, b) => a.Multiplier > b.Multiplier ? a : b).Attribute.RawValue,
                };

                Stats.Add(temp);
            }

            Stats = Stats.OrderBy(x => x.Name)
                                    .ToList();

            foreach (GW2API.Item rune in BuildsManager.Data.Runes)
            {
                var temp = new TemplateItem()
                {
                    API_Item = rune,
                    Texture = rune.getIcon(),
                };

                Runes.Add(temp);
            }

            foreach(GW2API.Item sigil in BuildsManager.Data.Sigils)
            {
                var temp = new TemplateItem()
                {
                    API_Item = sigil,
                    Texture = sigil.getIcon(),
                };

                Sigils.Add(temp);
            }

            _RuneTexture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            Input.Mouse.LeftMouseButtonReleased += OnClick;
            Input.Mouse.RightMouseButtonReleased += OnClick;

            Gear = gear;
            Gear.Equipment = new List<TemplateItem>();

            FilterBox = new TextBox()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Width = 300,
                PlaceholderText = "Search ...",
                Visible = false,
                ZIndex = 1000,
            };
            FilterBox.TextChanged += delegate {
                if (RuneSelector != null)
                {
                    foreach (TemplateItem rune in Runes)
                    {
                        rune.Visible = FilterBox.Text == "" || rune.API_Item.Name.ToLower().Contains(FilterBox.Text.ToLower());
                    }
                }

                if (SigilSelector != null)
                {
                    foreach (TemplateItem sigil in Sigils)
                    {
                        sigil.Visible = FilterBox.Text == "" || sigil.API_Item.Name.ToLower().Contains(FilterBox.Text.ToLower());
                    }
                }
            };

            values = Enum.GetValues(typeof(_EquipmentSlots));
            bool addUpgrades = true;

            foreach (_EquipmentSlots slot in values)
            {
                if (slot != _EquipmentSlots.Unkown)
                {
                    _EquipmentTextures enumTexture;
                    if (!Enum.TryParse(slot.ToString(), out enumTexture)) 
                    {
                        switch (slot)
                        {
                            case _EquipmentSlots.AquaticWeapon1:
                            case _EquipmentSlots.AquaticWeapon2:
                                enumTexture = _EquipmentTextures.AquaticWeapon;
                                break;

                            case _EquipmentSlots.Weapon1_MainHand:
                            case _EquipmentSlots.Weapon2_MainHand:
                                enumTexture = _EquipmentTextures.Mainhand_Weapon;
                                break;

                            case _EquipmentSlots.Weapon1_OffHand:
                            case _EquipmentSlots.Weapon2_OffHand:
                                enumTexture = _EquipmentTextures.Offhand_Weapon;
                                break;
                        }
                    };

                    var temp = new TemplateItem()
                    {
                        Slot = slot,
                        SelectedStat = Stats[2],
                        Texture = BuildsManager.DataManager.getEquipTexture(enumTexture).GetRegion(37, 37, 54, 54),
                    };


                    switch (slot)
                    {
                        case _EquipmentSlots.Amulet:
                            var item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 95380);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;

                        case _EquipmentSlots.Accessoire1:
                            item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 81908);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;

                        case _EquipmentSlots.Accessoire2:
                            item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 91048);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;

                        case _EquipmentSlots.Ring1:
                            item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 91234);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;

                        case _EquipmentSlots.Ring2:
                            item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 93105);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;

                        case _EquipmentSlots.Back:
                            item = BuildsManager.Data.LegendaryItems.Find(e => e.Id == 74155);
                            temp.API_Item = item;
                            temp.Texture = item.getIcon();
                            break;
                    }

                    if(addUpgrades) temp.Upgrades.Add(new TemplateItem() { Slot = slot, Texture = _RuneTexture });
                    if (slot == _EquipmentSlots.AquaticWeapon1 || slot == _EquipmentSlots.AquaticWeapon2) temp.Upgrades.Add(new TemplateItem() { Slot = slot, Texture = _RuneTexture }); 
                    if (slot == _EquipmentSlots.AquaticWeapon2) addUpgrades = false;

                    Gear.Equipment.Add(temp);
                }
            }

            Disposed += delegate { FilterBox.Dispose(); };
        }
        public void OnClick(object sender, MouseEventArgs mouse)
        {
            iClick(sender, mouse);
        }
        public void ApplyTemplate(Build value)
        {
            Profession = value.BuildTemplate.Profession;
            ArmorWeight = value.BuildTemplate.Profession.GetArmorWeight();

            foreach (TemplateItem item in Gear.Equipment)
            {
                var isArmor = item.Slot >= _EquipmentSlots.Helmet && item.Slot <= _EquipmentSlots.Boots;
                var isJuwellery = item.Slot >= _EquipmentSlots.Back && item.Slot <= _EquipmentSlots.Accessoire2;

                if (isArmor)
                {
                    foreach (GW2API.Item legyItem in BuildsManager.Data.LegendaryItems)
                    {
                        var match = isArmor && legyItem.Type.RawValue == "Armor" && item.Slot.GetArmorSlot().ToString() == legyItem.Details.Type.RawValue && legyItem.Details.WeightClass.Value == (int)ArmorWeight;
                        if (match && (BuildsManager.ArmoryItems.Count == 0 || BuildsManager.ArmoryItems.Contains((int)legyItem.Id)))
                        {
                            item.API_Item = legyItem;
                            item.Texture = legyItem.getIcon();
                        }
                    }
                }
            }
        }
        public bool iClick(object sender, MouseEventArgs mouse)
        {
            UpdateLayout(RelativeMousePosition);
            // SigilSelector = null;
            // RuneSelector = null;
            // ItemSelector = null;


            if (StatSelector != null)
            {
                foreach (Stat stat in Stats)
                {
                    if (stat.Hovered)
                    {
                        StatSelector.SelectedStat = stat;
                        StatSelector.Stat = stat.Id;
                        StatSelector = null;
                        return true;
                    }
                }
            }

            if (ItemSelector != null)
            {
                foreach (TemplateItem item in SelectionItems)
                {
                    if(item.Hovered)
                    {
                        BuildsManager.Logger.Debug(item.API_Item.Name);

                        ItemSelector.API_Item = item.API_Item;
                        ItemSelector.Texture = item.API_Item.getIcon();

                        if (item.Slot == _EquipmentSlots.Weapon1_MainHand && Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].API_Item != null)
                        {
                            var profession = BuildsManager.Data.Professions.Find(e => e.Id == Build.BuildTemplate.Profession.ToString());

                            if (profession != null)
                            {
                                GW2API.ProfessionWeapon weapon;
                                profession.Weapons.TryGetValue(item.API_Item.Details.convertWeaponType(), out weapon);

                                if (weapon != null && weapon.Flags.Find(e => e.RawValue == "TwoHand") != null) 
                                {
                                    Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].API_Item = null;
                                    Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Offhand_Weapon).GetRegion(37, 37, 54, 54);
                                }
                            }
                        }

                        if (item.Slot == _EquipmentSlots.Weapon2_MainHand && Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].API_Item != null)
                        {
                            var profession = BuildsManager.Data.Professions.Find(e => e.Id == Build.BuildTemplate.Profession.ToString());

                            if (profession != null)
                            {
                                GW2API.ProfessionWeapon weapon;
                                profession.Weapons.TryGetValue(item.API_Item.Details.convertWeaponType(), out weapon);

                                if (weapon != null && weapon.Flags.Find(e => e.RawValue == "TwoHand") != null)
                                {
                                    Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].API_Item = null;
                                    Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Offhand_Weapon).GetRegion(37, 37, 54, 54);

                                }
                            }
                        }


                        ItemSelector = null;
                        return true;
                    }
                }
                if (ItemSelector == null) SelectionItems = null;
            }

            if(RuneSelector != null)
            {
                var runeList = Runes.Where(e => e.Visible);

                foreach (TemplateItem rune in runeList)
                {
                    if (rune.Hovered)
                    {
                        RuneSelector.Upgrades[UpgradeIndex] = new TemplateItem() { API_Item = rune.API_Item, Texture = rune.API_Item.getIcon() };
                        RuneSelector = null;
                        FilterBox.Hide();
                        return true;
                    }
                }
            }

            if(SigilSelector != null)
            {
                var sigilList = Sigils.Where(e => e.Visible);

                foreach (TemplateItem sigil in sigilList)
                {
                    if (sigil.Hovered)
                    {
                        SigilSelector.Upgrades[UpgradeIndex] = new TemplateItem() { API_Item = sigil.API_Item, Texture = sigil.API_Item.getIcon() };
                        SigilSelector = null;
                        FilterBox.Hide();
                        return true;
                    }
                }
            }

            foreach (TemplateItem item in Gear.Equipment)
            {
                if (item.Hovered && (Build != null && Build.BuildTemplate != null && Build.BuildTemplate.Profession != 0) && (SigilSelector == null && RuneSelector == null))
                {
                    if (mouse.EventType == MouseEventType.RightMouseButtonReleased && (ItemSelector == null || ItemSelector != item))
                    {
                        bool canSelect = item.Slot >= _EquipmentSlots.Weapon1_MainHand && item.Slot <= _EquipmentSlots.AquaticWeapon2;
                        if(item.Slot == _EquipmentSlots.Weapon1_OffHand)
                        {
                            var mainHand = Gear.Equipment[(int)_EquipmentSlots.Weapon1_MainHand].API_Item;

                            var profession = BuildsManager.Data.Professions.Find(e => e.Id == Build.BuildTemplate.Profession.ToString());
                            if (mainHand != null && profession != null)
                            {
                                GW2API.ProfessionWeapon weapon;
                                profession.Weapons.TryGetValue(mainHand.Details.convertWeaponType(), out weapon);

                                if (weapon != null && weapon.Flags.Find(e => e.RawValue == "TwoHand") != null) canSelect = false;
                            }
                        }
                        else if(item.Slot == _EquipmentSlots.Weapon2_OffHand)
                        {
                            var mainHand = Gear.Equipment[(int)_EquipmentSlots.Weapon2_MainHand].API_Item;

                            var profession = BuildsManager.Data.Professions.Find(e => e.Id == Build.BuildTemplate.Profession.ToString());

                            if (mainHand != null && profession != null)
                            {
                                GW2API.ProfessionWeapon weapon;
                                profession.Weapons.TryGetValue(mainHand.Details.convertWeaponType(), out weapon);

                                if (weapon != null && weapon.Flags.Find(e => e.RawValue == "TwoHand") != null) canSelect = false;
                            }
                        }

                        if(canSelect ) getArmorySelection(item);

                        return true;
                    }
                    else
                    {
                        ItemSelector = null;
                        SelectionItems = null;
                    }


                    if (mouse.EventType == MouseEventType.LeftMouseButtonReleased)
                    {
                        StatSelector = (StatSelector == null || StatSelector != item) ? item : null;
                    }

                    return true;
                }

                if (item.Upgrades.Count > 0 && StatSelector == null)
                {
                    int index = 0;
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        var isArmor = item.Slot >= _EquipmentSlots.Helmet && item.Slot <= _EquipmentSlots.Boots;
                        var isWeapon = item.Slot >= _EquipmentSlots.Weapon1_MainHand && item.Slot <= _EquipmentSlots.AquaticWeapon2;
                        UpgradeIndex = index;

                        if (upgrade.Hovered && isWeapon)
                        {
                            SigilSelector = (SigilSelector == null || SigilSelector != item) ? item : null;
                            RuneSelector = null;
                            FilterBox.Text = "";
                            foreach (TemplateItem rune in Runes) { rune.Visible = true; }
                            return true;
                        }

                        if (upgrade.Hovered && isArmor)
                        {
                            RuneSelector = (RuneSelector == null || RuneSelector != item)? item : null;
                            SigilSelector = null;
                            FilterBox.Text = "";
                            foreach(TemplateItem sigil in Sigils) { sigil.Visible = true; }
                            return true;
                        }
                        index++;
                    }
                }
            }

            StatSelector = null;
            ItemSelector = null;
            SelectionItems = null;
            RuneSelector = null;
            SigilSelector = null;
            FilterBox.Hide();

            return false;
        }

        private void getArmorySelection(TemplateItem item)
        {
            ItemSelector = item;
            SelectionItems = new List<TemplateItem>();

            var isArmor = item.Slot >= _EquipmentSlots.Helmet && item.Slot <= _EquipmentSlots.Boots;
            var isWeapon = item.Slot >= _EquipmentSlots.Weapon1_MainHand && item.Slot <= _EquipmentSlots.AquaticWeapon2;
            var isJuwellery = item.Slot >= _EquipmentSlots.Back && item.Slot <= _EquipmentSlots.Accessoire2;

            if (isArmor)
            {
                foreach (GW2API.Item legyItem in BuildsManager.Data.LegendaryItems)
                {
                    var match = isArmor && legyItem.Type.RawValue == "Armor" && legyItem.Details.WeightClass.Value == (int)ArmorWeight;
                    if (match && (BuildsManager.ArmoryItems.Count == 0 || BuildsManager.ArmoryItems.Contains((int)legyItem.Id)))
                    {
                        var armor_Match = isArmor && legyItem.Details.Type.Value == (int)item.Slot.GetArmorSlot();
                        if (armor_Match)
                        {
                            SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                        }
                    }
                }
            }
            else if (isWeapon)
            {
                var profession = BuildsManager.Data.Professions.Find(e => e.Id == Build.BuildTemplate.Profession.ToString());

                if(profession != null)
                {
                    bool greatsword_Added = false;
                    foreach (GW2API.Item legyItem in BuildsManager.Data.LegendaryItems)
                    {
                        var armoryMatch = (BuildsManager.ArmoryItems.Count == 0 || BuildsManager.ArmoryItems.Contains((int)legyItem.Id));
                        var genOneGreatsword = BuildsManager.ArmoryItems.Contains(30689) ? 30689 : BuildsManager.ArmoryItems.Contains(30704) ? 30704 : BuildsManager.ArmoryItems.Contains(30703) ? 30703 : 0;

                        if (armoryMatch && legyItem.Type.RawValue == "Weapon")
                        {
                            GW2API.ProfessionWeapon weapon;
                            profession.Weapons.TryGetValue(legyItem.Details.convertWeaponType(), out weapon);

                            if (weapon != null)
                            {
                                var greatSword = legyItem.Id == 30689 || legyItem.Id == 30703 ||  legyItem.Id == 30704;
                                bool aquatic = weapon.Flags.Find(e => e.RawValue == "Aquatic") != null;
                                bool offhand = weapon.Flags.Find(e => e.RawValue == "Offhand") != null;
                                bool mainhand = weapon.Flags.Find(e => e.RawValue == "Mainhand") != null || weapon.Flags.Find(e => e.RawValue == "TwoHand") != null;

                                switch (item.Slot)
                                {
                                    case _EquipmentSlots.Weapon1_MainHand:
                                    case _EquipmentSlots.Weapon2_MainHand:
                                        if (!greatSword || legyItem.Id == genOneGreatsword)
                                        {
                                            if (!aquatic && mainhand) SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                            if (greatSword) greatsword_Added = true;
                                        }
                                        break;

                                    case _EquipmentSlots.Weapon1_OffHand:
                                    case _EquipmentSlots.Weapon2_OffHand:
                                        if (offhand) SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                        break;

                                    case _EquipmentSlots.AquaticWeapon1:
                                    case _EquipmentSlots.AquaticWeapon2:
                                        if (aquatic) SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                        break;
                                }
                            }
                        }
                    }

                    SelectionItems.Sort((a, b) => a.API_Item.Details.Type.RawValue.CompareTo(b.API_Item.Details.Type.RawValue));
                    var sorted = SelectionItems.OrderBy(x => x.API_Item.Details.Type.RawValue)
                        .ThenBy(x => x.API_Item.Id)
                        .ToList();

                    SelectionItems = sorted;
                }
            }
            else if (isJuwellery)
            {
                foreach (GW2API.Item legyItem in BuildsManager.Data.LegendaryItems)
                {
                    var match = legyItem.Type.RawValue == "Trinket";
                    var backItem = legyItem.Type.RawValue == "Back";
                    var armoryMatch = (BuildsManager.ArmoryItems.Count == 0 || BuildsManager.ArmoryItems.Contains((int)legyItem.Id));

                    if (armoryMatch && ((backItem && item.Slot == _EquipmentSlots.Back) || (match && legyItem.Details.Type != null))) 
                    {
                        switch (item.Slot)
                        {
                            case _EquipmentSlots.Amulet:
                                if(legyItem.Details.Type.RawValue == "Amulet") SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                break;

                            case _EquipmentSlots.Accessoire1:
                            case _EquipmentSlots.Accessoire2:
                                if(legyItem.Details.Type.RawValue == "Accessory") SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                break;

                            case _EquipmentSlots.Ring1:
                            case _EquipmentSlots.Ring2:
                                if(legyItem.Details.Type.RawValue == "Ring") SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                break;

                            case _EquipmentSlots.Back:
                                if(legyItem.Type.RawValue == "Back") SelectionItems.Add(new TemplateItem() { API_Item = legyItem, Slot = item.Slot, Texture = legyItem.getIcon() });
                                break;
                        }                        
                    }
                }
            }
        }

        protected void UpdateLayout(Point p)
        {
            int i;
            int offset;
            int statSize = (int)(ItemSize / 1.5);

            i = 0;
            offset = 0;
            for (int j = (int)_EquipmentSlots.Back; j <= (int)_EquipmentSlots.Accessoire2; j++)
            {
                var item = Gear.Equipment[j];
                item.Bounds = new Rectangle(offset + 15, i * 60, ItemSize, ItemSize).Add(Location);
                item.StatBounds = new Rectangle(offset + 15 + (ItemSize - statSize), i * 60 + (ItemSize - statSize), statSize, statSize).Add(Location);
                item.Hovered = item.Bounds.Contains(p);
                if (item.API_Item != null && !item.TextureLoaded) item.Texture = item.API_Item.getIcon();
                if (item.Hovered) BasicTooltipText = item.SelectedStat.Name;

                if (item.Upgrades.Count > 1)
                {
                    int jj = 0;
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + item.Bounds.Width, i * 60 + (jj * (_RuneTexture.Height / item.Upgrades.Count)), _RuneTexture.Width / item.Upgrades.Count, _RuneTexture.Height / item.Upgrades.Count).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                        jj++;
                    }
                }
                else
                {
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + _RuneTexture.Width, i * 60, _RuneTexture.Width, _RuneTexture.Height).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                    }
                }
                i++;
            }

            i = 0;
            offset = 100;
            for (int j = (int)_EquipmentSlots.Helmet; j <= (int)_EquipmentSlots.Boots; j++)
            {
                var item = Gear.Equipment[j];
                item.Bounds = new Rectangle(offset + 15, i * 60, ItemSize, ItemSize).Add(Location);
                item.StatBounds = new Rectangle(offset + 15 + (ItemSize - statSize), i * 60 + (ItemSize - statSize), statSize, statSize).Add(Location);
                item.Hovered = item.Bounds.Contains(p);
                if (item.API_Item != null && !item.TextureLoaded) item.Texture = item.API_Item.getIcon();
                if(item.Hovered) BasicTooltipText = item.SelectedStat.Name;

                if (item.Upgrades.Count > 1)
                {
                    int jj = 0;
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + item.Bounds.Width, i * 60 + (jj * (_RuneTexture.Height / item.Upgrades.Count)), _RuneTexture.Width / item.Upgrades.Count, _RuneTexture.Height / item.Upgrades.Count).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                        jj++;
                    }
                }
                else
                {
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + _RuneTexture.Width, i * 60, _RuneTexture.Width, _RuneTexture.Height).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                    }
                }
                i++;
            }

            i = 0;
            offset += 150;
            for (int j = (int)_EquipmentSlots.Weapon1_MainHand; j <= (int)_EquipmentSlots.AquaticWeapon1; j++)
            {
                var item = Gear.Equipment[j];
                if ((int)_EquipmentSlots.AquaticWeapon1 == j) i++;
                item.Bounds = new Rectangle(offset + 15, i * 60, ItemSize, ItemSize).Add(Location);
                item.StatBounds = new Rectangle(offset + 15 + (ItemSize - statSize), i * 60 + (ItemSize - statSize), statSize, statSize).Add(Location);
                item.Hovered = item.Bounds.Contains(p);
                if (item.API_Item != null && !item.TextureLoaded) item.Texture = item.API_Item.getIcon();
                if (item.Hovered) BasicTooltipText = item.SelectedStat.Name;

                if (item.Upgrades.Count > 1)
                {
                    int jj = 0;
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + item.Bounds.Width, i * 60 + (jj * (_RuneTexture.Height / item.Upgrades.Count)), _RuneTexture.Width / item.Upgrades.Count, _RuneTexture.Height / item.Upgrades.Count).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                        jj++;
                    }
                }
                else
                {
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + _RuneTexture.Width, i * 60, _RuneTexture.Width, _RuneTexture.Height).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                    }
                }
                i++;
            }

            i = 0;
            offset += 150;
            for (int j = (int)_EquipmentSlots.Weapon2_MainHand; j <= (int)_EquipmentSlots.AquaticWeapon2; j++)
            {
                var item = Gear.Equipment[j];
                if ((int)_EquipmentSlots.AquaticWeapon2 == j) i++;
                item.Bounds = new Rectangle(offset + 15, i * 60, ItemSize, ItemSize).Add(Location);
                item.StatBounds = new Rectangle(offset + 15 + (ItemSize - statSize), i * 60 + (ItemSize - statSize), statSize, statSize).Add(Location);
                item.Hovered = item.Bounds.Contains(p);
                if (item.API_Item != null && !item.TextureLoaded) item.Texture = item.API_Item.getIcon();
                if (item.Hovered) BasicTooltipText = item.SelectedStat.Name;

                if (item.Upgrades.Count > 1)
                {
                    int jj = 0;
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + item.Bounds.Width, i * 60 + (jj * (_RuneTexture.Height / item.Upgrades.Count)), _RuneTexture.Width / item.Upgrades.Count, _RuneTexture.Height / item.Upgrades.Count).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                        jj++;
                    }
                }
                else
                {
                    foreach (TemplateItem upgrade in item.Upgrades)
                    {
                        upgrade.Bounds = new Rectangle(offset + 20 + _RuneTexture.Width, i * 60, _RuneTexture.Width, _RuneTexture.Height).Add(Location);
                        upgrade.Hovered = upgrade.Bounds.Contains(p);
                        if (upgrade.API_Item != null && !upgrade.TextureLoaded) upgrade.Texture = upgrade.API_Item.getIcon();
                    }
                }
                i++;
            }

            if (ItemSelector != null)
            {
                ItemSelectorBounds = Rectangle.Empty;
                var col = 0;
                var row = 0;

                i = 0;
                foreach (TemplateItem item in SelectionItems)
                {
                    if(i == (int) Math.Ceiling(SelectionItems.Count / (double) 2))
                    {
                        row++;
                        col = 0;
                    }

                    item.Bounds = new Rectangle(ItemSelector.Bounds.Right + 5 + (col * ((ItemSelector.Bounds.Width - 10) + 3)), ItemSelector.Bounds.Top + 5 + (row * ((ItemSelector.Bounds.Height - 10) + 3)), ItemSelector.Bounds.Width - 10, ItemSelector.Bounds.Height - 10);
                    if (item.Bounds.Right > ItemSelectorBounds.Right) ItemSelectorBounds = item.Bounds;
                    item.Hovered = item.Bounds.Contains(p);
                    if (item.API_Item != null && !item.TextureLoaded) item.Texture = item.API_Item.getIcon();
                    col++;
                    i++;
                }
            }

            if (StatSelector != null)
            {
                i = 0;
                int row = 0;
                int col = 0;
                int size = _RuneTexture.Height;
                var bgColor = new Color(32, 32, 32, 255);

                foreach (Stat stat in Stats)
                {
                    stat.Bounds = new Rectangle(col * (size), row * (size), size, size).Add(new Point(StatSelector.Bounds.Right, StatSelector.Bounds.Y));
                    stat.Hovered = stat.Bounds.Contains(p);
                    if (stat.Hovered) BasicTooltipText = stat.Name;

                    if (col == 5) { row++; col = -1; }
                    col++;
                    i++;
                }
            }

            if (RuneSelector != null)
            {
                int size = _RuneTexture.Height;
                var runeList = Runes.Where(e => e.Visible);

                var desiredRows = (int)Math.Ceiling(runeList.Count() / (double)6);
                var maxRows = Math.Min(9, (int)Math.Ceiling(runeList.Count() / (double)6));
                var expander = desiredRows > maxRows ? 30 : 0;

                RuneSelectorBounds = new Rectangle(0, 0, 6 * (size + 2) + 4, FilterBox.Height + 4 + expander + maxRows * ((size + 2))).Add(new Point(RuneSelector.Bounds.Width + 7 + RuneSelector.Bounds.Right, RuneSelector.Bounds.Y));
                var filterRectangle = RuneSelectorBounds;


                int col = 0;
                int row = 0;
                foreach (TemplateItem rune in runeList)
                {
                    if (rune.Visible)
                    {
                        var rect = new Rectangle(2 + col * (size + 2), FilterBox.Height + 2 + row * (size + 2), size, size).Add(new Point(RuneSelector.Bounds.Width + 7 + RuneSelector.Bounds.Right, RuneSelector.Bounds.Y));

                        if (!filterRectangle.Contains(rect))
                        {
                            row++;
                            col = 0;
                            rect = new Rectangle(2 + col * (size + 2), FilterBox.Height + 2 + row * (size + 2), size, size).Add(new Point(RuneSelector.Bounds.Width + 7 + RuneSelector.Bounds.Right, RuneSelector.Bounds.Y));
                        }

                        rune.Bounds = rect;
                        rune.Hovered = rune.Bounds.Contains(p);
                        col++;
                    }
                }
            }

            if (SigilSelector != null)
            {
                int size = _RuneTexture.Height;
                var sigilList = Sigils.Where(e => e.Visible);

                var desiredRows = (int)Math.Ceiling(sigilList.Count() / (double)6);
                var maxRows = Math.Min(9, (int)Math.Ceiling(sigilList.Count() / (double)6));
                var expander = desiredRows > maxRows ? 30 : 0;

                SigilSelectorBounds = new Rectangle(0, 0, 6 * (size + 2) + 4, FilterBox.Height + 4 + expander + maxRows * ((size + 2))).Add(new Point(SigilSelector.Bounds.Width + 7 + SigilSelector.Bounds.Right, SigilSelector.Bounds.Y));
                var filterRectangle = SigilSelectorBounds;


                int col = 0;
                int row = 0;
                foreach (TemplateItem sigil in sigilList)
                {
                    if (sigil.Visible)
                    {
                        var rect = new Rectangle(2 + col * (size + 2), FilterBox.Height + 2 + row * (size + 2), size, size).Add(new Point(SigilSelector.Bounds.Width + 7 + SigilSelector.Bounds.Right, SigilSelector.Bounds.Y));

                        if (!filterRectangle.Contains(rect))
                        {
                            row++;
                            col = 0;
                            rect = new Rectangle(2 + col * (size + 2), FilterBox.Height + 2 + row * (size + 2), size, size).Add(new Point(SigilSelector.Bounds.Width + 7 + SigilSelector.Bounds.Right, SigilSelector.Bounds.Y));
                        }

                        sigil.Bounds = rect;
                        sigil.Hovered = sigil.Bounds.Contains(p);
                        col++;
                    }
                }
            }
        }
        public void PaintAfterChildren(SpriteBatch spriteBatch)
        {

            FilterBox.Visible = (RuneSelector != null) || (SigilSelector != null);
            if (ItemSelector != null && SelectionItems.Count > 0)
            {
                var first = SelectionItems[0].Bounds;
                var last = SelectionItems[SelectionItems.Count - 1].Bounds;

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(first.X - 5, first.Y - 5, ItemSelectorBounds.Right - first.Left + 10, last.Bottom - first.Top + 10),
                                    new Rectangle(first.X - 5, first.Y - 5, ItemSelectorBounds.Right - first.Left + 10, last.Bottom - first.Top + 10),
                                    new Color(100, 100, 100, 255),
                                    0f,
                                    default);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(first.X - 4, first.Y - 4, ItemSelectorBounds.Right - first.Left + 8, last.Bottom - first.Top + 8),
                                    new Rectangle(first.X - 4, first.Y - 4, ItemSelectorBounds.Right - first.Left + 8, last.Bottom - first.Top + 8),
                                    new Color(32, 32, 32, 255),
                                    0f,
                                    default);

                foreach (TemplateItem item in SelectionItems)
                {
                    spriteBatch.DrawOnCtrl(this,
                                        item.Texture,
                                        item.Bounds,
                                        item.Texture.Bounds,
                                        item.Hovered ? Color.White : new Color(175, 175, 175, 255),
                                        0f,
                                        default);
                }
            }

            if (StatSelector != null)
            {
                int i = 0;
                int row = 0;
                int col = 0;
                int size = _RuneTexture.Height;
                var bgColor = new Color(32, 32, 32, 255);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(0, 0, 6 * size, (Stats.Count / 5) * (size)).Add(new Point(StatSelector.Bounds.Right, StatSelector.Bounds.Y)),
                                    Rectangle.Empty,
                                    new Color(132, 132, 132, 255),
                                    0f,
                                    default);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(1, 1, 6 * size - 2, (Stats.Count/ 5) * (size) - 2).Add(new Point(StatSelector.Bounds.Right, StatSelector.Bounds.Y)),
                                    Rectangle.Empty,
                                    bgColor,
                                    0f,
                                    default);

                foreach (Stat stat in Stats)
                {
                    spriteBatch.DrawOnCtrl(this,
                                        stat.Texture,
                                        stat.Bounds,
                                        stat.Texture.Bounds,
                                        (stat.Id == StatSelector.Stat) || stat.Hovered ? Color.White : Color.DarkGray,
                                        0f,
                                        default);

                    if (col == 5) { row++; col = -1; }
                    col++;
                    i++;
                }
            }

            if (RuneSelector != null)
            {
                int size = _RuneTexture.Height;
                var runeList = Runes.Where(e => e.Visible);

                var desiredRows = (int)Math.Ceiling(runeList.Count() / (double)6);
                var maxRows = Math.Min(9, (int)Math.Ceiling(runeList.Count() / (double)6));
                var expander = desiredRows > maxRows ? 30 : 0;

                var bgColor = new Color(32, 32, 32, 255);
                var filterRectangle = RuneSelectorBounds;

                FilterBox.Width = filterRectangle.Width - 2;
                FilterBox.Location = new Point(filterRectangle.X  + 1, filterRectangle.Y + 1).Add(new Point(AbsoluteBounds.X, AbsoluteBounds.Y));

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    filterRectangle,
                                    Rectangle.Empty,
                                    new Color(132, 132, 132, 255),
                                    0f,
                                    default);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    filterRectangle.Add(new Rectangle(1, 1, -2, -2)),
                                    Rectangle.Empty,
                                    bgColor,
                                    0f,
                                    default);

                int col = 0;
                int row = 0;
                var cnt = new ContentService();
                var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize) 32, ContentService.FontStyle.Regular);

                foreach (TemplateItem rune in runeList)
                {
                    if (!filterRectangle.Contains(rune.Bounds))
                    {
                        row++;
                        col = 0;
                    }
                    if (row <= 8)
                    {
                        if (filterRectangle.Contains(rune.Bounds))
                        {
                            spriteBatch.DrawOnCtrl(this,
                                                rune.Texture,
                                                rune.Bounds,
                                                rune.Texture.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                        }
                    }
                    else
                    {
                        var rect = new Rectangle(0, FilterBox.Height + 2 + row * (size + 2), filterRectangle.Width, 15).Add(new Point(RuneSelector.Bounds.Width + 7 + RuneSelector.Bounds.Right, RuneSelector.Bounds.Y));
                        spriteBatch.DrawStringOnCtrl(Parent,
                                               ". . .",
                                               font,
                                               rect,
                                               Color.White,
                                               default,
                                               HorizontalAlignment.Center
                                               );
                        break;
                    }

                    col++;
                }
            }

            if (SigilSelector != null)
            {
                int size = _RuneTexture.Height;
                var sigilList = Sigils.Where(e => e.Visible);

                var desiredRows = (int)Math.Ceiling(sigilList.Count() / (double)6);
                var maxRows = Math.Min(9, (int)Math.Ceiling(sigilList.Count() / (double)6));
                var expander = desiredRows > maxRows ? 30 : 0;

                var bgColor = new Color(32, 32, 32, 255);
                var filterRectangle = SigilSelectorBounds;

                FilterBox.Width = filterRectangle.Width - 2;
                FilterBox.Location = new Point(filterRectangle.X  + 1, filterRectangle.Y + 1).Add(new Point(AbsoluteBounds.X, AbsoluteBounds.Y));

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    filterRectangle,
                                    Rectangle.Empty,
                                    new Color(132, 132, 132, 255),
                                    0f,
                                    default);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    filterRectangle.Add(new Rectangle(1, 1, -2, -2)),
                                    Rectangle.Empty,
                                    bgColor,
                                    0f,
                                    default);

                int col = 0;
                int row = 0;
                var cnt = new ContentService();
                var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize) 32, ContentService.FontStyle.Regular);

                foreach (TemplateItem sigil in sigilList)
                {
                    if (!filterRectangle.Contains(sigil.Bounds))
                    {
                        row++;
                        col = 0;
                    }
                    if (row <= 8)
                    {
                        if (filterRectangle.Contains(sigil.Bounds))
                        {
                            spriteBatch.DrawOnCtrl(this,
                                                sigil.Texture,
                                                sigil.Bounds,
                                                sigil.Texture.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                        }
                    }
                    else
                    {
                        var rect = new Rectangle(0, FilterBox.Height + 2 + row * (size + 2), filterRectangle.Width, 15).Add(new Point(SigilSelector.Bounds.Width + 7 + SigilSelector.Bounds.Right, SigilSelector.Bounds.Y));
                        spriteBatch.DrawStringOnCtrl(Parent,
                                               ". . .",
                                               font,
                                               rect,
                                               Color.White,
                                               default,
                                               HorizontalAlignment.Center
                                               );
                        break;
                    }

                    col++;
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            UpdateLayout(RelativeMousePosition);


            foreach (TemplateItem item in Gear.Equipment)
            {
                spriteBatch.DrawOnCtrl(this,
                                        item.Texture,
                                        item.Bounds,
                                        item.Texture.Bounds,
                                        item.API_Item != null ? new Color(75,75,75,255): Color.White,
                                        0f,
                                        default);

                if (item.SelectedStat != null && item.API_Item != null)
                {
                    spriteBatch.DrawOnCtrl(this,
                                            item.SelectedStat.Texture,
                                            item.StatBounds,
                                            item.SelectedStat.Texture.Bounds,
                                            Color.White,
                                            0f,
                                            default, SpriteEffects.None);
                }

                if(item.Upgrades.Count > 0)
                {
                    foreach(TemplateItem upgrade in item.Upgrades)
                    {
                        spriteBatch.DrawOnCtrl(this,
                                                upgrade.Texture,
                                                upgrade.Bounds,
                                                upgrade.Texture.Bounds,
                                                upgrade.Hovered ? Color.White : Color.LightGray,
                                                0f,
                                                default);
                    }
                }

                if(item.Stats.Count > 0)
                {
                    int i = 0;
                    var max = Math.Min(item.Stats.Count, 4);

                    foreach (int stat in item.Stats)
                    {
                        var texture = BuildsManager.DataManager._Stats[stat];

                        spriteBatch.DrawOnCtrl(this,
                                                texture,
                                                new Rectangle(item.Bounds.Left - 8 - (texture.Bounds.Width / Math.Min(item.Stats.Count, 4)), item.Bounds.Y + (i * (item.Bounds.Width / Math.Min(item.Stats.Count, 4) )), item.Bounds.Width / Math.Min(item.Stats.Count, 4), item.Bounds.Height / Math.Min(item.Stats.Count, 4)),
                                                texture.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                        i++;

                        if (i == max) break;
                    }
                }
            }

            if (ItemSelector != null && SelectionItems.Count > 0)
            {
                var first = SelectionItems[0].Bounds;
                var last = SelectionItems[SelectionItems.Count - 1].Bounds;

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(first.X - 5, first.Y - 5, last.Right - first.Left + 10, last.Bottom - first.Top + 10),
                                    new Rectangle(first.X - 5, first.Y - 5, last.Right - first.Left + 10, last.Bottom - first.Top + 10),
                                    new Color(100,100,100,255),
                                    0f,
                                    default);

                spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    new Rectangle(first.X - 4 , first.Y - 4, last.Right - first.Left + 8, last.Bottom - first.Top + 8),
                                    new Rectangle(first.X - 4, first.Y - 4, last.Right - first.Left + 8, last.Bottom - first.Top + 8),
                                    new Color(32,32,32,255),
                                    0f,
                                    default);

                foreach (TemplateItem item in SelectionItems)
                {
                    spriteBatch.DrawOnCtrl(this,
                                        item.Texture,
                                        item.Bounds,
                                        item.Texture.Bounds,
                                        item.Hovered ? Color.White : new Color(175, 175, 175, 255),
                                        0f,
                                        default);
                }
            }

            //PaintAfterChildren(spriteBatch);
        }
    }
}
