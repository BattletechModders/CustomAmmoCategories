using BattleTech;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSMissileLauncherEffect : AMSMultiShotWeaponEffect {
    public List<AMSMissileEffect> missiles = new List<AMSMissileEffect>();
    public GameObject MissilePrefab;
    public float firingInterval;
    private float firingIntervalRate;
    public float volleyInterval;
    private float volleyIntervalRate;
    //public float missileSpreadAngle;
    public float missileCurveStrength;
    public int missileCurveFrequency;
    public bool isSRM;
    public override float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      if (shotIdx < 0) { return curPath / pathLenth; }
      if (shotIdx >= this.missiles.Count) { return curPath / pathLenth; };
      return this.missiles[shotIdx].calculateInterceptCorrection(curPath, pathLenth, distance, missileProjectileSpeed);
    }
#if PUBLIC_ASSEMBLIES
    public override int ImpactPrecacheCount {
#else
    protected override int ImpactPrecacheCount {
#endif
      get {
        return 14;
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
    protected override void Start() {
      base.Start();
    }
    public virtual void Init(MissileLauncherEffect original) {
      base.Init(original);
      this.MissilePrefab = original.MissilePrefab;
      this.firingInterval = original.firingInterval;
      this.firingIntervalRate = (float)typeof(MissileLauncherEffect).GetField("firingIntervalRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.volleyInterval = original.volleyInterval;
      this.volleyIntervalRate = (float)typeof(MissileLauncherEffect).GetField("volleyIntervalRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      //this.missileSpreadAngle = original.missileSpreadAngle;
      this.missileCurveStrength = original.missileCurveStrength;
      this.missileCurveFrequency = original.missileCurveFrequency;
      this.isSRM = true;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (!((UnityEngine.Object)this.MissilePrefab != (UnityEngine.Object)null))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.MissilePrefab.name, BattleTechResourceType.Prefab, 1);
    }
    public override int BulletsCount() { return this.missiles.Count; }
    public override void AddBullet() {
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.MissilePrefab.name;
      Log.LogWrite("AMSMissileLauncherEffect.AddBullet getting from pool:" + prefabName + "\n");
      GameObject AMSgameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      AMSMissileEffect amsComponent = null;
      if (AMSgameObject != null) {
        Log.LogWrite(" getted from pool: " + AMSgameObject.GetInstanceID() + "\n");
        amsComponent = AMSgameObject.GetComponent<AMSMissileEffect>();
        if (amsComponent != null) {
          amsComponent.Init(this.weapon, this);
          this.missiles.Add(amsComponent);
        }
      }
      if (amsComponent == null) {
        Log.LogWrite(" not in pool. instansing.\n");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.MissilePrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error instantiating MissileObject " + this.MissilePrefab.name), (UnityEngine.Object)this);
          return;
        }
        AMSgameObject = GameObject.Instantiate(gameObject);
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        } else {
          AutoPoolObject AMSautoPoolObject = AMSgameObject.GetComponent<AutoPoolObject>();
          if (AMSautoPoolObject != null) { GameObject.Destroy(AMSautoPoolObject); };
        }
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.MissilePrefab.name, 4f);
        gameObject = null;
        AMSgameObject.transform.parent = (Transform)null;
        MissileEffect component = AMSgameObject.GetComponent<MissileEffect>();
        if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.MissilePrefab.name), (UnityEngine.Object)this);
          return;
        }
        amsComponent = AMSgameObject.AddComponent<AMSMissileEffect>();
        amsComponent.Init(component);
        amsComponent.Init(this.weapon, this);
        this.missiles.Add(amsComponent);
      }
    }
    public override void ClearBullets() {
      Log.LogWrite("AMSMissileLauncherEffect.ClearBullets\n");
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.MissilePrefab.name;
      for (int index = 0; index < this.missiles.Count; ++index) {
        GameObject gameObject = this.missiles[index].gameObject;
        Log.LogWrite(" returning to pool " + prefabName + " " + gameObject.GetInstanceID() + "\n");
        this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
      }
      this.missiles.Clear();
    }
    private bool AllMissilesComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.missiles.Count; ++index) {
        if (!this.missiles[index].FiringComplete)
          return false;
      }
      return true;
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("AMSMissileLauncherEffect.Fire\n");
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.firingIntervalRate = (double)this.firingInterval <= 0.0 ? 0.0f : 1f / this.firingInterval;
      this.volleyIntervalRate = (double)this.volleyInterval <= 0.0 ? this.firingIntervalRate : 1f / this.volleyInterval;
      this.FireMissile(hitIndex);
      this.PlayPreFire();
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS missile launcher effect can't fire normaly. Something is wrong.\n");
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
    }
    protected override void PlayImpact() {
      base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllMissilesComplete())
        return;
      this.OnComplete();
    }
    private void FireMissile(int missileIdx) {
      if (missileIdx < 0 || missileIdx >= this.missiles.Count) { return; }
      if (missileIdx == (this.missiles.Count - 1)) {
        if (this.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
        this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
      }
      AMSMissileEffect missile = this.missiles[missileIdx];
      int emitterIndex = missileIdx % this.numberOfEmitters;
      missile.tubeTransform = this.weaponRep.vfxTransforms[emitterIndex];
      missile.Fire(this.hitPositions, missileIdx, emitterIndex);
      if (missileIdx == 0) {
        if (this.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      }
      this.t = 0.0f;
      this.rate = this.firingIntervalRate;
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }

    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(hitDamage, structureDamage);
    }
    protected override void OnComplete() {
      base.OnComplete();
      this.ClearBullets();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
