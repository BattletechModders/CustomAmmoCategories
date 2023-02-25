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
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateSpline")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_UpdateSplineAnim {
    public static void Postfix(ActorMovementSequence __instance, Vector3 ___Forward, float ___t) {
      if (__instance.owningActor.NoMoveAnimation() == false) { return; }
      //Log.TWL(0, "ActorMovementSequence.UpdateSpline "+__instance.OwningActor.DisplayName+" forward:"+___Forward.normalized+" "+___Forward.magnitude+" velocity:"+__instance.Velocity.normalized+" "+__instance.Velocity.magnitude);
      Vector3 newPosition = __instance.owningActor.GameRep.thisTransform.position + __instance.Velocity * __instance.owningActor.Combat.StackManager.GetProgressiveDeltaTime(___t, __instance.isSpedUp);
      if (___Forward.sqrMagnitude > Core.Epsilon) {
        __instance.owningActor.GameRep.thisTransform.localRotation.SetLookRotation(___Forward);
      }
      __instance.owningActor.GameRep.thisTransform.position = newPosition;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("TurnParam")]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_TurnParam {
    public static bool Prefix(ActorMovementSequence __instance, float value) {
      if (__instance.owningActor.NoMoveAnimation() == false) { return true; }
      Log.TWL(0, "ActorMovementSequence.TurnParam " + __instance.OwningActor.DisplayName + " TurnParam:"+value);
      return false;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("ForwardParam")]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_ForwardParam {
    public static bool Prefix(ActorMovementSequence __instance, float value) {
      if (__instance.owningActor.NoMoveAnimation() == false) { return true; }
      Log.TWL(0, "ActorMovementSequence.ForwardParam " + __instance.OwningActor.DisplayName + " ForwardParam:" + value);
      return false;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("IsMovingParam")]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_IsMovingParam {
    public static bool Prefix(ActorMovementSequence __instance, bool value) {
      if (__instance.owningActor.NoMoveAnimation() == false) { return true; }
      Log.TWL(0, "ActorMovementSequence.IsMovingParam " + __instance.OwningActor.DisplayName + " IsMovingParam:" + value);
      return false;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("BeginMovementParam")]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_BeginMovementParam {
    public static bool Prefix(ActorMovementSequence __instance, bool value) {
      if (__instance.owningActor.NoMoveAnimation() == false) { return true; }
      Log.TWL(0, "ActorMovementSequence.BeginMovementParam " + __instance.OwningActor.DisplayName + " BeginMovementParam:" + value);
      return false;
    }
  }
}