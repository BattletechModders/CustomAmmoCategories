using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSMultiShotWeaponEffect: AMSWeaponEffect {
    public virtual void AddBullet() { }
    public virtual void ClearBullets() { }
    public virtual int BulletsCount() { return 0; }
    public virtual float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      return curPath / pathLenth;
    }
  }
  public class AMSBallisticEffect : AMSMultiShotWeaponEffect {
    private List<AMSBulletEffect> bullets = new List<AMSBulletEffect>();
    public float shotDelay;
    public float spreadAngle;
    public GameObject bulletPrefab;
    public string AMSbulletPrefabName;
    public string firstShotSFX;
    public string middleShotSFX;
    public string lastShotSFX;
    public override float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      if (shotIdx < 0) { return curPath / pathLenth; }
      if (shotIdx >= this.bullets.Count) { return curPath / pathLenth; };
      return this.bullets[shotIdx].calculateInterceptCorrection(curPath, pathLenth, distance, missileProjectileSpeed);
    }
    public void Init(BallisticEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("AMSBallisticEffect.Init\n");
      this.shotDelay = original.shotDelay;
      this.spreadAngle = original.spreadAngle;
      this.bulletPrefab = original.bulletPrefab;
      this.AMSbulletPrefabName = "AMS_"+original.bulletPrefab.name;
      this.firstShotSFX = original.firstShotSFX;
      this.middleShotSFX = original.middleShotSFX;
      this.lastShotSFX = original.lastShotSFX;
    }
    protected override int ImpactPrecacheCount {
      get {
        return 5;
      }
    }
    protected override void Awake() {
      base.Awake();
      this.AllowMissSkipping = false;
    }
    protected override void Start() {
      base.Start();
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (!((UnityEngine.Object)this.bulletPrefab != (UnityEngine.Object)null))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.bulletPrefab.name, BattleTechResourceType.Prefab, weapon.ProjectilesPerShot);
    }
    public override int BulletsCount() { return this.bullets.Count; }
    public override void AddBullet() {
      GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
        WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
        return;
      }
      GameObject AMSgameObject = GameObject.Instantiate(gameObject);
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
      } else {
        AutoPoolObject AMSautoPoolObject = AMSgameObject.GetComponent<AutoPoolObject>();
        if (AMSautoPoolObject != null) { GameObject.Destroy(AMSautoPoolObject); };
      }
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.bulletPrefab.name, 4f);
      gameObject = null;
      AMSgameObject.transform.parent = (Transform)null;
      BulletEffect component = AMSgameObject.GetComponent<BulletEffect>();
      if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
        WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.bulletPrefab.name), (UnityEngine.Object)this);
        return;
      }
      AMSBulletEffect amsComponent = AMSgameObject.AddComponent<AMSBulletEffect>();
      amsComponent.Init(component);
      amsComponent.Init(this.weapon, this);
      this.bullets.Add(amsComponent);
    }
    public override void ClearBullets() {
      for (int index = 0; index < this.bullets.Count; ++index) {
        GameObject gameObject = this.bullets[index].gameObject;
        GameObject.DestroyObject(gameObject);
      }
      this.bullets.Clear();
    }
    protected bool AllBulletsComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.bullets.Count; ++index) {
        if (!this.bullets[index].FiringComplete)
          return false;
      }
      return true;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS ballistic effect can't fire normaly. Something is wrong.\n");
      base.Fire(hitInfo, hitIndex, emitterIndex);
      //this.SetupBullets();
      //this.PlayPreFire();
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      base.Fire(hitPositions, hitIndex, emitterIndex);
      if ((double)this.shotDelay <= 0.0) { this.shotDelay = 0.5f; };
      this.rate = 1f / this.shotDelay;
      this.FireBullet(hitIndex);
      this.PlayPreFire();
    }

    protected override void PlayPreFire() {
      base.PlayPreFire();
    }

    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      base.PlayProjectile();
      //this.FireNextBullet();
    }
    protected void FireBullet(int bulletIdx) {
      if (bulletIdx < 0 || bulletIdx >= this.bullets.Count)
        return;
      AMSBulletEffect bullet = this.bullets[bulletIdx];
      bullet.bulletIdx = bulletIdx;
      bullet.Fire(this.hitPositions, bulletIdx, 0);
      this.PlayMuzzleFlash();
      string empty = string.Empty;
      string eventName = bulletIdx != 0 ? (bulletIdx != this.bullets.Count - 1 ? this.middleShotSFX : this.lastShotSFX) : this.firstShotSFX;
      if (!string.IsNullOrEmpty(eventName)) {
        int num = (int)WwiseManager.PostEvent(eventName, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      this.t = 0.0f;
      if (bulletIdx < this.bullets.Count) {
        return;
      }
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
    protected override void PlayImpact() {
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
        return;
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f) {
      if ((double)hitDamage <= 1.0 / 1000.0)
        return;
      base.OnImpact(hitDamage);
    }
    public void OnBulletImpact(BulletEffect bullet) {
      this.OnImpact(0.0f);
    }
    protected override void OnComplete() {
      this.OnImpact(0.0f);
      base.OnComplete();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
