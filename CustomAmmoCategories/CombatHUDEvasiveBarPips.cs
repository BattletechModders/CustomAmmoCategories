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
        List<Graphic> Pips = Traverse.Create(bar).Property<List<Graphic>>("Pips").Value;
        textGO.transform.SetSiblingIndex(Pips[0].transform.GetSiblingIndex());
      }
      text.SetText("0");
      text.fontSize = CustomAmmoCategories.Settings.EvasiveNumberFontSize;
    }
  }
  //[HarmonyPatch(typeof(EncounterLayerData))]
  //[HarmonyPatch("GetEncounterBoundaryTexture")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class EncounterLayerData_GetEncounterBoundaryTexture {
  //  private static bool Prefix(EncounterLayerData __instance, ref Texture2D __result,ref Texture2D ___encounterBoundaryTexture) {
  //    Log.M.TWL(0, "EncounterLayerData.GetEncounterBoundaryTexture");
  //    try {
  //      if ((UnityEngine.Object)___encounterBoundaryTexture == (UnityEngine.Object)null) {
  //        int mapBoundaryWidth = SplatMapInfo.mapBoundaryWidth;
  //        int num1 = 2048 - SplatMapInfo.mapBoundaryWidth;
  //        __instance.CalculateEncounterBoundary();
  //        if ((UnityEngine.Object)__instance.encounterBoundaryChunk != (UnityEngine.Object)null && __instance.encounterBoundaryChunk.encounterBoundaryRectList.Count > 0) {
  //          ___encounterBoundaryTexture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
  //          Color[] pixels = ___encounterBoundaryTexture.GetPixels();
  //          for (int index = 0; index < pixels.Length; ++index)
  //            pixels[index] = Color.black;
  //          ___encounterBoundaryTexture.SetPixels(pixels);
  //          for (int index1 = 0; index1 < __instance.encounterBoundaryChunk.encounterBoundaryRectList.Count; ++index1) {
  //            Rect rect = __instance.encounterBoundaryChunk.encounterBoundaryRectList[index1].rect;
  //            int x = (int)rect.x;
  //            int y = (int)rect.y;
  //            for (int index2 = 0; (double)index2 < (double)rect.height; ++index2) {
  //              for (int index3 = 0; (double)index3 < (double)rect.width; ++index3) {
  //                int num2 = index3 + 1024 + x;
  //                int num3 = index2 + 1024 + y;
  //                if (mapBoundaryWidth < num2 && num2 < num1 && (mapBoundaryWidth < num3 && num3 < num1))
  //                  ___encounterBoundaryTexture.SetPixel(num2 / 4, num3 / 4, Color.white);
  //              }
  //            }
  //          }
  //          ___encounterBoundaryTexture.Apply();
  //          Material mat = new Material(Shader.Find("Hidden/BT-ConvertToSDF"));
  //          mat.color = Color.magenta;
  //          mat.SetColor("_ColorBB", Color.magenta);
  //          RenderTexture temporary = RenderTexture.GetTemporary(___encounterBoundaryTexture.width, ___encounterBoundaryTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
  //          Graphics.Blit((Texture)___encounterBoundaryTexture, temporary, mat, 0);
  //          RenderTexture.active = temporary;
  //          ___encounterBoundaryTexture.ReadPixels(new Rect(0.0f, 0.0f, (float)___encounterBoundaryTexture.width, (float)___encounterBoundaryTexture.height), 0, 0);
  //          ___encounterBoundaryTexture.Apply(true, false);
  //          RenderTexture.active = (RenderTexture)null;
  //        }
  //      }
  //      __result = ___encounterBoundaryTexture;
  //    } catch (Exception e) {
  //      Log.M.TWL(0, e.ToString(), true);
  //      return true;
  //    }
  //    return false;
  //  }
  //}
  [HarmonyPatch(typeof(CombatHUDEvasiveBarPips))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class CombatHUDEvasiveBarPips_Init {
    private static void Postfix(CombatHUDEvasiveBarPips __instance, CombatHUD HUD) {
      Log.M.TWL(0, "CombatHUDEvasiveBarPips.Init Postfix");
      try {
        if (CustomAmmoCategories.Settings.ShowEvasiveAsNumber) {
          CombatHUDEvasivePipsText pipsText = __instance.gameObject.GetComponent<CombatHUDEvasivePipsText>();
          if (pipsText == null) { pipsText = __instance.gameObject.AddComponent<CombatHUDEvasivePipsText>(); pipsText.Init(__instance, HUD); }
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
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