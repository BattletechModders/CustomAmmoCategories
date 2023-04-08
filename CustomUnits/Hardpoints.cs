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
using CustAmmoCategories;
using HarmonyLib;
using Localize;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BattleTech.Data.DataManager;

namespace CustomUnits {
  [MessagePackObject]
  public class CustomHardpointDef {
    [Key(0)]
    public CustomVector offset { get; set; }
    [Key(1)]
    public CustomVector scale { get; set; }
    [Key(2)]
    public CustomVector rotate { get; set; }
    [Key(3)]
    public float prefireAnimationLength { get; set; }
    [Key(4)]
    public float fireAnimationLength { get; set; }
    [Key(5)]
    public string name { get; set; }
    [Key(6)]
    public string prefab { get; set; }
    [Key(7)]
    public string shaderSrc { get; set; }
    [Key(8)]
    public List<string> keepShaderIn { get; set; }
    [Key(9)]
    public string positionSrc { get; set; }
    [Key(10)]
    public string paintSchemePlaceholder { get; set; }
    [Key(11)]
    public List<string> emitters { get; set; }
    [Key(12)]
    public HardpointAttachType attachType { get; set; }
    [Key(13)]
    public Dictionary<string, float> animators { get; set; }
    [Key(14)]
    public List<string> fireEmitterAnimation { get; set; }
    [Key(15)]
    public string preFireAnimation { get; set; }
    [Key(16)]
    public string attachOverride { get; set; }
    public CustomHardpointDef() {
      emitters = new List<string>();
      animators = new Dictionary<string, float>();
      keepShaderIn = new List<string>();
      offset = new CustomVector(false);
      scale = new CustomVector(true);
      rotate = new CustomVector(false);
      shaderSrc = string.Empty;
      positionSrc = string.Empty;
      prefireAnimationLength = 1f;
      fireAnimationLength = 1.5f;
      attachType = HardpointAttachType.None;
      attachOverride = string.Empty;
      name = string.Empty;
      fireEmitterAnimation = new List<string>();
      paintSchemePlaceholder = "camoholder";
    }
  };
  public class CustomHardpointRepresentation : MonoBehaviour {
    public CustomHardpointDef def { get; protected set; }
    public WeaponRepresentation weaponRep { get; protected set; }
    public void Init(WeaponRepresentation weaponRep, CustomHardpointDef def) {
      this.def = def;
      this.weaponRep = weaponRep;
    }
  }
  [MessagePackObject]
  public class HadrpointAlias {
    [Key(0)]
    public string name { get; set; } = string.Empty;
    [Key(1)]
    public string prefab { get; set; } = string.Empty;
    [Key(2)]
    public string location { get; set; } = string.Empty;
    public HadrpointAlias() { }
  }
  [MessagePackObject]
  public class HardpointPrefabCandidate {
    [Key(0)]
    public string PrefabIdentifier { get; set; } = string.Empty;
    [Key(1)]
    public string PrefabName { get; set; } = string.Empty;
    [Key(2)]
    public float weight { get; set; } = 0f;
    public HardpointPrefabCandidate() { }
    public HardpointPrefabCandidate(string id, string name, float w) {
      PrefabIdentifier = id; PrefabName = name; this.weight = w;
    }
  }
  [MessagePackObject]
  public class HardpointsGroup {
    [Key(0)]
    public int index { get; set; } = 0;
    [Key(1)]
    public Dictionary<string, HardpointPrefabCandidate> GroupPrefabs { get; set; } = new Dictionary<string, HardpointPrefabCandidate>();
    public HardpointsGroup() { }
    public HardpointsGroup(int i) { this.index = i; }
  }
  public class HardpointCalculator {
    public class Element {
      public BaseComponentRef componentRef;
      public ChassisLocations location;
    }
    public class Weight {
      public Element component;
      public HardpointsGroup group;
      public string PrefabName;
      public float weight;
    }
    public static string FakeWeaponPrefab = "fake_weapon_prefab";
    public static string FakeComponentPrefab = "fake_component_prefab";
    //public HardpointDataDef HardpointData { get; set; }
    private Dictionary<BaseComponentRef, string> CalculationResult = new Dictionary<BaseComponentRef, string>();
    public HashSet<string> usedPrefabs { get; private set; }
    private Dictionary<string, HashSet<HardpointsGroup>> locationsGroups { get; set; }
    public float CalculateWeight(string prefabId, HardpointsGroup curGroup, HashSet<HardpointsGroup> restGroups, out string prefabName, out string debug) {
      float result = 0f;
      debug = string.Empty;
      foreach (HardpointsGroup restGrp in restGroups) {
        if (restGrp.GroupPrefabs.TryGetValue(prefabId.ToLower(), out HardpointPrefabCandidate restCandidate)) {
          result += restCandidate.weight;
          debug += " group: " + restGrp.index + " " + restCandidate.PrefabIdentifier + " weight:" + restCandidate.weight;
        }
      }
      prefabName = curGroup.GroupPrefabs.First().Value.PrefabName;
      if (curGroup.GroupPrefabs.TryGetValue(prefabId.ToLower(), out HardpointPrefabCandidate candidate)) {
        result = candidate.weight / (result + 1f);
        debug = "main:" + candidate.PrefabIdentifier + " weight:" + candidate.weight + debug;
        prefabName = candidate.PrefabName;
      } else {
        debug = "main:" + prefabId + " weight:" + 0f + debug;
        result = 0f;
      }
      return result;
    }
    public string GetComponentPrefabName(BaseComponentRef componentRef) {
      if (componentRef.Def.PrefabIdentifier.StartsWith("chrPrfWeap", true, CultureInfo.InvariantCulture) || componentRef.Def.PrefabIdentifier.StartsWith("chrPrfComp", true, CultureInfo.InvariantCulture)) { return componentRef.Def.PrefabIdentifier; };
      Log.TWL(0, "HardpointCalculator.GetComponentPrefabName " + componentRef.ComponentDefID);
      string prefabName = string.Empty;
      if (CalculationResult.TryGetValue(componentRef, out prefabName)) {
        Log.WL(1, "found: " + prefabName);
      }
      if (string.IsNullOrEmpty(prefabName) == false) {
        prefabName = CustomHardPointsHelper.Alias(prefabName);
      }
      if (componentRef.ComponentDefType == ComponentType.Weapon) {
        if (string.IsNullOrEmpty(prefabName)) { prefabName = HardpointCalculator.FakeWeaponPrefab; }
      }
      Log.WL(1, "result:" + prefabName);
      return prefabName;
    }
    public string GetComponentPrefabNameNoAlias(BaseComponentRef componentRef) {
      if (componentRef.Def.PrefabIdentifier.StartsWith("chrPrfWeap", true, CultureInfo.InvariantCulture) || componentRef.Def.PrefabIdentifier.StartsWith("chrPrfComp", true, CultureInfo.InvariantCulture)) { return componentRef.Def.PrefabIdentifier; };
      Log.TWL(0, "HardpointCalculator.GetComponentPrefabName " + componentRef.ComponentDefID);
      string prefabName = string.Empty;
      if (CalculationResult.TryGetValue(componentRef, out prefabName)) {
        Log.WL(1, "found: " + prefabName);
      }
      Log.WL(1, "result:" + prefabName);
      return prefabName;
    }
    public HashSet<HardpointsGroup> collectGroupByLocation(string location) {
      HashSet<HardpointsGroup> result = new HashSet<HardpointsGroup>();
      if (locationsGroups.TryGetValue(location, out HashSet<HardpointsGroup> groups)) {
        foreach (HardpointsGroup group in groups) { result.Add(group); }
      }
      return result;
    }
    public bool isHasUsedPrefabs(string[] group, HashSet<string> usedPrefabs) {
      foreach (string prefab in group) { if (usedPrefabs.Contains(prefab)) { return true; } }
      return false;
    }
    public void RecalcGroups(HardpointDataDef HardpointData, HashSet<string> usedPrefabs) {
      Log.TW(0, "HardpointCalculator.RecalcGroups " + HardpointData.ID);
      foreach (string prefab in usedPrefabs) { Log.W(1, prefab); }
      Log.WL(0, "");
      this.locationsGroups = new Dictionary<string, HashSet<HardpointsGroup>>();
      HashSet<string> conflictPrefabs = new HashSet<string>();
      foreach (string prefab in usedPrefabs) { conflictPrefabs.Add(prefab); }
      foreach (var locationGroups in HardpointData.HardpointData) {
        foreach (string[] group in locationGroups.weapons) {
          if (isHasUsedPrefabs(group, usedPrefabs)) {
            foreach (string prefab in group) {
              conflictPrefabs.Add(prefab);
            }
          }
        }
      }
      foreach (var locationGroups in HardpointData.HardpointData) {
        string location = locationGroups.location.ToLower();
        if (this.locationsGroups.TryGetValue(location, out HashSet<HardpointsGroup> groups) == false) {
          groups = new HashSet<HardpointsGroup>();
          this.locationsGroups.Add(location, groups);
        }
        for (int group_index = 0; group_index < locationGroups.weapons.Length; ++group_index) {
          HardpointsGroup group = new HardpointsGroup(group_index);
          //Log.WL(1, "Group:" + group_index);
          foreach (string prefabName in locationGroups.weapons[group_index]) {
            //Log.WL(2, "prefab:" + prefabName);
            if (conflictPrefabs.Contains(prefabName)) {
              //Log.WL(3, "conflict");
              continue;
            }
            ComponentPrefabName name = new ComponentPrefabName(prefabName);
            if (name.content.Length != 5) { Log.TWL(0, "!!!WARNING!!! hardpoint data definition " + HardpointData.ID + " contains bad weapon prefab " + prefabName); continue; }
            if (group.GroupPrefabs.TryGetValue(name.prefabIdentifier.ToLower(), out HardpointPrefabCandidate base_candidate)) {
              if (base_candidate.weight >= 1f) { Log.TWL(0, "!!!WARNING!!! hardpoint data definition " + HardpointData.ID + " contains duplicates weapon prefab identifiers " + name.prefabIdentifier.ToLower() + " in one group " + prefabName); continue; }
              base_candidate.weight = 1f;
              base_candidate.PrefabName = prefabName;
            } else {
              group.GroupPrefabs.Add(name.prefabIdentifier.ToLower(), new HardpointPrefabCandidate(name.prefabIdentifier, prefabName, 1f));
            }
            if (Core.Settings.weaponPrefabMappings == null) { continue; }
            //Log.WL(2, "prefabIdentifier:" + name.prefabIdentifier.ToLower());
            if (Core.Settings.weaponPrefabMappings.TryGetValue(name.prefabIdentifier.ToLower(), out Dictionary<string, float> candidates)) {
              //Log.WL(3, "candidates:"+ candidates.Count);
              foreach (var cnd in candidates) {
                float weight = cnd.Value;
                string prefabIdentifier = cnd.Key.ToLower();
                if (group.GroupPrefabs.TryGetValue(prefabIdentifier, out HardpointPrefabCandidate candidate)) {
                  if (weight > candidate.weight) {
                    candidate.PrefabName = prefabName; candidate.weight = weight;
                    //Log.WL(4, "update weight:" + prefabName + " " + weight);
                  }
                } else {
                  //Log.WL(4, "add:" + prefabIdentifier+" "+ weight);
                  group.GroupPrefabs.Add(prefabIdentifier, new HardpointPrefabCandidate(prefabIdentifier, prefabName, weight));
                }
              }
            }
          }
          if (group.GroupPrefabs.Count > 0) { groups.Add(group); }
        }
      }
      Log.WL(1, "result:");
      foreach (var hglocs in this.locationsGroups) {
        Log.WL(2, "location:" + hglocs.Key);
        foreach (var hg in hglocs.Value) {
          Log.WL(3, "group");
          Dictionary<string, Dictionary<string, float>> weights = new Dictionary<string, Dictionary<string, float>>();
          foreach (var pc in hg.GroupPrefabs) {
            if (weights.TryGetValue(pc.Value.PrefabName, out Dictionary<string, float> weight) == false) {
              weight = new Dictionary<string, float>();
              weights.Add(pc.Value.PrefabName, weight);
            }
            weight.Add(pc.Key, pc.Value.weight);
            //Log.WL(4, pc.Key + ": " + pc.Value.PrefabName + " weight:" + pc.Value.weight);
          }
          foreach (var prefab in weights) {
            Log.W(4, prefab.Key);
            foreach (var prefabId in prefab.Value) {
              Log.W(1, prefabId.Key + ":" + prefabId.Value);
            }
            Log.WL(0, "");
          }
        }
      }

    }
    public void Init(List<HardpointCalculator.Element> compInfos, HardpointDataDef HardpointData) {
      Log.TWL(0, "HardpointCalculator.Init " + HardpointData.ID);
      this.usedPrefabs = new HashSet<string>();
      Dictionary<string, List<HardpointCalculator.Element>> localtions = new Dictionary<string, List<HardpointCalculator.Element>>();
      foreach (HardpointCalculator.Element compInfo in compInfos) {
        string locationName = (HardpointData.isFakeHardpoint() || HardpointData.CustomHardpoints().IsVehicleStyleLocations) ? compInfo.location.toVehicleLocation().ToString().ToLower() : compInfo.location.ToString().ToLower();
        if (localtions.TryGetValue(locationName, out List<HardpointCalculator.Element> locInfos) == false) {
          locInfos = new List<HardpointCalculator.Element>();
          localtions.Add(locationName, locInfos);
        }
        locInfos.Add(compInfo);
      }
      foreach (var locComponents in localtions) {
        HashSet<HardpointsGroup> effectiveGroups = null;
        List<HardpointCalculator.Element> effectiveComponents = new List<HardpointCalculator.Element>();
        foreach (HardpointCalculator.Element component in locComponents.Value) {
          if (string.IsNullOrEmpty(component.componentRef.Def.PrefabIdentifier)) { continue; }
          effectiveComponents.Add(component);
        }
        Log.WL(1, "location: " + locComponents.Key);
        do {
          this.RecalcGroups(HardpointData, this.usedPrefabs);
          effectiveGroups = this.collectGroupByLocation(locComponents.Key);
          if (effectiveGroups.Count == 0) { Log.WL(1, "no effective groups"); break; }
          if (effectiveComponents.Count == 0) { Log.WL(1, "no effective components"); break; }
          Dictionary<Element, Dictionary<HardpointsGroup, Weight>> componentsWeights = new Dictionary<Element, Dictionary<HardpointsGroup, Weight>>();
          foreach (Element component in effectiveComponents) {
            if (componentsWeights.TryGetValue(component, out Dictionary<HardpointsGroup, Weight> grpWeights) == false) {
              grpWeights = new Dictionary<HardpointsGroup, Weight>();
              componentsWeights.Add(component, grpWeights);
            }
            HashSet<HardpointsGroup> tempGroups = new HashSet<HardpointsGroup>();
            foreach (HardpointsGroup tmpgrp in effectiveGroups) { tempGroups.Add(tmpgrp); }
            Log.WL(2, "effectiveGroups: " + effectiveGroups.Count);
            foreach (HardpointsGroup curGroup in effectiveGroups) {
              tempGroups.Remove(curGroup);
              float cur_weight = CalculateWeight(component.componentRef.Def.PrefabIdentifier, curGroup, tempGroups, out string curPrefabName, out string debug);
              grpWeights.Add(curGroup, new Weight() { PrefabName = curPrefabName, weight = cur_weight, component = component, group = curGroup });
              tempGroups.Add(curGroup);
              Log.WL(2, "group:" + curGroup.index + " component:" + component.componentRef.ComponentDefID + " prefabId:" + component.componentRef.Def.PrefabIdentifier + " prefabName:" + curPrefabName + " weight:" + cur_weight + " dbg:" + debug);
            }
          }
          Weight win_weight = componentsWeights.First().Value.First().Value;
          foreach (var grpWeight in componentsWeights) {
            foreach (var compWeight in grpWeight.Value) {
              float weightDelta = win_weight.weight - compWeight.Value.weight;
              if (weightDelta < (0f - Core.Epsilon)) { win_weight = compWeight.Value; } else if (weightDelta < Core.Epsilon) {
                if (compWeight.Value.component.componentRef.Def.Tonnage > win_weight.component.componentRef.Def.Tonnage) {
                  win_weight = compWeight.Value;
                } else if (compWeight.Value.component.componentRef.Def.Tonnage == win_weight.component.componentRef.Def.Tonnage) {
                  if (compWeight.Value.component.componentRef.Def.InventorySize > win_weight.component.componentRef.Def.InventorySize) {
                    win_weight = compWeight.Value;
                  } else if (compWeight.Value.component.componentRef.Def.InventorySize == win_weight.component.componentRef.Def.InventorySize) {
                    if (compWeight.Value.group.index < win_weight.group.index) {
                      win_weight = compWeight.Value;
                    }
                  }
                }
              }
            }
          }
          if (win_weight.weight < Core.Epsilon) {
            Log.WL(3, "no winner. group:" + win_weight.group.index + " component:" + win_weight.component.componentRef.ComponentDefID + " prefabId:" + win_weight.component.componentRef.Def.PrefabIdentifier + " prefabName:" + " no visuals" + " weight:" + win_weight.weight);
            effectiveComponents.Remove(win_weight.component);
          } else {
            Log.WL(3, "winner. group:" + win_weight.group.index + " component:" + win_weight.component.componentRef.ComponentDefID + " prefabId:" + win_weight.component.componentRef.Def.PrefabIdentifier + " prefabName:" + win_weight.PrefabName + " weight:" + win_weight.weight);
            CalculationResult.Add(win_weight.component.componentRef, win_weight.PrefabName);
            effectiveComponents.Remove(win_weight.component);
            this.usedPrefabs.Add(win_weight.PrefabName);
          }
        } while (effectiveComponents.Count != 0);
      }
    }
  }
  public class ComponentPrefabName {
    public string[] content { get; set; }
    public string prefix { get { return content[0]; } }
    public string unitPrefab { get { return content[1]; } }
    public string location { get { return content[2]; } }
    public string prefabIdentifier { get { return content[3]; } }
    public string index { get { return content[4]; } }
    public ComponentPrefabName(string prefabName) {
      this.content = prefabName.Split('_');
    }
  }
  [MessagePackObject]
  public class CustomHardpointsDef {
    [Key(0)]
    public bool IsVehicleStyleLocations { get; set; }
    [Key(1)]
    public List<CustomHardpointDef> prefabs { get; set; }
    [Key(2)]
    public Dictionary<string, HadrpointAlias> aliases { get; set; }
    [Key(3), JsonIgnore]
    protected Dictionary<string, HashSet<HardpointsGroup>> f_hardpointsGroups { get; set; } = null;
    [IgnoreMember, JsonIgnore]
    public Dictionary<string, HashSet<HardpointsGroup>> hardpointsGroups {
      get {
        if (f_hardpointsGroups == null) { this.InitGroups(); }
        return f_hardpointsGroups;
      }
    }
    [Key(4), JsonIgnore]
    public string HardpointDataDefID { get; set; } = string.Empty;
    [Key(5), JsonIgnore]
    public bool is_empty { get; private set; }
    [IgnoreMember, JsonIgnore]
    public HardpointDataDef parent { get; set; } = null;
    public CustomHardpointsDef() {
      prefabs = new List<CustomHardpointDef>();
      aliases = new Dictionary<string, HadrpointAlias>();
      IsVehicleStyleLocations = false;
      is_empty = false;
    }
    public CustomHardpointsDef(bool empty) {
      prefabs = new List<CustomHardpointDef>();
      aliases = new Dictionary<string, HadrpointAlias>();
      IsVehicleStyleLocations = false;
      is_empty = empty;
    }
    public void Apply(HardpointDataDef parent) {
      this.parent = parent;
      if (this.HardpointDataDefID != parent.ID) { f_hardpointsGroups = null; }
      this.HardpointDataDefID = parent.ID;
      string id = parent.ID;
      foreach (CustomHardpointDef chd in this.prefabs) {
        if (string.IsNullOrEmpty(chd.name)) { chd.name = chd.prefab; }
        if (string.IsNullOrEmpty(chd.name)) { continue; }
        string custHardpointName = id + "." + chd.name;
        CustomHardPointsHelper.Add(custHardpointName, chd);
      }
      foreach (var alias in this.aliases) {
        alias.Value.name = alias.Key;
        CustomHardPointsHelper.Add(alias.Value.name, id + "." + alias.Value.prefab);
      }
      if (string.IsNullOrEmpty(parent.ID) == false) {
        CustomHardPointsHelper.Add(parent.ID, this);
        Log.WL(0, $"Register custom hardpoint {parent.ID} aliaces:{aliases.Count}");
      }
      Dictionary<string, int> locationMap = new Dictionary<string, int>();
      List<HardpointDataDef._WeaponHardpointData> HardpointData = new List<HardpointDataDef._WeaponHardpointData>(parent.HardpointData);
      for (int i = 0; i < parent.HardpointData.Length; ++i) {
        locationMap[HardpointData[i].location] = i;
      }
      foreach (var alias in this.aliases) {
        if (locationMap.TryGetValue(alias.Value.location, out var index) == false) {
          index = HardpointData.Count;
          HardpointDataDef._WeaponHardpointData locationData = new HardpointDataDef._WeaponHardpointData();
          locationData.location = alias.Value.location;
          locationData.mountingPoints = new string[0];
          locationData.blanks = new string[0];
          locationData.weapons = new string[0][];
          HardpointData.Add(locationData);
          locationMap[alias.Value.location] = index;
        }
        int hindex = 0;
        if (alias.Key.Contains("_blank_")) {
          HashSet<string> tmp_blank = parent.HardpointData[index].blanks.ToHashSet();
          tmp_blank.Add(alias.Key);
          parent.HardpointData[index].blanks = tmp_blank.ToArray();
          continue;
        }
        try {
          hindex = int.Parse(alias.Key.Substring(alias.Key.Length - 1)) - 1;
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
        if (hindex < 0) { continue; }
        if (hindex >= parent.HardpointData[index].weapons.Length) {
          List<string[]> tmplist = parent.HardpointData[index].weapons.ToList();
          tmplist.Add(new string[] { });
          parent.HardpointData[index].weapons = tmplist.ToArray();
        }
        HashSet<string> tmp_weapons = parent.HardpointData[index].weapons[hindex].ToHashSet();
        tmp_weapons.Add(alias.Key);
        parent.HardpointData[index].weapons[hindex] = tmp_weapons.ToArray();
      }
      this.InitGroups();
    }
    protected void InitGroups() {
      if (f_hardpointsGroups != null) { return; }
      f_hardpointsGroups = new Dictionary<string, HashSet<HardpointsGroup>>();
      if (parent == null) { return; }
      foreach (var locationData in parent.HardpointData) {
        if (f_hardpointsGroups.TryGetValue(locationData.location.ToLower(), out HashSet<HardpointsGroup> groups) == false) {
          groups = new HashSet<HardpointsGroup>();
          f_hardpointsGroups.Add(locationData.location.ToLower(), groups);
        }
        for (int grp_index = 0; grp_index < locationData.weapons.Length; ++grp_index) {
          string[] prefabGroup = locationData.weapons[grp_index];
          HardpointsGroup group = new HardpointsGroup(grp_index);
          groups.Add(group);
          foreach (string prefabName in prefabGroup) {
            ComponentPrefabName name = new ComponentPrefabName(prefabName);
            if (name.content.Length != 5) { Log.TWL(0, "!!!WARNING!!! hardpoint data definition " + this.parent.ID + " contains bad weapon prefab " + prefabName); continue; }
            if (group.GroupPrefabs.TryGetValue(name.prefabIdentifier.ToLower(), out HardpointPrefabCandidate base_candidate)) {
              if (base_candidate.weight >= 1f) { Log.TWL(0, "!!!WARNING!!! hardpoint data definition " + parent.ID + " contains duplicates weapon prefab identifiers " + name.prefabIdentifier.ToLower() + " in one group " + prefabName); continue; }
              base_candidate.weight = 1f;
              base_candidate.PrefabName = prefabName;
            } else {
              group.GroupPrefabs.Add(name.prefabIdentifier.ToLower(), new HardpointPrefabCandidate(name.prefabIdentifier, prefabName, 1f));
            }
            if (Core.Settings.weaponPrefabMappings == null) { continue; }
            if (Core.Settings.weaponPrefabMappings.TryGetValue(name.prefabIdentifier.ToLower(), out Dictionary<string, float> candidates)) {
              foreach (var cnd in candidates) {
                float weight = cnd.Value;
                string prefabIdentifier = cnd.Key.ToLower();
                if (group.GroupPrefabs.TryGetValue(prefabIdentifier, out HardpointPrefabCandidate candidate)) {
                  if (weight > candidate.weight) { candidate.PrefabName = prefabName; candidate.weight = weight; }
                } else {
                  group.GroupPrefabs.Add(prefabIdentifier, new HardpointPrefabCandidate(prefabIdentifier, prefabName, weight));
                }
              }
            }
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(HardpointDataDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class HardpointDataDef_FromJSON {
    public static bool Prefix(HardpointDataDef __instance, ref string json, ref CustomHardpointsDef __state) {
      try {
        if (string.IsNullOrEmpty(__instance.ID) == false) {
          CustomHardpointsDef extDef = CustomPrewarm.Core.getDeserializedObject(BattleTechResourceType.HardpointDataDef, __instance.ID, "CustomUnits") as CustomHardpointsDef;
          if (extDef != null) {
            __state = extDef;
            return true;
          }
          Log.TWL(0, "HardpointDataDef.FromJSON " + __instance.ID + " has no CustomHardpointsDef");
        }
        JObject definition = JObject.Parse(json);
        if (definition["CustomHardpoints"] != null) {
          __state = definition["CustomHardpoints"].ToObject<CustomHardpointsDef>();
          definition.Remove("CustomHardpoints");
        } else {
          __state = new CustomHardpointsDef(true);
        }
        json = definition.ToString();
        return true;
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\nIN:" + json + "\n");
        return true;
      }
    }
    public static void Postfix(HardpointDataDef __instance, ref CustomHardpointsDef __state) {
      try {
        if (__state == null) { return; }
        __state.Apply(__instance);
        //if (__instance.ID == "hardpointdatadef_rotunda") {
        //  Log.TWL(0, "HardpointDataDef.Loaded:"+ __instance.ID+ " HardpointData:" + (__instance.HardpointData == null?"null": __instance.HardpointData.Length.ToString()));
        //}
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      //Log.TWL(0,JsonConvert.SerializeObject(__instance, Formatting.Indented));
    }
  }
  public class HardPointAnimationController : BaseHardPointAnimationController {
    public CustomHardpointDef customHardpoint { get; set; }
    private bool PrefireCompleete { get; set; }
    public Animator animator { get; set; }
    public Weapon weapon { get; set; }
    //private float recoil_step;
    //private float recoil_value;
    private float PrefireCompleeteCounter { get; set; }
    private List<float> fireCompleeteCounters;
    private List<bool> fireCompleete;
    private float PrefireSpeed { get; set; }
    private float FireSpeed { get; set; }
    private bool isIndirect { get; set; }
    private bool HasPrefireSpeed { get; set; } = false;
    private int HashPrefireSpeed { get; set; }
    private bool HasFireSpeed { get; set; } = false;
    private int HashFireSpeed { get; set; }
    private bool HasIndirect { get; set; } = false;
    private int HashIndirect { get; set; }
    private bool HasVertical { get; set; } = false;
    private int HashVertical { get; set; }
    private bool HasToFireNormal { get; set; } = false;
    private int HashToFireNormal { get; set; }
    private bool HasPrefireAnimation { get; set; } = false;
    private int HashPrefireAnimation { get; set; }
    private bool HasInBattle { get; set; } = false;
    private int HashInBattle { get; set; }
    private bool HasStartRandomIdle { get; set; } = false;
    private int HashStartRandomIdle { get; set; }
    private bool HasIdle { get; set; } = false;
    private int HashIdle { get; set; }
    private Dictionary<string, int> HashFireAnimation { get; set; } = new Dictionary<string, int>();
    public override void PrefireAnimationSpeed(float speed) {
      if (animator == null) { return; }
      PrefireSpeed = speed;
      if (HasPrefireSpeed == false) { return; }
      animator.SetFloat(HashPrefireSpeed, speed);
    }
    public override void FireAnimationSpeed(float speed) {
      if (animator == null) { return; }
      FireSpeed = speed;
      if (HasFireSpeed == false) { return; }
      animator.SetFloat(HashFireSpeed, speed);
    }
    public HardPointAnimationController() {
      fireCompleeteCounters = new List<float>();
      fireCompleete = new List<bool>();
      FireSpeed = 1f;
      PrefireSpeed = 1f;
      //recoil_step = 0f;
      //recoil_value = 0f;
    }
    public void SetFireAnimation(string name, bool value) {
      if (animator == null) { return; }
      if (HashFireAnimation.TryGetValue(name, out int hash)) { animator.SetBool(hash, value); }
    }
    public float Indirect {
      set {
        if (animator == null) { return; }
        if (HasIndirect) { animator.SetFloat(HashIndirect, value); }
      }
    }
    public float ToFireNormal {
      set {
        if (animator == null) { return; }
        if (HasToFireNormal) { animator.SetFloat(HashToFireNormal, value); }
      }
    }
    public float Vertical {
      set {
        if (animator == null) { return; }
        if (HasVertical) { animator.SetFloat(HashVertical, value); }
      }
    }
    public bool SetPrefireAnimation {
      set {
        if (animator == null) { return; }
        if (HasPrefireAnimation) { animator.SetBool(HashPrefireAnimation, value); }
      }
    }
    public bool InBattle {
      set {
        if (animator == null) { return; }
        if (HasInBattle) { animator.SetBool(HashInBattle, value); }
      }
    }
    public bool StartRandomIdle {
      set {
        if (animator == null) { return; }
        if (HasStartRandomIdle) { animator.SetBool(HashStartRandomIdle, value); }
      }
    }
    public float RandomIdle {
      set {
        if (animator == null) { return; }
        if (HasIdle) { animator.SetFloat(HashIdle, value); }
      }
    }
    public override bool isPrefireAnimCompleete() { return PrefireCompleete; }
    private void fireCompleeteAll(bool state) {
      for (int t = 0; t < fireCompleete.Count; ++t) { fireCompleete[t] = true; }
    }
    private void fireCompleeteCountersAll() {
      for (int t = 0; t < fireCompleeteCounters.Count; ++t) { fireCompleeteCounters[t] = 0f; }
    }
    public override void FireAnimation(int index) {
      if (animator == null) { fireCompleeteAll(true); return; }
      if (customHardpoint == null) { fireCompleeteAll(true); return; }
      if (fireCompleete.Count == 0) { return; };
      int realIndex = index % fireCompleete.Count;
      Log.LogWrite("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]HardPointAnimationController.FireAnimation(" + index + "/" + realIndex + "):" + customHardpoint.prefab + "\n");
      string animName = customHardpoint.fireEmitterAnimation[realIndex];
      if (string.IsNullOrEmpty(animName) == false) {
        SetFireAnimation(animName, true);
        Log.LogWrite(1, "animName(" + realIndex + "):" + animName + " true\n");
        fireCompleete[realIndex] = false;
        fireCompleeteCounters[realIndex] = (FireSpeed > 0.01f) ? customHardpoint.fireAnimationLength / FireSpeed : 0f;
      } else {
        Log.LogWrite(1, "animName(" + realIndex + "):" + animName + " false\n");
        fireCompleete[realIndex] = true;
        fireCompleeteCounters[realIndex] = 0f;
      }
      for (int t = 0; t < customHardpoint.fireEmitterAnimation.Count; ++t) {
        if (t == realIndex) { continue; }
        string tanimName = customHardpoint.fireEmitterAnimation[t];
        if (string.IsNullOrEmpty(tanimName) == false) {
          Log.LogWrite(1, "animName(" + t + "):" + tanimName + " - false\n");
          SetFireAnimation(tanimName, false);
          fireCompleete[t] = true;
          fireCompleeteCounters[t] = 0f;
        }
      }
    }
    public override bool isFireAnimCompleete(int index) {
      if (fireCompleete.Count == 0) { return true; }
      return fireCompleete[index % fireCompleete.Count];
    }
    public override void PrefireAnimation(Vector3 target, bool indirect) {
      this.isIndirect = indirect;
      if (animator == null) { PrefireCompleete = true; return; }
      if (customHardpoint == null) { PrefireCompleete = true; return; }
      if (string.IsNullOrEmpty(customHardpoint.preFireAnimation)) { PrefireCompleete = true; return; }
      PrefireCompleeteCounter = (PrefireSpeed > 0.01f) ? customHardpoint.prefireAnimationLength / PrefireSpeed : 0f;
      PrefireCompleete = false;
      Log.WL(0,$"HardPointAnimationController.PrefireAnimation {(weapon==null?this.gameObject.name:weapon.defId)}");
      if (customHardpoint.preFireAnimation == "_new_style") {
        if (this.isIndirect) {
          this.Indirect = 1f;
          this.ToFireNormal = 0.98f;
          //animator.SetBool(customHardpoint.preFireAnimation, true);
        } else {
          this.Indirect = 0.98f;
          this.Vertical = 0.5f;
          this.ToFireNormal = 1f;
        }
      } else {
        SetPrefireAnimation = true;
      }
    }
    public override void PostfireAnimation() {
      if (animator == null) { PrefireCompleete = true; return; }
      if (customHardpoint == null) { PrefireCompleete = true; return; }
      if (string.IsNullOrEmpty(customHardpoint.preFireAnimation)) { PrefireCompleete = true; return; }
      PrefireCompleete = true;
      if (customHardpoint.preFireAnimation != "_new_style") {
        SetPrefireAnimation = false;
      } else {
        if (this.isIndirect) {
          this.Indirect = 0.98f;
          this.ToFireNormal = 0.98f;
        }
      }
      for (int t = 0; t < customHardpoint.fireEmitterAnimation.Count; ++t) {
        string animName = customHardpoint.fireEmitterAnimation[t];
        if (string.IsNullOrEmpty(animName) == false) {
          Log.LogWrite(1, "animName(" + t + "):" + animName + " - false\n");
          SetFireAnimation(animName, false);
          fireCompleete[t] = true;
          fireCompleeteCounters[t] = 0f;
        }
      }
    }
    public void Update() {
      if (PrefireCompleete == false) {
        if (PrefireCompleeteCounter > 0f) { PrefireCompleeteCounter -= Time.deltaTime; } else { PrefireCompleete = true; };
      }
      for (int t = 0; t < fireCompleeteCounters.Count; ++t) {
        if (fireCompleete[t] == false) { if (fireCompleeteCounters[t] > 0f) { fireCompleeteCounters[t] -= Time.deltaTime; } else {
            Log.LogWrite("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] animation " + t + " finished\n");
            fireCompleete[t] = true;
          }; };
      }
    }
    public void OnPrefireCompleete(int i) {
      Log.WL(0,$"HardPointAnimationController.OnPrefireCompleete {i} weapon:{(weapon == null?"null": weapon.defId)}", true);
      PrefireCompleete = true;
    }
    //public void Init(WeaponRepresentation weaponRep) {
    //  this.Init(weaponRep, CustomHardPointsHelper.Find(weapon.baseComponentRef.prefabName));
    //}
    public void Init(WeaponRepresentation weaponRep, CustomHardpointDef hardpointDef) {
      Log.WL(0, $"HardPointAnimationController.Init {weaponRep.name}");
      this.weapon = weaponRep.weapon;
      this.customHardpoint = hardpointDef;
      PrefireCompleete = false;
      FireSpeed = 1f;
      PrefireSpeed = 1f;
      customHardpoint = hardpointDef;

      animator = weaponRep.gameObject.GetComponentInChildren<Animator>();
      if (animator == null) { PrefireCompleete = true; };
      if (animator != null) {
        this.HashPrefireSpeed = Animator.StringToHash("prefire_speed");
        this.HashFireSpeed = Animator.StringToHash("fire_speed");
        this.HashIndirect = Animator.StringToHash("indirect");
        this.HashVertical = Animator.StringToHash("vertical");
        this.HashToFireNormal = Animator.StringToHash("to_fire_normal");
        this.HashInBattle = Animator.StringToHash("in_battle");
        this.HashStartRandomIdle = Animator.StringToHash("start_random_idle");
        this.HashIdle = Animator.StringToHash("idle_param");

        if (customHardpoint != null && !string.IsNullOrEmpty(customHardpoint.preFireAnimation)) {
          this.HashPrefireAnimation = Animator.StringToHash(customHardpoint.preFireAnimation);
        } else {
          this.HashPrefireAnimation = Animator.StringToHash("prefire");
        }

        Dictionary<string, int> animHashes = new Dictionary<string, int>();
        this.HashFireAnimation.Clear();
        if (customHardpoint != null) {
          foreach (var fireAnim in customHardpoint.fireEmitterAnimation) {
            if (string.IsNullOrEmpty(fireAnim)) { continue; }
            animHashes[fireAnim] = Animator.StringToHash(fireAnim);
          }
        }

        foreach (var param in animator.parameters) {
          if ((param.name == "prefire_speed") && (param.type == AnimatorControllerParameterType.Float)) { this.HasPrefireSpeed = true; }
          if ((param.name == "fire_speed") && (param.type == AnimatorControllerParameterType.Float)) { this.HasFireSpeed = true; }
          if ((param.name == "indirect") && (param.type == AnimatorControllerParameterType.Float)) { this.HasIndirect = true; }
          if ((param.name == "vertical") && (param.type == AnimatorControllerParameterType.Float)) { this.HasVertical = true; }
          if ((param.name == "to_fire_normal") && (param.type == AnimatorControllerParameterType.Float)) { this.HasToFireNormal = true; }
          if ((param.name == "in_battle") && (param.type == AnimatorControllerParameterType.Bool)) { this.HasInBattle = true; }
          if ((param.name == "start_random_idle") && (param.type == AnimatorControllerParameterType.Bool)) { this.HasStartRandomIdle = true; }
          if ((param.name == "idle_param") && (param.type == AnimatorControllerParameterType.Float)) { this.HasIdle = true; }
          if ((param.name == customHardpoint?.preFireAnimation) && (param.type == AnimatorControllerParameterType.Bool)) { this.HasPrefireAnimation = true; }
          if (animHashes.ContainsKey(param.name) && (param.type == AnimatorControllerParameterType.Bool)) {
            HashFireAnimation[param.name] = animHashes[param.name];
          }
        }
      }

      Log.LogWrite(1, "customHardpoint: " + ((customHardpoint == null) ? "null" : customHardpoint.prefab) + "\n");
      if (animator != null) {
        Log.LogWrite(1, "clips(" + animator.runtimeAnimatorController.animationClips.Length + "):\n");
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
          Log.LogWrite(2, "clip:" + clip.name + "\n");
        }
      }

      if (customHardpoint != null) {
        fireCompleete.Clear();
        fireCompleeteCounters.Clear();
        foreach (string name in customHardpoint.fireEmitterAnimation) {
          fireCompleete.Add(true);
          fireCompleeteCounters.Add(0f);
        }
      }
      //HardpointAnimatorHelper.RegisterHardpointAnimator(weapon, this);
    }
  }
  public static class CustomHardPointsHelper {
    public static readonly CustomHardpointsDef EmptyCustomHardpointsDef = new CustomHardpointsDef(true);
    private static ConcurrentDictionary<string, CustomHardpointsDef> CustomHardpointsDefs = new ConcurrentDictionary<string, CustomHardpointsDef>();
    private static ConcurrentDictionary<string, string> hardPointAliases = new ConcurrentDictionary<string, string>();
    private static ConcurrentDictionary<string, CustomHardpointDef> CustomHardpointDefs = new ConcurrentDictionary<string, CustomHardpointDef>();
    //public static HardPointAnimationController hadrpointAnimator(this WeaponRepresentation weaponRep) {
    //  HardPointAnimationController result = weaponRep.gameObject.GetComponent<HardPointAnimationController>();
    //  if (result == null) { result = weaponRep.gameObject.AddComponent<HardPointAnimationController>(); result.Init(weaponRep); };
    //  return result;
    //}
    public static void Add(string id, CustomHardpointsDef defs) {
      CustomHardpointsDefs.AddOrUpdate(id, defs, (k, v) => { return defs; });
      //if (CustomHardpointsDefs.ContainsKey(id) == false) {
      //  CustomHardpointsDefs.Add(id, defs);
      //} else {
      //  CustomHardpointsDefs[id] = defs;
      //}
    }
    public static CustomHardpointsDef CustomHardpoints(this HardpointDataDef data) {
      return CustomHardpoints(data.ID);
    }
    public static CustomHardpointsDef CustomHardpoints(string ID) {
      if (CustomHardpointsDefs.TryGetValue(ID, out CustomHardpointsDef result)) {
        return result;
      }
      return EmptyCustomHardpointsDef;
    }
    public static void Add(string name, CustomHardpointDef def) {
      CustomHardpointDefs.AddOrUpdate(name, def, (k, v) => { return def; });
      //CustomHardpointDefs.Add(name, def);
    }
    public static void Add(string name, string prefab) {
      hardPointAliases.AddOrUpdate(name, prefab, (k, v) => { return prefab; });
      //if (hardPointAliases.ContainsKey(name) == false) { hardPointAliases.Add(name, prefab); return; };
      //hardPointAliases[name] = prefab;
    }
    public static CustomHardpointDef Find(string name) {
      if (CustomHardpointDefs.ContainsKey(name) == false) { return null; };
      return CustomHardpointDefs[name];
    }
    public static string Alias(string name) {
      if (hardPointAliases.ContainsKey(name) == false) { return name; };
      return hardPointAliases[name];
    }
  }
  [HarmonyPatch(typeof(MechHardpointRules))]
  [HarmonyPatch("GetComponentPrefabName")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechHardpointRules_GetComponentPrefabName {
    /*public static bool Prefix(HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames, ref string __result) {
      Log.LogWrite(0, "MechHardpointRules.GetComponentPrefabName hardpointDataDef:" + hardpointDataDef.ID + " weapon:" + componentRef.ComponentDefID + " prefabBase:" + prefabBase + " location: " + location + "\n");
      if (componentRef.HardpointSlot < 0) { __result = ""; return false; }
      WeaponDef def = componentRef.Def as WeaponDef;
      string desiredPrefabName = "";
      string lower = componentRef.Def.PrefabIdentifier.ToLower();
      string hardpointStr = "";
      if (def == null) { __result = ""; return false; }
      WeaponCategory category = def.Category;
      WeaponSubType weaponSubType = def.WeaponSubType;
      switch (category) {
        case WeaponCategory.Ballistic:
          hardpointStr = "_bh";
          break;
        case WeaponCategory.Energy:
          hardpointStr = "_eh";
          break;
        case WeaponCategory.Missile:
          hardpointStr = "_mh";
          break;
        case WeaponCategory.AntiPersonnel:
          hardpointStr = "_ah";
          break;
        case WeaponCategory.Melee:
          __result = "chrPrfWeap_generic_melee"; return false;
      }
      desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", (object)prefabBase, (object)location, (object)lower, (object)hardpointStr);
      Log.LogWrite(1, "desiredPrefabName:"+ desiredPrefabName + "\n");
      List<string> availableNames = new List<string>();
      HardpointDataDef._WeaponHardpointData weaponHardpointData = new HardpointDataDef._WeaponHardpointData("", new string[0][], new string[0], new string[0]);
      for (int index1 = 0; index1 < hardpointDataDef.HardpointData.Length; ++index1) {
        if (hardpointDataDef.HardpointData[index1].location == location) {
          weaponHardpointData = hardpointDataDef.HardpointData[index1];
          for (int index2 = 0; index2 < weaponHardpointData.weapons.Length; ++index2) {
            List<string> stringList = new List<string>((IEnumerable<string>)weaponHardpointData.weapons[index2]);
            bool flag = true;
            for (int index3 = 0; index3 < usedPrefabNames.Count; ++index3) {
              if (stringList.Contains(usedPrefabNames[index3])) {
                flag = false;
                break;
              }
            }
            if (flag)
              availableNames.AddRange((IEnumerable<string>)stringList);
          }
          break;
        }
      }
      if (category == WeaponCategory.AntiPersonnel) {
        availableNames.RemoveAll((Predicate<string>)(x => {
          if (!x.Contains(hardpointStr) && !x.Contains("_laser_eh") && !x.Contains("_flamer_eh"))
            return !x.Contains("_mg_bh");
          return false;
        }));
      } else {
        availableNames.RemoveAll((Predicate<string>)(x => !x.Contains(hardpointStr)));
      }
      //availableNames = availableNames.ConvertAll(d => d.ToLower());
      List<string> stringList1 = new List<string>((IEnumerable<string>)availableNames);
      Log.LogWrite(1, "availableNames(" + availableNames.Count + "):\n");foreach (var name in availableNames) { Log.LogWrite(2, name + "\n"); }
      stringList1.RemoveAll((Predicate<string>)(x => !x.Contains(desiredPrefabName)));
      if (stringList1.Count < 1) {
        desiredPrefabName = desiredPrefabName.ToLower();
        Log.LogWrite(1, "Can't find. "+desiredPrefabName+". Try lower case\n");
        stringList1.AddRange((IEnumerable<string>)availableNames);
        stringList1.RemoveAll((Predicate<string>)(x => !x.Contains(desiredPrefabName)));
      }

      if (stringList1.Count < 1) {
        Log.LogWrite(1, "Can't find fallback. Try default\n");
        switch (category) {
          case WeaponCategory.Ballistic:
            stringList1 = MechHardpointRules.GetFallbackBallisticNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.Energy:
            stringList1 = MechHardpointRules.GetFallbackEnergyNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.Missile:
            stringList1 = MechHardpointRules.GetFallbackMissileNames(availableNames, lower, hardpointStr);
            break;
          case WeaponCategory.AntiPersonnel:
            stringList1 = MechHardpointRules.GetFallbackSmallNames(availableNames, weaponSubType, lower, hardpointStr);
            break;
        }
      }
      Log.LogWrite(1, "stringList1(" + stringList1.Count + "):\n"); foreach (var name in stringList1) { Log.LogWrite(2, name + "\n"); }
      int num1 = int.MaxValue;
      for (int index = 0; index < stringList1.Count; ++index) {
        int num2 = int.Parse(stringList1[index].Substring(stringList1[index].Length - 1));
        if (num2 < num1) {
          desiredPrefabName = stringList1[index];
          num1 = num2;
        }
      }
      if (num1 < int.MaxValue) {
        usedPrefabNames.Add(desiredPrefabName);
        __result = desiredPrefabName; return false;
      }
      if (availableNames.Count > 0) {
        usedPrefabNames.Add(availableNames[0]);
        __result = availableNames[0]; return false;
      }
      if (weaponHardpointData.weapons.Length != 0 && weaponHardpointData.weapons[0].Length != 0) {
        if (!usedPrefabNames.Contains(weaponHardpointData.weapons[0][0]))
          usedPrefabNames.Add(weaponHardpointData.weapons[0][0]);
        __result = weaponHardpointData.weapons[0][0]; return false;
      }
      Log.LogWrite("GetComponentPrefabName failed to find a prefab name for unit " + prefabBase + " and component " + componentRef.Def.Description.Id + ", ideal match name was: " + desiredPrefabName + ", falling back to default item\n", true);
      __result = hardpointDataDef.HardpointData[0].weapons[0][0];
      return false;
    }*/
    public static void Prefix(ref bool __runOriginal, HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames, ref string __result) {
      if ((hardpointDataDef.HardpointData.Length == 0) || (hardpointDataDef.HardpointData[0].weapons.Length == 0) || hardpointDataDef.HardpointData[0].weapons[0].Length == 0) {
        Log.TWL(0, $"MechHardpointRules.GetComponentPrefabName {hardpointDataDef.ID} have empty hardpoints?");
        CustomHardpointsDef customHardpoint = CustomHardPointsHelper.CustomHardpoints(hardpointDataDef.ID);
        if (customHardpoint != null) {
          Log.WL(1, $"custom hardpoints definition found. aliases: {customHardpoint.aliases.Count}");
        }
      }
    }
    public static void Postfix(HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames, ref string __result) {
      //Log.WL(0, "MechHardpointRules.GetComponentPrefabName "+componentRef.ComponentDefID+ " prefabBase:"+ prefabBase + " location:"+ location + " " + __result);
      if (string.IsNullOrEmpty(__result)) {
        if (componentRef.Def.ComponentType == ComponentType.Weapon) {
          __result = HardpointCalculator.FakeWeaponPrefab;
        }
        return;
      }
      __result = CustomHardPointsHelper.Alias(__result);
      //Log.WL(1, " custom hardpoint found: prefab replacing:"+__result);
    }
  }
  [HarmonyPatch(typeof(MechHardpointRules))]
  [HarmonyPatch("GetWeaponPrefabName")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyWrapSafe]
  public static class MechHardpointRules_GetWeaponPrefabName {
    public static Exception Finalizer(Exception __exception, ref string __result) {
      if (__exception != null) {
        if ((__exception.GetType() == typeof(IndexOutOfRangeException))) {          
          __result = HardpointCalculator.FakeWeaponPrefab;
          return null;
        }
      }
      return __exception;
    }
  }

  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("RequestInventoryPrefabs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class MechDef_RequestInventoryPrefabs {
    public static bool Prefix(MechDef __instance, DataManager.DependencyLoadRequest dependencyLoad, uint loadWeight, MechComponentRef[] ___inventory) {
      try {
        if (loadWeight <= 10U) { return true; }
        Log.TWL(0, "MechDef.RequestInventoryPrefabs "+ __instance.Description.Id);
        for (int index = 0; index < ___inventory.Length; ++index) {
          if (___inventory[index].Def == null) { continue; }
          if (___inventory[index].hasPrefabName == false) { continue; }
          if (string.IsNullOrEmpty(___inventory[index].prefabName)) { continue; }
          if (___inventory[index].prefabName == HardpointCalculator.FakeWeaponPrefab) { continue; }
          if (___inventory[index].prefabName == HardpointCalculator.FakeComponentPrefab) { continue; }
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(___inventory[index].prefabName);
          if (customHardpoint == null) {
            dependencyLoad.RequestResource(BattleTechResourceType.Prefab, ___inventory[index].prefabName);
            Log.WL(1, "Request " + ___inventory[index].prefabName);
          } else {
            if (string.IsNullOrEmpty(customHardpoint.prefab) == false) {
              dependencyLoad.RequestResource(BattleTechResourceType.Prefab, customHardpoint.prefab);
              Log.WL(1, "Request " + customHardpoint.prefab);
            } else {
              dependencyLoad.RequestResource(BattleTechResourceType.Prefab, ___inventory[index].prefabName);
              Log.WL(1, "Request " + ___inventory[index].prefabName);
            }
          }
        }
        if (__instance.meleeWeaponRef.Def != null) {
          if (__instance.meleeWeaponRef.hasPrefabName) {
            if (string.IsNullOrEmpty(__instance.meleeWeaponRef.prefabName) == false) {
              CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(__instance.meleeWeaponRef.prefabName);
              if (customHardpoint == null) {
                dependencyLoad.RequestResource(BattleTechResourceType.Prefab, __instance.meleeWeaponRef.prefabName);
                Log.WL(1, "Request " + __instance.meleeWeaponRef.prefabName);
              } else {
                if (string.IsNullOrEmpty(customHardpoint.prefab) == false) {
                  dependencyLoad.RequestResource(BattleTechResourceType.Prefab, customHardpoint.prefab);
                  Log.WL(1, "Request " + customHardpoint.prefab);
                } else {
                  dependencyLoad.RequestResource(BattleTechResourceType.Prefab, __instance.meleeWeaponRef.prefabName);
                  Log.WL(1, "Request " + __instance.meleeWeaponRef.prefabName);
                }
              }
            }
          }
        }
        if (__instance.dfaWeaponRef.Def != null) {
          if (__instance.meleeWeaponRef.hasPrefabName) {
            if (string.IsNullOrEmpty(__instance.meleeWeaponRef.prefabName) == false) {
              CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(__instance.dfaWeaponRef.prefabName);
              if (customHardpoint == null) {
                dependencyLoad.RequestResource(BattleTechResourceType.Prefab, __instance.dfaWeaponRef.prefabName);
                Log.WL(1, "Request " + __instance.dfaWeaponRef.prefabName);
              } else {
                if (string.IsNullOrEmpty(customHardpoint.prefab) == false) {
                  dependencyLoad.RequestResource(BattleTechResourceType.Prefab, customHardpoint.prefab);
                  Log.WL(1, "Request " + customHardpoint.prefab);
                } else {
                  dependencyLoad.RequestResource(BattleTechResourceType.Prefab, __instance.dfaWeaponRef.prefabName);
                  Log.WL(1, "Request " + __instance.dfaWeaponRef.prefabName);
                }
              }
            }
          }
        }
        return false;
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(MechDef __instance, DataManager.DependencyLoadRequest dependencyLoad, uint loadWeight, MechComponentRef[] ___inventory) {
      if (loadWeight <= 10U) { return; }
      Log.LogWrite("MechDef.RequestInventoryPrefabs defId:" + __instance.Description.Id + " "+loadWeight+"\n");
      for (int index = 0; index < ___inventory.Length; ++index) {
        if (___inventory[index].Def != null) {
          Log.LogWrite(" prefab:" + ___inventory[index].ComponentDefID + ":" + ___inventory[index].prefabName + "\n");
          if (___inventory[index].hasPrefabName == false) { continue; }
          if (string.IsNullOrEmpty(___inventory[index].prefabName)) { continue; }
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(___inventory[index].prefabName);
          Log.LogWrite(" prefab:" + ___inventory[index].prefabName + "\n");
          if (customHardpoint == null) { Log.LogWrite("  no custom hardpoint\n"); continue; };
          if (string.IsNullOrEmpty(customHardpoint.shaderSrc)) { Log.LogWrite("  no shader source\n"); continue; };
          Log.LogWrite("  shader source " + customHardpoint.shaderSrc + " requested\n");
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, customHardpoint.shaderSrc);
        }
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("InventoryPrefabsLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  typeof(uint) })]
  public static class MechDef_InventoryPrefabsLoaded {
    public static bool Prefix(MechDef __instance, uint loadWeight, MechComponentRef[] ___inventory, ref bool __result) {
      __result = false;
      try {
        Log.TWL(0, "MechDef.InventoryPrefabsLoaded defId:" + __instance.Description.Id + " " + loadWeight);
        if (__instance.Chassis == null) { __result = false; return false; }
        if (__instance.Chassis.HardpointDataDef == null) { __result = false; return false; }
        HashSet<MechComponentRef> inventory = ___inventory.ToHashSet();
        inventory.Add(__instance.imaginaryLaserWeaponRef);
        inventory.Add(__instance.meleeWeaponRef);
        inventory.Add(__instance.dfaWeaponRef);
        foreach (MechComponentRef compRef in inventory) {
          if (compRef.Def == null) { __result = false; return false; }
          if (loadWeight <= 10U) { continue; }
          if (compRef.hasPrefabName == false) { continue; }
          if (string.IsNullOrEmpty(compRef.prefabName)) { continue; }
          if (compRef.prefabName == HardpointCalculator.FakeWeaponPrefab) { continue; }
          if (compRef.prefabName == HardpointCalculator.FakeComponentPrefab) { continue; }
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(compRef.prefabName);
          Log.WL(1,"prefab:" + compRef.prefabName);
          if (customHardpoint == null) {
            Log.WL(2, "no custom hardpoint");
            if (__instance.DataManager.Exists(BattleTechResourceType.Prefab, compRef.prefabName) == false) {
              Log.WL(3, "not exists in data manager");
              __result = false; return false;
            }
          } else {
            Log.WL(2, "custom hardpoint prefab:'"+ customHardpoint.prefab+"'");
            if (string.IsNullOrEmpty(customHardpoint.prefab) == false) {
              if (__instance.DataManager.Exists(BattleTechResourceType.Prefab, customHardpoint.prefab) == false) {
                Log.WL(3, "not exists in data manager");
                __result = false; return false;
              }
              if (string.IsNullOrEmpty(customHardpoint.shaderSrc)) {
                Log.WL(3, "no shader source\n");
              } else {
                if (__instance.DataManager.Exists(BattleTechResourceType.Prefab, customHardpoint.shaderSrc) == false) {
                  Log.WL(3,"shader source " + customHardpoint.shaderSrc + " not loaded\n");
                  __result = false; return false;
                }
              }
            } else {
              if (__instance.DataManager.Exists(BattleTechResourceType.Prefab, compRef.prefabName) == false) {
                Log.WL(3, "not exists in data manager");
                __result = false; return false;
              }
            }
          }
        }
        __result = true;
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("OnAttackComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackDirector_OnAttackComplete {
    public static void Prefix(AttackDirector __instance, MessageCenterMessage message) {
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      int sequenceId = attackCompleteMessage.sequenceId;
      AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
      if (attackSequence == null) { return; }
      foreach(Weapon weapon in attackSequence.attacker.Weapons) {
        AttachInfo info = weapon.attachInfo();
        if (info != null) { info.Postfire(); };
        if(weapon.weaponRep != null) {
          WeaponAttachRepresentation weaponAttach = weapon.weaponRep.GetComponent<WeaponAttachRepresentation>();
          if (weaponAttach != null) { if (weaponAttach.attachPoint != null) { weaponAttach.attachPoint.Postfire(); }; };
        }
      }
      CustomMechRepresentation custRep = attackSequence.attacker.GameRep as CustomMechRepresentation;
      if (custRep != null) { custRep.OnAttackComplete(); }
    }
  }
  //[HarmonyPatch(typeof(Vehicle))]
  //[HarmonyPatch("InitGameRep")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Transform) })]
  //public static class Vehicle_InitGameRep {
  //  public static void Postfix(Vehicle __instance) {
  //    Log.TWL(0, "Vehicle.InitGameRep "+__instance.VehicleDef.ChassisID);
  //    VTOLBodyAnimation bodyAnimation = __instance.VTOLAnimation();
  //    if (bodyAnimation == null) { return; }
  //    bodyAnimation.ResolveAttachPoints();
  //    Log.TWL(0, "Vehicle.InitGameRep:" + new Text(__instance.DisplayName).ToString());
  //    foreach (Weapon weapon in __instance.Weapons) {
  //      Log.WL(1, weapon.defId + " representation:" + (weapon.weaponRep == null ? "null" : weapon.weaponRep.GetInstanceID().ToString()));
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(Turret))]
  //[HarmonyPatch("InitGameRep")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Transform) })]
  //public static class Turret_InitGameRep {
  //  public static void Postfix(Turret __instance) {
  //    Log.TWL(0, "Turret.InitGameRep:" + new Text(__instance.DisplayName).ToString());
  //    foreach (Weapon weapon in __instance.Weapons) {
  //      Log.WL(1, weapon.defId + " representation:" + (weapon.weaponRep == null ? "null" : weapon.weaponRep.GetInstanceID().ToString()));
  //    }
  //  }
  //}
  public class ExtPrefire {
    public float t;
    public float rate;
    public int hitIndex;
    public int emitter;
    public WeaponHitInfo hitInfo;
    public WeaponEffect effect;
    public ExtPrefire(float rate, WeaponHitInfo hitInfo, int hitIndex, int emitter) {
      t = 0f; this.rate = rate;
      this.hitInfo = hitInfo;
      this.hitIndex = hitIndex;
      this.emitter = emitter;
    }
  }
  public static class extendedFireHelper {
    public static void extendedFire(WeaponEffect weaponEffect, WeaponHitInfo hitInfo, int hitIndex, int emiter) {
      Log.TWL(0, "extendedFireHelper.extendedFire "+ weaponEffect.GetType().ToString());
      ExtPrefire extPrefire = weaponEffect.extPrefire();
      if (extPrefire != null) {
        weaponEffect.extPrefire(null);
      } else {
        try {
          bool indirect = weaponEffect.weapon.parent.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId).indirectFire;
          if (weaponEffect.weapon.AlwaysIndirectVisuals()) { indirect = true; }
          AttachInfo info = null;
          if (weaponEffect.weapon.weaponRep != null) {
            WeaponAttachRepresentation weaponAttach = weaponEffect.weapon.weaponRep.GetComponent<WeaponAttachRepresentation>();
            if(weaponAttach != null) {
              if(weaponAttach.attachPoint != null) { info = weaponAttach.attachPoint; }
            }
          }
          if (info == null) { info = weaponEffect.weapon.attachInfo(); }
          if (info != null) { 
            info.Prefire(weaponEffect.weapon, hitInfo.hitPositions[hitIndex], indirect);
            weaponEffect.extPrefire(new ExtPrefire(info.LowestPrefireRate, hitInfo, hitIndex, emiter));
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
          weaponEffect.extPrefire(null);
        }
      }
      weaponEffect.Fire(hitInfo, hitIndex, emiter);
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_UpdatePrefire {
    public static MethodInfo getPlayPreFire(Type type) {
      MethodInfo result = type.GetMethod("PlayPreFire", BindingFlags.NonPublic | BindingFlags.Instance);
      if (result != null) { return result; };
      if (result == typeof(WeaponEffect)) { return null; }
      return getPlayPreFire(type.BaseType);
    }
    public static bool Prefix(WeaponEffect __instance, ref float ___preFireRate, CombatGameState ___Combat, ref float ___t) {
      if (__instance.currentState != WeaponEffect.WeaponEffectState.PreFiring) { return true; }
      ExtPrefire extPrefire = __instance.extPrefire();
      if (extPrefire == null) { return true; }
      if (extPrefire.t <= 1.0f) {
        extPrefire.t += extPrefire.rate * ___Combat.StackManager.GetProgressiveAttackDeltaTime(___t);
      }
      if (extPrefire.t >= 1.0f) {
        Log.TWL(0, "WeaponEffect.Update real prefire");
        try {
          __instance.currentState = WeaponEffect.WeaponEffectState.NotStarted;
          AdvWeaponHitInfo advInfo = extPrefire.hitInfo.advInfo();
          if(advInfo != null) {
            foreach(AdvWeaponHitInfoRec hit in advInfo.hits) {
              if (hit.isAOE) { continue; }
              if (hit.fragInfo.isFragPallet) { continue; }
              Vector3 newPos = __instance.weapon.weaponRep.vfxTransforms[hit.hitIndex % __instance.weapon.weaponRep.vfxTransforms.Length].position;
              if (newPos != hit.startPosition) {
                Log.WL(1, "start position moved "+ hit.startPosition+" -> "+newPos+" updating trajectory");
                hit.GenerateTrajectory();
              }
            }
          }
          __instance.Fire(extPrefire.hitInfo,extPrefire.hitIndex,extPrefire.emitter);
          __instance.extPrefire(null);
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
        //MethodInfo mPlayPrefire = __instance.GetType().GetMethod("PlayPreFire", BindingFlags.Instance | BindingFlags.NonPublic);
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("Fire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
  public static class WeaponEffect_Fire {
    private static Dictionary<WeaponEffect, ExtPrefire> extPrefireRates = new Dictionary<WeaponEffect, ExtPrefire>();
    public static ExtPrefire extPrefire(this WeaponEffect effect) {
      if(extPrefireRates.TryGetValue(effect, out ExtPrefire res)) {
        return res;
      }
      return null;
    }
    public static void extPrefire(this WeaponEffect effect, ExtPrefire extPrefire) {
      if (extPrefire == null) { extPrefireRates.Remove(effect); } else {
        if (extPrefireRates.ContainsKey(effect)) {
          extPrefireRates[effect] = extPrefire;
        } else {
          extPrefireRates.Add(effect,extPrefire);
        }
      }
    }

    public static bool Prefix(WeaponEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      try {
        ExtPrefire extPrefire = __instance.extPrefire();
        if (extPrefire != null) { return true; }
        bool indirect = false;
        AttackDirector.AttackSequence sequence = __instance.weapon.parent.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        Log.TWL(0, $"WeaponEffect.Fire {hitInfo.attackSequenceId} indirectFire:{(sequence == null?"null":sequence.indirectFire.ToString())}");
        if (sequence != null) { indirect = sequence.indirectFire; }
        //__instance.weapon.parent.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId).indirectFire;
        AttachInfo info = null;
        if (__instance.weapon.AlwaysIndirectVisuals()) { indirect = true; }
        if (__instance.weapon.weaponRep != null) {
          WeaponAttachRepresentation weaponAttach = __instance.weapon.weaponRep.GetComponent<WeaponAttachRepresentation>();
          if (weaponAttach != null) {
            if (weaponAttach.attachPoint != null) { info = weaponAttach.attachPoint; }
          }
        }
        if (info == null) { info = __instance.weapon.attachInfo(); }
        if (info != null) {
          float lowestPrefire = info.LowestPrefireRate;
          if (lowestPrefire < 10f) {
            info.Prefire(__instance.weapon, hitInfo.hitPositions[hitIndex], indirect);
            __instance.extPrefire(new ExtPrefire(lowestPrefire, hitInfo, hitIndex, emitterIndex));
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayMuzzleFlash")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayMuzzleFlash {
    public static void Postfix(WeaponEffect __instance) {
      Type type = __instance.GetType();
      if (
        (type != typeof(BulletEffect)) 
        && (type != typeof(MultiShotBulletEffect)) 
        && (type != typeof(PPCEffect))
        && (type != typeof(GaussEffect) )
        && (type != typeof(LBXEffect))
        && (type != typeof(MultiShotLBXBulletEffect))
        && (type != typeof(MultiShotPulseEffect))
      ) {
        return;
      }
      AttachInfo info = __instance.weapon.attachInfo();
      if (info != null) { info.Recoil(); }
      WeaponAttachRepresentation weaponAttach = __instance.weapon.weaponRep.GetComponent<WeaponAttachRepresentation>();
      if(weaponAttach != null) {
        if(weaponAttach.attachPoint != null) {
          weaponAttach.attachPoint.Recoil();
        }
      }
    }
  }
  [HarmonyPatch(typeof(ComponentRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Transform), typeof(bool), typeof(bool), typeof(string) })]
  public static class ComponentRepresentation_Init {
    public static void Prefix(ComponentRepresentation __instance, ICombatant actor, Transform parentTransform, bool isParented, bool leaveMyParentTransformOutOfThis, string parentDisplayName) {
      __instance.renderers = __instance.gameObject.GatherRenderers().ToList();
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("InitGameRep")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(Transform), typeof(string) })]
  public static class Weapon_InitGameRep {
    public static void printComponents(this GameObject obj, int level) {
      Component[] components = obj.GetComponents<Component>();
      Log.LogWrite(level, "object:" + obj.name + "\n");
      Log.LogWrite(level, "components(" + components.Length + "):\n");
      foreach (Component component in components) {
        Log.LogWrite(level + 1, component.name + ":" + component.GetType().ToString() + "\n");
      }
      Log.LogWrite(level, "childs(" + obj.transform.childCount + "):\n");
      for (int t = 0; t < obj.transform.childCount; ++t) {
        obj.transform.GetChild(t).gameObject.printComponents(level + 1);
      }
    }
    public static HashSet<Renderer> GatherRenderers(this GameObject go) {
      HashSet<Renderer> result = new HashSet<Renderer>();
      MeshRenderer[] mrenderers = go.GetComponentsInChildren<MeshRenderer>(true);
      foreach (MeshRenderer r in mrenderers) { result.Add(r); }
      SkinnedMeshRenderer[] srenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer r in srenderers) { result.Add(r); }
      return result;
    }
    public static bool Prefix(Weapon __instance, string prefabName, Transform parentBone, string parentDisplayName, CombatGameState ___combat) {
      Log.TWL(0, "Weapon.InitGameRep: " + __instance.defId + ":" + prefabName);
      try {
        if (string.IsNullOrEmpty(prefabName)) { prefabName = HardpointCalculator.FakeWeaponPrefab; }
        WeaponRepresentation component = null;
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(prefabName);
        GameObject prefab = null;
        HardpointAttachType attachType = HardpointAttachType.None;
        if (customHardpoint != null) {
          Log.WL(1, prefabName + " custom hardpoint found "+ customHardpoint.prefab);
          attachType = customHardpoint.attachType;
          prefab = ___combat.DataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if (prefab == null) {
            prefab = ___combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          } else {
            prefabName = customHardpoint.prefab;
          }
        } else {
          Log.LogWrite(1, prefabName + " have no custom hardpoint\n", true);
          prefab = ___combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        }
        if (prefab == null) {
          Log.LogWrite(1, prefabName + " absent prefab. fallback\n", true);
          prefab = new GameObject(prefabName);
        }
        component = prefab.GetComponent<WeaponRepresentation>();
        if (component == null) {
          Log.LogWrite(1, prefabName + " have no WeaponRepresentation\n", true);
          component = prefab.AddComponent<WeaponRepresentation>();
          if (customHardpoint != null) {
            Log.LogWrite(1, "reiniting vfxTransforms\n");
            List<Transform> transfroms = new List<Transform>();
            for (int index = 0; index < customHardpoint.emitters.Count; ++index) {
              Transform[] trs = component.GetComponentsInChildren<Transform>();
              foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[index]) { transfroms.Add(tr); break; } }
            }
            Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
            for (int index = 0; index < transfroms.Count; ++index) {
              Log.LogWrite(2, transfroms[index].name + ":" + transfroms[index].localPosition + "\n");
            }
            if (transfroms.Count == 0) { transfroms.Add(prefab.transform); };
            component.vfxTransforms = transfroms.ToArray();
            if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
              Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
              GameObject shaderPrefab = ___combat.DataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
              if (shaderPrefab != null) {
                Log.LogWrite(1, "shader prefab found\n");
                Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
                if (shaderComponent != null) {
                  Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
                  HashSet<Renderer> renderers = prefab.GatherRenderers();
                  foreach (Renderer renderer in renderers) {
                    for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                      if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
                        Log.LogWrite(2, "keep original shader: "+ renderer.gameObject.transform.name + "\n");
                        continue;
                      }
                      Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
                      renderer.materials[mindex].shader = shaderComponent.material.shader;
                      renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
                    }
                  }
                }
                ___combat.DataManager.PoolGameObject(customHardpoint.shaderSrc, shaderPrefab);
              }
            }
          } else {
            component.vfxTransforms = new Transform[] { component.transform };
          }
        }
        if (component == null) {
          string str = string.Format("Null WeaponRepresentation for prefabName[{0}] parentBoneName[{1}] parentDisplayName[{2}]", (object)prefabName, (object)parentBone.name, (object)parentDisplayName);
          Log.LogWrite(1, str + "\n");
        } else {
          Log.LogWrite(1, "component representation is not null\n");
        }
        typeof(MechComponent).GetProperty("componentRep", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { component });
        //__instance.componentRep = (ComponentRepresentation)component;
        if (__instance.weaponRep == null) {
          Log.LogWrite(1, "weapon representation still null\n", true);
          return false;
        }
        CustomHardpointRepresentation customHardpointRep = __instance.weaponRep.gameObject.GetComponent<CustomHardpointRepresentation>();
        if (customHardpointRep == null) { customHardpointRep = __instance.weaponRep.gameObject.AddComponent<CustomHardpointRepresentation>(); }
        customHardpointRep.Init(__instance.weaponRep, customHardpoint);
        if (__instance.parent != null) {
          VTOLBodyAnimation bodyAnimation = __instance.parent.VTOLAnimation();
          if((bodyAnimation != null)&&(__instance.vehicleComponentRef != null)) {
            Log.WL(1, "found VTOL body animation and vehicle component ref. Location:"+ __instance.vehicleComponentRef.MountedLocation.ToString()+" type:"+attachType);
            if (attachType == HardpointAttachType.None) {
              if ((bodyAnimation.bodyAttach != null)&&(__instance.vehicleComponentRef.MountedLocation != VehicleChassisLocations.Turret)) { parentBone = bodyAnimation.bodyAttach; }
            } else { 
              AttachInfo attachInfo = bodyAnimation.GetAttachInfo(__instance.vehicleComponentRef.MountedLocation.ToString(), attachType);
              Log.WL(2, "attachInfo:" + (attachInfo == null ? "null" : "not null"));
              if ((attachInfo != null) && (attachInfo.attach != null) && (attachInfo.main != null)) {
                Log.WL(2, "attachTransform:" + (attachInfo.attach == null ? "null" : attachInfo.attach.name));
                Log.WL(2, "mainTransform:" + (attachInfo.main == null ? "null" : attachInfo.main.name));
                parentBone = attachInfo.attach;
                __instance.attachInfo(attachInfo);
                attachInfo.weapons.Add(__instance);
                WeaponAttachRepresentation attachRepresentation = __instance.weaponRep.gameObject.GetComponent<WeaponAttachRepresentation>();
                if (attachRepresentation = null) { attachRepresentation = __instance.weaponRep.gameObject.AddComponent<WeaponAttachRepresentation>(); }
                attachRepresentation.Init(__instance.weaponRep, attachInfo);
              }
            }
          }
        }
        __instance.weaponRep.Init(__instance, parentBone, true, parentDisplayName, __instance.Location);
        if (customHardpoint != null) {
          if (customHardpoint.offset.set) {
            Log.W("Altering position:" + prefab.transform.localPosition + " -> ");
            prefab.transform.localPosition += customHardpoint.offset.vector;
            Log.WL(prefab.transform.localPosition.ToString());
          }
          if (customHardpoint.scale.set) {
            Log.W("Altering scale:" + prefab.transform.localScale + " -> ");
            prefab.transform.localScale = customHardpoint.scale.vector;
            Log.WL(prefab.transform.localScale.ToString());
          }
          if (customHardpoint.rotate.set) {
            Log.W("Altering rotation:" + prefab.transform.localRotation.eulerAngles + " -> ");
            prefab.transform.localRotation = Quaternion.Euler(customHardpoint.rotate.vector);
            Log.WL(prefab.transform.localRotation.eulerAngles.ToString());
          }
        }
        HardPointAnimationController animComponent = __instance.weaponRep.GetComponent<HardPointAnimationController>();
        if (animComponent == null) {
          animComponent = __instance.weaponRep.gameObject.AddComponent<HardPointAnimationController>(); animComponent.Init(__instance.weaponRep, customHardpoint);
          animComponent.InBattle = true;
        }
        ParticleSystem[] pss = prefab.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          Log.WL("Starting ParticleSystem:"+ps.name);
          ps.Play();
        }
        return false;
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
        return true;
      }
    }
    public static void Postfix(Weapon __instance, string prefabName, Transform parentBone, string parentDisplayName) {
      try {
        Log.LogWrite(0, "Weapon.InitGameRep postfix: " + __instance.defId + "\n");
        Log.LogWrite(1, prefabName + ":" + parentBone.name + "\n");
        if (__instance == null) { return; }
        if (__instance.weaponRep == null) {
          Log.LogWrite(1, "null. creating empty fallback\n");
          GameObject prefab = new GameObject("fake_hardpoint");
          WeaponRepresentation component = prefab.AddComponent<WeaponRepresentation>();
          typeof(MechComponent).GetProperty("componentRep", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { component });
          __instance.weaponRep.Init(__instance, parentBone, true, parentDisplayName, __instance.Location);
        }
        if (__instance.weaponRep.gameObject == null) { return; }
        //__instance.weaponRep.gameObject.printComponents(1);
      }catch(Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("Exists")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(BattleTechResourceType), typeof(string) })]
  public static class DataManager_Exists {
    public static void Prefix(DataManager __instance, BattleTechResourceType resourceType,ref string id) {
      try {
        if (resourceType != BattleTechResourceType.Prefab) { return; }
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(id);
        if (customHardpoint != null) {
          Log.TWL(0, "DataManager.Exists " + id + " -> " + (customHardpoint == null ? "null" : customHardpoint.prefab));
        }
        if (customHardpoint != null) { id = customHardpoint.prefab; }
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
    public static void Postfix(DataManager __instance, BattleTechResourceType resourceType, string id, ref bool __result) {
      if (__result == false) { return; }
      if (resourceType != BattleTechResourceType.Prefab) { return; }
      __result = CustomPrefabHelper.Exists(__instance, id);
    }
  }
  [HarmonyPatch(typeof(DependencyLoadRequest))]
  [HarmonyPatch("RequestResource")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(BattleTechResourceType), typeof(string) })]
  public static class DependencyLoadRequest_RequestResource {
    public static void Prefix(DependencyLoadRequest __instance, BattleTechResourceType type, ref string id) {
      try {
        if (type != BattleTechResourceType.Prefab) { return; }
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(id);
        if (customHardpoint != null) {
          Log.TWL(0, "DependencyLoadRequest.RequestResource " + id + " -> " + (customHardpoint == null ? "no change" : customHardpoint.prefab));
        }
        if (customHardpoint != null) { id = customHardpoint.prefab; }
        CustomPrefabHelper.RequestResource(__instance, id);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(DependencyLoadRequest))]
  [HarmonyPatch("Contains")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(BattleTechResourceType), typeof(string) })]
  public static class DependencyLoadRequest_Contains {
    public static void Prefix(DependencyLoadRequest __instance, BattleTechResourceType type,ref string resourceId) {
      try {
        if (type != BattleTechResourceType.Prefab) { return; }
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(resourceId);
        Log.TWL(0, "DependencyLoadRequest.Contains " + resourceId + " -> " + (customHardpoint == null ? "null" : customHardpoint.prefab));
        if (customHardpoint != null) { resourceId = customHardpoint.prefab; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(typeof(DataManager))]
  //[HarmonyPatch("PooledInstantiate")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyBefore("io.mission.modrepuation")]
  //[HarmonyPatch(new Type[] { typeof(string), typeof(BattleTechResourceType), typeof(Vector3?), typeof(Quaternion?), typeof(Transform) })]
  //public static class DataManager_PooledInstantiate {
  //  public static void Prefix(DataManager __instance,ref string id, BattleTechResourceType resourceType, Vector3? position, Quaternion? rotation, Transform parent, ref string __state) {
  //    try {
  //      if (resourceType != BattleTechResourceType.Prefab) { return; }
  //      __state = string.Empty;
  //      __state += id;
  //      CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(__state);
  //      if (customHardpoint != null) {
  //        id = customHardpoint.prefab;
  //      } else {
  //        CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.FindSimGame(__state);
  //        if (custRepDef != null) {
  //          id = custRepDef.SourcePrefabIdentifier;
  //        } else {
  //          custRepDef = CustomActorRepresentationHelper.FindSimGame(__state);
  //          if (custRepDef != null) {
  //            id = __state.Replace(custRepDef.PrefabBase, custRepDef.SourcePrefabBase);
  //          }
  //        }
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //  public static void Postfix(DataManager __instance, string id, BattleTechResourceType resourceType, Vector3? position, Quaternion? rotation, Transform parent, GameObject __result, ref string __state) {
  //    return;
  //    if (resourceType != BattleTechResourceType.Prefab) { return; }
  //    if (id == __state) { return; }
  //    CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(id);
  //    if (custRepDef != null) {
  //      CustomActorRepresentationHelper.ProcessBattle(__instance, ref __result, custRepDef);
  //    } else {
  //      custRepDef = CustomActorRepresentationHelper.FindSimGame(id);
  //      if (custRepDef != null) {
  //        //CustomActorRepresentationHelper.ProcessSimgame(__instance, id, resourceType, position, rotation, parent, __result);
  //      }
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PoolGameObject")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(GameObject) })]
  public static class DataManager_PoolGameObject {
    public static bool Prefix(DataManager __instance, ref string id, GameObject gameObj) {
      try {
        if (gameObj == null) { return false; }
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(id);
        Log.TWL(0, "DataManager.PoolGameObject " + id + "("+gameObj.name+") -> " + (customHardpoint == null ? "null" : customHardpoint.prefab));
        if (id == "chrPrfMech_atlasBase-001") { Log.WL(1,Environment.StackTrace); }
        if (customHardpoint != null) { id = customHardpoint.prefab; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
}
