using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CustAmmoCategories;
using Harmony;
using Localize;
using CustomAmmoCategoriesLog;
using BattleTech.UI;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    //public Morozov ;)
    public static bool isWRJammed(Weapon weapon) {
      return (bool)typeof(WeaponRealizer.Core).Assembly.GetType("WeaponRealizer.JammingEnabler").GetMethod("IsJammed", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { (object)weapon });
    }
    public static float DamageFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      float result = baseDamage;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.DamagePerShot;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.DamagePerShot;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.DamageMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.DamageMultiplier;
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float DamageFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      float result = weapon.weaponDef.Damage;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.DamagePerShot;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.DamagePerShot;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.DamageMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.DamageMultiplier;
        }
      }
      result = result * baseDamage / weapon.weaponDef.Damage;
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float HeatFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      float result = baseDamage;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.HeatDamagePerShot;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.HeatDamagePerShot;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.HeatMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.HeatMultiplier;
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float HeatFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      float result = weapon.weaponDef.HeatDamage;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.HeatDamagePerShot;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.HeatDamagePerShot;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.HeatMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.HeatMultiplier;
        }
      }
      result = result * baseDamage / weapon.weaponDef.HeatDamage;
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float InstabilityFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      float result = baseDamage;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.Instability;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.Instability;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.InstabilityMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.InstabilityMultiplier;
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float InstabilityFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      float result = weapon.weaponDef.Instability;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.Instability;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.Instability;
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result *= extAmmoDef.InstabilityMultiplier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result *= mode.InstabilityMultiplier;
        }
      }
      result = result * baseDamage / weapon.weaponDef.Instability;
      result = (float)Math.Round((double)result, 0);
      return result;
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon {
    public static void Postfix(CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { return; }
      UILookAndColorConstants LookAndColorConstants = (UILookAndColorConstants)typeof(CombatHUDWeaponSlot).GetProperty("LookAndColorConstants", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance,new object[0] { });
      __instance.WeaponText.overflowMode = TMPro.TextOverflowModes.Overflow;
      Log.M.TWL(0,"CombatHUDWeaponSlot.RefreshDisplayedWeapon '"+__instance.WeaponText.text+"' overflow:"+__instance.WeaponText.overflowMode+" worldwrap:"+__instance.WeaponText.enableWordWrapping+" autosize:"+__instance.WeaponText.enableAutoSizing);
      if (__instance.DisplayedWeapon.isAMS() == true) {
        __instance.HitChanceText.SetText("AMS");
      };
      if (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == true) {
        __instance.HitChanceText.SetText("JAM");
        return;
      };
      if (CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == true) {
        __instance.HitChanceText.SetText("JAM");
        return;
      };
      if (CustomAmmoCategories.IsCooldown((Weapon)__instance.DisplayedWeapon) > 0) {
        __instance.HitChanceText.SetText(string.Format("CLD -{0}T", CustomAmmoCategories.IsCooldown((Weapon)__instance.DisplayedWeapon)));
        return;
      };
    }
  }
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch("UIName")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechComponent_UIName {
    public static void Postfix(MechComponent __instance, ref Text __result) {
      /*if (__instance is Weapon) { if (CustomAmmoCategories.isWRJammed((Weapon)__instance) == true) { return; }; };
      if (__instance is Weapon) {
        if (CustomAmmoCategories.IsJammed((Weapon)__instance) == true) {
          __result.Append("({0})", new object[1] { (object)"JAM" });
          return;
        };
      };
      if (__instance is Weapon) {
        if (CustomAmmoCategories.IsCooldown((Weapon)__instance) > 0) {
          __result.Append("(CLDWN {0})", new object[1] { (object)(CustomAmmoCategories.IsCooldown((Weapon)__instance)) });
          return;
        };
      };*/
      if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        if (string.IsNullOrEmpty(CurrentAmmoId) == false) {
          if (__instance.parent.Combat.DataManager.AmmoDefs.Exists(CurrentAmmoId) == true) {
            string ammoBoxName = "";
            if (string.IsNullOrEmpty(__instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.UIName)) {
              ammoBoxName = __instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.Name;
            } else {
              ammoBoxName = __instance.parent.Combat.DataManager.AmmoDefs.Get(CurrentAmmoId).Description.UIName;
            }
            __result.Append("({0})", new object[1] { (object)ammoBoxName });
          }
        }
      }
      if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
        string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.Count > 1) {
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            Log.M.WL("mode name: "+mode.UIName);
            __result.Append("({0})", new object[1] { (object)mode.UIName });
          }
        }
      }
    }

    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("DamagePerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.Last)]
    public static class Weapon_DamagePerShot {
      public static void Postfix(Weapon __instance, ref float __result) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
        if (extWeapon.AlternateDamageCalc == false) {
          __result = CustomAmmoCategories.DamageFormulaOne(__instance, extWeapon, __result);
        } else {
          __result = CustomAmmoCategories.DamageFormulaTwo(__instance, extWeapon, __result);
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("Type")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.Last)]
    public static class Weapon_Type {
      public static void Postfix(Weapon __instance, ref WeaponType __result) {
        Log.LogWrite("Weapon type getted\n");
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("HeatDamagePerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_HeatDamagePerShot {
      public static void Postfix(Weapon __instance, ref float __result) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
        if (extWeapon.AlternateHeatDamageCalc == false) {
          __result = CustomAmmoCategories.HeatFormulaOne(__instance, extWeapon, __result);
        } else {
          __result = CustomAmmoCategories.HeatFormulaTwo(__instance, extWeapon, __result);
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ShotsWhenFired")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ShotsWhenFired {
      public static void Postfix(Weapon __instance, ref int __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.ShotsWhenFired;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.ShotsWhenFired;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ProjectilesPerShot")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ProjectilesPerShot {
      public static void Postfix(Weapon __instance, ref int __result) {
        Log.LogWrite(__instance.UIName+ ".ProjectilesPerShot base:"+__result+"\n");
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.ProjectilesPerShot;
          Log.LogWrite(" ammo "+CurrentAmmoId+" real id:"+extAmmoDef.Id+":" + __result + "\n");
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.ProjectilesPerShot;
            Log.LogWrite(" mode:" + __result + "\n");
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("CriticalChanceMultiplier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_CriticalChanceMultiplier {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.CriticalChanceMultiplier;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.CriticalChanceMultiplier;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("WillFire")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_WillFire {
      public static void Postfix(Weapon __instance, ref bool __result) {
        if (__instance.isAMS()) { __result = false; };
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("AccuracyModifier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_AccuracyModifier {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.AccuracyModifier;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.AccuracyModifier;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MinRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MinRange {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.MinRange;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.MinRange;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MaxRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MaxRange {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.MaxRange;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.MaxRange;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("ShortRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_ShortRange {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.ShortRange;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.ShortRange;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("MediumRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_MediumRange {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.MediumRange;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.MediumRange;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("LongRange")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_LongRange {
      public static void Postfix(Weapon __instance, ref float __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.LongRange;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.LongRange;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("HeatGenerated")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_HeatGenerated {
      public static void Postfix(Weapon __instance, ref float __result) {
        CombatGameState combat = (CombatGameState)typeof(MechComponent).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          if (string.IsNullOrEmpty(CurrentAmmoId) == false) {
            AmmunitionDef ammoDef = combat.DataManager.AmmoDefs.Get(CurrentAmmoId);
            __result += (float)((double)(ammoDef.HeatGenerated) * (double)combat.Constants.Heat.GlobalHeatIncreaseMultiplier * (__instance.parent != null ? (double)__instance.parent.StatCollection.GetValue<float>("WeaponHeatMultiplier") : 1.0));
          }
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += (float)((double)(mode.HeatGenerated) * (double)combat.Constants.Heat.GlobalHeatIncreaseMultiplier * (__instance.parent != null ? (double)__instance.parent.StatCollection.GetValue<float>("WeaponHeatMultiplier") : 1.0));
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("RefireModifier")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_RefireModifier {
      public static void Postfix(Weapon __instance, ref int __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          __result += extAmmoDef.RefireModifier;
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result += mode.RefireModifier;
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("IndirectFireCapable")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_IndirectFireCapable {
      public static void Postfix(Weapon __instance, ref bool __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          if (extAmmoDef.IndirectFireCapable != TripleBoolean.NotSet) {
            __result = (extAmmoDef.IndirectFireCapable == TripleBoolean.True);
          }
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result = (mode.IndirectFireCapable == TripleBoolean.True);
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("AOECapable")]
    [HarmonyPatch(MethodType.Getter)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_AOECapable {
      public static void Postfix(Weapon __instance, ref bool __result) {
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string CurrentAmmoId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
          if (extAmmoDef.IndirectFireCapable != TripleBoolean.NotSet) {
            __result = (extAmmoDef.AOECapable == TripleBoolean.True);
          }
        }
        if (CustomAmmoCategories.checkExistance(__instance.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
          string modeId = __instance.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            __result = (mode.AOECapable == TripleBoolean.True);
          }
        }
      }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("Instability")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class Weapon_Instability {
      public static void Postfix(Weapon __instance, ref float __result) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
        if (extWeapon.AlternateInstabilityCalc == false) {
          __result = CustomAmmoCategories.InstabilityFormulaOne(__instance, extWeapon, __result);
        } else {
          __result = CustomAmmoCategories.InstabilityFormulaTwo(__instance, extWeapon, __result);
        }
      }
    }
  }
}
