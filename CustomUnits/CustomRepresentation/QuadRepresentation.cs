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
using BattleTech.Rendering.UI;
using CustAmmoCategories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class QuadBodyKinematic: MonoBehaviour {
    public Transform RearLegsAttach { get; set; }
    public Transform FrontLegsAttach { get; set; }
    public Transform BodyTransform { get; set; }
    public void Update() {
      if (RearLegsAttach == null) { return; }
      if (FrontLegsAttach == null) { return; }
      if (BodyTransform == null) { return; }
      BodyTransform.position = RearLegsAttach.position;
      BodyTransform.LookAt(FrontLegsAttach,Vector3.up);
    }
  }
  public class QuadRepresentation : CustomMechRepresentation {
    public virtual QuadBodyKinematic quadBodyKinematic { get; set; }
    public virtual QuadVisualInfo quadVisualInfo { get; set; }
    public override bool HasOwnVisuals { get { return true; } }
    public virtual CustomMechRepresentation ForwardLegs { get; set; }
    public virtual CustomMechRepresentation RearLegs { get; set; }
    public virtual GameObject QuadBody { get; set; }
    public static void SuppressVisuals(CustomMechRepresentation rep, GameObject VisibleObject, List<string> sr, List<string> nsr) {
      HashSet<string> suppress = new HashSet<string>();
      HashSet<string> notSuppress = new HashSet<string>();
      foreach (string name in sr) { suppress.Add(name); }
      foreach (string name in nsr) { notSuppress.Add(name); }
      HashSet<GameObject> deleteObjects = new HashSet<GameObject>();
      SkinnedMeshRenderer[] skinnedRenderers = VisibleObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer renderer in skinnedRenderers) {
        if (renderer == null) { continue; }
        foreach (string name in suppress) {
          if (renderer.gameObject.name.Contains(name)) { deleteObjects.Add(renderer.gameObject); }
        }
        if (notSuppress.Count != 0) {
          bool notDel = false;
          foreach (string name in notSuppress) {
            if (renderer.gameObject.name.Contains(name)) { notDel = true; break; }
          }
          if (notDel == false) { deleteObjects.Add(renderer.gameObject); }
        }
      }
      MeshRenderer[] meshRenderers = VisibleObject.GetComponentsInChildren<MeshRenderer>(true);
      foreach (MeshRenderer renderer in meshRenderers) {
        if (renderer == null) { continue; }
        foreach (string name in suppress) {
          if (renderer.gameObject.name.Contains(name)) { deleteObjects.Add(renderer.gameObject); }
        }
        if (notSuppress.Count != 0) {
          bool notDel = false;
          foreach (string name in notSuppress) {
            if (renderer.gameObject.name.Contains(name)) { notDel = true; break; }
          }
          if (notDel == false) { deleteObjects.Add(renderer.gameObject); }
        }
      }
      foreach (GameObject go in deleteObjects) {
        if (go == null) { continue; }
        rep.UnregisterRenderers(go);
        GameObject.DestroyImmediate(go);
      };
    }
    public virtual void AddForwardLegs(CustomMechRepresentation flegs) {
      Log.TWL(0, "QuadRepresentation.AddForwardLegs " + this.gameObject.name + " " + (flegs == null ? "null" : flegs.name));
      this.ForwardLegs = flegs;
      flegs.transform.SetParent(j_Root);
      //frontLegs.HardpointData = dataManager.GetObjectOfType<HardpointDataDef>(customInfo.quadVisualInfo.);
      flegs.VisibleObject.name = "front_legs";
      flegs.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      Log.WL(1, "SuppressVisuals " + (flegs.VisibleObject == null ? "null" : flegs.VisibleObject.name) + " quadVisualInfo:" + (quadVisualInfo == null ? "null" : "not null"));
      QuadRepresentation.SuppressVisuals(flegs, flegs.VisibleObject, quadVisualInfo.SuppressRenderers, quadVisualInfo.NotSuppressRenderers);
      this.VisualObjects.Add(flegs.VisibleObject);
      this.slaveRepresentations.Add(flegs);
      flegs.isSlave = true;
      flegs.parentRepresentation = this;
      if (quadBodyKinematic != null) {
        quadBodyKinematic.FrontLegsAttach = this.ForwardLegs.vfxCenterTorsoTransform;
      }
      this.vfxLeftArmTransform = this.ForwardLegs.vfxLeftLegTransform;
      this.vfxRightArmTransform = this.ForwardLegs.vfxRightLegTransform;
      this.vfxLeftShoulderTransform = this.ForwardLegs.vfxLeftLegTransform;
      this.vfxRightShoulderTransform = this.ForwardLegs.vfxRightLegTransform;
      this.LeftArmAttach = this.ForwardLegs.LeftLegAttach;
      this.RightArmAttach = this.ForwardLegs.RightLegAttach;
      this.leftArmDestructible = this.ForwardLegs.leftLegDestructible;
      this.rightArmDestructible = this.ForwardLegs.rightLegDestructible;
    }
    public virtual void AddRearLegs(CustomMechRepresentation rlegs) {
      this.RearLegs = rlegs;
      rlegs.transform.SetParent(j_Root);
      //frontLegs.HardpointData = dataManager.GetObjectOfType<HardpointDataDef>(customInfo.quadVisualInfo.);
      rlegs.VisibleObject.name = "rear_legs";
      rlegs.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      QuadRepresentation.SuppressVisuals(rlegs, rlegs.VisibleObject, quadVisualInfo.SuppressRenderers, quadVisualInfo.NotSuppressRenderers);
      this.VisualObjects.Add(rlegs.VisibleObject);
      this.slaveRepresentations.Add(rlegs);
      rlegs.isSlave = true;
      rlegs.parentRepresentation = this;
      if (quadBodyKinematic != null) {
        quadBodyKinematic.RearLegsAttach = this.RearLegs.vfxCenterTorsoTransform;
      }
      this.vfxLeftLegTransform = this.RearLegs.vfxLeftLegTransform;
      this.vfxRightLegTransform = this.RearLegs.vfxRightLegTransform;
      this.LeftLegAttach = this.RearLegs.LeftLegAttach;
      this.RightLegAttach = this.RearLegs.RightLegAttach;
      this.leftLegDestructible = this.RearLegs.leftLegDestructible;
      this.rightLegDestructible = this.RearLegs.rightLegDestructible;
    }
    protected override void InitSlaves() {
      if (this.ForwardLegs != null) {
        this.ForwardLegs.Init(this.custMech, this.j_Root, true);
        this.ForwardLegs.transform.localPosition = new Vector3(0f, 0f, quadVisualInfo.BodyLength / 2f);
        this.SupressJumpjets(this.ForwardLegs);
        this.SupressHeadLights(this.ForwardLegs);
      }
      if (this.RearLegs != null) {
        this.RearLegs.Init(this.custMech, this.j_Root, true);
        this.RearLegs.transform.localPosition = new Vector3(0f, 0f, -quadVisualInfo.BodyLength / 2f);
        this.SupressJumpjets(this.RearLegs);
        this.SupressHeadLights(this.RearLegs);
      }
    }
    protected virtual void SupressJumpjets(CustomMechRepresentation rep) {
      for(int i = 0; i < rep.jumpjetReps.Count; ++i) {
        if (rep.jumpjetReps[i] == null) { continue; }
        Component cmp = rep.jumpjetReps[i];
        rep.UnregisterRenderers(cmp.gameObject);
        GameObject.DestroyImmediate(cmp.gameObject);
        rep.jumpjetReps[i] = null;
      }
      rep.jumpjetReps.Clear();
    }
    protected virtual void SupressHeadLights(CustomMechRepresentation rep) {
      for (int i = 0; i < rep.headlightReps.Count; ++i) {
        if (rep.headlightReps[i] == null) { continue; }
        rep.UnregisterRenderers(rep.headlightReps[i]);
        GameObject.DestroyImmediate(rep.headlightReps[i]);
        rep.headlightReps[i] = null;
      }
      rep.headlightReps.Clear();
    }
    public virtual void InitDestructable(Transform bodyRoot) {
      Log.TWL(0, "QuadRepresentation.InitDestructable quadVisualInfo:"+(this.quadVisualInfo==null?"null":"not null"));
      if (this.quadVisualInfo == null) { return; }
      foreach(var destr in this.quadVisualInfo.Destructables) {
        Log.WL(1, "searching obj:" + destr.Value.Name + " whole:" + destr.Value.wholeObj + " destroyedObj:" + destr.Value.destroyedObj);
        Transform obj = bodyRoot.FindRecursive(destr.Value.Name);
        Transform wholeObj = bodyRoot.FindRecursive(destr.Value.wholeObj);
        Transform destroyedObj = bodyRoot.FindRecursive(destr.Value.destroyedObj);
        if (obj == null) { Log.WL(1, "obj not found"); continue; }
        if (wholeObj == null) { Log.WL(1, "wholeObj not found"); continue; }
        if (destroyedObj == null) { Log.WL(1, "destroyedObj not found"); continue; }
        MechDestructibleObject dObj = obj.gameObject.AddComponent<MechDestructibleObject>();
        dObj.destroyedObj = destroyedObj.gameObject;
        dObj.wholeObj = wholeObj.gameObject;
        Log.WL(2, "updating destructible:" + destr.Key);
        switch (destr.Key) {
          case ChassisLocations.Head: this.headDestructible = dObj; break;
          case ChassisLocations.CenterTorso: this.centerTorsoDestructible = dObj; break;
          case ChassisLocations.LeftTorso: this.leftTorsoDestructible = dObj; break;
          case ChassisLocations.RightTorso: this.rightTorsoDestructible = dObj; break;
        }
      }
    }
    public virtual void AddBody(GameObject bodyGo, DataManager dataManager) {
      this.QuadBody = bodyGo;
      Transform bodyRoot = bodyGo.transform.FindRecursive("j_Root");
      Transform bodyMesh = bodyGo.transform.FindTopLevelChild("mesh");
      Transform camoholderGo = bodyGo.transform.FindTopLevelChild("camoholder");
      MeshRenderer camoholder = null;
      if (camoholderGo != null) {
        camoholder = camoholderGo.gameObject.GetComponent<MeshRenderer>();
        camoholderGo.transform.SetParent(this.VisibleObject.transform);
      }
      Log.TWL(0, "QuadRepresentation.AddBody: camoholder:" + (camoholder==null?"null":camoholder.name));
      if (bodyRoot != null) {
        bodyRoot.name = "j_QuadSkeleton";
        bodyRoot.SetParent(this.j_Root);
        quadBodyKinematic = bodyRoot.gameObject.AddComponent<QuadBodyKinematic>();
        quadBodyKinematic.BodyTransform = bodyRoot;//.FindRecursive("j_QuadBody");
        this.vfxCenterTorsoTransform = bodyRoot.FindRecursive("CT_vfx_transform");
        this.vfxLeftTorsoTransform = bodyRoot.FindRecursive("LT_vfx_transform");
        this.vfxRightTorsoTransform = bodyRoot.FindRecursive("RT_vfx_transform");
        this.vfxHeadTransform = bodyRoot.FindRecursive("HEAD_vfx_transform");
        this.TorsoAttach = bodyRoot.FindRecursive("CT_vfx_transform");
      }
      if (bodyMesh != null) {
        bodyMesh.gameObject.name = "quad_body";
        bodyMesh.SetParent(this.VisibleObject.transform);
        if (string.IsNullOrEmpty(quadVisualInfo.BodyShaderSource) == false) {
          GameObject shaderSource = dataManager.PooledInstantiate(quadVisualInfo.BodyShaderSource, BattleTechResourceType.Prefab);
          if (shaderSource != null) {
            Log.WL(1, "shader prefab found");
            Renderer shaderComponent = shaderSource.GetComponentInChildren<Renderer>();
            Renderer[] shaderTargets = bodyMesh.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in shaderTargets) {
              if (renderer.gameObject.name.StartsWith("camoholder")) { continue; }
              Log.WL(2, "renderer:" + renderer.name);
              for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                Log.WL(3, "material:" + renderer.materials[mindex].name + " <- " + shaderComponent.material.shader.name);
                renderer.materials[mindex].shader = shaderComponent.material.shader;
                renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
              }
            }
            dataManager.PoolGameObject(quadVisualInfo.BodyShaderSource, shaderSource);
          }
          this.headDestructible = null;
          this.centerTorsoDestructible = null;
          this.leftTorsoDestructible = null;
          this.rightTorsoDestructible = null;
          this.InitDestructable(bodyMesh);
        }
        this.VisualObjects.Add(bodyMesh.gameObject);
        //bodyMesh.gameObject.InitBindPoses();
        CustomMechMeshMerge merge = bodyMesh.gameObject.AddComponent<CustomMechMeshMerge>();
        merge.Init(this, bodyMesh.gameObject, this.GetComponentInParent<UICreep>(), this.GetComponent<PropertyBlockManager>(), "body_", camoholder);
        this.mechMerges.Add(merge);
        if((this.quadVisualInfo != null)&&(this.customRep != null)) {
          foreach (string bodyAnim in this.quadVisualInfo.Animators) { this.customRep.AddBodyAnimator(this.transform, bodyAnim); }
          foreach (string twistAnim in this.quadVisualInfo.TwistAnimators) { this.customRep.AddTwistAnimator(this.transform, twistAnim); }
          foreach (AttachInfoRecord wAttach in this.quadVisualInfo.WeaponsAttachPoints) { this.customRep.AddWeaponAttachPoint(this.gameObject, wAttach); }
          this.customRep.InBattle = true;
        }
        this.RegisterRenderersCustomHeraldry(bodyMesh.gameObject, camoholder);
      }
    }
    public override void _SetLoadAnimation() {
      base._SetLoadAnimation();
      this._SetLoadAnimation(this.ForwardLegs, true);
      this._SetLoadAnimation(this.RearLegs, false);
    }
    public virtual void _SetLoadAnimation(CustomMechRepresentation rep, bool isArms) {
      if (this.parentActor.IsDead) { return; }
      rep._UpdateHeatSetting();
      if (isArms) {
        rep._UpdateLegDamageAnimFlags(this.parentMech.LeftArmDamageLevel, this.parentMech.RightArmDamageLevel);
      } else {
        rep._UpdateLegDamageAnimFlags(this.parentMech.LeftLegDamageLevel, this.parentMech.RightLegDamageLevel);
      }
      if (this.parentMech.IsOrWillBeProne) {
        rep.thisAnimator.SetTrigger("LoadStateDowned");
        rep._ResetHitReactFlags();
        rep.thisAnimator.SetBool("KnockedDown", true);
        rep.thisAnimator.ResetTrigger("Stand");
      } else if (this.parentMech.IsShutDown) {
        rep.thisAnimator.SetTrigger("LoadStateShutdown");
        rep.thisAnimator.SetTrigger("LoadStateShutdownAdditive");
      } else {
        if (!this.parentMech.BracedLastRound && !this.parentMech.inMeleeIdle) { return; }
        rep._TriggerMeleeTransition(true);
      }
    }
    public override void _ClearLoadState() {
      base._ClearLoadState();
      this.ForwardLegs._ClearLoadState();
      this.RearLegs._ClearLoadState();
    }
    public override Vector3 GetHitPosition(int location) {
      return base.GetHitPosition(location);
    }
    public override Transform GetVFXTransform(int location) {
      return base.GetVFXTransform(location);
    }
    public override void _SetupDamageStates(Mech mech, MechDef mechDef) {
      base._SetupDamageStates(mech, mechDef);
    }
    public override void CollapseLocation(int location, Vector3 attackDirection, bool loading = false) {
      base.CollapseLocation(location, attackDirection, loading);
      this.ForwardLegs.needsToRefreshCombinedMesh = true;
      this.RearLegs.needsToRefreshCombinedMesh = true;
    }
    public override void _SetMeleeIdleState(bool isMelee) {
      base._SetMeleeIdleState(isMelee);
      this.ForwardLegs._SetMeleeIdleState(isMelee);
      this.RearLegs._SetMeleeIdleState(isMelee);
    }
    public override void _TriggerMeleeTransition(bool meleeIn) {
      base._TriggerMeleeTransition(meleeIn);
      this.ForwardLegs._TriggerMeleeTransition(meleeIn);
      this.RearLegs._TriggerMeleeTransition(meleeIn);
    }
    public override void SetRandomIdleValue(float value) {
      base.SetRandomIdleValue(value);
      this.ForwardLegs.SetRandomIdleValue(value);
      this.RearLegs.SetRandomIdleValue(value);
    }
    public override void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      base.PlayFireAnim(AttackSourceLimb.Torso, recoilStrength);
      this.ForwardLegs.PlayFireAnim(AttackSourceLimb.Torso, recoilStrength);
      this.RearLegs.PlayFireAnim(AttackSourceLimb.Torso, recoilStrength);
    }
    public override void PlayMeleeAnim(int meleeHeight, ICombatant target) {
      if (meleeHeight == 1) {
        if (target.FlyingHeight() >= Core.Settings.MaxHoveringHeightWithWorkingJets) {
          meleeHeight = 8;
        }
      }
      switch (meleeHeight) {
        case 0: meleeHeight = 0; break; //Tackle 
        case 1: meleeHeight = 1; break; //Stomp
        case 2: meleeHeight = 8; break; //Kick => Charge
        case 3: meleeHeight = 8; break; //Punch => Charge
        case 8: meleeHeight = 8; break; //Charge
        case 9: meleeHeight = 9; break; //DFA
      }
      base.PlayMeleeAnim(meleeHeight, target);
      this.ForwardLegs.PlayMeleeAnim(meleeHeight, target);
      this.RearLegs.PlayMeleeAnim(meleeHeight, target);
    }
    public override void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      base.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      this.ForwardLegs.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      this.RearLegs.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
    }
    public override void _PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      base._PlayImpactAnimSimple(attackDirection, totalDamage);
      this.ForwardLegs._PlayImpactAnimSimple(attackDirection, totalDamage);
      this.RearLegs._PlayImpactAnimSimple(attackDirection, totalDamage);
    }
    public override void _SetUnsteadyAnim(bool isUnsteady) {
      base._SetUnsteadyAnim(isUnsteady);
      this.ForwardLegs._SetUnsteadyAnim(isUnsteady);
      this.RearLegs._SetUnsteadyAnim(isUnsteady);
    }
    public override void PlayKnockdownAnim(Vector2 attackDirection) {
      base.PlayKnockdownAnim(attackDirection);
      this.ForwardLegs.PlayKnockdownAnim(attackDirection);
      this.RearLegs.PlayKnockdownAnim(attackDirection);
    }
    public override void _ForceKnockdown(Vector2 attackDirection) {
      base._ForceKnockdown(attackDirection);
      this.ForwardLegs._ForceKnockdown(attackDirection);
      this.RearLegs._ForceKnockdown(attackDirection);
    }
    public override void _ResetHitReactFlags() {
      base._ResetHitReactFlags();
      this.ForwardLegs._ResetHitReactFlags();
      this.RearLegs._ResetHitReactFlags();
    }
    public override void PlayStandAnim() {
      base.PlayStandAnim();
      this.ForwardLegs.PlayStandAnim();
      this.RearLegs.PlayStandAnim();
    }
    public override void PlayJumpLaunchAnim() {
      base.PlayJumpLaunchAnim();
      this.ForwardLegs.isJumping = true;
      this.ForwardLegs.thisAnimator.SetTrigger("Jump");
      this.ForwardLegs._SetMeleeIdleState(false);
      this.RearLegs.isJumping = true;
      this.RearLegs.thisAnimator.SetTrigger("Jump");
      this.RearLegs._SetMeleeIdleState(false);
    }
    public override void PlayFallingAnim(Vector2 direction) {
      base.PlayFallingAnim(direction);
      this.PlayFallingAnim(this.ForwardLegs, direction);
      this.PlayFallingAnim(this.RearLegs, direction);
    }
    public virtual void PlayFallingAnim(CustomMechRepresentation rep,Vector2 direction) {
      rep.GameRepresentation_PlayFallingAnim(direction);
      rep.thisAnimator.SetTrigger("Fall");
      rep._SetMeleeIdleState(false);
      this.UpdateJumpAirAnim(rep, direction.x, direction.y);
    }
    public virtual void UpdateJumpAirAnim(CustomMechRepresentation rep, float forward, float side) {
      rep.GameRepresentation_UpdateJumpAirAnim(forward, side);
      rep.thisAnimator.SetFloat("InAir_Forward", forward);
      rep.thisAnimator.SetFloat("InAir_Side", side);
    }
    public override void PlayJumpLandAnim(bool isDFA) {
      this.isJumping = false;
      if (isSlave == false) this._StopJumpjetEffect();
      if (isDFA) {
        this.thisAnimator.SetTrigger("DFA");
        base._SetMeleeIdleState(true);
      } else {
        this.thisAnimator.SetTrigger("Land");
      }
      this.ForwardLegs.PlayJumpLandAnim(isDFA);
      this.RearLegs.PlayJumpLandAnim(isDFA);
    }
    public override void _StartJumpjetEffect() {
      base._StartJumpjetEffect();
    }
    public override void _StopJumpjetEffect() {
      base._StopJumpjetEffect();
    }
    public override void OnPlayerVisibilityChangedCustom(VisibilityLevel newLevel) {
      try {
        base.OnPlayerVisibilityChangedCustom(newLevel);
        VisibilityLevel legsLevel = this.VisibleObject.activeSelf ? VisibilityLevel.LOSFull : VisibilityLevel.None;
        this.ForwardLegs.OnPlayerVisibilityChangedCustom(legsLevel);
        this.RearLegs.OnPlayerVisibilityChangedCustom(legsLevel);
      }catch(Exception e) {
        Log.TWL(0, e.ToString(),true);
      }
    }
    public override void PlayShutdownAnim() {
      base.PlayShutdownAnim();
      this.ForwardLegs.PlayShutdownAnim();
      this.RearLegs.PlayShutdownAnim();
    }
    public override void PlayStartupAnim() {
      base.PlayStartupAnim();
      this.ForwardLegs.PlayStartupAnim();
      this.RearLegs.PlayStartupAnim();
    }
    public override void HandleDeath(DeathMethod deathMethod, int location) {
      PilotableRepresentation_HandleDeath(deathMethod, location);
      this.ForwardLegs.PilotableRepresentation_HandleDeath(deathMethod, location);
      this.RearLegs.PilotableRepresentation_HandleDeath(deathMethod, location);
      if (isSlave == false) this._PlayDeathFloatie(deathMethod);
      if (this.parentActor.WasDespawned) { return; }
      if (this.VisibleObjectLight != null) { this.VisibleObjectLight.SetActive(false); }
      this.ForwardLegs?.VisibleObjectLight?.SetActive(false);
      this.RearLegs?.VisibleObjectLight?.SetActive(false);
      this.ForwardLegs.thisAnimator.SetTrigger("Death");
      this.RearLegs.thisAnimator.SetTrigger("Death");
      if (!this.parentMech.Combat.IsLoadingFromSave) {
        if (isSlave == false) {
          if (this.parentMech.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_destroyed");
          else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_destroyed");
        }
      }
      if (this.parentMech.IsOrWillBeProne || this.parentActor.WasEjected) { this.StartCoroutine(this.DelayProneOnDeath()); }
      if (!this.parentActor.WasEjected) { this.PlayDeathVFX(deathMethod, location); }
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) { this.StopManualPersistentVFX(stringList[index]); }
      this.__IsDead = true;
      if (deathMethod != DeathMethod.PilotKilled && !this.parentActor.WasEjected) {
        string vfxName;
        switch (UnityEngine.Random.Range(0, 4)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_A;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_B;
          break;
          case 2:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_C;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_D;
          break;
        }
        this.PlayVFX(8, vfxName, true, Vector3.zero, false, -1f);
        float num = UnityEngine.Random.Range(25f, 30f);
        FootstepManager.Instance.AddScorch(this.transform.position, new Vector3(UnityEngine.Random.Range(0.0f, 1f), 0.0f, UnityEngine.Random.Range(0.0f, 1f)).normalized, new Vector3(num, num, num), true);
      }
      this._ToggleHeadlights(false);
    }
    public override void _HandleDeathOnLoad(DeathMethod deathMethod, int location) {
      base._HandleDeathOnLoad(deathMethod, location);
      this.ForwardLegs._HandleDeathOnLoad(deathMethod, location);
      this.RearLegs._HandleDeathOnLoad(deathMethod, location);
    }
    public override void InitWeapons(List<ComponentRepresentationInfo> compInfo, string parentDisplayName) {
      base.InitWeapons(compInfo, parentDisplayName);
    }
    public override void SetupJumpJets() {
      this.jumpjetReps.Clear();
      if (this.HasOwnVisuals == false) { return; }
      if (this.quadVisualInfo == null) { return; }
      if (this.quadVisualInfo.JumpJets == null) { return; }
      if (this.quadVisualInfo.JumpJets.Count == 0) { return; }
      if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab)){ return; }
      GameObject jumpJetSrcPrefab = this._Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
      if (jumpJetSrcPrefab != null) {
        Transform jumpJetSrc = jumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
        if (jumpJetSrc != null) {
          foreach (string jumpjetAttachName in this.quadVisualInfo.JumpJets) {
            if (string.IsNullOrEmpty(jumpjetAttachName)) { continue; }
            Transform jumpJetAttach = this.gameObject.transform.FindRecursive(jumpjetAttachName);
            if (jumpJetAttach == null) { continue; }
            GameObject jumpJetBase = new GameObject("jumpJet");
            jumpJetBase.transform.SetParent(jumpJetAttach);
            jumpJetBase.transform.localPosition = Vector3.zero;
            jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject jumpJet = GameObject.Instantiate(jumpJetSrc.gameObject);
            jumpJet.transform.SetParent(jumpJetBase.transform);
            jumpJet.transform.localPosition = Vector3.zero;
            jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
            jRep.Init(this.parentCombatant, jumpJetAttach, true, false, this.name);
            jumpjetReps.Add(jRep);
          }
        }
        this._Combat.DataManager.PoolGameObject(Core.Settings.CustomJumpJetsComponentPrefab, jumpJetSrcPrefab);
      }
    }
    public override void SetupHeadlights() {
      this.headlightReps.Clear();
      if (string.IsNullOrEmpty(Core.Settings.CustomHeadlightComponentPrefab)) { return; }
      GameObject headlightSrcPrefab = this._Combat.DataManager.PooledInstantiate(Core.Settings.CustomHeadlightComponentPrefab, BattleTechResourceType.Prefab);
      Log.WL(0, "headlightSrcPrefab:" + (headlightSrcPrefab == null ? "null" : headlightSrcPrefab.name));
      if (headlightSrcPrefab != null) {
        //jumpJetSrcPrefab.printComponents(1);
        Transform headlightSrc = headlightSrcPrefab.transform.FindRecursive(Core.Settings.CustomHeadlightPrefabSrcObjectName);
        Log.WL(0, "headlightSrc:" + (headlightSrc == null ? "null" : headlightSrc.name));
        if (headlightSrc != null) {
          foreach (string headlightAttachName in this.quadVisualInfo.HeadLights) {
            if (string.IsNullOrEmpty(headlightAttachName)) { continue; }
            Transform headlightAttach = this.transform.FindRecursive(headlightAttachName);
            if (headlightAttach == null) { continue; }
            GameObject headlightBase = new GameObject("headlight");
            headlightBase.transform.SetParent(headlightAttach);
            headlightBase.transform.localPosition = Vector3.zero;
            headlightBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject headlight = GameObject.Instantiate(headlightSrc.gameObject);
            headlight.transform.SetParent(headlightBase.transform);
            headlight.transform.localPosition = Vector3.zero;
            headlight.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            this.headlightReps.Add(headlightBase);
          }
        }
        this._Combat.DataManager.PoolGameObject(Core.Settings.CustomHeadlightComponentPrefab, headlightSrcPrefab);
      }
    }
    protected override void LateUpdate() {
      if (this.SkipLateUpdate) { return; }
      PilotableActorRepresentation_LateUpdate();
      this.currentAnimTransition = this.thisAnimator.GetAnimatorTransitionInfo(0);
      this.currentAnimState = this.thisAnimator.GetCurrentAnimatorStateInfo(0);
      this.currentAnimStateHash = this.currentAnimState.fullPathHash;
      this.previousTransitionHash = this.previousAnimTransition.fullPathHash;
      this.previousAnimStateHash = this.previousAnimState.fullPathHash;
      if (this.currentAnimTransition.fullPathHash != this.previousTransitionHash)
        this.previousAnimTransition = this.currentAnimTransition;
      if (this.currentAnimStateHash != this.previousAnimStateHash) {
        if (this.currentAnimStateHash == this.idleStateEntryHash || this.currentAnimStateHash == this.idleStateMeleeEntryHash)
          this._SetIdleAnimState();
        if (this.previousAnimStateHash == this.standingHash) {
          if (isSlave == false) {
            string str = string.Format("Mech_Restart_{0}", (object)this.parentActor.GUID);
            AudioEventManager.CreateVOQueue(str, -1f, (MessageCenterMessage)null, (AkCallbackManager.EventCallback)null);
            AudioEventManager.QueueVOEvent(str, VOEvents.Mech_Stand_Restart, (AbstractActor)this.parentMech);
            AudioEventManager.StartVOQueue(0.0f);
          }
        }
        if ((this.currentAnimStateHash == this.groundDeathIdleHash || this.currentAnimStateHash == this.randomDeathIdleRandomizer) && !this.parentMech.Combat.IsLoadingFromSave) {
          if (isSlave == false) { this.PlayAlliesReportDeathVO(); }
        }
        this.previousAnimState = this.currentAnimState;
        if (isSlave == false) {
          int num = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_interrupted, this.audioObject);
        }
      }
    }
    public override void _ToggleRandomIdles(bool shouldIdle) {
      if (this.parentMech.IsOrWillBeProne || !this.parentMech.IsOperational || this.parentMech.IsDead) { return; }
      base._allowRandomIdles = shouldIdle;
      Log.TWL(0, "QuadRepresentation._ToggleRandomIdles " + shouldIdle + " " + this.parentMech.MechDef.ChassisID);
      if (this._allowRandomIdles) { return; }
      if (this.IsInMeleeIdle) {
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateMeleeEntryHash, 0.15f);
      } else {
        if (this.previousAnimState.fullPathHash != this.idleStateEntryHash && this.previousAnimState.fullPathHash != this.idleStateFlavorsHash && this.previousAnimState.fullPathHash != this.idleStateUnsteadyHash)
          return;
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateEntryHash, 0.15f);
      }
      this.ForwardLegs._ToggleRandomIdles(shouldIdle);
      this.RearLegs._ToggleRandomIdles(shouldIdle);
    }
    public override void OnGroundImpact(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      base.OnGroundImpact(forcedSlave);
    }
    public override void OnJumpLandI(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      base.OnJumpLandI(forcedSlave);
    }
    public override void OnCombatGameDestroyed() {
      base.OnCombatGameDestroyed();
      this.ForwardLegs.OnCombatGameDestroyed();
      this.RearLegs.OnCombatGameDestroyed();
    }
    public override float TurnParam {
      set {
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasTurnParam) { this.thisAnimator.SetFloat(this.TurnHash, value); }
        this.ForwardLegs.TurnParam = value;
        this.RearLegs.TurnParam = value;
      }
    }
    public override float ForwardParam {
      set {
        //Log.TWL(0,"AltRepresentations.ForwardParam "+this.parentMech.MechDef.ChassisID+ " NoMoveAnimation:"+ this.parentMech.NoMoveAnimation());
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasForwardParam) { this.thisAnimator.SetFloat(this.ForwardHash, value); }
        this.ForwardLegs.ForwardParam = value;
        this.RearLegs.ForwardParam = value;
      }
    }
    public override bool IsMovingParam {
      set {
        if (this.HasIsMovingParam) { this.thisAnimator.SetBool(this.IsMovingHash, value); }
        this.ForwardLegs.IsMovingParam = value;
        this.RearLegs.IsMovingParam = value;
      }
    }
    public override bool BeginMovementParam {
      set {
        if (this.HasBeginMovementParam) { this.thisAnimator.SetTrigger(this.BeginMovementHash); };
        this.ForwardLegs.BeginMovementParam = value;
        this.RearLegs.BeginMovementParam = value;
      }
    }
    public override void BeginMove(ActorMovementSequence sequence) {
      this.IsMovingParam = true;
      this.BeginMovementParam = true;
      if (this.parentMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.RearLegs.DamageParam = -1f;
      } else if (this.parentMech.RightLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.RearLegs.DamageParam = 1f;
      }
      if (this.parentMech.LeftArmDamageLevel == LocationDamageLevel.Destroyed) {
        this.ForwardLegs.DamageParam = -1f;
      } else if (this.parentMech.RightArmDamageLevel == LocationDamageLevel.Destroyed) {
        this.ForwardLegs.DamageParam = 1f;
      }
      this._SetMeleeIdleState(false);
      this.lastStateWasVisible = (this.rootParentRepresentation.VisibleObject.activeInHierarchy);
      this.lastStateWasVisible = this.lastStateWasVisible;
      if (this.lastStateWasVisible) { this.PlayMovementStartAudio(); }
    }
    public override void PlayVehicleTerrainImpactVFX(bool forcedSlave = false) {
      if (this.isSlave == false) { forcedSlave = true; }
      base.PlayVehicleTerrainImpactVFX(forcedSlave);
    }
    public override void ApplyScale(Vector3 sizeMultiplier) {
      this.ForwardLegs?.ApplyScale(sizeMultiplier);
      this.RearLegs?.ApplyScale(sizeMultiplier);
      this.quadBodyKinematic.BodyTransform.localScale = sizeMultiplier;
      if (this.ForwardLegs != null) {
        this.ForwardLegs.transform.localPosition = new Vector3(0f, 0f, (quadVisualInfo.BodyLength / 2f) * sizeMultiplier.z);
      }
      if (this.RearLegs != null) {
        this.RearLegs.transform.localPosition = new Vector3(0f, 0f, -(quadVisualInfo.BodyLength / 2f) * sizeMultiplier.z);
      }
    }

  }
}