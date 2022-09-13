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
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnits {
  public class AlternatesRepresentation : CustomMechRepresentation {
    public override AlternateRepresentationDef altDef {
      get { return (this.CurrentIndex >= 0) && (this.CurrentIndex < this.Alternates.Count) ? Alternates[CurrentIndex].altDef : this.altDef; }
      set { base.altDef = value; }
    }
    public override bool HasOwnVisuals { get { return false; } }
    public virtual List<CustomMechRepresentation> Alternates { get; protected set; } = new List<CustomMechRepresentation>();
    public virtual CustomMechRepresentation CurrentRepresentation { get; protected set; } = null;
    protected virtual int CurrentIndex { get; set; } = 0;
    public virtual void AddAlternate(CustomMechRepresentation rep) {
      this.slaveRepresentations.Add(rep);
      this.Alternates.Add(rep);
      rep.isSlave = true;
      rep.SkipLateUpdate = true;
      if (rep.BlipObjectGhostStrong != null) { rep.UnregisterRenderers(rep.BlipObjectGhostStrong); GameObject.DestroyImmediate(rep.BlipObjectGhostStrong); rep.BlipObjectGhostStrong = null; }
      if (rep.BlipObjectGhostWeak != null) { rep.UnregisterRenderers(rep.BlipObjectGhostWeak); GameObject.DestroyImmediate(rep.BlipObjectGhostWeak); rep.BlipObjectGhostWeak = null; }
      if (rep.BlipObjectIdentified != null) { rep.UnregisterRenderers(rep.BlipObjectIdentified); GameObject.DestroyImmediate(rep.BlipObjectIdentified); rep.BlipObjectIdentified = null; }
      if (rep.BlipObjectUnknown != null) { rep.UnregisterRenderers(rep.BlipObjectUnknown); GameObject.DestroyImmediate(rep.BlipObjectUnknown); rep.BlipObjectUnknown = null; }
      rep.parentRepresentation = this;
      this.VisualObjects.Add(rep.VisibleObject);
      if (CurrentRepresentation == null) {
        CurrentRepresentation = rep;
        rep.VisibleObject.name = "baseFromVisuals";
        rep.VisibleObject.transform.SetParent(this.VisibleObject.transform);
        this.vfxCenterTorsoTransform = rep.vfxCenterTorsoTransform;
        this.vfxLeftTorsoTransform = rep.vfxLeftTorsoTransform;
        this.vfxRightTorsoTransform = rep.vfxRightTorsoTransform;
        this.vfxHeadTransform = rep.vfxHeadTransform;
        this.vfxLeftArmTransform = rep.vfxLeftArmTransform;
        this.vfxRightArmTransform = rep.vfxRightArmTransform;
        this.vfxLeftLegTransform = rep.vfxLeftLegTransform;
        this.vfxRightLegTransform = rep.vfxRightLegTransform;
        this.vfxLeftShoulderTransform = rep.vfxLeftShoulderTransform;
        this.vfxRightShoulderTransform = rep.vfxRightShoulderTransform;
        this.LeftArmAttach = rep.LeftArmAttach;
        this.RightArmAttach = rep.RightArmAttach;
        this.LeftLegAttach = rep.LeftLegAttach;
        this.RightLegAttach = rep.RightLegAttach;
      } else {
        rep.VisibleObject.name = "alternate" + this.Alternates.Count + "Visuals";
        rep.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      }
    }
    public override void _SetLoadAnimation() {
      base._SetLoadAnimation();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._SetLoadAnimation(); }
    }
    public override void _ClearLoadState() {
      base._ClearLoadState();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._ClearLoadState(); }
    }
    public override void _SetupDamageStates(Mech mech, MechDef mechDef) {
      base._SetupDamageStates(mech, mechDef);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._SetupDamageStates(mech, mechDef); }
    }
    public override Vector3 GetHitPosition(int location) {
      if (this.CurrentRepresentation == null) { return base.GetHitPosition(location); }
      return this.CurrentRepresentation.GetHitPosition(location);
    }
    public override Transform GetVFXTransform(int location) {
      if (this.CurrentRepresentation == null) { return base.GetVFXTransform(location); }
      return this.CurrentRepresentation.GetVFXTransform(location);
    }
    public override void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      base.PlayComponentDestroyedVFX(location,attackDirection);
      this.needsToRefreshCombinedMesh = false;
      foreach (CustomMechRepresentation alt in Alternates) {
        if (alt == this.CurrentRepresentation) { alt.CollapseLocation(location, attackDirection); } else { alt.CollapseLocationNoVisual(location); }
        alt.needsToRefreshCombinedMesh = true;
      }
    }
    public override void CollapseLocation(int location, Vector3 attackDirection, bool loading = false) {
    }
    public override void _UpdateLegDamageAnimFlags(LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
      base._UpdateLegDamageAnimFlags(leftLegDamage, rightLegDamage);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._UpdateLegDamageAnimFlags(leftLegDamage, rightLegDamage); }
    }
    public override void _SetMeleeIdleState(bool isMelee) {
      base._SetMeleeIdleState(isMelee);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._SetMeleeIdleState(isMelee); }
    }
    public override void _TriggerMeleeTransition(bool meleeIn) {
      base._TriggerMeleeTransition(meleeIn);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._TriggerMeleeTransition(meleeIn); }
    }
    public override void SetRandomIdleValue(float value) {
      base.SetRandomIdleValue(value);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.SetRandomIdleValue(value); }
    }
    public override void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      base.PlayFireAnim(sourceLimb, recoilStrength);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayFireAnim(sourceLimb, recoilStrength); }
    }
    public override void PlayMeleeAnim(int meleeHeight, ICombatant target) {
      base.PlayMeleeAnim(meleeHeight, target);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayMeleeAnim(meleeHeight, target); }
    }
    public override void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      base.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage); }
    }
    public override void _PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      base._PlayImpactAnimSimple(attackDirection, totalDamage);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._PlayImpactAnimSimple(attackDirection, totalDamage); }
    }
    public override void _SetUnsteadyAnim(bool isUnsteady) {
      base._SetUnsteadyAnim(isUnsteady);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._SetUnsteadyAnim(isUnsteady); }
    }
    public override void PlayKnockdownAnim(Vector2 attackDirection) {
      base.PlayKnockdownAnim(attackDirection);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayKnockdownAnim(attackDirection); }
    }
    public override void _ForceKnockdown(Vector2 attackDirection) {
      base._ForceKnockdown(attackDirection);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._ForceKnockdown(attackDirection); }
    }
    public override void _ResetHitReactFlags() {
      base._ResetHitReactFlags();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._ResetHitReactFlags(); }
    }
    public override void PlayStandAnim() {
      this.j_Root.localRotation = Quaternion.identity;
      base.PlayStandAnim();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayStandAnim(); }
    }
    public override void PlayJumpLaunchAnim() {
      this.isJumping = true;
      this.thisAnimator.SetTrigger("Jump");
      base._SetMeleeIdleState(false);
      if (isSlave == false) this._StartJumpjetAudio();
      if (isSlave == false) this.PlayVFXAt((Transform)null, this.parentActor.CurrentPosition, (string)this.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayJumpLaunchAnim(); }
    }
    public override void PlayFallingAnim(Vector2 direction) {
      this.thisAnimator.SetTrigger("Fall");
      base._SetMeleeIdleState(false);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayFallingAnim(direction); }
    }
    public override void UpdateJumpAirAnim(float forward, float side) {
      this.thisAnimator.SetFloat("InAir_Forward", forward);
      this.thisAnimator.SetFloat("InAir_Side", side);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.UpdateJumpAirAnim(forward, side); }
    }
    public override void PlayJumpLandAnim(bool isDFA) {
      this.isJumping = false;
      if (isSlave == false) this._StopJumpjetEffect();
      if (isDFA) {
        this.thisAnimator.SetTrigger("DFA");
        this._SetMeleeIdleState(true);
      } else {
        this.thisAnimator.SetTrigger("Land");
      }
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayJumpLandAnim(isDFA); }
    }
    public override void _StartJumpjetEffect() {
      if(isSlave == false)this._StartJumpjetAudio();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._StartJumpjetEffect(); }
    }
    public override void _StopJumpjetEffect() {
      if (isSlave == false) this._StopJumpjetAudio();
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._StopJumpjetEffect(); }
    }
    public override void OnPlayerVisibilityChangedCustom(VisibilityLevel newLevel) {
      try {
        PilotableActorRepresentation_OnPlayerVisibilityChanged(newLevel);
        if (this.isJumping) {
          if (newLevel == VisibilityLevel.LOSFull)
            if (isSlave == false) this._StartJumpjetAudio();
            else
            if (isSlave == false) this._StopJumpjetAudio();
        }
        foreach (CustomMechRepresentation alt in this.Alternates) {
          VisibilityLevel altLvl = VisibilityLevel.None;
          if (alt == this.CurrentRepresentation) {
            if (newLevel == VisibilityLevel.LOSFull) { altLvl = VisibilityLevel.LOSFull; }
          }
          alt.OnPlayerVisibilityChangedCustom(altLvl);
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
    public override void _ToggleHeadlights(bool headlightsActive) {
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._ToggleHeadlights(headlightsActive); }
    }
    public override void PlayShutdownAnim() {
      GameRepresentation_PlayShutdownAnim();
      if (isSlave == false) { int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerdown, this.audioObject); }
      if (!this.parentMech.IsOrWillBeProne && !this.IsDead) {
        base._SetMeleeIdleState(false);
        base._TriggerMeleeTransition(false);
        this.thisAnimator.SetTrigger("PowerOff");
      }
      base._ToggleHeadlights(false);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayShutdownAnim(); }
    }
    public override void PlayStartupAnim() {
      GameRepresentation_PlayStartupAnim();
      if (isSlave == false) { int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerup, this.audioObject); }
      base._ToggleHeadlights(this.VisibleObject.activeInHierarchy);
      if (isSlave == false) {
        if (this.parentMech.team.LocalPlayerControlsTeam)
          AudioEventManager.PlayPilotVO(VOEvents.Mech_Power_Restart, (AbstractActor)this.parentMech);
        else
          AudioEventManager.PlayComputerVO(ComputerVOEvents.Mech_Powerup_Enemy);
      }
      this.thisAnimator.SetTrigger("PowerOn");
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PlayStartupAnim(); }
    }
    public override void HandleDeath(DeathMethod deathMethod, int location) {
      this.ChangeVisibility(0);
      PilotableRepresentation_HandleDeath(deathMethod, location);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.PilotableRepresentation_HandleDeath(deathMethod, location); }
      if (isSlave == false) this._PlayDeathFloatie(deathMethod);
      if (this.parentActor.WasDespawned) { return; }
      if (this.VisibleObjectLight != null) { this.VisibleObjectLight.SetActive(false); }
      try {
        foreach (CustomMechRepresentation alt in this.Alternates) { alt?.VisibleObjectLight?.SetActive(false); }
        foreach (CustomMechRepresentation alt in this.Alternates) { alt?.thisAnimator?.SetTrigger("Death"); }
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
      if (!this.parentMech.Combat.IsLoadingFromSave) {
        if (isSlave == false) {
          if (this.parentMech.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_destroyed");
          else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_destroyed");
        }
      }
      if (this.parentMech.FakeVehicle() == false) {
        if (this.parentMech.IsOrWillBeProne || this.parentActor.WasEjected) { this.StartCoroutine(this.DelayProneOnDeath()); }
      }
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
      foreach (CustomMechRepresentation alt in this.Alternates) { alt._HandleDeathOnLoad(deathMethod, location); }
    }
    public override void InitWeapons(List<ComponentRepresentationInfo> compInfo, string parentDisplayName) {
      Log.TWL(0, "AlternatesRepresentation.InitWeapons");
      foreach (CustomMechRepresentation alt in this.Alternates) {
        Log.WL(1, alt.gameObject.name);
        alt.InitWeapons(compInfo, parentDisplayName);
      }
      Log.WL(1, "current representation:"+ this.CurrentRepresentation.gameObject.name);
      foreach (ComponentRepresentation crep in this.CurrentRepresentation.miscComponentReps) {
        Log.WL(2, "miscComponent:" + crep.gameObject.name+" mechComponent:"+(crep.mechComponent == null?"null": crep.mechComponent.defId));
        if (crep.mechComponent == null) { continue; }
        crep.mechComponent.componentRep_set(crep);
      }
      foreach (WeaponRepresentation wrep in this.CurrentRepresentation.weaponReps) {
        Log.WL(2, "weapon:" + wrep.gameObject.name + " mechComponent:" + (wrep.weapon == null ? "null" : wrep.weapon.defId));
        if (wrep.weapon == null) { continue; }
        wrep.weapon.componentRep_set(wrep);
      }
    }
    public Statistic noRandomIdlesStat { get; set; }
    public Statistic CurrentRepStat { get; set; }
    public int pendingRepIndex { get; set; }
    public void noRandomIdlesChanged(Statistic value) {
      noRandomIdlesStat = value;
      this._allowRandomIdles = !noRandomIdlesStat.Value<bool>();
      Log.TWL(0, "AlternateMechRepresentations.noRandomIdlesChanged allowRandomIdles:" + this._allowRandomIdles);
      foreach (var altRep in Alternates) {
        if (this._allowRandomIdles == false) {
          altRep.ReturnToNeutralFacing(true, 0.5f, -1, -1, (GameRepresentation.RotationCompleteDelegate)null);
        }
        altRep._ToggleRandomIdles(allowRandomIdles);
      }
    }
    public override bool _allowRandomIdles {
      get { return base._allowRandomIdles; }
      set {
        base._allowRandomIdles = value;
        foreach (CustomMechRepresentation slave in this.Alternates) { slave._allowRandomIdles = value; }
      }
    }
    public virtual void MoveChilds(CustomMechRepresentation srcRep,Transform src, Transform dest) {
      List<Transform> trList = new List<Transform>();
      if(srcRep.skeleton.TryGetValue(src, out HashSet<Transform> bones) == false) {
        bones = new HashSet<Transform>();
      }
      Transform[] srcChilds = src.GetComponentsInChildren<Transform>(true);
      foreach(Transform tr in srcChilds) {
        if (tr.parent != src) { continue; }
        ComponentRepresentation compRep = tr.gameObject.GetComponent<ComponentRepresentation>();
        if (compRep != null) { continue; }
        Log.WL(2, "MoveChilds "+tr.name);
        if (bones.Contains(tr)) { Log.WL(3, "is in skeleton"); continue; }
        Vector3 localPos = tr.localPosition;
        Quaternion localRot = tr.localRotation;
        tr.SetParent(dest, false);
        tr.localPosition = localPos;
        tr.localRotation = localRot;
      }
    }
    public virtual void ChangeVisibility(int index) {
      Log.TWL(0, "AlternatesRepresentation.ChangeVisibility "+index);
      this.StopPersistentAudio();
      this.SetRandomIdleValue(0.6f);
      this.CurrentIndex = index;
      CustomMechRepresentation oldRep = this.CurrentRepresentation;
      foreach (Collider collider in oldRep.selfColliders) { collider.enabled = false; }
      this.CurrentRepresentation = this.Alternates[index];
      this.StartPersistentAudio();
      CustomMechRepresentation curRep = this.CurrentRepresentation;
      foreach (Collider collider in oldRep.selfColliders) { collider.enabled = true; }
      Log.WL(1, "vfxCenterTorsoTransform");
      this.MoveChilds(oldRep, oldRep.vfxCenterTorsoTransform, curRep.vfxCenterTorsoTransform);
      Log.WL(1, "vfxLeftTorsoTransform");
      this.MoveChilds(oldRep, oldRep.vfxLeftTorsoTransform, curRep.vfxLeftTorsoTransform);
      Log.WL(1, "vfxRightTorsoTransform");
      this.MoveChilds(oldRep, oldRep.vfxRightTorsoTransform, curRep.vfxRightTorsoTransform);
      Log.WL(1, "vfxHeadTransform");
      this.MoveChilds(oldRep, oldRep.vfxHeadTransform, curRep.vfxHeadTransform);
      Log.WL(1, "vfxLeftArmTransform");
      this.MoveChilds(oldRep, oldRep.vfxLeftArmTransform, curRep.vfxLeftArmTransform);
      Log.WL(1, "vfxRightArmTransform");
      this.MoveChilds(oldRep, oldRep.vfxRightArmTransform, curRep.vfxRightArmTransform);
      Log.WL(1, "vfxLeftLegTransform");
      this.MoveChilds(oldRep, oldRep.vfxLeftLegTransform, curRep.vfxLeftLegTransform);
      Log.WL(1, "vfxRightLegTransform");
      this.MoveChilds(oldRep, oldRep.vfxRightLegTransform, curRep.vfxRightLegTransform);
      Log.WL(1, "vfxLeftShoulderTransform");
      this.MoveChilds(oldRep, oldRep.vfxLeftShoulderTransform, curRep.vfxLeftShoulderTransform);
      Log.WL(1, "vfxRightShoulderTransform");
      this.MoveChilds(oldRep, oldRep.vfxRightShoulderTransform, curRep.vfxRightShoulderTransform);
      foreach (WeaponRepresentation wRep in this.CurrentRepresentation.weaponReps) {
        if (wRep == null) { continue; }
        if (wRep.weapon == null) { continue; }
        wRep.weapon.componentRep_set(wRep);
      }
      oldRep.OnPlayerVisibilityChangedCustom(VisibilityLevel.None);
      if (this.rootParentRepresentation.BlipDisplayed == false) {
        curRep.OnPlayerVisibilityChangedCustom(VisibilityLevel.LOSFull);
      }
      this.parentMech.FlyingHeight(this.CurrentRepresentation.altDef.FlyHeight);
      this.custMech.UpdateLOSHeight(this.CurrentRepresentation.altDef.FlyHeight);
      this.ClearAllTags();
      this.AddCurrentTags();
    }
    public virtual void ClearAllTags() {
      Log.TW(0, "AlternatesRepresentation.ClearAllTags");
      foreach (CustomMechRepresentation alt in this.Alternates) {
        foreach (string tag in alt.altDef.additionalEncounterTags) {
          this.parentMech.EncounterTags.Remove(tag);
          Log.W(1, tag);
        }
      }
      Log.WL(0, "");
    }
    public virtual void AddCurrentTags() {
      Log.TW(0, "AlternatesRepresentation.AddCurrentTags");
      foreach (string tag in this.CurrentRepresentation.altDef.additionalEncounterTags) {
        this.parentMech.EncounterTags.Add(tag);
        Log.W(1, tag);
      }
      Log.WL(0, "");
    }
    protected IEnumerator ChangeCurrentOnHeightCompleete(int index) {
      if (this.CurrentRepresentation == null) { this.ChangeVisibility(index); this.parentMech.BlockComponentsActivation(false); yield break; }
      if (this.CurrentRepresentation.HeightController == null) { this.ChangeVisibility(index); this.parentMech.BlockComponentsActivation(false); yield break; }
      while(this.CurrentRepresentation.HeightController.isInChangeHeight) {
        yield return new WaitForSeconds(0.1f);
      }
      this.ChangeVisibility(index); this.parentMech.BlockComponentsActivation(false);
    }
    protected IEnumerator RestoreActivationBlockOnColmpeete() {
      if (this.CurrentRepresentation == null) { this.parentMech.BlockComponentsActivation(false); yield break; }
      if (this.CurrentRepresentation.HeightController == null) { this.parentMech.BlockComponentsActivation(false); yield break; }
      while (this.CurrentRepresentation.HeightController.isInChangeHeight) {
        yield return new WaitForSeconds(0.1f);
      }
      this.parentMech.BlockComponentsActivation(false);
    }
    public virtual void CurrentRepChanged(Statistic value) {
      if (this.__IsDead) { return; }
      CurrentRepStat = value;
      if (Mathf.RoundToInt(CurrentRepStat.Value<float>()) == pendingRepIndex) { return; }
      pendingRepIndex = Mathf.RoundToInt(CurrentRepStat.Value<float>());
      int newRepIndex = pendingRepIndex;
      Log.TWL(0, "AlternateRepresentations.CurrentRepChanged CurrentRep:" + CurrentIndex + " newRepIndex:" + newRepIndex);
      if (newRepIndex < 0) { return; }
      if (newRepIndex >= Alternates.Count) { return; }
      Log.WL(1, "cur height:" + this.CurrentRepresentation.altDef.FlyHeight + " new height:" + Alternates[newRepIndex].altDef.FlyHeight);
      this.parentMech.BlockComponentsActivation(true);
      if (Alternates[newRepIndex].altDef.FlyHeight > this.CurrentRepresentation.altDef.FlyHeight) {
        Alternates[newRepIndex].HeightController.ForceHeight(this.CurrentRepresentation.altDef.FlyHeight);
        this.ChangeVisibility(newRepIndex);
        this.CurrentRepresentation.HeightController.PendingHeight = this.CurrentRepresentation.altDef.FlyHeight;
        this.StartCoroutine(this.RestoreActivationBlockOnColmpeete());
      } else {
        Alternates[newRepIndex].HeightController.ForceHeight(Alternates[newRepIndex].altDef.FlyHeight);
        this.CurrentRepresentation.HeightController.PendingHeight = Alternates[newRepIndex].altDef.FlyHeight;
        this.StartCoroutine(this.ChangeCurrentOnHeightCompleete(newRepIndex));
      }
    }
    protected override void InitSlaves() {
      foreach (Collider collider in selfColliders) { collider.enabled = false; }
      foreach (CustomMechRepresentation slave in this.Alternates) {
        bool enable_colliders = slave == this.CurrentRepresentation;
        foreach (Collider collider in slave.selfColliders) { collider.enabled = enable_colliders; }
        slave.Init(this.custMech, this.j_Root, true);
      }
      noRandomIdlesStat = this.parentCombatant.StatCollection.GetStatistic(UnitUnaffectionsActorStats.NoRandomIdlesActorStat);
      if (noRandomIdlesStat == null) { noRandomIdlesStat = mech.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoRandomIdlesActorStat, mech.MechDef.Chassis.GetCustomInfo().NoIdleAnimations); }
      noRandomIdlesStat.AddValueChangeListener(UnitUnaffectionsActorStats.NoRandomIdlesActorStat, new Action<Statistic>(this.noRandomIdlesChanged));
      allowRandomIdles = !noRandomIdlesStat.Value<bool>();
      CurrentRepStat = mech.StatCollection.GetStatistic(UnitUnaffectionsActorStats.AlternateRepresentationActorStat);
      if (CurrentRepStat == null) { CurrentRepStat = mech.StatCollection.AddStatistic<float>(UnitUnaffectionsActorStats.AlternateRepresentationActorStat, 0f); }
      CurrentRepStat.AddValueChangeListener(UnitUnaffectionsActorStats.AlternateRepresentationActorStat, new Action<Statistic>(this.CurrentRepChanged));
      CurrentIndex = Mathf.RoundToInt(CurrentRepStat.Value<float>());
    }
    public override void Twist(float angle) {
      this.thisAnimator.SetFloat("Twist", angle);
      foreach (CustomMechRepresentation alt in this.Alternates) { alt.Twist(angle); }
    }
    public virtual bool noRandomIdles { get; set; }
    protected override void LateUpdate() {
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
      if (this.CurrentRepresentation.triggerFootVFX) {
        this.CurrentRepresentation._TriggerFootFall(this.leftFootVFX);
      }
      if(this.CurrentRepresentation.customFootFalls.Count > 0) {
        foreach(Transform foot in this.CurrentRepresentation.customFootFalls) {
          this.CurrentRepresentation.CustomFootFall(foot);
        }
        this.CurrentRepresentation.customFootFalls.Clear();
      }
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
    public override void _ToggleRandomIdles(bool shouldIdle) {
      if (this.parentMech.IsOrWillBeProne || !this.parentMech.IsOperational || this.parentMech.IsDead) { return; }
      base._allowRandomIdles = shouldIdle;
      Log.TWL(0, "AltRepresentations._ToggleRandomIdles " + shouldIdle + " " + this.parentMech.MechDef.ChassisID);
      if (this._allowRandomIdles) { return; }
      if (this.IsInMeleeIdle) {
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateMeleeEntryHash, 0.15f);
      } else {
        if (this.previousAnimState.fullPathHash != this.idleStateEntryHash && this.previousAnimState.fullPathHash != this.idleStateFlavorsHash && this.previousAnimState.fullPathHash != this.idleStateUnsteadyHash)
          return;
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateEntryHash, 0.15f);
      }
      foreach (CustomMechRepresentation slave in this.Alternates) { slave._ToggleRandomIdles(shouldIdle); }
    }

    public override bool isSlaveVisible(CustomMechRepresentation slave) {
      if (slave.parentRepresentation != this) { return false; }
      if (this._VisibleToPlayer == false) { return false; }
      return slave == this.CurrentRepresentation;
    }
    public override MechFlyHeightController HeightController {
      get {
        if (this.CurrentRepresentation == null) return FHeightController;
        return this.CurrentRepresentation.HeightController;
      } set {
        if (this.CurrentRepresentation == null) this.FHeightController = value;
        this.CurrentRepresentation.HeightController = value;
      }
    }
    public override void GatherColliders() {
      base.GatherColliders();
      foreach (CustomMechRepresentation slave in this.Alternates) {
        slave.GatherColliders();
        foreach(Collider collider in slave.ownColliders) { this.ownColliders.Add(collider); }
      }
    }
    public override HashSet<string> presistantAudioStart { get { return this.CurrentRepresentation == null?this.f_presistantAudioStart:this.CurrentRepresentation.presistantAudioStart; } set { if (this.CurrentRepresentation == null) { this.f_presistantAudioStart = value; } else { this.CurrentRepresentation.presistantAudioStart = value; } } }
    public override HashSet<string> presistantAudioStop { get { return this.CurrentRepresentation == null ? this.f_presistantAudioStop : this.CurrentRepresentation.presistantAudioStop; } set { if (this.CurrentRepresentation == null) { this.f_presistantAudioStop = value; } else { this.CurrentRepresentation.presistantAudioStop = value; } } }
    public override HashSet<string> moveAudioStart { get { return this.CurrentRepresentation == null ? this.f_moveAudioStop : this.CurrentRepresentation.moveAudioStart; } set { if (this.CurrentRepresentation == null) { this.f_moveAudioStop = value; } else { this.CurrentRepresentation.moveAudioStart = value; } } }
    public override HashSet<string> moveAudioStop { get { return this.CurrentRepresentation == null ? this.f_presistantAudioStop : this.CurrentRepresentation.moveAudioStop; } set { if (this.CurrentRepresentation == null) { this.f_presistantAudioStop = value; } else { this.CurrentRepresentation.moveAudioStop = value; } } }
    public override void OnGroundImpact(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      if (this.CurrentRepresentation == null) { base.OnGroundImpact(forcedSlave); } else { this.CurrentRepresentation.OnGroundImpact(true); }
    }
    public override void OnJumpLand(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      if (this.CurrentRepresentation == null) { base.OnJumpLand(forcedSlave); } else { this.CurrentRepresentation.OnJumpLand(true); }
    }
    public override void OnCombatGameDestroyed() {
      base.OnCombatGameDestroyed();
      foreach (CustomMechRepresentation slave in this.Alternates) { slave.OnCombatGameDestroyed(); }
    }
    public override float TurnParam {
      set {
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasTurnParam) { this.thisAnimator.SetFloat(this.TurnHash, value); }
        foreach (CustomMechRepresentation slave in this.Alternates) { slave.TurnParam = value; }
      }
    }
    public override float ForwardParam {
      set {
        //Log.TWL(0,"AltRepresentations.ForwardParam "+this.parentMech.MechDef.ChassisID+ " NoMoveAnimation:"+ this.parentMech.NoMoveAnimation());
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasForwardParam) { this.thisAnimator.SetFloat(this.ForwardHash, value); }
        foreach (CustomMechRepresentation slave in this.Alternates) { slave.ForwardParam = value; }
      }
    }
    public override bool IsMovingParam {
      set {
        if (this.HasIsMovingParam) { this.thisAnimator.SetBool(this.IsMovingHash, value); }
        foreach (CustomMechRepresentation slave in this.Alternates) { slave.IsMovingParam = value; }
      }
    }
    public override bool BeginMovementParam {
      set {
        if (this.HasBeginMovementParam) { this.thisAnimator.SetTrigger(this.BeginMovementHash); };
        foreach (CustomMechRepresentation slave in this.Alternates) { slave.BeginMovementParam = value; }
      }
    }
    public override void BeginMove(ActorMovementSequence sequence) {
      this.IsMovingParam = true;
      this.BeginMovementParam = true;
      if (this.parentMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.DamageParam = -1f;
      } else if (this.parentMech.RightLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.DamageParam = 1f;
      }
      this._SetMeleeIdleState(false);
      this.CurrentRepresentation.lastStateWasVisible = (this.rootParentRepresentation.VisibleObject.activeInHierarchy);
      this.lastStateWasVisible = this.CurrentRepresentation.lastStateWasVisible;
      if (this.CurrentRepresentation.lastStateWasVisible) { this.PlayMovementStartAudio(); }
    }
    public override void PlayVehicleTerrainImpactVFX(bool forcedSlave = false) {
      if (this.isSlave == false) { forcedSlave = true; }
      this.CurrentRepresentation.PlayVehicleTerrainImpactVFX(forcedSlave);
    }
    public override void ApplyScale(Vector3 sizeMultiplier) {
      foreach(var altRep in this.Alternates) {
        altRep.ApplyScale(sizeMultiplier);
      }
    }
  }
  public partial class CustomMechRepresentation {
    protected MechFlyHeightController FHeightController = null;
    public virtual MechFlyHeightController HeightController { get { return FHeightController; } set { FHeightController = value; } }
    public virtual AlternateRepresentationDef altDef { get; set; } = null;
    public virtual int AlternateIndex { get; set; } = 0;
    public virtual bool RotateBody { get; set; } = false;
    public virtual bool SkipLateUpdate { get; set; } = false;
    public virtual bool HasOwnVisuals { get { return true; } }
    protected HashSet<string> f_presistantAudioStart = new HashSet<string>();
    protected HashSet<string> f_presistantAudioStop = new HashSet<string>();
    protected HashSet<string> f_moveAudioStart = new HashSet<string>();
    protected HashSet<string> f_moveAudioStop = new HashSet<string>();
    public virtual HashSet<string> presistantAudioStart { get { return f_presistantAudioStart; } set { f_presistantAudioStart = value; } }
    public virtual HashSet<string> presistantAudioStop { get { return f_presistantAudioStop; } set { f_presistantAudioStop = value; } }
    public virtual HashSet<string> moveAudioStart { get { return f_moveAudioStart; } set { f_moveAudioStart = value; } }
    public virtual HashSet<string> moveAudioStop { get { return f_moveAudioStop; } set { f_moveAudioStop = value; } }
    public virtual HashSet<string> footstepAudio { get; set; } = new HashSet<string>();
    public virtual HashSet<Collider> ownColliders { get; set; } = new HashSet<Collider>();
    public virtual HashSet<Collider> selfColliders { get; set; } = new HashSet<Collider>();
    public virtual Dictionary<MeshRenderer, bool> meshRenderersCache { get; set; } = new Dictionary<MeshRenderer, bool>();
    public virtual Dictionary<SkinnedMeshRenderer, bool> skinnedMeshRenderersCache { get; set; } = new Dictionary<SkinnedMeshRenderer, bool>();
    public virtual void ReturnToNeturalHeight() {
      this.HeightController.PendingHeight = this.altDef.FlyHeight;
    }
    public virtual void GatherColliders() {
      foreach (Collider collider in selfColliders) { ownColliders.Add(collider); }
    }
    public virtual void RegisterColliders(GameObject src) {
      Log.TWL(0, "CustomMechRepresentation.RegisterColliders "+src.name);
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        Log.WL(1,collider.gameObject.name);
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersMainHeraldry(GameObject src) {
      Log.TW(0, "CustomMechRepresentation.RegisterRenderersMainHeraldry: " + this.gameObject.name+" "+src.name);
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      Log.WL(1, "MeshRenderers:"+mRenderer.Length+ " SkinnedMeshRenderer:"+ sRenderer.Length);
      Dictionary<Renderer, MeshRenderer> customCamoHolders = new Dictionary<Renderer, MeshRenderer>();
      Transform[] trs = src.GetComponentsInChildren<Transform>();
      foreach (Transform tr in trs) {
        if (tr.name.StartsWith("camoholder") == false) { continue; }
        MeshRenderer camoholder = tr.gameObject.GetComponent<MeshRenderer>();
        if (camoholder == null) { continue; }
        SkinnedMeshRenderer parentSkinnedRenderer = tr.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
        MeshRenderer parentMeshRenderer = tr.parent.gameObject.GetComponent<MeshRenderer>();
        Renderer parentRenderer = parentSkinnedRenderer;
        if (parentRenderer == null) { parentRenderer = parentMeshRenderer; }
        if (parentRenderer == null) { continue; }
        if (customCamoHolders.ContainsKey(parentRenderer)) { continue; }
        customCamoHolders.Add(parentRenderer, camoholder);
      }
      foreach (MeshRenderer renderer in mRenderer) {
        if (renderer.gameObject.GetComponent<BTDecal>() != null) { continue; }
        if (meshRenderersCache.ContainsKey(renderer)) { continue; }
        Log.WL(1, "renderer:" + renderer.gameObject.name + " heraldry:" + true);
        this.meshRenderersCache.Add(renderer, true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, this.defaultMechCustomization);
        }
      }
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        if (skinnedMeshRenderersCache.ContainsKey(renderer)) { continue; };
        this.skinnedMeshRenderersCache.Add(renderer, true);
        Log.WL(1, "skinned renderer:" + renderer.gameObject.name+" heraldry:"+true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, this.defaultMechCustomization);
        }
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersCustomHeraldry(GameObject src, MeshRenderer paintSchemePlaceholder) {
      Log.TW(0, "CustomMechRepresentation.RegisterRenderersCustomHeraldry: " + this.gameObject.name + " " + src.name+ " paintSchemePlaceholder:"+(paintSchemePlaceholder == null?"null": paintSchemePlaceholder.gameObject.name));
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      Log.WL(1, "MeshRenderers:" + mRenderer.Length + " SkinnedMeshRenderer:" + sRenderer.Length);
      Dictionary<Renderer, MeshRenderer> customCamoHolders = new Dictionary<Renderer, MeshRenderer>();
      Transform[] trs = src.GetComponentsInChildren<Transform>();
      foreach (Transform tr in trs) {
        if(tr.name.StartsWith("camoholder") == false){ continue; }
        MeshRenderer camoholder = tr.gameObject.GetComponent<MeshRenderer>();
        if (camoholder == null) { continue; }
        SkinnedMeshRenderer parentSkinnedRenderer = tr.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
        MeshRenderer parentMeshRenderer = tr.parent.gameObject.GetComponent<MeshRenderer>();
        Renderer parentRenderer = parentSkinnedRenderer;
        if (parentRenderer == null) { parentRenderer = parentMeshRenderer; }
        if (parentRenderer == null) { continue; }
        if (customCamoHolders.ContainsKey(parentRenderer)) { continue; }
        customCamoHolders.Add(parentRenderer, camoholder);
      }
      foreach (MeshRenderer renderer in mRenderer) {
        if (renderer.gameObject.GetComponent<BTDecal>() != null) { continue; }
        if (renderer.name.StartsWith("camoholder")) { continue; }
        if (meshRenderersCache.ContainsKey(renderer)) { continue; }
        Log.WL(1, "renderer:" + renderer.gameObject.name + " heraldry:" + true);
        this.meshRenderersCache.Add(renderer, true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, paintSchemePlaceholder);
        }
      }
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        if (skinnedMeshRenderersCache.ContainsKey(renderer)) { continue; };
        if (renderer.name.StartsWith("camoholder")) { continue; }
        this.skinnedMeshRenderersCache.Add(renderer, true);
        Log.WL(1, "skinned renderer:" + renderer.gameObject.name + " heraldry:" + true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, paintSchemePlaceholder);
        }
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersComponentRepresentation(ComponentRepresentation src) {
      CustomHardpointDef hardpointDef = null;
      if(src.mechComponent != null) {
        hardpointDef = CustomHardPointsHelper.Find(src.mechComponent.mechComponentRef.prefabName);
      }
      if(hardpointDef == null) {
        RegisterRenderersMainHeraldry(src.gameObject);
      } else {
        Transform[] trs = src.gameObject.GetComponentsInChildren<Transform>(true);
        MeshRenderer paintSchemeHolder = null;
        foreach (Transform tr in trs) {
          if (tr.name.Contains(hardpointDef.paintSchemePlaceholder)) {
            paintSchemeHolder = tr.gameObject.GetComponent<MeshRenderer>();
            if (paintSchemeHolder != null) { break; }
          }
        }
        this.RegisterRenderersCustomHeraldry(src.gameObject, paintSchemeHolder);
      }
    }
    public virtual void UnregisterRenderers(GameObject src) {
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      foreach (MeshRenderer renderer in mRenderer) {
        meshRenderersCache.Remove(renderer);
      }
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        skinnedMeshRenderersCache.Remove(renderer);
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Remove(collider);
      }
    }
    public virtual void ClearRenderers() {
      meshRenderersCache.Clear();
      skinnedMeshRenderersCache.Clear();
    }
    public virtual void CollapseLocationNoVisual(int location) {
      MechDestructibleObject destructibleObject = this.GetDestructibleObject(location);
      if (destructibleObject != null) { destructibleObject.CollapseSwap(true); }
      switch ((ChassisLocations)location) {
        case ChassisLocations.LeftArm:
        case ChassisLocations.RightArm:
        for (int index = 0; index < this.weaponReps.Count; ++index) {
          WeaponRepresentation weaponRep = this.weaponReps[index];
          if (weaponRep != null && weaponRep.mountedLocation == location) { weaponRep.DestructibleCollapse(); }
        }
        break;
      }
    }
    public virtual void Twist(float angle) {
      this.currentTwistAngle = angle;
      Log.TWL(0,"CustomMechRepresentation.Twist "+angle+ " HasTwistAnimators:"+(this.customRep == null?"null": this.customRep.HasTwistAnimators.ToString()));
      if (this.customRep != null) {
        if (this.customRep.HasTwistAnimators) {
          this.customRep.Twist(angle);
          this.thisAnimator.SetFloat("Twist", 0f);
          return;
        }
      }
      if (this.RotateBody) {
        Quaternion eulerTwist = Quaternion.Euler(0f, 90.0f * angle, 0f);
        this.j_Root.localRotation = eulerTwist;
      } else {
        if (this.thisAnimator != null) { this.thisAnimator.SetFloat("Twist", angle); }
      }
    }
    public void AliginToWater(float deltaTime) {
      Log.TWL(0, "ActorMovementSequence.AliginToWater " + this.parentMech.MechDef.ChassisID);
      int layerMask = 1 << LayerMask.NameToLayer("Water");
      RaycastHit[] raycastHitArray = Physics.RaycastAll(new Ray(this.parentCombatant.CurrentPosition + Vector3.up * 100f, Vector3.down), 200f, layerMask);
      float waterLevel = float.NaN;
      foreach (RaycastHit hit in raycastHitArray) {
        if (float.IsNaN(waterLevel) || (hit.point.y > waterLevel)) {
          Log.LogWrite(2, "hit pos:" + hit.point + " " + hit.collider.gameObject.name + " layer:" + LayerMask.LayerToName(hit.collider.gameObject.layer) + "\n");
          waterLevel = hit.point.y;
        }
      }
      if (float.IsNaN(waterLevel) == false) {
        //if (waterLevel > this.parentCombatant.CurrentPosition.y) {
        this.HeightController.ForceHeight(waterLevel - this.parentCombatant.CurrentPosition.y - Core.Settings.MaxHoveringHeightWithWorkingJets / 2f);
        //}
      }
    }
    public void AliginToTerrain(float deltaTime) {
      Log.TWL(0, "ActorMovementSequence.AliginToTerrain " + this.parentMech.MechDef.ChassisID);
      if (Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value == 0) {
        Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value = LayerMask.GetMask("Terrain", "Obstruction");
      }
      RaycastHit[] raycastHitArray = Physics.RaycastAll(new Ray(this.j_Root.position + Vector3.up * 20f, Vector3.down), this.j_Root.localPosition.y + 40f, Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value);
      RaycastHit? nullable = new RaycastHit?();
      RaycastHit raycastHit;
      HashSet<Collider> skipColliders = this.rootParentRepresentation.ownColliders;
      //Log.W(1, "skipColliders:");
      //foreach (Collider collider in skipColliders) { Log.W(1,collider.transform.name); } ;
      //Log.WL(0, "");
      for (int index = 0; index < raycastHitArray.Length; ++index) {
        if (skipColliders.Contains(raycastHitArray[index].collider) == false) {
          Log.WL(1, "ray hit:" + raycastHitArray[index].point + " hit collider:" + raycastHitArray[index].collider.transform.name);
          if (!nullable.HasValue) {
            nullable = new RaycastHit?(raycastHitArray[index]);
          } else {
            raycastHit = nullable.Value;
            if ((double)raycastHit.point.y < (double)raycastHitArray[index].point.y)
              nullable = new RaycastHit?(raycastHitArray[index]);
          }
        }
      }
      if (!nullable.HasValue) { return; }
      raycastHit = nullable.Value;
      Vector3 normal = raycastHit.normal;
      Log.WL(1, "ray hit found. Point:" + raycastHit.point + " hit collider:" + raycastHit.collider.transform.name+ " normal:"+ normal);
      //Quaternion to = Quaternion.FromToRotation(this.j_Root.up, normal) * Quaternion.Euler(0.0f, this.j_Root.rotation.eulerAngles.y, 0.0f);
      //this.j_Root.rotation = Quaternion.RotateTowards(this.j_Root.rotation, to, 180f * deltaTime);
      Quaternion to = Quaternion.FromToRotation(this.transform.up, normal) * Quaternion.Euler(0.0f, this.transform.rotation.eulerAngles.y, 0.0f);
      this.j_Root.rotation = Quaternion.RotateTowards(this.transform.rotation, to, 180f * deltaTime);
    }
    public virtual void OnAttackComplete() {
      if(this.HeightController != null) {
        this.HeightController.PendingHeight = this.parentCombatant.FlyingHeight();
      }
    }
    public virtual void UpdateRotation(Transform moveTransform, Vector3 forward, float deltaT) {
      bool aliginToTerrain = false;
      bool vehicleMovement = this.parentCombatant.NoMoveAnimation() || this.parentCombatant.FakeVehicle();
      if (vehicleMovement && this.parentCombatant.FlyingHeight() < Core.Settings.MaxHoveringHeightWithWorkingJets) {
        aliginToTerrain = true;
      }
      if (this.parentCombatant.NavalUnit()) {
        AudioSwitch_surface_type currentSurfaceType = this.rootParentRepresentation._CurrentSurfaceType;
        if ((currentSurfaceType == AudioSwitch_surface_type.water_deep) || (currentSurfaceType == AudioSwitch_surface_type.water_shallow)) {
          aliginToTerrain = false;
          this.AliginToWater(deltaT);
        }
      }
      if (aliginToTerrain) {
        if (forward.sqrMagnitude > Core.Epsilon) {
          moveTransform.rotation = Quaternion.RotateTowards(moveTransform.rotation, Quaternion.LookRotation(forward), 180f * deltaT);
        }
        this.AliginToTerrain(deltaT);
      } else {
        if (forward.sqrMagnitude > Core.Epsilon) {
          moveTransform.LookAt(moveTransform.position + forward, Vector3.up);
        }
      }
    }
  }
  public class MechFlyHeightController: MonoBehaviour {
    private static GameObject JumpJetSrcPrefab = null;
    private static Transform JumpJetSrc = null;
    public virtual bool isJumpjetsActive { get; protected set; } = false;
    public virtual bool isInChangeHeight { get; protected set; } = false;
    protected float FPendingHeight = 0f;
    public virtual float CurrentHeight { get { return this.parent == null ? 0f : this.parent.j_Root.localPosition.y; } }
    public virtual float StartingHeight { get; set; }
    public virtual float PendingHeight { get { return FPendingHeight; } set { StartingHeight = this.CurrentHeight; isInChangeHeight = true; FPendingHeight = value; this.OnHeightChange(); } }
    public virtual CustomMechRepresentation parent { get; set; } = null;
    public List<JumpjetRepresentation> verticalJets { get; set; } = new List<JumpjetRepresentation>();
    public List<GameObject> verticalJetsObjects { get; set; } = new List<GameObject>();
    public virtual float UpSpeed { get; set; } = 5f;
    public virtual float DownSpeed { get; set; } = -20f;
    public virtual float EffectiveDownSpeed { get; set; } = -20f;
    public virtual bool FakeHeightControl { get; set; } = false;
    public virtual bool ForceJumpJetsActive { get; set; } = false;
    public virtual bool vfxEveryJet { get; set; } = false;
    public virtual HashSet<Action> heightChangeCompleteAction { get; set; } = new HashSet<Action>();
    public virtual void OnHeightChange() {
      bool VisibleToPlayer = parent.VisibleToPlayer;
      if (ForceJumpJetsActive || ((CurrentHeight < Core.Settings.MaxHoveringHeightWithWorkingJets)&&(this.PendingHeight > Core.Settings.MaxHoveringHeightWithWorkingJets))) {
        Log.TWL(0, "MechFlyHeightController.OnHeightChange " + this.parent.gameObject.name+ " VisibleToPlayer:" + VisibleToPlayer);
        this.isJumpjetsActive = true;
        parent.SetMeleeIdleState(false);
        if (VisibleToPlayer && (DeployManualHelper.IsInManualSpawnSequence == false)) {
          foreach (GameObject jet in verticalJetsObjects) { jet.SetActive(true); }
        }
        foreach (JumpjetRepresentation jet in verticalJets) {
          Log.WL(1, "starting jets "+jet.gameObject.name);
          jet.SetState(JumpjetRepresentation.JumpjetState.Launching);
        }
        if (VisibleToPlayer && (DeployManualHelper.IsInManualSpawnSequence == false)) {
          this.StartJumpjetAudio();
          if (this.vfxEveryJet == false) {
            this.parent.PlayVFXAt((Transform)null, this.parent.j_Root.position, (string)this.parent.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
          } else {
            foreach (GameObject jet in verticalJetsObjects) {
              this.parent.PlayVFXAt((Transform)null, jet.transform.position, (string)this.parent.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
            }
          }
        }
      }
    }
    public bool JumpAudioActive { get; set; } = false;
    public void StartJumpjetAudio() {
      Log.TWL(0, "MechFlyHeightController.StartJumpjetAudio "+this.parent.parentMech.MechDef.ChassisID);
      if (this.JumpAudioActive == false) {
        if (this.parent.parentMech.weightClass == WeightClass.HEAVY || this.parent.parentMech.weightClass == WeightClass.ASSAULT) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_heavy_start, this.parent.rootParentRepresentation.audioObject);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_light_start, this.parent.rootParentRepresentation.audioObject);
        }
        this.JumpAudioActive = true;
      }
    }
    public void StopJumpjetAudio() {
      Log.TWL(0, "MechFlyHeightController.StopJumpjetAudio " + this.parent.parentMech.MechDef.ChassisID);
      if (this.JumpAudioActive) {
        if (this.parent.parentMech.weightClass == WeightClass.HEAVY || this.parent.parentMech.weightClass == WeightClass.ASSAULT) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_heavy_stop, this.parent.rootParentRepresentation.audioObject);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_light_stop, this.parent.rootParentRepresentation.audioObject);
        }
        this.JumpAudioActive = false;
      }
    }
    public void OnVisibilityChange(VisibilityLevel level) {
      foreach (GameObject vJet in verticalJetsObjects) { vJet.SetActive(level == VisibilityLevel.LOSFull); }
      if (parent == null) { return; }
      if (isJumpjetsActive) {
        if(level == VisibilityLevel.LOSFull) {
          this.StartJumpjetAudio();
        } else {
          this.StopJumpjetAudio();
        }
      }
    }
    public void InitVisuals(CustomMechRepresentation parent, DataManager dataManager) {
      this.parent = parent;
      if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab) == false) {
        Log.WL(1, "spawning jump jets");
        try {
          if (this.parent.parentActor.GetCustomInfo().BossAppearAnimation) {
            this.AddBossJets();
          }
          if (MechFlyHeightController.JumpJetSrcPrefab == null) {
            MechFlyHeightController.JumpJetSrcPrefab = dataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
            if(MechFlyHeightController.JumpJetSrcPrefab == null) {
              Log.WL(2, "jumpJetSrcPrefab:" + (JumpJetSrcPrefab == null ? "null" : JumpJetSrcPrefab.name));
              return;
            }
            MechFlyHeightController.JumpJetSrcPrefab.SetActive(false);
          }
          Log.WL(2, "jumpJetSrcPrefab:" + (JumpJetSrcPrefab == null ? "null" : JumpJetSrcPrefab.name));
          if (JumpJetSrcPrefab != null) {
            JumpJetSrc = JumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
            Log.WL(2, "jumpJetSrc:" + (JumpJetSrc == null ? "null" : JumpJetSrc.name));
          }
          foreach (AirMechVerticalJetsDef vJet in parent.altDef.AirMechVerticalJets) {
            Log.WL(3, "attach:" + vJet.Attach + " prefab:" + vJet.Prefab + " JetsAttachPoints:" + vJet.JetsAttachPoints.Count);
            Transform attach = this.transform.FindRecursive(vJet.Attach);
            if (attach == null) { Log.WL(4, "attach is null"); continue; }
            GameObject vJetGO = dataManager.PooledInstantiate(vJet.Prefab, BattleTechResourceType.Prefab);
            if (vJetGO == null) { Log.WL(4, "prefab is null"); continue; }
            vJetGO.transform.SetParent(attach, false);
            if (JumpJetSrc == null) { Log.WL(4, "JumpJetSrc is null"); continue; }
            foreach (string jetSpwanPoint in vJet.JetsAttachPoints) {
              Log.WL(4, "jetSpwanPoint:" + jetSpwanPoint);
              Transform spawnJetPoint = vJetGO.transform.FindRecursive(jetSpwanPoint);
              if (spawnJetPoint == null) { Log.WL(5, "spawnJetPoint is null"); continue; }
              //this.DefaultFlyHeight = def.FlyHeight;
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
              jRep.Init(parent.parentMech, spawnJetPoint, true, false, parent.name);
              ParticleSystem[] psyss = jumpJetBase.GetComponentsInChildren<ParticleSystem>(true);
              foreach (ParticleSystem psys in psyss) {
                psys.RegisterRestoreScale();
                var main = psys.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                //main.loop = true;
                Log.WL(3, psys.name + ":" + psys.main.scalingMode);
              }
              verticalJets.Add(jRep);
            }
            verticalJetsObjects.Add(vJetGO);
            this.parent.RegisterRenderersCustomHeraldry(vJetGO, null);
            vJetGO.SetActive(false);
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
    }
    public void ForceHeight(float height) {
      if (parent == null) { return; }
      Vector3 localPos = parent.j_Root.transform.localPosition;
      localPos.y = height;
      parent.j_Root.transform.localPosition = localPos;
      isInChangeHeight = false;
      FPendingHeight = height;
      this.StartingHeight = height;
      this.FakeHeightControl = false;
    }
    public virtual void FinishChangeHeight() {
      if (this.isJumpjetsActive) {
        foreach (JumpjetRepresentation jet in verticalJets) { jet.SetState(JumpjetRepresentation.JumpjetState.Landing); }
        //foreach (GameObject jet in verticalJetsObjects) { jet.SetActive(false); }
        this.StopJumpjetAudio();
        isJumpjetsActive = false;
      }
      Log.TWL(0, "MechFlyHeightController.FinishChangeHeight "+ this.StartingHeight+"->"+ this.CurrentHeight);
      if (this.ForceJumpJetsActive ||((this.CurrentHeight <= Core.Settings.MaxHoveringHeightWithWorkingJets) && (this.StartingHeight > Core.Settings.MaxHoveringHeightWithWorkingJets))) {
        this.parent.OnJumpLand(true);
      }
      this.StartingHeight = this.CurrentHeight;
      this.FakeHeightControl = false;
      this.parent.parentActor.FakeHeightDelta(0f);
      foreach (var link in this.parent.custMech.linkedActors) {
        if (link.customMech != null) { continue; }
        Vector3 linkPos = link.rootHeightTransform.localPosition;
        linkPos.y = 0f;
        link.rootHeightTransform.localPosition = linkPos;
        link.actor.FakeHeightDelta(0f);
      }
      List<Action> events = this.heightChangeCompleteAction.ToList();
      heightChangeCompleteAction.Clear();
      foreach (Action onHeightChangeComleete in events) {
        onHeightChangeComleete();
      }
    }
    public void LateUpdate() {
      if (parent == null) { return; }
      if (isInChangeHeight == false) { return; }
      float delta = Mathf.Abs(parent.j_Root.transform.localPosition.y - this.PendingHeight);
      if (delta > Core.Epsilon) {

        float slowdown_factor = (delta / Mathf.Abs(this.StartingHeight - this.PendingHeight));
        if (slowdown_factor > 0.5f) {
          slowdown_factor = 1f;
        } else {
          slowdown_factor = 1f / ((0.5f - slowdown_factor) * 8f + 1f);
        }
        Log.WL(0, "HeightController.LateUpdate current height:" + parent.j_Root.transform.localPosition.y + " delta:"+delta+ " StartingHeight:"+ this.StartingHeight+ " PendingHeight:"+ PendingHeight+ " slowdown_factor:"+slowdown_factor);
        Vector3 localPos = parent.j_Root.transform.localPosition;
        float sign = localPos.y < this.PendingHeight ? this.UpSpeed : (this.DownSpeed * slowdown_factor);
        float ndelta = sign * Time.deltaTime;
        //Log.TWL(0,"Change height isInChange:"+this.isInChangeHeight+" cur:"+ localPos.y+" ndelta:"+ndelta+" delta:"+delta);
        if (Mathf.Abs(ndelta) >= delta) { localPos.y = this.PendingHeight; isInChangeHeight = false; } else { localPos.y += ndelta; }
        parent.j_Root.transform.localPosition = localPos;
        if (this.FakeHeightControl) {
          this.parent.parentActor.FakeHeightDelta(localPos.y);
          foreach(var link in this.parent.custMech.linkedActors) {
            if (link.customMech != null) { continue; }
            Vector3 linkPos = link.rootHeightTransform.localPosition;
            linkPos.y = localPos.y;
            link.rootHeightTransform.localPosition = linkPos;
            link.actor.FakeHeightDelta(localPos.y);
          }
        }
      } else {
        isInChangeHeight = false;
      }
      if(isInChangeHeight == false) {
        this.FinishChangeHeight();
      }
    }
  }
}