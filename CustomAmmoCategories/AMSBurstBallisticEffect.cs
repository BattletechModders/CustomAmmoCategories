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
  public class AMSBurstBallisticEffect : AMSWeaponEffect {
    public float impactTime = 0.5f;
    private int bulletsFired;
    public GameObject accurateProjectilePrefab;
    public GameObject inaccurateProjectilePrefab;
    public string preFireSoundEvent;
    public string projectileSoundEvent;
    public string fireCompleteStopEvent;
    private float floatieInterval;
    private float nextFloatie;
    public void Init(BurstBallisticEffect original) {
      base.Init(original);
      Log.Combat?.WL(0,"AMSBurstBallisticEffect.Init");
      this.impactTime = original.impactTime;
      this.bulletsFired = original.bulletsFired;
      this.accurateProjectilePrefab = original.accurateProjectilePrefab;
      this.inaccurateProjectilePrefab = original.inaccurateProjectilePrefab;
      this.preFireSoundEvent = original.preFireSoundEvent;
      this.projectileSoundEvent = original.projectileSoundEvent;
      this.fireCompleteStopEvent = original.fireCompleteStopEvent;
      this.floatieInterval = original.floatieInterval;
      this.nextFloatie = original.nextFloatie;
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
      this.floatieInterval = 1f / (float)this.weapon.ProjectilesPerShot;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0, "AMS burst ballistic effect can't fire normaly. Something is wrong.");
      WeaponEffect.logger.LogError("AMS effect can't fire normaly. Something is wrong.\n" + Environment.StackTrace);
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      this.projectilePrefab = this.accurateProjectilePrefab;
      this.bulletsFired = 0;
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.nextFloatie = 0.0f;
      this.impactTime = Mathf.Clamp01(this.impactTime);
      this.duration = this.projectileSpeed;
      if ((double)this.duration > 4.0) { this.duration = 4f; };
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    protected override void PlayPreFire() {
      base.PlayPreFire();
      this.t = 0.0f;
      if (string.IsNullOrEmpty(this.preFireSoundEvent))
        return;
      int num = (int)WwiseManager.PostEvent(this.preFireSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      this.PlayMuzzleFlash();
      this.t = 0.0f;
      if (!string.IsNullOrEmpty(this.projectileSoundEvent)) {
        int num = (int)WwiseManager.PostEvent(this.projectileSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.PlayProjectile();
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing) { return; }
      if ((double)this.t < 1.0) { return; };
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
      base.OnImpact(hitDamage, structureDamage);
    }
    public override void AudioStop() {
      if (string.IsNullOrEmpty(this.fireCompleteStopEvent) == false) {
        int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
    }
    protected override void OnComplete() {
      base.OnComplete();
      if (this.projectileParticles != null)
        this.projectileParticles.Stop(true);
    }
    public void OnDisable() {
      this.StopAudio();
    }
    public override void Reset() {
      this.StopAudio();
      base.Reset();
    }
  }
}
