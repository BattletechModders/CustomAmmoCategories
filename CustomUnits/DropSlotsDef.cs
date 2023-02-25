/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Data;
using HBS.Collections;
using Newtonsoft.Json;
using SVGImporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using static BattleTech.Data.DataManager;

namespace CustomUnits {
  public class HotDropDefinition {
    public MechDef mechDef { get; set; }
    public Pilot pilot { get; set; }
    public string TeamGUID { get; set; }
    public HotDropDefinition(MechDef mech, Pilot pilot, string team) {
      this.mechDef = mech;
      this.pilot = pilot;
      this.TeamGUID = team;
    }
  }
  public static class DropSystemHelper {
    public static readonly string CURRENT_DROP_LAYOUT_STAT_NAME = "CURRENT_DROP_LAYOUT";
    public static DropSlotDef fallbackSlot = new DropSlotDef();
    public static DropSlotDef fallbackDisabledSlot = new DropSlotDef();
    private static DropLanceDef fallbackLance = new DropLanceDef();
    private static DropSlotsDef fallbackLayout = new DropSlotsDef();
    private static Dictionary<string, DropClassDef> dropClasses = new Dictionary<string, DropClassDef>();
    private static Dictionary<string, DropSlotDef> dropSlots = new Dictionary<string, DropSlotDef>();
    private static Dictionary<string, DropLanceDef> dropLances = new Dictionary<string, DropLanceDef>();
    private static Dictionary<string, DropSlotsDef> dropLayouts = new Dictionary<string, DropSlotsDef>();
    private static Dictionary<string, DropSlotDecorationDef> dropDecoration = new Dictionary<string, DropSlotDecorationDef>();
    public static DropClassDef DEFAULT_MECH_DROP_CLASS { get; private set; } = null;
    public static DropClassDef DEFAULT_VEHICLE_DROP_CLASS { get; private set; } = null;
    public static DropSlotsDef currentLayout(this SimGameState sim) {
      if (sim == null) {
        return DropSystemHelper.getLayout(Core.Settings.DefaultSkirmishDropLayout);
      }
      Statistic curLayoutId = sim.CompanyStats.GetStatistic(CURRENT_DROP_LAYOUT_STAT_NAME);
      if (curLayoutId == null) { curLayoutId = sim.CompanyStats.AddStatistic<string>(CURRENT_DROP_LAYOUT_STAT_NAME,Core.Settings.DefaultSimgameDropLayout); }
      return DropSystemHelper.getLayout(curLayoutId.Value<string>());
    }
    public static void Register(this DropSlotDef def) {
      if (dropSlots.ContainsKey(def.Description.Id)) { dropSlots[def.Description.Id] = def; } else { dropSlots.Add(def.Description.Id, def); }
    }
    public static void Register(this DropLanceDef def) {
      if (dropLances.ContainsKey(def.Description.Id)) { dropLances[def.Description.Id] = def; } else { dropLances.Add(def.Description.Id, def); }
    }
    public static void Register(this DropSlotsDef def) {
      if (dropLayouts.ContainsKey(def.Description.Id)) { dropLayouts[def.Description.Id] = def; } else { dropLayouts.Add(def.Description.Id, def); }
    }
    public static void Register(this DropSlotDecorationDef def) {
      if (dropDecoration.ContainsKey(def.Description.Id)) { dropDecoration[def.Description.Id] = def; } else { dropDecoration.Add(def.Description.Id, def); }
    }
    public static void Register(this DropClassDef def) {
      if (dropClasses.ContainsKey(def.Description.Id)) { dropClasses[def.Description.Id] = def; } else { dropClasses.Add(def.Description.Id, def); }
    }
    public static DropSlotsDef getLayout(string name) {
      if (dropLayouts.TryGetValue(name, out DropSlotsDef result)) {
        return result;
      }
      Log.TWL(0,"Can't find drop layout definition "+name,true);
      return fallbackLayout;
    }
    public static DropLanceDef getLance(string name) {
      if (dropLances.TryGetValue(name, out DropLanceDef result)) {
        return result;
      }
      Log.TWL(0, "Can't find drop lance definition " + name,true);
      return fallbackLance;
    }
    public static DropSlotDef getSlot(string name) {
      if (dropSlots.TryGetValue(name, out DropSlotDef result)) {
        return result;
      }
      Log.TWL(0, "Can't find drop slot definition " + name,true);
      return fallbackSlot;
    }
    public static DropSlotDecorationDef getDecoration(string name) {
      if (dropDecoration.TryGetValue(name, out DropSlotDecorationDef result)) {
        return result;
      }
      Log.TWL(0, "Can't find drop decoration definition " + name, true);
      return null;
    }
    public static void Validate() {
      fallbackSlot.Description.Id = "fallback_slot";
      fallbackSlot.Description.Name = "default slot";
      fallbackSlot.Description.Details = "default slot";
      fallbackDisabledSlot.Description.Id = "fallback_slot";
      fallbackDisabledSlot.Description.Name = "default slot";
      fallbackDisabledSlot.Description.Details = "default slot";
      fallbackDisabledSlot.Disabled = true;
      fallbackLance.Description.Id = "fallback_lance";
      fallbackLance.Description.Name = "default lance";
      fallbackLance.Description.Details = "default lance";
      fallbackLance.DropSlots = new List<string>() { "fallback_slot", "fallback_slot", "fallback_slot", "fallback_slot" };
      fallbackLance.dropSlots = new List<DropSlotDef>() { fallbackSlot, fallbackSlot, fallbackSlot, fallbackSlot };
      fallbackLance.OverallUnits = 4;
      fallbackLayout.Description.Id = "fallback_layout";
      fallbackLayout.Description.Name = "default drop layout";
      fallbackLayout.Description.Details = "default drop layout";
      fallbackLayout.DropLances = new List<string>() { "fallback_lance" };
      fallbackLayout.dropLances = new List<DropLanceDef>() { fallbackLance };
      fallbackLayout.OverallUnits = 4;
      foreach (var lance in dropLances) {
        lance.Value.dropSlots.Clear();
        foreach (string slotid in lance.Value.DropSlots) {
          lance.Value.dropSlots.Add(DropSystemHelper.getSlot(slotid));
        }
        for (int t = lance.Value.DropSlots.Count; t < 4; ++t) {
          lance.Value.dropSlots.Add(fallbackDisabledSlot);
        }
      }
      foreach (var layout in dropLayouts) {
        layout.Value.dropLances.Clear();
        foreach (string lanceid in layout.Value.DropLances) {
          layout.Value.dropLances.Add(DropSystemHelper.getLance(lanceid));
        }
      }
      foreach(var slot in dropSlots) {
        slot.Value.decorations.Clear();
        foreach(string decoration_id in slot.Value.Decorations) {
          DropSlotDecorationDef decoration = DropSystemHelper.getDecoration(decoration_id);
          if (decoration != null) { slot.Value.decorations.Add(decoration); }
        }
      }
      if (dropClasses.TryGetValue(DropClassDef.DEFAULT_MECH_DROP_NAME, out DropClassDef GenericMechClass) == false) {
        GenericMechClass = new DropClassDef();
        GenericMechClass.Description.Id = DropClassDef.DEFAULT_MECH_DROP_NAME;
        GenericMechClass.Description.Name = DropClassDef.DEFAULT_MECH_DROP_UINAME;
        dropClasses.Add(DropClassDef.DEFAULT_MECH_DROP_NAME, GenericMechClass);
      }
      if (dropClasses.TryGetValue(DropClassDef.DEFAULT_VEHICLE_DROP_NAME, out DropClassDef GenericVehicleClass) == false) {
        GenericVehicleClass = new DropClassDef();
        GenericVehicleClass.Description.Id = DropClassDef.DEFAULT_VEHICLE_DROP_NAME;
        GenericVehicleClass.Description.Name = DropClassDef.DEFAULT_VEHICLE_DROP_UINAME;
        dropClasses.Add(DropClassDef.DEFAULT_VEHICLE_DROP_NAME, GenericVehicleClass);
      }
      if (GenericMechClass.unitTags.Count == 0) { GenericMechClass.unitTags.Add(DropClassDef.DEFAULT_MECH_UNIT_TAG); }
      if (GenericMechClass.slotTags.Count == 0) { GenericMechClass.slotTags.Add(DropClassDef.DEFAULT_MECH_DROP_TAG); }
      if (GenericVehicleClass.unitTags.Count == 0) { GenericVehicleClass.unitTags.Add(DropClassDef.DEFAULT_VEHICLE_UNIT_TAG); }
      if (GenericVehicleClass.slotTags.Count == 0) { GenericVehicleClass.slotTags.Add(DropClassDef.DEFAULT_VEHICLE_DROP_TAG); }
      DEFAULT_MECH_DROP_CLASS = GenericMechClass;
      DEFAULT_VEHICLE_DROP_CLASS = GenericVehicleClass;
      foreach (var pilotingClass0 in dropClasses) {
        if (pilotingClass0.Value.unitTags.IsEmpty) {
          throw new Exception("drop class " + pilotingClass0.Key + " unit tags set is empty. Fix this");
        }
        if (pilotingClass0.Value.slotTags.IsEmpty) {
          throw new Exception("drop class " + pilotingClass0.Key + " slot tags set is empty. Fix this");
        }
      }
    }
    private static ConcurrentDictionary<string, HashSet<DropClassDef>> dropClassesCacheBySlot = new ConcurrentDictionary<string, HashSet<DropClassDef>>();
    public static HashSet<DropClassDef> GetDropClass(this DropSlotDef slot) {
      if (dropClassesCacheBySlot.TryGetValue(slot.Description.Id, out HashSet<DropClassDef> result)) { return result; }
      result = new HashSet<DropClassDef>();
      foreach (var dropClass in dropClasses) {
        if (dropClass.Value.isMySlotClass(slot.tags)) { result.Add(dropClass.Value); }
      }
      dropClassesCacheBySlot.TryAdd(slot.Description.Id, result);
      return result;
    }
    public static bool CanBeDropedInto(this ChassisDef chassis, DropSlotDef slot, out string title, out string message) {
      title = string.Empty;
      message = string.Empty;
      HashSet<DropClassDef> chassis_dclasses = chassis.GetDropClass();
      bool result = true;
      StringBuilder fail_classes = new StringBuilder();
      Log.TWL(0, "CanBeDropedInto:" +chassis.Description.Id+" slot:"+slot.Description.Id);
      foreach (DropClassDef dclass in chassis_dclasses) {
        Log.WL(1, "chassis class:" + dclass.Description.Id);
        if (dclass.isMySlotClass(slot.tags) == false) {
          if (result == false) { fail_classes.Append(", "); }
          fail_classes.Append(dclass.Description.Name);
          result = false;
        }
      }
      if (result == false) {
        title = new Localize.Text("__/DROP.CANT_FIT/__", slot.Description.Name, chassis.Description.Name).ToString();
        message = new Localize.Text("__/DROP.CANT_FIT_MESSAGE/__", chassis.Description.Name, slot.Description.Name, fail_classes.ToString()).ToString();
      }
      return result;
    }
    private static ConcurrentDictionary<string, HashSet<DropClassDef>> dropClassesCache = new ConcurrentDictionary<string, HashSet<DropClassDef>>();
    public static void fallbackDropClass(this ChassisDef chassis) {
      UnitCustomInfo info = chassis.GetCustomInfo();
      if (info.FakeVehicle) {
        if (dropClasses.TryGetValue(DropClassDef.DEFAULT_VEHICLE_DROP_NAME, out DropClassDef pclass)) {
          chassis.ChassisTags.UnionWith(pclass.unitTags);
          if (dropClassesCache.TryGetValue(chassis.Description.Id, out HashSet<DropClassDef> result) == false) {
            result = new HashSet<DropClassDef>();
            dropClassesCache.TryAdd(chassis.Description.Id, result);
          }
          result.Add(pclass);
        }
      } else {
        if (dropClasses.TryGetValue(DropClassDef.DEFAULT_MECH_DROP_NAME, out DropClassDef pclass)) {
          chassis.ChassisTags.UnionWith(pclass.unitTags);
          if (dropClassesCache.TryGetValue(chassis.Description.Id, out HashSet<DropClassDef> result) == false) {
            result = new HashSet<DropClassDef>();
            dropClassesCache.TryAdd(chassis.Description.Id, result);
          }
          result.Add(pclass);
        }
      }
    }
    public static HashSet<DropClassDef> GetDropClass(this ChassisDef chassis, bool exception = true) {
      if (dropClassesCache.TryGetValue(chassis.Description.Id, out HashSet<DropClassDef> result)) { return result; }
      result = new HashSet<DropClassDef>();
      foreach (var dropClass in dropClasses) {
        if (dropClass.Value.isMyUnitClass(chassis.ChassisTags)) { result.Add(dropClass.Value); }
      }
      dropClassesCache.TryAdd(chassis.Description.Id, result);
      return result;
    }
  }
  public class DropDescriptionDef {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    public string Icon { get; set; }
  }
  public class DropSlotDef {
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    [JsonIgnore]
    public TagSet tags { get; private set; } = new TagSet();
    public List<string> Tags { set { tags = new TagSet(value); } }
    [JsonIgnore]
    public List<DropSlotDecorationDef> decorations { get;  set; } = new List<DropSlotDecorationDef>();
    public List<string> Decorations { get; set; } = new List<string>();
    public bool Disabled { get; set; } = false;
    public bool PlayerControl { get; set; } = true;
    public bool HotDrop { get; set; } = false;
    public bool UseMaxUnits { get; set; } = true;
  }
  public class DropLanceDef {
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    public int OverallUnits { get; set; } = 4;
    public List<DropSlotDef> dropSlots { get; set; } = new List<DropSlotDef>();
    public List<string> DropSlots { get; set; } = new List<string>();
  }
  public class DropSlotsDef {
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    public int OverallUnits { get; set; } = 4;
    [JsonIgnore]
    public List<DropLanceDef> dropLances { get; set; } = new List<DropLanceDef>();
    public List<string> DropLances { get; set; } = new List<string>();
    [JsonIgnore]
    private int f_slotsCount = -1;
    [JsonIgnore]
    public int slotsCount {
      get {
        if (f_slotsCount >= 0) { return f_slotsCount; }
        f_slotsCount = 0;
        foreach (DropLanceDef lance in dropLances) { f_slotsCount += lance.dropSlots.Count; }
        return f_slotsCount;
      }
    }
    public DropSlotDef GetSlotByIndex(int index) {
      int lance_index = 0;
      while (dropLances[lance_index].dropSlots.Count <= index) {
        index -= dropLances[lance_index].dropSlots.Count;
        ++lance_index;
        if (lance_index >= this.dropLances.Count) { return DropSystemHelper.fallbackSlot; }
      }
      return dropLances[lance_index].dropSlots[index];
    }
  }
  public class DropSlotDecorationDef: ILoadDependencies {
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    [JsonIgnore]
    public SVGAsset Icon => !string.IsNullOrEmpty(this.Description.Icon) && this.DataManager.Exists(BattleTechResourceType.SVGAsset, this.Description.Icon) ? this.DataManager.GetObjectOfType<SVGAsset>(this.Description.Icon, BattleTechResourceType.SVGAsset) : (SVGAsset)null;
    [JsonIgnore]
    private DataManager dataManager;
    [JsonIgnore]
    public DataManager DataManager {
      get {
        if (this.dataManager == null) { this.dataManager = UnityGameInstance.BattleTechGame.DataManager; }
        return this.dataManager;
      }
      set => this.dataManager = value;
    }
    public bool DependenciesLoaded(uint loadWeight) {
      if (this.dataManager == null) { return false; }
      if (string.IsNullOrEmpty(this.Description.Icon) == false) {
        if (this.dataManager.Exists(BattleTechResourceType.SVGAsset, this.Description.Icon) == false) { return false; }
      }
      return true;
    }
    public void GatherDependencies(DataManager dataManager, DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      this.dataManager = dataManager;
      if (string.IsNullOrEmpty(this.Description.Icon) == false) {
        dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, this.Description.Icon);
      }
    }
  }
  public class DropClassDef {
    public static readonly string DEFAULT_MECH_DROP_NAME = "GenericMechClass";
    public static readonly string DEFAULT_VEHICLE_DROP_NAME = "GenericVehicleClass";
    public static readonly string DEFAULT_MECH_DROP_UINAME = "__/DROP.MECH/__";
    public static readonly string DEFAULT_VEHICLE_DROP_UINAME = "__/DROP.VEHICLE/__";
    public static readonly string CAN_DROP_TITLE = "__/DROP.CAN_DROP/__";
    public static readonly string DEFAULT_MECH_UNIT_TAG = "unit_drop_type_generic_mech";
    public static readonly string DEFAULT_VEHICLE_UNIT_TAG = "unit_drop_type_generic_vehicle";
    public static readonly string DEFAULT_MECH_DROP_TAG = "can_drop_generic_mech";
    public static readonly string DEFAULT_VEHICLE_DROP_TAG = "can_drop_generic_vehicle";
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    [JsonIgnore]
    public TagSet unitTags { get; set; } = new TagSet();
    public List<string> UnitTags { set { unitTags = new TagSet(value); } }
    [JsonIgnore]
    public TagSet slotTags { get; set; } = new TagSet();
    public List<string> SlotTags { set { slotTags = new TagSet(value); } }
    public bool isMyUnitClass(TagSet tags) { return tags.ContainsAll(this.unitTags); }
    public bool isMySlotClass(TagSet tags) { return tags.ContainsAll(this.slotTags); }
  }
}