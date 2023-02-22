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
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustomPrewarm;
using Harmony;
using HBS;
using Newtonsoft.Json;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnits {
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "OnLanceConfiguratorClosed")]
  public static class LanceConfiguratorPanel_OnLanceConfiguratorClosed {
    public static void Prefix(LanceConfiguratorPanel __instance) {
      LanceLoadPopupSupervisor.Instance.OnClose();
    }
  }

  public class LanceLayoutDataElement {
    public SavedLanceLayout lanceLayout { get; set; } = null;
    public BaseDescriptionDef Description { get; set; } = null;
    public LanceLayoutDataElement(SavedLanceLayout lanceLayout, LanceConfiguratorPanel lanceConf) {
      this.lanceLayout = lanceLayout;
      StringBuilder details = new StringBuilder();
      for(int t=0;t < lanceLayout.items.Count;++t) {
        var item = lanceLayout.items[t];
        IMechLabDraggableItem forcedMech = null;
        SGBarracksRosterSlot forcedPilot = null;
        if (string.IsNullOrEmpty(item.mech) == false) {
          forcedMech = lanceConf.mechListWidget.GetInventoryItem(item.mech);
        }
        if(string.IsNullOrEmpty(item.pilot) == false) {
          forcedPilot = lanceConf.pilotListWidget.GetPilot(item.pilot);
        }
        string mech = forcedMech==null ? string.Empty : forcedMech.MechDef.Chassis.VariantName;
        if ((string.IsNullOrEmpty(item.mech) == false) && (forcedMech == null)) { mech = string.IsNullOrEmpty(item.mechName)?"absent":item.mechName; }
        string pilot = forcedPilot==null ? string.Empty : forcedPilot.Pilot.Callsign;
        if ((string.IsNullOrEmpty(item.pilot) == false) && (forcedPilot == null)) { pilot = string.IsNullOrEmpty(item.pilotCallsign) ? "absent" : item.pilotCallsign; }
        if (string.IsNullOrEmpty(pilot) && string.IsNullOrEmpty(mech)) { continue; }
        if (string.IsNullOrEmpty(mech)) { mech = "not set"; }
        if (string.IsNullOrEmpty(pilot)) { pilot = "not set"; }
        details.AppendLine($"Slot {(t+1)} Unit: {mech} Pilot: {pilot}");
      }
      Description = new BaseDescriptionDef($"lanceLayout_{lanceLayout.name}", lanceLayout.name, details.ToString(), string.Empty);      
    }
  }
  public class LanceLayoutDeleteItemButton : EventTrigger {
    public SVGImage svg { get; set; } = null;
    public Image img { get; set; } = null;
    public LanceLayoutUIItem parent { get; set; } = null;
    public override void OnPointerEnter(PointerEventData eventData) {
      Log.TWL(0, "LanceLayoutDeleteItemButton.OnPointerEnter " + parent.data.Description.Name);
      svg.color = UIManager.Instance.UIColorRefs.orange;
    }
    public override void OnPointerExit(PointerEventData eventData) {
      Log.TWL(0, "LanceLayoutDeleteItemButton.OnPointerExit " + parent.data.Description.Name);
      svg.color = UIManager.Instance.UIColorRefs.white;
    }
    public override void OnPointerClick(PointerEventData eventData) {
      Log.TWL(0, "LanceLayoutDeleteItemButton.OnPointerClick " + parent.data.Description.Name);
      parent.parent.DeleteItem(parent);
    }
  }

  public class LanceLayoutUIItem : MonoBehaviour, IPointerClickHandler {
    public LanceLayoutDataElement data { get; set; } = null;
    public LanceLayoutControl parent { get; set; } = null;
    public LayoutElement layoutElement { get; set; } = null;
    public LanceMechEquipmentListItem ui { get; set; } = null;
    public LanceLayoutDeleteItemButton settingsButton { get; set; } = null;
    private RectTransform f_rect = null;
    public RectTransform rect {
      get {
        if (f_rect == null) { f_rect = GetComponent<RectTransform>(); }
        return f_rect;
      }
    }
    private bool uiInited = false;
    public void Update() {
      if (uiInited == false) {
        RectTransform rectTransform = this.ui.GetComponent<RectTransform>();
        settingsButton.img.rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x, 1f);
        uiInited = true;
      }
    }

    public void Init() {
      RectTransform rectTransform = this.ui.GetComponent<RectTransform>();
      settingsButton = this.ui.GetComponentInChildren<LanceLayoutDeleteItemButton>(true);
      if (settingsButton == null) {
        GameObject itemGO = new GameObject("settingsImg");
        itemGO.transform.SetParent(this.ui.gameObject.transform);
        Image img = itemGO.AddComponent<Image>();
        img.rectTransform.pivot = new Vector2(1f, 0);
        img.rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * rectTransform.localScale.y - 2f, rectTransform.sizeDelta.y * rectTransform.localScale.y - 2f);
        img.color = Color.clear;
        img.rectTransform.anchoredPosition = new Vector2(rectTransform.sizeDelta.x, 1f);
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.zero;
        GameObject svgGO = new GameObject("icon");
        svgGO.transform.SetParent(itemGO.transform);
        svgGO.transform.localPosition = Vector3.zero;
        svgGO.transform.localScale = Vector3.one;
        SVGImage svg = svgGO.AddComponent<SVGImage>();
        svg.rectTransform.pivot = Vector2.zero;
        svg.rectTransform.sizeDelta = img.rectTransform.sizeDelta;
        svg.rectTransform.anchorMin = Vector2.zero;
        svg.rectTransform.anchorMax = Vector2.zero;
        CustomSvgCache.setIcon(svg, "delete-layout", UIManager.Instance.dataManager);
        settingsButton = itemGO.AddComponent<LanceLayoutDeleteItemButton>();
        settingsButton.svg = svg;
        settingsButton.img = img;
      }
      settingsButton.gameObject.SetActive(this.data.lanceLayout.clear == false);
      settingsButton.parent = this;
      this.uiInited = false;
    }
    public void OnPointerClick(PointerEventData eventData) {
      Log.TWL(0, "LanceLayoutUIItem.OnPointerClick");
      parent.selected = this;
      parent.UpdateColors();
    }
  }

  public class LanceLayoutControl : MonoBehaviour {
    public RectTransform listParent { get; set; } = null;
    public LocalizableText componentCountText { get; set; } = null;
    public List<LanceLayoutUIItem> layoutsList { get; set; } = new List<LanceLayoutUIItem>();
    public LanceLayoutUIItem selected { get; set; } = null;
    //public TargetUIItem placeholderItem { get; set; } = null;
    public LanceLoadPopupSupervisor parent { get; set; } = null;
    public void DeleteItem(LanceLayoutUIItem item) {
      SaveDropLayoutHelper.Delete(item.data.lanceLayout);
      layoutsList.Remove(item);
      GameObject.Destroy(item.gameObject);
    }
    public void AddUIItem(LanceLayoutDataElement dataElement) {
      GameObject gameObject = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_LC_LanceLayoutItem", BattleTechResourceType.UIModulePrefabs);
      if (gameObject == null) {
        gameObject = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_LC_MechLoadoutItem", BattleTechResourceType.UIModulePrefabs);
        gameObject.name = "uixPrfPanl_LC_LanceLayoutItem(Clone)";
      }
      gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
      LanceLayoutUIItem targetItem = gameObject.GetComponent<LanceLayoutUIItem>();
      if (null == targetItem) { targetItem = gameObject.AddComponent<LanceLayoutUIItem>(); }
      targetItem.layoutElement = gameObject.GetComponent<LayoutElement>();
      if (null == targetItem.layoutElement) { targetItem.layoutElement = gameObject.AddComponent<LayoutElement>(); }
      targetItem.layoutElement.ignoreLayout = false;
      targetItem.data = dataElement;
      targetItem.parent = this;
      LanceMechEquipmentListItem component = gameObject.GetComponent<LanceMechEquipmentListItem>();
      targetItem.ui = component;
      if (dataElement != null) {
        UIColor textColor = UIColor.White;
        UIColor bgColor = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.Equipment.UIColor;
        string text = dataElement.Description.Name;
        component.SetData(text, ComponentDamageLevel.Functional, textColor, bgColor);
        Traverse.Create(component).Field<UIColorRefTracker>("itemTextColor").Value.SetUIColor(textColor);
        if(Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value == null) {
          Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value = component.GetComponent<HBSTooltip>();
        }
        if (Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value != null) {
          Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value.SetDefaultStateData(dataElement.Description.GetTooltipStateData());
        }
      }
      targetItem.Init();
      layoutsList.Add(targetItem);
      gameObject.transform.SetParent(listParent, false);
    }
    public void UpdateColors() {
      foreach (LanceLayoutUIItem item in layoutsList) {
        UIColor textColor = this.selected == item?UIColor.Gold:UIColor.White;
        Traverse.Create(item.ui).Field<UIColorRefTracker>("itemTextColor").Value.SetUIColor(textColor);
      }
    }
    public void Clear() {
      try {
        Log.TWL(0, "LanceLayoutControl.Clear");
        for (int index = this.layoutsList.Count - 1; index >= 0; --index) {
          LanceMechEquipmentListItem labItemSlotElement = this.layoutsList[index].ui;
          labItemSlotElement.gameObject.transform.SetParent((Transform)null, false);
          LanceLayoutUIItem item = this.layoutsList[index];
          if (item != null) {
            item.parent = null;
            item.data = null;
          }
          UIManager.Instance.dataManager.PoolGameObject("uixPrfPanl_LC_LanceLayoutItem", labItemSlotElement.gameObject);
        }
        this.layoutsList.Clear();
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }

  }

  public class LanceLoadPopupSupervisor : MonoBehaviour {
    public enum PopupState { Main };
    public GenericPopup popup { get; set; } = null;
    public PopupState state { get; set; } = PopupState.Main;
    public LanceLayoutControl layoutsControl { get; set; } = null;
    public HBSButton backButton { get; set; } = null;
    public HBSButton selectButton { get; set; } = null;
    //public HBSTooltip helpTooltip { get; set; } = null;
    public LanceConfiguratorPanel parent { get; set; } = null;
    private static LanceLoadPopupSupervisor f_Instance = null;
    public static LanceLoadPopupSupervisor Instance {
      get {
        if (f_Instance == null) {
          f_Instance = UIManager.Instance.UIRoot.gameObject.GetComponentInChildren<LanceLoadPopupSupervisor>(true);
          if (f_Instance == null) {
            GameObject go = new GameObject("LancesLoadPopupSupervisor");
            go.transform.SetParent(UIManager.Instance.UIRoot.gameObject.transform);
            f_Instance = go.AddComponent<LanceLoadPopupSupervisor>();
            f_Instance.instantine();
          }
        }
        return f_Instance;
      }
    }
    public void instantine() {
      if (layoutsControl != null) { return; }
      try {
        GameObject uixPrfPanl_ML_main_Widget = LazySingletonBehavior<UIManager>.Instance.dataManager.PooledInstantiate("uixPrfPanl_ML_main-Widget", BattleTechResourceType.UIModulePrefabs);
        if (uixPrfPanl_ML_main_Widget == null) {
          Log.TWL(0, "uixPrfPanl_ML_main-Widget not found");
          return;
        }
        Log.TWL(0, "uixPrfPanl_ML_main-Widget found");
        MechLabDismountWidget dismountWidget = uixPrfPanl_ML_main_Widget.GetComponentInChildren<MechLabDismountWidget>(true);
        if (dismountWidget == null) {
          Log.TWL(0, "MechLabDismountWidget not found");
          return;
        }
        Log.TWL(0, "MechLabDismountWidget found");

        {
          GameObject controlGO = GameObject.Instantiate(dismountWidget.gameObject);
          controlGO.name = "ui_weaponsOrder";
          controlGO.transform.localScale = Vector3.one;
          MechLabDismountWidget localWidget = controlGO.GetComponent<MechLabDismountWidget>();
          controlGO.FindObject<LocalizableText>("txt_label").SetText("LAYOUTS");
          controlGO.FindObject<LocalizableText>("txt_instr").gameObject.SetActive(false);
          layoutsControl = controlGO.AddComponent<LanceLayoutControl>();
          layoutsControl.listParent = Traverse.Create(localWidget).Field<RectTransform>("listParent").Value;
          layoutsControl.componentCountText = Traverse.Create(localWidget).Field<LocalizableText>("componentCountText").Value;
          GameObject.Destroy(localWidget);
          layoutsControl.componentCountText.SetText("");
          //helpTooltip = layoutsControl.componentCountText.gameObject.AddComponent<HBSTooltip>();
          //helpTooltip.SetDefaultStateData(new BaseDescriptionDef("TargetsHelpTooltipID", "__/CAE.TRG.USAGE/__", "__/CAE.TRG.USAGE.TARGETS.DETAILS/__", string.Empty).GetTooltipStateData());
          VerticalLayoutGroup verticalLayoutGroup = layoutsControl.listParent.gameObject.GetComponent<VerticalLayoutGroup>();
          verticalLayoutGroup.spacing = 22f;
          layoutsControl.gameObject.SetActive(false);
          layoutsControl.parent = this;
          layoutsControl.gameObject.transform.SetParent(this.transform);
        }

        LazySingletonBehavior<UIManager>.Instance.dataManager.PoolGameObject("uixPrfPanl_ML_main-Widget", uixPrfPanl_ML_main_Widget);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public void OnBack() {
      OnClose();
    }
    public void OnSelect() {
      try {
        if (this.layoutsControl.selected == null) { OnClose(); return; }
        if (this.layoutsControl.selected.data == null) { OnClose(); return; }
        var layout = this.layoutsControl.selected.data.lanceLayout;
        LanceConfiguratorPanel lanceConfPanel = this.parent;
        OnClose();
        LanceLoadPopupSupervisor.RestoreAs(layout, lanceConfPanel);
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
    public static void RestoreAs(SavedLanceLayout layout, LanceConfiguratorPanel lanceConfPanel) {
      LanceLoadoutSlot[] loadoutSlots = Traverse.Create(lanceConfPanel).Field<LanceLoadoutSlot[]>("loadoutSlots").Value;
      StringBuilder errors = new StringBuilder();
      if (layout.clear) {
        for (int t = 0; t < loadoutSlots.Length; ++t) {
          LanceLoadoutSlot slot = loadoutSlots[t];
          if (slot.isFullLocked) { continue; }
          slot.OnClearSlotsClicked();
        }
        return;
      }
      for (int t = 0; t < loadoutSlots.Length; ++t) {
        if (t >= layout.items.Count) { break; }
        SavedLanceSlot saved = layout.items[t];
        CustomLanceSlot customSlot = loadoutSlots[t].gameObject.GetComponent<CustomLanceSlot>();
        LanceLoadoutSlot slot = loadoutSlots[t];
        IMechLabDraggableItem forcedMech = null;
        SGBarracksRosterSlot forcedPilot = null;
        if (slot.isFullLocked) { continue; }
        if (slot.isMechLocked == false) {
          if (string.IsNullOrEmpty(saved.mech) == false) {
            forcedMech = lanceConfPanel.mechListWidget.GetInventoryItem(saved.mech);
            if (forcedMech != null) {
              if (MechValidationRules.ValidateMechCanBeFielded(lanceConfPanel.Sim, forcedMech.MechDef) == false) {
                errors.AppendLine(forcedMech.MechDef.Description.Name + " can't be fielded");
                forcedMech = null;
              }
              if ((customSlot != null) && (forcedMech != null)) {
                if (forcedMech.MechDef.Chassis.CanBeDropedInto(customSlot.slotDef, out string title, out string message) == false) {
                  errors.AppendLine(message);
                  forcedMech = null;
                }
              }
              float minTonnage = Traverse.Create(slot).Field<float>("minTonnage").Value;
              float maxTonnage = Traverse.Create(slot).Field<float>("maxTonnage").Value;
              if ((forcedMech != null) && (minTonnage > 0f)) {
                if (forcedMech.MechDef.Chassis.Tonnage < minTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too low tonnage");
                  forcedMech = null;
                }
              }
              if ((forcedMech != null) && (maxTonnage > 0f)) {
                if (forcedMech.MechDef.Chassis.Tonnage > maxTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too big tonnage");
                  forcedMech = null;
                }
              }
            }
          }
        }
        if (slot.isPilotLocked == false) {
          if (string.IsNullOrEmpty(saved.pilot) == false) {
            forcedPilot = lanceConfPanel.pilotListWidget.GetPilot(saved.pilot);
            if (forcedPilot != null) {
              if (forcedPilot.Pilot.CanPilot == false) {
                errors.AppendLine(forcedPilot.Pilot.Callsign + " can't pilot");
                forcedPilot = null;
              }
            }
          }
        }
        if (slot.isPilotLocked) { forcedPilot = slot.SelectedPilot; }
        if (slot.isMechLocked) { forcedMech = slot.SelectedMech; }
        if ((slot.isPilotLocked) && (slot.isMechLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if (forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedMech = null;
          }
        }
        if ((slot.isPilotLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if (forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedPilot = null;
          }
        }
        if (slot.isPilotLocked) { forcedPilot = null; } else if (forcedPilot != null) {
          lanceConfPanel.pilotListWidget.RemovePilot(lanceConfPanel.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == forcedPilot.Pilot.Description.Id)));
        }
        if (slot.isMechLocked) { forcedMech = null; } else if (forcedMech != null) {

        }
        slot.SetLockedData(forcedMech, forcedPilot, false);
      }
      if (errors.Length > 0) {
        GenericPopup popup = GenericPopupBuilder.Create("APPLY LAYOUT WARNINGS", errors.ToString()).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      }
    }

    public void OnLayoutsButtonClick(LanceConfiguratorPanel parent) {
      if (parent == null) { return; }
      this.parent = parent;
      GenericPopupBuilder builder = GenericPopupBuilder.Create("AVAILABLE LAYOUTS", "PLACEHOLDER");
      builder.AddButton("CLOSE", new Action(this.OnBack), true);
      builder.AddButton("LOAD", new Action(this.OnSelect), true);
      popup = builder.CancelOnEscape().Render();
      selectButton = Traverse.Create(popup).Field<List<HBSButton>>("buttons").Value[1];
      backButton = Traverse.Create(popup).Field<List<HBSButton>>("buttons").Value[0];
      this.OnShow();
    }
    public void OnClose() {
      try {
        if (layoutsControl != null) {
          layoutsControl.gameObject.SetActive(false);
          layoutsControl.gameObject.transform.SetParent(this.transform);
          layoutsControl.Clear();
        }
        parent = null;
        if (popup != null) {
          Traverse.Create(popup).Field<LocalizableText>("_contentText").Value.gameObject.SetActive(true);
          popup.Pool();
          popup = null;
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString(), true);
      }
    }
    public void OnShow() {
      if ((layoutsControl != null) && (popup != null)) {
        try {
          layoutsControl.Clear();
          LocalizableText _contentText = Traverse.Create(popup).Field<LocalizableText>("_contentText").Value;
          _contentText.gameObject.SetActive(false);
          {
            RectTransform controlRT = layoutsControl.gameObject.GetComponent<RectTransform>();
            controlRT.pivot = _contentText.rectTransform.pivot;
            controlRT.sizeDelta = new Vector2(_contentText.rectTransform.sizeDelta.x, controlRT.sizeDelta.y);

            layoutsControl.gameObject.SetActive(true);
            layoutsControl.gameObject.transform.SetParent(_contentText.gameObject.transform.parent);
            layoutsControl.gameObject.transform.SetSiblingIndex(_contentText.transform.GetSiblingIndex() + 1);
            LayoutElement layoutElement = layoutsControl.gameObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = false;
          }
          GameObject contentTextGO = GameObject.Instantiate(_contentText.gameObject);
          contentTextGO.SetActive(false);
          contentTextGO.transform.SetParent(_contentText.gameObject.transform.parent);
          contentTextGO.transform.SetSiblingIndex(layoutsControl.transform.GetSiblingIndex() + 1);
          contentTextGO.transform.localScale = Vector3.one;
          GameObject.Destroy(contentTextGO.GetComponent<LocalizableText>());
          HorizontalLayoutGroup group = contentTextGO.AddComponent<HorizontalLayoutGroup>();
          group.spacing = 8f;
          group.padding = new RectOffset(10, 10, 0, 0);
          group.childAlignment = TextAnchor.UpperCenter;
          group.childControlHeight = false;
          group.childControlWidth = false;
          group.childForceExpandHeight = false;
          group.childForceExpandWidth = false;
          var invList = SaveDropLayoutHelper.LoadAll();
          invList.Sort((a, b) => { return a.name.CompareTo(b.name); });
          SavedLanceLayout clear = new SavedLanceLayout();
          clear.name = "CLEAR";
          clear.clear = true;
          invList.Insert(0, clear);
          foreach(var item in invList) {
            layoutsControl.AddUIItem(new LanceLayoutDataElement(item, this.parent));
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
    }
  }
  public static class SaveDropLayoutHelper {
    public static string BaseDirectory { get; set; }
    public static void Init(string dir) {
      BaseDirectory = Path.Combine(dir, "user_drop_layouts");
      if (Directory.Exists(BaseDirectory) == false) { Directory.CreateDirectory(BaseDirectory); }
    }
    public static void Save(SavedLanceLayout layout) {
      try {
        string path = Path.Combine(BaseDirectory, layout.name + ".json");
        File.WriteAllText(path, JsonConvert.SerializeObject(layout, Formatting.Indented));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Delete(SavedLanceLayout layout) {
      try {
        string path = Path.Combine(BaseDirectory, layout.name + ".json");
        if (File.Exists(path)) { File.Delete(path); }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static List<SavedLanceLayout> LoadAll() {
      List<SavedLanceLayout> result = new List<SavedLanceLayout>();
      try {
        string[] files = Directory.GetFiles(BaseDirectory, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string f in files) {
          try {
            result.Add(JsonConvert.DeserializeObject<SavedLanceLayout>(File.ReadAllText(f)));
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return result;
    }
  }
  public class SavedLanceSlot {
    public string mech { get; set; } = string.Empty;
    public string mechName { get; set; } = string.Empty;
    public string pilot { get; set; } = string.Empty;
    public string pilotCallsign { get; set; } = string.Empty;
  }
  public class SavedLanceLayout {
    public string name { get; set; } = string.Empty;
    [JsonIgnore]
    public bool clear { get; set; } = false;
    public List<SavedLanceSlot> items { get; set; } = new List<SavedLanceSlot>();
  }
  public class LanceConfigSaver: MonoBehaviour {
    public LanceConfiguratorPanel parent { get; set; } = null;
    public HBSDOTweenButton button { get; set; } = null;
    public void Init(LanceConfiguratorPanel parent) {
      this.parent = parent;
      this.button = this.gameObject.GetComponent<HBSDOTweenButton>();
      this.button.OnClicked = new UnityEngine.Events.UnityEvent();
      this.button.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnClicked));
      List<GameObject> tweenObjects = Traverse.Create(this.button).Field<List<GameObject>>("tweenObjects").Value;
      tweenObjects[0] = this.gameObject.FindComponent<Transform>("bttn2_Fill").gameObject;
      tweenObjects[1] = this.gameObject.FindComponent<Transform>("bttn2_border").gameObject;
      tweenObjects[2] = this.gameObject.FindComponent<Transform>("bttn2_Text-optional").gameObject;
      LocalizableText title = this.gameObject.GetComponentInChildren<LocalizableText>();
      title.SetText("SAVE");
      HBSTooltip tooltip = this.gameObject.GetComponentInChildren<HBSTooltip>();
      tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject((object)"Save current drop layout"));
    }
    public void SaveLoadoutName(string name) {
      if (string.IsNullOrEmpty(name)) { return; }
      name = name.Replace('\\', '_');
      name = name.Replace(':', '_');
      Log.TWL(0, "LanceConfigSaver.SaveLoadoutName " + name);
      LanceLoadoutSlot[] loadoutSlots = Traverse.Create(this.parent).Field<LanceLoadoutSlot[]>("loadoutSlots").Value;
      SavedLanceLayout savedLanceLayout = new SavedLanceLayout();
      savedLanceLayout.name = name;
      for (int t = 0; t < loadoutSlots.Length; ++t) {
        MechDef mechDef = null;
        Pilot pilot = null;
        if ((loadoutSlots[t].SelectedMech != null)&&(loadoutSlots[t].isMechLocked == false)&&(loadoutSlots[t].isFullLocked == false)) {
          if(loadoutSlots[t].SelectedMech.MechDef != null) {
            mechDef = loadoutSlots[t].SelectedMech.MechDef;
          }
        }
        if((loadoutSlots[t].SelectedPilot != null)&& (loadoutSlots[t].isPilotLocked == false) && (loadoutSlots[t].isFullLocked == false)) {
          if (loadoutSlots[t].SelectedPilot.Pilot != null) {
            pilot = loadoutSlots[t].SelectedPilot.Pilot;
          }
        }
        SavedLanceSlot slot = new SavedLanceSlot();
        slot.mech = mechDef == null ? string.Empty : mechDef.Description.Id;
        slot.mechName = mechDef == null ? string.Empty : mechDef.Chassis.VariantName;
        slot.pilot = pilot == null ? string.Empty : pilot.Description.Id;
        slot.pilotCallsign = pilot == null ? string.Empty : pilot.Callsign;
        savedLanceLayout.items.Add(slot);
        Log.WL(1, "["+t+"] mech:"+(mechDef == null?"null":mechDef.Description.Id)+" pilot:"+(pilot==null?"null":pilot.Description.Id)+" isMechLocked:"+ loadoutSlots[t].isMechLocked+" isPilotLocked:"+loadoutSlots[t].isPilotLocked);
      }
      SaveDropLayoutHelper.Save(savedLanceLayout);
    }
    public void OnClicked() {
      GenericPopup popup = null;
      popup = GenericPopupBuilder.Create("SAVE CURRENT LAYOUT AS", "Input name for layout").AddInput("name", this.SaveLoadoutName).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
    }
  }
  public class LanceConfigLoader : MonoBehaviour {
    public LanceConfiguratorPanel parent { get; set; } = null;
    public HBSDOTweenButton button { get; set; } = null;
    public void Init(LanceConfiguratorPanel parent) {
      this.parent = parent;
      this.button = this.gameObject.GetComponent<HBSDOTweenButton>();
      this.button.OnClicked = new UnityEngine.Events.UnityEvent();
      this.button.OnClicked.AddListener(new UnityEngine.Events.UnityAction(this.OnClicked));
      List<GameObject> tweenObjects = Traverse.Create(this.button).Field<List<GameObject>>("tweenObjects").Value;
      tweenObjects[0] = this.gameObject.FindComponent<Transform>("bttn2_Fill").gameObject;
      tweenObjects[1] = this.gameObject.FindComponent<Transform>("bttn2_border").gameObject;
      tweenObjects[2] = this.gameObject.FindComponent<Transform>("bttn2_Text-optional").gameObject;
      LocalizableText title = this.gameObject.GetComponentInChildren<LocalizableText>();
      title.SetText("LOAD");
      HBSTooltip tooltip = this.gameObject.GetComponentInChildren<HBSTooltip>();
      tooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject((object)"Save current drop layout"));
    }
    public void RestoreAs(SavedLanceLayout layout) {
      LanceLoadoutSlot[] loadoutSlots = Traverse.Create(this.parent).Field<LanceLoadoutSlot[]>("loadoutSlots").Value;
      StringBuilder errors = new StringBuilder();
      if (layout.clear) {
        for (int t = 0; t < loadoutSlots.Length; ++t) {
          LanceLoadoutSlot slot = loadoutSlots[t];
          if (slot.isFullLocked) { continue; }
          slot.OnClearSlotsClicked();
        }
        return;
      }
      for (int t = 0; t < loadoutSlots.Length; ++t) {
        if (t >= layout.items.Count) { break; }
        SavedLanceSlot saved = layout.items[t];
        CustomLanceSlot customSlot = loadoutSlots[t].gameObject.GetComponent<CustomLanceSlot>();
        LanceLoadoutSlot slot = loadoutSlots[t];
        IMechLabDraggableItem forcedMech = null;
        SGBarracksRosterSlot forcedPilot = null;
        if (slot.isFullLocked) { continue; }
        if(slot.isMechLocked == false) {
          if(string.IsNullOrEmpty(saved.mech) == false) {
            forcedMech = this.parent.mechListWidget.GetInventoryItem(saved.mech);
            if(forcedMech != null) {
              if (MechValidationRules.ValidateMechCanBeFielded(this.parent.Sim, forcedMech.MechDef) == false) {
                errors.AppendLine(forcedMech.MechDef.Description.Name + " can't be fielded");
                forcedMech = null;
              } 
              if ((customSlot != null)&&(forcedMech != null)) {
                if (forcedMech.MechDef.Chassis.CanBeDropedInto(customSlot.slotDef, out string title, out string message) == false) {
                  errors.AppendLine(message);
                  forcedMech = null;
                }
              }
              float minTonnage = Traverse.Create(slot).Field<float>("minTonnage").Value;
              float maxTonnage = Traverse.Create(slot).Field<float>("maxTonnage").Value;
              if ((forcedMech != null)&&(minTonnage > 0f)) {
                if(forcedMech.MechDef.Chassis.Tonnage < minTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too low tonnage");
                  forcedMech = null;
                }
              }
              if ((forcedMech != null) && (maxTonnage > 0f)) {
                if (forcedMech.MechDef.Chassis.Tonnage > maxTonnage) {
                  errors.AppendLine(forcedMech.MechDef.Description.Name + " too big tonnage");
                  forcedMech = null;
                }
              }
            }
          }
        }
        if (slot.isPilotLocked == false) {
          if (string.IsNullOrEmpty(saved.pilot) == false) {
            forcedPilot = this.parent.pilotListWidget.GetPilot(saved.pilot);
            if (forcedPilot != null) {
              if (forcedPilot.Pilot.CanPilot == false) {
                errors.AppendLine(forcedPilot.Pilot.Callsign + " can't pilot");
                forcedPilot = null;
              }
            }
          }
        }
        if (slot.isPilotLocked) { forcedPilot = slot.SelectedPilot; }
        if (slot.isMechLocked) { forcedMech = slot.SelectedMech; }
        if ((slot.isPilotLocked) && (slot.isMechLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if(forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedMech = null;
          }
        }
        if ((slot.isPilotLocked == false) && (forcedMech != null) && (forcedPilot != null)) {
          if (forcedMech.MechDef.Chassis.CanBePilotedBy(forcedPilot.Pilot.pilotDef, out string title, out string message) == false) {
            errors.AppendLine(message);
            forcedPilot = null;
          }
        }
        if (slot.isPilotLocked) { forcedPilot = null; } else if (forcedPilot != null) {
          this.parent.pilotListWidget.RemovePilot(this.parent.availablePilots.Find((Predicate<Pilot>)(x => x.Description.Id == forcedPilot.Pilot.Description.Id)));
        }
        if (slot.isMechLocked) { forcedMech = null; } else if (forcedMech != null) {
          
        }
        slot.SetLockedData(forcedMech, forcedPilot, false);
      }
      if(errors.Length > 0) {
        GenericPopup popup = GenericPopupBuilder.Create("APPLY LAYOUT WARNINGS", errors.ToString()).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
      }
    }
    public void OnClicked() {
      if (LanceLoadPopupSupervisor.Instance.popup == null) {
        LanceLoadPopupSupervisor.Instance.OnLayoutsButtonClick(parent);
      }
      return;
      GenericPopup popup = null;
      var invList = SaveDropLayoutHelper.LoadAll();
      invList.Sort((a, b) => { return a.name.CompareTo(b.name); });
      SavedLanceLayout clear = new SavedLanceLayout();
      clear.name = "CLEAR";
      clear.clear = true;
      invList.Insert(0, clear);
      if (invList.Count == 0) {
        popup = GenericPopupBuilder.Create("NO SAVED LAYOUTS", "No saved layouts").IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
        return;
      }
      StringBuilder text = new StringBuilder();
      int curIndex = 0;
      int pageSize = 10;
      for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
        if (index >= invList.Count) { break; }
        if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
        text.Append(invList[index].name);
        if (index == curIndex) { text.Append("</size></color>"); };
        text.AppendLine();
      }
      popup = GenericPopupBuilder.Create("AVAILABLE LAYOUTS", text.ToString())
        .AddButton("X", (Action)(() => { }), true)
        .AddButton("->", (Action)(() => {
          try {
            if (curIndex < (invList.Count - 1)) {
              ++curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(invList[index].name);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }), false)
        .AddButton("<-", (Action)(() => {
          try {
            if (curIndex > 0) {
              --curIndex;
              text.Clear();
              for (int index = curIndex - curIndex % pageSize; index < curIndex - curIndex % pageSize + pageSize; ++index) {
                if (index >= invList.Count) { break; }
                if (index == curIndex) { text.Append("<size=150%><color=orange>"); };
                text.Append(invList[index].name);
                if (index == curIndex) { text.Append("</size></color>"); };
                text.AppendLine();
              }
              if (popup != null) popup.TextContent = text.ToString();
            }
          } catch (Exception e) {
            Log.TWL(0, e.ToString(), true);
          }
        }), false).AddButton("APPLY", (Action)(() => {
          this.RestoreAs(invList[curIndex]);
        }), true).IsNestedPopupWithBuiltInFader().SetAlwaysOnTop().Render();
    }
  }
}