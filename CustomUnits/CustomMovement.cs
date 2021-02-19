using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.UI;
using CustAmmoCategories;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
      /*if (customQuadAnimation.ContainsKey(__instance.owningActor)) {
        Vector3 velocity = __instance.Velocity;
        velocity *= customQuadAnimation[__instance.owningActor].Velocity();
        __instance.Velocity(velocity);
      }*/
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateSpline")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_UpdateSplineSquad {
    public static void Postfix(ActorMovementSequence __instance, Vector3 ___Forward, float ___t, ICombatant ___meleeTarget) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) { squad.UpdateSpline(__instance, ___Forward); }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.UpdateSpline(__instance,___Forward,___t); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("CompleteMove")]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_CompleteMove {
    public static void Prefix(ActorMovementSequence __instance, Vector3 ___Forward, float ___t, ref Vehicle __state, bool ___playedMelee, ICombatant ___meleeTarget) {
      try {
        __state = null;
        if (__instance.OwningVehicle != null) {
          if (__instance.OwningVehicle.UnaffectedPathing()) {
            __state = __instance.OwningVehicle;
            __instance.OwningVehicle(null);
          }
        }
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) {
              sRep.Value.GameRep.transform.position = sRep.Value.deadLocation;
              sRep.Value.GameRep.transform.rotation = sRep.Value.deadRotation;
            } else {
              Vector3 newPosition = __instance.FinalPos + sRep.Value.delta;
              newPosition.y = __instance.owningActor.Combat.MapMetaData.GetCellAt(newPosition).cachedHeight;
              sRep.Value.GameRep.transform.rotation = Quaternion.LookRotation(__instance.FinalHeading, Vector3.up);
              sRep.Value.GameRep.transform.position = newPosition;
            }
          }
        }
        VTOLBodyAnimation bodyAnimation = __instance.owningActor.VTOLAnimation();
        if (bodyAnimation != null) {
          if (bodyAnimation.bodyAnimator != null) {
            bodyAnimation.bodyAnimator.SetFloat("backward", 0f);
            bodyAnimation.bodyAnimator.SetFloat("forward", 0f);
          }
        }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.CompleteMove(__instance, ___playedMelee, ___meleeTarget); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(ActorMovementSequence __instance, ref Vehicle __state) {
      Log.LogWrite("ActorMovementSequence.CompleteMove\n");
      if (__state != null) {
        __instance.OwningVehicle(__state);
      }
      CustomQuadLegController[] customMoveAnimators = __instance.owningActor.GameRep.gameObject.GetComponentsInChildren<CustomQuadLegController>();
      foreach (CustomQuadLegController customMoveAnimatior in customMoveAnimators) {
        Log.LogWrite(" CustomMoveAnimator:" + customMoveAnimatior.name + "\n");
        customMoveAnimatior.StopAnimation();
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("TurnParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_TurnParamSquad {
    public static void Postfix(ActorMovementSequence __instance, float value) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.TurnParam = value;
          }
        }
        QuadRepresentation quadRep = __instance.owningActor.GameRep.gameObject.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.TurnParam = value; }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.TurnParam = value; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("ForwardParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_ForwardParamSquad {
    public static void Postfix(ActorMovementSequence __instance, float value) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.ForwardParam = value;
          }
        }
        QuadRepresentation quadRep = __instance.owningActor.GameRep.gameObject.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.ForwardParam = value; }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.ForwardParam = value; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("IsMovingParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_IsMovingParamSquad {
    public static void Postfix(ActorMovementSequence __instance, bool value) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.IsMovingParam = value;
          }
        }
        QuadRepresentation quadRep = __instance.owningActor.GameRep.gameObject.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.IsMovingParam = value; }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.IsMovingParam = value; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("BeginMovementParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class ActorMovementSequence_BeginMovementParamSquad {
    public static void Postfix(ActorMovementSequence __instance, bool value) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.BeginMovementParam = value;
          }
        }
        QuadRepresentation quadRep = __instance.owningActor.GameRep.gameObject.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.BeginMovementParam = value; }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.BeginMovementParam = value; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("DamageParam")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class ActorMovementSequence_DamageParamSquad {
    public static void Postfix(ActorMovementSequence __instance, float value) {
      try {
        TrooperSquad squad = __instance.owningActor as TrooperSquad;
        if (squad != null) {
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.DamageParam = value;
          }
        }
        QuadRepresentation quadRep = __instance.owningActor.GameRep.gameObject.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.DamageParam = value; }
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.DamageParam = value; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("UpdateStateInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_UpdateStateInfo {
    public static void UpdateStateInfoNoFloaties(this PilotableActorRepresentation rep, ref bool ___wasEvasiveLastFrame, ref bool ___guardedLastFrame, ref bool ___coverLastFrame, ref bool ___wasUnsteadyLastFrame, ref bool ___wasEntrenchedLastFrame) {
      if (rep.parentActor == null || rep.parentActor.IsDead || !rep.parentActor.Combat.TurnDirector.GameHasBegun)
        return;
      bool isUnsteady = rep.parentActor.IsUnsteady;
      ___wasUnsteadyLastFrame = isUnsteady;
      bool isEvasive = rep.parentActor.IsEvasive;
      ___wasEvasiveLastFrame = isEvasive;
      bool hasCover = rep.parentActor.HasCover;
      ___coverLastFrame = hasCover;
      bool bracedLastRound = rep.parentActor.BracedLastRound;
      ___guardedLastFrame = bracedLastRound;
      bool isEntrenched = rep.parentActor.IsEntrenched;
      ___wasEntrenchedLastFrame = isEntrenched;
    }
    public static bool Prefix(PilotableActorRepresentation __instance,ref bool ___wasEvasiveLastFrame,ref bool ___guardedLastFrame,ref bool ___coverLastFrame, ref bool ___wasUnsteadyLastFrame, ref bool ___wasEntrenchedLastFrame) {
      try {
        QuadLegsRepresentation legs = __instance.GetComponent<QuadLegsRepresentation>();
        if (legs != null) { __instance.UpdateStateInfoNoFloaties(ref ___wasEvasiveLastFrame, ref ___guardedLastFrame,ref ___coverLastFrame,ref ___wasUnsteadyLastFrame,ref ___wasEntrenchedLastFrame); return false; }
        AlternateMechRepresentation altRep = __instance.GetComponent<AlternateMechRepresentation>();
        if (altRep != null) { __instance.UpdateStateInfoNoFloaties(ref ___wasEvasiveLastFrame, ref ___guardedLastFrame, ref ___coverLastFrame, ref ___wasUnsteadyLastFrame, ref ___wasEntrenchedLastFrame); return false; }
        TrooperSquad squad = __instance.parentActor as TrooperSquad;
        if (squad != null) {
          MechRepresentation mechRep = __instance as MechRepresentation;
          if (squad.MechReps.Contains(mechRep)) {
            mechRep.UpdateStateInfoNoFloaties(ref ___wasEvasiveLastFrame, ref ___guardedLastFrame, ref ___coverLastFrame, ref ___wasUnsteadyLastFrame, ref ___wasEntrenchedLastFrame);
            return false;
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("OnPlayerVisibilityChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
  public static class MechRepresentation_OnPlayerVisibilityChanged {
    public static void Postfix(MechRepresentation __instance, VisibilityLevel newLevel) {
      try {
        AlternateMechRepresentations alternateMechRepresentations = __instance.GetComponent<AlternateMechRepresentations>();
        if(alternateMechRepresentations != null) {
          __instance.BlipObjectGhostStrong.SetActive(false);
          __instance.BlipObjectGhostWeak.SetActive(false);
          alternateMechRepresentations.OnPlayerVisibilityChanged(newLevel);
        }
        QuadLegsRepresentation quadLegs = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegs != null) {
          __instance.BlipObjectUnknown.SetActive(false);
          __instance.BlipObjectIdentified.SetActive(false);
          __instance.BlipObjectGhostWeak.SetActive(false);
          __instance.BlipObjectGhostStrong.SetActive(false);
        }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if(quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.OnPlayerVisibilityChanged(newLevel);
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.OnPlayerVisibilityChanged(newLevel);
            sRep.Value.MechRep.BlipObjectUnknown.SetActive(false);
            sRep.Value.MechRep.BlipObjectIdentified.SetActive(false);
            sRep.Value.MechRep.BlipObjectGhostWeak.SetActive(false);
            sRep.Value.MechRep.BlipObjectGhostStrong.SetActive(false);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_OnCombatGameDestroyed {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.OnCombatGameDestroyed();
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.OnCombatGameDestroyed(); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            sRep.Value.MechRep.OnCombatGameDestroyed();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayComponentDestroyedVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(Vector3) })]
  public static class MechRepresentation_PlayComponentDestroyedVFX {
    public static bool Prefix(MechRepresentation __instance, int location, Vector3 attackDirection) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayComponentDestroyedVFX(location, attackDirection); return false; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("VisibleToPlayer")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_VisibleToPlayer {
    public static void Postfix(GameRepresentation __instance, ref bool __result) {
      try {
        if (__result == false) { return; }
        AlternateMechRepresentation altRep = __instance.GetComponent(typeof(AlternateMechRepresentation)) as AlternateMechRepresentation;
        if (altRep != null) { __result = altRep.isVisible; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("CollapseLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(Vector3), typeof(bool) })]
  public static class MechRepresentation_CollapseLocation {
    public delegate void d_CollapseLocation(MechRepresentation mechRep, int location, Vector3 attackDirection, bool loading);
    private static d_CollapseLocation i_CollapseLocation = null;
    public static void CollapseLocation(this MechRepresentation rep, int location, Vector3 attackDirection, bool loading) {
      i_CollapseLocation(rep, location, attackDirection, loading);
    }
    public static bool Prepare() {
      {
        MethodInfo method = typeof(MechRepresentation).GetMethod("CollapseLocation", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUCollapseLocation", null, new Type[] { typeof(MechRepresentation), typeof(int), typeof(Vector3), typeof(bool) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_CollapseLocation = (d_CollapseLocation)dm.CreateDelegate(typeof(d_CollapseLocation));
      }
      return true;
    }
    public static bool Prefix(MechRepresentation __instance, int location, Vector3 attackDirection, bool loading) {
      try {
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          Log.TWL(0, "MechRepresentation.CollapseLocation quad. " + __instance.parentActor.DisplayName + " " + ((ChassisLocations)location));
          switch ((ChassisLocations)location) {
            case ChassisLocations.CenterTorso: return false;
            case ChassisLocations.Head: return false;
            case ChassisLocations.LeftTorso:
            case ChassisLocations.RightTorso:
              QuadBodyAnimation quadBody = __instance.GetComponentInChildren<QuadBodyAnimation>(true);
              if (quadBody != null) { quadBody.CollapseLocation(location); return false; }
            break;
            case ChassisLocations.LeftArm:
            case ChassisLocations.RightArm:
              MechDestructibleObject destructibleObject = __instance.GetDestructibleObject(location);
              Log.WL(1, "destructibleObject: " + (destructibleObject == null?"null":destructibleObject.name));
              if ((UnityEngine.Object)destructibleObject != (UnityEngine.Object)null) {
                if (loading)
                  destructibleObject.CollapseSwap(true);
                else
                  destructibleObject.Collapse(attackDirection, __instance.Constants.ResolutionConstants.ComponentDestructionForceMultiplier);
              }
            return false;
          }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayDeathVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DeathMethod), typeof(int) })]
  public static class MechRepresentation_PlayDeathVFX {
    public static void Postfix(MechRepresentation __instance, DeathMethod deathMethod, int location) {
      try {
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) { quadRepresentation.fLegsRep.LegsRep.PlayDeathVFX(deathMethod, location); };
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayDeathFloatie")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DeathMethod) })]
  public static class MechRepresentation_PlayDeathFloatie {
    public static bool Prefix(MechRepresentation __instance, DeathMethod deathMethod) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return false; };
        AlternateMechRepresentation altRep = __instance.GetComponent<AlternateMechRepresentation>();
        if (altRep != null) { return false; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("UpdateLegDamageAnimFlags")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LocationDamageLevel), typeof(LocationDamageLevel) })]
  public static class MechRepresentation_UpdateLegDamageAnimFlags {
    public static void Postfix(MechRepresentation __instance, LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.UpdateLegDamageAnimFlags(leftLegDamage, rightLegDamage); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetMeleeIdleState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_SetMeleeIdleState {
    public static void Postfix(MechRepresentation __instance, bool isMelee) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.SetMeleeIdleState(isMelee);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.SetMeleeIdleState(isMelee); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.thisAnimator.SetFloat("MeleeEngaged", isMelee ? 1f : 0.0f);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("TriggerMeleeTransition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_TriggerMeleeTransition {
    public static void Postfix(MechRepresentation __instance, bool meleeIn) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.TriggerMeleeTransition(meleeIn);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.TriggerMeleeTransition(meleeIn); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            if (meleeIn)
              sRep.Value.MechRep.thisAnimator.SetTrigger("MeleeIdleIn");
            else
              sRep.Value.MechRep.thisAnimator.SetTrigger("MeleeIdleOut");
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayFireAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackSourceLimb), typeof(int) })]
  public static class MechRepresentation_PlayFireAnim {
    public static void Prefix(MechRepresentation __instance, ref AttackSourceLimb sourceLimb, int recoilStrength) {
      QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
      if (quadRepresentation != null) {
        sourceLimb = AttackSourceLimb.Torso;
        quadRepresentation.fLegsRep.LegsRep.PlayFireAnim(sourceLimb, recoilStrength);
      }
    }
    public static void Postfix(MechRepresentation __instance, AttackSourceLimb sourceLimb, int recoilStrength) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayFireAnim(sourceLimb, recoilStrength); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayFireAnim(sourceLimb, recoilStrength);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayMeleeAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_PlayMeleeAnimSquad {
    public static bool Prefix(MechRepresentation __instance,ref int meleeHeight) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return true; }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayMeleeAnim(meleeHeight); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return true; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayMeleeAnim(meleeHeight);
          }
        }
        WeaponMode mode = __instance.parentMech.MeleeWeapon.mode();
        if(mode.meleeAttackType != MeleeAttackType.NotSet) {
          meleeHeight = __instance.parentMech.Combat.MeleeRules.GetMeleeHeightFromAttackType(mode.meleeAttackType);
        }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          bool mainAnimPlay = true;
          int frontMeleeHight = meleeHeight;
          switch (meleeHeight) {
            case 0: mainAnimPlay = true; meleeHeight = 0; frontMeleeHight = 0; break; //Tackle 
            case 1: mainAnimPlay = true; meleeHeight = 1; frontMeleeHight = 1; break; //Stomp
            case 2: mainAnimPlay = true; meleeHeight = 8; frontMeleeHight = 8; break; //Kick
            case 3: mainAnimPlay = true; meleeHeight = 8; frontMeleeHight = 8; break; //Punch
            case 8: mainAnimPlay = true; meleeHeight = 8; frontMeleeHight = 8; break; //Charge
            case 9: mainAnimPlay = true; meleeHeight = 9; frontMeleeHight = 9; break; //DFA
          }
          quadRepresentation.fLegsRep.LegsRep.PlayMeleeAnim(frontMeleeHight);
          return mainAnimPlay;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ToggleRandomIdles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_ToggleRandomIdlesSquad {
    public static void Postfix(MechRepresentation __instance, bool shouldIdle) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.ToggleRandomIdles(shouldIdle);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.ToggleRandomIdles(shouldIdle); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.ToggleRandomIdles(shouldIdle);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayImpactAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(MeleeAttackType), typeof(float) })]
  public static class MechRepresentation_PlayImpactAnimSquad {
    public static void Postfix(MechRepresentation __instance, WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      try {
        if (__instance == null) { return; }
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          if (quadRepresentation.fLegsRep != null) {
            if (quadRepresentation.fLegsRep.LegsRep != null) {
              quadRepresentation.fLegsRep.LegsRep.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
            }
          }
        }
        AlternateMechRepresentations altReps = null;
        try { altReps = __instance.GetComponent<AlternateMechRepresentations>(); } catch (Exception) { altReps = null; }
        if (altReps != null) { altReps.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage); }
        TrooperSquad squad = null;
        try { squad = __instance.parentMech as TrooperSquad; } catch(Exception) { squad = null; };
        if (squad == null) {
          if (squad.MechReps != null) {
            if (squad.MechReps.Contains(__instance)) { return; }
            foreach (var sRep in squad.squadReps) {
              if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
              if (sRep.Value == null) { continue; }
              if (sRep.Value.MechRep == null) { continue; }
              sRep.Value.MechRep.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetUnsteadyAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_SetUnsteadyAnim {
    public static void Postfix(MechRepresentation __instance, bool isUnsteady) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.SetUnsteadyAnim(isUnsteady);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.SetUnsteadyAnim(isUnsteady); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.SetUnsteadyAnim(isUnsteady);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("PlayMeleeAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_PlayKnockdownAnim {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "ActorMovementSequence.PlayMeleeAnim Transpiler");
      MethodInfo targetMethod = typeof(GameRepresentation).GetMethod("PlayMeleeAnim", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(ActorMovementSequence_PlayKnockdownAnim), nameof(PlayMeleeAnim));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
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
        Log.WL(1, "can't find MechRepresentation.PlayMeleeAnim call");
        return uInstructions;
      }
      Log.WL(1, "found MechRepresentation.PlayMeleeAnim call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      return uInstructions;
    }
    public static void PlayMeleeAnim(MechRepresentation mechRep, int meleeHeight, ActorMovementSequence seq) {
      AlternateMechRepresentations altReps = mechRep.GetComponent<AlternateMechRepresentations>();
      if (altReps == null) { mechRep.PlayMeleeAnim(meleeHeight); return; }
      ICombatant meleeTarget = Traverse.Create(seq).Field<ICombatant>("meleeTarget").Value;
      if (meleeTarget == null) { mechRep.PlayMeleeAnim(meleeHeight); return; }
      Log.TWL(0, "ActorMovementSequence.MechRepresentation.PlayMeleeAnim " + (mechRep == null ? "null" : mechRep.name)+" meleeTarget:"+ (meleeTarget==null?"null":meleeTarget.DisplayName));
      altReps.PlayMeleeAnim(meleeTarget, Traverse.Create(seq).Field<MeleeAttackType>("meleeType").Value);
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayKnockdownAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector2) })]
  public static class MechRepresentation_PlayKnockdownAnim {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "MechRepresentation.PlayKnockdownAnim Transpiler");
      MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("PlayPilotVO", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(MechRepresentation_PlayKnockdownAnim), nameof(PlayPilotVO));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
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
        Log.WL(1, "can't find AudioEventManager.PlayPilotVO call");
        return uInstructions;
      }
      Log.WL(1, "found AudioEventManager.PlayPilotVO call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      return uInstructions;
    }
    public static bool PlayPilotVO(VOEvents voEvent, AbstractActor audioActor, AkCallbackManager.EventCallback callback, object in_cookie, bool decrementGlobalCooldown, MechRepresentation mechRep) {
      Log.TWL(0, "MechRepresentation.PlayKnockdownAnim.PlayPilotVO " + (mechRep == null?"null": mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return true; }
      AlternateMechRepresentation altRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (altRep != null) { return true; }
      return AudioEventManager.PlayPilotVO(voEvent, audioActor, callback, in_cookie, decrementGlobalCooldown);
    }
    public static void Postfix(MechRepresentation __instance, Vector2 attackDirection) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayKnockdownAnim(attackDirection);
          __instance.parentActor.NoRandomIdles(true);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayKnockdownAnim(attackDirection); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayKnockdownAnim(attackDirection);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ForceKnockdown")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector2) })]
  public static class MechRepresentation_ForceKnockdown {
    public static void Postfix(MechRepresentation __instance, Vector2 attackDirection) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.ForceKnockdown(attackDirection);
          __instance.parentActor.NoRandomIdles(true);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.ForceKnockdown(attackDirection); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.ForceKnockdown(attackDirection);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayStandAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_ResetHitReactFlags {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayStandAnim();
          __instance.parentActor.NoRandomIdles(false);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayStandAnim(); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayStandAnim();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayJumpLaunchAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_PlayJumpLaunchAnimSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayJumpLaunchAnim();
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayJumpLaunchAnim(); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayJumpLaunchAnim();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayFallingAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector2) })]
  public static class MechRepresentation_PlayFallingAnimSquad {
    public static void Postfix(MechRepresentation __instance, Vector2 direction) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayFallingAnim(direction);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayFallingAnim(direction); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayFallingAnim(direction);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("UpdateJumpAirAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float), typeof(float) })]
  public static class MechRepresentation_UpdateJumpAirAnimSquad {
    public static void Postfix(MechRepresentation __instance, float forward, float side) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.UpdateJumpAirAnim(forward,side);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.UpdateJumpAirAnim(forward,side); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.UpdateJumpAirAnim(forward,side);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayJumpLandAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_PlayJumpLandAnimSquad {
    public static void Postfix(MechRepresentation __instance, bool isDFA) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayJumpLandAnim(isDFA);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayJumpLandAnim(isDFA); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayJumpLandAnim(isDFA);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StartJumpjetEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StartJumpjetEffectSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.StartJumpjetEffect();
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.StartJumpjetEffect();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StopJumpjetEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StopJumpjetEffectSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.StopJumpjetEffect();
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.StopJumpjetEffect();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayShutdownAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_PlayShutdownAnim {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayShutdownAnim();
          __instance.parentActor.NoRandomIdles(true);
        }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { altReps.PlayShutdownAnim(); }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayShutdownAnim();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayStartupAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_PlayStartupAnim {
    public static void Postfix(MechRepresentation __instance) {
      try {
        QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegsRepresentation != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if (quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.PlayStartupAnim();
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          if (squad.MechReps.Contains(__instance)) { return; }
          foreach (var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
            sRep.Value.MechRep.PlayStartupAnim();
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("HandleDeath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DeathMethod), typeof(int) })]
  public static class MechRepresentation_HandleDeathSquad {
    public static bool Prefix(MechRepresentation __instance, DeathMethod deathMethod, int location) {
      Log.TWL(0, "MechRepresentation.HandleDeath "+__instance.parentActor.DisplayName);
      AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
      if (altReps != null) {
        Log.WL(1, "CanHandleDeath:"+ altReps.CanHandleDeath);
        if (altReps.CanHandleDeath == false) {
          altReps.DelayedHandleDeath(deathMethod, location); return false;
        } else {
          altReps.HandleDeath(deathMethod, location); return false;
        }
      }
      return true;
    }
    public static void Postfix(MechRepresentation __instance, DeathMethod deathMethod, int location) {
      try {
        QuadLegsRepresentation quadLegs = __instance.GetComponent<QuadLegsRepresentation>();
        if (quadLegs != null) { return; }
        QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
        if(quadRepresentation != null) {
          quadRepresentation.fLegsRep.LegsRep.HandleDeath(deathMethod, location);
          __instance.parentActor.NoRandomIdles(true);
        }

        //TrooperSquad squad = __instance.parentMech as TrooperSquad;
        //if (squad == null) { return; }
        //if (squad.MechReps.Contains(__instance)) { return; }
        //ArmorLocation armorLocation = (ArmorLocation)location;
        //if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)) {
        //  trooperRep.MechRep.HandleDeath(deathMethod, (int)ArmorLocation.CenterTorso);
        //}
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("TriggerFootFall")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_TriggerFootFall {
    public static bool Prefix(MechRepresentation __instance, int leftFoot) {
      try {
        AlternateMechRepresentation altRep = __instance.GetComponent<AlternateMechRepresentation>();
        if (altRep != null) { return false; }
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { if (altReps.isHovering) { return false; } }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("RefreshSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class PilotableActorRepresentation_RefreshSurfaceType {
    public static bool Prefix(PilotableActorRepresentation __instance, bool forceUpdate, ref bool __result) {
      Log.LogWrite("PilotableActorRepresentation.RefreshSurfaceType Prefix\n");
      if (__instance.parentCombatant == null) { return false; }
      if (__instance.parentCombatant.UnaffectedDesignMasks()) {
        Log.LogWrite(" unaffected\n");
        __result = true;
        return false;
      }
      AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
      if (altReps != null) { if (altReps.isHovering) { Log.LogWrite(" hovering\n"); return false; } }
      return true;
    }
  }

  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("LateUpdate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_LateUpdate {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "MechRepresentation.LateUpdate Transpiler");
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
          Log.WL(1, "can't find AudioEventManager.CreateVOQueue call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.CreateVOQueue call " + MethodPos.ToString("X"));
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
          Log.WL(1, "can't find AudioEventManager.QueueVOEvent call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.QueueVOEvent call " + MethodPos.ToString("X"));
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
          Log.WL(1, "can't find AudioEventManager.StartVOQueue call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.StartVOQueue call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      return uInstructions;
    }
    public static VOQueue CreateVOQueue(string id, float queueCompleteDelay, MessageCenterMessage queueCompleteMessage, AkCallbackManager.EventCallback queueCompleteCallback, MechRepresentation mechRep) {
      Log.TWL(0, "MechRepresentation.LateUpdate.CreateVOQueue " + (mechRep == null ? "null" : mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return null; }
      AlternateMechRepresentation alternateRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return null; }
      return AudioEventManager.CreateVOQueue(id, queueCompleteDelay, queueCompleteMessage, queueCompleteCallback);
    }
    public static void QueueVOEvent(string queueId, VOEvents voEvent, AbstractActor audioActor, MechRepresentation mechRep) {
      Log.TWL(0, "MechRepresentation.LateUpdate.QueueVOEvent " + (mechRep == null ? "null" : mechRep.name));
      QuadLegsRepresentation quadLegs = mechRep.GetComponent<QuadLegsRepresentation>();
      if (quadLegs != null) { return; }
      AlternateMechRepresentation alternateRep = mechRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return; }
      AudioEventManager.QueueVOEvent(queueId, voEvent, audioActor);
    }
    public static void StartVOQueue(float initialDelay, MechRepresentation mechRep) {
      Log.TWL(0, "MechRepresentation.LateUpdate.StartVOQueue " + (mechRep == null ? "null" : mechRep.name));
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
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "ActorMovementSequence.OnAdded Transpiler");
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("CreateVOQueue", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(ActorMovementSequence_OnAdded), nameof(CreateVOQueue));
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
          Log.WL(1, "can't find AudioEventManager.CreateVOQueue call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.CreateVOQueue call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("QueueVOEvent", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(ActorMovementSequence_OnAdded), nameof(QueueVOEvent));
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
          Log.WL(1, "can't find AudioEventManager.QueueVOEvent call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.QueueVOEvent call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      {
        int MethodPos = -1;
        MethodInfo targetMethod = typeof(AudioEventManager).GetMethod("StartVOQueue", BindingFlags.Static | BindingFlags.Public);
        var replacementMethod = AccessTools.Method(typeof(ActorMovementSequence_OnAdded), nameof(StartVOQueue));
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
          Log.WL(1, "can't find AudioEventManager.StartVOQueue call");
          return uInstructions;
        }
        Log.WL(1, "found AudioEventManager.StartVOQueue call " + MethodPos.ToString("X"));
        uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      }
      return uInstructions;
    }
    public static VOQueue CreateVOQueue(string id, float queueCompleteDelay, MessageCenterMessage queueCompleteMessage, AkCallbackManager.EventCallback queueCompleteCallback, ActorMovementSequence moveSeq) {
      Log.TWL(0, "ActorMovementSequence.OnAdded.CreateVOQueue " + (moveSeq.owningActor.GameRep == null ? "null" : moveSeq.owningActor.GameRep.name),true);
      AlternateMechRepresentation alternateRep = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return null; }
      AlternateMechRepresentations alternateReps = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentations>();
      if (alternateReps != null) { if (alternateReps.isHovering) { Log.WL(1, "Hovering"); return null; } }
      return AudioEventManager.CreateVOQueue(id, queueCompleteDelay, queueCompleteMessage, queueCompleteCallback);
    }
    public static void QueueVOEvent(string queueId, VOEvents voEvent, AbstractActor audioActor, ActorMovementSequence moveSeq) {
      Log.TWL(0, "ActorMovementSequence.OnAdded.QueueVOEvent " + (moveSeq.owningActor.GameRep == null ? "null" : moveSeq.owningActor.GameRep.name));
      AlternateMechRepresentation alternateRep = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return; }
      AlternateMechRepresentations alternateReps = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentations>();
      if (alternateReps != null) { if (alternateReps.isHovering) { Log.WL(1, "Hovering"); return; } }
      AudioEventManager.QueueVOEvent(queueId, voEvent, audioActor);
    }
    public static void StartVOQueue(float initialDelay, ActorMovementSequence moveSeq) {
      Log.TWL(0, "ActorMovementSequence.OnAdded.StartVOQueue " + (moveSeq.owningActor.GameRep == null ? "null" : moveSeq.owningActor.GameRep.name));
      AlternateMechRepresentation alternateRep = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentation>();
      if (alternateRep != null) { return; }
      AlternateMechRepresentations alternateReps = moveSeq.owningActor.GameRep.GetComponent<AlternateMechRepresentations>();
      if (alternateReps != null) { if (alternateReps.isHovering) { Log.WL(1,"Hovering"); return; } }
      AudioEventManager.StartVOQueue(initialDelay);
    }
  }
  [HarmonyPatch(typeof(MechRepresentationSimGame))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(MechDef), typeof(Transform), typeof(HeraldryDef) })]
  public static class MechRepresentationSimGame_InitSquad {
    public static bool Prefix(MechRepresentationSimGame __instance, DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      UnitCustomInfo info = mechDef.GetCustomInfo();
      if (info == null) { return true; }
      if (info.SquadInfo.Troopers >= 1) {
        if (__instance.GetComponent<TrooperRepresentationSimGame>() != null) {
          Log.TWL(0, "TrooperRepresentationSimGame.Init");
        } else {
          Log.TWL(0, "SquadRepresentationSimGame.Init");
          SquadRepresentationSimGame squadRepresentationSimGame = __instance.GetComponent<SquadRepresentationSimGame>();
          if (squadRepresentationSimGame == null) {
            List<GameObject> squadTroopers = new List<GameObject>();
            for (int index = 0; index < info.SquadInfo.Troopers; ++index) {
              GameObject squadTrooper = GameObject.Instantiate(__instance.gameObject);
              squadTroopers.Add(GameObject.Instantiate(__instance.gameObject));
            }
            squadRepresentationSimGame = __instance.gameObject.AddComponent<SquadRepresentationSimGame>();
            squadRepresentationSimGame.Instantine(mechDef, squadTroopers);
          }
          squadRepresentationSimGame.Init(dataManager, mechDef, parentTransform, heraldryDef);
          return true;
        }
      }
      if (info.quadVisualInfo.UseQuadVisuals) {
        QuadRepresentationSimGame quadRepresentationSimGame = __instance.GetComponent<QuadRepresentationSimGame>();
        if(quadRepresentationSimGame == null) {
          quadRepresentationSimGame = __instance.gameObject.AddComponent<QuadRepresentationSimGame>();
          quadRepresentationSimGame.Instantine(__instance,dataManager,mechDef);
        }
      }
      mechDef.SpawnCustomParts(__instance);
      return true;
    }
    public static void Postfix(MechRepresentationSimGame __instance, DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      SquadRepresentationSimGame squadRepresentationSimGame = __instance.GetComponent<SquadRepresentationSimGame>();
      if (squadRepresentationSimGame != null) {
        string arg = string.IsNullOrEmpty(mechDef.prefabOverride) ? mechDef.Chassis.PrefabBase : mechDef.prefabOverride.Replace("chrPrfMech_", "");
        arg += "_squad";
        __instance.prefabName = string.Format("chrPrfComp_{0}_simgame", arg);
      }
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
    public static void LoadDamageState(this MechRepresentationSimGame simRep) { i_LoadDamageState(simRep); }
    public static void CollapseLocation(this MechRepresentationSimGame simRep, int location, bool isDestroyed) { i_CollapseLocation(simRep, location, isDestroyed); }
    public static bool Prefix(MechRepresentationSimGame __instance) {
      if (__instance.gameObject.GetComponent<SquadRepresentationSimGame>() != null) {
        __instance.gameObject.GetComponent<SquadRepresentationSimGame>().LoadDamageState();
        return false;
      }
      if (__instance.gameObject.GetComponent<TrooperRepresentationSimGame>() != null) {
        //__instance.gameObject.GetComponent<TrooperRepresentationSimGame>().LoadDamageState(isDestroyed);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("GetHitPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_GetHitPosition {
    public static void Postfix(MechRepresentation __instance, int location, ref Vector3 __result) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if (altReps != null) { altReps.GetHitPosition(location, ref __result); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("GetVFXTransform")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_GetVFXTransform {
    public static void Postfix(MechRepresentation __instance, int location, ref Transform __result) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if (altReps != null) { __result = altReps.GetVFXTransform(location); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(string), typeof(bool), typeof(Vector3), typeof(bool), typeof(float) })]
  public static class MechRepresentation_PlayVFX {
    public static bool Prefix(MechRepresentation __instance, int location, string vfxName, bool attached, Vector3 lookAtPos, bool oneShot, float duration) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if (altReps != null) {
          if (__instance.BlipDisplayed || string.IsNullOrEmpty(vfxName)) { return true; }
          altReps.PlayVFX(location, vfxName, attached, lookAtPos, oneShot, duration);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform) })]
  public static class ActorMovementSequence_Init {
    public static void Postfix(ActorMovementSequence __instance, AbstractActor actor, Transform xform, ICombatant ___meleeTarget) {
      try {
        Log.TWL(0,"ActorMovementSequence.Init ");
        AlternateMechRepresentations altReps = __instance.ActorRep.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if (altReps != null) {
          altReps.StartMovement();
        }
        CustomQuadLegController[] customMoveAnimators = xform.gameObject.GetComponentsInChildren<CustomQuadLegController>();
        foreach (CustomQuadLegController customMoveAnimatior in customMoveAnimators) {
          Log.LogWrite(" CustomMoveAnimator:" + customMoveAnimatior.name + "\n");
          customMoveAnimatior.StartAnimation();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
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
        Log.TWL(0, "MechMeleeSequence.ExecuteMove ");
        AlternateMechRepresentations altReps = __instance.MechRep.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if(altReps != null) {
          if (altReps.isHovering) {
            altReps.InitMeleeTargetHeight(__instance.MeleeTarget.getFlyingRepMeleeHeight());
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("FacePoint")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(Vector3), typeof(bool), typeof(float), typeof(int), typeof(int), typeof(bool), typeof(GameRepresentation.RotationCompleteDelegate) })]
  public static class PilotableActorRepresentation_FacePoint {
    public static bool Prefix(PilotableActorRepresentation __instance, bool isParellelSequence, Vector3 lookAt, bool isLookVector, float twistTime, int stackItemUID, int sequenceId,bool isMelee, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      try {
        Log.TWL(0, "PilotableActorRepresentation.FacePoint rep:" + __instance.name + " actor:"+ new Text(__instance.parentActor.DisplayName).ToString() + " lookAt:" + lookAt + " isMelee:" + isMelee + " curAngle:" + __instance.currentTwistAngle);
        //bool intIsMelee = isMelee;
        //AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        //if (altReps != null) {
        //  Log.WL(1, "AlternateMechRepresentations exists. Is hovering:"+ altReps.isHovering + " return to netural:"+(lookAt == __instance.thisTransform.forward));
        //  if (altReps.isHovering) { intIsMelee = true; }
        //}
        CustomTwistSequence actorTwistSequence = new CustomTwistSequence(__instance.parentActor, lookAt, isLookVector, isMelee, twistTime, stackItemUID, sequenceId, completeDelegate);
        if (isParellelSequence)
          __instance.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddParallelSequenceToStackMessage((IStackSequence)actorTwistSequence));
        else
          __instance.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)actorTwistSequence));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("ReturnToNeutralFacing")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(float), typeof(int), typeof(int), typeof(GameRepresentation.RotationCompleteDelegate) })]
  public static class PilotableActorRepresentation_ReturnToNeutralFacing {
    public static bool Prefix(PilotableActorRepresentation __instance, bool isParellelSequence, float twistTime, int stackItemUID, int sequenceId, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      try {
        __instance.FacePoint(isParellelSequence, __instance.parentActor.CurrentRotation * Vector3.forward, true, twistTime, stackItemUID, sequenceId, false, completeDelegate);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("InitPaintScheme")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(HeraldryDef), typeof(string) })]
  public static class PilotableActorRepresentation_InitPaintScheme {
    public static void Postfix(PilotableActorRepresentation __instance, HeraldryDef heraldryDef, string teamGUID) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent(typeof(AlternateMechRepresentations)) as AlternateMechRepresentations;
        if (altReps != null) { altReps.InitPaintScheme(heraldryDef, teamGUID); }
        MechRepresentation mechRep = __instance as MechRepresentation;
        if (mechRep != null) {
          TrooperSquad squad = mechRep.parentMech as TrooperSquad;
          if (squad != null) {
            if (squad.MechReps.Contains(mechRep)) { return; }
            foreach (var sRep in squad.squadReps) {
              sRep.Value.MechRep.InitPaintScheme(heraldryDef, teamGUID);
            }
          }
        }
        QuadRepresentation quadRep = __instance.GetComponent<QuadRepresentation>();
        if (quadRep != null) { quadRep.InitPaintScheme(heraldryDef, teamGUID); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return;
      }
      return;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("WorkingJumpjets")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_WorkingJumpjets {
    public static void Postfix(Mech __instance, ref int __result) {
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) {
          __result = squad.isHasWorkingJumpjets() ? 1 : 0;
        }
        if (__instance.GameRep != null) {
          AlternateMechRepresentations altReps = __instance.GameRep.GetComponent<AlternateMechRepresentations>();
          if (altReps != null) {
            if (altReps.NoJumpjetsBlock == false) {
              if (altReps.isHovering && (altReps.HoveringHeight > Core.Settings.MaxHoveringHeightWithWorkingJets)) { __result = 0; }
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch("OnDestroy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class GameRepresentation_OnDestroy {
    public static bool Prefix(GameRepresentation __instance) {
      try {
        __instance.OnCombatGameDestroyed();
        if ((UnityEngine.Object)__instance.audioObject != (UnityEngine.Object)null) {
          AkSoundEngine.StopAll(__instance.audioObject.gameObject);
        }
        if (__instance.persistentVFXParticles != null) { __instance.persistentVFXParticles.Clear(); };
        if (__instance.pilotRep != null) {
          __instance.pilotRep.gameRep = (GameRepresentation)null;
        }
        __instance.pilotRep = (PilotRepresentation)null;
        Traverse.Create(__instance).Field<PropertyBlockManager>("_propertyBlock").Value = (PropertyBlockManager)null;
        if(__instance.renderers != null) __instance.renderers.Clear();
        Traverse.Create(__instance).Field < ICombatant > ("_parentCombatant").Value = (ICombatant)null;
        Traverse.Create(__instance).Field<AbstractActor>("_parentActor").Value = (AbstractActor)null;
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
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
      try {
        if (__instance.SelectedActor.isDFAForbidden()) { __result.Clear(); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("CanDFATargetFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3) })]
  public static class Mech_CanDFATargetFromPosition {
    public static void Postfix(Mech __instance, ICombatant target, Vector3 position, ref bool __result) {
      try {
        if(__result == true) { if (__instance.isDFAForbidden()) { __result = false; } }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GuardLevel")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_GuardLevel {
    public static void Postfix(Mech __instance, ref int __result) {
      try {
        if (__result != 0) { if (__instance.isDFAForbidden()) { __result = 0; } }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(new Type[] { typeof(ICombatant), typeof(out string) })]
  [HarmonyPatch()]
  public static class Mech_CanEngageTarget {
    public static MethodBase TargetMethod() {
      List<MethodInfo> methods = AccessTools.GetDeclaredMethods(typeof(Mech));
      Log.TWL(0, "Mech.CanEngageTarget searching");
      foreach (MethodInfo info in methods) {
        Log.WL(1, info.Name);
        if (info.Name != "CanEngageTarget") { continue; }
        ParameterInfo[] pars = info.GetParameters();
        Log.WL(2, "params:"+pars.Length);
        foreach (ParameterInfo pinfo in pars) { Log.WL(3, pinfo.ParameterType.ToString()); }
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
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}