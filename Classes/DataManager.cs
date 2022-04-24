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
    public class SkillID
    {
        public int PaletteID;
        public int ID;
    }
    public class iTexture
    {
        public Texture2D Texture;
        public string FileName;

    }
    public class _UpgradeIDs
    {
        public _UpgradeIDs(List<int> runes, List<int> sigils)
        {
            _Sigils = new List<int>(sigils);
            _Runes = new List<int>(runes);
        }
        public List<int> _Sigils { get; private set; }
        public List<int> _Runes { get; private set; }
    }

    public class iDataManager
    {
        public _UpgradeIDs _UpgradeIDs;
        public List<Texture2D> _Backgrounds = new List<Texture2D>();
        public List<Texture2D> _Icons = new List<Texture2D>();
        public List<Texture2D> _Emblems = new List<Texture2D>();
        public List<Texture2D> _Controls = new List<Texture2D>();
        public List<Texture2D> _EquipmentTextures = new List<Texture2D>();
        public List<Texture2D> _Stats = new List<Texture2D>();
        public List<Texture2D> _StatIcons = new List<Texture2D>();

        public Blish_HUD.Modules.Managers.ContentsManager ContentsManager;
        public Blish_HUD.Modules.Managers.DirectoriesManager DirectoriesManager;

        public iDataManager(Blish_HUD.Modules.Managers.ContentsManager contentsManager, Blish_HUD.Modules.Managers.DirectoriesManager  directoriesManager)
        {
            ContentsManager = contentsManager;
            DirectoriesManager = directoriesManager;

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
            
            values = Enum.GetValues(typeof(_Stats));
            _Stats = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_Stats num in values)
            {
                var texture = ContentsManager.GetTexture(@"textures\stats\" + (int)num + ".png");
                _Stats.Insert((int)num, texture);
            }            

            values = Enum.GetValues(typeof(_StatIcons));
            _StatIcons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
            foreach (_StatIcons num in values) 
            { 
                var value = (int) num;
                var texture = ContentsManager.GetTexture(@"textures\stat icons\" + value + ".png");
                _StatIcons.Insert(value, texture);
            }

            string runesJson = new StreamReader(ContentsManager.GetFileStream(@"data\runes.json")).ReadToEnd();
            List<int> runes = JsonConvert.DeserializeObject<List<int>>(runesJson);

            string sigilsJson = new StreamReader(ContentsManager.GetFileStream(@"data\sigils.json")).ReadToEnd();
            List<int> sigils = JsonConvert.DeserializeObject<List<int>>(sigilsJson);

            _UpgradeIDs = new _UpgradeIDs(runes, sigils);
        }

        public Texture2D getBackground(_Backgrounds background)
        {
            var index = (int)background;

            if (index < _Backgrounds.Count && _Backgrounds[index] != null) return _Backgrounds[index];
            return _Icons[0];
        }

        public int getStat_ID(string statName)
        {
            return (int) Enum.Parse(typeof(_EquipmentStats), statName);
        }

        public Texture2D getStat_Texture(_EquipmentStats stat)
        {
            var index = (int) Enum.Parse(typeof(_StatIcons), stat.ToString());

            if (index < _StatIcons.Count && _StatIcons[index] != null) return _StatIcons[index];
            return _Icons[0];
        }

        public int getStat_ID(_EquipmentStats stat)
        {
            return (int)stat;
        }

        public GW2API.Stats getStat_Info(_EquipmentStats stat)
        {
            var id = (int)stat;
            return BuildsManager.Data.Stats.Find(e => e.Id == id);
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
