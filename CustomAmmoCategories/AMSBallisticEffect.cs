using BattleTech;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSMultiShotWeaponEffect: AMSWeaponEffect {
    public static readonly string AMSPrefabPrefix = "_AMS_";
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
      this.AllowMissSkipping = false;
    }
#if PUBLIC_ASSEMBLIES
    public override void Start() {
#else
    protected override void Start() {
# endif
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
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.bulletPrefab.name;
      Log.LogWrite("AMSBallisticEffect.AddBullet getting from pool:" + prefabName+"\n");
      GameObject AMSgameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      AMSBulletEffect amsComponent = null;
      if (AMSgameObject != null) {
        Log.LogWrite(" getted from pool: "+AMSgameObject.GetInstanceID()+"\n");
        amsComponent = AMSgameObject.GetComponent<AMSBulletEffect>();
        if (amsComponent != null) {
          amsComponent.Init(this.weapon, this);
          this.bullets.Add(amsComponent);
        }
      }
      if (amsComponent == null) {
        Log.LogWrite(" not in pool. instansing.\n");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
          return;
        }
        AMSgameObject = GameObject.Instantiate(gameObject);
        //AMSgameObject.name = prefabName;
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
        amsComponent = AMSgameObject.AddComponent<AMSBulletEffect>();
        amsComponent.Init(component);
        amsComponent.Init(this.weapon, this);
        this.bullets.Add(amsComponent);
      }
    }
    public override void ClearBullets() {
      Log.LogWrite("AMSBulletEffect.ClearBullets\n");
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.bulletPrefab.name;
      for (int index = 0; index < this.bullets.Count; ++index) {
        GameObject gameObject = this.bullets[index].gameObject;
        Log.LogWrite(" returning to pool "+prefabName+" "+gameObject.GetInstanceID()+"\n");
        this.Combat.DataManager.PoolGameObject(prefabName,gameObject);
        //GameObject.DestroyObject(gameObject);
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
#if PUBLIC_ASSEMBLIES
    public override void PlayPreFire() {
#else
    protected override void PlayPreFire() {
#endif
      base.PlayPreFire();
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
#if PUBLIC_ASSEMBLIES
    public override void PlayImpact() {
#else
    protected override void PlayImpact() {
#endif
      base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
        return;
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
      if ((double)hitDamage <= 1.0 / 1000.0)
        return;
      base.OnImpact(hitDamage, structureDamage);
    }
    public void OnBulletImpact(BulletEffect bullet) {
      this.OnImpact(0.0f);
    }
#if PUBLIC_ASSEMBLIES
    public override void OnComplete() {
#else
    protected override void OnComplete() {
#endif
      this.OnImpact(0.0f);
      base.OnComplete();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
