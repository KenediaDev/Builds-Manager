using System.IO;
using Newtonsoft.Json;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class Template_json
    {
        public string Name;
        public string BuildCode;
        public string GearCode;

        public Template_json(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                Template_json template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    Name = template.Name;
                    BuildCode = template.BuildCode;
                    GearCode = template.GearCode;
                }
            }
            else
            {
                Name = "Empty Build";
                GearCode = "[0][0][0][0][0][0][0|0][0|0][0|0][0|0][0|0][0|0][0|-1|0][0|-1|0][0|-1|0][0|-1|0][0|-1|0|0][0|-1|0|0]";
                BuildCode = "[&DQIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]";
            }
        }
    }
}
