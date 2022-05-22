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
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Input;
using System.Text.RegularExpressions;
using MonoGame.Extended.BitmapFonts;
using Blish_HUD.Controls.Extern;
using System.Threading;
using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager
{
    public class CustomTooltip : Control
    {
        private Texture2D Background;
        private int _Height;
        private object _CurrentObject;
        public object CurrentObject
        {
            get => _CurrentObject;
            set
            {
                _CurrentObject = value;
                _Height = -1;
            }
        }
        private string _Header;
        public string Header
        {
            get => _Header;
            set
            {
                _Header = value;
                _Height = -1;
            }
        }

        public Color HeaderColor = Color.Orange;
        public Color ContentColor = Color.Honeydew;
        private List<string> _Content;
        public List<string> Content
        {
            get => _Content;
            set
            {
                _Content = value;
                _Height = -1;
            }
        }
        public CustomTooltip(Container parent)
        {
            Parent = GameService.Graphics.SpriteScreen;

            Size = new Point(225, 275);
            Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];
            ZIndex = 1000;
            Visible = false;


            Input.Mouse.MouseMoved += delegate
            {
                Location = Input.Mouse.Position.Add(new Point(20, -10));
            };
        }
        void UpdateLayout()
        {
            if (Header == null || Content == null) return;

            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var ItemNameSize = headerFont.GetStringRectangle(Header);

            var width = (int)ItemNameSize.Width + 30;
            //var height = 10 + (int)ItemNameSize.Height;
            var height = 0;

            List<string> newStrings = new List<string>();
            foreach (string s in Content)
            {
                var ss = s;

                ss = Regex.Replace(ss, "<c=@reminder>", "\n\n");
                ss = Regex.Replace(ss, "<c=@abilitytype>", "");
                ss = Regex.Replace(ss, "</c>", "");
                ss = Regex.Replace(ss, "<br>", "");
                ss = ss.Replace(Environment.NewLine, "\n");
                newStrings.Add(ss);

                var rect = font.GetStringRectangle(ss);
                width = Math.Max(width, Math.Min((int) rect.Width + 20, 300));
            }

            foreach (string s in newStrings)
            {
                var yRect = font.CalculateTextRectangle(s, new Rectangle(0, 0, width, 0));
                height += (int) yRect.Height;                
            }

            Content = newStrings;
            var hRect = headerFont.CalculateTextRectangle(Header, new Rectangle(0, 0, width, 0));

            Height = 10 + hRect.Height + 15 + height + 5;
            Width = width + 20;
            _Height = Height;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Header == null || Content == null) return;
            if (_Height == -1) UpdateLayout();
           // UpdateLayout();
            var font = GameService.Content.DefaultFont14;
            var headerFont = GameService.Content.DefaultFont18;

            var hRect = headerFont.CalculateTextRectangle(Header, new Rectangle(0, 0, Width - 20, 0));

            var rect = font.GetStringRectangle(Header);

            spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    bounds,
                                    bounds,
                                    new Color(55, 55, 55, 255),
                                    0f,
                                    default);

            spriteBatch.DrawOnCtrl(this,
                                    Background,
                                    bounds,
                                    bounds,
                                    Color.White,
                                    0f,
                                    default);

            var color = Color.Black;
            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);


            spriteBatch.DrawStringOnCtrl(this,
                                   Header,
                                   headerFont,
                                   new Rectangle(10, 10, 0, (int)rect.Height),
                                   HeaderColor,
                                   false,
                                   HorizontalAlignment.Left
                                   );

            spriteBatch.DrawStringOnCtrl(this,
                                   string.Join(Environment.NewLine, Content),
                                   font,
                                   new Rectangle(10, 10 + (int)rect.Height + 15, Width - 20, Height - (10 + (int)rect.Height + 15)),
                                   ContentColor,
                                   true,
                                   HorizontalAlignment.Left,
                                   VerticalAlignment.Top
                                   );
        }
    }

    public class SelectionPopUp : Control
    {
        public class SelectionEntry
        {
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
            get => _SelectionType;
            set
            {
                if (_SelectionType != value && FilterBox != null) FilterBox.Text = "";
                _SelectionType = value;
            }
        }
        public List<SelectionEntry> List = new List<SelectionEntry>();
        public List<SelectionEntry> FilteredList = new List<SelectionEntry>();
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
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
        }
        public _EquipmentSlots Slot = _EquipmentSlots.Unkown;
        public int UpgradeIndex = 0;

        private ContentService ContentService;
        private BitmapFont Font;
        private BitmapFont HeaderFont;
        private Scrollbar Scrollbar;
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
            Background = BuildsManager.ModuleInstance.TextureManager._Backgrounds[(int)_Backgrounds.Tooltip];
            //BackgroundColor = Color.Red;
            FilterBox = new TextBox()
            {
                Parent = Parent,
                PlaceholderText = Strings.common.Search + " ...",
                Width = Width - 6,
                ZIndex = 998,
                Visible = false,
            };
            FilterBox.TextChanged += FilterBox_TextChanged;
            BuildsManager.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            ContentService = new ContentService();
            Font = ContentService.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular);
            HeaderFont = ContentService.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)18, ContentService.FontStyle.Regular);

            Input.Mouse.LeftMouseButtonPressed += delegate { OnChanged(); };

            Moved += delegate
            {
                FilterBox.Location = Location.Add(new Point(3, 4));
            };
            Resized += delegate
            {
                FilterBox.Width = Width - 6;
            };
            Hidden += delegate
            {
                FilterBox.Hide();
                Clicked = false;
            };
            Shown += delegate
            {
                FilterBox.Show();
                FilterBox.Focused = true;
                UpdateLayout();
                Clicked = false;
            };
            Disposed += delegate
            {
                FilterBox.Dispose();
            };
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
            if (List == null || List.Count == 0 || !Visible) return;

            var list = new List<SelectionEntry>(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                if (entry.Hovered)
                {
                    switch (SelectionType)
                    {
                        case selectionType.Runes:
                            var rune = (API.RuneItem)entry.Object;
                            var armor = (Armor_TemplateItem)SelectionTarget;
                            armor.Rune = rune;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Sigils:
                            var sigil = (API.SigilItem)entry.Object;
                            var weapon = (Weapon_TemplateItem)SelectionTarget;
                            weapon.Sigil = sigil;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticSigils:
                            var aquaSigil = (API.SigilItem)entry.Object;
                            var aquaWeapon = (AquaticWeapon_TemplateItem)SelectionTarget;
                            aquaWeapon.Sigils[UpgradeIndex] = aquaSigil;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Profession:
                            SelectedProfession = (API.Profession)entry.Object;
                            FilterBox.Text = null;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Stats:
                            var stat = (API.Stat)entry.Object;
                            var item = (TemplateItem)SelectionTarget;
                            item.Stat = stat;
                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.Weapons:
                            var selectedWeapon = (API.WeaponItem)entry.Object;
                            var iWeapon = (Weapon_TemplateItem)SelectionTarget;
                            iWeapon.WeaponType = selectedWeapon.WeaponType;

                            switch (Slot)
                            {
                                case _EquipmentSlots.Weapon1_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_OffHand].WeaponType = API.weaponType.Unkown;
                                    }
                                    break;

                                case _EquipmentSlots.Weapon2_MainHand:
                                    if ((int)selectedWeapon.Slot == (int)API.weaponHand.TwoHand && Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType != API.weaponType.Unkown)
                                    {
                                        Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_OffHand].WeaponType = API.weaponType.Unkown;
                                    }
                                    break;
                            }

                            this.Changed?.Invoke(this, EventArgs.Empty);
                            break;

                        case selectionType.AquaticWeapons:
                            var selectedAquaWeapon = (API.WeaponItem)entry.Object;
                            var AquaWeapon = (AquaticWeapon_TemplateItem)SelectionTarget;
                            AquaWeapon.WeaponType = selectedAquaWeapon.WeaponType;
                            this.Changed?.Invoke(this, EventArgs.Empty);
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

        class filterTag
        {
            public string text;
            public bool match;
        }
        private void UpdateLayout()
        {
            if (List == null || List.Count == 0) return;

            int i = 0;
            int size = 42;

            FilteredList = new List<SelectionEntry>();
            if ((SelectionType == selectionType.Weapons || SelectionType == selectionType.AquaticWeapons) && Template != null && Template.Build != null && Template.Build.Profession != null)
            {

                List<string> weapons = new List<string>();

                foreach (API.ProfessionWeapon weapon in Template.Build.Profession.Weapons)
                {
                    if (weapon.Specialization == 0 || Template.Build.SpecLines.Find(e => e.Specialization != null && e.Specialization.Id == weapon.Specialization) != null)
                    {
                        switch (Slot)
                        {
                            case _EquipmentSlots.Weapon1_MainHand:
                            case _EquipmentSlots.Weapon2_MainHand:
                                if (!weapon.Wielded.Contains(API.weaponHand.Aquatic) && (weapon.Wielded.Contains(API.weaponHand.Mainhand) || weapon.Wielded.Contains(API.weaponHand.TwoHand) || weapon.Wielded.Contains(API.weaponHand.DualWielded))) weapons.Add(weapon.Weapon.ToString());
                                break;

                            case _EquipmentSlots.Weapon1_OffHand:
                            case _EquipmentSlots.Weapon2_OffHand:
                                if (weapon.Wielded.Contains(API.weaponHand.Offhand) || weapon.Wielded.Contains(API.weaponHand.DualWielded)) weapons.Add(weapon.Weapon.ToString());
                                break;

                            case _EquipmentSlots.AquaticWeapon1:
                            case _EquipmentSlots.AquaticWeapon2:
                                if (weapon.Wielded.Contains(API.weaponHand.Aquatic)) weapons.Add(weapon.Weapon.ToString());
                                break;
                        }
                    }
                }

                List = List.Where(e => weapons.Contains(e.Header)).ToList();
            }

            if (FilterBox.Text != null && FilterBox.Text != "")
            {
                List<string> tags = FilterBox.Text.ToLower().Split(' ').ToList();
                var filteredTags = tags.Where(e => e.Trim().Length > 0);

                foreach (SelectionEntry entry in List)
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
                            FilteredList.Add(entry);
                            tag.match = true;
                        }

                        foreach (string s in entry.Content)
                        {
                            var lower = s.ToLower();

                            tag.match = tag.match ? tag.match : lower.Contains(tag.text);
                            if (tag.match) break;
                        }
                    }

                    if (!FilteredList.Contains(entry) && (Tags.Count == Tags.Where(e => e.match == true).ToList().Count)) FilteredList.Add(entry);
                }
            }
            else
            {
                FilteredList = new List<SelectionEntry>(List);
            }

            foreach (SelectionEntry entry in FilteredList)
            {
                entry.AbsolutBounds = new Rectangle(0, FilterBox.Height + 5 + i * (size + 5), Width, size);
                entry.TextureBounds = new Rectangle(2, FilterBox.Height + 5 + i * (size + 5), size, size);
                entry.TextBounds = new Rectangle(2 + size + 5, FilterBox.Height + i * (size + 5), size, size);
                entry.ContentBounds = new List<Rectangle>();

                int j = 0;
                int statSize = Font.LineHeight;
                if (entry.ContentTextures != null && entry.ContentTextures.Count > 0)
                {
                    foreach (AsyncTexture2D texture in entry.ContentTextures)
                    {
                        entry.ContentBounds.Add(new Rectangle(size + j * statSize, FilterBox.Height + Font.LineHeight + 12 + i * (size + 5), statSize, statSize));
                        j++;
                    }
                }

                i++;
            }

            Height = FilterBox.Height + 5 + Math.Min(10, Math.Max(FilteredList.Count, 1)) * (size + 5);
        }

        private void UpdateStates()
        {
            var list = new List<SelectionEntry>(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                entry.Hovered = entry.AbsolutBounds.Contains(RelativeMousePosition);

                if (entry.Hovered && SelectionType != selectionType.Weapons && CustomTooltip != null)
                {
                    CustomTooltip.Visible = true;
                    CustomTooltip.Header = entry.Header;
                    CustomTooltip.Content = entry.Content;
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

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

            spriteBatch.DrawOnCtrl(this,
                                    ContentService.Textures.Pixel,
                                    bounds,
                                    bounds,
                                    new Color(75, 75, 75, 255),
                                    0f,
                                    default);

            spriteBatch.DrawOnCtrl(this,
                                    Background,
                                    bounds.Add(-2, 0, 0, 0),
                                    bounds,
                                    Color.White,
                                    0f,
                                    default);


            int i = 0;
            int size = 42;
            var list = new List<SelectionEntry>(FilteredList);
            foreach (SelectionEntry entry in list)
            {
                spriteBatch.DrawOnCtrl(this,
                                        entry.Texture,
                                        entry.TextureBounds,
                                        entry.Texture.Texture.Bounds,
                                       entry.Hovered ? Color.Orange : Color.White,
                                        0f,
                                        default);

                spriteBatch.DrawStringOnCtrl(this,
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
                        spriteBatch.DrawOnCtrl(this,
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

                            //if (CurrentObject != null && CurrentObject.GetType().Name == "Trait_Control") height += (font.LineHeight * Regex.Matches(s, "<c=@reminder>").Count);
                            //ss = Regex.Replace(s, "<c=@reminder>", Environment.NewLine + Environment.NewLine);
                        }

                        ss = Regex.Replace(ss, "<c=@abilitytype>", "");
                        ss = Regex.Replace(ss, "</c>", "");
                        ss = Regex.Replace(ss, "<br>", "");
                        strings.Add(ss);
                    }

                    spriteBatch.DrawStringOnCtrl(this,
                                            string.Join("; ", strings).Replace(Environment.NewLine, ""),
                                            Font,
                                            new Rectangle(2 + size + 5, Font.LineHeight + FilterBox.Height + i * (size + 5) + Font.LineHeight -5, size, Font.LineHeight),
                                            Color.LightGray,
                                            false,
                                            HorizontalAlignment.Left,
                                            VerticalAlignment.Top);
                }
                i++;
            }

            var color = Color.Black;
            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
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
            Parent = parent;
            
            // BackgroundColor = Color.Aqua;
            _RuneTexture = BuildsManager.ModuleInstance.TextureManager.getEquipTexture(_EquipmentTextures.Rune).GetRegion(37, 37, 54, 54);

            Trinkets = new List<API.TrinketItem>();
            foreach (API.TrinketItem item in BuildsManager.ModuleInstance.Data.Trinkets)
            {
                Trinkets.Add(item);
            }

            WeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon1_OffHand],

                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_MainHand],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.Weapon2_OffHand],
            };

            AquaticWeaponSlots = new List<Texture2D>()
            {
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon1],
                BuildsManager.ModuleInstance.TextureManager._EquipSlotTextures[(int)_EquipSlotTextures.AquaticWeapon2],
            };

            Weapons = new List<API.WeaponItem>() { };
            foreach (API.WeaponItem weapon in BuildsManager.ModuleInstance.Data.Weapons) { Weapons.Add(weapon); }

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
                //Template = Template,
            };
            SelectionPopUp.Changed += delegate
            {
                OnChanged();
            };

            Disposed += delegate
            {
                CustomTooltip.Dispose();
                SelectionPopUp.Dispose();
            };

            foreach (API.RuneItem item in BuildsManager.ModuleInstance.Data.Runes)
            {
                Runes_Selection.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = item,
                    Texture = item.Icon._AsyncTexture,
                    Header = item.Name,
                    Content = item.Bonuses,
                });
            }

            foreach (API.SigilItem item in BuildsManager.ModuleInstance.Data.Sigils)
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
                    Header = item.WeaponType.ToString(),
                    Content = new List<string>() { "" },
                });
            }

            foreach (API.Stat item in BuildsManager.ModuleInstance.Data.Stats)
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
            BuildsManager.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            Shown += delegate
            {
                UpdateLayout();
            };

            ProfessionChanged();
            UpdateLayout();

            BuildsManager.ModuleInstance.Selected_Template_Changed += ModuleInstance_Selected_Template_Changed;
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            Instructions = Strings.common.GearTab_Tips.Split('\n').ToList();
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        public EventHandler Changed;
        private void OnChanged()
        {
            BuildsManager.ModuleInstance.Selected_Template.SetChanged();
        }

        private void OnGlobalClick(object sender, MouseEventArgs m)
        {
            if (!MouseOver && !SelectionPopUp.MouseOver) SelectionPopUp.Hide();
        }

        private void SetClipboard(string text)
        {
            if (text != "" && text != null)
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
            if (DateTime.Now.Subtract(SelectionPopUp.LastClick).TotalMilliseconds < 250) return;
            SelectionPopUp.Hide();

            if (Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Alt))
            {
                foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
                {
                    if (item.Hovered)
                    {
                        bool canSelect = true;
                        if (item.Slot == _EquipmentSlots.Weapon1_OffHand)
                        {
                            canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                        }
                        if (item.Slot == _EquipmentSlots.Weapon2_OffHand)
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
                var text = "";
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

                if (BuildsManager.ModuleInstance.PasteOnCopy.Value) Paste(text);
            }
        }
        public async void Paste(string text)
        {
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)
                               .ContinueWith(clipboardResult =>
                               {
                                   if (clipboardResult.IsFaulted)
                                       BuildsManager.Logger.Warn(clipboardResult.Exception, "Failed to set clipboard text to {text}!", text);
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
            if (DateTime.Now.Subtract(SelectionPopUp.LastClick).TotalMilliseconds < 250) return;
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
                    if (item.Slot == _EquipmentSlots.Weapon1_OffHand)
                    {
                        canSelect = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType == API.weaponType.Unkown || (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) != (int)API.weaponHand.TwoHand;
                    }
                    if (item.Slot == _EquipmentSlots.Weapon2_OffHand)
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
            var id = "Unkown";

            if (Template.Build.Profession != null)
            {
                _Profession = Template.Build.Profession.Id;
                id = Template.Build.Profession.Id;
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

            Armors = new List<API.ArmorItem>()
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
                    Armors[(int)armor.Slot] = armor;
                }
            }

            //SelectionPopUp.Template = Template;
        }

        public void UpdateLayout()
        {
            Point mPos = RelativeMousePosition;
            int i;
            int offset = 1;
            int size = 48;
            int statSize = (int)(size / 1.5);

            if (CustomTooltip.Visible) CustomTooltip.Visible = false;

            i = 0;
            foreach (TemplateItem item in Template.Gear.Trinkets)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                }
                i++;
            }

            i = 0;
            offset += 90;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                }
                i++;
            }

            i = 0;
            offset += 150;
            foreach (Weapon_TemplateItem item in Template.Gear.Weapons)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);
                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                }
                if (i == 1) i++;
                i++;
            }

            i = 0;
            offset += 150;
            foreach (AquaticWeapon_TemplateItem item in Template.Gear.AquaticWeapons)
            {
                if (item != null)
                {
                    item.Bounds = new Rectangle(offset, 5 + i * (size + 6), size, size);
                    item.UpgradeBounds = new Rectangle(offset + size + 8, 5 + i * (size + 6), size, size);

                    for (int j = 0; j < 2; j++)
                    {
                        item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), item.UpgradeBounds.Width / 2 - 2, item.UpgradeBounds.Height / 2 - 2);
                    }

                    item.StatBounds = new Rectangle(offset + (size - statSize), 5 + i * (size + 6) + (size - statSize), statSize, statSize);
                }

                if (i == 0) i = i + 2;
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

            if (CustomTooltip.Visible) CustomTooltip.Visible = false;

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
                            CustomTooltip.Content = new List<string>();
                            foreach (API.StatAttribute attribute in item.Stat.Attributes)
                            {
                                CustomTooltip.Content.Add("+ " + (attribute.Value + Math.Round(attribute.Multiplier * Trinkets[i].AttributeAdjustment)) + " " + attribute.Name);
                            }
                        }
                    };
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
                                CustomTooltip.Content = item.Rune.Bonuses;
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            CustomTooltip.Visible = true;
                            if (CustomTooltip.CurrentObject != item)
                            {
                                CustomTooltip.CurrentObject = item;
                                CustomTooltip.Header = item.Stat.Name;
                                CustomTooltip.Content = new List<string>();
                                foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                {
                                    CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * Armors[i].AttributeAdjustment) + " " + attribute.Name);
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
                                CustomTooltip.Content = new List<string>() { item.Sigil.Description };
                            }
                        }
                        else if (item.Hovered && item.Stat != null)
                        {
                            CustomTooltip.Visible = true;

                            if (CustomTooltip.CurrentObject != item)
                            {
                                CustomTooltip.CurrentObject = item;
                                CustomTooltip.Header = item.Stat.Name;
                                CustomTooltip.Content = new List<string>();

                                bool twoHanded = false;
                                if (item.Slot == _EquipmentSlots.Weapon1_MainHand || item.Slot == _EquipmentSlots.Weapon1_OffHand)
                                {
                                    twoHanded = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon1_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }
                                if (item.Slot == _EquipmentSlots.Weapon2_MainHand || item.Slot == _EquipmentSlots.Weapon2_OffHand)
                                {
                                    twoHanded = Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType != API.weaponType.Unkown && (int)Enum.Parse(typeof(API.weaponSlot), Template.Gear.Weapons[(int)Template._WeaponSlots.Weapon2_MainHand].WeaponType.ToString()) == (int)API.weaponHand.TwoHand;
                                }

                                var weapon = twoHanded ? Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword) : Weapons.Find(e => e.WeaponType == API.weaponType.Axe);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
                                    }
                                }
                            }
                        }
                    }
                }

                if (i == 1) i++;
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
                        item.SigilsBounds[j] = new Rectangle(item.UpgradeBounds.X, item.UpgradeBounds.Y + 1 + (item.UpgradeBounds.Height / 2 * j), item.UpgradeBounds.Width / 2 - 2, item.UpgradeBounds.Height / 2 - 2);

                        if ((!SelectionPopUp.Visible || !SelectionPopUp.MouseOver) && item.Sigils != null)
                        {
                            if (item.SigilsBounds[j].Contains(mPos) && item.Sigils.Count > j && item.Sigils[j] != null)
                            {
                                CustomTooltip.Visible = true;

                                if (CustomTooltip.CurrentObject != item.Sigils[j])
                                {
                                    CustomTooltip.CurrentObject = item.Sigils[j];
                                    CustomTooltip.Header = item.Sigils[j].Name;
                                    CustomTooltip.Content = new List<string>() { item.Sigils[j].Description };
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
                                CustomTooltip.Content = new List<string>();

                                var weapon = Weapons.Find(e => e.WeaponType == API.weaponType.Greatsword);
                                if (weapon != null)
                                {
                                    foreach (API.StatAttribute attribute in item.Stat.Attributes)
                                    {
                                        CustomTooltip.Content.Add("+ " + Math.Round(attribute.Multiplier * weapon.AttributeAdjustment) + " " + attribute.Name);
                                    }
                                }
                            }
                        }
                    }
                }

                if (i == 0) i = i + 2;
                i++;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template == null) return;
            if (Template.Build.Profession != null && _Profession != Template.Build.Profession.Id) ProfessionChanged();

            UpdateStates();

            int i;
            Color itemColor = new Color(75, 75, 75, 255);
            Color frameColor = new Color(125, 125, 125, 255);

            i = 0;
            foreach (Armor_TemplateItem item in Template.Gear.Armor)
            {
                if (item != null)
                {
                    spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                    spriteBatch.DrawOnCtrl(this,
                                            Armors.Count > i ? Armors[i].Icon._AsyncTexture.Texture : BuildsManager.ModuleInstance.TextureManager._Icons[0],
                                            item.Bounds,
                                            Armors.Count > i ? Armors[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.ModuleInstance.TextureManager._Icons[0].Bounds,
                                            itemColor,
                                            0f,
                                            default);


                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(this,
                                                item.Stat.Icon._AsyncTexture,
                                                item.StatBounds,
                                                item.Stat.Icon._AsyncTexture.Texture.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                    }


                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                                            Rectangle.Empty,
                                            item.Rune == null ? Color.Transparent : frameColor,
                                            0f,
                                            default);

                    spriteBatch.DrawOnCtrl(this,
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
                    spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        frameColor,
                                        0f,
                                        default);

                    spriteBatch.DrawOnCtrl(this,
                                            Trinkets.Count > i ? Trinkets[i].Icon._AsyncTexture.Texture : BuildsManager.ModuleInstance.TextureManager._Icons[0],
                                            item.Bounds,
                                            Trinkets.Count > i ? Trinkets[i].Icon._AsyncTexture.Texture.Bounds : BuildsManager.ModuleInstance.TextureManager._Icons[0].Bounds,
                                            itemColor,
                                            0f,
                                            default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(this,
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
                    spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        item.WeaponType != API.weaponType.Unkown ? frameColor : Color.Transparent,
                                        0f,
                                        default);

                    spriteBatch.DrawOnCtrl(this,
                                            item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture : WeaponSlots[i],
                                            item.Bounds,
                                            item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : WeaponSlots[i].Bounds,
                                            item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                                            0f,
                                            default);


                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(this,
                                                item.Stat.Icon._AsyncTexture,
                                                item.StatBounds,
                                                item.Stat.Icon._AsyncTexture.Texture.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                    }

                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            item.UpgradeBounds.Add(new Rectangle(-1, -1, 2, 2)),
                                            Rectangle.Empty,
                                            item.Sigil == null ? Color.Transparent : frameColor,
                                            0f,
                                            default);

                    spriteBatch.DrawOnCtrl(this,
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
                    spriteBatch.DrawOnCtrl(this,
                                        ContentService.Textures.Pixel,
                                        item.Bounds.Add(new Rectangle(-1, -1, 2, 2)),
                                        Rectangle.Empty,
                                        item.WeaponType != API.weaponType.Unkown ? frameColor : Color.Transparent,
                                        0f,
                                        default);

                    spriteBatch.DrawOnCtrl(this,
                                            item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture: AquaticWeaponSlots[i],
                                            item.Bounds,
                                            item.WeaponType != API.weaponType.Unkown ? Weapons[(int)item.WeaponType].Icon._AsyncTexture.Texture.Bounds : AquaticWeaponSlots[i].Bounds,
                                            item.WeaponType != API.weaponType.Unkown ? itemColor : Color.White,
                                            0f,
                                            default);

                    if (item.Stat != null)
                    {
                        spriteBatch.DrawOnCtrl(this,
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

                        spriteBatch.DrawOnCtrl(this,
                                                ContentService.Textures.Pixel,
                                                item.SigilsBounds[j].Add(new Rectangle(-1, -1, 2, 2)),
                                                Rectangle.Empty,
                                                sigil == null ? Color.Transparent : frameColor,
                                                0f,
                                                default);

                        spriteBatch.DrawOnCtrl(this,
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


            var font = GameService.Content.DefaultFont14;

            var lastTrinket = Template.Gear.Trinkets[Template.Gear.Trinkets.Count - 1];
            var texture = BuildsManager.ModuleInstance.TextureManager.getEmblem(_Emblems.QuestionMark);
            var mTexture = BuildsManager.ModuleInstance.TextureManager.getIcon(_Icons.Mouse);
            if (lastTrinket != null)
            {
                spriteBatch.DrawOnCtrl(this,
                                        texture,
                                        new Rectangle(lastTrinket.Bounds.Left, LocalBounds.Bottom - (font.LineHeight * 3), (font.LineHeight * 3), (font.LineHeight * 3)),
                                        texture.Bounds,
                                        Color.White,
                                        0f,
                                        default
                                        );

                i = 0;
                foreach (string s in Instructions)
                {
                    spriteBatch.DrawOnCtrl(this,
                                            mTexture,
                                            new Rectangle(lastTrinket.Bounds.Left + (font.LineHeight * 3), LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), font.LineHeight, font.LineHeight),
                                            mTexture.Bounds,
                                            Color.White,
                                            0f,
                                            default
                                            );

                    spriteBatch.DrawStringOnCtrl(this,
                                            s,
                                            font,
                                            new Rectangle(lastTrinket.Bounds.Left + 5 + (font.LineHeight * 4), LocalBounds.Bottom - (font.LineHeight * 3) + (i * font.LineHeight), 100, font.LineHeight),
                                            Color.White,
                                            false,
                                            HorizontalAlignment.Left
                                            );
                    i++;
                }
            }
        }
    }
}
