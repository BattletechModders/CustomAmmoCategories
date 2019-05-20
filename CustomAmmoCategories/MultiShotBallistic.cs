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
  }
  public class MultiShotBallisticEffect : CopyAbleWeaponEffect {
    private List<MultiShotBulletEffect> bullets = new List<MultiShotBulletEffect>();
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
      if ((double)this.shotDelay <= 0.0)
        this.shotDelay = 0.5f;
      this.rate = 1f / this.shotDelay;
      this.ClearBullets();
      int bulletsCount = this.hitInfo.numberOfShots;
      if (this.weapon.HasShells() == false) { bulletsCount *= weapon.ProjectilesPerShot; };
      for (int index = 0; index < bulletsCount; ++index) {
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
          break;
        }
        GameObject MultiShotGameObject = GameObject.Instantiate(gameObject);
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
        MultiShotBulletEffect msComponent = MultiShotGameObject.AddComponent<MultiShotBulletEffect>();
        msComponent.Init(component);
        msComponent.Init(this.weapon, this);
        this.bullets.Add(msComponent);
      }
    }

    protected void ClearBullets() {
      for (int index = 0; index < this.bullets.Count; ++index) {
        GameObject gameObject = this.bullets[index].gameObject;
        GameObject.Destroy(gameObject);
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
        //Log.LogWrite(" sequence is null\n");
      } else {
        Log.LogWrite(" sequence is null\n");
      }
      if (CustomAmmoCategories.getWeaponAlwaysIndirectVisuals(this.weapon)) {
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
    protected override void PlayProjectile() {
      base.PlayProjectile();
      this.FireNextShot();
    }
    protected void FireNextShot() {
      if (this.currentBullet < 0 || this.currentBullet >= this.bullets.Count) { return; };
      this.PlayMuzzleFlash();
      bool shells = this.weapon.HasShells();
      int bulletsPerShot = shells ? 1 : weapon.ProjectilesPerShot;

      for (int index = 0; index < bulletsPerShot; ++index) {
        MultiShotBulletEffect bullet = this.bullets[this.currentBullet];
        bullet.bulletIdx = this.currentBullet;
        bullet.Fire(this.hitInfo, this.bulletHitIndex, 0, index >= (bulletsPerShot - 1));
        ++this.currentBullet;
      }
      ++this.bulletHitIndex;
      string empty = string.Empty;
      string eventName = this.currentBullet != 0 ? (this.currentBullet >= this.bullets.Count ? this.middleShotSFX : this.lastShotSFX) : this.firstShotSFX;
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

    protected override void Update() {
      base.Update();
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t >= 1.0)
        this.FireNextShot();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
        return;
      this.OnComplete();
    }

    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f) {
      base.OnImpact(0.0f);
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
