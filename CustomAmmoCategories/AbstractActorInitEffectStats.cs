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
      __instance.StatCollection.AddStatistic<float>(DynamicMapHelper.MINEFIELD_TRIGGER_PROBABILITY_STATISTIC_NAME, 1.0f);
      __instance.StatCollection.AddStatistic<float>(AdvancedCriticalProcessor.FLAT_CRIT_CHANCE_STAT_NAME, 1.0f);
      __instance.StatCollection.AddStatistic<float>(AdvancedCriticalProcessor.BASE_CRIT_CHANCE_STAT_NAME, 1.0f);
      __instance.StatCollection.AddStatistic<float>(AdvancedCriticalProcessor.AP_CRIT_CHANCE_STAT_NAME, 1.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.HeatSinkCapacityMultActorStat, 1.0f);
      __instance.StatCollection.AddStatistic<float>(CustomAmmoCategories.Settings.MinefieldIFFStatName, 0.0f);
      __instance.InitExDamageStats();
      CombatHUDMiniMap.InitMinimapStatistic(__instance);
    }
  }
}