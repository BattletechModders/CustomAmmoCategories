using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using CustomAmmoCategoriesLog;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public static class TempAmmoCountHelper {
    private static Dictionary<Weapon, int> tempWeaponAmmoCount = new Dictionary<Weapon, int>();
    private static Dictionary<AmmunitionBox, int> tempAmmoBoxAmmoCount = new Dictionary<AmmunitionBox, int>();
    public static int tInternalAmmo(this Weapon weapon) {
      if (tempWeaponAmmoCount.ContainsKey(weapon) == false) { tempWeaponAmmoCount.Add(weapon, weapon.InternalAmmo); }
      return tempWeaponAmmoCount[weapon];
    }
    public static int tCurrentAmmo(this AmmunitionBox box) {
      if (tempAmmoBoxAmmoCount.ContainsKey(box) == false) { tempAmmoBoxAmmoCount.Add(box, box.CurrentAmmo); }
      return tempAmmoBoxAmmoCount[box];
    }
    public static void tInternalAmmo(this Weapon weapon, int count) {
      if (tempWeaponAmmoCount.ContainsKey(weapon) == false) { tempWeaponAmmoCount.Add(weapon, count); }
      tempWeaponAmmoCount[weapon] = count;
    }
    public static void tCurrentAmmo(this AmmunitionBox box, int count) {
      if (tempAmmoBoxAmmoCount.ContainsKey(box) == false) { tempAmmoBoxAmmoCount.Add(box, box.CurrentAmmo); }
      tempAmmoBoxAmmoCount[box] = count;
    }
    public static void ResetTempAmmo(this Weapon weapon) {
      weapon.tInternalAmmo(weapon.InternalAmmo);
      foreach(AmmunitionBox box in weapon.ammoBoxes) { box.tCurrentAmmo(box.CurrentAmmo); };
    }
    public static void Clear() {
      tempWeaponAmmoCount.Clear();
      tempAmmoBoxAmmoCount.Clear();
    }
    public static int ShotsToHits(this Weapon weapon, int shoots) {
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (weapon.DisabledClustering() == false)) {
        shoots *= weapon.ProjectilesPerShot;
      }
      return shoots;
    }
    public static int HitsToShots(this Weapon weapon, int hits) {
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (weapon.DisabledClustering() == false)) {
        hits /= weapon.ProjectilesPerShot;
      }
      return hits;
    }
    public static int ShootsToAmmo(this Weapon weapon, int shoots) {
      return Mathf.RoundToInt((float)shoots / weapon.ShotsPerAmmo());
    }
    public static int AmmoToShoots(this Weapon weapon, int ammo) {
      return Mathf.RoundToInt((float)ammo * weapon.ShotsPerAmmo()); ;
    }
  }
  public static partial class CustomAmmoCategories {
    public static bool DisabledClustering(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (weapon.HasShells() == true) { return true; };
      if (weapon.DamagePerPallet() == true) { return false; }
      return extWeapon.DisableClustering == TripleBoolean.True;
    }
    public static bool isStreak(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.Streak != TripleBoolean.NotSet) { return mode.Streak == TripleBoolean.True; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.Streak != TripleBoolean.NotSet) { return mode.Streak == TripleBoolean.True; };
      return weapon.exDef().StreakEffect;
    }
    public static void ReturnNoFireHeat(Weapon weapon, int stackItemUID, int numNeedShots, int numSuccesHits) {
      CustomAmmoCategoriesLog.Log.LogWrite("Returining heat\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Needed shots " + numNeedShots + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Success shots " + numSuccesHits + "\n");
      float returnHeat = 0.0f - ((float)(numNeedShots - numSuccesHits) * weapon.HeatGenerated) / (float)numNeedShots;
      CustomAmmoCategoriesLog.Log.LogWrite(" Heat to return " + (int)returnHeat + "\n");
      weapon.parent.AddWeaponHeat(weapon, (int)returnHeat);
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("DecrementAmmo")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(int) })]
    public static class Weapon_DecrementAmmo {
      public static bool Prefix(Weapon __instance, int stackItemUID, ref int __result) {
        Log.M.TW(0, "Weapon.DecrementAmmo:" + __instance.defId);
        __result = __instance.CountAmmoForShot(stackItemUID);
        Log.M.W(1, "shots:" + __result);
        __result = __instance.ShotsToHits(__result);
        Log.M.WL(1, "hits:" + __result);
        return false;
      }
    }
    public static int CountAmmoForShot(this Weapon weapon, int stackItemUID) {
      Log.M.TWL(0, "Weapon.CountAmmoForShot:" + weapon.defId);
      int ammoWhenFired =  weapon.ShootsToAmmo(weapon.ShotsWhenFired);
      Log.M.WL(1, "ammoWhenFired:"+ammoWhenFired);
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return weapon.AmmoToShoots(ammoWhenFired); };
      if (weapon.parent != null) {
        Turret turret = weapon.parent as Turret;
        if (turret != null) {
          if (turret.TurretDef.Description.Id.Contains("Assault")) {
            Log.M.WL(1, "hardened turret detected");
            return weapon.AmmoToShoots(ammoWhenFired); ;
          }
        }
      }
      int modValue = 0;
      if (weapon.tInternalAmmo() >= ammoWhenFired) {
        weapon.tInternalAmmo(weapon.tInternalAmmo() - ammoWhenFired);
        return weapon.AmmoToShoots(ammoWhenFired);
      } else {
        modValue = ammoWhenFired - weapon.tInternalAmmo();
        weapon.tInternalAmmo(0);
      }
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (modValue == 0) { break; }
        AmmunitionBox ammoBox = weapon.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.tCurrentAmmo() <= 0) { continue; }
        if (ammoBox.ammoDef.Description.Id != ammo.Id) { continue; }
        if (ammoBox.tCurrentAmmo() >= modValue) {
          ammoBox.tCurrentAmmo(ammoBox.tCurrentAmmo() - modValue); modValue = 0; break;
        } else {
          modValue -= ammoBox.tCurrentAmmo();
          ammoBox.tCurrentAmmo(0);
        }
      }
      return weapon.AmmoToShoots(ammoWhenFired - modValue);
    }
    public static void RealDecrementAmmo(this Weapon weapon, int stackItemUID, int realShootsUsed) {
      Log.M.TWL(0,"RealDecrementAmmo:" + weapon.defId);
      int ammoWhenFired = weapon.ShootsToAmmo(realShootsUsed);
      CustomAmmoCategory ammoCategory = weapon.CustomAmmoCategory();
      if (ammoCategory.BaseCategory.Is_NotSet) { Log.M.WL(1, "not using ammo"); return; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.Id != ammoCategory.Id) {
        ammo = ammoCategory.defaultAmmo();
      };
      if(ammo.AmmoCategory.BaseCategory.Is_NotSet) { Log.M.WL(1, "very strange behavior this ammo category have no ammo definitions"); return; };
      if (weapon.parent != null) {
        Turret turret = weapon.parent as Turret;
        if (turret != null) {
          if (turret.TurretDef.Description.Id.Contains("Assault")) {
            Log.M.WL(1,"Hardened turret detected"); return;
          }
        }
      }
      int modValue = 0;
      if (weapon.InternalAmmo >= ammoWhenFired) {
        weapon.DecInternalAmmo(stackItemUID, ammoWhenFired);
        //weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Subtract, ammoWhenFired, -1, true);
        weapon.tInternalAmmo(weapon.InternalAmmo);
        Log.M.WL(1, "new internal ammo:"+ weapon.InternalAmmo);
        return;
      } else {
        weapon.ZeroInternalAmmo(stackItemUID);
        //weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, 0, -1, true);
        weapon.tInternalAmmo(0);
        modValue = ammoWhenFired - weapon.InternalAmmo;
      }
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (modValue == 0) { break; }
        AmmunitionBox ammoBox = weapon.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.CurrentAmmo <= 0) { continue; }
        if (ammoBox.ammoDef.Description.Id != ammo.Id) { continue; }
        if (ammoBox.CurrentAmmo >= modValue) {
          ammoBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Subtract, modValue, -1, true);
          ammoBox.tCurrentAmmo(ammoBox.CurrentAmmo);
          modValue = 0;
          Log.M.WL(1, "new current ammo in "+ammoBox.defId+":" + weapon.InternalAmmo);
          CustomAmmoCategories.AddToExposionCheck(ammoBox);
          break;
        } else {
          modValue -= ammoBox.CurrentAmmo;
          ammoBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, 0, -1, true);
          Log.M.WL(1, "new current ammo in " + ammoBox.defId + ":" + weapon.InternalAmmo);
          ammoBox.tCurrentAmmo(0);
          CustomAmmoCategories.AddToExposionCheck(ammoBox);
        }
      }
      return;
    }
    /*public static int DecrementAmmo(this Weapon instance, int stackItemUID, int StreakHitCount, bool forceStreak = false) {
      int shotsWhenFired = instance.ShotsWhenFired;
      if (StreakHitCount != 0) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (CustomAmmoCategories.getWeaponDisabledClustering(instance) == false)) {
          shotsWhenFired = StreakHitCount / instance.ProjectilesPerShot;
        } else {
          shotsWhenFired = StreakHitCount;
        }
      }
      bool streakEffect = instance.isStreak();
      if (forceStreak) { streakEffect = true; };
      if ((streakEffect == false) && (StreakHitCount == 0)) {
        StreakHitCount = shotsWhenFired;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Weapon.DecrementAmmo " + instance.UIName + " real fire count:" + StreakHitCount + "\n");
      int result = 0;
      bool noAmmoUsing = false;
      if (instance.CustomAmmoCategory().Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { noAmmoUsing = true; };
      if (instance.parent != null) {
        if (instance.parent is Turret) {
          if (instance.parent.DisplayName.Contains("Hardened")) { //TODO: Check localization
            Log.LogWrite(" Hardened turret detected\n");
          } else {
            noAmmoUsing = true;
          }
        }
      }
      if (noAmmoUsing) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
          result = shotsWhenFired;
        } else {
          result = shotsWhenFired * instance.ProjectilesPerShot;
        }
        Log.LogWrite("  weapon has no ammo (energy or turret) " + instance.UIName + "\n");
        return result;
      }
      int modValue;
      if (instance.InternalAmmo >= shotsWhenFired) {
        if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Subtract, shotsWhenFired, -1, true);
        modValue = 0;
      } else {
        modValue = shotsWhenFired - instance.InternalAmmo;
        if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, 0, -1, true);
      }
      string CurrentAmmoId = "";
      if (instance.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == true) {
        CurrentAmmoId = instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      } else {
        if (instance.ammoBoxes.Count > 0) {
          CurrentAmmoId = instance.ammoBoxes[0].ammoDef.Description.Id;
          CustomAmmoCategoriesLog.Log.LogWrite($"WARNING! strange behavior " + instance.UIName + " has no data in statistics. fallback to default ammo " + CurrentAmmoId + "\n");
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite($"WARNING! strange behavior " + instance.UIName + " not energy, parent no turret but no ammo boxes\n");
          if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
            result = shotsWhenFired;
          } else {
            result = shotsWhenFired * instance.ProjectilesPerShot;
          }
          return result;
        }
      }
      //ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
      //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.weaponDef.Description.Id);
      if (modValue == 0) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
          result = shotsWhenFired;
        } else {
          result = shotsWhenFired * instance.ProjectilesPerShot;
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  fire internal ammo. projectiles:" + result + "\n");
        return result;
      }
      for (int index = 0; index < instance.ammoBoxes.Count; ++index) {
        AmmunitionBox ammoBox = instance.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.CurrentAmmo <= 0) { continue; }
        if ((string.IsNullOrEmpty(CurrentAmmoId) == false) && (ammoBox.ammoDef.Description.Id != CurrentAmmoId)) { continue; }
        if (ammoBox.CurrentAmmo >= modValue) {
          if (StreakHitCount != 0) {
            ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Subtract, modValue, -1, true);
            CustomAmmoCategories.AddToExposionCheck(ammoBox);
          }
          modValue = 0;
        } else {
          modValue -= ammoBox.CurrentAmmo;
          if (StreakHitCount != 0) {
            ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, 0, -1, true);
            CustomAmmoCategories.AddToExposionCheck(ammoBox);
          }
        }
      }
      if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
        result = (instance.ShotsWhenFired - modValue);
      } else {
        result = (instance.ShotsWhenFired - modValue) * instance.ProjectilesPerShot;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("  fire external ammo. projectiles:" + result + "\n");
      return result;
    }*/
  }
}
