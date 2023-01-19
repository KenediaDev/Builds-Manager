using Kenedia.Modules.BuildsManager.Enums;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class EnumExtension
    {
        public static EquipmentSlots GetEquipmentSlot(this ArmorSlot slot)
        {
            switch (slot)
            {
                case ArmorSlot.Helm:
                    return EquipmentSlots.Helmet;
                case ArmorSlot.Shoulders:
                    return EquipmentSlots.Shoulders;
                case ArmorSlot.Coat:
                    return EquipmentSlots.Chest;
                case ArmorSlot.Gloves:
                    return EquipmentSlots.Gloves;
                case ArmorSlot.Leggings:
                    return EquipmentSlots.Leggings;
                case ArmorSlot.Boots:
                    return EquipmentSlots.Boots;
            }

            return EquipmentSlots.Unkown;
        }

        public static ArmorSlot GetArmorSlot(this EquipmentSlots slot)
        {
            switch (slot)
            {
                case EquipmentSlots.Helmet:
                    return ArmorSlot.Helm;
                case EquipmentSlots.Shoulders:
                    return ArmorSlot.Shoulders;
                case EquipmentSlots.Chest:
                    return ArmorSlot.Coat;
                case EquipmentSlots.Gloves:
                    return ArmorSlot.Gloves;
                case EquipmentSlots.Leggings:
                    return ArmorSlot.Leggings;
                case EquipmentSlots.Boots:
                    return ArmorSlot.Boots;
            }

            return ArmorSlot.Unkown;
        }

        public static ArmorWeight GetArmorWeight(this Gw2Sharp.Models.ProfessionType profession)
        {
            switch (profession)
            {
                case Gw2Sharp.Models.ProfessionType.Elementalist:
                case Gw2Sharp.Models.ProfessionType.Necromancer:
                case Gw2Sharp.Models.ProfessionType.Mesmer:
                    return ArmorWeight.Light;

                case Gw2Sharp.Models.ProfessionType.Ranger:
                case Gw2Sharp.Models.ProfessionType.Thief:
                case Gw2Sharp.Models.ProfessionType.Engineer:
                    return ArmorWeight.Medium;

                case Gw2Sharp.Models.ProfessionType.Warrior:
                case Gw2Sharp.Models.ProfessionType.Guardian:
                case Gw2Sharp.Models.ProfessionType.Revenant:
                    return ArmorWeight.Heavy;
            }

            return ArmorWeight.Unkown;
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