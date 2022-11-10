using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustAmmoCategories {
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