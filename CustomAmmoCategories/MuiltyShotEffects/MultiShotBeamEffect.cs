using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.UrbanWarfare;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public enum ColorChangeRule {
    None,Linear,Random,RandomOnce,t0,t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16,t17,t18,t19,t20,t21,t22,t23,t24,t25,t26,t27,t28,t29,t30,t31
  }
  public class ColorTableJsonEntry {
    public ColorTableJsonEntry() {
      FRealColor = Color.magenta;
      FI = 5f;
      SyncColor();
    }
    private void SyncColor() {
      FColor.r = FRealColor.r * FI;
      FColor.g = FRealColor.g * FI;
      FColor.b = FRealColor.b * FI;
      FColor.a = 1f;
    }
    public string C {
      set {
        Color temp;
        if(ColorUtility.TryParseHtmlString(value,out temp)) {
          FRealColor = temp;
          SyncColor();
        } else {
          Log.LogWrite("Bad color:"+value+"\n",true);
        }
      }
    }
    public float I { set {
        FI = value; SyncColor();
    } }
    [JsonIgnore]
    public Color Color { get {
        return FColor;
      }
    }
    [JsonIgnore]
    private Color FColor;
    [JsonIgnore]
    private Color FRealColor;
    [JsonIgnore]
    private float FI;

  }
  public class ColorPair {
    public Color Start;
    public Color End;
    public ColorPair(Color s, Color e) { this.Start = s; this.End = e; }
  }
  public class MultiShotBeamEffect : CopyAbleWeaponEffect {
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
    public MultiShotLaserEffect parentProjector;
    public int beamIdx;
    public bool primeBeam;
    protected override int ImpactPrecacheCount {
      get {
        return 1;
      }
    }

    protected override void Awake() {
      base.Awake();
    }

    protected override void Start() {
      base.Start();
    }
    public void Init(LaserEffect original) {
      base.Init(original);
      CustomAmmoCategoriesLog.Log.LogWrite("MultiShotLaserEffect.Init\n");
      this.lightIntensity = original.lightIntensity;
      this.lightRadius = original.lightRadius;
      this.laserColor = (Color[])typeof(LaserEffect).GetField("laserColor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.beamStartSFX = original.beamStartSFX;
      this.beamStopSFX = original.beamStopSFX;
      this.pulseSFX = original.pulseSFX;
      this.pulseDelay = original.pulseDelay;
      this.pulseTime = (float)typeof(LaserEffect).GetField("pulseTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserAnim = original.laserAnim;
      this.mpb = (MaterialPropertyBlock)typeof(LaserEffect).GetField("mpb", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.beamRenderer = (LineRenderer)typeof(LaserEffect).GetField("beamRenderer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserLight = (BTLight)typeof(LaserEffect).GetField("laserLight", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.impactParticles = (ParticleSystem)typeof(LaserEffect).GetField("impactParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.laserAlpha = (float)typeof(LaserEffect).GetField("laserAlpha", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
  }

  public void Init(Weapon weapon, MultiShotLaserEffect parentProjector) {
      Log.LogWrite("MultiShotBeamEffect.Init\n");
      this.Init(weapon);
      this.parentProjector = parentProjector;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.subEffect = true;
    }

    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0, bool pb = false) {
      Log.LogWrite("MultiShotBeamEffect.Fire " + hitInfo.attackWeaponIndex + " " + hitIndex + " ep:" + hitInfo.hitPositions[hitIndex] + " prime:" + pb + "\n");
      this.primeBeam = pb;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      endPos.x += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      endPos.y += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      endPos.z += Random.Range(-this.parentProjector.spreadAngle, this.parentProjector.spreadAngle);
      this.endPos = endPos;
      this.duration = this.projectileSpeed * this.weapon.ProjectileSpeedMultiplier();
      if ((double)this.duration <= 0.0) { this.duration = 1f; }
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
      Component[] components = this.projectile.GetComponentsInChildren<Component>();
      Log.LogWrite("MultiShotBeamEffect.SetupLaser\n");
      foreach (Component component in components) {
        Log.LogWrite(" "+component.name+":"+component.GetType().ToString()+"\n");
      }
      Log.LogWrite("this.beamRenderer.Materials\n");
      foreach (Material material in this.beamRenderer.materials) {
        Log.LogWrite(" " + material.name + ":" + material.shader.name + " "+ material.GetColor("_ColorBB") + "\n");
      }
    }
    //public Color originalColor;
    public override void StoreOriginalColor() {
       this.beamRenderer.material.RegisterRestoreColor();
    }
    public override void SetColor(Color color) {
      this.beamRenderer.material.SetColor("_ColorBB",color);
    }
    public override void RestoreOriginalColor() {
      //this.beamRenderer.material.SetColor("_ColorBB", this.originalColor);
    }

    protected override void PlayPreFire() {
      base.PlayPreFire();
      if (string.IsNullOrEmpty(this.beamStartSFX))
        return;
      int num = (int)WwiseManager.PostEvent(this.beamStartSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
    }

    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }

    protected override void PlayProjectile() {
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

    protected override void PlayImpact() {
      this.PlayImpactAudio();
      if (this.hitInfo.DidShotHitAnything(this.hitIndex) && !string.IsNullOrEmpty(this.impactVFXBase)) {
        if ((UnityEngine.Object)this.impactParticles != (UnityEngine.Object)null)
          this.impactParticles.Stop(true);
        string str1 = string.Empty;
        AbstractActor actorByGuid = this.Combat.FindActorByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
        if (actorByGuid != null && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536 && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)actorByGuid.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex)))
          str1 = "_crit";
        else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0)
          str1 = "_" + this.impactVFXVariations[Random.Range(0, this.impactVFXVariations.Length)];
        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)("WeaponEffect.PlayImpact had an invalid VFX name: " + str2));
        } else {
          this.impactParticles = gameObject.GetComponent<ParticleSystem>();
          this.impactParticles.Stop(true);
          this.impactParticles.Clear(true);
          this.impactParticles.transform.position = this.endPos;
          this.impactParticles.transform.LookAt(this.startingTransform.position);
          BTCustomRenderer.SetVFXMultiplier(this.impactParticles);
          this.impactParticles.Play(true);
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, this.impactParticles);
        }
      }
      this.PlayImpactDamageOverlay();
      this.DestroyFlimsyObjects();
      this.OnImpact(0.0f);
    }

    protected override void DestroyFlimsyObjects() {
      if (!this.shotsDestroyFlimsyObjects)
        return;
      Vector3 normalized = (this.endPos - this.startPos).normalized;
      RaycastHit[] raycastHitArray = Physics.SphereCastAll(this.startPos, 2f, normalized, Vector3.Distance(this.startPos, this.endPos), -5, QueryTriggerInteraction.Ignore);
      float num = this.weapon.DamagePerShot + this.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
      for (int index = 0; index < raycastHitArray.Length; ++index) {
        RaycastHit raycastHit = raycastHitArray[index];
        DestructibleObject component1 = raycastHit.collider.gameObject.GetComponent<DestructibleObject>();
        DestructibleUrbanFlimsy component2 = raycastHit.collider.gameObject.GetComponent<DestructibleUrbanFlimsy>();
        if ((UnityEngine.Object)component1 != (UnityEngine.Object)null && component1.isFlimsy) {
          component1.TakeDamage(raycastHit.point, normalized, num);
          component1.Collapse(normalized, num);
        }
        if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
          component2.PlayDestruction(normalized, num);
      }
    }

    protected override void Update() {
      base.Update();
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing) {
        if ((double)this.t >= 1.0) { this.OnComplete(); }
        this.UpdateColor();
        //this.beamRenderer.material.SetColor("_ColorBB", this.laserColor[0]);
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
      if (this.currentState != WeaponEffect.WeaponEffectState.WaitingForImpact) { return; };
      if ((double)this.t <= 1.0) {
        this.t += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
      }
      if ((double)this.t < 1.0) { return; };
      base.OnComplete();
    }

    protected override void LateUpdate() {
      base.LateUpdate();
    }

    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
#if BT1_8
    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
#else
    protected override void OnImpact(float hitDamage = 0.0f) {
#endif
      //base.OnImpact(this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask));
      Log.LogWrite("MultiShotBeamEffect.OnImpact wi:" + this.hitInfo.attackWeaponIndex + " hi:" + this.hitInfo + " bi:" + this.beamIdx + " prime:" + this.primeBeam + "\n");
      if (this.primeBeam) {
        Log.LogWrite(" prime. Damage message fired\n");
        float damage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
#if BT1_8
        float apDamage = this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
#endif
        if (this.weapon.DamagePerPallet()&&(this.weapon.DamageNotDivided() == false)) {
          damage /= this.weapon.ProjectilesPerShot;
#if BT1_8
          apDamage /= this.weapon.ProjectilesPerShot;
#endif
        };
#if BT1_8
        base.OnImpact(damage, apDamage);
#else
        base.OnImpact(damage);
#endif
      } else {
        Log.LogWrite(" no prime. No damage message fired\n");
      }
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; };
      this.projectileParticles.Stop(true);
    }

    protected override void OnComplete() {
      if (!string.IsNullOrEmpty(this.beamStopSFX)) {
        int num = (int)WwiseManager.PostEvent(this.beamStopSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      if ((UnityEngine.Object)this.impactParticles != (UnityEngine.Object)null) {
        this.impactParticles.Stop(true);
      }
      //this.RestoreOriginalColor();
      this.beamRenderer.SetPosition(0, this.startPos);
      this.beamRenderer.SetPosition(1, this.startPos);
      if (VFXRenderer.HasInstance && VFXRenderer.Instance.laserRenderers.Contains((Renderer)this.beamRenderer)) {
        VFXRenderer.Instance.laserRenderers.Remove((Renderer)this.beamRenderer);
        this.beamRenderer.gameObject.layer = LayerMask.NameToLayer("VFXOnly");
      }
      if ((UnityEngine.Object)this.laserLight != (UnityEngine.Object)null) {
        this.laserLight.intensity = 0.0f;
      }
      this.duration = this.projectileSpeed;
      if ((double)this.duration <= 0.0) { this.duration = 1f; }
      this.rate = 1f / this.duration;
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }

    public override void Reset() {
      if (this.Active && !string.IsNullOrEmpty(this.beamStopSFX)) {
        int num = (int)WwiseManager.PostEvent(this.beamStopSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      base.Reset();
    }
  }
}
