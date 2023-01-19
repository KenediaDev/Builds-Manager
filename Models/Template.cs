namespace Kenedia.Modules.BuildsManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Blish_HUD.Controls;
    using Newtonsoft.Json;

    public class Template : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                // Profession?.Dispose();
                this.Profession = null;

                // Specialization?.Dispose();
                this.Specialization = null;
                this.Gear?.Dispose();
                this.Build?.Dispose();
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
            get => this._Name;
            set
            {
                if (this._Name != null && this.Path == null)
                {
                    return;
                }

                this._Name = value;
            }
        }

        public API.Profession Profession;
        public API.Specialization Specialization;

        public string Path;
        public GearTemplate Gear = new GearTemplate();
        public BuildTemplate Build;

        public Template(string name, string build, string gear)
        {
            this.Template_json = new Template_json()
            {
                Name = name,
                BuildCode = build,
                GearCode = gear,
            };

            this.Name = name;

            this.Path = BuildsManager.ModuleInstance.Paths.builds + "Builds.json";

            this.Build = new BuildTemplate(this.Template_json.BuildCode);
            this.Gear = new GearTemplate(this.Template_json.GearCode);

            this.Profession = BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == this.Build?.Profession?.Id);
            this.Specialization = this.Profession != null ? this.Build.SpecLines.Find(e => e.Specialization?.Elite == true)?.Specialization : null;
        }

        public Template(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                var template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    this.Template_json = template;
                    this.Name = template.Name;

                    this.Path = BuildsManager.ModuleInstance.Paths.builds + "Builds.json";

                    this.Build = new BuildTemplate(this.Template_json.BuildCode);
                    this.Gear = new GearTemplate(this.Template_json.GearCode);

                    this.Profession = BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == this.Build?.Profession.Id);
                    this.Specialization = this.Profession != null ? this.Build.SpecLines.Find(e => e.Specialization.Elite)?.Specialization : null;
                }
            }
            else
            {
                this.Gear = new GearTemplate();
                this.Build = new BuildTemplate();
                this.Name = "[No Name Set]";

                this.Template_json = new Template_json();
                this.Profession = BuildsManager.ModuleInstance.CurrentProfession;

                this.Path = BuildsManager.ModuleInstance.Paths.builds + "Builds.json";

                this.SetChanged();
            }
        }

        public void Reset()
        {
            this.Name = "[No Name Set]";
            this.Template_json = new Template_json()
            {
                Name = this.Name,
            };
            this.Specialization = null;

            foreach (TemplateItem item in this.Gear.Trinkets)
            {
                item.Stat = null;
            }

            foreach (Armor_TemplateItem item in this.Gear.Armor)
            {
                item.Stat = null;
                item.Rune = null;
            }

            foreach (Weapon_TemplateItem item in this.Gear.Weapons)
            {
                item.WeaponType = API.weaponType.Unkown;
                item.Stat = null;
                item.Sigil = null;
            }

            foreach (AquaticWeapon_TemplateItem item in this.Gear.AquaticWeapons)
            {
                item.WeaponType = API.weaponType.Unkown;
                item.Stat = null;
                item.Sigils = new List<API.SigilItem>() { new API.SigilItem(), new API.SigilItem() };
            }

            this.Build = new BuildTemplate();

            this.Profession = BuildsManager.ModuleInstance.CurrentProfession;
            this.Path = BuildsManager.ModuleInstance.Paths.builds;

            this.SetChanged();
        }

        public void Delete()
        {
            if (this.Name == "[No Name Set]")
            {
                BuildsManager.ModuleInstance.Templates.Remove(this);
                BuildsManager.ModuleInstance.OnTemplate_Deleted();
                return;
            }

            if (this.Path == null)
            {
                return;
            }

            BuildsManager.ModuleInstance.Templates.Remove(this);

            if (this.Path == null || this.Name == null)
            {
                return;
            }

            this.Save();
            BuildsManager.ModuleInstance.OnTemplate_Deleted();
            this.Deleted?.Invoke(this, EventArgs.Empty);
        }

        public void Save_Unformatted()
        {
            if (this.Path == null || this.Name == null)
            {
                return;
            }

            if (this.Name == "[No Name Set]")
            {
                return;
            }

            var path = this.Path;

            BuildsManager.Logger.Debug("Saving: {0} in {1}.", this.Name, this.Path);

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            if (this.Name.Contains("/") || this.Name.Contains(@"\") || ReferenceEquals(fi, null))
            {
                // file name is not valid
                ScreenNotification.ShowNotification(this.Name + " is not a valid Name!", ScreenNotification.NotificationType.Error);
            }
            else
            {
                this.Template_json.Name = this.Name;
                this.Template_json.BuildCode = this.Build?.ParseBuildCode();
                this.Template_json.GearCode = this.Gear?.TemplateCode;

                var culture = BuildsManager.getCultureString();
                File.WriteAllText(this.Path, JsonConvert.SerializeObject(BuildsManager.ModuleInstance.Templates.Where(e => e.Path == this.Path).Select(a => a.Template_json).ToList()));

                // File.WriteAllText(Path, JsonConvert.SerializeObject(BuildsManager.ModuleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList(), Formatting.Indented));
            }
        }

        public void Save()
        {
            if (this.Path == null || this.Name == null)
            {
                return;
            }

            if (this.Name == "[No Name Set]")
            {
                return;
            }

            var path = this.Path;

            BuildsManager.Logger.Debug("Saving: {0} in {1}.", this.Name, this.Path);

            FileInfo fi = null;
            try
            {
                fi = new FileInfo(path);
            }
            catch (ArgumentException) { }
            catch (PathTooLongException) { }
            catch (NotSupportedException) { }

            if (this.Name.Contains("/") || this.Name.Contains(@"\") || ReferenceEquals(fi, null))
            {
                // file name is not valid
                ScreenNotification.ShowNotification(this.Name + " is not a valid Name!", ScreenNotification.NotificationType.Error);
            }
            else
            {
                this.Template_json.Name = this.Name;
                this.Template_json.BuildCode = this.Build?.ParseBuildCode();
                this.Template_json.GearCode = this.Gear?.TemplateCode;

                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (Template_json template in BuildsManager.ModuleInstance.Templates.Where(e => e.Path == this.Path).Select(a => a.Template_json).ToList())
                {
                    StringWriter sw = new StringWriter(sb);
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

                File.WriteAllText(this.Path, "[" + sb.ToString() + "]");

                // File.WriteAllText(Path, JsonConvert.SerializeObject(BuildsManager.ModuleInstance.Templates.Where(e => e.Path == Path).Select(a => a.Template_json).ToList(), Formatting.Indented));
            }
        }

        public event EventHandler Edit;

        public event EventHandler Deleted;

        private void OnEdit(object sender, EventArgs e)
        {
            this.Edit?.Invoke(this, EventArgs.Empty);
            this.Save();
        }

        public void SetChanged()
        {
            this.OnEdit(null, EventArgs.Empty);
        }
    }
}
