namespace Kenedia.Modules.BuildsManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Controls.Extern;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class CustomTooltip : Control
    {
        private Texture2D Background;
        private int _Height;
        private object _CurrentObject;

        public object CurrentObject
        {
            get => this._CurrentObject;
            set
            {
                this._CurrentObject = value;
                this._Height = -1;
            }
        }

        private string _Header;

        public string Header
        {
            get => this._Header;
            set
            {
                this._Header = value;
                this._Height = -1;
            }
        }

        public Color HeaderColor = Color.Orange;
        public Color ContentColor = Color.Honeydew;
        private List<string> _Content;

        public List<string> TooltipContent
        {
            get => this._Content;
            set
            {
                this._Content = value;
                this._Height = -1;
            }
        }

        public CustomTooltip(Container parent)
        {
            this.Parent = GameService.Graphics.SpriteScreen;

            this.Size = new Point(225, 275);
            this.Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];
            this.ZIndex = 1000;
            this.Visible = false;

            Input.Mouse.MouseMoved += this.Mouse_MouseMoved;
        }

        private void Mouse_MouseMoved(object sender, MouseEventArgs e)
        {
            this.Location = Input.Mouse.Position.Add(new Point(20, -10));
        }

        private void UpdateLayout()
        {
            if (this.Header == null || this.TooltipContent == null)
            {
                return;
            }

            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var ItemNameSize = headerFont.GetStringRectangle(this.Header);

            var width = (int)ItemNameSize.Width + 30;

            // var height = 10 + (int)ItemNameSize.Height;
            var height = 0;

            List<string> newStrings = new List<string>();
            foreach (string s in this.TooltipContent)
            {
                var ss = s;

                ss = Regex.Replace(ss, "<c=@reminder>", "\n\n");
                ss = Regex.Replace(ss, "<c=@abilitytype>", string.Empty);
                ss = Regex.Replace(ss, "</c>", string.Empty);
                ss = Regex.Replace(ss, "<br>", string.Empty);
                ss = ss.Replace(Environment.NewLine, "\n");
                newStrings.Add(ss);

                var rect = font.GetStringRectangle(ss);
                width = Math.Max(width, Math.Min((int)rect.Width + 20, 300));
            }

            foreach (string s in newStrings)
            {
                var yRect = font.CalculateTextRectangle(s, new Rectangle(0, 0, width, 0));
                height += (int)yRect.Height;
            }

            this.TooltipContent = newStrings;
            var hRect = headerFont.CalculateTextRectangle(this.Header, new Rectangle(0, 0, width, 0));

            this.Height = 10 + hRect.Height + 15 + height + 5;
            this.Width = width + 20;
            this._Height = this.Height;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Header == null || this.TooltipContent == null)
            {
                return;
            }

            if (this._Height == -1)
            {
                this.UpdateLayout();
            }

            // UpdateLayout();
            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var hRect = headerFont.CalculateTextRectangle(this.Header, new Rectangle(0, 0, this.Width - 20, 0));

            var rect = font.GetStringRectangle(this.Header);

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                bounds,
                bounds,
                new Color(55, 55, 55, 255),
                0f,
                default);

            spriteBatch.DrawOnCtrl(
                this,
                this.Background,
                bounds,
                bounds,
                Color.White,
                0f,
                default);

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

            spriteBatch.DrawStringOnCtrl(
                this,
                this.Header,
                headerFont,
                new Rectangle(10, 10, 0, (int)rect.Height),
                this.HeaderColor,
                false,
                HorizontalAlignment.Left);

            spriteBatch.DrawStringOnCtrl(
                this,
                string.Join(Environment.NewLine, this.TooltipContent),
                font,
                new Rectangle(10, 10 + (int)rect.Height + 15, this.Width - 20, this.Height - (10 + (int)rect.Height + 15)),
                this.ContentColor,
                true,
                HorizontalAlignment.Left,
                VerticalAlignment.Top);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            this.Background = null;
            Input.Mouse.MouseMoved -= this.Mouse_MouseMoved;
        }
    }

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

        public _EquipmentSlots Slot = _EquipmentSlots.Unkown;
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
            this.Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];

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
                                case _EquipmentSlots.Weapon1_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType = API.weaponType.Unkown;
                                    }

                                    break;

                                case _EquipmentSlots.Weapon2_MainHand:
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
                            case _EquipmentSlots.Weapon1_MainHand:
                            case _EquipmentSlots.Weapon2_MainHand:
                                if (!weapon.Wielded.Contains(API.weaponHand.Aquatic) && (weapon.Wielded.Contains(API.weaponHand.Mainhand) || weapon.Wielded.Contains(API.weaponHand.TwoHand) || weapon.Wielded.Contains(API.weaponHand.DualWielded)))
                                {
                                    weapons.Add(weapon.Weapon.getLocalName());
                                }

                                break;

                            case _EquipmentSlots.Weapon1_OffHand:
                            case _EquipmentSlots.Weapon2_OffHand:
                                if (weapon.Wielded.Contains(API.weaponHand.Offhand) || weapon.Wielded.Contains(API.weaponHand.DualWielded))
                                {
                                    weapons.Add(weapon.Weapon.getLocalName());
                                }

                                break;

                            case _EquipmentSlots.AquaticWeapon1:
                            case _EquipmentSlots.AquaticWeapon2:
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
            this._RuneTexture = BuildsManager.ModuleInstance.TextureManager.getEquipTexture(_EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            this.Trinkets = new List<API.TrinketItem>();
            foreach (API.TrinketItem item in BuildsManager.ModuleInstance.Data.Trinkets)
            {
                this.Trinkets.Add(item);
            }

            this.WeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_OffHand],

                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_OffHand],
            };

            this.AquaticWeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon1],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon2],
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
                        if (item.Slot == _EquipmentSlots.Weapon1_OffHand)
                        {
                            canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }

                        if (item.Slot == _EquipmentSlots.Weapon2_OffHand)
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
                    if (item.Slot == _EquipmentSlots.Weapon1_OffHand)
                    {
                        canSelect = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }

                    if (item.Slot == _EquipmentSlots.Weapon2_OffHand)
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
                                if (item.Slot == _EquipmentSlots.Weapon1_MainHand || item.Slot == _EquipmentSlots.Weapon1_OffHand)
                                {
                                    twoHanded = this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), this.Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                if (item.Slot == _EquipmentSlots.Weapon2_MainHand || item.Slot == _EquipmentSlots.Weapon2_OffHand)
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
            var texture = BuildsManager.ModuleInstance.TextureManager.getEmblem(_Emblems.QuestionMark);
            var mTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Mouse);
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
