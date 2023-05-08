using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class MultiShotBulletEffect: CopyAbleWeaponEffect {
    public int bulletIdx;
    public bool primeBullet;
    private MultiShotBallisticEffect parentLauncher;
    private CurvySpline spline;
    public static readonly float missileCurveStrength = 0.0f;
    public static readonly int missileCurveFrequency = 2;
    private void GenerateIndirectMissilePath() {
      float max = MultiShotBulletEffect.missileCurveStrength;
      int num1 = Random.Range(2, MultiShotBulletEffect.missileCurveFrequency);
      if ((double)max < 0.1 || MultiShotBulletEffect.missileCurveFrequency < 1) {
        max = 0.0f;
        num1 = 2;
      }
      Vector3 up = Vector3.up;
      this.spline.Interpolation = CurvyInterpolation.Bezier;
      this.spline.Clear();
      this.spline.Closed = false;
      Vector3 axis = this.endPos - this.startPos;
      int length = 9;
      if (num1 > length)
        length = num1;
      float num2 = (float)((double)this.endPos.y - (double)this.startPos.y + 15.0);
      Vector3[] vector3Array = new Vector3[length];
      Vector3 vector3_1 = this.endPos - this.startPos;
      float num3 = (float)(((double)Mathf.Max(this.endPos.y, this.startPos.y) - (double)Mathf.Min(this.endPos.y, this.startPos.y)) * 0.5) + num2;
      vector3Array[0] = this.startPos;
      for (int index = 1; index < length - 1; ++index) {
        float num4 = (float)index / (float)length;
        float num5 = (float)(1.0 - (double)Mathf.Abs(num4 - 0.5f) / 0.5);
        float num6 = (float)(1.0 - (1.0 - (double)num5) * (1.0 - (double)num5));
        Vector3 worldPos = vector3_1 * num4;
        float lerpedHeightAt = this.Combat.MapMetaData.GetLerpedHeightAt(worldPos);
        if ((double)num3 < (double)lerpedHeightAt)
          num3 = lerpedHeightAt + 5f;
        worldPos.y += num6 * num3;
        worldPos += this.startPos;
        Vector3 vector3_2 = Vector3.up * Random.Range(-max, max);
        vector3_2 = Quaternion.AngleAxis((float)Random.Range(0, 360), axis) * vector3_2;
        if ((double)vector3_2.y < 0.0)
          vector3_2.y = 0.0f;
        worldPos += vector3_2;
        if ((double)worldPos.y < (double)lerpedHeightAt)
          worldPos.y = lerpedHeightAt + 5f;
        vector3Array[index] = worldPos;
      }
      vector3Array[length - 1] = this.endPos;
      this.spline.Add(vector3Array);
      this.spline.Refresh();
    }

    protected override int ImpactPrecacheCount {
      get {
        return 5;
      }
    }
    protected override void Awake() {
      base.Awake();
    }
    protected override void Start() {
      base.Start();
    }
    public MultiShotBulletEffect() {
      this.spline = null;
    }
    public void Init(Weapon weapon, MultiShotBallisticEffect parentLauncher) {
      this.Init(weapon);
      this.parentLauncher = parentLauncher;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      if(this.spline == null) {
        this.spline = this.gameObject.AddComponent<CurvySpline>();
      }
    }
    public override void SetupCustomSettings() {
      this.customPrefireSFX = this.preFireSFX;
      switch (this.playSFX) {
        case PlaySFXType.First: this.customPrefireSFX = this.parentLauncher.firstPreFireSFX; break;
        case PlaySFXType.Middle: this.customPrefireSFX = this.parentLauncher.middlePrefireSFX; break;
        case PlaySFXType.Last: this.customPrefireSFX = this.parentLauncher.lastPreFireSFX; break;
        case PlaySFXType.None: this.customPrefireSFX = string.Empty; break;
      }
      if (this.playSFX != PlaySFXType.None) {
        this.preFireStartSFX = weapon.preFireStartSFX();
        this.preFireStopSFX = weapon.preFireStopSFX();
        this.customPulseSFX = weapon.pulseSFX();
        this.customPulseSFXdelay = weapon.pulseSFXdelay();
        if (this.preFireStartSFX == null) { this.preFireStartSFX = string.Empty; }
        if (this.preFireStopSFX == null) { this.preFireStopSFX = string.Empty; }
        if (this.customPulseSFX == null) { this.customPulseSFX = string.Empty; }
        if (customPulseSFXdelay < CustomAmmoCategories.Epsilon) { this.customPulseSFXdelay = -1f; }
      } else {
        this.preFireStartSFX = string.Empty;
        this.preFireStopSFX = string.Empty;
        this.customPulseSFXdelay = 0f;
        this.customPulseSFX = string.Empty;
      }
      if (weapon.prefireDuration() > CustomAmmoCategories.Epsilon) {
        this.preFireDuration = weapon.prefireDuration();
      } else {
        this.preFireDuration = this.originalPrefireDuration;
      }
      if (weapon.ProjectileSpeed() > CustomAmmoCategories.Epsilon) {
        this.projectileSpeed = weapon.ProjectileSpeed();
      } else {
        this.projectileSpeed = parentLauncher.projectileSpeed;
      }
      this.projectileSpeed *= weapon.ProjectileSpeedMultiplier();
    }
    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, bool pb) {
      //Log.Combat?.TWL(0,"MultiShotBulletEffect.Fire "+hitInfo.attackWeaponIndex+" "+hitIndex+" emitter:" + emitterIndex + " ep:"+hitInfo.hitPositions[hitIndex]+" prime:"+pb+"\n");
      this.primeBullet = pb;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      this.SetupCustomSettings();
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      //Log.Combat?.WL(1, $"endPos restored:{this.endPos}");
      AdvWeaponHitInfoRec advRec = this.hitInfo.advRec(hitIndex);
      endPos.x += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.y += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.z += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      this.endPos = endPos;
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if (this.parentLauncher.isIndirect) {
        this.GenerateIndirectMissilePath();
        num = this.spline.Length;
      } else {
        this.spline.Clear();
      }
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
      if (advRec == null) {
        //Log.Combat?.WL(1, $"no advanced record.");
        return;
      }
      if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
        //Log.Combat?.WL(1, $"frag projectile separated.");
        this.RegisterFragWeaponEffect();
      }
    }
    public TrailRenderer trailRendered;
    public override void StoreOriginalColor() {
      this.trailRendered = this.projectile.GetComponentInChildren<TrailRenderer>();
      if (trailRendered != null) {
        this.trailRendered.material.RegisterRestoreColor();
      };
    }
    public override void SetColor(Color color) {
      float coeff = color.maxColorComponent;
      Color tempColor = Color.white;
      tempColor.a = 1f;
      tempColor.r = (color.r / coeff) * 8.0f + 1f;
      tempColor.g = (color.g / coeff) * 8.0f + 1f;
      tempColor.b = (color.b / coeff) * 8.0f + 1f;
      if (trailRendered != null) {  this.trailRendered.material.SetColor("_ColorBB", tempColor); };
    }
    public override void RestoreOriginalColor() {
      //if (trailRendered != null) { this.trailRendered.material.SetColor("_ColorBB", this.originalColor); };
    }
    public override void InitProjectile() {
      base.InitProjectile();
      //Log.Combat?.TWL(0,"MultiShotBulletEffect.InitProjectile");
      Component[] components = this.projectile.GetComponentsInChildren<Component>();
      foreach (Component component in components) {
        //Log.Combat?.WL(1, $"{component.name}:{component.GetType().ToString()}");
      }
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      base.PlayProjectile();
      this.PlayMuzzleFlash();
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
      try {
        base.Update();
        if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
          return;
        if ((double)this.t < 1.0) {
          this.UpdateColor();
          if (this.spline.Count > 0) {
            this.currentPos = this.spline.InterpolateByDistance(this.spline.Length * this.t);
            this.projectileTransform.position = this.currentPos;
            this.projectileTransform.rotation = this.spline.GetOrientationFast(this.t, false);
          } else {
            this.currentPos = Vector3.Lerp(this.startPos, this.endPos, this.t);
            this.projectileTransform.position = this.currentPos;
          }
        }
        if ((double)this.t < 1.0)
          return;
        this.PlayImpact();
        this.OnComplete();
      }catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      /*if (this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536) {
        AbstractActor combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId) as AbstractActor;
        if (combatantByGuid != null && ((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null)) {
          combatantByGuid.GameRep.PlayImpactAnim(this.hitInfo, this.hitIndex, this.weapon, MeleeAttackType.NotSet, 0.0f);
        }
      }*/ // это в принципе не нужно потом что PlayImpactAnim проигрывается на каждый импакт, в оригинальной реализации попадание пули просто не вызывало импакт
      Log.Combat?.TWL(0,$"MultiShotBulletEffect.OnImpact wi:{this.hitInfo.attackWeaponIndex} hi:{this.hitInfo} bi:{this.bulletIdx} prime:{this.primeBullet}");
      if (this.primeBullet) {
        Log.Combat?.WL(1, $"prime. Damage message fired");
        float damage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
        float apDamage = this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
        if (this.weapon.DamagePerPallet()&&(this.weapon.DamageNotDivided() == false)) {
          damage /= this.weapon.ProjectilesPerShot;
          apDamage /= this.weapon.ProjectilesPerShot;
        };
        base.OnImpact(damage, apDamage);
      } else {
        Log.Combat?.WL(1, $"no prime. No damage message fired");
      }
      if (this.projectileParticles == null) { return; };
      this.projectileParticles.Stop(true);
    }
    protected override void OnComplete() {
      base.OnComplete();
    }
    public void OnDisable() {
      if (this.projectileAudioObject == null){ return; }
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
      int num = (int)AkSoundEngine.UnregisterGameObj(this.projectileAudioObject.gameObject);
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
