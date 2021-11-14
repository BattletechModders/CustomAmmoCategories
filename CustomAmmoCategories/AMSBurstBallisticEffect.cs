using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSBurstBallisticEffect : AMSWeaponEffect {
    public float impactTime = 0.5f;
    private int bulletsFired;
    public GameObject accurateProjectilePrefab;
    public GameObject inaccurateProjectilePrefab;
    public string preFireSoundEvent;
    public string projectileSoundEvent;
    public string fireCompleteStopEvent;
    private float floatieInterval;
    private float nextFloatie;
    public void Init(BurstBallisticEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("AMSBurstBallisticEffect.Init\n");
      this.impactTime = original.impactTime;
      this.bulletsFired = (int)typeof(BurstBallisticEffect).GetField("bulletsFired", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.accurateProjectilePrefab = original.accurateProjectilePrefab;
      this.inaccurateProjectilePrefab = original.inaccurateProjectilePrefab;
      this.preFireSoundEvent = original.preFireSoundEvent;
      this.projectileSoundEvent = original.projectileSoundEvent;
      this.fireCompleteStopEvent = original.fireCompleteStopEvent;
      this.floatieInterval = (float)typeof(BurstBallisticEffect).GetField("floatieInterval", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.nextFloatie = (float)typeof(BurstBallisticEffect).GetField("nextFloatie", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
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
#if PUBLIC_ASSEMBLIES
    public override void Start() {
#else
    protected override void Start() {
# endif
      base.Start();
      this.floatieInterval = 1f / (float)this.weapon.ProjectilesPerShot;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS burst ballistic effect can't fire normaly. Something is wrong.\n");
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      this.projectilePrefab = this.accurateProjectilePrefab;
      this.bulletsFired = 0;
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.nextFloatie = 0.0f;
      this.impactTime = Mathf.Clamp01(this.impactTime);
      this.duration = this.projectileSpeed;
      if ((double)this.duration > 4.0) { this.duration = 4f; };
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayPreFire() {
#else
    protected override void PlayPreFire() {
#endif
      base.PlayPreFire();
      this.t = 0.0f;
      if (string.IsNullOrEmpty(this.preFireSoundEvent))
        return;
      int num = (int)WwiseManager.PostEvent(this.preFireSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayMuzzleFlash() {
#else
    protected override void PlayMuzzleFlash() {
#endif
      base.PlayMuzzleFlash();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayProjectile() {
#else
    protected override void PlayProjectile() {
#endif
      this.PlayMuzzleFlash();
      this.t = 0.0f;
      if (!string.IsNullOrEmpty(this.projectileSoundEvent)) {
        int num = (int)WwiseManager.PostEvent(this.projectileSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.PlayProjectile();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayImpact() {
#else
    protected override void PlayImpact() {
#endif
      this.PlayImpactAudio();
      base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing) { return; }
      if ((double)this.t < 1.0) { return; };
      this.OnComplete();
    }
#if PUBLIC_ASSEMBLIES
    public override void OnPreFireComplete() {
#else
    protected override void OnPreFireComplete() {
#endif
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
#if PUBLIC_ASSEMBLIES
    public override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
#else
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
#endif
      base.OnImpact(hitDamage, structureDamage);
    }
#if PUBLIC_ASSEMBLIES
    public override void OnComplete() {
#else
    protected override void OnComplete() {
#endif
      base.OnComplete();
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)
        this.projectileParticles.Stop(true);
      if (string.IsNullOrEmpty(this.fireCompleteStopEvent))
        return;
      int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }
    public override void Reset() {
      if (this.Active && !string.IsNullOrEmpty(this.fireCompleteStopEvent)) {
        int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.Reset();
    }
  }
}
