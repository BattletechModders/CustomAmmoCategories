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
using BattleTech.Rendering.UI;
using BattleTech.Rendering.UrbanWarfare;
using CustAmmoCategories;
using FogOfWar;
using Harmony;
using HBS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomUnits {
  [HarmonyPatch(typeof(UICreep))]
  [HarmonyPatch("RefreshCache")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_RefreshCache {
    private static bool CheckShader(Shader shader, out int pass) {
      int num1 = shader.name == "BattleTech Mech Standard" ? 1 : 0;
      bool flag1 = shader.name == "BattleTech Standard" || shader.name == "Urban/Urban Building";
      bool flag2 = shader.name == "Custom/WorldUVShader";
      pass = flag2 ? 1 : 5;
      int num2 = flag1 ? 1 : 0;
      return (num1 | num2 | (flag2 ? 1 : 0)) != 0;
    }
    public static bool Prefix(UICreep __instance,ref Bounds ___creepBounds) {
      try {
        __instance.renderCache.Clear();
        if (__instance.useBakedRenderers && __instance.bakedMeshRenderers.Length != 0) {
          foreach (MeshRenderer bakedMeshRenderer in __instance.bakedMeshRenderers) {
            int pass;
            if ((UnityEngine.Object)bakedMeshRenderer != (UnityEngine.Object)null && bakedMeshRenderer.gameObject.activeInHierarchy && ((UnityEngine.Object)bakedMeshRenderer.sharedMaterial != (UnityEngine.Object)null && CheckShader(bakedMeshRenderer.sharedMaterial.shader, out pass))) {
              MeshFilter component = bakedMeshRenderer.GetComponent<MeshFilter>();
              if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component.sharedMesh == (UnityEngine.Object)null))
                __instance.renderCache.Add(new UICreep.RenderCacheObject((Renderer)bakedMeshRenderer, pass));
            }
          }
        } else {
          foreach (SkinnedMeshRenderer componentsInChild in __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true)) {
            int pass;
            if ((UnityEngine.Object)componentsInChild.sharedMaterial != (UnityEngine.Object)null && CheckShader(componentsInChild.sharedMaterial.shader, out pass) && (componentsInChild.gameObject.activeInHierarchy && componentsInChild.enabled)) {
              if (componentsInChild.sharedMesh != null) {
                if (componentsInChild.sharedMesh.bounds != null) {
                  __instance.renderCache.Add(new UICreep.RenderCacheObject((Renderer)componentsInChild, pass));
                  ___creepBounds.Encapsulate(componentsInChild.sharedMesh.bounds);
                } else {
                  Log.TWL(0, "UICreep.RefreshCache "+ componentsInChild.name+" have null sharedMesh.bounds");
                }
              } else {
                Log.TWL(0, "UICreep.RefreshCache " + componentsInChild.name + " have null sharedMesh");
              }
            }
          }
          LODGroup[] componentsInChildren = __instance.GetComponentsInChildren<LODGroup>();
          if (componentsInChildren != null) {
            foreach (LODGroup lodGroup in componentsInChildren) {
              LOD[] loDs = lodGroup.GetLODs();
              if (loDs.Length != 0) {
                foreach (Renderer renderer in loDs[0].renderers) {
                  if (!((UnityEngine.Object)renderer == (UnityEngine.Object)null)) {
                    DestructibleObject component1 = renderer.GetComponent<DestructibleObject>();
                    bool flag = false;
                    if ((UnityEngine.Object)component1 != (UnityEngine.Object)null)
                      flag = component1.isFlimsy;
                    int pass;
                    if (CheckShader(renderer.sharedMaterial.shader, out pass) && !flag) {
                      MeshFilter component2 = renderer.GetComponent<MeshFilter>();
                      if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null) && !((UnityEngine.Object)component2.sharedMesh == (UnityEngine.Object)null)) {
                        __instance.renderCache.Add(new UICreep.RenderCacheObject(renderer, pass));
                        ___creepBounds.Encapsulate(component2.sharedMesh.bounds);
                      }
                    }
                  }
                }
              }
            }
          }
          if (__instance.renderCache.Count == 0) {
            DestructibleUrbanBuilding component1 = __instance.GetComponent<DestructibleUrbanBuilding>();
            if ((UnityEngine.Object)component1 != (UnityEngine.Object)null) {
              foreach (Renderer componentsInChild in component1.buildingWholeGameObject.GetComponentsInChildren<Renderer>()) {
                int pass;
                if (CheckShader(componentsInChild.sharedMaterial.shader, out pass)) {
                  MeshFilter component2 = componentsInChild.GetComponent<MeshFilter>();
                  if (!((UnityEngine.Object)component2 == (UnityEngine.Object)null) && !((UnityEngine.Object)component2.sharedMesh == (UnityEngine.Object)null)) {
                    __instance.renderCache.Add(new UICreep.RenderCacheObject(componentsInChild, pass));
                    ___creepBounds.Encapsulate(component2.sharedMesh.bounds);
                  }
                }
              }
            }
          }
          if ((UnityEngine.Object)__instance.GetComponent<MechRepresentation>() != (UnityEngine.Object)null) {
            foreach (MeshRenderer componentsInChild in __instance.gameObject.GetComponentsInChildren<MeshRenderer>()) {
              int pass;
              if ((UnityEngine.Object)componentsInChild != (UnityEngine.Object)null && componentsInChild.gameObject.activeInHierarchy && ((UnityEngine.Object)componentsInChild.sharedMaterial != (UnityEngine.Object)null && CheckShader(componentsInChild.sharedMaterial.shader, out pass))) {
                MeshFilter component = componentsInChild.GetComponent<MeshFilter>();
                if (!((UnityEngine.Object)component == (UnityEngine.Object)null) && !((UnityEngine.Object)component.sharedMesh == (UnityEngine.Object)null))
                  __instance.renderCache.Add(new UICreep.RenderCacheObject((Renderer)componentsInChild, pass));
              }
            }
          }
        }
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetLoadAnimation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class MechRepresentation_SetLoadAnimation {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetLoadAnimation();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("UpdateHeatSetting")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_UpdateHeatSetting {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._UpdateHeatSetting();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("UpdateLegDamageAnimFlags")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LocationDamageLevel), typeof(LocationDamageLevel) })]
  public static class MechRepresentation_UpdateLegDamageAnimFlags {
    public static bool Prefix(MechRepresentation __instance, LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._UpdateLegDamageAnimFlags(leftLegDamage, rightLegDamage);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("TriggerMeleeTransition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_TriggerMeleeTransition {
    public static bool Prefix(MechRepresentation __instance, bool meleeIn) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._TriggerMeleeTransition(meleeIn);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ClearLoadState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_ClearLoadState {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ClearLoadState();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayComponentCritVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_PlayComponentCritVFX {
    public static bool Prefix(MechRepresentation __instance, int location) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PlayComponentCritVFX(location);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayDeathFloatie")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DeathMethod) })]
  public static class MechRepresentation_PlayDeathFloatie {
    public static bool Prefix(MechRepresentation __instance, DeathMethod deathMethod) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PlayDeathFloatie(deathMethod);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetMeleeIdleState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_SetMeleeIdleState {
    public static bool Prefix(MechRepresentation __instance, bool isMelee) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetMeleeIdleState(isMelee);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ToggleRandomIdles")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_ToggleRandomIdles {
    public static bool Prefix(MechRepresentation __instance, bool shouldIdle) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ToggleRandomIdles(shouldIdle);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetIdleAnimState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_SetIdleAnimState {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetIdleAnimState();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayImpactAnimSimple")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection), typeof(float) })]
  public static class MechRepresentation_PlayImpactAnimSimple {
    public static bool Prefix(MechRepresentation __instance, AttackDirection attackDirection, float totalDamage) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PlayImpactAnimSimple(attackDirection, totalDamage);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("SetUnsteadyAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_SetUnsteadyAnim {
    public static bool Prefix(MechRepresentation __instance, bool isUnsteady) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._SetUnsteadyAnim(isUnsteady);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ForceKnockdown")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector2) })]
  public static class MechRepresentation_ForceKnockdown {
    public static bool Prefix(MechRepresentation __instance, Vector2 attackDirection) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ForceKnockdown(attackDirection);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StartJumpjetAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StartJumpjetAudio {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StartJumpjetAudio();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StopJumpjetAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StopJumpjetAudio {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StopJumpjetAudio();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StartJumpjetEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StartJumpjetEffect {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StartJumpjetEffect();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StopJumpjetEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StopJumpjetEffect {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._StopJumpjetEffect();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("ToggleHeadlights")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechRepresentation_ToggleHeadlights {
    public static bool Prefix(MechRepresentation __instance, bool headlightsActive) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._ToggleHeadlights(headlightsActive);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("HandleDeathOnLoad")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DeathMethod), typeof(int) })]
  public static class MechRepresentation_HandleDeathOnLoad {
    public static bool Prefix(MechRepresentation __instance, DeathMethod deathMethod, int location) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._HandleDeathOnLoad(deathMethod, location);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayAlliesReportDeathVO")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_PlayAlliesReportDeathVO {
    public static bool Prefix(MechRepresentation __instance) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._PlayAlliesReportDeathVO();
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("TriggerFootFall")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_TriggerFootFall {
    public static bool Prefix(MechRepresentation __instance, int leftFoot) {
      try {
        CustomMechRepresentation custMechRep = __instance as CustomMechRepresentation;
        if (custMechRep != null) {
          custMechRep._TriggerFootFall(leftFoot);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("PlayMeleeAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_PlayMeleeAnim {
    public static void Prefix(ActorMovementSequence __instance, ref bool __state, bool ___playedMelee) {
      try {
        __state = ___playedMelee;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(ActorMovementSequence __instance, ref bool __state, bool ___playedMelee, ICombatant ___meleeTarget) {
      try {
        if (___meleeTarget != null) {
          if((___playedMelee == true)&&(__state == false)) {
            CustomMechRepresentation custRep = __instance.owningActor.GameRep as CustomMechRepresentation;
            if (custRep != null) {
              if (custRep.parentActor != null) {
                custRep.PlayMeleeAnim(custRep.parentActor.Combat.MeleeRules.GetMeleeHeightFromAttackType(__instance.meleeType), ___meleeTarget);
              }
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public partial class CustomMechRepresentation {
    public virtual float GetCameraDistance(DialogCameraDistance cameraDistance) {
      switch (cameraDistance) {
        case DialogCameraDistance.Far:
        return 800f;
        case DialogCameraDistance.Medium:
        return 400f;
        case DialogCameraDistance.Close:
        return 200f;
        default:
        CameraControl.cameraLogger.LogError((object)"ERROR: no camera distance found for camera, returning default 100");
        return 100f;
      }
    }
    public virtual float GetCameraHeight(DialogCameraHeight cameraHeight, float distance) {
      switch (cameraHeight) {
        case DialogCameraHeight.Default:
        return Mathf.Min(CameraControl.Instance.CurrentHeight, distance / 2f);
        case DialogCameraHeight.Low:
        return 32f;
        case DialogCameraHeight.High:
        return distance / 2f;
        default:
        CameraControl.cameraLogger.LogError((object)"ERROR: no camera height found for camera, returning default");
        return CameraControl.Instance.CurrentHeight;
      }
    }
    public virtual void ClearFocalPoint() {
      if (this.focalPoint == null) { return; }
      GameObject.Destroy(this.focalPoint);
      this.focalPoint = null;
    }
    public virtual GameObject focalPoint { get; set; } = null;
    public virtual int nextRevealatronIndex { get; set; } = 0;
    public virtual int GetNextRevealatronIndex() {
      int revealatronIndex = this.nextRevealatronIndex;
      ++this.nextRevealatronIndex;
      return revealatronIndex;
    }

    public virtual void _SetLoadAnimation() {
      if (this.parentActor.IsDead) { return; }
      this._UpdateHeatSetting();
      this._UpdateLegDamageAnimFlags(this.parentMech.LeftLegDamageLevel, this.parentMech.RightLegDamageLevel);
      if (this.parentMech.IsOrWillBeProne) {
        this.thisAnimator.SetTrigger("LoadStateDowned");
        this._ResetHitReactFlags();
        this.thisAnimator.SetBool("KnockedDown", true);
        this.thisAnimator.ResetTrigger("Stand");
      } else if (this.parentMech.IsShutDown) {
        this.thisAnimator.SetTrigger("LoadStateShutdown");
        this.thisAnimator.SetTrigger("LoadStateShutdownAdditive");
      } else {
        if (!this.parentMech.BracedLastRound && !this.parentMech.inMeleeIdle) { return; }
        this._TriggerMeleeTransition(true);
      }
      //foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._SetLoadAnimation(); }
    }
    public virtual void _ClearLoadState() { this.thisAnimator.SetInteger("LoadState", 0); }
    public virtual void _SetupDamageStates(Mech mech, MechDef mechDef) {
      foreach (ChassisLocations chassisLocations in Enum.GetValues(typeof(ChassisLocations))) {
        switch (chassisLocations) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default:
          LocationDamageLevel locationDamageLevel = mech == null ? mechDef.GetLocationLoadoutDef(chassisLocations).DamageLevel : mech.GetLocationDamageLevel(chassisLocations);
          if (locationDamageLevel >= LocationDamageLevel.Penalized)
            this.PlayPersistentDamageVFX((int)chassisLocations);
          if (locationDamageLevel == LocationDamageLevel.Destroyed) {
            this.CollapseLocation((int)chassisLocations, Vector3.zero, mech.Combat.IsLoadingFromSave);
            continue;
          }
          continue;
        }
      }
    }
    public virtual void SetupJumpJets() {
      this.jumpjetReps.Clear();
      Log.TWL(0, "CustomMechRepresentation.SetupJumpJets " + this.PrefabBase);
      if (this.HasOwnVisuals == false) { return; }
      string id1 = string.Format("chrPrfComp_{0}_centertorso_jumpjet", (object)this.PrefabBase);
      string id2 = string.Format("chrPrfComp_{0}_leftleg_jumpjet", (object)this.PrefabBase);
      string id3 = string.Format("chrPrfComp_{0}_rightleg_jumpjet", (object)this.PrefabBase);
      GameObject gameObject1 = this._Combat.DataManager.PooledInstantiate(id1, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject1 != (UnityEngine.Object)null) {
        this.RegisterRenderersMainHeraldry(gameObject1);
        JumpjetRepresentation component = gameObject1.GetComponent<JumpjetRepresentation>();
        if (mech != null)
          component.Init((ICombatant)mech, this.TorsoAttach, true, false, this.name);
        this.jumpjetReps.Add(component);
      }
      GameObject gameObject2 = this._Combat.DataManager.PooledInstantiate(id2, BattleTechResourceType.Prefab);
      if ((UnityEngine.Object)gameObject2 != (UnityEngine.Object)null) {
        this.RegisterRenderersMainHeraldry(gameObject2);
        JumpjetRepresentation component = gameObject2.GetComponent<JumpjetRepresentation>();
        if (mech != null)
          component.Init((ICombatant)mech, this.LeftLegAttach, true, false, this.name);
        this.jumpjetReps.Add(component);
      }
      GameObject gameObject3 = this._Combat.DataManager.PooledInstantiate(id3, BattleTechResourceType.Prefab);
      if (gameObject3 != null) {
        this.RegisterRenderersMainHeraldry(gameObject3);
        JumpjetRepresentation component1 = gameObject3.GetComponent<JumpjetRepresentation>();
        if (mech != null)
          component1.Init((ICombatant)mech, this.RightLegAttach, true, false, this.name);
        this.jumpjetReps.Add(component1);
      }
      if (string.IsNullOrEmpty(Core.Settings.CustomJumpJetsComponentPrefab)) { return; }
      GameObject jumpJetSrcPrefab = this._Combat.DataManager.PooledInstantiate(Core.Settings.CustomJumpJetsComponentPrefab, BattleTechResourceType.Prefab);
      if (jumpJetSrcPrefab != null) {
        Transform jumpJetSrc = jumpJetSrcPrefab.transform.FindRecursive(Core.Settings.CustomJumpJetsPrefabSrcObjectName);
        if (jumpJetSrc != null) {
          Log.WL(1, "JetStreamsAttaches:"+ this.customRep.CustomDefinition.JetStreamsAttaches.Count);
          foreach (string jumpjetAttachName in this.customRep.CustomDefinition.JetStreamsAttaches) {
            if (string.IsNullOrEmpty(jumpjetAttachName)) { continue; }
            Transform jumpJetAttach = this.gameObject.transform.FindRecursive(jumpjetAttachName);
            Log.WL(2, "custom jet attach:"+ jumpjetAttachName+" transform found: "+(jumpJetAttach == null?"false":"true"));
            if (jumpJetAttach == null) { continue; }
            GameObject jumpJetBase = new GameObject("jumpJet");
            jumpJetBase.transform.SetParent(jumpJetAttach);
            jumpJetBase.transform.localPosition = Vector3.zero;
            jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject jumpJet = GameObject.Instantiate(jumpJetSrc.gameObject);
            jumpJet.transform.SetParent(jumpJetBase.transform);
            jumpJet.transform.localPosition = Vector3.zero;
            jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
            jRep.Init(this.parentCombatant, jumpJetAttach, true, false, this.name);
            this.jumpjetReps.Add(jRep);
          }
        }
        this.parentActor.Combat.DataManager.PoolGameObject(Core.Settings.CustomJumpJetsComponentPrefab, jumpJetSrcPrefab);
      }
    }
    protected virtual void OnHeatChanged(MessageCenterMessage message) {
      if (!((message as HeatChangedMessage).affectedObjectGuid == this.parentMech.GUID))
        return;
      this._UpdateHeatSetting();
    }
    public virtual void _UpdateHeatSetting() {
      float num = 0.0f;
      if (this.isFakeOverheated)
        num = 1f;
      else if (this.parentMech.IsPastMaxHeat)
        num = 1f;
      else if (this.parentMech.IsOverheated)
        num = 0.4f;
      this.heatAmount.PropertyFloat = num;
      if (this.__propertyBlock != null) { this.__propertyBlock.UpdateProperties(); }
    }
    public override void OnCombatGameDestroyed() {
      GameRepresentation_OnCombatGameDestroyed();
      if (this.parentMech == null) { return; }
      this.parentMech.Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.OnHeatChanged, new ReceiveMessageCenterMessage(this.OnHeatChanged));
    }
    protected CustomVoices.AudioObject f_customAudioObject = null;
    public virtual CustomVoices.AudioObject customAudioObject {
      get {
        if (f_customAudioObject == null) {
          f_customAudioObject = this.gameObject.GetComponent<CustomVoices.AudioObject>();
          if (f_customAudioObject == null) {
            f_customAudioObject = this.gameObject.AddComponent<CustomVoices.AudioObject>();
          }
        }
        return f_customAudioObject;
      }
    }
    public override void StartPersistentAudio() {
      //GameRepresentation_StartPersistentAudio();
      if (isSlave == false) {
        Log.TWL(0, "CustomMechRepresentaion.StartPersistentAudio " + (this.parentMech == null ? "null" : this.parentMech.MechDef.ChassisID));
        foreach (string event_name in this.presistantAudioStart) {
          if (CustomVoices.AudioEngine.isInAudioManifest(event_name)) {
            Log.WL(1, event_name + " playing custom");
            this.customAudioObject.Play(event_name, true);
            continue;
          }
          //int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_start, this.audioObject);
          int num = (int)WwiseManager.PostEvent(event_name, this.audioObject);
          Log.WL(1, event_name + ":" + num);
        }
      }
    }
    public override void StopPersistentAudio() {
      //base.StopPersistentAudio();
      if (isSlave == false) {
        Log.TWL(0, "CustomMechRepresentaion.StopPersistentAudio " + (this.parentMech == null ? "null" : this.parentMech.MechDef.ChassisID));
        foreach (string event_name in this.presistantAudioStart) {
          if (CustomVoices.AudioEngine.isInAudioManifest(event_name)) {
            Log.WL(1, event_name + " stopping custom");
            this.customAudioObject.Stop(event_name);
            continue;
          }
        }
        foreach (string event_name in this.presistantAudioStop) {
          //int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_stop, this.audioObject);
          int num = (int)WwiseManager.PostEvent(event_name, this.audioObject);
          Log.WL(1, event_name + ":" + num);
        }
      }
    }
    public virtual void PlayMovementStartAudio() {
      this.PlayMovementStartAudio(false);
    }
    public virtual void PlayMovementStopAudio() {
      this.PlayMovementStopAudio(false);
    }
    public virtual void PlayMovementStartAudio(bool forcedSlave) {
      //GameRepresentation_StartPersistentAudio();
      if (isSlave == false || (forcedSlave)) {
        Log.TWL(0, "CustomMechRepresentaion.PlayMovementStartAudio " + (this.parentMech == null ? "null" : this.parentMech.MechDef.ChassisID));
        foreach (string event_name in this.moveAudioStart) {
          //int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_start, this.audioObject);
          int num = (int)WwiseManager.PostEvent(event_name, this.rootParentRepresentation.audioObject);
          Log.WL(1, event_name + ":" + num);
        }
      }
    }
    public virtual void PlayMovementStopAudio(bool forcedSlave) {
      //base.StopPersistentAudio();
      if (isSlave == false || (forcedSlave)) {
        Log.TWL(0, "CustomMechRepresentaion.PlayMovementStopAudio " + (this.parentMech == null ? "null" : this.parentMech.MechDef.ChassisID));
        foreach (string event_name in this.moveAudioStop) {
          //int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_stop, this.audioObject);
          int num = (int)WwiseManager.PostEvent(event_name, this.rootParentRepresentation.audioObject);
          Log.WL(1, event_name + ":" + num);
        }
      }
    }
    protected override void Update() {
      //base.Update();
      PilotableActorRepresentation_Update();
      if (this.needsToRefreshCombinedMesh == false) { return; }
      if (this.VisibleToPlayer == false) { return; }
      if (this.mechMerges.Count == 0) { return; }
      foreach (CustomMechMeshMerge mechMerge in this.mechMerges) { mechMerge.RefreshCombinedMesh(true); }
      this.needsToRefreshCombinedMesh = false;
    }
    public override bool RefreshSurfaceType(bool forceUpdate = false) {
      return PilotableActorRepresentation_RefreshSurfaceType(forceUpdate);
    }
    protected override void LateUpdate() {
      if (this.SkipLateUpdate) { return; }
      PilotableActorRepresentation_LateUpdate();
      this.currentAnimTransition = this.thisAnimator.GetAnimatorTransitionInfo(0);
      this.currentAnimState = this.thisAnimator.GetCurrentAnimatorStateInfo(0);
      this.currentAnimStateHash = this.currentAnimState.fullPathHash;
      this.previousTransitionHash = this.previousAnimTransition.fullPathHash;
      this.previousAnimStateHash = this.previousAnimState.fullPathHash;
      if (this.currentAnimTransition.fullPathHash != this.previousTransitionHash)
        this.previousAnimTransition = this.currentAnimTransition;
      if (this.currentAnimStateHash != this.previousAnimStateHash) {
        if (this.currentAnimStateHash == this.idleStateEntryHash || this.currentAnimStateHash == this.idleStateMeleeEntryHash)
          this._SetIdleAnimState();
        if (this.previousAnimStateHash == this.standingHash) {
          if (isSlave == false) {
            string str = string.Format("Mech_Restart_{0}", (object)this.parentActor.GUID);
            AudioEventManager.CreateVOQueue(str, -1f, (MessageCenterMessage)null, (AkCallbackManager.EventCallback)null);
            AudioEventManager.QueueVOEvent(str, VOEvents.Mech_Stand_Restart, (AbstractActor)this.parentMech);
            AudioEventManager.StartVOQueue(0.0f);
          }
        }
        if ((this.currentAnimStateHash == this.groundDeathIdleHash || this.currentAnimStateHash == this.randomDeathIdleRandomizer) && !this.parentMech.Combat.IsLoadingFromSave) {
          if (isSlave == false) { this.PlayAlliesReportDeathVO(); }
        }
        this.previousAnimState = this.currentAnimState;
        if (isSlave == false) {
          int num = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_interrupted, this.audioObject);
        }
      }
      if (this.triggerFootVFX) {
        this._TriggerFootFall(this.leftFootVFX);
      }
      if (this.customFootFalls.Count > 0) {
        foreach (Transform foot in this.customFootFalls) {
          this.CustomFootFall(foot);
        }
        this.customFootFalls.Clear();
      }
    }

    public override Vector3 GetHitPosition(int location) {
      ArmorLocation armorLocation = (ArmorLocation)location;
      switch (armorLocation) {
        case ArmorLocation.None:
        case ArmorLocation.Invalid:
        return this.vfxCenterTorsoTransform.position;
        case ArmorLocation.Head:
        return this.vfxHeadTransform.position;
        case ArmorLocation.LeftArm:
        return this.vfxLeftArmTransform.position;
        case ArmorLocation.LeftTorso:
        case ArmorLocation.LeftTorso | ArmorLocation.LeftTorsoRear:
        return this.vfxLeftTorsoTransform.position;
        case ArmorLocation.CenterTorso:
        case ArmorLocation.CenterTorso | ArmorLocation.CenterTorsoRear:
        return this.vfxCenterTorsoTransform.position;
        case ArmorLocation.RightTorso:
        case ArmorLocation.RightTorso | ArmorLocation.RightTorsoRear:
        return this.vfxRightTorsoTransform.position;
        case ArmorLocation.RightArm:
        return this.vfxRightArmTransform.position;
        case ArmorLocation.LeftLeg:
        return this.vfxLeftLegTransform.position;
        case ArmorLocation.RightLeg:
        return this.vfxRightLegTransform.position;
        case ArmorLocation.LeftTorsoRear:
        return this.vfxLeftTorsoTransform.position;
        case ArmorLocation.CenterTorsoRear:
        return this.vfxCenterTorsoTransform.position;
        case ArmorLocation.RightTorsoRear:
        return this.vfxRightTorsoTransform.position;
        default:
        Debug.LogError((object)("GetHitPosition had an invalid armor location " + (object)armorLocation));
        return this.parentCombatant.CurrentPosition;
      }
    }
    public override Vector3 GetMissPosition(Vector3 attackOrigin, Weapon weapon, NetworkRandom random) {
      Vector3 position = this.vfxCenterTorsoTransform.position;
      position.y = this.vfxCenterTorsoTransform.position.y;
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

    public override Transform GetVFXTransform(int location) {
      Transform centerTorsoTransform = this.vfxCenterTorsoTransform;
      Transform transform;
      switch ((ChassisLocations)location) {
        case ChassisLocations.Head:
        transform = this.vfxHeadTransform;
        break;
        case ChassisLocations.LeftArm:
        transform = this.vfxLeftArmTransform;
        break;
        case ChassisLocations.LeftTorso:
        transform = this.vfxLeftTorsoTransform;
        break;
        case ChassisLocations.CenterTorso:
        transform = this.vfxCenterTorsoTransform;
        break;
        case ChassisLocations.RightTorso:
        transform = this.vfxRightTorsoTransform;
        break;
        case ChassisLocations.RightArm:
        transform = this.vfxRightArmTransform;
        break;
        case ChassisLocations.LeftLeg:
        transform = this.vfxLeftLegTransform;
        break;
        case ChassisLocations.RightLeg:
        transform = this.vfxRightLegTransform;
        break;
        default:
        transform = this.vfxCenterTorsoTransform;
        break;
      }
      return transform;
    }
    public override void PlayVFX(int location, string vfxName, bool attached, Vector3 lookAtPos, bool oneShot, float duration) {
      if (string.IsNullOrEmpty(vfxName)) { return; }
      if (this.rootParentRepresentation.BlipDisplayed) { return; }
      GameRepresentation_PlayVFX(location, vfxName, attached, lookAtPos, oneShot, duration);
      ChassisLocations chassisLocations = (ChassisLocations)location;
      if (chassisLocations <= ChassisLocations.None || chassisLocations >= ChassisLocations.All) {
        GameRepresentation.initLogger.LogWarning((object)("Location out of bounds for VFX " + vfxName + ", location: " + (object)location), (UnityEngine.Object)this);
      } else {
        this.PlayVFXAt(this.GetVFXTransform(location), Vector3.zero, vfxName, attached, lookAtPos, oneShot, duration);
      }
    }

    public override void PlayPersistentDamageVFX(int location) {
      if (this.persistentDmgList.Count <= 0) { return; }
      int index = UnityEngine.Random.Range(0, this.persistentDmgList.Count);
      string persistentDmg = this.persistentDmgList[index];
      this.persistentDmgList.RemoveAt(index);
      if (persistentDmg.Contains("Smoke")) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_fire_small_internal, this.audioObject);
      } else if (persistentDmg.Contains("Electrical")) {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_electrical_start, this.audioObject);
      } else {
        int num3 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_sparks_start, this.audioObject);
      }
      Transform parentTransform = this.GetVFXTransform(location);
      switch ((ChassisLocations)location) {
        case ChassisLocations.LeftArm:
        parentTransform = this.vfxLeftShoulderTransform;
        break;
        case ChassisLocations.RightArm:
        parentTransform = this.vfxRightShoulderTransform;
        break;
      }
      this.PlayVFXAt(parentTransform, Vector3.zero, persistentDmg, true, Vector3.zero, false, -1f);
    }

    public virtual void _PlayComponentCritVFX(int location) {
      if (this.persistentCritList.Count <= 0) { return; }
      int index = UnityEngine.Random.Range(0, this.persistentCritList.Count);
      string persistentCrit = this.persistentCritList[index];
      this.persistentCritList.RemoveAt(index);
      Transform parentTransform = this.GetVFXTransform(location);
      switch ((ChassisLocations)location) {
        case ChassisLocations.LeftArm:
        parentTransform = this.vfxLeftShoulderTransform;
        break;
        case ChassisLocations.RightArm:
        parentTransform = this.vfxRightShoulderTransform;
        break;
      }
      this.PlayVFXAt(parentTransform, Vector3.zero, persistentCrit, true, Vector3.zero, false, -1f);
    }

    public override void PlayComponentDestroyedVFX(int location, Vector3 attackDirection) {
      string vfxName;
      AudioEventList_mech eventEnumValue;
      switch (UnityEngine.Random.Range(0, 4)) {
        case 0:
        vfxName = (string)this.Constants.VFXNames.componentDestruction_A;
        eventEnumValue = AudioEventList_mech.mech_part_break_a;
        break;
        case 1:
        vfxName = (string)this.Constants.VFXNames.componentDestruction_B;
        eventEnumValue = AudioEventList_mech.mech_part_break_b;
        break;
        case 2:
        vfxName = (string)this.Constants.VFXNames.componentDestruction_C;
        eventEnumValue = AudioEventList_mech.mech_part_break_c;
        break;
        default:
        vfxName = (string)this.Constants.VFXNames.componentDestruction_D;
        eventEnumValue = AudioEventList_mech.mech_part_break_a;
        break;
      }
      this.PlayVFX(location, vfxName, false, Vector3.zero, true, -1f);
      if (isSlave == false) {
        int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(eventEnumValue, this.audioObject);
        if (this.parentMech.team.LocalPlayerControlsTeam)
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_crippled");
        else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
          AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_crippled");
      }
      this.CollapseLocation(location, attackDirection);
      this.needsToRefreshCombinedMesh = true;
    }

    public virtual void CollapseLocation(int location, Vector3 attackDirection, bool loading = false) {
      MechDestructibleObject destructibleObject = this.GetDestructibleObject(location);
      if ((UnityEngine.Object)destructibleObject != (UnityEngine.Object)null) {
        if (loading)
          destructibleObject.CollapseSwap(true);
        else
          destructibleObject.Collapse(attackDirection, this.Constants.ResolutionConstants.ComponentDestructionForceMultiplier);
      }
      switch ((ChassisLocations)location) {
        case ChassisLocations.LeftArm:
        case ChassisLocations.RightArm:
        for (int index = 0; index < this.weaponReps.Count; ++index) {
          WeaponRepresentation weaponRep = this.weaponReps[index];
          if ((UnityEngine.Object)weaponRep != (UnityEngine.Object)null && weaponRep.mountedLocation == location) {
            if (loading)
              weaponRep.OnPlayerVisibilityChanged(VisibilityLevel.None);
            else
              weaponRep.DestructibleCollapse();
          }
        }
        this.StopAllPersistentVFXAttachedToLocation(location);
        break;
      }
      if (this.customRep != null) { this.StopCustomParticlesInLocation((ChassisLocations)location); }
    }
    public virtual void PlayDeathVFXVehicle(DeathMethod deathMethod, int location) {
      Log.TWL(0, "MechRepresentation.PlayDeathVFXVehicle deathMethod:" + deathMethod);
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
      this.HeightController.PendingHeight = 0f;
      this.customRep?.OnUnitDestroy();
      this.PlayVFX(location, vfxName, false, Vector3.zero, true, -1f);
      if (this.parentActor.Combat.IsLoadingFromSave) { return; }
      if (isSlave == false) {
        int num = (int)WwiseManager.PostEvent<AudioEventList_vehicle>(eventEnumValue, this.audioObject);
        Log.WL(1, "WwiseManager.PostEvent:" + eventEnumValue + " result:" + num);
      }
    }

    public override void PlayDeathVFX(DeathMethod deathMethod, int location) {
      if (deathMethod == DeathMethod.PilotKilled) { return; }
      if (this.parentMech.FakeVehicle()) {
        this.PlayDeathVFXVehicle(deathMethod, location);
        return;
      }
      string deathCenterTorsoA = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
      AudioEventList_mech eventEnumValue = AudioEventList_mech.mech_cockpit_explosion;
      bool attached = false;
      string vfxName;
      switch (deathMethod - 1) {
        case DeathMethod.NOT_SET:
        case DeathMethod.PilotKilled:
        vfxName = (string)this.Constants.VFXNames.mechDeath_cockpit;
        attached = true;
        break;
        case DeathMethod.HeadDestruction:
        case DeathMethod.Unknown:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
          eventEnumValue = AudioEventList_mech.mech_destruction_a;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_B;
          eventEnumValue = AudioEventList_mech.mech_destruction_b;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_C;
          eventEnumValue = AudioEventList_mech.mech_destruction_c;
          break;
        }
        break;
        case DeathMethod.CenterTorsoDestruction:
        vfxName = (string)this.Constants.VFXNames.mechDeath_legs;
        break;
        case DeathMethod.LegDestruction:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_A;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_B;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_ammoExplosion_C;
          break;
        }
        break;
        case DeathMethod.AmmoExplosion:
        vfxName = (string)this.Constants.VFXNames.mechDeath_engine;
        break;
        case DeathMethod.EngineDestroyed:
        vfxName = (string)this.Constants.VFXNames.mechDeath_gyro;
        break;
        case DeathMethod.CockpitDestroyed:
        case DeathMethod.PilotEjectionNoMessage:
        vfxName = (string)this.Constants.VFXNames.mechDeath_vitalComponent;
        break;
        default:
        switch (UnityEngine.Random.Range(0, 3)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_A;
          eventEnumValue = AudioEventList_mech.mech_destruction_a;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_B;
          eventEnumValue = AudioEventList_mech.mech_destruction_b;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.mechDeath_centerTorso_C;
          eventEnumValue = AudioEventList_mech.mech_destruction_c;
          break;
        }
        break;
      }
      this.PlayVFX(location, vfxName, attached, Vector3.zero, true, -1f);
      this.HeightController.PendingHeight = 0f;
      this.customRep?.OnUnitDestroy();
      if (this.parentMech.Combat.IsLoadingFromSave) { return; }
      if (isSlave == false) {
        int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(eventEnumValue, this.audioObject);
      }
    }
    public override MechDestructibleObject GetDestructibleObject(int location) {
      MechDestructibleObject destructibleObject = (MechDestructibleObject)null;
      switch ((ChassisLocations)location) {
        case ChassisLocations.Head:
        destructibleObject = this.headDestructible;
        break;
        case ChassisLocations.LeftArm:
        destructibleObject = this.leftArmDestructible;
        break;
        case ChassisLocations.LeftTorso:
        destructibleObject = this.leftTorsoDestructible;
        break;
        case ChassisLocations.CenterTorso:
        destructibleObject = this.centerTorsoDestructible;
        break;
        case ChassisLocations.RightTorso:
        destructibleObject = this.rightTorsoDestructible;
        break;
        case ChassisLocations.RightArm:
        destructibleObject = this.rightArmDestructible;
        break;
        case ChassisLocations.LeftLeg:
        destructibleObject = this.leftLegDestructible;
        break;
        case ChassisLocations.RightLeg:
        destructibleObject = this.rightLegDestructible;
        break;
        default:
        Debug.LogError((object)("Location out of bounds for GetDestructibleObject " + (object)location), (UnityEngine.Object)this);
        break;
      }
      return destructibleObject;
    }
    public virtual void _PlayDeathFloatie(DeathMethod deathMethod) {
      if (isSlave) { return; }
      string text = Localize.Strings.T("DESTROYED");
      switch (deathMethod) {
        case DeathMethod.HeadDestruction:
        text = Localize.Strings.T("HEAD DESTROYED");
        break;
        case DeathMethod.CenterTorsoDestruction:
        text = Localize.Strings.T("CT DESTROYED");
        break;
        case DeathMethod.LegDestruction:
        text = Localize.Strings.T("BOTH LEGS DESTROYED");
        break;
        case DeathMethod.AmmoExplosion:
        text = Localize.Strings.T("AMMO EXPLOSION");
        break;
        case DeathMethod.EngineDestroyed:
        text = Localize.Strings.T("ENGINE DESTROYED");
        break;
        case DeathMethod.GyroDestroyed:
        text = Localize.Strings.T("GYRO DESTROYED");
        break;
        case DeathMethod.PilotKilled:
        return;
        case DeathMethod.CockpitDestroyed:
        text = Localize.Strings.T("COCKPIT DESTROYED");
        break;
        case DeathMethod.VitalComponentDestroyed:
        text = Localize.Strings.T("VITAL COMPONENT DESTROYED");
        break;
        case DeathMethod.VehicleLocationDestroyed:
        text = Localize.Strings.T("VEHICLE DESTROYED");
        break;
        case DeathMethod.PilotEjection:
        text = Localize.Strings.T("PILOT EJECTED");
        break;
        case DeathMethod.PilotEjectionActorDisabled:
        text = Localize.Strings.T("MECH DISABLED");
        break;
        case DeathMethod.DespawnedNoMessage:
        return;
        case DeathMethod.DespawnedEscaped:
        text = Localize.Strings.T("EXTRACTED");
        break;
        case DeathMethod.PilotEjectionNoMessage:
        return;
        case DeathMethod.ComponentExplosion:
        text = Localize.Strings.T("COMPONENT EXPLOSION");
        break;
      }
      this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.parentCombatant.GUID, this.parentCombatant.GUID, text, FloatieMessage.MessageNature.CriticalHit));
    }
    public virtual void _UpdateLegDamageAnimFlags(LocationDamageLevel leftLegDamage, LocationDamageLevel rightLegDamage) {
      float num = 0.0f;
      if (leftLegDamage == LocationDamageLevel.Destroyed)
        num = -1f;
      else if (rightLegDamage == LocationDamageLevel.Destroyed)
        num = 1f;
      this.thisAnimator.SetFloat("DamagedLeg", num);
    }
    public virtual void _SetMeleeIdleState(bool isMelee) {
      this.thisAnimator.SetFloat("MeleeEngaged", isMelee ? 1f : 0.0f);
    }
    public virtual void _TriggerMeleeTransition(bool meleeIn) {
      if (meleeIn)
        this.thisAnimator.SetTrigger("MeleeIdleIn");
      else
        this.thisAnimator.SetTrigger("MeleeIdleOut");
    }
    public virtual void _ToggleRandomIdles(bool shouldIdle) {
      if (this.parentMech.IsOrWillBeProne || !this.parentMech.IsOperational || this.parentMech.IsDead) { return; }
      CustomTwistAnimation custIdleAnim = this.gameObject.GetComponent<CustomTwistAnimation>();
      if (custIdleAnim != null) {
        custIdleAnim.allowRandomIdles = shouldIdle;
        custIdleAnim.PlayIdleAnimation();
        this._allowRandomIdles = false;
        shouldIdle = false;
      }
      this._allowRandomIdles = shouldIdle;
      Log.TWL(0, "CustomMechRepresentation._ToggleRandomIdles " + shouldIdle + " " + this.parentMech.MechDef.ChassisID);
      if (this._allowRandomIdles) { return; }
      if (this.IsInMeleeIdle) {
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateMeleeEntryHash, 0.15f);
      } else {
        if (this.previousAnimState.fullPathHash != this.idleStateEntryHash && this.previousAnimState.fullPathHash != this.idleStateFlavorsHash && this.previousAnimState.fullPathHash != this.idleStateUnsteadyHash)
          return;
        this.thisAnimator.CrossFadeInFixedTime(this.idleStateEntryHash, 0.15f);
      }
    }
    public virtual void SetRandomIdleValue(float value) {
      //CustomTwistAnimation custIdleAnim = this.gameObject.GetComponent<CustomTwistAnimation>();
      //Log.TWL(0, "CustomMechRepresentation.SetRandomIdleValue: " + this.gameObject.name + " " + (this.parentMech == null ? "null" : this.parentMech.MechDef.ChassisID) + " value:" + value + " RotateBody:" + this.RotateBody);
      if (this.RotateBody) {
        //Log.WL(1, "this.thisAnimator.SetFloat(this.idleRandomValueHash, 0.6f)");
        this.thisAnimator.SetFloat(this.idleRandomValueHash, 0.6f);
        return;
      }
      if (this.customRep != null) {
        if (this.customRep.HasTwistAnimators) {
          this.customRep.IdleTwist(value);
          if (this.customRep.CustomDefinition.KeepRandomIdleAnimation) {
            //Log.WL(1, "this.thisAnimator.SetFloat(this.idleRandomValueHash, "+ value + ")");
            this.thisAnimator.SetFloat(this.idleRandomValueHash, value);
          } else {
            //Log.WL(1, "this.thisAnimator.SetFloat(this.idleRandomValueHash, 0.6f)");
            this.thisAnimator.SetFloat(this.idleRandomValueHash, 0.6f);
          }
          return;
        }
      }
      //Log.WL(1, "this.thisAnimator.SetFloat(this.idleRandomValueHash, " + value + ")");
      this.thisAnimator.SetFloat(this.idleRandomValueHash, value);
    }
    public virtual void _SetIdleAnimState() {
      if (this.parentCombatant != null) if (this.parentCombatant.NoRandomIdles()) { this._allowRandomIdles = false; }
      float value = 0.6f;
      if (this._allowRandomIdles == false) {
        value = 0.6f;
      } else {
        switch (UnityEngine.Random.Range(0, 7)) {
          case 0:
          value = 0.7f;
          break;
          case 1:
          value = 0.8f;
          break;
          case 2:
          value = 0.9f;
          break;
          default:
          value = 0.6f;
          break;
        }
      }
      SetRandomIdleValue(value);
    }
    public override void PlayFireAnim(AttackSourceLimb sourceLimb, int recoilStrength) {
      GameRepresentation_PlayFireAnim(sourceLimb, recoilStrength);
      this.thisAnimator.SetInteger("WeaponAttach", (int)sourceLimb);
      this.thisAnimator.SetFloat("RecoilWeightF", (float)recoilStrength);
      this.thisAnimator.SetTrigger("WeaponFire");
      this.thisAnimator.SetTrigger("WeaponFirePelvis");
    }
    public virtual void PlayMeleeAnim(int meleeHeight, ICombatant target) {
      GameRepresentation_PlayMeleeAnim(meleeHeight);
      if(meleeHeight == 1) {
        if(target.FlyingHeight() >= Core.Settings.MaxHoveringHeightWithWorkingJets) {
          meleeHeight = 3;
        }
      }
      this._SetMeleeIdleState(true);
      if (meleeHeight == 9)
        this.PlayJumpLandAnim(true);
      else if (meleeHeight == 8) {
        this.thisAnimator.SetTrigger("Charge");
      } else {
        this.thisAnimator.SetInteger("MeleeHeight", meleeHeight);
        this.thisAnimator.SetTrigger("Melee");
      }
    }
    public override void PlayMeleeAnim(int meleeHeight) {
      //return;
      GameRepresentation_PlayMeleeAnim(meleeHeight);
      this._SetMeleeIdleState(true);
      if (meleeHeight == 9)
        this.PlayJumpLandAnim(true);
      else if (meleeHeight == 8) {
        this.thisAnimator.SetTrigger("Charge");
      } else {
        this.thisAnimator.SetInteger("MeleeHeight", meleeHeight);
        this.thisAnimator.SetTrigger("Melee");
      }
    }
    public override void PlayImpactAnim(WeaponHitInfo hitInfo, int hitIndex, Weapon weapon, MeleeAttackType meleeType, float cumulativeDamage) {
      if (this.parentMech.IsOrWillBeProne || this.parentMech.IsDead) { return; }
      int fullPathHash = this.previousAnimState.fullPathHash;
      if (fullPathHash == this.hitReactHeavyHash || fullPathHash == this.hitReactMeleeHash || (fullPathHash == this.hitReactDodgeHash || fullPathHash == this.hitReactDFAHash))
        return;
      GameRepresentation_PlayImpactAnim(hitInfo, hitIndex, weapon, meleeType, cumulativeDamage);
      ArmorLocation armorLocation = (ArmorLocation)hitInfo.ShotHitLocation(hitIndex);
      Vector2 cardinalVector = this.parentActor.Combat.HitLocation.ConvertAttackDirectionToCardinalVector(hitInfo.attackDirections[hitIndex]);
      this._ResetHitReactFlags();
      this.thisAnimator.ResetTrigger("Stand");
      if (meleeType != MeleeAttackType.NotSet || (double)this.thisAnimator.GetFloat("MeleeEngaged") > 0.0 || hitInfo.dodgeSuccesses[hitIndex]) {
        if (armorLocation == ArmorLocation.None || armorLocation == ArmorLocation.Invalid) {
          int num1 = UnityEngine.Random.Range(0, 2) * 2;
          float num2;
          switch (hitInfo.attackDirections[hitIndex]) {
            case AttackDirection.FromFront:
            num2 = (float)num1;
            break;
            case AttackDirection.FromLeft:
            num2 = 0.0f;
            break;
            case AttackDirection.FromRight:
            num2 = 2f;
            break;
            case AttackDirection.FromBack:
            num2 = 1f;
            break;
            default:
            num2 = (float)num1;
            break;
          }
          this.thisAnimator.SetFloat("Dodge_Direction", num2);
          this.thisAnimator.SetBool("Hit_Dodge", true);
        } else if (meleeType == MeleeAttackType.DFA) {
          Mech parent = weapon.parent as Mech;
          this.thisAnimator.SetFloat("DFA_Power", (double)parent.tonnage <= 70.0 ? ((double)parent.tonnage <= 40.0 ? 0.3f : 0.6f) : 1f);
          this.thisAnimator.SetBool("Hit_DFA", true);
        } else {
          this.thisAnimator.SetFloat("HitDirection_X", cardinalVector.x);
          this.thisAnimator.SetFloat("HitDirection_Y", cardinalVector.y);
          this.thisAnimator.SetBool("Hit_Melee", true);
        }
        this.thisAnimator.SetTrigger("Hit_React");
        if (hitInfo.dodgeSuccesses[hitIndex])
          return;
        this._SetMeleeIdleState(true);
      } else {
        bool flag = (double)this.parentMech.StabilityPercentage > 0.5 || (double)cumulativeDamage >= (double)this.Constants.ResolutionConstants.HeavyHitReactDamageThreshold;
        if (weapon != null && weapon.WeaponCategoryValue.ForceLightHitReact)
          flag = false;
        if (this.parentMech.IsShutDown) {
          flag = false;
          this.thisAnimator.SetBool("Shutdown_Hit", true);
        }
        this.thisAnimator.SetFloat("HitDirection_X", cardinalVector.x);
        this.thisAnimator.SetFloat("HitDirection_Y", cardinalVector.y);
        this.thisAnimator.SetBool("Hit_Stagger", flag);
        this.thisAnimator.SetBool("Hit_Light", !flag);
        this.thisAnimator.SetTrigger("Hit_React");
      }
    }
    public virtual void _PlayImpactAnimSimple(AttackDirection attackDirection, float totalDamage) {
      if (this.parentMech.IsOrWillBeProne || this.parentMech.IsDead)
        return;
      int fullPathHash = this.previousAnimState.fullPathHash;
      if (fullPathHash == this.hitReactHeavyHash || fullPathHash == this.hitReactMeleeHash || (fullPathHash == this.hitReactDodgeHash || fullPathHash == this.hitReactDFAHash))
        return;
      this._ResetHitReactFlags();
      this.thisAnimator.ResetTrigger("Stand");
      Vector2 cardinalVector = this.parentActor.Combat.HitLocation.ConvertAttackDirectionToCardinalVector(attackDirection);
      bool flag = (double)this.parentMech.StabilityPercentage > 0.5 || (double)totalDamage >= (double)this.Constants.ResolutionConstants.HeavyHitReactDamageThreshold;
      if (this.parentMech.IsShutDown) {
        flag = false;
        this.thisAnimator.SetBool("Shutdown_Hit", true);
      }
      this.thisAnimator.SetFloat("HitDirection_X", cardinalVector.x);
      this.thisAnimator.SetFloat("HitDirection_Y", cardinalVector.y);
      this.thisAnimator.SetBool("Hit_Stagger", flag);
      this.thisAnimator.SetBool("Hit_Light", !flag);
      this.thisAnimator.SetTrigger("Hit_React");
    }
    public virtual void _SetUnsteadyAnim(bool isUnsteady) {
      this.thisAnimator.SetBool("Unsteady", isUnsteady);
      if (!isUnsteady)
        return;
      this.thisAnimator.SetTrigger("TriggerUnsteady");
    }
    public override void PlayKnockdownAnim(Vector2 attackDirection) {
      if (this.parentMech.IsProne || this.parentMech.IsDead) {
        Log.TWL(0, string.Format("{0} PlayKnockdownAnim failed: IsProne: {1}, IsBecomingProne: {2}, IsDead {3}", (object)this.parentMech.DisplayName, (object)this.parentMech.IsProne, (object)this.parentMech.IsBecomingProne, (object)this.parentMech.IsDead));
      } else {
        Log.TWL(0, "CustomMechRepresentation.PlayKnockdownAnim " + this.gameObject.name);
        this._SetMeleeIdleState(false);
        GameRepresentation_PlayKnockdownAnim(attackDirection);
        this._ResetHitReactFlags();
        this.thisAnimator.SetBool("KnockedDown", true);
        this.thisAnimator.SetTrigger("Hit_Knockdown");
        this.thisAnimator.ResetTrigger("Stand");
        this.thisAnimator.SetFloat("HitDirection_X", attackDirection.x);
        this.thisAnimator.SetFloat("HitDirection_Y", attackDirection.y);
        this.thisAnimator.SetTrigger("Hit_React");
        if ((double)this.thisAnimator.speed < 0.990000009536743) {
          Log.TWL(0, "Animator speed is less than 0.99f - resetting to full speed!");
          this.thisAnimator.speed = 1f;
        }
        if (isSlave == false) AudioEventManager.PlayPilotVO(VOEvents.Mech_Instability_Fall, (AbstractActor)this.parentMech);
        if (this.parentMech.team.LocalPlayerControlsTeam) {
          if (isSlave == false) AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_falling");
        } else {
          if (this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
            return;
          if (isSlave == false) AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_falling");
        }
        if (this.HasOwnVisuals) {
          Log.WL(1, "before to terrain: " + this.j_Root.eulerAngles);
          this.AliginToTerrain(9999f);
          Log.WL(1, "after to terrain: " + this.j_Root.eulerAngles);
        }
      }
    }
    public virtual void _ForceKnockdown(Vector2 attackDirection) {
      GameRepresentation_PlayKnockdownAnim(attackDirection);
      this._ResetHitReactFlags();
      this.thisAnimator.SetBool("KnockedDown", true);
      this.thisAnimator.SetTrigger("Hit_Knockdown");
      this.thisAnimator.ResetTrigger("Stand");
      this.thisAnimator.SetFloat("HitDirection_X", attackDirection.x);
      this.thisAnimator.SetFloat("HitDirection_Y", attackDirection.y);
      this.thisAnimator.SetTrigger("Hit_React");
      if ((double)this.thisAnimator.speed < 0.990000009536743) {
        Log.TWL(0, "Animator speed is less than 0.99f - resetting to full speed!");
        this.thisAnimator.speed = 1f;
      }
      this._SetMeleeIdleState(false);
      if (this.HasOwnVisuals) {
        Log.WL(1, "before to terrain: " + this.j_Root.eulerAngles);
        this.AliginToTerrain(9999f);
        Log.WL(1, "after to terrain: " + this.j_Root.eulerAngles);
      }
    }
    public virtual void _ResetHitReactFlags() {
      this.thisAnimator.SetBool("Hit_Melee", false);
      this.thisAnimator.SetBool("Hit_DFA", false);
      this.thisAnimator.SetBool("Hit_Light", false);
      this.thisAnimator.SetBool("Hit_Stagger", false);
      this.thisAnimator.SetBool("Hit_Dodge", false);
      this.thisAnimator.SetBool("Hit_Knockback", false);
      this.thisAnimator.SetBool("Shutdown_Hit", false);
      //foreach (CustomMechRepresentation slave in slaveRepresentations) { slave._ResetHitReactFlags(); }
      //if (!MechRepresentation.logger.IsWarningEnabled)
      //  return;
      //if (this.thisAnimator.GetBool("Hit_Melee"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_Melee flag to false, but it is not false now!");
      //if (this.thisAnimator.GetBool("Hit_DFA"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_DFA flag to false, but it is not false now!");
      //if (this.thisAnimator.GetBool("Hit_Light"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_Light flag to false, but it is not false now!");
      //if (this.thisAnimator.GetBool("Hit_Stagger"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_Stagger flag to false, but it is not false now!");
      //if (this.thisAnimator.GetBool("Hit_Dodge"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_Dodge flag to false, but it is not false now!");
      //if (this.thisAnimator.GetBool("Hit_Knockback"))
      //  MechRepresentation.logger.LogError((object)"Set Hit_Knockback flag to false, but it is not false now!");
      //if (!this.thisAnimator.GetBool("Shutdown_Hit"))
      //  return;
      //MechRepresentation.logger.LogError((object)"Set Shutdown_Hit flag to false, but it is not false now!");
    }

    public override void PlayStandAnim() {

      GameRepresentation_PlayStandAnim();
      this.thisAnimator.ResetTrigger("Stand");
      this.thisAnimator.SetTrigger("Stand");
      this.thisAnimator.SetBool("KnockedDown", false);
      this._SetMeleeIdleState(false);
      this._ResetHitReactFlags();
      if (!this.parentMech.team.LocalPlayerControlsTeam) { return; }
      if (isSlave == false) { AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_standup"); }
      if (this.HasOwnVisuals) {
        this.j_Root.localRotation = Quaternion.identity;
      }
    }

    public virtual void _StartJumpjetAudio() {
      if (!this.VisibleToPlayer || this.isPlayingJumpSound)
        return;
      this.isPlayingJumpSound = true;
      Log.TWL(0, "CustomMechRepresentation._StartJumpjetAudio "+this.parentMech.MechDef.ChassisID);
      if (this.parentMech.weightClass == WeightClass.HEAVY || this.parentMech.weightClass == WeightClass.ASSAULT) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_heavy_start, this.parentCombatant.GameRep.audioObject);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_light_start, this.parentCombatant.GameRep.audioObject);
      }
    }

    public virtual void _StopJumpjetAudio() {
      if (!this.isPlayingJumpSound)
        return;
      this.isPlayingJumpSound = false;
      Log.TWL(0, "CustomMechRepresentation._StopJumpjetAudio " + this.parentMech.MechDef.ChassisID);
      if (this.parentMech.weightClass == WeightClass.HEAVY || this.parentMech.weightClass == WeightClass.ASSAULT) {
        int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_heavy_stop, this.parentCombatant.GameRep.audioObject);
      } else {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpjets_light_stop, this.parentCombatant.GameRep.audioObject);
      }
    }

    public override void PlayJumpLaunchAnim() {
      GameRepresentation_PlayJumpLaunchAnim();
      this.thisAnimator.SetTrigger("Jump");
      this._SetMeleeIdleState(false);
      for (int index = 0; index < this.jumpjetReps.Count; ++index)
        this.jumpjetReps[index].SetState(JumpjetRepresentation.JumpjetState.Launching);
      this.isJumping = true;
      if (isSlave == false) this._StartJumpjetAudio();
      if (isSlave == false) this.PlayVFXAt((Transform)null, this.parentActor.CurrentPosition, (string)this.Constants.VFXNames.jumpjet_launch, false, Vector3.zero, true, -1f);
    }

    public override void PlayFallingAnim(Vector2 direction) {
      GameRepresentation_PlayFallingAnim(direction);
      this.thisAnimator.SetTrigger("Fall");
      this._SetMeleeIdleState(false);
      this.UpdateJumpAirAnim(direction.x, direction.y);
    }

    public override void UpdateJumpAirAnim(float forward, float side) {
      GameRepresentation_UpdateJumpAirAnim(forward, side);
      this.thisAnimator.SetFloat("InAir_Forward", forward);
      this.thisAnimator.SetFloat("InAir_Side", side);
      for (int index = 0; index < this.jumpjetReps.Count; ++index)
        this.jumpjetReps[index].SetState(JumpjetRepresentation.JumpjetState.Flight);
    }

    public override void PlayJumpLandAnim(bool isDFA) {
      GameRepresentation_PlayJumpLandAnim(isDFA);
      this.isJumping = false;
      for (int index = 0; index < this.jumpjetReps.Count; ++index) {
        this.jumpjetReps[index].SetState(JumpjetRepresentation.JumpjetState.Landing);
      }
      if (isDFA) {
        this.thisAnimator.SetTrigger("DFA");
        this._SetMeleeIdleState(true);
      } else {
        this.thisAnimator.SetTrigger("Land");
      }
      if (isSlave == false) this._StopJumpjetEffect();
    }

    public virtual void _StartJumpjetEffect() {
      if (isSlave == false) this._StartJumpjetAudio();
      for (int index = 0; index < this.jumpjetReps.Count; ++index)
        this.jumpjetReps[index].SetState(JumpjetRepresentation.JumpjetState.Launching);
    }

    public virtual void _StopJumpjetEffect() {
      if (isSlave == false) this._StopJumpjetAudio();
      for (int index = 0; index < this.jumpjetReps.Count; ++index)
        this.jumpjetReps[index].SetState(JumpjetRepresentation.JumpjetState.Landed);
    }
    public static readonly string SeenByPlayer = "CUSeenByPlayer";
    public override void OnPlayerVisibilityChanged(VisibilityLevel newLevel) {
      if (DeployManualHelper.IsInManualSpawnSequence) { newLevel = VisibilityLevel.None; }
      if (this.isSlave == false) {
        Statistic FirstSeenByPlayer_stat = this.parentCombatant.StatCollection.GetStatistic(SeenByPlayer);
        if (FirstSeenByPlayer_stat == null) { FirstSeenByPlayer_stat = this.parentCombatant.StatCollection.AddStatistic(SeenByPlayer, false); }
        if ((newLevel >= VisibilityLevel.Blip1Type) && (FirstSeenByPlayer_stat.Value<bool>() == false)) {
          FirstSeenByPlayer_stat.SetValue<bool>(true);
          if (this.parentActor.GetCustomInfo().BossAppearAnimation) {
            Log.TWL(0, "BossAppearAnimation:"+this.parentActor.PilotableActorDef.ChassisID);
            //Vector3 currentPosition = this.parentActor.CurrentPosition;
            //float cameraDistance1 = this.GetCameraDistance(DialogCameraDistance.Far);
            //float cameraHeight1 = this.GetCameraHeight(DialogCameraHeight.High, cameraDistance1);
            //EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new AddParallelSequenceToStackMessage((IStackSequence)CameraControl.Instance.ShowFocalCamAtHeight(currentPosition, cameraHeight1, cameraDistance1, 2f)));
            //this.RevealRadius(this.mech.Radius * 2f);
            return;
          }
        }
      }
      this.OnPlayerVisibilityChangedCustom(newLevel);
    }
    public virtual void OnPlayerVisibilityChangedCustom(VisibilityLevel newLevel) {
      try {
        PilotableActorRepresentation_OnPlayerVisibilityChanged(newLevel);
        for (int index = 0; index < this.jumpjetReps.Count; ++index)
          this.jumpjetReps[index].OnPlayerVisibilityChanged(newLevel);
        if (this.isJumping) {
          if (newLevel == VisibilityLevel.LOSFull)
            if (isSlave == false) this._StartJumpjetAudio();
            else
            if (isSlave == false) this._StopJumpjetAudio();
        }
        this.ToggleHeadlights(newLevel == VisibilityLevel.LOSFull);
        if (this.customRep != null) { this.ShowCustomParticles(newLevel); }
        if (this.HeightController != null) { this.HeightController.OnVisibilityChange(newLevel); }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public virtual void _ToggleHeadlights(bool headlightsActive) {
      if (this.IsDead)
        headlightsActive = false;
      for (int index = 0; index < this.headlightReps.Count; ++index)
        this.headlightReps[index].SetActive(headlightsActive);
    }
    public override void PlayShutdownAnim() {
      GameRepresentation_PlayShutdownAnim();
      if (isSlave == false) { int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerdown, this.audioObject); }
      if (!this.parentMech.IsOrWillBeProne && !this.IsDead) {
        this._SetMeleeIdleState(false);
        this._TriggerMeleeTransition(false);
        this.thisAnimator.SetTrigger("PowerOff");
      }
      this._ToggleHeadlights(false);
      this.HeightController.PendingHeight = 0f;
      this.parentCombatant.FlyingHeight(0f);
      this.custMech.UpdateLOSHeight(0f);
      this.customRep.InBattle = false;
      this.StopPersistentAudio();
    }
    public override void PlayStartupAnim() {
      GameRepresentation_PlayStartupAnim();
      if (isSlave == false) { int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerup, this.audioObject); }
      this._ToggleHeadlights(this.VisibleObject.activeInHierarchy);
      if (isSlave == false) {
        if (this.parentMech.team.LocalPlayerControlsTeam)
          AudioEventManager.PlayPilotVO(VOEvents.Mech_Power_Restart, (AbstractActor)this.parentMech);
        else
          AudioEventManager.PlayComputerVO(ComputerVOEvents.Mech_Powerup_Enemy);
      }
      this.thisAnimator.SetTrigger("PowerOn");
      Log.TWL(0, "PlayStartupAnim:"+this.parentCombatant.PilotableActorDef.ChassisID);
      this.parentCombatant.FlyingHeight(this.altDef.FlyHeight);
      this.HeightController.PendingHeight = this.parentCombatant.FlyingHeight();
      this.custMech.UpdateLOSHeight(this.parentCombatant.FlyingHeight());
      this.customRep.InBattle = true;
      this.StartPersistentAudio();
    }
    public override void HandleDeath(DeathMethod deathMethod, int location) {
      PilotableRepresentation_HandleDeath(deathMethod, location);
      if (this.customRep != null) { this.StopCustomParticles(); }
      if (isSlave == false) this._PlayDeathFloatie(deathMethod);
      if (this.parentActor.WasDespawned) { return; }
      if (this.VisibleObjectLight != null) { this.VisibleObjectLight.SetActive(false); }
      this.thisAnimator.SetTrigger("Death");
      if (!this.parentMech.Combat.IsLoadingFromSave) {
        if (isSlave == false) {
          if (this.parentMech.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_mech_destroyed");
          else if (!this.parentMech.team.IsFriendly(this.parentMech.Combat.LocalPlayerTeam))
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_mech_destroyed");
        }
      }
      if (this.parentMech.IsOrWillBeProne || this.parentActor.WasEjected) { this.StartCoroutine(this.DelayProneOnDeath()); }
      if (!this.parentActor.WasEjected) { this.PlayDeathVFX(deathMethod, location); }
      this.HeightController.PendingHeight = 0f;
      if (this.customRep != null) { this.customRep.InBattle = false; }
      List<string> stringList = new List<string>((IEnumerable<string>)this.persistentVFXParticles.Keys);
      for (int index = stringList.Count - 1; index >= 0; --index) { this.StopManualPersistentVFX(stringList[index]); }
      this.__IsDead = true;
      if (deathMethod != DeathMethod.PilotKilled && !this.parentActor.WasEjected) {
        string vfxName;
        switch (UnityEngine.Random.Range(0, 4)) {
          case 0:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_A;
          break;
          case 1:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_B;
          break;
          case 2:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_C;
          break;
          default:
          vfxName = (string)this.Constants.VFXNames.deadMechLoop_D;
          break;
        }
        this.PlayVFX(8, vfxName, true, Vector3.zero, false, -1f);
        float num = UnityEngine.Random.Range(25f, 30f);
        FootstepManager.Instance.AddScorch(this.transform.position, new Vector3(UnityEngine.Random.Range(0.0f, 1f), 0.0f, UnityEngine.Random.Range(0.0f, 1f)).normalized, new Vector3(num, num, num), true);
      }
      this._ToggleHeadlights(false);
    }
    public virtual void _HandleDeathOnLoad(DeathMethod deathMethod, int location) {
      this.gameObject.layer = LayerMask.NameToLayer("UI_IgnoreMouseEvents");
      this.thisAnimator.SetTrigger("LoadStateDead");
      if ((UnityEngine.Object)this.VisibleObjectLight != (UnityEngine.Object)null)
        this.VisibleObjectLight.SetActive(false);
      this.IsDead = true;
      this._ToggleHeadlights(false);
    }
    protected virtual IEnumerator DelayProneOnDeath() {
      yield return new WaitForSeconds(3f);
      this.OnDeath();
      yield break;
    }
    public virtual void _PlayAlliesReportDeathVO() {
      bool flag;
      if (isSlave == false) {
        if (this.parentActor.GetPilot() != null && this.parentActor.GetPilot().LethalInjuries) {
          flag = true;
          AudioEventManager.PlayRandomPilotVO(VOEvents.TakeDamage_PilotKilled, this.parentCombatant.team, this.parentCombatant.team.units);
        } else {
          flag = true;
          AudioEventManager.PlayRandomPilotVO(VOEvents.TakeDamage_AllyDestroyed, this.parentCombatant.team, this.parentCombatant.team.units);
        }
        if (!flag)
          return;
        foreach (AbstractActor unit in this.parentCombatant.team.units)
          AudioEventManager.SetPilotVOSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, unit);
      }
    }
    public override void OnFootFall(int leftFoot) {
      Log.TWL(0,"MechRepresentation.OnFootFall "+this.mech.PilotableActorDef.ChassisID+ " leftFoot:" + leftFoot);
      Log.WL(1, UnityEngine.StackTraceUtility.ExtractStackTrace());
      if (this.triggerFootVFX) { return; }
      this.triggerFootVFX = true;
      this.leftFootVFX = leftFoot;
    }
    public virtual void CustomFootFall(Transform foot) {
      try {
        if (this.rootParentRepresentation.BlipDisplayed) { return; }
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.VisibleObject.activeSelf) {
          TriggerCustomFootFall(foot);
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public virtual void TriggerCustomFootFall(Transform foot) {
      Vector3 position = foot.position;
      Vector3 currentPosition = this.parentMech.CurrentPosition;
      position = new Vector3(position.x, currentPosition.y + 0.2f, position.z);
      Vector3 forward = this.thisTransform.forward;
      Vector3 lookAtPos = position + forward * 100f;
      float num1;
      switch (this.parentMech.MechDef.Chassis.weightClass) {
        case WeightClass.LIGHT:
        num1 = 3f;
        break;
        case WeightClass.MEDIUM:
        num1 = 5f;
        break;
        case WeightClass.HEAVY:
        num1 = 7f;
        break;
        default:
        num1 = 7f;
        break;
      }
      FootstepManager.Instance.AddFootstep(position, forward, new Vector3(num1, num1, num1));
      string vfxName1 = string.Format("{0}{1}{2}{3}", (object)this.Constants.VFXNames.footfallBase, (object)(this.IsInAnyIdle ? "idle_" : ""), (object)this.rootParentRepresentation.terrainImpactParticleName, (object)this.rootParentRepresentation.vfxNameModifier);
      Log.TWL(0, "_TriggerFootFall " + this.gameObject.name + " " + vfxName1);
      this.PlayVFXAt(foot, Vector3.zero, vfxName1, false, lookAtPos, true, -1f);
      if (this.currentSurfaceType == AudioSwitch_surface_type.wood)
        this.PlayVFX(8, "vfxPrfPrtl_envTreeRustle_vHigh", false, Vector3.zero, true, -1f);
      WwiseManager.SetSwitch<AudioSwitch_mech_coil_fx_yesno>(AudioSwitch_mech_coil_fx_yesno.coil_fx_no, this.rootParentRepresentation.audioObject);
      ActorMovementSequence movementSequence = (ActorMovementSequence)null;
      if (this.parentActor != null && this.parentActor.Combat != null && this.parentActor.Combat.StackManager != null)
        movementSequence = this.parentActor.Combat.StackManager.GetMoveSequenceForActor(this.parentActor);
      if (movementSequence != null && !movementSequence.isSprinting && this.parentActor.Weapons.FindAll((Predicate<Weapon>)(x => x.weaponDef.Type == WeaponType.COIL && x.DamageLevel == ComponentDamageLevel.Functional)).Count > 0) {
        string vfxName2 = (double)movementSequence.distanceTravelled < (double)this.parentActor.Combat.Constants.CombatUIConstants.COIL_Footstep_Charge_Max ? ((double)movementSequence.distanceTravelled < (double)this.parentActor.Combat.Constants.CombatUIConstants.COIL_Footstep_Charge_Med ? "vfxPrfPrtl_weaponCOILstepCharge_1" : "vfxPrfPrtl_weaponCOILstepCharge_2") : "vfxPrfPrtl_weaponCOILstepCharge_3";
        this.PlayVFXAt(foot, Vector3.zero, vfxName2, true, lookAtPos, true, -1f);
        WwiseManager.SetSwitch<AudioSwitch_mech_coil_fx_yesno>(AudioSwitch_mech_coil_fx_yesno.coil_fx_yes, this.rootParentRepresentation.audioObject);
      }
      if (this.IsInAnyIdle) {
        int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_footstep_idle, this.rootParentRepresentation.audioObject);
      } else {
        CameraControl.Instance?.AddCameraShake(this.parentMech.tonnage * this.Constants.CombatUIConstants.ScreenShakeFootfallRelativeMod + this.Constants.CombatUIConstants.ScreenShakeFootfallAbsoluteMod, 1f, this.parentMech.CurrentPosition);
        int num3 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_footstep, this.rootParentRepresentation.audioObject);
      }

    }
    public virtual void _TriggerFootFall(int leftFoot) {
      try {
        if (this.rootParentRepresentation.BlipDisplayed) { return; }
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.VisibleObject.activeSelf) {
          Transform footTransform = leftFoot > 0 ? this.leftFootTransform : this.rightFootTransform;
          TriggerCustomFootFall(footTransform);
          if (this.parentMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed || this.parentMech.RightLegDamageLevel == LocationDamageLevel.Destroyed)
            this.IsOnLimpingLeg = leftFoot <= 0 ? this.parentMech.RightLegDamageLevel == LocationDamageLevel.Destroyed : this.parentMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed;
        }
        if (!this.triggerFootVFX)
          return;
        this.triggerFootVFX = false;
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
    public override void OnAudioEvent(string audioEvent) {
      GameRepresentation_OnAudioEvent(audioEvent);
      if (isSlave) { return; }
      if (this.BlipDisplayed || !this.VisibleObject.activeSelf || !this.parentActor.Combat.TurnDirector.GameHasBegun) { return; }
      if (string.IsNullOrEmpty(audioEvent)) {
        Log.TWL(0, "AnimationEvent OnAudioEvent got an invalid audioEvent name: null", true);
      } else {
        audioEvent = audioEvent.ToLowerInvariant();
        string[] strArray = audioEvent.Split('_');
        if (strArray.Length < 2) {
          Log.TWL(0, "AnimationEvent OnAudioEvent got an invalid audioEvent name: " + audioEvent, true);
        } else {
          //Log.TWL(0, "MechRepresentation.OnAudioEvent "+this.name + " " + audioEvent, true);
          int num = (int)WwiseManager.PostEvent(string.Format("AudioEventList_{0}_{1}", (object)strArray[0], (object)audioEvent), this.rootParentRepresentation.audioObject);
        }
      }
    }

    public override void OnVFXEvent(AnimationEvent animEvent) {
      string stringParameter = animEvent.stringParameter;
      this.PlayVFX(animEvent.intParameter, stringParameter, true, Vector3.zero, true, 0.0f);
    }

    public override void OnDeath() {
      Log.TWL(0, "MechRepresentation.OnDeath " + this.name);
      PilotableRepresentation_OnDeath();
      int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_engine_powerdown_death, this.rootParentRepresentation.audioObject);
      int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_electrical_stop, this.rootParentRepresentation.audioObject);
      int num3 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_sparks_stop, this.rootParentRepresentation.audioObject);
      int num4 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_fire_stop, this.rootParentRepresentation.audioObject);
    }

    public override void OnGroundImpact() {
      Log.TWL(0, "MechRepresentation.OnGroundImpact "+this.name);
      this.OnGroundImpact(false);
    }
    public virtual void OnGroundImpact(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      GameRepresentation_OnGroundImpact();
      if (this.rootParentRepresentation.BlipDisplayed || !this.VisibleObject.activeSelf) { return; }
      Vector3 currentPosition = this.j_Root.position;
      string str = "sm";
      if (this.parentMech.weightClass == WeightClass.ASSAULT || this.parentMech.weightClass == WeightClass.HEAVY || (this.CurrentSurfaceType == AudioSwitch_surface_type.water_deep || this.CurrentSurfaceType == AudioSwitch_surface_type.water_shallow)) { str = "lrg"; }
      string vfxName = string.Format("{0}{1}{2}_{3}", (object)this.Constants.VFXNames.groundImpactBase, (object)this.rootParentRepresentation.terrainImpactParticleName, (object)this.vfxNameModifier, (object)str);
      Vector3 lookAtPos = currentPosition + Vector3.up;
      this.PlayVFXAt((Transform)null, currentPosition, vfxName, false, lookAtPos, true, -1f);
      CameraControl.Instance.AddCameraShake(this.parentMech.tonnage * this.Constants.CombatUIConstants.ScreenShakeGroundImpactRelativeMod + this.Constants.CombatUIConstants.ScreenShakeGroundImpactAbsoluteMod, 1f, this.parentMech.CurrentPosition);
      int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_bodyfall_heavy, this.rootParentRepresentation.audioObject);
      int num2 = (int)WwiseManager.PostEvent<AudioEventList_torso>(AudioEventList_torso.torso_rotate_interrupted, this.rootParentRepresentation.audioObject);
    }

    public override void OnJumpLand() {
      this.OnJumpLand(false);
    }
    public virtual void OnJumpLand(bool forcedSlave) {
      if (isSlave && (forcedSlave == false)) { return; }
      GameRepresentation_OnJumpLand();
      if (this.rootParentRepresentation.BlipDisplayed || !this.VisibleObject.activeSelf) { return; }
      Vector3 currentPosition = this.j_Root.position;
      string str = "_sm";
      if (this.parentMech.weightClass == WeightClass.ASSAULT || this.parentMech.weightClass == WeightClass.HEAVY) { str = "_lrg"; }
      string vfxName = string.Format("{0}{1}{2}{3}", (object)this.Constants.VFXNames.jumpjetLandBase, (object)this.rootParentRepresentation.terrainImpactParticleName, (object)this.vfxNameModifier, (object)str);
      this.PlayVFXAt((Transform)null, currentPosition, vfxName, false, Vector3.zero, true, -1f);
      CameraControl.Instance.AddCameraShake(this.parentMech.tonnage * this.Constants.CombatUIConstants.ScreenShakeGroundImpactRelativeMod + this.Constants.CombatUIConstants.ScreenShakeGroundImpactAbsoluteMod, 1f, this.parentMech.CurrentPosition);
      int num = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_jumpland, this.rootParentRepresentation.audioObject);
    }

    public override void OnMeleeImpact(AnimationEvent animEvent) {
      GameRepresentation_OnMeleeImpact(animEvent);
      if (isSlave) { return; }
      foreach (AttackDirector.AttackSequence attackSequence in this.parentCombatant.Combat.AttackDirector.GetAllAttackSequencesWithAttacker(this.parentCombatant)) {
        if (attackSequence.isMelee) {
          this.parentCombatant.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceAnimationEventMessage(attackSequence.stackItemUID, attackSequence.id, animEvent.intParameter, animEvent.stringParameter));
          break;
        }
      }
    }
    public int ForwardHash { get; set; }
    public int TurnHash { get; set; }
    public int IsMovingHash { get; set; }
    public int BeginMovementHash { get; set; }
    public int DamageHash { get; set; }
    public bool HasForwardParam { get; set; }
    public bool HasTurnParam { get; set; }
    public bool HasIsMovingParam { get; set; }
    public bool HasBeginMovementParam { get; set; }
    public bool HasDamageParam { get; set; }
    public virtual float TurnParam {
      set {
        if (!this.HasTurnParam) { return; }
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HeightController.CurrentHeight <= Core.Settings.MaxHoveringHeightWithWorkingJets) {
          this.thisAnimator.SetFloat(this.TurnHash, value);
          if (this.customRep != null) { this.customRep.TurnParam = value; }
        }
      }
    }
    public virtual float ForwardParam {
      set {
        if (!this.HasForwardParam) { return; }
        if (this.parentMech.NoMoveAnimation()) { return; }
        if (this.HeightController.CurrentHeight <= Core.Settings.MaxHoveringHeightWithWorkingJets) {
          this.thisAnimator.SetFloat(this.ForwardHash, value);
        }
        if (this.customRep != null) { this.customRep.Forward = value; }
      }
    }
    public virtual bool IsMovingParam {
      set {
        if (!this.HasIsMovingParam) { return; }
        //if (this.parentMech.NoMoveAnimation()) { return; }
        this.thisAnimator.SetBool(this.IsMovingHash, value);
        if (this.customRep != null) { this.customRep.IsMoving = value; }
      }
    }
    public virtual bool BeginMovementParam {
      set {
        if (!this.HasBeginMovementParam) { return; };
        //if (this.parentMech.NoMoveAnimation()) { return; }
        this.thisAnimator.SetTrigger(this.BeginMovementHash);
        if (this.customRep != null) { this.customRep.BeginMove(); }
      }
    }
    public virtual float DamageParam {
      set {
        if (!this.HasDamageParam) { return; };
        //if (this.parentMech.NoMoveAnimation()) { return; }
        this.thisAnimator.SetFloat(this.DamageHash, value);
      }
    }
    public virtual void InitAnimations() {
      this.ForwardHash = Animator.StringToHash("Forward");
      this.TurnHash = Animator.StringToHash("Turn");
      this.IsMovingHash = Animator.StringToHash("IsMoving");
      this.BeginMovementHash = Animator.StringToHash("BeginMovement");
      this.DamageHash = Animator.StringToHash("Damage");
      AnimatorControllerParameter[] parameters = this.thisAnimator.parameters;
      for (int index = 0; index < parameters.Length; ++index) {
        if (parameters[index].nameHash == this.ForwardHash)
          this.HasForwardParam = true;
        if (parameters[index].nameHash == this.TurnHash)
          this.HasTurnParam = true;
        if (parameters[index].nameHash == this.IsMovingHash)
          this.HasIsMovingParam = true;
        if (parameters[index].nameHash == this.BeginMovementHash)
          this.HasBeginMovementParam = true;
        if (parameters[index].nameHash == this.DamageHash)
          this.HasDamageParam = true;
      }
    }
    public virtual bool lastStateWasVisible { get; set; }
    public virtual void PlayVehicleTerrainImpactVFX(bool forcedSlave = false) {
      bool vehicleStyleMove = this.parentCombatant.NoMoveAnimation() || this.parentCombatant.FakeVehicle();
      Log.TWL(0, "CustomMechRepresentation.PlayVehicleTerrainImpactVFX (NoMoveAnimation || FakeVehicle):" + vehicleStyleMove + " FlyingHeight:" + this.parentMech.FlyingHeight()+ " lastStateWasVisible:"+this.lastStateWasVisible);
      if ((this.rootParentRepresentation.BlipDisplayed)||(this.VisibleObject.activeInHierarchy == false)) {
        if (this.lastStateWasVisible == false) { return; }
        this.lastStateWasVisible = false;
        this.PlayMovementStopAudio(forcedSlave);
      } else {
        if (this.lastStateWasVisible == false) {
          this.lastStateWasVisible = true;
          this.PlayMovementStartAudio(forcedSlave);
        }
        if (vehicleStyleMove && (this.parentMech.FlyingHeight() < Core.Settings.MaxHoveringHeightWithWorkingJets)) {
          Vector3 lookAtPos = this.thisTransform.position + this.thisTransform.forward;
          string terrainImpactParticleName = this.rootParentRepresentation.terrainImpactParticleName;
          AudioSwitch_surface_type currentSurfaceType = this.rootParentRepresentation._CurrentSurfaceType;
          string vfxNameModifier = this.rootParentRepresentation.vfxNameModifier;
          this.PlayVFXAt(this.vfxRightArmTransform, Vector3.zero, string.Format("{0}{1}{2}", (object)this.parentActor.Combat.Constants.VFXNames.footfallBase, terrainImpactParticleName, vfxNameModifier), false, lookAtPos, true, -1f);
          if (currentSurfaceType == AudioSwitch_surface_type.wood) {
            this.PlayVFX(1, "vfxPrfPrtl_envTreeRustle_vHigh", false, Vector3.zero, true, -1f);
          }
        }
      }
    }
    public override ParticleSystem PlayVFXAt(Transform parentTransform,Vector3 offset,string vfxName,bool attached,Vector3 lookAtPos,bool oneShot,float duration) {
      return this.PilotableRepresentation_PlayVFXAt(parentTransform, offset, vfxName, attached, lookAtPos, oneShot, duration);
    }

    public virtual void updateVehicleMovementVFX(ActorMovementSequence moveSeq) {
      Traverse.Create(moveSeq).Field<float>("vehicleVFXTimer").Value += Traverse.Create(moveSeq).Property<float>("deltaTime").Value;
      if (Traverse.Create(moveSeq).Field<float>("vehicleVFXTimer").Value < 0.100000001490116) { return; }
      Traverse.Create(moveSeq).Field<float>("vehicleVFXTimer").Value = 0.0f;
      this.PlayVehicleTerrainImpactVFX(false);
    }
    public virtual void UpdateSpline(ActorMovementSequence sequence, Vector3 Forward, float t, ICombatant meleeTarget) {
      this.updateVehicleMovementVFX(sequence);
      if (meleeTarget != null) {
        if (this.HeightController != null) {
          float targetHeight = meleeTarget.FlyingHeight();
          float myHeight = this.parentCombatant.FlyingHeight();
          if (myHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            if (targetHeight <= Core.Settings.MaxHoveringHeightWithWorkingJets) { targetHeight = Core.Settings.MaxHoveringHeightWithWorkingJets + 0.1f; }
            this.HeightController.ForceHeight(myHeight + (targetHeight - myHeight) * t);
          }
        }
      }
    }
    public virtual void UpdateJumpFlying(MechJumpSequence sequence) {

    }
    public virtual void CompleteJump(MechJumpSequence sequence) {

    }
    public virtual void CompleteMove(ActorMovementSequence sequence, bool playedMelee, ICombatant meleeTarget) {
      this.lastStateWasVisible = (this.rootParentRepresentation.BlipDisplayed == false);
      if (this.lastStateWasVisible) { this.PlayMovementStopAudio(); }
      if (meleeTarget != null) {
        if (this.HeightController != null) {
          float targetHeight = meleeTarget.FlyingHeight();
          float myHeight = this.parentCombatant.FlyingHeight();
          if (myHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            if (targetHeight <= Core.Settings.MaxHoveringHeightWithWorkingJets) { targetHeight = Core.Settings.MaxHoveringHeightWithWorkingJets + 0.1f; }
            this.HeightController.ForceHeight(targetHeight);
          }
        }
      }
    }
    public virtual void BeginMove(ActorMovementSequence sequence) {
      this.IsMovingParam = true;
      this.BeginMovementParam = true;
      if (this.parentMech.LeftLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.DamageParam = -1f;
      } else if (this.parentMech.RightLegDamageLevel == LocationDamageLevel.Destroyed) {
        this.DamageParam = 1f;
      }
      this._SetMeleeIdleState(false);
      this.lastStateWasVisible = (this.rootParentRepresentation.BlipDisplayed == false);
      if (this.lastStateWasVisible) { this.PlayMovementStartAudio(); }
    }
  }
}