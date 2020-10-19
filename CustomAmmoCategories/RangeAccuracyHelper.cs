using BattleTech;
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
