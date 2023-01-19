namespace Kenedia.Modules.BuildsManager.Models
{
    using System;
    using System.Collections.Generic;
    using Kenedia.Modules.BuildsManager.Enums;

    public class GearTemplate : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Trinkets = null;

                // Trinkets?.DisposeAll();
                this.Armor = null;

                // Armor?.DisposeAll();
                this.Weapons = null;

                // Weapons?.DisposeAll();
                this.AquaticWeapons = null;

                // AquaticWeapons?.DisposeAll();
            }
        }

        public List<TemplateItem> Trinkets = new List<TemplateItem>()
        {
            new TemplateItem() { _Slot = "Back"},
            new TemplateItem() { _Slot = "Amulet"},
            new TemplateItem() { _Slot = "Ring1"},
            new TemplateItem() { _Slot = "Ring2"},
            new TemplateItem() { _Slot = "Accessoire1"},
            new TemplateItem() { _Slot = "Accessoire2"},
        };

        public List<Armor_TemplateItem> Armor = new List<Armor_TemplateItem>()
        {
            new Armor_TemplateItem() { _Slot = "Helmet"},
            new Armor_TemplateItem() { _Slot = "Shoulders"},
            new Armor_TemplateItem() { _Slot = "Chest"},
            new Armor_TemplateItem() { _Slot = "Gloves"},
            new Armor_TemplateItem() { _Slot = "Leggings"},
            new Armor_TemplateItem() { _Slot = "Boots"},
        };

        public List<Weapon_TemplateItem> Weapons = new List<Weapon_TemplateItem>() {
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon1_MainHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon1_OffHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon2_MainHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon2_OffHand},
        };

        public List<AquaticWeapon_TemplateItem> AquaticWeapons = new List<AquaticWeapon_TemplateItem>()
        {
            new AquaticWeapon_TemplateItem() { Slot = EquipmentSlots.AquaticWeapon1},
            new AquaticWeapon_TemplateItem() { Slot = EquipmentSlots.AquaticWeapon2},
        };

        public GearTemplate(string code = default)
        {
            if (code != default)
            {
                int j = 0;
                var ItemStrings = code.Split(']');
                if (ItemStrings.Length == 19)
                {
                    j = 0;
                    for (int i = 0; i < this.Trinkets.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        int id;
                        Int32.TryParse(ItemStrings[i], out id);
                        if (id > 0)
                        {
                            this.Trinkets[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == id);
                        }

                        BuildsManager.Logger.Debug("Trinkets[" + j + "].Stat: " + this.Trinkets[j].Stat?.Name);
                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count; i < this.Trinkets.Count + this.Armor.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.Armor[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Stat: " + this.Armor[j].Stat?.Name);

                        int rune_id;
                        Int32.TryParse(ids[1], out rune_id);
                        if (stat_id > 0)
                        {
                            this.Armor[j].Rune = BuildsManager.ModuleInstance.Data.Runes.Find(e => e.Id == rune_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Rune: " + this.Armor[j].Rune?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count + this.Armor.Count; i < (this.Trinkets.Count + this.Armor.Count + this.Weapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.Weapons[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Stat: " + this.Weapons[j].Stat?.Name);

                        int weaponType = -1;
                        Int32.TryParse(ids[1], out weaponType);
                        this.Weapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("Weapons[" + j + "].WeaponType: " + this.Weapons[j].WeaponType.ToString());

                        int sigil_id;
                        Int32.TryParse(ids[2], out sigil_id);
                        if (stat_id > 0)
                        {
                            this.Weapons[j].Sigil = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Sigil: " + this.Weapons[j].Sigil?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count + this.Armor.Count + this.Weapons.Count; i < (this.Trinkets.Count + this.Armor.Count + this.Weapons.Count + this.AquaticWeapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.AquaticWeapons[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Stat: " + this.AquaticWeapons[j].Stat?.Name);

                        int weaponType = -1;
                        Int32.TryParse(ids[1], out weaponType);
                        this.AquaticWeapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].WeaponType: " + this.AquaticWeapons[j].WeaponType.ToString());

                        int sigil1_id;
                        Int32.TryParse(ids[2], out sigil1_id);
                        if (sigil1_id > 0)
                        {
                            this.AquaticWeapons[j].Sigils[0] = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil1_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + this.AquaticWeapons[j].Sigils[0]?.Name);

                        int sigil2_id;
                        Int32.TryParse(ids[3], out sigil2_id);
                        if (sigil2_id > 0)
                        {
                            this.AquaticWeapons[j].Sigils[1] = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil2_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + this.AquaticWeapons[j].Sigils[1]?.Name);

                        j++;
                    }
                }
            }
        }

        public string TemplateCode
        {
            get
            {
                string code = string.Empty;

                foreach (TemplateItem item in this.Trinkets)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "]";
                }

                foreach (Armor_TemplateItem item in this.Armor)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + (item.Rune != null ? item.Rune.Id : 0) + "]";
                }

                foreach (Weapon_TemplateItem item in this.Weapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigil != null ? item.Sigil.Id : 0) + "]";
                }

                foreach (AquaticWeapon_TemplateItem item in this.AquaticWeapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigils[0] != null ? item.Sigils[0].Id : 0) + "|" + (item.Sigils[1] != null ? item.Sigils[1].Id : 0) + "]";
                }

                return code;
            }
        }
    }
}
