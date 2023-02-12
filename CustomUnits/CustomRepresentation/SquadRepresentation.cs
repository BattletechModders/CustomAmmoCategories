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
    public Dictionary<ChassisLocations, CustomMechRepresentation> squad { get; set; } = new Dictionary<ChassisLocations, CustomMechRepresentation>();
    public virtual CustomMechRepresentation currentLiveUnit { get; set; } = null;
    public override bool HasOwnVisuals { get { return false; } }
    public void AddUnit(CustomMechRepresentation unit) {
      int index = squad.Count;
      ChassisLocations location = TrooperSquad.locations[index];
      this.slaveRepresentations.Add(unit);
      this.squad.Add(location, unit);
      unit.isSlave = true;
      unit.SkipLateUpdate = false;
      if (unit.BlipObjectGhostStrong != null) { unit.UnregisterRenderers(unit.BlipObjectGhostStrong); GameObject.DestroyImmediate(unit.BlipObjectGhostStrong); unit.BlipObjectGhostStrong = null; }
      if (unit.BlipObjectGhostWeak != null) { unit.UnregisterRenderers(unit.BlipObjectGhostWeak); GameObject.DestroyImmediate(unit.BlipObjectGhostWeak); unit.BlipObjectGhostWeak = null; }
      if (unit.BlipObjectIdentified != null) { unit.UnregisterRenderers(unit.BlipObjectIdentified); GameObject.DestroyImmediate(unit.BlipObjectIdentified); unit.BlipObjectIdentified = null; }
      if (unit.BlipObjectUnknown != null) { unit.UnregisterRenderers(unit.BlipObjectUnknown); GameObject.DestroyImmediate(unit.BlipObjectUnknown); unit.BlipObjectUnknown = null; }
      unit.parentRepresentation = this;
      this.VisualObjects.Add(unit.VisibleObject);
      if (currentLiveUnit == null) {
        currentLiveUnit = unit;
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
        slave.Value.Init(this.custMech, this.j_Root, true);
        slave.Value.ApplyScale(unitScale);
      }
      Log.TWL(0, "SquadRepresentaion.InitSlaves");
      foreach (var slave in this.squad) {
        float xd = 0f;
        float yd = 0f;
        if (slave.Key != ChassisLocations.Head) {
          xd = Mathf.Cos(Mathf.Deg2Rad * TrooperSquad.positions[slave.Key]) * TrooperSquad.SquadRadius;
          yd = Mathf.Sin(Mathf.Deg2Rad * TrooperSquad.positions[slave.Key]) * TrooperSquad.SquadRadius;
        }
        Vector3 unitPos = new Vector3(xd, 0f, yd);
        Log.WL(1, slave.Key.ToString() + ":" + unitPos);
        slave.Value.transform.localPosition = unitPos;
      }
    }
    public override void _SetLoadAnimation() {
      base._SetLoadAnimation();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._SetLoadAnimation();
      }
    }
    public override void _ClearLoadState() {
      base._ClearLoadState();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._ClearLoadState();
      }
    }
    public override void PlayPersistentDamageVFX(int location) {
      if (this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentation unit)) {
        foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
          switch (chassisLocations) {
            case ChassisLocations.None:
            case ChassisLocations.Torso:
            case ChassisLocations.Arms:
            case ChassisLocations.MainBody:
            case ChassisLocations.Legs:
            case ChassisLocations.All: continue;
            default: unit.PlayPersistentDamageVFX((int)chassisLocations); continue;
          }
        }
      }
    }
    public override void CollapseLocation(int location, Vector3 attackDirection, bool loading = false) {
      if (this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentation unit)) {
        foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
          switch (chassisLocations) {
            case ChassisLocations.None:
            case ChassisLocations.Torso:
            case ChassisLocations.Arms:
            case ChassisLocations.MainBody:
            case ChassisLocations.Legs:
            case ChassisLocations.All: continue;
            default: unit.CollapseLocation(location, attackDirection, loading); continue;
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
      if (this.squad.TryGetValue(chassisLocation, out CustomMechRepresentation unit)) {
        if (this.parentMech.IsLocationDestroyed(chassisLocation)) {
          return this.parentMech.CurrentPosition + Vector3.up * this.HeightController.CurrentHeight;
        } else {
          return unit.GetHitPosition((int)ArmorLocation.CenterTorso);
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
      if (this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentation unit)) {
        return unit.GetVFXTransform((int)ChassisLocations.CenterTorso);
      }
      return base.GetVFXTransform(location);
    }
    public virtual void _PlayUnitDeathFloatie(DeathMethod deathMethod) {
      if (isSlave) { return; }
      string text = Localize.Strings.T("UNIT DESTROYED");
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentCombatant.GUID, this.parentCombatant.GUID, text, FloatieMessage.MessageNature.CriticalHit));
    }
    public class UnitDeathInfo {
      public Vector3 position { get; set; }
      public Quaternion rotation { get; set; }
      public UnitDeathInfo(Vector3 pos, Quaternion rot) {
        this.position = pos;
        this.rotation = rot;
      }
    }
    public virtual Dictionary<CustomMechRepresentation, UnitDeathInfo> deathPositionInfo { get; set; } = new Dictionary<CustomMechRepresentation, UnitDeathInfo>();
    public virtual void UpdateDeathPositionInfo(CustomMechRepresentation unitRep) {
      if (deathPositionInfo.ContainsKey(unitRep)) {
        deathPositionInfo[unitRep].position = unitRep.transform.position;
        deathPositionInfo[unitRep].rotation = unitRep.transform.rotation;
      } else {
        deathPositionInfo.Add(unitRep, new UnitDeathInfo(unitRep.transform.position, unitRep.transform.rotation));
      }
    }
    public virtual Vector3 getDeathPosition(CustomMechRepresentation unit) {
      if (deathPositionInfo.TryGetValue(unit, out UnitDeathInfo res)) {
        return res.position;
      }
      return unit.transform.position;
    }
    public virtual Quaternion getDeathRotation(CustomMechRepresentation unit) {
      if (deathPositionInfo.TryGetValue(unit, out UnitDeathInfo res)) {
        return res.rotation;
      }
      return unit.transform.rotation;
    }
    public override void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      this.CollapseLocation(location, attackDirection, false);
      if (this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentation unit)) {
        unit.HandleDeath(DeathMethod.CenterTorsoDestruction, (int)ChassisLocations.CenterTorso);
        UpdateDeathPositionInfo(unit);
        this._PlayUnitDeathFloatie(DeathMethod.CenterTorsoDestruction);
        if (this.currentLiveUnit == unit) {
          foreach (var liveunit in squad) {
            if (liveunit.Value.__IsDead) { continue; }
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
        unit.Value._SetMeleeIdleState(isMelee);
      }
    }
    public override void _TriggerMeleeTransition(bool meleeIn) {
      base._TriggerMeleeTransition(meleeIn);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._TriggerMeleeTransition(meleeIn);
      }
    }
    public override void SetRandomIdleValue(float value) {
      base.SetRandomIdleValue(value);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.SetRandomIdleValue(value);
      }
    }
    public override void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      base.PlayFireAnim(sourceLimb, recoilStrength);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayFireAnim(sourceLimb, recoilStrength);
      }
    }
    public override void PlayMeleeAnim(int meleeHeight, ICombatant target) {
      base.PlayMeleeAnim(meleeHeight, target);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayMeleeAnim(meleeHeight, target);
      }
    }
    public override void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      base.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      }
    }
    public override void _PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      base._PlayImpactAnimSimple(attackDirection, totalDamage);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._PlayImpactAnimSimple(attackDirection, totalDamage);
      }
    }
    public override void _SetUnsteadyAnim(bool isUnsteady) {
      base._SetUnsteadyAnim(isUnsteady);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._SetUnsteadyAnim(isUnsteady);
      }
    }
    public override void PlayKnockdownAnim(Vector2 attackDirection) {
      base.PlayKnockdownAnim(attackDirection);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayKnockdownAnim(attackDirection);
      }
    }
    public override void _ForceKnockdown(Vector2 attackDirection) {
      base._ForceKnockdown(attackDirection);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._ForceKnockdown(attackDirection);
      }
    }
    public override void _ResetHitReactFlags() {
      base._ResetHitReactFlags();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._ResetHitReactFlags();
      }
    }
    public override void PlayStandAnim() {
      base.PlayStandAnim();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayStandAnim();
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
        unit.Value.PlayJumpLaunchAnim();
      }
    }
    public override void PlayFallingAnim(Vector2 direction) {
      this.thisAnimator.SetTrigger("Fall");
      base._SetMeleeIdleState(false);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.PlayFallingAnim(direction);
      }
    }
    public override void UpdateJumpAirAnim(float forward, float side) {
      this.thisAnimator.SetFloat("InAir_Forward", forward);
      this.thisAnimator.SetFloat("InAir_Side", side);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.UpdateJumpAirAnim(forward, side);
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
        unit.Value.PlayJumpLandAnim(isDFA);
      }
    }
    public override void _StartJumpjetEffect() {
      if (isSlave == false) this._StartJumpjetAudio();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._StartJumpjetEffect();
      }
    }
    public override void _StopJumpjetEffect() {
      if (isSlave == false) this._StopJumpjetAudio();
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value._StopJumpjetEffect();
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
          unit.Value.OnPlayerVisibilityChangedCustom(newLevel);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public override void _ToggleHeadlights(bool headlightsActive) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { unit.Value._ToggleHeadlights(false); continue; }
        unit.Value._ToggleHeadlights(headlightsActive);
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
        unit.Value.PlayShutdownAnim();
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
        unit.Value.PlayStartupAnim();
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
        unit.Value.PilotableRepresentation_HandleDeath(deathMethod, location);
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
          if (unit.Value.__IsDead) { return; }
          unit.Value.PlayDeathVFX(deathMethod, location);
        }
      }
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) { this.StopManualPersistentVFX(stringList[index]); }
      this.__IsDead = true;
      foreach (var unit in this.squad) {
        stringList = new List<string>((IEnumerable<string>)unit.Value.persistentVFXParticles.Keys);
        for (int index = stringList.Count - 1; index >= 0; --index) { unit.Value.StopManualPersistentVFX(stringList[index]); }
        if (unit.Value.__IsDead == false) { unit.Value.__IsDead = true; }
      }
      if (deathMethod != DeathMethod.PilotKilled && !this.parentActor.WasEjected) {
        foreach (var unit in this.squad) {
          if (unit.Value.__IsDead) { continue; }
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
          unit.Value.PlayVFX(8, vfxName, true, Vector3.zero, false, -1f);
          float num = UnityEngine.Random.Range(25f, 30f);
          FootstepManager.Instance.AddScorch(unit.Value.transform.position, new Vector3(UnityEngine.Random.Range(0.0f, 1f), 0.0f, UnityEngine.Random.Range(0.0f, 1f)).normalized, new Vector3(num, num, num), true);
          unit.Value._ToggleHeadlights(false);
        }
      }
      this._ToggleHeadlights(false);
    }
    public override void _HandleDeathOnLoad(DeathMethod deathMethod, int location) {
      foreach (var unit in this.squad) {
        unit.Value._HandleDeathOnLoad(deathMethod, (int)ChassisLocations.CenterTorso);
      }
    }
    public override void _PlayDeathFloatie(DeathMethod deathMethod) {
      if (isSlave) { return; }
      string text = Localize.Strings.T("WASTED");
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentCombatant.GUID, this.parentCombatant.GUID, text, FloatieMessage.MessageNature.CriticalHit));
    }
    public override MechDestructibleObject GetDestructibleObject(int location) {
      MechDestructibleObject destructibleObject = (MechDestructibleObject)null;
      if (this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentation unit)) {
        destructibleObject = unit.GetDestructibleObject((int)ChassisLocations.CenterTorso);
      }
      return destructibleObject;
    }
    public override void InitWeapons(List<ComponentRepresentationInfo> compInfo, string parentDisplayName) {
      Log.TWL(0, "SquadRepresentation.InitWeapons");
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
        Log.WL(1, unit.Key.ToString() + " " + unit.Value.gameObject.name);
        if (squadComponents.TryGetValue(unit.Key, out List<ComponentRepresentationInfo> unitComponents)) {
          unit.Value.InitWeapons(unitComponents, parentDisplayName);
          foreach (ComponentRepresentationInfo comp in unitComponents) {
            Log.WL(2, comp.attachLocation + ":" + comp.component.defId + ":" + comp.component.baseComponentRef.prefabName);
          }
        }
      }
    }
    public override void UpdateSpline(ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.transform.position = getDeathPosition(unit.Value);
          unit.Value.transform.rotation = getDeathRotation(unit.Value);
        } else {
          unit.Value.UpdateSpline(sequence, Forward, t, meleeTarget);
        }
      }
    }
    public override void UpdateJumpFlying(MechJumpSequence sequence) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.transform.position = getDeathPosition(unit.Value);
          unit.Value.transform.rotation = getDeathRotation(unit.Value);
        } else {
          unit.Value.UpdateJumpFlying(sequence);
        }
      }
    }
    public override void CompleteJump(MechJumpSequence sequence) {
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) {
          unit.Value.transform.position = getDeathPosition(unit.Value);
          unit.Value.transform.rotation = getDeathRotation(unit.Value);
        } else {
          unit.Value.CompleteJump(sequence);
        }
      }
    }

    public override void Twist(float angle) {
      this.thisAnimator.SetFloat("Twist", angle);
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        unit.Value.Twist(angle);
      }
    }
    public override void _ToggleRandomIdles(bool shouldIdle) {
      if (this.parentMech.IsOrWillBeProne || !this.parentMech.IsOperational || this.parentMech.IsDead) { return; }
      base._allowRandomIdles = shouldIdle;
      Log.TWL(0, "SquadRepresentation._ToggleRandomIdles " + shouldIdle + " " + this.parentMech.MechDef.ChassisID);
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
        unit.Value._ToggleRandomIdles(shouldIdle);
      }
    }
    public override void OnCombatGameDestroyed() {
      base.OnCombatGameDestroyed();
      foreach (var unit in this.squad) { unit.Value.OnCombatGameDestroyed(); }
    }
    public override float TurnParam {
      set {
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HasTurnParam) { this.thisAnimator.SetFloat(this.TurnHash, value); }
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.TurnParam = value;
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
          unit.Value.ForwardParam = value;
        }
      }
    }
    public override bool IsMovingParam {
      set {
        if (this.HasIsMovingParam) { this.thisAnimator.SetBool(this.IsMovingHash, value); }
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.IsMovingParam = value;
        }
      }
    }
    public override bool BeginMovementParam {
      set {
        if (this.HasBeginMovementParam) { this.thisAnimator.SetTrigger(this.BeginMovementHash); };
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.BeginMovementParam = value;
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
        unit.Value.lastStateWasVisible = this.lastStateWasVisible;
      }
      if (this.lastStateWasVisible) { this.PlayMovementStartAudio(); }
    }
    public class MoveSquadContext : MoveContext {
      public Dictionary<ChassisLocations, MoveContext> squadHits { get; set; } = new Dictionary<ChassisLocations, MoveContext>();
      public MoveSquadContext() : base() {

      }
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
      this.CompleteMove(sequence, playedMelee, meleeTarget);
      RaycastHit? raycast = new RaycastHit?();
      bool aliginToTerrain = false;
      bool vehicleMovement = this.parentCombatant.NoMoveAnimation() || this.parentCombatant.FakeVehicle();
      if (vehicleMovement && this.parentCombatant.FlyingHeight() < Core.Settings.MaxHoveringHeightWithWorkingJets) {
        aliginToTerrain = true;
      }
      if (this.parentCombatant.NavalUnit() || (this.parentCombatant.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets)) {
        raycast = this.GetTerrainRayHit(finalPos, true);
      }
      if (raycast.HasValue) {
        this.thisTransform.position = raycast.Value.point;
        this.thisTransform.rotation = Quaternion.LookRotation(finalHeading, Vector3.up);
      } else {
        this.thisTransform.position = finalPos;
        this.thisTransform.rotation = Quaternion.LookRotation(finalHeading, Vector3.up);
      }
      if (aliginToTerrain) {
        foreach (var unit in this.squad) {
          LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
          if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
          unit.Value.AliginToTerrain(new RaycastHit?(), 100f, false);
        }
      }
    }

    public override void UpdateRotation(object context, Transform moveTransform, Vector3 forward, float deltaT) {
      if (forward.sqrMagnitude > Core.Epsilon) {
        moveTransform.LookAt(moveTransform.position + forward, Vector3.up);
      }
      MoveSquadContext rayhit = context as MoveSquadContext;
      Log.TWL(0,$"SquadRepresentation.UpdateRotation {this.chassisDef.Description.Id} pos:{this.transform.position}");
      foreach (var unit in this.squad) {
        LocationDamageLevel dmgLvl = this.parentMech.GetLocationDamageLevel(unit.Key);
        if ((dmgLvl == LocationDamageLevel.Destroyed) || (dmgLvl == LocationDamageLevel.NonFunctional)) { continue; }
        Vector3 squadpos = unit.Value.transform.position;
        MoveContext squadContext = unit.Value.GetMoveContext(ref squadpos);
        squadpos.y = unit.Value.transform.parent.position.y + (squadpos.y - this.transform.position.y);
        Log.WL(1,$"{unit.Value.transform.position} -> {squadpos} raycast:{squadContext.mainRayHit.HasValue}");
        unit.Value.transform.position = squadpos;
        unit.Value.UpdateRotation(squadContext, unit.Value.transform, forward, deltaT);
      }
    }

    //public override object UpdateSpline(Vector3 worldPos, ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
    //  object result = base.UpdateSpline(worldPos, sequence, Forward, t, meleeTarget);
    //  this.UpdateSplineSlaves(worldPos, result, sequence, Forward, t, meleeTarget);
    //  return result;
    //}
    public override void PlayVehicleTerrainImpactVFX(bool forcedSlave = false) {
      Log.TWL(0, "SquadRepresentation.PlayVehicleTerrainImpactVFX NoMoveAnimation:" + this.parentMech.NoMoveAnimation() + " FlyingHeight:" + this.parentMech.FlyingHeight() + " lastStateWasVisible:" + this.lastStateWasVisible);
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
        unit.Value.PlayVehicleTerrainImpactVFX(false);
      }
    }
    public override void ApplyScale(Vector3 sizeMultiplier) {
      Log.TWL(0, "SQUADS DOES NOT SUPPORT SCALING");
      return;
    }
  }
}