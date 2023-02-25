/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
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
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;
using CustomAmmoCategoriesPatches;
using System.Threading;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(ApplicationConstants))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class ApplicationConstants_FromJSON {
    public static void Postfix(ApplicationConstants __instance) {
      Log.M.TWL(0, "ApplicationConstants.FromJSON. PrewarmRequests:");
      try {
        List<PrewarmRequest> prewarmRequests = new List<PrewarmRequest>();
        prewarmRequests.AddRange(__instance.PrewarmRequests);
        HashSet<PrewarmRequest> toDel = new HashSet<PrewarmRequest>();
        HashSet<string> uiIcons = CustomAmmoCategories.Settings.uiIcons.ToHashSet<string>();
        foreach (var preq in prewarmRequests) {
          if (preq.ResourceType == BattleTechResourceType.AmmunitionDef) { toDel.Add(preq); } else
          if (preq.ResourceType == BattleTechResourceType.AmmunitionBoxDef) { toDel.Add(preq); } else
          if (preq.ResourceType == BattleTechResourceType.WeaponDef) { toDel.Add(preq); }else
          if ((preq.ResourceType == BattleTechResourceType.SVGAsset)&&(uiIcons.Contains(preq.ResourceID))) { toDel.Add(preq); } 
        }
        foreach (var preq in toDel) { prewarmRequests.Remove(preq); };
        if (CustomPrewarm.Core.Settings.UseFastPreloading == false) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.AmmunitionDef, PrewarmRequest.PREWARM_ALL_OF_TYPE));
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.AmmunitionBoxDef, PrewarmRequest.PREWARM_ALL_OF_TYPE));
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.WeaponDef, PrewarmRequest.PREWARM_ALL_OF_TYPE));
        }
        foreach(string iconid in uiIcons) {
          prewarmRequests.Add(new PrewarmRequest(BattleTechResourceType.SVGAsset, iconid));
          CustomSvgCache.RegisterSVG(iconid);
        }
        typeof(ApplicationConstants).GetProperty("PrewarmRequests", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[] { prewarmRequests.ToArray() });
        foreach (PrewarmRequest preq in __instance.PrewarmRequests) {
          Log.M.WL(1, preq.ResourceType + ":" + preq.ResourceID);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(typeof(MechComponentDef))]
  //[HarmonyPatch("DependenciesLoaded")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(uint) })]
  //public static class MechComponentDef_DependenciesLoaded {
  //  public static void Postfix(MechComponentDef __instance, uint loadWeight, bool __result) {
  //    try {
  //      //Log.M.TWL(0, "MechComponentDef.DependenciesLoaded(" + loadWeight + "):" + __instance.Description.Id + " result:" + __result);
  //    } catch (Exception e) {
  //      Log.M.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class WeaponDef_DependenciesLoaded {
    private static Func<WeaponDef, uint, bool> MechComponentDef_DependenciesLoaded = null;
    public static bool Prepare() {
      try {
        var method = typeof(MechComponentDef).GetMethod("DependenciesLoaded", BindingFlags.Instance | BindingFlags.Public);
        if (method == null) {
          Log.M.TWL(0, "WeaponDef.DependenciesLoaded: can't find MechComponentDef.DependenciesLoaded", true);
          return false;
        }
        var dm = new DynamicMethod("CACMechComponentDefDependenciesLoaded", typeof(bool), new Type[] { typeof(WeaponDef), typeof(uint) }, typeof(WeaponDef));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        MechComponentDef_DependenciesLoaded = (Func<WeaponDef, uint, bool>)dm.CreateDelegate(typeof(Func<WeaponDef, uint, bool>));
      } catch (Exception e) {
        Log.M.TWL(0, "WeaponDef.DependenciesLoaded prepare:\n" + e.ToString(), true);
        return false;
      }
      return true;
    }
    public static bool Prefix(WeaponDef __instance, uint loadWeight, ref bool __result, DataManager ___dataManager) {
      Log.M.TW(0, "WeaponDef.DependenciesLoaded(" + loadWeight + ")" + __instance.Description.Id);
      try {
        if (MechComponentDef_DependenciesLoaded(__instance, loadWeight) == false) { __result = false; return false; }
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.Description.Id);
        if (loadWeight >= 1000U) {
          Log.M.WL(1, "Checking WeaponEffectIDs");
          HashSet<string> weaponEffects = new HashSet<string>();
          weaponEffects.Add(__instance.WeaponEffectID);
          foreach (var mode in extWeaponDef.Modes) {
            if (string.IsNullOrEmpty(mode.Value.WeaponEffectID)) { continue; }
            if (weaponEffects.Contains(mode.Value.WeaponEffectID)) { continue; }
            weaponEffects.Add(mode.Value.WeaponEffectID);
          }
          foreach (string WeaponEffectId in weaponEffects) {
            Log.M.W(2, WeaponEffectId);
            if (___dataManager.Exists(BattleTechResourceType.Prefab, WeaponEffectId) == false) { __result = false; Log.M.WL(1,"abcent"); return false; }
            Log.M.WL(1, "present");
          }
        }
        HashSet<string> ammoTypes = new HashSet<string>();
        if (__instance.AmmoCategoryValue.Is_NotSet == false) { ammoTypes.Add(__instance.AmmoCategoryValue.Name); }
        foreach (var mode in extWeaponDef.Modes) {
          if (mode.Value.AmmoCategory.BaseCategory.Is_NotSet) { continue; }
          if (string.IsNullOrEmpty(mode.Value.AmmoCategory.BaseCategory.Name)) { continue; }
          if (ammoTypes.Contains(mode.Value.AmmoCategory.BaseCategory.Name)) { continue; }
          ammoTypes.Add(mode.Value.AmmoCategory.BaseCategory.Name);
        }
        Log.M.WL(1, "Checking ammunitions and boxes");
        foreach (string ammoType in ammoTypes) {
          HashSet<string> ammoIds = BattleTech_AmmunitionDef_fromJSON_Patch.ammunitions(ammoType);
          foreach (string ammoId in ammoIds) {
            if (string.IsNullOrEmpty(ammoId)) { continue; }
            Log.M.W(2, "ammo:"+ammoId);
            if (___dataManager.AmmoDefs.Exists(ammoId) == false) { __result = false; Log.M.WL(1, "abcent"); return false; }
            HashSet<string> ammoBoxIds = AmmunitionBoxDef_FromJSON.ammoBoxesForAmmoId(ammoId);
            Log.M.WL(1, "present");
            foreach (string ammoBoxId in ammoBoxIds) {
              if (string.IsNullOrEmpty(ammoBoxId)) { continue; }
              Log.M.W(3, "ammoBox:" + ammoBoxId);
              if (___dataManager.AmmoBoxDefs.Exists(ammoBoxId) == false) { __result = false; Log.M.WL(1, "abcent"); return false; }
              Log.M.WL(1, "present");
            }
          }
        }
        Log.M.WL(1, "Internal ammo");
        foreach(var intAmmo in extWeaponDef.InternalAmmo) {
          Log.M.W(2, intAmmo.Key);
          if(___dataManager.AmmoDefs.Exists(intAmmo.Key) == false) { __result = false; Log.M.WL(1, "abcent"); return false; }
        }
        __result = true;
        Log.M.WL(1, "result:"+__result.ToString());
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(WeaponDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class WeaponDef_GatherDependencies {
    public static void AmmoGatherDependencies(string ammoId, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      Log.M.WL(1, ammoId+ ".GatherDependencies");
      ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(ammoId);
      if (activeRequestWeight >= 10U) {
        if (string.IsNullOrEmpty(ammo.WeaponEffectID) == false) {
          Log.M.WL(2, "WeaponEffectIDs:"+ ammo.WeaponEffectID);
          if (dataManager.Exists(BattleTechResourceType.Prefab, ammo.WeaponEffectID) == false) { dependencyLoad.RequestResource(BattleTechResourceType.Prefab, ammo.WeaponEffectID); }
        }
      }
      for (int index = 0; index < ammo.statusEffects.Length; ++index) {
        if (!string.IsNullOrEmpty(ammo.statusEffects[index].Description.Icon)) {
          Log.M.WL(2, "ammo effect icon:" + ammo.statusEffects[index].Description.Icon);
          dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, ammo.statusEffects[index].Description.Icon);
        }
      }
    }
    public static bool Prefix(WeaponDef __instance,ref DataManager ___dataManager, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      Log.M.TW(0, "WeaponDef.GatherDependencies(" + activeRequestWeight + ")" + __instance.Description.Id);
      ___dataManager = dataManager;
      try {
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.Description.Id);
        if (activeRequestWeight >= 10U) {
          Log.M.WL(1, "gathering WeaponEffectIDs");
          HashSet<string> weaponEffects = new HashSet<string>();
          weaponEffects.Add(__instance.WeaponEffectID);
          foreach (var mode in extWeaponDef.Modes) {
            if (string.IsNullOrEmpty(mode.Value.WeaponEffectID)) { continue; }
            if (weaponEffects.Contains(mode.Value.WeaponEffectID)) { continue; }
            weaponEffects.Add(mode.Value.WeaponEffectID);
          }
          foreach (string WeaponEffectId in weaponEffects) {
            Log.M.WL(2, WeaponEffectId);
            if (dataManager.Exists(BattleTechResourceType.Prefab, WeaponEffectId) == false) { dependencyLoad.RequestResource(BattleTechResourceType.Prefab, WeaponEffectId); }
          }
        }
        HashSet<string> ammoTypes = new HashSet<string>();
        if (__instance.AmmoCategoryValue.Is_NotSet == false) { ammoTypes.Add(__instance.AmmoCategoryValue.Name); }
        foreach (var mode in extWeaponDef.Modes) {
          if (mode.Value.AmmoCategory.BaseCategory.Is_NotSet) { continue; }
          if (string.IsNullOrEmpty(mode.Value.AmmoCategory.BaseCategory.Name)) { continue; }
          if (ammoTypes.Contains(mode.Value.AmmoCategory.BaseCategory.Name)) { continue; }
          ammoTypes.Add(mode.Value.AmmoCategory.BaseCategory.Name);
        }
        Log.M.WL(1, "gathering ammunitions and boxes");
        foreach (string ammoType in ammoTypes) {
          HashSet<string> ammoIds = BattleTech_AmmunitionDef_fromJSON_Patch.ammunitions(ammoType);
          if (ammoIds.Count == 0) { ammoIds.Add(__instance.AmmoCategoryToAmmoId); }
          foreach (string ammoId in ammoIds) {
            if (string.IsNullOrEmpty(ammoId)) { continue; }
            Log.M.WL(2, "ammo:" + ammoId);
            if (dataManager.Exists(BattleTechResourceType.AmmunitionDef, ammoId) == false) { dependencyLoad.RequestResource(BattleTechResourceType.AmmunitionDef, ammoId); }
            AmmoGatherDependencies(ammoId, dataManager, dependencyLoad, activeRequestWeight);
            HashSet<string> ammoBoxIds = AmmunitionBoxDef_FromJSON.ammoBoxesForAmmoId(ammoId);
            if (ammoBoxIds.Count == 0) {
              AmmoCategoryValue ammoVal = CustomAmmoCategories.findExtAmmo(ammoId).AmmoCategory.BaseCategory;
              if (ammoVal.Is_NotSet) {
                ammoVal = AmmoCategoryEnumeration.GetAmmoCategoryByName(ammoType);
              }
              if (ammoVal.Is_NotSet == false) {
                if (ammoVal.UsesInternalAmmo == false) { ammoBoxIds.Add("Ammo_AmmunitionBox_Generic_"+ ammoVal.Name); }
              }
            }
            foreach (string ammoBoxId in ammoBoxIds) {
              if (string.IsNullOrEmpty(ammoBoxId)) { continue; }
              Log.M.WL(3, "ammoBox:" + ammoBoxId);
              if (dataManager.Exists(BattleTechResourceType.AmmunitionBoxDef, ammoBoxId) == false) {
                if (dataManager.ResourceLocator.EntryByID(ammoBoxId, BattleTechResourceType.AmmunitionBoxDef) != null) {
                  dependencyLoad.RequestResource(BattleTechResourceType.AmmunitionBoxDef, ammoBoxId);
                }
              }
            }
          }
        }
        Log.M.WL(1, "gathering icons");
        if (!string.IsNullOrEmpty(__instance.Description.Icon)) {
          Log.M.WL(2, "icon:" + __instance.Description.Icon);
          dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, __instance.Description.Icon);
        }
        for (int index = 0; index < __instance.statusEffects.Length; ++index) {
          if (!string.IsNullOrEmpty(__instance.statusEffects[index].Description.Icon)) {
            Log.M.WL(2, "self effect icon:" + __instance.statusEffects[index].Description.Icon);
            dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, __instance.statusEffects[index].Description.Icon);
          }
        }
        foreach(var mode in extWeaponDef.Modes) {
          for (int index = 0; index < mode.Value.statusEffects.Count; ++index) {
            if (!string.IsNullOrEmpty(mode.Value.statusEffects[index].Description.Icon)) {
              Log.M.WL(2, "mode "+mode.Value+" effect icon:" + mode.Value.statusEffects[index].Description.Icon);
              dependencyLoad.RequestResource(BattleTechResourceType.SVGAsset, mode.Value.statusEffects[index].Description.Icon);
            }
          }
        }
        Log.M.WL(1, "Internal ammo");
        foreach(var intAmmo in extWeaponDef.InternalAmmo) {
          Log.M.W(2, intAmmo.Key);
          if(dataManager.Exists(BattleTechResourceType.AmmunitionDef, intAmmo.Key) == false) { dependencyLoad.RequestResource(BattleTechResourceType.AmmunitionDef, intAmmo.Key); }
        }
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBoxDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AmmunitionBoxDef_FromJSON {
    private static SpinLock spinLock = new SpinLock();
    private static Dictionary<string, HashSet<string>> ammoBoxes = new Dictionary<string, HashSet<string>>();
    public static HashSet<string> ammoBoxesForAmmoId(string AmmoId) {
      if (ammoBoxes.ContainsKey(AmmoId)) { return ammoBoxes[AmmoId]; }
      return new HashSet<string>();
    }
    public static void Postfix(AmmunitionBoxDef __instance) {
      Log.M.TWL(0, "AmmunitionBoxDef.FromJSON:" + __instance.Description.Id);
      try {
        if (string.IsNullOrEmpty(__instance.AmmoID) == false) {
          bool locked = false;
          try {
            spinLock.Enter(ref locked);
            if (ammoBoxes.ContainsKey(__instance.AmmoID) == false) { ammoBoxes.Add(__instance.AmmoID, new HashSet<string>()); }
            if (ammoBoxes[__instance.AmmoID].Contains(__instance.Description.Id) == false) { ammoBoxes[__instance.AmmoID].Add(__instance.Description.Id); }
          } catch (Exception e) {
            if (locked) { spinLock.Exit(); locked = false; }
            throw e;
          }
          if (locked) { spinLock.Exit(); locked = false; }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}