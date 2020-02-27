using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

[HarmonyPatch(typeof(CombatHUDWeaponPanel))]
[HarmonyPatch("Init")]
[HarmonyPatch(MethodType.Normal)]
[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
public static class TurnDirector_Update {
  public static void Postfix(CombatHUDWeaponPanel __instance, CombatGameState Combat, CombatHUD HUD) {
    Log.M.TWL(0, "CombatHUDWeaponPanel.Init "+__instance.gameObject.GetInstanceID());
    Log.M.printComponents(__instance.gameObject,1);
  }
}
