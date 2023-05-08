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
    public override void StoreOriginalColor() {
      Log.Combat?.TWL(0, "MultiShotLBXBulletEffect.StoreOriginalColor");
    }
    public override void SetColor(Color color) {
    }
    protected override int ImpactPrecacheCount {
      get {
        return 5;
      }
    }
    protected override void Awake() {
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
      Log.Combat?.TWL(0,"MultiShotLBXBulletEffect.Init");
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
      Log.Combat?.TWL(0,$"MultiShotLBXBulletEffect.Fire {hitInfo.attackWeaponIndex} {hitIndex} ep:{hitInfo.hitPositions[hitIndex]} volleySize:{volleySize}");
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
      Log.Combat?.WL(1,$"endPos restored:{this.endPos}");
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
    }
    public override void InitProjectile() {
      base.InitProjectile();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      this.PlayMuzzleFlash();
      this.t = 0.0f;
      base.PlayProjectile();
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
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
      Log.Combat?.TWL(0,$"MultiShotLBXBulletEffect.OnImpact wi:{this.hitInfo.attackWeaponIndex} hi:{this.hitInfo} bi:{this.bulletIdx} volleySize:{this.volleySize}");
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
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
