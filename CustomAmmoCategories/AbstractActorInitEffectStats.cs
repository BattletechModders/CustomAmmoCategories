using BattleTech;
using CustAmmoCategories;
using Harmony;
using System;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitEffectStats {
    public static int mineFieldIFFLevel(this AbstractActor unit) {
      return Mathf.RoundToInt(unit.StatCollection.GetStatistic(CustomAmmoCategories.Settings.MinefieldIFFStatName).Value<float>());
    }
    public static void Postfix(AbstractActor __instance) {
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.MinRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.ShortRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.MediumRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.LongRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.ExtraLongRangeAccModActorStat, 0.0f);
      __instance.FlatJammChance(0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.Settings.MinefieldDetectorStatName, 1.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.Settings.MinefieldIFFStatName, 0.0f);
    }
  }
}