using BattleTech;
using FluffyUnderware.Curvy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomDeploy {
  [HarmonyPatch(typeof(FollowActorCameraSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("CheckForFinished")]
  [HarmonyPatch(new Type[] { })]
  public static class FollowActorCameraSequence_CheckForFinished {
    public static bool Prefix(FollowActorCameraSequence __instance) {
      if (__instance.timeSinceActorStoppedMoving >= __instance.Combat.Constants.CameraConstants.FollowCamDelayTime)
        return false;
      if(__instance.followTarget.MovingToPosition == null) {
        goto advice_time;
      }
      Vector3 curpos = __instance.followTarget.CurrentPosition;
      curpos.y = 0f;
      Vector3 trgpos = __instance.followTarget.MovingToPosition.Position;
      trgpos.y = 0f;
      float dist = Vector3.Distance(curpos, trgpos);
      //Log.TWL(0, $"FollowActorCameraSequence.CheckForFinished cur:{curpos} trg:{trgpos} dist:{dist}");
      if (dist < 0.1f) {
        goto advice_time;
      }
      __instance.timeSinceActorStoppedMoving = 0.0f;
      goto check_time;
    advice_time:
      __instance.timeSinceActorStoppedMoving += Time.deltaTime;
    check_time:
      if (__instance.timeSinceActorStoppedMoving <= __instance.Combat.Constants.CameraConstants.FollowCamDelayTime) {
        return false;
      }
      __instance.state = CameraSequence.CamSequenceState.Finished;
      return false;
    }
  }
  public static class ActorMovementSequence_CompleteMove {

    public static void Prefix(ActorMovementSequence __instance, Action<AbstractActor, Vector3, Vector3, ActorMovementSequence, bool, ICombatant> completeMoveDelegate) {
      try {
        if (__instance.OrdersAreComplete) {
          ActorMovementSequence.logger.LogWarning((object)"completeMove called but orders are complete!");
        } else {
          ActorMovementSequence.logger.Log((object)("MovementSequence completeMove: Current Position pre-Snap:  x: " + (object)__instance.MoverTransform.position.x + ", y: " + (object)__instance.MoverTransform.position.y + ", z: " + (object)__instance.MoverTransform.position.z + ", Current Heading:  x: " + (object)__instance.MoverTransform.forward.x + ", y: " + (object)__instance.MoverTransform.forward.y + ", z: " + (object)__instance.MoverTransform.forward.z + ", Current Rotation: " + (object)__instance.MoverTransform.rotation.eulerAngles.y));
          ActorMovementSequence.logger.Log((object)("MovementSequence completeMove will snap to: Final Position:  x: " + (object)__instance.FinalPos.x + ", y: " + (object)__instance.FinalPos.y + ", z: " + (object)__instance.FinalPos.z + ", Final Heading:  x: " + (object)__instance.FinalHeading.x + ", y: " + (object)__instance.FinalHeading.y + ", z: " + (object)__instance.FinalHeading.z + ", Final Rotation: " + (object)Quaternion.LookRotation(__instance.FinalHeading).eulerAngles.y));
          //__instance.MoverTransform.position = __instance.FinalPos;
          //__instance.MoverTransform.transform.rotation = Quaternion.LookRotation(__instance.FinalHeading, Vector3.up);
          //if (__instance.isVehicle)
          //  ActorMovementSequence.AlignVehicleToGround(__instance.MoverTransform, 100f);
          completeMoveDelegate(__instance.OwningActor, __instance.FinalPos, __instance.FinalHeading, __instance, __instance.playedMelee, __instance.meleeTarget);
          __instance.OwningActor.OnPositionUpdate(__instance.MoverTransform.position, __instance.MoverTransform.rotation, __instance.SequenceGUID, true, __instance.stickyMasks);
          ActorMovementSequence.logger.Log((object)("MovementSequence completeMove Validation Check: Adjusted Position:  x: " + (object)__instance.MoverTransform.position.x + ", y: " + (object)__instance.MoverTransform.position.y + ", z: " + (object)__instance.MoverTransform.position.z + ", Adjusted Heading:  x: " + (object)__instance.MoverTransform.forward.x + ", y: " + (object)__instance.MoverTransform.forward.y + ", z: " + (object)__instance.MoverTransform.forward.z + ", Adjusted Rotation: " + (object)__instance.MoverTransform.rotation.eulerAngles.y));
          __instance.RadialVelocity = 0.0f;
          __instance.Velocity = Vector3.zero;
          if (!__instance.IgnoreEndSmoothing) {
            __instance.ForwardParam = 0.0f;
            __instance.IsMovingParam = false;
          }
          __instance.TurnParam = 0.0f;
          __instance.PlayMeleeAnim();
          __instance.OrdersAreComplete = true;
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class ActorMovementSequence_UpdateSpline {
    public static void Prefix(ActorMovementSequence __instance, 
      Func<AbstractActor, Vector3, ActorMovementSequence, Vector3, float, ICombatant, object> updateSplineDelegate, 
      Action<AbstractActor, object, Transform, Vector3, float> updateRotationDelegate,
      Func<AbstractActor, object> createMoveContextDelegate
      ) {
      try {
        if (__instance.OrdersAreComplete)
          return;
        __instance.UpdateSplineWaypoint();
        object rayhit = createMoveContextDelegate(__instance.OwningActor);
        Vector3 currentWaypointPos = __instance.currentWaypointPos;
        if (__instance.onFinalSegment) {
          if (__instance.atFinalPos) {
            __instance.UpdateRadialVelocity(__instance.FinalHeading);
            __instance.Velocity = Vector3.zero;
          } else {
            Vector3 delta1 = __instance.currentWaypointPos - __instance.MoverTransform.position;
            delta1.Normalize();
            delta1.y = 0.0f;
            __instance.UpdateRadialVelocity(delta1);
            Vector3 delta2 = __instance.FinalPos - __instance.MoverTransform.position;
            if (!__instance.IgnoreEndSmoothing)
              __instance.DecelerateTowardDelta(delta2);
          }
        } else {
          if (__instance.onInitialSegment && !__instance.atInitialHeading) {
            __instance.UpdateRadialVelocity(__instance.initialHeading);
            //__instance.UpdateRotation();
            if (__instance.OrdersAreComplete == false) {
              updateRotationDelegate(__instance.OwningActor, rayhit, __instance.MoverTransform, __instance.Forward, __instance.t);
            }
            return;
          }
          __instance.MoveTowardWaypoint(currentWaypointPos);
        }
        __instance.Velocity = Vector3.ClampMagnitude(__instance.Velocity, __instance.MaxVelAdjusted);
        Vector3 velocity = __instance.Velocity;
        __instance.distanceMovedThisFrame = velocity.magnitude * __instance.deltaTime;
        __instance.distanceTravelled += __instance.distanceMovedThisFrame;
        __instance.t = __instance.spline.DistanceToTF(__instance.distanceTravelled, CurvyClamping.Clamp);
        if (__instance.t < 1.0f) {
          //Vector3 position = __instance.MoverTransform.position;
          Vector3 worldPos = __instance.spline.Interpolate(__instance.t);
          //float num = Mathf.Clamp(__instance.owningActor.Combat.MapMetaData.GetLerpedHeightAt(worldPos) - worldPos.y, -20f, 20f);
          //worldPos.y += num;
          //__instance.MoverTransform.position = worldPos;
          rayhit = updateSplineDelegate(__instance.OwningActor, worldPos,__instance, __instance.Forward, __instance.t, __instance.meleeTarget);
        }
        if (__instance.OwningActor.GameRep != null) {
          if (__instance.OwningActor.team.IsFriendly(__instance.OwningActor.Combat.LocalPlayerTeam) || __instance.OwningActor.Combat.LocalPlayerTeam.VisibilityToTarget(__instance.OwningActor) == VisibilityLevel.LOSFull) {
            velocity = __instance.Velocity;
            __instance.throttleAudioValue = (float)((double)velocity.magnitude / (double)__instance.MaxVelAdjusted * 100.0);
            WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, __instance.throttleAudioValue, __instance.OwningActor.GameRep.audioObject);
          } else {
            __instance.throttleAudioValue = 0.0f;
            WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, __instance.throttleAudioValue, __instance.OwningActor.GameRep.audioObject);
          }
        }
        //__instance.UpdateRotation();
        if (__instance.OrdersAreComplete == false) {
          updateRotationDelegate(__instance.OwningActor, rayhit, __instance.MoverTransform, __instance.Forward, __instance.t);
        }
        if (!__instance.atFinalPosAndHeading)
          return;
        __instance.CompleteMove();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }

}