using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.UrbanWarfare;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class MultiShotLBXBulletEffect : CopyAbleWeaponEffect {
    public MultiShotLBXBallisticEffect parentProjector;
    public GameObject accurateProjectilePrefab;
    public GameObject inaccurateProjectilePrefab;
    public string preFireSoundEvent;
    public string projectileSoundEvent;
    public string fireCompleteStopEvent;
    public int volleySize;
    public int bulletIdx;
    /*public Color ppcTrail_color;
    public Color ppcBeamCoil_color;
    public Color zapArcFlip1_color;
    public Color PPCspark_color;
    public Color glowBlue_color;*/
    public override void StoreOriginalColor() {
      Log.M.TWL(0, "MultiShotLBXBulletEffect.StoreOriginalColor");
    }
    public override void SetColor(Color color) {
    }

#if PUBLIC_ASSEMBLIES
    public override int ImpactPrecacheCount {
#else
    protected override int ImpactPrecacheCount {
#endif
      get {
        return 5;
      }
    }
#if PUBLIC_ASSEMBLIES
    public override void Awake() {
#else
    protected override void Awake() {
#endif
      base.Awake();
    }
    protected override void Start() {
      base.Start();
    }
    public void Init(LBXEffect original) {
      base.Init(original);
      this.accurateProjectilePrefab = original.accurateProjectilePrefab;
      this.inaccurateProjectilePrefab = original.inaccurateProjectilePrefab;
      this.preFireSoundEvent = original.preFireSoundEvent;
      this.projectileSoundEvent = original.projectileSoundEvent;
      this.fireCompleteStopEvent = original.fireCompleteStopEvent;
      CustomAmmoCategoriesLog.Log.LogWrite("MultiShotLBXBulletEffect.Init\n");
    }
    public void Init(Weapon weapon, MultiShotLBXBallisticEffect parentProjector) {
      this.Init(weapon);
      this.parentProjector = parentProjector;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.subEffect = true;
    }
    public override void SetupCustomSettings() {
      this.customPrefireSFX = this.preFireSFX;
      switch (this.playSFX) {
        case PlaySFXType.First: this.customPrefireSFX = this.parentProjector.firstPreFireSFX; break;
        case PlaySFXType.Middle: this.customPrefireSFX = this.parentProjector.middlePrefireSFX; break;
        case PlaySFXType.Last: this.customPrefireSFX = this.parentProjector.lastPreFireSFX; break;
        case PlaySFXType.None: this.customPrefireSFX = string.Empty; break;
      }
      if (this.playSFX != PlaySFXType.None) {
        this.preFireStartSFX = weapon.preFireStartSFX();
        this.preFireStopSFX = weapon.preFireStopSFX();
        this.customPulseSFX = weapon.pulseSFX();
        this.customPulseSFXdelay = weapon.pulseSFXdelay();
        this.projectileFireSFX = weapon.projectileFireSFX();
        this.projectilePrefireSFX = weapon.projectilePreFireSFX();
        this.projectileStopSFX = weapon.projectileStopSFX();
        if (this.preFireStartSFX == null) { this.preFireStartSFX = string.Empty; }
        if (this.preFireStopSFX == null) { this.preFireStopSFX = string.Empty; }
        if (this.customPulseSFX == null) { this.customPulseSFX = string.Empty; }
        if (this.projectileFireSFX == null) { this.projectileFireSFX = this.projectileSoundEvent; }
        if (this.projectilePrefireSFX == null) { this.projectilePrefireSFX = this.preFireSoundEvent; }
        if (this.projectileStopSFX == null) { this.projectileStopSFX = this.fireCompleteStopEvent; }
        if (customPulseSFXdelay < CustomAmmoCategories.Epsilon) { this.customPulseSFXdelay = -1f; }
      } else {
        this.preFireStartSFX = string.Empty;
        this.preFireStopSFX = string.Empty;
        this.customPulseSFXdelay = 0f;
        this.customPulseSFX = string.Empty;
      }
      if (weapon.prefireDuration() > CustomAmmoCategories.Epsilon) {
        this.preFireDuration = weapon.prefireDuration();
      } else {
        this.preFireDuration = this.originalPrefireDuration;
      }
      if (weapon.ProjectileSpeed() > CustomAmmoCategories.Epsilon) {
        this.projectileSpeed = weapon.ProjectileSpeed();
      } else {
        this.projectileSpeed = this.originalProjectileSpeed;
      }
      this.projectileSpeed *= weapon.ProjectileSpeedMultiplier();
    }

    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0, int volleySize = 1) {
      Log.LogWrite("MultiShotLBXBulletEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + " volleySize:" + volleySize + "\n");
      this.volleySize = volleySize;
      if (hitInfo.DidShotHitChosenTarget(hitIndex)) {
        this.projectilePrefab = this.accurateProjectilePrefab;
      } else {
        this.projectilePrefab = this.inaccurateProjectilePrefab;
      }
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      this.SetupCustomSettings();
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if (this.projectileSpeed > CustomAmmoCategories.Epsilon) {
        this.duration = num / this.projectileSpeed;
      } else {
        this.duration = 1f;
      }
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
      this.t = 0.0f;
      //if (string.IsNullOrEmpty(this.preFireSFX)) {
      //  if (string.IsNullOrEmpty(this.preFireSoundEvent) == false) {
      //    int num = (int)WwiseManager.PostEvent(this.preFireSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //  }
      //}
    }
    public override void InitProjectile() {
      base.InitProjectile();
      //Log.LogWrite("MultiShotPulseEffect.InitProjectile\n");
      //Component[] components = this.projectile.GetComponentsInChildren<Component>();
      //foreach (Component component in components) {
      //  Log.LogWrite(" " + component.name + ":" + component.GetType().ToString() + "\n");
      //}
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      this.PlayMuzzleFlash();
      this.t = 0.0f;
      //if (string.IsNullOrEmpty(this.projectileSoundEvent) == false) {
      //  int num = (int)WwiseManager.PostEvent(this.projectileSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //}
      base.PlayProjectile();
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
          return;
        if ((double)this.t < 1.0) {
          this.UpdateColor();
        }
        if ((double)this.t < 1.0)
          return;
        int baseHitIndex = this.hitIndex;
        for (int index = 0; index < volleySize; ++index) {
          this.hitIndex = index + baseHitIndex;
          this.PlayImpact();
        }
        this.OnComplete();
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      Log.LogWrite("MultiShotLBXBulletEffect.OnImpact wi:" + this.hitInfo.attackWeaponIndex + " hi:" + this.hitInfo + " bi:" + this.bulletIdx + " volleySize:" + this.volleySize + "\n");
      float damage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
      float apDamage = this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
      if (this.weapon.DamagePerPallet() && (this.weapon.DamageNotDivided() == false)) {
        damage /= this.weapon.ProjectilesPerShot;
        apDamage /= this.weapon.ProjectilesPerShot;
      };
      base.OnImpact(damage, apDamage);
    }
    protected override void OnComplete() {
      base.OnComplete();
      if (this.projectileParticles != null) { this.projectileParticles.Stop(true); }
      //if (string.IsNullOrEmpty(this.fireCompleteStopEvent) == false) {
      //  int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //}
    }
    public override void Reset() {
      //if (this.Active && !string.IsNullOrEmpty(this.fireCompleteStopEvent)) {
      //  int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //}
      base.Reset();
    }
  }
}
