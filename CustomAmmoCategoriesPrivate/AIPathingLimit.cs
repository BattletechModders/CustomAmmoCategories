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
using HarmonyLib;
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
  public class FlatPosition {
    private short x;
    private short y;
    public float X { get { return (float)x / 10f; } }
    public float Y { get { return (float)y / 10f; } }
    public FlatPosition(Vector3 pos) {
      this.x = (short)Mathf.RoundToInt(pos.x * 10f);
      this.y = (short)Mathf.RoundToInt(pos.z * 10f);
    }
    public FlatPosition() {
      this.x = 0;
      this.y = 0;
    }
    public override int GetHashCode() {
      var xb = BitConverter.GetBytes(this.x);
      var yb = BitConverter.GetBytes(this.y);
      byte[] rb = new byte[4];
      xb.CopyTo(rb, 0);
      yb.CopyTo(rb, 2);
      return BitConverter.ToInt32(rb, 0);
    }
    public override bool Equals(object obj) {
      if(obj is FlatPosition b) {
        return (this.x == b.x) && (this.y == b.y);
      }
      return false;
    }
  }
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
      public void process(int radius, int goal) {
        foreach (var titems in typedItems) {
          if (DO_NOT_OPTIMIZE_MOVE_TYPES.Contains(titems.Key)) { continue; }
          titems.Value.Sort((a, b) => { return b.cost.CompareTo(a.cost); });
        }
        int current = this.optimized_nodes();
        if (current <= goal) { return; }
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
                    --current;
                    ++remove_counter;
                    if (current <= goal) { goto exit_optimization; }
                  }
                }
              } else {
                //Log.P?.WL(1, $"not found {rem}");
              }
            }
          }
        }
        exit_optimization:
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
        index.process(Mathf.RoundToInt(optimization_radius), CustomAmmoCategories.Settings.AIPathingSamplesLimit);
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
    }
    public static void MoveCandidatesFilter(LeafBehaviorNode __instance) {
      try {
        Log.M?.TWL(0,$"MoveCandidatesFilter {__instance.tree.unit.PilotableActorDef.ChassisID} FlyingHeight:{__instance.tree.unit.FlyingHeight()}");
        if (original_movementCandidateLocations.TryGetValue(__instance.tree, out MoveDestOriginalStatistic original) == false) {
          original = new MoveDestOriginalStatistic();
          original_movementCandidateLocations.Add(__instance.tree, original);
        }
        AIMinefieldHelper.FilterMoveCandidates(__instance.tree.unit,ref __instance.tree.movementCandidateLocations);
        FilterFastUnits(original, __instance, __instance.tree);
        Log.P?.WL(1, "filter result:" + __instance.tree.movementCandidateLocations.Count);
        //if (__instance.tree.unit.FlyingHeight() < 3f) { return; }
        //Log.P?.WL(1, "searching for non flying teammates");
        //HashSet<FlatPosition> nonflyingTeammates = new HashSet<FlatPosition>();
        //HashSet<MoveDestination> nonflyingTeammatesPositions = new HashSet<MoveDestination>();
        //foreach (var teammate in __instance.tree.unit.team.units) {
        //  if (__instance.tree.unit == teammate) { continue; }
        //  if (teammate.IsDead) { continue; }
        //  if (teammate.FlyingHeight() > 3f) { continue; }
        //  nonflyingTeammates.Add(new FlatPosition(teammate.CurrentPosition));
        //}
        //if (nonflyingTeammates.Count == 0) { return; }
        //foreach (var pos in __instance.tree.movementCandidateLocations) {
        //  FlatPosition fpos = new FlatPosition(pos.PathNode.Position);
        //  if (nonflyingTeammates.Contains(fpos)) { nonflyingTeammatesPositions.Add(pos); }
        //}
        //if (nonflyingTeammatesPositions.Count > 0) {
        //  __instance.tree.movementCandidateLocations = nonflyingTeammatesPositions.ToList();
        //}
        //Log.P?.WL(1, "non flying filter result:" + __instance.tree.movementCandidateLocations.Count);
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