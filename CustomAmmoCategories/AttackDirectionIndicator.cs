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
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class CombatHUD_DirectionIndicators {
    private static bool LastKeyState = false;
    public static bool Enabled = true;
    public static void Postfix(CombatHUD __instance) {
      bool CurrentKeyState = Input.GetKey(KeyCode.T);
      if (CurrentKeyState == LastKeyState) { return; };
      LastKeyState = CurrentKeyState;
      if (LastKeyState == false) { return; };
      bool Modifyer = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
      if (Modifyer == false) { return; }
      Enabled = !Enabled;
      Log.LogWrite("AttackDirectionIndicator visibility changed:"+Enabled+"\n");
    }
  }
  [HarmonyPatch(typeof(AttackDirectionIndicator))]
  [HarmonyPatch("ShouldShowArcsForVisibility")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackDirectionIndicator_ShouldShowArcsForVisibility {
    public static void Postfix(AttackDirectionIndicator __instance,ref bool __result) {
      if (CombatHUD_DirectionIndicators.Enabled == false) { __result = false; };
    }
  }
  [HarmonyPatch(typeof(AttackDirectionIndicator))]
  [HarmonyPatch("ShouldShowArcsForTarget")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackDirectionIndicator_ShouldShowArcsForTarget {
    public static void Postfix(AttackDirectionIndicator __instance, ref bool __result) {
      if (CombatHUD_DirectionIndicators.Enabled == false) { __result = false; };
    }
  }
}