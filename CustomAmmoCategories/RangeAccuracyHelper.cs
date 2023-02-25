/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustAmmoCategories;
using HarmonyLib;
using System;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static readonly string MinRangeAccModActorStat = "CACMinRangeAccuracyMod"; // 0 < X < MinRange
    public static readonly string ShortRangeAccModActorStat = "CACShortRangeAccuracyMod"; // MinRange < X < ShortRange
    public static readonly string MediumRangeAccModActorStat = "CACMediumRangeAccuracyMod"; // ShortRange < X < MediumRange
    public static readonly string LongRangeAccModActorStat = "CACLongRangeAccuracyMod"; // MediumRange < X < LongRange
    public static readonly string ExtraLongRangeAccModActorStat = "CACExtraLongRangeAccuracyMod"; // LongRange < X < MaxRange
    public static readonly string HeatSinkCapacityMultActorStat = "HeatSinkCapacityMult"; 
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
