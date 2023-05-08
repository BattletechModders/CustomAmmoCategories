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

  public class FragBallisticEffect : FragWeaponEffect {
    public static readonly string FragPrefabPrefix = "_FRAG_";
    private List<FragBulletEffect> bullets = new List<FragBulletEffect>();
    public float shotDelay;
    public float spreadAngle;
    public GameObject bulletPrefab;
    public string firstShotSFX;
    public string middleShotSFX;
    public string lastShotSFX;

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
      Log.Combat?.WL(1, "FragBallisticEffect.Init");
      this.shotDelay = original.shotDelay;
      this.spreadAngle = original.spreadAngle;
      this.bulletPrefab = original.bulletPrefab;
      this.firstShotSFX = original.firstShotSFX;
      this.middleShotSFX = original.middleShotSFX;
      this.lastShotSFX = original.lastShotSFX;
      this.subEffect = true;
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if (!((UnityEngine.Object)this.bulletPrefab != (UnityEngine.Object)null))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(this.bulletPrefab.name, BattleTechResourceType.Prefab, weapon.ProjectilesPerShot);
    }
    protected void SetupBullets() {
      if ((double)this.shotDelay <= 0.0)
        this.shotDelay = 0.5f;
      this.rate = 1f / this.shotDelay;
      this.ClearBullets();
      string prefabName = FragBallisticEffect.FragPrefabPrefix + this.bulletPrefab.name;
      Log.Combat?.WL(1, "FragBallisticEffect.SetupBullets getting from pool:" + prefabName);
      for (int index = 0; index < this.weapon.ProjectilesPerShot; ++index) {
        GameObject FraggameObject = this.Combat.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        FragBulletEffect fragComponent = null;
        if (FraggameObject != null) {
          Log.Combat?.WL(1, "getted from pool: " + FraggameObject.GetInstanceID());
          fragComponent = FraggameObject.GetComponent<FragBulletEffect>();
          if (fragComponent != null) {
            fragComponent.Init(this.weapon, this);
            this.bullets.Add(fragComponent);
          }
        }
        if (fragComponent == null) {
          Log.Combat?.WL(1, "not in pool. instansing.");
          GameObject gameObject = this.Combat.DataManager.PooledInstantiate(this.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
            WeaponEffect.logger.LogError((object)("Error instantiating BulletObject " + this.bulletPrefab.name), (UnityEngine.Object)this);
            break;
          }
          FraggameObject = GameObject.Instantiate(gameObject);
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          } else {
            AutoPoolObject FragautoPoolObject = FraggameObject.GetComponent<AutoPoolObject>();
            if (FragautoPoolObject != null) { GameObject.Destroy(FragautoPoolObject); };
          }
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.bulletPrefab.name, 4f);
          gameObject = null;
          FraggameObject.transform.parent = (Transform)null;
          BulletEffect component = FraggameObject.GetComponent<BulletEffect>();
          if (component == null) {
            WeaponEffect.logger.LogError((object)("Error finding BulletEffect on GO " + this.bulletPrefab.name), (UnityEngine.Object)this);
            return;
          }

          fragComponent = FraggameObject.AddComponent<FragBulletEffect>();
          fragComponent.Init(component);
          fragComponent.Init(this.weapon, this);
          this.bullets.Add(fragComponent);
        }
      }
    }
    protected void ClearBullets() {
      string prefabName = FragBallisticEffect.FragPrefabPrefix + this.bulletPrefab.name;
      Log.LogWrite("FragBallisticEffect.ClearBullets\n");
      for (int index = 0; index < this.bullets.Count; ++index) {
        if (this.bullets[index] == null) { continue; }
        GameObject gameObject = this.bullets[index].gameObject;
        if (gameObject == null) { continue; };
        Log.LogWrite(" returning to pool " + prefabName + " " + gameObject.GetInstanceID() + "\n");
        this.Combat.DataManager.PoolGameObject(prefabName, gameObject);
        this.bullets[index] = null;
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
    public override void Fire(Vector3 sPos,WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("FragBallisticEffect.Fire "+sPos+" wi:"+hitInfo.attackWeaponIndex+" hi:"+hitIndex+"\n");
      base.Fire(sPos,hitInfo, hitIndex, emitterIndex);
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
      this.FireBullets();
    }
    protected void FireBullets() {
      Log.LogWrite("FragBallisticEffect.FireBullets " + this.startPos + " wi:" + hitInfo.attackWeaponIndex + " hi:" + hitIndex + "\n");
      if (this.hitIndex < 0 || this.hitIndex >= this.hitInfo.hitLocations.Length) { return; }
      int limit = this.bullets.Count;
      if ((this.bullets.Count + this.hitIndex) > this.hitInfo.hitLocations.Length) {
        int delLimit = this.hitInfo.hitLocations.Length - this.hitIndex;
        while (this.bullets.Count > delLimit) {
          FragBulletEffect delBullet = this.bullets[this.bullets.Count - 1];
          GameObject.Destroy(delBullet.gameObject);
          this.bullets.RemoveAt(this.bullets.Count - 1);
        }
      }
      this.PlayMuzzleFlash();
      string eventName = this.firstShotSFX;
      if (!string.IsNullOrEmpty(eventName)) {
        int num = (int)WwiseManager.PostEvent(eventName, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      for (int index = 0; index < this.bullets.Count; ++index) { 
        FragBulletEffect bullet = this.bullets[index];
        bullet.bulletIdx = index;
        bullet.Fire(this.startPos,this.hitInfo, this.hitIndex+index, 0);
      }
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
    protected override void PlayImpact() {
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact || !this.AllBulletsComplete()) { return; }
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      Log.Combat?.WL(0, "FragBallisticEffect.OnPreFireComplete " + this.startPos + " wi:" + hitInfo.attackWeaponIndex + " hi:" + hitIndex);
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(0.0f, 0f);
    }
    public void OnBulletImpact(FragBulletEffect bullet) {
      //this.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask));
      /*if (this.hitInfo.hitLocations[this.hitIndex] == 0 || this.hitInfo.hitLocations[this.hitIndex] == 65536 || bullet.bulletIdx >= this.weapon.ProjectilesPerShot - 1)
        return;
      AbstractActor combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId) as AbstractActor;
      if (combatantByGuid == null || !((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null))
        return;
      combatantByGuid.GameRep.PlayImpactAnim(this.hitInfo, this.hitIndex, this.weapon, MeleeAttackType.NotSet, 0.0f);*/
    }
    protected override void OnComplete() {
      //this.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) * (float)this.weapon.ShotsWhenFired);
      base.OnComplete();
      Log.Combat?.WL(0, "FragBallisticEffect.Complete");
      if (this.parentWeaponEffect != null) {
        int hitIndex = this.parentWeaponEffect.hitIndex;
        Log.Combat?.WL(1, "parent weapon found " + this.parentWeaponEffect.hitInfo.attackWeaponIndex+":"+ hitIndex);
        this.parentWeaponEffect.PublishWeaponCompleteMessage();
      }
    }
    public override void Reset() {
      base.Reset();
    }
  }
}
