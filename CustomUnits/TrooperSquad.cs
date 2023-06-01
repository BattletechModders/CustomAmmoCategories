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
using BattleTech.UI;
using CustAmmoCategories;
using CustomComponents;
using HarmonyLib;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using MessagePack;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("LethalInjurePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameConstants), typeof(string), typeof(int), typeof(bool), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
  public static class Pilot_LethalInjurePilot {
    public static void Prefix(ref bool __runOriginal, Pilot __instance, CombatGameConstants constants, string sourceID, int stackItemUID, bool isLethal, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.ParentActor is TrooperSquad squad) {
          if (squad.GetOperationalUnitsCount() > 0) {
            Log.Combat?.TWL(0, $"!!!Exception!!! Someone tries to LethalInjurePilot squad {squad.PilotableActorDef.ChassisID} illegally should be punished");
            Log.Combat?.WL(0, Environment.StackTrace);
            __runOriginal = false; return;
          }
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("MaxInjurePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameConstants), typeof(string), typeof(int), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
  public static class Pilot_MaxInjurePilot {
    public static void Prefix(ref bool __runOriginal, Pilot __instance, CombatGameConstants constants, string sourceID, int stackItemUID, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
      try {
        if (!__runOriginal) { return; }
        if (__instance.ParentActor is TrooperSquad squad) {
          if (squad.GetOperationalUnitsCount() > 0) {
            Log.Combat?.TWL(0, $"!!!Exception!!! Someone tries to MaxInjurePilot squad {squad.PilotableActorDef.ChassisID} illegally should be punished");
            Log.Combat?.WL(0, Environment.StackTrace);
            __runOriginal = false; return;
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InjurePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
  public static class Pilot_InjurePilot {
    public static void Postfix(Pilot __instance, string sourceID, int stackItemUID, int dmg, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
      try {
        if (__instance.ParentActor is TrooperSquad squad) {
          if (TrooperSquad.SafeInjurePilot) { return; }
          Log.Combat?.TWL(0, $"!!!Exception!!! Someone tries to InjurePilot squad {squad.PilotableActorDef.ChassisID} illegally should be punished");
          Log.Combat?.WL(0, Environment.StackTrace);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MessageCenter))]
  [HarmonyPatch("PublishMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MessageCenter_PublishMessage {
    public static void Prefix(CombatHUDInWorldElementMgr __instance, MessageCenterMessage message) {
      try {
        if (message.MessageType != MessageCenterMessageType.FloatieMessage) { return; }
        FloatieMessage msg = message as FloatieMessage;
        if (msg == null) { return; }
        //Log.TWL(0, "MessageCenter.PublishMessage " + msg.text+" nature:"+msg.nature+" GUID:"+msg.actingObjectGuid+" GUID:"+msg.affectedObjectGuid);
        //Log.WL(0,Environment.StackTrace);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch("DamageComponent")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(ComponentDamageLevel), typeof(bool) })]
  public static class MechComponent_DamageComponent {
    public static void Postfix(MechComponent __instance, WeaponHitInfo hitInfo, ComponentDamageLevel damageLevel, bool applyEffects) {
      try {
        Log.Combat?.TWL(0, "MechComponent.DamageComponent Postfix " + __instance.defId + " DamageLevel:" + __instance.DamageLevel + "/" + damageLevel);
        if ((__instance.DamageLevel >= ComponentDamageLevel.Destroyed) || (damageLevel >= ComponentDamageLevel.Destroyed)) {
          if(__instance.parent is TrooperSquad squad) {
            squad.ResetJumpjetlocationsCache();
          }
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.damageLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("KillPilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameConstants), typeof(string), typeof(int), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
  public static class Pilot_KillPilot {
    public static void Prefix(ref bool __runOriginal, Pilot __instance, CombatGameConstants constants, string sourceID, int stackItemUID, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
      try {
        Log.Combat?.TWL(0, "Pilot.KillPilot "+__instance.Callsign+" "+(__instance.ParentActor == null?"null":__instance.ParentActor.PilotableActorDef.ChassisID));
        if(__instance.ParentActor is TrooperSquad squad){
          Log.Combat?.WL(1,"squad can't be killed that way");
          __runOriginal = false; return;
        }
        return;
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.damageLogger.LogException(e);
        return;
      }
    }
  }

  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("StartPersistentAudio")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class MechRepresentation_StartPersistentAudioSquad {
  //  public static void Postfix(MechRepresentation __instance) {
  //    try {
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad == null) { return; }
  //      if (squad.MechReps.Contains(__instance)) { return; }
  //      foreach (var sRep in squad.squadReps) {
  //        if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
  //        sRep.Value.MechRep.StartPersistentAudio();
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("GetHitPosition")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int) })]
  //public static class MechRepresentation_GetHitPositionSquad {
  //  public static void Postfix(MechRepresentation __instance, int location,ref Vector3 __result) {
  //    try {
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad == null) { return; }
  //      ArmorLocation armorLocation = (ArmorLocation)location;
  //      if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)){
  //        __result = trooperRep.MechRep.vfxCenterTorsoTransform.position;
  //      } else {
  //        __result = __instance.parentCombatant.CurrentPosition;
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("PlayPersistentDamageVFX")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int) })]
  //public static class MechRepresentation_PlayPersistentDamageVFXSquad {
  //  public static bool Prefix(MechRepresentation __instance, int location,ref List<string> ___persistentDmgList) {
  //    try {
  //      AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
  //      if(altReps != null) {
  //        altReps.PlayPersistentDamageVFX(location);
  //        return false;
  //      }
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad != null) {
  //        ArmorLocation armorLocation = (ArmorLocation)location;
  //        if (___persistentDmgList.Count <= 0) { return true; }
  //        int index = UnityEngine.Random.Range(0, ___persistentDmgList.Count);
  //        string persistentDmg = ___persistentDmgList[index];
  //        ___persistentDmgList.RemoveAt(index);
  //        if (persistentDmg.Contains("Smoke")) {
  //          int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_fire_small_internal, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
  //        } else if (persistentDmg.Contains("Electrical")) {
  //          int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_electrical_start, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
  //        } else {
  //          int num3 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_sparks_start, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
  //        }
  //        Transform parentTransform = __instance.GetVFXTransform((int)ArmorLocation.CenterTorso);
  //        if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)) {
  //          parentTransform = trooperRep.MechRep.vfxCenterTorsoTransform;
  //        }
  //        __instance.PlayVFXAt(parentTransform, Vector3.zero, persistentDmg, true, Vector3.zero, false, -1f);
  //        return false;
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return true;
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("PlayComponentCritVFX")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int) })]
  //public static class MechRepresentation_PlayComponentCritVFXSquad {
  //  public static bool Prefix(MechRepresentation __instance, int location, ref List<string> ___persistentCritList) {
  //    try {
  //      AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
  //      if (altReps != null) {
  //        altReps.PlayComponentCritVFX(location);
  //        return false;
  //      }
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad != null) {
  //        ArmorLocation armorLocation = (ArmorLocation)location;
  //        if (___persistentCritList.Count <= 0) { return true; };
  //        int index = UnityEngine.Random.Range(0, ___persistentCritList.Count);
  //        string persistentCrit = ___persistentCritList[index];
  //        ___persistentCritList.RemoveAt(index);
  //        Transform parentTransform = __instance.GetVFXTransform((int)ArmorLocation.CenterTorso);
  //        if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)) {
  //          parentTransform = trooperRep.MechRep.vfxCenterTorsoTransform;
  //        }
  //        __instance.PlayVFXAt(parentTransform, Vector3.zero, persistentCrit, true, Vector3.zero, false, -1f);
  //        return false;
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return true;
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("StartJumpjetAudio")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class MechRepresentation_StartJumpjetAudioSquad {
  //  public static void Postfix(MechRepresentation __instance) {
  //    try {
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad == null) { return; }
  //      if (squad.MechReps.Contains(__instance)) { return; }
  //      foreach (var sRep in squad.squadReps) {
  //        if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
  //        sRep.Value.MechRep.StartJumpjetAudio();
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("StopJumpjetAudio")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class MechRepresentation_StopJumpjetAudioSquad {
  //  public static void Postfix(MechRepresentation __instance) {
  //    try {
  //      TrooperSquad squad = __instance.parentMech as TrooperSquad;
  //      if (squad == null) { return; }
  //      if (squad.MechReps.Contains(__instance)) { return; }
  //      foreach (var sRep in squad.squadReps) {
  //        if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
  //        sRep.Value.MechRep.StopJumpjetAudio();
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetCurrentStructure")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations) })]
  public static class Mech_GetCurrentStructureSquad {
    public static void Postfix(Mech __instance, ChassisLocations location, ref float __result) {
      try {
        if (__instance == null) { return; }
        if (__instance.MechDef == null) { return; }
        if (__instance.MechDef.Chassis == null) { return; }
        LocationDef locDef = __instance.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { __result = 0f; };
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.attackLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InjuryReasonDescription")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Pilot_InjuryReasonDescription {
    public static void Postfix(Pilot __instance, ref string __result) {
      try {
        if (__instance.ParentActor == null) { return; }
        TrooperSquad squad = __instance.ParentActor as TrooperSquad;
        if (squad == null) { return; }
        __result = "UNIT DESTROYED";
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.attackLogger.LogException(e);
      }
    }
  }
  [MessagePackObject]
  public class TrooperSquadDef {
    [Key(0)]
    public int Troopers { get; set; } = 0;
    [Key(1)]
    public int DeadUnitToHitMod { get; set; } = 0;
    [Key(2)]
    public float UnitSize { get; set; } = 1f;
    [Key(3)]
    public WeightClass weightClass { get; set; } = WeightClass.MEDIUM;
    [Key(4)]
    public Dictionary<string, ChassisLocations> Hardpoints { get; set; } = new Dictionary<string, ChassisLocations>();
    [Key(5)]
    public string armorIcon { get; set; } = string.Empty;
    [Key(6)]
    public string outlineIcon { get; set; } = string.Empty;
    [Key(7)]
    public string structureIcon { get; set; } = string.Empty;
    public TrooperSquadDef() {
      //Hardpoints = new Dictionary<string, ChassisLocations>();
      //UnitSize = 1f;
      //DeadUnitToHitMod = 0;
      //weightClass = WeightClass.MEDIUM;
    }
  }
  public class TrooperRepresentation {
    public Vector3 delta { get; private set; }
    public GameObject GameRep { get;private set; }
    public MechRepresentation MechRep { get; private set; }
    public CustomRepresentation CustRep { get; private set; }
    public Vector3 deadLocation { get; private set; }
    public Quaternion deadRotation { get; private set; }
    private Animator ThisAnimator { get; set; }
    public int ForwardHash { get; set; }
    public int TurnHash { get; set; }
    public int IsMovingHash { get; set; }
    public int BeginMovementHash { get; set; }
    public int DamageHash { get; set; }
    public bool HasForwardParam { get; set; }
    public bool HasTurnParam { get; set; }
    public bool HasIsMovingParam { get; set; }
    public bool HasBeginMovementParam { get; set; }
    public bool HasDamageParam { get; set; }
    public ChassisLocations location { get; set; }
    public float TurnParam {
      set {
        if (!this.HasTurnParam)
          return;
        this.ThisAnimator.SetFloat(this.TurnHash, value);
      }
    }
    public float ForwardParam {
      set {
        if (!this.HasForwardParam)
          return;
        this.ThisAnimator.SetFloat(this.ForwardHash, value);
      }
    }
    public bool IsMovingParam {
      set {
        if (!this.HasIsMovingParam)
          return;
        this.ThisAnimator.SetBool(this.IsMovingHash, value);
      }
    }
    public bool BeginMovementParam {
      set {
        if (!this.HasBeginMovementParam)
          return;
        this.ThisAnimator.SetTrigger(this.BeginMovementHash);
      }
    }
    public float DamageParam {
      set {
        if (!this.HasDamageParam)
          return;
        this.ThisAnimator.SetFloat(this.DamageHash, value);
      }
    }
    public void HandleDeath(DeathMethod deathMethod, int location) {
      this.deadLocation = this.MechRep.transform.position;
      this.deadRotation = this.MechRep.transform.rotation;
      this.MechRep.HandleDeath(deathMethod, location);
    }
    public TrooperRepresentation(GameObject rep, Vector3 posDelta, ChassisLocations location) {
      this.location = location;
      this.delta = posDelta;
      this.GameRep = rep;
      this.MechRep = rep.GetComponent<MechRepresentation>();
      this.ThisAnimator = this.GameRep.GetComponent<Animator>();
      this.ForwardHash = Animator.StringToHash("Forward");
      this.TurnHash = Animator.StringToHash("Turn");
      this.IsMovingHash = Animator.StringToHash("IsMoving");
      this.BeginMovementHash = Animator.StringToHash("BeginMovement");
      this.DamageHash = Animator.StringToHash("Damage");
      AnimatorControllerParameter[] parameters = this.ThisAnimator.parameters;
      for (int index = 0; index < parameters.Length; ++index) {
        if (parameters[index].nameHash == this.ForwardHash)
          this.HasForwardParam = true;
        if (parameters[index].nameHash == this.TurnHash)
          this.HasTurnParam = true;
        if (parameters[index].nameHash == this.IsMovingHash)
          this.HasIsMovingParam = true;
        if (parameters[index].nameHash == this.BeginMovementHash)
          this.HasBeginMovementParam = true;
        if (parameters[index].nameHash == this.DamageHash)
          this.HasDamageParam = true;
      }
    }
  }
  public class TrooperSquad: CustomMech, ICustomMech {
    public static bool SafeInjurePilot { get; set; } = false;
    public static bool SafeSetNeedsInjury { get; set; } = false;
    public static readonly List<ChassisLocations> locations = new List<ChassisLocations>() { ChassisLocations.Head, ChassisLocations.CenterTorso, ChassisLocations.LeftTorso, ChassisLocations.RightTorso, ChassisLocations.LeftArm, ChassisLocations.RightArm, ChassisLocations.LeftLeg, ChassisLocations.RightLeg };
    public static readonly List<ArmorLocation> armorLocations = new List<ArmorLocation>() { ArmorLocation.Head, ArmorLocation.CenterTorso, ArmorLocation.LeftTorso, ArmorLocation.RightTorso, ArmorLocation.LeftArm, ArmorLocation.RightArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    public static readonly Dictionary<ChassisLocations, float> positions = new Dictionary<ChassisLocations, float>() {
      { ChassisLocations.CenterTorso, 25f }, { ChassisLocations.LeftTorso, 115f }, { ChassisLocations.RightTorso, 205f }, { ChassisLocations.LeftArm, -65f },
      { ChassisLocations.RightArm, 70f }, { ChassisLocations.LeftLeg, 160f }, { ChassisLocations.RightLeg, -20f }
    };
    public static readonly float SquadRadius = 7f;
    public virtual SquadRepresentation squadRepresentation { get { return this.GameRep as SquadRepresentation; } }
    //public Dictionary<ChassisLocations, TrooperRepresentation> squadReps;
    //public HashSet<TrooperRepresentation> Reps;
    //public HashSet<MechRepresentation> MechReps;
    public UnitCustomInfo info;
    public override bool isQuad { get { return false; } }
    public override string DefaultStructureDef { get { return "squad"; } }

    public static void Pilot_IsIncapacitated_Patch(Pilot __instance,ref bool __result) {
      if (__instance.ParentActor is TrooperSquad squad) { __result = squad.GetOperationalUnitsCount() <= 0; return; }
    }
    public static MethodBase Pilot_IsIncapacitated_get() { return AccessTools.Property(typeof(Pilot), "IsIncapacitated").GetGetMethod(); }
    public static HarmonyMethod Pilot_IsIncapacitated_get_patch() { return new HarmonyMethod(AccessTools.Method(typeof(TrooperSquad), nameof(Pilot_IsIncapacitated_Patch))); }
    public TrooperSquad(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
                  :base (mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef)
    {
      //Reps = new HashSet<TrooperRepresentation>();
      //MechReps = new HashSet<MechRepresentation>();
      info = mDef.GetCustomInfo();
      if (info == null) {
        throw new NullReferenceException("UnitCustomInfo is not defined for "+mDef.ChassisID);
      }
    }
    public override void FlagForDeath(string reason, DeathMethod deathMethod, DamageType damageType, int location, int stackItemID, string attackerID, bool isSilent) {
      if (this._flaggedForDeath) { return; }
      Log.Combat?.TWL(0, "TrooperSquad.FlagForDeath " + reason + " method:" + deathMethod + " dmgType:" + damageType + " location:" + location);
      bool hasNonDestroyedLocations = false;
      Log.Combat?.WL(1, "testing locations");
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        Log.Combat?.W(2, loc.ToString());
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) {
          Log.Combat?.WL(1, "not exists MaxArmor:" + locDef.MaxArmor + " InternalStructure:" + locDef.InternalStructure);
          continue;
        }
        Log.Combat?.WL(1, "IsLocationDestroyed:" + this.IsLocationDestroyed(loc));
        if (this.IsLocationDestroyed(loc) == false) { hasNonDestroyedLocations = true; break; }
      }
      if (hasNonDestroyedLocations) {
        Log.Combat?.WL(1, "refuse to death cause have non destroyed locations");
        Log.Combat?.WL(0, Environment.StackTrace);
        return;
      }
      base.FlagForDeath(reason, deathMethod, damageType, location, stackItemID, attackerID, isSilent);
    }
    public void OnLocationDestroyedSquad(ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      Log.Combat?.TWL(0, "TrooperSquad.OnLocationDestroyedSquad "+this.MechDef.ChassisID+" "+location);
      this.location_index = -1;
      this.ResetJumpjetlocationsCache();
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)this, new Text("UNIT DESTROYED"), FloatieMessage.MessageNature.LocationDestroyed, true)));
      AttackDirector.AttackSequence attackSequence = this.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) { attackSequence.FlagAttackDestroyedAnyLocation(this.GUID); };
      UnitCustomInfo info = this.GetCustomInfo();
      foreach (MechComponent allComponent in this.allComponents) {
        if ((ChassisLocations)allComponent.Location == location) {
          if (allComponent.componentDef.Is<CustomComponents.Flags>(out var f) && f.IsSet("ignore_damage")) { continue; }
          allComponent.DamageComponent(hitInfo, ComponentDamageLevel.Destroyed, false);
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("====@@@ Component Destroyed: {0}", (object)allComponent.Name));
          if (attackSequence != null) {
            Weapon weapon = allComponent as Weapon;
            AmmunitionBox ammoBox = allComponent as AmmunitionBox;
            attackSequence.FlagAttackScoredCrit(this.GUID, weapon, ammoBox);
          }
        }
      }
      bool hasNotDestroyedLocations = false;
      Log.Combat?.WL(1,"testing locations");
      foreach(ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        Log.Combat?.W(2, loc.ToString());
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) {
          Log.Combat?.WL(1, "not exists MaxArmor:" + locDef.MaxArmor + " InternalStructure:" + locDef.InternalStructure);
          continue;
        }
        Log.Combat?.WL(1, "IsLocationDestroyed:" + this.IsLocationDestroyed(loc));
        if (this.IsLocationDestroyed(loc) == false) { hasNotDestroyedLocations = true; break; }
      }
      DeathMethod deathMethod = DeathMethod.NOT_SET;
      string reason = "";
      if (hasNotDestroyedLocations == false) {
        Log.Combat?.WL(1, "all trooper squad locations destroyed");
        deathMethod = DeathMethod.HeadDestruction;
        reason = "Squad destroyed";
        if (damageType == DamageType.AmmoExplosion) {
          deathMethod = DeathMethod.AmmoExplosion;
          reason = "Ammo Explosion: " + location.ToString();
        } else if (damageType == DamageType.ComponentExplosion) {
          deathMethod = DeathMethod.ComponentExplosion;
          reason = "Component Explosion: " + location.ToString();
        }
      }
      if (deathMethod != DeathMethod.NOT_SET) {
        this.FlagForDeath(reason, deathMethod, damageType, (int)location, hitInfo.stackItemUID, hitInfo.attackerId, false);
      } else {
        Pilot pilot = this.GetPilot();
        TrooperSquad.SafeSetNeedsInjury = true;
        if (pilot != null) { pilot.SetNeedsInjury(InjuryReason.HeadHit); }
        TrooperSquad.SafeSetNeedsInjury = false;
      }
      this.GameRep.PlayComponentDestroyedVFX((int)location, attackDirection);
      //if (this.squadReps.TryGetValue(location, out TrooperRepresentation trooperRep)) {
        //trooperRep.HandleDeath(DeathMethod.Unknown, (int)ChassisLocations.CenterTorso);
      //}
    }
    public override void _ApplyArmorStatDamage(ArmorLocation location, float damage, WeaponHitInfo hitInfo) {
      this.statCollection.ModifyStat<float>(hitInfo.attackerId, hitInfo.stackItemUID, this.GetStringForArmorLocation(location), StatCollection.StatOperation.Float_Subtract, damage);
      this.OnArmorDamaged((int)location, hitInfo, damage);
    }
    public Transform GetAttachTransform(MechRepresentation gameRep, ChassisLocations location) {
      if ((UnityEngine.Object)gameRep == (UnityEngine.Object)null)
        throw new ArgumentNullException("GetAttachTransform requires a valid GameRep!");
      switch (location) {
        case ChassisLocations.LeftArm:
        return gameRep.LeftArmAttach;
        case ChassisLocations.RightArm:
        return gameRep.RightArmAttach;
        case ChassisLocations.LeftLeg:
        return gameRep.LeftLegAttach;
        case ChassisLocations.RightLeg:
        return gameRep.RightLegAttach;
        default:
        return gameRep.TorsoAttach;
      }
    }
    public override float SummaryStructureCurrent {
      get {
        if (this.IsDead) { return 0.0f; }
        float result = 0f;
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          result += this.GetCurrentStructure(location);
        }
        return result;
      }
    }
    public override float SummaryArmorCurrent {
      get {
        float result = 0f;
        foreach (ArmorLocation location in TrooperSquad.armorLocations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(location));
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          result += this.GetCurrentArmor(location);
        }
        return result;
      }
    }
    public override bool IsAnyStructureExposed {
      get {
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          if (this.GetCurrentStructure(location) <= 0f) { return true; }
        }
        return false;
      }
    }
    public override bool IsDead {
      get {
        if (this.HasHandledDeath) { return true; }
        if (this.pilot.IsIncapacitated) { return true; }
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          if (this.GetCurrentStructure(location) > 0f) { return false; }          
        }
        return true;
      }
    }
    public override bool _MoveMultiplierOverride { get { return true; } }
    public override float _MoveMultiplier { get { return 1f; } }

    //public override float MaxWalkDistance {
    //  get {
    //    return this.WalkSpeed;
    //  }
    //}
    //public override float MaxSprintDistance {
    //  get {
    //    return this.RunSpeed;
    //  }
    //}
    public override bool CanSprint {
      get {
        return !this.HasFiredThisRound;
      }
    }
    //public override float MaxBackwardDistance {
    //  get {
    //    return this.WalkSpeed;
    //  }
    //}
    public override void VentCoolant() {
    }
    public override bool IsProne {
      get { return false; }
      protected set { return; }
    }
    private HashSet<ChassisLocations> workingJumpsLocations_cache = null;
    public void ResetJumpjetlocationsCache() {
      workingJumpsLocations_cache = null;
    }
    public HashSet<ChassisLocations> workingJumpsLocations() {
      if (workingJumpsLocations_cache != null) { return workingJumpsLocations_cache; }
      Log.Combat?.TWL(0, "TrooperSquad.workingJumpsLocations "+this.PilotableActorDef.ChassisID);
      workingJumpsLocations_cache = new HashSet<ChassisLocations>();
      foreach (Jumpjet component in jumpjets) {
        if (component.IsFunctional == false) { continue; }
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(component.mechComponentRef.MountedLocation);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(component.mechComponentRef.MountedLocation)) { continue; }
        if (workingJumpsLocations_cache.Contains(component.mechComponentRef.MountedLocation) == false) {
          Log.Combat?.WL(1, component.mechComponentRef.MountedLocation+" has working jumpjets");
        }
        workingJumpsLocations_cache.Add(component.mechComponentRef.MountedLocation);
      }
      bool notAllLocations = false;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(loc)) { continue; }
        if (workingJumpsLocations_cache.Contains(loc) == false) {
          Log.Combat?.WL(1, loc + " has no working jumpjets");
          notAllLocations = true; break;
        }
      }
      if (notAllLocations) { workingJumpsLocations_cache.Clear(); }
      return workingJumpsLocations_cache;
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      List<int> result = new List<int>();
      foreach(ArmorLocation alocation in TrooperSquad.locations) {
        ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(location)) { continue; }
        result.Add((int)alocation);
      }
      return result;
    }
    public static readonly int ToHitTableSumm = 100;
    private static ArmorLocation GetAdjacentLocations_cache = ArmorLocation.None;
    public override ArmorLocation GetAdjacentLocations(ArmorLocation location) {
      if (GetAdjacentLocations_cache != ArmorLocation.None) { return GetAdjacentLocations_cache; }
      foreach (ArmorLocation alocation in TrooperSquad.locations) { GetAdjacentLocations_cache |= alocation; }
      return GetAdjacentLocations_cache;
    }
    private static Dictionary<int, Dictionary<ArmorLocation, int>> GetHitTableSquad_cache = new Dictionary<int, Dictionary<ArmorLocation, int>>();
    private static Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetHitTableCluster_cache = new Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    private int location_index = -1;
    private bool has_not_destroyed_locations = false;
    private HashSet<ArmorLocation> avaible_locations = new HashSet<ArmorLocation>();
    public int GetOperationalUnitsCount() {
      this.RecalculateAvaibleLocations();
      return this.has_not_destroyed_locations?this.avaible_locations.Count:0;
    }
    public int GetMaxUnitsCount() {
      int result = 0;
      foreach (ChassisLocations location in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++result;
      }
      return result;
    }
    public virtual void RecalculateAvaibleLocations() {
      if (this.location_index == -1) {
        this.location_index = 0;
        this.avaible_locations.Clear();
        this.has_not_destroyed_locations = false;
        foreach (ArmorLocation alocation in TrooperSquad.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          if (this.IsLocationDestroyed(location)) { continue; }
          this.has_not_destroyed_locations = true; break;
        }
        foreach (ArmorLocation alocation in TrooperSquad.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          Log.Combat?.WL(1, "location:" + alocation + "()" + location + " max armor:" + locDef.MaxArmor + " structure:" + locDef.InternalStructure + " is destroyed:" + this.IsLocationDestroyed(location) + " has not destroyed:" + has_not_destroyed_locations);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (this.IsLocationDestroyed(location) && (has_not_destroyed_locations == true)) { continue; }
          this.location_index |= (int)alocation;
          this.avaible_locations.Add(alocation);
        }
        Log.Combat?.WL(1, "recalculated: " + (this.location_index >= 0 ? Convert.ToString(this.location_index, 2).PadLeft(16, '0') : "-1"));
      }
    }
    public override Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      Log.Combat?.TWL(0,"TrooperSquad.GetHitTable "+this.MechDef.Description.Id+" location index:" + (this.location_index>=0?Convert.ToString(this.location_index,2).PadLeft(16,'0'):"-1"));
      Dictionary<ArmorLocation, int> result = null;
      this.RecalculateAvaibleLocations();
      if (GetHitTableSquad_cache.TryGetValue(location_index, out result)) {
        Log.Combat?.WL(1,"cached:"+result.Count);
        return result;
      }
      result = new Dictionary<ArmorLocation, int>();
      if (avaible_locations.Count > 0) {
        foreach (ArmorLocation aloc in avaible_locations) {
          result.Add(aloc, ToHitTableSumm / avaible_locations.Count);
        }
      }
      GetHitTableSquad_cache.Add(location_index, result);
      return result.Count > 0 ? result : null;
    }
    private static Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterTable_cache = new Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public override Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      if (location_index == -1) {
        location_index = 0;
        avaible_locations.Clear();
        foreach (ArmorLocation alocation in TrooperSquad.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (this.IsLocationDestroyed(location)) { continue; }
          location_index |= (int)alocation;
          avaible_locations.Add(alocation);
        }
      }
      if (GetClusterTable_cache.TryGetValue(location_index, out Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> squad_tables) == false) {
        squad_tables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        GetClusterTable_cache.Add(location_index, squad_tables);
      }
      if (squad_tables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (originalLocation == keyValuePair.Key) {
          dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
        } else { 
          dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
        }
      }
      squad_tables.Add(originalLocation, dictionary);
      return dictionary;
    }
    private static Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterHitTableSquad_cache = new Dictionary<int, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public override Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      if (location_index == -1) {
        location_index = 0;
        foreach (ArmorLocation alocation in TrooperSquad.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (this.IsLocationDestroyed(location)) { continue; }
          location_index |= (int)alocation;
          //alocations.Add(alocation);
        }
      }
      if (GetClusterHitTableSquad_cache.TryGetValue(location_index, out Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        GetClusterHitTableSquad_cache.Add(location_index, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      return result;
    }

    //public static Dictionary<ArmorLocation, int> GetHitTable(Mech mech, bool isCluster, ArmorLocation aLoc, AttackDirection from) {
    //  TrooperSquad squad = mech as TrooperSquad;
    //  if (squad == null) { return null; };
    //  return squad.GetHitTable(from);
    //}
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      try {
        Log.Combat?.TWL(0, "TrooperSquad.GetHitLocation " + this.MechDef.Description.Id);
        Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(AttackDirection.FromFront);
        Log.Combat?.WL(1, "hitTable:" + (hitTable == null ? "null" : "not null"));
        int result = (int)HitLocation.GetHitLocation(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier);
        if ((result == 0) || (result == 65535)) {
          Log.Combat?.WL(1, "Exception. something went wrong. location is bad:"+result);
          foreach (var ht in hitTable) {
            Log.Combat?.W(1, ht.Key.ToString() + ":" + ht.Value);
          }
          Log.Combat?.WL(1, "result:"+(ArmorLocation)result);
        }
        return result;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        return 0;
      }
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(AttackDirection.FromFront);
      if ((double)originalMultiplier > 1.00999999046326 || (double)adjacentMultiplier > 1.00999999046326) {
        Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
        foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
          if ((int)keyValuePair.Key == previousHitLocation) {
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)originalMultiplier));
          } else {
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)adjacentMultiplier));
          }
        }
        hitTable = dictionary;
      }
      return (int)HitLocation.GetHitLocation<ArmorLocation>(hitTable, randomRoll, ArmorLocation.None, 0.0f);
    }
    public override bool isSquad { get { return true; } }
    public override bool isVehicle { get { return false; } }
    public override string UnitTypeNameDefault { get { return "SQUAD"; } }

#if BT_PUBLIC_ASSEMBLY
    public 
#else
    protected
#endif
    override float EvaluateExpectedArmorFromAttackDirection(AttackDirection attackDirection) {
      float num1 = 0.0f;
      Dictionary<ArmorLocation, int> mechHitTable = this.GetHitTable(attackDirection);
      if (mechHitTable != null) {
        float num2 = 0.0f;
        foreach (ArmorLocation key in mechHitTable.Keys) {
          int num3 = mechHitTable[key];
          num2 += (float)num3;
        }
        foreach (ArmorLocation key in mechHitTable.Keys) {
          int num3 = mechHitTable[key];
          float num4 = this.ArmorForLocation((int)key) * (float)num3 / num2;
          num1 += num4;
        }
      }
      return num1;
    }
    public override Text GetLongArmorLocation(ArmorLocation location) {
      //Log.TWL(0, "TrooperSquad.GetLongArmorLocation " + this.DisplayName+" "+location);
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
    public override HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      //Log.TWL(0, "TrooperSquad.GetDFASelfDamageLocations " + this.DisplayName);
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach(ArmorLocation aloc in TrooperSquad.armorLocations) {
        ChassisLocations loc = MechStructureRules.GetChassisLocationFromArmorLocation(aloc);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(loc)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public HashSet<ArmorLocation> GetLandmineDamageLocations() {
      //Log.TWL(0, "TrooperSquad.GetLandmineDamageLocations " + this.MechDef.ChassisID);
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach (ArmorLocation aloc in TrooperSquad.armorLocations) {
        ChassisLocations loc = MechStructureRules.GetChassisLocationFromArmorLocation(aloc);
        if (this.IsLocationDestroyed(loc)) { continue; }
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public override HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      //Log.TWL(0, "TrooperSquad.GetBurnDamageLocations " + this.DisplayName);
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach (ArmorLocation aloc in TrooperSquad.armorLocations) {
        ChassisLocations chassisLoc = MechStructureRules.GetChassisLocationFromArmorLocation(aloc);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(chassisLoc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(chassisLoc)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public override Dictionary<int, float> GetAOESpreadArmorLocations() {
      //Log.TWL(0, "TrooperSquad.GetAOESpreadLocations " + m.DisplayName);
      if (CustomAmmoCategories.SquadHitLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); }
      return CustomAmmoCategories.SquadHitLocations;
    }
    public override List<int> GetAOEPossibleHitLocations(Vector3 attackPos) {
      Dictionary<ArmorLocation, int> squadHitTable = this.GetHitTable(this.Combat.HitLocation.GetAttackDirection(attackPos, (ICombatant)this));
      if (squadHitTable == null) return (List<int>)null;
      List<int> intList = new List<int>();
      foreach (ArmorLocation key in squadHitTable.Keys) {
        ChassisLocations chassisLoc = MechStructureRules.GetChassisLocationFromArmorLocation(key);
        LocationDef location = this.MechDef.Chassis.GetLocationDef(chassisLoc);
        if ((location.MaxArmor <= 0f) && (location.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(chassisLoc)) { continue; }
        intList.Add((int)key);
      }
      return intList;
    }
    public static float GetSquadSizeToHitMod(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      //Log.TWL(0, "TrooperSquad.GetSquadSizeToHitMod " + target.DisplayName);
      TrooperSquad squad = target as TrooperSquad;
      if (squad == null) { return 0f; };
      UnitCustomInfo info = squad.GetCustomInfo();
      if (info == null) { return 0f; };
      int deadUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (squad.IsLocationDestroyed(loc)) { ++deadUnitsCount; }
      }
      return Mathf.Round((info.SquadInfo.DeadUnitToHitMod/(info.SquadInfo.Troopers - 1))*deadUnitsCount);
    }
    public static string GetSquadSizeToHitModName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      TrooperSquad squad = target as TrooperSquad;
      if (squad == null) { return string.Empty; };
      Log.Combat?.TWL(0, "TrooperSquad.GetSquadSizeToHitMod " + target.DisplayName);
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return string.Format("UNITS {0}/{1}", liveUnitsCount, allUnitsCount);
    }
    public static float SquadSizeDamageMod(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (weapon.WeaponCategoryValue.IsMelee == false) { return 1f; }
      TrooperSquad squad = weapon.parent as TrooperSquad;
      if (squad == null) { return 1f; };
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return (float)liveUnitsCount / (float)allUnitsCount;
    }
    public static string SquadSizeDamageModName(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (weapon.WeaponCategoryValue.IsMelee == false) { return string.Empty; }
      TrooperSquad squad = weapon.parent as TrooperSquad;
      if (squad == null) { return string.Empty; };
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return string.Format("UNITS {0}/{1}", liveUnitsCount, allUnitsCount);
    }
    public override Vector3 GetImpactPosition(AbstractActor attacker, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation) {
      return this.Combat.LOS.GetImpactPosition(attacker, (ICombatant)this, attackPosition, weapon, ref hitLocation, ref attackDirection, ref secondaryTargetId, ref secondaryHitLocation);
    }
    private void CreateBlankPrefabs(MechRepresentation gameRep, List<string> usedPrefabNames, ChassisLocations location) {
      List<string> componentBlankNames = MechHardpointRules.GetComponentBlankNames(usedPrefabNames, this.MechDef, location);
      Transform attachTransform = this.GetAttachTransform(gameRep, location);
      for (int index = 0; index < componentBlankNames.Count; ++index) {
        WeaponRepresentation component = this.Combat.DataManager.PooledInstantiate(componentBlankNames[index], BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<WeaponRepresentation>();
        component.Init((ICombatant)this, attachTransform, true, this.LogDisplayName, (int)location);
        this.GameRep.weaponReps.Add(component);
      }
    }
#if BT_PUBLIC_ASSEMBLY
    public 
#else
    protected
#endif
    override void InitStats() {
      base.InitStats();
      this.pilot.StatCollection.GetStatistic("Health").SetValue<int>(info.SquadInfo.Troopers);
    }
    public override void CheckPilotStatusFromAttack(string sourceID, int sequenceID, int stackItemID) {
      if (!this.IsPilotable)
        return;
      Pilot pilot = this.GetPilot();
      int currentUnitsCount = this.GetOperationalUnitsCount();
      if (pilot.IsIncapacitated && (currentUnitsCount > 0)) {
        Log.Combat?.TWL(0, $"!!!Exception!!!Something goes wrong Trooper Squad operator can't die while units {currentUnitsCount} operational");
      }
      int effectiveHealth = pilot.TotalHealth - pilot.Injuries;
      int damageCount = 1;
      if (effectiveHealth == currentUnitsCount) {
        pilot.ClearNeedsInjury();
      }else if(effectiveHealth < currentUnitsCount) {
        int maxUnitsCount = this.GetMaxUnitsCount();
        pilot.StatCollection.GetOrCreateStatisic<int>("BonusHealth", 0).SetValue<int>(0);
        pilot.StatCollection.GetOrCreateStatisic<int>("Health", maxUnitsCount).SetValue<int>(maxUnitsCount);
        pilot.StatCollection.GetOrCreateStatisic<int>("Injuries", 0).SetValue<int>(maxUnitsCount - currentUnitsCount);
        pilot.ClearNeedsInjury();
      } else {
        damageCount = effectiveHealth - currentUnitsCount;
      }
      DamageType damageType = DamageType.Combat;
      if (!pilot.IsIncapacitated && pilot.NeedsInjury) {
        TrooperSquad.SafeInjurePilot = true;
        pilot.InjurePilot(sourceID, stackItemID, damageCount, damageType, (Weapon)null, this.Combat.FindActorByGUID(sourceID));
        TrooperSquad.SafeInjurePilot = false;
        if (!pilot.IsIncapacitated) {
          if (this.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_warrior_injured", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          else
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_warrior_injured", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          IStackSequence sequence;
          if (pilot.Injuries == 0) {
            sequence = (IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("{0}: INJURY IGNORED", (object)pilot.InjuryReasonDescription), FloatieMessage.MessageNature.PilotInjury, true);
          } else {
            sequence = (IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("SQUAD UNIT OPERATOR INJURED"), FloatieMessage.MessageNature.PilotInjury, true);
            AudioEventManager.SetPilotVOSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, this);
            AudioEventManager.PlayPilotVO(VOEvents.Pilot_TakeDamage, this, (AkCallbackManager.EventCallback)null, (object)null, true);
          }
          this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(sequence));
        }
        pilot.ClearNeedsInjury();
      }
      if (!pilot.IsIncapacitated)
        return;
      this.FlagForDeath("All Pilots Killed", DeathMethod.PilotKilled, damageType, 1, stackItemID, sourceID, false);
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("ALL PILOTS INCAPACITATED!"), FloatieMessage.MessageNature.PilotInjury, true)));
      this.HandleDeath(sourceID);
    }
    //  public override void InitGameRep(Transform parentTransform) {
    //    UnitCustomInfo info = this.GetCustomInfo();
    //    if (info == null) { base.InitGameRep(parentTransform); return; }
    //    if (info.SquadInfo.Troopers <= 1) { base.InitGameRep(parentTransform); return; }
    //    List<MechComponent> orig_allComponents = this.allComponents;
    //    List<Weapon> orig_weapons = this.weapons;
    //    List<MechComponent> orig_supportComponents = this.supportComponents;
    //    this.allComponents = new List<MechComponent>();
    //    this.weapons = new List<Weapon>();
    //    this.supportComponents = new List<MechComponent>();
    //    Dictionary<MechComponent, ChassisLocations> orig_Locations = new Dictionary<MechComponent, ChassisLocations>();
    //    Log.TWL(0, "TrooperSquad.InitGameRep");
    //    base.InitGameRep(parentTransform);
    //    var identifier = this.MechDef.ChassisID;
    //    Vector3 sizeMultiplier = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
    //    Log.WL(1, $"{identifier}: {sizeMultiplier}");
    //    var originalLOSSourcePositions = Traverse.Create(this).Field("originalLOSSourcePositions").GetValue<Vector3[]>();
    //    var originalLOSTargetPositions = Traverse.Create(this).Field("originalLOSTargetPositions").GetValue<Vector3[]>();
    //    var newSourcePositions = MechResizer.MechResizer.ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
    //    var newTargetPositions = MechResizer.MechResizer.ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
    //    Traverse.Create(this).Field("originalLOSSourcePositions").SetValue(newSourcePositions);
    //    Traverse.Create(this).Field("originalLOSTargetPositions").SetValue(newTargetPositions);
    //    Transform transformToScale = this.GameRep.thisTransform;
    //    Transform j_Root = this.GameRep.gameObject.transform.FindRecursive("j_Root");
    //    if (j_Root != null) { transformToScale = j_Root; }
    //    transformToScale.localScale = sizeMultiplier;
    //    SkinnedMeshRenderer[] meshes = this.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>();
    //    GameObject go = new GameObject("Empty");
    //    Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
    //    go.AddComponent<MeshRenderer>();
    //    if (meshes != null) {
    //      foreach (SkinnedMeshRenderer mesh in meshes) {
    //        mesh.sharedMesh = emptyMesh;
    //      }
    //    }
    //    List<GameObject> headlightReps = Traverse.Create(this.GameRep).Field<List<GameObject>>("headlightReps").Value;
    //    foreach(GameObject light in headlightReps) {
    //      GameObject.Destroy(light);
    //    }
    //    List<JumpjetRepresentation> jumpjetReps = Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
    //    foreach (JumpjetRepresentation jumpjetRep in jumpjetReps) {
    //      GameObject.Destroy(jumpjetRep.gameObject);
    //    }
    //    Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value = new List<JumpjetRepresentation>();
    //    headlightReps.Clear();
    //    GameObject.Destroy(go);
    //    this.allComponents = orig_allComponents;
    //    this.weapons = orig_weapons;
    //    this.supportComponents = orig_supportComponents;
    //    Log.WL(1, "allComponents:" + allComponents.Count);
    //    foreach (MechComponent component in allComponents) {
    //      Log.WL(2, component.defId + ":"+component.mechComponentRef.MountedLocation+":"+component.baseComponentRef.prefabName);
    //    }
    //    Log.WL(1, "weapons:" + weapons.Count);
    //    foreach (MechComponent component in weapons) {
    //      Log.WL(2, component.defId + ":" + component.mechComponentRef.MountedLocation + ":" + component.baseComponentRef.prefabName);
    //    }
    //    Log.WL(1, "supportComponents:" + supportComponents.Count);
    //    foreach (MechComponent component in supportComponents) {
    //      Log.WL(2, component.defId + ":" + component.mechComponentRef.MountedLocation + ":" + component.baseComponentRef.prefabName);
    //    }
    //    string prefabIdentifier = this.MechDef.Chassis.PrefabIdentifier;
    //    Log.WL(0, "Initing squad members reps:" + prefabIdentifier);
    //    squadReps = new Dictionary<ChassisLocations, TrooperRepresentation>();
    //    //squadReps.Add(ChassisLocations.Head, this._gameRep as MechRepresentation);
    //    for (int ti = 0; ti < info.SquadInfo.Troopers; ++ti) {
    //      if (ti >= locations.Count) { break; }
    //      GameObject squadGO = this.Combat.DataManager.PooledInstantiate(prefabIdentifier, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
    //      squadGO.name = prefabIdentifier + "_" + locations[ti].ToString();
    //      MechRepresentation squadRep = squadGO.GetComponent<MechRepresentation>();
    //      squadGO.GetComponent<Animator>().enabled = true;
    //      squadRep.Init(this, parentTransform, false);
    //      if ((UnityEngine.Object)parentTransform == (UnityEngine.Object)null) {
    //        squadGO.transform.position = this.currentPosition;
    //        squadGO.transform.rotation = this.currentRotation;
    //      }
    //      transformToScale = squadRep.thisTransform;
    //      j_Root = squadRep.gameObject.transform.FindRecursive("j_Root");
    //      if (j_Root != null) { transformToScale = j_Root; }
    //      transformToScale.localScale = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
    //      Vector3 pos = Vector3.zero;
    //      if (ti != 0) {
    //        pos.x += TrooperSquad.SquadRadius * Mathf.Cos(Mathf.Deg2Rad * positions[locations[ti]]);
    //        pos.z += TrooperSquad.SquadRadius * Mathf.Sin(Mathf.Deg2Rad * positions[locations[ti]]);
    //      }
    //      pos.y = 0;
    //      TrooperRepresentation trooperRep = new TrooperRepresentation(squadGO, pos, locations[ti]);
    //      this.Reps.Add(trooperRep);
    //      this.MechReps.Add(squadRep);
    //      squadReps.Add(locations[ti], trooperRep);
    //      pos += squadGO.transform.position;
    //      pos.y = this.Combat.MapMetaData.GetCellAt(pos).cachedHeight;
    //      //this.Combat.MapMetaData.GetLerpedHeightAt(pos);
    //      squadGO.transform.position = pos;
    //      squadGO.transform.SetParent(this.GameRep.gameObject.transform, true);
    //      jumpjetReps = Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
    //      foreach (JumpjetRepresentation jrep in jumpjetReps) {
    //        foreach (ParticleSystem psys in jrep.jumpjetParticles) {
    //          psys.RegisterRestoreScale();
    //          var main = psys.main;
    //          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
    //          Log.LogWrite(" " + psys.name + ":" + psys.main.scalingMode + "\n");
    //        }
    //      }
    //    }
    //    foreach (var trooper in squadReps) {
    //      List<string> usedPrefabNames = new List<string>();
    //      foreach (MechComponent allComponent in this.allComponents) {
    //        if (allComponent.mechComponentRef.MountedLocation != trooper.Key) { continue; }
    //        if (allComponent.componentType != ComponentType.Weapon) {
    //          allComponent.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, allComponent.baseComponentRef, this.MechDef.Chassis.PrefabBase, ChassisLocations.CenterTorso.ToString().ToLower(), ref usedPrefabNames);
    //          allComponent.baseComponentRef.hasPrefabName = true;
    //          if (!string.IsNullOrEmpty(allComponent.baseComponentRef.prefabName)) {
    //            Transform attachTransform = this.GetAttachTransform(trooper.Value.GameRep.GetComponent<MechRepresentation>(), ChassisLocations.CenterTorso);
    //            allComponent.InitGameRep(allComponent.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
    //            trooper.Value.GameRep.GetComponent<MechRepresentation>().miscComponentReps.Add(allComponent.componentRep);
    //          }
    //        }
    //      }
    //      foreach (Weapon weapon in this.Weapons) {
    //        if (weapon.mechComponentRef.MountedLocation != trooper.Key) { continue; }
    //        Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = ChassisLocations.CenterTorso;
    //        if (info.SquadInfo.Hardpoints.ContainsKey(weapon.WeaponCategoryValue.Name)) {
    //          Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = info.SquadInfo.Hardpoints[weapon.WeaponCategoryValue.Name];
    //        }
    //        weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, this.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
    //        weapon.baseComponentRef.hasPrefabName = true;
    //        if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
    //          Transform attachTransform = this.GetAttachTransform(trooper.Value.GameRep.GetComponent<MechRepresentation>(), weapon.mechComponentRef.MountedLocation);
    //          weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
    //          trooper.Value.GameRep.GetComponent<MechRepresentation>().weaponReps.Add(weapon.weaponRep);
    //          string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(this.MechDef, weapon.mechComponentRef);
    //          if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
    //            WeaponRepresentation component = this.Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<WeaponRepresentation>();
    //            component.Init((ICombatant)this, attachTransform, true, this.LogDisplayName, weapon.Location);
    //            trooper.Value.GameRep.GetComponent<MechRepresentation>().weaponReps.Add(component);
    //          }
    //        }
    //        Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = trooper.Key;
    //      }
    //      foreach (MechComponent supportComponent in this.supportComponents) {
    //        if (supportComponent.mechComponentRef.MountedLocation != trooper.Key) { continue; }
    //        Weapon weapon = supportComponent as Weapon;
    //        if (weapon != null) {
    //          Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = ChassisLocations.CenterTorso;
    //          if (info.SquadInfo.Hardpoints.ContainsKey(weapon.WeaponCategoryValue.Name)) {
    //            Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = info.SquadInfo.Hardpoints[weapon.WeaponCategoryValue.Name];
    //          }
    //          weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, this.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
    //          weapon.baseComponentRef.hasPrefabName = true;
    //          if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
    //            Transform attachTransform = this.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
    //            weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
    //            this.GameRep.miscComponentReps.Add((ComponentRepresentation)weapon.weaponRep);
    //          }
    //          Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = trooper.Key;
    //        }
    //      }
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.CenterTorso);
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.LeftTorso);
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.RightTorso);
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.LeftArm);
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.RightArm);
    //      this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.Head);
    //      bool flag1 = this.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
    //      bool flag2 = this.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
    //      if (flag1 | flag2) {
    //        SkinnedMeshRenderer[] componentsInChildren = trooper.Value.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
    //        for (int index = 0; index < componentsInChildren.Length; ++index) {
    //          if (flag1)
    //            componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderUnfinishedMaterial;
    //          if (flag2)
    //            componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderImpostorMaterial;
    //        }
    //      }
    //      trooper.Value.GameRep.GetComponent<MechRepresentation>().RefreshEdgeCache();
    //      trooper.Value.GameRep.GetComponent<MechRepresentation>().FadeIn(1f);
    //    }
    //    Log.WL(1, "result:");
    //    foreach (var trooper in squadReps) {
    //      Log.WL(1, trooper.Key.ToString()+":"+trooper.Value.GameRep.name);
    //    }
    //    //squadReps.Add(ChassisLocations.Head, this._gameRep as MechRepresentation);
    //  }
  }
  //public class TrooperRepresentationSimGame: MonoBehaviour {
  //  public MechRepresentationSimGame simGameRep;
  //  public ChassisLocations location;
  //  public SquadRepresentationSimGame parent;
  //  public void LoadDamageState(bool isDestroyed) {
  //    foreach (ChassisLocations loc in Enum.GetValues(typeof(ChassisLocations))) {
  //      switch (loc) {
  //        case ChassisLocations.None:
  //        case ChassisLocations.Torso:
  //        case ChassisLocations.Arms:
  //        case ChassisLocations.MainBody:
  //        case ChassisLocations.Legs:
  //        case ChassisLocations.All: continue;
  //        default: simGameRep.CollapseLocation((int)loc, isDestroyed); continue;
  //      }
  //    }
  //  }
  //  public void LoadWeapons() {
  //    Log.TWL(0, "TrooperRepresentationSimGame.LoadWeapons");
  //    List<string> usedPrefabNames = new List<string>();
  //    DataManager ___dataManager = Traverse.Create(simGameRep).Field<DataManager>("dataManager").Value;
  //    MechRepresentationSimGame __instance = this.simGameRep;
  //    VTOLBodyAnimation bodyAnimation = __instance.VTOLBodyAnim();
  //    MechTurretAnimation MechTurret = __instance.gameObject.GetComponentInChildren<MechTurretAnimation>(true);
  //    //QuadBodyAnimation quadBody = __instance.gameObject.GetComponentInChildren<QuadBodyAnimation>(true);
  //    Log.WL(1, "bodyAnimation:" + (bodyAnimation == null ? "null" : "not null"));
  //    for (int index = 0; index < __instance.mechDef.Inventory.Length; ++index) {
  //      MechComponentRef componentRef = __instance.mechDef.Inventory[index];
  //      if (location != componentRef.MountedLocation) { continue; }
  //      ChassisLocations Location = ChassisLocations.CenterTorso;//componentRef.MountedLocation;\
  //      WeaponDef weaponDef = componentRef.Def as WeaponDef;
  //      if (weaponDef != null) {
  //        if(this.parent.info.SquadInfo.Hardpoints.TryGetValue(weaponDef.WeaponCategoryValue.Name, out ChassisLocations loc)) {
  //          Location = loc;
  //        }
  //      }
  //      string MountedLocation = Location.ToString();
  //      bool correctLocation = false;
  //      if (__instance.mechDef.IsChassisFake()) {
  //        switch (Location) {
  //          case ChassisLocations.LeftArm: MountedLocation = VehicleChassisLocations.Front.ToString(); correctLocation = true; break;
  //          case ChassisLocations.RightArm: MountedLocation = VehicleChassisLocations.Rear.ToString(); correctLocation = true; break;
  //          case ChassisLocations.LeftLeg: MountedLocation = VehicleChassisLocations.Left.ToString(); correctLocation = true; break;
  //          case ChassisLocations.RightLeg: MountedLocation = VehicleChassisLocations.Right.ToString(); correctLocation = true; break;
  //          case ChassisLocations.Head: MountedLocation = VehicleChassisLocations.Turret.ToString(); correctLocation = true; break;
  //        }
  //      } else {
  //        correctLocation = true;
  //      }
  //      Log.WL(1, "Component " + componentRef.Def.GetType().ToString() + ":" + componentRef.GetType().ToString() + " id:" + componentRef.Def.Description.Id + " loc:" + MountedLocation);
  //      if (correctLocation) {
  //        Log.WL(2, "GetComponentPrefabName " + __instance.mechDef.Chassis.HardpointDataDef.ID + " base:" + __instance.mechDef.Chassis.PrefabBase + " loc:" + MountedLocation + " currPrefabName:" + componentRef.prefabName + " hasPrefab:" + componentRef.hasPrefabName + " hardpointSlot:" + componentRef.HardpointSlot);
  //        if (weaponDef != null) {
  //          string desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), componentRef.Def.PrefabIdentifier.ToLower(), weaponDef.WeaponCategoryValue.HardpointPrefabText);
  //          Log.WL(3, "desiredPrefabName:" + desiredPrefabName);
  //        } else {
  //          Log.WL(3, "");
  //        }
  //        //if (componentRef.hasPrefabName == false) {
  //        componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.mechDef.Chassis.HardpointDataDef, (BaseComponentRef)componentRef, __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), ref usedPrefabNames);
  //        componentRef.hasPrefabName = true;
  //        Log.WL(3, "effective prefab name:" + componentRef.prefabName);

  //        //}
  //      }
  //      if (!string.IsNullOrEmpty(componentRef.prefabName)) {
  //        HardpointAttachType attachType = HardpointAttachType.None;
  //        Log.WL(1, "component:" + componentRef.ComponentDefID + ":" + Location);
  //        Transform attachTransform = __instance.GetAttachTransform(Location);
  //        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(componentRef.prefabName);
  //        GameObject prefab = null;
  //        string prefabName = componentRef.prefabName;
  //        if (customHardpoint != null) {
  //          attachType = customHardpoint.attachType;
  //          prefab = ___dataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //          if (prefab == null) {
  //            prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //          } else {
  //            prefabName = customHardpoint.prefab;
  //          }
  //        } else {
  //          Log.WL(1, componentRef.prefabName + " have no custom hardpoint");
  //          prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //        }
  //        if (prefab != null) {
  //          ComponentRepresentation component1 = prefab.GetComponent<ComponentRepresentation>();
  //          if (component1 == null) {
  //            Log.WL(1, prefabName + " have no ComponentRepresentation");
  //            if (customHardpoint != null) {
  //              component1 = prefab.AddComponent<WeaponRepresentation>();
  //              Log.LogWrite(1, "reiniting vfxTransforms\n");
  //              List<Transform> transfroms = new List<Transform>();
  //              for (int i = 0; i < customHardpoint.emitters.Count; ++i) {
  //                Transform[] trs = component1.GetComponentsInChildren<Transform>();
  //                foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[i]) { transfroms.Add(tr); break; } }
  //              }
  //              Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
  //              for (int i = 0; i < transfroms.Count; ++i) {
  //                Log.LogWrite(2, transfroms[i].name + ":" + transfroms[i].localPosition + "\n");
  //              }
  //              if (transfroms.Count == 0) { transfroms.Add(prefab.transform); };
  //              component1.vfxTransforms = transfroms.ToArray();
  //              if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
  //                Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
  //                GameObject shaderPrefab = ___dataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //                if (shaderPrefab != null) {
  //                  Log.LogWrite(1, "shader prefab found\n");
  //                  Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
  //                  if (shaderComponent != null) {
  //                    Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
  //                    MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
  //                    foreach (MeshRenderer renderer in renderers) {
  //                      for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
  //                        if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
  //                          Log.LogWrite(2, "keep original shader: " + renderer.gameObject.transform.name + "\n");
  //                          continue;
  //                        }
  //                        Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
  //                        renderer.materials[mindex].shader = shaderComponent.material.shader;
  //                        renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
  //                      }
  //                    }
  //                  }
  //                  GameObject.Destroy(shaderPrefab);
  //                }
  //              }
  //            } else {
  //              component1 = prefab.AddComponent<ComponentRepresentation>();
  //            }
  //          }
  //          if (bodyAnimation != null) {
  //            Log.WL(1, "found VTOL body animation and vehicle component ref. Location:" + MountedLocation + " type:" + attachType);
  //            if (attachType == HardpointAttachType.None) {
  //              if ((bodyAnimation.bodyAttach != null) && (MountedLocation != VehicleChassisLocations.Turret.ToString())) { attachTransform = bodyAnimation.bodyAttach; }
  //            } else {
  //              AttachInfo attachInfo = bodyAnimation.GetAttachInfo(MountedLocation, attachType);
  //              Log.WL(2, "attachInfo:" + (attachInfo == null ? "null" : "not null"));
  //              if ((attachInfo != null) && (attachInfo.attach != null) && (attachInfo.main != null)) {
  //                Log.WL(2, "attachTransform:" + (attachInfo.attach == null ? "null" : attachInfo.attach.name));
  //                Log.WL(2, "mainTransform:" + (attachInfo.main == null ? "null" : attachInfo.main.name));
  //                attachTransform = attachInfo.attach;
  //                attachInfo.bayComponents.Add(component1);
  //              }
  //            }
  //          } else if (MechTurret != null) {
  //            Log.WL(1, "found mech turret:" + MountedLocation + " type:" + attachType);
  //            if (attachType == HardpointAttachType.Turret) {
  //              if(string.IsNullOrEmpty(customHardpoint.attachOverride) == false) {
  //                if (MechTurret.attachPointsNames.TryGetValue(customHardpoint.attachOverride, out AttachInfo attachPoint)) {
  //                  attachTransform = attachPoint.attach;
  //                }
  //              } else {
  //                if (MechTurret.attachPoints.TryGetValue(Location, out List<AttachInfo> attachPoints)) {
  //                  if (attachPoints.Count > 0) {
  //                    attachTransform = attachPoints[0].attach;
  //                  }
  //                }
  //              }
  //            }
  //          }
  //          if (component1 != null) {
  //            component1.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
  //            component1.gameObject.SetActive(true);
  //            component1.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
  //            component1.gameObject.name = componentRef.prefabName;
  //            __instance.componentReps.Add(component1);
  //            Log.WL(3, "Component representation spawned and inited. GameObject name:" + component1.gameObject.name + " Active:" + component1.gameObject.activeInHierarchy + " parent transform:" + component1.transform.parent.name);
  //          }
  //        }
  //        string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(__instance.mechDef, componentRef);
  //        if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
  //          ComponentRepresentation component2 = ___dataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<ComponentRepresentation>();
  //          component2.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
  //          component2.gameObject.SetActive(true);
  //          component2.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
  //          component2.gameObject.name = mountingPointPrefabName;
  //          __instance.componentReps.Add(component2);
  //        }
  //      }
  //    }
  //    if (bodyAnimation != null) { bodyAnimation.ResolveAttachPoints(); };
  //    if (quadBody != null) {
  //      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
  //      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
  //      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
  //      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
  //    }
  //    __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
  //    __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
  //  }
  //  public TrooperRepresentationSimGame Init(ChassisLocations loc, SquadRepresentationSimGame parent) {
  //    this.location = loc;
  //    this.parent = parent;
  //    simGameRep = this.GetComponent<MechRepresentationSimGame>();
  //    return this;
  //  }
  //}
  //public class SquadRepresentationSimGame: MonoBehaviour {
  //  public Dictionary<ChassisLocations, TrooperRepresentationSimGame> squadSimGameReps { get; private set; }
  //  public UnitCustomInfo info { get; private set; }
  //  public MechDef mechDef { get; private set; }
  //  public void LoadDamageState() {
  //    foreach(var simRep in squadSimGameReps) {
  //      LocationDef locDef = mechDef.GetChassisLocationDef(simRep.Key);
  //      bool isDestroyed = locDef.InternalStructure <= 0f;
  //      //if (simRep.Key == ChassisLocations.CenterTorso) { isDestroyed = true; };
  //      simRep.Value.LoadDamageState(isDestroyed);
  //    }
  //  }
  //  public SquadRepresentationSimGame() {
  //    squadSimGameReps = new Dictionary<ChassisLocations, TrooperRepresentationSimGame>();
  //  }
  //  public void Init(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
  //    this.info = mechDef.GetCustomInfo();
  //    this.mechDef = mechDef;
  //    foreach(var simRep in squadSimGameReps) {
  //      simRep.Value.simGameRep.Init(dataManager, mechDef, this.transform, heraldryDef);
  //      Transform transformToScale = simRep.Value.simGameRep.rootTransform;
  //      transformToScale.localScale = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
  //      Vector3 pos = Vector3.zero;
  //      if (simRep.Key != ChassisLocations.Head) {
  //        pos.x += TrooperSquad.SquadRadius * Mathf.Cos(Mathf.Deg2Rad * TrooperSquad.positions[simRep.Key]);
  //        pos.z += TrooperSquad.SquadRadius * Mathf.Sin(Mathf.Deg2Rad * TrooperSquad.positions[simRep.Key]);
  //      }
  //      pos.y = 0;
  //      transformToScale.localPosition = pos;
  //    }
  //  }
  //  public void Instantine(MechDef mechDef, List<GameObject> squadTroopers) {
  //    MechRepresentationSimGame mechRep = this.gameObject.GetComponent<MechRepresentationSimGame>();
  //    SkinnedMeshRenderer[] meshes = this.GetComponentsInChildren<SkinnedMeshRenderer>();
  //    GameObject go = new GameObject("Empty");
  //    Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
  //    go.AddComponent<MeshRenderer>();
  //    if (meshes != null) {
  //      foreach (SkinnedMeshRenderer mesh in meshes) {
  //        mesh.sharedMesh = emptyMesh;
  //      }
  //    }
  //    GameObject.Destroy(go);
  //    UnitCustomInfo info = mechDef.GetCustomInfo();
  //    for (int index = 0; index < squadTroopers.Count; ++index) {
  //      ChassisLocations location = TrooperSquad.locations[index];
  //      MechRepresentationSimGame squadRep = squadTroopers[index].GetComponent<MechRepresentationSimGame>();
  //      squadSimGameReps.Add(location, squadRep.gameObject.AddComponent<TrooperRepresentationSimGame>().Init(location,this));
  //    }
  //  }
  //}
}