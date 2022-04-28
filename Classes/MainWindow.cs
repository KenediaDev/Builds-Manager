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

namespace Kenedia.Modules.BuildsManager
{
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

        int TabBarHeight = 40;
        private Template _Template;
        public Template Template
        {
            get => _Template;
            set
            {
                if (value != null)
                {
                    _Template = value;
                    NameBox.Text = value.Name;
                    NameLabel.Text = value.Name;
                    if (Gear != null) Gear.Template = value;
                    if (Build != null) Build.Template = value;
                }
            }
        }

        public iMainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, TextureManager textureManager, Container parent, Template template) : base(background, windowRegion, contentRegion)
        {

            TextureManager = textureManager;
            Parent = parent;
            _Template = template;

            _TabBarTexture = BuildsManager.TextureManager.getControlTexture(_Controls.TabBar_FadeIn);
            _TabBar_Line = BuildsManager.TextureManager.getControlTexture(_Controls.TabBar_Line);

            Title = "Builds Manager";
            Emblem = TextureManager.getEmblem(_Emblems.SwordAndShield);
            Subtitle = "Gear";
            SavesPosition = true;
            Id = $"BuildsManager";
            ContentRegion = ContentRegion.Add(new Point(0, 50));

            _BuildSelection_Bounds = new Rectangle(ContentBounds.X, 0 + TitleBarBounds.Height, 150, 40);

            _TabBar_Bounds = new Rectangle(_BuildSelection_Bounds.Right, 0 + TitleBarBounds.Height, ContentRegion.Width - _BuildSelection_Bounds.Right, 40);
            _TabBar_LineBounds = new Rectangle(_BuildSelection_Bounds.Right, 0 + TitleBarBounds.Height + _TabBar_Bounds.Height - 5, ContentRegion.Width - _BuildSelection_Bounds.Right, 10);

            Build_Tab = new iTab(this)
            {
                Icon = TextureManager.getIcon(_Icons.Template),
                Name = "Build",
                Location = new Point(_BuildSelection_Bounds.Right, 65),
            };
            iTabs.Add(Build_Tab);
            active_Tab = Build_Tab;
            Build_Tab.Resized += delegate { Build.Size = Build_Tab.Size.Add(new Point(0, -30)); };

            Build = new Control_Build(Build_Tab, Template)
            {
                Parent = Build_Tab,
                Location = new Point(0, 30),
                Size = Build_Tab.Size.Add(new Point(0, -30)),
                Scale = 0.93,
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

            var font = Content.DefaultFont18;

            ProfessionIcon = new Image()
            {
                Texture = BuildsManager.TextureManager.getIcon(_Icons.Bug),
                Location = new Point(_BuildSelection_Bounds.Right, 0),
                Size = new Point(font.LineHeight + (4 * 3), font.LineHeight + (4 * 3)),
                Parent = this,
            };


            NameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(_BuildSelection_Bounds.Right + ProfessionIcon.Width + 5, 0),
                Visible = false,
                Font = font,
                Height = font.LineHeight + (4 * 3),
            };

            ResetButton = new Image()
            {
                BasicTooltipText = "Reset",
                Texture = BuildsManager.TextureManager.getControlTexture(_Controls.ResetButton),
                Size = new Point(NameBox.Height , NameBox.Height),
                Parent = this,
                Location = new Point(Width - 5 - NameBox.Height, 0),
        };
            ResetButton.MouseEntered += delegate
            {
                ResetButton.Texture = BuildsManager.TextureManager.getControlTexture(_Controls.ResetButton_Hovered);
            };
            ResetButton.MouseLeft += delegate
            {
                ResetButton.Texture = BuildsManager.TextureManager.getControlTexture(_Controls.ResetButton);
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
                Width = NameBox.Width,
                VerticalAlignment = VerticalAlignment.Middle,
                Location = new Point(_BuildSelection_Bounds.Right + ProfessionIcon.Width + 5, 0),
                Height = font.LineHeight + (4 * 3),
                //AutoSizeHeight = true,
                Font = font,
            };
            NameLabel.Click += delegate
            {
                NameBox.Show();
                NameLabel.Hide();
                CancelName.Show();
                SaveName.Show();
                NameBox.Text = NameLabel.Text;
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
                Width = Build.LocalBounds.Width - 35 - 100,
                Font = GameService.Content.DefaultFont12,
                Location = new Point(Build.LocalBounds.X, Build.LocalBounds.Bottom + 10 ),
            };
            Build.Resized += delegate
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
            else
            {
                ProfessionIcon.Texture = Template.Profession.IconBig.Texture;
            }

            UpdateTabStates();

            base.UpdateContainer(gameTime);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {

            base.PaintBeforeChildren(spriteBatch, bounds);
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
