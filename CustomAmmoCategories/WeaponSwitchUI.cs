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

namespace CustomAmmoCategoriesPathes {
  public class WeaponSwitchButton: EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public SVGImage image;
    public SVGAsset icon;
    public bool moveUp;
    //public string iconid;
    private bool hovered;
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel,CombatHUDWeaponSlot slot, string iconid, bool moveUp) {
      weaponPanel = panel;
      parent = slot;
      this.HUD = HUD;
      this.moveUp = moveUp;
      if (image == null) { this.gameObject.GetComponent<SVGImage>(); }
      Log.M.TWL(0, "WeaponSwitchButton.Init");
      icon = HUD.Combat.DataManager.GetObjectOfType<SVGAsset>(iconid, BattleTechResourceType.SVGAsset);
      if (icon != null) {
        Log.M.WL(1, "icon " + iconid + " found");
        if(image != null) image.vectorGraphics = icon;
      } else {
        Log.M.WL(1, "icon " + iconid + " not found");
      }
    }
    public void Awake() {
      Log.M.TWL(0, "WeaponSwitchButton.Awake");
      image = this.gameObject.GetComponent<SVGImage>();
      if(icon != null) {
        image.vectorGraphics = icon;
      } else {
        Log.M.WL(1, "icon is null");
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
      if (image == null) { return; }
      if (hovered) {
        if (image.color != Color.red) { image.color = Color.red; }
      } else {
        if (image.color != Color.white) { image.color = Color.white; }
      }
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
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("RefreshDisplayedWeapons")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
  public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons {
    public static bool last_consideringJump = false;
    public static bool last_useCOILPathingPreview = false;
    private static PropertyInfo p_HUD = typeof(CombatHUDWeaponPanel).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic);
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
    private static FieldInfo f_WeaponSlots = typeof(CombatHUDWeaponPanel).GetField("WeaponSlots", BindingFlags.Instance | BindingFlags.NonPublic);
    public static List<CombatHUDWeaponSlot> WeaponSlots(this CombatHUDWeaponPanel panel) { return (List<CombatHUDWeaponSlot>)f_WeaponSlots.GetValue(panel); }
    public static void Postfix(CombatHUDWeaponPanel __instance, List<CombatHUDWeaponSlot> ___WeaponSlots, bool consideringJump, bool useCOILPathingPreview) {
      if (CustomAmmoCategories.Settings.ShowWeaponOrderButtons == false) { return; }
      last_consideringJump = consideringJump;
      last_useCOILPathingPreview = useCOILPathingPreview;
      if (__instance.gameObject.GetInstanceID() == panelInstance) { return; }
      Log.M.TWL(0, "CombatHUDWeaponPanel.RefreshDisplayedWeapons " + __instance.gameObject.GetInstanceID());
      panelInstance = __instance.gameObject.GetInstanceID();
      __instance.gameObject.transform.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelWidthScale, CustomAmmoCategories.Settings.WeaponPanelHeightScale, 1f);
      RectTransform panel_background = __instance.transform.FindRecursive("panel_background") as RectTransform;
      if (panel_background == null) { return; }
      Vector3[] corners = new Vector3[4];
      panel_background.GetWorldCorners(corners);
      float width = corners[3].x - corners[0].x;
      Log.M.WL(1, "width:" + width);
      if (width == 0f) { panelInstance = -1; return; }
      panel_background.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelBackWidthScale, 1f, 1f);
      panel_background.GetWorldCorners(corners);
      float newWidth = corners[3].x - corners[0].x;
      Log.M.WL(1, "newWidth:" + newWidth);
      Vector3 pos = panel_background.position;
      pos.x += (newWidth - width) * 0.5f;
      panel_background.position = pos;
      pos = __instance.transform.position;
      pos.x -= (newWidth - width) * 0.5f;
      __instance.transform.position = pos;
      CombatHUD HUD = __instance.HUD();
      foreach (CombatHUDWeaponSlot slot in ___WeaponSlots) {
        GameObject go = slot.gameObject;
        RectTransform slotTr = slot.gameObject.GetComponent<RectTransform>();
        if (slotTr == null) { continue; };
        if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { continue; }
        GameObject btnupgo = GameObject.Instantiate(go);
        HashSet<GameObject> childs = new HashSet<GameObject>();
        for(int i = 0; i < btnupgo.transform.childCount; ++i) {
          childs.Add(btnupgo.transform.GetChild(i).gameObject);
        }
        CombatHUDWeaponSlot nslot = btnupgo.GetComponent<CombatHUDWeaponSlot>();
        if (nslot != null) { GameObject.Destroy(nslot); };
        CombatHUDTooltipHoverElement hover = btnupgo.GetComponent<CombatHUDTooltipHoverElement>();
        if (hover != null) { GameObject.Destroy(hover); };
        SVGToggleButton svgbtn = btnupgo.GetComponent<SVGToggleButton>();
        if (svgbtn != null) { GameObject.Destroy(svgbtn); };
        Button ubtn = btnupgo.GetComponent<Button>();
        if (ubtn != null) { GameObject.Destroy(ubtn); };

        foreach (GameObject dstro in childs) { GameObject.Destroy(dstro); };
        btnupgo.transform.SetParent(slot.transform.parent);
        btnupgo.transform.localScale = Vector3.one;
        RectTransform btnupTr = btnupgo.gameObject.GetComponent<RectTransform>();
        Vector2 size = btnupTr.sizeDelta;
        size.x = size.y * CustomAmmoCategories.Settings.OrderButtonWidthScale;
        btnupTr.sizeDelta = size;
        btnupgo.transform.localPosition = new Vector3(slotTr.sizeDelta.x + size.x * CustomAmmoCategories.Settings.OrderButtonPaddingScale, 0f, 0f);
        WeaponSwitchButton btn = btnupgo.GetComponent<WeaponSwitchButton>();
        if (btn == null) { btn = btnupgo.AddComponent<WeaponSwitchButton>(); }
        btn.Init(HUD, __instance, slot, "weapon_up",true);
        GameObject btndwngo = GameObject.Instantiate(go);
        childs.Clear();
        for (int i = 0; i < btndwngo.transform.childCount; ++i) {
          childs.Add(btndwngo.transform.GetChild(i).gameObject);
        }
        nslot = btndwngo.GetComponent<CombatHUDWeaponSlot>();
        if (nslot != null) { GameObject.Destroy(nslot); };
        hover = btndwngo.GetComponent<CombatHUDTooltipHoverElement>();
        if (hover != null) { GameObject.Destroy(hover); };
        svgbtn = btndwngo.GetComponent<SVGToggleButton>();
        if (svgbtn != null) { GameObject.Destroy(svgbtn); };
        ubtn = btndwngo.GetComponent<Button>();
        if (ubtn != null) { GameObject.Destroy(ubtn); };
        foreach (GameObject dstro in childs) { GameObject.Destroy(dstro); };
        btndwngo.transform.SetParent(slot.transform.parent);
        btndwngo.transform.localScale = Vector3.one;
        RectTransform btndwnTr = btndwngo.gameObject.GetComponent<RectTransform>();
        size = btndwnTr.sizeDelta;
        size.x = size.y * CustomAmmoCategories.Settings.OrderButtonWidthScale; ;
        btndwngo.transform.localPosition = new Vector3(slotTr.sizeDelta.x + btnupTr.sizeDelta.x + 2.0f * size.x * CustomAmmoCategories.Settings.OrderButtonPaddingScale, 0f, 0f);
        btndwnTr.sizeDelta = size;
        btn = btndwngo.GetComponent<WeaponSwitchButton>();
        if (btn == null) { btn = btndwngo.AddComponent<WeaponSwitchButton>(); }
        btn.Init(HUD, __instance, slot, "weapon_down",false);
      }
      //Log.M.printComponents(__instance.gameObject,1);
    }
  }
}