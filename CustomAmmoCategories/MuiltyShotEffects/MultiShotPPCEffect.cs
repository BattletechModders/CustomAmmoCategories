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
  public class MultiShotPPCEffect : MuiltShotAnimatedEffect {
    private List<MultiShotPulseEffect> pulses = new List<MultiShotPulseEffect>();
    public static readonly string ImprovedPulsePrefabPrefix = "_IMPROVED_";
    public int currentPulse;
    public int pulseHitIndex;
    public string PulseEffectPrefab;
    public float spreadAngle = 0.5f;
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
    public void Init(PPCEffect original, string prefab) {
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
      Log.Combat?.TWL(0,"MultiShotPPCEffect.Init");
      this.PulseEffectPrefab = prefab;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
    }
    protected void SetupBeams() {
      Log.Combat?.TWL(0, "MultiShotPPCEffect.SetupBeams");
      try {
        this.currentPulse = 0;
        this.pulseHitIndex = 0;
        this.ClearBeams();
        int beamsCount = this.hitInfo.numberOfShots;
        if (this.weapon.DamagePerPallet() == false) {
          beamsCount *= weapon.ProjectilesPerShot;
        }
        string prefabName = MultiShotPPCEffect.ImprovedPulsePrefabPrefix + this.PulseEffectPrefab;
        Log.Combat?.WL(0, $"MultiShotPPCEffect.SetupBullets getting from pool:{prefabName}");
        for (int index = 0; index < beamsCount; ++index) {
          GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          MultiShotPulseEffect msComponent = null;
          if (MultiShotGameObject != null) {
            Log.Combat?.WL(1, $"getted from pool: {MultiShotGameObject.GetInstanceID()}");
            msComponent = MultiShotGameObject.GetComponent<MultiShotPulseEffect>();
            if (msComponent != null) {
              msComponent.Init(this.weapon, this);
              this.pulses.Add(msComponent);
            }
          }
          if (msComponent == null) {
            Log.Combat?.WL(1, $"not in pool. instansing.");
            GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.PulseEffectPrefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            if (gameObject == null) {
              WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.PulseEffectPrefab), (UnityEngine.Object)this);
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
            autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.PulseEffectPrefab, 4f);
            gameObject = null;
            MultiShotGameObject.transform.parent = (Transform)null;
            PPCEffect component = MultiShotGameObject.GetComponent<PPCEffect>();
            if (component == null) {
              WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.PulseEffectPrefab), (UnityEngine.Object)this);
              return;
            }
            msComponent = MultiShotGameObject.AddComponent<MultiShotPulseEffect>();
            msComponent.Init(component);
            msComponent.Init(this.weapon, this);
            this.pulses.Add(msComponent);
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        WeaponEffect.logger.LogException(e);
      }
    }
    protected void ClearBeams() {
      string prefabName = MultiShotPPCEffect.ImprovedPulsePrefabPrefix + this.PulseEffectPrefab;
      Log.Combat?.TWL(0, "MultiShotPPCEffect.ClearBullets");
      for (int index = 0; index < this.pulses.Count; ++index) {
        this.pulses[index].Reset();
        GameObject gameObject = this.pulses[index].gameObject;
        Log.Combat?.WL(1,$"returning to pool {prefabName} {gameObject.GetInstanceID()}");
        this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
      }
      this.pulses.Clear();
    }
    public bool AllBeamsComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.pulses.Count; ++index) {
        if (!this.pulses[index].FiringComplete)
          return false;
      }
      return true;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.TWL(0,$"MultiShotPPCEffect.Fire {hitInfo.attackWeaponIndex} {hitIndex} ep:{hitInfo.hitPositions[hitIndex]}");
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      this.SetupCustomSettings();
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.Combat?.WL(1,$"endPos restored:{this.endPos}");
      AttackSequence sequence = Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      this.SetupBeams();
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
    }
    public override void SetupCustomSettings() {
      this.customPrefireSFX = weapon.parentPreFireSFX();
      if (this.customPrefireSFX == null) { this.customPrefireSFX = string.Empty; }
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
    protected override void FireNextShot() {
      Log.Combat?.TWL(0,$"MultiShotPPCEffect.FireNextShot {weapon.defId}");
      if (this.currentPulse < 0 || this.currentPulse >= this.pulses.Count) { return; };
      //this.PlayMuzzleFlash();
      base.FireNextShot();
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      int beamsPerShot = weapon.ProjectilesPerShot;
      //float longestDistance = 0f;
      float effective_shotDelay = 0f;
      if (this.shotDelay <= CustomAmmoCategories.Epsilon) {
        this.shotDelay = 0.5f * (1f + this.weapon.FireDelayMultiplier());
        effective_shotDelay = this.shotDelay;
      } else {
        effective_shotDelay = this.shotDelay * this.weapon.FireDelayMultiplier();
      }
      if (effective_shotDelay < 0.5f) { effective_shotDelay = 0.5f; }
      this.duration = effective_shotDelay;
      this.rate = 1f / effective_shotDelay;
      Log.Combat?.WL(1,$"projectileSpeed:{projectileSpeed} shotDelay:{effective_shotDelay} rate:{this.rate} FireDelayMultiplier:{this.weapon.FireDelayMultiplier()}");
      if (this.currentPulse == 0) {
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
        if (this.currentPulse >= this.pulses.Count) { break; };
        MultiShotPulseEffect bullet = this.pulses[this.currentPulse];
        bullet.pulseIdx = this.currentPulse;
        bool prime = index >= (beamsPerShot - 1);
        if (dmgPerBullet == true) { prime = true; };
        if (index == 0) {
          if (this.currentPulse == 0) { bullet.playSFX = PlaySFXType.First; } else { bullet.playSFX = PlaySFXType.Middle; }
          if ((this.currentPulse + beamsPerShot) >= this.pulses.Count) { bullet.playSFX = PlaySFXType.Last; }
        } else {
          bullet.playSFX = PlaySFXType.None;
        }
        bullet.Fire(this.hitInfo, this.pulseHitIndex, (bullet.pulseIdx%this.numberOfEmitters), prime);
        ++this.currentPulse;
        if (dmgPerBullet == true) { ++this.pulseHitIndex; };
      }
      if (dmgPerBullet == false) { ++this.pulseHitIndex; };
      this.t = 0.0f;
      if (this.currentPulse < this.pulses.Count) {
        this.currentState = WeaponEffect.WeaponEffectState.Firing;
      } else {
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
    }
    protected override void PlayImpact() {
      //base.PlayImpact();
    }
    protected override void Update() {
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBeamsComplete())
          return;
        this.OnComplete();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
      }
    }
    protected override void OnImpact(float hitDamage = 0.0f,float structureDamage = 0f) {
      base.OnImpact(0.0f,0f);
    }
    protected override void OnComplete() {
      Log.Combat?.TWL(0, $"MultiShotPPCEffect.OnComplete wi:{this.hitInfo.attackWeaponIndex} hi:{this.hitIndex}");
      this.RestoreOriginalColor();
      base.OnComplete();
      this.ClearBeams();
    }
    public override void Reset() {
      for (int index = 0; index < this.pulses.Count; ++index) {
        this.pulses[index].Reset();
      }
      base.Reset();
    }
  }
}
