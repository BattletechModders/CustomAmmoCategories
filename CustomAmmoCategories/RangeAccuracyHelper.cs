﻿using BattleTech;
using CustAmmoCategories;
using Harmony;
using System;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static readonly string MinRangeAccModActorStat = "CACMinRangeAccuracyMod"; // 0 < X < MinRange
    public static readonly string ShortRangeAccModActorStat = "CACShortRangeAccuracyMod"; // MinRange < X < ShortRange
    public static readonly string MediumRangeAccModActorStat = "CACMediumRangeAccuracyMod"; // ShortRange < X < MediumRange
    public static readonly string LongRangeAccModActorStat = "CACLongRangeAccuracyMod"; // MediumRange < X < LongRange
    public static readonly string ExtraLongRangeAccModActorStat = "CACExtraLongRangeAccuracyMod"; // LongRange < X < MaxRange
    public static float MinRangeAccMod(this AbstractActor unit) {
      Statistic value = unit.StatCollection.GetStatistic(MinRangeAccModActorStat);
      if (value == null) { return 0f; }
      return value.Value<float>();
      //if (unit.StatCollection.ContainsStatistic(MinRangeAccModActorStat) == false) { return 0f; }
      //return unit.StatCollection.GetStatistic(MinRangeAccModActorStat).Value<float>();
    }
    public static float ShortRangeAccMod(this AbstractActor unit) {
      Statistic value = unit.StatCollection.GetStatistic(ShortRangeAccModActorStat);
      if (value == null) { return 0f; }
      return value.Value<float>();
    }
    public static float MediumRangeAccMod(this AbstractActor unit) {
      Statistic value = unit.StatCollection.GetStatistic(MediumRangeAccModActorStat);
      if (value == null) { return 0f; }
      return value.Value<float>();
    }
    public static float LongRangeRangeAccMod(this AbstractActor unit) {
      Statistic value = unit.StatCollection.GetStatistic(LongRangeAccModActorStat);
      if (value == null) { return 0f; }
      return value.Value<float>();
    }
    public static float ExtraLongRangeAccMod(this AbstractActor unit) {
      Statistic value = unit.StatCollection.GetStatistic(ExtraLongRangeAccModActorStat);
      if (value == null) { return 0f; }
      return value.Value<float>();
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitEffectStats {
    public static void Postfix(AbstractActor __instance) {
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.MinRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.ShortRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.MediumRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.LongRangeAccModActorStat, 0.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.ExtraLongRangeAccModActorStat, 0.0f);
    }
  }
}