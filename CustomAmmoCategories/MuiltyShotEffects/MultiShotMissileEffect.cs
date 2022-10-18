using BattleTech;
using BattleTech.Rendering;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class MultiShotMissileEffect : CopyAbleWeaponEffect {
  public float impactLightIntensity = 1000000f;
  private MultiShotMissileLauncherEffect parentLauncher;
  public Transform tubeTransform;
  private bool isSRM;
  private bool isIndirect;
  private Vector3 preFireEndPos;
  private CurvySpline spline;
  public DopplerEffect doppler;
  public bool primeMissile;
  public int missileIdx;
  public bool intercepted;

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

  public void Init(MissileEffect original) {
    this.Init(original);
    CustomAmmoCategoriesLog.Log.LogWrite("MultiShotMissileEffect.Init\n");
    this.impactLightIntensity = original.impactLightIntensity;
    this.parentLauncher = null;
    this.tubeTransform = original.tubeTransform;
    this.isSRM = false;
    this.isIndirect = false;
    this.preFireEndPos = Vector3.zero;
    this.spline = null;
    this.doppler = original.doppler;
    this.primeMissile = false;
    this.missileIdx = 0;
    this.intercepted = false;
  }

  public void Init(Weapon weapon, MultiShotMissileLauncherEffect parentLauncher) {
    this.weaponImpactType = parentLauncher.weaponImpactType;
    this.Init(weapon);
    this.parentLauncher = parentLauncher;
    this.isSRM = parentLauncher.isSRM;
    this.projectileSpeed = parentLauncher.projectileSpeed;
    float max = this.projectileSpeed * 0.1f;
    this.projectileSpeed += Random.Range(-max, max);
    if (this.spline != null)
      return;
    this.spline = this.gameObject.AddComponent<CurvySpline>();
  }

  public void FireEx(WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, bool isIndirect) {
    this.SetupCustomSettings();
    this.Fire(hitInfo, hitIndex, emitterIndex);
    this.isIndirect = isIndirect;
    this.preFireEndPos = this.startingTransform.position;
    if (this.weapon.parent.GameRep != null) {
      if (this.doppler == null)
        this.doppler = this.gameObject.AddComponent<DopplerEffect>();
      this.doppler.enabled = true;
      this.doppler.Init(this.projectileAudioObject, CameraControl.Instance.GetMainCamera().gameObject);
    }
    this.PlayPreFire();
  }

  protected override void PlayPreFire() {
    base.PlayPreFire();
  }

  protected override void PlayMuzzleFlash() {
    base.PlayMuzzleFlash();
  }
  public override void SetupCustomSettings() {
    this.customPrefireSFX = this.preFireSFX;
    switch (this.playSFX) {
      case PlaySFXType.First: this.customPrefireSFX = this.parentLauncher.firstPreFireSFX; break;
      case PlaySFXType.Middle: this.customPrefireSFX = this.parentLauncher.middlePrefireSFX; break;
      case PlaySFXType.Last: this.customPrefireSFX = this.parentLauncher.lastPreFireSFX; break;
      case PlaySFXType.None: this.customPrefireSFX = string.Empty; break;
    }
    this.fireSFX = string.Empty;
    switch (this.playSFX) {
      case PlaySFXType.First: this.fireSFX = this.parentLauncher.firstFireSFX; break;
      case PlaySFXType.Middle: this.fireSFX = this.parentLauncher.middlefireSFX; break;
      case PlaySFXType.Last: this.fireSFX = this.parentLauncher.lastFireSFX; break;
      case PlaySFXType.None: this.fireSFX = string.Empty; break;
    }
    if (this.playSFX != PlaySFXType.None) {
      this.preFireStartSFX = weapon.preFireStartSFX();
      this.preFireStopSFX = weapon.preFireStopSFX();
      this.customPulseSFX = weapon.pulseSFX();
      this.customPulseSFXdelay = weapon.pulseSFXdelay();
      this.projectileFireSFX = weapon.projectileFireSFX();
      this.projectilePrefireSFX = weapon.projectilePreFireSFX();
      this.projectileStopSFX = weapon.projectileStopSFX();
      if (this.preFireStartSFX == null) { this.preFireStartSFX = string.Empty; }
      if (this.preFireStopSFX == null) { this.preFireStopSFX = string.Empty; }
      if (this.customPulseSFX == null) { this.customPulseSFX = string.Empty; }
      if (this.projectileFireSFX == null) { this.projectileFireSFX = this.isSRM?"AudioEventList_srm_srm_projectile_start": "AudioEventList_lrm_lrm_projectile_start"; }
      if (this.projectilePrefireSFX == null) { this.projectilePrefireSFX = string.Empty; }
      if (this.projectileStopSFX == null) { this.projectileStopSFX = this.isSRM ? "AudioEventList_srm_srm_projectile_stop" : "AudioEventList_lrm_lrm_projectile_stop"; }
      if (customPulseSFXdelay < CustomAmmoCategories.Epsilon) { this.customPulseSFXdelay = -1f; }
    } else {
      this.preFireStartSFX = string.Empty;
      this.preFireStopSFX = string.Empty;
      this.customPulseSFXdelay = 0f;
      this.customPulseSFX = string.Empty;
    }
    if (weapon.prefireDuration() > CustomAmmoCategories.Epsilon) {
      this.preFireDuration = weapon.prefireDuration();
    } else {
      this.preFireDuration = this.originalPrefireDuration;
    }
    if (weapon.ProjectileSpeed() > CustomAmmoCategories.Epsilon) {
      this.projectileSpeed = weapon.ProjectileSpeed();
    } else {
      this.projectileSpeed = this.parentLauncher.projectileSpeed;
    }
    this.projectileSpeed *= weapon.ProjectileSpeedMultiplier();
  }

  protected override void PlayProjectile() {
    this.PlayMuzzleFlash();
    AdvWeaponHitInfoRec cachedCurve = this.hitInfo.advRec(hitIndex);
    if (cachedCurve != null) {
      CustomAmmoCategoriesLog.Log.LogWrite("Cached missile path found " + this.weapon.defId + " " + this.hitInfo.attackSequenceId + " " + this.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("Altering start/end pos and projectile speed\n");
      this.startPos = cachedCurve.startPosition;
      this.preFireEndPos = cachedCurve.startPosition;
      this.endPos = cachedCurve.hitPosition;
      CustomAmmoCategoriesLog.Log.LogWrite("Intercept " + this.hitInfo.attackWeaponIndex + " " + hitIndex + " T:" + cachedCurve.interceptInfo.getAMSShootT() + " " + cachedCurve.interceptInfo.AMSShootIndex + "/" + cachedCurve.interceptInfo.AMSShoots.Count + "\n");
      this.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.getAMSShootT();
      this.projectileSpeed = cachedCurve.projectileSpeed;
      float num1 = Vector3.Distance(this.startPos, this.endPos);
      if ((double)this.projectileSpeed > 0.0) {
        this.duration = num1 / this.projectileSpeed;
      } else {
        this.duration = 1f;
      }
      if ((double)this.duration > 4.0) { this.duration = 4f; }
      this.rate = 1f / this.duration;
      CustomAmmoCategoriesLog.Log.LogWrite("Altering trajectory\n");
      this.spline.Interpolation = CurvyInterpolation.Bezier;
      this.spline.Clear();
      this.spline.Closed = false;
      this.spline.Add(cachedCurve.trajectory);
    } else {
      this.startPos = this.preFireEndPos;
      this.currentPos = this.startPos;
      float num1 = Vector3.Distance(this.startPos, this.endPos);
      if ((double)this.projectileSpeed > 0.0) {
        this.duration = num1 / this.projectileSpeed;
      } else {
        this.duration = 1f;
      }
      if ((double)this.duration > 4.0) {this.duration = 4f;}
      this.rate = 1f / this.duration;
      if (this.isIndirect) {
        this.GenerateIndirectMissilePath();
      } else {
        this.GenerateMissilePath();
      }
    }
    base.PlayProjectile();
    this.startPos = this.preFireEndPos;
    if (this.isSRM) {
      //int num2 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_start, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
      //if (this.hitIndex >= this.hitInfo.numberOfShots) {
      //  int num3 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_missile_launch_last, this.parentAudioObject, (AkCallbackManager.EventCallback)null, null);
      //} else {
      //  int num4 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_missile_launch, this.parentAudioObject, (AkCallbackManager.EventCallback)null, null);
      //}
    } else {
      //int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_start, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
      //if (this.hitIndex >= this.hitInfo.numberOfShots) {
      //  int num3 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_missile_launch_last, this.parentAudioObject, (AkCallbackManager.EventCallback)null, null);
      //} else {
      //  int num4 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_missile_launch, this.parentAudioObject, (AkCallbackManager.EventCallback)null, null);
      //}
    }
  }
  public override void InitProjectile() {
    base.InitProjectile();
    CustomVector scale = this.weapon.ProjectileScale();
    if (scale.set) {
      Log.LogWrite("ImprovedWeaponEffect.InitProjectile set scale " + scale.ToString() + "\n");
      this.projectileTransform.localScale = scale.vector;
    }
  }
  protected override void PlayImpact() {
    this.PlayImpactAudio();
    this.StopAudio();
    //if (this.isSRM) {
    //  int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
    //} else {
    //  int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
    //}
    if ((this.hitInfo.DidShotHitAnything(this.hitIndex)||(this.intercepted)) && !string.IsNullOrEmpty(this.impactVFXBase)) {
      string str1 = string.Empty;
      bool flag = false;
      string str2 = string.Empty;
      AbstractActor actorByGuid = this.Combat.FindActorByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
      if (this.hitInfo.hitLocations[this.hitIndex] == 65536) {
        flag = true;
        MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.endPos);
        if (cellAt != null) {
          str2 = cellAt.GetVFXNameModifier();
          switch (cellAt.GetAudioSurfaceType()) {
            case AudioSwitch_surface_type.dirt: str1 = "_dirt"; break;
            case AudioSwitch_surface_type.metal: str1 = "_metal"; break;
            case AudioSwitch_surface_type.snow: str1 = "_snow"; break;
            case AudioSwitch_surface_type.wood: str1 = "_wood"; break;
            case AudioSwitch_surface_type.brush: str1 = "_brush"; break;
            case AudioSwitch_surface_type.concrete: str1 = "_concrete"; break;
            case AudioSwitch_surface_type.debris_glass: str1 = "_debris_glass"; break;
            case AudioSwitch_surface_type.gravel: str1 = "_gravel"; break;
            case AudioSwitch_surface_type.ice: str1 = "_ice"; break;
            case AudioSwitch_surface_type.lava: str1 = "_lava"; break;
            case AudioSwitch_surface_type.mud: str1 = "_mud"; break;
            case AudioSwitch_surface_type.sand: str1 = "_sand"; break;
            case AudioSwitch_surface_type.water_deep:
            case AudioSwitch_surface_type.water_shallow: str1 = "_water"; break;
            default: str1 = "_dirt"; break;
          }
        }
      } else if (actorByGuid != null && (this.intercepted == false) && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536 && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)actorByGuid.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex))) {
        str1 = "_crit";
      } else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0) {
        str1 = "_" + this.impactVFXVariations[Random.Range(0, this.impactVFXVariations.Length)];
      }
      this.SpawnImpactExplosion(string.Format("{0}{1}{2}", this.impactVFXBase, str1, str2));
      if (flag) {
        float num3 = Random.Range(20f, 25f) * (!this.isSRM ? 1f : 0.75f);
        FootstepManager.Instance.AddScorch(this.endPos, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(num3, num3, num3), false);
      }
    }
    this.PlayImpactDamageOverlay();
    if (this.projectileMeshObject != null)
      this.projectileMeshObject.SetActive(false);
    if (this.projectileLightObject != null)
      this.projectileLightObject.SetActive(false);
    this.OnImpact(0.0f);
  }

  private void SpawnImpactExplosion(string explosionName) {
    Log.LogWrite("MultiShotMissileEffect.SpawnImpactExplosion "+explosionName+"\n");
    GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(explosionName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
    if (gameObject == null) {
      Log.LogWrite(" no such explosion\n");
      WeaponEffect.logger.LogError(("Missile impact had an invalid explosion prefab : " + explosionName));
    } else {
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      BTLight componentInChildren1 = gameObject.GetComponentInChildren<BTLight>(true);
      BTWindZone componentInChildren2 = gameObject.GetComponentInChildren<BTWindZone>(true);
      component.Stop(true);
      component.Clear(true);
      component.transform.position = this.endPos;
      component.transform.LookAt(this.preFireEndPos);
      BTCustomRenderer.SetVFXMultiplier(component);
      component.Play(true);
      if (componentInChildren1 != null) {
        componentInChildren1.contributeVolumetrics = true;
        componentInChildren1.volumetricsMultiplier = 1000f;
        componentInChildren1.intensity = this.impactLightIntensity;
        componentInChildren1.FadeIntensity(0.0f, 0.5f);
        componentInChildren1.RefreshLightSettings(true);
      }
      if (componentInChildren2 != null)
        componentInChildren2.PlayAnimCurve();
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if (autoPoolObject == null)
        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, explosionName, component);
      gameObject.transform.rotation = Random.rotationUniform;
      if (this.isSRM) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_large, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_medium, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
      }
      if (this.intercepted == false) {
        Log.LogWrite(" missile not intercepted. DestroyFlimsyObjects\n");
        this.DestroyFlimsyObjects();
      } else {
        Log.LogWrite(" missile intercepted. No DestroyFlimsyObjects\n");
      }
    }
  }
  protected void BaseUpdate() {
    if (this.currentState == WeaponEffect.WeaponEffectState.PreFiring) {
      if ((double)this.t <= 1.0)
        this.t += this.preFireRate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
      if ((double)this.t >= 1.0)
        this.OnPreFireComplete();
    }
    if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t <= 1.0)
      this.t += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
      if (!this.Active || this.subEffect || (this.weapon.WeaponCategoryValue.IsMelee || (double)this.attackSequenceNextDelayTimer <= 0.0))
      return;
    this.attackSequenceNextDelayTimer -= this.Combat.StackManager.GetProgressiveAttackDeltaTime(0.01f);
    if ((double)this.attackSequenceNextDelayTimer > 0.0)
      return;
    this.PublishNextWeaponMessage();
  }
  protected override void Update() {
    try {
      this.BaseUpdate();
      if (this.currentState == WeaponEffect.WeaponEffectState.PreFiring && (double)this.t < 1.0) {
        this.currentPos = Vector3.Lerp(this.startPos, this.preFireEndPos, this.t);
        this.projectileTransform.position = this.currentPos;
        if (this.currentPos != this.preFireEndPos)
          this.projectileTransform.LookAt(this.preFireEndPos);
      }
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0 && this.spline.Count > 0) {
        this.currentPos = this.spline.InterpolateByDistance(this.spline.Length * this.t);
        this.projectileTransform.position = this.currentPos;
        this.projectileTransform.rotation = this.spline.GetOrientationFast(this.t, false);
        if (this.hitInfo.dodgeRolls[hitIndex] <= -2.0f) {
          float AMSShootT = (0.0f - this.hitInfo.dodgeRolls[hitIndex]) - 2.0f;
          if (this.t >= AMSShootT) {
            Log.LogWrite(" Update missile " + this.hitInfo.attackWeaponIndex + ":" + hitIndex + " t:" + t + " " + AMSShootT + "\n");
            AdvWeaponHitInfoRec cachedCurve = hitInfo.advRec(hitIndex);
            //CachedMissileCurve cachedCurve = CustomAmmoCategories.getCachedMissileCurve(this.hitInfo, hitIndex);
            this.hitInfo.dodgeRolls[hitIndex] = 0f;
            if (cachedCurve != null) {
              this.intercepted = cachedCurve.interceptInfo.Intercepted;
              AMSShoot amsShoot = cachedCurve.interceptInfo.getAMSShoot();
              if (amsShoot == null) {
                this.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
              } else
              if (amsShoot.AMS == null) {
                this.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
              } else {
                Log.LogWrite(" firing AMS " + amsShoot.AMS.UIName + " sootIdx:" + amsShoot.shootIdx + "\n");
                amsShoot.AMS.AMS().Fire(amsShoot.shootIdx);
                cachedCurve.interceptInfo.nextAMSShoot();
                amsShoot = cachedCurve.interceptInfo.getAMSShoot();
                if (amsShoot == null) {
                  this.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
                } else {
                  this.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.getAMSShootT();
                }
              }
            }
          }
        }
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }catch(Exception e) {
      Log.M.TWL(0, e.ToString(), true);
    }
  }

  private void ApplyDoppler(GameObject audioListener) {
  }

  private void GenerateMissilePath() {
    float max = this.parentLauncher.missileCurveStrength;
    int index1 = Random.Range(2, this.parentLauncher.missileCurveFrequency);
    if ((double)max < 0.1 || this.parentLauncher.missileCurveFrequency < 1) {
      max = 0.0f;
      index1 = 2;
    }
    if (this.isSRM && !this.hitInfo.DidShotHitChosenTarget(this.hitIndex)) {
      max += Random.Range(5f, 12f);
      index1 += Random.Range(3, 9);
    }
    float num1 = 1f / (float)index1;
    Vector3 startPos = this.startPos;
    Vector3 up = Vector3.up;
    Vector3 axis = this.endPos - this.startPos;
    Vector3[] vector3Array = new Vector3[index1 + 1];
    vector3Array[0] = this.startPos;
    float num2 = !this.isSRM ? 25f : 0.0f;
    for (int index2 = 1; index2 < index1; ++index2) {
      Vector3 vector3_1 = Vector3.Lerp(this.startPos, this.endPos, num1 * (float)index2);
      Vector3 vector3_2 = Vector3.up * Random.Range(-max, max);
      vector3_2 = Quaternion.AngleAxis((float)Random.Range(0, 360), axis) * vector3_2;
      if ((double)vector3_2.y < 0.0)
        vector3_2.y = 0.0f;
      Vector3 worldPos = vector3_1 + vector3_2;
      float lerpedHeightAt = this.Combat.MapMetaData.GetLerpedHeightAt(worldPos, false);
      if ((double)worldPos.y < (double)lerpedHeightAt)
        worldPos.y = lerpedHeightAt + num2;
      vector3Array[index2] = worldPos;
    }
    vector3Array[index1] = this.endPos;
    this.spline.Interpolation = CurvyInterpolation.Bezier;
    this.spline.Clear();
    this.spline.Closed = false;
    this.spline.Add(vector3Array);
  }

  private void GenerateIndirectMissilePath() {
    float max = this.parentLauncher.missileCurveStrength;
    int num1 = Random.Range(2, this.parentLauncher.missileCurveFrequency);
    if ((double)max < 0.1 || this.parentLauncher.missileCurveFrequency < 1) {
      max = 0.0f;
      num1 = 2;
    }
    Vector3 up = Vector3.up;
    this.spline.Interpolation = CurvyInterpolation.Bezier;
    this.spline.Clear();
    this.spline.Closed = false;
    Vector3 axis = this.endPos - this.startPos;
    int length = 9;
    if (num1 > length)
      length = num1;
    float num2 = (float)((double)this.endPos.y - (double)this.startPos.y + 15.0);
    Vector3[] vector3Array = new Vector3[length];
    Vector3 vector3_1 = this.endPos - this.startPos;
    float num3 = (float)(((double)Mathf.Max(this.endPos.y, this.startPos.y) - (double)Mathf.Min(this.endPos.y, this.startPos.y)) * 0.5) + num2;
    vector3Array[0] = this.startPos;
    for (int index = 1; index < length - 1; ++index) {
      float num4 = (float)index / (float)length;
      float num5 = (float)(1.0 - (double)Mathf.Abs(num4 - 0.5f) / 0.5);
      float num6 = (float)(1.0 - (1.0 - (double)num5) * (1.0 - (double)num5));
      Vector3 worldPos = vector3_1 * num4;
      float lerpedHeightAt = this.Combat.MapMetaData.GetLerpedHeightAt(worldPos, false);
      if ((double)num3 < (double)lerpedHeightAt)
        num3 = lerpedHeightAt + 5f;
      worldPos.y += num6 * num3;
      worldPos += this.startPos;
      Vector3 vector3_2 = Vector3.up * Random.Range(-max, max);
      vector3_2 = Quaternion.AngleAxis((float)Random.Range(0, 360), axis) * vector3_2;
      if ((double)vector3_2.y < 0.0)
        vector3_2.y = 0.0f;
      worldPos += vector3_2;
      if ((double)worldPos.y < (double)lerpedHeightAt)
        worldPos.y = lerpedHeightAt + 5f;
      vector3Array[index] = worldPos;
    }
    vector3Array[length - 1] = this.endPos;
    this.spline.Add(vector3Array);
  }

  protected override void OnPreFireComplete() {
    base.OnPreFireComplete();
    this.PlayProjectile();
  }
  protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
    base.OnImpact(
      this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
      ,this.weapon.StructureDamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask)
    );
    if (this.projectileParticles != null)
      this.projectileParticles.Stop(true);
    if (!(this.doppler != null))
      return;
    this.doppler.enabled = false;
  }

  protected override void OnComplete() {
    base.OnComplete();
  }

  /*public void OnDisable() {
    if (!(this.projectileAudioObject != null))
      return;
    AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
    int num = (int)AkSoundEngine.UnregisterGameObj(this.projectileAudioObject.gameObject);
  }*/

  public override void Reset() {
    //if (this.Active) {
    //  if (this.isSRM) {
    //    int num1 = (int)WwiseManager.PostEvent<AudioEventList_srm>(AudioEventList_srm.srm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
    //  } else {
    //    int num2 = (int)WwiseManager.PostEvent<AudioEventList_lrm>(AudioEventList_lrm.lrm_projectile_stop, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, null);
    //  }
    //}
    base.Reset();
  }
}
