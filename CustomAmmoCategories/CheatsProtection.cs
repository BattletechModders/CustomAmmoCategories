using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Diagnostics;
using UnityEngine;

namespace CustAmmoCategories {
  //[HarmonyPatch(typeof(UnityGameInstance))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class UnityGameInstance_UpdateCheatProtection {
  //  public static readonly float CHECK_PERIOD = 1f;
  //  private static float t = 0f;
  //  public static void Postfix() {
  //    t += Time.deltaTime;
  //    if(t > CHECK_PERIOD) {
  //      t = 0f;
  //      try {
  //        Log.P.TWL(0,"current process modules:");
  //        bool cheatHappens_detected = false;
  //        foreach (ProcessModule module in Process.GetCurrentProcess().Modules) {
  //          Log.P.WL(1, module.FileName);
  //          if (module.FileName.Contains("\\CHM64")) { cheatHappens_detected = true; }
  //        }
  //        if (cheatHappens_detected) {
  //          Log.P.TWL(0, "WARNING!!!CheatHappensTrainer been detected!!!");
  //        }
  //      } catch(Exception e) {
  //        Log.P.TWL(0,e.ToString(),true);
  //      }
  //    }
  //  }
  //}
}