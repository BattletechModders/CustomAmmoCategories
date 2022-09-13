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
    public class MoveDestOriginalStatistic {
      public float startTime { get; set; } = 0f;
      public HashSet<MoveDestination> moveDestinations { get; set; } = new HashSet<MoveDestination>();
      public Dictionary<MoveType, int> moveType_statistic { get; set; } = new Dictionary<MoveType, int>();
      public Dictionary<MoveType, Dictionary<int, List<MoveDestination>>> costs { get; set; } = new Dictionary<MoveType, Dictionary<int, List<MoveDestination>>>();
      public void Clear() {
        startTime = 0;
        moveDestinations.Clear();
        moveType_statistic.Clear();
        costs.Clear();
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
          int cost = Mathf.CeilToInt(moveDestination.MoveType == MoveType.Jumping ? Vector3.Distance(curPos, moveDestination.PathNode.Position) : moveDestination.PathNode.CostToThisNode);
          if(moveTypeCosts.TryGetValue(cost, out var costNode) == false) {
            costNode = new List<MoveDestination>();
            moveTypeCosts.Add(cost, costNode);
          }
          costNode.Add(moveDestination);
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
    public static void MoveCandidatesFilter(LeafBehaviorNode __instance, BehaviorTree ___tree) {
      try {
        if (original_movementCandidateLocations.TryGetValue(___tree, out MoveDestOriginalStatistic original) == false) {
          original = new MoveDestOriginalStatistic();
          original_movementCandidateLocations.Add(___tree, original);
        }
        AIMinefieldHelper.FilterMoveCandidates(___tree.unit,ref ___tree.movementCandidateLocations);
        if (CustomAmmoCategories.Settings.AIPathingOptimization == false) { return; }
        foreach (MoveDestination moveDestination in ___tree.movementCandidateLocations) {
          original.Add(___tree.unit.CurrentPosition ,moveDestination);
        }
        Log.P?.TWL(0, "MoveCandidatesFilter " + ___tree.unit.PilotableActorDef.ChassisID + " movementCandidateLocations:" + ___tree.movementCandidateLocations.Count, true);
        if (___tree.movementCandidateLocations.Count <= CustomAmmoCategories.Settings.AIPathingSamplesLimit) { goto print_locations; }
        Dictionary<MoveType, int> moveTypeCounts = new Dictionary<MoveType, int>();
        Log.P?.WL(1, "filter target:");
        foreach (var stat in original.moveType_statistic) {
          int count = stat.Value;
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(stat.Key)) { goto stat_add; }
          //if (minefields_no_optimize && (DO_NOT_OPTIMIZE_MINEFIELD_MOVE_TYPES.Contains(stat.Key))) { goto stat_add; }
          count = Mathf.Max((stat.Value * CustomAmmoCategories.Settings.AIPathingSamplesLimit) / original.moveDestinations.Count, 1);
          stat_add:
          moveTypeCounts.Add(stat.Key, count);
          Log.P?.WL(2, stat.Key + ":" + count);
        }
        ___tree.movementCandidateLocations.Clear();
        foreach (var counts in moveTypeCounts) {
          if (original.costs.TryGetValue(counts.Key, out var origCounts)) {
            List<int> costsKeys = new List<int>();
            costsKeys.AddRange(origCounts.Keys);
            costsKeys.Sort((a, b) => b.CompareTo(a));
            List<MoveDestination> result = new List<MoveDestination>();
            for (int index = 0; index < costsKeys.Count; ++index) {
              if (result.Count >= counts.Value) { break; }
              int cost = costsKeys[index];
              result.AddRange(origCounts[cost]);
            }
            ___tree.movementCandidateLocations.AddRange(result);
          }
        }
        print_locations:
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