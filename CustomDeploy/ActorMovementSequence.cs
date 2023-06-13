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
    private static Vector3 prev_CurrentPosition = Vector3.zero;
    private static Vector3 prev_CurrentRotation = Vector3.zero;
    private static int Sequence_hash = -1;
    private static bool move_started = false;
    public static void Prefix(bool __runOriginal, FollowActorCameraSequence __instance) {
      try {
        //__runOriginal = false; return;
        Vector3 curpos = Vector3.zero;
        Vector3 trgpos = Vector3.zero;
        float dist = 0f;
        float movedist = 0f;
        float rotspeed = 0f;
        if (__instance.timeSinceActorStoppedMoving >= __instance.Combat.Constants.CameraConstants.FollowCamDelayTime) {
          __runOriginal = false; return;
        }
        if (__instance.followTarget.MovingToPosition == null) {
          goto advice_time;
        }
        curpos = __instance.followTarget.CurrentPosition;
        Vector3 currot = __instance.followTarget.CurrentRotation.eulerAngles;
        curpos.y = 0f;
        trgpos = __instance.followTarget.MovingToPosition.Position;
        trgpos.y = 0f;
        if (Sequence_hash != __instance.GetHashCode()) {
          Log.TWL(0, $"FollowActorCameraSequence.CheckForFinished start {__instance.followTarget.PilotableActorDef.ChassisID} cur:{curpos} trg:{trgpos}");
          prev_CurrentPosition = curpos;
          prev_CurrentRotation = currot;
          move_started = false;
          Sequence_hash = __instance.GetHashCode();
        }
        dist = Vector3.Distance(curpos, trgpos);
        movedist = Vector3.Distance(curpos, prev_CurrentPosition);
        rotspeed = Vector3.Distance(currot, prev_CurrentRotation);
        prev_CurrentRotation = currot;
        prev_CurrentPosition = curpos;
        if ((move_started == false) && ((movedist > 0.001f) || (rotspeed > 0.001f))) {
          Log.TWL(0, $"FollowActorCameraSequence.CheckForFinished move started {__instance.followTarget.PilotableActorDef.ChassisID} cur:{curpos} trg:{trgpos} dist:{dist} move dist:{movedist} rotspeed:{rotspeed}");
          move_started = true;
        }
        if (dist < 0.1f) {
          goto advice_time;
        }
        if ((dist < (__instance.Combat.HexGrid.HexWidth / 2f)) || (move_started)) {
          if ((movedist < 0.001f) && (rotspeed < 0.001f)) {
            Log.TWL(0, $"FollowActorCameraSequence.CheckForFinished {__instance.followTarget.PilotableActorDef.ChassisID} cur:{curpos} trg:{trgpos} dist:{dist} move dist:{movedist} rotspeed:{rotspeed}");
            goto advice_time;
          }
        }
        __instance.timeSinceActorStoppedMoving = 0.0f;
        goto check_time;
      advice_time:
        __instance.timeSinceActorStoppedMoving += Time.deltaTime;
      check_time:
        if (__instance.timeSinceActorStoppedMoving <= __instance.Combat.Constants.CameraConstants.FollowCamDelayTime) {
          __runOriginal = false; return;
        }
        Log.TWL(0, $"FollowActorCameraSequence.CheckForFinished end {__instance.followTarget.PilotableActorDef.ChassisID} cur:{curpos} trg:{trgpos} dist:{dist} move dist:{movedist}");
        __instance.state = CameraSequence.CamSequenceState.Finished;
        __runOriginal = false; return;
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
        __instance.state = CameraSequence.CamSequenceState.Finished;
        __runOriginal = false; return;
      }
    }
  }
  [HarmonyPatch(typeof(FollowActorCameraSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Update")]
  [HarmonyPatch(new Type[] { })]
  public static class FollowActorCameraSequence_Update {
    public static void Finalizer(FollowActorCameraSequence __instance, Exception __exception) {
      if (__exception != null) {
        __exception = null;
        __instance.state = CameraSequence.CamSequenceState.Finished;
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnAdded")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_OnAdded {
    public static void Finalizer(ActorMovementSequence __instance, Exception __exception) {
      if (__exception != null) {
        Log.TWL(0, $"ActorMovementSequence.OnAdded {__instance.SequenceGUID} owner:{__instance.owningActor.PilotableActorDef.ChassisID}");
        Log.TWL(0,__exception.ToString(),true);
        ActorMovementSequence.logger.LogException(__exception);
      }
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
          Log.TWL(0, $"ActorMovementSequence.CompleteMove {__instance.SequenceGUID} owner:{__instance.owningActor.PilotableActorDef.ChassisID}");
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
        ActorMovementSequence.logger.LogException(e);
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
        ActorMovementSequence.logger.LogException(e);
      }
    }
  }

}