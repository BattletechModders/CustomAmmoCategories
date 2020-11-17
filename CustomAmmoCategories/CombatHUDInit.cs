using BattleTech;
using BattleTech.UI;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init {
    private static CombatHUD m_HUD = null;
    private static CombatGameState m_Combat = null;
    public static CombatHUD HUD() { return m_HUD; }
    public static CombatHUD HUD(this CombatHUDInfoSidePanel panel) { return m_HUD; }
    public static CombatGameState Combat(this CombatHUDInfoSidePanel panel) { return m_Combat; }
    public static bool Prefix(CombatHUD __instance, CombatGameState Combat) {
      CustomAmmoCategoriesLog.Log.LogWrite("pre CombatHUD.Init\n");
      //AttackSequenceWatchDogHelper.StartWatchDogThread();
      CustomAmmoCategories.ActorsEjectedAmmo.Clear();
      //CustomAmmoCategories.ClearPlayerWeapons();
      foreach (var unit in Combat.AllActors) {
        CustomAmmoCategoriesLog.Log.LogWrite("  " + unit.DisplayName + "\n");
        foreach (var Weapon in unit.Weapons) {
          CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
        }
      }
      return true;
    }
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      ASWatchdog wd = __instance.gameObject.GetComponent<ASWatchdog>();
      if (wd == null) {
        wd = __instance.gameObject.AddComponent<ASWatchdog>();
      }
      if (wd != null) {
        wd.Init(__instance);
      }
      ExplosionAPIHelper.Init(Combat);
      DynamicMapAsyncProcHelper.Init(__instance);
      DamageModifiersCache.Init(__instance);
      m_HUD = __instance;
      m_Combat = Combat;
      __instance.SidePanelInit();
      ToHitModifiersHelper.InitHUD(__instance);
      try {
        CombatHUDEvasivePipsText.SidePanelInit(__instance);
        PersistentFloatieHelper.Init(__instance);
        DeferredEffectHelper.Clear();
        PersistentFloatieHelper.Clear();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}