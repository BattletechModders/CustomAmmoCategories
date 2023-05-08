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
using CustomAmmoCategoriesHelper;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class AMSWeaponEffect : WeaponEffect {
    private static HashSet<AMSWeaponEffect> registredAMSEffects = new HashSet<AMSWeaponEffect>();
    public static void Register(AMSWeaponEffect effect) { registredAMSEffects.Add(effect); }
    public static void Sanitize() {
      foreach (var effect in registredAMSEffects) { effect.Pool(); }
    }
    public static void Clear() { registredAMSEffects.Clear(); }
    public Vector3[] hitPositions;
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
    public virtual float calculateInterceptCorrection(float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      float amsProjectileSpeed = this.projectileSpeed;
      float timeToIntercept = distance / amsProjectileSpeed;
      float missileDistanceToIntecept = missileProjectileSpeed * timeToIntercept;
      if (curPath <= missileDistanceToIntecept) { return 0.1f; };
      return (curPath - missileDistanceToIntecept) / pathLenth;
    }
    public void Init(WeaponEffect original) {
      AMSWeaponEffect.Register(this);
      this.impactVFXBase = original.impactVFXBase;
      this.preFireSFX = original.preFireSFX;
      this.Combat = original.Combat();
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
      this.preFireRate = original.preFireRate();
      this.duration = original.duration();
      this.rate = original.rate();
      this.projectileSpeed = original.projectileSpeed;
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
      this.impactVFXVariations = original.impactVFXVariations;
      this.armorDamageVFXName = original.armorDamageVFXName;
      this.structureDamageVFXName = original.structureDamageVFXName;
      this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
      this.FiringComplete = original.FiringComplete;
      this.AllowMissSkipping = original.AllowMissSkipping;
      this.hitPositions = new Vector3[0] { };
    }
    protected override int ImpactPrecacheCount {
      get {
        return 1;
      }
    }
    protected override void Awake() {
      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
      this.hasSentNextWeaponMessage(false);
      this.AllowMissSkipping = true;
    }
    protected override void Start() {
      if ((double)this.duration <= 0.0)
        this.duration = 1f;
      this.rate = 1f / this.duration;
    }
    protected override void OnDestroy() {
      if (this.projectileAudioObject == null) { return; }
      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
    }
    public override void Init(Weapon weapon) {
      Log.Combat?.WL(0,"CACWeaponEffect.Init");
      this.weapon = weapon;
      this.weaponRep = weapon.weaponRep;
      this.Combat = weapon.parent.Combat;
      if (this.weaponRep == null) {
        Log.Combat?.WL(0, "!!!I will not fire weapon without representation!!!Under no circumstances", true);
        this.numberOfEmitters = 1;
      } else {
        this.numberOfEmitters = this.weaponRep.vfxTransforms.Length;
      }
      if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null)
        this.Combat.DataManager.PrecachePrefabAsync(this.projectilePrefab.name, BattleTechResourceType.Prefab, 1);
      if ((UnityEngine.Object)this.preFireVFXPrefab != (UnityEngine.Object)null)
        this.Combat.DataManager.PrecachePrefabAsync(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, 1);
      if ((UnityEngine.Object)this.muzzleFlashVFXPrefab != (UnityEngine.Object)null)
        this.Combat.DataManager.PrecachePrefabAsync(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, 1);
      if (!string.IsNullOrEmpty(this.armorDamageVFXName)) {
        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
      }
      if (!string.IsNullOrEmpty(this.structureDamageVFXName)) {
        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
      }
      this.PreCacheImpacts();
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
    private void PreCacheImpacts() {
      if (string.IsNullOrEmpty(this.impactVFXBase))
        return;
      this.Combat.DataManager.PrecachePrefabAsync(string.Format("{0}_crit", (object)this.impactVFXBase), BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
      if (this.impactVFXVariations == null)
        return;
      for (int index = 0; index < this.impactVFXVariations.Length; ++index)
        this.Combat.DataManager.PrecachePrefabAsync(string.Format("{0}_{1}", (object)this.impactVFXBase, (object)this.impactVFXVariations[index]), BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
    }
    public override void InitProjectile() {
      Log.Combat?.WL(0,"AMSWeaponEffect.InitProjectile: "+this.GetType().ToString());
      Log.Combat?.WL(1, "projectilePrefab '"+(projectilePrefab==null?"null": (projectilePrefab.name+":"+projectilePrefab.GetInstanceID())));
      Log.Combat?.WL(1, "projectile '" + (projectile == null ? "null" : (projectile.name + ":" + projectile.GetInstanceID())));
      try {
        if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null && (UnityEngine.Object)this.projectile != (UnityEngine.Object)null) {
          this.weapon.parent.Combat.DataManager.PoolGameObject(this.activeProjectileName, this.projectile);
        }
        if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null) {
          this.activeProjectileName = this.projectilePrefab.name;
          this.projectile = this.Combat.DataManager.PooledInstantiate(this.activeProjectileName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        };
        if(this.projectile == null) {
          Log.Combat?.WL(1, "fail to get from pool '" + this.activeProjectileName + "'");
          return;
        } else {
          Log.Combat?.WL(1, "success instantine '" + this.activeProjectileName + "'");
        }
        this.projectileParticles = this.projectile.GetComponent<ParticleSystem>();
        this.projectileTransform = this.projectile.transform;
        MeshRenderer componentInChildren1 = this.projectile.GetComponentInChildren<MeshRenderer>(true);
        if ((UnityEngine.Object)componentInChildren1 != (UnityEngine.Object)null) {
          this.projectileMeshObject = componentInChildren1.gameObject;
          this.projectileMeshObject.SetActive(false);
        }
        BTLight componentInChildren2 = this.projectile.GetComponentInChildren<BTLight>(true);
        if ((UnityEngine.Object)componentInChildren2 != (UnityEngine.Object)null) {
          this.projectileLightObject = componentInChildren2.gameObject;
          this.projectileLightObject.SetActive(false);
        }
        this.ScaleWeaponEffect(this.projectile);
        //this.UpdateScale();
        this.projectileAudioObject = this.projectile.GetComponent<AkGameObj>();
        if ((UnityEngine.Object)this.projectileAudioObject == (UnityEngine.Object)null)
          this.projectileAudioObject = this.projectile.AddComponent<AkGameObj>();
        this.projectileAudioObject.listenerMask = 0;
        this.projectileAudioObject.isEnvironmentAware = false;
        WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.projectileAudioObject);
        this.parentAudioObject = !((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) || !((UnityEngine.Object)this.weapon.parent.GameRep.audioObject != (UnityEngine.Object)null) ? this.projectileAudioObject : this.weapon.parent.GameRep.audioObject;
        WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.parentAudioObject);
        Mech parent = this.weapon.parent as Mech;
        if (parent == null)
          return;
        AudioSwitch_mech_weight_type switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
        switch (parent.MechDef.Chassis.weightClass) {
          case WeightClass.LIGHT:
            switchEnumValue = AudioSwitch_mech_weight_type.a_light;
            break;
          case WeightClass.MEDIUM:
            switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
            break;
          case WeightClass.HEAVY:
            switchEnumValue = AudioSwitch_mech_weight_type.c_heavy;
            break;
          case WeightClass.ASSAULT:
            switchEnumValue = AudioSwitch_mech_weight_type.d_assault;
            break;
        }
        WwiseManager.SetSwitch<AudioSwitch_mech_weight_type>(switchEnumValue, this.projectileAudioObject);
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        WeaponEffect.logger.LogException(e);
      }
    }
    public virtual void Fire(Vector3[] hitPositions, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0,"AMSWeaponEffect.Fire");
      this.AudioStoped = false;
      this.t = 0.0f;
      this.hitPositions = hitPositions;
      this.hitIndex = hitIndex;
      this.emitterIndex = emitterIndex;
      if (this.weaponRep == null) {
        this.startingTransform = this.weapon.parent.GameRep.transform;
      } else { 
        this.startingTransform = this.weaponRep.vfxTransforms[emitterIndex];
      }
      this.startPos = this.startingTransform.position;
      this.endPos = hitPositions[hitIndex];
      this.currentPos = this.startPos;
      this.FiringComplete = false;
      this.InitProjectile();
      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
    }
    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
      Log.Combat?.WL(0,"AMS effect can't fire normaly. Something is wrong.");
      this.hitInfo = hitInfo;
      this.hitIndex = hitIndex;
      this.currentState = WeaponEffect.WeaponEffectState.Complete;
      base.OnComplete();
    }
    protected override void PlayPreFire() {
      if ((UnityEngine.Object)this.preFireVFXPrefab != (UnityEngine.Object)null) {
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
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
      if (!string.IsNullOrEmpty(this.preFireSFX)) {
        int num = (int)WwiseManager.PostEvent(this.preFireSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
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
    }
    protected override void PlayMuzzleFlash() {
      if (!((UnityEngine.Object)this.muzzleFlashVFXPrefab != (UnityEngine.Object)null))
        return;
      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.muzzleFlashVFXPrefab.name, component);
      component.Stop(true);
      component.Clear(true);
      component.transform.parent = this.startingTransform;
      component.transform.localPosition = Vector3.zero;
      component.transform.LookAt(this.endPos);
      BTCustomRenderer.SetVFXMultiplier(component);
      component.Play(true);
      BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
      if (!((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null))
        return;
      componentInChildren.StopAnimation();
      componentInChildren.PlayAnimation();
    }
    protected override void PlayProjectile() {
      this.ColorChangeSpeed = this.weapon.ColorSpeedChange();
      this.colorsTable = this.weapon.ColorsTable();
      this.colorChangeRule = this.weapon.colorChangeRule();
      this.colorT = 0f;
      this.ColorIndex = 0;
      this.StoreOriginalColor();
      //this.originalColor = this.beamRenderer.material.GetColor("_ColorBB");
      Log.Combat?.WL(1,"ColorChangeSpeed " + this.ColorChangeSpeed);
      Log.Combat?.WL(1, "colorsTable.Count " + this.colorsTable.Count);
      Log.Combat?.WL(1, "colorChangeRule " + this.colorChangeRule);
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
      Log.Combat?.WL(1, "NeedColorCalc " + this.NeedColorCalc);
      this.t = 0.0f;
      this.currentState = WeaponEffect.WeaponEffectState.Firing;
      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
        this.projectileMeshObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
        this.projectileLightObject.SetActive(true);
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        this.projectileParticles.Stop(true);
        this.projectileParticles.Clear(true);
      }
      this.projectileTransform.position = this.startingTransform.position;
      this.projectileTransform.LookAt(this.endPos);
      this.startPos = this.startingTransform.position;
      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
        BTCustomRenderer.SetVFXMultiplier(this.projectileParticles);
        this.projectileParticles.Play(true);
        BTLightAnimator componentInChildren = this.projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
        if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
          componentInChildren.StopAnimation();
          componentInChildren.PlayAnimation();
        }
      }
      if ((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) {
        int num;
        switch ((ChassisLocations)this.weapon.Location) {
          case ChassisLocations.LeftArm:
            num = 1;
            break;
          case ChassisLocations.RightArm:
            num = 2;
            break;
          default:
            num = 0;
            break;
        }
        this.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, this.weapon.weaponDef.AttackRecoil);
      }
    }
    protected override void PlayImpact() {
      if (!string.IsNullOrEmpty(this.impactVFXBase)) {
        string str1 = string.Empty;
        if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0) {
          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
        }
        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          WeaponEffect.logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2));
        } else {
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
    protected override void PlayTerrainImpactVFX() {
    }

    protected override void PlayImpactDamageOverlay() {
    }

    protected override void PlayImpactAudio() {
    }

    protected override void DestroyFlimsyObjects() {
      /*if (!this.shotsDestroyFlimsyObjects)
        return;
      foreach (Collider collider in Physics.OverlapSphere(this.endPos, 15f, -5, QueryTriggerInteraction.Ignore)) {
        DestructibleObject component = collider.gameObject.GetComponent<DestructibleObject>();
        if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.isFlimsy) {
          Vector3 normalized = (collider.transform.position - this.endPos).normalized;
          float forceMagnitude = this.weapon.DamagePerShot + this.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
          component.TakeDamage(this.endPos, normalized, forceMagnitude);
          component.Collapse(normalized, forceMagnitude);
        }
      }*/
    }

    protected override void Update() {
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
      this.PublishNextWeaponMessageCAC();
    }

    protected override void LateUpdate() {
    }

    protected override void OnPreFireComplete() {
    }

    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0.0f) {
    }
    protected override void OnComplete() {
      if (this.currentState == WeaponEffect.WeaponEffectState.Complete) { return; }
      this.StopAudio();
      this.currentState = WeaponEffect.WeaponEffectState.Complete;
      this.PublishNextWeaponMessageCAC();
      this.PublishWeaponCompleteMessageCAC();
      if ((this.projectile != null)&&(this.projectilePrefab != null)) {
        AutoPoolObject autoPoolObject = this.projectile.GetComponent<AutoPoolObject>();
        if (autoPoolObject == null) { autoPoolObject = this.projectile.AddComponent<AutoPoolObject>(); }
        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.activeProjectileName, 4f);
        this.projectile = null;
      }
    }
    protected void PublishNextWeaponMessageCAC() {
      this.attackSequenceNextDelayTimer = -1f;
      this.hasSentNextWeaponMessage(true);
    }
    public void PublishWeaponCompleteMessageCAC() {
      this.FiringComplete = true;
    }
    protected bool AudioStoped = false;
    public virtual void StopAudio() {
      if (AudioStoped == false) {
        AudioStoped = true;
        AudioStop();
      }
    }
    public virtual void AudioStop() {

    }
    public virtual void Pool() {
      if ((this.projectile != null) && (this.projectilePrefab != null)) {
        if (string.IsNullOrEmpty(this.activeProjectileName)) { this.activeProjectileName = this.projectilePrefab.name; }
        this.RestoreOriginalColor();
        this.weapon.parent.Combat.DataManager.PoolGameObject(this.activeProjectileName, this.projectile);
      }
      if(projectile == null) {
        this.projectileMeshObject = null;
        this.projectileLightObject = null;
      }
      if (this.projectileMeshObject != null) {
        this.projectileMeshObject.SetActive(false);
      }
      if (this.projectileLightObject != null) {
        this.projectileLightObject.SetActive(false);
      }
    }
    public override void Reset() {
      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
      this.StopAudio();
      this.hasSentNextWeaponMessage(false);
      if (this.projectileMeshObject != null) {
        this.projectileMeshObject.SetActive(false);
      }
      if (this.projectileLightObject != null) {
        this.projectileLightObject.SetActive(false);
      }
    }
  }
}
