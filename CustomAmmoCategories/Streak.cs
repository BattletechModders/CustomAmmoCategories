﻿/*  
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
using BattleTech.AttackDirectorHelpers;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MessageCoordinator))]
  [HarmonyPatch("AddExpectedMessages")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo) })]
  public static class MessageCoordinator_Debug {
    public static bool Prefix(MessageCoordinator __instance, WeaponHitInfo weaponHitInfo) {
      Log.M.TWL(0, "MessageCoordinator.AddExpectedMessages grp:"+weaponHitInfo.attackGroupIndex+" wpn:"+weaponHitInfo.attackWeaponIndex+" shots:"+weaponHitInfo.numberOfShots);
      if (weaponHitInfo.numberOfShots == 0) {
        Log.M.WL(1, "Streak all miss detected. No messages expected from weapon.");
        return false;
      }
      return true;
    }
  }
}
