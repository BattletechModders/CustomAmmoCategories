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
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Threading;
using BattleTech.Data;
using CustomSettings;
using Log = CustomAmmoCategoriesLog.Log;
using InControl;
using System.Reflection.Emit;
using IRBTModUtils;
using BattleTech.UI.Tooltips;
using BattleTech.Framework;
using System.Linq;
using Harmony;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUDObjectiveItem))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Localize.Text), typeof(bool), typeof(bool) })]
  public static class CombatHUDObjectiveItem_Init0 {
    public static void Postfix(CombatHUDObjectiveItem __instance, Localize.Text title, bool isPrimary, bool useOnPointer) {
      Image background = __instance.GetComponent<Image>();
      if(background != null)background.color = new Color(0f, 0f, 0f, 0f);
    }
  }
  [HarmonyPatch(typeof(CombatHUDObjectiveItem))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(CombatHUDObjectivesList), typeof(CombatHUDObjectiveStatusNotify), typeof(ObjectiveGameLogic) })]
  public static class CombatHUDObjectiveItem_Init1 {
    public static void Postfix(CombatHUDObjectiveItem __instance, CombatGameState combat, CombatHUD HUD, CombatHUDObjectivesList objectivesList, CombatHUDObjectiveStatusNotify notify, ObjectiveGameLogic objective) {
      Image background = __instance.GetComponent<Image>();
      if (background != null) background.color = new Color(0f, 0f, 0f, 0f);
    }
  }
  [HarmonyPatch(typeof(CombatHUDObjectiveItem))]
  [HarmonyPatch("OnPointerEnter")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDObjectiveItem_OnPointerEnter {
    public static void Postfix(CombatHUDObjectiveItem __instance) {
      if (CustomAmmoCategories.Settings.ObjectiveBlackBackgroundOnEnter == false) { return; }
      Image background = __instance.GetComponent<Image>();
      if (background != null) {
        background.color = new Color(0f, 0f, 0f, 1f);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDObjectiveItem))]
  [HarmonyPatch("OnPointerExit")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDObjectiveItem_OnPointerExit {
    public static void Postfix(CombatHUDObjectiveItem __instance) {
      if (CustomAmmoCategories.Settings.ObjectiveBlackBackgroundOnEnter == false) { return; }
      Image background = __instance.GetComponent<Image>();
      if (background != null) {
        background.color = new Color(0f, 0f, 0f, 0f);
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_Refresh {
    public static void Postfix(MechDef __instance) {
      if (DataManager_PooledInstantiate_CombatHUD.desiredWeapons < __instance.Inventory.Length) { DataManager_PooledInstantiate_CombatHUD.desiredWeapons = __instance.Inventory.Length; }
    }
  }
  [HarmonyPatch(typeof(VehicleDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleDef_Refresh {
    public static void Postfix(VehicleDef __instance) {
      int inventory_length = Traverse.Create(__instance).Field<VehicleComponentRef[]>("inventory").Value.Length;
      if (DataManager_PooledInstantiate_CombatHUD.desiredWeapons < inventory_length) { DataManager_PooledInstantiate_CombatHUD.desiredWeapons = inventory_length; }
    }
  }
  [HarmonyPatch(typeof(TurretDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class TurretDef_Refresh {
    public static void Postfix(TurretDef __instance) {
      int inventory_length = Traverse.Create(__instance).Field<TurretComponentRef[]>("inventory").Value.Length;
      if (DataManager_PooledInstantiate_CombatHUD.desiredWeapons < inventory_length) { DataManager_PooledInstantiate_CombatHUD.desiredWeapons = inventory_length; }
    }
  }
  [HarmonyPatch(typeof(WeaponRangeIndicators))]
  [HarmonyPatch("SetAllWeaponsArc")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<Weapon>) })]
  public static class WeaponRangeIndicators_SetAllWeaponsArc_Debug {
    //private static bool calloriginal = false;
    private static Vector4 GetRangesVector(this WeaponRangeIndicators __instance, float minRange,float shortRange,float medRange,float maxRange) {
      return Traverse.Create(__instance).Method("GetRangesVector", new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }, new object[] { minRange, shortRange, medRange, maxRange }).GetValue<Vector4>();
    }
    private static void setArcState(this WeaponRangeIndicators __instance, WeaponRangeIndicators.FiringArcState newState) {
      Traverse.Create(__instance).Method("setArcState", new Type[] { typeof(WeaponRangeIndicators.FiringArcState) }, new object[] { newState }).GetValue();
    }
    public static bool Prefix(WeaponRangeIndicators __instance,ref List<Weapon> weapons,ref int[] ___MultWeaponShaderVectorInts,ref int[] ___MultWeaponShaderStrengthInts,ref int ___MultiWeaponNumberInt) {
      //if (calloriginal) { return true; }
      //calloriginal = true;
      try {
        Log.M.TWL(0, "SetAllWeaponsArc " + weapons.Count);
        weapons.Sort((a, b) => {
          try {
            float damage_b = 0f;
            try { damage_b = b.DamagePerShot * (float)b.ShotsWhenFired; } catch (Exception e) {
              Log.M.TWL(0, "damage_b fail " + e.ToString(), true);
            }
            float damage_a = 0f;
            try { damage_a = a.DamagePerShot * (float)a.ShotsWhenFired; } catch (Exception e) {
              Log.M.TWL(0, "damage_a fail " + e.ToString(), true);
            }
            return damage_b.CompareTo(damage_a);
          } catch(Exception e) {
            Log.M.TWL(0,e.ToString(), true);
          }
          return 0;
        });
        int index1 = 0;
        List<Vector4> vector4List = new List<Vector4>();
        for (int index2 = 0; index2 < weapons.Count; ++index2) {
          if (index1 >= 7) {
            Debug.Log((object)"Note - trying to add more than 7 weapon arcs. Only showing 7 highest DamagePerShot weapons.");
            break;
          }
          Weapon weapon = weapons[index2];
          if (weapon.IsEnabled && weapon.WillFire && !weapon.Name.Contains("Melee")) {
            Vector4 ranges = __instance.GetRangesVector(weapon.MinRange, weapon.ShortRange, weapon.MediumRange, weapon.MaxRange);
            if (!vector4List.Find((Predicate<Vector4>)(x => x.Equals(ranges))).Equals(ranges)) {
              vector4List.Add(ranges);
              Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetVector(___MultWeaponShaderVectorInts[index1], ranges);
              Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetFloat(___MultWeaponShaderStrengthInts[index1], 1f);
              //__instance.allWeaponsMat.SetFloat(__instance.MultWeaponShaderStrengthInts[index1], 1f);
              ++index1;
            }
          }
        }
        if (index1 == 0) {
          Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetVector(___MultWeaponShaderVectorInts[0], __instance.GetRangesVector(0.0f, 0.0f, 0.0f, 270f));
          Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetFloat(___MultWeaponShaderStrengthInts[0], 1f);
          index1 = 1;
        }
        Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetFloat(___MultiWeaponNumberInt, (float)index1);
        for (int index2 = index1; index2 < 7; ++index2) {
          Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetVector(___MultWeaponShaderVectorInts[index2], Vector4.zero);
          Traverse.Create(__instance).Property<Material>("allWeaponsMat").Value.SetFloat(___MultWeaponShaderStrengthInts[index2], 0.0f);
        }
        __instance.setArcState(WeaponRangeIndicators.FiringArcState.AllWeapons);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      //calloriginal = false;
      return false;
    }
  }
  //[HarmonyPatch(typeof(ActorMovementSequence))]
  //[HarmonyPatch("Update")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class ActorMovementSequence_Update_Debug {
  //  private static bool originalcall = false;
  //  public static bool Prefix(ActorMovementSequence __instance, float ___distanceMovedThisFrame) {
  //    if (originalcall) { return true; }
  //    originalcall = true;
  //    try {
  //      __instance.Update();
  //      Transform MoverTransform = Traverse.Create(__instance).Property<Transform>("MoverTransform").Value;
  //      MovementCapabilitiesDef Capabilities = Traverse.Create(__instance).Property<MovementCapabilitiesDef>("Capabilities").Value;
  //      //Log.M.TWL(0, "ActorMovementSequence.Update " + MoverTransform.name+" pos:"+ MoverTransform.position+ " Velocity:" + __instance.Velocity+" distFrame:"+ ___distanceMovedThisFrame+" capabilities:"+Capabilities.Description.Id+" sprint:"+ Capabilities.SprintVelocity+ " MaxVelAdjusted:"+__instance.MaxVelAdjusted);
  //    } catch (Exception e) {
  //      Log.M.TWL(0, e.ToString(), true);
  //    }
  //    originalcall = false;
  //    return false;
  //  }
  //}
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("OnAddToStack")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMove_OnAddToStack_Debug {
    public static bool originalcall = false;
    public delegate void d_OnAddToStack(SelectionStateMove instance);
    private static d_OnAddToStack i_OnAddToStack = null;
    public static bool Prefix(SelectionStateMove __instance) {
      try {
        if (originalcall) { return true; }
        if(i_OnAddToStack == null) {
          MethodInfo method = typeof(SelectionStateMove).GetMethod("OnAddToStack", BindingFlags.Public | BindingFlags.Instance);
          var dm = new DynamicMethod("CACOnAddToStack", null, new Type[] { typeof(SelectionStateMove) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_OnAddToStack = (d_OnAddToStack)dm.CreateDelegate(typeof(d_OnAddToStack));
        }
        originalcall = true;
        Log.M.TWL(0, "SelectionStateMove.OnAddToStack");
        i_OnAddToStack(__instance);
        originalcall = false;
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(CameraControl))]
  [HarmonyPatch("UpdatePlayerControl")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CameraControl_UpdatePlayerControl {
    public static bool AllowZoom = true;
    private static PlayerOneAxisAction FakeAxis = null;
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(InputManagerExtended), "Combat_CameraZoom");
      var replacementMethod = AccessTools.Method(typeof(CameraControl_UpdatePlayerControl), "Combat_CameraZoom");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    public static void Prefix(ref bool __state) {
      try {
        if (FakeAxis == null) {
          FakeAxis = (PlayerOneAxisAction)typeof(PlayerOneAxisAction).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(PlayerAction), typeof(PlayerAction) }, null).Invoke(new object[] { BTInput.Instance.StaticActions.None, BTInput.Instance.StaticActions.None });
        }
      }catch(Exception e) {
        Log.M.TWL(0,e.ToString(),true);
      }
    }
    public static PlayerOneAxisAction Combat_CameraZoom(BTInput btInput) {
      if ((AllowZoom == false)&&(FakeAxis != null)) {
        return FakeAxis;
      }
      return btInput.StaticActions.MouseScroll.WasPressed ? btInput.StaticActions.MouseScroll : btInput.DynamicActions.CameraZoom;      //Log.LogWrite(0, "MechDef.GatherDependencies postfix " + __instance.Description.Id + " " + activeRequestWeight, true);
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("ProcessInput")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatSelectionHandler_ProcessInput {
    public static bool AllowRightButton = true;
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Method(typeof(InputManagerExtended), "Mouse_RightButton");
      var replacementMethod = AccessTools.Method(typeof(CombatSelectionHandler_ProcessInput), "Mouse_RightButton");
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    public static PlayerAction Mouse_RightButton(BTInput btInput) {
      return AllowRightButton?btInput.Mouse_RightButton():btInput.Key_None();
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("ShowWeaponsUpTo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class CombatHUDWeaponPanel_ShowWeaponsUpTo {
    public static void Postfix(CombatHUDWeaponPanel __instance, int topIndex) {
      if (topIndex <= 0) { return; }
      CombatHUDWeaponPanelScroller scroller = __instance.gameObject.GetComponentInParent<CombatHUDWeaponPanelScroller>();
      if (scroller != null) {
        scroller.weaponPannelScrollRect.verticalNormalizedPosition = 1f;
        scroller.beenScrolled = false;
      }
    }
  }
  public class CombatHUDWeaponSlotsScrollControl: MonoBehaviour {
    public RectTransform weaponPanelRect { get; set; }
    public CombatHUDWeaponPanel weaponPanel { get; set; }
    public RectTransform main { get; set; }
    public RectTransform scrollerRect { get; set; }
    public RectTransform scrollBarRect { get; set; }
    public ScrollRect scroller { get; set; }
    public CanvasGroup Canvas { get; set; } = null;
    public void Awake() {
      this.Canvas = this.gameObject.GetComponent<CanvasGroup>();
      if (this.Canvas == null) { Canvas = this.gameObject.AddComponent<CanvasGroup>(); }
      Canvas.alpha = 1f;
      Canvas.blocksRaycasts = true;
    }
    public float lastSize = 0f;
    public void Update() {
      if (weaponPanelRect == null) { return; }
      if (weaponPanel == null) { return; }
      if (main == null) { return; }
      if (scrollerRect == null) { return; }
      if (scrollBarRect == null) { return; }
      if (scroller == null) { return; }
      if (weaponPanelRect.gameObject.activeSelf != scrollerRect.gameObject.activeSelf) {
        scrollerRect.gameObject.SetActive(weaponPanelRect.gameObject.activeSelf);
      }
      if (this.Canvas.alpha != this.weaponPanel.Canvas.alpha) {
        this.Canvas.alpha = this.weaponPanel.Canvas.alpha;
      }
      if (this.Canvas.blocksRaycasts != this.weaponPanel.Canvas.blocksRaycasts) {
        this.Canvas.blocksRaycasts = this.weaponPanel.Canvas.blocksRaycasts;
      }
      float y = weaponPanelRect.sizeDelta.y - 200;
      if (y > 200f) { y = 200f; }
      scrollerRect.sizeDelta = new Vector2(weaponPanelRect.sizeDelta.x, y);
      this.scrollerRect.gameObject.SetActive(this.weaponPanelRect.gameObject.activeSelf);
      Vector3 localPos = scrollBarRect.localPosition;
      localPos.x = weaponPanelRect.sizeDelta.x + 60f;
      scrollBarRect.localPosition = localPos;
      if(Mathf.Abs(lastSize - weaponPanelRect.sizeDelta.y) > 1.0f) {
        lastSize = weaponPanelRect.sizeDelta.y;
        scroller.verticalNormalizedPosition = 1f;
      }      
    }
  }
  public class CombatHUDWeaponPanelScroller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler {
    public ScrollRect weaponPannelScrollRect { get; set; } = null;
    public RectTransform slotTransform { get; set; } = null;
    public bool hovered { get; set; } = false;
    public bool beenScrolled { get; set; } = false;
    public Image panel_background { get; set; } = null;
    public void Awake() {
      if (panel_background == null) { panel_background = this.gameObject.FindObject<Image>("panel_background"); }
    }
    public void OnPointerEnter(PointerEventData eventData) {
      Log.M.TWL(0, "CombatHUDWeaponPanelScroller.OnPointerEnter");
      CameraControl_UpdatePlayerControl.AllowZoom = false;
      CombatSelectionHandler_ProcessInput.AllowRightButton = false;
      //if (panel_background != null) { panel_background.color = Color.white; }
      this.hovered = true;
    }
    public void OnPointerExit(PointerEventData eventData) {
      Log.M.TWL(0, "CombatHUDWeaponPanelScroller.OnPointerExit");
      CameraControl_UpdatePlayerControl.AllowZoom = true;
      CombatSelectionHandler_ProcessInput.AllowRightButton = true;
      //InputManagerExtended_Mouse_RightButton.AllowPress = true;
      //if (panel_background != null) { panel_background.color = Color.black; }
      this.hovered = false;
    }
    public void LateUpdate() {
      //if (beenScrolled == false) {
      //  weaponPannelScrollRect.verticalNormalizedPosition = 1f;
      //}
      //Log.M.TWL(0, "CombatHUDWeaponPanelScroller.weaponPannelScrollRect.verticalNormalizedPosition:"+ weaponPannelScrollRect.verticalNormalizedPosition);
    }
    public void OnScroll(PointerEventData eventData) {
      if (hovered == false) { return; }
      if (weaponPannelScrollRect == null) { return; }
      if (slotTransform == null) { return; }
      if (weaponPannelScrollRect.verticalScrollbar.gameObject.activeSelf == false) { return; }
      this.beenScrolled = true;
      float delta = eventData.scrollDelta.y * (slotTransform.sizeDelta.y / weaponPannelScrollRect.content.sizeDelta.y);
      weaponPannelScrollRect.verticalNormalizedPosition += delta;
      weaponPannelScrollRect.verticalNormalizedPosition = Mathf.Clamp(weaponPannelScrollRect.verticalNormalizedPosition, 0f, 1f);
      //weaponPannelScrollRect.verticalNormalizedPosition += 
      Log.M.TWL(0, "CombatHUDWeaponPanelScroller.OnScroll "+eventData.scrollDelta+" normPos:"+ weaponPannelScrollRect.verticalNormalizedPosition);
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PooledInstantiate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(BattleTechResourceType), typeof(Vector3?), typeof(Quaternion?), typeof(Transform) })]
  public static class DataManager_PooledInstantiate_CombatHUD {
    public static int desiredWeapons = 14;
    public static void Postfix(DataManager __instance, string id, BattleTechResourceType resourceType, ref GameObject __result) {
      try {
        //return;
        if (resourceType != BattleTechResourceType.UIModulePrefabs) { return; }
        if (__result == null) { return; }
        if (id != "uixPrfPanl_HUD") { return; }
        Log.M.TWL(0, "DataManager.PooledInstantiate " + id);
        CombatHUD HUD = __result.GetComponent<CombatHUD>();
        CombatHUDWeaponPanel weaponPanel = __result.GetComponentInChildren<CombatHUDWeaponPanel>(true);
        CombatHUDWeaponSlotsScrollControl scroller = weaponPanel.gameObject.GetComponentInChildren<CombatHUDWeaponSlotsScrollControl>(true);
        if (scroller == null) {
          GameObject uixPrfPanl_STG_gameModule = __instance.PooledInstantiate("uixPrfPanl_STG_gameModule", BattleTechResourceType.UIModulePrefabs);
          GameSettingsModule module = uixPrfPanl_STG_gameModule.GetComponent<GameSettingsModule>();
          GameObject.DestroyImmediate(module);
          uixPrfPanl_STG_gameModule.transform.SetParent(weaponPanel.transform.parent);
          uixPrfPanl_STG_gameModule.transform.localPosition = weaponPanel.transform.localPosition;
          uixPrfPanl_STG_gameModule.transform.localScale = Vector3.one;
          uixPrfPanl_STG_gameModule.transform.localRotation = Quaternion.identity;

          RectTransform scrollRect = uixPrfPanl_STG_gameModule.transform as RectTransform;
          RectTransform weaponsRect = weaponPanel.transform as RectTransform;
          scrollRect.anchoredPosition = weaponsRect.anchoredPosition;
          scrollRect.sizeDelta = new Vector2(weaponsRect.sizeDelta.x * CustomAmmoCategories.Settings.WeaponPanelWidthScale, weaponsRect.sizeDelta.y);
          scrollRect.pivot = weaponsRect.pivot;
          scrollRect.anchorMax = new Vector2(1f, 0f);
          scrollRect.anchorMin = new Vector2(0.5f, 0.5f);
          scrollRect.anchoredPosition = new Vector2(0f, -240f);
          scrollRect.pivot = Vector2.one;
          //scrollRect.localPosition = new Vector3(0f, -240f, 0f);
          VerticalLayoutGroup Content = uixPrfPanl_STG_gameModule.FindObject<VerticalLayoutGroup>("Content");
          if (Content != null) { GameObject.DestroyImmediate(Content.gameObject); }
          Image viewport = uixPrfPanl_STG_gameModule.FindObject<Image>("Viewport");
          if (viewport != null) { viewport.enabled = false; }
          ScrollRect gameplay_scroll = uixPrfPanl_STG_gameModule.FindObject<ScrollRect>("gameplay_scroll");
          RectTransform gameplay_scroll_TR = gameplay_scroll.transform as RectTransform;
          RectTransform scrollBar = gameplay_scroll.verticalScrollbar.transform as RectTransform;
          scrollBar.sizeDelta = new Vector2(0f, -50f);
          scrollBar.pivot = new Vector2(1f, 0f);
          scrollBar.anchoredPosition = Vector2.zero;
          gameplay_scroll_TR.anchoredPosition = Vector2.zero;
          gameplay_scroll_TR.pivot = new Vector2(-0.2f, 0f);
          gameplay_scroll_TR.anchoredPosition = Vector2.zero;
          gameplay_scroll_TR.anchorMax = new Vector3(0.5f, 0f);
          gameplay_scroll_TR.anchorMin = new Vector3(0f, 0.5f);
          //(gameplay_scroll.transform as RectTransform).anchoredPosition = Vector2.zero;
          gameplay_scroll.content = weaponsRect;
          weaponPanel.transform.SetParent(gameplay_scroll.viewport.transform);
          weaponPanel.transform.localPosition = Vector3.zero;
          weaponPanel.transform.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelWidthScale, CustomAmmoCategories.Settings.WeaponPanelHeightScale, 1f); ;
          weaponPanel.transform.localRotation = Quaternion.identity;
          weaponsRect.anchoredPosition = Vector2.zero;
          weaponsRect.pivot = new Vector2(CustomAmmoCategories.Settings.WeaponPanelBackWidthScale, 0f);
          scroller = uixPrfPanl_STG_gameModule.AddComponent<CombatHUDWeaponSlotsScrollControl>();
          scroller.main = scrollRect;
          scroller.weaponPanelRect = weaponsRect;
          scroller.weaponPanel = weaponPanel;
          scroller.scrollerRect = gameplay_scroll_TR;
          scroller.scroller = gameplay_scroll;
          scroller.scrollBarRect = gameplay_scroll.verticalScrollbar.transform as RectTransform;
          gameplay_scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
          Log.M.WL(1, "weaponPanel.transform.localScale:" + weaponPanel.transform.localScale + " WeaponPanelWidthScale:" + CustomAmmoCategories.Settings.WeaponPanelWidthScale);
          CombatHUDWeaponPanelScroller zoomDisabler = weaponPanel.gameObject.GetComponent<CombatHUDWeaponPanelScroller>();
          if (zoomDisabler == null) { zoomDisabler = weaponPanel.gameObject.AddComponent<CombatHUDWeaponPanelScroller>(); }
          zoomDisabler.weaponPannelScrollRect = gameplay_scroll;
          CombatHUDWeaponSlot[] zoomSlots = weaponPanel.gameObject.GetComponentsInChildren<CombatHUDWeaponSlot>(true);
          zoomDisabler.slotTransform = zoomSlots[0].gameObject.transform.parent as RectTransform;
        }
        CombatHUDWeaponSlot[] slotsArr = weaponPanel.gameObject.GetComponentsInChildren<CombatHUDWeaponSlot>(true);
        Log.M.WL(1, "desiredCount:"+ desiredWeapons);
        List<CombatHUDWeaponSlot> slots = new List<CombatHUDWeaponSlot>();
        foreach (CombatHUDWeaponSlot slot in slotsArr) { if (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Normal) { slots.Add(slot); }; };
        for(int t=slots.Count; t < desiredWeapons; ++t) {
          GameObject slotGO = GameObject.Instantiate(slots[0].gameObject.transform.parent.gameObject);
          slotGO.name = "wp_Slot" + (t + 1);
          slotGO.transform.SetParent(slots[0].gameObject.transform.parent.gameObject.transform.parent);
          slotGO.transform.localScale = slots[0].transform.parent.localScale;
          slotGO.transform.localPosition = slots[0].transform.parent.localPosition;
          slotGO.transform.localRotation = slots[0].transform.parent.localRotation;
          slotGO.transform.SetSiblingIndex(slots[slots.Count - 1].transform.parent.GetSiblingIndex() + (t - slots.Count) + 1);
          Log.M.WL(1, slotGO.name+ " SiblingIndex:"+ slotGO.transform.GetSiblingIndex());
        }
      } catch (Exception e) {
        Log.M.TWL(0,e.ToString(), true);
      }
    }
  }
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
  public class DebugMouseClickTester : MonoBehaviour {
    void Update() {
      if (Input.GetMouseButtonDown(0)) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000)) {
          Log.O.TWL(0, "MouseClick: " + hit.transform.gameObject.name);
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
      foreach (CombatHUDWeaponSlotEx slotEx in slotsEx) {
        slotEx.Refresh();
      }
      header.ResetUI();
    }
    public bool ExpandWeaponPanel() {
      if (weaponPanel == null) { return false; }
      weaponPanel.gameObject.transform.localScale = new Vector3(CustomAmmoCategories.Settings.WeaponPanelWidthScale, CustomAmmoCategories.Settings.WeaponPanelHeightScale, 1f);
      Log.M.TWL(0, "weaponPanel.transform.localScale:" + weaponPanel.gameObject.transform.localScale);
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
      //Vector3 rightBottom = wp_sideLineVert_left.WorldRightBottom();
      //Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      //Vector3 desRightBottom = mainUiCamera.ScreenToWorldPoint(new Vector3(mainUiCamera.pixelWidth - 3f, 0f, mainUiCamera.nearClipPlane));
      //float xDelta = desRightBottom.x - rightBottom.x;
      //CanvasRenderer canvas = weaponPanel.gameObject.GetComponent<CanvasRenderer>();
      //Log.M.WL(1, mainUiCamera.pixelWidth.ToString() + "x" + mainUiCamera.pixelHeight + " worldCorner:" + desRightBottom + " delta:" + xDelta);
      //pos = weaponPanel.transform.position;
      //pos.x += xDelta;
      //weaponPanel.transform.position = pos;
      return true;
    }
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel weaponPanel) {
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
      if (ammoHover != null) ammoHover.RefreshText();
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
    public void Init(CombatHUDWeaponPanelEx panel, CombatHUDWeaponSlot slot) {
      this.panel = panel;
      this.slot = slot;
      HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
      HL.spacing = 2.0f;
      //if (slot.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Normal) {
      GameObject btnupgo = slot.gameObject.FindRecursive("weapon_button_up");
      if (btnupgo == null) {
        btnupgo = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
        btnupgo.name = "weapon_button_up";
        btnupgo.transform.SetParent(slot.gameObject.transform);
        btnupgo.transform.localScale = Vector3.one;
        btnupgo.transform.SetSiblingIndex(0);
        btnupgo.SetActive(true);
      }
      if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { btnupgo.SetActive(false); }

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
        btndowngo.SetActive(true);
      }
      if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { btndowngo.SetActive(false); }

      downButton = btndowngo.gameObject.GetComponent<WeaponSwitchButton>();
      if (downButton == null) { downButton = btndowngo.gameObject.AddComponent<WeaponSwitchButton>(); }
      downButton.Init(panel.HUD, panel.weaponPanel, slot, "weapon_down", false);
      GameObject modText = slot.gameObject.FindRecursive("mode_text");
      if (modText == null) {
        modText = GameObject.Instantiate(slot.DamageText.gameObject);
        modText.name = "mode_text";
        modText.transform.SetParent(slot.transform);
        modText.transform.localScale = Vector3.one;
        modText.transform.SetSiblingIndex(slot.WeaponText.gameObject.transform.GetSiblingIndex() + 1);
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
        modTextBack.SetActive(true);
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
        ammoText.transform.SetSiblingIndex(slot.AmmoText.gameObject.transform.GetSiblingIndex() - 1);
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
        ammoTextBack.SetActive(true);
      }
      ammoHover = ammoTextBack.GetComponent<WeaponAmmoHover>();
      if (ammoHover == null) { ammoHover = ammoTextBack.AddComponent<WeaponAmmoHover>(); }
      ammoHover.Init(panel.HUD, panel.weaponPanel, slot, this);
      slot.RegisterHover(ammoHover);
      if (slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) { ammoText.SetActive(false); }
      //} else {
      //  LayoutElement nameLayout = slot.WeaponText.gameObject.GetComponent<LayoutElement>();
      //  LayoutElement damageLayout = slot.DamageText.gameObject.GetComponent<LayoutElement>();
      //  LayoutElement buttonLayout = slot.ToggleButton.childImage.gameObject.GetComponent<LayoutElement>();
      //  GameObject modText = slot.gameObject.FindRecursive("mode_text");
      //  if (modText == null) {
      //    modText = GameObject.Instantiate(slot.DamageText.gameObject);
      //    modText.name = "mode_text";
      //    modText.transform.SetParent(slot.transform);
      //    modText.transform.localScale = Vector3.one;
      //    modText.transform.SetSiblingIndex(4);
      //    WeaponDamageHover unusedDmgCover = modText.GetComponent<WeaponDamageHover>();
      //    if (unusedDmgCover != null) { GameObject.Destroy(unusedDmgCover); unusedDmgCover = null; }
      //  }
      //  GameObject modTextBack = slot.gameObject.FindRecursive("mode_text_background");
      //  if (modTextBack == null) {
      //    modTextBack = GameObject.Instantiate(slot.ToggleButton.childImage.gameObject);
      //    modTextBack.name = "mode_text_background";
      //    modTextBack.transform.SetParent(modText.transform);
      //    modTextBack.transform.localScale = Vector3.one;
      //    modTextBack.transform.localPosition = Vector3.zero;
      //  }
      //  this.modeHover = modTextBack.GetComponent<WeaponModeHover>();
      //  if (this.modeHover == null) { this.modeHover = modTextBack.AddComponent<WeaponModeHover>(); }
      //  this.modeHover.Init(panel.HUD, panel.weaponPanel, slot, this);
      //  slot.RegisterHover(modeHover);
      //  if ((nameLayout != null)&&(damageLayout != null)&&(buttonLayout != null)) {
      //    nameLayout.preferredWidth += (buttonLayout.preferredWidth*2.0f + HL.spacing*3.0f);
      //  };
      //}
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
      if(slot.weaponSlotType != CombatHUDWeaponSlot.WeaponSlotType.Normal) {
        var sizeDelta = slot.HitChanceText.rectTransform.sizeDelta;
        if (sizeDelta.y < 20f) { sizeDelta.y = 20f; };
        slot.HitChanceText.rectTransform.sizeDelta = sizeDelta;
      }
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
      Log.M.TWL(0, "CombatHUDWeaponSlotEx.UIInit " + this.transform.parent.name + " width:" + width + " desWidth:" + destWidth + " new width:" + tr.worldWidth());
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
        size = panel.slotsEx[0].slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta;
        slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        size = panel.slotsEx[0].ammoHover.gameObject.GetComponent<RectTransform>().sizeDelta;
        this.ammoHover.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        size = panel.slotsEx[0].modeHover.gameObject.GetComponent<RectTransform>().sizeDelta;
        this.modeHover.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
        size = slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta;
        size.x = panel.slotsEx[0].upButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].downButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += panel.slotsEx[0].modeHover.modeText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
        size.x += (HL.spacing * 3.0f);
        slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
        LayoutElement layoutElement = slot.WeaponText.gameObject.GetComponent<LayoutElement>();
        if (layoutElement.preferredWidth * 0.9f < size.x) { ui_inited = true; }
      } else {
        ui_inited = true;
      }
      //  HorizontalLayoutGroup HL = slot.gameObject.GetComponent<HorizontalLayoutGroup>();
      //  size = slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta;
      //  size.x = panel.slotsEx[0].upButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
      //  size.x += panel.slotsEx[0].downButton.gameObject.GetComponent<RectTransform>().sizeDelta.x;
      //  size.x += panel.slotsEx[0].slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
      //  size.x += panel.slotsEx[0].modeHover.modeText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
      //  size.x += panel.slotsEx[0].ammoHover.ammoText.gameObject.GetComponent<RectTransform>().sizeDelta.x;
      //  size.x += (HL.spacing * 4.0f);
      //  slot.WeaponText.gameObject.GetComponent<RectTransform>().sizeDelta = size;
      //  LayoutElement layoutElement = slot.WeaponText.gameObject.GetComponent<LayoutElement>();
      //  if (layoutElement.preferredWidth * 0.9f < size.x) { ui_inited = true; }
      //} else {
      //  ui_inited = true;
      //}
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
    //private bool hovered;
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot) {
      this.HUD = HUD;
      parent = slot;
      weaponPanel = panel;
      LookAndColorConstants = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants;
      //hovered = false;
    }
    public void ShowSidePanel() {
      Text title = new Text(this.parent.DisplayedWeapon.UIName);
      Text description = new Text("__/CAC.WEAPON_SIDE_HEADER/__");
      ExtendedDetails extDescr = parent.DisplayedWeapon.componentDef.GetComponent<ExtendedDetails>();
      if (parent.DisplayedWeapon.CanFire) {
        description.AppendLine("OPERATIONAL");
      } else {
        description.AppendLine(parent.DisplayedWeapon.CantFireReason());
      }
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
      if (CustomAmmoCategories.Settings.ShowJammChance) {
        this.parent.DisplayedWeapon.FlatJammingChance(out string jammDescr);
        description.Append("\n" + jammDescr);
      }
      HUD.SidePanel.ForceShowPersistant(title, description, null, false);
    }
    public override void OnPointerEnter(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerEnter called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      //hovered = true;
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponDamageHover.OnPointerExit called." + data.position + "\n");
      HUD.SidePanel.ForceHide();
      //hovered = false;
    }
    public override void OnPointerDown(PointerEventData data) {
      if (data.button != PointerEventData.InputButton.Left) { return; }
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
      if (data.button != PointerEventData.InputButton.Left) { return; }
      Log.LogWrite("WeaponDamageHover.OnPointerClick called." + data.position + "\n");
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      if (data.button != PointerEventData.InputButton.Left || parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.DFA || parent.weaponSlotType == CombatHUDWeaponSlot.WeaponSlotType.Melee) {
        return;
      }
      bool isShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift));
      if (parent.DisplayedWeapon.IsDisabled || HUD.Combat.StackManager.GetAnyAttackSequence() != null) {
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_destroyed);
      } else if (this.HUD.SelectionHandler.ActiveState != null && this.HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.FireMulti) {
        parent.CycleWeapon();
      } else if (!parent.DisplayedWeapon.IsEnabled) {
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_choose_yes);
        parent.DisplayedWeapon.EnableWeapon();
        if (isShift) {
          foreach(Weapon weapon in parent.DisplayedWeapon.parent.Weapons) {
            if (weapon == parent.DisplayedWeapon) { continue; }
            if (weapon.defId != parent.DisplayedWeapon.defId) { continue; }
            weapon.EnableWeapon();
          }
        }
        this.HUD.OnWeaponModified();
      } else {
        this.HUD.GenerateButtonEvent("WeaponSlot", AudioEventList_ui.ui_weapon_disabled);
        parent.DisplayedWeapon.DisableWeapon();
        if (isShift) {
          foreach (Weapon weapon in parent.DisplayedWeapon.parent.Weapons) {
            if (weapon == parent.DisplayedWeapon) { continue; }
            if (weapon.defId != parent.DisplayedWeapon.defId) { continue; }
            weapon.DisableWeapon();
          }
        }
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
      WeaponExtendedInfo info = parent.DisplayedWeapon.info();
      if (info.modes.Count > 1) {
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
      //Log.M.TWL(0, "WeaponModeHover.initUI " + size.x + "x" + size.y);
      //RectTransform selfRt = this.gameObject.GetComponent<RectTransform>();
      //RectTransform parentRt = this.gameObject.transform.parent.gameObject.GetComponent<RectTransform>();
      //RectTransform imgRt = this.background.gameObject.GetComponent<RectTransform>();
      //Vector2 size = imgRt.sizeDelta;
      //size.x = selfRt.sizeDelta.x;
      //size.y = parentRt.sizeDelta.y;
      //imgRt.sizeDelta = size;
      //imgRt.localPosition = Vector3.zero;
      //background.color = new Color32(0xff, 0xff, 0xff,0xff);
      if (layoutElement.preferredWidth * 0.8f < size.x) ui_inited = true;
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
      if ((ASWatchdog.instance != null)&&(ASWatchdog.instance.isAnySequenceTracked())) {
        description.Append("\n<color=red>Attack in progress mode change forbidden</color>");
      }
      List<WeaponMode> modes = this.parent.DisplayedWeapon.AvaibleModes();
      WeaponMode mode = this.parent.DisplayedWeapon.mode();
      description.AppendLine("\n" + mode.Description);
      foreach (WeaponMode m in modes) {
        if (m == mode) {
          description.Append("<size=150%>");
        } else {
          description.Append("<size=100%>");
        }
        description.Append(m.Name);
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
      Log.M.TWL(0, "WeaponModeHover.OnPointerExit called." + data.position);
      HUD.SidePanel.ForceHide();
      hovered = false;
      RefreshColor(this.parentHovered);
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.M.TWL(0,"WeaponHitChanceHover.OnPointerClick called." + data.position, true);
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      if ((ASWatchdog.instance != null) && (ASWatchdog.instance.isAnySequenceTracked())) {
        Log.M.WL(1, "Attack sequence is active");
        ASWatchdog.instance.logTrackedSequences();
        return;
      }
      try {
        bool isShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift));
        bool prevIndirectState = parent.DisplayedWeapon.IndirectFireCapable();
        if (CustomAmmoCategories.CycleMode(parent.DisplayedWeapon, data.button == PointerEventData.InputButton.Left)) {
          if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
            uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, parent.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
            Log.S.TWL(0, "Playing sound by id:" + num2);
          } else {
            Log.S.TWL(0, "Can't play");
          }
          if (isShift) {
            foreach (CombatHUDWeaponSlot slot in this.weaponPanel.DisplayedWeaponSlots) {
              if (slot.DisplayedWeapon == parent.DisplayedWeapon) { continue; }
              if (slot.DisplayedWeapon.defId != parent.DisplayedWeapon.defId) { continue; }
              bool indState = slot.DisplayedWeapon.IndirectFireCapable();
              slot.DisplayedWeapon.SyncMode(parent.DisplayedWeapon);
              slot.RefreshWeaponCapabilities(indState);
            }
          }
        }
        parent.RefreshWeaponCapabilities(prevIndirectState);
        if (hovered) { ShowSidePanel(); }
        RefreshText();
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
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
      try {
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
      }catch(Exception e) {
        Log.M.TWL(0, "parent.DisplayedWeapon:"+(parent.DisplayedWeapon == null?"null": parent.DisplayedWeapon.defId));
        if(parent.DisplayedWeapon != null) {
          ExtAmmunitionDef ammo = parent.DisplayedWeapon.ammo();
          Log.M.WL(1, "ammo:" + (ammo == null?"null":ammo.Id));
        }
        Log.M.TWL(0, e.ToString(), true);
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
      if ((ASWatchdog.instance != null) && (ASWatchdog.instance.isAnySequenceTracked())) {
        description.Append("\n<color=red>Attack in progress ammo change forbidden</color>");
        return;
      }
      CustomAmmoCategory weaponAmmoCategory = this.parent.DisplayedWeapon.info().effectiveAmmoCategory;
      if (weaponAmmoCategory.BaseCategory.Is_NotSet == false) {
        ExtAmmunitionDef ammo = this.parent.DisplayedWeapon.ammo();
        List<ExtAmmunitionDef> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(this.parent.DisplayedWeapon, weaponAmmoCategory);
        foreach (ExtAmmunitionDef ammoDef in AvaibleAmmo) {
          if (ammoDef.Id == ammo.Id) {
            description.Append("<size=150%>");
          } else {
            description.Append("<size=100%>");
          }
          description.Append(CustomAmmoCategories.Settings.AmmoNameInSidePanel ? ammoDef.Name : ammoDef.UIName);
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
      Log.M.TWL(0,"WeaponAmmoHover.OnPointerClick called." + data.position);
      if (this.parent.DisplayedWeapon == null) { return; }
      if (this.weaponPanel.DisplayedActor == null) { return; }
      if ((ASWatchdog.instance != null) && (ASWatchdog.instance.isAnySequenceTracked())) {
        Log.M.WL(1, "Attack sequence is active");
        ASWatchdog.instance.logTrackedSequences();
        return;
      }
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
      bool isShift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift));
      if (CustomAmmoCategories.CycleAmmo(parent.DisplayedWeapon, data.button == PointerEventData.InputButton.Left)) {
        if (SceneSingletonBehavior<WwiseManager>.HasInstance) {
          uint num2 = SceneSingletonBehavior<WwiseManager>.Instance.PostEventById(390458608, parent.DisplayedWeapon.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          Log.S.TWL(0, "Playing sound by id:" + num2);
        } else {
          Log.S.TWL(0, "Can't play");
        }
        if (isShift) {
          foreach (CombatHUDWeaponSlot slot in this.weaponPanel.DisplayedWeaponSlots) {
            if (slot.DisplayedWeapon == parent.DisplayedWeapon) { continue; }
            if (slot.DisplayedWeapon.defId != parent.DisplayedWeapon.defId) { continue; }
            bool indState = slot.DisplayedWeapon.IndirectFireCapable();
            slot.DisplayedWeapon.SyncAmmo(parent.DisplayedWeapon);
            slot.RefreshWeaponCapabilities(indState);
          }
        }
      }
      parent.RefreshWeaponCapabilities(prevIndirectState);
      if (hovered) { ShowSidePanel(); }
      RefreshText();
    }
  }
  public class WeaponDamageHover : EventTrigger {
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
      if (HUD.SelectedTarget != null) {
        Vector3 position = (parent.DisplayedWeapon.parent.HasMovedThisRound ? null : HUD.SelectionHandler.ActiveState?.PreviewPos) ?? parent.DisplayedWeapon.parent.CurrentPosition;
        CustAmmoCategories.DamageModifiers mods = parent.DisplayedWeapon.GetDamageModifiers(position, HUD.SelectedTarget);
        float damage = parent.DisplayedWeapon.DamagePerShot;
        float ap = parent.DisplayedWeapon.StructureDamagePerShot;
        float heat = parent.DisplayedWeapon.HeatDamagePerShot;
        float stability = parent.DisplayedWeapon.Instability();
        int shots = parent.DisplayedWeapon.ShotsWhenFired;
        string descr = string.Empty;
        mods.Calculate(-1, ref damage, ref ap, ref heat, ref stability, ref descr, false, true);
        description.Append("<size=80%>" + descr + "</size>");
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
      foreach (AmmunitionBox box in boxes) {
        if (box.ammoDef.Description.Id != ammo.Id) { continue; }
        description.Append(box == currentBox ? "<size=100%>" : "<size=80%>");
        if (box.IsFunctional == false) {
          description.Append("<color=red>");
        } else
        if (box.CurrentAmmo == 0) {
          description.Append("<color=orange>");
        } else {
          description.Append("<color=white>");
        }
        description.Append(box.Description.UIName);
        Thread.CurrentThread.pushActor(this.parent.DisplayedWeapon.parent);
        if (box.mechComponentRef != null) {
          description.Append("(" + Mech.GetAbbreviatedChassisLocation(box.mechComponentRef.MountedLocation) + ")");
        } else if (box.vehicleComponentRef != null) {
          description.Append("(" + Vehicle.GetLongChassisLocation(box.vehicleComponentRef.MountedLocation) + ")");
        }
        Thread.CurrentThread.clearActor();
        description.Append(" " + box.CurrentAmmo + "/" + box.ammunitionBoxDef.Capacity);
        description.Append("</color>");
        description.Append("</size>\n");
      }
      if (currentBox != null) {
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
    //private bool hovered;
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
      //hovered = true;
      ShowSidePanel();
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.LogWrite("WeaponHitChanceHover.OnPointerExit called." + data.position + "\n");
      //HUD.SidePanel.ForceHide();
      //hovered = false;
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
  public class WeaponSwitchButton : EventTrigger {
    public CombatHUDWeaponSlot parent;
    public CombatHUDWeaponPanel weaponPanel;
    public CombatHUD HUD;
    public SVGImage image;
    public string iconid;
    public bool moveUp;
    //public string iconid;
    private bool hovered;
    public void Init(CombatHUD HUD, CombatHUDWeaponPanel panel, CombatHUDWeaponSlot slot, string iconid, bool moveUp) {
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
      } catch (Exception e) {
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
        if (image != null) {
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
    public void Init(CombatHUDWeaponPanel panel, CombatHUDWeaponSlotEx slot) {
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
      if (text_HitChance != null) text_HitChance.SetPosX(slot.hitChanceHover.transform.position.x + 20f);
      if (text_WeaponText != null) Log.M.WL(1, "text_WeaponText x:" + text_WeaponText.transform.position.x);
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
      Log.M.TWL(0, "AttackSequence.SortSelectedWeapons " + new Localize.Text(__instance.attacker.DisplayName).ToString());
      List<Weapon> uiSorted = __instance.attacker.sortedUIWeapons();
      if (uiSorted == null) { return; };
      Log.M.WL(1, "ui weapons order found:" + uiSorted.Count);
      Dictionary<Weapon, int> uiIndexes = new Dictionary<Weapon, int>();
      for (int i = 0; i < uiSorted.Count; ++i) {
        Log.M.WL(2, "[" + i + "]" + uiSorted[i].defId);
        uiIndexes.Add(uiSorted[i], i);
      }
      for (int gid = 0; gid < ___sortedWeapons.Count; ++gid) {
        bool weaponHasNoUISortIndex = false;
        Log.M.WL(1, "original");
        for (int wid = 0; wid < ___sortedWeapons[gid].Count; ++wid) {
          Log.M.WL(2, "weapon[" + gid + "][" + wid + "] = " + ___sortedWeapons[gid][wid].defId + " " + (uiIndexes.ContainsKey(___sortedWeapons[gid][wid]) ? uiIndexes[___sortedWeapons[gid][wid]].ToString() : "none"));
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
  public class WeaponOrderData {
    public Dictionary<string, List<string>> definitionOrders { get; set; } = null;
    public WeaponOrderData() {
      definitionOrders = new Dictionary<string, List<string>>();
    }
  }
  public class AmmoBoxOderDataElementDef {
    public string ammoBoxDefId { get; set; } = string.Empty;
    public ChassisLocations mountLocation { get; set; } = ChassisLocations.None;
    public AmmoBoxOderDataElementDef() { }
    public AmmoBoxOderDataElementDef(MechComponentRef componentRef) {
      this.ammoBoxDefId = componentRef.ComponentDefID;
      this.mountLocation = componentRef.MountedLocation;
    }
  }
  public class WeaponOrderDataElementDef {
    public string weaponDefId { get; set; } = string.Empty;
    public ChassisLocations mountLocation { get; set; } = ChassisLocations.None;
    public string defaultModeId { get; set; } = string.Empty;
    public string defaultAmmoId { get; set; } = string.Empty;
    public bool automaticAmmoBoxesOrder { get; set; } = true;
    public List<AmmoBoxOderDataElementDef> ammoBoxesOrder { get; set; } = new List<AmmoBoxOderDataElementDef>();
    public WeaponOrderDataElementDef() { }
    public WeaponOrderDataElementDef(WeaponOrderDataElement element) {
      this.weaponDefId = element.componentRef.ComponentDefID;
      this.mountLocation = element.componentRef.MountedLocation;
      this.defaultModeId = element.defaultModeId;
      this.defaultAmmoId = element.defaultAmmoId;
      this.automaticAmmoBoxesOrder = element.automaticAmmoBoxesOrder;
      this.ammoBoxesOrder = new List<AmmoBoxOderDataElementDef>();
      foreach(var box in element.ammoBoxesOrder) {
        this.ammoBoxesOrder.Add(new AmmoBoxOderDataElementDef(box));
      }
    }
  }
  public static class WeaponDefModesCollectHelper {
    private static Dictionary<string, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>>> registry = new Dictionary<string, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>>>();
    public static void RegisterCallback(string id, Func<BaseComponentRef, List<BaseComponentRef>, List<WeaponMode>> callback) {
      registry[id] = callback;
    }
    private static Dictionary<BaseComponentRef, List<WeaponMode>> weaponModesCache = new Dictionary<BaseComponentRef, List<WeaponMode>>();
    public static void flushCache() { weaponModesCache.Clear(); }
    public static List<WeaponMode> WeaponModes(this BaseComponentRef componentRef, List<BaseComponentRef> inventory) {
      if (weaponModesCache.TryGetValue(componentRef, out var ret)) { return ret; }
      Dictionary<string, WeaponMode> result = new Dictionary<string, WeaponMode>();
      try {
        //WeaponDef weaponDef = componentRef.Def as WeaponDef;
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(componentRef.ComponentDefID);
        //if (weaponDef == null) { return result; }
        foreach (var mode in extWeaponDef.Modes) {
          result[mode.Key] = mode.Value;
        }
        foreach (var callback in registry) {
          foreach(var mode in callback.Value(componentRef, inventory)) {
            if (result.ContainsKey(mode.Id) && mode.isFromJson) {
              result[mode.Id] = result[mode.Id].merge(mode);
            } else {
              result[mode.Id] = mode;
            }
          }
          //result.AddRange(callback.Value(componentRef, inventory));
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
      weaponModesCache.Add(componentRef, result.Values.ToList());
      return weaponModesCache[componentRef];
    }
  }
  public class WeaponOrderDataElement {
    public MechComponentRef componentRef { get; set; } = null;
    public string defaultModeId { get; set; } = string.Empty;
    public string defaultAmmoId { get; set; } = string.Empty;
    public List<BaseComponentRef> inventory { get; set; } = new List<BaseComponentRef>();
    public List<WeaponMode> modes { get; set; } = new List<WeaponMode>();
    public List<MechComponentRef> ammoBoxesOrder { get; set; } = new List<MechComponentRef>();
    public bool automaticAmmoBoxesOrder { get; set; } = true;
    public WeaponOrderDataElement(MechComponentRef compRef, MechDef mechDef, WeaponOrderDataElementDef dataDef) {
      componentRef = compRef;
      List<MechComponentRef> allammo = new List<MechComponentRef>();
      inventory = new List<BaseComponentRef>();
      modes = new List<WeaponMode>();
      foreach (var inv in mechDef.Inventory) {
        if (inv == null) { continue; }
        if (inv.MountedLocation == ChassisLocations.None) { continue; }
        inventory.Add(inv);
      }
      HashSet<string> ammoCatIds = new HashSet<string>();
      WeaponDef weaponDef = componentRef.Def as WeaponDef;
      Log.M?.TWL(0, "WeaponOrderDataElement "+weaponDef.Description.Id);
      if (weaponDef != null) {
        modes = componentRef.WeaponModes(inventory);
        if (weaponDef.exDef().AmmoCategory.BaseCategory.Is_NotSet == false) {
          Log.M?.WL(1, "ammo cat:"+ weaponDef.exDef().AmmoCategory.Id);
          ammoCatIds.Add(weaponDef.exDef().AmmoCategory.Id);
        }
      }
      foreach (var mode in modes) {
        if (mode.AmmoCategory == null) { continue; }
        if (mode.AmmoCategory.BaseCategory.Is_NotSet) { continue; }
        Log.M?.WL(1, "ammo cat:" + mode.AmmoCategory.Id);
        ammoCatIds.Add(mode.AmmoCategory.Id);
      }
      if (ammoCatIds.Count > 0) {
        for (int i = 0; i < mechDef.Inventory.Length; ++i) {
          MechComponentRef componentRef = mechDef.Inventory[i];
          if (componentRef.ComponentDefType != ComponentType.AmmunitionBox) { continue; }
          AmmunitionBoxDef ammunitionBoxDef = componentRef.Def as AmmunitionBoxDef;
          if (ammunitionBoxDef == null) { continue; }
          Log.M?.WL(1, "ammunitionBoxDef:" + ammunitionBoxDef.Description.Id);
          if (ammunitionBoxDef.Ammo == null) { Log.M?.WL(2, "Ammo is null"); continue; }
          if (ammunitionBoxDef.Ammo.extDef() == null) { Log.M?.WL(2, "extDef is null"); continue; }
          ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammunitionBoxDef.AmmoID);
          if (extAmmo == CustomAmmoCategories.DefaultAmmo) {
            Log.M?.WL(2, "can't find extended definition for "+ ammunitionBoxDef.AmmoID);
          }
          Log.M?.WL(2, "ammo: "+ ammunitionBoxDef.AmmoID+ " category "+ extAmmo.AmmoCategory.Id+" id:"+extAmmo.Id);
          if (ammoCatIds.Contains(extAmmo.AmmoCategory.Id) == false) { continue; }
          allammo.Add(componentRef);
        }
      }
      Log.M?.WL(1, "allammo:" + allammo.Count);

      if (dataDef == null) {
        this.automaticAmmoBoxesOrder = true;
        this.defaultAmmoId = string.Empty;
        this.defaultModeId = string.Empty;
      } else {
        this.automaticAmmoBoxesOrder = dataDef.automaticAmmoBoxesOrder;
        this.defaultAmmoId = dataDef.defaultAmmoId;
        this.defaultModeId = dataDef.defaultModeId;
        for(int i=0;i< dataDef.ammoBoxesOrder.Count; ++i) { AmmoBoxOderDataElementDef ammoDataDef = dataDef.ammoBoxesOrder[i];
          for(int ii = 0; ii < allammo.Count; ++ii) { MechComponentRef componentRef = allammo[ii];
            if((componentRef.ComponentDefID == ammoDataDef.ammoBoxDefId)&&(componentRef.MountedLocation == ammoDataDef.mountLocation)) {
              ammoBoxesOrder.Add(componentRef);
              allammo.RemoveAt(ii);
              break;
            }
          }
        }
      }
      foreach(var ammobox in allammo) {
        ammoBoxesOrder.Add(ammobox);
      }
      Log.M?.WL(1, "ammoBoxesOrder:"+ ammoBoxesOrder.Count);
    }
  }
  public class WeaponOrderDataEx {
    public Dictionary<string, List<WeaponOrderDataElementDef>> definitionOrders { get; set; } = new Dictionary<string, List<WeaponOrderDataElementDef>>();
  }
  public static class WeaponOrderSimGameHelper {
    public static WeaponOrderData ordersData { get; set; } = new WeaponOrderData();
    public static WeaponOrderDataEx ordersDataEx { get; set; } = new WeaponOrderDataEx();
    public static string WeaponOrderStatisticName { get; private set; } = "CACWeaponOrder";
    public static string WeaponOrderExStatisticName { get; private set; } = "CACWeaponOrderEx";
    public static void InitSimGame(SimGameState sim) {
      Statistic stat = sim.CompanyStats.GetStatistic(WeaponOrderStatisticName);
      if (stat != null) { ordersData = JsonConvert.DeserializeObject<WeaponOrderData>(stat.Value<string>()); } else {
        ordersData = new WeaponOrderData();
      }
      stat = sim.CompanyStats.GetStatistic(WeaponOrderExStatisticName);
      if (stat != null) { ordersDataEx = JsonConvert.DeserializeObject<WeaponOrderDataEx>(stat.Value<string>()); } else {
        ordersDataEx = new WeaponOrderDataEx();
      }
    }
    public static List<WeaponOrderDataElement> unpackWeaponsOrderData(this MechDef mechDef, List<WeaponOrderDataElementDef> weaponOrderData) {
      List<WeaponOrderDataElement> result = new List<WeaponOrderDataElement>();
      List<MechComponentRef> allweapons = new List<MechComponentRef>();
      for (int index = 0; index < mechDef.Inventory.Length; ++index) {
        if (mechDef.Inventory[index].ComponentDefType == ComponentType.Weapon) { allweapons.Add(mechDef.Inventory[index]); }
      }
      for (int i = 0; i < weaponOrderData.Count; ++i) { WeaponOrderDataElementDef dataDef = weaponOrderData[i];
        for (int ii = 0; ii < allweapons.Count; ++ii) { MechComponentRef componentRef = allweapons[ii];
          if ((dataDef.weaponDefId == componentRef.ComponentDefID)&&(dataDef.mountLocation == componentRef.MountedLocation)) {
            result.Add(new WeaponOrderDataElement(componentRef, mechDef, dataDef));
            allweapons.RemoveAt(ii);
            break;
          }
        }
      }
      for (int ii = 0; ii < allweapons.Count; ++ii) { MechComponentRef componentRef = allweapons[ii];
        result.Add(new WeaponOrderDataElement(componentRef, mechDef, null));
      }
      return result;
    }
    public static List<WeaponOrderDataElement> unpackWeaponsOrderData(this MechDef mechDef, List<string> weaponOrderData) {
      List<WeaponOrderDataElement> result = new List<WeaponOrderDataElement>();
      List<MechComponentRef> allweapons = new List<MechComponentRef>();
      for (int index = 0; index < mechDef.Inventory.Length; ++index) {
        if (mechDef.Inventory[index].ComponentDefType == ComponentType.Weapon) { allweapons.Add(mechDef.Inventory[index]); }
      }
      for (int i = 0; i < weaponOrderData.Count; ++i) {
        string dataDefId = weaponOrderData[i];
        for (int ii = 0; ii < allweapons.Count; ++ii) {
          MechComponentRef componentRef = allweapons[ii];
          if (dataDefId == componentRef.ComponentDefID) {
            result.Add(new WeaponOrderDataElement(componentRef, mechDef, null));
            allweapons.RemoveAt(ii);
            break;
          }
        }
      }
      for (int ii = 0; ii < allweapons.Count; ++ii) {
        MechComponentRef componentRef = allweapons[ii];
        result.Add(new WeaponOrderDataElement(componentRef, mechDef, null));
      }
      return result;
    }
  }
  public class ModesOrderControl : MonoBehaviour {
    public RectTransform listParent { get; set; } = null;
    public LocalizableText componentCountText { get; set; } = null;
    public WeaponsOrderPopupSupervisor parent { get; set; } = null;
    public WeaponsOrderUIItem currentItem { get; set; } = null;
    public ModesOrderUIItem selectedItem { get; set; } = null;
    //public List<ModesOrderUIItem> avaibleItems { get; set; } = new List<ModesOrderUIItem>();
    public List<ModesOrderUIItem> activeItems { get; set; } = new List<ModesOrderUIItem>();
    public ModesOrderUIItem placeholderItem { get; set; } = null;
    public void Apply(WeaponsOrderUIItem item) {
      currentItem = item;
      WeaponDef weaponDef = item.data.componentRef.Def as WeaponDef;
      foreach (var mode in item.data.modes) {
        AddWeaponUIItem(mode, mode.Id == item.data.defaultModeId);
      }
      AddWeaponUIItem(null,false);
    }
    public void SelectionChanged(ModesOrderUIItem item) {
      if(selectedItem != null) {
        Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("backgroundColor").Value.SetUIColor(UIColor.EquipmentColor);
      }
      selectedItem = item;
      if (selectedItem != null) {
        Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("backgroundColor").Value.SetUIColor(UIColor.Orange);
      }
      this.currentItem.data.defaultModeId = selectedItem != null ? selectedItem.data.Id : string.Empty;
    }
    public void AddWeaponUIItem(WeaponMode dataElement, bool selected) {
      GameObject gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_ModesOrderItem", BattleTechResourceType.UIModulePrefabs);
      if (gameObject == null) {
        gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_MechLoadoutItem", BattleTechResourceType.UIModulePrefabs);
        gameObject.name = "uixPrfPanl_LC_ModesOrderItem(Clone)";
      }
      gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
      ModesOrderUIItem item = gameObject.GetComponent<ModesOrderUIItem>();
      if (null == item) { item = gameObject.AddComponent<ModesOrderUIItem>(); }
      item.layoutElement = gameObject.GetComponent<LayoutElement>();
      if (null == item.layoutElement) { item.layoutElement = gameObject.AddComponent<LayoutElement>(); }
      item.layoutElement.ignoreLayout = false;
      item.data = dataElement;
      item.parent = this;
      LanceMechEquipmentListItem component = gameObject.GetComponent<LanceMechEquipmentListItem>();
      item.ui = component;
      if (dataElement != null) {
        string text = new Text("{0}", dataElement.UIName).ToString();
        component.SetData(text, ComponentDamageLevel.Functional, UIColor.White, selected?UIColor.Orange:UIColor.EquipmentColor);
        Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value = component.GetComponent<HBSTooltip>();
        if(Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value != null) {
          Traverse.Create(component).Field<HBSTooltip>("EquipmentTooltip").Value.SetDefaultStateData(new BaseDescriptionDef(dataElement.Id, dataElement.UIName, dataElement.Description, string.Empty).GetTooltipStateData());
        }
      } else {
        component.SetData(string.Empty, ComponentDamageLevel.Functional, UIColor.Clear, UIColor.Clear);
      }
      gameObject.transform.SetParent(listParent, false);
      item.index = gameObject.transform.GetSiblingIndex();
      if (dataElement != null) {
        item.gameObject.SetActive(true);
        activeItems.Add(item);
        if (selected) { selectedItem = item; }
      } else {
        item.gameObject.SetActive(false);
        this.placeholderItem = item;
      }
    }
    public void Clear() {
      for (int index = this.activeItems.Count - 1; index >= 0; --index) {
        LanceMechEquipmentListItem labItemSlotElement = this.activeItems[index].ui;
        labItemSlotElement.gameObject.transform.SetParent((Transform)null, false);
        ModesOrderUIItem weaponsOrderItem = this.activeItems[index];
        if (weaponsOrderItem != null) {
          weaponsOrderItem.parent = null;
          weaponsOrderItem.data = null;
        }
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_ModesOrderItem", labItemSlotElement.gameObject);
      }
      this.activeItems.Clear();
      selectedItem = null;
      currentItem = null;
      if (placeholderItem != null) {
        placeholderItem.gameObject.transform.SetParent(null, false);
        placeholderItem.parent = null;
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_ModesOrderItem", placeholderItem.gameObject);
      }
    }
  }
  public class ModesOrderUIItem : MonoBehaviour, IPointerClickHandler {
    public WeaponMode data { get; set; } = null;
    public ModesOrderControl parent { get; set; } = null;
    public LayoutElement layoutElement { get; set; } = null;
    public LanceMechEquipmentListItem ui { get; set; } = null;
    public int index { get; set; } = -1;
    private RectTransform f_rect = null;
    public RectTransform rect {
      get {
        if (f_rect == null) { f_rect = GetComponent<RectTransform>(); }
        return f_rect;
      }
    }
    public void OnPointerDoubleClick(PointerEventData eventData) {
      this.parent.SelectionChanged(this);
    }
    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left) {
        eventData.clickCount = 0;
        this.OnPointerDoubleClick(eventData);
      }
    }
  }
  public class AmmoOrderControl : MonoBehaviour {
    public RectTransform listParent { get; set; } = null;
    public LocalizableText componentCountText { get; set; } = null;
    public LocalizableText componentLabelText { get; set; } = null;
    public WeaponsOrderPopupSupervisor parent { get; set; } = null;
    public WeaponsOrderUIItem currentItem { get; set; } = null;
    public AmmoOrderUIItem selectedItem { get; set; } = null;
    //public List<ModesOrderUIItem> avaibleItems { get; set; } = new List<ModesOrderUIItem>();
    public List<AmmoOrderUIItem> activeItems { get; set; } = new List<AmmoOrderUIItem>();
    public AmmoOrderUIItem placeholderItem { get; set; } = null;
    public void Apply(WeaponsOrderUIItem item) {
      try {
        Log.M?.TWL(0, "AmmoOrderControl.Apply "+ item.data.componentRef.ComponentDefID+ " defAmmo:"+ item.data.defaultAmmoId);
        currentItem = item;
        bool selected = false;
        foreach (var ammo in item.data.ammoBoxesOrder) {
          Log.M?.WL(1, "box:" + (ammo.Def as AmmunitionBoxDef).AmmoID);
          if (selected == false) {
            if ((ammo.Def as AmmunitionBoxDef).AmmoID == item.data.defaultAmmoId) {
              AddWeaponUIItem(ammo, true);
              selected = true;
            } else {
              AddWeaponUIItem(ammo, false);
            }
          } else {
            AddWeaponUIItem(ammo, false);
          }
        }
        AddWeaponUIItem(null, false);
        this.SelectionChanged(this.selectedItem);
        componentLabelText.SetText(item.data.automaticAmmoBoxesOrder ? "__/CAC.WO.AMMO.AUTOMATIC/__" : "__/CAC.WO.AMMO.MANUAL/__");
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public void SelectionChanged(AmmoOrderUIItem item) {
      try {
        if (selectedItem != null) {
          Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("itemTextColor").Value.SetUIColor(UIColor.White);
          UIColor bgColor = MechComponentRef.GetUIColor(selectedItem.data);
          if (selectedItem.data.DamageLevel == ComponentDamageLevel.Destroyed) {
            bgColor = UIColor.Disabled;
          }
          Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("backgroundColor").Value.SetUIColor(bgColor);
        }
        selectedItem = item;
        if (selectedItem != null) {
          Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("itemTextColor").Value.SetUIColor(UIColor.Black);
          Traverse.Create(selectedItem.ui).Field<UIColorRefTracker>("backgroundColor").Value.SetUIColor(UIColor.Orange);
        }
        this.currentItem.data.defaultAmmoId = selectedItem != null ? (selectedItem.data.Def as AmmunitionBoxDef).AmmoID : string.Empty;
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public void AddWeaponUIItem(MechComponentRef dataElement, bool selected) {
      Log.M?.TWL(0, "AddWeaponUIItem "+ (dataElement==null?"null":dataElement.ComponentDefID)+" selected:"+selected);
      GameObject gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_AmmoOrderItem", BattleTechResourceType.UIModulePrefabs);
      if (gameObject == null) {
        gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_MechLoadoutItem", BattleTechResourceType.UIModulePrefabs);
        gameObject.name = "uixPrfPanl_LC_AmmoOrderItem(Clone)";
      }
      gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
      AmmoOrderUIItem item = gameObject.GetComponent<AmmoOrderUIItem>();
      if (null == item) { item = gameObject.AddComponent<AmmoOrderUIItem>(); }
      item.layoutElement = gameObject.GetComponent<LayoutElement>();
      if (null == item.layoutElement) { item.layoutElement = gameObject.AddComponent<LayoutElement>(); }
      item.layoutElement.ignoreLayout = false;
      item.data = dataElement;
      item.parent = this;
      LanceMechEquipmentListItem component = gameObject.GetComponent<LanceMechEquipmentListItem>();
      item.ui = component;
      if (dataElement != null) {
        UIColor bgColor = MechComponentRef.GetUIColor(dataElement);
        if (dataElement.DamageLevel == ComponentDamageLevel.Destroyed) {
          bgColor = UIColor.Disabled;
        }
        string text = new Text("{0} {1}", dataElement.Def.Description.UIName, Mech.GetAbbreviatedChassisLocation(dataElement.MountedLocation)).ToString();
        if (selected) {
          component.SetData(text, ComponentDamageLevel.Functional, UIColor.Black, UIColor.Orange);
        } else {
          component.SetData(text, ComponentDamageLevel.Functional, UIColor.White, bgColor);
        }
        component.SetTooltipData(dataElement.Def);
      } else {
        component.SetData(string.Empty, ComponentDamageLevel.Functional, UIColor.Clear, UIColor.Clear);
      }
      gameObject.transform.SetParent(listParent, false);
      item.index = gameObject.transform.GetSiblingIndex();
      if (dataElement != null) {
        item.gameObject.SetActive(true);
        activeItems.Add(item);
        if (selected) { selectedItem = item; }
      } else {
        item.gameObject.SetActive(false);
        this.placeholderItem = item;
      }
    }
    public void Clear() {
      for (int index = this.activeItems.Count - 1; index >= 0; --index) {
        LanceMechEquipmentListItem labItemSlotElement = this.activeItems[index].ui;
        labItemSlotElement.gameObject.transform.SetParent((Transform)null, false);
        AmmoOrderUIItem weaponsOrderItem = this.activeItems[index];
        if (weaponsOrderItem != null) {
          weaponsOrderItem.parent = null;
          weaponsOrderItem.data = null;
        }
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_AmmoOrderItem", labItemSlotElement.gameObject);
      }
      this.activeItems.Clear();
      selectedItem = null;
      currentItem = null;
      if (placeholderItem != null) {
        placeholderItem.gameObject.transform.SetParent(null, false);
        placeholderItem.parent = null;
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_AmmoOrderItem", placeholderItem.gameObject);
      }
    }
  }

  public class AmmoOrderUIItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {
    public MechComponentRef data { get; set; } = null;
    public AmmoOrderControl parent { get; set; } = null;
    public LayoutElement layoutElement { get; set; } = null;
    public LanceMechEquipmentListItem ui { get; set; } = null;
    private RectTransform f_rect = null;
    public int index { get; set; } = -1;

    public RectTransform rect {
      get {
        if (f_rect == null) { f_rect = GetComponent<RectTransform>(); }
        return f_rect;
      }
    }
    private Vector2 lastMousePosition;
    public void OnBeginDrag(PointerEventData eventData) {
      Log.M?.TWL(0, "AmmoOrderUIItem.OnBeginDrag " + data.ComponentDefID);
      lastMousePosition = eventData.position;
      layoutElement.ignoreLayout = true;
      this.parent.placeholderItem.gameObject.SetActive(true);
      //rect.position += new Vector3(0,0f,0f);
      int cur_index = this.transform.GetSiblingIndex();
      this.transform.SetAsLastSibling();
      this.parent.placeholderItem.transform.SetSiblingIndex(cur_index);
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 currentMousePosition = eventData.position;
      Vector2 diff = currentMousePosition - lastMousePosition;

      Vector3 newPosition = rect.position + new Vector3(0f, diff.y, 0f);
      Vector3 oldPos = rect.position;
      rect.position = newPosition;
      if (!IsRectTransformInsideSreen(rect)) {
        rect.position = oldPos;
      }
      bool pos_found = false;
      for (int i = 0; i < this.parent.activeItems.Count; ++i) {
        if (this.parent.activeItems[i] == this) { continue; }
        if (this.parent.activeItems[i].transform.position.y < this.transform.position.y) {
          int cur_index = this.parent.activeItems[i].transform.GetSiblingIndex();
          pos_found = true;
          if (this.parent.placeholderItem.transform.GetSiblingIndex() != (cur_index - 1)) {
            this.parent.placeholderItem.transform.SetSiblingIndex(cur_index - 1);
          }
          break;
        }
      }
      if (pos_found == false) {
        int cur_index = this.transform.GetSiblingIndex();
        if (this.parent.placeholderItem.transform.GetSiblingIndex() != (cur_index - 1)) {
          this.parent.placeholderItem.transform.SetSiblingIndex(cur_index - 1);
        }
      }
      lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
      Log.M?.TWL(0, "WeaponsOrderItem.OnEndDrag " + data.ComponentDefID);
      this.parent.currentItem.data.automaticAmmoBoxesOrder = false;
      this.parent.componentLabelText.SetText(this.parent.currentItem.data.automaticAmmoBoxesOrder ? "__/CAC.WO.AMMO.AUTOMATIC/__" : "__/CAC.WO.AMMO.MANUAL/__");
      this.index = this.parent.placeholderItem.transform.GetSiblingIndex();
      this.transform.SetSiblingIndex(this.parent.placeholderItem.transform.GetSiblingIndex());
      this.parent.placeholderItem.transform.SetAsLastSibling();
      this.parent.placeholderItem.gameObject.SetActive(false);
      layoutElement.ignoreLayout = false;
      AmmoOrderUIItem[] items = this.transform.parent.gameObject.GetComponentsInChildren<AmmoOrderUIItem>();
      foreach (var item in items) {
        item.index = item.transform.GetSiblingIndex();
        if (item.data == null) { continue; }
        //this.parent.parent.weapons[item.index] = item.componentRef;
        this.parent.activeItems[item.index] = item;
      }
    }
    private bool IsRectTransformInsideSreen(RectTransform rectTransform) {
      //if ((rectTransform.localPosition.y + rectTransform.sizeDelta.y) < 0f) { return false; }
      return true;
    }
    public void OnPointerDoubleClick(PointerEventData eventData) {
      this.parent.SelectionChanged(this);
    }
    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left) {
        eventData.clickCount = 0;
        this.OnPointerDoubleClick(eventData);
      }
    }
  }

  public class WeaponsOrderControl: MonoBehaviour {
    public RectTransform listParent { get; set; } = null;
    public LocalizableText componentCountText { get; set; } = null;
    public List<WeaponsOrderUIItem> weaponsList { get; set; } = new List<WeaponsOrderUIItem>();
    public WeaponsOrderUIItem placeholderItem { get; set; } = null;
    public WeaponsOrderPopupSupervisor parent { get; set; } = null;
    public void AddWeaponUIItem(WeaponOrderDataElement dataElement) {
      GameObject gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_WeaponsOrderItem", BattleTechResourceType.UIModulePrefabs);
      if (gameObject == null) {
        gameObject = this.parent.mechBayPanel.DataManager.PooledInstantiate("uixPrfPanl_LC_MechLoadoutItem", BattleTechResourceType.UIModulePrefabs);
        gameObject.name = "uixPrfPanl_LC_WeaponsOrderItem(Clone)";
      }
      gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
      WeaponsOrderUIItem weaponsOrderItem = gameObject.GetComponent<WeaponsOrderUIItem>();
      if (null == weaponsOrderItem) { weaponsOrderItem = gameObject.AddComponent<WeaponsOrderUIItem>(); }
      weaponsOrderItem.layoutElement = gameObject.GetComponent<LayoutElement>();
      if (null == weaponsOrderItem.layoutElement) { weaponsOrderItem.layoutElement = gameObject.AddComponent<LayoutElement>(); }
      weaponsOrderItem.layoutElement.ignoreLayout = false;
      weaponsOrderItem.data = dataElement;
      weaponsOrderItem.parent = this;
      LanceMechEquipmentListItem component = gameObject.GetComponent<LanceMechEquipmentListItem>();
      weaponsOrderItem.ui = component;
      if (dataElement != null) {
        UIColor bgColor = MechComponentRef.GetUIColor(dataElement.componentRef);
        if (dataElement.componentRef.DamageLevel == ComponentDamageLevel.Destroyed) {
          bgColor = UIColor.Disabled;
        }
        string text = new Text("{0} {1}", dataElement.componentRef.Def.Description.UIName, Mech.GetAbbreviatedChassisLocation(dataElement.componentRef.MountedLocation)).ToString();
        component.SetData(text, dataElement.componentRef.DamageLevel, UIColor.White, bgColor);
        component.SetTooltipData(dataElement.componentRef.Def);
      } else {
        component.SetData(string.Empty, ComponentDamageLevel.Functional, UIColor.Clear, UIColor.Clear);
      }
      gameObject.transform.SetParent(listParent, false);
      weaponsOrderItem.index = gameObject.transform.GetSiblingIndex();
      if (dataElement != null) {
        weaponsOrderItem.gameObject.SetActive(true);
        weaponsList.Add(weaponsOrderItem);
      } else {
        weaponsOrderItem.gameObject.SetActive(false);
        this.placeholderItem = weaponsOrderItem;
      }
    }
    public void Clear() {
      for (int index = this.weaponsList.Count - 1; index >= 0; --index) {
        LanceMechEquipmentListItem labItemSlotElement = this.weaponsList[index].ui;
        labItemSlotElement.gameObject.transform.SetParent((Transform)null, false);
        WeaponsOrderUIItem weaponsOrderItem = this.weaponsList[index];
        if (weaponsOrderItem != null) {
          weaponsOrderItem.parent = null;
          weaponsOrderItem.data = null;
        }
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_WeaponsOrderItem", labItemSlotElement.gameObject);
      }
      this.weaponsList.Clear();
      if (placeholderItem != null) {
        placeholderItem.gameObject.transform.SetParent(null, false);
        placeholderItem.parent = null;
        this.parent.mechBayPanel.DataManager.PoolGameObject("uixPrfPanl_LC_WeaponsOrderItem", placeholderItem.gameObject);
      }
    }
  }
  public class WeaponsOrderUIItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler {
    public WeaponOrderDataElement data { get; set; } = null;
    public WeaponsOrderControl parent { get; set; } = null;
    public LayoutElement layoutElement { get; set; } = null;
    public LanceMechEquipmentListItem ui { get; set; } = null;
    private RectTransform f_rect = null;
    public RectTransform rect {
      get {
        if (f_rect == null) { f_rect = GetComponent<RectTransform>(); }
        return f_rect;
      }
    }
    private Vector2 lastMousePosition;
    public int index { get; set; } = -1;
    public void OnBeginDrag(PointerEventData eventData) {
      Log.M?.TWL(0, "WeaponsOrderItem.OnBeginDrag "+data.componentRef.ComponentDefID);
      lastMousePosition = eventData.position;
      layoutElement.ignoreLayout = true;
      this.parent.placeholderItem.gameObject.SetActive(true);
      //rect.position += new Vector3(0,0f,0f);
      int cur_index = this.transform.GetSiblingIndex();
      this.transform.SetAsLastSibling();
      this.parent.placeholderItem.transform.SetSiblingIndex(cur_index);
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 currentMousePosition = eventData.position;
      Vector2 diff = currentMousePosition - lastMousePosition;

      Vector3 newPosition = rect.position + new Vector3(0f, diff.y, 0f);
      Vector3 oldPos = rect.position;
      rect.position = newPosition;
      if (!IsRectTransformInsideSreen(rect)) {
        rect.position = oldPos;
      }
      bool pos_found = false;
      for(int i=0; i < this.parent.weaponsList.Count; ++i) {
        if (this.parent.weaponsList[i] == this) { continue; }
        if (this.parent.weaponsList[i].transform.position.y < this.transform.position.y) {
          int cur_index = this.parent.weaponsList[i].transform.GetSiblingIndex();
          pos_found = true;
          if (this.parent.placeholderItem.transform.GetSiblingIndex() != (cur_index - 1)) {
            this.parent.placeholderItem.transform.SetSiblingIndex(cur_index - 1);
          }
          break;
        }
      }
      if (pos_found == false) {
        int cur_index = this.transform.GetSiblingIndex();
        if (this.parent.placeholderItem.transform.GetSiblingIndex() != (cur_index - 1)) {
          this.parent.placeholderItem.transform.SetSiblingIndex(cur_index - 1);
        }
      }
      lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
      Log.M?.TWL(0, "WeaponsOrderItem.OnEndDrag " + data.componentRef.ComponentDefID);
      this.index = this.parent.placeholderItem.transform.GetSiblingIndex();
      this.transform.SetSiblingIndex(this.parent.placeholderItem.transform.GetSiblingIndex());
      this.parent.placeholderItem.transform.SetAsLastSibling();
      this.parent.placeholderItem.gameObject.SetActive(false);
      layoutElement.ignoreLayout = false;
      WeaponsOrderUIItem[] items = this.transform.parent.gameObject.GetComponentsInChildren<WeaponsOrderUIItem>();
      foreach (var item in items) {
        item.index = item.transform.GetSiblingIndex();
        if (item.data == null) { continue; }
        //this.parent.parent.weapons[item.index] = item.componentRef;
        this.parent.weaponsList[item.index] = item;
      }
    }
    private bool IsRectTransformInsideSreen(RectTransform rectTransform) {
      //if ((rectTransform.localPosition.y + rectTransform.sizeDelta.y) < 0f) { return false; }
      return true;
    }
    public void OnPointerDoubleClick(PointerEventData eventData) {
      this.parent.parent.OnWeaponDetails(this);
    }
    public void OnPointerClick(PointerEventData eventData) {
      if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left) {
        eventData.clickCount = 0;
        this.OnPointerDoubleClick(eventData);
      }
    }
  }
  public class WeaponsOrderPopupSupervisor: MonoBehaviour {
    public enum PopupState { Main, Weapon };
    public GenericPopup popup { get; set; } = null;
    public PopupState state { get; set; } = PopupState.Main;
    public WeaponsOrderControl weaponsControl { get; set; } = null;
    public RectTransform modesAmmoRT { get; set; } = null;
    public ModesOrderControl modesControl { get; set; } = null;
    public AmmoOrderControl ammoControl { get; set; } = null;
    public RectTransform modeAmmoLayout { get; set; } = null;
    public HBSButton backButton { get; set; } = null;
    public HBSButton saveButton { get; set; } = null;
    public MechBayPanel mechBayPanel { get; set; } = null;
    public void instantine() {
      if (weaponsControl != null) { return; }
      try {
        mechBayPanel = this.gameObject.GetComponent<MechBayPanel>();
        GameObject uixPrfPanl_ML_main_Widget = LazySingletonBehavior<UIManager>.Instance.dataManager.PooledInstantiate("uixPrfPanl_ML_main-Widget", BattleTechResourceType.UIModulePrefabs);
        if (uixPrfPanl_ML_main_Widget == null) {
          Log.M?.TWL(0, "uixPrfPanl_ML_main-Widget not found");
          return;
        }
        Log.M?.TWL(0, "uixPrfPanl_ML_main-Widget found");
        MechLabDismountWidget dismountWidget = uixPrfPanl_ML_main_Widget.GetComponentInChildren<MechLabDismountWidget>(true);
        if (dismountWidget == null) {
          Log.M?.TWL(0, "MechLabDismountWidget not found");
          return;
        }
        Log.M?.TWL(0, "MechLabDismountWidget found");

        {
          GameObject controlGO = GameObject.Instantiate(dismountWidget.gameObject);
          controlGO.name = "ui_weaponsOrder";
          controlGO.transform.localScale = Vector3.one;
          MechLabDismountWidget localWidget = controlGO.GetComponent<MechLabDismountWidget>();
          controlGO.FindObject<LocalizableText>("txt_label").SetText("__/CAC.WO.WEAPONS/__");
          controlGO.FindObject<LocalizableText>("txt_instr").gameObject.SetActive(false);
          weaponsControl = controlGO.AddComponent<WeaponsOrderControl>();
          weaponsControl.listParent = Traverse.Create(localWidget).Field<RectTransform>("listParent").Value;
          weaponsControl.componentCountText = Traverse.Create(localWidget).Field<LocalizableText>("componentCountText").Value;
          GameObject.Destroy(localWidget);
          weaponsControl.componentCountText.SetText("__/CAC.WO.HELP/__");
          HBSTooltip hBSTooltip = weaponsControl.componentCountText.gameObject.AddComponent<HBSTooltip>();
          hBSTooltip.SetDefaultStateData(new BaseDescriptionDef("WeaponsOrderHelpTooltipID", "__/CAC.WO.USAGE/__", "__/CAC.WO.USAGE.WEAPONS.DETAILS/__", string.Empty).GetTooltipStateData());
          VerticalLayoutGroup verticalLayoutGroup = weaponsControl.listParent.gameObject.GetComponent<VerticalLayoutGroup>();
          verticalLayoutGroup.spacing = 22f;
          weaponsControl.gameObject.SetActive(false);
          weaponsControl.parent = this;
          weaponsControl.gameObject.transform.SetParent(this.transform);
        }

        {
          GameObject controlGO = GameObject.Instantiate(dismountWidget.gameObject);
          controlGO.name = "ui_modesOrder";
          controlGO.transform.localScale = Vector3.one;
          MechLabDismountWidget localWidget = controlGO.GetComponent<MechLabDismountWidget>();
          controlGO.FindObject<LocalizableText>("txt_label").SetText("__/CAC.WO.MODES/__");
          controlGO.FindObject<LocalizableText>("txt_instr").gameObject.SetActive(false);
          modesControl = controlGO.AddComponent<ModesOrderControl>();
          modesControl.listParent = Traverse.Create(localWidget).Field<RectTransform>("listParent").Value;
          modesControl.componentCountText = Traverse.Create(localWidget).Field<LocalizableText>("componentCountText").Value;
          GameObject.Destroy(localWidget);
          modesControl.componentCountText.SetText("HELP");
          HBSTooltip hBSTooltip = modesControl.componentCountText.gameObject.AddComponent<HBSTooltip>();
          hBSTooltip.SetDefaultStateData(new BaseDescriptionDef("ModesOrderHelpTooltipID", "__/CAC.WO.USAGE/__", "__/CAC.WO.USAGE.MODE.DETAILS/__", string.Empty).GetTooltipStateData());
          VerticalLayoutGroup verticalLayoutGroup = modesControl.listParent.gameObject.GetComponent<VerticalLayoutGroup>();
          verticalLayoutGroup.spacing = 2f;
          modesControl.gameObject.SetActive(false);
          modesControl.parent = this;
          modesControl.gameObject.transform.SetParent(this.transform);
        }

        {
          GameObject controlGO = GameObject.Instantiate(dismountWidget.gameObject);
          controlGO.name = "ui_ammoOrder";
          controlGO.transform.localScale = Vector3.one;
          MechLabDismountWidget localWidget = controlGO.GetComponent<MechLabDismountWidget>();
          //controlGO.FindObject<LocalizableText>("txt_label").SetText("AMMO");
          controlGO.FindObject<LocalizableText>("txt_instr").gameObject.SetActive(false);
          ammoControl = controlGO.AddComponent<AmmoOrderControl>();
          ammoControl.componentLabelText = controlGO.FindObject<LocalizableText>("txt_label");
          ammoControl.listParent = Traverse.Create(localWidget).Field<RectTransform>("listParent").Value;
          ammoControl.componentCountText = Traverse.Create(localWidget).Field<LocalizableText>("componentCountText").Value;
          GameObject.Destroy(localWidget);
          ammoControl.componentCountText.SetText("HELP");
          HBSTooltip hBSTooltip = ammoControl.componentCountText.gameObject.AddComponent<HBSTooltip>();
          hBSTooltip.SetDefaultStateData(new BaseDescriptionDef("AmmoOrderHelpTooltipID", "__/CAC.WO.USAGE/__", "__/CAC.WO.USAGE.AMMO.DETAILS/__", string.Empty).GetTooltipStateData());
          VerticalLayoutGroup verticalLayoutGroup = ammoControl.listParent.gameObject.GetComponent<VerticalLayoutGroup>();
          verticalLayoutGroup.spacing = 2f;
          ammoControl.gameObject.SetActive(false);
          ammoControl.parent = this;
          ammoControl.gameObject.transform.SetParent(this.transform);
        }

        LazySingletonBehavior<UIManager>.Instance.dataManager.PoolGameObject("uixPrfPanl_ML_main-Widget", uixPrfPanl_ML_main_Widget);
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
    public void OnBack() {
      if (state == PopupState.Main) { OnClose(); }
      if (state == PopupState.Weapon) {
        CloseWeaponDetails();
      }
    }
    public void CloseWeaponDetails() {
      backButton.SetText("__/CAC.WO.CLOSE/__");
      saveButton.gameObject.SetActive(true);
      popup.Title = "__/CAC.WO.WEAPONS.ORDER/__";
      this.modesControl.Clear();
      this.ammoControl.Clear();
      this.weaponsControl.gameObject.SetActive(true);
      this.modesAmmoRT.gameObject.SetActive(false);
      state = PopupState.Main;
    }
    public void OnWeaponDetails(WeaponsOrderUIItem item) {
      this.state = PopupState.Weapon;
      this.modesControl.Apply(item);
      this.ammoControl.Apply(item);
      backButton.SetText("__/CAC.WO.BACK/__");
      saveButton.gameObject.SetActive(false);
      popup.Title = Traverse.Create(item.ui).Field<LocalizableText>("itemText").Value.text;
      this.weaponsControl.gameObject.SetActive(false);
      this.modesAmmoRT.gameObject.SetActive(true);
      
    }
    public void OnWeaponOrderButtonClick() {
      GenericPopupBuilder builder = GenericPopupBuilder.Create("__/CAC.WO.WEAPONS.ORDER/__", "PLACEHOLDER");
      builder.AddButton("__/CAC.WO.CLOSE/__", new Action(this.OnBack), false);
      builder.AddButton("__/CAC.WO.SAVE/__", new Action(this.OnSave), false);
      popup = builder.CancelOnEscape().Render();
      backButton = Traverse.Create(popup).Field<List<HBSButton>>("buttons").Value[0];
      saveButton = Traverse.Create(popup).Field<List<HBSButton>>("buttons").Value[1];
      this.OnShow();
    }
    public void OnSave() {
      if (this.mechBayPanel == null) { return; }
      if (state != PopupState.Main) { this.OnBack(); return; }
      MechBayMechUnitElement selectedMech = Traverse.Create(mechBayPanel).Field<MechBayMechUnitElement>("selectedMech").Value;
      if (selectedMech == null) { return; }
      if (WeaponOrderSimGameHelper.ordersDataEx.definitionOrders.TryGetValue(selectedMech.MechDef.GUID, out List<WeaponOrderDataElementDef> weaponsOrder) == false) {
        weaponsOrder = new List<WeaponOrderDataElementDef>();
        WeaponOrderSimGameHelper.ordersDataEx.definitionOrders.Add(selectedMech.MechDef.GUID, weaponsOrder);
      }
      weaponsOrder.Clear();
      for(int i = 0; i < this.weaponsControl.weaponsList.Count; ++i) {
        weaponsOrder.Add(new WeaponOrderDataElementDef(this.weaponsControl.weaponsList[i].data));
      }
      Statistic stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetStatistic(WeaponOrderSimGameHelper.WeaponOrderExStatisticName);
      if (stat == null) {
        stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.AddStatistic(WeaponOrderSimGameHelper.WeaponOrderExStatisticName, string.Empty);
      }
      Log.M?.TWL(0, "WeaponsOrderPopupSupervisor.OnSave");
      Log.M?.WL(0, JsonConvert.SerializeObject(WeaponOrderSimGameHelper.ordersDataEx,Formatting.Indented));
      stat.SetValue<string>(JsonConvert.SerializeObject(WeaponOrderSimGameHelper.ordersDataEx));
      this.OnClose();
    }

    public void OnClose() {
      WeaponDefModesCollectHelper.flushCache();
      if (weaponsControl != null) {
        weaponsControl.gameObject.SetActive(false);
        weaponsControl.gameObject.transform.SetParent(this.transform);
        weaponsControl.Clear();
      }
      if (modesControl != null) {
        modesControl.gameObject.SetActive(false);
        modesControl.gameObject.transform.SetParent(this.transform);
      }
      if (ammoControl != null) {
        ammoControl.gameObject.SetActive(false);
        ammoControl.gameObject.transform.SetParent(this.transform);
      }
      if (modesAmmoRT != null) {
        GameObject.Destroy(modesAmmoRT.gameObject);
        modesAmmoRT = null;
      }
      if(popup != null) {
        Traverse.Create(popup).Field<LocalizableText>("_contentText").Value.gameObject.SetActive(true);
        popup.Pool();
        popup = null;
      }
    }
    public void OnShow() {
      if ((weaponsControl != null) && (popup != null)) {
        try {
          WeaponDefModesCollectHelper.flushCache();
          LocalizableText _contentText = Traverse.Create(popup).Field<LocalizableText>("_contentText").Value;
          _contentText.gameObject.SetActive(false);
          {
            RectTransform controlRT = weaponsControl.gameObject.GetComponent<RectTransform>();
            controlRT.pivot = _contentText.rectTransform.pivot;
            controlRT.sizeDelta = new Vector2(_contentText.rectTransform.sizeDelta.x, controlRT.sizeDelta.y);

            weaponsControl.gameObject.SetActive(true);
            weaponsControl.gameObject.transform.SetParent(_contentText.gameObject.transform.parent);
            weaponsControl.gameObject.transform.SetSiblingIndex(_contentText.transform.GetSiblingIndex() + 1);
            LayoutElement layoutElement = weaponsControl.gameObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = false;
          }
          GameObject contentTextGO = GameObject.Instantiate(_contentText.gameObject);
          contentTextGO.SetActive(false);
          contentTextGO.transform.SetParent(_contentText.gameObject.transform.parent);
          contentTextGO.transform.SetSiblingIndex(weaponsControl.transform.GetSiblingIndex() + 1);
          contentTextGO.transform.localScale = Vector3.one;
          GameObject.Destroy(contentTextGO.GetComponent<LocalizableText>());
          modesAmmoRT = contentTextGO.GetComponent<RectTransform>();
          HorizontalLayoutGroup group = contentTextGO.AddComponent<HorizontalLayoutGroup>();
          group.spacing = 8f;
          group.padding = new RectOffset(10, 10, 0, 0);
          group.childAlignment = TextAnchor.UpperCenter;
          group.childControlHeight = false;
          group.childControlWidth = false;
          group.childForceExpandHeight = false;
          group.childForceExpandWidth = false;

          {
            RectTransform controlRT = modesControl.gameObject.GetComponent<RectTransform>();
            //controlRT.pivot = _contentText.rectTransform.pivot;
            //controlRT.sizeDelta = new Vector2(_contentText.rectTransform.sizeDelta.x, controlRT.sizeDelta.y);

            modesControl.gameObject.SetActive(true);
            modesControl.gameObject.transform.SetParent(modesAmmoRT);
            LayoutElement layoutElement = modesControl.gameObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = false;
          }
          {
            RectTransform controlRT = ammoControl.gameObject.GetComponent<RectTransform>();
            //controlRT.pivot = _contentText.rectTransform.pivot;
            //controlRT.sizeDelta = new Vector2(_contentText.rectTransform.sizeDelta.x, controlRT.sizeDelta.y);

            ammoControl.gameObject.SetActive(true);
            ammoControl.gameObject.transform.SetParent(modesAmmoRT);
            LayoutElement layoutElement = ammoControl.gameObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = false;
          }
          MechBayMechUnitElement selectedMech = Traverse.Create(mechBayPanel).Field<MechBayMechUnitElement>("selectedMech").Value;
          if (selectedMech == null) { return; }

          List<MechComponentRef> allweapons = new List<MechComponentRef>();
          for (int index = 0; index < selectedMech.MechDef.Inventory.Length; ++index) {
            if (selectedMech.MechDef.Inventory[index].ComponentDefType == ComponentType.Weapon) {
              allweapons.Add(selectedMech.MechDef.Inventory[index]);
            }
          }
          List<WeaponOrderDataElement> weaponsOrder = new List<WeaponOrderDataElement>();
          if (WeaponOrderSimGameHelper.ordersDataEx.definitionOrders.TryGetValue(selectedMech.MechDef.GUID, out List<WeaponOrderDataElementDef> weaponsOrderDef)) {
            weaponsOrder = selectedMech.MechDef.unpackWeaponsOrderData(weaponsOrderDef);
          }else
          if (WeaponOrderSimGameHelper.ordersData.definitionOrders.TryGetValue(selectedMech.MechDef.GUID, out List<string> weaponsOrderOld)) {
            weaponsOrder = selectedMech.MechDef.unpackWeaponsOrderData(weaponsOrderOld);
          } else {
            weaponsOrder = selectedMech.MechDef.unpackWeaponsOrderData(new List<string>());
          }
          Thread.CurrentThread.pushActorDef(selectedMech.MechDef);
          for (int index = 0; index < weaponsOrder.Count; ++index) {
            weaponsControl.AddWeaponUIItem(weaponsOrder[index]);
          }
          Thread.CurrentThread.clearActorDef();
          weaponsControl.AddWeaponUIItem(null);
        } catch(Exception e) {
          Log.M?.TWL(0,e.ToString(),true);
        }
      }
    }
  }

  [HarmonyPatch(typeof(MechBayPanel))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class MechBayPanel_ViewBays {
    //private static MechBayPanel mechBayPanel = null;
    //private static GenericPopup popup = null;
    //private static List<MechComponentRef> weapons = new List<MechComponentRef>();
    //private static int selectedWeapon = 0;
    //private static string BuildText() {
    //  StringBuilder result = new StringBuilder();
    //  for(int index = 0; index < weapons.Count; ++index) {
    //    if (index == selectedWeapon) { result.Append("<color=orange>"); }
    //    result.Append(weapons[index].Def.Description.UIName);
    //    if (index == selectedWeapon) { result.Append("</color>"); }
    //    result.AppendLine();
    //  }
    //  return result.ToString();
    //}
    //private static void Init() {
    //  if (mechBayPanel == null) { return; }
    //  MechBayMechUnitElement selectedMech = Traverse.Create(mechBayPanel).Field<MechBayMechUnitElement>("selectedMech").Value;
    //  if (selectedMech == null) { return; }
    //  weapons.Clear();
    //  selectedWeapon = 0;
    //  List<MechComponentRef> allweapons = new List<MechComponentRef>();
    //  for (int index = 0; index < selectedMech.MechDef.Inventory.Length; ++index) {
    //    if(selectedMech.MechDef.Inventory[index].ComponentDefType == ComponentType.Weapon) {
    //      allweapons.Add(selectedMech.MechDef.Inventory[index]);
    //    }
    //  }
    //  if (WeaponOrderSimGameHelper.ordersData.definitionOrders.TryGetValue(selectedMech.MechDef.GUID, out List<string> weaponsOrder) == false) {
    //    weaponsOrder = new List<string>();
    //  }
    //  int count = allweapons.Count;
    //  for (int index = 0; index < count; ++index) {
    //    string defid = string.Empty;
    //    if (index < weaponsOrder.Count) { defid = weaponsOrder[index]; };
    //    if (string.IsNullOrEmpty(defid)) { weapons.Add(null); }
    //    bool found = false;
    //    for(int i = 0; i < allweapons.Count; ++i) {
    //      if (allweapons[i].ComponentDefID == defid) { weapons.Add(allweapons[i]); allweapons.RemoveAt(i); found = true; break; }
    //    }
    //    if (found == false) { weapons.Add(null); };
    //  }
    //  for (int index = 0; index < weapons.Count; ++index) {
    //    if (weapons[index] != null) { continue; }
    //    if (allweapons.Count == 0) { continue; }
    //    weapons.Add(allweapons[0]);
    //    allweapons.RemoveAt(0);
    //  }
    //  for (int index = 0; index < weapons.Count;) {
    //    if (weapons[index] != null) { ++index; continue; }
    //    weapons.RemoveAt(index);
    //  }
    //}
    //private static void Clear() {
    //  selectedWeapon = 0;
    //  weapons.Clear();
    //}
    //private static void Save() {
    //  if (mechBayPanel == null) { return; }
    //  MechBayMechUnitElement selectedMech = Traverse.Create(mechBayPanel).Field<MechBayMechUnitElement>("selectedMech").Value;
    //  if (selectedMech == null) { return; }
    //  if (WeaponOrderSimGameHelper.ordersData.definitionOrders.TryGetValue(selectedMech.MechDef.GUID, out List<string> weaponsOrder) == false) {
    //    weaponsOrder = new List<string>();
    //    WeaponOrderSimGameHelper.ordersData.definitionOrders.Add(selectedMech.MechDef.GUID,weaponsOrder);
    //  }
    //  weaponsOrder.Clear();
    //  for(int index = 0;index < weapons.Count; ++index) {
    //    weaponsOrder.Add(weapons[index].ComponentDefID);
    //  }
    //  Statistic stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetStatistic(WeaponOrderSimGameHelper.WeaponOrderStatisticName);
    //  if (stat == null) {
    //    stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.AddStatistic(WeaponOrderSimGameHelper.WeaponOrderStatisticName,string.Empty);
    //  }
    //  stat.SetValue<string>(JsonConvert.SerializeObject(WeaponOrderSimGameHelper.ordersData));
    //}
    //private static void Left() {
    //  selectedWeapon = (selectedWeapon + 1) % weapons.Count;
    //  popup.TextContent = BuildText();
    //}
    //private static void Right() {
    //  selectedWeapon = selectedWeapon - 1;
    //  if (selectedWeapon < 0) { selectedWeapon = weapons.Count - 1; }
    //  popup.TextContent = BuildText();
    //}
    //private static void Up() {
    //  if(selectedWeapon > 0) {
    //    MechComponentRef tmp = weapons[selectedWeapon - 1];
    //    weapons[selectedWeapon - 1] = weapons[selectedWeapon];
    //    weapons[selectedWeapon] = tmp;
    //    selectedWeapon -= 1;
    //  }
    //  popup.TextContent = BuildText();
    //}
    //private static void Down() {
    //  if (selectedWeapon < (weapons.Count - 1)) {
    //    MechComponentRef tmp = weapons[selectedWeapon + 1];
    //    weapons[selectedWeapon + 1] = weapons[selectedWeapon];
    //    weapons[selectedWeapon] = tmp;
    //    selectedWeapon += 1;
    //  }
    //  popup.TextContent = BuildText();
    //}
    public static void Postfix(MechBayPanel __instance) {
      Log.M.TWL(0, "MechBayPanel.OnAddedToHierarchy");
      Transform weaponOrder = __instance.transform.FindRecursive("uixPrfBttn_BASE_iconActionButton-MANAGED-repair");
      if (weaponOrder == null) { return; }
      Log.M.WL(1, "uixPrfBttn_BASE_iconActionButton-MANAGED-repair found");
      Transform label = weaponOrder.FindRecursive("iconBtn_highlightLabel");
      if (label == null) { return; }
      Log.M.WL(1, "iconBtn_highlightLabel");
      label.gameObject.GetComponent<LocalizableText>().SetText("__/WEAPON ORDER/__");
      //MechBayPanel_ViewBays.mechBayPanel = __instance;
      HBSDOTweenButton button = weaponOrder.gameObject.GetComponent<HBSDOTweenButton>();
      WeaponsOrderPopupSupervisor weaponsOrderPopupSupervisor = __instance.gameObject.GetComponent<WeaponsOrderPopupSupervisor>();
      if (weaponsOrderPopupSupervisor == null) {
        weaponsOrderPopupSupervisor = __instance.gameObject.AddComponent<WeaponsOrderPopupSupervisor>();
        weaponsOrderPopupSupervisor.instantine();
      }
      button.OnClicked.AddListener(new UnityAction(weaponsOrderPopupSupervisor.OnWeaponOrderButtonClick));
      weaponOrder.gameObject.SetActive(true);
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("ResetSortedWeaponList")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDWeaponPanel_ResetSortedWeaponList {
    private static Dictionary<AbstractActor, List<Weapon>> sortedWeaponsByActor = new Dictionary<AbstractActor, List<Weapon>>();
    public static List<Weapon> sortedUIWeapons(this AbstractActor unit) {
      if (sortedWeaponsByActor.TryGetValue(unit, out List<Weapon> result)) {
        return result;
      }
      return null;
    }
    public static void Clear() { sortedWeaponsByActor.Clear(); }
    public static bool MoveWeaponUp(this AbstractActor unit, Weapon weapon) {
      if (sortedWeaponsByActor.TryGetValue(unit, out List<Weapon> sortedList)) {
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
        if (index >= (sortedList.Count - 1)) { return false; }
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
        } else {
          if (string.IsNullOrEmpty(__instance.DisplayedActor.PilotableActorDef.GUID)) { return true; }
          if (WeaponOrderSimGameHelper.ordersDataEx.definitionOrders.TryGetValue(__instance.DisplayedActor.PilotableActorDef.GUID, out List<WeaponOrderDataElementDef> simGameOrderEx)) {
            Log.M?.TWL(0, "CombatHUDWeaponPanel.ResetSortedWeaponList "+ __instance.DisplayedActor.PilotableActorDef.ChassisID+" GUID:"+ __instance.DisplayedActor.PilotableActorDef.GUID);
            List<Weapon> allweapons = new List<Weapon>();
            List<Weapon> result = new List<Weapon>();
            allweapons.AddRange(__instance.DisplayedActor.Weapons);
            int count = allweapons.Count;
            for (int index = 0; index < count; ++index) {
              string defid = string.Empty;
              int location = (int)ChassisLocations.None;
              WeaponOrderDataElementDef simOrderData = null;
              if (index < simGameOrderEx.Count) {
                defid = simGameOrderEx[index].weaponDefId; location = (int)simGameOrderEx[index].mountLocation;
                simOrderData = simGameOrderEx[index];
              }
              if (string.IsNullOrEmpty(defid)) { result.Add(null); continue; }
              bool found = false;
              for (int i = 0; i < allweapons.Count; ++i) {
                if ((allweapons[i].defId == defid)&&(allweapons[i].Location == location)) {
                  Log.M?.WL(1,"weapon "+ allweapons[i].defId + " found at "+index+". Mode:"+ simOrderData.defaultModeId+" Ammo:"+ simOrderData.defaultAmmoId+" boxes:"+ allweapons[i].ammoBoxes.Count);
                  if (string.IsNullOrEmpty(simOrderData.defaultAmmoId) == false) { allweapons[i].forceAmmo(simOrderData.defaultAmmoId); }
                  if (string.IsNullOrEmpty(simOrderData.defaultModeId) == false) { allweapons[i].info().setMode(simOrderData.defaultModeId); }
                  allweapons[i].info().Revalidate();
                  allweapons[i].info().setSorting(simOrderData);
                  if (simOrderData.automaticAmmoBoxesOrder) {
                    allweapons[i].FillAmmoBoxesCache();
                  }
                  found = true; result.Add(allweapons[i]); allweapons.RemoveAt(i); break;
                }
              }
              if (found == false) { result.Add(null); }
            }
            for (int index = 0; index < result.Count; ++index) {
              if (result[index] != null) { continue; }
              if (allweapons.Count == 0) { continue; }
              result[index] = allweapons[0]; allweapons.RemoveAt(0);
            }
            for (int index = 0; index < result.Count;) {
              if (result[index] != null) { ++index; continue; }
              result.RemoveAt(index);
            }
            ___sortedWeaponsList.Clear();
            ___sortedWeaponsList.AddRange(result);
            return false;
          } else
          if (WeaponOrderSimGameHelper.ordersData.definitionOrders.TryGetValue(__instance.DisplayedActor.PilotableActorDef.GUID, out List<string> simGameOrder)) {
            List<Weapon> allweapons = new List<Weapon>();
            List<Weapon> result = new List<Weapon>();
            allweapons.AddRange(__instance.DisplayedActor.Weapons);
            int count = allweapons.Count;
            for(int index = 0; index < count; ++index) {
              string defid = string.Empty;
              if (index < simGameOrder.Count) { defid = simGameOrder[index]; }
              if (string.IsNullOrEmpty(defid)) { result.Add(null); continue; }
              bool found = false;
              for(int i = 0; i < allweapons.Count; ++i) {
                if (allweapons[i].defId == defid) { found = true; result.Add(allweapons[i]); allweapons.RemoveAt(i); break; }
              }
              if(found == false) { result.Add(null); }
            }
            for(int index = 0; index < result.Count; ++index) {
              if (result[index] != null) { continue; }
              if (allweapons.Count == 0) { continue; }
              result[index] = allweapons[0]; allweapons.RemoveAt(0);
            }
            for (int index = 0; index < result.Count;) {
              if (result[index] != null) { ++index; continue; }
              result.RemoveAt(index);
            }
            ___sortedWeaponsList.Clear();
            ___sortedWeaponsList.AddRange(result);
            return false;
          }
        }
      } catch (Exception e) {
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
      } catch (Exception e) {
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
        foreach (var aInit in additionalInit) {
          aInit(__instance);
        }
        //DebugMouseClickTester tester = HUD.gameObject.GetComponent<DebugMouseClickTester>();
        //if (tester == null) { tester = HUD.gameObject.AddComponent<DebugMouseClickTester>(); }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(UIModule))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  public static class UIModule_OnAddedToHierarchy {
    public static void Postfix(UIModule __instance) {
      try {
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
      } catch (Exception e) {
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
    //private static int panelInstance = -1;
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
      last_consideringJump = consideringJump;
      last_useCOILPathingPreview = useCOILPathingPreview;
      CombatHUDWeaponPanelEx panelEx = __instance.gameObject.GetComponent<CombatHUDWeaponPanelEx>();
      if (panelEx != null) { panelEx.Refresh(); }
    }
  }
}