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
using HBS;
using IRBTModUtils;
using SVGImporter;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnits {
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class CombatHUDTargetingComputer_Init_fakeTurret {
    public static void Prefix(CombatHUDTargetingComputer __instance, CombatHUD HUD) {
      try {
        if (__instance.TurretArmorDisplay is FakeHUDTurretArmorReadout) { return; }
        FakeHUDTurretArmorReadout fakeHUDTurretArmorReadout = __instance.TurretArmorDisplay.gameObject.GetComponent<FakeHUDTurretArmorReadout>();
        if(fakeHUDTurretArmorReadout == null) {
          fakeHUDTurretArmorReadout = __instance.TurretArmorDisplay.gameObject.AddComponent<FakeHUDTurretArmorReadout>();
          fakeHUDTurretArmorReadout.ArmorBar = __instance.TurretArmorDisplay.ArmorBar;
          fakeHUDTurretArmorReadout.StructureBar = __instance.TurretArmorDisplay.StructureBar;
          fakeHUDTurretArmorReadout.turretArmor = __instance.TurretArmorDisplay.turretArmor;
          fakeHUDTurretArmorReadout.turretStructure = __instance.TurretArmorDisplay.turretStructure;
          fakeHUDTurretArmorReadout.HoverInfoTextArmor = __instance.TurretArmorDisplay.HoverInfoTextArmor;
          fakeHUDTurretArmorReadout.HoverInfoTextStructure = __instance.TurretArmorDisplay.HoverInfoTextStructure;
          fakeHUDTurretArmorReadout.turretArmor.gameObject.SetActive(true);
          fakeHUDTurretArmorReadout.turretStructure.gameObject.SetActive(true);
          HUDTurretArmorReadout old = __instance.TurretArmorDisplay;
          __instance.TurretArmorDisplay = fakeHUDTurretArmorReadout;
          GameObject.DestroyImmediate(old);
        } else {
          HUDTurretArmorReadout old = __instance.TurretArmorDisplay;
          __instance.TurretArmorDisplay = fakeHUDTurretArmorReadout;
          GameObject.DestroyImmediate(old);
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(HUDTurretArmorReadout))]
  [HarmonyPatch("DisplayedTurret")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(Turret) })]
  public static class HUDTurretArmorReadout_DisplayedTurret_set {
    public static bool Prefix(HUDTurretArmorReadout __instance, Turret value) {
      try {
        if (__instance is FakeHUDTurretArmorReadout fakeTurretReadout) {
          if (Thread.CurrentThread.isFlagSet("RefreshActorInfo")) {
            AbstractActor actor = Thread.CurrentThread.currentActor();
            Log.Combat?.TWL(0, "HUDTurretArmorReadout.DisplayedTurret real actor is "+ (actor == null?"null": actor.PilotableActorDef.ChassisID));
            fakeTurretReadout._DisplayedTurret = Thread.CurrentThread.currentActor();
          } else {
            fakeTurretReadout._DisplayedTurret = value;
          }
          return false;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(HUDTurretArmorReadout))]
  [HarmonyPatch("UpdateTurretStructureAndArmor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class HUDTurretArmorReadout_UpdateTurretStructureAndArmor {
    public static bool Prefix(HUDTurretArmorReadout __instance) {
      try {
        if (__instance is FakeHUDTurretArmorReadout fakeTurretReadout) {
          fakeTurretReadout._UpdateTurretStructureAndArmor();
          return false;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(HUDTurretArmorReadout))]
  [HarmonyPatch("OnActorTakeDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class HUDTurretArmorReadout_OnActorTakeDamage {
    public static bool Prefix(HUDTurretArmorReadout __instance, MessageCenterMessage message) {
      try {
        if (__instance is FakeHUDTurretArmorReadout fakeTurretReadout) {
          fakeTurretReadout._OnActorTakeDamage(message);
          return false;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(HUDTurretArmorReadout))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatHUD) })]
  public static class HUDTurretArmorReadout_Init {
    public static void Prefix(ref bool __runOriginal, HUDTurretArmorReadout __instance, CombatHUD HUD) {
      try {
        if (__instance is FakeHUDTurretArmorReadout fakeTurretReadout) {
          fakeTurretReadout._Init(HUD);
          __runOriginal = false; return;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SelectionStateMoraleAttack))]
  [HarmonyPatch("NeedsCalledShot")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMoraleAttack_NeedsCalledShot {
    public static void Postfix(SelectionStateMoraleAttack __instance, ref bool __result) {
      try {
        if (__result == false) { return; }
        if(__instance.TargetedCombatant is AbstractActor actor) {
          if (actor.GetCustomInfo().TurretArmorReadout) {
            __result = false;
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDTargetingComputer))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("RefreshActorInfo")]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDTargetingComputer_RefreshActorInfo {
    public static void Prefix(CombatHUDTargetingComputer __instance) {
      try {
        Thread.CurrentThread.SetFlag("RefreshActorInfo");
        Thread.CurrentThread.pushActor(__instance.ActivelyShownCombatant as AbstractActor);
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public static void Posftix(CombatHUDTargetingComputer __instance) {
      try {
        Thread.CurrentThread.ClearFlag("RefreshActorInfo");
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }

  public class FakeHUDTurretArmorReadout : HUDTurretArmorReadout {
    public new AbstractActor displayedTurret { get; set; } = null;
    public AbstractActor _DisplayedTurret {
      get => this.displayedTurret;
      set {
        if (this.displayedTurret != value) {
          this.ResetDamageTimes();
          this.displayedTurret = value;
          if (this.displayedTurret != null)
            this.ResetArmorStructureBars();
        }
        if (this.displayedTurret == null)
          return;
        this.RefreshTurretStructureAndArmor();
      }
    }
    public void _Init(CombatHUD iHUD) {
      this.HUD = iHUD;
    }
    public new void ResetArmorStructureBars() {
      //Log.TWL(0, "FakeHUDTurretArmorReadout.ResetArmorStructureBars");
      try {
        if (this.displayedTurret == null)
          return;
        //Log.WL(1, "displayedTurret:" + displayedTurret.PilotableActorDef.ChassisID);
        float summaryArmorCurrent = this.displayedTurret.SummaryArmorCurrent;
        float summaryArmorMax = this.displayedTurret.SummaryArmorMax;
        float structureCurrent = this.displayedTurret.SummaryStructureCurrent;
        float summaryStructureMax = this.displayedTurret.SummaryStructureMax;
        this.ArmorBar.ShowNewSummary(summaryArmorCurrent, summaryArmorMax, false);
        this.StructureBar.ShowNewSummary(structureCurrent, summaryStructureMax, this.displayedTurret.IsAnyStructureExposed);
        this.HoverInfoTextArmor.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(summaryArmorCurrent), (object)HUDMechArmorReadout.FormatForSummary(summaryArmorMax));
        this.HoverInfoTextStructure.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(structureCurrent), (object)HUDMechArmorReadout.FormatForSummary(summaryStructureMax));
        if (this.displayedTurret != null && this.HoverInfoTextArmor != null && this.HoverInfoTextStructure != null) {
          if (!this.displayedTurret.Combat.HostilityMatrix.IsLocalPlayerFriendly(this.displayedTurret.TeamId)) {
            //Log.TWL(0, $"Hiding armor and structure on target: {(this.displayedTurret.DisplayName)}");
            LowVisibilityAPIHelper.ObfuscateArmorAndStructText(this.displayedTurret, this.HoverInfoTextArmor, this.HoverInfoTextStructure);
          }
        }
        //Log.WL(1, "FakeHUDTurretArmorReadout armor " + summaryArmorCurrent+"/"+ summaryArmorMax);
        //Log.WL(1, "FakeHUDTurretArmorReadout structure " + structureCurrent + "/" + summaryStructureMax);
      } catch (Exception e) {
        Log.ECombat?.TWL(0,e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }

    public new void UpdateArmorStructureBars() {
      //Log.TWL(0, "FakeHUDTurretArmorReadout.UpdateArmorStructureBars");
      try {
        if (this.displayedTurret == null)
          return;
        //Log.WL(1, "displayedTurret:"+ displayedTurret.PilotableActorDef.ChassisID);
        float summaryArmorCurrent = this.displayedTurret.SummaryArmorCurrent;
        float summaryArmorMax = this.displayedTurret.SummaryArmorMax;
        float structureCurrent = this.displayedTurret.SummaryStructureCurrent;
        float summaryStructureMax = this.displayedTurret.SummaryStructureMax;
        this.ArmorBar.UpdateSummary(summaryArmorCurrent, false);
        this.StructureBar.UpdateSummary(structureCurrent, this.displayedTurret.IsAnyStructureExposed);
        this.HoverInfoTextArmor.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(summaryArmorCurrent), (object)HUDMechArmorReadout.FormatForSummary(summaryArmorMax));
        this.HoverInfoTextStructure.SetText("{0}/{1}", (object)HUDMechArmorReadout.FormatForSummary(structureCurrent), (object)HUDMechArmorReadout.FormatForSummary(summaryStructureMax));
        if (this.displayedTurret != null && this.HoverInfoTextArmor != null && this.HoverInfoTextStructure != null) {
          if (!this.displayedTurret.Combat.HostilityMatrix.IsLocalPlayerFriendly(this.displayedTurret.TeamId)) {
            //Log.TWL(0, $"Hiding armor and structure on target: {(this.displayedTurret.DisplayName)}");
            LowVisibilityAPIHelper.ObfuscateArmorAndStructText(this.displayedTurret, this.HoverInfoTextArmor, this.HoverInfoTextStructure);
          }
        }
        //Log.WL(1, "FakeHUDTurretArmorReadout armor " + summaryArmorCurrent + "/" + summaryArmorMax);
        //Log.WL(1, "FakeHUDTurretArmorReadout structure " + structureCurrent + "/" + summaryStructureMax);
      } catch (Exception e) {
        Log.ECombat?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
    public new void RefreshTurretStructureAndArmor() {
      if (this.displayedTurret == null)
        return;
      float turretStructure = this.displayedTurret.SummaryStructureCurrent;
      float summaryStructureMax = this.displayedTurret.SummaryStructureMax;
      float turretArmor = this.displayedTurret.SummaryArmorCurrent;
      float summaryArmorMax = this.displayedTurret.SummaryArmorMax;
      this.TurretArmorCached = (double)turretArmor > 0.0 ? UIHelpers.getLerpedColorFromArray(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorColors, 1f - turretArmor / summaryArmorMax, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorUseStairSteps, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorNumStairSteps) : Color.clear;
      if ((double)turretStructure <= 0.0)
        this.TurretStructureCached = LazySingletonBehavior<UIManager>.Instance.UIColorRefs.structureDestroyed;
      else
        this.TurretStructureCached = UIHelpers.getLerpedColorFromArray(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureColors, 1f - turretStructure / summaryStructureMax, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureUseStairSteps, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StructureNumStairSteps);
    }

    public void _UpdateTurretStructureAndArmor() {
      this.timeSinceArmorDamaged += Time.deltaTime;
      this.timeSinceStructureDamaged += Time.deltaTime;
      UIHelpers.SetImageColor((Graphic)this.turretArmor, Color.Lerp(this.TurretArmorCached, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorFlash.color, Mathf.Clamp01((float)(1.0 - (double)this.timeSinceArmorDamaged / (double)LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FlashArmorTime))));
      UIHelpers.SetImageColor((Graphic)this.turretStructure, Color.Lerp(this.TurretStructureCached, LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ArmorFlash.color, Mathf.Clamp01((float)(1.0 - (double)this.timeSinceStructureDamaged / (double)LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.FlashArmorTime))));
    }

    public void _OnActorTakeDamage(MessageCenterMessage message) {
      this.timeSinceArmorDamaged = 0.0f;
      this.timeSinceStructureDamaged = 0.0f;
      this.UpdateArmorStructureBars();
    }
  }

}