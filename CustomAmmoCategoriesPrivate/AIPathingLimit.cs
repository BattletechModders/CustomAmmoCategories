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
using Harmony;
using System.Reflection;
using System.Threading;
using IRBTModUtils;
using BattleTech;
using System.Collections.Generic;
using CustomAmmoCategoriesLog;
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace CustAmmoCategories {
  //[HarmonyPatch]
  //public static class MovementCandidateLocations_Add {
  //  public static MethodBase TargetMethod() {
  //    return AccessTools.Method(typeof(List<MoveDestination>), "OnLoadedWithText");
  //  }
  //  public static void Prefix(object __instance, object item) {
  //    if(item is MoveDestination moveDest) {
  //      Log.P?.TWL(0, "List<MoveDestination>.Add()");
  //      Log.P?.WL(0, Environment.StackTrace.ToString());
  //    }
  //  }
  //}

  public static class AIPathingLimiter {
    public static string LIMIT_PATHING_SAMPLES = "LIMIT_PATHING_SAMPLES";
    public static MethodBase GetSampledPathNodes() => (MethodBase)AccessTools.Method(typeof(PathNodeGrid), "GetSampledPathNodes");
    public static MethodBase BehaviorTreeUpdate() => (MethodBase)AccessTools.Method(typeof(BehaviorTree), "Update");
    public static MethodInfo GetSampledPathNodesPostfix() => AccessTools.Method(typeof(AIPathingLimiter), nameof(GetSampledPathNodes_Postfix));
    public static MethodInfo GetSampledPathNodesPrefix() => AccessTools.Method(typeof(AIPathingLimiter), nameof(GetSampledPathNodes_Prefix));
    public static MethodBase HasDirectLOFToAnyHostileFromReachableLocationsNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("HasDirectLOFToAnyHostileFromReachableLocationsNode"), "Tick");
    public static MethodBase HasLOFToAnyHostileFromReachableLocationsNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("HasLOFToAnyHostileFromReachableLocationsNode"), "Tick");
    public static MethodBase CalcHighFidelityMaxExpectedDamageToHostile() => (MethodBase)AccessTools.Method(typeof(AIUtil), "CalcHighFidelityMaxExpectedDamageToHostile");
    public static MethodBase GenerateSprintMoveCandidatesNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("GenerateSprintMoveCandidatesNode"), "Tick");
    public static MethodBase GenerateMoveCandidatesNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("GenerateMoveCandidatesNode"), "Tick");
    public static MethodBase GenerateForwardMoveCandidatesNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("GenerateForwardMoveCandidatesNode"), "Tick");
    public static MethodBase GenerateReverseMoveCandidatesNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("GenerateReverseMoveCandidatesNode"), "Tick");
    public static MethodBase GenerateJumpMoveCandidatesNode() => (MethodBase)AccessTools.Method(AccessTools.TypeByName("GenerateJumpMoveCandidatesNode"), "Tick");
    public static MethodInfo PrefixMethod() => AccessTools.Method(typeof(AIPathingLimiter), nameof(Prefix));
    public static MethodInfo PostfixMethod() => AccessTools.Method(typeof(AIPathingLimiter), nameof(Postfix));
    public static MethodInfo PostfixFilter() => AccessTools.Method(typeof(AIPathingLimiter), nameof(MoveCandidatesFilter));
    public static MethodInfo BehaviorTreeUpdatePrefix() => AccessTools.Method(typeof(AIPathingLimiter), nameof(BehaviorTree_Update_Prefix));
    public static MethodInfo BehaviorTreeUpdatePostfix() => AccessTools.Method(typeof(AIPathingLimiter), nameof(BehaviorTree_Update_Postfix));
    //private static List<MoveDestination> original_movementCandidateLocations { get; set; } = new List<MoveDestination>();
    public static HashSet<MoveType> DO_NOT_OPTIMIZE_MOVE_TYPES = new HashSet<MoveType>() { MoveType.Melee, MoveType.None };
    public static HashSet<MoveType> DO_NOT_OPTIMIZE_MINEFIELD_MOVE_TYPES = new HashSet<MoveType>() { MoveType.Melee, MoveType.Backward, MoveType.Walking, MoveType.Sprinting };
    public static Dictionary<float, HexGrid> hexGrids = new Dictionary<float, HexGrid>();
    public class MoveDestOriginalStatistic {
      public float startTime { get; set; } = 0f;
      public HashSet<MoveDestination> moveDestinations { get; set; } = new HashSet<MoveDestination>();
      public Dictionary<MoveType,List<MoveDestination>> moveTypes { get; set; } = new Dictionary<MoveType, List<MoveDestination>>();
      public Dictionary<MoveType, int> moveType_statistic { get; set; } = new Dictionary<MoveType, int>();
      public Dictionary<MoveType, Dictionary<int, List<MoveDestination>>> costs { get; set; } = new Dictionary<MoveType, Dictionary<int, List<MoveDestination>>>();
      public void Clear() {
        startTime = 0;
        moveDestinations.Clear();
        moveType_statistic.Clear();
        moveTypes.Clear();
        costs.Clear();
      }
      public int GetDestinationsCount() {
        int result = 0;
        foreach (var dest in moveTypes) {
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(dest.Key)) { continue; }
          result += dest.Value.Count;
        }
        return result;
      }
      public void Add(Vector3 curPos, MoveDestination moveDestination) {
        if (moveDestinations.Add(moveDestination)) {
          if (moveType_statistic.ContainsKey(moveDestination.MoveType)) {
            moveType_statistic[moveDestination.MoveType] += 1;
          } else {
            moveType_statistic.Add(moveDestination.MoveType,1);
          }
          if(costs.TryGetValue(moveDestination.MoveType, out var moveTypeCosts) == false) {
            moveTypeCosts = new Dictionary<int, List<MoveDestination>>();
            costs.Add(moveDestination.MoveType, moveTypeCosts);
          }
          if (moveTypes.TryGetValue(moveDestination.MoveType, out var moveTypeDests) == false) {
            moveTypeDests = new List<MoveDestination>();
            moveTypes.Add(moveDestination.MoveType, moveTypeDests);
          }
          int cost = Mathf.CeilToInt(moveDestination.MoveType == MoveType.Jumping ? Vector3.Distance(curPos, moveDestination.PathNode.Position) : moveDestination.PathNode.CostToThisNode);
          if(moveTypeCosts.TryGetValue(cost, out var costNode) == false) {
            costNode = new List<MoveDestination>();
            moveTypeCosts.Add(cost, costNode);
          }
          costNode.Add(moveDestination);
          moveTypeDests.Add(moveDestination);
        }
      }
    }
    private static Dictionary<BehaviorTree, MoveDestOriginalStatistic> original_movementCandidateLocations { get; set; } = new Dictionary<BehaviorTree, MoveDestOriginalStatistic>();
    public static void BehaviorTree_Update_Prefix(BehaviorTree __instance) {
      try {
        BehaviorNodeState state = Traverse.Create(__instance.RootNode).Field<BehaviorNodeState>("currentState").Value;
        if (state == BehaviorNodeState.Running) { return; }
        if (original_movementCandidateLocations.TryGetValue(__instance, out MoveDestOriginalStatistic original) == false) {
          original = new MoveDestOriginalStatistic();
          original_movementCandidateLocations.Add(__instance, original);
        }
        original.Clear();
        original.startTime = __instance.unit.Combat.BattleTechGame.Time;
        Log.P?.TWL(0, "AI " + __instance.unit.PilotableActorDef.ChassisID + " calculating started", true);
      }catch(Exception e) {
        Log.P?.TWL(0, e.ToString(), true);
      }
    }
    public static void BehaviorTree_Update_Postfix(BehaviorTree __instance,ref BehaviorTreeResults __result) {
      try {
        if (__result == null) { goto flush_statistic; }
        if (__result.nodeState != BehaviorNodeState.Running) { goto flush_statistic; }
        return;
      flush_statistic:
        if (original_movementCandidateLocations.TryGetValue(__instance, out MoveDestOriginalStatistic original)&&(__instance.movementCandidateLocations != null)) {
          Log.P?.TWL(0, "AI " + __instance.unit.PilotableActorDef.ChassisID + " calculating finished in " + (__instance.unit.Combat.BattleTechGame.Time - original.startTime) + " s. Original movementCandidatesCount:" + original.moveDestinations.Count + " Effective movementCandidatesCount:" + __instance.movementCandidateLocations.Count, true);
        }
      } catch (Exception e) {
        Log.P?.TWL(0, e.ToString(), true);
      }
    }
    public static HexGrid GetHexGrid(float size) {
      if (hexGrids.TryGetValue(size, out var result)) { return result; }
      result = new HexGrid(null);
      result.HexWidth = size;
      hexGrids.Add(size, result);
      return result;
    }
    public class OptimizationItem {
      public Vector3 simplePos = Vector3.zero;
      public MoveDestination moveDest { get; set; } = null;
      public int cost { get; set; } = 0;
      public bool removed { get; set; } = false;
      public OptimizationItem(Vector3 basepos, MoveDestination dest) {
        this.simplePos.x = dest.PathNode.Position.x;
        this.simplePos.z = dest.PathNode.Position.z;
        moveDest = dest;
        this.cost = Mathf.CeilToInt(moveDest.MoveType == MoveType.Jumping ? Vector3.Distance(basepos, moveDest.PathNode.Position) : moveDest.PathNode.CostToThisNode);
      }
      public static UInt32 GetVectorHash(Vector3 pos) {
        byte[] x = BitConverter.GetBytes((short)Mathf.RoundToInt(pos.x * 10f));
        byte[] z = BitConverter.GetBytes((short)Mathf.RoundToInt(pos.z * 10f));
        byte[] result = new byte[4];
        result[0] = x[0]; result[1] = x[1]; result[2] = z[0]; result[3] = z[1];
        return BitConverter.ToUInt32(result, 0);
      }
    }
    public class OptimizationIndex {
      public AbstractActor unit { get; set; } = null;
      public List<OptimizationItem> items { get; set; } = new List<OptimizationItem>();
      public List<OptimizationItem> opt_items { get; set; } = new List<OptimizationItem>();
      public Dictionary<UInt32, List<OptimizationItem>> posItems { get; set; } = new Dictionary<UInt32, List<OptimizationItem>>();
      public Dictionary<MoveType, List<OptimizationItem>> typedItems { get; set; } = new Dictionary<MoveType, List<OptimizationItem>>();
      public OptimizationIndex(AbstractActor unit, List<MoveDestination> moveDestinations) {
        this.unit = unit;
        Log.P?.W(1, "optimization index:");
        foreach (var dest in moveDestinations) {
          var item = new OptimizationItem(unit.CurrentPosition, dest);
          items.Add(item);
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(dest.MoveType)) { continue; }
          opt_items.Add(item);
          if (typedItems.TryGetValue(dest.MoveType, out var titems) == false) {
            titems = new List<OptimizationItem>();
            typedItems.Add(dest.MoveType, titems);
          }
          titems.Add(item);
          uint vectorHash = OptimizationItem.GetVectorHash(item.simplePos);
          if (posItems.TryGetValue(vectorHash, out var pitems) == false) {
            pitems = new List<OptimizationItem>();
            posItems.Add(vectorHash, pitems);
            Log.P.W(1, $"{vectorHash}:{item.simplePos}");
          }
          posItems[vectorHash].Add(item);
        }
        Log.P?.WL(0,"",true);
      }
      public int optimized_nodes() {
        int result = 0;
        foreach(var item in opt_items) {
          if (item.removed) { continue; }
          ++result;
        }
        return result;
      }
      public void process(int radius, bool log) {
        foreach (var titems in typedItems) {
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(titems.Key)) { continue; }
          titems.Value.Sort((a, b) => { return b.cost.CompareTo(a.cost); });
        }
        Log.P?.TWL(0,$"optimization:{this.unit.PilotableActorDef.ChassisID} items:{items.Count}");
        //Log.P?.W(0, "available positions:");
        //foreach (var pitems in posItems) {
        //  int positions = 0;
        //  foreach (var item in pitems.Value) {
        //    if (item.removed) { continue; }
        //    ++positions;
        //  }
        //  Log.P.W(1,$"{pitems.Key.ToString()}:{positions}");
        //}
        //Log.P.WL(0,"");
        int remove_counter = 0;
        foreach (var titems in typedItems) {
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(titems.Key)) { continue; }
          foreach (var item in titems.Value) {
            if (item.removed) { continue; }
            //if (posItems.TryGetValue(titems.Key, out var pitems) == false) { continue; }
            var toRemove = unit.Combat.HexGrid.GetGridPointsWithinCartesianDistance(item.simplePos, radius);
            //Log.P?.WL(1, $"test position:{item.simplePos} {item.moveDest.MoveType} {item.cost} to remove:{toRemove.Count}");
            foreach (var rem in toRemove) {
              //Log.P?.W(2,$"{rem}");
              if (rem == item.simplePos) {
                //Log.P?.WL(1, "self");
                continue;
              }
              uint remhash = OptimizationItem.GetVectorHash(rem);
              //Log.P?.W(1, $":{remhash}");
              if (posItems.TryGetValue(remhash, out var rem_items)) {
                foreach (var rem_item in rem_items) {
                  if (rem_item.removed) {
                    //Log.P?.WL(1, $"already removed");
                  } else {
                    //Log.P?.WL(1, $"removed:{rem_item.moveDest.PathNode.Position} {rem_item.simplePos}");
                    rem_item.removed = true;
                    ++remove_counter;
                  }
                }
              } else {
                //Log.P?.WL(1, $"not found {rem}");
              }
            }
          }
        }
        Log.P?.WL(1, $"removed:{remove_counter}");
      }
    }
    public static void FilterFastUnits(MoveDestOriginalStatistic original, LeafBehaviorNode __instance, BehaviorTree ___tree) {
      if (CustomAmmoCategories.Settings.AIPathingOptimization == false) { return; }
      foreach (MoveDestination moveDestination in ___tree.movementCandidateLocations) {
        original.Add(___tree.unit.CurrentPosition, moveDestination);
      }
      Log.P?.TWL(0, $"MoveCandidatesFilter {___tree.unit.PilotableActorDef.ChassisID} movementCandidateLocations:{___tree.movementCandidateLocations.Count}", true);
      if (___tree.movementCandidateLocations.Count <= CustomAmmoCategories.Settings.AIPathingSamplesTreshold) { return; }
      float optimization_radius = ___tree.unit.Combat.Constants.MoveConstants.ExperimentalGridDistance;
      //AIEnemyDistanceHelper.WeaponDistCandidatesFilter(___tree.unit, ref ___tree.movementCandidateLocations);
      //Dictionary<MoveType, int> moveTypeCounts = new Dictionary<MoveType, int>();
      OptimizationIndex index = new OptimizationIndex(___tree.unit, ___tree.movementCandidateLocations);
      int opt_res = index.opt_items.Count;
      int watchdog = 2;
      do {
        Log.P?.WL(1, $"optimization radius:{optimization_radius}:{opt_res}");
        index.process(Mathf.RoundToInt(optimization_radius), false);
        int new_opt_res = index.optimized_nodes();
        int removed = opt_res - new_opt_res;
        opt_res = new_opt_res;
        Log.P?.WL(1, $"result:{opt_res} removed:{removed}");
        if (removed <= 0) { --watchdog; };
        if (watchdog <= 0) { break; }
        optimization_radius += (0.5f * ___tree.unit.Combat.Constants.MoveConstants.ExperimentalGridDistance);
      } while (opt_res > CustomAmmoCategories.Settings.AIPathingSamplesLimit);
      ___tree.movementCandidateLocations.Clear();
      foreach(var item in index.items) {
        if (item.removed) { continue; }
        ___tree.movementCandidateLocations.Add(item.moveDest);
      }
      Log.P?.WL(1, $"result:{___tree.movementCandidateLocations.Count}");
      //foreach (var stat in original.moveType_statistic) {
      //  int count = stat.Value;
      //  if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(stat.Key)) { goto stat_add; }
      //  count = Mathf.Max((stat.Value * CustomAmmoCategories.Settings.AIPathingSamplesLimit) / original.moveDestinations.Count, 1);
      //stat_add:
      //  moveTypeCounts.Add(stat.Key, count);
      //  Log.P?.WL(2, stat.Key + ":" + count);
      //}
      //___tree.movementCandidateLocations.Clear();
      //foreach (var counts in moveTypeCounts) {
      //  if (original.costs.TryGetValue(counts.Key, out var origCounts)) {
      //    List<int> costsKeys = new List<int>();
      //    costsKeys.AddRange(origCounts.Keys);
      //    costsKeys.Sort((a, b) => b.CompareTo(a));
      //    List<MoveDestination> result = new List<MoveDestination>();
      //    for (int index = 0; index < costsKeys.Count; ++index) {
      //      if (result.Count >= counts.Value) { break; }
      //      int cost = costsKeys[index];
      //      result.AddRange(origCounts[cost]);
      //    }
      //    ___tree.movementCandidateLocations.AddRange(result);
      //  }
      //}
    }
    public static void MoveCandidatesFilter(LeafBehaviorNode __instance, BehaviorTree ___tree) {
      try {
        Log.M?.TWL(0,$"MoveCandidatesFilter");
        if (original_movementCandidateLocations.TryGetValue(___tree, out MoveDestOriginalStatistic original) == false) {
          original = new MoveDestOriginalStatistic();
          original_movementCandidateLocations.Add(___tree, original);
        }
        AIMinefieldHelper.FilterMoveCandidates(___tree.unit,ref ___tree.movementCandidateLocations);
        FilterFastUnits(original, __instance, ___tree);
        //AIFleeHelper.FleeCandidatesFilter(___tree.unit, ref ___tree.movementCandidateLocations);
        Log.P?.WL(1, "filter result:" + ___tree.movementCandidateLocations.Count);
        //___tree.movementCandidateLocations.Sort((a, b) => { return a.PathNode.CostToThisNode.CompareTo(b.PathNode.CostToThisNode); });
        //foreach(MoveDestination moveDestination in ___tree.movementCandidateLocations) {
        //  Log.P?.WL(2,$"{moveDestination.MoveType} {moveDestination.PathNode.Position} {moveDestination.PathNode.CostToThisNode}");
        //}
      } catch (Exception e) {
        Log.P?.TWL(0,e.ToString(),true);
      }
    }
    /* code for this method is provided by Ashakar */
    private static Stopwatch GetSampledPathNodes_timer = new Stopwatch();
    public static bool GetSampledPathNodes_Prefix(PathNodeGrid __instance,ref List<PathNode> __result) {
      GetSampledPathNodes_timer.Restart();
      if (CustomAmmoCategories.Settings.AIPathingMultithread == false) { return true; }
      try {
        ConcurrentBag<PathNode> list = new ConcurrentBag<PathNode>();
        int num = Mathf.CeilToInt(__instance.MaxDistance / __instance.OneXUnit);
        int num2 = Mathf.CeilToInt(__instance.MaxDistance / __instance.OneZUnit);
        Parallel.For(-num, num, delegate (int i) {
          float x = (float)i * __instance.OneXUnit;
          for (int j = -num2; j <= num2; j++) {
            float z = (float)j * __instance.OneZUnit;
            Vector3 delta = new Vector3(x, 0f, z);
            Point indexFromDelta = __instance.GetIndexFromDelta(delta);
            if (__instance.combat.MapMetaData.IsWithinBounds(indexFromDelta) && !__instance.IsOutOfGrid(indexFromDelta) && __instance.pathNodes[indexFromDelta.X, indexFromDelta.Z] != null) {
              PathNode pathNode = __instance.pathNodes[indexFromDelta.X, indexFromDelta.Z];
              if (pathNode != null && pathNode.IsValidDestination) {
                list.Add(__instance.pathNodes[indexFromDelta.X, indexFromDelta.Z]);
              }
            }
          }
        });
        __result = list.ToList<PathNode>();
        return false;
      } catch (Exception e) {
        Log.P.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void GetSampledPathNodes_Postfix(PathNodeGrid __instance, AbstractActor ___owningActor, MoveType ___moveType, CombatGameState ___combat, ref List<PathNode> __result) {
      try {
        GetSampledPathNodes_timer.Stop();
        Log.P?.TWL(0, "PathNodeGrid.GetSampledPathNodes "+___owningActor.PilotableActorDef.ChassisID 
          + " limit samples:"+ Thread.CurrentThread.isFlagSet(LIMIT_PATHING_SAMPLES) 
          + " moveType:"+___moveType
          + " multi-thread:"+CustomAmmoCategories.Settings.AIPathingMultithread
          + " elapsed:" + GetSampledPathNodes_timer.Elapsed.TotalMilliseconds+" ms"
          + " result:" +__result.Count,true);
        if (Thread.CurrentThread.isFlagSet(LIMIT_PATHING_SAMPLES) == false) {
          //Log.P?.TWL(0, "PathNodeGrid.GetSampledPathNodes NOT AI GENERATION " + ___owningActor.PilotableActorDef.ChassisID + " moveType:" + ___moveType + " result:" + __result.Count, true);
          //Log.P?.WL(0, Environment.StackTrace.ToString());
          return;
        }
        if (CustomAmmoCategories.Settings.AIPathingOptimization == false) { return; }
        if (__result.Count <= CustomAmmoCategories.Settings.AIPathingSamplesLimit) { return; }
        List<PathNode> result = new List<PathNode>();
        Dictionary<int, List<PathNode>> costs = new Dictionary<int, List<PathNode>>();
        Log.P?.WL(1, "optimizing", true);
        //float costCap = __instance.MaxDistance * CustomAmmoCategories.Settings.AIPathingSamplesCoef;
        foreach (PathNode node in __result) {
          int cost = Mathf.CeilToInt(node.CostToThisNode);
          if (costs.TryGetValue(cost, out List<PathNode> list) == false) {
            list = new List<PathNode>();
            costs.Add(cost, list);
          }
          list.Add(node);
        }
        List<int> costsKeys = new List<int>();
        costsKeys.AddRange(costs.Keys);
        costsKeys.Sort((a, b) => b.CompareTo(a));
        //for (int index = 0; index < costsKeys.Count; ++index) {
        //  int cost = costsKeys[index];
        //  //Log.P?.WL(2, "cost: "+ cost + " count:"+ costs[cost].Count);
        //}
        __result.Clear();
        for (int index=0;index<costsKeys.Count;++index) { 
          if (result.Count >= CustomAmmoCategories.Settings.AIPathingSamplesLimit) { break; }
          int cost = costsKeys[index];
          result.AddRange(costs[cost]);
        }
        __result.AddRange(result);
        //__result.AddRange(costs[0]);
        Log.P?.WL(1, "optimized " + __result.Count, true);
      } catch (Exception e) {
        Log.P?.TWL(0, e.ToString(), true);
      }
    }
    public static void Prefix() {
      Thread.CurrentThread.SetFlag(LIMIT_PATHING_SAMPLES);
    }
    public static void Postfix() {
      Thread.CurrentThread.ClearFlag(LIMIT_PATHING_SAMPLES);
    }
  }
}