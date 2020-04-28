using BattleTech;
using BattleTech.AttackDirectorHelpers;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MessageCoordinator))]
  [HarmonyPatch("AddExpectedMessages")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo) })]
  public static class MessageCoordinator_Debug {
    public static bool Prefix(MessageCoordinator __instance, WeaponHitInfo weaponHitInfo) {
      Log.M.TWL(0, "MessageCoordinator.AddExpectedMessages grp:"+weaponHitInfo.attackGroupIndex+" wpn:"+weaponHitInfo.attackWeaponIndex+" shots:"+weaponHitInfo.numberOfShots);
      if (weaponHitInfo.numberOfShots == 0) {
        Log.M.WL(1, "Streak all miss detected. No messages expected from weapon.");
        return false;
      }
      return true;
    }
  }
}
