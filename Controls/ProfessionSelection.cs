using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class ProfessionSelection : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Profession = null;
            }
        }

        public API.Profession Profession;
        public Rectangle Bounds;
        public bool Hovered;
        public int Index;
    }
}
