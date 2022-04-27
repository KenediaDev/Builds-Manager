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

        public static TextureManager TextureManager;
        public static iPaths Paths;
        public static iData Data; public iTicks Ticks = new iTicks();
        public static List<int> ArmoryItems = new List<int>();

        private SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        private SettingEntry<int> GameVersion;

        public iMainWindow MainWindow;
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

            GameVersion = settings.DefineSetting(nameof(GameVersion), 0);
        }

        protected override void Initialize()
        {
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
            ReloadKey.Value.Activated += delegate
            {
                ScreenNotification.ShowNotification("Rebuilding UI!", ScreenNotification.NotificationType.Error);
                MainWindow.Dispose();
                CreateUI();
                MainWindow.Show();
            };

            DataLoaded = false;
        }

        protected override async Task LoadAsync()
        {
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

        private async Task Fetch_APIData()
        {
            if (GameVersion.Value != Gw2MumbleService.Gw2Mumble.Info.Version || false)
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

                var runes = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_runes);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var armory_items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ArmoryItems);
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var traits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

                var stats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
                completed++;
                downloadBar.Progress = completed / 9;
                downloadBar.Text = string.Format("{0} / 9", completed);

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
                                Icon = new API.Icon() { Url = rune.Icon.Url.ToString(), Path = Paths.rune_icons + Regex.Match(rune.Icon, "[0-9]*.png") },
                                Bonuses = rune.Details.Bonuses.ToList(),
                            };
                            Runes.Add(temp);

                            if (!System.IO.File.Exists(temp.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Item Icon '{0}'", rune.Name),
                                    url = temp.Icon.Url,
                                    path = temp.Icon.Path,
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
                                Icon = new API.Icon() { Url = sigil.Icon.Url.ToString(), Path = Paths.sigil_icons + Regex.Match(sigil.Icon, "[0-9]*.png") },
                                Description = sigil.Details.InfixUpgrade.Buff.Description,
                            };
                            Sigils.Add(temp);

                            if (!System.IO.File.Exists(temp.Icon.Path))
                            {
                                downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Item Icon '{0}'", sigil.Name),
                                    url = temp.Icon.Url,
                                    path = temp.Icon.Path,
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
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons + Regex.Match(item.Icon, "[0-9]*.png") },
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
                                    Icon = new API.Icon() { Url = item.Icon.Url.ToString(), Path = Paths.armory_icons + Regex.Match(item.Icon, "[0-9]*.png") },
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
                            Icon = new API.Icon() { Url = trait.Icon.Url.ToString(), Path = Paths.traits_icons + Regex.Match(trait.Icon, "[0-9]*.png") },
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
                            Icon = new API.Icon() { Url = spec.Icon.Url.ToString(), Path = Paths.spec_icons + Regex.Match(spec.Icon, "[0-9]*.png") },
                            Background = new API.Icon() { Url = spec.Background.Url.ToString(), Path = Paths.spec_backgrounds + Regex.Match(spec.Background, "[0-9]*.png") },
                            ProfessionIconBig = spec.ProfessionIconBig != null ? new API.Icon() { Url = spec.ProfessionIconBig.ToString(), Path = Paths.spec_icons + Regex.Match(spec.ProfessionIconBig, "[0-9]*.png") } : null,
                            ProfessionIcon = spec.ProfessionIcon != null ? new API.Icon() { Url = spec.ProfessionIcon.ToString(), Path = Paths.spec_icons + Regex.Match(spec.ProfessionIcon, "[0-9]*.png") } : null,
                            Profession = spec.Profession,
                            Elite = spec.Elite,
                        };

                        temp.WeaponTrait = Traits.Find(e => e.Id == spec.WeaponTrait);
                        if (temp.WeaponTrait != null && !System.IO.File.Exists(temp.WeaponTrait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Trait Icon '{0}'", temp.WeaponTrait.Name),
                            url = temp.WeaponTrait.Icon.Url,
                            path = temp.WeaponTrait.Icon.Path,
                        });

                        foreach (int id in spec.MinorTraits)
                        {
                            var trait = Traits.Find(e => e.Id == id);

                            if (trait != null)
                            {
                                temp.MinorTraits.Add(trait);
                                if (!System.IO.File.Exists(trait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                    url = trait.Icon.Url,
                                    path = trait.Icon.Path,
                                });
                            }
                        }
                        foreach (int id in spec.MajorTraits)
                        {
                            var trait = Traits.Find(e => e.Id == id);

                            if (trait != null)
                            {
                                temp.MajorTraits.Add(trait);
                                if (!System.IO.File.Exists(trait.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                    url = trait.Icon.Url,
                                    path = trait.Icon.Path,
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
                            Icon = new API.Icon() { Url = skill.Icon.Url.ToString(), Path = Paths.skill_icons + Regex.Match(skill.Icon, "[0-9]*.png") },
                            ChatLink = skill.ChatLink,
                            Description = skill.Description,
                            Specialization = skill.Specialization != null ? (int)skill.Specialization : 0,
                        };

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
                            Icon = new API.Icon() { Url = profession.Icon.Url.ToString(), Path = Paths.profession_icons + Regex.Match(profession.Icon, "[0-9]*.png") },
                            IconBig = new API.Icon() { Url = profession.IconBig.Url.ToString(), Path = Paths.profession_icons + Regex.Match(profession.IconBig, "[0-9]*.png") },
                        };

                        Logger.Debug("Adding Specs ....");
                        foreach (int id in profession.Specializations)
                        {
                            var spec = Specializations.Find(e => e.Id == id);
                            if (spec != null)
                            {
                                temp.Specializations.Add(spec);

                                if (!System.IO.File.Exists(spec.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Specialization Icon '{0}'", spec.Name),
                                    url = spec.Icon.Url,
                                    path = spec.Icon.Path,
                                });

                                if (!System.IO.File.Exists(spec.Background.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Background '{0}'", spec.Name),
                                    url = spec.Background.Url,
                                    path = spec.Background.Path,
                                });

                                if (spec.ProfessionIcon != null && !System.IO.File.Exists(spec.ProfessionIcon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading ProfessionIcon '{0}'", spec.Name),
                                    url = spec.ProfessionIcon.Url,
                                    path = spec.ProfessionIcon.Path,
                                });

                                if (spec.ProfessionIconBig != null && !System.IO.File.Exists(spec.ProfessionIconBig.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading ProfessionIconBig '{0}'", spec.Name),
                                    url = spec.ProfessionIconBig.Url,
                                    path = spec.ProfessionIconBig.Path,
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
                        foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette)
                        {
                            var skill = Skills.Find(e => e.Id == skillIDs.Value);
                            if (skill != null)
                            {
                                skill.PaletteId = skillIDs.Key;
                                temp.Skills.Add(skill);

                                if (!System.IO.File.Exists(skill.Icon.Path)) downloadList.Add(new APIDownload_Image()
                                {
                                    display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                    url = skill.Icon.Url,
                                    path = skill.Icon.Path,
                                });
                            }
                        }


                        if (!System.IO.File.Exists(temp.Icon.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                            url = temp.Icon.Url,
                            path = temp.Icon.Path,
                        });

                        if (!System.IO.File.Exists(temp.IconBig.Path)) downloadList.Add(new APIDownload_Image()
                        {
                            display_text = string.Format("Downloading Profession Icon '{0}'", temp.Name),
                            url = temp.IconBig.Url,
                            path = temp.IconBig.Path,
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
            }
        }

        async Task LoadData()
        {
            await Fetch_APIData();

            Data = new iData(ContentsManager, DirectoriesManager);
        }

        private void CreateUI()
        {
            var Height = 670;
            var Width = 915;


            MainWindow = new iMainWindow(
                TextureManager.getBackground(_Backgrounds.MainWindow),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 5, Width - 5, Height - 30),
                TextureManager,
                GameService.Graphics.SpriteScreen);

            MainWindow.Template = new Template(Paths.builds + @"Condi Harbaebae.json");
            MainWindow.ToggleWindow();

            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIEKRYqPTlwAAAAogEAAGoAAACvAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]");
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQEQGzEvPjZLF0sXehZ6FjYBNgFTF1MXcRJxEgAAAAAAAAAAAAAAAAAAAAA=]");
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIEBhYVPT9wAKYAPQGoALMAagCpAOIBnADuAAAAAAAAAAAAAAAAAAAAAAA=]"); // All Filled
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIkAAAAEgAAAAAAqQAAAAAAAAAAAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]"); // Partly Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIzAAAACwAAAAAAAAAAAD0BAAAAAAAAAADuAAAAAAAAAAAAAAAAAAAAAAA=]"); // Minimum Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]"); //Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQMGOyYvOSsqDwAAhgAAACYBAABXFgAA8BUAAAAAAAAAAAAAAAAAAAAAAAA=]");
        }
    }
}