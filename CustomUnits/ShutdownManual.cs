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
using BattleTech.Serialization;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using HarmonyLib;
using HBS.Util;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  //[HarmonyPatch(typeof(CombatHUDObjectiveItem))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] {  })]
  //public static class CombatHUDObjectiveItem_Update {
  //  private static object ObjectiveState_None = null;
  //  private static object ObjectiveState_Shown = null;
  //  private static object ObjectiveState_Succeeded = null;
  //  private static object ObjectiveState_SucceededFading = null;
  //  private static object ObjectiveState_Failed = null;
  //  private static object ObjectiveState_FailedFading = null;
  //  private static object ObjectiveState_Off = null;
  //  private static Type ObjectiveState_type = null;
  //  private static MethodInfo CombatHUDObjectiveItem_SetState = null;
  //  public static void Prefix(CombatHUDObjectiveItem __instance) {
  //    try {
  //      Log.TWL(0, "CombatHUDObjectiveItem.Update");
  //      if (ObjectiveState_type == null) { ObjectiveState_type = typeof(CombatHUDObjectiveItem).GetNestedType("ObjectiveState", System.Reflection.BindingFlags.NonPublic); }
  //      if (ObjectiveState_type == null) { Log.TWL(0, "can't get CombatHUDObjectiveItem.ObjectiveState", true); return; }
  //      if (ObjectiveState_None == null) { ObjectiveState_None = Enum.Parse(ObjectiveState_type, "None"); }
  //      if (ObjectiveState_Shown == null) { ObjectiveState_Shown = Enum.Parse(ObjectiveState_type, "Shown"); }
  //      if (ObjectiveState_Succeeded == null) { ObjectiveState_Succeeded = Enum.Parse(ObjectiveState_type, "Succeeded"); }
  //      if (ObjectiveState_SucceededFading == null) { ObjectiveState_SucceededFading = Enum.Parse(ObjectiveState_type, "SucceededFading"); }
  //      if (ObjectiveState_Failed == null) { ObjectiveState_Failed = Enum.Parse(ObjectiveState_type, "Failed"); }
  //      if (ObjectiveState_FailedFading == null) { ObjectiveState_FailedFading = Enum.Parse(ObjectiveState_type, "FailedFading"); }
  //      if (ObjectiveState_Off == null) { ObjectiveState_Off = Enum.Parse(ObjectiveState_type, "Off"); }
  //      if (CombatHUDObjectiveItem_SetState == null) { CombatHUDObjectiveItem_SetState = typeof(CombatHUDObjectiveItem).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic); }
  //      if (CombatHUDObjectiveItem_SetState == null) { Log.TWL(0, "can't get CombatHUDObjectiveItem.SetState method", true); return; }
  //      CombatHUDObjectiveItem_SetState.Invoke(__instance, new object[] { ObjectiveState_Failed });
  //    } catch (Exception e) {
  //      Log.TWL(0,e.ToString(), true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("ResetAbilityButton")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(CombatHUDActionButton), typeof(Ability), typeof(bool) })]
  public static class CombatHUDMechwarriorTray_ResetAbilityButton {
    public static void Prefix(ref bool __runOriginal, CombatHUDMechwarriorTray __instance, AbstractActor actor, CombatHUDActionButton button, Ability ability, bool forceInactive) {
      try {
        if (!__runOriginal) { return; }
        if (ability == null) { __runOriginal = false; return; }
        if (forceInactive) { button.DisableButton(); } else if (button.IsAbilityActivated) {
          button.ResetButtonIfNotActive(actor);
        } else if (ability.IsAvailable == false) {
          button.DisableButton();
        } else {
          bool canBeActivatedInShutdown = false;
          AbilityDefEx abilityDefEx = ability.Def.exDef();
          if (abilityDefEx != null) { if (abilityDefEx.CanBeUsedInShutdown) { canBeActivatedInShutdown = true; } }
          if (actor.HasActivatedThisRound) { button.DisableButton(); __runOriginal = false; return; }
          if (actor.IsAvailableThisPhase == false) { button.DisableButton(); __runOriginal = false; return; }
          if (actor.MovingToPosition != null) { button.DisableButton(); __runOriginal = false; return; }
          if (__instance.Combat.StackManager.IsAnyOrderActive && __instance.Combat.TurnDirector.IsInterleaved) { button.DisableButton(); __runOriginal = false; return; }
          if (actor.IsShutDown && (canBeActivatedInShutdown == false)) { button.DisableButton(); __runOriginal = false; return; }
          if(actor.IsProne) { button.DisableButton(); __runOriginal = false; return; }
          if (actor.HasFiredThisRound && (ability.Def.ActivationTime == AbilityDef.ActivationTiming.ConsumedByFiring)) { button.DisableButton(); __runOriginal = false; return; }
          if (actor.HasMovedThisRound && (ability.Def.ActivationTime == AbilityDef.ActivationTiming.ConsumedByMovement)) { button.DisableButton(); __runOriginal = false; return; }
          button.ResetButtonIfNotActive(actor);
        }
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("IsShutDown")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_IsShutDown {
    public static void Postfix(AbstractActor __instance, ref bool __result) {
      try {
        if (__result == false) { return; }
        if (Thread.CurrentThread.peekFromStack<AbstractActor>(CombatHUDMechwarriorTrayEx.IGNORE_SHUTDOWN_STACK_NAME) == __instance) {
          __result = false;
        }
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  //[HarmonyPatch("ResetAbilityButton")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(CombatHUDActionButton), typeof(Ability), typeof(bool) })]
  //public static class CombatHUDMechwarriorTray_ResetAbilityButton {
  //  public static void Prefix(CombatHUDMechwarriorTray __instance, AbstractActor actor, CombatHUDActionButton button, Ability ability, bool forceInactive, ref bool __state) {
  //    try {
  //      __state = false;
  //      AbilityDefEx abilityDefEx = ability.Def.exDef();
  //      if (abilityDefEx == null) { return; }
  //      if (abilityDefEx.CanBeUsedInShutdown == false) { return; }
  //      if (actor == null) { return; }
  //      Thread.CurrentThread.pushToStack<AbstractActor>(CombatHUDMechwarriorTrayEx.IGNORE_SHUTDOWN_STACK_NAME, actor);
  //      __state = true;
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //  public static void Postfix(ref bool __state) {
  //    if (__state) { Thread.CurrentThread.popFromStack<AbstractActor>(CombatHUDMechwarriorTrayEx.IGNORE_SHUTDOWN_STACK_NAME); }
  //  }
  //}

  public class UnitShutdownSequence : MultiSequence {
    private UnitShutdownSequence.ShutdownState state;
    private float timeInCurrentState;
    private const float TimeToRise = 1f;
    private const float TimeToShutdown = 1f;

    private AbstractActor OwningActor { get; set; }

    public UnitShutdownSequence(AbstractActor unit): base(unit.Combat) {
      this.OwningActor = unit;
      this.setState(UnitShutdownSequence.ShutdownState.Rising);
    }

    private void update() {
      this.timeInCurrentState += Time.deltaTime;
      switch (this.state) {
        case UnitShutdownSequence.ShutdownState.Rising:
        if ((double)this.timeInCurrentState <= 1.0)
          break;
        this.setState(UnitShutdownSequence.ShutdownState.ShuttingDown);
        break;
        case UnitShutdownSequence.ShutdownState.ShuttingDown:
        if ((double)this.timeInCurrentState <= 1.0)
          break;
        this.setState(UnitShutdownSequence.ShutdownState.Finished);
        break;
      }
    }

    private void setState(UnitShutdownSequence.ShutdownState newState) {
      if (this.state == newState) { return; }
      this.state = newState;
      this.timeInCurrentState = 0.0f;
      switch (newState) {
        case UnitShutdownSequence.ShutdownState.ShuttingDown:
          if (this.OwningActor.GameRep != null) {
            this.OwningActor.GameRep.PlayShutdownAnim();
            this.OwningActor.CancelCreatedEffects();
          }
        break;
        case UnitShutdownSequence.ShutdownState.Finished:
          Log.M?.TWL(0,("Unit " + this.OwningActor.PilotableActorDef.Description.Id + " shuts down from pilot command"));
          this.OwningActor.IsShutDown = true;
          this.OwningActor.DumpAllEvasivePips();
          this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(this.OwningActor.DoneWithActor()));
          Log.M?.WL(1, "Done with actor");
        break;
      }
    }

    public override void OnAdded() {
      base.OnAdded();
      string str = string.Format("UnitShutdownSequence_{0}_{1}", (object)this.RootSequenceGUID, (object)this.SequenceGUID);
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(this.OwningActor.GUID, this.OwningActor.GUID, "ENGINE SHUTDOWN!", FloatieMessage.MessageNature.Debuff));
      int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_overheat_alarm_3, WwiseManager.GlobalAudioObject);
    }

    public override void OnUpdate() => this.update();

    public override void OnSuspend() => base.OnSuspend();

    public override void OnResume() => base.OnResume();

    public override void OnComplete() => base.OnComplete();

    public override void OnCanceled() {
    }

    public override void OnUndo() {
    }

    public override bool IsValidMultiSequenceChild => true;

    public override bool IsParallelInterruptable => false;

    public override bool IsCancelable => false;

    public override bool IsComplete => this.state == UnitShutdownSequence.ShutdownState.Finished;

    public override int Size() => 0;

    public override bool ShouldSave() => false;

    public override void Save(SerializationStream stream) {
    }

    public override void Load(SerializationStream stream) {
    }

    public override void LoadComplete() {
    }

    public enum ShutdownState {
      None,
      Rising,
      ShuttingDown,
      Finished,
    }
  }
  [SerializableContract("ShutdownInvocation")]
  public class ShutdownInvocation : InvocationMessage {
    public override MessageCenterMessageType MessageType => MessageCenterMessageType.EjectInvocation;

    [SerializableMember(SerializationTarget.Networking)]
    public string SourceGUID { get; private set; }

    public override string ToString() {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendFormat("Source Guid = {0}\n", (object)this.SourceGUID);
      return stringBuilder.ToString();
    }

    private ShutdownInvocation() {
    }

    public ShutdownInvocation(AbstractActor source)
      : base(UnityEngine.Random.Range(0, 99999))
      => this.SourceGUID = source.GUID;

    public override bool Invoke(CombatGameState combatGameState) {
      AbstractActor actorByGuid = combatGameState.FindActorByGUID(this.SourceGUID);
      if (actorByGuid == null) {
        Log.Combat?.TWL(0,string.Format("ShutdownInvocation.Invoke failed! Source AbstractActor with GUID {0} not found!", (object)this.SourceGUID));
        return false;
      }
      UnitShutdownSequence shutdownSequence = new UnitShutdownSequence(actorByGuid);
      this.PublishStackSequence(combatGameState.MessageCenter, (IStackSequence)shutdownSequence, (InvocationMessage)this);
      return true;
    }

    public override void FromJSON(string json) => throw new NotImplementedException();

    public override string ToJSON() => throw new NotImplementedException();

    public override string GenerateJSONTemplate() => throw new NotImplementedException();
  }
  public class SelectionStateShutdown : SelectionStateConfirm {
    public override bool CanActorUseThisState(AbstractActor actor) {
      if (actor == null) { return false; }
      if (actor.IsPilotable == false) { return false; }
      if (actor.GetPilot() == null) { return false; }
      if (actor.IsShutDown) { return false; }
      if (actor.IsProne) { return false; }
      return true;
    }
    protected override string FireButtonString => Strings.T("Are you sure want to shutdown engine?");

    protected override bool CreateConfirmationOrders() {
      if (this.SelectedActor != null) {
        MessageCenterMessage message1 = (MessageCenterMessage)new ShutdownInvocation(this.SelectedActor);
        if (message1 == null) {
          Debug.LogError((object)"No invocation created");
          return false;
        }
        ReceiveMessageCenterMessage subscriber = (ReceiveMessageCenterMessage)(message => this.Orders = (message as AddSequenceToStackMessage).sequence);
        this.Combat.MessageCenter.AddSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        this.Combat.MessageCenter.PublishMessage(message1);
        this.Combat.MessageCenter.RemoveSubscriber(MessageCenterMessageType.AddSequenceToStackMessage, subscriber);
        return true;
      }
      Log.Combat?.TWL(0,"no selectedActor?");
      return false;
    }

    public SelectionStateShutdown(CombatGameState Combat,CombatHUD HUD,CombatHUDActionButton FromButton, AbstractActor actor): base(Combat, HUD, FromButton, actor) {
      this.SelectionType = SelectionType.VentCoolant;
      this.PriorityLevel = SelectionPriorityLevel.BasicCommand;
    }
  }
  [HarmonyPatch(typeof(SelectionState))]
  [HarmonyPatch("GetNewSelectionStateByType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SelectionType), typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDActionButton), typeof(AbstractActor) })]
  public static class SelectionState_GetNewSelectionStateByTypeShutdown {
    public static bool SelectionForbidden = false;
    public static void Prefix(ref bool __runOriginal, SelectionType type, CombatGameState Combat, CombatHUD HUD, CombatHUDActionButton FromButton, AbstractActor actor, ref SelectionState __result) {
      if (!__runOriginal) { return; }
      Log.Combat?.TWL(0, "SelectionState.GetNewSelectionStateByType shutdown:" + type + ":" + FromButton.GUID);
      if ((type == SelectionType.VentCoolant) && (FromButton.GUID == CombatHUDMechwarriorTrayEx.ShutdownAbilityId)) {
        Log.Combat?.WL(1, "creating own selection state");
        __result = new SelectionStateShutdown(Combat, HUD, FromButton, actor);
        __runOriginal = false;
        return;
      }
      return;
    }
  }
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ToggleAbilityState")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class CombatHUDActionButton_ToggleAbilityState {
    public static void Prefix(CombatHUDActionButton __instance) {
      try {
        Log.Combat?.TWL(0, "CombatHUDActionButton.ToggleAbilityState " + __instance.GUID + " ability:" + (__instance.Ability == null ? "null" : __instance.Ability.Def.Id)+" state:"+ Traverse.Create(__instance).Property("state").GetValue());
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActionButton))]
  [HarmonyPatch("ActivateAbility")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActionButton_ActivateAbility {
    public static void Prefix(CombatHUDActionButton __instance) {
      try {
        Log.Combat?.TWL(0, "CombatHUDActionButton.ActivateAbility " + __instance.GUID + " ability:" + (__instance.Ability == null?"null":__instance.Ability.Def.Id));
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class PilotDef_DependenciesLoaded {
    public static void Postfix(PilotDef __instance, uint loadWeight, ref bool __result) {
      try {
        Log.Combat?.TWL(0, "PilotDef.DependenciesLoaded " + __instance.Description.Id);
        if (__result == false) { return; }
        if (__instance.DataManager.AbilityDefs.TryGet(CombatHUDMechwarriorTrayEx.ShutdownAbilityId, out AbilityDef aDef) == false) {
          __result = false;
        }
        if (__instance.DataManager.AbilityDefs.TryGet(SpawnHelper.SpawnAbilityDefID, out AbilityDef spawnAbilityDef) == false) {
          __result = false;
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager?.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class PilotDef_GatherDependencies {
    public static void Postfix(PilotDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      try {
        //Log.TWL(0, "PilotDef.GatherDependencies " + __instance.Description.Id);
        if (dataManager.AbilityDefs.Exists(CombatHUDMechwarriorTrayEx.ShutdownAbilityId) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, CombatHUDMechwarriorTrayEx.ShutdownAbilityId);
        }
        if (dataManager.AbilityDefs.Exists(SpawnHelper.SpawnAbilityDefID) == false) {
          dependencyLoad.RequestResource(BattleTechResourceType.AbilityDef, SpawnHelper.SpawnAbilityDefID);
        }
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager?.logger.LogException(e);
      }
    }
  }
  public static class ShutdownAbilitiesHelper {
    private static Dictionary<AbstractActor, Ability> ShutdownAbilities = new Dictionary<AbstractActor, Ability>();
    public static Ability ShutdownAbility(this AbstractActor unit) {
      if (ShutdownAbilities.TryGetValue(unit, out Ability result)) {
        return result;
      }
      AbilityDef Def = unit.Combat.DataManager.AbilityDefs.Get(CombatHUDMechwarriorTrayEx.ShutdownAbilityId);
      result = new Ability(Def);
      result.Init(unit.Combat);
      ShutdownAbilities.Add(unit, result);
      return result;
    }
  }
  public partial class CombatHUDMechwarriorTrayEx {
    public static readonly string ShutdownAbilityId = "AbilityDefCU_Shutdown";
    public static readonly string IGNORE_SHUTDOWN_STACK_NAME = "IGNORE_SHUTDOWN_FOR_ACTOR";
    public CombatHUDActionButton ShutdownBtn { get; set; }
    public RectTransform EjectButton { get; set; }
    public RectTransform ShutdownButton { get; set; }
    public void InitSutdownButtonUI() {
      try {
        Vector3 pos = EjectButton.localPosition;
        pos.x -= (EjectButton.sizeDelta.x + 5f);
        ShutdownButton.sizeDelta = EjectButton.sizeDelta;
        ShutdownButton.localPosition = pos;
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString(), true);
      }
    } 
    public void InstantineShutdownButton(CombatHUD HUD) {
      try {
        Log.Combat?.TWL(0, "CombatHUDMechwarriorTrayEx.InstantineShutdownButton");
        this.EjectButton = HUD.MechTray.gameObject.transform.FindRecursive("EjectButton") as RectTransform;
        if (this.EjectButton == null) { return; }
        GameObject ShutDownGO = GameObject.Instantiate(this.EjectButton.gameObject);
        this.ShutdownButton = ShutDownGO.GetComponent<RectTransform>();
        ShutDownGO.transform.SetParent(EjectButton.transform.parent);
        ShutDownGO.transform.SetSiblingIndex(EjectButton.transform.GetSiblingIndex() + 1);
        ShutDownGO.name = "ShutdownButton";
        Transform shutdownButton_Holder = ShutDownGO.transform.FindRecursive("ejectButton_Holder");
        HBSTooltip tooltip = ShutDownGO.GetComponentInChildren<HBSTooltip>(true);
        tooltip.defaultStateData.SetString("SHUTDOWN");
        shutdownButton_Holder.gameObject.name = "shutdownButton_Holder";
        List<GameObject> ActionButtonHolders = new List<GameObject>();
        ActionButtonHolders.AddRange(HUD.MechWarriorTray.ActionButtonHolders);
        ActionButtonHolders.Add(shutdownButton_Holder.gameObject);
        HUD.MechWarriorTray.ActionButtonHolders = ActionButtonHolders.ToArray();
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        CombatHUD.uiLogger.LogException(e);
      }
    }
  }
}