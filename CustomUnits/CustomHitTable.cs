using BattleTech;
using CustAmmoCategories;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomUnits {
  [HarmonyPatch(typeof(CombatGameConstants))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnDataLoaded")]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
  public static class CombatGameConstants_OnDataLoaded {
    public static void Postfix(CombatGameConstants __instance, string id, string json) {
      try {
        CustomStructureDef.AddDefaults(__instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class CustomHitTableDef {
    private static HashSet<string> tables = new HashSet<string>();
    public static void Register(string filename) {
      tables.Add(filename);
    }
    public static void Register(CustomHitTableDef def) {
      var parent = CustomStructureDef.Search(def.ParentStructureId);
      if (parent.is_empty == false) {
        if (def.Id == "default") { parent.defaultHitTable = def; } else {
          parent.tables[def.Id] = def;
        }
      }
    }
    public static void Resolve() {
      foreach(var filename in tables) {
        try {
          CustomHitTableDef table = JsonConvert.DeserializeObject<CustomHitTableDef>(File.ReadAllText(filename));
          var parent = CustomStructureDef.Search(table.ParentStructureId);
          if (parent.is_empty == false) {
            if (table.Id == "default") { parent.defaultHitTable = table; } else {
              parent.tables[table.Id] = table;
            }
          }
        } catch(Exception e) {
          Log.TWL(0,filename);
          Log.WL(0, e.ToString(), true);
        }
      }
    }
    //private static Dictionary<string, CustomHitTableDef> tables = new Dictionary<string, CustomHitTableDef>();
    public string ParentStructureId { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public Dictionary<AttackDirection, Dictionary<string, int>> hitTable { get; set; } = new Dictionary<AttackDirection, Dictionary<string, int>>();
    [JsonIgnore]
    private Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> f_HitTable = null;
    [JsonIgnore]
    public Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> HitTable {
      get {
        if (f_HitTable == null) {
          f_HitTable = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
          foreach (var dir in hitTable) {
            var hitTbl = new Dictionary<ArmorLocation, int>();
            foreach (var hit in dir.Value) {
              if (Enum.TryParse<ArmorLocation>(hit.Key, out var loc)) {
                hitTbl.Add(loc, hit.Value);
              } else if (Enum.TryParse<VehicleChassisLocations>(hit.Key, out var vloc)) {
                hitTbl.Add(vloc.toFakeArmor(), hit.Value);
              }
            }
            f_HitTable.Add(dir.Key, hitTbl);
          }
        }
        return f_HitTable;
      }
      set {
        f_HitTable = value;
      }
    }
    public static CustomHitTableDef DefaultMechHitTable(CombatGameConstants constants) {
      CustomHitTableDef result = new CustomHitTableDef();
      result.f_HitTable = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
      result.f_HitTable[AttackDirection.FromArtillery] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromArtillery) {
        result.f_HitTable[AttackDirection.FromArtillery].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromFront] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromFront) {
        result.f_HitTable[AttackDirection.FromFront].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromLeft] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromLeft) {
        result.f_HitTable[AttackDirection.FromLeft].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromRight] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromRight) {
        result.f_HitTable[AttackDirection.FromRight].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromBack] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromBack) {
        result.f_HitTable[AttackDirection.FromBack].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromTop] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromTop) {
        result.f_HitTable[AttackDirection.FromTop].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.ToProne] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationProne) {
        result.f_HitTable[AttackDirection.ToProne].Add(hit.Key, hit.Value);
      }
      result.f_HitTable[AttackDirection.FromArtillery] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitMechLocationFromArtillery) {
        result.f_HitTable[AttackDirection.FromArtillery].Add(hit.Key, hit.Value);
      }
      return result;
    }
    public static CustomHitTableDef DefaultVehcileHitTable(CombatGameConstants constants) {
      CustomHitTableDef result = new CustomHitTableDef();
      result.f_HitTable = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
      result.f_HitTable[AttackDirection.FromArtillery] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromArtillery) {
        result.f_HitTable[AttackDirection.FromArtillery].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromFront] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromFront) {
        result.f_HitTable[AttackDirection.FromFront].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromLeft] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromLeft) {
        result.f_HitTable[AttackDirection.FromLeft].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromRight] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromRight) {
        result.f_HitTable[AttackDirection.FromRight].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromBack] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromBack) {
        result.f_HitTable[AttackDirection.FromBack].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromTop] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromTop) {
        result.f_HitTable[AttackDirection.FromTop].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.ToProne] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromArtillery) {
        result.f_HitTable[AttackDirection.ToProne].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      result.f_HitTable[AttackDirection.FromArtillery] = new Dictionary<ArmorLocation, int>();
      foreach (var hit in constants.HitTables.HitVehicleLocationFromArtillery) {
        result.f_HitTable[AttackDirection.FromArtillery].Add(hit.Key.toFakeArmor(), hit.Value);
      }
      return result;
    }
  }
  public class CustomStructureDef {
    [JsonIgnore]
    public bool is_empty { get; set; } = true;
    public static CustomStructureDef empty { get; set; } = new CustomStructureDef();
    private static Dictionary<string, CustomStructureDef> rules = new Dictionary<string, CustomStructureDef>();
    [JsonIgnore]
    public Dictionary<string, CustomHitTableDef> tables { get; set; } = new Dictionary<string, CustomHitTableDef>();
    public static readonly string DEFAULT_MECH_STRUCTURE_RULES = "mech";
    public static readonly string DEFAULT_VEHICLE_STRUCTURE_RULES = "vehicle";
    public static void AddDefaults(CombatGameConstants constants) {
      Log.TWL(0,$"CustomStructureDef.AddDefaults");
      if (rules.TryGetValue(DEFAULT_MECH_STRUCTURE_RULES, out var default_mech_rules) == false) {
        default_mech_rules = new CustomStructureDef();
        default_mech_rules.is_empty = false;
        default_mech_rules.f_AdjacentLocations = new Dictionary<ArmorLocation, ArmorLocation>() {
          { ArmorLocation.Head, ArmorLocation.LeftTorso | ArmorLocation.CenterTorso | ArmorLocation.RightTorso | ArmorLocation.LeftTorsoRear | ArmorLocation.CenterTorsoRear | ArmorLocation.RightTorsoRear  },
          { ArmorLocation.LeftArm, ArmorLocation.LeftTorso | ArmorLocation.LeftTorsoRear},
          { ArmorLocation.LeftTorso, ArmorLocation.LeftArm | ArmorLocation.CenterTorso | ArmorLocation.LeftLeg | ArmorLocation.LeftTorsoRear},
          { ArmorLocation.CenterTorso, ArmorLocation.Head | ArmorLocation.LeftTorso | ArmorLocation.RightTorso | ArmorLocation.CenterTorsoRear},
          { ArmorLocation.RightTorso, ArmorLocation.CenterTorso | ArmorLocation.RightArm | ArmorLocation.RightLeg | ArmorLocation.RightTorsoRear},
          { ArmorLocation.RightArm, ArmorLocation.RightTorso | ArmorLocation.RightTorsoRear},
          { ArmorLocation.LeftLeg, ArmorLocation.LeftTorso | ArmorLocation.LeftTorsoRear},
          { ArmorLocation.RightLeg, ArmorLocation.RightTorso | ArmorLocation.RightTorsoRear},
          { ArmorLocation.LeftTorsoRear, ArmorLocation.LeftArm | ArmorLocation.LeftTorso | ArmorLocation.LeftLeg | ArmorLocation.CenterTorsoRear},
          { ArmorLocation.CenterTorsoRear, ArmorLocation.Head | ArmorLocation.CenterTorso | ArmorLocation.LeftTorsoRear | ArmorLocation.RightTorsoRear},
          { ArmorLocation.RightTorsoRear, ArmorLocation.RightTorso | ArmorLocation.RightArm | ArmorLocation.RightLeg | ArmorLocation.CenterTorsoRear}
        };
        default_mech_rules.f_ClusterSpecialLocation = ArmorLocation.Head;
        default_mech_rules.defaultHitTable = CustomHitTableDef.DefaultMechHitTable(constants);
        default_mech_rules.defaultHitTable.Id = "default";
        default_mech_rules.defaultHitTable.ParentStructureId = DEFAULT_MECH_STRUCTURE_RULES;
        default_mech_rules.tables[default_mech_rules.defaultHitTable.Id] = default_mech_rules.defaultHitTable;
        rules.Add(DEFAULT_MECH_STRUCTURE_RULES, default_mech_rules);
      }
      if(rules.TryGetValue(DEFAULT_VEHICLE_STRUCTURE_RULES, out var default_vehicle_rules) == false) {
        default_vehicle_rules = new CustomStructureDef();
        default_vehicle_rules.is_empty = false;
        default_vehicle_rules.f_AdjacentLocations = new Dictionary<ArmorLocation, ArmorLocation>() {
          { VehicleChassisLocations.Turret.toFakeArmor(),  VehicleChassisLocations.Front.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor() },
          { VehicleChassisLocations.Front.toFakeArmor(),  VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor() },
          { VehicleChassisLocations.Left.toFakeArmor(),  VehicleChassisLocations.Front.toFakeArmor() | VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor() },
          { VehicleChassisLocations.Right.toFakeArmor(),  VehicleChassisLocations.Front.toFakeArmor() | VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor() },
          { VehicleChassisLocations.Rear.toFakeArmor(),  VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor() },
        };
        default_vehicle_rules.f_ClusterSpecialLocation = ArmorLocation.None;
        default_vehicle_rules.defaultHitTable = CustomHitTableDef.DefaultVehcileHitTable(constants);
        default_vehicle_rules.defaultHitTable.Id = "default";
        default_vehicle_rules.defaultHitTable.ParentStructureId = DEFAULT_VEHICLE_STRUCTURE_RULES;
        default_vehicle_rules.tables[default_vehicle_rules.defaultHitTable.Id] = default_vehicle_rules.defaultHitTable;
        rules.Add(DEFAULT_VEHICLE_STRUCTURE_RULES, default_vehicle_rules);
      }
    }
    public CustomHitTableDef defaultHitTable { get; set; } = new CustomHitTableDef();
    public static void Register(string filepath) {
      try {
        CustomStructureDef def = JsonConvert.DeserializeObject<CustomStructureDef>(File.ReadAllText(filepath));
        def.is_empty = false;
        rules[def.Id] = def;
      } catch(Exception e) {
        Log.TWL(0,filepath);
        Log.TWL(0,e.ToString(),true);
      }
    }
    public static CustomStructureDef Search(string id) {
      if (string.IsNullOrEmpty(id)) { return empty; }
      if(rules.TryGetValue(id, out var result)) { return result; }
      return empty;
    }
    public string Id { get; set; } = string.Empty;
    public Dictionary<string, HashSet<string>> adjacentLocations { get; set; } = new Dictionary<string, HashSet<string>>();
    [JsonIgnore]
    private Dictionary<ArmorLocation, ArmorLocation> f_AdjacentLocations = null;
    [JsonIgnore]
    public Dictionary<ArmorLocation, ArmorLocation> AdjacentLocations {
      get {
        if (f_AdjacentLocations == null) {
          f_AdjacentLocations = new Dictionary<ArmorLocation, ArmorLocation>();
          foreach (var strloc in adjacentLocations) {
            ArmorLocation srcloc = ArmorLocation.None;
            if (Enum.TryParse<ArmorLocation>(strloc.Key, out var loc)) {
              srcloc = loc;
            } else if (Enum.TryParse<VehicleChassisLocations>(strloc.Key, out var vloc)) {
              srcloc = vloc.toFakeArmor();
            } else {
              continue;
            }
            ArmorLocation locations = ArmorLocation.None;
            foreach (var stradjloc in strloc.Value) {
              ArmorLocation adjloc = ArmorLocation.None;
              if (Enum.TryParse<ArmorLocation>(stradjloc, out var adj_loc)) {
                adjloc = adj_loc;
              } else if (Enum.TryParse<VehicleChassisLocations>(stradjloc, out var adj_vloc)) {
                adjloc = adj_vloc.toFakeArmor();
              } else {
                continue;
              }
              locations |= adjloc;
            }
            f_AdjacentLocations.Add(srcloc, locations);
          }
        }
        return f_AdjacentLocations;
      }
    }
    public string clusterSpecialLocation { get; set; } = string.Empty;
    [JsonIgnore]
    private ArmorLocation? f_ClusterSpecialLocation { get; set; } = new ArmorLocation?();
    [JsonIgnore]
    public ArmorLocation ClusterSpecialLocation {
      get {
        if(f_ClusterSpecialLocation.HasValue == false) {
          ArmorLocation srcloc = ArmorLocation.None;
          if (Enum.TryParse<ArmorLocation>(clusterSpecialLocation, out var loc)) {
            srcloc = loc;
          } else if (Enum.TryParse<VehicleChassisLocations>(clusterSpecialLocation, out var vloc)) {
            srcloc = vloc.toFakeArmor();
          }
          f_ClusterSpecialLocation = srcloc;
        }
        return f_ClusterSpecialLocation.Value;
      }
    }
  }
}