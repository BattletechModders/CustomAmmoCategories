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
using BattleTech.UI.TMProWrapper;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustAmmoCategories {
  public class CombatHUDEvasivePipsText: MonoBehaviour {
    public static HashSet<CombatHUDEvasivePipsText> TextPips = new HashSet<CombatHUDEvasivePipsText>();
    public static void SidePanelInit(CombatHUD HUD) {
      foreach (CombatHUDEvasivePipsText txt in TextPips) {
        txt.text.font = HUD.SidePanel.WarningText.font;
      }
      TextPips.Clear();
    }
    public CombatHUDEvasivePipsText() {
      text = null;
    }
    public LocalizableText text { get; set; }
    //public
    public void Init(CombatHUDEvasiveBarPips bar, CombatHUD HUD) {
      if (this.text == null) {
        GameObject textGO = new GameObject("EvasiveText");
        textGO.transform.SetParent(this.gameObject.transform);
        textGO.SetActive(false);
        text = textGO.AddComponent<LocalizableText>();
        if (HUD.SidePanel != null) {
          if (HUD.SidePanel.WarningText != null) {
            this.text.font = HUD.SidePanel.WarningText.font;
          } else {
            CombatHUDEvasivePipsText.TextPips.Add(this);
          }
        } else {
          CombatHUDEvasivePipsText.TextPips.Add(this);
        }
        this.text.overflowMode = TextOverflowModes.Overflow;
        this.text.enableWordWrapping = false;
        this.text.alignment = TextAlignmentOptions.Center;
        this.text.autoSizeTextContainer = true;
        LayoutElement lEl = textGO.AddComponent<LayoutElement>();
        lEl.minWidth = CustomAmmoCategories.Settings.EvasiveNumberWidth;
        lEl.minHeight = CustomAmmoCategories.Settings.EvasiveNumberHeight;
        lEl.preferredWidth = CustomAmmoCategories.Settings.EvasiveNumberWidth;
        lEl.preferredHeight = CustomAmmoCategories.Settings.EvasiveNumberHeight;
        List<Graphic> Pips = bar.Pips;
        textGO.transform.SetSiblingIndex(Pips[0].transform.GetSiblingIndex());
      }
      text.SetText("0");
      text.fontSize = CustomAmmoCategories.Settings.EvasiveNumberFontSize;
    }
  }
  [HarmonyPatch(typeof(CombatHUDEvasiveBarPips))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class CombatHUDEvasiveBarPips_Init {
    private static void Postfix(CombatHUDEvasiveBarPips __instance, CombatHUD HUD) {
      Log.Combat?.TWL(0, "CombatHUDEvasiveBarPips.Init Postfix");
      try {
        if (CustomAmmoCategories.Settings.ShowEvasiveAsNumber) {
          CombatHUDEvasivePipsText pipsText = __instance.gameObject.GetComponent<CombatHUDEvasivePipsText>();
          if (pipsText == null) { pipsText = __instance.gameObject.AddComponent<CombatHUDEvasivePipsText>(); pipsText.Init(__instance, HUD); }
        }
      }catch(Exception e) {
        Log.Combat.TWL(0, e.ToString(), true);
        CombatHUD.uiLogger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDEvasiveBarPips))]
  [HarmonyPatch("ShowCurrent")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class CombatHUDEvasiveBarPips_ShowCurrent {
    private static void Postfix(CombatHUDEvasiveBarPips __instance) {
      if (CustomAmmoCategories.Settings.ShowEvasiveAsNumber) {
        CombatHUDEvasivePipsText pipsText = __instance.gameObject.GetComponent<CombatHUDEvasivePipsText>();
        if (pipsText == null) { return; }
        if (__instance.Current > 0f) {
          pipsText.text.gameObject.SetActive(true);
          pipsText.text.SetText(__instance.Current.ToString()+" ");
        } else {
          pipsText.text.gameObject.SetActive(false);
        }
      }
    }
  }
}