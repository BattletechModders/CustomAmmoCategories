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
using System.Text;
using UnityEngine;

namespace CustAmmoCategories {
  public class AMSMultiShotWeaponEffect: AMSWeaponEffect {
    public static readonly string AMSPrefabPrefix = "_AMS_";
    public virtual void AddBullet() { }
    public virtual void ClearBullets() { }
    public virtual int BulletsCount() { return 0; }
    public virtual float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      return curPath / pathLenth;
    }
  }
  public class AMSBallisticEffect : AMSMultiShotWeaponEffect {
    private List<AMSBulletEffect> bullets = new List<AMSBulletEffect>();
    public float shotDelay;
    public float spreadAngle;
    public GameObject bulletPrefab;
    public string AMSbulletPrefabName;
    public string firstShotSFX;
    public string middleShotSFX;
    public string lastShotSFX;
    public override float calculateInterceptCorrection(int shotIdx, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      if (shotIdx < 0) { return curPath / pathLenth; }
      if (shotIdx >= this.bullets.Count) { return curPath / pathLenth; };
      return this.bullets[shotIdx].calculateInterceptCorrection(curPath, pathLenth, distance, missileProjectileSpeed);
    }
    public void Init(BallisticEffect original) {
      base.Init(original);
      Log.Combat?.WL(0, "AMSBallisticEffect.Init");
      this.shotDelay = original.shotDelay;
      this.spreadAngle = original.spreadAngle;
      this.bulletPrefab = original.bulletPrefab;
      this.AMSbulletPrefabName = "AMS_"+original.bulletPrefab.name;
      this.firstShotSFX = original.firstShotSFX;
      this.middleShotSFX = original.middleShotSFX;
      this.lastShotSFX = original.lastShotSFX;
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
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (this.bulletPrefab == null) { return; }
      this.Combat.DataManager.PrecachePrefabAsync(this.bulletPrefab.name, BattleTechResourceType.Prefab, weapon.ProjectilesPerShot);
    }
    public override int BulletsCount() { return this.bullets.Count; }
    public override void AddBullet() {
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.bulletPrefab.name;
      Log.Combat?.WL(0, "AMSBallisticEffect.AddBullet getting from pool:" + prefabName);
      GameObject AMSgameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      AMSBulletEffect amsComponent = null;
      if (AMSgameObject != null) {
        Log.Combat?.WL(1, "getted from pool: " + AMSgameObject.GetInstanceID());
        amsComponent = AMSgameObject.GetComponent<AMSBulletEffect>();
        if (amsComponent != null) {
          amsComponent.Init(this.weapon, this);
          this.bullets.Add(amsComponent);
        }
      }
      if (amsComponent == null) {
        Log.Combat?.WL(0, "not in pool. instansing.");
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
          return;
        }
        AMSgameObject = GameObject.Instantiate(gameObject);
        //AMSgameObject.name = prefabName;
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        } else {
          AutoPoolObject AMSautoPoolObject = AMSgameObject.GetComponent<AutoPoolObject>();
          if (AMSautoPoolObject != null) { GameObject.Destroy(AMSautoPoolObject); };
        }
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.bulletPrefab.name, 4f);
        gameObject = null;
        AMSgameObject.transform.parent = (Transform)null;
        BulletEffect component = AMSgameObject.GetComponent<BulletEffect>();
        if ((UnityEngine.Object)component == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.bulletPrefab.name), (UnityEngine.Object)this);
          return;
        }
        amsComponent = AMSgameObject.AddComponent<AMSBulletEffect>();
        amsComponent.Init(component);
        amsComponent.Init(this.weapon, this);
        this.bullets.Add(amsComponent);
      }
    }
    public override void ClearBullets() {
      Log.Combat?.WL(0, "AMSBulletEffect.ClearBullets");
      string prefabName = AMSMultiShotWeaponEffect.AMSPrefabPrefix + this.bulletPrefab.name;
      for (int index = 0; index < this.bullets.Count; ++index) {
        GameObject gameObject = this.bullets[index].gameObject;
        Log.Combat?.WL(1, "returning to pool " + prefabName+" "+gameObject.GetInstanceID());
        this.Combat.DataManager.PoolGameObject(prefabName,gameObject);
      }
      this.bullets.Clear();
    }
    protected bool AllBulletsComplete() {
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing && this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return false;
      for (int index = 0; index < this.bullets.Count; ++index) {
        if (!this.bullets[index].FiringComplete)
          return false;
      }
      return true;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "AMS ballistic effect can't fire normaly. Something is wrong.");
      WeaponEffect.logger.LogError("AMS ballistic effect can't fire normaly. Something is wrong.\n"+Environment.StackTrace);
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      base.Fire(hitPositions, hitIndex, emitterIndex);
      if ((double)this.shotDelay <= 0.0) { this.shotDelay = 0.5f; };
      this.rate = 1f / this.shotDelay;
      this.FireBullet(hitIndex);
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
    }
    protected void FireBullet(int bulletIdx) {
      if (bulletIdx < 0 || bulletIdx >= this.bullets.Count)
        return;
      AMSBulletEffect bullet = this.bullets[bulletIdx];
      bullet.bulletIdx = bulletIdx;
      bullet.Fire(this.hitPositions, bulletIdx, 0);
      this.PlayMuzzleFlash();
      string empty = string.Empty;
      string eventName = bulletIdx != 0 ? (bulletIdx != this.bullets.Count - 1 ? this.middleShotSFX : this.lastShotSFX) : this.firstShotSFX;
      if (!string.IsNullOrEmpty(eventName)) {
        int num = (int)WwiseManager.PostEvent(eventName, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      this.t = 0.0f;
      if (bulletIdx < this.bullets.Count) {
        return;
      }
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
    protected override void PlayImpact() {
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete())
        return;
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
      if (hitDamage <= 0.001f) { return; }
      base.OnImpact(hitDamage, structureDamage);
    }
    public void OnBulletImpact(BulletEffect bullet) {
      this.OnImpact(0.0f);
    }
    protected override void OnComplete() {
      this.OnImpact(0.0f);
      base.OnComplete();
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
