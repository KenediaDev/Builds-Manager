using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager
{
    public class iPaths
    {
        public string BasePath;

        public string builds { get => BasePath + @"\builds\"; }
        public string api { get => BasePath + @"\api\"; }
        public string traits { get => BasePath + @"\api\traits\"; }
        public string traits_icons { get => BasePath + @"\api\traits\icons\"; }
        public string professions { get => BasePath + @"\api\professions\"; }
        public string profession_icons { get => BasePath + @"\api\professions\icons\"; }
        //public string facts { get => BasePath + @"\api\facts\"; }
        //public string fact_icons { get => BasePath + @"\api\facts\icons\"; }
        public string stats { get => BasePath + @"\api\stats\"; }
        public string stats_icons { get => BasePath + @"\api\stats\icons\"; }
        public string skills { get => BasePath + @"\api\skills\"; }
        public string skill_icons { get => BasePath + @"\api\skills\icons\"; }
        public string specs { get => BasePath + @"\api\specs\"; }
        public string spec_icons { get => BasePath + @"\api\specs\icons\"; }
        public string spec_backgrounds { get => BasePath + @"\api\specs\backgrounds\"; }
        public string armory { get => BasePath + @"\api\armory\"; }
        public string armory_icons { get => BasePath + @"\api\armory\icons\"; }
        public string upgrades { get => BasePath + @"\api\upgrades\"; }
        public string sigils { get => BasePath + @"\api\upgrades\sigils\"; }
        public string sigil_icons { get => BasePath + @"\api\upgrades\sigils\icons\"; }
        public string runes { get => BasePath + @"\api\upgrades\runes\"; }
        public string rune_icons { get => BasePath + @"\api\upgrades\runes\icons\"; }

        public iPaths(string basePath)
        {
            BasePath = basePath;
            var properties = this.GetType().GetProperties();
            foreach (var property in properties)
            {
                var type = property.PropertyType;
                if(type == typeof(string))
                {
                    var path = property.GetValue(this, null).ToString();
                    if (path != null && path != "" && !Directory.Exists(path)) Directory.CreateDirectory(path);
                }
            }
        }
    }
}
