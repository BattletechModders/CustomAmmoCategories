/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using BattleTech.Rendering;
using fastJSON;
using HBS.Logging;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using CustomAmmoCategoriesHelper;

namespace CustAmmoCategories {

  public class FragWeaponEffect : WeaponEffect {
    public WeaponEffect parentWeaponEffect;
    public GameObject startingTransformObject;
    public FragWeaponEffect() {
      startingTransformObject = null;
    }
    public void Init(WeaponEffect original) {
      this.impactVFXBase = original.impactVFXBase;
      this.preFireSFX = original.preFireSFX;
      this.Combat = original.Combat();
      this.hitInfo = original.hitInfo;
      this.hitIndex = original.hitIndex;
      this.emitterIndex = original.emitterIndex();
      this.numberOfEmitters = original.numberOfEmitters();
      this.subEffect = original.subEffect;
      this.currentState = original.currentState;
      this.weaponRep = original.weaponRep;
      this.weapon = original.weapon;
      this.parentAudioObject = original.parentAudioObject();
      this.startingTransform = original.startingTransform();
      this.startPos = original.startPos();
      this.endPos = original.endPos();
      this.currentPos = original.currentPos();
      this.t = original.t();
      this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
      this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = original.attackSequenceNextDelayTimer();
      this.hasSentNextWeaponMessage(original.hasSentNextWeaponMessage());
      this.preFireDuration = original.preFireDuration;
      this.preFireRate = original.preFireRate();
      this.duration = original.duration();
      this.rate = original.rate();
      this.projectileSpeed = original.projectileSpeed;
      this.weaponImpactType = original.weaponImpactType;
      this.preFireVFXPrefab = original.preFireVFXPrefab;
      this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
      this.projectilePrefab = original.projectilePrefab;
      this.projectile = original.projectile;
      this.activeProjectileName = original.activeProjectileName();
      this.projectileTransform = original.projectileTransform();
      this.projectileParticles = original.projectileParticles();
      this.projectileAudioObject = original.projectileAudioObject();
      this.projectileMeshObject = original.projectileMeshObject();
      this.projectileLightObject = original.projectileLightObject();
      Log.Combat?.WL(1, "projectile:" + (projectile == null ? "null" : projectile.name));
      Log.Combat?.WL(1, "projectilePrefab:" + (projectilePrefab == null ? "null" : projectilePrefab.name));
      Log.Combat?.WL(1, "activeProjectileName:" + activeProjectileName);
      if (projectilePrefab != null) {
        if (string.IsNullOrEmpty(this.activeProjectileName)) {
          this.activeProjectileName = projectilePrefab.name;
        }
      }
      this.impactVFXVariations = original.impactVFXVariations;
      this.armorDamageVFXName = original.armorDamageVFXName;
      this.structureDamageVFXName = original.structureDamageVFXName;
      this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
      this.FiringComplete = original.FiringComplete;
      this.AllowMissSkipping = original.AllowMissSkipping;
      this.parentWeaponEffect = null;
    }
    public virtual void Fire(Vector3 sPos,WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "FragWeaponEffect.Fire " + sPos + " wi:" + hitInfo.attackWeaponIndex + " hi:" + hitIndex);
      this.t = 0.0f;
      this.hitIndex = hitIndex;
      this.emitterIndex = emitterIndex;
      this.hitInfo = hitInfo;
      if(this.startingTransformObject == null) {
        this.startingTransformObject = new GameObject("FragEffectStartingTransformObject");
      }
      this.startingTransformObject.transform.position = sPos;
      this.startingTransformObject.transform.LookAt(sPos - this.weapon.weaponRep.vfxTransforms[emitterIndex].position);
      this.startingTransform = this.startingTransformObject.transform;
      this.startPos = sPos;
      this.endPos = hitInfo.hitPositions[hitIndex];
      this.currentPos = this.startPos;
      this.FiringComplete = false;
      this.InitProjectile();
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    protected override void PlayPreFire() {
      if (this.preFireVFXPrefab != null) {
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.preFireVFXPrefab.name, component);
        component.Stop(true);
        component.Clear(true);
        component.transform.parent = (Transform)null;
        component.transform.position = this.startPos;
        component.transform.LookAt(this.endPos);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
        if ((double)this.preFireDuration <= 0.0)
          this.preFireDuration = component.main.duration;
      }
      if (!string.IsNullOrEmpty(this.preFireSFX)) {
        int num = (int)WwiseManager.PostEvent(this.preFireSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      this.preFireRate = (double)this.preFireDuration <= 0.0 ? 1000f : 1f / this.preFireDuration;
      if ((double)this.attackSequenceNextDelayMin <= 0.0 && (double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = this.preFireDuration;
      if ((double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = 0.05f;
      if ((double)this.attackSequenceNextDelayMin >= (double)this.attackSequenceNextDelayMax)
        this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    protected override void PlayMuzzleFlash() {
      if (this.muzzleFlashVFXPrefab == null)
        return;
      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if (autoPoolObject ==null)
        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.muzzleFlashVFXPrefab.name, component);
      component.Stop(true);
      component.Clear(true);
      component.transform.parent = this.startingTransform;
      component.transform.localPosition = Vector3.zero;
      component.transform.LookAt(this.endPos);
      BTCustomRenderer.SetVFXMultiplier(component);
      component.Play(true);
      BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
      if (componentInChildren == null)
        return;
      componentInChildren.StopAnimation();
      componentInChildren.PlayAnimation();
    }
    protected override void PlayProjectile() {
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      if (this.projectileMeshObject != null)
        this.projectileMeshObject.SetActive(true);
      if (this.projectileLightObject != null)
        this.projectileLightObject.SetActive(true);
      if (this.projectileParticles != null) {
        this.projectileParticles.Stop(true);
        this.projectileParticles.Clear(true);
      }
      this.projectileTransform.position = this.startPos;
      this.projectileTransform.LookAt(this.endPos);
      //this.startPos = this.startingTransform.position;
      if (this.projectileParticles != null) {
        BTCustomRenderer.SetVFXMultiplier(this.projectileParticles);
        this.projectileParticles.Play(true);
        BTLightAnimator componentInChildren = this.projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
        if (componentInChildren != null) {
          componentInChildren.StopAnimation();
          componentInChildren.PlayAnimation();
        }
      }
      if (this.weapon.parent.GameRep != null) {
        int num;
        switch ((ChassisLocations)this.weapon.Location) {
          case ChassisLocations.LeftArm:
            num = 1;
            break;
          case ChassisLocations.RightArm:
            num = 2;
            break;
          default:
            num = 0;
            break;
        }
        this.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, this.weapon.weaponDef.AttackRecoil);
      }
      if (!this.AllowMissSkipping || this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536)
        return;
      this.PublishWeaponCompleteMessage();
    }
    protected override void PlayImpact() {
      if (!string.IsNullOrEmpty(this.impactVFXBase) && this.hitInfo.hitLocations[this.hitIndex] != 0) {
        string str1 = string.Empty;
        ICombatant combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId);
        if (this.hitInfo.hitLocations[this.hitIndex] != 65536 && combatantByGuid != null && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)combatantByGuid.ArmorForLocation(this.hitInfo.hitLocations[this.hitIndex]))
          str1 = "_crit";
        else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0)
          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (gameObject == null) {
          WeaponEffect.logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2));
        } else {
          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
          component.Stop(true);
          component.Clear(true);
          component.transform.position = this.endPos;
          component.transform.LookAt(this.startPos);
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
          BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
          if (componentInChildren != null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if (autoPoolObject == null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
        }
      }
      this.PlayImpactDamageOverlay();
      if (this.hitInfo.hitLocations[this.hitIndex] == 65536) {
        this.PlayTerrainImpactVFX();
        this.DestroyFlimsyObjects();
      }
      if (this.projectileMeshObject != null)
        this.projectileMeshObject.SetActive(false);
      if (this.projectileLightObject != null)
        this.projectileLightObject.SetActive(false);
      this.OnImpact(0.0f);
    }
  }
}
