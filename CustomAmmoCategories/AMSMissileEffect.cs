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
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class AMSMissileEffect : AMSWeaponEffect {
    public float impactLightIntensity = 1000000f;
    private AMSMissileLauncherEffect parentLauncher;
    public Transform tubeTransform;
    private bool isSRM;
    private Vector3 preFireEndPos;
    public DopplerEffect doppler;
    public void Init(MissileEffect original) {
      base.Init(original);
      Log.Combat?.WL(0, "AMSMissileEffect.Init");
      this.impactLightIntensity = original.impactLightIntensity;
      this.tubeTransform = original.tubeTransform;
      this.isSRM = true;
      this.doppler = original.doppler;
    }
    protected override int ImpactPrecacheCount {
      get {
        return 14;
      }
    }
    protected override void Awake() {
      base.Awake();
    }
    protected override void Start() {
      base.Start();
    }
    public void Init(Weapon weapon, AMSMissileLauncherEffect parentLauncher) {
      this.weaponImpactType = parentLauncher.weaponImpactType;
      this.Init(weapon);
      this.parentLauncher = parentLauncher;
      this.isSRM = parentLauncher.isSRM;
      this.projectileSpeed = parentLauncher.projectileSpeed;
      float max = this.projectileSpeed * 0.1f;
      this.projectileSpeed += Random.Range(-max, max);
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "AMSMissileEffect.Fire");
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.preFireEndPos = this.startingTransform.position;
      if ((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) {
        if ((UnityEngine.Object)this.doppler == (UnityEngine.Object)null)
          this.doppler = this.gameObject.AddComponent<DopplerEffect>();
        this.doppler.enabled = true;
        this.doppler.Init(this.projectileAudioObject, CameraControl.Instance.GetMainCamera().gameObject);
      }
      this.PlayPreFire();
    }
    public void Fire(WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, bool isIndirect) {
      Log.Combat?.WL(0, "AMS missile effect can't fire normally. Something is wrong.");
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
      this.PlayMuzzleFlash();
      this.startPos = this.preFireEndPos;
      this.currentPos = this.startPos;
      float num1 = Vector3.Distance(this.startPos, this.endPos);
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num1 / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      base.PlayProjectile();
      this.startPos = this.preFireEndPos;
      if (this.isSRM) {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_start, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        if (this.hitIndex >= this.hitInfo.numberOfShots) {
          int num3 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_missile_launch_last, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num4 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_missile_launch, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_start, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        if (this.hitIndex >= this.hitInfo.numberOfShots) {
          int num3 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_missile_launch_last, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num4 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_missile_launch, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      }
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      if (this.isSRM) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(false);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(false);
      this.OnImpact(0.0f);
    }
    private void SpawnImpactExplosion(string explosionName) {
    }
    protected override void Update() {
      base.Update();
      if (this.currentState == WeaponEffect.WeaponEffectState.PreFiring && (double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.startPos, this.preFireEndPos, this.t);
        this.projectileTransform.position = this.currentPos;
        if (this.currentPos != this.preFireEndPos)
          this.projectileTransform.LookAt(this.preFireEndPos);
      }
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.preFireEndPos, this.endPos, this.t);
        this.projectileTransform.position = this.currentPos;
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }
    private void ApplyDoppler(GameObject audioListener) {
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
        ,this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
      );
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)
        this.projectileParticles.Stop(true);
      if (!((UnityEngine.Object)this.doppler != (UnityEngine.Object)null))
        return;
      this.doppler.enabled = false;
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
      if (this.Active) {
        if (this.isSRM) {
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        } else {
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
        }
      }
      base.Reset();
    }
  }
}
