﻿using BattleTech;
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
using UnityEngine;
using CustomAmmoCategoriesPatches;
using CustomAmmoCategoriesPathes;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    //public Morozov ;)
    public static int HitIndex(this WeaponEffect we) {
#if BT1_8
      return we.hitIndex;
#else
      return (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(we);
#endif
    }
    public static void HitIndex(this WeaponEffect we, int hi) {
#if BT1_8
      we.hitIndex = hi;
#else
      typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(we,hi);
#endif
    }
    public static bool isWRJammed(Weapon weapon) {
      return (bool)typeof(WeaponRealizer.Core).Assembly.GetType("WeaponRealizer.JammingEnabler").GetMethod("IsJammed", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { (object)weapon });
    }
    public static float DamageFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (baseDamage + extAmmoDef.DamagePerShot + mode.DamagePerShot) * extAmmoDef.DamageMultiplier * mode.DamageMultiplier;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0)&&(weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Damage + extAmmoDef.evasivePipsMods.Damage + mode.evasivePipsMods.Damage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float DamageFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (weapon.weaponDef.Damage + extAmmoDef.DamagePerShot + mode.DamagePerShot) * extAmmoDef.DamageMultiplier * mode.DamageMultiplier * baseDamage / weapon.weaponDef.Damage;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Damage + extAmmoDef.evasivePipsMods.Damage + mode.evasivePipsMods.Damage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float HeatFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (baseDamage + extAmmoDef.HeatDamagePerShot + mode.HeatDamagePerShot) * extAmmoDef.HeatMultiplier * mode.HeatMultiplier;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Heat + extAmmoDef.evasivePipsMods.Heat + mode.evasivePipsMods.Heat;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        } else {
        }
      } else {
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float HeatFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (weapon.weaponDef.HeatDamage + extAmmoDef.HeatDamagePerShot + mode.HeatDamagePerShot) * extAmmoDef.HeatMultiplier * mode.HeatMultiplier * baseDamage / weapon.weaponDef.HeatDamage;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Heat + extAmmoDef.evasivePipsMods.Heat + mode.evasivePipsMods.Heat;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float InstabilityFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (baseDamage + extAmmoDef.Instability + mode.Instability) * extAmmoDef.InstabilityMultiplier * mode.InstabilityMultiplier;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Instablility + extAmmoDef.evasivePipsMods.Instablility + mode.evasivePipsMods.Instablility;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float InstabilityFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (weapon.weaponDef.Instability + extAmmoDef.Instability + mode.Instability) * extAmmoDef.InstabilityMultiplier * mode.InstabilityMultiplier * baseDamage / weapon.weaponDef.Instability;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.Instablility + extAmmoDef.evasivePipsMods.Instablility + mode.evasivePipsMods.Instablility;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float APDamageFormulaOne(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (baseDamage + extAmmoDef.APDamage + mode.APDamage) * extAmmoDef.APDamageMultiplier * mode.APDamageMultiplier;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.APDamage + extAmmoDef.evasivePipsMods.APDamage + mode.evasivePipsMods.APDamage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float APDamageFormulaTwo(Weapon weapon, ExtWeaponDef extWeapon, float baseDamage) {
      if (baseDamage < CustomAmmoCategories.Epsilon) { return 0f; }
      ExtAmmunitionDef extAmmoDef = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = (weapon.weaponDef.StructureDamage + extAmmoDef.APDamage + mode.APDamage) * extAmmoDef.APDamageMultiplier * mode.APDamageMultiplier * baseDamage / weapon.weaponDef.StructureDamage;
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.APDamage + extAmmoDef.evasivePipsMods.APDamage + mode.evasivePipsMods.APDamage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      result = (float)Math.Round((double)result, 0);
      return result;
    }
    public static float AOERange(this Weapon weapon) {
      float result = 0f;
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (ammo.AOECapable != TripleBoolean.NotSet) {
        result = ammo.AOERange;
      } else {
        if (extWeapon.AOECapable != TripleBoolean.NotSet) { result = extWeapon.AOERange; }
      }
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0) && (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.AOERange + ammo.evasivePipsMods.AOERange + mode.evasivePipsMods.AOERange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      return result;
    }
    public static float AOEDamage(this Weapon weapon) {
      float result = 0f;
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (ammo.AOECapable != TripleBoolean.NotSet) {
        result = ammo.AOEDamage;
      } else {
        if (extWeapon.AOECapable != TripleBoolean.NotSet) { result = extWeapon.AOEDamage; }
      }
      if (weapon.parent != null) {
        if ((weapon.parent.EvasivePipsCurrent > 0)&& (weapon.parent.HasMovedThisRound)) {
          float evasiveMod = extWeapon.evasivePipsMods.AOEDamage + ammo.evasivePipsMods.AOEDamage + mode.evasivePipsMods.AOEDamage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      return result;
    }
    public static float ShotsPerAmmo(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      float result = ammo.ShotsPerAmmo * extWeapon.ShotsPerAmmo * mode.ShotsPerAmmo;
      return result;
    }
    public static float GetStatisticFloat(this Weapon weapon, string name) {
      return weapon.StatCollection.GetStatistic(CustAmmoCategories.ExtWeaponDef.StatisticAttributePrefix + name).Value<float>();
    }
    public static float GetStatisticMod(this Weapon weapon, string name) {
      return weapon.StatCollection.GetStatistic(CustAmmoCategories.ExtWeaponDef.StatisticAttributePrefix + name+ CustAmmoCategories.ExtWeaponDef.StatisticModifierSuffix).Value<float>();
    }
    public static float MinMissRadius(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      float result = ammo.MinMissRadius + mode.MinMissRadius + weapon.GetStatisticFloat("MinMissRadius"); //extWeapon.MinMissRadius;
      return result*weapon.GetStatisticMod("MinMissRadius");
    }
    public static float MaxMissRadius(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      float result = ammo.MaxMissRadius + mode.MaxMissRadius + weapon.GetStatisticFloat("MaxMissRadius"); //extWeapon.MaxMissRadius;
      return result * weapon.GetStatisticMod("MaxMissRadius");
    }
    public static float AOEHeatDamage(this Weapon weapon) {
      float result = 0f;
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (ammo.AOECapable != TripleBoolean.NotSet) {
        result = ammo.AOEHeatDamage;
      } else {
        if (extWeapon.AOECapable != TripleBoolean.NotSet) { result = extWeapon.AOEHeatDamage; }
      }
      if (weapon.parent != null) {
        if (weapon.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = extWeapon.evasivePipsMods.AOEHeatDamage + ammo.evasivePipsMods.AOEHeatDamage + mode.evasivePipsMods.AOEHeatDamage;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      return result;
    }
    public static float AOEInstability(this Weapon weapon) {
      float result = 0f;
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (ammo.AOECapable != TripleBoolean.NotSet) {
        result = ammo.AOEInstability;
      } else {
        if (extWeapon.AOECapable != TripleBoolean.NotSet) { result = extWeapon.AOEInstability; }
      }
      if (weapon.parent != null) {
        if (weapon.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = extWeapon.evasivePipsMods.AOEInstability + ammo.evasivePipsMods.AOEInstability + mode.evasivePipsMods.AOEInstability;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) result = result * Mathf.Pow((float)weapon.parent.EvasivePipsCurrent, evasiveMod);
        }
      }
      return result;
    }
  }
}


namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("CurrentAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_CurrentAmmo {
    public static bool Prefix(Weapon __instance, ref int __result) {
      //CustomCategoriesLog.LogWrite("Weapon CurrentAmmo " + __instance.Description.Id + "\n");
      Statistic stat = __instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
      if (stat == null) { return true; }
      string CurrentAmmoId = stat.Value<string>();
      __result = __instance.InternalAmmo;
      //CustomCategoriesLog.LogWrite("  internal ammo "+ __result.ToString());
      for (int index = 0; index < __instance.ammoBoxes.Count; ++index) {
        //CustomCategoriesLog.LogWrite("  AmmoBox " + __instance.ammoBoxes[index].Description.Id+" "+ __instance.ammoBoxes[index].CurrentAmmo+"\n");
        if (__instance.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (__instance.ammoBoxes[index].IsFunctional == false) { continue; }
        if (__instance.ammoBoxes[index].ammoDef.Description.Id != CurrentAmmoId) { continue; };
        __result += __instance.ammoBoxes[index].CurrentAmmo;
      }
      //CustomCategoriesLog.LogWrite("  Result " + __result.ToString()+"\n");
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(MethodType.Normal)]
#if BT1_8
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(int?), typeof(bool), typeof(bool) })]
#else
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
#endif
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon {
    public static void Postfix(CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { return; }
      UILookAndColorConstants LookAndColorConstants = (UILookAndColorConstants)typeof(CombatHUDWeaponSlot).GetProperty("LookAndColorConstants", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true).Invoke(__instance, new object[0] { });
      if (CustomAmmoCategories.Settings.patchWeaponSlotsOverflowCultures.Contains(Strings.CurrentCulture)) {
        if (__instance.WeaponText.overflowMode != TMPro.TextOverflowModes.Overflow) {
          __instance.WeaponText.overflowMode = TMPro.TextOverflowModes.Overflow;
        }
        if (__instance.HitChanceText.overflowMode != TMPro.TextOverflowModes.Overflow) {
          __instance.HitChanceText.overflowMode = TMPro.TextOverflowModes.Overflow;
        }
      } else {
        if (__instance.WeaponText.overflowMode != TMPro.TextOverflowModes.Ellipsis) {
          __instance.WeaponText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        }
        if (__instance.HitChanceText.overflowMode != TMPro.TextOverflowModes.Ellipsis) {
          __instance.HitChanceText.overflowMode = TMPro.TextOverflowModes.Ellipsis;
        }
      }
      if (__instance.DisplayedWeapon.isBlocked()) {
        __instance.HitChanceText.SetText("BLK");
        return;
      } else
      if (__instance.DisplayedWeapon.isAMS() == true) {
        __instance.HitChanceText.SetText("AMS");
      } else
      if (CustomAmmoCategories.isWRJammed(__instance.DisplayedWeapon) == true) {
        __instance.HitChanceText.SetText("JAM");
        return;
      } else
      if (CustomAmmoCategories.IsJammed(__instance.DisplayedWeapon) == true) {
        __instance.HitChanceText.SetText("JAM");
        return;
      } else
      if (CustomAmmoCategories.IsCooldown((Weapon)__instance.DisplayedWeapon) > 0) {
        __instance.HitChanceText.SetText(string.Format("CLD -{0}T", CustomAmmoCategories.IsCooldown((Weapon)__instance.DisplayedWeapon)));
      }
      //Log.M.TWL(0, "CombatHUDWeaponSlot.RefreshDisplayedWeapon '" + __instance.WeaponText.text + "' overflow:" + __instance.WeaponText.overflowMode + " worldwrap:" + __instance.WeaponText.enableWordWrapping + " autosize:" + __instance.WeaponText.enableAutoSizing+" hitChance:"+ __instance.HitChanceText.text+ " overflow:" + __instance.HitChanceText.overflowMode);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("IsTargetMarked")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_IsTargetMarked {
    public static bool Prefix(AbstractActor __instance, ref bool __result) {
      try {
        __result = false;
        List <Effect> effects = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance);
        foreach(Effect effect in effects) {
          if (effect == null) { continue; }
          if (effect.EffectData == null) { continue; };
          if (effect.EffectData.Description == null) { continue; }
          if (string.IsNullOrEmpty(effect.EffectData.Description.Id)) { continue; }
          if (effect.EffectData.Description.Id.Contains("StatusEffect-TAG")) { __result = true; break; }
          if (effect.EffectData.Description.Id.Contains("StatusEffect-NARC")) { __result = true; break; }
        }
      } catch(Exception) {
        return true;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch("UIName")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechComponent_UIName {
    public static void Postfix(MechComponent __instance, ref Text __result) {
      return;
      ////Log.M.WL(0, "MechComponent.UIName "+__instance.defId);
      //Weapon weapon = __instance as Weapon;
      //if (weapon == null) { return; }
      //ExtAmmunitionDef ammo = weapon.ammo();
      //string ammoBoxName = string.Empty;
      //if (ammo.AmmoCategory.BaseCategory.Is_NotSet == false) {
      //  Log.M.WL(1, "has ammo: "+ammo.Id+" isWeaponHasAmmoVariants:"+ weapon.isWeaponHasAmmoVariants()+ " ammo.HideIfOnlyVariant:"+ ammo.HideIfOnlyVariant);
      //  if (weapon.isWeaponHasAmmoVariants()||(ammo.HideIfOnlyVariant == false)) {
      //    if (string.IsNullOrEmpty(ammo.UIName) == false) { ammoBoxName = ammo.UIName; } else { ammoBoxName = ammo.Name; };
      //  }
      //}
      //if (string.IsNullOrEmpty(ammoBoxName) == false) __result.Append("({0})", new object[1] { (object)ammoBoxName });
      //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
      //if (extWeapon.Modes.Count > 1) {
      //  WeaponMode mode = weapon.mode();
      //  if (string.IsNullOrEmpty(mode.UIName) == false) __result.Append("({0})", new object[1] { (object)mode.UIName });
      //}
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
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechComponent_InitStatsWeapon {
    public static void Postfix(MechComponent __instance) {
      Weapon weapon = __instance as Weapon;
      if (weapon == null) { return; }
      Log.M.TWL(0, "Weapon.InitStats");
      ExtWeaponDef def = weapon.exDef();
      bool isPlayerMech = false;
      try {
        if (UnityGameInstance.BattleTechGame.Simulation != null) { isPlayerMech = UnityGameInstance.BattleTechGame.Simulation.isPlayerMech(__instance.parent.PilotableActorDef); }
      }catch(Exception e) {

      }
      foreach(var ia in def.InternalAmmo) {
        string statName = Weapon_InternalAmmo.InternalAmmoName + ia.Key;
        int capacity = ia.Value;
        if (isPlayerMech) {
          AmmunitionDef ammo = __instance.parent.Combat.DataManager.AmmoDefs.Get(ia.Key);
          if (ammo == null) { continue; }
          if (ammo.extDef().AutoRefill != AutoRefilType.Automatic) {
            int ammocount = UnityGameInstance.BattleTechGame.Simulation.GetReservedAmmoCount(ia.Key);
            if (ammocount < capacity) { capacity = ammocount; }
            ammocount -= capacity;
            UnityGameInstance.BattleTechGame.Simulation.SetReservedAmmoCount(ia.Key, ammocount);
            Log.M.WL(1, "Updating ammo count for " + ia.Key + " to " + UnityGameInstance.BattleTechGame.Simulation.GetReservedAmmoCount(ia.Key));
          }
        }
        __instance.StatCollection.AddStatistic<int>(statName, capacity);
        Log.M.WL(1,statName+":"+ia.Key);
      }
      PropertyInfo[] props = typeof(ExtWeaponDef).GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach(PropertyInfo prop in props) {
        object[] attrs = prop.GetCustomAttributes(true);
        foreach(object attr in attrs) {
          StatCollectionFloatAttribute fattr = attr as StatCollectionFloatAttribute;
          if (fattr == null) { continue; }
          weapon.StatCollection.AddStatistic<float>(ExtWeaponDef.StatisticAttributePrefix + prop.Name,(float)prop.GetValue(def));
          weapon.StatCollection.AddStatistic<float>(ExtWeaponDef.StatisticAttributePrefix + prop.Name+ExtWeaponDef.StatisticModifierSuffix, 1f);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("InternalAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  [HarmonyPriority(Priority.First)]
  public static class Weapon_InternalAmmo {
    public static readonly string InternalAmmoName = "InternalAmmo_";
    private static Dictionary<Weapon, int> InternalAmmoCache = new Dictionary<Weapon, int>();
    public static void ClearInternalAmmoCache(this Weapon weapon) { InternalAmmoCache.Remove(weapon); }
    public static void Clear(){ InternalAmmoCache.Clear(); }
    public static void DecInternalAmmo(this Weapon weapon, int stackItemUID, int ammoCount) {
      CustomAmmoCategory cat = weapon.CustomAmmoCategory();
      if (cat.BaseCategory.Is_NotSet) {  return; }
      string statName = InternalAmmoName + weapon.ammo().Id;
      weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, statName, StatCollection.StatOperation.Int_Subtract, ammoCount, -1, true);
      weapon.ClearInternalAmmoCache();
    }
    public static void SetInternalAmmo(this Weapon weapon, int stackItemUID, int ammoCount) {
      CustomAmmoCategory cat = weapon.CustomAmmoCategory();
      if (cat.BaseCategory.Is_NotSet) { return; }
      string statName = InternalAmmoName + weapon.ammo().Id;
      weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, statName, StatCollection.StatOperation.Set, ammoCount, -1, true);
      weapon.ClearInternalAmmoCache();
    }
    public static void ZeroInternalAmmo(this Weapon weapon, int stackItemUID) {
      CustomAmmoCategory cat = weapon.CustomAmmoCategory();
      if (cat.BaseCategory.Is_NotSet) { return; }
      string statName = InternalAmmoName + weapon.ammo().Id;
      weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, statName, StatCollection.StatOperation.Set, 0, -1, true);
      weapon.ClearInternalAmmoCache();
    }
    public static bool Prefix(Weapon __instance, ref int __result) {
      if (InternalAmmoCache.TryGetValue(__instance, out var intAmmo)) { __result = intAmmo; return false; };
      CustomAmmoCategory cat = __instance.CustomAmmoCategory();
      if (cat.BaseCategory.Is_NotSet) { __result = 0; return false; }
      string statName = InternalAmmoName + __instance.ammo().Id;
      Statistic stat = __instance.StatCollection.GetStatistic(statName);
      if(stat == null) {
        __result = 0;
        InternalAmmoCache.Add(__instance,__result);
        return false;
      } else {
        __result = stat.Value<int>();
        InternalAmmoCache.Add(__instance, __result);
        return false;
      }
    }
  }
#if BT1_8
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("StructureDamagePerShot")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  [HarmonyPriority(Priority.Last)]
  public static class Weapon_StructureDamagePerShot {
    public static void Postfix(Weapon __instance, ref float __result) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.defId);
      if (extWeapon.AlternateDamageCalc == false) {
        __result = CustomAmmoCategories.APDamageFormulaOne(__instance, extWeapon, __result);
      } else {
        __result = CustomAmmoCategories.APDamageFormulaTwo(__instance, extWeapon, __result);
      }

    }
  }
#endif
  /*[HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("Type")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  [HarmonyPriority(Priority.Last)]
  public static class Weapon_Type {
    public static void Postfix(Weapon __instance, ref WeaponType __result) {
      Log.LogWrite("Weapon type getted\n");
    }
  }*/
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.ShotsWhenFired;
      __result += mode.ShotsWhenFired;
      __result = Mathf.RoundToInt(__result * ammo.ShotsWhenFiredMod * mode.ShotsWhenFiredMod);
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("ProjectilesPerShot")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_ProjectilesPerShot {
    public static void Postfix(Weapon __instance, ref int __result) {
      __result += __instance.ammo().ProjectilesPerShot;
      __result += __instance.mode().ProjectilesPerShot;
    }
  }
#if BT1_8
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("ClusteringModifier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_ClusteringModifier {
    public static void Postfix(Weapon __instance, ref float __result) {
      __result += __instance.ammo().ClusteringModifier;
      __result += __instance.mode().ClusteringModifier;
    }
  }
#endif
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("CriticalChanceMultiplier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_CriticalChanceMultiplier {
    public static void Postfix(Weapon __instance, ref float __result) {
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.CriticalChanceMultiplier;
      __result += mode.CriticalChanceMultiplier;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.CriticalChanceMultiplier + ammo.evasivePipsMods.CriticalChanceMultiplier + mode.evasivePipsMods.CriticalChanceMultiplier;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.AccuracyModifier;
      __result += mode.AccuracyModifier;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.AccuracyModifier + ammo.evasivePipsMods.AccuracyModifier + mode.evasivePipsMods.AccuracyModifier;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.MinRange;
      __result += mode.MinRange;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.MinRange + ammo.evasivePipsMods.MinRange + mode.evasivePipsMods.MinRange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.MaxRange;
      __result += mode.MaxRange;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.MaxRange + ammo.evasivePipsMods.MaxRange + mode.evasivePipsMods.MaxRange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.ShortRange;
      __result += mode.ShortRange;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.ShortRange + ammo.evasivePipsMods.ShortRange + mode.evasivePipsMods.ShortRange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.MediumRange;
      __result += mode.MediumRange;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.MediumRange + ammo.evasivePipsMods.MediumRange + mode.evasivePipsMods.MediumRange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.LongRange;
      __result += mode.LongRange;
      if (__instance.parent != null) {
        if (__instance.parent.EvasivePipsCurrent > 0) {
          float evasiveMod = __instance.exDef().evasivePipsMods.LongRange + ammo.evasivePipsMods.LongRange + mode.evasivePipsMods.LongRange;
          if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
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
      if (__instance.parent == null) { return; }
      if (__instance.weaponDef.HeatGenerated < CustomAmmoCategories.Epsilon) { __result = 0f; return; }
      float heatBaseMod = __instance.StatCollection.GetValue<float>("HeatGenerated") / __instance.weaponDef.HeatGenerated;
      float heatMod = __instance.parent.StatCollection.GetValue<float>("WeaponHeatMultiplier") 
        * __instance.parent.Combat.Constants.Heat.GlobalHeatIncreaseMultiplier
        * heatBaseMod;
      ExtAmmunitionDef ammoDef = __instance.ammo();
      __result += (ammoDef.HeatGenerated * heatMod);
      WeaponMode mode = __instance.mode();
      __result += (mode.HeatGenerated * heatMod);
      if (__instance.parent.EvasivePipsCurrent > 0) {
        float evasiveMod = __instance.exDef().evasivePipsMods.GeneratedHeat + ammoDef.evasivePipsMods.GeneratedHeat + mode.evasivePipsMods.GeneratedHeat;
        if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = __result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod);
      }
      __result *= ammoDef.HeatGeneratedModifier;
      __result *= mode.HeatGeneratedModifier;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("RefireModifier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_RefireModifier {
    public static void Postfix(Weapon __instance, ref int __result) {
      ExtAmmunitionDef ammo = __instance.ammo();
      WeaponMode mode = __instance.mode();
      __result += ammo.RefireModifier;
      __result += mode.RefireModifier;
      if (__instance.parent.EvasivePipsCurrent > 0) {
        float evasiveMod = __instance.exDef().evasivePipsMods.RefireModifier + ammo.evasivePipsMods.RefireModifier + mode.evasivePipsMods.RefireModifier;
        if (Mathf.Abs(evasiveMod) > CustomAmmoCategories.Epsilon) __result = Mathf.RoundToInt((float)__result * Mathf.Pow((float)__instance.parent.EvasivePipsCurrent, evasiveMod));
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("IndirectFireCapable")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_IndirectFireCapable {
    public static void Postfix(Weapon __instance, ref bool __result) {
      WeaponMode mode = __instance.mode();
      if (mode.IndirectFireCapable != TripleBoolean.NotSet) {
        __result = (mode.IndirectFireCapable == TripleBoolean.True);
        return;
      }
      ExtAmmunitionDef extAmmoDef = __instance.ammo();
      if (extAmmoDef.IndirectFireCapable != TripleBoolean.NotSet) {
        __result = (extAmmoDef.IndirectFireCapable == TripleBoolean.True);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("AOECapable")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_AOECapable {
    public static void Postfix(Weapon __instance, ref bool __result) {
      WeaponMode mode = __instance.mode();
      if (mode.AOECapable != TripleBoolean.NotSet) {
        __result = (mode.AOECapable == TripleBoolean.True);
        return;
      }
      ExtAmmunitionDef extAmmoDef = __instance.ammo();
      if (extAmmoDef.AOECapable != TripleBoolean.NotSet) {
        __result = (extAmmoDef.AOECapable == TripleBoolean.True);
        return;
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

