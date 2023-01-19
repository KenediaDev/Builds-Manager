namespace Kenedia.Modules.BuildsManager
{
    using System.IO;

    public class iPaths
    {
        public string BasePath;

        public string builds { get => this.BasePath + @"\builds\"; }

        public string api { get => this.BasePath + @"\api\"; }

        public string traits { get => this.BasePath + @"\api\traits\"; }

        public string traits_icons { get => this.BasePath + @"\api\traits\icons\"; }

        public string professions { get => this.BasePath + @"\api\professions\"; }

        public string profession_icons { get => this.BasePath + @"\api\professions\icons\"; }

        // public string facts { get => BasePath + @"\api\facts\"; }
        // public string fact_icons { get => BasePath + @"\api\facts\icons\"; }
        public string stats { get => this.BasePath + @"\api\stats\"; }

        public string stats_icons { get => this.BasePath + @"\api\stats\icons\"; }

        public string skills { get => this.BasePath + @"\api\skills\"; }

        public string skill_icons { get => this.BasePath + @"\api\skills\icons\"; }

        public string specs { get => this.BasePath + @"\api\specs\"; }

        public string spec_icons { get => this.BasePath + @"\api\specs\icons\"; }

        public string spec_backgrounds { get => this.BasePath + @"\api\specs\backgrounds\"; }

        public string armory { get => this.BasePath + @"\api\armory\"; }

        public string armory_icons { get => this.BasePath + @"\api\armory\icons\"; }

        public string upgrades { get => this.BasePath + @"\api\upgrades\"; }

        public string sigils { get => this.BasePath + @"\api\upgrades\sigils\"; }

        public string sigil_icons { get => this.BasePath + @"\api\upgrades\sigils\icons\"; }

        public string runes { get => this.BasePath + @"\api\upgrades\runes\"; }

        public string rune_icons { get => this.BasePath + @"\api\upgrades\runes\icons\"; }

        public iPaths(string basePath)
        {
            this.BasePath = basePath;
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var type = property.PropertyType;
                if (type == typeof(string))
                {
                    var path = property.GetValue(this, null).ToString();
                    if (path != null && path != string.Empty && !Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }
    }
}
