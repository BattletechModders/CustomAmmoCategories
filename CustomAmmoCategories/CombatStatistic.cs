using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomSettings;
using HarmonyLib;
using IRBTModUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Log = CustomAmmoCategoriesLog.Log;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(Briefing), "InitializeContractComplete")]
  public static class Briefing_InitializeContractComplete {
    public static void Prefix(Briefing __instance, MessageCenterMessage message) {
      Log.Combat?.TWL(0, $"Briefing.InitializeContractComplete clearing combat statistic");
      try {
        CombatStatisticHelper.Clear();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Contract), "RequestConversations")]
  public static class Contract_RequestConversations {
    public static void Postfix(Contract __instance, LoadRequest loadRequest) {
      Log.Combat?.TWL(0,$"Contract.RequestConversations");
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite) == false) {
        Log.Combat?.WL(1, $"sprite:{CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite}");
        if (__instance.DataManager.Exists(BattleTechResourceType.Sprite, CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite) == false) {
          if (__instance.DataManager.ResourceLocator.EntryByID(CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite, BattleTechResourceType.Sprite) != null) {
            Log.Combat?.WL(2, $"exist in manifest but not loaded");
            loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite);
          } else {
            Log.Combat?.WL(2, $"not exist in manifest");
          }
        } else {
          Log.Combat?.WL(2, $"already loaded");
        }
      }
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite) == false) {
        Log.Combat?.WL(1, $"sprite:{CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite}");
        if (__instance.DataManager.Exists(BattleTechResourceType.Sprite, CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite) == false) {
          if (__instance.DataManager.ResourceLocator.EntryByID(CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite, BattleTechResourceType.Sprite) != null) {
            Log.Combat?.WL(2, $"exist in manifest but not loaded");
            loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite);
          } else {
            Log.Combat?.WL(2, $"not exist in manifest");
          }
        } else {
          Log.Combat?.WL(2, $"already loaded");
        }
      }
    }
  }
  [HarmonyPatch(typeof(AAR_UnitStatusWidget))]
  [HarmonyPatch("InitData")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AAR_UnitStatusWidget_InitData {
    public static void Postfix(AAR_UnitStatusWidget __instance, UnitResult result, SimGameState theSimGame, DataManager dataMan, Contract theContract) {
      try {
        if (theSimGame == null) { return; }
        if (dataMan == null) { return; }
        if (theContract == null) { return; }
        if (result == null) { return; }
        if (result.mech == null) { return; }
        if (CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false) { return; }
        Log.Combat?.TWL(0,$"AAR_UnitStatusWidget.InitData GUID:{result.mech.GUID}");
        UnitCombatStatistic stat = result.stat();
        if (stat == null) { return; }
        LocalizableText killText = __instance.gameObject.FindObject<LocalizableText>("killsLabel_text");
        if(killText != null) {
          HBSTooltip tooltip = killText.gameObject.GetComponent<HBSTooltip>();
          if (tooltip == null) { tooltip = killText.gameObject.AddComponent<HBSTooltip>(); }
          string description = $"MECHS KILLED:{result.pilot.MechsKilled}\n"+ $"OTHERS KILLED:{result.pilot.OthersKilled}\n" + stat.GenerateDescription();
          BaseDescriptionDef descriptionDef = new BaseDescriptionDef(result.mech.GUID+"_statData", "BATTLE STATISTIC", description, string.Empty);
          tooltip.SetDefaultStateData(descriptionDef.GetTooltipStateData());
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AAR_UnitStatusWidget))]
  [HarmonyPatch("FillInData")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AAR_UnitStatusWidget_FillInData {
    public static void AddKilled(DataManager dm, Transform KillGridParent, UnitCombatStatistic.KilledUnit unit) {
      string prefabid = "uixPrfIcon_AA_mechKillStamp";
      
      switch (unit.Icon) {
        case UnitCombatStatistic.KilledUnitIconType.Mech: prefabid = "uixPrfIcon_AA_mechKillStamp"; break;
        case UnitCombatStatistic.KilledUnitIconType.Vehicle: prefabid = "uixPrfIcon_AA_vehicleKillStamp"; break;
        default: prefabid = "uixPrfIcon_AA_vehicleKillStamp"; break;
      }
      GameObject gameObject = dm.PooledInstantiate(prefabid, BattleTechResourceType.UIModulePrefabs, parent: ((Transform)KillGridParent));
      if (gameObject == null) { return; }
      Image background = gameObject.GetComponent<Image>();
      if (background == null) { return; }
      Image[] images = gameObject.GetComponentsInChildren<Image>(true);
      Image foreground = null;
      foreach (var img in images) {
        if (img.gameObject != gameObject) { foreground = img; break; }
      }
      if (foreground == null) { return; }
      string update_sprite = string.Empty;
      switch (unit.Icon) {
        case UnitCombatStatistic.KilledUnitIconType.Turret: update_sprite = CustomAmmoCategories.Settings.StatisticOnResultScreenTurretSprite; break;
        case UnitCombatStatistic.KilledUnitIconType.Squad: update_sprite = CustomAmmoCategories.Settings.StatisticOnResultScreenBattleArmorSprite; break;
      }
      if(string.IsNullOrEmpty(update_sprite) == false) {
        if(dm.Exists(BattleTechResourceType.Sprite, update_sprite)) {
          foreground.sprite = dm.GetObjectOfType<Sprite>(update_sprite, BattleTechResourceType.Sprite);
        }
      }
      HBSTooltip tooltip = gameObject.GetComponent<HBSTooltip>();
      if (tooltip == null) { tooltip = gameObject.AddComponent<HBSTooltip>(); }
      DescriptionDef description = null;
      if((string.IsNullOrEmpty(unit.MechDefId) == false) && (dm.MechDefs.Exists(unit.MechDefId))) {
        MechDef definition = dm.MechDefs.Get(unit.MechDefId);
        tooltip.SetDefaultStateData(definition.GetTooltipStateData());
        description = definition.Description;
      } else if((string.IsNullOrEmpty(unit.VehicleDefId) == false) && (dm.VehicleDefs.Exists(unit.VehicleDefId))) {
        VehicleDef definition = dm.VehicleDefs.Get(unit.VehicleDefId);
        //tooltip.SetDefaultStateData(definition.Description.GetTooltipStateData());
        tooltip.SetDefaultStateData(new BaseDescriptionDef(definition.Description.Id, definition.Description.Name, definition.Description.Details, definition.Description.Icon).GetTooltipStateData());
        description = definition.Description;
      } else if((string.IsNullOrEmpty(unit.TurretDefId) == false) && (dm.TurretDefs.Exists(unit.TurretDefId))) {
        TurretDef definition = dm.TurretDefs.Get(unit.TurretDefId);
        tooltip.SetDefaultStateData(new BaseDescriptionDef(definition.Description.Id, definition.Description.Name, definition.Description.Details, definition.Description.Icon).GetTooltipStateData());
        description = definition.Description;
      } else {
        tooltip.SetDefaultStateData(new BaseDescriptionDef("KILLED_UNIT", "KILLED UNIT", unit.DisplayName,string.Empty).GetTooltipStateData());
      }
      bool sprite_updated = false;
      if (CustomAmmoCategories.Settings.StatisticOnResultScreenRealIcons) {
        if(description != null) {
          if (string.IsNullOrEmpty(description.Icon) == false) {
            if(dm.Exists(BattleTechResourceType.Sprite, description.Icon)) {
              foreground.sprite = dm.GetObjectOfType<Sprite>(description.Icon, BattleTechResourceType.Sprite);
              sprite_updated = true;
            }
          }
        }
      }      
      if (sprite_updated) {
        if (unit.isEjected) { background.color = UIManager.Instance.UIColorRefs.structureDamaged; }
        gameObject.transform.localScale = new Vector3(CustomAmmoCategories.Settings.StatisticOnResultScreenRealIconsScale, CustomAmmoCategories.Settings.StatisticOnResultScreenRealIconsScale,1f);
      } else {
        if (unit.isEjected) { foreground.color = UIManager.Instance.UIColorRefs.structureDamaged; }
        gameObject.transform.localScale = Vector3.one;
      }
    }
    public static void Postfix(AAR_UnitStatusWidget __instance) {
      try {
        Log.Combat?.TWL(0, $"AAR_UnitStatusWidget.InitData GUID:{__instance.UnitData.mech.GUID}");
        if(CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false) {
          Log.Combat?.WL(1, "clearing killed stat");
          __instance.UnitData.statClear();
          return;
        }
        UnitCombatStatistic stat = __instance.UnitData.stat();
        if (stat == null) { return; }
        foreach(var killed in stat.killedUnits) {
          AddKilled(__instance.dm, __instance.KillGridParent, killed);
        }
        Log.Combat?.WL(1,"clearing killed stat");
        __instance.UnitData.statClear();
        Log.Combat?.WL(1, $"now killed units:{__instance.UnitData.stat().killedUnits}");
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("EjectPilot")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AbstractActor_EjectPilot {
    public static void Postfix(AbstractActor __instance) {
      try {
        AbstractActor attacker = Thread.CurrentThread.peekFromStack<AbstractActor>(UnitCombatStatisticHelper.IN_ProcessBatchedTurnDamage_ATTACKER);
        if(attacker != null) {
          Log.Combat?.TWL(0, $"AbstractActor.EjectPilot {__instance.PilotableActorDef.ChassisID} by PanicSystem. Attacker:{attacker.PilotableActorDef.ChassisID}");
          if(__instance.Combat.LocalPlayerTeamGuid == __instance.TeamId) {
            Log.Combat?.WL(1, $"player unit ejected. not count");
            return;
          }
          if (attacker.TeamId == __instance.TeamId) {
            Log.Combat?.WL(1, $"friendly fire. not count");
            return;
          }
          if (UnitCombatStatisticHelper.ejectedUnits.Contains(__instance)) {
            Log.Combat?.WL(1, $"already ejected. not count");
            return;
          }
          UnitCombatStatisticHelper.ejectedUnits.Add(__instance);
          attacker.AddKilled(__instance, true);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("FlagForDeath")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AbstractActor_FlagForDeath {
    public static void AddToStatistic(this AbstractActor __instance, string attackerID) {
      if (UnitCombatStatisticHelper.deadUnits.Contains(__instance)) {
        Log.Combat?.WL(1, "already flagged for death");
        return;
      }
      UnitCombatStatisticHelper.deadUnits.Add(__instance);
      AbstractActor attacker = __instance.Combat.FindActorByGUID(attackerID);
      if (attacker == null) {
        Log.Combat?.WL(1, "can't find attacker");
        return;
      }
      if (attacker == __instance) {
        Log.Combat?.WL(1, "suicide. not count");
        return;
      }
      if (attacker.TeamId == __instance.TeamId) {
        Log.Combat?.WL(1, $"same team attacker:{attacker.TeamId} unit:{__instance.TeamId}");
        return;
      }
      attacker.AddKilled(__instance, false);
    }
    public static void Postfix(AbstractActor __instance, string reason, DeathMethod deathMethod, DamageType damageType, int location, int stackItemID, string attackerID, bool isSilent) {
      try {
        Log.Combat?.TWL(0, $"AbstractActor.FlagForDeath {__instance.PilotableActorDef.ChassisID}. Attacker:{attackerID}",true);
        __instance.AddToStatistic(attackerID);
        AbstractActor_FlagForDeath_Atrillery.Postfix(__instance);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AAR_UnitStatusWidget))]
  [HarmonyPatch("AddKilledMech")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AAR_UnitStatusWidget_AddKilledMech {
    public static bool Prefix(AAR_UnitStatusWidget __instance) {
      try {
        if (CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false) { return true; }
        if (__instance.UnitData.stat() != null) { return false; }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AAR_UnitStatusWidget))]
  [HarmonyPatch("AddKilledVehicle")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AAR_UnitStatusWidget_AddKilledVehicle {
    public static bool Prefix(AAR_UnitStatusWidget __instance) {
      try {
        if (CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false) { return true; }
        if (__instance.UnitData.stat() != null) { return false; }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return true;
    }
  }
  public static class UnitCombatStatisticHelper {
    public static HashSet<AbstractActor> ejectedUnits { get; set; } = new HashSet<AbstractActor>();
    public static HashSet<AbstractActor> deadUnits { get; set; } = new HashSet<AbstractActor>();
    public static void Clear() {
      ejectedUnits.Clear();
      deadUnits.Clear();
    }
    public static readonly string IN_ProcessBatchedTurnDamage_ATTACKER = "IN_ProcessBatchedTurnDamage_ATTACKER";
    public static Assembly PanicSystemAssembly { get; set; } = null;
    public static Type DamageHandler_type { get; set; } = null;
    public static Type TurnDamageTracker_type { get; set; } = null;
    public static Type AARIcons_type { get; set; } = null;
    public static MethodInfo TurnDamageTracker_attackActor_Method { get; set; } = null;
    public static HarmonyMethod DamageHandler_ProcessBatchedTurnDamage_PrefixMethod() {
      return new HarmonyMethod(AccessTools.Method(typeof(UnitCombatStatisticHelper), nameof(DamageHandler_ProcessBatchedTurnDamage_Prefix)));
    }
    public static HarmonyMethod DamageHandler_ProcessBatchedTurnDamage_PostfixMethod() {
      return new HarmonyMethod(AccessTools.Method(typeof(UnitCombatStatisticHelper), nameof(DamageHandler_ProcessBatchedTurnDamage_Postfix)));
    }
    public static void DamageHandler_ProcessBatchedTurnDamage_Prefix(AbstractActor actor, ref bool __state) {
      __state = false;
      try {
        AbstractActor attacker = TurnDamageTracker_attackActor();
        if (attacker != null) {
          Thread.CurrentThread.pushToStack<AbstractActor>(IN_ProcessBatchedTurnDamage_ATTACKER, attacker);
          __state = true;
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.logger.LogException(e);
      }
    }
    public static void DamageHandler_ProcessBatchedTurnDamage_Postfix(AbstractActor actor, ref bool __state) {
      try {
        if(__state == true) {
          Thread.CurrentThread.popFromStack<AbstractActor>(IN_ProcessBatchedTurnDamage_ATTACKER);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
    public static void AddKilled(this AbstractActor attacker, AbstractActor victim, bool ejected) {
      attacker.stat()?.AddKilled(victim, ejected);
    }
    public static bool AARIcons_AddEjectedMech_Prefix() { return CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false; }
    public static bool AARIcons_AddEjectedVehicle_Prefix() { return CustomAmmoCategories.Settings.StatisticOnResultScreenEnabled == false; }
    public static HarmonyMethod AARIcons_AddEjectedMech_PrefixMethod() { return new HarmonyMethod(AccessTools.Method(typeof(UnitCombatStatisticHelper),nameof(AARIcons_AddEjectedMech_Prefix))); }
    public static HarmonyMethod AARIcons_AddEjectedVehicle_PrefixMethod() { return new HarmonyMethod(AccessTools.Method(typeof(UnitCombatStatisticHelper), nameof(AARIcons_AddEjectedVehicle_Prefix))); ; }
    public static void Init() {
      Log.M?.TWL(0,"UnitCombatStatisticHelper.Init");
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("PanicSystem, Version=")) { PanicSystemAssembly = assembly; break; }
      }
      if(PanicSystemAssembly != null) {
        Log.M?.WL(1, "PanicSystem assembly found");
        DamageHandler_type = PanicSystemAssembly.GetType("PanicSystem.Components.DamageHandler");
        TurnDamageTracker_type = PanicSystemAssembly.GetType("PanicSystem.Components.TurnDamageTracker");
        AARIcons_type = PanicSystemAssembly.GetType("PanicSystem.Components.AARIcons");
        if (DamageHandler_type != null) {
          Log.M?.WL(2 , "PanicSystem.Components.DamageHandler found");
          MethodInfo DamageHandler_ProcessBatchedTurnDamage = AccessTools.Method(DamageHandler_type, "ProcessBatchedTurnDamage");
          if(DamageHandler_ProcessBatchedTurnDamage != null) {
            Log.M?.WL(3, "PanicSystem.Components.DamageHandler.ProcessBatchedTurnDamage found");
            CACMain.Core.harmony.Patch(DamageHandler_ProcessBatchedTurnDamage, DamageHandler_ProcessBatchedTurnDamage_PrefixMethod(), DamageHandler_ProcessBatchedTurnDamage_PostfixMethod());
          }
        }
        if(TurnDamageTracker_type != null) {
          Log.M?.WL(2, "PanicSystem.Components.TurnDamageTracker found");
          TurnDamageTracker_attackActor_Method = AccessTools.Method(TurnDamageTracker_type, "attackActor");
          if(TurnDamageTracker_attackActor_Method != null) {
            Log.M?.WL(3, "PanicSystem.Components.TurnDamageTracker.attackActor found");
          }
        }
        if(AARIcons_type != null) {
          Log.M?.WL(2, "PanicSystem.Components.AARIcons found");
          MethodInfo AARIcons_AddEjectedMech = AccessTools.Method(AARIcons_type, "AddEjectedMech");
          if(AARIcons_AddEjectedMech != null) {
            Log.M?.WL(3, "PanicSystem.Components.AARIcons.AddEjectedMech found");
            CACMain.Core.harmony.Patch(AARIcons_AddEjectedMech, AARIcons_AddEjectedMech_PrefixMethod());
          }
          MethodInfo AARIcons_AddEjectedVehicle = AccessTools.Method(AARIcons_type, "AddEjectedVehicle");
          if (AARIcons_AddEjectedVehicle != null) {
            Log.M?.WL(3, "PanicSystem.Components.AARIcons.AddEjectedVehicle found");
            CACMain.Core.harmony.Patch(AARIcons_AddEjectedVehicle, AARIcons_AddEjectedVehicle_PrefixMethod());
          }
        }
      }
    }
    public static AbstractActor TurnDamageTracker_attackActor() {
      if (TurnDamageTracker_attackActor_Method == null) {
        if (TurnDamageTracker_type == null) { return null; }
        TurnDamageTracker_attackActor_Method = AccessTools.Method(TurnDamageTracker_type, "attackActor");
      }
      return (AbstractActor)TurnDamageTracker_attackActor_Method.Invoke(null, new object[] { });
    }
  }
  public class UnitCombatStatistic {
    [JsonIgnore]
    public string DefinitionGUID { get; set; } = string.Empty;
    [JsonIgnore]
    public HashSet<int> attacksIds { get; set; } = new HashSet<int>();
    public float overallCombatDamage { get; set; } = 0f;
    public int attacksCount { get; set; } = 0;
    public float shootsCount { get; set; } = 0f;
    public float successHitsCount { get; set; } = 0f;
    public float criticalHitsCount { get; set; } = 0f;
    public float criticalSuccessCount { get; set; } = 0f;
    public float incomingShootsCount { get; set; } = 0f;
    public float incomingHitsCount { get; set; } = 0f;
    public float incomingCriticalsCount { get; set; } = 0f;
    public float incomingCritSuccessCount { get; set; } = 0f;
    public void MergeStatistic(UnitCombatStatistic add) {
      this.overallCombatDamage += add.overallCombatDamage;
      this.attacksCount += add.attacksCount;
      this.shootsCount += add.shootsCount;
      this.successHitsCount += add.successHitsCount;
      this.criticalHitsCount += add.criticalHitsCount;
      this.criticalSuccessCount += add.criticalSuccessCount;
      this.incomingShootsCount += add.incomingShootsCount;
      this.incomingHitsCount += add.incomingHitsCount;
      this.incomingCriticalsCount += add.incomingCriticalsCount;
      this.incomingCritSuccessCount += add.incomingCritSuccessCount;
    }
    public UnitCombatStatistic Copy() {
      var result = new UnitCombatStatistic();
      result.overallCombatDamage = this.overallCombatDamage;
      result.attacksCount = this.attacksCount;
      result.shootsCount = this.shootsCount;
      result.successHitsCount = this.successHitsCount;
      result.criticalHitsCount = this.criticalHitsCount;
      result.criticalSuccessCount = this.criticalSuccessCount;
      result.incomingShootsCount = this.incomingShootsCount;
      result.incomingHitsCount = this.incomingHitsCount;
      result.incomingCriticalsCount = this.incomingCriticalsCount;
      result.incomingCritSuccessCount = this.incomingCritSuccessCount;
      result.killedUnits.AddRange(this.killedUnits);
      return result;
    }
    public enum KilledUnitIconType { Mech, Vehicle, Turret, Squad };
    public class KilledUnit {
      public string DisplayName { get; set; } = "Unknown";
      public KilledUnitIconType Icon { get; set; } = KilledUnitIconType.Mech;
      public string MechDefId { get; set; } = string.Empty;
      public string VehicleDefId { get; set; } = string.Empty;
      public string TurretDefId { get; set; } = string.Empty;
      public bool isEjected { get; set; } = false;
      public KilledUnit(AbstractActor unit, bool ejected) {
        isEjected = ejected;
        DisplayName = unit.DisplayName;
        ICustomMech custMech = unit as ICustomMech;
        if(custMech != null) {
          if (custMech.isTurret) {
            Icon = KilledUnitIconType.Turret;
          } else
          if (custMech.isSquad) {
            Icon = KilledUnitIconType.Squad;
          } else
          if (custMech.isVehicle) {
            Icon = KilledUnitIconType.Vehicle;
          }
        } else {
          switch (unit.UnitType) {
            case UnitType.Mech: Icon = KilledUnitIconType.Mech; break;
            case UnitType.Vehicle: Icon = KilledUnitIconType.Vehicle; break;
            case UnitType.Turret: Icon = KilledUnitIconType.Turret; break;
            default: Icon = KilledUnitIconType.Vehicle; break;
          }
        }
        if (unit.Combat.DataManager.MechDefs.Exists(unit.PilotableActorDef.Description.Id)) {
          MechDefId = unit.PilotableActorDef.Description.Id;
        } else
        if (unit.Combat.DataManager.VehicleDefs.Exists(unit.PilotableActorDef.Description.Id)) {
          VehicleDefId = unit.PilotableActorDef.Description.Id;
        } else
        if (unit.Combat.DataManager.TurretDefs.Exists(unit.PilotableActorDef.Description.Id)) {
          TurretDefId = unit.PilotableActorDef.Description.Id;
        }
        Log.Combat?.TWL(0,$"add unit to kill list {unit.PilotableActorDef.ChassisID} Icon:{Icon} ejected:{isEjected}");
      }
    }
    public List<KilledUnit> killedUnits { get; set; } = new List<KilledUnit>();
    public void AddKilled(AbstractActor unit, bool ejected) {
      killedUnits.Add(new KilledUnit(unit, ejected));
    }
    public string GenerateDescription() {
      StringBuilder result = new StringBuilder();
      result.AppendLine($"{overallCombatDamage} DAMAGE BY UNIT");
      result.AppendLine($"{attacksCount} ATTACKS BY UNIT");
      result.AppendLine($"{successHitsCount}/{shootsCount}"+(shootsCount > CustomAmmoCategories.Epsilon?$"({(Mathf.Round(successHitsCount * 1000f / shootsCount) / 10f)}%)": "(--.-%)")+ " UNIT'S HITS");
      result.AppendLine($"{criticalSuccessCount}/{criticalHitsCount}" + (criticalHitsCount > CustomAmmoCategories.Epsilon ? $"({(Mathf.Round(criticalSuccessCount * 1000f / criticalHitsCount) / 10f)}%)" : "(--.-%)") + " UNIT'S CRITS");
      result.AppendLine($"{incomingHitsCount}/{incomingShootsCount}" + (incomingShootsCount > CustomAmmoCategories.Epsilon ? $"({(Mathf.Round(incomingHitsCount * 1000f / incomingShootsCount) / 10f)}%)" : "(--.-%)")+ " INCOMING HITS");
      result.AppendLine($"{incomingCritSuccessCount}/{incomingCriticalsCount}" + (incomingCriticalsCount > CustomAmmoCategories.Epsilon ? $"({(Mathf.Round(incomingCritSuccessCount * 1000f / incomingCriticalsCount) / 10f)}%)" : "(--.-%)")+ " INCOMING CRITS");
      return result.ToString();
    }
    public void ProcessShoot(AdvWeaponHitInfoRec hit) {
      shootsCount += 1f;
      if (hit.isHit) {
        successHitsCount += 1f;
        overallCombatDamage += (hit.Damage + hit.APDamage);
      }
    }
    public void ProcessCrit(AdvCritLocationInfo crit) {
      criticalHitsCount += 1f;
      if (crit.component != null) {
        criticalSuccessCount += 1f;
      }
    }
    public void ProcessIncomingShoot(AdvWeaponHitInfoRec hit) {
      incomingShootsCount += 1f;
      if (hit.isHit) {
        incomingHitsCount += 1f;
      }
    }
    public void ProcessIncomingCrit(AdvCritLocationInfo crit) {
      incomingCriticalsCount += 1f;
      if (crit.component != null) {
        incomingCritSuccessCount += 1f;
      }
    }
  }
  public static class CombatStatisticHelper {
    private static Dictionary<string, UnitCombatStatistic> statistic = new Dictionary<string, UnitCombatStatistic>();
    public static void Clear() {
      statistic.Clear();
    }
    public static UnitCombatStatistic stat(this AbstractActor unit) {
      if (string.IsNullOrEmpty(unit.PilotableActorDef.GUID)) { return null; }
      if(statistic.TryGetValue(unit.PilotableActorDef.GUID, out var result) == false) {
        result = new UnitCombatStatistic();
        statistic.Add(unit.PilotableActorDef.GUID, result);
      }
      return result;
    }
    public static UnitCombatStatistic stat(this UnitResult unit) {
      if (string.IsNullOrEmpty(unit.mech.GUID)) { return null; }
      if (statistic.TryGetValue(unit.mech.GUID, out var result) == false) {
        result = new UnitCombatStatistic();
        statistic.Add(unit.mech.GUID, result);
      }
      return result;
    }
    public static void statClear(this UnitResult unit) {
      if (string.IsNullOrEmpty(unit.mech.GUID)) { return; }
      if (statistic.ContainsKey(unit.mech.GUID)) {
        statistic.Remove(unit.mech.GUID);
      }      
    }
    public static void statClear(this AbstractActor unit) {
      if (string.IsNullOrEmpty(unit.PilotableActorDef.GUID)) { return; }
      if (statistic.ContainsKey(unit.PilotableActorDef.GUID)) {
        statistic.Remove(unit.PilotableActorDef.GUID);
      }
    }
    public static void ProcessAttack(AdvWeaponHitInfo advInfo) {
      UnitCombatStatistic attacker = advInfo.weapon.parent.stat();
      if(attacker != null) {
        attacker.attacksIds.Add(advInfo.attackSequenceId);
        attacker.attacksCount = attacker.attacksCount + 1;
      }
      foreach (AdvWeaponHitInfoRec advRec in advInfo.hits) {
        attacker?.ProcessShoot(advRec);
        if(advRec.target is AbstractActor trg) {
          trg.stat()?.ProcessIncomingShoot(advRec);
        }
      }
      foreach (var aCrits in advInfo.resolveInfo) {
        foreach (var advCrit in aCrits.Value.Crits) {
          attacker?.ProcessCrit(advCrit);
          if (aCrits.Key is AbstractActor trg) {
            trg.stat()?.ProcessIncomingCrit(advCrit);
          }
        }
      }
    }
  }
}