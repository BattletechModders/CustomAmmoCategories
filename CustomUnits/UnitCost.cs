using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public static class UnitCostHelper {
    private static Dictionary<string, int> UnitsJsonCosts = new Dictionary<string, int>();
    private static HashSet<int> MechDefs_loaded = new HashSet<int>();
    public static void StoreCost(this MechDef mechDef) {
      UnitsJsonCosts[mechDef.Description.Id] = mechDef.Description.Cost;
      MechDefs_loaded.Add(mechDef.GetHashCode());
    }
    public static int GetJsonCost(this MechDef mechDef) {
      if (mechDef == null) { return 0; }
      if (mechDef.Description == null) { return 0; }
      if (string.IsNullOrEmpty(mechDef.Description.Id)) { return mechDef.Description.Cost; }
      if (MechDefs_loaded.Contains(mechDef.GetHashCode()) == false) { return mechDef.Description.Cost; }
      if (UnitsJsonCosts.TryGetValue(mechDef.Description.Id, out var cost)) { return cost; }
      return mechDef.Description.Cost;
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("RefreshBattleValue")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_RefreshBattleValue {
    public static void Postfix(MechDef __instance) {
      try {
        if (Core.Settings.PreserveJsonUnitCost == false) { return; }
        int restoreCost = __instance.GetJsonCost();
        Log.M?.TWL(0, $"MechDef.RefreshBattleValue {__instance.Description.Id}:{__instance.GetHashCode()} {(restoreCost != __instance.Description.Cost?("was "+ __instance.Description.Cost.ToString() + " become "+restoreCost.ToString()) :("not changed " + __instance.Description.Cost.ToString()))}");
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager.logger.LogException(e);
      }
    }
  }
  public static class SimGameState_RehydrateUnitsCost {
    public static void Postfix(SimGameState __instance) {
      try {
        foreach(var mech in __instance.DataManager.MechDefs) {
          if (mech.Value == null) { continue; }
          int newCost = Mathf.RoundToInt( (float)mech.Value.simGameMechPartCost * (5.0f / (float)__instance.Constants.Story.DefaultMechPartMax) );
          if (mech.Value.simGameMechPartCost != newCost) {
            Log.M?.WL(0, $"simGameMechPartCost {mech.Key} {mech.Value.simGameMechPartCost}->{newCost}");
            mech.Value.simGameMechPartCost = newCost;
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager.logger.LogException(e);
      }
    }
  }

}