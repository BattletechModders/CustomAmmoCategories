using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static float getWeaponEvasivePipsIgnored(Weapon weapon) {
      float result = weapon.weaponDef.EvasivePipsIgnored;
      //CustomAmmoCategoriesLog.Log.LogWrite("getWeaponEvasivePipsIgnored " + weapon.UIName + "\n");
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        result += extAmmo.EvasivePipsIgnored;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        result += CustomAmmoCategories.getWeaponMode(weapon).EvasivePipsIgnored;
      }
        //CustomAmmoCategoriesLog.Log.LogWrite("  modified EvasivePipsIgnored\n");
      return result;
    }
  }
}

namespace CustomAmmoCategoriesPatches
{
    [HarmonyPatch(typeof(ToHit))]
    [HarmonyPatch("GetEvasivePipsModifier")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(int), typeof(Weapon) })]
    public static class ToHit_GetEvasivePipsModifier
    {
        public static bool Prefix(ToHit __instance, int evasivePips, Weapon weapon,ref float __result)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("ToHit.GetEvasivePipsModifier");
            try
            {
                float num = 0.0f;
                if (evasivePips > 0)
                {
                    int index = Mathf.RoundToInt((float)((double)evasivePips - 1.0 - (weapon == null ? 0.0 : (double)(CustomAmmoCategories.getWeaponEvasivePipsIgnored(weapon)))));
                    if (index > -1)
                    {
                        CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                        num += combat.Constants.ToHit.ToHitMovingPipUMs[index];
                    }
                }
                __result = num;
                return false;
            }
            catch (Exception e)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default\n");
                return true;
            }
        }
    }
}
