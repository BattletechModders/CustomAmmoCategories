using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(GenericPopup))]
  [HarmonyPatch("TextContent")]
  [HarmonyPatch(MethodType.Setter)]
  public static class GenericPopup_TextContent_set {
    public class GenericPopupScroll: MonoBehaviour {
      public LocalizableText contentText;
      public GameObject contentTextContainer;
      public void Hide() {
        contentTextContainer?.SetActive(false);
      }
      public void Show(string value) {
        contentTextContainer?.SetActive(true);
        contentText?.SetText(value);
      }
      public void Update() {
        contentText.transform.localPosition = new Vector3(0f, contentText.transform.localPosition.y, contentText.transform.localPosition.z);
      }
      public static GenericPopupScroll Create(GenericPopup popup) {
        var mwdetails = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_SIM_mwDetails-Widget", BattleTechResourceType.UIModulePrefabs);
        GenericPopupScroll result = null;
        if(mwdetails == null) { return result; }
        SGBarracksServicePanel panel = mwdetails.GetComponentInChildren<SGBarracksServicePanel>(true);
        if(panel == null) { goto exit; }
        ScrollRect scroll = panel.gameObject.FindObject<ScrollRect>("uixPrfPanl_BASE_scrollVertical-Element", false);
        if(scroll == null) { goto exit; }
        result = popup.gameObject.AddComponent<GenericPopupScroll>();
        result.contentTextContainer = GameObject.Instantiate(scroll.gameObject);
        result.contentTextContainer.transform.SetParent(popup._contentText.transform.parent);
        result.contentTextContainer.transform.localScale = Vector3.one;
        result.contentTextContainer.transform.localPosition = Vector3.zero;
        result.contentTextContainer.transform.SetSiblingIndex(popup._contentText.transform.GetSiblingIndex() + 1);
        HashSet<GameObject> toClear = new HashSet<GameObject>();
        scroll = result.contentTextContainer.GetComponent<ScrollRect>();
        while(scroll.content.transform.childCount > 0) { GameObject.DestroyImmediate(scroll.content.transform.GetChild(0).gameObject); }
        result.contentText = GameObject.Instantiate(popup._contentText.gameObject).GetComponent<LocalizableText>();
        result.contentText.gameObject.transform.SetParent(scroll.content.transform);
        result.contentText.gameObject.transform.localScale = Vector3.one;
        result.contentText.gameObject.transform.localPosition = Vector3.zero;
        scroll.verticalScrollbar.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-56f, 0f);
exit:
        UIManager.Instance.dataManager.PoolGameObject("uixPrfPanl_SIM_mwDetails-Widget", mwdetails);
        return result;
      }
    }
    public static readonly int GenericPopup_MAX_LINES_TO_SCROLL = 18;
    public static void Prefix(ref bool __runOriginal, GenericPopup __instance, string value) {
      try {
        if(__runOriginal == false) { return; }
        GenericPopupScroll scroll = __instance.gameObject.GetComponent<GenericPopupScroll>();
        int lines = value.Split('\n').Length;
        if(lines <= GenericPopup_MAX_LINES_TO_SCROLL) { scroll?.Hide(); __instance._contentText.gameObject.SetActive(true); return; }
        if(scroll == null) { scroll = GenericPopupScroll.Create(__instance);  }
        __instance._contentText.gameObject.SetActive(false);
        scroll?.Show(value);
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  //[HarmonyPatch(typeof(MainMenu))]
  //[HarmonyPatch("ReceiveButtonPress")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(string) })]
  //public static class MainMenu_ReceiveButtonPress {
  //  public static void Prefix(MainMenu __instance, string button) {
  //    try {
  //      GenericPopupBuilder.Create("TEST", "string1\nstring2\nstring3\nstring4\nstring5\nstring6\nstring7\nstring8\nstring9\nstringA\nstringB\nstringC\nstringD\nstringE\nstringF\nstringG\nstringH\nstringI\nstringJ\nstringK\nstringL\nstringM\nstringN\nstringO\nstringP\nstringQ\nstringR\nstringS\nstringT\n").Render();
  //    } catch(Exception e) {
  //      UIManager.logger.LogException(e);
  //    }
  //  }
  //}

}