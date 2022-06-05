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
        Tooltip,
        Build,
    }

    public enum _Emblems
    {
        SwordAndShield,
        QuestionMark,
    }
    public enum _EquipSlotTextures
    {
        Helmet,
        Shoulders,
        Chest,
        Gloves,
        Leggings,
        Boots,
        Weapon1_MainHand = 6,
        Weapon1_OffHand = 7,
        AquaticWeapon1 = 9,
        Weapon2_MainHand = 6,
        Weapon2_OffHand = 7,
        AquaticWeapon2 = 9,
        Back,
        Amulet,
        Ring1,
        Ring2,
        Accessoire1,
        Accessoire2,
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
        Concentration = 2,
        ConditionDamage,
        ConditionDuration = 4,
        Expertise = 4,
        CritDamage = 5,
        Ferocity = 5,
        Healing,
        HealingPower = 6,
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
        Cavaliers = 583,
        Berserkers = 584,
        Rabid = 585,
        Soldiers = 586,
        Celestial = 588,
        Clerics = 656,
        Knights = 657,
        Rampagers = 658,
        Apothecarys = 659,
        Captains = 660,
        Settlers = 690,
        Sentinels = 1035,
        Magis = 1037,
        Carrion = 1038,
        Assassins = 1128,
        Nomads = 1066,
        Sinister = 1064,
        Shamans = 1097,
        Crusader = 1098,
        Dire = 1114,
        Trailblazers = 1115,
        Valkyrie = 1119,
        Commanders = 1125,
        Vipers = 1130,
        Minstrels = 1134,
        Vigilant = 1139,
        Marauder = 1145,
        Wanderers = 1162,
        Zealots = 1163,
        Seraph = 1220,
        Grieving = 1329,
        Marshals = 1337,
        Harriers = 1345,
        Givers = 1430,
        Bringers = 1436,
        Plaguedoctors = 1486,
        Diviners = 1538,
        Dragons = 1697,
        Ritualists = 1694,
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
        Shoulders = 8,
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
        AddButton,
        ResetButton,
        ResetButton_Hovered,
        Template_Border,
        Delete,
        Delete_Hovered,
        Clear,
        Add,
        Add_Hovered,
        Copy,
        Copy_Hovered,
        Import,
        Import_Hovered,
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
        Mouse,
        Lock_Locked,
        SingleSpinner,
    }
    public enum _RevenantLegends
    {
        //      Legend1 = 1,
        LegendaryDragonStance = 1,

        //    Legend2 = 2,
        LegendaryAssassinStance = 2,

        //    Legend3 = 3,
        LegendaryDwarfStance = 3,

        //    Legend4 = 4,
        LegendaryDemonStance = 4,

        //     Legend5 = 5,
        LegendaryRenegadeStance = 5,

        // Legend6 = 6,
        LegendaryCentaurStance = 6,

        // Legend7 = 7,
        LegendaryAllianceStance = 7,
    }
}