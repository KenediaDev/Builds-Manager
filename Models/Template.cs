using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Blish_HUD.Controls;
using Newtonsoft.Json;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class Template : IDisposable
    {
        private readonly bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                // Profession?.Dispose();
                Profession = null;

                // Specialization?.Dispose();
                Specialization = null;
                Gear?.Dispose();
                Build?.Dispose();
            }
        }

        public enum _TrinketSlots
        {
            Back,
            Amulet,
            Ring1,
            Ring2,
            Accessoire1,
            Accessoire2,
        }

        public enum _AmorSlots
        {
            Helmet,
            Shoulders,
            Chest,
            Gloves,
            Leggings,
            Boots,
        }

        public enum _WeaponSlots
        {
            Weapon1_MainHand,
            Weapon1_OffHand,
            Weapon2_MainHand,
            Weapon2_OffHand,
        }

        private enum _AquaticWeaponSlots
        {
            AquaticWeapon1,
            AquaticWeapon2,
        }

        public Template_json Template_json;

        private string _Name;

        public string Name
        {
            get => _Name;
            set
            {
                if (_Name != null && Path == null)
                {
                    return;
                }

                _Name = value;
            }
        }

        public API.Profession Profession;
        public API.Specialization Specialization;

        public string Path;
        public GearTemplate Gear = new();
        public BuildTemplate Build;

        public Template(string name, string build, string gear)
        {
            Template_json = new Template_json()
            {
                Name = name,
                BuildCode = build,
                GearCode = gear,
            };

            Name = name;

            Path = BuildsManager.s_moduleInstance.Paths.builds + "Builds.json";

            Build = new BuildTemplate(Template_json.BuildCode);
            Gear = new GearTemplate(Template_json.GearCode);

            Profession = BuildsManager.s_moduleInstance.Data.Professions.Find(e => e.Id == Build?.Profession?.Id);
            Specialization = Profession != null ? Build.SpecLines.Find(e => e.Specialization?.Elite == true)?.Specialization : null;
        }

        public Template(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                Template_json template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    Template_json = template;
                    Name = template.Name;

                    Path = BuildsManager.s_moduleInstance.Paths.builds + "Builds.json";

                    Build = new BuildTemplate(Template_json.BuildCode);
                    Gear = new GearTemplate(Template_json.GearCode);

                    Profession = BuildsManager.s_moduleInstance.Data.Professions.Find(e => e.Id == Build?.Profession.Id);
                    Specialization = Profession != null ? Build.SpecLines.Find(e => e.Specialization.Elite)?.Specialization : null;
                }
            }
            else
            {
                Gear = new GearTemplate();
                Build = new BuildTemplate();
                Name = "[No Name Set]";

                Template_json = new Template_json();
                Profession = BuildsManager.s_moduleInstance.CurrentProfession;

                Path = BuildsManager.s_moduleInstance.Paths.builds + "Builds.json";

                SetChanged();
            }
        }

        public void Reset()
        {
            Name = "[No Name Set]";
            Template_json = new Template_json()
            {
                Name = Name,
            };
            Specialization = null;

            foreach (TemplateItem item in Gear.Trinkets)
            {
                item.Stat = null;
            }

            foreach (Armor_TemplateItem item in Gear.Armor)
            {
                item.Stat = null;
                item.Rune = null;
            }

            foreach (Weapon_TemplateItem item in Gear.Weapons)
            {
                item.WeaponType = API.weaponType.Unkown;
                item.Stat = null;
                item.Sigil = null;
            }

            foreach (AquaticWeapon_TemplateItem item in Gear.AquaticWeapons)
            {
                item.WeaponType = API.weaponType.Unkown;
                item.Stat = null;
                item.Sigils = new List<API.SigilItem>() { new API.SigilItem(), new API.SigilItem() };
            }

            Build = new BuildTemplate();

            Profession = BuildsManager.s_moduleInstance.CurrentProfession;
            Path = BuildsManager.s_moduleInstance.Paths.builds;

            SetChanged();
        }

        public void Delete()
        {
            if (Name == "[No Name Set]")
            {
                BuildsManager.s_moduleInstance.Templates.Remove(this);
                BuildsManager.s_moduleInstance.OnTemplate_Deleted();
                return;
            }

            if (Path == null)
            {
                return;
            }

            BuildsManager.s_moduleInstance.Templates.Remove(this);

            if (Path == null || Name == null)
            {
                return;
            }

            Save();
            BuildsManager.s_moduleInstance.OnTemplate_Deleted();
            Deleted?.Invoke(this, EventArgs.Empty);
        }

        public void Save_Unformatted()
        {
            if (Path == null || Name == null)
            {
                return;
            }

            if (Name == "[No Name Set]")
            {
                return;
            }

            string path = Path;

            BuildsManager.Logger.Debug("Saving: {0} in {1}.", Name, Path);

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            if (Name.Contains("/") || Name.Contains(@"\") || ReferenceEquals(fi, null))
            {
                // file name is not valid
                ScreenNotification.ShowNotification(Name + " is not a valid Name!", ScreenNotification.NotificationType.Error);
            }
            else
            {
                Template_json.Name = Name;
                Template_json.BuildCode = Build?.ParseBuildCode();
                Template_json.GearCode = Gear?.TemplateCode;

                string culture = BuildsManager.getCultureString();
                File.WriteAllText(Path, JsonConvert.SerializeObject(BuildsManager.s_moduleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList()));

                // File.WriteAllText(Path, JsonConvert.SerializeObject(BuildsManager.ModuleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList(), Formatting.Indented));
            }
        }

        public void Save()
        {
            if (Path == null || Name == null)
            {
                return;
            }

            if (Name == "[No Name Set]")
            {
                return;
            }

            string path = Path;

            BuildsManager.Logger.Debug("Saving: {0} in {1}.", Name, Path);

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            if (Name.Contains("/") || Name.Contains(@"\") || ReferenceEquals(fi, null))
            {
                // file name is not valid
                ScreenNotification.ShowNotification(Name + " is not a valid Name!", ScreenNotification.NotificationType.Error);
            }
            else
            {
                Template_json.Name = Name;
                Template_json.BuildCode = Build?.ParseBuildCode();
                Template_json.GearCode = Gear?.TemplateCode;

                StringBuilder sb = new();
                bool first = true;
                foreach (Template_json template in BuildsManager.s_moduleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList())
                {
                    StringWriter sw = new(sb);
                    if (!first)
                    {
                        sb.Append("," + Environment.NewLine);
                    }

                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;

                        writer.WriteStartObject();

                        writer.WritePropertyName("Name");
                        writer.WriteValue(template.Name);

                        writer.WritePropertyName("BuildCode");
                        writer.WriteValue(template.BuildCode);

                        writer.WritePropertyName("GearCode");
                        writer.WriteValue(template.GearCode);

                        writer.WriteEndObject();
                    }

                    first = false;
                }

                File.WriteAllText(Path, "[" + sb.ToString() + "]");

                // File.WriteAllText(Path, JsonConvert.SerializeObject(BuildsManager.ModuleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList(), Formatting.Indented));
            }
        }

        public event EventHandler Edit;

        public event EventHandler Deleted;

        private void OnEdit(object sender, EventArgs e)
        {
            Edit?.Invoke(this, EventArgs.Empty);
            Save();
        }

        public void SetChanged()
        {
            OnEdit(null, EventArgs.Empty);
        }
    }
}
