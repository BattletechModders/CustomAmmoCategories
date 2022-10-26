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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class AMSLaserEffect : AMSWeaponEffect {
    public float lightIntensity = 3500000f;
    public float lightRadius = 100f;
    protected Color[] laserColor = new Color[2] { Color.white, Color.white };
    public string beamStartSFX;
    public string beamStopSFX;
    public string pulseSFX;
    public float pulseDelay;
    protected float pulseTime;
    public AnimationCurve laserAnim;
    protected MaterialPropertyBlock mpb;
    protected LineRenderer beamRenderer;
    protected BTLight laserLight;
    protected ParticleSystem impactParticles;
    protected float laserAlpha;
    public Color originalColor;
    public override void StoreOriginalColor() {
      this.originalColor = this.beamRenderer.material.GetColor("_ColorBB");
    }
    public override void SetColor(Color color) {
      this.beamRenderer.material.SetColor("_ColorBB", color);
    }
    public override void RestoreOriginalColor() {
      this.beamRenderer.material.SetColor("_ColorBB", this.originalColor);
    }
    public override float calculateInterceptCorrection(float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      return curPath / pathLenth;
    }
#if PUBLIC_ASSEMBLIES
    public override int ImpactPrecacheCount {
#else
    protected override int ImpactPrecacheCount {
#endif
      get {
        return 1;
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
    public virtual void Init(LaserEffect original) {
      base.Init(original);
      this.lightIntensity = original.lightIntensity;
      this.lightRadius = original.lightRadius;
      this.laserColor = (Color[])typeof(LaserEffect).GetField("laserColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.beamStartSFX = original.beamStartSFX;
      this.beamStopSFX = original.beamStopSFX;
      this.pulseSFX = original.pulseSFX;
      this.pulseDelay = original.pulseDelay;
      this.pulseTime = (float)typeof(LaserEffect).GetField("pulseTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserAnim = original.laserAnim;
      this.preFireDuration = 0.001f;
      this.mpb = (MaterialPropertyBlock)typeof(LaserEffect).GetField("mpb", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.beamRenderer = (LineRenderer)typeof(LaserEffect).GetField("beamRenderer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserLight = (BTLight)typeof(LaserEffect).GetField("laserLight", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.impactParticles = (ParticleSystem)typeof(LaserEffect).GetField("impactParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserAlpha = (float)typeof(LaserEffect).GetField("laserAlpha", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      CustomAmmoCategoriesLog.Log.LogWrite("AMS laser effect can't fire normaly. Something is wrong.\n");
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    public override void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      base.Fire(hitPositions, hitIndex, emitterIndex);
      this.duration = this.projectileSpeed;
      if ((double)this.duration <= 0.0)
        this.duration = 1f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
    }
    protected void SetupLaser() {
      this.beamRenderer = this.projectile.GetComponent<LineRenderer>();
      this.beamRenderer.SetPosition(0, this.startPos);
      this.beamRenderer.SetPosition(1, this.endPos);
      this.beamRenderer.shadowCastingMode = ShadowCastingMode.Off;
      this.beamRenderer.lightProbeUsage = LightProbeUsage.Off;
      if (VFXRenderer.HasInstance && !VFXRenderer.Instance.laserRenderers.Contains((Renderer)this.beamRenderer)) {
        VFXRenderer.Instance.laserRenderers.Add((Renderer)this.beamRenderer);
        this.beamRenderer.gameObject.layer = LayerMask.NameToLayer("Reflector");
      }
      this.laserLight = this.beamRenderer.GetComponentInChildren<BTLight>(true);
      if ((UnityEngine.Object)this.laserLight != (UnityEngine.Object)null) {
        if (!this.laserLight.gameObject.activeSelf)
          this.laserLight.gameObject.SetActive(true);
        if (!this.laserLight.enabled)
          this.laserLight.enabled = true;
        this.laserLight.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(-(double)Vector3.Distance(this.startPos, this.endPos) / 2.0));
        this.laserLight.transform.localRotation = Quaternion.identity;
        this.laserLight.length = Vector3.Distance(this.startPos, this.endPos);
        this.laserLight.RefreshLightSettings(true);
      }
      this.projectileTransform.parent = this.transform;
      this.projectileTransform.localPosition = Vector3.zero;
      this.projectileTransform.localRotation = Quaternion.identity;
      this.pulseTime = this.pulseDelay;
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayPreFire() {
#else
    protected override void PlayPreFire() {
#endif
      base.PlayPreFire();
      if (string.IsNullOrEmpty(this.beamStartSFX))
        return;
      int num = (int)WwiseManager.PostEvent(this.beamStartSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
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
      this.SetupLaser();
      this.PlayMuzzleFlash();
      if (!string.IsNullOrEmpty(this.pulseSFX)) {
        int num = (int)WwiseManager.PostEvent(this.pulseSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.PlayProjectile();
      this.projectileTransform.parent = (Transform)null;
      this.projectileTransform.position = this.endPos;
      this.PlayImpact();
    }
#if PUBLIC_ASSEMBLIES
    public override void PlayImpact() {
#else
    protected override void PlayImpact() {
#endif
      this.PlayImpactAudio();
      this.OnImpact(0.0f);
    }
    protected override void DestroyFlimsyObjects() {
    }
    protected override void Update() {
      base.Update();
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing) {
        if ((double)this.t >= 1.0)
          this.OnComplete();
        this.UpdateColor();
        if (this.laserAnim != null) {
          this.laserAlpha = this.laserAnim.Evaluate(this.t);
          this.laserColor[0].a = this.laserAlpha;
          this.laserColor[1].a = this.laserAlpha;
          this.beamRenderer.startColor = this.laserColor[0];
          this.beamRenderer.endColor = this.laserColor[1];
          if ((UnityEngine.Object)this.laserLight != (UnityEngine.Object)null) {
            this.laserLight.intensity = this.laserAlpha * this.lightIntensity;
            this.laserLight.radius = this.lightRadius;
          }
        }
        if ((double)this.pulseDelay > 0.0 && (double)this.t >= (double)this.pulseTime) {
          this.pulseTime += this.pulseDelay;
          if (!string.IsNullOrEmpty(this.pulseSFX)) {
            int num = (int)WwiseManager.PostEvent(this.pulseSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          }
        }
      }
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact)
        return;
      if ((double)this.t <= 1.0)
        this.t += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
      if ((double)this.t < 1.0)
        return;
      base.OnComplete();
    }
    protected override void LateUpdate() {
      base.LateUpdate();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }

    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
      base.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
        ,this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
      );
    }
    public override void AudioStop() {
      if (string.IsNullOrEmpty(this.beamStopSFX) == false) {
        int num = (int)WwiseManager.PostEvent(this.beamStopSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
    }

    protected override void OnComplete() {
      this.StopAudio();
      if (this.impactParticles != null) { this.impactParticles.Stop(true); }
      this.beamRenderer.SetPosition(0, this.startPos);
      this.beamRenderer.SetPosition(1, this.startPos);
      this.RestoreOriginalColor();
      if (VFXRenderer.HasInstance && VFXRenderer.Instance.laserRenderers.Contains(this.beamRenderer)) {
        VFXRenderer.Instance.laserRenderers.Remove(this.beamRenderer);
        this.beamRenderer.gameObject.layer = LayerMask.NameToLayer("VFXOnly");
      }
      if (this.laserLight != null)
        this.laserLight.intensity = 0.0f;
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
    public override void Reset() {
      try {
        this.StopAudio();
        if (this.impactParticles != null) { this.impactParticles.Stop(true); }
        if (this.beamRenderer != null) {
          this.beamRenderer.SetPosition(0, this.startPos);
          this.beamRenderer.SetPosition(1, this.startPos);
        }
        this.RestoreOriginalColor();
        if (VFXRenderer.HasInstance && VFXRenderer.Instance.laserRenderers.Contains(this.beamRenderer)) {
          VFXRenderer.Instance.laserRenderers.Remove(this.beamRenderer);
          this.beamRenderer.gameObject.layer = LayerMask.NameToLayer("VFXOnly");
        }
        if (this.laserLight != null)
          this.laserLight.intensity = 0.0f;
        base.Reset();
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
}
