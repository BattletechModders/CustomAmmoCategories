using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategories {
  public class TooltipPrefab_Weapon_Additional : MonoBehaviour {
    public BaseComponentRef componentRef = null;
  }
  [HarmonyPatch(typeof(MechLabLocationWidget))]
  [HarmonyPatch("OnAddItem")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDraggableItem), typeof(bool) })]
  public static class MechLabLocationWidget_OnAddItem {
    public static void Postfix(MechLabLocationWidget __instance, IMechLabDraggableItem item, bool validate) {
      try {
        var mechLabPanel = __instance.GetComponentInParent<MechLabPanel>();
        foreach (var invitem in mechLabPanel.activeMechInventory) {
          if (invitem != null) { invitem.ClearAmmoModeCache(); }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LocationLoadoutDef) })]
  public static class MechLabLocationWidget_SetData {
    public static void Postfix(MechLabLocationWidget __instance, LocationLoadoutDef loadout, ref List<MechLabItemSlotElement> ___localInventory) {
      try {
        var mechLabPanel = __instance.GetComponentInParent<MechLabPanel>();
        foreach (var invitem in mechLabPanel.activeMechInventory) {
          if (invitem != null) { invitem.ClearAmmoModeCache(); }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabLocationWidget))]
  [HarmonyPatch("OnRemoveItem")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IMechLabDraggableItem), typeof(bool) })]
  public static class MechLabLocationWidget_OnRemoveItem {
    public static void Postfix(MechLabLocationWidget __instance, IMechLabDraggableItem item, bool validate) {
      try {
        var mechLabPanel = __instance.GetComponentInParent<MechLabPanel>();
        foreach (var invitem in mechLabPanel.activeMechInventory) {
          if (invitem != null) { invitem.ClearAmmoModeCache(); }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }

  public static class WeaponRefDataHelper {
    public static float Damage(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.Damage + ammomode.mode.DamagePerShot + ammomode.ammo.DamagePerShot) * ammomode.mode.DamageMultiplier * ammomode.ammo.DamageMultiplier;
        if (result != weaponDef.Damage)
          Log.M?.WL(0, $"WeaponRefDataHelper.Damage {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def value:{weaponDef.Damage} realvalue:{result}");
        return result;
      }
      return 0f;
    }
    public static float HeatDamage(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.HeatDamage + ammomode.mode.HeatDamagePerShot + ammomode.ammo.HeatDamagePerShot) * ammomode.mode.HeatMultiplier * ammomode.ammo.HeatMultiplier;
        if (result != weaponDef.HeatDamage)
          Log.M?.WL(0, $"WeaponRefDataHelper.HeatDamage {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.HeatDamage} real:{result}");
        return result;
      }
      return 0f;
    }
    public static float StructureDamage(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.StructureDamage + ammomode.mode.APDamage + ammomode.ammo.APDamage) * ammomode.mode.APDamageMultiplier * ammomode.ammo.APDamageMultiplier;
        if (result != weaponDef.StructureDamage)
          Log.M?.WL(0, $"WeaponRefDataHelper.StructureDamage {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.StructureDamage} real:{result}");
        return result;
      }
      return 0f;
    }
    public static int ShotsWhenFired(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        int result = Mathf.RoundToInt((weaponDef.ShotsWhenFired + ammomode.mode.ShotsWhenFired + ammomode.ammo.ShotsWhenFired) * ammomode.mode.ShotsWhenFiredMod * ammomode.ammo.ShotsWhenFiredMod);
        if (result != weaponDef.ShotsWhenFired)
          Log.M?.WL(0, $"WeaponRefDataHelper.ShotsWhenFired {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.ShotsWhenFired} real:{result}");
        return result;
      }
      return 0;
    }
    public static int ProjectilesPerShot(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        int result = weaponDef.ProjectilesPerShot + ammomode.mode.ProjectilesPerShot + ammomode.ammo.ProjectilesPerShot;
        if (result != weaponDef.ProjectilesPerShot)
          Log.M?.WL(0, $"WeaponRefDataHelper.ProjectilesPerShot {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.ProjectilesPerShot} real:{result}");
        return result;
      }
      return 0;
    }
    public static int HeatGenerated(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        int result = Mathf.RoundToInt((weaponDef.HeatGenerated + ammomode.mode.HeatGenerated + ammomode.ammo.HeatGenerated) * ammomode.mode.HeatGeneratedModifier * ammomode.ammo.HeatGeneratedModifier);
        if (result != weaponDef.HeatGenerated)
          Log.M?.WL(0, $"WeaponRefDataHelper.HeatGenerated {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.HeatGenerated} real:{result}");
        return result;
      }
      return 0;
    }
    public static float Instability(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.Instability + ammomode.mode.Instability + ammomode.ammo.Instability) * ammomode.mode.InstabilityMultiplier * ammomode.ammo.InstabilityMultiplier;
        if (result != weaponDef.Instability)
          Log.M?.WL(0, $"WeaponRefDataHelper.Instability {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.Instability} real:{result}");
        return result;
      }
      return 0f;
    }
    public static float AccuracyModifier(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.AccuracyModifier + ammomode.mode.AccuracyModifier + ammomode.ammo.AccuracyModifier);
        if (result != weaponDef.AccuracyModifier)
          Log.M?.WL(0, $"WeaponRefDataHelper.AccuracyModifier {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.AccuracyModifier} real:{result}");
        return result;
      }
      return 0f;
    }
    public static float MinRange(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.MinRange + ammomode.mode.MinRange + ammomode.ammo.MinRange);
        if (result != weaponDef.MinRange)
          Log.M?.WL(0, $"WeaponRefDataHelper.MinRange {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.MinRange} real:{result}");
        return result;
      }
      return 0f;
    }
    public static float MediumRange(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null?new MechComponentRef[0]:mechDef.Inventory);
        float result = (weaponDef.MediumRange + ammomode.mode.MediumRange + ammomode.ammo.MediumRange);
        if (result != weaponDef.MediumRange)
          Log.M?.WL(0, $"WeaponRefDataHelper.MediumRange {weaponRef.ComponentDefID} chassis:{(mechDef == null? "null" : mechDef.ChassisID)} def:{weaponDef.MediumRange} real:{result}");
        return result;
      }
      return 0f;
    }
    public static float MaxRange(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return 0f; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        float result = (weaponDef.MaxRange + ammomode.mode.MaxRange + ammomode.ammo.MaxRange);
        if (result != weaponDef.MaxRange)
          Log.M?.WL(0, $"WeaponRefDataHelper.MaxRange {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.MaxRange} real:{result}");
        return result;
      }
      return 0f;
    }
    public static bool IndirectFireCapable(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return false; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        if (mechDef == null) { return weaponDef.IndirectFireCapable; }
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        bool result = weaponDef.IndirectFireCapable;
        if (ammomode.mode.IndirectFireCapable != TripleBoolean.NotSet) { result = ammomode.mode.IndirectFireCapable == TripleBoolean.True; }else
        if (ammomode.ammo.IndirectFireCapable != TripleBoolean.NotSet) { result = ammomode.ammo.IndirectFireCapable == TripleBoolean.True; }
        if(result != weaponDef.IndirectFireCapable)
          Log.M?.WL(0, $"WeaponRefDataHelper.IndirectFireCapable {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{weaponDef.IndirectFireCapable} real:{result}");
        return result;
      }
      return false;
    }
    public static bool CanUseInMelee(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return false; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        if (mechDef == null) { return weaponDef.WeaponCategoryValue.CanUseInMelee; }
        //Log.M?.WL(0, $"WeaponRefDataHelper.CanUseInMelee {weaponRef.ComponentDefID} chassis:{mechDef.ChassisID}");
        return weaponDef.WeaponCategoryValue.CanUseInMelee;
      }
      return false;
    }
    public static bool HasShells(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return false; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        ExtWeaponDef exdef = weaponDef.exDef();
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        if (mechDef == null) { return exdef.HasShells == TripleBoolean.True; }
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        bool result = (exdef.HasShells == TripleBoolean.True);
        if (ammomode.mode.HasShells != TripleBoolean.NotSet) { result = ammomode.mode.HasShells == TripleBoolean.True; } else
        if (ammomode.ammo.HasShells != TripleBoolean.NotSet) { result = ammomode.ammo.HasShells == TripleBoolean.True; }
        if (result != (exdef.HasShells == TripleBoolean.True))
          Log.M?.WL(0, $"WeaponRefDataHelper.HasShells {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{exdef.HasShells} real:{result}");
        return result;
      }
      return false;
    }
    public static bool DamageNotDivided(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return false; }
      if (weaponRef.Def is WeaponDef weaponDef) {
        ExtWeaponDef exdef = weaponDef.exDef();
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        if (mechDef == null) { return exdef.DamageNotDivided == TripleBoolean.True; }
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        bool result = (exdef.DamageNotDivided == TripleBoolean.True);
        if (ammomode.mode.DamageNotDivided != TripleBoolean.NotSet) { result = ammomode.mode.DamageNotDivided == TripleBoolean.True; } else
        if (ammomode.ammo.DamageNotDivided != TripleBoolean.NotSet) { result = ammomode.ammo.DamageNotDivided == TripleBoolean.True; }
        if (result != (exdef.DamageNotDivided == TripleBoolean.True))
          Log.M?.WL(0, $"WeaponRefDataHelper.HasShells {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{exdef.DamageNotDivided} real:{result}");
        return result;
      }
      return false;
    }

    public static bool DamagePerPallet(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return false; }
      if (weaponRef.HasShells() == true) { return false; };
      if (weaponRef.Def is WeaponDef weaponDef) {
        ExtWeaponDef exdef = weaponDef.exDef();
        MechDef mechDef = Thread.CurrentThread.peekFromStack<MechDef>("CalculateStat_mechDef");
        if (mechDef == null) { return exdef.BallisticDamagePerPallet == TripleBoolean.True; }
        var ammomode = weaponRef.GetCurrentAmmoMode(mechDef == null ? new MechComponentRef[0] : mechDef.Inventory);
        bool result = (exdef.BallisticDamagePerPallet == TripleBoolean.True);
        if (ammomode.mode.BallisticDamagePerPallet != TripleBoolean.NotSet) { result = ammomode.mode.BallisticDamagePerPallet == TripleBoolean.True; } else
        if (ammomode.ammo.BallisticDamagePerPallet != TripleBoolean.NotSet) { result = ammomode.ammo.BallisticDamagePerPallet == TripleBoolean.True; }
        if (result != (exdef.BallisticDamagePerPallet == TripleBoolean.True))
          Log.M?.WL(0, $"WeaponRefDataHelper.DamagePerPallet {weaponRef.ComponentDefID} chassis:{(mechDef == null ? "null" : mechDef.ChassisID)} def:{exdef.BallisticDamagePerPallet} real:{result}");
        return result;
      }
      return false;
    }

  }

  public class WeaponRefModes {
    public string SimgameUID { get; set; }
    public Dictionary<string, WeaponMode> modes { get; set; } = new Dictionary<string, WeaponMode>();
    public WeaponRefModes(BaseComponentRef weaponRef) {
      this.SimgameUID = weaponRef.SimGameUID;
      ExtWeaponDef def = CustomAmmoCategories.getExtWeaponDef(weaponRef.ComponentDefID);
      if (def == CustomAmmoCategories.DefaultWeapon) {
        throw new Exception($"Weapon definition {weaponRef.ComponentDefID} does not exists");
      }
      foreach(var mode in def.modes) { modes[mode.Id] = mode; }
    }
  }
  internal class AmmoModePairSortable {
    public ExtAmmunitionDef ammo { get; set; } = null;
    public WeaponMode mode { get; set; } = null;
    public float sortFactor { get; set; } = 0f;
    public AmmoModePairSortable(WeaponDef def, ExtWeaponDef exdef, WeaponMode mode, ExtAmmunitionDef ammo) {
      this.mode = mode;
      this.ammo = ammo;
      float shoots = (def.ShotsWhenFired + mode.ShotsWhenFired + ammo.ShotsWhenFired) * mode.ShotsWhenFiredMod * ammo.ShotsWhenFiredMod;
      float damage = (def.Damage + mode.DamagePerShot + ammo.DamagePerShot) * mode.DamageMultiplier * ammo.DamageMultiplier;
      sortFactor = shoots * damage * (mode.isBaseMode?100f:1f);
    }
  }
  public static class WeaponDefModesCollectHelper {
    private static Dictionary<string, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>>> registry = new Dictionary<string, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>>>();
    private static Dictionary<BaseComponentRef, AmmoModePairSortable> currentAmmoMode = new Dictionary<BaseComponentRef, AmmoModePairSortable>();
    public static void ClearAmmoModeCache() {
      currentAmmoMode.Clear();
    }
    private static AmmoModePairSortable GetCurrentAmmoModeNoCache(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory) {
      List<AmmoModePairSortable> ammomodes = new List<AmmoModePairSortable>();
      var modes = weaponRef.WeaponModesDict(inventory);
      HashSet<string> ammoCategories = new HashSet<string>();
      WeaponDef def = weaponRef.Def as WeaponDef;
      ExtWeaponDef exdef = CustomAmmoCategories.getExtWeaponDef(def.Description.Id);
      foreach(var mode in modes) {
        if (mode.Value.AmmoCategory.BaseCategory.Is_NotSet) {
          ammomodes.Add(new AmmoModePairSortable(def, exdef, mode.Value, CustomAmmoCategories.DefaultAmmo));
          continue;
        }
        HashSet<string> ammoIDs = new HashSet<string>();
        foreach (var invitem in inventory) {
          if (invitem.DamageLevel >= ComponentDamageLevel.NonFunctional && invitem.DamageLevel != ComponentDamageLevel.Installing) { continue; }
          if(invitem.Def is AmmunitionBoxDef ammoboxDef) {
            ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(ammoboxDef.AmmoID);
            if(ammo.AmmoCategory.Id == mode.Value.AmmoCategory.Id) {
              ammoIDs.Add(ammo.Id);
            }
          }
        }
        if (ammoIDs.Count == 0) {
          ammomodes.Add(new AmmoModePairSortable(def, exdef, mode.Value, CustomAmmoCategories.DefaultAmmo));
        } else {
          foreach (var ammoId in ammoIDs) {
            ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(ammoId);
            ammomodes.Add(new AmmoModePairSortable(def, exdef, mode.Value, ammo));
          }
        }
      }
      ammomodes.Sort((a, b) => { return b.sortFactor.CompareTo(a.sortFactor); });
      Log.M?.TWL(0, $"GetCurrentAmmoMode:{weaponRef.ComponentDefID} LocalGUID:{weaponRef.LocalGUID} ammomodes:{ammomodes.Count}");
      foreach (var ammomode in ammomodes) {
        Log.M?.WL(1,$"{ammomode.mode.Id}({ammomode.mode.Name}):{ammomode.ammo.Id}");
      }
      return ammomodes[0];
    }
    internal static AmmoModePairSortable GetCurrentAmmoMode(this BaseComponentRef weaponRef, MechComponentRef[] inventory) {
      if (currentAmmoMode.TryGetValue(weaponRef, out var result)) {
        return result;
      }
      result = GetCurrentAmmoModeNoCache(weaponRef, inventory.ToList<BaseComponentRef>());
      currentAmmoMode.Add(weaponRef, result);
      return result;
    }
    public static void ClearAmmoModeCache(this BaseComponentRef weaponRef) {
      if (weaponRef == null) { return; }
      currentAmmoMode.Remove(weaponRef);
    }
    public static void RegisterCallback(string id, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>> callback) {
      registry[id] = callback;
    }
    private static Dictionary<BaseComponentRef, WeaponRefModes> MODES = new Dictionary<BaseComponentRef, WeaponRefModes>();
    public static void InitHelper(Harmony harmony) {
      harmony.Patch(CalculateHeatEfficiencyStat_Method(), CalculateHeatEfficiencyStat_Prefix_H(), CalculateHeatEfficiencyStat_Postfix_H());
      harmony.Patch(CalculateRangeStat_Method(), CalculateRangeStat_Prefix_H(), CalculateRangeStat_Postfix_H());
      harmony.Patch(CalculateFirepowerStat_Method(), CalculateFirepowerStat_Prefix_H(), CalculateFirepowerStat_Postfix_H());
      harmony.Patch(CalculateMeleeStat_Method(), CalculateMeleeStat_Prefix_H(), CalculateMeleeStat_Postfix_H());
      harmony.Patch(StatTooltipData_SetData_Method(), StatTooltipData_SetData_Prefix_H(), StatTooltipData_SetData_Postfix_H());
    }
    internal static MethodInfo CalculateHeatEfficiencyStat_Method() {
      return AccessTools.Method(typeof(MechStatisticsRules), "CalculateHeatEfficiencyStat");
    }
    internal static void CalculateHeatEfficiencyStat_Prefix(MechDef mechDef) {
      if (mechDef == null) { return; }
      Log.M?.TWL(0, $"MechStatisticsRules.CalculateHeatEfficiencyStat prefix {mechDef.ChassisID}");
      Thread.CurrentThread.pushToStack<MechDef>("CalculateStat_mechDef", mechDef);
    }
    internal static void CalculateHeatEfficiencyStat_Postfix(MechDef mechDef) {
      if (mechDef == null) { return; }
      Log.M?.TWL(0, $"MechStatisticsRules.CalculateHeatEfficiencyStat postfix {mechDef.ChassisID}");
      Thread.CurrentThread.popFromStack<MechDef>("CalculateStat_mechDef");
    }
    internal static HarmonyMethod CalculateHeatEfficiencyStat_Prefix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateHeatEfficiencyStat_Prefix)));
      result.priority = 1000;
      return result;
    }
    internal static HarmonyMethod CalculateHeatEfficiencyStat_Postfix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateHeatEfficiencyStat_Postfix)));
      result.priority = -400;
      return result;
    }
    internal static MethodInfo CalculateRangeStat_Method() {
      return AccessTools.Method(typeof(MechStatisticsRules), "CalculateRangeStat");
    }
    internal static void CalculateRangeStat_Prefix(MechDef mechDef) {
      Thread.CurrentThread.pushToStack<MechDef>("CalculateStat_mechDef", mechDef);
    }
    internal static void CalculateRangeStat_Postfix(MechDef mechDef) {
      Thread.CurrentThread.popFromStack<MechDef>("CalculateStat_mechDef");
    }
    internal static HarmonyMethod CalculateRangeStat_Prefix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateRangeStat_Prefix)));
      result.priority = 1000;
      return result;
    }
    internal static HarmonyMethod CalculateRangeStat_Postfix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateRangeStat_Postfix)));
      result.priority = -400;
      return result;
    }
    internal static MethodInfo CalculateFirepowerStat_Method() {
      return AccessTools.Method(typeof(MechStatisticsRules), "CalculateFirepowerStat");
    }
    internal static void CalculateFirepowerStat_Prefix(MechDef mechDef) {
      Thread.CurrentThread.pushToStack<MechDef>("CalculateStat_mechDef", mechDef);
    }
    internal static void CalculateFirepowerStat_Postfix(MechDef mechDef) {
      Thread.CurrentThread.popFromStack<MechDef>("CalculateStat_mechDef");
    }
    internal static HarmonyMethod CalculateFirepowerStat_Prefix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateFirepowerStat_Prefix)));
      result.priority = 1000;
      return result;
    }
    internal static HarmonyMethod CalculateFirepowerStat_Postfix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateFirepowerStat_Postfix)));
      result.priority = -400;
      return result;
    }
    internal static MethodInfo CalculateMeleeStat_Method() {
      return AccessTools.Method(typeof(MechStatisticsRules), "CalculateMeleeStat");
    }
    internal static void CalculateMeleeStat_Prefix(MechDef mechDef) {
      Thread.CurrentThread.pushToStack<MechDef>("CalculateStat_mechDef", mechDef);
    }
    internal static void CalculateMeleeStat_Postfix(MechDef mechDef) {
      Thread.CurrentThread.popFromStack<MechDef>("CalculateStat_mechDef");
    }
    internal static HarmonyMethod CalculateMeleeStat_Prefix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateMeleeStat_Prefix)));
      result.priority = 1000;
      return result;
    }
    internal static HarmonyMethod CalculateMeleeStat_Postfix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(CalculateMeleeStat_Postfix)));
      result.priority = -400;
      return result;
    }
    internal static MethodInfo StatTooltipData_SetData_Method() {
      return AccessTools.Method(typeof(StatTooltipData), "SetData");
    }
    internal static void StatTooltipData_SetData_Prefix(MechDef def) {
      Thread.CurrentThread.pushToStack<MechDef>("CalculateStat_mechDef", def);
    }
    internal static void StatTooltipData_SetData_Postfix(MechDef def) {
      Thread.CurrentThread.popFromStack<MechDef>("CalculateStat_mechDef");
    }
    internal static HarmonyMethod StatTooltipData_SetData_Prefix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(StatTooltipData_SetData_Prefix)));
      result.priority = 1000;
      return result;
    }
    internal static HarmonyMethod StatTooltipData_SetData_Postfix_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(WeaponDefModesCollectHelper), nameof(StatTooltipData_SetData_Postfix)));
      result.priority = -400;
      return result;
    }
    private static Dictionary<string, WeaponMode> GatherModesDirectly(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory) {
      Dictionary<string, WeaponMode> result = new Dictionary<string, WeaponMode>();
      try {
        ExtWeaponDef def = CustomAmmoCategories.getExtWeaponDef(weaponRef.ComponentDefID);
        if (def == CustomAmmoCategories.DefaultWeapon) {
          throw new Exception($"Weapon definition {weaponRef.ComponentDefID} does not exists");
        }
        foreach (var mode in def.Modes) {
          result[mode.Key] = mode.Value;
        }
        foreach (var callback in registry) {
          foreach (var mode in callback.Value(weaponRef, inventory)) {
            if (result.ContainsKey(mode.Id) && mode.isFromJson) {
              result[mode.Id] = result[mode.Id].merge(mode);
            } else {
              result[mode.Id] = mode;
            }
          }
        }
      }catch(Exception e) {
        Log.M?.TWL(0, $"Fail to gather modes for {weaponRef.ComponentDefID}");
        Log.M?.WL(0, e.ToString(), true);
      }
      Log.M?.TWL(0, $"GatherModesDirectly:{weaponRef.ComponentDefID} SimGameUID:{weaponRef.SimGameUID} LocalGUID:{weaponRef.LocalGUID} inventory:{inventory.Count}");
      foreach (var mode in result) {
        Log.M?.WL(1, $"mode:{mode.Value.Id}({mode.Value.Name}) category:{(mode.Value.AmmoCategory == null? "null":mode.Value.AmmoCategory.Id)}");
      };
      return result;
    }
    public static Dictionary<string, WeaponMode> UpdateModes(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory) {
      //if (string.IsNullOrEmpty(weaponRef.SimGameUID)) { return weaponRef.GatherModesDirectly(inventory); }
      if (weaponRef == null) { return new Dictionary<string, WeaponMode>(); }
      if (MODES.TryGetValue(weaponRef, out var modes) == false) {
        modes = new WeaponRefModes(weaponRef);
        MODES.Add(weaponRef, modes);
      }
      modes.modes = weaponRef.GatherModesDirectly(inventory);
      MODES[weaponRef] = modes;
      return modes.modes;
    }
    public static List<WeaponMode> WeaponModes(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory) {
      //if (string.IsNullOrEmpty(weaponRef.SimGameUID)) {
      //  return weaponRef.GatherModesDirectly(inventory).Values.ToList();
      //}
      if (weaponRef == null) { return new List<WeaponMode>(); }
      if (MODES.TryGetValue(weaponRef, out var modes)) {
        return modes.modes.Values.ToList();
      }
      return weaponRef.UpdateModes(inventory).Values.ToList();
    }
    public static Dictionary<string, WeaponMode> WeaponModesDict(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory) {
      //if (string.IsNullOrEmpty(weaponRef.SimGameUID)) {
      //  return weaponRef.GatherModesDirectly(inventory);
      //}
      if (weaponRef == null) { return new Dictionary<string, WeaponMode>(); }
      if (MODES.TryGetValue(weaponRef, out var modes)) {
        return modes.modes;
      }
      return weaponRef.UpdateModes(inventory);
    }
  }
}