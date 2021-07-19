using BattleTech;
using BattleTech.Data;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
#pragma warning disable CS0252
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("InitFromSave")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_InitFromSave {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(MechDef), "RefreshInventory");
      var replacementMethod = AccessTools.Method(typeof(MechDef_RefreshInventory), "RefreshInventory");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_Refresh {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(MechDef), "RefreshInventory");
      var replacementMethod = AccessTools.Method(typeof(MechDef_RefreshInventory), "RefreshInventory");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("SetInventory")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_SetInventory {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(MechDef), "RefreshInventory");
      var replacementMethod = AccessTools.Method(typeof(MechDef_RefreshInventory), "RefreshInventory");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
  }

  //[HarmonyPatch(typeof(MechDef))]
  //[HarmonyPatch("RefreshInventory")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  public static class MechDef_RefreshInventory {
    public static HashSet<string> CollectInventoryPrefabs(this MechDef mechDef, bool updatePrefabName) {
      HashSet<string> result = new HashSet<string>();
      MechComponentRef[] inventory = Traverse.Create(mechDef).Field<MechComponentRef[]>("inventory").Value;
      Log.TWL(0, "MechDef.CollectInventoryPrefabs " + mechDef.Description.Id + " inventory:" + inventory.Length);
      for (int index = 0; index < inventory.Length; ++index) {
        if (inventory[index].Def != null) {
          string prefabName = inventory[index].prefabName;
          if (string.IsNullOrEmpty(prefabName)) { continue; }
          Log.WL(1, prefabName);
          if (prefabName.Contains("|")) {
            string[] prefabNames = prefabName.Split('|');
            foreach (string name in prefabNames) {
              Log.WL(2, name);
              if (string.IsNullOrEmpty(name)) { continue; }
              if (name == HardpointCalculator.FakeWeaponPrefab) { continue; }
              result.Add(name);
            }
            inventory[index].prefabName = prefabNames[0];
          } else {
            if (prefabName == HardpointCalculator.FakeWeaponPrefab) { continue; }
            result.Add(prefabName);
          }
        }
      }
      if (mechDef.meleeWeaponRef.Def != null)
        result.Add(mechDef.meleeWeaponRef.prefabName);
      if (mechDef.dfaWeaponRef.Def != null) {
        result.Add(mechDef.dfaWeaponRef.prefabName);
      }
      return result;
    }
    public static void RequestInventoryPrefabs(this MechDef mechDef, DataManager.DependencyLoadRequest dependencyLoad, uint loadWeight) {
      if (loadWeight <= 10U) return;
      MechComponentRef[] inventory = Traverse.Create(mechDef).Field<MechComponentRef[]>("inventory").Value;
      Log.TWL(0, "MechDef.RequestInventoryPrefabs "+mechDef.Description.Id+" inventory:"+inventory.Length);
      HashSet<string> prefabNames = mechDef.CollectInventoryPrefabs(true);
      foreach(string prefabName in prefabNames) {
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, prefabName);
      }
    }

    public static void RequestMechInventoryPrefabsNormal(this HardpointDataDef hardpointDataDef, List<HardpointCalculator.Element> components) {
      List<string> usedPrefabs = new List<string>();
      HardpointCalculator calculator = new HardpointCalculator();
      calculator.Init(components, hardpointDataDef);
      foreach(HardpointCalculator.Element component in components) {
        if (component.componentRef.hasPrefabName == false) {
          component.componentRef.prefabName = calculator.GetComponentPrefabName(component.componentRef);
          component.componentRef.hasPrefabName = true;
        }
      }
    }
    public static void RequestMechInventoryPrefabsAlternates(this MechDef mechDef, List<HardpointCalculator.Element> components) {
      UnitCustomInfo info = mechDef.GetCustomInfo();
      if (info == null) { mechDef.Chassis.HardpointDataDef.RequestMechInventoryPrefabsNormal(components); return; }
      if (info.AlternateRepresentations.Count <= 0) { mechDef.Chassis.HardpointDataDef.RequestMechInventoryPrefabsNormal(components); }
      Dictionary<HardpointDataDef, HardpointCalculator> hardpoints = new Dictionary<HardpointDataDef, HardpointCalculator>();
      hardpoints.Add(mechDef.Chassis.HardpointDataDef, new HardpointCalculator());
      hardpoints[mechDef.Chassis.HardpointDataDef].Init(components, mechDef.Chassis.HardpointDataDef);
      foreach (AlternateRepresentationDef altDef in info.AlternateRepresentations) {
        HardpointDataDef hardpoint = mechDef.DataManager.GetObjectOfType<HardpointDataDef>(altDef.HardpointDataDef, BattleTechResourceType.HardpointDataDef);
        if (hardpoint == null) { continue; }
        HardpointCalculator calculator = new HardpointCalculator();
        calculator.Init(components, hardpoint);
        if (hardpoints.ContainsKey(hardpoint)) { continue; }
        hardpoints.Add(hardpoint, calculator);
      }
      List<string> usedPrefabs = new List<string>();
      foreach (HardpointCalculator.Element component in components) {
        if (component.componentRef.hasPrefabName == false) {
          component.componentRef.prefabName = string.Empty;
          foreach (var alternanates in hardpoints) {
            string prefabName = alternanates.Value.GetComponentPrefabName(component.componentRef);
            if (string.IsNullOrEmpty(component.componentRef.prefabName) == false) { component.componentRef.prefabName += "|"; }
            component.componentRef.prefabName += prefabName;
          }
          component.componentRef.hasPrefabName = true;
        }
      }
    }
    public static void RequestMechInventoryPrefabsSquad(this MechDef mechDef, List<HardpointCalculator.Element> components) {
      UnitCustomInfo info = mechDef.GetCustomInfo();
      int SquadSize = 1;
      if (info != null) { SquadSize = info.SquadInfo.Troopers; }
      if (SquadSize <= 1) { mechDef.RequestMechInventoryPrefabsAlternates(components); return; }
      for (int unit_index = 0; unit_index < SquadSize; ++unit_index) {
        ChassisLocations unitLocation = TrooperSquad.locations[unit_index];
        List<HardpointCalculator.Element> squad_components = new List<HardpointCalculator.Element>();
        foreach (HardpointCalculator.Element component in components) {
          if (component.location != unitLocation) { continue; }
          ChassisLocations loc = ChassisLocations.CenterTorso;
          if(component.componentRef.Def is WeaponDef weapon) {
            if (info.SquadInfo.Hardpoints.TryGetValue(weapon.WeaponCategoryValue.Name, out loc)) { }
          }
          squad_components.Add(new HardpointCalculator.Element() { location = loc, componentRef = component.componentRef });
        }
        mechDef.RequestMechInventoryPrefabsAlternates(squad_components);
      }
    }
    public static void RefreshInventory(this MechDef mechDef) {
      Log.TWL(0, "MechDef.RefreshInventory " + mechDef.ChassisID);
      mechDef.InsertFixedEquipmentIntoInventory();
      MechComponentRef[] inventory = Traverse.Create(mechDef).Field<MechComponentRef[]>("inventory").Value;
      for (int index = 0; index < inventory.Length; ++index) {
        MechComponentRef mechComponentRef = inventory[index];
        if (mechComponentRef == null) {
          Debug.Log((object)"Found an empty inventory slot");
        } else {
          if (mechComponentRef.DataManager == null)
            mechComponentRef.DataManager = mechDef.DataManager;
          mechComponentRef.RefreshComponentDef();
        }
      }
      mechDef.meleeWeaponRef.DataManager = mechDef.DataManager;
      mechDef.meleeWeaponRef.RefreshComponentDef();
      if (mechDef.meleeWeaponRef.Def != null && !mechDef.meleeWeaponRef.hasPrefabName) {
        mechDef.meleeWeaponRef.prefabName = "chrPrfWeap_generic_melee";
        mechDef.meleeWeaponRef.hasPrefabName = true;
      }
      mechDef.dfaWeaponRef.DataManager = mechDef.DataManager;
      mechDef.dfaWeaponRef.RefreshComponentDef();
      if (mechDef.dfaWeaponRef.Def != null && !mechDef.dfaWeaponRef.hasPrefabName) {
        mechDef.dfaWeaponRef.prefabName = "chrPrfWeap_generic_melee";
        mechDef.dfaWeaponRef.hasPrefabName = true;
      }
      mechDef.imaginaryLaserWeaponRef.DataManager = mechDef.DataManager;
      mechDef.imaginaryLaserWeaponRef.RefreshComponentDef();
      if (mechDef.imaginaryLaserWeaponRef.Def == null || mechDef.imaginaryLaserWeaponRef.hasPrefabName)
        return;
      mechDef.imaginaryLaserWeaponRef.prefabName = "chrPrfWeap_generic_melee";
      mechDef.imaginaryLaserWeaponRef.hasPrefabName = true;
      if(mechDef.Chassis != null) {
        if(mechDef.Chassis.HardpointDataDef != null) {
          List<HardpointCalculator.Element> components = new List<HardpointCalculator.Element>();
          bool unrequestedComponents = false;
          foreach(MechComponentRef component in inventory) {
            if (component.Def == null) { continue; }
            if (component.hasPrefabName == false) { unrequestedComponents = true; };
            components.Add(new HardpointCalculator.Element() { location = component.MountedLocation, componentRef = component });
          }
          if (unrequestedComponents) {
            mechDef.RequestMechInventoryPrefabsSquad(components);
          }
        }
      }
    }
    //private static Dictionary<int, HardpointCalculator> calulatorStack = new Dictionary<int, HardpointCalculator>();
#pragma warning restore CS0252

    private static string GetComponentPrefabName(ChassisDef chassis, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames) {
      Log.TWL(0, "MechDef.RefreshInventory.GetComponentPrefabName chassis " + chassis.Description.Id + " isVehicle: " + chassis.IsVehicle() + " isVehicleStyle: " + chassis.HardpointDataDef.CustomHardpoints().IsVehicleStyleLocations);
      Log.W(1, location);
      bool VehicleStyle = false;
      if (chassis.Description.Id.IsInFakeChassis()) { VehicleStyle = true; }
      if (chassis.IsVehicle() && chassis.HardpointDataDef.CustomHardpoints().IsVehicleStyleLocations) { VehicleStyle = true; }
      if (VehicleStyle) {
        if (location == ChassisLocations.LeftArm.ToString().ToLower()) {
          location = VehicleChassisLocations.Front.ToString().ToLower();
        } else if (location == ChassisLocations.RightArm.ToString().ToLower()) {
          location = VehicleChassisLocations.Rear.ToString().ToLower();
        } else if (location == ChassisLocations.LeftLeg.ToString().ToLower()) {
          location = VehicleChassisLocations.Left.ToString().ToLower();
        } else if (location == ChassisLocations.RightLeg.ToString().ToLower()) {
          location = VehicleChassisLocations.Right.ToString().ToLower();
        } else if (location == ChassisLocations.CenterTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.RightTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.LeftTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.Head.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        }
      }
      Log.WL(0, "->" + location);
      return MechHardpointRules.GetComponentPrefabName(chassis.HardpointDataDef, componentRef, prefabBase, location, ref usedPrefabNames);
    }
  }

  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class VehicleChassisDef_GatherDependencies {
    public static void AddCustomDeps(this UnitCustomInfo info, DataManager.DependencyLoadRequest dependencyLoad) {
      foreach (CustomPart part in info.CustomParts) {
        if (string.IsNullOrEmpty(part.prefab)) { continue; }
        Log.LogWrite(1, "additional prefab:" + part.prefab, true);
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, part.prefab);
        foreach (var mi in part.MaterialInfo) {
          if (string.IsNullOrEmpty(mi.Value.shader) == false) {
            Log.LogWrite(1, "additional shader:" + mi.Value.shader, true);
            dependencyLoad.RequestResource(BattleTechResourceType.Prefab, mi.Value.shader);
          }
          foreach (var ti in mi.Value.materialTextures) {
            if (string.IsNullOrEmpty(ti.Value) == false) {
              Log.LogWrite(1, "additional textures:" + ti.Value, true);
              dependencyLoad.RequestResource(BattleTechResourceType.Texture2D, ti.Value);
            }
          }
        }
      }
      Log.LogWrite(1, "additional melee def:" + info.MeleeWeaponOverride.DefaultWeapon, true);
      dependencyLoad.RequestResource(BattleTechResourceType.WeaponDef, info.MeleeWeaponOverride.DefaultWeapon);
      foreach (var cm in info.MeleeWeaponOverride.Components) {
        Log.LogWrite(1, "additional melee def:" + cm.Key, true);
        dependencyLoad.RequestResource(BattleTechResourceType.WeaponDef, cm.Key);
      }
    }
    public static void AddCustomDeps(this ChassisDef chassis, LoadRequest loadRequest) {
      UnitCustomInfo info = chassis.GetCustomInfo();
      if (info != null) {
        foreach (CustomPart part in info.CustomParts) {
          if (string.IsNullOrEmpty(part.prefab)) { continue; }
          Log.LogWrite(1, "additional prefab:" + part.prefab, true);
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, part.prefab, new bool?(false));
          foreach (var mi in part.MaterialInfo) {
            if (string.IsNullOrEmpty(mi.Value.shader) == false) {
              Log.LogWrite(1, "additional shader:" + mi.Value.shader, true);
              loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mi.Value.shader, new bool?(false));
            }
            foreach (var ti in mi.Value.materialTextures) {
              if (string.IsNullOrEmpty(ti.Value) == false) {
                Log.LogWrite(1, "additional textures:" + ti.Value, true);
                loadRequest.AddBlindLoadRequest(BattleTechResourceType.Texture2D, ti.Value, new bool?(false));
              }
            }
          }
        }
      }
    }
    public static void AddQuadDeps(this ChassisDef chassis, LoadRequest loadRequest) {
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(chassis.PrefabIdentifier);
      if (custRepDef != null) {
        if (custRepDef.quadVisualInfo.UseQuadVisuals == false) { return; }
        if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.FLegsPrefab) == false) {
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.FLegsPrefab);
        }
        if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyPrefab) == false) {
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyPrefab);
        }
        if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyShaderSource) == false) {
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyShaderSource);
        }
      }
    }
    public static void AddCustomDeps(this VehicleChassisDef chassis, LoadRequest loadRequest) {
      UnitCustomInfo info = chassis.GetCustomInfo();
      if (info != null) {
        foreach (CustomPart part in info.CustomParts) {
          if (string.IsNullOrEmpty(part.prefab)) { continue; }
          Log.LogWrite(1, "additional prefab:" + part.prefab, true);
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, part.prefab, new bool?(false));
          foreach (var mi in part.MaterialInfo) {
            if (string.IsNullOrEmpty(mi.Value.shader) == false) {
              Log.LogWrite(1, "additional shader:" + mi.Value.shader, true);
              loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mi.Value.shader, new bool?(false));
            }
            foreach (var ti in mi.Value.materialTextures) {
              if (string.IsNullOrEmpty(ti.Value) == false) {
                Log.LogWrite(1, "additional textures:" + ti.Value, true);
                loadRequest.AddBlindLoadRequest(BattleTechResourceType.Texture2D, ti.Value, new bool?(false));
              }
            }
          }
        }
      }
    }
    public static void Postfix(VehicleChassisDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      Log.LogWrite(0, "VehicleChassisDef.GatherDependencies postfix " + __instance.Description.Id, true);
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) {
          Log.LogWrite(1, "no custom", true);
          return;
        }
        info.AddCustomDeps(dependencyLoad);
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class VehicleChassisDef_DependenciesLoaded {
    public static bool CheckCustomDeps(this UnitCustomInfo info, DataManager dataManager) {
      foreach (CustomPart part in info.CustomParts) {
        if (string.IsNullOrEmpty(part.prefab)) { continue; }
        Log.LogWrite(1, "additional prefab:" + part.prefab, false);
        if (dataManager.Exists(BattleTechResourceType.Prefab, part.prefab) == false) {
          Log.LogWrite(1, " not exists", true);
          return false;
        }
        foreach (var mi in part.MaterialInfo) {
          if (string.IsNullOrEmpty(mi.Value.shader)) { continue; };
          Log.LogWrite(1, "additional shader:" + mi.Value.shader, false);
          if (dataManager.Exists(BattleTechResourceType.Prefab, mi.Value.shader) == false) {
            Log.LogWrite(2, "not exists", true);
            return false;
          }
          Log.LogWrite(2, "exists", true);
          foreach (var ti in mi.Value.materialTextures) {
            if (string.IsNullOrEmpty(ti.Value) == false) {
              Log.LogWrite(1, "additional texture:" + ti.Value, true);
              if (dataManager.Exists(BattleTechResourceType.Texture2D, ti.Value) == false) {
                Log.LogWrite(2, "not exists", true);
                return false;
              }
            }
          }
          Log.LogWrite(2, "exists", true);
        }
      }
      Log.LogWrite(1, "additional melee def:" + info.MeleeWeaponOverride.DefaultWeapon, false);
      if (dataManager.Exists(BattleTechResourceType.WeaponDef, info.MeleeWeaponOverride.DefaultWeapon) == false) { Log.LogWrite(2, "not exists", true); return false; }
      Log.LogWrite(2, "exists", true);
      foreach (var cm in info.MeleeWeaponOverride.Components) {
        Log.LogWrite(1, "additional melee def:" + cm.Key, true);
        if (dataManager.Exists(BattleTechResourceType.WeaponDef, cm.Key) == false) { Log.LogWrite(2, "not exists", true); return false; };
        Log.LogWrite(2, "exists", true);
      }
      return true;
    }
    public static void Postfix(VehicleChassisDef __instance, uint loadWeight, ref bool __result) {
      Log.LogWrite(0, "VehicleChassisDef.DependenciesLoaded postfix " + __instance.Description.Id, true);
      if (__instance.DataManager == null) { return; }
      if (__result == false) { return; }
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) {
          Log.LogWrite(1, "no custom", true);
          return;
        }
        if (info.CheckCustomDeps(__instance.DataManager) == false) {
          __result = false;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class ChassisDef_GatherDependencies {
    public static void AddQuadDeps(this ChassisDef chassis, DataManager.DependencyLoadRequest dependencyLoad) {
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(chassis.PrefabIdentifier);
      if (custRepDef == null) { return; }
      if (custRepDef.quadVisualInfo.UseQuadVisuals == false) { return; }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.FLegsPrefab) == false) {
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.FLegsPrefab);
      }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyPrefab) == false) {
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyPrefab);
      }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyShaderSource) == false) {
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyShaderSource);
      }
    }
    public static void AddAlternateDeps(this UnitCustomInfo info, DataManager.DependencyLoadRequest dependencyLoad) {
      foreach (var altRep in info.AlternateRepresentations) {
        foreach (string addPrefab in altRep.AdditionalPrefabs) {
          if (string.IsNullOrEmpty(addPrefab)) { continue; }
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, addPrefab);
        }
        foreach (AirMechVerticalJetsDef vJetDef in altRep.AirMechVerticalJets) {
          if (string.IsNullOrEmpty(vJetDef.Prefab)) { continue; }
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, vJetDef.Prefab);
        }
        if (string.IsNullOrEmpty(altRep.HardpointDataDef) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.HardpointDataDef, altRep.HardpointDataDef);
        }
        if (string.IsNullOrEmpty(altRep.PrefabIdentifier)) { continue; }
        dependencyLoad.RequestResource(BattleTechResourceType.Prefab, altRep.PrefabIdentifier);
      }
    }
    public static void AddCustomRepDeps(this ChassisDef __instance, DataManager.DependencyLoadRequest dependencyLoad) {
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(__instance.PrefabIdentifier);
      if (custRepDef != null) {
        if (string.IsNullOrEmpty(custRepDef.SourcePrefabIdentifier) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.SourcePrefabIdentifier);
        }
        if (string.IsNullOrEmpty(custRepDef.ShaderSource) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.ShaderSource);
        }
        if (string.IsNullOrEmpty(custRepDef.BlipSource) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, custRepDef.BlipSource);
        }
      }
    }
    public static void Postfix(ChassisDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      Log.LogWrite(0, "ChassisDef.GatherDependencies postfix " + activeRequestWeight + " " + __instance.Description.Id, true);
      try {
        __instance.AddCustomRepDeps(dependencyLoad);
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) {
          Log.LogWrite(1, "no custom", true);
          return;
        }
        info.AddCustomDeps(dependencyLoad);
        __instance.AddQuadDeps(dependencyLoad);
        info.AddAlternateDeps(dependencyLoad);
        if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsPrefabSrc) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, Core.Settings.CustomJumpJetsPrefabSrc);
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class MechDef_GatherDependencies {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(MechDef), "RequestInventoryPrefabs");
      var replacementMethod = AccessTools.Method(typeof(MechDef_RefreshInventory), "RequestInventoryPrefabs");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    public static void Postfix(MechDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      Log.LogWrite(0, "MechDef.GatherDependencies postfix " + __instance.Description.Id + " " + activeRequestWeight, true);
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class MechDef_DependenciesLoaded {
    public static string GetNoDependenciesLoadedReason(this MechDef __instance, uint loadWeight) {
      if (__instance.DataManager == null) { return "no data manager"; }
      if (__instance.DataManager.ChassisDefs.Exists(__instance.ChassisID) == false) { return "chassis " + __instance.ChassisID + " not exists"; }
      if ((string.IsNullOrEmpty(__instance.HeraldryID) == false) && (__instance.HeraldryDef == null)) { return "HeraldryID " + __instance.HeraldryID + " is null"; }
      if ((string.IsNullOrEmpty(__instance.HeraldryID) == false) && (__instance.HeraldryDef != null) && (__instance.HeraldryDef.DependenciesLoaded(loadWeight) == false)) { return "HeraldryID " + __instance.HeraldryID + " has unresolved dependencies"; }
      for (int index = 0; index < __instance.inventory().Length; ++index) {
        DataManager.ILoadDependencies loadDependencies = (DataManager.ILoadDependencies)__instance.inventory()[index];
        if (loadDependencies != null) {
          loadDependencies.DataManager = __instance.DataManager;
          if (loadDependencies.DependenciesLoaded(loadWeight) == false) {
            return "component " + __instance.inventory()[index].ComponentDefID + " has unresolved dependencies";
          }
        }
      }
      if (__instance.meleeWeaponRef.DependenciesLoaded(loadWeight) == false) { return "component " + __instance.meleeWeaponRef.ComponentDefID + " has unresolved dependencies"; }
      if (__instance.dfaWeaponRef.DependenciesLoaded(loadWeight) == false) { return "component " + __instance.dfaWeaponRef.ComponentDefID + " has unresolved dependencies"; }
      if (__instance.Chassis == null) { return "no chassis"; }
      if (__instance.Chassis.DependenciesLoaded(loadWeight) == false) { return "chassis " + __instance.ChassisID + " has unresolved dependencies"; }
      if (__instance.Chassis.DependenciesLoaded(loadWeight) == false) { return "chassis " + __instance.ChassisID + " has unresolved dependencies"; }
      if (__instance.Chassis.HardpointDataDef == null) { return "chassis " + __instance.ChassisID + " has no hardpoints def " + __instance.Chassis.HardpointDataDefID; }
      HashSet<MechComponentRef> inventory = new HashSet<MechComponentRef>();
      foreach(MechComponentRef compRef in __instance.inventory()) {
        inventory.Add(compRef);
      }
      inventory.Add(__instance.meleeWeaponRef);
      inventory.Add(__instance.dfaWeaponRef);
      inventory.Add(__instance.imaginaryLaserWeaponRef);
      foreach(MechComponentRef compRef in inventory) {
        if (compRef.Def == null) { return compRef.ComponentDefID + " has no def"; }
        if (loadWeight <= 10U) { continue; }
        if (compRef.hasPrefabName) {
          if(string.IsNullOrEmpty(compRef.prefabName) == false) {
            if(__instance.DataManager.Exists(BattleTechResourceType.Prefab, compRef.prefabName) == false) {
              return compRef.ComponentDefID + " has no prefab "+ compRef.prefabName;
            }
          }
        }
      }
      return "unknown";
    }
    public static void Postfix(MechDef __instance, uint loadWeight, ref bool __result) {
      if (__result == true) {
        return;
      }
      string reason = __instance.GetNoDependenciesLoadedReason(loadWeight);
      Log.TWL(0, "MechDef.DependenciesLoaded " + __instance.Description.Id + " fail reason:" + reason);
    }
  }
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("BeginRequestResources")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Contract_BeginRequestResources {
    public static void Prefix(Contract __instance) {
      Log.TWL(0, "Contract.BeginRequestResources");
      foreach (var lance in __instance.Lances.Lances) {
        Log.WL(1, "lance:" + lance.Key);
        foreach (var lanceUnit in lance.Value) {
          Log.WL(2, "unit:" + lanceUnit.PilotId + " " + lanceUnit.UnitId + " " + lanceUnit.unitType);
        }
      }
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class ChassisDef_DependenciesLoaded {
    public static bool CheckQuadDeps(this ChassisDef chassis, DataManager dataManager) {
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(chassis.PrefabIdentifier);
      if (custRepDef == null) { return true; }
      if (custRepDef.quadVisualInfo.UseQuadVisuals == false) { return true; }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.FLegsPrefab) == false) {
        if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.FLegsPrefab) == false) { return false; }
      }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyPrefab) == false) {
        if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyPrefab) == false) { return false; }
      }
      if (string.IsNullOrEmpty(custRepDef.quadVisualInfo.BodyShaderSource) == false) {
        if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.quadVisualInfo.BodyShaderSource) == false) { return false; }
      }
      return true;
    }
    public static bool CheckAltDeps(this UnitCustomInfo info, DataManager dataManager) {
      foreach (var altRep in info.AlternateRepresentations) {
        foreach (string addPrefab in altRep.AdditionalPrefabs) {
          if (string.IsNullOrEmpty(addPrefab)) { continue; }
          if (dataManager.Exists(BattleTechResourceType.Prefab, addPrefab) == false) { return false; }
        }
        foreach (AirMechVerticalJetsDef vJetDef in altRep.AirMechVerticalJets) {
          if (string.IsNullOrEmpty(vJetDef.Prefab)) { continue; }
          if (dataManager.Exists(BattleTechResourceType.Prefab, vJetDef.Prefab) == false) { return false; }
        }
        if (string.IsNullOrEmpty(altRep.HardpointDataDef) == false) {
          if (dataManager.Exists(BattleTechResourceType.HardpointDataDef, altRep.HardpointDataDef) == false) { return false; }
        }
        if (string.IsNullOrEmpty(altRep.PrefabIdentifier)) { continue; }
        if (dataManager.Exists(BattleTechResourceType.Prefab, altRep.PrefabIdentifier) == false) { return false; }
      }
      return true;
    }
    public static bool CheckCustomRepDeps(this ChassisDef __instance, DataManager dataManager) {
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(__instance.PrefabIdentifier);
      if (custRepDef != null) {
        if (string.IsNullOrEmpty(custRepDef.SourcePrefabIdentifier) == false) {
          if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.SourcePrefabIdentifier) == false) { return false; }
        }
        if (string.IsNullOrEmpty(custRepDef.ShaderSource) == false) {
          if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.ShaderSource) == false) { return false; }
        }
        if (string.IsNullOrEmpty(custRepDef.BlipSource) == false) {
          if (dataManager.Exists(BattleTechResourceType.Prefab, custRepDef.BlipSource) == false) { return false; }
        }
      }
      return true;
    }
    public static void Postfix(ChassisDef __instance, uint loadWeight, ref bool __result) {
      Log.LogWrite(0, "ChassisDef.DependenciesLoaded postfix " + loadWeight + " " + __instance.Description.Id, true);
      if (__instance.DataManager == null) { return; }
      if (__result == false) { return; }
      try {
        if (__instance.CheckCustomRepDeps(__instance.DataManager) == false) {
          __result = false;
        }
        if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsPrefabSrc) == false) {
          if (__instance.DataManager.Exists(BattleTechResourceType.Prefab, Core.Settings.CustomJumpJetsPrefabSrc) == false) {
            __result = false;
          }
        }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) {
          Log.LogWrite(1, "no custom", true);
          return;
        }
        if (info.CheckCustomDeps(__instance.DataManager) == false) {
          __result = false;
        }
        if (__instance.CheckQuadDeps(__instance.DataManager) == false) {
          __result = false;
        }
        if (info.CheckAltDeps(__instance.DataManager) == false) {
          __result = false;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return;
    }
  }
}