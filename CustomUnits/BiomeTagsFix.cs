/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Data;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS.Collections;
using System;
using System.Reflection;

namespace CustomUnits {
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("StartContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract) })]
  public static class SimGameState_StartContract {
    public static Contract CurrentContract = null;
    public static void Postfix(SimGameState __instance, Contract contract) {
      Log.Combat?.WL(0,"SimGameState.StartContract postfix");
      SimGameState_StartContract.CurrentContract = contract;
      return;
    }
  }
  [HarmonyPatch(typeof(TagSetQueryExtensions))]
  [HarmonyPatch("GetMatchingUnitDefs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MetadataDatabase), typeof(TagSet), typeof(TagSet), typeof(bool), typeof(DateTime?), typeof(TagSet) })]
  public static class TagSetQueryExtensions_GetMatchingUnitDefs {
    public static MethodInfo FOwner;
    public static void Prefix(this MetadataDatabase mdd, ref TagSet requiredTags, ref TagSet excludedTags, bool checkOwnership, DateTime? currentDate, TagSet companyTags) {
      Log.Combat?.WL(0, "TagSetQueryExtensions.GetMatchingUnitDefs prefix");
      if (SimGameState_StartContract.CurrentContract != null) {
        Log.Combat?.WL(1, "biome:" + SimGameState_StartContract.CurrentContract.ContractBiome);
        excludedTags.Add("NoBiome_" + SimGameState_StartContract.CurrentContract.ContractBiome);
      } else {
        Log.Combat?.WL(1, "biome: null");
      }
      //Log.LogWrite(" stack:" + Environment.StackTrace + "\n");
      Log.Combat?.WL(1, "requiredTags:");
      foreach (string tag in requiredTags) { Log.Combat?.WL(2,tag); };
      Log.Combat?.WL(1, "excludedTags:");
      foreach (string tag in excludedTags) { Log.Combat?.WL(2, tag); };
    }
  }
}