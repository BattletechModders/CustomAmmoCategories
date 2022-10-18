using BattleTech;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static BattleTech.AttackDirector;

namespace CustAmmoCategories {
  public class MultiShotLBXBallisticEffect : MuiltShotAnimatedEffect {
    private List<MultiShotLBXBulletEffect> bullets = new List<MultiShotLBXBulletEffect>();
    public static readonly string ImprovedLaserPrefabPrefix = "_IMPROVED_";
    public int currentVolley;
    public int volleySize;
    public int shotHitIndex;
    public string LBXEffectPrefab;
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
    protected override void Start() {
      base.Start();
    }
    public void Init(LBXEffect original, string prefab) {
      base.Init(original);
      this.attackSequenceNextDelayMin = 1f;
      this.attackSequenceNextDelayMax = 1.5f;
      this.preFireDuration = 1f / 1000f;
      this.preFireVFXPrefab = null;
      this.muzzleFlashVFXPrefab = null;
      this.projectilePrefab = null;
      this.projectile = new GameObject("DummyProjectile");
      this.impactVFXBase = "";
      this.impactVFXVariations = new string[0];
      this.armorDamageVFXName = string.Empty;
      this.structureDamageVFXName = string.Empty;
      this.terrainHitVFXBase = string.Empty;
      this.buildingHitOverlayVFXName = string.Empty;
      this.shotsDestroyFlimsyObjects = false;
      this.preFireSFX = string.Empty;
      Log.M.TWL(0, "MultiShotLBXBallisticEffect.Init");
      this.LBXEffectPrefab = prefab;
    }
    public override void Init(Weapon weapon) {
      Log.M.TWL(0,"MultiShotLBXBallisticEffect.BaseInit");
      base.Init(weapon);
    }
    public override void SetupCustomSettings() {
      this.customPrefireSFX = string.Empty;
      this.firstPreFireSFX = weapon.firstPreFireSFX();
      this.middlePrefireSFX = weapon.preFireSFX();
      this.lastPreFireSFX = weapon.lastPreFireSFX();
      //this.longPreFireSFX = weapon.longPreFireSFX();
      this.firingStartSFX = weapon.firingStartSFX();
      this.firingStopSFX = weapon.firingStopSFX();
      if (string.IsNullOrEmpty(firingStartSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(firingStartSFX)) {
          this.firingStopSFX = firingStartSFX;
        }
      }
      if (firingStartSFX == null) { firingStartSFX = string.Empty; }
      if (firingStopSFX == null) { firingStopSFX = string.Empty; }
      if (this.middlePrefireSFX == null) { this.middlePrefireSFX = this.preFireSFX; }
      if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.middlePrefireSFX; }
      if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.middlePrefireSFX; }
      this.fireSFX = string.Empty;
      this.middlefireSFX = weapon.fireSFX();
      this.firstFireSFX = weapon.firstFireSFX();
      this.lastFireSFX = weapon.lastFireSFX();
      if (this.middlefireSFX == null) { this.middlefireSFX = string.Empty; }
      if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.middlefireSFX; }
      if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.middlefireSFX; }
      this.preFireStartSFX = string.Empty;
      this.preFireStopSFX = string.Empty;
      this.customPulseSFX = string.Empty;
      this.customPulseSFXdelay = 0f;
      this.preFireDuration = 1f / 1000f;
      if (weapon.shotDelay() > CustomAmmoCategories.Epsilon) {
        this.shotDelay = weapon.shotDelay();
      } else {
        this.shotDelay = 0f;
      }
    }
    protected void SetupBulets() {
      Log.M.TWL(0, "MultiShotLBXBallisticEffect.SetupBeams");
      this.currentVolley = 0;
      this.shotHitIndex = 0;
      float effective_shotDelay = 0f;
      if (this.shotDelay < CustomAmmoCategories.Epsilon) {
        this.shotDelay = 0.5f * (1f + this.weapon.FireDelayMultiplier());
        effective_shotDelay = this.shotDelay;
      } else {
        effective_shotDelay = this.shotDelay * this.weapon.FireDelayMultiplier();
      }
      if (effective_shotDelay <= 0.5f) { effective_shotDelay = 0.5f; }
      this.duration = effective_shotDelay;
      this.rate = 1f / effective_shotDelay;
      Log.LogWrite(" projectileSpeed:" + projectileSpeed + " duration: " + this.duration + " FireDelayMultiplier: " + this.weapon.FireDelayMultiplier() + " shotDelay:" + shotDelay + " rate:" + this.rate + "\n");
      this.ClearBullets();
      int volleysCount = this.hitInfo.numberOfShots;
      this.volleySize = 1;
      if (this.weapon.DamagePerPallet() == true) {
        volleysCount /= weapon.ProjectilesPerShot;
        volleySize = weapon.ProjectilesPerShot;
        if ((volleysCount % weapon.ProjectilesPerShot) != 0) { volleysCount += 1; }
        Log.M.WL(2, "numberOfShots:" + this.hitInfo.numberOfShots + " ProjectilesPerShot:" + weapon.ProjectilesPerShot + " volleysCount:" + volleysCount);
      }
      string prefabName = MultiShotLBXBallisticEffect.ImprovedLaserPrefabPrefix + this.LBXEffectPrefab;
      Log.LogWrite("MultiShotLBXBallisticEffect.SetupBullets getting from pool:" + prefabName + "\n");
      for (int index = 0; index < volleysCount; ++index) {
        GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        MultiShotLBXBulletEffect msComponent = null;
        if (MultiShotGameObject != null) {
          Log.LogWrite(" getted from pool: " + MultiShotGameObject.GetInstanceID() + "\n");
          msComponent = MultiShotGameObject.GetComponent<MultiShotLBXBulletEffect>();
          if (msComponent != null) {
            msComponent.Init(this.weapon, this);
            this.bullets.Add(msComponent);
          }
        }
        if (msComponent == null) {
          Log.LogWrite(" not in pool. instansing.\n");
          GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.LBXEffectPrefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.LBXEffectPrefab), (UnityEngine.Object)this);
            break;
          }
          MultiShotGameObject = GameObject.Instantiate(gameObject);
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          } else {
            AutoPoolObject MultiShotAutoPoolObject = MultiShotGameObject.GetComponent<AutoPoolObject>();
            if (MultiShotAutoPoolObject != null) { GameObject.Destroy(MultiShotAutoPoolObject); };
          }
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.LBXEffectPrefab, 4f);
          gameObject = null;
          MultiShotGameObject.transform.parent = (Transform)null;
          LaserEffect component = MultiShotGameObject.GetComponent<LaserEffect>();
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.LBXEffectPrefab), (UnityEngine.Object)this);
            return;
          }
          msComponent = MultiShotGameObject.AddComponent<MultiShotLBXBulletEffect>();
          msComponent.Init(component);
          msComponent.Init(this.weapon, this);
          this.bullets.Add(msComponent);
        }
      }
    }
    protected void ClearBullets() {
      string prefabName = MultiShotLBXBallisticEffect.ImprovedLaserPrefabPrefix + this.LBXEffectPrefab;
      Log.LogWrite("MultiShotLBXBulletEffect.ClearBullets\n");
      for (int index = 0; index < this.bullets.Count; ++index) {
        this.bullets[index].Reset();
        GameObject gameObject = this.bullets[index].gameObject;
        Log.LogWrite(" returning to pool " + prefabName + " " + gameObject.GetInstanceID() + "\n");
        this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
      }
      this.bullets.Clear();
    }
    public bool AllBulletsComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.bullets.Count; ++index) {
        if (!this.bullets[index].FiringComplete)
          return false;
      }
      return true;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("MultiShotLBXBallisticEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + "\n");
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      this.SetupCustomSettings();
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      AttackSequence sequence = Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      this.SetupBulets();
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
      this.preFireRate = 1000f;
      this.preFireDuration = 1f / 1000f;
      if (this.attackSequenceNextDelayMin <= 0.0)
        this.attackSequenceNextDelayMin = 1f;
      if (this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = 1.5f;
      if (this.attackSequenceNextDelayMin >= this.attackSequenceNextDelayMax)
        this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    protected override void PlayMuzzleFlash() {
      //base.PlayMuzzleFlash();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void PlayProjectile() {
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      base.PlayProjectile(false);
      this.t = 1f;
      //this.FireNextShot();
    }
    protected override void FireNextShot() {
      if (this.currentVolley < 0 || this.currentVolley >= this.bullets.Count) { return; };
      base.FireNextShot();
      if (this.currentVolley == 0) {
        if (string.IsNullOrEmpty(this.firingStartSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(firingStartSFX)) {
            if (this.customParentAudioObject == null) {
              this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<CustomVoices.AudioObject>();
              if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<CustomVoices.AudioObject>(); }
            }
            this.customParentAudioObject.Play(firingStartSFX, true);
          } else {
            int num = (int)WwiseManager.PostEvent(firingStartSFX, this.parentAudioObject);
          }
        }
      }
      //this.PlayMuzzleFlash();
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      if (this.currentVolley >= this.bullets.Count) { return; };
      MultiShotLBXBulletEffect bullet = this.bullets[this.currentVolley];
      int bulletHitIndex = this.currentVolley * this.volleySize;
      int curVolleySize = Mathf.Min(this.volleySize,hitInfo.numberOfShots - bulletHitIndex);
      if (this.currentVolley == 0) { bullet.playSFX = PlaySFXType.First; } else { bullet.playSFX = PlaySFXType.Middle; }
      if (this.currentVolley >= (this.bullets.Count - 1)) { bullet.playSFX = PlaySFXType.Last; }
      bullet.Fire(this.hitInfo, bulletHitIndex, (this.currentVolley % this.numberOfEmitters), curVolleySize);
      ++this.currentVolley;
      this.t = 0.0f;
      if (this.currentVolley < this.bullets.Count) { return; }
      if (string.IsNullOrEmpty(this.firingStopSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(firingStopSFX)) {
          if (this.customParentAudioObject == null) {
            this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<CustomVoices.AudioObject>();
            if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<CustomVoices.AudioObject>(); }
          }
          this.customParentAudioObject.Play(firingStopSFX, true);
        } else {
          int num = (int)WwiseManager.PostEvent(firingStopSFX, this.parentAudioObject);
        }
      }
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayImpact() {
#else
    protected override void PlayImpact() {
#endif
      //base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
          return;
        this.OnComplete();
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(0.0f, 0f);
    }
    protected override void OnComplete() {
      base.OnComplete();
      this.ClearBullets();
    }
    public override void Reset() {
      for (int index = 0; index < this.bullets.Count; ++index) {
        this.bullets[index].Reset();
      }
      base.Reset();
    }
  }
}
