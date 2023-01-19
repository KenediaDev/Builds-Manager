namespace Kenedia.Modules.BuildsManager.Models
{
    using System;
    using Kenedia.Modules.BuildsManager.Enums;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class TemplateItem : TemplateItem_json, IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Stat?.Dispose();
                this.Stat = null;
            }
        }

        public EquipmentSlots Slot = EquipmentSlots.Unkown;
        public API.Stat Stat;

        public Rectangle Bounds;
        public Rectangle StatBounds;
        public Rectangle UpgradeBounds;
        public bool Hovered;
    }
}
