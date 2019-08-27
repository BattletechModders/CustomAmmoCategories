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

namespace CustAmmoCategories {
  public class FragWeaponEffect : WeaponEffect {
    private static FieldInfo fi_hasSentNextWeaponMessage = null;
    public WeaponEffect parentWeaponEffect;
    public GameObject startingTransformObject;
    public FragWeaponEffect() {
      startingTransformObject = null;
    }
    public static Vector3 getMissPosition(GameRepresentation targetRep) {
      TurretRepresentation tRep = targetRep as TurretRepresentation;
      MechRepresentation mRep = targetRep as MechRepresentation;
      VehicleRepresentation vRep = targetRep as VehicleRepresentation;
      Vector3 position = targetRep.thisTransform.position;
      float radius = 1f;
      if (mRep != null) {
        position = mRep.vfxCenterTorsoTransform.position;
        position.y = mRep.vfxCenterTorsoTransform.position.y;
        radius = mRep.parentMech.MechDef.Chassis.Radius * UnityEngine.Random.Range(mRep.Constants.ResolutionConstants.MissOffsetHorizontalMin, mRep.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      } else
      if (tRep != null) {
        position = tRep.TurretLOS.position;
        radius = Random.Range(5f, 15f);
      } else
      if (vRep != null) {
        position = vRep.TurretLOS.position;
        radius = Random.Range(5f, 15f);
      }
      Vector2 vector2 = Random.insideUnitCircle.normalized * radius;
      position.x += vector2.x;
      position.z += vector2.y;
      return position;
    }
    public void Init(WeaponEffect original) {
      this.impactVFXBase = original.impactVFXBase;
      this.preFireSFX = original.preFireSFX;
      this.Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.hitInfo = original.hitInfo;
      this.hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.emitterIndex = (int)typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.subEffect = original.subEffect;
      this.currentState = original.currentState;
      this.weaponRep = original.weaponRep;
      this.weapon = original.weapon;
      this.parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.currentPos = (Vector3)typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
      this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = (float)typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
      if (fi_hasSentNextWeaponMessage != null) {
        this.hasSentNextWeaponMessage = (bool)fi_hasSentNextWeaponMessage.GetValue(original);
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
      }
      this.preFireDuration = original.preFireDuration;
      this.preFireRate = (float)typeof(WeaponEffect).GetField("preFireRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.duration = (float)typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.rate = (float)typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileSpeed = original.projectileSpeed;
      this.weaponImpactType = original.weaponImpactType;
      this.preFireVFXPrefab = original.preFireVFXPrefab;
      this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
      this.projectilePrefab = original.projectilePrefab;
      this.projectile = original.projectile;
      this.activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.impactVFXVariations = original.impactVFXVariations;
      this.armorDamageVFXName = original.armorDamageVFXName;
      this.structureDamageVFXName = original.structureDamageVFXName;
      this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
      this.FiringComplete = original.FiringComplete;
      this.AllowMissSkipping = original.AllowMissSkipping;
      this.parentWeaponEffect = null;
    }
    protected bool hasSentNextWeaponMessage {
      get {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          return (bool)fi_hasSentNextWeaponMessage.GetValue(this);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
          return false;
        }
      }
      set {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          fi_hasSentNextWeaponMessage.SetValue(this, value);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't set WeaponEffect.hasSentNextWeaponMessage\n");
        }
      }
    }
    public virtual void Fire(Vector3 sPos,WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("FragWeaponEffect.Fire " + sPos + " wi:" + hitInfo.attackWeaponIndex + " hi:" + hitIndex + "\n");
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
      if ((UnityEngine.Object)this.preFireVFXPrefab != (UnityEngine.Object)null) {
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
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
      if (!((UnityEngine.Object)this.muzzleFlashVFXPrefab != (UnityEngine.Object)null))
        return;
      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
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
      if (!((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null))
        return;
      componentInChildren.StopAnimation();
      componentInChildren.PlayAnimation();
    }

    protected override void PlayProjectile() {
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        this.projectileParticles.Stop(true);
        this.projectileParticles.Clear(true);
      }
      this.projectileTransform.position = this.startPos;
      this.projectileTransform.LookAt(this.endPos);
      //this.startPos = this.startingTransform.position;
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        BTCustomRenderer.SetVFXMultiplier(this.projectileParticles);
        this.projectileParticles.Play(true);
        BTLightAnimator componentInChildren = this.projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
        if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
          componentInChildren.StopAnimation();
          componentInChildren.PlayAnimation();
        }
      }
      if ((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) {
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
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
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
          if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
        }
      }
      this.PlayImpactDamageOverlay();
      if (this.hitInfo.hitLocations[this.hitIndex] == 65536) {
        this.PlayTerrainImpactVFX();
        this.DestroyFlimsyObjects();
      }
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(false);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(false);
      this.OnImpact(0.0f);
    }
  }
}
