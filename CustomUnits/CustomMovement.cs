using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("MoveTowardDelta")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class ActorMovementSequence_MoveTowardDelta {
    public static Dictionary<ICombatant, CustomQuadLegController> customQuadAnimation = new Dictionary<ICombatant, CustomQuadLegController>();
    private static MethodInfo pVelocity = null;
    public static bool Prepare() {
      pVelocity = typeof(ActorMovementSequence).GetProperty("Velocity", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
      if(pVelocity == null) {
        Log.LogWrite("WARNING! Can't find ActorMovementSequence.Velocity seter\n",true);
        return false;
      }
      return true;
    }
    public static void RegisterMoveController(this CustomQuadLegController contr) {
      if (customQuadAnimation.ContainsKey(contr.parent) == false) {
        customQuadAnimation.Add(contr.parent, contr);
      }
    }
    public static void Velocity(this ActorMovementSequence sequence, Vector3 value) {
      if (pVelocity == null) { return; };
      //pVelocity.Invoke(sequence,new object[1] { (object)value });
    }
    public static void Postfix(ActorMovementSequence __instance, Vector3 delta) {
      return;
      if (customQuadAnimation.ContainsKey(__instance.owningActor)) {
        Vector3 velocity = __instance.Velocity;
        velocity *= customQuadAnimation[__instance.owningActor].Velocity();
        __instance.Velocity(velocity);
      }
    }
  }

}