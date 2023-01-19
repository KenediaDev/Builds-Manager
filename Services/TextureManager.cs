using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager
{
    public class UpgradeIDs : IDisposable
    {
        public void Dispose()
        {
            Sigils.Clear();
            Runes.Clear();
        }

        public UpgradeIDs(List<int> runes, List<int> sigils)
        {
            Sigils = new List<int>(sigils);
            Runes = new List<int>(runes);
        }

        public List<int> Sigils { get; private set; }

        public List<int> Runes { get; private set; }
    }

    public class TextureManager : IDisposable
    {
        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                _Backgrounds?.DisposeAll();
                _Icons?.DisposeAll();
                _Emblems?.DisposeAll();
                _Controls?.DisposeAll();
                _EquipmentTextures?.DisposeAll();
                _Stats?.DisposeAll();
                _StatIcons?.DisposeAll();
                _EquipSlotTextures?.DisposeAll();
            }
        }

        public UpgradeIDs _UpgradeIDs;
        public List<Texture2D> _Backgrounds = new();
        public List<Texture2D> _Icons = new();
        public List<Texture2D> _Emblems = new();
        public List<Texture2D> _Controls = new();
        public List<Texture2D> _EquipmentTextures = new();
        public List<Texture2D> _Stats = new();
        public List<Texture2D> _StatIcons = new();

        public List<Texture2D> _EquipSlotTextures = new();

        public TextureManager()
        {
            Blish_HUD.Modules.Managers.ContentsManager ContentsManager = BuildsManager.s_moduleInstance.ContentsManager;

            // var DirectoriesManager = BuildsManager.ModuleInstance.DirectoriesManager;

            Array values = Enum.GetValues(typeof(Backgrounds));
            _Backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Backgrounds num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                _Backgrounds.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Icons));
            _Icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Icons num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                _Icons.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Emblems));
            _Emblems = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Emblems num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\emblems\" + (int)num + ".png");
                _Emblems.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(ControlTexture));
            _Controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (ControlTexture num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                _Controls.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(EquipmentTextures));
            _EquipmentTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (EquipmentTextures num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png");
                _EquipmentTextures.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(EquipSlotTextures));
            _EquipSlotTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (EquipSlotTextures num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png").GetRegion(37, 37, 54, 54);
                _EquipSlotTextures.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(Stats));
            _Stats = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (Stats num in values)
            {
                Texture2D texture = ContentsManager.GetTexture(@"textures\stats\" + (int)num + ".png");
                _Stats.Insert((int)num, texture);
            }

            BuildsManager.s_moduleInstance.LoadingTexture = getIcon(Icons.SingleSpinner);
        }

        public Texture2D getBackground(Backgrounds background)
        {
            int index = (int)background;

            if (index < _Backgrounds.Count && _Backgrounds[index] != null)
            {
                return _Backgrounds[index];
            }

            return _Icons[0];
        }

        public Texture2D getIcon(Icons icon)
        {
            int index = (int)icon;

            if (index < _Icons.Count && _Icons[index] != null)
            {
                return _Icons[index];
            }

            return _Icons[0];
        }

        public Texture2D getEmblem(Emblems emblem)
        {
            int index = (int)emblem;
            if (index < _Emblems.Count && _Emblems[index] != null)
            {
                return _Emblems[index];
            }

            return _Icons[0];
        }

        public Texture2D getEquipTexture(EquipmentTextures equipment)
        {
            int index = (int)equipment;
            if (index < _EquipmentTextures.Count && _EquipmentTextures[index] != null)
            {
                return _EquipmentTextures[index];
            }

            return _Icons[0];
        }

        public Texture2D getStatTexture(Stats stat)
        {
            int index = (int)stat;
            if (index < _Stats.Count && _Stats[index] != null)
            {
                return _Stats[index];
            }

            return _Icons[0];
        }

        public Texture2D getControlTexture(ControlTexture control)
        {
            int index = (int)control;
            if (index < _Controls.Count && _Controls[index] != null)
            {
                return _Controls[index];
            }

            return _Icons[0];
        }
    }
}
