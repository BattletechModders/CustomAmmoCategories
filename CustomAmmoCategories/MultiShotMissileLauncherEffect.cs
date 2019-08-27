﻿// Decompiled with JetBrains decompiler
// Type: MissileLauncherEffect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A45ECDCD-AC3C-461F-86FA-6E4E161AEB1A
// Assembly location: C:\Games\steamapps\common\BATTLETECH\BattleTech_Data\Managed\Assembly-CSharp.dll

using BattleTech;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
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
    Log.LogWrite("MultiShotMissileLauncherEffect.Init\n");
    this.MissilePrefab = original.MissilePrefab;
    this.firingInterval = original.firingInterval;
    this.firingIntervalRate = 0f;
    this.volleyInterval = original.volleyInterval;
    this.volleyIntervalRate = 0f;
    this.missileSpreadAngle = original.missileSpreadAngle;
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
    if (!((Object)this.MissilePrefab != (Object)null))
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
    Log.LogWrite("MultiShotMissileLauncherEffect.SetupMissiles getting from pool:" + prefabName + "\n");
    for (int index = 0; index < this.hitInfo.numberOfShots; ++index) {
      GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      MultiShotMissileEffect msComponent = null;
      if (MultiShotGameObject != null) {
        Log.LogWrite(" getted from pool: " + MultiShotGameObject.GetInstanceID() + "\n");
        msComponent = MultiShotGameObject.GetComponent<MultiShotMissileEffect>();
        if (msComponent != null) {
          msComponent.Init(this.weapon, this);
          this.missiles.Add(msComponent);
        }
      }
      if (msComponent == null) {
        Log.LogWrite(" not in pool. instansing.\n");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.MissilePrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          Log.LogWrite("Error instantiating MissileObject " + this.MissilePrefab.name+"\n");
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
        if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
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
    Log.LogWrite("MultiShotBallisticEffect.ClearBullets\n");
    for (int index = 0; index < this.missiles.Count; ++index) {
      this.missiles[index].Reset();
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

  public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
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
    base.Update();
    if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t >= 1.0)
      this.FireNextMissile();
    if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllMissilesComplete())
      return;
    this.OnComplete();
  }

  private void FireNextMissile() {
    Log.LogWrite("MultiShotMissileLauncherEffect.FireNextMissile hitIndex:" + this.hitIndex + "/" + this.hitInfo.numberOfShots + " volleyIndex:" + this.missileVolleyIdx+"/"+ this.VolleySize+"\n");
    if (this.hitIndex < this.hitInfo.numberOfShots && this.missileVolleyIdx < this.VolleySize)
      this.LaunchMissile();
    if (this.hitIndex >= this.hitInfo.numberOfShots) {
      if (this.isSRM) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    } else {
      if (this.missileVolleyIdx >= this.VolleySize) {
        this.FireNextVolley();
      }
    }
  }

  private void FireNextVolley() {
    Log.LogWrite("MultiShotMissileLauncherEffect.FireNextVolley\n");
    this.t = 0.0f;
    this.rate = this.volleyIntervalRate;
    this.missileVolleyIdx = 0;
    if ((double)this.rate >= 0.00999999977648258)
      return;
    this.FireNextMissile();
  }

  private void LaunchMissile() {
    Log.LogWrite("MultiShotMissileLauncherEffect.LaunchMissile hitIndex:" + this.hitIndex + "/" + this.hitInfo.numberOfShots + " volleyIndex:" + this.missileVolleyIdx + "/" + this.VolleySize + " emmiters: "+ this.emitterIndex + "/" +this.weaponRep.vfxTransforms.Length+ "\n");
    MultiShotMissileEffect missile = this.missiles[this.hitIndex] as MultiShotMissileEffect;
    missile.tubeTransform = this.weaponRep.vfxTransforms[this.emitterIndex];
    missile.FireEx(this.hitInfo, this.hitIndex, this.emitterIndex, this.isIndirect);
    if (this.hitIndex == 0) {
      if (this.isSRM) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
    }
    ++this.hitIndex;
    ++this.missileVolleyIdx;
    this.emitterIndex = (this.emitterIndex + 1)%this.numberOfEmitters;
    this.t = 0.0f;
    this.rate = this.firingIntervalRate;
    if ((double)this.rate >= 0.00999999977648258)
      return;
    this.FireNextMissile();
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
    this.ClearMissiles();
  }

  public override void Reset() {
    base.Reset();
  }
}
