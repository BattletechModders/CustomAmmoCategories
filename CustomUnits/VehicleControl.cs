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
using BattleTech.Data;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using Harmony;
using HBS;
using HBS.Collections;
using HBS.Data;
using HBS.Util;
using InControl;
using IRBTModUtils;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SVGImporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetActorInfoFromVisLevel")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(VisibilityLevel) })]
  public static class Mech_GetActorInfoFromVisLevel {
    public static void Postfix(Mech __instance, VisibilityLevel visLevel, ref Localize.Text __result) {
      if (Core.Settings.LowVisDetected) { return; }
      if (visLevel == VisibilityLevel.LOSFull || visLevel == VisibilityLevel.BlipGhost) { return; }
      if (__instance.TrooperSquad()) {
        if(visLevel >= VisibilityLevel.Blip4Maximum) {
          __result = new Localize.Text("SQUAD, {0}t", new object[1] { (object)__instance.MechDef.Chassis.Tonnage });
        } else if (visLevel == VisibilityLevel.Blip1Type) {
          __result = new Localize.Text("UNKNOWN SQUAD");
        }
      }else if (__instance.FakeVehicle()) {
        if (visLevel >= VisibilityLevel.Blip4Maximum) {
          __result = new Localize.Text("VEHICLE, {0}t", new object[1] { (object)__instance.MechDef.Chassis.Tonnage });
        } else if (visLevel == VisibilityLevel.Blip1Type) {
          __result = new Localize.Text("UNKNOWN VEHICLE");
        }
      }
    }
  }
  [HarmonyPatch(typeof(StatTooltipData))]
  [HarmonyPatch("SetHeatData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class StatTooltipData_SetHeatData {
    public static bool Prefix(StatTooltipData __instance, MechDef def) {
      if (def == null) { return true; }
      if (def.IsVehicle() == false) { return true; }
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
      if (mechDef.IsVehicle() == false) { return; }
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
      if (theMech.IsVehicle() == false) {
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
      if (__instance.mechDef.IsVehicle()) { theWidget.manufacturerName.SetText("{0}", "__/VEHICLE/__"); }
    }
  }
  [HarmonyPatch(typeof(InventoryDataObject_SalvageFullMech))]
  [HarmonyPatch("RefreshInfoOnWidget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(InventoryItemElement) })]
  public static class InventoryDataObject_SalvageFullMech_RefreshInfoOnWidget {
    public static void Postfix(InventoryDataObject_ShopFullMech __instance, InventoryItemElement theWidget) {
      if (__instance.mechDef.IsVehicle()) { theWidget.manufacturerName.SetText("{0}", "__/VEHICLE/__"); }
    }
  }
  [HarmonyPatch(typeof(StatTooltipData))]
  [HarmonyPatch("SetMeleeData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class StatTooltipData_SetMeleeData {
    public static bool Prefix(StatTooltipData __instance, MechDef def) {
      if (def == null) { return true; }
      if (def.IsVehicle() == false) { return true; }
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
  public static class HUDMechArmorReadout_GetInitialStructureForLocationMechDef {
    public static void Postfix(MechDef curMech, ChassisLocations location,ref float __result) {
      if (curMech == null) { return; }
      if (curMech.Chassis == null) { return; }
      LocationDef locDef = curMech.Chassis.GetLocationDef(location);
      if ((locDef.InternalStructure <= 1.0f) && (locDef.MaxArmor <= 0f)) { __result = 0f; }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("GetInitialStructureForLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisDef), typeof(ChassisLocations) })]
  public static class HUDMechArmorReadout_GetInitialStructureForLocationChassisDef {
    public static void Postfix(ChassisDef chassis, ChassisLocations location, ref float __result) {
      if (chassis == null) { return; }
      LocationDef locDef = chassis.GetLocationDef(location);
      if ((locDef.InternalStructure <= 1.0f) && (locDef.MaxArmor <= 0f)) { __result = 0f; }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("GetCurrentStructureForLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(ArmorLocation) })]
  public static class HUDMechArmorReadout_GetCurrentStructureForLocationMechDef {
    public static void Postfix(MechDef curMech, ArmorLocation location, ref float __result) {
      if (curMech == null) { return; }
      if (curMech.Chassis == null) { return; }
      Log.TWL(0, "HUDMechArmorReadout.GetCurrentStructureForLocation " + curMech.ChassisID + " location:" + location);
      LocationDef locDef = curMech.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(location));
      if ((locDef.InternalStructure <= 1.0f) && (locDef.MaxArmor <= 0f)) { __result = 0f; }
      return;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("GetCurrentStructureForLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisDef), typeof(ArmorLocation) })]
  public static class HUDMechArmorReadout_GetCurrentStructureForLocationChassisDef {
    public static void Postfix(ChassisDef chassis, ArmorLocation location, ref float __result) {
      if (chassis == null) { return; }
      Log.TWL(0, "HUDMechArmorReadout.GetCurrentStructureForLocation " + chassis.Description.Id + " location:" + location);
      LocationDef locDef = chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(location));
      if ((locDef.InternalStructure <= 1.0f) && (locDef.MaxArmor <= 0f)) { __result = 0f; }
      return;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsAnyStructureExposed")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsAnyStructureExposed {
    public static void Postfix(Mech __instance, ref bool __result) {
      if (__instance.MechDef == null) { return; }
      if (__instance.MechDef.Chassis == null) { return; }
      if ((__instance.MechDef.Chassis.Head.MaxArmor > 0f) && (__instance.MechDef.Chassis.Head.InternalStructure > 1f)) { if (__instance.HeadArmor <= 0.0f) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.LeftArm.MaxArmor > 0f) && (__instance.MechDef.Chassis.LeftArm.InternalStructure > 1f)) { if (__instance.LeftArmArmor <= 0.0f) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.RightArm.MaxArmor > 0f) && (__instance.MechDef.Chassis.RightArm.InternalStructure > 1f)) { if (__instance.RightArmArmor <= 0.0f) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.LeftLeg.MaxArmor > 0f) && (__instance.MechDef.Chassis.LeftLeg.InternalStructure > 1f)) { if (__instance.LeftLegArmor <= 0.0f) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.RightArm.MaxArmor > 0f) && (__instance.MechDef.Chassis.RightArm.InternalStructure > 1f)) { if (__instance.RightLegArmor <= 0.0f) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.CenterTorso.MaxArmor > 0f) && (__instance.MechDef.Chassis.CenterTorso.InternalStructure > 1f)) { if ((__instance.CenterTorsoFrontArmor <= 0.0f) || (__instance.CenterTorsoRearArmor <= 0f)) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.LeftTorso.MaxArmor > 0f) && (__instance.MechDef.Chassis.LeftTorso.InternalStructure > 1f)) { if ((__instance.LeftTorsoFrontArmor <= 0.0f) || (__instance.RightTorsoRearArmor <= 0f)) { __result = true; return; } }
      if ((__instance.MechDef.Chassis.RightTorso.MaxArmor > 0f) && (__instance.MechDef.Chassis.RightTorso.InternalStructure > 1f)) { if ((__instance.RightTorsoFrontArmor <= 0.0f) || (__instance.RightTorsoRearArmor <= 0f)) { __result = true; return; } }
      __result = false;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("SummaryStructureCurrent")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_SummaryStructureCurrent {
    public static void Postfix(Mech __instance, ref float __result) {
      if (__instance.MechDef == null) { return; }
      if (__instance.MechDef.Chassis == null) { return; }
      __result = 0f;
      MechDef def = __instance.MechDef;
      if ((def.Chassis.Head.MaxArmor > 0f) && (def.Chassis.Head.InternalStructure > 1f)) { __result += __instance.HeadStructure; }
      if ((def.Chassis.LeftArm.MaxArmor > 0f) && (def.Chassis.LeftArm.InternalStructure > 1f)) { __result += __instance.LeftArmStructure; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { __result += __instance.RightArmStructure; }
      if ((def.Chassis.LeftLeg.MaxArmor > 0f) && (def.Chassis.LeftLeg.InternalStructure > 1f)) { __result += __instance.LeftLegStructure; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { __result += __instance.RightLegStructure; }
      if ((def.Chassis.CenterTorso.MaxArmor > 0f) && (def.Chassis.CenterTorso.InternalStructure > 1f)) { __result += (__instance.CenterTorsoStructure); }
      if ((def.Chassis.LeftTorso.MaxArmor > 0f) && (def.Chassis.LeftTorso.InternalStructure > 1f)) { __result += (__instance.LeftTorsoStructure); }
      if ((def.Chassis.RightTorso.MaxArmor > 0f) && (def.Chassis.RightTorso.InternalStructure > 1f)) { __result += (__instance.RightTorsoStructure); }
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("MechDefCurrentStructure")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_MechDefCurrentStructure {
    public static void Postfix(MechDef __instance, ref float __result) {
      if (__instance.Chassis == null) { return; }
      __result = __instance.GetMechDefCurrentStructure();
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("MechDefMaxStructure")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_MechDefMaxStructure {
    public static void Postfix(MechDef __instance, ref float __result) {
      if (__instance.Chassis == null) { return; }
      __result = __instance.Chassis.GetMechDefMaxStructure();
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("ResetArmorStructureBars")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class HUDMechArmorReadout_ResetArmorStructureBars {
    public static float GetMechDefCurrentArmor(this MechDef def) {
      float result = 0f;
      if ((def.Chassis.Head.MaxArmor > 0f) && (def.Chassis.Head.InternalStructure > 1f)) { result += def.Head.CurrentArmor; }
      if ((def.Chassis.LeftArm.MaxArmor > 0f) && (def.Chassis.LeftArm.InternalStructure > 1f)) { result += def.LeftArm.CurrentArmor; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.CurrentArmor; }
      if ((def.Chassis.LeftLeg.MaxArmor > 0f) && (def.Chassis.LeftLeg.InternalStructure > 1f)) { result += def.LeftLeg.CurrentArmor; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.CurrentArmor; }
      if ((def.Chassis.CenterTorso.MaxArmor > 0f) && (def.Chassis.CenterTorso.InternalStructure > 1f)) { result += (def.CenterTorso.CurrentArmor + def.CenterTorso.CurrentRearArmor);  }
      if ((def.Chassis.LeftTorso.MaxArmor > 0f) && (def.Chassis.LeftTorso.InternalStructure > 1f)) { result += (def.LeftTorso.CurrentArmor + def.LeftTorso.CurrentRearArmor); }
      if ((def.Chassis.RightTorso.MaxArmor > 0f) && (def.Chassis.RightTorso.InternalStructure > 1f)) { result += (def.RightTorso.CurrentArmor + def.RightTorso.CurrentRearArmor); }
      return result;
    }
    public static float GetMechDefMaxArmor(this MechDef def) {
      float result = 0f;
      if ((def.Chassis.Head.MaxArmor > 0f) && (def.Chassis.Head.InternalStructure > 1f)) { result += def.Head.AssignedArmor; }
      if ((def.Chassis.LeftArm.MaxArmor > 0f) && (def.Chassis.LeftArm.InternalStructure > 1f)) { result += def.LeftArm.AssignedArmor; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.AssignedArmor; }
      if ((def.Chassis.LeftLeg.MaxArmor > 0f) && (def.Chassis.LeftLeg.InternalStructure > 1f)) { result += def.LeftLeg.AssignedArmor; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.AssignedArmor; }
      if ((def.Chassis.CenterTorso.MaxArmor > 0f) && (def.Chassis.CenterTorso.InternalStructure > 1f)) { result += (def.CenterTorso.AssignedArmor + def.CenterTorso.AssignedRearArmor); }
      if ((def.Chassis.LeftTorso.MaxArmor > 0f) && (def.Chassis.LeftTorso.InternalStructure > 1f)) { result += (def.LeftTorso.AssignedArmor + def.LeftTorso.AssignedRearArmor); }
      if ((def.Chassis.RightTorso.MaxArmor > 0f) && (def.Chassis.RightTorso.InternalStructure > 1f)) { result += (def.RightTorso.AssignedArmor + def.RightTorso.AssignedRearArmor); }
      return result;
    }
    public static float GetMechDefCurrentStructure(this MechDef def) {
      float result = 0f;
      if ((def.Chassis.Head.MaxArmor > 0f) && (def.Chassis.Head.InternalStructure > 1f)) { result += def.Head.CurrentInternalStructure; }
      if ((def.Chassis.LeftArm.MaxArmor > 0f) && (def.Chassis.LeftArm.InternalStructure > 1f)) { result += def.LeftArm.CurrentInternalStructure; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.CurrentInternalStructure; }
      if ((def.Chassis.LeftLeg.MaxArmor > 0f) && (def.Chassis.LeftLeg.InternalStructure > 1f)) { result += def.LeftLeg.CurrentInternalStructure; }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { result += def.RightArm.CurrentInternalStructure; }
      if ((def.Chassis.CenterTorso.MaxArmor > 0f) && (def.Chassis.CenterTorso.InternalStructure > 1f)) { result += (def.CenterTorso.CurrentInternalStructure); }
      if ((def.Chassis.LeftTorso.MaxArmor > 0f) && (def.Chassis.LeftTorso.InternalStructure > 1f)) { result += (def.LeftTorso.CurrentInternalStructure); }
      if ((def.Chassis.RightTorso.MaxArmor > 0f) && (def.Chassis.RightTorso.InternalStructure > 1f)) { result += (def.RightTorso.CurrentInternalStructure); }
      return result;
    }
    public static float GetMechDefMaxStructure(this ChassisDef def) {
      float result = 0f;
      if ((def.Head.MaxArmor > 0f) && (def.Head.InternalStructure > 1f)) { result += def.Head.InternalStructure; }
      if ((def.LeftArm.MaxArmor > 0f) && (def.LeftArm.InternalStructure > 1f)) { result += def.LeftArm.InternalStructure; }
      if ((def.RightArm.MaxArmor > 0f) && (def.RightArm.InternalStructure > 1f)) { result += def.RightArm.InternalStructure; }
      if ((def.LeftLeg.MaxArmor > 0f) && (def.LeftLeg.InternalStructure > 1f)) { result += def.LeftLeg.InternalStructure; }
      if ((def.RightArm.MaxArmor > 0f) && (def.RightArm.InternalStructure > 1f)) { result += def.RightArm.InternalStructure; }
      if ((def.CenterTorso.MaxArmor > 0f) && (def.CenterTorso.InternalStructure > 1f)) { result += (def.CenterTorso.InternalStructure); }
      if ((def.LeftTorso.MaxArmor > 0f) && (def.LeftTorso.InternalStructure > 1f)) { result += (def.LeftTorso.InternalStructure); }
      if ((def.RightTorso.MaxArmor > 0f) && (def.RightTorso.InternalStructure > 1f)) { result += (def.RightTorso.InternalStructure); }
      return result;
    }
    public static bool GetMechDefExposed(this MechDef def) {
      if ((def.Chassis.Head.MaxArmor > 0f) && (def.Chassis.Head.InternalStructure > 1f)) { if (def.Head.CurrentArmor <= 0.0f) { return true; } }
      if ((def.Chassis.LeftArm.MaxArmor > 0f) && (def.Chassis.LeftArm.InternalStructure > 1f)) { if (def.LeftArm.CurrentArmor <= 0.0f) { return true; } }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { if (def.RightArm.CurrentArmor <= 0.0f) { return true; } }
      if ((def.Chassis.LeftLeg.MaxArmor > 0f) && (def.Chassis.LeftLeg.InternalStructure > 1f)) { if (def.LeftLeg.CurrentArmor <= 0.0f) { return true; } }
      if ((def.Chassis.RightArm.MaxArmor > 0f) && (def.Chassis.RightArm.InternalStructure > 1f)) { if (def.RightArm.CurrentArmor <= 0.0f) { return true; } }
      if ((def.Chassis.CenterTorso.MaxArmor > 0f) && (def.Chassis.CenterTorso.InternalStructure > 1f)) { if ((def.CenterTorso.CurrentArmor <= 0.0f)||(def.CenterTorso.CurrentRearArmor <= 0f)) { return true; } }
      if ((def.Chassis.LeftTorso.MaxArmor > 0f) && (def.Chassis.LeftTorso.InternalStructure > 1f)) { if ((def.LeftTorso.CurrentArmor <= 0.0f) || (def.LeftTorso.CurrentRearArmor <= 0f)) { return true; } }
      if ((def.Chassis.RightTorso.MaxArmor > 0f) && (def.Chassis.RightTorso.InternalStructure > 1f)) { if ((def.RightTorso.CurrentArmor <= 0.0f) || (def.RightTorso.CurrentRearArmor <= 0f)) { return true; } }
      return false;
    }
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if (__instance.DisplayedMech != null) { return true; }
      float current1 = 0.0f;
      float max1 = 1f;
      float current2 = 0.0f;
      float max2 = 1f;
      bool exposed = true;
      if (__instance.DisplayedMechDef != null) {
        Log.TWL(0, "HUDMechArmorReadout.ResetArmorStructureBars " + __instance.DisplayedMechDef.ChassisID);
        current1 = __instance.DisplayedMechDef.GetMechDefCurrentArmor();
        max1 = __instance.DisplayedMechDef.GetMechDefMaxArmor();
        current2 = __instance.DisplayedMechDef.GetMechDefCurrentStructure();
        max2 = __instance.DisplayedMechDef.Chassis.GetMechDefMaxStructure();
        exposed = __instance.DisplayedMechDef.GetMechDefExposed();
      } else if (__instance.DisplayedChassisDef != null) {
        max2 = __instance.DisplayedChassisDef.GetMechDefMaxStructure();
        current2 = max2;
        exposed = true;
      } else {
        return false;
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
      float current1 = 0.0f;
      float current2 = 0.0f;
      bool exposed = true;
      if (__instance.DisplayedMechDef != null) {
        Log.TWL(0, "HUDMechArmorReadout.UpdateArmorStructureBars " + __instance.DisplayedMechDef.ChassisID);
        current1 = __instance.DisplayedMechDef.GetMechDefCurrentArmor();
        current2 = __instance.DisplayedMechDef.GetMechDefCurrentStructure();
        exposed = __instance.DisplayedMechDef.GetMechDefExposed();
      } else if (__instance.DisplayedChassisDef != null) {
        exposed = true;
      } else {
        return false;
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
      if (___activeMech.IsVehicle() == false) {
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
      if (___selectedMech.IsVehicle() == false) { return true; }
      if (Core.Settings.AllowVehiclesEdit == true) { return true; }
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
      if (___selectedMech.IsVehicle() == false) { return true; }
      if (Core.Settings.AllowVehiclesEdit == true) { return true; }
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
      if (___selectedMech.IsVehicle() == false) { return true; }
      if (___selectedMechElement.inMaintenance) {
        GenericPopupBuilder.Create("Cannot Repair Vehicle", Strings.T("This 'Vehicle is already under maintenance. You must first cancel the existing task in order to begin repairs on this 'Vehicle.")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
        return false;
      }
      return true;
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
      if (___selectedMech.IsVehicle() == false) { return true; }
      if (___selectedMechElement.inMaintenance) {
        GenericPopupBuilder.Create("Cannot Scrap Vehicle", Strings.T("This 'Vehicle is already under maintenance. You must first cancel the existing task in order to scrap this 'Vehicle.")).AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      } else {
        GenericPopupBuilder.Create(Strings.T("Scrap 'Vehicle - {0}", (object)___selectedMech.Description.Name), Strings.T("Are you sure you want to scrap this 'Vehicle?\n\nThis 'Vehicle's components will be stored and its chassis removed permanently from your inventory.\n\nSCRAP VALUE: <color=#F79B26FF>{0}</color>", (object)SimGameState.GetCBillString(Mathf.RoundToInt((float)___selectedMech.Chassis.Description.Cost * __instance.sim.Constants.Finances.MechScrapModifier)))).AddButton("Cancel", (Action)null, true, (PlayerAction)null).AddButton("Scrap", new Action(new d_MechBayMechInfoWidget(__instance).ConfirmScrapClicked), true, (PlayerAction)null).CancelOnEscape().AddFader(new UIColorRef?(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill), 0.0f, true).Render();
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDHeatDisplay))]
  [HarmonyPatch("GetProjectedHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(CombatHUD) })]
  public static class CombatHUDHeatDisplay_GetProjectedHeat {
    public static void Postfix(CombatHUDHeatDisplay __instance, Mech mech, CombatHUD hud, ref int __result) {
      try {
        if (mech.isHasHeat() == false) { __result = 0; }
      } catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDHeatDisplay))]
  [HarmonyPatch("GetPredictedLevel")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDHeatDisplay_GetPredictedLevel {
    public static bool Prefix(CombatHUDHeatDisplay __instance, ref float __result) {
      try {
        if (__instance.DisplayedActor == null) { return true; }
        if (__instance.DisplayedActor.isHasHeat() == false) { __result = 0f; return false; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDStabilityDisplay))]
  [HarmonyPatch("GetPredictedLevel")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class CombatHUDStabilityDisplay_GetPredictedLevel {
    public static bool Prefix(CombatHUDHeatDisplay __instance, ref float __result) {
      try {
        if (__instance.DisplayedActor == null) { return true; }
        if (__instance.DisplayedActor.isHasStability() == false) { __result = 0f; return false; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("_heat")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech__heat_get {
    public static void Postfix(Mech __instance, ref int __result) {
      try {
        if (__instance.isHasHeat() == false) { __result = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("_heat")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech__heat_set {
    public static void Prefix(Mech __instance, ref int value) {
      try {
        if (__instance.isHasHeat() == false) { value = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("TempHeat")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_TempHeat_get {
    public static void Postfix(Mech __instance, ref int __result,ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { __result = 0; ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddEngineDamageHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddEngineDamageHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddExternalHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class Mech_AddExternalHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat, string reason, int amt) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddJumpHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class Mech_AddJumpHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddWalkHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddWalkHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddTempHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddSprintHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; Traverse.Create(__instance).Property<int>("_heat").Value = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddWeaponHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(int) })]
  public static class Mech_AddWeaponHeat {
    public static void Postfix(Mech __instance, ref int ____tempHeat) {
      try {
        if (__instance.isHasHeat() == false) { ____tempHeat = 0; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
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
      //dest.MechTags.Add();
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
      //if (src.IsDead) {
      //  result.StatCollection.Set<float>(result.GetStringForStructureLocation(ChassisLocations.CenterTorso), 0f);
      //  result.StatCollection.Set<LocationDamageLevel>(result.GetStringForStructureDamageLevel(ChassisLocations.CenterTorso), LocationDamageLevel.Destroyed);
      //  result.pilot.StatCollection.Set("LethalInjury",true);
      //}
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
      for(int index=0;index< allMechs.Count;) {
        if (allMechs[index].IsDeployDirector()) { allMechs.RemoveAt(index); } else { ++index; }
      }
      foreach (AbstractActor actor in allActors) {
        try {
          if (actor.IsDeployDirector()) { continue; }
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
      if (mechDef.IsVehicle()) {
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
      if (mechDef.IsVehicle()) {
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
      if (mechDef.IsVehicle()) {
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
      if (mechDef.IsVehicle()) {
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
      if (___selectedMech.IsVehicle() == false) {
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
      bool isVehicle = false;
      bool isTrooper = false;
      int troopersCount = 0;
      if (___activeMech.IsVehicle()) { isVehicle = true; }
      UnitCustomInfo info = ___activeMech.GetCustomInfo();
      if(info != null) {
        troopersCount = info.SquadInfo.Troopers;
        if (troopersCount > 1) { isTrooper = true; }
      }
      if (isTrooper) {
        ___headLabel.SetText("U0");
        ___centerTorsoLabel.SetText("U1"); ___centerTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftTorsoLabel.SetText("U2"); ___leftTorsoLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 3);
        ___rightTorsoLabel.SetText("U3"); ___rightTorsoLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 4);
        ___leftArmLabel.SetText("U4"); ___leftArmLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 5);
        ___rightArmLabel.SetText("U5"); ___rightArmLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 6);
        ___leftLegLabel.SetText("U6"); ___leftLegLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 7);
        ___rightLegLabel.SetText("U7"); ___rightLegLabel.gameObject.transform.parent.gameObject.SetActive(troopersCount >= 8);
      }else
      if (isVehicle) {
        ___headLabel.SetText("T");
        ___centerTorsoLabel.SetText("CT"); ___centerTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___leftTorsoLabel.SetText("LT"); ___leftTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___rightTorsoLabel.SetText("RT"); ___rightTorsoLabel.gameObject.transform.parent.gameObject.SetActive(false);
        ___leftArmLabel.SetText("F");
        ___rightArmLabel.SetText("R");
        ___leftLegLabel.SetText("L");
        ___rightLegLabel.SetText("R");
      } else {
        ___headLabel.SetText("H");
        ___centerTorsoLabel.SetText("CT"); ___centerTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftTorsoLabel.SetText("LT"); ___leftTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___rightTorsoLabel.SetText("RT"); ___rightTorsoLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftArmLabel.SetText("LA"); ___leftArmLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___rightArmLabel.SetText("RA"); ___rightArmLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___leftLegLabel.SetText("LL"); ___leftLegLabel.gameObject.transform.parent.gameObject.SetActive(true);
        ___rightLegLabel.SetText("RL"); ___rightLegLabel.gameObject.transform.parent.gameObject.SetActive(true);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmor_info {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      return true;
    }
    public static void Postfix(HUDMechArmorReadout __instance) {
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
      CustomDeploy.Core.SpawnMech_internal = SpawnMech;
      MethodInfo targetMethod = typeof(UnitSpawnPointGameLogic).GetMethod("SpawnMech", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(UnitSpawnPointGameLogic_Spawn), nameof(SpawnMech));
      return Transpilers.MethodReplacer(instructions, targetMethod, replacementMethod);
    }
    public static AbstractActor SpawnMech(UnitSpawnPointGameLogic instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef) {
      Log.TWL(0, "UnitSpawnPointGameLogic.Spawn SpawnMech " + mDef.Description.Id + " chassis:" + (mDef.Chassis == null?"null":mDef.Chassis.Description.Id));
      if (mDef.Chassis == null) { return null; }
      DataManager dataManager = mDef.DataManager;
      if (dataManager == null) { dataManager = mDef.Chassis.DataManager; }
      if (dataManager == null) { dataManager = instance.Combat.DataManager; }
      if (dataManager == null) { dataManager = mDef.DataManager(); }
      Log.WL(1, "dataManager:" + (dataManager == null ? "null" : "not null"));
      AbstractActor result = null;
      try {
        bool spawnAsVehicle = false;
        if(mDef.Chassis.ChassisInfo().SpawnAs == SpawnType.Undefined) {
          if (mDef.ChassisID.IsInFakeChassis()) {
            spawnAsVehicle = true;
          }
        } else {
          spawnAsVehicle = mDef.Chassis.ChassisInfo().SpawnAs == SpawnType.AsVehicle;
        }
        if (spawnAsVehicle == false) {
          Log.WL(1, "spawning mech");
          result = instance.SpawnMech(mDef, pilot, team, lance, customHeraldryDef);
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
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("AlignVehicleToGround")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Transform), typeof(float) })]
  public static class ActorMovementSequence_AlignVehicleToGround {
    public static bool Prefix(Transform vehicleTransform, float deltaTime) {

      PilotableActorRepresentation rep = vehicleTransform.gameObject.GetComponent<PilotableActorRepresentation>();
      if (rep != null) {
        if (rep.parentActor != null) {
          if (rep.parentActor.UnaffectedPathing()) { return false; }
        }
      }
      //Log.TWL(0, "ActorMovementSequence.AlignVehicleToGround "+ vehicleTransform.name);
      if (Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value == 0) {
        Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value = LayerMask.GetMask("Terrain", "Obstruction", "Combatant");
      }
      RaycastHit[] raycastHitArray = Physics.RaycastAll(new Ray(vehicleTransform.position + Vector3.up * 20f, Vector3.down), 40f, Traverse.Create(typeof(ActorMovementSequence)).Field<int>("ikLayerMask").Value);
      RaycastHit? nullable = new RaycastHit?();
      RaycastHit raycastHit;
      for (int index = 0; index < raycastHitArray.Length; ++index) {
        if (raycastHitArray[index].collider.transform.name.Contains("SBODY")) { continue; }
        //Log.WL(1, "ray hit:" + raycastHitArray[index].point + " hit collider:" + raycastHitArray[index].collider.transform.name);
        if (!((UnityEngine.Object)raycastHitArray[index].transform == (UnityEngine.Object)vehicleTransform)) {
          if (!nullable.HasValue) {
            nullable = new RaycastHit?(raycastHitArray[index]);
          } else {
            raycastHit = nullable.Value;
            if ((double)raycastHit.point.y < (double)raycastHitArray[index].point.y)
              nullable = new RaycastHit?(raycastHitArray[index]);
          }
        }
      }
      if (!nullable.HasValue) { return false; }
      raycastHit = nullable.Value;
      Vector3 normal = raycastHit.normal;
      //Log.WL(1, "ray hit found. Point:" + raycastHit.point + " hit collider:" + raycastHit.collider.transform.name);
      Quaternion to = Quaternion.FromToRotation(vehicleTransform.up, normal) * Quaternion.Euler(0.0f, vehicleTransform.rotation.eulerAngles.y, 0.0f);
      vehicleTransform.rotation = Quaternion.RotateTowards(vehicleTransform.rotation, to, 180f * deltaTime);
      return false;
    }
  }
  //[HarmonyPatch(typeof(UnitSpawnPointGameLogic))]
  //[HarmonyPatch("SpawnMech")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(Team), typeof(Lance), typeof(HeraldryDef) })]
  //public static class UnitSpawnPointGameLogic_SpawnMechAlign {
  //  public static void Postfix(UnitSpawnPointGameLogic __instance, MechDef mDef, PilotDef pilot, Team team, Lance lance, HeraldryDef customHeraldryDef, ref Mech __result) {
  //    UnitCustomInfo info = mDef.GetCustomInfo();
  //    Log.TWL(0, "UnitSpawnPointGameLogic.SpawnMech "+mDef.Description.Id);
  //    if (info != null) {
  //      if (info.FakeVehicle) {
  //        Log.WL(1, "AlignVehicleToGround "+ __result.GameRep.transform.name+" rotation:"+__result.GameRep.transform.rotation);
  //        ActorMovementSequence.AlignVehicleToGround(__result.GameRep.transform, 100f);
  //        Log.WL(1, "rotation:" + __result.GameRep.transform.rotation);
  //      }
  //    };
  //  }
  //}
  //[HarmonyPatch(typeof(ActorMovementSequence))]
  //[HarmonyPatch("UpdateRotation")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class ActorMovementSequence_UpdateRotation {
  //  private static MethodInfo mOwningVehicleSet;
  //  private delegate void OwningVehicleSetDelegate(ActorMovementSequence seq, Vehicle v);
  //  private static OwningVehicleSetDelegate OwningVehicleSetInvoker = null;
  //  public static bool Prepare() {
  //    mOwningVehicleSet = null;
  //    try {
  //      mOwningVehicleSet = typeof(ActorMovementSequence).GetProperty("OwningVehicle", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
  //      if (mOwningVehicleSet == null) { return false; }
  //    } catch (Exception e) {
  //      Log.LogWrite(e.ToString() + "\n");
  //      return false;
  //    }
  //    var dm = new DynamicMethod("CUOwningVehicleSet", null, new Type[] { typeof(ActorMovementSequence), typeof(Vehicle) }, typeof(ActorMovementSequence));
  //    var gen = dm.GetILGenerator();
  //    gen.Emit(OpCodes.Ldarg_0);
  //    gen.Emit(OpCodes.Ldarg_1);
  //    gen.Emit(OpCodes.Call, mOwningVehicleSet);
  //    gen.Emit(OpCodes.Ret);
  //    OwningVehicleSetInvoker = (OwningVehicleSetDelegate)dm.CreateDelegate(typeof(OwningVehicleSetDelegate));
  //    return true;
  //  }
  //  public static void OwningVehicle(this ActorMovementSequence seq, Vehicle v) {
  //    OwningVehicleSetInvoker(seq, v);
  //  }
  //  public static bool Prefix(ActorMovementSequence __instance, ref Vehicle __state, Vector3 ___Forward) {
  //    __state = null;
  //    CustomMechRepresentation custRep = __instance.ActorRep as CustomMechRepresentation;
  //    if (custRep != null) {
  //      if (__instance.OrdersAreComplete == false) {
  //        Transform MoverTransform = Traverse.Create(__instance).Property<Transform>("MoverTransform").Value;
  //        float deltaTime = Traverse.Create(__instance).Property<float>("deltaTime").Value;
  //        custRep.UpdateRotation(MoverTransform, ___Forward, deltaTime);
  //        return false;
  //      }
  //    }
  //    if (__instance.OwningVehicle != null) {
  //      if (__instance.OwningVehicle.UnaffectedPathing() == false) { return true; };
  //      __state = __instance.OwningVehicle;
  //      __instance.OwningVehicle(null);
  //    }
  //    return true;
  //  }
  //  public static void Postfix(ActorMovementSequence __instance, ref Vehicle __state,ref Vector3 ___Forward) {
  //    if (__state != null) {
  //      __instance.OwningVehicle(__state);
  //    }
  //    //if (__instance.OwningMech != null) {
  //      //UnitCustomInfo info = __instance.OwningMech.GetCustomInfo();
  //      //if (info != null) {
  //      //  if (info.FakeVehicle) {
  //      //    Transform MoverTransform = Traverse.Create(__instance).Property<Transform>("MoverTransform").Value;
  //      //    float deltaTime = Traverse.Create(__instance).Property<float>("deltaTime").Value;
  //      //    MoverTransform.rotation = Quaternion.RotateTowards(MoverTransform.rotation, Quaternion.LookRotation(___Forward), 180f * deltaTime);
  //      //    ActorMovementSequence.AlignVehicleToGround(MoverTransform, deltaTime);
  //      //  }
  //      //}
  //    //}
  //  }
  //}

  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class MechDef_GatherDependencies_fake {
    public static void Postfix(MechDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      if (__instance.Description.Id.IsInFakeDef() == false) { return; }
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info != null) { if (info.FakeVehicle) { return; } }
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
      if (__instance.Description.Id.IsInFakeChassis() == false) { return; }
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
      LocationDef locDef = __instance.Chassis.GetLocationDef(loc);
      if ((locDef.InternalStructure <= 1.0f) && (locDef.MaxArmor == 0f)) { __result = false; }
      //if (__instance.IsFake(__instance.Description.Id) == false) { return; };
      //if (loc == ChassisLocations.Head) { if (__instance.Chassis.Head.InternalStructure == 0f) { __result = false; }; }
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
      if (mechDef.IsVehicle()) { return; };
      __result = ((double)mechDef.LeftArm.CurrentInternalStructure >= 1.0) && ((double)mechDef.RightArm.CurrentInternalStructure >= 1.0) && ((double)mechDef.LeftLeg.CurrentInternalStructure >= 1.0) && ((double)mechDef.RightLeg.CurrentInternalStructure >= 1.0);
      if (mechDef.Chassis.Head.InternalStructure > 0f) { __result = __result && (mechDef.Head.CurrentInternalStructure >= 1f); };
      Log.TWL(0, "MechValidationRules.ValidateMechStructureSimple " + mechDef.Chassis.Description.Id + " isVehicle:" + mechDef.IsVehicle() + " result:" + __result);
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
        Log.TW(0, "MechValidationRules.ValidateMechCanBeFielded");
        if (mechDef == null) { Log.WL(1, "mechDef is null"); return; };
        Log.W(1, mechDef.Description.Id);
        bool isFake = mechDef.IsVehicle();
        Log.W(1, mechDef.ChassisID + " fake:" + isFake + " result:" + __result);
        //if (isFake == false) { return; };

        bool inMaintaince = MechValidationRules.ValidateSimGameMechNotInMaintenance(sim, mechDef) == false;
        Log.WL(1, "inMaintaince = " + inMaintaince);
        bool badStructure = MechValidationRules.ValidateMechStructureSimple(mechDef);
        Log.WL(1, "badStructure = " + badStructure);
        bool badWeapon = MechValidationRules.ValidateMechPosessesWeaponsSimple(mechDef);
        Log.WL(1, "badWeapon = " + badWeapon);
        __result = (inMaintaince || badStructure || badWeapon) == false;
        Log.WL(1, "CanBeFielded:" + __result);
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
      if (mechDef.IsVehicle() == false) { return; };
      Log.TWL(0, "MechValidationRules.ValidateMechPosessesWeaponsSimple " + mechDef.Chassis.Description.Id + " is vehicle:" + mechDef.IsVehicle() + " result:" + __result);
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
      //Log.TWL(0, "MechDef.FromJSON fake "+(__instance.Description == null?"null": __instance.Description.Id));
      if (__instance.Description != null) {
        //Log.TWL(0, "MechDef.FromJSON fake already preloaded:"+__instance.Description.Id);
        return true;
      }
      try {
        JObject olddef = JObject.Parse(json);
        string id = (string)olddef["Description"]["Id"];
        string chassisId = (string)olddef["ChassisID"];
        bool isFake = chassisId.IsInFakeChassis();
        //Log.WL(1,id+" chassis:"+chassisId+" isFake:"+isFake);
        if (isFake == false) { return true; }
        JObject newdef = new JObject();
        if (olddef["Chassis"] != null) { return true; };
        float ArmorMultiplierVehicle = 1f;
        float StructureMultiplierVehicle = 1f;
        if (CombatValueMultipliers.HasValue == false) {
          if (Core.Settings.DefaultCombatValueMultipliers != null) {
            CombatValueMultipliers = Core.Settings.DefaultCombatValueMultipliers;
            ArmorMultiplierVehicle = CombatValueMultipliers.Value.ArmorMultiplierVehicle;
            StructureMultiplierVehicle = CombatValueMultipliers.Value.StructureMultiplierVehicle;
          }
        } else {
          ArmorMultiplierVehicle = CombatValueMultipliers.Value.ArmorMultiplierVehicle;
          StructureMultiplierVehicle = CombatValueMultipliers.Value.StructureMultiplierVehicle;
        }
        //Log.WL(1, "ArmorMultiplierVehicle:"+ ArmorMultiplierVehicle);
        //Log.WL(1, "StructureMultiplierVehicle:" + StructureMultiplierVehicle);
        newdef["Description"] = olddef["Description"];
        newdef["ChassisID"] = olddef["ChassisID"];
        newdef["simGameMechPartCost"] = 0;
        newdef["MechTags"] = olddef["VehicleTags"];
        TagSet MechTags = null;
        if (olddef["VehicleTags"] != null) {
          MechTags = new TagSet();
          MechTags.FromJSON(olddef["VehicleTags"].ToString());
        } else {
          MechTags = new TagSet();
        }
        MechTags.Add("fake_vehicle");
        newdef["MechTags"] = JObject.Parse(MechTags.ToJSON());
        JArray vInventory = olddef["inventory"] as JArray;
        JArray mInventory = new JArray();
       // Log.WL(1, "inventory");
        foreach (JObject vcRef in vInventory) {
          //Log.WL(2, vcRef["ComponentDefID"] + ":" + vcRef["MountedLocation"]);
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
          //Log.WL(2, vcLoc["Location"] + ":" + vcLoc["AssignedArmor"]);
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
        //Log.WL(1, json);
      } catch (Exception e) {
        Log.LogWrite(json, true);
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  public static class ChassisDef_FromJSON_fake {
    private static ConcurrentDictionary<string, byte> fakeHardpoints = new ConcurrentDictionary<string, byte>();
    public static bool isFakeHardpoint(this HardpointDataDef hardpoint) { return fakeHardpoints.ContainsKey(hardpoint.ID); }
    public static string ConstructMechFakeVehicle(this string json) {
      //Log.TW(0, "ChassisDef.ConstructFake");
      string result = json;
      try {
        JObject olddef = JObject.Parse(json);
        string id = (string)olddef["Description"]["Id"];
        bool isFake = id.IsInFakeChassis();
        //Log.WL(1, id + " isFake:" + isFake);
        if (isFake == false) { return result; }
        float ArmorMultiplierVehicle = 1f;
        float StructureMultiplierVehicle = 1f;
        if (MechDef_FromJSON_fake.CombatValueMultipliers.HasValue == false) {
          if (Core.Settings.DefaultCombatValueMultipliers != null) {
            MechDef_FromJSON_fake.CombatValueMultipliers = Core.Settings.DefaultCombatValueMultipliers;
            ArmorMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.ArmorMultiplierVehicle;
            StructureMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.StructureMultiplierVehicle;
          }
        } else {
          ArmorMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.ArmorMultiplierVehicle;
          StructureMultiplierVehicle = MechDef_FromJSON_fake.CombatValueMultipliers.Value.StructureMultiplierVehicle;
        }
        //Log.WL(1, "ArmorMultiplierVehicle:" + ArmorMultiplierVehicle);
        //Log.WL(1, "StructureMultiplierVehicle:" + StructureMultiplierVehicle);
        JObject newdef = new JObject();
        if (olddef["Custom"] != null) {
          newdef["Custom"] = olddef["Custom"];
        }
        newdef["Description"] = olddef["Description"];
        newdef["MovementCapDefID"] = olddef["MovementCapDefID"];
        newdef["PathingCapDefID"] = olddef["PathingCapDefID"];
        newdef["HardpointDataDefID"] = olddef["HardpointDataDefID"];
        fakeHardpoints.TryAdd((string)olddef["HardpointDataDefID"],1);
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
        newdef["LOSSourcePositions"] = olddef["LOSSourcePositions"];
        newdef["LOSTargetPositions"] = olddef["LOSTargetPositions"];
        //JArray.Parse("[ { \"x\": 0, \"y\": 12, \"z\": 0 }, { \"x\": 3, \"y\": 12, \"z\": 0 }, { \"x\": -3, \"y\": 12,\"z\": 0 }]");
        //newdef["LOSTargetPositions"] = JArray.Parse("[ { \"x\": 0, \"y\": 12, \"z\": 0 }, { \"x\": 3, \"y\": 12, \"z\": 0 }, { \"x\": -3, \"y\": 12,\"z\": 0 }, { \"x\": 2.5, \"y\": 6,\"z\": 0 }, { \"x\": -2.5, \"y\": 6,\"z\": 0 }]");
        newdef["SpotterDistanceMultiplier"] = olddef["SpotterDistanceMultiplier"];
        newdef["VisibilityMultiplier"] = olddef["VisibilityMultiplier"];
        newdef["SensorRangeMultiplier"] = olddef["SensorRangeMultiplier"];
        newdef["Signature"] = olddef["Signature"];
        newdef["Radius"] = olddef["Radius"];
        newdef["PunchesWithLeftArm"] = false;
        newdef["MeleeDamage"] = (olddef["MeleeDamage"]==null)? ((float)olddef["Tonnage"] / 2f) : olddef["MeleeDamage"];
        newdef["MeleeInstability"] = 0;
        newdef["MeleeToHitModifier"] = (olddef["MeleeToHitModifier"] == null) ? (0f) : olddef["MeleeToHitModifier"];
        newdef["DFADamage"] = (olddef["DFADamage"] == null) ? ((float)olddef["Tonnage"]) : olddef["DFADamage"]; ; ;
        newdef["DFAToHitModifier"] = (olddef["DFAToHitModifier"] == null) ? (0f) : olddef["DFAToHitModifier"]; ;
        newdef["DFASelfDamage"] = (olddef["DFASelfDamage"] == null) ? ((float)olddef["Tonnage"]) : olddef["DFASelfDamage"];
        newdef["DFAInstability"] = 0;
        TagSet ChassisTags = null;
        if (olddef["ChassisTags"] != null) {
          ChassisTags = new TagSet();
          ChassisTags.FromJSON(olddef["ChassisTags"].ToString());
        } else {
          ChassisTags = new TagSet();
        }
        ChassisTags.Add("fake_vehicle_chassis");
        newdef["ChassisTags"] = JObject.Parse(ChassisTags.ToJSON());
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
          //Log.WL(2, vcLoc["Location"] + ":" + vcLoc["InternalStructure"]);
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
        result = newdef.ToString(Newtonsoft.Json.Formatting.Indented);
        //Log.WL(1, result);
      } catch (Exception e) {
        Log.TWL(0,json, true);
        Log.TWL(0,e.ToString(), true);
      }
      return result;
    }
  }
  [HarmonyPatch(typeof(ContentPackIndex), "TryFinalizeDataLoad")]
  public static class ContentPackIndex_TryFinalizeDataLoad_Patch
  {
    public static void Postfix(ContentPackIndex __instance)
    {
      try
      {
        Log.TWL(0, "ContentPackIndex.TryFinalizeDataLoad");
        // called every time content packs defs are fully loaded from default manifest
        // and dlc ownership changes are detected (e.g. after paradox login and backer unlock)
        if (__instance.AllContentPacksLoaded())
        {
          FakeDatabase.AllContentPacksLoaded();
        }
      }
      catch (Exception e)
      {
        Log.WL(0, $"Exception: {e}");
      }
    }
  }

  internal static class FakeDatabase {
    private static HashSet<string> fakemechDefs = new HashSet<string>();
    private static HashSet<string> fakeChassisDef = new HashSet<string>();
    private static Dictionary<string, HashSet<string>> chassisMechsRegistry = new Dictionary<string, HashSet<string>>();
    private static BattleTechResourceLocator ResourceLocator => UnityGameInstance.BattleTechGame.DataManager.ResourceLocator;
    //public static void RegisterMech(this MechDef def, string mechId, string chassisId) {
    //  if(chassisMechsRegistry.TryGetValue(chassisId, out HashSet<string> registry) == false) {
    //    registry = new HashSet<string>();
    //    chassisMechsRegistry.Add(chassisId, registry);
    //  }
    //  registry.Add(mechId);
    //}
    //public static void AddAllRegisterdMechsToFake(this ChassisDef chassis, string chassisId) {
    //  if (chassisMechsRegistry.TryGetValue(chassisId, out HashSet<string> registry)) {
    //    foreach (string mechid in registry) { fakemechDefs.Add(mechid); }
    //  }
    //}
    public static bool IsInFakeChassis(this string id) {
      return fakeChassisDef.Contains(id);
    }
    public static bool IsInFakeDef(this string id) {
      return fakemechDefs.Contains(id);
    }
    public static bool IsVehicle(this ChassisDef chassisDef) {
      if (chassisDef == null) { return false; }
      //Log.TWL(0, "ChassisDef.IsVehicle");
      ChassisDef chassis = Thread.CurrentThread.peekFromStack<ChassisDef>("OnReadyMech_chassis");
      if (chassis != null) {
        return chassis.GetHangarShift() > 0;
      }
      UnitCustomInfo info = chassisDef.GetCustomInfo();
      if (info == null) { return false; }
      return info.FakeVehicle;
    }
    public static bool IsVehicle(this MechDef mechDef) {
      if (fakeChassisDef.Contains(mechDef.ChassisID)) { return true; }
      if (mechDef.Chassis != null) {
        UnitCustomInfo info = mechDef.GetCustomInfo();
        if (info != null) { if (info.FakeVehicle) { return true; } }
      } else {
        if (mechDef.MechTags.Contains(Core.Settings.MechIsVehicleTag)) { return true; }
      }
      return false;
    }

    private static readonly VersionManifestAddendum FakeEntries = new VersionManifestAddendum("CustomUnitsVehicles");
    internal static void AllContentPacksLoaded() {
      ResourceLocator.RemoveAddendum(FakeEntries); // remove old entries

      {
        fakemechDefs.Clear();
        foreach (var vehicle in ResourceLocator.AllEntriesOfResource(BattleTechResourceType.VehicleDef)) {
          Log.WL(1, "adding MechDef " + vehicle.Id + " " + vehicle.GetRawPath());
          fakemechDefs.Add(vehicle.Id);
          FakeEntries.Add(new VersionManifestEntry(vehicle.Id
            , vehicle.GetRawPath()
            , BattleTechResourceType.MechDef.ToString()
            , vehicle.AddedOn
            , vehicle.Version.ToString()
            , vehicle.AssetBundleName
            , vehicle.IsAssetBundlePersistent)
          );
        }
      }

      {
        fakeChassisDef.Clear();
        foreach (var vchassi in ResourceLocator.AllEntriesOfResource(BattleTechResourceType.VehicleChassisDef)) {
          Log.WL(1, "adding ChassisDef " + vchassi.Id + " " + vchassi.GetRawPath());
          fakeChassisDef.Add(vchassi.Id);
          FakeEntries.Add(new VersionManifestEntry(vchassi.Id
            , vchassi.GetRawPath()
            , BattleTechResourceType.ChassisDef.ToString()
            , vchassi.AddedOn
            , vchassi.Version.ToString()
            , vchassi.AssetBundleName
            , vchassi.IsAssetBundlePersistent));
        }
      }

      ResourceLocator.ApplyAddendum(FakeEntries);
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
        UnitCustomInfo info = __instance.DisplayedActor.GetCustomInfo();
        bool FakeVehicle = false;
        HUDFakeVehicleArmorReadout fakeVehicleArmor = __instance.FakeVehicleArmorDisplay();
        if (info != null) { if (info.FakeVehicle) { if (fakeVehicleArmor != null) { FakeVehicle = true; } } }
        if (FakeVehicle) {
          __instance.MechArmorDisplay.OnActorTakeDamage((MessageCenterMessage)takeDamageMessage);
        } else {
          __instance.FakeVehicleArmorDisplay().OnActorTakeDamage((MessageCenterMessage)takeDamageMessage);
        }
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
    public static void Postfix(CombatHUDMechTray __instance, AbstractActor ___displayedActor) {
      if (___displayedActor == null) { return; }
      if (___displayedActor.UnitType == UnitType.Vehicle) {
        __instance.VehicleArmorDisplay().UpdateVehicleStructureAndArmor(__instance.shownAttackDirection);
      }else if(___displayedActor.UnitType == UnitType.Mech) {
        UnitCustomInfo info = ___displayedActor.GetCustomInfo();
        if (info != null) {
          if (info.FakeVehicle) {
            __instance.FakeVehicleArmorDisplay().UpdateVehicleStructureAndArmor(__instance.shownAttackDirection);
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmor {
    public static bool Prefix(HUDMechArmorReadout __instance) {
      if ((__instance.DisplayedMech == null)&&(__instance.DisplayedMechDef == null)&&(__instance.DisplayedChassisDef == null)) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(VehicleDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleDef_Refresh {
    public static bool Prefix(VehicleDef __instance, ref VehicleLocationLoadoutDef[] ___Locations) {
      return true;
    }
  }
  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("Refresh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class VehicleChassisDef_Refresh {
    public static bool Prefix(VehicleChassisDef __instance, ref VehicleLocationLoadoutDef[] ___Locations) {
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
        __instance.SetTrayState(CombatHUDMechTray_MWTrayState_Down);
      } else {
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
              bool fakeVehile = false;
              UnitCustomInfo info = mech.GetCustomInfo();
              if (info != null) { if (info.FakeVehicle) { fakeVehile = true; } }
              if (fakeVehile) {
                __instance.MechPaperDoll().SetActive(false);
                __instance.MechArmorDisplay.DisplayedMech = null;
                __instance.VehicleArmorDisplay().DisplayedVehicle = vehicle;
                __instance.VehicleArmorDisplay().gameObject.SetActive(false);
                __instance.FakeVehicleArmorDisplay().gameObject.SetActive(true);
                __instance.FakeVehicleArmorDisplay().DisplayedVehicle = mech;
                __instance.RefreshStabilityInfo();
              } else {
                __instance.MechPaperDoll().SetActive(true);
                __instance.MechArmorDisplay.DisplayedMech = mech;
                __instance.VehicleArmorDisplay().DisplayedVehicle = vehicle;
                __instance.VehicleArmorDisplay().gameObject.SetActive(false);
                __instance.FakeVehicleArmorDisplay().gameObject.SetActive(false);
                __instance.FakeVehicleArmorDisplay().DisplayedVehicle = null;
                __instance.RefreshStabilityInfo();
              }
            } else if (vehicle != null) {
              __instance.MechPaperDoll().SetActive(false);
              __instance.MechArmorDisplay.DisplayedMech = mech;
              __instance.VehicleArmorDisplay().gameObject.SetActive(true);
              __instance.VehicleArmorDisplay().DisplayedVehicle = vehicle;
              __instance.FakeVehicleArmorDisplay().gameObject.SetActive(false);
              __instance.FakeVehicleArmorDisplay().DisplayedVehicle = null;
            }
          }
        }
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta))]
  [HarmonyPatch("FinalizeLances")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SkirmishSettings_Beta_FinalizeLances {
    public static void Postfix(SkirmishSettings_Beta __instance, ref LanceConfiguration __result, LancePreviewPanel ___playerLancePreview, UIManager ___uiManager) {
      Log.TWL(0, "SkirmishSettings_Beta.FinalizeLances units:" + __result.Lances[___playerLancePreview.playerGUID].Count);
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SkirmishSettings_Beta_OnAddedToHierarchy {
    public static void Prefix(SkirmishSettings_Beta __instance) {
      Log.TWL(0, "SkirmishSettings_Beta.OnAddedToHierarchy");
      //Core.InitLancesLoadoutDefault();
    }
  }
  [HarmonyPatch(typeof(CombatHUDHeatMeter))]
  [HarmonyPatch("RefreshHeatInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDHeatMeter_RefreshHeatInfo {
    public static bool Prefix(CombatHUDHeatMeter __instance, float ___underlyingHeatTarget, float ___underlyingHeatDisplayed, float ___underlyingPredictionTarget, float ___underlyingPredictionDisplayed) {
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
    public static bool Prepare() { return false; }
    public static bool Prefix(CombatHUDWeaponSlot __instance, ICombatant target, CombatHUD ___HUD, Weapon ___displayedWeapon, ref int ___modifier, CombatGameState ___Combat) {
      return true;
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
      return false;
    }
    public static void AddToolTipDetail(this CombatHUDWeaponSlot slot, string description, int modifier) {
      i_AddToolTipDetail(slot, description, modifier);
    }

    public static float GetToHitModifierWeaponDamage(AbstractActor attacker, Weapon weapon) {
      if (weapon.DamageLevel == ComponentDamageLevel.Misaligned || weapon.DamageLevel == ComponentDamageLevel.Penalized)
        return attacker.Combat.Constants.ToHit.ToHitSelfWeaponDamaged;
      return 0.0f;
    }
    public static bool Prefix(CombatHUDWeaponSlot __instance, Weapon ___displayedWeapon, ref int ___modifier, CombatGameState ___Combat) {
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
        Thread.CurrentThread.pushActor(mech);
        ___modifier = (int)MechStructureRules.GetToHitModifierLocationDamage(mech, ___displayedWeapon);
        __instance.AddToolTipDetail(Strings.T("{0} DAMAGED", (object)Mech.GetAbbreviatedChassisLocation((ChassisLocations)___displayedWeapon.Location)), ___modifier);
        Thread.CurrentThread.clearActor();
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

}