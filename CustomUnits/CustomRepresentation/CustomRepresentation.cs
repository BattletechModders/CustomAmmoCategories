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
using BattleTech.Rendering.MechCustomization;
using BattleTech.Rendering.UI;
using CustAmmoCategories;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  public class MechSimGameComponentRepresentation : MonoBehaviour {
    public string HardpointDefId { get; set; }
    public string OriginalPrefabId { get; set; }
  }
  public class CustomMouseInteactions: MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
    public PilotableActorRepresentation parentRep { get; set; }
    public void OnPointerClick(PointerEventData eventData) {
      if (parentRep == null) { return; }
      parentRep.OnPointerClick(eventData);
    }
    public void OnPointerEnter(PointerEventData eventData) {
      if (parentRep == null) { return; }
      parentRep.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
      if (parentRep == null) { return; }
      parentRep.OnPointerExit(eventData);
    }
  }
  public class CustomRepresentationAnimatorInfo {
    public bool HasInBattle { get; protected set; }
    public int HashInBattle { get; protected set; }
    public bool HasCharge { get; protected set; }
    public int HashCharge { get; protected set; }
    public bool HasForward { get; protected set; }
    public int HashForward { get; protected set; }
    public bool HasDropOff { get; protected set; }
    public int HashDropOff { get; protected set; }
    public bool HasIsMoving { get; protected set; }
    public int HashIsMoving { get; protected set; }
    public bool HasBeginMove { get; protected set; }
    public int HashBeginMove { get; protected set; }
    public bool HasTurnParam { get; protected set; }
    public int HashTurnParam { get; protected set; }
    public bool HasDeathAnim { get; protected set; }
    public int HashDeathAnim { get; protected set; }
    public Animator animator { get; protected set; }
    public bool InBattle {
      set {
        if (animator == null) { return; }
        if (HasInBattle == false) { return; }
        animator.SetBool(HashInBattle, value);
        CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.InBattle "+animator.name+" "+value);
      }
    }
    public bool DropOff {
      set {
        if (animator == null) { return; }
        if (HasDropOff == false) { return; }
        animator.SetBool(HashDropOff, value);
        CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.HashDropOff " + animator.name + " " + value);
      }
    }
    public void Charge() {
      if (animator == null) { return; }
      if (HasCharge == false) { return; }
      animator.SetTrigger(HashCharge);
      CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.Charge " + animator.name);
    }
    public float Forward {
      set {
        if (animator == null) { return; }
        if (HasForward == false) { return; }
        animator.SetFloat(HashForward, value);
        CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.Forward " + animator.name + " " + value);
      }
    }
    public bool IsMoving {
      set {
        if (animator == null) { return; }
        if (HasIsMoving == false) { return; }
        animator.SetBool(HashIsMoving, value);
        CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.IsMoving " + animator.name + " " + value);
      }
    }
    public void BeginMove(bool value) {
      if (animator == null) { return; }
      if (HasBeginMove == false) { return; }
      if (value) { animator.SetTrigger(HashBeginMove); } else { animator.ResetTrigger(HashBeginMove); }
      CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.BeginMove " + animator.name);
    }
    public float TurnParam {
      set {
        if (animator == null) { return; }
        if (HasTurnParam == false) { return; }
        animator.SetFloat(HashTurnParam, value);
        CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.TurnParam " + animator.name + " " + value);
      }
    }
    public void DeathAnim() {
      if (animator == null) { return; }
      if (HasDeathAnim == false) { return; }
      animator.SetTrigger(HashDeathAnim);
      CustomAmmoCategoriesLog.Log.O?.TWL(0, "CustomRepresentationAnimatorInfo.DeathAnim " + animator.name);
    }
    public CustomRepresentationAnimatorInfo(Animator animator) {
      this.animator = animator;
      HasInBattle = false;
      HashInBattle = Animator.StringToHash("in_battle");
      HasCharge = false;
      HashCharge = Animator.StringToHash("charge");
      HasForward = false;
      HashForward = Animator.StringToHash("forward");
      HasDropOff = false;
      HashDropOff = Animator.StringToHash("drop_off");
      HasIsMoving = false;
      HashIsMoving = Animator.StringToHash("is_moving");
      HasBeginMove = false;
      HashBeginMove = Animator.StringToHash("begin_move_trigger");
      HasTurnParam = false;
      HashTurnParam = Animator.StringToHash("turn_param");
      HasDeathAnim = false;
      HashDeathAnim = Animator.StringToHash("death_trigger");
      if (this.animator == null) { return; }
      AnimatorControllerParameter[] parameter = this.animator.parameters;
      foreach(AnimatorControllerParameter param in parameter) {
        if (param.nameHash == HashInBattle) { HasInBattle = true; }
        if (param.nameHash == HashCharge) { HasCharge = true; }
        if (param.nameHash == HashForward) { HasForward = true; }
        if (param.nameHash == HashDropOff) { HasDropOff = true; }
        if (param.nameHash == HashIsMoving) { HasIsMoving = true; }
        if (param.nameHash == HashBeginMove) { HasBeginMove = true; }
        if (param.nameHash == HashTurnParam) { HasTurnParam = true; }
        if (param.nameHash == HashDeathAnim) { HasDeathAnim = true; }
      }
    }
  }
  public class CustomParticleSystemRep {
    public ParticleSystem ps { get; set; }
    public ChassisLocations location { get; set; }
    public CustomParticleSystemRep(ParticleSystem obj, ChassisLocations loc) {
      this.ps = obj; this.location = loc;
    }
  }
  public class CustomRepresentation : MonoBehaviour {
    //public AlternateMechRepresentations altRepresentations { get; protected set; }
    public List<CustomMouseInteactions> custMouseReceivers { get; set; } = new List<CustomMouseInteactions>();
    protected PilotableActorRepresentation f_GameRepresentation;
    public PilotableActorRepresentation GameRepresentation {
      get {
        return f_GameRepresentation;
      }
      set {
        f_GameRepresentation = value;
        foreach (CustomMouseInteactions ci in custMouseReceivers) { ci.parentRep = value; }
      }
    }
    public MechRepresentationSimGame SimGameRepresentation { get; set; }
    public MechRepresentation mechRepresentation { get { return f_GameRepresentation as MechRepresentation; } }
    public CustomActorRepresentationDef CustomDefinition { get; protected set; }
    public Dictionary<string, AttachInfo> WeaponAttachPoints { get; protected set; } = new Dictionary<string, AttachInfo>();
    public HashSet<CustomRepresentationAnimatorInfo> animators { get; protected set; } = new HashSet<CustomRepresentationAnimatorInfo>();
    public HashSet<CustomTwistAnimatorInfo> twistAnimators { get; protected set; } = new HashSet<CustomTwistAnimatorInfo>();
    public bool HasTwistAnimators { get { return twistAnimators.Count > 0; } }
    public List<Transform> Headlights { get; protected set; }
    private float dropOff_t = 0f;
    private Action dropOffAnimationCompleete = null;
    public void Update() {
      if(dropOff_t > 0f) {
        dropOff_t -= Time.deltaTime;
        if (dropOff_t <= 0f) {
          InDropOff = false;
          dropOffAnimationCompleete?.Invoke();
          dropOff_t = 0f;
        }
      }
    }
    public void DropOffAnimation(Action animCompleete) {
      if (dropOff_t == 0f) {
        InDropOff = true;
        dropOff_t = 1f;
        dropOffAnimationCompleete = animCompleete;
      } else {
        animCompleete?.Invoke();
      }
    }
    public bool InBattle { set { foreach (CustomRepresentationAnimatorInfo info in animators) { info.InBattle = value; } } }
    public bool InDropOff { set { foreach (CustomRepresentationAnimatorInfo info in animators) { info.DropOff = value; } } }
    public void Charge() { foreach (CustomRepresentationAnimatorInfo info in animators) { info.Charge(); } }
    public float Forward { set { foreach (CustomRepresentationAnimatorInfo info in animators) { info.Forward = value; } } }
    public bool IsMoving { set { foreach (CustomRepresentationAnimatorInfo info in animators) { info.IsMoving = value; } } }
    public float TurnParam { set { foreach (CustomRepresentationAnimatorInfo info in animators) { info.TurnParam = value; } } }
    public void BeginMove() { foreach (CustomRepresentationAnimatorInfo info in animators) { info.BeginMove(true); } }
    public float TwistR {
      set { foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.TwistR = value; } }
    }
    public float TwistL {
      set { foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.TwistL = value; } }
    }
    public float IdleTwistR {
      set { foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.IdleTwistR = value; } }
    }
    public float IdleTwistL {
      set { foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.IdleTwistL = value; } }
    }
    public bool StartRandomIdle {
      set {
        Log.TWL(0, "CustomTwistAnimation.StartRandomIdle: " + value);
        if (value) { if (idleTimeElapsed.IsRunning == false) { idleTimeElapsed.Reset(); idleTimeElapsed.Start(); }; } else { idleTimeElapsed.Reset(); idleTimeElapsed.Stop(); }
        foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.StartRandomIdle = value; }
      }
    }
    public void Twist(float angle) {
      //StartRandomIdle = false;
      angle *= (90f / 180f);
      Log.TWL(0, "CustomTwistAnimation.Twist " + angle);
      if (angle >= 0f) { this.TwistL = 0f; this.TwistR = angle; } else
      if (angle <= 0f) { this.TwistL = Mathf.Abs(angle); this.TwistR = 0f; }
    }
    private Stopwatch idleTimeElapsed = new Stopwatch();
    private bool isInIdleRotated = false;
    public void IdleTwist(float value) {
      Log.TWL(0, "CustomRepresentation.IdleTwist value:" + value + " IsRunning:" + idleTimeElapsed.IsRunning + " ElapsedMilliseconds:" + idleTimeElapsed.ElapsedMilliseconds + " isInIdleRotated:" + isInIdleRotated);
      //StartRandomIdle = true;
      if (idleTimeElapsed.IsRunning == false) { return; }
      if (idleTimeElapsed.ElapsedMilliseconds < 5000) { return; }
      idleTimeElapsed.Reset();
      idleTimeElapsed.Start();
      if (isInIdleRotated) {
        isInIdleRotated = false;
        foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.StartRandomIdle = false; }
        return;
      } else {
        foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.StartRandomIdle = true; }
        isInIdleRotated = true;
      }
      float angle = 0.0f;
      if (value == 0.6f) { angle = 0f; } else
      if (value == 0.7f) { angle = -0.3f; } else
      if (value == 0.8f) { angle = 0.5f; } else
      if (value == 0.9f) { angle = -0.5f; }
      Log.TWL(0, "CustomTwistAnimation.IdleTwist " + angle);
      if (angle >= 0f) { this.IdleTwistL = 0f; this.IdleTwistR = angle; } else
      if (angle <= 0f) { this.IdleTwistL = Mathf.Abs(angle); this.IdleTwistR = 0f; }
    }
    private void InitDestructable() {
      Log.TWL(0, "CustomRepresentation.InitDestructable");
      if (GameRepresentation == null) { return; }
      foreach (var ddef in CustomDefinition.destructibles) {
        if (string.IsNullOrEmpty(ddef.Value.Name)) { continue; }
        Transform destructableTR = null;
        Transform[] transforms = GameRepresentation.gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform tr in transforms) { if (tr.name == ddef.Value.Name) { destructableTR = tr; break; }; }
        if (destructableTR == null) { continue; }
        Log.WL(1, ddef.Value.Name + " found");
        MechDestructibleObject destr = destructableTR.gameObject.GetComponent<MechDestructibleObject>();
        if (destr == null) { destr = destructableTR.gameObject.AddComponent<MechDestructibleObject>(); }
        if (string.IsNullOrEmpty(ddef.Value.destroyedObj) == false) {
          Transform destroyedObjTR = null;
          foreach (Transform tr in transforms) { if (tr.name == ddef.Value.destroyedObj) { destroyedObjTR = tr; break; }; }
          Log.WL(2, ddef.Value.destroyedObj + " found");
          destr.destroyedObj = destroyedObjTR.gameObject;
        } else {
          destr.destroyedObj = null;
        }
        if (string.IsNullOrEmpty(ddef.Value.wholeObj) == false) {
          Transform wholeObjTR = null;
          foreach (Transform tr in transforms) { if (tr.name == ddef.Value.wholeObj) { wholeObjTR = tr; break; }; }
          Log.WL(2, ddef.Value.wholeObj + " found");
          destr.wholeObj = wholeObjTR.gameObject;
        } else {
          destr.wholeObj = null;
        }
        if (this.mechRepresentation != null) {
          switch (ddef.Key) {
            case ChassisLocations.Head: this.mechRepresentation.headDestructible = destr; break;
            case ChassisLocations.CenterTorso: this.mechRepresentation.centerTorsoDestructible = destr; break;
            case ChassisLocations.LeftTorso: this.mechRepresentation.leftTorsoDestructible = destr; break;
            case ChassisLocations.RightTorso: this.mechRepresentation.rightTorsoDestructible = destr; break;
            case ChassisLocations.LeftArm: this.mechRepresentation.leftArmDestructible = destr; break;
            case ChassisLocations.RightArm: this.mechRepresentation.rightArmDestructible = destr; break;
            case ChassisLocations.LeftLeg: this.mechRepresentation.leftLegDestructible = destr; break;
            case ChassisLocations.RightLeg: this.mechRepresentation.rightLegDestructible = destr; break;
          }
        }
        if (this.SimGameRepresentation != null) {
          switch (ddef.Key) {
            case ChassisLocations.Head: this.SimGameRepresentation.headDestructible = destr; break;
            case ChassisLocations.CenterTorso: this.SimGameRepresentation.centerTorsoDestructible = destr; break;
            case ChassisLocations.LeftTorso: this.SimGameRepresentation.leftTorsoDestructible = destr; break;
            case ChassisLocations.RightTorso: this.SimGameRepresentation.rightTorsoDestructible = destr; break;
            case ChassisLocations.LeftArm: this.SimGameRepresentation.leftArmDestructible = destr; break;
            case ChassisLocations.RightArm: this.SimGameRepresentation.rightArmDestructible = destr; break;
            case ChassisLocations.LeftLeg: this.SimGameRepresentation.leftLegDestructible = destr; break;
            case ChassisLocations.RightLeg: this.SimGameRepresentation.rightLegDestructible = destr; break;
          }
        }
      }
    }
    public void OnUnitDestroy() {
      Log.TWL(0, "CustomRepresentation.OnDestroy");
      this.InBattle = false;
      this.idleTimeElapsed.Stop();
      foreach (CustomTwistAnimatorInfo info in twistAnimators) { info.StartRandomIdle = false; }
      this.IdleTwistL = 0f;
      this.IdleTwistR = 0f;
      this.TwistL = 0f;
      this.TwistR = 0f;
      if (GameRepresentation == null) { return; }
      //return;
      if (CustomDefinition.OnDestroy.SuppressCombinedMesh) {
        MechRepresentation mechRep = this.GameRepresentation as MechRepresentation;
        if (mechRep != null) {
          MechMeshMerge mechMerge = Traverse.Create(mechRep).Field<MechMeshMerge>("mechMerge").Value;
          if (mechMerge != null) {
            GameObject newMeshRoot0 = Traverse.Create(mechMerge).Field<GameObject>("newMeshRoot0").Value;
            GameObject newMeshRoot1 = Traverse.Create(mechMerge).Field<GameObject>("newMeshRoot1").Value;
            if (newMeshRoot0 != null) { newMeshRoot0.SetActive(false); }
            if (newMeshRoot1 != null) { newMeshRoot1.SetActive(false); }
            Log.WL(1, "combined mesh suppressed");
          }
        }
      }
      if (CustomDefinition.OnDestroy.SuppressWeaponRepresentations) {
        MechRepresentation mechRep = this.GameRepresentation as MechRepresentation;
        foreach (WeaponRepresentation wRep in mechRep.weaponReps) {
          if (wRep == null) { continue; }
          wRep.gameObject.SetActive(false);
        }
      }
      if (CustomDefinition.OnDestroy.CollapseAllDestructables) {
        MechDestructibleObject[] destructables = this.GameRepresentation.gameObject.GetComponentsInChildren<MechDestructibleObject>(true);
        foreach (MechDestructibleObject destructable in destructables) {
          Traverse.Create(destructable).Field<Rigidbody[]>("destroyedRigidbodies").Value = null;
          Rigidbody[] rigs = destructable.GetComponentsInChildren<Rigidbody>(true);
          foreach (Rigidbody rig in rigs) { rig.gameObject.layer = LayerMask.NameToLayer("VFXPhysicsActive"); }
          destructable.gameObject.SetActive(true);
          bool forward = UnityEngine.Random.Range(0, 1f) > 0.5f;
          bool left = UnityEngine.Random.Range(0, 1f) > 0.5f;
          Vector3 explosionVec = new Vector3((left ? 1f : -1f) * UnityEngine.Random.Range(0.5f, 1f), 1f, (forward ? 1f : -1f) * UnityEngine.Random.Range(0.5f, 1f));
          explosionVec.Normalize();
          Log.WL(1, "destructable: " + destructable.transform.name + " Vector:" + explosionVec);
          destructable.Collapse(explosionVec, 15f);
          Log.WL(1, "collapse:" + destructable.transform.name);
        }
      }
    }
    public void AddBodyAnimator(Transform baseObj, string animName) {
      if (string.IsNullOrEmpty(animName)) { return; }
      Transform animTR = baseObj.FindRecursive(animName);
      if (animTR == null) { return; }
      Animator anim = animTR.gameObject.GetComponent<Animator>();
      if (anim == null) { return; }
      this.animators.Add(new CustomRepresentationAnimatorInfo(anim));
    }
    public void AddTwistAnimator(Transform baseObj, string animName) {
      if (string.IsNullOrEmpty(animName)) { return; }
      Transform animTR = baseObj.FindRecursive(animName);
      if (animTR == null) { return; }
      Animator anim = animTR.gameObject.GetComponent<Animator>();
      if (anim == null) { return; }
      this.twistAnimators.Add(new CustomTwistAnimatorInfo(anim));
    }
    public void AddWeaponAttachPoint(GameObject parent, AttachInfoRecord wAttach) {
      WeaponAttachPoints.Add(wAttach.Name, new AttachInfo(parent, wAttach));
    }
    public void Init(PilotableActorRepresentation rep, CustomActorRepresentationDef def) {
      Log.TWL(0, "CustomRepresentation.Init "+rep.name+" def:"+def.Id);
      GameRepresentation = rep;
      SimGameRepresentation = null;
      CustomDefinition = def;
      WeaponAttachPoints = new Dictionary<string, AttachInfo>();
      this.animators = new HashSet<CustomRepresentationAnimatorInfo>();
      Log.WL(1, "WeaponsAttachPoints:" + def.WeaponsAttachPoints.Count);
      foreach (AttachInfoRecord wAttach in def.WeaponsAttachPoints) {
        Log.WL(2, wAttach.Name);
        WeaponAttachPoints.Add(wAttach.Name, new AttachInfo(rep.gameObject, wAttach));
      }
      foreach (string animName in def.Animators) {
        Transform animTR = rep.transform.FindRecursive(animName);
        if (animTR == null) { continue; }
        Animator anim = animTR.gameObject.GetComponent<Animator>();
        if (anim == null) { continue; }
        this.animators.Add(new CustomRepresentationAnimatorInfo(anim));
      }
      foreach (string animName in def.TwistAnimators) {
        Transform animTR = rep.transform.FindRecursive(animName);
        if (animTR == null) { continue; }
        Animator anim = animTR.gameObject.GetComponent<Animator>();
        if (anim == null) { continue; }
        this.twistAnimators.Add(new CustomTwistAnimatorInfo(anim));
      }
      Headlights = new List<Transform>();
      foreach (string headlightA in def.HeadLights) {
        Transform headlightTR = rep.transform.FindRecursive(headlightA);
        if (headlightTR != null) { Headlights.Add(headlightTR); }
      }
      this.StartRandomIdle = true;
      this.InitDestructable();
      this.InitMouceReceivers();
    }
    public void InitMouceReceivers() {
      if (this.GameRepresentation == null) { return; }
      foreach (Transform tr in this.GameRepresentation.gameObject.GetComponentsInChildren<Transform>(true)) {
        if (this.CustomDefinition.CustomMouseReceiver.Contains(tr.name) == false) { continue; }
        if (tr.GetComponent<Collider>() == null) { continue; }
        CustomMouseInteactions cmi = tr.gameObject.AddComponent<CustomMouseInteactions>();
        cmi.parentRep = this.GameRepresentation;
        this.custMouseReceivers.Add(cmi);
      }
    }
    public void Init(MechRepresentationSimGame rep, CustomActorRepresentationDef def) {
      SimGameRepresentation = rep;
      GameRepresentation = null;
      CustomDefinition = def;
      WeaponAttachPoints = new Dictionary<string, AttachInfo>();
      this.animators = new HashSet<CustomRepresentationAnimatorInfo>();
      foreach (AttachInfoRecord wAttach in def.WeaponsAttachPoints) {
        WeaponAttachPoints.Add(wAttach.Name, new AttachInfo(rep.gameObject, wAttach));
      }
      foreach (string animName in def.Animators) {
        Transform animTR = rep.transform.FindRecursive(animName);
        if (animTR == null) { continue; }
        Animator anim = animTR.gameObject.GetComponent<Animator>();
        if (anim == null) { continue; }
        this.animators.Add(new CustomRepresentationAnimatorInfo(anim));
      }
      Headlights = new List<Transform>();
      foreach (string headlightA in def.HeadLights) {
        Transform headlightTR = rep.transform.FindRecursive(headlightA);
        if (headlightTR != null) { Headlights.Add(headlightTR); }
      }
      this.StartRandomIdle = false;
      this.InitDestructable();
    }
    public void AttachHeadlights() {
      Log.TWL(0, "CustomRepresentation.AttachHeadlights");
      MechRepresentation mechRep = this.GameRepresentation as MechRepresentation;
      if (mechRep == null) { return; }
      List<GameObject> headlightsReps = Traverse.Create(mechRep).Field<List<GameObject>>("headlightReps").Value;
      GameObject headlightSrcPrefab = mechRep.parentActor.Combat.DataManager.PooledInstantiate(Core.Settings.CustomHeadlightComponentPrefab, BattleTechResourceType.Prefab);
      Log.WL(1, "headlightSrcPrefab:" + (headlightSrcPrefab == null ? "null" : headlightSrcPrefab.name));
      if (headlightSrcPrefab != null) {
        //jumpJetSrcPrefab.printComponents(1);
        Transform headlightSrc = headlightSrcPrefab.transform.FindRecursive(Core.Settings.CustomHeadlightPrefabSrcObjectName);
        Log.WL(1, "headlightSrc:" + (headlightSrc == null ? "null" : headlightSrc.name));
        if (headlightSrc != null) {
          foreach (Transform headlightAttach in this.Headlights) {
            GameObject headlightBase = new GameObject("headlight");
            headlightBase.transform.SetParent(headlightAttach);
            headlightBase.transform.localPosition = Vector3.zero;
            headlightBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject headlight = GameObject.Instantiate(headlightSrc.gameObject);
            headlight.transform.SetParent(headlightBase.transform);
            headlight.transform.localPosition = Vector3.zero;
            headlight.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            headlightsReps.Add(headlightBase);
          }
        }
        mechRep.parentActor.Combat.DataManager.PoolGameObject(Core.Settings.CustomHeadlightComponentPrefab, headlightSrcPrefab);
      }
      Traverse.Create(mechRep).Field<List<GameObject>>("headlightReps").Value = headlightsReps;
    }
    public void AttachWeapons() {
      Log.TWL(0, "CustomRepresentation.AttachWeapons");
      try {
        if (this.GameRepresentation == null) { return; }
        foreach (WeaponRepresentation weaponRep in this.GameRepresentation.weaponReps) {
          if (weaponRep == null) { continue; }
          if (weaponRep.weapon == null) { continue; }
          try {
            Log.WL(1, "prefab " + weaponRep.weapon.mechComponentRef.prefabName);
            CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(weaponRep.weapon.mechComponentRef.prefabName);
            if (customHardpoint == null) { Log.WL(3, "no custom hardpoint"); continue; }
            Log.WL(2, "attachType:" + customHardpoint.attachType + " attachOverride:" + customHardpoint.attachOverride);
            AttachInfo attachPoint = null;
            if (string.IsNullOrEmpty(customHardpoint.attachOverride) == false) {
              if (this.WeaponAttachPoints.ContainsKey(customHardpoint.attachOverride)) {
                attachPoint = this.WeaponAttachPoints[customHardpoint.attachOverride];
              };
            }
            Log.WL(2, "attachPoint:" + (attachPoint == null ? "null" : attachPoint.attach.name));
            if (attachPoint != null) {
              if (weaponRep.parentTransform != null) { weaponRep.parentTransform = attachPoint.attach; }
              if (weaponRep.thisTransform != null) {
                weaponRep.thisTransform.SetParent(attachPoint.attach, false);
                weaponRep.thisTransform.localPosition = Vector3.zero;
                weaponRep.thisTransform.localScale = Vector3.one;
                weaponRep.thisTransform.localRotation = Quaternion.identity;
                WeaponAttachRepresentation attachRep = weaponRep.gameObject.GetComponent<WeaponAttachRepresentation>();
                if (attachRep == null) { attachRep = weaponRep.gameObject.AddComponent<WeaponAttachRepresentation>(); }
                attachRep.Init(weaponRep, attachPoint);
                if (weaponRep.weapon != null) { attachPoint.weapons.Add(weaponRep.weapon); }
              };
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
        foreach (var ap in this.WeaponAttachPoints) {
          if (ap.Value.hideIfEmpty == false) { continue; }
          if (ap.Value.main == null) { continue; }
          if (ap.Value.weapons.Count == 0) { ap.Value.main.gameObject.SetActive(false); }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public void AttachWeaponsSimGame() {
      Log.TWL(0, "CustomRepresentation.AttachWeaponsSimGame " + (this.SimGameRepresentation == null ? "null" : this.SimGameRepresentation.gameObject.name));
      try {
        if (this.SimGameRepresentation == null) { return; }
        List<ComponentRepresentation> componentReps = this.SimGameRepresentation.componentReps;
        if (this.SimGameRepresentation is CustomMechRepresentationSimGame custRepSimgame) {
          componentReps = new List<ComponentRepresentation>();
          foreach (var compRepsLocation in custRepSimgame.componentReps) {
            foreach (ComponentRepresentation compRep in compRepsLocation.Value) {
              componentReps.Add(compRep);
            }
          }
        }
        foreach (ComponentRepresentation compRep in componentReps) {
          try {
            string prefab = compRep.gameObject.name;
            MechSimGameComponentRepresentation simgameRep = compRep.gameObject.GetComponent<MechSimGameComponentRepresentation>();
            if (simgameRep != null) {
              prefab = simgameRep.OriginalPrefabId;
            }
            Log.WL(1, "prefab " + prefab);
            CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(prefab);
            if (customHardpoint == null) { Log.WL(3, "no custom hardpoint"); continue; }
            Log.WL(2, "attachType:" + customHardpoint.attachType + " attachOverride:" + customHardpoint.attachOverride);
            AttachInfo attachPoint = null;
            if (string.IsNullOrEmpty(customHardpoint.attachOverride) == false) {
              if (this.WeaponAttachPoints.ContainsKey(customHardpoint.attachOverride)) {
                attachPoint = this.WeaponAttachPoints[customHardpoint.attachOverride];
              };
            }
            Log.WL(2, "attachPoint:" + (attachPoint == null ? "null" : attachPoint.attach.name));
            if (attachPoint != null) {
              if (compRep.parentTransform != null) { compRep.parentTransform = attachPoint.attach; }
              if (compRep.thisTransform != null) {
                compRep.thisTransform.SetParent(attachPoint.attach, false);
                compRep.thisTransform.localPosition = Vector3.zero;
                compRep.thisTransform.localScale = Vector3.one;
                compRep.thisTransform.localRotation = Quaternion.identity;
                WeaponAttachRepresentation attachRep = compRep.gameObject.GetComponent<WeaponAttachRepresentation>();
                if (attachRep == null) { attachRep = compRep.gameObject.AddComponent<WeaponAttachRepresentation>(); }
                attachRep.Init(compRep, attachPoint);

                //HACKY FIX: I'm not sure what KMission's intent with weapons was, but as far as I know they don't take paint.
                //  Because of that, I'm disabling the paint patterns on them here. This prevents the 'corrupted texture' look in the mechbay.
                CustomPaintPattern[] paintPatterns = attachRep.gameObject.GetComponentsInChildren<CustomPaintPattern>();
                foreach (CustomPaintPattern cpp in paintPatterns)
                {                    
                    cpp._paintPatterns.Clear();
                    cpp._currentIndex = -1;
                }
              };
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class CustomActorRepresentationHelper {
    private static Dictionary<string, CustomActorRepresentationDef> customRepresentations = new Dictionary<string, CustomActorRepresentationDef>();
    private static Dictionary<string, CustomActorRepresentationDef> customSimGameRepresentations = new Dictionary<string, CustomActorRepresentationDef>();
    public static bool AddEventUnique(this AnimationClip clip, AnimationEvent aevent) {
      foreach (var existing_event in clip.events) {
        if(existing_event.time == aevent.time) {
          if(existing_event.functionName == aevent.functionName) {
            if((existing_event.stringParameter == aevent.stringParameter)
              && (existing_event.intParameter == aevent.intParameter)
              && (existing_event.floatParameter == aevent.floatParameter)
              ) {
              return false;
            }
          }
        }
      }
      clip.AddEvent(aevent);
      return true;
    }
    public static void Register(this CustomActorRepresentationDef customRepDef) {
      if (customRepresentations.ContainsKey(customRepDef.Id)) {
        Log.TWL(0, "Custom" + customRepDef.RepType + "RepresentationDef " + customRepDef.Id + " already registered skipping");
        return;
      }
      customRepresentations.Add(customRepDef.Id, customRepDef);
      string simGamePrefabName = string.Format("chrPrfComp_{0}_simgame", customRepDef.PrefabBase);
      if (customSimGameRepresentations.ContainsKey(simGamePrefabName)) {
        Log.TWL(0, "Custom" + customRepDef.RepType + "SimGameRepresentation " + simGamePrefabName + " already registered skipping");
        return;
      }
      customSimGameRepresentations.Add(simGamePrefabName, customRepDef);
    }
    public static CustomMechRepresentationDef defaultCustomMechRepDef = new CustomMechRepresentationDef();
    public static CustomActorRepresentationDef Find(string id) {
      if (customRepresentations.TryGetValue(id, out CustomActorRepresentationDef result)) {
        return result;
      }
      return null;
    }
    public static CustomActorRepresentationDef FindSimGame(string id) {
      if (customSimGameRepresentations.TryGetValue(id, out CustomActorRepresentationDef result)) {
        return result;
      }
      return null;
    }
    //public static void PrepareForCustom(this VehicleRepresentation vehicleRep) {
    //  Transform j_Root = null;
    //  Transform mesh = null;
    //  HashSet<Transform> other_transforms = null;
    //  vehicleRep.gameObject.name = "j_Body";

    //}
    public static void CopyModel(this MechRepresentation mechRep, GameObject meshSource, DataManager dataManager) {
      Log.TWL(0, "CustomActorRepresentationHelper.CopyMechRep " + mechRep.name + " meshSrc:" + meshSource.name);
      try {
        SkinnedMeshRenderer[] recvRenderers = mechRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer[] srcRenderers = meshSource.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Dictionary<string, SkinnedMeshRenderer> targetRenderers = new Dictionary<string, SkinnedMeshRenderer>();
        Dictionary<string, SkinnedMeshRenderer> sourceRenderers = new Dictionary<string, SkinnedMeshRenderer>();
        Material srcBaseMat = null;
        Material srcAddMat = null;
        foreach (SkinnedMeshRenderer renderer in srcRenderers) {
          string suffix = renderer.name.Substring(renderer.name.IndexOf('_'));
          sourceRenderers.Add(suffix, renderer);
          if (renderer.sharedMaterial.name.Contains("base")) {
            if (srcBaseMat == null) srcBaseMat = renderer.sharedMaterial;
          } else {
            if (srcAddMat == null) srcAddMat = renderer.sharedMaterial;
          }
        }
        Log.WL(1, "base source material:" + (srcBaseMat == null ? "null" : srcBaseMat.name));
        Log.WL(1, "additional source material:" + (srcAddMat == null ? "null" : srcAddMat.name));
        Material trgBaseMat = null;
        Material trgAddMat = null;
        foreach (SkinnedMeshRenderer renderer in recvRenderers) {
          if (renderer.sharedMaterial.name.Contains("base")) {
            if ((trgBaseMat == null) && (srcBaseMat != null)) {
              trgBaseMat = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
              Log.WL(1, "altering base material:" + renderer.sharedMaterial.name);
              trgBaseMat.SetTexture("_MainTex", srcBaseMat.GetTexture("_MainTex"));
              trgBaseMat.SetTextureOffset("_MainTex", srcBaseMat.GetTextureOffset("_MainTex"));
              trgBaseMat.SetTextureScale("_MainTex", srcBaseMat.GetTextureScale("_MainTex"));

              trgBaseMat.SetTexture("_MetallicGlossMap", srcBaseMat.GetTexture("_MetallicGlossMap"));
              trgBaseMat.SetTextureOffset("_MetallicGlossMap", srcBaseMat.GetTextureOffset("_MetallicGlossMap"));
              trgBaseMat.SetTextureScale("_MetallicGlossMap", srcBaseMat.GetTextureScale("_MetallicGlossMap"));

              trgBaseMat.SetTexture("_BumpMap", srcBaseMat.GetTexture("_BumpMap"));
              trgBaseMat.SetTextureOffset("_BumpMap", srcBaseMat.GetTextureOffset("_BumpMap"));
              trgBaseMat.SetTextureScale("_BumpMap", srcBaseMat.GetTextureScale("_BumpMap"));

              trgBaseMat.SetTexture("_OcclusionMap", srcBaseMat.GetTexture("_OcclusionMap"));
              trgBaseMat.SetTextureOffset("_OcclusionMap", srcBaseMat.GetTextureOffset("_OcclusionMap"));
              trgBaseMat.SetTextureScale("_OcclusionMap", srcBaseMat.GetTextureScale("_OcclusionMap"));
            }
          } else {
            if ((trgAddMat == null) && (srcAddMat != null)) {
              trgAddMat = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
              Log.WL(1, "altering base material:" + renderer.sharedMaterial.name);
              trgAddMat.SetTexture("_MainTex", srcAddMat.GetTexture("_MainTex"));
              trgAddMat.SetTextureOffset("_MainTex", srcAddMat.GetTextureOffset("_MainTex"));
              trgAddMat.SetTextureScale("_MainTex", srcAddMat.GetTextureScale("_MainTex"));

              trgAddMat.SetTexture("_MetallicGlossMap", srcAddMat.GetTexture("_MetallicGlossMap"));
              trgAddMat.SetTextureOffset("_MetallicGlossMap", srcAddMat.GetTextureOffset("_MetallicGlossMap"));
              trgAddMat.SetTextureScale("_MetallicGlossMap", srcAddMat.GetTextureScale("_MetallicGlossMap"));

              trgBaseMat.SetTexture("_BumpMap", srcAddMat.GetTexture("_BumpMap"));
              trgBaseMat.SetTextureOffset("_BumpMap", srcAddMat.GetTextureOffset("_BumpMap"));
              trgBaseMat.SetTextureScale("_BumpMap", srcAddMat.GetTextureScale("_BumpMap"));

              trgAddMat.SetTexture("_OcclusionMap", srcAddMat.GetTexture("_OcclusionMap"));
              trgAddMat.SetTextureOffset("_OcclusionMap", srcAddMat.GetTextureOffset("_OcclusionMap"));
              trgAddMat.SetTextureScale("_OcclusionMap", srcAddMat.GetTextureScale("_OcclusionMap"));
            }
          }
        }
        Log.WL(1, "base target material:" + (trgBaseMat == null ? "null" : trgBaseMat.name));
        Log.WL(1, "additional target material:" + (trgAddMat == null ? "null" : trgAddMat.name));
        GameObject emptyGo = new GameObject("emptyMesh");
        emptyGo.AddComponent<MeshRenderer>();
        emptyGo.AddComponent<MeshFilter>();
        Mesh emptyMesh = emptyGo.GetComponent<MeshFilter>().mesh;
        Log.WL(1, "emptyMesh:" + (emptyMesh == null ? "null" : emptyMesh.name));
        foreach (SkinnedMeshRenderer renderer in recvRenderers) {
          if (renderer.sharedMaterials.Length > 1) { continue; }
          if (renderer.sharedMesh == null) { continue; }
          if (renderer.sharedMaterial == null) { continue; }
          string suffix = renderer.name.Substring(renderer.name.IndexOf('_'));
          if (renderer.sharedMaterial.name.Contains("base")) {
            if (trgBaseMat != null) { renderer.sharedMaterial = trgBaseMat; };
          } else {
            if (trgAddMat != null) { renderer.sharedMaterial = trgAddMat; }
          }
          if (sourceRenderers.TryGetValue(suffix, out SkinnedMeshRenderer srcRnd)) {
            renderer.sharedMesh = UnityEngine.Object.Instantiate(srcRnd.sharedMesh);
            renderer.sharedMesh.name = srcRnd.sharedMesh.name;
            renderer.localBounds = srcRnd.localBounds;
          } else {
            if (renderer.name.Contains("_dmg")) {
              continue;
            }
            if (renderer.gameObject.activeInHierarchy == false) {
              continue;
            }
            Log.WL(3, "destroying:" + renderer.name);
            renderer.sharedMesh = emptyMesh;
          }
        }
        Transform[] transforms = meshSource.GetComponentsInChildren<Transform>(true);
        Log.WL(1, "searching paint schemes holder");
        MechCustomization mechCustomization = mechRep.GetComponentInChildren<MechCustomization>(true);
        if (mechCustomization != null) {
          Log.WL(2, "MechCustomization found");
          foreach (Transform tr in transforms) {
            if (tr.name.Contains("camoholder")) {
              Log.WL(2, "camoholder found");
              MeshRenderer camosource = tr.gameObject.GetComponent<MeshRenderer>();
              if (camosource == null) { Log.WL(3, "no mesh renderer"); continue; }
              if (camosource.materials.Length == 0) { Log.WL(3, "no materials"); continue; }
              for (int i = 0; i < Math.Min(camosource.materials.Length, mechCustomization.paintPatterns.Length); ++i) {
                Log.WL(3, "material:" + camosource.materials[i].name);
                mechCustomization.paintPatterns[i] = camosource.materials[i].GetTexture("_MainTex") as Texture2D;
                Log.WL(4, "found paint scheme:" + (mechCustomization.paintPatterns[i] == null ? "null" : mechCustomization.paintPatterns[i].name));
              }
            };
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void CopyShader(this GameObject meshSource, DataManager dataManager, CustomActorRepresentationDef custRepDef) {
      if (string.IsNullOrEmpty(custRepDef.ShaderSource) == false) {
        GameObject shaderSource = dataManager.PooledInstantiate(custRepDef.ShaderSource, BattleTechResourceType.Prefab);
        if (shaderSource != null) {
          Log.WL(1, "shader prefab found");
          Renderer shaderComponent = null;
          Renderer[] shaderComponents = shaderSource.GetComponentsInChildren<Renderer>(true);
          foreach(Renderer shaderCmp in shaderComponents) {
            if (shaderCmp.sharedMaterial == null) { continue; }
            shaderComponent = shaderCmp;
            break;
          }
          if (shaderComponent != null) {
            Log.WL(1, "shader renderer found");
            Renderer[] shaderTargets = meshSource.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in shaderTargets) {
              SkinnedMeshRenderer s_renderer = renderer as SkinnedMeshRenderer;
              MeshRenderer m_renderer = renderer as MeshRenderer;
              if ((s_renderer == null) && (m_renderer == null)) { continue; }
              Log.WL(2, "renderer:" + renderer.name);
              for (int mindex = 0; mindex < renderer.sharedMaterials.Length; ++mindex) {
                Log.WL(3, "material:" + renderer.sharedMaterials[mindex].name + " <- " + shaderComponent.sharedMaterial.shader.name);
                renderer.sharedMaterials[mindex].shader = shaderComponent.sharedMaterial.shader;
                renderer.sharedMaterials[mindex].shaderKeywords = shaderComponent.sharedMaterial.shaderKeywords;
              }
            }
          }
          dataManager.PoolGameObject(custRepDef.ShaderSource, shaderSource);
        }
      }
    }
    public static void InitBindPoses(this GameObject meshSource) {
      SkinnedMeshRenderer[] renderers = meshSource.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      HashSet<Transform> nomerge = new HashSet<Transform>();
      Transform[] transforms = meshSource.GetComponentsInChildren<Transform>(true);
      foreach (Transform tr in transforms) {
        if (tr.name == "nomerge") { if (tr.parent != null) { nomerge.Add(tr.parent); } }
        if (tr.GetComponent<Rigidbody>() != null) { nomerge.Add(tr); }
      }
      foreach (SkinnedMeshRenderer renderer in renderers) {
        if (nomerge.Contains(renderer.transform)) { continue; }
        Transform[] meshBones = renderer.bones;
        bool flag = false;
        if (meshBones == null) { meshBones = new Transform[1]; flag = true; }
        if (meshBones.Length == 0) { meshBones = new Transform[1]; flag = true; }
        if (flag) { meshBones[0] = renderer.rootBone; }
        renderer.bones = meshBones;
        Matrix4x4[] bindPoses = renderer.sharedMesh.bindposes;
        flag = false;
        if (bindPoses == null) { bindPoses = new Matrix4x4[1]; flag = true; }
        if (bindPoses.Length == 0) { bindPoses = new Matrix4x4[1]; flag = true; }
        if (flag) {
          bindPoses[0] = Matrix4x4.identity;
        }
        renderer.sharedMesh.bindposes = bindPoses;
      }
    }
    public static void SuppressMeshes(this GameObject mechRep, CustomActorRepresentationDef custRepDef) {
      bool supress = false;
      if (custRepDef == null) { supress = true; } else { supress = custRepDef.SupressAllMeshes; }
      if (supress) {
        Transform mesh = mechRep.transform.FindRecursive("mesh");
        CustomMechRepresentation custRep = mechRep.GetComponent<CustomMechRepresentation>();
        HashSet<GameObject> toDestroy = new HashSet<GameObject>();
        foreach (Transform tr in mesh.GetComponentsInChildren<Transform>(true)) {
          if (tr.parent == mesh) { toDestroy.Add(tr.gameObject); }
        }
        foreach (GameObject obj in toDestroy) {
          if (custRep != null) { custRep.UnregisterRenderers(obj); }
          GameObject.DestroyImmediate(obj);
        }
      }
    }
    public static string RelativeName(this Transform tr, Transform parent) {
      Transform temp = tr;
      string result = temp.name;
      while (temp.parent != null) {
        if (temp.parent == parent) { break; }
        temp = temp.parent;
        result = temp.name + "/" + result;
      }
      return result;
    }
    public static void MoveBonesAndRenderers(this GameObject meshSource, GameObject mechRep, CustomActorRepresentationDef custRepDef) {
      Transform meshesTrg = mechRep.transform.FindRecursive("mesh");
      Transform meshesSrc = meshSource.transform.FindRecursive("meshes");
      if (meshesSrc == null) {
        meshesSrc = meshSource.transform.FindRecursive("mesh");
      }
      if ((meshesSrc != null) && (meshesTrg != null)) {
        HashSet<Transform> visuals = new HashSet<Transform>();
        foreach (Transform tr in meshesSrc.GetComponentInChildren<Transform>(true)) {
          if (tr.parent != meshesSrc) { continue; }
          visuals.Add(tr);
        }
        foreach (Transform tr in visuals) {
          Vector3 pos = tr.localPosition;
          Quaternion rot = tr.localRotation;
          Vector3 scale = tr.localScale;
          tr.SetParent(meshesTrg, false);
          tr.localPosition = pos;
          tr.localRotation = rot;
          tr.localScale = scale;
        }
      }
      Dictionary<string, Transform> resultTransforms = new Dictionary<string, Transform>();
      Transform j_Root = mechRep.transform.FindRecursive("j_Root");
      resultTransforms.Add("j_Root", j_Root);
      foreach (Transform tr in j_Root.GetComponentsInChildren<Transform>(true)) {
        if (tr == j_Root) { continue; }; resultTransforms.Add(tr.RelativeName(j_Root.parent), tr);
      }
      Log.TWL(0, "MoveBonesAndRenderers from " + meshSource.name + " to " + mechRep.name);
      foreach (var rt in resultTransforms) { Log.WL(1, rt.Key); };
      Transform bones = meshSource.transform.FindRecursive("bones");
      if (bones != null) {
        Log.WL(1, "merging");
        foreach (Transform tr in bones.GetComponentsInChildren<Transform>(true)) {
          if (tr == bones) { continue; }
          string tr_name = tr.RelativeName(bones);
          Log.WL(2, "name: " + tr_name);
          if (resultTransforms.TryGetValue(tr_name, out Transform resultTransform)) {
            if (custRepDef.MoveSkeletalBones) {
              Log.WL(3, "pos:" + resultTransform.localPosition + "=>" + tr.localPosition + " rot:" + resultTransform.localRotation.eulerAngles + "=>" + tr.localRotation + " scale:" + resultTransform.localScale + "=>" + tr.localScale);
              resultTransform.localPosition = tr.localPosition;
              resultTransform.localRotation = tr.localRotation;
              resultTransform.localScale = tr.localScale;
            }
            continue;
          }
          if (tr.parent == null) { continue; }
          string parent_name = tr.parent.RelativeName(bones);
          Log.WL(2, "parent name: " + parent_name);
          if (resultTransforms.ContainsKey(parent_name) == false) { continue; }
          Log.WL(2, "moving");
          Transform parent = resultTransforms[parent_name];
          Vector3 pos = tr.localPosition;
          Quaternion rot = tr.localRotation;
          Vector3 scale = tr.localScale;
          tr.SetParent(parent, false);
          tr.localPosition = pos;
          tr.localRotation = rot;
          tr.localScale = scale;
          resultTransforms.Add(tr_name, tr);
          Log.WL(2, "adding");
          foreach (Transform subTR in tr.GetComponentsInChildren<Transform>()) {
            if (subTR == tr) { continue; }
            string str_name = subTR.RelativeName(j_Root.parent);
            Log.WL(3, "str_name:" + str_name);
            resultTransforms.Add(str_name, subTR);
          }
        }
        if (meshesTrg != null) {
          Log.WL(1, "skinned renderers:");
          foreach (SkinnedMeshRenderer r in meshesTrg.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
            Log.WL(2, r.name);
            if (r.rootBone != null) {
              if (resultTransforms.ContainsValue(r.rootBone) == false) {
                string tr_name = r.rootBone.RelativeName(bones);
                if (resultTransforms.ContainsKey(tr_name)) {
                  r.rootBone = resultTransforms[tr_name];
                  Log.WL(3, "rootBone:" + tr_name + " moved to");
                }
              }
            }
            List<Transform> r_bones = new List<Transform>(r.bones);
            for (int i = 0; i < r_bones.Count; ++i) {
              if (r_bones[i] == null) { continue; }
              if (resultTransforms.ContainsValue(r_bones[i]) == false) {
                string tr_name = r_bones[i].RelativeName(bones);
                if (resultTransforms.ContainsKey(tr_name)) {
                  r_bones[i] = resultTransforms[tr_name];
                }
              }
            }
            r.bones = r_bones.ToArray();
            for (int i = 0; i < r.bones.Length; ++i) {
              Log.WL(3, "bones[" + i + "] -> " + (r.bones[i] == null ? "null" : r.bones[i].RelativeName(j_Root)));
            }
          }
        }
        if (custRepDef.MoveAnimations) {
          Animator sourceAnimator = bones.gameObject.GetComponent<Animator>();
          Animator targetAnimator = mechRep.GetComponent<Animator>();
          Log.WL(1, "source animator:" + (sourceAnimator == null ? "null" : "not null"));
          Log.WL(1, "target animator:" + (targetAnimator == null ? "null" : "not null"));
          if (sourceAnimator != null && targetAnimator != null) {
            Log.WL(1, "Moving animator");
            //Log.WL(2, "source animator");
            Dictionary<string, AnimationClip> sourceClips = new Dictionary<string, AnimationClip>();
            foreach (AnimationClip clip in sourceAnimator.runtimeAnimatorController.animationClips) {
              sourceClips.Add(clip.name, clip);
            }
            Log.WL(2, "target animator");
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(targetAnimator.runtimeAnimatorController);
            Dictionary<string, AnimationClip> overrideClips = new Dictionary<string, AnimationClip>();
            foreach (AnimationClip clip in sourceAnimator.runtimeAnimatorController.animationClips) {
              Log.WL(3, clip.name);
              if (sourceClips.TryGetValue(clip.name, out var overrideClip)) {
                overrideClips.Add(clip.name, overrideClip);
                foreach (AnimationEvent aevent in clip.events) {
                  overrideClip.AddEventUnique(aevent);
                }
              }
            }
            foreach (var overrideClip in overrideClips) {
              animatorOverrideController[overrideClip.Key] = overrideClip.Value;
              custRepDef.ApplyAnimationEvent(overrideClip.Value);
            }
            targetAnimator.runtimeAnimatorController = animatorOverrideController;
            Log.WL(2, "resulting clips");
            foreach (var clip in targetAnimator.runtimeAnimatorController.animationClips) {
              if (clip.events.Length > 0) {
                Log.WL(3, "\"" + clip.name + "\":[");
                bool first = true;
                foreach (var cevent in clip.events) {
                  if (first) { first = false; Log.W(3, " "); } else { Log.W(4, ","); }
                  Log.W(0, "{ \"time\": " + cevent.time + ",\"functionName\": \"" + cevent.functionName + "\"");
                  if (string.IsNullOrEmpty(cevent.stringParameter) == false) {
                    Log.W(1, ",\"stringParameter\":\"" + cevent.stringParameter + "\"");
                  }
                  if(cevent.functionName == "OnFootFall") {
                    Log.W(1, ",\"intParameter\":" + cevent.intParameter);
                  }
                  Log.WL(1, "}");
                }
                Log.WL(3, "],");
              } else {
                Log.WL(3, "\"" + clip.name + "\":[],");
              }
            }
          }
        }
      }
      foreach(MonoBehaviour component in mechRep.GetComponentsInChildren<MonoBehaviour>(true)) {
        if (component is IEnableOnMove enInited) { enInited.Init(); enInited.Enable(); enInited.Disable(); }
      }
      //custRepDef.ApplyGrounders(mechRep);
      //custRepDef.ApplySolvers(mechRep);
    }
    public static void MoveBonesAndRenderersSimGame(this CustomMechRepresentationSimGame mechRep, GameObject source) {
      Log.TWL(0, "MoveBonesAndRenderersSimGame:" + source.name);
      Transform meshesTrg = mechRep.transform.FindRecursive("mesh");
      Transform meshesSrc = source.transform.FindRecursive("meshes");
      if (meshesSrc == null) {
        meshesSrc = source.transform.FindRecursive("mesh");
      }
      if ((meshesSrc != null) && (meshesTrg != null)) {
        HashSet<Transform> visuals = new HashSet<Transform>();
        foreach (Transform tr in meshesSrc.GetComponentInChildren<Transform>(true)) {
          if (tr.parent != meshesSrc) { continue; }
          visuals.Add(tr);
        }
        foreach (Transform tr in visuals) {
          Vector3 pos = tr.localPosition;
          Quaternion rot = tr.localRotation;
          Vector3 scale = tr.localScale;
          tr.SetParent(meshesTrg, false);
          tr.localPosition = pos;
          tr.localRotation = rot;
          tr.localScale = scale;
        }
      }
      Dictionary<string, Transform> targetTransformsDict = new Dictionary<string, Transform>();
      HashSet<Transform> targetTransforms = new HashSet<Transform>();
      Transform target_j_Root = mechRep.transform.FindRecursive("j_Root");
      Transform source_j_Root = source.transform.FindRecursive("j_Root");
      //source_j_Root.SetParent(target_j_Root.parent, false);
      //resultTransforms.Add("j_Root", j_Root);
      foreach (Transform tr in target_j_Root.GetComponentsInChildren<Transform>(true)) {
        if (tr == target_j_Root) { continue; };
        targetTransforms.Add(tr);
        targetTransformsDict.Add(tr.RelativeName(target_j_Root), tr);
      }
      foreach (SkinnedMeshRenderer r in meshesTrg.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
        if (r.rootBone == null) { continue; }
        if (targetTransforms.Contains(r.rootBone)) { continue; }
        string rootBoneName = r.rootBone.RelativeName(source_j_Root);
        Log.WL(1, r.transform.name + " rootBone:" + rootBoneName);
        if (targetTransformsDict.TryGetValue(rootBoneName, out Transform newRootBone)) {
          Log.WL(2, "found");
          r.rootBone = newRootBone;
        }
      }

      //Log.TWL(0, "MoveBonesAndRenderers from " + meshSource.name + " to " + mechRep.name);
      //foreach (var rt in resultTransforms) { Log.WL(1, rt.Key); };
      //Transform bones = meshSource.transform.FindRecursive("bones");
      //if (bones != null) {
      //  Log.WL(1, "merging");
      //  foreach (Transform tr in bones.GetComponentsInChildren<Transform>(true)) {
      //    if (tr == bones) { continue; }
      //    string tr_name = tr.RelativeName(bones);
      //    Log.WL(2, "name: " + tr_name);
      //    if (resultTransforms.ContainsKey(tr_name)) { continue; }
      //    if (tr.parent == null) { continue; }
      //    string parent_name = tr.parent.RelativeName(bones);
      //    Log.WL(2, "parent name: " + parent_name);
      //    if (resultTransforms.ContainsKey(parent_name) == false) { continue; }
      //    Log.WL(2, "moving");
      //    Transform parent = resultTransforms[parent_name];
      //    Vector3 pos = tr.localPosition;
      //    Quaternion rot = tr.localRotation;
      //    Vector3 scale = tr.localScale;
      //    tr.SetParent(parent, false);
      //    tr.localPosition = pos;
      //    tr.localRotation = rot;
      //    tr.localScale = scale;
      //    resultTransforms.Add(tr_name, tr);
      //    Log.WL(2, "adding");
      //    foreach (Transform subTR in tr.GetComponentsInChildren<Transform>()) {
      //      if (subTR == tr) { continue; }
      //      string str_name = subTR.RelativeName(j_Root.parent);
      //      Log.WL(3, "str_name:" + str_name);
      //      resultTransforms.Add(str_name, subTR);
      //    }
      //  }
      //  if (meshesTrg != null) {
      //    Log.WL(1, "skinned renderers:");
      //    foreach (SkinnedMeshRenderer r in meshesTrg.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
      //      Log.W(2, r.name);
      //      if (r.rootBone == null) { Log.WL(1, "no root bone"); continue; }
      //      if (resultTransforms.ContainsValue(r.rootBone)) { Log.WL(1, "already moved"); continue; }
      //      string tr_name = r.rootBone.RelativeName(bones);
      //      if (resultTransforms.ContainsKey(tr_name) == false) { Log.WL(1, tr_name + " not exists"); continue; }
      //      r.rootBone = resultTransforms[tr_name];
      //      Log.WL(1, tr_name + " moved to");
      //    }
      //  }
      //}
    }
    public static void MoveVFXTransforms(this MechRepresentationSimGame mechRep, GameObject meshSource, CustomActorRepresentationDef custRepDef) {
      CustomMechRepresentationDef custMechDef = custRepDef as CustomMechRepresentationDef;
      if (custMechDef != null) {
        if (string.IsNullOrEmpty(custMechDef.TorsoAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.TorsoAttach);
          if (attach != null) { mechRep.TorsoAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.LeftArmAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.LeftArmAttach);
          if (attach != null) { mechRep.LeftArmAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.RightArmAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.RightArmAttach);
          if (attach != null) { mechRep.RightArmAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.LeftLegAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.LeftLegAttach);
          if (attach != null) { mechRep.LeftLegAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.RightLegAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.RightLegAttach);
          if (attach != null) { mechRep.RightLegAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxCenterTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxCenterTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxCenterTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxLeftTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxRightTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxHeadTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxHeadTransform);
          if (vfxTransform != null) { mechRep.vfxHeadTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftArmTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftArmTransform);
          if (vfxTransform != null) { mechRep.vfxLeftArmTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightArmTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightArmTransform);
          if (vfxTransform != null) { mechRep.vfxRightArmTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftLegTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftLegTransform);
          if (vfxTransform != null) { mechRep.vfxLeftLegTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightLegTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightLegTransform);
          if (vfxTransform != null) { mechRep.vfxRightLegTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftShoulderTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftShoulderTransform);
          if (vfxTransform != null) { mechRep.vfxLeftShoulderTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightShoulderTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightShoulderTransform);
          if (vfxTransform != null) { mechRep.vfxRightShoulderTransform = vfxTransform; }
        }
      }
    }
    public static void MoveVFXTransforms(this MechRepresentation mechRep, CustomActorRepresentationDef custRepDef) {
      CustomMechRepresentationDef custMechDef = custRepDef as CustomMechRepresentationDef;
      Log.TWL(0, "");
      if (custMechDef != null) {
        if (string.IsNullOrEmpty(custMechDef.TorsoAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.TorsoAttach);
          if (attach != null) { mechRep.TorsoAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.LeftArmAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.LeftArmAttach);
          if (attach != null) { mechRep.LeftArmAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.RightArmAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.RightArmAttach);
          if (attach != null) { mechRep.RightArmAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.LeftLegAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.LeftLegAttach);
          if (attach != null) { mechRep.LeftLegAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.RightLegAttach) == false) {
          Transform attach = mechRep.transform.FindRecursive(custMechDef.RightLegAttach);
          if (attach != null) { mechRep.RightLegAttach = attach; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxCenterTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxCenterTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxCenterTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxLeftTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightTorsoTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightTorsoTransform);
          if (vfxTransform != null) { mechRep.vfxRightTorsoTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxHeadTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxHeadTransform);
          if (vfxTransform != null) { mechRep.vfxHeadTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftArmTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftArmTransform);
          if (vfxTransform != null) { mechRep.vfxLeftArmTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightArmTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightArmTransform);
          if (vfxTransform != null) { mechRep.vfxRightArmTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftLegTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftLegTransform);
          if (vfxTransform != null) { mechRep.vfxLeftLegTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightLegTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightLegTransform);
          if (vfxTransform != null) { mechRep.vfxRightLegTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxLeftShoulderTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxLeftShoulderTransform);
          if (vfxTransform != null) { mechRep.vfxLeftShoulderTransform = vfxTransform; }
        }
        if (string.IsNullOrEmpty(custMechDef.vfxRightShoulderTransform) == false) {
          Transform vfxTransform = mechRep.transform.FindRecursive(custMechDef.vfxRightShoulderTransform);
          if (vfxTransform != null) { mechRep.vfxRightShoulderTransform = vfxTransform; }
        }
      }
    }
    public static void MoveBlips(this MechRepresentation mechRep, DataManager dataManager, CustomActorRepresentationDef custRepDef) {
      Log.TWL(0,"MoveBlips:"+ custRepDef.Id + " "+ mechRep.gameObject.name);
      if (string.IsNullOrEmpty(custRepDef.BlipSource) == false) {
        GameObject blipSource = dataManager.PooledInstantiate(custRepDef.BlipSource, BattleTechResourceType.Prefab);
        if (blipSource != null) {
          PilotableActorRepresentation actorRepresentation = blipSource.GetComponent<PilotableActorRepresentation>();
          if (actorRepresentation != null) {
            if (actorRepresentation.BlipObjectUnknown != null) {
              if (mechRep.BlipObjectUnknown != null) { GameObject.Destroy(mechRep.BlipObjectUnknown); mechRep.BlipObjectUnknown = null; }
              mechRep.BlipObjectUnknown = GameObject.Instantiate(actorRepresentation.BlipObjectUnknown);
              mechRep.BlipObjectUnknown.name = "BlipObjectUnknown";
              mechRep.BlipObjectUnknown.transform.SetParent(mechRep.transform);
            }
            if (actorRepresentation.BlipObjectIdentified != null) {
              if (mechRep.BlipObjectIdentified != null) { GameObject.Destroy(mechRep.BlipObjectIdentified); mechRep.BlipObjectIdentified = null; }
              mechRep.BlipObjectIdentified = GameObject.Instantiate(actorRepresentation.BlipObjectIdentified);
              mechRep.BlipObjectIdentified.name = "BlipObjectIdentified";
              mechRep.BlipObjectIdentified.transform.SetParent(mechRep.transform);
            }
            if (actorRepresentation.BlipObjectGhostWeak != null) {
              if (mechRep.BlipObjectGhostWeak != null) { GameObject.Destroy(mechRep.BlipObjectGhostWeak); mechRep.BlipObjectGhostWeak = null; }
              mechRep.BlipObjectGhostWeak = GameObject.Instantiate(actorRepresentation.BlipObjectGhostWeak);
              mechRep.BlipObjectGhostWeak.name = "BlipObjectGhostWeak";
              mechRep.BlipObjectGhostWeak.transform.SetParent(mechRep.transform);
            }
            if (actorRepresentation.BlipObjectGhostStrong != null) {
              if (mechRep.BlipObjectGhostStrong != null) { GameObject.Destroy(mechRep.BlipObjectGhostStrong); mechRep.BlipObjectGhostStrong = null; }
              mechRep.BlipObjectGhostStrong = GameObject.Instantiate(actorRepresentation.BlipObjectGhostStrong);
              mechRep.BlipObjectGhostStrong.name = "BlipObjectGhostStrong";
              mechRep.BlipObjectGhostStrong.transform.SetParent(mechRep.transform);
            }
          }
          dataManager.PoolGameObject(custRepDef.BlipSource, blipSource);
        }
      }
      if (string.IsNullOrEmpty(custRepDef.BlipMeshSource) == false) {
        Log.WL(1, "BlipMeshSource:" + custRepDef.BlipMeshSource);
        GameObject blipSource = dataManager.PooledInstantiate(custRepDef.BlipMeshSource, BattleTechResourceType.Prefab);
        if (blipSource != null) {
          Log.WL(1, "blipSource:" + blipSource.gameObject.name);
          Dictionary<string, MeshFilter> sourceRenderers = new Dictionary<string, MeshFilter>();
          foreach(MeshFilter renderer in blipSource.GetComponentsInChildren<MeshFilter>(true)) {
            sourceRenderers.Add(renderer.gameObject.transform.name, renderer);
            Log.WL(2, renderer.gameObject.transform.name+ " mesh:" + renderer.mesh.name+ " sharedMesh:" + renderer.sharedMesh.name);
          }
          if(mechRep.BlipObjectUnknown != null) {
            foreach (MeshFilter renderer in mechRep.BlipObjectUnknown.GetComponentsInChildren<MeshFilter>(true)) {
              Log.WL(2, "BlipObjectUnknown:"+ renderer.gameObject.transform.name);
              if (sourceRenderers.TryGetValue(renderer.gameObject.transform.name, out var sourceRenderer)) {
                Log.WL(3, "found");
                Log.WL(3, renderer.gameObject.transform.name + ".sharedMesh = " + renderer.sharedMesh.name);
                Log.WL(3, renderer.gameObject.transform.name + ".mesh = " + renderer.mesh.name);
                renderer.sharedMesh = sourceRenderer.sharedMesh;
                renderer.mesh = sourceRenderer.mesh;
                Log.WL(3, renderer.gameObject.transform.name+ ".sharedMesh = "+ renderer.sharedMesh.name);
                Log.WL(3, renderer.gameObject.transform.name + ".mesh = " + renderer.mesh.name);
                renderer.transform.localPosition = sourceRenderer.transform.localPosition;
                renderer.transform.localRotation = sourceRenderer.transform.localRotation;
                renderer.transform.localScale = sourceRenderer.transform.localScale;
              }
            }
          }
          if (mechRep.BlipObjectIdentified != null) {
            foreach (MeshFilter renderer in mechRep.BlipObjectIdentified.GetComponentsInChildren<MeshFilter>(true)) {
              Log.WL(2, "BlipObjectIdentified:" + renderer.gameObject.transform.name);
              if (sourceRenderers.TryGetValue(renderer.gameObject.transform.name, out var sourceRenderer)) {
                Log.WL(3, "found");
                Log.WL(3, renderer.gameObject.transform.name + ".sharedMesh = " + renderer.sharedMesh.name);
                Log.WL(3, renderer.gameObject.transform.name + ".mesh = " + renderer.mesh.name);
                renderer.sharedMesh = sourceRenderer.sharedMesh;
                renderer.mesh = sourceRenderer.mesh;
                Log.WL(3, renderer.gameObject.transform.name + ".sharedMesh = " + renderer.sharedMesh.name);
                Log.WL(3, renderer.gameObject.transform.name + ".mesh = " + renderer.mesh.name);
                renderer.transform.localPosition = sourceRenderer.transform.localPosition;
                renderer.transform.localRotation = sourceRenderer.transform.localRotation;
                renderer.transform.localScale = sourceRenderer.transform.localScale;
              }
            }
          }
          GameObject.Destroy(blipSource);
        }
      }
    }
    public static void MoveBone(this CustomMechRepresentationSimGame mechRep, CustomActorRepresentationDef custRepDef, DataManager dataManager) {
      GameObject meshSource = null;
      if (string.IsNullOrEmpty(custRepDef.Id) == false) {
        meshSource = dataManager.PooledInstantiate(custRepDef.Id, BattleTechResourceType.Prefab);
        meshSource.TestMissingMaterials(custRepDef.Id, dataManager);
      }
      if (meshSource != null) {
        mechRep.gameObject.name = meshSource.name;
        meshSource.CopyShader(dataManager, custRepDef);
        //meshSource.InitBindPoses();
      }
      mechRep.gameObject.SuppressMeshes(custRepDef);
      if (meshSource != null) {
        mechRep.InitCustomParticles(meshSource, custRepDef);
        mechRep.StopCustomParticles();
        mechRep.InitCamoflage(meshSource);
        meshSource.MoveBonesAndRenderers(mechRep.gameObject, custRepDef);
        //mechRep.MoveBonesAndRenderersSimGame(meshSource);
      }
      mechRep.MoveVFXTransforms(meshSource, custRepDef);
      if (meshSource != null) { GameObject.Destroy(meshSource); }
    }
    public static void MoveBone(this CustomMechRepresentation mechRep, CustomActorRepresentationDef custRepDef, DataManager dataManager) {
      GameObject meshSource = null;
      if (string.IsNullOrEmpty(custRepDef.Id) == false) {
        meshSource = dataManager.PooledInstantiate(custRepDef.Id, BattleTechResourceType.Prefab);
        meshSource.TestMissingMaterials(custRepDef.Id, dataManager);
      }
      if (meshSource != null) {
        mechRep.gameObject.name = meshSource.name;
        meshSource.CopyShader(dataManager, custRepDef);
        //meshSource.InitBindPoses();
      }
      mechRep.gameObject.SuppressMeshes(custRepDef);
      if (meshSource != null) {
        mechRep.InitCustomParticles(meshSource, custRepDef);
        mechRep.StartCustomParticles();
        mechRep.InitCamoflage(meshSource);
        meshSource.MoveBonesAndRenderers(mechRep.gameObject, custRepDef);
      }
      mechRep.MoveVFXTransforms(custRepDef);
      mechRep.MoveBlips(dataManager, custRepDef);
      if (meshSource != null) { GameObject.Destroy(meshSource); }
    }
    public static void MoveNone(this CustomMechRepresentation mechRep, CustomActorRepresentationDef custRepDef, DataManager dataManager) {
      mechRep.gameObject.name = custRepDef.Id;
      mechRep.gameObject.SuppressMeshes(custRepDef);
      mechRep.MoveBlips(dataManager, custRepDef);
    }
    public static void InitCamoflage(this CustomMechRepresentationSimGame mechRep, GameObject source) {
      Log.TWL(0, "CustomMechRepresentation.InitCamoflage " + mechRep.name);
      Transform camoholder = null;
      Transform[] transforms = source.GetComponentsInChildren<Transform>(true);
      foreach (Transform tr in transforms) {
        if (tr.parent != source.transform) { continue; }
        if (tr.name != "camoholder") { continue; }
        camoholder = tr;
        break;
      }
      if (camoholder == null) { Log.WL(1, "camoholder not found"); return; }
      MeshRenderer renderer = camoholder.GetComponent<MeshRenderer>();
      if (renderer == null) { Log.WL(1, "camoholder has no meshRenderer"); return; }
      for (int index = 0; index < renderer.materials.Length; ++index) {
        if (index >= mechRep.defaultMechCustomization.paintPatterns.Length) { break; }
        mechRep.defaultMechCustomization.paintPatterns[index] = renderer.materials[index].GetTexture("_MainTex") as Texture2D;
        Log.WL(1, "material:" + renderer.materials[index].name);
        Log.WL(1, "found paint scheme:" + (mechRep.defaultMechCustomization.paintPatterns[index] == null ? "null" : mechRep.defaultMechCustomization.paintPatterns[index].name));
      }
    }
    public static void InitCamoflage(this CustomMechRepresentation mechRep, GameObject source) {
      Log.TWL(0, "CustomMechRepresentationSimGame.InitCamoflage " + mechRep.name);
      Transform camoholder = null;
      Transform[] transforms = source.GetComponentsInChildren<Transform>(true);
      foreach (Transform tr in transforms) {
        if (tr.parent != source.transform) { continue; }
        if (tr.name != "camoholder") { continue; }
        camoholder = tr;
        break;
      }
      if (camoholder == null) { Log.WL(1, "camoholder not found"); return; }
      MeshRenderer renderer = camoholder.GetComponent<MeshRenderer>();
      if (renderer == null) { Log.WL(1, "camoholder has no meshRenderer"); return; }
      for (int index = 0; index < renderer.materials.Length; ++index) {
        if (index >= mechRep.defaultMechCustomization.paintPatterns.Length) { break; }
        mechRep.defaultMechCustomization.paintPatterns[index] = renderer.materials[index].GetTexture("_MainTex") as Texture2D;
        Log.WL(1, "material:" + renderer.materials[index].name);
        Log.WL(1, "found paint scheme:" + (mechRep.defaultMechCustomization.paintPatterns[index] == null ? "null" : mechRep.defaultMechCustomization.paintPatterns[index].name));
      }
    }
    public static void CopyModel(this MechRepresentationSimGame mechRep, CustomActorRepresentationDef custRepDef, DataManager dataManager) {
      GameObject meshSource = null;
      if (string.IsNullOrEmpty(custRepDef.Id) == false) { meshSource = dataManager.PooledInstantiate(custRepDef.Id, BattleTechResourceType.Prefab); }
      Log.TWL(0, "CustomActorRepresentationHelper.CopyMechRep " + mechRep.name + " meshSrc:" + meshSource.name);
      try {
        SkinnedMeshRenderer[] recvRenderers = mechRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer[] srcRenderers = meshSource.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Dictionary<string, SkinnedMeshRenderer> targetRenderers = new Dictionary<string, SkinnedMeshRenderer>();
        Dictionary<string, SkinnedMeshRenderer> sourceRenderers = new Dictionary<string, SkinnedMeshRenderer>();
        Material srcBaseMat = null;
        Material srcAddMat = null;
        foreach (SkinnedMeshRenderer renderer in srcRenderers) {
          string suffix = renderer.name.Substring(renderer.name.IndexOf('_'));
          sourceRenderers.Add(suffix, renderer);
          if (renderer.sharedMaterial.name.Contains("base")) {
            if (srcBaseMat == null) srcBaseMat = renderer.sharedMaterial;
          } else {
            if (srcAddMat == null) srcAddMat = renderer.sharedMaterial;
          }
        }
        Log.WL(1, "base source material:" + (srcBaseMat == null ? "null" : srcBaseMat.name));
        Log.WL(1, "additional source material:" + (srcAddMat == null ? "null" : srcAddMat.name));
        Material trgBaseMat = null;
        Material trgAddMat = null;
        foreach (SkinnedMeshRenderer renderer in recvRenderers) {
          if (renderer.sharedMaterial.name.Contains("base")) {
            if ((trgBaseMat == null) && (srcBaseMat != null)) {
              trgBaseMat = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
              Log.WL(1, "altering base material:" + renderer.sharedMaterial.name);
              trgBaseMat.SetTexture("_MainTex", srcBaseMat.GetTexture("_MainTex"));
              trgBaseMat.SetTextureOffset("_MainTex", srcBaseMat.GetTextureOffset("_MainTex"));
              trgBaseMat.SetTextureScale("_MainTex", srcBaseMat.GetTextureScale("_MainTex"));

              trgBaseMat.SetTexture("_MetallicGlossMap", srcBaseMat.GetTexture("_MetallicGlossMap"));
              trgBaseMat.SetTextureOffset("_MetallicGlossMap", srcBaseMat.GetTextureOffset("_MetallicGlossMap"));
              trgBaseMat.SetTextureScale("_MetallicGlossMap", srcBaseMat.GetTextureScale("_MetallicGlossMap"));

              trgBaseMat.SetTexture("_BumpMap", srcBaseMat.GetTexture("_BumpMap"));
              trgBaseMat.SetTextureOffset("_BumpMap", srcBaseMat.GetTextureOffset("_BumpMap"));
              trgBaseMat.SetTextureScale("_BumpMap", srcBaseMat.GetTextureScale("_BumpMap"));

              trgBaseMat.SetTexture("_OcclusionMap", srcBaseMat.GetTexture("_OcclusionMap"));
              trgBaseMat.SetTextureOffset("_OcclusionMap", srcBaseMat.GetTextureOffset("_OcclusionMap"));
              trgBaseMat.SetTextureScale("_OcclusionMap", srcBaseMat.GetTextureScale("_OcclusionMap"));
            }
          } else {
            if ((trgAddMat == null) && (srcAddMat != null)) {
              trgAddMat = UnityEngine.Object.Instantiate(renderer.sharedMaterial);
              Log.WL(1, "altering base material:" + renderer.sharedMaterial.name);
              trgAddMat.SetTexture("_MainTex", srcAddMat.GetTexture("_MainTex"));
              trgAddMat.SetTextureOffset("_MainTex", srcAddMat.GetTextureOffset("_MainTex"));
              trgAddMat.SetTextureScale("_MainTex", srcAddMat.GetTextureScale("_MainTex"));

              trgAddMat.SetTexture("_MetallicGlossMap", srcAddMat.GetTexture("_MetallicGlossMap"));
              trgAddMat.SetTextureOffset("_MetallicGlossMap", srcAddMat.GetTextureOffset("_MetallicGlossMap"));
              trgAddMat.SetTextureScale("_MetallicGlossMap", srcAddMat.GetTextureScale("_MetallicGlossMap"));

              trgBaseMat.SetTexture("_BumpMap", srcAddMat.GetTexture("_BumpMap"));
              trgBaseMat.SetTextureOffset("_BumpMap", srcAddMat.GetTextureOffset("_BumpMap"));
              trgBaseMat.SetTextureScale("_BumpMap", srcAddMat.GetTextureScale("_BumpMap"));

              trgAddMat.SetTexture("_OcclusionMap", srcAddMat.GetTexture("_OcclusionMap"));
              trgAddMat.SetTextureOffset("_OcclusionMap", srcAddMat.GetTextureOffset("_OcclusionMap"));
              trgAddMat.SetTextureScale("_OcclusionMap", srcAddMat.GetTextureScale("_OcclusionMap"));
            }
          }
        }
        Log.WL(1, "base target material:" + (trgBaseMat == null ? "null" : trgBaseMat.name));
        Log.WL(1, "additional target material:" + (trgAddMat == null ? "null" : trgAddMat.name));
        GameObject emptyGo = new GameObject("emptyMesh");
        emptyGo.AddComponent<MeshRenderer>();
        emptyGo.AddComponent<MeshFilter>();
        Mesh emptyMesh = emptyGo.GetComponent<MeshFilter>().mesh;
        Log.WL(1, "emptyMesh:" + (emptyMesh == null ? "null" : emptyMesh.name));
        foreach (SkinnedMeshRenderer renderer in recvRenderers) {
          if (renderer.sharedMaterials.Length > 1) { continue; }
          if (renderer.sharedMesh == null) { continue; }
          if (renderer.sharedMaterial == null) { continue; }
          string suffix = renderer.name.Substring(renderer.name.IndexOf('_'));
          if (renderer.sharedMaterial.name.Contains("base")) {
            if (trgBaseMat != null) { renderer.sharedMaterial = trgBaseMat; };
          } else {
            if (trgAddMat != null) { renderer.sharedMaterial = trgAddMat; }
          }
          if (sourceRenderers.TryGetValue(suffix, out SkinnedMeshRenderer srcRnd)) {
            renderer.sharedMesh = UnityEngine.Object.Instantiate(srcRnd.sharedMesh);
            renderer.sharedMesh.name = srcRnd.sharedMesh.name;
            renderer.localBounds = srcRnd.localBounds;
          } else {
            if (renderer.name.Contains("_dmg")) {
              continue;
            }
            if (renderer.gameObject.activeInHierarchy == false) {
              continue;
            }
            Log.WL(3, "destroying:" + renderer.name);
            renderer.sharedMesh = emptyMesh;
          }
        }
        GameObject.Destroy(emptyGo);
        Transform[] transforms = meshSource.GetComponentsInChildren<Transform>(true);
        Log.WL(1, "searching paint schemes holder");
        MechCustomization mechCustomization = mechRep.GetComponentInChildren<MechCustomization>(true);
        if (mechCustomization != null) {
          Log.WL(2, "MechCustomization found");
          foreach (Transform tr in transforms) {
            if (tr.name.Contains("camoholder")) {
              Log.WL(2, "camoholder found");
              MeshRenderer camosource = tr.gameObject.GetComponent<MeshRenderer>();
              if (camosource == null) { Log.WL(3, "no mesh renderer"); continue; }
              if (camosource.materials.Length == 0) { Log.WL(3, "no materials"); continue; }
              for (int i = 0; i < Math.Min(camosource.materials.Length, mechCustomization.paintPatterns.Length); ++i) {
                Log.WL(3, "material:" + camosource.materials[i].name);
                mechCustomization.paintPatterns[i] = camosource.materials[i].GetTexture("_MainTex") as Texture2D;
                Log.WL(4, "found paint scheme:" + (mechCustomization.paintPatterns[i] == null ? "null" : mechCustomization.paintPatterns[i].name));
              }
            };
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static HashSet<Renderer> GetMeshRenderers(this GameObject go) {
      HashSet<Renderer> result = new HashSet<Renderer>();
      SkinnedMeshRenderer[] srs = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      MeshRenderer[] rs = go.GetComponentsInChildren<MeshRenderer>(true);
      foreach (var r in srs) { result.Add(r); }
      foreach (var r in rs) { result.Add(r); }
      return result;
    }
    public static void Prepare(this VehicleRepresentation vRep) {
      HashSet<Renderer> renderers = vRep.VisibleObject.GetMeshRenderers();
      Transform[] trs = vRep.VisibleObject.GetComponentsInChildren<Transform>(true);
      HashSet<Renderer> nomerges = new HashSet<Renderer>();
      foreach (Transform tr in trs) {
        if (tr.name != "nomerge") { continue; }
        Renderer r = tr.parent.GetComponent<Renderer>();
        if (r == null) { continue; }
        nomerges.Add(r);
      }
      foreach (Renderer renderer in renderers) {
        if (nomerges.Contains(renderer)) { continue; }
        GameObject nomerge = new GameObject("nomerge");
        nomerge.transform.SetParent(renderer.gameObject.transform);
      }
    }
    private static Dictionary<string, bool> BlipObjectNames = new Dictionary<string, bool>() { { "BlipObjectUnknown", true }, { "BlipObjectIdentified", true }, { "BlipObjectGhostWeak", false }, { "BlipObjectGhostStrong",false } };
    public static VehicleDrivenMechRepresentation InitFromVehicle(this VehicleRepresentation vRep, DataManager dataManager) {
      GameObject defaultMechRepGO = dataManager.PooledInstantiate(Core.Settings.DefaultMechBattleRepresentationPrefab, BattleTechResourceType.Prefab);
      MechRepresentation mechRep = defaultMechRepGO.GetComponent<MechRepresentation>();
      VehicleDrivenMechRepresentation custMechRep = defaultMechRepGO.GetComponent<VehicleDrivenMechRepresentation>();
      if (custMechRep == null) { custMechRep = defaultMechRepGO.AddComponent<VehicleDrivenMechRepresentation>(); custMechRep.Copy(mechRep); GameObject.DestroyImmediate(mechRep); mechRep = custMechRep; }
      defaultMechRepGO.SuppressMeshes(null);
      defaultMechRepGO.name = vRep.gameObject.name;
      vRep.Prepare();
      custMechRep.Copy(vRep);
      MechCustomization vehicleCust = vRep.gameObject.GetComponentInChildren<MechCustomization>(true);
      if (vehicleCust != null) { GameObject.DestroyImmediate(vehicleCust); }
      Log.TWL(0, "CustomMechRepresentation.InitFromVehicle");
      foreach(var blipName in BlipObjectNames) {
        Transform BlipObjectM = defaultMechRepGO.transform.FindRecursive(blipName.Key);
        Transform BlipObjectV = vRep.transform.FindRecursive(blipName.Key);
        Log.WL(1, "blip:" + blipName.Key + ":" + blipName.Value+" mech found "+(BlipObjectM!=null)+" vehcile found "+ (BlipObjectV!=null));
        Transform parent = defaultMechRepGO.transform;
        if (BlipObjectM != null) { parent = BlipObjectM.parent; }
        if ((BlipObjectM != null)&&(BlipObjectV != null)) {
          GameObject.DestroyImmediate(BlipObjectM.gameObject);
          Traverse.Create(custMechRep).Field<GameObject>(blipName.Key).Value = BlipObjectV.gameObject;
        }
        if (blipName.Value) {
          if (BlipObjectV != null) BlipObjectV.SetParent(parent);
        }
      }
      Transform visibleObject = vRep.VisibleObject.transform;
      if (visibleObject == null) { visibleObject = vRep.transform.FindRecursive("mesh"); }
      Transform target_visibleObject = custMechRep.VisibleObject.transform;
      if (target_visibleObject == null) { target_visibleObject = custMechRep.transform.FindRecursive("mesh"); }
      if ((visibleObject != null)&&(target_visibleObject != null)) {
        HashSet<Transform> visibleObjects = new HashSet<Transform>();
        Transform[] trs = visibleObject.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs) {
          if (tr.parent != visibleObject) { continue; }
          visibleObjects.Add(tr);
        }
        foreach (Transform tr in visibleObjects) {
          tr.SetParent(target_visibleObject);
        }
      }
      vRep.gameObject.name = "j_Body";
      GameObject vRepGO = vRep.gameObject;
      GameObject.DestroyImmediate(vRep);
      Transform j_Root = custMechRep.j_Root;
      vRepGO.transform.SetParent(j_Root);
      vRepGO.transform.localPosition = Vector3.zero;
      vRepGO.transform.localScale = Vector3.one;
      vRepGO.transform.localRotation = Quaternion.identity;
      custMechRep.vehicleRep = vRepGO.transform;
      Log.WL(1, "vRep now parent: "+ (custMechRep.vehicleRep.parent == null?"null": custMechRep.vehicleRep.name));
      return custMechRep;
    }
    public static GameObject ProcessBattle(DataManager dataManager,ref GameObject result, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "CustomActorRepresentationHelper.ProcessBattle custom representation found:" + custRepDef.Id);
      MechRepresentation mechRep = result.GetComponent<MechRepresentation>();
      CustomMechRepresentation custMechRep = null;
      if (mechRep == null) {
        Log.WL(1, "no MechRepresentation:" + result.name);
        VehicleRepresentation vehicleRep = result.GetComponent<VehicleRepresentation>();
        if (vehicleRep == null) {
          Log.WL(1, "no VehicleRepresentation:" + result.name);
          return result;
        }
        custMechRep = vehicleRep.InitFromVehicle(dataManager);
        result = custMechRep.gameObject;
      } else {
        custMechRep = result.GetComponent<CustomMechRepresentation>();
        if (custMechRep == null) { custMechRep = result.AddComponent<CustomMechRepresentation>(); custMechRep.Copy(mechRep); GameObject.DestroyImmediate(mechRep); mechRep = custMechRep; }
      }
      custMechRep.RegisterColliders(custMechRep.gameObject);
      MechCustomization mechCust = custMechRep.GetComponentInChildren<MechCustomization>(true);
      custMechRep.PrefabBase = chassisDef.PrefabBase;
      custMechRep.HardpointData = chassisDef.HardpointDataDef;
      custMechRep.chassisDef = chassisDef;
      CustomPropertyBlockManager custPropertyBlock = custMechRep.GetComponentInChildren<CustomPropertyBlockManager>();
      if(custPropertyBlock == null) {
        PropertyBlockManager propertyBlock = custMechRep.GetComponentInChildren<PropertyBlockManager>();
        if(propertyBlock != null) {
          custPropertyBlock = propertyBlock.gameObject.AddComponent<CustomPropertyBlockManager>();
          custPropertyBlock.Copy(propertyBlock);
          GameObject.DestroyImmediate(propertyBlock);
          propertyBlock = null;
          custMechRep._propertyBlock = custPropertyBlock;
          Log.WL(0, "customPropertyBlock " + (custMechRep.customPropertyBlock == null ? "null" : custMechRep.customPropertyBlock.gameObject.name));
        }
      }
      if (mechCust != null) {
        CustomMechCustomization custMechCust = custMechRep.VisibleObject.AddComponent<CustomMechCustomization>();
        custMechCust.Copy(mechCust);
        GameObject.DestroyImmediate(mechCust);
        custMechRep.defaultMechCustomization = custMechCust;
        custMechCust.Init(custMechRep);
      }
      switch (custRepDef.ApplyType) {
        //case CustomActorRepresentationDef.RepresentationApplyType.CopyMesh: mechRep.CopyModel(source, dataManager); break;
        case CustomActorRepresentationDef.RepresentationApplyType.MoveBone: custMechRep.MoveBone(custRepDef, dataManager); break;
        case CustomActorRepresentationDef.RepresentationApplyType.None: custMechRep.MoveNone(custRepDef, dataManager); break;
      }
      if (custMechRep.BlipObjectIdentified != null) { custMechRep.RegisterRenderersCustomHeraldry(custMechRep.BlipObjectIdentified, null); }
      if (custMechRep.BlipObjectUnknown != null) { custMechRep.RegisterRenderersCustomHeraldry(custMechRep.BlipObjectUnknown, null); }
      if (custMechRep.BlipObjectGhostStrong != null) { custMechRep.RegisterRenderersCustomHeraldry(custMechRep.BlipObjectGhostStrong, null); }
      if (custMechRep.BlipObjectGhostWeak != null) { custMechRep.RegisterRenderersCustomHeraldry(custMechRep.BlipObjectGhostWeak, null); }
      custMechRep.RegisterRenderersMainHeraldry(custMechRep.VisibleObject);
      CustomRepresentation customRepresentation = result.GetComponent<CustomRepresentation>();
      if (customRepresentation == null) { customRepresentation = result.AddComponent<CustomRepresentation>(); }
      customRepresentation.Init(custMechRep, custRepDef);
      customRepresentation.InBattle = true;
      custMechRep.customRep = customRepresentation;
      MechFlyHeightController heightController = result.GetComponent<MechFlyHeightController>();
      if (heightController == null) { heightController = result.AddComponent<MechFlyHeightController>(); };
      custMechRep.HeightController = heightController;
      custMechRep.Test();
      return result;
    }
    public static GameObject ProcessSimGame(DataManager dataManager, ref GameObject result, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "CustomActorRepresentationHelper.ProcessSimGame custom representation found:" + custRepDef.Id);
      try {
        MechRepresentationSimGame mechRep = result.gameObject.GetComponent<MechRepresentationSimGame>();
        if (mechRep == null) {
          Log.WL(1, "no MechRepresentationSimGame:" + result.name);
        } else {
          CustomMechRepresentationSimGame custMechRep = result.gameObject.GetComponent<CustomMechRepresentationSimGame>();
          if (custMechRep == null) {
            custMechRep = result.gameObject.AddComponent<CustomMechRepresentationSimGame>();
            custMechRep.CopyFrom(mechRep);
            custMechRep.chassisDef = chassisDef;
            GameObject.DestroyImmediate(mechRep);
            mechRep = custMechRep;
          }
          MechCustomization mechCust = custMechRep.mechCustomization;
          custMechRep.PrefabBase = chassisDef.PrefabBase;
          custMechRep.HardpointData = chassisDef.HardpointDataDef;
          CustomPropertyBlockManager custPropertyBlock = custMechRep.GetComponentInChildren<CustomPropertyBlockManager>();
          if (custPropertyBlock == null) {
            PropertyBlockManager propertyBlock = custMechRep.GetComponentInChildren<PropertyBlockManager>();
            if (propertyBlock != null) {
              custPropertyBlock = propertyBlock.gameObject.AddComponent<CustomPropertyBlockManager>();
              custPropertyBlock.Copy(propertyBlock);
              GameObject.DestroyImmediate(propertyBlock);
              propertyBlock = null;
              custMechRep.propertyBlock = custPropertyBlock;
              Log.WL(0, "customPropertyBlock " + (custMechRep.customPropertyBlock == null ? "null" : custMechRep.customPropertyBlock.gameObject.name));
            }
          }
          if (mechCust != null) {
            CustomMechCustomization custMechCust = custMechRep.VisibleObject.AddComponent<CustomMechCustomization>();
            custMechCust.Copy(mechCust);
            GameObject.DestroyImmediate(mechCust);
            custMechRep.defaultMechCustomization = custMechCust;
            custMechCust.Init(custMechRep);
          }
          switch (custRepDef.ApplyType) {
            //case CustomActorRepresentationDef.RepresentationApplyType.CopyMesh: custMechRep.CopyModel(custRepDef, dataManager); break;
            case CustomActorRepresentationDef.RepresentationApplyType.MoveBone: custMechRep.MoveBone(custRepDef, dataManager); break;
          }
          custMechRep.RegisterRenderersMainHeraldry(custMechRep.VisibleObject);
          CustomRepresentation customRepresentation = result.GetComponent<CustomRepresentation>();
          if (customRepresentation == null) { customRepresentation = result.AddComponent<CustomRepresentation>(); }
          customRepresentation.Init(mechRep, custRepDef);
          customRepresentation.InBattle = false;
          custMechRep.customRep = customRepresentation;
        }
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
      return result;
    }
    public static GameObject ProcessAlternatesBattle(DataManager dataManager, ref GameObject result, string id, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "ProsessAlternatesBattle:"+id);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      MechRepresentation mechRepresentation = result.GetComponent<MechRepresentation>();
      if (mechRepresentation == null) { return result; }
      AlternatesRepresentation altMechRepresentation = mechRepresentation as AlternatesRepresentation;
      if (altMechRepresentation == null) {
        altMechRepresentation = result.AddComponent<AlternatesRepresentation>();
        altMechRepresentation.Copy(mechRepresentation);
        GameObject.DestroyImmediate(mechRepresentation);
        mechRepresentation = null;
        CustomPropertyBlockManager custPropertyBlock = altMechRepresentation.GetComponentInChildren<CustomPropertyBlockManager>();
        if (custPropertyBlock == null) {
          PropertyBlockManager propertyBlock = altMechRepresentation.GetComponentInChildren<PropertyBlockManager>();
          if (propertyBlock != null) {
            custPropertyBlock = propertyBlock.gameObject.AddComponent<CustomPropertyBlockManager>();
            custPropertyBlock.Copy(propertyBlock);
            GameObject.DestroyImmediate(propertyBlock);
            propertyBlock = null;
            altMechRepresentation._propertyBlock = custPropertyBlock;
            Log.WL(0, "customPropertyBlock " + (altMechRepresentation.customPropertyBlock == null ? "null" : altMechRepresentation.customPropertyBlock.gameObject.name));
          }
        }
      }
      altMechRepresentation.altDef = new AlternateRepresentationDef();
      altMechRepresentation.chassisDef = chassisDef;
      altMechRepresentation.UnregisterRenderers(altMechRepresentation.VisibleObject);
      HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
      foreach (Transform child in altMechRepresentation.VisibleObject.transform) {
        objectsToDestroy.Add(child.gameObject);
      }
      MechCustomization mechCust = altMechRepresentation.GetComponentInChildren<MechCustomization>(true);
      if (mechCust != null) { GameObject.DestroyImmediate(mechCust); mechCust = null; }
      Transform j_Root = altMechRepresentation.transform.FindRecursive("j_Root");
      Log.WL(1, "j_Root:"+(j_Root == null?"null": j_Root.parent.name));
      //if (j_Root != null) { foreach (Transform child in j_Root) { objectsToDestroy.Add(child.gameObject); } }
      foreach (GameObject toDestroy in objectsToDestroy) { GameObject.DestroyImmediate(toDestroy); };
      objectsToDestroy.Clear();
      GameObject baseAlternateGO = dataManager.PooledInstantiate_CustomMechRep_Battle(id, chassisDef, false, false, true);
      baseAlternateGO.name = "baseFormSekeleton";
      baseAlternateGO.transform.SetParent(j_Root);
      baseAlternateGO.transform.localPosition = Vector3.zero;
      baseAlternateGO.transform.localRotation = Quaternion.identity;
      baseAlternateGO.transform.localScale = new Vector3(1f,1f,1f);
      CustomMechRepresentation baseAlternate = baseAlternateGO.GetComponent<CustomMechRepresentation>();
      altMechRepresentation.AddAlternate(baseAlternate);
      baseAlternate.altDef = new AlternateRepresentationDef(chassisDef);
      baseAlternate.RotateBody = baseAlternate.altDef.Type == AlternateRepType.AirMech;
      baseAlternate.PrefabBase = chassisDef.PrefabBase;
      baseAlternate.HardpointData = chassisDef.HardpointDataDef;
      baseAlternate.chassisDef = chassisDef;
      for (int i = 0; i < customInfo.AlternateRepresentations.Count; ++i) {
        string prefabId = customInfo.AlternateRepresentations[i].PrefabIdentifier;
        GameObject alternateGO = dataManager.PooledInstantiate_CustomMechRep_Battle(prefabId, chassisDef, false, false, true);
        alternateGO.name = "alternate"+i+"Skeleton";
        alternateGO.transform.SetParent(j_Root);
        alternateGO.transform.localPosition = Vector3.zero;
        alternateGO.transform.localRotation = Quaternion.identity;
        alternateGO.transform.localScale = new Vector3(1f, 1f, 1f);
        CustomMechRepresentation alternate = alternateGO.GetComponent<CustomMechRepresentation>();
        alternate.altDef = customInfo.AlternateRepresentations[i];
        alternate.RotateBody = alternate.altDef.Type == AlternateRepType.AirMech;
        alternate.AlternateIndex = i + 1;
        alternate.PrefabBase = customInfo.AlternateRepresentations[i].PrefabBase;
        alternate.HardpointData = dataManager.GetObjectOfType<HardpointDataDef>(customInfo.AlternateRepresentations[i].HardpointDataDef,BattleTechResourceType.HardpointDataDef);
        alternate.chassisDef = chassisDef;
        altMechRepresentation.AddAlternate(alternate);
      }
      return result;
    }
    public static GameObject ProcessSquadBattle(DataManager dataManager, ref GameObject result, string id, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "ProcessSquadBattle:" + id);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      MechRepresentation mechRepresentation = result.GetComponent<MechRepresentation>();
      if (mechRepresentation == null) { return result; }
      SquadRepresentation squadMechRepresentation = mechRepresentation as SquadRepresentation;
      if (squadMechRepresentation == null) {
        squadMechRepresentation = result.AddComponent<SquadRepresentation>();
        squadMechRepresentation.Copy(mechRepresentation);
        GameObject.DestroyImmediate(mechRepresentation);
        mechRepresentation = null;
        CustomPropertyBlockManager custPropertyBlock = squadMechRepresentation.GetComponentInChildren<CustomPropertyBlockManager>();
        if (custPropertyBlock == null) {
          PropertyBlockManager propertyBlock = squadMechRepresentation.GetComponentInChildren<PropertyBlockManager>();
          if (propertyBlock != null) {
            custPropertyBlock = propertyBlock.gameObject.AddComponent<CustomPropertyBlockManager>();
            custPropertyBlock.Copy(propertyBlock);
            GameObject.DestroyImmediate(propertyBlock);
            propertyBlock = null;
            squadMechRepresentation._propertyBlock = custPropertyBlock;
            Log.WL(0, "customPropertyBlock " + (squadMechRepresentation.customPropertyBlock == null ? "null" : squadMechRepresentation.customPropertyBlock.gameObject.name));
          }
        }
      }
      squadMechRepresentation.altDef = new AlternateRepresentationDef();
      squadMechRepresentation.UnregisterRenderers(squadMechRepresentation.VisibleObject);
      squadMechRepresentation.chassisDef = chassisDef;
      HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
      foreach (Transform child in squadMechRepresentation.VisibleObject.transform) {
        objectsToDestroy.Add(child.gameObject);
      }
      MechCustomization mechCust = squadMechRepresentation.GetComponentInChildren<MechCustomization>(true);
      if (mechCust != null) { GameObject.DestroyImmediate(mechCust); mechCust = null; }
      Transform j_Root = squadMechRepresentation.transform.FindRecursive("j_Root");
      Log.WL(1, "j_Root:" + (j_Root == null ? "null" : j_Root.parent.name));
      //if (j_Root != null) { foreach (Transform child in j_Root) { objectsToDestroy.Add(child.gameObject); } }
      foreach (GameObject toDestroy in objectsToDestroy) { GameObject.DestroyImmediate(toDestroy); };
      objectsToDestroy.Clear();
      string squadRepId = id;
      if(custRepDef != null) {
        if (string.IsNullOrEmpty(custRepDef.Id) == false) { squadRepId = custRepDef.Id; }
      }
      for (int index = 0; index < customInfo.SquadInfo.Troopers; ++index) {
        GameObject unitGO = dataManager.PooledInstantiate_CustomMechRep_Battle(squadRepId, chassisDef, true, false, true);
        unitGO.name = "unit_" + index + "_skeleton";
        unitGO.transform.SetParent(j_Root);
        unitGO.transform.localPosition = Vector3.zero;
        unitGO.transform.localRotation = Quaternion.identity;
        unitGO.transform.localScale = new Vector3(1f, 1f, 1f);
        //Transform unit_j_Root = unitGO.transform.FindRecursive("j_Root");
        //if (unit_j_Root == null) { unit_j_Root = unitGO.transform; }
        Vector3 unitScale = new Vector3(customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize);
        CustomMechRepresentation unitRep = unitGO.GetComponent<CustomMechRepresentation>();
        if (unitRep != null) {
          unitRep.ApplyScale(unitScale);
          squadMechRepresentation.AddUnit(unitRep);
          unitRep.chassisDef = chassisDef;
          unitRep.HardpointData = chassisDef.HardpointDataDef;
        }
      }
      return result;
    }
    public static GameObject ProcessSquadSimGame(DataManager dataManager, ref GameObject result, string id, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "ProcessSquadSimGame:" + id);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      MechRepresentationSimGame mechRepresentation = result.GetComponent<MechRepresentationSimGame>();
      if (mechRepresentation == null) { return result; }
      SquadRepresentationSimGame squadMechRepresentation = mechRepresentation as SquadRepresentationSimGame;
      if (squadMechRepresentation != null) { return result; }
      squadMechRepresentation = result.AddComponent<SquadRepresentationSimGame>();
      squadMechRepresentation.CopyFrom(mechRepresentation);
      GameObject.DestroyImmediate(mechRepresentation);
      mechRepresentation = null;
      CustomPropertyBlockManager custPropertyBlock = squadMechRepresentation.GetComponentInChildren<CustomPropertyBlockManager>();
      if (custPropertyBlock == null) {
        PropertyBlockManager propertyBlock = squadMechRepresentation.GetComponentInChildren<PropertyBlockManager>();
        if (propertyBlock != null) {
          custPropertyBlock = propertyBlock.gameObject.AddComponent<CustomPropertyBlockManager>();
          custPropertyBlock.Copy(propertyBlock);
          GameObject.DestroyImmediate(propertyBlock);
          propertyBlock = null;
          squadMechRepresentation.propertyBlock = custPropertyBlock;
          Log.WL(0, "customPropertyBlock " + (squadMechRepresentation.customPropertyBlock == null ? "null" : squadMechRepresentation.customPropertyBlock.gameObject.name));
        }
      }
      squadMechRepresentation.UnregisterRenderers(squadMechRepresentation.VisibleObject);
      squadMechRepresentation.chassisDef = chassisDef;
      HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
      foreach (Transform child in squadMechRepresentation.VisibleObject.transform) {
        objectsToDestroy.Add(child.gameObject);
      }
      MechCustomization mechCust = squadMechRepresentation.GetComponentInChildren<MechCustomization>(true);
      if (mechCust != null) { GameObject.DestroyImmediate(mechCust); mechCust = null; }
      Transform j_Root = squadMechRepresentation.transform.FindRecursive("j_Root");
      Log.WL(1, "j_Root:" + (j_Root == null ? "null" : j_Root.parent.name));
      //if (j_Root != null) { foreach (Transform child in j_Root) { objectsToDestroy.Add(child.gameObject); } }
      foreach (GameObject toDestroy in objectsToDestroy) { GameObject.DestroyImmediate(toDestroy); };
      objectsToDestroy.Clear();
      string squadRepId = id;
      if (custRepDef != null) {
        if (string.IsNullOrEmpty(custRepDef.PrefabBase) == false) { squadRepId = string.Format("chrPrfComp_{0}_simgame", custRepDef.PrefabBase); }
      }
      for (int index = 0; index < customInfo.SquadInfo.Troopers; ++index) {
        GameObject unitGO = dataManager.PooledInstantiate_CustomMechRep_MechLab(squadRepId, chassisDef, false, true);
        unitGO.name = "unit_" + index + "_skeleton";
        unitGO.transform.SetParent(j_Root);
        unitGO.transform.localPosition = Vector3.zero;
        unitGO.transform.localRotation = Quaternion.identity;
        unitGO.transform.localScale = new Vector3(1f, 1f, 1f);
        Transform unit_j_Root = unitGO.transform.FindRecursive("j_Root");
        if (unit_j_Root == null) { unit_j_Root = unitGO.transform; }
        unit_j_Root.localScale = new Vector3(customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize);
        CustomMechRepresentationSimGame unitRep = unitGO.GetComponent<CustomMechRepresentationSimGame>();
        if (unitRep != null) {
          squadMechRepresentation.AddUnit(unitRep);
          unitRep.chassisDef = chassisDef;
          unitRep.HardpointData = chassisDef.HardpointDataDef;
        }
      }
      return result;
    }
    public static GameObject ProcessQuadBattle(DataManager dataManager, ref GameObject result, string id, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "ProcessQuadBattle:" + id);
      result = CustomActorRepresentationHelper.ProcessBattle(dataManager, ref result, custRepDef, chassisDef);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      CustomMechRepresentation custMechRepresentation = result.GetComponent<CustomMechRepresentation>();
      if(custMechRepresentation == null) {
        return result;
      }
      QuadRepresentation quadMechRepresentation = custMechRepresentation as QuadRepresentation;
      if (quadMechRepresentation == null) {
        quadMechRepresentation = result.AddComponent<QuadRepresentation>();
        quadMechRepresentation.Copy(custMechRepresentation);
        quadMechRepresentation.defaultMechCustomization = custMechRepresentation.defaultMechCustomization;
        quadMechRepresentation._propertyBlock = custMechRepresentation.customPropertyBlock;
        quadMechRepresentation.HardpointData = custMechRepresentation.HardpointData;
        quadMechRepresentation.PrefabBase = custMechRepresentation.PrefabBase;
        quadMechRepresentation.quadVisualInfo = custRepDef.quadVisualInfo;
        quadMechRepresentation.defaultMechCustomization.parent = quadMechRepresentation;
        quadMechRepresentation.customRep = custMechRepresentation.customRep;
        quadMechRepresentation.customRep.GameRepresentation = quadMechRepresentation;
        GameObject.DestroyImmediate(custMechRepresentation);
        custMechRepresentation = null;

        quadMechRepresentation.altDef = new AlternateRepresentationDef(chassisDef);
        quadMechRepresentation.chassisDef = chassisDef;
        quadMechRepresentation.UnregisterRenderers(quadMechRepresentation.VisibleObject);
        HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
        foreach (Transform child in quadMechRepresentation.VisibleObject.transform) {
          objectsToDestroy.Add(child.gameObject);
        }
        CustomMechMeshMerge merge = quadMechRepresentation.GetComponent<CustomMechMeshMerge>();
        if (merge != null) { GameObject.DestroyImmediate(merge); merge = null; }
        Transform j_Root = quadMechRepresentation.transform.FindRecursive("j_Root");
        Log.WL(1, "j_Root:" + (j_Root == null ? "null" : j_Root.parent.name));
        foreach (GameObject toDestroy in objectsToDestroy) { GameObject.DestroyImmediate(toDestroy); };
        objectsToDestroy.Clear();
        GameObject bodyGO = dataManager.PooledInstantiate(custRepDef.quadVisualInfo.BodyPrefab, BattleTechResourceType.Prefab);
        if(bodyGO != null) {
          quadMechRepresentation.AddBody(bodyGO, dataManager);
          GameObject.DestroyImmediate(bodyGO);
        }
        GameObject frontLegsGO = dataManager.PooledInstantiate_CustomMechRep_Battle(custRepDef.quadVisualInfo.FLegsPrefab, chassisDef, false, false, false);
        CustomMechRepresentation frontLegs = frontLegsGO.GetComponent<CustomMechRepresentation>();
        quadMechRepresentation.VisualObjects.Add(frontLegs.VisibleObject);
        GameObject rearLegsGO = dataManager.PooledInstantiate_CustomMechRep_Battle(custRepDef.quadVisualInfo.RLegsPrefab, chassisDef, false, false, false);
        CustomMechRepresentation rearLegs = rearLegsGO.GetComponent<CustomMechRepresentation>();
        quadMechRepresentation.VisualObjects.Add(rearLegs.VisibleObject);

        if (frontLegs != null) {
          quadMechRepresentation.AddForwardLegs(frontLegs);
          frontLegs.chassisDef = chassisDef;
        }
        if (rearLegs != null) {
          quadMechRepresentation.AddRearLegs(rearLegs);
          rearLegs.chassisDef = chassisDef;
        }
      }
      return result;
    }
    public static GameObject ProcessQuadSimGame(DataManager dataManager, ref GameObject result, string id, CustomActorRepresentationDef custRepDef, ChassisDef chassisDef) {
      Log.TWL(0, "ProcessQuadSimGame:" + id);
      result = CustomActorRepresentationHelper.ProcessSimGame(dataManager, ref result, custRepDef, chassisDef);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      CustomMechRepresentationSimGame custMechRepresentation = result.GetComponent<CustomMechRepresentationSimGame>();
      if (custMechRepresentation == null) {
        return result;
      }
      QuadRepresentationSimGame quadMechRepresentation = custMechRepresentation as QuadRepresentationSimGame;
      if (quadMechRepresentation == null) {
        quadMechRepresentation = result.AddComponent<QuadRepresentationSimGame>();
        quadMechRepresentation.CopyFrom(custMechRepresentation);
        quadMechRepresentation.defaultMechCustomization = custMechRepresentation.defaultMechCustomization;
        quadMechRepresentation.propertyBlock = custMechRepresentation.customPropertyBlock;
        quadMechRepresentation.HardpointData = custMechRepresentation.HardpointData;
        quadMechRepresentation.PrefabBase = custMechRepresentation.PrefabBase;
        quadMechRepresentation.quadVisualInfo = custRepDef.quadVisualInfo;
        quadMechRepresentation.defaultMechCustomization.parent = quadMechRepresentation;
        quadMechRepresentation.customRep = custMechRepresentation.customRep;
        quadMechRepresentation.customRep.SimGameRepresentation = quadMechRepresentation;
        GameObject.DestroyImmediate(custMechRepresentation);
        custMechRepresentation = null;

        quadMechRepresentation.chassisDef = chassisDef;
        quadMechRepresentation.UnregisterRenderers(quadMechRepresentation.VisibleObject);
        HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();
        foreach (Transform child in quadMechRepresentation.VisibleObject.transform) {
          objectsToDestroy.Add(child.gameObject);
        }
        Transform j_Root = quadMechRepresentation.transform.FindRecursive("j_Root");
        Log.WL(1, "j_Root:" + (j_Root == null ? "null" : j_Root.parent.name));
        foreach (GameObject toDestroy in objectsToDestroy) { GameObject.DestroyImmediate(toDestroy); };
        objectsToDestroy.Clear();
        GameObject bodyGO = dataManager.PooledInstantiate(custRepDef.quadVisualInfo.BodyPrefab, BattleTechResourceType.Prefab);
        if (bodyGO != null) {
          quadMechRepresentation.AddBody(bodyGO, dataManager);
          GameObject.DestroyImmediate(bodyGO);
        }
        GameObject frontLegsGO = dataManager.PooledInstantiate_CustomMechRep_MechLab(custRepDef.quadVisualInfo.FLegsPrefabBase.GetSimGamePrefabName(), chassisDef, false, false);
        CustomMechRepresentationSimGame frontLegs = frontLegsGO.GetComponent<CustomMechRepresentationSimGame>();
        quadMechRepresentation.VisualObjects.Add(frontLegs.VisibleObject);
        GameObject rearLegsGO = dataManager.PooledInstantiate_CustomMechRep_MechLab(custRepDef.quadVisualInfo.RLegsPrefabBase.GetSimGamePrefabName(), chassisDef, false, false);
        CustomMechRepresentationSimGame rearLegs = rearLegsGO.GetComponent<CustomMechRepresentationSimGame>();
        quadMechRepresentation.VisualObjects.Add(rearLegs.VisibleObject);

        if (frontLegs != null) {
          quadMechRepresentation.AddForwardLegs(frontLegs);
          frontLegs.chassisDef = chassisDef;
          frontLegs.RegisterRenderersMainHeraldry(frontLegs.VisibleObject);
          frontLegs.customPropertyBlock._UpdateCache();
        }
        if (rearLegs != null) {
          quadMechRepresentation.AddRearLegs(rearLegs);
          rearLegs.chassisDef = chassisDef;
          rearLegs.RegisterRenderersMainHeraldry(rearLegs.VisibleObject);
          rearLegs.customPropertyBlock._UpdateCache();
        }
      }
      return result;
    }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("childrenRenderers")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class MechMeshMerge_childrenRenderers {
    public static bool Prefix(MechMeshMerge __instance, ref bool __state, ref SkinnedMeshRenderer[] ____childrenRenderers,ref GameObject ___visibleObject, ref SkinnedMeshRenderer[] __result) {
      if (____childrenRenderers != null) {
        __result = ____childrenRenderers; return false;
      }
      List<SkinnedMeshRenderer> result = new List<SkinnedMeshRenderer>();
      SkinnedMeshRenderer[] list = ___visibleObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      Log.TWL(0, "MechMeshMerge.childrenRenderers populating");
      foreach(SkinnedMeshRenderer renderer in list) {
        if (renderer.name.StartsWith("DESTROYING_")) { continue; }
        result.Add(renderer);
        Log.WL(1, renderer.name);
      }
      ____childrenRenderers = result.ToArray();
      __result = ____childrenRenderers;
      return false;
    }
  }
  public static class MechCustomRepresentationHelper {
    //private static MethodInfo MechComponent_componentRep_set = null;
    public static void componentRep_set(this MechComponent cmp, ComponentRepresentation rep) {
      Log.TWL(0, $"MechComponent.componentRep set {cmp.parent.PilotableActorDef.ChassisID}:{cmp.baseComponentRef.ComponentDefID} {rep.transform.name}");
      ComponentRepresentation prevRep = cmp.componentRep;
      if (prevRep == rep) {
        Log.WL(1,"no change");
        return;
      }
      Traverse.Create(cmp).Property<ComponentRepresentation>("componentRep").Value = rep;
      WeaponRepresentation prevWpnRep = prevRep as WeaponRepresentation;
      WeaponRepresentation wpnRep = rep as WeaponRepresentation;
      if((prevWpnRep != null)&&(wpnRep != null)) {
        WeaponEffect[] weaponEffects = prevWpnRep.gameObject.GetComponentsInChildren<WeaponEffect>(true);
        foreach (WeaponEffect weaponEffect in weaponEffects) {
          if (weaponEffect.transform.parent != prevWpnRep.transform) { continue; }
          Log.WL(1, $"move weapon effect {weaponEffect.transform.name}");
          weaponEffect.gameObject.transform.parent = wpnRep.transform;
          weaponEffect.gameObject.transform.localPosition = Vector3.zero;
          weaponEffect.gameObject.transform.rotation = Quaternion.identity;
          weaponEffect.weaponRep = wpnRep;
          Traverse.Create(weaponEffect).Field<int>("numberOfEmitters").Value = wpnRep.vfxTransforms.Length;
        }
      }
      //if (MechComponent_componentRep_set == null) { MechComponent_componentRep_set = typeof(MechComponent).GetProperty("componentRep").GetSetMethod(true); }
      //MechComponent_componentRep_set.Invoke(cmp, new object[] { rep });
    }
    public static GameObject PooledInstantiate_CustomMechRep_MechLab(this DataManager dataManager, string id, ChassisDef chassisDef, bool squadProcess, bool quadProcess) {
      string origId = id;
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.FindSimGame(origId);
      Log.TWL(0, "DataManager.PooledInstantiate_CustomMechRep_MechLab " +id + " custRepDef:" + (custRepDef == null?"null":"not null"));
      if (custRepDef == null) {
        custRepDef = CustomActorRepresentationHelper.defaultCustomMechRepDef;
      } else {
        //id = custRepDef.SourcePrefabIdentifier;
        id = origId.Replace(custRepDef.PrefabBase, custRepDef.SourcePrefabBase);
      }
      GameObject result = dataManager.PooledInstantiate(id, BattleTechResourceType.Prefab);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      if (result != null) {
        bool setupAlternates = false;
        bool setupSquad = false;
        bool setupQuad = false;
        if (squadProcess) if (customInfo != null) { if (customInfo.SquadInfo.Troopers > 1) { setupSquad = true; } }
        if (quadProcess) if (customInfo != null) { if (custRepDef.quadVisualInfo.UseQuadVisuals) { setupQuad = true; } }
        Log.WL(1, "setupAlternates:" + setupAlternates);
        Log.WL(1, "setupSquad:" + setupSquad);
        Log.WL(1, "setupQuad:" + setupQuad);
        if (setupSquad) {
          return CustomActorRepresentationHelper.ProcessSquadSimGame(dataManager, ref result, id, custRepDef, chassisDef);
          //return CustomActorRepresentationHelper.ProcessSimGame(dataManager, ref result, custRepDef, chassisDef);
        } else {
          if (setupQuad) {
            return CustomActorRepresentationHelper.ProcessQuadSimGame(dataManager, ref result, id, custRepDef, chassisDef);
            //return CustomActorRepresentationHelper.ProcessSimGame(dataManager, ref result, custRepDef, chassisDef);
          } else {
            return CustomActorRepresentationHelper.ProcessSimGame(dataManager, ref result, custRepDef, chassisDef);
          }
        }
      }
      return result;
    }
    public static GameObject PooledInstantiate_CustomMechRep_Battle(this DataManager dataManager, string id, ChassisDef chassisDef, bool altProcess, bool squadProcess, bool quadProcess) {
      string origId = id;
      CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(origId);
      if (custRepDef == null) {
        custRepDef = CustomActorRepresentationHelper.defaultCustomMechRepDef;
      } else {
        id = custRepDef.SourcePrefabIdentifier;
      }
      Log.TWL(0, "DataManager.PooledInstantiateCustomRep " + origId+"->"+id);
      GameObject result = dataManager.PooledInstantiate(id, BattleTechResourceType.Prefab);
      UnitCustomInfo customInfo = chassisDef.GetCustomInfo();
      if (result != null) {
        result.TestMissingMaterials(id, dataManager);
        bool setupAlternates = false;
        bool setupSquad = false;
        bool setupQuad = false;
        if(altProcess) if (customInfo != null) { if (customInfo.AlternateRepresentations.Count > 0) { setupAlternates = true; } }
        if (squadProcess) if (customInfo != null) { if (customInfo.SquadInfo.Troopers > 1) { setupSquad = true; } }
        if (quadProcess) if (customInfo != null) { if (custRepDef.quadVisualInfo.UseQuadVisuals) { setupQuad = true; } }
        Log.WL(1, "setupAlternates:"+ setupAlternates);
        Log.WL(1, "setupSquad:" + setupSquad);
        Log.WL(1, "setupQuad:" + setupQuad);
        if (setupSquad) {
          return CustomActorRepresentationHelper.ProcessSquadBattle(dataManager, ref result, id, custRepDef, chassisDef);
        } else {
          if (setupAlternates) {
            return CustomActorRepresentationHelper.ProcessAlternatesBattle(dataManager, ref result, id, custRepDef, chassisDef);
          } else {
            if (setupQuad) {
              return CustomActorRepresentationHelper.ProcessQuadBattle(dataManager, ref result, id, custRepDef, chassisDef);
            } else {
              return CustomActorRepresentationHelper.ProcessBattle(dataManager, ref result, custRepDef, chassisDef);
            }
          }
        }
      }
      return result;
    }
    public static void TestMissingMaterials(this GameObject prefab, string id, DataManager dataManager) {
      Log.TWL(0, "TestMissingMaterials:"+id);
      HashSet<Renderer> renderers = new HashSet<Renderer>();
      foreach(SkinnedMeshRenderer renderer in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
        renderers.Add(renderer);
      }
      foreach (MeshRenderer renderer in prefab.GetComponentsInChildren<MeshRenderer>(true)) {
        if (renderer.gameObject.name.Contains("camoholder")) { continue; }
        renderers.Add(renderer);
      }
      foreach (Renderer renderer in renderers) {
        if (renderer.sharedMaterial == null) {
          Log.WL(1, renderer.gameObject.name+" has missing material");
        }
      }
    }
  }
  [HarmonyPatch(typeof(Renderer))]
  [HarmonyPatch("sharedMaterial")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(Material) })]
  public static class Renderer_sharedMaterial_set {
    public static void Prefix(Renderer __instance, Material value) {
      try {
        if (value == null) {
          Log.TWL(0, "Renderer.sharedMaterial set null "+ __instance.gameObject.name);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }

}