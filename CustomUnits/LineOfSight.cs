using BattleTech;
using HarmonyLib;
using System;
using UnityEngine;
using System.Threading;
using IRBTModUtils;
using System.Collections.Generic;
using CustAmmoCategories;

namespace CustomUnits {
  [HarmonyPatch(typeof(LineOfSight), "GetLineOfFireUncached")]
  [HarmonyPriority(-800)]
  public static class LineOfSight_GetLineOfFireUncached {
    public static readonly string IN_LineOfSight_FLAG = "IN_LineOfSight";
    public static void Prefix(ref bool __runOriginal, LineOfSight __instance, AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation, ref Vector3 collisionWorldPos, ref LineOfFireLevel __result) {
      //Log.Combat?.TWL(0, $"LineOfSight.GetLineOfFireUncached prefix original:{__runOriginal}");
      try {
        Thread.CurrentThread.SetFlag(IN_LineOfSight_FLAG);
      }catch(Exception e) {
        Log.ECombat?.TWL(0,e.ToString(),true);
      }
    }
    public static void Postfix(bool __runOriginal, LineOfSight __instance, AbstractActor source, Vector3 sourcePosition, ICombatant target, Vector3 targetPosition, Quaternion targetRotation, ref Vector3 collisionWorldPos, ref LineOfFireLevel __result) {
      //Log.Combat?.TWL(0, $"LineOfSight.GetLineOfFireUncached postfix original:{__runOriginal}");
      try {
        Thread.CurrentThread.ClearFlag(IN_LineOfSight_FLAG);
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
      }
    }
    public static void Finalizer(Exception __exception) {
      if (__exception == null) { return; }
      Log.ECombat?.TWL(0, __exception.ToString(), true);
      CombatGameState.gameInfoLogger.LogException(__exception);
    }
  }
  [HarmonyPatch(typeof(CombatGameState), "AllActors", MethodType.Getter)]
  public static class CombatGameState_AllActors {
    public static void Postfix(ref List<AbstractActor> __result) {
      if (Thread.CurrentThread.isFlagSet(LineOfSight_GetLineOfFireUncached.IN_LineOfSight_FLAG) == false) { return; }
      //Log.Combat?.W(0, $"LineOfSight.GetLineOfFireUncached.AllActors {__result.Count}->");
      for (int t = 0; t < __result.Count;) {
        AbstractActor unit = __result[t];
        if (unit.IsDead) { __result.RemoveAt(t); continue; }
        if (unit.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) { __result.RemoveAt(t); continue; }
        ++t;
      }
      //Log.Combat?.WL(0,$"{__result.Count}");
    }
  }
  [HarmonyPatch(typeof(CombatGameState), "GetAllLivingActors")]
  public static class CombatGameState_GetAllLivingActors {
    public static void Postfix(ref List<AbstractActor> __result) {
      if (Thread.CurrentThread.isFlagSet(LineOfSight_GetLineOfFireUncached.IN_LineOfSight_FLAG) == false) { return; }
      //Log.Combat?.W(0, $"LineOfSight.GetLineOfFireUncached.GetAllLivingActors {__result.Count}->");
      for (int t = 0; t < __result.Count;) {
        AbstractActor unit = __result[t];
        if (unit.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) { __result.RemoveAt(t); continue; }
        ++t;
      }
      //Log.Combat?.WL(0, $"{__result.Count}");
    }
  }
}