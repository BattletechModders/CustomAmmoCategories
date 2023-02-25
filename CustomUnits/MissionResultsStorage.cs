//using BattleTech;
//using BattleTech.UI;
//using BattleTech.UI.TMProWrapper;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//namespace CustomUnits {
//  //uixPrfPanl_AA_nextButtonPanel
//  //AAR_SalvageScreen
//  //uixPrfPanl_SIM_mechStorage-Widget
//  public class SalavageStorageWidget : MonoBehaviour, IMechLabDropTarget {
//    public MechBayMechStorageWidget storageWidget { get; set; } = null;
//    public GameObject storageWidgetGO { get; set; } = null;
//    public GameObject storageButtonGO { get; set; } = null;
//    public AAR_SalvageScreen salvageScreen { get; set; } = null;
//    public MechLabDropTargetType dropTargetType { get { return MechLabDropTargetType.MechList; } }
//    private MechBayChassisUnitElement dragItem { get; set; } = null;
//    public IMechLabDraggableItem DragItem => (IMechLabDraggableItem) this.dragItem;
//    public IMechLabDropTarget ParentDropTarget => (IMechLabDropTarget) this;
//    public bool Initialized => true;
//    public bool StackQuantities => false;
//    public bool IsSimGame => true;
//    public SimGameState Sim { get { return salvageScreen ? salvageScreen.Sim : null; } }
//    public void ApplicationFocusChange(bool hasFocus) {
//    }
//    public bool OnAddItem(IMechLabDraggableItem item, bool validate) {
//      Log.TWL(0, "SalavageStorageWidget.OnAddItem "+item.GetType().Name);
//      return true;
//    }
//    public void OnButtonClicked(IMechLabDraggableItem item) {
//      Log.TWL(0, "SalavageStorageWidget.OnButtonClicked " + item.GetType().Name);
//    }
//    public void OnButtonDoubleClicked(IMechLabDraggableItem item) {
//      Log.TWL(0, "SalavageStorageWidget.OnButtonDoubleClicked " + item.GetType().Name);
//    }
//    public void OnDrag(PointerEventData eventData) {
//      Log.TWL(0, "SalavageStorageWidget.OnDrag");
//    }
//    public bool OnItemGrab(IMechLabDraggableItem item, PointerEventData eventData) {
//      Log.TWL(0, "SalavageStorageWidget.OnItemGrab " + item.GetType().Name);
//      return false;
//    }
//    public void OnItemHoverEnter(IMechLabDraggableItem item, PointerEventData eventData) {
//      Log.TWL(0, "SalavageStorageWidget.OnItemHoverEnter " + item.GetType().Name);
//    }
//    public void OnItemHoverExit(IMechLabDraggableItem item, PointerEventData eventData) {
//      Log.TWL(0, "SalavageStorageWidget.OnItemHoverExit " + item.GetType().Name);
//    }
//    public void OnMechLabDrop(PointerEventData eventData, MechLabDropTargetType addToType) {
//      Log.TWL(0, "SalavageStorageWidget.OnMechLabDrop " + addToType);
//    }
//    public bool OnRemoveItem(IMechLabDraggableItem item, bool validate) {
//      Log.TWL(0, "SalavageStorageWidget.OnRemoveItem " + item.GetType().Name);
//      return false;
//    }
//    public void OrderItemRepair(IMechLabDraggableItem item) {
//      Log.TWL(0, "SalavageStorageWidget.OrderItemRepair " + item.GetType().Name);
//    }
//    public void SetRaycastBlockerActive(bool isActive) {
//      Log.TWL(0, "SalavageStorageWidget.SetRaycastBlockerActive " + isActive);
//    }
//    public HBSDOTweenButton showHideButton { get; set; } = null;
//    public void OnHideShowButton() {
//      Log.TWL(0, "SalavageStorageWidget.OnHideShowButton");
//      try {
//        if (this.storageWidget != null) {
//          this.storageWidget.gameObject.SetActive(!this.storageWidget.gameObject.activeSelf);
//          return;
//        }
//        showHideButton?.SetState(ButtonState.Disabled,true);
//        storageWidget = this.gameObject.GetComponentInChildren<MechBayMechStorageWidget>();
//        if (storageWidget == null) {
//          storageWidgetGO = this.Sim.DataManager.PooledInstantiate("uixPrfPanl_SIM_mechStorage-Widget", BattleTechResourceType.UIModulePrefabs);
//          if (storageWidgetGO != null) {
//            storageWidgetGO.name = "uixPrfPanl_SIM_mechStorage-Widget-MANAGED";
//            Transform Overall_layout = this.gameObject.FindComponent<Transform>("Overall-layout");
//            if (Overall_layout == null) {
//              Overall_layout = this.gameObject.transform;
//            }
//            storageWidgetGO.transform.SetParent(Overall_layout);
//            storageWidgetGO.transform.localScale = Vector3.one;
//            storageWidgetGO.transform.localPosition = Vector3.zero;
//            storageWidget = storageWidgetGO.GetComponentInChildren<MechBayMechStorageWidget>(true);
//            if (storageWidget != null) {
//              GameObject uixPrfPanl_AA_SalvageLeftPanel = this.gameObject.FindComponent<Transform>("uixPrfPanl_AA_SalvageLeftPanel").gameObject;
//              RectTransform deco = uixPrfPanl_AA_SalvageLeftPanel.FindComponent<RectTransform>("deco");
//              RectTransform storageWidgetRT = storageWidget.gameObject.GetComponent<RectTransform>();
//              storageWidgetRT.pivot = new Vector2(0f, 1f);
//              storageWidgetRT.position = deco.position;
//              storageWidgetRT.localScale = new Vector3(0.9f, 0.9f, 0.9f);
//              Transform toDisable = storageWidget.gameObject.FindComponent<Transform>("T_brackets_cap (3)");
//              toDisable?.gameObject.SetActive(false);
//              toDisable = storageWidget.gameObject.FindComponent<Transform>("uixPrfDeco_brackets_cap-Bttm (1)");
//              toDisable?.gameObject.SetActive(false);
//              toDisable = storageWidget.gameObject.FindComponent<Transform>("Deco (1)");
//              toDisable?.gameObject.SetActive(false);
//              Vector3 pos = storageWidgetRT.position;
//              pos.x = -100f;
//              storageWidgetRT.position = pos;
//            }
//          }
//        } else {
//          storageWidgetGO = storageWidget.gameObject;
//        }
//        if (storageWidget != null) {
//          storageWidget.SetData(this, this.Sim.DataManager, "uixPrfPanl_storageMechUnit-Element", false, true, MechLabDraggableItemType.Chassis);
//          storageWidget.InitInventory(this.Sim.GetAllInventoryMechDefs(), true);
//        }
//        showHideButton?.SetState(ButtonState.Enabled, true);
//      }catch(Exception e) {
//        Log.TWL(0,e.ToString(),true);
//      }
//    }
//    public void Init(AAR_SalvageScreen salvageScreen) {
//      this.salvageScreen = salvageScreen;
//      MechBayMechStorageWidget existingStorageWidget = this.gameObject.GetComponentInChildren<MechBayMechStorageWidget>(true);
//      if (existingStorageWidget != null) { existingStorageWidget.gameObject.SetActive(false); }
//      RectTransform hideShowButton = this.gameObject.FindComponent<RectTransform>("uixPrfPanl_AA_storageButtonPanel");
//      if(hideShowButton == null) {
//        GameObject buttonSource = this.gameObject.FindComponent<Transform>("uixPrfPanl_AA_nextButtonPanel").gameObject;
//        this.storageButtonGO = GameObject.Instantiate(buttonSource);
//        hideShowButton = storageButtonGO.transform as RectTransform;
//        storageButtonGO.name = "uixPrfPanl_AA_storageButtonPanel";
//        hideShowButton.SetParent(buttonSource.transform.parent);
//        GameObject uixPrfPanl_AA_SalvageLeftPanel = this.gameObject.FindComponent<Transform>("uixPrfPanl_AA_SalvageLeftPanel").gameObject;
//        RectTransform deco = uixPrfPanl_AA_SalvageLeftPanel.FindComponent<RectTransform>("deco");
//        deco.gameObject.SetActive(false);
//        hideShowButton.position = deco.position;
//        hideShowButton.localScale = new Vector3(0.5f,0.5f,0.5f);
//        hideShowButton.pivot = new Vector2(0f,0f);
//        showHideButton = hideShowButton.gameObject.GetComponentInChildren<HBSDOTweenButton>();
//        showHideButton.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
//        showHideButton.OnClicked = new UnityEngine.Events.UnityEvent();
//        showHideButton.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnHideShowButton));
//        showHideButton.SetState(ButtonState.Enabled, true);
//        LocalizableText text = hideShowButton.GetComponentInChildren<LocalizableText>();
//        text?.SetText("STORAGE");
//      }      
//    }
//    public void OnDestroy() {
//      Log.TWL(0, "SalavageStorageWidget.OnDestroy");
//      if (this.storageWidget != null) { storageWidget = null; }
//      if (storageWidgetGO != null) { GameObject.Destroy(storageWidgetGO); storageWidgetGO = null; }
//      if (storageButtonGO != null) { GameObject.Destroy(storageButtonGO); storageButtonGO = null; }
//    }
//  }
//  [HarmonyPatch(typeof(AAR_SalvageScreen))]
//  [HarmonyPatch("InitializeData")]
//  [HarmonyPatch(MethodType.Normal)]
//  public static class AAR_SalvageScreen_InitializeData {
//    public static void Postfix(AAR_SalvageScreen __instance) {
//      try {
//        SalavageStorageWidget storageWidget = __instance.GetComponent<SalavageStorageWidget>();
//        if (storageWidget == null) {
//          storageWidget = __instance.gameObject.AddComponent<SalavageStorageWidget>();
//        }
//        if(storageWidget != null) {
//          storageWidget.Init(__instance);
//        }
//      } catch(Exception e) {
//        Log.TWL(0,e.ToString(),true);
//      }
//    }
//  }
//  [HarmonyPatch(typeof(AAR_SalvageScreen))]
//  [HarmonyPatch("OnCompleted")]
//  [HarmonyPatch(MethodType.Normal)]
//  public static class AAR_SalvageScreen_OnCompleted {
//    public static void Prefix(AAR_SalvageScreen __instance) {
//      try {
//        SalavageStorageWidget storageWidget = __instance.GetComponent<SalavageStorageWidget>();
//        if (storageWidget == null) {
//          GameObject.Destroy(storageWidget);
//        }
//      } catch (Exception e) {
//        Log.TWL(0, e.ToString(), true);
//      }
//    }
//  }

//}