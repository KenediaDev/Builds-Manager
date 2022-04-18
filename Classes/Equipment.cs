using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Graphics;
using Gw2Sharp.ChatLinks;

namespace Kenedia.Modules.BuildsManager
{
    public class TemplateItem
    {
        public GW2API.Item API_Item;
        public _EquipmentSlots Slot;
        public _EquipmentStats Stat;
        public Texture2D Texture;
    }
    public class GearTemplate
    {
        public List<TemplateItem> Equipment = new List<TemplateItem>(new TemplateItem[19]);
    }

    public class Equipment : Control
    {
        public double Scale;
        public GearTemplate Gear;
        private Texture2D _RuneTexture;

        public Equipment(GearTemplate gear)
        {
            _RuneTexture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            Gear = gear;
            Gear.Equipment = new List<TemplateItem>();

            for (int j = 0; j < 19; j++)
            {
                Gear.Equipment.Add(new TemplateItem());
            }

           #region Armor
            Gear.Equipment[(int)_EquipmentSlots.Aquabreather].Slot = _EquipmentSlots.Aquabreather;
            Gear.Equipment[(int)_EquipmentSlots.Aquabreather].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Aquabreather).GetRegion(37,37,54,54);
            Gear.Equipment[(int)_EquipmentSlots.Aquabreather].Stat = _EquipmentStats.Berserkers;

            Gear.Equipment[(int)_EquipmentSlots.Helmet].Slot = _EquipmentSlots.Helmet;
            Gear.Equipment[(int)_EquipmentSlots.Helmet].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Helmet).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Helmet].Stat = _EquipmentStats.Berserkers;

            Gear.Equipment[(int)_EquipmentSlots.Shoulders].Slot = _EquipmentSlots.Shoulders;
            Gear.Equipment[(int)_EquipmentSlots.Shoulders].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Shoulders).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Shoulders].Stat = _EquipmentStats.Berserkers;

            Gear.Equipment[(int)_EquipmentSlots.Chest].Slot = _EquipmentSlots.Chest;
            Gear.Equipment[(int)_EquipmentSlots.Chest].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Chest).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Chest].Stat = _EquipmentStats.Berserkers;            

            Gear.Equipment[(int)_EquipmentSlots.Gloves].Slot = _EquipmentSlots.Gloves;
            Gear.Equipment[(int)_EquipmentSlots.Gloves].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Gloves).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Gloves].Stat = _EquipmentStats.Berserkers;            

            Gear.Equipment[(int)_EquipmentSlots.Leggings].Slot = _EquipmentSlots.Leggings;
            Gear.Equipment[(int)_EquipmentSlots.Leggings].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Leggings).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Leggings].Stat = _EquipmentStats.Berserkers;            

            Gear.Equipment[(int)_EquipmentSlots.Boots].Slot = _EquipmentSlots.Boots;
            Gear.Equipment[(int)_EquipmentSlots.Boots].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Boots).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Boots].Stat = _EquipmentStats.Berserkers;
            #endregion

            #region Weapons
            Gear.Equipment[(int)_EquipmentSlots.Weapon1_MainHand].Slot = _EquipmentSlots.Weapon1_MainHand;
            Gear.Equipment[(int)_EquipmentSlots.Weapon1_MainHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Mainhand_Weapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Weapon1_MainHand].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].Slot = _EquipmentSlots.Weapon1_OffHand;
            Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Offhand_Weapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Weapon1_OffHand].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.Weapon2_MainHand].Slot = _EquipmentSlots.Weapon2_MainHand;
            Gear.Equipment[(int)_EquipmentSlots.Weapon2_MainHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Mainhand_Weapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Weapon2_MainHand].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].Slot = _EquipmentSlots.Weapon2_OffHand;
            Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Offhand_Weapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Weapon2_OffHand].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon1].Slot = _EquipmentSlots.AquaticWeapon1;
            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon1].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.AquaticWeapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon1].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon2].Slot = _EquipmentSlots.AquaticWeapon2;
            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon2].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.AquaticWeapon).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.AquaticWeapon2].Stat = _EquipmentStats.Berserkers;
            #endregion

            #region Juwellery
            Gear.Equipment[(int)_EquipmentSlots.Back].Slot = _EquipmentSlots.Back;
            Gear.Equipment[(int)_EquipmentSlots.Back].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Back).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Back].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.Amulet].Slot = _EquipmentSlots.Amulet;
            Gear.Equipment[(int)_EquipmentSlots.Amulet].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Amulet).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Amulet].Stat = _EquipmentStats.Berserkers;

            Gear.Equipment[(int)_EquipmentSlots.Ring1].Slot = _EquipmentSlots.Ring1;
            Gear.Equipment[(int)_EquipmentSlots.Ring1].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Ring1).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Ring1].Stat = _EquipmentStats.Berserkers;


            Gear.Equipment[(int)_EquipmentSlots.Ring2].Slot = _EquipmentSlots.Ring2;
            Gear.Equipment[(int)_EquipmentSlots.Ring2].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Ring2).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Ring2].Stat = _EquipmentStats.Berserkers;

            Gear.Equipment[(int)_EquipmentSlots.Accessoire1].Slot = _EquipmentSlots.Chest;
            Gear.Equipment[(int)_EquipmentSlots.Accessoire1].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Accessoire1).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Accessoire1].Stat = _EquipmentStats.Berserkers;
            

            Gear.Equipment[(int)_EquipmentSlots.Accessoire2].Slot = _EquipmentSlots.Accessoire2;
            Gear.Equipment[(int)_EquipmentSlots.Accessoire2].Texture = BuildsManager.DataManager.getEquipTexture(_EquipmentTextures.Accessoire2).GetRegion(37, 37, 54, 54);
            Gear.Equipment[(int)_EquipmentSlots.Accessoire2].Stat = _EquipmentStats.Berserkers;
            

            #endregion
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var i = 0;
            var offset = 0;

            i = 0;
            for (int j = (int)_EquipmentSlots.Helmet; j <= (int) _EquipmentSlots.Boots; j++)
            {
                var item = Gear.Equipment[j];

                spriteBatch.DrawOnCtrl(this,
                                       item.Texture,
                                       new Rectangle(offset, i* 60, item.Texture.Width, item.Texture.Height).Add(Location),
                                      item.Texture.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                spriteBatch.DrawOnCtrl(this,
                                       _RuneTexture,
                                       new Rectangle(offset + _RuneTexture.Width + 10, i* 60, _RuneTexture.Width, _RuneTexture.Height).Add(Location),
                                      _RuneTexture.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                i++;
            }

            i = 0;
            offset += 200;
            for (int j = (int)_EquipmentSlots.Back; j <= (int) _EquipmentSlots.Accessoire2; j++)
            {
                var item = Gear.Equipment[j];

                spriteBatch.DrawOnCtrl(this,
                                       item.Texture,
                                       new Rectangle(offset, i* 60, item.Texture.Width, item.Texture.Height).Add(Location),
                                      item.Texture.Bounds,
                                      Color.White,
                                      0f,
                                      default);


                i++;
            }

            i = 0;
            offset += 100;
            for (int j = (int)_EquipmentSlots.Weapon1_MainHand; j <= (int) _EquipmentSlots.AquaticWeapon1; j++)
            {
                var item = Gear.Equipment[j];

                spriteBatch.DrawOnCtrl(this,
                                       item.Texture,
                                       new Rectangle(offset, i* 60, item.Texture.Width, item.Texture.Height).Add(Location),
                                      item.Texture.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                if (j == (int)_EquipmentSlots.Weapon1_OffHand || j == (int)_EquipmentSlots.Weapon2_OffHand) i++;
                i++;
            }

            i = 0;
            offset += 100;
            for (int j = (int)_EquipmentSlots.Weapon2_MainHand; j <= (int) _EquipmentSlots.AquaticWeapon2; j++)
            {
                var item = Gear.Equipment[j];

                spriteBatch.DrawOnCtrl(this,
                                       item.Texture,
                                       new Rectangle(offset, i* 60, item.Texture.Width, item.Texture.Height).Add(Location),
                                      item.Texture.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                if (j == (int)_EquipmentSlots.Weapon1_OffHand || j == (int)_EquipmentSlots.Weapon2_OffHand) i++;

                i++;
            }
        }
    }
}
