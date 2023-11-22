using BattleTech;
using BattleTech.Save;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(SimGameState), "ResolveCompleteContract")]
  public static class SimGameState_ResolveCompleteContract {
    public static void Prefix(SimGameState __instance) {
      try {
        Log.M?.TWL(0, $"SimGameState.ResolveCompleteContract");
        __instance.clearSkipped();
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(GameInstanceSave), "PreSerialization")]
  public static class GameInstanceSave_PreSerialization {
    public static void Prefix(GameInstanceSave __instance) {
      try {
        if(__instance.SaveReason == BattleTech.Save.SaveGameStructure.SaveReason.SIM_GAME_COMPLETED_CONTRACT) {
          Statistic stat = __instance.SimGameSave.simGameState.CompanyStats.GetStatistic("RETRIGGER_RestoreMechPostCombat");
          if(stat == null) {
            stat = __instance.SimGameSave.simGameState.CompanyStats.AddStatistic("RETRIGGER_RestoreMechPostCombat", JsonConvert.SerializeObject(__instance.SimGameSave.simGameState.getSkipped()));
          } else {
            stat.SetValue<string>(JsonConvert.SerializeObject(__instance.SimGameSave.simGameState.getSkipped()));
          }
          Log.M?.TWL(0, $"GameInstanceSave.PreSerialization {JsonConvert.SerializeObject(__instance.SimGameSave.simGameState.getSkipped(), Formatting.Indented)}");
        }
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "SetCharacterCreationWidget")]
  public static class SimGameState_SetCharacterCreationWidget {
    public static void Postfix(SimGameState __instance) {
      try {
        if(SimGameState_RestoreMechPostCombat.TRIGGER_ARMORREPAIR == false) { return; }
        SimGameState_RestoreMechPostCombat.TRIGGER_ARMORREPAIR = false;
        __instance.RetriggerRestoreMechPostCombat();
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState), "RestoreMechPostCombat")]
  [HarmonyBefore("io.github.citizenSnippy.ArmorRepair")]
  public static class SimGameState_RestoreMechPostCombat {
    public static bool SKIP = false;
    public static bool TRIGGER_ARMORREPAIR = false;
    public static MethodInfo ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Prefix = null;
    public static MethodInfo ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Postfix = null;
    public static void Init() {
      foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if(assembly.FullName.StartsWith("ArmorRepair, Version=")) {
          Type type = assembly.GetType("ArmorRepair.SimGameState_ResolveCompleteContract_Patch");
          if(type == null) { Log.M?.TWL(0, "Fail to find ArmorRepair.SimGameState_ResolveCompleteContract_Patch"); continue; }
          Log.M?.TWL(0, "ArmorRepair.SimGameState_ResolveCompleteContract_Patch found");
          ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Prefix = type.GetMethod("Prefix");
          if(ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Prefix == null) {
            Log.M?.TWL(0, "ArmorRepair.SimGameState_ResolveCompleteContract_Patch.Prefix not found");
          } else {
            Log.M?.WL(1, "ArmorRepair.SimGameState_ResolveCompleteContract_Patch.Prefix found");
          }
          ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Postfix = type.GetMethod("Postfix");
          if(ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Postfix == null) {
            Log.M?.TWL(0, "ArmorRepair.SimGameState_ResolveCompleteContract_Patch.Postfix not found");
          } else {
            Log.M?.WL(1, "ArmorRepair.SimGameState_ResolveCompleteContract_Patch.Postfix found");
          }
        }
      }
    }
    public class LocationArmorInfo {
      public float front = 0f;
      public float rear = 0f;
      public LocationArmorInfo() { front = 0f; rear = 0f; }
      public LocationArmorInfo(LocationLoadoutDef location) { front = location.AssignedArmor; rear = location.AssignedRearArmor; }
    }
    public class UnitArmorInfo {
      public Dictionary<ChassisLocations, LocationArmorInfo> assignedArmor = new Dictionary<ChassisLocations, LocationArmorInfo>();
      public UnitArmorInfo() { assignedArmor = new Dictionary<ChassisLocations, LocationArmorInfo>(); }
      public UnitArmorInfo(MechDef mech) {
        assignedArmor = new Dictionary<ChassisLocations, LocationArmorInfo>();
        foreach(var location in mech.Locations) {
          assignedArmor[location.Location] = new LocationArmorInfo(location);
        }
      }
    }
    private static Dictionary<string, UnitArmorInfo> skippedRestoreMechPostCombat = new Dictionary<string, UnitArmorInfo>();
    public static void clearSkipped(this SimGameState sim) { skippedRestoreMechPostCombat.Clear(); }
    public static Dictionary<string, UnitArmorInfo> getSkipped(this SimGameState sim) { return skippedRestoreMechPostCombat; }
    public static void RetriggerRestoreMechPostCombat(this SimGameState sim) {
      try {
        if(ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Prefix == null) { return; }
        if(ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Postfix == null) { return; }
        Statistic stat = sim.CompanyStats.GetStatistic("RETRIGGER_RestoreMechPostCombat");
        if(stat == null) { return; }
        var skipped = JsonConvert.DeserializeObject<Dictionary<string, UnitArmorInfo>>(stat.Value<string>());
        Log.M?.TWL(0, $"RetriggerRestoreMechPostCombat: {JsonConvert.SerializeObject(skipped)}");
        ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Prefix.Invoke(null, new object[] { sim });
        foreach(var mech in sim.ActiveMechs) {
          if(skipped.TryGetValue(mech.Value.GUID, out var assignedInfo)) {
            Log.M?.WL(1, $"{mech.Value.Description.Id}:{mech.Value.ChassisID}:{mech.Value.GUID}");
            SimGameState_RestoreMechPostCombat.SKIP = true;
            foreach(var location in mech.Value.Locations) {
              if(assignedInfo.assignedArmor.TryGetValue(location.Location, out var info)) {
                Log.M?.WL(2, $"{location.Location} armor:{location.CurrentArmor}/{location.AssignedArmor}({info.front}) rear:{location.CurrentRearArmor}/{location.AssignedRearArmor}({info.rear}) structure:{location.CurrentInternalStructure}");
                location.AssignedArmor = info.front;
                location.AssignedRearArmor = info.rear;
              }
            }
            sim.RestoreMechPostCombat(mech.Value);
            SimGameState_RestoreMechPostCombat.SKIP = false;
          }
        }
        ArmorRepair_SimGameState_ResolveCompleteContract_Patch_Postfix.Invoke(null, new object[] { sim });
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
    public static void RetriggerRestoreMechPostCombat(this GameInstanceSave save, SimGameState sim) {
      try {
        if(save.SaveReason != BattleTech.Save.SaveGameStructure.SaveReason.SIM_GAME_COMPLETED_CONTRACT) { return; }
        TRIGGER_ARMORREPAIR = true;
      } catch(Exception e) {
        Log.M?.TWL(0,e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
    public static void Prefix(ref bool __runOriginal, SimGameState __instance, MechDef mech, ref UnitArmorInfo __state) {
      try {
        __state = null;
        if(SimGameState_RestoreMechPostCombat.SKIP) { return; }
        Log.M?.TWL(0, $"SimGameState.RestoreMechPostCombat prefix {mech.Description.Id}:{mech.GUID} runOriginal:{__runOriginal}");
        if(__runOriginal == false) { return; }
        foreach(var location in mech.Locations) {
          Log.M?.WL(1, $"{location.Location} armor:{location.CurrentArmor}/{location.AssignedArmor} rear:{location.CurrentRearArmor}/{location.AssignedRearArmor} structure:{location.CurrentInternalStructure}");
        }
        __state = new UnitArmorInfo(mech);
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
    public static void Postfix(ref bool __runOriginal, SimGameState __instance, MechDef mech, ref UnitArmorInfo __state) {
      try {
        if(SimGameState_RestoreMechPostCombat.SKIP) { return; }
        if(__state == null) { return; }
        Log.M?.TWL(0,$"SimGameState.RestoreMechPostCombat postfix {mech.Description.Id}:{mech.GUID} runOriginal:{__runOriginal}");
        if(__runOriginal == false) { skippedRestoreMechPostCombat[mech.GUID] = __state; }
      } catch(Exception e) {
        Log.M?.TWL(0,e.ToString());
        SimGameState.logger.LogException(e);
      }
    }
  }
}