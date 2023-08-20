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
using BattleTech.Framework;
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.UI;
using HarmonyLib;
using HBS;
using Localize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("GeneratePotentialContracts")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_GeneratePotentialContracts {
    private static IEnumerator StartGeneratePotentialContractsRoutine(this SimGameState sim, bool clearExistingContracts, Action onContractGenComplete, StarSystem systemOverride, bool useWait) {
      Log.M?.TWL(0, "SimGameState.StartGeneratePotentialContractsRoutine");
      int debugCount = 0;
      bool usingBreadcrumbs = systemOverride != null;
      if (useWait) { yield return (object)new WaitForSeconds(0.2f); }
      StarSystem system = null;
      List<Contract> contractList = null;
      SimGameState.ContractDifficultyRange difficultyRange = null;
      WeightedList<MapAndEncounters> playableMaps = null;
      int maxContracts = 0;
      Dictionary<int, List<ContractOverride>> potentialContracts = null;
      Dictionary<string, WeightedList<SimGameState.ContractParticipants>> validParticipants = null;
      try {
        if (usingBreadcrumbs) {
          system = systemOverride;
          contractList = sim.CurSystem.SystemBreadcrumbs;
          maxContracts = sim.CurSystem.CurMaxBreadcrumbs;
        } else {
          system = sim.CurSystem;
          contractList = sim.CurSystem.SystemContracts;
          maxContracts = Mathf.CeilToInt(system.CurMaxContracts);
        }
        if (clearExistingContracts) { contractList.Clear(); }
        difficultyRange = sim.GetContractRangeDifficultyRange(system, sim.SimGameMode, sim.GlobalDifficulty);// .GetContractRangeDifficultyRange(system, sim.SimGameMode, sim.GlobalDifficulty);
        potentialContracts = sim.GetSinglePlayerProceduralContractOverrides(difficultyRange);
        playableMaps = MetadataDatabase.Instance.GetReleasedMapsAndEncountersBySinglePlayerProceduralContractTypeAndTags(system.Def.MapRequiredTags, system.Def.MapExcludedTags, system.Def.SupportedBiomes, true).ToWeightedList<MapAndEncounters>(WeightedListType.SimpleRandom);
        validParticipants = sim.GetValidParticipants(system);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
      bool genComplete = true;
      try {
        genComplete = !sim.HasValidMaps(system, playableMaps) || !sim.HasValidContracts(difficultyRange, potentialContracts) || !sim.HasValidParticipants(system, validParticipants);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        SimGameState.logger.LogException(e);
      }
      if (genComplete) {
        if (onContractGenComplete != null)
          onContractGenComplete();
      } else {
        try {
          sim.ClearUsedBiomeFromDiscardPile(playableMaps);
        } catch (Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
          SimGameState.logger.LogException(e);
        }
        while (contractList.Count < maxContracts && debugCount < 1000) {
          try {
            ++debugCount;
            IEnumerable<int> source = playableMaps.Select<MapAndEncounters, int>((Func<MapAndEncounters, int>)(map => map.Map.Weight));
            WeightedList<MapAndEncounters> activeMaps = new WeightedList<MapAndEncounters>(WeightedListType.WeightedRandom, playableMaps.ToList(), source.ToList<int>());
            sim.FilterActiveMaps(activeMaps, contractList);
            activeMaps.Reset();
            MapAndEncounters next = activeMaps.GetNext(false);
            SimGameState.MapEncounterContractData MapEncounterContractData;
            for (MapEncounterContractData = sim.FillMapEncounterContractData(system, difficultyRange, potentialContracts, validParticipants, next); !MapEncounterContractData.HasContracts && activeMaps.ActiveListCount > 0; MapEncounterContractData = sim.FillMapEncounterContractData(system, difficultyRange, potentialContracts, validParticipants, next))
              next = activeMaps.GetNext(false);
            system.SetCurrentContractFactions();
            if (MapEncounterContractData == null || MapEncounterContractData.Contracts.Count == 0) {
              if (sim.mapDiscardPile.Count > 0) {
                sim.mapDiscardPile.Clear();
              } else {
                debugCount = 1000;
                SimGameState.logger.LogError((object)string.Format("[CONTRACT] Unable to find any valid contracts for available map pool. Alert designers.", (object[])Array.Empty<object>()));
              }
            }
            GameContext gameContext = new GameContext((GameContext)sim.Context);
            gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, (object)system);
            contractList.Add(sim.CreateProceduralContract(system, usingBreadcrumbs, next, MapEncounterContractData, gameContext));
          } catch (Exception e) {
            Log.M?.TWL(0, e.ToString(), true);
            SimGameState.logger.LogException(e);
          }
          if (useWait) { yield return (object)new WaitForSeconds(0.2f); }
        }
        if (debugCount >= 1000)
          SimGameState.logger.LogError((object)"Unable to fill contract list. Please inform AJ Immediately");
        if (onContractGenComplete != null)
          onContractGenComplete();
      }
    }
    public static bool Prefix(SimGameState __instance, bool clearExistingContracts, Action onContractGenComplete, StarSystem systemOverride, bool useCoroutine) {
      //Log.TWL(0, "SimGameState.GeneratePotentialContracts");
      //SceneSingletonBehavior<UnityGameInstance>.Instance.StartCoroutine(__instance.StartGeneratePotentialContractsRoutine(clearExistingContracts, onContractGenComplete, systemOverride, useCoroutine));
      return true;
    }
  }

  [HarmonyPatch(typeof(EncounterLayerParent))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class EncounterLayerParent_Update {
    public static void Postfix(EncounterLayerParent __instance) {
      if (Application.isPlaying == false) { return; }
      HotDropManager hotdropManager = __instance.gameObject.GetComponent<HotDropManager>();
      if (hotdropManager == null) { hotdropManager = __instance.gameObject.AddComponent<HotDropManager>(); }
      hotdropManager.parent = __instance;
      hotdropManager.UpdateDropPodPrefabs();
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetMechwarriorButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_ResetMechwarriorButtons_dbg {
    public static void Prefix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Log.Combat?.TWL(0, "CombatHUDMechwarriorTray.ResetMechwarriorButtons debug:" + (actor == null ? "null" : actor.DisplayName));
      if (actor != null) {
        Log.Combat?.WL(1, "actor.HasActivatedThisRound:" + actor.HasActivatedThisRound);
        Log.Combat?.WL(1, "actor.IsAvailableThisPhase:" + actor.IsAvailableThisPhase);
        Log.Combat?.WL(1, "Combat.StackManager.IsAnyOrderActive:" + actor.Combat.StackManager.IsAnyOrderActive);
        Log.Combat?.WL(1, "TurnDirector.IsInterleaved:" + actor.Combat.TurnDirector.IsInterleaved);
      }
    }
  }
  [HarmonyPatch(typeof(EncounterLayerParent))]
  [HarmonyPatch("InitializeContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class EncounterLayerParent_InitializeContract {
    public static void Postfix(EncounterLayerParent __instance, MessageCenterMessage message) {
      InitializeContractMessage initializeContractMessage = message as InitializeContractMessage;
      HotDropManager hotdropManager = __instance.gameObject.GetComponent<HotDropManager>();
      if (hotdropManager == null) { hotdropManager = __instance.gameObject.AddComponent<HotDropManager>(); }
      hotdropManager.parent = __instance;
      hotdropManager.Combat = initializeContractMessage.combat;
      hotdropManager.UpdateDropped();
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init_Hotdrop {
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      EncounterLayerParent encounterLayerParent = Combat.EncounterLayerData.gameObject.GetComponentInParent<EncounterLayerParent>();
      HotDropManager hotdropManager = encounterLayerParent.gameObject.GetComponent<HotDropManager>();
      if (hotdropManager == null) { hotdropManager = encounterLayerParent.gameObject.AddComponent<HotDropManager>(); }
      hotdropManager.parent = encounterLayerParent;
      hotdropManager.Combat = Combat;
      hotdropManager.HUD = __instance;
      hotdropManager.UpdateDropped();
    }
  }
  public class HotDropManager : MonoBehaviour {
    public EncounterLayerParent parent { get; set; } = null;
    public CombatGameState Combat { get; set; } = null;
    public CombatHUD HUD { get; set; } = null;
    public List<HotDropPoint> dropPoints { get; set; } = new List<HotDropPoint>();
    public void UpdateDropPodPrefabs() {
      HotDropPoint[] drops = this.gameObject.GetComponentsInChildren<HotDropPoint>(true);
      foreach (HotDropPoint drop in drops) {
        drop.LoadDropPodPrefabs(parent.DropPodVfxPrefab, parent.dropPodLandedPrefab);
      }
    }
    public void Awake() {
      HotDropPoint[] drops = this.gameObject.GetComponentsInChildren<HotDropPoint>(true);
      for (int t = CustomLanceHelper.hotdropLayout.Count; t < drops.Length; ++t) {
        drops[t].dropDef = null;
        drops[t].unit = null;
      }
      for (int t = drops.Length; t < CustomLanceHelper.hotdropLayout.Count; ++t) {
        GameObject drop = new GameObject("hotdropPoint" + t.ToString(), typeof(HotDropPoint));
        drop.transform.SetParent(this.gameObject.transform);
      }
      drops = this.gameObject.GetComponentsInChildren<HotDropPoint>(true);
      dropPoints.Clear();
      for (int t = 0; t < CustomLanceHelper.hotdropLayout.Count; ++t) {
        dropPoints.Add(drops[t]);
        drops[t].dropDef = CustomLanceHelper.hotdropLayout[t];
        drops[t].parent = this;
        drops[t].unit = null;
      }
    }
    public void UpdateDropped() {
      if (this.Combat == null) { return; }
      List<AbstractActor> units = this.Combat.AllActors;
      foreach (AbstractActor unit in units) {
        string GUID = unit.PilotableActorDef.GUID + "_" + unit.PilotableActorDef.Description.Id + "_" + unit.GetPilot().Description.Id;
        foreach (HotDropPoint dropPoint in dropPoints) {
          if (dropPoint.dropDef == null) { continue; }
          if (dropPoint.InDroping) { continue; }
          string dropGUID = dropPoint.dropDef.mechDef.GUID + "_" + dropPoint.dropDef.mechDef.Description.Id + "_" + dropPoint.dropDef.pilot.Description.Id;
          if (GUID == dropGUID) { dropPoint.unit = unit; }
        }
      }
    }
    public void OnDropComplete() {
      this.Combat.RebuildAllLists();
      if (refreshHUDCheck == false) { return; }
      bool refreshTeam = true;
      foreach (HotDropPoint dropPoint in this.dropPoints) {
        if (dropPoint.dropDef == null) { continue; }
        if (dropPoint.InDroping) { refreshTeam = false; break; }
      }
      if (refreshTeam) {
        if (HUD != null) { HUD.MechWarriorTray.RefreshTeam(this.Combat.LocalPlayerTeam); refreshHUDCheck = false; }
      }
    }
    private bool refreshHUDCheck = false;
    public static void DeferredHotDrop(Weapon weapon, Vector3 pos) {
      EncounterLayerParent encounterLayerParent = weapon.parent.Combat.EncounterLayerData.GetComponentInParent<EncounterLayerParent>();
      HotDropManager hotdropManager = encounterLayerParent.GetComponent<HotDropManager>();
      hotdropManager.HotDrop(new List<Vector3>() { pos }, weapon.parent.GUID);
    }

    public static bool DeferredHotDrop(AbstractActor parent, List<Vector3> positions) {
      EncounterLayerParent encounterLayerParent = parent.Combat.EncounterLayerData.GetComponentInParent<EncounterLayerParent>();
      HotDropManager hotDropManager = encounterLayerParent.GetComponent<HotDropManager>();

      hotDropManager.HotDrop(positions, parent.GUID);
      return true;
    }

    public static bool DeferredHotDrop(AbstractActor parent, Vector3 pos) {
      EncounterLayerParent encounterLayerParent = parent.Combat.EncounterLayerData.GetComponentInParent<EncounterLayerParent>();
      HotDropManager hotdropManager = encounterLayerParent.GetComponent<HotDropManager>();
      GenericPopup popup = null;

      StringBuilder text = new StringBuilder();
      List<KeyValuePair<int, HotDropDefinition>> invList = new List<KeyValuePair<int, HotDropDefinition>>();
      for(int t=0;t< hotdropManager.dropPoints.Count; ++t) { 
        if (hotdropManager.dropPoints[t].dropDef == null) { continue; }
        if (hotdropManager.dropPoints[t].unit != null) { continue; }
        if (hotdropManager.dropPoints[t].InDroping) { continue; }
        invList.Add(new KeyValuePair<int, HotDropDefinition>(t, hotdropManager.dropPoints[t].dropDef));
      }
      if (invList.Count <= 0) {
        popup = GenericPopupBuilder.Create("NO UNIT TO HOTDROP", "You have empty hotdrop queue").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
        return false;
      }
      int curIndex = 0;
      int pageSize = 10;
      for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
        if (index >= invList.Count) { break; }
        if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
        text.Append(new Text(invList[index].Value.mechDef.Name).ToString() + ":" + invList[index].Value.pilot.Callsign);
        if (index == curIndex) { text.Append("</size></color>"); };
        text.AppendLine();
      }

      popup = GenericPopupBuilder.Create("CHOOSE UNIT TO HOTDROP", text.ToString())
        .AddButton("X", (Action)(() => { }), true)
        .AddButton("->", (Action)(() => {
          try {
            if (curIndex < (invList.Count - 1)) {
              ++curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(new Text(invList[index].Value.mechDef.Name).ToString() + ":" + invList[index].Value.pilot.Callsign);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
          }
        }), false)
        .AddButton("<-", (Action)(() => {
          try {
            if (curIndex > 0) {
              --curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(new Text(invList[index].Value.mechDef.Name).ToString() + ":" + invList[index].Value.pilot.Callsign);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.Combat?.TWL(0, e.ToString(), true);
          }
        }), false).AddButton("DROP", (Action)(() => {
          hotdropManager.HotDrop(invList[curIndex].Key, pos, parent.GUID);
          parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(parent.DoneWithActor()));
        }), true).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();

      return true;
    }
    public void HotDrop(int index, Vector3 pos, string spawnGUID) {
      if (this.Combat == null) { return; }
      if (string.IsNullOrEmpty(spawnGUID)) { throw new Exception("spawnGUID should not be empty"); }
      this.UpdateDropped();
      Log.Combat?.TWL(0, "HotDropManager.HotDrop " + index);
      if ((index < 0) || (index >= dropPoints.Count)) { return; }
      if (dropPoints[index].dropDef == null) { return; }
      if (dropPoints[index].unit != null) { return; }
      if (dropPoints[index].InDroping) { return; }
      if (dropPoints[index].dropDef.TeamGUID == this.Combat.LocalPlayerTeamGuid) { refreshHUDCheck = true; }
      dropPoints[index].HotDrop(pos, OnDropComplete, spawnGUID);
    }
    public void HotDrop(List<Vector3> positions, string spawnGUID) {
      if (this.Combat == null) { return; }
      if (string.IsNullOrEmpty(spawnGUID)) { throw new Exception("spawnGUID should not be empty"); }
      this.UpdateDropped();
      Log.Combat?.TWL(0, "HotDropManager.HotDrop");
      foreach (Vector3 pos in positions) {
        foreach (HotDropPoint dropPoint in this.dropPoints) {
          Log.Combat?.WL(1, "dropDef:" + (dropPoint.dropDef == null ? "null" : dropPoint.dropDef.mechDef.Description.Id) + " unit:" + (dropPoint.unit == null ? "null" : dropPoint.unit.DisplayName) + " InDroping:" + dropPoint.InDroping);
          if (dropPoint.dropDef == null) { continue; }
          if (dropPoint.unit != null) { continue; }
          if (dropPoint.InDroping) { continue; }
          if (dropPoint.dropDef.TeamGUID == this.Combat.LocalPlayerTeamGuid) { refreshHUDCheck = true; }
          dropPoint.HotDrop(pos, OnDropComplete, spawnGUID);
          break;
        }
      }
    }
  }
  public class HotDropPoint : MonoBehaviour {
    public AbstractActor unit { get; set; } = null;
    public HotDropManager parent { get; set; } = null;
    public HotDropDefinition dropDef { get; set; } = null;
    public bool InDroping { get; set; } = false;
    public ParticleSystem dropPodVfxPrefab { get; set; }
    public GameObject dropPodLandedPrefab { get; set; }
    public Vector3 offscreenDropPodPosition { get; set; } = Vector3.zero;
    public string spawnGUID { get; set; }
    public Action onDropCompleete { get; set; }
    public void OnDepsLoaded() {
      Team team = UnityGameInstance.BattleTechGame.Combat.GetLoadedTeamByGUID(dropDef.TeamGUID);
      if (team == null) { team = UnityGameInstance.BattleTechGame.Combat.LocalPlayerTeam; }
      if (string.IsNullOrEmpty(this.spawnGUID)) { this.spawnGUID = team.GUID; }
      Mech mech = ActorFactory.CreateMech(dropDef.mechDef, dropDef.pilot.pilotDef, team.EncounterTags, UnityGameInstance.BattleTechGame.Combat, Guid.NewGuid().ToString(), this.spawnGUID, team.HeraldryDef);
      mech.Init(this.transform.position, 0f, true);
      mech.InitGameRep((Transform)null);
      team.AddUnit(mech);
      Lance spawnLance = null;
      foreach (Lance lance in team.lances) {
        spawnLance = lance;
        break;
      }
      if (spawnLance != null) {
        spawnLance.AddUnitGUID(mech.GUID);
        mech.AddToLance(spawnLance);
      }
      mech.OnPlayerVisibilityChanged(VisibilityLevel.None);
      UnitSpawnedMessage unitSpawnedMessage = new UnitSpawnedMessage(this.spawnGUID, mech.GUID);
      mech.OnPositionUpdate(this.transform.position, Quaternion.identity, -1, true, (List<DesignMaskDef>)null, false);
      mech.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(mech.Combat.BattleTechGame, mech, BehaviorTreeIDEnum.DoNothingTree); ;
      UnityGameInstance.BattleTechGame.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new UnitSpawnedMessage("HOTDROP_SPAWNER", mech.GUID));
      Log.Combat?.WL(1, "spawned:" + mech.PilotableActorDef.Description.Id + ":" + mech.pilot.Description.Id + " " + mech.CurrentPosition);
      this.unit = mech;
      unit.PlaceFarAwayFromMap();
      this.parent.Combat.ItemRegistry.AddItem(unit);
      this.parent.Combat.RebuildAllLists();
      this.StartCoroutine(this.StartDropPodAnimation(0f, onDropCompleete));
    }
    public void HotDrop(Vector3 pos, Action onDropEnd, string sGUID) {
      if (this.dropDef == null) { return; }
      if (this.unit != null) { return; }
      this.InDroping = true;
      this.spawnGUID = sGUID;
      this.onDropCompleete = onDropEnd;
      this.gameObject.transform.position = pos;
      Log.Combat?.TWL(0, "HotDropPoint.HotDrop:" + pos + " " + this.dropDef.mechDef.Description.Id + ":deps - " + this.dropDef.mechDef.DependenciesLoaded(1000u) + " pilot:" + this.dropDef.pilot.pilotDef.Description.Id + " deps - " + this.dropDef.pilot.pilotDef.DependenciesLoaded(1000u));
      if (
        (this.dropDef.mechDef.DependenciesLoaded(1000u) == false)
        || (this.dropDef.pilot.pilotDef.DependenciesLoaded(1000u) == false)
        ) {
        DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(this.parent.Combat.DataManager);
        dependencyLoad.RegisterLoadCompleteCallback(new Action(this.OnDepsLoaded));
        if (this.dropDef.mechDef.DependenciesLoaded(1000u) == false) {
          this.dropDef.mechDef.GatherDependencies(this.parent.Combat.DataManager, dependencyLoad, 1000u);
        }
        if (this.dropDef.pilot.pilotDef.DependenciesLoaded(1000u) == false) {
          this.dropDef.pilot.pilotDef.GatherDependencies(this.parent.Combat.DataManager, dependencyLoad, 1000u);
        }
        this.parent.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
      } else {
        this.OnDepsLoaded();
      }
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
        Log.Combat?.TWL(0, "Null drop pod animation for this biome.");
      }
      yield return (object)new WaitForSeconds(1f);
      int num2 = (int)WwiseManager.PostEvent<AudioEventList_play>(AudioEventList_play.play_dropPod_impact, WwiseManager.GlobalAudioObject);
      yield return (object)this.DestroyFlimsyObjects();
      //yield return (object)this.ApplyDropPodDamageToSquashedUnits(sequenceGUID, rootSequenceGUID);
      yield return (object)new WaitForSeconds(3f);
      this.TeleportUnitToSpawnPoint();
      yield return (object)new WaitForSeconds(2f);
      this.InDroping = false;
      this.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(this.unit.DoneWithActor()));
      unitDropPodAnimationComplete();
    }
    public void TeleportUnitToSpawnPoint() {
      Vector3 spawnPosition = this.gameObject.transform.position;
      if (this.dropPodLandedPrefab != null) {
        this.dropPodLandedPrefab.transform.position = spawnPosition;
        this.dropPodLandedPrefab.transform.rotation = this.transform.rotation;
      }
      this.unit.TeleportActor(spawnPosition);
      this.unit.GameRep.FadeIn(1f);
      this.unit.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
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
}