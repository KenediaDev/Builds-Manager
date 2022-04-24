namespace Kenedia.Modules.BuildsManager
{
    public static class EnumConversion
    {
        public static _EquipmentSlots GetEquipmentSlot(this _ArmorSlot slot)
        {
            switch (slot)
            {
                case _ArmorSlot.Helm:
                    return _EquipmentSlots.Helmet;
                case _ArmorSlot.Shoulders:
                    return _EquipmentSlots.Shoulders;
                case _ArmorSlot.Coat:
                    return _EquipmentSlots.Chest;
                case _ArmorSlot.Gloves:
                    return _EquipmentSlots.Gloves;
                case _ArmorSlot.Leggings:
                    return _EquipmentSlots.Leggings;
                case _ArmorSlot.Boots:
                    return _EquipmentSlots.Boots;
            }

            return _EquipmentSlots.Unkown;
        }

        public static _ArmorSlot GetArmorSlot(this _EquipmentSlots slot)
        {
            switch (slot)
            {
                case _EquipmentSlots.Helmet:
                    return _ArmorSlot.Helm;
                case _EquipmentSlots.Shoulders:
                    return _ArmorSlot.Shoulders;
                case _EquipmentSlots.Chest:
                    return _ArmorSlot.Coat;
                case _EquipmentSlots.Gloves:
                    return _ArmorSlot.Gloves;
                case _EquipmentSlots.Leggings:
                    return _ArmorSlot.Leggings;
                case _EquipmentSlots.Boots:
                    return _ArmorSlot.Boots;
            }

            return _ArmorSlot.Unkown;
        }

        public static _ArmorWeight GetArmorWeight(this Gw2Sharp.Models.ProfessionType profession)
        {
            switch (profession)
            {
                case Gw2Sharp.Models.ProfessionType.Elementalist:
                case Gw2Sharp.Models.ProfessionType.Necromancer:
                case Gw2Sharp.Models.ProfessionType.Mesmer:
                    return _ArmorWeight.Light;

                case Gw2Sharp.Models.ProfessionType.Ranger:
                case Gw2Sharp.Models.ProfessionType.Thief:
                case Gw2Sharp.Models.ProfessionType.Engineer:
                    return _ArmorWeight.Medium;

                case Gw2Sharp.Models.ProfessionType.Warrior:
                case Gw2Sharp.Models.ProfessionType.Guardian:
                case Gw2Sharp.Models.ProfessionType.Revenant:
                    return _ArmorWeight.Heavy;
            }

            return _ArmorWeight.Unkown;
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

    public enum _Backgrounds
    {
        MainWindow,
        BlueishMainWindow,
    }

    public enum _Emblems
    {
        SwordAndShield,
    }
    public enum _EquipmentTextures
    {
        Helmet,
        Shoulders,
        Chest,
        Gloves,
        Leggings,
        Boots,
        Mainhand_Weapon,
        Offhand_Weapon,
        //Aquabreather,
        AquaticWeapon = 9,
        Back,
        Amulet,
        Ring1,
        Ring2,
        Accessoire1,
        Accessoire2,
        Rune,
        Sickle,
        Axe,
        Pickaxe,
    }
    public enum _Stats
    {
        BoonDuration = 2,
        ConditionDamage,
        ConditionDuration,
        CritDamage,
        Healing,
        Power,
        Precision, 
        Toughness,
        Vitality,
        CritChance,
        Health,
        MagicFind,
    }
    public enum _EquipmentStats
    {
        Cavaliers = 616,
        Berserkers = 161,
        Rabid = 154,
        Soldiers = 162,
        Celestial = 559,
        Clerics = 155,
        Knights = 158,
        Rampagers = 159,
        Apothecarys = 605,
        Captains = 660,
        Settlers = 700,
        Sentinels = 686,
        Magis = 156,
        Carrion = 160,
        Assassins = 753,
        Nomads = 1026,
        Sinister = 1067,
        Shamans = 153,
        Crusader = 1109,
        Dire = 754,
        Trailblazers = 1085,
        Valkyrie = 157,
        Commanders = 1131,
        Vipers = 1153,
        Minstrels = 1123,
        Vigilant = 1118,
        Marauder = 1111,
        Wanderers = 1140,
        Zealots = 799,
        Seraph = 1222,
        Grieving = 1344,
        Marshals = 1364,
        Harriers = 1363,
        Givers = 628,
        Bringers = 1032,
        Plaguedoctors = 1559,
        Diviners = 1556,
        Dragons = 1681,
        Ritualists = 1686,
    }
    public enum _StatIcons
    {
        Berserkers,
        Soldiers,
        Valkyrie,
        Captains,
        Rampagers,
        Knights,
        Cavaliers,
        Givers,
        Shamans,
        Carrion,
        Rabid,
        //Snowflake, //not assigned
        Clerics = 12,
        Magis,
        Apothecarys,
        Trailblazers,
        Wanderers,
        Minstrels,
        Celestial,
        //BerserkersAndValkyrie,
        //DireAndRabids,
        //RabidAndApothecarys,
        Vipers = 22,
        Sentinels,
        Settlers,
        Assassins,
        Dire,
        Zealots,
        Nomads,
        Sinister,
        Vigilant,
        Marauder,
        Crusader,
        Commanders,
        Seraph,
        Marshals,
        Grieving,
        //Skull, //not assigned
        //Footprint, //not assigned
        Harriers = 39,
        Bringers,
        Plaguedoctors,
        Diviners,
        Ritualists,
        Dragons,
    }
    public enum _EquipmentSlots
    {
        Unkown = -1,
        Helmet,
        Shoulders,
        Chest,
        Gloves,
        Leggings,
        Boots,
        Weapon1_MainHand,
        Weapon1_OffHand,
        AquaticWeapon1,
        Weapon2_MainHand,
        Weapon2_OffHand,
        AquaticWeapon2,
     //   Aquabreather,
        Back,
        Amulet,
        Ring1,
        Ring2,
        Accessoire1,
        Accessoire2,
    }
    public enum _ArmorSlot
    {
        Unkown = -1,
        Helm = 6,
        Shoulders  = 8,
        Coat = 3,
        Gloves = 5,
        Leggings = 7,
        Boots = 4,
    }
    public enum _ArmorWeight
    {
        Unkown = -1,
        Heavy = 1,
        Medium,
        Light,
    }

    public enum _Controls
    {
        GlidingBar,
        GlidingFill,
        GlidingFill_Gray,
        TabActive,
        SpecHighlight,
        Line,
        Land,
        Water,
        EliteFrame,
        PlaceHolder_Traitline,
        SpecFrame,
        SpecSideSelector,
        SpecSideSelector_Hovered,
        SkillSelector,
        SkillSelector_Hovered,
        NoWaterTexture,
        TabBorderLeft,
        TabBorderRight,
        TabBar_FadeIn,
        TabBar_Line,
        Selector,
    }
    public enum _Icons
    {
        Bug,
        Refresh,
        Template,
        Template_White,
        Helmet,
        Helmet_White,
        Cog,
        Cog_White,
        Undo,
        Undo_White,
        Checkmark_White,
        Checkmark_Color,
        Checkmark_Highlight,
        Stop_White,
        Stop_Color,
        Stop_Highlight,
        Search,
        Search_Highlight,
        Edit_Feather,
        Edit_Feather_Highlight,
        Edit_Feather_Pressed,
    }
}
