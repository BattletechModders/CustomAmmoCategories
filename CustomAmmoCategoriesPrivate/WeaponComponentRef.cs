using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(TooltipPrefab_Weapon))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(object) })]
  public static class TooltipPrefab_Weapon_SetData {
    public static void Prefix(TooltipPrefab_Weapon __instance, ref object data, bool __result, ref BaseComponentRef __state) {
      try {
        __state = null;
        TooltipPrefab_Weapon_Additional additional = __instance.gameObject.GetComponent<TooltipPrefab_Weapon_Additional>();
        if (additional == null) { additional = __instance.gameObject.AddComponent<TooltipPrefab_Weapon_Additional>(); }
        additional.componentRef = null;
        Log.M?.TWL(0,$"TooltipPrefab_Weapon.SetData {data.GetType()}");
        if (data is BaseComponentRef compRef) {
          if (compRef.Def is WeaponDef weaponDef) {
            __state = compRef;
            data = weaponDef;
            additional.componentRef = __state;
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(TooltipPrefab_Weapon __instance, bool __result, ref BaseComponentRef __state) {
      if (__result == false) { return; }
      try {
        if (__state == null) { return; }
        int ShotsWhenFired = __state.ShotsWhenFired();
        int ProjectilesPerShot = __state.ProjectilesPerShot();
        float APDamage = __state.StructureDamage();
        float Damage = __state.Damage();
        float HeatDamage = __state.HeatDamage();
        float StabDamage = __state.Instability();
        bool DamagePerPallet = __state.DamagePerPallet();
        if ((DamagePerPallet == true) && (__state.DamageNotDivided() == false)) {
          Damage /= (float)ProjectilesPerShot;
          HeatDamage /= (float)ProjectilesPerShot;
          StabDamage /= (float)ProjectilesPerShot;
          APDamage /= (float)ProjectilesPerShot;
        }
        if(DamagePerPallet == false) {
          ProjectilesPerShot = 1;
        }
        if (ShotsWhenFired > 1) {
          if (ProjectilesPerShot > 1) {
            if (APDamage > 0f) {
              __instance.damage.SetText($"(({Damage}, {APDamage} AP)x{ProjectilesPerShot})x{ShotsWhenFired}");
            } else {
              __instance.damage.SetText($"(({Damage})x{ProjectilesPerShot})x{ShotsWhenFired}");
            }
            __instance.stability.SetText($"(({StabDamage})x{ProjectilesPerShot})x{ShotsWhenFired}");
          } else {
            if (APDamage > 0f) {
              __instance.damage.SetText($"({Damage}, {APDamage} AP)x{ShotsWhenFired}");
            } else {
              __instance.damage.SetText($"({Damage})x{ShotsWhenFired}");
            }
            __instance.stability.SetText($"({StabDamage})x{ShotsWhenFired}");
          }
        } else {
          if (ProjectilesPerShot > 1) {
            if (APDamage > 0f) {
              __instance.damage.SetText($"({Damage}, {APDamage} AP)x{ProjectilesPerShot}");
            } else {
              __instance.damage.SetText($"({Damage})x{ProjectilesPerShot}");
            }
            __instance.stability.SetText($"({StabDamage})x{ProjectilesPerShot}");
          } else {
            if (APDamage > 0f) {
              __instance.damage.SetText($"{Damage}, {APDamage} AP");
            } else {
              __instance.damage.SetText($"{Damage}");
            }
            __instance.stability.SetText($"{StabDamage}");
          }
        }
        if (HeatDamage > 0f) {
          if (ShotsWhenFired > 1) {
            if (ProjectilesPerShot > 1) {
              __instance.heatDamage.SetText($"(({HeatDamage} AP)x{ProjectilesPerShot})x{ShotsWhenFired}");
            } else {
              __instance.damage.SetText($"({HeatDamage} AP)x{ShotsWhenFired}");
            }
          } else {
            if (ProjectilesPerShot > 1) {
              __instance.damage.SetText($"({HeatDamage} AP)x{ProjectilesPerShot}");
            } else {
              __instance.stability.SetText($"{HeatDamage}");
            }
          }
        } else {
          __instance.heatDamage.SetText("");
        }
        __instance.heat.SetText($"{__state.HeatGenerated()}");
        float MinRange = __state.MinRange();
        __instance.rangeMin.SetText("Min {0}m", new object[1] { __state.MinRange() });
        if (MinRange > 0f) {
          __instance.rangeOptimal.SetText("/Optimal {0}m /", new object[1] { __state.MediumRange() });
        } else {
          __instance.rangeOptimal.SetText("Optimal {0}m /", new object[1] { __state.MediumRange() });
        }
        __instance.rangeMax.SetText("Max {0}m", new object[1] { __state.MaxRange() });
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechLabItemSlotElement))]
  [HarmonyPatch("RefreshInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabItemSlotElement_RefreshInfo {
    public static bool Prefix(MechLabItemSlotElement __instance) {
      try {
        __instance.SetIconAndText();
        HBSTooltipStateData defaultStateData = new HBSTooltipStateData();
        MechComponentDef def = __instance.ComponentRef.Def;
        WeaponDef weaponDef = def as WeaponDef;
        if((weaponDef != null) && (__instance.tooltip != null)) {
          Log.M?.TWL(0,$"MechLabItemSlotElement.RefreshInfo {weaponDef.Description.Id}");
          __instance.tooltip.SetDefaultStateData(__instance.ComponentRef.GetTooltipStateData());
        } else
        if (def != null && __instance.tooltip != null) {
          defaultStateData.SetObject(TooltipUtilities.MechComponentDefHandlerForTooltip(def));
          __instance.tooltip.SetDefaultStateData(defaultStateData);
        }
        __instance.RefreshItemColor();
        __instance.RefreshDamageOverlays();
        __instance.SetSpacers();
        return false;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(TooltipManager))]
  [HarmonyPatch("Awake")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TooltipManager_Awake {
    public static void Prefix(TooltipManager __instance, ref List<TooltipManager.TooltipObject> ___TooltipPool) {
      try {
        Log.M?.TWL(0, $"TooltipManager.Awake");
        TooltipManager.TooltipObject WeaponDef_tooltip = null;
        TooltipManager.TooltipObject MechComponentRef_tooltip = null;
        foreach (var tooltip in ___TooltipPool) {
          Log.M?.WL(1, $"{tooltip.dataType}:{tooltip.prefab}");
          if (tooltip.dataType == typeof(WeaponDef).Name) {
            WeaponDef_tooltip = tooltip;
          }
          if (tooltip.dataType == typeof(MechComponentRef).Name) {
            MechComponentRef_tooltip = tooltip;
          }
        }
        if ((MechComponentRef_tooltip == null) && (WeaponDef_tooltip != null)) {
          MechComponentRef_tooltip = new TooltipManager.TooltipObject();
          Traverse.Create(MechComponentRef_tooltip).Field<string>("DataType").Value = typeof(MechComponentRef).Name;
          Traverse.Create(MechComponentRef_tooltip).Field<GameObject>("Prefab").Value = WeaponDef_tooltip.prefab;
          ___TooltipPool.Add(MechComponentRef_tooltip);
          Log.M?.WL(1, $"add {MechComponentRef_tooltip.dataType}:{MechComponentRef_tooltip.prefab}");
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
}