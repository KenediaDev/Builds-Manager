using Kenedia.Modules.BuildsManager.Enums;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class EnumExtension
    {
        public static EquipmentSlots GetEquipmentSlot(this ArmorSlot slot)
        {
            return slot switch
            {
                ArmorSlot.Helm => EquipmentSlots.Helmet,
                ArmorSlot.Shoulders => EquipmentSlots.Shoulders,
                ArmorSlot.Coat => EquipmentSlots.Chest,
                ArmorSlot.Gloves => EquipmentSlots.Gloves,
                ArmorSlot.Leggings => EquipmentSlots.Leggings,
                ArmorSlot.Boots => EquipmentSlots.Boots,
                _ => EquipmentSlots.Unkown,
            };
        }

        public static ArmorSlot GetArmorSlot(this EquipmentSlots slot)
        {
            return slot switch
            {
                EquipmentSlots.Helmet => ArmorSlot.Helm,
                EquipmentSlots.Shoulders => ArmorSlot.Shoulders,
                EquipmentSlots.Chest => ArmorSlot.Coat,
                EquipmentSlots.Gloves => ArmorSlot.Gloves,
                EquipmentSlots.Leggings => ArmorSlot.Leggings,
                EquipmentSlots.Boots => ArmorSlot.Boots,
                _ => ArmorSlot.Unkown,
            };
        }

        public static ArmorWeight GetArmorWeight(this Gw2Sharp.Models.ProfessionType profession)
        {
            return profession switch
            {
                Gw2Sharp.Models.ProfessionType.Elementalist or Gw2Sharp.Models.ProfessionType.Necromancer or Gw2Sharp.Models.ProfessionType.Mesmer => ArmorWeight.Light,
                Gw2Sharp.Models.ProfessionType.Ranger or Gw2Sharp.Models.ProfessionType.Thief or Gw2Sharp.Models.ProfessionType.Engineer => ArmorWeight.Medium,
                Gw2Sharp.Models.ProfessionType.Warrior or Gw2Sharp.Models.ProfessionType.Guardian or Gw2Sharp.Models.ProfessionType.Revenant => ArmorWeight.Heavy,
                _ => ArmorWeight.Unkown,
            };
        }

        public static string convertWeaponType(this GW2API.intDetails details)
        {
            if (details.Type != null)
            {
                switch (details.Type.RawValue)
                {
                    case "Harpoon":
                        return "Spear";

                    case "Spear":
                        return "Harpoon";

                    case "LongBow":
                        return "Longbow";

                    case "Longbow":
                        return "LongBow";

                    case "ShortBow":
                        return "Shortbow";

                    case "Shortbow":
                        return "ShortBow";
                }
            }

            return details.Type != null ? details.Type.RawValue : "Unkown";
        }
    }
}