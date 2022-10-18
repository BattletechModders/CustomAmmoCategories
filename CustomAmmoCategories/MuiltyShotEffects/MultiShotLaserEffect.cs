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
  public class MultiShotLaserEffect : MuiltShotAnimatedEffect {
    private List<MultiShotBeamEffect> beams = new List<MultiShotBeamEffect>();
    public static readonly string ImprovedLaserPrefabPrefix = "_IMPROVED_";
    public int currentBeam;
    public int beamHitIndex;
    public string LaserEffectPrefab;
    public float spreadAngle = 0.5f;


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
    public void Init(LaserEffect original, string prefab) {
      base.Init(original);
      this.attackSequenceNextDelayMin = 0f;
      this.attackSequenceNextDelayMax = 0f;
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
      this.customPrefireSFX = string.Empty;
      this.LaserEffectPrefab = prefab;
    }
    public override void Init(Weapon weapon) {
      Log.LogWrite("MultiShotLaserEffect.BaseInit\n");
      base.Init(weapon);
    }
    protected void SetupBeams() {
      Log.LogWrite("MultiShotLaserEffect.SetupBeams\n");
      this.currentBeam = 0;
      this.beamHitIndex = 0;
      float effective_shotDelay = 0f;
      if (this.shotDelay <= CustomAmmoCategories.Epsilon) {
        this.shotDelay = this.projectileSpeed * (1f + this.weapon.FireDelayMultiplier());
        effective_shotDelay = this.shotDelay;
      } else {
        effective_shotDelay = this.shotDelay * this.weapon.FireDelayMultiplier();
      }
      if (effective_shotDelay <= 0.5f) { effective_shotDelay = 0.5f; }
      this.duration = effective_shotDelay;
      this.rate = 1f / effective_shotDelay;

      Log.LogWrite(" projectileSpeed:" + projectileSpeed + " duration: " + this.duration + " FireDelayMultiplier: " + this.weapon.FireDelayMultiplier() + " shotDelay:" + shotDelay + " rate:" + this.rate + "\n");
      this.ClearBeams();
      int beamsCount = this.hitInfo.numberOfShots;
      if (this.weapon.DamagePerPallet() == false) {
        beamsCount *= weapon.ProjectilesPerShot;
      }
      string prefabName = MultiShotLaserEffect.ImprovedLaserPrefabPrefix + this.LaserEffectPrefab;
      Log.LogWrite("MultiShotLaserEffect.SetupBullets getting from pool:" + prefabName + "\n");
      for (int index = 0; index < beamsCount; ++index) {
        GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        MultiShotBeamEffect msComponent = null;
        if (MultiShotGameObject != null) {
          Log.LogWrite(" getted from pool: " + MultiShotGameObject.GetInstanceID() + "\n");
          msComponent = MultiShotGameObject.GetComponent<MultiShotBeamEffect>();
          if (msComponent != null) {
            msComponent.Init(this.weapon, this);
            this.beams.Add(msComponent);
          }
        }
        if (msComponent == null) {
          Log.LogWrite(" not in pool. instansing.\n");
          GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.LaserEffectPrefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.LaserEffectPrefab), (UnityEngine.Object)this);
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
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.LaserEffectPrefab, 4f);
          gameObject = null;
          MultiShotGameObject.transform.parent = (Transform)null;
          LaserEffect component = MultiShotGameObject.GetComponent<LaserEffect>();
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.LaserEffectPrefab), (UnityEngine.Object)this);
            return;
          }
          msComponent = MultiShotGameObject.AddComponent<MultiShotBeamEffect>();
          msComponent.Init(component);
          msComponent.Init(this.weapon, this);
          this.beams.Add(msComponent);
        }
      }
    }
    protected void ClearBeams() {
      string prefabName = MultiShotLaserEffect.ImprovedLaserPrefabPrefix + this.LaserEffectPrefab;
      Log.LogWrite("MultiShotLaserEffect.ClearBullets\n");
      for (int index = 0; index < this.beams.Count; ++index) {
        this.beams[index].Reset();
        GameObject gameObject = this.beams[index].gameObject;
        Log.LogWrite(" returning to pool " + prefabName + " " + gameObject.GetInstanceID() + "\n");
        this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
      }
      this.beams.Clear();
    }
    public bool AllBeamsComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.beams.Count; ++index) {
        if (!this.beams[index].FiringComplete)
          return false;
      }
      return true;
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
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("MultiShotLazerEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + "\n");
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      this.SetupCustomSettings();
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      AttackSequence sequence = Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      this.SetupBeams();
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
      this.preFireRate = 1000f;
      if ((double)this.attackSequenceNextDelayMin <= 0.0 && (double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = this.preFireDuration;
      if ((double)this.attackSequenceNextDelayMax <= 0.0)
        this.attackSequenceNextDelayMax = 0.05f;
      if ((double)this.attackSequenceNextDelayMin >= (double)this.attackSequenceNextDelayMax)
        this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
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
      if (this.currentBeam < 0 || this.currentBeam >= this.beams.Count) { return; };
      base.FireNextShot();
      //this.PlayMuzzleFlash();
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      int beamsPerShot = weapon.ProjectilesPerShot;
      if (this.currentBeam == 0) {
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
      for (int index = 0; index < beamsPerShot; ++index) {
        if (this.currentBeam >= this.beams.Count) { break; };
        MultiShotBeamEffect bullet = this.beams[this.currentBeam];
        bullet.beamIdx = this.currentBeam;
        bool prime = index >= (beamsPerShot - 1);
        if (dmgPerBullet == true) { prime = true; };
        if (index == 0) {
          if (this.currentBeam == 0) { bullet.playSFX = PlaySFXType.First; } else { bullet.playSFX = PlaySFXType.Middle; }
          if ((this.currentBeam + beamsPerShot) >= this.beams.Count) { bullet.playSFX = PlaySFXType.Last; }
        } else {
          bullet.playSFX = PlaySFXType.None;
        }
        bullet.Fire(this.hitInfo, this.beamHitIndex, (bullet.beamIdx % this.numberOfEmitters), prime);
        ++this.currentBeam;
        if (dmgPerBullet == true) { ++this.beamHitIndex; };
      }
      if (dmgPerBullet == false) { ++this.beamHitIndex; };
      this.t = 0.0f;
      if (this.currentBeam < this.beams.Count) { return; }
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
    protected override void PlayImpact() {
      //base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBeamsComplete())
          return;
        this.OnComplete();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    protected override void OnImpact(float hitDamage = 0.0f,float structureDamage = 0f) {
      base.OnImpact(0.0f,0f);
    }
    protected override void OnComplete() {
      base.OnComplete();
      this.ClearBeams();
    }
    public override void Reset() {
      for (int index = 0; index < this.beams.Count; ++index) {
        this.beams[index].Reset();
      }
      base.Reset();
    }
  }
}
