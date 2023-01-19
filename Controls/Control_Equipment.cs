namespace Kenedia.Modules.BuildsManager.Controls
{
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

    public class Control_Equipment : Control
    {
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public double Scale;
        private Texture2D _RuneTexture;
        private List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();
        private List<API.ArmorItem> Armors = new List<API.ArmorItem>();
        private List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
        private List<Texture2D> WeaponSlots = new List<Texture2D>();
        private List<Texture2D> AquaticWeaponSlots = new List<Texture2D>();

        private List<SelectionPopUp.SelectionEntry> Stats_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Sigils_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Runes_Selection = new List<SelectionPopUp.SelectionEntry>();
        private List<SelectionPopUp.SelectionEntry> Weapons_Selection = new List<SelectionPopUp.SelectionEntry>();

        private List<string> Instructions = new List<string>();

        private string _Profession;
        public CustomTooltip CustomTooltip;
        public SelectionPopUp SelectionPopUp;

        public Control_Equipment(Container parent)
        {
            this.Parent = parent;

            // BackgroundColor = Color.Aqua;
            this._RuneTexture = BuildsManager.ModuleInstance.TextureManager.getEquipTexture(EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            this.Trinkets = new List<API.TrinketItem>();
            foreach (API.TrinketItem item in BuildsManager.ModuleInstance.Data.Trinkets)
            {
                this.Trinkets.Add(item);
            }

            this.WeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon1_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon1_OffHand],

                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon2_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.Weapon2_OffHand],
            };

            this.AquaticWeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.AquaticWeapon1],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)EquipSlotTextures.AquaticWeapon2],
            };

            this.Weapons = new List<API.WeaponItem>() { };
            foreach (API.WeaponItem weapon in BuildsManager.ModuleInstance.Data.Weapons) { this.Weapons.Add(weapon); }

            this.Click += this.OnClick;
            this.RightMouseButtonPressed += this.OnRightClick;
            Input.Mouse.LeftMouseButtonPressed += this.OnGlobalClick;

            this.CustomTooltip = new CustomTooltip(this.Parent)
            {
                ClipsBounds = false,
            };
            this.SelectionPopUp = new SelectionPopUp(GameService.Graphics.SpriteScreen)
            {
                CustomTooltip = this.CustomTooltip,

                // Template = Template,
            };
            this.SelectionPopUp.Changed += this.SelectionPopUp_Changed;

            foreach (API.RuneItem item in BuildsManager.ModuleInstance.Data.Runes)
            {
                this.Runes_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = item.Bonuses,
                });
            }

            foreach (API.SigilItem item in BuildsManager.ModuleInstance.Data.Sigils)
            {
                this.Sigils_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = new List<string>() { item.Description },
                });
            }

            foreach (API.WeaponItem item in this.Weapons)
            {
                this.Weapons_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.WeaponType.getLocalName(),
                    Content = new List<string>() { string.Empty },
                });
            }

            foreach (API.Stat item in BuildsManager.ModuleInstance.Data.Stats)
            {
                this.Stats_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = item.Attributes.Select(e => "+ " + e.Name).ToList(),
                    ContentTextures = item.Attributes.Select(e => e.Icon._AsyncTexture).ToList(),
                });
            }

            this.Instructions = Strings.common.GearTab_Tips.Split('\n').ToList();
            BuildsManager.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;

            this.ProfessionChanged();
            this.UpdateLayout();

            BuildsManager.ModuleInstance.Selected_Template_Changed += this.ModuleInstance_Selected_Template_Changed;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.CustomTooltip?.Dispose();
            this.SelectionPopUp?.Dispose();

            this.Click -= this.OnClick;
            this.RightMouseButtonPressed -= this.OnRightClick;
            Input.Mouse.LeftMouseButtonPressed -= this.OnGlobalClick;
            this.SelectionPopUp.Changed -= this.SelectionPopUp_Changed;

            this.Runes_Selection.DisposeAll();
            this.Sigils_Selection.DisposeAll();
            this.Weapons.DisposeAll();
            this.Weapons_Selection.DisposeAll();
            this.Stats_Selection.DisposeAll();

            BuildsManager.ModuleInstance.LanguageChanged -= this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Selected_Template_Changed -= this.ModuleInstance_Selected_Template_Changed;
        }

        protected override void OnShown(EventArgs e)
        {
            this.UpdateLayout();
        }

        private void SelectionPopUp_Changed(object sender, EventArgs e)
        {
            this.OnChanged();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.Instructions = Strings.common.GearTab_Tips.Split('\n').ToList();

            foreach (SelectionPopUp.SelectionEntry entry in this.Weapons_Selection)
            {
                var item = (API.WeaponItem)entry.Object;
                entry.Header = item.WeaponType.getLocalName();
            }

            foreach (SelectionPopUp.SelectionEntry entry in this.Stats_Selection)
            {
                var stat = (API.Stat)entry.Object;
                entry.Header = stat.Name;
            }

            foreach (SelectionPopUp.SelectionEntry entry in this.Runes_Selection)
            {
                var stat = (API.RuneItem)entry.Object;
                entry.Header = stat.Name;
            }

            foreach (SelectionPopUp.SelectionEntry entry in this.Sigils_Selection)
            {
                var stat = (API.SigilItem)entry.Object;
                entry.Header = stat.Name;
            }
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }

        public EventHandler Changed;

        private void OnChanged()
        {
            BuildsManager.ModuleInstance.Selected_Template.SetChanged();
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!this.MouseOver && !this.SelectionPopUp.MouseOver)
            {
                this.SelectionPopUp.Hide();
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
            if (DateTime.Now.Subtract(this.SelectionPopUp.LastClick).TotalMilliseconds < 250)
            {
                return;
            }

            this.SelectionPopUp.Hide();

            if (Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Alt))
            {
                foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
                {
                    if (item.Hovered)
                    {
                        bool canSelect = true;
                        if (item.Slot == EquipmentSlots.Weapon1_OffHand)
                        {
                            canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }

                        if (item.Slot == EquipmentSlots.Weapon2_OffHand)
                        {
                            canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }

                        if (canSelect)
                        {
                            this.SelectionPopUp.Show();
                            this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                            this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Weapons;
                            this.SelectionPopUp.List = this.Weapons_Selection;
                            this.SelectionPopUp.Slot = item.Slot;
                            this.SelectionPopUp.SelectionTarget = item;
                        }
                    }
                }

                foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
                {
                    if (item.Hovered)
                    {
                        this.SelectionPopUp.Show();
                        this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                        this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticWeapons;
                        this.SelectionPopUp.List = this.Weapons_Selection;
                        this.SelectionPopUp.Slot = item.Slot;
                        this.SelectionPopUp.SelectionTarget = item;
                    }
                }
            }
            else
            {
                var text = string.Empty;
                foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        this.SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.UpgradeBounds.Contains(this.RelativeMousePosition) && item.Sigil != null)
                    {
                        this.SetClipboard(item.Sigil.Name);
                        text = item.Sigil.Name;
                    }
                }

                foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        this.SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.Sigils != null)
                    {
                        for (int i = 0; i < item.Sigils.Count; i++)
                        {
                            if (item.Sigils[i] != null && item.SigilsBounds[i].Contains(this.RelativeMousePosition))
                            {
                                this.SetClipboard(item.Sigils[i].Name);
                                text = item.Sigils[i].Name;
                            }
                        }
                    }
                }

                foreach (Armor_TemplateItem item in this.Template.Gear.Armor)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        this.SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }

                    if (item.UpgradeBounds.Contains(this.RelativeMousePosition) && item.Rune != null)
                    {
                        this.SetClipboard(item.Rune.Name);
                        text = item.Rune.Name;
                    }
                }

                foreach (TemplateItem item in this.Template.Gear.Trinkets)
                {
                    if (item.Hovered && item.Stat != null)
                    {
                        this.SetClipboard(item.Stat.Name);
                        text = item.Stat.Name;
                    }
                }

                if (BuildsManager.ModuleInstance.PasteOnCopy.Value)
                {
                    this.Paste(text);
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
                                               ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                       });
                               });
        }

        private void OnClick(object sender, MouseEventArgs m)
        {
            if (DateTime.Now.Subtract(this.SelectionPopUp.LastClick).TotalMilliseconds < 250)
            {
                return;
            }

            this.SelectionPopUp.Hide();

            foreach (TemplateItem item in this.Template.Gear.Trinkets)
            {
                if (item.Hovered)
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Stats_Selection;
                }
            }

            foreach (Armor_TemplateItem item in this.Template.Gear.Armor)
            {
                if (item.Hovered)
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Stats_Selection;
                }

                if (item.UpgradeBounds.Contains(this.RelativeMousePosition))
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Runes;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Runes_Selection;
                }
            }

            foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
            {
                if (item.Hovered)
                {
                    bool canSelect = true;
                    if (item.Slot == EquipmentSlots.Weapon1_OffHand)
                    {
                        canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }

                    if (item.Slot == EquipmentSlots.Weapon2_OffHand)
                    {
                        canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }

                    if (canSelect)
                    {
                        this.SelectionPopUp.Show();
                        this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                        this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                        this.SelectionPopUp.SelectionTarget = item;
                        this.SelectionPopUp.List = this.Stats_Selection;
                    }
                }

                if (item.UpgradeBounds.Contains(this.RelativeMousePosition))
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Sigils;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Sigils_Selection;
                }
            }

            foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
            {
                if (item.Hovered)
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.Bounds.Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.Bounds.Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.Stats;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Stats_Selection;
                }

                if (item.SigilsBounds[0].Contains(this.RelativeMousePosition))
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.SigilsBounds[1].Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.SigilsBounds[0].Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticSigils;
                    this.SelectionPopUp.UpgradeIndex = 0;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Sigils_Selection;
                }

                if (item.SigilsBounds[1].Contains(this.RelativeMousePosition))
                {
                    this.SelectionPopUp.Show();
                    this.SelectionPopUp.Location = new Point(Input.Mouse.Position.X - this.RelativeMousePosition.X + item.SigilsBounds[1].Right + 3, Input.Mouse.Position.Y - this.RelativeMousePosition.Y + item.SigilsBounds[1].Y - 1);
                    this.SelectionPopUp.SelectionType = SelectionPopUp.selectionType.AquaticSigils;
                    this.SelectionPopUp.UpgradeIndex = 1;
                    this.SelectionPopUp.SelectionTarget = item;
                    this.SelectionPopUp.List = this.Sigils_Selection;
                }
            }

        }

        private void UpdateTemplate()
        {
        }

        private void ProfessionChanged()
        {
            var id = "Unkown";

            if (this.Template.Build.Profession != null)
            {
                this._Profession = this.Template.Build.Profession.Id;
                id = this.Template.Build.Profession.Id;
            }

            var armorWeight = API.armorWeight.Heavy;

            switch (id)
            {
                case "Elementalist":
                case "Necromancer":
                case "Mesmer":
                    armorWeight = API.armorWeight.Light;
                    break;

                case "Ranger":
                case "Thief":
                case "Engineer":
                    armorWeight = API.armorWeight.Medium;
                    break;

                default:
                    armorWeight = API.armorWeight.Heavy;
                    break;
            }

            this.Armors = new List<API.ArmorItem>()
            {
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
                new API.ArmorItem(),
            };
            foreach (API.ArmorItem armor in BuildsManager.ModuleInstance.Data.Armors)
            {
                if (armor.ArmorWeight == armorWeight)
                {
                    this.Armors[(int)armor.Slot] = armor;
                }
            }

            // SelectionPopUp.Template = Template;
        }

        public void UpdateLayout()
        {
            Point mPos = this.RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (this.CustomTooltip.Visible)
            {
                this.CustomTooltip.Visible = false;
            }

            i = 0;
            foreach (TemplateItem item in this.Template.Gear.Trinkets)
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
            foreach (Armor_TemplateItem item in this.Template.Gear.Armor)
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
            foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
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
            foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
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
            Point mPos = this.RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (this.CustomTooltip.Visible)
            {
                this.CustomTooltip.Visible = false;
            }

            i = 0;
            foreach (TemplateItem item in this.Template.Gear.Trinkets)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);
                    if (item.Hovered && item.Stat != null && (!this.SelectionPopUp.Visible || !this.SelectionPopUp.MouseOver))
                    {
                        this.CustomTooltip.Visible = true;

                        if (this.CustomTooltip.CurrentObject != item)
                        {
                            this.CustomTooltip.CurrentObject = item;
                            this.CustomTooltip.Header = item.Stat.Name;
                            this.CustomTooltip.TooltipContent = new List<string>();
                            foreach (API.StatAttribute attribute in item.Stat.Attributes)
                            {
                                this.CustomTooltip.TooltipContent.Add("+ " + (attribute.Value + Math.Round(attribute.Multiplier * this.Trinkets[i].AttributeAdjustment)) + " " + attribute.Name);
                            }
                        }
                    }
;
                }

                i++;
            }

            i = 0;
            offset += 90;
            foreach (Armor_TemplateItem item in this.Template.Gear.Armor)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);

                    if (!this.SelectionPopUp.Visible || !this.SelectionPopUp.MouseOver)
                    {
                        if (item.UpgradeBounds.Contains(mPos) && item.Rune != null)
                        {
                            this.CustomTooltip.Visible = true;
                            if (this.CustomTooltip.CurrentObject != item.Rune)
                            {
                                this.CustomTooltip.CurrentObject = item.Rune;
                                this.CustomTooltip.Header = item.Rune.Name;
                                this.CustomTooltip.TooltipContent = item.Rune.Bonuses;
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            this.CustomTooltip.Visible = true;
                            if (this.CustomTooltip.CurrentObject != item)
                            {
                                this.CustomTooltip.CurrentObject = item;
                                this.CustomTooltip.Header = item.Stat.Name;
                                this.CustomTooltip.TooltipContent = new List<string>();
                                foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                {
                                    this.CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * this.Armors[i].AttributeAdjustment) + " " + attribute.Name);
                                }
                            }
                        }
                    }
                }

                i++;
            }

            i = 0;
            offset += 150;
            foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
            {
                if (item != null)
                {
                    item.Hovered = item.Bounds.Contains(mPos);

                    if (!this.SelectionPopUp.Visible || !this.SelectionPopUp.MouseOver)
                    {
                        if (item.UpgradeBounds.Contains(mPos) && item.Sigil != null)
                        {
                            this.CustomTooltip.Visible = true;

                            if (this.CustomTooltip.CurrentObject != item.Sigil)
                            {
                                this.CustomTooltip.CurrentObject = item.Sigil;
                                this.CustomTooltip.Header = item.Sigil.Name;
                                this.CustomTooltip.TooltipContent = new List<string>() { item.Sigil.Description };
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            this.CustomTooltip.Visible = true;

                            if (this.CustomTooltip.CurrentObject != item)
                            {
                                this.CustomTooltip.CurrentObject = item;
                                this.CustomTooltip.Header = item.Stat.Name;
                                this.CustomTooltip.TooltipContent = new List<string>();

                                bool twoHanded = false;
                                if (item.Slot == EquipmentSlots.Weapon1_MainHand || item.Slot == EquipmentSlots.Weapon1_OffHand)
                                {
                                    twoHanded = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                if (item.Slot == EquipmentSlots.Weapon2_MainHand || item.Slot == EquipmentSlots.Weapon2_OffHand)
                                {
                                    twoHanded = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                var weapon = twoHanded ? this.Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword) : this.Weapons.Find(e => e.WeaponType == API.weaponType.Axe);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        this.CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
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
            foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
            {
                if (item != null)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), (item.UpgradeBounds.Width / 2) - 2, (item.UpgradeBounds.Height / 2) - 2);

                        if ((!this.SelectionPopUp.Visible || !this.SelectionPopUp.MouseOver) && item.Sigils != null)
                        {
                            if (item.SigilsBounds[j].Contains(mPos) && item.Sigils.Count > j && item.Sigils[j] != null)
                            {
                                this.CustomTooltip.Visible = true;

                                if (this.CustomTooltip.CurrentObject != item.Sigils[j])
                                {
                                    this.CustomTooltip.CurrentObject = item.Sigils[j];
                                    this.CustomTooltip.Header = item.Sigils[j].Name;
                                    this.CustomTooltip.TooltipContent = new List<string>() { item.Sigils[j].Description };
                                }
                            }
                        }
                    }

                    item.Hovered = item.Bounds.Contains(mPos);
                    if (!this.SelectionPopUp.Visible || !this.SelectionPopUp.MouseOver)
                    {
                        if (item.Hovered && item.Stat != null)
                        {
                            this.CustomTooltip.Visible = true;
                            if (this.CustomTooltip.CurrentObject != item)
                            {
                                this.CustomTooltip.CurrentObject = item;
                                this.CustomTooltip.Header = item.Stat.Name;
                                this.CustomTooltip.TooltipContent = new List<string>();

                                var weapon = this.Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        this.CustomTooltip.TooltipContent.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
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
            if (this.Template == null)
            {
                return;
            }

            if (this.Template.Build.Profession != null && this._Profession != this.Template.Build.Profession.Id)
            {
                this.ProfessionChanged();
            }

            this.UpdateStates();

            int i;
            Color itemColor = new Color(75, 75, 75, 255);
            Color frameColor = new Color(125, 125, 125, 255);

            i = 0;
            foreach (Armor_TemplateItem item in this.Template.Gear.Armor)
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
                        this.Armors.Count > i ? this.Armors[i].Icon._AsyncTexture.Texture : BuildsManager.ModuleInstance.TextureManager._Icons[0],
                        item.Bounds,
                        this.Armors.Count > i ? this.Armors[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.ModuleInstance.TextureManager._Icons[0].Bounds,
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
                        item.Rune == null ? this._RuneTexture : item.Rune.Icon._AsyncTexture.Texture,
                        item.UpgradeBounds,
                        item.Rune == null ? this._RuneTexture.Bounds : item.Rune.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                i++;
            }

            i = 0;
            foreach (TemplateItem item in this.Template.Gear.Trinkets)
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
                        this.Trinkets.Count > i ? this.Trinkets[i].Icon._AsyncTexture.Texture : BuildsManager.ModuleInstance.TextureManager._Icons[0],
                        item.Bounds,
                        this.Trinkets.Count > i ? this.Trinkets[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.ModuleInstance.TextureManager._Icons[0].Bounds,
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
            foreach (Weapon_TemplateItem item in this.Template.Gear.Weapons)
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
                        item.WeaponType != API.weaponType.Unkown ? this.Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture : this.WeaponSlots[i],
                        item.Bounds,
                        item.WeaponType != API.weaponType.Unkown ? this.Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : this.WeaponSlots[i].Bounds,
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
                        item.Sigil == null ? this._RuneTexture : item.Sigil.Icon._AsyncTexture.Texture,
                        item.UpgradeBounds,
                        item.Sigil == null ? this._RuneTexture.Bounds : item.Sigil.Icon._AsyncTexture.Texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                i++;
            }

            i = 0;
            foreach (AquaticWeapon_TemplateItem item in this.Template.Gear.AquaticWeapons)
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
                        item.WeaponType != API.weaponType.Unkown ? this.Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture : this.AquaticWeaponSlots[i],
                        item.Bounds,
                        item.WeaponType != API.weaponType.Unkown ? this.Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : this.AquaticWeaponSlots[i].Bounds,
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
                        var sigil = item.Sigils != null && item.Sigils.Count > j && item.Sigils[j] != null && item.Sigils[j].Id > 0 ? item.Sigils[j] : null;

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
                            sigil == null ? this._RuneTexture : sigil.Icon._AsyncTexture.Texture,
                            item.SigilsBounds[j],
                            sigil == null ? this._RuneTexture.Bounds : sigil.Icon._AsyncTexture.Texture.Bounds,
                            Color.White,
                            0f,
                            default);
                    }
                }

                i++;
            }

            var font = GameService.Content.DefaultFont14;

            var lastTrinket = this.Template.Gear.Trinkets[this.Template.Gear.Trinkets.Count - 1];
            var texture = BuildsManager.ModuleInstance.TextureManager.getEmblem(Emblems.QuestionMark);
            var mTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(Icons.Mouse);
            if (lastTrinket != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    new Rectangle(lastTrinket.Bounds.Left, this.LocalBounds.Bottom - (font.LineHeight * 3), font.LineHeight * 3, font.LineHeight * 3),
                    texture.Bounds,
                    Color.White,
                    0f,
                    default);

                i = 0;
                foreach (string s in this.Instructions)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        mTexture,
                        new Rectangle(lastTrinket.Bounds.Left + (font.LineHeight * 3), this.LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), font.LineHeight, font.LineHeight),
                        mTexture.Bounds,
                        Color.White,
                        0f,
                        default);

                    spriteBatch.DrawStringOnCtrl(
                        this,
                        s,
                        font,
                        new Rectangle(lastTrinket.Bounds.Left + 5 + (font.LineHeight * 4), this.LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), 100, font.LineHeight),
                        Color.White,
                        false,
                        HorizontalAlignment.Left);
                    i++;
                }
            }
        }
    }
}
