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
using CustAmmoCategories;
using HarmonyLib;
using HBS.Math;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorTwistSequence))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(bool), typeof(bool), typeof(float), typeof(int), typeof(int), typeof(GameRepresentation.RotationCompleteDelegate) })]
  public static class ActorTwistSequence_Constructor {
    public static void Prefix(ref Animator ___actorAnim, ref PilotableActorRepresentation ___actorRep, ref float ___desiredAngle, ActorTwistSequence __instance, AbstractActor actor, Vector3 lookAt, bool isLookVector, bool isBodyRotation, float twistDuration, int stackItemUID, int sequenceId, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      Log.TWL(0, "ActorTwistSequence.Constructor isBodyRotation:" + isBodyRotation);
    }
    public static void Postfix(ref Animator ___actorAnim,ref PilotableActorRepresentation ___actorRep,ref float ___desiredAngle, ActorTwistSequence __instance, AbstractActor actor, Vector3 lookAt, bool isLookVector, bool isBodyRotation, float twistDuration, int stackItemUID, int sequenceId, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      //CustomTwistAnimation custAnimator = ___actorRep.GetComponent<CustomTwistAnimation>();
      //if (custAnimator == null) { return; }
      //if (custAnimator.mechTurret == null) { return; }
      //if (custAnimator.mechTurret.turnAnimator == null) { return; }
      //___actorAnim = custAnimator.mechTurret.turnAnimator;
      //Log.TWL(0, "ActorTwistSequence.Constructor angle:"+ ___desiredAngle);
    }
  }
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("ToggleRandomIdles")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(bool) })]
  //public static class MechRepresentation_ToggleRandomIdles {
  //  public static bool Prefix(MechRepresentation __instance,ref bool shouldIdle, ref bool ___allowRandomIdles) {
  //    CustomTwistAnimation custIdleAnim = __instance.gameObject.GetComponent<CustomTwistAnimation>();
  //    if (custIdleAnim == null) { return true; }
  //    custIdleAnim.allowRandomIdles = shouldIdle;
  //    custIdleAnim.PlayIdleAnimation();
  //    ___allowRandomIdles = false;
  //    shouldIdle = false;
  //    return true;
  //  }
  //}
  public class CustomTwistAnimatorInfo {
    public Animator animator { get; protected set; }
    public bool HasStartRandomIdle { get; protected set; }
    public int HashStartRandomIdle { get; protected set; }
    public bool HasTwistR { get; protected set; }
    public int HashTwistR { get; protected set; }
    public bool HasTwistL { get; protected set; }
    public int HashTwistL { get; protected set; }
    public bool HasIdleTwistR { get; protected set; }
    public int HashIdleTwistR { get; protected set; }
    public bool HasIdleTwistL { get; protected set; }
    public int HashIdleTwistL { get; protected set; }
    public bool StartRandomIdle {
      set {
        Log.TWL(0, "CustomTwistAnimatorInfo.StartRandomIdle " + (this.animator == null ? "null" : this.animator.gameObject.name) + " HasStartRandomIdle:" + this.HasStartRandomIdle + " value:" + value);
        if (this.animator == null) { return; }
        if (this.HasStartRandomIdle == false) { return; }
        this.animator.SetBool(HashStartRandomIdle, value);
      }
    }
    public float TwistR {
      set {
        Log.TWL(0, "CustomTwistAnimatorInfo.TwistR "+(this.animator == null?"null":this.animator.gameObject.name)+ " HasTwistR:"+this.HasTwistR + " value:" + value);
        if (this.animator == null) { return; }
        if (this.HasTwistR == false) { return; }
        this.animator.SetFloat(HashTwistR, value);
      }
    }
    public float TwistL {
      set {
        Log.TWL(0, "CustomTwistAnimatorInfo.TwistL " + (this.animator == null ? "null" : this.animator.gameObject.name) + " HasTwistL:" + this.HasTwistL+" value:"+value);
        if (this.animator == null) { return; }
        if (this.HasTwistL == false) { return; }
        this.animator.SetFloat(HashTwistL, value);
      }
    }
    public float IdleTwistR {
      set {
        if (this.animator == null) { return; }
        if (this.HasTwistR == false) { return; }
        this.animator.SetFloat(HashIdleTwistR, value);
      }
    }
    public float IdleTwistL {
      set {
        if (this.animator == null) { return; }
        if (this.HasTwistL == false) { return; }
        this.animator.SetFloat(HashIdleTwistL, value);
      }
    }
    public CustomTwistAnimatorInfo(Animator anim) {
      this.animator = anim;
      HashStartRandomIdle = Animator.StringToHash("StartRandomIdle");
      HashTwistR = Animator.StringToHash("TwistR");
      HashTwistL = Animator.StringToHash("TwistL");
      HashIdleTwistR = Animator.StringToHash("IdleTwistR");
      HashIdleTwistL = Animator.StringToHash("IdleTwistL");
      if (this.animator == null) { return; }
      AnimatorControllerParameter[] parameters = this.animator.parameters;
      foreach(AnimatorControllerParameter param in parameters) {
        if (param.nameHash == HashStartRandomIdle) { this.HasStartRandomIdle = true; }
        if (param.nameHash == HashTwistR) { this.HasTwistR = true; }
        if (param.nameHash == HashTwistL) { this.HasTwistL = true; }
        if (param.nameHash == HashIdleTwistR) { this.HasIdleTwistR = true; }
        if (param.nameHash == HashIdleTwistL) { this.HasIdleTwistL = true; }
      }
    }
  }
  public class CustomTwistAnimation: MonoBehaviour {
    public bool HasTurret { get; protected set; }
    public MechRepresentation ownerRep;
    public HashSet<CustomTwistAnimatorInfo> animators;
    public Statistic noRandomIdles;
    public bool allowRandomIdles { get; set; }
    public bool inRandomAnimation;
    public float t;
    public static readonly float minTimeBetweenRandoms = 10f;
    public bool StartRandomIdle { set { foreach (CustomTwistAnimatorInfo info in animators) { info.StartRandomIdle = value; } } }
    public float TwistR {
      set { foreach (CustomTwistAnimatorInfo info in animators) { info.TwistR = value; } }
    }
    public float TwistL {
      set { foreach (CustomTwistAnimatorInfo info in animators) { info.TwistL = value; } }
    }
    public float IdleTwistR {
      set { foreach (CustomTwistAnimatorInfo info in animators) { info.IdleTwistR = value; } }
    }
    public float IdleTwistL {
      set { foreach (CustomTwistAnimatorInfo info in animators) { info.IdleTwistL = value; } }
    }
    public void ReturnToIdleNeturalFacing() {
      inRandomAnimation = false;
      Log.TWL(0, "CustomRandomIdleAnimation.ReturnToIdleNeturalFacing");
      this.StartRandomIdle = false;
    }
    public void twist(float value) {
      inRandomAnimation = false;
      this.StartRandomIdle = false;
      value *= (90f / 180f);
      Log.TWL(0, "CustomTwistAnimation.twist " + value);
      if (value >= 0f) { this.TwistL=0f; this.TwistR = value; } else
      if (value <= 0f) { this.TwistL = Mathf.Abs(value); this.TwistR= 0f; }
    }
    public void SetIdleRotation(float rnd) {
      inRandomAnimation = true;
      this.StartRandomIdle = false;
      Log.TWL(0, "CustomRandomIdleAnimation.SetIdleRotation " + ownerRep.name + " rnd:" + rnd);
      if (rnd < 0f) {
        this.IdleTwistR = 0f;
        this.IdleTwistL = Mathf.Abs(rnd);
      } else {
        this.IdleTwistL = 0f;
        this.IdleTwistR = rnd;
      }
      this.StartRandomIdle = true;
    }
    public void PlayIdleAnimation() {
      t = 0f;
      if (inRandomAnimation == true) {
        ReturnToIdleNeturalFacing();
        return;
      }
      if (allowRandomIdles) {
        SetIdleRotation(UnityEngine.Random.Range(-1f, 1f));
      }
    }
    public void noRandomIdlesChanged(Statistic value) {
      allowRandomIdles = !value.Value<bool>();
      Log.TWL(0, "CustomTwistAnimation.noRandomIdlesChanged allowRandomIdles:"+ allowRandomIdles);
      if(allowRandomIdles == false) {
        this.ReturnToIdleNeturalFacing();
      }
    }
    public void LateUpdate() {
      if (allowRandomIdles && (ownerRep.parentActor.IsDead == false)) {
        t += Time.deltaTime;
        if (t > minTimeBetweenRandoms) { this.PlayIdleAnimation(); }
      }
    }
    public void Init() {
      this.HasTurret = false;
      this.animators = new HashSet<CustomTwistAnimatorInfo>();
      ownerRep = this.gameObject.GetComponent<MechRepresentation>();
      MechTurretAnimation mechTurret = this.gameObject.GetComponentInChildren<MechTurretAnimation>(true);
      if (mechTurret != null) { if (mechTurret.turnAnimator != null) { HasTurret = true; this.animators.Add(new CustomTwistAnimatorInfo(mechTurret.turnAnimator)); } }
      noRandomIdles = ownerRep.parentMech.StatCollection.GetStatistic(UnitUnaffectionsActorStats.NoRandomIdlesActorStat);
      UnitCustomInfo info = ownerRep.parentMech.MechDef.Chassis.GetCustomInfo();
      if (noRandomIdles == null) {
        noRandomIdles = ownerRep.parentMech.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoRandomIdlesActorStat, info == null ? false: info.NoIdleAnimations);
      }
      noRandomIdles.AddValueChangeListener(UnitUnaffectionsActorStats.NoRandomIdlesActorStat,new Action<Statistic>(this.noRandomIdlesChanged));
      allowRandomIdles = !noRandomIdles.Value<bool>();
      inRandomAnimation = false;
      Log.TWL(0, "CustomTwistAnimation.Init allowRandomIdles:" + allowRandomIdles);
      CustomRepresentation customRepresentation = this.gameObject.GetComponent<CustomRepresentation>();
      if (customRepresentation != null) {
        Log.WL(1, "CustomRepresentation exists");
        if (customRepresentation.CustomDefinition != null) {
          foreach (string twistAnimatorName in customRepresentation.CustomDefinition.TwistAnimators) {
            Transform twistAnimatorTR = this.gameObject.transform.FindRecursive(twistAnimatorName);
            if (twistAnimatorTR == null) { continue; }
            Animator twistAnimator = twistAnimatorTR.gameObject.GetComponent<Animator>();
            if (twistAnimator == null) { continue; }
            Log.WL(2, "custom twist animator:"+ twistAnimatorTR.name+" animator:"+ twistAnimator.name);
            this.animators.Add(new CustomTwistAnimatorInfo(twistAnimator));
          }
        }
      }
    }
  }
  public class MechTurretMountData {
    public string Name { get; set; }
    public ChassisLocations Location { get; set; }
    public string AttachTo { get; set; }
    public string Animator { get; set; }
  }
  public class MechTurretAnimData {
    public string TurnAnimator { get; set; }
    public List<MechTurretMountData> AttachPoints { get; set; }
    public MechTurretAnimData() {
      AttachPoints = new List<MechTurretMountData>();
    }
  }
  /*public class MechTurretAttachPoint {
    private static HashSet<Animator> AnimatorsInPosition = new HashSet<Animator>();
    public string Name { get; set; }
    //public MechTurretAnimation parent { get; set; }
    public Animator animator { get; set; }
    public ChassisLocations Location { get; set; }
    public Transform attachTransform { get; set; }
    public bool noRecoil { get; private set; }
    private float recoilValue;
    private float recoilDelta;
    public void Update(float t) {
      if (recoilDelta == 0f) { return; };
      recoilValue += recoilDelta * t;
      if (recoilValue < 0f) {
        recoilValue = 0f; recoilDelta = 0f;
        Log.TWL(0, "MechTurretAttach.Recoiled " + parent.name + " up");
      };
      if (recoilValue > 1f) {
        recoilValue = 1f; recoilDelta = -1f;
        Log.TWL(0, "MechTurretAttach.Recoiled " + parent.name + " down");
      };
      animator.SetFloat("recoil", recoilValue);
    }
    public void Recoil() {
      Log.TWL(0, "MechTurretAttach.Recoil:" + parent.name + " no recoil:" + noRecoil);
      if (noRecoil == false) { recoilDelta = 10f; }
    }
    public void Prefire(Vector3 target, bool indirect) {
      Log.WL(1, "MechTurretAttach.Prefire animator:" + (animator == null ? "null" : animator.name));
      if (animator == null) { return; }
      bool inPos = AnimatorsInPosition.Contains(animator);
      if (inPos) { return; }
      if (indirect) {
        animator.SetFloat("to_fire_normal", 0.98f);
        animator.SetFloat("indirect", 1f);
        AnimatorsInPosition.Add(animator);
      } else {
        animator.SetFloat("indirect", 0.98f);
        animator.SetFloat("to_fire_normal", 1f);
        if (this.attachTransform != null) {
          Vector3 desiredLookDirection = target - attachTransform.position;
          float angle = NvMath.AngleSigned(attachTransform.forward, desiredLookDirection.normalized, Vector3.right);
          Log.WL(2, "angle:" + angle);
          angle /= 90f;
          Log.WL(2, "vertical anim:" + angle);
          if (angle < 0f) { angle = 0f; }
          //animator.SetFloat("vertical", angle);
        } else {
          //animator.SetFloat("vertical", 0.5f);
        }
        AnimatorsInPosition.Add(animator);
      }
    }
    public void Postfire() {
      Log.WL(1, "AttachInfo.Postfire animator:" + (animator == null ? "null" : "not null"));
      if (animator == null) { return; }
      animator.SetFloat("to_fire_normal", 0.98f);
      animator.SetFloat("indirect", 0.98f);
      AnimatorsInPosition.Remove(animator);
    }
    public MechTurretAttachPoint(MechTurretAnimation parent, MechTurretMountData data) {
      this.Location = data.Location;
      //this.parent = parent;
      this.Name = data.Name;
      if (string.IsNullOrEmpty(this.Name)) { this.Name = data.Location.ToString(); }
      Transform anim = parent.gameObject.transform.FindRecursive(data.Animator);
      if (anim != null) { this.animator = anim.gameObject.GetComponent<Animator>(); }
      Transform attach = parent.gameObject.transform.FindRecursive(data.AttachTo);
      if (attach != null) { this.attachTransform = attach; } else { this.attachTransform = parent.gameObject.transform; }
      recoilValue = 0f;
      recoilDelta = 0f;
    }
  }*/
  public class WeaponAttachRepresentation: MonoBehaviour {
    public WeaponRepresentation weaponRep { get; set; }
    public ComponentRepresentation compRep { get; set; }
    public AttachInfo attachPoint { get; set; }
    public void Init(WeaponRepresentation wRrp, AttachInfo attachPoint) {
      this.weaponRep = wRrp;
      this.attachPoint = attachPoint;
      this.attachPoint.weapons.Add(wRrp.mechComponent);
      this.attachPoint.AddHardpointAnimators(wRrp);
    }
    public void Init(ComponentRepresentation cRrp, AttachInfo attachPoint) {
      this.weaponRep = null;
      this.attachPoint = attachPoint;
      this.attachPoint.weapons.Add(cRrp.mechComponent);
      this.attachPoint.AddHardpointAnimators(cRrp);
    }
    public void Update() {
      if (attachPoint == null) { return; }
      if (weaponRep == null) { return; }
      attachPoint.Update(Time.deltaTime);
    }
  }
  public class MechTurretAnimation: GenericAnimatedComponent {
    public Animator turnAnimator { get; set; }
    public Dictionary<ChassisLocations, List<AttachInfo>> attachPoints;
    public Dictionary<string, AttachInfo> attachPointsNames;
    public override bool StayOnDeath() { return true; }
    public override bool StayOnLocationDestruction() { return true; }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      MechTurretAnimData turretData = JsonConvert.DeserializeObject<MechTurretAnimData>(data);
      Transform anim = this.gameObject.transform.FindRecursive(turretData.TurnAnimator);
      if (anim != null) { turnAnimator = anim.gameObject.GetComponent<Animator>(); };
      attachPoints = new Dictionary<ChassisLocations, List<AttachInfo>>();
      attachPointsNames = new Dictionary<string, AttachInfo>();
      foreach (MechTurretMountData apData in turretData.AttachPoints) {
        if(attachPoints.TryGetValue(apData.Location, out List<AttachInfo> aPoints) == false) {
          aPoints = new List<AttachInfo>();
          attachPoints.Add(apData.Location, aPoints);
        }
        AttachInfo mtPoint = new AttachInfo(this, apData);
        aPoints.Add(mtPoint);
        attachPointsNames.Add(mtPoint.Name, mtPoint);
      }
      Mech mech = a as Mech;
    }
  }
  //public static class Mech_InitGameRepTurret {
  //  public static void Postfix(Mech __instance, Transform parentTransform){
  //    Log.TWL(0, "Mech.InitGameRep "+__instance.DisplayName);
  //    MechTurretAnimation MechTurret = __instance.GameRep.GetComponentInChildren<MechTurretAnimation>(true);
  //    if (MechTurret != null) {
  //      Log.WL(1, "MechTurretAnimation found " + MechTurret.name);
  //      foreach (WeaponRepresentation weaponRep in __instance.GameRep.weaponReps) {
  //        if (weaponRep == null) { continue; }
  //        Log.WL(2, "prefab " + weaponRep.weapon.mechComponentRef.prefabName);
  //        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(weaponRep.weapon.mechComponentRef.prefabName);
  //        if (customHardpoint == null) { Log.WL(3, "no custom hardpoint"); continue; }
  //        Log.WL(3, "attachType:" + customHardpoint.attachType + " attachOverride:" + customHardpoint.attachOverride);
  //        if (customHardpoint.attachType != HardpointAttachType.Turret) { continue; }
  //        AttachInfo attachPoint = null;
  //        if (string.IsNullOrEmpty(customHardpoint.attachOverride) == false) {
  //          if (MechTurret.attachPointsNames.ContainsKey(customHardpoint.attachOverride)) {
  //            attachPoint = MechTurret.attachPointsNames[customHardpoint.attachOverride];
  //          };
  //        }
  //        if (attachPoint == null) {
  //          if (MechTurret.attachPoints.ContainsKey(weaponRep.weapon.mechComponentRef.MountedLocation)) {
  //            if (MechTurret.attachPoints[weaponRep.weapon.mechComponentRef.MountedLocation].Count > 0) {
  //              attachPoint = MechTurret.attachPoints[weaponRep.weapon.mechComponentRef.MountedLocation][0];
  //            }
  //          }
  //        }
  //        Log.WL(3, "attachPoint:" + (attachPoint == null ? "null" : attachPoint.attach.name));
  //        if (attachPoint != null) {
  //          if (weaponRep.parentTransform != null) { weaponRep.parentTransform = attachPoint.attach; }
  //          if (weaponRep.thisTransform != null) {
  //            weaponRep.thisTransform.SetParent(attachPoint.attach, false);
  //            weaponRep.thisTransform.localPosition = Vector3.zero;
  //            weaponRep.thisTransform.localScale = Vector3.one;
  //            weaponRep.thisTransform.localRotation = Quaternion.identity;
  //            WeaponAttachRepresentation attachRep = weaponRep.gameObject.GetComponent<WeaponAttachRepresentation>();
  //            if (attachRep == null) { attachRep = weaponRep.gameObject.AddComponent<WeaponAttachRepresentation>(); }
  //            attachRep.Init(weaponRep, attachPoint);
  //          };
  //        }
  //      }
  //    }
  //    CustomTwistAnimation idleHelper = __instance.GameRep.gameObject.GetComponent<CustomTwistAnimation>();
  //    if (idleHelper == null) { idleHelper = __instance.GameRep.gameObject.AddComponent<CustomTwistAnimation>(); };
  //    idleHelper.Init();
  //    CustomRepresentation custRep = __instance.GameRep.gameObject.GetComponent<CustomRepresentation>();
  //    if (custRep != null) { custRep.AttachWeapons(); custRep.AttachHeadlights(); }
  //  }
  //}
}