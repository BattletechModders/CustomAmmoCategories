/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Assetbundles;
using BattleTech.Data;
using BattleTech.ModSupport.Utils;
using BattleTech.Rendering;
using BattleTech.Rendering.UI;
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using HBS;
using HBS.Collections;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityHeapCrawler;

namespace CustomUnits {
  public static class DamageLocationHelper {
    public static bool DamageLocation_private(this Mech mech,int originalHitLoc,WeaponHitInfo hitInfo,ArmorLocation aLoc,Weapon weapon,float totalArmorDamage,float directStructureDamage,int hitIndex,AttackImpactQuality impactQuality,DamageType damageType) {
      try {
        return mech.DamageLocation(originalHitLoc, hitInfo, aLoc, weapon, totalArmorDamage, directStructureDamage, hitIndex, impactQuality, damageType);
      }catch(Exception e) {
        CustomDeploy.Log.TWL(0,e.ToString(),true);
        return false;
      }
    }
  }
  public static class MechRepresentationHelper {
    public static List<JumpjetRepresentation> jumpjetReps(this MechRepresentation mechRepresentation) { return mechRepresentation.jumpjetReps; }
    public static void jumpjetReps(this MechRepresentation mechRepresentation, List<JumpjetRepresentation> val) { mechRepresentation.jumpjetReps = val; }
    public static List<GameObject> headlightReps(this MechRepresentation mechRepresentation) { return mechRepresentation.headlightReps; }
    public static void headlightReps(this MechRepresentation mechRepresentation, List<GameObject> val) { mechRepresentation.headlightReps = val; }
    public static AnimatorTransitionInfo previousAnimTransition(this MechRepresentation mechRepresentation) { return mechRepresentation.previousAnimTransition; }
    public static void previousAnimTransition(this MechRepresentation mechRepresentation, AnimatorTransitionInfo val) { mechRepresentation.previousAnimTransition = val; }
    public static AnimatorStateInfo previousAnimState(this MechRepresentation mechRepresentation) { return mechRepresentation.previousAnimState; }
    public static void previousAnimState(this MechRepresentation mechRepresentation, AnimatorStateInfo val) { mechRepresentation.previousAnimState = val; }
    public static AnimatorTransitionInfo currentAnimTransition(this MechRepresentation mechRepresentation) { return mechRepresentation.currentAnimTransition; }
    public static void currentAnimTransition(this MechRepresentation mechRepresentation, AnimatorTransitionInfo val) { mechRepresentation.currentAnimTransition = val; }
    public static AnimatorStateInfo currentAnimState(this MechRepresentation mechRepresentation) { return mechRepresentation.currentAnimState; }
    public static void currentAnimState(this MechRepresentation mechRepresentation, AnimatorStateInfo val) { mechRepresentation.currentAnimState = val; }
    public static int currentAnimStateHash(this MechRepresentation mechRepresentation) { return mechRepresentation.currentAnimStateHash; }
    public static void currentAnimStateHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.currentAnimStateHash = val; }
    public static int previousTransitionHash(this MechRepresentation mechRepresentation) { return mechRepresentation.previousTransitionHash; }
    public static void previousTransitionHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.previousTransitionHash = val; }
    public static int previousAnimStateHash(this MechRepresentation mechRepresentation) { return mechRepresentation.previousAnimStateHash; }
    public static void previousAnimStateHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.previousAnimStateHash = val; }
    public static bool isPlayingJumpSound(this MechRepresentation mechRepresentation) { return mechRepresentation.isPlayingJumpSound; }
    public static void isPlayingJumpSound(this MechRepresentation mechRepresentation, bool val) { mechRepresentation.isPlayingJumpSound = val; }
    public static bool isJumping(this MechRepresentation mechRepresentation) { return mechRepresentation.isJumping; }
    public static void isJumping(this MechRepresentation mechRepresentation, bool val) { mechRepresentation.isJumping = val; }
    public static PropertyBlockManager.PropertySetting heatAmount(this MechRepresentation mechRepresentation) { return mechRepresentation.heatAmount; }
    public static void heatAmount(this MechRepresentation mechRepresentation, PropertyBlockManager.PropertySetting val) { mechRepresentation.heatAmount = val; }
    public static bool isFakeOverheated(this MechRepresentation mechRepresentation) { return mechRepresentation.isFakeOverheated; }
    public static void isFakeOverheated(this MechRepresentation mechRepresentation, bool val) { mechRepresentation.isFakeOverheated = val; }
    public static bool triggerFootVFX(this MechRepresentation mechRepresentation) { return mechRepresentation.triggerFootVFX; }
    public static void triggerFootVFX(this MechRepresentation mechRepresentation, bool val) { mechRepresentation.triggerFootVFX = val; }
    public static int leftFootVFX(this MechRepresentation mechRepresentation) { return mechRepresentation.leftFootVFX; }
    public static void leftFootVFX(this MechRepresentation mechRepresentation, int val) { mechRepresentation.leftFootVFX = val; }
    public static List<string> persistentCritList(this MechRepresentation mechRepresentation) { return mechRepresentation.persistentCritList; }
    public static void persistentCritList(this MechRepresentation mechRepresentation, List<string> val) { mechRepresentation.persistentCritList = val; }
    public static Vector3 blipPendingPosition(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.blipPendingPosition; }
    public static void blipPendingPosition(this PilotableActorRepresentation mechRepresentation, Vector3 val) { mechRepresentation.blipPendingPosition = val; }
    public static Quaternion blipPendingRotation(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.blipPendingRotation; }
    public static void blipPendingRotation(this PilotableActorRepresentation mechRepresentation, Quaternion val) { mechRepresentation.blipPendingRotation = val; }
    public static bool blipHasPendingPositionRotation(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.blipHasPendingPositionRotation; }
    public static void blipHasPendingPositionRotation(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.blipHasPendingPositionRotation = val; }
    public static float blipLastUpdateTime(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.blipLastUpdateTime; }
    public static void blipLastUpdateTime(this PilotableActorRepresentation mechRepresentation, float val) { mechRepresentation.blipLastUpdateTime = val; }
    public static CapsuleCollider mainCollider(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.mainCollider; }
    public static void mainCollider(this PilotableActorRepresentation mechRepresentation, CapsuleCollider val) { mechRepresentation.mainCollider = val; }
    public static bool paintSchemeInitialized(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.paintSchemeInitialized; }
    public static void paintSchemeInitialized(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.paintSchemeInitialized = val; }
    public static PilotableActorRepresentation pooledPrefab(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.pooledPrefab; }
    public static void pooledPrefab(this PilotableActorRepresentation mechRepresentation, PilotableActorRepresentation val) { mechRepresentation.pooledPrefab = val; }
    public static Mech mech(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.mech; }
    public static void mech(this PilotableActorRepresentation mechRepresentation, Mech val) { mechRepresentation.mech = val; }
    public static Vehicle vehicle(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.vehicle; }
    public static void vehicle(this PilotableActorRepresentation mechRepresentation, Vehicle val) { mechRepresentation.vehicle = val; }
    public static Turret turret(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.turret; }
    public static void turret(this PilotableActorRepresentation mechRepresentation, Turret val) { mechRepresentation.turret = val; }
    public static string prefabId(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.prefabId; }
    public static void prefabId(this PilotableActorRepresentation mechRepresentation, string val) { mechRepresentation.prefabId = val; }
    public static GameObject testGO(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.testGO; }
    public static void testGO(this PilotableActorRepresentation mechRepresentation, GameObject val) { mechRepresentation.testGO = val; }
    public static int framesCounted(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.framesCounted; }
    public static void framesCounted(this PilotableActorRepresentation mechRepresentation, int val) { mechRepresentation.framesCounted = val; }
    public static int framesToSkip(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.framesToSkip; }
    public static void framesToSkip(this PilotableActorRepresentation mechRepresentation, int val) { mechRepresentation.framesToSkip = val; }
    public static List<Renderer> rendererList(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.rendererList; }
    public static void rendererList(this PilotableActorRepresentation mechRepresentation, List<Renderer> val) { mechRepresentation.rendererList = val; }
    public static Renderer localRenderer(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.localRenderer; }
    public static void localRenderer(this PilotableActorRepresentation mechRepresentation, Renderer val) { mechRepresentation.localRenderer = val; }
    public static Renderer pooledPrefabRenderer(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.pooledPrefabRenderer; }
    public static void pooledPrefabRenderer(this PilotableActorRepresentation mechRepresentation, Renderer val) { mechRepresentation.pooledPrefabRenderer = val; }
    public static Material[] sharedMaterialsSource(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.sharedMaterialsSource; }
    public static void sharedMaterialsSource(this PilotableActorRepresentation mechRepresentation, Material[] val) { mechRepresentation.sharedMaterialsSource = val; }
    public static Material[] sharedMaterialsCopy(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.sharedMaterialsCopy; }
    public static void sharedMaterialsCopy(this PilotableActorRepresentation mechRepresentation, Material[] val) { mechRepresentation.sharedMaterialsCopy = val; }
    public static Material defaultMaterial(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.defaultMaterial; }
    public static void defaultMaterial(this PilotableActorRepresentation mechRepresentation, Material val) { mechRepresentation.defaultMaterial = val; }
    public static bool wasEvasiveLastFrame(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.wasEvasiveLastFrame; }
    public static void wasEvasiveLastFrame(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.wasEvasiveLastFrame = val; }
    public static bool guardedLastFrame(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.guardedLastFrame; }
    public static void guardedLastFrame(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.guardedLastFrame = val; }
    public static bool coverLastFrame(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.coverLastFrame; }
    public static void coverLastFrame(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.coverLastFrame = val; }
    public static bool wasUnsteadyLastFrame(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.wasUnsteadyLastFrame; }
    public static void wasUnsteadyLastFrame(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.wasUnsteadyLastFrame = val; }
    public static bool wasEntrenchedLastFrame(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.wasEntrenchedLastFrame; }
    public static void wasEntrenchedLastFrame(this PilotableActorRepresentation mechRepresentation, bool val) { mechRepresentation.wasEntrenchedLastFrame = val; }
    public static float timeNow(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.timeNow; }
    public static void timeNow(this PilotableActorRepresentation mechRepresentation, float val) { mechRepresentation.timeNow = val; }
    public static float elapsedTime(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.elapsedTime; }
    public static void elapsedTime(this PilotableActorRepresentation mechRepresentation, float val) { mechRepresentation.elapsedTime = val; }
    public static float blipAlpha(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.blipAlpha; }
    public static void blipAlpha(this PilotableActorRepresentation mechRepresentation, float val) { mechRepresentation.blipAlpha = val; }
    public static float timeFromEnd(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.timeFromEnd; }
    public static void timeFromEnd(this PilotableActorRepresentation mechRepresentation, float val) { mechRepresentation.timeFromEnd = val; }
    public static BattleTech.Rendering.MechCustomization.MechCustomization mechCustomization(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.mechCustomization; }
    public static void mechCustomization(this PilotableActorRepresentation mechRepresentation, BattleTech.Rendering.MechCustomization.MechCustomization val) { mechRepresentation.mechCustomization = val; }
    public static AudioSwitch_surface_type currentSurfaceType(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.currentSurfaceType; }
    public static void currentSurfaceType(this PilotableActorRepresentation mechRepresentation, AudioSwitch_surface_type val) { mechRepresentation.currentSurfaceType = val; }
    public static string terrainImpactParticleName(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.terrainImpactParticleName; }
    public static void terrainImpactParticleName(this PilotableActorRepresentation mechRepresentation, string val) { mechRepresentation.terrainImpactParticleName = val; }
    public static string vfxNameModifier(this PilotableActorRepresentation mechRepresentation) { return mechRepresentation.vfxNameModifier; }
    public static void vfxNameModifier(this PilotableActorRepresentation mechRepresentation, string val) { mechRepresentation.vfxNameModifier = val; }
    public static int idleStateEntryHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateEntryHash; }
    public static void idleStateEntryHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateEntryHash = val; }
    public static int idleStateFlavorsHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateFlavorsHash; }
    public static void idleStateFlavorsHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateFlavorsHash = val; }
    public static int idleStateUnsteadyHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateUnsteadyHash; }
    public static void idleStateUnsteadyHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateUnsteadyHash = val; }
    public static int idleStateMeleeBaseHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateMeleeBaseHash; }
    public static void idleStateMeleeBaseHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateMeleeBaseHash = val; }
    public static int idleStateMeleeEntryHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateMeleeEntryHash; }
    public static void idleStateMeleeEntryHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateMeleeEntryHash = val; }
    public static int idleStateMeleeHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateMeleeHash; }
    public static void idleStateMeleeHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateMeleeHash = val; }
    public static int idleStateMeleeUnsteadyHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleStateMeleeUnsteadyHash; }
    public static void idleStateMeleeUnsteadyHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleStateMeleeUnsteadyHash = val; }
    public static int TEMPIdleStateMeleeIdleHash(this MechRepresentation mechRepresentation) { return mechRepresentation.TEMPIdleStateMeleeIdleHash; }
    public static void TEMPIdleStateMeleeIdleHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.TEMPIdleStateMeleeIdleHash = val; }
    public static int idleRandomValueHash(this MechRepresentation mechRepresentation) { return mechRepresentation.idleRandomValueHash; }
    public static void idleRandomValueHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.idleRandomValueHash = val; }
    public static int standingHash(this MechRepresentation mechRepresentation) { return mechRepresentation.standingHash; }
    public static void standingHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.standingHash = val; }
    public static int groundDeathIdleHash(this MechRepresentation mechRepresentation) { return mechRepresentation.groundDeathIdleHash; }
    public static void groundDeathIdleHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.groundDeathIdleHash = val; }
    public static int randomDeathIdleA(this MechRepresentation mechRepresentation) { return mechRepresentation.randomDeathIdleA; }
    public static void randomDeathIdleA(this MechRepresentation mechRepresentation, int val) { mechRepresentation.randomDeathIdleA = val; }
    public static int randomDeathIdleB(this MechRepresentation mechRepresentation) { return mechRepresentation.randomDeathIdleB; }
    public static void randomDeathIdleB(this MechRepresentation mechRepresentation, int val) { mechRepresentation.randomDeathIdleB = val; }
    public static int randomDeathIdleC(this MechRepresentation mechRepresentation) { return mechRepresentation.randomDeathIdleC; }
    public static void randomDeathIdleC(this MechRepresentation mechRepresentation, int val) { mechRepresentation.randomDeathIdleC = val; }
    public static int randomDeathIdleBase(this MechRepresentation mechRepresentation) { return mechRepresentation.randomDeathIdleBase; }
    public static void randomDeathIdleBase(this MechRepresentation mechRepresentation, int val) { mechRepresentation.randomDeathIdleBase = val; }
    public static int randomDeathIdleRandomizer(this MechRepresentation mechRepresentation) { return mechRepresentation.randomDeathIdleRandomizer; }
    public static void randomDeathIdleRandomizer(this MechRepresentation mechRepresentation, int val) { mechRepresentation.randomDeathIdleRandomizer = val; }
    public static int hitReactLightHash(this MechRepresentation mechRepresentation) { return mechRepresentation.hitReactLightHash; }
    public static void hitReactLightHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.hitReactLightHash = val; }
    public static int hitReactHeavyHash(this MechRepresentation mechRepresentation) { return mechRepresentation.hitReactHeavyHash; }
    public static void hitReactHeavyHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.hitReactHeavyHash = val; }
    public static int hitReactMeleeHash(this MechRepresentation mechRepresentation) { return mechRepresentation.hitReactMeleeHash; }
    public static void hitReactMeleeHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.hitReactMeleeHash = val; }
    public static int hitReactDodgeHash(this MechRepresentation mechRepresentation) { return mechRepresentation.hitReactDodgeHash; }
    public static void hitReactDodgeHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.hitReactDodgeHash = val; }
    public static int hitReactDFAHash(this MechRepresentation mechRepresentation) { return mechRepresentation.hitReactDFAHash; }
    public static void hitReactDFAHash(this MechRepresentation mechRepresentation, int val) { mechRepresentation.hitReactDFAHash = val; }
    public static CombatGameConstants Constants(this MechRepresentation mechRepresentation) { return mechRepresentation.Constants; }
    public static void Constants(this MechRepresentation mechRepresentation, CombatGameConstants val) { mechRepresentation.Constants = val; }
    public static Transform projectileTransform(this WeaponEffect weffect) { return weffect.projectileTransform; }
    public static void projectileTransform(this WeaponEffect weffect, Transform val) { weffect.projectileTransform = val; }
    public static float t(this WeaponEffect weffect) { return weffect.t; }
    public static void t(this WeaponEffect weffect, float val) { weffect.t = val; }
    public static CombatGameState Combat(this WeaponEffect weffect) { return weffect.Combat; }
    public static void Combat(this WeaponEffect weffect, CombatGameState val) { weffect.Combat = val; }
    public static float _stability(this Mech mech) { return mech._stability; }
    public static void _stability(this Mech mech, float val) { mech._stability = val; }
    public static float _tempHeat(this Mech mech) { return mech._tempHeat; }
    public static void _tempHeat(this Mech mech, int val) { mech._tempHeat = val; }
    public static float _heat(this Mech mech) { return mech._heat; }
    public static void _heat(this Mech mech, int val) { mech._heat = val; }
    public static void pilot(this Mech mech, Pilot val) { mech.pilot = val; }
    public static float MoveMultiplier(this Mech mech) { return mech.MoveMultiplier; }
    public static CombatHUD HUD(this SelectionStateMoveBase state) { return state.HUD; }
    public static CombatGameState Combat(this SelectionStateMoveBase state) { return state.Combat; }
    public static void _PublishInvocation(this SelectionState state, MessageCenter messageCenter, MessageCenterMessage invocation) {
      state.PublishInvocation(messageCenter, invocation);
    }
    public static void _CreateBlankPrefabs(this Mech mech, List<string> usedPrefabNames, ChassisLocations location) {
      mech.CreateBlankPrefabs(usedPrefabNames, location);
    }
    public static void init_heatAmount(this MechRepresentation mechRepresentation) {
      mechRepresentation.heatAmount = new PropertyBlockManager.PropertySetting("_Heat", 0.0f);
      mechRepresentation.propertyBlock.AddProperty(ref mechRepresentation.heatAmount);
    }
    public static void parentActor(this PilotableActorRepresentation rep, AbstractActor val) { rep.parentActor = val; }
  }
}

namespace CustomDeploy{
  public static class Log {
    //private static string m_assemblyFile;
    private static string m_logfile;
    private static readonly Mutex mutex = new Mutex();
    public static string BaseDirectory;
    private static StringBuilder m_cache = new StringBuilder();
    private static StreamWriter m_fs = null;
    private static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        Log.LogWrite("flush\n");
        Log.flush();
      }
    }
    public static void InitLog() {
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomDeploy.log");
      File.Delete(Log.m_logfile);
      Log.m_fs = new StreamWriter(Log.m_logfile);
      Log.m_fs.AutoFlush = true;
      Log.flushThread.Start();
    }
    public static void flush() {
      if (Log.mutex.WaitOne(1000)) {
        Log.m_fs.Write(Log.m_cache.ToString());
        Log.m_fs.Flush();
        Log.m_cache.Length = 0;
        Log.mutex.ReleaseMutex();
      }
    }
    public static void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n", isCritical);
      } else {
        LogWrite(prefix + line, isCritical);
      }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      //try {
      if ((Core.debugLog) || (isCritical)) {
        if (Log.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          //File.AppendAllText(Log.m_logfile, line);
          Log.mutex.ReleaseMutex();
        }
        if (isCritical) { Log.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
      }
      //} catch (Exception) {
      //i'm sertanly don't know what to do
      //}
    }
    public static void W(string line, bool isCritical = false) {
      LogWrite(line, isCritical);
    }
    public static void WL(string line, bool isCritical = false) {
      line += "\n"; W(line, isCritical);
    }
    public static void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; W(line, isCritical);
    }
    public static void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; WL(line, isCritical);
    }
    public static void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line, isCritical);
    }
    public static void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line, isCritical);
    }
  }
  //[HarmonyPatch(typeof(Quaternion))]
  //[HarmonyPatch("LookRotation")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3) })]
  //public static class Quaternion_LookRotation {
  //  public static void Prefix(Vector3 forward, Vector3 upwards) {
  //    if (forward == Vector3.zero) {
  //      Log.TWL(0, "Quaternion.LookRotation zero vector");
  //      Log.WL(0, Environment.StackTrace);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("DangerousLocationCellsList")]
  [HarmonyPatch(MethodType.Getter)]
  public static class UnitSpawnPointGameLogic_DangerousLocationCellsList {
    public static bool Prefix(UnitSpawnPointGameLogic __instance,ref List<MapTerrainDataCell> __result) {
      if (__instance.dangerousLocationCellList == null) {
        __instance.dangerousLocationCellList = new List<MapTerrainDataCell>(9);
        try {
          int xindex = __instance.Combat.MapMetaData.GetXIndex(__instance.hexPosition.x);
          int zindex = __instance.Combat.MapMetaData.GetZIndex(__instance.hexPosition.z);
          if(xindex == 0 || xindex >= (__instance.Combat.MapMetaData.mapTerrainDataCells.GetLength(1) - 1)) {
            Log.TWL(0, "UnitSpawnPointGameLogic too close to encounter bounds. Fix this! pos:"+ __instance.hexPosition+ " x:"+xindex+" z:"+zindex);
          }
          if (zindex == 0 || zindex >= (__instance.Combat.MapMetaData.mapTerrainDataCells.GetLength(0) - 1)) {
            Log.TWL(0, "UnitSpawnPointGameLogic too close to encounter bounds. Fix this! pos:" + __instance.hexPosition + " x:" + xindex + " z:" + zindex);
          }
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex, zindex)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex, zindex));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex, zindex + 1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex, zindex + 1));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex, zindex - 1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex, zindex - 1));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex+1, zindex)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex + 1, zindex));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex+1, zindex+1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex + 1, zindex + 1));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex+1, zindex-1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex + 1, zindex - 1));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex-1, zindex)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex - 1, zindex));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex-1, zindex+1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex - 1, zindex + 1));
          if (__instance.Combat.MapMetaData.IsWithinBounds(xindex-1, zindex-1)) __instance.dangerousLocationCellList.Add(__instance.Combat.MapMetaData.GetCellAt(xindex - 1, zindex - 1));
        }catch(Exception e) {
          Log.TWL(0, e.ToString());
        }
      }
      __result = __instance.dangerousLocationCellList;
      return false;
    }
  }
  [HarmonyPatch(typeof(InfluenceMapEvaluator))]
  [HarmonyPatch("GetEvaluationAtPositionOrientation")]
  [HarmonyPatch(MethodType.Normal)]
  public static class UnitSpawnPointGameLogic_GetEvaluationAtPositionOrientation {
    public static bool Prefix(InfluenceMapEvaluator __instance, Vector3 pos, int rotationIndex, MoveType moveType, PathNode pathNode, ref float __result) {
      __result = 0.0f;
      try {
        if (pathNode == null)
          throw new ArgumentException(string.Format("missing pathNode in GetEvaluationAtPositionOrientation", (object[])Array.Empty<object>()));
        if (moveType == MoveType.Walking && __instance.unit.GainsEntrenchedFromNormalMoves)
          __result += __instance.unit.BehaviorTree.GetBehaviorVariableValue(BehaviorVariableName.Float_SureFootingAbilityWalkBoost).FloatVal;
        for (int index = 0; index < __instance.positionalFactors.Length; ++index) {
          InfluenceMapPositionFactor positionalFactor = __instance.positionalFactors[index];
          BehaviorVariableName name = positionalFactor != null ? positionalFactor.GetRegularMoveWeightBVName() : throw new IndexOutOfRangeException(string.Format("null factor for pos index {0}", (object)index));
          float floatVal = (__instance.unit.BehaviorTree.GetBehaviorVariableValue(name) ?? throw new ArgumentException(string.Format("missing behavior variable value for {0}", (object)name))).FloatVal;
          if ((floatVal != 0f)&&(positionalFactor != null)) {
            try {
              __result += floatVal * positionalFactor.EvaluateInfluenceMapFactorAtPosition(__instance.unit, pos, (float)rotationIndex, moveType, pathNode);
            }catch(Exception e) {
              Log.TWL(0, "I'm just a victim here", true);
              Log.TWL(0, e.ToString(), true);
            }
          }
        }
        for (int index = 0; index < __instance.unit.BehaviorTree.enemyUnits.Count; ++index) {
          if (__instance.unit.BehaviorTree.enemyUnits[index] is AbstractActor enemyUnit)
            enemyUnit.EvaluateExpectedArmor();
        }
        __instance.unit.EvaluateExpectedArmor();
        for (int index1 = 0; index1 < __instance.hostileFactors.Length; ++index1) {
          InfluenceMapHostileFactor hostileFactor = __instance.hostileFactors[index1];
          float floatVal = __instance.unit.BehaviorTree.GetBehaviorVariableValue(hostileFactor.GetRegularMoveWeightBVName()).FloatVal;
          if ((double)floatVal != 0.0) {
            for (int index2 = 0; index2 < __instance.unit.BehaviorTree.enemyUnits.Count; ++index2) {
              ICombatant enemyUnit = __instance.unit.BehaviorTree.enemyUnits[index2];
              if (!__instance.unit.BehaviorTree.IsTargetIgnored(enemyUnit)) {
                float num2 = FactorUtil.HostileFactor(__instance.unit, enemyUnit);
                __result += floatVal * num2 * hostileFactor.EvaluateInfluenceMapFactorAtPositionWithHostile(__instance.unit, pos, (float)rotationIndex, moveType, enemyUnit);
              }
            }
          }
        }
        for (int index1 = 0; index1 < __instance.allyFactors.Length; ++index1) {
          InfluenceMapAllyFactor allyFactor = __instance.allyFactors[index1];
          float floatVal = __instance.unit.BehaviorTree.GetBehaviorVariableValue(allyFactor.GetRegularMoveWeightBVName()).FloatVal;
          if ((double)floatVal != 0.0) {
            for (int index2 = 0; index2 < __instance.unit.BehaviorTree.GetAllyUnits().Count; ++index2) {
              AbstractActor allyUnit = __instance.unit.BehaviorTree.GetAllyUnits()[index2];
              __result += floatVal * allyFactor.EvaluateInfluenceMapFactorAtPositionWithAlly(__instance.unit, pos, (float)rotationIndex, (ICombatant)allyUnit);
            }
          }
        }
        return false;
      } catch (Exception e) {
        Log.TWL(0, "I'm just a victim here", true);
        Log.TWL(0, e.ToString(),true);
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(Interpolator))]
  [HarmonyPatch("GetStringFromObjectDispatch")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Interpolator_GetStringFromObjectDispatch {
    private static string GetStringFromObjectDispatch_Local(object obj, string expr) {
      int num = expr.IndexOf('?');
      string text = expr.Substring(0, num);
      string text2 = expr.Substring(num + 1).Trim();
      List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
      string[] array = text2.Split(new char[]
      {
        '|'
      });
      for (int i = 0; i < array.Length; i++) {
        int num2 = array[i].IndexOf(':');
        string key = array[i].Substring(0, num2).Trim();
        string value = array[i].Substring(num2 + 1);
        list.Add(new KeyValuePair<string, string>(key, value));
      }
      object obj2 = null;
      try {
        obj2 = Interpolator.GetObjectByStringFromObject(text, obj);
      } catch (Exception arg) {
        string result = string.Format("ERROR: resolving '{0}' on {1}\n{2}", expr, obj, arg);
        Log.TWL(0, result + "\n" + arg.ToString(), true);
        return result;
      }
      if (obj2 == null) {
        return string.Format("ERROR: can't resolve '{0}' on {1}. Null value.", text, obj);
      }
      string a = obj2.ToString();
      for (int j = 0; j < list.Count; j++) {
        if (a == list[j].Key || list[j].Key == "Default") {
          return list[j].Value;
        }
      }
      return string.Format("ERROR: Could not resolve '{0}'", expr);
    }
    public static bool Prefix(ref string __result, object obj, string expr) {
      try {
        __result = GetStringFromObjectDispatch_Local(obj, expr);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Interpolator))]
  [HarmonyPatch("LookupStringFromObjectAndMaybeDispatch")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Interpolator_LookupStringFromObjectAndMaybeDispatch {
    private static string LookupStringFromObjectAndMaybeDispatch_Local(object obj, string expr, out bool dispatchLocalize) {
      if (expr.IndexOf('?') != -1) {
        dispatchLocalize = false;
        return Interpolator.GetStringFromObjectDispatch(obj, expr);
      }
      dispatchLocalize = true;
      string result;
      try {
        object objectByStringFromObject = Interpolator.GetObjectByStringFromObject(expr, obj);
        if (objectByStringFromObject != null) {
          result = objectByStringFromObject.ToString();
        } else {
          result = string.Format("ERROR: can not resolve '{0}' on {1}. Null value.", expr, obj);
        }
      } catch (Exception ex) {
        result = string.Format("ERROR: resolving '{0}' on {1}. {2}", expr, obj, ex.Message);
        Log.TWL(0, result + "\n"+ ex.ToString(), true);
      }
      return result;
    }

    public static bool Prefix(ref string __result,object obj, string expr, ref bool dispatchLocalize) {
      try {
        __result = LookupStringFromObjectAndMaybeDispatch_Local(obj, expr, out dispatchLocalize);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(HeapSnapshotCollector))]
  [HarmonyPatch("CollectStaticFields")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class HeapSnapshotCollector_CollectStaticFields {
    private static void CollectStaticFields_Local(this HeapSnapshotCollector __instance) {
      IEnumerable<Type> second = AppDomain.CurrentDomain.GetAssemblies().Where(new Func<Assembly, bool>(HeapSnapshotCollector.IsValidAssembly)).SelectMany((Assembly a) => a.GetTypesSafe());
      IEnumerable<Type> enumerable = __instance.staticTypes.Concat(second);
      HashSet<string> hashSet = new HashSet<string>();
      foreach (Type type in enumerable) {
        try {
          __instance.AddStaticFields(type, hashSet);
        } catch (Exception exception) {
          Debug.LogException(exception);
        }
      }
      if (hashSet.Count > 0) {
        List<string> list = hashSet.ToList<string>();
        list.Sort();
        using (StreamWriter streamWriter = new StreamWriter(__instance.outputDir + "generic-static-fields.txt")) {
          foreach (string value in list) {
            streamWriter.WriteLine(value);
          }
        }
      }
    }
    public static bool Prefix(HeapSnapshotCollector __instance) {
      try {
        __instance.CollectStaticFields_Local();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(AssemblyUtil))]
  [HarmonyPatch("FindMethods")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Assembly), typeof(string), typeof(string) })]
  public static class AssemblyUtil_FindMethods {
    public static MethodInfo[] FindMethods_Local(Assembly assembly, string methodName, string typeName = null) {
      List<Type> list = new List<Type>();
      if (typeName == null) {
        list.AddRange(from x in assembly.GetTypesSafe()
                      where x.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public) != null
                      select x);
      } else {
        list.Add(assembly.GetType(typeName));
      }
      if (list.Count == 0) {
        return null;
      }
      List<MethodInfo> list2 = new List<MethodInfo>();
      foreach (Type type in list) {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        list2.Add(method);
      }
      return list2.ToArray();
    }
    public static bool Prefix(ref MethodInfo[] __result, Assembly assembly, string methodName, string typeName) {
      try {
        __result = FindMethods_Local(assembly, methodName, typeName);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("OverrideSpawn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SpawnableUnit) })]
  public static class UnitSpawnPointGameLogic_OverrideSpawn_unit {
    public static void Postfix(UnitSpawnPointGameLogic __instance, SpawnableUnit spawnableUnit) {
      try {
        Log.TWL(0, "UnitSpawnPointGameLogic.OverrideSpawn SpawnableUnit");
        Log.WL(1, "team:" + spawnableUnit.TeamDefinitionGuid + " type:" + spawnableUnit.unitType + " MechDef:" + (spawnableUnit.Unit == null ? "null" : spawnableUnit.Unit.Description.Id)
          + " VehicleDef:" + (spawnableUnit.VUnit == null ? "null" : spawnableUnit.VUnit.Description.Id)
          + " TurretDef:" + (spawnableUnit.TUnit == null ? "null" : spawnableUnit.TUnit.Description.Id)
          + " UnitId:" + spawnableUnit.UnitId
          );
        if (__instance.Combat == null) { Log.WL(1,"combat is null. i can't proceed"); return; }
        if ((__instance.vehicleDefOverride != null)&&(__instance.unitType == UnitType.Vehicle)) {
          Log.WL(1, "vehicleDefOverride detected. replacing");
          if (__instance.mechDefOverride != null) {
            Log.WL(1, "strange mechDefOverride is also not null");
          }
          if (__instance.Combat.DataManager.MechDefs.Exists(__instance.vehicleDefOverride.Description.Id) == false) {
            Log.WL(1, "can't find corresponding mech "+ __instance.vehicleDefOverride.Description.Id);
            return;
          }
          __instance.mechDefOverride = __instance.Combat.DataManager.MechDefs.Get(__instance.vehicleDefOverride.Description.Id);
          __instance.vehicleDefOverride = null;
          __instance.unitType = UnitType.Mech;
        }
        if ((string.IsNullOrEmpty(__instance.vehicleDefId) == false) && (__instance.unitType == UnitType.Vehicle)) {
          Log.WL(1, "vehicleDefId detected. replacing");
          if (string.IsNullOrEmpty(__instance.mechDefId) == false) {
            Log.WL(1, "strange mechDefId is also not null");
          }
          __instance.mechDefId = __instance.vehicleDefId;
          __instance.vehicleDefId = string.Empty;
          __instance.unitType = UnitType.Mech;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(LanceConfiguration))]
  [HarmonyPatch("AddUnits")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IEnumerable<SpawnableUnit>) })]
  public static class LanceConfiguration_AddUnits {
    public static void Prefix(LanceConfiguration __instance, IEnumerable<SpawnableUnit> units) {
      try {
        Log.TWL(0, "LanceConfiguration.AddUnits");
        foreach(SpawnableUnit unit in units) {
          Log.WL(1, "team:"+unit.TeamDefinitionGuid+" type:"+unit.unitType+" MechDef:"+(unit.Unit==null?"null":unit.Unit.Description.Id)
            + " VehicleDef:" + (unit.VUnit == null ? "null" : unit.VUnit.Description.Id)
            + " TurretDef:" + (unit.TUnit == null ? "null" : unit.TUnit.Description.Id)
            + " UnitId:" + unit.UnitId
            );
          if (unit.unitType == UnitType.Vehicle) {
            unit.unitType = UnitType.Mech;
            if (unit.VUnit != null) {
              Log.WL(1, "VUnit is not null. Replacing");
              unit.VUnit = null;
              unit.Unit = UnityGameInstance.BattleTechGame.DataManager.MechDefs.Get(unit.UnitId);
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("OverrideSpawn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(UnitSpawnPointGameLogic) })]
  public static class UnitSpawnPointGameLogic_OverrideSpawn_logic {
    public static void Postfix(UnitSpawnPointGameLogic __instance, UnitSpawnPointGameLogic logic) {
      try {
        Log.TWL(0, "UnitSpawnPointGameLogic.OverrideSpawn SpawnableUnit");
        if (__instance.Combat == null) { Log.WL(1, "combat is null. i can't proceed"); return; }
        if ((__instance.vehicleDefOverride != null) && (__instance.unitType == UnitType.Vehicle)) {
          Log.WL(1, "vehicleDefOverride detected. replacing");
          if (__instance.mechDefOverride != null) {
            Log.WL(1, "strange mechDefOverride is also not null");
          }
          if (__instance.Combat.DataManager.MechDefs.Exists(__instance.vehicleDefOverride.Description.Id) == false) {
            Log.WL(1, "can't find corresponding mech " + __instance.vehicleDefOverride.Description.Id);
            return;
          }
          __instance.mechDefOverride = __instance.Combat.DataManager.MechDefs.Get(__instance.vehicleDefOverride.Description.Id);
          __instance.vehicleDefOverride = null;
          __instance.unitType = UnitType.Mech;
        }
        if ((string.IsNullOrEmpty(__instance.vehicleDefId) == false) && (__instance.unitType == UnitType.Vehicle)) {
          Log.WL(1, "vehicleDefId detected. replacing");
          if (string.IsNullOrEmpty(__instance.mechDefId) == false) {
            Log.WL(1, "strange mechDefId is also not null");
          }
          __instance.mechDefId = __instance.vehicleDefId;
          __instance.vehicleDefId = string.Empty;
          __instance.unitType = UnitType.Mech;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(AkTriggerBase))]
  [HarmonyPatch("GetAllDerivedTypes")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AkTriggerBase_GetAllDerivedTypes {
    public static Dictionary<uint, string> GetAllDerivedTypes_Local() {
      Type typeFromHandle = typeof(AkTriggerBase);
      Type[] types = typeFromHandle.Assembly.GetTypesSafe();
      Dictionary<uint, string> dictionary = new Dictionary<uint, string>();
      for (int i = 0; i < types.Length; i++) {
        if (types[i].IsClass && (types[i].IsSubclassOf(typeFromHandle) || (typeFromHandle.IsAssignableFrom(types[i]) && typeFromHandle != types[i]))) {
          string name = types[i].Name;
          dictionary.Add(AkUtilities.ShortIDGenerator.Compute(name), name);
        }
      }
      dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Awake"), "Awake");
      dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Start"), "Start");
      dictionary.Add(AkUtilities.ShortIDGenerator.Compute("Destroy"), "Destroy");
      return dictionary;
    }
    public static bool Prefix(ref Dictionary<uint, string> __result) {
      try {
        __result = GetAllDerivedTypes_Local();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechStatisticsRules))]
  [HarmonyPatch("CalculateCBillValue")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechStatisticsRules_CalculateCBillValues {
    public static bool Prefix(MechDef mechDef, ref float currentValue,ref float maxValue) {
      try {
        Log.TWL(0, "MechStatisticsRules.CalculateCBillValue "+ (mechDef.chassisID));
        if (mechDef.DataManager == null) {
          Log.WL(1, "no data manager. Fixing. UnityGameInstance.BattleTechGame.DataManager: "+(UnityGameInstance.BattleTechGame.DataManager == null?"null":"not null"));
          mechDef.DataManager = UnityGameInstance.BattleTechGame.DataManager;
          mechDef.Refresh();
        }
        if(mechDef.Chassis == null) {
          Log.WL(1, "no chassis manager. Fixing "+mechDef.ChassisID);
          if (mechDef.DataManager.ChassisDefs.Exists(mechDef.ChassisID)) {
            Log.WL(2, "found in data manager");
            mechDef.Chassis = mechDef.DataManager.ChassisDefs.Get(mechDef.ChassisID);
          }
        }
        currentValue = (float)mechDef.Chassis.Description.Cost;
        float num1 = 10000f;
        float num2 = (0.0f + mechDef.Head.AssignedArmor + mechDef.CenterTorso.AssignedArmor + mechDef.CenterTorso.AssignedRearArmor + mechDef.LeftTorso.AssignedArmor + mechDef.LeftTorso.AssignedRearArmor + mechDef.RightTorso.AssignedArmor + mechDef.RightTorso.AssignedRearArmor + mechDef.LeftArm.AssignedArmor + mechDef.RightArm.AssignedArmor + mechDef.LeftLeg.AssignedArmor + mechDef.RightLeg.AssignedArmor) * UnityGameInstance.BattleTechGame.MechStatisticsConstants.CBILLS_PER_ARMOR_POINT;
        currentValue += num2;
        for (int index = 0; index < mechDef.Inventory.Length; ++index) {
          MechComponentRef mechComponentRef = mechDef.Inventory[index];
          if (mechComponentRef.DataManager == null) { mechComponentRef.DataManager = mechDef.DataManager; }
          if (mechComponentRef.Def == null) { continue; }
          currentValue += (float)mechComponentRef.Def.Description.Cost;
        }
        currentValue = Mathf.Round(currentValue / num1) * num1;
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(LoadRequest))]
  [HarmonyPatch("TryCreateAndAddLoadRequest")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LoadRequest_TryCreateAndAddLoadRequest {
    public static void Postfix(LoadRequest __instance, BattleTechResourceType resourceType, string resourceId, bool __result) {
      try {
        if (__result == true) { return; }
        Log.TWL(0, "LoadRequest.TryCreateAndAddLoadRequest failed " + resourceId+" "+resourceType);
        VersionManifestEntry versionManifestEntry = __instance.dataManager.ResourceLocator.EntryByID(resourceId, resourceType);
        if (versionManifestEntry == null) {
          Log.WL(1,"manifest entry is null");
          Log.WL(1, Environment.StackTrace);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MainMenu))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MainMenu_OnAddedToHierarchy {
    public static void Postfix(MainMenu __instance) {
      try {
        Shader[] shaders = UnityEngine.Object.FindObjectsOfType<Shader>();
        Log.TWL(0, "shaders:" + shaders.Length);
        foreach (Shader shader in shaders) {
          Log.WL(1, shader.name + ":" + shader.GetInstanceID());
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(StarSystem))]
  [HarmonyPatch("GetLastPilotAddedToHiringName")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class StarSystem_GetLastPilotAddedToHiringName {
    public static bool Prefix(StarSystem __instance, ref string __result) {
      try {
        if (__instance == null) {
          Log.TWL(0, "Warning call GetLastPilotAddedToHiringName from null star system");
          __result = "ERROR: null star system";
          return false;
        }
        if (__instance.LastPilotAdded == null) {
          Log.TWL(0, "Warning call GetLastPilotAddedToHiringName with null LastPilotAdded. AvailablePilots:"+__instance.AvailablePilots.Count);
          foreach(PilotDef pilot in __instance.AvailablePilots) {
            Log.WL(1, pilot.Description.Id+":"+pilot.Description.Callsign);
          }
          if(__instance.AvailablePilots.Count > 0) {
            __instance.LastPilotAdded = __instance.AvailablePilots.Last();
            return true;
          } else {
            __result = "ERROR: no pilots in roster";
            return false;
          }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("AddPilotToHiringHall")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string) })]
  public static class SimGameState_AddPilotToHiringHall {
    public static bool Prefix(SimGameState __instance, string pilotDefID, string starSystemID, ref bool __result) {
      try {
        Log.TWL(0, "SimGameState.AddPilotToHiringHall "+pilotDefID+" "+starSystemID+" curSystem:"+(__instance.CurSystem==null?"null":__instance.CurSystem.Def.Description.Id));
        if (string.IsNullOrEmpty(pilotDefID) || !__instance.starDict.ContainsKey(starSystemID)) {
          __result = false;
          return false;
        }
        StarSystem system = __instance.starDict[starSystemID];
        if(__instance.DataManager.PilotDefs.TryGet(pilotDefID, out PilotDef def)) {
          Log.WL(1, "pilot " + pilotDefID + ":" + def.Description.Callsign + " already exists in data manager");
          __instance.AddPilotToHiringHall(def, system);
          __result = true;
          return false;
        }
        VersionManifestEntry pilotEntry = __instance.DataManager.ResourceLocator.EntryByID(pilotDefID, BattleTechResourceType.PilotDef);
        if(pilotEntry == null) {
          Log.WL(1,"no pilot entry "+pilotDefID+" in manifest");
          __result = false;
          return false;
        }
        try {
          if (pilotEntry.IsAssetBundled) {
            Log.WL(1, "Pilot "+pilotDefID+" is in asset bundle. Тут мои полномочия все (с)");
            return true;
          }
          Log.WL(1, "Pilot " + pilotDefID + " reading from:" + pilotEntry.FilePath);
          using (StreamReader stream = new StreamReader(pilotEntry.FilePath)) {
            string json = stream.ReadToEnd();
            def = new PilotDef();
            def.FromJSON(json);
            __instance.DataManager.pilotDefs.Add(def.Description.Id, def);
            DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(__instance.DataManager);
            def.GatherDependencies(__instance.DataManager, dependencyLoad, 10u);
            if(dependencyLoad.DependencyCount() > 0) {
              Log.WL(2, "dependencies "+ dependencyLoad.DependencyCount());
              foreach (var request in dependencyLoad.loadRequests) {
                foreach (var dep in request.loadRequests) {
                  Log.WL(3, dep.Key.id);
                }
              }
              dependencyLoad.RegisterLoadCompleteCallback((Action)(() => {
                Log.TWL(0, "SimGameState.AddPilotToHiringHall dependencies "+pilotDefID+" success");
              }));
              __instance.DataManager.InjectDependencyLoader(dependencyLoad, 10U);
            }
            __instance.AddPilotToHiringHall(def, system);
            __result = true;
            return false;
          }
        } catch(Exception e) {
          Log.TWL(0, e.ToString());
          return true;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(StarSystem))]
  [HarmonyPatch("AddAvailablePilot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PilotDef), typeof(bool) })]
  public static class StarSystem_AddAvailablePilot {
    public static PilotDef lastPilotAdded { get; set; } = null;
    public static void Prefix(StarSystem __instance, PilotDef def, bool isRonin) {
      try {
        Log.TWL(0, "StarSystem.AddAvailablePilot " + def.Description.Id + " "+__instance.Def.Description.Id);
        lastPilotAdded = def;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("UpdateMeleePath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class Pathing_UpdateMeleePath_Debug {
    public static bool Prefix(Pathing __instance, bool calledFromUI) {
      try {
        List<AbstractActor> allActors = __instance.Combat.AllActors;
        allActors.Remove(__instance.OwningActor);
        allActors.Remove(__instance.CurrentMeleeTarget);
        PathNode endNode;
        __instance.GetMeleeDestination(__instance.CurrentMeleeTarget, allActors, out endNode, out __instance.ResultDestination, out __instance.ResultAngle);
        __instance.CurrentPath = __instance.CurrentGrid.BuildPathFromEnd(endNode, __instance.MaxCost, endNode.Position, __instance.CurrentMeleeTarget.CurrentPosition, __instance.CurrentMeleeTarget, out __instance.costLeft, out __instance.ResultDestination, out __instance.ResultAngle);
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("UpdateRequests")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class DataManager_UpdateRequests {
    public static bool Prefix(DataManager __instance) {
      try {
        int LightRequestCount = 0;
        int HeavyRequestCount = 0;
        for (int index = 0; index < __instance.activeLoadBatches.Count; ++index) {
          LoadRequest activeLoadBatch = __instance.activeLoadBatches[index];
          if (activeLoadBatch.CurrentState == LoadRequest.State.Processing) {
            LightRequestCount += activeLoadBatch.GetActiveLightRequestCount();
            HeavyRequestCount += activeLoadBatch.GetActiveHeavyRequestCount();
            if (((BattleTech.Data.DataManager.MaxConcurrentLoadsLight <= 0 ? 0 : (LightRequestCount >= BattleTech.Data.DataManager.MaxConcurrentLoadsLight ? 1 : 0)) | (BattleTech.Data.DataManager.MaxConcurrentLoadsHeavy <= 0 ? (false ? 1 : 0) : (HeavyRequestCount >= BattleTech.Data.DataManager.MaxConcurrentLoadsHeavy ? 1 : 0))) != 0) {
              break;
            }
            for (BattleTech.Data.DataManager.FileLoadRequest fileLoadRequest = activeLoadBatch.PopPendingRequest(); fileLoadRequest != null; fileLoadRequest = activeLoadBatch.PopPendingRequest()) {
              if (!fileLoadRequest.ManifestEntryValid) {
                __instance.logger.LogError((object)string.Format("LoadRequest for {0} of type {1} has an invalid manifest entry. Any requests for this object will fail.", (object)fileLoadRequest.ResourceId, (object)fileLoadRequest.ResourceType));
                fileLoadRequest.NotifyLoadFailed();
              } else if (!fileLoadRequest.RequestWeight.RequestAllowed) {
                __instance.logger.LogWarning((object)string.Format("LoadRequest for {0} of type {1} not allowed due to current request weight.", (object)fileLoadRequest.ResourceId, (object)fileLoadRequest.ResourceType));
                fileLoadRequest.SetLoadComplete();
              } else {
                if (fileLoadRequest.IsMemoryRequest) {
                  __instance.RemoveObjectOfType(fileLoadRequest.ResourceId, fileLoadRequest.ResourceType);
                }
                if (fileLoadRequest.RequestWeight.AllowedWeight == 10U) {
                  ++LightRequestCount;
                } else {
                  ++HeavyRequestCount;
                }
                try {
                  fileLoadRequest.Load();
                }catch(Exception e) {
                  __instance.logger.LogWarning((object)string.Format("LoadRequest {0} of type {1} load exception\n{2}", (object)fileLoadRequest.ResourceId, (object)fileLoadRequest.ResourceType, (object)e.ToString()));
                  Log.TWL(0, string.Format("LoadRequest {0} of type {1} load exception", fileLoadRequest.ResourceId, fileLoadRequest.ResourceType));
                  Log.WL(0, e.ToString(), true);
                  fileLoadRequest.NotifyLoadFailed();
                }
              }
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PreferExposedAlonePositionalFactor))]
  [HarmonyPatch("EvaluateInfluenceMapFactorAtPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(float), typeof(MoveType), typeof(PathNode) })]
  public static class PreferExposedAlonePositionalFactor_EvaluateInfluenceMapFactorAtPosition {
    public static PilotDef lastPilotAdded { get; set; } = null;
    public static void Prefix(PreferExposedAlonePositionalFactor __instance, AbstractActor unit, Vector3 position, float angle, MoveType moveType_unused, PathNode pathNode_unused, ref float __result) {
      __result = 0.0f;
      try {
        if (__instance.exposureOK || __instance.exposedTeammateCount > 0) {
          return;
        }
        if(unit == null) {
          Log.TWL(0, "EXCEPTION: PreferExposedAlonePositionalFactor.EvaluateInfluenceMapFactorAtPosition unit is null", true);
          return;
        }
        if (unit.BehaviorTree == null) {
          Log.TWL(0, "EXCEPTION: PreferExposedAlonePositionalFactor.EvaluateInfluenceMapFactorAtPosition "+unit.PilotableActorDef.chassisID+ " has null BehaviorTree",true);
          return;
        }
        Quaternion targetRotation = Quaternion.Euler(0.0f, angle, 0.0f);
        for (int index = 0; index < unit.BehaviorTree.enemyUnits.Count; ++index) {
          try {
            AbstractActor enemyUnit = unit.BehaviorTree.enemyUnits[index] as AbstractActor;
            if (enemyUnit == null) { continue; }
            if (enemyUnit.IsDead) { continue; }
            if (__instance.maxRanges == null) {
              Log.TWL(0, "EXCEPTION:maxRanges is not filled");
              continue;
            }
            if(__instance.isIndirectFireCapable == null) {
              Log.TWL(0, "EXCEPTION:isIndirectFireCapable is not filled");
              continue;
            }
            if(__instance.maxRanges.ContainsKey(enemyUnit) == false) {
              Log.TWL(0, $"EXCEPTION:maxRanges does not contains info on {enemyUnit.PilotableActorDef.ChassisID}");
              continue;
            }
            if (__instance.isIndirectFireCapable.ContainsKey(enemyUnit) == false) {
              Log.TWL(0, $"EXCEPTION:isIndirectFireCapable does not contains info on {enemyUnit.PilotableActorDef.ChassisID}");
              continue;
            }
            if (false == enemyUnit.HasLOFToTargetUnitAtTargetPosition(unit, __instance.maxRanges[enemyUnit], unit.CurrentPosition, Quaternion.LookRotation(position - unit.CurrentPosition), position, targetRotation, __instance.isIndirectFireCapable[enemyUnit])) {
              continue;
            }
            ++__result;
          }catch(Exception e) {
            Log.TWL(0, "I'm just a victim here",true);
            Log.TWL(0, e.ToString(), true);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDAttackModeSelector))]
  [HarmonyPatch("UpdateOverheatWarnings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class CombatHUDHeatDisplay_UpdateOverheatWarnings {
    public static void Prefix(CombatHUDAttackModeSelector __instance, ref bool overHeated, ref bool shutDown) {
      try {
        ICombatant unit = __instance.HUD.SelectedActor;
        if (unit != null) {
          if (unit.isHasHeat() == false) { overHeated = false; shutDown = false; }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDAttackModeSelector))]
  [HarmonyPatch("ShowFireButton")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUDFireButton.FireMode), typeof(string), typeof(bool) })]
  public static class CombatHUDHeatDisplayShowFireButton {
    public static void Postfix(CombatHUDAttackModeSelector __instance, CombatHUDFireButton.FireMode mode, string additionalDetails, bool showHeatWarnings) {
      try {
        if (showHeatWarnings) {
          ICombatant unit = __instance.HUD.SelectedActor;
          if (unit != null) {
            if (unit.isHasHeat() == false) { __instance.showHeatWarnings = false; }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Utilities))]
  [HarmonyPatch("BuildExtensionMethodCacheForType")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Utilities_BuildExtensionMethodCacheForType {
    public static bool Prefix(System.Type type) {
      try {
        if (Utilities.extensionMethodsCache.ContainsKey(type)) { return false; }
        List<MethodInfo> methodInfoList = new List<MethodInfo>();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          try {
            foreach (System.Type type1 in ((IEnumerable<System.Type>)assembly.GetTypes()).Where<System.Type>((Func<System.Type, bool>)(t => t.IsSealed && !t.IsGenericType && !t.IsNested))) {
              foreach (MethodInfo method in type1.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (method.IsDefined(typeof(ExtensionAttribute), false) && method.GetParameters()[0].ParameterType == type)
                  methodInfoList.Add(method);
              }
            }
          }catch(Exception e) {
            Log.TWL(0, "Harmless exception. Just for log:" + assembly.FullName + "\n" + e.ToString(), true);
          }
        }
        Utilities.extensionMethodsCache[type] = methodInfoList;
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("BuildSimGameResults")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameEventResult[]), typeof(GameContext), typeof(SimGameStatDescDef.DescriptionTense?), typeof(Pilot) })]
  public static class Briefing_BuildSimGameResults {
    public static List<ResultDescriptionEntry> BuildSimGameStatsResultsLocal(this SimGameState __instance, SimGameStat[] stats,GameContext context,SimGameStatDescDef.DescriptionTense tense,string prefix = "•") {
      List<ResultDescriptionEntry> descriptionEntryList = new List<ResultDescriptionEntry>();
      foreach (SimGameStat stat in stats) {
        if (!string.IsNullOrEmpty(stat.name) && stat.value != null) {
          SimGameStatDescDef simGameStatDescDef = (SimGameStatDescDef)null;
          GameContext context1 = new GameContext(context);
          if (__instance.DataManager.SimGameStatDescDefs.Exists("SimGameStatDesc_" + stat.name)) {
            simGameStatDescDef = __instance.DataManager.GetStatDescDef(stat);
          } else {
            int length = stat.name.IndexOf('.');
            if (length >= 0) {
              string str1 = stat.name.Substring(0, length);
              if (__instance.DataManager.SimGameStatDescDefs.Exists("SimGameStatDesc_" + str1)) {
                simGameStatDescDef = __instance.DataManager.SimGameStatDescDefs.Get("SimGameStatDesc_" + str1);
                string[] strArray = stat.name.Split('.');
                BattleTechResourceType? nullable = new BattleTechResourceType?();
                object obj = (object)null;
                string id;
                if (strArray.Length < 3) {
                  if (str1 == "Reputation") {
                    id = "faction_" + strArray[1];
                    nullable = new BattleTechResourceType?(BattleTechResourceType.FactionDef);
                  } else {
                    id = (string)null;
                    nullable = new BattleTechResourceType?();
                  }
                } else {
                  string str2 = strArray[1];
                  id = strArray[2];
                  try {
                    nullable = new BattleTechResourceType?((BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), str2));
                  } catch {
                    nullable = new BattleTechResourceType?();
                  }
                }
                if (nullable.HasValue)
                  obj = __instance.DataManager.Get(nullable.Value, id);
                if (obj != null)
                  context1.SetObject(GameContextObjectTagEnum.ResultObject, obj);
                else
                  simGameStatDescDef = (SimGameStatDescDef)null;
              }
            }
          }
          if (simGameStatDescDef != null) {
            if (!simGameStatDescDef.hidden) {
              context1.SetObject(GameContextObjectTagEnum.ResultValue, (object)Mathf.Abs(stat.ToSingle()));
              if (stat.set) {
                string str = !(stat.Type == typeof(bool)) ? Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Set, tense), context1) : (!stat.ToBool() ? Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), context1) : Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), context1));
                if (!string.IsNullOrEmpty(str)) {
                  descriptionEntryList.Add(new ResultDescriptionEntry(new Localize.Text("{0} {1}\n", new object[2]
                  {
                    (object) prefix,
                    (object) str
                  }), context1, stat.name));
                  Log.WL(1, "A - descriptionEntryList:"+str);
                }
              } else if (stat.Type == typeof(int) || stat.Type == typeof(float)) {
                string str = (string)null;
                if ((double)stat.ToSingle() > 0.0)
                  str = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), context1);
                else if ((double)stat.ToSingle() < 0.0)
                  str = Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), context1);
                if (!string.IsNullOrEmpty(str)) {
                  descriptionEntryList.Add(new ResultDescriptionEntry(new Localize.Text("{0} {1}\n", new object[2]
                  {
                    (object) prefix,
                    (object) str
                  }), context1, stat.name));
                  Log.WL(1, "B - descriptionEntryList:" + str);
                }
              }
            }
          } else {
            string tooltipString = __instance.DataManager.GetTooltipString(stat);
            descriptionEntryList.Add(new ResultDescriptionEntry(new Localize.Text("{0} {1} {2}\n", new object[3]
            {
              (object) prefix,
              (object) tooltipString,
              (object) stat.value
            }), context1, stat.name));
            Log.WL(1, "C - descriptionEntryList:" + tooltipString);
          }
        }
      }
      return descriptionEntryList;
    }
    public static List<ResultDescriptionEntry> BuildSimGameActionStringLocal(this SimGameState __instance, SimGameResultAction[] actions,GameContext context,SimGameStatDescDef.DescriptionTense tense,string prefix = "•") {
      List<ResultDescriptionEntry> descriptionEntryList = new List<ResultDescriptionEntry>();
      foreach (SimGameResultAction action in actions) {
        string id1 = "SimGameStatDesc_SimGameResultAction_" + (object)action.Type;
        if (__instance.DataManager.SimGameStatDescDefs.Exists(id1)) {
          SimGameStatDescDef simGameStatDescDef = __instance.DataManager.SimGameStatDescDefs.Get(id1);
          if (simGameStatDescDef != null && !simGameStatDescDef.hidden) {
            GameContext context1 = new GameContext(context);
            context1.SetObject(GameContextObjectTagEnum.ResultValue, (object)action.value);
            if (action.additionalValues != null) {
              for (int index = 0; index < action.additionalValues.Length; ++index) {
                string additionalValue = action.additionalValues[index];
                string id2 = string.Format("faction_{0}", (object)additionalValue);
                string key = additionalValue.StartsWith("starsystemdef_") ? additionalValue : "starsystemdef_" + additionalValue;
                if (__instance.DataManager.Factions.Exists(id2)) {
                  FactionDef factionDef = __instance.DataManager.Factions.Get(id2);
                  context1.SetObject(GameContextObjectTagEnum.ResultFaction, (object)factionDef);
                } else if (__instance.starDict.ContainsKey(key))
                  context1.SetObject(GameContextObjectTagEnum.ResultSystem, (object)__instance.starDict[key]);
              }
            }
            bool result = false;
            string str = !bool.TryParse(action.value, out result) ? Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Set, tense), context1) : (!result ? Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Negative, tense), context1) : Interpolator.Interpolate(simGameStatDescDef.GetResultString(SimGameStatDescDef.DescriptionType.Positive, tense), context1));
            descriptionEntryList.Add(new ResultDescriptionEntry(string.Format("{0} {1}{2}", (object)prefix, (object)str, (object)Environment.NewLine), context1));
          }
        }
      }
      return descriptionEntryList;
    }

    public static List<ResultDescriptionEntry> BuildSimGameResultsLocal(this SimGameState __instance, SimGameEventResult[] resultsList,GameContext context,SimGameStatDescDef.DescriptionTense? tenseOverride,Pilot pilotOverride) {
      Log.TWL(0, "SimGameState.BuildSimGameResults",true);
      List<ResultDescriptionEntry> descriptionEntryList1 = new List<ResultDescriptionEntry>();
      if (resultsList != null) {
        TagDataStructFetcher dataStructFetcher = __instance.Context.GetObject(GameContextObjectTagEnum.TagDataStructFetcher) as TagDataStructFetcher;
        foreach (SimGameEventResult results in resultsList)
        {
          if (!__instance.MeetsRequirements(results.Requirements)) continue;
          GameContext context1 = new GameContext(context);
          TagSet tagSet = (TagSet)null;
          Pilot pilot = (Pilot)null;
          MechDef mechDef = (MechDef)null;
          StarSystem starSystem = (StarSystem)null;
          if (pilotOverride != null) {
            pilot = pilotOverride;
            context1.SetObject(GameContextObjectTagEnum.ResultMechWarrior, (object)pilot);
            tagSet = pilot.pilotDef.PilotTags;
          } else {
            switch (results.Scope) {
              case EventScope.Company:
              tagSet = __instance.companyTags;
              break;
              case EventScope.MechWarrior:
              pilot = context1.GetObject(GameContextObjectTagEnum.TargetMechWarrior) as Pilot;
              context1.SetObject(GameContextObjectTagEnum.ResultMechWarrior, (object)pilot);
              tagSet = pilot.pilotDef.PilotTags;
              break;
              case EventScope.Mech:
              mechDef = context1.GetObject(GameContextObjectTagEnum.TargetUnit) as MechDef;
              context1.SetObject(GameContextObjectTagEnum.ResultMech, (object)mechDef);
              tagSet = mechDef.MechTags;
              break;
              case EventScope.Commander:
              pilot = __instance.Commander;
              context1.SetObject(GameContextObjectTagEnum.ResultMechWarrior, (object)__instance.Commander);
              tagSet = __instance.commander.pilotDef.PilotTags;
              break;
              case EventScope.StarSystem:
              starSystem = context1.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
              context1.SetObject(GameContextObjectTagEnum.ResultSystem, (object)starSystem);
              tagSet = starSystem.Tags;
              break;
              case EventScope.SecondaryMechWarrior:
              pilot = context1.GetObject(GameContextObjectTagEnum.SecondaryMechWarrior) as Pilot;
              context1.SetObject(GameContextObjectTagEnum.ResultMechWarrior, (object)pilot);
              tagSet = pilot.pilotDef.PilotTags;
              break;
              case EventScope.SecondaryMech:
              mechDef = context1.GetObject(GameContextObjectTagEnum.SecondaryUnit) as MechDef;
              context1.SetObject(GameContextObjectTagEnum.ResultMech, (object)mechDef);
              tagSet = mechDef.MechTags;
              break;
              case EventScope.TertiaryMechWarrior:
              pilot = context1.GetObject(GameContextObjectTagEnum.TertiaryMechWarrior) as Pilot;
              context1.SetObject(GameContextObjectTagEnum.ResultMechWarrior, (object)pilot);
              tagSet = pilot.pilotDef.PilotTags;
              break;
              case EventScope.RandomMech:
              mechDef = context1.GetObject(GameContextObjectTagEnum.RandomUnit) as MechDef;
              context1.SetObject(GameContextObjectTagEnum.ResultMech, (object)mechDef);
              tagSet = mechDef.MechTags;
              break;
            }
          }
          if (results.TemporaryResult)
            context1.SetObject(GameContextObjectTagEnum.ResultDuration, (object)results.ResultDuration);
          if (results.AddedTags != null && tagSet != null) {
            List<string> stringList = new List<string>();
            foreach (string addedTag in results.AddedTags) {
              TagDataStruct tagDataStruct = dataStructFetcher.GetItem(addedTag, false);
              if (tagDataStruct != null && tagDataStruct.IsVisible && !string.IsNullOrEmpty(tagDataStruct.FriendlyName))
                stringList.Add(tagDataStruct.ToToolTipString().ToString(true));
            }
            if (stringList.Count > 0) {
              string str1 = pilot == null ? (mechDef == null ? (starSystem == null ? (!tenseOverride.HasValue || tenseOverride.Value != SimGameStatDescDef.DescriptionTense.TemporalEnd ? (!results.TemporaryResult ? Localize.Strings.T(__instance.Constants.Story.TagAddedCompany) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedCompanyTemp, context1)) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedCompanyTempEnd, context1)) : (!tenseOverride.HasValue || tenseOverride.Value != SimGameStatDescDef.DescriptionTense.TemporalEnd ? (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagAddedSystem, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedSystemTemp, context1)) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedSystemTempEnd, context1))) : (!tenseOverride.HasValue || tenseOverride.Value != SimGameStatDescDef.DescriptionTense.TemporalEnd ? (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagAddedMech, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedMechTemp, context1)) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedMechTempEnd, context1))) : (!tenseOverride.HasValue || tenseOverride.Value != SimGameStatDescDef.DescriptionTense.TemporalEnd ? (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagAddedMW, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedMWTemp, context1)) : Interpolator.Interpolate(__instance.Constants.Story.TagAddedMWTempEnd, context1));
              foreach (string str2 in stringList) {
                descriptionEntryList1.Add(new ResultDescriptionEntry(new Localize.Text("{1} {2} {3}{0}", new object[4]
                {
                  (object) Environment.NewLine,
                  (object) "•",
                  (object) str1,
                  (object) str2
                }), context1));
                Log.WL(1, "descriptionEntryList1 '" + str1 + "' '" + str2 + "'");
              }
            }
          }
          if (results.RemovedTags != null && tagSet != null) {
            List<string> stringList = new List<string>();
            foreach (string removedTag in results.RemovedTags) {
              TagDataStruct tagDataStruct = dataStructFetcher.GetItem(removedTag, false);
              if (tagDataStruct != null && tagDataStruct.IsVisible && !string.IsNullOrEmpty(tagDataStruct.FriendlyName))
                stringList.Add(tagDataStruct.ToToolTipString().ToString(true));
            }
            if (stringList.Count > 0) {
              string str1 = pilot == null ? (mechDef == null ? (starSystem == null ? (!results.TemporaryResult ? Localize.Strings.T(__instance.Constants.Story.TagRemovedCompany) : Interpolator.Interpolate(__instance.Constants.Story.TagRemovedCompanyTemp, context1)) : (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagRemovedSystem, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagRemovedSystemTemp, context1))) : (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagRemovedMech, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagRemovedMechTemp, context1))) : (!results.TemporaryResult ? Interpolator.Interpolate(__instance.Constants.Story.TagRemovedMW, context1) : Interpolator.Interpolate(__instance.Constants.Story.TagRemovedMWTemp, context1));
              foreach (string str2 in stringList) {
                descriptionEntryList1.Add(new ResultDescriptionEntry(string.Format("{1} {2} {3}{0}", (object)Environment.NewLine, (object)"•", (object)str1, (object)str2), context1));
              }
            }
          }
          if (results.Stats != null) {
            SimGameStatDescDef.DescriptionTense tense = SimGameStatDescDef.DescriptionTense.Default;
            if (tenseOverride.HasValue)
              tense = tenseOverride.Value;
            else if (results.TemporaryResult)
              tense = SimGameStatDescDef.DescriptionTense.Temporal;
            List<ResultDescriptionEntry> descriptionEntryList2 = __instance.BuildSimGameStatsResultsLocal(results.Stats, context1, tense);
            if (descriptionEntryList2 != null && descriptionEntryList2.Count > 0)
              descriptionEntryList1.AddRange((IEnumerable<ResultDescriptionEntry>)descriptionEntryList2);
          }
          if (results.Actions != null) {
            List<ResultDescriptionEntry> descriptionEntryList2 = __instance.BuildSimGameActionString(results.Actions, context1, SimGameStatDescDef.DescriptionTense.Default);
            if (descriptionEntryList2 != null && descriptionEntryList2.Count > 0)
              descriptionEntryList1.AddRange((IEnumerable<ResultDescriptionEntry>)descriptionEntryList2);
          }
        }
      }
      return descriptionEntryList1;
    }
    public static bool Prefix(SimGameState __instance, SimGameEventResult[] resultsList, GameContext context,SimGameStatDescDef.DescriptionTense? tenseOverride,Pilot pilotOverride, ref List<ResultDescriptionEntry> __result) {
      try {
        __result = __instance.BuildSimGameResultsLocal(resultsList, context, tenseOverride, pilotOverride);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  public static class MechInitGameRepHelper {
    public static void InitGameRepLocal(this Mech __instance,Transform parentTransform) {
      try {
        if (__instance == null) { Log.TWL(0, "Mech.InitGameRepLocal mech is null");  return;  }
        Log.TWL(0, "Mech.InitGameRepLocal "+__instance.PilotableActorDef.Description.Id);
        string prefabIdentifier = __instance.MechDef.Chassis.PrefabIdentifier;
        GameObject gameObject = __instance.Combat.DataManager.PooledInstantiate(prefabIdentifier, BattleTechResourceType.Prefab);
        __instance._gameRep = (GameRepresentation)gameObject.GetComponent<MechRepresentation>();
        gameObject.GetComponent<Animator>().enabled = true;
        __instance.GameRep.Init(__instance, parentTransform, false);
        if ((UnityEngine.Object)parentTransform == (UnityEngine.Object)null) {
          gameObject.transform.position = __instance.currentPosition;
          gameObject.transform.rotation = __instance.currentRotation;
        }
        List<string> usedPrefabNames = new List<string>();
        foreach (MechComponent allComponent in __instance.allComponents) {
          if (allComponent.componentType != ComponentType.Weapon) {
            allComponent.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.MechDef.Chassis.HardpointDataDef, allComponent.baseComponentRef, __instance.MechDef.Chassis.PrefabBase, allComponent.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
            allComponent.baseComponentRef.hasPrefabName = true;
            if (!string.IsNullOrEmpty(allComponent.baseComponentRef.prefabName)) {
              Transform attachTransform = __instance.GetAttachTransform(allComponent.mechComponentRef.MountedLocation);
              allComponent.InitGameRep(allComponent.baseComponentRef.prefabName, attachTransform, __instance.LogDisplayName);
              __instance.GameRep.miscComponentReps.Add(allComponent.componentRep);
            }
          }
        }
        foreach (Weapon weapon in __instance.Weapons) {
          weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, __instance.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
          weapon.baseComponentRef.hasPrefabName = true;
          if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
            Transform attachTransform = __instance.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
            weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, __instance.LogDisplayName);
            __instance.GameRep.weaponReps.Add(weapon.weaponRep);
            string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(__instance.MechDef, weapon.mechComponentRef);
            if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
              WeaponRepresentation component = __instance.Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab).GetComponent<WeaponRepresentation>();
              component.Init((ICombatant)__instance, attachTransform, true, __instance.LogDisplayName, weapon.Location);
              __instance.GameRep.weaponReps.Add(component);
            }
          }
        }
        foreach (MechComponent supportComponent in __instance.supportComponents) {
          if (supportComponent is Weapon weapon) {
            weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, __instance.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
            weapon.baseComponentRef.hasPrefabName = true;
            if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
              Transform attachTransform = __instance.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
              weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, __instance.LogDisplayName);
              __instance.GameRep.miscComponentReps.Add((ComponentRepresentation)weapon.weaponRep);
            }
          }
        }
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
        if (!__instance.MeleeWeapon.baseComponentRef.hasPrefabName) {
          __instance.MeleeWeapon.baseComponentRef.prefabName = "chrPrfWeap_generic_melee";
          __instance.MeleeWeapon.baseComponentRef.hasPrefabName = true;
        }
        __instance.MeleeWeapon.InitGameRep(__instance.MeleeWeapon.baseComponentRef.prefabName, __instance.GetAttachTransform(__instance.MeleeWeapon.mechComponentRef.MountedLocation), __instance.LogDisplayName);
        if (!__instance.DFAWeapon.mechComponentRef.hasPrefabName) {
          __instance.DFAWeapon.mechComponentRef.prefabName = "chrPrfWeap_generic_melee";
          __instance.DFAWeapon.mechComponentRef.hasPrefabName = true;
        }
        __instance.DFAWeapon.InitGameRep(__instance.DFAWeapon.mechComponentRef.prefabName, __instance.GetAttachTransform(__instance.DFAWeapon.mechComponentRef.MountedLocation), __instance.LogDisplayName);
        bool flag1 = __instance.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
        bool flag2 = __instance.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
        if (flag1 | flag2) {
          SkinnedMeshRenderer[] componentsInChildren = __instance.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
          for (int index = 0; index < componentsInChildren.Length; ++index) {
            if (flag1)
              componentsInChildren[index].sharedMaterial = __instance.Combat.DataManager.TextureManager.PlaceholderUnfinishedMaterial;
            if (flag2)
              componentsInChildren[index].sharedMaterial = __instance.Combat.DataManager.TextureManager.PlaceholderImpostorMaterial;
          }
        }
        __instance.GameRep.RefreshEdgeCache();
        __instance.GameRep.FadeIn(1f);
        Log.WL(1, "GameRep inited successfully");
        if (__instance.IsDead || !__instance.Combat.IsLoadingFromSave)
          return;
        if (__instance.AuraComponents != null) {
          foreach (MechComponent auraComponent in __instance.AuraComponents) {
            for (int index = 0; index < auraComponent.componentDef.statusEffects.Length; ++index) {
              if (auraComponent.componentDef.statusEffects[index].targetingData.auraEffectType == AuraEffectType.ECM_GHOST) {
                __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                __instance.GameRep.PlayVFXAt(__instance.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
                return;
              }
            }
          }
        }
        if (__instance.VFXDataFromLoad != null) {
          foreach (VFXEffect.StoredVFXEffectData storedVfxEffectData in __instance.VFXDataFromLoad)
            __instance.GameRep.PlayVFXAt(__instance.GameRep.GetVFXTransform(storedVfxEffectData.hitLocation), storedVfxEffectData.hitPos, storedVfxEffectData.vfxName, storedVfxEffectData.isAttached, storedVfxEffectData.lookatPos, storedVfxEffectData.isOneShot, storedVfxEffectData.duration);
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }

    }
  }
  public static class Core{
    public static System.Type[] GetTypesSafe(this Assembly assembly) {
      try {
        return assembly.GetTypes();
      } catch (ReflectionTypeLoadException e) {
        return e.Types.Where(x => x != null).ToArray();
      }
    }
    private static HashSet<string> PooledInstantiate_Fallback_tracked = new HashSet<string>();
    public static void ClearFallbackTracked() {
      Log.TWL(0, "ClearFallbackTracked");
      PooledInstantiate_Fallback_tracked.Clear();
    }
    public static void HideAll(this CombatHUDActorInfo __instance) {
      __instance.SetGOActive((MonoBehaviour)__instance.DetailsDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.PhaseDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.InspiredDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.StabilityDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.HeatDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.MarkDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.StateStack, false);
      __instance.SetGOActive((MonoBehaviour)__instance.NameDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.LogoDisplay, false);
      __instance.SetGOActive((MonoBehaviour)__instance.ArmorBar, false);
      __instance.SetGOActive((MonoBehaviour)__instance.StructureBar, false);
      __instance.SetGOActive((MonoBehaviour)__instance.ExplosiveDisplay, false);
      __instance.gameObject.SetActive(false);
    }
    public static Func<UnitSpawnPointGameLogic, MechDef, PilotDef, Team, Lance, HeraldryDef, AbstractActor> SpawnMech_internal = null;
    public static bool UnitSpawnPointGameLogic_Spawn(UnitSpawnPointGameLogic __instance, bool spawnOffScreen) {
      try {
        if (!__instance.HasUnitToSpawn) { return false; }
        Log.TWL(0, "UnitSpawnPointGameLogic_Spawn "+ __instance.unitType + " mech:"+ __instance.mechDefId + " pilot:"+ __instance.pilotDefId);
        Team turnActorByUniqueId = __instance.Combat.TurnDirector.GetTurnActorByUniqueId(__instance.teamDefinitionGuid) as Team;
        if (__instance.teamDefinitionGuid == "421027ec-8480-4cc6-bf01-369f84a22012" || turnActorByUniqueId == null) {
          UnitSpawnPointGameLogic.logger.LogError((object)string.Format("Invalid teamIndex for SpawnPoint [{0}][{1}] - TeamIndex[{2}]", (object)__instance.name, (object)__instance.encounterObjectGuid, (object)__instance.teamDefinitionGuid));
        } else {
          PilotDef t1 = (PilotDef)null;
          HeraldryDef t2 = (HeraldryDef)null;
          if (!string.IsNullOrEmpty(__instance.customHeraldryDefId) && !__instance.Combat.DataManager.Heraldries.TryGet(__instance.customHeraldryDefId, out t2))
            __instance.LogError("Invalid custom heraldry id: " + __instance.customHeraldryDefId);
          if (__instance.pilotDefOverride != null) {
            t1 = __instance.pilotDefOverride;
          } else {
            string id = string.IsNullOrEmpty(__instance.pilotDefId) ? UnitSpawnPointGameLogic.PilotDef_Default : __instance.pilotDefId;
            if (!__instance.Combat.DataManager.PilotDefs.TryGet(id, out t1)) {
              __instance.LogError(string.Format("PilotDef [{0}] not previously requested. Falling back to [{1}]", (object)id, (object)UnitSpawnPointGameLogic.PilotDef_Default), true);
              t1 = __instance.Combat.DataManager.PilotDefs.Get(UnitSpawnPointGameLogic.PilotDef_Default);
            }
          }
          Lance lanceByUid = turnActorByUniqueId.GetLanceByUID(__instance.lanceGuid);
          string empty = string.Empty;
          string chassisId;
          AbstractActor unit;
          switch (__instance.unitType) {
            case UnitType.Mech:
            MechDef t3 = (MechDef)null;
            if (__instance.mechDefOverride != null) {
              t3 = __instance.mechDefOverride;
            } else {
              if (!__instance.Combat.DataManager.MechDefs.TryGet(__instance.mechDefId, out t3)) {
                __instance.LogError(string.Format("MechDef [{0}] not previously requested. Aborting unit spawn.", (object)__instance.mechDefId), true);
                return false;
              }
              if (!t3.DependenciesLoaded(1000U)) {
                UnitSpawnPointGameLogic.logger.LogError((object)string.Format("Invalid mechdef for SpawnPoint [{0}][{1}] - [{2}]", (object)__instance.name, (object)__instance.encounterObjectGuid, (object)__instance.mechDefId), (UnityEngine.Object)__instance.gameObject);
                return false;
              }
              t3.Refresh();
            }
            chassisId = t3.ChassisID;
            Log.WL(1, "SpawnMech_internal:"+(SpawnMech_internal == null?"null":"not null"));
            if (SpawnMech_internal != null) {
              unit = (AbstractActor)SpawnMech_internal(__instance, t3, t1, turnActorByUniqueId, lanceByUid, t2);
            } else {
              unit = (AbstractActor)__instance.SpawnMech(t3, t1, turnActorByUniqueId, lanceByUid, t2);
            }
            break;
            case UnitType.Vehicle:
            VehicleDef t4 = (VehicleDef)null;
            if (__instance.vehicleDefOverride != null) {
              t4 = __instance.vehicleDefOverride;
            } else {
              if (!__instance.Combat.DataManager.VehicleDefs.TryGet(__instance.vehicleDefId, out t4)) {
                __instance.LogError(string.Format("VehicleDef [{0}] not previously requested. Aborting unit spawn.", (object)__instance.vehicleDefId), true);
                return false;
              }
              t4.Refresh();
            }
            chassisId = t4.ChassisID;
            unit = (AbstractActor)__instance.SpawnVehicle(t4, t1, turnActorByUniqueId, lanceByUid, t2);
            break;
            case UnitType.Turret:
            TurretDef t5 = (TurretDef)null;
            if (__instance.turretDefOverride != null) {
              t5 = __instance.turretDefOverride;
            } else {
              if (!__instance.Combat.DataManager.TurretDefs.TryGet(__instance.turretDefId, out t5)) {
                __instance.LogError(string.Format("TurretDef [{0}] not previously requested. Aborting unit spawn.", (object)__instance.turretDefId), true);
                return false;
              }
              t5.Refresh();
            }
            chassisId = t5.ChassisID;
            unit = (AbstractActor)__instance.SpawnTurret(t5, t1, turnActorByUniqueId, lanceByUid, t2);
            break;
            default:
            throw new ArgumentException("UnitSpawnPointGameLocic.SpawnUnit had invalid unitType: " + __instance.unitType.ToString());
          }
          Log.WL(1,"unit:"+(unit == null?"null":"not null"));
          __instance.LogMessage(string.Format("Spawning unit - Team [{0}], UnitId [{1}], ChassisId [{2}], PilotId [{3}]", (object)turnActorByUniqueId.DisplayName, (object)unit.GUID, (object)chassisId, (object)__instance.pilotDefId));
          unit.OverriddenPilotDisplayName = __instance.customUnitName;
          __instance.lastSpawnedUnit = (ICombatant)unit;
          if (spawnOffScreen) {
            unit.PlaceFarAwayFromMap();
            __instance.spawningOffScreen = true;
            __instance.timePlacedOffScreen = 0.0f;
          }
          if ((!(turnActorByUniqueId is AITeam aiTeam) ? 0 : (aiTeam.ThinksOnThisMachine ? 1 : 0)) != 0) {
            if (!Enum.IsDefined(typeof(BehaviorTreeIDEnum), (object)(int)__instance.behaviorTree))
              __instance.behaviorTree = BehaviorTreeIDEnum.CoreAITree;
            unit.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(__instance.Combat.BattleTechGame, unit, __instance.behaviorTree);
            for (int i = 0; i < __instance.aiOrderList.Count; ++i)
              unit.IssueAIOrder(__instance.aiOrderList[i]);
          }
          for (int index = 0; index < __instance.spawnEffectTags.Count; ++index)
            unit.CreateSpawnEffectByTag(__instance.spawnEffectTags[index]);
          UnitSpawnedMessage unitSpawnedMessage = new UnitSpawnedMessage(__instance.encounterObjectGuid, unit.GUID);
          EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)unitSpawnedMessage);
          if (!__instance.triggerInterruptPhaseOnSpawn)
            return false;
          unit.IsInterruptActor = true;
          __instance.Combat.StackManager.InsertInterruptPhase(turnActorByUniqueId.GUID, unitSpawnedMessage.messageIndex);
          return false;
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }

    public delegate void d_AbstractActor_InitStats(AbstractActor instance);
    private static d_AbstractActor_InitStats i_AbstractActor_InitStats = null;
    public static void Init_AbstractActor_InitStats() {
      {
        MethodInfo method = typeof(AbstractActor).GetMethod("InitStats", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CDDEBUG_AbstractActor_InitStats", null, new Type[] { typeof(AbstractActor) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AbstractActor_InitStats = (d_AbstractActor_InitStats)dm.CreateDelegate(typeof(d_AbstractActor_InitStats));
      }
      return;
    }

    public static bool Mech_InitStats_Prefix(Mech __instance) {
      try {
        Log.TWL(0, " Mech.InitStats "+__instance.PilotableActorDef.ChassisID);
        if (!__instance.Combat.IsLoadingFromSave) {
          if (i_AbstractActor_InitStats == null) { Init_AbstractActor_InitStats(); }
          i_AbstractActor_InitStats((AbstractActor)__instance);
          switch (__instance.MechDef.Chassis.weightClass) {
            case WeightClass.LIGHT:
            __instance.Initiative = __instance.Combat.Constants.Phase.PhaseLight;
            break;
            case WeightClass.MEDIUM:
            __instance.Initiative = __instance.Combat.Constants.Phase.PhaseMedium;
            break;
            case WeightClass.HEAVY:
            __instance.Initiative = __instance.Combat.Constants.Phase.PhaseHeavy;
            break;
            case WeightClass.ASSAULT:
            __instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault;
            break;
          }
          __instance.statCollection.AddStatistic<int>("BaseInitiative", __instance.Initiative);
          __instance.statCollection.AddStatistic<int>("TurnRadius", __instance.MechDef.Chassis.TurnRadius);
          __instance.statCollection.AddStatistic<int>("MaxJumpjets", __instance.MechDef.Chassis.MaxJumpjets);
          __instance.StatCollection.AddStatistic<float>("SpotterDistanceMultiplier", __instance.MechDef.Chassis.SpotterDistanceMultiplier);
          __instance.StatCollection.AddStatistic<float>("SpotterDistanceAbsolute", 0.0f);
          __instance.StatCollection.AddStatistic<float>("SpottingVisibilityMultiplier", __instance.MechDef.Chassis.VisibilityMultiplier);
          __instance.StatCollection.AddStatistic<float>("SpottingVisibilityAbsolute", 0.0f);
          __instance.StatCollection.AddStatistic<float>("SensorDistanceMultiplier", __instance.MechDef.Chassis.SensorRangeMultiplier);
          __instance.StatCollection.AddStatistic<float>("SensorDistanceAbsolute", 0.0f);
          __instance.StatCollection.AddStatistic<float>("SensorSignatureModifier", __instance.MechDef.Chassis.Signature);
          __instance.statCollection.AddStatistic<float>("MinStability", 0.0f);
          __instance.statCollection.AddStatistic<float>("MaxStability", __instance.MechDef.Chassis.Stability);
          __instance.StatCollection.AddStatistic<float>("UnsteadyThreshold", __instance.Combat.Constants.ResolutionConstants.DefaultUnsteadyThreshold);
          __instance.StatCollection.AddStatistic<int>("MaxHeat", __instance.Combat.Constants.Heat.MaxHeat);
          __instance.StatCollection.AddStatistic<int>("OverheatLevel", (int)((double)__instance.Combat.Constants.Heat.MaxHeat * (double)__instance.Combat.Constants.Heat.OverheatLevel));
          __instance.statCollection.AddStatistic<int>("MinHeatNextActivation", -1);
          __instance.statCollection.AddStatistic<int>("HeatSinkCapacity", 0);
          __instance.StatCollection.AddStatistic<bool>("IgnoreHeatToHitPenalties", false);
          __instance.StatCollection.AddStatistic<bool>("IgnoreHeatMovementPenalties", false);
          __instance.statCollection.AddStatistic<int>("EndMoveHeat", 0);
          __instance.statCollection.AddStatistic<float>("WalkSpeed", __instance.MovementCaps.MaxWalkDistance);
          __instance.statCollection.AddStatistic<float>("RunSpeed", __instance.MovementCaps.MaxSprintDistance);
          __instance.statCollection.AddStatistic<float>("EngageRangeModifier", __instance.MechDef.Chassis.EngageRangeModifier);
          __instance.statCollection.AddStatistic<float>("DFASelfDamage", __instance.MechDef.Chassis.DFASelfDamage);
          __instance.statCollection.AddStatistic<bool>("DFACausesSelfUnsteady", true);
          __instance.statCollection.AddStatistic<int>("EvasivePipsGainedAdditional", 0);
          __instance.statCollection.AddStatistic<int>("MeleeHitPushBackPhases", 0);
          __instance.statCollection.AddStatistic<bool>("HeadShotImmunity", false);
          __instance.statCollection.AddStatistic<int>("CurrentHeat", 0);
          __instance.statCollection.AddStatistic<float>("Stability", 0.0f);
          __instance.statCollection.AddStatistic<bool>("IsProne", false);
          string[] names = Enum.GetNames(typeof(InstabilitySource));
          int length = names.Length;
          for (int index = 0; index < length; ++index)
            __instance.statCollection.AddStatistic<float>(string.Format("StabilityDefense.{0}", (object)names[index]), __instance.MechDef.Chassis.StabilityDefenses[index]);
          __instance.statCollection.AddStatistic<float>("ReceivedInstabilityMultiplier", 1f);
          if (AbstractActor.initLogger.IsLogEnabled)
            AbstractActor.initLogger.Log((object)(__instance.DisplayName + " applying armor/structure multiplier: " + (object)__instance.ArmorMultiplier + "/" + (object)__instance.StructureMultiplier));
          __instance.statCollection.AddStatistic<float>("Head.Armor", __instance.MechDef.Head.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("Head.Structure", __instance.MechDef.Head.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("Head.DamageLevel", __instance.MechDef.Head.DamageLevel);
          __instance.statCollection.AddStatistic<float>("CenterTorso.Armor", __instance.MechDef.CenterTorso.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("CenterTorso.RearArmor", __instance.MechDef.CenterTorso.AssignedRearArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("CenterTorso.Structure", __instance.MechDef.CenterTorso.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("CenterTorso.DamageLevel", __instance.MechDef.CenterTorso.DamageLevel);
          __instance.statCollection.AddStatistic<float>("LeftTorso.Armor", __instance.MechDef.LeftTorso.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("LeftTorso.RearArmor", __instance.MechDef.LeftTorso.AssignedRearArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("LeftTorso.Structure", __instance.MechDef.LeftTorso.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("LeftTorso.DamageLevel", __instance.MechDef.LeftTorso.DamageLevel);
          __instance.statCollection.AddStatistic<float>("RightTorso.Armor", __instance.MechDef.RightTorso.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("RightTorso.RearArmor", __instance.MechDef.RightTorso.AssignedRearArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("RightTorso.Structure", __instance.MechDef.RightTorso.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("RightTorso.DamageLevel", __instance.MechDef.RightTorso.DamageLevel);
          __instance.statCollection.AddStatistic<float>("LeftArm.Armor", __instance.MechDef.LeftArm.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("LeftArm.Structure", __instance.MechDef.LeftArm.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("LeftArm.DamageLevel", __instance.MechDef.LeftArm.DamageLevel);
          __instance.statCollection.AddStatistic<float>("RightArm.Armor", __instance.MechDef.RightArm.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("RightArm.Structure", __instance.MechDef.RightArm.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("RightArm.DamageLevel", __instance.MechDef.RightArm.DamageLevel);
          __instance.statCollection.AddStatistic<float>("LeftLeg.Armor", __instance.MechDef.LeftLeg.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("LeftLeg.Structure", __instance.MechDef.LeftLeg.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("LeftLeg.DamageLevel", __instance.MechDef.LeftLeg.DamageLevel);
          __instance.statCollection.AddStatistic<float>("RightLeg.Armor", __instance.MechDef.RightLeg.AssignedArmor * __instance.ArmorMultiplier);
          __instance.statCollection.AddStatistic<float>("RightLeg.Structure", __instance.MechDef.RightLeg.CurrentInternalStructure * __instance.StructureMultiplier);
          __instance.statCollection.AddStatistic<LocationDamageLevel>("RightLeg.DamageLevel", __instance.MechDef.RightLeg.DamageLevel);
          __instance.InitEffectStats();
          foreach (MechComponent allComponent in __instance.allComponents) {
            if (allComponent == null) {
              Log.WL(1,"!!!!WARNING ONE OF COMPONENT IS NULL!!!!");
              continue;
            }
            allComponent.Init();
            allComponent.InitStats();
          }
          if (__instance.MeleeWeapon == null) {
            Log.WL(1, "!!!!WARNING MeleeWeapon IS NULL!!!!");
          } else {
            __instance.MeleeWeapon.Init();
            __instance.MeleeWeapon.InitStats();
          }
          if (__instance.MeleeWeapon == null) {
            Log.WL(1, "!!!!WARNING DFAWeapon IS NULL!!!!");
          } else {
            __instance.DFAWeapon.Init();
            __instance.DFAWeapon.InitStats();
          }
          __instance.ImaginaryLaserWeapon.Init();
          __instance.AssignAmmoToWeapons();
          __instance.CalcAndSetAlphaStrikesRemaining();
          __instance.StartingStructure = __instance.SummaryStructureCurrent;
          __instance.StartingArmor = __instance.SummaryArmorCurrent;
          foreach (MechComponent allComponent in __instance.allComponents)
            allComponent.InitPassiveSelfEffects();
        } else {
          foreach (MechComponent allComponent in __instance.allComponents)
            allComponent.UpdateToAuraIfNeeded();
        }
        __instance.HighestLOSPosition = Vector3.zero;
        float mechScaleMultiplier = __instance.Combat.Constants.CombatValueMultipliers.TEST_MechScaleMultiplier;
        __instance.originalLOSSourcePositions = new Vector3[__instance.MechDef.Chassis.LOSSourcePositions.Length];
        __instance.losSourcePositions = new Vector3[__instance.MechDef.Chassis.LOSSourcePositions.Length];
        for (int index = 0; index < __instance.losSourcePositions.Length; ++index) {
          FakeVector3 losSourcePosition = __instance.MechDef.Chassis.LOSSourcePositions[index];
          __instance.originalLOSSourcePositions[index] = new Vector3(losSourcePosition.x * mechScaleMultiplier, losSourcePosition.y * mechScaleMultiplier, losSourcePosition.z * mechScaleMultiplier);
          __instance.LOSSourcePositions[index] = __instance.originalLOSSourcePositions[index];
          if ((double)__instance.originalLOSSourcePositions[index].y > (double)__instance.HighestLOSPosition.y)
            __instance.HighestLOSPosition.y = __instance.originalLOSSourcePositions[index].y;
        }
        __instance.originalLOSTargetPositions = new Vector3[__instance.MechDef.Chassis.LOSTargetPositions.Length];
        __instance.losTargetPositions = new Vector3[__instance.MechDef.Chassis.LOSTargetPositions.Length];
        for (int index = 0; index < __instance.losTargetPositions.Length; ++index) {
          FakeVector3 losTargetPosition = __instance.MechDef.Chassis.LOSTargetPositions[index];
          __instance.originalLOSTargetPositions[index] = new Vector3(losTargetPosition.x * mechScaleMultiplier, losTargetPosition.y * mechScaleMultiplier, losTargetPosition.z * mechScaleMultiplier);
          __instance.losTargetPositions[index] = __instance.originalLOSTargetPositions[index];
        }
        __instance.UpdateLOSPositions();
        __instance.pilot.Init(__instance.Combat, (AbstractActor)__instance);
        bool ModifyStats = !__instance.Combat.IsLoadingFromSave;
        __instance.pilot.InitAbilities(ModifyStats, __instance.Combat.IsLoadingFromSave);
        __instance.InitAbilities(ModifyStats);
        __instance.AuraAbilities.AddRange((IEnumerable<Ability>)__instance.pilot.AuraAbilities);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }

    public static void PooledInstantiate_Fallback(this DataManager __instance,ref GameObject __result,string id,BattleTechResourceType resourceType,Vector3? position,Quaternion? rotation,Transform parent) {
      return;
      if (__result != null) { return; }
      if (resourceType != BattleTechResourceType.Prefab) { return; }
      if (BattleTech.UnityGameInstance.BattleTechGame == null) { return; }
      if (BattleTech.UnityGameInstance.BattleTechGame.Combat == null) { return; }
      if (PooledInstantiate_Fallback_tracked.Contains(id)) { return; }
      Log.TWL(0, "PooledInstantiate_Fallback: " + id + " result:" + (__result == null ? "null" : "not null"));
      PooledInstantiate_Fallback_tracked.Add(id);
      try {
        VersionManifestEntry entry = __instance.ResourceLocator.EntryByID(id, BattleTechResourceType.Prefab);
        if (entry == null) { Log.WL(1, "entry not found in manifest"); return; }
        if (string.IsNullOrEmpty(entry.AssetBundleName)) { Log.WL(1, "entry not asset bundled"); return; }
        VersionManifestEntry bundleEntry = __instance.ResourceLocator.EntryByID(entry.AssetBundleName, BattleTechResourceType.AssetBundle);
        if (entry == null) { Log.WL(1, "AssetBundle " + entry.AssetBundleName + " not found in manifest"); return; }
        if (__instance.AssetBundleManager.IsBundleLoaded(entry.AssetBundleName)) {
          Log.WL(1, "AssetBundle " + entry.AssetBundleName + " is already loaded");
          return;
        }
        AssetBundle bundle = AssetBundle.LoadFromFile(bundleEntry.FilePath);
        if (bundle == null) {
          Log.WL(1, "AssetBundle " + bundleEntry.FilePath + " fail to load");
          return;
        }
        AssetBundleTracker tracker = new AssetBundleTracker(bundle, false);
        tracker.ClearObjectMap();
        foreach (UnityEngine.Object allAsset in tracker.assetBundle.LoadAllAssets()) {
          System.Type type = allAsset.GetType();
          Dictionary<string, UnityEngine.Object> dictionary;
          if (!tracker.loadedObjects.TryGetValue(type, out dictionary)) {
            dictionary = new Dictionary<string, UnityEngine.Object>();
            tracker.loadedObjects[type] = dictionary;
          }
          if (!dictionary.ContainsKey(allAsset.name))
            dictionary.Add(allAsset.name, allAsset);
        }
        tracker.CurrentState = AssetBundleTracker.State.Ready;
        __instance.AssetBundleManager.loadedBundles.Add(entry.AssetBundleName, tracker);
        Log.WL(1, "AssetBundle " + bundleEntry.Name + " loaded. Request " + id + " again");
        __result = __instance.PooledInstantiate(id, BattleTechResourceType.Prefab, position, rotation, parent);
        Log.WL(1, "result:" + (__result == null ? "null" : "not null"));
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void FinishLoading() {
      Core.HarmonyInstance.Patch(typeof(DataManager).GetMethod("PooledInstantiate", BindingFlags.Public | BindingFlags.Instance),null, new HarmonyMethod(typeof(CustomDeploy.Core).GetMethod(nameof(CustomDeploy.Core.PooledInstantiate_Fallback), BindingFlags.Static | BindingFlags.Public)));
      //Core.HarmonyInstance.Patch(typeof(UnitSpawnPointGameLogic).GetMethod("Spawn", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(CustomDeploy.Core).GetMethod(nameof(CustomDeploy.Core.UnitSpawnPointGameLogic_Spawn), BindingFlags.Static | BindingFlags.Public)));
    }
    public static string BaseDir { get; set; }
    public static bool debugLog { get; set; }
    public static Harmony HarmonyInstance = null;
    public static void Init(string directory,bool debugLog) {
      Log.BaseDirectory = directory;
      Core.debugLog = debugLog;
      Log.InitLog();
      Core.BaseDir = directory;
      Log.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      try {
        HarmonyInstance = new Harmony("io.mission.customdeploy");
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}
 