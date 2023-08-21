// Decompiled with JetBrains decompiler
// Type: MissileLauncherEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A45ECDCD-AC3C-461F-86FA-6E4E161AEB1A
// Assembly location: C:\Games\steamapps\common\BATTLETECH\BattleTech_Data\Managed\Assembly-CSharp.dll

using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MultiShotMissileLauncherEffect : CopyAbleWeaponEffect {
  public static readonly string ImprovedMissilePrefabPrefix = "_IMPROVED_";
  public List<MultiShotMissileEffect> missiles = new List<MultiShotMissileEffect>();
  public GameObject MissilePrefab;
  public float firingInterval;
  private float firingIntervalRate;
  public float volleyInterval;
  private float volleyIntervalRate;
  public float missileSpreadAngle;
  public float missileCurveStrength;
  public int missileCurveFrequency;
  public bool isSRM;
  private bool isIndirect;
  public int VolleySize;
  public int missileVolleyIdx;
  protected override int ImpactPrecacheCount {
    get {
      return 14;
    }
  }
  protected override void Awake() {
    base.Awake();
    this.AllowMissSkipping = false;
  }
  protected override void Start() {
    base.Start();
  }
  public void Init(MissileLauncherEffect original) {
    base.Init(original);
    //Log.Combat?.TWL(1,$"MultiShotMissileLauncherEffect.Init");
    this.MissilePrefab = original.MissilePrefab;
    this.firingInterval = original.firingInterval;
    this.firingIntervalRate = 0f;
    this.volleyInterval = original.volleyInterval;
    this.volleyIntervalRate = 0f;
    //this.missileSpreadAngle = original.missileSpreadAngle;
    this.missileCurveStrength = original.missileCurveStrength;
    this.missileCurveFrequency = original.missileCurveFrequency;
    this.isSRM = original.isSRM;
    this.isIndirect = false;
    this.VolleySize = 0;
    this.missileVolleyIdx = 0;
  }

  public override void Init(Weapon weapon) {
    base.Init(weapon);
    if(weapon.MissileVolleySize() > 0) {
      this.VolleySize = weapon.MissileVolleySize();
    } else {
      this.VolleySize = this.numberOfEmitters;
    }
    this.missileVolleyIdx = 0;
    if (!(this.MissilePrefab != null))
      return;
    this.Combat.DataManager.PrecachePrefabAsync(this.MissilePrefab.name, BattleTechResourceType.Prefab, 1);
  }
  private void SetupMissiles() {
    this.emitterIndex = 0;
    this.missileVolleyIdx = 0;
    this.firingIntervalRate = (double)this.firingInterval <= 0.0 ? 0.0f : 1f / this.firingInterval;
    this.volleyIntervalRate = (double)this.volleyInterval <= 0.0 ? this.firingIntervalRate : 1f / this.volleyInterval;
    this.isIndirect = this.Combat.AttackDirector.GetAttackSequence(this.hitInfo.attackSequenceId).indirectFire;
    this.ClearMissiles();
    string prefabName = MultiShotMissileLauncherEffect.ImprovedMissilePrefabPrefix + this.MissilePrefab.name;
    Log.Combat?.TWL(0, $"MultiShotMissileLauncherEffect.SetupMissiles getting from pool:{prefabName}");
    for (int index = 0; index < this.hitInfo.numberOfShots; ++index) {
      GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      MultiShotMissileEffect msComponent = null;
      if (MultiShotGameObject != null) {
        Log.Combat?.WL(1,$"getted from pool: {MultiShotGameObject.GetInstanceID()}");
        msComponent = MultiShotGameObject.GetComponent<MultiShotMissileEffect>();
        if (msComponent != null) {
          msComponent.Init(this.weapon, this);
          this.missiles.Add(msComponent);
        }
      }
      if (msComponent == null) {
        Log.Combat?.WL(1, $"not in pool. instansing.");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.MissilePrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (gameObject == null) {
          Log.Combat?.WL(1, $"Error instantiating MissileObject {this.MissilePrefab.name}");
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
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.MissilePrefab.name, 4f);
        gameObject = null;
        MultiShotGameObject.transform.parent = (Transform)null;
        MissileEffect component = MultiShotGameObject.GetComponent<MissileEffect>();
        if (component == null) {
          WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.MissilePrefab.name), (UnityEngine.Object)this);
          return;
        }
        msComponent = MultiShotGameObject.AddComponent<MultiShotMissileEffect>();
        msComponent.Init(component);
        msComponent.Init(this.weapon, this);
        this.missiles.Add(msComponent);
      }
    }
  }
  private void ClearMissiles() {
    string prefabName = MultiShotMissileLauncherEffect.ImprovedMissilePrefabPrefix + this.MissilePrefab.name;
    Log.Combat?.TWL(0, $"MultiShotBallisticEffect.ClearBullets");
    for (int index = 0; index < this.missiles.Count; ++index) {
      this.missiles[index].Reset();
      GameObject gameObject = this.missiles[index].gameObject;
      Log.Combat?.WL(1, $"returning to pool {prefabName} {gameObject.GetInstanceID()}");
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
  public override void SetupCustomSettings() {
    this.customPrefireSFX = weapon.parentPreFireSFX();
    if (this.customPrefireSFX == null) { this.customPrefireSFX = string.Empty; }
    this.fireSFX = string.Empty;
    this.firstPreFireSFX = weapon.firstPreFireSFX();
    this.middlePrefireSFX = weapon.preFireSFX();
    this.lastPreFireSFX = weapon.lastPreFireSFX();
    this.firingStartSFX = weapon.firingStartSFX();
    this.firingStopSFX = weapon.firingStopSFX();
    if(string.IsNullOrEmpty(firingStartSFX) == false) {
      if (CustomVoices.AudioEngine.isInAudioManifest(firingStartSFX)) {
        this.firingStopSFX = firingStartSFX;
      }
    }
    if (firingStartSFX == null) { firingStartSFX = this.isSRM?"AudioEventList_srm_srm_launcher_start":"AudioEventList_lrm_lrm_launcher_start"; }
    if (firingStopSFX == null) { firingStopSFX = this.isSRM ? "AudioEventList_srm_srm_launcher_start" : "AudioEventList_lrm_lrm_launcher_start"; }
    if (this.middlePrefireSFX == null) { this.middlePrefireSFX = this.preFireSFX; }
    if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.middlePrefireSFX; }
    if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.middlePrefireSFX; }

    this.middlefireSFX = weapon.fireSFX();
    this.firstFireSFX = weapon.firstFireSFX();
    this.lastFireSFX = weapon.lastFireSFX();
    if (this.middlefireSFX == null) { this.middlefireSFX = this.isSRM ? "AudioEventList_srm_srm_missile_launch" : "AudioEventList_lrm_lrm_missile_launch"; }
    if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.middlefireSFX; }
    if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.isSRM? "AudioEventList_srm_srm_missile_launch_last": "AudioEventList_lrm_lrm_missile_launch_last"; }

    this.preFireStartSFX = string.Empty;
    this.preFireStopSFX = string.Empty;
    this.customPulseSFX = string.Empty;
    this.customPulseSFXdelay = 0f;
  }
  public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
    this.SetupCustomSettings();
    base.Fire(hitInfo, hitIndex, emitterIndex);
    this.SetupMissiles();
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
    this.FireNextMissile();
  }
  protected override void PlayImpact() {
    base.PlayImpact();
  }
  protected override void Update() {
    try {
      base.Update();
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t >= 1.0)
        this.FireNextMissile();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllMissilesComplete())
        return;
      this.OnComplete();
    }catch(Exception e) {
      Log.M?.TWL(0,e.ToString());
    }
  }
  private void FireNextMissile() {
    Log.Combat?.TWL(0,$"MultiShotMissileLauncherEffect.FireNextMissile hitIndex:{this.hitIndex}/{this.hitInfo.numberOfShots} volleyIndex:{this.missileVolleyIdx}/{this.VolleySize}");
    try {
      if (this.hitIndex < this.hitInfo.numberOfShots && this.missileVolleyIdx < this.VolleySize)
        this.LaunchMissile();
      if (this.hitIndex >= this.hitInfo.numberOfShots) {
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
      } else {
        if (this.missileVolleyIdx >= this.VolleySize) {
          this.FireNextVolley();
        }
      }
    }catch(Exception e) {
      Log.Combat.TWL(0,e.ToString(),true);
      WeaponEffect.logger.LogException(e);
    }
  }
  private void FireNextVolley() {
    Log.Combat?.TWL(0,"MultiShotMissileLauncherEffect.FireNextVolley");
    this.t = 0.0f;
    this.rate = this.volleyIntervalRate;
    this.missileVolleyIdx = 0;
    if (this.rate >= 0.01f)
      return;
    this.FireNextMissile();
  }
  private void LaunchMissile() {
    Log.Combat?.TWL(0, $"MultiShotMissileLauncherEffect.LaunchMissile hitIndex:{this.hitIndex}/{this.hitInfo.numberOfShots} volleyIndex:{this.missileVolleyIdx}/{this.VolleySize} emmiters: {this.emitterIndex}/{this.weaponRep.vfxTransforms.Length}");
    try {
      MultiShotMissileEffect missile = this.missiles[this.hitIndex] as MultiShotMissileEffect;
      missile.tubeTransform = this.weaponRep.vfxTransforms[this.emitterIndex];
      missile.playSFX = PlaySFXType.Middle;
      if (this.hitIndex == 0) { missile.playSFX = PlaySFXType.First; }
      if (this.hitIndex == (this.missiles.Count - 1)) { missile.playSFX = PlaySFXType.Last; }
      missile.FireEx(this.hitInfo, this.hitIndex, this.emitterIndex, this.isIndirect);
      if (this.hitIndex == 0) {
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
      ++this.hitIndex;
      ++this.missileVolleyIdx;
      this.emitterIndex = (this.emitterIndex + 1) % this.numberOfEmitters;
      this.t = 0.0f;
      this.rate = this.firingIntervalRate;
      if (this.rate >= 0.01f)
        return;
      this.FireNextMissile();
    }catch(Exception e) {
      Log.Combat?.TWL(0,e.ToString(),true);
      WeaponEffect.logger.LogException(e);
    }
  }
  protected override void OnPreFireComplete() {
    base.OnPreFireComplete();
    this.PlayProjectile();
  }
  protected override void OnImpact(float hitDamage = 0.0f,float structureDamage = 0f) {
    base.OnImpact(0.0f,0f);
  }
  protected override void OnComplete() {
    base.OnComplete();
    this.ClearMissiles();
  }
  public override void Reset() {
    base.Reset();
  }
}
