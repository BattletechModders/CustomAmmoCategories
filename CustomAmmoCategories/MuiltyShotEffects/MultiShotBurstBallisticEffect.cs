using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class MultiShotBurstBallisticEffect : CopyAbleWeaponEffect {
    public float impactTime = 0.5f;
    private int bulletsFired;
    public GameObject accurateProjectilePrefab;
    public GameObject inaccurateProjectilePrefab;
    public string preFireSoundEvent;
    public string projectileSoundEvent;
    public string fireCompleteStopEvent;
    private float floatieInterval;
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
    public override void Init(Weapon weapon) {
      this.Init(weapon);
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.floatieInterval = 1f / (float)this.weapon.ProjectilesPerShot;
    }
    public void Init(BurstBallisticEffect original) {
      base.Init(original);
      Log.Combat?.TWL(0,$"MultiShotBurstBallisticEffect.Init");
      this.impactTime = original.impactTime;
      this.bulletsFired = 0;
      this.accurateProjectilePrefab = original.accurateProjectilePrefab;
      this.inaccurateProjectilePrefab = original.inaccurateProjectilePrefab;
      this.preFireSoundEvent = original.preFireSoundEvent;
      this.projectileSoundEvent = original.projectileSoundEvent;
      this.fireCompleteStopEvent = original.fireCompleteStopEvent;
    }
    public override void SetupCustomSettings() {
      this.customPrefireSFX = this.preFireSFX;
      this.preFireStartSFX = string.Empty;
      this.preFireStopSFX = string.Empty;
      this.customPulseSFXdelay = 0f;
      this.customPulseSFX = string.Empty;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.TWL(0,$"MultiShotBurstBallisticEffect.Fire {hitInfo.attackWeaponIndex} {hitIndex} emitter:{emitterIndex} ep:{hitInfo.hitPositions[hitIndex]}");
      this.SetupCustomSettings();
      if (hitInfo.DidShotHitChosenTarget(hitIndex))
        this.projectilePrefab = this.accurateProjectilePrefab;
      else
        this.projectilePrefab = this.inaccurateProjectilePrefab;
      this.bulletsFired = 0;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.Combat?.WL(1,$"endPos restored:{this.endPos}");
      this.impactTime = Mathf.Clamp01(this.impactTime);
      this.duration = this.projectileSpeed;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    public override void InitProjectile() {
      base.InitProjectile();
      Log.Combat?.TWL(0, $"MultiShotBurstBallisticEffect.InitProjectile");
      Component[] components = this.projectile.GetComponentsInChildren<Component>();
      foreach (Component component in components) {
        Log.Combat?.WL(1, $"{component.name}:{component.GetType().ToString()}");
      }
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
      this.t = 0.0f;
      if (string.IsNullOrEmpty(this.preFireSoundEvent))
        return;
      int num = (int)WwiseManager.PostEvent(this.preFireSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      this.PlayMuzzleFlash();
      this.t = 0.0f;
      if (!string.IsNullOrEmpty(this.projectileSoundEvent)) {
        int num = (int)WwiseManager.PostEvent(this.projectileSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.PlayProjectile();
    }
    protected override void PlayImpact() {
      ++this.bulletsFired;
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.Firing || (double)this.t < 1.0)
          return;
        for (int index = 0; index < this.hitInfo.numberOfShots; ++index) {
          this.hitIndex = index;
          this.PlayImpact();
        }
        this.OnComplete();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
      }
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      float hitDamage1 = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
      float structureDamage1 = this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
      if (this.t >= 1f)
        base.OnImpact(hitDamage1, structureDamage1);
      else
        base.OnImpact(hitDamage, structureDamage);
    }
    protected override void OnComplete() {
      base.OnComplete();
      if (this.projectileParticles != null)
        this.projectileParticles.Stop(true);
      if (string.IsNullOrEmpty(this.fireCompleteStopEvent))
        return;
      int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }
    public void OnDisable() {
      if (this.projectileAudioObject == null)
        return;
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
      int num = (int)AkSoundEngine.UnregisterGameObj(this.projectileAudioObject.gameObject);
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
