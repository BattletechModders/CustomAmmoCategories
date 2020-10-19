using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Localize;
using Text = Localize.Text;
using CustomComponents.ExtendedDetails;
using CustomComponents;
using System.Text;
using CustomAmmoCategoriesPatches;
using HBS;
using TMPro;

namespace CustomAmmoCategoriesPatches {
  public static class CombatHUDWeaponSlotHelper {
    private static MethodInfo m_ShowTextColor = null;
    private static MethodInfo m_CycleWeapon = null;
    public static void ShowTextColor(this CombatHUDWeaponSlot slot, Color color, Color damageColor, Color hitChanceColor, bool allowAmmoWarnings = true) {
      if (m_ShowTextColor == null) {
        m_ShowTextColor = typeof(CombatHUDWeaponSlot).GetMethod("ShowTextColor", BindingFlags.Instance | BindingFlags.NonPublic);
      }
      m_ShowTextColor.Invoke(slot, new object[] { color, damageColor, hitChanceColor, allowAmmoWarnings });
    }
    public static void CycleWeapon(this CombatHUDWeaponSlot slot) {
      if (m_CycleWeapon == null) {
        m_CycleWeapon = typeof(CombatHUDWeaponSlot).GetMethod("CycleWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
      }
      m_CycleWeapon.Invoke(slot, new object[] { });
    }
  }
  public class DebugMouseClickTester: MonoBehaviour {
    void Update() {
      if (Input.GetMouseButtonDown(0)) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000)) {
          Log.O.TWL(0,"MouseClick: "+ hit.transform.gameObject.name);
        }
      }
    }
  }
  public class CombatHUDWeaponPanelEx : MonoBehaviour {
    public CombatHUD HUD { get; set; }
    public RectTransform panelBackground { get; set; }
    public CombatHUDWeaponPanel weaponPanel { get; set; }
    public List<CombatHUDWeaponSlotEx> slotsEx { get; set; }
    public WeaponPanelHeaderAligner header { get; set; }
    private bool ui_inited;
    public bool isUIInted { get { return ui_inited; } }
    public CombatHUDWeaponPanelEx() {
      List<CombatHUDWeaponSlotEx> slots = new List<CombatHUDWeaponSlotEx>();
      weaponPanel = null;
      ui_inited = false;
    }
    public void Refresh() {
      foreach(CombatHUDWeaponSlotEx slotEx in slotsEx) {
        slotEx.Refresh();
      }
      header.ResetUI();
    }
    public bool ExpandWeaponPanel() {
      if (weaponPanel == null) { return false; }
      weaponPanel.gameObject.transform.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelWidthScale, CustomAmmoCategories.Settings.WeaponPanelHeightScale, 1f);
      panelBackground = weaponPanel.transform.FindRecursive("panel_background") as RectTransform;
      if (panelBackground == null) { return false; }
      RectTransform wp_sideLineVert_left = weaponPanel.transform.FindRecursive("wp_sideLineVert (1)") as RectTransform;
      RectTransform wp_sideLineVert_right = weaponPanel.transform.FindRecursive("wp_sideLineVert") as RectTransform;
      if (wp_sideLineVert_left == null) { return false; }
      Log.M.TWL(0, "ExpandWeaponPanel");
      float oldWidth = panelBackground.worldWidth();
      Log.M.WL(1, "old width:" + oldWidth);
      if (oldWidth < CustomAmmoCategories.Epsilon) { return false; }
      panelBackground.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelBackWidthScale, 1f, 1f);
      float newWidth = panelBackground.worldWidth();
      Log.M.WL(1, "new width:" + newWidth);
      Vector3 pos = panelBackground.position;
      pos.x += (newWidth - oldWidth) * 0.5f;
      panelBackground.position = pos;
      Vector3 rightBottom = wp_sideLineVert_left.WorldRightBottom();
      Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      Vector3 desRightBottom = mainUiCamera.ScreenToWorldPoint(new Vector3(mainUiCamera.pixelWidth - 3f, 0f, mainUiCamera.nearClipPlane));
      float xDelta = desRightBottom.x - rightBottom.x;
      CanvasRenderer canvas = weaponPanel.gameObject.GetComponent<CanvasRenderer>();
      Log.M.WL(1, mainUiCamera.pixelWidth.ToString() + "x" + mainUiCamera.pixelHeight + " worldCorner:" + desRightBottom + " delta:" + xDelta);
      pos = weaponPanel.transform.position;
      pos.x += xDelta;
      weaponPanel.transform.position = pos;
      return true;
    }
    public void Init(CombatHUD HUD,CombatHUDWeaponPanel weaponPanel) {
      this.HUD = HUD;
      this.weaponPanel = weaponPanel;
      List<CombatHUDWeaponSlot> WeaponSlots = Traverse.Create(weaponPanel).Field<List<CombatHUDWeaponSlot>>("WeaponSlots").Value;
      slotsEx = new List<CombatHUDWeaponSlotEx>();
      List<CombatHUDWeaponSlot> slots = new List<CombatHUDWeaponSlot>();
      for (int si = 0; si < WeaponSlots.Count;) {
        CombatHUDWeaponSlot slot = WeaponSlots[si];
        if (slot == null) {
          WeaponSlots.RemoveAt(si);
          continue;
        }
        ++si;
        slots.Add(slot);
      }
      slots.Add(Traverse.Create(weaponPanel).Field<CombatHUDWeaponSlot>("dfaSlot").Value);
      slots.Add(Traverse.Create(weaponPanel).Field<CombatHUDWeaponSlot>("meleeSlot").Value);
      foreach (CombatHUDWeaponSlot slot in slots) {
        GameObject go = slot.gameObject;
        CombatHUDWeaponSlotEx slotEx = slot.gameObject.GetComponent<CombatHUDWeaponSlotEx>();
        if (slotEx == null) { slotEx = slot.gameObject.AddComponent<CombatHUDWeaponSlotEx>(); }
        slotEx.Init(this, slot);
        slotsEx.Add(slotEx);
      }
      header = weaponPanel.gameObject.GetComponent<WeaponPanelHeaderAligner>();
      if (header == null) { header = weaponPanel.gameObject.AddComponent<WeaponPanelHeaderAligner>(); }
      header.Init(weaponPanel, slots[0].gameObject.GetComponent<CombatHUDWeaponSlotEx>());
    }
    public void UIInit() {
      if (ExpandWeaponPanel() == false) { return; }
      ui_inited = true;
    }
    public void Update() {
      if (ui_inited) { return; }
      UIInit();
    }
  }
  public class CombatHUDWeaponSlotEx : MonoBehaviour {
    public CombatHUDWeaponPanelEx panel { get; set; }
    public CombatHUDWeaponSlot slot { get; set; }
    public WeaponNameHover nameHover { get; set; }
    public WeaponModeHover modeHover { get; set; }
    public WeaponAmmoHover ammoHover { get; set; }
    public WeaponDamageHover damageHover { get; set; }
    public WeaponAmmoCounterHover counterHover { get; set; }
    public WeaponHitChanceHover hitChanceHover { get; set; }
    public WeaponSwitchButton upButton { get; set; }
    public WeaponSwitchButton downButton { get; set; }
    private bool ui_inited;
    public bool isUIInited { get { return ui_inited; } }
    public CombatHUDWeaponSlotEx() {
      ui_inited = false;
      nameHover = null;
      modeHover = null;
      ammoHover = null;
      damageHover = null;
      counterHover = null;
      hitChanceHover = null;
      upButton = null;
      downButton = null;
    }
    public void RefreshText() {
      if(ammoHover != null) ammoHover.RefreshText();
      if (modeHover != null) modeHover.RefreshText();
    }
    public void RefreshColor(bool parentHovered) {
      if (ammoHover != null) ammoHover.RefreshColor(parentHovered);
      if (modeHover != null) modeHover.RefreshColor(parentHovered);
    }
    public void Refresh() {
      if (modeHover != null) modeHover.Refresh();
      if (ammoHover != null) ammoHover.Refresh();
    }
    public void Init(CombatHUDWeaponPanelEx panel,CombatHUDWeaponSlot slot) {
      this.panel = panel;
      this.slot = slot;
      HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
      HL.spacing = 2.0f;
      if (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Normal) {
        GameObject btnupgo = slot.gameObject.FindRecursive("weapon_button_up");
        if(btnupgo == null) {
          btnupgo = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
          btnupgo.name = "weapon_button_up";
          btnupgo.transform.SetParent(slot.gameObject.transform);
          btnupgo.transform.localScale = Vector3.one;
          btnupgo.transform.SetSiblingIndex(0);
        }
        upButton = btnupgo.GetComponent<WeaponSwitchButton>();
        if (upButton == null) { upButton = btnupgo.AddComponent<WeaponSwitchButton>(); }
        upButton.Init(panel.HUD, panel.weaponPanel, slot, "weapon_up", true);

        GameObject btndowngo = slot.gameObject.FindRecursive("weapon_button_down");
        if (btndowngo == null) {
          btndowngo = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
          btndowngo.gameObject.name = "weapon_button_down";
          btndowngo.transform.SetParent(slot.gameObject.transform);
          btndowngo.transform.localScale = Vector3.one;
          btndowngo.transform.SetSiblingIndex(1);
        }
        downButton = btndowngo.gameObject.GetComponent<WeaponSwitchButton>();
        if (downButton == null) { downButton = btndowngo.gameObject.AddComponent<WeaponSwitchButton>(); }
        downButton.Init(panel.HUD, panel.weaponPanel, slot, "weapon_down", false);
        //RectTransform btnupTr = btnupgo.gameObject.GetComponent<RectTransform>();
        //size = btnupTr.sizeDelta;
        //size.x = upDownButtonsWidth;
        //btnupTr.sizeDelta = size;
        //WeaponSwitchButton btn = btnupgo.GetComponent<WeaponSwitchButton>();
        //if (btn == null) { btn = btnupgo.AddComponent<WeaponSwitchButton>(); }
        //GameObject btndowngo = GameObject.Instantiate(slots[0].ToggleButton.childImage.gameObject);
        //btndowngo.name = "weapon_button_up";
        //btndowngo.transform.SetParent(slot.gameObject.transform);
        //btndowngo.transform.localScale = Vector3.one;
        //btndowngo.transform.SetSiblingIndex(1);
        //RectTransform btndownTr = btndowngo.gameObject.GetComponent<RectTransform>();
        //size = btndownTr.sizeDelta;
        //size.x = upDownButtonsWidth;
        //btndownTr.sizeDelta = size;
        //btn = btndowngo.GetComponent<WeaponSwitchButton>();
        //if (btn == null) { btn = btndowngo.AddComponent<WeaponSwitchButton>(); }
        //btn.Init(HUD, __instance, slot, "weapon_down", false);
        GameObject modText = slot.gameObject.FindRecursive("mode_text");
        if (modText == null) {
          modText = GameObject.Instantiate(slot.DamageText.gameObject);
          modText.name = "mode_text";
          modText.transform.SetParent(slot.transform);
          modText.transform.localScale = Vector3.one;
          modText.transform.SetSiblingIndex(4);
          WeaponDamageHover unusedDmgCover = modText.GetComponent<WeaponDamageHover>();
          if (unusedDmgCover != null) { GameObject.Destroy(unusedDmgCover); unusedDmgCover = null; }
        }
        GameObject modTextBack = slot.gameObject.FindRecursive("mode_text_background");
        if (modTextBack == null) {
          modTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
          modTextBack.name = "mode_text_background";
          modTextBack.transform.SetParent(modText.transform);
          modTextBack.transform.localScale = Vector3.one;
          modTextBack.transform.localPosition = Vector3.zero;
        }
        this.modeHover = modTextBack.GetComponent<WeaponModeHover>();
        if (this.modeHover == null) { this.modeHover = modTextBack.AddComponent<WeaponModeHover>(); }
        this.modeHover.Init(panel.HUD, panel.weaponPanel, slot, this);
        slot.RegisterHover(modeHover);

        GameObject ammoText = slot.gameObject.FindRecursive("ammo_name_text");
        if (ammoText == null) {
          ammoText = GameObject.Instantiate(slot.DamageText.gameObject);
          ammoText.name = "ammo_name_text";
          ammoText.transform.SetParent(slot.transform);
          ammoText.transform.localScale = Vector3.one;
          ammoText.transform.SetSiblingIndex(4);
          WeaponDamageHover unusedDmgCover = modText.GetComponent<WeaponDamageHover>();
          if (unusedDmgCover != null) { GameObject.Destroy(unusedDmgCover); unusedDmgCover = null; }
        }
        GameObject ammoTextBack = slot.gameObject.FindRecursive("ammo_name_text_background");
        if (ammoTextBack == null) {
          ammoTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
          ammoTextBack.name = "ammo_name_text_background";
          ammoTextBack.transform.SetParent(ammoText.transform);
          ammoTextBack.transform.localScale = Vector3.one;
          ammoTextBack.transform.localPosition = Vector3.zero;
        }
        ammoHover = ammoTextBack.GetComponent<WeaponAmmoHover>();
        if (ammoHover == null) { ammoHover = ammoTextBack.AddComponent<WeaponAmmoHover>(); }
        ammoHover.Init(panel.HUD, panel.weaponPanel, slot, this);
        slot.RegisterHover(ammoHover);

        //RectTransform modTextTr = modText.GetComponent<RectTransform>();
        //size = modTextTr.sizeDelta;
        //size.x = (widthDelta * 0.5f) - 10f;
        //modTextTr.sizeDelta = size;
        //modText.GetComponent<LocalizableText>().SetText("MODE");
        //GameObject modTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
        //modTextBack.name = "modTextBack";
        //RectTransform modTextBackTR = modTextBack.GetComponent<RectTransform>();
        //modTextBackTR.SetParent(modTextTr);
        //modTextBackTR.localScale = Vector3.one;
        //modTextBackTR.localPosition = Vector3.zero;
        //modTextBackTR.sizeDelta = size;
        //modTextBack.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
        //WeaponModeHover modeHover = modTextBack.AddComponent<WeaponModeHover>();
        //modeHover.Init(HUD, __instance, slot);
        ////modeHovers.Add(slot, modeHover);
        //slot.RegisterHover(modeHover);

        //GameObject ammoText = GameObject.Instantiate(slot.DamageText.gameObject);
        //ammoCounterHover = ammoText.GetComponent<WeaponDamageHover>();
        //if (ammoCounterHover != null) { GameObject.Destroy(ammoCounterHover); ammoCounterHover = null; }
        //ammoText.name = "ammoName_text";
        //ammoText.transform.SetParent(slot.transform);
        //ammoText.transform.localScale = Vector3.one;
        //RectTransform ammoTextTr = ammoText.GetComponent<RectTransform>();
        //size = ammoTextTr.sizeDelta;
        //size.x = (widthDelta * 0.5f) - 10f;
        //ammoTextTr.sizeDelta = size;
        //ammoTextTr.SetSiblingIndex(5);
        //ammoText.GetComponent<LocalizableText>().SetText("AMMO");
        //GameObject ammoTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
        //ammoTextBack.name = "ammoName_text";
        //RectTransform ammoTextBackTR = ammoTextBack.GetComponent<RectTransform>();
        //ammoTextBackTR.SetParent(ammoTextTr);
        //ammoTextBackTR.localScale = Vector3.one;
        //ammoTextBackTR.localPosition = Vector3.zero;
        //ammoTextBackTR.sizeDelta = size;
        //ammoTextBack.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
        //WeaponAmmoHover ammoHover = ammoTextBack.AddComponent<WeaponAmmoHover>();
        //ammoHover.Init(HUD, __instance, slot);
        //slot.RegisterHover(ammoHover);
        //ammoHovers.Add(slot, ammoHover);
      } else {
        LayoutElement nameLayout = slot.WeaponText.gameObject.GetComponent<LayoutElement>();
        LayoutElement damageLayout = slot.DamageText.gameObject.GetComponent<LayoutElement>();
        LayoutElement buttonLayout = slot.ToggleButton.childImage.gameObject.GetComponent<LayoutElement>();
        if ((nameLayout != null)&&(damageLayout != null)&&(buttonLayout != null)) { nameLayout.preferredWidth += (damageLayout.preferredWidth * 2.0f + buttonLayout.preferredWidth*2.0f + HL.spacing*4.0f); };
      }
      damageHover = slot.DamageText.gameObject.GetComponent<WeaponDamageHover>();
      if (damageHover == null) { damageHover = slot.DamageText.gameObject.AddComponent<WeaponDamageHover>(); }
      damageHover.Init(panel.HUD, panel.weaponPanel, slot);
      nameHover = slot.WeaponText.gameObject.GetComponent<WeaponNameHover>();
      if (nameHover == null) { nameHover = slot.WeaponText.gameObject.AddComponent<WeaponNameHover>(); }
      nameHover.Init(panel.HUD, panel.weaponPanel, slot);
      hitChanceHover = slot.HitChanceText.gameObject.GetComponent<WeaponHitChanceHover>();
      if (hitChanceHover == null) { hitChanceHover = slot.HitChanceText.gameObject.AddComponent<WeaponHitChanceHover>(); }
      hitChanceHover.Init(panel.HUD, panel.weaponPanel, slot);
      counterHover = slot.AmmoText.gameObject.GetComponent<WeaponAmmoCounterHover>();
      if (counterHover == null) { counterHover = slot.AmmoText.gameObject.AddComponent<WeaponAmmoCounterHover>(); }
      counterHover.Init(panel.HUD, panel.weaponPanel, slot);
    }
    public void UIInit() {
      if (panel == null) { return; }
      if (panel.isUIInted == false) { return; }
      if (panel.panelBackground == null) { return; }
      if(slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) {
        if (panel.slotsEx[0].isUIInited == false) { return; }
        if (panel.slotsEx[0].modeHover.ui_inited == false) { return; }
        if (panel.slotsEx[0].ammoHover.ui_inited == false) { return; }
      }
      RectTransform tr = this.gameObject.GetComponent<RectTransform>();
      if (tr == null) { return; }
      float width = tr.worldWidth();
      float destWidth = panel.panelBackground.worldWidth() - 50f;
      Vector2 size = tr.sizeDelta;
      float widthMod = destWidth / width;
      size.x = size.x * (widthMod);
      tr.sizeDelta = size;
      Log.M.TWL(0, "CombatHUDWeaponSlotEx.UIInit " + this.transform.parent.name + " width:" + width + " desWidth:" + destWidth + " new width:"+ tr.worldWidth());
      if (modeHover != null) {
        modeHover.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", panel.HUD.Combat.DataManager);
        this.modeHover.transform.localPosition = Vector3.zero;
        size = this.modeHover.gameObject.GetComponent<RectTransform>().sizeDelta;
        size.y = tr.sizeDelta.y;
        size.x = this.modeHover.gameObject.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        this.modeHover.gameObject.GetComponent<RectTransform>().sizeDelta = size;
      }
      if (ammoHover != null) {
        ammoHover.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", panel.HUD.Combat.DataManager);
        this.ammoHover.transform.localPosition = Vector3.zero;
        size = this.ammoHover.gameObject.GetComponent<RectTransform>().sizeDelta;
        size.y = tr.sizeDelta.y;
        size.x = this.ammoHover.gameObject.transform.parent.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        this.ammoHover.gameObject.GetComponent<RectTransform>().sizeDelta = size;
      }
      if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) {
        HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
        size = slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta;
        size.x = panel.slotsEx[0].upButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].downButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].modeHover.modeText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].ammoHover.ammoText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += (HL.spacing * 4.0f);
        slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        LayoutElement layoutElement = slot.WeaponText.gameObject.GetComponent<LayoutElement>();
        if (layoutElement.preferredWidth * 0.9f < size.x) { ui_inited = true; }
      } else {
        ui_inited = true;
      }
    }
    public void Update() {
      if (ui_inited) { return; }
      UIInit();
    }
  }
  public class WeaponNameHover : EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public UILookAndColorConstants LookAndColorConstants;
    private bool hovered;
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot) {
      this.HUD = HUD;
      parent = slot;
      weaponPanel = panel;
      LookAndColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
      hovered = false;
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.WEAPON_SIDE_HEADER/__");
      ExtendedDetails extDescr = parent.DisplayedWeapon.componentDef.GetComponent<ExtendedDetails>();
      if (extDescr == null) {
        description.Append("<size=80%>" + parent.DisplayedWeapon.Description.Details + "</size>");
        Log.M.WL(1, "no extended description:" + description);
      } else {
        StringBuilder descr = new StringBuilder();
        Log.M.WL(1, "extended description:" + description);
        foreach (ExtendedDetail detail in extDescr.GetDetails()) {
          if (detail.UnitType != UnitType.UNDEFINED) { if (detail.UnitType != this.parent.DisplayedWeapon.parent.UnitType) { continue; } };
          Log.M.WL(2, "detail:" + detail.Identifier + ":" + detail.Text);
          string addtext = new Localize.Text(detail.Text).ToString();
          addtext = addtext.Replace("\n\n", "\n");
          description.Append("<size=80%>" + addtext + "</size>");
        }
        description.Append(descr.ToString());
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      hovered = true;
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      hovered = false;
    }
    public override void OnPointerDown(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      if (data.button != PointerEventData.InputButton.Left || parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.DFA || (parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Melee || parent.DisplayedWeapon.IsDisabled) || !parent.DisplayedWeapon.HasAmmo)
        return;
      parent.MainImage.color = this.LookAndColorConstants.WeaponSlotColors.PressedBGColor;
      parent.ShowTextColor(this.LookAndColorConstants.WeaponSlotColors.PressedTextColor, this.LookAndColorConstants.WeaponSlotColors.PressedTextColor, this.LookAndColorConstants.WeaponSlotColors.PressedTextColor, true);
      parent.ToggleButton.childImage.color = this.LookAndColorConstants.WeaponSlotColors.PressedToggleColor;
    }
    public override void OnPointerUp(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      if (data.button != PointerEventData.InputButton.Left || parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.DFA || parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Melee)
        return;
      if (parent.DisplayedWeapon.IsDisabled || HUD.Combat.StackManager.GetAnyAttackSequence() != null)
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_destroyed);
      else if (this.HUD.SelectionHandler.ActiveState != null && this.HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.FireMulti)
        parent.CycleWeapon();
      else if (!parent.DisplayedWeapon.IsEnabled) {
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_choose_yes);
        parent.DisplayedWeapon.EnableWeapon();
        this.HUD.OnWeaponModified();
      } else {
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_disabled);
        parent.DisplayedWeapon.DisableWeapon();
        this.HUD.OnWeaponModified();
      }
      parent.RefreshDisplayedWeapon((ICombatant)null, new int?(), false, false);
    }
  }
  public class WeaponModeHover : EventTrigger {
    public CombatHUDWeaponSlotEx parentEx;
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public LocalizableText modeText;
    public UILookAndColorConstants LookAndColorConstants;
    public string text;
    private bool hovered;
    private bool parentHovered;
    //public SVGImage background;
    public WeaponModeHover() {
      ui_inited = false;
    }
    public void Refresh() {
      this.RefreshColor(parentHovered);
      this.RefreshText();
      this.ui_inited = false;
    }
    public void RefreshText() {
      if (modeText == null) { return; }
      if (parent.DisplayedWeapon == null) { modeText.SetText("  "); return; }
      ExtWeaponDef extWeapon = parent.DisplayedWeapon.exDef();
      if (extWeapon.Modes.Count > 1) {
        WeaponMode mode = parent.DisplayedWeapon.mode();
        if (string.IsNullOrEmpty(mode.UIName) == false) {
          modeText.SetText(mode.UIName);
        } else {
          modeText.SetText("--");
        }
      } else {
        modeText.SetText("--");
      }      
    }
    public void RefreshColor(bool parentHovered) {
      this.parentHovered = parentHovered;
      if (modeText == null) { return; };
      if (hovered) {
        modeText.color = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.orange;
      } else {
        modeText.color = parentHovered ? this.LookAndColorConstants.WeaponSlotColors.SelectedTextColor : this.LookAndColorConstants.WeaponSlotColors.UnavailableSelTextColor;
      }
    }
    public bool ui_inited;
    public void Update() {
      if (ui_inited) { return; }
      if (parentEx.isUIInited == false) { return; }
      RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      RectTransform slotRt = this.gameObject.transform.parent.parent.gameObject.GetComponent<RectTransform>();
      LayoutElement layoutElement = this.gameObject.transform.parent.gameObject.GetComponent<LayoutElement>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      Vector2 size = selfRt.sizeDelta;
      size.x = parentRt.sizeDelta.x;
      size.y = slotRt.sizeDelta.y;
      selfRt.sizeDelta = size;
      this.transform.localPosition = Vector3.zero;
      Log.M.TWL(0, "WeaponModeHover.initUI "+size.x+"x"+size.y);
      //RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      //RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      //Vector2 size = imgRt.sizeDelta;
      //size.x = selfRt.sizeDelta.x;
      //size.y = parentRt.sizeDelta.y;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xff, 0xff, 0xff,0xff);
      if(layoutElement.preferredWidth*0.8f < size.x) ui_inited = true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot, CombatHUDWeaponSlotEx slotEx) {
      this.HUD = HUD;
      parent = slot;
      parentEx = slotEx;
      weaponPanel = panel;
      modeText = gameObject.transform.parent.gameObject.GetComponent<LocalizableText>();
      LookAndColorConstants = UIManager.Instance.UILookAndColorConstants;
      //background.vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      ui_inited = false;
      RefreshText();
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.MODE_SIZE_HEADER/__");
      List<WeaponMode> modes = this.parent.DisplayedWeapon.AvaibleModes();
      WeaponMode mode = this.parent.DisplayedWeapon.mode();
      foreach(WeaponMode m in modes) {
        if (m == mode) {
          description.Append("<size=150%>");
        } else {
          description.Append("<size=100%>");
        }
        description.Append(m.UIName);
        description.AppendLine("</size>");
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponModeHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      hovered = true;
      RefreshColor(this.parentHovered);
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponModeHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      hovered = false;
      RefreshColor(this.parentHovered);
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("WeaponHitChanceHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      bool prevIndirectState = parent.DisplayedWeapon.IndirectFireCapable();
      if (CustomAmmoCategories.CycleMode(parent.DisplayedWeapon)) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, parent.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.S.TWL(0, "Playing sound by id:" + num2);
        } else {
          Log.S.TWL(0, "Can't play");
        }
      }
      parent.RefreshWeaponCapabilities(prevIndirectState);
      if (hovered) { ShowSidePanel(); }
      RefreshText();
    }
  }
  public class WeaponAmmoHover : EventTrigger {
    public CombatHUDWeaponSlotEx parentEx;
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public LocalizableText ammoText;
    public UILookAndColorConstants LookAndColorConstants;
    public string text;
    private bool hovered;
    private bool parentHovered;
    //public SVGImage background;
    public WeaponAmmoHover() {
      ui_inited = false;
    }
    public void Refresh() {
      this.RefreshColor(parentHovered);
      this.RefreshText();
      this.ui_inited = false;
    }
    public void RefreshText() {
      if (ammoText == null) { return; }
      if (parent.DisplayedWeapon == null) { ammoText.SetText("--"); return; }
      ExtAmmunitionDef ammo = parent.DisplayedWeapon.ammo();
      string ammoBoxName = string.Empty;
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet == false) {
        if (parent.DisplayedWeapon.isWeaponHasAmmoVariants() || (ammo.HideIfOnlyVariant == false)) {
          if (string.IsNullOrEmpty(ammo.UIName) == false) { ammoBoxName = ammo.UIName; } else { ammoBoxName = ammo.Name; };
        }
      }
      if (string.IsNullOrEmpty(ammoBoxName)) {
        ammoText.SetText("--");
      } else {
        ammoText.SetText(ammoBoxName);
      }
    }
    public void RefreshColor(bool parentHovered) {
      this.parentHovered = parentHovered;
      if (ammoText == null) { return; };
      if (hovered) {
        ammoText.color = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.orange;
      } else {
        ammoText.color = parentHovered ? this.LookAndColorConstants.WeaponSlotColors.SelectedTextColor : this.LookAndColorConstants.WeaponSlotColors.UnavailableSelTextColor;
      }
    }
    public bool ui_inited;
    public void Update() {
      if (ui_inited) { return; }
      if (parentEx == null) { return; }
      if (parentEx.isUIInited == false) { return; }
      RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      RectTransform slotRt = this.gameObject.transform.parent.parent.gameObject.GetComponent<RectTransform>();
      LayoutElement layoutElement = this.gameObject.transform.parent.gameObject.GetComponent<LayoutElement>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      Vector2 size = selfRt.sizeDelta;
      size.x = parentRt.sizeDelta.x;
      size.y = slotRt.sizeDelta.y;
      selfRt.sizeDelta = size;
      this.transform.localPosition = Vector3.zero;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xff, 0xff, 0xff,0xff);
      if (layoutElement.preferredWidth * 0.8f < size.x) ui_inited = true;
      ui_inited = true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot, CombatHUDWeaponSlotEx slotEx) {
      this.HUD = HUD;
      this.parentEx = slotEx;
      parent = slot;
      weaponPanel = panel;
      ammoText = gameObject.transform.parent.gameObject.GetComponent<LocalizableText>();
      LookAndColorConstants = UIManager.Instance.UILookAndColorConstants;
      //background.vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      ui_inited = false;
      RefreshText();
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.AMMO_SIDE_HEADER/__");
      CustomAmmoCategory weaponAmmoCategory = this.parent.DisplayedWeapon.CustomAmmoCategory();
      if (weaponAmmoCategory.BaseCategory.Is_NotSet == false) {
        ExtAmmunitionDef ammo = this.parent.DisplayedWeapon.ammo();
        List<ExtAmmunitionDef> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(this.parent.DisplayedWeapon, weaponAmmoCategory);
        foreach (ExtAmmunitionDef ammoDef in AvaibleAmmo) {
          if(ammoDef.Id == ammo.Id) {
            description.Append("<size=150%>");
          } else {
            description.Append("<size=100%>");
          }
          description.Append(CustomAmmoCategories.Settings.AmmoNameInSidePanel?ammoDef.Name:ammoDef.UIName);
          description.AppendLine("</size>");
        }
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponModeHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      hovered = true;
      RefreshColor(this.parentHovered);
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponModeHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      hovered = false;
      RefreshColor(this.parentHovered);
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("WeaponAmmoHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      bool prevIndirectState = parent.DisplayedWeapon.IndirectFireCapable();
      bool modifyers = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
      if (modifyers) {
        ExtWeaponDef extWeapon = parent.DisplayedWeapon.exDef();
        if (extWeapon.EjectWeapon == false) {
          CustomAmmoCategories.EjectAmmo(parent.DisplayedWeapon, parent);
        } else {
          CustomAmmoCategories.EjectWeapon(parent.DisplayedWeapon, parent);
        }
        parent.RefreshWeaponCapabilities(prevIndirectState);
        RefreshText();
        if (hovered) { ShowSidePanel(); }
        return;
      }
      if (CustomAmmoCategories.CycleAmmo(parent.DisplayedWeapon)) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, parent.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.S.TWL(0, "Playing sound by id:" + num2);
        } else {
          Log.S.TWL(0, "Can't play");
        }
      }
      parent.RefreshWeaponCapabilities(prevIndirectState);
      if (hovered) { ShowSidePanel(); }
      RefreshText();
    }
  }
  public class WeaponDamageHover: EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    //public SVGImage background;
    private bool hovered;
    public WeaponDamageHover() {
      ui_inited = false;
    }
    public bool ui_inited;
    public void Update() {
      if (ui_inited) { return; }
      //RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      //RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      //Vector2 size = imgRt.sizeDelta;
      //size.x = selfRt.sizeDelta.x;
      //size.y = parentRt.sizeDelta.y;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xff, 0xff, 0xff, 0xff);
      ui_inited = true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot) {
      this.HUD = HUD;
      parent = slot;
      weaponPanel = panel;
      ui_inited = false;
      //background.vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.DAMAGE_SIZE_HEADER/__");
      if(HUD.SelectedTarget != null) {
        Vector3 position = (parent.DisplayedWeapon.parent.HasMovedThisRound ? null : HUD.SelectionHandler.ActiveState?.PreviewPos) ?? parent.DisplayedWeapon.parent.CurrentPosition;
        CustAmmoCategories.DamageModifiers mods = parent.DisplayedWeapon.GetDamageModifiers(position, HUD.SelectedTarget);
        float damage = parent.DisplayedWeapon.DamagePerShot;
        float ap = parent.DisplayedWeapon.StructureDamagePerShot;
        float heat = parent.DisplayedWeapon.HeatDamagePerShot;
        float stability = parent.DisplayedWeapon.Instability();
        int shots = parent.DisplayedWeapon.ShotsWhenFired;
        string descr = string.Empty;
        mods.Calculate(-1,ref damage, ref ap, ref heat, ref stability, ref descr, false, true);
        description.Append("<size=80%>"+descr+"</size>");
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      hovered = true;
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      hovered = false;
    }
    public override void OnPointerClick(PointerEventData data) {
      if (hovered) { ShowSidePanel(); }
    }
  }
  public class WeaponAmmoCounterHover : EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public UILookAndColorConstants LookAndColorConstants;
    //public SVGImage background;
    //private bool hovered;
    public WeaponAmmoCounterHover() {
      ui_inited = false;
    }
    public bool ui_inited;
    public void Update() {
      if (ui_inited) { return; }
      //RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      //RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      //Vector2 size = imgRt.sizeDelta;
      //size.x = selfRt.sizeDelta.x;
      //size.y = parentRt.sizeDelta.y;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
      ui_inited = true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot) {
      this.HUD = HUD;
      parent = slot;
      weaponPanel = panel;
      LookAndColorConstants = UIManager.Instance.UILookAndColorConstants;
      //background.vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      //hovered = false;
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.AMMO_COUNT_SIDE_HEADER/__");
      AmmunitionBox currentBox = this.parent.DisplayedWeapon.currentAmmoBox();
      List<AmmunitionBox> boxes = this.parent.DisplayedWeapon.SortedAmmoBoxes();
      ExtAmmunitionDef ammo = this.parent.DisplayedWeapon.ammo();
      foreach(AmmunitionBox box in boxes) {
        if (box.ammoDef.Description.Id != ammo.Id) { continue; }
        description.Append(box==currentBox?"<size=100%>":"<size=80%>");
        if(box.IsFunctional == false) {
          description.Append("<color=red>");
        }else
        if(box.CurrentAmmo == 0) {
          description.Append("<color=orange>");
        } else {
          description.Append("<color=white>");
        }
        description.Append(box.Description.UIName);
        if(box.mechComponentRef != null) {
          description.Append("("+Mech.GetAbbreviatedChassisLocation(box.mechComponentRef.MountedLocation)+ ")");
        } else if(box.vehicleComponentRef != null) {
          description.Append("(" + Vehicle.GetLongChassisLocation(box.vehicleComponentRef.MountedLocation) + ")");
        }
        description.Append(" "+box.CurrentAmmo+"/"+box.ammunitionBoxDef.Capacity);
        description.Append("</color>");
        description.Append("</size>\n");
      }
      if(currentBox != null) {
        description.Append("__/CAC.AMMO_COUNT_CURRENT_SIDE_HEADER/__");
        ExtendedDetails extDescr = currentBox.componentDef.GetComponent<ExtendedDetails>();
        if (extDescr == null) {
          description.Append("<size=80%>" + currentBox.Description.Details + "</size>");
          Log.M.WL(1, "no extended description:" + description);
        } else {
          StringBuilder descr = new StringBuilder();
          Log.M.WL(1, "extended description:" + description);
          foreach (ExtendedDetail detail in extDescr.GetDetails()) {
            if (detail.UnitType != UnitType.UNDEFINED) { if (detail.UnitType != currentBox.parent.UnitType) { continue; } };
            Log.M.WL(2, "detail:" + detail.Identifier + ":" + detail.Text);
            string addtext = new Localize.Text(detail.Text).ToString();
            addtext = addtext.Replace("\n\n", "\n");
            description.Append("<size=80%>" + addtext + "</size>");
          }
          description.Append(descr.ToString());
        }
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      ShowSidePanel();
      //hovered = true;
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      //hovered = false;
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
    }
  }
  public class WeaponHitChanceHover : EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    private bool hovered;
    //public SVGImage background;
    public WeaponHitChanceHover() {
      ui_inited = false;
    }
    public bool ui_inited;
    public void Update() {
      if (ui_inited) { return; }
      //RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      //RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      //Vector2 size = imgRt.sizeDelta;
      //size.x = selfRt.sizeDelta.x;
      //size.y = parentRt.sizeDelta.y;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xff, 0xff, 0xff,0xff);
      ui_inited = true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot) {
      this.HUD = HUD;
      parent = slot;
      weaponPanel = panel;
      //background.vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      ui_inited = false;
    }
    public void ShowSidePanel() {
      //Text title = new Text(this.parent.DisplayedWeapon.UIName);
      //Text description = new Text("CLICK TO SWITCH MODE");
      //HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponHitChanceHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      hovered = true;
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponHitChanceHover.OnPointerExit called." + data.position + "\n");
      //HUD.SidePanel.ForceHide();
      hovered = false;
    }
    public override void OnPointerClick(PointerEventData data) {
      //Log.LogWrite("WeaponHitChanceHover.OnPointerClick called." + data.position + "\n");
      //if (this.parent.DisplayedWeapon == null) { return; }
      //if (this.weaponPanel.DisplayedActor == null) { return; }
      //bool prevIndirectState = parent.DisplayedWeapon.IndirectFireCapable();
      //if (CustomAmmoCategories.CycleMode(parent.DisplayedWeapon)) {
      //  if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
      //    uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, parent.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
      //    Log.S.TWL(0, "Playing sound by id:" + num2);
      //  } else {
      //    Log.S.TWL(0, "Can't play");
      //  }
      //}
      //parent.RefreshWeaponCapabilities(prevIndirectState);
      //if (hovered) { ShowSidePanel(); }
    }
  }
  public class WeaponSwitchButton: EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public SVGImage image;
    public string iconid;
    public bool moveUp;
    //public string iconid;
    private bool hovered;
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel,CombatHUDWeaponSlot slot, string iconid, bool moveUp) {
      Log.M.TWL(0, "WeaponSwitchButton.Init");
      try {
        weaponPanel = panel;
        parent = slot;
        this.HUD = HUD;
        this.moveUp = moveUp;
        this.iconid = iconid;
        if (image == null) {
          image = this.gameObject.GetComponent<SVGImage>();
        }
        //icon = CustomSvgCache.get(iconid); //HUD.Combat.DataManager.GetObjectOfType<SVGAsset>(iconid, BattleTechResourceType.SVGAsset);
        if (image != null) {
          image.vectorGraphics = null;
          CustomSvgCache.setIcon(image, iconid, HUD.Combat.DataManager);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
    public void Awake() {
      Log.M.TWL(0, "WeaponSwitchButton.Awake");
      try {
        if (this == null) { return; }
        if (this.gameObject == null) { return; }
        image = this.gameObject.GetComponent<SVGImage>();
        if (this.image != null) {
          //image.vectorGraphics = null;
          CustomSvgCache.setIcon(image, iconid, HUD.Combat.DataManager);
        }
        /*if (icon != null) {
          image.vectorGraphics = icon;
        } else {
          Log.M.WL(1, "icon is null");
        }*/
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponSwitchButton.OnPointerEnter called." + data.position + "\n");
      hovered = true;
    }
    public override void OnPointerExit(PointerEventData data) {
      //SidePanel.ForceHide();
      Log.LogWrite("WeaponSwitchButton.OnPointerExit called." + data.position + "\n");
      hovered = false;
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.LogWrite("WeaponSwitchButton.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      int index = this.weaponPanel.DisplayedWeaponSlots.IndexOf(this.parent);
      if (moveUp) {
        if (this.weaponPanel.DisplayedActor.MoveWeaponUp(this.parent.DisplayedWeapon) == false) { return; }
        if (index <= 0) { return; }
        if (index >= this.weaponPanel.DisplayedWeaponSlots.Count) { return; }        
        Weapon curWeapon = this.parent.DisplayedWeapon;
        Weapon upWeapon = this.weaponPanel.DisplayedWeaponSlots[index - 1].DisplayedWeapon;
        this.parent.DisplayedWeapon = upWeapon;
        this.weaponPanel.DisplayedWeaponSlots[index - 1].DisplayedWeapon = curWeapon;
        this.weaponPanel.RefreshDisplayedWeapons(CombatHUDWeaponPanel_RefreshDisplayedWeapons.last_consideringJump, CombatHUDWeaponPanel_RefreshDisplayedWeapons.last_useCOILPathingPreview);
      } else {
        if (this.weaponPanel.DisplayedActor.MoveWeaponDown(this.parent.DisplayedWeapon) == false) { return; }
        if (index < 0) { return; }
        if (index >= (this.weaponPanel.DisplayedWeaponSlots.Count - 1)) { return; }
        Weapon curWeapon = this.parent.DisplayedWeapon;
        Weapon dwnWeapon = this.weaponPanel.DisplayedWeaponSlots[index + 1].DisplayedWeapon;
        this.parent.DisplayedWeapon = dwnWeapon;
        this.weaponPanel.DisplayedWeaponSlots[index + 1].DisplayedWeapon = curWeapon;
        this.weaponPanel.RefreshDisplayedWeapons(CombatHUDWeaponPanel_RefreshDisplayedWeapons.last_consideringJump, CombatHUDWeaponPanel_RefreshDisplayedWeapons.last_useCOILPathingPreview);
      }
    }

    public void Update() {
      if (image == null) {
        image = this.gameObject.GetComponent<SVGImage>();
        if(image != null) {
          CustomSvgCache.setIcon(image, iconid, HUD.Combat.DataManager);
        }
      }
      if (image == null) { return; }
      if (image.vectorGraphics == null) {
        CustomSvgCache.setIcon(image, iconid, HUD.Combat.DataManager);
      }
      if (hovered) {
        if (image.color != Color.red) { image.color = Color.red; }
      } else {
        if (image.color != Color.white) { image.color = Color.white; }
      }
    }
  }
  public class WeaponPanelHeaderAligner : MonoBehaviour {
    public CombatHUDWeaponSlotEx slot;
    public RectTransform text_WeaponText;
    public RectTransform text_Mode;
    public RectTransform text_AmmoName;
    public RectTransform text_AmmoCount;
    public RectTransform text_Damage;
    public RectTransform text_HitChance;
    private bool ui_inited = false;
    public void ResetUI() {
      ui_inited = false;
    }
    public void Init(CombatHUDWeaponPanel panel,CombatHUDWeaponSlotEx slot) {
      this.slot = slot;
      text_WeaponText = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_WeaponText") as RectTransform;
      text_Mode = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_Mode") as RectTransform;
      text_AmmoName = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_AmmoName") as RectTransform;
      text_AmmoCount = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_Ammo") as RectTransform;
      text_Damage = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_Damage") as RectTransform;
      text_HitChance = panel.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_HitChance") as RectTransform;
      if (text_Mode == null) {
        GameObject text_ModeGO = GameObject.Instantiate(text_AmmoCount.gameObject);
        text_ModeGO.name = "text_Mode";
        text_ModeGO.transform.SetParent(text_AmmoCount.parent);
        text_ModeGO.transform.localScale = Vector3.one;
        text_ModeGO.transform.SetSiblingIndex(1);
        text_ModeGO.GetComponent<LocalizableText>().SetText("__/CAC.MODE_LABEL/__");
        text_ModeGO.transform.localPosition = text_AmmoCount.localPosition;
        text_Mode = text_ModeGO.GetComponent<RectTransform>();
      }
      if (text_AmmoName == null) {
        GameObject text_AmmoNameGO = GameObject.Instantiate(text_AmmoCount.gameObject);
        text_AmmoNameGO.name = "text_AmmoName";
        text_AmmoNameGO.transform.SetParent(text_AmmoCount.parent);
        text_AmmoNameGO.transform.localScale = Vector3.one;
        text_AmmoNameGO.transform.SetSiblingIndex(2);
        text_AmmoNameGO.GetComponent<LocalizableText>().SetText("__/CAC.AMMO_NAME_LABEL/__");
        text_AmmoNameGO.transform.localPosition = text_AmmoCount.localPosition;
        text_AmmoName = text_AmmoNameGO.GetComponent<RectTransform>();
      }
      ui_inited = false;
    }
    public void Update() {
      if (ui_inited) { return; }
      if (slot == null) { return; }
      if (slot.isUIInited == false) { return; }
      Log.M.TWL(0, "WeaponPanelHeaderAligner.Update ui init");
      if (text_WeaponText != null) text_WeaponText.SetPosX(slot.nameHover.transform.position.x);
      if (text_Mode != null) text_Mode.SetPosX(slot.modeHover.transform.position.x);
      if (text_AmmoName != null) text_AmmoName.SetPosX(slot.ammoHover.transform.position.x);
      if (text_AmmoCount != null) text_AmmoCount.SetPosX(slot.counterHover.transform.position.x);
      if (text_Damage != null) text_Damage.SetPosX(slot.damageHover.transform.position.x);
      if (text_HitChance != null) text_HitChance.SetPosX(slot.hitChanceHover.transform.position.x+20f);
      if (text_WeaponText != null) Log.M.WL(1, "text_WeaponText x:"+ text_WeaponText.transform.position.x);
      if (text_Mode != null) Log.M.WL(1, "text_Mode x:" + text_Mode.transform.position.x);
      if (text_AmmoName != null) Log.M.WL(1, "text_AmmoName x:" + text_AmmoName.transform.position.x);
      if (text_AmmoCount != null) Log.M.WL(1, "text_AmmoCount x:" + text_AmmoCount.transform.position.x);
      if (text_Damage != null) Log.M.WL(1, "text_Damage x:" + text_Damage.transform.position.x);
      if (text_HitChance != null) Log.M.WL(1, "text_HitChance x:" + text_HitChance.transform.position.x);
      ui_inited = true;
    }
  }
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("SortSelectedWeapons")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackSequence_SortSelectedWeapons {
    public static void Postfix(AttackDirector.AttackSequence __instance, ref List<List<Weapon>> ___sortedWeapons) {
      Log.M.TWL(0, "AttackSequence.SortSelectedWeapons "+new Localize.Text(__instance.attacker.DisplayName).ToString());
      List<Weapon> uiSorted = __instance.attacker.sortedUIWeapons();
      if (uiSorted == null) { return; };
      Log.M.WL(1, "ui weapons order found:"+uiSorted.Count);
      Dictionary<Weapon, int> uiIndexes = new Dictionary<Weapon, int>();
      for(int i = 0; i < uiSorted.Count; ++i) {
        Log.M.WL(2, "["+i+"]"+uiSorted[i].defId);
        uiIndexes.Add(uiSorted[i], i);
      }
      for(int gid = 0; gid < ___sortedWeapons.Count; ++gid) {
        bool weaponHasNoUISortIndex = false;
        Log.M.WL(1, "original");
        for (int wid = 0; wid < ___sortedWeapons[gid].Count; ++wid) {
          Log.M.WL(2, "weapon["+gid+"][" + wid + "] = " + ___sortedWeapons[gid][wid].defId+" "+(uiIndexes.ContainsKey(___sortedWeapons[gid][wid])? uiIndexes[___sortedWeapons[gid][wid]].ToString():"none"));
          if (uiIndexes.ContainsKey(___sortedWeapons[gid][wid]) == false) {
            weaponHasNoUISortIndex = true;
          }
        }
        if (weaponHasNoUISortIndex) {
          Log.M.WL(1, "weapon without ui sort index detected");
          continue;
        }
        ___sortedWeapons[gid].Sort((Comparison<Weapon>)((x, y) => (uiIndexes[x].CompareTo(uiIndexes[y]))));
        Log.M.WL(1, "reordered");
        for (int wid = 0; wid < ___sortedWeapons[gid].Count; ++wid) {
          Log.M.WL(2, "weapon[" + gid + "][" + wid + "] = " + ___sortedWeapons[gid][wid].defId);
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(GameObject) })]
  public static class CombatHUDWeaponSlot_Init {
    public static void Postfix(CombatHUDWeaponSlot __instance, CombatGameState Combat, CombatHUD HUD, GameObject ToolTipPosition) {
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("ResetSortedWeaponList")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDWeaponPanel_CombatHUDWeaponPanel {
    private static Dictionary<AbstractActor, List<Weapon>> sortedWeaponsByActor = new Dictionary<AbstractActor, List<Weapon>>();
    public static List<Weapon> sortedUIWeapons(this AbstractActor unit) {
      if(sortedWeaponsByActor.TryGetValue(unit,out List<Weapon> result)) {
        return result;
      }
      return null;
    }
    public static void Clear() { sortedWeaponsByActor.Clear(); }
    public static bool MoveWeaponUp(this AbstractActor unit,Weapon weapon) {
      if(sortedWeaponsByActor.TryGetValue(unit,out List<Weapon> sortedList)) {
        int index = sortedList.IndexOf(weapon);
        if (index <= 0) { return false; }
        if (index >= sortedList.Count) { return false; }
        sortedList[index] = sortedList[index - 1];
        sortedList[index - 1] = weapon;
        return true;
      }
      return false;
    }
    public static bool MoveWeaponDown(this AbstractActor unit, Weapon weapon) {
      if (sortedWeaponsByActor.TryGetValue(unit, out List<Weapon> sortedList)) {
        int index = sortedList.IndexOf(weapon);
        if (index < 0) { return false; }
        if (index >= (sortedList.Count-1)) { return false; }
        sortedList[index] = sortedList[index + 1];
        sortedList[index + 1] = weapon;
        return true;
      }
      return false;
    }
    public static bool Prefix(CombatHUDWeaponPanel __instance, ref List<Weapon> ___sortedWeaponsList) {
      try {
        if (__instance.DisplayedActor == null) { return true; }
        if (sortedWeaponsByActor.TryGetValue(__instance.DisplayedActor, out List<Weapon> weapons)) {
          ___sortedWeaponsList.Clear();
          ___sortedWeaponsList.AddRange(weapons);
          return false;
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static void Postfix(CombatHUDWeaponPanel __instance, ref List<Weapon> ___sortedWeaponsList) {
      try {
        if (__instance.DisplayedActor == null) { return; }
        if (sortedWeaponsByActor.ContainsKey(__instance.DisplayedActor) == false) {
          List<Weapon> sorted = new List<Weapon>();
          sorted.AddRange(___sortedWeaponsList);
          sortedWeaponsByActor.Add(__instance.DisplayedActor, sorted);
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon {
    public static void Postfix(CombatHUDWeaponSlot __instance) {
      try {
        __instance.RefreshAdditional();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("Init")]
  [HarmonyPriority(Priority.First)]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDWeaponPanel_Init {
    private static List<Action<CombatHUDWeaponPanel>> additionalInit = new List<Action<CombatHUDWeaponPanel>>();
    public static void AddAdditionalInitCalls(Action<CombatHUDWeaponPanel> onInit) {
      additionalInit.Add(onInit);
    }
    public static void Postfix(CombatHUDWeaponPanel __instance, List<CombatHUDWeaponSlot> ___WeaponSlots, CombatGameState Combat, CombatHUD HUD) {
      try {
        Log.M.TWL(0, "CombatHUDWeaponPanel.Init");
        //return;
        CombatHUDWeaponPanelEx weaponPanelEx = __instance.gameObject.GetComponent<CombatHUDWeaponPanelEx>();
        if (weaponPanelEx == null) { weaponPanelEx = __instance.gameObject.AddComponent<CombatHUDWeaponPanelEx>(); };
        weaponPanelEx.Init(HUD, __instance);
        foreach(var aInit in additionalInit) {
          aInit(__instance);
        }
        //DebugMouseClickTester tester = HUD.gameObject.GetComponent<DebugMouseClickTester>();
        //if (tester == null) { tester = HUD.gameObject.AddComponent<DebugMouseClickTester>(); }
      } catch(Exception e){
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(UIModule))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  public static class UIModule_OnAddedToHierarchy
  {
    public static void Postfix(UIModule __instance)
    {
      try
      {
        //CombatHUD HUD = __instance as CombatHUD;
        //if(HUD == null) { return; }
        //Log.M.TWL(0, "CombatHUD.OnAddedToHierarchy");
        //Transform wp_Slot0 = HUD.gameObject.transform.FindRecursive("wp_Slot0");
        //if(wp_Slot0 != null) { return; }
        //Transform wp_Slot1 = HUD.gameObject.transform.FindRecursive("wp_Slot1");
        //if(wp_Slot1 == null) { return; }
        //wp_Slot0 = GameObject.Instantiate(wp_Slot1).transform;
        //wp_Slot0.gameObject.name = "wp_Slot0";
        //wp_Slot0.SetParent(wp_Slot1.parent);
        //wp_Slot0.localScale = Vector3.one;
        //Log.M.WL(1, "wp_Slot0 instanced succesfully");
        //wp_Slot0.SetSiblingIndex(wp_Slot1.GetSiblingIndex());
      } catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("RefreshDisplayedWeapons")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
  public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons {
    public static void SetPosX(this Transform tr, float x) { Vector3 pos = tr.position; pos.x = x; tr.position = pos; }
    public static bool last_consideringJump = false;
    public static bool last_useCOILPathingPreview = false;
    private static PropertyInfo p_HUD = typeof(CombatHUDWeaponPanel).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic);
    private static Dictionary<CombatHUDWeaponSlot, WeaponModeHover> modeHovers = new Dictionary<CombatHUDWeaponSlot, WeaponModeHover>();
    private static Dictionary<CombatHUDWeaponSlot, WeaponAmmoHover> ammoHovers = new Dictionary<CombatHUDWeaponSlot, WeaponAmmoHover>();
    public static void Clear() {
      modeHovers.Clear();
      ammoHovers.Clear();
    }
    public static void RefreshAdditional(this CombatHUDWeaponSlot slot) {
      CombatHUDWeaponSlotEx slotEx = slot.gameObject.GetComponent<CombatHUDWeaponSlotEx>();
      if (slotEx != null) { slotEx.RefreshText(); }
    }
    public static void RefreshAdditionalColors(this CombatHUDWeaponSlot slot, bool selected) {
      CombatHUDWeaponSlotEx slotEx = slot.gameObject.GetComponent<CombatHUDWeaponSlotEx>();
      if (slotEx != null) { slotEx.RefreshColor(selected); }
    }
    public static void RegisterHover(this CombatHUDWeaponSlot slot, WeaponModeHover hover) {
      if (modeHovers.ContainsKey(slot)) { modeHovers[slot] = hover; } else { modeHovers.Add(slot, hover); }
    }
    public static void RegisterHover(this CombatHUDWeaponSlot slot, WeaponAmmoHover hover) {
      if (ammoHovers.ContainsKey(slot)) { ammoHovers[slot] = hover; } else { ammoHovers.Add(slot, hover); }
    }
    public static CombatHUD HUD(this CombatHUDWeaponPanel panel) { return (CombatHUD)p_HUD.GetValue(panel); }
    private static int panelInstance = -1;
    public static Transform FindRecursive(this Transform transform, string checkName) {
      foreach (Transform t in transform) {
        if (t.name == checkName) return t;
        Transform possibleTransform = FindRecursive(t, checkName);
        if (possibleTransform != null) return possibleTransform;
      }
      return null;
    }
    public static GameObject FindRecursive(this GameObject gameObject, string checkName) {
      foreach (Transform t in gameObject.transform) {
        if (t.name == checkName) return t.gameObject;
        GameObject possibleTransform = FindRecursive(t.gameObject, checkName);
        if (possibleTransform != null) return possibleTransform;
      }
      return null;
    }
    private static FieldInfo f_WeaponSlots = typeof(CombatHUDWeaponPanel).GetField("WeaponSlots", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<CombatHUDWeaponSlot> WeaponSlots(this CombatHUDWeaponPanel panel) { return (List<CombatHUDWeaponSlot>)f_WeaponSlots.GetValue(panel); }
    public static Vector3 WorldRightBottom(this RectTransform rectTransform) {
      Vector3[] corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);
      return corners[3];
    }
    public static Vector3 WorldLeftTop(this RectTransform rectTransform) {
      Vector3[] corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);
      return corners[1];
    }
    public static float worldWidth(this RectTransform rectTransform) {
      Vector3[] corners = new Vector3[4];
      rectTransform.GetWorldCorners(corners);
      return corners[3].x - corners[0].x;
    }
    public static void Postfix(CombatHUDWeaponPanel __instance, List<CombatHUDWeaponSlot> ___WeaponSlots, bool consideringJump, bool useCOILPathingPreview, CombatHUDWeaponSlot ___meleeSlot, CombatHUDWeaponSlot ___dfaSlot) {
      //if (CustomAmmoCategories.Settings.ShowWeaponOrderButtons == false) { return; }
      //return;
      last_consideringJump = consideringJump;
      last_useCOILPathingPreview = useCOILPathingPreview;
      CombatHUDWeaponPanelEx panelEx = __instance.gameObject.GetComponent<CombatHUDWeaponPanelEx>();
      if (panelEx != null) { panelEx.Refresh(); }
      //if (__instance.gameObject.GetInstanceID() == panelInstance) {
      //  WeaponPanelHeaderAligner ha = __instance.gameObject.GetComponent<WeaponPanelHeaderAligner>();
      //  if (ha != null) { ha.ResetUI(); }
      //  return;
      //}
      //Log.M.TWL(0, "CombatHUDWeaponPanel.RefreshDisplayedWeapons " + __instance.gameObject.GetInstanceID());
      //panelInstance = __instance.gameObject.GetInstanceID();
      //if (ExpandWeaponPanel(__instance) == false) { panelInstance = -1; return; }
      //CombatHUD HUD = __instance.HUD();
      //float upDownButtonsWidth = 0f;
      ////float upDownButtonsPadding = 0f;
      //float newSlotWidth = 0f;
      //float widthDelta = 0f;
      //for(int si = 0; si < ___WeaponSlots.Count;) {
      //  CombatHUDWeaponSlot slot = ___WeaponSlots[si];
      //  if(slot == null) {
      //    ___WeaponSlots.RemoveAt(si);
      //    continue;
      //  }
      //  ++si;
      //  RectTransform slotTr = slot.gameObject.GetComponent<RectTransform>();
      //  if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { continue; }
      //  if (upDownButtonsWidth == 0f) { upDownButtonsWidth = slotTr.sizeDelta.y * CustomAmmoCategories.Settings.OrderButtonWidthScale; }
      //  //if (upDownButtonsPadding == 0f) { upDownButtonsPadding = slotTr.sizeDelta.y * CustomAmmoCategories.Settings.OrderButtonWidthScale * CustomAmmoCategories.Settings.OrderButtonPaddingScale; }
      //  if((upDownButtonsWidth > 0f)) {
      //    newSlotWidth = slotTr.sizeDelta.x * (CustomAmmoCategories.Settings.WeaponPanelBackWidthScale+0.05f);
      //    widthDelta = newSlotWidth - slotTr.sizeDelta.x;
      //    break;
      //  }
      //}
      //RectTransform text_WeaponText = __instance.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_WeaponText") as RectTransform;
      //RectTransform text_Ammo = __instance.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_Ammo") as RectTransform;
      //RectTransform text_Damage = __instance.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_Damage") as RectTransform;
      //RectTransform text_HitChance = __instance.gameObject.transform.FindRecursive("wp_Labels").FindRecursive("text_HitChance") as RectTransform;
      //GameObject text_Mode = GameObject.Instantiate(text_Ammo.gameObject);
      //text_Mode.name = "text_Mode";
      //text_Mode.transform.SetParent(text_Ammo.parent);
      //text_Mode.transform.localScale = Vector3.one;
      //text_Mode.transform.SetSiblingIndex(1);
      //text_Mode.GetComponent<LocalizableText>().SetText("__/CAC.MODE_LABEL/__");
      //text_Mode.transform.localPosition = text_Ammo.localPosition;
      //GameObject text_AmmoName = GameObject.Instantiate(text_Ammo.gameObject);
      //text_AmmoName.name = "text_AmmoName";
      //text_AmmoName.transform.SetParent(text_Ammo.parent);
      //text_AmmoName.transform.localScale = Vector3.one;
      //text_AmmoName.transform.SetSiblingIndex(2);
      //text_AmmoName.GetComponent<LocalizableText>().SetText("__/CAC.AMMO_NAME_LABEL/__");
      //text_AmmoName.transform.localPosition = text_Ammo.localPosition;
      //text_Ammo.gameObject.GetComponent<LocalizableText>().SetText("__/CAC.AMMO_COUNT_LABEL/__");
      //List<CombatHUDWeaponSlot> slots = new List<CombatHUDWeaponSlot>();
      //for(int si = 0; si < ___WeaponSlots.Count;) {
      //  CombatHUDWeaponSlot slot = ___WeaponSlots[si];
      //  if(slot == null) {
      //    ___WeaponSlots.RemoveAt(si);
      //    continue;
      //  }
      //  ++si;
      //  slots.Add(slot);
      //}
      //{
      //  slots.Add(___dfaSlot);
      //}
      //{
      //  slots.Add(___meleeSlot);
      //}
      //foreach (CombatHUDWeaponSlot slot in slots) {
      //  GameObject go = slot.gameObject;
      //  RectTransform slotTr = slot.gameObject.GetComponent<RectTransform>();
      //  if (slotTr == null) { continue; };
      //  HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
      //  HL.spacing = 5.0f;
      //  Vector2 size = slotTr.sizeDelta;
      //  size.x = newSlotWidth;
      //  slotTr.sizeDelta = size;
      //  if (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Normal) {
      //    GameObject btnupgo = GameObject.Instantiate(slots[0].ToggleButton.childImage.gameObject);
      //    btnupgo.name = "weapon_button_up";
      //    btnupgo.transform.SetParent(slot.gameObject.transform);
      //    btnupgo.transform.localScale = Vector3.one;
      //    btnupgo.transform.SetSiblingIndex(0);
      //    RectTransform btnupTr = btnupgo.gameObject.GetComponent<RectTransform>();
      //    size = btnupTr.sizeDelta;
      //    size.x = upDownButtonsWidth;
      //    btnupTr.sizeDelta = size;
      //    WeaponSwitchButton btn = btnupgo.GetComponent<WeaponSwitchButton>();
      //    if (btn == null) { btn = btnupgo.AddComponent<WeaponSwitchButton>(); }
      //    btn.Init(HUD, __instance, slot, "weapon_up", true);
      //    GameObject btndowngo = GameObject.Instantiate(slots[0].ToggleButton.childImage.gameObject);
      //    btndowngo.name = "weapon_button_up";
      //    btndowngo.transform.SetParent(slot.gameObject.transform);
      //    btndowngo.transform.localScale = Vector3.one;
      //    btndowngo.transform.SetSiblingIndex(1);
      //    RectTransform btndownTr = btndowngo.gameObject.GetComponent<RectTransform>();
      //    size = btndownTr.sizeDelta;
      //    size.x = upDownButtonsWidth;
      //    btndownTr.sizeDelta = size;
      //    btn = btndowngo.GetComponent<WeaponSwitchButton>();
      //    if (btn == null) { btn = btndowngo.AddComponent<WeaponSwitchButton>(); }
      //    btn.Init(HUD, __instance, slot, "weapon_down", false);

      //    GameObject modText = GameObject.Instantiate(slot.DamageText.gameObject);
      //    WeaponDamageHover ammoCounterHover = modText.GetComponent<WeaponDamageHover>();
      //    if (ammoCounterHover != null) { GameObject.Destroy(ammoCounterHover); ammoCounterHover = null; }
      //    modText.name = "mode_text";
      //    modText.transform.SetParent(slot.transform);
      //    modText.transform.localScale = Vector3.one;
      //    RectTransform modTextTr = modText.GetComponent<RectTransform>();
      //    size = modTextTr.sizeDelta;
      //    size.x = (widthDelta * 0.5f) - 10f;
      //    modTextTr.sizeDelta = size;
      //    modTextTr.SetSiblingIndex(4);
      //    modText.GetComponent<LocalizableText>().SetText("MODE");
      //    GameObject modTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
      //    modTextBack.name = "modTextBack";
      //    RectTransform modTextBackTR = modTextBack.GetComponent<RectTransform>();
      //    modTextBackTR.SetParent(modTextTr);
      //    modTextBackTR.localScale = Vector3.one;
      //    modTextBackTR.localPosition = Vector3.zero;
      //    modTextBackTR.sizeDelta = size;
      //    modTextBack.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      //    WeaponModeHover modeHover = modTextBack.AddComponent<WeaponModeHover>();
      //    modeHover.Init(HUD, __instance, slot);
      //    //modeHovers.Add(slot, modeHover);
      //    slot.RegisterHover(modeHover);

      //    GameObject ammoText = GameObject.Instantiate(slot.DamageText.gameObject);
      //    ammoCounterHover = ammoText.GetComponent<WeaponDamageHover>();
      //    if (ammoCounterHover != null) { GameObject.Destroy(ammoCounterHover); ammoCounterHover = null; }
      //    ammoText.name = "ammoName_text";
      //    ammoText.transform.SetParent(slot.transform);
      //    ammoText.transform.localScale = Vector3.one;
      //    RectTransform ammoTextTr = ammoText.GetComponent<RectTransform>();
      //    size = ammoTextTr.sizeDelta;
      //    size.x = (widthDelta * 0.5f) - 10f;
      //    ammoTextTr.sizeDelta = size;
      //    ammoTextTr.SetSiblingIndex(5);
      //    ammoText.GetComponent<LocalizableText>().SetText("AMMO");
      //    GameObject ammoTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
      //    ammoTextBack.name = "ammoName_text";
      //    RectTransform ammoTextBackTR = ammoTextBack.GetComponent<RectTransform>();
      //    ammoTextBackTR.SetParent(ammoTextTr);
      //    ammoTextBackTR.localScale = Vector3.one;
      //    ammoTextBackTR.localPosition = Vector3.zero;
      //    ammoTextBackTR.sizeDelta = size;
      //    ammoTextBack.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get("weapon_btn", HUD.Combat.DataManager);
      //    WeaponAmmoHover ammoHover = ammoTextBack.AddComponent<WeaponAmmoHover>();
      //    ammoHover.Init(HUD, __instance, slot);
      //    slot.RegisterHover(ammoHover);
      //    //ammoHovers.Add(slot, ammoHover);
      //  } else {
      //    RectTransform nameTransform = slot.WeaponText.gameObject.GetComponent<RectTransform>();
      //    size = nameTransform.sizeDelta;
      //    size.x += (widthDelta-20f);
      //    nameTransform.sizeDelta = size;
      //  }
      //}
    }
  }
}