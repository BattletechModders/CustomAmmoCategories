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
using BattleTech.Rendering;
using CustAmmoCategories;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class SquadRepresentation : CustomMechRepresentation {
    public class UnitDeathInfo {
      public Vector3 position { get; set; }
      public Quaternion rotation { get; set; }
      public UnitDeathInfo(Vector3 pos, Quaternion rot) {
        this.position = pos;
        this.rotation = rot;
      }
    }
    public class SlaveUnitInfo {
      public CustomMechRepresentation rep { get; set; }
      public ChassisLocations location { get; set; }
      public UnitDeathInfo deathInfo { get; set; }
      public Vector3 position { get; set; }
      public SlaveUnitInfo(CustomMechRepresentation r, ChassisLocations l) {
        this.rep = r;
        this.location = l;
        this.deathInfo = null;
      }
      public void UpdateDeathPositionInfo() {
        if (this.deathInfo != null) {
          deathInfo.position = rep.transform.position;
          deathInfo.rotation = rep.transform.rotation;
        } else {
          deathInfo = new UnitDeathInfo(rep.transform.position, rep.transform.rotation);
        }
      }
      public void UpdateDead() {
        if(this.deathInfo != null) {
          this.rep.transform.position = this.deathInfo.position;
          this.rep.transform.rotation = this.deathInfo.rotation;
        } else {
          this.UpdateDeathPositionInfo();
        }
      }
    }
    //public virtual Dictionary<CustomMechRepresentation, UnitDeathInfo> deathPositionInfo { get; set; } = new Dictionary<CustomMechRepresentation, UnitDeathInfo>();

    public Dictionary<ChassisLocations, SlaveUnitInfo> squad { get; set; } = new Dictionary<ChassisLocations, SlaveUnitInfo>();
    public virtual SlaveUnitInfo currentLiveUnit { get; set; } = null;
    public override bool HasOwnVisuals { get { return false; } }
    public void AddUnit(CustomMechRepresentation unit) {
      int index = squad.Count;
      ChassisLocations location = TrooperSquad.locations[index];
      this.slaveRepresentations.Add(unit);
      var unitInfo = new SlaveUnitInfo(unit, location);
      this.squad.Add(location, unitInfo);
      unit.isSlave = true;
      unit.SkipLateUpdate = false;
      if (unit.BlipObjectGhostStrong != null) { unit.UnregisterRenderers(unit.BlipObjectGhostStrong); GameObject.DestroyImmediate(unit.BlipObjectGhostStrong); unit.BlipObjectGhostStrong = null; }
      if (unit.BlipObjectGhostWeak != null) { unit.UnregisterRenderers(unit.BlipObjectGhostWeak); GameObject.DestroyImmediate(unit.BlipObjectGhostWeak); unit.BlipObjectGhostWeak = null; }
      if (unit.BlipObjectIdentified != null) { unit.UnregisterRenderers(unit.BlipObjectIdentified); GameObject.DestroyImmediate(unit.BlipObjectIdentified); unit.BlipObjectIdentified = null; }
      if (unit.BlipObjectUnknown != null) { unit.UnregisterRenderers(unit.BlipObjectUnknown); GameObject.DestroyImmediate(unit.BlipObjectUnknown); unit.BlipObjectUnknown = null; }
      unit.parentRepresentation = this;
      this.VisualObjects.Add(unit.VisibleObject);
      if (currentLiveUnit == null) {
        currentLiveUnit = unitInfo;
      }
      unit.VisibleObject.name = "unit_" + index;
      unit.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      switch (location) {
        case ChassisLocations.CenterTorso: this.vfxCenterTorsoTransform = unit.vfxCenterTorsoTransform; this.TorsoAttach = unit.TorsoAttach; break;
        case ChassisLocations.LeftTorso: this.vfxLeftTorsoTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.RightTorso: this.vfxRightTorsoTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.Head: this.vfxHeadTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.LeftArm: this.vfxLeftArmTransform = unit.vfxCenterTorsoTransform; this.vfxLeftShoulderTransform = unit.vfxCenterTorsoTransform; this.LeftArmAttach = unit.TorsoAttach; break;
        case ChassisLocations.RightArm: this.vfxRightArmTransform = unit.vfxCenterTorsoTransform; this.vfxRightShoulderTransform = unit.vfxCenterTorsoTransform; this.RightArmAttach = unit.TorsoAttach; break;
        case ChassisLocations.LeftLeg: this.vfxRightLegTransform = unit.vfxCenterTorsoTransform; this.RightLegAttach = unit.TorsoAttach; break;
        case ChassisLocations.RightLeg: this.vfxRightLegTransform = unit.vfxCenterTorsoTransform; this.RightLegAttach = unit.TorsoAttach; break;
      }
    }
    protected override void InitSlaves() {
      foreach (Collider collider in selfColliders) { collider.enabled = false; }
      UnitCustomInfo customInfo = this.chassisDef.GetCustomInfo();
      Vector3 unitScale = new Vector3(customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize, customInfo.SquadInfo.UnitSize);
      foreach (var slave in this.squad) {
        slave.Value.rep.Init(this.custMech, this.j_Root, true);
        slave.Value.rep.ApplyScale(unitScale);
      }
      Log.Combat?.TWL(0, "SquadRepresentaion.InitSlaves");
      foreach (var slave in this.squad) {
        float xd = 0f;
        float yd = 0f;
        if (slave.Key != ChassisLocations.Head) {
          xd = Mathf.Cos(Mathf.Deg2Rad * TrooperSquad.positions[slave.Key]) * TrooperSquad.SquadRadius;
          yd = Mathf.Sin(Mathf.Deg2Rad * TrooperSquad.positions[slave.Key]) * TrooperSquad.SquadRadius;
        }
        Vector3 unitPos = new Vector3(xd, 0f, yd);
        Log.Combat?.WL(1, slave.Key.ToString() + ":" + unitPos);
        slave.Value.rep.transform.localPosition = unitPos;
        slave.Value.position = unitPos;
      }
    }
    public override void _SetLoadAnimation() {
      base._SetLoadAnimation();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._SetLoadAnimation();
      }
    }
    public override void _ClearLoadState() {
      base._ClearLoadState();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._ClearLoadState();
      }
    }
    public override void PlayPersistentDamageVFX(int location) {
      if (this.squad.TryGetValue((ChassisLocations)location, out var unit)) {
        foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
          switch (chassisLocations) {
            case ChassisLocations.None:
            case ChassisLocations.Torso:
            case ChassisLocations.Arms:
            case ChassisLocations.MainBody:
            case ChassisLocations.Legs:
            case ChassisLocations.All: continue;
            default: unit.rep.PlayPersistentDamageVFX((int)chassisLocations); continue;
          }
        }
      }
    }
    public override void CollapseLocation(int location, Vector3 attackDirection, bool loading = false) {
      if (this.squad.TryGetValue((ChassisLocations)location, out var unit)) {
        foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
          switch (chassisLocations) {
            case ChassisLocations.None:
            case ChassisLocations.Torso:
            case ChassisLocations.Arms:
            case ChassisLocations.MainBody:
            case ChassisLocations.Legs:
            case ChassisLocations.All: continue;
            default: unit.rep.CollapseLocation(location, attackDirection, loading); continue;
          }
        }
      }
    }
    public override void _SetupDamageStates(Mech mech, MechDef mechDef) {
      foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
        switch (chassisLocations) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default:
          LocationDamageLevel locationDamageLevel = mech == null ? mechDef.GetLocationLoadoutDef(chassisLocations).DamageLevel : mech.GetLocationDamageLevel(chassisLocations);
          if (locationDamageLevel >= LocationDamageLevel.Penalized)
            this.PlayPersistentDamageVFX((int)chassisLocations);
          if (locationDamageLevel == LocationDamageLevel.Destroyed) {
            this.CollapseLocation((int)chassisLocations, Vector3.zero, mech.Combat.IsLoadingFromSave);
            continue;
          }
          continue;
        }
      }
    }
    public override Vector3 GetHitPosition(int location) {
      ChassisLocations chassisLocation = MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location);
      if (this.squad.TryGetValue(chassisLocation, out var unit)) {
        if (this.parentMech.IsLocationDestroyed(chassisLocation)) {
          return this.parentMech.CurrentPosition + Vector3.up * this.HeightController.CurrentHeight;
        } else {
          return unit.rep.GetHitPosition((int)ArmorLocation.CenterTorso);
        }
      }
      return base.GetHitPosition(location);
    }
    public override Vector3 GetMissPosition(Vector3 attackOrigin, Weapon weapon, NetworkRandom random) {
      Vector3 position = this.parentMech.CurrentPosition + Vector3.up * this.HeightController.CurrentHeight;
      float radius = this.parentMech.MechDef.Chassis.Radius;
      AttackDirection attackDirection = this.parentActor.Combat.HitLocation.GetAttackDirection(weapon.parent, (ICombatant)this.parentActor);
      bool flag = random.Int(max: 2) == 0;
      float num1 = random.Float(this.Constants.ResolutionConstants.MissOffsetHorizontalMin, this.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      if (weapon.Type == WeaponType.LRM) {
        Vector2 vector2 = random.Circle().normalized * (radius * num1);
        position.x += vector2.x;
        position.z += vector2.y;
        return position;
      }
      Vector3 vector3;
      switch (attackDirection) {
        case AttackDirection.FromFront:
        vector3 = !flag ? position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1 : position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1;
        break;
        case AttackDirection.FromLeft:
        vector3 = !flag ? position - this.thisTransform.forward * (radius * 0.6f) - this.thisTransform.forward * num1 : position + this.thisTransform.forward * (radius * 0.6f) + this.thisTransform.forward * num1;
        break;
        case AttackDirection.FromRight:
        vector3 = !flag ? position + this.thisTransform.forward * (radius * 0.6f) + this.thisTransform.forward * num1 : position - this.thisTransform.forward * (radius * 0.6f) - this.thisTransform.forward * num1;
        break;
        case AttackDirection.FromBack:
        vector3 = !flag ? position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1 : position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1;
        break;
        default:
        vector3 = !flag ? position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1 : position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1;
        break;
      }
      float num2 = random.Float(-this.Constants.ResolutionConstants.MissOffsetVerticalMin, this.Constants.ResolutionConstants.MissOffsetVerticalMax);
      vector3.y += num2;
      return vector3;
    }

    public override Transform GetVFXTransform(int location) {
      if (this.squad.TryGetValue((ChassisLocations)location, out var unit)) {
        return unit.rep.GetVFXTransform((int)ChassisLocations.CenterTorso);
      }
      return base.GetVFXTransform(location);
    }
    public virtual void _PlayUnitDeathFloatie(DeathMethod deathMethod) {
      if (isSlave) { return; }
      string text = Localize.Strings.T("UNIT DESTROYED");
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentCombatant.GUID, this.parentCombatant.GUID, text, FloatieMessage.MessageNature.CriticalHit));
    }
    public override void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      this.CollapseLocation(location, attackDirection, false);
      if (this.squad.TryGetValue((ChassisLocations)location, out var unit)) {
        unit.rep.HandleDeath(DeathMethod.CenterTorsoDestruction, (int)ChassisLocations.CenterTorso);
        unit.UpdateDeathPositionInfo();
        this._PlayUnitDeathFloatie(DeathMethod.CenterTorsoDestruction);
        if (this.currentLiveUnit == unit) {
          foreach (var liveunit in squad) {
            if (liveunit.Value.rep._IsDead) { continue; }
            this.currentLiveUnit = liveunit.Value; break;
          }
        }
      }
    }
    public override void _UpdateLegDamageAnimFlags(LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
    }
    public override void _SetMeleeIdleState(bool isMelee) {
      base._SetMeleeIdleState(isMelee);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._SetMeleeIdleState(isMelee);
      }
    }
    public override void _TriggerMeleeTransition(bool meleeIn) {
      base._TriggerMeleeTransition(meleeIn);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._TriggerMeleeTransition(meleeIn);
      }
    }
    public override void SetRandomIdleValue(float value) {
      base.SetRandomIdleValue(value);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.SetRandomIdleValue(value);
      }
    }
    public override void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      base.PlayFireAnim(sourceLimb, recoilStrength);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayFireAnim(sourceLimb, recoilStrength);
      }
    }
    public override void PlayMeleeAnim(int meleeHeight, ICombatant target) {
      base.PlayMeleeAnim(meleeHeight, target);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayMeleeAnim(meleeHeight, target);
      }
    }
    public override void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      base.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      }
    }
    public override void _PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      base._PlayImpactAnimSimple(attackDirection, totalDamage);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._PlayImpactAnimSimple(attackDirection, totalDamage);
      }
    }
    public override void _SetUnsteadyAnim(bool isUnsteady) {
      base._SetUnsteadyAnim(isUnsteady);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._SetUnsteadyAnim(isUnsteady);
      }
    }
    public override void PlayKnockdownAnim(Vector2 attackDirection) {
      base.PlayKnockdownAnim(attackDirection);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayKnockdownAnim(attackDirection);
      }
    }
    public override void _ForceKnockdown(Vector2 attackDirection) {
      base._ForceKnockdown(attackDirection);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._ForceKnockdown(attackDirection);
      }
    }
    public override void _ResetHitReactFlags() {
      base._ResetHitReactFlags();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._ResetHitReactFlags();
      }
    }
    public override void PlayStandAnim() {
      base.PlayStandAnim();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayStandAnim();
      }
    }
    public override void PlayJumpLaunchAnim() {
      this.isJumping = true;
      this.thisAnimator.SetTrigger("Jump");
      base._SetMeleeIdleState(false);
      if (isSlave == false) this._StartJumpjetAudio();
      if (isSlave == false) this.PlayVFXAt((Transform)null, this.parentActor.CurrentPosition, (string)this.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayJumpLaunchAnim();
      }
    }
    public override void PlayFallingAnim(Vector2 direction) {
      this.thisAnimator.SetTrigger("Fall");
      base._SetMeleeIdleState(false);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayFallingAnim(direction);
      }
    }
    public override void UpdateJumpAirAnim(float forward, float side) {
      this.thisAnimator.SetFloat("InAir_Forward", forward);
      this.thisAnimator.SetFloat("InAir_Side", side);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.UpdateJumpAirAnim(forward, side);
      }
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
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayJumpLandAnim(isDFA);
      }
    }
    public override void _StartJumpjetEffect() {
      if (isSlave == false) this._StartJumpjetAudio();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._StartJumpjetEffect();
      }
    }
    public override void _StopJumpjetEffect() {
      if (isSlave == false) this._StopJumpjetAudio();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._StopJumpjetEffect();
      }
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
        foreach (var unit in this.squad) {
          unit.Value.rep.OnPlayerVisibilityChangedCustom(newLevel);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
    public override void _ToggleHeadlights(bool headlightsActive) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { unit.Value.rep._ToggleHeadlights(false); continue; }
        unit.Value.rep._ToggleHeadlights(headlightsActive);
      }
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
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayShutdownAnim();
      }
    }
    public override void PlayStartupAnim() {
      GameRepresentation_PlayStartupAnim();
      if (isSlave == false) { int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerup, this.audioObject); }
      base._ToggleHeadlights(false);
      if (isSlave == false) {
        if (this.parentMech.team.LocalPlayerControlsTeam)
          AudioEventManager.PlayPilotVO(VOEvents.Mech_Power_Restart, (AbstractActor)this.parentMech);
        else
          AudioEventManager.PlayComputerVO(ComputerVOEvents.Mech_Powerup_Enemy);
      }
      this.thisAnimator.SetTrigger("PowerOn");
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayStartupAnim();
      }
    }
    public override void PlayDeathVFX(DeathMethod deathMethod, int location) {
      if (deathMethod == DeathMethod.PilotKilled) { return; }
      string deathCenterTorsoA = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
      AudioEventList_mech eventEnumValue = AudioEventList_mech.mech_cockpit_explosion;
      string vfxName;
      switch (deathMethod - 1) {
        case DeathMethod.NOT_SET:
        case DeathMethod.PilotKilled:
        vfxName = (string)this.Constants.VFXNames.mechDeath_cockpit;
        break;
        case DeathMethod.HeadDestruction:
        case DeathMethod.Unknown:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
          eventEnumValue = AudioEventList_mech.mech_destruction_a;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_B;
          eventEnumValue = AudioEventList_mech.mech_destruction_b;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_C;
          eventEnumValue = AudioEventList_mech.mech_destruction_c;
          break;
        }
        break;
        case DeathMethod.CenterTorsoDestruction:
        vfxName = (string)this.Constants.VFXNames.mechDeath_legs;
        break;
        case DeathMethod.LegDestruction:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_A;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_B;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_C;
          break;
        }
        break;
        case DeathMethod.AmmoExplosion:
        vfxName = (string)this.Constants.VFXNames.mechDeath_engine;
        break;
        case DeathMethod.EngineDestroyed:
        vfxName = (string)this.Constants.VFXNames.mechDeath_gyro;
        break;
        case DeathMethod.CockpitDestroyed:
        case DeathMethod.PilotEjectionNoMessage:
        vfxName = (string)this.Constants.VFXNames.mechDeath_vitalComponent;
        break;
        default:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
          eventEnumValue = AudioEventList_mech.mech_destruction_a;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_B;
          eventEnumValue = AudioEventList_mech.mech_destruction_b;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_C;
          eventEnumValue = AudioEventList_mech.mech_destruction_c;
          break;
        }
        break;
      }
      if (this.parentMech.Combat.IsLoadingFromSave) { return; }
      if (isSlave == false) {
        int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(eventEnumValue, this.audioObject);
      }
    }
    public override void HandleDeath(DeathMethod deathMethod, int location) {
      PilotableRepresentation_HandleDeath(deathMethod, location);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PilotableRepresentation_HandleDeath(deathMethod, location);
      }
      if (isSlave == false) this._PlayDeathFloatie(deathMethod);
      if (this.parentActor.WasDespawned) { return; }
      if (this.VisibleObjectLight != null) { this.VisibleObjectLight.SetActive(false); }
      this.thisAnimator.SetTrigger("Death");
      if (!this.parentMech.Combat.IsLoadingFromSave) {
        if (isSlave == false) {
          if (this.parentMech.team.LocalPlayerControlsTeam) {
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_destroyed");
          } else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam)) {
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_destroyed");
          }
        }
      }
      if (this.parentMech.IsOrWillBeProne || this.parentActor.WasEjected) { this.StartCoroutine(this.DelayProneOnDeath()); }
      if (!this.parentActor.WasEjected) {
        this.PlayDeathVFX(deathMethod, location);
        foreach (var unit in this.squad) {
          if (unit.Value.rep._IsDead) { return; }
          unit.Value.rep.PlayDeathVFX(deathMethod, location);
        }
      }
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) { this.StopManualPersistentVFX(stringList[index]); }
      this._IsDead = true;
      foreach (var unit in this.squad) {
        stringList = new List<string>((IEnumerable<string>)unit.Value.rep.persistentVFXParticles.Keys);
        for (int index = stringList.Count - 1; index >= 0; --index) { unit.Value.rep.StopManualPersistentVFX(stringList[index]); }
        if (unit.Value.rep._IsDead == false) { unit.Value.rep._IsDead = true; }
      }
      if (deathMethod != DeathMethod.PilotKilled && !this.parentActor.WasEjected) {
        foreach (var unit in this.squad) {
          if (unit.Value.rep._IsDead) { continue; }
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
          unit.Value.rep.PlayVFX(8, vfxName, true, Vector3.zero, false, -1f);
          float num = UnityEngine.Random.Range(25f, 30f);
          FootstepManager.Instance.AddScorch(unit.Value.rep.transform.position, new Vector3(UnityEngine.Random.Range(0.0f, 1f), 0.0f, UnityEngine.Random.Range(0.0f, 1f)).normalized, new Vector3(num, num, num), true);
          unit.Value.rep._ToggleHeadlights(false);
        }
      }
      this._ToggleHeadlights(false);
    }
    public override void _HandleDeathOnLoad(DeathMethod deathMethod, int location) {
      foreach (var unit in this.squad) {
        unit.Value.rep._HandleDeathOnLoad(deathMethod, (int)ChassisLocations.CenterTorso);
      }
    }
    public override void _PlayDeathFloatie(DeathMethod deathMethod) {
      if (isSlave) { return; }
      string text = Localize.Strings.T("WASTED");
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentCombatant.GUID, this.parentCombatant.GUID, text, FloatieMessage.MessageNature.CriticalHit));
    }
    public override MechDestructibleObject GetDestructibleObject(int location) {
      MechDestructibleObject destructibleObject = (MechDestructibleObject)null;
      if (this.squad.TryGetValue((ChassisLocations)location, out var unit)) {
        destructibleObject = unit.rep.GetDestructibleObject((int)ChassisLocations.CenterTorso);
      }
      return destructibleObject;
    }
    public override void InitWeapons(List<ComponentRepresentationInfo> compInfo, string parentDisplayName) {
      Log.Combat?.TWL(0, "SquadRepresentation.InitWeapons");
      Dictionary<ChassisLocations, List<ComponentRepresentationInfo>> squadComponents = new Dictionary<ChassisLocations, List<ComponentRepresentationInfo>>();
      UnitCustomInfo info = this.mech.MechDef.GetCustomInfo();
      foreach (ComponentRepresentationInfo winfo in compInfo) {
        if (squadComponents.TryGetValue(winfo.attachLocation, out List<ComponentRepresentationInfo> unitComponents) == false) {
          unitComponents = new List<ComponentRepresentationInfo>();
          squadComponents.Add(winfo.attachLocation, unitComponents);
        }
        ChassisLocations unitLocation = ChassisLocations.CenterTorso;
        if (winfo.component is Weapon weapon) {
          if (info.SquadInfo.Hardpoints.TryGetValue(weapon.WeaponCategoryValue.Name, out unitLocation)) {

          }
        }
        ComponentRepresentationInfo unitCompInfo = new ComponentRepresentationInfo(winfo.component, unitLocation, unitLocation, winfo.repType);
        unitComponents.Add(unitCompInfo);
      }
      foreach (var unit in this.squad) {
        Log.Combat?.WL(1, unit.Key.ToString() + " " + unit.Value.rep.gameObject.name);
        if (squadComponents.TryGetValue(unit.Key, out List<ComponentRepresentationInfo> unitComponents)) {
          unit.Value.rep.InitWeapons(unitComponents, parentDisplayName);
          foreach (ComponentRepresentationInfo comp in unitComponents) {
            Log.Combat?.WL(2, comp.attachLocation + ":" + comp.component.defId + ":" + comp.component.baseComponentRef.prefabName);
          }
        }
      }
    }
    public override object UpdateSpline(Vector3 worldPos, ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
      MoveContext raycast = this.GetMoveContext(ref worldPos, false);
      this.thisTransform.position = worldPos;
      this.UpdateSpline(sequence, Forward, t, meleeTarget);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.UpdateDead();
        } else {
          unit.Value.rep.UpdateSpline(worldPos+unit.Value.position, sequence, Forward, t, meleeTarget);
        }
      }
      return raycast;
    }

    public override void UpdateSpline(ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
    }
    public override void UpdateJumpFlying(MechJumpSequence sequence) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.UpdateDead();
        } else {
          unit.Value.rep.UpdateJumpFlying(sequence);
        }
      }
    }
    public override void CompleteJump(MechJumpSequence sequence) {
      //Vector3 finalpos = sequence.FinalPos;
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.UpdateDead();
        } else {
          //sequence.FinalPos = finalpos + unit.Value.position;
          //unit.Value.rep.CompleteJump(sequence);
          unit.Value.rep.ForcePositionToTerrain(sequence.FinalPos + unit.Value.position);
        }
      }
      //sequence.FinalPos = finalpos;
    }

    public override void Twist(float angle) {
      this.thisAnimator.SetFloat("Twist", angle);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.Twist(angle);
      }
    }
    public override void _ToggleRandomIdles(bool shouldIdle) {
      if (this.parentMech.IsOrWillBeProne || !this.parentMech.IsOperational || this.parentMech.IsDead) { return; }
      base._allowRandomIdles = shouldIdle;
      Log.Combat?.TWL(0, "SquadRepresentation._ToggleRandomIdles " + shouldIdle + " " + this.parentMech.MechDef.ChassisID);
      if (this._allowRandomIdles) { return; }
      if (this.IsInMeleeIdle) {
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateMeleeEntryHash, 0.15f);
      } else {
        if (this.previousAnimState.fullPathHash != this.idleStateEntryHash && this.previousAnimState.fullPathHash != this.idleStateFlavorsHash && this.previousAnimState.fullPathHash != this.idleStateUnsteadyHash)
          return;
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateEntryHash, 0.15f);
      }
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep._ToggleRandomIdles(shouldIdle);
      }
    }
    public override void OnCombatGameDestroyed() {
      base.OnCombatGameDestroyed();
      foreach (var unit in this.squad) { unit.Value.rep.OnCombatGameDestroyed(); }
    }
    public override float TurnParam {
      set {
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasTurnParam) { this.thisAnimator.SetFloat(this.TurnHash, value); }
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.rep.TurnParam = value;
        }
      }
    }
    public override float ForwardParam {
      set {
        //Log.TWL(0,"AltRepresentations.ForwardParam "+this.parentMech.MechDef.ChassisID+ " NoMoveAnimation:"+ this.parentMech.NoMoveAnimation());
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasForwardParam) { this.thisAnimator.SetFloat(this.ForwardHash, value); }
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.rep.ForwardParam = value;
        }
      }
    }
    public override bool IsMovingParam {
      set {
        if (this.HasIsMovingParam) { this.thisAnimator.SetBool(this.IsMovingHash, value); }
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.rep.IsMovingParam = value;
        }
      }
    }
    public override bool BeginMovementParam {
      set {
        if (this.HasBeginMovementParam) { this.thisAnimator.SetTrigger(this.BeginMovementHash); };
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.rep.BeginMovementParam = value;
        }
      }
    }
    public override void BeginMove(ActorMovementSequence sequence) {
      this.IsMovingParam = true;
      this.BeginMovementParam = true;
      this.DamageParam = 0f;
      this._SetMeleeIdleState(false);
      this.lastStateWasVisible = (this.rootParentRepresentation.VisibleObject.activeInHierarchy);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.lastStateWasVisible = this.lastStateWasVisible;
      }
      if (this.lastStateWasVisible) { this.PlayMovementStartAudio(); }
    }
    public class MoveSquadContext : MoveContext {
      public Dictionary<ChassisLocations, MoveContext> squadHits { get; set; } = new Dictionary<ChassisLocations, MoveContext>();
      public MoveSquadContext() : base() {

      }
    }
    public override void SetVisualHeight(float height) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.HeightController?.ForceHeight(height);
      }
    }
    public override void PendVisualHeight(float height) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.HeightController.PendingHeight = (height);
      }
    }
    public override float GetVisualHeight() {
      return this.currentLiveUnit.rep.HeightController.CurrentHeight;
    }
    public override void RegisterHeightChangeCompleteEvent(Action e) {
      this.currentLiveUnit.rep.HeightController.heightChangeCompleteAction.Add(e);
    }
    public override void ClearHeightChangeCompleteEvent() {
      this.currentLiveUnit.rep.HeightController.heightChangeCompleteAction.Clear();
    }

    public override MoveContext createMoveContext() {
      MoveSquadContext result = new MoveSquadContext();
      foreach(var unit in this.squad) { result.squadHits.Add(unit.Key, new MoveContext()); }
      return result;
    }
    //public virtual void UpdateSplineSlaves(Vector3 worldPos, object context, ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
    //  MoveSquadContext rayhit = context as MoveSquadContext;
    //  foreach (var unit in this.squad) {
    //    LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
    //    if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
    //    Vector3 squadpos = unit.Value.transform.position;
    //    squadpos.y = worldPos.y;
    //    rayhit.squadHits[unit.Key] = unit.Value.GetMoveContext(ref squadpos, sequence, Forward, t, meleeTarget);
    //    squadpos.y = unit.Value.transform.parent.position.y + (worldPos.y - squadpos.y);
    //    unit.Value.transform.position = squadpos;
    //  }
    //}
    public override void CompleteMove(Vector3 finalPos, Vector3 finalHeading, ActorMovementSequence sequence, bool playedMelee, ICombatant meleeTarget) {
      //this.CompleteMove(sequence, playedMelee, meleeTarget);
      //RaycastHit? raycast = new RaycastHit?();
      //bool aliginToTerrain = false;
      //bool vehicleMovement = this.parentCombatant.NoMoveAnimation() || this.parentCombatant.FakeVehicle();
      //if (vehicleMovement && this.parentCombatant.FlyingHeight() < Core.Settings.MaxHoveringHeightWithWorkingJets) {
      //  aliginToTerrain = true;
      //}
      //if (this.parentCombatant.NavalUnit() || (this.parentCombatant.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets)) {
      //  raycast = this.GetTerrainRayHit(finalPos, true);
      //}
      //if (raycast.HasValue) {
      //  this.thisTransform.position = raycast.Value.point;
      //  this.thisTransform.rotation = Quaternion.LookRotation(finalHeading, Vector3.up);
      //} else {
      this.thisTransform.position = finalPos;
      this.thisTransform.rotation = Quaternion.LookRotation(finalHeading, Vector3.up);
      //}
      //if (aliginToTerrain) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.ForcePositionToTerrain(finalPos + unit.Value.position);
      }
      //}
    }

    public override void UpdateRotation(object context, Transform moveTransform, Vector3 forward, float deltaT) {
      if (forward.sqrMagnitude > Core.Epsilon) {
        moveTransform.LookAt(moveTransform.position + forward, Vector3.up);
      }
      MoveSquadContext rayhit = context as MoveSquadContext;
      Log.Combat?.TWL(0,$"SquadRepresentation.UpdateRotation {this.chassisDef.Description.Id}:{moveTransform.name} pos:{moveTransform.position}");
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        Vector3 squadpos = moveTransform.position + unit.Value.position;
        MoveContext squadContext = unit.Value.rep.GetMoveContext(ref squadpos, true);
        //squadpos.y = unit.Value.transform.parent.position.y + (squadpos.y - this.transform.position.y);
        Log.Combat?.WL(1,$"squad pos:{moveTransform.position} -> unitpos:{(squadpos - moveTransform.position)} raycast:{(squadContext.mainRayHit.HasValue==false?"no raycast": squadContext.mainRayHit.Value.point.ToString())}");
        unit.Value.rep.transform.position = squadpos;
        unit.Value.rep.UpdateRotation(squadContext, unit.Value.rep.transform, forward, deltaT);
      }
    }

    //public override object UpdateSpline(Vector3 worldPos, ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
    //  object result = base.UpdateSpline(worldPos, sequence, Forward, t, meleeTarget);
    //  this.UpdateSplineSlaves(worldPos, result, sequence, Forward, t, meleeTarget);
    //  return result;
    //}
    public override void PlayVehicleTerrainImpactVFX(bool forcedSlave = false) {
      Log.Combat?.TWL(0, "SquadRepresentation.PlayVehicleTerrainImpactVFX NoMoveAnimation:" + this.parentMech.NoMoveAnimation() + " FlyingHeight:" + this.parentMech.FlyingHeight() + " lastStateWasVisible:" + this.lastStateWasVisible);
      if ((this.rootParentRepresentation.BlipDisplayed) || (this.VisibleObject.activeInHierarchy == false)) {
        if (this.lastStateWasVisible == false) { return; }
        this.lastStateWasVisible = false;
        this.PlayMovementStopAudio(forcedSlave);
      } else {
        if (this.lastStateWasVisible == false) {
          this.lastStateWasVisible = true;
          this.PlayMovementStartAudio(forcedSlave);
        }
      }
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.rep.PlayVehicleTerrainImpactVFX(false);
      }
    }
    public override void ApplyScale(Vector3 sizeMultiplier) {
      Log.Combat?.TWL(0, "SQUADS DOES NOT SUPPORT SCALING");
      return;
    }
  }
}