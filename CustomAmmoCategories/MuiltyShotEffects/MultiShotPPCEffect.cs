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
    public void Init(PPCEffect original, string prefab) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("MultiShotPPCEffect.Init\n");
      this.PulseEffectPrefab = prefab;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
    }
    protected void SetupBeams() {
      Log.LogWrite("MultiShotPPCEffect.SetupBeams\n");
      this.currentPulse = 0;
      this.pulseHitIndex = 0;
      this.ClearBeams();
      int beamsCount = this.hitInfo.numberOfShots;
      if (this.weapon.DamagePerPallet() == false) {
        beamsCount *= weapon.ProjectilesPerShot;
      }
      string prefabName = MultiShotPPCEffect.ImprovedPulsePrefabPrefix + this.PulseEffectPrefab;
      Log.LogWrite("MultiShotPPCEffect.SetupBullets getting from pool:" + prefabName + "\n");
      for (int index = 0; index < beamsCount; ++index) {
        GameObject MultiShotGameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        MultiShotPulseEffect msComponent = null;
        if (MultiShotGameObject != null) {
          Log.LogWrite(" getted from pool: " + MultiShotGameObject.GetInstanceID() + "\n");
          msComponent = MultiShotGameObject.GetComponent<MultiShotPulseEffect>();
          if (msComponent != null) {
            msComponent.Init(this.weapon, this);
            this.pulses.Add(msComponent);
          }
        }
        if (msComponent == null) {
          Log.LogWrite(" not in pool. instansing.\n");
          GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.PulseEffectPrefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
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
          if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.PulseEffectPrefab), (UnityEngine.Object)this);
            return;
          }
          msComponent = MultiShotGameObject.AddComponent<MultiShotPulseEffect>();
          msComponent.Init(component);
          msComponent.Init(this.weapon, this);
          this.pulses.Add(msComponent);
        }
      }
    }
    protected void ClearBeams() {
      string prefabName = MultiShotPPCEffect.ImprovedPulsePrefabPrefix + this.PulseEffectPrefab;
      Log.LogWrite("MultiShotPPCEffect.ClearBullets\n");
      for (int index = 0; index < this.pulses.Count; ++index) {
        this.pulses[index].Reset();
        GameObject gameObject = this.pulses[index].gameObject;
        Log.LogWrite(" returning to pool " + prefabName + " " + gameObject.GetInstanceID() + "\n");
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
      Log.LogWrite("MultiShotPPCEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + "\n");
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
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
      //base.PlayMuzzleFlash();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
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
      Log.LogWrite("MultiShotPPCEffect.FireNextShot\n");
      if (this.currentPulse < 0 || this.currentPulse >= this.pulses.Count) { return; };
      //this.PlayMuzzleFlash();
      base.FireNextShot();
      bool dmgPerBullet = this.weapon.DamagePerPallet();
      int beamsPerShot = weapon.ProjectilesPerShot;
      float longestDistance = 0f;

      for (int index = 0; index < beamsPerShot; ++index) {
        if (this.currentPulse >= this.pulses.Count) { break; };
        MultiShotPulseEffect bullet = this.pulses[this.currentPulse];
        bullet.pulseIdx = this.currentPulse;
        bool prime = index >= (beamsPerShot - 1);
        if (dmgPerBullet == true) { prime = true; };
        bullet.Fire(this.hitInfo, this.pulseHitIndex, (bullet.pulseIdx%this.numberOfEmitters) , prime);
        ++this.currentPulse;
        float distance = Vector3.Distance(this.startPos, this.hitInfo.hitPositions[this.pulseHitIndex]);
        if (distance > longestDistance) { longestDistance = distance; };
        if (dmgPerBullet == true) { ++this.pulseHitIndex; };
      }
      if (dmgPerBullet == false) { ++this.pulseHitIndex; };
      if ((double)this.projectileSpeed > 0.0) {
        this.duration = longestDistance / this.projectileSpeed;
      } else {
        this.duration = 1f;
      }
      if ((double)this.duration > 4.0) {
        this.duration = 4f;
      }
      float shotDelay = this.duration * (1f + this.weapon.FireDelayMultiplier());
      if ((double)shotDelay <= 0.1f) { shotDelay = 0.1f; }
      this.rate = 1f / shotDelay;
      Log.LogWrite(" projectileSpeed:" + projectileSpeed + " distance:" + longestDistance + " shotDelay:" + shotDelay + " rate:" + this.rate + "\n");
      this.t = 0.0f;
      if (this.currentPulse < this.pulses.Count) {
        this.currentState = WeaponEffect.WeaponEffectState.Firing;
      } else {
        this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
      }
    }
    protected override void PlayImpact() {
      //base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBeamsComplete())
        return;
      this.OnComplete();
    }
    protected override void OnImpact(float hitDamage = 0.0f,float structureDamage = 0f) {
      base.OnImpact(0.0f,0f);
    }
    protected override void OnComplete() {
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
