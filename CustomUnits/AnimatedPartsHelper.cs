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
using BattleTech.Data;
using BattleTech.Rendering;
using FogOfWar;
using HarmonyLib;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using CustAmmoCategories;
using System.Threading;
using System.Collections;
using Localize;
using System.Reflection;

namespace CustomUnits {
  //[HarmonyPatch(typeof(LoadRequest))]
  //[HarmonyPatch("AddBlindLoadRequestInternal")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(BattleTechResourceType), typeof(string), typeof(bool), typeof(bool?) })]
  //public static class LoadRequest_AddBlindLoadRequestInternal {
  //  public static void Postfix(LoadRequest __instance, BattleTechResourceType resourceType, string resourceId, bool allowDuplicateInstantiation, bool? filterByOwnerShip) {
  //    Log.LogWrite("LoadRequest.AddBlindLoadRequestInternal type: " + resourceType + " id " + resourceId + "\n");
  //  }
  //}
  [HarmonyPatch(typeof(LoadRequest))]
  [HarmonyPatch("CompleteLoadTracker")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LoadRequest_CompleteLoadTracker {
    public static void Postfix(LoadRequest __instance, LoadTracker tracker, bool loadSuccess) {
      try {
        if (loadSuccess == true) { return; }
        Log.Combat?.WL(0,"LoadRequest.CompleteLoadTracker tracker: " + tracker + " loadSuccess " + loadSuccess);
        VersionManifestEntry manifest = tracker.resourceManifestEntry;
        Log.Combat?.WL(1, manifest.Type + " streamingAssetsPath:" + Application.streamingAssetsPath + " path:" + manifest.path);
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("PerformAttack")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirector.AttackSequence) })]
  public static class AttackDirector_PerformAttack {
    public static void Prefix(AttackDirector __instance, AttackDirector.AttackSequence sequence) {
      try {
        Log.Combat?.WL(0, "AttackDirector.PerformAttack from " + sequence.attacker.DisplayName + " to " + sequence.chosenTarget.DisplayName);
        TurretAnimator turret = sequence.attacker.GetCustomTurret();
        if (turret == null) {
          Log.Combat?.WL(1, "no turret at attaker");
          return;
        }
        if (sequence.chosenTarget == null) {
          Log.Combat?.WL(1, "choosen target is null");
          return;
        };
        if (sequence.chosenTarget.GameRep == null) {
          Log.Combat?.WL(1, "choosen target has no game rep");
          return;
        }
        turret.target = sequence.chosenTarget;
        Log.Combat?.WL(1, "turret target set");
      } catch (Exception e) {
        Log.Combat?.WL(0, e.ToString(), true);
        AttackDirector.attackLogger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(VehicleRepresentation))]
  [HarmonyPatch("OnAudioEvent")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class VehicleRepresentation_OnAudioEvent {
    public static void Prefix(VehicleRepresentation __instance, string audioEvent) {
      Log.Combat?.WL(0, "VehicleRepresentation.OnAudioEvent " + __instance.name + " audioEvent:" + audioEvent);
    }
  }
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("SetIdleAnimState")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class MechRepresentation_SetIdleAnimState {
  //  public static bool Prefix(MechRepresentation __instance, ref bool ___allowRandomIdles, ref bool __state) {
  //    __state = ___allowRandomIdles;
  //    if (___allowRandomIdles == true) {
  //      UnitCustomInfo info = __instance.parentMech.GetCustomInfo();
  //      if (info != null) {
  //        if (info.NoIdleAnimations) { ___allowRandomIdles = false; return true; }
  //      };
  //      CustomTwistAnimation custIdleAnim = __instance.gameObject.GetComponent<CustomTwistAnimation>();
  //      if(custIdleAnim != null) {
  //        custIdleAnim.PlayIdleAnimation();
  //        ___allowRandomIdles = false; return true;
  //      }
  //    }
  //    return true;
  //  }
  //  public static void Postfix(MechRepresentation __instance, ref bool ___allowRandomIdles, ref bool __state) {
  //    //___allowRandomIdles = __state;
  //    //Log.TWL(0, "MechRepresentation.SetIdleAnimState "+__instance.name+" "+__instance.thisAnimator.GetFloat(___idleRandomValueHash));
  //  }
  //}
  [HarmonyPatch(typeof(VehicleRepresentation))]
  [HarmonyPatch("PlayEngineStartAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class VehicleRepresentation_PlayEngineStartAudio {
    public static void Prefix(VehicleRepresentation __instance) {
      Log.Combat?.WL(0, "VehicleRepresentation.PlayEngineStartAudio " + __instance.name);
      return;
    }
  }
  [HarmonyPatch(typeof(VehicleRepresentation))]
  [HarmonyPatch("PlayEngineStopAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class VehicleRepresentation_PlayEngineStopAudio {
    public static void Prefix(VehicleRepresentation __instance) {
      Log.Combat?.WL(0, "VehicleRepresentation.PlayEngineStopAudio " + __instance.name);
    }
  }
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("OnPlayerVisibilityChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
  public static class PilotableActorRepresentation_OnPlayerVisibilityChanged {
    public static void Postfix(PilotableActorRepresentation __instance, VisibilityLevel newLevel) {
      Log.Combat?.WL(0, "PilotableActorRepresentation.OnPlayerVisibilityChanged " + __instance.parentCombatant.DisplayName);
      if (UnitsAnimatedPartsHelper.animatedParts.ContainsKey(__instance.parentCombatant) == false) {
        Log.Combat?.WL(1, "no animated parts");
        return;
      };
      try {
        List<GameObject> objectsList = UnitsAnimatedPartsHelper.animatedParts[__instance.parentCombatant];
        foreach (GameObject apGameObject in objectsList) {
          if (apGameObject == null) { continue; };
          try {
            GenericAnimatedComponent[] aps = apGameObject.GetComponents<GenericAnimatedComponent>();
            foreach (GenericAnimatedComponent ap in aps) {
              try {
                Log.Combat?.WL(1, "AnimatedPart:" + ap.GetType() + ":" + newLevel);
                ap.OnPlayerVisibilityChanged(__instance.parentCombatant, newLevel);
              }catch(Exception e) {
                Log.Combat?.TWL(0, e.ToString(), true);
                AbstractActor.logger.LogException(e);
              }
            }
          }catch(Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
            AbstractActor.logger.LogException(e);
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("GetAttachTransform")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VehicleChassisLocations) })]
  public static class Vehicle_GetAttachTransform {
    public static void Prefix(ref bool __runOriginal, Vehicle __instance, VehicleChassisLocations location, ref Transform __result) {
      if (!__runOriginal) { return; }
      Log.Combat?.WL(0, "Vehicle.GetAttachTransform " + __instance.DisplayName);
      if (UnitsAnimatedPartsHelper.animatedParts.ContainsKey(__instance) == false) {
        Log.Combat?.WL(1, "no animated parts");
        return;
      };
      TurretAnimator turret = __instance.GetCustomTurret(location);
      if (turret != null) {
        if (turret.barrels.Count > 0) {
          __result = turret.barrels[0].transform;
          Log.Combat?.WL(1, "found barrel:" + __result.name + " parent:" + __result.parent.name + " offset:" + __result.localPosition + " rotation:" + __result.localRotation.eulerAngles + " scale:" + __result.localScale);
          __runOriginal = false;
          return;
        } else {
          Log.Combat?.WL(1, "no barrels");
        }
      } else {
        Log.Combat?.WL(1, "no turrets in location:" + location);
      }
      WeaponMountPoint mp = __instance.GetCustomMountPoint(location);
      if (mp != null) {
        if (mp.barrels.Count > 0) {
          __result = mp.barrels[0].transform;
          Log.Combat?.WL(1, "found mount point:" + __result.name + " parent:" + __result.parent.name + " offset:" + __result.localPosition + " rotation:" + __result.localRotation.eulerAngles + " scale:" + __result.localScale);
          __runOriginal = false;
          return;
        } else {
          Log.Combat?.WL(1, "no barrels");
        }
      } else {
        Log.Combat?.WL(1, "no mount points in location:" + location);
      }
      if (location != VehicleChassisLocations.Turret) {
        VTOLBodyAnimation bodyAnimation = __instance.VTOLAnimation();
        if (bodyAnimation != null) {
          Log.Combat?.WL(1, "found VTOL body animation");
        }
      }
      return;
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("IsTargetPositionInFiringArc")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(Vector3) })]
  public static class Vehicle_IsTargetPositionInFiringArc {
    public static void Prefix(ref bool __runOriginal, Vehicle __instance, ICombatant targetUnit, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition, ref bool __result) {
      if (!__runOriginal) { return; }
      //Log.LogWrite("Vehicle.IsTargetPositionInFiringArc " + __instance.DisplayName+" -> "+targetUnit.DisplayName+"\n");
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        //Log.LogWrite(" no custom info\n");
        return;
      }
      if (__instance.FiringArc() <= 10f) { return; }
      Vector3 forward = targetPosition - attackPosition;
      forward.y = 0.0f;
      Quaternion b = Quaternion.LookRotation(forward);
      __result = (double)Quaternion.Angle(attackRotation, b) < (double)__instance.FiringArc();
      //Log.LogWrite(" rotation to target: " + Quaternion.Angle(attackRotation, b) + " firingArc:" + info.FiringArc + " result: " + __result + "\n");
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsTargetPositionInFiringArc")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(Vector3) })]
  public static class Mech_IsTargetPositionInFiringArc {
    public static void Prefix(ref bool __runOriginal, Mech __instance, ICombatant targetUnit, Vector3 attackPosition, Quaternion attackRotation, Vector3 targetPosition, ref bool __result) {
      if (!__runOriginal) { return; }
      UnitCustomInfo info = __instance.GetCustomInfo();
      //Log.TWL(0, "Mech.IsTargetPositionInFiringArc " + __instance.MechDef.Description.Id);
      if (info == null) {
        //Log.WL(1, "no custom info");
        return;
      }
      if (__instance.FiringArc() <= 10f) {
        //Log.WL(1, "too low arc value");
        return;
      }
      Vector3 forward = targetPosition - attackPosition;
      forward.y = 0.0f;
      if (targetUnit.Type == TaggedObjectType.Building) {
        BattleTech.Building building = targetUnit as BattleTech.Building;
        if (building != null && !string.IsNullOrEmpty(__instance.standingOnBuildingGuid)) {
          string str = __instance.standingOnBuildingGuid + ".Building";
          if (__instance.standingOnBuildingGuid == building.GUID || str == building.GUID) { __result = true; __runOriginal = false;  return; }
        }
      }
      if (forward.sqrMagnitude > Core.Epsilon) {
        Quaternion b = Quaternion.LookRotation(forward);
        __result = (double)Quaternion.Angle(attackRotation, b) < (double)__instance.FiringArc();
      } else {
        __result = true;
      }
      __runOriginal = false;
      //Log.WL(1, Quaternion.Angle(attackRotation, b) + " and " + __instance.FiringArc() + " : " + __result);
      return;
    }
  }
  /*[HarmonyPatch(typeof(FiringPreviewManager))]
  [HarmonyPatch("CanRotateToFace")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class FiringPreviewManager_CanRotateToFace {
    public static bool Prefix(AbstractActor attacker, ICombatant target, Vector3 position, Quaternion rotation, bool isJump, ref bool __result) {
      if (isJump) { __result = true; return false; };
      Log.TWL(0, "FiringPreviewManager.CanRotateToFace " + new Text(attacker.DisplayName).ToString());
      VehicleCustomInfo info = attacker.GetCustomInfo();
      if (info == null) {
        Log.WL(1, "no custom info");
        return true;
      };
      if (info.FiringArc <= 10f) {
        Log.WL(1, "too low arc value");
        return true;
      }
      __result = (double)Mathf.Abs(Mathf.DeltaAngle(attacker.Pathing.lockedAngle, PathingUtil.GetAngle(target.CurrentPosition - position))) < (double)info.FiringArc + (double)attacker.Pathing.AngleAvailable;
      Log.WL(1, "result:"+__result);
      return false;
    }
  }*/
  /*[HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("TeleportUnitsToSpwanPoints")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_TeleportUnitsToSpwanPoints {
    public static bool Prefix(LanceSpawnerGameLogic __instance) {
      Log.LogWrite("LanceSpawnerGameLogic.LanceSpawnerGameLogic " + __instance.DisplayNameWithGuid + "\n");
      try {
        foreach (UnitSpawnPointGameLogic spawnPointGameLogic in __instance.unitSpawnPointGameLogicList) {
          Log.LogWrite(" spawnPoint:" + spawnPointGameLogic.DisplayNameWithGuid + "\n");
          spawnPointGameLogic.TeleportUnitToSpawnPoint();
        }
        if (FogOfWarSystem.Instance == null) {
          FogOfWarSystem.Instance.UpdateAllViewers();
        } else {
          Log.LogWrite(" fog of war not inited?!\n");
        }
      }catch(Exception e) {
        Log.LogWrite(e.ToString()+"\n");
      }
      return false;
    }
  }*/
  public class GenericAnimatedData {
    public string sound_start_event { get; set; }
    public string sound_stop_event { get; set; }
    public GenericAnimatedData() {
      sound_start_event = string.Empty;
      sound_stop_event = string.Empty;
    }
  }
  public class SimpleRotatorData {
    public float speed { get; set; }
    public string axis { get; set; }
    public string rotateBone { get; set; }
    public SimpleRotatorData() {
      speed = 0f;
      axis = "y";
      rotateBone = string.Empty;
    }
  }
  public class TurretData {
    public float speed { get; set; }
    public VehicleChassisLocations VehicleLocation;
    public ChassisLocations MechLocation;
    public List<CustomTransform> barrels { get; set; }
    public TurretData() {
      speed = 0f;
      barrels = new List<CustomTransform>();
    }
  }
  public class GenericAnimatedComponent : MonoBehaviour {
    public ICombatant parent { get; set; }
    protected List<Renderer> renderers;
    public int Location { get; set; }
    protected string AudioEventNameStart;
    protected string AudioEventNameStop;
    public string PrefabName { get; private set; }
    public virtual bool StayOnDeath() { return false; }
    public virtual bool StayOnLocationDestruction() { return false; }
    public virtual void OnDeath() {
      if (string.IsNullOrEmpty(AudioEventNameStop) == false) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(AudioEventNameStop, this.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.Combat?.TWL(0, "Stop playing sound by id (" + AudioEventNameStop + "):" + soundid);
        } else {
          Log.Combat?.TWL(0, "Can't play");
        }
      }
    }
    public virtual bool KeepPosOnDeath() { return false; }
    public GenericAnimatedComponent() {
      parent = null;
      Location = 0;
      renderers = new List<Renderer>();
    }
    public virtual void Init(ICombatant a, int loc, string data, string prefabName) {
      this.PrefabName = prefabName;
      GenericAnimatedData jdata = JsonConvert.DeserializeObject<GenericAnimatedData>(data);
      AudioEventNameStart = jdata.sound_start_event;
      AudioEventNameStop = jdata.sound_stop_event;
      Log.Combat?.WL(0, "GenericAnimatedComponent.Init parent:" + (a == null?"null":new Text(a.DisplayName).ToString()) +" " + this.gameObject.name + " AudioEventNameStart: " + this.AudioEventNameStart + " AudioEventNameStop:"+ AudioEventNameStop);
      parent = a;
      Location = loc;
      if (this.renderers != null) {
        Renderer[] renderers = this.gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers) {
          this.renderers.Add(renderer);
        }
      }
    }
    public virtual void OnPlayerVisibilityChanged(ICombatant combatant, VisibilityLevel newLevel) {
      if (this.parent == null) { this.parent = combatant; }
      if (this.renderers == null)
        return;
      try { 
        if (newLevel == VisibilityLevel.LOSFull) {
          for (int index = 0; index < this.renderers.Count; ++index) {
            this.renderers[index].enabled = true;
          }
          if (string.IsNullOrEmpty(AudioEventNameStart) == false) {
            if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
              uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(AudioEventNameStart, this.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
              Log.Combat?.TWL(0, "Playing sound by id (" + AudioEventNameStart + "):" + soundid);
            } else {
              Log.Combat?.TWL(0, "Can't play");
            }
          }
        } else {
          for (int index = 0; index < this.renderers.Count; ++index) {
            this.renderers[index].enabled = false;
          }
          if (string.IsNullOrEmpty(AudioEventNameStop) == false) {
            if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
              uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(AudioEventNameStop, this.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
              Log.Combat?.TWL(0, "Stop playing sound by id (" + AudioEventNameStop + "):" + soundid);
            } else {
              Log.Combat?.TWL(0, "Can't play");
            }
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.logger.LogException(e);
      }
    }
    public virtual void Clear() {
      Log.Combat?.TWL(0, "GenericAnimatedComponent.Clear: stop:"+ AudioEventNameStop+" parent:"+(this.parent==null?"null":"not null"));
      if (this.parent == null) { return; }
      if (string.IsNullOrEmpty(AudioEventNameStop) == false) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint soundid = SceneSingletonBehavior<WwiseManager>.Instance.PostEventByName(AudioEventNameStop, this.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.Combat?.TWL(0, "Stop playing sound by id (" + AudioEventNameStop + "):" + soundid);
        } else {
          Log.Combat?.TWL(0, "Can't play (" + AudioEventNameStop + ")");
        }
      }
    }
  }
  public class SimpleRotator : GenericAnimatedComponent {
    //private float rotateAngle;
    private float speed;
    private int axis;
    private Transform rotateBone;
    //private AudioSource audioSrc;
    //private AudioClip audioClip;
    public SimpleRotator() {
      rotateBone = null;
    }
    public void Awake() {
      Log.Combat?.WL(0,"Awake " + this.gameObject.name);
      //this.rotateAngle = 0.0f;
    }
    protected virtual void Update() {
      if (rotateBone != null) {
        switch (axis) {
          case 1: this.rotateBone.Rotate(0f, speed, 0f, Space.Self); break;
          case 2: this.rotateBone.Rotate(0f, 0f, speed, Space.Self); break;
          default: this.rotateBone.Rotate(speed, 0f, 0f, Space.Self); break;
        }
      }
    }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      SimpleRotatorData srdata = JsonConvert.DeserializeObject<SimpleRotatorData>(data);
      this.speed = srdata.speed;
      this.axis = 0;
      if (srdata.axis == "y") {
        this.axis = 1;
      } else if (srdata.axis == "z") {
        this.axis = 2;
      }
      Log.Combat?.WL(0, "SimpleRotator.Init " + this.gameObject.name + " axis: " + this.axis);
      if (string.IsNullOrEmpty(srdata.rotateBone)) { this.rotateBone = this.gameObject.transform; } else {
        Transform[] bones = this.gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform bone in bones) {
          Log.Combat?.WL(1, "Bone:" + bone.name + "/" + srdata.rotateBone, true);
          if (bone.name == srdata.rotateBone) {
            Log.Combat?.WL(2, "found!", true);
            this.rotateBone = bone; break;
          }
        }
      }
      if (this.rotateBone == null) { this.rotateBone = this.gameObject.transform; }
      Log.Combat?.WL(1, "found rotation bone:" + this.rotateBone.name, true);
    }
  }
  public class CustomMoveAnimator : GenericAnimatedComponent {
    public Animator animator = null;
    public CustomMoveAnimator() {
    }
    public void Awake() {
      Log.Combat?.WL(0, "Awake " + this.gameObject.name);
    }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      Component[] components = this.gameObject.GetComponentsInChildren<Component>();
      this.animator = this.gameObject.GetComponentInChildren<Animator>();
      //if (this.animator != null) { this.animator.speed = 0; }
      Log.Combat?.WL(0, "CustomMoveAnimator.Init");
      foreach (Component component in components) {
        if (component == null) { continue; }
        Log.Combat?.WL(1, component.name + ":" + component.GetType().ToString());
      }
    }
  }
  public enum LegAnimationState {
    lasNone,
    lasMove,
    lasForward,
    lasBackward0,
    lasBackward1,
    lasBackward2,
    lasBackward3
  }
  public class CustomQuadLegController : GenericAnimatedComponent {
    public Animator LFAnimator = null;
    public Animator LBAnimator = null;
    public Animator RFAnimator = null;
    public Animator RBAnimator = null;
    public float t = 0f;
    public bool prevFState = false;
    public bool prevBState = false;
    public bool prevTState = false;
    public LegAnimationState legAnimState;
    void Awake() {
    }
    void Start() {
    }
    public float Velocity() {
      if (legAnimState == LegAnimationState.lasForward) { return 1f; }
      return 0f;
    }
    public void StopAnimation() {
      t = 0f;
      legAnimState = LegAnimationState.lasNone;
      //if (this.LFAnimator != null) { this.LFAnimator.enabled = true; this.LFAnimator.SetBool("move", false); };
      //if (this.LBAnimator != null) { this.LFAnimator.enabled = true; this.LBAnimator.SetBool("move", false); };
      //if (this.RFAnimator != null) { this.LFAnimator.enabled = true; this.RFAnimator.SetBool("move", false); };
      //if (this.RBAnimator != null) { this.LFAnimator.enabled = true; this.RBAnimator.SetBool("move", false); };
    }
    public void StartAnimation() {
      t = 0f;
      Log.Combat?.WL(0, "CustomQuadLegController.StartAnimation");
      this.ForwardAnimation();
      /*if (this.LFAnimator != null) {
        this.LBAnimator.speed = 1f;
        this.LFAnimator.enabled = true; this.LFAnimator.SetBool("move", true);
        Log.LogWrite(" left front move\n");
      };*/
    }
    public void ForwardAnimation() {
      //t = 0f;
      legAnimState = LegAnimationState.lasForward;
      if (this.LFAnimator != null) { this.LFAnimator.enabled = true; this.LFAnimator.SetBool("forward", true); };
      if (this.LBAnimator != null) { this.LFAnimator.enabled = true; this.LBAnimator.SetBool("forward", true); };
      if (this.RFAnimator != null) { this.LFAnimator.enabled = true; this.RFAnimator.SetBool("forward", true); };
      if (this.RBAnimator != null) { this.LFAnimator.enabled = true; this.RBAnimator.SetBool("forward", true); };
    }

    public void BackwardAnimation(int index) {
      Animator animator = null;
      switch (index) {
        case 0: animator = this.LFAnimator; legAnimState = LegAnimationState.lasBackward0; break;
        case 1: animator = this.LBAnimator; legAnimState = LegAnimationState.lasBackward1; break;
        case 2: animator = this.RFAnimator; legAnimState = LegAnimationState.lasBackward2; break;
        case 3: animator = this.RBAnimator; legAnimState = LegAnimationState.lasBackward3; break;
      }
      if (animator != null) { animator.enabled = true; animator.SetBool("forward", false); }
    }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      Animator[] animators = this.parent.GameRep.gameObject.GetComponentsInChildren<Animator>();
      Log.Combat?.WL(0, "CustomQuadLegController.Init");
      foreach (Animator animator in animators) {
        if (animator.gameObject.name.StartsWith("lf_limb")) {
          Log.Combat?.WL(1, "left front animatior found");
          LFAnimator = animator;
        } else if (animator.gameObject.name.StartsWith("lr_limb")) {
          Log.Combat?.WL(1, "left rear animatior found");
          LBAnimator = animator;
        } else if (animator.gameObject.name.StartsWith("rf_limb")) {
          Log.Combat?.WL(1, "right front animatior found");
          RFAnimator = animator;
        } else if (animator.gameObject.name.StartsWith("rr_limb")) {
          Log.Combat?.WL(1, "right rear animatior found");
          RBAnimator = animator;
        }
      }
      legAnimState = LegAnimationState.lasNone;
      t = 0f;
      this.RegisterMoveController();
    }
    void Update() {
      if (legAnimState == LegAnimationState.lasNone) { return; }
      this.t += Time.deltaTime;
      switch (legAnimState) {
        case LegAnimationState.lasForward: if (t >= 0.75f) { this.BackwardAnimation(0); t -= 0.75f; }; break;
        case LegAnimationState.lasBackward0: if (t >= 0.25f) { this.BackwardAnimation(1); t -= 0.25f; }; break;
        case LegAnimationState.lasBackward1: if (t >= 0.25f) { this.BackwardAnimation(2); t -= 0.25f; }; break;
        case LegAnimationState.lasBackward2: if (t >= 0.25f) { this.BackwardAnimation(3); t -= 0.25f; }; break;
        case LegAnimationState.lasBackward3: if (t >= 0.25f) { this.ForwardAnimation(); t = 0f; }; break;
      }
      /*if (legAnimState == LegAnimationState.lasMove) {
        if (this.t > 0f) {
          this.LBAnimator.speed = 1f;
          this.LBAnimator.enabled = true; this.LBAnimator.SetBool("move", true);
          Log.LogWrite(" left rear move\n");
        }
        if (this.t > 0.5f) {
          this.RFAnimator.speed = 1f;
          this.RFAnimator.enabled = true; this.RFAnimator.SetBool("move", true);
          Log.LogWrite(" left rear move\n");
        }
        if (this.t > 0.25f) {
          this.RBAnimator.speed = 1f;
          this.RBAnimator.enabled = true; this.RBAnimator.SetBool("move", true);
          Log.LogWrite(" left rear move\n");
          this.legAnimState = LegAnimationState.lasNone;
        }
      }*/
    }
  }
  public class TurretAnimator : GenericAnimatedComponent {
    //private float rotateAngle;
    public ICombatant target;
    public float idleSpeed;
    public float rotationDir;
    public float curAngle;
    public VehicleChassisLocations VehicleChassisLocation;
    public ChassisLocations MechChassisLocation;
    public List<GameObject> barrels;
    public TurretAnimator() {
      target = null;
      idleSpeed = 1f;
      rotationDir = 0f;
      curAngle = 0f;
      VehicleChassisLocation = VehicleChassisLocations.Turret;
      MechChassisLocation = ChassisLocations.CenterTorso;
      barrels = new List<GameObject>();
    }
    protected virtual void Update() {
      if (this.target != null) {
        //Log.LogWrite("Turret.Update traget " + target.DisplayName + "\n");
        if (target.GameRep != null) {
          var lookPos = target.GameRep.transform.position - transform.position;
          lookPos.y = 0;
          var rotation = lookPos.sqrMagnitude > Core.Epsilon ? Quaternion.LookRotation(lookPos) : Quaternion.identity;
          transform.rotation = rotation;
        }
      } else {
        //Log.LogWrite("Turret.Update "+curAngle+" dir:"+rotationDir+"\n");
        if (Math.Abs(curAngle) > 60f) { rotationDir *= -1f; };
        this.transform.Rotate(0f, idleSpeed * rotationDir, 0f, Space.Self);
        curAngle += (idleSpeed * rotationDir);
      }
    }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      TurretData tdata = JsonConvert.DeserializeObject<TurretData>(data);
      this.idleSpeed = Math.Abs(tdata.speed);
      this.VehicleChassisLocation = tdata.VehicleLocation;
      this.MechChassisLocation = tdata.MechLocation;
      for (int t = 0; t < tdata.barrels.Count; ++t) {
        GameObject goBarrel = new GameObject("barrel_" + t);
        goBarrel.transform.SetParent(this.transform);
        goBarrel.transform.localPosition = tdata.barrels[t].offset.vector;
        goBarrel.transform.localEulerAngles = tdata.barrels[t].rotate.vector;
        goBarrel.transform.localScale = tdata.barrels[t].scale.vector;
        barrels.Add(goBarrel);
      }
      rotationDir = 1f;
    }
    public override void Clear() {
      base.Clear();
      foreach (GameObject barrel in barrels) {
        GameObject.Destroy(barrel);
      }
      barrels.Clear();
    }
  }
  public class WeaponMountPoint : GenericAnimatedComponent {
    //private float rotateAngle;
    public VehicleChassisLocations VehicleChassisLocation;
    public ChassisLocations MechChassisLocation;
    public List<GameObject> barrels;
    public WeaponMountPoint() {
      VehicleChassisLocation = VehicleChassisLocations.Turret;
      MechChassisLocation = ChassisLocations.CenterTorso;
      barrels = new List<GameObject>();
    }
    protected virtual void Update() {
    }
    public override void Init(ICombatant a, int loc, string data, string prefabName) {
      base.Init(a, loc, data, prefabName);
      TurretData tdata = JsonConvert.DeserializeObject<TurretData>(data);
      this.VehicleChassisLocation = tdata.VehicleLocation;
      this.MechChassisLocation = tdata.MechLocation;
      for (int t = 0; t < tdata.barrels.Count; ++t) {
        GameObject goBarrel = new GameObject("barrel_" + t);
        goBarrel.transform.SetParent(this.transform);
        goBarrel.transform.localPosition = tdata.barrels[t].offset.vector;
        goBarrel.transform.localEulerAngles = tdata.barrels[t].rotate.vector;
        goBarrel.transform.localScale = tdata.barrels[t].scale.vector;
        barrels.Add(goBarrel);
      }
    }
    public override void Clear() {
      base.Clear();
      foreach (GameObject barrel in barrels) {
        GameObject.Destroy(barrel);
      }
      barrels.Clear();
    }
  }
  public static class UnitsAnimatedPartsHelper {
    public static Dictionary<ICombatant, List<GameObject>> animatedParts = new Dictionary<ICombatant, List<GameObject>>();
    public static void OnDeathAnimation(this ICombatant unit) {
      if (animatedParts.ContainsKey(unit) == false) { return; };
      for (int t = 0; t < animatedParts[unit].Count; ++t) {
        GenericAnimatedComponent[] components = animatedParts[unit][t].GetComponents<GenericAnimatedComponent>();
        if (components == null) { continue; }
        foreach (GenericAnimatedComponent component in components) {
          if (component == null) { continue; }
          component.OnDeath();
        }
      }
    }
    public static Dictionary<GenericAnimatedComponent,Vector3> storePositions(ICombatant unit) {
      Dictionary<GenericAnimatedComponent, Vector3> result = new Dictionary<GenericAnimatedComponent, Vector3>();
      if (animatedParts.ContainsKey(unit) == false) { return result; };
      for (int t = 0; t < animatedParts[unit].Count; ++t) {
        if (animatedParts[unit][t] != null) {
          GenericAnimatedComponent[] components = animatedParts[unit][t].GetComponents<GenericAnimatedComponent>();
          if (components == null) { continue; }
          foreach (GenericAnimatedComponent component in components) {
            if (component == null) { continue; }
            if (component.KeepPosOnDeath() == false) { continue; }
            result.Add(component, component.transform.position);
          }
        }
      }
      return result;
    }
    public static void RestorePositions(Dictionary<GenericAnimatedComponent, Vector3> result) {
      foreach(var cmp in result) { cmp.Key.transform.position = cmp.Value; }
    }
    public static TurretAnimator GetCustomTurret(this ICombatant unit) {
      if (animatedParts.ContainsKey(unit) == false) { return null; }
      foreach (GameObject obj in animatedParts[unit]) {
        TurretAnimator turret = obj.GetComponent<TurretAnimator>();
        if (turret != null) { return turret; };
      }
      return null;
    }
    public static TurretAnimator GetCustomTurret(this ICombatant unit, VehicleChassisLocations location) {
      if (animatedParts.ContainsKey(unit) == false) { return null; }
      foreach (GameObject obj in animatedParts[unit]) {
        TurretAnimator turret = obj.GetComponent<TurretAnimator>();
        if (turret == null) { continue; };
        if (turret.barrels.Count == 0) { continue; }
        if (turret.VehicleChassisLocation != location) { continue; }
        return turret;
      }
      return null;
    }
    public static WeaponMountPoint GetCustomMountPoint(this ICombatant unit, VehicleChassisLocations location) {
      if (animatedParts.ContainsKey(unit) == false) { return null; }
      foreach (GameObject obj in animatedParts[unit]) {
        WeaponMountPoint mp = obj.GetComponent<WeaponMountPoint>();
        if (mp == null) { continue; };
        if (mp.barrels.Count == 0) { continue; }
        if (mp.VehicleChassisLocation != location) { continue; }
        return mp;
      }
      return null;
    }
    public static void Clear(ICombatant unit, bool combatEnd) {
      if (animatedParts.ContainsKey(unit) == false) { return; };
      bool wasDespawned = false;
      AbstractActor actor = unit as AbstractActor;
      if (actor != null) { wasDespawned = actor.WasDespawned; }
      Log.Combat?.TWL(0, "UnitsAnimatedPartsHelper.Clear "+unit.DisplayName+ " combatEnd:"+ combatEnd+ " animatedParts:" + animatedParts[unit].Count);
      for (int t = 0; t < animatedParts[unit].Count; ++t) {
        if (animatedParts[unit][t] != null) {
          GenericAnimatedComponent[] components = animatedParts[unit][t].GetComponents<GenericAnimatedComponent>();
          if (components != null) {
            bool keepObject = false;
            foreach (GenericAnimatedComponent component in components) {
              if (component == null) { continue; }
              if (wasDespawned == false) {
                if (component.StayOnDeath() && (combatEnd == false)) { keepObject = true; continue; }
              }
              component.Clear();
              GameObject.Destroy(component);
            }
            Log.Combat?.WL(0, animatedParts[unit][t].name + " keep:"+ keepObject);
            if (keepObject == false) {
              GameObject.Destroy(animatedParts[unit][t]);
              animatedParts[unit][t] = null;
            }
          } else {
            GameObject.Destroy(animatedParts[unit][t]);
            animatedParts[unit][t] = null;
          }
        };
      }
    }
    public static void DestroyAnimation(this ICombatant unit, int location) {
      if (animatedParts.ContainsKey(unit) == false) { return; };
      HashSet<GameObject> toDestroy = new HashSet<GameObject>();
      for (int t = 0; t < animatedParts[unit].Count; ++t) {
        if (animatedParts[unit][t] != null) {
          GenericAnimatedComponent[] components = animatedParts[unit][t].GetComponents<GenericAnimatedComponent>();
          if (components != null) {
            foreach (GenericAnimatedComponent component in components) {
              if (component == null) { continue; };
              if (component.Location != location) { continue; };
              if (component.StayOnLocationDestruction()) { continue; }
              toDestroy.Add(animatedParts[unit][t]);
            }
          }
        };
      }
      Log.Combat?.WL(0, "DestroyAnimation count:" + toDestroy.Count, true);
      foreach (GameObject go in toDestroy) {
        GenericAnimatedComponent[] components = go.GetComponents<GenericAnimatedComponent>();
        if (components != null) {
          foreach (GenericAnimatedComponent component in components) {
            if (component != null) { component.Clear(); }
            GameObject.Destroy(component);
          }
        }
        animatedParts[unit].Remove(go);
        GameObject.Destroy(go);
      }
    }
    public static void Clear() {
      foreach (var ap in animatedParts) {
        UnitsAnimatedPartsHelper.Clear(ap.Key,true);
      }
      animatedParts.Clear();
    }
    public static Transform getAttachMech(this MechRepresentationSimGame rep, int location) {
      ChassisLocations loc = (ChassisLocations)location;
      switch (loc) {
        case ChassisLocations.Head: return rep.vfxHeadTransform;
        case ChassisLocations.CenterTorso: return rep.vfxCenterTorsoTransform;
        case ChassisLocations.RightTorso: return rep.vfxRightTorsoTransform;
        case ChassisLocations.LeftTorso: return rep.vfxLeftTorsoTransform;
        case ChassisLocations.RightArm: return rep.RightArmAttach;
        case ChassisLocations.LeftArm: return rep.LeftArmAttach;
        case ChassisLocations.RightLeg: return rep.RightLegAttach;
        case ChassisLocations.LeftLeg: return rep.LeftLegAttach;
        default: return rep.vfxCenterTorsoTransform;
      }
    }
    public static Transform getAttachVehicle(this MechRepresentationSimGame rep, int location) {
      VehicleChassisLocations loc = (VehicleChassisLocations)location;
      switch (loc) {
        case VehicleChassisLocations.Turret: return rep.vfxHeadTransform;
        case VehicleChassisLocations.Front: return rep.LeftArmAttach;
        case VehicleChassisLocations.Rear: return rep.RightArmAttach;
        case VehicleChassisLocations.Left: return rep.LeftLegAttach;
        case VehicleChassisLocations.Right: return rep.RightLegAttach;
        default: return rep.vfxHeadTransform;
      }
    }
    public static void SpawnAnimatedPart(MechDef def,MechRepresentationSimGame rep, CustomPart spawnPart, int Location) {
      if (def == null) { return; }
      if (rep == null) { return; }
      Log.Combat?.WL(0, "SpawnAnimatedPart " + def.Description.Id + " " + spawnPart.prefab + " " + spawnPart.AnimationType + " " + spawnPart.Data + " " + Location + " " + spawnPart.prefabTransform.offset + " " + spawnPart.prefabTransform.scale + " " + spawnPart.prefabTransform.rotate);
      Renderer actorRenderer = rep.gameObject.GetComponentInChildren<Renderer>();
      GameObject gameObject = null;
      if (string.IsNullOrEmpty(spawnPart.prefab) == false) {
        gameObject = def.DataManager.PooledInstantiate(spawnPart.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          Log.Combat?.WL(0, "Can't find " + spawnPart.prefab + " in in-game prefabs");
          if (CACMain.Core.AdditinalFXObjects.ContainsKey(spawnPart.prefab)) {
            Log.Combat?.WL(0, "Found in additional prefabs");
            gameObject = GameObject.Instantiate(CACMain.Core.AdditinalFXObjects[spawnPart.prefab]);
          } else {
            Log.Combat?.WL(1, "can't spawn prefab " + spawnPart.prefab + " it is absent in pool,in-game assets and external assets", true);
            return;
          }
        }
      } else {
        Log.Combat?.WL(1, "creating empty object");
        gameObject = new GameObject();
      }
      Transform parentTransform = null;
      if (string.IsNullOrEmpty(spawnPart.boneName) == false) {
        Transform[] transforms = rep.gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform bone in transforms) {
          if (bone.name == spawnPart.boneName) {
            Log.Combat?.WL(1, "parent bone found");
            parentTransform = bone;
            break;
          }
        }
      }
      if (parentTransform == null) { parentTransform = def.IsVehicle()?rep.getAttachVehicle(Location) :rep.getAttachMech(Location); };
      gameObject.transform.parent = parentTransform;
      gameObject.transform.localPosition = spawnPart.prefabTransform.offset.vector;
      Quaternion rotation = Quaternion.Euler(spawnPart.prefabTransform.rotate.vector);
      gameObject.transform.localRotation = rotation;
      gameObject.transform.localScale = spawnPart.prefabTransform.scale.vector;
      /*Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
      foreach (Transform transform in transforms) {
        Log.LogWrite("Transform:"+transform.name+" scale:"+ transform.gameObject.transform.localScale + "->");
        Log.LogWrite(" " + transform.gameObject.transform.localScale+"\n");
      }*/
      Component[] components = gameObject.GetComponents<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.Combat?.WL(0, "Component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID());
      }
      components = gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.Combat?.WL(0, "Child component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID());
      }
      /*components = unit.GameRep.gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.LogWrite(0, "Actor component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID(), true);
      }*/
      ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in particleSystems) {
        ps.Play(true);
      }
      Renderer[] spawnRenderers = gameObject.GetComponentsInChildren<Renderer>(true);
      foreach (Renderer renderer in spawnRenderers) {
        foreach (Material material in renderer.materials) {
          Log.Combat?.WL(0, "Renderer:" + renderer.GetType() + ":" + renderer.name + " material:" + material.name);
          CustomMaterialInfo mInfo = spawnPart.findMaterialInfo(material.name);
          if (mInfo != null) {
            Log.Combat?.WL(1, "need custom processing");
            if (string.IsNullOrEmpty(mInfo.shader) == false) {
              GameObject shaderGo = null;
              if (mInfo.shader != def.Chassis.PrefabIdentifier) {
                shaderGo =  def.DataManager.PooledInstantiate(mInfo.shader, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
              } else {
                shaderGo = rep.gameObject;
              }
              if (shaderGo != null) {
                Renderer shaderRenderer = shaderGo.GetComponentInChildren<Renderer>();
                if (shaderRenderer != null) {
                  Shader shader = shaderRenderer.material.shader;
                  Log.Combat?.WL(2, "shader real name:" + shader.name);
                  material.shader = shader;
                } else {
                  Log.Combat?.WL(2, "can't found renderer in shader carrier " + mInfo.shader);
                }
                if (mInfo.shader != def.Chassis.PrefabIdentifier) {
                  def.DataManager.PoolGameObject(mInfo.shader, shaderGo);
                }
              } else {
                Log.Combat?.WL(2, "can't found shader carrier " + mInfo.shader);
              }
            }
            if (mInfo.shaderKeyWords.Count > 0) {
              material.shaderKeywords = mInfo.shaderKeyWords.ToArray();
              Log.Combat?.WL(2, "shaders keywords updated " + material.shaderKeywords);
            }
          } else {
            Log.Combat?.WL(1, "no additional custom processing");
          }
        }
      }
      /*if (_BumpMap != null) newPrototype.materials[lod].SetTexture("_BumpMap", _BumpMap);
      if (_MainTex != null) newPrototype.materials[lod].SetTexture("_MainTex", _MainTex);
      if (_OcculusionMap != null) newPrototype.materials[lod].SetTexture("_OcculusionMap", _OcculusionMap);
      if (_Transmission != null) newPrototype.materials[lod].SetTexture("_Transmission", _Transmission);
      if (_MetallicGlossMap != null) newPrototype.materials[lod].SetTexture("_MetallicGlossMap", _MetallicGlossMap);*/
      //Texture albedo = spawnRenderer.material.GetTexture("_MainTex");
      //Texture metallic = spawnRenderer.material.GetTexture("_MetallicGlossMap");
      //Texture normalMap = spawnRenderer.material.GetTexture("_BumpMap");
      //Texture occulsionMap = spawnRenderer.material.GetTexture("_OcculusionMap");
      /*Log.LogWrite("Textures get\n");
      //spawnRenderer.material = GameObject.Instantiate(actorRenderer.material);
      //spawnRenderer.material.SetTexture("_BumpMap", null);
      spawnRenderer.material.SetTexture("_DamageAlbedoMap", null);
      spawnRenderer.material.SetTexture("_DamageNormalMap", null);
      spawnRenderer.material.SetTexture("_DetailAlbedoMap", null);
      spawnRenderer.material.SetTexture("_DetailMask", null);
      spawnRenderer.material.SetTexture("_DetailNormalMap", null);
      spawnRenderer.material.SetTexture("_EmblemMap", null);
      spawnRenderer.material.SetTexture("_EmissionMap", null);
      //spawnRenderer.material.SetTexture("_MainTex", null);
      //spawnRenderer.material.SetTexture("_MetallicGlossMap", null);
      //spawnRenderer.material.SetTexture("_OcclusionMap", null);
      spawnRenderer.material.SetTexture("_PaintScheme", null);
      spawnRenderer.material.SetTexture("_PaintSchemeOverride", null);
      spawnRenderer.material.SetTexture("_ParallaxMap", null);
      Log.LogWrite("Textures set\n");*/
      /*AudioSource audioSource = gameObject.GetComponentInChildren<AudioSource>();
      if (audioSource != null) {
        Log.LogWrite(" audioSource exists in childs\n");
        AudioClip clip = audioSource.clip;
        if (clip != null) {
          Log.LogWrite(" clip exists. loadState:" + clip.loadState + " length:" + clip.length + " name:" + clip.name + " loadType:" + clip.loadType + "\n");
          if (clip.loadState == AudioDataLoadState.Unloaded) {
            Log.LogWrite(" load audio data:" + clip.LoadAudioData());
            Log.LogWrite(" loadState:" + clip.loadState + " length:" + clip.length + "\n");
          }
        }
        audioSource.loop = true;
        Log.LogWrite(" playing\n");
        audioSource.minDistance = 0f;
        audioSource.Play();
      }*/
      /*AudioClip rotorClip = Resources.Load<AudioClip>("Sounds/helicopter-hovering-01");
      if (rotorClip != null) {
        Log.LogWrite(" rotorClip exists. loadState:" + rotorClip.loadState + " length:" + rotorClip.length + " name:" + rotorClip.name + " loadType:" + rotorClip.loadType + "\n");
      } else {
        Log.LogWrite(" rotor clip not loaded\n");
      }*/
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      if (component != null) {
        Log.Combat?.WL(0, "ParticleSystem for " + spawnPart.prefab + " found");
        //component.transform.localScale.Set(scale.x, scale.y, scale.z);
        component.transform.parent = def.IsVehicle() ? rep.getAttachVehicle(Location) : rep.getAttachMech(Location);
        component.transform.localPosition = spawnPart.prefabTransform.offset.vector;
        component.transform.localRotation = rotation;
        gameObject.SetActive(true);
        component.Stop(true);
        component.Clear(true);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
      } else {
        Log.Combat?.WL(0, "no particle system for " + spawnPart.prefab);
      }
      GenericAnimatedComponent acomponent = null;
      try {
        if (spawnPart.AnimationType == "VTOLBody") {
          acomponent = gameObject.AddComponent<VTOLBodyAnimation>();
        } else
        if (spawnPart.AnimationType == "SimpleRotator") {
          acomponent = gameObject.AddComponent<SimpleRotator>();
        } else
        if (spawnPart.AnimationType == "Turret") {
          acomponent = gameObject.AddComponent<TurretAnimator>();
        } else
        if (spawnPart.AnimationType == "WeaponMountPoint") {
          acomponent = gameObject.AddComponent<WeaponMountPoint>();
        } else
        if (spawnPart.AnimationType == "Animation") {
          acomponent = gameObject.AddComponent<CustomMoveAnimator>();
        } else
        if (spawnPart.AnimationType == "MechTurret") {
          acomponent = gameObject.AddComponent<MechTurretAnimation>();
        } else
        if (spawnPart.AnimationType == "CustomQuadLegController") {
          acomponent = gameObject.AddComponent<CustomQuadLegController>();
        } else { 
        //if (spawnPart.AnimationType == "QuadBody") {
          //acomponent = gameObject.AddComponent<QuadBodyAnimation>();
        //} else {
          acomponent = gameObject.AddComponent<GenericAnimatedComponent>();
        }
        if (acomponent != null) {
          acomponent.Init(null, Location, spawnPart.Data, spawnPart.prefab);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
      gameObject.SetActive(true);
      //if (animatedParts.ContainsKey(unit) == false) {
      //  Log.LogWrite("new list\n");
      //  animatedParts.Add(unit, new List<GameObject>());
      //};
      //animatedParts[unit].Add(gameObject);
      //Log.LogWrite("animatedParts.Count = " + animatedParts[unit].Count + "\n");
    }
    public static void SpawnAnimatedPart(ICombatant unit, CustomPart spawnPart, int Location) {
      if (unit.GameRep == null) {
        return;
      }
      if (unit.GameRep.gameObject == null) {
        return;
      }
      Log.Combat?.WL(0, "SpawnAnimatedPart " + unit.DisplayName + " " + spawnPart.prefab + " " + spawnPart.AnimationType + " " + spawnPart.Data + " " + Location + " " + spawnPart.prefabTransform.offset + " " + spawnPart.prefabTransform.scale + " " + spawnPart.prefabTransform.rotate);
      Renderer actorRenderer = unit.GameRep.gameObject.GetComponentInChildren<Renderer>();
      GameObject gameObject = null;
      if (string.IsNullOrEmpty(spawnPart.prefab) == false) {
        gameObject = unit.Combat.DataManager.PooledInstantiate(spawnPart.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if ((UnityEngine.Object)gameObject == (UnityEngine.Object)null) {
          Log.Combat?.WL(0, "Can't find " + spawnPart.prefab + " in in-game prefabs");
          if (CACMain.Core.AdditinalFXObjects.ContainsKey(spawnPart.prefab)) {
            Log.Combat?.WL(0, "Found in additional prefabs");
            gameObject = GameObject.Instantiate(CACMain.Core.AdditinalFXObjects[spawnPart.prefab]);
          } else {
            Log.Combat?.WL(1, "can't spawn prefab " + spawnPart.prefab + " it is absent in pool,in-game assets and external assets", true);
            return;
          }
        }
      } else {
        Log.Combat?.WL(1, "creating empty object");
        gameObject = new GameObject();
      }
      Transform parentTransform = null;
      if (string.IsNullOrEmpty(spawnPart.boneName) == false) {
        Transform[] transforms = unit.GameRep.gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform bone in transforms) {
          if (bone.name == spawnPart.boneName) {
            Log.Combat?.WL(1, "parent bone found");
            parentTransform = bone;
            break;
          }
        }
      }
      if (parentTransform == null) { parentTransform = unit.GameRep.GetVFXTransform(Location); };
      gameObject.transform.parent = parentTransform;
      gameObject.transform.localPosition = spawnPart.prefabTransform.offset.vector;
      Quaternion rotation = Quaternion.Euler(spawnPart.prefabTransform.rotate.vector);
      gameObject.transform.localRotation = rotation;
      gameObject.transform.localScale = spawnPart.prefabTransform.scale.vector;
      /*Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
      foreach (Transform transform in transforms) {
        Log.LogWrite("Transform:"+transform.name+" scale:"+ transform.gameObject.transform.localScale + "->");
        Log.LogWrite(" " + transform.gameObject.transform.localScale+"\n");
      }*/
      Component[] components = gameObject.GetComponents<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.Combat?.WL(0, "Component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID());
      }
      components = gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.Combat?.WL(0, "Child component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID());
      }
      /*components = unit.GameRep.gameObject.GetComponentsInChildren<Component>();
      foreach (Component cmp in components) {
        if (cmp == null) { continue; }
        Log.LogWrite(0, "Actor component:" + cmp.GetType() + " " + cmp.name + " id:" + cmp.GetInstanceID() + " owner:" + cmp.gameObject.GetInstanceID(), true);
      }*/
      ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in particleSystems) {
        ps.Play(true);
      }
      Renderer[] spawnRenderers = gameObject.GetComponentsInChildren<Renderer>(true);
      foreach (Renderer renderer in spawnRenderers) {
        foreach (Material material in renderer.materials) {
          Log.Combat?.WL(0, "Renderer:" + renderer.GetType() + ":" + renderer.name + " material:" + material.name);
          CustomMaterialInfo mInfo = spawnPart.findMaterialInfo(material.name);
          if (mInfo != null) {
            Log.Combat?.WL(1, "need custom processing");
            if (string.IsNullOrEmpty(mInfo.shader) == false) {
              GameObject shaderGo = null;
              bool noPoolShader = false;
              Mech mech = unit as Mech;
              Vehicle vehicle = unit as Vehicle;
              if (mech != null) {
                if(mInfo.shader == mech.MechDef.Chassis.PrefabIdentifier) {
                  shaderGo = unit.GameRep.gameObject;
                  noPoolShader = true;
                }
              } else if(vehicle != null) {
                if(mInfo.shader == vehicle.VehicleDef.Chassis.PrefabIdentifier) {
                  shaderGo = unit.GameRep.gameObject;
                  noPoolShader = true;
                }
              }
              if (shaderGo == null) {
                shaderGo = unit.Combat.DataManager.PooledInstantiate(mInfo.shader, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
              }
              if (shaderGo != null) {
                Renderer shaderRenderer = shaderGo.GetComponentInChildren<Renderer>();
                if (shaderRenderer != null) {
                  Shader shader = shaderRenderer.material.shader;
                  Log.Combat?.WL(2, "shader real name:" + shader.name);
                  material.shader = shader;
                } else {
                  Log.Combat?.WL(2, "can't found renderer in shader carrier " + mInfo.shader);
                }
                if(noPoolShader == false) unit.Combat.DataManager.PoolGameObject(mInfo.shader, shaderGo);
              } else {
                Log.Combat?.WL(2, "can't found shader carrier " + mInfo.shader);
              }
            }
            if (mInfo.shaderKeyWords.Count > 0) {
              material.shaderKeywords = mInfo.shaderKeyWords.ToArray();
              Log.Combat?.WL(2, "shaders keywords updated " + material.shaderKeywords);
            }
          } else {
            Log.Combat?.WL(1, "no additional custom processing");
          }
        }
      }
      /*if (_BumpMap != null) newPrototype.materials[lod].SetTexture("_BumpMap", _BumpMap);
      if (_MainTex != null) newPrototype.materials[lod].SetTexture("_MainTex", _MainTex);
      if (_OcculusionMap != null) newPrototype.materials[lod].SetTexture("_OcculusionMap", _OcculusionMap);
      if (_Transmission != null) newPrototype.materials[lod].SetTexture("_Transmission", _Transmission);
      if (_MetallicGlossMap != null) newPrototype.materials[lod].SetTexture("_MetallicGlossMap", _MetallicGlossMap);*/
      //Texture albedo = spawnRenderer.material.GetTexture("_MainTex");
      //Texture metallic = spawnRenderer.material.GetTexture("_MetallicGlossMap");
      //Texture normalMap = spawnRenderer.material.GetTexture("_BumpMap");
      //Texture occulsionMap = spawnRenderer.material.GetTexture("_OcculusionMap");
      /*Log.LogWrite("Textures get\n");
      //spawnRenderer.material = GameObject.Instantiate(actorRenderer.material);
      //spawnRenderer.material.SetTexture("_BumpMap", null);
      spawnRenderer.material.SetTexture("_DamageAlbedoMap", null);
      spawnRenderer.material.SetTexture("_DamageNormalMap", null);
      spawnRenderer.material.SetTexture("_DetailAlbedoMap", null);
      spawnRenderer.material.SetTexture("_DetailMask", null);
      spawnRenderer.material.SetTexture("_DetailNormalMap", null);
      spawnRenderer.material.SetTexture("_EmblemMap", null);
      spawnRenderer.material.SetTexture("_EmissionMap", null);
      //spawnRenderer.material.SetTexture("_MainTex", null);
      //spawnRenderer.material.SetTexture("_MetallicGlossMap", null);
      //spawnRenderer.material.SetTexture("_OcclusionMap", null);
      spawnRenderer.material.SetTexture("_PaintScheme", null);
      spawnRenderer.material.SetTexture("_PaintSchemeOverride", null);
      spawnRenderer.material.SetTexture("_ParallaxMap", null);
      Log.LogWrite("Textures set\n");*/
      /*AudioSource audioSource = gameObject.GetComponentInChildren<AudioSource>();
      if (audioSource != null) {
        Log.LogWrite(" audioSource exists in childs\n");
        AudioClip clip = audioSource.clip;
        if (clip != null) {
          Log.LogWrite(" clip exists. loadState:" + clip.loadState + " length:" + clip.length + " name:" + clip.name + " loadType:" + clip.loadType + "\n");
          if (clip.loadState == AudioDataLoadState.Unloaded) {
            Log.LogWrite(" load audio data:" + clip.LoadAudioData());
            Log.LogWrite(" loadState:" + clip.loadState + " length:" + clip.length + "\n");
          }
        }
        audioSource.loop = true;
        Log.LogWrite(" playing\n");
        audioSource.minDistance = 0f;
        audioSource.Play();
      }*/
      /*AudioClip rotorClip = Resources.Load<AudioClip>("Sounds/helicopter-hovering-01");
      if (rotorClip != null) {
        Log.LogWrite(" rotorClip exists. loadState:" + rotorClip.loadState + " length:" + rotorClip.length + " name:" + rotorClip.name + " loadType:" + rotorClip.loadType + "\n");
      } else {
        Log.LogWrite(" rotor clip not loaded\n");
      }*/
      ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
      if (component != null) {
        Log.Combat?.WL(0, "ParticleSystem for " + spawnPart.prefab + " found");
        //component.transform.localScale.Set(scale.x, scale.y, scale.z);
        component.transform.parent = unit.GameRep.GetVFXTransform(Location);
        component.transform.localPosition = spawnPart.prefabTransform.offset.vector;
        component.transform.localRotation = rotation;
        gameObject.SetActive(true);
        component.Stop(true);
        component.Clear(true);
        BTCustomRenderer.SetVFXMultiplier(component);
        component.Play(true);
      } else {
        Log.Combat?.WL(0, "no particle system for " + spawnPart.prefab);
      }
      GenericAnimatedComponent acomponent = null;
      try {
        if (spawnPart.AnimationType == "VTOLBody") {
          acomponent = gameObject.AddComponent<VTOLBodyAnimation>();
        } else
        if (spawnPart.AnimationType == "SimpleRotator") {
          acomponent = gameObject.AddComponent<SimpleRotator>();
        } else
        if (spawnPart.AnimationType == "Turret") {
          acomponent = gameObject.AddComponent<TurretAnimator>();
        } else
        if (spawnPart.AnimationType == "WeaponMountPoint") {
          acomponent = gameObject.AddComponent<WeaponMountPoint>();
        } else
        if (spawnPart.AnimationType == "Animation") {
          acomponent = gameObject.AddComponent<CustomMoveAnimator>();
        } else
        if (spawnPart.AnimationType == "MechTurret") {
          acomponent = gameObject.AddComponent<MechTurretAnimation>();
        } else
        if (spawnPart.AnimationType == "CustomQuadLegController") {
          acomponent = gameObject.AddComponent<CustomQuadLegController>();
          //        } else
          //        if (spawnPart.AnimationType == "QuadBody") {
          //          acomponent = gameObject.AddComponent<QuadBodyAnimation>();
        } else {
          acomponent = gameObject.AddComponent<GenericAnimatedComponent>();
        }
        if (acomponent != null) {
          acomponent.Init(unit, Location, spawnPart.Data, spawnPart.prefab);
        }
      } catch (Exception e) {
        Log.Combat?.WL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
      gameObject.SetActive(true);
      if (animatedParts.ContainsKey(unit) == false) {
        Log.Combat?.WL(0, "new list");
        animatedParts.Add(unit, new List<GameObject>());
      };
      animatedParts[unit].Add(gameObject);
      Log.Combat?.WL(0, "animatedParts.Count = " + animatedParts[unit].Count);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HandleDeath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AbstractActor_HandleDeath {
    public static void TieToGround(Transform transform, string tname, MapMetaData meta) {
      Log.Combat?.W(1, tname + ":" + transform.position + " rot:" + transform.rotation.eulerAngles);
      float y = meta.GetLerpedHeightAt(transform.position) + Core.Settings.DeathHeight - transform.position.y;
      Log.Combat?.W(1,"->  down " + y);
      transform.Translate(Vector3.up * y, Space.World);
      //transform.position.Set(transform.position.x, y, transform.position.z);
      Log.Combat?.W(1,"-> " + transform.position + " rot: " + transform.rotation.eulerAngles);
    }
    public static void Postfix(AbstractActor __instance) {
      try {
        if (__instance.IsDead) {
          Log.Combat?.WL(0,"AbstractActor.HandleDeath " + __instance.DisplayName + ":" + __instance.GUID);
          UnitsAnimatedPartsHelper.Clear(__instance, false);
          Vehicle vehicle = __instance as Vehicle;
          if (vehicle == null) {
            Log.Combat?.WL(1, "not a vehicle");
            return;
          }
          VehicleRepresentation vRep = vehicle.GameRep;
          UnitCustomInfo info = vehicle.GetCustomInfo();
          if (info == null) {
            Log.Combat?.WL(1, "no custom info");
            return;
          }
          if (info.TieToGroundOnDeath) {
            Dictionary<GenericAnimatedComponent, Vector3> keepPos = UnitsAnimatedPartsHelper.storePositions(__instance);
            TieToGround(vRep.TurretAttach.transform, "TurretAttach", __instance.Combat.MapMetaData);
            TieToGround(vRep.BodyAttach.transform, "BodyAttach", __instance.Combat.MapMetaData);
            TieToGround(vRep.TurretLOS.transform, "TurretLOS", __instance.Combat.MapMetaData);
            TieToGround(vRep.LeftSideLOS.transform, "LeftSideLOS", __instance.Combat.MapMetaData);
            TieToGround(vRep.RightSideLOS.transform, "RightSideLOS", __instance.Combat.MapMetaData);
            TieToGround(vRep.leftVFXTransform.transform, "leftVFXTransform", __instance.Combat.MapMetaData);
            TieToGround(vRep.rightVFXTransform.transform, "rightVFXTransform", __instance.Combat.MapMetaData);
            TieToGround(vRep.rearVFXTransform.transform, "rearVFXTransform", __instance.Combat.MapMetaData);
            TieToGround(vRep.thisTransform.transform, "thisTransform", __instance.Combat.MapMetaData);
            UnitsAnimatedPartsHelper.RestorePositions(keepPos);
          }
          __instance.OnDeathAnimation();
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("OnLocationDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(Vector3), typeof(WeaponHitInfo), typeof(DamageType) })]
  public static class Mech_OnLocationDestroyed {
    public static void Postfix(Mech __instance, ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      try {
        Log.Combat?.WL(0,"Mech.OnLocationDestroyed " + __instance.DisplayName + ":" + __instance.GUID);
        __instance.DestroyAnimation((int)location);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        AbstractActor.logger.LogException(e);
      }
    }
  }
}