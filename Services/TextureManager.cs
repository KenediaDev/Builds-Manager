namespace Kenedia.Modules.BuildsManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Kenedia.Modules.BuildsManager.Enums;
    using Kenedia.Modules.BuildsManager.Extensions;
    using Microsoft.Xna.Framework.Graphics;

    public class _UpgradeIDs : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            this._Sigils.Clear();
            this._Runes.Clear();
        }

        public _UpgradeIDs(List<int> runes, List<int> sigils)
        {
            this._Sigils = new List<int>(sigils);
            this._Runes = new List<int>(runes);
        }

        public List<int> _Sigils { get; private set; }

        public List<int> _Runes { get; private set; }
    }

    public class TextureManager : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                this._Backgrounds?.DisposeAll();
                this._Icons?.DisposeAll();
                this._Emblems?.DisposeAll();
                this._Controls?.DisposeAll();
                this._EquipmentTextures?.DisposeAll();
                this._Stats?.DisposeAll();
                this._StatIcons?.DisposeAll();
                this._EquipSlotTextures?.DisposeAll();
            }
        }

        public _UpgradeIDs _UpgradeIDs;
        public List<Texture2D> _Backgrounds = new List<Texture2D>();
        public List<Texture2D> _Icons = new List<Texture2D>();
        public List<Texture2D> _Emblems = new List<Texture2D>();
        public List<Texture2D> _Controls = new List<Texture2D>();
        public List<Texture2D> _EquipmentTextures = new List<Texture2D>();
        public List<Texture2D> _Stats = new List<Texture2D>();
        public List<Texture2D> _StatIcons = new List<Texture2D>();

        public List<Texture2D> _EquipSlotTextures = new List<Texture2D>();

        public TextureManager()
        {
            var ContentsManager = BuildsManager.ModuleInstance.ContentsManager;

            // var DirectoriesManager = BuildsManager.ModuleInstance.DirectoriesManager;

            var values = Enum.GetValues(typeof(Backgrounds));
            this._Backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Backgrounds num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                this._Backgrounds.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Icons));
            this._Icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Icons num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                this._Icons.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Emblems));
            this._Emblems = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Emblems num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\emblems\" + (int)num + ".png");
                this._Emblems.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(ControlTexture));
            this._Controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (ControlTexture num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                this._Controls.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(EquipmentTextures));
            this._EquipmentTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (EquipmentTextures num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png");
                this._EquipmentTextures.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(EquipSlotTextures));
            this._EquipSlotTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (EquipSlotTextures num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png").GetRegion(37, 37, 54, 54);
                this._EquipSlotTextures.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Stats));
            this._Stats = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Stats num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\stats\" + (int)num + ".png");
                this._Stats.Insert((int)num, texture);
            }

            BuildsManager.ModuleInstance.LoadingTexture = this.getIcon(Icons.SingleSpinner);
        }

        public Texture2D getBackground(Backgrounds background)
        {
            var index = (int)background;

            if (index < this._Backgrounds.Count && this._Backgrounds[index] != null)
            {
                return this._Backgrounds[index];
            }

            return this._Icons[0];
        }

        public Texture2D getIcon(Icons icon)
        {
            var index = (int)icon;

            if (index < this._Icons.Count && this._Icons[index] != null)
            {
                return this._Icons[index];
            }

            return this._Icons[0];
        }

        public Texture2D getEmblem(Emblems emblem)
        {
            var index = (int)emblem;
            if (index < this._Emblems.Count && this._Emblems[index] != null)
            {
                return this._Emblems[index];
            }

            return this._Icons[0];
        }

        public Texture2D getEquipTexture(EquipmentTextures equipment)
        {
            var index = (int)equipment;
            if (index < this._EquipmentTextures.Count && this._EquipmentTextures[index] != null)
            {
                return this._EquipmentTextures[index];
            }

            return this._Icons[0];
        }

        public Texture2D getStatTexture(Stats stat)
        {
            var index = (int)stat;
            if (index < this._Stats.Count && this._Stats[index] != null)
            {
                return this._Stats[index];
            }

            return this._Icons[0];
        }

        public Texture2D getControlTexture(ControlTexture control)
        {
            var index = (int)control;
            if (index < this._Controls.Count && this._Controls[index] != null)
            {
                return this._Controls[index];
            }

            return this._Icons[0];
        }
    }
}
