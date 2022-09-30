/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
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
#pragma warning disable CS0252
  public class WeightedFactorHelperTranspilerSafeException : Exception {
    public WeightedFactorHelperTranspilerSafeException(string message) : base(message) {
    }
  }
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
    private static bool patched_success = false;
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      patched_success = false;
      List<CodeInstruction> result = instructions.ToList();
      MethodInfo CollectMasks = typeof(WeightedFactor).GetMethod("CollectMasksForCellAndPathNode");
      if(CollectMasks == null) {
        Log.LogWrite("can't find WeightedFactor.CollectMasksForCellAndPathNode\n", true);
        throw new Exception("fail");
        //return result;
      }
      MethodInfo tCollectMasks = typeof(WeightedFactorHelper).GetMethod("CollectMasksForCellAndPathNode");
      if (CollectMasks == null) {
        Log.LogWrite("can't find WeightedFactorHelper.CollectMasksForCellAndPathNode\n", true);
        throw new Exception("fail");
        //return result;
      }
      int methodIndex = result.FindIndex(instruction => instruction.opcode == OpCodes.Call && instruction.operand == CollectMasks);
      if (methodIndex < 0) {
        Log.LogWrite("can't find WeightedFactorHelper.CollectMasksForCellAndPathNode call\n", true);
        throw new Exception("fail");
        //return result;
      }
      result[methodIndex].operand = tCollectMasks;
      while(methodIndex >= 0) {
        if(result[methodIndex].opcode == OpCodes.Ldarg_0) {
          result[methodIndex].opcode = OpCodes.Ldarg_1;
          break;
        }
        --methodIndex;
      }
      patched_success = true;
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
        MethodInfo method = tp.GetMethod("EvaluateInfluenceMapFactorAtPosition");
        if (method == null) {
          Log.LogWrite(1, "can't find EvaluateInfluenceMapFactorAtPosition method", true);
          continue;
        }
        if (method.IsAbstract) {
          Log.LogWrite(1, "EvaluateInfluenceMapFactorAtPosition method is abstract", true);
          continue;
        }
        patched_success = false;
        try {
          var patched = harmony.Patch(method, null, null, new HarmonyMethod(transpliter));
          if (patched_success == false) harmony.Unpatch(patched, HarmonyPatchType.Transpiler);
        } catch(Exception) {
          //Log.TWL(0,e.ToString());
        }
      }
      //harmony.Patch()
    }
  }
#pragma warning restore CS0252
}
