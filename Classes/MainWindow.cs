using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.UI.Views;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Blish_HUD;
using Blish_HUD.Input;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Gw2Sharp.ChatLinks;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager
{
    public class Control_AddButton : Control
    {
        Texture2D Add;
        Texture2D AddHovered;
        Texture2D _EmptyTraitLine;
        Texture2D _Template_Border;
        public BitmapFont Font;

        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                _Text = value;
            }
        }

        public Control_AddButton()
        {
            //BackgroundColor = Color.Red;
            Add = BuildsManager.TextureManager.getControlTexture(_Controls.Add);
            AddHovered = BuildsManager.TextureManager.getControlTexture(_Controls.Add_Hovered);
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Template_Border = BuildsManager.TextureManager.getControlTexture(_Controls.Template_Border);

            var cnt = new ContentService();
            Font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)16, ContentService.FontStyle.Regular);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                                   ContentService.Textures.Pixel,
                                   bounds,
                                   bounds,
                                    new Color(0,0,0,125),
                                   0f,
                                   Vector2.Zero
                                   );

            var color = Color.Black;
            var rect = bounds;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            bounds = bounds.Add(1, 1, -2, -2);
            
            spriteBatch.DrawOnCtrl(this,
                                   _EmptyTraitLine,
                                   bounds.Add(2,2, -4, -6),
                                   _EmptyTraitLine.Bounds,
                                    MouseOver ? new Color(135, 135, 135, 255) : new Color(105, 105, 105, 255),
                                   0f,
                                   Vector2.Zero
                                   );

            spriteBatch.DrawOnCtrl(this,
                                   _Template_Border,
                                   bounds,
                                   _Template_Border.Bounds,
                                    MouseOver ? Color.Gray : Color.Black,
                                   0f,
                                   Vector2.Zero
                                   );

            spriteBatch.DrawOnCtrl(this,
                                   MouseOver ? AddHovered : Add,
                                   new Rectangle(6,6, Height - 12, Height- 12),
                                   MouseOver ? AddHovered.Bounds : Add.Bounds,
                                   Color.White,
                                   0f,
                                   Vector2.Zero
                                   );

            var textBounds = new Rectangle(12 + (Height - 12), 6, Width - (12 + (Height - 12)), Height - 12);
            rect = Font.CalculateTextRectangle(Text, textBounds);

            spriteBatch.DrawStringOnCtrl(this,
                                    Text,
                                    Font,
                                    textBounds,
                                    MouseOver ? Color.White : Color.LightGray,
                                    false,
                                    rect.Height > textBounds.Height ? HorizontalAlignment.Left: HorizontalAlignment.Center
                                    );

        }
    }

    public class iTab : Panel
    {
        public bool Hovered;
        public Texture2D LeftBar;
        public Texture2D RightBar;
        public Texture2D Icon;
        public string Name;
        public Rectangle TabBounds;
        public Rectangle ContentBounds;
        public Action OnActivate;
        public Action OnDeactivate;

        public iTab(Container parent)
        {
            Parent = parent;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;
            CanScroll = true;

            Icon = BuildsManager.TextureManager.getIcon(_Icons.Bug);
            LeftBar = BuildsManager.TextureManager.getControlTexture(_Controls.TabBorderLeft);
            RightBar = BuildsManager.TextureManager.getControlTexture(_Controls.TabBorderRight);

            Size = parent.ContentBounds;
        }

        public void Draw(Control tabbedControl, SpriteBatch spriteBatch, Rectangle bounds, bool selected, bool hovered)
        {
            if (this.Icon == null) return;

            spriteBatch.DrawOnCtrl(tabbedControl,
                                   Icon,
                                   new Rectangle(bounds.Right - bounds.Width / 2 - this.Icon.Width / 2,
                                                 bounds.Bottom - bounds.Height / 2 - this.Icon.Height / 2,
                                                 this.Icon.Width,
                                                 this.Icon.Height),
                                   selected || hovered
                                        ? Color.White
                                        : ContentService.Colors.DullColor);
        }
    }

    public class iMainWindow : StandardWindow
    {
        public Control_Build Build;
        public Control_Equipment Gear;
        Texture2D _TabBarTexture;
        Texture2D _TabBar_Line;

        Rectangle _TabBar_Bounds;
        Rectangle _TabBar_LineBounds;
        Rectangle _BuildSelection_Bounds;
        Point _ControlsPadding = new Point(10, 5);

        TextureManager TextureManager;

        public Label NameLabel;

        public Image ProfessionIcon;
        public Image EditName;
        public Image SaveName;
        public Image CancelName;

        public Image ResetButton;
        public TextBox NameBox;
        public TextBox TemplateBox;
        public StandardButton Reset_Button;
        public StandardButton Copy_Button;

        public iTab Gear_Tab;
        public iTab Build_Tab;

        public iTab active_Tab;
        public iTab hovered_Tab;

        public Rectangle Tab_Indicator;
        public List<iTab> iTabs = new List<iTab>();

        private Control_TemplateSelection _TemplateSelection;

        int TabBarHeight = 40;
        private Template _Template;
        public Template Template
        {
            get => BuildsManager.ModuleInstance.Selected_Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    NameBox.Text = value.Name;
                    NameLabel.Text = value.Name;
                    if (Gear != null) Gear.Template = value;

                    _Template.Changed += delegate
                    {
                        TemplateBox.Text = Template.Build.ParseBuildCode();
                        NameLabel.Text = Template.Name;
                    };
                }
            }
        }

        private Texture2D _EmptyTraitLine;
        BitmapFont Font = Content.DefaultFont18;

        public iMainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, TextureManager textureManager, Container parent, Template template) : base(background, windowRegion, contentRegion)
        {

            TextureManager = textureManager;
            Parent = parent;
            _Template = template;
            BuildsManager.ModuleInstance.Selected_Template = template;
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);

            _Template.Changed += delegate
            {
                TemplateBox.Text = Template.Build.ParseBuildCode();
                NameLabel.Text = Template.Name;
            };

            _TabBarTexture = BuildsManager.TextureManager.getControlTexture(_Controls.TabBar_FadeIn);
            _TabBar_Line = BuildsManager.TextureManager.getControlTexture(_Controls.TabBar_Line);

            Title = "Builds Manager";
            Emblem = TextureManager.getEmblem(_Emblems.SwordAndShield);
            Subtitle = "Gear";
            SavesPosition = true;
            Id = $"BuildsManager";
            ContentRegion = ContentRegion.Add(new Point(0, 50));

            _BuildSelection_Bounds = new Rectangle(ContentBounds.X, 0 + TitleBarBounds.Height, 265, 40);
            _TemplateSelection = new Control_TemplateSelection(this)
            {                 
                Location = new Point(0, 50),
            };
            _TemplateSelection.TemplateChanged += _TemplateSelection_TemplateChanged;

            _TabBar_Bounds = new Rectangle(_BuildSelection_Bounds.Right, 0 + TitleBarBounds.Height, ContentRegion.Width - _BuildSelection_Bounds.Right, 40);
            _TabBar_LineBounds = new Rectangle(_BuildSelection_Bounds.Right, 0 + TitleBarBounds.Height + _TabBar_Bounds.Height - 5, ContentRegion.Width - _BuildSelection_Bounds.Right, 10);

            Build_Tab = new iTab(this)
            {
                Icon = TextureManager.getIcon(_Icons.Template),
                Name = "Build",
                Location = new Point(_BuildSelection_Bounds.Right, 45),
            };
            iTabs.Add(Build_Tab);
            active_Tab = Build_Tab;
            Build_Tab.Resized += delegate { 
                //Build.Size = Build_Tab.Size.Add(new Point(0, -30)); 
            };

            Build = new Control_Build(Build_Tab, Template)
            {
                Parent = Build_Tab,
                Scale = 1,
                //BackgroundColor = Color.Aqua,
            };

            Gear_Tab = new iTab(this)
            {
                Icon = TextureManager.getIcon(_Icons.Helmet),
                Location = new Point(_BuildSelection_Bounds.Right, 45),
                Name = "Gear",
                Visible = false,
            };
            iTabs.Add(Gear_Tab);
            Gear_Tab.Resized += delegate { Gear.Size = Gear_Tab.Size; };

            Gear = new Control_Equipment(Gear_Tab, Template)
            {
                Parent = Gear_Tab,
                Size = Gear_Tab.Size,
                Scale = 1,
            };
            Gear.Resized += delegate
            {

                NameBox.Width = Gear.Width - 130 - (NameBox.Height * 2);
                SaveName.Location = new Point(NameBox.LocalBounds.Right + (NameBox.Height * 1), 0);
                CancelName.Location = new Point(NameBox.LocalBounds.Right, 0);
                ResetButton.Location = new Point(_BuildSelection_Bounds.Right + Gear.Width - ResetButton.Width, 0);
            };


            ProfessionIcon = new Image()
            {
                Texture = BuildsManager.TextureManager.getIcon(_Icons.Bug),
                Location = new Point(_BuildSelection_Bounds.Right + 2, 2),
                Size = new Point(Font.LineHeight + (4 * 3) - 4, Font.LineHeight + (4 * 3) - 4),
                Parent = this,
            };


            NameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(_BuildSelection_Bounds.Right + ProfessionIcon.Width + 5, 0),
                Visible = false,
                Font = Font,
                Height = Font.LineHeight + (4 * 3),
            };

            ResetButton = new Image()
            {
                BasicTooltipText = "Delete",
                Texture = BuildsManager.TextureManager.getControlTexture(_Controls.Delete),
                Size = new Point(NameBox.Height - 1, NameBox.Height - 1),
                Parent = this,
                Location = new Point(Width - 7 - NameBox.Height, 1),
        };
            ResetButton.MouseEntered += delegate
            {
                ResetButton.Texture = BuildsManager.TextureManager.getControlTexture(_Controls.Delete_Hovered);
            };
            ResetButton.MouseLeft += delegate
            {
                ResetButton.Texture = BuildsManager.TextureManager.getControlTexture(_Controls.Delete);
            };
            ResetButton.Click += delegate
            {
                Template.Reset();
                NameLabel.Text = Template.Name;
                NameBox.Text = Template.Name;
            };

            NameLabel = new Label()
            {
                Text = "This Builds Name",
                Parent = this,
                Width = Width - ResetButton.Width - 10 - (_BuildSelection_Bounds.Right + ProfessionIcon.Width + 5),
                VerticalAlignment = VerticalAlignment.Middle,
                Location = new Point(_BuildSelection_Bounds.Right + ProfessionIcon.Width + 5, 0),
                Height = Font.LineHeight + (4 * 3),
                //AutoSizeHeight = true,
                Font = Font,
            };
            NameLabel.Click += delegate
            {
                NameBox.Show();
                NameLabel.Hide();
                CancelName.Show();
                SaveName.Show();
                NameBox.Text = NameLabel.Text;
                NameBox.Width = NameLabel.Width - (NameBox.Height * 2);
                NameBox.Height = NameLabel.Height - 2;
                NameBox.Location = new Point(NameLabel.Location.X, NameLabel.Location.Y + 1);
                
                NameBox.Focused = true;
                NameBox.SelectionStart = 0;
                NameBox.SelectionEnd= NameBox.Text.Length;
                
                SaveName.Location = new Point(NameBox.LocalBounds.Right + (NameBox.Height * 1), 0);
                CancelName.Location = new Point(NameBox.LocalBounds.Right, 0);
            };


            SaveName = new Image()
            {
                Texture = BuildsManager.TextureManager.getIcon(_Icons.Checkmark_Color),
                Parent = this,
                Location = new Point(NameBox.LocalBounds.Right + (NameBox.Height * 1), 0),
                Size = new Point(NameBox.Height, NameBox.Height),
                Visible = false,
            };
            SaveName.Click += delegate
            {
                NameLabel.Text = NameBox.Text;
                Template.Name = NameBox.Text;
                Template.Save();

                NameBox.Text = "";

                NameLabel.Show();
                NameBox.Hide();
                CancelName.Hide();
                SaveName.Hide();
            };
            NameBox.EnterPressed += delegate
            {
                Template.Name = NameBox.Text;
                Template.Save();

                NameLabel.Text = NameBox.Text;
                NameBox.Text = "";

                NameLabel.Show();
                NameBox.Hide();
                CancelName.Hide();
                SaveName.Hide();
            };
            SaveName.MouseEntered += delegate
            {
                SaveName.Texture = BuildsManager.TextureManager.getIcon(_Icons.Checkmark_Highlight);

            };
            SaveName.MouseLeft += delegate
            {
                SaveName.Texture = BuildsManager.TextureManager.getIcon(_Icons.Checkmark_Color);

            };

            CancelName = new Image()
            {
                Texture = BuildsManager.TextureManager.getIcon(_Icons.Stop_Color),
                Parent = this,
                Location = new Point(NameBox.LocalBounds.Right, 0),
                Size = new Point(NameBox.Height, NameBox.Height),
                Visible = false,
            };
            CancelName.Click += delegate
            {
                NameBox.Text = "";

                NameLabel.Show();
                NameBox.Hide();
                CancelName.Hide();
                SaveName.Hide();
            };
            CancelName.MouseEntered += delegate
            {
                CancelName.Texture = BuildsManager.TextureManager.getIcon(_Icons.Stop_Highlight);
            };
            CancelName.MouseLeft += delegate
            {
                CancelName.Texture = BuildsManager.TextureManager.getIcon(_Icons.Stop_Color);
            };

            TemplateBox = new TextBox()
            {
                Parent = Build_Tab,
                Width = Build.LocalBounds.Width - 97,
                Font = GameService.Content.DefaultFont12,
                Location = new Point(Build.LocalBounds.X, Build.LocalBounds.Bottom + 5),
            };
            Build.Resized += delegate
            {
                TemplateBox.Width = Build.LocalBounds.Width - Copy_Button.Width;
                Copy_Button.Location = new Point(TemplateBox.LocalBounds.Right, TemplateBox.LocalBounds.Top);
            };
            Build.Moved += delegate
            {
                TemplateBox.Width = Build.LocalBounds.Width - Copy_Button.Width;
                Copy_Button.Location = new Point(TemplateBox.LocalBounds.Right, TemplateBox.LocalBounds.Top);
            };
            //TemplateBox.Text = Build.BuildTemplateCode;

            TemplateBox.TextChanged += delegate
            {
                if (TemplateBox.Focused && TemplateBox.Text != null && TemplateBox.Text != "")
                {
                    //Build.BuildTemplateCode = TemplateBox.Text;
                }
            };

            Reset_Button = new StandardButton()
            {
                Parent = this,
                Width = 100,
                Location = new Point(TemplateBox.LocalBounds.Right, TemplateBox.LocalBounds.Top),
                Text = "Reset",
                Visible = false,
            };
            Reset_Button.Click += delegate
            {
                BuildChatLink build = new BuildChatLink();
                var bytes = build.ToArray();
                build.Parse(bytes);
                var player = GameService.Gw2Mumble.PlayerCharacter;
                build.Profession = player != null ? player.Profession : Gw2Sharp.Models.ProfessionType.Guardian;

            };

            Copy_Button = new StandardButton()
            {
                Parent = Build_Tab,
                Width = 100,
                Location = new Point(TemplateBox.LocalBounds.Right, TemplateBox.LocalBounds.Top),
                Text = "Copy",
                Height = TemplateBox.Height,
            };
            Copy_Button.Click += delegate
            {
                if (TemplateBox.Text != null && TemplateBox.Text != "") System.Windows.Forms.Clipboard.SetText(TemplateBox.Text);
            };

            TemplateBox.Text = Template.Build.ParseBuildCode();
            NameLabel.Text = Template.Name;

            var button = new Control_AddButton()
            {
                Parent = this,
                Text = "Create",
                Location = new Point(0,0),
                Size = new Point(125,30),
            };
        }

        private bool registered;

        private void _TemplateSelection_TemplateChanged(object sender, TemplateChangedEvent e)
        {
            BuildsManager.ModuleInstance.Selected_Template = e.Template;
            NameLabel.Text = e.Template.Name;
            TemplateBox.Text = e.Template.Build.TemplateCode;
            //Template = e.Template;
            //Build.Template = e.Template;
            //Gear.Template = e.Template;
        }

        private void UpdateTabStates()
        {

            var i = 0;
            var width = _TabBar_Bounds.Width / iTabs.Count;
            foreach (iTab tab in this.iTabs)
            {
                tab.TabBounds = new Rectangle(i * width, _TabBar_Bounds.Top, width, _TabBar_Bounds.Height).Add(new Point(_TabBar_Bounds.Left, 0));
                tab.Hovered = tab.TabBounds.Contains(RelativeMousePosition);
                if (tab.Hovered) BasicTooltipText = tab?.Name;

                i++;
            }
        }
        protected override void OnClick(MouseEventArgs e)
        {
            foreach (iTab tab in this.iTabs)
            {
                if (tab.Hovered && tab != active_Tab)
                {
                    active_Tab.Hide();
                    if (active_Tab.OnDeactivate != null) active_Tab.OnDeactivate();

                    active_Tab = tab;
                    active_Tab.Show();
                    if (active_Tab.OnActivate != null) active_Tab.OnActivate();
                }
            }

            base.OnClick(e);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            var texture = ProfessionIcon.Texture;

            if(Template.Specialization != null)
            {
                ProfessionIcon.Texture = Template.Specialization.ProfessionIconBig.Texture;
            }
            else if ( Template.Build.Profession != null)
            {
                ProfessionIcon.Texture = Template.Build.Profession.IconBig.Texture;
            }

            UpdateTabStates();

            base.UpdateContainer(gameTime);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {         
            base.PaintBeforeChildren(spriteBatch, bounds);

            var rect = new Rectangle(ProfessionIcon.Location.X - 2, 85, NameLabel.Width + ProfessionIcon.Width + 5, Font.LineHeight + (4 * 3));
            spriteBatch.DrawOnCtrl(this,
                                   _EmptyTraitLine,
                                   rect,
                                  _EmptyTraitLine.Bounds,
                                    new Color(135, 135, 135, 255),
                                  0f,
                                  default);

            var color = Color.Black;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.DrawOnCtrl(this,
                                   _TabBarTexture,
                                   _TabBar_Bounds,
                                  _TabBarTexture.Bounds,
                                  Color.White,
                                  0f,
                                  default);

            spriteBatch.DrawOnCtrl(this,
                                   _TabBar_Line,
                                   _TabBar_LineBounds,
                                  _TabBar_Line.Bounds,
                                  Color.White,
                                  0f,
                                  default);

            var i = 0;
            var width = _TabBar_Bounds.Width / iTabs.Count;

            foreach (iTab tab in this.iTabs)
            {
                tab.TabBounds = new Rectangle(i * width, _TabBar_Bounds.Top, width, _TabBar_Bounds.Height).Add(new Point(_TabBar_Bounds.Left, 0));
                var leftLine = new Rectangle(tab.TabBounds.X, tab.TabBounds.Y, tab.LeftBar.Width, tab.TabBounds.Height);
                var rightLine = new Rectangle(tab.TabBounds.Right - tab.RightBar.Width, tab.TabBounds.Y, tab.RightBar.Width, tab.TabBounds.Height);
                var icon = tab.Icon.Bounds.Add(new Point(i * width + _TabBar_Bounds.Left, _TabBar_Bounds.Top + ((tab.TabBounds.Height - tab.Icon.Bounds.Height) / 2)));
                var text = new Rectangle(icon.Right + 5, tab.TabBounds.Top, tab.TabBounds.Width - icon.Width, tab.TabBounds.Height);

                var cnt = new ContentService();
                var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)ContentService.FontSize.Size16, ContentService.FontStyle.Regular);

                spriteBatch.DrawOnCtrl(this,
                                      tab.LeftBar,
                                      leftLine,
                                      tab.LeftBar.Bounds,
                                      Color.White,
                                      0f,
                                      default);


                bool selected = tab == active_Tab;
                bool hovered = tab == hovered_Tab;

                if (selected)
                {
                    spriteBatch.DrawOnCtrl(this,
                                       this.WindowBackground,
                                       tab.TabBounds,
                                       tab.TabBounds,
                                      Color.Gray
                                       );
                }

                spriteBatch.DrawOnCtrl(this,
                                      tab.Icon,
                                      icon,
                                      tab.Icon.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                spriteBatch.DrawStringOnCtrl(this,
                                       tab.Name,
                                       font,
                                       text,
                                       selected ? Color.White : Color.LightGray
                                       );

                spriteBatch.DrawOnCtrl(this,
                                      tab.RightBar,
                                      rightLine,
                                      tab.RightBar.Bounds,
                                      Color.White,
                                      0f,
                                      default);
                i++;
            }

            base.PaintAfterChildren(spriteBatch, bounds);
            //if(active_Tab == Gear_Tab) Gear.PaintAfterChildren(spriteBatch);
            //if (active_Tab == Build_Tab) Build.PaintAfterChildren(spriteBatch);


        }
    }
}
