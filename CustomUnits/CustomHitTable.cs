using BattleTech;
using CustAmmoCategories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustomUnits {
  public class CustomHitTableDef {
    [JsonIgnore]
    public bool native { get; set; } = true;
    public static CustomHitTableDef empty { get; set; } = new CustomHitTableDef();
    private static Dictionary<string, CustomHitTableDef> tables = new Dictionary<string, CustomHitTableDef>();
    public static void Register(string filepath) {
      try {
        CustomHitTableDef def = JsonConvert.DeserializeObject<CustomHitTableDef>(File.ReadAllText(filepath));
        def.native = false;
        tables[def.Id] = def;
      } catch(Exception e) {
        Log.TWL(0,filepath);
        Log.TWL(0,e.ToString(),true);
      }
    }
    public static CustomHitTableDef Search(string id) {
      if (string.IsNullOrEmpty(id)) { return empty; }
      if(tables.TryGetValue(id, out var result)) { return result; }
      return empty;
    }
    public string Id { get; set; } = string.Empty;
    public Dictionary<AttackDirection, Dictionary<string, int>> hitTable { get; set; } = new Dictionary<AttackDirection, Dictionary<string, int>>();
    [JsonIgnore]
    private Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> f_HitTable = null;
    [JsonIgnore]
    public Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> HitTable {
      get {
        if(f_HitTable == null) {
          f_HitTable = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
          foreach(var dir in hitTable) {
            var hitTbl = new Dictionary<ArmorLocation, int>();
            foreach(var hit in dir.Value) {
              if(Enum.TryParse<ArmorLocation>(hit.Key, out var loc)) {
                hitTbl.Add(loc, hit.Value);
              }else if(Enum.TryParse<VehicleChassisLocations>(hit.Key, out var vloc)) {
                hitTbl.Add(vloc.toFakeArmor(), hit.Value);
              }
            }
            f_HitTable.Add(dir.Key, hitTbl);
          }
        }
        return f_HitTable;
      }
    }
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