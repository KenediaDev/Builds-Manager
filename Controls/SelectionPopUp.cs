namespace Kenedia.Modules.BuildsManager.Controls
{
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

    public class SelectionPopUp : Control
    {
        public class SelectionEntry : IDisposable
        {
            private bool disposed = false;

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.Texture?.Dispose();
                    this.Texture = null;

                    this.ContentTextures?.DisposeAll();
                    this.ContentTextures = null;
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

        private Texture2D Background;
        private TextBox FilterBox;
        private selectionType _SelectionType;

        public selectionType SelectionType
        {
            get => this._SelectionType;
            set
            {
                if (this._SelectionType != value && this.FilterBox != null)
                {
                    this.FilterBox.Text = string.Empty;
                }

                this._SelectionType = value;
            }
        }

        public List<SelectionEntry> List = new List<SelectionEntry>();
        public List<SelectionEntry> FilteredList = new List<SelectionEntry>();
        private object _SelectionTarget;

        public object SelectionTarget
        {
            get => this._SelectionTarget;
            set
            {
                this.FilteredList = new List<SelectionEntry>();
                this._SelectionTarget = value;
                this.UpdateLayouts = true;
            }
        }

        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }

        public EquipmentSlots Slot = EquipmentSlots.Unkown;
        public int UpgradeIndex = 0;

        private BitmapFont Font;
        public CustomTooltip CustomTooltip;
        public bool Clicked = false;
        public DateTime LastClick = DateTime.Now;

        private bool UpdateLayouts;

        public SelectionPopUp(Container parent)
        {
            this.Parent = parent;
            this.Visible = false;
            this.ZIndex = 997;
            this.Size = new Point(300, 500);
            this.Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)Backgrounds.Tooltip];

            // BackgroundColor = Color.Red;
            this.FilterBox = new TextBox()
            {
                Parent = this.Parent,
                PlaceholderText = Strings.common.Search + " ...",
                Width = this.Width - 6,
                ZIndex = 998,
                Visible = false,
            };
            this.FilterBox.TextChanged += this.FilterBox_TextChanged;
            BuildsManager.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;

            this.Font = GameService.Content.DefaultFont14;

            Input.Mouse.LeftMouseButtonPressed += this.Mouse_LeftMouseButtonPressed;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.CustomTooltip?.Dispose();
            this.FilterBox?.Dispose();
            if (this.FilterBox != null)
            {
                this.FilterBox.TextChanged -= this.FilterBox_TextChanged;
            }

            BuildsManager.ModuleInstance.LanguageChanged -= this.ModuleInstance_LanguageChanged;
            Input.Mouse.LeftMouseButtonPressed -= this.Mouse_LeftMouseButtonPressed;

            this.FilteredList?.DisposeAll();
            this.List?.DisposeAll();
            this._SelectionTarget = null;

            this.SelectedProfession?.Dispose();
            this.SelectedProfession = null;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            this.FilterBox?.Hide();
            this.Clicked = false;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            if (this.FilterBox != null)
            {
                this.FilterBox.Width = this.Width - 6;
            }
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);

            if (this.FilterBox != null)
            {
                this.FilterBox.Location = this.Location.Add(new Point(3, 4));
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            this.OnChanged();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.FilterBox.PlaceholderText = Strings.common.Search + " ...";
        }

        private void FilterBox_TextChanged(object sender, EventArgs e)
        {
            this.UpdateLayout();
        }

        public API.Profession SelectedProfession;

        public event EventHandler Changed;

        private void OnChanged()
        {
            if (this.List == null || this.List.Count == 0 || !this.Visible)
            {
                return;
            }

            var list = new List<SelectionEntry>(this.FilteredList);
            foreach (SelectionEntry entry in list)
            {
                if (entry.Hovered)
                {
                    switch (this.SelectionType)
                    {
                        case selectionType.Runes:
                            var rune = (API.RuneItem)entry.Object;
                            var armor = (Armor_TemplateItem)this.SelectionTarget;
                            armor.Rune = rune;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Sigils:
                            var sigil = (API.SigilItem)entry.Object;
                            var weapon = (Weapon_TemplateItem)this.SelectionTarget;
                            weapon.Sigil = sigil;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticSigils:
                            var aquaSigil = (API.SigilItem)entry.Object;
                            var aquaWeapon = (AquaticWeapon_TemplateItem)this.SelectionTarget;
                            aquaWeapon.Sigils[this.UpgradeIndex] = aquaSigil;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Profession:
                            this.SelectedProfession = (API.Profession)entry.Object;
                            this.FilterBox.Text = null;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Stats:
                            var stat = (API.Stat)entry.Object;
                            var item = (TemplateItem)this.SelectionTarget;
                            item.Stat = stat;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Weapons:
                            var selectedWeapon = (API.WeaponItem)entry.Object;
                            var iWeapon = (Weapon_TemplateItem)this.SelectionTarget;
                            iWeapon.WeaponType = selectedWeapon.WeaponType;

                            switch (this.Slot)
                            {
                                case EquipmentSlots.Weapon1_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType = API.weaponType.Unkown;
                                    }

                                    break;

                                case EquipmentSlots.Weapon2_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType = API.weaponType.Unkown;
                                    }

                                    break;
                            }

                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticWeapons:
                            var selectedAquaWeapon = (API.WeaponItem)entry.Object;
                            var AquaWeapon = (AquaticWeapon_TemplateItem)this.SelectionTarget;
                            AquaWeapon.WeaponType = selectedAquaWeapon.WeaponType;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;
                    }

                    this.LastClick = DateTime.Now;
                    this.List = new List<SelectionEntry>();
                    this.FilteredList = new List<SelectionEntry>();

                    this.Clicked = true;
                    this.Hide();
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
            if (this.List == null || this.List.Count == 0)
            {
                return;
            }

            int i = 0;
            int size = 42;

            this.FilteredList = new List<SelectionEntry>();
            if ((this.SelectionType == selectionType.Weapons || this.SelectionType == selectionType.AquaticWeapons) && this.Template != null && this.Template.Build != null && this.Template.Build.Profession != null)
            {
                List<string> weapons = new List<string>();

                foreach (API.ProfessionWeapon weapon in this.Template.Build.Profession.Weapons)
                {
                    if (weapon.Specialization == 0 || this.Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == weapon.Specialization) != null)
                    {
                        switch (this.Slot)
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

                this.List = this.List.Where(e => weapons.Contains(e.Header)).ToList();
            }

            if (this.FilterBox.Text != null && this.FilterBox.Text != string.Empty)
            {
                List<string> tags = this.FilterBox.Text.ToLower().Split(' ').ToList();
                var filteredTags = tags.Where(e => e.Trim().Length > 0);

                foreach (SelectionEntry entry in this.List)
                {
                    List<filterTag> Tags = new List<filterTag>();

                    foreach (string t in filteredTags)
                    {
                        var tag = new filterTag()
                        {
                            text = t.Trim().ToLower(),
                            match = false,
                        };
                        Tags.Add(tag);

                        if (entry.Header.ToLower().Contains(tag.text))
                        {
                            this.FilteredList.Add(entry);
                            tag.match = true;
                        }

                        foreach (string s in entry.Content)
                        {
                            var lower = s.ToLower();

                            tag.match = tag.match ? tag.match : lower.Contains(tag.text);
                            if (tag.match)
                            {
                                break;
                            }
                        }
                    }

                    if (!this.FilteredList.Contains(entry) && (Tags.Count == Tags.Where(e => e.match == true).ToList().Count))
                    {
                        this.FilteredList.Add(entry);
                    }
                }
            }
            else
            {
                this.FilteredList = new List<SelectionEntry>(this.List);
            }

            foreach (SelectionEntry entry in this.FilteredList)
            {
                entry.AbsolutBounds = new Rectangle(0, this.FilterBox.Height + 5 + (i * (size + 5)), this.Width, size);
                entry.TextureBounds = new Rectangle(2, this.FilterBox.Height + 5 + (i * (size + 5)), size, size);
                entry.TextBounds = new Rectangle(2 + size + 5, this.FilterBox.Height + (i * (size + 5)), size, size);
                entry.ContentBounds = new List<Rectangle>();

                int j = 0;
                int statSize = this.Font.LineHeight;
                if (entry.ContentTextures != null && entry.ContentTextures.Count > 0)
                {
                    foreach (AsyncTexture2D texture in entry.ContentTextures)
                    {
                        entry.ContentBounds.Add(new Rectangle(size + (j * statSize), this.FilterBox.Height + this.Font.LineHeight + 12 + (i * (size + 5)), statSize, statSize));
                        j++;
                    }
                }

                i++;
            }

            this.Height = this.FilterBox.Height + 5 + (Math.Min(10, Math.Max(this.FilteredList.Count, 1)) * (size + 5));
        }

        private void UpdateStates()
        {
            var list = new List<SelectionEntry>(this.FilteredList);
            foreach (SelectionEntry entry in list)
            {
                entry.Hovered = entry.AbsolutBounds.Contains(this.RelativeMousePosition);

                if (entry.Hovered && this.SelectionType != selectionType.Weapons && this.CustomTooltip != null)
                {
                    this.CustomTooltip.Visible = true;
                    this.CustomTooltip.Header = entry.Header;
                    this.CustomTooltip.TooltipContent = entry.Content;
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.UpdateLayout();
            this.FilterBox.Show();
            this.Clicked = false;

            this.FilterBox.Focused = true;
            this.FilterBox.SelectionStart = 0;
            this.FilterBox.SelectionEnd = this.FilterBox.Length;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            this.Clicked = false;
            if (this.UpdateLayouts)
            {
                this.UpdateLayout();
                this.UpdateLayouts = false;
            }

            this.UpdateStates();

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
                this.Background,
                bounds.Add(-2, 0, 0, 0),
                bounds,
                Color.White,
                0f,
                default);

            int i = 0;
            int size = 42;
            var list = new List<SelectionEntry>(this.FilteredList);
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
                    this.Font,
                    entry.TextBounds,
                    entry.Hovered ? Color.Orange : Color.White,
                    false,
                    HorizontalAlignment.Left);

                if (entry.ContentTextures != null && entry.ContentTextures.Count > 0)
                {
                    int j = 0;
                    int statSize = this.Font.LineHeight;
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
                    List<string> strings = new List<string>();
                    foreach (string s in entry.Content)
                    {
                        var ss = s;

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
                        this.Font,
                        new Rectangle(2 + size + 5, this.Font.LineHeight + this.FilterBox.Height + (i * (size + 5)) + this.Font.LineHeight - 5, size, this.Font.LineHeight),
                        Color.LightGray,
                        false,
                        HorizontalAlignment.Left,
                        VerticalAlignment.Top);
                }

                i++;
            }

            var color = Color.Black;

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
