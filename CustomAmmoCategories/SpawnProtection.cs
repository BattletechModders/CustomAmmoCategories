using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using UnityEngine;

namespace CustAmmoCategories {
  public static class SpawnProtectionHelper {
    public static readonly string SPAWN_PROTECTION_STAT_NAME = "CAC_SPAWN_PROTECTION";
    public static readonly string SPAWN_PROTECTION_STAT_NAME_ADD = "CAC_SPAWN_PROTECTION_ADD";
    public static bool isSpawnProtected(this ICombatant combatant) {
      return combatant.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).Value<bool>();
    }
    public static void addSpawnProtection(this AbstractActor unit, string reason) {
      Log.M?.TWL(0, $"SpawnProtection add {unit.PilotableActorDef.ChassisID} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase} reason:{reason}");
      unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).SetValue<bool>(true);
    }
    public static void removeSpawnProtection(this AbstractActor unit) {
      Log.M?.TWL(0, $"SpawnProtection del {unit.PilotableActorDef.ChassisID} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase}");
      unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).SetValue<bool>(false);
    }
    public static bool isAddSpawnProtected(this ICombatant combatant) {
      return combatant.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).Value<bool>();
    }
    public static void addAddSpawnProtection(this AbstractActor unit, string reason) {
      Log.M?.TWL(0, $"SpawnProtection add delayed {unit.PilotableActorDef.ChassisID} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase} reason:{reason}");
      unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).SetValue<bool>(true);
    }
    public static void removeAddSpawnProtection(this AbstractActor unit) {
      unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).SetValue<bool>(false);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnActivationEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class AbstractActor_OnActivationEndSpawnProtection {
    public static void Postfix(AbstractActor __instance, string sourceID, int stackItemID) {
      try {
        if (__instance.isSpawnProtected()) {
          __instance.removeSpawnProtection();
        }
        if (__instance.isAddSpawnProtected()) {
          __instance.addSpawnProtection("delayed");
          __instance.removeAddSpawnProtection();
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetToHitChance")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(int), typeof(MeleeAttackType), typeof(bool) })]
  public static class ToHit_GetToHitChance {
    public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, int numTargets, MeleeAttackType meleeAttackType, bool isMoraleAttack, ref float __result) {
      if (__result > CustomAmmoCategories.Epsilon) {
        if (attacker.isSpawnProtected() || target.isSpawnProtected()) {
          __result = 0f;
        }
      }
    }
  }
}