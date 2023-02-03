using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Threading;
using IRBTModUtils;
using System.Collections.Generic;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("ShowCalledShotPopUp")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor) })]
  public static class CombatHUD_ShowCalledShotPopUp {
    public static void Prefix(CombatHUD __instance, AbstractActor attacker, AbstractActor target) {
      Thread.CurrentThread.SetFlag("ShowCalledShotPopUp");
    }
    public static void Postfix(CombatHUD __instance, AbstractActor attacker, AbstractActor target) {
      Thread.CurrentThread.ClearFlag("ShowCalledShotPopUp");
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetAttackDirection")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant) })]
  public static class ToHit_GetAttackDirection {
    private static Dictionary<AbstractActor, AttackDirection> attackDirectionOverride = new Dictionary<AbstractActor, AttackDirection>();
    public static void Clear() { attackDirectionOverride.Clear(); }
    public static void SetAttackDirection(this AbstractActor target, AttackDirection adir) { attackDirectionOverride[target] = adir; }
    public static void Postfix(ToHit __instance, AbstractActor attacker, AbstractActor target, ref AttackDirection __result) {
      if (Thread.CurrentThread.isFlagSet("ShowCalledShotPopUp") == false) { return; }
      if ((CustomAmmoCategories.Settings.PlayerAlwaysCalledShotDirection == AttackDirection.None)&&(attackDirectionOverride.ContainsKey(target) == false)) { return; }
      AttackDirection result = CustomAmmoCategories.Settings.PlayerAlwaysCalledShotDirection;
      if (attackDirectionOverride.TryGetValue(target, out var overrd)) { result = overrd; }
      Log.M?.TWL(0, $"ShowCalledShotPopUp GetAttackDirection was:{__result} become:{result}");
      __result = result;
    }
  }
  [HarmonyPatch(typeof(SelectionStateFire))]
  [HarmonyPatch("NeedsCalledShot")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateFire_NeedsCalledShot {
    public static void Postfix(SelectionStateFire __instance, ref bool __result) {
      if (CustomAmmoCategories.Settings.PlayerAlwaysCalledShot == false) { return; }
      if (__instance.SelectedActor.TeamId != __instance.SelectedActor.Combat.LocalPlayerTeamGuid) { return; }
      __result = true;
    }
  }

}