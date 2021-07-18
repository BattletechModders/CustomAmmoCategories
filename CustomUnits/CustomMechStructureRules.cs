﻿using BattleTech;
using BattleTech.Rendering;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using CustomComponents;
using Harmony;
using Localize;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyArmorStatDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation), typeof(float), typeof(WeaponHitInfo) })]
  public static class Mech_ApplyArmorStatDamage {
    public static void Prefix(Mech __instance, ArmorLocation location, float damage,ref WeaponHitInfo hitInfo) {
      try {
        Log.TWL(0, "Mech.ApplyArmorStatDamage prefix " + (__instance != null ? __instance.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(__instance);
        Thread.CurrentThread.SetFlag("ApplyArmorStatDamage");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(Mech __instance, ArmorLocation location, float damage, ref WeaponHitInfo hitInfo) {
      try {
        Log.TWL(0, "Mech.ApplyArmorStatDamage postfix" + (__instance != null ? __instance.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.ClearFlag("ApplyArmorStatDamage");
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("SetNeedsInjury")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(InjuryReason) })]
  public static class Pilot_SetNeedsInjury {
    public static bool Prefix(Pilot __instance, InjuryReason reason) {
      try {
        Log.TWL(0, "Pilot.SetNeedsInjury Prefix" + __instance.Description.Id + " mech:" + (Thread.CurrentThread.currentActor() == null?"null": Thread.CurrentThread.currentActor().Description.Id));
        if (Thread.CurrentThread.isFlagSet("ApplyArmorStatDamage")) {
          AbstractActor unit = Thread.CurrentThread.currentActor();
          if (unit == null) { return true; }
          if (unit.FakeVehicle()) {
            Log.WL(1, "stop propagation");
            return false;
          } else {
            TrooperSquad squad = unit as TrooperSquad;
            if (squad != null) {
              Log.WL(1, "stop propagation");
              return false;
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("setToolTipInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ArmorLocation) })]
  public static class CombatHUDMechTrayArmorHover_setToolTipInfoMech {
    public static void Prefix(CombatHUDMechTrayArmorHover __instance, Mech mech, ArmorLocation location) {
      try {
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo prefix " + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(mech);
        Thread.CurrentThread.SetFlag("CHANGE_MECH_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(CombatHUDMechTrayArmorHover __instance, Mech mech, ArmorLocation location) {
      try {
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo postfix" + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
        Thread.CurrentThread.ClearFlag("CHANGE_MECH_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("setToolTipInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(ArmorLocation) })]
  public static class CombatHUDMechTrayArmorHover_setToolTipInfoMechDef {
    public static void Prefix(CombatHUDMechTrayArmorHover __instance, MechDef mech, ArmorLocation location) {
      try {
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo prefix " + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActorDef(mech);
        Thread.CurrentThread.SetFlag("CHANGE_MECHDEF_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(CombatHUDMechTrayArmorHover __instance, MechDef mech, ArmorLocation location) {
      try {
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo postfix" + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActorDef();
        Thread.CurrentThread.ClearFlag("CHANGE_MECHDEF_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDAttackModeSelector))]
  [HarmonyPatch("DisplayedLocation")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation) })]
  public static class CombatHUDAttackModeSelector_DisplayedLocation {
    public static void Prefix(CombatHUDAttackModeSelector __instance, ArmorLocation value) {
      try {
        AbstractActor mech = Traverse.Create(__instance).Property<CombatHUD>("HUD").Value.SelectedTarget as AbstractActor;
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo prefix " + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(mech);
        Thread.CurrentThread.SetFlag("CHANGE_MECH_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(CombatHUDMechTrayArmorHover __instance, ArmorLocation value) {
      try {
        AbstractActor mech = Traverse.Create(__instance).Property<CombatHUD>("HUD").Value.SelectedTarget as AbstractActor;
        Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo postfix" + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.ClearFlag("CHANGE_MECH_LOCATION_NAME");
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AttackStackSequence))]
  [HarmonyPatch("OnAttackBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackStackSequence_OnAttackBeginSquad {
    public static void Prefix(AttackStackSequence __instance, MessageCenterMessage message) {
      try {
        AttackDirector.AttackSequence attackSequence = Traverse.Create(__instance).Property<CombatGameState>("Combat").Value.AttackDirector.GetAttackSequence((message as AttackSequenceBeginMessage).sequenceId);
        AbstractActor mech = attackSequence.chosenTarget as AbstractActor;
        Log.TWL(0, "AttackStackSequence.OnAttackBegin prefix " + (mech != null? mech.Description.Id:"null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(mech);
        Thread.CurrentThread.SetFlag("CHANGE_MECH_LOCATION_NAME");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(AttackStackSequence __instance, MessageCenterMessage message) {
      try {
        AttackDirector.AttackSequence attackSequence = Traverse.Create(__instance).Property<CombatGameState>("Combat").Value.AttackDirector.GetAttackSequence((message as AttackSequenceBeginMessage).sequenceId);
        AbstractActor mech = attackSequence.chosenTarget as AbstractActor;
        Log.TWL(0, "AttackStackSequence.OnAttackBegin postfix" + (mech != null ? mech.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.ClearFlag("CHANGE_MECH_LOCATION_NAME");
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetLongArmorLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation) })]
  public static class Mech_GetLongArmorLocation {
    public static void Postfix(ArmorLocation location, ref Text __result) {
      try {
        if (Thread.CurrentThread.isFlagSet("GetLongArmorLocation_CallNative")) { return; }
        Log.TWL(0, "Mech.GetLongArmorLocation Prefix" + (Thread.CurrentThread.currentActor() == null ? "null" : Thread.CurrentThread.currentActor().Description.Id));
        if (Thread.CurrentThread.isFlagSet("CHANGE_MECH_LOCATION_NAME")) {
          ICustomMech mech = Thread.CurrentThread.currentMech() as ICustomMech;
          if (mech != null) { __result = mech.GetLongArmorLocation(location); }
        } else
        if (Thread.CurrentThread.isFlagSet("CHANGE_MECHDEF_LOCATION_NAME")) {
          MechDef m = Thread.CurrentThread.currentMechDef();
          if (m == null) { return; }
          UnitCustomInfo info = m.GetCustomInfo();
          if (info == null) { return; };
          if (info.SquadInfo.Troopers <= 1) { return; };
          if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
            __result = new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
          }
          __result = new Text("UNIT");
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetAbbreviatedChassisLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations) })]
  public static class Mech_GetAbbreviatedChassisLocation {
    public static void Postfix(ChassisLocations location, ref Text __result) {
      try {
        //Log.TWL(0, "Mech.GetAbbreviatedChassisLocation Prefix" + (Thread.CurrentThread.currentActor() == null ? "null" : Thread.CurrentThread.currentActor().Description.Id));
        Mech mech = Thread.CurrentThread.currentMech();
        if (mech == null) { return; }
        TrooperSquad squad = mech as TrooperSquad;
        if (squad != null) {
          if (BaySquadReadoutAligner.STRUCTURE_TO_SQUAD.TryGetValue(location, out int index)) {
            __result = new Text("U{0}", index);
          } else {
            __result = new Text("U");
          }
        } else
        if (mech.FakeVehicle()) {
          __result = new Text(ToHitModifiersHelper.GetAbbreviatedChassisLocation(location.toFakeVehicleChassis()));
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("OnPointerClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDMechTrayArmorHover_OnPointerClick {
    public static bool Prefix(CombatHUDMechTrayArmorHover __instance, bool ___usedForCalledShots) {
      try {
        HUDMechArmorReadout Readout = Traverse.Create(__instance).Property<HUDMechArmorReadout>("Readout").Value;
        Mech m = Readout.DisplayedMech;
        Log.TWL(0, "CombatHUDMechTrayArmorHover.OnPointerClick Prefix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(m);
        if (___usedForCalledShots == false) return false;
        ArmorLocation locationFromIndex = HUDMechArmorReadout.GetArmorLocationFromIndex(__instance.chassisIndex, __instance.isRearArmor, __instance.isRearArmor ? Readout.flipRearDisplay : Readout.flipFrontDisplay);
        if (!(Readout.HUD.SelectionHandler.ActiveState is SelectionStateFire activeState) || !activeState.NeedsCalledShot || (!Readout.gameObject.activeSelf || locationFromIndex == ArmorLocation.None) || (!(activeState.TargetedCombatant is Mech targetedCombatant) || targetedCombatant.IsLocationDestroyed(MechStructureRules.GetChassisLocationFromArmorLocation(locationFromIndex)))) {
          return false;
        }
        Dictionary<ArmorLocation, int> mechHitTable = Readout.HUD.Combat.HitLocation.GetMechHitTableCustom(Readout.HUD.Combat.HitLocation.GetAttackDirection(Readout.HUD.SelectedActor, (ICombatant)Readout.DisplayedMech),m, null, -1, false);
        if (!mechHitTable.ContainsKey(locationFromIndex) || mechHitTable[locationFromIndex] == 0) { return false; }
        activeState.SetCalledShot(locationFromIndex);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(CombatHUDMechTrayArmorHover __instance) {
      try {
        Mech m = Traverse.Create(__instance).Property<HUDMechArmorReadout>("Readout").Value.DisplayedMech;
        Log.TWL(0, "CombatHUDMechTrayArmorHover.OnPointerClick Postfix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmorSquad {
    public static void Prefix(HUDMechArmorReadout __instance) {
      try {
        HUDMechArmorReadout readout = Traverse.Create(__instance).Property<HUDMechArmorReadout>("Readout").Value;
        Mech m = null;
        if (readout != null) { m = readout.DisplayedMech; }
        //Log.TWL(0, "HUDMechArmorReadout.UpdateMechStructureAndArmor Prefix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(m);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(HUDMechArmorReadout __instance) {
      try {
        HUDMechArmorReadout readout = Traverse.Create(__instance).Property<HUDMechArmorReadout>("Readout").Value;
        Mech m = null;
        if (readout != null) { m = readout.DisplayedMech; }
        //Log.TWL(0, "HUDMechArmorReadout.UpdateMechStructureAndArmor Postfix " + (m != null ? m.Description.Id : "null") + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetMechHitTable")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection), typeof(bool) })]
  public static class HitLocation_GetMechHitTable {
    //private static Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> GetMechHitTableCache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
    //public static void Postfix(HitLocation __instance, AttackDirection from, bool log, ref Dictionary<ArmorLocation, int> __result) {
    public static Dictionary<ArmorLocation, int> Get(HitLocation hitLocation, AttackDirection from, Mech target, Weapon w, int attackSequence, bool log) {
      //Mech mech = Thread.CurrentThread.currentMech();
      Thread.CurrentThread.pushActor(target);
      Dictionary<ArmorLocation, int> result = null;
      ICustomMech customMech = target as ICustomMech;
      if (customMech != null) { result = customMech.GetHitTable(from); } else { result = hitLocation.GetMechHitTable(from);  }
      Thread.CurrentThread.clearActor();
      return result;
    }
    public static void Postfix(AttackDirection from, ref Dictionary<ArmorLocation, int> __result) {
      //if (Thread.CurrentThread.isFlagSet("BuildClusterTables")) { return; }
      //if (Thread.CurrentThread.isFlagSet("RunHitDiagnostics")) { return; }
      if (Thread.CurrentThread.isFlagSet("CallOriginal_GetMechHitTable")) { return; }
      Mech mech = Thread.CurrentThread.currentMech();
      if (mech == null) {
        //Log.TWL(0, "illegal GetMechHitTable call :"+Environment.StackTrace);
        throw new Exception("illegal GetMechHitTable call");
      } else { 
        ICustomMech customMech = mech as ICustomMech;
        if (customMech != null) {
          __result = customMech.GetHitTable(from);
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatGameConstants))]
  [HarmonyPatch("BuildClusterTables")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatGameConstants_BuildClusterTables {
    public static void Prefix(CombatGameConstants __instance, CombatGameState combat) {
      try {
        Log.TWL(0, "CombatGameConstants.BuildClusterTables prefix");
        Thread.CurrentThread.SetFlag("CallOriginal_GetMechHitTable");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(CombatGameConstants __instance, CombatGameState combat) {
      try {
        Log.TWL(0, "CombatGameConstants.BuildClusterTables postfix");
        Thread.CurrentThread.ClearFlag("CallOriginal_GetMechHitTable");
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("NukeStructureLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(ChassisLocations), typeof(Vector3), typeof(DamageType) })]
  public static class Mech_NukeStructureLocation {
    public static void Prefix(Mech __instance, WeaponHitInfo hitInfo, int hitLoc, ChassisLocations location, Vector3 attackDirection, DamageType damageType) {
      try {
        Log.TWL(0, "Mech.NukeStructureLocation prefix " + __instance.Description.Id + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(__instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, int hitLoc, ChassisLocations location, Vector3 attackDirection, DamageType damageType) {
      try {
        Log.TWL(0, "Mech.NukeStructureLocation postfix" + __instance.Description.Id+" threadid:"+ Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("DamageLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
  public static class Mech_DamageLocation {
    public static void Prefix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalArmorDamage, float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      try {
        Log.TWL(0, "Mech.NukeStructureLocation prefix " + __instance.Description.Id + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.pushActor(__instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalArmorDamage, float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      try {
        Log.TWL(0, "Mech.NukeStructureLocation postfix" + __instance.Description.Id + " threadid:" + Thread.CurrentThread.ManagedThreadId);
        Thread.CurrentThread.clearActor();
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechStructureRules))]
  [HarmonyPatch("GetDependentLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations) })]
  public static class MechStructureRules_GetDependentLocation {
    public static void Postfix(ChassisLocations location, ref ChassisLocations __result) {
      try {
        Mech mech = Thread.CurrentThread.currentMech();
        if (mech != null) {
          UnitCustomInfo info = mech.GetCustomInfo();
          if (info != null) { if (info.SquadInfo.Troopers > 1) { __result = ChassisLocations.None; } }
          if (mech.NoDependentLocations()) { __result = ChassisLocations.None; }
          if (info.FakeVehicle) { __result = ChassisLocations.None; }
        }
        Log.TWL(0, "MechStructureRules.GetDependentLocation mech:" + (mech == null ? "null" : mech.Description.Id) + " " + location + "=>"+__result+" noDeps:" + (mech == null ? "null" : mech.NoDependentLocations().ToString()));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechStructureRules))]
  [HarmonyPatch("GetPassthroughLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation),typeof(AttackDirection) })]
  public static class MechStructureRules_GetPassthroughLocation {
    public static void Postfix(ArmorLocation location,AttackDirection attackDirection, ref ArmorLocation __result) {
      try {
        Mech mech = Thread.CurrentThread.currentMech();
        if (mech != null) {
          UnitCustomInfo info = mech.GetCustomInfo();
          if (info != null) { if (info.SquadInfo.Troopers > 1) { __result = ArmorLocation.None; } }
          if (mech.NoDependentLocations()) { __result = ArmorLocation.None; }
          if (mech.FakeVehicle()) { __result = ArmorLocation.None; }
        }
        Log.TWL(0, "MechStructureRules.GetPassthroughLocation mech:" + (mech == null ? "null" : mech.Description.Id) + " " + location + "=>" + __result + " noDeps:" + (mech == null ? "null" : mech.NoDependentLocations().ToString()));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MoveMultiplier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MoveMultiplier {
    public static bool Prefix(Mech __instance, ref float __result) {
      try {
        if (__instance.FakeVehicle()) { __result = 1f; return false; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        float num = 0.0f;
        if (__instance.IsOverheated) {
          num += __instance.Combat.Constants.MoveConstants.OverheatedMovePenalty;
        }
        List<ChassisLocations> legsDamageLevels = new List<ChassisLocations>();
        legsDamageLevels.Add(ChassisLocations.LeftLeg);
        legsDamageLevels.Add(ChassisLocations.RightLeg);
        if (info.ArmsCountedAsLegs) {
          legsDamageLevels.Add(ChassisLocations.LeftArm);
          legsDamageLevels.Add(ChassisLocations.RightArm);
        }
        float blackMod = info.LegDestroyedMovePenalty >= 0f ? info.LegDestroyedMovePenalty : __instance.Combat.Constants.MoveConstants.LegDestroyedPenalty;
        float redMod = info.LegDamageRedMovePenalty >= 0f ? info.LegDamageRedMovePenalty : __instance.Combat.Constants.MoveConstants.LegDamageRedPenalty;
        float yellowMod = info.LegDamageYellowMovePenalty >= 0f ? info.LegDamageYellowMovePenalty : __instance.Combat.Constants.MoveConstants.LegDamageYellowPenalty;
        foreach (ChassisLocations location in legsDamageLevels) {
          if (__instance.IsLocationDestroyed(location)) {
            num += blackMod;
          } else if (__instance.GetLocationDamageLevel(location) > LocationDamageLevel.Penalized) {
            num += redMod;
          } else {
            num += yellowMod;
          }
        }
        __result = Mathf.Max(__instance.Combat.Constants.MoveConstants.MinMoveSpeed, 1f - num);
        return false;
      }catch(Exception e) {
        Log.TWL(0,e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsLegged")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsLegged {
    public static int DestroyedLegsCount(this Mech mech) {
      int result = 0;
      if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg)) { ++result; }
      if (mech.IsLocationDestroyed(ChassisLocations.RightLeg)) { ++result; }
      UnitCustomInfo info = mech.GetCustomInfo();
      if (info != null) {
        if (info.ArmsCountedAsLegs) {
          if (mech.IsLocationDestroyed(ChassisLocations.LeftArm)) { ++result; }
          if (mech.IsLocationDestroyed(ChassisLocations.RightArm)) { ++result; }
        }
      }
      return result;
    }
    public static void Postfix(Mech __instance, ref bool __result) {
      try {
        if (__instance.TrooperSquad()) { __result = false; return; }
        if (__instance.FakeVehicle()) { __result = false; return; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return; }
        if (info.ArmsCountedAsLegs == false) { return; }
        __result = (__instance.DestroyedLegsCount() >= 2);
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsDead")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsDead {
    public static void Postfix(Mech __instance, ref bool __result) {
      try {
        if (__instance.HasHandledDeath) { return; }
        if (__instance.pilot.IsIncapacitated) { return; }
        if (__instance.pilot.HasEjected) { return; }
        if (__instance.HeadStructure <= 0.0f) { return; }
        if (__instance.CenterTorsoStructure <= 0.0f) { return; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info != null) {
          if (__result == false) {
            foreach (ChassisLocations location in info.LethalLocations) {
              if (__instance.GetCurrentStructure(location) <= 0f) { __result = true; return; }
            }
          }
          if (info.ArmsCountedAsLegs) {
            if ((__instance.DestroyedLegsCount() < 4)&&(__result == true)) { __result = false; return; }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyLegStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplyLegStructureEffects {
    public static void ApplyLegStructureEffects(this Mech mech, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      float LegDamageRelativeInstability = mech.Combat.Constants.PilotingConstants.LegDamageRelativeInstability;
      float LegDestroyRelativeInstability = 1f;
      UnitCustomInfo info = mech.GetCustomInfo();
      if(info != null) {
        LegDamageRelativeInstability = info.LegDamageRelativeInstability >= 0.0f ? info.LegDamageRelativeInstability : mech.Combat.Constants.PilotingConstants.LegDamageRelativeInstability;
        LegDestroyRelativeInstability = info.LegDestroyRelativeInstability >= 0.0f ? info.LegDestroyRelativeInstability : 1f;
      }
      mech.AddRelativeInstability(LegDamageRelativeInstability, StabilityChangeSource.BodyPartDamaged, sourceID);
      if (newDamageLevel == oldDamageLevel) { return; }
      if (newDamageLevel == LocationDamageLevel.Destroyed) {
        mech.AddRelativeInstability(LegDestroyRelativeInstability, StabilityChangeSource.LegDestroyed, sourceID);
      }
      mech.ResetPathing(false);
      if (mech.GameRep != null) {
        mech.GameRep.UpdateLegDamageAnimFlags(mech.LeftLegDamageLevel, mech.RightLegDamageLevel);
        //QuadRepresentation quadRepresentation = mech.GameRep.GetComponent<QuadRepresentation>();
        //if (quadRepresentation != null) { quadRepresentation.fLegsRep.LegsRep.UpdateLegDamageAnimFlags(mech.LeftArmDamageLevel,mech.RightArmDamageLevel); }
      }
    }
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        if (__instance.FakeVehicle()) { return false; }
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) { return false; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        __instance.ApplyLegStructureEffects(location, oldDamageLevel, newDamageLevel, sourceID, stackItemUID);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyArmStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplyArmStructureEffects {
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) { return false; }
        if (__instance.FakeVehicle()) { return false; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        if (info.ArmsCountedAsLegs == false) { return true; }
        __instance.ApplyLegStructureEffects(location, oldDamageLevel, newDamageLevel, sourceID, stackItemUID);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyHeadStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(WeaponHitInfo) })]
  public static class Mech_ApplyHeadStructureEffects {
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, WeaponHitInfo hitInfo) {
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) { return false; }
        if (__instance.FakeVehicle()) { return false; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplySideTorsoStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplySideTorsoStructureEffects {
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) { return false; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyCenterTorsoStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplyCenterTorsoStructureEffects {
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad != null) { return false; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("OnLocationDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(Vector3), typeof(WeaponHitInfo), typeof(DamageType) })]
  public static class Mech_OnLocationDestroyedRules {
    public static void UpdateMinStability(this Mech mech,WeaponHitInfo hitInfo) {
      if (mech.isHasStability() == false) { return; }
      float modValue = 0.0f;
      HashSet<ChassisLocations> legs = new HashSet<ChassisLocations>();
      legs.Add(ChassisLocations.LeftLeg);
      legs.Add(ChassisLocations.RightLeg);
      UnitCustomInfo info = mech.GetCustomInfo();
      float chassisModifier = 1f;
      if (info != null) {
        if (info.ArmsCountedAsLegs) {
          legs.Add(ChassisLocations.LeftArm);
          legs.Add(ChassisLocations.RightArm);
        }
        chassisModifier = info.LocDestroyedPermanentStabilityLossMod;
      }
      foreach (ChassisLocations location in Enum.GetValues(typeof(ChassisLocations))) {
        switch (location) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default: {
            if (mech.IsLocationDestroyed(location) == false) { continue; }
            if(mech.Combat.Constants.PilotingConstants.OnlyPermanentLossFromLegs == false) {
              modValue += mech.Combat.Constants.PilotingConstants.LocationDestroyedPermanentStabilityLoss * mech.MaxStability * chassisModifier;
            } else {
              if (legs.Contains(location)) {
                modValue += mech.Combat.Constants.PilotingConstants.LocationDestroyedPermanentStabilityLoss * mech.MaxStability * chassisModifier;
              }
            }
          }
          continue;
        }
      }
      mech.StatCollection.ModifyStat<float>(hitInfo.attackerId, hitInfo.stackItemUID, "MinStability", StatCollection.StatOperation.Set, modValue, -1, true);
      if (Traverse.Create(mech).Property<float>("_stability").Value < mech.MinStability) {
        Traverse.Create(mech).Property<float>("_stability").Value = mech.MinStability;
      }
      mech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new StabilityChangedMessage(mech.GUID));
    }
    public static void OnLocationDestroyedGeneral(this Mech instance, ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      if (location != ChassisLocations.Head && location != ChassisLocations.CenterTorso)
        instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)instance, new Text("{0} DESTROYED", new object[1]
        {
          (object) Mech.GetAbbreviatedChassisLocation(location)
        }), FloatieMessage.MessageNature.LocationDestroyed, true)));
      Log.TWL(0, "OnLocationDestroyedGeneral " + instance.DisplayName+" "+location);
      AttackDirector.AttackSequence attackSequence = instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) { attackSequence.FlagAttackDestroyedAnyLocation(instance.GUID); };
      UnitCustomInfo info = instance.GetCustomInfo();
      HashSet<ChassisLocations> legs = new HashSet<ChassisLocations>();
      bool armsAsLegs = false;
      legs.Add(ChassisLocations.RightLeg);
      legs.Add(ChassisLocations.LeftLeg);
      if (info != null) {
        if (info.ArmsCountedAsLegs) {
          legs.Add(ChassisLocations.RightArm);
          legs.Add(ChassisLocations.LeftArm);
          armsAsLegs = true;
        }
      }
      int destroyedLegsCount = instance.DestroyedLegsCount();
      Log.WL(0, "destroyedLegsCount:"+ destroyedLegsCount);
      if (legs.Contains(location)) {
        if ((armsAsLegs == false) || (destroyedLegsCount >= 2)) {
          instance.StatCollection.ModifyStat<float>(attackSequence == null ? "debug" : attackSequence.attacker.GUID, attackSequence == null ? -1 : attackSequence.attackSequenceIdx, "RunSpeed", StatCollection.StatOperation.Set, 0.0f, -1, true);
          instance.FlagForKnockdown();
        }
        if (attackSequence != null) {
          if (instance == attackSequence.attacker) {
            attackSequence.FlagAttackDestroyedAttackerLeg();
          } else if (attackSequence.allAffectedTargetIds.Contains(instance.GUID)) {
            attackSequence.FlagAttackDestroyedLeg(instance.GUID);
            if ((armsAsLegs == false) || (destroyedLegsCount >= 2)) attackSequence.FlagAttackCausedKnockdown(instance.GUID);
          }
        }
      }
      foreach (MechComponent allComponent in instance.allComponents) {
        if ((ChassisLocations)allComponent.Location == location) {
          if (allComponent.componentDef.Is<Flags>(out var f) && f.IsSet("ignore_damage")) { continue; }
          allComponent.DamageComponent(hitInfo, ComponentDamageLevel.Destroyed, false);
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("====@@@ Component Destroyed: {0}", (object)allComponent.Name));
          if (attackSequence != null) {
            Weapon weapon = allComponent as Weapon;
            AmmunitionBox ammoBox = allComponent as AmmunitionBox;
            attackSequence.FlagAttackScoredCrit(instance.GUID, weapon, ammoBox);
          }
        }
      }
      instance.UpdateMinStability(hitInfo);
      DeathMethod deathMethod = DeathMethod.NOT_SET;
      string reason = "";
      if (instance.FakeVehicle()) {
        deathMethod = DeathMethod.VehicleLocationDestroyed;
      }
      if (info != null) {
        if (info.LethalLocations.Contains(location)) {
          deathMethod = instance.FakeVehicle()?DeathMethod.VehicleLocationDestroyed:DeathMethod.CenterTorsoDestruction;
          reason = "Location Destroyed: " + location.ToString();
        }
      }
      if (deathMethod == DeathMethod.NOT_SET) {
        switch (location) {
          case ChassisLocations.Head:
          deathMethod = DeathMethod.HeadDestruction;
          reason = "Location Destroyed: " + location.ToString();
          break;
          case ChassisLocations.CenterTorso:
          deathMethod = DeathMethod.CenterTorsoDestruction;
          reason = "Location Destroyed: " + location.ToString();
          break;
          case ChassisLocations.LeftArm:
          case ChassisLocations.RightArm:
          case ChassisLocations.LeftLeg:
          case ChassisLocations.RightLeg: {
            if (legs.Contains(location) == false) { break; }
            if (((armsAsLegs == false) && (destroyedLegsCount >= 2)) || ((armsAsLegs == true) && (destroyedLegsCount >= 4))) {
              deathMethod = DeathMethod.LegDestruction;
              reason = "Location Destroyed: " + location.ToString();
              break;
            }
          }
          break;
        }
      }
      if (damageType == DamageType.AmmoExplosion && (location == ChassisLocations.CenterTorso || location == ChassisLocations.Head)) {
        deathMethod = DeathMethod.AmmoExplosion;
        reason = "Ammo Explosion: " + location.ToString();
      } else if (damageType == DamageType.ComponentExplosion && (location == ChassisLocations.CenterTorso || location == ChassisLocations.Head)) {
        deathMethod = DeathMethod.ComponentExplosion;
        reason = "Component Explosion: " + location.ToString();
      }
      if (deathMethod != DeathMethod.NOT_SET)
        instance.FlagForDeath(reason, deathMethod, damageType, (int)location, hitInfo.stackItemUID, hitInfo.attackerId, false);
      else if ((location == ChassisLocations.LeftTorso || location == ChassisLocations.RightTorso) && instance.Combat.Constants.PilotingConstants.InjuryFromSideTorsoDestruction) {
        Pilot pilot = instance.GetPilot();
        if (pilot != null)
          pilot.SetNeedsInjury(InjuryReason.SideTorsoDestroyed);
      }
      if (instance.GameRep != null) {
        instance.GameRep.PlayComponentDestroyedVFX((int)location, attackDirection);
      }
    }
    public static bool Prefix(Mech __instance, ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      Log.TWL(0, "Mech.OnLocationDestroyed "+__instance.DisplayName+" location:"+location);
      Thread.CurrentThread.pushActor(__instance);
      try {
        TrooperSquad squad = __instance as TrooperSquad;
        if (squad == null) {
          __instance.OnLocationDestroyedGeneral(location, attackDirection, hitInfo, damageType);
        } else {
          squad.OnLocationDestroyedSquad(location, attackDirection, hitInfo, damageType);
        }
        return false;
      }catch(Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
    public static void Postfix() {
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("EvaluateExpectedArmorFromAttackDirection")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  public static class Mech_EvaluateExpectedArmorFromAttackDirection {
    public static bool Prefix(Mech __instance, AttackDirection attackDirection, ref float __result) {
      Log.TWL(0, "Mech.EvaluateExpectedArmorFromAttackDirection " + __instance.DisplayName + " attackDirection:" + attackDirection);
      Thread.CurrentThread.pushActor(__instance);
      try {
        __result = 0.0f;
        Dictionary<ArmorLocation, int> mechHitTable = __instance.Combat.HitLocation.GetMechHitTableCustom(attackDirection, __instance, null, -1, false);
        if (mechHitTable != null) {
          float num2 = 0.0f;
          foreach (ArmorLocation key in mechHitTable.Keys) {
            int num3 = mechHitTable[key];
            num2 += (float)num3;
          }
          foreach (ArmorLocation key in mechHitTable.Keys) {
            int num3 = mechHitTable[key];
            float num4 = __instance.ArmorForLocation((int)key) * (float)num3 / num2;
            __result += num4;
          }
        }
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
    public static void Postfix() {
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(CombatDebugHUD))]
  [HarmonyPatch("RunHitDiagnostics")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_RunHitDiagnostics {
    public static void Prefix(CombatDebugHUD __instance) {
      Log.TWL(0, "CombatDebugHUD.RunHitDiagnostics ");
      Thread.CurrentThread.SetFlag("CallOriginal_GetMechHitTable");
    }
    public static void Postfix() {
      Thread.CurrentThread.ClearFlag("CallOriginal_GetMechHitTable");
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetPossibleHitLocations")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech) })]
  public static class HitLocation_GetPossibleHitLocations {
    public static void Prefix(HitLocation __instance, Vector3 attackerPosition, Mech target) {
      Thread.CurrentThread.pushActor(target);
    }
    public static void Postfix(HitLocation __instance, Vector3 attackerPosition, Mech target, ref List<int> __result) {
      try {
        TrooperSquad squad = target as TrooperSquad;
        if (squad != null) {
          __result.Clear();
          foreach (ArmorLocation aLoc in TrooperSquad.armorLocations) {
            ChassisLocations loc = MechStructureRules.GetChassisLocationFromArmorLocation(aLoc);
            LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
            if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
            if (squad.IsLocationDestroyed(loc)) { continue; }
            __result.Add((int)aLoc);
          }
          if (__result.Count == 0) { __result = null; }
        } else if(target.FakeVehicle()) { 
          __result.Clear();
          Dictionary<VehicleChassisLocations, int> vehicleHitTable = __instance.GetVehicleHitTable(__instance.GetAttackDirection(attackerPosition, (ICombatant)target), false);
          if (vehicleHitTable == null) { __result = null; }
          foreach (var vHit in vehicleHitTable) {
            ArmorLocation aLoc = vHit.Key.toFakeArmor();
            __result.Add((int)aLoc);
          }
          if (__result.Count == 0) { __result = null; }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetHitLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech), typeof(float), typeof(ArmorLocation), typeof(float) })]
  public static class HitLocation_GetHitLocation {
    public static VehicleChassisLocations GetHitLocationFakeVehicle(this HitLocation __instance, Vector3 attackPosition, ICombatant target, float randomRoll, VehicleChassisLocations bonusLocation, float bonusChanceMultiplier) {
      AttackDirection attackDirection = __instance.GetAttackDirection(attackPosition, target);
      if (attackDirection == AttackDirection.FromBack && target.StatCollection.GetValue<bool>("GuaranteeNextBackHit")) {
        target.StatCollection.Set<bool>("GuaranteeNextBackHit", false);
        return VehicleChassisLocations.Rear;
      }
      Dictionary<VehicleChassisLocations, int> vehicleHitTable = __instance.GetVehicleHitTable(attackDirection);
      return vehicleHitTable != null ? HitLocation.GetHitLocation<VehicleChassisLocations>(vehicleHitTable, randomRoll, bonusLocation, bonusChanceMultiplier) : VehicleChassisLocations.None;
    }
    public static bool Prefix(HitLocation __instance, Vector3 attackPosition, Mech target, float randomRoll, ArmorLocation calledShotLocation, float bonusChanceMultiplier, ref ArmorLocation __result) {
      Thread.CurrentThread.pushActor(target);
      try {
        if (target.FakeVehicle() == false) { return true; }
        __result = __instance.GetHitLocationFakeVehicle(attackPosition, target, randomRoll, calledShotLocation.toFakeVehicleChassis(), bonusChanceMultiplier).toFakeArmor();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(HitLocation __instance, Vector3 attackPosition, Mech target, float randomRoll, ArmorLocation calledShotLocation, float bonusChanceMultiplier) {
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetAdjacentHitLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech), typeof(float), typeof(ArmorLocation), typeof(float), typeof(float), typeof(ArmorLocation), typeof(float) })]
  public static class HitLocation_GetAdjacentHitLocation {
    public static VehicleChassisLocations GetAdjacentHitLocationFakeVehicle(this HitLocation __instance, Vector3 attackPosition, ICombatant target, float randomRoll, VehicleChassisLocations previousHitLocation, float originalMultiplier, float adjacentMultiplier, VehicleChassisLocations bonusLocation, float bonusChanceMultiplier) {
      AttackDirection attackDirection = __instance.GetAttackDirection(attackPosition, target);
      Dictionary<VehicleChassisLocations, int> hitTable = target.Combat.Constants.GetVehicleClusterTable(previousHitLocation, attackDirection);
      if (hitTable == null)
        return VehicleChassisLocations.None;
      if ((double)originalMultiplier > 1.00999999046326 || (double)adjacentMultiplier > 1.00999999046326) {
        Dictionary<VehicleChassisLocations, int> dictionary = new Dictionary<VehicleChassisLocations, int>();
        VehicleChassisLocations adjacentLocations = VehicleStructureRules.GetAdjacentLocations(previousHitLocation);
        foreach (KeyValuePair<VehicleChassisLocations, int> keyValuePair in hitTable) {
          if (keyValuePair.Key == previousHitLocation)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)originalMultiplier));
          else if ((adjacentLocations | keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)adjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
        hitTable = dictionary;
      }
      return HitLocation.GetHitLocation<VehicleChassisLocations>(hitTable, randomRoll, bonusLocation, bonusChanceMultiplier);
    }
    public static bool Prefix(HitLocation __instance, Vector3 attackPosition, Mech target, float randomRoll, ArmorLocation previousHitLocation, float originalMultiplier, float adjacentMultiplier, ArmorLocation bonusLocation, float bonusChanceMultiplier, ref ArmorLocation __result) {
      Thread.CurrentThread.pushActor(target);
      try {
        if (target.FakeVehicle() == false) { return true; }
        __result = __instance.GetAdjacentHitLocationFakeVehicle(attackPosition, target, randomRoll, previousHitLocation.toFakeVehicleChassis()
                          , originalMultiplier, adjacentMultiplier, bonusLocation.toFakeVehicleChassis(), bonusChanceMultiplier).toFakeArmor();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
    public static void Postfix(HitLocation __instance, Vector3 attackPosition, Mech target, float randomRoll, ArmorLocation previousHitLocation, float originalMultiplier, float adjacentMultiplier, ArmorLocation bonusLocation, float bonusChanceMultiplier) {
      Thread.CurrentThread.clearActor();
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyStructureStatDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(float), typeof(WeaponHitInfo) })]
  public static class Mech_ApplyStructureStatDamage {
    public static void Prefix(Mech __instance, ChassisLocations location, float damage, ref WeaponHitInfo hitInfo, ref LocationDamageLevel? __state) {
      __state = null;
      try {
        __state = __instance.GetLocationDamageLevel(location);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(Mech __instance, ChassisLocations location, float damage, ref WeaponHitInfo hitInfo, ref LocationDamageLevel? __state) {
      try {
        if (__state.HasValue == false) { return; }
        if (__instance.FakeVehicle() == false) { return; }
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return; }
        if (info.FakeVehicle == false) { return; }
        if (info.MechVehicleCrewLocation != location) { return; }
        __instance.pilot.SetNeedsInjury(InjuryReason.HeadHit);
        LocationDamageLevel locationDamageLevel = __instance.GetLocationDamageLevel(location);
        if (locationDamageLevel == __state.Value || locationDamageLevel != LocationDamageLevel.Destroyed) { return; }
        AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        __instance.pilot.LethalInjurePilot(__instance.Combat.Constants, hitInfo.attackerId, hitInfo.stackItemUID, true, DamageType.HeadShot, attackSequence.GetWeapon(hitInfo.attackGroupIndex, hitInfo.attackWeaponIndex), attackSequence.attacker);
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, "PILOT: LETHAL DAMAGE!", FloatieMessage.MessageNature.PilotInjury, true)));
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDInWorldScalingActorInfo))]
  [HarmonyPatch("GetWorldPos")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDInWorldScalingActorInfo_GetWorldPos {
    public static void Postfix(CombatHUDInWorldScalingActorInfo __instance, ref Vector3 __result) {
      try {
        if (__instance.DisplayedActor != null) {
          //Log.TWL(0, "CombatHUDInWorldScalingActorInfo.GetWorldPos " + __instance.anchorPosition + " " + __instance.DisplayedActor.PilotableActorDef.Description.Id + " currentPos:"+__instance.DisplayedActor.CurrentPosition+" result:"+__result+" diff:"+(__result - __instance.DisplayedActor.CurrentPosition));
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("FadeIn")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class PropertyBlockManager_FadeIn {
    public static IEnumerator FadeRoutine(PropertyBlockManager instance, bool fadeOut, float time) {
      Dictionary<MeshRenderer, Material[]> meshDict = new Dictionary<MeshRenderer, Material[]>();
      Dictionary<SkinnedMeshRenderer, Material[]> skinDict = new Dictionary<SkinnedMeshRenderer, Material[]>();
      List<Material> faderMaterial = new List<Material>();
      foreach (MeshRenderer key in instance.MeshRendererCache) {
        Material[] sharedMaterials = key.sharedMaterials;
        Material[] materialArray = new Material[sharedMaterials.Length];
        meshDict.Add(key, sharedMaterials);
        for (int index = 0; index < sharedMaterials.Length; ++index) {
          if ((UnityEngine.Object)sharedMaterials[index] != (UnityEngine.Object)null) {
            materialArray[index] = new Material(sharedMaterials[index]);
            materialArray[index].EnableKeyword("_STIPPLE_FADE");
            faderMaterial.Add(materialArray[index]);
          }
        }
        key.sharedMaterials = materialArray;
      }
      foreach (SkinnedMeshRenderer key in instance.SkinnedRendererCache) {
        Material[] sharedMaterials = key.sharedMaterials;
        Material[] materialArray = new Material[sharedMaterials.Length];
        skinDict.Add(key, sharedMaterials);
        for (int index = 0; index < sharedMaterials.Length; ++index) {
          if ((UnityEngine.Object)sharedMaterials[index] != (UnityEngine.Object)null) {
            materialArray[index] = new Material(sharedMaterials[index]);
            materialArray[index].EnableKeyword("_STIPPLE_FADE");
            faderMaterial.Add(materialArray[index]);
          }
        }
        key.sharedMaterials = materialArray;
      }
      float startValue = fadeOut ? 1f : 0.0f;
      float endValue = fadeOut ? 0.0f : 1f;
      PropertyBlockManager.PropertySetting fadeProp = new PropertyBlockManager.PropertySetting("_StippleAlpha", startValue);
      instance.AddProperty(ref fadeProp);
      instance.UpdateProperties();
      yield return (object)null;
      float timeElapsed = 0.0f;
      while ((double)timeElapsed < (double)time) {
        fadeProp.PropertyFloat = Mathf.Lerp(startValue, endValue, timeElapsed / time);
        instance.UpdateProperties();
        timeElapsed += Time.deltaTime;
        yield return (object)null;
      }
      if (fadeOut) {
        foreach (Renderer renderer in instance.MeshRendererCache)
          renderer.enabled = false;
        foreach (Renderer renderer in instance.SkinnedRendererCache)
          renderer.enabled = false;
      }
      instance.RemoveProperty(ref fadeProp);
      foreach (MeshRenderer key in instance.MeshRendererCache) {
        if (key == null) { continue; }
        if (meshDict.TryGetValue(key, out Material[] matArray)) {
          if (matArray == null) { continue; }
          key.sharedMaterials = matArray;
        }
      }
      foreach (SkinnedMeshRenderer key in instance.SkinnedRendererCache)
        key.sharedMaterials = skinDict[key];
      for (int index = 0; index < faderMaterial.Count; ++index)
        UnityEngine.Object.Destroy((UnityEngine.Object)faderMaterial[index]);
      meshDict.Clear();
      faderMaterial.Clear();
    }
    public static bool Prefix(PropertyBlockManager __instance, float length) {
      try {
        __instance.StartCoroutine(FadeRoutine(__instance, false, length));
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("FadeOut")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class PropertyBlockManager_FadeOut {
    public static bool Prefix(PropertyBlockManager __instance, float length) {
      try {
        __instance.StartCoroutine(PropertyBlockManager_FadeIn.FadeRoutine(__instance, true, length));
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
}