using System.Collections.Generic;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class AquaticWeapon_TemplateItem : TemplateItem
    {
        public API.weaponType WeaponType = API.weaponType.Unkown;
        public List<API.SigilItem> Sigils = new()
        {
            new API.SigilItem(),
            new API.SigilItem(),
        };

        public List<Rectangle> SigilsBounds = new()
        {
            Rectangle.Empty,
            Rectangle.Empty,
        };
    }
}
