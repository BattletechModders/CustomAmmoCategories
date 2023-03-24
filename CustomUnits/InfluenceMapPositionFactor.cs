using BattleTech;
using CustomDeploy;
using System;
using IRBTModUtils;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;


namespace CustomUnits {
  [HarmonyPatch(typeof(WeightedFactor))]
  [HarmonyPatch("CollectMasksForCellAndPathNode")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(MapTerrainDataCell), typeof(PathNode) })]
  public static class WeightedFactor_CollectMasksForCellAndPathNode {
    public static bool Prefix(WeightedFactor __instance, CombatGameState combat, MapTerrainDataCell cell, PathNode pathNode, ref List<DesignMaskDef> __result) {
      try {
        AbstractActor unit = Thread.CurrentThread.currentActor();
        if (unit == null) {
          Log.TWL(0, "!WARNING! CollectMasksForCellAndPathNode without unit. Result might be wrong");
          return true;
        }
        __result = unit.CollectMasksForCellAndPathNode(combat, cell, pathNode);
        return false;
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
      return true;
    }
  }
  public static class InfluenceMapPositionFactorPatch {
    public static void Prefix(AbstractActor unit, ref bool? __state) {
      try {
        __state = false;
        if (unit != null) { Thread.CurrentThread.pushActor(unit); __state = true; }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(AbstractActor unit, ref bool? __state) {
      try { 
        if((__state != null)&&(__state.HasValue)&&(__state.Value == true)) Thread.CurrentThread.clearActor();
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Prefix_Unused(AbstractActor unit_unused, ref bool? __state) {
      try {
        __state = false;
        if (unit_unused != null) { Thread.CurrentThread.pushActor(unit_unused); __state = true; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix_Unused(AbstractActor unit_unused, ref bool? __state) {
      try {
        if ((__state != null) && (__state.HasValue) && (__state.Value == true)) Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static MethodInfo PrefixMethod() => AccessTools.Method(typeof(InfluenceMapPositionFactorPatch), nameof(Prefix));
    public static MethodInfo PostfixMethod() => AccessTools.Method(typeof(InfluenceMapPositionFactorPatch), nameof(Postfix));
    public static MethodInfo PrefixMethod_Unused() => AccessTools.Method(typeof(InfluenceMapPositionFactorPatch), nameof(Prefix_Unused));
    public static MethodInfo PostfixMethod_Unused() => AccessTools.Method(typeof(InfluenceMapPositionFactorPatch), nameof(Postfix_Unused));
    public static void PatchAll(Harmony harmony) {
      try {
        List<Type> InfluenceMapPositionFactors = new List<Type>();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          try {
            Type[] types = assembly.GetTypesSafe();
            foreach (Type type in types) {
              try {
                if (type.IsInterface) { continue; }
                if (type.IsAbstract) { continue; }
                if (typeof(InfluenceMapPositionFactor).IsAssignableFrom(type) == false) { continue; }
                InfluenceMapPositionFactors.Add(type);
              } catch (Exception e) {
                Log.TWL(0, assembly.FullName);
                Log.WL(1, type.FullName);
                Log.WL(0, e.ToString(), true);
              }
            }
          } catch (Exception e) {
            Log.TWL(0, assembly.FullName);
            Log.WL(0, e.ToString(), true);
          }
        }
        foreach (Type influenceMapPositionFactor in InfluenceMapPositionFactors) {
          MethodBase EvaluateInfluenceMapFactorAtPosition = (MethodBase)AccessTools.Method(influenceMapPositionFactor, "EvaluateInfluenceMapFactorAtPosition");
          if (EvaluateInfluenceMapFactorAtPosition == null) { continue; }
          try {
            bool is_unit_unused = false;
            foreach (var param in EvaluateInfluenceMapFactorAtPosition.GetParameters()) {
              if (param.Name == "unit_unused") { is_unit_unused = true; }
            }
            if (is_unit_unused) {
              Log.WL(0,$"Patching {influenceMapPositionFactor.Name}.{EvaluateInfluenceMapFactorAtPosition.Name} with unit_unused param");
              harmony.Patch(EvaluateInfluenceMapFactorAtPosition, new HarmonyMethod(PrefixMethod_Unused()), new HarmonyMethod(PostfixMethod_Unused()));
            } else {
              Log.WL(0, $"Patching {influenceMapPositionFactor.Name}.{EvaluateInfluenceMapFactorAtPosition.Name} with unit param");
              harmony.Patch(EvaluateInfluenceMapFactorAtPosition, new HarmonyMethod(PrefixMethod()), new HarmonyMethod(PostfixMethod()));
            }
          }catch(Exception e) {
            //if(e.ToString().Contains("System.Exception: Parameter \"unit\" not found in method") == false) {
              Log.TWL(0,e.ToString(),true);
            //}
          }
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}