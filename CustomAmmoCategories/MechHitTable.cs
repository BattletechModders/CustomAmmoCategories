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
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AIAttackEvaluator))]
  [HarmonyPatch("GetLocationDictionary")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech), typeof(Vector3), typeof(Quaternion) })]
  public static class AIAttackEvaluator_GetLocationDictionary {
    public static AttackDirection GetAttackDirection(Vector3 attackerPosition,AbstractActor targetActor,Vector3 targetPosition, Quaternion targetRotation) {
      return targetActor.IsProne ? AttackDirection.ToProne : targetActor.Combat.HitLocation.GetAttackDirection(attackerPosition, targetPosition, targetRotation);
    }
    public static Dictionary<T, float> HitTableToLocationDirectory<T>(Dictionary<T, int> hitTable) {
      float num = 0.0f;
      foreach (KeyValuePair<T, int> keyValuePair in hitTable)
        num += (float)keyValuePair.Value;
      Dictionary<T, float> dictionary = new Dictionary<T, float>();
      foreach (KeyValuePair<T, int> keyValuePair in hitTable)
        dictionary[keyValuePair.Key] = (float)keyValuePair.Value / num;
      return dictionary;
    }
    public static bool Prefix(Vector3 attackerPosition, Mech m, Vector3 targetPosition, Quaternion targetRotation, ref Dictionary<ArmorLocation, float> __result) {
      try {
        Log.Combat?.TWL(0, "AIAttackEvaluator.GetLocationDictionary Prefix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(m);
        AttackDirection attackDirection = AIAttackEvaluator_GetLocationDictionary.GetAttackDirection(attackerPosition, (AbstractActor)m, targetPosition, targetRotation);
        __result = AIAttackEvaluator_GetLocationDictionary.HitTableToLocationDirectory<ArmorLocation>(m.Combat.HitLocation.GetMechHitTableCustom(attackDirection, m, Thread.CurrentThread.currentWeapon(), -1));
        return false;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return true;
      }
    }
    public static void Postfix(Vector3 attackerPosition, Mech m, Vector3 targetPosition, Quaternion targetRotation) {
      try {
        Log.Combat?.TWL(0, "AIAttackEvaluator.GetLocationDictionary Postfix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AIAttackEvaluator))]
  [HarmonyPatch("evaluateWeaponAttackOnMech")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AIAttackEvaluator_evaluateWeaponAttackOnMech {
    public static void Prefix(Weapon w) {
      try {
        Thread.CurrentThread.pushWeapon(w);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
    public static void Postfix() {
      try {
        Thread.CurrentThread.clearWeapon();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(DamageOrderUtility))]
  [HarmonyPatch("ApplyDamageToAllLocations")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(ICombatant), typeof(int), typeof(int), typeof(AttackDirection), typeof(DamageType) })]
  public static class DamageOrderUtility_ApplyDamageToAllLocations {
    public static bool Prefix(string owningActorGUID, int sequenceGUID, int rootSequenceGUID, ICombatant target, int minDamage, int maxDamage, AttackDirection attackDirection, DamageType damageType) {
      try {
        Mech mech = target as Mech;
        Vehicle vehicle = target as Vehicle;
        Turret turret = target as Turret;
        BattleTech.Building building = target as BattleTech.Building;
        CombatGameState combat = target.Combat;
        NetworkRandom networkRandom = combat.NetworkRandom;
        if (mech != null) {
          Dictionary<ArmorLocation, int> mechHitTable = combat.HitLocation.GetMechHitTableCustom(attackDirection, mech, null, -1);
          float totalDamage = 0.0f;
          foreach (ArmorLocation key in mechHitTable.Keys) {
            if (key != ArmorLocation.Head) {
              int num = networkRandom.Int(minDamage, maxDamage + 1);
              totalDamage += (float)num;
              mech.DEBUG_DamageLocation(key, (float)num, (AbstractActor)null, damageType, owningActorGUID);
              Vector3 hitPosition = mech.GameRep.GetHitPosition((int)key);
              DamageOrderUtility.ShowDamageFloatie(combat, owningActorGUID, (ICombatant)mech, (float)num, hitPosition, (int)key);
            }
          }
          mech.GameRep.PlayImpactAnimSimple(attackDirection, totalDamage);
          if (!mech.IsDead)
            mech.CheckPilotStatusFromAttack(owningActorGUID, sequenceGUID, rootSequenceGUID);
        } else if (vehicle != null) {
          foreach (VehicleChassisLocations key in combat.HitLocation.GetVehicleHitTable(attackDirection).Keys) {
            if (key != VehicleChassisLocations.Turret || (double)vehicle.GetCurrentStructure(VehicleChassisLocations.Turret) > 0.0) {
              int num = networkRandom.Int(minDamage, maxDamage + 1);
              vehicle.DEBUG_DamageLocation(key, (float)num, attackerGUID: owningActorGUID);
              Vector3 hitPosition = vehicle.GameRep.GetHitPosition((int)key);
              DamageOrderUtility.ShowDamageFloatie(combat, owningActorGUID, (ICombatant)vehicle, (float)num, hitPosition, (int)key);
            }
          }
        } else if (turret != null) {
          int num = networkRandom.Int(minDamage, maxDamage + 1);
          turret.DEBUG_DamageLocation(BuildingLocation.Structure, (float)num, attackerGUID: owningActorGUID);
          Vector3 hitPosition = turret.GameRep.GetHitPosition(1);
          DamageOrderUtility.ShowDamageFloatie(combat, owningActorGUID, (ICombatant)turret, (float)num, hitPosition, 1);
        } else if (building != null && !building.IsDead) {
          int num = networkRandom.Int(minDamage, maxDamage + 1);
          float totalDamage = (float)num * combat.Constants.CombatValueMultipliers.MortarBuildingDamageMultiplier;
          building.DEBUG_DamageBuilding(totalDamage, attackerGUID: owningActorGUID);
          Vector3 hitPosition = building.GameRep.GetHitPosition(1);
          DamageOrderUtility.ShowDamageFloatie(combat, owningActorGUID, (ICombatant)building, (float)num, hitPosition, 1);
        }
        target.HandleDeath("Artillery");
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.damageLogger.LogException(e);
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetMechHitTable")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection), typeof(bool) })]
  public static class HitLocation_GetMechHitTableCustom {
    //private static Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> GetMechHitTableCache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
    public delegate Dictionary<ArmorLocation, int> d_GetMechHitTable(HitLocation hitLocation, AttackDirection from, Mech target, Weapon w, int attackSequence, bool log);
    public static d_GetMechHitTable i_GetMechHitTable { get; set; } = null;
    public delegate Dictionary<ArmorLocation, int> d_GetMechHitTableClustered(HitLocation hitLocation, AttackDirection from, Mech target, ArmorLocation location, Weapon w, int attackSequence, bool log);
    public static d_GetMechHitTableClustered i_GetMechHitTableClustered = null;
    public static Dictionary<ArmorLocation, int> GetMechHitTableCustom(this HitLocation hitLocation, AttackDirection from, Mech target, Weapon w, int attackSequence, bool log = true) {
      if (i_GetMechHitTable != null) { return i_GetMechHitTable(hitLocation, from, target, w, attackSequence, log); }
      return hitLocation.GetMechHitTable(from, log);
    }
    public static Dictionary<ArmorLocation, int> GetMechClusterTableCustom(this HitLocation hitLocation, AttackDirection from, Mech target, ArmorLocation location, Weapon w, int attackSequence, bool log = true) {
      if (i_GetMechHitTableClustered != null) { return i_GetMechHitTableClustered(hitLocation, from, target, location, w, attackSequence, log); }
      return hitLocation.combat.Constants.GetMechClusterTable(location, from);
    }
    public static void Postfix(HitLocation __instance, AttackDirection from, bool log, ref Dictionary<ArmorLocation, int> __result) {
      //throw new Exception("call not allowed");
      //Mech mech = Thread.CurrentThread.currentMech();
      //if (mech == null) { return; }
      //Log.TWL(0, "HitLocation.GetMechHitTable " + mech.Description.Id);
      //TrooperSquad squad = mech as TrooperSquad;
      //if (squad != null) {
      //  __result = squad.GetHitTable(from);
      //} else
      //if (mech.FakeVehicle()) {
      //  if (GetMechHitTableCache.TryGetValue(from, out Dictionary<ArmorLocation, int> result)) {
      //    __result = result;
      //  } else {
      //    Dictionary<VehicleChassisLocations, int> vres = __instance.GetVehicleHitTable(from, log);
      //    result = new Dictionary<ArmorLocation, int>();
      //    foreach (var vloc in vres) {
      //      ArmorLocation aloc = CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(vloc.Key);
      //      if (result.ContainsKey(aloc)) { continue; }
      //      result.Add(aloc, vloc.Value);
      //    }
      //    GetMechHitTableCache.Add(from, result);
      //    __result = result;
      //  }
      //}
    }
  }
}