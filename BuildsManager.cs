using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
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


        public static TextureManager TextureManager;
        public static iPaths Paths;
        public static iData Data; public iTicks Ticks = new iTicks();
        public static List<int> ArmoryItems = new List<int>();

        public SettingEntry<bool> PasteOnCopy;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        public SettingEntry<int> GameVersion;
        public SettingEntry<string> ModuleVersion;

        public List<Template> Templates = new List<Template>();
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

        public event EventHandler Selected_Template_Edit;
        public void OnSelected_Template_Edit(object sender, EventArgs e)
        {
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
            LoadTemplates();
            this.Template_Deleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Templates_Loaded;
        public void OnTemplates_Loaded()
        {
            LoadTemplates();
            this.Templates_Loaded?.Invoke(this, EventArgs.Empty);
        }

        public Window_MainWindow MainWindow;
        public LoadingSpinner loadingSpinner;
        public ProgressBar downloadBar;
        private CornerIcon cornerIcon;

        public event EventHandler DataLoaded_Event;
        private static bool _DataLoaded;
        public static bool DataLoaded
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

            ReloadKey = settings.DefineSetting(nameof(ReloadKey),
                                                      new Blish_HUD.Input.KeyBinding(Keys.LeftControl),
                                                      () => "Reload Button",
                                                      () => "");

            PasteOnCopy = settings.DefineSetting(nameof(PasteOnCopy),
                                                      true,
                                                      () => "Paste Stat/Upgrade Name",
                                                      () => "Paste Stat/Upgrade Name after copying it.");

            var internal_settings = settings.AddSubCollection("Internal Settings", false);
            GameVersion = internal_settings.DefineSetting(nameof(GameVersion), 0);
            ModuleVersion = internal_settings.DefineSetting(nameof(ModuleVersion), "0.0.0");
        }

        protected override void Initialize()
        {            
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
            //ReloadKey.Value.Activated += Value_Activated;

            DataLoaded = false;
        }

        private void Value_Activated(object sender, EventArgs e)
        {
            ScreenNotification.ShowNotification("Rebuilding the UI", ScreenNotification.NotificationType.Warning);
            MainWindow?.Dispose();
            CreateUI();
            MainWindow.ToggleWindow();
        }

        protected override async Task LoadAsync()
        {
        }
        public void LoadTemplates()
        {
            var currentTemplate = _Selected_Template?.Name;

            Templates = new List<Template>();
            var files = Directory.GetFiles(BuildsManager.Paths.builds, "*.json", SearchOption.AllDirectories).ToList();

            files.Sort((a, b) => a.CompareTo(b));

            foreach (string path in files)
            {
                var template = new Template(path);
                Templates.Add(template);

                if (template.Name == currentTemplate) _Selected_Template = template;
            }

            _Selected_Template?.SetChanged();
            OnSelected_Template_Changed();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            TextureManager = new TextureManager(ContentsManager, DirectoriesManager);

            cornerIcon = new CornerIcon()
            {
                Icon = TextureManager.getIcon(_Icons.Template),
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen
            };
            cornerIcon.MouseEntered += delegate
            {
                cornerIcon.Icon = TextureManager.getIcon(_Icons.Template_White);
            };
            cornerIcon.MouseLeft += delegate
            {
                cornerIcon.Icon = TextureManager.getIcon(_Icons.Template);
            };
            cornerIcon.Click += delegate
            {
                if (MainWindow != null) MainWindow.ToggleWindow();
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
            cornerIcon.Moved += delegate
            {
                loadingSpinner.Location = new Point(cornerIcon.Location.X - cornerIcon.Width, cornerIcon.Location.Y + cornerIcon.Height + 5);
                downloadBar.Location = new Point(cornerIcon.Location.X, cornerIcon.Location.Y + cornerIcon.Height + 5 + 3);
            };

            // Base handler must be called
            base.OnModuleLoaded(e);

            DataLoaded_Event += delegate { CreateUI(); };
            LoadData();
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.global += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Ticks.global > 250)
            {
                Ticks.global -= 250;

            }
        }

        protected override void Unload()
        {
            MainWindow.Dispose();
            Data = null;
            TextureManager = null;
            cornerIcon.Dispose();

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
                var downloadList = new List<APIDownload_Image>();
                var culture = getCultureString();

                double total = 0;
                double completed = 0;
                double progress = 0;

                var _runes = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\runes.json")).ReadToEnd());
                var _sigils = JsonConvert.DeserializeObject<List<int>>(new StreamReader(ContentsManager.GetFileStream(@"data\sigils.json")).ReadToEnd());

                Logger.Debug("Fetching all required Data from the API!");
                loadingSpinner.Visible = true;
                downloadBar.Visible = true;
                downloadBar.Progress = progress;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var sigils = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_sigils);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Sigils"));

                var runes = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_runes);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Runes"));

                var armory_items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ArmoryItems);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Armory"));

                var professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Professions"));

                var specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Specs"));

                var traits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Traits"));

                List<int> Skill_Ids = new List<int>();

                foreach (Profession profession in professions)
                {
                    Logger.Debug(string.Format("Checking {0} Skills", profession.Name));
                    foreach (ProfessionSkill skill in profession.Skills)
                    {
                        if(!Skill_Ids.Contains(skill.Id)) Skill_Ids.Add(skill.Id);
                    }
                }
                Logger.Debug(string.Format("Fetching a total of {0} Skills", Skill_Ids.Count));

                var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(Skill_Ids);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
                Logger.Debug(string.Format("Fetched {0}", "Skills"));

                var stats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);
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
                System.IO.File.WriteAllText(Paths.runes + "runes [" + culture + "].json", JsonConvert.SerializeObject(Runes.ToArray()));

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
                System.IO.File.WriteAllText(Paths.sigils + "sigils [" + culture + "].json", JsonConvert.SerializeObject(Sigils.ToArray()));

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
                System.IO.File.WriteAllText(Paths.stats + "stats [" + culture + "].json", JsonConvert.SerializeObject(Stats.ToArray()));

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

                        if(!System.IO.File.Exists(Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png")))
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
                System.IO.File.WriteAllText(Paths.armory + "armors [" + culture + "].json", JsonConvert.SerializeObject(Armors.ToArray()));
                System.IO.File.WriteAllText(Paths.armory + "weapons [" + culture + "].json", JsonConvert.SerializeObject(Weapons.ToArray()));
                System.IO.File.WriteAllText(Paths.armory + "trinkets [" + culture + "].json", JsonConvert.SerializeObject(Trinkets.ToArray()));

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
                    if (skill != null && skill.Icon != null && skill.Icon.Url != null && skill.Professions.Count == 1)
                    {
                        var temp = new API.Skill()
                        {
                            Name = skill.Name,
                            Id = skill.Id,
                            Icon = new API.Icon() { Url = skill.Icon.Url.ToString(), Path = Paths.skill_icons.Replace(Paths.BasePath, "") + Regex.Match(skill.Icon, "[0-9]*.png") },
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

                            temp.Weapons.Add(new API.ProfessionWeapon() {
                                Weapon = weaponType,
                                Specialization = weapon.Value.Specialization,
                                Wielded = weapon.Value.Flags.Select(e => (API.weaponHand) Enum.Parse(typeof(API.weaponHand), e.RawValue)).ToList(),
                            });
                        }

                        Logger.Debug("Adding Skills ....");

                        var SkillID_Pairs = JsonConvert.DeserializeObject<List<iData.SkillID_Pair>>(new StreamReader(ContentsManager.GetFileStream(@"data\skillpalettes.json")).ReadToEnd());

                        if (profession.Id == "Revenant")
                        {
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
                System.IO.File.WriteAllText(Paths.professions + "professions [" + culture + "].json", JsonConvert.SerializeObject(Professions.ToArray()));

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
            }
        }

        async Task LoadData()
        {
            await Fetch_APIData();

            Data = new iData(ContentsManager, DirectoriesManager);
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
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"BuildsManager New",
            };

            Selected_Template = Selected_Template;

            //MainWindow.ToggleWindow();
        }
    }
}