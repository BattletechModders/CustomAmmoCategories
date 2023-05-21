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
using BattleTech.UI;
using HarmonyLib;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InitAbilities")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class Pilot_InitAbilities {
    public static void Postfix(Pilot __instance, bool ModifyStats, bool FromSave, List<AbilityStateInfo> ___SerializedAbilityStates) {
      try {
        Log.M?.TWL(0, "Pilot.InitAbilities " + __instance.Description.Id);
        if (__instance.pilotDef.DataManager.AbilityDefs.TryGet(SpawnHelper.SpawnAbilityDefID, out AbilityDef abilityDef) == false) {
          return;
        }
        Ability ability = new Ability(abilityDef);
        AbilityStateInfo LoadedState = (AbilityStateInfo)null;
        CombatGameState Combat = Traverse.Create(__instance).Property<CombatGameState>("Combat").Value;
        if (___SerializedAbilityStates != null)
          LoadedState = ___SerializedAbilityStates.FirstOrDefault<AbilityStateInfo>((Func<AbilityStateInfo, bool>)(x => x.AbilityDefID == abilityDef.Id));
        if (FromSave && LoadedState != null)
          ability.InitFromSave(Combat, LoadedState);
        else
          ability.Init(Combat);
        __instance.Abilities.Add(ability);
        __instance.ActiveAbilities.Add(ability);
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        Pilot.pilotErrorLog.LogException(e);
      }
    }
  }
  public class SelectionStateCommandSpawnUnit : SelectionStateCommandTargetSinglePoint {
    private bool HasActivated;
    public SelectionStateCommandSpawnUnit(CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor) : base(Combat, HUD, FromButton) {
      this.SelectedActor = actor;
    }
    protected override bool ShouldShowWeaponsUI { get { return false; } }
    protected override bool ShouldShowTargetingLines { get { return false; } }
    protected override bool ShouldShowFiringArcs { get { return false; } }
    protected override bool showHeatWarnings { get { return false; } }
    public override bool ConsumesMovement { get { return true; } }
    public override bool ConsumesFiring { get { return true; } }
    public override bool CanBackOut { get { return NumPositionsLocked > 0; } }
    public virtual bool HasCalledShot { get { return false; } }
    public virtual bool NeedsCalledShot { get { return false; } }
    public override bool CanActorUseThisState(AbstractActor actor) { return true; }
    public override bool CanDeselect { get { return true; } }
    public override void OnAddToStack() {
      NumPositionsLocked = 0;
      Log.Combat?.TWL(0, "SelectionStateCommandSpawnUnit OnAddToStack");
      base.OnAddToStack();
    }
    public override void OnInactivate() {
      Log.Combat?.TWL(0, "SelectionStateCommandSpawnUnit.OnInactivate");
      NumPositionsLocked = 0;
      base.OnInactivate();
    }
    public void AttackOrdersSet(MessageCenterMessage message) {
      this.Orders = (message as AddSequenceToStackMessage).sequence;
    }
    public override bool ProcessPressedButton(string button) {
      try {
        Log.Combat?.TWL(0, "SelectionStateCommandSpawnUnit.ProcessPressedButton "+ button); 
        this.HasActivated = button == "BTN_FireConfirm";
        if (button != "BTN_FireConfirm") {
          return base.ProcessPressedButton(button);
        }
        if (this.Orders != null) { return false; }
        this.HideFireButton(false);
        ReceiveMessageCenterMessage messageCenterMessage = (ReceiveMessageCenterMessage)(message => { });
        this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, new ReceiveMessageCenterMessage(this.HandleAddSequenceToOrders));
        Log.Combat?.WL(1,"SpawnAtPos");
        HotDropManager.DefferedHotDrop(this.SelectedActor, this.targetPosition);
        this.HasActivated = true;
        this.OnInactivate();
      }catch(Exception e) {
        Log.ECombat?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
      return true;
    }
    public override void BackOut() {
      if (this.NumPositionsLocked > 1) { this.NumPositionsLocked = 1; }
      if (this.NumPositionsLocked > 0) {
        this.HideFireButton(false);
        this.NumPositionsLocked = 0;
        Log.Combat?.TWL(0, "SelectionStateCommandSpawnUnit.BackOut:" + this.NumPositionsLocked);
      } else {
        Log.Combat?.TWL(0, "SelectionStateCommandSpawnUnit.BackOut:" + this.NumPositionsLocked);
      }
    }
    public override bool ProcessLeftClick(Vector3 worldPos) {
      if (this.NumPositionsLocked != 0) { return false; }
      float minDist = (float)this.FromButton.Ability.Def.IntParam1;
      float maxDist = (float)this.FromButton.Ability.Def.IntParam2;
      this.targetPosition = this.GetVaidSpawnPos(worldPos, minDist, maxDist);
      this.NumPositionsLocked = 1;
      this.ShowFireButton(CombatHUDFireButton.FireMode.Confirm, Ability.ProcessDetailString(this.FromButton.Ability).ToString(true));
      return true;
    }
    public override Vector3 PreviewPos {
      get {
        return this.SelectedActor.CurrentPosition;
      }
    }
    public Vector3 GetVaidSpawnPos(Vector3 worldPos, float min, float max) {
      Vector3 result = new Vector3(worldPos.x, worldPos.y, worldPos.z);
      float dist = Vector3.Distance(this.SelectedActor.CurrentPosition, worldPos);
      if ((double)dist < (double)min)
        result = this.SelectedActor.CurrentPosition + (worldPos - this.SelectedActor.CurrentPosition).normalized * min;
      else if ((double)dist > (double)max)
        result = this.SelectedActor.CurrentPosition + (worldPos - this.SelectedActor.CurrentPosition).normalized * max;
      return HUD.Combat.HexGrid.GetClosestPointOnGrid(result);
    }
    public override void ProcessMousePos(Vector3 worldPos) {
      if (this.HasActivated)
        return;
      float floatParam1 = this.FromButton.Ability.Def.FloatParam1;
      float minDist = (float)this.FromButton.Ability.Def.IntParam1;
      float maxDist = (float)this.FromButton.Ability.Def.IntParam2;
      worldPos = this.GetVaidSpawnPos(worldPos, minDist, maxDist);
      switch (this.NumPositionsLocked) {
        case 0:
          CombatTargetingReticle.Instance.UpdateReticle(worldPos, floatParam1, false);
        break;
        case 1:
          CombatTargetingReticle.Instance.UpdateReticle(this.targetPosition, floatParam1, false);
        break;
      }
      CombatTargetingReticle.Instance.ShowRangeIndicators(this.SelectedActor.CurrentPosition, minDist, maxDist, true, true);
    }
    public override int ProjectedHeatForState { get { return 0; } }
  };

  public static class SpawnHelper {
    public static readonly string SpawnAbilityDefID = "AbilityDefCU_SpawnUnit";
    private static void SpawnAtPos(MechDef mechDef, PilotDef newPilotDef, Vector3 pos, Team parentTeam, AbstractActor parentActor) {
      Log.Combat?.TWL(0, "SpawnAtPos mechDef:" + mechDef.ChassisID + " pilotDef:" + newPilotDef.Description.Id + " team:" + parentTeam.DisplayName + " actor:" + parentActor.PilotableActorDef.ChassisID);
      try {
        var customEncounterTags = new TagSet(parentTeam.EncounterTags);
        customEncounterTags.Add("SpawnedFromAbility");
        Lance spawnLance = null;
        foreach (Lance lance in parentTeam.lances) { spawnLance = lance; break; }
        var newActor = ActorFactory.CreateMech(mechDef, newPilotDef, customEncounterTags, parentTeam.Combat, parentTeam.GetNextSupportUnitGuid(), parentActor.spawnerGUID, parentActor.team.HeraldryDef);
        newActor.Init(pos, parentActor.CurrentRotation.eulerAngles.y, true);
        newActor.InitGameRep(null);
        parentTeam.AddUnit(newActor);
        newActor.AddToTeam(parentTeam);
        spawnLance?.AddUnitGUID(newActor.GUID);
        if (spawnLance != null) newActor.AddToLance(spawnLance);
        newActor.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
        UnitSpawnedMessage unitSpawnedMessage = new UnitSpawnedMessage(parentActor.spawnerGUID, newActor.GUID);
        newActor.OnPositionUpdate(pos, Quaternion.identity, -1, true, (List<DesignMaskDef>)null, false);
        newActor.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(parentActor.Combat.BattleTechGame, newActor, BehaviorTreeIDEnum.CoreAITree);
        parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new UnitSpawnedMessage(parentActor.spawnerGUID, newActor.GUID));
        Log.Combat?.WL(1, "spawn success:" + newActor.PilotableActorDef.ChassisID + ":" + newActor.GUID);
        if(parentTeam == parentActor.Combat.LocalPlayerTeam) {
          CombatHUD HUD = UIManager.Instance.gameObject.GetComponentInChildren<CombatHUD>();
          HUD?.MechWarriorTray.RefreshTeam(parentActor.Combat.LocalPlayerTeam);
        }
        parentActor.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(newActor.DoneWithActor()));
      } catch (Exception e) {
        Log.ECombat?.TWL(0,e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public static void SpawnAtPos(string mechDefId, Vector3 pos, Team parentTeam, AbstractActor parentActor) {
      PilotDef newPilotDef = null;
      try {
        Log.Combat?.TWL(0, "SpawnAtPos mechDefId:" + mechDefId + " team:" + parentTeam.DisplayName + " actor:" + parentActor.PilotableActorDef.ChassisID);
        if (UnityGameInstance.BattleTechGame.Simulation != null) {
          SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
          Log.Combat?.WL(1, "GenerateRandomPilot");
          newPilotDef = sim.PilotGenerator.GenerateRandomPilot(sim.CurSystem.Def.GetDifficulty(sim.SimGameMode));
        } else {
          List<PilotDef> pilots = new List<PilotDef>();
          Log.Combat?.WL(1, "Choose random pilot:"+ parentActor.Combat.DataManager.PilotDefs.Count);
          foreach (var pilot in parentActor.Combat.DataManager.PilotDefs) {
            pilots.Add(pilot.Value);
          }
          if (pilots.Count > 0) {
            newPilotDef = new PilotDef(pilots[UnityEngine.Random.Range(0, pilots.Count)],0,0,0,false,0,0,0,null);
          }
        }
        if (newPilotDef != null) {
          SpawnAtPos(mechDefId, newPilotDef, pos, parentTeam, parentActor);
        }
      }catch(Exception e) {
        Log.ECombat?.TWL(0,e.ToString(),true);
        UIManager.logger.LogException(e);
      }
    }
    public static void SpawnAtPos(string mechDefId, PilotDef newPilotDef, Vector3 pos, Team parentTeam, AbstractActor parentActor) {
      Log.Combat?.TWL(0, "SpawnAtPos mechDefId:" + mechDefId + " pilotDef:"+newPilotDef.Description.Id+" team:"+parentTeam.DisplayName+" actor:"+parentActor.PilotableActorDef.ChassisID);
      DataManager dataManager = parentActor.Combat.DataManager;
      CombatGameState combat = parentActor.Combat;
      if(dataManager.MechDefs.TryGet(mechDefId, out var newMechDef) == false) {
        Log.Combat?.WL(1, mechDefId + " absent in data manager");
        return;
      }
      newMechDef.Refresh();
      var injectedDependencyLoadRequest = new DataManager.InjectedDependencyLoadRequest(dataManager);
      newMechDef.GatherDependencies(dataManager, injectedDependencyLoadRequest, 1000U);
      newPilotDef.GatherDependencies(dataManager, injectedDependencyLoadRequest, 1000U);
      Log.Combat?.WL(1, "DependencyCount:" + injectedDependencyLoadRequest.DependencyCount());
      if (injectedDependencyLoadRequest.DependencyCount() == 0) {
        SpawnAtPos(newMechDef, newPilotDef, pos, parentTeam, parentActor);
        return;
      }
      injectedDependencyLoadRequest.RegisterLoadCompleteCallback((Action)(() => {
        Log.Combat?.TWL(0, "DependencyLoadSuccess mechDefId:" + mechDefId + " pilotDef:" + newPilotDef.Description.Id);
        SpawnAtPos(newMechDef, newPilotDef, pos, parentTeam, parentActor);
      }));
      injectedDependencyLoadRequest.RegisterLoadFailedCallback((Action)(() => {
        Log.Combat?.TWL(0, "DependencyLoadFailed mechDefId:" + mechDefId + " pilotDef:" + newPilotDef.Description.Id);
      }));
      dataManager.InjectDependencyLoader(injectedDependencyLoadRequest, 1000U);
    }
  }
}