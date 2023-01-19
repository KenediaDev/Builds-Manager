using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SelectionPopUp : Control
    {
        public class SelectionEntry : IDisposable
        {
            private bool disposed = false;

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    Texture?.Dispose();
                    Texture = null;

                    ContentTextures?.DisposeAll();
                    ContentTextures = null;
                }
            }

            public object Object;
            public AsyncTexture2D Texture;
            public string Header;
            public List<string> Content;
            public List<AsyncTexture2D> ContentTextures;

            public Rectangle TextureBounds;
            public Rectangle TextBounds;
            public List<Rectangle> ContentBounds;
            public Rectangle Bounds;
            public Rectangle AbsolutBounds;
            public bool Hovered;
        }

        public enum selectionType
        {
            Runes,
            Sigils,
            Stats,
            Weapons,
            AquaticSigils,
            AquaticWeapons,
            Profession,
        }

        private readonly Texture2D Background;
        private readonly TextBox FilterBox;
        private selectionType _SelectionType;

        public selectionType SelectionType
        {
            get => _SelectionType;
            set
            {
                if (_SelectionType != value && FilterBox != null)
                {
                    FilterBox.Text = string.Empty;
                }

                _SelectionType = value;
            }
        }

        public List<SelectionEntry> List = new();
        public List<SelectionEntry> FilteredList = new();
        private object _SelectionTarget;

        public object SelectionTarget
        {
            get => _SelectionTarget;
            set
            {
                FilteredList = new List<SelectionEntry>();
                _SelectionTarget = value;
                UpdateLayouts = true;
            }
        }

        public Template Template => BuildsManager.s_moduleInstance.Selected_Template;

        public EquipmentSlots Slot = EquipmentSlots.Unkown;
        public int UpgradeIndex = 0;

        private readonly BitmapFont Font;
        public CustomTooltip CustomTooltip;
        public bool Clicked = false;
        public DateTime LastClick = DateTime.Now;

        private bool UpdateLayouts;

        public SelectionPopUp(Container parent)
        {
            Parent = parent;
            Visible = false;
            ZIndex = 997;
            Size = new Point(300, 500);
            Background = BuildsManager.s_moduleInstance.TextureManager._Backgrounds[(int)Backgrounds.Tooltip];

            // BackgroundColor = Color.Red;
            FilterBox = new TextBox()
            {
                Parent = Parent,
                PlaceholderText = Strings.common.Search + " ...",
                Width = Width - 6,
                ZIndex = 998,
                Visible = false,
            };
            FilterBox.TextChanged += FilterBox_TextChanged;
            BuildsManager.s_moduleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            Font = GameService.Content.DefaultFont14;

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            CustomTooltip?.Dispose();
            FilterBox?.Dispose();
            if (FilterBox != null)
            {
                FilterBox.TextChanged -= FilterBox_TextChanged;
            }

            BuildsManager.s_moduleInstance.LanguageChanged -= ModuleInstance_LanguageChanged;
            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;

            FilteredList?.DisposeAll();
            List?.DisposeAll();
            _SelectionTarget = null;

            SelectedProfession?.Dispose();
            SelectedProfession = null;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            FilterBox?.Hide();
            Clicked = false;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            if (FilterBox != null)
            {
                FilterBox.Width = Width - 6;
            }
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            if (FilterBox != null)
            {
                FilterBox.Location = Location.Add(new Point(3, 4));
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            OnChanged();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        public API.Profession SelectedProfession;

        public event EventHandler Changed;

        private void OnChanged()
        {
            if (List == null || List.Count == 0 || !Visible)
            {
                return;
            }

            List<SelectionEntry> list = new(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                if (entry.Hovered)
                {
                    switch (SelectionType)
                    {
                        case selectionType.Runes:
                            API.RuneItem rune = (API.RuneItem)entry.Object;
                            Armor_TemplateItem armor = (Armor_TemplateItem)SelectionTarget;
                            armor.Rune = rune;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Sigils:
                            API.SigilItem sigil = (API.SigilItem)entry.Object;
                            Weapon_TemplateItem weapon = (Weapon_TemplateItem)SelectionTarget;
                            weapon.Sigil = sigil;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticSigils:
                            API.SigilItem aquaSigil = (API.SigilItem)entry.Object;
                            AquaticWeapon_TemplateItem aquaWeapon = (AquaticWeapon_TemplateItem)SelectionTarget;
                            aquaWeapon.Sigils[UpgradeIndex] = aquaSigil;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Profession:
                            SelectedProfession = (API.Profession)entry.Object;
                            FilterBox.Text = null;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Stats:
                            API.Stat stat = (API.Stat)entry.Object;
                            TemplateItem item = (TemplateItem)SelectionTarget;
                            item.Stat = stat;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Weapons:
                            API.WeaponItem selectedWeapon = (API.WeaponItem)entry.Object;
                            Weapon_TemplateItem iWeapon = (Weapon_TemplateItem)SelectionTarget;
                            iWeapon.WeaponType = selectedWeapon.WeaponType;

                            switch (Slot)
                            {
                                case EquipmentSlots.Weapon1_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType = API.weaponType.Unkown;
                                    }

                                    break;

                                case EquipmentSlots.Weapon2_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType = API.weaponType.Unkown;
                                    }

                                    break;
                            }

                            Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticWeapons:
                            API.WeaponItem selectedAquaWeapon = (API.WeaponItem)entry.Object;
                            AquaticWeapon_TemplateItem AquaWeapon = (AquaticWeapon_TemplateItem)SelectionTarget;
                            AquaWeapon.WeaponType = selectedAquaWeapon.WeaponType;
                            Changed?.Invoke(this, EventArgs.Empty);
                            break;
                    }

                    LastClick = DateTime.Now;
                    List = new List<SelectionEntry>();
                    FilteredList = new List<SelectionEntry>();

                    Clicked = true;
                    Hide();
                    break;
                }
            }
        }

        private class filterTag
        {
            public string text;
            public bool match;
        }

        private void UpdateLayout()
        {
            if (List == null || List.Count == 0)
            {
                return;
            }

            int i = 0;
            int size = 42;

            FilteredList = new List<SelectionEntry>();
            if ((SelectionType == selectionType.Weapons || SelectionType == selectionType.AquaticWeapons) && Template != null && Template.Build != null && Template.Build.Profession != null)
            {
                List<string> weapons = new();

                foreach (API.ProfessionWeapon weapon in Template.Build.Profession.Weapons)
                {
                    if (weapon.Specialization == 0 || Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == weapon.Specialization) != null)
                    {
                        switch (Slot)
                        {
                            case EquipmentSlots.Weapon1_MainHand:
                            case EquipmentSlots.Weapon2_MainHand:
                                if (!weapon.Wielded.Contains(API.weaponHand.Aquatic) && (weapon.Wielded.Contains(API.weaponHand.Mainhand) || weapon.Wielded.Contains(API.weaponHand.TwoHand) || weapon.Wielded.Contains(API.weaponHand.DualWielded)))
                                {
                                    weapons.Add(weapon.Weapon.getLocalName());
                                }

                                break;

                            case EquipmentSlots.Weapon1_OffHand:
                            case EquipmentSlots.Weapon2_OffHand:
                                if (weapon.Wielded.Contains(API.weaponHand.Offhand) || weapon.Wielded.Contains(API.weaponHand.DualWielded))
                                {
                                    weapons.Add(weapon.Weapon.getLocalName());
                                }

                                break;

                            case EquipmentSlots.AquaticWeapon1:
                            case EquipmentSlots.AquaticWeapon2:
                                if (weapon.Wielded.Contains(API.weaponHand.Aquatic))
                                {
                                    weapons.Add(weapon.Weapon.getLocalName());
                                }

                                break;
                        }
                    }
                }

                List = List.Where(e => weapons.Contains(e.Header)).ToList();
            }

            if (FilterBox.Text != null && FilterBox.Text != string.Empty)
            {
                List<string> tags = FilterBox.Text.ToLower().Split(' ').ToList();
                IEnumerable<string> filteredTags = tags.Where(e => e.Trim().Length > 0);

                foreach (SelectionEntry entry in List)
                {
                    List<filterTag> Tags = new();

                    foreach (string t in filteredTags)
                    {
                        filterTag tag = new()
                        {
                            text = t.Trim().ToLower(),
                            match = false,
                        };
                        Tags.Add(tag);

                        if (entry.Header.ToLower().Contains(tag.text))
                        {
                            FilteredList.Add(entry);
                            tag.match = true;
                        }

                        foreach (string s in entry.Content)
                        {
                            string lower = s.ToLower();

                            tag.match = tag.match ? tag.match : lower.Contains(tag.text);
                            if (tag.match)
                            {
                                break;
                            }
                        }
                    }

                    if (!FilteredList.Contains(entry) && (Tags.Count == Tags.Where(e => e.match == true).ToList().Count))
                    {
                        FilteredList.Add(entry);
                    }
                }
            }
            else
            {
                FilteredList = new List<SelectionEntry>(List);
            }

            foreach (SelectionEntry entry in FilteredList)
            {
                entry.AbsolutBounds = new Rectangle(0, FilterBox.Height + 5 + (i * (size + 5)), Width, size);
                entry.TextureBounds = new Rectangle(2, FilterBox.Height + 5 + (i * (size + 5)), size, size);
                entry.TextBounds = new Rectangle(2 + size + 5, FilterBox.Height + (i * (size + 5)), size, size);
                entry.ContentBounds = new List<Rectangle>();

                int j = 0;
                int statSize = Font.LineHeight;
                if (entry.ContentTextures != null && entry.ContentTextures.Count > 0)
                {
                    foreach (AsyncTexture2D texture in entry.ContentTextures)
                    {
                        entry.ContentBounds.Add(new Rectangle(size + (j * statSize), FilterBox.Height + Font.LineHeight + 12 + (i * (size + 5)), statSize, statSize));
                        j++;
                    }
                }

                i++;
            }

            Height = FilterBox.Height + 5 + (Math.Min(10, Math.Max(FilteredList.Count, 1)) * (size + 5));
        }

        private void UpdateStates()
        {
            List<SelectionEntry> list = new(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                entry.Hovered = entry.AbsolutBounds.Contains(RelativeMousePosition);

                if (entry.Hovered && SelectionType != selectionType.Weapons && CustomTooltip != null)
                {
                    CustomTooltip.Visible = true;
                    CustomTooltip.Header = entry.Header;
                    CustomTooltip.TooltipContent = entry.Content;
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            UpdateLayout();
            FilterBox.Show();
            Clicked = false;

            FilterBox.Focused = true;
            FilterBox.SelectionStart = 0;
            FilterBox.SelectionEnd = FilterBox.Length;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Clicked = false;
            if (UpdateLayouts)
            {
                UpdateLayout();
                UpdateLayouts = false;
            }

            UpdateStates();

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(75, 75, 75, 255),
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                Background,
                bounds.Add(-2, 0, 0, 0),
                bounds,
                Color.White,
                0f,
                default);

            int i = 0;
            int size = 42;
            List<SelectionEntry> list = new(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    entry.Texture,
                    entry.TextureBounds,
                    entry.Texture.Texture.Bounds,
                    entry.Hovered ? Color.Orange : Color.White,
                    0f,
                    default);

                spriteBatch.DrawStringOnCtrl(
                    this,
                    entry.Header,
                    Font,
                    entry.TextBounds,
                    entry.Hovered ? Color.Orange : Color.White,
                    false,
                    HorizontalAlignment.Left);

                if (entry.ContentTextures != null && entry.ContentTextures.Count > 0)
                {
                    int j = 0;
                    int statSize = Font.LineHeight;
                    foreach (AsyncTexture2D texture in entry.ContentTextures)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            texture,
                            entry.ContentBounds[j],
                            texture.Texture.Bounds,
                            entry.Hovered ? Color.Orange : Color.White,
                            0f,
                            default);
                        j++;
                    }
                }
                else
                {
                    List<string> strings = new();
                    foreach (string s in entry.Content)
                    {
                        string ss = s;

                        if (s.Contains("<c=@reminder>"))
                        {
                            ss = Regex.Replace(s, "<c=@reminder>", Environment.NewLine + Environment.NewLine);

                            // if (CurrentObject != null && CurrentObject.GetType().Name == "Trait_Control") height += (font.LineHeight * Regex.Matches(s, "<c=@reminder>").Count);
                            // ss = Regex.Replace(s, "<c=@reminder>", Environment.NewLine + Environment.NewLine);
                        }

                        ss = Regex.Replace(ss, "<c=@abilitytype>", string.Empty);
                        ss = Regex.Replace(ss, "</c>", string.Empty);
                        ss = Regex.Replace(ss, "<br>", string.Empty);
                        strings.Add(ss);
                    }

                    spriteBatch.DrawStringOnCtrl(
                        this,
                        string.Join("; ", strings).Replace(Environment.NewLine, string.Empty),
                        Font,
                        new Rectangle(2 + size + 5, Font.LineHeight + FilterBox.Height + (i * (size + 5)) + Font.LineHeight - 5, size, Font.LineHeight),
                        Color.LightGray,
                        false,
                        HorizontalAlignment.Left,
                        VerticalAlignment.Top);
                }

                i++;
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }
    }
}
