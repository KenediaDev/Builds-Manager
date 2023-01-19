using System.Collections.Generic;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class GearTemplate_json
    {
        public List<TemplateItem_json> Trinkets = new()
        {
            new TemplateItem_json() { _Slot = "Back"},
            new TemplateItem_json() { _Slot = "Amulet"},
            new TemplateItem_json() { _Slot = "Ring1"},
            new TemplateItem_json() { _Slot = "Ring2"},
            new TemplateItem_json() { _Slot = "Accessoire1"},
            new TemplateItem_json() { _Slot = "Accessoire2"},
        };

        public List<Armor_TemplateItem_json> Armor = new()
        {
            new Armor_TemplateItem_json() { _Slot = "Helmet"},
            new Armor_TemplateItem_json() { _Slot = "Shoulders"},
            new Armor_TemplateItem_json() { _Slot = "Chest"},
            new Armor_TemplateItem_json() { _Slot = "Gloves"},
            new Armor_TemplateItem_json() { _Slot = "Leggings"},
            new Armor_TemplateItem_json() { _Slot = "Boots"},
        };

        public List<Weapon_TemplateItem_json> Weapons = new()
        {
            new Weapon_TemplateItem_json() {_Slot = "Weapon1_MainHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon1_OffHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon2_MainHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon2_OffHand"},
        };

        public List<AquaticWeapon_TemplateItem_json> AquaticWeapons = new()
        {
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon1"},
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon2"},
        };
    }
}
