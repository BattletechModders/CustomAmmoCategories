using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustAmmoCategories {
  public static class HBSTooltipHelper {
    private static bool tooltipIsPinned = false;
    public static bool IsPinned() { return tooltipIsPinned; }
    public static void PinTooltip(this TooltipManager manager) {
      tooltipIsPinned = true;
      if(manager.activeTooltip != null) {
        CanvasGroup canvasGroup = manager.activeTooltip.gameObject.GetComponent<CanvasGroup>();
        if(canvasGroup != null) {
          canvasGroup.ignoreParentGroups = true;
          canvasGroup.interactable = true;
          canvasGroup.blocksRaycasts = true;
        }
        TooltipScroll scroll = manager.activeTooltip.gameObject.GetComponent<TooltipScroll>();
        if(scroll != null) {
          scroll.pinHelpTxt.SetText("RMB to unpin this");
        }
      }
    }
    public static void UnPinTooltip(this TooltipManager manager) {
      tooltipIsPinned = false;
      if(manager.activeTooltip != null) {
        CanvasGroup canvasGroup = manager.activeTooltip.gameObject.GetComponent<CanvasGroup>();
        if(canvasGroup != null) {
          canvasGroup.ignoreParentGroups = false;
          canvasGroup.interactable = false;
          canvasGroup.blocksRaycasts = false;
        }
        TooltipScroll scroll = manager.activeTooltip.gameObject.GetComponent<TooltipScroll>();
        if(scroll != null) {
          scroll.pinHelpTxt.SetText("RMB to pin this");
        }
      }
    }
  }

  [HarmonyPatch(typeof(TooltipManager))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class TooltipManager_Update {
    public static void Prefix(ref bool __runOriginal, TooltipManager __instance) {
      try {
        if(__runOriginal == false) { return; }
        __runOriginal = false;
        if(!__instance.TooltipManagerActive)
          return;
        if(!__instance.tooltipSpawned) {
          __instance.activeTooltip.ForceRebuild();
          __instance.activeTooltip.ResizeViewport();
        }
        if(HBSTooltipHelper.IsPinned() == false) __instance.activeTooltip.SetPosition(__instance.MousetoCanvas(), __instance.UICamera);
        if(__instance.spawnTimer < __instance.spawnDelay) {
          __instance.spawnTimer += Time.deltaTime;
        } else {
          __instance.activeTooltip.Enable();
          __instance.tooltipSpawned = true;
          if(BTInput.Instance.Mouse_RightButton().WasPressed) {
            if(HBSTooltipHelper.IsPinned() == false) { __instance.PinTooltip(); } else { __instance.ClearTooltip(); }
          }
          if(BTInput.Instance.Key_Escape().WasPressed || BTInput.Instance.Mouse_LeftButton().WasPressed){
            __instance.ClearTooltip();
          }
        }
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(HBSTooltip))]
  [HarmonyPatch("OnPointerExit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class HBSTooltip_OnPointerExit {
    public static void Prefix(ref bool __runOriginal, HBSTooltip __instance, PointerEventData eventData) {
      try {
        if(__runOriginal == false) { return; }
        if(HBSTooltipHelper.IsPinned()) { __runOriginal = false; }
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(HBSTooltip))]
  [HarmonyPatch("OnPointerEnter")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class HBSTooltip_OnPointerEnter {
    public static void Prefix(ref bool __runOriginal, HBSTooltip __instance, PointerEventData eventData) {
      try {
        if(__runOriginal == false) { return; }
        if(HBSTooltipHelper.IsPinned()) { __runOriginal = false; }
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(HBSTooltip))]
  [HarmonyPatch("OnPointerClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class HBSTooltip_OnPointerClick {
    public static void Prefix(ref bool __runOriginal, HBSTooltip __instance, PointerEventData eventData) {
      try {
        if(__runOriginal == false) { return; }
        //if(HBSTooltipHelper.IsPinned()) {
        //  if(eventData.button == PointerEventData.InputButton.Right) {
        //    LazySingletonBehavior<TooltipManager>.Instance.ClearTooltip();
        //  } else {
        //    __runOriginal = false;
        //  }
        //}
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(TooltipManager))]
  [HarmonyPatch("ClearTooltip")]
  [HarmonyPatch(MethodType.Normal)]
  public static class TooltipManager_ClearTooltip {
    public static void Prefix(TooltipManager __instance) {
      try {
        __instance.UnPinTooltip();
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        UIManager.logger.LogException(e);
      }
    }
  }

  public class TooltipScroll :MonoBehaviour {
    public LocalizableText body;
    public GameObject bodyContainer;
    public TooltipPrefab_Weapon parent;
    public LocalizableText pinHelpTxt;
    public ScrollRect scroll;
    public static float MAX_SIZE_TO_SCROLL = 400f;
    public void Hide() {
      bodyContainer?.SetActive(false);
    }
    public void SetText(string value) {
      //bodyContainer?.SetActive(true);
      body?.SetText(value);
    }
    public void Update() {
      body.transform.localPosition = new Vector3(0f, body.transform.localPosition.y, body.transform.localPosition.z);
      if(parent == null) { return; }
      if(body == null) { return; }
      if(bodyContainer == null) { return; }
      if(pinHelpTxt == null) { return; }
      if(scroll == null) { return; }
      if(parent.body.gameObject.activeSelf == false) { return; }
      if(parent.body.rectTransform.sizeDelta.y > MAX_SIZE_TO_SCROLL) {
        bodyContainer.SetActive(true);
        parent.body.gameObject.SetActive(false);
        scroll.verticalNormalizedPosition = 1f;
      }
    }
    public static TooltipScroll Create(TooltipPrefab_Weapon tooltip) {
      var mwdetails = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_SIM_mwDetails-Widget", BattleTechResourceType.UIModulePrefabs);
      TooltipScroll result = null;
      if(mwdetails == null) { return result; }
      SGBarracksServicePanel panel = mwdetails.GetComponentInChildren<SGBarracksServicePanel>(true);
      if(panel == null) { goto exit; }
      ScrollRect scroll = panel.gameObject.FindObject<ScrollRect>("uixPrfPanl_BASE_scrollVertical-Element", false);
      if(scroll == null) { goto exit; }
      result = tooltip.gameObject.AddComponent<TooltipScroll>();
      result.parent = tooltip;
      result.bodyContainer = GameObject.Instantiate(scroll.gameObject);
      result.bodyContainer.transform.SetParent(tooltip.body.transform.parent);
      result.bodyContainer.transform.localScale = Vector3.one;
      result.bodyContainer.transform.localPosition = Vector3.zero;
      result.bodyContainer.transform.SetSiblingIndex(tooltip.body.transform.GetSiblingIndex() + 1);
      HashSet<GameObject> toClear = new HashSet<GameObject>();
      scroll = result.bodyContainer.GetComponent<ScrollRect>();
      while(scroll.content.transform.childCount > 0) { GameObject.DestroyImmediate(scroll.content.transform.GetChild(0).gameObject); }
      result.scroll = scroll;
      result.body = GameObject.Instantiate(tooltip.body.gameObject).GetComponent<LocalizableText>();
      result.body.gameObject.transform.SetParent(scroll.content.transform);
      result.body.gameObject.transform.localScale = Vector3.one;
      result.body.gameObject.transform.localPosition = Vector3.zero;
      result.pinHelpTxt = GameObject.Instantiate(tooltip.quantity.gameObject).GetComponent<LocalizableText>();
      result.pinHelpTxt.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
      result.pinHelpTxt.transform.SetParent(tooltip.quantity.transform.parent);
      result.pinHelpTxt.transform.localPosition = Vector3.zero;
      result.pinHelpTxt.transform.localScale = Vector3.one;
      result.pinHelpTxt.rectTransform.anchoredPosition = new Vector2(10f, 0f);
      result.pinHelpTxt.rectTransform.pivot = new Vector2(0f, 1f);
      result.pinHelpTxt.SetText("RMB to pin this");
      scroll.verticalScrollbar.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5f, 0f);      
      scroll.rectTransform.sizeDelta = new Vector2(tooltip.body.rectTransform.sizeDelta.x, scroll.rectTransform.sizeDelta.y);
exit:
      UIManager.Instance.dataManager.PoolGameObject("uixPrfPanl_SIM_mwDetails-Widget", mwdetails);
      return result;
    }
  }

  [HarmonyPatch(typeof(TooltipPrefab_Weapon))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(object) })]
  public static class TooltipPrefab_Weapon_SetData_scroll {
    public static void Postfix(TooltipPrefab_Weapon __instance) {
      try {
        TooltipScroll scroll = __instance.gameObject.GetComponent<TooltipScroll>();
        //int lines = __instance.body.text.Split('\n').Length;
        Log.M?.TWL(0,$"TooltipPrefab_Weapon.SetData {__instance.weaponName} {__instance.body.rectTransform.sizeDelta.x}x{__instance.body.rectTransform.sizeDelta.y}");
        if(scroll == null) { scroll = TooltipScroll.Create(__instance); }
        __instance.body.gameObject.SetActive(true);
        scroll?.Hide();
        scroll?.SetText(__instance.body.text);
      } catch(Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }

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