using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using DG.Tweening;
using Harmony;
using HBS;
using HBS.Data;
using InControl;
using Localize;
using MechResizer;
using SVGImporter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  /*[HarmonyPatch]
  public static class AssetBundleManager_RequestAsset {
    static MethodBase TargetMethod() {
      return typeof(AssetBundleManager).GetMethod("RequestAsset").MakeGenericMethod(typeof(GameObject));
    }
    public static void Postfix(string id) {
      Log.TWL(0, "AssetBundleManager.RequestAsset id:" + id);
    }
  }
  [HarmonyPatch]
  public static class AssetBundleTracker_LoadAsset {
    static MethodBase TargetMethod() {
      // refer to C# reflection documentation:
      return typeof(WeaponEffect).Assembly.GetType("BattleTech.Assetbundles.AssetBundleTracker").GetMethod("LoadAsset").MakeGenericMethod(typeof(GameObject));
    }
    private static PropertyInfo CurrentState = typeof(WeaponEffect).Assembly.GetType("BattleTech.Assetbundles.AssetBundleTracker").GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo AssetBundle = typeof(WeaponEffect).Assembly.GetType("BattleTech.Assetbundles.AssetBundleTracker").GetField("assetBundle", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Postfix(IDisposable __instance,string name,ref GameObject __result) {
      Log.TWL(0, "AssetBundleManager.LoadAsset name:" + name);
      Log.WL(1, "CurrentState:" + CurrentState.GetValue(__instance).ToString());
      AssetBundle bundle = AssetBundle.GetValue(__instance) as AssetBundle;
      Log.WL(1, "AssetBundle:" + (bundle != null?bundle.name:"null"));
      Log.WL(1, "result:" + (__result != null ? __result.name : "null"));
    }
  }
  public static class PrefabLoadRequest_Load {
    public static void Postfix(VersionManifestEntry ___manifestEntry, string ___resourceId) {
      Log.TWL(0, "PrefabLoadRequest.Load id:" + ___resourceId + " IsAssetBundled: " + ___manifestEntry.IsAssetBundled + ":" + ___manifestEntry.FilePath);
    }
  }
  public static class AssetBundleTracker_BuildObjectMap {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static void Postfix(Dictionary<System.Type, Dictionary<string, UnityEngine.Object>> ___loadedObjects, AssetBundle ___assetBundle) {
      try {
        Log.TWL(0, "AssetBundleTracker.BuildObjectMap bundle:" + ___assetBundle.name);
        foreach (var types in ___loadedObjects) {
          Log.WL(1, types.Key.ToString());
          foreach (var objects in types.Value) {
            Log.WL(2, objects.Key + "=" + objects.Value.name);
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }*/
  [HarmonyPatch(typeof(SGRoomController_MechBay))]
  [HarmonyPatch("NullWidgets")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SGRoomController_MechBay_NullWidgets {
    public static void PoolLoadedMechParts(this SGRoomController_MechBay instance, MechRepresentationSimGame loadedMech) {
      if ((UnityEngine.Object)loadedMech != (UnityEngine.Object)null) {
        GenericAnimatedComponent[] animatedComponents = loadedMech.GetComponentsInChildren<GenericAnimatedComponent>(true);
        Dictionary<GameObject, string> objects = new Dictionary<GameObject, string>();
        foreach (GenericAnimatedComponent component in animatedComponents) {
          if (objects.ContainsKey(component.gameObject) == false) {
            objects.Add(component.gameObject, component.PrefabName);
          }
        }
        foreach (var obj in objects) {
          obj.Key.transform.SetParent(null);
          instance.simState.DataManager.PoolGameObject(obj.Value, obj.Key);
        }
      }
    }
    public static void Prefix(SGRoomController_MechBay __instance, MechRepresentationSimGame ___loadedMech) {
      __instance.PoolLoadedMechParts(___loadedMech);
    }
  }
  [HarmonyPatch(typeof(SGRoomController_MechBay))]
  [HarmonyPatch("RemoveMech")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SGRoomController_MechBay_RemoveMech {
    public static void Prefix(SGRoomController_MechBay __instance, MechRepresentationSimGame ___loadedMech) {
      __instance.PoolLoadedMechParts(___loadedMech);
    }
  }
  [HarmonyPatch(typeof(SGRoomController_MechBay))]
  [HarmonyPatch("LoadMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(float), typeof(bool), typeof(bool) })]
  public static class SGRoomController_MechBay_LoadMech {
    private static FieldInfo f_loadedMech = typeof(SGRoomController_MechBay).GetField("loadedMech", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo f_mechBay = typeof(SGRoomController_MechBay).GetField("mechBay", BindingFlags.Instance | BindingFlags.NonPublic);
    private delegate void d_FitCameraToMech(SGRoomController_MechBay bay, GameObject mech, Camera camera);
    private static d_FitCameraToMech i_FitCameraToMech = null;
    private static PropertyInfo p_Unlocks = typeof(DataManager).GetProperty("Unlocks", BindingFlags.Instance | BindingFlags.NonPublic);
    public static bool Prepare() {
      {
        MethodInfo method = typeof(SGRoomController_MechBay).GetMethod("FitCameraToMech", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUFitCameraToMech", null, new Type[] { typeof(SGRoomController_MechBay), typeof(GameObject), typeof(Camera) }, typeof(SGRoomController_MechBay));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_FitCameraToMech = (d_FitCameraToMech)dm.CreateDelegate(typeof(d_FitCameraToMech));
      }
      Log.TWL(0, "SGRoomController_MechBay.Prepare p_Unlocks " + (p_Unlocks == null ? "null" : "not null"));
      return true;
    }
    public static AssetUnlocks Unlocks(this DataManager dataManager) { return (AssetUnlocks)p_Unlocks.GetValue(dataManager); }
    public static void FitCameraToMech(this SGRoomController_MechBay bay, GameObject mech, Camera camera) { i_FitCameraToMech(bay, mech, camera); }
    public static MechRepresentationSimGame loadedMech(this SGRoomController_MechBay mechBay) {
      return (MechRepresentationSimGame)f_loadedMech.GetValue(mechBay);
    }
    public static void loadedMech(this SGRoomController_MechBay mechBay, MechRepresentationSimGame value) {
      f_loadedMech.SetValue(mechBay, value);
    }
    public static MechBayPanel mechBay(this SGRoomController_MechBay mechBay) {
      return (MechBayPanel)f_mechBay.GetValue(mechBay);
    }
    public static void mechBay(this SGRoomController_MechBay mechBay, MechBayPanel value) {
      f_mechBay.SetValue(mechBay, value);
    }
    public static bool Prefix(SGRoomController_MechBay __instance, MechDef mechDef, float fadeDuration, bool useCameraFit, bool force, ref MechDef ___loadedMechDef) {
      if (!(___loadedMechDef != mechDef | force)) { return false; }
      ___loadedMechDef = mechDef;
      __instance.simState.CameraController.StartCoroutine(__instance.TransitionMech(mechDef, fadeDuration, __instance.simState.CameraController.simCamera, useCameraFit));
      return false;
    }
    public static void InitFromBattleRepresentation(this MechRepresentationSimGame bayRep, VehicleRepresentation battleRep) {
      bayRep.rootTransform = battleRep.thisTransform;
      bayRep.thisAnimator = battleRep.thisAnimator;
      bayRep.LeftArmAttach = battleRep.BodyAttach;
      bayRep.RightArmAttach = battleRep.BodyAttach;
      bayRep.TorsoAttach = battleRep.TurretAttach;
      bayRep.LeftLegAttach = battleRep.BodyAttach;
      bayRep.RightLegAttach = battleRep.BodyAttach;
      bayRep.vfxCenterTorsoTransform = battleRep.BodyAttach;
      bayRep.vfxLeftTorsoTransform = battleRep.BodyAttach;
      bayRep.vfxRightTorsoTransform = battleRep.BodyAttach;
      bayRep.vfxHeadTransform = battleRep.TurretLOS;
      bayRep.vfxLeftArmTransform = battleRep.BodyAttach;
      bayRep.vfxRightArmTransform = battleRep.BodyAttach;
      bayRep.vfxLeftLegTransform = battleRep.LeftSideLOS;
      bayRep.vfxRightLegTransform = battleRep.RightSideLOS;
      bayRep.vfxLeftShoulderTransform = battleRep.BodyAttach;
      bayRep.vfxRightShoulderTransform = battleRep.BodyAttach;
      bayRep.headDestructible = battleRep.vehicleDestructible;
      bayRep.centerTorsoDestructible = null;
      bayRep.leftTorsoDestructible = null;
      bayRep.rightTorsoDestructible = null;
      bayRep.leftArmDestructible = battleRep.vehicleDestructible;
      bayRep.rightArmDestructible = battleRep.vehicleDestructible;
      bayRep.leftLegDestructible = battleRep.vehicleDestructible;
      bayRep.rightLegDestructible = battleRep.vehicleDestructible;
      bayRep.mechCustomization = battleRep.GetComponentInChildren<BattleTech.Rendering.MechCustomization.MechCustomization>(true);
    }
    public static string GetSimGamePrefabName(this MechDef mechDef) {
      string arg = string.IsNullOrEmpty(mechDef.prefabOverride) ? mechDef.Chassis.PrefabBase : mechDef.prefabOverride.Replace("chrPrfMech_", "");
      UnitCustomInfo info = mechDef.GetCustomInfo();
      if (info != null) {
        if (info.quadVisualInfo.UseQuadVisuals) {
          arg = info.quadVisualInfo.RLegsPrefabBase;
        };
      }
      if (info != null) { if (info.SquadInfo.Troopers >= 1) { arg += "_squad"; } }
      if (info != null) { if (info.quadVisualInfo.UseQuadVisuals) { arg += "_quad"; } }
      string mechPrefabName = string.Format("chrPrfComp_{0}_simgame", arg);
      return mechPrefabName;
    }
    public static string GetSimGameBasePrefabName(this MechDef mechDef) {
      string arg = string.IsNullOrEmpty(mechDef.prefabOverride) ? mechDef.Chassis.PrefabBase : mechDef.prefabOverride.Replace("chrPrfMech_", "");
      UnitCustomInfo info = mechDef.GetCustomInfo();
      if(info != null) {
        if (info.quadVisualInfo.UseQuadVisuals) {
          arg = info.quadVisualInfo.RLegsPrefabBase;
        };
      }
      string mechPrefabName = string.Format("chrPrfComp_{0}_simgame", arg);
      return mechPrefabName;
    }
    private static void LoadPrefab(this SGRoomController_MechBay mechbay, MechDef mechDef) {
      string mechPrefabName = mechDef.GetSimGameBasePrefabName();
      mechbay.prefabsLoaded = false;
      LoadRequest loadRequest = mechbay.simState.DataManager.CreateLoadRequest(delegate (LoadRequest request) { mechbay.prefabsLoaded = true; }, false);
      if (!string.IsNullOrEmpty(mechDef.prefabOverride)) {
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechbay.simState.DataManager.Unlocks().GetBasePrefabIdForPrefabId(mechDef.Chassis.PrefabIdentifier), new bool?(false));
      }
      if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
        Log.TWL(0, "TransitionMech requesting battle game representation " + mechDef.ChassisID + " " + mechDef.Chassis.PrefabIdentifier);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.Chassis.PrefabIdentifier, new bool?(false));
        VehicleChassisDef vchassis = mechbay.simState.DataManager.VehicleChassisDefs.Get(mechDef.ChassisID);
        if (vchassis != null) { vchassis.AddCustomDeps(loadRequest); }
        vchassis.Refresh();
        mechDef.Refresh();
      } else {
        Log.TWL(0, "TransitionMech requesting bay game representation " + mechDef.ChassisID + " " + mechPrefabName);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechPrefabName, new bool?(false));
        mechDef.Chassis.AddCustomDeps(loadRequest);
        mechDef.Chassis.AddQuadDeps(loadRequest);
        mechDef.Refresh();
        if (mechDef.meleeWeaponRef.Def != null)
          if (string.IsNullOrEmpty(mechDef.meleeWeaponRef.prefabName) == false) loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.meleeWeaponRef.prefabName);
        if (mechDef.dfaWeaponRef.Def != null) {
          if (string.IsNullOrEmpty(mechDef.dfaWeaponRef.prefabName) == false) loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.dfaWeaponRef.prefabName);
        }
      }
      List<string> usedPrefabNames = new List<string>();
      Log.WL(1, "Requesting inventory: " + mechDef.Inventory.Length);
      for (int index = 0; index < mechDef.Inventory.Length; ++index) {
        if (mechDef.Inventory[index].Def != null) {
          MechComponentRef componentRef = mechDef.Inventory[index];
          string MountedLocation = componentRef.MountedLocation.ToString();
          bool correctLocation = false;
          if (mechDef.IsChassisFake()) {
            switch (componentRef.MountedLocation) {
              case ChassisLocations.LeftArm: MountedLocation = VehicleChassisLocations.Front.ToString(); correctLocation = true; break;
              case ChassisLocations.RightArm: MountedLocation = VehicleChassisLocations.Rear.ToString(); correctLocation = true; break;
              case ChassisLocations.LeftLeg: MountedLocation = VehicleChassisLocations.Left.ToString(); correctLocation = true; break;
              case ChassisLocations.RightLeg: MountedLocation = VehicleChassisLocations.Right.ToString(); correctLocation = true; break;
              case ChassisLocations.Head: MountedLocation = VehicleChassisLocations.Turret.ToString(); correctLocation = true; break;
            }
          } else {
            correctLocation = true;
          }
          Log.WL(1, "Component " + componentRef.Def.GetType().ToString() + ":" + componentRef.GetType().ToString() + " id:" + componentRef.Def.Description.Id + " loc:" + MountedLocation);
          if (correctLocation) {
            WeaponDef def = componentRef.Def as WeaponDef;
            Log.WL(2, "GetComponentPrefabName " + mechDef.Chassis.HardpointDataDef.ID + " base:" + mechDef.Chassis.PrefabBase + " loc:" + MountedLocation + " currPrefabName:" + componentRef.prefabName + " hasPrefab:" + componentRef.hasPrefabName + " hardpointSlot:" + componentRef.HardpointSlot);
            if (def != null) {
              string desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), componentRef.Def.PrefabIdentifier.ToLower(), def.WeaponCategoryValue.HardpointPrefabText);
              Log.WL(3, "desiredPrefabName:" + desiredPrefabName);
            } else {
              Log.WL(3, "");
            }
            //if (componentRef.hasPrefabName == false) {

            componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(mechDef.Chassis.HardpointDataDef, (BaseComponentRef)componentRef, mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), ref usedPrefabNames);
            componentRef.hasPrefabName = true;
            Log.WL(3, "effective prefab name:" + componentRef.prefabName);
            //} else {
            //usedPrefabNames.Add(componentRef.prefabName);
            //}
          }
          if (string.IsNullOrEmpty(componentRef.prefabName) == false) {
            loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, componentRef.prefabName);
            CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(componentRef.prefabName);
            if (customHardpoint != null) {
              if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
                loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, customHardpoint.shaderSrc);
              }
            }
          }
        }
      }
      loadRequest.ProcessRequests(1000u);
    }
    private static IEnumerator TransitionMech(this SGRoomController_MechBay mechbay, MechDef mechDef, float fadeDuration, Camera camera, bool useCameraFit) {
      float fadeRate = 0.01f;
      if (fadeDuration > 0f) {
        fadeRate = 1f / fadeDuration;
      }
      float t = 0f;
      bool spinRoom = mechbay.simState.CurRoomState == DropshipLocation.MECH_BAY && mechbay.simState.CameraController.mechLabSpin != null;
      if (spinRoom && mechbay.simState.CameraController.mechBay != null) {
        mechbay.simState.CameraController.mechBay.SetLightRotation(true);
      }
      if (fadeDuration > 0f) {
        while (t <= 1f) {
          float fade = Mathf.Lerp(1f, 0f, t);
          mechbay.simState.CameraController.postProcess.fade = fade;
          t += fadeRate * Time.deltaTime;
          if (spinRoom) {
            float y = Mathf.SmoothStep(0f, 2f, t);
            Quaternion localRotation = default(Quaternion);
            localRotation.eulerAngles = new Vector3(0f, y, 0f);
            mechbay.simState.CameraController.mechLabSpin.transform.localRotation = localRotation;
            if (mechbay.simState.CameraController.mechBay != null) {
              float lightBrightness = Mathf.Clamp01(t / 0.25f);
              mechbay.simState.CameraController.mechBay.SetLightBrightness(lightBrightness);
              mechbay.simState.CameraController.mechBay.SetLightRotation(true);
            }
          }
          yield return null;
        }
      }
      if (fadeDuration > 0f) {
        mechbay.simState.CameraController.postProcess.fade = 0f;
      }
      if (mechbay.loadedMech() != null) {
        mechbay.PoolLoadedMechParts(mechbay.loadedMech());
        mechbay.simState.DataManager.PoolGameObject(mechbay.loadedMech().prefabName, mechbay.loadedMech().gameObject);
        mechbay.loadedMech(null);
      }
      Transform mechAnchor = mechbay.simState.CameraController.MechAnchor;
      if (mechAnchor.childCount > 0) {
        Log.TWL(0, "MechBay.TransitionMech: MechAnchor still contains a gameObject after clearing loadedMech. Force-clearing transform hierarchy.", true);
        for (int i = mechAnchor.childCount - 1; i >= 0; i--) {
          UnityEngine.Object.Destroy(mechAnchor.GetChild(i).gameObject);
        }
        mechAnchor.DetachChildren();
      }
      if (mechDef != null) {
        mechbay.LoadPrefab(mechDef);
        while (!mechbay.prefabsLoaded) { yield return null; }
        string mechPrefabName = mechDef.GetSimGamePrefabName();
        GameObject gameObject = mechbay.simState.DataManager.PooledInstantiate(mechPrefabName, BattleTechResourceType.Prefab, null, null, null);
        if (gameObject == null) {
          mechPrefabName = mechDef.GetSimGameBasePrefabName();
          gameObject = mechbay.simState.DataManager.PooledInstantiate(mechPrefabName, BattleTechResourceType.Prefab, null, null, null);
        }
        MechRepresentationSimGame bayRepresentation = null;
        if (gameObject == null) {
          if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
            Log.TWL(0, "TransitionMech spawning battle game representation " + mechDef.ChassisID + " " + mechDef.Chassis.PrefabIdentifier);
            GameObject battleGameObject = mechbay.simState.DataManager.PooledInstantiate(mechDef.Chassis.PrefabIdentifier, BattleTechResourceType.Prefab, null, null, null);
            //Vehicle vehicle = ActorFactory.CreateVehicle(vDef, pilot, team.EncounterTags, null, Guid.NewGuid().ToString(), spawnerGUID, team.HeraldryDef);
            GameObject bayGameObject = GameObject.Instantiate(battleGameObject);
            //GameObject bayGameObject = battleGameObject;
            mechbay.simState.DataManager.PoolGameObject(mechDef.Chassis.PrefabIdentifier, battleGameObject);
            VehicleRepresentation battleRep = bayGameObject.GetComponent<VehicleRepresentation>();
            if (battleRep != null) {
              AkGameObj audio = battleRep.audioObject;
              battleRep.audioObject = null;
              battleRep.RegisterChassis(mechbay.simState.DataManager.VehicleChassisDefs.Get(mechDef.ChassisID));
              battleRep.Init(null, mechbay.simState.CameraController.MechAnchor, true);
              battleRep.audioObject = audio;
              bayRepresentation = bayGameObject.AddComponent<MechRepresentationSimGame>();
              bayRepresentation.InitFromBattleRepresentation(battleRep);
              bayRepresentation.thisAnimator = null;
              battleRep.BlipObjectIdentified.SetActive(false);
              battleRep.BlipObjectUnknown.SetActive(false);
              //mechbay.simState.DataManager.PoolGameObject(mechPrefabName, bayGameObject);
              gameObject = bayGameObject;
              Log.WL(1, "pooling MechRepresentationSimGame as " + mechPrefabName);
              //GameObject.Destroy(battleRep);
            } else {
              Log.WL(1, "no VehicleRepresentation");
              GameObject.Destroy(bayGameObject);
            }
            //gameObject = mechbay.simState.DataManager.PooledInstantiate(mechPrefabName, BattleTechResourceType.Prefab, null, null, null);
            Log.WL(1, "requested from pool " + mechPrefabName + " result:" + (gameObject == null ? "null" : "not null"));
          }
        }
        if (gameObject != null) {
          bayRepresentation = gameObject.GetComponent<MechRepresentationSimGame>();
          if (bayRepresentation != null) {
            mechbay.loadedMech(bayRepresentation);
            mechbay.loadedMech().Init(mechbay.simState.DataManager, mechDef, mechbay.simState.CameraController.MechAnchor, mechbay.simState.Player1sMercUnitHeraldryDef);

            Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
            for (int j = 0; j < componentsInChildren.Length; j++) {
              componentsInChildren[j].gameObject.layer = LayerMask.NameToLayer("Characters");
            }
            BTLightController.ResetAllShadowIndicies();
            if (mechDef.IsChassisFake()) {
              var sizeMultiplier = SizeMultiplier.Get(mechDef);
              bayRepresentation.rootTransform.localScale = sizeMultiplier;

              Log.WL(1, "scale as fake:" + sizeMultiplier);
              foreach (string tag in mechDef.MechTags) {
                Log.WL(2, tag);
              }
            } else {
              var sizeMultiplier = SizeMultiplier.Get(mechDef.Chassis);
              bayRepresentation.rootTransform.localScale = sizeMultiplier;
              Log.WL(1, "scale normal:" + sizeMultiplier);
              foreach (string tag in mechDef.Chassis.ChassisTags) {
                Log.WL(2, tag);
              }
            }
          }
        }
        mechbay.mechBay().RefreshPaintSelector();
        mechPrefabName = null;
      }
      bool camSet = false;
      if (useCameraFit && mechbay.loadedMech() != null && !mechbay.simState.CameraController.CameraMoving) {
        mechbay.FitCameraToMech(mechbay.loadedMech().gameObject, camera);
        camSet = true;
      }
      if (fadeDuration > 0f) {
        t = 0f;
        while (t <= 1f) {
          float fade2 = Mathf.Lerp(0f, 1f, t);
          mechbay.simState.CameraController.postProcess.fade = fade2;
          t += fadeRate * Time.deltaTime;
          if (spinRoom) {
            float y2 = Mathf.SmoothStep(-2f, 0f, t);
            Quaternion localRotation2 = default(Quaternion);
            localRotation2.eulerAngles = new Vector3(0f, y2, 0f);
            mechbay.simState.CameraController.mechLabSpin.transform.localRotation = localRotation2;
            if (mechbay.simState.CameraController.mechBay != null) {
              float lightBrightness2 = Mathf.Clamp01((1f - t) / 0.25f);
              mechbay.simState.CameraController.mechBay.SetLightBrightness(lightBrightness2);
            }
          }
          yield return null;
        }
        mechbay.simState.CameraController.postProcess.fade = 1f;
      }
      if (spinRoom) {
        mechbay.simState.CameraController.mechLabSpin.transform.localRotation = Quaternion.identity;
        if (mechbay.simState.CameraController.mechBay != null) {
          mechbay.simState.CameraController.mechBay.SetLightRotation(false);
          mechbay.simState.CameraController.mechBay.SetLightBrightness(0f);
        }
      }
      while (mechbay.simState.CameraController.CameraMoving) {
        yield return null;
      }
      if (useCameraFit && !camSet && mechbay.loadedMech() != null) {
        mechbay.FitCameraToMech(mechbay.loadedMech().gameObject, camera);
      }
      mechbay.simState.GetInterruptQueue().DisplayIfAvailable();
      yield break;
    }
  }
  public class MechBayPanelEx: MonoBehaviour{
    public GameObject baySwitch;
    public GameObject up;
    public GameObject down;
    public bool MechVehicleState;
    public int startRow;
  };
  public class MechBayUIUpButton : MechBayUIButton {
    public override void Init(GameObject source, string sourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      base.Init(source, sourceName, bays, parent);
      CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconUp, UnityGameInstance.BattleTechGame.DataManager);
      //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconUp); //panel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconUp, BattleTechResourceType.SVGAsset);
    }
    protected override void Upd() {
      if (this.icon.vectorGraphics == null) {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconUp, UnityGameInstance.BattleTechGame.DataManager);
      }
    }
    protected override void initUI() {
      if (parent == null) { return; }
      base.initUI();
      Vector3 desPos = this.transform.localPosition;
      desPos.y = 0f-this.gameObject.GetComponent<RectTransform>().sizeDelta.y;
      this.transform.localPosition = desPos;
    }
    public override void OnClick() {
      base.OnClick();
      if (parent.startRow > 0) { parent.startRow -= 1; }
      if (parent.MechVehicleState) {
        mechBays.ShowVehicleBays(parent.startRow);
      } else {
        mechBays.ShowMechBays(parent.startRow);
      }
    }
  }
  public class MechPopupUIUpButton : MechBayUIButton {
    public override void Init(GameObject source, string sourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      base.Init(source, sourceName, bays, parent);
      CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconUp, UnityGameInstance.BattleTechGame.DataManager);
      //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconUp); //panel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconUp, BattleTechResourceType.SVGAsset);
    }
    protected override void Upd() {
      if (this.icon.vectorGraphics == null) {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconUp, UnityGameInstance.BattleTechGame.DataManager);
      }
    }
    protected override void initUI() {
      if (parent == null) { return; }
      base.initUI();
      Vector3 desPos = this.transform.localPosition;
      desPos.y = 0f - this.gameObject.GetComponent<RectTransform>().sizeDelta.y;
      if (desPos.y < 0f) { desPos.y = 0f; }
      this.transform.localPosition = desPos;
    }
    public override void OnClick() {
      base.OnClick();
      if (parent.startRow > 0) { parent.startRow -= 1; }
      if (parent.MechVehicleState) {
        mechBays.ShowVehicleBays(parent.startRow);
      } else {
        mechBays.ShowMechBays(parent.startRow);
      }
    }
  }
  public class MechBayUIDownButton : MechBayUIButton {
    public override void Init(GameObject source, string sourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      base.Init(source, sourceName, bays, parent);
      CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconDown, UnityGameInstance.BattleTechGame.DataManager);
      //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconDown);//panel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconDown, BattleTechResourceType.SVGAsset);
    }
    protected override void Upd() {
      if (this.icon.vectorGraphics == null) {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconDown, UnityGameInstance.BattleTechGame.DataManager);
      }
    }
    protected override void initUI() {
      if (parent == null) { return; }
      base.initUI();
      Vector3 desPos = this.transform.localPosition;
      desPos.y = 0f - this.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.y;
      this.transform.localPosition = desPos;
    }
    public override void OnClick() {
      base.OnClick();
      if (parent.MechVehicleState) {
        if ((parent.startRow + mechBays.RowsPerList()) < (mechBays.VehicleBaysCount())) { parent.startRow += 1; }
        mechBays.ShowVehicleBays(parent.startRow);
      } else {
        if ((parent.startRow + mechBays.RowsPerList()) < (mechBays.MechBaysCount())) { parent.startRow += 1; }
        mechBays.ShowMechBays(parent.startRow);
      }
    }
  }
  public class MechPopupUIDownButton : MechBayUIButton {
    public override void Init(GameObject source, string sourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      base.Init(source, sourceName, bays, parent);
      CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconDown, UnityGameInstance.BattleTechGame.DataManager);
      //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconDown);//panel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconDown, BattleTechResourceType.SVGAsset);
    }
    protected override void Upd() {
      if (this.icon.vectorGraphics == null) {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconDown, UnityGameInstance.BattleTechGame.DataManager);
      }
    }
    protected override void initUI() {
      if (parent == null) { return; }
      base.initUI();
      Vector3 desPos = this.transform.localPosition;
      desPos.y = 0f - this.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.y;
      if (desPos.y <= 0f) { desPos.y = this.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.y; }
      this.transform.localPosition = desPos;
    }
    public override void OnClick() {
      base.OnClick();
      if (parent.MechVehicleState) {
        if ((parent.startRow + mechBays.RowsPerList()) < (mechBays.VehicleBaysCount())) { parent.startRow += 1; }
        mechBays.ShowVehicleBays(parent.startRow);
      } else {
        if ((parent.startRow + mechBays.RowsPerList()) < (mechBays.MechBaysCount())) { parent.startRow += 1; }
        mechBays.ShowMechBays(parent.startRow);
      }
    }
  }
  public class MechBayUISwitch : MechBayUIButton {
    public override void Init(GameObject source, string sourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      base.Init(source, sourceName, bays, parent);
      CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconMech, UnityGameInstance.BattleTechGame.DataManager);
        //CustomSvgCache.get(Core.Settings.MechBaySwitchIconMech);//panel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconMech, BattleTechResourceType.SVGAsset);
      parent.MechVehicleState = false;
    }
    protected override void Upd() {
      if(this.icon.vectorGraphics == null) {
        if (parent.MechVehicleState) {
          CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconVehicle, UnityGameInstance.BattleTechGame.DataManager);
        } else {
          CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconMech, UnityGameInstance.BattleTechGame.DataManager);
        }
      }
    }
    public override void OnClick() {
      base.OnClick();
      parent.MechVehicleState = !parent.MechVehicleState;
      parent.startRow = 0;
      if (parent.MechVehicleState) {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconVehicle, UnityGameInstance.BattleTechGame.DataManager);
        //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconVehicle);//mechBayPanel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconVehicle, BattleTechResourceType.SVGAsset);
        mechBays.ShowVehicleBays(parent.startRow);
      } else {
        CustomSvgCache.setIcon(this.icon, Core.Settings.MechBaySwitchIconMech, UnityGameInstance.BattleTechGame.DataManager);
        //this.icon.vectorGraphics = CustomSvgCache.get(Core.Settings.MechBaySwitchIconMech);//mechBayPanel.DataManager.GetObjectOfType<SVGAsset>(Core.Settings.MechBaySwitchIconMech, BattleTechResourceType.SVGAsset);
        mechBays.ShowMechBays(parent.startRow);
      }
    }
  }
  public class MechBayUIButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
    public SVGImage icon;
    public GameObject source;
    public string sizeSourceName;
    public MechBayRowGroupWidget mechBays;
    public MechBayPanelEx parent;
    private bool ui_inited;
    public MechBayUIButton() {
      ui_inited = false;
    }
    public virtual void Init(GameObject source, string sizeSourceName, MechBayRowGroupWidget bays, MechBayPanelEx parent) {
      this.source = source;
      this.sizeSourceName = sizeSourceName;
      this.mechBays = bays;
      this.parent = parent;
      this.icon = gameObject.GetComponent<SVGImage>();
      this.icon.vectorGraphics = null;
    }
    protected virtual void initUI() {
      if (source == null) { return; } //"uixPrfBttn_BASE_TabMedium-bays"
      RectTransform uixPrfBttn_BASE_TabMedium_bays = source.transform.FindRecursive(sizeSourceName) as RectTransform;
      RectTransform rectTr = this.gameObject.GetComponent<RectTransform>();
      Vector2 size = uixPrfBttn_BASE_TabMedium_bays.sizeDelta;
      if (size.y < 0f) { size.y = 25f; }
      size.x = size.y;
      rectTr.sizeDelta = size;
      icon.color = Color.white;
      ui_inited = true;
    }
    protected virtual void Upd() {

    }
    public void Update() {
      if (ui_inited == false) {
        initUI();
      } else {
        Upd();
      }
    }
    public virtual void OnClick() {

    }
    public void OnPointerClick(PointerEventData eventData) {
      //throw new NotImplementedException();
      OnClick();
    }

    public void OnPointerEnter(PointerEventData eventData) {
      icon.color = UIManager.Instance.UIColorRefs.orange;
    }

    public void OnPointerExit(PointerEventData eventData) {
      icon.color = UIManager.Instance.UIColorRefs.white;
    }
  }
  //public class MechBayIconButton: Event
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("ViewBays")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayPanel_ViewBays {
    private static int toggleState = 0;
    //private static int bayListCount = 4;
    public static void set_toggleState(this MechBayPanel panel, int state) {
      toggleState = state;
      Transform uixPrfBttn_BASE_TabMedium_bays = panel.transform.FindRecursive("uixPrfBttn_BASE_TabMedium-bays");
      if (uixPrfBttn_BASE_TabMedium_bays != null) {
        Transform tab_text = uixPrfBttn_BASE_TabMedium_bays.FindRecursive("tab_text");
        if (tab_text != null) {
          LocalizableText text = tab_text.gameObject.GetComponent<LocalizableText>();
          if (toggleState >= panel.MechBaysCount()) {
            if (text != null) { text.SetText(new Text("__/V.BAYS/__ {0}-{1}", (toggleState - panel.MechBaysCount() + 1), (toggleState - panel.MechBaysCount() + panel.RowsPerList()))); }
          } else {
            if (text != null) { text.SetText(new Text("__/BAYS/__ {0}-{1}", (toggleState + 1), (toggleState + panel.RowsPerList()))); }
          }
        }
      }
    }
    private static GameObject InstantineButton(GameObject src,Transform parent) {
      GameObject baySwitch = GameObject.Instantiate(src);
      baySwitch.transform.SetParent(parent);
      GameObject.Destroy(baySwitch.GetComponent<HBSDOTweenToggle>());
      GameObject.Destroy(baySwitch.GetComponent<HBSTooltip>());
      DOTweenAnimation[] animations = baySwitch.GetComponents<DOTweenAnimation>();
      foreach (DOTweenAnimation animation in animations) {
        GameObject.Destroy(animation);
      }
      Transform tab_bg = baySwitch.transform.FindRecursive("tab_bg");
      GameObject.Destroy(tab_bg.gameObject);
      Transform tab_text = baySwitch.transform.FindRecursive("tab_text");
      GameObject.Destroy(tab_text.gameObject);
      Transform tab_border = baySwitch.transform.FindRecursive("tab_border");
      GameObject.Destroy(tab_border.gameObject);
      baySwitch.transform.localPosition = Vector3.zero;
      return baySwitch;
    }
    public static void Postfix(MechBayPanel __instance, MechBayRowGroupWidget ___bayGroupWidget) {
      if (Core.Settings.ShowVehicleBays == false) { return; }
      MechBayPanelEx panelEx = __instance.gameObject.GetComponent<MechBayPanelEx>();
      RectTransform uixPrfBttn_BASE_TabMedium_bays = __instance.gameObject.transform.FindRecursive("uixPrfBttn_BASE_TabMedium-bays") as RectTransform;
      if (panelEx == null) {
        GameObject baySwitch = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, ___bayGroupWidget.transform);
        baySwitch.name = "MechBaySwitchButton";
        baySwitch.transform.localPosition = Vector3.zero;

        GameObject upBtn = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, ___bayGroupWidget.transform);
        upBtn.name = "MechBayUpButton";
        upBtn.transform.localPosition = Vector3.zero;

        GameObject downBtn = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, ___bayGroupWidget.transform);
        downBtn.name = "MechBayDownButton";
        downBtn.transform.localPosition = Vector3.zero;

        panelEx = __instance.gameObject.AddComponent<MechBayPanelEx>();
        panelEx.baySwitch = baySwitch;
        panelEx.up = upBtn;
        panelEx.down = downBtn;

        panelEx.MechVehicleState = false;
        panelEx.startRow = 0;

        baySwitch.AddComponent<MechBayUISwitch>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
        upBtn.AddComponent<MechBayUIUpButton>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
        downBtn.AddComponent<MechBayUIDownButton>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
      } else {
        GameObject baySwitch = panelEx.baySwitch;
        panelEx.MechVehicleState = false;
        panelEx.startRow = 0;
        panelEx.baySwitch.GetComponent<MechBayUISwitch>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
        panelEx.up.GetComponent<MechBayUIUpButton>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
        panelEx.down.GetComponent<MechBayUIDownButton>().Init(__instance.gameObject, "uixPrfBttn_BASE_TabMedium-bays", ___bayGroupWidget, panelEx);
      }
      ___bayGroupWidget.ShowMechBays(0);
    }
  }
  [HarmonyPatch(typeof(MechPlacementPopup))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameState), typeof(MechDef) })]
  public static class MechPlacementPopup_SetData {
    private static GameObject InstantineButton(GameObject src, Transform parent) {
      GameObject baySwitch = GameObject.Instantiate(src);
      baySwitch.transform.SetParent(parent);
      return baySwitch;
    }
    public static void Postfix(MechPlacementPopup __instance, MechBayRowGroupWidget ___rowGroupWidget) {
      if (Core.Settings.ShowVehicleBays == false) { return; }
      MechBayPanelEx panelEx = __instance.gameObject.GetComponent<MechBayPanelEx>();
      RectTransform uixPrfBttn_BASE_TabMedium_bays = __instance.gameObject.transform.FindRecursive("iconBttn_icon") as RectTransform;
      if (panelEx == null) {
        Transform Representation = ___rowGroupWidget.transform.FindRecursive("Representation");
        GameObject baySwitch = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, Representation);
        baySwitch.name = "MechBaySwitchButton";
        baySwitch.transform.localPosition = Vector3.zero;
        baySwitch.SetActive(false);

        GameObject upBtn = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, Representation);
        upBtn.name = "MechBayUpButton";
        upBtn.transform.localPosition = Vector3.zero;
        upBtn.transform.localScale = Vector3.one;

        GameObject downBtn = InstantineButton(uixPrfBttn_BASE_TabMedium_bays.gameObject, Representation);
        downBtn.name = "MechBayDownButton";
        downBtn.transform.localPosition = Vector3.zero;
        RectTransform downBtnRT = downBtn.GetComponent<RectTransform>();
        downBtnRT.pivot = new Vector2(0.5f, 2f);
        downBtn.transform.localScale = Vector3.one;

        panelEx = __instance.gameObject.AddComponent<MechBayPanelEx>();
        panelEx.baySwitch = baySwitch;
        panelEx.up = upBtn;
        panelEx.down = downBtn;

        panelEx.MechVehicleState = false;
        panelEx.startRow = 0;

        baySwitch.AddComponent<MechBayUISwitch>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
        upBtn.AddComponent<MechPopupUIUpButton>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
        downBtn.AddComponent<MechPopupUIDownButton>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
      } else {
        GameObject baySwitch = panelEx.baySwitch;
        panelEx.MechVehicleState = false;
        panelEx.startRow = 0;
        panelEx.baySwitch.GetComponent<MechBayUISwitch>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
        panelEx.up.GetComponent<MechPopupUIUpButton>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
        panelEx.down.GetComponent<MechPopupUIDownButton>().Init(__instance.gameObject, "iconBttn_icon", ___rowGroupWidget, panelEx);
      }
      ___rowGroupWidget.ShowMechBays(0);
    }
  }
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<MechComponentRef>) })]
  public static class MechBayPanel_SetData {
    public static void Prefix(MechBayPanel __instance, List<MechComponentRef> inventory) {
      Log.TWL(0, "MechBayPanel.SetData ");
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GetPilotFullExpertise")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Pilot) })]
  public static class SimGameState_GetPilotFullExpertise {
    //private static bool toggleState = false;
    public static void Postfix(SimGameState __instance, Pilot p, ref string __result) {
      Log.TWL(0, "SimGameState.GetPilotFullExpertise " + p.Description.Id);
      string add_str = "";
      if (p.pilotDef.canPilotMech()) {
        add_str = "M";
      }
      if (p.pilotDef.canPilotVehicle()) {
        add_str += "V";
      }
      __result += " (" + add_str + ")";
    }
  }
  [HarmonyPatch(typeof(PilotGenerator))]
  [HarmonyPatch("GeneratePilots")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotGenerator_GeneratePilots {
    public static void Callsign(this HumanDescriptionDef descr, string value) {
      typeof(HumanDescriptionDef).GetProperty("Callsign", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(descr, new object[1] { value });
    }
    public static void Details(this BaseDescriptionDef descr, string value) {
      typeof(BaseDescriptionDef).GetProperty("Details", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(descr, new object[1] { value });
    }
    public static void Postfix(PilotGenerator __instance, ref List<PilotDef> __result) {
      Log.TWL(0, "PilotGenerator.GeneratePilots");
      if (__result == null) { return; }
      if (__result.Count == 0) { return; }
      int vehiclePilotsCount = 0;
      foreach (PilotDef pilot in __result) {
        if (vehiclePilotsCount > Core.Settings.MaxVehicleRandomPilots) { break; }
        float roll = UnityEngine.Random.Range(0f, 1f);
        Log.WL(1, "First roll:"+roll);
        if (roll < Core.Settings.CanPilotVehicleProbability) {
          ++vehiclePilotsCount;
          roll = UnityEngine.Random.Range(0f, 1f);
          pilot.PilotTags.Add(Core.Settings.CanPilotVehicleTag);
          Log.WL(1, "Second roll:" + roll);
          if (roll < Core.Settings.CanPilotAlsoMechProbability) {
            pilot.Description.Details(Core.Settings.CanPilotBothDescription + pilot.Description.Details);
          } else {
            pilot.Description.Details(Core.Settings.CanPilotVehicleDescription + pilot.Description.Details);
            pilot.PilotTags.Add(Core.Settings.CannotPilotMechTag);
            pilot.SetUnspentExperience(0);
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GetMaxActiveMechs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_GetMaxActiveMechs {
    //private static bool toggleState = false;
    public static void Postfix(SimGameState __instance, ref int __result) {
      Log.TWL(0, "SimGameState.GetMaxActiveMechs "+__instance.CurDropship);
      if(__instance.CurDropship != DropshipType.Leopard) {
        if (Core.Settings.BaysCountExternalControl == false) {
          __result += Core.Settings.ArgoBaysFix * __instance.Constants.Story.MaxMechsPerPod;
        }
      }
    }
  }
  [HarmonyPatch(typeof(MechBayRowGroupWidget))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDropTarget), typeof(SimGameState) })]
  public static class MechBayRowGroupWidget_SetData {
    //private static Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>> mechsBays = new Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>>();
    //private static Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>> vehiclesBays = new Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>>();
    public static void ShowMechBays(this MechBayRowGroupWidget bays, int start_index) {
      bays.ShowBays(start_index);
    }
    public static void ShowBays(this MechBayRowGroupWidget bays, int start_index) {
      int startindex = start_index;
      int endindex = start_index + bays.RowsPerList();
      Log.TWL(0,"ShowBays:"+startindex+"->"+endindex+" all:"+ bays.Bays.Length);
      for (int index = 0; index < bays.Bays.Length; ++index) {
        if ((index >= startindex) && (index < endindex)) { bays.Bays[index].gameObject.SetActive(true); } else {
          bays.Bays[index].gameObject.SetActive(false);
        }
      }
    }
    public static void ShowVehicleBays(this MechBayRowGroupWidget bays, int start_index) {
      bays.ShowBays(start_index+bays.MechBaysCount());
    }
    private static void InstantineBays(MechBayRowGroupWidget __instance, MechBayPanel dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      __state = new List<MechBayRowWidget>();
      __state.AddRange(___bays);
      int fullBaysCount = __instance.MechBaysCount() + __instance.VehicleBaysCount();
      MechBayRowWidget srcBay = ___bays[0];
      for (int index = ___bays.Length; index < fullBaysCount; ++index) {
        MechBayRowWidget newBay = GameObject.Instantiate(srcBay).GetComponent<MechBayRowWidget>();
        newBay.gameObject.transform.SetParent(srcBay.transform.parent);
        newBay.transform.localScale = srcBay.transform.localScale;
        newBay.Init();
        foreach (var mechEl in newBay.MechList) { mechEl.Init(); }
        newBay.gameObject.SetActive(false);
        __state.Add(newBay);
      }
      ___bays = __state.ToArray();
      Log.WL(1, "->" + ___bays.Length);
    }
    private static void InstantineBays(MechBayRowGroupWidget __instance, MechPlacementPopup dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      __state = new List<MechBayRowWidget>();
      __state.AddRange(___bays);
      MechDef purchasedMech = Traverse.Create(dropParent).Field<MechDef>("purchasedMech").Value;
      ChassisDef purchasedChassis = Traverse.Create(dropParent).Field<ChassisDef>("purchasedChassis").Value;
      bool showVehicles = false;
      if (purchasedMech != null) { showVehicles = purchasedMech.IsChassisFake(); } else
        if (purchasedChassis != null) { showVehicles = purchasedChassis.IsFake(purchasedChassis.Description.Id); };
      int fullBaysCount = showVehicles == false ? __instance.MechBaysCount() : __instance.VehicleBaysCount();
      MechBayRowWidget srcBay = ___bays[0];
      for (int index = ___bays.Length; index < fullBaysCount; ++index) {
        MechBayRowWidget newBay = GameObject.Instantiate(srcBay).GetComponent<MechBayRowWidget>();
        newBay.gameObject.transform.SetParent(srcBay.transform.parent);
        newBay.gameObject.transform.SetSiblingIndex(srcBay.transform.GetSiblingIndex() + __state.Count);
        newBay.transform.localScale = srcBay.transform.localScale;
        newBay.Init();
        foreach (var mechEl in newBay.MechList) { mechEl.Init(); }
        newBay.gameObject.SetActive(false);
        __state.Add(newBay);
      }
      ___bays = __state.ToArray();
      Log.WL(1, "->" + ___bays.Length);
    }
    public static bool Prefix(MechBayRowGroupWidget __instance, IMechLabDropTarget dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      Log.TWL(0, "MechBayRowGroupWidget.SetData prefix " + ___bays.Length + " parent:"+ dropParent.GetType().ToString());
      MechBayPanel mechBayPanel = dropParent as MechBayPanel;
      MechPlacementPopup mechPlacementPopup = dropParent as MechPlacementPopup;
      if (mechBayPanel != null) {
        InstantineBays(__instance,mechBayPanel,sim,ref ___bays, ref __state);
      }else if(mechPlacementPopup != null) {
        InstantineBays(__instance, mechPlacementPopup, sim, ref ___bays, ref __state);
      }
      return false;
    }
    private static void FillBays(MechBayRowGroupWidget __instance, MechBayPanel dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      if (___bays.Length < __state.Count) { ___bays = __state.ToArray(); };
      int maxMechsPerPod = sim.Constants.Story.MaxMechsPerPod;
      int maxActiveMechs = sim.GetMaxActiveMechs();
      int fullBaysCount = __instance.MechBaysCount() + __instance.VehicleBaysCount();
      int baySlotStart = 0;
      int baySlotEnd = maxMechsPerPod;
      for (int mechBayIndex = 0; mechBayIndex < __instance.MechBaysCount(); ++mechBayIndex) {
        ___bays[mechBayIndex].SetData(dropParent, sim, string.Format("Bay {0}", (mechBayIndex + 1)), maxActiveMechs > baySlotStart, baySlotStart, baySlotEnd);
        int slot = 0;
        for (int key = baySlotStart; key < baySlotEnd; ++key) {
          MechDef mechDef = (MechDef)null;
          if (!sim.ActiveMechs.TryGetValue(key, out mechDef))
            sim.ReadyingMechs.TryGetValue(key, out mechDef);
          bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
          bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
          bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
          ___bays[mechBayIndex].SetMech(slot, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
          ++slot;
        }
        baySlotStart += maxMechsPerPod;
        baySlotEnd += maxMechsPerPod;
      }
      baySlotStart = sim.VehicleShift();
      baySlotEnd = sim.VehicleShift() + maxMechsPerPod;
      for (int vehicleBayIndex = __instance.MechBaysCount(); vehicleBayIndex < fullBaysCount; ++vehicleBayIndex) {
        ___bays[vehicleBayIndex].SetData(dropParent, sim, string.Format("V.Bay {0}", (vehicleBayIndex - __instance.MechBaysCount() + 1)), maxActiveMechs > (baySlotStart - sim.VehicleShift()), baySlotStart, baySlotEnd);
        int slot = 0;
        for (int key = baySlotStart; key < baySlotEnd; ++key) {
          MechDef mechDef = (MechDef)null;
          if (!sim.ActiveMechs.TryGetValue(key, out mechDef))
            sim.ReadyingMechs.TryGetValue(key, out mechDef);
          bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
          bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
          bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
          ___bays[vehicleBayIndex].SetMech(slot, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
          ++slot;
        }
        baySlotStart += maxMechsPerPod;
        baySlotEnd += maxMechsPerPod;
      }
    }
    private static void FillBays(MechBayRowGroupWidget __instance, MechPlacementPopup dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      if (___bays.Length < __state.Count) { ___bays = __state.ToArray(); };
      int maxMechsPerPod = sim.Constants.Story.MaxMechsPerPod;
      int maxActiveMechs = sim.GetMaxActiveMechs();
      MechDef purchasedMech = Traverse.Create(dropParent).Field<MechDef>("purchasedMech").Value;
      ChassisDef purchasedChassis = Traverse.Create(dropParent).Field<ChassisDef>("purchasedChassis").Value;
      bool showVehicles = false;
      if (purchasedMech != null) { showVehicles = purchasedMech.IsChassisFake(); } else
        if (purchasedChassis != null) { showVehicles = purchasedChassis.IsFake(purchasedChassis.Description.Id); };
      int fullBaysCount = showVehicles == false ? __instance.MechBaysCount() : __instance.VehicleBaysCount();
      int baySlotStart = showVehicles == false ? 0 : sim.VehicleShift();
      int baySlotEnd = showVehicles == false ? maxMechsPerPod : sim.VehicleShift() + maxMechsPerPod;
      for (int bayIndex = 0; bayIndex < fullBaysCount; ++bayIndex) {
        ___bays[bayIndex].SetData(dropParent, sim, string.Format(showVehicles == false ? "Bay {0}" :"V.Bay {0}", (bayIndex + 1)), maxActiveMechs > (baySlotStart - (showVehicles == false ? 0 : sim.VehicleShift())), baySlotStart, baySlotEnd);
        int slot = 0;
        for (int key = baySlotStart; key < baySlotEnd; ++key) {
          MechDef mechDef = (MechDef)null;
          if (!sim.ActiveMechs.TryGetValue(key, out mechDef))
            sim.ReadyingMechs.TryGetValue(key, out mechDef);
          bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
          bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
          bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
          ___bays[bayIndex].SetMech(slot, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
          ++slot;
        }
        baySlotStart += maxMechsPerPod;
        baySlotEnd += maxMechsPerPod;
      }
    }
    public static void Postfix(MechBayRowGroupWidget __instance, IMechLabDropTarget dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      Log.TWL(0, "MechBayRowGroupWidget.SetData postfix " + ___bays.Length + "/" + __state.Count + " parent:" + dropParent.GetType().ToString());
      MechBayPanel mechBayPanel = dropParent as MechBayPanel;
      MechPlacementPopup mechPlacementPopup = dropParent as MechPlacementPopup;
      if (mechBayPanel != null) {
        FillBays(__instance, mechBayPanel, sim, ref ___bays, ref __state);
      } else if (mechPlacementPopup != null) {
        FillBays(__instance, mechPlacementPopup, sim, ref ___bays, ref __state);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddMechs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<string>) })]
  public static class SimGameState_AddMechs_strings {
    public static bool Prefix(SimGameState __instance, ref List<string> startingMechs) {
      Log.TWL(0, "SimGameState.AddMechs");
      List<MechDef> mechs = new List<MechDef>();
      List<MechDef> vehicles = new List<MechDef>();
      for (int idx = 0; idx < startingMechs.Count; ++idx) {
        MechDef mech = new MechDef(__instance.DataManager.MechDefs.Get(startingMechs[idx]), __instance.GenerateSimGameUID(), true);
        if (mech.IsChassisFake() == false) {
          mechs.Add(mech);
        } else {
          vehicles.Add(mech);
        }
      }
      for (int idx = 0; idx < mechs.Count; ++idx) {
        __instance.AddMech(idx, mechs[idx], true, true, false, (string)null);
      }
      for (int idx = 0; idx < vehicles.Count; ++idx) {
        __instance.AddMech(idx+__instance.VehicleShift(), vehicles[idx], true, true, false, (string)null);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddMechs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<MechDef>) })]
  public static class SimGameState_AddMechs_MechDefs {
    public static bool Prefix(SimGameState __instance, ref List<MechDef> startingMechs) {
      Log.TWL(0, "SimGameState.AddMechs");
      List<MechDef> mechs = new List<MechDef>();
      List<MechDef> vehicles = new List<MechDef>();
      for (int idx = 0; idx < startingMechs.Count; ++idx) {
        MechDef mech = startingMechs[idx];
        if (mech.IsChassisFake() == false) {
          mechs.Add(mech);
        } else {
          vehicles.Add(mech);
        }
      }
      for (int idx = 0; idx < mechs.Count; ++idx) {
        __instance.AddMech(idx, mechs[idx], true, true, false, (string)null);
      }
      for (int idx = 0; idx < vehicles.Count; ++idx) {
        __instance.AddMech(idx + __instance.VehicleShift(), vehicles[idx], true, true, false, (string)null);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(MechDef), typeof(bool), typeof(bool), typeof(bool), typeof(string) })]
  public static class SimGameState_AddMech {
    public static int VehicleShift(this SimGameState sim) {
      return 100;
    }
    public static int GetFirstFreeMechBay(this SimGameState sim, MechDef mech) {
      if (mech == null) { return sim.GetFirstFreeMechBay(); };
      if (BattleTechResourceLocator_RefreshTypedEntries_Patch.IsChassisFake(mech.ChassisID) == false) { return sim.GetFirstFreeMechBay(); }
      int maxActiveMechs = sim.GetMaxActiveMechs() + sim.VehicleShift();
      int minActiveMechs = sim.VehicleShift();
      for (int key = minActiveMechs; key < maxActiveMechs; ++key) {
        if (!sim.ActiveMechs.ContainsKey(key)) { return key; };
      }
      return -1;
    }
    public static bool Prefix(SimGameState __instance, int idx, MechDef mech, bool active, bool forcePlacement, bool displayMechPopup, string mechAddedHeader, StatCollection ___companyStats, SimGameInterruptManager ___interruptQueue) {
      try {
        if (string.IsNullOrEmpty(mech.GUID)) { mech.SetGuid(__instance.GenerateSimGameUID()); }
        if (!__instance.DataManager.ContentPackIndex.IsResourceOwned(mech.Description.Id) || !__instance.DataManager.ContentPackIndex.IsResourceOwned(mech.Chassis.Description.Id) || !__instance.DataManager.ContentPackIndex.IsResourceOwned(mech.Chassis.PrefabIdentifier)) {
          return false;
        }
        ___companyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechsAdded", StatCollection.StatOperation.Int_Add, 1, -1, true);
        if (displayMechPopup) {
          Localize.Text text;
          if (string.IsNullOrEmpty(mechAddedHeader)) {
            text = new Localize.Text("'Mech Chassis Complete", (object[])Array.Empty<object>());
            int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_sim_popup_newChassis, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
          } else
            text = new Localize.Text(mechAddedHeader, (object[])Array.Empty<object>());
          text.Append(": ", (object[])Array.Empty<object>());
          text.Append(mech.Description.UIName, (object[])Array.Empty<object>());
          ___interruptQueue.QueuePauseNotification(text.ToString(true), mech.Chassis.YangsThoughts, __instance.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_mechreadycomplete", (Action)(() => {
            int firstFreeMechBay = __instance.GetFirstFreeMechBay(mech);
            if (firstFreeMechBay >= 0)
              __instance.ActiveMechs[firstFreeMechBay] = mech;
            else
              __instance.CreateMechPlacementPopup(mech);
          }), "Continue", (Action)null, (string)null);
        } else if (!forcePlacement && __instance.GetFirstFreeMechBay(mech) < 0) {
          __instance.CreateMechPlacementPopup(mech);
        } else {
          if (active) {
            if (__instance.ActiveMechs.ContainsKey(idx))
              SimGameState.logger.LogError((object)("SimGame.AddMech is attempting to add a mech " + mech.Description.Id + " to bay " + (object)idx + " but that bay is already occupied! This will overwrite the mech in that slot!"));
            __instance.ActiveMechs[idx] = mech;
          } else {
            System.Type type = typeof(MechDef);
            __instance.AddItemStat(mech.Description.Id, type, false);
          }
          __instance.MessageCenter.PublishMessage((MessageCenterMessage)new SimGameMechAddedMessage(mech, 0, false));
        }
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentationSimGame))]
  [HarmonyPatch("LoadWeapons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentationSimGame_LoadWeapons {
    private static MethodInfo m_GetAttachTransform = typeof(MechRepresentationSimGame).GetMethod("GetAttachTransform", BindingFlags.Instance | BindingFlags.NonPublic);
    public static Transform GetAttachTransform(this MechRepresentationSimGame rep,ChassisLocations location) {
      return (Transform)m_GetAttachTransform.Invoke(rep, new object[] { location });
    }
    public static VTOLBodyAnimation VTOLBodyAnim(this MechRepresentationSimGame simRep) {
      return simRep.gameObject.GetComponentInChildren<VTOLBodyAnimation>();
    }
    private static MethodInfo m_CreateBlankPrefabs = typeof(MechRepresentationSimGame).GetMethod("CreateBlankPrefabs", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void CreateBlankPrefabs(this MechRepresentationSimGame rep, List<string> usedPrefabNames, ChassisLocations location) {
      m_CreateBlankPrefabs.Invoke(rep, new object[] { usedPrefabNames, location });
    }
    public static bool Prefix(MechRepresentationSimGame __instance,DataManager ___dataManager) {
      Log.TWL(0, "MechRepresentationSimGame.LoadWeapons");
      if (__instance.gameObject.GetComponent<SquadRepresentationSimGame>() != null) { return false; }
      if (__instance.gameObject.GetComponent<TrooperRepresentationSimGame>() != null) {
        __instance.gameObject.GetComponent<TrooperRepresentationSimGame>().LoadWeapons();
        return false;
      }
      List<string> usedPrefabNames = new List<string>();
      VTOLBodyAnimation bodyAnimation = __instance.VTOLBodyAnim();
      MechTurretAnimation MechTurret = __instance.gameObject.GetComponentInChildren<MechTurretAnimation>(true);
      QuadBodyAnimation quadBody = __instance.gameObject.GetComponentInChildren<QuadBodyAnimation>(true);
      Log.WL(1, "bodyAnimation:"+(bodyAnimation==null?"null":"not null"));
      for (int index = 0; index < __instance.mechDef.Inventory.Length; ++index) {
        MechComponentRef componentRef = __instance.mechDef.Inventory[index];
        string MountedLocation = componentRef.MountedLocation.ToString();
        bool correctLocation = false;
        if (__instance.mechDef.IsChassisFake()) {
          switch (componentRef.MountedLocation) {
            case ChassisLocations.LeftArm: MountedLocation = VehicleChassisLocations.Front.ToString(); correctLocation = true; break;
            case ChassisLocations.RightArm: MountedLocation = VehicleChassisLocations.Rear.ToString(); correctLocation = true; break;
            case ChassisLocations.LeftLeg: MountedLocation = VehicleChassisLocations.Left.ToString(); correctLocation = true; break;
            case ChassisLocations.RightLeg: MountedLocation = VehicleChassisLocations.Right.ToString(); correctLocation = true; break;
            case ChassisLocations.Head: MountedLocation = VehicleChassisLocations.Turret.ToString(); correctLocation = true; break;
          }
        } else {
          correctLocation = true;
        }
        Log.WL(1, "Component " + componentRef.Def.GetType().ToString() + ":" + componentRef.GetType().ToString() + " id:" + componentRef.Def.Description.Id + " loc:" + MountedLocation);
        if (correctLocation) {
          WeaponDef def = componentRef.Def as WeaponDef;
          Log.WL(2, "GetComponentPrefabName " + __instance.mechDef.Chassis.HardpointDataDef.ID + " base:" + __instance.mechDef.Chassis.PrefabBase + " loc:" + MountedLocation + " currPrefabName:" + componentRef.prefabName + " hasPrefab:" + componentRef.hasPrefabName + " hardpointSlot:" + componentRef.HardpointSlot);
          if (def != null) {
            string desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), componentRef.Def.PrefabIdentifier.ToLower(), def.WeaponCategoryValue.HardpointPrefabText);
            Log.WL(3, "desiredPrefabName:"+ desiredPrefabName);
          } else {
            Log.WL(3, "");
          }
          //if (componentRef.hasPrefabName == false) {
          componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.mechDef.Chassis.HardpointDataDef, (BaseComponentRef)componentRef, __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), ref usedPrefabNames);
          componentRef.hasPrefabName = true;
          Log.WL(3, "effective prefab name:" + componentRef.prefabName);

          //}
        }
        if (!string.IsNullOrEmpty(componentRef.prefabName)) {
          HardpointAttachType attachType = HardpointAttachType.None;
          Log.WL(1, "component:"+componentRef.ComponentDefID+":"+componentRef.MountedLocation);
          Transform attachTransform = __instance.GetAttachTransform(componentRef.MountedLocation);
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(componentRef.prefabName);
          GameObject prefab = null;
          string prefabName = componentRef.prefabName;
          if (customHardpoint != null) {
            attachType = customHardpoint.attachType;
            prefab = ___dataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            if (prefab == null) {
              prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            } else {
              prefabName = customHardpoint.prefab;
            }
          } else {
            Log.WL(1, componentRef.prefabName + " have no custom hardpoint");
            prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          }
          if (prefab != null) {
            ComponentRepresentation component1 = prefab.GetComponent<ComponentRepresentation>();
            if (component1 == null) {
              Log.WL(1, prefabName + " have no ComponentRepresentation");
              if(customHardpoint != null) {
                component1 = prefab.AddComponent<WeaponRepresentation>();
                Log.LogWrite(1, "reiniting vfxTransforms\n");
                List<Transform> transfroms = new List<Transform>();
                for (int i = 0; i < customHardpoint.emitters.Count; ++i) {
                  Transform[] trs = component1.GetComponentsInChildren<Transform>();
                  foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[i]) { transfroms.Add(tr); break; } }
                }
                Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
                for (int i = 0; i < transfroms.Count; ++i) {
                  Log.LogWrite(2, transfroms[i].name + ":" + transfroms[i].localPosition + "\n");
                }
                if (transfroms.Count == 0) { transfroms.Add(prefab.transform); };
                component1.vfxTransforms = transfroms.ToArray();
                if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
                  Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
                  GameObject shaderPrefab = ___dataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
                  if (shaderPrefab != null) {
                    Log.LogWrite(1, "shader prefab found\n");
                    Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
                    if (shaderComponent != null) {
                      Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
                      MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                      foreach (MeshRenderer renderer in renderers) {
                        for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                          if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
                            Log.LogWrite(2, "keep original shader: " + renderer.gameObject.transform.name + "\n");
                            continue;
                          }
                          Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
                          renderer.materials[mindex].shader = shaderComponent.material.shader;
                          renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
                        }
                      }
                    }
                    GameObject.Destroy(shaderPrefab);
                  }
                }
              } else {
                component1 = prefab.AddComponent<ComponentRepresentation>();
              }
            }
            if (bodyAnimation != null) {
              Log.WL(1, "found VTOL body animation and vehicle component ref. Location:" + MountedLocation + " type:" + attachType);
              if (attachType == HardpointAttachType.None) {
                if ((bodyAnimation.bodyAttach != null) && (MountedLocation != VehicleChassisLocations.Turret.ToString())) { attachTransform = bodyAnimation.bodyAttach; }
              } else {
                AttachInfo attachInfo = bodyAnimation.GetAttachInfo(MountedLocation, attachType);
                Log.WL(2, "attachInfo:" + (attachInfo == null ? "null" : "not null"));
                if ((attachInfo != null) && (attachInfo.attach != null) && (attachInfo.main != null)) {
                  Log.WL(2, "attachTransform:" + (attachInfo.attach == null ? "null" : attachInfo.attach.name));
                  Log.WL(2, "mainTransform:" + (attachInfo.main == null ? "null" : attachInfo.main.name));
                  attachTransform = attachInfo.attach;
                  attachInfo.bayComponents.Add(component1);
                }
              }
            } else if(MechTurret != null) { 
              Log.WL(1, "found mech turret:" + MountedLocation + " type:" + attachType);
              if(attachType == HardpointAttachType.Turret) {
                if (MechTurret.attachPoints.TryGetValue(componentRef.MountedLocation, out MechTurretAttachPoint attachPoint)) {
                  attachTransform = attachPoint.attachTransform;
                }
              }
            }
            if (component1 != null) {
              component1.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
              component1.gameObject.SetActive(true);
              component1.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
              component1.gameObject.name = componentRef.prefabName;
              __instance.componentReps.Add(component1);
              Log.WL(3,"Component representation spawned and inited. GameObject name:"+ component1.gameObject.name+" Active:"+component1.gameObject.activeInHierarchy+" parent transform:"+component1.transform.parent.name);
            }
          }
          string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(__instance.mechDef, componentRef);
          if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
            ComponentRepresentation component2 = ___dataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<ComponentRepresentation>();
            component2.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
            component2.gameObject.SetActive(true);
            component2.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
            component2.gameObject.name = mountingPointPrefabName;
            __instance.componentReps.Add(component2);
          }
        }
      }
      if (bodyAnimation != null) { bodyAnimation.ResolveAttachPoints(); };
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
      return false;
    }
  }
#pragma warning disable CS0252
  [HarmonyPatch(typeof(MechBayChassisInfoWidget))]
  [HarmonyPatch("OnReadyClicked")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayChassisInfoWidget_OnReadyClicked {
    public static int GetFirstFreeMechBay(this SimGameState sim, ChassisDef chassis) {
      if (chassis == null) { return sim.GetFirstFreeMechBay(); };
      if (BattleTechResourceLocator_RefreshTypedEntries_Patch.IsChassisFake(chassis.Description.Id) == false) { return sim.GetFirstFreeMechBay(); }
      int maxActiveMechs = sim.GetMaxActiveMechs() + sim.VehicleShift();
      int minActiveMechs = sim.VehicleShift();
      for (int key = minActiveMechs; key < maxActiveMechs; ++key) {
        if (!sim.ActiveMechs.ContainsKey(key)) { return key; };
      }
      return -1;
    }
    public static bool Prefix(MechBayChassisInfoWidget __instance, ChassisDef ___selectedChassis, MechBayPanel ___mechBay, MechBayChassisUnitElement ___chassisElement) {
      try {
        Log.TWL(0, "MechBayChassisInfoWidget.OnReadyClicked");
        if (___selectedChassis == null) { return true; }
        if (___selectedChassis.IsFake(___selectedChassis.Description.Id) == false) { return true; }
        if (___selectedChassis.MechPartCount < ___selectedChassis.MechPartMax) {
          int num = ___selectedChassis.MechPartMax - ___selectedChassis.MechPartCount;
          GenericPopupBuilder.Create("'Vehicle Chassis Incomplete", Strings.T("This chassis requires {0} more part{1} before it can be readied for combat.", (object)num, num == 1 ? (object)"" : (object)"s")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        } else if (___mechBay.Sim.GetFirstFreeMechBay(___selectedChassis) < 0) {
          GenericPopupBuilder.Create("Cannot Ready 'Vehicle", "There are no available slots in the 'Vehicle Bay. You must move an active 'Vehicle into storage before readying this chassis.").AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        } else {
          GenericPopupBuilder.Create("Ready 'Vehicle?", Strings.T("It will take {0} day(s) to ready this BattleMech chassis for combat.", (object)Mathf.CeilToInt((float)___mechBay.Sim.Constants.Story.MechReadyTime / (float)___mechBay.Sim.MechTechSkill))).AddButton("Cancel", (Action)null, true, (PlayerAction)null)
            .AddButton("Ready", (Action)(() => {
              ___mechBay.OnReadyMech(___chassisElement);
              __instance.SetData(___mechBay, (MechBayChassisUnitElement)null);
            }), true, (PlayerAction)null)
            .AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).CancelOnEscape().Render();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return false;
    }
    public static int GetFirstFreeMechBayTranspiler(SimGameState simGameState, MechBayChassisInfoWidget chassisWidget) {
      MechBayChassisUnitElement chassisElement = Traverse.Create(chassisWidget).Field<MechBayChassisUnitElement>("chassisElement").Value;
      Log.TWL(0, "MechBayChassisInfoWidget.OnReadyClicked.GetFirstFreeMechBay "+chassisElement.ChassisDef.Description.Id);
      return simGameState.GetFirstFreeMechBay(chassisElement.ChassisDef);
    }
  }
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("OnReadyMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechBayChassisUnitElement) })]
  public static class MechBayPanel_OnReadyMech {
    public static void Prefix(MechBayPanel __instance, MechBayChassisUnitElement chassisElement, MechBayRowGroupWidget ___bayGroupWidget) {
      Log.TWL(0, "MechBayPanel.OnReadyMech "+ chassisElement.ChassisDef.Description.Id+" "+ ___bayGroupWidget.GetFirstFreeBaySlot());
    }
    public static int GetFirstFreeMechBayTranspiler(SimGameState simGameState, MechBayChassisUnitElement chassisElement) {
      Log.TWL(0, "MechBayPanel.OnReadyMech.GetFirstFreeMechBay " + chassisElement.ChassisDef.Description.Id);
      return simGameState.GetFirstFreeMechBay(chassisElement.ChassisDef);
    }
  }
  [HarmonyPatch()]
  public static class MechDefs_Get {
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(DictionaryStore<MechDef>), "Get");
    }
    public static void Prefix(object __instance,ref string id) {
      if (__instance.GetType() != typeof(DictionaryStore<MechDef>)) { return; }
      if (id.StartsWith("vehiclemechdef_")) {
        id = id.Replace("vehiclemechdef_", "vehicledef_");
        Log.TWL(0, "DataManager.MechDefs.Get " + __instance.GetType() + " " + id);
      }
    }
  }
  [HarmonyPatch()]
  public static class MechDefs_Exists {
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(DictionaryStore<MechDef>), "Exists");
    }
    public static void Prefix(object __instance, ref string id) {
      if (__instance.GetType() != typeof(DictionaryStore<MechDef>)) { return; }
      if (id.StartsWith("vehiclemechdef_")) {
        id = id.Replace("vehiclemechdef_", "vehicledef_");
        Log.TWL(0, "DataManager.MechDefs.Exists " + __instance.GetType() + " " + id);
      }
    }
  }
  [HarmonyPatch()]
  public static class MechDefs_TryGet {
    public static MethodBase TargetMethod() {
      return AccessTools.Method(typeof(DictionaryStore<MechDef>), "TryGet");
    }
    public static void Prefix(object __instance, ref string id) {
      if (__instance.GetType() != typeof(DictionaryStore<MechDef>)) { return; }
      if (id.StartsWith("vehiclemechdef_")) {
        id = id.Replace("vehiclemechdef_", "vehicledef_");
        Log.TWL(0, "DataManager.MechDefs.TryGet " + __instance.GetType() + " " + id);
      }
    }
  }

#pragma warning restore CS0252
}