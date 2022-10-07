﻿using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using Localize;
using System;
using UnityEngine;

namespace CustAmmoCategories {
  public static class SpawnProtectionHelper {
    public static readonly string SPAWN_PROTECTION_STAT_NAME = "CAC_SPAWN_PROTECTION";
    public static readonly string SPAWN_PROTECTION_ROUND_STAT_NAME = "CAC_SPAWN_PROTECTION_ROUND";
    public static readonly string SPAWN_PROTECTION_STAT_NAME_ADD = "CAC_SPAWN_PROTECTION_ADD";
    public static bool isSpawnProtected(this ICombatant combatant) {
      try {
        return combatant.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).Value<int>() >= combatant.Combat.TurnDirector.CurrentRound;
      } catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return false;
      //return combatant.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).Value<bool>();
    }
    public static bool wasSpawnProtectedPrevRound(this ICombatant combatant) {
      try {
        if (combatant.Combat.TurnDirector.CurrentRound == 1) { return false; }
        return combatant.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).Value<int>() >= (combatant.Combat.TurnDirector.CurrentRound - 1);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return false;
      //return combatant.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).Value<bool>();
    }
    public static int SpawnProtectedRound(this ICombatant combatant) {
      try {
        return combatant.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).Value<int>();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return -1;
    }
    public static void addSpawnProtection(this AbstractActor unit, string reason) {
      Log.M?.TWL(0, $"SpawnProtection add current round {unit.PilotableActorDef.ChassisID} team:{(unit.team == null ? "null" : unit.team.Name)} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase} reason:{reason}");
      try {
        unit.Combat.MessageCenter.PublishMessage(new FloatieMessage(unit.GUID, unit.GUID, new Text($"ADDED SPAWN PROTECTION FOR ROUND {unit.Combat.TurnDirector.CurrentRound}"), FloatieMessage.MessageNature.Buff));
        unit.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).SetValue<int>(unit.Combat.TurnDirector.CurrentRound);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void addSpawnProtection(this AbstractActor unit, int roundOffset, string reason) {
      Log.M?.TWL(0, $"SpawnProtection add till round {unit.Combat.TurnDirector.CurrentRound + roundOffset} {unit.PilotableActorDef.ChassisID} team:{(unit.team == null ? "null" : unit.team.Name)} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase} reason:{reason}");
      try {
        unit.Combat.MessageCenter.PublishMessage(new FloatieMessage(unit.GUID, unit.GUID, new Text($"ADDED SPAWN PROTECTION FOR ROUND {unit.Combat.TurnDirector.CurrentRound + roundOffset}"), FloatieMessage.MessageNature.Buff));
        unit.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).SetValue<int>(unit.Combat.TurnDirector.CurrentRound + roundOffset);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void removeSpawnProtection(this AbstractActor unit) {
      Log.M?.TWL(0, $"SpawnProtection del {unit.PilotableActorDef.ChassisID} team:{(unit.team == null ? "null" : unit.team.Name)} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase}");
      try {
        unit.StatCollection.GetOrCreateStatisic<int>(SPAWN_PROTECTION_ROUND_STAT_NAME, 0).SetValue<int>(0);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      //unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME, false).SetValue<bool>(false);
    }
    public static bool isAddSpawnProtected(this ICombatant combatant) {
      return false;
      //return combatant.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).Value<bool>();
    }
    public static void addAddSpawnProtection(this AbstractActor unit, string reason) {
      //Log.M?.TWL(0, $"SpawnProtection add delayed {unit.PilotableActorDef.ChassisID} round:{unit.Combat.TurnDirector.CurrentRound} phase:{unit.Combat.TurnDirector.CurrentPhase} reason:{reason}");
      //unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).SetValue<bool>(true);
    }
    public static void removeAddSpawnProtection(this AbstractActor unit) {
      //unit.StatCollection.GetOrCreateStatisic<bool>(SPAWN_PROTECTION_STAT_NAME_ADD, false).SetValue<bool>(false);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnActivationEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class AbstractActor_OnActivationEndSpawnProtection {
    public static void Postfix(AbstractActor __instance, string sourceID, int stackItemID) {
      try {
        //if (__instance.isSpawnProtected()) {
        //  __instance.removeSpawnProtection();
        //}
        //if (__instance.isAddSpawnProtected()) {
        //  __instance.addSpawnProtection("delayed");
        //  __instance.removeAddSpawnProtection();
        //}
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(TurnDirector))]
  [HarmonyPatch("FinishBeginRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TurnDirector_FinishBeginRound {
    public static void Postfix(TurnDirector __instance) {
      try {
        Log.M.TWL(0, $"SpawnProtection report. Round:{__instance.CurrentRound}");
        foreach (AbstractActor unit in __instance.Combat.AllActors) {
          if(unit.isSpawnProtected() == false) {
            if (unit.wasSpawnProtectedPrevRound()) {
              unit.Combat.MessageCenter.PublishMessage(new FloatieMessage(unit.GUID, unit.GUID, new Text($"SPAWN PROTECTION REMOVED"), FloatieMessage.MessageNature.Debuff));
            }
          }
          Log.M.WL(1, $"{unit.PilotableActorDef.ChassisID} team:{(unit.team == null ? "null" : unit.team.Name)} is spawn protected:{unit.isSpawnProtected()} was spawn protected at round:{unit.SpawnProtectedRound()}");
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
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