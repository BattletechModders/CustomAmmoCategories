/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
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
using HarmonyLib;
using HBS.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnits {
  public class CustomBayShower: MonoBehaviour, IPointerEnterHandler {
    public CustomBaysUICaster parent { get; set; } = null;
    public void OnPointerEnter(PointerEventData eventData) {
      if(parent != null) {
        foreach (RectTransform aBay in parent.additionalBays) {
          aBay.gameObject.SetActive(true);
        }
        HBSDOTweenToggle main_toggle = this.parent.mainBay.gameObject.GetComponent<HBSDOTweenToggle>();
        this.parent.mainBay.gameObject.GetComponentInChildren<LocalizableText>(true).SetText("Mech Bays");
      }
    }
  }
  public class CustomBayHider : MonoBehaviour, IPointerExitHandler {
    public CustomBaysUICaster parent { get; set; } = null;
    public void OnPointerExit(PointerEventData eventData) {
      if (parent != null) {
        foreach (RectTransform aBay in parent.additionalBays) {
          aBay.gameObject.SetActive(false);
        }
        HBSDOTweenToggle cur_toggle = this.parent.currentBay.gameObject.GetComponent<HBSDOTweenToggle>();
        HBSDOTweenToggle main_toggle = this.parent.mainBay.gameObject.GetComponent<HBSDOTweenToggle>();
        if (cur_toggle != main_toggle) {
          main_toggle.SetState(cur_toggle.State);
          LocalizableText main_text = this.parent.mainBay.gameObject.GetComponentInChildren<LocalizableText>(true);
          LocalizableText cur_text = this.parent.currentBay.gameObject.GetComponentInChildren<LocalizableText>(true);
          main_text.SetText(cur_text.text);
        }
      }
    }
  }
  public class CustomBaysButton : MonoBehaviour, IPointerClickHandler {
    public CustomBaysButtonsController parent { get; set; } = null;
    public void OnPointerClick(PointerEventData eventData) {
      if (parent == null) { return; }
      if(this.parent.parent != null) {
        RectTransform rectTransform = this.gameObject.GetComponent<RectTransform>();
        if (this.parent.parent.currentBay != rectTransform) {
          if (this.parent.parent.mainBay == rectTransform) {
            this.parent.parent.currentBay = rectTransform;
            this.parent.parent.currentHangarInfo.definition = null;
            this.parent.parent.BayPanel.ViewBays();
          } else
          if (this.parent.parent.additionalBays.Contains(rectTransform)) {
            this.parent.parent.currentBay = rectTransform;
            CustomHangarInfo info = rectTransform.gameObject.GetComponent<CustomHangarInfo>();
            this.parent.parent.currentHangarInfo.definition = info == null ? null : info.definition;
            this.parent.parent.BayPanel.ViewBays();
          }
        }
      }
      HBSDOTweenToggle thisToggle = this.gameObject.GetComponent<HBSDOTweenToggle>();
      foreach (HBSDOTweenToggle toggle in this.parent.buttons) {
        if (toggle == thisToggle) { continue; }
        toggle.SetState(ButtonState.Enabled);
      }
    }
  }
  public class CustomBaysButtonsController : MonoBehaviour {
    public List<HBSDOTweenToggle> buttons { get; set; } = null;
    public CustomBaysUICaster parent { get; set; } = null;
    public void Update() {
      if(buttons == null) {
        buttons = new List<HBSDOTweenToggle>();
        HBSDOTweenToggle[] src_buttons = this.gameObject.GetComponentsInChildren<HBSDOTweenToggle>(true);
        foreach(HBSDOTweenToggle btn in src_buttons) { buttons.Add(btn); btn.gameObject.AddComponent<CustomBaysButton>().parent = this; }
      }
    }
  }
  public class CustomHangarInfo: MonoBehaviour {
    public CustomHangarDef definition { get; set; } = null;
  }
  public class CustomBaysPopupUICaster: MonoBehaviour {
    private bool UIInited { get; set; } = false;
    private RectTransform m_rectTransform = null;
    public SimGameState SimGame { get; set; } = null;
    public MechPlacementPopup parent { get; set; } = null;
    public MechBayPanel BayPanel { get; set; } = null;
    public RectTransform rectTransform { get { if (m_rectTransform != null) { return m_rectTransform; } m_rectTransform = this.transform as RectTransform; return m_rectTransform; } }
    public void Update() {
      if (UIInited) { return; }
      if (SimGame == null) { return; }
      if (parent == null) { return; }
      if (BayPanel == null) { return; }
      if (SimGame.mechBayPanel() == null) { return; }
      try {
        Transform layout_storageScroller = Traverse.Create(this.BayPanel).Field<MechBayMechStorageWidget>("storageWidget").Value.gameObject.transform.FindRecursive("layout_storageScroller");
        Transform layout_bays = Traverse.Create(this.parent).Field<MechBayRowGroupWidget>("rowGroupWidget").Value.gameObject.transform.FindRecursive("layout_bays");
        if ((layout_storageScroller != null) && (layout_bays != null)) {
          GameObject layout_baysScroller = GameObject.Instantiate(layout_storageScroller.gameObject);
          layout_baysScroller.transform.SetParent(layout_bays.parent);
          layout_baysScroller.name = "layout_baysScroller";
          RectTransform layout_baysScrollerTR = layout_baysScroller.transform as RectTransform;
          RectTransform layout_baysTR = layout_bays as RectTransform;
          layout_baysScrollerTR.sizeDelta = layout_baysTR.sizeDelta;
          layout_baysScroller.transform.localPosition = layout_baysTR.localPosition;
          layout_baysScroller.transform.localRotation = layout_baysTR.localRotation;
          layout_baysScroller.transform.localScale = layout_baysTR.localScale;
          layout_baysScrollerTR.pivot = layout_baysTR.pivot;
          GridLayoutGroup grid = layout_baysScroller.GetComponentInChildren<GridLayoutGroup>();
          GameObject gridGO = grid.gameObject;
          GameObject.DestroyImmediate(grid);
          VerticalLayoutGroup old_vertical = layout_bays.GetComponent<VerticalLayoutGroup>();
          VerticalLayoutGroup vertical = gridGO.AddComponent<VerticalLayoutGroup>();
          vertical.childAlignment = old_vertical.childAlignment;
          vertical.childControlHeight = old_vertical.childControlHeight;
          vertical.childControlWidth = old_vertical.childControlWidth;
          vertical.childForceExpandHeight = old_vertical.childForceExpandHeight;
          vertical.childForceExpandWidth = old_vertical.childForceExpandWidth;
          vertical.spacing = old_vertical.spacing;
          vertical.padding = old_vertical.padding;
          HashSet<Transform> old_childs = new HashSet<Transform>();
          Log.TWL(0, "CustomBaysPopupUICaster.Spawn grid already contains:"+ gridGO.transform.childCount);
          for(int t=0;t < gridGO.transform.childCount; ++t) {
            old_childs.Add(gridGO.transform.GetChild(t));
          }
          foreach (Transform child in old_childs) {
            Log.WL(1,"removing:"+child.name, true);
            GameObject.DestroyImmediate(child.gameObject);
          }
          HashSet<Transform> childs = new HashSet<Transform>();
          List<MechBayRowWidget> rows = new List<MechBayRowWidget>(Traverse.Create(this.parent).Field<MechBayRowGroupWidget>("rowGroupWidget").Value.Bays);
          Transform rowsParent = rows[0].gameObject.transform.parent;
          Log.TWL(0, "CustomBaysPopupUICaster.Spawn rowsParent:"+ rowsParent.name);
          for (int t = 0; t < rows[0].gameObject.transform.parent.childCount; ++t) {
            Transform child = rows[0].gameObject.transform.parent.GetChild(t); childs.Add(child);
            Log.WL(1, child.name);
          }
          foreach (Transform child in childs) {
            child.SetParent(gridGO.transform);
            Log.WL(1, child.name+".parent => "+ child.parent.transform.name);
          }
          layout_bays.gameObject.SetActive(false);
          //foreach (MechBayRowWidget row in Traverse.Create(this.parent).Field<MechBayRowGroupWidget>("rowGroupWidget").Value.Bays) {
           //row.transform.SetParent(gridGO.transform);
          //}
          ScrollRect scroll = layout_baysScroller.GetComponent<ScrollRect>();
          scroll.verticalScrollbar.gameObject.transform.localPosition = new Vector3(layout_baysScrollerTR.sizeDelta.x, 0f - (layout_baysScrollerTR.sizeDelta.y/2f), 0f);
          int baysCount = Core.Settings.baysWidgetsCount > rows.Count ? Core.Settings.baysWidgetsCount : rows.Count;
          for (int t = rows.Count; t < baysCount; ++t) {
            GameObject bayRowGO = GameObject.Instantiate(rows[0].gameObject);
            bayRowGO.transform.SetParent(rows[0].gameObject.transform.parent);
            bayRowGO.transform.localPosition = rows[0].gameObject.transform.localPosition;
            bayRowGO.transform.localScale = rows[0].gameObject.transform.localScale;
            bayRowGO.transform.localRotation = rows[0].gameObject.transform.localRotation;
            bayRowGO.transform.SetSiblingIndex(t);
            rows.Add(bayRowGO.GetComponent<MechBayRowWidget>());
          }
          Traverse.Create(Traverse.Create(this.parent).Field<MechBayRowGroupWidget>("rowGroupWidget").Value).Field<MechBayRowWidget[]>("bays").Value = rows.ToArray();
          Traverse.Create(this.parent).Field<MechBayRowGroupWidget>("rowGroupWidget").Value.SetData(this.parent, this.SimGame);
        }
        UIInited = true;
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  public class CustomBaysUICaster: MonoBehaviour {
    private bool UIInited { get; set; } = false;
    private bool UIAligned { get; set; } = false;
    private RectTransform m_rectTransform = null;
    public RectTransform rectTransform { get { if (m_rectTransform != null) { return m_rectTransform; } m_rectTransform = this.transform as RectTransform; return m_rectTransform; } }
    public RectTransform baysTR { get; set; } = null;
    public RectTransform mainBay { get; set; } = null;
    public RectTransform currentBay { get; set; } = null;
    public CustomHangarInfo currentHangarInfo { get; set; } = null;
    public MechBayPanel BayPanel { get; set; } = null;
    public List<RectTransform> additionalBays = new List<RectTransform>();
    private Vector2 pendingPivot = new Vector2(0.5f, 0.5f);
    public void Update() {
      if(UIInited&&(UIAligned == false)) {
        UIAligned = true;
        this.mainBay.localPosition = Vector3.zero;
        for(int t=0;t < additionalBays.Count; ++t) {
          additionalBays[t].localPosition = new Vector3(0f, (this.mainBay.sizeDelta.y+10f)*(t+1), 0f);
        }
      }
      if (UIInited) { return; }
      try {
        if (this.BayPanel != null) {
          Transform uixPrfBttn_BASE_TabMedium_bays = this.gameObject.transform.FindTopLevelChild("uixPrfBttn_BASE_TabMedium-bays");
          if (uixPrfBttn_BASE_TabMedium_bays != null) {
            UIInited = true;
            uixPrfBttn_BASE_TabMedium_bays.name = "uixPrfBttn_BASE_TabMedium-bays0";
            GameObject bays = new GameObject("uixPrfBttn_BASE_TabMedium-tab-bays", typeof(RectTransform));
            this.baysTR = bays.transform as RectTransform;
            RectTransform uixPrfBttn_BASE_TabMedium_bays_tr = (uixPrfBttn_BASE_TabMedium_bays.transform as RectTransform);
            this.baysTR.sizeDelta = uixPrfBttn_BASE_TabMedium_bays_tr.sizeDelta;
            this.baysTR.SetParent(uixPrfBttn_BASE_TabMedium_bays.transform.parent);
            this.baysTR.pivot = uixPrfBttn_BASE_TabMedium_bays_tr.pivot;
            this.baysTR.localPosition = Vector3.zero;
            GameObject baysLayout = new GameObject("bays", typeof(RectTransform));
            Image img = baysLayout.AddComponent<Image>();
            Color transparent = Color.white;
            transparent.a = 0f;
            img.color = transparent;
            baysLayout.transform.SetParent(baysTR);
            RectTransform baysLayoutTR = baysLayout.transform as RectTransform;
            baysLayoutTR.localPosition = Vector3.zero;
            baysLayoutTR.pivot = new Vector2(0.5f, 0.0f);
            baysLayoutTR.sizeDelta = new Vector3(uixPrfBttn_BASE_TabMedium_bays_tr.sizeDelta.x, (uixPrfBttn_BASE_TabMedium_bays_tr.sizeDelta.y + 10f) * (CustomHangarHelper.listHangars.Count) + uixPrfBttn_BASE_TabMedium_bays_tr.sizeDelta.y / 2f);
            uixPrfBttn_BASE_TabMedium_bays_tr.SetParent(baysLayout.transform);
            uixPrfBttn_BASE_TabMedium_bays_tr.localPosition = Vector3.zero;
            this.mainBay = uixPrfBttn_BASE_TabMedium_bays_tr;
            bays.AddComponent<LayoutElement>().ignoreLayout = false;
            this.baysTR.SetAsFirstSibling();
            for (int t = this.baysTR.childCount; t <= CustomHangarHelper.listHangars.Count; ++t) {
              Transform tab = GameObject.Instantiate(uixPrfBttn_BASE_TabMedium_bays).transform;
              tab.gameObject.name = "uixPrfBttn_BASE_TabMedium-bays" + (t).ToString();
              tab.SetParent(uixPrfBttn_BASE_TabMedium_bays.parent);
              tab.localPosition = new Vector3(0f, uixPrfBttn_BASE_TabMedium_bays_tr.localPosition.y + uixPrfBttn_BASE_TabMedium_bays_tr.sizeDelta.y * t + 10f * t, 0f);
              tab.localScale = uixPrfBttn_BASE_TabMedium_bays.localScale;
              tab.localRotation = uixPrfBttn_BASE_TabMedium_bays.localRotation;
              HBSDOTweenToggle toggle = tab.gameObject.GetComponent<HBSDOTweenToggle>();
              toggle.SetState(ButtonState.Enabled);
              additionalBays.Add(tab as RectTransform);
              tab.gameObject.SetActive(false);
              toggle.gameObject.GetComponentInChildren<LocalizableText>(true).SetText(CustomHangarHelper.listHangars[t - 1].Description.Name);
              CustomHangarInfo hangarInfo = tab.gameObject.GetComponent<CustomHangarInfo>();
              if (hangarInfo == null) { hangarInfo = tab.gameObject.AddComponent<CustomHangarInfo>(); }
              hangarInfo.definition = CustomHangarHelper.listHangars[t - 1];
            }
            CustomBayShower shower = uixPrfBttn_BASE_TabMedium_bays.gameObject.GetComponent<CustomBayShower>();
            if (shower == null) { shower = uixPrfBttn_BASE_TabMedium_bays.gameObject.AddComponent<CustomBayShower>(); }
            shower.parent = this;
            CustomBayHider hider = baysLayout.gameObject.GetComponent<CustomBayHider>();
            if (hider == null) { hider = baysLayout.gameObject.AddComponent<CustomBayHider>(); }
            hider.parent = this;
            CustomBaysButtonsController btn_ctrl = this.gameObject.GetComponent<CustomBaysButtonsController>();
            if (btn_ctrl == null) { btn_ctrl = this.gameObject.AddComponent<CustomBaysButtonsController>(); }
            this.currentBay = this.mainBay;
            btn_ctrl.parent = this;
            currentHangarInfo = Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value.gameObject.GetComponent<CustomHangarInfo>();
            if (currentHangarInfo == null) {
              currentHangarInfo = Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value.gameObject.AddComponent<CustomHangarInfo>();
            }
            currentHangarInfo.definition = null;
          }
          Transform layout_storageScroller = Traverse.Create(this.BayPanel).Field<MechBayMechStorageWidget>("storageWidget").Value.gameObject.transform.FindRecursive("layout_storageScroller");
          Transform layout_bays = Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value.gameObject.transform.FindRecursive("layout_bays");
          if ((layout_storageScroller != null)&&(layout_bays != null)) {
            GameObject layout_baysScroller = GameObject.Instantiate(layout_storageScroller.gameObject);
            layout_baysScroller.transform.SetParent(layout_bays.parent);
            layout_baysScroller.name = "layout_baysScroller";
            layout_baysScroller.transform.localPosition = layout_storageScroller.localPosition;
            layout_baysScroller.transform.localRotation = layout_storageScroller.localRotation;
            layout_baysScroller.transform.localScale = layout_storageScroller.localScale;
            RectTransform layout_baysScrollerTR = layout_baysScroller.transform as RectTransform;
            RectTransform layout_baysTR = layout_bays as RectTransform;
            layout_baysScrollerTR.sizeDelta = layout_baysTR.sizeDelta;
            GridLayoutGroup grid = layout_baysScroller.GetComponentInChildren<GridLayoutGroup>();
            GameObject gridGO = grid.gameObject;
            GameObject.DestroyImmediate(grid);
            VerticalLayoutGroup old_vertical = layout_bays.GetComponent<VerticalLayoutGroup>();
            VerticalLayoutGroup vertical = gridGO.AddComponent<VerticalLayoutGroup>();
            vertical.childAlignment = old_vertical.childAlignment;
            vertical.childControlHeight = old_vertical.childControlHeight;
            vertical.childControlWidth = old_vertical.childControlWidth;
            vertical.childForceExpandHeight = old_vertical.childForceExpandHeight;
            vertical.childForceExpandWidth = old_vertical.childForceExpandWidth;
            vertical.spacing = old_vertical.spacing;
            vertical.padding = old_vertical.padding;
            HashSet<Transform> old_childs = new HashSet<Transform>();
            Log.TWL(0, "CustomBaysUICaster.Spawn grid already contains:" + gridGO.transform.childCount);
            for (int t = 0; t < gridGO.transform.childCount; ++t) {
              old_childs.Add(gridGO.transform.GetChild(t));
            }
            foreach (Transform child in old_childs) {
              Log.WL(1, "removing:" + child.name, true);
              GameObject.DestroyImmediate(child.gameObject);
            }
            layout_bays.gameObject.SetActive(false);
            foreach(MechBayRowWidget row in Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value.Bays) {
              row.transform.SetParent(gridGO.transform);
            }
            ScrollRect scroll = layout_baysScroller.GetComponent<ScrollRect>();
            scroll.verticalScrollbar.gameObject.transform.localPosition = new Vector3(layout_baysScrollerTR.sizeDelta.x / 2f - 30f,0f,0f);
            List<MechBayRowWidget> rows = new List<MechBayRowWidget>(Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value.Bays);
            int baysCount = Core.Settings.baysWidgetsCount > rows.Count? Core.Settings.baysWidgetsCount : rows.Count;
            for (int t = rows.Count; t < baysCount; ++t) {
              Log.WL(1, $"Instantiate:{t}");
              //GameObject bayRowGO = GameObject.Instantiate(rows[0].gameObject);
              //"uixPrfPanl_SIM_mechBay_bay-Element";
              //bool needInit = false;
              GameObject bayRowGO = BayPanel.DataManager.PooledInstantiate("uixPrfPanl_SIM_mechBay_bay-Element",BattleTechResourceType.UIModulePrefabs);
              //if (bayRowGO != null) {
              //Log.WL(2, $"from data manager");
              //} else {
              //GameObject bayRowGO = GameObject.Instantiate(rows[0].gameObject);
              //needInit = true;
              //}
              bayRowGO.transform.SetParent(rows[0].gameObject.transform.parent);
              bayRowGO.transform.localPosition = rows[0].gameObject.transform.localPosition;
              bayRowGO.transform.localScale = rows[0].gameObject.transform.localScale;
              bayRowGO.transform.localRotation = rows[0].gameObject.transform.localRotation;
              //if (needInit) {
                UIModule[] modules = bayRowGO.GetComponentsInChildren<UIModule>(true);
                foreach (UIModule module in modules) {
                  Log.WL(3, $"initing UI module {module.name}:{module.GetType()}");
                  module.Init();
                }
              //}
              rows.Add(bayRowGO.GetComponent<MechBayRowWidget>());
            }
            Traverse.Create(Traverse.Create(this.BayPanel).Field<MechBayRowGroupWidget>("bayGroupWidget").Value).Field<MechBayRowWidget[]>("bays").Value = rows.ToArray();
            BayPanel.ViewBays();
          }
        }
      } catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("GetBayRowFromSlot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechBayPanel_GetBayRowFromSlot {
    public static void Prefix(MechBayPanel __instance,ref int slot) {
      Log.TWL(0, "MechBayPanel.GetBayRowFromSlot "+slot);
      try {
        CustomHangarInfo hangar = __instance.GetComponentInChildren<CustomHangarInfo>();
        if (hangar == null) { return; }
        if (hangar.definition == null) { return; }
        slot -= hangar.definition.PositionShift;
        Log.WL(1, "hangar:"+hangar.definition.Description.Id+" slot:"+slot);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("ViewMechStorage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayPanel_ViewMechStorage {
    public static void Postfix(MechBayPanel __instance, MechBayRowGroupWidget ___bayGroupWidget, SimGameState ___sim) {
      Log.TWL(0, "MechBayPanel.ViewMechStorage ");
      try {
        CustomHangarInfo hangar = ___bayGroupWidget.GetComponentInChildren<CustomHangarInfo>();
        if (hangar == null) { return; }
        if (hangar.definition == null) { return; }
        hangar.definition = null;
        ___bayGroupWidget.SetData((IMechLabDropTarget)__instance, ___sim);
        CustomBaysUICaster baysUI = __instance.gameObject.GetComponentInChildren<CustomBaysUICaster>(true);
        if(baysUI != null) {
          baysUI.currentBay = baysUI.mainBay;
          baysUI.mainBay.gameObject.GetComponentInChildren<LocalizableText>(true).SetText("Mech Bays");
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(SimGameState) })]
  public static class MechBayPanel_Init {
    private static MechBayPanel f_mechBayPanel = null;
    public static MechBayPanel mechBayPanel(this SimGameState sim) { return f_mechBayPanel; }
    public static void Prefix(MechBayPanel __instance, SimGameState sim) {
      try {
        Log.TWL(0, "MechBayPanel.Init");
        f_mechBayPanel = __instance;
        Transform layout_tabs = __instance.gameObject.transform.FindRecursive("layout_tabs");
        if (layout_tabs != null) {
          CustomBaysUICaster caster = layout_tabs.gameObject.GetComponent<CustomBaysUICaster>();
          if (caster == null) { caster = layout_tabs.gameObject.AddComponent<CustomBaysUICaster>(); }
          caster.BayPanel = __instance;
        }

      } catch(Exception e) {
        Log.TWL(0,e.ToString(), true);
      }
      Log.TWL(0, "MechBayPanel.Inited");
    }
  }
  public static class CustomHangarHelper {
    private static Dictionary<string, CustomHangarDef> hangars = new Dictionary<string, CustomHangarDef>();
    private static List<CustomHangarDef> f_list_hangars = null;
    public static void Register(this CustomHangarDef def) {
      if (hangars.ContainsKey(def.Description.Id)) { hangars[def.Description.Id] = def; } else { hangars.Add(def.Description.Id, def); }
    }
    public static CustomHangarDef HangarDef(this ChassisDef chassis) {
      foreach(var hangar in hangars) {
        if (chassis.ChassisTags.ContainsAll(hangar.Value.tags)) { return hangar.Value; }
      }
      return null;
    }
    public static int FallbackVehicleShift() {
      if(hangars.TryGetValue(CustomHangarDef.DEFAULT_VEHICLE_HANGAR_ID, out CustomHangarDef def)) {
        return def.PositionShift;
      }
      return 0;
    }
    public static int GetHangarShift(this MechDef mechDef) {
      if (mechDef.DataManager == null) { mechDef.DataManager = UnityGameInstance.BattleTechGame.DataManager; }
      if (mechDef.Chassis == null) { mechDef.Chassis = mechDef.DataManager.ChassisDefs.Get(mechDef.ChassisID); };
      if (mechDef.Chassis == null) { throw new Exception(mechDef.Description.Id + " absent chassis "+ mechDef.ChassisID); }
      CustomHangarDef def = mechDef.Chassis.HangarDef();
      return def == null ? 0 : def.PositionShift;
    }
    public static int GetHangarShift(this ChassisDef chassisDef) {
      CustomHangarDef def = chassisDef.HangarDef();
      return def == null ? 0 : def.PositionShift;
    }
    public static List<CustomHangarDef> listHangars {
      get {
        if (f_list_hangars == null) {
          f_list_hangars = hangars.Values.ToList();
          f_list_hangars.Sort((x,y)=> { return x.PositionShift - y.PositionShift; });
        }
        return f_list_hangars;
      }
    }
    public static int MaxPositionShift {
      get {
        return listHangars.Count == 0 ? 0 : listHangars[listHangars.Count - 1].PositionShift;
      }
    }
  }
  public class CustomHangarDef {
    public static string DEFAULT_VEHICLE_HANGAR_ID = "vehicle_hangar";
    public static string DEFAULT_VEHICLE_HANGAR_NAME = "VEHICLE BAYS";
    public static int POSITION_SHIFT_DELTA = 1000;
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    [JsonIgnore]
    public TagSet tags { get; private set; } = new TagSet();
    public List<string> Tags { set { tags = new TagSet(value); } }
    [JsonIgnore]
    public int f_position_shift = -1;
    public int PositionShift {
      get {
        if(f_position_shift <= 0) {
          f_position_shift = CustomHangarHelper.MaxPositionShift + POSITION_SHIFT_DELTA;
        }
        return f_position_shift;
      }
      set {
        f_position_shift = value;
      }
    }
  }
}