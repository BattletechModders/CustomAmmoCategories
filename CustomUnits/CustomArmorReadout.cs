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
using CustAmmoCategories;
using Harmony;
using IRBTModUtils;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  [HarmonyPatch(typeof(CombatHUDActorDetailsDisplay))]
  [HarmonyPatch("RefreshInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActorDetailsDisplay_RefreshInfo {
    public static void Postfix(CombatHUDActorDetailsDisplay __instance) {
      try {
        UnitCustomInfo info = __instance.DisplayedActor.GetCustomInfo();
        Mech mech = __instance.DisplayedActor as Mech;
        TrooperSquad squad = __instance.DisplayedActor as TrooperSquad;
        if (squad != null) {
          if (info == null) { return; }
          __instance.ActorWeightText.SetText("{0}: {1}", (object)"SQUAD", info.SquadInfo.weightClass);
        } else if(mech != null) {
          if (mech.FakeVehicle()) {
            __instance.ActorWeightText.SetText("{0}: {1}", (object)"VEHICLE", mech.weightClass);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class CombatHUDCustomTrayArmorHover : EventTrigger {
    public CombatHUDMechTrayArmorHover hover { get; private set; }
    public CombatHUDTooltipHoverElement tooltip { get; private set; }
    public void Init(CombatHUDMechTrayArmorHover hover, CombatHUDTooltipHoverElement tooltip) {
      this.hover = hover;
      this.tooltip = tooltip;
    }
    public override void OnPointerEnter(PointerEventData eventData) {
      Log.TWL(0, "CombatHUDSquadTrayArmorHover.OnPointerEnter " + this.gameObject.name);
      if (hover != null) { hover.OnPointerEnter(eventData); }
      if (tooltip != null) { tooltip.OnPointerEnter(eventData); }
    }
    public override void OnPointerExit(PointerEventData eventData) {
      Log.TWL(0, "CombatHUDSquadTrayArmorHover.OnPointerEnter " + this.gameObject.name);
      if (hover != null) { hover.OnPointerExit(eventData); }
      if (tooltip != null) { tooltip.OnPointerExit(eventData); }
    }
    public override void OnPointerClick(PointerEventData eventData) {
      Log.TWL(0, "CombatHUDSquadTrayArmorHover.OnPointerEnter " + this.gameObject.name);
      if (hover != null) { hover.OnPointerClick(eventData); }
      if (tooltip != null) { tooltip.OnPointerClick(eventData); }
    }
  }
  public class CombatHUDTargetingComputerCustom: MonoBehaviour {
    public CombatHUD HUD { get; set; }
    public CombatHUDTargetingComputer targetingComputer { get; set; }
    public HUDFakeVehicleArmorReadout fakeVehicleReadout { get; set; }
    public void Init(CombatHUD HUD, CombatHUDTargetingComputer targetingComputer) {
      this.targetingComputer = targetingComputer;
      this.HUD = HUD;
      fakeVehicleReadout = targetingComputer.VehicleArmorDisplay.transform.parent.gameObject.GetComponentInChildren<HUDFakeVehicleArmorReadout>(true);
    }
  }
  public class CombatHUDCalledShotPopUpCustom : MonoBehaviour {
    public CombatHUD HUD { get; set; }
    public CombatHUDCalledShotPopUp calledShotPopUp { get; set; }
    public HUDFakeVehicleArmorReadout fakeVehicleReadout { get; set; }

    private Dictionary<VehicleChassisLocations, int> currentVHitTable;
    public LocalizableText[] vReticleTexts;
    private UIManager uiManager;
    public SVGImage[] vReticleImages;
    public GameObject[] ShownVTargetReticles;
    private AttackDirection shownAttackDirection;

    public void Init(CombatHUD HUD, CombatHUDCalledShotPopUp calledShotPopUp) {
      Log.TWL(0, "CombatHUDCalledShotPopUpCustom.Init");
      this.calledShotPopUp = calledShotPopUp;
      this.HUD = HUD;
      fakeVehicleReadout = calledShotPopUp.VehicleArmorDisplay.transform.parent.gameObject.GetComponentInChildren<HUDFakeVehicleArmorReadout>(true);
      this.ShownVTargetReticles = new GameObject[calledShotPopUp.ShownVTargetReticles.Length];
      Log.WL(1, "ShownVTargetReticles "+ calledShotPopUp.ShownVTargetReticles.Length);
      for (int t=0; t < calledShotPopUp.ShownVTargetReticles.Length; ++t) {
        GameObject go = calledShotPopUp.ShownVTargetReticles[t];
        if (go == null) { this.ShownVTargetReticles[t] = null; continue; }
        string path = go.transform.GetRelativePath(calledShotPopUp.VehicleArmorDisplay.gameObject.transform);
        Transform tr = this.fakeVehicleReadout.transform.FindByPath(path);
        Log.WL(2, "path:" + path+" found:"+(tr != null));
        this.ShownVTargetReticles[t] = tr == null ? null : tr.gameObject;
      }
      this.vReticleTexts = new LocalizableText[this.ShownVTargetReticles.Length];
      this.vReticleImages = new SVGImage[this.ShownVTargetReticles.Length];
      for (int index = 0; index < this.ShownVTargetReticles.Length; ++index) {
        this.vReticleTexts[index] = this.ShownVTargetReticles[index].GetComponentInChildren<LocalizableText>(true);
        this.vReticleImages[index] = this.ShownVTargetReticles[index].GetComponentInChildren<SVGImage>(true);
        this.vReticleTexts[index].gameObject.SetActive(true);
        this.vReticleImages[index].gameObject.SetActive(true);
      }
      this.uiManager = Traverse.Create(calledShotPopUp).Field<UIManager>("uiManager").Value;
    }
    public string GetHitPercent(VehicleChassisLocations location, VehicleChassisLocations targetedLocation) {
      return calledShotPopUp.GetHitPercent(location,targetedLocation);
    }

    public AttackDirection ShownAttackDirection {
      get => this.shownAttackDirection;
      set {
        this.shownAttackDirection = value;
        this.currentVHitTable = this.HUD.Combat.HitLocation.GetVehicleHitTable(this.shownAttackDirection, false);
        if (this.fakeVehicleReadout.HoveredArmor != ChassisLocations.None) {
          this.fakeVehicleReadout.ClearHoveredArmor(this.fakeVehicleReadout.HoveredArmor);
        }
      }
    }

    public void UpdateFakeVehicleDisplay() {
      VehicleChassisLocations hoveredArmor = fakeVehicleReadout.HoveredArmor.toFakeVehicleChassis();
      switch (hoveredArmor) {
        case VehicleChassisLocations.None:
        case VehicleChassisLocations.Invalid:
        if (this.calledShotPopUp.locationNameText.text != "-choose target-") {
          this.calledShotPopUp.locationNameText.SetText("-choose target-", (object[])Array.Empty<object>());
          break;
        }
        break;
        default:
        if (this.currentVHitTable.ContainsKey(hoveredArmor) && this.currentVHitTable[hoveredArmor] != 0) {
          string longChassisLocation = Vehicle.GetLongChassisLocation(hoveredArmor);
          if (this.calledShotPopUp.locationNameText.text != longChassisLocation) {
            this.calledShotPopUp.locationNameText.SetText(longChassisLocation, (object[])Array.Empty<object>());
            break;
          }
          break;
        }
        goto case VehicleChassisLocations.None;
      }
      //Log.TWL(0, "CombatHUDCalledShotPopUpCustom.UpdateFakeVehicleDisplay");
      for (int index = 0; index < 5; ++index) {
        VehicleChassisLocations locationFromIndex = HUDVehicleArmorReadout.GetVCLocationFromIndex(index);
        if (this.currentVHitTable.ContainsKey(locationFromIndex) && this.currentVHitTable[locationFromIndex] != 0) {
          if (this.ShownVTargetReticles[index].transform.parent.gameObject.activeSelf == false) {
            this.ShownVTargetReticles[index].transform.parent.gameObject.SetActive(true);
          }
          if (this.vReticleTexts[index].gameObject.activeSelf == false)
            this.vReticleTexts[index].gameObject.SetActive(true);
          this.vReticleTexts[index].SetText(string.Format("{0}", (object)this.GetHitPercent(locationFromIndex, hoveredArmor)), (object[])Array.Empty<object>());
          if (locationFromIndex == hoveredArmor) {
            this.vReticleTexts[index].color = this.uiManager.UIColorRefs.gold;
            this.vReticleTexts[index].fontSize = 18f;
            if (this.vReticleImages[index].gameObject.activeSelf == false)
              this.vReticleImages[index].gameObject.SetActive(true);
          } else {
            this.vReticleTexts[index].color = this.uiManager.UIColorRefs.white;
            this.vReticleTexts[index].fontSize = 16f;
            if (this.vReticleImages[index].gameObject.activeSelf)
              this.vReticleImages[index].gameObject.SetActive(false);
          }
        } else if (this.ShownVTargetReticles[index].transform.parent.gameObject.activeSelf) {
          this.ShownVTargetReticles[index].transform.parent.gameObject.SetActive(false);
        }
        //Log.WL(1, "index:"+index + " location:" + locationFromIndex + " isActive:"+ this.ShownVTargetReticles[index].transform.parent.gameObject.activeSelf+" text:"+ this.vReticleTexts[index].gameObject.activeSelf);
      }
      this.UpdateFakeVehicleStructureAndArmor();
    }
    public void UpdateFakeVehicleStructureAndArmor() {
      if (this.calledShotPopUp.DisplayedActor.UnitType != UnitType.Mech) { return; }
      if (this.calledShotPopUp.DisplayedActor.FakeVehicle()) {
        this.fakeVehicleReadout.UpdateVehicleStructureAndArmor(shownAttackDirection);
      }
    }
    public void ShowFakeVehicleDisplay() {
      Log.TWL(0, "CombatHUDCalledShotPopUpCustom.ShowFakeVehicleDisplay");
      if (this.calledShotPopUp.MechArmorDisplay.gameObject.activeSelf)
        this.calledShotPopUp.MechArmorDisplay.gameObject.SetActive(false);
      if (this.calledShotPopUp.VehicleArmorDisplay.gameObject.activeSelf)
        this.calledShotPopUp.VehicleArmorDisplay.gameObject.SetActive(false);
      if (this.fakeVehicleReadout.gameObject.activeSelf == false)
        this.fakeVehicleReadout.gameObject.SetActive(true);
      Mech mech = this.calledShotPopUp.DisplayedActor as Mech;
      if (mech != null) {
        LocationDef locDef = mech.MechDef.GetChassisLocationDef(ChassisLocations.Head);
        bool noTurret = (locDef.MaxArmor == 0f) && (locDef.InternalStructure <= 1f);
        if (noTurret) {
          this.ShownVTargetReticles[0].SetActive(true);
        } else {
          this.ShownVTargetReticles[0].SetActive(false);
        }
      }
      this.UpdateFakeVehicleStructureAndArmor();
    }
  }
  public class TargetHUDArmorReadout : MonoBehaviour {
    public static readonly string ARMOR_PREFIX = "SquadTray_Armor";
    public static readonly string STRUCTURE_PREFIX = "Squad_TrayInternal";
    public static readonly float SQUAD_ICON_SIZE = 25f;
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() {
      { 0, 0 }, { 1, 4 }, { 2, 2 }, { 3, 1 }, { 4, 3 }, { 5, 5 }, { 6, 6 }, { 7, 7 } };
    public HashSet<GameObject> toHide { get; set; }
    public HashSet<GameObject> toShow { get; set; }
    public Dictionary<int, SVGImage> SquadArmor { get; private set; }
    public Dictionary<int, SVGImage> SquadArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> SquadStructure { get; private set; }
    public Dictionary<int, SVGImage> MechArmor { get; private set; }
    public Dictionary<int, SVGImage> MechArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> MechStructure { get; private set; }
    public TargetHUDArmorReadout() {
      toHide = new HashSet<GameObject>();
      toShow = new HashSet<GameObject>();
      SquadArmor = new Dictionary<int, SVGImage>();
      SquadArmorOutline = new Dictionary<int, SVGImage>();
      SquadStructure = new Dictionary<int, SVGImage>();
      MechArmor = new Dictionary<int, SVGImage>();
      MechArmorOutline = new Dictionary<int, SVGImage>();
      MechStructure = new Dictionary<int, SVGImage>();
    }
    public void Instantine(HUDMechArmorReadout instance) {
      Transform Mech_TrayInternaLT = this.gameObject.transform.FindRecursive("Mech_TrayInternaLT");
      if (Mech_TrayInternaLT != null) { Mech_TrayInternaLT.gameObject.name = "Mech_TrayInternalLT"; };
      Transform Mech_TrayInternaRT = this.gameObject.transform.FindRecursive("Mech_TrayInternaRT");
      if (Mech_TrayInternaRT != null) { Mech_TrayInternaRT.gameObject.name = "Mech_TrayInternalRT"; };
      Transform Mech_RearSpotlightFill = this.gameObject.transform.FindRecursive("Mech_RearSpotlightFill");
      Transform Mech_FrontSpotlightFill = this.gameObject.transform.FindRecursive("Mech_FrontSpotlightFill");
      if (Mech_FrontSpotlightFill != null) { toHide.Add(Mech_FrontSpotlightFill.gameObject); }
      if (Mech_RearSpotlightFill != null) { toHide.Add(Mech_RearSpotlightFill.gameObject); }
      Transform HitDirectionEcho_LeftFront = this.gameObject.transform.FindRecursive("HitDirectionEcho_LeftFront");
      if (HitDirectionEcho_LeftFront != null) { toHide.Add(HitDirectionEcho_LeftFront.gameObject); }
      Transform HitDirectionEcho_RightFront = this.gameObject.transform.FindRecursive("HitDirectionEcho_RightFront");
      if (HitDirectionEcho_RightFront != null) { toHide.Add(HitDirectionEcho_RightFront.gameObject); }
      Transform HitDirectionEcho_LeftRear = this.gameObject.transform.FindRecursive("HitDirectionEcho_LeftRear");
      if (HitDirectionEcho_LeftRear != null) { toHide.Add(HitDirectionEcho_LeftRear.gameObject); }
      Transform HitDirectionEcho_RightRear = this.gameObject.transform.FindRecursive("HitDirectionEcho_RightRear");
      if (HitDirectionEcho_RightRear != null) { toHide.Add(HitDirectionEcho_RightRear.gameObject); }

      if (instance.directionalIndicatorLeftFront != null) { toHide.Add(instance.directionalIndicatorLeftFront.gameObject); }
      if (instance.directionalIndicatorRightFront != null) { toHide.Add(instance.directionalIndicatorRightFront.gameObject); }
      if (instance.directionalIndicatorLeftRear != null) { toHide.Add(instance.directionalIndicatorLeftRear.gameObject); }
      if (instance.directionalIndicatorRightRear != null) { toHide.Add(instance.directionalIndicatorRightRear.gameObject); }
      SVGImage[] svgs = this.gameObject.GetComponentsInChildren<SVGImage>(true);
      foreach (SVGImage svg in svgs) {
        if (svg.name.StartsWith("MechTray_Armor") == false) { continue; }
        if (svg.name.EndsWith("OutlineRear") == false) { continue; }
        toHide.Add(svg.gameObject);
      }
      //Dictionary<int, CombatHUDMechTrayArmorHover> hovers = new Dictionary<int, CombatHUDMechTrayArmorHover>();
      //Dictionary<int, CombatHUDTooltipHoverElement> tooltips = new Dictionary<int, CombatHUDTooltipHoverElement>();
      for (int index = 0; index < instance.Armor.Length; ++index) {
        SVGImage svg = instance.Armor[index];
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TargetHUDArmorReadout.ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = BaySquadReadoutAligner.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 2.1f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechArmor.Add(index, svg);
        SquadArmor.Add(index, squadSVG.GetComponent<SVGImage>());
        RectTransform outline = squadSVG.transform.FindRecursive(svg.name + BaySquadReadoutAligner.OUTLINE_SUFFIX) as RectTransform;
        outline.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
        outline.gameObject.name = outline.gameObject.name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TargetHUDArmorReadout.ARMOR_PREFIX);
        outline.anchoredPosition = Vector2.zero;
        outline.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        outline.pivot = new Vector2(0f, 1f);
        SquadArmorOutline.Add(index, outline.gameObject.GetComponent<SVGImage>());
      }
      foreach (SVGImage svg in instance.ArmorRear) {
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
      }
      for (int index = 0; index < instance.Armor.Length; ++index) {
        SVGImage svg = instance.Structure[index];
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
        MechArmorOutline.Add(index, svg);
      }
      for (int index = 0; index < instance.Structure.Length; ++index) {
        SVGImage svg = instance.Structure[index];
        if (svg == null) {
          MechStructure.Add(index, null);
          SquadStructure.Add(index, null);
          continue;
        }
        toHide.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.STRUCTURE_PREFIX, TargetHUDArmorReadout.STRUCTURE_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = BaySquadReadoutAligner.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 2.1f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechStructure.Add(index, svg);
        SquadStructure.Add(index, squadSVG.GetComponent<SVGImage>());
        //squadSVG.AddComponent<CombatHUDSquadTrayArmorHover>().Init(hovers[index], tooltips[index]);
      }
      foreach (SVGImage svg in instance.StructureRear) {
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
      }
    }
    public void ShowMech(Mech mech) {
      foreach (GameObject go in toHide) {
        go.SetActive(true);
      }
      foreach (GameObject go in toShow) {
        go.SetActive(false);
      }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = MechArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = MechArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = MechStructure[index];
        if (readout.Structure[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
    public void ShowSquad(TrooperSquad squad) {
      foreach (GameObject go in toHide) {
        go.SetActive(false);
      }
      foreach (GameObject go in toShow) {
        go.SetActive(true);
      }
      UnitCustomInfo info = squad.GetCustomInfo();
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.outlineIcon) == false) {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.outlineIcon, squad.Combat.DataManager);
        } else {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.armorIcon) == false) {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.armorIcon, squad.Combat.DataManager);
        } else {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.structureIcon) == false) {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.structureIcon, squad.Combat.DataManager);
        } else {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
  }
  public class CalledHUDSquadArmorReadout : MonoBehaviour {
    public static readonly string ARMOR_PREFIX = "MechTray_Armor";
    public static readonly string STRUCTURE_PREFIX = "Mech_TrayInternal";
    public static readonly float SQUAD_ICON_SIZE = 25f;
    public static readonly List<string> READOUT_NAMES = new List<string>() { "Head", "Torso", "RT", "LT", "RA", "LA", "RL", "LL" };
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() { { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };
    public HashSet<GameObject> toHide { get; set; }
    public HashSet<GameObject> toShow { get; set; }
    public Dictionary<int, SVGImage> SquadArmor { get; private set; }
    public Dictionary<int, SVGImage> SquadArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> SquadStructure { get; private set; }
    public Dictionary<int, SVGImage> MechArmor { get; private set; }
    public Dictionary<int, SVGImage> MechArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> MechStructure { get; private set; }
    public CalledHUDSquadArmorReadout() {
      toHide = new HashSet<GameObject>();
      toShow = new HashSet<GameObject>();
      SquadArmor = new Dictionary<int, SVGImage>();
      SquadArmorOutline = new Dictionary<int, SVGImage>();
      SquadStructure = new Dictionary<int, SVGImage>();
      MechArmor = new Dictionary<int, SVGImage>();
      MechArmorOutline = new Dictionary<int, SVGImage>();
      MechStructure = new Dictionary<int, SVGImage>();
    }
    public void Instantine(HUDMechArmorReadout instance) {
      Log.TWL(0, "CalledHUDSquadArmorReadout.Instantine");
      Transform Mech_TrayInternaLT = this.gameObject.transform.FindRecursive("Mech_TrayInternaLT");
      if (Mech_TrayInternaLT != null) { Mech_TrayInternaLT.gameObject.name = "Mech_TrayInternalLT"; };
      Transform Mech_TrayInternaRT = this.gameObject.transform.FindRecursive("Mech_TrayInternaRT");
      if (Mech_TrayInternaRT != null) { Mech_TrayInternaRT.gameObject.name = "Mech_TrayInternalRT"; };
      if (instance.directionalIndicatorLeftFront != null) { toHide.Add(instance.directionalIndicatorLeftFront.gameObject); }
      if (instance.directionalIndicatorRightFront != null) { toHide.Add(instance.directionalIndicatorRightFront.gameObject); }
      if (instance.directionalIndicatorLeftRear != null) { toHide.Add(instance.directionalIndicatorLeftRear.gameObject); }
      if (instance.directionalIndicatorRightRear != null) { toHide.Add(instance.directionalIndicatorRightRear.gameObject); }
      SVGImage[] svgs = this.gameObject.GetComponentsInChildren<SVGImage>(true);
      foreach (SVGImage svg in svgs) {
        if (svg.name.StartsWith("MechTray_Armor") == false) { continue; }
        if (svg.name.EndsWith("OutlineRear") == false) { continue; }
        toHide.Add(svg.gameObject);
      }
      Transform FrontArmorComponents = this.gameObject.transform.FindRecursive("FrontArmorComponents");
      Transform RearArmorComponents = this.gameObject.transform.FindRecursive("RearArmorComponents");
      toHide.Add(FrontArmorComponents.gameObject);
      toHide.Add(RearArmorComponents.gameObject);
      GameObject SquadArmorComponents = GameObject.Instantiate(FrontArmorComponents.gameObject);
      SquadArmorComponents.name = "SquadArmorComponents";
      SquadArmorComponents.transform.SetParent(FrontArmorComponents.parent);
      SquadArmorComponents.transform.localPosition = FrontArmorComponents.localPosition;
      SquadArmorComponents.transform.localScale = FrontArmorComponents.localScale;
      RectTransform SquadArmorComponentsTR = SquadArmorComponents.GetComponent<RectTransform>();
      SquadArmorComponentsTR.pivot = new Vector3(0f, 1.15f);
      toShow.Add(SquadArmorComponents);
      for (int index = 0; index < instance.Armor.Length; ++index) {
        int squadIndex = CalledHUDSquadArmorReadout.READOUT_INDEX_TO_SQUAD[index];
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + CalledHUDSquadArmorReadout.READOUT_NAMES[squadIndex];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + CalledHUDSquadArmorReadout.READOUT_NAMES[squadIndex] + BaySquadReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + CalledHUDSquadArmorReadout.READOUT_NAMES[squadIndex];
        RectTransform MechTray_Armor = FrontArmorComponents.transform.FindRecursive(armorName) as RectTransform;
        RectTransform MechTray_ArmorOutline = FrontArmorComponents.transform.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform Mech_TrayInternal = FrontArmorComponents.transform.FindRecursive(structureName) as RectTransform;
        RectTransform SquadTray_Armor = SquadArmorComponents.transform.FindRecursive(armorName) as RectTransform;
        RectTransform SquadTray_ArmorOutline = SquadArmorComponents.transform.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform Squad_TrayInternal = SquadArmorComponents.transform.FindRecursive(structureName) as RectTransform;
        CombatHUDMechTrayArmorHover hover = MechTray_ArmorOutline.GetComponentInChildren<CombatHUDMechTrayArmorHover>(true);
        Log.WL(1, armorName + " " + armorOutlineName + " " + structureName + " " + index + " " + squadIndex + " hover:" + (hover == null ? "null" : hover.chassisIndex.ToString()));
        SquadTray_Armor.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
        SquadTray_ArmorOutline.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
        Squad_TrayInternal.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
        MechArmor.Add(index, MechTray_Armor.gameObject.GetComponent<SVGImage>());
        MechArmorOutline.Add(index, MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>());
        MechStructure.Add(index, Mech_TrayInternal.gameObject.GetComponent<SVGImage>());
        SquadArmor.Add(index, SquadTray_Armor.gameObject.GetComponent<SVGImage>());
        SquadArmorOutline.Add(index, SquadTray_ArmorOutline.gameObject.GetComponent<SVGImage>());
        SquadStructure.Add(index, Squad_TrayInternal.gameObject.GetComponent<SVGImage>());
        SquadTray_Armor.anchoredPosition = Vector2.zero;
        SquadTray_ArmorOutline.anchoredPosition = Vector2.zero;
        Squad_TrayInternal.anchoredPosition = Vector2.zero;
        SquadTray_Armor.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        SquadTray_ArmorOutline.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        Squad_TrayInternal.sizeDelta = new Vector2(TargetHUDArmorReadout.SQUAD_ICON_SIZE, TargetHUDArmorReadout.SQUAD_ICON_SIZE);
        SquadTray_ArmorOutline.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 2.1f + 1.1f * ((float)((int)squadIndex / (int)2)));
        SquadTray_Armor.pivot = new Vector2(0f, 1f);
        Squad_TrayInternal.pivot = new Vector2(0f, 1f);
      }
    }
    public void ShowMech(Mech mech) {
      foreach (GameObject go in toHide) {
        go.SetActive(true);
      }
      foreach (GameObject go in toShow) {
        go.SetActive(false);
      }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = MechArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = MechArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = MechStructure[index];
        if (readout.Structure[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
    public void ShowSquad(TrooperSquad squad) {
      foreach (GameObject go in toHide) {
        go.SetActive(false);
      }
      foreach (GameObject go in toShow) {
        go.SetActive(true);
      }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      UnitCustomInfo info = squad.GetCustomInfo();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.outlineIcon) == false) {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.outlineIcon, squad.Combat.DataManager);
        } else {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.armorIcon) == false) {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.armorIcon, squad.Combat.DataManager);
        } else {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.structureIcon) == false) {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.structureIcon, squad.Combat.DataManager);
        } else {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
  }
  public class TrayHUDCustomArmorReadout : MonoBehaviour {
    public static readonly string SQUAD_ARMOR_PREFIX = "SquadTray_Armor";
    public static readonly string SQUAD_STRUCTURE_PREFIX = "Squad_TrayInternal";
    public static readonly float SQUAD_ICON_SIZE = 25f;
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() { { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };
    //public HashSet<GameObject> toHide { get; set; }
    public HashSet<GameObject> toShowMech { get; set; }
    public HashSet<GameObject> toShowVehicle { get; set; }
    public HashSet<GameObject> toShowSquad { get; set; }
    public Dictionary<int, SVGImage> SquadArmor { get; private set; }
    public Dictionary<int, SVGImage> SquadArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> SquadStructure { get; private set; }
    public Dictionary<int, SVGImage> VehicleArmor { get; private set; }
    public Dictionary<int, SVGImage> VehicleArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> VehicleStructure { get; private set; }
    public Dictionary<int, SVGImage> MechArmor { get; private set; }
    public Dictionary<int, SVGImage> MechArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> MechStructure { get; private set; }
    public TrayHUDCustomArmorReadout() {
      toShowMech = new HashSet<GameObject>();
      toShowVehicle = new HashSet<GameObject>();
      toShowSquad = new HashSet<GameObject>();
      SquadArmor = new Dictionary<int, SVGImage>();
      SquadArmorOutline = new Dictionary<int, SVGImage>();
      SquadStructure = new Dictionary<int, SVGImage>();
      MechArmor = new Dictionary<int, SVGImage>();
      MechArmorOutline = new Dictionary<int, SVGImage>();
      MechStructure = new Dictionary<int, SVGImage>();
    }
    public void ShowMech(Mech mech) {
      foreach (GameObject go in toShowSquad) { go.SetActive(false); }
      foreach (GameObject go in toShowVehicle) { go.SetActive(false); }
      foreach (GameObject go in toShowMech) { go.SetActive(true); }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = MechArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = MechArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = MechStructure[index];
        if (readout.Structure[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = mech.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
    public void ShowSquad(TrooperSquad squad) {
      foreach (GameObject go in toShowMech) { go.SetActive(false); }
      foreach (GameObject go in toShowVehicle) { go.SetActive(false); }
      foreach (GameObject go in toShowSquad) { go.SetActive(true); }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      UnitCustomInfo info = squad.GetCustomInfo();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        if(string.IsNullOrEmpty(info.SquadInfo.outlineIcon) == false) {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.outlineIcon, squad.Combat.DataManager);
        } else {
          readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.armorIcon) == false) {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.armorIcon, squad.Combat.DataManager);
        } else {
          readout.Armor[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
        if (string.IsNullOrEmpty(info.SquadInfo.structureIcon) == false) {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.structureIcon, squad.Combat.DataManager);
        } else {
          readout.Structure[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, squad.Combat.DataManager);
        }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
    public virtual void Instantine(HUDMechArmorReadout instance) {
      this.InstantineSquad(instance);
    }
    protected void InstantineSquad(HUDMechArmorReadout instance) {
      Transform Mech_TrayInternaLT = this.gameObject.transform.FindRecursive("Mech_TrayInternaLT");
      if (Mech_TrayInternaLT != null) { Mech_TrayInternaLT.gameObject.name = "Mech_TrayInternalLT"; };
      Transform Mech_TrayInternaRT = this.gameObject.transform.FindRecursive("Mech_TrayInternaRT");
      if (Mech_TrayInternaRT != null) { Mech_TrayInternaRT.gameObject.name = "Mech_TrayInternalRT"; };
      Transform Mech_RearSpotlightFill = this.gameObject.transform.FindRecursive("Mech_RearSpotlightFill");
      Transform Mech_FrontSpotlightFill = this.gameObject.transform.FindRecursive("Mech_FrontSpotlightFill");
      if (Mech_FrontSpotlightFill != null) { toShowMech.Add(Mech_FrontSpotlightFill.gameObject); }
      if (Mech_RearSpotlightFill != null) { toShowMech.Add(Mech_RearSpotlightFill.gameObject); }
      if (instance.directionalIndicatorLeftFront != null) { toShowMech.Add(instance.directionalIndicatorLeftFront.gameObject); }
      if (instance.directionalIndicatorRightFront != null) { toShowMech.Add(instance.directionalIndicatorRightFront.gameObject); }
      if (instance.directionalIndicatorLeftRear != null) { toShowMech.Add(instance.directionalIndicatorLeftRear.gameObject); }
      if (instance.directionalIndicatorRightRear != null) { toShowMech.Add(instance.directionalIndicatorRightRear.gameObject); }
      SVGImage[] svgs = this.gameObject.GetComponentsInChildren<SVGImage>(true);
      foreach (SVGImage svg in svgs) {
        if (svg.name.StartsWith("MechTray_Armor") == false) { continue; }
        if (svg.name.EndsWith("OutlineRear") == false) { continue; }
        toShowMech.Add(svg.gameObject);
      }
      Dictionary<int, CombatHUDMechTrayArmorHover> hovers = new Dictionary<int, CombatHUDMechTrayArmorHover>();
      Dictionary<int, CombatHUDTooltipHoverElement> tooltips = new Dictionary<int, CombatHUDTooltipHoverElement>();

      for (int index = 0; index < instance.ArmorOutline.Length; ++index) {
        SVGImage svg = instance.ArmorOutline[index];
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, SQUAD_ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE);
        int squadIndex = READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));
        toShowSquad.Add(squadSVG);
        MechArmorOutline.Add(index, svg);
        SquadArmorOutline.Add(index, squadSVG.GetComponent<SVGImage>());
        hovers.Add(index, squadSVG.GetComponent<CombatHUDMechTrayArmorHover>());
        tooltips.Add(index, squadSVG.GetComponent<CombatHUDTooltipHoverElement>());
      }
      for (int index = 0; index < instance.Armor.Length; ++index) {
        SVGImage svg = instance.Armor[index];
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, SQUAD_ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE);
        int squadIndex = READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShowSquad.Add(squadSVG);
        MechArmor.Add(index, svg);
        SquadArmor.Add(index, squadSVG.GetComponent<SVGImage>());
      }
      foreach (SVGImage svg in instance.ArmorRear) {
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
      }
      foreach (SVGImage svg in instance.ArmorOutlineRear) {
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
      }
      for (int index = 0; index < instance.Structure.Length; ++index) {
        SVGImage svg = instance.Structure[index];
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.STRUCTURE_PREFIX, SQUAD_STRUCTURE_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE);
        int squadIndex = READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShowSquad.Add(squadSVG);
        MechStructure.Add(index, svg);
        SquadStructure.Add(index, squadSVG.GetComponent<SVGImage>());
        squadSVG.AddComponent<CombatHUDCustomTrayArmorHover>().Init(hovers[index], tooltips[index]);
      }
      foreach (SVGImage svg in instance.StructureRear) {
        if (svg == null) { continue; }
        toShowMech.Add(svg.gameObject);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnPointerEnter")]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDMechTrayArmorHover_OnPointerEnter {
    public static void Prefix(CombatHUDMechTrayArmorHover __instance, PointerEventData data) {
      try {
        Log.TWL(0, "CombatHUDMechTrayArmorHover.OnPointerEnter " + __instance.gameObject.name);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Init")]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class CombatHUDTargetingComputer_Init {
    public static void Postfix(CombatHUDTargetingComputer __instance, CombatHUD HUD) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        if (computerCustom == null) { computerCustom = __instance.gameObject.AddComponent<CombatHUDTargetingComputerCustom>(); }
        computerCustom.Init(HUD,__instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Init")]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class CombatHUDCalledShotPopUp_Init {
    private delegate string d_GetHitPercent(CombatHUDCalledShotPopUp popup, VehicleChassisLocations location, VehicleChassisLocations targetedLocation);
    private static d_GetHitPercent i_GetHitPercent = null;
    public static string GetHitPercent(this CombatHUDCalledShotPopUp popup, VehicleChassisLocations location, VehicleChassisLocations targetedLocation) {
      if (i_GetHitPercent == null) { return string.Empty; }
      return i_GetHitPercent(popup, location, targetedLocation);
    }
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDCalledShotPopUp).GetMethod("GetHitPercent", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(VehicleChassisLocations), typeof(VehicleChassisLocations) }, null);
        var dm = new DynamicMethod("CUGetHitPercent", typeof(string), new Type[] { typeof(CombatHUDCalledShotPopUp), typeof(VehicleChassisLocations), typeof(VehicleChassisLocations) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_GetHitPercent = (d_GetHitPercent)dm.CreateDelegate(typeof(d_GetHitPercent));
      }
      return true;
    }
    public static void Postfix(CombatHUDCalledShotPopUp __instance, CombatHUD HUD) {
      try {
        CombatHUDCalledShotPopUpCustom popupCustom = __instance.gameObject.GetComponent<CombatHUDCalledShotPopUpCustom>();
        if (popupCustom == null) { popupCustom = __instance.gameObject.AddComponent<CombatHUDCalledShotPopUpCustom>(); }
        popupCustom.Init(HUD, __instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateMechDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDCalledShotPopUp_UpdateMechDisplay {
    public static bool Prefix(CombatHUDCalledShotPopUp __instance) {
      try {
        Thread.CurrentThread.pushActor(__instance.DisplayedActor as Mech);
        if (__instance.DisplayedActor.FakeVehicle() == false) { return true; }
        CombatHUDCalledShotPopUpCustom customPopup = __instance.gameObject.GetComponent<CombatHUDCalledShotPopUpCustom>();
        if (customPopup == null) { return true; }
        customPopup.UpdateFakeVehicleDisplay();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(CombatHUDCalledShotPopUp __instance) {
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("ShownAttackDirection")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDCalledShotPopUp_ShownAttackDirection {
    public static void Prefix(CombatHUDCalledShotPopUp __instance, CombatHUD ___HUD, ref AttackDirection value, ref AttackDirection ___shownAttackDirection, Mech ___displayedMech) {
      Thread.CurrentThread.pushActor(___displayedMech);
      TrooperSquad squad = ___displayedMech as TrooperSquad;
      if (squad != null) {
        value = AttackDirection.FromFront;
      }
    }
    public static void Postfix(CombatHUDCalledShotPopUp __instance, CombatHUD ___HUD, ref AttackDirection value, Mech ___displayedMech, ref AttackDirection ___shownAttackDirection) {
      try {
        TrooperSquad squad = ___displayedMech as TrooperSquad;
        if (squad != null) {
          ___shownAttackDirection = AttackDirection.FromFront;
          __instance.FrontComponents.SetActive(false);
          __instance.RearComponents.SetActive(false);
          Traverse.Create(__instance).Property<Dictionary<ArmorLocation, int>>("currentHitTable").Value = squad.GetHitTable(value);
          if (__instance.MechArmorDisplay.HoveredArmor != ArmorLocation.None) {
            __instance.MechArmorDisplay.ClearHoveredArmor(__instance.MechArmorDisplay.HoveredArmor);
          }
        }
        if (__instance.DisplayedActor.FakeVehicle()) { 
          Traverse.Create(__instance).Property<Dictionary<VehicleChassisLocations, int>>("currentVHitTable").Value = ___HUD.Combat.HitLocation.GetVehicleHitTable(value, false);
          CombatHUDCalledShotPopUpCustom popupCustom = __instance.gameObject.GetComponent<CombatHUDCalledShotPopUpCustom>();
          if (popupCustom != null) { popupCustom.ShownAttackDirection = value; }
        }
        Log.TWL(0, $"CombatHUDCalledShotPopUp.ShownAttackDirection {value} {__instance.DisplayedActor.PilotableActorDef.ChassisID}");
        Log.W(2, "hitTable:");
        var mechHitTable = Traverse.Create(__instance).Property<Dictionary<ArmorLocation, int>>("currentHitTable").Value;
        foreach (var hit in mechHitTable) { Log.W(1, $"{hit.Key}:{hit.Value}"); }
        Log.WL(0, "");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowMechDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDCalledShotPopUp_ShowMechDisplay {
    public static bool Prefix(CombatHUDCalledShotPopUp __instance) {
      try {
        Log.TWL(0, "CombatHUDCalledShotPopUp.ShowMechDisplay");
        CombatHUDCalledShotPopUpCustom customPopup = __instance.gameObject.GetComponent<CombatHUDCalledShotPopUpCustom>();
        if (customPopup == null) {
          return true;
        }
        if (__instance.DisplayedActor == null) { return true; }
        if (__instance.DisplayedActor.FakeVehicle() == false) {
          customPopup.fakeVehicleReadout.gameObject.SetActive(false);
          return true;
        }
        customPopup.fakeVehicleReadout.DisplayedVehicle = __instance.DisplayedActor as Mech;
        customPopup.ShowFakeVehicleDisplay();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowVehicleDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDCalledShotPopUp_ShowVehicleDisplay {
    public static void Postfix(CombatHUDCalledShotPopUp __instance) {
      try {
        Log.TWL(0, "CombatHUDCalledShotPopUp.ShowVehicleDisplay");
        CombatHUDCalledShotPopUpCustom customPopup = __instance.gameObject.GetComponent<CombatHUDCalledShotPopUpCustom>();
        if (customPopup == null) { return; }
        customPopup.fakeVehicleReadout.gameObject.SetActive(false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("ShowMechDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_ShowMechDisplay {
    public static void Postfix(CombatHUDTargetingComputer __instance) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        ICombatant DisplayedCombatant = __instance.ActivelyShownCombatant;
        //if (DisplayedCombatant == null) { DisplayedCombatant = __instance.MechArmorDisplay.DisplayedMech; }
        Log.TWL(0, "CombatHUDTargetingComputer.ShowMechDisplay DisplayedCombatant:" + (DisplayedCombatant == null ? "null":(DisplayedCombatant.PilotableActorDef.Description.Id + " fake:"+ DisplayedCombatant.FakeVehicle())) + " computerCustom:"+(computerCustom == null?"null":"not null"));
        if (computerCustom == null) { return; }
        if (DisplayedCombatant is AbstractActor actor) {
          //Log.WL(1, "TurretArmorReadout:"+ actor.GetCustomInfo().TurretArmorReadout+" isFakeDisplay:" + );
          if ((actor.GetCustomInfo().TurretArmorReadout)&&(__instance.TurretArmorDisplay is FakeHUDTurretArmorReadout fakeHUDTurretArmorReadout)) {
            Log.WL(1,"display as turret");
            __instance.MechArmorDisplay.DisplayedMech = null;
            __instance.MechArmorDisplay.gameObject.SetActive(false);
            computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
            computerCustom.fakeVehicleReadout.DisplayedVehicle = null;
            fakeHUDTurretArmorReadout.gameObject.SetActive(true);
            fakeHUDTurretArmorReadout._DisplayedTurret = actor;
          } else
          if (actor.FakeVehicle()) {
            Log.WL(1, "display as vehicle");
            __instance.MechArmorDisplay.DisplayedMech = null;
            __instance.MechArmorDisplay.gameObject.SetActive(false);
            computerCustom.fakeVehicleReadout.gameObject.SetActive(true);
            computerCustom.fakeVehicleReadout.DisplayedVehicle = actor as Mech;
          } else {
            Log.WL(1, "display as mech");
            computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
            computerCustom.fakeVehicleReadout.DisplayedVehicle = null;
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("showVehicleDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_showVehicleDisplay {
    public static void Postfix(CombatHUDTargetingComputer __instance) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        if (computerCustom == null) { return; }
        computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("showBuildingDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_showBuildingDisplay {
    public static void Postfix(CombatHUDTargetingComputer __instance) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        if (computerCustom == null) { return; }
        computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("showTurretDisplay")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_showTurretDisplay {
    public static void Postfix(CombatHUDTargetingComputer __instance) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        if (computerCustom == null) { return; }
        computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("UpdateStructureAndArmor")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_UpdateStructureAndArmor {
    public static bool Prefix(CombatHUDTargetingComputer __instance) {
      try {
        CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
        if (computerCustom == null) { return true; }
        if (__instance.ActivelyShownCombatant == null) { return true; }
        if (__instance.ActivelyShownCombatant.UnitType != UnitType.Mech) { return true; }
        AbstractActor actor = __instance.ActivelyShownCombatant as AbstractActor;
        if (actor == null) { return true; }
        //bool fakeVehicle = actor.FakeVehicle();
        if (actor.GetCustomInfo().TurretArmorReadout) {
          __instance.TurretArmorDisplay.UpdateTurretStructureAndArmor();
          return false;
        } else if (actor.FakeVehicle()) { 
          computerCustom.fakeVehicleReadout.UpdateVehicleStructureAndArmor(__instance.shownAttackDirection);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RefreshHoverInfo")]
  [HarmonyPatch(new Type[] { })]
  public static class HUDMechArmorReadout_RefreshHoverInfoSquad {
    public static void Prefix(HUDMechArmorReadout __instance) {
      try {
        Transform squad_FrontArmor = __instance.gameObject.transform.FindRecursive("squad_FrontArmor");
        if (squad_FrontArmor != null) {
          squad_FrontArmor.gameObject.GetComponent<BaySquadReadoutAligner>().ResetUI();
        }
        Transform vehicle_FrontArmor = __instance.gameObject.transform.FindRecursive("vehicle_FrontArmor");
        if (vehicle_FrontArmor != null) {
          vehicle_FrontArmor.gameObject.GetComponent<BayVehicleReadoutAligner>().ResetUI();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(typeof(CombatHUDInWorldElementMgr))]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch("ShowAttackDirection")]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor), typeof(AttackDirection), typeof(float), typeof(MeleeAttackType), typeof(int) })]
  //public static class CombatHUDInWorldElementMgr_ShowAttackDirection {
  //  public static void Prefix(CombatHUDInWorldElementMgr __instance, AbstractActor attacker, AbstractActor target, AttackDirection direction, float yOffset, MeleeAttackType meleeAttackType, int firingWeapons, CombatHUD ___HUD) {
  //    try {
  //      Log.TWL(0, $"CombatHUDInWorldElementMgr.ShowAttackDirection {attacker.PilotableActorDef.ChassisID}->{target.PilotableActorDef.ChassisID} direction:{direction} TargetingComputer:{(___HUD.TargetingComputer.DisplayedCombatant == null?"null": ___HUD.TargetingComputer.DisplayedCombatant.PilotableActorDef.ChassisID)}");
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedMech")]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class HUDMechArmorReadout_DisplayedMechSquad {
    public static void Prefix(HUDMechArmorReadout __instance, Mech value) {
      if (value == null) { return; }
      TrooperSquad squad = value as TrooperSquad;
      TrayHUDCustomArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TrayHUDCustomArmorReadout>();
      if (squadArmorReadout != null) {
        if (squad != null) { squadArmorReadout.ShowSquad(squad); return; };
        squadArmorReadout.ShowMech(value);
      }
      TargetHUDArmorReadout squadArmorReadoutTrg = __instance.gameObject.GetComponent<TargetHUDArmorReadout>();
      if (squadArmorReadoutTrg != null) {
        if (squad != null) { squadArmorReadoutTrg.ShowSquad(squad); return; };
        squadArmorReadoutTrg.ShowMech(value);
      }
      CalledHUDSquadArmorReadout squadArmorReadoutCall = __instance.gameObject.GetComponent<CalledHUDSquadArmorReadout>();
      if (squadArmorReadoutCall != null) {
        if (squad != null) { squadArmorReadoutCall.ShowSquad(squad); return; };
        squadArmorReadoutCall.ShowMech(value);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD), typeof(bool), typeof(bool), typeof(bool) })]
  public static class HUDMechArmorReadout_Init_info {
    public static void Prefix(HUDMechArmorReadout __instance, CombatHUD HUD, bool useHoversForCalledShots, bool hideArmorWhenStructureDamage, bool showArmorAllOrNothing) {
      if (HUD == null) {
        Transform vehicle_FrontArmor = __instance.gameObject.transform.FindRecursive("vehicle_FrontArmor");
        Transform mech_FrontArmor = __instance.gameObject.transform.FindRecursive("mech_FrontArmor");
        Transform squad_FrontArmor = __instance.gameObject.transform.FindRecursive("squad_FrontArmor");
        if (vehicle_FrontArmor == null) {
          if (mech_FrontArmor != null) {
            vehicle_FrontArmor = GameObject.Instantiate(mech_FrontArmor.gameObject).transform;
            vehicle_FrontArmor.gameObject.name = "vehicle_FrontArmor";
            vehicle_FrontArmor.SetParent(mech_FrontArmor.parent, false);
            vehicle_FrontArmor.localPosition = mech_FrontArmor.localPosition;
            vehicle_FrontArmor.localScale = mech_FrontArmor.localScale;
            vehicle_FrontArmor.gameObject.AddComponent<BayVehicleReadoutAligner>().Init(__instance, mech_FrontArmor as RectTransform);
          }
        }
        if ((squad_FrontArmor == null) && (mech_FrontArmor != null)) {
          squad_FrontArmor = GameObject.Instantiate(mech_FrontArmor.gameObject).transform;
          squad_FrontArmor.gameObject.name = "squad_FrontArmor";
          squad_FrontArmor.SetParent(mech_FrontArmor.parent, false);
          squad_FrontArmor.localPosition = mech_FrontArmor.localPosition;
          squad_FrontArmor.localScale = mech_FrontArmor.localScale;
          squad_FrontArmor.gameObject.AddComponent<BaySquadReadoutAligner>().Init(__instance, mech_FrontArmor as RectTransform, true);
        }
      } else {
        if (__instance.gameObject.GetComponent<CombatHUDMechTray>() != null) {
          TrayHUDCustomArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TrayHUDCustomArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<TrayHUDCustomArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        } else if (__instance.gameObject.transform.parent.parent.gameObject.GetComponent<CombatHUDTargetingComputer>() != null) {
          TargetHUDArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TargetHUDArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<TargetHUDArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        } else if (__instance.gameObject.transform.parent.parent.parent.gameObject.GetComponent<CombatHUDCalledShotPopUp>() != null) {
          CalledHUDSquadArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<CalledHUDSquadArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<CalledHUDSquadArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedMechDef")]
  //[HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class HUDMechArmorReadout_DisplayedMechDefSquad {
    public static float GetLocationArmor(this MechDef def, ArmorLocation location) {
      if (def.Chassis != null) {
        switch (location) {
          case ArmorLocation.Head: return def.Chassis.Head.MaxArmor;
          case ArmorLocation.LeftArm: return def.Chassis.LeftArm.MaxArmor;
          case ArmorLocation.RightArm: return def.Chassis.RightArm.MaxArmor;
          case ArmorLocation.LeftLeg: return def.Chassis.LeftLeg.MaxArmor;
          case ArmorLocation.RightLeg: return def.Chassis.RightLeg.MaxArmor;
          case ArmorLocation.CenterTorso: return def.Chassis.CenterTorso.MaxArmor;
          case ArmorLocation.RightTorso: return def.Chassis.RightTorso.MaxArmor;
          case ArmorLocation.LeftTorso: return def.Chassis.LeftTorso.MaxArmor;
          case ArmorLocation.CenterTorsoRear: return def.Chassis.CenterTorso.MaxArmor;
          case ArmorLocation.RightTorsoRear: return def.Chassis.RightTorso.MaxArmor;
          case ArmorLocation.LeftTorsoRear: return def.Chassis.LeftTorso.MaxArmor;
        }
      } else {
        switch (location) {
          case ArmorLocation.Head: return def.Head.CurrentArmor;
          case ArmorLocation.LeftArm: return def.LeftArm.CurrentArmor;
          case ArmorLocation.RightArm: return def.RightArm.CurrentArmor;
          case ArmorLocation.LeftLeg: return def.LeftLeg.CurrentArmor;
          case ArmorLocation.RightLeg: return def.RightLeg.CurrentArmor;
          case ArmorLocation.CenterTorso: return def.CenterTorso.CurrentArmor;
          case ArmorLocation.RightTorso: return def.RightTorso.CurrentArmor;
          case ArmorLocation.LeftTorso: return def.LeftTorso.CurrentArmor;
          case ArmorLocation.CenterTorsoRear: return def.CenterTorso.CurrentRearArmor;
          case ArmorLocation.RightTorsoRear: return def.RightTorso.CurrentRearArmor;
          case ArmorLocation.LeftTorsoRear: return def.LeftTorso.CurrentRearArmor;
        }
      }
      return 0f;
    }
    public static float GetLocationStructure(this MechDef def, ArmorLocation location) {
      if (def.Chassis != null) {
        switch (location) {
          case ArmorLocation.Head: return def.Chassis.Head.InternalStructure;
          case ArmorLocation.LeftArm: return def.Chassis.LeftArm.InternalStructure;
          case ArmorLocation.RightArm: return def.Chassis.RightArm.InternalStructure;
          case ArmorLocation.LeftLeg: return def.Chassis.LeftLeg.InternalStructure;
          case ArmorLocation.RightLeg: return def.Chassis.RightLeg.InternalStructure;
          case ArmorLocation.CenterTorso: return def.Chassis.CenterTorso.InternalStructure;
          case ArmorLocation.RightTorso: return def.Chassis.RightTorso.InternalStructure;
          case ArmorLocation.LeftTorso: return def.Chassis.LeftTorso.InternalStructure;
          case ArmorLocation.CenterTorsoRear: return def.Chassis.CenterTorso.InternalStructure;
          case ArmorLocation.RightTorsoRear: return def.Chassis.RightTorso.InternalStructure;
          case ArmorLocation.LeftTorsoRear: return def.Chassis.LeftTorso.InternalStructure;
        }
      } else {
        switch (location) {
          case ArmorLocation.Head: return def.Head.CurrentInternalStructure;
          case ArmorLocation.LeftArm: return def.LeftArm.CurrentInternalStructure;
          case ArmorLocation.RightArm: return def.RightArm.CurrentInternalStructure;
          case ArmorLocation.LeftLeg: return def.LeftLeg.CurrentInternalStructure;
          case ArmorLocation.RightLeg: return def.RightLeg.CurrentInternalStructure;
          case ArmorLocation.CenterTorso: return def.CenterTorso.CurrentInternalStructure;
          case ArmorLocation.RightTorso: return def.RightTorso.CurrentInternalStructure;
          case ArmorLocation.LeftTorso: return def.LeftTorso.CurrentInternalStructure;
          case ArmorLocation.CenterTorsoRear: return def.CenterTorso.CurrentInternalStructure;
          case ArmorLocation.RightTorsoRear: return def.RightTorso.CurrentInternalStructure;
          case ArmorLocation.LeftTorsoRear: return def.LeftTorso.CurrentInternalStructure;
        }
      }
      return 0f;
    }
    public static void BayShowSquad(this HUDMechArmorReadout readout, ChassisDef def) {
      Log.TWL(0, "HUDMechArmorReadout.BayShowSquad " + (def == null ? "null" : def.Description.Id));
      Transform mech_FrontArmor = readout.gameObject.transform.FindRecursive("mech_FrontArmor");
      Transform mech_RearArmor = readout.gameObject.transform.FindRecursive("mech_RearArmor");
      Transform squad_FrontArmor = readout.gameObject.transform.FindRecursive("squad_FrontArmor");
      Transform vehicle_FrontArmor = readout.gameObject.transform.FindRecursive("vehicle_FrontArmor");
      if (squad_FrontArmor != null) { squad_FrontArmor.gameObject.SetActive(true); }
      if (mech_FrontArmor != null) { mech_FrontArmor.gameObject.SetActive(false); }
      if (mech_RearArmor != null) { mech_RearArmor.gameObject.SetActive(false); }
      if (vehicle_FrontArmor != null) { vehicle_FrontArmor.gameObject.SetActive(false); }
      Log.TWL(0, "BayShowSquad");
      UnitCustomInfo info = def.GetCustomInfo();
      for (int index = 0; index < BaySquadReadoutAligner.READOUT_NAMES.Count; ++index) {
        int squadIndex = BaySquadReadoutAligner.READOUT_INDEX_TO_SQUAD[index];
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex] + BaySquadReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex];
        if (squad_FrontArmor == null) { continue; }
        Log.WL(1, squadIndex.ToString() + "/" + index + " " + armorName + " " + armorOutlineName + " " + structureName);

        RectTransform MechTray_Armor = squad_FrontArmor.FindRecursive(armorName) as RectTransform;
        RectTransform MechTray_ArmorOutline = squad_FrontArmor.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform MechTray_Structure = squad_FrontArmor.FindRecursive(structureName) as RectTransform;
        if (MechTray_Armor != null) readout.Armor[index] = MechTray_Armor.gameObject.GetComponent<SVGImage>();
        if (MechTray_ArmorOutline != null) readout.ArmorOutline[index] = MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>();
        if (MechTray_Structure != null) readout.Structure[index] = MechTray_Structure.gameObject.GetComponent<SVGImage>();
        try {
          if (readout.Armor[index] != null) {
            if (string.IsNullOrEmpty(info.SquadInfo.armorIcon) == false) {
              readout.Armor[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.armorIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              readout.Armor[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
          }
          if (readout.ArmorOutline[index] != null) {
            if (string.IsNullOrEmpty(info.SquadInfo.outlineIcon) == false) {
              readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.outlineIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              readout.ArmorOutline[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
          }
          if (readout.Structure[index] != null) {
            if (string.IsNullOrEmpty(info.SquadInfo.structureIcon) == false) {
              readout.Structure[index].vectorGraphics = CustomSvgCache.get(info.SquadInfo.structureIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              readout.Structure[index].vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
          }
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
        if (def != null) {
          LocationDef locationDef = def.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true)));
          float armor = locationDef.MaxArmor;
          float structure = locationDef.InternalStructure;
          Log.W(1, squadIndex.ToString() + "/" + index + " " + armorName + " " + armorOutlineName + " " + structureName + " a:" + armor + "(" + HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true) + ")" + " s:" + structure + "(" + HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true) + ")");
          if ((armor <= CustomAmmoCategories.Epsilon) && (structure <= 1f)) {
            Log.WL(1, " hide");
            if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(false);
            if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(false);
            if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(false);
          } else {
            Log.WL(1, " show");
            if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(true);
            if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(true);
            if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(true);
          }
        }
      }
    }
    public static void BayShowMech(this HUDMechArmorReadout readout, ChassisDef def) {
      Log.TWL(0, "HUDMechArmorReadout.BayShowMech " + (def == null?"null":def.Description.Id));
      Transform mech_FrontArmor = readout.gameObject.transform.FindRecursive("mech_FrontArmor");
      Transform mech_RearArmor = readout.gameObject.transform.FindRecursive("mech_RearArmor");
      Transform squad_FrontArmor = readout.gameObject.transform.FindRecursive("squad_FrontArmor");
      Transform vehicle_FrontArmor = readout.gameObject.transform.FindRecursive("vehicle_FrontArmor");
      if (squad_FrontArmor != null) { squad_FrontArmor.gameObject.SetActive(false); }
      if (vehicle_FrontArmor != null) { vehicle_FrontArmor.gameObject.SetActive(false); }
      if (mech_FrontArmor != null) { mech_FrontArmor.gameObject.SetActive(true); }
      if (mech_RearArmor != null) { mech_RearArmor.gameObject.SetActive(true); }
      for (int index = 0; index < BaySquadReadoutAligner.READOUT_NAMES.Count; ++index) {
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.MECH_READOUT_NAMES[index];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.MECH_READOUT_NAMES[index] + BaySquadReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + BaySquadReadoutAligner.MECH_READOUT_NAMES[index];
        if (mech_FrontArmor == null) { continue; }
        RectTransform MechTray_Armor = mech_FrontArmor.FindRecursive(armorName) as RectTransform;
        RectTransform MechTray_ArmorOutline = mech_FrontArmor.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform MechTray_Structure = mech_FrontArmor.FindRecursive(structureName) as RectTransform;
        if (MechTray_Armor != null) readout.Armor[index] = MechTray_Armor.gameObject.GetComponent<SVGImage>();
        if (MechTray_ArmorOutline != null) readout.ArmorOutline[index] = MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>();
        if (MechTray_Structure != null) readout.Structure[index] = MechTray_Structure.gameObject.GetComponent<SVGImage>();
        if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(true);
        if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(true);
        if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(true);
        if (def != null) {
          LocationDef locationDef = def.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true)));
          float armor = locationDef.MaxArmor;
          float structure = locationDef.InternalStructure;
          if ((armor <= CustomAmmoCategories.Epsilon) && (structure <= 1f)) {
            if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(false);
            if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(false);
            if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(false);
          }
        }
      }
    }
    public static void BayShowVehicle(this HUDMechArmorReadout readout, ChassisDef def) {
      Log.TWL(0, "HUDMechArmorReadout.BayShowVehicle " + "readout:" + readout.GetInstanceID() +" " + (def == null ? "null" : def.Description.Id));
      Transform mech_FrontArmor = readout.gameObject.transform.FindRecursive("mech_FrontArmor");
      Transform mech_RearArmor = readout.gameObject.transform.FindRecursive("mech_RearArmor");
      Transform squad_FrontArmor = readout.gameObject.transform.FindRecursive("squad_FrontArmor");
      Transform vehicle_FrontArmor = readout.gameObject.transform.FindRecursive("vehicle_FrontArmor");
      if (squad_FrontArmor != null) { squad_FrontArmor.gameObject.SetActive(false); }
      if (vehicle_FrontArmor != null) { vehicle_FrontArmor.gameObject.SetActive(true); Log.WL(1, "vehicle_FrontArmor:" + squad_FrontArmor.gameObject.activeSelf); }
      if (mech_FrontArmor != null) { mech_FrontArmor.gameObject.SetActive(false); Log.WL(1, "mech_FrontArmor:" + squad_FrontArmor.gameObject.activeSelf); }
      if (mech_RearArmor != null) { mech_RearArmor.gameObject.SetActive(false); Log.WL(1, "mech_RearArmor:" + squad_FrontArmor.gameObject.activeSelf); }
      Log.WL(1, "squad_FrontArmor:" +  (squad_FrontArmor == null?"null":squad_FrontArmor.gameObject.activeSelf.ToString()));
      Log.WL(1, "vehicle_FrontArmor:" + (vehicle_FrontArmor == null ? "null" : vehicle_FrontArmor.gameObject.activeSelf.ToString()));
      Log.WL(1, "mech_FrontArmor:" + (squad_FrontArmor == null ? "null" : mech_FrontArmor.gameObject.activeSelf.ToString()));
      Log.WL(1, "mech_RearArmor:" + (squad_FrontArmor == null ? "null" : mech_RearArmor.gameObject.activeSelf.ToString()));
      for (int index = 0; index < BaySquadReadoutAligner.READOUT_NAMES.Count; ++index) {
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + BayVehicleReadoutAligner.READOUT_NAMES[index];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + BayVehicleReadoutAligner.READOUT_NAMES[index] + BayVehicleReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + BayVehicleReadoutAligner.READOUT_NAMES[index];
        if (vehicle_FrontArmor == null) { continue; }
        RectTransform MechTray_Armor = vehicle_FrontArmor.FindRecursive(armorName) as RectTransform;
        RectTransform MechTray_ArmorOutline = vehicle_FrontArmor.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform MechTray_Structure = vehicle_FrontArmor.FindRecursive(structureName) as RectTransform;
        if (MechTray_Armor != null) readout.Armor[index] = MechTray_Armor.gameObject.GetComponent<SVGImage>();
        if (MechTray_ArmorOutline != null) readout.ArmorOutline[index] = MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>();
        if (MechTray_Structure != null) readout.Structure[index] = MechTray_Structure.gameObject.GetComponent<SVGImage>();
        if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(true);
        if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(true);
        if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(true);
        if (def != null) {
          LocationDef locationDef = def.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true)));
          float armor = locationDef.MaxArmor;
          float structure = locationDef.InternalStructure;
          if ((armor <= CustomAmmoCategories.Epsilon) && (structure <= 1f)) {
            if (MechTray_Armor != null) MechTray_Armor.gameObject.SetActive(false);
            if (MechTray_ArmorOutline != null) MechTray_ArmorOutline.gameObject.SetActive(false);
            if (MechTray_Structure != null) MechTray_Structure.gameObject.SetActive(false);
          }
        }
      }
    }
    public static void Prefix(HUDMechArmorReadout __instance, MechDef value) {
      try {
        if (value == null) {
          __instance.BayShowMech(null);
          return;
        }
        UnitCustomInfo info = value.GetCustomInfo();
        Log.TWL(0, "HUDMechArmorReadout.DisplayedMechDef " + value.Description.Id+" chassis:"+value.ChassisID+" isFake:"+ value.IsVehicle()+" info:"+(info == null?"null":info.FakeVehicle.ToString()));
        if (value.IsVehicle()) {
          __instance.BayShowVehicle(value.Chassis);
          return;
        }
        if (info == null) {
          __instance.BayShowMech(value.Chassis);
          return;
        }
        if (info.FakeVehicle) {
          __instance.BayShowVehicle(value.Chassis);
          return;
        }
        if (info.SquadInfo.Troopers <= 1) {
          __instance.BayShowMech(value.Chassis);
          return;
        }
        __instance.BayShowSquad(value.Chassis);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedChassisDef")]
  //[HarmonyPatch(new Type[] { typeof(ChassisDef) })]
  public static class HUDMechArmorReadout_DisplayedChassisDef {
    public static void Prefix(HUDMechArmorReadout __instance, ChassisDef value) {
      try {
        if (value == null) {
          __instance.BayShowMech(value);
          return;
        }
        if (value.Description.Id.IsInFakeChassis()) {
          __instance.BayShowVehicle(value);
          return;
        }
        UnitCustomInfo info = value.GetCustomInfo();
        if (info == null) {
          __instance.BayShowMech(value);
          return;
        }
        if (info.FakeVehicle) {
          __instance.BayShowVehicle(value);
          return;
        }
        if (info.SquadInfo.Troopers <= 1) {
          __instance.BayShowMech(value);
          return;
        }
        __instance.BayShowSquad(value);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class BaySquadReadoutAligner : MonoBehaviour {
    private bool ui_inited = false;
    private bool svg_inited = false;
    private bool armor_outline_child = true;
    private HUDMechArmorReadout parent;
    private RectTransform frontArmor;
    public void ResetUI() {
      ui_inited = false;
    }
    public void Init(HUDMechArmorReadout readout, RectTransform frontArmor, bool armor_outline_child) {
      parent = readout;
      this.armor_outline_child = armor_outline_child;
      this.frontArmor = frontArmor;
    }
    public static readonly float SQUAD_ICON_SIZE = 45f;
    public static readonly string OUTLINE_SUFFIX = "Outline";
    public static readonly string ARMOR_PREFIX = "MechTray_Armor";
    public static readonly string STRUCTURE_PREFIX = "Mech_TrayInternal";
    public static readonly List<string> READOUT_NAMES = new List<string>() { "Head", "Torso", "RT", "LT", "RA", "LA", "LL", "RL" };
    public static readonly List<string> MECH_READOUT_NAMES = new List<string>() { "Head", "LA", "LT", "Torso", "RT", "RA", "LL", "RL" };
    public static readonly Dictionary<ArmorLocation, int> ARMOR_TO_SQUAD = new Dictionary<ArmorLocation, int>() {
      { ArmorLocation.Head, 0 },
      { ArmorLocation.CenterTorso, 1 },
      { ArmorLocation.LeftTorso, 2 },
      { ArmorLocation.RightTorso, 3 },
      { ArmorLocation.LeftArm, 4 },
      { ArmorLocation.RightArm, 5 },
      { ArmorLocation.LeftLeg, 6 },
      { ArmorLocation.RightLeg, 7 } };
    public static readonly Dictionary<ChassisLocations, int> STRUCTURE_TO_SQUAD = new Dictionary<ChassisLocations, int>() {
      { ChassisLocations.Head, 0 },
      { ChassisLocations.CenterTorso, 1 },
      { ChassisLocations.LeftTorso, 2 },
      { ChassisLocations.RightTorso, 3 },
      { ChassisLocations.LeftArm, 4 },
      { ChassisLocations.RightArm, 5 },
      { ChassisLocations.LeftLeg, 6 },
      { ChassisLocations.RightLeg, 7 } };
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() {
      { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };
    private void SVGInit() {
      try {
        ChassisDef chassisDef = null;
        if (this.parent.DisplayedMech != null) { chassisDef = this.parent.DisplayedMech.MechDef.Chassis; } else
        if (this.parent.DisplayedMechDef != null) { chassisDef = this.parent.DisplayedMechDef.Chassis; } else
        if (this.parent.DisplayedChassisDef != null) { chassisDef = this.parent.DisplayedChassisDef; }
        Log.TWL(0, $"SquadReadoutAligner.SVGInit {(chassisDef == null?"null":chassisDef.Description.Id)}");
        UnitCustomInfo info = null;
        if (chassisDef != null) { info = chassisDef.GetCustomInfo(); }
        for (int index = 0; index < READOUT_NAMES.Count; ++index) {
          int squadIndex = READOUT_INDEX_TO_SQUAD[index];
          string armorName = ARMOR_PREFIX + READOUT_NAMES[squadIndex];
          string armorOutlineName = ARMOR_PREFIX + READOUT_NAMES[squadIndex] + OUTLINE_SUFFIX;
          string structureName = STRUCTURE_PREFIX + READOUT_NAMES[squadIndex];
          RectTransform MechTray_Armor = this.gameObject.transform.FindRecursive(armorName) as RectTransform;
          RectTransform MechTray_ArmorOutline = this.gameObject.transform.FindRecursive(armorOutlineName) as RectTransform;
          RectTransform Mech_TrayInternal = this.gameObject.transform.FindRecursive(structureName) as RectTransform;
          Log.W(1, armorName + ":" + (MechTray_Armor == null ? "null" : "not null") + " " + armorOutlineName + ":" + (MechTray_ArmorOutline == null ? "null" : "not null") + " " + structureName + ":" + (Mech_TrayInternal == null ? "null" : "not null"));
          int row = (int)squadIndex / (int)2;
          Log.WL(1, "row:" + row + " col:" + (squadIndex % 2) + " squadIndex:" + squadIndex + " al:" + HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, false) + " sl:" + HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, false));
          if (MechTray_Armor != null) {
            if ((info == null) || (string.IsNullOrEmpty(info.SquadInfo.armorIcon))) {
              MechTray_Armor.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              MechTray_Armor.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(info.SquadInfo.armorIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
            MechTray_Armor.pivot = new Vector2((squadIndex % 2 == 0 ? 0f : -1.1f), 1f + 1.1f * ((float)(row)));
            MechTray_Armor.anchoredPosition = Vector2.zero; MechTray_Armor.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE); MechTray_Armor.localScale = Vector3.one;
          }
          if (MechTray_ArmorOutline != null) {
            if ((info == null) || (string.IsNullOrEmpty(info.SquadInfo.outlineIcon))) {
              MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(info.SquadInfo.outlineIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
            MechTray_ArmorOutline.pivot = new Vector2(0f, 1f);
            MechTray_ArmorOutline.anchoredPosition = Vector2.zero; MechTray_ArmorOutline.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE);
          }
          if (Mech_TrayInternal != null) {
            if ((info == null) || (string.IsNullOrEmpty(info.SquadInfo.structureIcon))) {
              Mech_TrayInternal.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
            } else {
              Mech_TrayInternal.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(info.SquadInfo.structureIcon, UnityGameInstance.BattleTechGame.DataManager);
            }
            Mech_TrayInternal.pivot = MechTray_Armor.pivot;
            Mech_TrayInternal.anchoredPosition = Vector2.zero; Mech_TrayInternal.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE); Mech_TrayInternal.localScale = Vector3.one;
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      svg_inited = true;
    }
    public void UIInit() {
      this.transform.localPosition = frontArmor.localPosition;

      ui_inited = true;
    }
    public void Update() {
      if (svg_inited == false) { SVGInit(); }
      if (ui_inited == false) { UIInit(); }
    }
  }
  public class BayVehicleReadoutAligner : MonoBehaviour {
    private bool ui_inited = false;
    private bool svg_inited = false;
    private HUDMechArmorReadout parent;
    private RectTransform frontArmor;
    public void ResetUI() {
      ui_inited = false;
    }
    public void Init(HUDMechArmorReadout readout, RectTransform frontArmor) {
      parent = readout;
      this.frontArmor = frontArmor;
    }
    public static readonly float SQUAD_ICON_SIZE = 45f;
    public static readonly string OUTLINE_SUFFIX = "Outline";
    public static readonly string ARMOR_PREFIX = "MechTray_Armor";
    public static readonly string STRUCTURE_PREFIX = "Mech_TrayInternal";
    public static readonly List<string> READOUT_NAMES = new List<string>() { "Head", "RA", "RT", "Torso", "LT", "LA", "RL", "LL" };
    public static readonly List<VehicleChassisLocations> READOUT_LOCATIONS = new List<VehicleChassisLocations>() {
      VehicleChassisLocations.Turret, VehicleChassisLocations.Front, VehicleChassisLocations.None,
      VehicleChassisLocations.None, VehicleChassisLocations.None, VehicleChassisLocations.Rear, VehicleChassisLocations.Left, VehicleChassisLocations.Right };
    public static readonly List<Vector2> ArmorIconSizes = new List<Vector2>() {
      new Vector2(20f,50f), new Vector2(40f,20f), new Vector2(0f,0f), new Vector2(0f,0f),
      new Vector2(0f,0f), new Vector2(40f,20f), new Vector2(15f,40f), new Vector2(15f,40f)
    };
    public static readonly List<Vector2> ArmorIconPivot = new List<Vector2>() {
      new Vector2(-0.8f,1.2f), new Vector2(-0.15f,1.15f), new Vector2(0f,0f), new Vector2(0f,0f),
      new Vector2(0f,0f), new Vector2(-0.17f,3.7f), new Vector2(-0.2f,1.45f), new Vector2(-2.3f,1.45f)
    };
    public static readonly List<Vector2> StructureIconSizes = new List<Vector2>() {
      new Vector2(20f,50f), new Vector2(40f,18f), new Vector2(0f,0f), new Vector2(0f,0f),
      new Vector2(0f,0f), new Vector2(40f,18f), new Vector2(15f,30f), new Vector2(15f,35f)
    };
    public static readonly List<Vector2> StructureIconPivot = new List<Vector2>() {
      new Vector2(-0.8f,1.2f), new Vector2(-0.15f,1.15f), new Vector2(0f,0f), new Vector2(0f,0f),
      new Vector2(0f,0f), new Vector2(-0.17f,4.1f), new Vector2(-0.2f,1.77f), new Vector2(-2.3f,1.6f)
    };
    public static string ReadoutIndexToArmorIconName(int index) {
      switch (READOUT_LOCATIONS[index]) {
        case VehicleChassisLocations.Turret: return Core.Settings.VehicleTurretArmorIcon;
        case VehicleChassisLocations.Front: return Core.Settings.VehicleFrontArmorIcon;
        case VehicleChassisLocations.Rear: return Core.Settings.VehicleRearArmorIcon;
        case VehicleChassisLocations.Left: return Core.Settings.VehicleLeftArmorIcon;
        case VehicleChassisLocations.Right: return Core.Settings.VehicleRightArmorIcon;
      }
      return null;
    }
    public static string ReadoutIndexToArmorOutlineIconName(int index) {
      switch (READOUT_LOCATIONS[index]) {
        case VehicleChassisLocations.Turret: return Core.Settings.VehicleTurretArmorOutlineIcon;
        case VehicleChassisLocations.Front: return Core.Settings.VehicleFrontArmorOutlineIcon;
        case VehicleChassisLocations.Rear: return Core.Settings.VehicleRearArmorOutlineIcon;
        case VehicleChassisLocations.Left: return Core.Settings.VehicleLeftArmorOutlineIcon;
        case VehicleChassisLocations.Right: return Core.Settings.VehicleRightArmorOutlineIcon;
      }
      return null;
    }
    public static string ReadoutIndexToStructureIconName(int index) {
      switch (READOUT_LOCATIONS[index]) {
        case VehicleChassisLocations.Turret: return Core.Settings.VehicleTurretStructureIcon;
        case VehicleChassisLocations.Front: return Core.Settings.VehicleFrontStructureIcon;
        case VehicleChassisLocations.Rear: return Core.Settings.VehicleRearStructureIcon;
        case VehicleChassisLocations.Left: return Core.Settings.VehicleLeftStructureIcon;
        case VehicleChassisLocations.Right: return Core.Settings.VehicleRightStructureIcon;
      }
      return null;
    }
    private void SVGInit() {
      try {
        for (int index = 0; index < READOUT_NAMES.Count; ++index) {
          string armorName = ARMOR_PREFIX + READOUT_NAMES[index];
          string armorOutlineName = ARMOR_PREFIX + READOUT_NAMES[index] + OUTLINE_SUFFIX;
          string structureName = STRUCTURE_PREFIX + READOUT_NAMES[index];
          RectTransform MechTray_Armor = this.gameObject.transform.FindRecursive(armorName) as RectTransform;
          string armorIconName = ReadoutIndexToArmorIconName(index);
          if (string.IsNullOrEmpty(armorIconName)) {
            MechTray_Armor.gameObject.SetActive(false);
          } else {
            MechTray_Armor.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(armorIconName, UnityGameInstance.BattleTechGame.DataManager);
            MechTray_Armor.pivot = ArmorIconPivot[index];
            MechTray_Armor.anchoredPosition = Vector2.zero; MechTray_Armor.sizeDelta = ArmorIconSizes[index];
          }
          string armorOutlineIconName = ReadoutIndexToArmorOutlineIconName(index);
          RectTransform MechTray_ArmorOutline = this.gameObject.transform.FindRecursive(armorOutlineName) as RectTransform;
          if (string.IsNullOrEmpty(armorOutlineIconName)) {
            MechTray_ArmorOutline.gameObject.SetActive(false);
          } else {
            MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(armorOutlineIconName, UnityGameInstance.BattleTechGame.DataManager);
            MechTray_ArmorOutline.pivot = new Vector2(0f, 1f);
            MechTray_ArmorOutline.anchoredPosition = Vector2.zero; MechTray_ArmorOutline.sizeDelta = ArmorIconSizes[index];
          }
          string structureIconName = ReadoutIndexToStructureIconName(index);
          RectTransform Mech_TrayInternal = this.gameObject.transform.FindRecursive(structureName) as RectTransform;
          if (string.IsNullOrEmpty(structureIconName)) {
            Mech_TrayInternal.gameObject.SetActive(false);
          } else {
            Mech_TrayInternal.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(structureIconName, UnityGameInstance.BattleTechGame.DataManager);
            Mech_TrayInternal.pivot = StructureIconPivot[index];
            Mech_TrayInternal.anchoredPosition = Vector2.zero; Mech_TrayInternal.sizeDelta = StructureIconSizes[index];
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      svg_inited = true;
    }
    public void UIInit() {
      if (frontArmor == null) { return; }
      this.transform.localPosition = frontArmor.localPosition;

      ui_inited = true;
    }
    public void Update() {
      if (svg_inited == false) { SVGInit(); }
      if (ui_inited == false) { UIInit(); }
    }
  }
  public class MechTrayPaperdollController : MonoBehaviour {
    public GameObject MechPaperDoll { get; set; }
    public HUDVehicleArmorReadout VehicleArmorDisplay { get; set; }
    public HUDFakeVehicleArmorReadout FakeVehicleArmorDisplay { get; set; }
    public CombatHUD HUD { get; set; }
    public void InstantineVehiclePaperDoll(CombatHUD HUD, HUDVehicleArmorReadout source) {
      List<CombatHUDVehicleArmorHover> result = new List<CombatHUDVehicleArmorHover>();
      source.GetComponentsInChildren<CombatHUDVehicleArmorHover>(true, result);
      Log.WL(1, "parent vehicleSlots:" + result.Count);
      for (int index = 0; index < result.Count; ++index) {
        CombatHUDVehicleArmorHover armorSlot = result[index];
        CombatHUDTooltipHoverElement ToolTip = armorSlot.GetComponent<CombatHUDTooltipHoverElement>();
        if (ToolTip == null) { continue; }
        Log.WL(2, "[" + index + "] toolTip:" + ToolTip.GetInstanceID() + " hoverpanel:" + (ToolTip.ToolTip == null ? "null" : ToolTip.ToolTip.GetInstanceID().ToString()));
      }
      GameObject vehiclePaperDoll = GameObject.Instantiate(source.gameObject);
      vehiclePaperDoll.name = "MechTray(vehicle)";
      vehiclePaperDoll.transform.SetParent(HUD.MechTray.MechArmorDisplay.gameObject.transform);
      vehiclePaperDoll.transform.localScale = Vector3.one * 1.2f;
      GameObject MechTray_ArmorTorso = MechPaperDoll.transform.FindRecursive("MechTray_ArmorTorso").gameObject;
      GameObject MechTray_ArmorLL = MechPaperDoll.transform.FindRecursive("MechTray_ArmorLL").gameObject;
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
      VehicleArmorDisplay = vehiclePaperDoll.GetComponent<HUDVehicleArmorReadout>();
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
        if (ToolTip == null) { continue; }
        ToolTip.DisplayPosition = DisplayPosition;
        ToolTip.Orientation = CombatHUDTooltipHoverElement.ToolTipOrientation.Up;
        ToolTip.DisplayOffset = DisplayOffset;
        Log.WL(2, "[" + index + "] toolTip:" + ToolTip.GetInstanceID() + " hoverpanel:" + (ToolTip.ToolTip == null ? "null" : ToolTip.ToolTip.GetInstanceID().ToString()));
      }
      VehicleArmorDisplay.Init(HUD, false);
      VehicleArmorDisplay.ArmorBar = HUD.MechTray.MechArmorDisplay.ArmorBar;
      VehicleArmorDisplay.StructureBar = HUD.MechTray.MechArmorDisplay.StructureBar;
      VehicleArmorDisplay.HoverInfoTextArmor = HUD.MechTray.MechArmorDisplay.HoverInfoTextArmor;
      VehicleArmorDisplay.HoverInfoTextStructure = HUD.MechTray.MechArmorDisplay.HoverInfoTextStructure;
    }
    public void Instantine(CombatHUD HUD, HUDVehicleArmorReadout source, bool usedForCalledShots) {
      this.HUD = HUD;
      MechPaperDoll = HUD.MechTray.MechArmorDisplay.gameObject.transform.Find("MechPaperDoll").gameObject;
      InstantineVehiclePaperDoll(HUD,source);
    }
    public void Init(CombatHUD HUD) {
      this.HUD = HUD;
      MechPaperDoll = HUD.MechTray.MechArmorDisplay.gameObject.transform.Find("MechPaperDoll").gameObject;
      VehicleArmorDisplay = HUD.MechTray.MechArmorDisplay.GetComponentInChildren<HUDVehicleArmorReadout>(true);
      FakeVehicleArmorDisplay = HUD.MechTray.MechArmorDisplay.GetComponentInChildren<HUDFakeVehicleArmorReadout>(true);
      FakeVehicleArmorDisplay.ArmorBar = VehicleArmorDisplay.ArmorBar;
      FakeVehicleArmorDisplay.StructureBar = VehicleArmorDisplay.StructureBar;
      FakeVehicleArmorDisplay.HoverInfoTextArmor = VehicleArmorDisplay.HoverInfoTextArmor;
      FakeVehicleArmorDisplay.HoverInfoTextStructure = VehicleArmorDisplay.HoverInfoTextStructure;
    }
  };
  [HarmonyPatch(typeof(HUDVehicleArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class HUDVehicleArmorReadout_Init {
    public static HUDVehicleArmorReadout VehicleArmorDisplay(this CombatHUDMechTray tray) {
      MechTrayPaperdollController paperdollController = tray.gameObject.GetComponent<MechTrayPaperdollController>();
      if (paperdollController == null) { return null; }
      return paperdollController.VehicleArmorDisplay;
    }
    public static HUDFakeVehicleArmorReadout FakeVehicleArmorDisplay(this CombatHUDMechTray tray) {
      MechTrayPaperdollController paperdollController = tray.gameObject.GetComponent<MechTrayPaperdollController>();
      if (paperdollController == null) { return null; }
      return paperdollController.FakeVehicleArmorDisplay;
    }
    public static GameObject MechPaperDoll(this CombatHUDMechTray tray) {
      MechTrayPaperdollController paperdollController = tray.gameObject.GetComponent<MechTrayPaperdollController>();
      if (paperdollController == null) { return null; }
      return paperdollController.MechPaperDoll;
    }
    public static void Postfix(HUDVehicleArmorReadout __instance, CombatHUD HUD, bool usedForCalledShots) {
      Log.TWL(0, "HUDVehicleArmorReadout.Init gameObject:" + __instance.gameObject.name + " parent:" + __instance.gameObject.transform.parent.gameObject.name);
      HUDFakeVehicleArmorReadout fakeVehicleArmorReadout = __instance.gameObject.transform.parent.gameObject.GetComponentInChildren<HUDFakeVehicleArmorReadout>(true);
      if (fakeVehicleArmorReadout == null) {
        GameObject fakeVehicleArmorReadoutGO = GameObject.Instantiate(__instance.gameObject);
        fakeVehicleArmorReadoutGO.name = __instance.gameObject.name + "_fake";
        fakeVehicleArmorReadoutGO.transform.SetParent(__instance.gameObject.transform.parent);
        fakeVehicleArmorReadoutGO.transform.localPosition = __instance.gameObject.transform.localPosition;
        fakeVehicleArmorReadoutGO.transform.localScale = __instance.gameObject.transform.localScale;
        fakeVehicleArmorReadoutGO.transform.localRotation = __instance.gameObject.transform.localRotation;
        fakeVehicleArmorReadout = fakeVehicleArmorReadoutGO.AddComponent<HUDFakeVehicleArmorReadout>();
        fakeVehicleArmorReadoutGO.SetActive(false);
        HUDVehicleArmorReadout VehicleArmorReadout = fakeVehicleArmorReadoutGO.GetComponent<HUDVehicleArmorReadout>();
        fakeVehicleArmorReadout.Copy(__instance, VehicleArmorReadout);
        if (VehicleArmorReadout != null) { GameObject.Destroy(VehicleArmorReadout); }
      }
      if (fakeVehicleArmorReadout != null) { fakeVehicleArmorReadout.Init(HUD, usedForCalledShots); }
      if (usedForCalledShots == false) {
        MechTrayPaperdollController paperdollController = HUD.MechTray.gameObject.GetComponent<MechTrayPaperdollController>();
        if (paperdollController == null) {
          paperdollController = HUD.MechTray.gameObject.AddComponent<MechTrayPaperdollController>();
          paperdollController.Instantine(HUD, __instance, usedForCalledShots);
        }
        paperdollController.Init(HUD);
      }
    }
  }
  [HarmonyPatch(typeof(GameRepresentation))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("SetHighlightAlpha")]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class GameRepresentation_SetHighlightAlpha {
    public static void Prefix(GameRepresentation __instance, float alpha) {
      //Log.TWL(0, "GameRepresentation.SetHighlightAlpha "+(__instance == null?"null": __instance.name)+" unit:"+(__instance.parentCombatant == null?"null": __instance.parentCombatant.DisplayName) +" alpha:"+alpha);
    }
  }
}