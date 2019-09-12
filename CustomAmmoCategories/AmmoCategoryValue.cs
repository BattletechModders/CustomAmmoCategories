using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AmmoCategoryEnumeration))]
  [HarmonyPatch("RefreshStaticData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponRepresentation_ResetWeaponEffect {
    public static void Postfix(WeaponRepresentation __instance) {
      Log.M.TWL(0,"AmmoCategoryEnumeration.RefreshStaticData");

    }
  }
}