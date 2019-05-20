using BattleTech;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class AMSBulletEffect : AMSWeaponEffect {
    public int bulletIdx;
    private AMSBallisticEffect parentLauncher;
    public void Init(BulletEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("AMSBulletEffect.Init\n");
      this.bulletIdx = original.bulletIdx;
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

    public void Init(Weapon weapon, AMSBallisticEffect parentLauncher) {
      this.Init(weapon);
      this.parentLauncher = parentLauncher;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.projectileSpeed = parentLauncher.projectileSpeed;
    }

    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("AMSBulletEffect.Fire\n");
      base.Fire(hitPositions, hitIndex, emitterIndex);
      Vector3 endPos = this.endPos;
      endPos.x += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.y += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.z += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      this.endPos = endPos;
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS ballistic effect can't fire normaly. Something is wrong.\n");
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }

    protected override void PlayPreFire() {
      base.PlayPreFire();
    }

    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }

    protected override void PlayProjectile() {
      base.PlayProjectile();
      this.PlayMuzzleFlash();
    }

    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }

    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.startPos, this.endPos, this.t);
        this.projectileTransform.position = this.currentPos;
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }

    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }

    protected override void OnImpact(float hitDamage = 0.0f) {
      //this.parentLauncher.OnBulletImpact(this);
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; }
      this.projectileParticles.Stop(true);
    }

    protected override void OnComplete() {
      base.OnComplete();
    }

    public void OnDisable() {
      if (!((UnityEngine.Object)this.projectileAudioObject != (UnityEngine.Object)null))
        return;
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
      int num = (int)AkSoundEngine.UnregisterGameObj(this.projectileAudioObject.gameObject);
    }

    public override void Reset() {
      base.Reset();
    }
  }
}
