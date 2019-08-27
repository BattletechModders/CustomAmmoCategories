using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using BattleTech;
using CustAmmoCategories;
using System.Reflection.Emit;

namespace CustomUnits {
  public static class WeightedFactorHelper {
    public static List<Type> FindAllDerivedTypes<T>() {
      return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
    }
    public static List<Type> FindAllDerivedTypes<T>(Assembly assembly) {
      var derivedType = typeof(T);
      return assembly
          .GetTypes()
          .Where(t =>
              t != derivedType &&
              derivedType.IsAssignableFrom(t)
              ).ToList();

    }
    public static List<DesignMaskDef> CollectMasksForCellAndPathNode(this AbstractActor unit, CombatGameState combat, MapTerrainDataCell cell, PathNode pathNode) {
      List<DesignMaskDef> designMaskDefList = new List<DesignMaskDef>();
      if (unit.UnaffectedDesignMasks()) {return designMaskDefList;};
      if (cell != null) {
        DesignMaskDef priorityDesignMask = combat.MapMetaData.GetPriorityDesignMask(cell);
        if (priorityDesignMask != null)
          designMaskDefList.Add(priorityDesignMask);
      }
      for (; pathNode != null; pathNode = pathNode.Parent) {
        Point point = new Point(combat.MapMetaData.GetXIndex(pathNode.Position.x), combat.MapMetaData.GetZIndex(pathNode.Position.z));
        DesignMaskDef priorityDesignMask = combat.MapMetaData.GetPriorityDesignMask(combat.MapMetaData.mapTerrainDataCells[point.Z, point.X]);
        if (priorityDesignMask != null && priorityDesignMask.stickyEffect != null && (priorityDesignMask.stickyEffect.effectType != EffectType.NotSet && !designMaskDefList.Contains(priorityDesignMask)))
          designMaskDefList.Add(priorityDesignMask);
      }
      return designMaskDefList;
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      List<CodeInstruction> result = instructions.ToList();
      MethodInfo CollectMasks = typeof(WeightedFactor).GetMethod("CollectMasksForCellAndPathNode");
      if(CollectMasks == null) {
        Log.LogWrite("can't find WeightedFactor.CollectMasksForCellAndPathNode\n", true);
        return result;
      }
      MethodInfo tCollectMasks = typeof(WeightedFactorHelper).GetMethod("CollectMasksForCellAndPathNode");
      if (CollectMasks == null) {
        Log.LogWrite("can't find WeightedFactorHelper.CollectMasksForCellAndPathNode\n", true);
        return result;
      }
      int methodIndex = result.FindIndex(instruction => instruction.opcode == OpCodes.Call && instruction.operand == CollectMasks);
      if (methodIndex < 0) {
        Log.LogWrite("can't find WeightedFactorHelper.CollectMasksForCellAndPathNode call\n", true);
        return result;
      }
      result[methodIndex].operand = tCollectMasks;
      while(methodIndex >= 0) {
        if(result[methodIndex].opcode == OpCodes.Ldarg_0) {
          result[methodIndex].opcode = OpCodes.Ldarg_1;
          break;
        }
        --methodIndex;
      }
      return result;
    }
    public static void PatchInfluenceMapPositionFactor(HarmonyInstance harmony) {
      List<Type> types = FindAllDerivedTypes<InfluenceMapPositionFactor>(typeof(InfluenceMapPositionFactor).Assembly);
      MethodInfo transpliter = typeof(WeightedFactorHelper).GetMethod("Transpiler", BindingFlags.Static | BindingFlags.Public);
      if (transpliter == null) {
        Log.LogWrite(1, "can't find Transpiler method", true);
        return;
      }
      foreach (Type tp in types) {
        Log.LogWrite(0,"patching class "+tp.ToString()+"\n");
        MethodInfo method = tp.GetMethod("EvaluateInfluenceMapFactorAtPosition");
        if (method == null) {
          Log.LogWrite(1, "can't find EvaluateInfluenceMapFactorAtPosition method", true);
          continue;
        }
        if (method.IsAbstract) {
          Log.LogWrite(1, "EvaluateInfluenceMapFactorAtPosition method is abstract", true);
          continue;

        }
        harmony.Patch(method,null,null,new HarmonyMethod(transpliter));
      }
      //harmony.Patch()
    }
  }
}
