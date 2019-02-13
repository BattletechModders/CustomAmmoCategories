using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static bool isWeaponHasDiffirentAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return false; }
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        if (string.IsNullOrEmpty(ammoId)) { ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id; continue; };
        if (ammoId != weapon.ammoBoxes[index].ammoDef.Description.Id) { return true; }
      }
      return false;
    }
    public static bool isWeaponHasDiffirentModes(Weapon weapon) {
      return CustomAmmoCategories.getExtWeaponDef(weapon.defId).Modes.Count > 1;
    }
    public static bool isWeaponHasHeatAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return false; }
      float HeatPerShot = 0;
      bool HeatSetted = false;
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (HeatSetted == false) { HeatSetted = true; HeatPerShot = extAmmo.HeatDamagePerShot; continue; };
        if (HeatPerShot != extAmmo.HeatDamagePerShot) { return true; }
      }
      return false;
    }
    public static bool isWeaponHasClusterAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return false; }
      int ClusterCount = 0;
      bool ClusterSetted = false;
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        int ammoShots = (weapon.weaponDef.ShotsWhenFired + extAmmo.ShotsWhenFired) * (weapon.weaponDef.ProjectilesPerShot + extAmmo.ProjectilesPerShot);
        if (ClusterSetted == false) { ClusterSetted = true; ClusterCount = ammoShots; continue; };
        if (ClusterCount != ammoShots) { return true; }
      }
      return false;
    }
    public static void swtichToBestToHitMode(Weapon weapon,AbstractActor unit, ICombatant target) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      AbstractActor targetActor = (target as AbstractActor);
      string modeId = extWeapon.baseModeId;
      float delta = 1;
      CustomAmmoCategoriesLog.Log.LogWrite("detecting best "+weapon.UIName+" mode to hit "+target.DisplayName+"\n");
      foreach (var mode in extWeapon.Modes) {
        if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, mode.Key);
        }else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, mode.Key);
        }
        float toHit = 0;
        if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
          toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" Mode "+mode.Key+"\n");
        float curDelta = Math.Abs(toHit - mode.Value.AIHitChanceCap);
        CustomAmmoCategoriesLog.Log.LogWrite(" toHit: " + toHit + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" AIHitCap: " + mode.Value.AIHitChanceCap + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" delta: " + curDelta + "\n");
        if (curDelta < delta) {
          delta = curDelta;
          modeId = mode.Key;
        } else
        if(Math.Abs(curDelta-delta) < Epsilon) {
          if(mode.Value.isBaseMode == true) {
            delta = curDelta;
            modeId = mode.Key;
            CustomAmmoCategoriesLog.Log.LogWrite(" base mode\n");
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" Mode choosed\n");
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
    }
    public static void switchToMostHeatAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return; }
      float HeatPerShot = 0;
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (string.IsNullOrEmpty(ammoId)) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          HeatPerShot = extAmmo.HeatDamagePerShot;
          continue;
        };
        if (HeatPerShot < extAmmo.HeatDamagePerShot) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          HeatPerShot = extAmmo.HeatDamagePerShot;
        }
      }
      if (string.IsNullOrEmpty(ammoId) == false) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        }
      }
    }
    public static void switchToMostClusterAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return; }
      int ClusterShot = 0;
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        int ammoShots = (weapon.weaponDef.ShotsWhenFired + extAmmo.ShotsWhenFired) * (weapon.weaponDef.ProjectilesPerShot + extAmmo.ProjectilesPerShot);
        if (string.IsNullOrEmpty(ammoId)) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          ClusterShot = ammoShots;
          continue;
        };
        if (ClusterShot < ammoShots) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          ClusterShot = ammoShots;
        }
      }
      if (string.IsNullOrEmpty(ammoId) == false) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        }
      }
    }
    public static float calcHeatCoeff(AbstractActor unit, ICombatant target) {
      AbstractActor targetActor = (target as AbstractActor);
      //if (targetActor == null) { return 0.0f; };
      float result = 0;
      foreach (Weapon weapon in unit.Weapons) {
        if (weapon.HeatDamagePerShot == 0) { continue; }
        float toHit = 0f;
        if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
          toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
        }
        result += (
            weapon.ShotsWhenFired
            * weapon.ProjectilesPerShot
            * weapon.HeatDamagePerShot
            * toHit
        );
      }
      return result;
    }
    public static bool hasHittableLocations(Weapon weapon, List<int> hitLocations, Mech target) {
      foreach (int location in hitLocations) {
        if (weapon.DamagePerShot * 2 >= target.ArmorForLocation(location)) {
          return true;
        }
      }
      return false;
    }
    public static List<string> getAvaibleEffectiveAmmo(Weapon weapon) {
      List<string> result = new List<string>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        if (result.IndexOf(weapon.ammoBoxes[index].ammoDef.Description.Id) < 0) {
          result.Add(weapon.ammoBoxes[index].ammoDef.Description.Id);
        }
      }
      return result;
    }
    public static void SetWeaponAmmo(Weapon weapon, string ammoId) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      }
    }
    public static void ChooseBestWeaponForTarget(AbstractActor unit, ICombatant target, bool isStationary) {
      List<Weapon> ammoWeapons = new List<Weapon>();
      foreach (Weapon weapon in unit.Weapons) {
        if (CustomAmmoCategories.isWeaponHasDiffirentAmmo(weapon) == true) {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has ammo to choose\n");
          ammoWeapons.Add(weapon);
        }
      }
      if (ammoWeapons.Count > 0) {
        if (target is Mech) {
          CustomAmmoCategoriesLog.Log.LogWrite(" Target is mech\n");
          Mech targetMech = (target as Mech);
          List<Weapon> ammoHeatWeapons = new List<Weapon>();
          foreach (Weapon weapon in ammoWeapons) {
            if (CustomAmmoCategories.isWeaponHasHeatAmmo(weapon) == true) {
              CustomAmmoCategories.switchToMostHeatAmmo(weapon);
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has hit ammo\n");
              ammoHeatWeapons.Add(weapon);
            }
          }
          float expectedHeat = CustomAmmoCategories.calcHeatCoeff(unit, target);
          CustomAmmoCategoriesLog.Log.LogWrite(" Expected heat " + expectedHeat + "\n");
          if ((targetMech.CurrentHeat + expectedHeat) > targetMech.OverheatLevel) {
            foreach (Weapon weapon in ammoHeatWeapons) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " - ammo choosed\n");
              ammoWeapons.Remove(weapon);
            }
          }
          List<int> hitLocations = targetMech.GetPossibleHitLocations(unit);
          List<Weapon> ammoClusterWeapon = new List<Weapon>();
          foreach (Weapon weapon in ammoWeapons) {
            if (CustomAmmoCategories.isWeaponHasClusterAmmo(weapon)) {
              CustomAmmoCategories.switchToMostClusterAmmo(weapon);
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has cluster ammo\n");
              ammoClusterWeapon.Add(weapon);
            }
          }
          foreach (Weapon weapon in ammoClusterWeapon) {
            float toHit = 0f;
            if (weapon.parent.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
              toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, targetMech.IsEvasive, false);
            }
            if (toHit < 0.4f) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " cluster toHit is too low " + toHit + "\n");
              continue;
            }
            if (CustomAmmoCategories.hasHittableLocations(weapon, hitLocations, targetMech) == true) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " can crit one of locations\n");
              ammoWeapons.Remove(weapon);
            }
          }
        }
        AbstractActor targetActor = (target as AbstractActor);
        foreach (Weapon weapon in ammoWeapons) {
          List<string> avaibleAmmo = CustomAmmoCategories.getAvaibleEffectiveAmmo(weapon);
          if (avaibleAmmo.Count == 0) { continue; };
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " choose ammo default algorithm\n");
          string bestAmmo = "";
          float expectedDamage = 0;
          foreach (string ammoId in avaibleAmmo) {
            CustomAmmoCategories.SetWeaponAmmo(weapon, ammoId);
            float toHit = 0f;
            if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
              toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
            }
            float nonClusterCoeff = 1f;
            int numberOfShots = weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
            if ((toHit > 0.6f) && (numberOfShots == 1)) {
              nonClusterCoeff = 1.2f;
            }
            float tempExpectedDamage = numberOfShots * weapon.DamagePerShot * toHit * nonClusterCoeff;
            CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " toHit " + toHit + " expectedDamage:" + tempExpectedDamage + "\n");
            if (tempExpectedDamage > expectedDamage) {
              expectedDamage = tempExpectedDamage;
              bestAmmo = ammoId;
            }
          }
          if (string.IsNullOrEmpty(bestAmmo) == false) {
            CustomAmmoCategories.SetWeaponAmmo(weapon, bestAmmo);
            CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " best ammo choosed\n");
          }
        }
      }else {
        CustomAmmoCategoriesLog.Log.LogWrite(" No ammo to choose\n");
      }
      List<Weapon> modeWeapons = new List<Weapon>();
      foreach (Weapon weapon in unit.Weapons) {
        if (CustomAmmoCategories.isWeaponHasDiffirentModes(weapon) == true) {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has mode to choose\n");
          modeWeapons.Add(weapon);
        }
      }
      foreach (Weapon weapon in modeWeapons) {
        CustomAmmoCategories.swtichToBestToHitMode(weapon,unit,target);
      }
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrderForTarget")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackEvaluator_MakeAttackOrderForTarget {
    public static bool Prefix(AbstractActor unit, ICombatant target, int enemyUnitIndex, bool isStationary) {
      CustomAmmoCategoriesLog.Log.LogWrite(unit.DisplayName + " choosing best weapon for target " + target.DisplayName + "\n");
      try {
        CustomAmmoCategories.ChooseBestWeaponForTarget(unit, target, isStationary);
        return true;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default\n");
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrder")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackEvaluator_MakeAttackOrder {
    public static void Postfix(AbstractActor unit, bool isStationary, BehaviorTreeResults __result) {
      CustomAmmoCategoriesLog.Log.LogWrite("Choose result for " + unit.DisplayName + "\n");
      try {
        if (__result.nodeState == BehaviorNodeState.Failure) {
          CustomAmmoCategoriesLog.Log.LogWrite("  AI choosed not attack\n");
        } else
        if (__result.orderInfo is AttackOrderInfo) {
          CustomAmmoCategoriesLog.Log.LogWrite("  AI choosed to attack " + (__result.orderInfo as AttackOrderInfo).TargetUnit.DisplayName + "\n");
          CustomAmmoCategories.ChooseBestWeaponForTarget(unit, (__result.orderInfo as AttackOrderInfo).TargetUnit, isStationary);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("  AI choosed something else beside attaking\n");
        }
        return;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default\n");
        return;
      }
    }
  }
}
