/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSMissileLauncherEffect : AMSMultiShotWeaponEffect {
    public List<AMSMissileEffect> missiles = new List<AMSMissileEffect>();
    public GameObject MissilePrefab;
    public float firingInterval;
    private float firingIntervalRate;
    public float volleyInterval;
    private float volleyIntervalRate;
    //public float missileSpreadAngle;
    public float missileCurveStrength;
    public int missileCurveFrequency;
    public bool isSRM;
    public override float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      if (shotIdx < 0) { return curPath / pathLenth; }
      if (shotIdx >= this.missiles.Count) { return curPath / pathLenth; };
      return this.missiles[shotIdx].calculateInterceptCorrection(curPath, pathLenth, distance, missileProjectileSpeed);
    }
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
    public virtual void Init(MissileLauncherEffect original) {
      base.Init(original);
      this.MissilePrefab = original.MissilePrefab;
      this.firingInterval = original.firingInterval;
      this.firingIntervalRate = original.firingIntervalRate;
      this.volleyInterval = original.volleyInterval;
      this.volleyIntervalRate = original.volleyIntervalRate;
      this.missileCurveStrength = original.missileCurveStrength;
      this.missileCurveFrequency = original.missileCurveFrequency;
      this.isSRM = true;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (this.MissilePrefab == null)
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.MissilePrefab.name, BattleTechResourceType.Prefab, 1);
    }
    public override int BulletsCount() { return this.missiles.Count; }
    public override void AddBullet() {
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.MissilePrefab.name;
      Log.Combat?.WL(0,"AMSMissileLauncherEffect.AddBullet getting from pool:" + prefabName);
      GameObject AMSgameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      AMSMissileEffect amsComponent = null;
      if (AMSgameObject != null) {
        Log.Combat?.WL(1, "getted from pool: " + AMSgameObject.GetInstanceID());
        amsComponent = AMSgameObject.GetComponent<AMSMissileEffect>();
        if (amsComponent != null) {
          amsComponent.Init(this.weapon, this);
          this.missiles.Add(amsComponent);
        }
      }
      if (amsComponent == null) {
        Log.Combat?.WL(1, "not in pool. instansing.");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.MissilePrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (gameObject == null) {
          WeaponEffect.logger.LogError((object)("Error instantiating MissileObject " + this.MissilePrefab.name), (UnityEngine.Object)this);
          return;
        }
        AMSgameObject = GameObject.Instantiate(gameObject);
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null) {
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        } else {
          AutoPoolObject AMSautoPoolObject = AMSgameObject.GetComponent<AutoPoolObject>();
          if (AMSautoPoolObject != null) { GameObject.Destroy(AMSautoPoolObject); };
        }
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.MissilePrefab.name, 4f);
        gameObject = null;
        AMSgameObject.transform.parent = (Transform)null;
        MissileEffect component = AMSgameObject.GetComponent<MissileEffect>();
        if (component == null) {
          WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.MissilePrefab.name), (UnityEngine.Object)this);
          return;
        }
        amsComponent = AMSgameObject.AddComponent<AMSMissileEffect>();
        amsComponent.Init(component);
        amsComponent.Init(this.weapon, this);
        this.missiles.Add(amsComponent);
      }
    }
    public override void ClearBullets() {
      Log.Combat?.WL(0,"AMSMissileLauncherEffect.ClearBullets");
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.MissilePrefab.name;
      for (int index = 0; index < this.missiles.Count; ++index) {
        GameObject gameObject = this.missiles[index].gameObject;
        Log.Combat?.WL(1, "returning to pool " + prefabName + " " + gameObject.GetInstanceID());
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
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "AMSMissileLauncherEffect.Fire");
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.firingIntervalRate = (double)this.firingInterval <= 0.0 ? 0.0f : 1f / this.firingInterval;
      this.volleyIntervalRate = (double)this.volleyInterval <= 0.0 ? this.firingIntervalRate : 1f / this.volleyInterval;
      this.FireMissile(hitIndex);
      this.PlayPreFire();
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "AMS missile launcher effect can't fire normaly. Something is wrong.");
      WeaponEffect.logger.LogError("AMS missile effect can't fire normally. Something is wrong.\n" + Environment.StackTrace);
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      base.PlayProjectile();
    }
    protected override void PlayImpact() {
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllMissilesComplete())
        return;
      this.OnComplete();
    }
    private void FireMissile(int missileIdx) {
      if (missileIdx < 0 || missileIdx >= this.missiles.Count) { return; }
      if (missileIdx == (this.missiles.Count - 1)) {
        if (this.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_end, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
        this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
      }
      AMSMissileEffect missile = this.missiles[missileIdx];
      int emitterIndex = missileIdx % this.numberOfEmitters;
      missile.tubeTransform = this.weaponRep.vfxTransforms[emitterIndex];
      missile.Fire(this.hitPositions, missileIdx, emitterIndex);
      if (missileIdx == 0) {
        if (this.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_launcher_start, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      }
      this.t = 0.0f;
      this.rate = this.firingIntervalRate;
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }

    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(hitDamage, structureDamage);
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
