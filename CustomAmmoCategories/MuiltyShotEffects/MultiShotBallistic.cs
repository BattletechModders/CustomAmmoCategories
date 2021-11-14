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
    public static float FireDelayMultiplier(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef ammo = weapon.ammo();
      return Mathf.Abs(extWeapon.FireDelayMultiplier * mode.FireDelayMultiplier * ammo.FireDelayMultiplier);
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
    public static bool AlternateBallistic(this Weapon weapon) {
      ExtWeaponDef extWeapon = weapon.exDef();
      if (extWeapon.HasShells == TripleBoolean.True) { return true; };
      foreach (var mode in extWeapon.Modes) {
        if (mode.Value.HasShells == TripleBoolean.True) { return true; };
      }
      foreach (var box in weapon.ammoBoxes) {
        ExtAmmunitionDef ammoDef = CustomAmmoCategories.findExtAmmo(box.ammoDef.Description.Id);
        if (ammoDef.HasShells == TripleBoolean.True) { return true; }
      }
      return false;
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
    public float shotDelay;
    public float spreadAngle;
    public GameObject bulletPrefab;
    private int currentBullet;
    private int bulletHitIndex;
    public string firstShotSFX;
    public string middleShotSFX;
    public string lastShotSFX;
    public bool isIndirect;
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
    public void Init(BallisticEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("MultiShootBallisticEffect.Init\n");
      this.shotDelay = original.shotDelay;
      this.spreadAngle = original.spreadAngle;
      this.bulletPrefab = original.bulletPrefab;
      this.firstShotSFX = original.firstShotSFX;
      this.middleShotSFX = original.middleShotSFX;
      this.lastShotSFX = original.lastShotSFX;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (!((UnityEngine.Object)this.bulletPrefab != (UnityEngine.Object)null))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.bulletPrefab.name, BattleTechResourceType.Prefab, weapon.ProjectilesPerShot);
    }
    protected void SetupBullets() {
      this.currentBullet = 0;
      this.bulletHitIndex = 0;
      if ((double)this.shotDelay <= CustomAmmoCategories.Epsilon) {this.shotDelay = 0.5f;}
      float shotDelay = this.shotDelay * this.weapon.FireDelayMultiplier();
      if ((double)shotDelay <= 0.5f) { shotDelay = 0.5f; }
      this.rate = 1f / shotDelay;
      this.ClearBullets();
      int bulletsCount = this.hitInfo.numberOfShots;
      if (this.weapon.HasShells() == false) {
        if (this.weapon.DamagePerPallet() == false) {
          bulletsCount *= this.weapon.ProjectilesPerShot;
        }
      };
      string prefabName = MultiShotBallisticEffect.ImprovedBulletPrefabPrefix + this.bulletPrefab.name;
      Log.LogWrite("MultiShotBallisticEffect.SetupBullets "+this.weapon.UIName+" getting from pool:" + prefabName + " x"+bulletsCount+" = "+this.hitInfo.numberOfShots+" x "+this.weapon.ProjectilesPerShot+"\n");
      for (int index = 0; index < bulletsCount; ++index) {
        GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        MultiShotBulletEffect msComponent = null;
        if (MultiShotGameObject != null) {
          Log.LogWrite(" getted from pool: " + MultiShotGameObject.GetInstanceID() + "\n");
          msComponent = MultiShotGameObject.GetComponent<MultiShotBulletEffect>();
          if (msComponent != null) {
            msComponent.Init(this.weapon, this);
            this.bullets.Add(msComponent);
          }
        }
        if (msComponent == null) {
          Log.LogWrite(" not in pool. instansing.\n");
          GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
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
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.bulletPrefab.name, 4f);
          gameObject = null;
          MultiShotGameObject.transform.parent = (Transform)null;
          BulletEffect component = MultiShotGameObject.GetComponent<BulletEffect>();
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.bulletPrefab.name), (UnityEngine.Object)this);
            return;
          }
          msComponent = MultiShotGameObject.AddComponent<MultiShotBulletEffect>();
          msComponent.Init(component);
          msComponent.Init(this.weapon, this);
          this.bullets.Add(msComponent);
        }
      }
    }
    protected void ClearBullets() {
      string prefabName = MultiShotBallisticEffect.ImprovedBulletPrefabPrefix + this.bulletPrefab.name;
      Log.LogWrite("MultiShotBallisticEffect.ClearBullets\n");
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
      Log.LogWrite("MultiShotBallisticEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + "\n");
      this.isIndirect = false;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:"+this.endPos+"\n");
      AttackSequence sequence = Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if(sequence != null) {
        this.isIndirect = sequence.indirectFire;
        Log.M.WL(1, "sequence.indirectFire: "+ sequence.indirectFire);
      } else {
        Log.LogWrite("sequence is null\n");
      }
      if (this.weapon.AlwaysIndirectVisuals()) {
        Log.M.WL(1, "AlwaysIndirectVisuals");
        this.isIndirect = true;
      }
      Log.LogWrite(" isIndirect:"+this.isIndirect+"\n");
      this.SetupBullets();
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayProjectile() {
#else
    protected override void PlayProjectile() {
#endif
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      base.PlayProjectile(false);
      this.t = 1f;
    }
    protected override void FireNextShot() {
      if (this.currentBullet < 0 || this.currentBullet >= this.bullets.Count) { return; };
      base.PlayProjectile();
      base.FireNextShot();
      this.PlayMuzzleFlash();
      this.startingTransform = this.weaponRep.vfxTransforms[this.emitterIndex];
      this.startPos = this.startingTransform.position;
      bool shells = this.weapon.HasShells();
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      int bulletsPerShot = shells ? 1 : weapon.ProjectilesPerShot;
      for (int index = 0; index < bulletsPerShot; ++index) {
        if (this.currentBullet >= this.bullets.Count) { break; };
        MultiShotBulletEffect bullet = this.bullets[this.currentBullet];
        bullet.bulletIdx = this.currentBullet;
        bool prime = index >= (bulletsPerShot - 1);
        if (dmgPerBullet == true) { prime = true; };
        bullet.Fire(this.hitInfo, this.bulletHitIndex, (this.emitterIndex), prime);
        ++this.currentBullet;
        if (dmgPerBullet == true) { ++this.bulletHitIndex; };
      }
      this.emitterIndex = (this.emitterIndex + 1) % this.numberOfEmitters;
      if (dmgPerBullet == false) { ++this.bulletHitIndex; };
      string empty = string.Empty;
      string eventName = this.bulletHitIndex != 0 ? (this.bulletHitIndex >= this.hitInfo.numberOfShots ? this.middleShotSFX : this.lastShotSFX) : this.firstShotSFX;
      Log.M.TWL(0, "MultiShotBallisticEffect.FireNextShot sfx:"+eventName+" fsfx:"+ this.firstShotSFX+" msfx:"+ this.middleShotSFX+" lsfx:"+ this.lastShotSFX);
      if (!string.IsNullOrEmpty(eventName)) {
        int num = (int)WwiseManager.PostEvent(eventName, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      this.t = 0.0f;
      if (this.currentBullet < this.bullets.Count)
        return;
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
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
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
        return;
      this.OnComplete();
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
      this.ClearBullets();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
