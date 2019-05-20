using BattleTech;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class CopyAbleWeaponEffect: WeaponEffect {
    private static FieldInfo fi_hasSentNextWeaponMessage = null;
    public void Init(WeaponEffect original) {
      this.impactVFXBase = original.impactVFXBase;
      this.preFireSFX = original.preFireSFX;
      this.Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.hitInfo = original.hitInfo;
      this.hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.emitterIndex = (int)typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.subEffect = original.subEffect;
      this.currentState = original.currentState;
      this.weaponRep = original.weaponRep;
      this.weapon = original.weapon;
      this.parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.currentPos = (Vector3)typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
      this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = (float)typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
      if (fi_hasSentNextWeaponMessage != null) {
        this.hasSentNextWeaponMessage = (bool)fi_hasSentNextWeaponMessage.GetValue(original);
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
      }
      this.preFireDuration = original.preFireDuration;
      this.preFireRate = (float)typeof(WeaponEffect).GetField("preFireRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.duration = (float)typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.rate = (float)typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileSpeed = original.projectileSpeed;
      this.weaponImpactType = original.weaponImpactType;
      this.preFireVFXPrefab = original.preFireVFXPrefab;
      this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
      this.projectilePrefab = original.projectilePrefab;
      this.projectile = original.projectile;
      this.activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.impactVFXVariations = original.impactVFXVariations;
      this.armorDamageVFXName = original.armorDamageVFXName;
      this.structureDamageVFXName = original.structureDamageVFXName;
      this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
      this.FiringComplete = original.FiringComplete;
      this.AllowMissSkipping = original.AllowMissSkipping;
    }
    protected bool hasSentNextWeaponMessage {
      get {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          return (bool)fi_hasSentNextWeaponMessage.GetValue(this);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
          return false;
        }
      }
      set {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          fi_hasSentNextWeaponMessage.SetValue(this, value);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't set WeaponEffect.hasSentNextWeaponMessage\n");
        }
      }
    }
  }
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
      this.projectileSpeed = parentLauncher.projectileSpeed;
      if(this.spline == null) {
        this.spline = this.gameObject.AddComponent<CurvySpline>();
      }
    }

    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0, bool pb = false) {
      Log.LogWrite("MultiShotBulletEffect.Fire "+hitInfo.attackWeaponIndex+" "+hitIndex+" ep:"+hitInfo.hitPositions[hitIndex]+" prime:"+pb+"\n");
      this.primeBullet = pb;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
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
      ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(this.hitInfo, this.hitIndex);
      if (shrapnelHitRecord != null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" shrapnel Hit info found:" + shrapnelHitRecord.shellsHitIndex + "\n");
        if (shrapnelHitRecord.isSeparated == true) {
          this.RegisterFragWeaponEffect();
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" not separated\n");
        }
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
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
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
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f) {
      /*if (this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536) {
        AbstractActor combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId) as AbstractActor;
        if (combatantByGuid != null && ((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null)) {
          combatantByGuid.GameRep.PlayImpactAnim(this.hitInfo, this.hitIndex, this.weapon, MeleeAttackType.NotSet, 0.0f);
        }
      }*/ // это в принципе не нужно потом что PlayImpactAnim проигрывается на каждый импакт, в оригинальной реализации попадание пули просто не вызывало импакт
      Log.LogWrite("MultiShotBulletEffect.OnImpact wi:"+this.hitInfo.attackWeaponIndex+" hi:"+this.hitInfo+" bi:"+this.bulletIdx+" prime:"+this.primeBullet+"\n");
      if (this.primeBullet) {
        Log.LogWrite(" prime. Damage message fired\n");
        base.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask));
      } else {
        Log.LogWrite(" no prime. No damage message fired\n");
      }
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; };
      this.projectileParticles.Stop(true);
    }
    protected override void OnComplete() {
      base.OnComplete();
    }
    public void OnDisable() {
      if (!((UnityEngine.Object)this.projectileAudioObject != (UnityEngine.Object)null))
        return;
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
      int num = (int)AkSoundEngine.UnregisterGameObj(this.projectileAudioObject.gameObject);
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
