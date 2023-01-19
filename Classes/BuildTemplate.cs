namespace Kenedia.Modules.BuildsManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Blish_HUD.Controls;
    using Gw2Sharp.ChatLinks;
    using Newtonsoft.Json;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class TemplateItem_json
    {
        public string _Slot { get; set; }

        public int? _Stat { get; set; }
    }

    public class Armor_TemplateItem_json : TemplateItem_json
    {
        public int? _Rune;
    }

    public class Weapon_TemplateItem_json : TemplateItem_json
    {
        public string _WeaponType = "Unkown";
        public int? _Sigil;
    }

    public class AquaticWeapon_TemplateItem_json : TemplateItem_json
    {
        public string _WeaponType = "Unkown";
        public List<int> _Sigils = new List<int>();
    }

    public class GearTemplate_json
    {
        public List<TemplateItem_json> Trinkets = new List<TemplateItem_json>()
        {
            new TemplateItem_json() { _Slot = "Back"},
            new TemplateItem_json() { _Slot = "Amulet"},
            new TemplateItem_json() { _Slot = "Ring1"},
            new TemplateItem_json() { _Slot = "Ring2"},
            new TemplateItem_json() { _Slot = "Accessoire1"},
            new TemplateItem_json() { _Slot = "Accessoire2"},
        };

        public List<Armor_TemplateItem_json> Armor = new List<Armor_TemplateItem_json>()
        {
            new Armor_TemplateItem_json() { _Slot = "Helmet"},
            new Armor_TemplateItem_json() { _Slot = "Shoulders"},
            new Armor_TemplateItem_json() { _Slot = "Chest"},
            new Armor_TemplateItem_json() { _Slot = "Gloves"},
            new Armor_TemplateItem_json() { _Slot = "Leggings"},
            new Armor_TemplateItem_json() { _Slot = "Boots"},
        };

        public List<Weapon_TemplateItem_json> Weapons = new List<Weapon_TemplateItem_json>()
        {
            new Weapon_TemplateItem_json() {_Slot = "Weapon1_MainHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon1_OffHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon2_MainHand"},
            new Weapon_TemplateItem_json() {_Slot = "Weapon2_OffHand"},
        };

        public List<AquaticWeapon_TemplateItem_json> AquaticWeapons = new List<AquaticWeapon_TemplateItem_json>()
        {
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon1"},
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon2"},
        };
    }

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

    public class TemplateItem : TemplateItem_json, IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // Stat?.Dispose();
                this.Stat = null;
            }
        }

        public _EquipmentSlots Slot = _EquipmentSlots.Unkown;
        public API.Stat Stat;

        public Rectangle Bounds;
        public Rectangle StatBounds;
        public Rectangle UpgradeBounds;
        public bool Hovered;
    }

    public class Armor_TemplateItem : TemplateItem
    {
        public API.RuneItem Rune;
    }

    public class Weapon_TemplateItem : TemplateItem
    {
        public API.weaponType WeaponType = API.weaponType.Unkown;
        public API.SigilItem Sigil;
    }

    public class AquaticWeapon_TemplateItem : TemplateItem
    {
        public API.weaponType WeaponType = API.weaponType.Unkown;
        public List<API.SigilItem> Sigils = new List<API.SigilItem>()
        {
            new API.SigilItem(),
            new API.SigilItem(),
        };

        public List<Rectangle> SigilsBounds = new List<Rectangle>()
        {
            Rectangle.Empty,
            Rectangle.Empty,
        };
    }

    public class GearTemplate : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Trinkets = null;

                // Trinkets?.DisposeAll();
                this.Armor = null;

                // Armor?.DisposeAll();
                this.Weapons = null;

                // Weapons?.DisposeAll();
                this.AquaticWeapons = null;

                // AquaticWeapons?.DisposeAll();
            }
        }

        public List<TemplateItem> Trinkets = new List<TemplateItem>()
        {
            new TemplateItem() { _Slot = "Back"},
            new TemplateItem() { _Slot = "Amulet"},
            new TemplateItem() { _Slot = "Ring1"},
            new TemplateItem() { _Slot = "Ring2"},
            new TemplateItem() { _Slot = "Accessoire1"},
            new TemplateItem() { _Slot = "Accessoire2"},
        };

        public List<Armor_TemplateItem> Armor = new List<Armor_TemplateItem>()
        {
            new Armor_TemplateItem() { _Slot = "Helmet"},
            new Armor_TemplateItem() { _Slot = "Shoulders"},
            new Armor_TemplateItem() { _Slot = "Chest"},
            new Armor_TemplateItem() { _Slot = "Gloves"},
            new Armor_TemplateItem() { _Slot = "Leggings"},
            new Armor_TemplateItem() { _Slot = "Boots"},
        };

        public List<Weapon_TemplateItem> Weapons = new List<Weapon_TemplateItem>() {
        new Weapon_TemplateItem() { Slot = _EquipmentSlots.Weapon1_MainHand},
        new Weapon_TemplateItem() { Slot = _EquipmentSlots.Weapon1_OffHand},
        new Weapon_TemplateItem() { Slot = _EquipmentSlots.Weapon2_MainHand},
        new Weapon_TemplateItem() { Slot = _EquipmentSlots.Weapon2_OffHand},
        };

        public List<AquaticWeapon_TemplateItem> AquaticWeapons = new List<AquaticWeapon_TemplateItem>()
        {
            new AquaticWeapon_TemplateItem() { Slot = _EquipmentSlots.AquaticWeapon1},
            new AquaticWeapon_TemplateItem() { Slot = _EquipmentSlots.AquaticWeapon2},
        };

        public GearTemplate(string code = default)
        {
            if (code != default)
            {
                int j = 0;
                var ItemStrings = code.Split(']');
                if (ItemStrings.Length == 19)
                {
                    j = 0;
                    for (int i = 0; i < this.Trinkets.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        int id;
                        Int32.TryParse(ItemStrings[i], out id);
                        if (id > 0)
                        {
                            this.Trinkets[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == id);
                        }

                        BuildsManager.Logger.Debug("Trinkets[" + j + "].Stat: " + this.Trinkets[j].Stat?.Name);
                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count; i < this.Trinkets.Count + this.Armor.Count; i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.Armor[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Stat: " + this.Armor[j].Stat?.Name);

                        int rune_id;
                        Int32.TryParse(ids[1], out rune_id);
                        if (stat_id > 0)
                        {
                            this.Armor[j].Rune = BuildsManager.ModuleInstance.Data.Runes.Find(e => e.Id == rune_id);
                        }

                        BuildsManager.Logger.Debug("Armor[" + j + "].Rune: " + this.Armor[j].Rune?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count + this.Armor.Count; i < (this.Trinkets.Count + this.Armor.Count + this.Weapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.Weapons[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Stat: " + this.Weapons[j].Stat?.Name);

                        int weaponType = -1;
                        Int32.TryParse(ids[1], out weaponType);
                        this.Weapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("Weapons[" + j + "].WeaponType: " + this.Weapons[j].WeaponType.ToString());

                        int sigil_id;
                        Int32.TryParse(ids[2], out sigil_id);
                        if (stat_id > 0)
                        {
                            this.Weapons[j].Sigil = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil_id);
                        }

                        BuildsManager.Logger.Debug("Weapons[" + j + "].Sigil: " + this.Weapons[j].Sigil?.Name);

                        j++;
                    }

                    j = 0;
                    for (int i = this.Trinkets.Count + this.Armor.Count + this.Weapons.Count; i < (this.Trinkets.Count + this.Armor.Count + this.Weapons.Count + this.AquaticWeapons.Count); i++)
                    {
                        ItemStrings[i] = ItemStrings[i].Replace("[", string.Empty).Replace("]", string.Empty);
                        var ids = ItemStrings[i].Split('|');

                        int stat_id;
                        Int32.TryParse(ids[0], out stat_id);
                        if (stat_id > 0)
                        {
                            this.AquaticWeapons[j].Stat = BuildsManager.ModuleInstance.Data.Stats.Find(e => e.Id == stat_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Stat: " + this.AquaticWeapons[j].Stat?.Name);

                        int weaponType = -1;
                        Int32.TryParse(ids[1], out weaponType);
                        this.AquaticWeapons[j].WeaponType = (API.weaponType)weaponType;
                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].WeaponType: " + this.AquaticWeapons[j].WeaponType.ToString());

                        int sigil1_id;
                        Int32.TryParse(ids[2], out sigil1_id);
                        if (sigil1_id > 0)
                        {
                            this.AquaticWeapons[j].Sigils[0] = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil1_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + this.AquaticWeapons[j].Sigils[0]?.Name);

                        int sigil2_id;
                        Int32.TryParse(ids[3], out sigil2_id);
                        if (sigil2_id > 0)
                        {
                            this.AquaticWeapons[j].Sigils[1] = BuildsManager.ModuleInstance.Data.Sigils.Find(e => e.Id == sigil2_id);
                        }

                        BuildsManager.Logger.Debug("AquaticWeapons[" + j + "].Sigil: " + this.AquaticWeapons[j].Sigils[1]?.Name);

                        j++;
                    }
                }
            }
        }

        public string TemplateCode
        {
            get
            {
                string code = string.Empty;

                foreach (TemplateItem item in this.Trinkets)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "]";
                }

                foreach (Armor_TemplateItem item in this.Armor)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + (item.Rune != null ? item.Rune.Id : 0) + "]";
                }

                foreach (Weapon_TemplateItem item in this.Weapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigil != null ? item.Sigil.Id : 0) + "]";
                }

                foreach (AquaticWeapon_TemplateItem item in this.AquaticWeapons)
                {
                    code += "[" + (item.Stat != null ? item.Stat.Id : 0) + "|" + ((int)item.WeaponType) + "|" + (item.Sigils[0] != null ? item.Sigils[0].Id : 0) + "|" + (item.Sigils[1] != null ? item.Sigils[1].Id : 0) + "]";
                }

                return code;
            }
        }
    }

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

    public class SpecLine : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this._Specialization?.Dispose();

                // Specialization?.Dispose();
                this.Traits?.DisposeAll();
                this.Control?.Dispose();
            }
        }

        public int Index;
        private API.Specialization _Specialization;

        public API.Specialization Specialization
        {
            get => this._Specialization;
            set
            {
                this._Specialization = value;
                this.Traits = new List<API.Trait>();
                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<API.Trait> Traits = new List<API.Trait>();
        public EventHandler Changed;
        public Specialization_Control Control;
    }

    public class BuildTemplate : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                foreach (SpecLine specLine in this.SpecLines)
                {
                    if (specLine != null)
                    {
                        specLine.Changed -= this.OnChanged;
                    }
                }

                // Profession?.Dispose();
                // SpecLines?.DisposeAll();
                // Skills_Terrestrial?.DisposeAll();
                // InactiveSkills_Terrestrial?.DisposeAll();
                // Skills_Aquatic?.DisposeAll();
                // InactiveSkills_Aquatic?.DisposeAll();
                // Legends_Terrestrial?.DisposeAll();
                // Legends_Aquatic?.DisposeAll();

                this.Profession = null;
                this.SpecLines = null;
                this.Skills_Terrestrial = null;
                this.InactiveSkills_Terrestrial = null;
                this.Skills_Aquatic = null;
                this.InactiveSkills_Aquatic = null;
                this.Legends_Terrestrial = null;
                this.Legends_Aquatic = null;
            }
        }

        private string _TemplateCode;

        public string TemplateCode
        {
            private set
            {
                this._TemplateCode = value;
            }

            get
            {
                this._TemplateCode = this.ParseBuildCode();
                return this._TemplateCode;
            }
        }

        public API.Profession Profession;

        public List<SpecLine> SpecLines = new List<SpecLine>
        {
            new SpecLine() { Index = 0},
            new SpecLine() { Index = 1},
            new SpecLine() { Index = 2},
        };

        public List<API.Skill> Skills_Terrestrial = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Terrestrial = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> Skills_Aquatic = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Skill> InactiveSkills_Aquatic = new List<API.Skill>
        {
            new API.Skill() { PaletteId = 4572},
            new API.Skill() { PaletteId = 4614},
            new API.Skill() { PaletteId = 4651},
            new API.Skill() { PaletteId = 4564},
            new API.Skill() { PaletteId = 4554},
        };

        public List<API.Legend> Legends_Terrestrial = new List<API.Legend>()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public List<API.Legend> Legends_Aquatic = new List<API.Legend>()
        {
            new API.Legend(),
            new API.Legend(),
        };

        public string ParseBuildCode()
        {
            string code = string.Empty;
            if (this.Profession != null)
            {
                BuildChatLink build = new BuildChatLink();
                build.Profession = (Gw2Sharp.Models.ProfessionType)Enum.Parse(typeof(Gw2Sharp.Models.ProfessionType), this.Profession.Id);
                var rev = build.Profession == Gw2Sharp.Models.ProfessionType.Revenant;

                build.RevenantActiveTerrestrialLegend = (byte)(rev && this.Legends_Terrestrial[0]?.Id != null ? this.Legends_Terrestrial[0]?.Id : 0);
                build.RevenantInactiveTerrestrialLegend = (byte)(rev && this.Legends_Terrestrial[1]?.Id != null ? this.Legends_Terrestrial[1]?.Id : 0);
                build.RevenantInactiveTerrestrialUtility1SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[1]?.PaletteId != null ? this.InactiveSkills_Terrestrial[1]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility2SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[2]?.PaletteId != null ? this.InactiveSkills_Terrestrial[2]?.PaletteId : 0);
                build.RevenantInactiveTerrestrialUtility3SkillPaletteId = (ushort)(rev && this.InactiveSkills_Terrestrial[3]?.PaletteId != null ? this.InactiveSkills_Terrestrial[3]?.PaletteId : 0);

                build.RevenantActiveAquaticLegend = (byte)(rev && this.Legends_Aquatic[0]?.Id != null ? this.Legends_Aquatic[0]?.Id : 0);
                build.RevenantInactiveAquaticLegend = (byte)(rev && this.Legends_Aquatic[1]?.Id != null ? this.Legends_Aquatic[1]?.Id : 0);
                build.RevenantInactiveAquaticUtility1SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[1]?.PaletteId != null ? this.InactiveSkills_Aquatic[1]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility2SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[2]?.PaletteId != null ? this.InactiveSkills_Aquatic[2]?.PaletteId : 0);
                build.RevenantInactiveAquaticUtility3SkillPaletteId = (ushort)(rev && this.InactiveSkills_Aquatic[3]?.PaletteId != null ? this.InactiveSkills_Aquatic[3]?.PaletteId : 0);

                build.TerrestrialHealingSkillPaletteId = (ushort)(this.Skills_Terrestrial[0]?.Id > 0 ? this.Skills_Terrestrial[0]?.PaletteId : 0);
                build.TerrestrialUtility1SkillPaletteId = (ushort)(this.Skills_Terrestrial[1]?.Id > 0 ? this.Skills_Terrestrial[1]?.PaletteId : 0);
                build.TerrestrialUtility2SkillPaletteId = (ushort)(this.Skills_Terrestrial[2]?.Id > 0 ? this.Skills_Terrestrial[2]?.PaletteId : 0);
                build.TerrestrialUtility3SkillPaletteId = (ushort)(this.Skills_Terrestrial[3]?.Id > 0 ? this.Skills_Terrestrial[3]?.PaletteId : 0);
                build.TerrestrialEliteSkillPaletteId = (ushort)(this.Skills_Terrestrial[4]?.Id > 0 ? this.Skills_Terrestrial[4]?.PaletteId : 0);

                build.AquaticHealingSkillPaletteId = (ushort)(this.Skills_Aquatic[0]?.Id > 0 ? this.Skills_Aquatic[0]?.PaletteId : 0);
                build.AquaticUtility1SkillPaletteId = (ushort)(this.Skills_Aquatic[1]?.Id > 0 ? this.Skills_Aquatic[1]?.PaletteId : 0);
                build.AquaticUtility2SkillPaletteId = (ushort)(this.Skills_Aquatic[2]?.Id > 0 ? this.Skills_Aquatic[2]?.PaletteId : 0);
                build.AquaticUtility3SkillPaletteId = (ushort)(this.Skills_Aquatic[3]?.Id > 0 ? this.Skills_Aquatic[3]?.PaletteId : 0);
                build.AquaticEliteSkillPaletteId = (ushort)(this.Skills_Aquatic[4]?.Id > 0 ? this.Skills_Aquatic[4]?.PaletteId : 0);

                SpecLine specLine;
                List<API.Trait> selectedTraits;

                specLine = this.SpecLines[0];
                selectedTraits = this.SpecLines[0].Traits;
                build.Specialization1Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization1Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization1Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization1Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = this.SpecLines[1];
                selectedTraits = this.SpecLines[1].Traits;
                build.Specialization2Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization2Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization2Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization2Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                specLine = this.SpecLines[2];
                selectedTraits = this.SpecLines[2].Traits;
                build.Specialization3Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization3Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization3Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization3Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }

                var bytes = build.ToArray();
                build.Parse(bytes);

                code = build.ToString();
            }

            return code;
        }

        public BuildTemplate(string code = default)
        {
            if (code != default)
            {
                BuildChatLink build = new BuildChatLink();
                IGw2ChatLink chatlink = null;
                if (BuildChatLink.TryParse(code, out chatlink))
                {
                    this.TemplateCode = code;
                    build.Parse(chatlink.ToArray());

                    this.Profession = BuildsManager.ModuleInstance.Data.Professions.Find(e => e.Id == build.Profession.ToString());
                    if (this.Profession != null)
                    {
                        if (build.Specialization1Id != 0)
                        {
                            this.SpecLines[0].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization1Id);

                            if (this.SpecLines[0].Specialization != null)
                            {
                                this.SpecLines[0].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[0].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index });
                                var selectedTraits = this.SpecLines[0].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        if (build.Specialization2Id != 0)
                        {
                            this.SpecLines[1].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization2Id);

                            if (this.SpecLines[1].Specialization != null)
                            {
                                this.SpecLines[1].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[1].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index });
                                var selectedTraits = this.SpecLines[1].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        if (build.Specialization3Id != 0)
                        {
                            this.SpecLines[2].Specialization = this.Profession.Specializations.Find(e => e.Id == (int)build.Specialization3Id);

                            if (this.SpecLines[2].Specialization != null)
                            {
                                this.SpecLines[2].Traits = new List<API.Trait>();

                                var spec = this.SpecLines[2].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index });
                                var selectedTraits = this.SpecLines[2].Traits;
                                int traitIndex = 0;

                                foreach (byte bit in traits)
                                {
                                    if (bit > 0)
                                    {
                                        selectedTraits.Add(spec.MajorTraits[(traitIndex * 3) + bit - 1]);
                                    }

                                    traitIndex++;
                                }
                            }
                        }

                        // [&DQkDJg8mPz3cEQAABhIAACsSAADUEQAAyhEAAAUCAADUESsSBhIAAAAAAAA=]
                        if (this.Profession.Id == "Revenant")
                        {
                            this.Legends_Terrestrial[0] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveTerrestrialLegend);
                            this.Legends_Terrestrial[1] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveTerrestrialLegend);

                            if (this.Legends_Terrestrial[0] != null)
                            {
                                var legend = this.Legends_Terrestrial[0];
                                this.Skills_Terrestrial[0] = legend.Heal;
                                this.Skills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility1SkillPaletteId);
                                this.Skills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility2SkillPaletteId);
                                this.Skills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.TerrestrialUtility3SkillPaletteId);
                                this.Skills_Terrestrial[4] = legend.Elite;
                            }

                            if (this.Legends_Terrestrial[1] != null)
                            {
                                var legend = this.Legends_Terrestrial[1];
                                this.InactiveSkills_Terrestrial[0] = legend.Heal;
                                this.InactiveSkills_Terrestrial[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility1SkillPaletteId);
                                this.InactiveSkills_Terrestrial[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility2SkillPaletteId);
                                this.InactiveSkills_Terrestrial[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveTerrestrialUtility3SkillPaletteId);
                                this.InactiveSkills_Terrestrial[4] = legend.Elite;
                            }

                            this.Legends_Aquatic[0] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantActiveAquaticLegend);
                            this.Legends_Aquatic[1] = this.Profession.Legends.Find(e => e.Id == (int)build.RevenantInactiveAquaticLegend);

                            if (this.Legends_Aquatic[0] != null)
                            {
                                var legend = this.Legends_Aquatic[0];
                                this.Skills_Aquatic[0] = legend.Heal;
                                this.Skills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility1SkillPaletteId);
                                this.Skills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility2SkillPaletteId);
                                this.Skills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.AquaticUtility3SkillPaletteId);
                                this.Skills_Aquatic[4] = legend.Elite;
                            }

                            if (this.Legends_Aquatic[1] != null)
                            {
                                var legend = this.Legends_Aquatic[1];
                                this.InactiveSkills_Aquatic[0] = legend.Heal;
                                this.InactiveSkills_Aquatic[1] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility1SkillPaletteId);
                                this.InactiveSkills_Aquatic[2] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility2SkillPaletteId);
                                this.InactiveSkills_Aquatic[3] = legend.Utilities.Find(e => e.PaletteId == (int)build.RevenantInactiveAquaticUtility3SkillPaletteId);
                                this.InactiveSkills_Aquatic[4] = legend.Elite;
                            }
                        }
                        else
                        {
                            List<ushort> Terrestrial_PaletteIds = new List<ushort>{
                                build.TerrestrialHealingSkillPaletteId,
                                build.TerrestrialUtility1SkillPaletteId,
                                build.TerrestrialUtility2SkillPaletteId,
                                build.TerrestrialUtility3SkillPaletteId,
                                build.TerrestrialEliteSkillPaletteId,
                            };

                            int skillindex = 0;
                            foreach (ushort pid in Terrestrial_PaletteIds)
                            {
                                API.Skill skill = this.Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    this.Skills_Terrestrial[skillindex] = skill;
                                }

                                skillindex++;
                            }

                            List<ushort> Aqua_PaletteIds = new List<ushort>{
                                build.AquaticHealingSkillPaletteId,
                                build.AquaticUtility1SkillPaletteId,
                                build.AquaticUtility2SkillPaletteId,
                                build.AquaticUtility3SkillPaletteId,
                                build.AquaticEliteSkillPaletteId,
                            };
                            skillindex = 0;
                            foreach (ushort pid in Aqua_PaletteIds)
                            {
                                API.Skill skill = this.Profession.Skills.Find(e => e.PaletteId == pid);
                                if (skill != null)
                                {
                                    this.Skills_Aquatic[skillindex] = skill;
                                }

                                skillindex++;
                            }
                        }

                        foreach (SpecLine specLine in this.SpecLines)
                        {
                            specLine.Changed += this.OnChanged;
                        }
                    }
                }
            }
            else
            {
                this.Profession = BuildsManager.ModuleInstance.CurrentProfession;
            }
        }

        public void SwapLegends()
        {
            if (this.Profession.Id == "Revenant")
            {
                var tLegend = this.Legends_Terrestrial[0];
                this.Legends_Terrestrial[0] = this.Legends_Terrestrial[1];
                this.Legends_Terrestrial[1] = tLegend;

                var tSkill1 = this.Skills_Terrestrial[1];
                var tSkill2 = this.Skills_Terrestrial[2];
                var tSkill3 = this.Skills_Terrestrial[3];

                this.Skills_Terrestrial[0] = this.Legends_Terrestrial[0]?.Heal;
                this.Skills_Terrestrial[1] = this.InactiveSkills_Terrestrial[1];
                this.Skills_Terrestrial[2] = this.InactiveSkills_Terrestrial[2];
                this.Skills_Terrestrial[3] = this.InactiveSkills_Terrestrial[3];
                this.Skills_Terrestrial[4] = this.Legends_Terrestrial[0]?.Elite;

                this.InactiveSkills_Terrestrial[1] = tSkill1;
                this.InactiveSkills_Terrestrial[2] = tSkill2;
                this.InactiveSkills_Terrestrial[3] = tSkill3;

                tLegend = this.Legends_Aquatic[0];
                this.Legends_Aquatic[0] = this.Legends_Aquatic[1];
                this.Legends_Aquatic[1] = tLegend;

                tSkill1 = this.Skills_Aquatic[1];
                tSkill2 = this.Skills_Aquatic[2];
                tSkill3 = this.Skills_Aquatic[3];

                this.Skills_Aquatic[0] = this.Legends_Aquatic[0]?.Heal;
                this.Skills_Aquatic[1] = this.InactiveSkills_Aquatic[1];
                this.Skills_Aquatic[2] = this.InactiveSkills_Aquatic[2];
                this.Skills_Aquatic[3] = this.InactiveSkills_Aquatic[3];
                this.Skills_Aquatic[4] = this.Legends_Aquatic[0]?.Elite;

                this.InactiveSkills_Aquatic[1] = tSkill1;
                this.InactiveSkills_Aquatic[2] = tSkill2;
                this.InactiveSkills_Aquatic[3] = tSkill3;
                if (BuildsManager.ModuleInstance.Selected_Template.Build == this)
                {
                    BuildsManager.ModuleInstance.OnSelected_Template_Edit(null, null);
                }
            }
        }

        public EventHandler Changed;

        private void OnChanged(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
