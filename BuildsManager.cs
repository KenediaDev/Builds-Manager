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
        public static iData Data;
        public iTicks Ticks = new iTicks();

        public iMainWindow MainWindow;
        public LoadingSpinner loadingSpinner;
        public ProgressBar downloadBar;

        public static List<APIDownload_Image> download_Images = new List<APIDownload_Image>();
        public static List<API_Image> load_Images = new List<API_Image>();
        bool download_Running;

        protected override void DefineSettings(SettingCollection settings)
        {

           ReloadKey =  settings.DefineSetting(nameof(ReloadKey),
                                                     new Blish_HUD.Input.KeyBinding(Keys.LeftControl),
                                                     () => "Reload Button",
                                                     () => "");
        }

        protected override void Initialize()
        {
            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += delegate
            {
                ScreenNotification.ShowNotification("Rebuilding UI!", ScreenNotification.NotificationType.Error);
                MainWindow.Dispose();
                CreateUI();
                MainWindow.Show();
            };
        }

        private async void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {

            if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
            {
                Logger.Debug("API Token update! Fetching new characters!");
                var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();

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
        private async Task Load_APIData()
        {
            var downloadList = new List<APIDownload_Image>();
            var culture = getCultureString();

            double total;
            double completed;
            double progress = 0;

            Logger.Debug("Fetching all required Data from the API!");
            loadingSpinner.Visible = true;
            downloadBar.Visible = true;
            downloadBar.Progress = progress;

            var stats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
            var specs = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
            var traits = await Gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
            var skills = await Gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            var armory_raw = await Gw2ApiManager.Gw2ApiClient.V2.LegendaryArmory.AllAsync();
            var armory_ids = new List<int>();


            foreach (LegendaryArmory legendary in armory_raw)
            {
                armory_ids.Add(legendary.Id);
            }

            Logger.Debug("All base API Infos fetched. Checking Items ....");
            Logger.Debug( string.Join(", ", armory_ids));

            var armory_items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(armory_ids);
            total = stats.Count + specs.Count + traits.Count + skills.Count + armory_raw.Count + armory_items.Count;

            Logger.Debug("All API Infos fetched. Checking Images to downloading. Starting Items ....");

            var armory_path = DirectoriesManager.GetFullDirectoryPath("builds/api/armory");
            var armory_local = new List<GW2API.LegendaryItem>();
            foreach (Item item in armory_items)
            {
                if (item != null)
                {
                    if (item.Icon != null && item.Icon.Url != null)
                    {
                        var file_name = Regex.Match(item.Icon, "[0-9]*.png");
                        var file_path = armory_path + @"/icons/" + file_name;

                        if (!System.IO.File.Exists(file_path))
                        {
                            downloadList.Add(new APIDownload_Image()
                            {
                                display_text = string.Format("Downloading Item Icon '{0}'", item.Name),
                                url = item.Icon.Url.ToString(),
                                path = file_path,
                            });
                        }

                        item.HttpResponseInfo = null;
                        //armory_local.Add(new GW2API.LegendaryItem(item));
                    }
                }
            }
            Logger.Debug("Saving Files ....");

            System.IO.File.WriteAllText(armory_path + @"/" + "armory.json", JsonConvert.SerializeObject(armory_raw.ToArray()));            
            System.IO.File.WriteAllText(armory_path + @"/" + "armory [" + culture + "].json", JsonConvert.SerializeObject(armory_items.ToArray()));


            Logger.Debug("Items done. Starting Stats ....");
            var stats_path = DirectoriesManager.GetFullDirectoryPath("builds/api/stats");
            System.IO.File.WriteAllText(stats_path + @"/" + "stats [" + culture + "].json", JsonConvert.SerializeObject(stats.ToArray()));

            Logger.Debug("Stats done. Starting specs ....");
            var fact_path = DirectoriesManager.GetFullDirectoryPath("builds/api/facts") + @"/";
            var spec_path = DirectoriesManager.GetFullDirectoryPath("builds/api/specs");
            foreach (Specialization spec in specs)
            {
                if (spec != null)
                {
                    if (spec.Icon != null && spec.Icon.Url != null)
                    {
                        var file_name = Regex.Match(spec.Icon, "[0-9]*.png");
                        var file_path = spec_path + @"/icons/" + file_name;

                        if (!System.IO.File.Exists(file_path))
                        {
                            downloadList.Add(new APIDownload_Image() {
                                display_text = string.Format("Downloading Specialization Icon '{0}'", spec.Name),
                                url = spec.Icon,
                                path = file_path,
                            });
                        }
                    }

                    if (spec.Background != null && spec.Background.Url != null)
                    {
                        var file_name = Regex.Match(spec.Background, "[0-9]*.png");
                        var file_path = spec_path + @"/backgrounds/" + file_name;

                        if (!System.IO.File.Exists(file_path))
                        {
                            downloadList.Add(new APIDownload_Image()
                            {
                                display_text = string.Format("Downloading Specialization Background '{0}'", spec.Name),
                                url = spec.Background,
                                path = file_path,
                            });
                        }
                    }

                    spec.HttpResponseInfo = null;
                }
            }
            System.IO.File.WriteAllText(spec_path + @"/" + "specializations [" + culture + "].json", JsonConvert.SerializeObject(specs.ToArray()));

            Logger.Debug("Specs done. Starting traits ....");
            var trait_path = DirectoriesManager.GetFullDirectoryPath("builds/api/traits");
            foreach (Trait trait in traits)
            {
                if (trait != null)
                {
                    if (trait.Icon != null && trait.Icon.Url != null)
                    {
                        var file_name = Regex.Match(trait.Icon, "[0-9]*.png");
                        var file_path = trait_path + @"/icons/" + file_name;

                        if (!System.IO.File.Exists(file_path))
                        {
                            downloadList.Add(new APIDownload_Image()
                            {
                                display_text = string.Format("Downloading Trait Icon '{0}'", trait.Name),
                                url = trait.Icon,
                                path = file_path,
                            });
                        }
                    }

                    if (trait.Facts != null && trait.Facts.Count > 0)
                    {
                        foreach (TraitFact fact in trait.Facts)
                        {
                            if (fact.Icon != null && fact.Icon.HasValue)
                            {
                                var file_name = Regex.Match(fact.Icon, "[0-9]*.png");
                                var file_path = fact_path + file_name;

                                if (!System.IO.File.Exists(file_path))
                                {
                                    downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Fact Icon '{0}'", trait.Name),
                                        url = fact.Icon,
                                        path = file_path,
                                    });
                                }
                            }
                        }
                    }

                    trait.HttpResponseInfo = null;
                }
            }
            System.IO.File.WriteAllText(trait_path + @"/" + "traits [" + culture + "].json", JsonConvert.SerializeObject(traits.ToArray()));

            Logger.Debug("Traits done. Starting skills ....");
            var skill_path = DirectoriesManager.GetFullDirectoryPath("builds/api/skills");
            foreach (Skill skill in skills)
            {
                if (skill != null)
                {
                    if (skill.Icon != null && skill.Icon.Url != null)
                    {
                        var file_name = Regex.Match(skill.Icon, "[0-9]*.png");
                        var file_path = skill_path + @"/icons/" + file_name;

                        if (!System.IO.File.Exists(file_path))
                        {
                            downloadList.Add(new APIDownload_Image()
                            {
                                display_text = string.Format("Downloading Skill Icon '{0}'", skill.Name),
                                url = skill.Icon,
                                path = file_path,
                            });
                        }
                    }

                    if (skill.Facts != null && skill.Facts.Count > 0)
                    {
                        foreach (SkillFact fact in skill.Facts)
                        {
                            if (fact.Icon != null && fact.Icon.HasValue)
                            {
                                var file_name = Regex.Match(fact.Icon, "[0-9]*.png");
                                var file_path = fact_path + file_name;

                                if (!System.IO.File.Exists(file_path))
                                {
                                    downloadList.Add(new APIDownload_Image()
                                    {
                                        display_text = string.Format("Downloading Fact Icon '{0}'", skill.Name),
                                        url = fact.Icon,
                                        path = file_path,
                                    });
                                }
                            }
                        }
                    }

                    skill.HttpResponseInfo = null;
                }
            }
            System.IO.File.WriteAllText(skill_path + @"/" + "skills [" + culture + "].json", JsonConvert.SerializeObject(skills.ToArray()));

            completed = total - downloadList.Count;

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
                downloadBar.Text = Math.Round((progress * 100), 2).ToString() + "%";
                downloadBar.BasicTooltipText = image.display_text;
                downloadBar._Label.BasicTooltipText = image.display_text;
                downloadBar._BackgroundTexture.BasicTooltipText = image.display_text;
                downloadBar._FilledTexture.BasicTooltipText = image.display_text;
            }

            loadingSpinner.Visible = false;
            downloadBar.Visible = false;
        }

        private async Task DownloadQueuedImages()
        {
            if (!download_Running && download_Images.Count > 0)
            {
                var downloadList = new List<APIDownload_Image>(download_Images);
                download_Images = new List<APIDownload_Image>();
                download_Running = true;

                foreach (APIDownload_Image image in downloadList)
                {
                    var stream = new FileStream(image.path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    await Gw2ApiManager.Gw2ApiClient.Render.DownloadToStreamAsync(stream, image.url);
                    image.Parent.fileFetched = true;
                    stream.Close();
                }

                download_Running = false;
            }
        }

        private void LoadQueued_LocalImages()
        {
            if (load_Images.Count > 0)
            {
                GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
                {
                    foreach(API_Image image in load_Images)
                    {
                        image._Texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(image.iconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        foreach (Control c in image.connectedControls)
                        {
                            if (c != null)
                            {
                                switch (c)
                                {
                                    case ItemImage img:
                                        img.Texture = image._Texture;
                                        break;

                                    case Image img:
                                        img.Texture = image._Texture;
                                        break;
                                }
                            }
                        }

                        image.connectedControls = new List<Control>();
                        image.fileLoaded = true;
                        image.fileChecked = true;
                        image.fileFetched = true;
                    }

                    load_Images.Clear();
                });
            }
        }

        protected override async Task LoadAsync()
        {
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            DataManager = new iDataManager(ContentsManager);
            Data = new iData(DirectoriesManager);

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

            //Load_APIData();
            CreateUI();

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
                new Rectangle(85, 15, Width - 55, Height + 15),
                DataManager,
                GameService.Graphics.SpriteScreen);

            MainWindow.ToggleWindow();
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.global += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (Ticks.global > 2500)
            {
                Ticks.global -= 2500;
                LoadQueued_LocalImages();
                DownloadQueuedImages();
            }
        }

        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= Gw2ApiManager_SubtokenUpdated; 

            ModuleInstance = null;
        }
    }
}