using BattleTech;
using BattleTech.Rendering;
using CustomAmmoCategoriesHelper;
using CustomAmmoCategoriesLog;
using CustomVoices;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Log = CustomAmmoCategoriesLog.Log;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public abstract class BaseHardPointAnimationController : MonoBehaviour {
    public abstract void PrefireAnimation(Vector3 target,bool indirect);
    public abstract void PostfireAnimation();
    public abstract void PrefireAnimationSpeed(float speed);
    public abstract void FireAnimationSpeed(float speed);
    public abstract void FireAnimation(int index);
    public abstract bool isPrefireAnimCompleete();
    public abstract bool isFireAnimCompleete(int index);
  }
  public static class HardpointAnimatorHelper {
    public static BaseHardPointAnimationController HardpointAnimator(this Weapon weapon) {
      if (weapon.weaponRep == null) { return null; }
      BaseHardPointAnimationController result = weapon.weaponRep.gameObject.GetComponent<BaseHardPointAnimationController>();
      return result;
    }
  }
  public class MuiltShotAnimatedEffect : CopyAbleWeaponEffect {
    private BaseHardPointAnimationController baseHardpointAnimator;
    protected int shotIndex;
    private int animationIndex;
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      shotIndex = 0;
      animationIndex = 0;
      if (weapon != null) {
        baseHardpointAnimator = weapon.HardpointAnimator();
        if (baseHardpointAnimator != null) {
          baseHardpointAnimator.PrefireAnimationSpeed(weapon.PrefireAnimationSpeedMod());
          baseHardpointAnimator.FireAnimationSpeed(weapon.FireAnimationSpeedMod());
        }
        Log.Combat?.WL(0,$"baseHardpointAnimator: {((baseHardpointAnimator == null) ? "null" : "not null")}");
      }
      if (this.baseHardpointAnimator != null) { this.baseHardpointAnimator.PrefireAnimation(hitInfo.hitPositions[hitIndex], false); }
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    protected virtual void FireNextShot() {
      ++this.shotIndex;
    }
    protected virtual void PlayProjectile(bool callbase) {
      if (baseHardpointAnimator != null) {
        baseHardpointAnimator.FireAnimation(this.shotIndex);
        this.animationIndex = this.shotIndex;
      }
      if (callbase) { base.PlayProjectile(); }
    }
    protected override void Update() {
      if (this.currentState == WeaponEffectState.PreFiring) {
        if (baseHardpointAnimator != null) {
          if (baseHardpointAnimator.isPrefireAnimCompleete() == false) { return; }

        }
      }
      if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t >= 1.0) {
        if (baseHardpointAnimator != null) {
          if (this.shotIndex != this.animationIndex) {
            baseHardpointAnimator.FireAnimation(this.shotIndex);
            this.animationIndex = this.shotIndex;
            return;
          } else {
            if (baseHardpointAnimator.isFireAnimCompleete(this.animationIndex) == false) { return; }
          }
        }
        this.FireNextShot();
      }
      base.Update();
    }
    protected override void OnComplete() {
      base.OnComplete();
      if (this.baseHardpointAnimator != null) { this.baseHardpointAnimator.PostfireAnimation(); }
    }
  }
  public static class DataManagerRestoreScaleHelper {
    public static Dictionary<int, Vector3> restoreScale = new Dictionary<int, Vector3>();
    public static Dictionary<int, ParticleSystemScalingMode> restoreParticleScale = new Dictionary<int, ParticleSystemScalingMode>();
    public static Dictionary<int, Vector3> restoreTrailScale = new Dictionary<int, Vector3>();
    public static Dictionary<int, Color> restoreMaterialColor = new Dictionary<int, Color>();
    public static void ScaleWeaponEffect(this WeaponEffect effect, GameObject go) {
      CustomVector scale = effect.weapon.ProjectileScale();
      go.ScaleEffect(scale);
    }
    public static void ScaleEffect(this GameObject go, CustomVector scale) {
      if (scale.set && (go != null)) {
        //Log.Combat?.WL(0,$"ImprovedWeaponEffect.ScaleWeaponEffect {go.name} -> {scale}");
        ParticleSystem[] psyss = go.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem psys in psyss) {
          psys.RegisterRestoreScale();
          var main = psys.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          //Log.Combat?.WL(1,$"{psys.name}:{psys.main.scalingMode}");
        }
        ParticleSystemRenderer[] renderers = go.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer renderer in renderers) {
          //Log.Combat?.WL(1, $"{renderer.name}: materials");
          foreach (Material material in renderer.materials) {
            //Log.Combat?.WL(2, $"{material.name}: {material.shader}");
          }
        }
        go.RegisterRestoreScale();
        go.transform.localScale = scale.vector;
        Component[] components = go.GetComponentsInChildren<Component>();
        foreach (Component component in components) {
          //Log.Combat?.WL(1, $"{component.name}:{component.GetType().ToString()}");
        }
        TrailRenderer[] trails = go.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails) {
          trail.RegisterRestoreScale();
          trail.widthMultiplier = scale.x;
          trail.time *= scale.y;
          //Log.Combat?.WL(1, $"{trail.name}: materials");
          foreach (Material material in trail.materials) {
            //Log.Combat?.WL(2, $"{material.name}: {material.shader}");
          }
        }
      }
    }
    public static Color ColorBB(this Material mat) {
      return mat.GetColor("_ColorBB");
    }
    public static void RegisterRestoreColor(this Material mat) {
      int id = mat.GetInstanceID();
      if (restoreMaterialColor.ContainsKey(id) == false) {
        restoreMaterialColor.Add(id, mat.GetColor("_ColorBB"));
      }
    }
    public static void RegisterRestoreScale(this GameObject go) {
      int id = go.GetInstanceID();
      if (restoreScale.ContainsKey(id) == false) { restoreScale.Add(id, go.transform.localScale); };
    }
    public static void RegisterRestoreScale(this ParticleSystem ps) {
      int id = ps.GetInstanceID();
      if (restoreScale.ContainsKey(id) == false) { restoreParticleScale.Add(id, ps.main.scalingMode); };
    }
    public static void RegisterRestoreScale(this TrailRenderer trail) {
      int id = trail.GetInstanceID();
      if (restoreTrailScale.ContainsKey(id) == false) {
        Vector3 scale = Vector3.zero;
        scale.x = trail.widthMultiplier;
        scale.z = trail.time;
        restoreTrailScale.Add(id, scale);
      };
    }
    public static void RestoreScaleColor(this GameObject go) {
      //Log.Combat?.WL(0, "RestoreScaleColor {go.name}");
      try {
        int id = go.GetInstanceID();
        if (restoreScale.ContainsKey(id)) {
          go.transform.localScale = restoreScale[id];
          //Log.Combat?.WL(1,$"{go.name} restoring scale {go.transform.localScale}");
          restoreScale.Remove(id);
        }
        ParticleSystem[] pss = go.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          id = ps.GetInstanceID();
          if (restoreParticleScale.ContainsKey(id)) {
            var main = ps.main;
            main.scalingMode = restoreParticleScale[id];
            //Log.Combat?.WL(1, $"{ps.name} restoring scale mode {main.scalingMode}");
            restoreParticleScale.Remove(id);
          }
        }
        TrailRenderer[] trails = go.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails) {
          id = trail.GetInstanceID();
          if (restoreTrailScale.ContainsKey(id)) {
            Vector3 scale = restoreTrailScale[id];
            trail.widthMultiplier = scale.x;
            trail.time = scale.z;
            //Log.Combat?.WL(1, $"{trail.name} restoring scale {scale}");
            restoreTrailScale.Remove(id);
          }
        }
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
          foreach (Material material in renderer.materials) {
            id = material.GetInstanceID();
            if (restoreMaterialColor.ContainsKey(id)) {
              Color color = restoreMaterialColor[id];
              material.SetColor("_ColorBB", restoreMaterialColor[id]);
              //Log.Combat?.WL(1, $"{material.name} restoring color {color}");
              restoreMaterialColor.Remove(id);
            }
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString(), true);
        CustomAmmoCategories.AttackSequence_logger.LogException(e);
      }
    }
  }
  public abstract class CopyAbleWeaponEffect : WeaponEffect {
    public enum PlaySFXType { None, First, Middle, Last };
    protected bool NeedColorCalc;
    public Color CurrentColor;
    public Color NextColor;
    public float colorT;
    public float ColorChangeSpeed;
    public int ColorIndex;
    public ColorChangeRule colorChangeRule;
    public Color effectiveColor;
    public List<ColorTableJsonEntry> colorsTable;
    public AudioObject customParentAudioObject = null;
    public AudioObject customProjectileAudioObject = null;
    private bool audioStopped { get; set; } = true;
    public PlaySFXType playSFX { get; set; } = PlaySFXType.None;
    public string firstPreFireSFX { get; set; } = null;
    public string middlePrefireSFX { get; set; } = null;
    public string lastPreFireSFX { get; set; } = null;
    public string firstFireSFX { get; set; } = null;
    public string middlefireSFX { get; set; } = null;
    public string lastFireSFX { get; set; } = null;
    public string customPrefireSFX { get; set; } = null;
    public string fireSFX { get; set; } = null;
    public string projectilePrefireSFX { get; set; } = null;
    public string projectileFireSFX { get; set; } = null;
    public string projectileStopSFX { get; set; } = null;
    public string firingStartSFX { get; set; } = null;
    public string firingStopSFX { get; set; } = null;
    public float originalPrefireDuration { get; set; } = 0f;
    public float originalProjectileSpeed { get; set; } = 0f;
    public float shotDelay { get; set; } = 0f;
    public string preFireStartSFX { get; set; } = null;
    public string preFireStopSFX { get; set; } = null;
    public string customPulseSFX { get; set; } = null;
    public float customPulseSFXdelay { get; set; } = 0f;
    protected float pulseTime;
    public Color getNextColor() {
      Color result = Color.white;
      switch (colorChangeRule) {
        case ColorChangeRule.Linear: result = colorsTable[ColorIndex % colorsTable.Count].Color; ColorIndex = (ColorIndex + 1) % colorsTable.Count; break;
        case ColorChangeRule.Random: result = colorsTable[Random.Range(0, colorsTable.Count)].Color; break;
        case ColorChangeRule.RandomOnce: result = colorsTable[Random.Range(0, colorsTable.Count)].Color; break;
      }
      return result;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      this.audioStopped = false;
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    public void Init(WeaponEffect original) {
      Log.Combat?.TWL(0, $"CopyAbleWeaponEffect.Init {this.GetType().ToString()} original:{original.GetType().ToString()}");
      try {
        this.impactVFXBase = original.impactVFXBase;
        this.preFireSFX = original.preFireSFX;
        this.Combat = original.Combat();
        this.hitInfo = original.hitInfo;
        this.hitIndex = original.hitIndex;
        this.emitterIndex = original.emitterIndex();
        this.numberOfEmitters = original.numberOfEmitters();
        this.subEffect = original.subEffect;
        this.currentState = original.currentState;
        this.weaponRep = original.weaponRep;
        this.weapon = original.weapon;
        this.parentAudioObject = original.parentAudioObject();
        this.startingTransform = original.startingTransform();
        this.startPos = original.startPos();
        this.endPos = original.endPos();
        this.currentPos = original.currentPos();
        this.t = original.t();
        this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
        this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
        this.attackSequenceNextDelayTimer = original.attackSequenceNextDelayTimer();
        this.hasSentNextWeaponMessage(original.hasSentNextWeaponMessage());
        this.preFireDuration = original.preFireDuration;
        this.originalPrefireDuration = original.preFireDuration;
        this.preFireRate = original.preFireRate();
        this.duration = original.duration();
        this.rate = original.rate();
        this.projectileSpeed = original.projectileSpeed;
        this.originalProjectileSpeed = this.projectileSpeed;
        this.weaponImpactType = original.weaponImpactType;
        this.preFireVFXPrefab = original.preFireVFXPrefab;
        this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
        this.projectilePrefab = original.projectilePrefab;
        this.projectile = original.projectile;
        this.activeProjectileName = original.activeProjectileName();
        this.projectileTransform = original.projectileTransform();
        this.projectileParticles = original.projectileParticles();
        this.projectileAudioObject = original.projectileAudioObject();
        this.projectileMeshObject = original.projectileMeshObject();
        this.projectileLightObject = original.projectileLightObject();
        Log.Combat?.WL(1, $"projectile:{(projectile == null ? "null" : projectile.name)}");
        Log.Combat?.WL(1, $"projectilePrefab:{(projectilePrefab == null ? "null" : projectilePrefab.name)}");
        Log.Combat?.WL(1, $"activeProjectileName:{activeProjectileName}");
        if (projectilePrefab != null) {
          if (string.IsNullOrEmpty(this.activeProjectileName)) {
            this.activeProjectileName = projectilePrefab.name;
          }
        }
        this.impactVFXVariations = original.impactVFXVariations;
        this.armorDamageVFXName = original.armorDamageVFXName;
        this.structureDamageVFXName = original.structureDamageVFXName;
        this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
        this.FiringComplete = original.FiringComplete;
        this.AllowMissSkipping = original.AllowMissSkipping;
        this.NeedColorCalc = false;
        this.CurrentColor = Color.white;
        this.NextColor = Color.white;
        this.colorT = 0f;
        this.ColorChangeSpeed = 0f;
        this.ColorIndex = 0;
        this.colorChangeRule = ColorChangeRule.None;
        this.colorsTable = new List<ColorTableJsonEntry>();
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        WeaponEffect.logger.LogException(e);
      }
    }
    public override void Init(Weapon weapon) {
      base.Init(weapon);
      if(this.weapon.parent.GameRep != null) {
        this.customParentAudioObject = this.weapon.parent.GameRep.gameObject.GetComponent<AudioObject>();
        if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weapon.parent.GameRep.gameObject.AddComponent<AudioObject>(); }
        if (this.weapon.parent.GameRep.audioObject != null) {
          this.parentAudioObject = this.weapon.parent.GameRep.audioObject;
        }
      }
    }
    public virtual void StoreOriginalColor() {

    }
    public virtual void SetColor(Color color) {

    }
    public virtual void RestoreOriginalColor() {

    }
    public virtual void UpdateColor() {
      if (this.NeedColorCalc) {
        if (this.colorT > 1f) {
          this.CurrentColor = this.NextColor;
          this.colorT = 0f;
          this.NextColor = this.getNextColor();
        }
        Color effectiveColor = Color.Lerp(this.CurrentColor, this.NextColor, this.colorT);
        this.SetColor(effectiveColor);
        this.colorT += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.colorT) * this.ColorChangeSpeed;
      }
    }
    protected override void PlayProjectile() {
      Log.Combat?.TWL(0,$"{this.GetType().Name}.PlayProjectile customPulseSFX:{this.customPulseSFX} pulseDelay:{this.customPulseSFXdelay}");
      Log.Combat?.WL(1, $"playSFX:{this.playSFX} fireSFX:{this.fireSFX}");
      try {
        this.ColorChangeSpeed = this.weapon.ColorSpeedChange();
        this.colorsTable = this.weapon.ColorsTable();
        this.colorChangeRule = this.weapon.colorChangeRule();
        this.colorT = 0f;
        this.ColorIndex = 0;
        this.StoreOriginalColor();
        Log.Combat?.WL(1, $"ColorChangeSpeed {this.ColorChangeSpeed}");
        Log.Combat?.WL(1, $"colorsTable.Count {this.colorsTable.Count}");
        Log.Combat?.WL(1, $"colorChangeRule {this.colorChangeRule}");
        this.NeedColorCalc = (this.ColorChangeSpeed > CustomAmmoCategories.Epsilon);
        if (this.colorsTable.Count <= 1) { this.NeedColorCalc = false; };
        if ((this.colorChangeRule != ColorChangeRule.None) && (this.colorsTable.Count > 0)) {
          if (this.colorsTable.Count == 1) {
            this.CurrentColor = this.colorsTable[0].Color;
            this.SetColor(this.CurrentColor);
            this.NeedColorCalc = false;
          } else if (this.colorChangeRule == ColorChangeRule.RandomOnce) {
            this.NeedColorCalc = false;
            this.CurrentColor = this.getNextColor();
            this.SetColor(this.CurrentColor);
          } else if (this.colorChangeRule >= ColorChangeRule.t0) {
            this.NeedColorCalc = false;
            this.ColorIndex = ((int)this.colorChangeRule - (int)ColorChangeRule.t0) % this.colorsTable.Count;
            this.CurrentColor = this.colorsTable[this.ColorIndex].Color;
            this.SetColor(this.CurrentColor);
          } else {
            this.CurrentColor = this.getNextColor();
            this.NextColor = this.getNextColor();
            this.SetColor(this.CurrentColor);
          }
        } else {
          this.NeedColorCalc = false;
        }
        Log.Combat?.WL(1, $"NeedColorCalc {this.NeedColorCalc}");
        if (string.IsNullOrEmpty(this.customPulseSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(customPulseSFX)) {
            if (this.customParentAudioObject == null) {
              this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<AudioObject>();
              if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<AudioObject>(); }
            }
            this.customParentAudioObject.Play(customPulseSFX, false);
          } else {
            int num = (int)WwiseManager.PostEvent(customPulseSFX, this.parentAudioObject);
          }
        }
        if (string.IsNullOrEmpty(this.fireSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(this.fireSFX)) {
            if (this.customParentAudioObject == null) {
              this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<AudioObject>();
              if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<AudioObject>(); }
            }
            this.customParentAudioObject.Play(this.fireSFX, false);
          } else {
            int num = (int)WwiseManager.PostEvent(this.fireSFX, this.parentAudioObject);
          }
        }
        if (string.IsNullOrEmpty(this.projectileFireSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(projectileFireSFX)) {
            this.customProjectileAudioObject?.Play(projectileFireSFX, true);
          } else {
            int num = (int)WwiseManager.PostEvent(projectileFireSFX, this.projectileAudioObject);
          }
        }
        base.PlayProjectile();
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        WeaponEffect.logger.LogException(e);
      }
    }
    public override void InitProjectile() {
      //if(this.projectile != null)
      if (this.projectilePrefab != null && this.projectile != null) {
        if (this.activeProjectileName == null) { this.activeProjectileName = this.projectilePrefab.name; }
      }
      base.InitProjectile();
      this.customProjectileAudioObject = this.projectile.GetComponent<AudioObject>();
      if (this.customProjectileAudioObject == null) { this.projectile.AddComponent<AudioObject>(); }
      if (this.customParentAudioObject == null) { this.customParentAudioObject = this.customProjectileAudioObject; }

      this.ScaleWeaponEffect(this.projectile);
    }
    public virtual void SetupCustomSettings() {
      if (weapon.prefireDuration() > CustomAmmoCategories.Epsilon) {
        this.preFireDuration = weapon.prefireDuration();
      }
      if (weapon.preFireSFX() != null) {
        this.customPrefireSFX = weapon.preFireSFX();
      } else {
        this.customPrefireSFX = this.preFireSFX;
      }
      this.firstPreFireSFX = weapon.firstPreFireSFX();
      this.middlePrefireSFX = weapon.preFireSFX();
      this.lastPreFireSFX = weapon.lastPreFireSFX();
    }
    protected override void Update() {
      try {
        base.Update();
        if (this.currentState == WeaponEffectState.Firing) {
          if ((this.customPulseSFXdelay > CustomAmmoCategories.Epsilon) && (this.t >= this.pulseTime)) {
            this.pulseTime += this.customPulseSFXdelay;
            if (string.IsNullOrEmpty(this.customPulseSFX) == false) {
              if (CustomVoices.AudioEngine.isInAudioManifest(customPulseSFX)) {
                if (this.customParentAudioObject == null) {
                  this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<AudioObject>();
                  if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<AudioObject>(); }
                }
                this.customParentAudioObject.Play(customPulseSFX, false);
              } else {
                int num = (int)WwiseManager.PostEvent(customPulseSFX, this.parentAudioObject);
              }
            }
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        WeaponEffect.logger.LogException(e);
      }
    }
    protected override void PlayTerrainImpactVFX() {
      MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
      if (cellAt == null) { return; };
      try {
        string vfxNameModifier = cellAt.GetVFXNameModifier();
        string str1;
        switch (cellAt.GetAudioSurfaceType()) {
          case AudioSwitch_surface_type.dirt:
            str1 = "dirt" + vfxNameModifier;
            break;
          case AudioSwitch_surface_type.metal:
            str1 = "metal";
            break;
          case AudioSwitch_surface_type.snow:
            str1 = "snow";
            break;
          case AudioSwitch_surface_type.wood:
            str1 = "wood";
            break;
          case AudioSwitch_surface_type.brush:
            str1 = "brush";
            break;
          case AudioSwitch_surface_type.concrete:
            str1 = "concrete" + vfxNameModifier;
            break;
          case AudioSwitch_surface_type.debris_glass:
            str1 = "debris_glass" + vfxNameModifier;
            break;
          case AudioSwitch_surface_type.gravel:
            str1 = "gravel";
            break;
          case AudioSwitch_surface_type.ice:
            str1 = "ice";
            break;
          case AudioSwitch_surface_type.lava:
            str1 = "lava";
            break;
          case AudioSwitch_surface_type.mud:
            str1 = "mud";
            break;
          case AudioSwitch_surface_type.sand:
            str1 = "sand";
            break;
          case AudioSwitch_surface_type.water_deep:
          case AudioSwitch_surface_type.water_shallow:
            str1 = "water";
            break;
          default:
            str1 = "dirt";
            break;
        }
        string str2 = string.Format("{0}_{1}", (object)this.terrainHitVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (gameObject == null) {
          WeaponEffect.logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayTerrainImpactVFX had an invalid VFX name: " + str2));
        } else {
          this.ScaleWeaponEffect(gameObject);
          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
          component.Stop(true);
          component.Clear(true);
          component.transform.position = this.endPos;
          component.transform.LookAt(this.startingTransform.position);
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if (autoPoolObject == null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        WeaponEffect.logger.LogException(e);
      }
    }
    protected override void PlayImpact() {
      try {
        if (this.hitInfo.DidShotHitAnything(this.hitIndex) && !string.IsNullOrEmpty(this.impactVFXBase)) {
          string str1 = string.Empty;
          AbstractActor actorByGuid = this.Combat.FindActorByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
          if (actorByGuid != null && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536 && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)actorByGuid.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex)))
            str1 = "_crit";
          else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0)
            str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
          string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
          GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if (gameObject == null) {
            Log.LogWrite(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2 + "\n");
          } else {
            this.ScaleWeaponEffect(gameObject);
            ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
            component.Stop(true);
            component.Clear(true);
            component.transform.position = this.endPos;
            component.transform.LookAt(this.startingTransform.position);
            BTCustomRenderer.SetVFXMultiplier(component);
            component.Play(true);
            BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
            if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
              componentInChildren.StopAnimation();
              componentInChildren.PlayAnimation();
            }
            AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
            if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
              autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
            autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
          }
        }
        this.PlayImpactDamageOverlay();
        if (this.projectileMeshObject != null)
          this.projectileMeshObject.SetActive(false);
        if (this.projectileLightObject != null)
          this.projectileLightObject.SetActive(false);
        this.OnImpact(0.0f);
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        WeaponEffect.logger.LogException(e);
      }
    }
    protected override void PlayPreFire() {
      try {
        if (this.customPrefireSFX == null) { this.customPrefireSFX = this.preFireSFX; }
        Log.Combat?.TWL(0, $"{this.GetType().Name}.PlayPreFire {this.name} playSFX:{this.playSFX} prefireSFX:{this.customPrefireSFX} preFireStartSFX:{this.preFireStartSFX} preFireDuration:{this.preFireDuration}");
        if (this.preFireVFXPrefab != null) {
          GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab);
          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
          if (autoPoolObject == null)
            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.preFireVFXPrefab.name, component);
          component.Stop(true);
          component.Clear(true);
          component.transform.parent = (Transform)null;
          component.transform.position = this.startingTransform.position;
          component.transform.LookAt(this.endPos);
          BTCustomRenderer.SetVFXMultiplier(component);
          component.Play(true);
          if ((double)this.preFireDuration <= 0.0)
            this.preFireDuration = component.main.duration;
        }
        if (string.IsNullOrEmpty(this.customPrefireSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(this.customPrefireSFX)) {
            if (this.customParentAudioObject == null) {
              this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<AudioObject>();
              if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<AudioObject>(); }
            }
            this.customParentAudioObject.Play(this.customPrefireSFX, false);
          } else {
            int num = (int)WwiseManager.PostEvent(this.customPrefireSFX, this.parentAudioObject);
          }
        }
        if (string.IsNullOrEmpty(this.preFireStartSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(this.preFireStartSFX)) {
            if (this.customParentAudioObject == null) {
              this.customParentAudioObject = this.weaponRep.gameObject.GetComponent<AudioObject>();
              if (this.customParentAudioObject == null) { this.customParentAudioObject = this.weaponRep.gameObject.AddComponent<AudioObject>(); }
            }
            this.customParentAudioObject.Play(this.preFireStartSFX, true);
          } else {
            int num = (int)WwiseManager.PostEvent(this.preFireStartSFX, this.parentAudioObject);
          }
        }
        if (string.IsNullOrEmpty(this.projectilePrefireSFX) == false) {
          if (CustomVoices.AudioEngine.isInAudioManifest(projectilePrefireSFX)) {
            this.customProjectileAudioObject?.Play(projectilePrefireSFX, true);
          } else {
            int num = (int)WwiseManager.PostEvent(projectilePrefireSFX, this.projectileAudioObject);
          }
        }
        this.preFireRate = (double)this.preFireDuration <= 0.0 ? 1000f : 1f / this.preFireDuration;
        if ((double)this.attackSequenceNextDelayMin <= 0.0 && (double)this.attackSequenceNextDelayMax <= 0.0)
          this.attackSequenceNextDelayMax = this.preFireDuration;
        if ((double)this.attackSequenceNextDelayMax <= 0.0)
          this.attackSequenceNextDelayMax = 0.05f;
        if ((double)this.attackSequenceNextDelayMin >= (double)this.attackSequenceNextDelayMax)
          this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
        this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
        this.t = 0.0f;
        this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        WeaponEffect.logger.LogException(e);
      }
    }
    public virtual void StopAudio() {
      if (audioStopped) { return; }
      audioStopped = true;
      if ((string.IsNullOrEmpty(this.preFireStopSFX) == false)
        && (string.IsNullOrEmpty(this.preFireStartSFX) == false)
        && CustomVoices.AudioEngine.isInAudioManifest(this.preFireStartSFX) == false) {
        int num = (int)WwiseManager.PostEvent(this.preFireStopSFX, this.parentAudioObject);
      } else
      if (string.IsNullOrEmpty(this.preFireStartSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(this.preFireStartSFX)) {
          this.customParentAudioObject?.Stop(this.preFireStartSFX);
        }
      }
      if(string.IsNullOrEmpty(this.projectileFireSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(this.projectileFireSFX)) {
          this.customProjectileAudioObject?.Stop(this.projectileFireSFX);
        }
      }
      if (string.IsNullOrEmpty(this.projectilePrefireSFX) == false) {
        if (CustomVoices.AudioEngine.isInAudioManifest(this.projectilePrefireSFX)) {
          this.customProjectileAudioObject?.Stop(this.projectilePrefireSFX);
        }
      }
      if (string.IsNullOrEmpty(this.projectileStopSFX) == false) {
        int num = (int)WwiseManager.PostEvent(this.projectileStopSFX, this.projectileAudioObject);
      }
    }
    protected override void PlayImpactAudio() {
      base.PlayImpactAudio();
    }
    protected override void OnComplete() {
      this.StopAudio();
      base.OnComplete();
    }
    public override void Reset() {
      if (this.Active) { this.StopAudio(); }
      //this.RestoreScale();
      base.Reset();

    }
  }
}