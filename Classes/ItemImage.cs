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
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework;

namespace Kenedia.Modules.BuildsManager
{
    public class ItemImage : Panel {
        public string BasicTooltipText 
        {
            set { Image.BasicTooltipText = value; }
        }
        private Texture2D _Texture;
        public Texture2D Texture 
        {
            get { return _Texture; }
            set
            {
                _Texture = value;
                Image.Texture = Texture;
            }
        }
        Image Image;
        public int FrameWidth = 3;

        public ItemImage()
        {
            Image = new Image()
            {
                Parent = this,
                Location = new Point(FrameWidth, FrameWidth),
                Size = new Point(Width - (FrameWidth * 2), Height - (FrameWidth * 2))
            };

            BackgroundColor = FrameColor;
            Resized += delegate
            {
                Image.Location = new Point(FrameWidth, FrameWidth);
                Image.Size = new Point(Width - (FrameWidth * 2), Height - (FrameWidth * 2));
            };
        }

        public ItemRarity __Rarity;
        public ItemRarity _Rarity 
        {
            get { return __Rarity; }
            set { __Rarity = value; BackgroundColor = FrameColor; }
        }
        public Color FrameColor
        {
            get
            {
                switch (_Rarity)
                {
                    case ItemRarity.Unknown: return new Color(0, 0, 0, 225);
                    case ItemRarity.Junk: return new Color(170, 170, 170, 225);
                    case ItemRarity.Basic: return new Color(240, 240, 240, 225);
                    case ItemRarity.Fine: return new Color(98, 164, 218, 225);
                    case ItemRarity.Masterwork: return new Color(26, 147, 6, 225);
                    case ItemRarity.Rare: return new Color(252, 208, 11, 225);
                    case ItemRarity.Exotic: return new Color(255, 164, 5, 225);
                    case ItemRarity.Ascended: return new Color(251, 62, 141, 225);
                    case ItemRarity.Legendary: return new Color(76, 19, 157, 225);
                }
                
                return new Color(0,0,0,0);
            }
        }       
    }
}
