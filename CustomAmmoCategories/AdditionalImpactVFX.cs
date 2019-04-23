using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, List<ObjectSpawnDataSelf>> additinalImpactEffects = new Dictionary<string, List<ObjectSpawnDataSelf>>();
    public static string getCACGUID(this Weapon weapon) {
      string wGUID;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.GUIDStatisticName) == false) {
        wGUID = Guid.NewGuid().ToString();
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.GUIDStatisticName, wGUID);
      } else {
        wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      }
      return wGUID;
    }
    public static void clearImpactVFX(this Weapon weapon) {
      string wGUID = weapon.getCACGUID();
      if (CustomAmmoCategories.additinalImpactEffects.ContainsKey(wGUID)) {
        List<ObjectSpawnDataSelf> vfxs = CustomAmmoCategories.additinalImpactEffects[wGUID];
        foreach(var vfx in vfxs) {
          vfx.CleanupSelf();
        }
        CustomAmmoCategories.additinalImpactEffects.Remove(wGUID);
      }
    }
    public static string AdditionalImpactEffect(this Weapon weapon, out Vector3 scale) {
      ExtWeaponDef extWeapon = weapon.exDef();
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          if (string.IsNullOrEmpty(mode.AdditionalImpactVFX) == false) {
            scale = new Vector3(mode.AdditionalImpactVFXScaleX, mode.AdditionalImpactVFXScaleY, mode.AdditionalImpactVFXScaleZ);
            return mode.AdditionalImpactVFX;
          }
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        if (string.IsNullOrEmpty(extAmmoDef.AdditionalImpactVFX) == false) {
          scale = new Vector3(extAmmoDef.AdditionalImpactVFXScaleX, extAmmoDef.AdditionalImpactVFXScaleY, extAmmoDef.AdditionalImpactVFXScaleZ);
          return extAmmoDef.AdditionalImpactVFX;
        }
      }
      scale = new Vector3(extWeapon.AdditionalImpactVFXScaleX, extWeapon.AdditionalImpactVFXScaleY, extWeapon.AdditionalImpactVFXScaleZ);
      return extWeapon.AdditionalImpactVFX;
    }
    public static void SpawnAdditionalImpactEffect(this Weapon weapon,Vector3 pos) {
      Vector3 scale;
      string VFXprefab = weapon.AdditionalImpactEffect(out scale);
      if (string.IsNullOrEmpty(VFXprefab)) { return; }
      string wGUID = weapon.getCACGUID();
      List<ObjectSpawnDataSelf> effects = null;
      if (CustomAmmoCategories.additinalImpactEffects.ContainsKey(wGUID)) {
        effects = CustomAmmoCategories.additinalImpactEffects[wGUID];
      } else {
        effects = new List<ObjectSpawnDataSelf>();
        CustomAmmoCategories.additinalImpactEffects.Add(wGUID, effects);
      }
      CustomAmmoCategoriesLog.Log.LogWrite("SpawnAdditionalImpactEffect:"+VFXprefab+" "+scale+"\n");
      ObjectSpawnDataSelf vfx = new ObjectSpawnDataSelf(VFXprefab, pos, Quaternion.identity, scale, true, false);
      vfx.SpawnSelf(weapon.parent.Combat);
      effects.Add(vfx);
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedAoE {
    public static bool Prefix(CombatGameState __instance) {
      foreach(var ae in CustomAmmoCategories.additinalImpactEffects) {
        foreach(var sp in ae.Value) {
          try { sp.CleanupSelf(); } finally { };
        }
      }
      CustomAmmoCategories.additinalImpactEffects.Clear();
      return true;
    }
  }
}

