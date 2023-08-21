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
using CustAmmoCategories;
using HarmonyLib;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using HBS;
using MessagePack;

namespace CustomUnits {
  public enum AlternateRepType { Normal, AirMech };
  [MessagePackObject]
  public class AirMechVerticalJetsDef {
    [Key(0)]
    public string Prefab { get; set; }
    [Key(1)]
    public string Attach { get; set; }
    [Key(2)]
    public List<string> JetsAttachPoints { get; set; }
    public AirMechVerticalJetsDef() {
      JetsAttachPoints = new List<string>();
    }
  }
  [MessagePackObject]
  public class AlternateRepresentationDef {
    [Key(0)]
    public string PrefabIdentifier { get; set; } = string.Empty;
    [Key(1)]
    public string HardpointDataDef { get; set; } = string.Empty;
    [Key(2)]
    public List<string> AdditionalPrefabs { get; set; } = new List<string>();
    [Key(3)]
    public string PrefabBase { get; set; } = string.Empty;
    [Key(4)]
    public string HoveringSoundStart { get; set; } = string.Empty;
    [Key(5)]
    public string HoveringSoundEnd { get; set; } = string.Empty;
    [Key(6)]
    public string TransformationSound { get; set; } = string.Empty;
    [Key(7)]
    public string MoveStartSound { get; set; } = string.Empty;
    [Key(8)]
    public string MoveStopSound { get; set; } = string.Empty;
    [Key(9)]
    public float MoveClamp { get; set; } = 0f;
    [Key(10)]
    public bool NoJumpjetsBlock { get; set; } = false;
    [Key(11)]
    public float MinJumpDistance { get; set; } = 1f;
    [Key(12)]
    public List<string> additionalEncounterTags { get; set; } = new List<string>();
    [Key(13)]
    public AlternateRepType Type { get; set; } = AlternateRepType.Normal;
    [Key(14)]
    public float FlyHeight { get; set; } = 0f;
    [Key(15)]
    public List<AirMechVerticalJetsDef> AirMechVerticalJets { get; set; } = new List<AirMechVerticalJetsDef>();
    [Key(16)]
    public string SwitchInAudio { get; set; } = string.Empty;
    [Key(17)]
    public string SwitchOutAudio { get; set; } = string.Empty;
    public AlternateRepresentationDef() {
      //AdditionalPrefabs = new List<string>();
      //AirMechVerticalJets = new List<AirMechVerticalJetsDef>();
      //FlyHeight = 0f;
      //Type = AlternateRepType.Normal;
      //HoveringSoundStart = "jet_start";
      //HoveringSoundEnd = "jet_end";
      //TransformationSound = string.Empty;
      //MoveStartSound = string.Empty;
      //MoveStopSound = string.Empty;
      //additionalEncounterTags = new List<string>();
      //MoveClamp = 0f;
      //NoJumpjetsBlock = false;
      //MinJumpDistance = 1f;
    }
    public AlternateRepresentationDef(ChassisDef def) {
      UnitCustomInfo info = def.GetCustomInfo();
      AdditionalPrefabs = new List<string>();
      AirMechVerticalJets = new List<AirMechVerticalJetsDef>();
      PrefabIdentifier = def.PrefabIdentifier;
      PrefabBase = def.PrefabBase;
      HardpointDataDef = def.HardpointDataDefID;
      Type = AlternateRepType.Normal;
      if (info != null) { if (info.Unaffected.Pathing) { this.Type = AlternateRepType.AirMech; } }
      FlyHeight = info!=null? info.FlyingHeight: 0f;
      HoveringSoundStart = string.Empty;
      HoveringSoundEnd = string.Empty;
      TransformationSound = string.Empty;
      MoveStartSound = string.Empty;
      MoveStopSound = string.Empty;
      additionalEncounterTags = new List<string>();
      MoveClamp = info == null ? 0f : info.Unaffected.MoveClamp;
      MinJumpDistance = info == null ? 1f : info.Unaffected.MinJumpDistance;
    }
  }
  public class AirMechRepresentationData {
    public float Height { get; set; }
  }
  public class HasPrefabData {
    public string prefabName { get; set; }
    public bool hasPrefabName { get; set; }
    public HasPrefabData(string prefabName, bool hasPrefabName) {
      this.prefabName = prefabName; this.hasPrefabName = hasPrefabName;
    }
  };
  public enum AltRepState { Grounded, Starting, Falling, Flying, Standing };
  public enum AltRepMeleeState { Idle, MeleeAnim, ToHeight, FromHeight };
  public class AltRepInvokeDelegate {
    public object obj;
    public List<object> args;
    public MethodInfo method;
    public void Invoke() {
      Log.Combat?.TW(0, "AltRepInvokeDelegate.Invoke " + obj.GetType().ToString() + "." + method.Name + " args:" + args.Count);
      foreach (object arg in args) { Log.Combat?.W(1, arg.GetType().ToString() + ":" + arg.ToString()); }
      Log.Combat?.WL(0, "");
      try {
        method.Invoke(obj, args.ToArray());
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
      }
    }
    public AltRepInvokeDelegate(object trg, MethodInfo m, object[] a) {
      this.obj = trg;
      this.method = m;
      this.args = new List<object>();
      this.args.AddRange(a);
    }
  }
  public class AlternateMechRepresentation : MonoBehaviour {
    private static GameObject JumpJetSrcPrefab = null;
    private static Transform JumpJetSrc = null;
    public MechRepresentation mechRep { get; set; }
    public AltRepState state { get; set; }
    public AlternateMechRepresentations parent { get; set; }
    public Dictionary<MechComponent, ComponentRepresentation> components { get; set; }
    public AlternateRepresentationDef def { get; set; }
    public float DefaultFlyHeight { get; set; }
    public List<JumpjetRepresentation> verticalJets { get; set; }
    public List<GameObject> verticalJetsObjects { get; set; }
    public bool isVisible { get; set; }
    public void HandleDeath(DeathMethod deathMethod, int location) {
      if ((UnityEngine.Object)this.mechRep.VisibleObjectLight != (UnityEngine.Object)null) {
        this.mechRep.VisibleObjectLight.SetActive(false);
      }
      this.mechRep.thisAnimator.SetTrigger("Death");
      this.mechRep.OnDeath();
      List<string> stringList = new List<string>((IEnumerable<string>)this.mechRep.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index)
        this.mechRep.StopManualPersistentVFX(stringList[index]);
      this.mechRep.IsDead = true;
      this.mechRep.ToggleHeadlights(false);
    }
    public AlternateMechRepresentation() {
      components = new Dictionary<MechComponent, ComponentRepresentation>();
      rate = 0.25f;
      endAnimationQueue = new Queue<AltRepInvokeDelegate>();
      verticalJets = new List<JumpjetRepresentation>();
      verticalJetsObjects = new List<GameObject>();
      meleeState = AltRepMeleeState.Idle;
      meleeRate = 1f;
      DefaultFlyHeight = 0f;
    }
    public bool Inited { get; set; }
    public bool isBasic { get; set; }
    public bool IsPlayingJumpSound {
      get {
        return this.mechRep.isPlayingJumpSound();
      }
      set {
        this.mechRep.isPlayingJumpSound(value);
      }
    }
    public bool isJumping {
      get {
        return this.mechRep.isJumping();
      }
      set {
        this.mechRep.isJumping(value);
      }
    }
    public bool isRepEnabled {
      get {
        return this.parent.mechReps[this.parent.CurrentRep] == this;
      }
    }
    private float t;
    private float rate;
    private Transform j_Root;
    private Queue<AltRepInvokeDelegate> endAnimationQueue;
    public void pushQueue(object trg, MethodInfo method, object[] args) {
      endAnimationQueue.Enqueue(new AltRepInvokeDelegate(trg, method, args));
    }
    private bool isJumpjetsActive;
    public void StartMovement() {
      Log.Combat?.TWL(0, "AlternateMechRepresentation.StartMovement " + this.gameObject.name);
      isMoving = true;
      if (string.IsNullOrEmpty(this.def.MoveStartSound) == false) {
        uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(this.def.MoveStartSound, this.parent.parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
        Log.Combat?.TWL(0, "StartMovement by id (" + def.MoveStartSound + "):" + soundid);
      }
    }
    public void EndMovement() {
      Log.Combat?.TWL(0, "AlternateMechRepresentation.EndMovement "+this.gameObject.name);
      isMoving = false;
      if (string.IsNullOrEmpty(this.def.MoveStopSound) == false) {
        uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(this.def.MoveStopSound, this.parent.parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
        Log.Combat?.TWL(0, "EndMovement by id (" + def.MoveStopSound + "):" + soundid);
      }
    }
    public void PlayTransformSound() {
      if (string.IsNullOrEmpty(this.def.TransformationSound) == false) {
        uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(this.def.TransformationSound, this.parent.parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
        Log.Combat?.TWL(0, "PlayTransformSound by id (" + def.TransformationSound + "):" + soundid);
      }
    }
    public void UpdateFlyingAnim(Vector3 forward) {
      Vector3 normalized = forward.normalized;
      //this.mechRep.thisAnimator.SetFloat("InAir_Forward", normalized.x);
      //this.mechRep.thisAnimator.SetFloat("InAir_Side", normalized.y);
    }
    public void HeightChangeState() {
      mechRep.thisAnimator.SetTrigger("Jump");
      mechRep.thisAnimator.ResetTrigger("Land");
    }
    public void HeightSteadyState() {
      mechRep.thisAnimator.SetTrigger("Land");
      mechRep.thisAnimator.ResetTrigger("Jump");
    }
    public void StartHoverAudio() {
      if (string.IsNullOrEmpty(def.HoveringSoundStart) == false) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(def.HoveringSoundStart, this.parent.parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.Combat?.TWL(0, "StartHoverAudio by id (" + def.HoveringSoundStart + "):" + soundid);
        } else {
          Log.Combat?.TWL(0, "StartHoverAudio Can't play");
        }
      }

    }
    public void StopHoverAudio() {
      if (string.IsNullOrEmpty(def.HoveringSoundEnd) == false) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(def.HoveringSoundEnd, this.parent.parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.Combat?.TWL(0, "StopHoverAudio by id (" + def.HoveringSoundEnd + "):" + soundid);
        } else {
          Log.Combat?.TWL(0, "StopHoverAudio Can't play");
        }
      }

    }

    public void PlayStartAnimation() {
      Log.Combat?.TWL(0, "AlternateMechRepresentation.PlayStartAnimation " + this.gameObject.name);
      HeightChangeState();
      if (DefaultFlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
        mechRep.SetMeleeIdleState(false);
        if (this.isVisible) {
          foreach (GameObject jet in verticalJetsObjects) { jet.SetActive(true); }
          foreach (JumpjetRepresentation jet in verticalJets) { jet.SetState(JumpjetRepresentation.JumpjetState.Launching); }
        }
        this.isJumping = true;
        mechRep.StartJumpjetAudio();
        mechRep.PlayVFXAt((Transform)null, mechRep.parentActor.CurrentPosition, (string)mechRep.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
      }
    }
    public void PlayLandAnimation() {
      this.isJumping = false;
      HeightSteadyState();
      if (DefaultFlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
        mechRep.StopJumpjetAudio();
        foreach (GameObject jet in verticalJetsObjects) { jet.SetActive(false); }
      }
    }
    public void Starting() {
      t = 0f;
      rate = DefaultFlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets?0.25f:2f;
      this.state = AltRepState.Starting;
      isJumpjetsActive = true;
      PlayStartAnimation();
      this.parent.parentMech.BlockComponentsActivation(true);
    }
    public void Landing() {
      t = 0f;
      rate = 1f;
      HeightChangeState();
      this.state = AltRepState.Falling;
      isJumpjetsActive = false;
      this.parent.parentMech.BlockComponentsActivation(true);
    }
    public void Standing() {
      t = 0f;
      rate = 0.25f;
      this.state = AltRepState.Standing;
      isJumpjetsActive = false;
      this.parent.parentMech.BlockComponentsActivation(true);
    }
    public void Flying() {
      if (DefaultFlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
        foreach (JumpjetRepresentation jet in verticalJets) { jet.SetState(JumpjetRepresentation.JumpjetState.Flight); }
      }
      if (meleeState != AltRepMeleeState.Idle) {
        if (meleeT >= 1f) {
          if(meleeState == AltRepMeleeState.MeleeAnim) {
            meleeT = 0f;
            meleeRate = 2f;
            meleeState = AltRepMeleeState.FromHeight;
          } else {
            meleeState = AltRepMeleeState.Idle;
            meleeT = 0f;
            meleeRate = 1f;
          }
        } else {
          if (meleeState == AltRepMeleeState.ToHeight) {
            j_Root.localPosition = Vector3.up * Mathf.Lerp(this.DefaultFlyHeight, targetHeight, meleeT);
          } else {
            meleeT += meleeRate * Time.deltaTime;
            if (meleeState == AltRepMeleeState.FromHeight) {
              j_Root.localPosition = Vector3.up * Mathf.Lerp(targetHeight, this.DefaultFlyHeight, meleeT);
            }
          }
        }
      }
    }
    public void updateMeleeT(float mt) {
      if (meleeState == AltRepMeleeState.ToHeight) {
        this.meleeT = mt;
      }
    }
    public void updateMeleeIdle() {
      meleeState = AltRepMeleeState.Idle;
    }
    public void StopJumpjets() {
      foreach (JumpjetRepresentation jet in verticalJets) { jet.SetState(JumpjetRepresentation.JumpjetState.Landing); }
      isJumpjetsActive = false;
    }
    public void Land() {
      PlayLandAnimation();
      isJumpjetsActive = false;
      if (def.FlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
        DynamicMapHelper.calculateJumpDamage(this.parent.parentMech, this.parent.parentMech.CurrentPosition);
        if (DynamicMapHelper.registredMineFieldDamage.ContainsKey(this.parent.parentMech) == false) { this.parent.parentMech.Combat.HandleSanitize(); return; };
        MineFieldDamage mfDamage = DynamicMapHelper.registredMineFieldDamage[this.parent.parentMech];
        mfDamage.resolveMineFiledDamage(this.parent.parentMech, -1);
        DynamicMapHelper.registredMineFieldDamage.Remove(this.parent.parentMech);
        this.parent.parentMech.Combat.HandleSanitize(true, true);
      }
    }
    private AltRepMeleeState meleeState;
    private float meleeT;
    private float targetHeight;
    private float meleeRate;
    public void initTargetHeight(float height) {
      targetHeight = height;
      meleeT = 0f;
      meleeRate = 1f;
      meleeState = AltRepMeleeState.ToHeight;
      Log.Combat?.TWL(0, "AlternateMechRepresentation.initTargetHeight "+this.name+" height:"+ targetHeight);
    }
    public void meleeAnim() {
      j_Root.localPosition = Vector3.up * targetHeight;
      meleeT = 0f;
      meleeRate = 0.25f;
      meleeState = AltRepMeleeState.MeleeAnim;
      Log.Combat?.TWL(0, "AlternateMechRepresentation.meleeAnim " + this.name + " height:" + targetHeight);
    }
    public void Update() {
      if (j_Root == null) { return; }
      if (state == AltRepState.Grounded) { return; }
      if (state == AltRepState.Flying) {
        this.Flying();
        return;
      }
      t += rate * Time.deltaTime;
      if(state == AltRepState.Standing) {
        if (t >= 1.0f) { this.Starting(); }
      }
      if (state == AltRepState.Starting) {
        if ((t >= 0.5f)&&(isJumpjetsActive)) { this.StopJumpjets(); };
        if (t < 1.0f) { j_Root.localPosition = Vector3.up * Mathf.Lerp(0f, this.DefaultFlyHeight, t); } else {
          t = 0f; state = AltRepState.Flying;
          HeightSteadyState();
          if (DefaultFlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) { mechRep.StopJumpjetAudio(); }
          this.StartHoverAudio();
          this.parent.parentMech.BlockComponentsActivation(false);
          while (endAnimationQueue.Count > 0) { endAnimationQueue.Dequeue().Invoke(); }
        }
      } else
      if (state == AltRepState.Falling) {
        if (t < 1.0f) { j_Root.localPosition = Vector3.up * Mathf.Lerp(this.DefaultFlyHeight, 0f, t); } else {
          t = 0f; state = AltRepState.Grounded;
          this.parent.parentMech.BlockComponentsActivation(false);
          this.Land();
          this.StopHoverAudio();
          while (endAnimationQueue.Count > 0) { endAnimationQueue.Dequeue().Invoke(); }
        }
      }
    }
    public void InitAnimations() {
      j_Root = this.mechRep.transform.FindRecursive("j_Root");
      this.ThisAnimator = this.mechRep.GetComponent<Animator>();
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
    public void InitGameRep(Mech mech, Transform parentTransform) {
      try {
        if (Inited) { return; }
        Inited = true;
        Log.Combat?.TWL(0, "AlternateMechRepresentation.InitGameRep " + mech.DisplayName + " prefab:" + def.PrefabBase + " hardpoints:" + def.HardpointDataDef + " GameRep:" + mech.GameRep.name);
        if ((UnityEngine.Object)parentTransform == (UnityEngine.Object)null) {
          gameObject.transform.position = mech.CurrentPosition;
          gameObject.transform.rotation = mech.CurrentRotation;
        }
        this.InitAnimations();
        List<string> usedPrefabNames = new List<string>();
        HardpointDataDef hardpointDataDef = mech.Combat.DataManager.HardpointDataDefs.Get(def.HardpointDataDef);
        foreach (MechComponent allComponent in mech.allComponents) {
          if (allComponent.componentType != ComponentType.Weapon) {
            allComponent.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(hardpointDataDef, allComponent.baseComponentRef, def.PrefabBase, allComponent.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
            allComponent.baseComponentRef.hasPrefabName = true;
            Log.Combat?.WL(1, "allComponent.baseComponentRef.prefabName:" + allComponent.baseComponentRef.prefabName);
            if (!string.IsNullOrEmpty(allComponent.baseComponentRef.prefabName)) {
              Transform attachTransform = mech.GetAttachTransform(allComponent.mechComponentRef.MountedLocation);
              allComponent.InitGameRep(allComponent.baseComponentRef.prefabName, attachTransform, mech.LogDisplayName);
              mech.GameRep.miscComponentReps.Add(allComponent.componentRep);
            }
          }
        }
        foreach (Weapon weapon in mech.Weapons) {
          weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(hardpointDataDef, weapon.baseComponentRef, def.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
          weapon.baseComponentRef.hasPrefabName = true;
          Log.Combat?.WL(1, "weapon.baseComponentRef.prefabName:" + weapon.baseComponentRef.prefabName);
          if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
            Transform attachTransform = mech.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
            weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, mech.LogDisplayName);
            mech.GameRep.weaponReps.Add(weapon.weaponRep);
            string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(mech.MechDef, weapon.mechComponentRef);
            if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
              WeaponRepresentation component = mech.Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<WeaponRepresentation>();
              component.Init((ICombatant)this, attachTransform, true, mech.LogDisplayName, weapon.Location);
              mech.GameRep.weaponReps.Add(component);
            }
          }
        }
        foreach (MechComponent supportComponent in mech.supportComponents) {
          Weapon weapon = supportComponent as Weapon;
          if (weapon != null) {
            weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(hardpointDataDef, weapon.baseComponentRef, def.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
            weapon.baseComponentRef.hasPrefabName = true;
            Log.Combat?.WL(1, "supportComponent.baseComponentRef.prefabName:" + weapon.baseComponentRef.prefabName);
            if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
              Transform attachTransform = mech.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
              weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, mech.LogDisplayName);
              mech.GameRep.miscComponentReps.Add((ComponentRepresentation)weapon.weaponRep);
            }
          }
        }
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
        mech._CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
        if (!mech.MeleeWeapon.baseComponentRef.hasPrefabName) {
          mech.MeleeWeapon.baseComponentRef.prefabName = "chrPrfWeap_generic_melee";
          mech.MeleeWeapon.baseComponentRef.hasPrefabName = true;
        }
        mech.MeleeWeapon.InitGameRep(mech.MeleeWeapon.baseComponentRef.prefabName, mech.GetAttachTransform(mech.MeleeWeapon.mechComponentRef.MountedLocation), mech.LogDisplayName);
        if (!mech.DFAWeapon.mechComponentRef.hasPrefabName) {
          mech.DFAWeapon.mechComponentRef.prefabName = "chrPrfWeap_generic_melee";
          mech.DFAWeapon.mechComponentRef.hasPrefabName = true;
        }
        mech.DFAWeapon.InitGameRep(mech.DFAWeapon.mechComponentRef.prefabName, mech.GetAttachTransform(mech.DFAWeapon.mechComponentRef.MountedLocation), mech.LogDisplayName);
        bool flag1 = mech.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
        bool flag2 = mech.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
        if (flag1 | flag2) {
          SkinnedMeshRenderer[] componentsInChildren = mech.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
          for (int index = 0; index < componentsInChildren.Length; ++index) {
            if (flag1)
              componentsInChildren[index].sharedMaterial = mech.Combat.DataManager.TextureManager().PlaceholderUnfinishedMaterial;
            if (flag2)
              componentsInChildren[index].sharedMaterial = mech.Combat.DataManager.TextureManager().PlaceholderImpostorMaterial;
          }
        }
        if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab) == false) {
          Log.Combat?.WL(1, "spawning jump jets");
          try {
            if (AlternateMechRepresentation.JumpJetSrcPrefab == null) {
              AlternateMechRepresentation.JumpJetSrcPrefab = mech.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
              AlternateMechRepresentation.JumpJetSrcPrefab.SetActive(false);
            }
            //mech.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
            Log.Combat?.WL(2, "jumpJetSrcPrefab:" + (JumpJetSrcPrefab == null ? "null" : JumpJetSrcPrefab.name));
            if (JumpJetSrcPrefab != null) {
              JumpJetSrc = JumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
              Log.Combat?.WL(2, "jumpJetSrc:" + (JumpJetSrc == null ? "null" : JumpJetSrc.name));
            }
            this.DefaultFlyHeight = 0f;
            foreach (AirMechVerticalJetsDef vJet in def.AirMechVerticalJets) {
              Log.Combat?.WL(3,"attach:"+vJet.Attach+" prefab:"+vJet.Prefab+ " JetsAttachPoints:"+vJet.JetsAttachPoints.Count);
              Transform attach = this.transform.FindRecursive(vJet.Attach);
              if (attach == null) { Log.Combat?.WL(4,"attach is null"); continue; }
              GameObject vJetGO = mech.Combat.DataManager.PooledInstantiate(vJet.Prefab, BattleTechResourceType.Prefab);
              if (vJetGO == null) { Log.Combat?.WL(4, "prefab is null"); continue; }
              vJetGO.transform.SetParent(attach, false);
              if (JumpJetSrc == null) { Log.Combat?.WL(4, "JumpJetSrc is null"); continue; }
              foreach (string jetSpwanPoint in vJet.JetsAttachPoints) {
                Log.Combat?.WL(4, "jetSpwanPoint:"+ jetSpwanPoint);
                Transform spawnJetPoint = vJetGO.transform.FindRecursive(jetSpwanPoint);
                if (spawnJetPoint == null) { Log.Combat?.WL(5, "spawnJetPoint is null"); continue; }
                this.DefaultFlyHeight = def.FlyHeight;
                GameObject jumpJetBase = new GameObject("jumpJet");
                jumpJetBase.transform.SetParent(spawnJetPoint);
                jumpJetBase.transform.localPosition = Vector3.zero;
                jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                GameObject jumpJet = GameObject.Instantiate(JumpJetSrc.gameObject);
                jumpJet.SetActive(true);
                jumpJet.transform.SetParent(jumpJetBase.transform);
                jumpJet.transform.localPosition = Vector3.zero;
                jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
                jRep.Init(mech, spawnJetPoint, true, false, this.mechRep.name);
                ParticleSystem[] psyss = jumpJetBase.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem psys in psyss) {
                  psys.RegisterRestoreScale();
                  var main = psys.main;
                  main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                  //main.loop = true;
                  Log.Combat?.WL(3, psys.name + ":" + psys.main.scalingMode);
                }
                verticalJets.Add(jRep);
              }
              verticalJetsObjects.Add(vJetGO);
              vJetGO.SetActive(false);
            }
          } catch(Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
          }
        }
        mech.GameRep.RefreshEdgeCache();
        mech.GameRep.FadeIn(1f);
        if ((mech.IsDead == false) && mech.Combat.IsLoadingFromSave) {
          if (mech.AuraComponents != null) {
            foreach (MechComponent auraComponent in mech.AuraComponents) {
              for (int index = 0; index < auraComponent.componentDef.statusEffects.Length; ++index) {
                if (auraComponent.componentDef.statusEffects[index].targetingData.auraEffectType == AuraEffectType.ECM_GHOST) {
                  mech.GameRep.PlayVFXAt(mech.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                  mech.GameRep.PlayVFXAt(mech.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
                  return;
                }
              }
            }
          }
          if (mech.VFXDataFromLoad != null) {
            foreach (VFXEffect.StoredVFXEffectData storedVfxEffectData in mech.VFXDataFromLoad)
              mech.GameRep.PlayVFXAt(mech.GameRep.GetVFXTransform(storedVfxEffectData.hitLocation), storedVfxEffectData.hitPos, storedVfxEffectData.vfxName, storedVfxEffectData.isAttached, storedVfxEffectData.lookatPos, storedVfxEffectData.isOneShot, storedVfxEffectData.duration);
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
      }
    }
    public void Reset(Mech mech, Dictionary<BaseComponentRef, HasPrefabData> baseData) {
      foreach (MechComponent allComponent in mech.allComponents) {
        Traverse.Create(allComponent).Property<ComponentRepresentation>("componentRep").Value = null;
        allComponent.baseComponentRef.hasPrefabName = false;
        allComponent.baseComponentRef.prefabName = null;
        if (baseData.TryGetValue(allComponent.baseComponentRef, out HasPrefabData prefabData)) {
          allComponent.baseComponentRef.hasPrefabName = prefabData.hasPrefabName;
          allComponent.baseComponentRef.prefabName = prefabData.prefabName;
        }
      }
      foreach (Weapon weapon in mech.Weapons) {
        Traverse.Create(weapon).Property<ComponentRepresentation>("componentRep").Value = null;
        weapon.baseComponentRef.hasPrefabName = false;
        weapon.baseComponentRef.prefabName = null;
        if (baseData.TryGetValue(weapon.baseComponentRef, out HasPrefabData prefabData)) {
          weapon.baseComponentRef.hasPrefabName = prefabData.hasPrefabName;
          weapon.baseComponentRef.prefabName = prefabData.prefabName;
        }
      }
      foreach (MechComponent supportComponent in mech.supportComponents) {
        Weapon weapon = supportComponent as Weapon;
        if (weapon == null) { continue; }
        Traverse.Create(weapon).Property<ComponentRepresentation>("componentRep").Value = null;
        weapon.baseComponentRef.hasPrefabName = false;
        weapon.baseComponentRef.prefabName = null;
        if (baseData.TryGetValue(weapon.baseComponentRef, out HasPrefabData prefabData)) {
          weapon.baseComponentRef.hasPrefabName = prefabData.hasPrefabName;
          weapon.baseComponentRef.prefabName = prefabData.prefabName;
        }
      }
    }
    public void RestorePrefabs(Mech mech) {
      foreach (MechComponent allComponent in mech.allComponents) {
        if (components.TryGetValue(allComponent, out ComponentRepresentation representation)) {
          Traverse.Create(allComponent).Property<ComponentRepresentation>("componentRep").Value = representation;
        }
      }
      foreach (Weapon weapon in mech.Weapons) {
        if (components.TryGetValue(weapon, out ComponentRepresentation representation)) {
          Traverse.Create(weapon).Property<ComponentRepresentation>("componentRep").Value = representation;
        }
      }
      foreach (MechComponent supportComponent in mech.supportComponents) {
        Weapon weapon = supportComponent as Weapon;
        if (weapon == null) { continue; }
        if (components.TryGetValue(weapon, out ComponentRepresentation representation)) {
          Traverse.Create(weapon).Property<ComponentRepresentation>("componentRep").Value = representation;
        }
      }
    }
    public void StoreComponents(Mech mech) {
      foreach (MechComponent allComponent in mech.allComponents) {
        if (allComponent.componentRep == null) { continue; }
        if (components.ContainsKey(allComponent)) { continue; }
        components.Add(allComponent, allComponent.componentRep);
      }
      foreach (Weapon weapon in mech.Weapons) {
        if (weapon.componentRep == null) { continue; }
        if (components.ContainsKey(weapon)) { continue; }
        components.Add(weapon, weapon.weaponRep);
      }
      foreach (MechComponent supportComponent in mech.supportComponents) {
        Weapon weapon = supportComponent as Weapon;
        if (weapon == null) { continue; }
        if (weapon.componentRep == null) { continue; }
        if (components.ContainsKey(weapon)) { continue; }
        components.Add(weapon, weapon.weaponRep);
      }
    }
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
    public float TurnParam {
      set {
        if (!this.HasTurnParam) { return; }
        if (this.def.Type == AlternateRepType.AirMech) { return; }
        this.ThisAnimator.SetFloat(this.TurnHash, value);
      }
    }
    public float ForwardParam {
      set {
        if (!this.HasForwardParam) { return; }
        if (this.def.Type == AlternateRepType.AirMech) { return; }
        this.ThisAnimator.SetFloat(this.ForwardHash, value);
      }
    }
    public bool IsMovingParam {
      set {
        if (!this.HasIsMovingParam) { return; }
        if (this.def.Type == AlternateRepType.AirMech) { return; }
        this.ThisAnimator.SetBool(this.IsMovingHash, value);
      }
    }
    public bool BeginMovementParam {
      set {
        if (!this.HasBeginMovementParam) { return; };
        if (this.def.Type == AlternateRepType.AirMech) { return; }
        this.ThisAnimator.SetTrigger(this.BeginMovementHash);
      }
    }
    public float DamageParam {
      set {
        if (!this.HasDamageParam) { return; };
        if (this.def.Type == AlternateRepType.AirMech) { return; }
        this.ThisAnimator.SetFloat(this.DamageHash, value);
      }
    }
    public List<JumpjetRepresentation> jumpjetReps {
      get {
        return this.mechRep.jumpjetReps();
      }
    }
    public CapsuleCollider mainCollider {
      get {
        return this.mechRep.mainCollider();
      }
    }
    public void SetVisible(bool isVisible, bool isGhost) {
      this.isVisible = isVisible;
      mechRep.VisibleObject.SetActive(isVisible);
      foreach (GameObject jet in verticalJetsObjects) { jet.SetActive(isVisible); }
      mechRep.BlipObjectUnknown.SetActive(false);
      mechRep.BlipObjectIdentified.SetActive(false);
      mechRep.BlipObjectGhostWeak.SetActive(false);
      mechRep.BlipObjectGhostStrong.SetActive(isGhost);
      this.mainCollider.enabled = isVisible;
      if (mechRep.IsDead == false) {
        if (mechRep.VisibleLights != null) {
          foreach (Behaviour visibleLight in mechRep.VisibleLights) {
            visibleLight.enabled = true;
          }
        }
        if (mechRep.weaponReps != null) {
          for (int index = 0; index < mechRep.weaponReps.Count; ++index) {
            if ((UnityEngine.Object)mechRep.weaponReps[index] != (UnityEngine.Object)null) {
              int mountedLocation = mechRep.weaponReps[index].mountedLocation;
              ChassisLocations chassisLocations = (ChassisLocations)mountedLocation;
              bool flag = (double)mechRep.parentActor.StructureForLocation(mountedLocation) > 0.0;
              if (chassisLocations != ChassisLocations.LeftArm && chassisLocations != ChassisLocations.RightArm)
                flag = true;
              if (flag)
                mechRep.weaponReps[index].OnPlayerVisibilityChanged(isGhost ? VisibilityLevel.BlipGhost : (isVisible ? VisibilityLevel.LOSFull : VisibilityLevel.None));
              else
                mechRep.weaponReps[index].OnPlayerVisibilityChanged(VisibilityLevel.None);
            }
          }
        }
        if (mechRep.miscComponentReps != null) {
          for (int index = 0; index < mechRep.miscComponentReps.Count; ++index) {
            if ((UnityEngine.Object)mechRep.miscComponentReps[index] != (UnityEngine.Object)null)
              mechRep.miscComponentReps[index].OnPlayerVisibilityChanged(isGhost ? VisibilityLevel.BlipGhost : (isVisible ? VisibilityLevel.LOSFull : VisibilityLevel.None));
          }
        }
      }
      for (int index = 0; index < this.jumpjetReps.Count; ++index) {
        this.jumpjetReps[index].OnPlayerVisibilityChanged(isVisible ? (isGhost ? VisibilityLevel.BlipGhost : VisibilityLevel.LOSFull) : VisibilityLevel.None);
      }
      if (this.isJumping) {
        if (isVisible) {
          this.mechRep.StartJumpjetAudio();
        } else {
          this.mechRep.StopJumpjetAudio();
        }
      }
      this.mechRep.ToggleHeadlights(isVisible);
    }
    public void OnPlayerVisibilityChanged(VisibilityLevel newLevel) {
      mechRep.OnPlayerVisibilityChanged(newLevel);
      if (isBasic == false) {
        mechRep.BlipObjectUnknown.SetActive(false);
        mechRep.BlipObjectIdentified.SetActive(false);
      }
    }
    public void PlayComponentDestroyedVFX(int location, Vector3 attackDirection, string vfxName) {
      mechRep.PlayVFX(location, vfxName, false, Vector3.zero, true, -1f);
      //mechRep.CollapseLocation(location, attackDirection, false);
      mechRep.needsToRefreshCombinedMesh = true;
    }
    public void ToggleRandomIdles(bool shouldIdle) {
      this.mechRep.ToggleRandomIdles(this.def.Type == AlternateRepType.Normal ? shouldIdle : false);
      if(this.def.Type != AlternateRepType.Normal) {
        this.mechRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, (GameRepresentation.RotationCompleteDelegate)null);
      }
    }
    private bool isMoving;
    public void FacePoint() {
      Log.Combat?.TWL(0, "AlternateRepresentation.FacePoint type:"+this.def.Type+" state:"+this.state+" isMoving:"+isMoving);
      if (isMoving) { return; }
      if (this.def.Type == AlternateRepType.Normal) { return; }
      if (this.state != AltRepState.Flying) { return; }
    }
    public void ReturnToNeturalFacing() {
      Log.Combat?.TWL(0, "AlternateRepresentation.ReturnToNeturalFacing type:" + this.def.Type + " state:" + this.state + " isMoving:" + isMoving);
      if (isMoving) { return; }
      if (this.def.Type == AlternateRepType.Normal) { return; }
      if (this.state != AltRepState.Flying) { return; }
    }
  }
  public enum PendingAnimationStyle {
    None, PlayKnockdownAnim, ForceKnockdown, PlayShutdownAnim
  }
  public class AlternateMechRepresentations : MonoBehaviour {
    public Statistic noRandomIdlesStat;
    public bool noRandomIdles;
    public bool allowRandomIdles;
    private PendingAnimationStyle pendingAnimation;
    private Vector2 pendingAnimationDir;
    public float MoveClamp {
      get {
        if (CurrentRep < 0) { return 0f; }
        if (CurrentRep >= mechReps.Count) { return 0f; }
        return mechReps[CurrentRep].def.MoveClamp;
      }
    }
    public float MinJumpDistance {
      get {
        if (CurrentRep < 0) { return 1f; }
        if (CurrentRep >= mechReps.Count) { return 1f; }
        return mechReps[CurrentRep].def.MinJumpDistance;
      }
    }
    public Mech parentMech { get; set; }
    public Statistic CurrentRepStat { get; set; }
    public int CurrentRep { get; set; }
    public List<AlternateMechRepresentation> mechReps { get; set; }
    public PilotableActorRepresentation currentRepresentation {
      get {
        if (CurrentRep < 0) { return parentMech.GameRep; }
        if (CurrentRep >= mechReps.Count) { return parentMech.GameRep; }
        return mechReps[CurrentRep].mechRep;
      }
    }
    public AlternateMechRepresentation currentAltRep {
      get {
        if (CurrentRep < 0) { return null; }
        if (CurrentRep >= mechReps.Count) { return null; }
        return mechReps[CurrentRep];
      }
    }
    public bool isHovering {
      get {
        if (CurrentRep < 0) { return false; }
        if (CurrentRep >= mechReps.Count) { return false; }
        AlternateMechRepresentation altRep = mechReps[CurrentRep];
        if (altRep.def.Type == AlternateRepType.Normal) { return false; }
        if (altRep.state == AltRepState.Flying) { return true; }
        return false;
      }
    }
    public bool NoJumpjetsBlock {
      get {
        if (CurrentRep < 0) { return true; }
        if (CurrentRep >= mechReps.Count) { return true; }
        AlternateMechRepresentation altRep = mechReps[CurrentRep];
        return altRep.def.NoJumpjetsBlock;
      }
    }
    public float HoveringHeight {
      get {
        if (CurrentRep < 0) { return 0f; }
        if (CurrentRep >= mechReps.Count) { return 0f; }
        AlternateMechRepresentation altRep = mechReps[CurrentRep];
        if (altRep.def.Type == AlternateRepType.Normal) { return 0f; }
        if (altRep.state != AltRepState.Flying) { return 0f; }
        return altRep.def.FlyHeight;
      }
    }
    public void InitMeleeTargetHeight(float height) {
      if (CurrentRep < 0) { return; }
      if (CurrentRep >= mechReps.Count) { return; }
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) { return; }
      if (altRep.state != AltRepState.Flying) { return; }
      altRep.initTargetHeight(height);
    }
    public void hoverMeleeAnim() {
      if (CurrentRep < 0) { return; }
      if (CurrentRep >= mechReps.Count) { return; }
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) { return; }
      if (altRep.state != AltRepState.Flying) { return; }
      altRep.meleeAnim();
    }
    public void updateMeleeHeightT(float mt) {
      if (CurrentRep < 0) { return; }
      if (CurrentRep >= mechReps.Count) { return; }
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) { return; }
      if (altRep.state != AltRepState.Flying) { return; }
      altRep.updateMeleeT(mt);
    }
    public void twist(float angle) {
      Quaternion eulerTwist = Quaternion.Euler(0f,90.0f*angle,0f);
      //Log.TWL(0,"AlternateRepresentation twist:"+angle+" euler:"+eulerTwist.eulerAngles);
      foreach (AlternateMechRepresentation altRep in this.mechReps) {
        if (altRep.def.Type == AlternateRepType.Normal) { altRep.mechRep.currentTwistAngle = angle; if (altRep.mechRep.thisAnimator != null) { altRep.mechRep.thisAnimator.SetFloat("Twist",angle); } } else {
          altRep.mechRep.thisTransform.localRotation = eulerTwist;
        }
      }
    }
    public void FacePoint() {
      Log.Combat?.TWL(0, "AlternateMechRepresentations.FacePoint " + CurrentRep + " count:" + mechReps.Count);
      mechReps[CurrentRep].FacePoint();
    }
    public void ReturnToNeturalFacing() {
      Log.Combat?.TWL(0, "AlternateMechRepresentations.ReturnToNeturalFacing " + CurrentRep + " count:" + mechReps.Count);
      if ((CurrentRep < 0) && (CurrentRep >= mechReps.Count)) { return; }
      mechReps[CurrentRep].ReturnToNeturalFacing();
    }
    public void StartMovement() {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) { return; }
      if (altRep.state != AltRepState.Flying) { return; }
      altRep.StartMovement();
    }
    public void EndMovement() {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) { return; }
      if (altRep.state != AltRepState.Flying) { return; }
      altRep.EndMovement();
    }
    public void GetHitPosition(int location, ref Vector3 result) {
      if ((CurrentRep >= 0) && (CurrentRep < mechReps.Count)) {
        result = mechReps[CurrentRep].mechRep.GetHitPosition(location);
      }
    }
    public Transform GetVFXTransform(int location) {
      return mechReps[CurrentRep].mechRep.GetVFXTransform(location);
    }
    public void InitPaintScheme(HeraldryDef heraldryDef, string teamGUID) {
      foreach(AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.InitPaintScheme(heraldryDef, teamGUID);
      }
    }
    public void PlayVFX(int location, string vfxName, bool attached, Vector3 lookAtPos, bool oneShot, float duration) {
      if (attached) {
        foreach (AlternateMechRepresentation altRep in mechReps) {
          altRep.mechRep.PlayVFX(location, vfxName, attached, lookAtPos, oneShot, duration);
        }
      } else {
        AlternateMechRepresentation altRep = mechReps[CurrentRep];
        altRep.mechRep.PlayVFX(location, vfxName, attached, lookAtPos, oneShot, duration);
      }
    }
    public void PlayPersistentDamageVFX(int location) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayPersistentDamageVFX(location);
      }
    }
    public void PlayComponentCritVFX(int location) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayComponentCritVFX(location);
      }
    }
    public bool CanHandleDeath {
      get {
        AlternateMechRepresentation altRep = mechReps[CurrentRep];
        if (altRep.def.Type == AlternateRepType.Normal) { return true; }
        if (altRep.state == AltRepState.Grounded) { return true; }
        return false;
      }
    }
    public void DelayedHandleDeath(DeathMethod deathMethod, int location) {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        return;
      } else if (altRep.state == AltRepState.Grounded) {
        return;
      } else {
        Log.Combat?.TWL(0, "AlternateRepresentations.DelayedHandleDeath:"+this.parentMech.DisplayName);
        altRep.Landing();
        altRep.pushQueue(this, AccessTools.Method(typeof(AlternateMechRepresentations), "HandleDeath"), new object[] { deathMethod, location });
      }
    }
    public void HandleDeath(DeathMethod deathMethod, int location) {
      Log.Combat?.TWL(0, "AlternateRepresentations.HandleDeath:" + this.parentMech.DisplayName);
      MechRepresentation mechRep = this.parentMech.GameRep;
      if ((UnityEngine.Object)mechRep.VisibleObjectLight != (UnityEngine.Object)null) {
        mechRep.VisibleObjectLight.SetActive(false);
      }
      mechRep.thisAnimator.SetTrigger("Death");
      mechRep.OnDeath();
      List<string> stringList = new List<string>((IEnumerable<string>)mechRep.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) {
        mechRep.StopManualPersistentVFX(stringList[index]);
      }      
      mechRep.IsDead = true;
      mechRep.ToggleHeadlights(false);

      AlternateMechRepresentation curRep = mechReps[CurrentRep];
      curRep.mechRep.HandleDeath(deathMethod, location);
      foreach (AlternateMechRepresentation altRep in mechReps) {
        if (altRep == curRep) { continue; }
        altRep.HandleDeath(deathMethod,location);
      }
    }
    public void PlayKnockdownAnim(Vector2 attackDirection) {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        altRep.mechRep.PlayKnockdownAnim(attackDirection);
      } else if (altRep.state == AltRepState.Grounded) {
        altRep.mechRep.PlayKnockdownAnim(attackDirection);
      } else {
        this.pendingAnimation = PendingAnimationStyle.PlayKnockdownAnim;
        this.pendingAnimationDir = attackDirection;
        //altRep.Landing();
        //altRep.pushQueue(altRep.mechRep, AccessTools.Method(typeof(MechRepresentation), "PlayKnockdownAnim"), new object[] { attackDirection });
      }
    }
    public void PlayStandAnim() {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        altRep.mechRep.PlayStandAnim();
      } else if (altRep.state == AltRepState.Grounded) {
        altRep.mechRep.PlayStandAnim();
        altRep.Standing();
      }
    }
    public void ForceKnockdown(Vector2 attackDirection) {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        altRep.mechRep.ForceKnockdown(attackDirection);
      } else if (altRep.state == AltRepState.Grounded) {
        altRep.mechRep.ForceKnockdown(attackDirection);
      } else {
        this.pendingAnimation = PendingAnimationStyle.ForceKnockdown;
        this.pendingAnimationDir = attackDirection;
        //altRep.Landing();
        //altRep.pushQueue(altRep.mechRep, AccessTools.Method(typeof(MechRepresentation), "ForceKnockdown"), new object[] { attackDirection });
      }
    }
    public void PlayShutdownAnim() {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        altRep.mechRep.PlayShutdownAnim();
        this.pendingAnimation = PendingAnimationStyle.None;
      } else if (altRep.state == AltRepState.Grounded) {
        altRep.mechRep.PlayShutdownAnim();
        this.pendingAnimation = PendingAnimationStyle.None;
      } else {
        this.pendingAnimation = PendingAnimationStyle.PlayShutdownAnim;
        //altRep.Landing();
        //altRep.pushQueue(altRep.mechRep, AccessTools.Method(typeof(MechRepresentation), "PlayShutdownAnim"), new object[] { });
      }
    }
    public void PlayStartupAnim() {
      AlternateMechRepresentation altRep = mechReps[CurrentRep];
      if (altRep.def.Type == AlternateRepType.Normal) {
        altRep.mechRep.PlayStartupAnim();
      } else if (altRep.state == AltRepState.Grounded) {
        altRep.mechRep.PlayStartupAnim();
        altRep.Standing();
      }
    }
    public void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      }
    }
    public void PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayImpactAnimSimple(attackDirection, totalDamage);
      }
    }
    public void SetUnsteadyAnim(bool isUnsteady) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.SetUnsteadyAnim(isUnsteady);
      }
    }
    public void SetMeleeIdleState(bool isMelee) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.SetMeleeIdleState(isMelee);
      }
    }
    public void TriggerMeleeTransition(bool meleeIn) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.TriggerMeleeTransition(meleeIn);
      }
    }
    public void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayFireAnim(sourceLimb, recoilStrength);
      }
    }
    public void PlayMeleeAnim(int meleeHeight) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.PlayMeleeAnim(meleeHeight);
      }
    }
    public void PlayJumpLaunchAnim() {
      currentAltRep.mechRep.PlayJumpLaunchAnim();
    }
    public void PlayFallingAnim(Vector2 direction) {
      currentAltRep.mechRep.PlayFallingAnim(direction);
    }
    public void UpdateJumpAirAnim(float forward, float side) {
      currentAltRep.mechRep.UpdateJumpAirAnim(forward,side);
    }
    public void PlayJumpLandAnim(bool isDFA) {
      currentAltRep.mechRep.PlayJumpLandAnim(isDFA);
    }
    public void PlayMeleeAnim(ICombatant target, MeleeAttackType meleeType) {
      if ((meleeType == MeleeAttackType.Stomp)||(meleeType == MeleeAttackType.Tackle)) {
        Vehicle vehicle = target as Vehicle;
        if(vehicle != null) {
          if (vehicle.FlyingHeight() > 0.5f) { meleeType = MeleeAttackType.Punch; }
        }
      }
      this.parentMech.GameRep.PlayMeleeAnim(target.Combat.MeleeRules.GetMeleeHeightFromAttackType(meleeType));
    }
    public void UpdateLegDamageAnimFlags(LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.UpdateLegDamageAnimFlags(leftLegDamage, rightLegDamage);
      }
    }
    public void ToggleRandomIdles(bool shouldIdle) {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.ToggleRandomIdles(shouldIdle);
      }
    }
    public void UpdateSpline(ActorMovementSequence __instance, Vector3 ___Forward, float ___t) {
      Transform MoverTransform = __instance.MoverTransform;
      Vector3 newPosition = MoverTransform.position;
      foreach (AlternateMechRepresentation altRep in this.mechReps) {
        altRep.mechRep.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        altRep.mechRep.transform.localPosition = Vector3.zero;
      }
      mechReps[CurrentRep].UpdateFlyingAnim(___Forward);
      ICombatant meleeTarget = __instance.meleeTarget;
      if (meleeTarget != null) {
        mechReps[CurrentRep].updateMeleeT(___t);
      } else {
        mechReps[CurrentRep].updateMeleeIdle();
      }
    }
    public void CompleteMove(ActorMovementSequence __instance, bool playedMelee, ICombatant meleeTarget) {
      Vector3 newPosition = __instance.FinalPos;
      newPosition.y = __instance.owningActor.Combat.MapMetaData.GetCellAt(newPosition).cachedHeight;
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        altRep.mechRep.transform.localPosition = Vector3.zero;
      }
      if ((meleeTarget != null) && (playedMelee == false)) { mechReps[CurrentRep].meleeAnim(); };
      this.EndMovement();
      //AlternateMechRepresentation alternateRep = mechReps[CurrentRep];
      //if((alternateRep.def.Type == AlternateRepType.AirMech)&&(alternateRep.state == AltRepState.Flying)) {
      //  alternateRep.EndMovement();
      //}
    }
    public void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      string vfxName;
      AudioEventList_mech eventEnumValue;
      switch (UnityEngine.Random.Range(0, 4)) {
        case 0:
        vfxName = (string)parentMech.GameRep.Constants.VFXNames.componentDestruction_A;
        eventEnumValue = AudioEventList_mech.mech_part_break_a;
        break;
        case 1:
        vfxName = (string)parentMech.GameRep.Constants.VFXNames.componentDestruction_B;
        eventEnumValue = AudioEventList_mech.mech_part_break_b;
        break;
        case 2:
        vfxName = (string)parentMech.GameRep.Constants.VFXNames.componentDestruction_C;
        eventEnumValue = AudioEventList_mech.mech_part_break_c;
        break;
        default:
        vfxName = (string)parentMech.GameRep.Constants.VFXNames.componentDestruction_D;
        eventEnumValue = AudioEventList_mech.mech_part_break_a;
        break;
      }
      int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(eventEnumValue, parentMech.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
      if (this.parentMech.team.LocalPlayerControlsTeam)
        AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_crippled", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
      else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
        AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_crippled", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.PlayComponentDestroyedVFX(location, attackDirection, vfxName);
      }
    }
    public void OnCombatGameDestroyed() {
      foreach (AlternateMechRepresentation altRep in mechReps) {
        altRep.mechRep.OnCombatGameDestroyed();
      }
    }
    public float TurnParam {
      set {
        foreach (var altRep in mechReps) {
          altRep.TurnParam = value;
        }
      }
    }
    public float ForwardParam {
      set {
        foreach (var altRep in mechReps) {
          altRep.ForwardParam = value;
        }
      }
    }
    public bool IsMovingParam {
      set {
        foreach (var altRep in mechReps) {
          altRep.IsMovingParam = value;
        }
      }
    }
    public bool BeginMovementParam {
      set {
        foreach (var altRep in mechReps) {
          altRep.BeginMovementParam = value;
        }
      }
    }
    public float DamageParam {
      set {
        foreach (var altRep in mechReps) {
          altRep.DamageParam = value;
        }
      }
    }
    public AlternateMechRepresentations() {
      mechReps = new List<AlternateMechRepresentation>();
    }
    public static void ClearVisuals(MechRepresentation mechRep) {
      {
        SkinnedMeshRenderer[] meshes = mechRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        GameObject go = new GameObject("Empty");
        Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
        go.AddComponent<MeshRenderer>();
        //Log.TWL(0, "ClearVisuals:"+ mechRep.name);
        //GameObject nullRootBone = new GameObject("j_Null");
        //nullRootBone.transform.SetParent(mechRep.transform);
        //nullRootBone.transform.localPosition = Vector3.down * 1000f;
        //nullRootBone.transform.localScale = Vector3.zero;
        if (meshes != null) {
          foreach (SkinnedMeshRenderer mesh in meshes) {
            mesh.sharedMesh = emptyMesh;
            //mesh.rootBone = nullRootBone.transform;
            //Log.WL(1, mesh.gameObject.name + ":" + mesh.rootBone.name);
          }
        }
        GameObject.Destroy(go);
      }
      //Transform j_Root = mechRep.transform.FindRecursive("j_Root");
      //if (j_Root != null) { j_Root.localPosition += Vector3.down * 1000f; };
      //j_Root.localScale = new Vector3(0.001f, 0.001f, 0.001f);
      List<JumpjetRepresentation> jumpJets = new List<JumpjetRepresentation>(mechRep.jumpjetReps());
      mechRep.jumpjetReps().Clear();
      foreach (JumpjetRepresentation jRep in jumpJets) {
        jRep.gameObject.SetActive(false);
        GameObject.Destroy(jRep);
      }
      mechRep.VisibleLights = new BTLight[] { };
      List<GameObject> headLights = new List<GameObject>(mechRep.headlightReps());
      foreach (GameObject headlight in headLights) {
        headlight.SetActive(false);
      }
      mechRep.headlightReps().Clear();
      BTLight[] lights = mechRep.GetComponentsInChildren<BTLight>(true);
      foreach (BTLight light in lights) {
        light.enabled = false;
      }
    }
    public static void ClearComponents(MechRepresentation mechRep) {
      HashSet<WeaponRepresentation> wReps = new HashSet<WeaponRepresentation>();
      foreach (WeaponRepresentation wRep in mechRep.weaponReps) { wReps.Add(wRep); }
      foreach (MechComponent allComponent in mechRep.parentMech.allComponents) {
        ComponentRepresentation cRep = allComponent.componentRep;
        if (cRep != null) {
          WeaponRepresentation wRep = cRep as WeaponRepresentation; if (wRep != null) if (wReps.Contains(wRep)) { wReps.Remove(wRep); }
          GameObject.Destroy(cRep.gameObject);
          allComponent.componentRep = null;
        }
      }
      foreach (Weapon weapon in mechRep.parentMech.Weapons) {
        ComponentRepresentation cRep = weapon.componentRep;
        if (cRep != null) {
          WeaponRepresentation wRep = cRep as WeaponRepresentation; if (wRep != null) if (wReps.Contains(wRep)) { wReps.Remove(wRep); }
          GameObject.Destroy(cRep.gameObject);
          weapon.componentRep = null;
        }
      }
      foreach (MechComponent supportComponent in mechRep.parentMech.supportComponents) {
        Weapon weapon = supportComponent as Weapon;
        if (weapon == null) { continue; }
        ComponentRepresentation cRep = weapon.componentRep;
        if (cRep != null) {
          WeaponRepresentation wRep = cRep as WeaponRepresentation; if (wRep != null) if (wReps.Contains(wRep)) { wReps.Remove(wRep); }
          GameObject.Destroy(cRep.gameObject);
          weapon.componentRep = null;
        }
      }
      mechRep.miscComponentReps.Clear();
      mechRep.weaponReps.Clear();
      foreach (WeaponRepresentation wRep in wReps) { GameObject.Destroy(wRep.gameObject); }
    }
    public void noRandomIdlesChanged(Statistic value) {
      noRandomIdlesStat = value;
      allowRandomIdles = !noRandomIdlesStat.Value<bool>();
      Log.Combat?.TWL(0, "AlternateMechRepresentations.noRandomIdlesChanged allowRandomIdles:" + allowRandomIdles);
      foreach (var altRep in mechReps) {
        if (allowRandomIdles == false) {
          altRep.mechRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, (GameRepresentation.RotationCompleteDelegate)null);
        }
        altRep.ToggleRandomIdles(allowRandomIdles);
      }
    }
    public VisibilityLevel currentVisibilityLevel { get; set; }
    public void OnPlayerVisibilityChanged(VisibilityLevel newLevel) {
      currentVisibilityLevel = newLevel;
      for (int index = 0; index < this.mechReps.Count; ++index) {
        mechReps[index].OnPlayerVisibilityChanged(newLevel);
        if (index == CurrentRep) {
          mechReps[index].SetVisible(newLevel == VisibilityLevel.BlipGhost || newLevel == VisibilityLevel.LOSFull, newLevel == VisibilityLevel.BlipGhost);
        } else {
          mechReps[index].SetVisible(false, false);
        }
      }
    }
    private int pendingRepIndex;
    public void LateUpdate() {
      if (this.parentMech == null) { return; }
      if (this.parentMech.IsFlaggedForDeath) { return; }
      if (this.parentMech.IsDead) { return; }
      if (CurrentRepStat != null) {
        if (pendingRepIndex != Mathf.RoundToInt(CurrentRepStat.Value<float>())) {
          CurrentRepChanged(CurrentRepStat);
        }
      }
      if (noRandomIdlesStat != null) {
        if (noRandomIdles != noRandomIdlesStat.Value<bool>()) {
          noRandomIdles = noRandomIdlesStat.Value<bool>();
          noRandomIdlesChanged(noRandomIdlesStat);
        }
      }
    }
    public void ClearAllTags() {
      //foreach (AlternateMechRepresentation altRep in mechReps) {
      //  foreach (string tag in altRep.def.additionalEncounterTags) {
      //    this.parentMech.EncounterTags.Remove(tag);
      //  }
      //}
    }
    public void AddCurrentTags() {
      //AlternateMechRepresentation curRep = mechReps[CurrentRep];
      //foreach(string tag in curRep.def.additionalEncounterTags) {
      //  this.parentMech.EncounterTags.Add(tag);
      //}
    }
    public void PlayPendingAnimations() {
      if (this.pendingAnimation == PendingAnimationStyle.None) { return; }
      PendingAnimationStyle animType = this.pendingAnimation;
      this.pendingAnimation = PendingAnimationStyle.None;
      switch (animType) {
        case PendingAnimationStyle.PlayKnockdownAnim: this.PlayKnockdownAnim(this.pendingAnimationDir); break;
        case PendingAnimationStyle.ForceKnockdown: this.ForceKnockdown(this.pendingAnimationDir); break;
        case PendingAnimationStyle.PlayShutdownAnim: this.PlayShutdownAnim(); break;
      }
    }
    public void ChangeVisiblity(int repIndex) {
      if (repIndex < 0) { return; }
      if (repIndex >= mechReps.Count) { return; }
      if (this.parentMech.IsDead) { return; }
      if (this.parentMech.IsFlaggedForDeath) { return; }
      CurrentRep = repIndex;
      UpdateVisibility();
      mechReps[CurrentRep].RestorePrefabs(this.parentMech);
      mechReps[CurrentRep].PlayTransformSound();
      if (mechReps[CurrentRep].def.Type == AlternateRepType.Normal) { this.PlayPendingAnimations(); }
      ClearAllTags();
      AddCurrentTags();
      MoveClampHelper.Clear(this.parentMech);
      this.parentMech.isDFAForbidden((mechReps[CurrentRep].def.Type == AlternateRepType.AirMech) && (mechReps[CurrentRep].def.FlyHeight > Core.Settings.MaxHoveringHeightWithWorkingJets));
    }
    public void UpdateVisibility() {
      if (currentVisibilityLevel == VisibilityLevel.LOSFull || currentVisibilityLevel == VisibilityLevel.BlipGhost) {
        for (int index = 0; index < this.mechReps.Count; ++index) {
          if (index == CurrentRep) {
            mechReps[index].SetVisible(true, currentVisibilityLevel == VisibilityLevel.BlipGhost);
          } else {
            mechReps[index].SetVisible(false, false);
          }
        }
      }
    }
    public void CurrentRepChanged(Statistic value) {
      CurrentRepStat = value;
      if (Mathf.RoundToInt(CurrentRepStat.Value<float>()) == pendingRepIndex) { return; }
      pendingRepIndex = Mathf.RoundToInt(CurrentRepStat.Value<float>());
      int newRepIndex = pendingRepIndex;
      Log.Combat?.TWL(0, "AlternateMechRepresentations.CurrentRepChanged CurrentRep:" + CurrentRep + " newRepIndex:" + newRepIndex);
      if (newRepIndex < 0) { return; }
      if (newRepIndex >= mechReps.Count) { return; }
      Log.Combat?.WL(0, "curType:" + mechReps[CurrentRep].def.Type + " newType:" + mechReps[newRepIndex].def.Type + " curState:" + mechReps[CurrentRep].state + " newState:" + mechReps[newRepIndex].state);
      if ((mechReps[CurrentRep].def.Type == AlternateRepType.AirMech)
        && (mechReps[newRepIndex].def.Type == AlternateRepType.Normal)
        && (mechReps[CurrentRep].state != AltRepState.Grounded)
      ) {
        mechReps[CurrentRep].Landing();
        mechReps[CurrentRep].pushQueue(this, AccessTools.Method(typeof(AlternateMechRepresentations), "ChangeVisiblity"), new object[] { newRepIndex });
      } else
      if ((mechReps[CurrentRep].def.Type == AlternateRepType.Normal)
        && (mechReps[newRepIndex].def.Type == AlternateRepType.AirMech)
        && (mechReps[newRepIndex].state != AltRepState.Flying)
      ) {
        mechReps[newRepIndex].Starting();
        this.ChangeVisiblity(newRepIndex);
      } else {
        this.ChangeVisiblity(newRepIndex);
      }
    }
    public void InitRepresentations(Mech mech, Transform parentTransform) {
      this.parentMech = mech;
      UnitCustomInfo info = mech.GetCustomInfo();
      if (info == null) { return; }
      GameObject gameObject = mech.Combat.DataManager.PooledInstantiate(mech.MechDef.Chassis.PrefabIdentifier, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      Log.Combat?.WL(2, "PooledInstantiate:" + (gameObject == null ? "null" : gameObject.name));
      if (gameObject != null) {
        MechRepresentation mechRep = gameObject.GetComponent<MechRepresentation>();
        if (mechRep == null) { GameObject.Destroy(gameObject); } else {
          AlternateMechRepresentation alternateRepresentation = gameObject.GetComponent<AlternateMechRepresentation>();
          if (alternateRepresentation == null) { alternateRepresentation = gameObject.AddComponent<AlternateMechRepresentation>(); }
          alternateRepresentation.mechRep = mechRep;
          gameObject.GetComponent<Animator>().enabled = true;
          alternateRepresentation.def = new AlternateRepresentationDef(mech.MechDef.Chassis);
          alternateRepresentation.Inited = false;
          alternateRepresentation.isBasic = true;
          alternateRepresentation.parent = this;
          alternateRepresentation.state = AltRepState.Grounded;
          string prefabBase = mech.MechDef.Chassis.PrefabBase;
          alternateRepresentation.mechRep.Init(mech, parentTransform, false);
          this.mechReps.Add(alternateRepresentation);
          alternateRepresentation.mechRep.transform.SetParent(mech.GameRep.transform);
          Transform j_Root = alternateRepresentation.mechRep.transform.FindRecursive("j_Root");
          //if (j_Root != null) { j_Root.localPosition += Vector3.up * 20f; }
        }
      }
      foreach (var altRep in info.AlternateRepresentations) {
        gameObject = mech.Combat.DataManager.PooledInstantiate(altRep.PrefabIdentifier, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        Log.Combat?.WL(2, "PooledInstantiate:" + (gameObject == null ? "null" : gameObject.name));
        if (gameObject == null) { continue; }
        MechRepresentation mechRep = gameObject.GetComponent<MechRepresentation>();
        if (mechRep == null) { GameObject.Destroy(gameObject); continue; };
        AlternateMechRepresentation alternateRepresentation = gameObject.GetComponent<AlternateMechRepresentation>();
        if (alternateRepresentation == null) { alternateRepresentation = gameObject.AddComponent<AlternateMechRepresentation>(); }
        alternateRepresentation.mechRep = mechRep;
        gameObject.GetComponent<Animator>().enabled = true;
        alternateRepresentation.def = altRep;
        alternateRepresentation.Inited = false;
        alternateRepresentation.isBasic = false;
        alternateRepresentation.parent = this;
        alternateRepresentation.state = AltRepState.Grounded;
        string prefabBase = mech.MechDef.Chassis.PrefabBase;
        mech.MechDef.Chassis.PrefabBase = altRep.PrefabBase;
        alternateRepresentation.mechRep.Init(mech, parentTransform, false);
        mech.MechDef.Chassis.PrefabBase = prefabBase;
        this.mechReps.Add(alternateRepresentation);
        alternateRepresentation.mechRep.transform.SetParent(mech.GameRep.transform);
        Transform j_Root = alternateRepresentation.mechRep.transform.FindRecursive("j_Root");
        //if (j_Root != null) { j_Root.localPosition += Vector3.up * 15f; }
        alternateRepresentation.mechRep.mainCollider().center += Vector3.up * 15f;
        alternateRepresentation.mechRep.ToggleRandomIdles(alternateRepresentation.def.Type == AlternateRepType.Normal);
      }
    }
    public void Init(Mech mech) {
      pendingAnimation = PendingAnimationStyle.None;
      currentVisibilityLevel = VisibilityLevel.None;
      noRandomIdlesStat = mech.StatCollection.GetStatistic(UnitUnaffectionsActorStats.NoRandomIdlesActorStat);
      if (noRandomIdlesStat == null) { noRandomIdlesStat = mech.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoRandomIdlesActorStat, mech.MechDef.Chassis.GetCustomInfo().NoIdleAnimations); }
      noRandomIdlesStat.AddValueChangeListener(UnitUnaffectionsActorStats.NoRandomIdlesActorStat, new Action<Statistic>(this.noRandomIdlesChanged));
      allowRandomIdles = !noRandomIdlesStat.Value<bool>();
      CurrentRepStat = mech.StatCollection.GetStatistic(UnitUnaffectionsActorStats.AlternateRepresentationActorStat);
      if (CurrentRepStat == null) { CurrentRepStat = mech.StatCollection.AddStatistic<float>(UnitUnaffectionsActorStats.AlternateRepresentationActorStat, 0f); }
      CurrentRepStat.AddValueChangeListener(UnitUnaffectionsActorStats.AlternateRepresentationActorStat, new Action<Statistic>(this.CurrentRepChanged));
      CurrentRep = Mathf.RoundToInt(CurrentRepStat.Value<float>());
    }
  }
  public static class MechRepresentation_InitAlternate {
    public static TextureManager TextureManager(this DataManager dataManager) {
      return dataManager.TextureManager;
    }
    public static void Prefix(MechRepresentation __instance, Mech mech, Transform parentTransform, bool isParented, UnitCustomInfo info) {
      if (info.AlternateRepresentations.Count > 0) {
        Log.Combat?.TWL(0, "MechRepresentation.Init Alternate " + mech.DisplayName + " " + __instance.name);
        Log.Combat?.WL(1, "info.AlternateRepresentations.Count:" + info.AlternateRepresentations.Count);
        AlternateMechRepresentation altRep = __instance.GetComponent<AlternateMechRepresentation>();
        if (altRep != null) { return; };
        AlternateMechRepresentations alternateRepresentations = __instance.GetComponent<AlternateMechRepresentations>();
        if (alternateRepresentations == null) { alternateRepresentations = __instance.gameObject.AddComponent<AlternateMechRepresentations>(); }
      }
    }
  }
}