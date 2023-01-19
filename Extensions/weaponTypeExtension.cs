namespace Kenedia.Modules.BuildsManager.Extensions
{
    internal static class weaponTypeExtension
    {
        public static string getLocalName(this API.weaponType weaponType)
        {
            string text = weaponType.ToString();

            return weaponType switch
            {
                API.weaponType.Axe => Strings.common.Axe,
                API.weaponType.Dagger => Strings.common.Dagger,
                API.weaponType.Mace => Strings.common.Mace,
                API.weaponType.Pistol => Strings.common.Pistol,
                API.weaponType.Scepter => Strings.common.Scepter,
                API.weaponType.Sword => Strings.common.Sword,
                API.weaponType.Focus => Strings.common.Focus,
                API.weaponType.Shield => Strings.common.Shield,
                API.weaponType.Torch => Strings.common.Torch,
                API.weaponType.Warhorn => Strings.common.Warhorn,
                API.weaponType.Greatsword => Strings.common.Greatsword,
                API.weaponType.Hammer => Strings.common.Hammer,
                API.weaponType.Longbow => Strings.common.Longbow,// case API.weaponType.LongBow:
                API.weaponType.Rifle => Strings.common.Rifle,
                API.weaponType.Shortbow => Strings.common.Shortbow,// case API.weaponType.ShortBow:
                API.weaponType.Staff => Strings.common.Staff,
                API.weaponType.Spear => Strings.common.Spear,// case API.weaponType.Harpoon:
                API.weaponType.Speargun => Strings.common.Speargun,
                API.weaponType.Trident => Strings.common.Trident,
                _ => text,
            };
        }
    }
}
