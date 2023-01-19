namespace Kenedia.Modules.BuildsManager.Controls
{
    using System;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class ProfessionSelection : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Profession = null;
            }
        }

        public API.Profession Profession;
        public Rectangle Bounds;
        public bool Hovered;
        public int Index;
    }
}
