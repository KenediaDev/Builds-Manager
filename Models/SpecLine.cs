using System;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Extensions;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class SpecLine : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                _Specialization?.Dispose();

                // Specialization?.Dispose();
                Traits?.DisposeAll();
                Control?.Dispose();
            }
        }

        public int Index;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => _Specialization;
            set
            {
                _Specialization = value;
                Traits = new List<API.Trait>();
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<API.Trait> Traits = new();
        public EventHandler Changed;
        public Specialization_Control Control;
    }
}
