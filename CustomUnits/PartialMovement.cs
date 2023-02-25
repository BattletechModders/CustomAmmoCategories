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
using BattleTech;
using BattleTech.UI;
using CustAmmoCategories;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class StoredPathing {
    public AbstractActor owner { get; set; }
    public List<PathNode> Path { get; set; }
    public float FullInitalMovement { get; set; }
    public float FullMovementSpent {
      get {
        return owner.PartialMovementSpent();
      }
    }
    public StoredPathing(AbstractActor o) {
      this.owner = o;
      Path = new List<PathNode>();
      FullInitalMovement = o.Pathing.MaxCost;
      //FullMovementSpent = 0f;
    }
  }
  public static class PathingHelper {
    private static Dictionary<AbstractActor, StoredPathing> pathingStorage = new Dictionary<AbstractActor, StoredPathing>();
    public static bool HasStoredPathing(this AbstractActor unit) {
      return pathingStorage.ContainsKey(unit);
    }
    public static void ClearStoredPathing(this AbstractActor unit) {
      Log.TWL(0, "PathingHelper.ClearStoredPathing "+unit.DisplayName);
      unit.PartialMovementSpent(0f);
      pathingStorage.Remove(unit);
    }
    public static void Clear() {
      pathingStorage.Clear();
    }
    public static float MaxMoveDistance(this AbstractActor unit) {
      if (pathingStorage.TryGetValue(unit, out StoredPathing spath) == false) {
        return unit.Pathing.MaxCost;
      };
      return spath.FullInitalMovement;
    }
    public static float MoveCostLeft(this AbstractActor unit) {
      if (pathingStorage.TryGetValue(unit, out StoredPathing spath) == false) {
        return unit.Pathing.CostLeft;
      };
      return unit.Pathing.CostLeft;
    }
    public static float RestPathingModifier(this AbstractActor unit) {
      if (pathingStorage.TryGetValue(unit, out StoredPathing spath) == false) {
        return 1f;
      };
      if (spath.FullInitalMovement < Core.Epsilon) { return 1f; }
      return (spath.FullInitalMovement - spath.FullMovementSpent) / spath.FullInitalMovement;
    }
    public static float CurrentPathingMoveLength(this AbstractActor unit) {
      if (pathingStorage.TryGetValue(unit, out StoredPathing spath) == false) {
        return (unit.Pathing.MaxCost - unit.Pathing.CostLeft);
      };
      return spath.FullInitalMovement - spath.FullMovementSpent - unit.Pathing.CostLeft;
    }
    public static void UpdateWaypoint(this AbstractActor unit, bool justStoodUp) {
      if (unit.Pathing == null) { return; }
      if (unit.Pathing.CostLeft < Core.Settings.PartialMovementGuardDistance) { return; }
      if (pathingStorage.TryGetValue(unit, out StoredPathing spath) == false) {
        spath = new StoredPathing(unit);
        spath.owner = unit;
        pathingStorage.Add(unit, spath);
      };
      Log.TWL(0, "PathingHelper.UpdateWaypoint "+unit.DisplayName + " " + unit.CurrentPosition);
      spath.Path.Clear();
      if(unit.Pathing.CurrentPath != null) spath.Path.AddRange(unit.Pathing.CurrentPath);
      unit.PartialMovementSpent(unit.PartialMovementSpent() + (unit.Pathing.MaxCost - unit.Pathing.CostLeft));
      foreach (PathNode node in spath.Path) { Log.WL(1,node.Position.ToString()); }
      Log.WL(1,"rest path: "+unit.RestPathingModifier()+ " MaxCost:" + unit.Pathing.MaxCost+ " CostLeft:"+ unit.Pathing.CostLeft+ " PartialMovementSpent:" + unit.PartialMovementSpent());
      unit.Pathing.ResetPathGrid(unit.Pathing.ResultDestination, unit.Pathing.ResultAngle, unit, justStoodUp);
      unit.Combat.PathingManager.AddPathing(unit.Pathing);
    }
    public static void PrependStoredPath(this Pathing pathing, ref List<PathNode> CurrentPath) {
      if(pathingStorage.TryGetValue(pathing.OwningActor, out StoredPathing spath)) {
        if (CurrentPath == null) {
          if (spath.Path.Count > 0) { CurrentPath = new List<PathNode>(spath.Path); }
        } else {
          CurrentPath.InsertRange(0, spath.Path);
        }
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ResetPathing")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Pathing_ResetPathGrid {
    public static void Prefix(AbstractActor __instance, bool justStoodUp) {
      Log.TWL(0, "AbstractActor.ResetPathing " + __instance.DisplayName);
      __instance.ClearStoredPathing();
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("CompleteMove")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_CompleteMovePartial {
    public static void Postfix(ActorMovementSequence __instance) {
      __instance.owningActor.ClearStoredPathing();
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("ResetPathGridIfTouching")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<Rect>), typeof(Vector3), typeof(float), typeof(AbstractActor) })]
  public static class Pathing_ResetPathGridIfTouchingPartial {
    public static void Prefix(Pathing __instance, AbstractActor actor) {
      if (actor == null) { return; }
      try {
        Log.TWL(0, "Pathing.ResetPathGridIfTouching " + actor.DisplayName);
        actor.ClearStoredPathing();
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("UpdateCurrentPath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Pathing_UpdateCurrentPath {
    private static Vector3 resultDest = Vector3.zero;
    public static void Postfix(Pathing __instance, bool calledFromUI) {
      try {
        if (__instance == null) { return; }
        float dist = Vector3.Distance(__instance.ResultDestination, resultDest);
        bool show = false;//dist > 1f;
        resultDest = __instance.ResultDestination;
        if (show) Log.TWL(0, "Pathing.UpdateCurrentPath " + __instance.OwningActor.DisplayName + " " + __instance.OwningActor.CurrentPosition + " origin:" + __instance.CurrentGrid.WorldPosOrigin);
        if (show && (__instance.CurrentPath != null)) foreach (PathNode node in __instance.CurrentPath) { Log.WL(1, node.Position.ToString()); }
        if (show) Log.WL(1, "dest:" + __instance.ResultDestination);
        __instance.PrependStoredPath(ref __instance.CurrentPath);
        if (show) {
          Log.WL(1, "fullpath:");
          if (__instance.CurrentPath != null) foreach (PathNode node in __instance.CurrentPath) { Log.WL(2, node.Position.ToString()); }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("UpdateMeleePath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Pathing_UpdateMeleePath {
    private static Vector3 resultDest = Vector3.zero;
    public static void Postfix(Pathing __instance, bool calledFromUI) {
      try {
        if (__instance == null) { return; }
        float dist = Vector3.Distance(__instance.ResultDestination, resultDest);
        bool show = (dist > 1f) && (__instance.CurrentPath != null);
        resultDest = __instance.ResultDestination;
        if (show) Log.TWL(0, "Pathing.UpdateMeleePath " + __instance.OwningActor.DisplayName + " " + __instance.OwningActor.CurrentPosition + " origin:" + __instance.CurrentGrid.WorldPosOrigin);
        if (show && (__instance.CurrentPath != null)) foreach (PathNode node in __instance.CurrentPath) { Log.WL(1, node.Position.ToString()); }
        if (show) Log.WL(1, "dest:"+__instance.ResultDestination.ToString());
        __instance.PrependStoredPath(ref __instance.CurrentPath);
        if (show) {
          Log.WL(1, "fullpath:");
          foreach (PathNode node in __instance.CurrentPath) { Log.WL(2,node.Position.ToString()); }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(typeof(Pathing))]
  //[HarmonyPatch("UpdateLockedPath")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool), typeof(float) })]
  //public static class Pathing_UpdateLockedPath {
  //  private static Vector3 resultDest = Vector3.zero;
  //  public static void Postfix(Pathing __instance, Vector3 destination, Vector3 lookTarget, bool calledFromUI, float lookTargetInfluence) {
  //    float dist = Vector3.Distance(__instance.ResultDestination, resultDest);
  //    bool show = dist > 1f;
  //    resultDest = __instance.ResultDestination;
  //    if (show) Log.TWL(0, "Pathing.UpdateLockedPath " + __instance.OwningActor.DisplayName + " " + __instance.OwningActor.CurrentPosition + " origin:" + __instance.CurrentGrid.WorldPosOrigin);
  //    if (show && (__instance.CurrentPath != null)) foreach (PathNode node in __instance.CurrentPath) { Log.W(1,  node.Position.ToString()); }
  //    if (show) Log.WL(1, "dest:" + __instance.ResultDestination);
  //    __instance.PrependStoredPath(ref __instance.CurrentPath);
  //    if (show) {
  //      Log.WL(1, "fullpath:");
  //      foreach (PathNode node in __instance.CurrentPath) { Log.WL(2, node.Position.ToString()); }
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProcessLeftClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateMove_ProcessLeftClickPosPartial {
    public static bool Prefix(SelectionStateMove __instance, ref Vector3 worldPos, ref bool __result) {
      try {
        if (__instance.HasDestination) { return true; }
        if (__instance.HasTarget) { return true; }
        if (__instance.Orders != null || !__instance.SelectedActor.Pathing.ArePathGridsComplete || __instance.SelectedActor.HasFiredThisRound && !__instance.SelectedActor.CanMoveAfterShooting) { return true; }
        if (Input.GetKey(KeyCode.LeftShift) == false) { return true; }
        if(Core.Settings.AllowPartialMove == false) { __result = false; return false; }
        if (__instance.SelectedActor.AllowPartialMovement() == false) { __result = false; return false; }
        if (__instance.SelectedActor.MoveClamp() > Core.Epsilon) { __result = false; return false; }
        if (__instance.SelectedActor.Pathing.MoveType == MoveType.Sprinting) {
          if (Core.Settings.AllowPartialSprint == false) { __result = false; return false; }
          if (__instance.SelectedActor.AllowPartialSprint() == false) { __result = false; return false; }
        }
        __instance.SelectedActor.UpdateWaypoint(__instance.SelectedActor.StoodUpThisRound);
        __result = false;
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return false;
      }
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("LockToRotateInPlace")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Pathing_LockToRotateInPlace {
    public static bool Prefix(Pathing __instance, bool calledFromUI) {
      try {
        Traverse.Create(__instance).Property<AbstractActor>("CurrentMeleeTarget").Value = (AbstractActor)null;
        __instance.UnlockPosition();
        Traverse.Create(__instance).Field<MoveType>("moveType").Value = MoveType.Walking;
        __instance.CurrentDestination = __instance.CurrentGrid.WorldPosOrigin;
        __instance.UpdateCurrentPath(calledFromUI);
        __instance.LockPosition();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return false;
      }
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("OnAddToStack")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMoveBase_OnAddToStack {
    public static void Postfix(SelectionState __instance) {
      try {
        if (__instance is SelectionStateMoveBase movebase) {
          Log.TWL(0, "SelectionStateMoveBase.OnAddToStack " + movebase.SelectedActor.DisplayName + " gridOrigin:" + movebase.SelectedActor.Pathing.CurrentGrid.WorldPosOrigin + " pos:" + movebase.SelectedActor.CurrentPosition);
          if (movebase.SelectedActor.Pathing.CurrentGrid.WorldPosOrigin != __instance.SelectedActor.CurrentPosition) {
            movebase.SelectedActor.ClearStoredPathing();
            movebase.SelectedActor.ResetPathing(__instance.SelectedActor.StoodUpThisRound);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoveBase))]
  [HarmonyPatch("CanBackOut")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMoveBase_CanBackOut {
    public static void Postfix(SelectionStateMoveBase __instance, ref bool __result) {
      try {
        if (__result == true) { return; }
        if (__instance.SelectedActor.HasStoredPathing()) { __result = true; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(JumpPathing))]
  [HarmonyPatch("UpdateCurrentPath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class JumpPathing_UpdateCurrentPath {
    public static void Prefix(JumpPathing __instance, ref float __state) {
      try {
        if (Traverse.Create(__instance).Property<Mech>("Mech").Value.AllowRotateWhileJump()) { return; }
        if (__instance.IsLockedToDest) { __state = __instance.ResultAngle; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(JumpPathing __instance, ref float __state) {
      try {
        if (Traverse.Create(__instance).Property<Mech>("Mech").Value.AllowRotateWhileJump()) { return; }
        if (__instance.IsLockedToDest) { __instance.ResultAngle = __state; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoveBase))]
  [HarmonyPatch("BackOut")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMoveBase_BackOut {
    public static void Postfix(SelectionStateMoveBase __instance) {
      try {
        Log.TWL(0, "SelectionStateMoveBase.BackOut "+__instance.SelectedActor.DisplayName+" gridOrigin:"+ __instance.SelectedActor.Pathing.CurrentGrid.WorldPosOrigin+" pos:"+ __instance.SelectedActor.CurrentPosition);
        if(__instance.SelectedActor.Pathing.CurrentGrid.WorldPosOrigin != __instance.SelectedActor.CurrentPosition) {
          __instance.SelectedActor.ClearStoredPathing();
          __instance.SelectedActor.ResetPathing(__instance.SelectedActor.StoodUpThisRound);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}