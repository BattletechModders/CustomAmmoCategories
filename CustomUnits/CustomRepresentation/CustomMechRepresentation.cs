using BattleTech;
using BattleTech.Rendering;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using CustAmmoCategories;
using BattleTech.Data;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using BattleTech.Rendering.UI;
using MechResizer;
using Localize;
using System.Threading;

namespace CustomUnits {
  [HarmonyPatch(typeof(ActorFactory))]
  [HarmonyPatch("CreateMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(TagSet), typeof(CombatGameState), typeof(string), typeof(string), typeof(HeraldryDef) })]
  public static class ActorFactory_CreateMech {
    public static bool Prefix(MechDef mDef, PilotDef pilot, TagSet additionalTags, CombatGameState combat, string uniqueId, string spawnerId, HeraldryDef customHeraldryDef, ref Mech __result) {
      try {
        Log.TWL(0, "ActorFactory.CreateMech " + mDef.Description.Id);
        UnitCustomInfo info = mDef.Chassis.GetCustomInfo();
        if (info != null) {
          if (info.SquadInfo.Troopers >= 1) {
            __result = new TrooperSquad(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
          }
          if((info.FakeVehicle == true) || (mDef.ChassisID.IsInFakeChassis() == true)) {
            __result = new FakeVehicleMech(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
          }
        }
        if (__result == null) {
          __result = new CustomMech(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
        }
        combat.ItemRegistry.AddItem((ITaggedItem)__result);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechDestructibleObject))]
  [HarmonyPatch("Collapse")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float) })]
  public static class MechDestructibleObject_Collapse {
    //public static void GatherRigidbodies(this MechDestructibleObject __instance, ref Rigidbody[] destroyedRigidbodies) {
    //  if (__instance.destroyedObj != null) {
    //    this.destroyedRigidbodies = this.destroyedObj.GetComponentsInChildren<Rigidbody>();
    //    this.destroyedObj.SetActive(false);
    //  } else
    //    this.destroyedRigidbodies = new Rigidbody[0];
    //}
    public static bool Prefix(MechDestructibleObject __instance, Vector3 forceDirection, float forceMagnitude,ref Rigidbody[] ___destroyedRigidbodies) {
      try {
        Log.TWL(0, "MechDestructibleObject.Collapse " + __instance.gameObject.name);
        if (__instance.destroyedObj == null) { return true; }
        Log.WL(1, "destroyedObj " + __instance.destroyedObj.name);
        Rigidbody[] bodies = __instance.destroyedObj.GetComponentsInChildren<Rigidbody>(true);
        foreach(Rigidbody body in bodies) {
          Log.WL(2, "Rigidbody " + body.name);
          SkinnedMeshRenderer rnd_body = body.gameObject.GetComponent<SkinnedMeshRenderer>();
          if (rnd_body.rootBone == null) { continue; }
          if (rnd_body.rootBone == rnd_body.gameObject.transform) { continue; }
          Transform prevRoot = rnd_body.rootBone;
          rnd_body.rootBone = rnd_body.gameObject.transform;
          rnd_body.gameObject.transform.position = prevRoot.position;
          rnd_body.gameObject.transform.rotation = prevRoot.rotation;
          rnd_body.gameObject.transform.localScale = prevRoot.lossyScale;
        }
        //if (__instance.wholeObj != null) { __instance.wholeObj.SetActive(false); }
        //if (__instance.destroyedObj == null) { return false; }
        //__instance.destroyedObj.SetActive(true);
        //Log.WL(1, "destroyedObj " + __instance.destroyedObj.name+" is active "+ __instance.destroyedObj.activeInHierarchy);
        //if (___destroyedRigidbodies == null) { __instance.GatherRigidbodies(ref ___destroyedRigidbodies); }
        //for (int index = 0; index < ___destroyedRigidbodies.Length; ++index) {
        //  Rigidbody destroyedRigidbody = ___destroyedRigidbodies[index];
        //  Collider component = destroyedRigidbody.gameObject.GetComponent<Collider>();
        //  destroyedRigidbody.gameObject.transform.parent = (Transform)null;
        //  if (!__instance.CheckCollisionWithTerrain(component)) {
        //    if (!destroyedRigidbody.isKinematic) {
        //      destroyedRigidbody.gameObject.layer = LayerMask.NameToLayer("VFXPhysicsActive");
        //      float num = Mathf.Pow(9f, (float)Mathf.FloorToInt(Mathf.Log10(destroyedRigidbody.mass)));
        //      destroyedRigidbody.AddExplosionForce(forceMagnitude * num, __instance.destroyedObj.transform.position - forceDirection * 3f, 0.0f, 10f, ForceMode.Impulse);
        //    }
        //  } else
        //    Debug.LogError((object)(destroyedRigidbody.name + " failed collapse "));
        //}
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  public class ComponentRepresentationInfo {
    public enum RepType { Component, Weapon, Support }
    public MechComponent component { get; private set; }
    public ChassisLocations prefabLocation { get; private set; }
    public ChassisLocations attachLocation { get; private set; }
    public RepType repType { get; private set; }
    public ComponentRepresentationInfo(MechComponent component, ChassisLocations prefabLocation, ChassisLocations attachLocation, RepType repType) {
      this.component = component;
      this.prefabLocation = prefabLocation;
      this.attachLocation = attachLocation;
      this.repType = repType;
    }
  }
  public class CustomMech: Mech, ICustomMech {
    public CustomMechRepresentation custGameRep { get { return this.GameRep as CustomMechRepresentation; } }
    public CustomMech(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
      : base(mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef) 
    {

    }
    public virtual void _InitGameRep(Transform parentTransform) {
      Log.TWL(0, "CustomMech._InitGameRep:"+ this.MechDef.Chassis.PrefabIdentifier);
      string prefabIdentifier = this.MechDef.Chassis.PrefabIdentifier;
      if (AbstractActor.initLogger.IsLogEnabled) { AbstractActor.initLogger.Log((object)("InitGameRep Loading this -" + prefabIdentifier)); }
      GameObject gameObject = this.Combat.DataManager.PooledInstantiate_CustomMechRep_Battle(prefabIdentifier, this.MechDef.Chassis, true, true, true);
      if (gameObject == null) {
        throw new Exception(prefabIdentifier + " fail to load. Chassis:"+this.MechDef.ChassisID);
      }
      MechRepresentation mechRep = gameObject.GetComponent<MechRepresentation>();
      if (mechRep == null) {
        throw new Exception(prefabIdentifier + " is not a mech prefab. Chassis:" + this.MechDef.ChassisID);
      }
      CustomMechRepresentation custMechRepLoc = gameObject.GetComponent<CustomMechRepresentation>();
      if (custMechRepLoc == null) {
        throw new Exception(prefabIdentifier + " CustomMech can only operate CustomMechRepresentation");
      }
      Log.WL(1, "current game representation:" + (mechRep == null ? "null" : mechRep.name));
      this._gameRep = (GameRepresentation)mechRep;
      this.custGameRep.Init(this, parentTransform, false);
      if(parentTransform == null) {
        this.custGameRep.gameObject.transform.position = this.currentPosition;
        this.custGameRep.gameObject.transform.rotation = this.currentRotation;
      }
      this.InitWeapons();
      bool flag1 = this.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
      bool flag2 = this.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
      if (flag1 | flag2) {
        SkinnedMeshRenderer[] componentsInChildren = this.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int index = 0; index < componentsInChildren.Length; ++index) {
          if (flag1)
            componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderUnfinishedMaterial;
          if (flag2)
            componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderImpostorMaterial;
        }
      }
      this.custGameRep.GatherColliders();
      //this.custGameRep.CustomPostInit();
      this.GameRep.RefreshEdgeCache();
      this.GameRep.FadeIn(1f);
      if (this.IsDead || !this.Combat.IsLoadingFromSave) { return; }
      if (this.AuraComponents != null) {
        foreach (MechComponent auraComponent in this.AuraComponents) {
          for (int index = 0; index < auraComponent.componentDef.statusEffects.Length; ++index) {
            if (auraComponent.componentDef.statusEffects[index].targetingData.auraEffectType == AuraEffectType.ECM_GHOST) {
              this.GameRep.PlayVFXAt(this.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
              this.GameRep.PlayVFXAt(this.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
              return;
            }
          }
        }
      }
      if (this.VFXDataFromLoad != null) {
        foreach (VFXEffect.StoredVFXEffectData storedVfxEffectData in this.VFXDataFromLoad)
          this.GameRep.PlayVFXAt(this.GameRep.GetVFXTransform(storedVfxEffectData.hitLocation), storedVfxEffectData.hitPos, storedVfxEffectData.vfxName, storedVfxEffectData.isAttached, storedVfxEffectData.lookatPos, storedVfxEffectData.isOneShot, storedVfxEffectData.duration);
      }
    }
    public virtual void InitWeapons() {
      List<ComponentRepresentationInfo> componentsToInit = new List<ComponentRepresentationInfo>();
      foreach (MechComponent allComponent in this.allComponents) {
        if (allComponent.componentType != ComponentType.Weapon) {
          componentsToInit.Add(new ComponentRepresentationInfo(allComponent, allComponent.mechComponentRef.MountedLocation, allComponent.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Component));
        }
      }
      foreach (Weapon weapon in this.Weapons) {
        componentsToInit.Add(new ComponentRepresentationInfo(weapon, weapon.mechComponentRef.MountedLocation, weapon.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Weapon));
      }
      foreach (MechComponent supportComponent in this.supportComponents) {
        if (supportComponent is Weapon weapon) {
          componentsToInit.Add(new ComponentRepresentationInfo(weapon, weapon.mechComponentRef.MountedLocation, weapon.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Support));
        }
      }
      this.custGameRep.InitWeapons(componentsToInit, this.LogDisplayName);
      if (!this.MeleeWeapon.baseComponentRef.hasPrefabName) {
        this.MeleeWeapon.baseComponentRef.prefabName = "chrPrfWeap_generic_melee";
        this.MeleeWeapon.baseComponentRef.hasPrefabName = true;
      }
      this.MeleeWeapon.InitGameRep(this.MeleeWeapon.baseComponentRef.prefabName, this.GetAttachTransform(this.MeleeWeapon.mechComponentRef.MountedLocation), this.LogDisplayName);
      if (!this.DFAWeapon.mechComponentRef.hasPrefabName) {
        this.DFAWeapon.mechComponentRef.prefabName = "chrPrfWeap_generic_melee";
        this.DFAWeapon.mechComponentRef.hasPrefabName = true;
      }
      this.DFAWeapon.InitGameRep(this.DFAWeapon.mechComponentRef.prefabName, this.GetAttachTransform(this.DFAWeapon.mechComponentRef.MountedLocation), this.LogDisplayName);
    }
    public override void InitGameRep(Transform parentTransform) {
      //this.MechInitGameRep_prefixes();
      this._InitGameRep(parentTransform);
      this.custGameRep.HeightController.ForceHeight(this.FlyingHeight());
      //this.MechInitGameRep_postfixes();
    }
    public override int GetHitLocation(AbstractActor attacker,Vector3 attackPosition,float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attackPosition, this));
      Thread.CurrentThread.pushActor(this);
      int result = (int)(hitTable != null ? HitLocation.GetHitLocation(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier) : ArmorLocation.None);
      Thread.CurrentThread.clearActor();
      return result;
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      List<int> result = this.Combat.HitLocation.GetPossibleHitLocations(attacker, this);
      if (this.CanBeHeadShot == false) { result.Remove((int)ArmorLocation.Head); }
      return result;
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition,float randomRoll,int previousHitLocation,float originalMultiplier = 1f,float adjacentMultiplier = 1f) {
      return (int)this.Combat.HitLocation.GetAdjacentHitLocation(attackPosition, this, randomRoll, (ArmorLocation)previousHitLocation, originalMultiplier, adjacentMultiplier, ArmorLocation.None, 0.0f);
    }

    public virtual HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }

    public virtual HashSet<ArmorLocation> GetLandmineDamageArmorLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }

    public virtual HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear,
          ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear, ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear,
          ArmorLocation.RightArm, ArmorLocation.LeftArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg
      };
    }

    public virtual Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      Thread.CurrentThread.pushActor(this);
      Thread.CurrentThread.SetFlag("CallOriginal_GetMechHitTable");
      Dictionary<ArmorLocation, int> result = new Dictionary<ArmorLocation, int>(this.Combat.HitLocation.GetMechHitTable(from));
      Thread.CurrentThread.ClearFlag("CallOriginal_GetMechHitTable");
      Thread.CurrentThread.clearActor();
      if (!this.CanBeHeadShot && result.ContainsKey(ArmorLocation.Head)) result.Remove(ArmorLocation.Head);
      return result;
    }

    public virtual Dictionary<int, float> GetAOESpreadArmorLocations() {
      return CustomAmmoCategories.NormMechHitLocations;
    }

    public virtual List<int> GetAOEPossibleHitLocations(Vector3 attackPos) {
      return this.Combat.HitLocation.GetPossibleHitLocations(attackPos, this);
    }
    public new virtual Text GetLongArmorLocation(ArmorLocation location) {
      return Mech.GetLongArmorLocation(location);
    }
    public virtual ArmorLocation GetAdjacentLocations(ArmorLocation location) {
      return MechStructureRules.GetAdjacentLocations(location);
    }
    private static Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> GetClusterTable_cache = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
    public virtual Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      if (GetClusterTable_cache.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      ArmorLocation adjacentLocations = this.GetAdjacentLocations(originalLocation);
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (keyValuePair.Key != ArmorLocation.Head || !this.Combat.Constants.ToHit.ClusterChanceNeverClusterHead || originalLocation == ArmorLocation.Head) {
          if (keyValuePair.Key == ArmorLocation.Head && this.Combat.Constants.ToHit.ClusterChanceNeverMultiplyHead)
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
          else if (originalLocation == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
          else if ((adjacentLocations & keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceNonadjacentMultiplier));
        }
      }
      GetClusterTable_cache.Add(originalLocation, dictionary);
      return dictionary;
    }
    private static Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterHitTable_cache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public virtual Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      if(GetClusterHitTable_cache.TryGetValue(from, out Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        GetClusterHitTable_cache.Add(from, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      return result;
    }
    public virtual bool isSquad { get { return false; } }
    public virtual bool isVehicle { get { return false; } }
  }
  public partial class CustomMechRepresentation : MechRepresentation, ICustomizationTarget {
    public HashSet<CustomParticleSystemRep> CustomParticleSystemReps { get; set; } = new HashSet<CustomParticleSystemRep>();
    public virtual GameObject _gameObject { get { return this.gameObject; } }
    public virtual GameObject _visibleObject { get { return this.VisibleObject; } }
    public virtual ChassisDef chassisDef { get; set; }
    public List<JumpjetRepresentation> jumpjetReps { get { return Traverse.Create(this).Field<List<JumpjetRepresentation>>("jumpjetReps").Value; } set { Traverse.Create(this).Field<List<JumpjetRepresentation>>("jumpjetReps").Value = value; } }
    public List<GameObject> headlightReps { get { return Traverse.Create(this).Field<List<GameObject>>("headlightReps").Value; } set { Traverse.Create(this).Field<List<GameObject>>("headlightReps").Value = value; } }
    public AnimatorTransitionInfo previousAnimTransition { get { return Traverse.Create(this).Field<AnimatorTransitionInfo>("previousAnimTransition").Value; } set { Traverse.Create(this).Field<AnimatorTransitionInfo>("previousAnimTransition").Value = value; } }
    public AnimatorStateInfo previousAnimState { get { return Traverse.Create(this).Field<AnimatorStateInfo>("previousAnimState").Value; } set { Traverse.Create(this).Field<AnimatorStateInfo>("previousAnimState").Value = value; } }
    public AnimatorTransitionInfo currentAnimTransition { get { return Traverse.Create(this).Field<AnimatorTransitionInfo>("currentAnimTransition").Value; } set { Traverse.Create(this).Field<AnimatorTransitionInfo>("currentAnimTransition").Value = value; } }
    public AnimatorStateInfo currentAnimState { get { return Traverse.Create(this).Field<AnimatorStateInfo>("currentAnimState").Value; } set { Traverse.Create(this).Field<AnimatorStateInfo>("currentAnimState").Value = value; } }
    public int currentAnimStateHash { get { return Traverse.Create(this).Field<int>("currentAnimStateHash").Value; } set { Traverse.Create(this).Field<int>("currentAnimStateHash").Value = value; } }
    public int previousTransitionHash { get { return Traverse.Create(this).Field<int>("previousTransitionHash").Value; } set { Traverse.Create(this).Field<int>("previousTransitionHash").Value = value; } }
    public int previousAnimStateHash { get { return Traverse.Create(this).Field<int>("previousAnimStateHash").Value; } set { Traverse.Create(this).Field<int>("previousAnimStateHash").Value = value; } }
    public bool isPlayingJumpSound { get { return Traverse.Create(this).Field<bool>("isPlayingJumpSound").Value; } set { Traverse.Create(this).Field<bool>("isPlayingJumpSound").Value = value; } }
    public bool isJumping { get { return Traverse.Create(this).Field<bool>("isJumping").Value; } set { Traverse.Create(this).Field<bool>("isJumping").Value = value; } }
    public float voDelay { get { return Traverse.Create(this).Field<float>("voDelay").Value; } set { Traverse.Create(this).Field<float>("voDelay").Value = value; } }
    public PropertyBlockManager.PropertySetting _heatAmount;
    public PropertyBlockManager.PropertySetting heatAmount { get { return Traverse.Create(this).Field<PropertyBlockManager.PropertySetting>("heatAmount").Value; } set { Traverse.Create(this).Field<PropertyBlockManager.PropertySetting>("heatAmount").Value = value; _heatAmount = value; } }
    public bool isFakeOverheated { get { return Traverse.Create(this).Field<bool>("isFakeOverheated").Value; } set { Traverse.Create(this).Field<bool>("isFakeOverheated").Value = value; } }
    public List<CustomMechMeshMerge> mechMerges = new List<CustomMechMeshMerge>();
    public bool triggerFootVFX { get { return Traverse.Create(this).Field<bool>("triggerFootVFX").Value; } set { Traverse.Create(this).Field<bool>("triggerFootVFX").Value = value; } }
    public int leftFootVFX { get { return Traverse.Create(this).Field<int>("leftFootVFX").Value; } set { Traverse.Create(this).Field<int>("leftFootVFX").Value = value; } }
    public List<string> persistentCritList { get { return Traverse.Create(this).Field<List<string>>("persistentCritList").Value; } set { Traverse.Create(this).Field<List<string>>("persistentCritList").Value = value; } }
    public float BLIP_MOVEMENT_THRESHOLD { get { return Traverse.Create(this).Field<float>("BLIP_MOVEMENT_THRESHOLD").Value; } set { Traverse.Create(this).Field<float>("BLIP_MOVEMENT_THRESHOLD").Value = value; } }
    public float BLIP_UPDATE_SECONDS { get { return Traverse.Create(this).Field<float>("BLIP_UPDATE_SECONDS").Value; } set { Traverse.Create(this).Field<float>("BLIP_UPDATE_SECONDS").Value = value; } }
    public float BLIP_PEAK_ALPHA { get { return Traverse.Create(this).Field<float>("BLIP_PEAK_ALPHA").Value; } set { Traverse.Create(this).Field<float>("BLIP_PEAK_ALPHA").Value = value; } }
    public float BLIP_ATTACK_TIME { get { return Traverse.Create(this).Field<float>("BLIP_ATTACK_TIME").Value; } set { Traverse.Create(this).Field<float>("BLIP_ATTACK_TIME").Value = value; } }
    public float BLIP_DECAY_TIME { get { return Traverse.Create(this).Field<float>("BLIP_DECAY_TIME").Value; } set { Traverse.Create(this).Field<float>("BLIP_DECAY_TIME").Value = value; } }
    public float BLIP_STATIC_ALPHA { get { return Traverse.Create(this).Field<float>("BLIP_STATIC_ALPHA").Value; } set { Traverse.Create(this).Field<float>("BLIP_STATIC_ALPHA").Value = value; } }
    public Vector3 blipPendingPosition { get { return Traverse.Create(this).Field<Vector3>("blipPendingPosition").Value; } set { Traverse.Create(this).Field<Vector3>("blipPendingPosition").Value = value; } }
    public Quaternion blipPendingRotation { get { return Traverse.Create(this).Field<Quaternion>("blipPendingRotation").Value; } set { Traverse.Create(this).Field<Quaternion>("blipPendingRotation").Value = value; } }
    public bool blipHasPendingPositionRotation { get { return Traverse.Create(this).Field<bool>("blipHasPendingPositionRotation").Value; } set { Traverse.Create(this).Field<bool>("blipHasPendingPositionRotation").Value = value; } }
    public float blipLastUpdateTime { get { return Traverse.Create(this).Field<float>("blipLastUpdateTime").Value; } set { Traverse.Create(this).Field<float>("blipLastUpdateTime").Value = value; } }
    public CapsuleCollider mainCollider { get { return Traverse.Create(this).Field<CapsuleCollider>("mainCollider").Value; } set { Traverse.Create(this).Field<CapsuleCollider>("mainCollider").Value = value; } }
    public bool paintSchemeInitialized { get { return Traverse.Create(this).Field<bool>("paintSchemeInitialized").Value; } set { Traverse.Create(this).Field<bool>("paintSchemeInitialized").Value = value; } }
    public PilotableActorRepresentation pooledPrefab { get { return Traverse.Create(this).Field<PilotableActorRepresentation>("pooledPrefab").Value; } set { Traverse.Create(this).Field<PilotableActorRepresentation>("pooledPrefab").Value = value; } }
    public Mech mech { get { return Traverse.Create(this).Field<Mech>("mech").Value; } set { Traverse.Create(this).Field<Mech>("mech").Value = value; } }
    public Vehicle vehicle { get { return Traverse.Create(this).Field<Vehicle>("vehicle").Value; } set { Traverse.Create(this).Field<Vehicle>("vehicle").Value = value; } }
    public Turret turret { get { return Traverse.Create(this).Field<Turret>("turret").Value; } set { Traverse.Create(this).Field<Turret>("turret").Value = value; } }
    public string prefabId { get { return Traverse.Create(this).Field<string>("prefabId").Value; } set { Traverse.Create(this).Field<string>("prefabId").Value = value; } }
    public GameObject testGO { get { return Traverse.Create(this).Field<GameObject>("testGO").Value; } set { Traverse.Create(this).Field<GameObject>("testGO").Value = value; } }
    public int framesCounted { get { return Traverse.Create(this).Field<int>("framesCounted").Value; } set { Traverse.Create(this).Field<int>("framesCounted").Value = value; } }
    public int framesToSkip { get { return Traverse.Create(this).Field<int>("framesToSkip").Value; } set { Traverse.Create(this).Field<int>("framesToSkip").Value = value; } }
    public List<Renderer> rendererList { get { return Traverse.Create(this).Field<List<Renderer>>("rendererList").Value; } set { Traverse.Create(this).Field<List<Renderer>>("rendererList").Value = value; } }
    public Renderer localRenderer { get { return Traverse.Create(this).Field<Renderer>("localRenderer").Value; } set { Traverse.Create(this).Field<Renderer>("localRenderer").Value = value; } }
    public Renderer pooledPrefabRenderer { get { return Traverse.Create(this).Field<Renderer>("pooledPrefabRenderer").Value; } set { Traverse.Create(this).Field<Renderer>("pooledPrefabRenderer").Value = value; } }
    public Material[] sharedMaterialsSource { get { return Traverse.Create(this).Field<Material[]>("sharedMaterialsSource").Value; } set { Traverse.Create(this).Field<Material[]>("sharedMaterialsSource").Value = value; } }
    public Material[] sharedMaterialsCopy { get { return Traverse.Create(this).Field<Material[]>("sharedMaterialsCopy").Value; } set { Traverse.Create(this).Field<Material[]>("sharedMaterialsCopy").Value = value; } }
    public Material defaultMaterial { get { return Traverse.Create(this).Field<Material>("defaultMaterial").Value; } set { Traverse.Create(this).Field<Material>("defaultMaterial").Value = value; } }
    public bool wasEvasiveLastFrame { get { return Traverse.Create(this).Field<bool>("wasEvasiveLastFrame").Value; } set { Traverse.Create(this).Field<bool>("wasEvasiveLastFrame").Value = value; } }
    public bool guardedLastFrame { get { return Traverse.Create(this).Field<bool>("guardedLastFrame").Value; } set { Traverse.Create(this).Field<bool>("guardedLastFrame").Value = value; } }
    public bool coverLastFrame { get { return Traverse.Create(this).Field<bool>("coverLastFrame").Value; } set { Traverse.Create(this).Field<bool>("coverLastFrame").Value = value; } }
    public bool wasUnsteadyLastFrame { get { return Traverse.Create(this).Field<bool>("wasUnsteadyLastFrame").Value; } set { Traverse.Create(this).Field<bool>("wasUnsteadyLastFrame").Value = value; } }
    public bool wasEntrenchedLastFrame { get { return Traverse.Create(this).Field<bool>("wasEntrenchedLastFrame").Value; } set { Traverse.Create(this).Field<bool>("wasEntrenchedLastFrame").Value = value; } }
    public float timeNow { get { return Traverse.Create(this).Field<float>("timeNow").Value; } set { Traverse.Create(this).Field<float>("timeNow").Value = value; } }
    public float elapsedTime { get { return Traverse.Create(this).Field<float>("elapsedTime").Value; } set { Traverse.Create(this).Field<float>("elapsedTime").Value = value; } }
    public float blipAlpha { get { return Traverse.Create(this).Field<float>("blipAlpha").Value; } set { Traverse.Create(this).Field<float>("blipAlpha").Value = value; } }
    public float timeFromEnd { get { return Traverse.Create(this).Field<float>("timeFromEnd").Value; } set { Traverse.Create(this).Field<float>("timeFromEnd").Value = value; } }
    public float baseWindIntensity { get { return Traverse.Create(this).Field<float>("baseWindIntensity").Value; } set { Traverse.Create(this).Field<float>("baseWindIntensity").Value = value; } }
    public PropertyBlockManager _propertyBlock { get { return Traverse.Create(this).Field<PropertyBlockManager>("_propertyBlock").Value; } set { Traverse.Create(this).Field<PropertyBlockManager>("_propertyBlock").Value = value; } }
    public CustomPropertyBlockManager customPropertyBlock { get { return _propertyBlock as CustomPropertyBlockManager; } }
    public float timePlacedOffScreen { get { return Traverse.Create(this).Field<float>("timePlacedOffScreen").Value; } set { Traverse.Create(this).Field<float>("timePlacedOffScreen").Value = value; } }
    public BattleTech.Rendering.MechCustomization.MechCustomization _mechCustomization { get { return Traverse.Create(this).Property<BattleTech.Rendering.MechCustomization.MechCustomization>("mechCustomization").Value; } set { Traverse.Create(this).Property<BattleTech.Rendering.MechCustomization.MechCustomization>("mechCustomization").Value = value; } }
    public CombatGameState _Combat { get { return Traverse.Create(this).Property<CombatGameState>("Combat").Value; } set { Traverse.Create(this).Property<CombatGameState>("Combat").Value = value; } }
    private readonly float timeLimitOffScreen = 20f;
    public CustomMechCustomization defaultMechCustomization { get; set; } = null;
    public List<CustomMechCustomization> mechCustomizations { get; set; } = new List<CustomMechCustomization>();
    public virtual bool _allowRandomIdles { get { return this.allowRandomIdles; }
      set {
        if (this.customRep != null) { this.customRep.StartRandomIdle = value; }
        this.allowRandomIdles = value;
      }
    }
    public void Copy(MechRepresentation source) {
      // ---------------------- GameRepresentation -----------------------
      this._parentCombatant = Traverse.Create(source).Field<ICombatant>("_parentCombatant").Value;
      this._parentActor = Traverse.Create(source).Field<AbstractActor>("_parentActor").Value;
      this.thisGameObject = Traverse.Create(source).Field<GameObject>("thisGameObject").Value;
      this.thisTransform = Traverse.Create(source).Field<Transform>("thisTransform").Value;
      this.thisCharacterController = Traverse.Create(source).Field<CharacterController>("thisCharacterController").Value;
      this.thisAnimator = Traverse.Create(source).Field<Animator>("thisAnimator").Value;
      this.thisIKController = Traverse.Create(source).Field<InverseKinematic>("thisIKController").Value;
      this.audioObject = Traverse.Create(source).Field<AkGameObj>("audioObject").Value;
      this.pilotRep = Traverse.Create(source).Field<PilotRepresentation>("pilotRep").Value;
      this.parentTransform = Traverse.Create(source).Field<Transform>("parentTransform").Value;
      this.renderers = Traverse.Create(source).Field<List<Renderer>>("renderers").Value;
      this.vfxTransforms = Traverse.Create(source).Field<Transform[]>("vfxTransforms").Value;
      this._allRaycastColliders = Traverse.Create(source).Field<Collider[]>("_allRaycastColliders").Value;
      this.persistentVFXParticles = Traverse.Create(source).Field<Dictionary<string, List<ParticleSystem>>>("persistentVFXParticles").Value;
      this.persistentDmgList = Traverse.Create(source).Field<List<string>>("persistentDmgList").Value;
      this._propertyBlock = Traverse.Create(source).Field<PropertyBlockManager>("_propertyBlock").Value;
      this.timePlacedOffScreen = Traverse.Create(source).Field<float>("timePlacedOffScreen").Value;
      this.windZone = Traverse.Create(source).Field<WindZone>("windZone").Value;
      this.baseWindIntensity = Traverse.Create(source).Field<float>("baseWindIntensity").Value;
      this.edgeHighlight = Traverse.Create(source).Property<MechEdgeSelection>("edgeHighlight").Value;
      this.currentHighlight = Traverse.Create(source).Property<GameRepresentation.HighlightType>("currentHighlight").Value;
      this._mechCustomization = Traverse.Create(source).Property<BattleTech.Rendering.MechCustomization.MechCustomization>("mechCustomization").Value;
      this._isTargetable = Traverse.Create(source).Property<bool>("isTargetable").Value;
      this._isTargeted = Traverse.Create(source).Property<bool>("isTargeted").Value;
      this._isAvailable = Traverse.Create(source).Property<bool>("isAvailable").Value;
      this._isSelected = Traverse.Create(source).Property<bool>("isSelected").Value;
      this._isHovered = Traverse.Create(source).Property<bool>("isHovered").Value;
      this._isDead = Traverse.Create(source).Property<bool>("isDead").Value;
      // ---------------------- PilotableActorRepresentation -----------------------
      this.BlipObjectUnknown = source.BlipObjectUnknown;
      this.BlipObjectIdentified = source.BlipObjectIdentified;
      this.BlipObjectGhostWeak = source.BlipObjectGhostWeak;
      this.BlipObjectGhostStrong = source.BlipObjectGhostStrong;
      this.VisibleObject = source.VisibleObject;
      this.VisibleObjectLight = source.VisibleObjectLight;
      this.VisibleLights = source.VisibleLights;
      this.AuraReticle = source.AuraReticle;
      this.blipPendingPosition = Traverse.Create(source).Field<Vector3>("blipPendingPosition").Value;
      this.blipPendingRotation = Traverse.Create(source).Field<Quaternion>("blipPendingRotation").Value;
      this.blipHasPendingPositionRotation = Traverse.Create(source).Field<bool>("blipHasPendingPositionRotation").Value;
      this.blipLastUpdateTime = Traverse.Create(source).Field<float>("blipLastUpdateTime").Value;
      this.VFXCollider = source.VFXCollider;
      this.currentSurfaceType = Traverse.Create(source).Field<AudioSwitch_surface_type>("currentSurfaceType").Value;
      this.terrainImpactParticleName = Traverse.Create(source).Field<string>("terrainImpactParticleName").Value;
      this.twistTransform = source.twistTransform;
      this.currentTwistAngle = source.currentTwistAngle;
      this.miscComponentReps = source.miscComponentReps;
      this.weaponReps = source.weaponReps;
      this.mainCollider = Traverse.Create(source).Field<CapsuleCollider>("mainCollider").Value;
      this.paintSchemeInitialized = Traverse.Create(source).Field<bool>("paintSchemeInitialized").Value;
      this.pooledPrefab = Traverse.Create(source).Field<PilotableActorRepresentation>("pooledPrefab").Value;
      this.mech = Traverse.Create(source).Field<Mech>("mech").Value;
      this.vehicle = Traverse.Create(source).Field<Vehicle>("vehicle").Value;
      this.turret = Traverse.Create(source).Field<Turret>("turret").Value;
      this.prefabId = Traverse.Create(source).Field<string>("prefabId").Value;
      this.testGO = Traverse.Create(source).Field<GameObject>("testGO").Value;
      this.framesCounted = Traverse.Create(source).Field<int>("framesCounted").Value;
      this.framesToSkip = Traverse.Create(source).Field<int>("framesToSkip").Value;
      this.rendererList = Traverse.Create(source).Field<List<Renderer>>("rendererList").Value;
      this.localRenderer = Traverse.Create(source).Field<Renderer>("localRenderer").Value;
      this.pooledPrefabRenderer = Traverse.Create(source).Field<Renderer>("pooledPrefabRenderer").Value;
      this.sharedMaterialsSource = Traverse.Create(source).Field<Material[]>("sharedMaterialsSource").Value;
      this.sharedMaterialsCopy = Traverse.Create(source).Field<Material[]>("sharedMaterialsCopy").Value;
      this.defaultMaterial = Traverse.Create(source).Field<Material>("defaultMaterial").Value;
      this.wasEvasiveLastFrame = Traverse.Create(source).Field<bool>("wasEvasiveLastFrame").Value;
      this.guardedLastFrame = Traverse.Create(source).Field<bool>("guardedLastFrame").Value;
      this.coverLastFrame = Traverse.Create(source).Field<bool>("coverLastFrame").Value;
      this.wasUnsteadyLastFrame = Traverse.Create(source).Field<bool>("wasUnsteadyLastFrame").Value;
      this.wasEntrenchedLastFrame = Traverse.Create(source).Field<bool>("wasEntrenchedLastFrame").Value;
      this.forcedPlayerVisibilityLevel = source.forcedPlayerVisibilityLevel;
      this.vfxNameModifier = Traverse.Create(source).Field<string>("vfxNameModifier").Value;
      this.timeNow = Traverse.Create(source).Field<float>("timeNow").Value;
      this.elapsedTime = Traverse.Create(source).Field<float>("elapsedTime").Value;
      this.blipAlpha = Traverse.Create(source).Field<float>("blipAlpha").Value;
      this.timeFromEnd = Traverse.Create(source).Field<float>("timeFromEnd").Value;
      // ---------------------- MechRepresentation -----------------------
      this.LeftArmAttach = source.LeftArmAttach;
      this.RightArmAttach = source.RightArmAttach;
      this.TorsoAttach = source.TorsoAttach;
      this.LeftLegAttach = source.LeftLegAttach;
      this.RightLegAttach = source.RightLegAttach;
      this.vfxCenterTorsoTransform = source.vfxCenterTorsoTransform;
      this.vfxLeftTorsoTransform = source.vfxLeftTorsoTransform;
      this.vfxRightTorsoTransform = source.vfxRightTorsoTransform;
      this.vfxHeadTransform = source.vfxHeadTransform;
      this.vfxLeftArmTransform = source.vfxLeftArmTransform;
      this.vfxRightArmTransform = source.vfxRightArmTransform;
      this.vfxLeftLegTransform = source.vfxLeftLegTransform;
      this.vfxRightLegTransform = source.vfxRightLegTransform;
      this.vfxLeftShoulderTransform = source.vfxLeftShoulderTransform;
      this.vfxRightShoulderTransform = source.vfxRightShoulderTransform;
      this.ikLeftFootLimb = source.ikLeftFootLimb;
      this.ikLeftFootTarget = source.ikLeftFootTarget;
      this.ikRightFootLimb = source.ikRightFootLimb;
      this.ikRightFootTarget = source.ikRightFootTarget;
      this.useBirdFoot = source.useBirdFoot;
      this.leftFootTransform = source.leftFootTransform;
      this.rightFootTransform = source.rightFootTransform;
      this.jumpjetReps = Traverse.Create(source).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
      this.headlightReps = Traverse.Create(source).Field<List<GameObject>>("headlightReps").Value;
      this.headDestructible = source.headDestructible;
      this.centerTorsoDestructible = source.centerTorsoDestructible;
      this.leftTorsoDestructible = source.leftTorsoDestructible;
      this.rightTorsoDestructible = source.rightTorsoDestructible;
      this.leftArmDestructible = source.leftArmDestructible;
      this.rightArmDestructible = source.rightArmDestructible;
      this.leftLegDestructible = source.leftLegDestructible;
      this.rightLegDestructible = source.rightLegDestructible;
      this.idleStateEntryHash = Traverse.Create(source).Field<int>("idleStateEntryHash").Value;
      this.idleStateFlavorsHash = Traverse.Create(source).Field<int>("idleStateFlavorsHash").Value;
      this.idleStateUnsteadyHash = Traverse.Create(source).Field<int>("idleStateUnsteadyHash").Value;
      this.idleStateMeleeBaseHash = Traverse.Create(source).Field<int>("idleStateMeleeBaseHash").Value;
      this.idleStateMeleeEntryHash = Traverse.Create(source).Field<int>("idleStateMeleeEntryHash").Value;
      this.idleStateMeleeHash = Traverse.Create(source).Field<int>("idleStateMeleeHash").Value;
      this.idleStateMeleeUnsteadyHash = Traverse.Create(source).Field<int>("idleStateMeleeUnsteadyHash").Value;
      this.TEMPIdleStateMeleeIdleHash = Traverse.Create(source).Field<int>("TEMPIdleStateMeleeIdleHash").Value;
      this.idleRandomValueHash = Traverse.Create(source).Field<int>("idleRandomValueHash").Value;
      this.standingHash = Traverse.Create(source).Field<int>("standingHash").Value;
      this.groundDeathIdleHash = Traverse.Create(source).Field<int>("groundDeathIdleHash").Value;
      this.randomDeathIdleA = Traverse.Create(source).Field<int>("randomDeathIdleA").Value;
      this.randomDeathIdleB = Traverse.Create(source).Field<int>("randomDeathIdleB").Value;
      this.randomDeathIdleC = Traverse.Create(source).Field<int>("randomDeathIdleC").Value;
      this.randomDeathIdleBase = Traverse.Create(source).Field<int>("randomDeathIdleBase").Value;
      this.randomDeathIdleRandomizer = Traverse.Create(source).Field<int>("randomDeathIdleRandomizer").Value;
      this.hitReactLightHash = Traverse.Create(source).Field<int>("hitReactLightHash").Value;
      this.hitReactHeavyHash = Traverse.Create(source).Field<int>("hitReactHeavyHash").Value;
      this.hitReactMeleeHash = Traverse.Create(source).Field<int>("hitReactMeleeHash").Value;
      this.hitReactDodgeHash = Traverse.Create(source).Field<int>("hitReactDodgeHash").Value;
      this.hitReactDFAHash = Traverse.Create(source).Field<int>("hitReactDFAHash").Value;
      this._allowRandomIdles = true;
      this.previousAnimTransition = Traverse.Create(source).Field<AnimatorTransitionInfo>("previousAnimTransition").Value;
      this.previousAnimState = Traverse.Create(source).Field<AnimatorStateInfo>("previousAnimState").Value;
      this.currentAnimTransition = Traverse.Create(source).Field<AnimatorTransitionInfo>("currentAnimTransition").Value;
      this.currentAnimState = Traverse.Create(source).Field<AnimatorStateInfo>("currentAnimState").Value;
      this.currentAnimStateHash = Traverse.Create(source).Field<int>("currentAnimStateHash").Value;
      this.previousTransitionHash = Traverse.Create(source).Field<int>("previousTransitionHash").Value;
      this.previousAnimStateHash = Traverse.Create(source).Field<int>("previousAnimStateHash").Value;
      this.IsOnLimpingLeg = source.IsOnLimpingLeg;
      this.HasCalledOutLimping = source.HasCalledOutLimping;
      this.isPlayingJumpSound = Traverse.Create(source).Field<bool>("isPlayingJumpSound").Value;
      this.isJumping = Traverse.Create(source).Field<bool>("isJumping").Value;
      //this.voDelay = Traverse.Create(source).Field<float>("voDelay").Value;
      this.heatAmount = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("heatAmount").Value;
      this.isFakeOverheated = Traverse.Create(source).Field<bool>("isFakeOverheated").Value;
      this.needsToRefreshCombinedMesh = source.needsToRefreshCombinedMesh;
      //this.mechMerge = null;
      this.triggerFootVFX = Traverse.Create(source).Field<bool>("triggerFootVFX").Value;
      this.leftFootVFX = Traverse.Create(source).Field<int>("leftFootVFX").Value;
      this.persistentCritList = Traverse.Create(source).Field<List<string>>("persistentCritList").Value;
    }
    public bool SetupFallbackTransforms() {
      bool flag = false;
      if ((UnityEngine.Object)this.twistTransform == (UnityEngine.Object)null) {
        flag = true;
        this.twistTransform = this.findRecursive(this.transform, "j_Spine2");
        if ((UnityEngine.Object)this.twistTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup twistTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.LeftArmAttach == (UnityEngine.Object)null) {
        flag = true;
        this.LeftArmAttach = this.findRecursive(this.transform, "j_LForearm");
        if ((UnityEngine.Object)this.LeftArmAttach == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftArmAttach for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.RightArmAttach == (UnityEngine.Object)null) {
        flag = true;
        this.RightArmAttach = this.findRecursive(this.transform, "j_RForearm");
        if ((UnityEngine.Object)this.RightArmAttach == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightArmAttach for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.TorsoAttach == (UnityEngine.Object)null) {
        flag = true;
        this.TorsoAttach = this.findRecursive(this.transform, "j_Spine2");
        if ((UnityEngine.Object)this.TorsoAttach == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup TorsoAttach for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.LeftLegAttach == (UnityEngine.Object)null) {
        flag = true;
        this.LeftLegAttach = this.findRecursive(this.transform, "j_LCalf");
        if ((UnityEngine.Object)this.LeftLegAttach == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftLegAttach for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.leftFootTransform == (UnityEngine.Object)null) {
        flag = true;
        this.leftFootTransform = this.findRecursive(this.transform, "j_LHeel");
        if ((UnityEngine.Object)this.leftFootTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftFootTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.RightLegAttach == (UnityEngine.Object)null) {
        flag = true;
        this.RightLegAttach = this.findRecursive(this.transform, "j_RCalf");
        if ((UnityEngine.Object)this.RightLegAttach == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightLegAttach for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.rightFootTransform == (UnityEngine.Object)null) {
        flag = true;
        this.rightFootTransform = this.findRecursive(this.transform, "j_RHeel");
        if ((UnityEngine.Object)this.rightFootTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightFootTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxCenterTorsoTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxCenterTorsoTransform = this.twistTransform;
        if ((UnityEngine.Object)this.vfxCenterTorsoTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxCenterTorsoTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxLeftArmTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if ((UnityEngine.Object)this.vfxLeftArmTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxRightArmTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if ((UnityEngine.Object)this.vfxRightArmTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxHeadTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxHeadTransform = this.twistTransform;
        if ((UnityEngine.Object)this.vfxHeadTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxHeadTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxLeftArmTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if ((UnityEngine.Object)this.vfxLeftArmTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxRightArmTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if ((UnityEngine.Object)this.vfxRightArmTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxLeftLegTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxLeftLegTransform = this.LeftLegAttach;
        if ((UnityEngine.Object)this.vfxLeftLegTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftLegTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.vfxRightLegTransform == (UnityEngine.Object)null) {
        flag = true;
        this.vfxRightLegTransform = this.RightLegAttach;
        if ((UnityEngine.Object)this.vfxRightLegTransform == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightLegTransform for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.BlipObjectUnknown == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectUnknown");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.BlipObjectUnknown = recursive.gameObject;
        if ((UnityEngine.Object)this.BlipObjectUnknown == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Unknown Blip for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.BlipObjectIdentified == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectIdentified");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.BlipObjectIdentified = recursive.gameObject;
        if ((UnityEngine.Object)this.BlipObjectIdentified == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Identified Blip for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.BlipObjectGhostWeak == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostWeak");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.BlipObjectGhostWeak = recursive.gameObject;
        if ((UnityEngine.Object)this.BlipObjectGhostWeak == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Weak Ghost Blip for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.BlipObjectGhostStrong == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostStrong");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.BlipObjectGhostStrong = recursive.gameObject;
        if ((UnityEngine.Object)this.BlipObjectGhostStrong == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Strong Ghost Blip for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.headDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "Head_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.headDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.headDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup headDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.centerTorsoDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "torso_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.centerTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.centerTorsoDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup centerTorsoDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.leftTorsoDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "lefttorso_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.leftTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.leftTorsoDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftTorsoDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.rightTorsoDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "righttorso_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.rightTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.rightTorsoDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightTorsoDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.leftArmDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "LArm_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.leftArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.leftArmDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftArmDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.rightArmDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "RArm_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.rightArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.rightArmDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightArmDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.leftLegDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "LLeg_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.leftLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.leftLegDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftLegDestructible for mech " + this.parentMech.DisplayName));
      }
      if ((UnityEngine.Object)this.rightLegDestructible == (UnityEngine.Object)null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "RLeg_whole");
        if ((UnityEngine.Object)recursive != (UnityEngine.Object)null)
          this.rightLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((UnityEngine.Object)this.rightLegDestructible == (UnityEngine.Object)null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightLegDestructible for mech " + this.parentMech.DisplayName));
      }
      return flag;
    }
    public virtual void SetupHeadlights() {
      if (this.HasOwnVisuals == false) { return; }
      //return;
      //if (this.altDef != null) { if (string.IsNullOrEmpty(this.altDef.PrefabBase) == false) { prefabBase = this.altDef.PrefabBase; } }
      string id1 = string.Format("chrPrfComp_{0}_centertorso_headlight", (object)this.PrefabBase);
      string id2 = string.Format("chrPrfComp_{0}_leftshoulder_headlight", (object)this.PrefabBase);
      string id3 = string.Format("chrPrfComp_{0}_rightshoulder_headlight", (object)this.PrefabBase);
      string id4 = string.Format("chrPrfComp_{0}_leftleg_headlight", (object)this.PrefabBase);
      string id5 = string.Format("chrPrfComp_{0}_rightleg_headlight", (object)this.PrefabBase);
      this.headlightReps.Clear();
      GameObject gameObject1 = mech.Combat.DataManager.PooledInstantiate(id1, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject1 != (UnityEngine.Object)null) {
        gameObject1.transform.parent = this.vfxHeadTransform;
        gameObject1.transform.localPosition = Vector3.zero;
        gameObject1.transform.localRotation = Quaternion.identity;
        gameObject1.transform.localScale = Vector3.one;
        this.headlightReps.Add(gameObject1);
      }
      GameObject gameObject2 = mech.Combat.DataManager.PooledInstantiate(id2, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject2 != (UnityEngine.Object)null) {
        gameObject2.transform.parent = this.vfxLeftShoulderTransform;
        gameObject2.transform.localPosition = Vector3.zero;
        gameObject2.transform.localRotation = Quaternion.identity;
        gameObject2.transform.localScale = Vector3.one;
        this.headlightReps.Add(gameObject2);
      }
      GameObject gameObject3 = mech.Combat.DataManager.PooledInstantiate(id3, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject3 != (UnityEngine.Object)null) {
        gameObject3.transform.parent = this.vfxRightShoulderTransform;
        gameObject3.transform.localPosition = Vector3.zero;
        gameObject3.transform.localRotation = Quaternion.identity;
        gameObject3.transform.localScale = Vector3.one;
        this.headlightReps.Add(gameObject3);
      }
      GameObject gameObject4 = mech.Combat.DataManager.PooledInstantiate(id4, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject4 != (UnityEngine.Object)null) {
        gameObject4.transform.parent = this.vfxLeftLegTransform;
        gameObject4.transform.localPosition = Vector3.zero;
        gameObject4.transform.localRotation = Quaternion.identity;
        gameObject4.transform.localScale = Vector3.one;
        this.headlightReps.Add(gameObject4);
      }
      GameObject gameObject5 = mech.Combat.DataManager.PooledInstantiate(id5, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject5 != (UnityEngine.Object)null) {
        gameObject5.transform.parent = this.vfxRightLegTransform;
        gameObject5.transform.localPosition = Vector3.zero;
        gameObject5.transform.localRotation = Quaternion.identity;
        gameObject5.transform.localScale = Vector3.one;
        this.headlightReps.Add(gameObject5);
      }
      if (this.customRep != null) { this.customRep.AttachHeadlights(); }
    }
  }
  public partial class CustomMechRepresentation : MechRepresentation {
    public List<GameObject> VisualObjects { get; set; } = new List<GameObject>();
    public CustomMechRepresentation parentRepresentation { get; set; } = null;
    public CustomRepresentation customRep { get; set; } = null;
    public List<CustomMechRepresentation> slaveRepresentations { get; set; } = new List<CustomMechRepresentation>();
    public CustomMechRepresentation rootParentRepresentation {
      get {
        CustomMechRepresentation result = this;
        while (result.parentRepresentation != null) { result = result.parentRepresentation; }
        return result;
      }
    }
    public delegate void d_Update(PilotableActorRepresentation rep);
    private static d_Update i_Update = null;
    public bool isSlave { get; set; } = false;
    //public bool isRoot { get; set; }
    public virtual Transform j_Root { get; protected set; }
    public virtual CustomMech custMech { get; protected set; }
    public virtual HardpointDataDef HardpointData { get; set; }
    public virtual string PrefabBase { get; set; }
    private static void Prepare() {
      {
        MethodInfo method = typeof(PilotableActorRepresentation).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("PilotableActorRepresentationUpdate", null, new Type[] { typeof(PilotableActorRepresentation) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_Update = (d_Update)dm.CreateDelegate(typeof(d_Update));
      }
    }
    //public CustomRepresentation customRepresentation { get; set; }
    public virtual void _Init(Mech mech, Transform parentTransform, bool isParented) {
      Log.TWL(0,this.GetType().ToString()+"._Init "+mech.MechDef.ChassisID);
      this.j_Root = this.transform.FindRecursive("j_Root");
      this.gameObject.GetComponent<Animator>().enabled = true;
      Animator[] animators = this.gameObject.GetComponentsInChildren<Animator>(true);
      foreach (Animator animator in animators) { animator.enabled = true; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
      this._Init((AbstractActor)mech, parentTransform, isParented);
      this.custMech = this.parentCombatant as CustomMech;
      this.mech = this.parentCombatant as Mech;
      this.vehicle = this.parentCombatant as Vehicle;
      this.turret = this.parentCombatant as Turret;
      typeof(MechRepresentation).GetProperty("Constants").GetSetMethod(true).Invoke(this,new object[] { mech.Combat.Constants } );
      bool flag = false;
      if (this.altDef == null) { this.altDef = new AlternateRepresentationDef(mech.MechDef.Chassis); }
      if (this.f_presistantAudioStart.Count == 0) {
        if(this.altDef != null) {
          if(string.IsNullOrEmpty(this.altDef.HoveringSoundStart) == false) {
            this.f_presistantAudioStart.Add(this.altDef.HoveringSoundStart);
          }
        }
        if(this.customRep != null) {
          if(this.customRep.CustomDefinition != null) {
            if (string.IsNullOrEmpty(this.customRep.CustomDefinition.persistentAudioStart) == false) {
              this.f_presistantAudioStart.Add(this.customRep.CustomDefinition.persistentAudioStart);
            }
          }
        }
        if (this.f_presistantAudioStart.Count == 0) {
          UnitCustomInfo info = this.parentMech.GetCustomInfo();
          string defaultEventId = "AudioEventList_mech_mech_engine_start";
          if (info != null) {
            if (info.FakeVehicle) {
              switch (info.FakeVehicleMovementType) {
                case VehicleMovementType.Wheeled: defaultEventId = "AudioEventList_vehicle_vehicle_gallant_engine_start"; break;
                case VehicleMovementType.Tracked: defaultEventId = "AudioEventList_vehicle_vehicle_tank_engine_start"; break;
                case VehicleMovementType.Hovered: defaultEventId = "AudioEventList_vehicle_vehicle_striker_engine_start"; break;
              }
            }
          }
          f_presistantAudioStart.Add(defaultEventId);
        }
      }
      if (this.f_presistantAudioStop.Count == 0) {
        if (this.altDef != null) {
          if (string.IsNullOrEmpty(this.altDef.HoveringSoundEnd) == false) {
            this.f_presistantAudioStop.Add(this.altDef.HoveringSoundEnd);
          }
        }
        if (this.customRep != null) {
          if (this.customRep.CustomDefinition != null) {
            if (string.IsNullOrEmpty(this.customRep.CustomDefinition.persistentAudioStop) == false) {
              this.f_presistantAudioStop.Add(this.customRep.CustomDefinition.persistentAudioStop);
            }
          }
        }
        if (f_presistantAudioStop.Count == 0) { 
          UnitCustomInfo info = this.parentMech.GetCustomInfo();
          string defaultEventId = "AudioEventList_mech_mech_engine_stop";
          if (info != null) {
            if (info.FakeVehicle) {
              switch (info.FakeVehicleMovementType) {
                case VehicleMovementType.Wheeled: defaultEventId = "AudioEventList_vehicle_vehicle_gallant_engine_stop"; break;
                case VehicleMovementType.Tracked: defaultEventId = "AudioEventList_vehicle_vehicle_tank_engine_stop"; break;
                case VehicleMovementType.Hovered: defaultEventId = "AudioEventList_vehicle_vehicle_striker_engine_stop"; break;
              }
            }
          }
          f_presistantAudioStop.Add(defaultEventId);
        }
      }
      if(f_moveAudioStart.Count == 0) {
        if(this.altDef != null) {
          if (string.IsNullOrEmpty(this.altDef.MoveStartSound) == false) {
            this.f_moveAudioStart.Add(this.altDef.MoveStartSound);
          }
        }
        if (this.f_moveAudioStart.Count == 0) {
          UnitCustomInfo info = this.parentMech.GetCustomInfo();
          string defaultEventId = "AudioEventList_vehicle_vehicle_tank_start";
          if (info != null) {
            switch (info.FakeVehicleMovementType) {
              case VehicleMovementType.Wheeled: defaultEventId = "AudioEventList_vehicle_vehicle_gallant_start"; break;
              case VehicleMovementType.Tracked: defaultEventId = "AudioEventList_vehicle_vehicle_tank_start"; break;
              case VehicleMovementType.Hovered: defaultEventId = "AudioEventList_vehicle_vehicle_striker_start"; break;
            }
          }
          f_moveAudioStart.Add(defaultEventId);
        }
      }
      if (f_moveAudioStop.Count == 0) {
        if (this.altDef != null) {
          if (string.IsNullOrEmpty(this.altDef.MoveStopSound) == false) {
            this.f_moveAudioStop.Add(this.altDef.MoveStopSound);
          }
        }
        if (this.f_moveAudioStop.Count == 0) {
          UnitCustomInfo info = this.parentMech.GetCustomInfo();
          string defaultEventId = "AudioEventList_vehicle_vehicle_tank_stop";
          if (info != null) {
            switch (info.FakeVehicleMovementType) {
              case VehicleMovementType.Wheeled: defaultEventId = "AudioEventList_vehicle_vehicle_gallant_stop"; break;
              case VehicleMovementType.Tracked: defaultEventId = "AudioEventList_vehicle_vehicle_tank_stop"; break;
              case VehicleMovementType.Hovered: defaultEventId = "AudioEventList_vehicle_vehicle_striker_stop"; break;
            }
          }
          f_moveAudioStop.Add(defaultEventId);
        }
      }
      this.idleStateEntryHash = Animator.StringToHash("Base Layer.Idle.Empty");
      this.idleStateFlavorsHash = Animator.StringToHash("Base Layer.Idle.Flavors");
      this.idleStateUnsteadyHash = Animator.StringToHash("Base Layer.Idle.Unsteady");
      this.idleStateMeleeBaseHash = Animator.StringToHash("Base Layer.Idle.Melee Idle");
      this.idleStateMeleeEntryHash = Animator.StringToHash("Base Layer.Idle.Melee Idle.Empty");
      this.idleStateMeleeHash = Animator.StringToHash("Base Layer.Idle.Melee Idle.Melee Idle");
      this.idleStateMeleeUnsteadyHash = Animator.StringToHash("Base Layer.Idle.Melee Idle.Melee Unsteady");
      this.TEMPIdleStateMeleeIdleHash = Animator.StringToHash("Base Layer.Idle.Melee Idle");
      this.idleRandomValueHash = Animator.StringToHash("IdleRandom");
      this.standingHash = Animator.StringToHash("Base Layer.Downed.Getup");
      this.groundDeathIdleHash = Animator.StringToHash("Base Layer.Death.OnGround Death Idle");
      this.randomDeathIdleA = Animator.StringToHash("Base Layer.Death.RandomDeath.Death 1");
      this.randomDeathIdleB = Animator.StringToHash("Base Layer.Death.RandomDeath.Death 2");
      this.randomDeathIdleC = Animator.StringToHash("Base Layer.Death.RandomDeath.Death 3");
      this.randomDeathIdleBase = Animator.StringToHash("Base Layer.Death.RandomDeath");
      this.randomDeathIdleRandomizer = Animator.StringToHash("Base Layer.Death.RandomDeath.Randomize");
      this.hitReactLightHash = Animator.StringToHash("Base Layer.Hit React.Hit React Light");
      this.hitReactHeavyHash = Animator.StringToHash("Base Layer.Hit React.Hit React Stagger");
      this.hitReactMeleeHash = Animator.StringToHash("Base Layer.Hit React.Hit React Melee");
      this.hitReactDodgeHash = Animator.StringToHash("Base Layer.Hit React.Melee Dodge");
      this.hitReactDFAHash = Animator.StringToHash("Base Layer.Hit React.DFA");
      this._allowRandomIdles = true;
      this._SetIdleAnimState();
      this.thisAnimator.Play(this.idleStateFlavorsHash, 0, UnityEngine.Random.Range(0.0f, 1f));
      this.InitAnimations();
      this.IsOnLimpingLeg = false;
      this.HasCalledOutLimping = false;
      this.isJumping = false;
      this.isPlayingJumpSound = false;
      this.persistentCritList = new List<string>((IEnumerable<string>)this.Constants.VFXNames.persistentCritNames);
      flag = this.SetupFallbackTransforms();
      //this.mechMerges = new List<CustomMechMeshMerge>();
      if ((UnityEngine.Object)this.VisibleObject == (UnityEngine.Object)null)
        Debug.LogError((object)("================= ERROR! Mech " + this.parentMech.DisplayName + " has no visible object!!! FIX THIS ======================"));
      else {
        if (VisualObjects.Count == 0) {
          CustomMechMeshMerge merge = this.VisibleObject.GetComponent<CustomMechMeshMerge>();
          if (merge != null) { GameObject.DestroyImmediate(merge); merge = null; }
          merge = VisibleObject.AddComponent<CustomMechMeshMerge>();
          merge.Init(this,this.VisibleObject ,this.GetComponentInParent<UICreep>(), this.GetComponent<PropertyBlockManager>());
          this.mechMerges.Add(merge);
        }
      }
      if (flag)
        Debug.LogError((object)("================== ERROR! Found missing transforms in mech " + this.parentMech.DisplayName + "; auto-settings are going to look wrong! FIX THIS ============"));
      if ((UnityEngine.Object)this.audioObject == (UnityEngine.Object)null)
        Debug.LogError((object)("================= ERROR! Mech " + this.parentMech.DisplayName + " has no audio object!!! FIX THIS ======================"));
      if ((UnityEngine.Object)this.audioObject != (UnityEngine.Object)null) {
        AudioSwitch_mech_weight_type switchEnumValue = AudioSwitch_mech_weight_type.b_medium;
        switch (this.parentMech.MechDef.Chassis.weightClass) {
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
        WwiseManager.SetSwitch<AudioSwitch_mech_weight_type>(switchEnumValue, this.audioObject);
        if (this.useBirdFoot)
          WwiseManager.SetSwitch<AudioSwitch_mech_foot_type>(AudioSwitch_mech_foot_type.bird, this.audioObject);
        else
          WwiseManager.SetSwitch<AudioSwitch_mech_foot_type>(AudioSwitch_mech_foot_type.human, this.audioObject);
        WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_heeltoe_delay, 100f - mech.tonnage, this.audioObject);
        WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, 0.0f, this.audioObject);
        WwiseManager.SetSwitch<AudioSwitch_mech_engine_damaged_mildly_yesno>(AudioSwitch_mech_engine_damaged_mildly_yesno.damaged_mildly_no, this.audioObject);
        WwiseManager.SetSwitch<AudioSwitch_mech_engine_damaged_badly_yesno>(AudioSwitch_mech_engine_damaged_badly_yesno.damaged_badly_no, this.audioObject);
      }
      int count = ((IEnumerable<MechComponentRef>)mech.MechDef.Inventory).Where<MechComponentRef>((Func<MechComponentRef, bool>)(x => x.ComponentDefType == ComponentType.JumpJet)).Count<MechComponentRef>();
      if (count > 0) {
        this.SetupJumpJets();
        mech.Combat.DataManager.PrecachePrefab("JumpjetCurves", BattleTechResourceType.Prefab, count);
      }
      if (this.FHeightController == null) { this.FHeightController = this.gameObject.AddComponent<MechFlyHeightController>(); }
      this.FHeightController.InitVisuals(this, mech.Combat.DataManager);
      this.SetupHeadlights();
      this.RefreshSurfaceType(true);
      this._InitHighlighting();
      this._InitWindZone();
      this.InitSkeleton();
      mech.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.OnHeatChanged, new ReceiveMessageCenterMessage(this.OnHeatChanged));
      this.heatAmount = new PropertyBlockManager.PropertySetting("_Heat", 0.0f);
      this.propertyBlock.AddProperty(ref this._heatAmount);
      this.gameObject.AddComponent<ShadowTracker>();
      this._SetupDamageStates(mech, mech.MechDef);
      this.InitSlaves();
      foreach (CustomMechMeshMerge merge in this.mechMerges) { merge.RefreshCombinedMesh(true); }
    }
    public virtual Dictionary<Transform, HashSet<Transform>> skeleton { get; set; } = null;
    public virtual void InitSkeletonBone(Transform bone) {
      if(skeleton == null) skeleton = new Dictionary<Transform, HashSet<Transform>>();
      if (bone == null) { return; }
      if(skeleton.TryGetValue(bone, out HashSet<Transform> skelet) == false) {
        skelet = new HashSet<Transform>();
        skeleton.Add(bone, skelet);
      }
      foreach(Transform subbone in bone) { skelet.Add(subbone); }
    }
    public virtual void InitSkeleton() {
      if (this.vfxCenterTorsoTransform != null) { this.InitSkeletonBone(this.vfxCenterTorsoTransform); }
      if (this.vfxLeftTorsoTransform != null) { this.InitSkeletonBone(this.vfxLeftTorsoTransform); }
      if (this.vfxRightTorsoTransform != null) { this.InitSkeletonBone(this.vfxRightTorsoTransform); }
      if (this.vfxHeadTransform != null) { this.InitSkeletonBone(this.vfxHeadTransform); }
      if (this.vfxLeftArmTransform != null) { this.InitSkeletonBone(this.vfxLeftArmTransform); }
      if (this.vfxRightArmTransform != null) { this.InitSkeletonBone(this.vfxRightArmTransform); }
      if (this.vfxLeftLegTransform != null) { this.InitSkeletonBone(this.vfxLeftLegTransform); }
      if (this.vfxRightLegTransform != null) { this.InitSkeletonBone(this.vfxRightLegTransform); }
      if (this.vfxLeftShoulderTransform != null) { this.InitSkeletonBone(this.vfxLeftShoulderTransform); }
      if (this.vfxRightShoulderTransform != null) { this.InitSkeletonBone(this.vfxRightShoulderTransform); }
    }
    protected virtual void InitSlaves() {
      foreach (CustomMechRepresentation slave in this.slaveRepresentations) { slave.Init(this.custMech,this.j_Root,true); }
    }
    public void InitCustomParticles(GameObject source, CustomActorRepresentationDef custRepDef) {
      try {
        Log.TWL(0, "CustomMechRepresentation.InitCustomParticles " + source.transform.name);
        Dictionary<string, CustomParticleSystemDef> definitions = new Dictionary<string, CustomParticleSystemDef>();
        foreach (CustomParticleSystemDef customParticleSystemDef in custRepDef.Particles) {
          definitions.Add(customParticleSystemDef.object_name, customParticleSystemDef);
        }
        ParticleSystem[] particles = source.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles) {
          Log.W(1, ps.transform.name);
          if (definitions.TryGetValue(ps.transform.name, out CustomParticleSystemDef def)) {
            Log.WL(1, ":" + def.Location);
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, def.Location));
           } else {
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, ChassisLocations.None));
            Log.WL(1, ":" + ChassisLocations.None);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public void StopCustomParticlesInLocation(ChassisLocations location) {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        if (psRep.location == location) { psRep.ps.Stop(true); }
      }
    }
    public void StopCustomParticles() {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        psRep.ps.Stop(true);
      }
    }
    public void StartCustomParticles() {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        psRep.ps.Play(true);
      }
    }
    public void ShowCustomParticles(VisibilityLevel newLevel) {
      //return;
      //if(newLevel == VisibilityLevel.LOSFull) {
      try {
        Log.TWL(0, "CustomRepresentation.ShowCustomParticles " + this.name+":"+ newLevel);
        foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
          Log.WL(1, psRep.ps.gameObject.name);
          psRep.ps.gameObject.SetActive(newLevel == VisibilityLevel.LOSFull);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      //}
    }

    public virtual void Init(CustomMech mech, Transform parentTransform, bool isParented) {
      Log.TWL(0, "CustomMechRepresentation.Init "+mech.Description.Id);
      this.customRep = this.GetComponent<CustomRepresentation>();
      this._Init(mech, parentTransform, isParented);
      mech.sizeMultiplier();
    }
    public virtual string GetComponentMountingPointPrefabName(HardpointDataDef hardpointData, string prefabName, ChassisLocations MountedLocation) {
      string[] strArray = prefabName.Split('_');
      string str = strArray[strArray.Length - 1];
      string lower = MountedLocation.ToString().ToLower();
      for (int index1 = 0; index1 < hardpointData.HardpointData.Length; ++index1) {
        if (hardpointData.HardpointData[index1].location == lower) {
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].mountingPoints.Length; ++index2) {
            string mountingPoint = hardpointData.HardpointData[index1].mountingPoints[index2];
            if (mountingPoint.EndsWith(str))
              return mountingPoint;
          }
        }
      }
      return "";
    }
    public List<string> GetComponentBlankNames(List<string> usedPrefabNames, HardpointDataDef hardpointData, ChassisLocations location) {
      List<string> stringList1 = new List<string>();
      string lower = location.ToString().ToLower();
      List<string> stringList2 = new List<string>();
      for (int index1 = 0; index1 < hardpointData.HardpointData.Length; ++index1) {
        if (hardpointData.HardpointData[index1].location == lower || location == ChassisLocations.All) {
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].weapons.Length; ++index2) {
            List<string> stringList3 = new List<string>();
            bool flag = false;
            for (int index3 = 0; index3 < hardpointData.HardpointData[index1].weapons[index2].Length; ++index3) {
              string str = hardpointData.HardpointData[index1].weapons[index2][index3];
              stringList3.Add(str);
              if (usedPrefabNames.Contains(str))
                flag = true;
            }
            if (flag)
              stringList2.AddRange((IEnumerable<string>)stringList3);
          }
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].blanks.Length; ++index2) {
            string blank = hardpointData.HardpointData[index1].blanks[index2];
            stringList1.Add(blank);
          }
        }
      }
      if (location == ChassisLocations.All || stringList1.Count < 1)
        return stringList1;
      foreach (string str in stringList2) {
        char[] chArray = new char[1] { '_' };
        string[] strArray = str.Split(chArray);
        string prefabLocation = strArray[2];
        string prefabHardpoint = strArray[strArray.Length - 1];
        stringList1.RemoveAll((Predicate<string>)(x => x.Contains(prefabLocation) && x.EndsWith(prefabHardpoint)));
      }
      return stringList1;
    }
    public virtual void CreateBlankPrefabs(List<string> usedPrefabNames, HardpointDataDef hardpointData, ChassisLocations location, string parentDisplayName) {
      List<string> componentBlankNames = this.GetComponentBlankNames(usedPrefabNames, hardpointData, location);
      Transform attachTransform = this.GetAttachTransform(location);
      for (int index = 0; index < componentBlankNames.Count; ++index) {
        GameObject blankGO = this._Combat.DataManager.PooledInstantiate(componentBlankNames[index], BattleTechResourceType.Prefab);
        if (blankGO != null) {
          this.RegisterRenderersMainHeraldry(blankGO);
          WeaponRepresentation component = blankGO.GetComponent<WeaponRepresentation>();
          if (component == null) { component = blankGO.AddComponent<WeaponRepresentation>(); }
          component.Init(this.parentCombatant, attachTransform, true, parentDisplayName, (int)location);
          this.weaponReps.Add(component);
        }
      }
    }
    public virtual Transform GetAttachTransform(ChassisLocations location) {
      Transform result = null;
      if (result == null) {
        switch (location) {
          case ChassisLocations.LeftArm: result = this.LeftArmAttach; break;
          case ChassisLocations.RightArm: result = this.RightArmAttach; break;
          case ChassisLocations.LeftLeg: result = this.LeftLegAttach; break;
          case ChassisLocations.RightLeg: result = this.RightLegAttach; break;
          default: result = this.TorsoAttach; break;
        }
      }
      return result;
    }
    public virtual void InitWeapons(List<ComponentRepresentationInfo> compInfo, string parentDisplayName) {
      Log.TWL(0, "CustomMechRepresentation.InitWeapons "+this.mech.MechDef.ChassisID+ " HardpointData:"+this.HardpointData.ID);
      List<HardpointCalculator.Element> calcData = new List<HardpointCalculator.Element>();
      foreach (ComponentRepresentationInfo info in compInfo) { calcData.Add(new HardpointCalculator.Element() { location = info.prefabLocation, componentRef = info.component.baseComponentRef }); }
      HardpointCalculator calculator = new HardpointCalculator();
      calculator.Init(calcData, this.HardpointData);
      List<string> usedPrefabNames = calculator.usedPrefabs.ToList();
      foreach (var cmp in compInfo) {
        if (cmp.repType != ComponentRepresentationInfo.RepType.Component) { continue; }
        if (cmp.component.componentType != ComponentType.Weapon) {
          //cmp.component.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.HardpointData, cmp.component.baseComponentRef, this.PrefabBase, cmp.prefabLocation.ToString().ToLower(), ref usedPrefabNames);
          cmp.component.baseComponentRef.prefabName = calculator.GetComponentPrefabName(cmp.component.baseComponentRef);
          cmp.component.baseComponentRef.hasPrefabName = true;
          Log.WL(1, cmp.component.defId+":"+ cmp.component.baseComponentRef.prefabName);
          if (string.IsNullOrEmpty(cmp.component.baseComponentRef.prefabName) == false) {
            //CustomHardpointDef hpDef = CustomHardPointsHelper.Find(cmp.component.baseComponentRef.prefabName);
            Transform attachTransform = this.GetAttachTransform(cmp.attachLocation);
            cmp.component.InitGameRep(cmp.component.baseComponentRef.prefabName, attachTransform, parentDisplayName);
            this.miscComponentReps.Add(cmp.component.componentRep);
            if (cmp.component.componentRep != null) { this.RegisterRenderersComponentRepresentation(cmp.component.componentRep); }
          }
        }
      }
      foreach (var cmp in compInfo) {
        if (cmp.repType != ComponentRepresentationInfo.RepType.Weapon) { continue; }
        if (cmp.component.componentType != ComponentType.Weapon) { continue; }
        Weapon weapon = cmp.component as Weapon;
        if (weapon == null) { continue; }
        //weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.HardpointData, weapon.baseComponentRef, this.PrefabBase, cmp.prefabLocation.ToString().ToLower(), ref usedPrefabNames);
        weapon.baseComponentRef.prefabName = calculator.GetComponentPrefabName(weapon.baseComponentRef);
        weapon.baseComponentRef.hasPrefabName = true;
        Log.WL(0, cmp.component.defId + ":" + weapon.baseComponentRef.prefabName);
        if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
          //CustomHardpointDef hpDef = CustomHardPointsHelper.Find(cmp.component.baseComponentRef.prefabName);
          Transform attachTransform = this.GetAttachTransform(cmp.attachLocation);
          weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, parentDisplayName);
          this.weaponReps.Add(weapon.weaponRep);
          if (weapon.weaponRep != null) { this.RegisterRenderersComponentRepresentation(weapon.weaponRep); }
          string originalPrefabName = calculator.GetComponentPrefabNameNoAlias(weapon.baseComponentRef);
          if (string.IsNullOrEmpty(originalPrefabName) == false) {
            string mountingPointPrefabName = this.GetComponentMountingPointPrefabName(this.HardpointData, originalPrefabName, cmp.attachLocation);
            Log.WL(1, cmp.component.defId + " mount point:" + mountingPointPrefabName);
            if (string.IsNullOrEmpty(mountingPointPrefabName) == false) {
              GameObject mountPointGO = this._Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab);
              if (mountPointGO != null) {
                this.RegisterRenderersMainHeraldry(mountPointGO);
                Log.WL(2, "game object:" + mountPointGO.name);
                WeaponRepresentation component = mountPointGO.GetComponent<WeaponRepresentation>();
                if (component == null) { component = mountPointGO.AddComponent<WeaponRepresentation>(); }
                component.Init(this.parentCombatant, attachTransform, true, parentDisplayName, weapon.Location);
                this.weaponReps.Add(component);
              }
            }
          }
        }
      }
      foreach (var cmp in compInfo) {
        if (cmp.repType != ComponentRepresentationInfo.RepType.Support) { continue; }
        Weapon weapon = cmp.component as Weapon;
        if (weapon == null) { continue; }
        //weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.HardpointData, weapon.baseComponentRef, this.PrefabBase, cmp.prefabLocation.ToString().ToLower(), ref usedPrefabNames);
        weapon.baseComponentRef.prefabName = calculator.GetComponentPrefabName(weapon.baseComponentRef);
        weapon.baseComponentRef.hasPrefabName = true;
        if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
          CustomHardpointDef hpDef = CustomHardPointsHelper.Find(cmp.component.baseComponentRef.prefabName);
          Transform attachTransform = this.GetAttachTransform(cmp.attachLocation);
          weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, parentDisplayName);
          this.miscComponentReps.Add((ComponentRepresentation)weapon.weaponRep);
          if (weapon.weaponRep != null) { this.RegisterRenderersComponentRepresentation(weapon.weaponRep); }
        }
      }
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.CenterTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.LeftTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.RightTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.LeftArm, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.RightArm, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.Head, parentDisplayName);
      if (this.customRep != null) { this.customRep.AttachWeapons(); }
      this.propertyBlock.UpdateCache();
    }
  }
}