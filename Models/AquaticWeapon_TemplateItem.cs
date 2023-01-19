namespace Kenedia.Modules.BuildsManager.Models
{
    using System.Collections.Generic;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class AquaticWeapon_TemplateItem : TemplateItem
    {
        public API.weaponType WeaponType = API.weaponType.Unkown;
        public List<API.SigilItem> Sigils = new List<API.SigilItem>()
        {
            new API.SigilItem(),
            new API.SigilItem(),
        };

        public List<Rectangle> SigilsBounds = new List<Rectangle>()
        {
            Rectangle.Empty,
            Rectangle.Empty,
        };
    }
}
