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
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPathes;
using HarmonyLib;
using HBS;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("OnPointerDown")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDWeaponSlot_OnPointerDown {
    public static CombatHUD HUD(this CombatHUDWeaponSlot slot) { return Traverse.Create(slot).Field<CombatHUD>("HUD").Value; }
    public static bool ShouldShowWeaponsUI(this SelectionState state) { return Traverse.Create(state).Property<bool>("ShouldShowWeaponsUI").Value; }
    public static bool ShouldShowTargetingLines(this SelectionState state) { return Traverse.Create(state).Property<bool>("ShouldShowTargetingLines").Value; }
    public static bool shouldUseMultiTargetLines(this SelectionState state) { return Traverse.Create(state).Property<bool>("shouldUseMultiTargetLines").Value; }
    public static void RefreshWeaponCapabilities(this CombatHUDWeaponSlot slot, bool prevIndirectState) {
      try {
        Log.M.TWL(0, "CombatHUDWeaponSlot.RefreshWeaponCapabilities");
        if (prevIndirectState != slot.DisplayedWeapon.IndirectFireCapable()) {
          slot.HUD().FiringPreview.Recalc(slot.DisplayedWeapon.parent, slot.DisplayedWeapon.parent.CurrentIntendedPosition, slot.DisplayedWeapon.parent.CurrentRotation, false, false);
        }
        if (slot.HUD().SelectedTarget != null) {
          LineOfFireLevel lineOfFireLevel = slot.HUD().Combat.LOFCache.GetLineOfFire(slot.HUD().SelectedActor, slot.HUD().SelectedActor.CurrentIntendedPosition, slot.HUD().SelectedTarget, slot.HUD().SelectedTarget.CurrentPosition, slot.HUD().SelectedTarget.CurrentRotation, out Vector3 collision);
          if(lineOfFireLevel <= LineOfFireLevel.LOFBlocked) {
            if(slot.DisplayedWeapon.IndirectFireCapable() == false) {
              slot.DisplayedWeapon.DisableWeapon();
            }
          }
        }
        slot.RefreshDisplayedWeapon(slot.HUD().SelectedTarget);
        if (slot.HUD().SelectionHandler == null) { return; }
        SelectionState selectionState = slot.HUD().SelectionHandler.ActiveState;
        if (selectionState == null) { return; }
        Log.M.WL(1, "selectionState " + selectionState.GetType().ToString());
        if (selectionState.Orders != null) { Log.M.WL(1, "Orders not null"); return; }

        if (selectionState.ShouldShowWeaponsUI() == false) { Log.M.WL(1, "ShouldShowWeaponsUI - false"); return; }
        if (selectionState.ShouldShowTargetingLines()) {
          WeaponRangeIndicators.Instance.UpdateTargetingLines(selectionState.SelectedActor, selectionState.PreviewPos, selectionState.PreviewRot, selectionState.IsPositionLocked, selectionState.TargetedCombatant, selectionState.shouldUseMultiTargetLines(), selectionState.AllTargetedCombatants, selectionState.PotentialMeleeTarget != null);
        } else {
          Log.M.WL(1, "ShouldShowTargetingLines - false");
        }
      } catch (Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
    public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUDWeaponSlot.OnPointerDown\n");
      //return false;
      //if (eventData.button != PointerEventData.InputButton.Left) { return true; }
      //if (__instance.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { return true; }
      //if (__instance.DisplayedWeapon == null) { return true; }
      //Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      //if (mainUiCamera == null) {
      //  CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
      //}
      //Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
      //Vector3[] corners = new Vector3[4];
      //__instance.GetComponent<RectTransform>().GetWorldCorners(corners);
      //float width = corners[2].x - corners[0].x;
      //float height = __instance.GetComponent<RectTransform>().rect.height;
      //float clickXrel = worldClickPos.x - __instance.transform.position.x;
      //bool trigger_mode = clickXrel > ((width / 3.0f) * 2.0f);
      //bool trigger_ammo = clickXrel > ((width / 3.0f));
      //bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  modifyers = " + (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  trigger_ammo = " + trigger_ammo + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  trigger_mode = " + trigger_mode + "\n");
      //bool prevIndirectState = __instance.DisplayedWeapon.IndirectFireCapable();
      //if (modifyers) {
      //  ExtWeaponDef extWeapon = __instance.DisplayedWeapon.exDef();
      //  if (extWeapon.EjectWeapon == false) {
      //    CustomAmmoCategories.EjectAmmo(__instance.DisplayedWeapon, __instance);
      //  } else {
      //    CustomAmmoCategories.EjectWeapon(__instance.DisplayedWeapon, __instance);
      //  }
      //  __instance.RefreshWeaponCapabilities(prevIndirectState);
      //  return false;
      //}
      //if (trigger_mode) {
      //  if (CustomAmmoCategories.CycleMode(__instance.DisplayedWeapon)) {
      //    if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
      //      uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, __instance.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //      Log.S.TWL(0, "Playing sound by id:" + num2);
      //    } else {
      //      Log.S.TWL(0, "Can't play");
      //    }
      //  }
      //  __instance.RefreshWeaponCapabilities(prevIndirectState);
      //  return false;
      //} else
      //if (trigger_ammo) {
      //  if (CustomAmmoCategories.CycleAmmo(__instance.DisplayedWeapon)) {
      //    if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
      //      uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, __instance.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //      Log.S.TWL(0, "Playing sound by id:" + num2);
      //    } else {
      //      Log.S.TWL(0, "Can't play");
      //    }
      //  }
      //  __instance.RefreshWeaponCapabilities(prevIndirectState);
      //  return false;
      //};
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("OnPointerUp")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDWeaponSlot_OnPointerUp {
    public static bool Prefix(CombatHUDWeaponSlot __instance, PointerEventData eventData) {
      //return false;
      //Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      //if (mainUiCamera == null) {
      //  CustomAmmoCategoriesLog.Log.LogWrite("  can't get UI camera\n");
      //}
      //Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(eventData.position);
      //Vector3[] corners = new Vector3[4];
      //__instance.GetComponent<RectTransform>().GetWorldCorners(corners);
      //float width = corners[2].x - corners[0].x;
      //float height = __instance.GetComponent<RectTransform>().rect.height;
      //float clickXrel = worldClickPos.x - __instance.transform.position.x;
      //bool trigger = clickXrel > (width / 3.0f);
      //bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.l = " + __instance.transform.position.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.t = " + __instance.transform.position.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.r = " + (__instance.transform.position.x + width) + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  instance.b = " + (__instance.transform.position.y + height) + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  position.x = " + eventData.position.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  position.y = " + eventData.position.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.x = " + worldClickPos.x + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  worldClickPos.y = " + worldClickPos.y + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  clickXrel = " + clickXrel + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  width = " + width + "\n");
      //CustomAmmoCategoriesLog.Log.LogWrite("  trigger = " + trigger + "\n");
      //if (modifyers) {
      //  return false;
      //}
      //if (trigger) {
      //  return false;
      //}
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshHighlighted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_RefreshHighlighted {
    public static bool Prefix(CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { return false; };
      __instance.RefreshAdditionalColors(true);
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshNonHighlighted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_RefreshNonHighlighted {
    public static bool Prefix(CombatHUDWeaponSlot __instance) {
      if (__instance.DisplayedWeapon == null) { return false; };
      __instance.RefreshAdditionalColors(false);
      return true;
    }
  }
}