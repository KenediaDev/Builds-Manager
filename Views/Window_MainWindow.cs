namespace Kenedia.Modules.BuildsManager
{
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
        public List<Tab> Tabs = new List<Tab>();
        private Texture2D _TabBarTexture;
        public TextBox TemplateBox;
        public TextBox GearBox;
        private Texture2D _Copy;
        private Texture2D _CopyHovered;
        private int TabSize;

        public Container_TabbedPanel()
        {
            this._TabBarTexture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.TabBar_FadeIn);

            this._Copy = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Copy);
            this._CopyHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Copy_Hovered);

            this.TemplateBox = new TextBox()
            {
                Parent = this,
                Width = this.Width,
                Location = new Point(5, 45),
            };
            this.TemplateBox.InputFocusChanged += this.TemplateBox_InputFocusChanged;

            this.GearBox = new TextBox()
            {
                Parent = this,
                Width = this.Width,
                Location = new Point(5, 45 + 5 + this.TemplateBox.Height),
            };
            this.GearBox.InputFocusChanged += this.GearBox_InputFocusChanged;
        }

        private void GearBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                this.GearBox.SelectionStart = 0;
                this.GearBox.SelectionEnd = this.GearBox.Text.Length;
            }
            else
            {
                this.GearBox.SelectionStart = 0;
                this.GearBox.SelectionEnd = 0;
            }
        }

        private void TemplateBox_InputFocusChanged(object sender, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                this.TemplateBox.SelectionStart = 0;
                this.TemplateBox.SelectionEnd = this.TemplateBox.Text.Length;
            }
            else
            {
                this.TemplateBox.SelectionStart = 0;
                this.TemplateBox.SelectionEnd = 0;
            }
        }

        protected override void DisposeControl()
        {
            foreach (Tab tab in this.Tabs)
            {
                if (tab.Panel != null)
                {
                    tab.Panel.Dispose();
                }
            }

            this.TemplateBox.InputFocusChanged -= this.TemplateBox_InputFocusChanged;
            this.GearBox.InputFocusChanged -= this.GearBox_InputFocusChanged;

            base.DisposeControl();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < 2; i++)
            {
                var rect = new Rectangle(this.LocalBounds.Width - this.TemplateBox.Height - 6, this.TemplateBox.LocalBounds.Y + (i * (this.TemplateBox.Height + 5)), this.TemplateBox.Height, this.TemplateBox.Height);

                if (rect.Contains(this.RelativeMousePosition))
                {
                    var text = i == 0 ? this.TemplateBox.Text : this.GearBox.Text;
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

                    return;
                }
            }

            foreach (Tab tab in this.Tabs)
            {
                if (tab.Hovered)
                {
                    this.SelectedTab?.Panel?.Hide();

                    this.SelectedTab = tab;
                    this.SelectedTab.Panel?.Show();
                    return;
                }
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            this.TemplateBox.Width = this.Width - 10 - this.TemplateBox.Height - 3;
            this.GearBox.Width = this.Width - 10 - this.GearBox.Height - 3;

            foreach (Tab tab in this.Tabs)
            {
                if (tab.Panel != null)
                {
                    tab.Panel.Size = new Point(this.Width, this.Height - 50);
                }
            }
        }

        private void UpdateLayout()
        {
            int i = 0;
            this.TabSize = this.Width / Math.Max(1, this.Tabs.Count);

            foreach (Tab tab in this.Tabs)
            {
                if (tab.Panel != null)
                {
                    tab.Panel.Size = new Point(this.Width, this.Height - 60);
                }

                if (tab.Panel?.Location.Y < this.GearBox.LocalBounds.Bottom + 5)
                {
                    tab.Panel.Location = new Point(tab.Panel.Location.X, this.GearBox.LocalBounds.Bottom + 5);
                }

                if (tab.Panel?.Location.X < 5)
                {
                    tab.Panel.Location = new Point(5, tab.Panel.Location.Y);
                }

                tab.Bounds = new Rectangle(i * this.TabSize, 0, this.TabSize, 40);

                if (tab.Icon != null)
                {
                    tab.Icon_Bounds = new Rectangle((i * this.TabSize) + 5, 5, 30, 30);
                    tab.Text_Bounds = new Rectangle((i * this.TabSize) + 45, 0, this.TabSize - 50, 40);
                }
                else
                {
                    tab.Text_Bounds = new Rectangle(i * this.TabSize, 0, this.TabSize, 40);
                }

                tab.Hovered = tab.Bounds.Contains(this.RelativeMousePosition);
                i++;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            this.UpdateLayout();

            var font = GameService.Content.DefaultFont16;

            Color color;
            Rectangle rect;

            foreach (Tab tab in this.Tabs)
            {
                var color2 = this.SelectedTab == tab ? new Color(30, 30, 30, 10) : tab.Hovered ? new Color(0, 0, 0, 50) : Color.Transparent;
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, tab.Bounds, tab.Bounds, color2);

                spriteBatch.DrawOnCtrl(
                    this,
                    this._TabBarTexture,
                    tab.Bounds,
                    this._TabBarTexture.Bounds,
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
                    this.SelectedTab == tab ? Color.White : Color.LightGray,
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
                rect = new Rectangle(bounds.Width - this.TemplateBox.Height - 6, this.TemplateBox.LocalBounds.Y + (i * (this.TemplateBox.Height + 5)), this.TemplateBox.Height, this.TemplateBox.Height);
                var hovered = rect.Contains(this.RelativeMousePosition);
                spriteBatch.DrawOnCtrl(
                    this,
                    hovered ? this._CopyHovered : this._Copy,
                    rect,
                    this._Copy.Bounds,
                    Color.White,
                    0f,
                    default);
                this.BasicTooltipText = hovered ? Strings.common.Copy + " " + Strings.common.Template : null;

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
        private Panel Templates_Panel;
        private Container_TabbedPanel Detail_Panel;
        private Control_Equipment Gear;
        private Control_Build Build;
        public Control_TemplateSelection _TemplateSelection;
        private Texture2D _EmptyTraitLine;
        private Texture2D _Delete;
        private Texture2D _DeleteHovered;
        private Texture2D ProfessionIcon;
        private Texture2D Disclaimer_Background;
        private SelectionPopUp ProfessionSelection;
        private List<SelectionPopUp.SelectionEntry> _Professions;
        private TextBox NameBox;
        private Label NameLabel;
        private Control_AddButton Add_Button;
        public Control_AddButton Import_Button;

        private BitmapFont Font;

        private void _TemplateSelection_TemplateChanged(object sender, TemplateChangedEvent e)
        {
            BuildsManager.ModuleInstance.Selected_Template = e.Template;
            this.Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Build.TemplateCode;
            this.NameLabel.Text = e.Template.Name;

            this.NameLabel.Show();
            this.NameBox.Hide();
        }

        public Window_MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion)
            : base(background, windowRegion, contentRegion)
        {
            this._EmptyTraitLine = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            this._Delete = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Delete);
            this._DeleteHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Delete_Hovered);

            this.Disclaimer_Background = BuildsManager.ModuleInstance.TextureManager._Controls[(int)ControlTexture.PlaceHolder_Traitline].GetRegion(0, 0, 647, 136);

            this.Font = GameService.Content.DefaultFont18;

            this.Templates_Panel = new Panel()
            {
                Parent = this,
                Location = new Point(0, 0),
                Size = new Point(260, this.ContentRegion.Height),
                BackgroundColor = new Color(0, 0, 0, 50),
            };

            this.Detail_Panel = new Container_TabbedPanel()
            {
                Parent = this,
                Location = new Point(this.Templates_Panel.LocalBounds.Right, 40),
                Size = new Point(this.ContentRegion.Width - this.Templates_Panel.LocalBounds.Right, this.ContentRegion.Height - 45),

                // BackgroundColor = Color.Aqua,
            };

            this._Professions = new List<SelectionPopUp.SelectionEntry>();
            this.ProfessionSelection = new SelectionPopUp(this)
            {
            };

            foreach (API.Profession profession in BuildsManager.ModuleInstance.Data.Professions)
            {
                this._Professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig._AsyncTexture.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<AsyncTexture2D>(),
                });
            }

            this.ProfessionSelection.List = this._Professions;
            this.ProfessionSelection.Changed += this.ProfessionSelection_Changed;

            this.Import_Button = new Control_AddButton()
            {
                Texture = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Import),
                TextureHovered = BuildsManager.ModuleInstance.TextureManager.getControlTexture(ControlTexture.Import_Hovered),
                Parent = this.Templates_Panel,
                Text = string.Empty,
                Location = new Point(this.Templates_Panel.Width - 130 - 40, 0),
                Size = new Point(35, 35),
                BasicTooltipText = string.Format("Import 'BuildPad' builds from '{0}config.ini'", BuildsManager.ModuleInstance.Paths.builds),
                Visible = false,
            };
            this.Import_Button.Click += this.Import_Button_Click;

            this.Add_Button = new Control_AddButton()
            {
                Parent = this.Templates_Panel,
                Text = Strings.common.Create,
                Location = new Point(this.Templates_Panel.Width - 130, 0),
                Size = new Point(125, 35),
            };
            this.Add_Button.Click += this.Button_Click;
            BuildsManager.ModuleInstance.LanguageChanged += this.ModuleInstance_LanguageChanged;

            this._TemplateSelection = new Control_TemplateSelection(this)
            {
                Location = new Point(5, 40),
                Parent = this.Templates_Panel,
            };
            this._TemplateSelection.TemplateChanged += this._TemplateSelection_TemplateChanged;

            this.Detail_Panel.Tabs = new List<Tab>()
            {
                new Tab()
                {
                    Name = Strings.common.Build,
                    Icon = BuildsManager.ModuleInstance.TextureManager.getIcon(Icons.Template),
                    Panel = new Panel() { Parent = this.Detail_Panel, Visible = true },
                },
                new Tab()
                {
                    Name = Strings.common.Gear,
                    Icon = BuildsManager.ModuleInstance.TextureManager.getIcon(Icons.Helmet),
                    Panel = new Panel() { Parent = this.Detail_Panel, Visible = false },
                },
            };

            this.Detail_Panel.SelectedTab = this.Detail_Panel.Tabs[0];
            this.Detail_Panel.Tabs[0].Panel.Resized += this.Panel_Resized;

            this.Build = new Control_Build(this.Detail_Panel.Tabs[0].Panel)
            {
                Parent = this.Detail_Panel.Tabs[0].Panel,
                Size = this.Detail_Panel.Tabs[0].Panel.Size,
                Scale = 1,
            };

            this.Gear = new Control_Equipment(this.Detail_Panel.Tabs[1].Panel)
            {
                Parent = this.Detail_Panel.Tabs[1].Panel,
                Size = this.Detail_Panel.Tabs[1].Panel.Size,
                Scale = 1,
            };

            this.Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Build.ParseBuildCode();
            this.Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Gear.TemplateCode;

            this.NameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(this.Detail_Panel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = this.Detail_Panel.Width - 38 - 32 - 5,
                Font = this.Font,
                Visible = false,
            };
            this.NameBox.EnterPressed += this.NameBox_TextChanged;

            this.NameLabel = new Label()
            {
                Text = "A Template Name",
                Parent = this,
                Location = new Point(this.Detail_Panel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = this.Detail_Panel.Width - 38 - 32 - 5,
                Font = this.Font,
            };
            this.NameLabel.Click += this.NameLabel_Click;

            BuildsManager.ModuleInstance.Selected_Template_Edit += this.Selected_Template_Edit;
            BuildsManager.ModuleInstance.Selected_Template_Changed += this.ModuleInstance_Selected_Template_Changed;
            BuildsManager.ModuleInstance.Templates_Loaded += this.Templates_Loaded;
            BuildsManager.ModuleInstance.Selected_Template_Redraw += this.Selected_Template_Redraw;

            this.Detail_Panel.TemplateBox.EnterPressed += this.TemplateBox_EnterPressed;
            this.Detail_Panel.GearBox.EnterPressed += this.GearBox_EnterPressed;

            Input.Mouse.LeftMouseButtonPressed += this.GlobalClick;

            GameService.Gw2Mumble.PlayerCharacter.NameChanged += this.PlayerCharacter_NameChanged;
        }

        private void Panel_Resized(object sender, ResizedEventArgs e)
        {
            this.Gear.Size = this.Detail_Panel.Tabs[0].Panel.Size.Add(new Point(0, -this.Detail_Panel.GearBox.Bottom + 30));
        }

        private void ProfessionSelection_Changed(object sender, EventArgs e)
        {
            if (this.ProfessionSelection.SelectedProfession != null)
            {
                var template = new Template();
                template.Profession = this.ProfessionSelection.SelectedProfession;
                template.Build.Profession = this.ProfessionSelection.SelectedProfession;
                BuildsManager.ModuleInstance.Selected_Template = template;

                BuildsManager.ModuleInstance.Templates.Add(BuildsManager.ModuleInstance.Selected_Template);
                BuildsManager.ModuleInstance.Selected_Template.SetChanged();

                this._TemplateSelection.RefreshList();
                this.ProfessionSelection.SelectedProfession = null;
            }
        }

        public void PlayerCharacter_NameChanged(object sender, ValueEventArgs<string> e)
        {
            if (BuildsManager.ModuleInstance.ShowCurrentProfession.Value)
            {
                this._TemplateSelection.SetSelection();
            }
        }

        private void Import_Button_Click(object sender, MouseEventArgs e)
        {
            BuildsManager.ModuleInstance.ImportTemplates();
            this._TemplateSelection.Refresh();
            this.Import_Button.Hide();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            this.Add_Button.Text = Strings.common.Create;
            this.Import_Button.Text = Strings.common.Create;
            this.Detail_Panel.Tabs[0].Name = Strings.common.Build;
            this.Detail_Panel.Tabs[1].Name = Strings.common.Gear;
        }

        private void GlobalClick(object sender, MouseEventArgs e)
        {
            if (!this.NameBox.MouseOver)
            {
                this.NameBox.Visible = false;
                this.NameLabel.Visible = true;
            }
        }

        private void Selected_Template_Redraw(object sender, EventArgs e)
        {
            this.Build.SkillBar.ApplyBuild(sender, e);
            this.Gear.UpdateLayout();
        }

        private void GearBox_EnterPressed(object sender, EventArgs e)
        {
            var code = this.Detail_Panel.GearBox.Text;
            var gear = new GearTemplate(code);

            if (gear != null)
            {
                BuildsManager.ModuleInstance.Selected_Template.Gear = gear;
                BuildsManager.ModuleInstance.Selected_Template.SetChanged();
                BuildsManager.ModuleInstance.OnSelected_Template_Redraw(null, null);
            }
        }

        private void TemplateBox_EnterPressed(object sender, EventArgs e)
        {
            var code = this.Detail_Panel.TemplateBox.Text;
            var build = new BuildTemplate(code);

            if (build != null && build.Profession != null)
            {
                BuildsManager.ModuleInstance.Selected_Template.Build = build;
                BuildsManager.ModuleInstance.Selected_Template.Profession = build.Profession;

                foreach (SpecLine spec in BuildsManager.ModuleInstance.Selected_Template.Build.SpecLines)
                {
                    if (spec.Specialization?.Elite == true)
                    {
                        BuildsManager.ModuleInstance.Selected_Template.Specialization = spec.Specialization;
                        break;
                    }
                }

                BuildsManager.ModuleInstance.Selected_Template.SetChanged();
                BuildsManager.ModuleInstance.OnSelected_Template_Redraw(null, null);
            }
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            BuildsManager.ModuleInstance.Selected_Template.Name = this.NameBox.Text;
            BuildsManager.ModuleInstance.Selected_Template.Save();

            this.NameLabel.Visible = true;
            this.NameBox.Visible = false;
            this.NameLabel.Text = BuildsManager.ModuleInstance.Selected_Template.Name;
            this._TemplateSelection.RefreshList();
        }

        private void NameLabel_Click(object sender, MouseEventArgs e)
        {
            if (BuildsManager.ModuleInstance.Selected_Template?.Path != null)
            {
                this.NameLabel.Visible = false;
                this.NameBox.Visible = true;
                this.NameBox.Text = this.NameLabel.Text;
                this.NameBox.SelectionStart = 0;
                this.NameBox.SelectionEnd = this.NameBox.Text.Length;
                this.NameBox.Focused = true;
            }
        }

        private void Button_Click(object sender, MouseEventArgs e)
        {
            this._Professions = new List<SelectionPopUp.SelectionEntry>();
            foreach (API.Profession profession in BuildsManager.ModuleInstance.Data.Professions)
            {
                this._Professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig._AsyncTexture.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<AsyncTexture2D>(),
                });
            }

            this.ProfessionSelection.List = this._Professions;

            this.ProfessionSelection.Show();
            this.ProfessionSelection.Location = this.Add_Button.Location.Add(new Point(this.Add_Button.Width + 5, 0));
            this.ProfessionSelection.SelectionType = SelectionPopUp.selectionType.Profession;
            this.ProfessionSelection.SelectionTarget = BuildsManager.ModuleInstance.Selected_Template.Profession;
            this.ProfessionSelection.Width = 175;
            this.ProfessionSelection.SelectionTarget = null;
            this.ProfessionSelection.List = this._Professions;
        }

        private void Templates_Loaded(object sender, EventArgs e)
        {
            this._TemplateSelection.Invalidate();
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            this.NameLabel.Text = BuildsManager.ModuleInstance.Selected_Template.Name;
            this.Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template.Build.TemplateCode;
            this.Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template.Gear.TemplateCode;
        }

        private void Selected_Template_Edit(object sender, EventArgs e)
        {
            BuildsManager.ModuleInstance.Selected_Template.Specialization = null;

            foreach (SpecLine spec in BuildsManager.ModuleInstance.Selected_Template.Build.SpecLines)
            {
                if (spec.Specialization?.Elite == true)
                {
                    BuildsManager.ModuleInstance.Selected_Template.Specialization = spec.Specialization;
                    break;
                }
            }

            this.Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template.Build.ParseBuildCode();
            this.Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Gear.TemplateCode;
            this._TemplateSelection.Refresh();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var rect = new Rectangle(this.Detail_Panel.LocalBounds.Right - 35, 44, 35, 35);
            if (rect.Contains(this.RelativeMousePosition) && BuildsManager.ModuleInstance.Selected_Template.Path != null)
            {
                BuildsManager.ModuleInstance.Selected_Template.Delete();
                BuildsManager.ModuleInstance.Selected_Template = new Template();
            }

            this.ProfessionSelection.Hide();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            var template = BuildsManager.ModuleInstance.Selected_Template;

            if (false && template != null && template.Profession != null && template.Profession.Id == "Revenant")
            {
                var texture = this.Disclaimer_Background;
                var rect = new Rectangle(this.Detail_Panel.LocalBounds.X + 5, this.Detail_Panel.LocalBounds.Y + (this.Detail_Panel.LocalBounds.Height / 2) - 50, this.Detail_Panel.LocalBounds.Width - 10, this.Font.LineHeight + 100);
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    rect,
                    texture.Bounds,
                    new Color(0, 0, 0, 175),
                    0f,
                    default);

                var color = Color.Black;

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
                    this.Font,
                    new Rectangle(this.Detail_Panel.LocalBounds.X + 10, this.Detail_Panel.LocalBounds.Y + (this.Detail_Panel.LocalBounds.Height / 2), this.Detail_Panel.LocalBounds.Width, this.Font.LineHeight),
                    Color.Red,
                    false,
                    HorizontalAlignment.Left);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var rect = new Rectangle(this.Detail_Panel.Location.X, 44, this.Detail_Panel.Width - 38, 35);

            spriteBatch.DrawOnCtrl(
                this,
                this._EmptyTraitLine,
                rect,
                this._EmptyTraitLine.Bounds,
                new Color(135, 135, 135, 255),
                0f,
                default);

            var color = Color.Black;

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

            rect = new Rectangle(this.Detail_Panel.LocalBounds.Right - 35, 44, 35, 35);
            var hovered = rect.Contains(this.RelativeMousePosition);

            spriteBatch.DrawOnCtrl(
                this,
                hovered ? this._DeleteHovered : this._Delete,
                rect,
                this._Delete.Bounds,
                Color.White,
                0f,
                default);

            this.BasicTooltipText = hovered ? Strings.common.Delete + " " + Strings.common.Template : null;

            if (BuildsManager.ModuleInstance.Selected_Template.Profession != null)
            {
                var template = BuildsManager.ModuleInstance.Selected_Template;
                Texture2D texture = BuildsManager.ModuleInstance.TextureManager._Icons[0];

                if (template.Specialization != null)
                {
                    texture = template.Specialization.ProfessionIconBig._AsyncTexture.Texture;
                }
                else if (template.Build.Profession != null)
                {
                    texture = template.Build.Profession.IconBig._AsyncTexture.Texture;
                }

                rect = new Rectangle(this.Detail_Panel.Location.X + 2, 46, 30, 30);

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

        protected override void DisposeControl()
        {
            this.Add_Button.Click -= this.Button_Click;
            this.Add_Button.Dispose();

            BuildsManager.ModuleInstance.LanguageChanged -= this.ModuleInstance_LanguageChanged;
            BuildsManager.ModuleInstance.Selected_Template_Edit -= this.Selected_Template_Edit;
            BuildsManager.ModuleInstance.Selected_Template_Changed -= this.ModuleInstance_Selected_Template_Changed;
            BuildsManager.ModuleInstance.Templates_Loaded -= this.Templates_Loaded;
            BuildsManager.ModuleInstance.Selected_Template_Redraw -= this.Selected_Template_Redraw;

            this.Detail_Panel.Tabs[0].Panel.Resized -= this.Panel_Resized;
            this.Detail_Panel.TemplateBox.EnterPressed -= this.TemplateBox_EnterPressed;
            this.Detail_Panel.GearBox.EnterPressed -= this.GearBox_EnterPressed;

            Input.Mouse.LeftMouseButtonPressed -= this.GlobalClick;

            GameService.Gw2Mumble.PlayerCharacter.NameChanged -= this.PlayerCharacter_NameChanged;

            this._TemplateSelection.TemplateChanged -= this._TemplateSelection_TemplateChanged;
            this._TemplateSelection.Dispose();

            this.NameBox.EnterPressed -= this.NameBox_TextChanged;
            this.NameBox.Dispose();

            this.NameLabel.Click -= this.NameLabel_Click;
            this.NameLabel.Dispose();

            this.Import_Button.Click -= this.Import_Button_Click;
            this.Import_Button.Dispose();

            this.ProfessionSelection.Changed -= this.ProfessionSelection_Changed;
            this.ProfessionSelection.Dispose();

            base.DisposeControl();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (BuildsManager.ModuleInstance.ShowCurrentProfession.Value)
            {
                this._TemplateSelection.SetSelection();
            }
        }
    }
}
