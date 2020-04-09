using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using HBS;
using HBS.Collections;
using HBS.Data;
using HBS.Util;
using InControl;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using static BattleTech.Data.DataManager;

namespace CustomUnits {
  public class HUDMechArmorReadoutOriginal {
    public Vector3 position { get; set; }
    public Vector3 localPosition { get; set; }
    public Vector2 anchorMin { get; set; }
    public Vector2 anchorMax { get; set; }
    public Vector2 anchoredPosition { get; set; }
    public Vector2 sizeDelta { get; set; }
    public Vector2 pivot { get; set; }
    public Vector3 localScale { get; set; }
    public SVGAsset icon { get; set; }
    public Color? color { get; set; }
    public SVGAsset aicon { get; set; }
    public void Restore(RectTransform tr, ref SVGImage img) {
      tr.anchorMin = anchorMin;
      tr.anchorMax = anchorMax;
      tr.anchoredPosition = anchoredPosition;
      tr.sizeDelta = sizeDelta;
      tr.pivot = pivot;
      tr.localScale = localScale;
    }
    public HUDMechArmorReadoutOriginal(RectTransform tr, SVGImage i) {
      position = tr.position;
      localPosition = tr.localPosition;
      anchorMin = tr.anchorMin;
      anchorMax = tr.anchorMax;
      anchoredPosition = tr.anchoredPosition;
      sizeDelta = tr.sizeDelta;
      pivot = tr.pivot;
      localScale = tr.localScale;
      icon = i.vectorGraphics;
      //color = i.color;
    }
  }
  public class VehicleReadoutImage {
    public Vector3 pos { get; set; }
    public Vector2 anchorMin { get; set; }
    public Vector2 anchorMax { get; set; }
    public Vector2 anchoredPosition { get; set; }
    public Vector2 sizeDelta { get; set; }
    public Vector2 pivot { get; set; }
    public Vector3 localScale { get; set; }
    public SVGAsset structure { get; set; }
    public SVGAsset armor { get; set; }

    public void Update(RectTransform tr) {
      tr.anchorMin = anchorMin;
      tr.anchorMax = anchorMax;
      tr.anchoredPosition = anchoredPosition;
      tr.sizeDelta = sizeDelta;
      tr.pivot = pivot;
      tr.localScale = localScale;
    }
    public VehicleReadoutImage(RectTransform tr, SVGAsset s, SVGAsset a) {
      Vector3 tp = tr.localPosition;
      tp.x -= 20f;
      pos = tp;
      anchorMin = tr.anchorMin;
      anchorMax = tr.anchorMax;
      anchoredPosition = tr.anchoredPosition;
      sizeDelta = tr.sizeDelta;
      pivot = tr.pivot;
      localScale = tr.localScale;
      structure = s; armor = a;
    }
  }
  [HarmonyPatch(typeof(StatTooltipData))]
  [HarmonyPatch("SetHeatData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class StatTooltipData_SetHeatData {
    public static bool Prefix(StatTooltipData __instance, MechDef def) {
      if (def == null) { return true; }
      if (def.Chassis == null) { return true; }
      if (def.Chassis.IsFake(def.ChassisID) == false) { return true; }
      Log.TWL(0, "StatTooltipData.SetHeatData " + def.ChassisID);
      __instance.dataList.Add(Strings.T("Heat Sinking"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Alpha Strike"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Avg. Jump Heat"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Shutdown"), Strings.T("N/A"));
      return false;
    }
  }
  [HarmonyPatch(typeof(TooltipPrefab_Mech))]
  [HarmonyPatch("SetData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(object) })]
  public static class TooltipPrefab_Mech_SetData {
    public static void Postfix(TooltipPrefab_Mech __instance, object data,ref LanceStatGraphic ___MeleeBar,ref LanceStatGraphic ___HeatEffBar) {
      ___MeleeBar.gameObject.SetActive(true);
      ___HeatEffBar.gameObject.SetActive(true);
      MechDef mechDef = data as MechDef;
      if (mechDef == null) { return; };
      if (mechDef.Chassis == null) { return; }
      if (mechDef.Chassis.IsFake(mechDef.ChassisID) == false) { return; }
      ___MeleeBar.gameObject.SetActive(false);
      ___HeatEffBar.gameObject.SetActive(false);
    }
  }
  [HarmonyPatch(typeof(SG_Shop_FullMechDetailPanel))]
  [HarmonyPatch("FillInFullMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class SG_Shop_FullMechDetailPanel_FillInFullMech {
    public static void Postfix(SG_Shop_FullMechDetailPanel __instance, MechDef theMech, ref LanceStat ___Stat4, ref LanceStat ___Stat6) {
      VerticalLayoutGroup layout = ___Stat4.transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
      if (theMech.Chassis.IsFake(theMech.ChassisID) == false) {
        layout.childControlHeight = true;
        ___Stat4.gameObject.SetActive(true);
        ___Stat6.gameObject.SetActive(true);
      } else {
        layout.childControlHeight = false;
        ___Stat4.gameObject.SetActive(false);
        ___Stat6.gameObject.SetActive(false);
      }
    }
  }
  [HarmonyPatch(typeof(InventoryDataObject_ShopFullMech))]
  [HarmonyPatch("RefreshInfoOnWidget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(InventoryItemElement) })]
  public static class InventoryDataObject_ShopFullMech_RefreshInfoOnWidget {
    public static void Postfix(InventoryDataObject_ShopFullMech __instance, InventoryItemElement theWidget) {
      if (__instance.mechDef.IsChassisFake()) { theWidget.manufacturerName.SetText("{0}", "__/VEHICLE/__"); }
    }
  }
  [HarmonyPatch(typeof(InventoryDataObject_SalvageFullMech))]
  [HarmonyPatch("RefreshInfoOnWidget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(InventoryItemElement) })]
  public static class InventoryDataObject_SalvageFullMech_RefreshInfoOnWidget {
    public static void Postfix(InventoryDataObject_ShopFullMech __instance, InventoryItemElement theWidget) {
      if (__instance.mechDef.IsChassisFake()) { theWidget.manufacturerName.SetText("{0}", "__/VEHICLE/__"); }
    }
  }
  [HarmonyPatch(typeof(StatTooltipData))]
  [HarmonyPatch("SetMeleeData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class StatTooltipData_SetMeleeData {
    public static bool Prefix(StatTooltipData __instance, MechDef def) {
      if (def == null) { return true; }
      if (def.Chassis == null) { return true; }
      if (def.Chassis.IsFake(def.ChassisID) == false) { return true; }
      Log.TWL(0, "StatTooltipData.SetHeatData " + def.ChassisID);
      __instance.dataList.Add(Strings.T("Base Dmg"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Chassis Quirk"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Total Dmg"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("Stability Dmg"), Strings.T("N/A"));
      __instance.dataList.Add(Strings.T("DFA Dmg"), Strings.T("N/A"));
      return false;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("GetInitialStructureForLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(ChassisLocations) })]
  public static class HUDMechArmorReadout_GetInitialStructureForLocation {
    public static bool Prefix(MechDef curMech, ChassisLocations location,ref float __result) {
      if (curMech == null) { return true; }
      if (curMech.Chassis == null) { return true; }
      if (curMech.Chassis.IsFake(curMech.ChassisID) == false) { return true; }
      Log.TWL(0, "HUDMechArmorReadout.GetInitialStructureForLocation " + curMech.ChassisID + " location:"+location);
      if (location == ChassisLocations.Head) {
        if (curMech.Chassis.Head.InternalStructure <= 1.0f) { __result = 0f; return false; }
      }
      if (location == ChassisLocations.LeftTorso) { __result = 0f; return false; }
      if (location == ChassisLocations.RightTorso) { __result = 0f; return false; }
      if (location == ChassisLocations.CenterTorso) { __result = 0f; return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("GetCurrentStructureForLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(ArmorLocation) })]
  public static class HUDMechArmorReadout_GetCurrentStructureForLocation {
    public static bool Prefix(MechDef curMech, ArmorLocation location, ref float __result) {
      if (curMech == null) { return true; }
      if (curMech.Chassis == null) { return true; }
      if (curMech.Chassis.IsFake(curMech.ChassisID) == false) { return true; }
      Log.TWL(0, "HUDMechArmorReadout.GetCurrentStructureForLocation " + curMech.ChassisID + " location:" + location);
      if(location == ArmorLocation.Head) {
        if (curMech.Chassis.Head.InternalStructure <= 1.0f) { __result = 0f; return false; }
      }
      if (location == ArmorLocation.LeftTorso) { __result = 0f; return false; }
      if (location == ArmorLocation.RightTorso) { __result = 0f; return false; }
      if (location == ArmorLocation.CenterTorso) { __result = 0f; return false; }
      if (location == ArmorLocation.LeftTorsoRear) { __result = 0f; return false; }
      if (location == ArmorLocation.RightTorsoRear) { __result = 0f; return false; }
      if (location == ArmorLocation.CenterTorsoRear) { __result = 0f; return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsAnyStructureExposed")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsAnyStructureExposed {
    public static void Postfix(Mech __instance, ref bool __result) {
      if (__result == false) { return; }
      if (__instance.MechDef == null) { return; }
      if (__instance.MechDef.Chassis == null) { return; }
      if (__instance.MechDef.Chassis.IsFake(__instance.MechDef.ChassisID) == false) { return; }
      Log.TWL(0, "Mech.IsAnyStructureExposed " + __instance.MechDef.ChassisID);
      __result = (__instance.LeftArmArmor <= 0f) || (__instance.RightArmArmor <= 0f) || (__instance.LeftLegArmor <= 0f) || (__instance.RightLegArmor <= 0f);
      if(__result == false) {
        if(__instance.MechDef.Chassis.Head.InternalStructure > 1.0f) {
          __result = __result || (__instance.HeadArmor <= 0f);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("SummaryStructureCurrent")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_SummaryStructureCurrent {
    public static void Postfix(Mech __instance, ref float __result) {
      if (__result == 0f) { return; }
      if (__instance.MechDef == null) { return; }
      if (__instance.MechDef.Chassis == null) { return; }
      if (__instance.MechDef.Chassis.IsFake(__instance.MechDef.ChassisID) == false) { return; }
      Log.TWL(0, "Mech.SummaryStructureCurrent " + __instance.MechDef.ChassisID);
      __result -= (__instance.LeftTorsoStructure + __instance.CenterTorsoStructure + __instance.RightTorsoStructure);
      if (__instance.MechDef.Chassis.Head.InternalStructure <= 1.0f) {
        __result -= __instance.MechDef.Chassis.Head.InternalStructure;
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("MechDefCurrentStructure")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_MechDefCurrentStructure {
    public static void Postfix(MechDef __instance, ref float __result) {
      if (__instance.Chassis == null) { return; }
      if (__instance.Chassis.IsFake(__instance.ChassisID) == false) { return; }
      Log.TWL(0, "Mech.MechDefCurrentStructure " + __instance.ChassisID);
      __result -= (__instance.LeftTorso.CurrentInternalStructure + __instance.CenterTorso.CurrentInternalStructure + __instance.RightTorso.CurrentInternalStructure);
      if (__instance.Chassis.Head.InternalStructure <= 1.0f) {
        __result -= __instance.Chassis.Head.InternalStructure;
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("MechDefMaxStructure")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_MechDefMaxStructure {
    public static void Postfix(MechDef __instance, ref float __result) {
      if (__instance.Chassis == null) { return; }
      if (__instance.Chassis.IsFake(__instance.ChassisID) == false) { return; }
      Log.TWL(0, "Mech.MechDefMaxStructure " + __instance.ChassisID);
      __result -= (__instance.Chassis.LeftTorso.InternalStructure + __instance.Chassis.CenterTorso.InternalStructure + __instance.Chassis.RightTorso.InternalStructure);
      if (__instance.Chassis.Head.InternalStructure <= 1.0f) {
        __result -= __instance.Chassis.Head.InternalStructure;
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("ResetArmorStructureBars")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class HUDMechArmorReadout_ResetArmorStructureBars {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech != null) { return true; }
      if (__instance.DisplayedMechDef == null) { return true; }
      if(__instance.DisplayedMechDef.Chassis == null) { return true; }
      Log.TWL(0, "HUDMechArmorReadout.ResetArmorStructureBars "+ __instance.DisplayedMechDef.ChassisID);
      if (__instance.DisplayedMechDef.Chassis.IsFake(__instance.DisplayedMechDef.ChassisID) == false) { return true; }
      float current1 = 0.0f;
      float max1 = 1f;
      float current2 = 0.0f;
      float max2 = 1f;
      bool exposed = true;
      current1 = __instance.DisplayedMechDef.Head.CurrentArmor + __instance.DisplayedMechDef.LeftArm.CurrentArmor + __instance.DisplayedMechDef.RightArm.CurrentArmor + __instance.DisplayedMechDef.LeftLeg.CurrentArmor + __instance.DisplayedMechDef.RightLeg.CurrentArmor;
      max1 = __instance.DisplayedMechDef.Head.AssignedArmor + __instance.DisplayedMechDef.LeftArm.AssignedArmor + __instance.DisplayedMechDef.RightArm.AssignedArmor + __instance.DisplayedMechDef.LeftLeg.AssignedArmor + __instance.DisplayedMechDef.RightLeg.AssignedArmor;
      current2 = __instance.DisplayedMechDef.LeftArm.CurrentInternalStructure + __instance.DisplayedMechDef.RightArm.CurrentInternalStructure + __instance.DisplayedMechDef.LeftLeg.CurrentInternalStructure + __instance.DisplayedMechDef.RightLeg.CurrentInternalStructure;
      max2 = __instance.DisplayedMechDef.Chassis.LeftArm.InternalStructure + __instance.DisplayedMechDef.Chassis.RightArm.InternalStructure + __instance.DisplayedMechDef.Chassis.LeftLeg.InternalStructure + __instance.DisplayedMechDef.Chassis.RightLeg.InternalStructure;
      exposed =  __instance.DisplayedMechDef.LeftArm.CurrentArmor <= 0f || __instance.DisplayedMechDef.RightArm.CurrentArmor <= 0.0 || __instance.DisplayedMechDef.LeftLeg.CurrentArmor <= 0.0 || __instance.DisplayedMechDef.RightLeg.CurrentArmor <= 0.0;
      if(__instance.DisplayedMechDef.Chassis.Head.InternalStructure > 1.0f) {
        current2 += __instance.DisplayedMechDef.Head.CurrentInternalStructure;
        max2 += __instance.DisplayedMechDef.Chassis.Head.InternalStructure;
        exposed = exposed || (__instance.DisplayedMechDef.Head.CurrentArmor <= 0f);
      }
      if (((UnityEngine.Object)__instance.ArmorBar == (UnityEngine.Object)null) || ((UnityEngine.Object)__instance.StructureBar == (UnityEngine.Object)null)){
        return false;
      }
      Log.WL(1,"structure "+ current2+"/"+max2);
      __instance.ArmorBar.ShowNewSummary(current1, max1, false);
      __instance.StructureBar.ShowNewSummary(current2, max2, exposed);
      return false;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateArmorStructureBars")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class HUDMechArmorReadout_UpdateArmorStructureBars {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech != null) { return true; }
      if (__instance.DisplayedMechDef == null) { return true; }
      if (__instance.DisplayedMechDef.Chassis == null) { return true; }
      Log.TWL(0, "HUDMechArmorReadout.UpdateArmorStructureBars " + __instance.DisplayedMechDef.ChassisID);
      if (__instance.DisplayedMechDef.Chassis.IsFake(__instance.DisplayedMechDef.ChassisID) == false) { return true; }
      float current1 = 0.0f;
      float current2 = 0.0f;
      bool exposed = true;
      current1 = __instance.DisplayedMechDef.Head.CurrentArmor + __instance.DisplayedMechDef.LeftArm.CurrentArmor + __instance.DisplayedMechDef.RightArm.CurrentArmor + __instance.DisplayedMechDef.LeftLeg.CurrentArmor + __instance.DisplayedMechDef.RightLeg.CurrentArmor;
      current2 = __instance.DisplayedMechDef.LeftArm.CurrentInternalStructure + __instance.DisplayedMechDef.RightArm.CurrentInternalStructure + __instance.DisplayedMechDef.LeftLeg.CurrentInternalStructure + __instance.DisplayedMechDef.RightLeg.CurrentInternalStructure;
      exposed =  __instance.DisplayedMechDef.LeftArm.CurrentArmor <= 0f || __instance.DisplayedMechDef.RightArm.CurrentArmor <= 0.0 || __instance.DisplayedMechDef.LeftLeg.CurrentArmor <= 0.0 || __instance.DisplayedMechDef.RightLeg.CurrentArmor <= 0.0;
      if (__instance.DisplayedMechDef.Chassis.Head.InternalStructure > 1.0f) {
        current2 += __instance.DisplayedMechDef.Head.CurrentInternalStructure;
        exposed = exposed || (__instance.DisplayedMechDef.Head.CurrentArmor <= 0f);
      }
      if (((UnityEngine.Object)__instance.ArmorBar == (UnityEngine.Object)null) || ((UnityEngine.Object)__instance.StructureBar == (UnityEngine.Object)null)) {
        return false;
      }
      Log.WL(1, "structure " + current2);
      __instance.ArmorBar.UpdateSummary(current1, false);
      __instance.StructureBar.UpdateSummary(current2, exposed);
      return false;
    }
  }
  [HarmonyPatch(typeof(MechDetails))]
  [HarmonyPatch("SetStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDetails_SetStats {
    public static void Postfix(LanceMechEquipmentList __instance, List<LanceStat> ___statList, MechDef ___activeMech) {
      Log.TWL(0, "MechDetails.SetStats " + ___activeMech.ChassisID);
      VerticalLayoutGroup layout = ___statList[1].transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
      if (___activeMech.Chassis.IsFake(___activeMech.ChassisID) == false) {
        layout.childControlHeight = true;
        ___statList[1].gameObject.SetActive(true);
        ___statList[7].gameObject.SetActive(true);
      } else {
        layout.childControlHeight = false;
        ___statList[1].gameObject.SetActive(false);
        ___statList[7].gameObject.SetActive(false);
      }
    }
  }
  [HarmonyPatch(typeof(MechBayMechInfoWidget))]
  [HarmonyPatch("OnMechLabClicked")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayMechInfoWidget_OnMechLabClicked {
    public static bool Prefix(MechBayMechInfoWidget __instance,ref MechDef ___selectedMech) {
      if (___selectedMech == null) { return true; }
      if (___selectedMech.Chassis == null) { return true; }
      if (___selectedMech.Chassis.IsFake(___selectedMech.ChassisID) == false) { return true; }
      GenericPopupBuilder.Create("Cannot Refit vehicle", Strings.T("Vehicles can't be refited")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      return false;
    }
  }
  [HarmonyPatch(typeof(MechBayMechInfoWidget))]
  [HarmonyPatch("OnUnreadyClicked")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayMechInfoWidget_OnUnreadyClicked {
    public static bool Prefix(MechBayMechInfoWidget __instance, ref MechDef ___selectedMech) {
      if (___selectedMech == null) { return true; }
      if (___selectedMech.Chassis == null) { return true; }
      if (___selectedMech.Chassis.IsFake(___selectedMech.ChassisID) == false) { return true; }
      GenericPopupBuilder.Create("Cannot store vehicle", Strings.T("Vehicles can't be stored")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      return false;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("IsFriendly")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class AbstractActor_IsFriendly {
    public static bool Prefix(AbstractActor __instance, ICombatant target, ref bool __result) {
      if(__instance.team == null) {
        //Log.TWL(0, "AbstractActor.IsFriendly "+new Localize.Text(__instance.DisplayName).ToString() +" without team:"+__instance.TeamId);
        if (target.team == null) {
          //Log.WL(1, "target have no team too " + new Localize.Text(target.DisplayName).ToString());
          __result = false;
          return false;
        } else {
          __result = __instance.TeamId == target.team.GUID;
          return false;
        }
      }
      if(target.team == null) {
        //Log.TWL(0, "AbstractActor.IsFriendly target:" + new Localize.Text(target.DisplayName).ToString());
        __result = false;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechBayMechInfoWidget))]
  [HarmonyPatch("OnRepairClicked")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayMechInfoWidget_OnRepairClicked {
    public static bool Prefix(MechBayMechInfoWidget __instance, ref MechDef ___selectedMech,ref MechBayMechUnitElement ___selectedMechElement) {
      if (___selectedMech == null) { return true; }
      if (___selectedMech.Chassis == null) { return true; }
      if (___selectedMech.Chassis.IsFake(___selectedMech.ChassisID) == false) { return true; }
      if (___selectedMechElement.inMaintenance) {
        GenericPopupBuilder.Create("Cannot Repair Vehicle", Strings.T("This 'Vehicle is already under maintenance. You must first cancel the existing task in order to begin repairs on this 'Vehicle.")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(LanceConfiguratorPanel), "CreateLanceConfiguration")]
  public static class LanceConfiguratorPanel_CreateLanceConfiguration {
    static bool Prefix(LanceConfiguratorPanel __instance, ref LanceConfiguration __result) {
      try {
        return false;
      } catch (Exception) {
        return false;
      }
    }

    static void Postfix(LanceConfiguratorPanel __instance, ref LanceConfiguration __result, ref LanceLoadoutSlot[] ___loadoutSlots) {
      try {
        Log.TWL(0, "LanceConfiguratorPanel.CreateLanceConfiguration");
        LanceConfiguration lanceConfiguration = new LanceConfiguration();
        //List<KeyValuePair<LanceLoadoutSlot,int>> mechSlots = new List<KeyValuePair<LanceLoadoutSlot, int>>();
        //List<KeyValuePair<LanceLoadoutSlot, int>> vehicleSlots = new List<KeyValuePair<LanceLoadoutSlot, int>>();
        int vehcilesCount = 0;
        int mechesCount = 0;
        CustomLanceHelper.playerLanceLoadout.loadout.Clear();
        for (int i = 0; i < ___loadoutSlots.Length; i++) {
          LanceLoadoutSlot lanceLoadoutSlot = ___loadoutSlots[i];
          bool isVehicle = false;
          if (lanceLoadoutSlot.SelectedMech == null) { continue; }
          if (lanceLoadoutSlot.SelectedPilot == null) { continue; }
          if (lanceLoadoutSlot.SelectedMech.MechDef.IsChassisFake() && (lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef.canPilotVehicle())) {
            isVehicle = true;
          } else if (lanceLoadoutSlot.SelectedMech.MechDef.IsChassisFake()&&(!lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef.canPilotVehicle())) {
            continue;
          } else if ((!lanceLoadoutSlot.SelectedMech.MechDef.IsChassisFake())&&(!lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef.canPilotMech())) {
            continue;
          } else {
            isVehicle = false;
          }
          bool isPlayer = true;
          if (isVehicle) {
            if(CustomLanceHelper.playerControlVehicles() == -1) {
              isPlayer = true;
            } else if(vehcilesCount >= CustomLanceHelper.playerControlVehicles()) {
              isPlayer = false;
            } else {
              isPlayer = true;
            }
            ++vehcilesCount;
          } else {
            if (CustomLanceHelper.playerControlMechs() == -1) {
              isPlayer = true;
            } else if (mechesCount >= CustomLanceHelper.playerControlMechs()) {
              isPlayer = false;
            } else {
              isPlayer = true;
            }
            ++mechesCount;
          }
          if (isPlayer) {
            lanceConfiguration.AddUnit(__instance.playerGUID, lanceLoadoutSlot.SelectedMech.MechDef, lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef);
          } else {
            lanceConfiguration.AddUnit(Core.Settings.EMPLOYER_LANCE_GUID, lanceLoadoutSlot.SelectedMech.MechDef, lanceLoadoutSlot.SelectedPilot.Pilot.pilotDef);
          }
          CustomLanceHelper.playerLanceLoadout.loadout.Add(lanceLoadoutSlot.SelectedMech.MechDef.GUID, i);
        }
        __result = lanceConfiguration;
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(MechBayMechInfoWidget))]
  [HarmonyPatch("OnScrapClicked")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayMechInfoWidget_OnScrapClicked {
    internal class d_MechBayMechInfoWidget {
      public MechBayMechInfoWidget widget { get; set; }
      public void ConfirmScrapClicked() {
        typeof(MechBayMechInfoWidget).GetMethod("ConfirmScrapClicked", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(widget, new object[] { });
      }
      public d_MechBayMechInfoWidget(MechBayMechInfoWidget i) { widget = i; }
    }
    public static bool Prefix(MechBayMechInfoWidget __instance, ref MechDef ___selectedMech, ref MechBayMechUnitElement ___selectedMechElement) {
      if (___selectedMech == null) { return true; }
      if (___selectedMech.Chassis == null) { return true; }
      if (___selectedMech.Chassis.IsFake(___selectedMech.ChassisID) == false) { return true; }
      if (___selectedMechElement.inMaintenance) {
        GenericPopupBuilder.Create("Cannot Scrap Vehicle", Strings.T("This 'Vehicle is already under maintenance. You must first cancel the existing task in order to scrap this 'Vehicle.")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      } else {
        GenericPopupBuilder.Create(Strings.T("Scrap 'Vehicle - {0}", (object)___selectedMech.Description.Name), Strings.T("Are you sure you want to scrap this 'Vehicle?\n\nThis 'Vehicle's components will be stored and its chassis removed permanently from your inventory.\n\nSCRAP VALUE: <color=#F79B26FF>{0}</color>", (object)SimGameState.GetCBillString(Mathf.RoundToInt((float)___selectedMech.Chassis.Description.Cost * __instance.sim.Constants.Finances.MechScrapModifier)))).AddButton("Cancel", (Action)null, true, (PlayerAction)null).AddButton("Scrap", new Action(new d_MechBayMechInfoWidget(__instance).ConfirmScrapClicked), true, (PlayerAction)null).CancelOnEscape().AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("CompleteContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MissionResult), typeof(bool) })]
  public static class Contract_CompleteContract {
    private static MethodInfo m_pilot = typeof(Mech).GetProperty("pilot").GetSetMethod(true);
    public static void pilot(this Mech mech, Pilot pilot) { m_pilot.Invoke(mech, new object[1] { pilot }); }

    private static MethodInfo m_ComponentDefType = typeof(BaseComponentRef).GetProperty("ComponentDefType").GetSetMethod(true);
    public static void ComponentDefType(this BaseComponentRef def, ComponentType type) { m_ComponentDefType.Invoke(def, new object[1] { type }); }

    private static MethodInfo m_HardpointSlot = typeof(BaseComponentRef).GetProperty("HardpointSlot").GetSetMethod(true);
    public static void HardpointSlot(this BaseComponentRef def, int slot) { m_HardpointSlot.Invoke(def, new object[1] { slot }); }

    private static MethodInfo m_IsFixed = typeof(BaseComponentRef).GetProperty("IsFixed").GetSetMethod(true);
    public static void IsFixed(this BaseComponentRef def, bool value) { m_IsFixed.Invoke(def, new object[1] { value }); }

    private static MethodInfo m_MountedLocation = typeof(MechComponentRef).GetProperty("MountedLocation").GetSetMethod(true);
    public static void MountedLocation(this MechComponentRef def, ChassisLocations loc) { m_MountedLocation.Invoke(def, new object[1] { loc }); }

    private static MethodInfo m_setDescription = typeof(ActorDef).GetProperty("Description").GetSetMethod(true);
    public static void Description(this ActorDef def, DescriptionDef descr) { m_setDescription.Invoke(def, new object[1] { descr }); }

    private static FieldInfo f_chassisID = typeof(PilotableActorDef).GetField("chassisID", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void chassisID(this PilotableActorDef def, string chassisID) { f_chassisID.SetValue(def, chassisID); }

    //private static FieldInfo f_heraldryID = typeof(PilotableActorDef).GetField("heraldryID", BindingFlags.Instance | BindingFlags.NonPublic);
    //public static void heraldryID(this PilotableActorDef def, string heraldryID) { f_heraldryID.SetValue(def, heraldryID); }
    private static MethodInfo m_MechTags = typeof(MechDef).GetProperty("MechTags").GetSetMethod(true);
    public static void MechTags_set(this MechDef vDef, TagSet value) { m_MechTags.Invoke(vDef, new object[1] { value }); }

    private static FieldInfo f_simGameMechPartCost = typeof(MechDef).GetField("simGameMechPartCost", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void simGameMechPartCost(this MechDef def, int value) { f_simGameMechPartCost.SetValue(def, value); }

    private static FieldInfo f_inventory = typeof(MechDef).GetField("inventory", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void inventory(this MechDef def, MechComponentRef[] value) { f_inventory.SetValue(def, value); }
    public static MechComponentRef[] inventory(this MechDef def) { return (MechComponentRef[])f_inventory.GetValue(def); }

    private static FieldInfo f_Locations = typeof(MechDef).GetField("Locations", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Locations(this MechDef def, LocationLoadoutDef[] value) { f_Locations.SetValue(def, value); }
    public static LocationLoadoutDef[] Locations(this MechDef def) { return (LocationLoadoutDef[])f_Locations.GetValue(def); }

    private static MethodInfo m_CreateMeleeWeaponRefs = typeof(MechDef).GetMethod("CreateMeleeWeaponRefs", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void CreateMeleeWeaponRefs(this MechDef def) { m_CreateMeleeWeaponRefs.Invoke(def, new object[] { }); }
    private static MethodInfo m_InsertFixedEquipmentIntoInventory = typeof(MechDef).GetMethod("InsertFixedEquipmentIntoInventory", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void InsertFixedEquipmentIntoInventory(this MechDef def) { m_InsertFixedEquipmentIntoInventory.Invoke(def, new object[] { }); }
    private static MethodInfo m_RefreshLocationReferences = typeof(MechDef).GetMethod("RefreshLocationReferences", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void RefreshLocationReferences(this MechDef def) { m_RefreshLocationReferences.Invoke(def, new object[] { }); }

    private static FieldInfo f_hasHandledDeath = typeof(AbstractActor).GetField("_hasHandledDeath", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void _hasHandledDeath(this AbstractActor def, bool value) { f_hasHandledDeath.SetValue(def, value); }

    private static FieldInfo f_team = typeof(AbstractActor).GetField("_team", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void _team(this AbstractActor def, Team value) { f_team.SetValue(def, value); }

    private static FieldInfo f_teamId = typeof(AbstractActor).GetField("_teamId", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void _teamId(this AbstractActor def, string value) { f_teamId.SetValue(def, value); }
    public static Mech ToMech(this Vehicle src) {
      MechComponentRef[] inventory = new MechComponentRef[src.allComponents.Count];
      for (int index = 0; index < src.allComponents.Count; ++index) {
        inventory[index] = new MechComponentRef();
        inventory[index].DataManager = src.allComponents[index].vehicleComponentRef.DataManager;
        inventory[index].ComponentDefType(src.allComponents[index].vehicleComponentRef.ComponentDefType);
        inventory[index].HardpointSlot(src.allComponents[index].vehicleComponentRef.HardpointSlot);
        inventory[index].DamageLevel = (src.allComponents[index].DamageLevel == ComponentDamageLevel.Destroyed?ComponentDamageLevel.Penalized:src.allComponents[index].DamageLevel);
        inventory[index].prefabName = src.allComponents[index].vehicleComponentRef.prefabName;
        inventory[index].hasPrefabName = src.allComponents[index].vehicleComponentRef.hasPrefabName;
        inventory[index].IsFixed(true);
        inventory[index].ComponentDefID = src.allComponents[index].vehicleComponentRef.ComponentDefID;
        inventory[index].SimGameUID = src.allComponents[index].vehicleComponentRef.SimGameUID;
        ChassisLocations loc = ChassisLocations.Head;
        switch (src.allComponents[index].vehicleComponentRef.MountedLocation) {
          case VehicleChassisLocations.Turret: loc = ChassisLocations.Head; break;
          case VehicleChassisLocations.Left: loc = ChassisLocations.LeftLeg; break;
          case VehicleChassisLocations.Right: loc = ChassisLocations.RightLeg; break;
          case VehicleChassisLocations.Front: loc = ChassisLocations.LeftArm; break;
          case VehicleChassisLocations.Rear: loc = ChassisLocations.RightArm; break;
        }
        inventory[index].MountedLocation(loc);
        inventory[index].RefreshComponentDef();
      }
      MechDef dest = new MechDef(src.VehicleDef.Description, src.VehicleDef.ChassisID, inventory, src.VehicleDef.DataManager);
      dest.DataManager = src.VehicleDef.DataManager;
      dest.Description(src.VehicleDef.Description);
      dest.chassisID(src.VehicleDef.ChassisID);
      dest.Chassis = src.VehicleDef.DataManager.ChassisDefs.Get(src.VehicleDef.ChassisID);
      dest.prefabOverride = string.Empty;
      dest.paintTextureID(src.VehicleDef.PaintTextureID);
      dest.heraldryID(src.VehicleDef.HeraldryID);
      dest.heraldryDef(src.VehicleDef.HeraldryDef);
      dest.MechTags_set(new TagSet(src.VehicleDef.VehicleTags));
      dest.simGameMechPartCost(src.VehicleDef.Description.Cost);
      LocationLoadoutDef[] locations = new LocationLoadoutDef[8];
      if (src.VehicleDef.Chassis.HasTurret) {
        locations[0] = new LocationLoadoutDef(ChassisLocations.Head, src.VehicleDef.Turret.CurrentArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle, -1f, src.VehicleDef.Turret.CurrentInternalStructure, src.VehicleDef.Turret.AssignedArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle, -1f, src.VehicleDef.Turret.DamageLevel);
      } else {
        locations[0] = new LocationLoadoutDef(ChassisLocations.Head, 0f, -1f, 1f, 0f, -1f, LocationDamageLevel.Functional);
      }
      if (src.IsDead) {
        locations[1] = new LocationLoadoutDef(ChassisLocations.CenterTorso, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, LocationDamageLevel.Destroyed);
        locations[2] = new LocationLoadoutDef(ChassisLocations.LeftTorso, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, LocationDamageLevel.Destroyed);
        locations[3] = new LocationLoadoutDef(ChassisLocations.RightTorso, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, LocationDamageLevel.Destroyed);
      } else {
        locations[1] = new LocationLoadoutDef(ChassisLocations.CenterTorso, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, LocationDamageLevel.Functional);
        locations[2] = new LocationLoadoutDef(ChassisLocations.LeftTorso, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, LocationDamageLevel.Functional);
        locations[3] = new LocationLoadoutDef(ChassisLocations.RightTorso, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, LocationDamageLevel.Functional);
      }
      locations[4] = new LocationLoadoutDef(ChassisLocations.LeftArm, Mathf.Round(src.VehicleDef.Front.CurrentArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, Mathf.Round(src.VehicleDef.Front.CurrentInternalStructure), Mathf.Round(src.VehicleDef.Front.AssignedArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, src.VehicleDef.Front.DamageLevel);
      locations[5] = new LocationLoadoutDef(ChassisLocations.RightArm, Mathf.Round(src.VehicleDef.Rear.CurrentArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, Mathf.Round(src.VehicleDef.Rear.CurrentInternalStructure), Mathf.Round(src.VehicleDef.Rear.AssignedArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, src.VehicleDef.Rear.DamageLevel);
      locations[6] = new LocationLoadoutDef(ChassisLocations.LeftLeg, Mathf.Round(src.VehicleDef.Left.CurrentArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, Mathf.Round(src.VehicleDef.Left.CurrentInternalStructure), Mathf.Round(src.VehicleDef.Left.AssignedArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, src.VehicleDef.Left.DamageLevel);
      locations[7] = new LocationLoadoutDef(ChassisLocations.RightLeg, Mathf.Round(src.VehicleDef.Right.CurrentArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, Mathf.Round(src.VehicleDef.Right.CurrentInternalStructure), Mathf.Round(src.VehicleDef.Right.AssignedArmor * src.Combat.Constants.CombatValueMultipliers.ArmorMultiplierVehicle), -1f, src.VehicleDef.Right.DamageLevel);
      dest.SetLocations(locations);
      dest.CreateMeleeWeaponRefs();
      dest.InsertFixedEquipmentIntoInventory();
      dest.SetGuid(src.VehicleDef.GUID);
      dest.RefreshLocationReferences();
      dest.RefreshBattleValue();
      Mech result = new Mech(dest, src.pilot.pilotDef, src.EncounterTags, src.GUID, src.Combat, src.spawnerGUID, src.CustomHeraldryDef);
      result.pilot(src.pilot);
      result.Init(src.CurrentPosition, 0f, false);
      result._hasHandledDeath(src.IsDead);
      if (src.IsDead) {
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.CenterTorso), 0f);
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.CenterTorso), LocationDamageLevel.Destroyed);
        result.pilot.StatCollection.Set("LethalInjury",true);
      }
      result._team(src.team);
      result._teamId(src.TeamId);
      if (src.VehicleDef.Chassis.HasTurret) {
        result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.Head), Mathf.Round(src.TurretArmor));
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.Head), Mathf.Round(src.TurretStructure));
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.Head), src.TurretDamageLevel);
      } else {
        result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.Head),0f);
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.Head), 1f);
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.Head), LocationDamageLevel.Functional);
      }
      result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.LeftArm), Mathf.Round(src.FrontArmor));
      result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.LeftArm), Mathf.Round(src.FrontStructure));
      result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.LeftArm), src.FrontDamageLevel);

      result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.RightArm), Mathf.Round(src.RearArmor));
      result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.RightArm), Mathf.Round(src.RearStructure));
      result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.RightArm), src.RearDamageLevel);

      result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.LeftLeg), Mathf.Round(src.LeftSideArmor));
      result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.LeftLeg), Mathf.Round(src.LeftSideStructure));
      result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.LeftLeg), src.LeftSideDamageLevel);

      result.StatCollection.Set<float>(result.GetStringForArmorLocation(ArmorLocation.RightLeg), Mathf.Round(src.RightSideArmor));
      result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.RightLeg), Mathf.Round(src.RightSideStructure));
      result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.RightLeg), src.RightSideDamageLevel);

      if (src.IsDead) {
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.CenterTorso), 0f);
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.CenterTorso), LocationDamageLevel.Destroyed);
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.RightTorso), 0f);
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.RightTorso), LocationDamageLevel.Destroyed);
        result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.LeftTorso), 0f);
        result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.LeftTorso), LocationDamageLevel.Destroyed);
      }
      Log.TWL(0, "Vehicle.ToMech:" + result.Description.Id+" GUID:"+result.MechDef.GUID+" isDead:"+src.IsDead+"/"+result.IsDead+" isDestroyed:"+result.ToMechDef().IsDestroyed);
      Log.WL(1, "Head def: aa:"+result.MechDef.Head.AssignedArmor+" ca:"+result.MechDef.Head.CurrentArmor+" is:"+result.MechDef.Head.CurrentInternalStructure+" mech: a:"+result.HeadArmor+" is:"+result.HeadStructure);
      Log.WL(1, "CenterTorso def: aa:" + result.MechDef.CenterTorso.AssignedArmor + " ca:" + result.MechDef.CenterTorso.CurrentArmor + " is:" + result.MechDef.CenterTorso.CurrentInternalStructure + " mech: a:" + result.CenterTorsoFrontArmor + " is:" + result.CenterTorsoStructure);
      MechDef createdDef = result.ToMechDef();
      Log.WL(1, "created def CenterTorso def: aa:" + createdDef.CenterTorso.AssignedArmor + " ca:" + createdDef.CenterTorso.CurrentArmor + " is:" + createdDef.CenterTorso.CurrentInternalStructure + "/" +createdDef.IsLocationDestroyed(ChassisLocations.CenterTorso)+" mech: a:" + result.CenterTorsoFrontArmor + " is:" + result.CenterTorsoStructure);
      return result;
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      //List<CodeInstruction> result = new List<CodeInstruction>();
      //result.AddRange(instructions);
      MethodInfo getAllMechs = typeof(CombatGameState).GetProperty("AllMechs").GetMethod;
      MethodInfo getAllMechs_n = typeof(Contract_CompleteContract).GetMethod(nameof(AllMechs));
      return Transpilers.MethodReplacer(instructions, getAllMechs, getAllMechs_n);
      //return result;
    }

    public static List<Mech> AllMechs(CombatGameState combat) {
      Log.TWL(0, "Contract.CompleteContract AllMechs");
      List<Mech> allMechs = combat.AllMechs;
      List<Vehicle> playerVehicle = new List<Vehicle>();
      List<AbstractActor> allActors = combat.AllActors;
      HashSet<string> playerDefGuids = new HashSet<string>();
      foreach (var guid in CustomLanceHelper.playerLanceLoadout.loadout) { playerDefGuids.Add(guid.Key); }
      foreach (AbstractActor actor in allActors) {
        try {
          if(actor.UnitType == UnitType.Mech) {
            Mech mech = actor as Mech;
            Log.WL(1, "MechDef GUID:"+ mech.MechDef.GUID);
            if (playerDefGuids.Contains(mech.MechDef.GUID)) {
              Log.WL(2,"team:" + mech.TeamId+"->"+ combat.LocalPlayerTeamGuid);
              if (mech.TeamId != combat.LocalPlayerTeamGuid) {
                mech._teamId(combat.LocalPlayerTeamGuid);
                mech._team(combat.LocalPlayerTeam);
              }
            }
          }
          if (actor.UnitType == UnitType.Vehicle) {
            Vehicle vehicle = actor as Vehicle;
            Log.WL(1, "VehicleDef GUID:" + vehicle.VehicleDef.GUID);
            if (playerDefGuids.Contains(vehicle.VehicleDef.GUID)) {
              Log.WL(2, "team:" + vehicle.TeamId + "->" + combat.LocalPlayerTeamGuid);
              if (vehicle.TeamId != combat.LocalPlayerTeamGuid) {
                vehicle._teamId(combat.LocalPlayerTeamGuid);
                vehicle._team(combat.LocalPlayerTeam);
              }
            }
          }
          if (actor.UnitType != UnitType.Vehicle) { continue; }
          if (actor.TeamId == combat.LocalPlayerTeamGuid) {
            Vehicle vehicle = actor as Vehicle;
            if (vehicle != null) { allMechs.Add(vehicle.ToMech()); }
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        };
      }
      return allMechs;
    }
  }
  [HarmonyPatch(typeof(MechStatisticsRules))]
  [HarmonyPatch("CalculateHeatEfficiencyStat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class MechStatisticsRules_CalculateHeatEfficiencyStat {
    public static void Postfix(MechDef mechDef, ref float currentValue, ref float maxValue) {
      if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
        currentValue = 0f;
        maxValue = 10f;
      }
    }
  }
  [HarmonyPatch(typeof(MechStatisticsRules))]
  [HarmonyPatch("CalculateTonnage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class MechStatisticsRules_CalculateTonnage {
    public static void Postfix(MechDef mechDef, ref float currentValue, ref float maxValue) {
      if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
        currentValue = mechDef.Chassis.Tonnage;
      }
    }
  }
  [HarmonyPatch(typeof(MechStatisticsRules))]
  [HarmonyPatch("CalculateCBillValue")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class MechStatisticsRules_CalculateCBillValue {
    public static void Postfix(MechDef mechDef, ref float currentValue, ref float maxValue) {
      if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
        currentValue = mechDef.Description.Cost;
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("CreateMeleeOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  public static class SelectionStateMove_CreateMeleeOrders {
    public static bool Prefix(SelectionStateMove __instance) {
      if (__instance.SelectedActor == null) { return true; }
      if(__instance.SelectedActor.UnitType == UnitType.Vehicle) {
        GenericPopupBuilder.Create("Can not perform melee", Strings.T("We are very-very sorry, but vehicles can not in melee")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        return false;
      } else {
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechStatisticsRules))]
  [HarmonyPatch("CalculateMeleeStat")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechStatisticsRules_CalculateMeleeStat {
    public static bool Prefix(MechDef mechDef, ref float currentValue, ref float maxValue) {
      if (mechDef.Chassis.IsFake(mechDef.ChassisID)) {
        currentValue = 0f;
        maxValue = 10f;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechBayMechInfoWidget))]
  [HarmonyPatch("SetStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechBayMechInfoWidget_SetStats {
    public static void Postfix(LanceMechEquipmentList __instance, LanceStat[] ___mechStats, MechDef ___selectedMech) {
      if (___selectedMech == null) { return; }
      VerticalLayoutGroup layout = ___mechStats[3].transform.parent.gameObject.GetComponent<VerticalLayoutGroup>();
      if (___selectedMech.Chassis.IsFake(___selectedMech.ChassisID) == false) {
        layout.childControlHeight = true;
        ___mechStats[3].gameObject.SetActive(true);
        ___mechStats[5].gameObject.SetActive(true);
      } else {
        layout.childControlHeight = false;
        ___mechStats[3].gameObject.SetActive(false);
        ___mechStats[5].gameObject.SetActive(false);
      }
    }
  }
  [HarmonyPatch(typeof(LanceMechEquipmentList))]
  [HarmonyPatch("SetLoadout")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceMechEquipmentList_SetLoadout {
    public static void Postfix(LanceMechEquipmentList __instance,
        LocalizableText ___headLabel,
        LocalizableText ___centerTorsoLabel,
        LocalizableText ___leftTorsoLabel,
        LocalizableText ___rightTorsoLabel,
        LocalizableText ___leftArmLabel,
        LocalizableText ___rightArmLabel,
        LocalizableText ___leftLegLabel,
        LocalizableText ___rightLegLabel,
        MechDef ___activeMech
    ) {
      if (___activeMech.Chassis.IsFake(___activeMech.ChassisID) == false) {
        ___headLabel.SetText("H");
        ___centerTorsoLabel.SetText("CT"); ___centerTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftTorsoLabel.SetText("LT"); ___leftTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___rightTorsoLabel.SetText("RT"); ___rightTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftArmLabel.SetText("LA");
        ___rightArmLabel.SetText("RA");
        ___leftLegLabel.SetText("LL");
        ___rightLegLabel.SetText("RL");
      } else {
        ___headLabel.SetText("T");
        ___centerTorsoLabel.SetText("CT"); ___centerTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___leftTorsoLabel.SetText("LT"); ___leftTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___rightTorsoLabel.SetText("RT"); ___rightTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___leftArmLabel.SetText("F");
        ___rightArmLabel.SetText("R");
        ___leftLegLabel.SetText("L");
        ___rightLegLabel.SetText("R");
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD), typeof(bool), typeof(bool), typeof(bool) })]
  public static class HUDMechArmorReadout_Init_info {
    private static GameObject vehicleReadout = null;
    public static Dictionary<int, Vector3> d_originalFrontPos = new Dictionary<int, Vector3>();
    public static Dictionary<VehicleChassisLocations, VehicleReadoutImage> vehicleImages = new Dictionary<VehicleChassisLocations, VehicleReadoutImage>();
    public static Dictionary<int, List<HUDMechArmorReadoutOriginal>> d_originalStructure = new Dictionary<int, List<HUDMechArmorReadoutOriginal>>();
    public static Dictionary<int, List<HUDMechArmorReadoutOriginal>> d_originalArmor = new Dictionary<int, List<HUDMechArmorReadoutOriginal>>();
    public static Dictionary<int, List<HUDMechArmorReadoutOriginal>> d_originalArmorOutline = new Dictionary<int, List<HUDMechArmorReadoutOriginal>>();
    public static void Postfix(HUDMechArmorReadout __instance, CombatHUD HUD, bool useHoversForCalledShots, bool hideArmorWhenStructureDamage, bool showArmorAllOrNothing) {
      if (HUD != null) { return; }
      d_originalStructure.Remove(__instance.GetInstanceID());
      d_originalArmor.Remove(__instance.GetInstanceID());
      d_originalArmorOutline.Remove(__instance.GetInstanceID());
      d_originalFrontPos.Remove(__instance.GetInstanceID());
      if (HUDMechArmorReadout_Init_info.d_originalFrontPos.TryGetValue(__instance.GetInstanceID(), out Vector3 pos) == false) {
        for (int index = 0; index < __instance.Armor.Length; ++index) {
          if (__instance.Armor[index] == null) { continue; }
          HUDMechArmorReadout_Init_info.d_originalFrontPos.Add(__instance.GetInstanceID(), __instance.Armor[index].transform.parent.localPosition);
          break;
        }
      }
      if (HUDMechArmorReadout_Init_info.d_originalArmor.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalArmor) == false) {
        originalArmor = new List<HUDMechArmorReadoutOriginal>(); HUDMechArmorReadout_Init_info.d_originalArmor.Add(__instance.GetInstanceID(), originalArmor);
        for (int index = 0; index < __instance.Armor.Length; ++index) {
          if (__instance.Armor[index] == null) { originalArmor.Add(null); continue; };
          originalArmor.Add(new HUDMechArmorReadoutOriginal(
            __instance.Armor[index].rectTransform,
            __instance.Armor[index]
          ));
        }
      }
      if (HUDMechArmorReadout_Init_info.d_originalArmorOutline.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalArmorOutline) == false) {
        originalArmorOutline = new List<HUDMechArmorReadoutOriginal>(); HUDMechArmorReadout_Init_info.d_originalArmorOutline.Add(__instance.GetInstanceID(), originalArmorOutline);
        for (int index = 0; index < __instance.ArmorOutline.Length; ++index) {
          if (__instance.ArmorOutline[index] == null) { originalArmorOutline.Add(null); continue; };
          originalArmorOutline.Add(new HUDMechArmorReadoutOriginal(
            __instance.ArmorOutline[index].rectTransform,
            __instance.ArmorOutline[index]
          ));
        }
      }
      if (HUDMechArmorReadout_Init_info.d_originalStructure.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalStructure) == false) {
        originalStructure = new List<HUDMechArmorReadoutOriginal>(); HUDMechArmorReadout_Init_info.d_originalStructure.Add(__instance.GetInstanceID(), originalStructure);
        for (int index = 0; index < __instance.Structure.Length; ++index) {
          if (__instance.Structure[index] == null) { originalStructure.Add(null); continue; };
          originalStructure.Add(new HUDMechArmorReadoutOriginal(
            __instance.Structure[index].rectTransform,
            __instance.Structure[index]
          ));
        }
      }
      if (vehicleReadout == null) {
        if (UIManager.HasInstance == false) { Log.WL(1, "no UIManager instance"); return; }
        GameObject calledshot = UIManager.Instance.dataManager.PooledInstantiate("uixPrfPanl_targetingComputer", BattleTechResourceType.UIModulePrefabs);
        if (calledshot == null) { Log.WL(1, "can't instante uixPrfPanl_targetingComputer"); return; }
        //vehicleReadout = calledshot;
        //vehicleReadout.SetActive(false);
        vehicleReadout = GameObject.Instantiate(calledshot.transform.FindRecursive("tgtHud_VehicleArmorReadout").gameObject);
        vehicleReadout.transform.SetParent(__instance.gameObject.transform.parent);
        vehicleReadout.transform.localPosition = __instance.transform.localPosition;
        vehicleReadout.name = "fakeVehicleReadout";
        GameObject.Destroy(calledshot);
        vehicleReadout.SetActive(false);
        vehicleImages.Clear();
        vehicleImages.Add(VehicleChassisLocations.Front,
          new VehicleReadoutImage(
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorFront").gameObject.transform as RectTransform,
            vehicleReadout.transform.FindRecursive("Vehicle_InternalFront").gameObject.GetComponent<SVGImage>().vectorGraphics,
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorFront").gameObject.GetComponent<SVGImage>().vectorGraphics)
         );
        vehicleImages.Add(VehicleChassisLocations.Rear,
          new VehicleReadoutImage(
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorRear").gameObject.transform as RectTransform,
            vehicleReadout.transform.FindRecursive("Vehicle_InternalRear").gameObject.GetComponent<SVGImage>().vectorGraphics,
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorRear").gameObject.GetComponent<SVGImage>().vectorGraphics)
         );
        vehicleImages.Add(VehicleChassisLocations.Left,
          new VehicleReadoutImage(
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorL").gameObject.transform as RectTransform,
            vehicleReadout.transform.FindRecursive("Vehicle_InternalL").gameObject.GetComponent<SVGImage>().vectorGraphics,
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorL").gameObject.GetComponent<SVGImage>().vectorGraphics)
         );
        vehicleImages.Add(VehicleChassisLocations.Right,
          new VehicleReadoutImage(
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorR").gameObject.transform as RectTransform,
            vehicleReadout.transform.FindRecursive("Vehicle_InternalR").gameObject.GetComponent<SVGImage>().vectorGraphics,
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorR").gameObject.GetComponent<SVGImage>().vectorGraphics)
         );
        vehicleImages.Add(VehicleChassisLocations.Turret,
          new VehicleReadoutImage(
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorTurret").gameObject.transform as RectTransform,
            vehicleReadout.transform.FindRecursive("Vehicle_InternalTurret").gameObject.GetComponent<SVGImage>().vectorGraphics,
            vehicleReadout.transform.FindRecursive("Vehicle_ArmorTurret").gameObject.GetComponent<SVGImage>().vectorGraphics)
         );
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmor_info {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech != null) { return true; }
      if (__instance.DisplayedMechDef == null) { return true; }
      for (int index = 0; index < __instance.ArmorRear.Length; ++index) {
        if (__instance.ArmorRear[index] != null) { __instance.ArmorRear[index].gameObject.transform.parent.gameObject.SetActive(true); break; }
      }
      for (int index = 0; index < __instance.Armor.Length; ++index) {
        if (__instance.Armor[index] == null) { continue; }
        __instance.Armor[index].gameObject.transform.parent.localScale = Vector3.one * 0.3f;
        break;
      }
      if (HUDMechArmorReadout_Init_info.d_originalArmor.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalArmor) == false) {
        originalArmor = null;
      }
      if (HUDMechArmorReadout_Init_info.d_originalArmorOutline.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalArmorOutline) == false) {
        originalArmorOutline = null;
      }
      if (HUDMechArmorReadout_Init_info.d_originalStructure.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalStructure) == false) {
        originalStructure = null;
      }
      if (HUDMechArmorReadout_Init_info.d_originalFrontPos.TryGetValue(__instance.GetInstanceID(), out Vector3 frontLocalPos)) {
        for (int index = 0; index < __instance.Armor.Length; ++index) {
          if (__instance.Armor[index] == null) { continue; }
          __instance.Armor[index].transform.parent.localPosition = frontLocalPos;
          break;
        }
      }
      Log.TWL(0, "HUDMechArmorReadout.UpdateMechStructureAndArmor " + __instance.GetInstanceID());
      bool isFakeChassis = __instance.DisplayedMechDef.Chassis.IsFake(__instance.DisplayedMechDef.ChassisID);
      for (int index = 0; index < __instance.Armor.Length; ++index) {
        if (originalArmor == null) { break; }
        if (originalArmor.Count <= index) { break; }
        if (__instance.Armor[index] == null) { continue; };
        HUDMechArmorReadoutOriginal orig = originalArmor[index];
        if (orig == null) { continue; }
        __instance.Armor[index].gameObject.SetActive(true);
        __instance.Armor[index].vectorGraphics = orig.icon;
        __instance.Armor[index].transform.localPosition = orig.localPosition;
        orig.Restore(__instance.Armor[index].rectTransform, ref __instance.Armor[index]);
      }
      for (int index = 0; index < __instance.ArmorOutline.Length; ++index) {
        if (originalArmorOutline == null) { break; }
        if (originalArmorOutline.Count <= index) { break; }
        if (__instance.ArmorOutline[index] == null) { continue; };
        HUDMechArmorReadoutOriginal orig = originalArmorOutline[index];
        if (orig == null) { continue; }
        __instance.ArmorOutline[index].gameObject.SetActive(true);
        __instance.ArmorOutline[index].vectorGraphics = orig.icon;
        __instance.ArmorOutline[index].transform.localPosition = orig.localPosition;
        orig.Restore(__instance.ArmorOutline[index].rectTransform, ref __instance.ArmorOutline[index]);
      }
      for (int index = 0; index < __instance.Structure.Length; ++index) {
        if (originalStructure == null) { break; }
        if (originalStructure.Count <= index) { break; }
        if (__instance.Structure[index] == null) { continue; };
        HUDMechArmorReadoutOriginal orig = originalStructure[index];
        if (orig == null) { continue; }
        __instance.Structure[index].gameObject.SetActive(true);
        __instance.Structure[index].vectorGraphics = orig.icon;
        orig.Restore(__instance.Structure[index].rectTransform, ref __instance.Structure[index]);
        Log.WL(1, "[" + index + "] " + __instance.Structure[index].transform.localPosition + "->" + orig.localPosition);
        __instance.Structure[index].transform.localPosition = orig.localPosition;
        if (isFakeChassis == false) {
          if (orig.color.HasValue) {
            __instance.Structure[index].color = orig.color.Value;
            orig.color = null;
          }
        }
      }
      return true;
    }
    public static void Postfix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech != null) { return; }
      if (__instance.DisplayedMechDef == null) { return; }
      if (__instance.DisplayedMechDef.Chassis.IsFake(__instance.DisplayedMechDef.ChassisID) == false) { return; }
      Log.TWL(0, "HUDMechArmorReadout.UpdateMechStructureAndArmor");
      for (int index = 0; index < __instance.Armor.Length; ++index) {
        if (__instance.Armor[index] == null) { continue; }
        Vector3 pos = __instance.Armor[index].transform.parent.localPosition;
        pos.x -= 10f;
        __instance.Armor[index].transform.parent.localPosition = pos;
        break;
      }
      for (int index = 0; index < __instance.ArmorRear.Length; ++index) {
        if (__instance.ArmorRear[index] != null) { __instance.ArmorRear[index].gameObject.transform.parent.gameObject.SetActive(false); break; }
      }
      for (int index = 0; index < __instance.Armor.Length; ++index) {
        if (__instance.Armor[index] == null) { continue; }
        __instance.Armor[index].gameObject.transform.parent.localScale = Vector3.one * 0.5f;
        break;
      }
      for (int index = 0; index < __instance.Armor.Length; ++index) {
        if (__instance.Armor[index] == null) { continue; }
        if (index == 2) { __instance.Armor[index].gameObject.SetActive(false); continue; } else
        if (index == 3) { __instance.Armor[index].gameObject.SetActive(false); continue; } else
        if (index == 4) { __instance.Armor[index].gameObject.SetActive(false); continue; };
        VehicleChassisLocations vloc = VehicleChassisLocations.Turret;
        ArmorLocation aloc = ArmorLocation.Head;
        ChassisLocations loc = ChassisLocations.Head;
        if (index == 0) { vloc = VehicleChassisLocations.Turret; aloc = ArmorLocation.Head; loc = ChassisLocations.Head; } else
        if (index == 1) { vloc = VehicleChassisLocations.Rear; aloc = ArmorLocation.RightArm; loc = ChassisLocations.RightArm; } else
        if (index == 5) { vloc = VehicleChassisLocations.Front; aloc = ArmorLocation.LeftArm; loc = ChassisLocations.LeftArm; } else
        if (index == 6) { vloc = VehicleChassisLocations.Right; aloc = ArmorLocation.RightLeg; loc = ChassisLocations.RightLeg; } else
        if (index == 7) { vloc = VehicleChassisLocations.Left; aloc = ArmorLocation.LeftLeg; loc = ChassisLocations.LeftLeg; }
        if ((vloc == VehicleChassisLocations.Turret) && (__instance.DisplayedMechDef.Chassis.Head.InternalStructure <= 1.0f)) { __instance.Armor[index].gameObject.SetActive(false); continue; }
        float curAVal = HUDMechArmorReadout.GetCurrentArmorForLocation(__instance.DisplayedMechDef, aloc);
        float intAVal = HUDMechArmorReadout.GetInitialArmorForLocation(__instance.DisplayedMechDef, aloc);
        float curSVal = HUDMechArmorReadout.GetCurrentStructureForLocation(__instance.DisplayedMechDef, aloc);
        float intSVal = HUDMechArmorReadout.GetInitialStructureForLocation(__instance.DisplayedMechDef, loc);
        if ((intAVal - curAVal) >= 1.0f) { __instance.Armor[index].gameObject.SetActive(false); continue; }
        if ((intSVal - curSVal) >= 1.0f) { __instance.Armor[index].gameObject.SetActive(false); continue; }
        VehicleReadoutImage replaceimage = HUDMechArmorReadout_Init_info.vehicleImages[vloc];
        __instance.Armor[index].gameObject.transform.position = __instance.gameObject.transform.position + replaceimage.pos;
        replaceimage.Update(__instance.Armor[index].rectTransform);
        __instance.Armor[index].vectorGraphics = replaceimage.armor;
      }
      for (int index = 0; index < __instance.ArmorOutline.Length; ++index) {
        if (__instance.ArmorOutline[index] == null) { continue; }
        if (index == 2) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; } else
        if (index == 3) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; } else
        if (index == 4) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; };
        VehicleChassisLocations vloc = VehicleChassisLocations.Turret;
        ArmorLocation aloc = ArmorLocation.Head;
        ChassisLocations loc = ChassisLocations.Head;
        if (index == 0) { vloc = VehicleChassisLocations.Turret; aloc = ArmorLocation.Head; loc = ChassisLocations.Head; } else
        if (index == 1) { vloc = VehicleChassisLocations.Rear; aloc = ArmorLocation.RightArm; loc = ChassisLocations.RightArm; } else
        if (index == 5) { vloc = VehicleChassisLocations.Front; aloc = ArmorLocation.LeftArm; loc = ChassisLocations.LeftArm; } else
        if (index == 6) { vloc = VehicleChassisLocations.Right; aloc = ArmorLocation.RightLeg; loc = ChassisLocations.RightLeg; } else
        if (index == 7) { vloc = VehicleChassisLocations.Left; aloc = ArmorLocation.LeftLeg; loc = ChassisLocations.LeftLeg; }
        if ((vloc == VehicleChassisLocations.Turret) && (__instance.DisplayedMechDef.Chassis.Head.InternalStructure <= 1.0f)) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; }
        float curAVal = HUDMechArmorReadout.GetCurrentArmorForLocation(__instance.DisplayedMechDef, aloc);
        float intAVal = HUDMechArmorReadout.GetInitialArmorForLocation(__instance.DisplayedMechDef, aloc);
        float curSVal = HUDMechArmorReadout.GetCurrentStructureForLocation(__instance.DisplayedMechDef, aloc);
        float intSVal = HUDMechArmorReadout.GetInitialStructureForLocation(__instance.DisplayedMechDef, loc);
        if ((intAVal - curAVal) >= 1.0f) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; }
        if ((intSVal - curSVal) >= 1.0f) { __instance.ArmorOutline[index].gameObject.SetActive(false); continue; }
        VehicleReadoutImage replaceimage = HUDMechArmorReadout_Init_info.vehicleImages[vloc];
        replaceimage.Update(__instance.ArmorOutline[index].rectTransform);
        __instance.ArmorOutline[index].transform.localPosition = Vector3.zero;
        __instance.ArmorOutline[index].transform.localScale = Vector3.one * 0.8f;
        __instance.ArmorOutline[index].vectorGraphics = replaceimage.armor;
      }
      if (HUDMechArmorReadout_Init_info.d_originalStructure.TryGetValue(__instance.GetInstanceID(), out List<HUDMechArmorReadoutOriginal> originalStructure) == false) {
        originalStructure = null;
      }
      for (int index = 0; index < __instance.Structure.Length; ++index) {
        if (__instance.Structure[index] == null) { continue; }
        if (index == 2) { __instance.Structure[index].gameObject.SetActive(false); continue; } else
        if (index == 3) { __instance.Structure[index].gameObject.SetActive(false); continue; } else
        if (index == 4) { __instance.Structure[index].gameObject.SetActive(false); continue; };
        VehicleChassisLocations vloc = VehicleChassisLocations.Turret;
        ArmorLocation aloc = ArmorLocation.Head;
        ChassisLocations loc = ChassisLocations.Head;
        if (index == 0) { vloc = VehicleChassisLocations.Turret; aloc = ArmorLocation.Head; loc = ChassisLocations.Head; } else
        if (index == 1) { vloc = VehicleChassisLocations.Rear; aloc = ArmorLocation.RightArm; loc = ChassisLocations.RightArm; } else
        if (index == 5) { vloc = VehicleChassisLocations.Front; aloc = ArmorLocation.LeftArm; loc = ChassisLocations.LeftArm; } else
        if (index == 6) { vloc = VehicleChassisLocations.Right; aloc = ArmorLocation.RightLeg; loc = ChassisLocations.RightLeg; } else
        if (index == 7) { vloc = VehicleChassisLocations.Left; aloc = ArmorLocation.LeftLeg; loc = ChassisLocations.LeftLeg; }
        if ((vloc == VehicleChassisLocations.Turret) && (__instance.DisplayedMechDef.Chassis.Head.InternalStructure <= 1.0f)) { __instance.Structure[index].gameObject.SetActive(false); }
        VehicleReadoutImage replaceimage = HUDMechArmorReadout_Init_info.vehicleImages[vloc];
        float curSVal = HUDMechArmorReadout.GetCurrentStructureForLocation(__instance.DisplayedMechDef, aloc);
        float intSVal = HUDMechArmorReadout.GetInitialStructureForLocation(__instance.DisplayedMechDef, loc);
        __instance.Structure[index].gameObject.transform.position = __instance.gameObject.transform.position + replaceimage.pos;
        replaceimage.Update(__instance.Structure[index].rectTransform);
        __instance.Structure[index].vectorGraphics = replaceimage.structure;
        if(originalStructure != null) {
          HUDMechArmorReadoutOriginal orig = originalStructure[index];
          if (orig != null) { if (orig.color.HasValue == false) { orig.color = __instance.Structure[index].color; }; }
        }
        if (curSVal <= 0f) { __instance.Structure[index].color = UIManager.Instance.UIColorRefs.structureDestroyed; } else
        if ((intSVal - curSVal) >= 1.0f) { __instance.Structure[index].color = UIManager.Instance.UIColorRefs.structureDamaged; }
      }
    }
  }
  [HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  [HarmonyPatch("Spawn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class UnitSpawnPointGameLogic_Spawn {
    private static MethodInfo m_Description = typeof(ActorDef).GetProperty("Description").GetSetMethod(true);
    public static void Description_set(this ActorDef aDef, DescriptionDef descr) { m_Description.Invoke(aDef, new object[1] { descr }); }
    private static MethodInfo m_ChassisID = typeof(PilotableActorDef).GetProperty("ChassisID").GetSetMethod(true);
    public static void ChassisID_set(this PilotableActorDef aDef, string id) { m_ChassisID.Invoke(aDef, new object[1] { id }); }
    private static MethodInfo m_ComponentDefType = typeof(BaseComponentRef).GetProperty("ComponentDefType").GetSetMethod(true);
    public static void ComponentDefType_set(this BaseComponentRef cRef, ComponentType type) { m_ComponentDefType.Invoke(cRef, new object[1] { type }); }
    private static MethodInfo m_HardpointSlot = typeof(BaseComponentRef).GetProperty("HardpointSlot").GetSetMethod(true);
    public static void HardpointSlot_set(this BaseComponentRef cRef, int slot) { m_HardpointSlot.Invoke(cRef, new object[1] { slot }); }
    private static MethodInfo m_IsFixed = typeof(BaseComponentRef).GetProperty("IsFixed").GetSetMethod(true);
    public static void IsFixed_set(this BaseComponentRef cRef, bool isFixed) { m_IsFixed.Invoke(cRef, new object[1] { isFixed }); }
    private static MethodInfo m_MountedLocation = typeof(VehicleComponentRef).GetProperty("MountedLocation").GetSetMethod(true);
    public static void MountedLocation_set(this VehicleComponentRef cRef, VehicleChassisLocations loc) { m_MountedLocation.Invoke(cRef, new object[1] { loc }); }
    private static FieldInfo f_paintTextureID = typeof(PilotableActorDef).GetField("paintTextureID", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void paintTextureID(this PilotableActorDef pDef, string value) { f_paintTextureID.SetValue(pDef, value); }
    public static string paintTextureID(this PilotableActorDef pDef) { return (string)f_paintTextureID.GetValue(pDef); }
    private static FieldInfo f_heraldryID = typeof(PilotableActorDef).GetField("heraldryID", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void heraldryID(this PilotableActorDef pDef, string value) { f_heraldryID.SetValue(pDef, value); }
    private static FieldInfo f_heraldryDef = typeof(PilotableActorDef).GetField("heraldryDef", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void heraldryDef(this PilotableActorDef pDef, HeraldryDef value) { f_heraldryID.SetValue(pDef, value); }
    private static MethodInfo m_VehicleTags = typeof(VehicleDef).GetProperty("VehicleTags").GetSetMethod(true);
    public static void VehicleTags_set(this VehicleDef vDef, TagSet value) { m_VehicleTags.Invoke(vDef, new object[1] { value }); }
    private static FieldInfo f_vLocaltions = typeof(VehicleDef).GetField("Locations", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void Localtions(this VehicleDef vDef, VehicleLocationLoadoutDef[] value) { f_vLocaltions.SetValue(vDef, value); }
    private static FieldInfo f_inventory = typeof(VehicleDef).GetField("inventory", BindingFlags.Instance | BindingFlags.NonPublic);
    public static void inventory(this VehicleDef vDef, VehicleComponentRef[] value) { f_inventory.SetValue(vDef, value); }
    private static FieldInfo f_mLocaltions = typeof(MechDef).GetField("Locations", BindingFlags.Instance | BindingFlags.NonPublic);
    public static LocationLoadoutDef[] Localtions(this MechDef mDef) { return (LocationLoadoutDef[])f_mLocaltions.GetValue(mDef); }
    public static VehicleDef toVehicleDef(this MechDef mDef, DataManager dataManager) {
      List<VehicleComponentRef> inventory = new List<VehicleComponentRef>();
      foreach (MechComponentRef mRef in mDef.Inventory) {
        //if (mRef.IsFixed) { continue; }
        VehicleChassisLocations location = VehicleChassisLocations.Turret;
        switch (mRef.MountedLocation) {
          case ChassisLocations.Head: location = VehicleChassisLocations.Turret; break;
          case ChassisLocations.CenterTorso: location = VehicleChassisLocations.Turret; break;
          case ChassisLocations.RightTorso: location = VehicleChassisLocations.Turret; break;
          case ChassisLocations.LeftTorso: location = VehicleChassisLocations.Turret; break;
          case ChassisLocations.LeftArm: location = VehicleChassisLocations.Front; break;
          case ChassisLocations.RightArm: location = VehicleChassisLocations.Rear; break;
          case ChassisLocations.LeftLeg: location = VehicleChassisLocations.Left; break;
          case ChassisLocations.RightLeg: location = VehicleChassisLocations.Right; break;
        }
        VehicleComponentRef vRef = new VehicleComponentRef();
        vRef.DataManager = dataManager;
        vRef.ComponentDefType_set(mRef.ComponentDefType);
        vRef.HardpointSlot_set(mRef.HardpointSlot);
        vRef.DamageLevel = mRef.DamageLevel;
        vRef.prefabName = mRef.prefabName;
        vRef.hasPrefabName = mRef.hasPrefabName;
        vRef.IsFixed_set(true);
        vRef.ComponentDefID = mRef.ComponentDefID;
        vRef.SimGameUID = mRef.SimGameUID;
        vRef.MountedLocation_set(location);
        inventory.Add(vRef);
      }
      VehicleDef result = new VehicleDef();
      result.DataManager = dataManager;
      result.Description_set(mDef.Description);
      result.ChassisID_set(mDef.ChassisID);
      result.inventory(inventory.ToArray());
      //result.prefabOverride = def.prefabOverride;
      result.paintTextureID(mDef.paintTextureID());
      result.heraldryID(mDef.HeraldryID);
      result.heraldryDef(mDef.HeraldryDef);
      result.VehicleTags_set(new TagSet(mDef.MechTags));
      List<VehicleLocationLoadoutDef> localtions = new List<VehicleLocationLoadoutDef>();
      Dictionary<int, VehicleLocationLoadoutDef> dLoc = new Dictionary<int, VehicleLocationLoadoutDef>();
      foreach (LocationLoadoutDef lDef in mDef.Localtions()) {
        if ((lDef.CurrentInternalStructure == 0f) && (lDef.AssignedArmor == 0f)) { continue; }
        VehicleChassisLocations location = VehicleChassisLocations.Turret;
        int id = -1;
        switch (lDef.Location) {
          case ChassisLocations.Head: location = VehicleChassisLocations.Turret; id = 4; break;
          case ChassisLocations.CenterTorso: location = VehicleChassisLocations.Turret; id = -1; break;
          case ChassisLocations.RightTorso: location = VehicleChassisLocations.Turret; id = -1; break;
          case ChassisLocations.LeftTorso: location = VehicleChassisLocations.Turret; id = -1; break;
          case ChassisLocations.LeftArm: location = VehicleChassisLocations.Front; id = 0; break;
          case ChassisLocations.RightArm: location = VehicleChassisLocations.Rear; id = 3; break;
          case ChassisLocations.LeftLeg: location = VehicleChassisLocations.Left; id = 1; break;
          case ChassisLocations.RightLeg: location = VehicleChassisLocations.Right; id = 2; break;
        }
        VehicleLocationLoadoutDef vlDef = new VehicleLocationLoadoutDef(location, lDef.CurrentArmor, lDef.CurrentInternalStructure, lDef.AssignedArmor, lDef.DamageLevel);
        if (id < 0) { continue; }
        dLoc.Add(id, vlDef);
      }
      for (int t = 0; t < 5; ++t) {
        if (dLoc.TryGetValue(t, out VehicleLocationLoadoutDef vlDef)) {
          localtions.Add(vlDef);
        } else {
          localtions.Add(new VehicleLocationLoadoutDef());
        }
      }
      result.Localtions(localtions.ToArray());
      result.SetGuid(mDef.GUID);
      result.DataManager = dataManager;
      Log.TWL(0, "MechDef.toVehicleDef "+result.Description.Id+" GUID:"+result.GUID);
      result.Refresh();
      return result;
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "UnitSpawnPointGameLogic.Spawn Transpiler");
      MethodInfo targetMethod = typeof(UnitSpawnPointGameLogic).GetMethod("SpawnMech", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(UnitSpawnPointGameLogic_Spawn), nameof(SpawnMech));
      return Transpilers.MethodReplacer(instructions, targetMethod, replacementMethod);
    }
    public static AbstractActor SpawnMech(UnitSpawnPointGameLogic instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef) {
      Log.TWL(0, "UnitSpawnPointGameLogic.Spawn SpawnMech " + mDef.Description.Id + " chassis:" + mDef.ChassisID);
      DataManager dataManager = mDef.DataManager;
      if (dataManager == null) { dataManager = mDef.Chassis.DataManager; }
      if (dataManager == null) { dataManager = instance.Combat.DataManager; }
      if (dataManager == null) { dataManager = mDef.DataManager(); }
      Log.WL(1, "dataManager:" + (dataManager == null ? "null" : "not null"));
      AbstractActor result = null;
      try {
        if (mDef.IsFake(mDef.Description.Id) == false) {
          if (mDef.Chassis.IsFake(mDef.Chassis.Description.Id) == false) {
            Log.WL(1, "spawning mech");
            result = instance.SpawnMech(mDef, pilot, team, lance, customHeraldryDef);
          } else {
            Log.WL(1, "spawning vehicle");
            VehicleDef def = mDef.toVehicleDef(dataManager);
            Log.WL(1, def.ToJSON());
            result = instance.SpawnVehicle(def, pilot, team, lance, customHeraldryDef); ;
          }
        } else {
          Log.WL(1, "spawning vehicle");
          VehicleDef def = mDef.toVehicleDef(dataManager);
          Log.WL(1, def.ToJSON());
          result = instance.SpawnVehicle(def, pilot, team, lance, customHeraldryDef);
        }
        Log.WL(1, "success");
        return result;
      } catch (Exception e) {
        Log.WL(1, e.ToString());
        return null;
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class MechDef_GatherDependencies_fake {
    public static void Postfix(MechDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      if (__instance.IsFake(__instance.Description.Id) == false) { return; }
      Log.TWL(0, "MechDef.GatherDependencies fake " + __instance.Description.Id);
      dependencyLoad.RequestResource(BattleTechResourceType.VehicleDef, __instance.Description.Id);
      dependencyLoad.RequestResource(BattleTechResourceType.VehicleChassisDef, __instance.ChassisID);
      if (dataManager.VehicleDefs.TryGet(__instance.Description.Id, out VehicleDef vdef)) {
        //vdef.GatherDependencies(dataManager, dependencyLoad, activeRequestWeight);
      } else {
        dependencyLoad.RequestResource(BattleTechResourceType.VehicleDef, __instance.Description.Id);
      }
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class ChassisDef_GatherDependencies_fake {
    private static DataManager dataManager = null;
    public static DataManager DataManager(this ActorDef def) { return dataManager; }
    public static void Postfix(ChassisDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      ChassisDef_GatherDependencies_fake.dataManager = dataManager;
      if (__instance.IsFake(__instance.Description.Id) == false) { return; }
      Log.TWL(0, "ChassisDef.GatherDependencies fake " + __instance.Description.Id);
      if (dataManager.VehicleChassisDefs.TryGet(__instance.Description.Id, out VehicleChassisDef vchassis)) {
        //vchassis.GatherDependencies(dataManager, dependencyLoad, activeRequestWeight);
      } else {
        dependencyLoad.RequestResource(BattleTechResourceType.VehicleChassisDef, __instance.Description.Id);
      }
    }
  }
  [HarmonyPatch(typeof(VehicleDef))]
  [HarmonyPatch("RefreshChassis")]
  [HarmonyPatch(new Type[] { })]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleChassisDef_RefreshChassis {
    public static void Postfix(VehicleDef __instance) {
      Log.TWL(0, "VehicleChassisDef.RefreshChassis " + __instance.ChassisID + " chassis:" + (__instance.Chassis == null ? "null" : "not null"));
      if (__instance.Chassis == null) {
        Log.WL(1, "DataManager:" + (__instance.DataManager == null ? "null" : "not null"));
        if (__instance.DataManager != null) {
          Log.WL(1, "VehicleChassis:" + (__instance.DataManager.VehicleChassisDefs.Exists(__instance.ChassisID)));
        }
      }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("IsLocationDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_IsLocationDestroyed {
    public static void Postfix(MechDef __instance, ChassisLocations loc, ref bool __result) {
      if (__result == false) { return; }
      if (__instance.IsFake(__instance.Description.Id) == false) { return; };
      if (loc == ChassisLocations.Head) { if (__instance.Chassis.Head.InternalStructure == 0f) { __result = false; }; }
      //if (loc == ChassisLocations.CenterTorso) { __result = false; return; }
      //if (loc == ChassisLocations.RightTorso) { __result = false; return; }
      //if (loc == ChassisLocations.LeftTorso) { __result = false; return; }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechStructureSimple")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechStructureSimple {
    public static void Postfix(MechDef mechDef, ref bool __result) {
      if (__result == true) { return; }
      if (mechDef.Chassis.IsFake(mechDef.ChassisID) == false) { return; };
      __result = ((double)mechDef.LeftArm.CurrentInternalStructure >= 1.0) && ((double)mechDef.RightArm.CurrentInternalStructure >= 1.0) && ((double)mechDef.LeftLeg.CurrentInternalStructure >= 1.0) && ((double)mechDef.RightLeg.CurrentInternalStructure >= 1.0);
      if (mechDef.Chassis.Head.InternalStructure > 0f) { __result = __result && (mechDef.Head.CurrentInternalStructure >= 1f); };
      Log.TWL(0, "MechValidationRules.ValidateMechStructureSimple " + mechDef.Chassis.Description.Id + " fake:" + mechDef.Chassis.IsFake(mechDef.ChassisID) + " result:" + __result);
    }
  }
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("GenerateSalvage")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Contract_GenerateSalvage {
    public static void Postfix(List<UnitResult> lostUnits) {
      Log.TWL(0, "Contract.GenerateSalvage");
      foreach(UnitResult unit in lostUnits) {
        Log.WL(1, "lost:" + unit.mech.ChassisID + " is realy lost:" + unit.mechLost);
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechCanBeFielded")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechCanBeFielded {
    public static void Postfix(SimGameState sim, MechDef mechDef, ref bool __result) {
      if (__result == true) { return; }
      try {
        Log.TWL(0, "MechValidationRules.ValidateMechCanBeFielded");
        if (mechDef == null) { Log.WL(1, "mechDef is null"); return; };
        Log.WL(1, mechDef.Description.Id);
        bool isFake = BattleTechResourceLocator_RefreshTypedEntries_Patch.IsChassisFake(mechDef.ChassisID);
        if (isFake == false) { return; };
        Log.WL(1, mechDef.ChassisID + " fake:" + isFake + " result:" + __result);

        int num1 = MechValidationRules.ValidateSimGameMechNotInMaintenance(sim, mechDef) ? 1 : 0;
        Log.WL(1, "num1 = " + num1);
        bool flag1 = MechValidationRules.ValidateMechStructureSimple(mechDef);
        Log.WL(1, "flag1 = " + flag1);
        bool flag2 = MechValidationRules.ValidateMechPosessesWeaponsSimple(mechDef);
        Log.WL(1, "flag2 = " + flag2);
        int num2 = flag1 ? 1 : 0;
        Log.WL(1, "num2 = " + num2);
        __result = (num1 & num2 & (flag2 ? 1 : 0)) != 0;
        Log.WL(1, "result:" + __result);
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechPosessesWeaponsSimple")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechPosessesWeaponsSimple {
    public static void Postfix(MechDef mechDef, ref bool __result) {
      if (__result == true) { return; }
      if (mechDef.Chassis.IsFake(mechDef.Chassis.Description.Id) == false) { return; };
      Log.TWL(0, "MechValidationRules.ValidateMechPosessesWeaponsSimple " + mechDef.Chassis.Description.Id + " fake:" + mechDef.Chassis.IsFake(mechDef.Chassis.Description.Id) + " result:" + __result);
    }
  }

#pragma warning disable CS0252
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("RefreshInventory")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_RefreshInventory {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "MechDef.RefreshInventory Transpiler");
      MethodInfo targetMethod = typeof(MechHardpointRules).GetMethod("GetComponentPrefabName", BindingFlags.Static | BindingFlags.Public);
      MethodInfo removeMethod = typeof(ChassisDef).GetProperty("HardpointDataDef").GetMethod;
      var replacementMethod = AccessTools.Method(typeof(MechDef_RefreshInventory), nameof(GetComponentPrefabName));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if ((uInstructions[t].opcode == OpCodes.Call) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t; break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find MechHardpointRules.GetComponentPrefabName call");
        return uInstructions;
      }
      Log.WL(1, "found MechHardpointRules.GetComponentPrefabName call " + MethodPos.ToString("X"));
      int RemovePos = -1;
      for (int t = MethodPos; t >= 0; --t) {
        if ((uInstructions[t].opcode == OpCodes.Callvirt) && (uInstructions[t].operand == removeMethod)) {
          RemovePos = t; break;
        }
      }
      if (RemovePos < 0) {
        Log.WL(1, "can't find ChassisDef::get_HardpointDataDef() call");
        return uInstructions;
      }
      Log.WL(1, "found ChassisDef::get_HardpointDataDef() call " + RemovePos.ToString("X"));
      uInstructions[MethodPos].operand = replacementMethod;
      uInstructions.RemoveAt(RemovePos);
      return uInstructions;
    }
#pragma warning restore CS0252

    private static string GetComponentPrefabName(ChassisDef chassis, BaseComponentRef componentRef, string prefabBase, string location, ref List<string> usedPrefabNames) {
      Log.TWL(0, "MechDef.RefreshInventory.GetComponentPrefabName chassis " + chassis.Description.Id + " fake: " + chassis.IsFake(chassis.Description.Id));
      Log.W(1, location);
      if (chassis.IsFake(chassis.Description.Id)) {
        if (location == ChassisLocations.LeftArm.ToString().ToLower()) {
          location = VehicleChassisLocations.Front.ToString().ToLower();
        } else if (location == ChassisLocations.RightArm.ToString().ToLower()) {
          location = VehicleChassisLocations.Rear.ToString().ToLower();
        } else if (location == ChassisLocations.LeftLeg.ToString().ToLower()) {
          location = VehicleChassisLocations.Left.ToString().ToLower();
        } else if (location == ChassisLocations.RightLeg.ToString().ToLower()) {
          location = VehicleChassisLocations.Right.ToString().ToLower();
        } else if (location == ChassisLocations.CenterTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.RightTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.LeftTorso.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        } else if (location == ChassisLocations.Head.ToString().ToLower()) {
          location = VehicleChassisLocations.Turret.ToString().ToLower();
        }
      }
      Log.WL(0, "->" + location);
      return MechHardpointRules.GetComponentPrefabName(chassis.HardpointDataDef, componentRef, prefabBase, location, ref usedPrefabNames);
    }
  }
  /*[HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("RefreshInventory")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechDef_RefreshInventory {
    public static bool Prefix(HardpointDataDef hardpointDataDef, BaseComponentRef componentRef, string prefabBase,ref string location, ref List<string> usedPrefabNames) {
      Log.TWL(0, "MechHardpointRules.GetComponentPrefabName "+ hardpointDataDef.ID);
      if (hardpointDataDef.isFakeHardpoint()) {
        Log.W(1, location);
        if (location == ChassisLocations.LeftArm.ToString()) {
          location = VehicleChassisLocations.Front.ToString();
        } else if (location == ChassisLocations.RightArm.ToString()) {
          location = VehicleChassisLocations.Rear.ToString();
        } else if (location == ChassisLocations.LeftLeg.ToString()) {
          location = VehicleChassisLocations.Left.ToString();
        } else if (location == ChassisLocations.RightLeg.ToString()) {
          location = VehicleChassisLocations.Right.ToString();
        } else if (location == ChassisLocations.CenterTorso.ToString()) {
          location = VehicleChassisLocations.Turret.ToString();
        } else if (location == ChassisLocations.RightTorso.ToString()) {
          location = VehicleChassisLocations.Turret.ToString();
        } else if (location == ChassisLocations.LeftTorso.ToString()) {
          location = VehicleChassisLocations.Turret.ToString();
        } else if (location == ChassisLocations.Head.ToString()) {
          location = VehicleChassisLocations.Turret.ToString();
        }
        Log.WL(0,"->"+location);
      }
      return true;
    }
  }*/
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  [HarmonyPriority(Priority.First)]
  public static class MechDef_FromJSON_fake {
    public class CombatGameConstantsFake {
      public CombatValueMultipliersDef CombatValueMultipliers { get; private set; }
    };
    public static CombatValueMultipliersDef? CombatValueMultipliers = null;
    //private static FieldInfo f_Locations = typeof(VehicleDef).GetField("Locations");
    //private static Dictionary<string, string> realVehicleDefJSONs = new Dictionary<string, string>();
    //public static string getRealVehicleDefJSONs(string id) {
    //  if (realVehicleDefJSONs.TryGetValue(id, out string json)) {
    //    return json;
    //  } else {
    //    return null;
    //  }
    //}
    public static bool Prefix(MechDef __instance, ref string json) {
      Log.TW(0, "MechDef.FromJSON fake");
      try {
        JObject olddef = JObject.Parse(json);
        string id = (string)olddef["Description"]["Id"];
        string chassisId = (string)olddef["ChassisID"];
        bool isFake = BattleTechResourceLocator_RefreshTypedEntries_Patch.IsChassisFake(chassisId);
        Log.WL(1,id+" chassis:"+chassisId+" isFake:"+isFake);
        if (isFake == false) { return true; }
        Log.WL(1, "constants " + (BattleTech.UnityGameInstance.BattleTechGame.constantsManifest() == null ? "null" : BattleTech.UnityGameInstance.BattleTechGame.constantsManifest().FilePath));
        JObject newdef = new JObject();
        if (olddef["Chassis"] != null) { return true; };
        float ArmorMultiplierVehicle = 1f;
        float StructureMultiplierVehicle = 1f;
        if (CombatValueMultipliers.HasValue == false) {
          if (BattleTech.UnityGameInstance.BattleTechGame.constantsManifest() != null) {
            CombatGameConstantsFake constants = new CombatGameConstantsFake();
            JSONSerializationUtility.FromJSON<CombatGameConstantsFake>(constants, File.ReadAllText(BattleTech.UnityGameInstance.BattleTechGame.constantsManifest().FilePath));
            CombatValueMultipliers = constants.CombatValueMultipliers;
            ArmorMultiplierVehicle = CombatValueMultipliers.Value.ArmorMultiplierVehicle;
            StructureMultiplierVehicle = CombatValueMultipliers.Value.StructureMultiplierVehicle;
          }
        } else {
          ArmorMultiplierVehicle = CombatValueMultipliers.Value.ArmorMultiplierVehicle;
          StructureMultiplierVehicle = CombatValueMultipliers.Value.StructureMultiplierVehicle;
        }
        Log.WL(1, "ArmorMultiplierVehicle:"+ ArmorMultiplierVehicle);
        Log.WL(1, "StructureMultiplierVehicle:" + StructureMultiplierVehicle);
        newdef["Description"] = olddef["Description"];
        newdef["ChassisID"] = olddef["ChassisID"];
        newdef["simGameMechPartCost"] = 0;
        newdef["MechTags"] = olddef["VehicleTags"];

        JArray vInventory = olddef["inventory"] as JArray;
        JArray mInventory = new JArray();
        Log.WL(1, "inventory");
        foreach (JObject vcRef in vInventory) {
          Log.WL(2, vcRef["ComponentDefID"] + ":" + vcRef["MountedLocation"]);
          VehicleChassisLocations vLocation = (VehicleChassisLocations)Enum.Parse(typeof(VehicleChassisLocations), (string)vcRef["MountedLocation"]);
          ChassisLocations location = ChassisLocations.Head;
          switch (vLocation) {
            case VehicleChassisLocations.Front: location = ChassisLocations.LeftArm; break;
            case VehicleChassisLocations.Rear: location = ChassisLocations.RightArm; break;
            case VehicleChassisLocations.Left: location = ChassisLocations.LeftLeg; break;
            case VehicleChassisLocations.Right: location = ChassisLocations.RightLeg; break;
            case VehicleChassisLocations.Turret: location = ChassisLocations.Head; break;
          }
          JObject mcRef = new JObject();
          mcRef["MountedLocation"] = location.ToString();
          mcRef["SimGameUID"] = "";
          mcRef["ComponentDefID"] = vcRef["ComponentDefID"]; ;
          mcRef["ComponentDefType"] = vcRef["ComponentDefType"];
          mcRef["HardpointSlot"] = vcRef["HardpointSlot"];
          mcRef["GUID"] = null;
          mcRef["DamageLevel"] = vcRef["DamageLevel"];
          mcRef["IsFixed"] = true;
          mcRef["prefabName"] = vcRef["prefabName"];
          mcRef["hasPrefabName"] = vcRef["hasPrefabName"];
          //MechComponentRef mcRef = new MechComponentRef(vcRef.ComponentDefID, vcRef.SimGameUID, vcRef.ComponentDefType, location, vcRef.HardpointSlot, vcRef.DamageLevel, vcRef.IsFixed);
          mInventory.Add(mcRef);
        }
        newdef["inventory"] = mInventory;
        JArray vLocations = olddef["Locations"] as JArray;
        JArray mLocations = new JArray();
        HashSet<ChassisLocations> existingLocation = new HashSet<ChassisLocations>();
        existingLocation.Add(ChassisLocations.LeftArm);
        existingLocation.Add(ChassisLocations.RightArm);
        existingLocation.Add(ChassisLocations.LeftLeg);
        existingLocation.Add(ChassisLocations.RightLeg);
        existingLocation.Add(ChassisLocations.Head);
        existingLocation.Add(ChassisLocations.CenterTorso);
        existingLocation.Add(ChassisLocations.RightTorso);
        existingLocation.Add(ChassisLocations.LeftTorso);
        for(int loc_index=0;loc_index < vLocations.Count; ++loc_index) {
          //foreach (JObject vcLoc in vLocations) {
          JToken vcLoc = vLocations[loc_index];
          Log.WL(2, vcLoc["Location"] + ":" + vcLoc["AssignedArmor"]);
          VehicleChassisLocations vLocation = VehicleChassisLocations.None;
          if (vcLoc["Location"] == null) {
            switch (loc_index) {
              case 0: vLocation = VehicleChassisLocations.Front; break;
              case 1: vLocation = VehicleChassisLocations.Left; break;
              case 2: vLocation = VehicleChassisLocations.Right; break;
              case 3: vLocation = VehicleChassisLocations.Rear; break;
              case 4: vLocation = VehicleChassisLocations.Turret; break;
            }
          } else {
            vLocation = (VehicleChassisLocations)Enum.Parse(typeof(VehicleChassisLocations), (string)vcLoc["Location"]);
          }
          ChassisLocations location = ChassisLocations.Head;
          switch (vLocation) {
            case VehicleChassisLocations.Front: location = ChassisLocations.LeftArm; existingLocation.Remove(ChassisLocations.LeftArm); break;
            case VehicleChassisLocations.Rear: location = ChassisLocations.RightArm; existingLocation.Remove(ChassisLocations.RightArm); break;
            case VehicleChassisLocations.Left: location = ChassisLocations.LeftLeg; existingLocation.Remove(ChassisLocations.LeftLeg); break;
            case VehicleChassisLocations.Right: location = ChassisLocations.RightLeg; existingLocation.Remove(ChassisLocations.RightLeg); break;
            case VehicleChassisLocations.Turret: location = ChassisLocations.Head; existingLocation.Remove(ChassisLocations.Head); break;
          }
          JObject mcLoc = new JObject();
          mcLoc["DamageLevel"] = vcLoc["DamageLevel"];
          mcLoc["Location"] = location.ToString();
          mcLoc["CurrentArmor"] = Mathf.RoundToInt((float)vcLoc["CurrentArmor"]*ArmorMultiplierVehicle);
          mcLoc["CurrentRearArmor"] = -1;
          mcLoc["CurrentInternalStructure"] = Mathf.RoundToInt((float)vcLoc["CurrentInternalStructure"]*StructureMultiplierVehicle);
          mcLoc["AssignedArmor"] = Mathf.RoundToInt((float)vcLoc["AssignedArmor"]*ArmorMultiplierVehicle);
          mcLoc["AssignedRearArmor"] = -1;
          //LocationLoadoutDef mcLoc = new LocationLoadoutDef(location,vcLoc.CurrentArmor,0f,vcLoc.CurrentInternalStructure,vcLoc.AssignedArmor,0f,vcLoc.DamageLevel);
          mLocations.Add(mcLoc);
        }
        foreach (ChassisLocations loc in existingLocation) {
          JObject adLoc = new JObject();
          adLoc["DamageLevel"] = LocationDamageLevel.Functional.ToString();
          adLoc["Location"] = loc.ToString();
          adLoc["CurrentArmor"] = 0f;
          adLoc["CurrentRearArmor"] = 0f;
          adLoc["CurrentInternalStructure"] = 1f;
          adLoc["AssignedArmor"] = 0f;
          adLoc["AssignedRearArmor"] = 0f;
          mLocations.Add(adLoc);
        }
        newdef["Locations"] = mLocations;
        json = newdef.ToString(Newtonsoft.Json.Formatting.Indented);
        Log.WL(1, json);
      } catch (Exception e) {
        Log.LogWrite(json, true);
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  [HarmonyPriority(Priority.First)]
  public static class ChassisDef_FromJSON_fake {
    private static HashSet<string> fakeHardpoints = new HashSet<string>();
    public static bool isFakeHardpoint(this HardpointDataDef hardpoint) { return fakeHardpoints.Contains(hardpoint.ID); }
    public static bool Prefix(ChassisDef __instance, ref string json) {
      Log.TW(0, "ChassisDef.FromJSON fake");
      try {
        JObject olddef = JObject.Parse(json);
        string id = (string)olddef["Description"]["Id"];
        bool isFake = BattleTechResourceLocator_RefreshTypedEntries_Patch.IsChassisFake(id);
        Log.WL(1, id + " isFake:" + isFake);
        if (isFake == false) { return true; }
        Log.WL(1, "constants " + (BattleTech.UnityGameInstance.BattleTechGame.constantsManifest() == null ? "null" : BattleTech.UnityGameInstance.BattleTechGame.constantsManifest().FilePath));
        float ArmorMultiplierVehicle = 1f;
        float StructureMultiplierVehicle = 1f;
        if (MechDef_FromJSON_fake.CombatValueMultipliers.HasValue == false) {
          if (BattleTech.UnityGameInstance.BattleTechGame.constantsManifest() != null) {
            MechDef_FromJSON_fake.CombatGameConstantsFake constants = new MechDef_FromJSON_fake.CombatGameConstantsFake();
            JSONSerializationUtility.FromJSON<MechDef_FromJSON_fake.CombatGameConstantsFake>(constants, File.ReadAllText(BattleTech.UnityGameInstance.BattleTechGame.constantsManifest().FilePath));
            MechDef_FromJSON_fake.CombatValueMultipliers = constants.CombatValueMultipliers;
            ArmorMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.ArmorMultiplierVehicle;
            StructureMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.StructureMultiplierVehicle;
          }
        } else {
          ArmorMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.ArmorMultiplierVehicle;
          StructureMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.StructureMultiplierVehicle;
        }
        Log.WL(1, "ArmorMultiplierVehicle:" + ArmorMultiplierVehicle);
        Log.WL(1, "StructureMultiplierVehicle:" + StructureMultiplierVehicle);
        JObject newdef = new JObject();
        newdef["Description"] = olddef["Description"];
        newdef["MovementCapDefID"] = olddef["MovementCapDefID"];
        newdef["PathingCapDefID"] = olddef["PathingCapDefID"];
        newdef["HardpointDataDefID"] = olddef["HardpointDataDefID"];
        fakeHardpoints.Add((string)olddef["HardpointDataDefID"]);
        newdef["PrefabIdentifier"] = olddef["PrefabIdentifier"];
        newdef["PrefabBase"] = olddef["PrefabBase"];
        newdef["VariantName"] = olddef["Description"]["Name"];
        newdef["StockRole"] = "VEHICLE";
        newdef["YangsThoughts"] = olddef["Description"]["Details"];
        newdef["Tonnage"] = olddef["Tonnage"];
        newdef["InitialTonnage"] = 0;
        newdef["weightClass"] = olddef["weightClass"];
        newdef["Heatsinks"] = 0;
        newdef["TopSpeed"] = olddef["TopSpeed"];
        newdef["TurnRadius"] = olddef["TopSpeed"];
        newdef["MaxJumpjets"] = 0;
        newdef["Stability"] = 100;
        newdef["StabilityDefenses"] = JArray.Parse("[ 0, 0, 0, 0, 0, 0 ]");
        newdef["LOSSourcePositions"] = JArray.Parse("[ { \"x\": 0, \"y\": 12, \"z\": 0 }, { \"x\": 3, \"y\": 12, \"z\": 0 }, { \"x\": -3, \"y\": 12,\"z\": 0 }]");
        newdef["LOSTargetPositions"] = JArray.Parse("[ { \"x\": 0, \"y\": 12, \"z\": 0 }, { \"x\": 3, \"y\": 12, \"z\": 0 }, { \"x\": -3, \"y\": 12,\"z\": 0 }, { \"x\": 2.5, \"y\": 6,\"z\": 0 }, { \"x\": -2.5, \"y\": 6,\"z\": 0 }]");
        newdef["SpotterDistanceMultiplier"] = olddef["SpotterDistanceMultiplier"];
        newdef["VisibilityMultiplier"] = olddef["VisibilityMultiplier"];
        newdef["SensorRangeMultiplier"] = olddef["SensorRangeMultiplier"];
        newdef["Signature"] = olddef["Signature"];
        newdef["Radius"] = olddef["Radius"];
        newdef["PunchesWithLeftArm"] = false;
        newdef["MeleeDamage"] = 0;
        newdef["MeleeInstability"] = 0;
        newdef["MeleeToHitModifier"] = 0;
        newdef["DFADamage"] = 0;
        newdef["DFAToHitModifier"] = 0;
        newdef["DFASelfDamage"] = 0;
        newdef["DFAInstability"] = 0;
        newdef["ChassisTags"] = JObject.Parse("{ \"items\": [\"fake_vehicle_chassis\"], \"tagSetSourceFile\": \"\" }");
        JArray vLocations = olddef["Locations"] as JArray;
        JArray mLocations = new JArray();
        HashSet<ChassisLocations> existingLocation = new HashSet<ChassisLocations>();
        existingLocation.Add(ChassisLocations.LeftArm);
        existingLocation.Add(ChassisLocations.RightArm);
        existingLocation.Add(ChassisLocations.LeftLeg);
        existingLocation.Add(ChassisLocations.RightLeg);
        existingLocation.Add(ChassisLocations.Head);
        existingLocation.Add(ChassisLocations.CenterTorso);
        existingLocation.Add(ChassisLocations.RightTorso);
        existingLocation.Add(ChassisLocations.LeftTorso);
        for (int loc_index = 0; loc_index < vLocations.Count; ++loc_index) {
          //foreach (JObject vcLoc in vLocations) {
          ChassisLocations location = ChassisLocations.None;
          JToken vcLoc = vLocations[loc_index];
          Log.WL(2, vcLoc["Location"] + ":" + vcLoc["InternalStructure"]);
          VehicleChassisLocations vLocation = VehicleChassisLocations.None;
          if (vcLoc["Location"] == null) {
            switch (loc_index) {
              case 0: vLocation = VehicleChassisLocations.Front; break;
              case 1: vLocation = VehicleChassisLocations.Left; break;
              case 2: vLocation = VehicleChassisLocations.Right; break;
              case 3: vLocation = VehicleChassisLocations.Rear; break;
              case 4: vLocation = VehicleChassisLocations.Turret; break;
            }
          } else {
            vLocation = (VehicleChassisLocations)Enum.Parse(typeof(VehicleChassisLocations), (string)vcLoc["Location"]);
          }
          switch (vLocation) {
            case VehicleChassisLocations.Front: location = ChassisLocations.LeftArm; existingLocation.Remove(ChassisLocations.LeftArm); break;
            case VehicleChassisLocations.Rear: location = ChassisLocations.RightArm; existingLocation.Remove(ChassisLocations.RightArm); break;
            case VehicleChassisLocations.Left: location = ChassisLocations.LeftLeg; existingLocation.Remove(ChassisLocations.LeftLeg); break;
            case VehicleChassisLocations.Right: location = ChassisLocations.RightLeg; existingLocation.Remove(ChassisLocations.RightLeg); break;
            case VehicleChassisLocations.Turret: location = ChassisLocations.Head; existingLocation.Remove(ChassisLocations.Head); break;
          }
          JObject mcLoc = new JObject();
          mcLoc["Location"] = location.ToString();
          mcLoc["Hardpoints"] = vcLoc["Hardpoints"];
          mcLoc["Tonnage"] = vcLoc["Tonnage"];
          mcLoc["MaxArmor"] = Mathf.RoundToInt((float)vcLoc["MaxArmor"]*ArmorMultiplierVehicle);
          mcLoc["InventorySlots"] = vcLoc["InventorySlots"];
          mcLoc["InternalStructure"] = Mathf.RoundToInt((float)vcLoc["InternalStructure"]*StructureMultiplierVehicle);
          //LocationLoadoutDef mcLoc = new LocationLoadoutDef(location,vcLoc.CurrentArmor,0f,vcLoc.CurrentInternalStructure,vcLoc.AssignedArmor,0f,vcLoc.DamageLevel);
          mLocations.Add(mcLoc);
        }
        foreach (ChassisLocations loc in existingLocation) {
          JObject adLoc = new JObject();
          adLoc["Location"] = loc.ToString();
          adLoc["Hardpoints"] = new JArray();
          adLoc["Tonnage"] = 0;
          adLoc["MaxArmor"] = 0;
          adLoc["InventorySlots"] = 0;
          adLoc["InternalStructure"] = 0;
          mLocations.Add(adLoc);
        }
        newdef["Locations"] = mLocations;
        json = newdef.ToString(Newtonsoft.Json.Formatting.Indented);
        Log.WL(1, json);
      } catch (Exception e) {
        Log.LogWrite(json, true);
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(BattleTechResourceLocator), "RefreshTypedEntries")]
  public static class BattleTechResourceLocator_RefreshTypedEntries_Patch {
    private static HashSet<string> fakemechDefs = new HashSet<string>();
    private static HashSet<string> fakeChassisDef = new HashSet<string>();
    private static VersionManifestEntry CombatGameConstants = null;
    public static VersionManifestEntry constantsManifest(this GameInstance game) { return CombatGameConstants; }
    public static bool IsFake(this MechDef mechDef, string id) {
      return fakemechDefs.Contains(id);
    }
    public static bool IsFake(this ChassisDef chassisDef, string id) {
      return fakeChassisDef.Contains(id);
    }
    public static bool IsChassisFake(string id) {
      return fakeChassisDef.Contains(id);
    }
    public static bool IsChassisFake(this MechDef mechDef) {
      return fakeChassisDef.Contains(mechDef.ChassisID);
    }
    public static void Postfix(
            Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>> ___baseManifest,
            Dictionary<BattleTechResourceType, Dictionary<string, VersionManifestEntry>> ___contentPacksManifest) {
      Log.TWL(0, "BattleTechResourceLocator.RefreshTypedEntries");
      if (___baseManifest.TryGetValue(BattleTechResourceType.CombatGameConstants, out Dictionary<string, VersionManifestEntry> costants)) {
        if(costants.TryGetValue(nameof(CombatGameConstants),out VersionManifestEntry manifest)) {
          CombatGameConstants = manifest;
        }
      }
      if (___baseManifest.TryGetValue(BattleTechResourceType.VehicleDef, out Dictionary<string, VersionManifestEntry> vehicles)) {
        if (___baseManifest.TryGetValue(BattleTechResourceType.MechDef, out Dictionary<string, VersionManifestEntry> mechs) == false) {
          mechs = new Dictionary<string, VersionManifestEntry>();
          ___baseManifest.Add(BattleTechResourceType.MechDef, mechs);
        }
        fakemechDefs.Clear();
        foreach (var vehicle in vehicles) {
          if (mechs.ContainsKey(vehicle.Key)) { continue; }
          Log.WL(1, "adding MechDef " + vehicle.Key + " " + vehicle.Value.GetRawPath());
          fakemechDefs.Add(vehicle.Key);
          mechs.Add(vehicle.Key, new VersionManifestEntry(vehicle.Key
            , vehicle.Value.GetRawPath()
            , BattleTechResourceType.MechDef.ToString()
            , vehicle.Value.AddedOn
            , vehicle.Value.Version.ToString()
            , vehicle.Value.AssetBundleName
            , vehicle.Value.IsAssetBundlePersistent));
        }
      }
      if (___baseManifest.TryGetValue(BattleTechResourceType.VehicleChassisDef, out Dictionary<string, VersionManifestEntry> vchassis)) {
        if (___baseManifest.TryGetValue(BattleTechResourceType.ChassisDef, out Dictionary<string, VersionManifestEntry> mchassis) == false) {
          mchassis = new Dictionary<string, VersionManifestEntry>();
          ___baseManifest.Add(BattleTechResourceType.ChassisDef, mchassis);
        }
        fakeChassisDef.Clear();
        foreach (var vchassi in vchassis) {
          if (mchassis.ContainsKey(vchassi.Key)) { continue; }
          Log.WL(1, "adding ChassisDef " + vchassi.Key + " " + vchassi.Value.GetRawPath());
          fakeChassisDef.Add(vchassi.Key);
          mchassis.Add(vchassi.Key, new VersionManifestEntry(vchassi.Key
            , vchassi.Value.GetRawPath()
            , BattleTechResourceType.ChassisDef.ToString()
            , vchassi.Value.AddedOn
            , vchassi.Value.Version.ToString()
            , vchassi.Value.AssetBundleName
            , vchassi.Value.IsAssetBundlePersistent));
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("OnActorTakeDamage")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_OnActorTakeDamage {
    private delegate void d_refreshMechInfo(CombatHUDMechTray tray);
    private static d_refreshMechInfo i_refreshMechInfo = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetMethod("refreshMechInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUrefreshMechInfo", null, new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_refreshMechInfo = (d_refreshMechInfo)dm.CreateDelegate(typeof(d_refreshMechInfo));
      }
      return true;
    }
    public static void refreshMechInfo(this CombatHUDMechTray tray) { i_refreshMechInfo(tray); }
    public static bool Prefix(CombatHUDMechTray __instance, MessageCenterMessage message) {
      TakeDamageMessage takeDamageMessage = message as TakeDamageMessage;
      if (__instance.DisplayedActor == null || !(takeDamageMessage.affectedObjectGuid == __instance.DisplayedActor.GUID)) { return false; }
      if (__instance.DisplayedActor.UnitType == UnitType.Mech) {
        __instance.MechArmorDisplay.OnActorTakeDamage((MessageCenterMessage)takeDamageMessage);
      } else if (__instance.DisplayedActor.UnitType == UnitType.Vehicle) {
        __instance.VehicleArmorDisplay().OnActorTakeDamage((MessageCenterMessage)takeDamageMessage);
      }
      __instance.refreshMechInfo();
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_Update {
    private delegate float d_timeSinceLastPosChange_get(CombatHUDMechTray tray);
    private static d_timeSinceLastPosChange_get i_timeSinceLastPosChange_get = null;
    private delegate Vector3 d_targetTrayPos_get(CombatHUDMechTray tray);
    private static d_targetTrayPos_get i_targetTrayPos_get = null;
    private delegate Vector3 d_lastTrayPos_get(CombatHUDMechTray tray);
    private static d_lastTrayPos_get i_lastTrayPos_get = null;
    private delegate void d_timeSinceLastPosChange_set(CombatHUDMechTray tray, float value);
    private static d_timeSinceLastPosChange_set i_timeSinceLastPosChange_set = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("timeSinceLastPosChange", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUtimeSinceLastPosChange_get", typeof(float), new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_timeSinceLastPosChange_get = (d_timeSinceLastPosChange_get)dm.CreateDelegate(typeof(d_timeSinceLastPosChange_get));
      }
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("targetTrayPos", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUtargetTrayPos_get", typeof(Vector3), new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_targetTrayPos_get = (d_targetTrayPos_get)dm.CreateDelegate(typeof(d_targetTrayPos_get));
      }
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("lastTrayPos", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUlastTrayPos_get", typeof(Vector3), new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_lastTrayPos_get = (d_lastTrayPos_get)dm.CreateDelegate(typeof(d_lastTrayPos_get));
      }
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("timeSinceLastPosChange", BindingFlags.NonPublic | BindingFlags.Instance).SetMethod;
        var dm = new DynamicMethod("CUtimeSinceLastPosChange_set", null, new Type[] { typeof(CombatHUDMechTray), typeof(float) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_timeSinceLastPosChange_set = (d_timeSinceLastPosChange_set)dm.CreateDelegate(typeof(d_timeSinceLastPosChange_set));
      }
      return true;
    }
    public static float timeSinceLastPosChange(this CombatHUDMechTray tray) { return i_timeSinceLastPosChange_get(tray); }
    public static Vector3 targetTrayPos(this CombatHUDMechTray tray) { return i_targetTrayPos_get(tray); }
    public static Vector3 lastTrayPos(this CombatHUDMechTray tray) { return i_lastTrayPos_get(tray); }
    public static void timeSinceLastPosChange(this CombatHUDMechTray tray, float value) { i_timeSinceLastPosChange_set(tray, value); }
    /*public static bool Prefix(CombatHUDMechTray __instance, AbstractActor ___displayedActor, object ___TrayState, CombatHUD ___HUD) {
      float timeSinceLastPosChange = __instance.timeSinceLastPosChange();
      if (timeSinceLastPosChange < __instance.TimeToMoveTray) {
        __instance.timeSinceLastPosChange(timeSinceLastPosChange + Time.unscaledDeltaTime);
        if (timeSinceLastPosChange < __instance.TimeToMoveTray) {
          __instance.transform.localPosition = (Vector3)Vector2.Lerp((Vector2)__instance.lastTrayPos(), (Vector2)__instance.targetTrayPos(), Mathf.SmoothStep(0.0f, 1f, timeSinceLastPosChange / __instance.TimeToMoveTray));
        } else {
          __instance.transform.localPosition = __instance.targetTrayPos();
          if (___TrayState == __instance.Down())
            __instance.gameObject.SetActive(false);
        }
      }
      if (___displayedActor == null) { return false; }
      __instance.HeatMeter().UpdateHeat(___displayedActor as Mech);
      if ((UnityEngine.Object)__instance.ActorInfo != (UnityEngine.Object)null && ___HUD.SelectedActor != null && (___displayedActor.GUID == ___HUD.SelectedActor.GUID && ___displayedActor.IsAvailableThisPhase)) {
        if (!___displayedActor.HasFiredThisRound || ___displayedActor.CanMoveAfterShooting && !___displayedActor.HasMovedThisRound)
          __instance.ActorInfo.RefreshPredictedHeatInfo();
        if (!___displayedActor.HasBegunActivation || ___displayedActor.HasFiredThisRound && ___displayedActor.CanMoveAfterShooting && !___displayedActor.HasMovedThisRound)
          __instance.ActorInfo.RefreshPredictedStabilityInfo();
      }
      if (___displayedActor.UnitType == UnitType.Mech) {
        __instance.MechArmorDisplay.UpdateMechStructureAndArmor(__instance.shownAttackDirection);
      } else if (___displayedActor.UnitType == UnitType.Vehicle) { 
        __instance.VehicleArmorDisplay().UpdateVehicleStructureAndArmor(__instance.shownAttackDirection);
      };
      return false;
    }*/
    public static void Postfix(CombatHUDMechTray __instance, AbstractActor ___displayedActor) {
      if (___displayedActor == null) { return; }
      if (___displayedActor.UnitType == UnitType.Vehicle) {
        __instance.VehicleArmorDisplay().UpdateVehicleStructureAndArmor(__instance.shownAttackDirection);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmor {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech == null) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(VehicleDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleDef_Refresh {
    public static bool Prefix(VehicleDef __instance, ref VehicleLocationLoadoutDef[] ___Locations) {
      /*List<VehicleLocationLoadoutDef> locations = new List<VehicleLocationLoadoutDef>();
      List<VehicleLocationLoadoutDef> orderedLocations = new List<VehicleLocationLoadoutDef>();
      locations.AddRange(___Locations);
      VehicleLocationLoadoutDef? addloc = null;
      int index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Front);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Front); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Left);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Left); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Right);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Right); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Rear);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Rear); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Turret);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Turret); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      ___Locations = orderedLocations.ToArray();*/
      return true;
    }
  }
  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleChassisDef_Refresh {
    public static bool Prefix(VehicleChassisDef __instance, ref VehicleLocationLoadoutDef[] ___Locations) {
      /*List<VehicleLocationLoadoutDef> locations = new List<VehicleLocationLoadoutDef>();
      List<VehicleLocationLoadoutDef> orderedLocations = new List<VehicleLocationLoadoutDef>();
      locations.AddRange(___Locations);
      VehicleLocationLoadoutDef? addloc = null;
      int index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Front);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Front); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Left);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Left); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Right);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Right); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Rear);
      if (index == -1) { addloc = new VehicleLocationLoadoutDef(VehicleChassisLocations.Rear); } else { addloc = locations[index]; }; orderedLocations.Add(addloc.Value);
      index = locations.FindIndex(loc => loc.Location == VehicleChassisLocations.Turret);
      if (index == -1) {
      } else {
        addloc = locations[index];
        orderedLocations.Add(addloc.Value);
      }; 
      ___Locations = orderedLocations.ToArray();*/
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("refreshMechInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_refreshMechInfo {
    private static object CombatHUDMechTray_MWTrayState_Down = null;
    private static object CombatHUDMechTray_MWTrayState_Up = null;
    private static MethodInfo mi_SetTrayState = null;
    private delegate void d_SetTrayState(CombatHUDMechTray tray, object newState);
    //private static d_SetTrayState i_SetTrayState = null;
    private delegate CombatHUDHeatMeter d_HeatMeter(CombatHUDMechTray tray);
    private static d_HeatMeter i_HeatMeter = null;
    private delegate void d_RefreshStabilityInfo(CombatHUDMechTray tray);
    private static d_RefreshStabilityInfo i_RefreshStabilityInfo = null;
    private static Type MWTrayState = null;
    public static bool Prepare() {
      MWTrayState = typeof(CombatHUDMechTray).GetNestedType("MWTrayState", BindingFlags.NonPublic);
      Log.TWL(0, "MWTrayState:" + MWTrayState.Name);
      CombatHUDMechTray_MWTrayState_Down = Enum.Parse(MWTrayState, "Down");
      CombatHUDMechTray_MWTrayState_Up = Enum.Parse(MWTrayState, "Up");
      mi_SetTrayState = typeof(CombatHUDMechTray).GetMethod("SetTrayState", BindingFlags.NonPublic | BindingFlags.Instance);
      /*{
        MethodInfo method = typeof(CombatHUDMechTray).GetMethod("SetTrayState", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUSetTrayState", null, new Type[] { typeof(CombatHUDMechTray), typeof(object) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Callvirt, method);
        gen.Emit(OpCodes.Ret);
        i_SetTrayState = (d_SetTrayState)dm.CreateDelegate(typeof(d_SetTrayState));
      }*/
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetProperty("HeatMeter", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUHeatMeter_get", typeof(CombatHUDHeatMeter), new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_HeatMeter = (d_HeatMeter)dm.CreateDelegate(typeof(d_HeatMeter));
      }
      {
        MethodInfo method = typeof(CombatHUDMechTray).GetMethod("RefreshStabilityInfo", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CURefreshStabilityInfo", null, new Type[] { typeof(CombatHUDMechTray) }, typeof(CombatHUDMechTray));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_RefreshStabilityInfo = (d_RefreshStabilityInfo)dm.CreateDelegate(typeof(d_RefreshStabilityInfo));
      }
      return true;
    }
    public static object Up(this CombatHUDMechTray tray) { return CombatHUDMechTray_MWTrayState_Up; }
    public static object Down(this CombatHUDMechTray tray) { return CombatHUDMechTray_MWTrayState_Down; }
    public static void SetTrayState(this CombatHUDMechTray tray, object state) {
      mi_SetTrayState.Invoke(tray, new object[1] { state });
      //if (state == 0) {
      //i_SetTrayState(tray, state);
      //} else {
      //i_SetTrayState(tray, CombatHUDMechTray_MWTrayState_Up);
      //}
    }
    public static CombatHUDHeatMeter HeatMeter(this CombatHUDMechTray tray) { return i_HeatMeter(tray); }
    public static void RefreshStabilityInfo(this CombatHUDMechTray tray) { i_RefreshStabilityInfo(tray); }
    public static bool Prefix(CombatHUDMechTray __instance, CombatHUDStatusPanel ___StatusPanel, AbstractActor ___displayedActor) {
      ___StatusPanel.DisplayedCombatant = (ICombatant)___displayedActor;
      if (___displayedActor == null) {
        //typeof(CombatHUDMechTray).GetMethod("SetTrayState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { CombatHUDMechTray_MWTrayState_Down });
        __instance.SetTrayState(CombatHUDMechTray_MWTrayState_Down);
      } else {
        //typeof(CombatHUDMechTray).GetMethod("SetTrayState", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { CombatHUDMechTray_MWTrayState_Up });
        __instance.SetTrayState(CombatHUDMechTray_MWTrayState_Up);
        __instance.MechNameText.SetText(___displayedActor.DisplayName, (object[])Array.Empty<object>());
        Mech mech = ___displayedActor as Mech;
        Vehicle vehicle = ___displayedActor as Vehicle;
        __instance.HeatMeter().DisplayedActor = mech;
        if ((mech == null) && (vehicle == null)) {
          Debug.LogWarning((object)"Trying to use MechTray to show a non-mech non-vehicle actor");
        } else {
          Pilot pilot = ___displayedActor.GetPilot();
          if (pilot == null) {
            __instance.WarriorNameText.SetText("No pilot info", (object[])Array.Empty<object>());
          } else {
            __instance.WarriorNameText.SetText(pilot.Description.Callsign, (object[])Array.Empty<object>());
            if (mech != null) {
              __instance.MechPaperDoll().SetActive(true);
              __instance.MechArmorDisplay.DisplayedMech = mech;
              __instance.VehicleArmorDisplay().DisplayedVehicle = vehicle;
              __instance.VehicleArmorDisplay().gameObject.SetActive(false);
              __instance.RefreshStabilityInfo();
            } else if (vehicle != null) {
              __instance.MechPaperDoll().SetActive(false);
              __instance.VehicleArmorDisplay().gameObject.SetActive(true);
              __instance.MechArmorDisplay.DisplayedMech = mech;
              __instance.VehicleArmorDisplay().DisplayedVehicle = vehicle;
            }
          }
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(HUDVehicleArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDVehicleArmorReadout_Init {
    private static HUDVehicleArmorReadout fVehicleArmorDisplay = null;
    private static GameObject fMechPaperDoll = null;
    public static HUDVehicleArmorReadout VehicleArmorDisplay(this CombatHUDMechTray tray) {
      return fVehicleArmorDisplay;
    }
    public static GameObject MechPaperDoll(this CombatHUDMechTray tray) {
      return fMechPaperDoll;
    }
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static void Postfix(HUDVehicleArmorReadout __instance, CombatHUD HUD, bool usedForCalledShots) {
      Log.TWL(0, "HUDVehicleArmorReadout.Init gameObject:" + __instance.gameObject.name + " parent:" + __instance.gameObject.transform.parent.gameObject.name);
      if (usedForCalledShots) { return; }
      if (__instance.gameObject.name == "MechTray(vehicle)") { return; }
      List<CombatHUDVehicleArmorHover> result = new List<CombatHUDVehicleArmorHover>();
      __instance.GetComponentsInChildren<CombatHUDVehicleArmorHover>(true, result);
      Log.WL(1, "parent vehicleSlots:" + result.Count);
      for (int index = 0; index < result.Count; ++index) {
        CombatHUDVehicleArmorHover armorSlot = result[index];
        CombatHUDTooltipHoverElement ToolTip = armorSlot.GetComponent<CombatHUDTooltipHoverElement>();
        //typeof(CombatHUDTooltipHoverElement).GetProperty("ToolTip")
        if (ToolTip == null) { continue; }
        Log.WL(2, "[" + index + "] toolTip:" + ToolTip.GetInstanceID() + " hoverpanel:" + (ToolTip.ToolTip == null ? "null" : ToolTip.ToolTip.GetInstanceID().ToString()));
        //ToolTip.transform.position = slotPos;
        //Log.WL(0," -> "+ ToolTip.transform.position);
      }
      GameObject vehiclePaperDoll = GameObject.Instantiate(__instance.gameObject);
      vehiclePaperDoll.name = "MechTray(vehicle)";
      vehiclePaperDoll.transform.SetParent(HUD.MechTray.MechArmorDisplay.gameObject.transform);
      vehiclePaperDoll.transform.localScale = Vector3.one * 1.2f;
      fMechPaperDoll = HUD.MechTray.MechArmorDisplay.gameObject.transform.Find("MechPaperDoll").gameObject;
      GameObject MechTray_ArmorTorso = fMechPaperDoll.transform.Find("MechTray_ArmorTorso").gameObject;
      GameObject MechTray_ArmorLL = fMechPaperDoll.transform.Find("MechTray_ArmorLL").gameObject;
      vehiclePaperDoll.transform.position = MechTray_ArmorTorso.transform.position;
      RectTransform MechTray_ArmorLLrt = MechTray_ArmorLL.transform as RectTransform;
      RectTransform vehiclePaperDollrt = vehiclePaperDoll.transform as RectTransform;
      Vector3[] MechTray_ArmorLLc = new Vector3[4];
      Vector3[] vehiclePaperDollc = new Vector3[4];
      MechTray_ArmorLLrt.GetWorldCorners(MechTray_ArmorLLc);
      vehiclePaperDollrt.GetWorldCorners(vehiclePaperDollc);
      float bottom_desired = MechTray_ArmorLLc[0].y;
      float bottom_real = vehiclePaperDollc[0].y;
      Vector3 realposition = vehiclePaperDoll.transform.localPosition;
      Log.W(1, "vehiclePaperDoll.transform.position " + vehiclePaperDoll.transform.position);
      realposition.y += (bottom_desired - bottom_real);
      vehiclePaperDoll.transform.localPosition = realposition;
      Log.WL(0, " -> " + vehiclePaperDoll.transform.position);

      vehiclePaperDoll.SetActive(true);
      fVehicleArmorDisplay = vehiclePaperDoll.GetComponent<HUDVehicleArmorReadout>();
      List<CombatHUDMechTrayArmorHover> mechSlots = new List<CombatHUDMechTrayArmorHover>();
      HUD.MechTray.MechArmorDisplay.GetComponentsInChildren<CombatHUDMechTrayArmorHover>(true, mechSlots);
      Vector3 slotPos = Vector3.zero;
      Log.WL(1, "mechSlots:" + mechSlots.Count);
      GameObject DisplayPosition = null;
      Vector3 DisplayOffset = Vector3.zero;
      CombatHUDTooltipHoverElement mechToolTip = null;
      for (int index = 0; index < mechSlots.Count; ++index) {
        CombatHUDMechTrayArmorHover mechSlot = mechSlots[index];
        CombatHUDTooltipHoverElement ToolTip = mechSlot.GetComponent<CombatHUDTooltipHoverElement>();
        mechToolTip = ToolTip;
        if (ToolTip == null) { continue; }
        DisplayPosition = ToolTip.DisplayPosition;
        DisplayOffset = ToolTip.DisplayOffset;
        Log.WL(2, "[" + index + "] tooltip:" + ToolTip.GetInstanceID() + " orientation:" + ToolTip.Orientation + " DisplayPosition:" + ToolTip.DisplayPosition.GetInstanceID());
        slotPos = mechSlot.transform.position;
      }
      vehiclePaperDoll.GetComponentsInChildren<CombatHUDVehicleArmorHover>(true, result);
      Log.WL(1, "vehicleSlots:" + result.Count);
      for (int index = 0; index < result.Count; ++index) {
        CombatHUDVehicleArmorHover armorSlot = result[index];
        CombatHUDTooltipHoverElement ToolTip = armorSlot.GetComponent<CombatHUDTooltipHoverElement>();
        //typeof(CombatHUDTooltipHoverElement).GetProperty("ToolTip")
        if (ToolTip == null) { continue; }
        ToolTip.DisplayPosition = DisplayPosition;
        ToolTip.Orientation = CombatHUDTooltipHoverElement.ToolTipOrientation.Up;
        ToolTip.DisplayOffset = DisplayOffset;
        Log.WL(2, "[" + index + "] toolTip:" + ToolTip.GetInstanceID() + " hoverpanel:" + (ToolTip.ToolTip == null ? "null" : ToolTip.ToolTip.GetInstanceID().ToString()));
        //ToolTip.transform.position = slotPos;
        //Log.WL(0," -> "+ ToolTip.transform.position);
      }
      fVehicleArmorDisplay.Init(HUD, false);
      fVehicleArmorDisplay.ArmorBar = HUD.MechTray.MechArmorDisplay.ArmorBar;
      fVehicleArmorDisplay.StructureBar = HUD.MechTray.MechArmorDisplay.StructureBar;
      fVehicleArmorDisplay.HoverInfoTextArmor = HUD.MechTray.MechArmorDisplay.HoverInfoTextArmor;
      fVehicleArmorDisplay.HoverInfoTextStructure = HUD.MechTray.MechArmorDisplay.HoverInfoTextStructure;
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta))]
  [HarmonyPatch("FinalizeLances")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SkirmishSettings_Beta_FinalizeLances {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static void Postfix(SkirmishSettings_Beta __instance, ref LanceConfiguration __result, LancePreviewPanel ___playerLancePreview, UIManager ___uiManager) {
      Log.TWL(0, "SkirmishSettings_Beta.FinalizeLances units:" + __result.Lances[___playerLancePreview.playerGUID].Count);
      /*if (__result.Lances[___playerLancePreview.playerGUID].Count < 4) {
        VehicleDef testUnitDef = ___uiManager.dataManager.VehicleDefs.Get("vehicledef_WARRIOR_VTOL");
        PilotDef testPilotDef = ___uiManager.dataManager.PilotDefs.Get("pilot_kbeta_kraken");
        Log.WL(1, "unit:"+(testUnitDef == null?"null":"not null"));
        Log.WL(1, "pilot:" + (testPilotDef == null ? "null" : "not null"));
        if ((testUnitDef != null)&&(testPilotDef != null)) {
          __result.AddUnit(___playerLancePreview.playerGUID, testUnitDef,testPilotDef);
        }
      }*/
      //return true;
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SkirmishSettings_Beta_OnAddedToHierarchy {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static void Prefix(SkirmishSettings_Beta __instance) {
      Log.TWL(0, "SkirmishSettings_Beta.OnAddedToHierarchy");
      Core.InitLancesLoadoutDefault();
    }
  }
  [HarmonyPatch(typeof(CombatHUDHeatMeter))]
  [HarmonyPatch("RefreshHeatInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDHeatMeter_RefreshHeatInfo {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(CombatHUDHeatMeter __instance, float ___underlyingHeatTarget, float ___underlyingHeatDisplayed, float ___underlyingPredictionTarget, float ___underlyingPredictionDisplayed) {
      //Log.TWL(0, "CombatHUDHeatMeter.RefreshHeatInfo");
      if (__instance.DisplayedActor == null) {
        ___underlyingHeatTarget = 0f;
        ___underlyingHeatDisplayed = 0f;
        ___underlyingPredictionTarget = 0f;
        ___underlyingPredictionDisplayed = 0f;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("UpdateToolTipsFiring")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_UpdateToolTipsFiring {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(CombatHUDWeaponSlot __instance, ICombatant target, CombatHUD ___HUD, Weapon ___displayedWeapon, ref int ___modifier, CombatGameState ___Combat) {
      return true;
      AbstractActor abstractActor = target as AbstractActor;
      LineOfFireLevel lofLevel = ___HUD.SelectionHandler.ActiveState.FiringPreview.GetPreviewInfo((ICombatant)abstractActor).LOFLevel;
      bool flag = ___HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.FireMorale;
      __instance.ToolTipHoverElement.BasicModifierInt = (int)___Combat.ToHit.GetAllModifiers(___HUD.SelectedActor, ___displayedWeapon, target, ___HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, lofLevel, flag);
      ___modifier = (int)___Combat.ToHit.GetRangeModifier(___displayedWeapon, ___HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition);
      float num = Vector3.Distance(___displayedWeapon.parent.TargetPosition, target.TargetPosition);
      if ((double)num < (double)___displayedWeapon.MinRange)
        __instance.AddToolTipDetail("MIN RANGE", ___modifier);
      else if ((double)num < (double)___displayedWeapon.ShortRange)
        __instance.AddToolTipDetail("SHORT RANGE", ___modifier);
      else if ((double)num < (double)___displayedWeapon.MediumRange)
        __instance.AddToolTipDetail("MEDIUM RANGE", ___modifier);
      else if ((double)num < (double)___displayedWeapon.LongRange)
        __instance.AddToolTipDetail("LONG RANGE", ___modifier);
      else if ((double)num < (double)___displayedWeapon.MaxRange)
        __instance.AddToolTipDetail("MAX RANGE", ___modifier);
      else
        __instance.AddToolTipDetail("OUT OF RANGE", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetCoverModifier(___HUD.SelectedActor, target, lofLevel);
      __instance.AddToolTipDetail("OBSTRUCTED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetSelfSpeedModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("MOVED SELF", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetSelfSprintedModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("SPRINTED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetSelfArmMountedModifier(___displayedWeapon);
      __instance.AddToolTipDetail("ARM MOUNTED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetStoodUpModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("STOOD UP", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetHeightModifier(___HUD.SelectionHandler.ActiveState.PreviewPos.y, target.TargetPosition.y);
      __instance.AddToolTipDetail("HEIGHT DIFF", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetHeatModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("HEAT", ___modifier);
      if (abstractActor != null) {
        if (___displayedWeapon.parent.occupiedDesignMask != null) {
          ___modifier = (int)___Combat.ToHit.GetSelfTerrainModifier(___HUD.SelectionHandler.ActiveState.PreviewPos, false);
          __instance.AddToolTipDetail(Strings.T("FROM {0}", (object)___displayedWeapon.parent.occupiedDesignMask.Description.Name), ___modifier);
        }
        if (abstractActor.occupiedDesignMask != null) {
          ___modifier = (int)___Combat.ToHit.GetTargetTerrainModifier(target, target.CurrentPosition, false);
          __instance.AddToolTipDetail(Strings.T("INTO {0}", (object)abstractActor.occupiedDesignMask.Description.Name), ___modifier);
        }
      }
      ___modifier = (int)___Combat.ToHit.GetTargetSpeedModifier(target, ___displayedWeapon);
      __instance.AddToolTipDetail("TARGET MOVED", ___modifier);
      if (___displayedWeapon.parent.UnitType == UnitType.Mech) {
        ___modifier = (int)MechStructureRules.GetToHitModifierLocationDamage((Mech)___displayedWeapon.parent, ___displayedWeapon);
        __instance.AddToolTipDetail(Strings.T("{0} DAMAGED", (object)Mech.GetAbbreviatedChassisLocation((ChassisLocations)___displayedWeapon.Location)), ___modifier);
      }
      ___modifier = (int)CombatHUDWeaponSlot_UpdateToolTipsSelf.GetToHitModifierWeaponDamage(___displayedWeapon.parent, ___displayedWeapon);
      __instance.AddToolTipDetail("WEAPON DAMAGED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetTargetSizeModifier(target);
      __instance.AddToolTipDetail("TARGET SIZE", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetTargetShutdownModifier(target, false);
      __instance.AddToolTipDetail("TARGET SHUTDOWN", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetTargetProneModifier(target, false);
      __instance.AddToolTipDetail("TARGET PRONE", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetWeaponAccuracyModifier(___HUD.SelectedActor, ___displayedWeapon);
      __instance.AddToolTipDetail("WEAPON ACCURACY", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetAttackerAccuracyModifier(___HUD.SelectedActor);
      if (___modifier > 0)
        __instance.AddToolTipDetail("SENSORS IMPAIRED", ___modifier);
      else
        __instance.AddToolTipDetail("INSPIRED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetEnemyEffectModifier(target, ___displayedWeapon);
      __instance.AddToolTipDetail("ENEMY EFFECTS", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetRefireModifier(___displayedWeapon);
      __instance.AddToolTipDetail("REFIRE", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetTargetDirectFireModifier(target, lofLevel < LineOfFireLevel.LOFObstructed && ___displayedWeapon.IndirectFireCapable);
      __instance.AddToolTipDetail("SENSOR LOCK", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetIndirectModifier(___displayedWeapon.parent, lofLevel < LineOfFireLevel.LOFObstructed && ___displayedWeapon.IndirectFireCapable);
      __instance.AddToolTipDetail("INDIRECT FIRE", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetMoraleAttackModifier(target, flag);
      __instance.AddToolTipDetail(___Combat.Constants.CombatUIConstants.MoraleAttackDescription.Name, ___modifier);
      return false;
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoveBase))]
  [HarmonyPatch("CreateMoveOrders")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class SelectionStateMoveBase_CreateMoveOrders {
    private delegate CombatHUD getHUDDelegate(SelectionState state);
    private static getHUDDelegate getHUDInvoker = null;
    private delegate CombatGameState getCombatDelegate(SelectionState panel);
    private static getCombatDelegate getCombatInvoker = null;
    private delegate void d_PublishInvocation(SelectionState state, MessageCenter messageCenter, MessageCenterMessage invocation);
    private static d_PublishInvocation i_PublishInvocation = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(SelectionState).GetProperty("HUD", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUHUDget", typeof(CombatHUD), new Type[] { typeof(SelectionState) }, typeof(SelectionState));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        getHUDInvoker = (getHUDDelegate)dm.CreateDelegate(typeof(getHUDDelegate));
      }
      {
        MethodInfo method = typeof(SelectionState).GetProperty("Combat", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUCombatget", typeof(CombatGameState), new Type[] { typeof(SelectionState) }, typeof(SelectionState));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        getCombatInvoker = (getCombatDelegate)dm.CreateDelegate(typeof(getCombatDelegate));
      }
      {
        MethodInfo method = typeof(SelectionState).GetMethod("PublishInvocation", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUPublishInvocation", null, new Type[] { typeof(SelectionState), typeof(MessageCenter), typeof(MessageCenterMessage) }, typeof(SelectionState));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_PublishInvocation = (d_PublishInvocation)dm.CreateDelegate(typeof(d_PublishInvocation));
      }
      return true;
    }
    public static CombatHUD HUD(this SelectionState state) { return getHUDInvoker(state); }
    public static CombatGameState Combat(this SelectionState state) { return getCombatInvoker(state); }
    public static void PublishInvocation(this SelectionState state, MessageCenter messageCenter, MessageCenterMessage invocation) {
      i_PublishInvocation(state, messageCenter, invocation);
    }
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(SelectionStateMoveBase __instance, bool isJump, ref bool __result) {
      Mech mech = __instance.SelectedActor as Mech;
      Vehicle vehicle = __instance.SelectedActor as Vehicle;
      if ((mech == null) && (vehicle == null)) { __result = false; return false; }
      bool abilityConsumesFiring = __instance.HUD().MechWarriorTray.ConfirmAbilities(AbilityDef.ActivationTiming.ConsumedByMovement);
      if (!abilityConsumesFiring && __instance.HUD().MechWarriorTray.DoneWithMechButton.IsTutorialSuppressed) {
        __instance.FiringPreview.Recalc(__instance.SelectedActor, __instance.PreviewPos, __instance.PreviewRot, false, false);
        if (__instance.FiringPreview.AllPossibleTargets.Count < 1)
          abilityConsumesFiring = true;
      }
      if (mech != null) {
        __instance.PublishInvocation(__instance.Combat().MessageCenter, !isJump ? (MessageCenterMessage)new MechMovementInvocation(mech, abilityConsumesFiring) : (MessageCenterMessage)new MechJumpInvocation(mech, (ICombatant)null, abilityConsumesFiring));
      } else if (vehicle != null) {
        __instance.PublishInvocation(__instance.Combat().MessageCenter, (MessageCenterMessage)new AbstractActorMovementInvocation(vehicle, abilityConsumesFiring));
      }
      __instance.OnInactivate();
      __instance.HUD().PlayAudioEvent(AudioEventList_ui.ui_mech_move);
      if (!__instance.Combat().TurnDirector.IsInterleaved) {
        __instance.HUD().SelectionHandler.AutoSelectActor();
      }
      __instance.HUD().SidePanel.ForceHide();
      __result = true;
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("UpdateToolTipsSelf")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDWeaponSlot_UpdateToolTipsSelf {
    private delegate void d_AddToolTipDetail(CombatHUDWeaponSlot slot, string description, int modifier);
    private static d_AddToolTipDetail i_AddToolTipDetail = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDWeaponSlot).GetMethod("AddToolTipDetail", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CUAddToolTipDetail", null, new Type[] { typeof(CombatHUDWeaponSlot), typeof(string), typeof(int) }, typeof(CombatHUDWeaponSlot));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AddToolTipDetail = (d_AddToolTipDetail)dm.CreateDelegate(typeof(d_AddToolTipDetail));
      }
      return true;
    }
    public static void AddToolTipDetail(this CombatHUDWeaponSlot slot, string description, int modifier) {
      i_AddToolTipDetail(slot, description, modifier);
    }

    public static float GetToHitModifierWeaponDamage(AbstractActor attacker, Weapon weapon) {
      if (weapon.DamageLevel == ComponentDamageLevel.Misaligned || weapon.DamageLevel == ComponentDamageLevel.Penalized)
        return attacker.Combat.Constants.ToHit.ToHitSelfWeaponDamaged;
      return 0.0f;
    }
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(CombatHUDWeaponSlot __instance, Weapon ___displayedWeapon, ref int ___modifier, CombatGameState ___Combat) {
      //Log.TWL(0, "CombatHUDHeatMeter.RefreshHeatInfo");
      __instance.ToolTipHoverElement.BasicString = new Localize.Text(___displayedWeapon.Name, (object[])Array.Empty<object>());
      ___modifier = (int)___Combat.ToHit.GetSelfSpeedModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("MOVED SELF", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetSelfSprintedModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("SPRINTED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetStoodUpModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("STOOD UP", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetHeatModifier(___displayedWeapon.parent);
      __instance.AddToolTipDetail("HEAT", ___modifier);

      Mech mech = ___displayedWeapon.parent as Mech;
      if (mech != null) {
        ___modifier = (int)MechStructureRules.GetToHitModifierLocationDamage(mech, ___displayedWeapon);
        __instance.AddToolTipDetail(Strings.T("{0} DAMAGED", (object)Mech.GetAbbreviatedChassisLocation((ChassisLocations)___displayedWeapon.Location)), ___modifier);
      }
      ___modifier = (int)CombatHUDWeaponSlot_UpdateToolTipsSelf.GetToHitModifierWeaponDamage(___displayedWeapon.parent, ___displayedWeapon);
      __instance.AddToolTipDetail("WEAPON DAMAGED", ___modifier);
      ___modifier = (int)___Combat.ToHit.GetRefireModifier(___displayedWeapon);
      __instance.AddToolTipDetail("REFIRE", ___modifier);
      __instance.ToolTipHoverElement.UseModifier = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponPanel))]
  [HarmonyPatch("RefreshDisplayedWeapons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class CombatHUDWeaponPanel_RefreshDisplayedWeapons {
    private delegate CombatHUD getHUDDelegate(CombatHUDWeaponPanel panel);
    private delegate ICombatant getCombatantDelegate(CombatHUDWeaponPanel panel);
    private static getHUDDelegate getHUDInvoker = null;
    private static getCombatantDelegate get_target = null;
    private static getCombatantDelegate get_hoveredTarget = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDWeaponPanel).GetProperty("HUD", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUHUDget", typeof(CombatHUD), new Type[] { typeof(CombatHUDWeaponPanel) }, typeof(CombatHUDWeaponPanel));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        getHUDInvoker = (getHUDDelegate)dm.CreateDelegate(typeof(getHUDDelegate));
      }
      {
        MethodInfo method = typeof(CombatHUDWeaponPanel).GetProperty("target", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUget_target", typeof(ICombatant), new Type[] { typeof(CombatHUDWeaponPanel) }, typeof(CombatHUDWeaponPanel));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        get_target = (getCombatantDelegate)dm.CreateDelegate(typeof(getCombatantDelegate));
      }
      {
        MethodInfo method = typeof(CombatHUDWeaponPanel).GetProperty("hoveredTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetMethod;
        var dm = new DynamicMethod("CUget_hoveredTarget", typeof(ICombatant), new Type[] { typeof(CombatHUDWeaponPanel) }, typeof(CombatHUDWeaponPanel));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        get_hoveredTarget = (getCombatantDelegate)dm.CreateDelegate(typeof(getCombatantDelegate));
      }
      return true;
    }
    public static CombatHUD HUD(this CombatHUDWeaponPanel panel) { return getHUDInvoker(panel); }
    public static ICombatant target(this CombatHUDWeaponPanel panel) { return get_target(panel); }
    public static ICombatant hoveredTarget(this CombatHUDWeaponPanel panel) { return get_hoveredTarget(panel); }
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(CombatHUDWeaponPanel __instance, bool consideringJump, bool useCOILPathingPreview, AbstractActor ___displayedActor, CombatHUDWeaponSlot ___meleeSlot, CombatHUDWeaponSlot ___dfaSlot, List<CombatHUDWeaponSlot> ___WeaponSlots) {
      SelectionState activeState1 = __instance.HUD().SelectionHandler.ActiveState;
      ICombatant target = activeState1 == null || !(activeState1 is SelectionStateMove) ? __instance.target() ?? __instance.hoveredTarget() : __instance.hoveredTarget() ?? __instance.target();
      bool sprinting = false;
      bool vehicle = false;
      if (___displayedActor != null) {
        Mech mech = ___displayedActor as Mech;
        if (mech != null) {
          ___meleeSlot.DisplayedWeapon = mech.MeleeWeapon;
          ___dfaSlot.DisplayedWeapon = mech.DFAWeapon;
        } else {
          ___meleeSlot.DisplayedWeapon = null;
          ___dfaSlot.DisplayedWeapon = null;
          vehicle = true;
        }
        sprinting = ___displayedActor.HasSprintedThisRound;
      }
      SelectionStateFireMulti activeState2 = __instance.HUD().SelectionHandler.ActiveState as SelectionStateFireMulti; ;
      int? evasivePipOverride = new int?();
      for (int index = 0; index < ___WeaponSlots.Count; ++index) {
        CombatHUDWeaponSlot weaponSlot = ___WeaponSlots[index];
        if (activeState2 != null && weaponSlot.DisplayedWeapon != null && weaponSlot.DisplayedWeapon.Type != WeaponType.Melee)
          target = activeState2.GetSelectedTarget(weaponSlot.DisplayedWeapon) ?? __instance.target() ?? __instance.hoveredTarget();
        if (weaponSlot.DisplayedWeapon != null && weaponSlot.DisplayedWeapon.Type == WeaponType.COIL && (!evasivePipOverride.HasValue && ___displayedActor != null) && !___displayedActor.HasMovedThisRound && (useCOILPathingPreview || activeState1 != null && activeState1.SelectionType == SelectionType.Move))
          evasivePipOverride = !consideringJump ? new int?(___displayedActor.GetEvasivePipsResult(WayPoint.GetDistFromWaypointList(___displayedActor.CurrentPosition, ActorMovementSequence.ExtractWaypointsFromPath(___displayedActor, ___displayedActor.Pathing.CurrentPath, ___displayedActor.Pathing.ResultDestination, (ICombatant)null, ___displayedActor.Pathing.MoveType)), false, ___displayedActor.Pathing.MoveType == MoveType.Sprinting, ___displayedActor.Pathing.MoveType == MoveType.Melee)) : new int?(___displayedActor.GetEvasivePipsResult(Vector3.Distance(___displayedActor.CurrentPosition, ___displayedActor.JumpPathing.ResultDestination), true, false, false));
        weaponSlot.RefreshDisplayedWeapon(target, evasivePipOverride, consideringJump, sprinting);
      }
      if (vehicle == false) {
        ___meleeSlot.gameObject.SetActive(true);
        ___meleeSlot.RefreshDisplayedWeapon(target, new int?(), false, false);
      } else {
        ___meleeSlot.gameObject.SetActive(false);
      }
      if (___displayedActor == null || ___displayedActor.WorkingJumpjets <= 0) { return false; }
      ___dfaSlot.RefreshDisplayedWeapon(target, new int?(), false, false);
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechwarriorTray))]
  [HarmonyPatch("InitAbilityButtons")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDMechwarriorTray_InitAbilityButtons {
    //public static readonly string playerGUID = "bf40fd39-ccf9-47c4-94a6-061809681140";
    public static bool Prefix(CombatHUDMechwarriorTray __instance, AbstractActor actor) {
      Pilot pilot = actor.GetPilot();
      CombatHUDActionButton[] AbilityButtons = (CombatHUDActionButton[])typeof(CombatHUDMechwarriorTray).GetProperty("AbilityButtons", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance, new object[] { });
      if (pilot != null) {
        List<Ability> abilityList = new List<Ability>((IEnumerable<Ability>)pilot.ActiveAbilities);
        for (int index = 0; index < abilityList.Count; ++index) {
          AbilityButtons[index].InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(abilityList[index].Def.Targeting, false), abilityList[index], abilityList[index].Def.AbilityIcon, abilityList[index].Def.Description.Id, abilityList[index].Def.Description.Name, actor);
          AbilityButtons[index].isClickable = true;
        }
        List<Ability> all = pilot.PassiveAbilities.FindAll((Predicate<Ability>)(x => x.Def.DisplayParams == AbilityDef.DisplayParameters.ShowInMWTRay));
        for (int count = abilityList.Count; count < AbilityButtons.Length; ++count) {
          if (count < abilityList.Count + all.Count) {
            int index = count - abilityList.Count;
            AbilityButtons[count].InitButton(SelectionType.None, all[index], all[index].Def.AbilityIcon, all[index].Def.Description.Id, all[index].Def.Description.Name, actor);
            AbilityButtons[count].isClickable = false;
            AbilityButtons[count].RefreshUIColors();
          } else {
            AbilityButtons[count].InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
            AbilityButtons[count].isClickable = false;
            AbilityButtons[count].RefreshUIColors();
          }
        }
      }
      if (actor.team.CommandAbilities.Count > 0) {
        Ability commandAbility = actor.team.CommandAbilities[0];
        __instance.CommandButton.InitButton(CombatHUDMechwarriorTray.GetSelectionTypeFromTargeting(commandAbility.Def.Targeting, false), commandAbility, commandAbility.Def.AbilityIcon, commandAbility.Def.Description.Id, commandAbility.Def.Description.Name, (AbstractActor)null);
      } else
        __instance.CommandButton.InitButton(SelectionType.None, (Ability)null, (SVGAsset)null, "", "", (AbstractActor)null);
      return false;
    }
  }

}