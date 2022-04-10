using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using LegendaryItem = Kenedia.Modules.BuildsManager.GW2API.LegendaryItem;

namespace Kenedia.Modules.BuildsManager
{
   public class iData
    {
        private DirectoriesManager DirectoriesManager;
        public List<LegendaryItem> LegendaryItems = new List<LegendaryItem>();

        private LegendaryItem LegendaryItem(int id)
        {
            foreach(LegendaryItem item in LegendaryItems)
            {
                if (item != null && item.Id == id) return item;
            }

            return null;
        }

        public iData(DirectoriesManager directoriesManager)
        {
            DirectoriesManager = directoriesManager;

            var armory_path = DirectoriesManager.GetFullDirectoryPath("builds/api/armory");
            var file_path = armory_path + @"/" + "armory [" + BuildsManager.getCultureString() + "].json";

            if (System.IO.File.Exists(file_path))
            {
                LegendaryItems = JsonConvert.DeserializeObject<List<LegendaryItem>>(System.IO.File.ReadAllText(file_path));

                foreach (LegendaryItem legendary in LegendaryItems)
                {
                    if (legendary != null)
                    {
                        if (legendary.Icon != null && legendary.Icon != null)
                        {
                            legendary.api_Image = new API_Image()
                            {
                                folderPath = armory_path,
                                Url = legendary.Icon.Url,
                            };

                            var textureLoad = legendary.Texture;
                        }
                    }
                }
            }
        }
    }
}
