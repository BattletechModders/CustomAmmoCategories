using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using FluffyUnderware.Curvy;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
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
    public static void RegisterRestoreColor(this Material mat) {
      int id = mat.GetInstanceID();
      if(restoreMaterialColor.ContainsKey(id) == false) {
        restoreMaterialColor.Add(id,mat.GetColor("_ColorBB"));
      }
    }
    public static void RegisterRestoreScale(this GameObject go) {
      int id = go.GetInstanceID();
      if (restoreScale.ContainsKey(id) == false) { restoreScale.Add(id,go.transform.localScale); };
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
      Log.LogWrite("RestoreScaleColor "+go.name+"\n");
      try {
        int id = go.GetInstanceID();
        if (restoreScale.ContainsKey(id)) {
          go.transform.localScale = restoreScale[id];
          Log.LogWrite(" "+go.name + " restoring scale "+go.transform.localScale+"\n");
          restoreScale.Remove(id);
        }
        ParticleSystem[] pss = go.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem ps in pss) {
          id = ps.GetInstanceID();
          if (restoreParticleScale.ContainsKey(id)) {
            var main = ps.main;
            main.scalingMode = restoreParticleScale[id];
            Log.LogWrite(" " + ps.name + " restoring scale mode "+ main.scalingMode + "\n");
            restoreParticleScale.Remove(id);
          }
        }
        TrailRenderer[] trails = go.GetComponentsInChildren<TrailRenderer>();
        foreach(TrailRenderer trail in trails) {
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
        foreach(Renderer renderer in renderers) {
          foreach(Material material in renderer.materials) {
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
  public abstract class CopyAbleWeaponEffect: WeaponEffect {
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
      this.hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
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
      if ((this.colorChangeRule != ColorChangeRule.None)&&(this.colorsTable.Count > 0)) {
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
          Log.LogWrite(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2+"\n");
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
  public class MultiShotBulletEffect: CopyAbleWeaponEffect {
    public int bulletIdx;
    public bool primeBullet;
    private MultiShotBallisticEffect parentLauncher;
    private CurvySpline spline;
    public static readonly float missileCurveStrength = 0.0f;
    public static readonly int missileCurveFrequency = 2;
    private void GenerateIndirectMissilePath() {
      float max = MultiShotBulletEffect.missileCurveStrength;
      int num1 = Random.Range(2, MultiShotBulletEffect.missileCurveFrequency);
      if ((double)max < 0.1 || MultiShotBulletEffect.missileCurveFrequency < 1) {
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
        float lerpedHeightAt = this.Combat.MapMetaData.GetLerpedHeightAt(worldPos);
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
      this.spline.Refresh();
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
    }
    public MultiShotBulletEffect() {
      this.spline = null;
    }
    public void Init(Weapon weapon, MultiShotBallisticEffect parentLauncher) {
      this.Init(weapon);
      this.parentLauncher = parentLauncher;
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.projectileSpeed = parentLauncher.projectileSpeed * weapon.ProjectileSpeedMultiplier();
      if(this.spline == null) {
        this.spline = this.gameObject.AddComponent<CurvySpline>();
      }
    }

    public virtual void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0, bool pb = false) {
      Log.LogWrite("MultiShotBulletEffect.Fire "+hitInfo.attackWeaponIndex+" "+hitIndex+" ep:"+hitInfo.hitPositions[hitIndex]+" prime:"+pb+"\n");
      this.primeBullet = pb;
      Vector3 endPos = hitInfo.hitPositions[hitIndex];
      base.Fire(hitInfo, hitIndex, emitterIndex);
      this.endPos = endPos;
      hitInfo.hitPositions[hitIndex] = endPos;
      Log.LogWrite(" endPos restored:" + this.endPos + "\n");
      AdvWeaponHitInfoRec advRec = this.hitInfo.advRec(hitIndex);
      //if (pb == false) {
        endPos.x += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
        endPos.y += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
        endPos.z += Random.Range(-this.parentLauncher.spreadAngle, this.parentLauncher.spreadAngle);
        this.endPos = endPos;
      //}
      float num = Vector3.Distance(this.startingTransform.position, this.endPos);
      if (this.parentLauncher.isIndirect) {
        this.GenerateIndirectMissilePath();
        num = this.spline.Length;
      } else {
        this.spline.Clear();
      }
      if ((double)this.projectileSpeed > 0.0)
        this.duration = num / this.projectileSpeed;
      else
        this.duration = 1f;
      if ((double)this.duration > 4.0)
        this.duration = 4f;
      this.rate = 1f / this.duration;
      this.PlayPreFire();
      if (advRec == null) {
        Log.LogWrite(" no advanced record.");
        return;
      }
      if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
        Log.LogWrite(" frag projectile separated.");
        this.RegisterFragWeaponEffect();
      }
      /*ShrapnelHitRecord shrapnelHitRecord = CustomAmmoCategories.getShrapnelCache(this.hitInfo, this.hitIndex);
      if (shrapnelHitRecord != null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" shrapnel Hit info found:" + shrapnelHitRecord.shellsHitIndex + "\n");
        if (shrapnelHitRecord.isSeparated == true) {
          this.RegisterFragWeaponEffect();
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" not separated\n");
        }
      }*/
    }
    //public Color originalColor;
    public TrailRenderer trailRendered;
    public override void StoreOriginalColor() {
      this.trailRendered = this.projectile.GetComponentInChildren<TrailRenderer>();
      if (trailRendered != null) {
        this.trailRendered.material.RegisterRestoreColor();
      };
    }
    public override void SetColor(Color color) {
      float coeff = color.maxColorComponent;
      Color tempColor = Color.white;
      tempColor.a = 1f;
      tempColor.r = (color.r / coeff) * 8.0f + 1f;
      tempColor.g = (color.g / coeff) * 8.0f + 1f;
      tempColor.b = (color.b / coeff) * 8.0f + 1f;
      if (trailRendered != null) {  this.trailRendered.material.SetColor("_ColorBB", tempColor); };
    }
    public override void RestoreOriginalColor() {
      //if (trailRendered != null) { this.trailRendered.material.SetColor("_ColorBB", this.originalColor); };
    }
    public override void InitProjectile() {
      base.InitProjectile();
      Log.LogWrite("MultiShotBulletEffect.InitProjectile\n");
      Component[] components = this.projectile.GetComponentsInChildren<Component>();
      foreach (Component component in components) {
        Log.LogWrite(" " + component.name + ":" + component.GetType().ToString() + "\n");
      }
    }

    protected override void PlayPreFire() {
      base.PlayPreFire();
    }
    protected override void PlayMuzzleFlash() {
      base.PlayMuzzleFlash();
    }
    protected override void PlayProjectile() {
      base.PlayProjectile();
      this.PlayMuzzleFlash();
    }
    protected override void PlayImpact() {
      this.PlayImpactAudio();
      base.PlayImpact();
    }
    protected override void Update() {
      base.Update();
      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
        return;
      if ((double)this.t < 1.0) {
        this.UpdateColor();
        if (this.spline.Count > 0) {
          this.currentPos = this.spline.InterpolateByDistance(this.spline.Length * this.t);
          this.projectileTransform.position = this.currentPos;
          this.projectileTransform.rotation = this.spline.GetOrientationFast(this.t, false);
        } else {
          this.currentPos = Vector3.Lerp(this.startPos, this.endPos, this.t);
          this.projectileTransform.position = this.currentPos;
        }
      }
      if ((double)this.t < 1.0)
        return;
      this.PlayImpact();
      this.OnComplete();
    }
    protected override void OnPreFireComplete() {
      base.OnPreFireComplete();
      this.PlayProjectile();
    }
    protected override void OnImpact(float hitDamage = 0.0f) {
      /*if (this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536) {
        AbstractActor combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId) as AbstractActor;
        if (combatantByGuid != null && ((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null)) {
          combatantByGuid.GameRep.PlayImpactAnim(this.hitInfo, this.hitIndex, this.weapon, MeleeAttackType.NotSet, 0.0f);
        }
      }*/ // это в принципе не нужно потом что PlayImpactAnim проигрывается на каждый импакт, в оригинальной реализации попадание пули просто не вызывало импакт
      Log.LogWrite("MultiShotBulletEffect.OnImpact wi:"+this.hitInfo.attackWeaponIndex+" hi:"+this.hitInfo+" bi:"+this.bulletIdx+" prime:"+this.primeBullet+"\n");
      if (this.primeBullet) {
        Log.LogWrite(" prime. Damage message fired\n");
        float damage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
        if (this.weapon.DamagePerPallet()&&(this.weapon.DamageNotDivided() == false)) { damage /= this.weapon.ProjectilesPerShot; };
        base.OnImpact(damage);
      } else {
        Log.LogWrite(" no prime. No damage message fired\n");
      }
      if (!((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)) { return; };
      this.projectileParticles.Stop(true);
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
      base.Reset();
    }
  }
}
