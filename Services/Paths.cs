using System.IO;

namespace Kenedia.Modules.BuildsManager
{
    public class iPaths
    {
        public string BasePath;

        public string builds => BasePath + @"\builds\";

        public string api => BasePath + @"\api\";

        public string traits => BasePath + @"\api\traits\";

        public string traits_icons => BasePath + @"\api\traits\icons\";

        public string professions => BasePath + @"\api\professions\";

        public string profession_icons => BasePath + @"\api\professions\icons\";

        // public string facts { get => BasePath + @"\api\facts\"; }
        // public string fact_icons { get => BasePath + @"\api\facts\icons\"; }
        public string stats => BasePath + @"\api\stats\";

        public string stats_icons => BasePath + @"\api\stats\icons\";

        public string skills => BasePath + @"\api\skills\";

        public string skill_icons => BasePath + @"\api\skills\icons\";

        public string specs => BasePath + @"\api\specs\";

        public string spec_icons => BasePath + @"\api\specs\icons\";

        public string spec_backgrounds => BasePath + @"\api\specs\backgrounds\";

        public string armory => BasePath + @"\api\armory\";

        public string armory_icons => BasePath + @"\api\armory\icons\";

        public string upgrades => BasePath + @"\api\upgrades\";

        public string sigils => BasePath + @"\api\upgrades\sigils\";

        public string sigil_icons => BasePath + @"\api\upgrades\sigils\icons\";

        public string runes => BasePath + @"\api\upgrades\runes\";

        public string rune_icons => BasePath + @"\api\upgrades\runes\icons\";

        public iPaths(string basePath)
        {
            BasePath = basePath;
            System.Reflection.PropertyInfo[] properties = GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                System.Type type = property.PropertyType;
                if (type == typeof(string))
                {
                    string path = property.GetValue(this, null).ToString();
                    if (path != null && path != string.Empty && !Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }
    }
}
