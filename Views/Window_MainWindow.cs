using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    public class Tab
    {
        public Panel Panel;
        public string Name;
        public Texture2D Icon;
        public Rectangle Bounds;
        public Rectangle Icon_Bounds;
        public Rectangle Text_Bounds;
        public bool Hovered;
    }

    internal class Container_TabbedPanel : Container
    {
        public Tab SelectedTab;
        public List<Tab> Tabs = new();
        private readonly Texture2D _tabBarTexture;
        public TextBox TemplateBox;
        public TextBox GearBox;
        private readonly Texture2D _copy;
        private readonly Texture2D _copyHovered;
        private int _tabSize;

        public Container_TabbedPanel()
        {
            _tabBarTexture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.TabBar_FadeIn);

            _copy = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Copy);
            _copyHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Copy_Hovered);

            TemplateBox = new TextBox()
            {
                Parent = this,
                Width = Width,
                Location = new Point(5, 45),
            };
            TemplateBox.InputFocusChanged += TemplateBox_InputFocusChanged;

            GearBox = new TextBox()
            {
                Parent = this,
                Width = Width,
                Location = new Point(5, 45 + 5 + TemplateBox.Height),
            };
            GearBox.InputFocusChanged += GearBox_InputFocusChanged;
        }

        private void GearBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                GearBox.SelectionStart = 0;
                GearBox.SelectionEnd = GearBox.Text.Length;
            }
            else
            {
                GearBox.SelectionStart = 0;
                GearBox.SelectionEnd = 0;
            }
        }

        private void TemplateBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                TemplateBox.SelectionStart = 0;
                TemplateBox.SelectionEnd = TemplateBox.Text.Length;
            }
            else
            {
                TemplateBox.SelectionStart = 0;
                TemplateBox.SelectionEnd = 0;
            }
        }

        protected override void DisposeControl()
        {
            foreach (Tab tab in Tabs)
            {
                tab.Panel?.Dispose();
            }

            TemplateBox.InputFocusChanged -= TemplateBox_InputFocusChanged;
            GearBox.InputFocusChanged -= GearBox_InputFocusChanged;

            base.DisposeControl();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < 2; i++)
            {
                Rectangle rect = new(LocalBounds.Width - TemplateBox.Height - 6, TemplateBox.LocalBounds.Y + (i * (TemplateBox.Height + 5)), TemplateBox.Height, TemplateBox.Height);

                if (rect.Contains(RelativeMousePosition))
                {
                    string text = i == 0 ? TemplateBox.Text : GearBox.Text;
                    if (text != string.Empty && text != null)
                    {
                        try
                        {
                            _ = ClipboardUtil.WindowsClipboardService.SetTextAsync(text);
                        }
                        catch (ArgumentException)
                        {
                            ScreenNotification.ShowNotification("Failed to set the clipboard text!", ScreenNotification.NotificationType.Error);
                        }
                        catch
                        {
                        }
                    }

                    return;
                }
            }

            foreach (Tab tab in Tabs)
            {
                if (tab.Hovered)
                {
                    SelectedTab?.Panel?.Hide();

                    SelectedTab = tab;
                    SelectedTab.Panel?.Show();
                    return;
                }
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            TemplateBox.Width = Width - 10 - TemplateBox.Height - 3;
            GearBox.Width = Width - 10 - GearBox.Height - 3;

            foreach (Tab tab in Tabs)
            {
                if (tab.Panel != null)
                {
                    tab.Panel.Size = new Point(Width, Height - 50);
                }
            }
        }

        private void UpdateLayout()
        {
            int i = 0;
            _tabSize = Width / Math.Max(1, Tabs.Count);

            foreach (Tab tab in Tabs)
            {
                if (tab.Panel != null)
                {
                    tab.Panel.Size = new Point(Width, Height - 60);
                }

                if (tab.Panel?.Location.Y < GearBox.LocalBounds.Bottom + 5)
                {
                    tab.Panel.Location = new Point(tab.Panel.Location.X, GearBox.LocalBounds.Bottom + 5);
                }

                if (tab.Panel?.Location.X < 5)
                {
                    tab.Panel.Location = new Point(5, tab.Panel.Location.Y);
                }

                tab.Bounds = new Rectangle(i * _tabSize, 0, _tabSize, 40);

                if (tab.Icon != null)
                {
                    tab.Icon_Bounds = new Rectangle((i * _tabSize) + 5, 5, 30, 30);
                    tab.Text_Bounds = new Rectangle((i * _tabSize) + 45, 0, _tabSize - 50, 40);
                }
                else
                {
                    tab.Text_Bounds = new Rectangle(i * _tabSize, 0, _tabSize, 40);
                }

                tab.Hovered = tab.Bounds.Contains(RelativeMousePosition);
                i++;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            UpdateLayout();

            BitmapFont font = GameService.Content.DefaultFont16;

            Color color;
            Rectangle rect;

            foreach (Tab tab in Tabs)
            {
                Color color2 = SelectedTab == tab ? new Color(30, 30, 30, 10) : tab.Hovered ? new Color(0, 0, 0, 50) : Color.Transparent;
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, tab.Bounds, tab.Bounds, color2);

                spriteBatch.DrawOnCtrl(
                    this,
                    _tabBarTexture,
                    tab.Bounds,
                    _tabBarTexture.Bounds,
                    Color.White,
                    0f,
                    default);

                if (tab.Icon != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        tab.Icon,
                        tab.Icon_Bounds,
                        tab.Icon.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                spriteBatch.DrawStringOnCtrl(
                    this,
                    tab.Name,
                    font,
                    tab.Text_Bounds,
                    SelectedTab == tab ? Color.White : Color.LightGray,
                    false,
                    HorizontalAlignment.Left);

                color = Color.Black;
                rect = tab.Bounds;

                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);
            }

            for (int i = 0; i < 2; i++)
            {
                rect = new Rectangle(bounds.Width - TemplateBox.Height - 6, TemplateBox.LocalBounds.Y + (i * (TemplateBox.Height + 5)), TemplateBox.Height, TemplateBox.Height);
                bool hovered = rect.Contains(RelativeMousePosition);
                spriteBatch.DrawOnCtrl(
                    this,
                    hovered ? _copyHovered : _copy,
                    rect,
                    _copy.Bounds,
                    Color.White,
                    0f,
                    default);
                BasicTooltipText = hovered ? Strings.common.Copy + " " + Strings.common.Template : null;

                color = Color.Black;
                rect = bounds;

                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);
            }
        }
    }

    public class Window_MainWindow : StandardWindow
    {
        private readonly Panel _templatesPanel;
        private readonly Container_TabbedPanel _detailPanel;
        private readonly Control_Equipment _gear;
        private readonly Control_Build _build;
        private readonly ControlTemplateSelection _templateSelection;
        private readonly Texture2D _emptyTraitLine;
        private readonly Texture2D _delete;
        private readonly Texture2D _deleteHovered;
        private readonly Texture2D _disclaimerBackground;
        private readonly SelectionPopUp _professionSelection;
        private readonly TextBox _nameBox;
        private readonly Label _nameLabel;
        private readonly Control_AddButton _addButton;
        private readonly BitmapFont _font;

        public readonly Control_AddButton ImportButton;
        public ControlTemplateSelection TemplateSelection;

        private List<SelectionPopUp.SelectionEntry> _professions;

        public Window_MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion)
            : base(background, windowRegion, contentRegion)
        {
            _emptyTraitLine = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _delete = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Delete);
            _deleteHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Delete_Hovered);

            _disclaimerBackground = BuildsManager.s_moduleInstance.TextureManager._Controls[(int)ControlTexture.PlaceHolder_Traitline].GetRegion(0, 0, 647, 136);

            _font = GameService.Content.DefaultFont18;

            _templatesPanel = new Panel()
            {
                Parent = this,
                Location = new Point(0, 0),
                Size = new Point(260, ContentRegion.Height),
                BackgroundColor = new Color(0, 0, 0, 50),
            };

            _detailPanel = new Container_TabbedPanel()
            {
                Parent = this,
                Location = new Point(_templatesPanel.LocalBounds.Right, 40),
                Size = new Point(ContentRegion.Width - _templatesPanel.LocalBounds.Right, ContentRegion.Height - 45),

                // BackgroundColor = Color.Aqua,
            };

            _professions = new List<SelectionPopUp.SelectionEntry>();
            _professionSelection = new SelectionPopUp(this)
            {
            };

            foreach (API.Profession profession in BuildsManager.s_moduleInstance.Data.Professions)
            {
                _professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig._AsyncTexture.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<AsyncTexture2D>(),
                });
            }

            _professionSelection.List = _professions;
            _professionSelection.Changed += ProfessionSelection_Changed;

            ImportButton = new Control_AddButton()
            {
                Texture = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Import),
                TextureHovered = BuildsManager.s_moduleInstance.TextureManager.getControlTexture(ControlTexture.Import_Hovered),
                Parent = _templatesPanel,
                Text = string.Empty,
                Location = new Point(_templatesPanel.Width - 130 - 40, 0),
                Size = new Point(35, 35),
                BasicTooltipText = string.Format("Import 'BuildPad' builds from '{0}config.ini'", BuildsManager.s_moduleInstance.Paths.builds),
                Visible = false,
            };
            ImportButton.Click += Import_Button_Click;

            _addButton = new Control_AddButton()
            {
                Parent = _templatesPanel,
                Text = Strings.common.Create,
                Location = new Point(_templatesPanel.Width - 130, 0),
                Size = new Point(125, 35),
            };
            _addButton.Click += Button_Click;
            BuildsManager.s_moduleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            _templateSelection = new ControlTemplateSelection(this)
            {
                Location = new Point(5, 40),
                Parent = _templatesPanel,
            };
            _templateSelection.TemplateChanged += TemplateSelection_TemplateChanged;

            _detailPanel.Tabs = new List<Tab>()
            {
                new Tab()
                {
                    Name = Strings.common.Build,
                    Icon = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Template),
                    Panel = new Panel() { Parent = _detailPanel, Visible = true },
                },
                new Tab()
                {
                    Name = Strings.common.Gear,
                    Icon = BuildsManager.s_moduleInstance.TextureManager.getIcon(Icons.Helmet),
                    Panel = new Panel() { Parent = _detailPanel, Visible = false },
                },
            };

            _detailPanel.SelectedTab = _detailPanel.Tabs[0];
            _detailPanel.Tabs[0].Panel.Resized += Panel_Resized;

            _build = new Control_Build(_detailPanel.Tabs[0].Panel)
            {
                Parent = _detailPanel.Tabs[0].Panel,
                Size = _detailPanel.Tabs[0].Panel.Size,
                Scale = 1,
            };

            _gear = new Control_Equipment(_detailPanel.Tabs[1].Panel)
            {
                Parent = _detailPanel.Tabs[1].Panel,
                Size = _detailPanel.Tabs[1].Panel.Size,
                Scale = 1,
            };

            _detailPanel.TemplateBox.Text = BuildsManager.s_moduleInstance.Selected_Template?.Build.ParseBuildCode();
            _detailPanel.GearBox.Text = BuildsManager.s_moduleInstance.Selected_Template?.Gear.TemplateCode;

            _nameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(_detailPanel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = _detailPanel.Width - 38 - 32 - 5,
                Font = _font,
                Visible = false,
            };
            _nameBox.EnterPressed += NameBox_TextChanged;

            _nameLabel = new Label()
            {
                Text = "A Template Name",
                Parent = this,
                Location = new Point(_detailPanel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = _detailPanel.Width - 38 - 32 - 5,
                Font = _font,
            };
            _nameLabel.Click += NameLabel_Click;

            BuildsManager.s_moduleInstance.Selected_Template_Edit += Selected_Template_Edit;
            BuildsManager.s_moduleInstance.Selected_Template_Changed += ModuleInstance_Selected_Template_Changed;
            BuildsManager.s_moduleInstance.Templates_Loaded += Templates_Loaded;
            BuildsManager.s_moduleInstance.Selected_Template_Redraw += Selected_Template_Redraw;

            _detailPanel.TemplateBox.EnterPressed += TemplateBox_EnterPressed;
            _detailPanel.GearBox.EnterPressed += GearBox_EnterPressed;

            Input.Mouse.LeftMouseButtonPressed += GlobalClick;

            GameService.Gw2Mumble.PlayerCharacter.NameChanged += PlayerCharacter_NameChanged;
        }

        public void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> e)
        {
            if (BuildsManager.s_moduleInstance.ShowCurrentProfession.Value)
            {
                _templateSelection.SetSelection();
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            using Template template = BuildsManager.s_moduleInstance.Selected_Template;

            if (false && template != null && template.Profession != null && template.Profession.Id == "Revenant")
            {
                Texture2D texture = _disclaimerBackground;
                Rectangle rect = new(_detailPanel.LocalBounds.X + 5, _detailPanel.LocalBounds.Y + (_detailPanel.LocalBounds.Height / 2) - 50, _detailPanel.LocalBounds.Width - 10, _font.LineHeight + 100);
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    rect,
                    texture.Bounds,
                    new Color(0, 0, 0, 175),
                    0f,
                    default);

                Color color = Color.Black;

                // Top
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

                spriteBatch.DrawStringOnCtrl(
                    this,
                    "Revenant is currently not supported! It is in development tho and hopefully comes soon!",
                    _font,
                    new Rectangle(_detailPanel.LocalBounds.X + 10, _detailPanel.LocalBounds.Y + (_detailPanel.LocalBounds.Height / 2), _detailPanel.LocalBounds.Width, _font.LineHeight),
                    Color.Red,
                    false,
                    HorizontalAlignment.Left);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            Rectangle rect = new(_detailPanel.Location.X, 44, _detailPanel.Width - 38, 35);

            spriteBatch.DrawOnCtrl(
                this,
                _emptyTraitLine,
                rect,
                _emptyTraitLine.Bounds,
                new Color(135, 135, 135, 255),
                0f,
                default);

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 2, rect.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 2, rect.Top, 2, rect.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), Rectangle.Empty, color * 0.6f);

            rect = new Rectangle(_detailPanel.LocalBounds.Right - 35, 44, 35, 35);
            bool hovered = rect.Contains(RelativeMousePosition);

            spriteBatch.DrawOnCtrl(
                this,
                hovered ? _deleteHovered : _delete,
                rect,
                _delete.Bounds,
                Color.White,
                0f,
                default);

            BasicTooltipText = hovered ? Strings.common.Delete + " " + Strings.common.Template : null;

            if (BuildsManager.s_moduleInstance.Selected_Template.Profession != null)
            {
                Template template = BuildsManager.s_moduleInstance.Selected_Template;
                Texture2D texture = BuildsManager.s_moduleInstance.TextureManager._Icons[0];

                if (template.Specialization != null)
                {
                    texture = template.Specialization.ProfessionIconBig._AsyncTexture.Texture;
                }
                else if (template.Build.Profession != null)
                {
                    texture = template.Build.Profession.IconBig._AsyncTexture.Texture;
                }

                rect = new Rectangle(_detailPanel.Location.X + 2, 46, 30, 30);

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    rect,
                    texture.Bounds,
                    Color.White,
                    0f,
                    default);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Rectangle rect = new(_detailPanel.LocalBounds.Right - 35, 44, 35, 35);
            if (rect.Contains(RelativeMousePosition) && BuildsManager.s_moduleInstance.Selected_Template.Path != null)
            {
                BuildsManager.s_moduleInstance.Selected_Template.Delete();
                BuildsManager.s_moduleInstance.Selected_Template = new Template();
            }

            _professionSelection.Hide();
        }

        protected override void DisposeControl()
        {
            _addButton.Click -= Button_Click;
            _addButton.Dispose();

            BuildsManager.s_moduleInstance.LanguageChanged -= ModuleInstance_LanguageChanged;
            BuildsManager.s_moduleInstance.Selected_Template_Edit -= Selected_Template_Edit;
            BuildsManager.s_moduleInstance.Selected_Template_Changed -= ModuleInstance_Selected_Template_Changed;
            BuildsManager.s_moduleInstance.Templates_Loaded -= Templates_Loaded;
            BuildsManager.s_moduleInstance.Selected_Template_Redraw -= Selected_Template_Redraw;

            _detailPanel.Tabs[0].Panel.Resized -= Panel_Resized;
            _detailPanel.TemplateBox.EnterPressed -= TemplateBox_EnterPressed;
            _detailPanel.GearBox.EnterPressed -= GearBox_EnterPressed;

            Input.Mouse.LeftMouseButtonPressed -= GlobalClick;

            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= PlayerCharacter_NameChanged;

            _templateSelection.TemplateChanged -= TemplateSelection_TemplateChanged;
            _templateSelection.Dispose();

            _nameBox.EnterPressed -= NameBox_TextChanged;
            _nameBox.Dispose();

            _nameLabel.Click -= NameLabel_Click;
            _nameLabel.Dispose();

            ImportButton.Click -= Import_Button_Click;
            ImportButton.Dispose();

            _professionSelection.Changed -= ProfessionSelection_Changed;
            _professionSelection.Dispose();

            base.DisposeControl();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (BuildsManager.s_moduleInstance.ShowCurrentProfession.Value)
            {
                _templateSelection.SetSelection();
            }
        }

        private void TemplateSelection_TemplateChanged(object sender, TemplateChangedEvent e)
        {
            BuildsManager.s_moduleInstance.Selected_Template = e.Template;
            _detailPanel.TemplateBox.Text = BuildsManager.s_moduleInstance.Selected_Template?.Build.TemplateCode;
            _nameLabel.Text = e.Template.Name;

            _nameLabel.Show();
            _nameBox.Hide();
        }

        private void Panel_Resized(object sender, ResizedEventArgs e)
        {
            _gear.Size = _detailPanel.Tabs[0].Panel.Size.Add(new Point(0, -_detailPanel.GearBox.Bottom + 30));
        }

        private void ProfessionSelection_Changed(object sender, EventArgs e)
        {
            if (_professionSelection.SelectedProfession != null)
            {
                Template template = new()
                {
                    Profession = _professionSelection.SelectedProfession
                };
                template.Build.Profession = _professionSelection.SelectedProfession;
                BuildsManager.s_moduleInstance.Selected_Template = template;

                BuildsManager.s_moduleInstance.Templates.Add(BuildsManager.s_moduleInstance.Selected_Template);
                BuildsManager.s_moduleInstance.Selected_Template.SetChanged();

                _templateSelection.RefreshList();
                _professionSelection.SelectedProfession = null;
            }
        }

        private void Import_Button_Click(object sender, MouseEventArgs e)
        {
            BuildsManager.s_moduleInstance.ImportTemplates();
            _templateSelection.Refresh();
            ImportButton.Hide();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            _addButton.Text = Strings.common.Create;
            ImportButton.Text = Strings.common.Create;
            _detailPanel.Tabs[0].Name = Strings.common.Build;
            _detailPanel.Tabs[1].Name = Strings.common.Gear;
        }

        private void GlobalClick(object sender, MouseEventArgs e)
        {
            if (!_nameBox.MouseOver)
            {
                _nameBox.Visible = false;
                _nameLabel.Visible = true;
            }
        }

        private void Selected_Template_Redraw(object sender, EventArgs e)
        {
            _build.SkillBar.ApplyBuild(sender, e);
            _gear.UpdateLayout();
        }

        private void GearBox_EnterPressed(object sender, EventArgs e)
        {
            string code = _detailPanel.GearBox.Text;
            GearTemplate gear = new(code);

            if (gear != null)
            {
                BuildsManager.s_moduleInstance.Selected_Template.Gear = gear;
                BuildsManager.s_moduleInstance.Selected_Template.SetChanged();
                BuildsManager.s_moduleInstance.OnSelected_Template_Redraw(null, null);
            }
        }

        private void TemplateBox_EnterPressed(object sender, EventArgs e)
        {
            string code = _detailPanel.TemplateBox.Text;
            using BuildTemplate build = new(code);

            if (build != null && build.Profession != null)
            {
                BuildsManager.s_moduleInstance.Selected_Template.Build = build;
                BuildsManager.s_moduleInstance.Selected_Template.Profession = build.Profession;

                foreach (SpecLine spec in BuildsManager.s_moduleInstance.Selected_Template.Build.SpecLines)
                {
                    if (spec.Specialization?.Elite == true)
                    {
                        BuildsManager.s_moduleInstance.Selected_Template.Specialization = spec.Specialization;
                        break;
                    }
                }

                BuildsManager.s_moduleInstance.Selected_Template.SetChanged();
                BuildsManager.s_moduleInstance.OnSelected_Template_Redraw(null, null);
            }
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            BuildsManager.s_moduleInstance.Selected_Template.Name = _nameBox.Text;
            BuildsManager.s_moduleInstance.Selected_Template.Save();

            _nameLabel.Visible = true;
            _nameBox.Visible = false;
            _nameLabel.Text = BuildsManager.s_moduleInstance.Selected_Template.Name;
            _templateSelection.RefreshList();
        }

        private void NameLabel_Click(object sender, MouseEventArgs e)
        {
            if (BuildsManager.s_moduleInstance.Selected_Template?.Path != null)
            {
                _nameLabel.Visible = false;
                _nameBox.Visible = true;
                _nameBox.Text = _nameLabel.Text;
                _nameBox.SelectionStart = 0;
                _nameBox.SelectionEnd = _nameBox.Text.Length;
                _nameBox.Focused = true;
            }
        }

        private void Button_Click(object sender, MouseEventArgs e)
        {
            _professions = new List<SelectionPopUp.SelectionEntry>();
            foreach (API.Profession profession in BuildsManager.s_moduleInstance.Data.Professions)
            {
                _professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig._AsyncTexture.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<AsyncTexture2D>(),
                });
            }

            _professionSelection.List = _professions;

            _professionSelection.Show();
            _professionSelection.Location = _addButton.Location.Add(new Point(_addButton.Width + 5, 0));
            _professionSelection.SelectionType = SelectionPopUp.selectionType.Profession;
            _professionSelection.SelectionTarget = BuildsManager.s_moduleInstance.Selected_Template.Profession;
            _professionSelection.Width = 175;
            _professionSelection.SelectionTarget = null;
            _professionSelection.List = _professions;
        }

        private void Templates_Loaded(object sender, EventArgs e)
        {
            _templateSelection.Invalidate();
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            _nameLabel.Text = BuildsManager.s_moduleInstance.Selected_Template.Name;
            _detailPanel.TemplateBox.Text = BuildsManager.s_moduleInstance.Selected_Template.Build.TemplateCode;
            _detailPanel.GearBox.Text = BuildsManager.s_moduleInstance.Selected_Template.Gear.TemplateCode;
        }

        private void Selected_Template_Edit(object sender, EventArgs e)
        {
            BuildsManager.s_moduleInstance.Selected_Template.Specialization = null;

            foreach (SpecLine spec in BuildsManager.s_moduleInstance.Selected_Template.Build.SpecLines)
            {
                if (spec.Specialization?.Elite == true)
                {
                    BuildsManager.s_moduleInstance.Selected_Template.Specialization = spec.Specialization;
                    break;
                }
            }

            _detailPanel.TemplateBox.Text = BuildsManager.s_moduleInstance.Selected_Template.Build.ParseBuildCode();
            _detailPanel.GearBox.Text = BuildsManager.s_moduleInstance.Selected_Template?.Gear.TemplateCode;
            _templateSelection.Refresh();
        }
    }
}
