using System;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.Enums;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class GearTemplate : IDisposable
    {
        private readonly bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                Trinkets = null;

                // Trinkets?.DisposeAll();
                Armor = null;

                // Armor?.DisposeAll();
                Weapons = null;

                // Weapons?.DisposeAll();
                AquaticWeapons = null;

                // AquaticWeapons?.DisposeAll();
            }
        }

        public List<TemplateItem> Trinkets = new()
        {
            new TemplateItem() { _Slot = "Back"},
            new TemplateItem() { _Slot = "Amulet"},
            new TemplateItem() { _Slot = "Ring1"},
            new TemplateItem() { _Slot = "Ring2"},
            new TemplateItem() { _Slot = "Accessoire1"},
            new TemplateItem() { _Slot = "Accessoire2"},
        };

        public List<Armor_TemplateItem> Armor = new()
        {
            new Armor_TemplateItem() { _Slot = "Helmet"},
            new Armor_TemplateItem() { _Slot = "Shoulders"},
            new Armor_TemplateItem() { _Slot = "Chest"},
            new Armor_TemplateItem() { _Slot = "Gloves"},
            new Armor_TemplateItem() { _Slot = "Leggings"},
            new Armor_TemplateItem() { _Slot = "Boots"},
        };

        public List<Weapon_TemplateItem> Weapons = new() {
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon1_MainHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon1_OffHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon2_MainHand},
        new Weapon_TemplateItem() { Slot = EquipmentSlots.Weapon2_OffHand},
        };

        public List<AquaticWeapon_TemplateItem> AquaticWeapons = new()
        {
            new AquaticWeapon_TemplateItem() { Slot = EquipmentSlots.AquaticWeapon1},
            new AquaticWeapon_TemplateItem() { Slot = EquipmentSlots.AquaticWeapon2},
        };

        public GearTemplate(string code = default)
        {
            if (code != default)
            {
                int j = 0;
                string[] ItemStrings = code.Split(']');
                if (ItemStrings.Length == 19)
                {
                    j = 0;
                    for (int i = 0; i < Trinkets.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        int id;
                        int.TryParse(ItemStrings[i], out id);
                        if (id > 0)
                        {
                            Trinkets[j].Stat = BuildsManager.s_moduleInstance.Data.Stats.Find(e => e.Id == id);
                        }

                        BuildsManager.Logger.Debug("Trinkets[" + j + "].Stat: " + Trinkets[j].Stat?.Name);
                        j++;
                    }

                    j = 0;
                    for (int i = Trinkets.Count; i < Trinkets.Count + Armor.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        string[] ids = ItemStrings[i].Split('|');

                        int stat_id;
                        int.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            Armor[j].Stat = BuildsManager.s_moduleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Stat: " + Armor[j].Stat?.Name);

                        int rune_id;
                        int.TryParse(ids[1], out rune_id);
                        if (stat_id > 0)
                        {
                            Armor[j].Rune = BuildsManager.s_moduleInstance.Data.Runes.Find(e => e.Id == rune_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Rune: " + Armor[j].Rune?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = Trinkets.Count + Armor.Count; i < (Trinkets.Count + Armor.Count + Weapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        string[] ids = ItemStrings[i].Split('|');

                        int stat_id;
                        int.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            Weapons[j].Stat = BuildsManager.s_moduleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Stat: " + Weapons[j].Stat?.Name);

                        int weaponType = -1;
                        int.TryParse(ids[1], out weaponType);
                        Weapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("Weapons[" + j + "].WeaponType: " + Weapons[j].WeaponType.ToString());

                        int sigil_id;
                        int.TryParse(ids[2], out sigil_id);
                        if (stat_id > 0)
                        {
                            Weapons[j].Sigil = BuildsManager.s_moduleInstance.Data.Sigils.Find(e => e.Id == sigil_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Sigil: " + Weapons[j].Sigil?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = Trinkets.Count + Armor.Count + Weapons.Count; i < (Trinkets.Count + Armor.Count + Weapons.Count + AquaticWeapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        string[] ids = ItemStrings[i].Split('|');

                        int stat_id;
                        int.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            AquaticWeapons[j].Stat = BuildsManager.s_moduleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Stat: " + AquaticWeapons[j].Stat?.Name);

                        int weaponType = -1;
                        int.TryParse(ids[1], out weaponType);
                        AquaticWeapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].WeaponType: " + AquaticWeapons[j].WeaponType.ToString());

                        int sigil1_id;
                        int.TryParse(ids[2], out sigil1_id);
                        if (sigil1_id > 0)
                        {
                            AquaticWeapons[j].Sigils[0] = BuildsManager.s_moduleInstance.Data.Sigils.Find(e => e.Id == sigil1_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + AquaticWeapons[j].Sigils[0]?.Name);

                        int sigil2_id;
                        int.TryParse(ids[3], out sigil2_id);
                        if (sigil2_id > 0)
                        {
                            AquaticWeapons[j].Sigils[1] = BuildsManager.s_moduleInstance.Data.Sigils.Find(e => e.Id == sigil2_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + AquaticWeapons[j].Sigils[1]?.Name);

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

                foreach (TemplateItem item in Trinkets)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "]";
                }

                foreach (Armor_TemplateItem item in Armor)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + (item.Rune != null ? item.Rune.Id : 0) + "]";
                }

                foreach (Weapon_TemplateItem item in Weapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigil != null ? item.Sigil.Id : 0) + "]";
                }

                foreach (AquaticWeapon_TemplateItem item in AquaticWeapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigils[0] != null ? item.Sigils[0].Id : 0) + "|" + (item.Sigils[1] != null ? item.Sigils[1].Id : 0) + "]";
                }

                return code;
            }
        }
    }
}
