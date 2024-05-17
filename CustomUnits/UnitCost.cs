using BattleTech;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public static class UnitCostHelper {
    private static Dictionary<string, int> UnitsJsonCosts = new Dictionary<string, int>();
    private static Dictionary<string, int> UnitsJsonSimGameMechPartCosts = new Dictionary<string, int>();
    private static HashSet<int> MechDefs_loaded = new HashSet<int>();
    public static void StoreCost(this MechDef mechDef) {
      UnitsJsonCosts[mechDef.Description.Id] = mechDef.Description.Cost;
      UnitsJsonSimGameMechPartCosts[mechDef.Description.Id] = mechDef.SimGameMechPartCost;
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
    public static int GetJsonSimGameMechPartCost(this MechDef mechDef) {
      if (mechDef == null) { return 0; }
      if (mechDef.Description == null) { return mechDef.SimGameMechPartCost; }
      if (string.IsNullOrEmpty(mechDef.Description.Id)) { return mechDef.SimGameMechPartCost; }
      if (UnitsJsonSimGameMechPartCosts.TryGetValue(mechDef.Description.Id, out var cost)) { return cost; }
      return mechDef.SimGameMechPartCost;
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
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_Init {
    public static void Postfix(SimGameState __instance) {
      try {
        SimGameState_RehydrateUnitsCost.Postfix(__instance);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("InitFromSave")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_InitFromSave {
    public static void Postfix(SimGameState __instance) {
      try {
        SimGameState_RehydrateUnitsCost.Postfix(__instance);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager.logger.LogException(e);
      }
    }
  }
  public static class SimGameState_RehydrateUnitsCost {
    public static void Postfix(SimGameState __instance) {
      try {
        if (Core.Settings.RecalcUnitPartCost == false) { return; }
        Log.M?.TWL(0, $"Recalculating simGameMechPartCost DefaultMechPartMax:{__instance.Constants.Story.DefaultMechPartMax} base:{Core.Settings.RecalcUnitPartCostBase}");
        foreach (var mech in __instance.DataManager.MechDefs) {
          if (mech.Value == null) { continue; }
          //int newCost = Mathf.RoundToInt( (float)mech.Value.GetJsonSimGameMechPartCost() * (Core.Settings.RecalcUnitPartCostBase / (float)__instance.Constants.Story.DefaultMechPartMax) );
          int newCost = Mathf.RoundToInt((float)mech.Value.GetJsonSimGameMechPartCost() * (Core.Settings.RecalcUnitPartCostBase / (float)__instance.Constants.Story.DefaultMechPartMax));
          if (mech.Value.simGameMechPartCost != newCost) {
            Log.M?.WL(1, $"simGameMechPartCost {mech.Key} {mech.Value.simGameMechPartCost}->{newCost}");
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