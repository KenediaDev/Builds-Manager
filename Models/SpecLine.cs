namespace Kenedia.Modules.BuildsManager.Models
{
    using System;
    using System.Collections.Generic;
    using Kenedia.Modules.BuildsManager.Controls;
    using Kenedia.Modules.BuildsManager.Extensions;

    public class SpecLine : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this._Specialization?.Dispose();

                // Specialization?.Dispose();
                this.Traits?.DisposeAll();
                this.Control?.Dispose();
            }
        }

        public int Index;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => this._Specialization;
            set
            {
                this._Specialization = value;
                this.Traits = new List<API.Trait>();
                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<API.Trait> Traits = new List<API.Trait>();
        public EventHandler Changed;
        public Specialization_Control Control;
    }
}
