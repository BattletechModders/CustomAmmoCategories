using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetPossibleHitLocations")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vehicle) })]
  public static class HitLocation_GetPossibleHitLocations {
    public static void Postfix(HitLocation __instance, Vector3 attackerPosition, Vehicle target, ref List<int> __result) {
      if (target.VehicleDef.Chassis.HasTurret) { return; }
      __result.Remove((int)VehicleChassisLocations.Turret);
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetAdjacentHitLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vehicle), typeof(float), typeof(VehicleChassisLocations), typeof(float), typeof(float), typeof(VehicleChassisLocations), typeof(float) })]
  public static class HitLocation_GetAdjacentHitLocation {
    public static void Postfix(HitLocation __instance, Vector3 attackPosition, Vehicle target, float randomRoll, VehicleChassisLocations previousHitLocation, float originalMultiplier, float adjacentMultiplier, VehicleChassisLocations bonusLocation, float bonusChanceMultiplier, ref VehicleChassisLocations __result) {
      if (target.VehicleDef.Chassis.HasTurret) { return; }
      if (__result != VehicleChassisLocations.Turret) { return; }
      if (bonusLocation == VehicleChassisLocations.Turret) { bonusLocation = VehicleChassisLocations.None; };
      AttackDirection attackDirection = __instance.GetAttackDirection(attackPosition, (ICombatant)target);
      Dictionary<VehicleChassisLocations, int> hitTable = target.Combat.Constants.GetVehicleClusterTable(previousHitLocation, attackDirection);
      if (hitTable == null) { __result = VehicleChassisLocations.None; return; }
      hitTable.Remove(VehicleChassisLocations.Turret);
      if ((double)originalMultiplier > 1.00999999046326 || (double)adjacentMultiplier > 1.00999999046326) {
        Dictionary<VehicleChassisLocations, int> dictionary = new Dictionary<VehicleChassisLocations, int>();
        VehicleChassisLocations adjacentLocations = VehicleStructureRules.GetAdjacentLocations(previousHitLocation);
        foreach (KeyValuePair<VehicleChassisLocations, int> keyValuePair in hitTable) {
          if (keyValuePair.Key == previousHitLocation)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)originalMultiplier));
          else if ((adjacentLocations | keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)adjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
        hitTable = dictionary;
      }
      __result = HitLocation.GetHitLocation<VehicleChassisLocations>(hitTable, randomRoll, bonusLocation, bonusChanceMultiplier);
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetHitLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vehicle), typeof(float), typeof(VehicleChassisLocations), typeof(float) })]
  public static class HitLocation_GetHitLocation {
    public static void Postfix(HitLocation __instance, Vector3 attackPosition, Vehicle target, float randomRoll, VehicleChassisLocations bonusLocation, float bonusChanceMultiplier, ref VehicleChassisLocations __result) {
      if (target.VehicleDef.Chassis.HasTurret) { return; }
      if (__result != VehicleChassisLocations.Turret) { return; }
      if (bonusLocation == VehicleChassisLocations.Turret) { bonusLocation = VehicleChassisLocations.None; };
      AttackDirection attackDirection = __instance.GetAttackDirection(attackPosition, (ICombatant)target);
      Dictionary<VehicleChassisLocations, int> vehicleHitTable = __instance.GetVehicleHitTable(attackDirection, true);
      if (vehicleHitTable == null) { __result = VehicleChassisLocations.None; return; }
      vehicleHitTable.Remove(VehicleChassisLocations.Turret);
      __result = HitLocation.GetHitLocation<VehicleChassisLocations>(vehicleHitTable, randomRoll, bonusLocation, bonusChanceMultiplier);
    }
  }
}