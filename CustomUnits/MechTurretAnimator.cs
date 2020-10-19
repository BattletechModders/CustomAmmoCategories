using BattleTech;
using CustAmmoCategories;
using Harmony;
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
    public static void Postfix(ref Animator ___actorAnim,ref PilotableActorRepresentation ___actorRep,ref float ___desiredAngle, ActorTwistSequence __instance, AbstractActor actor, Vector3 lookAt, bool isLookVector, bool isBodyRotation, float twistDuration, int stackItemUID, int sequenceId, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      CustomTwistAnimation custAnimator = ___actorRep.GetComponent<CustomTwistAnimation>();
      if (custAnimator == null) { return; }
      if (custAnimator.mechTurret == null) { return; }
      if (custAnimator.mechTurret.turnAnimator == null) { return; }
      ___actorAnim = custAnimator.mechTurret.turnAnimator;
      Log.TWL(0, "ActorTwistSequence.Constructor angle:"+ ___desiredAngle);
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ToggleRandomIdles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_ToggleRandomIdles {
    public static bool Prefix(MechRepresentation __instance,ref bool shouldIdle, ref bool ___allowRandomIdles) {
      CustomTwistAnimation custIdleAnim = __instance.gameObject.GetComponent<CustomTwistAnimation>();
      if (custIdleAnim == null) { return true; }
      custIdleAnim.allowRandomIdles = shouldIdle;
      custIdleAnim.PlayIdleAnimation();
      ___allowRandomIdles = false;
      shouldIdle = false;
      return true;
    }
  }
  public class CustomTwistAnimation: MonoBehaviour {
    public MechRepresentation ownerRep;
    public MechTurretAnimation mechTurret;
    public Statistic noRandomIdles;
    public bool allowRandomIdles { get; set; }
    public bool inRandomAnimation;
    public float t;
    public static readonly float minTimeBetweenRandoms = 10f;
    public void ReturnToIdleNeturalFacing() {
      inRandomAnimation = false;
      Log.TWL(0, "CustomRandomIdleAnimation.ReturnToIdleNeturalFacing");
      if (mechTurret != null) {
        if (mechTurret.turnAnimator != null) {
          mechTurret.turnAnimator.SetBool("StartRandomIdle", false);
          //mechTurret.turnAnimator.SetFloat("IdleTwistR", 0f);
          //mechTurret.turnAnimator.SetFloat("TwistL", 0f);
        }
      }
    }
    public void twist(float value) {
      inRandomAnimation = false;
      if (mechTurret != null) {
        if (mechTurret.turnAnimator != null) {
          mechTurret.turnAnimator.SetBool("StartRandomIdle", false);
          value *= (90f / 180f);
          Log.TWL(0, "CustomTwistAnimation.twist " + value);
          if (value >= 0f) { mechTurret.turnAnimator.SetFloat("TwistL", 0f); mechTurret.turnAnimator.SetFloat("TwistR", value); } else
          if (value <= 0f) { mechTurret.turnAnimator.SetFloat("TwistL", Mathf.Abs(value)); mechTurret.turnAnimator.SetFloat("TwistR", 0f); }
        }
      }
    }
    public void SetIdleRotation(float rnd) {
      inRandomAnimation = true;
      if (mechTurret != null) {
        if (mechTurret.turnAnimator != null) {
          mechTurret.turnAnimator.SetBool("StartRandomIdle", false);
          Log.TWL(0, "CustomRandomIdleAnimation.SetIdleRotation " + ownerRep.name + " rnd:" + rnd);
          if (rnd < 0f) {
            mechTurret.turnAnimator.SetFloat("IdleTwistR", 0f);
            mechTurret.turnAnimator.SetFloat("IdleTwistL", Mathf.Abs(rnd));
          } else {
            mechTurret.turnAnimator.SetFloat("IdleTwistL", 0f);
            mechTurret.turnAnimator.SetFloat("IdleTwistR", rnd);
          }
          mechTurret.turnAnimator.SetBool("StartRandomIdle", true);
        }
      }
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
      if(allowRandomIdles == false) {
        this.ReturnToIdleNeturalFacing();
      }
    }
    public void Update() {
      if (allowRandomIdles) {
        t += Time.deltaTime;
        if (t > minTimeBetweenRandoms) { this.PlayIdleAnimation(); }
      }
    }
    public void Init() {
      ownerRep = this.gameObject.GetComponent<MechRepresentation>();
      mechTurret = this.gameObject.GetComponentInChildren<MechTurretAnimation>(true);
      noRandomIdles = ownerRep.parentMech.StatCollection.GetStatistic(UnitUnaffectionsActorStats.NoRandomIdlesActorStat);
      if (noRandomIdles == null) { noRandomIdles = ownerRep.parentMech.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoRandomIdlesActorStat,ownerRep.parentMech.MechDef.Chassis.GetCustomInfo().NoIdleAnimations); }
      noRandomIdles.AddValueChangeListener(UnitUnaffectionsActorStats.NoRandomIdlesActorStat,new Action<Statistic>(this.noRandomIdlesChanged));
      allowRandomIdles = !noRandomIdles.Value<bool>();
      inRandomAnimation = false;
      Log.TWL(0, "CustomRandomIdleAnimation.Init allowRandomIdles:"+ allowRandomIdles);
    }
  }
  public class MechTurretMountData {
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
  public class MechTurretAttachPoint {
    private static HashSet<Animator> AnimatorsInPosition = new HashSet<Animator>();
    public MechTurretAnimation parent { get; set; }
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
      this.parent = parent;
      Transform anim = parent.gameObject.transform.FindRecursive(data.Animator);
      if (anim != null) { this.animator = anim.gameObject.GetComponent<Animator>(); }
      Transform attach = parent.gameObject.transform.FindRecursive(data.AttachTo);
      if (attach != null) { this.attachTransform = attach; } else { this.attachTransform = parent.gameObject.transform; }
      recoilValue = 0f;
      recoilDelta = 0f;
    }
  }
  public class WeaponAttachRepresentation: MonoBehaviour {
    public WeaponRepresentation weaponRep { get; set; }
    public MechTurretAttachPoint attachPoint { get; set; }
    public void Init(WeaponRepresentation wRrp, MechTurretAttachPoint attachPoint) {
      this.weaponRep = wRrp;
      this.attachPoint = attachPoint;
    }
    public void Update() {
      if (attachPoint == null) { return; }
      if (weaponRep == null) { return; }
      attachPoint.Update(Time.deltaTime);
    }
  }
  public class MechTurretAnimation: GenericAnimatedComponent {
    public Animator turnAnimator { get; set; }
    public Dictionary<ChassisLocations, MechTurretAttachPoint> attachPoints;
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      MechTurretAnimData turretData = JsonConvert.DeserializeObject<MechTurretAnimData>(data);
      Transform anim = this.gameObject.transform.FindRecursive(turretData.TurnAnimator);
      if (anim != null) { turnAnimator = anim.gameObject.GetComponent<Animator>(); };
      attachPoints = new Dictionary<ChassisLocations, MechTurretAttachPoint>();
      foreach (MechTurretMountData apData in turretData.AttachPoints) {
        attachPoints.Add(apData.Location, new MechTurretAttachPoint(this, apData));
      }
      Mech mech = a as Mech;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("InitGameRep")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Transform) })]
  public static class Mech_InitGameRepTurret {
    public static void Postfix(Mech __instance, Transform parentTransform){
      Log.TWL(0, "Mech.InitGameRep "+__instance.DisplayName);
      MechTurretAnimation MechTurret = __instance.GameRep.GetComponentInChildren<MechTurretAnimation>(true);
      if (MechTurret == null) {
        Log.WL(1, "MechTurretAnimation not found");
        return;
      }
      Log.WL(1, "MechTurretAnimation found "+MechTurret.name);
      foreach (WeaponRepresentation weaponRep in __instance.GameRep.weaponReps) {
        if (weaponRep == null) { continue; }
        if(MechTurret.attachPoints.TryGetValue(weaponRep.weapon.mechComponentRef.MountedLocation, out MechTurretAttachPoint attachPoint)) {
          if (weaponRep.parentTransform != null) { weaponRep.parentTransform = attachPoint.attachTransform; }
          if (weaponRep.thisTransform != null) {
            weaponRep.thisTransform.SetParent(attachPoint.attachTransform,false);
            weaponRep.thisTransform.localPosition = Vector3.zero;
            weaponRep.thisTransform.localScale = Vector3.one;
            weaponRep.thisTransform.localRotation = Quaternion.identity;
            WeaponAttachRepresentation attachRep = weaponRep.gameObject.GetComponent<WeaponAttachRepresentation>();
            if (attachRep == null) { attachRep = weaponRep.gameObject.AddComponent<WeaponAttachRepresentation>(); }
            attachRep.Init(weaponRep, attachPoint);
          };
        }
      }
      CustomTwistAnimation idleHelper = __instance.GameRep.gameObject.GetComponent<CustomTwistAnimation>();
      if (idleHelper == null) { idleHelper = __instance.GameRep.gameObject.AddComponent<CustomTwistAnimation>(); };
      idleHelper.Init();
    }
  }
}