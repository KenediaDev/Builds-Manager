namespace Kenedia.Modules.BuildsManager.Models
{
    using System.IO;
    using Newtonsoft.Json;

    public class Template_json
    {
        public string Name;
        public string BuildCode;
        public string GearCode;

        public Template_json(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                var template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    this.Name = template.Name;
                    this.BuildCode = template.BuildCode;
                    this.GearCode = template.GearCode;
                }
            }
            else
            {
                this.Name = "Empty Build";
                this.GearCode = "[0][0][0][0][0][0][0|0][0|0][0|0][0|0][0|0][0|0][0|-1|0][0|-1|0][0|-1|0][0|-1|0][0|-1|0|0][0|-1|0|0]";
                this.BuildCode = "[&DQIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]";
            }
        }
    }
}
