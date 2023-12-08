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
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.UI;
using CustAmmoCategories;
using HarmonyLib;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
  public static class ActorMovementSequence_MoveTowardDelta {
    public static Dictionary<ICombatant, CustomQuadLegController> customQuadAnimation = new Dictionary<ICombatant, CustomQuadLegController>();
    public static void RegisterMoveController(this CustomQuadLegController contr) {
      if (customQuadAnimation.ContainsKey(contr.parent) == false) {
        customQuadAnimation.Add(contr.parent, contr);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateSpline")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_UpdateSplineSquad {
    public static object UpdateSplineDelegate(AbstractActor unit, Vector3 worldPos, ActorMovementSequence seq, Vector3 forward, float t, ICombatant meleeTarget) {
      try {
        return (unit.GameRep as CustomMechRepresentation).UpdateSpline(worldPos, seq, forward, t, meleeTarget);
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        ActorMovementSequence.logger.LogException(e);
        return null;
      }
    }
    public static void UpdateRotationDelegate(AbstractActor unit, object raycast, Transform moveTransform, Vector3 forward, float t) {
      try {
        (unit.GameRep as CustomMechRepresentation).UpdateRotation(raycast, moveTransform, forward, t);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
    public static object UpdateRotationDelegate(AbstractActor unit) {
      try {
        return (unit.GameRep as CustomMechRepresentation).createMoveContext();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
        return null;
      }
    }
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.ActorRep is CustomMechRepresentation custRep) {
          CustomDeploy.ActorMovementSequence_UpdateSpline.Prefix(__instance, UpdateSplineDelegate, UpdateRotationDelegate, UpdateRotationDelegate);
          __runOriginal = false; return;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("CompleteMove")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_CompleteMove {
    public static void CompleteMoveDelegate(AbstractActor unit, Vector3 finalPos, Vector3 finalHeading, ActorMovementSequence seq, bool playedMelee, ICombatant meleeTarget) {
      try {
        (unit.GameRep as CustomMechRepresentation).CompleteMove(finalPos, finalHeading, seq, playedMelee, meleeTarget);
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.ActorRep is CustomMechRepresentation custRep) {
          CustomDeploy.ActorMovementSequence_CompleteMove.Prefix(__instance, CompleteMoveDelegate);
          __runOriginal = false; return;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
    public static void Postfix(ActorMovementSequence __instance) {
      Log.Combat?.TWL(0, "ActorMovementSequence.CompleteMove " + __instance.owningActor.GameRep.transform.name);
      try {
        foreach (MonoBehaviour component in __instance.owningActor.GameRep.GetComponentsInChildren<MonoBehaviour>(true)) {
          if (component is IEnableOnMove enInited) { enInited.Disable(); }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(ICombatant) })]
  public static class MechJumpSequence_Constructor {
    public static void Prefix(ref bool __runOriginal, MechJumpSequence __instance, ref Vector3 finalPos, Quaternion finalHeading, ICombatant dfaTarget) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.MechRep is CustomMechRepresentation custMechRep) { custMechRep.InitJump(ref finalPos); }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("TurnParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_TurnParamSquad {
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance, float value) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.ActorRep is CustomMechRepresentation custRep) { custRep.TurnParam = value; __runOriginal = false; return; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("ForwardParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_ForwardParamSquad {
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance, float value) {
      try {
        if (__instance.ActorRep is CustomMechRepresentation custRep) { custRep.ForwardParam = value; __runOriginal = false; return; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("IsMovingParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_IsMovingParamSquad {
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance, bool value) {
      try {
        if (__instance.ActorRep is CustomMechRepresentation custRep) { custRep.IsMovingParam = value; __runOriginal = false; return; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("BeginMovementParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_BeginMovementParamSquad {
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance, bool value) {
      try {
        if (__instance.ActorRep is CustomMechRepresentation custRep) { custRep.BeginMovementParam = value; __runOriginal = false; return; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("DamageParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_DamageParamSquad {
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance, float value) {
      try {
        if (__instance.ActorRep is CustomMechRepresentation custRep) { custRep.DamageParam = value; __runOriginal = false; return; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_UpdateSquad {
    public static void Postfix(MechJumpSequence __instance) {
      try {
        if (__instance.HasStarted == false) { return; }
        if (__instance.OrdersAreComplete) { return; }
        if (__instance.OwningMech.GameRep is CustomMechRepresentation custRep) { custRep.UpdateJumpFlying(__instance); }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        MechJumpSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("JumpDistance")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_JumpDistance {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if(__instance.GetTags().Contains("irbtmu_immobile_unit")) { __result = 0f; return; }
        if(__instance is TrooperSquad squad) {
          int workingJumpjetLocaltions = squad.workingJumpsLocations().Count;
          __result = workingJumpjetLocaltions > 0 ? (__result / (float)workingJumpjetLocaltions) : 0f;
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        MechJumpSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("CompleteJump")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_CompleteJump {
    public static void Prefix(MechJumpSequence __instance, ref bool __state) {
      try {
        __state = __instance.OrdersAreComplete;
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        MechJumpSequence.logger.LogException(e);
      }
    }
    public static void Postfix(MechJumpSequence __instance, ref bool __state) {
      if (__state) { return; }
      if (__instance.OwningMech.GameRep is CustomMechRepresentation custRep) { custRep.CompleteJump(__instance); }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("RefreshSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class PilotableActorRepresentation_RefreshSurfaceType {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, bool forceUpdate, ref bool __result) {
      Log.Combat?.WL(0,"PilotableActorRepresentation.RefreshSurfaceType Prefix");
      if (__instance.parentCombatant == null) { __runOriginal = false; return; }
      if (__instance.parentCombatant.UnaffectedDesignMasks()) {
        Log.Combat?.WL(1, "unaffected");
        __result = true;
        __runOriginal = false; return;
      }
      AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
      if (altReps != null) { if (altReps.isHovering) { Log.Combat?.WL(1, "hovering"); __runOriginal = false; return; } }
      return;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("LateUpdate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_LateUpdate {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.M?.TWL(0, "MechRepresentation.LateUpdate Transpiler");
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("CreateVOQueue", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(MechRepresentation_LateUpdate), nameof(CreateVOQueue));
        for (int t = 0; t < uInstructions.Count; ++t) {
          if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && ((MethodInfo)uInstructions[t].operand == targetMethod)) {
            MethodPos = t;
            //uInstructions[t].operand = replacementMethod;
            uInstructions[t].opcode = OpCodes.Call;
            uInstructions[t].operand = replacementMethod;
            break;
          }
        }
        if (MethodPos < 0) {
          Log.M?.WL(1, "can't find AudioEventManager.CreateVOQueue call");
          return uInstructions;
        }
        Log.M?.WL(1, "found AudioEventManager.CreateVOQueue call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("QueueVOEvent", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(MechRepresentation_LateUpdate), nameof(QueueVOEvent));
        for (int t = 0; t < uInstructions.Count; ++t) {
          if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && ((MethodInfo)uInstructions[t].operand == targetMethod)) {
            MethodPos = t;
            //uInstructions[t].operand = replacementMethod;
            uInstructions[t].opcode = OpCodes.Call;
            uInstructions[t].operand = replacementMethod;
            break;
          }
        }
        if (MethodPos < 0) {
          Log.M?.WL(1, "can't find AudioEventManager.QueueVOEvent call");
          return uInstructions;
        }
        Log.M?.WL(1, "found AudioEventManager.QueueVOEvent call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("StartVOQueue", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(MechRepresentation_LateUpdate), nameof(StartVOQueue));
        for (int t = 0; t < uInstructions.Count; ++t) {
          if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && ((MethodInfo)uInstructions[t].operand == targetMethod)) {
            MethodPos = t;
            //uInstructions[t].operand = replacementMethod;
            uInstructions[t].opcode = OpCodes.Call;
            uInstructions[t].operand = replacementMethod;
            break;
          }
        }
        if (MethodPos < 0) {
          Log.M?.WL(1, "can't find AudioEventManager.StartVOQueue call");
          return uInstructions;
        }
        Log.M?.WL(1, "found AudioEventManager.StartVOQueue call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      return uInstructions;
    }
    public static VOQueue CreateVOQueue(string id, float queueCompleteDelay, MessageCenterMessage queueCompleteMessage, AkCallbackManager.EventCallback queueCompleteCallback, MechRepresentation mechRep) {
      Log.Combat?.TWL(0, "MechRepresentation.LateUpdate.CreateVOQueue " + (mechRep == null ? "null" : mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return null; }
      AlternateMechRepresentation alternateRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return null; }
      return AudioEventManager.CreateVOQueue(id, queueCompleteDelay, queueCompleteMessage, queueCompleteCallback);
    }
    public static void QueueVOEvent(string queueId, VOEvents voEvent, AbstractActor audioActor, MechRepresentation mechRep) {
      Log.Combat?.TWL(0, "MechRepresentation.LateUpdate.QueueVOEvent " + (mechRep == null ? "null" : mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return; }
      AlternateMechRepresentation alternateRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return; }
      AudioEventManager.QueueVOEvent(queueId, voEvent, audioActor);
    }
    public static void StartVOQueue(float initialDelay, MechRepresentation mechRep) {
      Log.Combat?.TWL(0, "MechRepresentation.LateUpdate.StartVOQueue " + (mechRep == null ? "null" : mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return; }
      AlternateMechRepresentation alternateRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return; }
      AudioEventManager.StartVOQueue(initialDelay);
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("OnAdded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_OnAdded {
    public delegate void d_OrderSequenceOnAdded(OrderSequence instance);
    private static d_OrderSequenceOnAdded i_OrderSequenceOnAdded = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(OrderSequence).GetMethod("OnAdded", BindingFlags.Public | BindingFlags.Instance);
        var dm = new DynamicMethod("CUOnAdded", null, new Type[] { typeof(OrderSequence) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_OrderSequenceOnAdded = (d_OrderSequenceOnAdded)dm.CreateDelegate(typeof(d_OrderSequenceOnAdded));
      }
      return true;
    }
    public static void OrderSequence_OnAdded(this OrderSequence seq) { i_OrderSequenceOnAdded(seq); }
    public static void Prefix(ref bool __runOriginal, ActorMovementSequence __instance) {
      try {
        Log.Combat?.TWL(0, "ActorMovementSequence.OnAdded "+__instance.owningActor.GameRep.transform.name);
        foreach(MonoBehaviour component in __instance.owningActor.GameRep.GetComponentsInChildren<MonoBehaviour>(true)) {
          if (component is IEnableOnMove enInited) { enInited.Enable(); }
        }
        CombatGameState Combat = __instance.Combat;
        if (__instance.owningActor.HasFiredThisRound)
          __instance.owningActor.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.owningActor.GUID, __instance.owningActor.GUID, "ACE PILOT", FloatieMessage.MessageNature.Buff));
        __instance.OrderSequence_OnAdded();
        if (!__instance.OwningActor.team.FirstMoveMade && __instance.OwningActor.team.GUID == Combat.LocalPlayerTeamGuid && AudioEventManager.musicCurrentMissionStatus == AudioSwitch_Mission_Status.ambient) {
          __instance.OwningActor.team.FirstMoveMade = true;
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "first_move");
          AudioEventManager.SetMusicState(AudioEventManager.musicCurrentMusicState, AudioSwitch_Mission_Status.ambient_hasmoved, AudioEventManager.musicCurrentPlayerState);
        }
        if (__instance.moveType == MoveType.Sprinting)
          __instance.OwningActor.SprintedLastRound = true;
        else if (__instance.moveType == MoveType.Walking)
          __instance.owningActor.MovedLastRound = true;
        else if (__instance.moveType == MoveType.SensorShadow)
          __instance.OwningActor.SensorShadowedLastRound = true;
        if (__instance.OwningActor.Pathing.MoveType == MoveType.Backward)
          __instance.OwningActor.LogReverseBegin();
        else if (__instance.OwningActor.Pathing.MoveType == MoveType.Sprinting)
          __instance.OwningActor.LogSprintBegin();
        else
          __instance.OwningActor.LogMoveBegin();
        __instance.OwningActor.MovingToPosition = new PositionAndRotation(__instance.FinalPos, __instance.FinalHeading.sqrMagnitude > Core.Epsilon ? Quaternion.LookRotation(__instance.FinalHeading) : Quaternion.identity, Combat.Constants);
        List<UnityEngine.Rect> Rectangles = new List<UnityEngine.Rect>();
        float num = __instance.OwningActor.Radius * 2f;
        Rectangles.Add(UnityEngine.Rect.MinMaxRect(__instance.OwningActor.CurrentPosition.x - num, __instance.OwningActor.CurrentPosition.z - num, __instance.OwningActor.CurrentPosition.x + num, __instance.OwningActor.CurrentPosition.z + num));
        Rectangles.Add(UnityEngine.Rect.MinMaxRect(__instance.FinalPos.x - num, __instance.FinalPos.z - num, __instance.FinalPos.x + num, __instance.FinalPos.z + num));
        __instance.OwningActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new RecomputePathingMessage(__instance.OwningActor.GUID, Rectangles));
        __instance.OwningActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new ActorMoveBeginMessage(__instance.OwningActor.GUID, (IStackSequence)__instance));
        Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.OnVisibilityAcquiredBlip, new ReceiveMessageCenterMessage(__instance.OnBlipAcquired));
        Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.PlayerVisibilityChanged, new ReceiveMessageCenterMessage(__instance.OnPlayerVisChanged));
        __instance.ShowCamera();
        CustomMechRepresentation custRep = __instance.ActorRep as CustomMechRepresentation;
        if (custRep == null) {
          if (__instance.OwningVehicle != null) {
            __instance.OwningVehicle.GameRep.lastStateWasVisible = !__instance.OwningVehicle.GameRep.BlipDisplayed;
            if (__instance.OwningVehicle.GameRep.lastStateWasVisible)
              __instance.OwningVehicle.GameRep.PlayMovementStartAudio();
          }
          Animator ThisAnimator = __instance.ThisAnimator;
          if (ThisAnimator != null) { ThisAnimator.speed = 1f; }
          __instance.IsMovingParam = true;
          __instance.BeginMovementParam = true;
          if (__instance.OwningMech != null) {
            if (__instance.OwningMech.GameRep != null)
              __instance.OwningMech.GameRep.SetMeleeIdleState(false);
            if (__instance.OwningMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed)
              __instance.DamageParam = -1f;
            else if (__instance.OwningMech.RightLegDamageLevel == LocationDamageLevel.Destroyed)
              __instance.DamageParam = 1f;
          }
        } else {
          custRep.BeginMove(__instance);
        }
        List<WayPoint> Waypoints = __instance.Waypoints;
        if (Waypoints != null) {
          if (Waypoints.Count > 1) {
            VOEvents voEvent = VOEvents.MechMove_Move;
            if (__instance.isSprinting && Combat.TurnDirector.IsInterleaved) { voEvent = VOEvents.MechMove_Sprint; }
            string str = string.Format("MechMovementSequence_{0}_{1}", (object)__instance.RootSequenceGUID, (object)__instance.SequenceGUID);
            AudioEventManager.CreateVOQueue(str, -1f, (MessageCenterMessage)null, (AkCallbackManager.EventCallback)null);
            AudioEventManager.QueueVOEvent(str, voEvent, __instance.OwningActor);
            AudioEventManager.StartVOQueue(0.5f);
          }
        }
        __runOriginal = false;
        return;
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        ActorMovementSequence.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(MechRepresentationSimGame))]
  [HarmonyPatch("LoadDamageState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentationSimGame_LoadDamageState {
    public delegate void d_LoadDamageState(MechRepresentationSimGame mechRepresentation);
    private static d_LoadDamageState i_LoadDamageState = null;
    public delegate void d_CollapseLocation(MechRepresentationSimGame mechRepresentation, int location, bool isDestroyed);
    private static d_CollapseLocation i_CollapseLocation = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(MechRepresentationSimGame).GetMethod("LoadDamageState", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CULoadDamageState", null, new Type[] { typeof(MechRepresentationSimGame) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_LoadDamageState = (d_LoadDamageState)dm.CreateDelegate(typeof(d_LoadDamageState));
      }
      {
        MethodInfo method = typeof(MechRepresentationSimGame).GetMethod("CollapseLocation", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUCollapseLocation", null, new Type[] { typeof(MechRepresentationSimGame), typeof(int), typeof(bool) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_CollapseLocation = (d_CollapseLocation)dm.CreateDelegate(typeof(d_CollapseLocation));
      }
      return true;
    }
    public static void Prefix(MechRepresentationSimGame __instance) {
    }
  }
  [HarmonyPatch(typeof(MechMeleeSequence))]
  [HarmonyPatch("ExecuteMove")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeleeSequence_ExecuteMove {
    public static float getFlyingRepMeleeHeight(this ICombatant target) {
      Vehicle vehicle = target as Vehicle;
      if (vehicle != null) {
        UnitCustomInfo info = vehicle.GetCustomInfo();
        if (info != null) {
          float result = vehicle.GameRep.BodyAttach.position.y  - vehicle.Combat.MapMetaData.GetLerpedHeightAt(vehicle.CurrentIntendedPosition) - 10f;
          if (result <= 0f) { return 0f; }
          return result;
        }
      }
      Mech mech = target as Mech;
      if (mech != null) {
        AlternateMechRepresentations altReps = target.GameRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) {
          if (altReps.isHovering) {
            float result = altReps.HoveringHeight;
            if (result > 0f) { return result; }
          }
        }
      }
      return 0f;
    }
    public static void Postfix(MechMeleeSequence __instance) {
      try {
        Log.Combat?.TWL(0, "MechMeleeSequence.ExecuteMove ");
        AlternateMechRepresentations altReps = __instance.MechRep.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if(altReps != null) {
          if (altReps.isHovering) {
            altReps.InitMeleeTargetHeight(__instance.MeleeTarget.getFlyingRepMeleeHeight());
          }
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        MechMeleeSequence.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("WorkingJumpjets")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_WorkingJumpjets {
    public static void Postfix(Mech __instance, ref int __result) {
      try {
        if(__instance.GetTags().Contains("irbtmu_immobile_unit")) { __result = 0; return; }
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) {
          __result = squad.workingJumpsLocations().Count;
        }
        if (__instance.GameRep is CustomMechRepresentation custRep) {
          if (__instance.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            if(custRep.altDef.NoJumpjetsBlock == false) {
              __result = 0;
            }
          }
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateJump))]
  [HarmonyPatch("GetAllDFATargets")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateJump_GetAllDFATargets {
    private static Dictionary<AbstractActor, bool> dfaForbiddenCache = new Dictionary<AbstractActor, bool>();
    public static bool isDFAForbidden(this AbstractActor unit) {
      if (dfaForbiddenCache.TryGetValue(unit, out bool result)) { return result; }
      dfaForbiddenCache.Add(unit, false);
      return false;
    }
    public static void isDFAForbidden(this AbstractActor unit, bool DFAForbidden) {
      if (dfaForbiddenCache.ContainsKey(unit)) { dfaForbiddenCache[unit] = DFAForbidden; return; }
      dfaForbiddenCache.Add(unit, DFAForbidden);
    }
    public static void Clear() {
      dfaForbiddenCache.Clear();
    }
    public static void Postfix(SelectionStateJump __instance, ref List<ICombatant> __result) {
      return;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("CanDFATargetFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3) })]
  public static class Mech_CanDFATargetFromPosition {
    public static void Postfix(AbstractActor __instance, ICombatant target, Vector3 position, ref bool __result) {
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("GuardLevel")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_GuardLevel {
    public static void Postfix(AbstractActor __instance, ref int __result) {
      try {
        if (__result != 0) { if (__instance.FakeVehicle()) { __result = 0; } }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(new Type[] { typeof(ICombatant), typeof(out string) })]
  [HarmonyPatch()]
  public static class Mech_CanEngageTarget {
    public static MethodBase TargetMethod() {
      List<MethodInfo> methods = AccessTools.GetDeclaredMethods(typeof(Mech));
      Log.Combat?.TWL(0, "Mech.CanEngageTarget searching");
      foreach (MethodInfo info in methods) {
        Log.Combat?.WL(1, info.Name);
        if (info.Name != "CanEngageTarget") { continue; }
        ParameterInfo[] pars = info.GetParameters();
        Log.Combat?.WL(2, "params:"+pars.Length);
        foreach (ParameterInfo pinfo in pars) { Log.Combat?.WL(3, pinfo.ParameterType.ToString()); }
        if (pars.Length == 2) {
          return info;
        }
      }
      return null;
      //return AccessTools.Method(typeof(Mech), "OnLoadedWithText");
    }
    public static void Postfix(Mech __instance, ICombatant target, ref string debugMsg, ref bool __result) {
      try {
        if(__result == true) {
          if ((__instance.UnaffectedPathing() == false) && (target.UnaffectedPathing() == true)) { __result = false; }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }

  [HarmonyPatch(typeof(MechDisplacementSequence))]
  [HarmonyPatch("ApplyDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDisplacementSequence_ApplyDamage {
    public static void Prefix(ref bool __runOriginal, MechDisplacementSequence __instance) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, "MechDisplacementSequence.ApplyDamage "+ (__instance.OwningMech==null?"null":__instance.OwningMech.PilotableActorDef.ChassisID));
        if (__instance.OwningMech.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
          Log.Combat?.WL(0, __instance.OwningMech.FlyingHeight()+" > "+ Core.Settings.MaxHoveringHeightWithWorkingJets+" preventing");
          __runOriginal = false; return;
        }
        ICustomMech custMech = __instance.OwningMech as ICustomMech;
        if (custMech == null) { return; }
        float num = Mathf.Max(0.0f, __instance.OwningMech.StatCollection.GetValue<float>("DFASelfDamage"));
        WeaponHitInfo hitInfo = new WeaponHitInfo(__instance.SequenceGUID, __instance.RootSequenceGUID, 0, 0, __instance.attackerGUID, __instance.OwningMech.GUID, 1, new float[1], new float[1], new float[1], new bool[1], new int[1], new int[1], new AttackImpactQuality[1], new AttackDirection[1], new Vector3[1], new string[1], new int[1]);
        Vector3 vector3_1 = __instance.OwningMech.GameRep.GetHitPosition(64) + UnityEngine.Random.insideUnitSphere * 5f;
        CombatGameState Combat = __instance.Combat;
        FloatieMessage floatieMessage1;
        if ((double)__instance.OwningMech.ArmorForLocation(64) < (double)num)
          floatieMessage1 = new FloatieMessage(__instance.attackerGUID, __instance.OwningMech.GUID, new Text("{0}", new object[1]
          {
          (object) (int) Mathf.Max(1f, num)
          }), Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.StructureDamage, vector3_1.x, vector3_1.y, vector3_1.z);
        else
          floatieMessage1 = new FloatieMessage(__instance.attackerGUID, __instance.OwningMech.GUID, new Text("{0}", new object[1]
          {
          (object) (int) Mathf.Max(1f, num)
          }), Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.ArmorDamage, vector3_1.x, vector3_1.y, vector3_1.z);
        Combat.MessageCenter.PublishMessage((MessageCenterMessage)floatieMessage1);
        Vector3 vector3_2 = __instance.OwningMech.GameRep.GetHitPosition(128) + UnityEngine.Random.insideUnitSphere * 5f;
        FloatieMessage floatieMessage2;
        if ((double)__instance.OwningMech.ArmorForLocation(64) < (double)num)
          floatieMessage2 = new FloatieMessage(__instance.attackerGUID, __instance.OwningMech.GUID, new Text("{0}", new object[1]
          {
          (object) (int) Mathf.Max(1f, num)
          }), Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.StructureDamage, vector3_2.x, vector3_2.y, vector3_2.z);
        else
          floatieMessage2 = new FloatieMessage(__instance.attackerGUID, __instance.OwningMech.GUID, new Text("{0}", new object[1]
          {
          (object) (int) Mathf.Max(1f, num)
          }), Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.ArmorDamage, vector3_2.x, vector3_2.y, vector3_2.z);
        Combat.MessageCenter.PublishMessage(floatieMessage2);
        Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(__instance.OwningMech, new Text("FALL DAMAGE", (object[])Array.Empty<object>()), FloatieMessage.MessageNature.CriticalHit, true)));
        Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.attackerGUID, __instance.OwningMech.GUID, new Text("FALL DAMAGE", (object[])Array.Empty<object>()), FloatieMessage.MessageNature.Debuff));
        float damageAmount = __instance.OwningMech.StatCollection.GetValue<float>("DFASelfDamage");
        HashSet<ArmorLocation> DFALocs = custMech.GetDFASelfDamageLocations();
        Log.Combat?.TWL(0, "Fall self damage " + __instance.OwningMech.MechDef.ChassisID);
        foreach (ArmorLocation aloc in DFALocs) {
          Log.Combat?.WL(1, aloc.ToString() + ":" + damageAmount);
          __instance.OwningMech.TakeWeaponDamage(hitInfo, (int)aloc, __instance.OwningMech.MeleeWeapon, num, 0.0f, 0, DamageType.DFASelf);
        }
        Log.Combat?.TWL(0, string.Format("@@@@@@@@ {0} takes {1} damage to its  from falling!", (object)__instance.OwningMech.DisplayName, (object)num));
        __instance.OwningMech.ApplyInstabilityReduction(StabilityChangeSource.Falling);
        __instance.OwningMech.NeedsInstabilityCheck = true;
        __instance.OwningMech.CheckForInstability();
        __instance.OwningMech.NeedsInstabilityCheck = true;
        __instance.OwningMech.CheckForInstability();
        __instance.OwningMech.HandleDeath(__instance.attackerGUID);
        if (__instance.OwningMech.IsDead == false) {
          __instance.OwningMech.HandleKnockdown(__instance.RootSequenceGUID, __instance.attackerGUID, Vector2.one, (SequenceFinished)null);
        }
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        MechDisplacementSequence.logger.LogException(e);
      }
      return;
    }
  }
}