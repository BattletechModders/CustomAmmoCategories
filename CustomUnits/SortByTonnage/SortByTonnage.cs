using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;
using BattleTech.UI;
using CustomUnits;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using static SortByTonnage.SortByTonnage;

namespace SortByTonnage {
  public class Settings {
    public bool debug = false;

    public bool orderByCbillValue = false;
    public bool OrderByCbillValue => orderByCbillValue;

    public bool orderByNickname = false;
    public bool OrderByNickname => orderByNickname;

    public bool orderByTonnage = false;
    public bool OrderByTonnage => orderByTonnage;
    public bool isAnySortingEnabled { get { return OrderByCbillValue || OrderByNickname || OrderByTonnage; } }
  }
  public static class SortByTonnage {
    public const string ModName = "SortByTonnage";
    private const string ModId = "com.joelmeador.SortByTonnage";

    internal static Settings ModSettings = new Settings();
    internal static string ModDirectory;

    public static void Init(string directory, Settings settings) {
      ModDirectory = directory;
      try {
        //ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
        ModSettings = settings;
      } catch (Exception ex) {
        Log.TWL(0,ex.ToString(),true);
        ModSettings = new Settings();
      }
    }

    private enum MechState {
      Active,
      Readying,
    }

    private static float CalculateCBillValue(MechDef mech) {
      const float num = 10000f;
      var currentCBillValue = (float)mech.Chassis.Description.Cost;
      var armorValues = new[]
      {
                mech.Head.CurrentArmor,
                mech.CenterTorso.CurrentArmor,
                mech.CenterTorso.CurrentRearArmor,
                mech.LeftTorso.CurrentArmor,
                mech.LeftTorso.CurrentRearArmor,
                mech.RightTorso.CurrentArmor,
                mech.RightTorso.CurrentRearArmor,
                mech.LeftArm.CurrentArmor,
                mech.RightArm.CurrentArmor,
                mech.LeftLeg.CurrentArmor,
                mech.RightLeg.CurrentArmor
            };
      currentCBillValue += armorValues.Sum() * UnityGameInstance.BattleTechGame.MechStatisticsConstants.CBILLS_PER_ARMOR_POINT;
      currentCBillValue += mech.Inventory.Sum(mechComponentRef => (float)mechComponentRef.Def.Description.Cost);
      currentCBillValue = Mathf.Round(currentCBillValue / num) * num;
      return currentCBillValue;
    }

    private static bool _isSortEnabled = true;

    public static void DisableSort() { _isSortEnabled = false; }
    public static void EnableSort() { _isSortEnabled = true; }

    private static List<IMechLabDraggableItem> SortMechDefs(List<IMechLabDraggableItem> mechs) {
      try {
        Log.TWL(0, $"pre-sort count: {mechs.Count}");
        if (ModSettings.isAnySortingEnabled == false) {
          Log.TWL(0, "sorting disabled");
          return mechs;
        }
        HashSet<IMechLabDraggableItem> sortable = new HashSet<IMechLabDraggableItem>();
        HashSet<IMechLabDraggableItem> unsortable = new HashSet<IMechLabDraggableItem>();
        foreach (IMechLabDraggableItem item in mechs) {
          if (item.MechDef == null) { unsortable.Add(item); continue; }
          if (item.MechDef.Chassis == null) {
            Log.WL(1,"");
            unsortable.Add(item); continue;
          }
        }
        if (ModSettings.OrderByNickname) {
          return
              mechs
                  .OrderByDescending(mech => mech.MechDef.Name)
                  .ThenBy(mech => mech.MechDef.Chassis.Tonnage)
                  .ThenBy(mech => mech.MechDef.Chassis.VariantName)
                  .ToList();
        }

        if (ModSettings.OrderByCbillValue) {
          return
              mechs
                  .OrderBy(mech => CalculateCBillValue(mech.MechDef))
                  .ThenBy(mech => mech.MechDef.Chassis.Tonnage)
                  .ThenByDescending(mech => mech.MechDef.Chassis.VariantName)
                  .ThenBy(mech => mech.MechDef.Name)
                  .ToList();
        }

        return
            mechs
                .OrderBy(mech => mech.MechDef.Chassis.Tonnage)
                .ThenByDescending(mech => mech.MechDef.Chassis.VariantName)
                .ThenByDescending(mech => mech.MechDef.Name)
                .ToList();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return mechs;
      }

    }

    private static List<IMechLabDraggableItem> SortChassisDefs(List<IMechLabDraggableItem> chassis) {
      try {
        Log.TWL(0, $"pre-sort chassis count: {chassis.Count}");
        if (ModSettings.isAnySortingEnabled == false) {
          Log.TWL(0, "sorting disabled");
          return chassis;
        }
        if (ModSettings.OrderByNickname) {
          return
              chassis
                  .OrderByDescending(ch => ch.ChassisDef.Description.Name)
                  .ThenBy(ch => ch.ChassisDef.Tonnage)
                  .ThenBy(ch => ch.ChassisDef.VariantName)
                  .ToList();
        }

        if (ModSettings.OrderByCbillValue) {
          return
              chassis
                  .OrderByDescending(ch => ch.ChassisDef.Description.Cost)
                  .ThenBy(ch => ch.ChassisDef.Tonnage)
                  .ThenByDescending(ch => ch.ChassisDef.VariantName)
                  .ThenBy(ch => ch.ChassisDef.Description.Name)
                  .ToList();
        }

        return
            chassis
                .OrderBy(ch => ch.ChassisDef.Tonnage)
                .ThenByDescending(ch => ch.ChassisDef.VariantName)
                .ThenByDescending(ch => ch.ChassisDef.Description.Name)
                .ToList();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return chassis;
      }

    }
    private static List<Tuple<MechState, MechDef>> SortMechDefs(Dictionary<int, Tuple<MechState, MechDef>> mechs) {
      try {
        Log.TWL(0, $"pre-sort count: {mechs.Count}");

        for (var i = 0; i < mechs.Count; i++) {
          if (mechs.ContainsKey(i)) {
            if (mechs[i].Item1 == MechState.Active) {
              Log.WL(1, $"found active {i} : {mechs[i].Item2.Name}");
            } else if (mechs[i].Item1 == MechState.Readying) {
              Log.WL(1, $"found readying {i} : {mechs[i].Item2.Name}");
            }
          } else {
            Log.WL(1, $"key {i} not found");
          }
        }
        if (ModSettings.isAnySortingEnabled == false) {
          Log.TWL(0, "sorting disabled");
          return mechs.Values.ToList();
        }

        if (ModSettings.OrderByNickname) {
          return
              mechs
                  .Values
                  .OrderBy(mech => mech.Item2.Name)
                  .ThenBy(mech => mech.Item2.Chassis.Tonnage)
                  .ThenBy(mech => mech.Item2.Chassis.VariantName)
                  .ToList();
        }

        if (ModSettings.OrderByCbillValue) {
          return
              mechs
                  .Values
                  .OrderByDescending(mech => CalculateCBillValue(mech.Item2))
                  .ThenBy(mech => mech.Item2.Chassis.Tonnage)
                  .ThenBy(mech => mech.Item2.Chassis.VariantName)
                  .ThenBy(mech => mech.Item2.Name)
                  .ToList();
        }

        return
            mechs
                .Values
                .OrderByDescending(mech => mech.Item2.Chassis.Tonnage)
                .ThenBy(mech => mech.Item2.Chassis.VariantName)
                .ThenBy(mech => mech.Item2.Name)
                .ToList();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return mechs.Values.ToList();
      }
    }

    private static Dictionary<int, Tuple<MechState, MechDef>> CombinedSlots(int mechSlots, Dictionary<int, MechDef> active, Dictionary<int, MechDef> readying) {
      var combined = new Dictionary<int, Tuple<MechState, MechDef>>();
      for (var i = 0; i <= mechSlots; i++) {
        if (active.ContainsKey(i)) {
          Log.WL(0, $"found active {i}");
          combined[i] = new Tuple<MechState, MechDef>(MechState.Active, active[i]);
        } else if (readying.ContainsKey(i)) {
          Log.WL(0, $"found readying {i}");
          combined[i] = new Tuple<MechState, MechDef>(MechState.Readying, readying[i]);
        }
      }
      Log.WL(0, $"combined size: {combined.Count}");
      return combined;
    }

    public static List<IMechLabDraggableItem> SortStorageWidgetMechs(List<IMechLabDraggableItem> chassis) {
      if (!_isSortEnabled) {
        Log.TWL(0, "sorting disabled");
        return chassis;
      }
      if(ModSettings.isAnySortingEnabled == false) {
        Log.TWL(0, "sorting disabled");
        return chassis;
      }
      return SortChassisDefs(chassis);
    }

    public static void SortMechLabMechs(int mechSlots, Dictionary<int, MechDef> activeMechs, Dictionary<int, MechDef> readyingMechs) {
      if (!_isSortEnabled) {
        Log.TWL(0, "sorting disabled");
        return;
      }
      if (ModSettings.isAnySortingEnabled == false) {
        Log.TWL(0, "sorting disabled");
        return;
      }

      var sortedMechs = SortMechDefs(CombinedSlots(mechSlots, activeMechs, readyingMechs));
      Log.TWL(0, $"sortedMechs #: {sortedMechs.Count}");
      for (var ii = 0; ii < sortedMechs.Count; ii++) {
        Log.WL(1, $"mech: {sortedMechs[ii].Item2.Name} / {sortedMechs[ii].Item2.Chassis.VariantName}\nreadying? {sortedMechs[ii].Item1 == MechState.Readying}\nactive? {sortedMechs[ii].Item1 == MechState.Active}");
      }

      for (var i = 0; i <= mechSlots; i++) {
        if (i < sortedMechs.Count) {
          if (activeMechs.ContainsKey(i)) {
            activeMechs.Remove(i);
          } else if (readyingMechs.ContainsKey(i)) {
            readyingMechs.Remove(i);
          }

          if (sortedMechs[i].Item1 == MechState.Active) {
            activeMechs.Add(i, sortedMechs[i].Item2);
          } else if (sortedMechs[i].Item1 == MechState.Readying) {
            readyingMechs.Add(i, sortedMechs[i].Item2);
          }
        } else {
          if (activeMechs.ContainsKey(i)) {
            activeMechs.Remove(i);
          } else if (readyingMechs.ContainsKey(i)) {
            readyingMechs.Remove(i);
          }
        }
      }
    }
  }

  // This is the entry point for sorting mechs in the lance configuration
  // screen (prep for combat drop)
  [HarmonyPatch(typeof(MechBayMechStorageWidget), "SetSorting")]
  public static class MechBayMechStorageWidget_SetSorting_Patch {
    static bool Prefix(MechBayMechStorageWidget __instance) {
      if (ModSettings.isAnySortingEnabled == false) {
        Log.TWL(0, "sorting disabled");
        return true;
      }
      __instance.inventory = SortStorageWidgetMechs(__instance.inventory);
      Traverse.Create(__instance).Method("ApplySort").GetValue();
      return false;
    }
  }

  // _OnDefsLoadComplete happens when you are loading a game from disk/starting a new game/etc.
  [HarmonyPatch(typeof(SimGameState), "_OnDefsLoadComplete")]
  public static class SimGameState__OnDefsLoadComplete_Patch {
    static void Prefix() {
      Log.TWL(0, "_OnDefsLoadComplete Prefix Patch Installed");
      DisableSort();
    }
    static void Postfix(SimGameState __instance) {
      Log.TWL(0, "_OnDefsLoadComplete Postfix Patch Installed");
      EnableSort();
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // AddMech happens when you are given a mech via salvage, and via milestone rewards
  [HarmonyPatch(typeof(SimGameState), "AddMech")]
  public static class SimGameState_AddMech_Patch {
    //static bool Prefix(int idx, MechDef mech, bool active, bool forcePlacement, bool displayMechPopup, string mechAddedHeader, SimGameState __instance) {
      //Log.TWL(0, "AddMech Prefix Patch Installed");
      //if (displayMechPopup) {
      //  if (string.IsNullOrEmpty(mech.GUID)) {
      //    mech.SetGuid(__instance.GenerateSimGameUID());
      //  }
      //  var companyStats = Traverse.Create(__instance).Field("companyStats").GetValue<StatCollection>();
      //  companyStats.ModifyStat<int>("Mission", 0, "COMPANY_MechsAdded", StatCollection.StatOperation.Int_Add, 1, -1, true);
      //  if (string.IsNullOrEmpty(mechAddedHeader)) {
      //    mechAddedHeader = "'Mech Chassis Complete";
      //    int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_sim_popup_newChassis, WwiseManager.GlobalAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //  }
      //  mechAddedHeader += ": {0}";
      //  __instance.GetInterruptQueue().QueuePauseNotification(
      //      string.Format(mechAddedHeader, (object)mech.Description.UIName), mech.Chassis.YangsThoughts,
      //      __instance.GetCrewPortrait(SimGameCrew.Crew_Yang), "notification_mechreadycomplete", (Action)(() => {
      //        int firstFreeMechBay = __instance.GetFirstFreeMechBay();
      //        if (firstFreeMechBay >= 0) {
      //          __instance.ActiveMechs[firstFreeMechBay] = mech;
      //          SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
      //        } else
      //          __instance.CreateMechPlacementPopup(mech);
      //      }), "Continue", (Action)null, (string)null);
      //  return false;
      //}
      //return true;

    //}

    static void Postfix(int idx, MechDef mech, bool active, bool forcePlacement, bool displayMechPopup, string mechAddedHeader, SimGameState __instance) {
      Log.TWL(0, "AddMech Postfix Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // ReadyMech is the function called when one readies a mech from the stored mechs
  [HarmonyPatch(typeof(SimGameState), "ReadyMech")]
  public static class SimGameState_ReadyMech_Patch {
    static void Prefix() {
      Log.TWL(0, "ReadyMech Prefix Patch Installed");
      DisableSort();
    }
    static void Postfix(int baySlot, string id, SimGameState __instance) {
      Log.TWL(0, "ReadyMech Postfix Patch Installed");
      EnableSort();
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // AFAICT this is never called by anything, so it probably isn't necessary,
  // but completeness
  [HarmonyPatch(typeof(SimGameState), "RemoveMech")]
  public static class SimGameState_RemoveMech_Patch {
    static void Postfix(int idx, MechDef mech, bool active, SimGameState __instance) {
      Log.TWL(0, "RemoveMech Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // ScrapActiveMech is called when the scrap button is clicked on the selected
  // mech in the mech bay
  [HarmonyPatch(typeof(SimGameState), "ScrapActiveMech")]
  public static class SimGameState_ScrapActiveMech_Patch {
    static void Postfix(int baySlot, MechDef def, SimGameState __instance) {
      Log.TWL(0, "ScrapActiveMech Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // ScrapInactiveMech is called by ReadyMech to remove placeholder,
  // by scrapping chassis in the storage, and when scrapping a mech
  // to make room for another one upon salvaged mech completion.
  [HarmonyPatch(typeof(SimGameState), "ScrapInactiveMech")]
  public static class SimGameState_ScrapInativeMech_Patch {
    static void Postfix(string id, bool pay, SimGameState __instance) {
      Log.TWL(0, "ScrapInactiveMech Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // Called when the strip button is clicked on an active mech.
  [HarmonyPatch(typeof(SimGameState), "StripMech")]
  public static class SimGameState_StripMech_Patch {
    static void Postfix(int baySlot, MechDef def, SimGameState __instance) {
      Log.TWL(0, "StripMech Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // Called when a readying mech is canceled before it's readied, 
  // including mech being worked on.
  [HarmonyPatch(typeof(SimGameState), "UnreadyMech")]
  public static class SimGameState_UnreadyMech_Patch {
    static void Postfix(int baySlot, MechDef def, SimGameState __instance) {
      Log.TWL(0, "UnreadyMech Patch Installed");
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
    }
  }

  // This is what happens when the mech lab finishes a mech work order as part of
  // a timeline event *I think*.
  [HarmonyPatch(typeof(SimGameState), "ML_ReadyMech")]
  public static class SimGamestate_ML_ReadyMech_Patch {
    static bool Prefix(WorkOrderEntry_ReadyMech order, SimGameState __instance) {
      Log.TWL(0, "ML_ReadyMech Patch Installed");
      if (order.IsMechLabComplete) {
        return false;
      }

      var index = __instance.ReadyingMechs.First(item => item.Value == order.Mech).Key;
      __instance.ReadyingMechs.Remove(index);
      __instance.ActiveMechs[index] = order.Mech;
      order.Mech.RefreshBattleValue();
      order.SetMechLabComplete(true);
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
      return false;
    }
  }

  // The actual thing when you cancel a ready mech job?
  // we override it because the original stuff uses bay index which we can't rely on
  [HarmonyPatch(typeof(SimGameState), "Cancel_ML_ReadyMech", new Type[] { typeof(WorkOrderEntry_ReadyMech) })]
  public static class SimGamestate_Cancel_ML_ReadyMech_Patch {
    static bool Prefix(WorkOrderEntry_ReadyMech order, SimGameState __instance) {
      Log.TWL(0, "ML_Cancel_ReadyMech Patch Installed");
      if (order.IsMechLabComplete) {
        Log.TWL(0, "wtf is happening how");
        return false;
      }

      var item = __instance.ReadyingMechs.First(readying => readying.Value == order.Mech);
      var index = item.Key;
      Log.TWL(0, $"cancel index: {index}\nmech? {item.Value.GUID} : {order.Mech.GUID} : {order.Mech.Chassis.VariantName}");
      __instance.UnreadyMech(index, order.Mech);
      __instance.ReadyingMechs.Remove(index);
      SortMechLabMechs(__instance.GetMaxActiveMechs(), __instance.ActiveMechs, __instance.ReadyingMechs);
      return false;
    }
  }

  // happens when you leave the mech lab for any reason
  [HarmonyPatch(typeof(MechBayPanel), "OnMechLabClosed")]
  public static class MechBayPanel_OnMechLabClosed_Patch {
    static void Prefix(MechBayPanel __instance) {
      Log.TWL(0, "OnMechLabClosed Patch Installed");
      if (!__instance.IsSimGame) {
        return;
      }
      SortMechLabMechs(__instance.Sim.GetMaxActiveMechs(), __instance.Sim.ActiveMechs, __instance.Sim.ReadyingMechs);
    }
  }

  // when your mech bays are full and you salvage, you get placement popup
  // which has a completely different set of rules around 
  // things that the other methods, so we have to handle it separately
  // this handles removing a mech that is in readying and gets kicked
  [HarmonyPatch(typeof(MechPlacementPopup), "ConfirmCancelWorkOrder")]
  public static class MechPlacementPopup_ConfirmCancelWorkOrder_Patch {
    static void Prefix() {
      DisableSort();
    }

    static void Postfix(MechPlacementPopup __instance) {
      EnableSort();
      SortMechLabMechs(__instance.Sim.GetMaxActiveMechs(), __instance.Sim.ActiveMechs, __instance.Sim.ReadyingMechs);
    }
  }

  // when your mech bays are full and you salvage, you get placement popup
  // which has a completely different set of rules around 
  // things that the other methods, so we have to handle it separately
  // this handles scapping a mech
  [HarmonyPatch(typeof(MechPlacementPopup), "ConfirmScrapMech")]
  public static class MechPlacementPopup_ConfirmScrapMech_Patch {
    static void Prefix() {
      DisableSort();
    }

    static void Postfix(MechPlacementPopup __instance) {
      EnableSort();
      SortMechLabMechs(__instance.Sim.GetMaxActiveMechs(), __instance.Sim.ActiveMechs, __instance.Sim.ReadyingMechs);
    }
  }

  // when your mech bays are full and you salvage, you get placement popup
  // which has a completely different set of rules around 
  // things that the other methods, so we have to handle it separately
  // this handles scapping a mech
  [HarmonyPatch(typeof(MechPlacementPopup), "OnConfirmClicked")]
  public static class MechPlacementPopup_OnConfirmClicked_Patch {
    static void Prefix() {
      DisableSort();
    }

    static void Postfix(MechPlacementPopup __instance) {
      EnableSort();
      SortMechLabMechs(__instance.Sim.GetMaxActiveMechs(), __instance.Sim.ActiveMechs, __instance.Sim.ReadyingMechs);
    }
  }
}