namespace Kenedia.Modules.BuildsManager.Extensions
{
    internal static class weaponTypeExtension
    {
        public static string getLocalName(this API.weaponType weaponType)
        {
            var text = weaponType.ToString();

            switch (weaponType)
            {
                case API.weaponType.Axe:
                    return Strings.common.Axe;
                case API.weaponType.Dagger:
                    return Strings.common.Dagger;
                case API.weaponType.Mace:
                    return Strings.common.Mace;
                case API.weaponType.Pistol:
                    return Strings.common.Pistol;
                case API.weaponType.Scepter:
                    return Strings.common.Scepter;
                case API.weaponType.Sword:
                    return Strings.common.Sword;
                case API.weaponType.Focus:
                    return Strings.common.Focus;
                case API.weaponType.Shield:
                    return Strings.common.Shield;
                case API.weaponType.Torch:
                    return Strings.common.Torch;
                case API.weaponType.Warhorn:
                    return Strings.common.Warhorn;
                case API.weaponType.Greatsword:
                    return Strings.common.Greatsword;
                case API.weaponType.Hammer:
                    return Strings.common.Hammer;
                case API.weaponType.Longbow:
                    // case API.weaponType.LongBow:
                    return Strings.common.Longbow;
                case API.weaponType.Rifle:
                    return Strings.common.Rifle;
                case API.weaponType.Shortbow:
                    // case API.weaponType.ShortBow:
                    return Strings.common.Shortbow;
                case API.weaponType.Staff:
                    return Strings.common.Staff;
                case API.weaponType.Spear:
                    // case API.weaponType.Harpoon:
                    return Strings.common.Spear;
                case API.weaponType.Speargun:
                    return Strings.common.Speargun;
                case API.weaponType.Trident:
                    return Strings.common.Trident;
            }

            return text;
        }
    }
}
