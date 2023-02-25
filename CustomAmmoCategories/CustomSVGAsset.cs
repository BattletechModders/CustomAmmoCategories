using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using SVGImporter;
using System;
using System.IO;
using UnityEngine;

namespace CustAmmoCategories {
  /*[HarmonyPatch(typeof(WeaponCategoryValue))]
  [HarmonyPatch("GetIcon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class WeaponCategoryValue_GetIcon {
    public static SVGAsset testSvg = null;
    public static void Postfix(WeaponCategoryValue __instance, ref SVGAsset __result) {
      Log.M.TWL(0, "WeaponCategoryValue.GetIcon "+__instance.Name);
      if (testSvg == null) {
        string path = Path.Combine(CustomAmmoCategories.Settings.directory, "binary-code.svg");
        if (File.Exists(path)) {
          testSvg = SVGAsset.Load(File.ReadAllText(path));
        } else {
          Log.M.WL(path+" - not exists");
        }
      }
      if (testSvg != null) { __result = testSvg; };
    }
  }*/
}