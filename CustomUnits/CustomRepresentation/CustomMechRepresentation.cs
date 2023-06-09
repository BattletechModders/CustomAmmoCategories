/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Rendering;
using HarmonyLib;
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
using IRBTModUtils;

namespace CustomUnits {
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("ContractInitialize")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class UnitSpawnPointGameLogic_ContractInitialize {
    public static void Postfix(UnitSpawnPointGameLogic __instance) {
      try {
        Log.Combat?.TW(0, "UnitSpawnPointGameLogic.ContractInitialize " + __instance.UnitDefId + ":" + __instance.unitType);
        if (__instance.unitType == UnitType.Vehicle) { __instance.mechDefId = __instance.vehicleDefId; __instance.vehicleDefId = string.Empty; __instance.unitType = UnitType.Mech; }
        Log.Combat?.WL(0, "->" + __instance.UnitDefId + ":" + __instance.unitType);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UnitSpawnPointGameLogic.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ActorFactory))]
  [HarmonyPatch("CreateMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(TagSet), typeof(CombatGameState), typeof(string), typeof(string), typeof(HeraldryDef) })]
  public static class ActorFactory_CreateMech {
    public static void Prefix(ref bool __runOriginal, MechDef mDef, PilotDef pilot, TagSet additionalTags, CombatGameState combat, string uniqueId, string spawnerId, HeraldryDef customHeraldryDef, ref Mech __result) {
      try {
        Log.Combat?.TWL(0, "ActorFactory.CreateMech " + mDef.Description.Id);
        UnitCustomInfo info = mDef.Chassis.GetCustomInfo();
        if (info != null) {
          if (info.SquadInfo.Troopers >= 1) {
            __result = new TrooperSquad(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
          }else
          if((info.FakeVehicle == true) || (mDef.ChassisID.IsInFakeChassis() == true)) {
            __result = new FakeVehicleMech(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
          }else if(info.ArmsCountedAsLegs == true) {
            __result = new QuadMech(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
          }
        }
        if (__result == null) {
          __result = new CustomMech(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
        }
        combat.ItemRegistry.AddItem((ITaggedItem)__result);
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.initLogger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("initializeActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Team), typeof(Lance) })]
  public static class UnitSpawnPointGameLogic_initializeActor {
    public static void Prefix(ref bool __runOriginal, UnitSpawnPointGameLogic __instance, AbstractActor actor, Team team, Lance lance) {
      string temp = "!NOT INITED!";
      if (!__runOriginal) { return; }
      try {
        Log.Combat?.TWL(0, "UnitSpawnPointGameLogic.initializeActor " + actor.PilotableActorDef.Description.Id);
        temp = actor.PilotableActorDef.Description.Id;
        Vector3 spawnPosition = Traverse.Create(__instance).Method("GetSpawnPosition").GetValue<Vector3>(); //__instance.GetSpawnPosition();
        __instance.LogMessage(spawnPosition.ToString());
        actor.Init(spawnPosition, __instance.facing, true);
        actor.InitGameRep((Transform)null);
        team.AddUnit(actor);
        lance.AddUnitGUID(actor.GUID);
        actor.AddToLance(lance);
        if (DeployManualHelper.IsInManualSpawnSequence) {
          actor.OnPlayerVisibilityChanged(VisibilityLevel.None);
        }else
        if (__instance.Combat.HostilityMatrix.IsLocalPlayerFriendly(team) || __instance.Combat.LocalPlayerTeam.VisibilityToTarget((ICombatant)actor) != VisibilityLevel.None) {
          //actor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
        } else {
          actor.OnPlayerVisibilityChanged(VisibilityLevel.None);
        }
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, "UnitSpawnPointGameLogic.initializeActor:"+temp);
        Log.Combat?.TWL(0, e.ToString(), true);
        UnitSpawnPointGameLogic.logger.LogException(e);
        return;
      }
    }
    public static void Postfix(UnitSpawnPointGameLogic __instance, AbstractActor actor, Team team, Lance lance) {
      try {
        if (DeployManualHelper.deployDirector != null) { return; }
        if (__instance.Combat.TurnDirector.CurrentRound <= 1) { return; }
        if (__instance.Combat.IsSpawnProtected()) { return; }
        if (team == __instance.Combat.LocalPlayerTeam) { return; }
        if (Core.Settings.OnUnitSpawnProtection == false) { return; }
        actor.addSpawnProtection("Middle battle spawn protection");
      }catch(Exception e) {
        UnitSpawnPointGameLogic.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("SpawnMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(Team), typeof(Lance), typeof(HeraldryDef) })]
  public static class UnitSpawnPointGameLogic_SpawnMechAlign {
    public static void Postfix(UnitSpawnPointGameLogic __instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef, ref Mech __result) {
      try {
        Log.Combat?.TWL(0, "UnitSpawnPointGameLogic.SpawnMech " + __result.PilotableActorDef.Description.Id);
        CustomMechRepresentation custRep = __result.GameRep as CustomMechRepresentation;
        if (custRep != null) {
          custRep.UpdateRotation(custRep.createMoveContext(),custRep.transform, custRep.transform.forward, 9999f);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.initLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(ActorFactory))]
  [HarmonyPatch("CreateVehicle")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VehicleDef), typeof(PilotDef), typeof(TagSet), typeof(CombatGameState), typeof(string), typeof(string), typeof(HeraldryDef) })]
  public static class ActorFactory_CreateVehicle {
    public static void Postfix(VehicleDef vDef,PilotDef pilot,TagSet additionalTags,CombatGameState combat,string uniqueId,string spawnerId,HeraldryDef customHeraldryDef, ref Vehicle __result) {
      try {
        Log.Combat?.TWL(0, "ActorFactory.CreateVehicle " + vDef.Description.Id+" result:"+(__result == null?"null": __result.GUID));
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.initLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechDestructibleObject))]
  [HarmonyPatch("Collapse")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float) })]
  public static class MechDestructibleObject_Collapse {
    public static void GatherRigidbodies(this MechDestructibleObject __instance, ref Rigidbody[] destroyedRigidbodies) {
      if (__instance.destroyedObj != null) {
        destroyedRigidbodies = __instance.destroyedObj.GetComponentsInChildren<Rigidbody>();
      } else
        destroyedRigidbodies = new Rigidbody[0];
    }
    public static void Prefix(ref bool __runOriginal, MechDestructibleObject __instance, Vector3 forceDirection, float forceMagnitude) {
      try {
        Log.Combat?.TWL(0, "MechDestructibleObject.Collapse " + __instance.gameObject.name);
        if (__instance.destroyedObj == null) { return; }
        Log.Combat?.WL(1, "destroyedObj " + __instance.destroyedObj.name);
        Rigidbody[] bodies = __instance.destroyedObj.GetComponentsInChildren<Rigidbody>(true);
        foreach(Rigidbody body in bodies) {
          Log.Combat?.WL(2, "Rigidbody " + body.name);
          SkinnedMeshRenderer rnd_body = body.gameObject.GetComponent<SkinnedMeshRenderer>();
          if (rnd_body == null) { continue; }
          if (rnd_body.rootBone == null) { continue; }
          if (rnd_body.rootBone == rnd_body.gameObject.transform) { continue; }
          Transform prevRoot = rnd_body.rootBone;
          rnd_body.rootBone = rnd_body.gameObject.transform;
          rnd_body.gameObject.transform.position = prevRoot.position;
          rnd_body.gameObject.transform.rotation = prevRoot.rotation;
          rnd_body.gameObject.transform.localScale = prevRoot.lossyScale;
          Log.Combat?.WL(2, "SkinnedMeshRenderer detected. scale:" + rnd_body.gameObject.transform.localScale);
        }
        if (__instance.wholeObj != null) { __instance.wholeObj.SetActive(false); }
        if (__instance.destroyedObj == null) { return; }
        __instance.destroyedObj.SetActive(true);
        if (__instance.destroyedRigidbodies == null) { __instance.GatherRigidbodies(ref __instance.destroyedRigidbodies); }
        for (int index = 0; index < __instance.destroyedRigidbodies.Length; ++index) {
          Rigidbody destroyedRigidbody = __instance.destroyedRigidbodies[index];
          Collider component = destroyedRigidbody.gameObject.GetComponent<Collider>();
          Vector3 position = destroyedRigidbody.gameObject.transform.position;
          Quaternion rotation = destroyedRigidbody.gameObject.transform.rotation;
          SkinnedMeshRenderer rnd_body = destroyedRigidbody.gameObject.GetComponent<SkinnedMeshRenderer>();
          Vector3 scale = destroyedRigidbody.gameObject.transform.lossyScale;
          if(rnd_body != null) {
            if(rnd_body.rootBone == rnd_body.transform) {
              scale = rnd_body.gameObject.transform.localScale;
            }
          }
          //destroyedRigidbody.gameObject.transform.lossyScale;
          Log.Combat?.WL(2, "Destroyed rig body:" + destroyedRigidbody.name+" scale:"+ scale);
          destroyedRigidbody.gameObject.transform.parent = (Transform)null;
          destroyedRigidbody.gameObject.transform.position = position;
          destroyedRigidbody.gameObject.transform.rotation = rotation;
          destroyedRigidbody.gameObject.transform.localScale = scale;
          if (!__instance.CheckCollisionWithTerrain(component)) {
            if (!destroyedRigidbody.isKinematic) {
              destroyedRigidbody.gameObject.layer = LayerMask.NameToLayer("VFXPhysicsActive");
              float num = Mathf.Pow(9f, (float)Mathf.FloorToInt(Mathf.Log10(destroyedRigidbody.mass)));
              destroyedRigidbody.AddExplosionForce(forceMagnitude * num, __instance.destroyedObj.transform.position - forceDirection * 3f, 0.0f, 10f, ForceMode.Impulse);
            }
          } else
            Debug.LogError((object)(destroyedRigidbody.name + " failed collapse "));
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        Debug.LogException(e);
      }
      __runOriginal = false;
      return;
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
  public partial class CustomMechRepresentation : MechRepresentation, ICustomizationTarget {
    public HashSet<CustomParticleSystemRep> CustomParticleSystemReps { get; set; } = new HashSet<CustomParticleSystemRep>();
    public virtual GameObject _gameObject { get { return this.gameObject; } }
    public virtual GameObject _visibleObject { get { return this.VisibleObject; } }
    public virtual ChassisDef chassisDef { get; set; }
    public List<JumpjetRepresentation> jumpjetReps { get { return this.jumpjetReps(); } set { this.jumpjetReps(value); } }
    public List<GameObject> headlightReps { get { return this.headlightReps(); } set { this.headlightReps(value); } }
    public AnimatorTransitionInfo previousAnimTransition { get { return this.previousAnimTransition(); } set { this.previousAnimTransition(value); } }
    public AnimatorStateInfo previousAnimState { get { return this.previousAnimState(); } set { this.previousAnimState(value); } }
    public AnimatorTransitionInfo currentAnimTransition { get { return this.currentAnimTransition(); } set { this.currentAnimTransition(value); } }
    public AnimatorStateInfo currentAnimState { get { return this.currentAnimState(); } set { this.currentAnimState(value); } }
    public int currentAnimStateHash { get { return this.currentAnimStateHash(); } set { this.currentAnimStateHash(value); } }
    public int previousTransitionHash { get { return this.previousTransitionHash(); } set { this.previousTransitionHash(value); } }
    public int previousAnimStateHash { get { return this.previousAnimStateHash(); } set { this.previousAnimStateHash(value); } }
    public bool isPlayingJumpSound { get { return this.isPlayingJumpSound(); } set { this.isPlayingJumpSound(value); } }
    public bool isJumping { get { return this.isJumping(); } set { this.isJumping(value); } }
    //public float voDelay { get { return Traverse.Create(this).Field<float>("voDelay").Value; } set { Traverse.Create(this).Field<float>("voDelay").Value = value; } }
    private PropertyBlockManager.PropertySetting _heatAmount;
    public PropertyBlockManager.PropertySetting heatAmount { get { return this.heatAmount(); } set { this.heatAmount(value); } }
    public bool isFakeOverheated { get { return this.isFakeOverheated(); } set { this.isFakeOverheated(value); } }
    public List<CustomMechMeshMerge> mechMerges = new List<CustomMechMeshMerge>();
    public bool triggerFootVFX { get { return this.triggerFootVFX(); } set { this.triggerFootVFX(value); } }
    public int leftFootVFX { get { return this.leftFootVFX(); } set { this.leftFootVFX(value); } }
    public List<string> persistentCritList { get { return this.persistentCritList(); } set { this.persistentCritList(value); } }
    //public float BLIP_MOVEMENT_THRESHOLD { get { return Traverse.Create(this).Field<float>("BLIP_MOVEMENT_THRESHOLD").Value; } set { Traverse.Create(this).Field<float>("BLIP_MOVEMENT_THRESHOLD").Value = value; } }
    //public float BLIP_UPDATE_SECONDS { get { return Traverse.Create(this).Field<float>("BLIP_UPDATE_SECONDS").Value; } set { Traverse.Create(this).Field<float>("BLIP_UPDATE_SECONDS").Value = value; } }
    //public float BLIP_PEAK_ALPHA { get { return Traverse.Create(this).Field<float>("BLIP_PEAK_ALPHA").Value; } set { Traverse.Create(this).Field<float>("BLIP_PEAK_ALPHA").Value = value; } }
    //public float BLIP_ATTACK_TIME { get { return Traverse.Create(this).Field<float>("BLIP_ATTACK_TIME").Value; } set { Traverse.Create(this).Field<float>("BLIP_ATTACK_TIME").Value = value; } }
    //public float BLIP_DECAY_TIME { get { return Traverse.Create(this).Field<float>("BLIP_DECAY_TIME").Value; } set { Traverse.Create(this).Field<float>("BLIP_DECAY_TIME").Value = value; } }
    //public float BLIP_STATIC_ALPHA { get { return Traverse.Create(this).Field<float>("BLIP_STATIC_ALPHA").Value; } set { Traverse.Create(this).Field<float>("BLIP_STATIC_ALPHA").Value = value; } }
    public Vector3 blipPendingPosition { get { return this.blipPendingPosition(); } set { this.blipPendingPosition(value); } }
    public Quaternion blipPendingRotation { get { return this.blipPendingRotation(); } set { this.blipPendingRotation(value); } }
    public bool blipHasPendingPositionRotation { get { return this.blipHasPendingPositionRotation(); } set { this.blipHasPendingPositionRotation(value); } }
    public float blipLastUpdateTime { get { return this.blipLastUpdateTime(); } set { this.blipLastUpdateTime(value); } }
    public CapsuleCollider mainCollider { get { return this.mainCollider(); } set { this.mainCollider(value); } }
    public bool paintSchemeInitialized { get { return this.paintSchemeInitialized(); } set { this.paintSchemeInitialized(value); } }
    public PilotableActorRepresentation pooledPrefab { get { return this.pooledPrefab(); } set { this.pooledPrefab(value); } }
    public Mech mech { get { return this.mech(); } set { this.mech(value); } }
    public Vehicle vehicle { get { return this.vehicle(); } set { this.vehicle(value); } }
    public Turret turret { get { return this.turret(); } set { this.turret(value); } }
    public string prefabId { get { return this.prefabId(); } set { this.prefabId(value); } }
    public GameObject testGO { get { return this.testGO(); } set { this.testGO(value); } }
    public int framesCounted { get { return this.framesCounted(); } set { this.framesCounted(value); } }
    public int framesToSkip { get { return this.framesToSkip(); } set { this.framesToSkip(value); } }
    public List<Renderer> rendererList { get { return this.rendererList(); } set { this.rendererList(value); } }
    public Renderer localRenderer { get { return this.localRenderer(); } set { this.localRenderer(value); } }
    public Renderer pooledPrefabRenderer { get { return this.pooledPrefabRenderer(); } set { this.pooledPrefabRenderer(value); } }
    public Material[] sharedMaterialsSource { get { return this.sharedMaterialsSource(); } set { this.sharedMaterialsSource(value); } }
    public Material[] sharedMaterialsCopy { get { return this.sharedMaterialsCopy(); } set { this.sharedMaterialsCopy(value); } }
    public Material defaultMaterial { get { return this.defaultMaterial(); } set { this.defaultMaterial(value); } }
    public bool wasEvasiveLastFrame { get { return this.wasEvasiveLastFrame(); } set { this.wasEvasiveLastFrame(value); } }
    public bool guardedLastFrame { get { return this.guardedLastFrame(); } set { this.guardedLastFrame(value); } }
    public bool coverLastFrame { get { return this.coverLastFrame(); } set { this.coverLastFrame(value); } }
    public bool wasUnsteadyLastFrame { get { return this.wasUnsteadyLastFrame(); } set { this.wasUnsteadyLastFrame(value); } }
    public bool wasEntrenchedLastFrame { get { return this.wasEntrenchedLastFrame(); } set { this.wasEntrenchedLastFrame(value); } }
    public float timeNow { get { return this.timeNow(); } set { this.timeNow(value); } }
    public float elapsedTime { get { return this.elapsedTime(); } set { this.elapsedTime(value); } }
    public float blipAlpha { get { return this.blipAlpha(); } set { this.blipAlpha(value); } }
    public float timeFromEnd { get { return this.timeFromEnd(); } set { this.timeFromEnd(value); } }
    public CustomPropertyBlockManager customPropertyBlock { get { return _propertyBlock as CustomPropertyBlockManager; } }
    public virtual BattleTech.Rendering.MechCustomization.MechCustomization mechCustomization { get { return this.mechCustomization(); } set { this.mechCustomization(value); } }
    public CombatGameState _Combat { get { return this.Combat; } set { this.Combat = value; } }
    public CustomMechCustomization defaultMechCustomization { get; set; } = null;
    public List<CustomMechCustomization> mechCustomizations { get; set; } = new List<CustomMechCustomization>();
    public virtual void CreateHeightController() {
      MechFlyHeightController heightController = this.gameObject.GetComponent<MechFlyHeightController>();
      if (heightController == null) { heightController = this.gameObject.AddComponent<MechFlyHeightController>(); };
      this.FHeightController = heightController;
    }
    public virtual bool _allowRandomIdles { get { return this.allowRandomIdles; }
      set {
        if (this.customRep != null) { this.customRep.StartRandomIdle = value; }
        this.allowRandomIdles = value;
      }
    }
    public void Copy(MechRepresentation source) {
      // ---------------------- GameRepresentation -----------------------
      this._parentCombatant = source._parentCombatant;
      this._parentActor = source._parentActor;
      this.thisGameObject = source.thisGameObject;
      this.thisTransform = source.thisTransform;
      this.thisCharacterController = source.thisCharacterController;
      this.thisAnimator = source.thisAnimator;
      this.thisIKController = source.thisIKController;
      this.audioObject = source.audioObject;
      this.pilotRep = source.pilotRep;
      this.parentTransform = source.parentTransform;
      this.renderers = source.renderers;
      this.vfxTransforms = source.vfxTransforms;
      this._allRaycastColliders = source._allRaycastColliders;
      this.persistentVFXParticles = source.persistentVFXParticles;
      this.persistentDmgList = source.persistentDmgList;
      this._propertyBlock = source._propertyBlock;
      this.timePlacedOffScreen = source.timePlacedOffScreen;
      this.windZone = source.windZone;
      this.baseWindIntensity = source.baseWindIntensity;
      this.edgeHighlight = source.edgeHighlight;
      this.currentHighlight = source.currentHighlight;
      this.mechCustomization = source.mechCustomization();
      this.isTargetable = source.isTargetable;
      this.isTargeted = source.isTargeted;
      this.isAvailable = source.isAvailable;
      this.isSelected = source.isSelected;
      this.isHovered = source.isHovered;
      this.isDead = source.isDead;
      // ---------------------- PilotableActorRepresentation -----------------------
      this.BlipObjectUnknown = source.BlipObjectUnknown;
      this.BlipObjectIdentified = source.BlipObjectIdentified;
      this.BlipObjectGhostWeak = source.BlipObjectGhostWeak;
      this.BlipObjectGhostStrong = source.BlipObjectGhostStrong;
      this.VisibleObject = source.VisibleObject;
      this.VisibleObjectLight = source.VisibleObjectLight;
      this.VisibleLights = source.VisibleLights;
      this.AuraReticle = source.AuraReticle;
      this.blipPendingPosition = source.blipPendingPosition();
      this.blipPendingRotation = source.blipPendingRotation();
      this.blipHasPendingPositionRotation = source.blipHasPendingPositionRotation();
      this.blipLastUpdateTime = source.blipLastUpdateTime();
      this.VFXCollider = source.VFXCollider;
      this.currentSurfaceType = source.currentSurfaceType();
      this.terrainImpactParticleName = source.terrainImpactParticleName();
      this.twistTransform = source.twistTransform;
      this.currentTwistAngle = source.currentTwistAngle;
      this.miscComponentReps = source.miscComponentReps;
      this.weaponReps = source.weaponReps;
      this.mainCollider = source.mainCollider();
      this.paintSchemeInitialized = source.paintSchemeInitialized();
      this.pooledPrefab = source.pooledPrefab();
      this.mech = source.mech();
      this.vehicle = source.vehicle();
      this.turret = source.turret();
      this.prefabId = source.prefabId();
      this.testGO = source.testGO();
      this.framesCounted = source.framesCounted();
      this.framesToSkip = source.framesToSkip();
      this.rendererList = source.rendererList();
      this.localRenderer = source.localRenderer();
      this.pooledPrefabRenderer = source.pooledPrefabRenderer();
      this.sharedMaterialsSource = source.sharedMaterialsSource();
      this.sharedMaterialsCopy = source.sharedMaterialsCopy();
      this.defaultMaterial = source.defaultMaterial();
      this.wasEvasiveLastFrame = source.wasEvasiveLastFrame();
      this.guardedLastFrame = source.guardedLastFrame();
      this.coverLastFrame = source.coverLastFrame();
      this.wasUnsteadyLastFrame = source.wasUnsteadyLastFrame();
      this.wasEntrenchedLastFrame = source.wasEntrenchedLastFrame();
      this.forcedPlayerVisibilityLevel = source.forcedPlayerVisibilityLevel;
      this.vfxNameModifier = source.vfxNameModifier();
      this.timeNow = source.timeNow();
      this.elapsedTime = source.elapsedTime();
      this.blipAlpha = source.blipAlpha();
      this.timeFromEnd = source.timeFromEnd();
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
      this.jumpjetReps = source.jumpjetReps();
      this.headlightReps = source.headlightReps();
      this.headDestructible = source.headDestructible;
      this.centerTorsoDestructible = source.centerTorsoDestructible;
      this.leftTorsoDestructible = source.leftTorsoDestructible;
      this.rightTorsoDestructible = source.rightTorsoDestructible;
      this.leftArmDestructible = source.leftArmDestructible;
      this.rightArmDestructible = source.rightArmDestructible;
      this.leftLegDestructible = source.leftLegDestructible;
      this.rightLegDestructible = source.rightLegDestructible;
      this.idleStateEntryHash = source.idleStateEntryHash();
      this.idleStateFlavorsHash = source.idleStateFlavorsHash();
      this.idleStateUnsteadyHash = source.idleStateUnsteadyHash();
      this.idleStateMeleeBaseHash = source.idleStateMeleeBaseHash();
      this.idleStateMeleeEntryHash = source.idleStateMeleeEntryHash();
      this.idleStateMeleeHash = source.idleStateMeleeHash();
      this.idleStateMeleeUnsteadyHash = source.idleStateMeleeUnsteadyHash();
      this.TEMPIdleStateMeleeIdleHash = source.TEMPIdleStateMeleeIdleHash();
      this.idleRandomValueHash = source.idleRandomValueHash();
      this.standingHash = source.standingHash();
      this.groundDeathIdleHash = source.groundDeathIdleHash();
      this.randomDeathIdleA = source.randomDeathIdleA();
      this.randomDeathIdleB = source.randomDeathIdleB();
      this.randomDeathIdleC = source.randomDeathIdleC();
      this.randomDeathIdleBase = source.randomDeathIdleBase();
      this.randomDeathIdleRandomizer = source.randomDeathIdleRandomizer();
      this.hitReactLightHash = source.hitReactLightHash();
      this.hitReactHeavyHash = source.hitReactHeavyHash();
      this.hitReactMeleeHash = source.hitReactMeleeHash();
      this.hitReactDodgeHash = source.hitReactDodgeHash();
      this.hitReactDFAHash = source.hitReactDFAHash();
      this._allowRandomIdles = true;
      this.previousAnimTransition = source.previousAnimTransition();
      this.previousAnimState = source.previousAnimState();
      this.currentAnimTransition = source.currentAnimTransition();
      this.currentAnimState = source.currentAnimState();
      this.currentAnimStateHash = source.currentAnimStateHash();
      this.previousTransitionHash = source.previousTransitionHash();
      this.previousAnimStateHash = source.previousAnimStateHash();
      this.IsOnLimpingLeg = source.IsOnLimpingLeg;
      this.HasCalledOutLimping = source.HasCalledOutLimping;
      this.isPlayingJumpSound = source.isPlayingJumpSound();
      this.isJumping = source.isJumping();
      this.heatAmount = source.heatAmount();
      this.isFakeOverheated = source.isFakeOverheated();
      this.needsToRefreshCombinedMesh = source.needsToRefreshCombinedMesh;
      this.triggerFootVFX = source.triggerFootVFX();
      this.leftFootVFX = source.leftFootVFX();
      this.persistentCritList = source.persistentCritList();
    }
    public virtual bool SetupFallbackTransforms() {
      bool NeedSetupDestructable = true;
      UnitCustomInfo info = this.parentActor.GetCustomInfo();
      if((info != null) && (info.FakeVehicle == true)) {
        NeedSetupDestructable = false;
      }
      bool isPartOfQuad = false;
      if(this.parentRepresentation != null) {
        if (this.parentRepresentation is QuadRepresentation) {
          isPartOfQuad = true;
        }
      }
      bool flag = false;
      if (this.twistTransform == null) {
        flag = true;
        this.twistTransform = this.findRecursive(this.transform, "j_Spine2");
        if (this.twistTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup twistTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.LeftArmAttach == null) {
        flag = true;
        this.LeftArmAttach = this.findRecursive(this.transform, "j_LForearm");
        if (this.LeftArmAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftArmAttach for mech " + this.parentMech.DisplayName));
      }
      if (this.RightArmAttach == null) {
        flag = true;
        this.RightArmAttach = this.findRecursive(this.transform, "j_RForearm");
        if (this.RightArmAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightArmAttach for mech " + this.parentMech.DisplayName));
      }
      if (this.TorsoAttach == null) {
        flag = true;
        this.TorsoAttach = this.findRecursive(this.transform, "j_Spine2");
        if (this.TorsoAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup TorsoAttach for mech " + this.parentMech.DisplayName));
      }
      if (this.LeftLegAttach == null) {
        flag = true;
        this.LeftLegAttach = this.findRecursive(this.transform, "j_LCalf");
        if (this.LeftLegAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftLegAttach for mech " + this.parentMech.DisplayName));
      }
      if (this.leftFootTransform == null) {
        flag = true;
        this.leftFootTransform = this.findRecursive(this.transform, "j_LHeel");
        if (this.leftFootTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftFootTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.RightLegAttach == null) {
        flag = true;
        this.RightLegAttach = this.findRecursive(this.transform, "j_RCalf");
        if (this.RightLegAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightLegAttach for mech " + this.parentMech.DisplayName));
      }
      if (this.rightFootTransform == null) {
        flag = true;
        this.rightFootTransform = this.findRecursive(this.transform, "j_RHeel");
        if (this.rightFootTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightFootTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxCenterTorsoTransform == null) {
        flag = true;
        this.vfxCenterTorsoTransform = this.twistTransform;
        if (this.vfxCenterTorsoTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxCenterTorsoTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftArmTransform == null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if (this.vfxLeftArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxRightArmTransform == null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if (this.vfxRightArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxHeadTransform == null) {
        flag = true;
        this.vfxHeadTransform = this.twistTransform;
        if (this.vfxHeadTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxHeadTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftArmTransform == null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if (this.vfxLeftArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxRightArmTransform == null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if (this.vfxRightArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftLegTransform == null) {
        flag = true;
        this.vfxLeftLegTransform = this.LeftLegAttach;
        if (this.vfxLeftLegTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftLegTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.vfxRightLegTransform == null) {
        flag = true;
        this.vfxRightLegTransform = this.RightLegAttach;
        if (this.vfxRightLegTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightLegTransform for mech " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectUnknown == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectUnknown");
        if (recursive != null)
          this.BlipObjectUnknown = recursive.gameObject;
        if (this.BlipObjectUnknown == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Unknown Blip for mech " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectIdentified == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectIdentified");
        if (recursive != null)
          this.BlipObjectIdentified = recursive.gameObject;
        if (this.BlipObjectIdentified == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Identified Blip for mech " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectGhostWeak == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostWeak");
        if (recursive != null)
          this.BlipObjectGhostWeak = recursive.gameObject;
        if (this.BlipObjectGhostWeak == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Weak Ghost Blip for mech " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectGhostStrong == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostStrong");
        if (recursive != null)
          this.BlipObjectGhostStrong = recursive.gameObject;
        if (this.BlipObjectGhostStrong == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Strong Ghost Blip for mech " + this.parentMech.DisplayName));
      }
      if (this.headDestructible == null) {
        flag = isPartOfQuad==false? true:flag;
        Transform recursive = this.findRecursive(this.transform, "Head_whole");
        if (recursive != null)
          this.headDestructible = recursive.GetComponent<MechDestructibleObject>();
        if ((this.headDestructible == null)&&(isPartOfQuad == false))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup headDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.centerTorsoDestructible == null) {
        flag = NeedSetupDestructable ? (isPartOfQuad == false ? true : flag) : flag;
        Transform recursive = this.findRecursive(this.transform, "torso_whole");
        if (recursive != null) {
          this.centerTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.centerTorsoDestructible = this.headDestructible;
        }
        if ((this.centerTorsoDestructible == null) && (isPartOfQuad == false))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup centerTorsoDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.leftTorsoDestructible == null) {
        flag = NeedSetupDestructable ? (isPartOfQuad == false ? true : flag) : flag;
        Transform recursive = this.findRecursive(this.transform, "lefttorso_whole");
        if (recursive != null) { 
          this.leftTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.leftTorsoDestructible = this.centerTorsoDestructible;
        }
        if ((this.leftTorsoDestructible == null) && (isPartOfQuad == false))
            Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftTorsoDestructible for mech " + this.parentMech.DisplayName));
       }
      if (this.rightTorsoDestructible == null) {
        flag = NeedSetupDestructable ? (isPartOfQuad == false ? true : flag) : flag;
        Transform recursive = this.findRecursive(this.transform, "righttorso_whole");
        if (recursive != null) {
          this.rightTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.rightTorsoDestructible = this.leftTorsoDestructible;
        }
        if ((this.rightTorsoDestructible == null) && (isPartOfQuad == false))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightTorsoDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.leftArmDestructible == null) {
        flag = NeedSetupDestructable ? (isPartOfQuad == false ? true : flag) : flag;
        Transform recursive = this.findRecursive(this.transform, "LArm_whole");
        if (recursive != null) {
          this.leftArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.leftArmDestructible = this.rightTorsoDestructible;
        }
        if ((this.leftArmDestructible == null) && (isPartOfQuad == false))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftArmDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.rightArmDestructible == null) {
        flag = NeedSetupDestructable ? true : flag;
        Transform recursive = this.findRecursive(this.transform, "RArm_whole");
        if (recursive != null) {
          this.rightArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.rightArmDestructible = this.leftArmDestructible;
        }
        if ((this.rightArmDestructible == null) && (isPartOfQuad == false))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightArmDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.leftLegDestructible == null) {
        flag = NeedSetupDestructable ? true : flag;
        Transform recursive = this.findRecursive(this.transform, "LLeg_whole");
        if (recursive != null) {
          this.leftLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.leftLegDestructible = this.rightArmDestructible;
        }
        if ((this.leftLegDestructible == null))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftLegDestructible for mech " + this.parentMech.DisplayName));
      }
      if (this.rightLegDestructible == null) {
        flag = NeedSetupDestructable ? true : flag;
        Transform recursive = this.findRecursive(this.transform, "RLeg_whole");
        if (recursive != null) {
          this.rightLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        } else {
          this.rightLegDestructible = this.leftLegDestructible;
        }
        if ((this.rightLegDestructible == null))
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightLegDestructible for mech " + this.parentMech.DisplayName));
      }
      return flag;
    }
    public virtual void SetupHeadlights() {
      if (this.HasOwnVisuals == false) { return; }
      Log.Combat?.TWL(0, $"CustomMechRepresentation.SetupHeadlights {this.custMech.PilotableActorDef.ChassisID}");
      this.headlightReps.Clear();
      List<string> repNames = new List<string>() {
        string.Format("chrPrfComp_{0}_centertorso_headlight", (object)this.PrefabBase)
        ,string.Format("chrPrfComp_{0}_leftshoulder_headlight", (object)this.PrefabBase)
        ,string.Format("chrPrfComp_{0}_rightshoulder_headlight", (object)this.PrefabBase)
        ,string.Format("chrPrfComp_{0}_leftleg_headlight", (object)this.PrefabBase)
        ,string.Format("chrPrfComp_{0}_rightleg_headlight", (object)this.PrefabBase)
      };
      foreach(string id in repNames) {
        Log.Combat?.WL(1, $"headlight:{id}");
        GameObject gameObject = mech.Combat.DataManager.PooledInstantiate(id, BattleTechResourceType.Prefab);
        if (gameObject != null) {
          gameObject.transform.parent = this.vfxHeadTransform;
          gameObject.transform.localPosition = Vector3.zero;
          gameObject.transform.localRotation = Quaternion.identity;
          gameObject.transform.localScale = Vector3.one;
          this.headlightReps.Add(gameObject);
        }
      }
      if (this.customRep != null) { this.customRep.AttachHeadlights(); }
    }
  }
  public partial class CustomMechRepresentation : MechRepresentation, IOnFootFallReceiver {
    public HashSet<Transform> customFootFalls { get; set; } = new HashSet<Transform>();
    public virtual void OnCustomFootFall(Transform foot) {
      Log.Combat?.TWL(0, "CustomMechRepresentation.OnCustomFootFall " + this.transform.name + " foot:" + foot.name);
      customFootFalls.Add(foot);
    }
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
    private Transform f_j_Root = null;
    public virtual Transform j_Root {
      get {
        if(this.f_j_Root != null) { return f_j_Root; }
        Transform[] trs = this.gameObject.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs) {
          if (tr.parent == null) { continue; }
          if (tr.transform.parent != this.transform) { continue; }
          if (tr.name == "j_Root") { this.f_j_Root = tr; break; }
        }
        if (f_j_Root == null) {
          throw new Exception("can't find j_Root in " + this.gameObject.name);
        }
        return f_j_Root;
      }
    }
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
      Log.Combat?.TWL(0,this.GetType().ToString()+"._Init "+mech.MechDef.ChassisID);
      //this.j_Root = this.transform.FindRecursive("j_Root");
      this.gameObject.GetComponent<Animator>().enabled = true;
      Animator[] animators = this.gameObject.GetComponentsInChildren<Animator>(true);
      foreach (Animator animator in animators) { animator.enabled = true; animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
      this._Init((AbstractActor)mech, parentTransform, isParented);
      this.custMech = this.parentCombatant as CustomMech;
      this.mech = this.parentCombatant as Mech;
      this.vehicle = this.parentCombatant as Vehicle;
      this.turret = this.parentCombatant as Turret;
      this.Constants(mech.Combat.Constants);
      bool flag = false;
      if (this.altDef == null) { this.altDef = new AlternateRepresentationDef(mech.MechDef.Chassis); }
      if (this.f_presistantAudioStart.Count == 0) {
        if(this.altDef != null) {
          if (this.altDef.Type == AlternateRepType.AirMech) {
            if (string.IsNullOrEmpty(this.altDef.HoveringSoundStart) == false) {
              this.f_presistantAudioStart.Add(this.altDef.HoveringSoundStart);
            }
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
          if (this.altDef.Type == AlternateRepType.AirMech) {
            if (string.IsNullOrEmpty(this.altDef.HoveringSoundEnd) == false) {
              this.f_presistantAudioStop.Add(this.altDef.HoveringSoundEnd);
            }
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
      if (this.VisibleObject == null)
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
      if (this.audioObject == null)
        Debug.LogError((object)("================= ERROR! Mech " + this.parentMech.DisplayName + " has no audio object!!! FIX THIS ======================"));
      if (this.audioObject != null) {
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
      Log.Combat?.WL(1,"JumpJets count:"+count);
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
      this.init_heatAmount();
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
        Log.Combat?.TWL(0, "CustomMechRepresentation.InitCustomParticles " + source.transform.name);
        Dictionary<string, CustomParticleSystemDef> definitions = new Dictionary<string, CustomParticleSystemDef>();
        foreach (CustomParticleSystemDef customParticleSystemDef in custRepDef.Particles) {
          definitions.Add(customParticleSystemDef.object_name, customParticleSystemDef);
        }
        ParticleSystem[] particles = source.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles) {
          Log.Combat?.W(1, ps.transform.name);
          if (definitions.TryGetValue(ps.transform.name, out CustomParticleSystemDef def)) {
            Log.Combat?.WL(1, ":" + def.Location);
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, def.Location));
           } else {
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, ChassisLocations.None));
            Log.Combat?.WL(1, ":" + ChassisLocations.None);
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        GameRepresentation.initLogger.LogException(e);
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
        Log.Combat?.TWL(0, "CustomRepresentation.ShowCustomParticles " + this.name+":"+ newLevel);
        foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
          Log.Combat?.WL(1, psRep.ps.gameObject.name);
          psRep.ps.gameObject.SetActive(newLevel == VisibilityLevel.LOSFull);
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
      //}
    }

    public virtual void Init(CustomMech mech, Transform parentTransform, bool isParented) {
      Log.Combat?.TWL(0, "CustomMechRepresentation.Init "+mech.Description.Id);
      this.customRep = this.GetComponent<CustomRepresentation>();
      this._Init(mech, parentTransform, isParented);
      mech.sizeMultiplier();
    }
    public virtual void ApplyScale(Vector3 sizeMultiplier) {
      if (this.HasOwnVisuals) {
        this.j_Root.localScale = sizeMultiplier;
      }
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
      Log.Combat?.TWL(0, "CustomMechRepresentation.CreateBlankPrefabs "+ hardpointData.ID+" location:"+location);
      List<string> componentBlankNames = this.GetComponentBlankNames(usedPrefabNames, hardpointData, location);
      Transform attachTransform = this.GetAttachTransform(location);
      for (int index = 0; index < componentBlankNames.Count; ++index) {
        string blankName = componentBlankNames[index];
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(blankName);
        Log.Combat?.WL(1,"blankName:"+blankName+(customHardpoint == null?"":("->"+ customHardpoint.prefab)));
        if (customHardpoint != null) { blankName = customHardpoint.prefab; }
        GameObject blankGO = this._Combat.DataManager.PooledInstantiate(blankName, BattleTechResourceType.Prefab);
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
      Log.Combat?.TWL(0, "CustomMechRepresentation.InitWeapons "+this.mech.MechDef.ChassisID+ " HardpointData:"+this.HardpointData.ID);
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
          Log.Combat?.WL(1, cmp.component.defId+":"+ cmp.component.baseComponentRef.prefabName);
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
        Log.Combat?.WL(0, cmp.component.defId + ":" + weapon.baseComponentRef.prefabName);
        if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
          //CustomHardpointDef hpDef = CustomHardPointsHelper.Find(cmp.component.baseComponentRef.prefabName);
          Transform attachTransform = this.GetAttachTransform(cmp.attachLocation);
          weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, parentDisplayName);
          this.weaponReps.Add(weapon.weaponRep);
          if (weapon.weaponRep != null) { this.RegisterRenderersComponentRepresentation(weapon.weaponRep); }
          string originalPrefabName = calculator.GetComponentPrefabNameNoAlias(weapon.baseComponentRef);
          if (string.IsNullOrEmpty(originalPrefabName) == false) {
            string mountingPointPrefabName = this.GetComponentMountingPointPrefabName(this.HardpointData, originalPrefabName, cmp.attachLocation);
            if (string.IsNullOrEmpty(mountingPointPrefabName) == false) {
              CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(mountingPointPrefabName);
              Log.Combat?.WL(1, cmp.component.defId + " mount point:" + mountingPointPrefabName+(customHardpoint == null?"":("->"+ customHardpoint.prefab)));
              if (customHardpoint != null) { mountingPointPrefabName = customHardpoint.prefab; }
              GameObject mountPointGO = this._Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab);
              if (mountPointGO != null) {
                this.RegisterRenderersMainHeraldry(mountPointGO);
                Log.Combat?.WL(2, "game object:" + mountPointGO.name);
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
    public virtual void BossAppearAnimation() {
      float starting_height = 200f;
      this.HeightController.ForceHeight(starting_height);
      this.parentActor.FakeHeightDelta(starting_height);
      this.HeightController.FakeHeightControl = true;
      this.HeightController.ForceJumpJetsActive = true;
      this.HeightController.DownSpeed = -50f;
      foreach (var link in this.custMech.linkedActors) {
        link.actor.FakeHeightDelta(starting_height);
        if (link.customMech != null) {
          link.customMech.custGameRep.HeightController.ForceHeight(starting_height);
          link.customMech.custGameRep.HeightController.PendingHeight = 0f;
          link.customMech.custGameRep.HeightController.FakeHeightControl = true;
          link.customMech.custGameRep.HeightController.ForceJumpJetsActive = true;
          continue;
        };
        link.rootHeightTransform.localPosition += Vector3.up * starting_height;
      }
      this.HeightController.PendingHeight = this.altDef.FlyHeight;
      //this.HeightController.DownSpeed = -5f;
    }
  }
}