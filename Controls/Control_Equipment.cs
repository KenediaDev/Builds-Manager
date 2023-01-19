using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class Control_Equipment : Control
    {
        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        public double Scale;
        private readonly Texture2D _RuneTexture;
        private readonly List<API.TrinketItem> Trinkets = new();
        private List<API.ArmorItem> Armors = new();
        private readonly List<API.WeaponItem> Weapons = new();
        private readonly List<Texture2D> WeaponSlots = new();
        private readonly List<Texture2D> AquaticWeaponSlots = new();

        private readonly List<SelectionPopUp.SelectionEntry> Stats_Selection = new();
        private readonly List<SelectionPopUp.SelectionEntry> Sigils_Selection = new();
        private readonly List<SelectionPopUp.SelectionEntry> Runes_Selection = new();
        private readonly List<SelectionPopUp.SelectionEntry> Weapons_Selection = new();

        private List<string> Instructions = new();

        private string _Profession;
        public CustomTooltip CustomTooltip;
        public SelectionPopUp SelectionPopUp;

        public Control_Equipment(Container parent)
        {
            Parent = parent;

            // BackgroundColor = Color.Aqua;
            _RuneTexture = BuildsManager.s_moduleInstance.TextureManager.getEquipTexture(EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            Trinkets = new List<API.TrinketItem>();
            foreach (API.TrinketItem item in BuildsManager.s_moduleInstance.Data.Trinkets)
            {
                Trinkets.Add(item);
            }

            WeaponSlots = new List<Texture2D>()
            {
                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon1_MainHand],
                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon1_OffHand],

                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon2_MainHand],
                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon2_OffHand],
            };

            AquaticWeaponSlots = new List<Texture2D>()
            {
                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.AquaticWeapon1],
                BuildsManager.s_moduleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.AquaticWeapon2],
            };

            Weapons = new List<API.WeaponItem>() { };
            foreach (API.WeaponItem weapon in BuildsManager.s_moduleInstance.Data.Weapons) { Weapons.Add(weapon); }

            Click += OnClick;
            RightMouseButtonPressed += OnRightClick;
            Input.Mouse.LeftMouseButtonPressed += OnGlobalClick;

            CustomTooltip = new CustomTooltip(Parent)
            {
                ClipsBounds = false,
            };
            SelectionPopUp = new SelectionPopUp(GameService.Graphics.SpriteScreen)
            {
                CustomTooltip = CustomTooltip,

                // Template = Template,
            };
            SelectionPopUp.Changed += SelectionPopUp_Changed;

            foreach (API.RuneItem item in BuildsManager.s_moduleInstance.Data.Runes)
            {
                Runes_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = item.Bonuses,
                });
            }

            foreach (API.SigilItem item in BuildsManager.s_moduleInstance.Data.Sigils)
            {
                Sigils_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = new List<string>() { item.Description },
                });
            }

            foreach (API.WeaponItem item in Weapons)
            {
                Weapons_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.WeaponType.getLocalName(),
                    Content = new List<string>() { string.Empty },
                });
            }

            foreach (API.Stat item in BuildsManager.s_moduleInstance.Data.Stats)
            {
                Stats_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = item.Attributes.Select(e => "+ " + e.Name).ToList(),
                    ContentTextures = item.Attributes.Select(e => e.Icon._AsyncTexture).ToList(),
                });
            }

            Instructions = Strings.common.GearTab_Tips.Split('\n').ToList();
            BuildsManager.s_moduleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            ProfessionChanged();
            UpdateLayout();

            BuildsManager.s_moduleInstance.Selected_Template_Changed += ModuleInstance_Selected_Template_Changed;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            CustomTooltip?.Dispose();
            SelectionPopUp?.Dispose();

            Click -= OnClick;
            RightMouseButtonPressed -= OnRightClick;
            Input.Mouse.LeftMouseButtonPressed -= OnGlobalClick;
            SelectionPopUp.Changed -= SelectionPopUp_Changed;

            Runes_Selection.DisposeAll();
            Sigils_Selection.DisposeAll();
            Weapons.DisposeAll();
            Weapons_Selection.DisposeAll();
            Stats_Selection.DisposeAll();

            BuildsManager.s_moduleInstance.LanguageChanged -= ModuleInstance_LanguageChanged;
            BuildsManager.s_moduleInstance.Selected_Template_Changed -= ModuleInstance_Selected_Template_Changed;
        }

        protected override void OnShown(EventArgs e)
        {
            UpdateLayout();
        }

        private void SelectionPopUp_Changed(object sender, EventArgs e)
        {
            OnChanged();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            Instructions = Strings.common.GearTab_Tips.Split('\n').ToList();

            foreach (SelectionPopUp.SelectionEntry entry in Weapons_Selection)
            {
                API.WeaponItem item = (API.WeaponItem)entry.Object;
                entry.Header = item.WeaponType.getLocalName();
            }

            foreach (SelectionPopUp.SelectionEntry entry in Stats_Selection)
            {
                API.Stat stat = (API.Stat)entry.Object;
                entry.Header = stat.Name;
            }

            foreach (SelectionPopUp.SelectionEntry entry in Runes_Selection)
            {
                API.RuneItem stat = (API.RuneItem)entry.Object;
                entry.Header = stat.Name;
            }

            foreach (SelectionPopUp.SelectionEntry entry in Sigils_Selection)
            {
                API.SigilItem stat = (API.SigilItem)entry.Object;
                entry.Header = stat.Name;
            }
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        public EventHandler Changed;

        private void OnChanged()
        {
            BuildsManager.s_moduleInstance.Selected_Template.SetChanged();
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!MouseOver && !SelectionPopUp.MouseOver)
            {
                SelectionPopUp.Hide();
            }
        }

        private void SetClipboard(string text)
        {
            if (text != string.Empty && text != null)
            {
                try
                {
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(text);
                }
                catch (ArgumentException)
                {
                    ScreenNotification.ShowNotification("Failed to set the clipboard text!", ScreenNotification.NotificationType.Error);
                }
                catch
                {
                }
            }
        }

        private void OnRightClick(object sender, MouseEventArgs mouse)
        {
            if (DateTime.Now.Subtract(SelectionPopUp.LastClick).TotalMilliseconds < 250)
            {
                return;
            }

            SelectionPopUp.Hide();

            if (Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Alt))
            {
                foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
                {
                    if (item.Hovered)
                    {
                        bool canSelect = true;
                        if (item.Slot == EquipmentSlots.Weapon1_OffHand)
                        {
                            canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }

                        if (item.Slot == EquipmentSlots.Weapon2_OffHand)
                        {
                            canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }

                        if (canSelect)
                        {
                            SelectionPopUp.Show();
                            SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                            SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Weapons;
                            SelectionPopUp.List = Weapons_Selection;
                            SelectionPopUp.Slot = item.Slot;
                            SelectionPopUp.SelectionTarget = item;
                        }
                    }
                }

                foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
                {
                    if (item.Hovered)
                    {
                        SelectionPopUp.Show();
                        SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                        SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticWeapons;
                        SelectionPopUp.List = Weapons_Selection;
                        SelectionPopUp.Slot = item.Slot;
                        SelectionPopUp.SelectionTarget = item;
                    }
                }
            }
            else
            {
                string text = string.Empty;
                foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.UpgradeBounds.Contains(RelativeMousePosition) && item.Sigil != null)
                    {
                        SetClipboard(item.Sigil.Name);
                        text = item.Sigil.Name;
                    }
                }

                foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.Sigils != null)
                    {
                        for (int i = 0; i < item.Sigils.Count; i++)
                        {
                            if (item.Sigils[i] != null && item.SigilsBounds[i].Contains(RelativeMousePosition))
                            {
                                SetClipboard(item.Sigils[i].Name);
                                text = item.Sigils[i].Name;
                            }
                        }
                    }
                }

                foreach (Armor_TemplateItem item in Template.Gear.Armor)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.UpgradeBounds.Contains(RelativeMousePosition) && item.Rune != null)
                    {
                        SetClipboard(item.Rune.Name);
                        text = item.Rune.Name;
                    }
                }

                foreach (TemplateItem item in Template.Gear.Trinkets)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }
                }

                if (BuildsManager.s_moduleInstance.PasteOnCopy.Value)
                {
                    Paste(text);
                }
            }
        }

        public async void Paste(string text)
        {
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)
                               .ContinueWith(clipboardResult =>
                               {
                                   if (clipboardResult.IsFaulted)
                                   {
                                       BuildsManager.Logger.Warn(clipboardResult.Exception, "Failed to set clipboard text to {text}!", text);
                                   }
                                   else
                                   {
                                       Task.Run(() =>
                                       {
                                           Blish_HUD.Controls.Intern.Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                           Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
                                           Thread.Sleep(50);
                                           Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                           Thread.Sleep(50);
                                           Blish_HUD.Controls.Intern.Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                       }).ContinueWith(result =>
                                       {
                                           if (result.IsFaulted)
                                           {
                                               BuildsManager.Logger.Warn(result.Exception, "Failed to paste {text}", text);
                                           }
                                           else if (prevClipboardContent != null)
                                           {
                                               ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                           }
                                       });
                                   }
                               });
        }

        private void OnClick(object sender, MouseEventArgs m)
        {
            if (DateTime.Now.Subtract(SelectionPopUp.LastClick).TotalMilliseconds < 250)
            {
                return;
            }

            SelectionPopUp.Hide();

            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Stats_Selection;
                }
            }

            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Stats_Selection;
                }

                if (item.UpgradeBounds.Contains(RelativeMousePosition))
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Runes;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Runes_Selection;
                }
            }

            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item.Hovered)
                {
                    bool canSelect = true;
                    if (item.Slot == EquipmentSlots.Weapon1_OffHand)
                    {
                        canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }

                    if (item.Slot == EquipmentSlots.Weapon2_OffHand)
                    {
                        canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }

                    if (canSelect)
                    {
                        SelectionPopUp.Show();
                        SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                        SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                        SelectionPopUp.SelectionTarget = item;
                        SelectionPopUp.List = Stats_Selection;
                    }
                }

                if (item.UpgradeBounds.Contains(RelativeMousePosition))
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Sigils;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Sigils_Selection;
                }
            }

            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item.Hovered)
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.Bounds.Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Stats_Selection;
                }

                if (item.SigilsBounds[0].Contains(RelativeMousePosition))
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.SigilsBounds[1].Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.SigilsBounds[0].Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticSigils;
                    SelectionPopUp.UpgradeIndex = 0;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Sigils_Selection;
                }

                if (item.SigilsBounds[1].Contains(RelativeMousePosition))
                {
                    SelectionPopUp.Show();
                    SelectionPopUp.Location = new Point(Input.Mouse.Position.X - RelativeMousePosition.X + item.SigilsBounds[1].Right + 3, Input.Mouse.Position.Y - RelativeMousePosition.Y + item.SigilsBounds[1].Y - 1);
                    SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticSigils;
                    SelectionPopUp.UpgradeIndex = 1;
                    SelectionPopUp.SelectionTarget = item;
                    SelectionPopUp.List = Sigils_Selection;
                }
            }
        }

        private void UpdateTemplate()
        {
        }

        private void ProfessionChanged()
        {
            string id = "Unkown";

            if (Template.Build.Profession != null)
            {
                _Profession = Template.Build.Profession.Id;
                id = Template.Build.Profession.Id;
            }

            var armorWeight = id switch
            {
                "Elementalist" or "Necromancer" or "Mesmer" => API.armorWeight.Light,
                "Ranger" or "Thief" or "Engineer" => API.armorWeight.Medium,
                _ => API.armorWeight.Heavy,
            };
            Armors = new List<API.ArmorItem>()
            {
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
            };
            foreach (API.ArmorItem armor in BuildsManager.s_moduleInstance.Data.Armors)
            {
                if (armor.ArmorWeight == armorWeight)
                {
                    Armors[(int)armor.Slot] = armor;
                }
            }

            // SelectionPopUp.Template = Template;
        }

        public void UpdateLayout()
        {
            Point mPos = RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (CustomTooltip.Visible)
            {
                CustomTooltip.Visible = false;
            }

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + (i * (size + 6)), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + (i * (size + 6)), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + (i * (size + 6)) + (size - statSize), statSize, statSize);
                }

                i++;
            }

            i = 0;
            offset += 90;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + (i * (size + 6)), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + (i * (size + 6)), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + (i * (size + 6)) + (size - statSize), statSize, statSize);
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + (i * (size + 6)), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + (i * (size + 6)), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + (i * (size + 6)) + (size - statSize), statSize, statSize);
                }

                if (i == 1)
                {
                    i++;
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + (i * (size + 6)), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + (i * (size + 6)), size, size);

                    for (int j = 0; j < 2; j++)
                    {
                        item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), (item.UpgradeBounds.Width / 2) - 2, (item.UpgradeBounds.Height / 2) - 2);
                    }

                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + (i * (size + 6)) + (size - statSize), statSize, statSize);
                }

                if (i == 0)
                {
                    i = i + 2;
                }

                i++;
            }
        }

        private void UpdateStates()
        {
            Point mPos = RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (CustomTooltip.Visible)
            {
                CustomTooltip.Visible = false;
            }

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);
                    if (item.Hovered && item.Stat != null && (!SelectionPopUp.Visible || !SelectionPopUp.MouseOver))
                    {
                        CustomTooltip.Visible = true;

                        if (CustomTooltip.CurrentObject != item)
                        {
                            CustomTooltip.CurrentObject = item;
                            CustomTooltip.Header = item.Stat.Name;
                            CustomTooltip.TooltipContent = new List<string>();
                            foreach (API.StatAttribute attribute in item.Stat.Attributes)
                            {
                                CustomTooltip.TooltipContent.Add("+ " + (attribute.Value + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment)) + " " + attribute.Name);
                            }
                        }
                    }
;
                }

                i++;
            }

            i = 0;
            offset += 90;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);

                    if (!SelectionPopUp.Visible || !SelectionPopUp.MouseOver)
                    {
                        if (item.UpgradeBounds.Contains(mPos) && item.Rune != null)
                        {
                            CustomTooltip.Visible = true;
                            if (CustomTooltip.CurrentObject != item.Rune)
                            {
                                CustomTooltip.CurrentObject = item.Rune;
                                CustomTooltip.Header = item.Rune.Name;
                                CustomTooltip.TooltipContent = item.Rune.Bonuses;
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            CustomTooltip.Visible = true;
                            if (CustomTooltip.CurrentObject != item)
                            {
                                CustomTooltip.CurrentObject = item;
                                CustomTooltip.Header = item.Stat.Name;
                                CustomTooltip.TooltipContent = new List<string>();
                                foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                {
                                    CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * Armors[i].AttributeAdjustment) + " " + attribute.Name);
                                }
                            }
                        }
                    }
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);

                    if (!SelectionPopUp.Visible || !SelectionPopUp.MouseOver)
                    {
                        if (item.UpgradeBounds.Contains(mPos) && item.Sigil != null)
                        {
                            CustomTooltip.Visible = true;

                            if (CustomTooltip.CurrentObject != item.Sigil)
                            {
                                CustomTooltip.CurrentObject = item.Sigil;
                                CustomTooltip.Header = item.Sigil.Name;
                                CustomTooltip.TooltipContent = new List<string>() { item.Sigil.Description };
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            CustomTooltip.Visible = true;

                            if (CustomTooltip.CurrentObject != item)
                            {
                                CustomTooltip.CurrentObject = item;
                                CustomTooltip.Header = item.Stat.Name;
                                CustomTooltip.TooltipContent = new List<string>();

                                bool twoHanded = false;
                                if (item.Slot == EquipmentSlots.Weapon1_MainHand || item.Slot == EquipmentSlots.Weapon1_OffHand)
                                {
                                    twoHanded = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                if (item.Slot == EquipmentSlots.Weapon2_MainHand || item.Slot == EquipmentSlots.Weapon2_OffHand)
                                {
                                    twoHanded = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                API.WeaponItem weapon = twoHanded ? Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword) : Weapons.Find(e => e.WeaponType == API.weaponType.Axe);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
                                    }
                                }
                            }
                        }
                    }
                }

                if (i == 1)
                {
                    i++;
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item != null)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), (item.UpgradeBounds.Width / 2) - 2, (item.UpgradeBounds.Height / 2) - 2);

                        if ((!SelectionPopUp.Visible || !SelectionPopUp.MouseOver) && item.Sigils != null)
                        {
                            if (item.SigilsBounds[j].Contains(mPos) && item.Sigils.Count > j && item.Sigils[j] != null)
                            {
                                CustomTooltip.Visible = true;

                                if (CustomTooltip.CurrentObject != item.Sigils[j])
                                {
                                    CustomTooltip.CurrentObject = item.Sigils[j];
                                    CustomTooltip.Header = item.Sigils[j].Name;
                                    CustomTooltip.TooltipContent = new List<string>() { item.Sigils[j].Description };
                                }
                            }
                        }
                    }

                    item.Hovered = item.Bounds.Contains(mPos);
                    if (!SelectionPopUp.Visible || !SelectionPopUp.MouseOver)
                    {
                        if (item.Hovered && item.Stat != null)
                        {
                            CustomTooltip.Visible = true;
                            if (CustomTooltip.CurrentObject != item)
                            {
                                CustomTooltip.CurrentObject = item;
                                CustomTooltip.Header = item.Stat.Name;
                                CustomTooltip.TooltipContent = new List<string>();

                                API.WeaponItem weapon = Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
                                    }
                                }
                            }
                        }
                    }
                }

                if (i == 0)
                {
                    i = i + 2;
                }

                i++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template == null)
            {
                return;
            }

            if (Template.Build.Profession != null && _Profession != Template.Build.Profession.Id)
            {
                ProfessionChanged();
            }

            UpdateStates();

            int i;
            Color itemColor = new(75, 75, 75, 255);
            Color frameColor = new(125, 125, 125, 255);

            i = 0;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        frameColor,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Armors.Count > i ? Armors[i].Icon._AsyncTexture.Texture : BuildsManager.s_moduleInstance.TextureManager._Icons[0],
                        item.Bounds,
                        Armors.Count > i ? Armors[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.s_moduleInstance.TextureManager._Icons[0].Bounds,
                        itemColor,
                        0f,
                        default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            item.Stat.Icon._AsyncTexture,
                            item.StatBounds,
                            item.Stat.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }

                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        item.Rune == null ? Color.Transparent : frameColor,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        item.Rune == null ? _RuneTexture : item.Rune.Icon._AsyncTexture.Texture,
                        item.UpgradeBounds,
                        item.Rune == null ? _RuneTexture.Bounds : item.Rune.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                i++;
            }

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        frameColor,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Trinkets.Count > i ? Trinkets[i].Icon._AsyncTexture.Texture : BuildsManager.s_moduleInstance.TextureManager._Icons[0],
                        item.Bounds,
                        Trinkets.Count > i ? Trinkets[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.s_moduleInstance.TextureManager._Icons[0].Bounds,
                        itemColor,
                        0f,
                        default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            item.Stat.Icon._AsyncTexture,
                            item.StatBounds,
                            item.Stat.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }
                }

                i++;
            }

            i = 0;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        item.WeaponType != API.weaponType.Unkown ? frameColor : Color.Transparent,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture : WeaponSlots[i],
                        item.Bounds,
                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : WeaponSlots[i].Bounds,
                        item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                        0f,
                        default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            item.Stat.Icon._AsyncTexture,
                            item.StatBounds,
                            item.Stat.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }

                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        item.Sigil == null ? Color.Transparent : frameColor,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        item.Sigil == null ? _RuneTexture : item.Sigil.Icon._AsyncTexture.Texture,
                        item.UpgradeBounds,
                        item.Sigil == null ? _RuneTexture.Bounds : item.Sigil.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                i++;
            }

            i = 0;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                        Rectangle.Empty,
                        item.WeaponType != API.weaponType.Unkown ? frameColor : Color.Transparent,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture : AquaticWeaponSlots[i],
                        item.Bounds,
                        item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : AquaticWeaponSlots[i].Bounds,
                        item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                        0f,
                        default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            item.Stat.Icon._AsyncTexture,
                            item.StatBounds,
                            item.Stat.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        API.SigilItem sigil = item.Sigils != null && item.Sigils.Count > j && item.Sigils[j] != null && item.Sigils[j].Id > 0 ? item.Sigils[j] : null;

                        spriteBatch.DrawOnCtrl(
                            this,
                            ContentService.Textures.Pixel,
                            item.SigilsBounds[j].Add(new Rectangle(-1, -1, 2, 2)),
                            Rectangle.Empty,
                            sigil == null ? Color.Transparent : frameColor,
                            0f,
                            default);

                        spriteBatch.DrawOnCtrl(
                            this,
                            sigil == null ? _RuneTexture : sigil.Icon._AsyncTexture.Texture,
                            item.SigilsBounds[j],
                            sigil == null ? _RuneTexture.Bounds : sigil.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }
                }

                i++;
            }

            MonoGame.Extended.BitmapFonts.BitmapFont font = GameService.Content.DefaultFont14;

            TemplateItem lastTrinket = Template.Gear.Trinkets[Template.Gear.Trinkets.Count - 1];
            Texture2D texture = BuildsManager.s_moduleInstance.TextureManager.getEmblem(Emblems.QuestionMark);
            Texture2D mTexture = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Mouse);
            if (lastTrinket != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    new Rectangle(lastTrinket.Bounds.Left, LocalBounds.Bottom - (font.LineHeight * 3), font.LineHeight * 3, font.LineHeight * 3),
                    texture.Bounds,
                    Color.White,
                    0f,
                    default);

                i = 0;
                foreach (string s in Instructions)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        mTexture,
                        new Rectangle(lastTrinket.Bounds.Left + (font.LineHeight * 3), LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), font.LineHeight, font.LineHeight),
                        mTexture.Bounds,
                        Color.White,
                        0f,
                        default);

                    spriteBatch.DrawStringOnCtrl(
                        this,
                        s,
                        font,
                        new Rectangle(lastTrinket.Bounds.Left + 5 + (font.LineHeight * 4), LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), 100, font.LineHeight),
                        Color.White,
                        false,
                        HorizontalAlignment.Left);
                    i++;
                }
            }
        }
    }
}
