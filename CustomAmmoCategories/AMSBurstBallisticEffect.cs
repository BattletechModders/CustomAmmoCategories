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
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing) { return; }
      if ((double)this.t < 1.0) { return; };
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f) {
      base.OnImpact(hitDamage);
    }
    protected override void OnComplete() {
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
