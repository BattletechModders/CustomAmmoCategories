using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using IRBTModUtils;
using System.Threading;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("ApplyEventAction")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameResultAction), typeof(object) })]
  public static class SimGameState_ApplyEventAction {
    public static void Prefix(SimGameResultAction action, object additionalObject) {
      try {
        Log.M?.TWL(0, $"SimGameState.ApplyEventAction {action.Type} {action.value}");
        Thread.CurrentThread.pushToStack<SimGameResultAction>("ApplyEventAction", action);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(SimGameResultAction action, object additionalObject) {
      try {
        Thread.CurrentThread.popFromStack<SimGameResultAction>("ApplyEventAction");
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("SetCurrentSystem")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(StarSystem), typeof(bool), typeof(bool) })]
  public static class SimGameState_SetCurrentSystem {
    public static void Postfix(SimGameState __instance, StarSystem system, bool force, bool timeSkip) {
      try {
        SimGameResultAction action = Thread.CurrentThread.peekFromStack<SimGameResultAction>("ApplyEventAction");
        if (action == null) { return; }
        if (action.Type != SimGameResultAction.ActionType.Company_TravelTo) { return; }
        Log.M?.TWL(0, $"SimGameState.ApplyEventAction {action.Type} {system.Name} {__instance.TravelManager.TravelState}");
        if (__instance.TravelManager.TravelState != SimGameTravelStatus.IN_SYSTEM) {
          if (__instance.ActiveTravelContract != null) { __instance.OnBreadcrumbCancelledByUser(); };
          __instance.Starmap.CancelTravelAndMoveToCurrentSystem();
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameEventTracker))]
  [HarmonyPatch("CheckRoll")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameEventTracker_CheckRoll {
    public static void Postfix(SimGameEventTracker __instance, bool incrementOnFailure, float randomRoll, ref bool __result) {
      try {
        //if (UnityGameInstance.BattleTechGame.Simulation.TravelState == SimGameTravelStatus.AT_JUMP_POINT) { __result = true; }
        Log.M?.TWL(0, $"SimGameEventTracker.CheckRoll TravelState:{UnityGameInstance.BattleTechGame.Simulation.TravelState} result:{__result}");
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("InitCompanyStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class SimGameState_InitCompanyStats {
    public static readonly string JumpShipCostOverrideStatName = "JumpShipCost_override";
    public static void Postfix(SimGameState __instance) {
      try {
        __instance.CompanyStats.AddStatistic<int>("JumpShipCost_override", __instance.Constants.Finances.JumpShipCost);
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class SimGameState_RehydrateJumpCost {
    public static void Postfix(SimGameState __instance) {
      try {
        int ret = __instance.Constants.Finances.JumpShipCost;
        Statistic jumpCost = __instance.CompanyStats.GetOrCreateStatisic<int>(SimGameState_InitCompanyStats.JumpShipCostOverrideStatName, ret);
        ret = jumpCost.Value<int>();
        Log.M?.TWL(0, $"SimGameState.Rehydrate JumpCost:{ret}");
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SGTravelManager))]
  [HarmonyPatch("HandleNextTravelStep")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGTravelManager_HandleNextTravelStep {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      int found = 0;
      foreach(var tmp in instructions) {
        var instruction = tmp;
        if ((instruction.opcode == OpCodes.Callvirt) && ((MethodBase)instruction.operand == AccessTools.Property(typeof(SimGameState), "Constants").GetGetMethod())) {
          instruction.opcode = OpCodes.Call;
          instruction.operand = (object)AccessTools.Method(typeof(SGTravelManager_HandleNextTravelStep), nameof(JumpCost));
          found = 2;
        } else if (found > 0) {
          --found;
          instruction.opcode = OpCodes.Nop;
          instruction.operand = null;
        }
        yield return instruction;
      }
    }
    public static int JumpCost(SimGameState sim) {
      int ret = sim.Constants.Finances.JumpShipCost;
      try {
        Statistic jumpCost = sim.CompanyStats.GetOrCreateStatisic<int>(SimGameState_InitCompanyStats.JumpShipCostOverrideStatName, ret);
        ret = jumpCost.Value<int>();
        Log.M?.TWL(0, $"Request jump cost {ret}");
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
      return ret;
    }
  }
  [HarmonyPatch(typeof(Starmap))]
  [HarmonyPatch("OnPathfindingComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AStar.AStarResult) })]
  public static class Starmap_OnPathfindingComplete {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      int found = 0;
      foreach (var tmp in instructions) {
        var instruction = tmp;
        if ((instruction.opcode == OpCodes.Callvirt) && ((MethodBase)instruction.operand == AccessTools.Property(typeof(SimGameState), "Constants").GetGetMethod())) {
          instruction.opcode = OpCodes.Call;
          instruction.operand = (object)AccessTools.Method(typeof(SGTravelManager_HandleNextTravelStep), nameof(SGTravelManager_HandleNextTravelStep.JumpCost));
          found = 2;
        } else if (found > 0) {
          --found;
          instruction.opcode = OpCodes.Nop;
          instruction.operand = null;
        }
        yield return instruction;
      }
    }
  }
}