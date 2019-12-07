using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static float EvasivePipsIgnored(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(Weapon_InitStats.EvasivePipsIgnoredStatName);
      if(stat != null) {
        return stat.Value<float>() + weapon.ammo().EvasivePipsIgnored + weapon.mode().EvasivePipsIgnored;
      } else {
        return weapon.weaponDef.EvasivePipsIgnored + weapon.ammo().EvasivePipsIgnored + weapon.mode().EvasivePipsIgnored;
      }
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetEvasivePipsModifier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(Weapon) })]
  public static class ToHit_GetEvasivePipsModifier {
    public static bool Prefix(ToHit __instance, int evasivePips, Weapon weapon, ref float __result) {
      //CustomAmmoCategoriesLog.Log.LogWrite("ToHit.GetEvasivePipsModifier");
      try {
        float num = 0.0f;
        CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if (evasivePips > 0) {
          int index = Mathf.RoundToInt((float)((double)evasivePips - 1.0 - (weapon == null ? 0.0 : (double)(weapon.EvasivePipsIgnored()))));
          if (index >= combat.Constants.ToHit.ToHitMovingPipUMs.Length) { index = combat.Constants.ToHit.ToHitMovingPipUMs.Length - 1; };
          if (index >= 0) {
            num += combat.Constants.ToHit.ToHitMovingPipUMs[index];
          }
        }
        __result = num;
        return false;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default\n");
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Weapon_InitStats {
    public static readonly string EvasivePipsIgnoredStatName = "EvasivePipsIgnored";
    public static void Postfix(Weapon __instance) {
      try {
        __instance.StatCollection.AddStatistic<float>(EvasivePipsIgnoredStatName, __instance.weaponDef.EvasivePipsIgnored);
      } catch (Exception e) {
        Log.LogWrite(e.ToString()+"\n",true);
      }
    }
  }
}
