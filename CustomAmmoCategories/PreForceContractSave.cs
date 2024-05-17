using BattleTech;
using BattleTech.Save.SaveGameStructure;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;

namespace CustAmmoCategories {
  public static class PreForceTakeContractSave {
    public static bool SkipSave = false;
    public static string APPLY_EVENT_ACTION_STAT_NAME = "CAC_FORCE_CONTRACT_APPLY_EVENT_ACTION_STAT_NAME";
    public static void ApplyEventAction_prefix(ref bool __runOriginal, SimGameResultAction action, object additionalObject) {
      if (SkipSave) { return; }
      if (CustomAmmoCategories.Settings.ConsequenceDropAutosave == false) { return; }
      if (__runOriginal == false) { return; }
      try {
        SimGameState sim = UnityGameInstance.BattleTechGame.Simulation;
        if (sim == null) { return; }
        SimGameState.AddContractData contractData = null;
        Log.M?.TWL(0, $"ApplyEventAction:{action.ToString()}");
        switch (action.Type) {
          case SimGameResultAction.ActionType.Flashpoint_StartContract:
            contractData = sim.ParseFlashpointContractActionData(action.value, action.additionalValues);
            break;
          //case SimGameResultAction.ActionType.System_StartContract:
          //  contractData = sim.ParseContractActionData(action.value, action.additionalValues);
          //  break;
          //case SimGameResultAction.ActionType.System_StartNonProceduralContract:
          //  contractData = sim.ParseNonProceduralContractActionData(action.value, action.additionalValues);
          //  break;
          default: break;
        }
        if (contractData != null) {
          if (sim.DataManager.ContractOverrides.TryGet(contractData.ContractName, out var contractOverride)) {
            contractOverride.FullRehydrate();
            Log.M?.WL(1, $"{contractOverride.ID} disableCancelButton:{contractOverride.disableCancelButton}");
            if (contractOverride.disableCancelButton == false) { return; }
            sim.SaveActiveContractName = $"Forced:{contractData.ContractName}";
            string ForcedContract = action.ToEditorString();
            sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).SetValue<string>(action.ToEditorString());
            var save_result = sim.TriggerSaveNow(BattleTech.Save.SaveGameStructure.SaveReason.SIM_GAME_COMPLETED_CONTRACT, SimGameState.TriggerSaveNowOption.QUEUE_IF_NEEDED);
            Log.M?.WL(1, $"TriggerSaveNow:'{sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).Value<string>()}' save result:{save_result}");
            sim.interruptQueue.QueuePauseNotification("__/CAC.CONCEQUENCE.DROP.TITLE/__", $"__/CAC.CONCEQUENCE.DROP.MESSAGE/__", sim.GetCrewPortrait(SimGameCrew.Crew_Darius), null, () => {
              try {
                sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).SetValue<string>(string.Empty);
                PreForceTakeContractSave.SkipSave = true;
                SimGameState.ApplyEventAction(action, additionalObject);
                PreForceTakeContractSave.SkipSave = false;
              } catch (Exception ex) {
                SimGameState.logger.LogException(ex);
                Log.M?.TWL(0, ex.ToString());
              }
            });
            __runOriginal = false;

          }
          //Log.M?.WL(1, $"{APPLY_EVENT_ACTION_STAT_NAME}:{sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).Value<string>()} save result:{save_result}");
        }
      } catch(Exception e) {
        SimGameState.logger.LogException(e);
        Log.M?.TWL(0,e.ToString());
      }
    }
    public static void Rehydrate_postfix(SimGameState sim) {
      try {
        if (CustomAmmoCategories.Settings.ConsequenceDropAutosave == false) { return; }
        string ForcedContract = sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).Value<string>();
        Log.M?.TWL(0, $"PreForceTakeContractSave.Rehydrate:{ForcedContract}");
        if (string.IsNullOrEmpty(ForcedContract)) { return; }
        //GenericPopupBuilder.Create(GenericPopupType.Warning, $"DROP IS IMMINENT\n{ForcedContract}").SetOnClose(() => {
        //  try {
        //    sim.CommanderStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).SetValue<string>(string.Empty);
        //    PreForceTakeContractSave.SkipSave = true;
        //    EventEditorValidationResults validationResults = new EventEditorValidationResults();
        //    var action = SimGameResultAction.FromEditorString(ForcedContract, validationResults);
        //    SimGameState.ApplyEventAction(action, null);
        //    PreForceTakeContractSave.SkipSave = false;
        //  } catch (Exception ex) {
        //    SimGameState.logger.LogException(ex);
        //    Log.M?.TWL(0, ex.ToString());
        //  }
        //});
        sim.interruptQueue.QueuePauseNotification("__/CAC.CONCEQUENCE.DROP.TITLE/__", $"__/CAC.CONCEQUENCE.DROP.MESSAGE/__", sim.GetCrewPortrait(SimGameCrew.Crew_Darius), null, () => {
          try {
            sim.CompanyStats.GetOrCreateStatisic<string>(APPLY_EVENT_ACTION_STAT_NAME, string.Empty).SetValue<string>(string.Empty);
            PreForceTakeContractSave.SkipSave = true;
            EventEditorValidationResults validationResults = new EventEditorValidationResults();
            var action = SimGameResultAction.FromEditorString(ForcedContract, validationResults);
            SimGameState.ApplyEventAction(action, null);
            PreForceTakeContractSave.SkipSave = false;
          } catch (Exception ex) {
            SimGameState.logger.LogException(ex);
            Log.M?.TWL(0, ex.ToString());
          }
        });
      } catch (Exception e) {
        SimGameState.logger.LogException(e);
        Log.M?.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(SGRoomManager))]
  [HarmonyPatch("OnSimGameReady")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SGRoomManager_OnSimGameReady {
    public static void Postfix(SGRoomManager __instance) {
      PreForceTakeContractSave.Rehydrate_postfix(__instance.simState);
    }
  }
  [HarmonyPatch(typeof(GameInstance))]
  [HarmonyPatch("Save")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SaveReason), typeof(string), typeof(bool) })]
  public static class GameInstance_Save {
    public static void Postfix(GameInstance __instance, SaveReason reason, string saveNameOverride, bool quit) {
      try {
        if (CustomAmmoCategories.Settings.ConsequenceDropAutosave == false) { return; }
        if (__instance.Simulation != null) {
          __instance.Simulation.CompanyStats.GetOrCreateStatisic<string>(PreForceTakeContractSave.APPLY_EVENT_ACTION_STAT_NAME, string.Empty).SetValue<string>(string.Empty);
        }
      }catch(Exception e) {
        GameInstance.gameInfoLogger.LogException(e);
        Log.M?.TWL(0,e.ToString());
      }
    }
  }

}