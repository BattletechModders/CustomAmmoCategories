using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AmmoCategoryEnumeration))]
  [HarmonyPatch("GetAmmoCategoryByName")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AmmoCategoryEnumeration_GetAmmoCategoryByName {
    public static void Postfix(string name, ref AmmoCategoryValue __result) {
      if(__result == null) {
        Log.M.TWL(0, "AmmoCategoryEnumeration.GetAmmoCategoryByName can't find " + name + "\n" + Environment.StackTrace,true);
      }
    }
  }
}