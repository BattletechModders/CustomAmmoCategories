﻿using BattleTech;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public abstract class BaseHardPointAnimationController : MonoBehaviour {
    public abstract void PrefireAnimation();
    public abstract void PostfireAnimation();
    public abstract void PrefireAnimationSpeed(float speed);
    public abstract void FireAnimationSpeed(float speed);
    public abstract void FireAnimation(int index);
    public abstract bool isPrefireAnimCompleete();
    public abstract bool isFireAnimCompleete(int index);
  }
  public static class HardpointAnimatorHelper {
    private static Dictionary<Weapon, BaseHardPointAnimationController> HardpointsAnimators = new Dictionary<Weapon, BaseHardPointAnimationController>();
    public static void RegisterHardpointAnimator(this Weapon weapon, BaseHardPointAnimationController anim) {
      Log.M.TWL(0, "HardpointAnimatorHelper.RegisterHardpointAnimator:" + weapon.defId + " anim:" + ((anim == null) ? "null" : "not null"));
      if (HardpointsAnimators.ContainsKey(weapon) == false) { HardpointsAnimators.Add(weapon, anim); } else { HardpointsAnimators[weapon] = anim; };
    }
    public static BaseHardPointAnimationController HardpointAnimator(this Weapon weapon) {
      if (HardpointsAnimators.ContainsKey(weapon) == false) { return null; }
      return HardpointsAnimators[weapon];
    }
    public static void Clear() {
      HardpointsAnimators.Clear();
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
        Log.LogWrite("baseHardpointAnimator: " + ((baseHardpointAnimator == null) ? "null" : "not null") + "\n");
      }
      if (this.baseHardpointAnimator != null) { this.baseHardpointAnimator.PrefireAnimation(); }
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
      if (scale.set && (go != null)) {
        Log.LogWrite("ImprovedWeaponEffect.ScaleWeaponEffect " + go.name + " -> " + scale + "\n");
        ParticleSystem[] psyss = go.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem psys in psyss) {
          psys.RegisterRestoreScale();
          var main = psys.main;
          main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          Log.LogWrite(" " + psys.name + ":" + psys.main.scalingMode + "\n");
        }
        ParticleSystemRenderer[] renderers = go.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer renderer in renderers) {
          Log.LogWrite(" " + renderer.name + ": materials\n");
          foreach (Material material in renderer.materials) {
            Log.LogWrite("  " + material.name + ": " + material.shader + "\n");
          }
        }
        go.RegisterRestoreScale();
        go.transform.localScale = scale.vector;
        Component[] components = go.GetComponentsInChildren<Component>();
        foreach (Component component in components) {
          Log.LogWrite(" " + component.name + ":" + component.GetType().ToString() + "\n");
        }
        TrailRenderer[] trails = go.GetComponentsInChildren<TrailRenderer>();
        foreach (TrailRenderer trail in trails) {
          trail.RegisterRestoreScale();
          trail.widthMultiplier = scale.x;
          trail.time *= scale.y;
          Log.LogWrite(" " + trail.name + ": materials\n");
          foreach (Material material in trail.materials) {
            Log.LogWrite("  " + material.name + ": " + material.shader + "\n");
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
      Log.LogWrite("RestoreScaleColor " + go.name + "\n");
      try {
        int id = go.GetInstanceID();
        if (restoreScale.ContainsKey(id)) {
          go.transform.localScale = restoreScale[id];
          Log.LogWrite(" " + go.name + " restoring scale " + go.transform.localScale + "\n");
          restoreScale.Remove(id);
        }
        ParticleSystem[] pss = go.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          id = ps.GetInstanceID();
          if (restoreParticleScale.ContainsKey(id)) {
            var main = ps.main;
            main.scalingMode = restoreParticleScale[id];
            Log.LogWrite(" " + ps.name + " restoring scale mode " + main.scalingMode + "\n");
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
            Log.LogWrite(" " + trail.name + " restoring scale " + scale + "\n");
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
              Log.LogWrite(" " + material.name + " restoring color " + color + "\n");
              restoreMaterialColor.Remove(id);
            }
          }
        }
      } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
    }
  }
  public abstract class CopyAbleWeaponEffect : WeaponEffect {
    private static FieldInfo fi_hasSentNextWeaponMessage = null;
    protected bool NeedColorCalc;
    public Color CurrentColor;
    public Color NextColor;
    public float colorT;
    public float ColorChangeSpeed;
    public int ColorIndex;
    public ColorChangeRule colorChangeRule;
    public Color effectiveColor;
    public List<ColorTableJsonEntry> colorsTable;
    public Color getNextColor() {
      Color result = Color.white;
      switch (colorChangeRule) {
        case ColorChangeRule.Linear: result = colorsTable[ColorIndex % colorsTable.Count].Color; ColorIndex = (ColorIndex + 1) % colorsTable.Count; break;
        case ColorChangeRule.Random: result = colorsTable[Random.Range(0, colorsTable.Count)].Color; break;
        case ColorChangeRule.RandomOnce: result = colorsTable[Random.Range(0, colorsTable.Count)].Color; break;
      }
      return result;
    }
    public void Init(WeaponEffect original) {
      this.impactVFXBase = original.impactVFXBase;
      this.preFireSFX = original.preFireSFX;
      this.Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.hitInfo = original.hitInfo;
      this.hitIndex = original.HitIndex();
      this.emitterIndex = (int)typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.subEffect = original.subEffect;
      this.currentState = original.currentState;
      this.weaponRep = original.weaponRep;
      this.weapon = original.weapon;
      this.parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.currentPos = (Vector3)typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
      this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
      this.attackSequenceNextDelayTimer = (float)typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
      if (fi_hasSentNextWeaponMessage != null) {
        this.hasSentNextWeaponMessage = (bool)fi_hasSentNextWeaponMessage.GetValue(original);
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
      }
      this.preFireDuration = original.preFireDuration;
      this.preFireRate = (float)typeof(WeaponEffect).GetField("preFireRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.duration = (float)typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.rate = (float)typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileSpeed = original.projectileSpeed;
      this.weaponImpactType = original.weaponImpactType;
      this.preFireVFXPrefab = original.preFireVFXPrefab;
      this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
      this.projectilePrefab = original.projectilePrefab;
      this.projectile = original.projectile;
      this.activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
      this.projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
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
        //Log.LogWrite("MultiShotBeamEffect.Update effectiveColor:" + effectiveColor + "\n");
        this.SetColor(effectiveColor);
        this.colorT += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.colorT) * this.ColorChangeSpeed;
      }
    }
    protected override void PlayProjectile() {
      this.ColorChangeSpeed = this.weapon.ColorSpeedChange();
      this.colorsTable = this.weapon.ColorsTable();
      this.colorChangeRule = this.weapon.colorChangeRule();
      this.colorT = 0f;
      this.ColorIndex = 0;
      this.StoreOriginalColor();
      //this.originalColor = this.beamRenderer.material.GetColor("_ColorBB");
      Log.LogWrite(" ColorChangeSpeed " + this.ColorChangeSpeed + "\n");
      Log.LogWrite(" colorsTable.Count " + this.colorsTable.Count + "\n");
      Log.LogWrite(" colorChangeRule " + this.colorChangeRule + "\n");
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
      Log.LogWrite(" NeedColorCalc " + this.NeedColorCalc + "\n");
      base.PlayProjectile();
    }
    public override void InitProjectile() {
      base.InitProjectile();
      this.ScaleWeaponEffect(this.projectile);
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      base.Fire(hitInfo, hitIndex, emitterIndex);
    }
    protected override void Update() {
      base.Update();
    }
    protected override void PlayTerrainImpactVFX() {
      MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
      if (cellAt == null)
        return;
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
      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
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
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
      }
    }
    protected override void PlayImpact() {
      if (this.hitInfo.DidShotHitAnything(this.hitIndex) && !string.IsNullOrEmpty(this.impactVFXBase)) {
        string str1 = string.Empty;
        AbstractActor actorByGuid = this.Combat.FindActorByGUID(this.hitInfo.ShotTargetId(this.hitIndex));
        if (actorByGuid != null && this.hitInfo.ShotHitLocation(this.hitIndex) != 65536 && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)actorByGuid.ArmorForLocation(this.hitInfo.ShotHitLocation(this.hitIndex)))
          str1 = "_crit";
        else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0)
          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
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
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(false);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(false);
      this.OnImpact(0.0f);
    }

    //public virtual void RestoreScale() {
    //  if (scaled == false) { return; }
    //  if (this.projectileTransform != null) { this.projectileTransform.localScale = this.originalProjectileScale; };
    //  if (this.projectileLightObject != null) { this.projectileLightObject.transform.localScale = this.originalLightScale; }
    //  foreach(var ps in originalParticleScaling) {
    //    var main = ps.Key.main;
    //    main.scalingMode = ps.Value;
    //  }
    //  originalParticleScaling.Clear();
    //  scaled = false;
    //}
    protected override void OnComplete() {
      //this.RestoreScale();
      base.OnComplete();
    }
    public override void Reset() {
      //this.RestoreScale();
      base.Reset();
    }
    protected bool hasSentNextWeaponMessage {
      get {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          return (bool)fi_hasSentNextWeaponMessage.GetValue(this);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
          return false;
        }
      }
      set {
        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
        if (fi_hasSentNextWeaponMessage != null) {
          fi_hasSentNextWeaponMessage.SetValue(this, value);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't set WeaponEffect.hasSentNextWeaponMessage\n");
        }
      }
    }
  }
}