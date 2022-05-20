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
using Blish_HUD.Input;
using MonoGame.Extended.BitmapFonts;
using System.Threading;

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

    class Container_TabbedPanel : Container
    {
        public Tab SelectedTab;
        public List<Tab> Tabs = new List<Tab>();
        Texture2D _TabBarTexture;
        public TextBox TemplateBox;
        public TextBox GearBox;

        Texture2D _Copy;
        Texture2D _CopyHovered;

        int TabSize;
        public Container_TabbedPanel()
        {
            _TabBarTexture = BuildsManager.TextureManager.getControlTexture(_Controls.TabBar_FadeIn);

            _Copy = BuildsManager.TextureManager.getControlTexture(_Controls.Copy);
            _CopyHovered = BuildsManager.TextureManager.getControlTexture(_Controls.Copy_Hovered);

            TemplateBox = new TextBox()
            {
                Parent = this,
                Width = Width,
                Location = new Point(5, 45)
            };
            TemplateBox.InputFocusChanged += TemplateBox_InputFocusChanged;

            GearBox = new TextBox()
            {
                Parent = this,
                Width = Width,
                Location = new Point(5, 45 + 5 + TemplateBox.Height)
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
                if (tab.Panel != null) tab.Panel.Dispose();
            }

            base.DisposeControl();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < 2; i++)
            {

               var rect = new Rectangle(LocalBounds.Width - TemplateBox.Height - 6, TemplateBox.LocalBounds.Y + (i * (TemplateBox.Height + 5)), TemplateBox.Height, TemplateBox.Height);

                if (rect.Contains(RelativeMousePosition))
                {
                    var text = i == 0 ? TemplateBox.Text : GearBox.Text;
                    if(text != "" && text != null)
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
                if (tab.Panel != null) tab.Panel.Size = new Point(Width, Height - 50);
            }
        }

        void UpdateLayout()
        {
            int i = 0;
            TabSize = (Width / Math.Max(1, Tabs.Count));

            foreach (Tab tab in Tabs)
            {
                if (tab.Panel != null) tab.Panel.Size = new Point(Width, Height - 60);
                if (tab.Panel?.Location.Y < GearBox.LocalBounds.Bottom + 5) tab.Panel.Location = new Point(tab.Panel.Location.X, GearBox.LocalBounds.Bottom + 5);
                if (tab.Panel?.Location.X < 5) tab.Panel.Location = new Point(5, tab.Panel.Location.Y);

                tab.Bounds = new Rectangle(i * TabSize, 0, TabSize, 40);

                if (tab.Icon != null)
                {
                    tab.Icon_Bounds = new Rectangle(i * TabSize + 5, 5, 30, 30);
                    tab.Text_Bounds = new Rectangle(i * TabSize + 45, 0, TabSize - 50, 40);
                }
                else
                {
                    tab.Text_Bounds = new Rectangle(i * TabSize, 0, TabSize, 40);
                }

                tab.Hovered = tab.Bounds.Contains(RelativeMousePosition);
                i++;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            UpdateLayout();

            var cnt = new ContentService();
            var font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)ContentService.FontSize.Size16, ContentService.FontStyle.Regular);

            Color color;
            Rectangle rect;

            foreach (Tab tab in Tabs)
            {
                var color2 = SelectedTab == tab ?  new Color(30, 30, 30, 10): tab.Hovered ? new Color(0, 0, 0, 50) : Color.Transparent;
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, tab.Bounds, tab.Bounds, color2);

                spriteBatch.DrawOnCtrl(this,
                                       _TabBarTexture,
                                       tab.Bounds,
                                      _TabBarTexture.Bounds,
                                      Color.White,
                                      0f,
                                      default);

                if (tab.Icon != null)
                {
                    spriteBatch.DrawOnCtrl(this,
                                          tab.Icon,
                                          tab.Icon_Bounds,
                                          tab.Icon.Bounds,
                                          Color.White,
                                          0f,
                                          default);
                }

                spriteBatch.DrawStringOnCtrl(this,
                                       tab.Name,
                                       font,
                                       tab.Text_Bounds,
                                       SelectedTab == tab ? Color.White : Color.LightGray,
                                       false,
                                       HorizontalAlignment.Left
                                       );

                color = Color.Black;
                rect = tab.Bounds;

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

            for (int i = 0; i < 2; i++)
            {

                rect = new Rectangle(bounds.Width - TemplateBox.Height - 6, TemplateBox.LocalBounds.Y + (i * (TemplateBox.Height + 5)), TemplateBox.Height, TemplateBox.Height);
                var hovered = rect.Contains(RelativeMousePosition);
                spriteBatch.DrawOnCtrl(this,
                                       hovered ? _CopyHovered : _Copy,
                                        rect,
                                        _Copy.Bounds,
                                        Color.White,
                                        0f,
                                        default);
                BasicTooltipText = hovered ? Strings.common.Copy + " " + Strings.common.Template : null;

                color = Color.Black;
                rect = bounds;

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
        }
    }

   public class Window_MainWindow : StandardWindow
    {
        Panel Templates_Panel;
        Container_TabbedPanel Detail_Panel;

        Control_Equipment Gear;
        Control_Build Build;
        Control_TemplateSelection _TemplateSelection;

        Texture2D _EmptyTraitLine;
        Texture2D _Delete;
        Texture2D _DeleteHovered;
        Texture2D ProfessionIcon;
        Texture2D Disclaimer_Background;

        SelectionPopUp ProfessionSelection;
        List<SelectionPopUp.SelectionEntry> _Professions;

        TextBox NameBox;
        Label NameLabel;
        Control_AddButton Add_Button;
        public Control_AddButton Import_Button;

        private BitmapFont Font;

        private void _TemplateSelection_TemplateChanged(object sender, TemplateChangedEvent e)
        {
            BuildsManager.ModuleInstance.Selected_Template = e.Template;
            Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Build.TemplateCode;
            NameLabel.Text = e.Template.Name;

            NameLabel.Show();
            NameBox.Hide();
        }

        public Window_MainWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            _EmptyTraitLine = BuildsManager.TextureManager.getControlTexture(_Controls.PlaceHolder_Traitline).GetRegion(0, 0, 647, 136);
            _Delete = BuildsManager.TextureManager.getControlTexture(_Controls.Delete);
            _DeleteHovered = BuildsManager.TextureManager.getControlTexture(_Controls.Delete_Hovered);

            Disclaimer_Background = BuildsManager.TextureManager._Controls[(int)_Controls.PlaceHolder_Traitline].GetRegion(0, 0, 647, 136);

            var cnt = new ContentService();
            Font = cnt.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)18, ContentService.FontStyle.Regular);

            Templates_Panel = new Panel()
            {
                Parent = this,
                Location = new Point(0, 0),
                Size = new Point(260, ContentRegion.Height),
                BackgroundColor = new Color(0, 0, 0, 50),
            };

            Detail_Panel = new Container_TabbedPanel()
            {
                Parent = this,
                Location = new Point(Templates_Panel.LocalBounds.Right, 40),
                Size = new Point(ContentRegion.Width - Templates_Panel.LocalBounds.Right, ContentRegion.Height - 45),
                //BackgroundColor = Color.Aqua,
            };

           _Professions = new List<SelectionPopUp.SelectionEntry>();
            ProfessionSelection = new SelectionPopUp(this)
            {
            };

            foreach (API.Profession profession in BuildsManager.Data.Professions)
            {
                _Professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<Texture2D>(),
                });
            }
            ProfessionSelection.List = _Professions;
            ProfessionSelection.Changed += delegate
            {
                if (ProfessionSelection.SelectedProfession != null)
                {
                    BuildsManager.ModuleInstance.Selected_Template = new Template();
                    BuildsManager.ModuleInstance.Selected_Template.Profession = ProfessionSelection.SelectedProfession;
                    BuildsManager.ModuleInstance.Selected_Template.Build.Profession = ProfessionSelection.SelectedProfession;
                    BuildsManager.ModuleInstance.Templates.Add(BuildsManager.ModuleInstance.Selected_Template);

                    BuildsManager.ModuleInstance.Selected_Template.SetChanged();
                    _TemplateSelection.Refresh();
                    ProfessionSelection.SelectedProfession = null;
                }
            };

            Import_Button = new Control_AddButton()
            {
                Texture = BuildsManager.TextureManager.getControlTexture(_Controls.Import),
                TextureHovered = BuildsManager.TextureManager.getControlTexture(_Controls.Import_Hovered),
                Parent = Templates_Panel,
                Text = "",
                Location = new Point(Templates_Panel.Width - 130 - 40, 0),
                Size = new Point(35, 35),
                BasicTooltipText = string.Format("Import 'BuildPad' builds from '{0}config.ini'", BuildsManager.Paths.builds),
                Visible = false,
            };
            Import_Button.Click += Import_Button_Click;


            Add_Button = new Control_AddButton()
            {
                Parent = Templates_Panel,
                Text = Strings.common.Create,
                Location = new Point(Templates_Panel.Width - 130, 0),
                Size = new Point(125, 35),
            };
            Add_Button.Click += Button_Click;
            BuildsManager.ModuleInstance.LanguageChanged += ModuleInstance_LanguageChanged;

            _TemplateSelection = new Control_TemplateSelection(this)
            {
                Location = new Point(5, 40),
                Parent = Templates_Panel,
            };
            _TemplateSelection.TemplateChanged += _TemplateSelection_TemplateChanged;

            Detail_Panel.Tabs = new List<Tab>()
            {
                new Tab()
                {
                    Name = Strings.common.Build,
                    Icon = BuildsManager.TextureManager.getIcon(_Icons.Template),
                    Panel = new Panel(){ Parent = Detail_Panel, Visible = true },
                },
                new Tab()
                {
                    Name = Strings.common.Gear,
                    Icon = BuildsManager.TextureManager.getIcon(_Icons.Helmet),
                    Panel = new Panel(){ Parent = Detail_Panel, Visible = false },
                },
            };

            Detail_Panel.SelectedTab = Detail_Panel.Tabs[0];
            Detail_Panel.Tabs[0].Panel.Resized += delegate
            {
                Gear.Size = Detail_Panel.Tabs[0].Panel.Size.Add(new Point(0, - Detail_Panel.GearBox.Bottom + 30));
            };

            Build = new Control_Build(Detail_Panel.Tabs[0].Panel)
            {
                Parent = Detail_Panel.Tabs[0].Panel,
                Size = Detail_Panel.Tabs[0].Panel.Size,
                Scale = 1,
            };

            Gear = new Control_Equipment(Detail_Panel.Tabs[1].Panel)
            {
                Parent = Detail_Panel.Tabs[1].Panel,
                Size = Detail_Panel.Tabs[1].Panel.Size,
                Scale = 1,
            };

            Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Build.ParseBuildCode();
            Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Gear.TemplateCode;

            NameBox = new TextBox()
            {
                Parent = this,
                Location = new Point(Detail_Panel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = Detail_Panel.Width - 38 - 32 - 5,
                Font = Font,
                Visible = false,
            };
            NameBox.EnterPressed += NameBox_TextChanged;

            NameLabel = new Label()
            {
                Text = "A Template Name",
                Parent = this,
                Location = new Point(Detail_Panel.Location.X + 5 + 32, 0),
                Height = 35,
                Width = Detail_Panel.Width - 38 - 32 - 5,
                Font = Font,
            };
            NameLabel.Click += NameLabel_Click;

            BuildsManager.ModuleInstance.Selected_Template_Edit += Selected_Template_Edit;
            BuildsManager.ModuleInstance.Selected_Template_Changed += ModuleInstance_Selected_Template_Changed;
            BuildsManager.ModuleInstance.Templates_Loaded += Templates_Loaded;
            BuildsManager.ModuleInstance.Selected_Template_Redraw += Selected_Template_Redraw;

            Detail_Panel.TemplateBox.EnterPressed += TemplateBox_EnterPressed;
            Detail_Panel.GearBox.EnterPressed += GearBox_EnterPressed;

            Input.Mouse.LeftMouseButtonPressed += GlobalClick;
        }

        private void ImportFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ScreenNotification.ShowNotification("IMPORT FILE", ScreenNotification.NotificationType.Warning);

        }

        private void Import_Button_Click(object sender, MouseEventArgs e)
        {
            BuildsManager.ModuleInstance.LoadTemplates();
            _TemplateSelection.Refresh();
            Import_Button.Hide();
        }

        private void ModuleInstance_LanguageChanged(object sender, EventArgs e)
        {
            Add_Button.Text = Strings.common.Create;
            Import_Button.Text = Strings.common.Create;
            Detail_Panel.Tabs[0].Name = Strings.common.Build;
            Detail_Panel.Tabs[1].Name = Strings.common.Gear;
        }

        private void GlobalClick(object sender, MouseEventArgs e)
        {
            if (!NameBox.MouseOver)
            {
                NameBox.Visible = false;
                NameLabel.Visible = true;
            }
        }

        private void Selected_Template_Redraw(object sender, EventArgs e)
        {
            Build.SkillBar.ApplyBuild(sender, e);
            Gear.UpdateLayout();
        }

        private void GearBox_EnterPressed(object sender, EventArgs e)
        {
            var code = Detail_Panel.GearBox.Text;
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
            var code = Detail_Panel.TemplateBox.Text;
            var build = new BuildTemplate(code);

            if(build != null && build.Profession != null)
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
            BuildsManager.ModuleInstance.Selected_Template.Name = NameBox.Text;
            BuildsManager.ModuleInstance.Selected_Template.Save();

            NameLabel.Visible = true;
            NameBox.Visible = false;
            NameLabel.Text = BuildsManager.ModuleInstance.Selected_Template.Name;
            _TemplateSelection.Refresh();
        }

        private void NameLabel_Click(object sender, MouseEventArgs e)
        {
            if (BuildsManager.ModuleInstance.Selected_Template?.Path != null)
            {
                NameLabel.Visible = false;
                NameBox.Visible = true;
                NameBox.Text = NameLabel.Text;
                NameBox.SelectionStart = 0;
                NameBox.SelectionEnd = NameBox.Text.Length;
                NameBox.Focused = true;
            }
        }

        private void Button_Click(object sender, MouseEventArgs e)
        {
            _Professions = new List<SelectionPopUp.SelectionEntry>();
            foreach (API.Profession profession in BuildsManager.Data.Professions)
            {
                _Professions.Add(new SelectionPopUp.SelectionEntry()
                {
                    Object = profession,
                    Texture = profession.IconBig.Texture,
                    Header = profession.Name,
                    Content = new List<string>(),
                    ContentTextures = new List<Texture2D>(),
                });
            }
            ProfessionSelection.List = _Professions;

            ProfessionSelection.Show();
            ProfessionSelection.Location = Add_Button.Location.Add(new Point(Add_Button.Width + 5, 0));
            ProfessionSelection.SelectionType = SelectionPopUp.selectionType.Profession;
            ProfessionSelection.SelectionTarget = BuildsManager.ModuleInstance.Selected_Template.Profession;
            ProfessionSelection.Width = 175;
            ProfessionSelection.SelectionTarget = null;
            ProfessionSelection.List = _Professions;
        }

        private void Templates_Loaded(object sender, EventArgs e)
        {
            _TemplateSelection.Invalidate();
        }

        private void ModuleInstance_Selected_Template_Changed(object sender, EventArgs e)
        {
            NameLabel.Text = BuildsManager.ModuleInstance.Selected_Template.Name;
            Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template.Build.TemplateCode;
            Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template.Gear.TemplateCode;
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

            Detail_Panel.TemplateBox.Text = BuildsManager.ModuleInstance.Selected_Template.Build.ParseBuildCode();
            Detail_Panel.GearBox.Text = BuildsManager.ModuleInstance.Selected_Template?.Gear.TemplateCode;
            _TemplateSelection.Refresh();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var rect = new Rectangle(Detail_Panel.LocalBounds.Right - 35, 44, 35, 35);
            if (rect.Contains(RelativeMousePosition) && BuildsManager.ModuleInstance.Selected_Template.Path != null)
            {
                BuildsManager.ModuleInstance.Selected_Template.Delete();
                BuildsManager.ModuleInstance.Selected_Template = new Template();

                _TemplateSelection.Refresh();
            }

            ProfessionSelection.Hide();
        }
        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            var template = BuildsManager.ModuleInstance.Selected_Template;

            if (template != null && template.Profession != null && template.Profession.Id == "Revenant")
            {
                var texture = Disclaimer_Background;
                var rect = new Rectangle(Detail_Panel.LocalBounds.X + 5, Detail_Panel.LocalBounds.Y + Detail_Panel.LocalBounds.Height / 2 - 50, Detail_Panel.LocalBounds.Width - 10, Font.LineHeight + 100);
                spriteBatch.DrawOnCtrl(this,
                                       texture,
                                        rect,
                                        texture.Bounds,
                                        new Color(0, 0, 0, 175),
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

                spriteBatch.DrawStringOnCtrl(this,
                                        "Revenant is currently not supported! It is in development tho and hopefully comes soon!",
                                        Font,
                                        new Rectangle(Detail_Panel.LocalBounds.X + 10, Detail_Panel.LocalBounds.Y + Detail_Panel.LocalBounds.Height / 2, Detail_Panel.LocalBounds.Width, Font.LineHeight),
                                        Color.Red,
                                        false,
                                        HorizontalAlignment.Left
                                        );
            }
        }
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var rect = new Rectangle(Detail_Panel.Location.X, 44, Detail_Panel.Width - 38, 35);

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



            rect = new Rectangle(Detail_Panel.LocalBounds.Right - 35, 44, 35, 35);
            var hovered = rect.Contains(RelativeMousePosition);

            spriteBatch.DrawOnCtrl(this,
                                   hovered ? _DeleteHovered : _Delete,
                                   rect,
                                    _Delete.Bounds,
                                    Color.White,
                                  0f,
                                  default);

            BasicTooltipText = hovered ? Strings.common.Delete + " " + Strings.common.Template : null;

            if(BuildsManager.ModuleInstance.Selected_Template.Profession != null)
            {
                var template = BuildsManager.ModuleInstance.Selected_Template;
                Texture2D texture = BuildsManager.TextureManager._Icons[0];

                if (template.Specialization != null)
                {
                    texture = template.Specialization.ProfessionIconBig.Texture;
                }
                else if (template.Build.Profession != null)
                {
                    texture = template.Build.Profession.IconBig.Texture;
                }

                rect = new Rectangle(Detail_Panel.Location.X + 2, 46, 30, 30);

                spriteBatch.DrawOnCtrl(this,
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
            _TemplateSelection.Dispose();
            NameBox.Dispose();
            NameLabel.Dispose();

            Import_Button.Dispose();
            Add_Button.Dispose();

            base.DisposeControl();
        }
    }
}
