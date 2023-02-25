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
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AmmoCategoryEnumeration))]
  [HarmonyPatch("GetAmmoCategoryByName")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class AmmoCategoryEnumeration_GetAmmoCategoryByName {
    public static void Postfix(string name, ref AmmoCategoryValue __result) {
      if(__result == null) {
        Log.M.TWL(0, "AmmoCategoryEnumeration.GetAmmoCategoryByName can't find " + name + "\n" + Environment.StackTrace,true);
      }
    }
  }
}