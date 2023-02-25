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
      Log.LogWrite("SimGameState.StartContract postfix\n");
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
    public static bool Prefix(this MetadataDatabase mdd, ref TagSet requiredTags, ref TagSet excludedTags, bool checkOwnership, DateTime? currentDate, TagSet companyTags) {
      Log.LogWrite("TagSetQueryExtensions.GetMatchingUnitDefs prefix\n");
      if (SimGameState_StartContract.CurrentContract != null) {
        Log.LogWrite(" biome:" + SimGameState_StartContract.CurrentContract.ContractBiome + "\n");
        excludedTags.Add("NoBiome_" + SimGameState_StartContract.CurrentContract.ContractBiome);
      } else {
        Log.LogWrite(" biome:" + "null" + "\n");
      }
      //Log.LogWrite(" stack:" + Environment.StackTrace + "\n");
      Log.LogWrite(" requiredTags:\n");
      foreach (string tag in requiredTags) { Log.LogWrite("  " + tag + "\n"); };
      Log.LogWrite(" excludedTags:\n");
      foreach (string tag in excludedTags) { Log.LogWrite("  " + tag + "\n"); };
      return true;
    }
  }
}