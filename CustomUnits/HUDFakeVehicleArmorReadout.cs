﻿using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS;
using Localize;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CustomUnits {
  public class CombatHUDFakeVehicleArmorHover : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler {
    public ChassisLocations ArmorLocation;
    private bool usedForCalledShots;
    private HUDFakeVehicleArmorReadout Readout { get; set; }
    private CombatHUDTooltipHoverElement ToolTip { get; set; }
    public void Copy(CombatHUDVehicleArmorHover src) {
      switch (src.ArmorLocation) {
        case VehicleChassisLocations.Turret: this.ArmorLocation = ChassisLocations.Head; break;
        case VehicleChassisLocations.Front: this.ArmorLocation = ChassisLocations.LeftArm; break;
        case VehicleChassisLocations.Rear: this.ArmorLocation = ChassisLocations.RightArm; break;
        case VehicleChassisLocations.Left: this.ArmorLocation = ChassisLocations.LeftLeg; break;
        case VehicleChassisLocations.Right: this.ArmorLocation = ChassisLocations.RightLeg; break;
      }
    }
    public void Init(CombatHUD HUD, HUDFakeVehicleArmorReadout Readout, bool usedForCalledShots) {
      this.Readout = Readout;
      this.usedForCalledShots = usedForCalledShots;
      this.ToolTip = this.GetComponent<CombatHUDTooltipHoverElement>();
      if (!((UnityEngine.Object)this.ToolTip != (UnityEngine.Object)null))
        return;
      this.ToolTip.Init(HUD);
    }
    private void setToolTipInfo(Mech vehicle, ChassisLocations location) {
      this.ToolTip.BuffStrings.Clear();
      this.ToolTip.DebuffStrings.Clear();
      this.ToolTip.BasicString = new Localize.Text(Vehicle.GetLongChassisLocation(vehicleLocationFromMechLocation(location)), (object[])Array.Empty<object>());
      for (int index = 0; index < vehicle.allComponents.Count; ++index) {
        MechComponent allComponent = vehicle.allComponents[index];
        if ((ChassisLocations)allComponent.Location == location) {
          Weapon weapon = allComponent as Weapon;
          if (allComponent.DamageLevel < ComponentDamageLevel.NonFunctional && (weapon == null || weapon.HasAmmo))
            this.ToolTip.BuffStrings.Add(allComponent.UIName);
          else
            this.ToolTip.DebuffStrings.Add(allComponent.UIName);
        }
      }
    }

    public void OnPointerEnter(PointerEventData data) {
      if (this.Readout.DisplayedVehicle == null)
        return;
      if ((UnityEngine.Object)this.ToolTip != (UnityEngine.Object)null)
        this.setToolTipInfo(this.Readout.DisplayedVehicle, this.ArmorLocation);
      this.Readout.SetHoveredArmor(this.ArmorLocation);
    }

    public void OnPointerExit(PointerEventData data) => this.Readout.ClearHoveredArmor(this.ArmorLocation);

    public void OnPointerClick(PointerEventData data) {
      if (!this.usedForCalledShots || !(this.Readout.HUD.SelectionHandler.ActiveState is SelectionStateFire activeState) || (!activeState.NeedsCalledShot || !this.Readout.gameObject.activeSelf) || (this.ArmorLocation == ChassisLocations.None || !(activeState.TargetedCombatant is Mech targetedCombatant) || targetedCombatant.IsLocationDestroyed(this.ArmorLocation)))
        return;
      Dictionary<VehicleChassisLocations, int> vehicleHitTable = this.Readout.HUD.Combat.HitLocation.GetVehicleHitTable(this.Readout.HUD.Combat.HitLocation.GetAttackDirection(this.Readout.HUD.SelectedActor, (ICombatant)this.Readout.DisplayedVehicle), false);
      if (!vehicleHitTable.ContainsKey(vehicleLocationFromMechLocation(this.ArmorLocation)) || vehicleHitTable[vehicleLocationFromMechLocation(this.ArmorLocation)] == 0)
        return;
      activeState.SetCalledShot(vehicleLocationFromMechLocation(this.ArmorLocation));
    }
  }
  public class HUDFakeVehicleArmorReadout : HUDArmorReadout {
    public float hiddenColorLerp = 0.8f;
    public SVGImage[] VStructure;
    public SVGImage[] VArmor;
    public SVGImage[] VArmorOutline;
    private Mech displayedVehicle;
    public LocalizableText HoverInfoTextArmor;
    public LocalizableText HoverInfoTextStructure;
    public HBSButton directionalIndicatorLeft;
    public HBSButton directionalIndicatorRight;
    public HBSButton directionalIndicatorFront;
    public HBSButton directionalIndicatorBack;

    public CombatHUD HUD { get; private set; }

    private Color[] vStructureCached { get; set; }

    private float[] timeSinceVStructureDamaged { get; set; }

    private Color[] vArmorCached { get; set; }

    private float[] timeSinceVArmorDamaged { get; set; }

    private Color[] vArmorOutlineCached { get; set; }

    public Mech DisplayedVehicle {
      get => this.displayedVehicle;
      set {
        if (this.displayedVehicle != value) {
          this.displayedVehicle = value;
          this.ResetArmorStructureBars();
          this.ResetDamageTimes();
        }
        if (this.displayedVehicle == null)
          return;
        this.RefreshVehicleStructureAndArmor();
        this.RefreshHoverInfo();
      }
    }

    public ChassisLocations HoveredArmor { get; private set; }

    public bool UseForCalledShots { get; private set; }
    public string GetName(Transform img, Transform source) {
      StringBuilder result = new StringBuilder();
      Transform curTR = img;
      while((curTR != source)&&(curTR != null)) {
        result.Append(curTR.name+".");
        curTR = curTR.parent;
      }
      return result.ToString();
    }
    public void Copy(HUDVehicleArmorReadout source) {
      this.ArmorBar = source.ArmorBar;
      this.StructureBar = source.StructureBar;
      this.HoverInfoTextArmor = source.HoverInfoTextArmor;
      this.HoverInfoTextStructure = source.HoverInfoTextStructure;
      this.VStructure = new SVGImage[source.VStructure.Length];
      this.VArmor = new SVGImage[source.VArmor.Length];
      this.VArmorOutline = new SVGImage[source.VArmorOutline.Length];
      Log.TWL(0, "HUDFakeVehicleArmorReadout.Copy "+source.gameObject.name);
      try {
        HBSButton[] buttons = this.GetComponentsInChildren<HBSButton>(true);
        Dictionary<string, HBSButton> btns = new Dictionary<string, HBSButton>();
        foreach (HBSButton btn in buttons) { string name = this.GetName(btn.transform,this.transform); btns.Add(name, btn); Log.WL(1, name + ":" + btn.name); }
        if (source.directionalIndicatorFront != null) {
          if (btns.ContainsKey(source.directionalIndicatorFront.gameObject.name)) { this.directionalIndicatorFront = btns[source.directionalIndicatorFront.gameObject.name]; }
        }
        if (source.directionalIndicatorBack != null) {
          if (btns.ContainsKey(source.directionalIndicatorBack.gameObject.name)) { this.directionalIndicatorBack = btns[source.directionalIndicatorBack.gameObject.name]; }
        }
        if (source.directionalIndicatorLeft != null) {
          if (btns.ContainsKey(source.directionalIndicatorLeft.gameObject.name)) { this.directionalIndicatorLeft = btns[source.directionalIndicatorLeft.gameObject.name]; }
        }
        if (source.directionalIndicatorRight != null) {
          if (btns.ContainsKey(source.directionalIndicatorRight.gameObject.name)) { this.directionalIndicatorRight = btns[source.directionalIndicatorRight.gameObject.name]; }
        }

        SVGImage[] simgs = this.GetComponentsInChildren<SVGImage>(true);
        Dictionary<string, SVGImage> images = new Dictionary<string, SVGImage>();
        foreach (SVGImage img in simgs) { string name = this.GetName(img.transform,this.transform); images.Add(name, img); Log.WL(1,name+":"+img.name); }
        Log.WL(1, "VStructure");
        for (int index = 0; index < source.VStructure.Length; ++index) {
          if (source.VStructure[index] == null) { this.VStructure[index] = null; continue; }
          string name = this.GetName(source.VStructure[index].transform, source.transform);
          Log.WL(2, name+" exists:"+ images.ContainsKey(name));
          if (images.ContainsKey(name)) { this.VStructure[index] = images[name]; }
        }
        Log.WL(1, "VArmor");
        for (int index = 0; index < source.VArmor.Length; ++index) {
          if (source.VArmor[index] == null) { this.VArmor[index] = null; continue; }
          string name = this.GetName(source.VArmor[index].transform, source.transform);
          Log.WL(2, name + " exists:" + images.ContainsKey(name));
          if (images.ContainsKey(name)) { this.VArmor[index] = images[name]; }
        }
        Log.WL(1, "VArmorOutline");
        for (int index = 0; index < source.VArmorOutline.Length; ++index) {
          if (source.VArmorOutline[index] == null) { this.VArmorOutline[index] = null; continue; }
          string name = this.GetName(source.VArmorOutline[index].transform, source.transform);
          Log.WL(2, name + " exists:" + images.ContainsKey(name));
          if (images.ContainsKey(name)) { this.VArmorOutline[index] = images[name]; }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public void Init(CombatHUD HUD, bool usedForCalledShots) {
      this.HUD = HUD;
      this.UseForCalledShots = usedForCalledShots;
      this.vArmorCached = new Color[5];
      this.vArmorOutlineCached = new Color[5];
      this.vStructureCached = new Color[5];
      this.timeSinceVArmorDamaged = new float[5];
      this.timeSinceVStructureDamaged = new float[5];
      List<CombatHUDVehicleArmorHover> sources = new List<CombatHUDVehicleArmorHover>();
      this.GetComponentsInChildren<CombatHUDVehicleArmorHover>(true, sources);
      foreach (CombatHUDVehicleArmorHover source in sources) {
        CombatHUDFakeVehicleArmorHover res = source.gameObject.GetComponent<CombatHUDFakeVehicleArmorHover>();
        if (res != null) { GameObject.Destroy(source); continue; }
        res = source.gameObject.AddComponent<CombatHUDFakeVehicleArmorHover>();
        res.Copy(source);
        GameObject.Destroy(source);
      }
      List<CombatHUDFakeVehicleArmorHover> result = new List<CombatHUDFakeVehicleArmorHover>();
      this.GetComponentsInChildren<CombatHUDFakeVehicleArmorHover>(true, result);
      for (int index = 0; index < result.Count; ++index) {
        result[index].Init(HUD, this, usedForCalledShots);
      }
    }

    public void SetHoveredArmor(ChassisLocations location) {
      this.HoveredArmor = location;
      this.RefreshHoverInfo();
    }

    public void ClearHoveredArmor(ChassisLocations location) {
      if (this.HoveredArmor != location)
        return;
      this.HoveredArmor = ChassisLocations.None;
      this.RefreshHoverInfo();
    }

    private void RefreshHoverInfo() {
      if (!((Object)this.HoverInfoTextArmor != (Object)null) || !((Object)this.HoverInfoTextStructure != (Object)null) || (!((Object)this.ArmorBar != (Object)null) || !((Object)this.StructureBar != (Object)null)))
        return;
      Log.TWL(0, "HUDFakeVehicleArmorReadout.RefreshHoverInfo "+ this.HoveredArmor);
      if (this.HoveredArmor == ChassisLocations.None) {
        float input1 = this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Front)) 
          + this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Turret)) 
          + this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Left)) 
          + this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Right)) 
          + this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Rear));
        float input2 = this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Front))
          + this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Turret)) 
          + this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Left)) 
          + this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Right)) 
          + this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Rear));
        float input3 = this.displayedVehicle.GetMaxArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Front)) 
          + this.displayedVehicle.GetMaxArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Turret))
          + this.displayedVehicle.GetMaxArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Left)) 
          + this.displayedVehicle.GetMaxArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Right)) 
          + this.displayedVehicle.GetMaxArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(VehicleChassisLocations.Rear));
        float input4 = this.displayedVehicle.GetMaxStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Front)) 
          + this.displayedVehicle.GetMaxStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Turret)) 
          + this.displayedVehicle.GetMaxStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Left)) 
          + this.displayedVehicle.GetMaxStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Right)) 
          + this.displayedVehicle.GetMaxStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(VehicleChassisLocations.Rear));
        this.HoverInfoTextArmor.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(input1), (object)HUDMechArmorReadout.FormatForSummary(input3));
        this.HoverInfoTextStructure.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(input2), (object)HUDMechArmorReadout.FormatForSummary(input4));
        this.ResetArmorStructureBars();
      } else {
        float armorForVlocation = HUDFakeVehicleArmorReadout.GetInitialArmorForVLocation(this.displayedVehicle, this.HoveredArmor);
        float currentArmor = this.displayedVehicle.GetCurrentArmor(MechStructureRules.GetArmorFromChassisLocation(this.HoveredArmor));
        this.HoverInfoTextArmor.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(currentArmor), (object)HUDMechArmorReadout.FormatForSummary(armorForVlocation));
        this.ArmorBar.ShowNewComponent(currentArmor, armorForVlocation, false);
        float structureForVlocation = HUDFakeVehicleArmorReadout.getInitialStructureForVLocation(this.displayedVehicle, this.HoveredArmor);
        float currentStructure = this.displayedVehicle.GetCurrentStructure(this.HoveredArmor);
        this.HoverInfoTextStructure.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(currentStructure), (object)HUDMechArmorReadout.FormatForSummary(structureForVlocation));
        this.StructureBar.ShowNewComponent(currentStructure, structureForVlocation, (double)currentArmor < 1.0);
      }
      LowVisibilityAPIHelper.ObfuscateArmorAndStructText(this.displayedVehicle, this.HoverInfoTextArmor, this.HoverInfoTextStructure);
    }

    private void ResetArmorStructureBars() {
      if (this.displayedVehicle == null || !((Object)this.ArmorBar != (Object)null) || !((Object)this.StructureBar != (Object)null))
        return;
      this.ArmorBar.ShowNewSummary(this.displayedVehicle.SummaryArmorCurrent, this.displayedVehicle.SummaryArmorMax, false);
      this.StructureBar.ShowNewSummary(this.displayedVehicle.SummaryStructureCurrent, this.displayedVehicle.SummaryStructureMax, this.displayedVehicle.IsAnyStructureExposed);
    }

    private void UpdateArmorStructureBars() {
      if (this.displayedVehicle == null || !((Object)this.ArmorBar != (Object)null) || !((Object)this.StructureBar != (Object)null))
        return;
      this.ArmorBar.UpdateSummary(this.displayedVehicle.SummaryArmorCurrent, false);
      this.StructureBar.UpdateSummary(this.displayedVehicle.SummaryStructureCurrent, this.displayedVehicle.IsAnyStructureExposed);
    }

    private void ResetDamageTimes() {
      for (int index = 0; index < 5; ++index) {
        this.timeSinceVArmorDamaged[index] = 99.9f;
        this.timeSinceVStructureDamaged[index] = 99.9f;
      }
    }

    public void OnActorTakeDamage(MessageCenterMessage message) {
      switch ((message as TakeDamageMessage).VehicleLocation) {
        case VehicleChassisLocations.Turret:
        this.timeSinceVArmorDamaged[0] = 0.0f;
        this.timeSinceVStructureDamaged[0] = 0.0f;
        break;
        case VehicleChassisLocations.Front:
        this.timeSinceVArmorDamaged[1] = 0.0f;
        this.timeSinceVStructureDamaged[1] = 0.0f;
        break;
        case VehicleChassisLocations.Left:
        this.timeSinceVArmorDamaged[2] = 0.0f;
        this.timeSinceVStructureDamaged[2] = 0.0f;
        break;
        case VehicleChassisLocations.Right:
        this.timeSinceVArmorDamaged[3] = 0.0f;
        this.timeSinceVStructureDamaged[3] = 0.0f;
        break;
        case VehicleChassisLocations.Rear:
        this.timeSinceVArmorDamaged[4] = 0.0f;
        this.timeSinceVStructureDamaged[4] = 0.0f;
        break;
      }
      this.UpdateArmorStructureBars();
    }

    private void RefreshVehicleStructureAndArmor() {
      for (int index = 0; index < 5; ++index) {
        VehicleChassisLocations locationFromIndex = HUDVehicleArmorReadout.GetVCLocationFromIndex(index);
        float currentArmor = this.displayedVehicle.GetCurrentArmor(CombatHUDFakeVehicleArmorHover.armorLocationFromVehicleLocation(locationFromIndex));
        float currentStructure = this.displayedVehicle.GetCurrentStructure(CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex));
        float armorForVlocation = HUDFakeVehicleArmorReadout.GetInitialArmorForVLocation(this.displayedVehicle, CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex));
        float structureForVlocation = HUDFakeVehicleArmorReadout.getInitialStructureForVLocation(this.displayedVehicle, CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex));
        if ((double)currentArmor <= 0.0) {
          this.vArmorOutlineCached[index] = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.armorDamaged;
          this.vArmorCached[index] = Color.clear;
          if ((double)currentStructure <= 0.0) {
            this.vStructureCached[index] = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.structureDestroyed;
          } else {
            Color lerpedColorFromArray = UIHelpers.getLerpedColorFromArray(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureColors, 1f - currentStructure / structureForVlocation, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureUseStairSteps, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureNumStairSteps);
            this.vStructureCached[index] = lerpedColorFromArray;
          }
        } else {
          Color lerpedColorFromArray = UIHelpers.getLerpedColorFromArray(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorColors, 1f - currentArmor / armorForVlocation, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorUseStairSteps, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorNumStairSteps);
          this.vArmorOutlineCached[index] = Color.clear;
          this.vArmorCached[index] = lerpedColorFromArray;
          this.vStructureCached[index] = Color.clear;
        }
      }
    }

    public static VehicleChassisLocations GetVCLocationFromIndex(int index) {
      switch (index) {
        case 0:
        return VehicleChassisLocations.Turret;
        case 1:
        return VehicleChassisLocations.Front;
        case 2:
        return VehicleChassisLocations.Left;
        case 3:
        return VehicleChassisLocations.Right;
        case 4:
        return VehicleChassisLocations.Rear;
        default:
        return VehicleChassisLocations.None;
      }
    }

    public static float getInitialStructureForVLocation(
      Mech vehicle,
      ChassisLocations location) {
      return vehicle.MechDef.GetChassisLocationDef(location).InternalStructure * vehicle.StructureMultiplier;
    }

    public static float GetInitialArmorForVLocation(
      Mech vehicle,
      ChassisLocations location) {
      return vehicle.MechDef.GetLocationLoadoutDef(location).AssignedArmor * vehicle.ArmorMultiplier;
    }

    public static bool IsVehicleTargetedFromDirection(
      CombatGameState Combat,
      VehicleChassisLocations location,
      AttackDirection direction) {
      switch (direction) {
        case AttackDirection.FromFront:
        return Combat.Constants.HitTables.HitVehicleLocationFromFront.ContainsKey(location);
        case AttackDirection.FromLeft:
        return Combat.Constants.HitTables.HitVehicleLocationFromFront.ContainsKey(location);
        case AttackDirection.FromRight:
        return Combat.Constants.HitTables.HitVehicleLocationFromFront.ContainsKey(location);
        case AttackDirection.FromBack:
        return Combat.Constants.HitTables.HitVehicleLocationFromFront.ContainsKey(location);
        case AttackDirection.FromTop:
        return Combat.Constants.HitTables.HitVehicleLocationFromFront.ContainsKey(location);
        default:
        return false;
      }
    }

    public static float GetVehicleIndexRatioFromDirection(
      CombatGameState Combat,
      VehicleChassisLocations location,
      AttackDirection direction) {
      Dictionary<VehicleChassisLocations, int> dictionary = (Dictionary<VehicleChassisLocations, int>)null;
      switch (direction) {
        case AttackDirection.FromFront:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromFront;
        break;
        case AttackDirection.FromLeft:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromLeft;
        break;
        case AttackDirection.FromRight:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromRight;
        break;
        case AttackDirection.FromBack:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromBack;
        break;
        case AttackDirection.FromTop:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromTop;
        break;
        case AttackDirection.FromArtillery:
        dictionary = Combat.Constants.HitTables.HitVehicleLocationFromArtillery;
        break;
      }
      if (dictionary == null || !dictionary.ContainsKey(location))
        return 0.0f;
      float num1 = (float)dictionary[location];
      float num2 = 0.0f;
      foreach (KeyValuePair<VehicleChassisLocations, int> keyValuePair in dictionary) {
        if ((double)keyValuePair.Value > (double)num2)
          num2 = (float)keyValuePair.Value;
      }
      return num1 / num2;
    }

    public void UpdateVehicleStructureAndArmor(AttackDirection shownAttackDirection) {
      //if (this.UseForCalledShots) { Log.TWL(0, "HUDFakeVehicleArmorReadout.UpdateVehicleStructureAndArmor direction:"+ shownAttackDirection+" hovered:"+ this.HoveredArmor); }
      this.ShowAttackDirection(shownAttackDirection);
      Dictionary<VehicleChassisLocations, int> dictionary = (Dictionary<VehicleChassisLocations, int>)null;
      if (shownAttackDirection != AttackDirection.None && this.UseForCalledShots) {
        dictionary = this.HUD.Combat.HitLocation.GetVehicleHitTable(shownAttackDirection, false);
      }
      Color color1 = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorFlash.color;
      Color color2 = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorHovered.color;
      for (int index = 0; index < 5; ++index) {
        this.timeSinceVArmorDamaged[index] += Time.deltaTime;
        this.timeSinceVStructureDamaged[index] += Time.deltaTime;
        float armorTime = Mathf.Clamp01((float)(1.0 - (double)this.timeSinceVArmorDamaged[index] / (double)LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FlashArmorTime));
        float structTime = Mathf.Clamp01((float)(1.0 - (double)this.timeSinceVStructureDamaged[index] / (double)LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FlashArmorTime));
        VehicleChassisLocations locationFromIndex = HUDVehicleArmorReadout.GetVCLocationFromIndex(index);
        bool showLocation = !this.UseForCalledShots || dictionary != null && dictionary.ContainsKey(locationFromIndex) && (uint)dictionary[locationFromIndex] > 0U;
        int ishowLocation = !this.UseForCalledShots ? 0 : (!showLocation ? 1 : 0);
        //if (this.UseForCalledShots) { Log.WL(1, "index: "+index+" location:" + locationFromIndex +" fakeLocation:" + CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex)+" isShow:"+ showLocation); }
        Color a1 = !showLocation || this.HoveredArmor != CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex) ? this.vArmorOutlineCached[index] : Color.Lerp(this.vArmorOutlineCached[index], color2, 0.5f);
        if (ishowLocation != 0)
          a1 = Color.Lerp(a1, Color.black, this.hiddenColorLerp);
        UIHelpers.SetImageColor((Graphic)this.VArmorOutline[index], Color.Lerp(a1, color1, armorTime));
        Color a2 = !showLocation || this.HoveredArmor != CombatHUDFakeVehicleArmorHover.chassisLocationFromVehicleLocation(locationFromIndex) ? this.vArmorCached[index] : Color.Lerp(this.vArmorCached[index], color2, 0.5f);
        if (ishowLocation != 0)
          a2 = Color.Lerp(a2, Color.black, this.hiddenColorLerp);
        UIHelpers.SetImageColor((Graphic)this.VArmor[index], Color.Lerp(a2, color1, armorTime));
        Color a3 = this.vStructureCached[index];
        if (ishowLocation != 0)
          a3 = Color.Lerp(a3, Color.black, this.hiddenColorLerp);
        UIHelpers.SetImageColor((Graphic)this.VStructure[index], Color.Lerp(a3, color1, structTime));
      }
    }

    protected void ShowAttackDirection(AttackDirection shownAttackDirection) {
      switch (shownAttackDirection) {
        case AttackDirection.None:
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Enabled);
        break;
        case AttackDirection.FromFront:
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Highlighted);
        break;
        case AttackDirection.FromLeft:
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Highlighted);
        break;
        case AttackDirection.FromRight:
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Highlighted);
        break;
        case AttackDirection.FromBack:
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Enabled);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Highlighted);
        break;
        case AttackDirection.FromTop:
        this.SetTweens(this.directionalIndicatorFront, ButtonState.Highlighted);
        this.SetTweens(this.directionalIndicatorRight, ButtonState.Highlighted);
        this.SetTweens(this.directionalIndicatorBack, ButtonState.Highlighted);
        this.SetTweens(this.directionalIndicatorLeft, ButtonState.Highlighted);
        break;
      }
    }
  }
}