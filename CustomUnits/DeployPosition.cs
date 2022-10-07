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
using BattleTech.Designed;
using BattleTech.Framework;
using BattleTech.Rendering;
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using FogOfWar;
using Harmony;
using HBS;
using HBS.Collections;
using InControl;
using Localize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("StartFirstRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_StartFirstRound {
    public static void Postfix(TurnDirector __instance) {
      try {
        if (DeployManualHelper.deployDirector != null) { return; }
        if (Core.Settings.DeployAutoSpawnProtection) {
          Log.TWL(0,$"TurnDirector.StartFirstRound add spawn protection");
          foreach(AbstractActor unit in __instance.Combat.AllActors) {
            Log.WL(1, $"{unit.PilotableActorDef.ChassisID}");
            unit.addSpawnProtection("first round");
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("OnActorSelected")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUD_OnActorSelected {
    public static void Postfix(CombatHUD __instance, AbstractActor actor) {
      try {
        if (DeployManualHelper.NeedSpawnProtection == false) { return; }
        if (actor.IsDeployDirector()) { return; }
        if (actor.IsAvailableThisPhase == false) { return; }
        DeployManualHelper.NeedSpawnProtection = false;
        foreach (AbstractActor unit in actor.Combat.AllActors) {
          if (unit.IsDeployDirector()) { continue; }
          unit.addSpawnProtection("manual delayed spawn protection");
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(TimerObjective))]
  [HarmonyPatch("ContractInitialize")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TimerObjective_ContractInitialize {
    public static void Postfix(TimerObjective __instance) {
      try {
        bool manualSpawn = __instance.Combat.ActiveContract.isManualSpawn() || (DeployManualHelper.deployDirector != null);
        Log.TWL(0, $"TimerObjective.ContractInitialize GUID:{__instance.GUID} contractType:{__instance.Combat.ActiveContract.ContractTypeValue.Name} ManualDeploy:{manualSpawn}");
        if (Core.Settings.timerObjectiveChange.TryGetValue(__instance.Combat.ActiveContract.ContractTypeValue.Name, out var change)) {
          Log.W(1, $"changing duration:{__instance.durationRemaining}/{__instance.durationToCount}");
          __instance.durationRemaining += manualSpawn ? change.manualDeployAdvice : change.autoDeployAdvice;
          __instance.durationToCount += manualSpawn ? change.manualDeployAdvice : change.autoDeployAdvice;
          Log.WL(0, $"->{__instance.durationRemaining}/{__instance.durationToCount}");
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }

  public class DropShipManager : MonoBehaviour {
    public GameObject DropOffPoint { get; set; }
    private DropshipGameLogic _leopardInstance;
    public CombatGameState Combat { get; set; } = null;
    public string encounterObjectGuid { get; set; }
    public string teamDefinitionGuid { get; set; }
    public DropshipAnimationStyle dropshipAnimationStyle { get; set; }
    public Action DropOffCompleete { get; set; }
    private bool DropAnimationCompleete { get; set; } = true;
    public void OnDropshipAnimationComplete(MessageCenterMessage message) {
      Log.TWL(0, "DropShipManager.OnDropshipAnimationComplete is already called:"+ DropAnimationCompleete);
      if (DropAnimationCompleete == true) { return; }
      DropAnimationCompleete = true;
      MessageCenter messageCenter = this.Combat.MessageCenter;
      messageCenter.Subscribe(MessageCenterMessageType.OnDropshipAnimationComplete, new ReceiveMessageCenterMessage(this.OnDropshipAnimationComplete), false);
      DropOffCompleete?.Invoke();
    }
    public void Init(PlayerLanceSpawnerGameLogic __instance) {
      this.Combat = __instance.Combat;
      if (this.DropOffPoint == null) {
        this.DropOffPoint = new GameObject("DropShipManager_DropoffPoint"); 
      }
      this.encounterObjectGuid = __instance.encounterObjectGuid;
      this.teamDefinitionGuid = __instance.teamDefinitionGuid;
      this.dropshipAnimationStyle = __instance.dropshipAnimationStyle;
      DropOffPoint.transform.SetParent(__instance.gameObject.transform);
      DropOffPoint.transform.position = __instance.transform.position;
      DropOffPoint.transform.rotation = __instance.transform.rotation;
    }
    public void DropOff(List<DeployPosition> positions, Action callback) {
      try {
        Vector3? centerPoint = null;
        int count = 0;
        foreach (DeployPosition pos in positions) {
          if (pos.position.HasValue == false) { continue; }
          if (centerPoint == null) { centerPoint = pos.position.Value; count = 1; continue; }
          Vector3 tmp = centerPoint.Value;
          tmp.x += pos.position.Value.x;
          tmp.z += pos.position.Value.z;
          ++count;
          centerPoint = tmp;
        }
        if (centerPoint != null) {
          Vector3 tmp = centerPoint.Value;
          tmp.x = tmp.x / count;
          tmp.z = tmp.z / count;
          tmp = Combat.HexGrid.GetClosestPointOnGrid(tmp);
          this.DropOffPoint.transform.position = tmp;
        }
        this.DropOffCompleete = callback;
        DropAnimationCompleete = false;
        MessageCenter messageCenter = this.Combat.MessageCenter;
        messageCenter.Subscribe(MessageCenterMessageType.OnDropshipAnimationComplete, new ReceiveMessageCenterMessage(this.OnDropshipAnimationComplete), true);
        EnsureLeopardDropship();
        Log.TWL(0, "LeopardInstance.StartDropoff");
        LeopardInstance.transform.SetParent(this.DropOffPoint.transform);
        LeopardInstance.transform.localPosition = Vector3.zero;
        LeopardInstance.transform.localRotation = Quaternion.identity;
        LeopardInstance.transform.localScale = Vector3.one;
        LeopardInstance.StartDropoff();
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        callback?.Invoke();
      }
    }
    private void EnsureLeopardDropship() {
      if (this.LeopardInstance != null) { return; }
      this.LeopardInstance = DropshipGameLogic.SpawnDropshipForFlyby(this.Combat, this.DropOffPoint.transform, this.encounterObjectGuid, LanceSpawnerGameLogic.GetDropshipGuid(this.encounterObjectGuid), false, this.teamDefinitionGuid, this.dropshipAnimationStyle);
    }
    private DropshipGameLogic LeopardInstance {
      get {
        if ((UnityEngine.Object)this._leopardInstance != (UnityEngine.Object)null)
          return this._leopardInstance;
        this._leopardInstance = (DropshipGameLogic)this.Combat.ItemRegistry.GetItemByGUID(LanceSpawnerGameLogic.GetDropshipGuid(this.encounterObjectGuid));
        return this._leopardInstance;
      }
      set => this._leopardInstance = value;
    }
  }
  public class DropPodPoint : MonoBehaviour {
    public AbstractActor unit { get; set; } = null;
    public DeployManualDropPodManager parent { get; set; } = null;
    public bool InDroping { get; set; } = false;
    public ParticleSystem dropPodVfxPrefab { get; set; }
    public GameObject dropPodLandedPrefab { get; set; }
    public Vector3 offscreenDropPodPosition { get; set; } = Vector3.zero;
    public Action onDropCompleete { get; set; }
    public void HotDrop(Action onDropEnd, float initalDelay) {
      if (this.unit == null) { return; }
      if (this.InDroping) { return; }
      this.InDroping = true;
      this.onDropCompleete = onDropEnd;
      this.StartCoroutine(this.StartDropPodAnimation(initalDelay, onDropCompleete));
    }
    public void LoadDropPodPrefabs(ParticleSystem dropPodVfxPrefab, GameObject dropPodLandedPrefab) {
      if (dropPodVfxPrefab != null) {
        this.dropPodVfxPrefab = UnityEngine.Object.Instantiate<ParticleSystem>(dropPodVfxPrefab, this.transform);
        this.dropPodVfxPrefab.transform.position = this.transform.position;
        this.dropPodVfxPrefab.Pause();
        this.dropPodVfxPrefab.Clear();
      }
      if (dropPodLandedPrefab != null) {
        this.dropPodLandedPrefab = UnityEngine.Object.Instantiate<GameObject>(dropPodLandedPrefab, this.offscreenDropPodPosition, Quaternion.identity);
      }
    }
    public IEnumerator StartDropPodAnimation(float initialDelay, Action unitDropPodAnimationComplete) {
      while (!EncounterLayerParent.encounterBegan)
        yield return (object)null;
      yield return (object)new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.75f) + initialDelay);
      int num1 = (int)WwiseManager.PostEvent<AudioEventList_play>(AudioEventList_play.play_dropPod_projectile, WwiseManager.GlobalAudioObject);
      if (this.dropPodVfxPrefab != null) {
        this.dropPodVfxPrefab.transform.position = this.transform.position;
        this.dropPodVfxPrefab.Simulate(0.0f);
        this.dropPodVfxPrefab.Play();
      } else {
        Log.TWL(0, "Null drop pod animation for this biome.");
      }
      yield return (object)new WaitForSeconds(1f);
      int num2 = (int)WwiseManager.PostEvent<AudioEventList_play>(AudioEventList_play.play_dropPod_impact, WwiseManager.GlobalAudioObject);
      yield return (object)this.DestroyFlimsyObjects();
      //yield return (object)this.ApplyDropPodDamageToSquashedUnits(sequenceGUID, rootSequenceGUID);
      yield return (object)new WaitForSeconds(3f);
      this.TeleportUnitToSpawnPoint();
      yield return (object)new WaitForSeconds(2f);
      this.InDroping = false;
      unitDropPodAnimationComplete();
    }
    public void TeleportUnitToSpawnPoint() {
      Vector3 spawnPosition = this.gameObject.transform.position;
      if (this.dropPodLandedPrefab != null) {
        this.dropPodLandedPrefab.transform.position = spawnPosition;
        this.dropPodLandedPrefab.transform.rotation = this.transform.rotation;
      }
      this.unit.TeleportActor(spawnPosition);
      Vector3 nearestEnemy = this.parent.nearestEnemy(spawnPosition);
      if (nearestEnemy != Vector3.zero) {
        Vector3 forwardNearestEnemy = (nearestEnemy - spawnPosition).normalized;
        Quaternion facingNearestEnemy = Quaternion.LookRotation(forwardNearestEnemy);
        facingNearestEnemy = Quaternion.Euler(0f, facingNearestEnemy.eulerAngles.y, 0f);
        this.unit.OnPositionUpdate(spawnPosition, facingNearestEnemy, -1, true, new List<DesignMaskDef>());
        this.unit.GameRep.transform.rotation = facingNearestEnemy;
        if (this.unit.GameRep is CustomMechRepresentation custRep) {
          custRep.UpdateRotation(new RaycastHit?(),this.unit.GameRep.transform, this.unit.GameRep.transform.forward, 9999f);
        } else {
          if(this.unit is Vehicle vehicle) {
            ActorMovementSequence.AlignVehicleToGround(unit.GameRep.transform, 9999f);
          }
        }
      } else {
        this.unit.OnPositionUpdate(spawnPosition, unit.CurrentRotation, -1, true, new List<DesignMaskDef>());
      }

      this.unit.GameRep.FadeIn(1f);
      this.unit.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
      if (this.unit.GameRep is PilotableActorRepresentation rep) { rep.RefreshSurfaceType(true); }
      this.unit.ReapplyDesignMasks();
    }
    public IEnumerator DestroyFlimsyObjects() {
      Collider[] hits = Physics.OverlapSphere(this.gameObject.transform.position, 36f, -5, QueryTriggerInteraction.Ignore);
      float impactMagnitude = 3f * this.parent.Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
      for (int i = 0; i < hits.Length; ++i) {
        Collider collider = hits[i];
        Vector3 normalized = (collider.transform.position - this.gameObject.transform.position).normalized;
        DestructibleObject component1 = collider.gameObject.GetComponent<DestructibleObject>();
        DestructibleUrbanFlimsy component2 = collider.gameObject.GetComponent<DestructibleUrbanFlimsy>();
        if (component1 != null && component1.isFlimsy) {
          component1.TakeDamage(this.gameObject.transform.position, normalized, impactMagnitude);
          component1.Collapse(normalized, impactMagnitude);
        }
        if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
          component2.PlayDestruction(normalized, impactMagnitude);
        if (i % 10 == 0)
          yield return (object)null;
      }
      yield return (object)null;
    }
  }
  public class DeployManualDropPodManager : MonoBehaviour {
    public EncounterLayerParent parent { get; set; } = null;
    public CombatGameState Combat { get; set; } = null;
    public CombatHUD HUD { get; set; } = null;
    public List<DropPodPoint> dropPoints { get; set; } = new List<DropPodPoint>();
    private List<Vector3> enemiesPositions { get; set; } = new List<Vector3>();
    public Action OnAllDropCompleete { get; set; } = null;
    public void UpdateDropPodPrefabs() {
      DropPodPoint[] drops = this.gameObject.GetComponentsInChildren<DropPodPoint>(true);
      foreach (DropPodPoint drop in drops) {
        drop.LoadDropPodPrefabs(parent.DropPodVfxPrefab, parent.dropPodLandedPrefab);
      }
    }
    public static Vector3 nearestEnemy(List<Vector3> enemiesPositions, Vector3 pos) {
      Vector3 nearest = pos;
      float nearest_dist = 0f;
      if (enemiesPositions.Count == 0) { return Vector3.zero; }
      foreach (Vector3 epos in enemiesPositions) {
        float dist = Vector3.Distance(pos, epos);
        if ((nearest_dist == 0f) || (nearest_dist > dist)) { nearest = epos; nearest_dist = dist; }
      }
      if (nearest == pos) { return Vector3.zero; }
      return nearest;
    }
    public Vector3 nearestEnemy(Vector3 pos) {
      return DeployManualDropPodManager.nearestEnemy(enemiesPositions, pos);
    }
    public void Init(CombatGameState Combat, CombatHUD HUD, EncounterLayerParent parent, List<DeployPosition> positions) {
      this.Combat = Combat;
      this.HUD = HUD;
      this.parent = parent;
      this.enemiesPositions = Combat.enemiesPositions();
      DropPodPoint[] drops = this.gameObject.GetComponentsInChildren<DropPodPoint>(true);
      for (int t = 0; t < drops.Length; ++t) {
        drops[t].unit = null;
      }
      for (int t = drops.Length; t < positions.Count; ++t) {
        GameObject drop = new GameObject("hotdropPoint" + t.ToString(), typeof(DropPodPoint));
        drop.transform.SetParent(this.gameObject.transform);
      }
      drops = this.gameObject.GetComponentsInChildren<DropPodPoint>(true);
      dropPoints.Clear();
      for (int t = 0; t < positions.Count; ++t) {
        dropPoints.Add(drops[t]);
        drops[t].InDroping = false;
        drops[t].parent = this;
        drops[t].unit = positions[t].unit;
        drops[t].transform.position = positions[t].position.Value;
      }
      UpdateDropPodPrefabs();
    }
    public void OnDropCompleete() {
      bool isAnyDropInProgress = false;
      foreach (DropPodPoint dropPoint in this.dropPoints) {
        if (dropPoint.unit == null) { continue; }
        if (dropPoint.InDroping) { isAnyDropInProgress = true; break; }
      }
      if (isAnyDropInProgress == false) { OnAllDropCompleete?.Invoke(); }
    }
    public void HotDrop(Action OnAllDropCompleete) {
      this.OnAllDropCompleete = OnAllDropCompleete;
      float initalDelay = 0f;
      foreach (DropPodPoint dropPoint in this.dropPoints) {
        dropPoint.HotDrop(this.OnDropCompleete, initalDelay);
        initalDelay += 0.5f;
      }
    }
  }
  public static class DeployManualHelper {
    public static readonly string DeployPilotDefID = "pilot_deploy_director";
    public static readonly string DeployAbilityDefID = "AbilityDefCU_DeploySetPosition";
    public static string DeployMechDefID { get { return CACConstants.DeployMechDefID; } }
    private static bool NeedCheckForDependenciesLoaded { get; set; } = true;
    public static bool IsInManualSpawnSequence { get { return CACCombatState.IsInDeployManualState; } set { CACCombatState.IsInDeployManualState = value; } }
    public static SpawnUnitMethodType originalSpawnMethod { get; set; } = SpawnUnitMethodType.InstantlyAtSpawnPoint;
    public static AbstractActor deployDirector { get; set; } = null;
    public static void Clean() { deployDirector = null; }
    public static bool NeedSpawnProtection { get; set; } = false;
    public static void Deploy(CombatHUD HUD, List<DeployPosition> positions) {
      if(originalSpawnMethod == SpawnUnitMethodType.ViaLeopardDropship) {
        DeployDropShip(HUD, positions);
      } else if(originalSpawnMethod == SpawnUnitMethodType.DropPod) {
        DeployDropPods(HUD, positions);
      } else {
        TeleportToPositions(HUD, positions);
      }
    }
    public static void DeployDropShip(CombatHUD HUD, List<DeployPosition> positions) {
      EncounterLayerParent encounterLayerParent = HUD.Combat.EncounterLayerData.gameObject.GetComponentInParent<EncounterLayerParent>();
      DropShipManager dropManager = encounterLayerParent.gameObject.GetComponent<DropShipManager>();
      if (dropManager == null) {
        DeployDropPods(HUD, positions);
        return;
      }
      dropManager.DropOff(positions, (Action)(() => {
        TeleportToPositions(HUD,positions);
      }));
    }
    public static void DeployDropPods(CombatHUD HUD, List<DeployPosition> positions) {
      EncounterLayerParent encounterLayerParent = HUD.Combat.EncounterLayerData.gameObject.GetComponentInParent<EncounterLayerParent>();
      DeployManualDropPodManager hotdropManager = encounterLayerParent.gameObject.GetComponent<DeployManualDropPodManager>();
      if (hotdropManager == null) { hotdropManager = encounterLayerParent.gameObject.AddComponent<DeployManualDropPodManager>(); }
      hotdropManager.Init(HUD.Combat, HUD, encounterLayerParent, positions);
      IsInManualSpawnSequence = false;
      hotdropManager.HotDrop((Action)(()=> {
        RestoreVisiblityState(HUD);
      }));
    }
    public static Dictionary<ICombatant, VisibilityLevel> GetHighestPlayerVisibilityLevel(this CombatGameState combat) {
      Dictionary<ICombatant, VisibilityLevel> result = new Dictionary<ICombatant, VisibilityLevel>();
      Team localPlayerTeam = combat.LocalPlayerTeam;
      List<ICombatant> allCombatants = combat.GetAllImporantCombatants();
      foreach (var target in allCombatants) {
        if (target is AbstractActor actor) {
          if(actor.team == null) {
            result.Add(target, VisibilityLevel.None);
            continue;
          }
          if (actor.team.IsFriendly(localPlayerTeam)) {
            result.Add(target, VisibilityLevel.LOSFull);
            continue;
          }
        }
        result.Add(target, localPlayerTeam.VisibilityToTarget(target));
      }
      return result;
    }
    public static void RestoreVisiblityState(CombatHUD HUD) {
      try {
        Log.TWL(0, "RestoreVisiblityState");
        HUD.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(deployDirector.DoneWithActor()));
        FogOfWarSystem_WipeToValue.NormalFoW();
        LazySingletonBehavior<FogOfWarView>.Instance.FowSystem.WipeToValue(HUD.Combat.EncounterLayerData.startingFogOfWarVisibility);
        HUD.MechWarriorTray.RefreshTeam(HUD.Combat.LocalPlayerTeam);
        DeployDirectorDespawner despawner = HUD.gameObject.GetComponent<DeployDirectorDespawner>();
        if (despawner == null) {
          despawner = HUD.gameObject.AddComponent<DeployDirectorDespawner>();
          despawner.Init(deployDirector.Combat, HUD);
        };
        despawner.deployDirector = deployDirector;
        List<AbstractActor> actors = HUD.Combat.GetAllLivingActors();
        List<ICombatant> allLivingCombatants = HUD.Combat.GetAllImporantCombatants();
        foreach (AbstractActor actor in actors) {
          try {
            if (actor == deployDirector) { continue; }
            Log.WL(1,actor.PilotableActorDef.ChassisID);
            CustomMechMeshMerge[] custMerges = actor.GameRep.gameObject.GetComponentsInChildren<CustomMechMeshMerge>(true);
            //MechMeshMerge[] merges = actor.GameRep.gameObject.GetComponentsInChildren<MechMeshMerge>(true);
            Log.WL(1, "CustomMechMeshMerge:"+ custMerges.Length);
            //Log.WL(1, "MechMeshMerge:" + merges.Length);
            //if ((custMerges.Length != 0)||(merges.Length != 0)) {
            //  actor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
            //}
            if (custMerges.Length != 0) {
              foreach (CustomMechMeshMerge merge in custMerges) {
                Log.WL(3, "CustomMechMeshMerge.RefreshCombinedMesh:" + merge.gameObject.name);
                merge.RefreshCombinedMesh(true);
              }
            }
            //if (merges.Length != 0) {
            //  foreach (MechMeshMerge merge in merges) {
            //    Log.WL(3, "MechMeshMerge.RefreshCombinedMesh:" + merge.gameObject.name);
            //    merge.RefreshCombinedMesh(true);
            //  }
            //}
            //if (actor.team == null) { actor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull); continue; }
            //if (actor.team.IsFriendly(HUD.Combat.LocalPlayerTeam)) {
            //  actor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
            //} else {
            //  actor.OnPlayerVisibilityChanged(VisibilityLevel.None);
            //}
            actor.VisibilityCache.RebuildCache(allLivingCombatants);
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
        List<Team> teams = HUD.Combat.Teams;
        foreach (Team team in teams) {
          try {
            team.VisibilityCache.RebuildCache(allLivingCombatants);
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
        Dictionary<ICombatant, VisibilityLevel> visibility = HUD.Combat.GetHighestPlayerVisibilityLevel();
        Log.TWL(0, "Refresh visibility "+ visibility.Count);
        foreach (var visLevel in visibility) {
          Log.WL(1, "Refresh visibility " + (visLevel.Key.PilotableActorDef == null ? visLevel.Key.DisplayName : visLevel.Key.PilotableActorDef.ChassisID) + " " + visLevel.Value);
          if (visLevel.Key is AbstractActor actor) {
            actor.OnPlayerVisibilityChanged(visLevel.Value);
          }else if (visLevel.Key.GameRep != null) { visLevel.Key.GameRep.OnPlayerVisibilityChanged(visLevel.Value); }
        }
        CombatHUDActorInfo[] actorInfos = UIManager.Instance.InWorldRoot.gameObject.GetComponentsInChildren<CombatHUDActorInfo>(true);
        foreach (CombatHUDActorInfo actorInfo in actorInfos) {
          if (actorInfo.gameObject.activeSelf) { continue; }
          if (actorInfo.DisplayedCombatant == deployDirector) { continue; }
          actorInfo.gameObject.SetActive(true);
          Traverse.Create(actorInfo).Method("RefreshAllInfo").GetValue();
        }
        foreach (AbstractActor unit in HUD.Combat.LocalPlayerTeam.units) {
          if (unit.IsDeployDirector()) { continue; }
          if (unit.IsAvailableThisPhase) {
            //if (Core.Settings.DeployManualSpawnProtection) {
            //  unit.addAddSpawnProtection("manual deploy");
            //}
            HUD.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(unit.DoneWithActor()));
          }
        }
        if (Core.Settings.DeployManualSpawnProtection) {
          NeedSpawnProtection = true;
          foreach (AbstractActor unit in HUD.Combat.AllActors) {
            if (unit.IsDeployDirector()) { continue; }
            //if (unit.isAddSpawnProtected()) { continue; }
            unit.addSpawnProtection(2,"manual deploy");
          }
        }
        //HUD.Combat.TurnDirector.StartFirstRound();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }

    }

    public static void TeleportToPositions(CombatHUD HUD, List<DeployPosition> positions) {
      try {
        IsInManualSpawnSequence = false;
        List<Vector3> enemiesPositions = HUD.Combat.enemiesPositions();
        Log.TWL(0, "DeployManualHelper.TeleportToPositions enemies:"+ enemiesPositions.Count);
        foreach (DeployPosition dPos in positions) {
          if (dPos.position.HasValue) {
            if (dPos.unit == null) { continue; }
            dPos.unit?.TeleportActor(dPos.position.Value);
            Vector3 nearestEnemy = DeployManualDropPodManager.nearestEnemy(enemiesPositions, dPos.position.Value);
            Log.WL(1, "position:"+ dPos.position.Value+" nearest enemy:"+ nearestEnemy);
            if (nearestEnemy != Vector3.zero) {
              Vector3 forwardNearestEnemy = (nearestEnemy - dPos.position.Value).normalized;
              Quaternion facingNearestEnemy = Quaternion.LookRotation(forwardNearestEnemy);
              Log.WL(2, "forward to enemy:" + forwardNearestEnemy);
              facingNearestEnemy = Quaternion.Euler(0f, facingNearestEnemy.eulerAngles.y, 0f);
              Log.WL(2, "facing to enemy:"+ facingNearestEnemy.eulerAngles+" facing original:"+dPos.unit.CurrentRotation.eulerAngles);
              dPos.unit.OnPositionUpdate(dPos.position.Value, facingNearestEnemy, -1, true, new List<DesignMaskDef>());
              dPos.unit.GameRep.transform.rotation = facingNearestEnemy;
              if (dPos.unit?.GameRep is CustomMechRepresentation custRep) {
                custRep.UpdateRotation(new RaycastHit?(),dPos.unit.GameRep.transform, dPos.unit.GameRep.transform.forward, 9999f);
              } else {
                if (dPos.unit is Vehicle) {
                  ActorMovementSequence.AlignVehicleToGround(dPos.unit?.GameRep.transform, 9999f);
                }
              }
            } else {
              dPos.unit?.OnPositionUpdate(dPos.position.Value, dPos.unit.CurrentRotation, -1, true, new List<DesignMaskDef>());
            }
            dPos.unit?.OnPositionUpdate(dPos.position.Value, dPos.unit.CurrentRotation, -1, true, new List<DesignMaskDef>());
            //dPos.unit?.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
            if(dPos.unit != null) {
              if (dPos.unit.GameRep is PilotableActorRepresentation rep) { rep.RefreshSurfaceType(true); }
            }
            dPos.unit?.ReapplyDesignMasks();
          }
        };
        RestoreVisiblityState(HUD);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void RebuildAllVisChaches(this CombatGameState combat) {
      List<AbstractActor> actors = combat.GetAllLivingActors();
      List<Team> teams = combat.Teams;
      List<ICombatant> allCombatants = combat.GetAllImporantCombatants();
      foreach (AbstractActor actor in actors) {
        actor.VisibilityCache.RebuildCache(allCombatants);
        if (IsInManualSpawnSequence) {
          actor.OnPlayerVisibilityChanged(VisibilityLevel.None);
        }
      }
      foreach (Team team in teams) {
        team.VisibilityCache.RebuildCache(allCombatants);
      }
    }
    public static List<Vector3> enemiesPositions(this CombatGameState combat) {
      List<Vector3> result = new List<Vector3>();
      foreach (AbstractActor unit in combat.AllEnemies) {
        result.Add(unit.CurrentPosition);
      }
      OccupyRegionObjective[] objectives = combat.EncounterLayerData.gameObject.GetComponentsInChildren<OccupyRegionObjective>();
      foreach (var objective in objectives) {
        Vector3 pos = objective.transform.position;
        pos.y = combat.MapMetaData.GetLerpedHeightAt(pos);
        result.Add(pos);
      }
      return result;
    }
    public static void SpawnDeployDirector(PlayerLanceSpawnerGameLogic __instance) {
      try {
        Team team = __instance.Combat.TurnDirector.GetTurnActorByUniqueId(__instance.teamDefinitionGuid) as Team;
        Lance lance = team.GetLanceByUID(__instance.LanceGuid);
        MechDef mechDef = __instance.Combat.DataManager.MechDefs.Get(DeployMechDefID);
        PilotDef pilotDef = __instance.Combat.DataManager.PilotDefs.Get(DeployPilotDefID);

        AbstractActor deployDirector = ActorFactory.CreateMech(mechDef, pilotDef, team.EncounterTags, UnityGameInstance.BattleTechGame.Combat, Guid.NewGuid().ToString(), __instance.encounterObjectGuid, team.HeraldryDef);
        deployDirector.Init(__instance.Position, 0f, true);
        deployDirector.InitGameRep((Transform)null);
        team.AddUnit(deployDirector);
        lance.AddUnitGUID(deployDirector.GUID);
        deployDirector.AddToLance(lance);
        deployDirector.OnPlayerVisibilityChanged(VisibilityLevel.None);
        UnitSpawnedMessage unitSpawnedMessage = new UnitSpawnedMessage(__instance.encounterObjectGuid, deployDirector.GUID);
        deployDirector.OnPositionUpdate(__instance.Position, Quaternion.identity, -1, true, (List<DesignMaskDef>)null, false);
        deployDirector.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(deployDirector.Combat.BattleTechGame, deployDirector, BehaviorTreeIDEnum.DoNothingTree);
        UnityGameInstance.BattleTechGame.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new UnitSpawnedMessage("DEPLOY_DIRECTOR_SPAWNER", deployDirector.GUID));
        Log.WL(1, "DeployManualHelper.SpawnDeployDirector:" + deployDirector.PilotableActorDef.Description.Id + ":" + deployDirector.GetPilot().Description.Id + " " + deployDirector.CurrentPosition);
        __instance.Combat.ItemRegistry.AddItem(deployDirector);
        __instance.Combat.RebuildAllLists();
        DeployManualHelper.deployDirector = deployDirector;
        CombatHUD HUD = UIManager.Instance.gameObject.GetComponentInChildren<CombatHUD>(true);
        HUD?.MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static bool CheckDefinitiosDeps(GameInstance __instance, Contract contract, string playerGUID) {
      try {
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(contract.DataManager);
        Log.TWL(0, "DeployManualHelper.CheckDefinitiosDeps");
        if (contract.DataManager.MechDefs.TryGet(DeployManualHelper.DeployMechDefID, out MechDef mechDef)) {
          if (mechDef.DependenciesLoaded(1000u) == false) {
            Log.WL(1, "mechdef deps");
            mechDef.GatherDependencies(contract.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          }
        }
        if (contract.DataManager.AbilityDefs.TryGet(DeployManualHelper.DeployAbilityDefID, out AbilityDef abilityDef)) {
          if (abilityDef.DependenciesLoaded(1000u) == false) {
            Log.WL(1, "ability deps");
            abilityDef.GatherDependencies(contract.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          }
        }
        if (contract.DataManager.PilotDefs.TryGet(DeployManualHelper.DeployPilotDefID, out PilotDef pilotDef)) {
          if (pilotDef.DependenciesLoaded(1000u) == false) {
            Log.WL(1, "pilot deps");
            pilotDef.GatherDependencies(contract.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          }
        }
        if (dependencyLoad.DependencyCount() > 0) {
          dependencyLoad.RegisterLoadCompleteCallback((Action)(() => {
            Log.WL(1, "DeployManualHelper.LoadCompleteCallback", true);
            DeployManualHelper.NeedCheckForDependenciesLoaded = false;
            __instance.LaunchContract(contract, playerGUID);
          }));
          contract.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static bool CheckForDeps(GameInstance __instance, Contract contract, string playerGUID) {
      bool needLoadRequest = false;
      Log.TWL(0, "DeployManualHelper.CheckForDeps");
      if (DeployManualHelper.NeedCheckForDependenciesLoaded == false) {
        DeployManualHelper.NeedCheckForDependenciesLoaded = true;
        return true;
      }
      try {
        LoadRequest request = contract.DataManager.CreateLoadRequest((Action<LoadRequest>)((LoadRequest loadRequest) => {
          Log.TWL(0, "DeployManualHelper.CheckForDeps base definitions loaded");
          CheckDefinitiosDeps(__instance, contract, playerGUID);
        }), false);
        if (contract.DataManager.PilotDefs.Exists(DeployManualHelper.DeployPilotDefID) == false) {
          request.AddLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, DeployManualHelper.DeployPilotDefID, null);
          Log.WL(1, "request:" + DeployManualHelper.DeployPilotDefID);
          needLoadRequest = true;
        }
        if (contract.DataManager.AbilityDefs.Exists(DeployManualHelper.DeployAbilityDefID) == false) {
          request.AddLoadRequest<PilotDef>(BattleTechResourceType.AbilityDef, DeployManualHelper.DeployAbilityDefID, null);
          Log.WL(1, "request:" + DeployManualHelper.DeployAbilityDefID);
          needLoadRequest = true;
        }
        if (contract.DataManager.MechDefs.Exists(DeployManualHelper.DeployMechDefID) == false) {
          request.AddLoadRequest<PilotDef>(BattleTechResourceType.MechDef, DeployManualHelper.DeployMechDefID, null);
          Log.WL(1, "request:" + DeployManualHelper.DeployMechDefID);
          needLoadRequest = true;
        }
        //{
        //  LoadRequest rotundaRequest = contract.DataManager.CreateLoadRequest((Action<LoadRequest>)((LoadRequest loadRequest) => {
        //    Log.TWL(0, "DeployManualHelper.CheckForDeps vehicledef_ROTUNDA_LRM loaded success");
        //  }), false);
        //  rotundaRequest.AddLoadRequest<VehicleDef>(BattleTechResourceType.VehicleDef, "vehicledef_ROTUNDA_LRM", null);
        //  rotundaRequest.ProcessRequests(1000u);
        //}
        if (needLoadRequest) {
          request.ProcessRequests(1000u);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return CheckDefinitiosDeps(__instance, contract, playerGUID);
    }
  }
  public class DeployDirectorDespawner : MonoBehaviour {
    public CombatGameState combat { get; set; }
    public CombatHUD HUD { get; set; }
    private AbstractActor f_deployDirector;
    public AbstractActor deployDirector { get { return f_deployDirector; } set { f_deployDirector = value; HasDeployDirector = value != null; } }
    public bool HasDeployDirector { get; set; } = false;
    public void Init(CombatGameState combat, CombatHUD HUD) {
      this.combat = combat;
      this.HUD = HUD;
    }
    public void LateUpdate() {
      //Log.TWL(0, "DeployDirectorDespawner combat:"+(this.combat != null)+" HUD:"+(this.HUD != null)+" deployDirector:"+(this.deployDirector != null) + " deployLoadRequest:"+ (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest!=null));
      if (this.combat == null) { return; }
      if (this.HUD == null) { return; }
      if (this.deployDirector == null) { return; }
      if (DeployManualHelper.IsInManualSpawnSequence) { return; }
      //if (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest == null) { return; }
      if (this.deployDirector.IsDead) { return; }
      if (this.deployDirector.IsFlaggedForDeath) { return; }
      try {
        Log.TWL(0, "Deploy director is ready for testing for despawn. IsAvailableThisPhase:" + this.deployDirector.IsAvailableThisPhase);
        if (this.HUD.SelectedActor != this.deployDirector) {
          this.HUD.SelectionHandler.DeselectActor(this.HUD.SelectedActor);
        }
        if (this.deployDirector.IsAvailableThisPhase) {
          this.combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(this.deployDirector.DoneWithActor()));
          Log.WL(1, "Done with actor");
        }
        EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(this.deployDirector.spawnerGUID, this.deployDirector.GUID, DeathMethod.DespawnedNoMessage));
        Log.WL(1, "Despawn");
        this.deployDirector = null;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class CombatHUDDeployAutoActivator : MonoBehaviour {
    public CombatHUDActionButton button { get; set; }
    public CombatHUD HUD { get; set; }
    public CombatHUDDeployAutoActivator() {
      button = null;
    }
    public void Init(CombatHUDActionButton parent, CombatHUD HUD) {
      this.button = parent;
      this.HUD = HUD;
    }
    public void Update() {
      if (button == null) { return; }
      if (HUD == null) { return; }
      if (button.Ability == null) { return; }
      if (button.IsActive) { return; }
      if (HUD.SelectedActor == null) { return; }
      if (HUD.SelectedActor.IsDead) { return; }
      if (HUD.SelectedActor.IsFlaggedForDeath) { return; }
      if (HUD.SelectedActor.IsDeployDirector() == false) { return; }
      button.OnClick();
    }
  }
  public class TextedCircle : MonoBehaviour {
    public GameObject circle { get; set; }
    public TextMeshPro text { get; set; }
    public CombatHUD HUD { get; set; }
    public void Init(CombatHUD HUD) {
      this.HUD = HUD;
      circle = GameObject.Instantiate(CombatTargetingReticle.Instance.Circles[0]);
      circle.SetActive(true);
      circle.transform.SetParent(this.transform);
      Vector3 localScale = circle.transform.localScale;
      localScale.x = 1f * 2f;
      localScale.z = 1f * 2f;
      circle.transform.localScale = localScale;
      circle.transform.localPosition = Vector3.zero;
      GameObject textGO = new GameObject("Text");
      textGO.transform.SetParent(this.transform);
      text = textGO.AddComponent<TextMeshPro>();
      text.font = HUD.SidePanel.WarningText.font;
      text.fontSize = Core.Settings.DeployLabelFontSize;
      text.autoSizeTextContainer = true;
      text.alignment = TextAlignmentOptions.Center;
      textGO.transform.localPosition = Vector3.up * Core.Settings.DeployLabelHeight;
      //text.SetText("!TEXT!");
    }
    public void UpdatePOS(Vector3 pos, float radius, string t) {
      Vector3 localScale = circle.transform.localScale;
      localScale.x = radius * 2f;
      localScale.z = radius * 2f;
      circle.transform.localScale = localScale;
      this.transform.position = pos;
      if (this.text.text != t) { this.text.SetText(t); }
    }
  }
  public static class TargetingCirclesHelper {
    private static List<TextedCircle> circles = new List<TextedCircle>();
    public static GameObject rootReticle = null;
    public static CombatHUD HUD = null;
    public static void Clear() {
      circles.Clear();
      GameObject.Destroy(rootReticle); rootReticle = null;
      HUD = null;
    }
    public static void InitCircles(int count, CombatHUD iHUD) {
      if (rootReticle == null) { rootReticle = new GameObject("TargetingCircles"); }
      if (count <= circles.Count) { return; }
      TargetingCirclesHelper.HUD = iHUD;
      while (circles.Count < count) {
        GameObject circleGO = new GameObject("Circle");
        TextedCircle circle = circleGO.AddComponent<TextedCircle>();
        circle.Init(HUD);
        circles.Add(circle);
      }
    }
    public static void HideAllCircles() {
      foreach (TextedCircle circle in circles) { circle.gameObject.SetActive(false); }
    }
    public static void HideRoot() {
      rootReticle.SetActive(false);
    }
    public static void ShowRoot() {
      rootReticle.SetActive(true);
    }
    public static void ShowCirclesCount(int count) {
      for (int t = 0; t < circles.Count; ++t) {
        if (t >= count) { if (circles[t].gameObject.activeSelf) { circles[t].gameObject.SetActive(false); } } else {
          if (circles[t].gameObject.activeSelf == false) { circles[t].gameObject.SetActive(true); }
        }
      }
    }
    public static void UpdateCircle(int index, Vector3 pos, float radius, string text) {
      if (index < 0) { return; }
      if (index >= circles.Count) { InitCircles(index + 1, HUD); };
      if (index >= circles.Count) { return; }
      //if (rootReticle.activeSelf == false) { rootReticle.SetActive(true); }
      //if(circles[index].activeSelf == false) { circles[index].SetActive(true); }
      circles[index].UpdatePOS(pos, radius, text);
    }
  }
  public class DeployPosition {
    //public PilotableActorDef def { get; set; }
    //public UnitSpawnPointGameLogic spawnPoint { get; set; }
    public int lanceid { get; set; }
    public int posid { get; set; }
    public AbstractActor unit { get; set; }
    public Vector3? position { get; set; }
    //public DeployPosition(AbstractActor u) { this.unit = u; position = null; }
  }
  public class SelectionStateCommandDeploy : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated = false;
    //private List<Vector3> targetPositions;
    //public float CircleRange = 50f;
    public Vector3 goodPos;
    public SelectionStateCommandDeploy(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
      HasActivated = false;
    }
#if BT_PUBLIC_ASSEMBLY
    public override bool ShouldShowWeaponsUI { get { return false; } }
    public override bool ShouldShowTargetingLines { get { return false; } }
    public override bool ShouldShowFiringArcs { get { return false; } }
    public override bool showHeatWarnings { get { return false; } }
#else
    protected override bool ShouldShowWeaponsUI { get { return false; } }
    protected override bool ShouldShowTargetingLines { get { return false; } }
    protected override bool ShouldShowFiringArcs { get { return false; } }
    protected override bool showHeatWarnings { get { return false; } }
#endif
    public override bool ConsumesMovement { get { return false; } }
    public override bool ConsumesFiring { get { return false; } }
    public override bool CanBackOut { get { return NumPositionsLocked > 0; } }
    public virtual bool HasCalledShot { get { return false; } }
    public virtual bool NeedsCalledShot { get { return false; } }
    public override bool CanActorUseThisState(AbstractActor actor) { return actor.IsDeployDirector(); }
    public override bool CanDeselect { get { return false; } }
    private int NeedPositionsCount { get; set; }
    private List<DeployPosition> deployPositions { get; set; } = new List<DeployPosition>();
    public void FillDeployPositionsInfo() {
      if (DeployManualHelper.IsInManualSpawnSequence == false) {
        deployPositions.Clear();
        return;
      }
      List<AbstractActor> units = new List<AbstractActor>(this.Combat.LocalPlayerTeam.units);
      units.Remove(DeployManualHelper.deployDirector);
      Log.TWL(0, "FillDeployPositionsInfo");
      Dictionary<int, DeployPosition> positions = new Dictionary<int, DeployPosition>();
      Dictionary<int, DeployPosition> avaibleSlots = new Dictionary<int, DeployPosition>();
      {
        int index = 0;
        for (int lanceid = 0; lanceid < UnityGameInstance.BattleTechGame.Simulation.currentLayout().dropLances.Count; ++lanceid) {
          DropLanceDef lanceDef = UnityGameInstance.BattleTechGame.Simulation.currentLayout().dropLances[lanceid];
          for (int lancepos = 0; lancepos < lanceDef.dropSlots.Count; ++lancepos) {
            DeployPosition position = new DeployPosition();
            position.lanceid = lanceid;
            position.posid = lancepos;
            position.position = null;
            positions.Add(index, position);
            avaibleSlots.Add(index, position);
            ++index;
          }
        }
      }
      foreach (AbstractActor unit in this.Combat.LocalPlayerTeam.units) {
        string defGUID = string.Empty;
        if (unit.IsDeployDirector()) { continue; }
        defGUID = unit.PilotableActorDef.GUID + "_" + unit.PilotableActorDef.Description.Id + "_" + unit.GetPilot().Description.Id;
        Log.WL(1, unit.PilotableActorDef.ChassisID + " tags:" + unit.EncounterTags.ContentToString() + " defGUID:" + defGUID);
        if (string.IsNullOrEmpty(defGUID) == false) {
          if (CustomLanceHelper.playerLanceLoadout.loadout.TryGetValue(defGUID, out int slotIndex)) {
            Log.WL(2, "found slot in layout:" + slotIndex);
            if (avaibleSlots.TryGetValue(slotIndex, out DeployPosition slot)) {
              Log.WL(3, "found slot in available:" + slotIndex);
              slot.unit = unit;
              avaibleSlots.Remove(slotIndex);
              units.Remove(unit);
              continue;
            }
          }
        }
      }
      foreach (var slot in avaibleSlots) {
        if (units.Count <= 0) { break; }
        slot.Value.unit = units[0];
        units.RemoveAt(0);
      }
      deployPositions = new List<DeployPosition>();
      Log.TWL(0, "fill deploy positions:");
      for (int t = 0; t < UnityGameInstance.BattleTechGame.Simulation.currentLayout().slotsCount; ++t) {
        if (positions.TryGetValue(t, out DeployPosition pos) == false) { continue; }
        if ((pos.unit == null)) { continue; }
        Log.WL(1, t + ":" + (pos.unit == null ? "null" : pos.unit.PilotableActorDef.ChassisID) + " lance:" + pos.lanceid + " pos:" + pos.posid);
        deployPositions.Add(pos);
      }
      NeedPositionsCount = deployPositions.Count;
    }
    public override void OnAddToStack() {
      NumPositionsLocked = 0;
      int count = 0;
      foreach (AbstractActor unit in this.Combat.LocalPlayerTeam.units) {
        if (unit.IsDeployDirector() == false) { ++count; }
      }
      TargetingCirclesHelper.InitCircles(
        //PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest != null? PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.unitSpawnerCount : 0,
        count,
        this.HUD
      );
      TargetingCirclesHelper.ShowRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      this.ResetCache();
      Log.TWL(0, "SelectionStateCommandDeploy OnAddToStack:" + NeedPositionsCount);
      this.FillDeployPositionsInfo();
      base.OnAddToStack();
    }
    public override void OnInactivate() {
      Log.TWL(0, "SelectionStateCommandDeploy.OnInactivate HasActivated: " + HasActivated);
      TargetingCirclesHelper.HideRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      NumPositionsLocked = 0;
      NeedPositionsCount = 0;
      deployPositions.Clear();
      this.ResetCache();
      base.OnInactivate();
      CombatHUDMechwarriorTrayEx trayEx = this.HUD.MechWarriorTray.gameObject.GetComponent<CombatHUDMechwarriorTrayEx>();
    }
    public void AttackOrdersSet(MessageCenterMessage message) {
      this.Orders = (message as AddSequenceToStackMessage).sequence;
      //Log.TWL(0, "Attack orders set:" + Orders.GetType().ToString() + " GUID:" + Orders.SequenceGUID);
    }
    public override bool ProcessPressedButton(string button) {
      this.HasActivated = button == "BTN_FireConfirm";
      if (button != "BTN_FireConfirm") {
        return base.ProcessPressedButton(button);
      }
      if (this.Orders != null)
        return false;
      this.HideFireButton(false);
      CombatTargetingReticle.Instance.HideReticle();
      TargetingCirclesHelper.HideRoot();
      TargetingCirclesHelper.ShowCirclesCount(0);
      Mech deployDirector = HUD.SelectedActor as Mech;
      DeployManualHelper.Deploy(this.HUD, this.deployPositions);
      //if (PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest != null) {
      //  PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.RestoreAndSpawn(this.deployPositions, deployDirector, this.HUD);
      //  //PlayerLanceSpawnerGameLogic_OnEnterActive.Clear();
      //}
      return true;
    }
    public override void BackOut() {
      if (this.NumPositionsLocked > this.deployPositions.Count) { this.NumPositionsLocked = this.deployPositions.Count; }
      if (this.NumPositionsLocked > 0) {
        this.HideFireButton(false);
        this.deployPositions[this.NumPositionsLocked - 1].position = null;
        --this.NumPositionsLocked;
        TargetingCirclesHelper.ShowRoot();
        TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
        this.ResetCache();
        Log.TWL(0, "SelectionStateCommandDeploy.BackOut:" + this.NumPositionsLocked);
      } else {
        Debug.LogError((object)"Tried to back out of 1-point command state while backout unavailable");
      }
    }
    public void AddDeployPosition(Vector3 worldPos) {
      worldPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(worldPos);
      worldPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(worldPos);
      this.deployPositions[this.NumPositionsLocked].position = worldPos;
      ++this.NumPositionsLocked;
      this.ResetCache();
    }
    public void AddLancePosition(Vector3 worldPos) {
      int curLance = this.deployPositions[this.NumPositionsLocked].lanceid;
      int size = 0;
      for (int t = this.NumPositionsLocked + 1; t < deployPositions.Count; ++t) {
        if (this.deployPositions[t].lanceid != curLance) { size = t - this.NumPositionsLocked; break; }
      }
      if (size == 0) { size = this.deployPositions.Count - this.NumPositionsLocked; }
      for (int t = 0; t < size; ++t) {
        Vector3 rndPos = worldPos;
        float nearest = 9999f;
        do {
          rndPos = worldPos;
          float radius = UnityEngine.Random.Range(0.5f * Core.Settings.DeploySpawnRadius, Core.Settings.DeploySpawnRadius);
          float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
          //radius *= UnityEngine.Random.Range(targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
          //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
          rndPos.x += Mathf.Sin(direction) * radius;
          rndPos.z += Mathf.Cos(direction) * radius;
          rndPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(rndPos);
          rndPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(rndPos);
          nearest = 9999f;
          foreach (DeployPosition depPos in deployPositions) {
            if (depPos.position.HasValue == false) { continue; }
            float dist = Vector3.Distance(depPos.position.Value, rndPos); if (nearest > dist) { nearest = dist; };
          };
          Log.WL(1, rndPos.ToString() + " distance:" + Vector3.Distance(rndPos, worldPos) + " nearest:" + nearest + " rejected:" + (nearest < Core.Settings.DeploySpawnRadius / 4f), true);
        } while (nearest < Core.Settings.DeploySpawnRadius / 4f);
        this.deployPositions[this.NumPositionsLocked + t].position = rndPos;
      }
      this.NumPositionsLocked += size;
      this.ResetCache();
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      //if (this.NumPositionsLocked != 0) { return false; }
      //float originalDist = Vector3.Distance(worldPos, PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.Position);
      //float originalDist = Vector3.Distance(worldPos, this.HUD.SelectedActor.CurrentPosition);
      //List<Vector3> enemies = this.Combat.ActiveContract.deplayedEnemySpawnPositions();
      //float nearesEnemyDist = 9999f;
      //foreach (Vector3 unit in enemies) {
      //  float dist = Vector3.Distance(worldPos, unit);
      //  if ((nearesEnemyDist == 9999f) || (dist < nearesEnemyDist)) { nearesEnemyDist = dist; }
      //}
      //if ((originalDist < Core.Settings.DeployMaxDistanceFromOriginal) || (nearesEnemyDist > Core.Settings.DeployMinDistanceFromEnemy)) {
      if (lastViewedPosition.HasValue == false) { return false; }
      if (this.CheckPosition(lastViewedPosition.Value)) {
        if (this.NumPositionsLocked < this.deployPositions.Count) {
          this.targetPosition = lastViewedPosition.Value;
          if (Input.GetKey(KeyCode.LeftAlt)) { this.AddDeployPosition(lastViewedPosition.Value); } else { this.AddLancePosition(lastViewedPosition.Value); }
          TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
          this.ResetCache();
        }
        if (this.NumPositionsLocked >= this.deployPositions.Count) {
          this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
          CombatTargetingReticle.Instance.HideReticle();
        }
        return true;
      } else {
        return false;
      }
      //} else {
      //  GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Drop point too close to enemy position "+ Mathf.Round(nearesEnemyDist)+" < "+ Core.Settings.DeployMinDistanceFromEnemy+"\nand too far from suggested deploy "+Mathf.Round(originalDist)+" > "+ Core.Settings.DeployMaxDistanceFromOriginal);
      //  popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
      //  popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
      //  return false;
      //}
    }
    public override Vector3 PreviewPos {
      get {
        return this.SelectedActor.CurrentPosition;
      }
    }
    public class PositionInfo {
      public string info { get; set; }
      public bool result { get; set; }
      public PositionInfo(string info, bool res) {
        this.info = info;
        result = res;
      }
    }
    public void ShowSidePanel(PositionInfo info) {
      Text title = new Text("DEPLOY POSITION INFO");
      Text description = new Text(info.info);
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }

    private Dictionary<Vector3, PositionInfo> positionsCache = new Dictionary<Vector3, PositionInfo>();
    public void ResetCache() { positionsCache.Clear(); }
    public bool CheckPosition(Vector3 worldPos) {
      if (positionsCache.TryGetValue(worldPos, out PositionInfo result)) {
        ShowSidePanel(result);
        return result.result;
      }
      result = new PositionInfo(string.Empty, true);
      Log.TWL(0, "SelectionStateCommandDeploy.CheckPosition " + worldPos);
      Vector3 f = worldPos + Vector3.forward * Core.Settings.DeploySpawnRadius;
      Vector3 b = worldPos + Vector3.back * Core.Settings.DeploySpawnRadius;
      Vector3 l = worldPos + Vector3.left * Core.Settings.DeploySpawnRadius;
      Vector3 r = worldPos + Vector3.right * Core.Settings.DeploySpawnRadius;
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(f) == false) { result.result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(b) == false) { result.result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(l) == false) { result.result = false; } else
      if (HUD.Combat.EncounterLayerData.IsInEncounterBounds(r) == false) { result.result = false; };
      if (result.result == false) {
        Log.WL(1, "out of bounds");
        result.info = "out of encounter bounds\n<color=red>DEPLOY FORBIDDEN</color>";
        positionsCache.Add(worldPos, result); return false;
      }
      float originalDist = Vector3.Distance(worldPos, this.HUD.SelectedActor.CurrentPosition);
      Log.WL(1, "original dist:" + originalDist + "/" + Core.Settings.DeployMaxDistanceFromOriginal);
      result.info = "distance from suggested deploy position " + Mathf.Round(originalDist) + " max allowed " + Core.Settings.DeployMaxDistanceFromOriginal;
      bool checkLancematesDistance = DeployManualHelper.originalSpawnMethod == SpawnUnitMethodType.ViaLeopardDropship;
      if ((this.NumPositionsLocked == 0) || (checkLancematesDistance == false)) {
        if (originalDist < Core.Settings.DeployMaxDistanceFromOriginal) {
          result.result = true;
          result.info += "\n<color=green>DEPLOY ALLOWED</color>";
          positionsCache.Add(worldPos, result); return result.result;
        }
      }
      if (checkLancematesDistance && this.NumPositionsLocked > 0) {
        Log.WL(1, "checking lancemates");
        Vector3? centerPoint = null;
        int count = 0;
        foreach (DeployPosition pos in deployPositions) {
          if (pos.position.HasValue == false) { continue; }
          Log.WL(2, "mate:" + pos.position.Value);
          if (centerPoint == null) { centerPoint = pos.position.Value; count = 1; continue; }
          Vector3 tmp = centerPoint.Value;
          tmp.x += pos.position.Value.x;
          tmp.z += pos.position.Value.z;
          ++count;
          centerPoint = tmp;
        }
        if (centerPoint != null) {
          Vector3 tmp = centerPoint.Value;
          tmp.x = tmp.x / count;
          tmp.z = tmp.z / count;
          tmp = HUD.Combat.HexGrid.GetClosestPointOnGrid(tmp);
          Log.WL(1, "center point:" + tmp);
          float matesDistance = Vector3.Distance(tmp, worldPos);
          //Log.WL(1, worldPos + " centerPoint:" + tmp+" distance:"+matesDistance);
          if (matesDistance > Core.Settings.DeployMaxDistanceFromMates) {
            result.result = false;
            result.info += "\ndistance from your lance mates " + Mathf.Round(matesDistance) + " max allowed " + Core.Settings.DeployMaxDistanceFromMates;
            result.info += "\n<color=red>DEPLOY FORBIDDEN</color>";
            positionsCache.Add(worldPos, result);
            Log.WL(1, "too far from mates:" + matesDistance + ">" + Core.Settings.DeployMaxDistanceFromMates);
            return result.result;
          }
        }
        if (originalDist < Core.Settings.DeployMaxDistanceFromOriginal) {
          result.result = true;
          result.info += "\n<color=green>DEPLOY ALLOWED</color>";
          positionsCache.Add(worldPos, result); return result.result;
        }
      }
      List<Vector3> enemies = this.Combat.enemiesPositions();
      float nearesEnemyDist = 9999f;
      foreach (Vector3 unit in enemies) {
        float dist = Vector3.Distance(worldPos, unit);
        if ((nearesEnemyDist == 9999f) || (dist < nearesEnemyDist)) { nearesEnemyDist = dist; }
      }
      result.result = nearesEnemyDist > Core.Settings.DeployMinDistanceFromEnemy;
      Log.WL(1, "nearest enemy:" + nearesEnemyDist + " setting:" + Core.Settings.DeployMinDistanceFromEnemy + " result:" + result);
      result.info += "\ndistance from known enemy position " + Mathf.Round(nearesEnemyDist) + " min allowed " + Core.Settings.DeployMinDistanceFromEnemy;
      if (result.result == false) {
        result.info += "\n<color=red>DEPLOY FORBIDDEN</color>";
      } else {
        result.info += "\n<color=green>DEPLOY ALLOWED</color>";
      }
      positionsCache.Add(worldPos, result);
      ShowSidePanel(result);
      return result.result;
    }
    public Vector3? lastViewedPosition;
    public override void ProcessMousePos(Vector3 worldPos) {
      //switch (this.NumPositionsLocked) {
      //  case 0: {
      //    CombatTargetingReticle.Instance.UpdateReticle(worldPos, Core.Settings.DeploySpawnRadius, false);
      //  };break;
      //  case 1:
      //    CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, Core.Settings.DeploySpawnRadius, false);
      //  break;
      //}
      //Log.TWL(0, "SelectionState_ProcessMousePos");
      try {
        if (deployPositions == null) { FillDeployPositionsInfo(); }
      }catch(Exception e) {
        Log.TWL(0, e.ToString());
      }
      if (deployPositions == null) { }
      try {
        try {
          TargetingCirclesHelper.ShowCirclesCount(this.NumPositionsLocked);
          for (int index = 0; index < this.NumPositionsLocked; ++index) {
            if (index >= deployPositions.Count) { continue; }
            if (deployPositions[index].position.HasValue) {
              string text = String.Empty;
              MechDef def = deployPositions[index].unit.PilotableActorDef as MechDef;
              if (def != null) { text = def.Chassis.VariantName; } else {
                text = deployPositions[index].unit.PilotableActorDef.Description.Name;
              }
              TargetingCirclesHelper.UpdateCircle(index, deployPositions[index].position.Value, Core.Settings.DeploySingleCircleRadiusTarget, text);
            }
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
        if (this.NumPositionsLocked < deployPositions.Count) {
          worldPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(worldPos);
          if (this.CheckPosition(worldPos)) {
            float range = Input.GetKey(KeyCode.LeftAlt) ? Core.Settings.DeploySingleCircleRadius : Core.Settings.DeploySpawnRadius;
            CombatTargetingReticle.Instance.UpdateReticle(worldPos, range, false);
            lastViewedPosition = worldPos;
          }
        }
        this.SelectionState_ProcessMousePos(SelectedActor.CurrentPosition);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public override int ProjectedHeatForState { get { return 0; } }
  };
  [HarmonyPatch(typeof(MessageCenter))]
  [HarmonyPatch("PublishMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MessageCenter_PublishMessage_SuppressFloaties {
    public static bool Prefix(MessageCenter __instance, MessageCenterMessage message) {
      if (DeployManualHelper.IsInManualSpawnSequence == false) { return true; }
      if (message == null) { return true; }
      if (message?.MessageType != MessageCenterMessageType.FloatieMessage) {
        return true;
      }
      Log.TWL(0, "MessageCenter.PublishMessage " + message.MessageType + " suppress");
      return false;
    }
  }
  //[HarmonyPatch(typeof(TurnDirector))]
  //[HarmonyPatch("BeginNewPhase")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int) })]
  //public static class TurnDirector_BeginNewPhase {
  //  public static void Postfix(TurnDirector __instance, int newPhase) {
  //    try {
  //      if (DeployManualHelper.IsInManualSpawnSequence) { return; }
  //      if (DeployManualHelper.deployDirector == null) { return; }
  //      if (DeployManualHelper.deployDirector.IsDead) { return; }
  //      if (DeployManualHelper.deployDirector.IsFlaggedForDeath) { return; }
  //      CombatHUD HUD = UIManager.Instance.UIRoot.gameObject.GetComponentInChildren<CombatHUD>(true);
  //      DeployDirectorDespawner despawner = HUD.gameObject.GetComponent<DeployDirectorDespawner>();
  //      if (despawner == null) {
  //        despawner = HUD.gameObject.AddComponent<DeployDirectorDespawner>();
  //        despawner.Init(HUD.Combat, HUD);
  //      };
  //      despawner.deployDirector = DeployManualHelper.deployDirector;
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByType {
    public static bool SelectionForbidden = false;
    public static bool Prefix(SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      Log.TWL(0, "SelectionState.GetNewSelectionStateByType " + type + ":" + FromButton.GUID);
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == DeployManualHelper.DeployAbilityDefID)) {
        Log.WL(1, "creating own selection state");
        __result = new SelectionStateCommandDeploy(Combat, HUD, FromButton, actor);
        return false;
      } else
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == SpawnHelper.SpawnAbilityDefID)) {
        Log.WL(1, "creating own selection state");
        __result = new SelectionStateCommandSpawnUnit(Combat, HUD, FromButton, actor);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnPlayerVisibilityChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
  public static class AbstractActor_OnPlayerVisibilityChanged {
    public static void Prefix(AbstractActor __instance, ref VisibilityLevel newLevel) {
      if (DeployManualHelper.IsInManualSpawnSequence) {
        Log.TWL(0, "AbstractActor.OnPlayerVisibilityChanged "+__instance.PilotableActorDef.ChassisID+" IsInManualSpawnSequence");
        newLevel = VisibilityLevel.None;
      }
    }
  }
  [HarmonyPatch(typeof(PathingManager))]
  [HarmonyPatch("removeDeadPaths")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class PathingManager_removeDeadPaths {
    public static bool SelectionForbidden = false;
    public static bool Prefix(PathingManager __instance) {
      for (int index = __instance.BlockingPaths.Count - 1; index >= 0; --index) {
        if (__instance.BlockingPaths[index].OwningActor == null)
          __instance.BlockingPaths.RemoveAt(index);
      }
      for (int index = __instance.PathsToBuild.Count - 1; index >= 0; --index) {
        if (__instance.PathsToBuild[index].OwningActor == null)
          __instance.PathsToBuild.RemoveAt(index);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Pathing))]
  [HarmonyPatch("ResetPathGridIfTouching")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<Rect>), typeof(Vector3), typeof(float), typeof(AbstractActor) })]
  public static class Pathing_ResetPathGridIfTouching {
    public static bool SelectionForbidden = false;
    public static bool Prefix(Pathing __instance, List<Rect> Rectangles, Vector3 origin, float beginAngle, AbstractActor actor) {
      PathingCapabilitiesDef PathingCaps = Traverse.Create(__instance).Property<PathingCapabilitiesDef>("PathingCaps").Value;
      if ((PathingCaps == null) && (actor.PathingCaps != null)) {
        Traverse.Create(__instance).Property<PathingCapabilitiesDef>("PathingCaps").Value = actor.PathingCaps;
      }
      Log.TWL(0, "Pathing.ResetPathGridIfTouching " + actor.DisplayName + " caps:" + (actor.PathingCaps == null ? "null" : actor.PathingCaps.Description.Id) + " pathing:" + (PathingCaps == null ? "null" : PathingCaps.Description.Id));
      return true;
    }
  }
  //[HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  //[HarmonyPatch("SpawnUnits")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(bool) })]
  //public static class LanceSpawnerGameLogic_SpawnUnits {
  //  private class OnUnitSpawnComplete_delegate {
  //    public LanceSpawnerGameLogic __instance { get; set; }
  //    public void OnUnitSpawnComplete() {
  //      Log.TWL(0, "LanceSpawnerGameLogic.OnUnitSpawnComplete " + __instance.Name);
  //      Traverse.Create(__instance).Method("OnUnitSpawnComplete").GetValue();
  //    }
  //    public OnUnitSpawnComplete_delegate(LanceSpawnerGameLogic __instance) { this.__instance = __instance; }
  //  }
  //  public static bool Prefix(LanceSpawnerGameLogic __instance, bool offScreen) {
  //    try {
  //      Log.TWL(0, "LanceSpawnerGameLogic.SpawnUnits " + __instance.Name);
  //      UnitSpawnPointGameLogic[] pointGameLogicList = __instance.unitSpawnPointGameLogicList;
  //      for (int index = 0; index < pointGameLogicList.Length; ++index)
  //        pointGameLogicList[index].MarkUnitSpawnInProgress();
  //      for (int index = 0; index < pointGameLogicList.Length; ++index)
  //        pointGameLogicList[index].SpawnUnit(offScreen, new Action(new OnUnitSpawnComplete_delegate(__instance).OnUnitSpawnComplete));
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return false;
  //  }
  //}
  //[HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  //[HarmonyPatch("SpawnUnit")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(bool), typeof(Action) })]
  //public static class UnitSpawnPointGameLogic_SpawnUnit {
  //  public static void Prefix(UnitSpawnPointGameLogic __instance, bool spawnOffScreen, Action onComplete) {
  //    Log.TWL(0, "UnitSpawnPointGameLogic.SpawnUnit "+ __instance.UnitDefId+":"+__instance.unitType+ " UnitIsLoaded:" + Traverse.Create(__instance).Method("UnitIsLoaded").GetValue<bool>());
  //  }
  //}
  //[HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  //[HarmonyPatch("CompleteSpawnUnit")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class UnitSpawnPointGameLogic_CompleteSpawnUnit {
  //  public static void Prefix(UnitSpawnPointGameLogic __instance) {
  //    Log.TWL(0, "UnitSpawnPointGameLogic.CompleteSpawnUnit " + __instance.UnitDefId + ":" + __instance.unitType);
  //  }
  //}
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("VisibilityToTargetUnit")]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class AbstractActor_VisibilityToTargetUnit {
    public static void Postfix(AbstractActor __instance, ICombatant targetUnit, ref VisibilityLevel __result) {
      if (__result == VisibilityLevel.None) { return; }
      if (DeployManualHelper.IsInManualSpawnSequence) { __result = VisibilityLevel.None; return; }
    }
  }
  [HarmonyPatch(typeof(SharedVisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class SharedVisibilityCache_RebuildCache {
    public static bool Prefix(SharedVisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          Traverse.Create(__instance).Field<Dictionary<string, VisibilityLevelAndAttribution>>("VisLevelCache").Value.Clear();
          __instance.previouslyDetectedEnemyUnits.Clear();
          __instance.previouslyDetectedEnemyLocations.Clear();
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_RebuildCache {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          Traverse.Create(__instance).Field<Dictionary<string, VisibilityLevelAndAttribution>>("VisLevelCache").Value.Clear();
          __instance.previouslyDetectedEnemyUnits.Clear();
          __instance.previouslyDetectedEnemyLocations.Clear();
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateCacheReciprocal")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_UpdateCacheReciprocal {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          Traverse.Create(__instance).Field<Dictionary<string, VisibilityLevelAndAttribution>>("VisLevelCache").Value.Clear();
          __instance.previouslyDetectedEnemyUnits.Clear();
          __instance.previouslyDetectedEnemyLocations.Clear();
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SharedVisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("getSharedValue")]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class VisibilityCache_getSharedValue {
    public static bool Prefix(VisibilityCache __instance, ICombatant target, ref VisibilityLevelAndAttribution __result) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          __result = VisibilityLevelAndAttribution.Unseen;
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(VisibilityCacheBase))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("HasAnyContact")]
  [HarmonyPatch(new Type[] { })]
  public static class VisibilityCacheBase_HasAnyContact {
    public static bool Prefix(VisibilityCacheBase __instance, ref bool __result) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          __result = false;
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(VisibilityCacheBase))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("HasAnyVisibility")]
  [HarmonyPatch(new Type[] { })]
  public static class VisibilityCacheBase_HasAnyVisibility {
    public static bool Prefix(VisibilityCacheBase __instance, ref bool __result) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence) {
          __result = false;
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SpawnMech")]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(Team), typeof(Lance), typeof(HeraldryDef) })]
  public static class UnitSpawnPointGameLogic_SpawnMech {
    public static void Postfix(UnitSpawnPointGameLogic __instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef, Mech __result) {
      if (__result.IsDeployDirector()) {
        //__result.GameRep.gameObject.SetActive(false);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowWeaponsUpTo")]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class CombatHUDWeaponPanel_ShowWeaponsUpTo {
    public static bool Prefix(CombatHUDWeaponPanel __instance) {
      if (__instance.DisplayedActor == null) { return true; }
      if (__instance.DisplayedActor.IsDeployDirector() && (__instance.DisplayedActor.IsDead == false)) {
        __instance.gameObject.SetActive(false);
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Update")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDMechTray_UpdateDirector {
    public static bool Prefix(CombatHUDMechTray __instance) {
      if (__instance.DisplayedActor == null) { return true; }
      if (__instance.DisplayedActor.IsDeployDirector() && (__instance.DisplayedActor.IsDead == false)) {
        __instance.gameObject.SetActive(false);
        return false;
      }
      return true;
    }
  }
  //[HarmonyPatch(typeof(TurnDirector))]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch("OnDropshipAnimationComplete")]
  //[HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  //public static class TurnDirector_OnDropshipAnimationComplete {
  //  public static void Postfix(TurnDirector __instance, MessageCenterMessage message) {
  //    Mech deployDirector = null;
  //    Log.TWL(0, "TurnDirector.OnDropshipAnimationComplete");
  //    foreach(AbstractActor unit in __instance.Combat.LocalPlayerTeam.units) {
  //      if (unit.IsDead) { continue; }
  //      if (unit.IsDeployDirector() == false) { continue; }
  //      deployDirector = unit as Mech;
  //      break;
  //    }
  //    if(deployDirector != null) {
  //      Log.WL(1, "deployDirector:"+ deployDirector.PilotableActorDef.ChassisID);
  //      if (__instance.Combat.LocalPlayerTeam.unitCount > 1) {
  //        deployDirector.HasActivatedThisRound = false;
  //        CombatHUD HUD = UIManager.Instance.UIRoot.gameObject.GetComponentInChildren<CombatHUD>(true);
  //        if (HUD != null) {
  //          Log.WL(1, "SelectedActor:" + (HUD.SelectedActor == null ? "null" : HUD.SelectedActor.PilotableActorDef.ChassisID));
  //          do {
  //            Traverse.Create(HUD.SelectionHandler).Method("BackOutOneStep", false).GetValue();
  //          } while (HUD.SelectedActor != null);
  //          Log.WL(1, "SelectedActor:" + (HUD.SelectedActor == null ? "null" : HUD.SelectedActor.PilotableActorDef.ChassisID));
  //          DeployDirectorDespawner despawner = HUD.gameObject.GetComponent<DeployDirectorDespawner>();
  //          if (despawner == null) {
  //            despawner = HUD.gameObject.AddComponent<DeployDirectorDespawner>();
  //            despawner.Init(deployDirector.Combat, HUD);
  //          }
  //          //deployDirector.HUD().SelectionHandler.DeselectActor(deployDirector);
  //          HUD.MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
  //          despawner.deployDirector = deployDirector;
  //        }
  //        __instance.Combat.StackManager.DEBUG_SequenceStack.Clear();
  //        typeof(TurnDirector).GetMethod("QueuePilotChatter", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
  //        List<AbstractActor> allActors = __instance.Combat.TurnDirector.Combat.AllActors;
  //        List<ICombatant> livingCombatants = __instance.Combat.TurnDirector.Combat.GetAllLivingCombatants();
  //        int num = 1;
  //        AuraCache.UpdateAllAuras(allActors, num != 0);
  //        for (int index = 0; index < __instance.Combat.TurnDirector.NumTurnActors; ++index) {
  //          Team turnActor = __instance.Combat.TurnDirector.TurnActors[index] as Team;
  //          if (turnActor != null)
  //            turnActor.RebuildVisibilityCacheAllUnits(livingCombatants);
  //        }
  //        __instance.Combat.TurnDirector.Combat.AllActors[0].UpdateVisibilityCache(livingCombatants);
  //        __instance.Combat.TurnDirector.StartFirstRound();
  //        //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new DialogComplete(DialogueGameLogic.missionStartDialogueGuid));
  //        //EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
  //      }
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(AbstractActor))]
  //[HarmonyPatch("DespawnActor")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  //public static class AbstractActor_DespawnActor {
  //  public static void Postfix(AbstractActor __instance, MessageCenterMessage message, ref string ____teamId, ref Team ____team) {
  //    try {
  //      DespawnActorMessage despawnActorMessage = message as DespawnActorMessage;
  //      if (despawnActorMessage == null) { return; }
  //      if (!(despawnActorMessage.affectedObjectGuid == __instance.GUID)) { return; }
  //      Log.TWL(0, "AbstractActor.DespawnActor " + __instance.DisplayName + ":" + despawnActorMessage.deathMethod);
  //      if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
  //      if (__instance.IsDeployDirector() == false) { return; }
  //      __instance.HUD().SelectionHandler.DeselectActor(__instance);
  //      __instance.Combat.TurnDirector.StartFirstRound();
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(CombatSelectionHandler))]
  //[HarmonyPatch("TrySelectActor")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(bool) })]
  //public static class AbstractActor_TrySelectActor {
  //  public static void Postfix(CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection) {
  //    if (actor == null) { return; }
  //    //if(actor.Combat.LocalPlayerTeam.unitCount == 1) {
  //    //  if (actor.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
  //    //    return;
  //    //  }
  //    //}
  //    //try {
  //    //  Mech deployDirector = null;
  //    //  foreach (AbstractActor unit in actor.Combat.LocalPlayerTeam.units) {
  //    //    if (unit.IsDead) { continue; }
  //    //    if (unit.IsDeployDirector() == false) { continue; }
  //    //    deployDirector = unit as Mech;
  //    //    break;
  //    //  }
  //    //  if (deployDirector != null) {
  //    //    Log.TWL(0, "CombatSelectionHandler.TrySelectActor phase:" + deployDirector.Combat.TurnDirector.CurrentPhase + " is avaible:" + deployDirector.IsAvailableThisPhase);
  //    //    if (deployDirector.IsAvailableThisPhase) {
  //    //      deployDirector.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(deployDirector.DoneWithActor()));
  //    //    } else {
  //    //      EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
  //    //    }
  //    //  }
  //    //} catch (Exception e) {
  //      //Log.TWL(0, e.ToString(), true);
  //    //}
  //  }
  //}
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_InitStats {
    public static void Postfix(Mech __instance) {
      try {
        if (__instance.IsDeployDirector()) {
          __instance.Initiative = 0;
          __instance.StatCollection.Set<int>("BaseInitiative", 0);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch("OnUnitSpawn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class FogOfWarSystem_OnUnitSpawn {
    public static bool Prefix(FogOfWarSystem __instance, MessageCenterMessage message, List<AbstractActor> ___viewers) {
      try {
        Log.TWL(0, "FogOfWarSystem.OnUnitSpawn");
        Log.WL(1, "viewers:");
        foreach (AbstractActor viewer in ___viewers) {
          Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
        }
        ITaggedItem itemByGuid = __instance.Combat.ItemRegistry.GetItemByGUID((message as UnitSpawnedMessage).affectedObjectGuid);
        if (itemByGuid == null) { return true; }
        AbstractActor unit = itemByGuid as AbstractActor;
        if (unit == null) { return true; };
        Log.WL(1, "unit:" + unit.DisplayName + " " + unit.GUID + " IsDeployDirector:" + unit.IsDeployDirector());
        if (unit.IsDeployDirector()) {
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(FogOfWarSystem __instance, MessageCenterMessage message, List<AbstractActor> ___viewers, List<FogOfWarRevealatron> ___revealatrons) {
      Log.TWL(0, "FogOfWarSystem.OnUnitSpawn");
      Log.WL(1, "viewers:");
      foreach (AbstractActor viewer in ___viewers) {
        Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
      }
      Log.WL(1, "revealatrons:");
      foreach (FogOfWarRevealatron revealatron in ___revealatrons) {
        Log.WL(2, revealatron.name + ":" + revealatron.GUID);
      }
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch("AddViewer")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class FogOfWarSystem_AddViewer {
    public static bool Prefix(FogOfWarSystem __instance, AbstractActor unit, List<AbstractActor> ___viewers) {
      try {
        Log.TWL(0, "FogOfWarSystem.AddViewer " + unit.DisplayName + ":" + unit.GUID + " IsDeployDirector:" + unit.IsDeployDirector());
        Log.WL(1, "viewers:");
        foreach (AbstractActor viewer in ___viewers) {
          Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
        }
        if (unit.IsDeployDirector()) {
          //__instance.WipeToValue(FogOfWarState.Revealed);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(FogOfWarSystem __instance, List<AbstractActor> ___viewers, List<FogOfWarRevealatron> ___revealatrons) {
      Log.TWL(0, "FogOfWarSystem.AddViewer");
      Log.WL(1, "viewers:");
      foreach (AbstractActor viewer in ___viewers) {
        Log.WL(2, viewer.DisplayName + ":" + viewer.GUID);
      }
      Log.WL(1, "revealatrons:");
      foreach (FogOfWarRevealatron revealatron in ___revealatrons) {
        Log.WL(2, revealatron.name + ":" + revealatron.GUID);
      }
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("DeselectActor")]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatSelectionHandler_DeselectActor {
    public static bool Prefix(CombatSelectionHandler __instance, AbstractActor actor) {
      if (actor == null) { return true; }
      if (actor.Combat == null) { return true; }
      if (actor.Combat.LocalPlayerTeam == null) { return true; }
      if (actor.Combat.LocalPlayerTeam.unitCount != 1) { return true; }
      if (actor.IsDeployDirector() && (actor.IsDead == false)) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(FogOfWarSystem))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("WipeToValue")]
  [HarmonyPatch(new Type[] { typeof(FogOfWarState) })]
  public static class FogOfWarSystem_WipeToValue {
    private static bool HideFogOfWar = false;
    public static void NormalFoW() { HideFogOfWar = false; }
    public static void RevealFoW() { HideFogOfWar = true; }
    public static bool Prefix(FogOfWarSystem __instance, ref FogOfWarState fowState) {
      if (HideFogOfWar) { fowState = FogOfWarState.Revealed; }
      return true;
    }
  }
  //[HarmonyPatch(typeof(EncounterChunkGameLogic))]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch("EncounterStart")]
  //[HarmonyPatch(new Type[] {  })]
  //public static class EncounterChunkGameLogic_EncounterStart {
  //  public static void Postfix(EncounterChunkGameLogic __instance) {
  //    try {
  //      if(__instance is AmbushChunkGameLogic ambush) {
  //        Log.TWL(0, "AmbushChunkGameLogic.EncounterStart");
  //        if (ambush.ambushTriggerRegion.encounterObject != null) {
  //          Log.WL(1, "Trigger region is not null");

  //        }
  //      }
  //    }catch(Exception e) {
  //      Log.TWL(0, e.ToString(),true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(GameInstance))]
  [HarmonyPatch("LaunchContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract), typeof(string) })]
  public static class GameInstance_LaunchContract {
    public static bool SpawnDelayed = false;
    private static bool originalInvoke = false;
    public static bool isManualSpawn(this Contract contract) { return SpawnDelayed; }
    public static bool canManualSpawn(this Contract contract) {
      if (Core.Settings.DeployManual == false) { return false; }
      if (contract.SimGameContract == false) { return false; }
      if (contract.IsFlashpointCampaignContract) { return false; }
      if (contract.IsFlashpointContract) { return false; }
      if (contract.IsRestorationContract) { return false; }
      if (contract.IsStoryContract) { return false; }
      if (Core.Settings.DeployForbidContractTypes.Contains(contract.ContractTypeValue.Name)) { return false; }
      return true;
    }
    public static void ClearManualSpawn(this Contract contract) { SpawnDelayed = false; }
    public static bool Prefix(GameInstance __instance, Contract contract, string playerGUID) {
      if (originalInvoke) { return true; }
      SpawnDelayed = false;
      DeployManualHelper.IsInManualSpawnSequence = false;
      if (contract.canManualSpawn() == false) { return true; }
      if (DeployManualHelper.CheckForDeps(__instance, contract, playerGUID) == false) { return false; }

      if (Core.Settings.AskForDeployManual) {
        bool isInvoked = false;
        GenericPopup popup = GenericPopupBuilder.Create("DEPLOY POSITION", "WOULD YOU LIKE TO SET DEPLOY POSITION MANUALY?")
          .AddButton("NO", (Action)(() => {
            if (isInvoked) { return; }; isInvoked = true;
            SpawnDelayed = false;
            DeployManualHelper.IsInManualSpawnSequence = false;
            originalInvoke = true;
            if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
              uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
              Log.TWL(0, "Playing sound by id:" + num2);
            } else {
              Log.TWL(0, "Can't play");
            }
            __instance.LaunchContract(contract, playerGUID);
            originalInvoke = false;
          }), true, BTInput.Instance.Key_Escape())
          .AddButton("YES", (Action)(() => {
            if (isInvoked) { return; }; isInvoked = true;
            SpawnDelayed = true;
            DeployManualHelper.IsInManualSpawnSequence = true;
            originalInvoke = true;
            if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
              uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
              Log.TWL(0, "Playing sound by id:" + num2);
            } else {
              Log.TWL(0, "Can't play");
            }
            __instance.LaunchContract(contract, playerGUID);
            originalInvoke = false;
          }), true, BTInput.Instance.Key_Return()).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      } else {
        SpawnDelayed = true;
        DeployManualHelper.IsInManualSpawnSequence = true;
        return true;
      }
      return false;
    }
  }
  //[HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  //[HarmonyPatch("OnEnterActive")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnEnterActive {
    //public static HashSet<LanceSpawnerGameLogic> delayedSpawners = new HashSet<LanceSpawnerGameLogic>();
    //public static List<Vector3> delayedEnemySpawnPositions(this Contract contract) {
    //  List<Vector3> result = new List<Vector3>();
    //  foreach (LanceSpawnerGameLogic spawner in delayedSpawners) {
    //    try {
    //      if (spawner == null) { continue; }
    //      Team team = spawner.Combat.TurnDirector.GetTurnActorByUniqueId(spawner.teamDefinitionGuid) as Team;
    //      if (team == null) { continue; }
    //      if (team.IsEnemy(spawner.Combat.LocalPlayerTeam) == false) { continue; }
    //      foreach (UnitSpawnPointGameLogic spawnPoint in spawner.unitSpawnPointGameLogicList) {
    //        if (spawnPoint.unitType == UnitType.UNDEFINED) { continue; }
    //        result.Add(spawnPoint.Position);
    //      }
    //    } catch (Exception e) {
    //      Log.TWL(0, e.ToString(), true);
    //    }
    //  }
    //  return result;
    //}
    //public static void SetSpawnerReady(this LanceSpawnerGameLogic spawner) {
    //  if (delayedSpawners.Contains(spawner)) {
    //    delayedSpawners.Remove(spawner);
    //    Log.TWL(0, "Delayed spawner ready: " + spawner.DisplayName + " objectives ready state:" + spawner.Combat.ActiveContract.isObjectivesReady());
    //  }
    //}
    //public static void ActivateDelayed(this Contract contract) {
    //  HashSet<LanceSpawnerGameLogic> spawners = new HashSet<LanceSpawnerGameLogic>();
    //  foreach (LanceSpawnerGameLogic sp in delayedSpawners) { spawners.Add(sp); }
    //  foreach (LanceSpawnerGameLogic sp in spawners) { sp.OnEnterActive(); }
    //}
    public static bool isObjectivesReady(this Contract contract) {
      return DeployManualHelper.IsInManualSpawnSequence == false;
    }
    //public static bool Prefix(LanceSpawnerGameLogic __instance) {
    //  Log.TW(0, "LanceSpawnerGameLogic.OnEnterActive " + __instance.Name + " HasUnitToSpawn:" + __instance.HasUnitToSpawn());
    //  foreach (UnitSpawnPointGameLogic unit in __instance.unitSpawnPointGameLogicList) { Log.W(1, unit.UnitDefId); }
    //  if (__instance.Combat.ActiveContract == null) { Log.WL(1, "ActiveContract is null"); return true; }
    //  if (__instance.Combat.ActiveContract.isManualSpawn() == false) { Log.WL(1, "not manual spawn"); return true; }
    //  if (__instance.unitSpawnPointGameLogicList.Length == 0) { Log.WL(1, "empty lance"); return true; }
    //  if (__instance.unitSpawnPointGameLogicList[0].mechDefId == CACConstants.DeployMechDefID) { Log.WL(1, "deploy director"); return true; }
    //  Log.WL(1, "delayed");
    //  delayedSpawners.Add(__instance);
    //  return false;
    //}
  }
  //[HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  //[HarmonyPatch("OnUnitSpawnComplete")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class LanceSpawnerGameLogic_OnUnitSpawnCompleteManual {
  //  public static void Postfix(LanceSpawnerGameLogic __instance) {
  //    if(__instance.Combat.ActiveContract.isManualSpawn() == false) {
  //      __instance.SetSpawnerReady();
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(ContractObjectiveGameLogic))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class ContractObjectiveGameLogic_Update {
  //  private static Dictionary<ContractObjectiveGameLogic, float> logInterval = new Dictionary<ContractObjectiveGameLogic, float>();
  //  public static void Clear() {
  //    ContractObjectiveGameLogic_Update.logInterval.Clear();
  //  }
  //  public static bool Prefix(ContractObjectiveGameLogic __instance) {
  //    if (__instance.Combat == null) { return true; }
  //    if (__instance.Combat.ActiveContract == null) { return true; }
  //    bool isObjectivesReady = __instance.Combat.ActiveContract.isObjectivesReady();
  //    if(logInterval.TryGetValue(__instance, out float t) == false) {
  //      t = 0f;
  //      logInterval.Add(__instance, t);
  //    }
  //    t += Time.deltaTime;
  //    if (t >= 1f) {
  //      t = 0f;
  //      Log.TWL(0, "ContractObjectiveGameLogic.Update "+__instance.title+" ready:"+ isObjectivesReady + " isManualSpawn:"+ __instance.Combat.ActiveContract.isManualSpawn()+" deplayedSpawns:"+ LanceSpawnerGameLogic_OnEnterActive.delayedSpawners.Count);
  //    }
  //    logInterval[__instance] = t;
  //    return isObjectivesReady;
  //  }
  //}
  //[HarmonyPatch(typeof(ObjectiveGameLogic))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class ObjectiveGameLogic_Update {
  //  private static Dictionary<ObjectiveGameLogic, float> logInterval = new Dictionary<ObjectiveGameLogic, float>();
  //  public static void Clear() {
  //    ObjectiveGameLogic_Update.logInterval.Clear();
  //  }
  //  public static bool Prefix(ObjectiveGameLogic __instance) {
  //    if (__instance.Combat == null) { return true; }
  //    if (__instance.Combat.ActiveContract == null) { return true; }
  //    bool isObjectivesReady = __instance.Combat.ActiveContract.isObjectivesReady();
  //    if (logInterval.TryGetValue(__instance, out float t) == false) {
  //      t = 0f;
  //      logInterval.Add(__instance, t);
  //    }
  //    t += Time.deltaTime;
  //    if (t >= 1f) {
  //      t = 0f;
  //      Log.TWL(0, "ObjectiveGameLogic.Update " + __instance.title + " ready:" + isObjectivesReady);
  //    }
  //    logInterval[__instance] = t;
  //    return isObjectivesReady;
  //  }
  //}
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("GetTaggedCombatants")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(TagSet), typeof(string) })]
  public static class ObjectiveGameLogic_GetTaggedCombatants0 {
    public static void Postfix(CombatGameState combatGameState, TagSet requiredTags, string lanceGuid, ref List<ICombatant> __result) {
      for (int index = 0; index < __result.Count; ++index) {
        if (__result[index].IsDeployDirector()) { __result.RemoveAt(index); break; }
      }
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("GetTaggedCombatants")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(TagSet) })]
  public static class ObjectiveGameLogic_GetTaggedCombatants1 {
    public static void Postfix(CombatGameState combatGameState, TagSet requiredTags, ref List<ICombatant> __result) {
      for (int index = 0; index < __result.Count; ++index) {
        if (__result[index].IsDeployDirector()) { __result.RemoveAt(index); break; }
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("DEBUG_DamageLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation), typeof(float), typeof(AbstractActor), typeof(DamageType), typeof(string) })]
  public static class Mech_DEBUG_DamageLocation {
    public static void Prefix(Mech __instance, ref ArmorLocation aLoc, float totalDamage, AbstractActor attacker, DamageType damageType, string attackerGUID) {
      try {
        if (__instance.IsDeployDirector()) { aLoc = ArmorLocation.None; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(AITeam))]
  [HarmonyPatch("getInvocationForCurrentUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AITeam_getInvocationForCurrentUnit {
    public static bool Prefix(AITeam __instance, AbstractActor ___currentUnit, ref InvocationMessage __result) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence == false) { return true; }
        Log.TWL(0, "AITeam.getInvocationForCurrentUnit deploy director still alive bracing");
        __result = (InvocationMessage)new ReserveActorInvocation(___currentUnit, ReserveActorAction.DONE, __instance.Combat.TurnDirector.CurrentRound);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(AITeam __instance, AbstractActor ___currentUnit, ref InvocationMessage __result) {
      try {
        Log.TWL(0, "AITeam.getInvocationForCurrentUnit "+(__result==null?"null": __result.GetType().Name));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorInfo))]
  [HarmonyPatch("UpdateItemVisibility")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActorInfo_UpdateItemVisibility {
    public static bool Prefix(CombatHUDActorInfo __instance) {
      try {
        if (DeployManualHelper.IsInManualSpawnSequence == false) { return true; }
        CustomDeploy.Core.HideAll(__instance);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }

  public class DeployDirectorLoadRequest {
    public PlayerLanceSpawnerGameLogic playerLanceSpawner;
    public UnitSpawnPointGameLogic spawnPoint;
    public Vector3 originalLocalPos;
    public MechDef originalMechDef;
    public PilotDef originalPilotDef;
    public Dictionary<UnitSpawnPointGameLogic, UnitType> originalUnitTypes;
    public SpawnUnitMethodType originalSpawnMethod;
    public Mech deployDirector { get; private set; }
    private bool isSpawned;
    public CombatHUD HUD;
    public void RestoreAndSpawn(List<DeployPosition> positions, Mech deployDirector, CombatHUD HUD) {
      if (isSpawned) { return; }
      isSpawned = true;
      this.deployDirector = deployDirector;
      FogOfWarSystem_WipeToValue.NormalFoW();
      LazySingletonBehavior<FogOfWarView>.Instance.FowSystem.WipeToValue(HUD.Combat.EncounterLayerData.startingFogOfWarVisibility);
      this.HUD = HUD;
      foreach (var origUnitType in originalUnitTypes) {
        origUnitType.Key.unitType = origUnitType.Value;
      }
      Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = originalPilotDef;
      spawnPoint.pilotDefId = originalPilotDef.Description.Id;
      if (spawnPoint.unitType == UnitType.Mech) {
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = originalMechDef;
        spawnPoint.mechDefId = originalMechDef.Description.Id;
      } else {
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = null;
        spawnPoint.mechDefId = string.Empty;
      }
      spawnPoint.LocalPosition = originalLocalPos;
      //playerLanceSpawner.Position = pos;
      float centerX = 0f;
      float centerZ = 0f;
      HashSet<Vector3> deployPositions = new HashSet<Vector3>();
      Log.TWL(0, "DeployDirectorLoadRequest.RestoreAndSpawn start pos generation", true);
      for (int index = 0; index < positions.Count; ++index) {
        centerX += positions[index].position.Value.x;
        centerZ += positions[index].position.Value.z;
      }
      Vector3 pos = new Vector3(centerX / positions.Count, 0f, centerZ / positions.Count);
      pos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(pos);
      playerLanceSpawner.Position = pos;
      //for (int index = 0; index < positions.Count; ++index) {
      //  if (index > playerLanceSpawner.unitSpawnPointGameLogicList.Length) { break; }
      //  centerX += positions[index].x;
      //  centerZ += positions[index].z;
      //  playerLanceSpawner.unitSpawnPointGameLogicList[index].Position = positions[index];
      //}
      //for (int index = 0; index < positions.Count; ++index) {
      //  positions[index].spawnPoint.Position = positions[index].position.Value;
      //}
      /*foreach(UnitSpawnPointGameLogic sp in playerLanceSpawner.unitSpawnPointGameLogicList) {
        Vector3 rndPos = pos;
        float nearest = 9999f;
        do {
          rndPos = pos;
          float radius = UnityEngine.Random.Range(0.5f * Core.Settings.DeploySpawnRadius, Core.Settings.DeploySpawnRadius);
          float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
          //radius *= UnityEngine.Random.Range(targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
          //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
          rndPos.x += Mathf.Sin(direction) * radius;
          rndPos.z += Mathf.Cos(direction) * radius;
          rndPos.y = HUD.Combat.MapMetaData.GetLerpedHeightAt(rndPos);
          rndPos = HUD.Combat.HexGrid.GetClosestPointOnGrid(rndPos);
          nearest = 9999f;
          foreach (Vector3 depPos in deployPositions) { float dist = Vector3.Distance(depPos, rndPos); if (nearest > dist) { nearest = dist; }; };
          Log.WL(1, rndPos.ToString() + " distance:" + Vector3.Distance(rndPos, pos) + " nearest:"+nearest+" rejected:"+(nearest < Core.Settings.DeploySpawnRadius / 4f), true);
        } while (nearest < Core.Settings.DeploySpawnRadius/4f);
        deployPositions.Add(rndPos);
        sp.Position = rndPos;
      }*/
      playerLanceSpawner.spawnUnitsOnActivation = true;
      playerLanceSpawner.spawnMethod = originalSpawnMethod;
      this.playerLanceSpawner.Combat.ActiveContract.ClearManualSpawn();
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
      //this.playerLanceSpawner.Combat.ActiveContract.ActivateDelayed();
    }
    public void MechDependenciesLoaded() {
      Log.TWL(0, "DeployDirectorLoadRequest.MechDependenciesLoaded");
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void PilotDependenciesLoaded() {
      Log.TWL(0, "DeployDirectorLoadRequest.PilotDependenciesLoaded");
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if (mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "mechDef do not have all dependencies");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void DirectorPilotDefLoaded(string id, PilotDef pilotDef) {
      Log.TWL(0, "DeployDirectorLoadRequest.DirectorPilotDefLoaded " + id);
      spawnPoint.pilotDefId = pilotDef.Description.Id;
      Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = pilotDef;
      if (pilotDef.DependenciesLoaded(1000u) == false) {
        Log.WL(0, "not all dependencies loaded. Injecting loading");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
        pilotDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(this.PilotDependenciesLoaded));
        playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
        return;
      }
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if (mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "mechDef do not have all dependencies");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
    public void DirectorMechDefLoaded(string id, MechDef def) {
      Log.TWL(0, "DeployDirectorLoadRequest.DirectorMechDefLoaded " + id);
      Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = def;
      if (def.DependenciesLoaded(1000u) == false) {
        Log.WL(0, "not all dependencies loaded. Injecting loading");
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
        def.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
        playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      } else {
        Log.WL(0, "dependencies loaded. Processing.");
        playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
      }
    }
    public DeployDirectorLoadRequest(PlayerLanceSpawnerGameLogic playerLanceSpawner) {
      Log.TWL(0, "DeployDirectorLoadRequest.DeployDirectorLoadRequest");
      isSpawned = false;
      FogOfWarSystem_WipeToValue.RevealFoW();
      UnitSpawnPointGameLogic[] pointGameLogicList = playerLanceSpawner.unitSpawnPointGameLogicList;
      this.playerLanceSpawner = playerLanceSpawner;
      this.spawnPoint = pointGameLogicList[0];
      this.originalSpawnMethod = playerLanceSpawner.spawnMethod;
      this.originalUnitTypes = new Dictionary<UnitSpawnPointGameLogic, UnitType>();
      this.originalLocalPos = this.spawnPoint.LocalPosition;
      //this.spawnPoint.Position = Vector3.zero;
      for (int index = 0; index < pointGameLogicList.Length; ++index) {
        originalUnitTypes.Add(pointGameLogicList[index], pointGameLogicList[index].unitType);
        pointGameLogicList[index].unitType = UnitType.UNDEFINED;
      }
      this.originalMechDef = Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value;
      this.originalPilotDef = Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value;
      playerLanceSpawner.spawnMethod = SpawnUnitMethodType.InstantlyAtSpawnPoint;
      spawnPoint.unitType = UnitType.Mech;
      PilotDef pilotDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<PilotDef>(DeployManualHelper.DeployPilotDefID, BattleTechResourceType.PilotDef);
      if (pilotDef != null) {
        Log.WL(0, "pilotDef present");
        spawnPoint.pilotDefId = pilotDef.Description.Id;
        Traverse.Create(spawnPoint).Field<PilotDef>("pilotDefOverride").Value = pilotDef;
        if (pilotDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "not all dependencies loaded. Injecting loading");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          pilotDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.PilotDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "pilotDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, DeployManualHelper.DeployPilotDefID, this.DirectorPilotDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if (mechDef != null) {
        Log.WL(0, "mechDef present");
        spawnPoint.mechDefId = mechDef.Description.Id;
        Traverse.Create(spawnPoint).Field<MechDef>("mechDefOverride").Value = mechDef;
        if (mechDef.DependenciesLoaded(1000u) == false) {
          Log.WL(0, "not all dependencies loaded. Injecting loading");
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(playerLanceSpawner.Combat.DataManager);
          mechDef.GatherDependencies(playerLanceSpawner.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(this.MechDependenciesLoaded));
          playerLanceSpawner.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
          return;
        }
      } else {
        Log.WL(0, "mechDef not present");
        LoadRequest request = playerLanceSpawner.Combat.DataManager.CreateLoadRequest(null, false);
        request.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, CACConstants.DeployMechDefID, this.DirectorMechDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      Log.WL(0, "LanceSpawnerGameLogic_OnEnterActive()");
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
    }
  }
  [HarmonyPatch(typeof(PlayerLanceSpawnerGameLogic))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnEnterActive")]
  [HarmonyPatch(new Type[] { })]
  public static class PlayerLanceSpawnerGameLogic_OnEnterActive {
    public delegate void d_LanceSpawnerGameLogic_OnEnterActive(LanceSpawnerGameLogic spawner);
    private static d_LanceSpawnerGameLogic_OnEnterActive i_LanceSpawnerGameLogic_OnEnterActive = null;
    //public static readonly string DeployMechDefID = "mechdef_deploy_director";
    //public static DeployDirectorLoadRequest deployLoadRequest { get; set; } = null;
    //public static void Clear() { deployLoadRequest = null; }
    public static void ResetDeployButton(this CombatHUDMechwarriorTray tray, AbstractActor actor, Ability ability, CombatHUDActionButton button, bool forceInactive) {
      CustomAmmoCategoriesLog.Log.LogWrite("ResetDeployButton:" + actor.DisplayName + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.HasActivatedThisRound:" + actor.HasActivatedThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.MovingToPosition:" + (actor.MovingToPosition != null) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" actor.Combat.TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" forceInactive:" + forceInactive + "\n");
      tray.ResetAbilityButton(actor, button, ability, forceInactive);
      if (forceInactive) { button.DisableButton(); };
      if (actor.Combat.TurnDirector.IsInterleaved == false) {
        if (actor.HasFiredThisRound == false) {
          if (ability.IsActive == false) {
            if (ability.IsAvailable == true) {
              if (actor.IsShutDown == false) {
                CustomAmmoCategoriesLog.Log.LogWrite(" ResetButtonIfNotActive:\n");
                CustomAmmoCategoriesLog.Log.LogWrite(" IsAbilityActivated:" + button.IsAbilityActivated + "\n");
                if (actor.MovingToPosition == null) { button.ResetButtonIfNotActive(actor); };
              }
            }
          }
        } else {
          button.DisableButton();
        }
      }
    }
    public static bool Prepare() {
      Log.TWL(0, "PlayerLanceSpawnerGameLogic.OnEnterActive.Prepare");
      {
        MethodInfo OnEnterActive = typeof(LanceSpawnerGameLogic).GetMethod("OnEnterActive", BindingFlags.Instance | BindingFlags.Public);
        var dm = new DynamicMethod("CACOnEnterActive", null, new Type[] { typeof(LanceSpawnerGameLogic) }, typeof(LanceSpawnerGameLogic));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, OnEnterActive);
        gen.Emit(OpCodes.Ret);
        i_LanceSpawnerGameLogic_OnEnterActive = (d_LanceSpawnerGameLogic_OnEnterActive)dm.CreateDelegate(typeof(d_LanceSpawnerGameLogic_OnEnterActive));
      }
      return true;
    }
    public static void LanceSpawnerGameLogic_OnEnterActive(this LanceSpawnerGameLogic spawner) {
      i_LanceSpawnerGameLogic_OnEnterActive(spawner);
    }
    public static bool Prefix(PlayerLanceSpawnerGameLogic __instance) {
      Log.TWL(0, "PlayerLanceSpawnerGameLogic.OnEnterActive");
      DeployManualHelper.NeedSpawnProtection = false;
      try {
        //typeof(HBS.DebugConsole.DebugConsole).GetProperty("DebugCommandsUnlocked", BindingFlags.Static | BindingFlags.Public).GetSetMethod(true).Invoke(null, new object[] { true });
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      //DeployManualHelper.IsInManualSpawnSequence = false;
      if (__instance.teamDefinitionGuid != __instance.Combat.LocalPlayerTeamGuid) { return true; }
      if (__instance.Combat.ActiveContract.isManualSpawn() == false) { return true; }
      //DeployManualHelper.IsInManualSpawnSequence = true;

      EncounterLayerParent encounterLayerParent = __instance.Combat.EncounterLayerData.gameObject.GetComponentInParent<EncounterLayerParent>();
      DropShipManager dropManager = encounterLayerParent.gameObject.GetComponent<DropShipManager>();
      if (dropManager == null) { dropManager = encounterLayerParent.gameObject.AddComponent<DropShipManager>(); }
      dropManager.Init(__instance);

      DeployManualHelper.originalSpawnMethod = __instance.spawnMethod;
      __instance.spawnMethod = SpawnUnitMethodType.InstantlyAtSpawnPoint;
      DeployManualHelper.SpawnDeployDirector(__instance);
      FogOfWarSystem_WipeToValue.RevealFoW();
      DeployManualHelper.RebuildAllVisChaches(__instance.Combat);
      //deployLoadRequest = new DeployDirectorLoadRequest(__instance);
      return true;
      //return true;
    }
  }
}