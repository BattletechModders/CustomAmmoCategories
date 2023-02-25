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
using BattleTech.Rendering.MechCustomization;
using BattleTech.UI;
using FogOfWar;
using HarmonyLib;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("_SendTurnActorActivateMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class TurnDirector__SendTurnActorActivateMessage {
    public static void Postfix(TurnDirector __instance, int turnActorIndex) {
      try {
        Team team = __instance.TurnActors[turnActorIndex] as Team;
        if(team == __instance.Combat.LocalPlayerTeam) {
          BossAppearManager.TestBossBeacons(__instance.Combat);
        }
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDInWorldScalingActorInfo))]
  [HarmonyPatch("GetWorldPos")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { } )]
  public static class CombatHUDInWorldScalingActorInfo_GetWorldPos {
    public static void Postfix(CombatHUDInWorldScalingActorInfo __instance, ref Vector3 __result) {
      try {
        if (__instance.DisplayedActor == null) { return; }
        if(__instance.anchorPosition == CombatHUDInWorldScalingActorInfo.AnchorPosition.Feet) { return; }
        __result += Vector3.up * __instance.DisplayedActor.FakeHeightDelta();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public static class FakeHeightController {
    private static Dictionary<AbstractActor, float> fakeHeightData = new Dictionary<AbstractActor, float>();
    public static void Clear() { fakeHeightData.Clear(); }
    public static float FakeHeightDelta(this AbstractActor unit) {
      if (fakeHeightData.TryGetValue(unit, out float result)) { return result; }
      return 0f;
    }
    public static void FakeHeightDelta(this AbstractActor unit, float height) {
      fakeHeightData[unit] = height;
    }
  }
  public class BossAppearBeacon : MonoBehaviour {
    public CustomMech parent { get; set; } = null;
    public bool triggered { get; set; } = false;
    public void Init(CustomMech parent) {
      this.parent = parent;
      this.triggered = false;
    }
    public void Reveal() {
      float revealRadius = this.parent.MechDef.Chassis.Radius * 2f;
      SnapToTerrain snapToTerrain = this.gameObject.AddComponent<SnapToTerrain>();
      snapToTerrain.verticalOffset = 10f;
      snapToTerrain.UpdatePosition();
      FogOfWarRevealatron revealatron = this.gameObject.AddComponent<FogOfWarRevealatron>();
      revealatron.GUID = this.parent.GUID + string.Format(".{0}", 0);
      revealatron.radiusMeters = revealRadius;
      LazySingletonBehavior<FogOfWarView>.Instance.FowSystem.AddRevealatronViewer(revealatron);
      List<AbstractActor> allActors = this.parent.Combat.AllActors;
      this.parent.ForcedVisible = true;
      this.parent.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
      this.parent.GameRep?.SetForcedPlayerVisibilityLevel(VisibilityLevel.LOSFull);
      foreach (var link in this.parent.linkedActors) {
        if (link.customMech != null) { link.customMech.ForcedVisible = true; }
        link.actor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
        if(link.actor.GameRep is PilotableActorRepresentation pilotableRep){
          pilotableRep.SetForcedPlayerVisibilityLevel(VisibilityLevel.LOSFull);
        }
      }
    }
  }
  public static class BossAppearManager {
    private static HashSet<BossAppearBeacon> bossBeacons = new HashSet<BossAppearBeacon>();
    private static GameObject landedEffect { get; set; } = null;
    private static GameObject dropOffDustEffect { get; set; } = null;
    private static GameObject dropOffEngineEffect { get; set; } = null;
    public static string landedEffectObjectName = "vfxPrfPrtl_dropshipLandingWash-MANAGED";
    public static string dropOffDustEffectName = "exhaust1_outward (2)";
    public static string dropOffEngineEffectName = "vfxPrfPrtl_dropshipHoverThrust_down";
    public static void TestBossBeacons(CombatGameState combat) {
      Log.TWL(0, "BossAppearManager.TestBossBeacons");
      foreach (BossAppearBeacon beacon in bossBeacons) {
        if (beacon.triggered) { continue; }
        bool beaconActivated = false;
        Log.WL(1, beacon.gameObject.name);
        foreach (AbstractActor unit in combat.LocalPlayerTeam.units) {
          if (unit.IsDead) { continue; }
          float distance = Vector3.Distance(beacon.transform.position, unit.CurrentPosition);
          Log.WL(2, unit.PilotableActorDef.ChassisID+" distance:"+distance);
          if (distance < 500.0f) {
            beaconActivated = true;
            break;
          }
        }
        if (beaconActivated) {
          Log.WL(1, beacon.parent.PilotableActorDef.ChassisID+" activated "+ beacon.transform.position);
          beacon.parent.CanBeBossSeen = true;
          beacon.triggered = true;
          beacon.parent.TeleportActor(beacon.transform.position);
          beacon.Reveal();
          beacon.parent.Combat.LocalPlayerTeam.RebuildVisibilityCacheAllUnits(beacon.parent.Combat.GetAllImporantCombatants());
          if (beacon.parent is CustomMech custMech) { custMech.BossAppearAnimation(); }
        }
      }
    }
    public static void Clear() {
      List<BossAppearBeacon> beacons = bossBeacons.ToList();
      foreach (BossAppearBeacon beacon in beacons) {
        GameObject.Destroy(beacon.gameObject);
      }
      bossBeacons.Clear();
      if (landedEffect != null) { GameObject.Destroy(landedEffect); }
      if (dropOffDustEffect != null) { GameObject.Destroy(dropOffDustEffect); }
      if (dropOffEngineEffect != null) { GameObject.Destroy(dropOffEngineEffect); }
    }
    public static bool isEffectsInited() {
      return landedEffect != null && dropOffDustEffect != null && dropOffEngineEffect != null;
    }
    public static void AddBossJets(this MechFlyHeightController heightController) {
      if (isEffectsInited() == false) { InitEffects(heightController.parent.parentActor.Combat); }
      if (heightController.parent.customRep != null) {
        if (heightController.parent.customRep.CustomDefinition != null) {
          foreach (string jetAttach in heightController.parent.customRep.CustomDefinition.JetStreamsAttaches) {
            Log.WL(3, "attach:" + jetAttach);
            Transform attach = heightController.transform.FindRecursive(jetAttach);
            if (attach == null) { Log.WL(4, "attach is null"); continue; }
            GameObject jumpJetBase = new GameObject("bossJet");
            jumpJetBase.transform.SetParent(attach);
            jumpJetBase.transform.localPosition = Vector3.zero;
            jumpJetBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject jumpJet = GameObject.Instantiate(dropOffEngineEffect);
            jumpJet.SetActive(true);
            ParticleSystem[] psystems = jumpJet.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem pssys in psystems) {
              Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
              var main = pssys.main;
              main.loop = false;
              main.playOnAwake = false;
              main.scalingMode = ParticleSystemScalingMode.Hierarchy;
              pssys.Stop(true);
              pssys.Clear(true);
            }
            jumpJet.transform.SetParent(jumpJetBase.transform);
            jumpJet.transform.localPosition = Vector3.zero;
            jumpJet.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            JumpjetRepresentation jRep = jumpJetBase.AddComponent<JumpjetRepresentation>();
            jRep.Init(heightController.parent.parentMech, attach, true, false, heightController.parent.name);
            jumpJetBase.transform.localScale = new Vector3(3f, 3f, 3f);
            heightController.verticalJets.Add(jRep);
            heightController.verticalJetsObjects.Add(jumpJetBase);
            jumpJetBase.SetActive(false);
          }
        }
        {
          GameObject dustBase = new GameObject("dustBase");
          dustBase.transform.SetParent(heightController.parent.j_Root);
          dustBase.transform.localPosition = Vector3.zero;
          dustBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
          GameObject dustEffect = GameObject.Instantiate(dropOffDustEffect);
          dustEffect.SetActive(true);
          ParticleSystem[] psystems = dustEffect.GetComponentsInChildren<ParticleSystem>(true);
          foreach (ParticleSystem pssys in psystems) {
            Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
            var main = pssys.main;
            main.loop = false;
            main.playOnAwake = false;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            pssys.Stop(true);
            pssys.Clear(true);
          }
          dustEffect.transform.SetParent(dustBase.transform);
          dustEffect.transform.localPosition = Vector3.zero;
          dustEffect.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
          JumpjetRepresentation jDustRep = dustBase.AddComponent<JumpjetRepresentation>();
          jDustRep.Init(heightController.parent.parentMech, heightController.parent.j_Root, true, false, heightController.parent.name);
          dustEffect.transform.localScale = new Vector3(2f, 2f, 2f);
          heightController.verticalJets.Add(jDustRep);
          heightController.verticalJetsObjects.Add(dustBase);
          dustBase.SetActive(false);
        }
        {
          GameObject windBase = new GameObject("windBase");
          windBase.transform.SetParent(heightController.parent.j_Root);
          windBase.transform.localPosition = Vector3.zero;
          windBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
          GameObject windEffect = GameObject.Instantiate(landedEffect);
          windEffect.SetActive(true);
          ParticleSystem[] psystems = windEffect.GetComponentsInChildren<ParticleSystem>(true);
          foreach (ParticleSystem pssys in psystems) {
            Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
            var main = pssys.main;
            main.loop = false;
            main.playOnAwake = false;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            pssys.Stop(true);
            pssys.Clear(true);
          }
          windEffect.transform.SetParent(windBase.transform);
          windEffect.transform.localPosition = Vector3.zero;
          windEffect.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
          JumpjetRepresentation jWindRep = windBase.AddComponent<JumpjetRepresentation>();
          jWindRep.Init(heightController.parent.parentMech, heightController.parent.j_Root, true, false, heightController.parent.name);
          windEffect.transform.localScale = new Vector3(2f, 2f, 2f);
          heightController.verticalJets.Add(jWindRep);
          heightController.verticalJetsObjects.Add(windBase);
          windBase.SetActive(false);
        }
      }
    }
    public static void InitEffects(CombatGameState combat) {
      try {
        GameObject leopard = combat.DataManager.PooledInstantiate(Core.Settings.CustomJetsStreamsPrefabSrc, BattleTechResourceType.Prefab);
        try {
          Component[] components = leopard.GetComponentsInChildren<Component>(true);
          foreach (Component component in components) {
            if (component is ParticleSystem) { continue; }
            if (component is ParticleSystemRenderer) { continue; }
            if (component is Transform) { continue; }
            GameObject.DestroyImmediate(component);
          }
          Transform vfxPrfPrtl_leopardLanding = leopard.transform.FindRecursive("vfxPrfPrtl_leopardLanding");
          Transform vfxPrfPrtl_leopardTakeoff = leopard.transform.FindRecursive("vfxPrfPrtl_leopardTakeoff");
          if (vfxPrfPrtl_leopardLanding != null) { GameObject.DestroyImmediate(vfxPrfPrtl_leopardLanding.gameObject); }
          if (vfxPrfPrtl_leopardTakeoff != null) { GameObject.DestroyImmediate(vfxPrfPrtl_leopardTakeoff.gameObject); }

          landedEffect = GameObject.Instantiate(leopard.transform.FindRecursive(landedEffectObjectName).gameObject);
          dropOffDustEffect = GameObject.Instantiate(leopard.transform.FindRecursive(dropOffDustEffectName).gameObject);
          dropOffEngineEffect = GameObject.Instantiate(leopard.transform.FindRecursive(dropOffEngineEffectName).gameObject);
          ParticleSystem[] psystems = landedEffect.GetComponentsInChildren<ParticleSystem>(true);
          foreach (ParticleSystem pssys in psystems) {
            Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
            var main = pssys.main;
            main.loop = true;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            pssys.Stop(true);
            pssys.Clear(true);
            pssys.Play(true);
          }
          psystems = dropOffDustEffect.GetComponentsInChildren<ParticleSystem>(true);
          foreach (ParticleSystem pssys in psystems) {
            Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
            var main = pssys.main;
            main.loop = true;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            pssys.Stop(true);
            pssys.Clear(true);
            pssys.Play(true);
          }
          psystems = dropOffEngineEffect.GetComponentsInChildren<ParticleSystem>(true);
          foreach (ParticleSystem pssys in psystems) {
            Log.WL(1, "ParticleSystem:" + pssys.gameObject.name);
            var main = pssys.main;
            main.loop = true;
            main.playOnAwake = true;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
          }
          landedEffect.SetActive(false);
          dropOffDustEffect.SetActive(false);
          dropOffEngineEffect.SetActive(false);

          //Transform[] childs = dropOffDustEffect.GetComponentsInChildren<Transform>(true);
          //HashSet<Transform> childsToDelete = new HashSet<Transform>();
          //foreach (Transform child in childs) {
          //  if (child.transform.parent != dropOffDustEffect.transform) { continue; }
          //  childsToDelete.Add(child);
          //}
          //foreach (Transform child in childsToDelete) {
          //  GameObject.Destroy(child.gameObject);
          //}

        } finally {
          if(leopard != null)GameObject.Destroy(leopard);
        }
      } catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
    public static string SafeToString(this object obj) {
      if (obj == null) { return "(null)"; } else { return obj.ToString(); }
    }
    public static void MinMaxCurveDump(this ParticleSystem.MinMaxCurve curve, int initiation) {
      Log.WL(initiation, "mode:" + curve.mode);
      Log.WL(initiation, "constant:" + curve.constant);
      Log.WL(initiation, "constantMax:" + curve.constantMax);
      Log.WL(initiation, "constantMin:" + curve.constantMin);
      Log.WL(initiation, "curveMultiplier:" + curve.curveMultiplier);
      Log.WL(initiation, "curve");
      curve.curve.AnimationCurveDump(initiation+1);
      Log.WL(initiation, "curveMax");
      curve.curveMax.AnimationCurveDump(initiation + 1);
      Log.WL(initiation, "curveMin");
      curve.curveMin.AnimationCurveDump(initiation + 1);
    }
    public static void AnimationCurveDump(this AnimationCurve curve, int initiation) {
      if (curve == null) {
        Log.WL(initiation, "(null)");
        return;
      }
      Log.W(initiation, "keys:" + curve.keys.Length);
      foreach(var key in curve.keys) {
        Log.W(1, "{" + key.time + ":" + key.value + "}");
      }
      Log.WL(0,"");
    }
    public static void ParticleSystemDump(this ParticleSystem particleSystem, int initiation) {
      Log.WL(initiation, "main.cullingMode:" + particleSystem.main.cullingMode);
      Log.WL(initiation, "main.customSimulationSpace:" + particleSystem.main.customSimulationSpace.SafeToString());
      Log.WL(initiation, "main.duration:" + particleSystem.main.duration);
      Log.WL(initiation, "main.emitterVelocityMode:" + particleSystem.main.emitterVelocityMode);
      Log.WL(initiation, "main.flipRotation:" + particleSystem.main.flipRotation);
      Log.WL(initiation, "main.gravityModifier");
      particleSystem.main.gravityModifier.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.gravityModifierMultiplier:" + particleSystem.main.gravityModifierMultiplier);
      Log.WL(initiation, "main.loop:" + particleSystem.main.loop);
      Log.WL(initiation, "main.maxParticles:" + particleSystem.main.maxParticles);
      Log.WL(initiation, "main.playOnAwake:" + particleSystem.main.playOnAwake);
      Log.WL(initiation, "main.prewarm:" + particleSystem.main.prewarm);
      Log.WL(initiation, "main.ringBufferLoopRange:" + particleSystem.main.ringBufferLoopRange);
      Log.WL(initiation, "main.ringBufferMode:" + particleSystem.main.ringBufferMode);
      Log.WL(initiation, "main.scalingMode:" + particleSystem.main.scalingMode);
      Log.WL(initiation, "main.simulationSpace:" + particleSystem.main.simulationSpace);
      Log.WL(initiation, "main.simulationSpeed:" + particleSystem.main.simulationSpeed);
      Log.WL(initiation, "main.startColor:" + particleSystem.main.startColor);
      Log.WL(initiation, "main.startDelay");
      particleSystem.main.startDelay.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startDelayMultiplier:" + particleSystem.main.startDelayMultiplier);
      Log.WL(initiation, "main.startLifetime");
      particleSystem.main.startLifetime.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startLifetimeMultiplier:" + particleSystem.main.startLifetimeMultiplier);
      Log.WL(initiation, "main.startRotation");
      particleSystem.main.startRotation.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startRotation3D:" + particleSystem.main.startRotation3D);
      Log.WL(initiation, "main.startRotationMultiplier:" + particleSystem.main.startRotationMultiplier);
      Log.WL(initiation, "main.startRotationX");
      particleSystem.main.startRotationX.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startRotationXMultiplier:" + particleSystem.main.startRotationXMultiplier);
      Log.WL(initiation, "particleSystem.main.startRotationY");
      particleSystem.main.startRotationY.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startRotationYMultiplier:" + particleSystem.main.startRotationYMultiplier);
      Log.WL(initiation, "main.startRotationZ");
      particleSystem.main.startRotationZ.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startRotationZMultiplier:" + particleSystem.main.startRotationZMultiplier);
      Log.WL(initiation, "main.startSize");
      particleSystem.main.startSize.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startSize3D:" + particleSystem.main.startSize3D);
      Log.WL(initiation, "main.startSizeMultiplier:" + particleSystem.main.startSizeMultiplier);
      Log.WL(initiation, "particleSystem.main.startSizeX");
      particleSystem.main.startSizeX.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startSizeXMultiplier:" + particleSystem.main.startSizeXMultiplier);
      Log.WL(initiation, "particleSystem.main.startSizeY");
      particleSystem.main.startSizeY.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startSizeYMultiplier:" + particleSystem.main.startSizeYMultiplier);
      Log.WL(initiation, "main.startSizeZ");
      particleSystem.main.startSizeZ.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "main.startSizeZMultiplier:" + particleSystem.main.startSizeZMultiplier);
      Log.WL(initiation, "main.stopAction:" + particleSystem.main.stopAction);
      Log.WL(initiation, "main.useUnscaledTime:" + particleSystem.main.useUnscaledTime);

      Log.WL(initiation, "emission.enabled:" + particleSystem.emission.enabled);
      Log.WL(initiation, "emission.burstCount:" + particleSystem.emission.burstCount);
      Log.WL(initiation, "particleSystem.emission.rateOverDistance");
      particleSystem.emission.rateOverDistance.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "emission.rateOverDistanceMultiplier:" + particleSystem.emission.rateOverDistanceMultiplier);
      Log.WL(initiation, "emission.rateOverTime");
      particleSystem.emission.rateOverTime.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "emission.rateOverTimeMultiplier:" + particleSystem.emission.rateOverTimeMultiplier);
      Log.WL(initiation, "collision.enabled:" + particleSystem.collision.enabled);
      Log.WL(initiation, "colorOverLifetime.enabled:" + particleSystem.colorOverLifetime.enabled);
      Log.WL(initiation, "colorBySpeed.enabled:" + particleSystem.colorBySpeed.enabled);
      Log.WL(initiation, "customData.enabled:" + particleSystem.customData.enabled);
      Log.WL(initiation, "externalForces.enabled:" + particleSystem.externalForces.enabled);
      Log.WL(initiation, "forceOverLifetime.enabled:" + particleSystem.forceOverLifetime.enabled);
      Log.WL(initiation, "inheritVelocity.enabled:" + particleSystem.inheritVelocity.enabled);
      Log.WL(initiation, "lights.enabled:" + particleSystem.lights.enabled);
      Log.WL(initiation, "limitVelocityOverLifetime.enabled:" + particleSystem.limitVelocityOverLifetime.enabled);
      Log.WL(initiation, "noise.enabled:" + particleSystem.noise.enabled);
      Log.WL(initiation, "rotationBySpeed.enabled:" + particleSystem.rotationBySpeed.enabled);
      Log.WL(initiation, "rotationOverLifetime.enabled:" + particleSystem.rotationOverLifetime.enabled);
      Log.WL(initiation, "sizeBySpeed.enabled:" + particleSystem.sizeBySpeed.enabled);
      Log.WL(initiation, "sizeOverLifetime.enabled:" + particleSystem.sizeOverLifetime.enabled);
      Log.WL(initiation, "sizeOverLifetime.separateAxes:" + particleSystem.sizeOverLifetime.separateAxes);
      Log.WL(initiation, "sizeOverLifetime.size");
      particleSystem.sizeOverLifetime.size.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "sizeOverLifetime.sizeMultiplier:" + particleSystem.sizeOverLifetime.sizeMultiplier);
      Log.WL(initiation, "sizeOverLifetime.x");
      particleSystem.sizeOverLifetime.x.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "sizeOverLifetime.xMultiplier:" + particleSystem.sizeOverLifetime.xMultiplier);
      Log.WL(initiation, "sizeOverLifetime.y");
      particleSystem.sizeOverLifetime.y.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "sizeOverLifetime.yMultiplier:" + particleSystem.sizeOverLifetime.yMultiplier);
      Log.WL(initiation, "sizeOverLifetime.z");
      particleSystem.sizeOverLifetime.z.MinMaxCurveDump(initiation + 1);
      Log.WL(initiation, "sizeOverLifetime.zMultiplier:" + particleSystem.sizeOverLifetime.zMultiplier);
      Log.WL(initiation, "subEmitters.enabled:" + particleSystem.subEmitters.enabled);
      Log.WL(initiation, "textureSheetAnimation.enabled:" + particleSystem.textureSheetAnimation.enabled);
    }
    public static void CreateBossBeacon(this CustomMech parent, Vector3 position) {
      GameObject beaconObj = new GameObject("BossBeacon_"+parent.GUID);
      try {
        beaconObj.transform.position = position;
        BossAppearBeacon beacon = beaconObj.AddComponent<BossAppearBeacon>();
        beacon.Init(parent);
        bossBeacons.Add(beacon);
        //GameObject leopard1 = parent.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJetsStreamsPrefabSrc, BattleTechResourceType.Prefab);
        //if (leopard1 != null) {
        //  Log.TWL(0,"LEOPARD1");
        //  Component[] components = leopard1.GetComponentsInChildren<Component>(true);
        //  foreach(Component component in components) {
        //    if (component is ParticleSystem) { continue; }
        //    if (component is ParticleSystemRenderer) { continue; }
        //    if (component is Transform) { continue; }
        //    GameObject.DestroyImmediate(component);
        //  }
        //  Transform vfxPrfPrtl_leopardLanding = leopard1.transform.FindRecursive("vfxPrfPrtl_leopardLanding");
        //  Transform vfxPrfPrtl_leopardTakeoff = leopard1.transform.FindRecursive("vfxPrfPrtl_leopardTakeoff");
        //  if (vfxPrfPrtl_leopardLanding != null) { GameObject.DestroyImmediate(vfxPrfPrtl_leopardLanding.gameObject); }
        //  if (vfxPrfPrtl_leopardTakeoff != null) { GameObject.DestroyImmediate(vfxPrfPrtl_leopardTakeoff.gameObject); }
        //  leopard1.SetActive(false);
        //  leopard1.transform.SetParent(beaconObj.transform);
        //  leopard1.transform.localPosition = Vector3.up * 50f;
        //  ParticleSystem[] psystems = leopard1.GetComponentsInChildren<ParticleSystem>(true);
        //  foreach (ParticleSystem pssys in psystems) {
        //    Log.WL(1, "ParticleSystem:"+pssys.gameObject.name);
        //    try {
        //      var main = pssys.main;
        //      main.loop = true;
        //      main.playOnAwake = true;
        //      main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        //      //main.startSize = new ParticleSystem.MinMaxCurve(1f);
        //      //var emission = pssys.emission;
        //      //if(emission.rateOverTime.mode == ParticleSystemCurveMode.Curve) {
        //      //  emission.rateOverTime = new ParticleSystem.MinMaxCurve(emission.rateOverTime.curveMultiplier);
        //      //}
        //      //var sizeOverLifetime = pssys.sizeOverLifetime;
        //      //if (sizeOverLifetime.size.mode == ParticleSystemCurveMode.Curve) {
        //      //  sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(sizeOverLifetime.size.curveMultiplier);
        //      //}
        //      //if (sizeOverLifetime.x.mode == ParticleSystemCurveMode.Curve) {
        //      //  sizeOverLifetime.x = new ParticleSystem.MinMaxCurve(sizeOverLifetime.x.curveMultiplier);
        //      //}
        //      //if (sizeOverLifetime.y.mode == ParticleSystemCurveMode.Curve) {
        //      //  sizeOverLifetime.y = new ParticleSystem.MinMaxCurve(sizeOverLifetime.y.curveMultiplier);
        //      //}
        //      //if (sizeOverLifetime.z.mode == ParticleSystemCurveMode.Curve) {
        //      //  sizeOverLifetime.z = new ParticleSystem.MinMaxCurve(sizeOverLifetime.z.curveMultiplier);
        //      //}
        //      //Log.WL(1, JsonConvert.SerializeObject(pssys, Formatting.Indented));
        //      pssys.ParticleSystemDump(2);
        //    } catch (Exception e) {
        //      Log.TWL(0, e.ToString(), true);
        //    }
        //  }
        //}
        //GameObject leopard2 = parent.Combat.DataManager.PooledInstantiate(Core.Settings.CustomJetsStreamsPrefabSrc, BattleTechResourceType.Prefab);
        //if (leopard2 != null) {
        //  leopard2.SetActive(false);
        //  leopard2.transform.SetParent(beaconObj.transform);
        //  leopard2.transform.localPosition = Vector3.up * 70f;
        //}
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}