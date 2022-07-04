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
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class AMSBulletEffect : AMSWeaponEffect {
    public int bulletIdx;
    private AMSBallisticEffect parentLauncher;
    public Color originalColor;
    public TrailRenderer trailRendered;
    public override void StoreOriginalColor() {
      this.trailRendered = this.projectile.GetComponentInChildren<TrailRenderer>();
      if (trailRendered != null) { this.originalColor = this.trailRendered.material.GetColor("_ColorBB"); };
    }
    public override void SetColor(Color color) {
      float coeff = color.maxColorComponent;
      Color tempColor = Color.white;
      tempColor.a = 1f;
      tempColor.r = (color.r / coeff) * 8.0f + 1f;
      tempColor.g = (color.g / coeff) * 8.0f + 1f;
      tempColor.b = (color.b / coeff) * 8.0f + 1f;
      if (trailRendered != null) { this.trailRendered.material.SetColor("_ColorBB", tempColor); };
    }
    public override void RestoreOriginalColor() {
      if (trailRendered != null) { this.trailRendered.material.SetColor("_ColorBB", this.originalColor); };
    }
    public void Init(BulletEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("AMSBulletEffect.Init\n");
      this.bulletIdx = original.bulletIdx;
    }

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
    }

#if PUBLIC_ASSEMBLIES
    public override void Start() {
#else
    protected override void Start() {
# endif
      base.Start();
    }
    public void Init(Weapon weapon, AMSBallisticEffect parentLauncher) {
      this.Init(weapon);
      this.parentLauncher = parentLauncher;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.projectileSpeed = parentLauncher.projectileSpeed;
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.LogWrite("AMSBulletEffect.Fire\n");
      base.Fire(hitPositions, hitIndex, emitterIndex);
      Vector3 endPos = this.endPos;
      endPos.x += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.y += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      endPos.z += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
      this.endPos = endPos;
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS ballistic effect can't fire normaly. Something is wrong.\n");
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }

#if PUBLIC_ASSEMBLIES
    public override void PlayPreFire() {
#else
    protected override void PlayPreFire() {
#endif
      base.PlayPreFire();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayMuzzleFlash() {
#else
    protected override void PlayMuzzleFlash() {
#endif
      base.PlayMuzzleFlash();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayProjectile() {
#else
    protected override void PlayProjectile() {
#endif
      base.PlayProjectile();
      this.PlayMuzzleFlash();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayImpact() {
#else
    protected override void PlayImpact() {
#endif
      this.PlayImpactAudio();
      base.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void Update() {
#else
    protected override void Update() {
#endif
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.startPos, this.endPos, this.t);
        this.projectileTransform.position = this.currentPos;
        this.UpdateColor();
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }
#if PUBLIC_ASSEMBLIES
    public override void OnPreFireComplete() {
#else
    protected override void OnPreFireComplete() {
#endif
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
#if PUBLIC_ASSEMBLIES
    public override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
#else
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
#endif
      //this.parentLauncher.OnBulletImpact(this);
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; }
      this.projectileParticles.Stop(true);
    }
#if PUBLIC_ASSEMBLIES
    public override void OnComplete() {
#else
    protected override void OnComplete() {
#endif
      this.RestoreOriginalColor();
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
