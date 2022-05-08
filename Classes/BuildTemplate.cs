using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.ChatLinks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.BuildsManager
{
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
            new TemplateItem_json(){ _Slot = "Back"},
            new TemplateItem_json(){ _Slot = "Amulet"},
            new TemplateItem_json(){ _Slot = "Ring1"},
            new TemplateItem_json(){ _Slot = "Ring2"},
            new TemplateItem_json(){ _Slot = "Accessoire1"},
            new TemplateItem_json(){ _Slot = "Accessoire2"},
        };
        public List<Armor_TemplateItem_json> Armor = new List<Armor_TemplateItem_json>()
        {
            new Armor_TemplateItem_json(){ _Slot = "Helmet"},
            new Armor_TemplateItem_json(){ _Slot = "Shoulders"},
            new Armor_TemplateItem_json(){ _Slot = "Chest"},
            new Armor_TemplateItem_json(){ _Slot = "Gloves"},
            new Armor_TemplateItem_json(){ _Slot = "Leggings"},
            new Armor_TemplateItem_json(){ _Slot = "Boots"},
        };
        public List<Weapon_TemplateItem_json> Weapons = new List<Weapon_TemplateItem_json>()
        {
            new Weapon_TemplateItem_json(){_Slot = "Weapon1_MainHand"},
            new Weapon_TemplateItem_json(){_Slot = "Weapon1_OffHand"},
            new Weapon_TemplateItem_json(){_Slot = "Weapon2_MainHand"},
            new Weapon_TemplateItem_json(){_Slot = "Weapon2_OffHand"},
        };
        public List<AquaticWeapon_TemplateItem_json> AquaticWeapons = new List<AquaticWeapon_TemplateItem_json>() 
        {
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon1"},
            new AquaticWeapon_TemplateItem_json() {_Slot = "AquaticWeapon2"},
        };
    }
    public class Template_json
    {
        public string Profession;
        public int Specialization;
        public string Name;
        public GearTemplate_json Gear;
        public string BuildCode;
        public Template_json(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                var template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    Name = template.Name;
                    Gear = template.Gear;
                    BuildCode = template.BuildCode;
                }
            }
            else
            {
                Gear = new GearTemplate_json();
                BuildCode = "[&DQIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=]";
            }
        }
    }

    public class TemplateItem : TemplateItem_json
    {
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

    public class GearTemplate
    {
        public List<TemplateItem> Trinkets = new List<TemplateItem>()
        {
            new TemplateItem(){ _Slot = "Back"},
            new TemplateItem(){ _Slot = "Amulet"},
            new TemplateItem(){ _Slot = "Ring1"},
            new TemplateItem(){ _Slot = "Ring2"},
            new TemplateItem(){ _Slot = "Accessoire1"},
            new TemplateItem(){ _Slot = "Accessoire2"},
        };
        public List<Armor_TemplateItem> Armor = new List<Armor_TemplateItem>()
        {
            new Armor_TemplateItem(){ _Slot = "Helmet"},
            new Armor_TemplateItem(){ _Slot = "Shoulders"},
            new Armor_TemplateItem(){ _Slot = "Chest"},
            new Armor_TemplateItem(){ _Slot = "Gloves"},
            new Armor_TemplateItem(){ _Slot = "Leggings"},
            new Armor_TemplateItem(){ _Slot = "Boots"},
        };
        public List<Weapon_TemplateItem> Weapons = new List<Weapon_TemplateItem>() {
        new Weapon_TemplateItem(){ Slot = _EquipmentSlots.Weapon1_MainHand},
        new Weapon_TemplateItem(){ Slot = _EquipmentSlots.Weapon1_OffHand},
        new Weapon_TemplateItem(){ Slot = _EquipmentSlots.Weapon2_MainHand},
        new Weapon_TemplateItem(){ Slot = _EquipmentSlots.Weapon2_OffHand},
        };
        public List<AquaticWeapon_TemplateItem> AquaticWeapons = new List<AquaticWeapon_TemplateItem>()
        {
            new AquaticWeapon_TemplateItem(){ Slot = _EquipmentSlots.AquaticWeapon1},
            new AquaticWeapon_TemplateItem(){ Slot = _EquipmentSlots.AquaticWeapon2},
        };
    }

    public class Template
    {
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
        enum _AquaticWeaponSlots
        {
            AquaticWeapon1,
            AquaticWeapon2,
        }

        public Template_json Template_json;

        public string Name;
        public API.Profession Profession;
        public API.Specialization Specialization;

        public string Path;
        public GearTemplate Gear = new GearTemplate();
        public BuildTemplate Build;

        public Template(string path = default)
        {
            if (path != default && File.Exists(path))
            {
                var template = JsonConvert.DeserializeObject<Template_json>(File.ReadAllText(path));
                if (template != null)
                {
                    Template_json = template;
                    Name = template.Name;
                    Profession = BuildsManager.Data.Professions.Find(e => e.Id == template.Profession);
                    Specialization = Profession != null ? Profession.Specializations.Find(e => e.Id == template.Specialization) : null;

                    Path = path.Replace(Name + ".json", "");

                    foreach (Armor_TemplateItem_json jItem in Template_json.Gear.Armor)
                    {
                        var index = (int)Enum.Parse(typeof(_AmorSlots), jItem._Slot);
                        if (Gear.Armor[index] == null) Gear.Armor[index] = new Armor_TemplateItem();
                        Gear.Armor[index].Stat = BuildsManager.Data.Stats.Find(e => e.Id == jItem._Stat);
                        Gear.Armor[index].Rune = BuildsManager.Data.Runes.Find(e => e.Id == jItem._Rune);
                        Gear.Armor[index].Slot = (_EquipmentSlots)Enum.Parse(typeof(_EquipmentSlots), jItem._Slot);
                    }
                    foreach (TemplateItem_json jItem in Template_json.Gear.Trinkets)
                    {
                        var index = (int)Enum.Parse(typeof(_TrinketSlots), jItem._Slot);
                        if (Gear.Trinkets[index] == null) Gear.Trinkets[index] = new TemplateItem();
                        Gear.Trinkets[index].Stat = BuildsManager.Data.Stats.Find(e => e.Id == jItem._Stat);
                        Gear.Trinkets[index].Slot = (_EquipmentSlots)Enum.Parse(typeof(_EquipmentSlots), jItem._Slot);
                    }
                    foreach (Weapon_TemplateItem_json jItem in Template_json.Gear.Weapons)
                    {
                        var index = (int)Enum.Parse(typeof(_WeaponSlots), jItem._Slot);
                        if (Gear.Weapons[index] == null) Gear.Weapons[index] = new Weapon_TemplateItem();
                        Gear.Weapons[index].Stat = BuildsManager.Data.Stats.Find(e => e.Id == jItem._Stat);
                        Gear.Weapons[index].Sigil = BuildsManager.Data.Sigils.Find(e => e.Id == jItem._Sigil);
                        Gear.Weapons[index].Slot = (_EquipmentSlots)Enum.Parse(typeof(_EquipmentSlots), jItem._Slot);
                        Gear.Weapons[index].WeaponType = (API.weaponType)Enum.Parse(typeof(API.weaponType), jItem._WeaponType);
                    }
                    foreach (AquaticWeapon_TemplateItem_json jItem in Template_json.Gear.AquaticWeapons)
                    {
                        var index = (int)Enum.Parse(typeof(_AquaticWeaponSlots), jItem._Slot);
                        if (Gear.AquaticWeapons[index] == null) Gear.AquaticWeapons[index] = new AquaticWeapon_TemplateItem();
                        Gear.AquaticWeapons[index].Stat = BuildsManager.Data.Stats.Find(e => e.Id == jItem._Stat);
                        Gear.AquaticWeapons[index].Slot = (_EquipmentSlots)Enum.Parse(typeof(_EquipmentSlots), jItem._Slot);
                        Gear.AquaticWeapons[index].WeaponType = (API.weaponType)Enum.Parse(typeof(API.weaponType), jItem._WeaponType);
                        Gear.AquaticWeapons[index].Sigils = new List<API.SigilItem>();

                        if (jItem._Sigils != null)
                        {
                            foreach (int id in jItem._Sigils)
                            {
                                Gear.AquaticWeapons[index].Sigils.Add(BuildsManager.Data.Sigils.Find(e => e.Id == id));
                            }
                        }

                        for (int i = Gear.AquaticWeapons[index].Sigils.Count; i < 2; i++)
                        {
                            Gear.AquaticWeapons[index].Sigils.Add(new API.SigilItem());
                        }
                    }

                    Build = new BuildTemplate(Template_json.BuildCode);

                    Build.Changed += OnChanged;
                }
            }
            else
            {
                Gear = new GearTemplate();
                Build = new BuildTemplate();
                Name = "[No Name Set]";
                Template_json = new Template_json();

                var player = GameService.Gw2Mumble.PlayerCharacter;

                if (player != null)
                {
                    Profession = BuildsManager.Data.Professions.Find(e => e.Id == player.Profession.ToString());
                    Path = BuildsManager.Paths.builds;
                }

                SetChanged();
            }
        }

        public void Reset()
        {
            Name = "[No Name Set]";
            Template_json = new Template_json()
            {
                Name = Name
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

            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (player != null)
            {
                Profession = BuildsManager.Data.Professions.Find(e => e.Id == player.Profession.ToString());
                Path = BuildsManager.Paths.builds;
            }

            SetChanged();
        }

        public void Delete()
        {
            if (Path == null) return;
            if (Name == "[No Name Set]") return;
            File.Delete(Path + Name + ".json");
            BuildsManager.ModuleInstance.OnTemplate_Deleted();
        }

        public void Save()
        {
            if (Path == null) return;
            if (Name == "[No Name Set]") return;
            if (Template_json.Name != Name) File.Delete(Path + Template_json.Name + ".json");

            Template_json.Name = Name;
            Template_json.Profession = Profession != null ? Profession.Id : "Unkown";
            Template_json.Specialization = Specialization != null ? Specialization.Id : 0;

            Template_json.Gear = new GearTemplate_json();

            for (int i = 0; i < Gear.Trinkets.Count; i++)
            {
                var item = Gear.Trinkets[i];
                Template_json.Gear.Trinkets[i]._Stat = item.Stat != null ? item.Stat.Id : 0;
            }

            for (int i = 0; i < Gear.Armor.Count; i++)
            {
                var item = Gear.Armor[i];
                Template_json.Gear.Armor[i]._Stat = item.Stat != null ? item.Stat.Id : 0;
                Template_json.Gear.Armor[i]._Rune = item.Rune != null ? item.Rune.Id : 0;
            }

            for (int i = 0; i < Gear.Weapons.Count; i++)
            {
                var item = Gear.Weapons[i];
                Template_json.Gear.Weapons[i]._Stat = item.Stat != null ? item.Stat.Id : 0;
                Template_json.Gear.Weapons[i]._Sigil = item.Sigil != null ? item.Sigil.Id : 0;
                Template_json.Gear.Weapons[i]._WeaponType = item.WeaponType.ToString();
            }

            for (int i = 0; i < Gear.AquaticWeapons.Count; i++)
            {
                var item = Gear.AquaticWeapons[i];

                Template_json.Gear.AquaticWeapons[i]._Stat = item.Stat != null ? item.Stat.Id : 0;
                Template_json.Gear.AquaticWeapons[i]._Sigils = item.Sigils != null ? item.Sigils.Select(e => e != null ? e.Id : 0).ToList() : new List<int>();
                Template_json.Gear.AquaticWeapons[i]._WeaponType = item.WeaponType.ToString();
            }

            Template_json.BuildCode = Build.ParseBuildCode();

            var existsAlready = File.Exists(Path + Name + ".json");

            File.WriteAllText(Path + Name + ".json", JsonConvert.SerializeObject(Template_json));
            if (!existsAlready) BuildsManager.ModuleInstance.OnTemplate_Deleted();
        }

        public EventHandler Changed;
        private void OnChanged(object sender, EventArgs e)
        {
            this.Changed?.Invoke(this, EventArgs.Empty);
            Save();
        }
        public void SetChanged()
        {
            OnChanged(null, EventArgs.Empty);
        }
    }

    public class SpecLine
    {
        public int Index;
        private API.Specialization _Specialization;
        public API.Specialization Specialization
        {
            get => _Specialization;
            set
            {
                _Specialization = value;
                Traits = new List<API.Trait>();
                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }
        public List<API.Trait> Traits = new List<API.Trait>();
        public EventHandler Changed;
        public Specialization_Control Control;
    }

    public class BuildTemplate
    {
        private string _TemplateCode;
        public string TemplateCode
        {
            private set
            {
                _TemplateCode = value;
            }
            get
            {
                _TemplateCode = ParseBuildCode();
                return _TemplateCode;
            }
        }
        public API.Profession Profession;

        public List<SpecLine> SpecLines = new List<SpecLine>
        {
            new SpecLine(){ Index = 0},
            new SpecLine(){ Index = 1},
            new SpecLine(){ Index = 2}
        };
        public List<API.Skill> Skills_Terrestial = new List<API.Skill>
        {
            new API.Skill(),
            new API.Skill(),
            new API.Skill(),
            new API.Skill(),
            new API.Skill()
        };
        public List<API.Skill> Skills_Aquatic = new List<API.Skill>
        {
            new API.Skill(),
            new API.Skill(),
            new API.Skill(),
            new API.Skill(),
            new API.Skill()
        };
        public string ParseBuildCode()
        {
            string code = "";
            if (Profession != null)
            {
                BuildChatLink build = new BuildChatLink();
                build.Profession = (Gw2Sharp.Models.ProfessionType)Enum.Parse(typeof(Gw2Sharp.Models.ProfessionType), Profession.Id);

                build.AquaticHealingSkillPaletteId = Skills_Aquatic[0] != null && Skills_Aquatic[0].PaletteId != 0 ? (ushort)Skills_Aquatic[0].PaletteId : (ushort)0;
                build.AquaticUtility1SkillPaletteId = Skills_Aquatic[1] != null && Skills_Aquatic[1].PaletteId != 0 ? (ushort)Skills_Aquatic[1].PaletteId : (ushort)0;
                build.AquaticUtility2SkillPaletteId = Skills_Aquatic[2] != null && Skills_Aquatic[2].PaletteId != 0 ? (ushort)Skills_Aquatic[2].PaletteId : (ushort)0;
                build.AquaticUtility3SkillPaletteId = Skills_Aquatic[3] != null && Skills_Aquatic[3].PaletteId != 0 ? (ushort)Skills_Aquatic[3].PaletteId : (ushort)0;
                build.AquaticEliteSkillPaletteId = Skills_Aquatic[4] != null && Skills_Aquatic[4].PaletteId != 0 ? (ushort)Skills_Aquatic[4].PaletteId : (ushort)0;

                build.TerrestrialHealingSkillPaletteId = Skills_Terrestial[0] != null && Skills_Terrestial[0].PaletteId != 0 ? (ushort)Skills_Terrestial[0].PaletteId : (ushort)0;
                build.TerrestrialUtility1SkillPaletteId = Skills_Terrestial[1] != null && Skills_Terrestial[1].PaletteId != 0 ? (ushort)Skills_Terrestial[1].PaletteId : (ushort)0;
                build.TerrestrialUtility2SkillPaletteId = Skills_Terrestial[2] != null && Skills_Terrestial[2].PaletteId != 0 ? (ushort)Skills_Terrestial[2].PaletteId : (ushort)0;
                build.TerrestrialUtility3SkillPaletteId = Skills_Terrestial[3] != null && Skills_Terrestial[3].PaletteId != 0 ? (ushort)Skills_Terrestial[3].PaletteId : (ushort)0;
                build.TerrestrialEliteSkillPaletteId = Skills_Terrestial[4] != null && Skills_Terrestial[4].PaletteId != 0 ? (ushort)Skills_Terrestial[4].PaletteId : (ushort)0;


                SpecLine specLine;
                List<API.Trait> selectedTraits;

                specLine = SpecLines[0];
                selectedTraits = SpecLines[0].Traits;
                build.Specialization1Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization1Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization1Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization1Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }
                specLine = SpecLines[1];
                selectedTraits = SpecLines[1].Traits;
                build.Specialization2Id = specLine.Specialization != null ? (byte)specLine.Specialization.Id : (byte)0;
                if (specLine.Specialization != null)
                {
                    build.Specialization2Trait1Index = selectedTraits.Count > 0 && selectedTraits[0] != null ? (byte)(selectedTraits[0].Order + 1) : (byte)0;
                    build.Specialization2Trait2Index = selectedTraits.Count > 1 && selectedTraits[1] != null ? (byte)(selectedTraits[1].Order + 1) : (byte)0;
                    build.Specialization2Trait3Index = selectedTraits.Count > 2 && selectedTraits[2] != null ? (byte)(selectedTraits[2].Order + 1) : (byte)0;
                }
                specLine = SpecLines[2];
                selectedTraits = SpecLines[2].Traits;
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
                    TemplateCode = code;
                    build.Parse(chatlink.ToArray());

                    Profession = BuildsManager.Data.Professions.Find(e => e.Id == build.Profession.ToString());
                    if (Profession != null)
                    {
                        if (build.Specialization1Id != 0)
                        {
                            SpecLines[0].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization1Id);

                            if (SpecLines[0].Specialization != null)
                            {
                                SpecLines[0].Traits = new List<API.Trait>();

                                var spec = SpecLines[0].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization1Trait1Index, build.Specialization1Trait2Index, build.Specialization1Trait3Index });
                                var selectedTraits = SpecLines[0].Traits;
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
                            SpecLines[1].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization2Id);

                            if (SpecLines[1].Specialization != null)
                            {
                                SpecLines[1].Traits = new List<API.Trait>();

                                var spec = SpecLines[1].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization2Trait1Index, build.Specialization2Trait2Index, build.Specialization2Trait3Index });
                                var selectedTraits = SpecLines[1].Traits;
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
                            SpecLines[2].Specialization = Profession.Specializations.Find(e => e.Id == (int)build.Specialization3Id);

                            if (SpecLines[2].Specialization != null)
                            {
                                SpecLines[2].Traits = new List<API.Trait>();

                                var spec = SpecLines[2].Specialization;
                                var traits = new List<byte>(new byte[] { build.Specialization3Trait1Index, build.Specialization3Trait2Index, build.Specialization3Trait3Index });
                                var selectedTraits = SpecLines[2].Traits;
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
                            API.Skill skill = Profession.Skills.Find(e => e.PaletteId == pid);
                            if (skill != null) Skills_Terrestial[skillindex] = skill;
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
                            API.Skill skill = Profession.Skills.Find(e => e.PaletteId == pid);
                            if (skill != null) Skills_Aquatic[skillindex] = skill;
                            skillindex++;
                        }

                        foreach (SpecLine specLine in SpecLines)
                        {
                            specLine.Changed += OnChanged;
                        }
                    }
                }
            }
            else
            {
                var player = GameService.Gw2Mumble.PlayerCharacter;

                if (player != null)
                {
                    Profession = BuildsManager.Data.Professions.Find(e => e.Id == player.Profession.ToString());
                }
            }
        }

        public EventHandler Changed;
        private void OnChanged(object sender, EventArgs e)
        {
            BuildsManager.Logger.Debug("Template Changed: ");
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
