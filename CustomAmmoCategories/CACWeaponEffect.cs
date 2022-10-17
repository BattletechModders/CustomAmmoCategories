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
using CustAmmoCategories;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomAmmoCategoriesPatches {
  /*  [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("Active")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class CACWeaponEffect_ActiveGet {
      public static bool Prefix(WeaponEffect __instance,ref bool __result) {
        CACWeaponEffect cacwe = __instance as CACWeaponEffect;
        if (cacwe != null) {__result = cacwe.ActiveCAC;return false;}
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("FiringComplete")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class CACWeaponEffect_FiringCompleteGet {
      public static bool Prefix(WeaponEffect __instance, ref bool __result) {
        CACWeaponEffect cacwe = __instance as CACWeaponEffect;
        if (cacwe != null) { __result = cacwe.FiringCompleteCAC; return false; }
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("FiringComplete")]
    [HarmonyPatch(MethodType.Setter)]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public static class CACWeaponEffect_FiringCompleteSet {
      public static bool Prefix(WeaponEffect __instance, bool value) {
        CACWeaponEffect cacwe = __instance as CACWeaponEffect;
        if (cacwe != null) { cacwe.FiringCompleteCAC = value; return false; }
        return true;
      }
    }*/
}

namespace CustAmmoCategories {
  public class CACAMSEffect {
    //public Dictionary<int, CACWeaponEffect> fireEffects
  }

//  public class CACBurstBallisticEffect : CACWeaponEffect {
//    public float impactTime = 0.5f;
//    private int bulletsFired;
//    public GameObject accurateProjectilePrefab;
//    public GameObject inaccurateProjectilePrefab;
//    public string preFireSoundEvent;
//    public string projectileSoundEvent;
//    public string fireCompleteStopEvent;
//    private float floatieInterval;
//    private float nextFloatie;

//    public void Init(BurstBallisticEffect original) {
//      base.Init(original);
//      CustomAmmoCategoriesLog.Log.LogWrite("Initing CAC burst\n");
//      this.impactTime = original.impactTime;
//      this.bulletsFired = (int)typeof(BurstBallisticEffect).GetField("bulletsFired", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.accurateProjectilePrefab = original.accurateProjectilePrefab;
//      this.inaccurateProjectilePrefab = original.inaccurateProjectilePrefab;
//      this.preFireSoundEvent = original.preFireSoundEvent;
//      this.projectileSoundEvent = original.projectileSoundEvent;
//      this.fireCompleteStopEvent = original.fireCompleteStopEvent;
//      this.floatieInterval = (float)typeof(BurstBallisticEffect).GetField("floatieInterval", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.nextFloatie = (float)typeof(BurstBallisticEffect).GetField("nextFloatie", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//    }

//#if PUBLIC_ASSEMBLIES
//    public override int ImpactPrecacheCount {
//#else
//    protected override int ImpactPrecacheCount {
//#endif

//      get {
//        return 5;
//      }
//    }
//#if PUBLIC_ASSEMBLIES
//    public override void Awake() {
//#else
//    protected override void Awake() {
//#endif
//      base.Awake();
//    }
//    protected override void Start() {
//      base.Start();
//      this.floatieInterval = 1f / (float)this.weapon.ProjectilesPerShot;
//    }
//    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
//      CustomAmmoCategoriesLog.Log.LogWrite("CACBurstBallisticEffect.Fire\n");
//      if (hitInfo.hitLocations[hitIndex] != 0 && hitInfo.hitLocations[hitIndex] != 65536)
//        this.projectilePrefab = this.accurateProjectilePrefab;
//      else
//        this.projectilePrefab = this.inaccurateProjectilePrefab;
//      this.bulletsFired = 0;
//      base.Fire(hitInfo, hitIndex, emitterIndex);
//      this.nextFloatie = 0.0f;
//      this.impactTime = Mathf.Clamp01(this.impactTime);
//      this.duration = this.projectileSpeed;
//      if ((double)this.duration > 4.0)
//        this.duration = 4f;
//      this.rate = 1f / this.duration;
//      this.PlayPreFire();
//    }
//    protected override void PlayPreFire() {
//      CustomAmmoCategoriesLog.Log.LogWrite("CACBurstBallisticEffect.PlayPreFire\n");
//      base.PlayPreFire();
//      this.t = 0.0f;
//      if (string.IsNullOrEmpty(this.preFireSoundEvent))
//        return;
//      int num = (int)WwiseManager.PostEvent(this.preFireSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//    }
//    protected override void PlayMuzzleFlash() {
//      base.PlayMuzzleFlash();
//    }
//    protected override void PlayProjectile() {
//      this.PlayMuzzleFlash();
//      this.t = 0.0f;
//      if (!string.IsNullOrEmpty(this.projectileSoundEvent)) {
//        int num = (int)WwiseManager.PostEvent(this.projectileSoundEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//      }
//      base.PlayProjectile();
//    }
//    protected override void PlayImpact() {
//      ++this.bulletsFired;
//      this.PlayImpactAudio();
//      base.PlayImpact();
//    }
//#if PUBLIC_ASSEMBLIES
//    public override void Update() {
//#else
//    protected override void Update() {
//#endif
//      base.Update();
//      if (this.currentState != WeaponEffect.WeaponEffectState.Firing)
//        return;
//      if ((double)this.t >= (double)this.impactTime && (double)this.t >= (double)this.nextFloatie && (this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536)) {
//        this.nextFloatie = this.t + this.floatieInterval;
//        this.PlayImpact();
//      }
//      if ((double)this.t < 1.0)
//        return;
//      float hitDamage = this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask);
//      for (int index = 0; index < this.weapon.ShotsWhenFired; ++index) {
//        if (this.hitInfo.hitLocations[index] != 0 && this.hitInfo.hitLocations[index] != 65536) {
//          this.hitIndex = index;
//          this.OnImpact(hitDamage);
//        }
//      }
//      this.OnComplete();
//    }
//    protected override void OnPreFireComplete() {
//      base.OnPreFireComplete();
//      this.PlayProjectile();
//    }
//    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
//      if ((double)hitDamage <= 1.0 / 1000.0)
//        return;
//      base.OnImpact(hitDamage, structureDamage);
//    }
//    protected override void OnComplete() {
//      base.OnComplete();
//      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null)
//        this.projectileParticles.Stop(true);
//      if (string.IsNullOrEmpty(this.fireCompleteStopEvent))
//        return;
//      int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//    }
//    public override void Reset() {
//      if (this.Active && !string.IsNullOrEmpty(this.fireCompleteStopEvent)) {
//        int num = (int)WwiseManager.PostEvent(this.fireCompleteStopEvent, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//      }
//      base.Reset();
//    }
//  }
//  public class CACWeaponEffect : WeaponEffect {
//    private static FieldInfo fi_hasSentNextWeaponMessage = null;
//    public void Init(WeaponEffect original) {
//      this.impactVFXBase = original.impactVFXBase;
//      this.preFireSFX = original.preFireSFX;
//      this.Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.hitInfo = original.hitInfo;
//      this.hitIndex = original.HitIndex();
//      this.emitterIndex = (int)typeof(WeaponEffect).GetField("emitterIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.subEffect = original.subEffect;
//      this.currentState = original.currentState;
//      this.weaponRep = original.weaponRep;
//      this.weapon = original.weapon;
//      this.parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.startPos = (Vector3)typeof(WeaponEffect).GetField("startPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.currentPos = (Vector3)typeof(WeaponEffect).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.attackSequenceNextDelayMin = original.attackSequenceNextDelayMin;
//      this.attackSequenceNextDelayMax = original.attackSequenceNextDelayMax;
//      this.attackSequenceNextDelayTimer = (float)typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
//      if (fi_hasSentNextWeaponMessage != null) {
//        this.hasSentNextWeaponMessage = (bool)fi_hasSentNextWeaponMessage.GetValue(original);
//      } else {
//        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
//      }
//      this.preFireDuration = original.preFireDuration;
//      this.preFireRate = (float)typeof(WeaponEffect).GetField("preFireRate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.duration = (float)typeof(WeaponEffect).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.rate = (float)typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileSpeed = original.projectileSpeed;
//      this.weaponImpactType = original.weaponImpactType;
//      this.preFireVFXPrefab = original.preFireVFXPrefab;
//      this.muzzleFlashVFXPrefab = original.muzzleFlashVFXPrefab;
//      this.projectilePrefab = original.projectilePrefab;
//      this.projectile = original.projectile;
//      this.activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(original);
//      this.impactVFXVariations = original.impactVFXVariations;
//      this.armorDamageVFXName = original.armorDamageVFXName;
//      this.structureDamageVFXName = original.structureDamageVFXName;
//      this.shotsDestroyFlimsyObjects = original.shotsDestroyFlimsyObjects;
//      this.FiringComplete = original.FiringComplete;
//      this.AllowMissSkipping = original.AllowMissSkipping;
//    }
//#if PUBLIC_ASSEMBLIES
//    public override int ImpactPrecacheCount {
//#else
//    protected override int ImpactPrecacheCount {
//#endif
//      get {
//        return 1;
//      }
//    }
//    protected bool hasSentNextWeaponMessage {
//      get {
//        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
//        if (fi_hasSentNextWeaponMessage != null) {
//          return (bool)fi_hasSentNextWeaponMessage.GetValue(this);
//        } else {
//          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't get WeaponEffect.hasSentNextWeaponMessage\n");
//          return false;
//        }
//      }
//      set {
//        if (fi_hasSentNextWeaponMessage == null) { fi_hasSentNextWeaponMessage = typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic); }
//        if (fi_hasSentNextWeaponMessage != null) {
//          fi_hasSentNextWeaponMessage.SetValue(this, value);
//        } else {
//          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Can't set WeaponEffect.hasSentNextWeaponMessage\n");
//        }
//      }
//    }
//#if PUBLIC_ASSEMBLIES
//    public override void Awake() {
//#else
//    protected override void Awake() {
//#endif
//      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
//      this.hasSentNextWeaponMessage = false;
//      this.AllowMissSkipping = true;
//    }
//    protected override void Start() {
//      if ((double)this.duration <= 0.0)
//        this.duration = 1f;
//      this.rate = 1f / this.duration;
//    }
//    protected override void OnDestroy() {
//      if (!((UnityEngine.Object)this.projectileAudioObject != (UnityEngine.Object)null))
//        return;
//      AkSoundEngine.StopAll(this.projectileAudioObject.gameObject);
//    }
//    public override void Init(Weapon weapon) {
//      CustomAmmoCategoriesLog.Log.LogWrite("CACWeaponEffect.Init\n");
//      this.weapon = weapon;
//      this.weaponRep = weapon.weaponRep;
//      this.Combat = weapon.parent.Combat;
//      this.numberOfEmitters = this.weaponRep.vfxTransforms.Length;
//      if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null)
//        this.Combat.DataManager.PrecachePrefabAsync(this.projectilePrefab.name, BattleTechResourceType.Prefab, 1);
//      if ((UnityEngine.Object)this.preFireVFXPrefab != (UnityEngine.Object)null)
//        this.Combat.DataManager.PrecachePrefabAsync(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, 1);
//      if ((UnityEngine.Object)this.muzzleFlashVFXPrefab != (UnityEngine.Object)null)
//        this.Combat.DataManager.PrecachePrefabAsync(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, 1);
//      if (!string.IsNullOrEmpty(this.armorDamageVFXName)) {
//        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
//        this.Combat.DataManager.PrecachePrefabAsync(this.armorDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
//      }
//      if (!string.IsNullOrEmpty(this.structureDamageVFXName)) {
//        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_sm", BattleTechResourceType.Prefab, 1);
//        this.Combat.DataManager.PrecachePrefabAsync(this.structureDamageVFXName + "_lrg", BattleTechResourceType.Prefab, 1);
//      }
//      this.PreCacheImpacts();
//    }
//    private void PreCacheImpacts() {
//      if (string.IsNullOrEmpty(this.impactVFXBase))
//        return;
//      this.Combat.DataManager.PrecachePrefabAsync(string.Format("{0}_crit", (object)this.impactVFXBase), BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
//      if (this.impactVFXVariations == null)
//        return;
//      for (int index = 0; index < this.impactVFXVariations.Length; ++index)
//        this.Combat.DataManager.PrecachePrefabAsync(string.Format("{0}_{1}", (object)this.impactVFXBase, (object)this.impactVFXVariations[index]), BattleTechResourceType.Prefab, this.ImpactPrecacheCount);
//    }
//    public override void InitProjectile() {
//      if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null && (UnityEngine.Object)this.projectile != (UnityEngine.Object)null)
//        this.weapon.parent.Combat.DataManager.PoolGameObject(this.activeProjectileName, this.projectile);
//      if ((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null) {
//        this.activeProjectileName = this.projectilePrefab.name;
//        this.projectile = this.Combat.DataManager.PooledInstantiate(this.activeProjectileName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
//      }
//      this.projectileParticles = this.projectile.GetComponent<ParticleSystem>();
//      this.projectileTransform = this.projectile.transform;
//      MeshRenderer componentInChildren1 = this.projectile.GetComponentInChildren<MeshRenderer>(true);
//      if ((UnityEngine.Object)componentInChildren1 != (UnityEngine.Object)null) {
//        this.projectileMeshObject = componentInChildren1.gameObject;
//        this.projectileMeshObject.SetActive(false);
//      }
//      BTLight componentInChildren2 = this.projectile.GetComponentInChildren<BTLight>(true);
//      if ((UnityEngine.Object)componentInChildren2 != (UnityEngine.Object)null) {
//        this.projectileLightObject = componentInChildren2.gameObject;
//        this.projectileLightObject.SetActive(false);
//      }
//      this.projectileAudioObject = this.projectile.GetComponent<AkGameObj>();
//      if ((UnityEngine.Object)this.projectileAudioObject == (UnityEngine.Object)null)
//        this.projectileAudioObject = this.projectile.AddComponent<AkGameObj>();
//      this.projectileAudioObject.listenerMask = 0;
//      this.projectileAudioObject.isEnvironmentAware = false;
//      WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.projectileAudioObject);
//      this.parentAudioObject = !((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) || !((UnityEngine.Object)this.weapon.parent.GameRep.audioObject != (UnityEngine.Object)null) ? this.projectileAudioObject : this.weapon.parent.GameRep.audioObject;
//      WwiseManager.SetSwitch<AudioSwitch_weapon_type>(this.weaponImpactType, this.parentAudioObject);
//      Mech parent = this.weapon.parent as Mech;
//      if (parent == null)
//        return;
//      AudioSwitch_mech_weight_type switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
//      switch (parent.MechDef.Chassis.weightClass) {
//        case WeightClass.LIGHT:
//          switchEnumValue = AudioSwitch_mech_weight_type.a_light;
//          break;
//        case WeightClass.MEDIUM:
//          switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
//          break;
//        case WeightClass.HEAVY:
//          switchEnumValue = AudioSwitch_mech_weight_type.c_heavy;
//          break;
//        case WeightClass.ASSAULT:
//          switchEnumValue = AudioSwitch_mech_weight_type.d_assault;
//          break;
//      }
//      WwiseManager.SetSwitch<AudioSwitch_mech_weight_type>(switchEnumValue, this.projectileAudioObject);
//    }
//    public override void Fire(WeaponHitInfo hitInfo, int hitIndex = 0, int emitterIndex = 0) {
//      CustomAmmoCategoriesLog.Log.LogWrite("CACWeaponEffect.Fire\n");
//      this.t = 0.0f;
//      this.hitIndex = hitIndex;
//      this.emitterIndex = emitterIndex;
//      this.hitInfo = hitInfo;
//      this.startingTransform = this.weaponRep.vfxTransforms[emitterIndex];
//      this.startPos = this.startingTransform.position;
//      ICombatant combatantByGuid = this.Combat.FindCombatantByGUID(hitInfo.targetId);
//      if (combatantByGuid != null) {
//        string secondaryTargetId = (string)null;
//        int secondaryHitLocation = 0;
//        hitInfo.hitPositions[hitIndex] = combatantByGuid.GetImpactPosition(this.weaponRep.parentCombatant as AbstractActor, this.startPos, this.weapon, ref hitInfo.hitLocations[hitIndex], ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
//      }
//      this.endPos = hitInfo.hitPositions[hitIndex];
//      this.currentPos = this.startPos;
//      this.FiringComplete = false;
//      this.InitProjectile();
//      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
//    }
//    protected override void PlayPreFire() {
//      if ((UnityEngine.Object)this.preFireVFXPrefab != (UnityEngine.Object)null) {
//        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.preFireVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
//        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
//        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
//        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
//          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
//        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.preFireVFXPrefab.name, component);
//        component.Stop(true);
//        component.Clear(true);
//        component.transform.parent = (Transform)null;
//        component.transform.position = this.startingTransform.position;
//        component.transform.LookAt(this.endPos);
//        BTCustomRenderer.SetVFXMultiplier(component);
//        component.Play(true);
//        if ((double)this.preFireDuration <= 0.0)
//          this.preFireDuration = component.main.duration;
//      }
//      if (!string.IsNullOrEmpty(this.preFireSFX)) {
//        int num = (int)WwiseManager.PostEvent(this.preFireSFX, this.parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//      }
//      this.preFireRate = (double)this.preFireDuration <= 0.0 ? 1000f : 1f / this.preFireDuration;
//      if ((double)this.attackSequenceNextDelayMin <= 0.0 && (double)this.attackSequenceNextDelayMax <= 0.0)
//        this.attackSequenceNextDelayMax = this.preFireDuration;
//      if ((double)this.attackSequenceNextDelayMax <= 0.0)
//        this.attackSequenceNextDelayMax = 0.05f;
//      if ((double)this.attackSequenceNextDelayMin >= (double)this.attackSequenceNextDelayMax)
//        this.attackSequenceNextDelayMin = this.attackSequenceNextDelayMax;
//      this.attackSequenceNextDelayTimer = UnityEngine.Random.Range(this.attackSequenceNextDelayMin, this.attackSequenceNextDelayMax);
//      this.t = 0.0f;
//      this.currentState = WeaponEffect.WeaponEffectState.PreFiring;
//    }
//    protected override void PlayMuzzleFlash() {
//      if (!((UnityEngine.Object)this.muzzleFlashVFXPrefab != (UnityEngine.Object)null))
//        return;
//      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(this.muzzleFlashVFXPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
//      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
//      AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
//      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
//        autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
//      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.muzzleFlashVFXPrefab.name, component);
//      component.Stop(true);
//      component.Clear(true);
//      component.transform.parent = this.startingTransform;
//      component.transform.localPosition = Vector3.zero;
//      component.transform.LookAt(this.endPos);
//      BTCustomRenderer.SetVFXMultiplier(component);
//      component.Play(true);
//      BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
//      if (!((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null))
//        return;
//      componentInChildren.StopAnimation();
//      componentInChildren.PlayAnimation();
//    }
//    protected override void PlayProjectile() {
//      this.t = 0.0f;
//      this.currentState = WeaponEffect.WeaponEffectState.Firing;
//      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
//        this.projectileMeshObject.SetActive(true);
//      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
//        this.projectileLightObject.SetActive(true);
//      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
//        this.projectileParticles.Stop(true);
//        this.projectileParticles.Clear(true);
//      }
//      this.projectileTransform.position = this.startingTransform.position;
//      this.projectileTransform.LookAt(this.endPos);
//      this.startPos = this.startingTransform.position;
//      if ((UnityEngine.Object)this.projectileParticles != (UnityEngine.Object)null) {
//        BTCustomRenderer.SetVFXMultiplier(this.projectileParticles);
//        this.projectileParticles.Play(true);
//        BTLightAnimator componentInChildren = this.projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
//        if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
//          componentInChildren.StopAnimation();
//          componentInChildren.PlayAnimation();
//        }
//      }
//      if ((UnityEngine.Object)this.weapon.parent.GameRep != (UnityEngine.Object)null) {
//        int num;
//        switch ((ChassisLocations)this.weapon.Location) {
//          case ChassisLocations.LeftArm:
//            num = 1;
//            break;
//          case ChassisLocations.RightArm:
//            num = 2;
//            break;
//          default:
//            num = 0;
//            break;
//        }
//        this.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, this.weapon.weaponDef.AttackRecoil);
//      }
//      if (!this.AllowMissSkipping || this.hitInfo.hitLocations[this.hitIndex] != 0 && this.hitInfo.hitLocations[this.hitIndex] != 65536)
//        return;
//      this.PublishWeaponCompleteMessageCAC();
//    }
//    protected override void PlayImpact() {
//      if (!string.IsNullOrEmpty(this.impactVFXBase) && this.hitInfo.hitLocations[this.hitIndex] != 0) {
//        string str1 = string.Empty;
//        ICombatant combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId);
//        if (this.hitInfo.hitLocations[this.hitIndex] != 65536 && combatantByGuid != null && (double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)combatantByGuid.ArmorForLocation(this.hitInfo.hitLocations[this.hitIndex]))
//          str1 = "_crit";
//        else if (this.impactVFXVariations != null && this.impactVFXVariations.Length > 0)
//          str1 = "_" + this.impactVFXVariations[UnityEngine.Random.Range(0, this.impactVFXVariations.Length)];
//        string str2 = string.Format("{0}{1}", (object)this.impactVFXBase, (object)str1);
//        GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
//        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
//          WeaponEffect.logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayImpact had an invalid VFX name: " + str2));
//        } else {
//          ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
//          component.Stop(true);
//          component.Clear(true);
//          component.transform.position = this.endPos;
//          component.transform.LookAt(this.startingTransform.position);
//          BTCustomRenderer.SetVFXMultiplier(component);
//          component.Play(true);
//          BTLightAnimator componentInChildren = gameObject.GetComponentInChildren<BTLightAnimator>(true);
//          if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
//            componentInChildren.StopAnimation();
//            componentInChildren.PlayAnimation();
//          }
//          AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
//          if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
//            autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
//          autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
//        }
//      }
//      this.PlayImpactDamageOverlay();
//      if (this.hitInfo.hitLocations[this.hitIndex] == 65536) {
//        this.PlayTerrainImpactVFX();
//        this.DestroyFlimsyObjects();
//      }
//      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
//        this.projectileMeshObject.SetActive(false);
//      if ((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null)
//        this.projectileLightObject.SetActive(false);
//      this.OnImpact(0.0f);
//    }
//    protected override void PlayTerrainImpactVFX() {
//      MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
//      if (cellAt == null)
//        return;
//      string empty = string.Empty;
//      AudioSwitch_surface_type audioSurfaceType = cellAt.GetAudioSurfaceType();
//      string vfxNameModifier = cellAt.GetVFXNameModifier();
//      string str1;
//      switch (audioSurfaceType) {
//        case AudioSwitch_surface_type.dirt:
//          str1 = "dirt";
//          break;
//        case AudioSwitch_surface_type.metal:
//          str1 = "metal";
//          break;
//        case AudioSwitch_surface_type.snow:
//          str1 = "snow";
//          break;
//        case AudioSwitch_surface_type.wood:
//          str1 = "wood";
//          break;
//        case AudioSwitch_surface_type.brush:
//          str1 = "brush";
//          break;
//        case AudioSwitch_surface_type.concrete:
//          str1 = "concrete";
//          break;
//        case AudioSwitch_surface_type.debris_glass:
//          str1 = "debris_glass";
//          break;
//        case AudioSwitch_surface_type.gravel:
//          str1 = "gravel";
//          break;
//        case AudioSwitch_surface_type.ice:
//          str1 = "ice";
//          break;
//        case AudioSwitch_surface_type.lava:
//          str1 = "lava";
//          break;
//        case AudioSwitch_surface_type.mud:
//          str1 = "mud";
//          break;
//        case AudioSwitch_surface_type.sand:
//          str1 = "sand";
//          break;
//        case AudioSwitch_surface_type.water_deep:
//        case AudioSwitch_surface_type.water_shallow:
//          str1 = "water";
//          break;
//        default:
//          str1 = "dirt";
//          break;
//      }
//      string str2 = string.Format("{0}{1}{2}_sm", (object)this.Combat.Constants.VFXNames.groundImpactBase, (object)str1, (object)vfxNameModifier);
//      GameObject gameObject = this.weapon.parent.Combat.DataManager.PooledInstantiate(str2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
//      if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
//        WeaponEffect.logger.LogError((object)(this.weapon.Name + " WeaponEffect.PlayTerrainImpactVFX had an invalid VFX name: " + str2));
//      } else {
//        ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
//        component.Stop(true);
//        component.Clear(true);
//        component.transform.position = this.endPos;
//        component.transform.LookAt(this.startingTransform.position);
//        BTCustomRenderer.SetVFXMultiplier(component);
//        component.Play(true);
//        AutoPoolObject autoPoolObject = gameObject.GetComponent<AutoPoolObject>();
//        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
//          autoPoolObject = gameObject.AddComponent<AutoPoolObject>();
//        autoPoolObject.Init(this.weapon.parent.Combat.DataManager, str2, component);
//      }
//    }
//    protected override void PlayImpactDamageOverlay() {
//      if (this.hitInfo.hitLocations[this.hitIndex] == 0 || this.hitInfo.hitLocations[this.hitIndex] == 65536)
//        return;
//      ICombatant combatantByGuid = this.Combat.FindCombatantByGUID(this.hitInfo.targetId);
//      if (combatantByGuid == null || !((UnityEngine.Object)combatantByGuid.GameRep != (UnityEngine.Object)null))
//        return;
//      string empty = string.Empty;
//      bool flag = false;
//      string str1;
//      if ((double)this.weapon.DamagePerShotAdjusted(this.weapon.parent.occupiedDesignMask) > (double)combatantByGuid.ArmorForLocation(this.hitInfo.hitLocations[this.hitIndex])) {
//        str1 = this.structureDamageVFXName;
//        flag = true;
//      } else
//        str1 = this.armorDamageVFXName;
//      string str2 = "_sm";
//      Mech mech = combatantByGuid as Mech;
//      if (mech != null && (mech.weightClass == WeightClass.ASSAULT || mech.weightClass == WeightClass.HEAVY))
//        str2 = "_lrg";
//      string vfxName = string.Format("{0}{1}", (object)str1, (object)str2);
//      ChassisLocations fromArmorLocation = MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)this.hitInfo.hitLocations[this.hitIndex]);
//      combatantByGuid.GameRep.PlayVFX((int)fromArmorLocation, vfxName, true, this.startPos, !flag, -1f);
//    }
//    protected override void PlayImpactAudio() {
//      if (this.hitInfo.hitLocations[this.hitIndex] == 0)
//        return;
//      AudioSwitch_surface_type switchEnumValue = AudioSwitch_surface_type.metal;
//      AbstractActor hitInfoTarget = this.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(this.hitInfo) as AbstractActor;
//      if (this.hitInfo.hitLocations[this.hitIndex] == 65536) {
//        MapTerrainDataCell cellAt = this.weapon.parent.Combat.MapMetaData.GetCellAt(this.hitInfo.hitPositions[this.hitIndex]);
//        if (cellAt != null)
//          switchEnumValue = cellAt.GetAudioSurfaceType();
//      } else if (hitInfoTarget != null && (double)hitInfoTarget.ArmorForLocation(this.hitInfo.hitLocations[this.hitIndex]) <= 0.0)
//        switchEnumValue = AudioSwitch_surface_type.mech_internal_structure;
//      WwiseManager.SetSwitch<AudioSwitch_surface_type>(switchEnumValue, this.projectileAudioObject);
//      int num = (int)WwiseManager.PostEvent<AudioEventList_impact>(AudioEventList_impact.impact_weapon, this.projectileAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
//    }
//    protected override void DestroyFlimsyObjects() {
//      if (!this.shotsDestroyFlimsyObjects)
//        return;
//      foreach (Collider collider in Physics.OverlapSphere(this.endPos, 15f, -5, QueryTriggerInteraction.Ignore)) {
//        DestructibleObject component = collider.gameObject.GetComponent<DestructibleObject>();
//        if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.isFlimsy) {
//          Vector3 normalized = (collider.transform.position - this.endPos).normalized;
//          float forceMagnitude = this.weapon.DamagePerShot + this.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
//          component.TakeDamage(this.endPos, normalized, forceMagnitude);
//          component.Collapse(normalized, forceMagnitude);
//        }
//      }
//    }
//#if PUBLIC_ASSEMBLIES
//    public override void Update() {
//#else
//    protected override void Update() {
//#endif
//      if (this.currentState == WeaponEffect.WeaponEffectState.PreFiring) {
//        if ((double)this.t <= 1.0)
//          this.t += this.preFireRate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
//        if ((double)this.t >= 1.0)
//          this.OnPreFireComplete();
//      }
//      if (this.currentState == WeaponEffect.WeaponEffectState.Firing && (double)this.t <= 1.0)
//        this.t += this.rate * this.Combat.StackManager.GetProgressiveAttackDeltaTime(this.t);
//      if (!this.Active || this.subEffect || (this.weapon.WeaponCategoryValue.IsMelee || (double)this.attackSequenceNextDelayTimer <= 0.0))
//        return;
//      this.attackSequenceNextDelayTimer -= this.Combat.StackManager.GetProgressiveAttackDeltaTime(0.01f);
//      if ((double)this.attackSequenceNextDelayTimer > 0.0)
//        return;
//      this.PublishNextWeaponMessageCAC();
//    }
//    protected override void LateUpdate() {
//    }
//    protected override void OnPreFireComplete() {
//    }
//    protected override void OnImpact(float hitDamage = 0.0f, float structureDamage = 0f) {
//      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(this.hitInfo, this.hitIndex, hitDamage, structureDamage));
//    }
//    protected override void OnComplete() {
//      if (this.currentState == WeaponEffect.WeaponEffectState.Complete)
//        return;
//      this.currentState = WeaponEffect.WeaponEffectState.Complete;
//      if (!this.subEffect)
//        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceResolveDamageMessage(this.hitInfo));
//      this.PublishNextWeaponMessageCAC();
//      this.PublishWeaponCompleteMessageCAC();
//      if (!((UnityEngine.Object)this.projectilePrefab != (UnityEngine.Object)null))
//        return;
//      AutoPoolObject autoPoolObject = this.projectile.GetComponent<AutoPoolObject>();
//      if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null)
//        autoPoolObject = this.projectile.AddComponent<AutoPoolObject>();
//      autoPoolObject.Init(this.weapon.parent.Combat.DataManager, this.activeProjectileName, 4f);
//      this.projectile = (GameObject)null;
//    }
//    protected void PublishNextWeaponMessageCAC() {
//      if (!this.subEffect && !this.hasSentNextWeaponMessage)
//        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponPreFireCompleteMessage(this.hitInfo.stackItemUID, this.hitInfo.attackSequenceId, this.hitInfo.attackGroupIndex, this.hitInfo.attackWeaponIndex));
//      this.attackSequenceNextDelayTimer = -1f;
//      this.hasSentNextWeaponMessage = true;
//    }
//    public void PublishWeaponCompleteMessageCAC() {
//      if (!this.subEffect && !this.FiringComplete)
//        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponCompleteMessage(this.hitInfo.stackItemUID, this.hitInfo.attackSequenceId, this.hitInfo.attackGroupIndex, this.hitInfo.attackWeaponIndex));
//      this.FiringComplete = true;
//    }
//    public override void Reset() {
//      this.currentState = WeaponEffect.WeaponEffectState.NotStarted;
//      this.hasSentNextWeaponMessage = false;
//      if ((UnityEngine.Object)this.projectileMeshObject != (UnityEngine.Object)null)
//        this.projectileMeshObject.SetActive(false);
//      if (!((UnityEngine.Object)this.projectileLightObject != (UnityEngine.Object)null))
//        return;
//      this.projectileLightObject.SetActive(false);
//    }
//  }
}
