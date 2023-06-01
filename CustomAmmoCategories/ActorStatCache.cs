using BattleTech;
using HarmonyLib;
using System;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(StatCollection))]
  [HarmonyPatch("ModifyStatistic")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(string), typeof(StatCollection.StatOperation), typeof(Variant), typeof(int), typeof(bool) })]
  public static class StatCollection_ShutdownCombatState {
    public static void Postfix(StatCollection __instance, string sourceID, int stackItemUID, string statName, StatCollection.StatOperation op, Variant variant, int modIndex, bool skipLogging) {
      if (statName == UnitUnaffectionsActorStats.CrewLocationActorStat) {
        UnitUnaffectionsActorStats.ResetCrewLocationCache(__instance);
      }
    }
  }
  public static class StatCollection_RemoveHistoryEvent_Crew {
    public static void Postfix(StatCollection __instance, Statistic stat, int eventUID, bool skipLogging) {
      if (stat.name == UnitUnaffectionsActorStats.CrewLocationActorStat) {
        UnitUnaffectionsActorStats.ResetCrewLocationCache(__instance);
      }
    }
  }
}