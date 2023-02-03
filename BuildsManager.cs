using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.Controls;
using Kenedia.Modules.BuildsManager.Enums;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
    [Export(typeof(Module))]
    public class BuildsManager : Module
    {
        internal static BuildsManager s_moduleInstance;
        public static readonly Logger Logger = Logger.GetLogger<BuildsManager>();

        #region Service Managers

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        #endregion

        [ImportingConstructor]
        public BuildsManager([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            s_moduleInstance = this;
        }

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public const uint WM_COMMAND = 0x0111;
        public const uint WM_PASTE = 0x0302;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public TextureManager TextureManager;
        public iPaths Paths;
        public Data Data; public iTicks Ticks = new();
        public List<int> ArmoryItems = new();

        public SettingEntry<bool> PasteOnCopy;
        public SettingEntry<bool> ShowCornerIcon;
        public SettingEntry<bool> IncludeDefaultBuilds;
        public SettingEntry<bool> ShowCurrentProfession;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ToggleWindow;
        public SettingEntry<int> GameVersion;
        public SettingEntry<string> ModuleVersion;

        public string CultureString;
        public List<Template> Templates = new();
        public List<Template> DefaultTemplates = new();
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
            Selected_Template_Redraw?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler LanguageChanged;

        public void OnLanguageChanged(object sender, EventArgs e)
        {
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected_Template_Edit;

        public void OnSelected_Template_Edit(object sender, EventArgs e)
        {
            MainWindow.TemplateSelection.RefreshList();
            Selected_Template_Edit?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Selected_Template_Changed;

        public void OnSelected_Template_Changed()
        {
            Selected_Template_Changed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Template_Deleted;

        public void OnTemplate_Deleted()
        {
            Selected_Template = new Template();
            Template_Deleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Templates_Loaded;

        public void OnTemplates_Loaded()
        {
            LoadTemplates();
            Templates_Loaded?.Invoke(this, EventArgs.Empty);
        }

        public Window_MainWindow MainWindow;
        public Texture2D LoadingTexture;
        public LoadingSpinner LoadingSpinner;
        public ProgressBar DownloadBar;
        private CornerIcon _cornerIcon;

        public event EventHandler DataLoaded_Event;

        private bool _dataLoaded;
        public bool FetchingAPI;

        public bool DataLoaded
        {
            get => _dataLoaded;
            set
            {
                _dataLoaded = value;
                if (value)
                {
                    s_moduleInstance.OnDataLoaded();
                }
            }
        }

        private void OnDataLoaded()
        {
            DataLoaded_Event?.Invoke(this, EventArgs.Empty);
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            ToggleWindow = settings.DefineSetting(
                nameof(ToggleWindow),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Ctrl, Keys.B),
                () => "Toggle Window",
                () => "Show / Hide the UI");

            PasteOnCopy = settings.DefineSetting(
                nameof(PasteOnCopy),
                false,
                () => "Paste Stat/Upgrade Name",
                () => "Paste Stat/Upgrade Name after copying it.");

            ShowCornerIcon = settings.DefineSetting(
                nameof(ShowCornerIcon),
                true,
                () => "Show Corner Icon",
                () => "Show / Hide the Corner Icon of this module.");

            IncludeDefaultBuilds = settings.DefineSetting(
                nameof(IncludeDefaultBuilds),
                true,
                () => "Incl. Default Builds",
                () => "Load the default builds from within the module.");

            ShowCurrentProfession = settings.DefineSetting(
                nameof(ShowCurrentProfession),
                true,
                () => "Filter Current Profession",
                () => "Always set the current Profession as an active filter.");

            SettingCollection internal_settings = settings.AddSubCollection("Internal Settings", false);
            GameVersion = internal_settings.DefineSetting(nameof(GameVersion), 0);
            ModuleVersion = internal_settings.DefineSetting(nameof(ModuleVersion), "0.0.0");

            ReloadKey = internal_settings.DefineSetting(
                nameof(ReloadKey),
                new Blish_HUD.Input.KeyBinding(Keys.None),
                () => "Reload Button",
                () => string.Empty);
        }

        protected override void Initialize()
        {
            CultureString = BuildsManager.getCultureString();
            Logger.Info("Starting Builds Manager v." + Version.BaseVersion());
            Paths = new iPaths(DirectoriesManager.GetFullDirectoryPath("builds-manager"));
            ArmoryItems.AddRange(new int[] {
                  80248, // Perfected Envoy armor (light) Head
                  80131, // Perfected Envoy armor (light) Shoulder
                  80190, // Perfected Envoy armor (light) Chest
                  80111, // Perfected Envoy armor (light) Hands
                  80356, // Perfected Envoy armor (light) Legs
                  80399, // Perfected Envoy armor (light) Feet

                  80296, // Perfected Envoy armor (medium) Head
                  80145, // Perfected Envoy armor (medium) Shoulder
                  80578, // Perfected Envoy armor (medium) Chest
                  80161, // Perfected Envoy armor (medium) Hands
                  80252, // Perfected Envoy armor (medium) Legs
                  80281, // Perfected Envoy armor (medium) Feet

                  80384, // Perfected Envoy armor (heavy) Head
                  80435, // Perfected Envoy armor (heavy) Shoulder
                  80254, // Perfected Envoy armor (heavy) Chest
                  80205, // Perfected Envoy armor (heavy) Hands
                  80277, // Perfected Envoy armor (heavy) Legs
                  80557, // Perfected Envoy armor (heavy) Feet

                  91505, // Legendary Sigil
                  91536, // Legendary Rune

                  81908, // Legendary Accessory Aurora
                  91048, // Legendary Accessory Vision
                  91234, // Legendary Ring Coalescence
                  93105, // Legendary Ring Conflux
                  95380, // Legendary Amulet Prismatic Champion's Regalia

                  74155, // Legendary Backpack Ad Infinitum

                  30698, // The Bifrost
                  30699, // Bolt
                  30686, // The Dreamer
                  30696, // The Flameseeker Prophecies
                  30697, // Frenzy
                  30695, // Meteorlogicus
                  30684, // Frostfang
                  30702, // Howler
                  30687, // Incinerator
                  30690, // The Juggernaut
                  30685, // Kudzu
                  30701, // Kraitkin
                  30691, // Kamohoali'i Kotaki
                  30688, // The Minstrel
                  30692, // The Moot
                  30694, // The Predator
                  30693, // Quip
                  30700, // Rodgort

                  // 30703, //Sunrise
                  // 30704, //Twilight
                  30689, // Eternity
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
                foreach (Template t in DefaultTemplates)
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

            MainWindow.TemplateSelection.Refresh();
        }

        public API.Profession CurrentProfession;
        public API.Specialization CurrentSpecialization;

        private void PlayerCharacter_SpecializationChanged(object sender, ValueEventArgs<int> eventArgs)
        {
            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (player != null && player.Profession > 0 && Data != null && MainWindow != null)
            {
                CurrentProfession = Data.Professions.Find(e => e.Id == player.Profession.ToString());
                CurrentSpecialization = CurrentProfession?.Specializations.Find(e => e.Id == player.Specialization);

                MainWindow.PlayerCharacter_NameChanged(null, null);

                Templates = Templates.OrderBy(a => a.Build?.Profession != BuildsManager.s_moduleInstance.CurrentProfession).ThenBy(a => a.Profession.Id).ThenBy(b => b.Specialization?.Id).ThenBy(b => b.Name).ToList();
                MainWindow.TemplateSelection.RefreshList();
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
            await Task.Delay(0);
        }

        public void ImportTemplates()
        {
            Template saveTemplate = null;
            if (System.IO.File.Exists(Paths.builds + "config.ini"))
            {
                string iniContent = System.IO.File.ReadAllText(Paths.builds + "config.ini");
                bool started = false;
                foreach (string s in iniContent.Split('\n'))
                {
                    if (s.Trim() != string.Empty)
                    {
                        if (started && s.StartsWith("["))
                        {
                            break;
                        }

                        if (started)
                        {
                            string[] buildpadBuild = s.Trim().Split('|');

                            Template template = new()
                            {
                                Template_json = new Template_json()
                                {
                                    Name = buildpadBuild[5],
                                    BuildCode = buildpadBuild[1],
                                }
                            };

                            if (Templates.Find(e => e.Template_json.Name == template.Template_json.Name && e.Template_json.BuildCode == template.Template_json.BuildCode) == null)
                            {
                                template.Build = new Models.BuildTemplate(buildpadBuild[1]);
                                template.Name = buildpadBuild[5];

                                template.Profession = template.Build.Profession;
                                template.Specialization = template.Build.SpecLines.Find(e => e.Specialization?.Elite == true)?.Specialization;

                                Logger.Debug("Adding new template: '{0}'", template.Template_json.Name);
                                Templates.Add(template);
                                saveTemplate = template;
                            }
                        }

                        if (!started && s.StartsWith("[Builds]"))
                        {
                            started = true;
                        }
                    }
                }

                System.IO.File.Delete(Paths.builds + "config.ini");
            }

            List<string> files = Directory.GetFiles(Paths.builds, "*.json", SearchOption.TopDirectoryOnly).ToList();
            if (files.Contains(Paths.builds + "Builds.json"))
            {
                files.Remove(Paths.builds + "Builds.json");
            }

            foreach (string path in files)
            {
                Template template = new(path);
                System.IO.File.Delete(path);
                Templates.Add(template);
                saveTemplate = template;

                if (path == files[files.Count - 1])
                {
                    template.Save();
                }
            }

            BuildsManager.Logger.Debug("Saving {0} Templates.", Templates.Count);
            saveTemplate?.Save();
        }

        public void LoadTemplates()
        {
            string currentTemplate = _Selected_Template?.Name;

            ImportTemplates();

            Templates = new List<Template>();
            List<string> paths = new()
            {
                Paths.builds + "Builds.json",

                // Paths.builds + "Default Builds.json",
            };

            foreach (string path in paths)
            {
                string content = string.Empty;
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        content = System.IO.File.ReadAllText(path);
                    }

                    if (content != null && content != string.Empty)
                    {
                        List<Template_json> templates = JsonConvert.DeserializeObject<List<Template_json>>(content);

                        foreach (Template_json jsonTemplate in templates)
                        {
                            Template template = new(jsonTemplate.Name, jsonTemplate.BuildCode, jsonTemplate.GearCode)
                            {
                                Path = path
                            };

                            Templates.Add(template);

                            if (template.Name == currentTemplate)
                            {
                                _Selected_Template = template;
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            List<Template_json> defaultTemps = JsonConvert.DeserializeObject<List<Template_json>>(new StreamReader(ContentsManager.GetFileStream(@"data\builds.json")).ReadToEnd());

            if (defaultTemps != null)
            {
                foreach (Template_json jsonTemplate in defaultTemps)
                {
                    Template template = new(jsonTemplate.Name, jsonTemplate.BuildCode, jsonTemplate.GearCode)
                    {
                        Path = null
                    };
                    DefaultTemplates.Add(template);
                    if (IncludeDefaultBuilds.Value)
                    {
                        Templates.Add(template);
                    }

                    if (template.Name == currentTemplate)
                    {
                        _Selected_Template = template;
                    }
                }
            }

            // Templates = Templates.OrderBy(a => a.Build?.Profession != BuildsManager.ModuleInstance.CurrentProfession).ThenBy(a => a.Profession.Id).ThenBy(b => b.Specialization?.Id).ThenBy(b => b.Name).ToList();

            _Selected_Template?.SetChanged();
            OnSelected_Template_Changed();
        }

        protected override async void OnModuleLoaded(EventArgs e)
        {
            TextureManager = new TextureManager();

            _cornerIcon = new CornerIcon()
            {
                Icon = TextureManager.getIcon(Icons.Template),
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen,
                Visible = ShowCornerIcon.Value,
            };

            LoadingSpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Location.X - _cornerIcon.Width, _cornerIcon.Location.Y + _cornerIcon.Height + 5),
                Size = _cornerIcon.Size,
                Visible = false,
                Parent = GameService.Graphics.SpriteScreen,
            };
            DownloadBar = new ProgressBar()
            {
                Location = new Point(_cornerIcon.Location.X, _cornerIcon.Location.Y + _cornerIcon.Height + 5 + 3),
                Size = new Point(150, _cornerIcon.Height - 6),
                Parent = GameService.Graphics.SpriteScreen,
                Progress = 0.66,
                Visible = false,
            };

            _cornerIcon.MouseEntered += CornerIcon_MouseEntered;
            _cornerIcon.MouseLeft += CornerIcon_MouseLeft;
            _cornerIcon.Click += CornerIcon_Click;
            _cornerIcon.Moved += CornerIcon_Moved;
            ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
            DataLoaded_Event += BuildsManager_DataLoaded_Event;

            // Base handler must be called
            base.OnModuleLoaded(e);

            await LoadData();
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            LoadingSpinner.Location = new Point(_cornerIcon.Location.X - _cornerIcon.Width, _cornerIcon.Location.Y + _cornerIcon.Height + 5);
            DownloadBar.Location = new Point(_cornerIcon.Location.X, _cornerIcon.Location.Y + _cornerIcon.Height + 5 + 3);
        }

        private void BuildsManager_DataLoaded_Event(object sender, EventArgs e)
        {
            CreateUI();
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void CornerIcon_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _cornerIcon.Icon = TextureManager.getIcon(Icons.Template);
        }

        private void CornerIcon_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _cornerIcon.Icon = TextureManager.getIcon(Icons.Template_White);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (_cornerIcon != null)
            {
                _cornerIcon.Visible = e.NewValue;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.global += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Ticks.global > 1250)
            {
                Ticks.global -= 1250;

                if (CurrentProfession == null)
                {
                    PlayerCharacter_SpecializationChanged(null, null);
                }

                if (MainWindow?.Visible == true)
                {
                    MainWindow.ImportButton.Visible = System.IO.File.Exists(Paths.builds + "config.ini");
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

            if (_Selected_Template != null) {
                _Selected_Template.Edit -= OnSelected_Template_Edit;
            }

            Selected_Template = null;
            CurrentProfession = null;
            CurrentSpecialization = null;

            Data?.Dispose();
            Data = null;

            TextureManager = null;

            ToggleWindow.Value.Enabled = false;
            ToggleWindow.Value.Activated -= ToggleWindow_Activated;

            ReloadKey.Value.Enabled = false;
            ReloadKey.Value.Activated -= ReloadKey_Activated;

            _cornerIcon?.Dispose();
            if (_cornerIcon != null)
            {
                _cornerIcon.MouseEntered -= CornerIcon_MouseEntered;
                _cornerIcon.MouseLeft -= CornerIcon_MouseLeft;
                _cornerIcon.Click -= CornerIcon_Click;
                _cornerIcon.Moved -= CornerIcon_Moved;
            }

            DataLoaded_Event -= BuildsManager_DataLoaded_Event;
            ShowCornerIcon.SettingChanged -= ShowCornerIcon_SettingChanged;
            IncludeDefaultBuilds.SettingChanged -= IncludeDefaultBuilds_SettingChanged;
            GameService.Gw2Mumble.PlayerCharacter.SpecializationChanged -= PlayerCharacter_SpecializationChanged;
            OverlayService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;

            DownloadBar?.Dispose();
            DownloadBar = null;

            DataLoaded = false;
            s_moduleInstance = null;
        }

        public static string getCultureString()
        {
            string culture = OverlayService.Overlay.UserLocale.Value switch
            {
                Gw2Sharp.WebApi.Locale.French => "fr-FR",
                Gw2Sharp.WebApi.Locale.Spanish => "es-ES",
                Gw2Sharp.WebApi.Locale.German => "de-DE",
                _ => "en-EN",
            };
            return culture;
        }

        public async Task<bool> Fetch_APIData(bool force = false)
        {
            if (GameVersion.Value != Gw2MumbleService.Gw2Mumble.Info.Version || ModuleVersion.Value != Version.BaseVersion().ToString() || force == true || false)
            {
                FetchingAPI = true;

                List<APIDownload_Image> downloadList = new();
                string culture = getCultureString();

                double total = 0;
                double completed = 0;
                double progress = 0;

                List<int> _runes = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\runes.json")).ReadToEnd());
                List<int> _sigils = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\sigils.json")).ReadToEnd());

                JsonSerializerSettings settings = new()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                };

                int totalFetches = 9;

                Logger.Debug("Fetching all required Data from the API!");
                LoadingSpinner.Visible = true;
                DownloadBar.Visible = true;
                DownloadBar.Progress = progress;
                DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);

                try
                {
                    IReadOnlyList<Item> sigils = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_sigils);
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Sigils"));

                    IReadOnlyList<Item> runes = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_runes);
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Runes"));

                    IReadOnlyList<Item> armory_items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ArmoryItems);
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Armory"));

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Profession> professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Professions"));

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Specialization> specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Specs"));

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Trait> traits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Traits"));

                    List<int> Skill_Ids = new();
                    List<Data.Legend> legends = JsonConvert.DeserializeObject<List<Data.Legend>>(new StreamReader(ContentsManager.GetFileStream(@"data\legends.json")).ReadToEnd());

                    foreach (Data.Legend legend in legends)
                    {
                        if (!Skill_Ids.Contains(legend.Skill))
                        {
                            Skill_Ids.Add(legend.Skill);
                        }

                        if (!Skill_Ids.Contains(legend.Swap))
                        {
                            Skill_Ids.Add(legend.Swap);
                        }

                        if (!Skill_Ids.Contains(legend.Heal))
                        {
                            Skill_Ids.Add(legend.Heal);
                        }

                        if (!Skill_Ids.Contains(legend.Elite))
                        {
                            Skill_Ids.Add(legend.Elite);
                        }

                        foreach (int id in legend.Utilities)
                        {
                            if (!Skill_Ids.Contains(id))
                            {
                                Skill_Ids.Add(id);
                            }
                        }
                    }

                    foreach (Profession profession in professions)
                    {
                        Logger.Debug(string.Format("Checking {0} Skills", profession.Name));
                        foreach (ProfessionSkill skill in profession.Skills)
                        {
                            if (!Skill_Ids.Contains(skill.Id))
                            {
                                Skill_Ids.Add(skill.Id);
                            }
                        }
                    }

                    Logger.Debug(string.Format("Fetching a total of {0} Skills", Skill_Ids.Count));

                    // var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(Skill_Ids);
                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Skill> skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Skills"));

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Itemstat> stats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
                    completed++;
                    DownloadBar.Progress = completed / totalFetches;
                    DownloadBar.Text = string.Format("{0} / {1}", completed, totalFetches);
                    Logger.Debug(string.Format("Fetched {0}", "Itemstats"));

                    List<API.RuneItem> Runes = new();
                    foreach (ItemUpgradeComponent rune in runes)
                    {
                        if (rune != null)
                        {
                            if (rune.Icon != null && rune.Icon.Url != null)
                            {
                                API.RuneItem temp = new()
                                {
                                    Name = rune.Name,
                                    Id = rune.Id,
                                    ChatLink = rune.ChatLink,
                                    Icon = new API.Icon() { Url = rune.Icon.Url.ToString(), Path = Paths.rune_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(rune.Icon, "[0-9]*.png") },
                                    Bonuses = rune.Details.Bonuses.ToList(),
                                };
                                Runes.Add(temp);

                                if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path))
                                {
                                    downloadList.Add(new APIDownload_Image()
                                    {
                                        Display_text = string.Format("Downloading Item Icon '{0}'", rune.Name),
                                        Url = temp.Icon.Url,
                                        Path = Paths.BasePath + temp.Icon.Path,
                                    });
                                }

                                total++;
                            }
                        }
                    }

                    System.IO.File.WriteAllText(Paths.runes + "runes [" + culture + "].json", JsonConvert.SerializeObject(Runes.ToArray(), settings));

                    List<API.SigilItem> Sigils = new();
                    foreach (ItemUpgradeComponent sigil in sigils)
                    {
                        if (sigil != null)
                        {
                            if (sigil.Icon != null && sigil.Icon.Url != null)
                            {
                                API.SigilItem temp = new()
                                {
                                    Name = sigil.Name,
                                    Id = sigil.Id,
                                    ChatLink = sigil.ChatLink,
                                    Icon = new API.Icon() { Url = sigil.Icon.Url.ToString(), Path = Paths.sigil_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(sigil.Icon, "[0-9]*.png") },
                                    Description = sigil.Details.InfixUpgrade.Buff.Description,
                                };
                                Sigils.Add(temp);

                                if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path))
                                {
                                    downloadList.Add(new APIDownload_Image()
                                    {
                                        Display_text = string.Format("Downloading Item Icon '{0}'", sigil.Name),
                                        Url = temp.Icon.Url,
                                        Path = Paths.BasePath + temp.Icon.Path,
                                    });
                                }

                                sigil.HttpResponseInfo = null;
                                total++;
                            }
                        }
                    }

                    System.IO.File.WriteAllText(Paths.sigils + "sigils [" + culture + "].json", JsonConvert.SerializeObject(Sigils.ToArray(), settings));

                    List<API.Stat> Stats = new();
                    foreach (Itemstat stat in stats)
                    {
                        if (stat != null && Enum.GetName(typeof(EquipmentStats), stat.Id) != null)
                        {
                            {
                                API.Stat temp = new()
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

                    List<API.ArmorItem> Armors = new();
                    List<API.WeaponItem> Weapons = new();
                    List<API.TrinketItem> Trinkets = new();
                    foreach (Item i in armory_items)
                    {
                        if (i != null && i.Icon != null && i.Icon.Url != null)
                        {
                            if (i.Type.RawValue == "Armor")
                            {
                                ItemArmor item = (ItemArmor)i;

                                if (item != null)
                                {
                                    API.ArmorItem temp = new()
                                    {
                                        Name = item.Name,
                                        Id = item.Id,
                                        ChatLink = item.ChatLink,
                                        Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                    API.WeaponItem temp = new()
                                    {
                                        Name = item.Name,
                                        Id = item.Id,
                                        ChatLink = item.ChatLink,
                                        Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                    API.TrinketItem temp = new()
                                    {
                                        Name = item.Name,
                                        Id = item.Id,
                                        ChatLink = item.ChatLink,
                                        Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                        Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(item.Icon, "[0-9]*.png") },
                                        TrinketType = API.trinketType.Back,
                                        AttributeAdjustment = item.Details.AttributeAdjustment,
                                    });
                                }
                            }

                            if (!System.IO.File.Exists(Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png")))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    Display_text = string.Format("Downloading Item Icon '{0}'", i.Name),
                                    Url = i.Icon,
                                    Path = Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png"),
                                });
                            }
                        }
                    }

                    System.IO.File.WriteAllText(Paths.armory + "armors [" + culture + "].json", JsonConvert.SerializeObject(Armors.ToArray(), settings));
                    System.IO.File.WriteAllText(Paths.armory + "weapons [" + culture + "].json", JsonConvert.SerializeObject(Weapons.ToArray(), settings));
                    System.IO.File.WriteAllText(Paths.armory + "trinkets [" + culture + "].json", JsonConvert.SerializeObject(Trinkets.ToArray(), settings));

                    Logger.Debug("Preparing Traits ....");
                    List<API.Trait> Traits = new();
                    foreach (Trait trait in traits)
                    {
                        if (trait != null && trait.Icon != null && trait.Icon.Url != null)
                        {
                            Traits.Add(new API.Trait()
                            {
                                Name = trait.Name,
                                Description = trait.Description,
                                Id = trait.Id,
                                Icon = new API.Icon() { Url = trait.Icon.Url.ToString(), Path = Paths.traits_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(trait.Icon, "[0-9]*.png") },
                                Specialization = trait.Specialization,
                                Tier = trait.Tier,
                                Order = trait.Order,
                                Type = (API.traitType)Enum.Parse(typeof(API.traitType), trait.Slot.RawValue, true),
                            });
                        }
                    }

                    Logger.Debug("Preparing Specializations ....");
                    List<API.Specialization> Specializations = new();
                    foreach (Specialization spec in specs)
                    {
                        if (spec != null && spec.Icon != null && spec.Icon.Url != null)
                        {
                            API.Specialization temp = new()
                            {
                                Name = spec.Name,
                                Id = spec.Id,
                                Icon = new API.Icon() { Url = spec.Icon.Url.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(spec.Icon, "[0-9]*.png") },
                                Background = new API.Icon() { Url = spec.Background.Url.ToString(), Path = Paths.spec_backgrounds.Replace(Paths.BasePath, string.Empty) + Regex.Match(spec.Background, "[0-9]*.png") },
                                ProfessionIconBig = spec.ProfessionIconBig != null ? new API.Icon() { Url = spec.ProfessionIconBig.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(spec.ProfessionIconBig, "[0-9]*.png") } : null,
                                ProfessionIcon = spec.ProfessionIcon != null ? new API.Icon() { Url = spec.ProfessionIcon.ToString(), Path = Paths.spec_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(spec.ProfessionIcon, "[0-9]*.png") } : null,
                                Profession = spec.Profession,
                                Elite = spec.Elite,
                                WeaponTrait = Traits.Find(e => e.Id == spec.WeaponTrait)
                            };
                            if (temp.WeaponTrait != null && !System.IO.File.Exists(Paths.BasePath + temp.WeaponTrait.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    Display_text = string.Format("Downloading Trait Icon '{0}'", temp.WeaponTrait.Name),
                                    Url = temp.WeaponTrait.Icon.Url,
                                    Path = Paths.BasePath + temp.WeaponTrait.Icon.Path,
                                });
                            }

                            foreach (int id in spec.MinorTraits)
                            {
                                API.Trait trait = Traits.Find(e => e.Id == id);

                                if (trait != null)
                                {
                                    temp.MinorTraits.Add(trait);
                                    if (!System.IO.File.Exists(Paths.BasePath + trait.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                            Url = trait.Icon.Url,
                                            Path = Paths.BasePath + trait.Icon.Path,
                                        });
                                    }
                                }
                            }

                            foreach (int id in spec.MajorTraits)
                            {
                                API.Trait trait = Traits.Find(e => e.Id == id);

                                if (trait != null)
                                {
                                    temp.MajorTraits.Add(trait);
                                    if (!System.IO.File.Exists(Paths.BasePath + trait.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                            Url = trait.Icon.Url,
                                            Path = Paths.BasePath + trait.Icon.Path,
                                        });
                                    }
                                }
                            }

                            Specializations.Add(temp);
                        }
                    }

                    Logger.Debug("Preparing Skills ....");
                    List<API.Skill> Skills = new();
                    foreach (Skill skill in skills)
                    {
                        if (skill != null && skill.Icon != null && skill.Professions.Count == 1)
                        {
                            API.Skill temp = new()
                            {
                                Name = skill.Name,
                                Id = skill.Id,
                                Icon = new API.Icon() { Url = skill.Icon.ToString(), Path = Paths.skill_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(skill.Icon, "[0-9]*.png") },
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
                    List<API.Profession> Professions = new();
                    foreach (Profession profession in professions)
                    {
                        if (profession != null && profession.Icon != null && profession.Icon.Url != null)
                        {
                            API.Profession temp = new()
                            {
                                Name = profession.Name,
                                Id = profession.Id,
                                Icon = new API.Icon() { Url = profession.Icon.Url.ToString(), Path = Paths.profession_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(profession.Icon, "[0-9]*.png") },
                                IconBig = new API.Icon() { Url = profession.IconBig.Url.ToString(), Path = Paths.profession_icons.Replace(Paths.BasePath, string.Empty) + Regex.Match(profession.IconBig, "[0-9]*.png") },
                            };

                            Logger.Debug("Adding Specs ....");
                            foreach (int id in profession.Specializations)
                            {
                                API.Specialization spec = Specializations.Find(e => e.Id == id);
                                if (spec != null)
                                {
                                    temp.Specializations.Add(spec);

                                    if (!System.IO.File.Exists(Paths.BasePath + spec.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Specialization Icon '{0}'", spec.Name),
                                            Url = spec.Icon.Url,
                                            Path = Paths.BasePath + spec.Icon.Path,
                                        });
                                    }

                                    if (!System.IO.File.Exists(Paths.BasePath + spec.Background.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Background '{0}'", spec.Name),
                                            Url = spec.Background.Url,
                                            Path = Paths.BasePath + spec.Background.Path,
                                        });
                                    }

                                    if (spec.ProfessionIcon != null && !System.IO.File.Exists(Paths.BasePath + spec.ProfessionIcon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading ProfessionIcon '{0}'", spec.Name),
                                            Url = spec.ProfessionIcon.Url,
                                            Path = Paths.BasePath + spec.ProfessionIcon.Path,
                                        });
                                    }

                                    if (spec.ProfessionIconBig != null && !System.IO.File.Exists(Paths.BasePath + spec.ProfessionIconBig.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading ProfessionIconBig '{0}'", spec.Name),
                                            Url = spec.ProfessionIconBig.Url,
                                            Path = Paths.BasePath + spec.ProfessionIconBig.Path,
                                        });
                                    }
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

                            List<Data.SkillID_Pair> SkillID_Pairs = JsonConvert.DeserializeObject<List<Data.SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

                            if (profession.Id == "Revenant")
                            {
                                foreach (Data.Legend legend in legends)
                                {
                                    API.Legend tempLegend = new()
                                    {
                                        Name = Skills.Find(e => e.Id == legend.Skill).Name,
                                        Skill = Skills.Find(e => e.Id == legend.Skill),
                                        Id = legend.Id,
                                        Specialization = legend.Specialization,
                                        Swap = Skills.Find(e => e.Id == legend.Swap),
                                        Heal = Skills.Find(e => e.Id == legend.Heal),
                                        Elite = Skills.Find(e => e.Id == legend.Elite)
                                    };

                                    if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Skill.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Skill.Name),
                                            Url = tempLegend.Skill.Icon.Url,
                                            Path = Paths.BasePath + tempLegend.Skill.Icon.Path,
                                        });
                                    }

                                    if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Swap.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Swap.Name),
                                            Url = tempLegend.Swap.Icon.Url,
                                            Path = Paths.BasePath + tempLegend.Swap.Icon.Path,
                                        });
                                    }

                                    if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Heal.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Heal.Name),
                                            Url = tempLegend.Heal.Icon.Url,
                                            Path = Paths.BasePath + tempLegend.Heal.Icon.Path,
                                        });
                                    }

                                    if (!System.IO.File.Exists(Paths.BasePath + tempLegend.Heal.Icon.Path))
                                    {
                                        downloadList.Add(new APIDownload_Image()
                                        {
                                            Display_text = string.Format("Downloading Skill Icon '{0}'", tempLegend.Elite.Name),
                                            Url = tempLegend.Elite.Icon.Url,
                                            Path = Paths.BasePath + tempLegend.Elite.Icon.Path,
                                        });
                                    }

                                    tempLegend.Utilities = new List<API.Skill>();

                                    foreach (int id in legend.Utilities)
                                    {
                                        API.Skill skill = Skills.Find(e => e.Id == id);
                                        tempLegend.Utilities.Add(skill);

                                        if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path))
                                        {
                                            downloadList.Add(new APIDownload_Image()
                                            {
                                                Display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                                Url = skill.Icon.Url,
                                                Path = Paths.BasePath + skill.Icon.Path,
                                            });
                                        }
                                    }

                                    temp.Legends.Add(tempLegend);
                                }

                                foreach (ProfessionSkill iSkill in profession.Skills)
                                {
                                    API.Skill skill = Skills.Find(e => e.Id == iSkill.Id);
                                    Data.SkillID_Pair paletteID = SkillID_Pairs.Find(e => e.ID == iSkill.Id);

                                    if (skill != null && paletteID != null)
                                    {
                                        skill.PaletteId = paletteID.PaletteID;
                                        temp.Skills.Add(skill);

                                        if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path))
                                        {
                                            downloadList.Add(new APIDownload_Image()
                                            {
                                                Display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                                Url = skill.Icon.Url,
                                                Path = Paths.BasePath + skill.Icon.Path,
                                            });
                                        }
                                    }
                                }

                                foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette)
                                {
                                    API.Skill skill = Skills.Find(e => e.Id == skillIDs.Value);
                                    if (skill != null && !temp.Skills.Contains(skill))
                                    {
                                        skill.PaletteId = skillIDs.Key;
                                        temp.Skills.Add(skill);

                                        if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path))
                                        {
                                            downloadList.Add(new APIDownload_Image()
                                            {
                                                Display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                                Url = skill.Icon.Url,
                                                Path = Paths.BasePath + skill.Icon.Path,
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette)
                                {
                                    API.Skill skill = Skills.Find(e => e.Id == skillIDs.Value);
                                    if (skill != null)
                                    {
                                        skill.PaletteId = skillIDs.Key;
                                        temp.Skills.Add(skill);

                                        if (!System.IO.File.Exists(Paths.BasePath + skill.Icon.Path))
                                        {
                                            downloadList.Add(new APIDownload_Image()
                                            {
                                                Display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                                Url = skill.Icon.Url,
                                                Path = Paths.BasePath + skill.Icon.Path,
                                            });
                                        }
                                    }
                                }
                            }

                            if (!System.IO.File.Exists(Paths.BasePath + temp.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    Display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                                    Url = temp.Icon.Url,
                                    Path = Paths.BasePath + temp.Icon.Path,
                                });
                            }

                            if (!System.IO.File.Exists(Paths.BasePath + temp.IconBig.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    Display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                                    Url = temp.IconBig.Url,
                                    Path = Paths.BasePath + temp.IconBig.Path,
                                });
                            }

                            Professions.Add(temp);
                        }
                    }

                    Logger.Debug("Saving Professions ....");
                    System.IO.File.WriteAllText(Paths.professions + "professions [" + culture + "].json", JsonConvert.SerializeObject(Professions.ToArray(), settings));

                    DownloadBar.Progress = 0;
                    total = downloadList.Count;
                    completed = 0;

                    Logger.Debug("All required Images queued. Downloading now ....");
                    foreach (APIDownload_Image image in downloadList)
                    {
                        Logger.Debug("Downloading: '{0}' from url '{1}' to '{2}'", image.Display_text, image.Url, image.Path);
                        FileStream stream = new(image.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                        await Gw2ApiManager.Gw2ApiClient.Render.DownloadToStreamAsync(stream, image.Url);
                        stream.Close();

                        completed++;
                        progress = completed / total;
                        DownloadBar.Progress = progress;
                        DownloadBar.Text = string.Format("{0} / {1} ({2})", completed, downloadList.Count, Math.Round(progress * 100, 2).ToString() + "%");
                        DownloadBar.BasicTooltipText = image.Display_text;
                        DownloadBar._Label.BasicTooltipText = image.Display_text;
                        DownloadBar._BackgroundTexture.BasicTooltipText = image.Display_text;
                        DownloadBar._FilledTexture.BasicTooltipText = image.Display_text;
                    }

                    LoadingSpinner.Visible = false;
                    DownloadBar.Visible = false;

                    GameVersion.Value = Gw2MumbleService.Gw2Mumble.Info.Version;
                    ModuleVersion.Value = Version.BaseVersion().ToString();
                    Logger.Debug("API Data sucessfully fetched!");
                    FetchingAPI = false;
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Debug("Fetching API Data failed!");
                    LoadingSpinner.Visible = false;
                    DownloadBar.Visible = false;
                    return false;
                }
            }

            return false;
        }

        private async Task LoadData()
        {
            string culture = BuildsManager.getCultureString();
            if (await Fetch_APIData(!System.IO.File.Exists(Paths.professions + @"professions [" + culture + "].json")))
            {

                if (Data == null)
                {
                    Data = new Data();
                }
                else
                {
                    Data.UpdateLanguage();
                }

                OverlayService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;
            }
            else
            {
                Unload();
            }
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

            int Height = 670;
            int Width = 915;

            MainWindow = new Window_MainWindow(
                TextureManager.getBackground(Backgrounds.MainWindow),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 15, Width - 3, Height + 25))
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Builds Manager",
                Emblem = TextureManager._Emblems[(int)Emblems.SwordAndShield],
                Subtitle = "v." + Version.BaseVersion().ToString(),
                SavesPosition = true,
                Id = $"BuildsManager New",
            };

            Selected_Template = Selected_Template;

            // MainWindow.ToggleWindow();
        }
    }
}