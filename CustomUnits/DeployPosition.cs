using BattleTech;
using BattleTech.Data;
using BattleTech.Designed;
using BattleTech.Framework;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using FogOfWar;
using Harmony;
using HBS;
using HBS.Collections;
using InControl;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
  public class CombatHUDDeployAutoActivator: MonoBehaviour {
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
      if (HUD.SelectedActor.IsDeployDirector() == false) { return; }
      button.OnClick();
    }
  }
  public class SelectionStateCommandDeploy : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated = false;
    //public float CircleRange = 50f;
    public Vector3 goodPos;
    public SelectionStateCommandDeploy(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
      HasActivated = false;
    }
    protected override bool ShouldShowWeaponsUI { get { return false; } }
    protected override bool ShouldShowTargetingLines { get { return false; } }
    protected override bool ShouldShowFiringArcs { get { return false; } }
    public override bool ConsumesMovement { get { return false; } }
    public override bool ConsumesFiring { get { return false; } }
    public override bool CanBackOut { get { return false; } }
    protected override bool showHeatWarnings { get { return false; } }
    public virtual bool HasCalledShot { get { return false; } }
    public virtual bool NeedsCalledShot { get { return false; } }
    public override bool CanActorUseThisState(AbstractActor actor) { return actor.IsDeployDirector(); }
    public override bool CanDeselect { get { return false; } }
    public override void OnAddToStack() {
      base.OnAddToStack();
    }
    public override void OnInactivate() {
      Log.TWL(0, "SelectionStateCommandDeploy.OnInactivate HasActivated: " + HasActivated);
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
      Mech deployDirector = HUD.SelectedActor as Mech;
      PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.RestoreAndSpawn(this.targetPosition, deployDirector, this.HUD);
      return true;
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      if (this.NumPositionsLocked != 0) { return false; }
      float originalDist = Vector3.Distance(worldPos, PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.Position);
      List<Vector3> enemies = this.Combat.ActiveContract.deplayedEnemySpawnPositions();
      float nearesEnemyDist = 9999f;
      foreach (Vector3 unit in enemies) {
        float dist = Vector3.Distance(worldPos, unit);
        if ((nearesEnemyDist == 9999f) || (dist < nearesEnemyDist)) { nearesEnemyDist = dist; }
      }
      if ((originalDist < Core.Settings.DeployMaxDistanceFromOriginal) || (nearesEnemyDist > Core.Settings.DeployMinDistanceFromEnemy)) {
        this.targetPosition = worldPos;
        ++this.NumPositionsLocked;
        this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
        return true;
      } else {
        GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Drop point too close to enemy position "+ Mathf.Round(nearesEnemyDist)+" < "+ Core.Settings.DeployMinDistanceFromEnemy+"\nand too far from suggested deploy "+Mathf.Round(originalDist)+" > "+ Core.Settings.DeployMaxDistanceFromOriginal);
        popup.AddButton("Ok", (Action)null, true, (PlayerAction)null);
        popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        return false;
      }
    }
    public override Vector3 PreviewPos {
      get {
        return this.SelectedActor.CurrentPosition;
      }
    }
    public override void ProcessMousePos(Vector3 worldPos) {
      switch (this.NumPositionsLocked) {
        case 0: {
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, Core.Settings.DeploySpawnRadius, false);
        };break;
        case 1:
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, Core.Settings.DeploySpawnRadius, false);
        break;
      }
      Log.TWL(0, "SelectionState_ProcessMousePos");
      this.SelectionState_ProcessMousePos(SelectedActor.CurrentPosition);
    }
    public override int ProjectedHeatForState { get { return 0; } }
  };
  [HarmonyPatch(typeof(MessageCenter))]
  [HarmonyPatch("PublishMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MissionFailedMessage_Constructor {
    public static void Postfix(MessageCenter __instance, MessageCenterMessage message) {
      if ((message.MessageType != MessageCenterMessageType.OnMissionSucceeded)
       && (message.MessageType != MessageCenterMessageType.OnMissionFailed)
       && (message.MessageType != MessageCenterMessageType.OnMissionRetreat)
      ) { return; }
      Log.TWL(0, "MessageCenter.PublishMessage " + message.MessageType);
      Log.WL(0, Environment.StackTrace);
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByType {
    public static bool SelectionForbidden = false;
    public static bool Prefix(SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      Log.TWL(0,"SelectionState.GetNewSelectionStateByType " + type + ":" + FromButton.GUID);
      if ((type == SelectionType.CommandTargetSinglePoint) && (FromButton.GUID == PlayerLanceSpawnerGameLogic_OnEnterActive.DeployAbilityDefID)) {
        Log.WL(1,"creating own selection state");
        __result = new SelectionStateCommandDeploy(Combat, HUD, FromButton, actor);
        return false;
      }
      return true;
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
      if((PathingCaps == null)&&(actor.PathingCaps != null)) {
        Traverse.Create(__instance).Property<PathingCapabilitiesDef>("PathingCaps").Value = actor.PathingCaps;
      }
      Log.TWL(0, "Pathing.ResetPathGridIfTouching " + actor.DisplayName + " caps:" + (actor.PathingCaps == null ? "null" : actor.PathingCaps.Description.Id) + " pathing:" + (PathingCaps == null ? "null" : PathingCaps.Description.Id));
      return true;
    }
  }
  [HarmonyPatch(typeof(TriggerSpawn))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class TriggerSpawn_Constructor {
    public static void Postfix(TriggerSpawn __instance, string spawnerGUID) {
      Log.TWL(0, "TriggerSpawn("+ spawnerGUID + ")");
      Log.WL(0,Environment.StackTrace);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("VisibilityToTargetUnit")]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class AbstractActor_VisibilityToTargetUnit {
    public static void Postfix(AbstractActor __instance, ICombatant targetUnit, ref VisibilityLevel __result) {
      if (__result == VisibilityLevel.None) { return; }
      if (__instance.IsDeployDirector()) { __result = VisibilityLevel.None; return; }
      if (targetUnit.IsDeployDirector()) { __result = VisibilityLevel.None; return; }
    }
  }
  [HarmonyPatch(typeof(SharedVisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class SharedVisibilityCache_RebuildCache {
    public static void Prefix(SharedVisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        for(int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RebuildCache")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_RebuildCache {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (Traverse.Create(__instance).Property<AbstractActor>("OwningActor").Value.IsDeployDirector()) { return false; }
        for (int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(VisibilityCache))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateCacheReciprocal")]
  [HarmonyPatch(new Type[] { typeof(List<ICombatant>) })]
  public static class VisibilityCache_UpdateCacheReciprocal {
    public static bool Prefix(VisibilityCache __instance, ref List<ICombatant> allLivingCombatants) {
      try {
        if (Traverse.Create(__instance).Property<AbstractActor>("OwningActor").Value.IsDeployDirector()) { return false; }
        for (int index = 0; index < allLivingCombatants.Count; ++index) {
          if (allLivingCombatants[index].IsDeployDirector()) { allLivingCombatants.RemoveAt(index); break; }
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SpawnMech")]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(Team), typeof(Lance), typeof(HeraldryDef) })]
  public static class UnitSpawnPointGameLogic_SpawnMech {
    public static void Postfix(UnitSpawnPointGameLogic __instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef, Mech __result) {
      if (__result.IsDeployDirector()) {
        __result.GameRep.gameObject.SetActive(false);
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
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnDropshipAnimationComplete")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class TurnDirector_OnDropshipAnimationComplete {
    public static void Postfix(TurnDirector __instance, MessageCenterMessage message) {
      Mech deployDirector = null;
      foreach(AbstractActor unit in __instance.Combat.LocalPlayerTeam.units) {
        if (unit.IsDead) { continue; }
        if (unit.IsDeployDirector() == false) { continue; }
        deployDirector = unit as Mech;
        break;
      }
      if(deployDirector != null) {
        if (__instance.Combat.LocalPlayerTeam.unitCount > 1) {
          deployDirector.HasActivatedThisRound = false;
          deployDirector.HUD().SelectionHandler.DeselectActor(deployDirector);
          deployDirector.HUD().MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
          __instance.Combat.StackManager.DEBUG_SequenceStack.Clear();
          typeof(TurnDirector).GetMethod("QueuePilotChatter", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
          List<AbstractActor> allActors = __instance.Combat.TurnDirector.Combat.AllActors;
          List<ICombatant> livingCombatants = __instance.Combat.TurnDirector.Combat.GetAllLivingCombatants();
          int num = 1;
          AuraCache.UpdateAllAuras(allActors, num != 0);
          for (int index = 0; index < __instance.Combat.TurnDirector.NumTurnActors; ++index) {
            Team turnActor = __instance.Combat.TurnDirector.TurnActors[index] as Team;
            if (turnActor != null)
              turnActor.RebuildVisibilityCacheAllUnits(livingCombatants);
          }
          __instance.Combat.TurnDirector.Combat.AllActors[0].UpdateVisibilityCache(livingCombatants);
          __instance.Combat.TurnDirector.StartFirstRound();
          //__instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new DialogComplete(DialogueGameLogic.missionStartDialogueGuid));
          //EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
        }
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("DespawnActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AbstractActor_DespawnActor {
    public static void Postfix(AbstractActor __instance, MessageCenterMessage message, ref string ____teamId, ref Team ____team) {
      try {
        DespawnActorMessage despawnActorMessage = message as DespawnActorMessage;
        if (despawnActorMessage == null) { return; }
        if (!(despawnActorMessage.affectedObjectGuid == __instance.GUID)) { return; }
        Log.TWL(0, "AbstractActor.DespawnActor " + __instance.DisplayName + ":" + despawnActorMessage.deathMethod);
        if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
        if (__instance.IsDeployDirector() == false) { return; }
        __instance.HUD().SelectionHandler.DeselectActor(__instance);
        foreach(Team team in __instance.Combat.Teams) {
          if (team == null) { continue; }

        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("TrySelectActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(bool) })]
  public static class AbstractActor_TrySelectActor {
    public static void Postfix(CombatSelectionHandler __instance, AbstractActor actor, bool manualSelection) {
      if (actor == null) { return; }
      if(actor.Combat.LocalPlayerTeam.unitCount == 1) {
        if (actor.Combat.LocalPlayerTeam.units[0].IsDeployDirector()) {
          return;
        }
      }
      try {
        Mech deployDirector = null;
        foreach (AbstractActor unit in actor.Combat.LocalPlayerTeam.units) {
          if (unit.IsDead) { continue; }
          if (unit.IsDeployDirector() == false) { continue; }
          deployDirector = unit as Mech;
          break;
        }
        if (deployDirector != null) {
          Log.TWL(0, "CombatSelectionHandler.TrySelectActor phase:" + deployDirector.Combat.TurnDirector.CurrentPhase + " is avaible:" + deployDirector.IsAvailableThisPhase);
          if (deployDirector.IsAvailableThisPhase) {
            deployDirector.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(deployDirector.DoneWithActor()));
          } else {
            EncounterLayerParent.EnqueueLoadAwareMessage((MessageCenterMessage)new DespawnActorMessage(PlayerLanceSpawnerGameLogic_OnEnterActive.deployLoadRequest.playerLanceSpawner.encounterObjectGuid, deployDirector.GUID, DeathMethod.DespawnedNoMessage));
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_InitStats {
    public static void Postfix(Mech __instance) {
      try {
        if (__instance.IsDeployDirector()) {
          __instance.Initiative = 1;
          __instance.StatCollection.Set<int>("BaseInitiative", 1);
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
        if (unit == null) { return true;  };
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
        Log.WL(2, viewer.DisplayName+":"+viewer.GUID);
      }
      Log.WL(1, "revealatrons:");
      foreach (FogOfWarRevealatron revealatron in ___revealatrons) {
        Log.WL(2, revealatron.name+":"+revealatron.GUID);
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
        Log.TWL(0, "FogOfWarSystem.AddViewer "+ unit.DisplayName+":"+ unit.GUID+ " IsDeployDirector:"+ unit.IsDeployDirector());
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
  [HarmonyPatch(typeof(GameInstance))]
  [HarmonyPatch("LaunchContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract), typeof(string) })]
  public static class GameInstance_LaunchContract {
    public static bool SpawnDelayed = false;
    public static bool isManualSpawn(this Contract contract) { return SpawnDelayed; }
    public static void ClearManualSpawn(this Contract contract) { SpawnDelayed = false; }
    public static void Prefix(GameInstance __instance, Contract contract, string playerGUID) {
      SpawnDelayed = false;
      if (Core.Settings.DeployManual == false) { return; }
      if (contract.SimGameContract == false) { return; }
      if (contract.IsFlashpointCampaignContract) { return; }
      if (contract.IsFlashpointContract) { return; }
      if (contract.IsRestorationContract) { return; }
      if (contract.IsStoryContract) { return; }
      if (Core.Settings.DeployForbidContractTypes.Contains(contract.ContractTypeValue.Name)) { return; }
      SpawnDelayed = true;
    }
  }
  [HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("OnEnterActive")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnEnterActive {
    public static HashSet<LanceSpawnerGameLogic> delayedSpawners = new HashSet<LanceSpawnerGameLogic>();
    public static List<Vector3> deplayedEnemySpawnPositions(this Contract contract) {
      List<Vector3> result = new List<Vector3>();
      foreach(LanceSpawnerGameLogic spawner in delayedSpawners) {
        Team team = spawner.Combat.TurnDirector.GetTurnActorByUniqueId(spawner.teamDefinitionGuid) as Team;
        if (team == null) { continue; }
        if (team.IsEnemy(spawner.Combat.LocalPlayerTeam) == false) { continue; }
        foreach(UnitSpawnPointGameLogic spawnPoint in spawner.unitSpawnPointGameLogicList) {
          if (spawnPoint.unitType == UnitType.UNDEFINED) { continue; }
          result.Add(spawnPoint.Position);
        }
      }
      return result;
    }
    public static void SetSpawnerReady(this LanceSpawnerGameLogic spawner) {
      if (delayedSpawners.Contains(spawner)) {
        delayedSpawners.Remove(spawner);
        Log.TWL(0, "Delayed spawner ready: " + spawner.DisplayName + " objectives ready state:"+spawner.Combat.ActiveContract.isObjectivesReady());
      }
    }
    public static void ActivateDelayed(this Contract contract) {
      HashSet<LanceSpawnerGameLogic> spawners = new HashSet<LanceSpawnerGameLogic>();
      foreach (LanceSpawnerGameLogic sp in delayedSpawners) { spawners.Add(sp); }
      foreach (LanceSpawnerGameLogic sp in spawners) { sp.OnEnterActive(); }
    }
    public static bool isObjectivesReady(this Contract contract) { return (delayedSpawners.Count == 0)&&(contract.isManualSpawn() == false); }
    public static bool Prefix(LanceSpawnerGameLogic __instance) {
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isManualSpawn() == false) { return true; }
      if (__instance.unitSpawnPointGameLogicList.Length == 0) { return true; }
      if (__instance.unitSpawnPointGameLogicList[0].mechDefId == CACConstants.DeployMechDefID) { return true; }
      delayedSpawners.Add(__instance);
      return false;
    }
  }
  [HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("OnUnitSpawnComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnUnitSpawnCompleteManual {
    public static void Postfix(LanceSpawnerGameLogic __instance) {
      if(__instance.Combat.ActiveContract.isManualSpawn() == false) {
        __instance.SetSpawnerReady();
      }
    }
  }
  [HarmonyPatch(typeof(ContractObjectiveGameLogic))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ContractObjectiveGameLogic_Update {
    public static bool Prefix(ContractObjectiveGameLogic __instance) {
      if (__instance.Combat == null) { return true; }
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isObjectivesReady() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ObjectiveGameLogic_Update {
    public static bool Prefix(ContractObjectiveGameLogic __instance) {
      if (__instance.Combat == null) { return true; }
      if (__instance.Combat.ActiveContract == null) { return true; }
      if (__instance.Combat.ActiveContract.isObjectivesReady() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("GetTaggedCombatants")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(TagSet), typeof(string) })]
  public static class ObjectiveGameLogic_GetTaggedCombatants0 {
    public static void Postfix(CombatGameState combatGameState, TagSet requiredTags, string lanceGuid, ref List<ICombatant> __result) {
      for(int index = 0; index < __result.Count; ++index) {
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

  public class DeployDirectorLoadRequest {
    public PlayerLanceSpawnerGameLogic playerLanceSpawner;
    public UnitSpawnPointGameLogic spawnPoint;
    public Vector3 originalLocalPos;
    public MechDef originalMechDef;
    public PilotDef originalPilotDef;
    public Dictionary<UnitSpawnPointGameLogic,UnitType> originalUnitTypes;
    public SpawnUnitMethodType originalSpawnMethod;
    public Mech deployDirector;
    public CombatHUD HUD;
    public void RestoreAndSpawn(Vector3 pos, Mech deployDirector, CombatHUD HUD) {
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
      playerLanceSpawner.Position = pos;
      HashSet<Vector3> deployPositions = new HashSet<Vector3>();
      Log.TWL(0, "DeployDirectorLoadRequest.RestoreAndSpawn start pos generation",true);
      foreach(UnitSpawnPointGameLogic sp in playerLanceSpawner.unitSpawnPointGameLogicList) {
        Vector3 rndPos = pos;
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
          Log.WL(1,rndPos.ToString()+" distance:"+Vector3.Distance(rndPos,pos),true);
        } while (deployPositions.Contains(rndPos));
        deployPositions.Add(rndPos);
        sp.Position = rndPos;
      }
      playerLanceSpawner.spawnUnitsOnActivation = true;
      playerLanceSpawner.spawnMethod = originalSpawnMethod;
      this.playerLanceSpawner.Combat.ActiveContract.ClearManualSpawn();
      playerLanceSpawner.LanceSpawnerGameLogic_OnEnterActive();
      this.playerLanceSpawner.Combat.ActiveContract.ActivateDelayed();
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
      Log.TWL(0, "DeployDirectorLoadRequest.DirectorPilotDefLoaded "+id);
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
      if(def.DependenciesLoaded(1000u) == false) {
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
      PilotDef pilotDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<PilotDef>(PlayerLanceSpawnerGameLogic_OnEnterActive.DeployPilotDefID, BattleTechResourceType.PilotDef);
      if(pilotDef != null) {
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
        request.AddLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, PlayerLanceSpawnerGameLogic_OnEnterActive.DeployPilotDefID, this.DirectorPilotDefLoaded);
        request.ProcessRequests(1000u);
        return;
      }
      MechDef mechDef = playerLanceSpawner.Combat.DataManager.GetObjectOfType<MechDef>(CACConstants.DeployMechDefID, BattleTechResourceType.MechDef);
      if(mechDef != null) {
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
    public static readonly string DeployPilotDefID = "pilot_deploy_director";
    public static readonly string DeployAbilityDefID = "AbilityDefCU_DeploySetPosition";
    public static DeployDirectorLoadRequest deployLoadRequest = null;
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
      if (__instance.teamDefinitionGuid != __instance.Combat.LocalPlayerTeamGuid) { return true; }
      if (__instance.Combat.ActiveContract.isManualSpawn() == false) { return true; }
      deployLoadRequest = new DeployDirectorLoadRequest(__instance);
      return false;
      //return true;
    }
  }
}