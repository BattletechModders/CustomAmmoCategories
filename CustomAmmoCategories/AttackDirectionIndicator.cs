using BattleTech.UI;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class CombatHUD_DirectionIndicators {
    private static bool LastKeyState = false;
    public static bool Enabled = true;
    public static void Postfix(CombatHUD __instance) {
      bool CurrentKeyState = Input.GetKey(KeyCode.T);
      if (CurrentKeyState == LastKeyState) { return; };
      LastKeyState = CurrentKeyState;
      if (LastKeyState == false) { return; };
      bool Modifyer = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
      if (Modifyer == false) { return; }
      Enabled = !Enabled;
      Log.LogWrite("AttackDirectionIndicator visibility changed:"+Enabled+"\n");
    }
  }
  [HarmonyPatch(typeof(AttackDirectionIndicator))]
  [HarmonyPatch("ShouldShowArcsForVisibility")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackDirectionIndicator_ShouldShowArcsForVisibility {
    public static void Postfix(AttackDirectionIndicator __instance,ref bool __result) {
      if (CombatHUD_DirectionIndicators.Enabled == false) { __result = false; };
    }
  }
  [HarmonyPatch(typeof(AttackDirectionIndicator))]
  [HarmonyPatch("ShouldShowArcsForTarget")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackDirectionIndicator_ShouldShowArcsForTarget {
    public static void Postfix(AttackDirectionIndicator __instance, ref bool __result) {
      if (CombatHUD_DirectionIndicators.Enabled == false) { __result = false; };
    }
  }
}