﻿using BattleTech;
using BattleTech.Rendering;
using BattleTech.Rendering.MechCustomization;
using BattleTech.Rendering.UI;
using CustAmmoCategories;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomUnits {
  public static class BlipFactoryHelper {
    private static GameObject mech_BlipObjectUnknown = null;
    private static GameObject mech_BlipObjectIdentified = null;
    private static GameObject mech_BlipObjectGhostWeak = null;
    private static GameObject mech_BlipObjectGhostStrong = null;
    public static void Clear() {
      if (mech_BlipObjectUnknown != null) { GameObject.Destroy(mech_BlipObjectUnknown); mech_BlipObjectUnknown = null; }
      if (mech_BlipObjectIdentified != null) { GameObject.Destroy(mech_BlipObjectIdentified); mech_BlipObjectIdentified = null; }
      if (mech_BlipObjectGhostWeak != null) { GameObject.Destroy(mech_BlipObjectGhostWeak); mech_BlipObjectGhostWeak = null; }
      if (mech_BlipObjectGhostStrong != null) { GameObject.Destroy(mech_BlipObjectUnknown); mech_BlipObjectGhostStrong = null; }
    }
  }
  public class VehicleDrivenMechRepresentation : CustomMechRepresentation {
    public virtual Animator vehicleAnimator { get; set; }
    public virtual Transform vehicleRep { get; set; } = null;
    public override void Test() { Log.TWL(0,"VehicleDrivenMechRepresentation.Test:"+(vehicleRep.parent == null?"null": vehicleRep.parent.name)); }
    public virtual Dictionary<Material, Material> destructedMaterials { get; set; } = new Dictionary<Material, Material>();
    public void Copy(VehicleRepresentation source) {
      this._parentCombatant = Traverse.Create(source).Field<ICombatant>("_parentCombatant").Value;
      this._parentActor = Traverse.Create(source).Field<AbstractActor>("_parentActor").Value;
      this.thisCharacterController = Traverse.Create(source).Field<CharacterController>("thisCharacterController").Value;
      this.thisIKController = Traverse.Create(source).Field<InverseKinematic>("thisIKController").Value;
      this.audioObject = Traverse.Create(source).Field<AkGameObj>("audioObject").Value;
      this.pilotRep = Traverse.Create(source).Field<PilotRepresentation>("pilotRep").Value;
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
      this._isTargetable = Traverse.Create(source).Property<bool>("isTargetable").Value;
      this._isTargeted = Traverse.Create(source).Property<bool>("isTargeted").Value;
      this._isAvailable = Traverse.Create(source).Property<bool>("isAvailable").Value;
      this._isSelected = Traverse.Create(source).Property<bool>("isSelected").Value;
      this._isHovered = Traverse.Create(source).Property<bool>("isHovered").Value;
      this._isDead = Traverse.Create(source).Property<bool>("isDead").Value;
      // ---------------------- PilotableActorRepresentation -----------------------
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

      this.LeftArmAttach = source.BodyAttach;
      this.RightArmAttach = source.BodyAttach;
      this.TorsoAttach = source.TurretAttach;
      this.LeftLegAttach = source.BodyAttach;
      this.RightLegAttach = source.BodyAttach;
      this.vfxCenterTorsoTransform = source.BodyAttach;
      this.vfxLeftTorsoTransform = source.BodyAttach;
      this.vfxRightTorsoTransform = source.BodyAttach;
      this.vfxHeadTransform = source.TurretLOS;
      this.vfxLeftArmTransform = source.BodyAttach;
      this.vfxRightArmTransform = source.BodyAttach;
      this.vfxLeftLegTransform = source.LeftSideLOS;
      this.vfxRightLegTransform = source.RightSideLOS;
      this.vfxLeftShoulderTransform = source.LeftSideLOS;
      this.vfxRightShoulderTransform = source.RightSideLOS;
      this.leftFootTransform = source.LeftSideLOS;
      this.rightFootTransform = source.RightSideLOS;
      this.headDestructible = source.vehicleDestructible;
      this.centerTorsoDestructible = source.vehicleDestructible;
      this.leftTorsoDestructible = source.vehicleDestructible;
      this.rightTorsoDestructible = source.vehicleDestructible;
      this.leftArmDestructible = source.vehicleDestructible;
      this.rightArmDestructible = source.vehicleDestructible;
      this.leftLegDestructible = source.vehicleDestructible;
      this.rightLegDestructible = source.vehicleDestructible;
      this.vehicleAnimator = source.GetComponent<Animator>();
      this.twistTransform = source.twistTransform;
      MechCustomization mechCust = this.GetComponentInChildren<MechCustomization>(true);
      MechCustomization vehicleCust = source.GetComponentInChildren<MechCustomization>(true);
      mechCust.paintPatterns = vehicleCust.paintPatterns;
      mechCust.backerPatterns = vehicleCust.backerPatterns;
      mechCust.decal0 = vehicleCust.decal0;
      mechCust.decal1 = vehicleCust.decal1;
      mechCust.decal2 = vehicleCust.decal2;
      mechCust.decal3 = vehicleCust.decal3;
      mechCust.decal4 = vehicleCust.decal4;
      mechCust.decal5 = vehicleCust.decal5;
    }
    public override void Twist(float angle) {
      base.Twist(angle);
      if (this.RotateBody == false) {
        if (this.vehicleAnimator != null) { this.vehicleAnimator.SetFloat("Twist", angle); }
      }
    }
    public override bool SetupFallbackTransforms() {
      bool flag = false;
      if (this.twistTransform == null) {
        flag = true;
        this.twistTransform = this.findRecursive(this.transform, "j_Twist");
        if (this.twistTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup twistTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.LeftArmAttach == null) {
        flag = true;
        this.LeftArmAttach = this.findRecursive(this.transform, "j_Base");
        if (this.LeftArmAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftArmAttach for vehicle " + this.parentMech.DisplayName));
      }
      if (this.RightArmAttach == null) {
        flag = true;
        this.RightArmAttach = this.findRecursive(this.transform, "j_Base");
        if (this.RightArmAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightArmAttach for vehicle " + this.parentMech.DisplayName));
      }
      if (this.TorsoAttach == null) {
        flag = true;
        this.TorsoAttach = this.findRecursive(this.transform, "j_Base");
        if (this.TorsoAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup TorsoAttach for vehicle " + this.parentMech.DisplayName));
      }
      if (this.LeftLegAttach == null) {
        flag = true;
        this.LeftLegAttach = this.findRecursive(this.transform, "j_Base");
        if (this.LeftLegAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup LeftLegAttach for vehicle " + this.parentMech.DisplayName));
      }
      if (this.leftFootTransform == null) {
        flag = true;
        this.leftFootTransform = this.findRecursive(this.transform, "j_Base");
        if (this.leftFootTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftFootTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.RightLegAttach == null) {
        flag = true;
        this.RightLegAttach = this.findRecursive(this.transform, "j_Base");
        if (this.RightLegAttach == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup RightLegAttach for vehicle " + this.parentMech.DisplayName));
      }
      if (this.rightFootTransform == null) {
        flag = true;
        this.rightFootTransform = this.findRecursive(this.transform, "j_Base");
        if (this.rightFootTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightFootTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxCenterTorsoTransform == null) {
        flag = true;
        this.vfxCenterTorsoTransform = this.twistTransform;
        if (this.vfxCenterTorsoTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxCenterTorsoTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftArmTransform == null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if (this.vfxLeftArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxRightArmTransform == null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if (this.vfxRightArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxHeadTransform == null) {
        flag = true;
        this.vfxHeadTransform = this.twistTransform;
        if (this.vfxHeadTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxHeadTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftArmTransform == null) {
        flag = true;
        this.vfxLeftArmTransform = this.LeftArmAttach;
        if (this.vfxLeftArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftArmTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxRightArmTransform == null) {
        flag = true;
        this.vfxRightArmTransform = this.RightArmAttach;
        if (this.vfxRightArmTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightArmTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxLeftLegTransform == null) {
        flag = true;
        this.vfxLeftLegTransform = this.LeftLegAttach;
        if (this.vfxLeftLegTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxLeftLegTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.vfxRightLegTransform == null) {
        flag = true;
        this.vfxRightLegTransform = this.RightLegAttach;
        if (this.vfxRightLegTransform == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup vfxRightLegTransform for vehicle " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectUnknown == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectUnknown");
        if (recursive != null)
          this.BlipObjectUnknown = recursive.gameObject;
        if (this.BlipObjectUnknown == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Unknown Blip for vehicle " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectIdentified == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectIdentified");
        if (recursive != null)
          this.BlipObjectIdentified = recursive.gameObject;
        if (this.BlipObjectIdentified == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Identified Blip for vehicle " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectGhostWeak == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostWeak");
        if (recursive != null)
          this.BlipObjectGhostWeak = recursive.gameObject;
        if (this.BlipObjectGhostWeak == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Weak Ghost Blip for vehicle " + this.parentMech.DisplayName));
      }
      if (this.BlipObjectGhostStrong == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "BlipObjectGhostStrong");
        if (recursive != null)
          this.BlipObjectGhostStrong = recursive.gameObject;
        if (this.BlipObjectGhostStrong == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup Strong Ghost Blip for vehicle " + this.parentMech.DisplayName));
      }
      if (this.headDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.headDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.headDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup headDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.centerTorsoDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.centerTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.centerTorsoDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup centerTorsoDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.leftTorsoDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.leftTorsoDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftTorsoDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.rightTorsoDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.rightTorsoDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightTorsoDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.leftArmDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.leftArmDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftArmDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.rightArmDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightArmDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.rightArmDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightArmDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.leftLegDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.leftLegDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup leftLegDestructible for vehicle " + this.parentMech.DisplayName));
      }
      if (this.rightLegDestructible == null) {
        flag = true;
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightLegDestructible = recursive.GetComponent<MechDestructibleObject>();
        if (this.rightLegDestructible == null)
          Debug.LogWarning((object)("ERROR! Couldn't auto-setup rightLegDestructible for vehicle " + this.parentMech.DisplayName));
      }
      return flag;
    }
    public override void SetupHeadlights() {
      if (this.HasOwnVisuals == false) { return; }
      this.headlightReps.Clear();
      BTLight[] headLights = this.GetComponentsInChildren<BTLight>(true);
      foreach(BTLight light in headLights) {
        this.headlightReps.Add(light.gameObject);
      }
      if (this.customRep != null) { this.customRep.AttachHeadlights(); }
    }
    public virtual void InitDestructedMaterials() {
      HashSet<Renderer> renderers = this.VisibleObject.GatherRenderers();
      foreach(Renderer r in renderers) {
        if (r.sharedMaterial == null) { continue; }
        if(this.destructedMaterials.ContainsKey(r.sharedMaterial) == false) {
          Material destructMaterial = GameObject.Instantiate<Material>(r.sharedMaterial);
          destructMaterial.SetTexture(MechMeshMerge.Uniforms._DamageAlbedoMap, (Texture)MechMeshMerge.damageAlbedo);
          destructMaterial.SetTextureScale("_DamageAlbedoMap", new Vector2(6f, 6f));
          destructMaterial.SetTexture(MechMeshMerge.Uniforms._DamageNormalMap, (Texture)MechMeshMerge.damageNormal);
          destructMaterial.EnableKeyword("_DAMAGED");
          destructMaterial.EnableKeyword("_DAMAGEDFULL");
          this.destructedMaterials.Add(r.sharedMaterial, destructMaterial);
        }
      }
    }
    public override void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      string vfxName;
      AudioEventList_vehicle eventEnumValue;
      switch (Random.Range(0, 4)) {
        case 0:
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.componentDestruction_A;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_a;
        break;
        case 1:
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.componentDestruction_B;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_b;
        break;
        case 2:
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.componentDestruction_C;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_a;
        break;
        default:
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.componentDestruction_D;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_b;
        break;
      }
      this.PlayVFX(location, vfxName, false, Vector3.zero, true, -1f);
      if (isSlave == false) {
        int num = (int)WwiseManager.PostEvent<AudioEventList_vehicle>(eventEnumValue, this.audioObject);
        if (this.parentMech.team.LocalPlayerControlsTeam)
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_crippled");
        else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_crippled");
      }
    }
    public override void PlayDeathVFX(DeathMethod deathMethod, int location) {
      string vehicleDeathA = (string)this.parentCombatant.Combat.Constants.VFXNames.vehicleDeath_A;
      string vfxName;
      AudioEventList_vehicle eventEnumValue;
      if (Random.Range(0, 2) == 0) {
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.vehicleDeath_A;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_a;
      } else {
        vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.vehicleDeath_B;
        eventEnumValue = AudioEventList_vehicle.vehicle_explosion_b;
      }
      if (this.renderers.Count == 0) {
        Debug.LogError((object)"No renderers");
      } else {
        if (this.renderers[0] == null || this.renderers[0].sharedMaterial == null) {
          Debug.LogError((object)"Error in VehicleRepresentation - null renderer or sharedMaterial found!");
        } else {
          this.InitDestructedMaterials();
          foreach (Renderer renderer in this.renderers) {
            if (renderer.sharedMaterial == null) { continue; }
            if (this.destructedMaterials.TryGetValue(renderer.sharedMaterial, out Material dMat)) {
              renderer.sharedMaterial = dMat;
            }
          }
        }
        MechDestructibleObject destructibleObject = this.GetDestructibleObject(location);
        if (destructibleObject != null) {
          destructibleObject.Collapse(Random.onUnitSphere, this.parentActor.Combat.Constants.ResolutionConstants.ComponentDestructionForceMultiplier);
        }
        this.HeightController.PendingHeight = 0f;
        this.customRep.OnUnitDestroy();
        this.PlayVFX(location, vfxName, false, Vector3.zero, true, -1f);
        if (this.parentActor.Combat.IsLoadingFromSave) { return; }
        if (isSlave == false) {
          int num = (int)WwiseManager.PostEvent<AudioEventList_vehicle>(eventEnumValue, this.audioObject);
        }
      }
    }
    public override void SetupJumpJets() {
      this.jumpjetReps.Clear();
      if (this.HasOwnVisuals == false) { return; }
      if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab) == false) {
        Log.WL(1, "VehicleDrivenMechRepresentation.SetupJumpJets");
        GameObject JumpJetSrcPrefab = null;
        try {
          JumpJetSrcPrefab = this.parentCombatant.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
          if (JumpJetSrcPrefab == null) {
            Log.WL(2, "jumpJetSrcPrefab:" + (JumpJetSrcPrefab == null ? "null" : JumpJetSrcPrefab.name));
            return;
          }
          Transform JumpJetSrc = null;
          if (JumpJetSrcPrefab != null) {
            JumpJetSrc = JumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
            Log.WL(2, "jumpJetSrc:" + (JumpJetSrc == null ? "null" : JumpJetSrc.name));
            if (JumpJetSrc == null) { return; }
          }
          List<Transform> jumpAttachs = new List<Transform>() { this.vfxLeftLegTransform, this.vfxRightLegTransform };
          foreach (Transform spawnJetPoint in jumpAttachs) {
            if (spawnJetPoint == null) { Log.WL(5, "spawnJetPoint is null"); continue; }
            GameObject jumpJetBase = new GameObject("jumpJet");
            jumpJetBase.transform.SetParent(spawnJetPoint);
            jumpJetBase.transform.localPosition = Vector3.zero;
            jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject jumpJet = GameObject.Instantiate(JumpJetSrc.gameObject);
            jumpJet.SetActive(true);
            jumpJet.transform.SetParent(jumpJetBase.transform);
            jumpJet.transform.localPosition = Vector3.zero;
            jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
            jRep.Init(this.parentMech, spawnJetPoint, true, false, this.name);
            ParticleSystem[] psyss = jumpJetBase.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem psys in psyss) {
              psys.RegisterRestoreScale();
              var main = psys.main;
              main.scalingMode = ParticleSystemScalingMode.Hierarchy;
              Log.WL(3, psys.name + ":" + psys.main.scalingMode);
            }
            this.jumpjetReps.Add(jRep);
            this.RegisterRenderersCustomHeraldry(jRep.gameObject, null);
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
        if (JumpJetSrcPrefab != null) { this.parentCombatant.Combat.DataManager.PoolGameObject(Core.Settings.CustomJumpJetsComponentPrefab, JumpJetSrcPrefab); }
      }
    }
    public override void _Init(Mech mech, Transform parentTransform, bool isParented) {
      base._Init(mech, parentTransform, isParented);
      WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, 0.0f, this.audioObject);
      this.renderers = new List<Renderer>((IEnumerable<Renderer>)this.VisibleObject.GetComponentsInChildren<MeshRenderer>(true));
      this.InitDestructedMaterials();
      this.Test();
    }

  }
}