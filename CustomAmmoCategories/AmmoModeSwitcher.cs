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
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesHelper;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPathes;
using HarmonyLib;
using HBS;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomAmmoCategoriesPatches {
  public static class CombatHUDWeaponSlot_OnPointerDown {
    public static void RefreshWeaponCapabilities(this CombatHUDWeaponSlot slot, bool prevIndirectState) {
      try {
        Log.Combat?.TWL(0, "CombatHUDWeaponSlot.RefreshWeaponCapabilities");
        if (prevIndirectState != slot.DisplayedWeapon.IndirectFireCapable()) {
          slot.HUD.FiringPreview.Recalc(slot.DisplayedWeapon.parent, slot.DisplayedWeapon.parent.CurrentIntendedPosition, slot.DisplayedWeapon.parent.CurrentRotation, false, false);
        }
        if (slot.HUD.SelectedTarget != null) {
          LineOfFireLevel lineOfFireLevel = slot.HUD.Combat.LOFCache.GetLineOfFire(slot.HUD.SelectedActor, slot.HUD.SelectedActor.CurrentIntendedPosition, slot.HUD.SelectedTarget, slot.HUD.SelectedTarget.CurrentPosition, slot.HUD.SelectedTarget.CurrentRotation, out Vector3 collision);
          if(lineOfFireLevel <= LineOfFireLevel.LOFBlocked) {
            if(slot.DisplayedWeapon.IndirectFireCapable() == false) {
              slot.DisplayedWeapon.DisableWeapon();
            }
          }
        }
        slot.RefreshDisplayedWeapon(slot.HUD.SelectedTarget);
        if (slot.HUD.SelectionHandler == null) { return; }
        SelectionState selectionState = slot.HUD.SelectionHandler.ActiveState;
        if (selectionState == null) { return; }
        Log.Combat?.WL(1, "selectionState " + selectionState.GetType().ToString());
        if (selectionState.Orders != null) { Log.Combat?.WL(1, "Orders not null"); return; }

        if (selectionState.ShouldShowWeaponsUI() == false) { Log.Combat?.WL(1, "ShouldShowWeaponsUI - false"); return; }
        if (selectionState.ShouldShowTargetingLines()) {
          WeaponRangeIndicators.Instance.UpdateTargetingLines(selectionState.SelectedActor, selectionState.PreviewPos, selectionState.PreviewRot, selectionState.IsPositionLocked, selectionState.TargetedCombatant, selectionState.shouldUseMultiTargetLines(), selectionState.AllTargetedCombatants, selectionState.PotentialMeleeTarget != null);
        } else {
          Log.Combat?.WL(1, "ShouldShowTargetingLines - false");
        }
      } catch (Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshHighlighted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_RefreshHighlighted {
    public static void Prefix(ref bool __runOriginal, CombatHUDWeaponSlot __instance) {
      if (__runOriginal == false) { return; }
      if (__instance.DisplayedWeapon == null) { __runOriginal = false; return; };
      __instance.RefreshAdditionalColors(true);
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshNonHighlighted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_RefreshNonHighlighted {
    public static void Prefix(ref bool __runOriginal, CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { __runOriginal = false; return; };
      __instance.RefreshAdditionalColors(false);
      return;
    }
  }
}