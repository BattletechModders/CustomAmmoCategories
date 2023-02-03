using BattleTech.UI;
using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Sheepy.BattleTechMod.AttackImprovementMod {
  using static Mod;
  using static System.Reflection.BindingFlags;

  public class WeaponInfo : BattleModModule {

    public override void CombatStartsOnce() {
      Type SlotType = typeof(CombatHUDWeaponSlot), PanelType = typeof(CombatHUDWeaponPanel);

      if (AIMSettings.SaturationOfLoadout != 0 || AIMSettings.ShowDamageInLoadout || AIMSettings.ShowMeleeDamageInLoadout || AIMSettings.ShowAlphaDamageInLoadout != null)
        Patch(typeof(CombatHUDTargetingComputer), "RefreshWeaponList", null, "EnhanceWeaponLoadout");

      if (AIMSettings.ShowBaseHitchance) {
        //Patch(SlotType, "UpdateToolTipsFiring", typeof(ICombatant), "ShowBaseHitChance", null);
        //Patch(SlotType, "UpdateToolTipsMelee", typeof(ICombatant), "ShowBaseMeleeChance", null);
      }
      if (AIMSettings.ShowNeutralRangeInBreakdown) {
        rangedPenalty = ModifierList.GetRangedModifierFactor("range");
        //Patch(SlotType, "UpdateToolTipsFiring", typeof(ICombatant), "ShowNeutralRange", null);
      }
      if (AIMSettings.ShowWeaponProp || AIMSettings.WeaponRangeFormat != null)
        Patch(SlotType, "GenerateToolTipStrings", null, "UpdateWeaponTooltip");

      if (AIMSettings.ShowReducedWeaponDamage || AIMSettings.CalloutWeaponStability) {
        Patch(SlotType, "RefreshDisplayedWeapon", null, "UpdateWeaponDamage");
      }
      if (AIMSettings.ShowTotalWeaponDamage) {
        Patch(PanelType, "ShowWeaponsUpTo", null, "ShowTotalDamageSlot");
        Patch(PanelType, "RefreshDisplayedWeapons", "ResetTotalWeaponDamage", "ShowTotalWeaponDamage");
        Patch(SlotType, "RefreshHighlighted", "BypassTotalSlotHighlight", null);
      }
      if (AIMSettings.ShowReducedWeaponDamage || AIMSettings.ShowTotalWeaponDamage) {
        // Update damage numbers (and multi-target highlights) _after_ all slots are in a correct state.
        Patch(typeof(SelectionStateFireMulti), "SetTargetedCombatant", null, "RefreshTotalDamage");
        Patch(SlotType, "OnPointerUp", null, "RefreshTotalDamage");
      }
      if (AIMSettings.CalloutWeaponStability)
        HeauUpDisplay.HookCalloutToggle(ToggleStabilityDamage);

      //if ( HasMod( "com.joelmeador.WeaponRealizer", "WeaponRealizer.Core" ) ) TryRun( ModLog, InitWeaponRealizerBridge );
    }

    public override void CombatStarts() {
      if (string.IsNullOrEmpty(RollCorrection.WeaponHitChanceFormat))
        BaseChanceFormat = "{2:0}%";
      else
        BaseChanceFormat = RollCorrection.WeaponHitChanceFormat.Replace("{0:", "{2:");
    }

    /*private void InitWeaponRealizerBridge () {
       Assembly WeaponRealizer = AppDomain.CurrentDomain.GetAssemblies().First( e => e.GetName().Name == "WeaponRealizer" );
       Type WeaponRealizerCalculator = WeaponRealizer?.GetType( "WeaponRealizer.Calculator" );
       WeaponRealizerDamageModifiers = WeaponRealizerCalculator?.GetMethod( "ApplyAllDamageModifiers", Static | NonPublic );
       if ( WeaponRealizerDamageModifiers == null )
          BattleMod.BTML_LOG.Warn( "Attack Improvement Mod cannot bridge with WeaponRealizer. Damage prediction may be inaccurate." );
       else
          Info( "Attack Improvement Mod has bridged with WeaponRealizer.Calculator on damage prediction." );
    }*/

    // ============ Weapon Loadout ============

    private const string MetaColour = "<#888888FF>";
    private static Color[] ByType;
    private static TMPro.TextMeshProUGUI loadout;

    private static void DisableLoadoutLineWrap(List<TMPro.TextMeshProUGUI> weaponNames) {
      foreach (TMPro.TextMeshProUGUI weapon in weaponNames)
        weapon.enableWordWrapping = false;
    }

    public static void EnhanceWeaponLoadout(CombatHUDTargetingComputer __instance, List<TMPro.TextMeshProUGUI> ___weaponNames, UIManager ___uiManager) {
      try {
        UIColorRefs colours = ___uiManager.UIColorRefs;
        AbstractActor actor = __instance.ActivelyShownCombatant as AbstractActor;
        List<Weapon> weapons = actor?.Weapons;
        if (actor == null || weapons == null || colours == null) return;
        if (ByType == null) {
          ByType = LerpWeaponColours(colours);
          DisableLoadoutLineWrap(___weaponNames);
        }
        float close = 0, medium = 0, far = 0;
        for (int i = Math.Min(___weaponNames.Count, weapons.Count) - 1; i >= 0; i--) {
          Weapon w = weapons[i];
          if (w == null || !w.CanFire) continue;
          float damage = w.DamagePerShot * w.ShotsWhenFired;
          if (AIMSettings.ShowDamageInLoadout)
            ___weaponNames[i].text = ___weaponNames[i].text.Replace(" +", "+") + MetaColour + " (" + damage + ")";
          if (ByType.Length > 0 && ___weaponNames[i].color == colours.qualityA) {
            if ((int)w.WeaponCategoryValue.ID < ByType.Length)
              ___weaponNames[i].color = ByType[(int)w.WeaponCategoryValue.ID];
          }
          if (w.MaxRange <= 90) close += damage;
          else if (w.MaxRange <= 360) medium += damage;
          else far += damage;
        }

        if (AIMSettings.ShowAlphaDamageInLoadout != null && HasDamageLabel(colours.white))
          loadout.text = new Text(AIMSettings.ShowAlphaDamageInLoadout, close + medium + far, close, medium, far, medium + far).ToString();

        if (AIMSettings.ShowMeleeDamageInLoadout && actor is Mech mech) {
          int start = weapons.Count, dmg = (int)(mech.MeleeWeapon?.DamagePerShot * mech.MeleeWeapon?.ShotsWhenFired);
          string format = AIMSettings.ShowDamageInLoadout ? "{0} {1}({2})" : "{0} {1}{2}";
          if (start < ___weaponNames.Count && dmg > 0)
            SetWeapon(___weaponNames[start], colours.white, format, Translate("__/AIM.MELEE/__"), MetaColour, dmg);
          dmg = (int)(mech.DFAWeapon?.DamagePerShot * mech.DFAWeapon?.ShotsWhenFired);
          if (actor.WorkingJumpjets > 0 && start + 1 < ___weaponNames.Count && dmg > 0)
            SetWeapon(___weaponNames[start + 1], colours.white, format, Translate("__/AIM.DFA/__"), MetaColour, dmg);
        }
      } catch (Exception ex) { Error(ex); }
    }

    private static bool HasDamageLabel(Color white) {
      if (loadout != null) return true;
      loadout = GameObject.Find("tgtWeaponsLabel")?.GetComponent<TMPro.TextMeshProUGUI>();
      if (loadout == null) return false;
      loadout.rectTransform.sizeDelta = new Vector2(200, loadout.rectTransform.sizeDelta.y);
      loadout.transform.Translate(10, 0, 0);
      loadout.alignment = TMPro.TextAlignmentOptions.Left;
      loadout.fontStyle = TMPro.FontStyles.Normal;
      loadout.color = white;
      return true;
    }

    private static Color[] LerpWeaponColours(UIColorRefs ui) {
      if (AIMSettings.SaturationOfLoadout <= 0) return new Color[0];
      Color[] colours = new Color[] { Color.clear, ui.ballisticColor, ui.energyColor, ui.missileColor, ui.smallColor };
      float saturation = (float)AIMSettings.SaturationOfLoadout, lower = saturation * 0.8f;
      for (int i = colours.Length - 1; i > 0; i--) {
        Color.RGBToHSV(colours[i], out float h, out float s, out float v);
        colours[i] = Color.HSVToRGB(h, i == 3 ? lower : saturation, 1);
      }
      return colours;
    }

    private static void SetWeapon(TMPro.TextMeshProUGUI ui, Color color, string text, params object[] augs) {
      ui.text = augs.Length <= 0 ? text : new Text(text, augs).ToString();
      ui.color = color;
      ui.transform.parent.gameObject.SetActive(true);
    }

    // ============ Mouseover Hint ============

    private static string BaseChanceFormat;

    public static void ShowBaseHitChance(CombatHUDWeaponSlot __instance, ICombatant target) {
      try {
        AbstractActor unit = HUD.SelectedActor;
        if (unit == null) { return; }
        float baseChance = RollModifier.StepHitChance(Combat.ToHit.GetBaseToHitChance(unit)) * 100;
        __instance.ToolTipHoverElement.BuffStrings.Add(new Text("{0} {1} = " + BaseChanceFormat, Translate(Pilot.PILOTSTAT_GUNNERY), unit.SkillGunnery, baseChance));
      } catch (Exception ex) { Error(ex); }
    }

    public static void ShowBaseMeleeChance(CombatHUDWeaponSlot __instance, ICombatant target) {
      try {
        Mech mech = HUD.SelectedActor as Mech;
        if (mech == null) { return; }
        float baseChance = RollModifier.StepHitChance( Combat.ToHit.GetBaseMeleeToHitChance(mech) ) * 100;
        __instance.ToolTipHoverElement.BuffStrings.Add(new Text("{0} {1} = " + BaseChanceFormat, Translate(Pilot.PILOTSTAT_PILOTING), mech.SkillPiloting, baseChance));
      } catch (Exception ex) { Error(ex); }
    }

    private static Func<ModifierList.AttackModifier> rangedPenalty;

    public static void ShowNeutralRange(CombatHUDWeaponSlot __instance, ICombatant target) {
      try {
        ModifierList.AttackModifier range = rangedPenalty();
        if (range.Value != 0) return;
        __instance.ToolTipHoverElement.BuffStrings.Add(new Text(range.DisplayName));
      } catch (Exception ex) { Error(ex); }
    }

    public static void UpdateWeaponTooltip(CombatHUDWeaponSlot __instance) {
      try {
        Weapon weapon = __instance.DisplayedWeapon;
        List<Text> spec = __instance.ToolTipHoverElement?.WeaponStrings;
        if (weapon == null || spec == null || spec.Count != 3) return;
        if (AIMSettings.ShowWeaponProp && !string.IsNullOrEmpty(weapon.weaponDef.BonusValueA))
          spec[0] = new Text(string.IsNullOrEmpty(weapon.weaponDef.BonusValueB) ? "{0}" : "{0}, {1}", weapon.weaponDef.BonusValueA, weapon.weaponDef.BonusValueB);
        if (AIMSettings.WeaponRangeFormat != null)
          spec[2] = new Text(AIMSettings.WeaponRangeFormat, weapon.MinRange, weapon.ShortRange, weapon.MediumRange, weapon.LongRange, weapon.MaxRange);
      } catch (Exception ex) { Error(ex); }
    }

    // ============ Weapon Panel ============

    //private static MethodInfo WeaponRealizerDamageModifiers;

    private static bool ShowingStabilityDamage; // Set only by ToggleStabilityDamage, which only triggers when CalloutWeaponStability is on
    private static float TotalDamage, AverageDamage;
    private static float TotalAPDamage, AverageAPDamage;
    private static float TotalHeatDamage, AverageHeatDamage;
    private static CombatHUDWeaponSlot TotalSlot;

    private static void AddToTotalDamage(float dmg, float ap, float heat, CombatHUDWeaponSlot slot) {
      Weapon w = slot.DisplayedWeapon;
      if (!w.IsEnabled) return;
      float chance = slot.HitChance;
      float mdmg = dmg * w.ShotsWhenFired;
      float map = ap * w.ShotsWhenFired;
      float mheat = heat * w.ShotsWhenFired;
      ICombatant target = HUD.SelectedTarget;
      if (chance <= 0) {
        if (target != null) return; // Hit Chance is -999.9 if it can't fire at target (Method ClearHitChance)
        chance = 1; // Otherwise, preview full damage when no target is selected.
      }
      SelectionState state = HUD.SelectionHandler.ActiveState;
      if (state is SelectionStateFireMulti multi
        && state.AllTargetedCombatants.Contains(HUD.SelectedTarget)
        && HUD.SelectedTarget != multi.GetSelectedTarget(w)) return;
      TotalDamage += mdmg;
      AverageDamage += mdmg * chance;
      TotalAPDamage += map;
      AverageAPDamage += map * chance;
      TotalHeatDamage += mheat;
      AverageHeatDamage += mheat * chance;
    }

    public static void UpdateWeaponDamage(CombatHUDWeaponSlot __instance, ICombatant target) {
      try {
        Weapon weapon = __instance.DisplayedWeapon;
        if (weapon == null || !weapon.CanFire || target == null) return;
        string text = null;
        AbstractActor owner = weapon.parent;
        Vector3 position = (owner.HasMovedThisRound ? null : ActiveState?.PreviewPos) ?? owner.CurrentPosition;
        CustAmmoCategories.DamageModifiers mods = CustAmmoCategories.DamageModifiersCache.GetDamageModifiers(weapon,position, target);
        float damage = weapon.DamagePerShot;
        float ap = weapon.StructureDamagePerShot;
        float heat = weapon.HeatDamagePerShot;
        float stability = weapon.Instability();
        int shots = weapon.ShotsWhenFired;
        string descr = string.Empty;
        mods.Calculate(-1,ref damage, ref ap, ref heat, ref stability, ref descr, false, true);
        //CustAmmoCategories.AdvWeaponHitInfoRec.PredictDamage(weapon, target, position, out float damage, out float ap, out float heat, out float stability, out int shots);
        if (ShowingStabilityDamage) {
          if (!__instance.WeaponText.text.Contains(HeatPrefix))
            __instance.WeaponText.text += FormatHeat(weapon.HeatGenerated);
          float dmg = stability;
          AddToTotalDamage(dmg,ap,heat, __instance);
          text = FormatStabilityDamage(dmg);
        } else {
          if (__instance.WeaponText.text.Contains(HeatPrefix))
            __instance.WeaponText.text = weapon.UIName.ToString();
          if (ActiveState is SelectionStateFireMulti multi && __instance.TargetIndex < 0) return;
          /*float raw = weapon.DamagePerShotAdjusted(), dmg = raw; // damage displayed by vanilla
          if (target != null) {
            dmg = weapon.DamagePerShotFromPosition(MeleeAttackType.NotSet, position, target); // damage with all masks and reductions factored
                                                                                              //Info( "{0} {1} => {2} {3} {4}, Dir {5}", owner.CurrentPosition, position, target, target.CurrentPosition, target.CurrentRotation, Combat.HitLocation.GetAttackDirection( position, target ) );
            if (WeaponRealizerDamageModifiers != null)
              dmg = (float)WeaponRealizerDamageModifiers.Invoke(null, new object[] { weapon.parent, target, weapon, dmg, false });
          }*/
          AddToTotalDamage(damage, ap, heat, __instance);
          //if (target == null || Math.Abs(raw - dmg) < 0.01) return;
          text = ((int)damage).ToString();
        }
        if (heat > CustAmmoCategories.CustomAmmoCategories.Epsilon) {
          text = string.Format(HUD.WeaponPanel.HeatFormatString, text, Mathf.RoundToInt(heat));
        }
        if (ap > CustAmmoCategories.CustomAmmoCategories.Epsilon) {
          text = string.Format(HUD.WeaponPanel.StructureFormatString, text, Mathf.RoundToInt(heat));
        }
        if (weapon.ShotsWhenFired > 1)
          text = string.Format("{0}</color> (x{1})", text, weapon.ShotsWhenFired);
        __instance.DamageText.text = text;
      } catch (Exception ex) { Error(ex); }
    }

    public static void ShowTotalDamageSlot(CombatHUDWeaponPanel __instance, int topIndex, List<CombatHUDWeaponSlot> ___WeaponSlots) {
      try {
        TotalSlot = null;
        if (topIndex <= 0 || topIndex >= ___WeaponSlots.Count || __instance.DisplayedActor == null) return;
        TotalSlot = ___WeaponSlots[topIndex];
        TotalSlot.transform.parent.gameObject.SetActive(true);
        TotalSlot.DisplayedWeapon = null;
        TotalSlot.WeaponText.text = new Text(GetTotalLabel()).ToString();
        TotalSlot.AmmoText.text = "";
        TotalSlot.MainImage.color = Color.clear;
        TotalSlot.ToggleButton.childImage.color = Color.clear;
      } catch (Exception ex) { Error(ex); }
    }

    public static void ResetTotalWeaponDamage() {
      TotalDamage = AverageDamage = 0;
    }

    public static void ShowTotalWeaponDamage(List<CombatHUDWeaponSlot> ___WeaponSlots) {
      try {
        if (TotalSlot == null) return;
        if (!AIMSettings.ShowReducedWeaponDamage) { // Sum damage when reduced damage patch is not applied.
          foreach (CombatHUDWeaponSlot slot in ___WeaponSlots) {
            Weapon w = slot.DisplayedWeapon;
            if (w != null && w.IsEnabled && w.CanFire)
              AddToTotalDamage(ShowingStabilityDamage ? w.Instability() : w.DamagePerShotAdjusted(), w.StructureDamagePerShotAdjusted(), w.HeatDamagePerShotAdjusted(AttackImpactQuality.Solid), slot);
          }
        }
        TotalSlot.WeaponText.text = GetTotalLabel();
        TotalSlot.DamageText.text = FormatTotalDamage(TotalDamage,TotalAPDamage,TotalHeatDamage, true);
        TotalSlot.HitChanceText.text = FormatTotalDamage(AverageDamage,AverageAPDamage,AverageHeatDamage, true);
      } catch (Exception ex) { Error(ex); }
    }

    public static void RefreshTotalDamage() {
      try {
        if (ActiveState is SelectionStateFireMulti)
          HUD?.WeaponPanel?.RefreshDisplayedWeapons(); // Refresh weapon highlight AFTER HUD.SelectedTarget is updated.
      } catch (Exception ex) { Error(ex); }
    }

    public static void ToggleStabilityDamage(bool IsCallout) {
      try {
        ShowingStabilityDamage = IsCallout;
        HUD?.WeaponPanel?.RefreshDisplayedWeapons();
      } catch (Exception ex) { Error(ex); }
    }

    public static bool BypassTotalSlotHighlight(CombatHUDWeaponSlot __instance) {
      try {
        if (__instance.DisplayedWeapon == null) return false; // Skip highlight if no weapon in this slot
        return true;
      } catch (Exception ex) { return Error(ex); }
    }

    // ============ Helpers ============

    private const string HeatPrefix = " <#FF0000>", HeatPostfix = "H";
    private const string StabilityPrefix = "<#FFFF00>", StabilityPostfix = "s";

    private static string FormatStabilityDamage(float dmg, bool alwaysShowNumber = false) {
      if (dmg < 1 && !alwaysShowNumber) return StabilityPrefix + "--";
      return StabilityPrefix + (int)dmg + StabilityPostfix;
    }

    private static string FormatHeat(float heat) {
      if (heat < 1) return HeatPrefix + "--";
      return HeatPrefix + (int)heat + HeatPostfix;
    }
    public static string HeatFormatString = "{0} <color=#F79232>({1})</color>";
    public static string StructureFormatString = "{0} <color=#F04228>({1})</color>";

    private static string FormatTotalDamage(float dmg, float ap, float heat, bool alwaysShowNumber = false) {
      if (ShowingStabilityDamage) return FormatStabilityDamage(dmg, alwaysShowNumber);
      string result = string.Format("{0}", (object)dmg);
      if(heat > CustAmmoCategories.CustomAmmoCategories.Epsilon) {
        result = string.Format(HeatFormatString, result, (object)Mathf.RoundToInt(heat));
      }
      if (ap > CustAmmoCategories.CustomAmmoCategories.Epsilon) {
        result = string.Format(StructureFormatString, result, (object)Mathf.RoundToInt(heat));
      }
      return result;
    }

    private static string GetTotalLabel() {
      string label = ShowingStabilityDamage ? "__/AIM.GetTotalLabel1/__{0} __/AIM.GetTotalLabel2/__" : "__/AIM.GetTotalLabel1/__{0}", target = null;
      if (ActiveState is SelectionStateFireMulti multi) {
        switch (multi.GetTargetIndex(HUD.SelectedTarget)) {
          case 0: target = "__/AIM.TARGET1/__"; break;
          case 1: target = "__/AIM.TARGET2/__"; break;
          case 2: target = "__/AIM.TARGET3/__"; break;
        }
      }
      return string.Format(label, target);
    }
  }
}