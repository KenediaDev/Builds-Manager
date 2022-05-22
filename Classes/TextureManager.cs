using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Gw2Sharp.ChatLinks;

namespace Kenedia.Modules.BuildsManager
{
    public class _UpgradeIDs : IDisposable
    {
        void IDisposable.Dispose() { }
        private bool disposed = false;
        public void Dispose()
        {
            _Sigils.Clear();
            _Runes.Clear();
        }

        public _UpgradeIDs(List<int> runes, List<int> sigils)
        {
            _Sigils = new List<int>(sigils);
            _Runes = new List<int>(runes);
        }
        public List<int> _Sigils { get; private set; }
        public List<int> _Runes { get; private set; }
    }

    public class TextureManager : IDisposable
    {        
        void IDisposable.Dispose() { }
        private bool disposed = false;
        public void Dispose()
        {
            _Backgrounds.Clear();
            _Icons.Clear();
            _Emblems.Clear();
            _Controls.Clear();
            _EquipmentTextures.Clear();
            _Stats.Clear();
            _StatIcons.Clear();
            _EquipSlotTextures.Clear();
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
            //var DirectoriesManager = BuildsManager.ModuleInstance.DirectoriesManager;

            var values = Enum.GetValues(typeof(_Backgrounds));
            _Backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Backgrounds num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                _Backgrounds.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(_Icons));
            _Icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Icons num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                _Icons.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(_Emblems));
            _Emblems = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Emblems num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\emblems\" + (int)num + ".png");
                _Emblems.Insert((int)num, texture);
            }

            values = Enum.GetValues(typeof(_Controls));
            _Controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Controls num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                _Controls.Insert((int)num, texture);
            }
            
            values = Enum.GetValues(typeof(_EquipmentTextures));
            _EquipmentTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_EquipmentTextures num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png");
                _EquipmentTextures.Insert((int)num, texture);
            }
            values = Enum.GetValues(typeof(_EquipSlotTextures));
            _EquipSlotTextures = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_EquipSlotTextures num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\equipment slots\" + (int)num + ".png").GetRegion(37, 37, 54, 54);
                _EquipSlotTextures.Insert((int)num, texture);
            }
            
            values = Enum.GetValues(typeof(_Stats));
            _Stats = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Stats num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\stats\" + (int)num + ".png");
                _Stats.Insert((int)num, texture);
            }

            BuildsManager.ModuleInstance.LoadingTexture = getIcon(Modules.BuildsManager._Icons.SingleSpinner);
        }

        public Texture2D getBackground(_Backgrounds background)
        {
            var index = (int)background;

            if (index < _Backgrounds.Count && _Backgrounds[index] != null) return _Backgrounds[index];
            return _Icons[0];
        }

        public Texture2D getIcon (_Icons icon)
        {
            var index = (int)icon;

            if (index < _Icons.Count && _Icons[index] != null) return _Icons[index];
            return _Icons[0];
        }

        public Texture2D getEmblem (_Emblems emblem)
        {
            var index = (int)emblem;
            if (index < _Emblems.Count && _Emblems[index] != null) return _Emblems[index];
            return _Icons[0];
        }

        public Texture2D getEquipTexture (_EquipmentTextures equipment)
        {
            var index = (int)equipment;
            if (index < _EquipmentTextures.Count && _EquipmentTextures[index] != null) return _EquipmentTextures[index];
            return _Icons[0];
        }

        public Texture2D getStatTexture (_Stats stat)
        {
            var index = (int)stat;
            if (index < _Stats.Count && _Stats[index] != null) return _Stats[index];
            return _Icons[0];
        }

        public Texture2D getControlTexture (_Controls control)
        {
            var index = (int)control;
            if (index < _Controls.Count && _Controls[index] != null) return _Controls[index];
            return _Icons[0];
        }
    }
}
