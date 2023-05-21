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
using BattleTech.Rendering.UI;
using BattleTech.Rendering.UrbanWarfare;
using CustAmmoCategories;
using HarmonyLib;
using Localize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("CurrentSurfaceType")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class PilotableActorRepresentation_CurrentSurfaceType {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, ref AudioSwitch_surface_type __result) {
      try {
        if (!__runOriginal) { return; }
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep._CurrentSurfaceType;
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("BlipDisplayed")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class PilotableActorRepresentation_BlipDisplayed {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, ref bool __result) {
      try {
        if (!__runOriginal) { return; }
        //Log.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          __result = custMechRep._BlipDisplayed;
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform), typeof(bool) })]
  public static class PilotableActorRepresentation_Init {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, AbstractActor unit, Transform parentTransform, bool isParented) {
      try {
        if (!__runOriginal) { return; }
        PilotableActorRepresentation_Init_vehicle.Prefix(__instance, unit, parentTransform, isParented);
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._Init(unit, parentTransform, isParented);
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("InitPaintScheme")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(HeraldryDef), typeof(string) })]
  public static class PilotableActorRepresentation_InitPaintScheme {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, HeraldryDef heraldryDef, string teamGUID) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, "PilotableActorRepresentation.InitPaintScheme :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._InitPaintScheme(heraldryDef, teamGUID);
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("SetForcedPlayerVisibilityLevel")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VisibilityLevel), typeof(bool) })]
  public static class PilotableActorRepresentation_SetForcedPlayerVisibilityLevel {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, VisibilityLevel newVisibility, bool showUI) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, "PilotableActorRepresentation.SetForcedPlayerVisibilityLevel :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetForcedPlayerVisibilityLevel(newVisibility, showUI);
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("ClearForcedPlayerVisibilityLevel")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class PilotableActorRepresentation_ClearForcedPlayerVisibilityLevel {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, List<ICombatant> allCombatants) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, "PilotableActorRepresentation.ClearForcedPlayerVisibilityLevel :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ClearForcedPlayerVisibilityLevel(allCombatants);
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("PlayEjectFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class PilotableActorRepresentation_PlayEjectFX {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, "PilotableActorRepresentation.PlayEjectFX :" + __instance.GetType().ToString());
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PlayEjectFX();
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("SetBlipPositionRotation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion) })]
  public static class PilotableActorRepresentation_SetBlipPositionRotation {
    public static void Prefix(ref bool __runOriginal, PilotableActorRepresentation __instance, Vector3 position, Quaternion rotation) {
      try {
        if (!__runOriginal) { return; }
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetBlipPositionRotation(position, rotation);
          __runOriginal = false;
          return;
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
        return;
      }
    }
  }
  public partial class CustomMechRepresentation {
    public AbstractActor __parentActor { get { return this.parentActor; } set { this.parentActor(value); } }

    public override Collider[] AllRaycastColliders {
      get {
        if (this._allRaycastColliders == null) {
          if ((UnityEngine.Object)this.VFXCollider != (UnityEngine.Object)null)
            this._allRaycastColliders = this.VFXCollider.gameObject.GetComponentsInChildren<Collider>(true);
          else
            this._allRaycastColliders = this.gameObject.GetComponentsInChildren<Collider>(true);
        }
        return this._allRaycastColliders;
      }
    }

    public virtual AudioSwitch_surface_type _CurrentSurfaceType { get { return this.currentSurfaceType; } }

    //private BattleTech.Rendering.MechCustomization.MechCustomization mechCustomization { get; set; }

    public virtual void _Init(AbstractActor unit, Transform parentTransform, bool isParented) {
      this.Init((ICombatant)unit, parentTransform, isParented, false, this.name);
      this.__parentActor = this._parentCombatant as AbstractActor;
      this.mainCollider = this.gameObject.GetComponent<CapsuleCollider>();
      this.currentTwistAngle = 0.0f;
      BlipManager instance = BlipManager.Instance;
      if(this.BlipObjectUnknown != null) this.BlipObjectUnknown.transform.localScale = Vector3.one;
      if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.transform.localScale = Vector3.one;
      if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.transform.SetParent(instance.gameObject.transform);
      if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.transform.SetParent(instance.gameObject.transform);
      if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.AddComponent<UIBleep>();
      if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.AddComponent<UIBleep>();
      this.VisibleLights = this.GetComponentsInChildren<BTLight>(true);
      for (int index = 0; index < this.VisibleLights.Length; ++index) {
        if (this.VisibleLights[index].lightType == BTLight.LightTypes.Spot) {
          this.VisibleLights[index].castShadows = true;
          this.VisibleLights[index].contributeVolumetrics = true;
          this.VisibleLights[index].volumetricsMultiplier = 100f;
        }
      }
      this.SetVFXColliderEnabled(false);
      this.mechCustomization = null;
      CustomMechCustomization[] customizations = this.VisibleObject.GetComponentsInChildren<CustomMechCustomization>(true);
      this.mechCustomizations = new List<CustomMechCustomization>();
    }
        public virtual void _InitPaintScheme(HeraldryDef heraldryDef, string teamGUID)
        {
            Log.Combat?.TWL(0, "CustomMechRepresentation._InitPaintScheme " + this.mech.MechDef.ChassisID + " ");
            if (this.paintSchemeInitialized) { return; }

            this.paintSchemeInitialized = true;
            CustomMechCustomization[] customizations = this.VisibleObject.GetComponentsInChildren<CustomMechCustomization>(true);
            this.mechCustomizations = new List<CustomMechCustomization>(customizations);
            if (this.mechCustomizations.Count == 0)
            {
                this.LogPaintSchemeError("mechCustomization is null");
            }
            else
            {
                if (heraldryDef == null)
                {
                    this.LogPaintSchemeError("HeraldryDef is null");
                    if (teamGUID == "bf40fd39-ccf9-47c4-94a6-061809681140")
                    {
                        heraldryDef = this.parentCombatant.Combat.DataManager.Heraldries.Get("heraldrydef_player");
                        heraldryDef.Refresh();
                        Log.Combat?.TWL(0, $"  heraldryDef is player. Colors: {heraldryDef.primaryMechColorID} / {heraldryDef.secondaryMechColorID} / {heraldryDef.tertiaryMechColorID}  logo: {heraldryDef.textureLogoID}");
                    }
                    else
                    {
                        heraldryDef = this.parentCombatant.Combat.DataManager.Heraldries.Get("heraldrydef_enemy");
                        heraldryDef.Refresh();
                        Log.Combat?.TWL(0, $"  heraldryDef is enemy. Colors: {heraldryDef.primaryMechColorID} / {heraldryDef.secondaryMechColorID} / {heraldryDef.tertiaryMechColorID}  logo: {heraldryDef.textureLogoID}");
                    }
                }
                else
                {
                    Log.Combat?.TWL(0, $"  heraldryDef already set. Colors: {heraldryDef.primaryMechColorID} / {heraldryDef.secondaryMechColorID} / {heraldryDef.tertiaryMechColorID}  logo: {heraldryDef.textureLogoID}");
                }

                string camoMaskTextureId = (string)null;
                int texture_id = -1;
                if (this.parentActor.team.IsLocalPlayer)
                {
                    camoMaskTextureId = this.parentActor.PilotableActorDef.PaintTextureID;
                    Log.Combat?.TWL(0, $"  localPlayer parentActorDef.paintTextureId: {this.parentActor.PilotableActorDef.PaintTextureID}");

                    SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
                    if (simulation != null)
                    {
                        if (string.IsNullOrEmpty(camoMaskTextureId))
                        {
                            // No camo texture specified, build one using discard logic
                            if (!simulation.pilotableActorPaintDiscardPile.ContainsKey(this.parentActor.PilotableActorDef.Description.Id))
                                simulation.pilotableActorPaintDiscardPile.Add(this.parentActor.PilotableActorDef.Description.Id, new List<string>());

                            List<string> discardList = simulation.pilotableActorPaintDiscardPile[this.parentActor.PilotableActorDef.Description.Id];
                            Dictionary<string, int> stringList2 = new Dictionary<string, int>();
                            for (int i = 0; i < this.mechCustomizations[0].paintPatterns.Length; ++i)
                            {
                                if (!discardList.Contains(this.mechCustomizations[0].paintPatterns[i].name))
                                    stringList2.Add(this.mechCustomizations[0].paintPatterns[i].name, i);
                            }

                            if (discardList.Count >= stringList2.Count) { discardList.Clear(); }
                            if (stringList2.Count > 0)
                            {
                                List<string> textures = new List<string>(stringList2.Keys);
                                camoMaskTextureId = textures[UnityEngine.Random.Range(0, stringList2.Count - 1)];
                                this.parentActor.PilotableActorDef.UpdatePaintTextureId(camoMaskTextureId);
                                texture_id = stringList2[camoMaskTextureId];
                                discardList.Add(camoMaskTextureId);
                            }
                            Log.Combat?.TWL(0, $"  Local player but not mask, randomized camoMask: {camoMaskTextureId}  new texture_id: {texture_id}");
                        }
                        else
                        {
                            // Camo texture specified, so go with player's choice
                            Dictionary<string, int> stringList2 = new Dictionary<string, int>();
                            for (int i = 0; i < this.mechCustomizations[0].paintPatterns.Length; ++i)
                            {
                                stringList2.Add(this.mechCustomizations[0].paintPatterns[i].name, i);
                            }
                            texture_id = stringList2[camoMaskTextureId];
                            Log.Combat?.TWL(0, $"  Local player retaining: {camoMaskTextureId}  new texture_id: {texture_id}");
                        }
                    }
                    else
                    {
                        // Skirmish, randomize the player's skins
                        texture_id = UnityEngine.Random.Range(0, this.mechCustomizations[0].paintPatterns.Length);
                        Log.Combat?.TWL(0, $"  Local player but skirmish, parentActorDef.paintTextureId: {this.parentActor.PilotableActorDef.PaintTextureID}  new texture_id: {texture_id}");
                    }                    
                }
                else
                {
                    texture_id = UnityEngine.Random.Range(0, this.mechCustomizations[0].paintPatterns.Length);
                    Log.Combat?.TWL(0, $"  Not local player, parentActorDef.paintTextureId: {this.parentActor.PilotableActorDef.PaintTextureID}  new texture_id: {texture_id}");                    
                }

                // Set the texture on every mech customization
                foreach (CustomMechCustomization mechCustomization in mechCustomizations)
                {
                    if (mechCustomization.paintPatterns.Length == 0) { continue; }
                    camoMaskTextureId = mechCustomization.paintPatterns[texture_id % mechCustomization.paintPatterns.Length].name;
                    Log.Combat?.WL(2, "object: " + mechCustomization.gameObject.name + " apply texture:" + camoMaskTextureId);
                    mechCustomization.ApplyHeraldry(heraldryDef, camoMaskTextureId);
                }
            }
        }

        protected virtual void PilotableActorRepresentation_Update() {
      if (DebugBridge.UseExperimentalPinkMechFix) {
        try {
          this.ReconnectMissingMaterial();
        } catch (Exception ex) {
          DebugBridge.UseExperimentalPinkMechFix = false;
          GameRepresentation.initLogger.LogException(ex);
        }
      }
      if (this.isSlave == false) {
        GameRepresentation_Update();
        this._updateBlips();
        this._UpdateStateInfo();
      }
    }

    private PilotableActorRepresentation PooledPrefab {
      get {
        if ((UnityEngine.Object)this.pooledPrefab == (UnityEngine.Object)null) {
          this.mech = this.parentCombatant as Mech;
          this.vehicle = this.parentCombatant as Vehicle;
          this.turret = this.parentCombatant as Turret;
          if (this.mech != null)
            this.prefabId = this.mech.MechDef.Chassis.PrefabIdentifier;
          else if (this.vehicle != null)
            this.prefabId = this.vehicle.VehicleDef.Chassis.PrefabIdentifier;
          else if (this.turret != null)
            this.prefabId = this.turret.TurretDef.Chassis.PrefabIdentifier;
          this.testGO = this.parentCombatant.Combat.DataManager.GetPooledPrefab(this.prefabId) as GameObject;
          this.pooledPrefab = this.testGO.GetComponent<PilotableActorRepresentation>();
        }
        return this.pooledPrefab;
      }
    }

    private List<Renderer> RendererList {
      get {
        if (this.rendererList == null) {
          this.rendererList = new List<Renderer>();
          this.AddRenderersToCachedList(this.transform.Find("mesh"));
          this.AddRenderersToCachedList(this.transform.Find("j_Root"));
        }
        return this.rendererList;
      }
    }

    private void AddRenderersToCachedList(Transform childTransform) {
      if ((UnityEngine.Object)childTransform == (UnityEngine.Object)null)
        return;
      foreach (Renderer componentsInChild in (Renderer[])childTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        this.rendererList.Add(componentsInChild);
      foreach (Renderer componentsInChild in (Renderer[])childTransform.GetComponentsInChildren<MeshRenderer>(true))
        this.rendererList.Add(componentsInChild);
    }

    private Renderer GetRenderer(string name) {
      for (int index = 0; index < this.RendererList.Count; ++index) {
        this.localRenderer = this.RendererList[index];
        if (this.localRenderer.name == name)
          return this.localRenderer;
      }
      return (Renderer)null;
    }

    private Material GetDefaultMaterial() {
      if ((UnityEngine.Object)this.defaultMaterial == (UnityEngine.Object)null) {
        for (int index1 = 0; index1 < this.RendererList.Count; ++index1) {
          this.localRenderer = this.RendererList[index1];
          this.sharedMaterialsSource = this.localRenderer.sharedMaterials;
          if (this.sharedMaterialsSource != null) {
            for (int index2 = 0; index2 < this.sharedMaterialsSource.Length; ++index2) {
              this.defaultMaterial = this.sharedMaterialsSource[index2];
              if ((UnityEngine.Object)this.defaultMaterial != (UnityEngine.Object)null)
                return this.defaultMaterial;
            }
          }
        }
      }
      return this.defaultMaterial;
    }
    public delegate Material d_GetDefaultMaterial(PilotableActorRepresentation rep);
    private static d_GetDefaultMaterial i_GetDefaultMaterial = null;
    private static Material GetDefaultMaterial(PilotableActorRepresentation rep) {
      if (i_GetDefaultMaterial == null) {
        MethodInfo method = typeof(PilotableActorRepresentation).GetMethod("GetDefaultMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("PilotableActorRepresentationGetDefaultMaterial", typeof(Material), new Type[] { typeof(PilotableActorRepresentation) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_GetDefaultMaterial = (d_GetDefaultMaterial)dm.CreateDelegate(typeof(d_GetDefaultMaterial));
      }
      return i_GetDefaultMaterial(rep);
    }
    public delegate Renderer d_GetRenderer(PilotableActorRepresentation rep, string name);
    private static d_GetRenderer i_GetRenderer = null;
    private static Renderer GetRenderer(PilotableActorRepresentation rep, string name) {
      if (i_GetRenderer == null) {
        MethodInfo method = typeof(PilotableActorRepresentation).GetMethod("GetRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("PilotableActorRepresentationGetRenderer", typeof(Renderer), new Type[] { typeof(PilotableActorRepresentation), typeof(string) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_GetRenderer = (d_GetRenderer)dm.CreateDelegate(typeof(d_GetRenderer));
      }
      return i_GetRenderer(rep, name);
    }

    private void ReconnectMissingMaterial() {
      if (this.framesCounted < this.framesToSkip) {
        ++this.framesCounted;
      } else {
        this.framesCounted = 0;
        if ((UnityEngine.Object)this.PooledPrefab == (UnityEngine.Object)null)
          return;
        if ((UnityEngine.Object)this.defaultMaterial == (UnityEngine.Object)null)
          this.defaultMaterial = GetDefaultMaterial(this.PooledPrefab);
        for (int index1 = 0; index1 < this.RendererList.Count; ++index1) {
          this.localRenderer = this.RendererList[index1];
          this.sharedMaterialsSource = this.localRenderer.sharedMaterials;
          for (int index2 = 0; index2 < this.sharedMaterialsSource.Length; ++index2) {
            if ((UnityEngine.Object)this.sharedMaterialsSource[index2] == (UnityEngine.Object)null) {
              this.pooledPrefabRenderer = GetRenderer(this.PooledPrefab, this.localRenderer.name);
              if ((UnityEngine.Object)this.pooledPrefabRenderer == (UnityEngine.Object)null || this.pooledPrefabRenderer.sharedMaterials == null || (index2 >= this.pooledPrefabRenderer.sharedMaterials.Length || (UnityEngine.Object)this.pooledPrefabRenderer.sharedMaterials[index2] == (UnityEngine.Object)null)) {
                if (!((UnityEngine.Object)this.defaultMaterial == (UnityEngine.Object)null)) {
                  this.sharedMaterialsCopy = this.localRenderer.sharedMaterials;
                  this.sharedMaterialsCopy[index2] = UnityEngine.Object.Instantiate<Material>(this.defaultMaterial);
                  this.localRenderer.sharedMaterials = this.sharedMaterialsCopy;
                }
              } else {
                this.sharedMaterialsCopy = this.localRenderer.sharedMaterials;
                this.sharedMaterialsCopy[index2] = UnityEngine.Object.Instantiate<Material>(this.pooledPrefabRenderer.sharedMaterials[index2]);
                this.localRenderer.sharedMaterials = this.sharedMaterialsCopy;
              }
            }
          }
        }
      }
    }
    protected virtual void _UpdateStateInfo() {
      if (isSlave) { return; }
      if (this.parentActor == null || this.parentActor.IsDead || !this.parentActor.Combat.TurnDirector.GameHasBegun)
        return;
      bool isUnsteady = this.parentActor.IsUnsteady;
      if (isUnsteady != this.wasUnsteadyLastFrame) {
        Mech parentActor = this.parentActor as Mech;
        if (isUnsteady && parentActor != null && !parentActor.IsOrWillBeProne)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "UNSTEADY", FloatieMessage.MessageNature.Debuff));
        else if (this.parentActor.Combat.Constants.CombatUIConstants.showStateRemovals)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "UNSTEADY", FloatieMessage.MessageNature.Removal));
      }
      this.wasUnsteadyLastFrame = isUnsteady;
      bool isEvasive = this.parentActor.IsEvasive;
      if (isEvasive != this.wasEvasiveLastFrame) {
        if (isEvasive)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "EVASIVE", FloatieMessage.MessageNature.Buff));
        else if (this.parentActor.Combat.Constants.CombatUIConstants.showStateRemovals || this.parentActor.EvasiveWasBroken || this.parentActor.EvasiveWasSuppressed) {
          if (this.parentActor.EvasiveWasSuppressed)
            this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "EVASIVE", FloatieMessage.MessageNature.Suppression));
          else
            this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "EVASIVE", FloatieMessage.MessageNature.Removal));
        }
      }
      this.wasEvasiveLastFrame = isEvasive;
      bool hasCover = this.parentActor.HasCover;
      if (hasCover != this.coverLastFrame) {
        if (hasCover) {
          if (this.parentActor.HasBulwarkAbility && !this.parentActor.GuardedWasBroken)
            this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "COVER (BULWARK)", FloatieMessage.MessageNature.Buff));
          else
            this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "COVER", FloatieMessage.MessageNature.Buff));
        } else if (this.parentActor.Combat.Constants.CombatUIConstants.showStateRemovals)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, Strings.T("COVER"), FloatieMessage.MessageNature.Removal));
      }
      this.coverLastFrame = hasCover;
      bool bracedLastRound = this.parentActor.BracedLastRound;
      if (bracedLastRound != this.guardedLastFrame && bracedLastRound) {
        if (this.parentActor.HasBulwarkAbility && !this.parentActor.GuardedWasBroken)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "GUARDED (BULWARK)", FloatieMessage.MessageNature.Buff));
        else
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "GUARDED", FloatieMessage.MessageNature.Buff));
      }
      this.guardedLastFrame = bracedLastRound;
      bool isEntrenched = this.parentActor.IsEntrenched;
      if (isEntrenched != this.wasEntrenchedLastFrame) {
        if (isEntrenched)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "ENTRENCHED", FloatieMessage.MessageNature.Buff));
        else if (this.parentActor.Combat.Constants.CombatUIConstants.showStateRemovals)
          this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentActor.GUID, this.parentActor.GUID, "ENTRENCHED", FloatieMessage.MessageNature.Removal));
      }
      this.wasEntrenchedLastFrame = isEntrenched;
    }

    protected virtual void PilotableActorRepresentation_LateUpdate() { }

    public override void SetVFXColliderEnabled(bool isEnabled) {
      GameRepresentation_SetVFXColliderEnabled(isEnabled);
      this.VFXCollider.gameObject.SetActive(isEnabled);
    }

    public virtual void _SetForcedPlayerVisibilityLevel(VisibilityLevel newVisibility, bool showUI = false) {
      this.forcedPlayerVisibilityLevel = new VisibilityLevel?(newVisibility);
      if (showUI)
        this.parentActor.IsForcedVisible = newVisibility == VisibilityLevel.LOSFull;
      this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new PlayerVisibilityChangedMessage(this.parentActor.GUID));
      this.OnPlayerVisibilityChanged(newVisibility);
    }

    public virtual void _ClearForcedPlayerVisibilityLevel(List<ICombatant> allCombatants) {
      this.forcedPlayerVisibilityLevel = new VisibilityLevel?();
      VisibilityLevel newLevel = !this.parentActor.Combat.HostilityMatrix.IsLocalPlayerFriendly(this.parentActor.TeamId) ? this.parentActor.Combat.LocalPlayerTeam.VisibilityToTarget((ICombatant)this.parentActor) : VisibilityLevel.LOSFull;
      this.parentActor.IsForcedVisible = false;
      this.parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new PlayerVisibilityChangedMessage(this.parentActor.GUID));
      this.OnPlayerVisibilityChanged(newLevel);
    }

    public virtual void PilotableActorRepresentation_OnPlayerVisibilityChanged(VisibilityLevel newLevel) {
      if (this.forcedPlayerVisibilityLevel.HasValue) { newLevel = this.forcedPlayerVisibilityLevel.Value; }
      if (newLevel == VisibilityLevel.LOSFull || newLevel == VisibilityLevel.BlipGhost) {
        this.VisibleObject.SetActive(true);
        if(this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
        if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
        this.mainCollider.enabled = true;
        if (this.parentActor.IsGhosted) {
          if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
          if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(true);
        } else {
          if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
          if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
        }
        if (!this._IsDead) {
          this.StartPersistentAudio();
          if (this.VisibleLights != null) {
            foreach (Behaviour visibleLight in this.VisibleLights)
              visibleLight.enabled = true;
          }
        }
        this._ResumeAllPersistentVFX();
      } else if (newLevel > VisibilityLevel.None) {
        if (!this._IsDead) {
          this.VisibleObject.SetActive(false);
          this._PauseAllPersistentVFX();
          this.StopPersistentAudio();
          this.mainCollider.enabled = true;
        }
        if (this.VisibleLights != null) {
          foreach (Behaviour visibleLight in this.VisibleLights)
            visibleLight.enabled = false;
        }
        this._SetBlipPositionRotation(this.transform.position, this.transform.rotation);
        if (this.parentActor.Combat.Constants.Visibility.GhostStateHidesBlips && this.parentActor.IsGhosted) {
          if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
          if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
          if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
          if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
          this.mainCollider.enabled = false;
        } else {
          bool flag = false;
          switch (newLevel) {
            case VisibilityLevel.Blip0Minimum:
            if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(true);
            if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
            if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
            if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
            flag = this.BlipObjectUnknown == null? false : this.BlipObjectUnknown.activeSelf;
            break;
            case VisibilityLevel.Blip1Type:
            case VisibilityLevel.Blip4Maximum:
            if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
            if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(true);
            if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
            if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
            flag = this.BlipObjectIdentified == null ? false : this.BlipObjectIdentified.activeSelf;
            break;
            default:
            GameRepresentation.initLogger.LogError((object)(this.parentActor.DisplayName + " OnPlayerVisibilityChanged got an invalid visLevel: " + (object)newLevel));
            break;
          }
          if (!flag) {
            int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_radar_blip_detected, WwiseManager.GlobalAudioObject);
          }
        }
      } else {
        if (!this._IsDead) {
          this.VisibleObject.SetActive(false);
          this.PauseAllPersistentVFX();
          this.StopPersistentAudio();
        }
        if (this.VisibleLights != null) {
          foreach (Behaviour visibleLight in this.VisibleLights)
            visibleLight.enabled = false;
        }
        this._SetBlipPositionRotation(this.transform.position, this.transform.rotation);
        if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
        if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
        if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
        if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
        if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
        this.mainCollider.enabled = false;
      }
      if (this._IsDead) {
        if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
        if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
        if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
        if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostWeak.SetActive(false);
      }
      if (this._IsDead == false) {
        if (this.weaponReps != null) {
          Log.Combat?.TWL(0, "CustomMechRepresentation.OnPlayerVisibilityChanged "+this.gameObject.name+" "+this.parentActor.PilotableActorDef.Description.Id+" team:"+this.parentActor.TeamId+" vislevel:"+newLevel+" weaponRepsCount:"+ this.weaponReps.Count);
          for (int index = 0; index < this.weaponReps.Count; ++index) {
            if (this.weaponReps[index] != null) {
              int mountedLocation = this.weaponReps[index].mountedLocation;
              ChassisLocations chassisLocations = (ChassisLocations)mountedLocation;
              bool isWeaponForceHide = false;
              if (isWeaponForceHide) {
                this.weaponReps[index].OnPlayerVisibilityChanged(VisibilityLevel.None);
                Log.Combat?.WL(1, this.weaponReps[index].name + ":" + VisibilityLevel.None);
              } else {
                Log.Combat?.WL(1, this.weaponReps[index].name+":"+newLevel);
                this.weaponReps[index].OnPlayerVisibilityChanged(newLevel);
              }
            }
          }
        }
        if (this.miscComponentReps != null) {
          for (int index = 0; index < this.miscComponentReps.Count; ++index) {
            if ((UnityEngine.Object)this.miscComponentReps[index] != (UnityEngine.Object)null)
              this.miscComponentReps[index].OnPlayerVisibilityChanged(newLevel);
          }
        }
      }
      LowVisibilityAPIHelper.LowVis_PilotableActorRepresentation_OnPlayerVisibilityChanged(this, newLevel);
    }

    public virtual void _PlayEjectFX() {
      if (this.rootParentRepresentation._BlipDisplayed) { return; }
      this.PlayVFX(1, (string)this.parentActor.Combat.Constants.VFXNames.pilotEject, false, Vector3.zero, true, -1f);
      if (isSlave == false) {
        AudioEventManager.PlayPilotVO(VOEvents.Pilot_Ejecting, this.parentActor);
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_eject>(AudioEventList_eject.eject_launch, this.audioObject);
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_eject>(AudioEventList_eject.eject_projectile, this.audioObject);
      }
    }

    public virtual bool PilotableActorRepresentation_RefreshSurfaceType(bool forceUpdate = false) {
      if (isSlave) { return true; }
      if (this.parentCombatant is AbstractActor) {
        List<MapEncounterLayerDataCell> encounterLayerCells = (this.parentCombatant as AbstractActor).occupiedEncounterLayerCells;
        if (encounterLayerCells != null && encounterLayerCells.Count > 0) {
          MapTerrainDataCell relatedTerrainCell = encounterLayerCells[0].relatedTerrainCell;
          AudioSwitch_surface_type audioSurfaceType = relatedTerrainCell.GetAudioSurfaceType();
          this.vfxNameModifier = relatedTerrainCell.GetVFXNameModifier();
          if (!forceUpdate && this.currentSurfaceType == audioSurfaceType)
            return false;
          WwiseManager.SetSwitch<AudioSwitch_surface_type>(audioSurfaceType, this.audioObject);
          WwiseManager.SetSwitch<AudioSwitch_under_water>(AudioSwitch_under_water.underwater_no, this.audioObject);
          bool flag1 = false;
          switch (audioSurfaceType) {
            case AudioSwitch_surface_type.dirt:
            this.terrainImpactParticleName = "dirt";
            break;
            case AudioSwitch_surface_type.metal:
            this.terrainImpactParticleName = "metal";
            break;
            case AudioSwitch_surface_type.snow:
            this.terrainImpactParticleName = "snow";
            break;
            case AudioSwitch_surface_type.wood:
            this.terrainImpactParticleName = "wood";
            break;
            case AudioSwitch_surface_type.brush:
            this.terrainImpactParticleName = "brush";
            break;
            case AudioSwitch_surface_type.concrete:
            this.terrainImpactParticleName = "concrete";
            break;
            case AudioSwitch_surface_type.debris_glass:
            this.terrainImpactParticleName = "debris_glass";
            break;
            case AudioSwitch_surface_type.gravel:
            this.terrainImpactParticleName = "gravel";
            break;
            case AudioSwitch_surface_type.ice:
            this.terrainImpactParticleName = "ice";
            break;
            case AudioSwitch_surface_type.lava:
            this.terrainImpactParticleName = "lava";
            break;
            case AudioSwitch_surface_type.mud:
            this.terrainImpactParticleName = "mud";
            break;
            case AudioSwitch_surface_type.sand:
            this.terrainImpactParticleName = "sand";
            break;
            case AudioSwitch_surface_type.water_deep:
            this.terrainImpactParticleName = "water";
            flag1 = true;
            WwiseManager.SetSwitch<AudioSwitch_under_water>(AudioSwitch_under_water.underwater_yes, this.audioObject);
            break;
            case AudioSwitch_surface_type.water_shallow:
            this.terrainImpactParticleName = "water";
            flag1 = true;
            break;
            default:
            this.terrainImpactParticleName = "dirt";
            break;
          }
          if (this.VisibleObject.activeSelf) {
            bool flag2 = this.currentSurfaceType == AudioSwitch_surface_type.water_deep || this.currentSurfaceType == AudioSwitch_surface_type.water_shallow;
            if (flag1 && !flag2) {
              string vfxName = "vfxPrfPrtl_mechFall_water_sm";
              if (this.parentActor is Mech) {
                vfxName = "vfxPrfPrtl_mechFall_water_lrg";
                this.PlayVFX(8, "vfxPrfSys_mechWaterRipple", true, Vector3.zero, false, -1f);
              }
              this.PlayVFXAt((Transform)null, this.thisTransform.position, vfxName, false, Vector3.zero, true, -1f);
              int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_water_enter, this.audioObject);
            } else if (!flag1 & flag2) {
              string vfxName = "vfxPrfPrtl_mechFall_water_sm";
              if (this.parentActor is Mech) {
                vfxName = "vfxPrfPrtl_mechFall_water_lrg";
                this.StopManualPersistentVFX("vfxPrfSys_mechWaterRipple");
              }
              this.PlayVFXAt((Transform)null, this.thisTransform.position, vfxName, false, Vector3.zero, true, -1f);
              int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_water_exit, this.audioObject);
            }
          }
          this.currentSurfaceType = audioSurfaceType;
        }
      }
      return true;
    }

    public virtual void PilotableRepresentation_HandleDeath(DeathMethod deathMethod, int location) {
      GameRepresentation_HandleDeath(deathMethod, location);
      if(this.BlipObjectUnknown != null) this.BlipObjectUnknown.SetActive(false);
      if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.SetActive(false);
      if (this.BlipObjectGhostWeak != null) this.BlipObjectGhostWeak.SetActive(false);
      if (this.BlipObjectGhostStrong != null) this.BlipObjectGhostStrong.SetActive(false);
      //if (this.parentActor.UnitType != UnitType.Mech) {
      if(this.parentActor.IsDead || this.parentActor.IsFlaggedForDeath) this.StopPersistentAudio();
      this._DestroyNearbyFlimsiesOnDeath();
      //}
      if (isSlave == false) {
        int numLivingUnits = this.parentActor.team.NumLivingUnits;
        if (numLivingUnits < 1) {
          if (!this.parentActor.team.IsEnemy(this.parentActor.Combat.LocalPlayerTeam)) { return; }
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "final_blow");
        } else {
          if (numLivingUnits != 1)
            return;
          if (this.parentActor.team.LocalPlayerControlsTeam) {
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "last_friendly_standing");
          } else {
            if (!this.parentActor.team.IsEnemy(this.parentActor.Combat.LocalPlayerTeam))
              return;
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "last_enemy_standing");
          }
        }
      }
    }

    public virtual void PilotableRepresentation_OnDeath() {
      GameRepresentation_OnDeath();
      //if (this.parentActor.UnitType != UnitType.Mech) { return; }
      this._DestroyNearbyFlimsiesOnDeath();
    }

    protected virtual void _DestroyNearbyFlimsiesOnDeath() {
      foreach (Collider collider in Physics.OverlapSphere(this.thisTransform.position, this.VFXCollider.radius * 4f)) {
        Vector3 normalized = (collider.transform.position - this.thisTransform.position).normalized;
        float num = 100f + this.parentActor.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
        DestructibleObject component1 = collider.gameObject.GetComponent<DestructibleObject>();
        DestructibleUrbanFlimsy component2 = collider.gameObject.GetComponent<DestructibleUrbanFlimsy>();
        if ((UnityEngine.Object)component1 != (UnityEngine.Object)null && component1.isFlimsy) {
          component1.TakeDamage(collider.transform.position, normalized, num);
          component1.Collapse(normalized, num);
        }
        if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
          component2.PlayDestruction(normalized, num);
      }
    }

    public virtual void PilotableRepresentation_OnCombatGameDestroyed() {
      GameRepresentation_OnCombatGameDestroyed();
      if (this.miscComponentReps != null)
        this.miscComponentReps.Clear();
      if (this.weaponReps != null)
        this.weaponReps.Clear();
      this.mechCustomization = (BattleTech.Rendering.MechCustomization.MechCustomization)null;
      if (this.rendererList != null)
        this.rendererList.Clear();
      this.sharedMaterialsSource = (Material[])null;
      this.sharedMaterialsCopy = (Material[])null;
    }

    public virtual ParticleSystem PilotableRepresentation_PlayVFXAt(Transform parentTransform,Vector3 offset,string vfxName,bool attached,Vector3 lookAtPos,bool oneShot,float duration) {
      return GameRepresentation_PlayVFXAt(parentTransform, offset, vfxName, attached, lookAtPos, oneShot, duration);
    }

    public override void FadeThrottleAudio(float startValue, float endValue, float duration) {
      if (isSlave) { return; }
      this.StopCoroutine("FadeThrottleCoroutine");
      this.StartCoroutine(this.FadeThrottleCoroutine(startValue, endValue, duration));
    }

    protected override IEnumerator FadeThrottleCoroutine(float startValue,float endValue,float duration) {
      PilotableActorRepresentation actorRepresentation = this;
      if ((double)duration <= 0.0) {
        WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, endValue, actorRepresentation.audioObject);
      } else {
        float t = 0.0f;
        float fadeRate = 1f / duration;
        while ((double)t < 1.0) {
          WwiseManager.SetRTPC<AudioRTPCList>(AudioRTPCList.bt_vehicle_speed, Mathf.Lerp(startValue, endValue, t), actorRepresentation.audioObject);
          t += fadeRate * Time.deltaTime;
          yield return (object)null;
        }
      }
    }

    public override void FaceTarget(bool isParellelSequence,ICombatant target,float twistTime,int stackItemUID,int sequenceId,bool isMelee, GameRepresentation.RotationCompleteDelegate completeDelegate) {
      GameRepresentation_FaceTarget(isParellelSequence, target, twistTime, stackItemUID, sequenceId, isMelee, completeDelegate);
      Vector3 position = target.CurrentPosition;
      if (target.GUID == this.parentCombatant.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(this.parentCombatant.GUID);
        if (terrainPos != null) {
          Log.Combat?.TWL(0, "terrain attack detected " + terrainPos.pos);
          position = terrainPos.pos;
        }
      }
      this.FacePoint(isParellelSequence, position, false, twistTime, stackItemUID, sequenceId, isMelee, completeDelegate);
    }

    public override void FacePoint(bool isParellelSequence,Vector3 lookAt,bool isLookVector,float twistTime,int stackItemUID,int sequenceId,bool isMelee,GameRepresentation.RotationCompleteDelegate completeDelegate) {
      GameRepresentation_FacePoint(isParellelSequence, lookAt, isLookVector, twistTime, stackItemUID, sequenceId, isMelee, completeDelegate);
      CustomTwistSequence actorTwistSequence = new CustomTwistSequence(this.parentActor, lookAt, isLookVector, isMelee, twistTime, stackItemUID, sequenceId, completeDelegate);
      if (isParellelSequence)
        this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddParallelSequenceToStackMessage((IStackSequence)actorTwistSequence));
      else
        this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)actorTwistSequence));
    }
    public override void ReturnToNeutralFacing(bool isParellelSequence,float twistTime,int stackItemUID,int sequenceId,GameRepresentation.RotationCompleteDelegate completeDelegate) {
      GameRepresentation_ReturnToNeutralFacing(isParellelSequence, twistTime, stackItemUID, sequenceId, completeDelegate);
      this.FacePoint(isParellelSequence, this.thisTransform.forward, true, twistTime, stackItemUID, sequenceId, false, completeDelegate);
    }
    public virtual void _SetBlipPositionRotation(Vector3 position, Quaternion rotation) {
      this.blipPendingPosition = position;
      this.blipPendingRotation = rotation;
      this.blipHasPendingPositionRotation = true;
    }
    public virtual void _SetBlipAlpha(float alpha) { this._SetHighlightAlpha(alpha); }
    public virtual bool _BlipDisplayed { get { return (this.BlipObjectUnknown != null?this.BlipObjectUnknown.activeSelf:false) || (this.BlipObjectIdentified != null?this.BlipObjectIdentified.activeSelf:false); } }
    protected virtual void _updateBlips() {
      this.timeNow = Time.time;
      if (this.BlipObjectUnknown.activeSelf && this.VisibleObject.activeSelf && !this.BlipObjectIdentified.activeSelf) {
        float height = Math.Min(this.GetVFXTransform((int)ChassisLocations.Head).transform.position.y + 20f, this.blipPendingPosition.y + 20f);
        Vector3 blipPendingPosition = this.blipPendingPosition;
        blipPendingPosition.y = height;
        this.blipPendingPosition = blipPendingPosition;
      }
      if ((double)this.timeNow - (double)this.blipLastUpdateTime > 1.0) {
        if (this.blipHasPendingPositionRotation) {
          this.BlipMoving = (double)(this.BlipObjectUnknown.transform.position - this.blipPendingPosition).magnitude > 0.100000001490116;
          if(this.BlipObjectUnknown != null) this.BlipObjectUnknown.transform.position = this.blipPendingPosition;
          if (this.BlipObjectUnknown != null) this.BlipObjectUnknown.transform.rotation = this.blipPendingRotation;
          if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.transform.position = this.blipPendingPosition;
          if (this.BlipObjectIdentified != null) this.BlipObjectIdentified.transform.rotation = this.blipPendingRotation;
          this.blipLastUpdateTime = this.timeNow;
          this.blipHasPendingPositionRotation = false;
          if (this._BlipDisplayed && this.BlipMoving) {
            int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_radar_blip_moving, this.audioObject);
          }
        } else
          this.BlipMoving = false;
      }
      this.elapsedTime = this.timeNow - this.blipLastUpdateTime;
      if (!this.BlipMoving) {
        this._SetBlipAlpha(1f);
      } else {
        this.blipAlpha = 1f;
        if ((double)this.elapsedTime < 0.100000001490116)
          this.blipAlpha = Mathf.Lerp(0.0f, 1f, this.elapsedTime / 0.1f);
        this.timeFromEnd = 1f - this.elapsedTime;
        if ((double)this.timeFromEnd < 0.400000005960464)
          this.blipAlpha = Mathf.Lerp(0.0f, 1f, this.timeFromEnd / 0.4f);
        this._SetBlipAlpha(this.blipAlpha);
      }
    }
  }
}