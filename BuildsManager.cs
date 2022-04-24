using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Overlay.UI.Views;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Gw2Sharp.Models;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Input;
using Blish_HUD.Settings;
using System.Text.RegularExpressions;

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

        private SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        private CornerIcon cornerIcon;
        public static iDataManager DataManager;
        public static iPaths Paths;
        public static iData Data;
        public iTicks Ticks = new iTicks();

        public iMainWindow MainWindow;
        public LoadingSpinner loadingSpinner;
        public ProgressBar downloadBar;
        public static List<int> ArmoryItems = new List<int>();

        public static List<WebDownload_Image> download_Images = new List<WebDownload_Image>();
        public static List<Load_Image> load_Images = new List<Load_Image>();
        public static List<string> load_ImagePaths = new List<string>();
        bool download_Running;

        protected override void DefineSettings(SettingCollection settings)
        {

            ReloadKey = settings.DefineSetting(nameof(ReloadKey),
                                                      new Blish_HUD.Input.KeyBinding(Keys.LeftControl),
                                                      () => "Reload Button",
                                                      () => "");
        }

        protected override void Initialize()
        {
            Paths = new iPaths(DirectoriesManager.GetFullDirectoryPath("builds-manager"));
            LocalTexture.DirectoriesManager = DirectoriesManager;

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

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
                  30703, //Sunrise
                  30704, //Twilight
                  30689, //Eternity
                });

            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += delegate
            {
                //var code = "[&DQIEKRYqPTlwAAAAogEAAGoAAACvAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]";
                //var template = new BuildTemplate(code);

                ScreenNotification.ShowNotification("Rebuilding UI!", ScreenNotification.NotificationType.Error);
                MainWindow.Dispose();
               CreateUI(); 
                MainWindow.Show();

                //MainWindow.Build.UpdateTemplate();
                //MainWindow.TemplateBox.Text = MainWindow.Build.ParsedBuildTemplateCode;
            };
        }

        private async void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {            
            if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Inventories, TokenPermission.Unlocks}))
            {
                Logger.Debug("Fetching account infos!");
                //var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                var armory = await Gw2ApiManager.Gw2ApiClient.V2.Account.LegendaryArmory.GetAsync();
                foreach(AccountLegendaryArmory item in armory)
                {
               //     if(item.Count >= 1) ArmoryItems.Add(item.Id);
                }
            }
        }
        public static bool HasProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
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
        private async Task getUpgrades()
        {
            Logger.Debug("Fetching all Upgrades running now ....");
            List<int> IDs = new List<int>();

            int[] Symbol_of_Control =
            {
            24602,
            24569,
            24864,
            24635,
            24606,
            24566,
            38293,
            24596,
            24556,
            68434,
            24638,
            44946,
            24593,
            24588,
            75623,
            24550,
            24568,
            24863,
            24634,
            24565,
            38292,
            24595,
            24557,
            68439,
            24637,
            44945,
            24587,
            75963,
            24549,
            72872,
            24601,
            24570,
            81045,
            24865,
            67340,
            24636,
            70825,
            24607,
            24567,
            82876,
            38294,
            24597,
            24555,
            67343,
            68436,
            24572,
            24639,
            24571,
            44947,
            24594,
            71130,
            84505,
            24589,
            24592,
            74326,
            24551,
            86170,
            };
            int[] Symbol_of_Enchancement =
            {
            24617,
            74847,
            24611,
            24586,
            24574,
            44943,
            24629,
            72010,
            24577,
            24614,
            24626,
            24598,
            24581,
            24590,
            44949,
            24579,
            24620,
            24623,
            24563,
            24633,
            24616,
            71220,
            24610,
            24585,
            24573,
            44942,
            24628,
            73289,
            24576,
            24613,
            24625,
            24861,
            44948,
            24619,
            24622,
            24564,
            24631,
            24618,
            72092,
            24612,
            24584,
            24575,
            44944,
            24630,
            72339,
            24578,
            67341,
            24583,
            24615,
            24627,
            24599,
            24582,
            24591,
            44950,
            49457,
            24580,
            24621,
            24561,
            73532,
            24624,
            24562,
            24632,
            24600,
            };
            int[] Symbol_of_Pain =
                {
            24553,
            24644,
            24665,
            24653,
            24608,
            24680,
            24559,
            24662,
            24546,
            24604,
            24808,
            24647,
            24650,
            24867,
            24677,
            37911,
            24671,
            24656,
            24659,
            24674,
            24683,
            24641,
            24668,
            36054,
            24552,
            24643,
            24666,
            24652,
            24862,
            24679,
            24558,
            24663,
            24547,
            24603,
            24807,
            24646,
            91340,
            24649,
            24866,
            24676,
            37910,
            24670,
            24657,
            24660,
            24673,
            24682,
            24640,
            24669,
            36055,
            24554,
            67913,
            24645,
            24664,
            24654,
            24609,
            24681,
            24560,
            24661,
            24548,
            24605,
            24809,
            24648,
            91339,
            24651,
            24868,
            24678,
            37912,
            24672,
            24655,
            24658,
            24675,
            24684,
            48911,
            24642,
            24667,
            36053,
         };
            int[] Charm_of_Brilliance = {
            38205,
            24731,
            24767,
            44952,
            24778,
            24775,
            24707,
            44958,
            24725,
            68435,
            24719,
            24686,
            24749,
            24722,
            24799,
            24695,
            24784,
            24761,
            24690,
            24802,
            24841,
            24805,
            24781,
            24835,
            24758,
            38204,
            24730,
            24766,
            44953,
            24777,
            24774,
            24706,
            44959,
            24724,
            68438,
            24718,
            24685,
            24748,
            24721,
            24798,
            24694,
            24783,
            24760,
            24689,
            24801,
            24840,
            24804,
            24780,
            24834,
            24759,
            38206,
            24732,
            24768,
            44951,
            89999,
            24779,
            24776,
            24708,
            44957,
            24726,
            68437,
            24720,
            76813,
            24687,
            24750,
            73399,
            24723,
            24800,
            24696,
            24785,
            24762,
            24688,
            24803,
            84127,
            24842,
            24806,
            24782,
            70829,
            24836,
            83663,
            76166,
            24757,
            83423,
         };
            int[] Charm_of_Potence = {
            24764,
            24728,
            24770,
            24713,
            24793,
            44955,
            24710,
            24853,
            24832,
            24740,
            24698,
            24743,
            24746,
            24796,
            24823,
            24755,
            24826,
            24820,
            24790,
            24763,
            24727,
            24769,
            24712,
            24792,
            44954,
            24709,
            24852,
            24831,
            24739,
            24697,
            24742,
            24745,
            24795,
            24822,
            24754,
            24825,
            24819,
            24789,
            24765,
            73653,
            24729,
            70600,
            24771,
            67342,
            24714,
            24794,
            44956,
            24711,
            24854,
            71425,
            24833,
            83367,
            24741,
            67912,
            24699,
            74978,
            24744,
            24747,
            83338,
            24797,
            24824,
            76100,
            24756,
            83502,
            69370,
            71276,
            84749,
            47908,
            24827,
            24821,
            24791,
         };
            int[] Charm_of_Skill = {
            24704,
            24859,
            24716,
            24856,
            24737,
            24829,
            24844,
            24787,
            24811,
            24850,
            24734,
            24752,
            36043,
            24847,
            24701,
            24814,
            24817,
            24692,
            24838,
            24705,
            24858,
            24715,
            24855,
            24736,
            24828,
            24853,
            24786,
            24810,
            24849,
            24733,
            24751,
            36042,
            24846,
            24700,
            24813,
            24816,
            24693,
            24837,
            48907,
            67344,
            24703,
            81091,
            24860,
            24717,
            49460,
            24857,
            24738,
            72912,
            24830,
            24845,
            24788,
            72852,
            82791,
            70450,
            24812,
            24851,
            24735,
            82633,
            24753,
            36044,
            24848,
            24702,
            24815,
            84171,
            83964,
            85713,
            24818,
            67339,
            24691,
            24839,
            88118,
         };

            IDs.AddRange(Symbol_of_Control);
            IDs.AddRange(Symbol_of_Enchancement);
            IDs.AddRange(Symbol_of_Pain);
            IDs.AddRange(Charm_of_Brilliance);
            IDs.AddRange(Charm_of_Potence);
            IDs.AddRange(Charm_of_Skill);

            List<int> requestIDs = new List<int>();
            List<Item> Upgrades = new List<Item>();
            foreach (int ID in IDs)
            {
                requestIDs.Add(ID);

                if (requestIDs.Count == 200)
                {
                    Logger.Debug("Sending API Request for " + requestIDs.Count + " ids.");
                    var items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(requestIDs);
                    Logger.Debug("API Request answered for " + requestIDs.Count + " ids.");

                    Upgrades.AddRange(items);
                    requestIDs.Clear();
                }
            }

            if (requestIDs.Count > 0)
            {
                Logger.Debug("Sending API Request for " + requestIDs.Count + " ids.");
                var items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(requestIDs);
                Logger.Debug("API Request answered for " + requestIDs.Count + " ids.");

                Upgrades.AddRange(items);
                requestIDs.Clear();
            }

            List<int> RuneIDs = new List<int>();
            List<int> SigilsIDs = new List<int>();
            Logger.Debug("All API Request done.");
            Logger.Debug("Creating ID List ...");

            foreach (ItemUpgradeComponent item in Upgrades)
            {
                if(item != null)
                {
                    if (item.Rarity == ItemRarity.Exotic)
                    {
                        Logger.Debug("Adding {0} ID to the ID List.", item.Name);

                        if (item.Details.Type == ItemUpgradeComponentType.Rune)
                        {
                            RuneIDs.Add(item.Id);
                        }
                        else if (item.Details.Type == ItemUpgradeComponentType.Sigil)
                        {
                            SigilsIDs.Add(item.Id);
                        }
                    }
                }
            }

            Logger.Debug("Saving ID List ...");
            System.IO.File.WriteAllText(Paths.BasePath + @"/runes.json", JsonConvert.SerializeObject(RuneIDs.ToArray()));
            System.IO.File.WriteAllText(Paths.BasePath + @"/sigils.json", JsonConvert.SerializeObject(SigilsIDs.ToArray()));
            Logger.Debug("Done!");
        }

        private async Task Load_APIData()
        {
            var downloadList = new List<APIDownload_Image>();
            var culture = getCultureString();

            double total = 0;
            double completed = 0;
            double progress = 0;

            Logger.Debug("Fetching all required Data from the API!");
            loadingSpinner.Visible = true;
            downloadBar.Visible = true;
            downloadBar.Progress = progress;
            downloadBar.Text = string.Format("{0} / 9", completed);

            var sigils = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(DataManager._UpgradeIDs._Sigils);
            completed++;
            downloadBar.Progress = completed / 9;
            downloadBar.Text = string.Format("{0} / 9", completed);

            var runes = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(DataManager._UpgradeIDs._Runes);
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

            var runes_path = Paths.runes;
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
            System.IO.File.WriteAllText(runes_path + @"/" + "custom runes [" + culture + "].json", JsonConvert.SerializeObject(Runes.ToArray()));

            var sigil_path = Paths.sigils;
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
            System.IO.File.WriteAllText(sigil_path + @"/" + "custom sigils [" + culture + "].json", JsonConvert.SerializeObject(Sigils.ToArray()));

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

                        foreach(ItemstatAttribute attribute in stat.Attributes)
                        {
                            temp.Attributes.Add(new API.StatAttribute() 
                            {
                                Id = (int) attribute.Attribute.Value,
                                Name = attribute.Attribute.RawValue,
                                Multiplier = attribute.Multiplier,
                                Icon = new API.Icon() { Path = @"textures\stats\" + (int)attribute.Attribute.Value + ".png" },
                            });
                        }

                        Stats.Add(temp);
                    }
                }
            }
            System.IO.File.WriteAllText(Paths.stats + "custom stats [" + culture + "].json", JsonConvert.SerializeObject(Stats.ToArray()));


            var armory_path = Paths.armory;
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
                            };

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
                            };

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
                            };

                            //Enum.TryParse(item.Details.Type.RawValue, out temp.TrinketType);
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
                            });
                        }
                    }

                    downloadList.Add(new APIDownload_Image()
                    {
                        display_text = string.Format("Downloading Item Icon '{0}'", i.Name),
                        url = i.Icon,
                        path = Paths.armory_icons + Regex.Match(i.Icon, "[0-9]*.png"),
                    });
                }
            }
            System.IO.File.WriteAllText(armory_path + @"/" + "custom armors [" + culture + "].json", JsonConvert.SerializeObject(Armors.ToArray()));
            System.IO.File.WriteAllText(armory_path + @"/" + "custom weapons [" + culture + "].json", JsonConvert.SerializeObject(Weapons.ToArray()));
            System.IO.File.WriteAllText(armory_path + @"/" + "custom trinkets [" + culture + "].json", JsonConvert.SerializeObject(Trinkets.ToArray()));

            Logger.Debug("Preparing Traits ....");
            List<API.Trait> Traits = new List<API.Trait> ();
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
            List<API.Specialization> Specializations = new List<API.Specialization> ();
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
                    foreach(int id in spec.MinorTraits)
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
                    foreach(int id in spec.MajorTraits)
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
                        Icon = new API.Icon() { Url = skill.Icon.Url.ToString(), Path = Paths.skill_icons+ Regex.Match(skill.Icon, "[0-9]*.png") },
                        ChatLink = skill.ChatLink,
                        Description = skill.Description,
                        Specialization = skill.Specialization != null ? (int) skill.Specialization : 0,
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
                        temp.Weapons.Add(weaponType);
                    }

                    Logger.Debug("Adding Skills ....");
                    foreach (KeyValuePair<int, int> skillIDs in profession.SkillsByPalette) 
                    {
                        var skill = Skills.Find(e => e.Id == skillIDs.Value);
                        if(skill != null)
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


                   if(!System.IO.File.Exists(temp.Icon.Path)) downloadList.Add(new APIDownload_Image()
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
            System.IO.File.WriteAllText(Paths.professions + "custom professions [" + culture + "].json", JsonConvert.SerializeObject(Professions.ToArray()));

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

            await CreateUI();
        }

        private async Task DownloadQueuedImages()
        {
            if (!download_Running && download_Images.Count > 0)
            {
                var downloadList = new List<WebDownload_Image>(download_Images);
                download_Images = new List<WebDownload_Image>();
                download_Running = true;

                foreach (WebDownload_Image image in downloadList)
                {
                    var stream = new FileStream(image.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    await Gw2ApiManager.Gw2ApiClient.Render.DownloadToStreamAsync(stream, image.Url);
                    image.OnDownloadComplete();
                    stream.Close();
                }

                download_Running = false;
            }
        }

        private void LoadQueued_LocalImages()
        {
            if (load_Images.Count > 0)
            {
                Logger.Debug("LoadQueued_LocalImages");

                var loadList = new List<Load_Image>(load_Images);
                load_Images = new List<Load_Image>();

                GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                {
                    foreach(Load_Image image in loadList)
                    {
                        Logger.Debug("Queued Image load for: " + image.Path);
                        image.Target.Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(image.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        image.OnLoadComplete();
                        image.Target.Controls.Clear();
                    }
                });
            }
        }

        protected override async Task LoadAsync()
        {
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            DataManager = new iDataManager(ContentsManager, DirectoriesManager);
            Data = new iData();


            cornerIcon = new CornerIcon()
            {
                Icon = DataManager.getIcon(_Icons.Template),
                BasicTooltipText = $"{Name}",
                Parent = GameService.Graphics.SpriteScreen
            };
            cornerIcon.MouseEntered += delegate
            {
                cornerIcon.Icon = DataManager.getIcon(_Icons.Template_White);
            };
            cornerIcon.MouseLeft += delegate
            {
                cornerIcon.Icon = DataManager.getIcon(_Icons.Template);
            };
            cornerIcon.Click += delegate
            {
                MainWindow.ToggleWindow();
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


            //getUpgrades();
            Load_APIData();

            

            // Base handler must be called
            base.OnModuleLoaded(e);
        }
        private async Task CreateUI()
        {
            var Height = 670;
            var Width = 915;

            MainWindow = new iMainWindow(
                DataManager.getBackground(_Backgrounds.MainWindow),
                new Rectangle(30, 30, Width, Height + 30),
                new Rectangle(30, 5, Width - 5, Height - 30),
                DataManager,
                GameService.Graphics.SpriteScreen);

            MainWindow.ToggleWindow();

            var base_path = Paths.BasePath;

            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIEKRYqPTlwAAAAogEAAGoAAACvAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]");
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQEQGzEvPjZLF0sXehZ6FjYBNgFTF1MXcRJxEgAAAAAAAAAAAAAAAAAAAAA=]");
            MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIEBhYVPT9wAKYAPQGoALMAagCpAOIBnADuAAAAAAAAAAAAAAAAAAAAAAA=]"); // All Filled
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIkAAAAEgAAAAAAqQAAAAAAAAAAAAAAnAAAAAAAAAAAAAAAAAAAAAAAAAA=]"); // Partly Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIzAAAACwAAAAAAAAAAAD0BAAAAAAAAAADuAAAAAAAAAAAAAAAAAAAAAAA=]"); // Minimum Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]"); //Empty
            //MainWindow.Build.BuildTemplate = new BuildTemplate("[&DQMGOyYvOSsqDwAAhgAAACYBAABXFgAA8BUAAAAAAAAAAAAAAAAAAAAAAAA=]");
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.global += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Ticks.global > 250)
            {
                Ticks.global -= 250;
                DownloadQueuedImages();
                LoadQueued_LocalImages();
            }
        }

        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= Gw2ApiManager_SubtokenUpdated; 

            ModuleInstance = null;
        }
    }
}