using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : Module
    {
        internal static BuildsManager ModuleInstance;
        public static readonly Logger Logger = Logger.GetLogger<BuildsManager>();

        #region Service Managers

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_PASTE = 0x0302;
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


        public TextureManager TextureManager;
        public iPaths Paths;
        public iData Data; public iTicks Ticks = new iTicks();
        public List<int> ArmoryItems = new List<int>();

        public SettingEntry<bool> PasteOnCopy;
        public SettingEntry<bool> ShowCornerIcon;
        public SettingEntry<bool> IncludeDefaultBuilds;
        public SettingEntry<bool> ShowCurrentProfession;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ToggleWindow;
        public SettingEntry<int> GameVersion;
        public SettingEntry<string> ModuleVersion;

        public string CultureString;
        public List<Template> Templates = new List<Template>();
        public List<Template> DefaultTemplates = new List<Template>();
        private Template _Selected_Template;
        public Template Selected_Template
        {
            get => _Selected_Template;
            set
            {
                if (_Selected_Template != null)
                {
                    _Selected_Template.Edit -= OnSelected_Template_Edit;
                }

                _Selected_Template = value;

                if (value != null)
                {
                    OnSelected_Template_Changed();
                    _Selected_Template.Edit += OnSelected_Template_Edit;
                }
            }
        }

        public event EventHandler Selected_Template_Redraw;
        public void OnSelected_Template_Redraw(object sender, EventArgs e)
        {
            this.Selected_Template_Redraw?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LanguageChanged;
        public void OnLanguageChanged(object sender, EventArgs e)
        {
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected_Template_Edit;
        public void OnSelected_Template_Edit(object sender, EventArgs e)
        {
            MainWindow._TemplateSelection.RefreshList();
            this.Selected_Template_Edit?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected_Template_Changed;
        public void OnSelected_Template_Changed()
        {
            this.Selected_Template_Changed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Template_Deleted;
        public void OnTemplate_Deleted()
        {
            Selected_Template = new Template();
            this.Template_Deleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Templates_Loaded;
        public void OnTemplates_Loaded()
        {
            LoadTemplates();
            this.Templates_Loaded?.Invoke(this, EventArgs.Empty);
        }

        public Window_MainWindow MainWindow;
        public Texture2D LoadingTexture;
        public LoadingSpinner loadingSpinner;
        public ProgressBar downloadBar;
        private CornerIcon cornerIcon;

        public event EventHandler DataLoaded_Event;
        private bool _DataLoaded;
        public bool FetchingAPI;
        public bool DataLoaded
        {
            get => _DataLoaded;
            set
            {
                _DataLoaded = value;
                if (value) ModuleInstance.OnDataLoaded();
            }
        }

        void OnDataLoaded()
        {
            this.DataLoaded_Event?.Invoke(this, EventArgs.Empty);
        }

        protected override void DefineSettings(SettingCollection settings)
        {


            ToggleWindow = settings.DefineSetting(nameof(ToggleWindow),
                                                      new Blish_HUD.Input.KeyBinding(ModifierKeys.Ctrl, Keys.B),
                                                      () => "Toggle Window",
                                                      () => "Show / Hide the UI");

            PasteOnCopy = settings.DefineSetting(nameof(PasteOnCopy),
                                                      false,
                                                      () => "Paste Stat/Upgrade Name",
                                                      () => "Paste Stat/Upgrade Name after copying it.");

            ShowCornerIcon = settings.DefineSetting(nameof(ShowCornerIcon),
                                                      true,
                                                      () => "Show Corner Icon",
                                                      () => "Show / Hide the Corner Icon of this module.");

            IncludeDefaultBuilds = settings.DefineSetting(nameof(IncludeDefaultBuilds),
                                                      true,
                                                      () => "Incl. Default Builds",
                                                      () => "Load the default builds from within the module.");

            ShowCurrentProfession = settings.DefineSetting(nameof(ShowCurrentProfession),
                                                      true,
                                                      () => "Filter Current Profession",
                                                      () => "Always set the current Profession as an active filter.");

            var internal_settings = settings.AddSubCollection("Internal Settings", false);
            GameVersion = internal_settings.DefineSetting(nameof(GameVersion), 0);
            ModuleVersion = internal_settings.DefineSetting(nameof(ModuleVersion), "0.0.0");

            ReloadKey = internal_settings.DefineSetting(nameof(ReloadKey),
                                                      new Blish_HUD.Input.KeyBinding(Keys.None),
                                                      () => "Reload Button",
                                                      () => "");
        }

        protected override void Initialize()
        {
            CultureString = BuildsManager.getCultureString();
            Logger.Info("Starting Builds Manager v." + Version.BaseVersion());
            Paths = new iPaths(DirectoriesManager.GetFullDirectoryPath("builds-manager"));
            ArmoryItems.AddRange(new int[] {
                  80248, //Perfected Envoy armor (light) Head
                  80131, //Perfected Envoy armor (light) Shoulder
                  80190, //Perfected Envoy armor (light) Chest
                  80111, //Perfected Envoy armor (light) Hands
                  80356, //Perfected Envoy armor (light) Legs
                  80399, //Perfected Envoy armor (light) Feet

                  80296, //Perfected Envoy armor (medium) Head
                  80145, //Perfected Envoy armor (medium) Shoulder
                  80578, //Perfected Envoy armor (medium) Chest
                  80161, //Perfected Envoy armor (medium) Hands
                  80252, //Perfected Envoy armor (medium) Legs
                  80281, //Perfected Envoy armor (medium) Feet

                  80384, //Perfected Envoy armor (heavy) Head
                  80435, //Perfected Envoy armor (heavy) Shoulder
                  80254, //Perfected Envoy armor (heavy) Chest
                  80205, //Perfected Envoy armor (heavy) Hands
                  80277, //Perfected Envoy armor (heavy) Legs
                  80557, //Perfected Envoy armor (heavy) Feet

                  91505, //Legendary Sigil
                  91536, //Legendary Rune

                  81908, //Legendary Accessory Aurora
                  91048, //Legendary Accessory Vision
                  91234, //Legendary Ring Coalescence
                  93105, //Legendary Ring Conflux
                  95380, //Legendary Amulet Prismatic Champion's Regalia

                  74155, //Legendary Backpack Ad Infinitum

                  30698, //The Bifrost
                  30699, //Bolt
                  30686, //The Dreamer
                  30696, //The Flameseeker Prophecies
                  30697, //Frenzy
                  30695, //Meteorlogicus
                  30684, //Frostfang
                  30702, //Howler
                  30687, //Incinerator
                  30690, //The Juggernaut
                  30685, //Kudzu
                  30701, //Kraitkin
                  30691, //Kamohoali'i Kotaki
                  30688, //The Minstrel
                  30692, //The Moot
                  30694, //The Predator
                  30693, //Quip
                  30700, //Rodgort
                  //30703, //Sunrise
                  //30704, //Twilight
                  30689, //Eternity
                });

            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += ReloadKey_Activated;

            ToggleWindow.Value.Enabled = true;
            ToggleWindow.Value.Activated += ToggleWindow_Activated;

            IncludeDefaultBuilds.SettingChanged += IncludeDefaultBuilds_SettingChanged;

            DataLoaded = false;

            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged += PlayerCharacter_SpecializationChanged;
        }

        private void IncludeDefaultBuilds_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue == false)
            {
                foreach(Template t in DefaultTemplates)
                {
                    Templates.Remove(t);
                }
            }
            else
            {
                foreach (Template t in DefaultTemplates)
                {
                    Templates.Add(t);
                }
            }

            MainWindow._TemplateSelection.Refresh();
        }

        public API.Profession CurrentProfession;
        public API.Specialization CurrentSpecialization;

        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> eventArgs)
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (player != null && player.Profession > 0 && Data != null && MainWindow != null)
            {
                CurrentProfession = Data.Professions.Find(e => e.Id == player.Profession.ToString());
                CurrentSpecialization = CurrentProfession?.Specializations.Find(e => e.Id == player.Specialization);

                MainWindow.PlayerCharacter_NameChanged(null, null);


                Templates = Templates.OrderBy(a => a.Build?.Profession != BuildsManager.ModuleInstance.CurrentProfession).ThenBy(a => a.Profession.Id).ThenBy(b => b.Specialization?.Id).ThenBy(b => b.Name).ToList();
                MainWindow._TemplateSelection.RefreshList();
            }
        }

        private void ToggleWindow_Activated(object sender, EventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void ReloadKey_Activated(object sender, EventArgs e)
        {
            ScreenNotification.ShowNotification("Rebuilding the UI", ScreenNotification.NotificationType.Warning);
            MainWindow?.Dispose();
            CreateUI();
            MainWindow.ToggleWindow();
        }

        protected override async Task LoadAsync()
        {
        }

        public void ImportTemplates()
        {
            Template saveTemplate = null;
            if (System.IO.File.Exists(Paths.builds + "config.ini"))
            {
                var iniContent = System.IO.File.ReadAllText(Paths.builds + "config.ini");
                bool started = false;
                foreach (string s in iniContent.Split('\n'))
                {
                    if (s.Trim() != "")
                    {
                        if (started && s.StartsWith("["))
                        {
                            break;
                        }

                        if (started)
                        {
                            var buildpadBuild = s.Trim().Split('|');

                            var template = new Template();
                            template.Template_json = new Template_json()
                            {
                                Name = buildpadBuild[5],
                                BuildCode = buildpadBuild[1],
                            };

                            if (Templates.Find(e => e.Template_json.Name == template.Template_json.Name && e.Template_json.BuildCode == template.Template_json.BuildCode) == null)
                            {
                                template.Build = new BuildTemplate(buildpadBuild[1]);
                                template.Name = buildpadBuild[5];

                                template.Profession = template.Build.Profession;
                                template.Specialization = template.Build.SpecLines.Find(e => e.Specialization?.Elite == true)?.Specialization;

                                Logger.Debug("Adding new template: '{0}'", template.Template_json.Name);
                                Templates.Add(template);
                                saveTemplate = template;
                            }
                        }

                        if (!started && s.StartsWith("[Builds]")) started = true;
                    }
                }
                System.IO.File.Delete(Paths.builds + "config.ini");
            }


            var files = Directory.GetFiles(Paths.builds, "*.json", SearchOption.TopDirectoryOnly).ToList();
            if (files.Contains(Paths.builds + "Builds.json")) files.Remove(Paths.builds + "Builds.json");

            foreach (string path in files)
            {
                var template = new Template(path);
                System.IO.File.Delete(path);
                Templates.Add(template);
                saveTemplate = template;

                if (path == files[files.Count - 1]) template.Save();
            }

            BuildsManager.Logger.Debug("Saving {0} Templates.", Templates.Count);
            if (saveTemplate != null) saveTemplate.Save();
        }

        public void LoadTemplates()
        {
            var currentTemplate = _Selected_Template?.Name;

            ImportTemplates();

            Templates = new List<Template>();
            var paths = new List<string>()
            {
                Paths.builds + "Builds.json",
                //Paths.builds + "Default Builds.json",
            };

            foreach (string path in paths)
            {
                string content = "";
                try
                {
                    if (System.IO.File.Exists(path)) content = System.IO.File.ReadAllText(path);
                    if (content != null && content != "")
                    {
                        var templates = JsonConvert.DeserializeObject<List<Template_json>>(content);

                        foreach (Template_json jsonTemplate in templates)
                        {
                            var template = new Template(jsonTemplate.Name, jsonTemplate.BuildCode, jsonTemplate.GearCode);
                            template.Path = path;

                            Templates.Add(template);

                            if (template.Name == currentTemplate) _Selected_Template = template;
                        }
                    }
                }
                catch
                {

                }
            }

            
            var defaultTemps = JsonConvert.DeserializeObject<List<Template_json>>(new StreamReader(ContentsManager.GetFileStream(@"data\builds.json")).ReadToEnd());

            if (defaultTemps != null)
            {
                foreach (Template_json jsonTemplate in defaultTemps)
                {
                    var template = new Template(jsonTemplate.Name, jsonTemplate.BuildCode, jsonTemplate.GearCode);
                    template.Path = null;
                    DefaultTemplates.Add(template);
                    if (IncludeDefaultBuilds.Value) Templates.Add(template);

                    if (template.Name == currentTemplate) _Selected_Template = template;
                }
            }

            //Templates = Templates.OrderBy(a => a.Build?.Profession != BuildsManager.ModuleInstance.CurrentProfession).ThenBy(a => a.Profession.Id).ThenBy(b => b.Specialization?.Id).ThenBy(b => b.Name).ToList();

            _Selected_Template?.SetChanged();
            OnSelected_Template_Changed();
        }

        protected override async void OnModuleLoaded(EventArgs e)
        {
            TextureManager = new TextureManager();

            cornerIcon = new CornerIcon()
            {
                Icon = TextureManager.getIcon(_Icons.Template),
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen,
                Visible = ShowCornerIcon.Value,
            };

            loadingSpinner = new LoadingSpinner()
            {
                Location = new Point(cornerIcon.Location.X - cornerIcon.Width, cornerIcon.Location.Y + cornerIcon.Height + 5),
                Size = cornerIcon.Size,
                Visible = false,
                Parent = GameService.Graphics.SpriteScreen,
            };
            downloadBar = new ProgressBar()
            {
                Location = new Point(cornerIcon.Location.X, cornerIcon.Location.Y + cornerIcon.Height + 5 + 3),
                Size = new Point(150, cornerIcon.Height - 6),
                Parent = GameService.Graphics.SpriteScreen,
                Progress = 0.66,
                Visible = false,
            };

            cornerIcon.MouseEntered += CornerIcon_MouseEntered;
            cornerIcon.MouseLeft += CornerIcon_MouseLeft;
            cornerIcon.Click += CornerIcon_Click;
            cornerIcon.Moved += CornerIcon_Moved;
            ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
            DataLoaded_Event += BuildsManager_DataLoaded_Event;

            // Base handler must be called
            base.OnModuleLoaded(e);

            await LoadData();
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            loadingSpinner.Location = new Point(cornerIcon.Location.X - cornerIcon.Width, cornerIcon.Location.Y + cornerIcon.Height + 5);
            downloadBar.Location = new Point(cornerIcon.Location.X, cornerIcon.Location.Y + cornerIcon.Height + 5 + 3);
        }

        private void BuildsManager_DataLoaded_Event(object sender, EventArgs e)
        {
            CreateUI();
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (MainWindow != null) MainWindow.ToggleWindow();
        }

        private void CornerIcon_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            cornerIcon.Icon = TextureManager.getIcon(_Icons.Template);
        }

        private void CornerIcon_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            cornerIcon.Icon = TextureManager.getIcon(_Icons.Template_White);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (cornerIcon != null) cornerIcon.Visible = e.NewValue;
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.global += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Ticks.global > 1250)
            {
                Ticks.global -= 1250;

                if (CurrentProfession == null ) PlayerCharacter_SpecializationChanged(null, null);

                if (MainWindow?.Visible == true)
                {
                    MainWindow.Import_Button.Visible = System.IO.File.Exists(Paths.builds + "config.ini");
                }
            }
        }

        protected override void Unload()
        {
            MainWindow?.Dispose();

            Templates?.DisposeAll();
            Templates?.Clear();

            DefaultTemplates?.DisposeAll();
            DefaultTemplates?.Clear();

            TextureManager?.Dispose();
            TextureManager = null;

            _Selected_Template.Edit -= OnSelected_Template_Edit;
            _Selected_Template.Edit -= null;

            Selected_Template = null;
            CurrentProfession = null;
            CurrentSpecialization = null;

            Data?.Dispose();
            Data = null;

            TextureManager = null;
            cornerIcon?.Dispose();

            ToggleWindow.Value.Enabled = false;
            ToggleWindow.Value.Activated -= ToggleWindow_Activated;

            ReloadKey.Value.Enabled = false;
            ReloadKey.Value.Activated -= ReloadKey_Activated;

            cornerIcon.MouseEntered -= CornerIcon_MouseEntered;
            cornerIcon.MouseLeft -= CornerIcon_MouseLeft;
            cornerIcon.Click -= CornerIcon_Click;
            cornerIcon.Moved -= CornerIcon_Moved;

            DataLoaded_Event -= BuildsManager_DataLoaded_Event;
            ShowCornerIcon.SettingChanged -= ShowCornerIcon_SettingChanged;
            IncludeDefaultBuilds.SettingChanged -= IncludeDefaultBuilds_SettingChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged -= PlayerCharacter_SpecializationChanged;
            OverlayService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
            
            downloadBar?.Dispose();
            downloadBar = null;

            DataLoaded = false;
            ModuleInstance = null;
        }

        public static string getCultureString()
        {
            var culture = "en-EN";
            switch (OverlayService.Overlay.UserLocale.Value)
            {
                case Gw2Sharp.WebApi.Locale.French:
                    culture = "fr-FR";
                    break;

                case Gw2Sharp.WebApi.Locale.Spanish:
                    culture = "es-ES";
                    break;

                case Gw2Sharp.WebApi.Locale.German:
                    culture = "de-DE";
                    break;

                default:
                    culture = "en-EN";
                    break;
            }
            return culture;
        }

        public async Task Fetch_APIData(bool force = false)
        {
            if (GameVersion.Value != Gw2MumbleService.Gw2Mumble.Info.Version || ModuleVersion.Value != Version.BaseVersion().ToString() || force == true || false)
            {
                FetchingAPI = true;

                var downloadList = new List<APIDownload_Image>();
                var culture = getCultureString();

                double total = 0;
                double completed = 0;
                double progress = 0;

                var _runes = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\runes.json")).ReadToEnd());
                var _sigils = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\sigils.json")).ReadToEnd());

                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                };

                int totalFetches = 9;

                Logger.Debug("Fetching all required Data from the API!");
                loadingSpinner.Visible = true;
                downloadBar.Visible = true;
                downloadBar.Progress = progress;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);

                var sigils = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_sigils);
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Sigils"));

                var runes = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_runes);
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Runes"));

                var armory_items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ArmoryItems);
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Armory"));

                var professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Professions"));

                var specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Specs"));

                var traits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Traits"));

                List<int> Skill_Ids = new List<int>();
                var legends = JsonConvert.DeserializeObject<List<iData._Legend>>(new StreamReader(ContentsManager.GetFileStream(@"data\legends.json")).ReadToEnd());

                foreach (iData._Legend legend in legends)
                {
                    if (!Skill_Ids.Contains(legend.Skill)) Skill_Ids.Add(legend.Skill);
                    if (!Skill_Ids.Contains(legend.Swap)) Skill_Ids.Add(legend.Swap);
                    if (!Skill_Ids.Contains(legend.Heal)) Skill_Ids.Add(legend.Heal);
                    if (!Skill_Ids.Contains(legend.Elite)) Skill_Ids.Add(legend.Elite);
                    foreach (int id in legend.Utilities)
                    {
                        if (!Skill_Ids.Contains(id)) Skill_Ids.Add(id);
                    }
                }

                foreach (Profession profession in professions)
                {
                    Logger.Debug(string.Format("Checking {0} Skills", profession.Name));
                    foreach (ProfessionSkill skill in profession.Skills)
                    {
                        if (!Skill_Ids.Contains(skill.Id)) Skill_Ids.Add(skill.Id);
                    }
                }
                Logger.Debug(string.Format("Fetching a total of {0} Skills", Skill_Ids.Count));

                //var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(Skill_Ids);
                var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Skills"));

                var stats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
                completed++;
                downloadBar.Progress = completed / totalFetches;
                downloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                Logger.Debug(string.Format("Fetched {0}", "Itemstats"));

                List<API.RuneItem> Runes = new List<API.RuneItem>();
                foreach (ItemUpgradeComponent rune in runes)
                {
                    if (rune != null)
                    {
                        if (rune.Icon != null && rune.Icon.Url != null)
                        {
                            var temp = new API.RuneItem()
                            {
                                Name = rune.Name,
                                Id = rune.Id,
                                ChatLink = rune.ChatLink,
                                Icon = new API.Icon() { Url = rune.Icon.Url.ToString(), Path = Paths.rune_icons.Replace(Paths.BasePath, "") + Regex.Match(rune.Icon, "[0-9]*.png") },
                                Bonuses = rune.Details.Bonuses.ToList(),
                            };
                            Runes.Add(temp);

                            if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Item Icon '{0}'", rune.Name),
                                    url = temp.Icon.Url,
                                    path = Paths.BasePath + temp.Icon.Path,
                                });
                            }

                            total++;
                        }
                    }
                }
                System.IO.File.WriteAllText(Paths.runes + "runes [" + culture + "].json", JsonConvert.SerializeObject(Runes.ToArray(), settings));

                List<API.SigilItem> Sigils = new List<API.SigilItem>();
                foreach (ItemUpgradeComponent sigil in sigils)
                {
                    if (sigil != null)
                    {
                        if (sigil.Icon != null && sigil.Icon.Url != null)
                        {
                            var temp = new API.SigilItem()
                            {
                                Name = sigil.Name,
                                Id = sigil.Id,
                                ChatLink = sigil.ChatLink,
                                Icon = new API.Icon() { Url = sigil.Icon.Url.ToString(), Path = Paths.sigil_icons.Replace(Paths.BasePath, "") + Regex.Match(sigil.Icon, "[0-9]*.png") },
                                Description = sigil.Details.InfixUpgrade.Buff.Description,
                            };
                            Sigils.Add(temp);

                            if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Item Icon '{0}'", sigil.Name),
                                    url = temp.Icon.Url,
                                    path = Paths.BasePath + temp.Icon.Path,
                                });
                            }

                            sigil.HttpResponseInfo = null;
                            total++;
                        }
                    }
                }
                System.IO.File.WriteAllText(Paths.sigils + "sigils [" + culture + "].json", JsonConvert.SerializeObject(Sigils.ToArray(), settings));

                List<API.Stat> Stats = new List<API.Stat>();
                foreach (Itemstat stat in stats)
                {
                    if (stat != null && Enum.GetName(typeof(_EquipmentStats), stat.Id) != null)
                    {
                        {
                            var temp = new API.Stat()
                            {
                                Name = stat.Name,
                                Id = stat.Id,
                                Icon = new API.Icon() { Path = @"textures\stat icons\" + stat.Id + ".png" },
                            };

                            foreach (ItemstatAttribute attribute in stat.Attributes)
                            {
                                temp.Attributes.Add(new API.StatAttribute()
                                {
                                    Id = (int)attribute.Attribute.Value,
                                    Name = API.UniformAttributeName(attribute.Attribute.RawValue),
                                    Multiplier = attribute.Multiplier,
                                    Value = attribute.Value,
                                    Icon = new API.Icon() { Path = @"textures\stats\" + (int)attribute.Attribute.Value + ".png" },
                                });
                            }

                            Stats.Add(temp);
                        }
                    }
                }
                System.IO.File.WriteAllText(Paths.stats + "stats [" + culture + "].json", JsonConvert.SerializeObject(Stats.ToArray(), settings));

                List<API.ArmorItem> Armors = new List<API.ArmorItem>();
                List<API.WeaponItem> Weapons = new List<API.WeaponItem>();
                List<API.TrinketItem> Trinkets = new List<API.TrinketItem>();
                foreach (Item i in armory_items)
                {
                    if (i != null && i.Icon != null && i.Icon.Url != null)
                    {
                        if (i.Type.RawValue == "Armor")
                        {
                            ItemArmor item = (ItemArmor)i;

                            if (item != null)
                            {
                                var temp = new API.ArmorItem()
                                {
                                    Name = item.Name,
                                    Id = item.Id,
                                    ChatLink = item.ChatLink,
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, "") + Regex.Match(item.Icon, "[0-9]*.png") },
                                    AttributeAdjustment = item.Details.AttributeAdjustment,
                                };

                                Enum.TryParse(item.Details.Type.RawValue, out temp.Slot);
                                Enum.TryParse(item.Details.WeightClass.RawValue, out temp.ArmorWeight);
                                Armors.Add(temp);
                            }
                        }

                        if (i.Type.RawValue == "Weapon")
                        {
                            ItemWeapon item = (ItemWeapon)i;

                            if (item != null)
                            {
                                var temp = new API.WeaponItem()
                                {
                                    Name = item.Name,
                                    Id = item.Id,
                                    ChatLink = item.ChatLink,
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, "") + Regex.Match(item.Icon, "[0-9]*.png") },
                                    AttributeAdjustment = item.Details.AttributeAdjustment,
                                };

                                Enum.TryParse(item.Details.Type.RawValue, out temp.Slot);
                                Enum.TryParse(item.Details.Type.RawValue, out temp.WeaponType);
                                Weapons.Add(temp);
                            }
                        }

                        if (i.Type.RawValue == "Trinket")
                        {
                            ItemTrinket item = (ItemTrinket)i;

                            if (item != null)
                            {
                                var temp = new API.TrinketItem()
                                {
                                    Name = item.Name,
                                    Id = item.Id,
                                    ChatLink = item.ChatLink,
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, "") + Regex.Match(item.Icon, "[0-9]*.png") },
                                    AttributeAdjustment = item.Details.AttributeAdjustment,
                                };

                                Enum.TryParse(item.Details.Type.RawValue, out temp.TrinketType);
                                Trinkets.Add(temp);
                            }
                        }

                        if (i.Type.RawValue == "Back")
                        {
                            ItemBack item = (ItemBack)i;

                            if (item != null)
                            {
                                Trinkets.Add(new API.TrinketItem()
                                {
                                    Name = item.Name,
                                    Id = item.Id,
                                    ChatLink = item.ChatLink,
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, "") + Regex.Match(item.Icon, "[0-9]*.png") },
                                    TrinketType = API.trinketType.Back,
                                    AttributeAdjustment = item.Details.AttributeAdjustment,
                                });
                            }
                        }

                        if (!System.IO.File.Exists(Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png")))
                        {
                            downloadList.Add(new APIDownload_Image()
                            {
                                display_text = string.Format("Downloading Item Icon '{0}'", i.Name),
                                url = i.Icon,
                                path = Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png"),
                            });
                        }
                    }
                }
                System.IO.File.WriteAllText(Paths.armory + "armors [" + culture + "].json", JsonConvert.SerializeObject(Armors.ToArray(), settings));
                System.IO.File.WriteAllText(Paths.armory + "weapons [" + culture + "].json", JsonConvert.SerializeObject(Weapons.ToArray(), settings));
                System.IO.File.WriteAllText(Paths.armory + "trinkets [" + culture + "].json", JsonConvert.SerializeObject(Trinkets.ToArray(), settings));

                Logger.Debug("Preparing Traits ....");
                List<API.Trait> Traits = new List<API.Trait>();
                foreach (Trait trait in traits)
                {
                    if (trait != null && trait.Icon != null && trait.Icon.Url != null)
                    {
                        Traits.Add(new API.Trait()
                        {
                            Name = trait.Name,
                            Description = trait.Description,
                            Id = trait.Id,
                            Icon = new API.Icon() { Url = trait.Icon.Url.ToString(), Path = Paths.traits_icons.Replace(Paths.BasePath, "") + Regex.Match(trait.Icon, "[0-9]*.png") },
                            Specialization = trait.Specialization,
                            Tier = trait.Tier,
                            Order = trait.Order,
                            Type = (API.traitType)Enum.Parse(typeof(API.traitType), trait.Slot.RawValue, true),
                        });
                    }
                }

                Logger.Debug("Preparing Specializations ....");
                List<API.Specialization> Specializations = new List<API.Specialization>();
                foreach (Specialization spec in specs)
                {
                    if (spec != null && spec.Icon != null && spec.Icon.Url != null)
                    {
                        var temp = new API.Specialization()
                        {
                            Name = spec.Name,
                            Id = spec.Id,
                            Icon = new API.Icon() { Url = spec.Icon.Url.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, "") + Regex.Match(spec.Icon, "[0-9]*.png") },
                            Background = new API.Icon() { Url = spec.Background.Url.ToString(), Path = Paths.spec_backgrounds.Replace(Paths.BasePath, "") + Regex.Match(spec.Background, "[0-9]*.png") },
                            ProfessionIconBig = spec.ProfessionIconBig != null ? new API.Icon() { Url = spec.ProfessionIconBig.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, "") + Regex.Match(spec.ProfessionIconBig, "[0-9]*.png") } : null,
                            ProfessionIcon = spec.ProfessionIcon != null ? new API.Icon() { Url = spec.ProfessionIcon.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, "") + Regex.Match(spec.ProfessionIcon, "[0-9]*.png") } : null,
                            Profession = spec.Profession,
                            Elite = spec.Elite,
                        };

                        temp.WeaponTrait = Traits.Find(e => e.Id == spec.WeaponTrait);
                        if (temp.WeaponTrait != null && !System.IO.File.Exists(Paths.BasePath + temp.WeaponTrait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Trait Icon '{0}'", temp.WeaponTrait.Name),
                            url = temp.WeaponTrait.Icon.Url,
                            path = Paths.BasePath + temp.WeaponTrait.Icon.Path,
                        });

                        foreach (int id in spec.MinorTraits)
                        {
                            var trait = Traits.Find(e => e.Id == id);

                            if (trait != null)
                            {
                                temp.MinorTraits.Add(trait);
                                if (!System.IO.File.Exists(Paths.BasePath + trait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                    url = trait.Icon.Url,
                                    path = Paths.BasePath + trait.Icon.Path,
                                });
                            }
                        }
                        foreach (int id in spec.MajorTraits)
                        {
                            var trait = Traits.Find(e => e.Id == id);

                            if (trait != null)
                            {
                                temp.MajorTraits.Add(trait);
                                if (!System.IO.File.Exists(Paths.BasePath + trait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                    url = trait.Icon.Url,
                                    path = Paths.BasePath + trait.Icon.Path,
                                });
                            }
                        }

                        Specializations.Add(temp);
                    }
                }

                Logger.Debug("Preparing Skills ....");
                List<API.Skill> Skills = new List<API.Skill>();
                foreach (Skill skill in skills)
                {
                    if (skill != null && skill.Icon != null && skill.Professions.Count == 1)
                    {
                        var temp = new API.Skill()
                        {
                            Name = skill.Name,
                            Id = skill.Id,
                            Icon = new API.Icon() { Url = skill.Icon.ToString(), Path = Paths.skill_icons.Replace(Paths.BasePath, "") + Regex.Match(skill.Icon, "[0-9]*.png") },
                            ChatLink = skill.ChatLink,
                            Description = skill.Description,
                            Specialization = skill.Specialization != null ? (int)skill.Specialization : 0,
                            Flags = skill.Flags.ToList().Select(e => e.RawValue).ToList(),
                            Categories = new List<string>(),
                        };

                        if (skill.Categories != null)
                        {
                            foreach (string category in skill.Categories)
                            {
                                temp.Categories.Add(category);
                            }
                        }

                        Enum.TryParse(skill.Slot.RawValue, out temp.Slot);
                        Skills.Add(temp);
                    }
                }

                Logger.Debug("Preparing Professions ....");
                List<API.Profession> Professions = new List<API.Profession>();
                foreach (Profession profession in professions)
                {
                    if (profession != null && profession.Icon != null && profession.Icon.Url != null)
                    {
                        var temp = new API.Profession()
                        {
                            Name = profession.Name,
                            Id = profession.Id,
                            Icon = new API.Icon() { Url = profession.Icon.Url.ToString(), Path = Paths.profession_icons.Replace(Paths.BasePath, "") + Regex.Match(profession.Icon, "[0-9]*.png") },
                            IconBig = new API.Icon() { Url = profession.IconBig.Url.ToString(), Path = Paths.profession_icons.Replace(Paths.BasePath, "") + Regex.Match(profession.IconBig, "[0-9]*.png") },
                        };

                        Logger.Debug("Adding Specs ....");
                        foreach (int id in profession.Specializations)
                        {
                            var spec = Specializations.Find(e => e.Id == id);
                            if (spec != null)
                            {
                                temp.Specializations.Add(spec);

                                if (!System.IO.File.Exists(Paths.BasePath + spec.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Specialization Icon '{0}'", spec.Name),
                                    url = spec.Icon.Url,
                                    path = Paths.BasePath + spec.Icon.Path,
                                });

                                if (!System.IO.File.Exists(Paths.BasePath + spec.Background.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Background '{0}'", spec.Name),
                                    url = spec.Background.Url,
                                    path = Paths.BasePath + spec.Background.Path,
                                });

                                if (spec.ProfessionIcon != null && !System.IO.File.Exists(Paths.BasePath + spec.ProfessionIcon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading ProfessionIcon '{0}'", spec.Name),
                                    url = spec.ProfessionIcon.Url,
                                    path = Paths.BasePath + spec.ProfessionIcon.Path,
                                });

                                if (spec.ProfessionIconBig != null && !System.IO.File.Exists(Paths.BasePath + spec.ProfessionIconBig.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading ProfessionIconBig '{0}'", spec.Name),
                                    url = spec.ProfessionIconBig.Url,
                                    path = Paths.BasePath + spec.ProfessionIconBig.Path,
                                });
                            }
                        }

                        Logger.Debug("Adding Weapons ....");
                        foreach (KeyValuePair<string, ProfessionWeapon> weapon in profession.Weapons)
                        {
                            API.weaponType weaponType;
                            Enum.TryParse(weapon.Key, out weaponType);

                            temp.Weapons.Add(new API.ProfessionWeapon()
                            {
                                Weapon = weaponType,
                                Specialization = weapon.Value.Specialization,
                                Wielded = weapon.Value.Flags.Select(e => (API.weaponHand)Enum.Parse(typeof(API.weaponHand), e.RawValue)).ToList(),
                            });
                        }

                        Logger.Debug("Adding Skills ....");

                        var SkillID_Pairs = JsonConvert.DeserializeObject<List<iData.SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

                        if (profession.Id == "Revenant")
                        {
                            foreach (iData._Legend legend in legends)
                            {
                                var tempLegend = new API.Legend()
                                {
                                };

                                tempLegend.Name = Skills.Find(e => e.Id == legend.Skill).Name;
                                tempLegend.Skill = Skills.Find(e => e.Id == legend.Skill);
                                tempLegend.Id = legend.Id;
                                tempLegend.Specialization = legend.Specialization;
                                tempLegend.Swap = Skills.Find(e => e.Id == legend.Swap);
                                tempLegend.Heal = Skills.Find(e => e.Id == legend.Heal);
                                tempLegend.Elite = Skills.Find(e => e.Id == legend.Elite);


                                if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Skill.Name),
                                    url = tempLegend.Skill.Icon.Url,
                                    path = Paths.BasePath + tempLegend.Skill.Icon.Path,
                                });

                                if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Swap.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Swap.Name),
                                    url = tempLegend.Swap.Icon.Url,
                                    path = Paths.BasePath + tempLegend.Swap.Icon.Path,
                                });

                                if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Heal.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Heal.Name),
                                    url = tempLegend.Heal.Icon.Url,
                                    path = Paths.BasePath + tempLegend.Heal.Icon.Path,
                                });

                                if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Heal.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Elite.Name),
                                    url = tempLegend.Elite.Icon.Url,
                                    path = Paths.BasePath + tempLegend.Elite.Icon.Path,
                                });


                                tempLegend.Utilities = new List<API.Skill>();

                                foreach (int id in legend.Utilities)
                                {
                                    var skill = Skills.Find(e => e.Id == id);
                                    tempLegend.Utilities.Add(skill);

                                    if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                        url = skill.Icon.Url,
                                        path = Paths.BasePath + skill.Icon.Path,
                                    });
                                }

                                temp.Legends.Add(tempLegend);
                            }


                            foreach (ProfessionSkill iSkill in profession.Skills)
                            {
                                var skill = Skills.Find(e => e.Id == iSkill.Id);
                                var paletteID = SkillID_Pairs.Find(e => e.ID == iSkill.Id);

                                if (skill != null && paletteID != null)
                                {
                                    skill.PaletteId = paletteID.PaletteID;
                                    temp.Skills.Add(skill);

                                    if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                        url = skill.Icon.Url,
                                        path = Paths.BasePath + skill.Icon.Path,
                                    });
                                }
                            }

                            foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette)
                            {
                                var skill = Skills.Find(e => e.Id == skillIDs.Value);
                                if (skill != null && !temp.Skills.Contains(skill))
                                {
                                    skill.PaletteId = skillIDs.Key;
                                    temp.Skills.Add(skill);


                                    if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                        url = skill.Icon.Url,
                                        path = Paths.BasePath + skill.Icon.Path,
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette)
                            {
                                var skill = Skills.Find(e => e.Id == skillIDs.Value);
                                if (skill != null)
                                {
                                    skill.PaletteId = skillIDs.Key;
                                    temp.Skills.Add(skill);


                                    if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                        url = skill.Icon.Url,
                                        path = Paths.BasePath + skill.Icon.Path,
                                    });
                                }
                            }
                        }

                        if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                            url = temp.Icon.Url,
                            path = Paths.BasePath + temp.Icon.Path,
                        });

                        if (!System.IO.File.Exists(Paths.BasePath + temp.IconBig.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                            url = temp.IconBig.Url,
                            path = Paths.BasePath + temp.IconBig.Path,
                        });

                        Professions.Add(temp);
                    }
                }

                Logger.Debug("Saving Professions ....");
                System.IO.File.WriteAllText(Paths.professions + "professions [" + culture + "].json", JsonConvert.SerializeObject(Professions.ToArray(), settings));

                downloadBar.Progress = 0;
                total = downloadList.Count;
                completed = 0;

                Logger.Debug("All required Images queued. Downloading now ....");
                foreach (APIDownload_Image image in downloadList)
                {
                    Logger.Debug("Downloading: '{0}' from url '{1}' to '{2}'", image.display_text, image.url, image.path);
                    var stream = new FileStream(image.path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    await Gw2ApiManager.Gw2ApiClient.Render.DownloadToStreamAsync(stream, image.url);
                    stream.Close();

                    completed++;
                    progress = completed / total;
                    downloadBar.Progress = progress;
                    downloadBar.Text = string.Format("{0} / {1} ({2})", completed, downloadList.Count, Math.Round((progress * 100), 2).ToString() + "%");
                    downloadBar.BasicTooltipText = image.display_text;
                    downloadBar._Label.BasicTooltipText = image.display_text;
                    downloadBar._BackgroundTexture.BasicTooltipText = image.display_text;
                    downloadBar._FilledTexture.BasicTooltipText = image.display_text;
                }

                loadingSpinner.Visible = false;
                downloadBar.Visible = false;

                GameVersion.Value = Gw2MumbleService.Gw2Mumble.Info.Version;
                ModuleVersion.Value = Version.BaseVersion().ToString();
                Logger.Debug("API Data sucessfully fetched!");
                FetchingAPI = false;
            }
        }

        async Task LoadData()
        {
            var culture = BuildsManager.getCultureString();
            await Fetch_APIData(!System.IO.File.Exists(Paths.professions + @"professions [" + culture + "].json"));

            if (Data == null)
            {
                Data = new iData();
            }
            else
            {
                Data.UpdateLanguage();
            }

            OverlayService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
        }

        private async void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e)
        {
            CultureString = BuildsManager.getCultureString();
            OverlayService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;
            await LoadData();

            OnLanguageChanged(null, null);
        }

        private void CreateUI()
        {
            LoadTemplates();
            Selected_Template = new Template();

            var Height = 670;
            var Width = 915;

            MainWindow = new Window_MainWindow(
                TextureManager.getBackground(_Backgrounds.MainWindow),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 15, Width - 3, Height + 25)
                )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Builds Manager",
                Emblem = TextureManager._Emblems[(int)_Emblems.SwordAndShield],
                Subtitle = "v." + Version.BaseVersion().ToString(),
                SavesPosition = true,
                Id = $"BuildsManager New",
            };

            Selected_Template = Selected_Template;

            //MainWindow.ToggleWindow();
        }
    }
}