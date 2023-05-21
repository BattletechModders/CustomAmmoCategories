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
using BattleTech.Rendering.MechCustomization;
using BattleTech.Rendering.UI;
using CustAmmoCategories;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomUnits {
  [HarmonyPatch(typeof(VehicleRepresentation))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class VehicleRepresentation_Update {
    public static bool Prefix(VehicleRepresentation __instance) {
      return true;
    }
  }

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
    //public override void Test() { Log.TWL(0,"VehicleDrivenMechRepresentation.Test:"+(vehicleRep.parent == null?"null": vehicleRep.parent.name)); }
    public virtual Dictionary<Material, Material> destructedMaterials { get; set; } = new Dictionary<Material, Material>();
    public void Copy(VehicleRepresentation source) {
      this._parentCombatant = (source._parentCombatant);
      this._parentActor = source._parentActor;
      this.thisCharacterController = source.thisCharacterController;
      this.thisIKController = source.thisIKController;
      this.audioObject = source.audioObject;
      this.pilotRep = source.pilotRep;
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
      this.isTargetable = source.isTargetable;
      this.isTargeted = source.isTargeted;
      this.isAvailable = source.isAvailable;
      this.isSelected = source.isSelected;
      this.isHovered = source.isHovered;
      this.isDead = source.isDead;
      // ---------------------- PilotableActorRepresentation -----------------------
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
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.headDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.centerTorsoDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.centerTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.leftTorsoDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.rightTorsoDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightTorsoDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.leftArmDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftArmDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.rightArmDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightArmDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.leftLegDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.leftLegDestructible = recursive.GetComponent<MechDestructibleObject>();
      }
      if (this.rightLegDestructible == null) {
        Transform recursive = this.findRecursive(this.transform, "whole_group");
        if (recursive != null)
          this.rightLegDestructible = recursive.GetComponent<MechDestructibleObject>();
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
    public override void HandleDeath(DeathMethod deathMethod, int location) {
      PilotableRepresentation_HandleDeath(deathMethod, location);
      if (this.customRep != null) { this.StopCustomParticles(); }
      if (isSlave == false) this._PlayDeathFloatie(deathMethod);
      if (this.parentActor.WasDespawned) { return; }
      if (this.VisibleObjectLight != null) { this.VisibleObjectLight.SetActive(false); }
      //this.thisAnimator.SetTrigger("Death");
      if (!this.parentMech.Combat.IsLoadingFromSave) {
        if (isSlave == false) {
          if (this.parentMech.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_destroyed");
          else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_destroyed");
        }
      }
      //if (this.parentMech.IsOrWillBeProne || this.parentActor.WasEjected) { this.StartCoroutine(this.DelayProneOnDeath()); }
      if (!this.parentActor.WasEjected) { this.PlayDeathVFX(deathMethod, location); }
      this.HeightController.PendingHeight = 0f;
      if (this.customRep != null) { this.customRep.InBattle = false; }
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) { this.StopManualPersistentVFX(stringList[index]); }
      this._IsDead = true;
      if (deathMethod != DeathMethod.PilotKilled && !this.parentActor.WasEjected) {
        string vfxName;
        switch (Random.Range(0, 4)) {
          case 0:
          vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.deadMechLoop_A;
          break;
          case 1:
          vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.deadMechLoop_B;
          break;
          case 2:
          vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.deadMechLoop_C;
          break;
          default:
          vfxName = (string)this.parentCombatant.Combat.Constants.VFXNames.deadMechLoop_D;
          break;
        }
        this.PlayVFX(8, vfxName, true, Vector3.zero, false, -1f);
      }
      this._ToggleHeadlights(false);
    }
    public override void PlayDeathVFX(DeathMethod deathMethod, int location) {
      Log.Combat?.TWL(0, "VehicleDrivenMechRepresentation.PlayDeathVFX deathMethod:" +deathMethod);
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
          Log.Combat?.WL(1, "WwiseManager.PostEvent:" + eventEnumValue+" result:"+num);
        }
      }
    }
    public override void SetupJumpJets() {
      this.jumpjetReps.Clear();
      if (this.HasOwnVisuals == false) { return; }
      if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab) == false) {
        Log.Combat?.WL(1, "VehicleDrivenMechRepresentation.SetupJumpJets");
        GameObject JumpJetSrcPrefab = null;
        try {
          JumpJetSrcPrefab = this.parentCombatant.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
          if (JumpJetSrcPrefab == null) {
            Log.Combat?.WL(2, "jumpJetSrcPrefab:" + (JumpJetSrcPrefab == null ? "null" : JumpJetSrcPrefab.name));
            return;
          }
          Transform JumpJetSrc = null;
          if (JumpJetSrcPrefab != null) {
            JumpJetSrc = JumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
            Log.Combat?.WL(2, "jumpJetSrc:" + (JumpJetSrc == null ? "null" : JumpJetSrc.name));
            if (JumpJetSrc == null) { return; }
          }
          List<Transform> jumpAttachs = new List<Transform>() { this.vfxLeftLegTransform, this.vfxRightLegTransform };
          foreach (Transform spawnJetPoint in jumpAttachs) {
            if (spawnJetPoint == null) { Log.Combat?.WL(5, "spawnJetPoint is null"); continue; }
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
              Log.Combat?.WL(3, psys.name + ":" + psys.main.scalingMode);
            }
            this.jumpjetReps.Add(jRep);
            this.RegisterRenderersCustomHeraldry(jRep.gameObject, null);
          }
        } catch (Exception e) {
          Log.Combat?.TWL(0, e.ToString(), true);
          AbstractActor.logger.LogException(e);
        }
        if (JumpJetSrcPrefab != null) { this.parentCombatant.Combat.DataManager.PoolGameObject(Core.Settings.CustomJumpJetsComponentPrefab, JumpJetSrcPrefab); }
      }
    }
    public override void _Init(Mech mech, Transform parentTransform, bool isParented) {
      base._Init(mech, parentTransform, isParented);
      WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, 0.0f, this.audioObject);
      this.renderers = new List<Renderer>((IEnumerable<Renderer>)this.VisibleObject.GetComponentsInChildren<MeshRenderer>(true));
      this.InitDestructedMaterials();
      //this.Test();
    }
    public override Vector3 GetMissPosition(Vector3 attackOrigin, Weapon weapon, NetworkRandom random) {
      Vector3 position = this.parentMech.CurrentPosition + Vector3.up * this.HeightController.CurrentHeight;
      float radius = this.parentMech.MechDef.Chassis.Radius;
      AttackDirection attackDirection = this.parentActor.Combat.HitLocation.GetAttackDirection(weapon.parent, (ICombatant)this.parentActor);
      bool flag = random.Int(max: 2) == 0;
      float num1 = random.Float(this.Constants.ResolutionConstants.MissOffsetHorizontalMin, this.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      if (weapon.Type == WeaponType.LRM) {
        Vector2 vector2 = random.Circle().normalized * (radius * num1);
        position.x += vector2.x;
        position.z += vector2.y;
        return position;
      }
      Vector3 vector3;
      switch (attackDirection) {
        case AttackDirection.FromFront:
        vector3 = !flag ? position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1 : position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1;
        break;
        case AttackDirection.FromLeft:
        vector3 = !flag ? position - this.thisTransform.forward * (radius * 0.6f) - this.thisTransform.forward * num1 : position + this.thisTransform.forward * (radius * 0.6f) + this.thisTransform.forward * num1;
        break;
        case AttackDirection.FromRight:
        vector3 = !flag ? position + this.thisTransform.forward * (radius * 0.6f) + this.thisTransform.forward * num1 : position - this.thisTransform.forward * (radius * 0.6f) - this.thisTransform.forward * num1;
        break;
        case AttackDirection.FromBack:
        vector3 = !flag ? position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1 : position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1;
        break;
        default:
        vector3 = !flag ? position - this.thisTransform.right * (radius * 1f) - this.thisTransform.right * num1 : position + this.thisTransform.right * (radius * 1f) + this.thisTransform.right * num1;
        break;
      }
      float num2 = random.Float(-this.Constants.ResolutionConstants.MissOffsetVerticalMin, this.Constants.ResolutionConstants.MissOffsetVerticalMax);
      vector3.y += num2;
      return vector3;
    }
  }
}