using BattleTech;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static BattleTech.AttackDirector;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static float ProjectileSpeedMultiplier(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      return Mathf.Abs(extWeapon.ProjectileSpeedMultiplier * mode.ProjectileSpeedMultiplier * ammo.ProjectileSpeedMultiplier);
    }
    public static float FireDelayMultiplier(this Weapon weapon, bool ballistic = false) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      float FireDelayMultiplier = extWeapon.FireDelayMultiplier;
      if (FireDelayMultiplier < CustomAmmoCategories.Epsilon) {
        FireDelayMultiplier = ballistic ? 10f : 1f;
      }
      return Mathf.Abs(FireDelayMultiplier * mode.FireDelayMultiplier * ammo.FireDelayMultiplier);
    }
    public static float MissileFiringIntervalMultiplier(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      return Mathf.Abs(extWeapon.MissileFiringIntervalMultiplier * mode.MissileFiringIntervalMultiplier * ammo.MissileFiringIntervalMultiplier);
    }
    public static float MissileVolleyIntervalMultiplier(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      return Mathf.Abs(extWeapon.MissileVolleyIntervalMultiplier * mode.MissileVolleyIntervalMultiplier * ammo.MissileVolleyIntervalMultiplier);
    }
    public static bool DamagePerPallet(this Weapon weapon) {
      if (weapon.HasShells() == true) { return false; };
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      if (mode.BallisticDamagePerPallet != TripleBoolean.NotSet) { return mode.BallisticDamagePerPallet == TripleBoolean.True; }
      if (ammo.BallisticDamagePerPallet != TripleBoolean.NotSet) { return ammo.BallisticDamagePerPallet == TripleBoolean.True; }
      if (extWeapon.BallisticDamagePerPallet != TripleBoolean.NotSet) { return extWeapon.BallisticDamagePerPallet == TripleBoolean.True; }
      return false;
    }
  }
  public class MultiShotBallisticEffect : MuiltShotAnimatedEffect {
    private List<MultiShotBulletEffect> bullets = new List<MultiShotBulletEffect>();
    public static readonly string ImprovedBulletPrefabPrefix = "_IMPROVED_";
    public float originalShotDelay;
    public float spreadAngle;
    public string bulletPrefab;
    private int currentBullet;
    private int bulletHitIndex;
    public string firstShotSFX;
    public string middleShotSFX;
    public string lastShotSFX;
    public bool isIndirect;
    public int bulletsPerShot { get; set; } = 1;
    public CurvySpline generateSimpleIndirectSpline(Vector3 startPos, Vector3 endPos, int hitLocation) {
      Vector3[] spline;
      spline = CustomAmmoCategories.GenerateIndirectMissilePath(
        MultiShotBulletEffect.missileCurveStrength,
        MultiShotBulletEffect.missileCurveFrequency,
        false,
        hitLocation, startPos, endPos, Combat);
      GameObject splineObject = new GameObject();
      CurvySpline UnitySpline = splineObject.AddComponent<CurvySpline>();
      UnitySpline.Interpolation = CurvyInterpolation.Bezier;
      UnitySpline.Clear();
      UnitySpline.Closed = false;
      UnitySpline.Add(spline);
      UnitySpline.Refresh();
      return UnitySpline;
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
    public void Init(BallisticEffect original) {
      base.Init(original);
      Log.Combat?.TWL(0,"MultiShootBallisticEffect.Init");
      this.originalShotDelay = original.shotDelay;
      this.spreadAngle = original.spreadAngle;
      this.bulletPrefab = original.bulletPrefab.name;
      this.firstShotSFX = original.firstShotSFX;
      this.middleShotSFX = original.middleShotSFX;
      this.lastShotSFX = original.lastShotSFX;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (string.IsNullOrEmpty(this.bulletPrefab))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.bulletPrefab, BattleTechResourceType.Prefab, weapon.ProjectilesPerShot);
    }
    protected void SetupBullets() {
      try {
        this.currentBullet = 0;
        this.bulletHitIndex = 0;
        if (this.shotDelay <= CustomAmmoCategories.Epsilon) { this.shotDelay = 0.5f; }
        float effective_shotDelay = this.shotDelay * this.weapon.FireDelayMultiplier(true);
        if (effective_shotDelay <= 0.5f) { effective_shotDelay = 0.5f; }
        this.rate = 1f / effective_shotDelay;
        this.ClearBullets();
        int bulletsCount = this.hitInfo.numberOfShots;
        if (this.weapon.HasShells() == false) {
          if (this.weapon.DamagePerPallet() == false) {
            bulletsCount *= this.weapon.ProjectilesPerShot;
          }
        };
        this.bulletsPerShot = this.weapon.HasShells() ? 1 : weapon.ProjectilesPerShot;
        string prefabName = MultiShotBallisticEffect.ImprovedBulletPrefabPrefix + this.bulletPrefab;
        Log.Combat?.TWL(0, $"MultiShotBallisticEffect.SetupBullets {this.weapon.UIName} getting from pool:{prefabName} x{bulletsCount} = {this.hitInfo.numberOfShots} x {this.weapon.ProjectilesPerShot}");
        for (int index = 0; index < bulletsCount; ++index) {
          GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          MultiShotBulletEffect msComponent = null;
          if (MultiShotGameObject != null) {
            Log.Combat?.WL(1, $"getted from pool: {MultiShotGameObject.GetInstanceID()}");
            msComponent = MultiShotGameObject.GetComponent<MultiShotBulletEffect>();
            if (msComponent != null) {
              msComponent.Init(this.weapon, this);
              this.bullets.Add(msComponent);
            }
          }
          if (msComponent == null) {
            Log.Combat?.WL(1, $"not in pool. instancing");
            GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            if (gameObject == null) {
              WeaponEffect.logger.LogError(($"Error instantiating BulletObject {this.bulletPrefab}"), this);
              Log.Combat?.WL(1, $"Error instantiating BulletObject {this.bulletPrefab}");
              break;
            }
            MultiShotGameObject = GameObject.Instantiate(gameObject);
            AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
            if (autoPoolObject == null) {
              autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
            } else {
              AutoPoolObject MultiShotAutoPoolObject = MultiShotGameObject.GetComponent<AutoPoolObject>();
              if (MultiShotAutoPoolObject != null) { GameObject.Destroy(MultiShotAutoPoolObject); };
            }
            autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.bulletPrefab, 4f);
            gameObject = null;
            MultiShotGameObject.transform.parent = null;
            BulletEffect component = MultiShotGameObject.GetComponent<BulletEffect>();
            if (component == null) {
              WeaponEffect.logger.LogError(($"Error finding BulletEffect on GO {this.bulletPrefab}"), (UnityEngine.Object)this);
              return;
            }
            msComponent = MultiShotGameObject.AddComponent<MultiShotBulletEffect>();
            msComponent.Init(component);
            msComponent.Init(this.weapon, this);
            this.bullets.Add(msComponent);
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        WeaponEffect.logger.LogException(e);
      }
    }
    protected void ClearBullets() {
      try {
        string prefabName = MultiShotBallisticEffect.ImprovedBulletPrefabPrefix + this.bulletPrefab;
        Log.Combat?.TWL(0, $"MultiShotBallisticEffect.ClearBullets");
        for (int index = 0; index < this.bullets.Count; ++index) {
          this.bullets[index].StopAudio();
          this.bullets[index].Reset();
          GameObject gameObject = this.bullets[index].gameObject;
          Log.Combat?.WL(1, $"returning to pool {prefabName} {gameObject.GetInstanceID()}");
          this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
        }
        this.bullets.Clear();
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        WeaponEffect.logger.LogException(e);
      }
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
    public override void SetupCustomSettings() {
      this.firstPreFireSFX = weapon.firstPreFireSFX();
      this.middlePrefireSFX = weapon.preFireSFX();
      this.customPrefireSFX = weapon.parentPreFireSFX();
      if (this.customPrefireSFX == null) { this.customPrefireSFX = this.preFireSFX; }
      this.lastPreFireSFX = weapon.lastPreFireSFX();
      this.firingStartSFX = weapon.firingStartSFX();
      this.firingStopSFX = weapon.firingStopSFX();
      if (string.IsNullOrEmpty(firingStartSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(firingStartSFX)) {
          this.firingStopSFX = firingStartSFX;
        }
      }
      if (firingStartSFX == null) { firingStartSFX = string.Empty; }
      if (firingStopSFX == null) { firingStopSFX = string.Empty; }
      if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.firstShotSFX; }
      if (this.middlePrefireSFX == null) { this.middlePrefireSFX = this.middleShotSFX; }
      if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.lastShotSFX; }
      this.fireSFX = string.Empty;
      this.middlefireSFX = weapon.fireSFX();
      this.firstFireSFX = weapon.firstFireSFX();
      this.lastFireSFX = weapon.lastFireSFX();
      if (this.middlefireSFX == null) { this.middlefireSFX = string.Empty; }
      if (this.firstPreFireSFX == null) { this.firstPreFireSFX = this.middlefireSFX; }
      if (this.lastPreFireSFX == null) { this.lastPreFireSFX = this.middlefireSFX; }
      if (weapon.shotDelay() > CustomAmmoCategories.Epsilon) {
        this.shotDelay = weapon.shotDelay();
      } else {
        this.shotDelay = this.originalShotDelay;
      }
      Log.Combat?.TWL(0, $"MultiShotBallisticEffect.SetupCustomSettings playSFX:{this.playSFX}");
      Log.Combat?.WL(1, $"customPrefireSFX:{this.customPrefireSFX}");
      Log.Combat?.WL(1, $"firstPreFireSFX:{this.firstPreFireSFX}");
      Log.Combat?.WL(1, $"middlePrefireSFX:{this.middlePrefireSFX}");
      Log.Combat?.WL(1, $"lastPreFireSFX:{this.lastPreFireSFX}");
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.TWL(0, $"MultiShotBallisticEffect.Fire {hitInfo.attackWeaponIndex} {hitIndex} ep:{hitInfo.hitPositions[hitIndex]}");
      try {
        this.isIndirect = false;
        Vector3 endPos = hitInfo.hitPositions[hitIndex];
        this.SetupCustomSettings();
        base.Fire(hitInfo, hitIndex, emitterIndex);
        this.endPos = endPos;
        hitInfo.hitPositions[hitIndex] = endPos;
        Log.Combat?.WL(1, $"endPos restored:{this.endPos}");
        AttackSequence sequence = Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        if (sequence != null) {
          this.isIndirect = sequence.indirectFire;
          Log.Combat?.WL(1, $"sequence.indirectFire: {sequence.indirectFire}");
        } else {
          Log.Combat?.WL(1, $"sequence is null");
        }
        if (this.weapon.AlwaysIndirectVisuals()) {
          Log.Combat?.WL(1, "AlwaysIndirectVisuals");
          this.isIndirect = true;
        }
        Log.Combat?.WL(1, $"isIndirect:{this.isIndirect}");
        this.SetupBullets();
        this.PlayPreFire();
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        WeaponEffect.logger.LogException(e);
      }
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      base.PlayProjectile(false);
      this.t = 1f;
    }
    protected override void FireNextShot() {
      if (this.currentBullet < 0 || this.currentBullet >= this.bullets.Count) { return; };
      base.FireNextShot();
      this.startingTransform = this.weaponRep.vfxTransforms[this.emitterIndex];
      this.startPos = this.startingTransform.position;
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      if(this.currentBullet == 0) {
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
      for (int index = 0; index < bulletsPerShot; ++index) {
        if (this.currentBullet >= this.bullets.Count) { break; };
        MultiShotBulletEffect bullet = this.bullets[this.currentBullet];
        bullet.bulletIdx = this.currentBullet;
        bool prime = index >= (bulletsPerShot - 1);
        if (dmgPerBullet == true) { prime = true; };
        if (index == 0) {
          if (this.currentBullet == 0) { bullet.playSFX = PlaySFXType.First; } else { bullet.playSFX = PlaySFXType.Middle; }
          if ((this.currentBullet + bulletsPerShot) >= this.bullets.Count) { bullet.playSFX = PlaySFXType.Last; }
        } else {
          bullet.playSFX = PlaySFXType.None;
        }
        bullet.Fire(this.hitInfo, this.bulletHitIndex, (this.emitterIndex), prime);
        ++this.currentBullet;
        if (dmgPerBullet == true) { ++this.bulletHitIndex; };
      }
      this.emitterIndex = (this.emitterIndex + 1) % this.numberOfEmitters;
      if (dmgPerBullet == false) { ++this.bulletHitIndex; };
      string empty = string.Empty;
      this.t = 0.0f;
      if (this.currentBullet < this.bullets.Count) { return; }
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
      base.PlayImpact();
    }
    protected override void Update() {
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
          return;
        this.OnComplete();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
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
      this.StopAudio();
      this.ClearBullets();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
