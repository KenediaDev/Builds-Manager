using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Blish_HUD;
using Blish_HUD.Controls;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.BuildsManager
{
    public class APIDownload_Image
    {
        public API_Image Parent;
        public string display_text = "";
        public string url;
        public string path;
    }

    public class API_Image
    {
        public List<Control> connectedControls = new List<Control>();
        public string Url;
        public bool fileChecked;
        public bool fileFetched;
        public bool fileLoaded;

        public string folderPath;

        private string _iconPath;
        private string _fileName;
        public string fileName
        {
            get
            {
                if (_fileName != null) return _fileName;
                _fileName = Regex.Match(Url, "[0-9]*.png").ToString();

                return _fileName;
            }
        }

        public string iconPath
        {
            get
            {
                return folderPath + @"/" + fileName;
            }
        }

        public Texture2D _Texture;
        public Texture2D Texture
        {
            get
            {
                if (_Texture != null) return _Texture;

                if (!fileChecked && folderPath != null)
                {
                    if (System.IO.File.Exists(iconPath))
                    {
                        if(!BuildsManager.load_Images.Contains(this)) BuildsManager.load_Images.Add(this);

                        return BuildsManager.DataManager._Icons[1];
                    }
                    else if (!fileFetched)
                    {
                        if (fetchImage()) return BuildsManager.DataManager._Icons[1];
                    }

                    fileChecked = fileFetched && true;
                }

                return BuildsManager.DataManager._Icons[0];
            }
        }
        bool fetchImage()
        {
            if (folderPath != null)
            {
                BuildsManager.download_Images.Add(new APIDownload_Image()
                {
                    Parent = this,
                    url = Url,
                    path = iconPath,
                });
                return true;
            }

            return false;
        }
    }

    public class GW2API
    {        
        public class LegendaryItem
        {
            public class iType
            {
                public bool IsUnknown;
                public int Value;
                public string RawValue;
            }

            public class iDetails
            {
                public List<int> StatChoices;
                public iType Type;
            }
            public class iIcon
            {
                public string Url;
            }
            public class iRarity
            {
                public bool isSet;
                public bool IsUnknown;
                public int Value;
                public string RawValue;
            }

            public string Name;
            public iIcon Icon;
            public string ChatLink;
            public int Id;
            public iDetails Details;
            public iType Type;
            public iRarity Rarity;
            public ItemRarity __Rarity;
            public ItemRarity _Rarity
            {
                get
                {
                    if (Rarity.isSet) return __Rarity;

                    if (Rarity != null) 
                    {
                        switch (Rarity.Value)
                        {
                            case 0: __Rarity = ItemRarity.Unknown; break; 
                            case 1: __Rarity = ItemRarity.Junk; break; 
                            case 2: __Rarity = ItemRarity.Basic; break; 
                            case 3: __Rarity = ItemRarity.Fine; break; 
                            case 4: __Rarity = ItemRarity.Masterwork; break; 
                            case 5: __Rarity = ItemRarity.Rare; break; 
                            case 6: __Rarity = ItemRarity.Exotic; break; 
                            case 7: __Rarity = ItemRarity.Ascended; break; 
                            case 8: __Rarity = ItemRarity.Legendary; break; 
                        }
                    }

                    Rarity.isSet = true;
                    return __Rarity;
                }
            }

            public API_Image api_Image;

            public Texture2D getTexture(Control control = null)
            {
                if (api_Image._Texture != null) return Texture;
                if (control != null) api_Image.connectedControls.Add(control);

                return Texture;
            }

            public Texture2D Texture
            {
                get { return api_Image != null ? api_Image.Texture : BuildsManager.DataManager._Icons[0]; }
            }
        }
    }
}
