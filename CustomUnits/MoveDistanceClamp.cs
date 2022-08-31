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
using CustAmmoCategoriesPatches;
using Harmony;
using InControl;
using Localize;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform) })]
  public static class ActorMovementSequence_InitDistanceClamp {
    //private static Dictionary<AbstractActor, float> lastMoveDistance = new Dictionary<AbstractActor, float>();
    //public static void Clear() {
      //lastMoveDistance.Clear();
    //}
    //public static float LastMoveDistance(this AbstractActor unit) {
      //if(lastMoveDistance.TryGetValue(unit,out float lmd)) { return lmd; }
      //return 0f;
    //}
    public static void Postfix(ActorMovementSequence __instance, AbstractActor actor, Transform xform) {
      Log.TWL(0,"ActorMovementSequence.Init "+new Text(actor.DisplayName).ToString());
      try {
        Log.WL(1, "Pathing.HasPath:"+(actor.Pathing==null?"null":actor.Pathing.HasPath.ToString()));
        if (actor.Pathing == null) { return; }
        if (actor.Pathing.HasPath == false) { return; }
        float costUsed = actor.Pathing.MaxCost - actor.Pathing.CostLeft;
        Log.LogWrite(" path:" + actor.Pathing.CurrentPath.Count + " max:"+ actor.Pathing.MaxCost + " left:"+actor.Pathing.CostLeft+" used:"+costUsed+"\n");
        foreach (PathNode pn in actor.Pathing.CurrentPath) {
          Log.LogWrite("  " + pn.Position + " "+pn.CostToThisNode+"\n");
        }
        PathNode node = actor.Pathing.CurrentPath[actor.Pathing.CurrentPath.Count - 1];
        float distance = node.CostToThisNode;
        if(distance < Core.Epsilon) {
          distance = Vector3.Distance(actor.Pathing.ResultDestination,actor.CurrentPosition);
          Log.LogWrite("  overriding distance\n");
        }
        Log.LogWrite(" distance:"+ distance + "\n");
        actor.LastMoveDistance(costUsed);
        //if (lastMoveDistance.ContainsKey(actor) == false){
        //  lastMoveDistance.Add(actor, costUsed);
        //} else {
        //  lastMoveDistance[actor] = costUsed;
        //}
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProcessMousePos")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateMove_ProcessMousePos {
    public static string MoveTypeTextPathing(this AbstractActor unit) {
      if (unit.Pathing == null) { return "<color=red>NO PATHING</color>"; }
      if (unit.Pathing.ArePathGridsComplete == false) { return "<color=orange>CALCULATING</color>"; }
      MoveType moveType = unit.Pathing.MoveType;
      float distance = (moveType == MoveType.Sprinting ? unit.MaxSprintDistanceInital() : unit.MaxWalkDistanceInital());
      float realdistance = unit.CurrentPathingMoveLength();
      if (realdistance < 0f) {
        realdistance = 0f;
        if(unit.Pathing.CurrentPath != null) {
          if(unit.Pathing.CurrentPath.Count != 0) {
            if(unit.Pathing.CurrentPath[unit.Pathing.CurrentPath.Count - 1] != null) {
              realdistance = unit.Pathing.CurrentPath[unit.Pathing.CurrentPath.Count - 1].CostToThisNode;
            }
          }
        }
      }
      unit.MoveClamp(distance,out float min,out float max);
      max = Mathf.Round(max);
      min = Mathf.Round(min);
      if (realdistance < min) { return "<color=red>MIN DIST. " + realdistance + " < "+min+"</color>"; }
      //if (realdistance > max) { return "<color=red>MAX DIST. " + realdistance + " > " + max + "</color></color>"; }
      return string.Empty;
    }
    public static void Postfix(SelectionStateMove __instance, ref Vector3 worldPos) {
      try {
        string text = __instance.SelectedActor.MoveTypeTextPathing();
        if (string.IsNullOrEmpty(text)) {
          __instance.HUD().ClearMoveTypeText();
        } else {
          __instance.HUD().SetExMoveTypeText(text);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProcessLeftClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateMove_ProcessLeftClickClamp {
    public static bool Prefix(SelectionStateMove __instance, ref Vector3 worldPos, ref bool __result) {
      try {
        __instance.SelectedActor.Pathing.Update(worldPos, true);
        string text = __instance.SelectedActor.MoveTypeTextPathing();
        if (string.IsNullOrEmpty(text)) {
          __instance.HUD().ClearMoveTypeText();
          return true;
        } else {
          __instance.HUD().SetExMoveTypeText(text);
          __result = false;
          MoveType moveType = __instance.SelectedActor.Pathing.MoveType;
          float distance = (moveType == MoveType.Sprinting ? __instance.SelectedActor.MaxSprintDistanceInital() : __instance.SelectedActor.MaxWalkDistanceInital());
          GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Message: "+text+"\n"+"MoveType: "+moveType+"\nRealMaxDistance:"+distance+"\nClamp:"+ __instance.SelectedActor.MoveClamp()+"\nLastMoveDist:"+ __instance.SelectedActor.LastMoveDistance());
          popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
          popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();

          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return false;
      }
    }
    public static void Postfix(SelectionStateMove __instance, ref Vector3 worldPos) {
      try {
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoveBase))]
  [HarmonyPatch("OnInactivate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMove_UpdateMousePosUI {
    public static void Postfix(SelectionStateMove __instance) {
      try {
        __instance.HUD().ClearMoveTypeText();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MaxWalkDistance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MaxWalkDistance {
    public static void MoveClamp(this AbstractActor unit,float distance,out float min, out float max) {
      min = 0f; max = distance;
      float MoveClamp = unit.MoveClamp();
      if (MoveClamp < Core.Epsilon) { return; }
      float last_move = unit.LastMoveDistance();
      if (last_move > distance) { last_move = distance; };
      if (last_move < Core.Epsilon) { last_move = 0f; };
      min = last_move - ((MoveClamp / 2f) * distance);
      max = last_move + ((MoveClamp / 2f) * distance);
      if (min < 0f) { max -= min; min = 0; }
      if (max > distance) { min -= (max - distance); max = distance; }
      if (min < 0f) { min = 0f; }
      if (max > distance) { max = distance; }
    }
    public static float MaxSprintDistanceInital(this AbstractActor unit) {
      Mech mech = unit as Mech;
      if (mech != null) {
        TrooperSquad squad = unit as TrooperSquad;
        if (squad == null) {
          if (mech.FakeVehicle()) {
            return mech.RunSpeed;
          } else {
            if (IRBTModUtils.Mod.Config.Features.EnableMovementModifiers == false) {
              return mech.RunSpeed * Traverse.Create(mech).Property<float>("MoveMultiplier").Value;
            } else {
              return IRBTModUtils.Extension.MechExtensions.ModifiedRunDistanceExt(mech, false, "CustomUnits");
            }
          }
        } else {
          return mech.RunSpeed;
        }
      }
      Vehicle vehicle = unit as Vehicle;
      if (vehicle != null) { return vehicle.FlankSpeed * Traverse.Create(vehicle).Property<float>("MoveMultiplier").Value; }
      return 0f;
    }
    public static float MaxWalkDistanceInital(this AbstractActor unit) {
      Mech mech = unit as Mech;
      if (mech != null) {
        TrooperSquad squad = unit as TrooperSquad;
        if (squad == null) {
          if (mech.FakeVehicle()) {
            return mech.WalkSpeed;
          } else {
            if (IRBTModUtils.Mod.Config.Features.EnableMovementModifiers == false) {
              return mech.WalkSpeed * Traverse.Create(mech).Property<float>("MoveMultiplier").Value;
            } else {
              return IRBTModUtils.Extension.MechExtensions.ModifiedWalkDistanceExt(mech, false, "CustomUnits");
            }
          }
        } else {
          return mech.WalkSpeed;
        }
      }
      Vehicle vehicle = unit as Vehicle;
      if (vehicle != null) { return vehicle.CruiseSpeed * Traverse.Create(vehicle).Property<float>("MoveMultiplier").Value; }
      return 0f;
    }
    public static float MaxSprintDistanceMod(Mech mech, float value) {
      //Log.TW(0, "MaxSprintDistanceMod " + mech.Description.Id + " value:" + value + " lastMove:" + mech.LastMoveDistance() + " spent:" + mech.PartialMovementSpent()+" clamp:"+ mech.MoveClamp()+" restMod:"+ mech.RestPathingModifier());
      if (mech.MoveClamp() > Core.Epsilon) {
        mech.MoveClamp(value, out float min, out float max);
        value = max;
      } else {
        value = value * mech.RestPathingModifier();
      }
      //Log.WL(1, "value:" + value);
      return value;
    }
    public static float MaxWalkDistanceMod(Mech mech, float value) {
      //Log.TW(0, "MaxWalkDistanceMod " + mech.Description.Id + " value:" + value + " lastMove:" + mech.LastMoveDistance() + " spent:" + mech.PartialMovementSpent() + " clamp:" + mech.MoveClamp() + " restMod:" + mech.RestPathingModifier());
      if (mech.MoveClamp() > Core.Epsilon) {
        mech.MoveClamp(value, out float min, out float max);
        value = max;
      } else {
        value = value * mech.RestPathingModifier();
      }
      //Log.WL(1, "value:" + value);
      return value;
    }
    public static float MaxBackwardDistanceMod(Mech mech, float value) {
      //Log.TW(0, "MaxBackwardDistanceMod " + mech.Description.Id + " value:" + value + " lastMove:" + mech.LastMoveDistance() + " spent:" + mech.PartialMovementSpent() + " clamp:" + mech.MoveClamp() + " restMod:" + mech.RestPathingModifier());
      if (mech.MoveClamp() > Core.Epsilon) {
        mech.MoveClamp(value, out float min, out float max);
        value = max;
      } else {
        value = value * mech.RestPathingModifier();
      }
      //Log.WL(1, "value:" + value);
      return value;
    }
    public static float MaxMeleeEngageRangeDistanceMod(Mech mech, float value) {
      //Log.TW(0, "MaxMeleeEngageRangeDistanceMod " + mech.Description.Id + " value:" + value + " lastMove:" + mech.LastMoveDistance() + " spent:" + mech.PartialMovementSpent() + " clamp:" + mech.MoveClamp() + " restMod:" + mech.RestPathingModifier());
      if (mech.MoveClamp() > Core.Epsilon) {
        mech.MoveClamp(value, out float min, out float max);
        value = max;
      } else {
        value = value * mech.RestPathingModifier();
      }
      //Log.WL(1, "value:" + value);
      return value;
    }
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (IRBTModUtils.Mod.Config.Features.EnableMovementModifiers) { return; }
        if (__instance.MoveClamp() > Core.Epsilon) {
          __instance.MoveClamp(__result, out float min, out float max);
          __result = max;
        } else {
          __result = __result * __instance.RestPathingModifier();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MaxSprintDistance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MaxSprintDistance {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (IRBTModUtils.Mod.Config.Features.EnableMovementModifiers) { return; }
        Log.TW(0, "Mech.MaxSprintDistance " + __instance.Description.Id + " inital:" + __result + " clamp:" + __instance.MoveClamp()+ " RestPathingModifier:" + __instance.RestPathingModifier());
        if (__instance.MoveClamp() > Core.Epsilon) {
          __instance.MoveClamp(__result, out float min, out float max);
          __result = max;
        } else {
          __result = __result * __instance.RestPathingModifier();
        }
        Log.WL(1, "result:" + __result);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("MaxWalkDistance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Vehicle_MaxWalkDistance {
    public static void Postfix(Vehicle __instance, ref float __result) {
      try {
        if (__instance.MoveClamp() > Core.Epsilon) {
          __instance.MoveClamp(__result, out float min, out float max);
          __result = max;
        } else {
          __result = __result * __instance.RestPathingModifier();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("MaxSprintDistance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Vehicle_MaxSprintDistance {
    public static void Postfix(Vehicle __instance, ref float __result) {
      try {
        if (__instance.MoveClamp() > Core.Epsilon) {
          __instance.MoveClamp(__result, out float min, out float max);
          __result = max;
        } else {
          __result = __result * __instance.RestPathingModifier();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(JumpPathing))]
  [HarmonyPatch("IsValidLandingSpot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(List<AbstractActor>) })]
  public static class JumpPathing_JumpPathing {
    public static void Postfix(JumpPathing __instance, Vector3 worldPos, List<AbstractActor> allActors, ref bool __result) {
      try {
        if(__result == true) {
          float distance = Vector3.Distance(worldPos, Traverse.Create(__instance).Property<Mech>("Mech").Value.CurrentPosition);
          float maxjump = Traverse.Create(__instance).Property<Mech>("Mech").Value.JumpDistance;
          float minJump = Traverse.Create(__instance).Property<Mech>("Mech").Value.MinJumpDistance();
          if (minJump <= 0f) { return; }
          if (minJump >= 1f) { return; }
          __result = distance >= maxjump * minJump;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class MoveClampHelper {
    private static Dictionary<AbstractActor, float> moveClampCache = new Dictionary<AbstractActor, float>();
    private static Dictionary<AbstractActor, float> minMoveDistance = new Dictionary<AbstractActor, float>();
    private static Dictionary<AbstractActor, float> minJumpDistance = new Dictionary<AbstractActor, float>();
    public static void Clear() { moveClampCache.Clear(); minMoveDistance.Clear(); minJumpDistance.Clear(); }
    public static void Clear(AbstractActor unit) { moveClampCache.Remove(unit); minMoveDistance.Remove(unit); minJumpDistance.Remove(unit);  }
    public static float MoveClamp(this AbstractActor unit) {
      if (moveClampCache.TryGetValue(unit, out float result)) { return result; }
      UnitCustomInfo info = unit.GetCustomInfo();
      if (info == null) { moveClampCache.Add(unit, 0f); minMoveDistance.Add(unit, 0f); minJumpDistance.Add(unit, 0f); return 0f; }
      if (unit.GameRep == null) { return 0f; }
      AlternateMechRepresentations altReps = unit.GameRep.GetComponent<AlternateMechRepresentations>();
      if (altReps == null) { moveClampCache.Add(unit, info.Unaffected.MoveClamp); minMoveDistance.Add(unit, 0f); minJumpDistance.Add(unit, info.Unaffected.MinJumpDistance); return info.Unaffected.MoveClamp; }
      result = altReps.MoveClamp;
      if (result < Core.Epsilon) { result = info.Unaffected.MoveClamp; }
      moveClampCache.Add(unit, result);
      return result;
    }
    public static float MinJumpDistance(this AbstractActor unit) {
      if (minJumpDistance.TryGetValue(unit, out float result)) { return result; }
      UnitCustomInfo info = unit.GetCustomInfo();
      if (info == null) { minJumpDistance.Add(unit, 0f); minJumpDistance.Add(unit, 0f); minJumpDistance.Add(unit, 0f); return 0f; }
      if (unit.GameRep == null) { return 0f; }
      AlternateMechRepresentations altReps = unit.GameRep.GetComponent<AlternateMechRepresentations>();
      if (altReps == null) { moveClampCache.Add(unit, info.Unaffected.MoveClamp); minMoveDistance.Add(unit, 0f); minJumpDistance.Add(unit, info.Unaffected.MinJumpDistance); return info.Unaffected.MinJumpDistance; }
      result = altReps.MinJumpDistance;
      if (result < Core.Epsilon) { result = info.Unaffected.MinJumpDistance; }
      minJumpDistance.Add(unit, result);
      return result;
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetSampledPathNodes")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class PathNodeGrid_GetSampledPathNodes {
    private static FieldInfo f_owningActor = typeof(PathNodeGrid).GetField("owningActor", BindingFlags.Instance|BindingFlags.NonPublic);
    private static FieldInfo f_moveType = typeof(PathNodeGrid).GetField("moveType", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Postfix(PathNodeGrid __instance, ref List<PathNode> __result) {
      try {
        AbstractActor unit = (AbstractActor)f_owningActor.GetValue(__instance);
        Log.TWL(0,"PathNodeGrid.GetSampledPathNodes " + unit.PilotableActorDef.ChassisID);
        float MoveClamp = unit.MoveClamp();
        if (MoveClamp < Core.Epsilon) {
          Log.WL(1,"no clamp info");
          return;
        }
        MoveType moveType = (MoveType)f_moveType.GetValue(__instance);
        if (moveType != MoveType.Jumping) {
          float distance = (moveType == MoveType.Sprinting ? unit.MaxSprintDistanceInital() : unit.MaxWalkDistanceInital());
          unit.MoveClamp(distance,out float min, out float max);
          List<PathNode> result = new List<PathNode>();
          Log.WL(1,"checking nodes:" + __result.Count + " last distance: " + unit.LastMoveDistance() + " speed:" + distance + " clamp:" + MoveClamp + " min:" + min + " max:" + max);
          int less = 0;
          int greater = 0;
          foreach (PathNode node in __result) {
            if (node.CostToThisNode < min) { /*Log.LogWrite("  "+node.Position+" "+node.CostToThisNode+" less than "+min+"\n");*/ ++less; continue; }
            //if (node.CostToThisNode > max) { /*Log.LogWrite("  " + node.Position + " " + node.CostToThisNode + " greater than " + max + "\n");*/ ++greater; continue; }
            //Log.LogWrite("  " + node.Position + " " + node.CostToThisNode + " ok\n");
            result.Add(node);
          }
          __result = result;
          Log.WL(1,"after filtering:" + __result.Count + " less:" + less + " greater:" + greater);
        }
        if (unit.UnaffectedPathing()) {
          SortedDictionary<float, List<PathNode>> result = new SortedDictionary<float, List<PathNode>>();
          SortedSet<float> distances = new SortedSet<float>();
          foreach (PathNode node in __result) {
            if(result.TryGetValue(node.CostToThisNode,out List<PathNode> nodes) == false) {
              nodes = new List<PathNode>();
              result.Add(node.CostToThisNode, nodes);
              distances.Add(node.CostToThisNode);
            }
            nodes.Add(node);
          }
          float distanceDiff = (distances.Max - distances.Min) / 10f;
          float distanceDelta = (distances.Max - distances.Min) / 40f;
          Log.WL(1,"distance diff:" + distanceDiff);
          float lastUsedDistance = distances.Min;
          __result.Clear();
          foreach (float distance in distances) {
            float diff = Math.Abs(distance - lastUsedDistance);
            if (diff < distanceDelta) { __result.AddRange(result[distance]); continue; }
            if (diff >= distanceDiff) { lastUsedDistance = distance; __result.AddRange(result[distance]); continue; }
            if (Math.Abs(distance - distances.Min) < Core.Epsilon){ lastUsedDistance = distance; __result.AddRange(result[distance]); break; }
          }
          Log.WL(1,"after filtering:" + __result.Count);
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
  }

}