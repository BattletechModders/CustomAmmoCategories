using BattleTech;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform) })]
  public static class ActorMovementSequence_InitDistanceClamp {
    private static Dictionary<AbstractActor, float> lastMoveDistance = new Dictionary<AbstractActor, float>();
    public static void Clear() {
      lastMoveDistance.Clear();
    }
    public static float LastMoveDistance(this AbstractActor unit) {
      if(lastMoveDistance.TryGetValue(unit,out float lmd)) { return lmd; }
      return 0f;
    }
    public static void Postfix(ActorMovementSequence __instance, AbstractActor actor, Transform xform) {
      Log.LogWrite("ActorMovementSequence.Init "+new Text(actor.DisplayName).ToString()+"\n");
      try {
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
        if (lastMoveDistance.ContainsKey(actor) == false){
          lastMoveDistance.Add(actor, costUsed);
        } else {
          lastMoveDistance[actor] = costUsed;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
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
        Log.LogWrite("PathNodeGrid.GetSampledPathNodes " + new Text(unit.DisplayName).ToString() + "\n");
        VehicleCustomInfo info = unit.GetCustomInfo();
        if (info == null) {
          Log.LogWrite(" no custom info\n");
          return;
        }
        if (info.Unaffected.MoveClamp < Core.Epsilon) {
          Log.LogWrite(" no clamp info\n");
          return;
        }
        MoveType moveType = (MoveType)f_moveType.GetValue(__instance);
        if (moveType == MoveType.Jumping) {
          Log.LogWrite(" jumping\n");
          return;
        }
        float distance = moveType == MoveType.Sprinting ? unit.MaxSprintDistance : unit.MaxWalkDistance;
        float min = unit.LastMoveDistance() - (info.Unaffected.MoveClamp * distance);
        float max = unit.LastMoveDistance() + (info.Unaffected.MoveClamp * distance);
        List<PathNode> result = new List<PathNode>();
        Log.LogWrite(" cheking nodes:"+__result.Count+" last distance: "+ unit.LastMoveDistance() + " speed:"+distance+" clamp:"+ info.Unaffected.MoveClamp + " min:"+min+" max:"+max+"\n");
        int less = 0;
        int greater = 0;
        foreach (PathNode node in __result) {
          if (node.CostToThisNode < min) { /*Log.LogWrite("  "+node.Position+" "+node.CostToThisNode+" less than "+min+"\n");*/ ++less; continue; }
          if (node.CostToThisNode > max) { /*Log.LogWrite("  " + node.Position + " " + node.CostToThisNode + " greater than " + max + "\n");*/ ++greater; continue; }
          //Log.LogWrite("  " + node.Position + " " + node.CostToThisNode + " ok\n");
          result.Add(node);
        }
        __result = result;
        Log.LogWrite(" after filtering:" + __result.Count + " less:"+less+" greater:"+greater+"\n");
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
  }

}