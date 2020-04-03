using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.UI;
using Harmony;
using MechResizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
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
        string arg = string.IsNullOrEmpty(mechDef.prefabOverride) ? mechDef.Chassis.PrefabBase : mechDef.prefabOverride.Replace("chrPrfMech_", "");
        string mechPrefabName = string.Format("chrPrfComp_{0}_simgame", arg);
        mechbay.prefabsLoaded = false;
        LoadRequest loadRequest = mechbay.simState.DataManager.CreateLoadRequest(delegate (LoadRequest request) {
          mechbay.prefabsLoaded = true;
        }, false);
        if (!string.IsNullOrEmpty(mechDef.prefabOverride)) {
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechbay.simState.DataManager.Unlocks().GetBasePrefabIdForPrefabId(mechDef.Chassis.PrefabIdentifier), new bool?(false));
        }
        if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
          Log.TWL(0, "TransitionMech requesting battle game representation " + mechDef.ChassisID + " " + mechDef.Chassis.PrefabIdentifier);
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.Chassis.PrefabIdentifier, new bool?(false));
          VehicleChassisDef vchassis = mechbay.simState.DataManager.VehicleChassisDefs.Get(mechDef.ChassisID);
          if (vchassis != null) {
            vchassis.AddCustomDeps(loadRequest);
          }
        } else {
          Log.TWL(0, "TransitionMech requesting bay game representation " + mechDef.ChassisID + " " + mechPrefabName);
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechPrefabName, new bool?(false));
          mechDef.Chassis.AddCustomDeps(loadRequest);
          mechDef.Refresh();
          for (int index = 0; index < mechDef.Inventory.Length; ++index) {
            if (mechDef.Inventory[index].Def != null) {
              if (string.IsNullOrEmpty(mechDef.Inventory[index].prefabName) == false) loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.Inventory[index].prefabName, new bool?(false));
            }
          }
          if (mechDef.meleeWeaponRef.Def != null)
            if (string.IsNullOrEmpty(mechDef.meleeWeaponRef.prefabName) == false) loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.meleeWeaponRef.prefabName);
          if (mechDef.dfaWeaponRef.Def != null) {
            if (string.IsNullOrEmpty(mechDef.dfaWeaponRef.prefabName) == false) loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, mechDef.dfaWeaponRef.prefabName);
          }
        }
        loadRequest.ProcessRequests(1000u);
        while (!mechbay.prefabsLoaded) {
          yield return null;
        }
        GameObject gameObject = mechbay.simState.DataManager.PooledInstantiate(mechPrefabName, BattleTechResourceType.Prefab, null, null, null);
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
              //AkGameObj audio = battleRep.audioObject;
              //battleRep.audioObject = null;
              //battleRep.Init(null, mechbay.simState.CameraController.MechAnchor, true);
              //battleRep.audioObject = audio;
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
            mechDef.SpawnCustomParts(bayRepresentation);
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
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("ViewBays")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayPanel_ViewBays {
    private static bool toggleState = false;
    public static void Prefix(MechBayPanel __instance, MechBayRowGroupWidget ___bayGroupWidget) {
      Log.TWL(0, "MechBayPanel.ViewBays " + ___bayGroupWidget.gameObject.activeSelf + "/" + toggleState);
      if (___bayGroupWidget.gameObject.activeSelf == false) { toggleState = false; } else { toggleState = !toggleState; };
      ___bayGroupWidget.ShowMechBays(!toggleState);
      ___bayGroupWidget.ShowVehicleBays(toggleState);
    }
  }
  [HarmonyPatch(typeof(PilotGenerator))]
  [HarmonyPatch("GeneratePilots")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotGenerator_GeneratePilots {
    public static void Callsign(this HumanDescriptionDef descr, string value) {
      typeof(HumanDescriptionDef).GetProperty("Callsign", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(descr, new object[1] { value });
    }
    public static void Postfix(PilotGenerator __instance, ref List<PilotDef> __result) {
      Log.TWL(0, "PilotGenerator.GeneratePilots");
      if (__result == null) { return; }
      if (__result.Count == 0) { return; }
      __result[__result.Count - 1].Description.Callsign("V.CREW");
      __result[__result.Count - 1].PilotTags.Add("pilot_vehicle_crew");
      __result[__result.Count - 1].SetUnspentExperience(0);
    }
  }
  [HarmonyPatch(typeof(MechBayRowGroupWidget))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDropTarget), typeof(SimGameState) })]
  public static class MechBayRowGroupWidget_SetData {
    private static readonly int mechBaysCount = 3;
    private static readonly int vehicleBaysCount = 3;
    private static Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>> mechsBays = new Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>>();
    private static Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>> vehiclesBays = new Dictionary<MechBayRowGroupWidget, List<MechBayRowWidget>>();
    public static void ShowMechBays(this MechBayRowGroupWidget bays, bool show) {
      if (mechsBays.TryGetValue(bays, out List<MechBayRowWidget> mechBays)) {
        foreach (MechBayRowWidget bay in mechBays) { bay.gameObject.SetActive(show); }
      }
    }
    public static void ShowVehicleBays(this MechBayRowGroupWidget bays, bool show) {
      if (vehiclesBays.TryGetValue(bays, out List<MechBayRowWidget> vehcileBays)) {
        foreach (MechBayRowWidget bay in vehcileBays) { bay.gameObject.SetActive(show); }
      }
    }
    public static void Prefix(MechBayRowGroupWidget __instance, IMechLabDropTarget dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      Log.TWL(0, "MechBayRowGroupWidget.SetData prefix " + ___bays.Length);
      __state = new List<MechBayRowWidget>();
      __state.AddRange(___bays);
      int fullBaysCount = mechBaysCount + vehicleBaysCount;
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
      if (mechsBays.TryGetValue(__instance, out List<MechBayRowWidget> mechBays) == false) {
        mechBays = new List<MechBayRowWidget>(); mechsBays.Add(__instance, mechBays);
      }
      if (vehiclesBays.TryGetValue(__instance, out List<MechBayRowWidget> vehicleBays) == false) {
        vehicleBays = new List<MechBayRowWidget>(); vehiclesBays.Add(__instance, vehicleBays);
      }
      for (int index = 0; index < __state.Count; ++index) {
        if (index < mechBaysCount) { mechBays.Add(__state[index]); } else {
          vehicleBays.Add(__state[index]);
        }
      }
      ___bays = __state.ToArray();
      //typeof(MechBayRowGroupWidget).GetField("bays", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __state.ToArray());
      Log.WL(1, "->" + ___bays.Length);
    }
    public static void Postfix(MechBayRowGroupWidget __instance, IMechLabDropTarget dropParent, SimGameState sim, ref MechBayRowWidget[] ___bays, ref List<MechBayRowWidget> __state) {
      Log.TWL(0, "MechBayRowGroupWidget.SetData postfix " + ___bays.Length + "/" + __state.Count);
      if (___bays.Length < __state.Count) { ___bays = __state.ToArray(); };
      int maxMechsPerPod = sim.Constants.Story.MaxMechsPerPod;
      int maxActiveMechs = sim.GetMaxActiveMechs();
      int baySlotStart1 = sim.VehicleShift();
      int baySlotEnd1 = sim.VehicleShift() + maxMechsPerPod;
      ___bays[0 + mechBaysCount].SetData(dropParent, sim, "V.Bay 1", true, baySlotStart1, baySlotEnd1);
      int slot1 = 0;
      for (int key = baySlotStart1; key < baySlotEnd1; ++key) {
        MechDef mechDef = (MechDef)null;
        sim.ActiveMechs.TryGetValue(key, out mechDef);
        bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
        bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
        bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
        ___bays[0 + mechBaysCount].SetMech(slot1, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
        ++slot1;
      }
      int baySlotStart2 = baySlotEnd1;
      int baySlotEnd2 = baySlotEnd1 + maxMechsPerPod;
      ___bays[1 + mechBaysCount].SetData(dropParent, sim, "V.Bay 2", maxActiveMechs > maxMechsPerPod, baySlotStart2, baySlotEnd2);
      int slot2 = 0;
      for (int key = baySlotStart2; key < baySlotEnd2; ++key) {
        MechDef mechDef = (MechDef)null;
        sim.ActiveMechs.TryGetValue(key, out mechDef);
        bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
        bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
        bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
        ___bays[1 + mechBaysCount].SetMech(slot2, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
        ++slot2;
      }
      int baySlotStart3 = baySlotEnd2;
      int baySlotEnd3 = baySlotEnd2 + maxMechsPerPod;
      ___bays[2 + mechBaysCount].SetData(dropParent, sim, "V.Bay 3", maxActiveMechs > maxMechsPerPod * 2, baySlotStart3, baySlotEnd3);
      int slot3 = 0;
      for (int key = baySlotStart3; key < baySlotEnd3; ++key) {
        MechDef mechDef = (MechDef)null;
        sim.ActiveMechs.TryGetValue(key, out mechDef);
        bool inMaintenance = sim.GetWorkOrderEntryForMech(mechDef) != null;
        bool isFieldable = MechValidationRules.ValidateMechCanBeFielded(sim, mechDef);
        bool hasFieldableWarnings = MechValidationRules.GetMechFieldableWarnings(sim.DataManager, mechDef).Count > 0;
        ___bays[2 + mechBaysCount].SetMech(slot3, mechDef, inMaintenance, isFieldable, hasFieldableWarnings);
        ++slot3;
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
        if (mech.IsChassisFake()) {
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
        if (mech.IsChassisFake()) {
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
    private static MethodInfo m_CreateBlankPrefabs = typeof(MechRepresentationSimGame).GetMethod("CreateBlankPrefabs", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void CreateBlankPrefabs(this MechRepresentationSimGame rep, List<string> usedPrefabNames, ChassisLocations location) {
      m_CreateBlankPrefabs.Invoke(rep, new object[] { usedPrefabNames, location });
    }
    public static bool Prefix(MechRepresentationSimGame __instance,DataManager ___dataManager) {
      List<string> usedPrefabNames = new List<string>();
      Log.TWL(0, "MechRepresentationSimGame.LoadWeapons");
      for (int index = 0; index < __instance.mechDef.Inventory.Length; ++index) {
        MechComponentRef componentRef = __instance.mechDef.Inventory[index];
        componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.mechDef.Chassis.HardpointDataDef, (BaseComponentRef)componentRef, __instance.mechDef.Chassis.PrefabBase, componentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
        componentRef.hasPrefabName = true;
        if (!string.IsNullOrEmpty(componentRef.prefabName)) {
          Log.WL(1, "component:"+componentRef.ComponentDefID+":"+componentRef.MountedLocation);
          Transform attachTransform = __instance.GetAttachTransform(componentRef.MountedLocation);
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(componentRef.prefabName);
          GameObject prefab = null;
          string prefabName = componentRef.prefabName;
          if (customHardpoint != null) {
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
                  foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[index]) { transfroms.Add(tr); break; } }
                }
                Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
                for (int i = 0; i < transfroms.Count; ++i) {
                  Log.LogWrite(2, transfroms[index].name + ":" + transfroms[index].localPosition + "\n");
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
            if (component1 != null) {
              component1.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
              component1.gameObject.SetActive(true);
              component1.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
              component1.gameObject.name = componentRef.prefabName;
              __instance.componentReps.Add(component1);
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
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
      return false;
    }
  }
}