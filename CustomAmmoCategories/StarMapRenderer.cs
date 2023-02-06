using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(StarmapRenderer))]
  [HarmonyPatch("PopulateMap")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Starmap) })]
  public static class StarmapRenderer_PopulateMap {
    public static bool Prepare() { return false; }
    public static bool Prefix(StarmapRenderer __instance, Starmap map) {
      Log.P?.TWL(0, $"StarmapRenderer.PopulateMap {map.VisisbleSystem.Count}");
      return false;
    }
  }

}