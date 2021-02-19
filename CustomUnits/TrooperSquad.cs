using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using CustAmmoCategories;
using CustomComponents;
using Harmony;
using HBS.Collections;
using Localize;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnits {
#pragma warning disable CS0252
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyArmorStatDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation), typeof(float), typeof(WeaponHitInfo) })]
  public static class Mech_ApplyArmorStatDamage {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.ApplyArmorStatDamage Transpiler");
      MethodInfo targetMethod = typeof(Pilot).GetMethod("SetNeedsInjury", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(Mech_ApplyArmorStatDamage), nameof(SetNeedsInjury));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      return uInstructions;
    }
    public static void SetNeedsInjury(Pilot pilot, InjuryReason reason, Mech m) {
      Log.TWL(0, "Mech.ApplyArmorStatDamage.SetNeedsInjury " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { pilot.SetNeedsInjury(reason); };
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("setToolTipInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ArmorLocation) })]
  public static class CombatHUDMechTrayArmorHover_setToolTipInfoMech {
    public static bool Prefix(CombatHUDMechTrayArmorHover __instance, Mech mech, ArmorLocation location) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo " + mech.DisplayName + " " +location);
      return true;
    }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo Transpiler");
      MethodInfo targetMethod = typeof(Mech).GetMethod("GetLongArmorLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_setToolTipInfoMech), nameof(GetLongArmorLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_1, null));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Text GetLongArmorLocation(ArmorLocation location, Mech m) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.GetLongArmorLocation " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return Mech.GetLongArmorLocation(location); };
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("setToolTipInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(ArmorLocation) })]
  public static class CombatHUDMechTrayArmorHover_setToolTipInfoMechDef {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.setToolTipInfo Transpiler");
      MethodInfo targetMethod = typeof(Mech).GetMethod("GetLongArmorLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_setToolTipInfoMechDef), nameof(GetLongArmorLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_1, null));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Text GetLongArmorLocation(ArmorLocation location, MechDef m) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.GetLongArmorLocation " + m.Description.Id);
      UnitCustomInfo info = m.GetCustomInfo();
      if (info == null) { return Mech.GetLongArmorLocation(location); };
      if (info.SquadInfo.Troopers <= 1) { return Mech.GetLongArmorLocation(location); };
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch("UpdateMechDisplay")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDCalledShotPopUp_UpdateMechDisplay {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDCalledShotPopUp.UpdateMechDisplay Transpiler");
      MethodInfo targetMethod = typeof(Mech).GetMethod("GetLongArmorLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDCalledShotPopUp_UpdateMechDisplay), nameof(GetLongArmorLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldfld, typeof(CombatHUDCalledShotPopUp).GetField("displayedMech", BindingFlags.Instance | BindingFlags.NonPublic)));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Text GetLongArmorLocation(ArmorLocation location, Mech m) {
      Log.TWL(0, "CombatHUDCalledShotPopUp.GetLongArmorLocation " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return Mech.GetLongArmorLocation(location); };
      if(BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
  }
  [HarmonyPatch(typeof(CombatHUDAttackModeSelector))]
  [HarmonyPatch("DisplayedLocation")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(ArmorLocation) })]
  public static class CombatHUDAttackModeSelector_DisplayedLocation {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDAttackModeSelector.DisplayedLocation Transpiler");
      MethodInfo targetMethod = typeof(Mech).GetMethod("GetLongArmorLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDAttackModeSelector_DisplayedLocation), nameof(GetLongArmorLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Text GetLongArmorLocation(ArmorLocation location, CombatHUDAttackModeSelector sel) {
      ICombatant m = Traverse.Create(sel).Property<CombatHUD>("HUD").Value.SelectedTarget;
      Log.TWL(0, "CombatHUDAttackModeSelector.GetLongArmorLocation " + (m==null?"null":m.DisplayName));
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return Mech.GetLongArmorLocation(location); };
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
  }
  [HarmonyPatch(typeof(AttackStackSequence))]
  [HarmonyPatch("OnAttackBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackStackSequence_OnAttackBeginSquad {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "AttackStackSequence.OnAttackBegin Transpiler");
      MethodInfo targetMethod = typeof(Mech).GetMethod("GetLongArmorLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(AttackStackSequence_OnAttackBeginSquad), nameof(GetLongArmorLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find Mech.GetLongArmorLocation call");
        return uInstructions;
      }
      Log.WL(1, "found Mech.GetLongArmorLocation call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0, null));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Text GetLongArmorLocation(ArmorLocation location, AttackDirector.AttackSequence seq) {
      Log.TWL(0, "AttackStackSequence.GetLongArmorLocation " + (seq.chosenTarget.DisplayName));
      TrooperSquad squad = seq.chosenTarget as TrooperSquad;
      if (squad == null) { return Mech.GetLongArmorLocation(location); };
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
  }
  [HarmonyPatch(typeof(AIAttackEvaluator))]
  [HarmonyPatch("GetLocationDictionary")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech), typeof(Vector3), typeof(Quaternion) })]
  public static class AIAttackEvaluator_GetLocationDictionary {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "AIAttackEvaluator.GetLocationDictionary Transpiler");
      MethodInfo targetMethod = typeof(HitLocation).GetMethod("GetMechHitTable", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_OnPointerClick), nameof(GetMechHitTable));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find HitLocation.GetMechHitTable call");
        return uInstructions;
      }
      Log.WL(1, "found HitLocation.GetMechHitTable call " + MethodPos.ToString("X"));
      uInstructions[MethodPos - 1].opcode = OpCodes.Ldarg_1;
      uInstructions[MethodPos - 1].operand = null;
      return uInstructions;
    }
    public static Dictionary<ArmorLocation, int> GetMechHitTable(HitLocation hitLocation, AttackDirection from, Mech m) {
      Log.TWL(0, "AIAttackEvaluator_GetLocationDictionary " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return hitLocation.GetMechHitTable(from, false); }
      return squad.GetHitTable(from);
    }
  }
  [HarmonyPatch(typeof(DamageOrderUtility))]
  [HarmonyPatch("ApplyDamageToAllLocations")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(ICombatant), typeof(int), typeof(int), typeof(AttackDirection), typeof(DamageType) })]
  public static class DamageOrderUtility_ApplyDamageToAllLocations {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "DamageOrderUtility.ApplyDamageToAllLocations Transpiler");
      MethodInfo targetMethod = typeof(HitLocation).GetMethod("GetMechHitTable", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(DamageOrderUtility_ApplyDamageToAllLocations), nameof(GetMechHitTable));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find HitLocation.GetMechHitTable call");
        return uInstructions;
      }
      Log.WL(1, "found HitLocation.GetMechHitTable call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldloc_0));
      return uInstructions;
    }
    public static Dictionary<ArmorLocation, int> GetMechHitTable(HitLocation hitLocation,AttackDirection from, bool log, Mech m) {
      Log.TWL(0, "DamageOrderUtility_ApplyDamageToAllLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return hitLocation.GetMechHitTable(from, log); }
      return squad.GetHitTable(from);
    }
  }
  [HarmonyPatch(typeof(CombatHUDCalledShotPopUp))]
  [HarmonyPatch("ShownAttackDirection")]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  public static class CombatHUDCalledShotPopUp_ShownAttackDirection {
    public static bool Prefix(CombatHUDCalledShotPopUp __instance, AttackDirection value, ref AttackDirection ___shownAttackDirection, Mech ___displayedMech) {
      Log.TWL(0, "CombatHUDCalledShotPopUp.ShownAttackDirection");
      TrooperSquad squad = ___displayedMech as TrooperSquad;
      if (squad == null) { return true; }
      ___shownAttackDirection = AttackDirection.FromFront;
      __instance.FrontComponents.SetActive(false);
      __instance.RearComponents.SetActive(false);
      Traverse.Create(__instance).Property<Dictionary<ArmorLocation, int>>("currentHitTable").Value = squad.GetHitTable(value);
      if (__instance.MechArmorDisplay.HoveredArmor != ArmorLocation.None) {
        __instance.MechArmorDisplay.ClearHoveredArmor(__instance.MechArmorDisplay.HoveredArmor);
      }
      return false;
    }
    public static Dictionary<ArmorLocation, int> GetMechHitTable(HitLocation hitLocation, AttackDirection from, Mech m) {
      Log.TWL(0, "CombatHUDCalledShotPopUp_ShownAttackDirection " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return hitLocation.GetMechHitTable(from, false); }
      return squad.GetHitTable(from);
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTrayArmorHover))]
  [HarmonyPatch("OnPointerClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PointerEventData) })]
  public static class CombatHUDMechTrayArmorHover_OnPointerClick {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover.OnPointerClick Transpiler");
      MethodInfo targetMethod = typeof(HitLocation).GetMethod("GetMechHitTable", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(CombatHUDMechTrayArmorHover_OnPointerClick), nameof(GetMechHitTable));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          //uInstructions[t].operand = replacementMethod;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find HitLocation.GetMechHitTable call");
        return uInstructions;
      }
      Log.WL(1, "found HitLocation.GetMechHitTable call " + MethodPos.ToString("X"));
      uInstructions[MethodPos - 1].opcode = OpCodes.Ldloc_2;
      uInstructions[MethodPos - 1].operand = null;
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString(): "null"));
      }
      //uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldloc_2));
      return uInstructions;
    }
    public static Dictionary<ArmorLocation, int> GetMechHitTable(HitLocation hitLocation, AttackDirection from, Mech m) {
      Log.TWL(0, "CombatHUDMechTrayArmorHover_OnPointerClick " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return hitLocation.GetMechHitTable(from, false); }
      return squad.GetHitTable(from);
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch("UpdateMechStructureAndArmor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackDirection) })]
  public static class HUDMechArmorReadout_UpdateMechStructureAndArmorSquad {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "HUDMechArmorReadout.UpdateMechStructureAndArmor Transpiler");
      MethodInfo targetMethod = typeof(HitLocation).GetMethod("GetMechHitTable", BindingFlags.Instance | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(HUDMechArmorReadout_UpdateMechStructureAndArmorSquad), nameof(GetMechHitTable));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if (((uInstructions[t].opcode == OpCodes.Call) || (uInstructions[t].opcode == OpCodes.Callvirt)) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          uInstructions[t].opcode = OpCodes.Call;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find HitLocation.GetMechHitTable call");
        return uInstructions;
      }
      Log.WL(1, "found HitLocation.GetMechHitTable call " + MethodPos.ToString("X"));
      uInstructions[MethodPos - 1].opcode = OpCodes.Ldarg_0;
      uInstructions[MethodPos - 1].operand = null;
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldfld, typeof(HUDMechArmorReadout).GetField("displayedMech", BindingFlags.Instance | BindingFlags.NonPublic)));
      for (int pos = 0; pos < uInstructions.Count; ++pos) {
        Log.WL(2, pos.ToString("X") + " " + uInstructions[pos].opcode.ToString() + " " + (uInstructions[pos].operand != null ? uInstructions[pos].operand.ToString() : "null"));
      }
      return uInstructions;
    }
    public static Dictionary<ArmorLocation, int> GetMechHitTable(HitLocation hitLocation,AttackDirection from, Mech m) {
      //Log.TWL(0, "HUDMechArmorReadout_UpdateMechStructureAndArmorSquad " + (m == null?"null":m.DisplayName));
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return hitLocation.GetMechHitTable(from, false); }
      return squad.GetHitTable(from);
    }
  }
#pragma warning restore CS0252
  [HarmonyPatch(typeof(MessageCenter))]
  [HarmonyPatch("PublishMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class MessageCenter_PublishMessage {
    public static void Prefix(CombatHUDInWorldElementMgr __instance, MessageCenterMessage message) {
      try {
        if (message.MessageType != MessageCenterMessageType.FloatieMessage) { return; }
        FloatieMessage msg = message as FloatieMessage;
        if (msg == null) { return; }
        Log.TWL(0, "MessageCenter.PublishMessage " + msg.text+" nature:"+msg.nature+" GUID:"+msg.actingObjectGuid+" GUID:"+msg.affectedObjectGuid);
        Log.WL(0,Environment.StackTrace);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorFactory))]
  [HarmonyPatch("CreateMech")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechDef), typeof(PilotDef), typeof(TagSet), typeof(CombatGameState), typeof(string), typeof(string), typeof(HeraldryDef) })]
  public static class ActorFactory_CreateMech {
    public static bool Prefix(MechDef mDef, PilotDef pilot, TagSet additionalTags, CombatGameState combat, string uniqueId, string spawnerId, HeraldryDef customHeraldryDef, ref Mech __result) {
      try {
        Log.TWL(0, "ActorFactory.CreateMech "+mDef.Description.Id);
        UnitCustomInfo info = mDef.Chassis.GetCustomInfo();
        if (info == null) {
          Log.WL(0, "no info");
          return true;
        }
        if (info.SquadInfo.Troopers <= 1) {
          Log.WL(0, "Troopers:"+ info.SquadInfo.Troopers);
          return true;
        }
        __result = new TrooperSquad(mDef, pilot, additionalTags, uniqueId, combat, spawnerId, customHeraldryDef);
        combat.ItemRegistry.AddItem((ITaggedItem)__result);
        return false;
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  public class BaySquadReadoutAligner: MonoBehaviour {
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
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() {
      { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };
    private void SVGInit() {
      try {
        Log.TWL(0, "SquadReadoutAligner.SVGInit");
        for(int index = 0; index < READOUT_NAMES.Count; ++index) {
          int squadIndex = READOUT_INDEX_TO_SQUAD[index];
          string armorName = ARMOR_PREFIX + READOUT_NAMES[squadIndex];
          string armorOutlineName = ARMOR_PREFIX + READOUT_NAMES[squadIndex] + OUTLINE_SUFFIX;
          string structureName = STRUCTURE_PREFIX + READOUT_NAMES[squadIndex];
          RectTransform MechTray_Armor = this.gameObject.transform.FindRecursive(armorName) as RectTransform;
          RectTransform MechTray_ArmorOutline = this.gameObject.transform.FindRecursive(armorOutlineName) as RectTransform;
          RectTransform Mech_TrayInternal = this.gameObject.transform.FindRecursive(structureName) as RectTransform;
          Log.W(1, armorName + ":" + (MechTray_Armor == null ? "null" : "not null") + " " + armorOutlineName + ":" + (MechTray_ArmorOutline == null ? "null" : "not null") + " " + structureName+":"+ (Mech_TrayInternal == null ? "null" : "not null"));
          int row = (int)squadIndex / (int)2;
          Log.WL(1, "row:" + row + " col:" + (squadIndex % 2)+" squadIndex:"+squadIndex+" al:"+HUDMechArmorReadout.GetArmorLocationFromIndex(index,false,false) + " sl:" +HUDMechArmorReadout.GetChassisLocationFromIndex(index,false,false) );
          if (MechTray_Armor != null) {
            MechTray_Armor.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
            MechTray_Armor.pivot = new Vector2((squadIndex % 2 == 0 ? 0f : -1.1f), 1f + 1.1f * ((float)(row)));
            MechTray_Armor.anchoredPosition = Vector2.zero; MechTray_Armor.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE); MechTray_Armor.localScale = Vector3.one;
          }
          if (MechTray_ArmorOutline != null) {
            MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
            MechTray_ArmorOutline.pivot = new Vector2(0f, 1f);
            MechTray_ArmorOutline.anchoredPosition = Vector2.zero; MechTray_ArmorOutline.sizeDelta = new Vector2(SQUAD_ICON_SIZE, SQUAD_ICON_SIZE);
          }
          if (Mech_TrayInternal != null) {
            Mech_TrayInternal.gameObject.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
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
  public class CombatHUDSquadTrayArmorHover : EventTrigger {
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
  public class TargetHUDSquadArmorReadout : MonoBehaviour {
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
    public TargetHUDSquadArmorReadout() {
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
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TrayHUDSquadArmorReadout.ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = BaySquadReadoutAligner.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 2.1f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechArmor.Add(index, svg);
        SquadArmor.Add(index, squadSVG.GetComponent<SVGImage>());
        RectTransform outline = squadSVG.transform.FindRecursive(svg.name + BaySquadReadoutAligner.OUTLINE_SUFFIX) as RectTransform;
        outline.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
        outline.gameObject.name = outline.gameObject.name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TrayHUDSquadArmorReadout.ARMOR_PREFIX);
        outline.anchoredPosition = Vector2.zero;
        outline.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
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
        name = name.Replace(BaySquadReadoutAligner.STRUCTURE_PREFIX, TrayHUDSquadArmorReadout.STRUCTURE_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
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
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
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
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() {
      { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };
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
      SquadArmorComponentsTR.pivot = new Vector3(0f,1.15f);
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
        Log.WL(1, armorName + " " + armorOutlineName + " " + structureName + " " + index + " " + squadIndex+" hover:"+(hover==null?"null":hover.chassisIndex.ToString()));
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
        SquadTray_Armor.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        SquadTray_ArmorOutline.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        Squad_TrayInternal.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
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
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
    }
  }
  public class TrayHUDSquadArmorReadout: MonoBehaviour {
    public static readonly string ARMOR_PREFIX = "SquadTray_Armor";
    public static readonly string STRUCTURE_PREFIX = "Squad_TrayInternal";
    public static readonly float SQUAD_ICON_SIZE = 25f;
    public static Dictionary<int, int> READOUT_INDEX_TO_SQUAD = new Dictionary<int, int>() {
//      { 0, 0 }, { 1, 4 }, { 2, 2 }, { 3, 1 }, { 4, 3 }, { 5, 5 }, { 6, 6 }, { 7, 7 } };
      { 0, 0 }, { 1, 5 }, { 2, 3 }, { 3, 1 }, { 4, 2 }, { 5, 4 }, { 6, 7 }, { 7, 6 } };

    public HashSet<GameObject> toHide { get; set; }
    public HashSet<GameObject> toShow { get; set; }
    public Dictionary<int, SVGImage> SquadArmor { get; private set; }
    public Dictionary<int, SVGImage> SquadArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> SquadStructure { get; private set; }
    public Dictionary<int, SVGImage> MechArmor { get; private set; }
    public Dictionary<int, SVGImage> MechArmorOutline { get; private set; }
    public Dictionary<int, SVGImage> MechStructure { get; private set; }
    public TrayHUDSquadArmorReadout() {
      toHide = new HashSet<GameObject>();
      toShow = new HashSet<GameObject>();
      SquadArmor = new Dictionary<int, SVGImage>();
      SquadArmorOutline = new Dictionary<int, SVGImage>();
      SquadStructure = new Dictionary<int, SVGImage>();
      MechArmor = new Dictionary<int, SVGImage>();
      MechArmorOutline = new Dictionary<int, SVGImage>();
      MechStructure = new Dictionary<int, SVGImage>();
    }
    public void ShowMech(Mech mech) {
      foreach(GameObject go in toHide) {
        go.SetActive(true);
      }
      foreach (GameObject go in toShow) {
        go.SetActive(false);
      }
      HUDMechArmorReadout readout = this.gameObject.GetComponent<HUDMechArmorReadout>();
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = MechArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index,false,true);
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
      for (int index = 0; index < readout.ArmorOutline.Length; ++index) {
        readout.ArmorOutline[index] = SquadArmorOutline[index];
        if (readout.ArmorOutline[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.ArmorOutline[index].gameObject.SetActive(false); } else { readout.ArmorOutline[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Armor.Length; ++index) {
        readout.Armor[index] = SquadArmor[index];
        if (readout.Armor[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Armor[index].gameObject.SetActive(false); } else { readout.Armor[index].gameObject.SetActive(true); }
      }
      for (int index = 0; index < readout.Structure.Length; ++index) {
        readout.Structure[index] = SquadStructure[index];
        if (readout.Structure[index] == null) { continue; }
        ChassisLocations loc = HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true);
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { readout.Structure[index].gameObject.SetActive(false); } else { readout.Structure[index].gameObject.SetActive(true); }
      }
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
      Dictionary<int, CombatHUDMechTrayArmorHover> hovers = new Dictionary<int, CombatHUDMechTrayArmorHover>();
      Dictionary<int, CombatHUDTooltipHoverElement> tooltips = new Dictionary<int, CombatHUDTooltipHoverElement>();
      
      for (int index = 0; index < instance.ArmorOutline.Length; ++index) {
        SVGImage svg = instance.ArmorOutline[index];
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TrayHUDSquadArmorReadout.ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorOutlineIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = TrayHUDSquadArmorReadout.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechArmorOutline.Add(index,svg);
        SquadArmorOutline.Add(index, squadSVG.GetComponent<SVGImage>());
        hovers.Add(index, squadSVG.GetComponent<CombatHUDMechTrayArmorHover>());
        tooltips.Add(index, squadSVG.GetComponent<CombatHUDTooltipHoverElement>());
      }
      for (int index=0; index < instance.Armor.Length; ++index) {
        SVGImage svg = instance.Armor[index];
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.ARMOR_PREFIX, TrayHUDSquadArmorReadout.ARMOR_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadArmorIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = TrayHUDSquadArmorReadout.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechArmor.Add(index, svg);
        SquadArmor.Add(index, squadSVG.GetComponent<SVGImage>());
      }
      foreach (SVGImage svg in instance.ArmorRear) {
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
      }
      foreach (SVGImage svg in instance.ArmorOutlineRear) {
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
      }
      for (int index = 0; index < instance.Structure.Length; ++index) {
        SVGImage svg = instance.Structure[index];
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
        string name = svg.name;
        name = name.Replace(BaySquadReadoutAligner.STRUCTURE_PREFIX, TrayHUDSquadArmorReadout.STRUCTURE_PREFIX);
        GameObject squadSVG = GameObject.Instantiate(svg.gameObject);
        squadSVG.name = name;
        squadSVG.transform.SetParent(svg.transform.parent, false);
        squadSVG.transform.localPosition = svg.transform.localPosition;
        squadSVG.transform.localScale = svg.transform.localScale;
        squadSVG.GetComponent<SVGImage>().vectorGraphics = CustomSvgCache.get(Core.Settings.SquadStructureIcon, UnityGameInstance.BattleTechGame.DataManager);
        RectTransform squadSVGTR = squadSVG.GetComponent<RectTransform>();
        squadSVGTR.anchoredPosition = Vector2.zero;
        squadSVGTR.sizeDelta = new Vector2(TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE, TrayHUDSquadArmorReadout.SQUAD_ICON_SIZE);
        int squadIndex = TrayHUDSquadArmorReadout.READOUT_INDEX_TO_SQUAD[index];
        squadSVGTR.pivot = new Vector2((squadIndex % 2 == 1 ? -1.7f : -0.6f), 1.3f + 1.1f * ((float)((int)squadIndex / (int)2)));

        toShow.Add(squadSVG);
        MechStructure.Add(index, svg);
        SquadStructure.Add(index, squadSVG.GetComponent<SVGImage>());
        squadSVG.AddComponent<CombatHUDSquadTrayArmorHover>().Init(hovers[index], tooltips[index]);
      }
      foreach (SVGImage svg in instance.StructureRear) {
        if (svg == null) { continue; }
        toHide.Add(svg.gameObject);
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
        Log.TWL(0, "CombatHUDMechTrayArmorHover.OnPointerEnter "+__instance.gameObject.name);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Init")]
  [HarmonyPatch(new Type[] { typeof(CombatHUD), typeof(bool), typeof(bool), typeof(bool) })]
  public static class HUDMechArmorReadout_InitSquad {
    public static void Prefix(HUDMechArmorReadout __instance, CombatHUD HUD, bool useHoversForCalledShots, bool hideArmorWhenStructureDamage, bool showArmorAllOrNothing) {
      try {
        Transform squad_FrontArmor = __instance.gameObject.transform.FindRecursive("squad_FrontArmor");
        Transform mech_FrontArmor = __instance.gameObject.transform.FindRecursive("mech_FrontArmor");
        if ((squad_FrontArmor == null)&&(mech_FrontArmor != null)) {
          squad_FrontArmor = GameObject.Instantiate(mech_FrontArmor.gameObject).transform;
          squad_FrontArmor.gameObject.name = "squad_FrontArmor";
          squad_FrontArmor.SetParent(mech_FrontArmor.parent, false);
          squad_FrontArmor.localPosition = mech_FrontArmor.localPosition;
          squad_FrontArmor.localScale = mech_FrontArmor.localScale;
          squad_FrontArmor.gameObject.AddComponent<BaySquadReadoutAligner>().Init(__instance, mech_FrontArmor as RectTransform, true);
        }else if (__instance.gameObject.GetComponent<CombatHUDMechTray>() != null) {
          TrayHUDSquadArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TrayHUDSquadArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<TrayHUDSquadArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        }else if (__instance.gameObject.transform.parent.parent.gameObject.GetComponent<CombatHUDTargetingComputer>() != null) {
          TargetHUDSquadArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TargetHUDSquadArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<TargetHUDSquadArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        }else if (__instance.gameObject.transform.parent.parent.parent.gameObject.GetComponent<CombatHUDCalledShotPopUp>() != null) {
          CalledHUDSquadArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<CalledHUDSquadArmorReadout>();
          if (squadArmorReadout == null) {
            squadArmorReadout = __instance.gameObject.AddComponent<CalledHUDSquadArmorReadout>();
            squadArmorReadout.Instantine(__instance);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
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
          vehicle_FrontArmor.gameObject.GetComponent<VehicleReadoutAligner>().ResetUI();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HUDMechArmorReadout))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedMech")]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class HUDMechArmorReadout_DisplayedMechSquad {
    public static void Prefix(HUDMechArmorReadout __instance, Mech value) {
      if (value == null) { return; }
      TrooperSquad squad = value as TrooperSquad;
      TrayHUDSquadArmorReadout squadArmorReadout = __instance.gameObject.GetComponent<TrayHUDSquadArmorReadout>();
      if (squadArmorReadout != null) {
        if (squad != null) { squadArmorReadout.ShowSquad(squad); return; };
        squadArmorReadout.ShowMech(value);
      }
      TargetHUDSquadArmorReadout squadArmorReadoutTrg = __instance.gameObject.GetComponent<TargetHUDSquadArmorReadout>();
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
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedMechDef")]
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
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
      Transform mech_FrontArmor = readout.gameObject.transform.FindRecursive("mech_FrontArmor");
      Transform mech_RearArmor = readout.gameObject.transform.FindRecursive("mech_RearArmor");
      Transform squad_FrontArmor = readout.gameObject.transform.FindRecursive("squad_FrontArmor");
      Transform vehicle_FrontArmor = readout.gameObject.transform.FindRecursive("vehicle_FrontArmor");
      if (squad_FrontArmor != null) { squad_FrontArmor.gameObject.SetActive(true); }
      if (mech_FrontArmor != null) { mech_FrontArmor.gameObject.SetActive(false); }
      if (mech_RearArmor != null) { mech_RearArmor.gameObject.SetActive(false); }
      if (vehicle_FrontArmor != null) { vehicle_FrontArmor.gameObject.SetActive(false); }
      Log.TWL(0, "BayShowSquad");
      for (int index = 0; index < BaySquadReadoutAligner.READOUT_NAMES.Count; ++index) {
        int squadIndex = BaySquadReadoutAligner.READOUT_INDEX_TO_SQUAD[index];
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex] + BaySquadReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + BaySquadReadoutAligner.READOUT_NAMES[squadIndex];
        if (squad_FrontArmor == null) { continue; }
        Log.WL(1, squadIndex.ToString() + "/" + index + " " + armorName+" "+ armorOutlineName+" "+ structureName);

        RectTransform MechTray_Armor = squad_FrontArmor.FindRecursive(armorName) as RectTransform;
        RectTransform MechTray_ArmorOutline = squad_FrontArmor.FindRecursive(armorOutlineName) as RectTransform;
        RectTransform MechTray_Structure = squad_FrontArmor.FindRecursive(structureName) as RectTransform;
        if (MechTray_Armor != null) readout.Armor[index] = MechTray_Armor.gameObject.GetComponent<SVGImage>();
        if (MechTray_ArmorOutline != null) readout.ArmorOutline[index] = MechTray_ArmorOutline.gameObject.GetComponent<SVGImage>();
        if (MechTray_Structure != null) readout.Structure[index] = MechTray_Structure.gameObject.GetComponent<SVGImage>();
        if (def != null) {
          LocationDef locationDef = def.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true)));
          float armor = locationDef.MaxArmor;
          float structure = locationDef.InternalStructure;
          Log.W(1, squadIndex.ToString() + "/" + index + " " + armorName + " " + armorOutlineName + " " + structureName+" a:"+armor+"("+ HUDMechArmorReadout.GetArmorLocationFromIndex(index, false, true) + ")"+" s:"+structure+"("+HUDMechArmorReadout.GetChassisLocationFromIndex(index, false, true) +")");
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
      Transform mech_FrontArmor = readout.gameObject.transform.FindRecursive("mech_FrontArmor");
      Transform mech_RearArmor = readout.gameObject.transform.FindRecursive("mech_RearArmor");
      Transform squad_FrontArmor = readout.gameObject.transform.FindRecursive("squad_FrontArmor");
      Transform vehicle_FrontArmor = readout.gameObject.transform.FindRecursive("vehicle_FrontArmor");
      if (squad_FrontArmor != null) { squad_FrontArmor.gameObject.SetActive(false); }
      if (vehicle_FrontArmor != null) { vehicle_FrontArmor.gameObject.SetActive(true); }
      if (mech_FrontArmor != null) { mech_FrontArmor.gameObject.SetActive(false); }
      if (mech_RearArmor != null) { mech_RearArmor.gameObject.SetActive(false); }
      for (int index = 0; index < BaySquadReadoutAligner.READOUT_NAMES.Count; ++index) {
        string armorName = BaySquadReadoutAligner.ARMOR_PREFIX + VehicleReadoutAligner.READOUT_NAMES[index];
        string armorOutlineName = BaySquadReadoutAligner.ARMOR_PREFIX + VehicleReadoutAligner.READOUT_NAMES[index] + VehicleReadoutAligner.OUTLINE_SUFFIX;
        string structureName = BaySquadReadoutAligner.STRUCTURE_PREFIX + VehicleReadoutAligner.READOUT_NAMES[index];
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
        if (value.IsChassisFake()) {
          __instance.BayShowVehicle(value.Chassis);
          return;
        }
        UnitCustomInfo info = value.GetCustomInfo();
        if(info == null) {
          __instance.BayShowMech(value.Chassis);
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
  [HarmonyPatch(new Type[] { typeof(MechDef) })]
  public static class HUDMechArmorReadout_DisplayedChassisDef {
    public static void Prefix(HUDMechArmorReadout __instance, ChassisDef value) {
      try {
        if (value == null) {
          __instance.BayShowMech(value);
          return;
        }
        if (value.IsFake(value.Description.Id)) {
          __instance.BayShowVehicle(value);
          return;
        }
        UnitCustomInfo info = value.GetCustomInfo();
        if (info == null) {
          __instance.BayShowMech(value);
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
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StartPersistentAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StartPersistentAudioSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad == null) { return; }
        if (squad.MechReps.Contains(__instance)) { return; }
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
          sRep.Value.MechRep.StartPersistentAudio();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("GetHitPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_GetHitPositionSquad {
    public static void Postfix(MechRepresentation __instance, int location,ref Vector3 __result) {
      try {
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad == null) { return; }
        ArmorLocation armorLocation = (ArmorLocation)location;
        if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)){
          __result = trooperRep.MechRep.vfxCenterTorsoTransform.position;
        } else {
          __result = __instance.parentCombatant.CurrentPosition;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayPersistentDamageVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_PlayPersistentDamageVFXSquad {
    public static bool Prefix(MechRepresentation __instance, int location,ref List<string> ___persistentDmgList) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if(altReps != null) {
          altReps.PlayPersistentDamageVFX(location);
          return false;
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          ArmorLocation armorLocation = (ArmorLocation)location;
          if (___persistentDmgList.Count <= 0) { return true; }
          int index = UnityEngine.Random.Range(0, ___persistentDmgList.Count);
          string persistentDmg = ___persistentDmgList[index];
          ___persistentDmgList.RemoveAt(index);
          if (persistentDmg.Contains("Smoke")) {
            int num1 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_fire_small_internal, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          } else if (persistentDmg.Contains("Electrical")) {
            int num2 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_electrical_start, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          } else {
            int num3 = (int)WwiseManager.PostEvent<AudioEventList_mech>(AudioEventList_mech.mech_damage_burning_sparks_start, __instance.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          }
          Transform parentTransform = __instance.GetVFXTransform((int)ArmorLocation.CenterTorso);
          if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)) {
            parentTransform = trooperRep.MechRep.vfxCenterTorsoTransform;
          }
          __instance.PlayVFXAt(parentTransform, Vector3.zero, persistentDmg, true, Vector3.zero, false, -1f);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayComponentCritVFX")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class MechRepresentation_PlayComponentCritVFXSquad {
    public static bool Prefix(MechRepresentation __instance, int location, ref List<string> ___persistentCritList) {
      try {
        AlternateMechRepresentations altReps = __instance.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) {
          altReps.PlayComponentCritVFX(location);
          return false;
        }
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad != null) {
          ArmorLocation armorLocation = (ArmorLocation)location;
          if (___persistentCritList.Count <= 0) { return true; };
          int index = UnityEngine.Random.Range(0, ___persistentCritList.Count);
          string persistentCrit = ___persistentCritList[index];
          ___persistentCritList.RemoveAt(index);
          Transform parentTransform = __instance.GetVFXTransform((int)ArmorLocation.CenterTorso);
          if (squad.squadReps.TryGetValue(MechStructureRules.GetChassisLocationFromArmorLocation(armorLocation), out TrooperRepresentation trooperRep)) {
            parentTransform = trooperRep.MechRep.vfxCenterTorsoTransform;
          }
          __instance.PlayVFXAt(parentTransform, Vector3.zero, persistentCrit, true, Vector3.zero, false, -1f);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StartJumpjetAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StartJumpjetAudioSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad == null) { return; }
        if (squad.MechReps.Contains(__instance)) { return; }
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
          sRep.Value.MechRep.StartJumpjetAudio();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("StopJumpjetAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechRepresentation_StopJumpjetAudioSquad {
    public static void Postfix(MechRepresentation __instance) {
      try {
        TrooperSquad squad = __instance.parentMech as TrooperSquad;
        if (squad == null) { return; }
        if (squad.MechReps.Contains(__instance)) { return; }
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
          sRep.Value.MechRep.StopJumpjetAudio();
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ActorTwistSequence), "update")]
  public static class ActorTwistSequence_updateSquad {
    static void Prefix(ActorTwistSequence __instance, ref object __state) {
      __state = __instance.state();
    }
    static void Postfix(ActorTwistSequence __instance, float ___t, Quaternion ___startingRotation, Quaternion ___desiredRotation, ref object __state, PilotableActorRepresentation ___actorRep) {
      TrooperSquad squad = ___actorRep.parentActor as TrooperSquad;
      if (squad == null) { return; }
      Log.TWL(0, "ActorTwistSequence_updateSquad.update "+__state.ToString());
      if (__state.ToString() == ActorTwistSequence_update.TwistState_MeleeFacing.ToString()) {
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
          sRep.Value.MechRep.thisTransform.rotation = Quaternion.Lerp(___startingRotation, ___desiredRotation, ___t);
        }
      }else
      if (__state.ToString() == ActorTwistSequence_update.TwistState_RangedTwisting.ToString()) {
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) { continue; }
          sRep.Value.MechRep.currentTwistAngle = ___actorRep.currentTwistAngle;
          sRep.Value.MechRep.thisAnimator.SetFloat("Twist", ___actorRep.currentTwistAngle);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetCurrentStructure")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations) })]
  public static class Mech_GetCurrentStructureSquad {
    public static void Postfix(Mech __instance, ChassisLocations location, ref float __result) {
      try {
        if (__instance == null) { return; }
        if (__instance.MechDef == null) { return; }
        if (__instance.MechDef.Chassis == null) { return; }
        LocationDef locDef = __instance.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { __result = 0f; };
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_UpdateSquad {
    public static void Postfix(MechJumpSequence __instance, bool ___HasStarted, float ___t) {
      try {
        if (___HasStarted == false) { return; }
        if (__instance.OrdersAreComplete) { return; }
        TrooperSquad squad = __instance.OwningMech as TrooperSquad;
        if (squad == null) { return; }
        if(___t < 1.0f) {
          foreach(var sRep in squad.squadReps) {
            if (squad.IsLocationDestroyed(sRep.Key)) {
              sRep.Value.GameRep.transform.position = sRep.Value.deadLocation;
              sRep.Value.GameRep.transform.rotation = sRep.Value.deadRotation;
            } else {
              sRep.Value.GameRep.transform.position = __instance.MoverTransform.position + sRep.Value.delta;
              sRep.Value.GameRep.transform.rotation = __instance.MoverTransform.rotation;
            }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("CompleteJump")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_CompleteJump {
    public static void Prefix(MechJumpSequence __instance) {
      try {
        if (__instance.OrdersAreComplete) { return; }
        TrooperSquad squad = __instance.OwningMech as TrooperSquad;
        if (squad == null) { return; }
        foreach (var sRep in squad.squadReps) {
          if (squad.IsLocationDestroyed(sRep.Key)) {
            sRep.Value.GameRep.transform.position = sRep.Value.deadLocation;
            sRep.Value.GameRep.transform.rotation = sRep.Value.deadRotation;
          } else {
            Vector3 pos = __instance.FinalPos + sRep.Value.delta;
            pos.y = squad.Combat.MapMetaData.GetCellAt(pos).cachedHeight;
            //squad.Combat.MapMetaData.GetLerpedHeightAt(pos);
            sRep.Value.GameRep.transform.position = pos;
            sRep.Value.GameRep.transform.rotation = __instance.FinalHeading;
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(HitLocation))]
  [HarmonyPatch("GetPossibleHitLocations")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Mech) })]
  public static class HitLocation_GetPossibleHitLocations {
    public static void Postfix(HitLocation __instance, Vector3 attackerPosition, Mech target, ref List<int> __result) {
      try {
        TrooperSquad squad = target as TrooperSquad;
        if (squad == null) { return; }
        __result.Clear();
        foreach (ArmorLocation aLoc in TrooperSquad.armorLocations) {
          ChassisLocations loc = MechStructureRules.GetChassisLocationFromArmorLocation(aLoc);
          LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (squad.IsLocationDestroyed(loc)) { continue; }
          __result.Add((int)aLoc);
        }
        if (__result.Count == 0) { __result = null; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDActorDetailsDisplay))]
  [HarmonyPatch("RefreshInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDActorDetailsDisplay_RefreshInfo {
    public static void Postfix(CombatHUDActorDetailsDisplay __instance) {
      try {
        TrooperSquad squad = __instance.DisplayedActor as TrooperSquad;
        if (squad == null) { return; }
        UnitCustomInfo info = squad.GetCustomInfo();
        if (info == null) { return; }
        __instance.ActorWeightText.SetText("{0}: {1}", (object)"SQUAD", info.SquadInfo.weightClass);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("InjuryReasonDescription")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Pilot_InjuryReasonDescription {
    public static void Postfix(Pilot __instance, ref string __result) {
      try {
        if (__instance.ParentActor == null) { return; }
        TrooperSquad squad = __instance.ParentActor as TrooperSquad;
        if (squad == null) { return; }
        __result = "UNIT DESTROYED";
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class TrooperSquadDef {
    public int Troopers { get; set; }
    public int DeadUnitToHitMod { get; set; }
    public float UnitSize { get; set; }
    public WeightClass weightClass { get; set; }
    public Dictionary<string, ChassisLocations> Hardpoints { get; set; }
    public TrooperSquadDef() {
      Hardpoints = new Dictionary<string, ChassisLocations>();
      UnitSize = 1f;
      DeadUnitToHitMod = 0;
      weightClass = WeightClass.MEDIUM;
    }
  }
  public class TrooperRepresentation {
    public Vector3 delta { get; private set; }
    public GameObject GameRep { get;private set; }
    public MechRepresentation MechRep { get; private set; }
    public Vector3 deadLocation { get; private set; }
    public Quaternion deadRotation { get; private set; }
    private Animator ThisAnimator { get; set; }
    public int ForwardHash { get; set; }
    public int TurnHash { get; set; }
    public int IsMovingHash { get; set; }
    public int BeginMovementHash { get; set; }
    public int DamageHash { get; set; }
    public bool HasForwardParam { get; set; }
    public bool HasTurnParam { get; set; }
    public bool HasIsMovingParam { get; set; }
    public bool HasBeginMovementParam { get; set; }
    public bool HasDamageParam { get; set; }
    public ChassisLocations location { get; set; }
    public float TurnParam {
      set {
        if (!this.HasTurnParam)
          return;
        this.ThisAnimator.SetFloat(this.TurnHash, value);
      }
    }
    public float ForwardParam {
      set {
        if (!this.HasForwardParam)
          return;
        this.ThisAnimator.SetFloat(this.ForwardHash, value);
      }
    }
    public bool IsMovingParam {
      set {
        if (!this.HasIsMovingParam)
          return;
        this.ThisAnimator.SetBool(this.IsMovingHash, value);
      }
    }
    public bool BeginMovementParam {
      set {
        if (!this.HasBeginMovementParam)
          return;
        this.ThisAnimator.SetTrigger(this.BeginMovementHash);
      }
    }
    public float DamageParam {
      set {
        if (!this.HasDamageParam)
          return;
        this.ThisAnimator.SetFloat(this.DamageHash, value);
      }
    }
    public void HandleDeath(DeathMethod deathMethod, int location) {
      this.deadLocation = this.MechRep.transform.position;
      this.deadRotation = this.MechRep.transform.rotation;
      this.MechRep.HandleDeath(deathMethod, location);
    }
    public TrooperRepresentation(GameObject rep, Vector3 posDelta, ChassisLocations location) {
      this.location = location;
      this.delta = posDelta;
      this.GameRep = rep;
      this.MechRep = rep.GetComponent<MechRepresentation>();
      this.ThisAnimator = this.GameRep.GetComponent<Animator>();
      this.ForwardHash = Animator.StringToHash("Forward");
      this.TurnHash = Animator.StringToHash("Turn");
      this.IsMovingHash = Animator.StringToHash("IsMoving");
      this.BeginMovementHash = Animator.StringToHash("BeginMovement");
      this.DamageHash = Animator.StringToHash("Damage");
      AnimatorControllerParameter[] parameters = this.ThisAnimator.parameters;
      for (int index = 0; index < parameters.Length; ++index) {
        if (parameters[index].nameHash == this.ForwardHash)
          this.HasForwardParam = true;
        if (parameters[index].nameHash == this.TurnHash)
          this.HasTurnParam = true;
        if (parameters[index].nameHash == this.IsMovingHash)
          this.HasIsMovingParam = true;
        if (parameters[index].nameHash == this.BeginMovementHash)
          this.HasBeginMovementParam = true;
        if (parameters[index].nameHash == this.DamageHash)
          this.HasDamageParam = true;
      }
    }
  }
  public class TrooperSquad: Mech {
    public static readonly List<ChassisLocations> locations = new List<ChassisLocations>() { ChassisLocations.Head, ChassisLocations.CenterTorso, ChassisLocations.LeftTorso, ChassisLocations.RightTorso, ChassisLocations.LeftArm, ChassisLocations.RightArm, ChassisLocations.LeftLeg, ChassisLocations.RightLeg };
    public static readonly List<ArmorLocation> armorLocations = new List<ArmorLocation>() { ArmorLocation.Head, ArmorLocation.CenterTorso, ArmorLocation.LeftTorso, ArmorLocation.RightTorso, ArmorLocation.LeftArm, ArmorLocation.RightArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    public static readonly Dictionary<ChassisLocations, float> positions = new Dictionary<ChassisLocations, float>() {
      { ChassisLocations.CenterTorso, 0f }, { ChassisLocations.LeftTorso, 90f }, { ChassisLocations.RightTorso, 180f }, { ChassisLocations.LeftArm, -90f },
      { ChassisLocations.RightArm, 45f }, { ChassisLocations.LeftLeg, 135f }, { ChassisLocations.RightLeg, -45f }
    };
    public static readonly float SquadRadius = 5f;
    public Dictionary<ChassisLocations, TrooperRepresentation> squadReps;
    public HashSet<TrooperRepresentation> Reps;
    public HashSet<MechRepresentation> MechReps;
    public UnitCustomInfo info;
    public TrooperSquad(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
                  :base (mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef)
    {
      Reps = new HashSet<TrooperRepresentation>();
      MechReps = new HashSet<MechRepresentation>();
      info = mDef.GetCustomInfo();
      if (info == null) {
        throw new NullReferenceException("UnitCustomInfo is not defined for "+mDef.ChassisID);
      }
    }
    public void UpdateSpline(ActorMovementSequence __instance, Vector3 ___Forward) {
      Transform MoverTransform = Traverse.Create(__instance).Property<Transform>("MoverTransform").Value;
      foreach (var sRep in this.squadReps) {
        if (this.IsLocationDestroyed(sRep.Key)) {
          sRep.Value.GameRep.transform.position = sRep.Value.deadLocation;
          sRep.Value.GameRep.transform.rotation = sRep.Value.deadRotation;
        } else {
          Vector3 newPosition = MoverTransform.position + sRep.Value.delta;
          newPosition.y = __instance.owningActor.Combat.MapMetaData.GetCellAt(newPosition).cachedHeight;
          //__instance.owningActor.Combat.MapMetaData.GetLerpedHeightAt(newPosition);
          sRep.Value.GameRep.transform.LookAt(newPosition + ___Forward, Vector3.up);
          sRep.Value.GameRep.transform.position = newPosition;
        }
        //Log.WL(1, sRep.Value.GameRep.name + " pos:"+ newPosition+ " Velocity:"+ __instance.Velocity);
      }
    }
    public void OnLocationDestroyed(ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)this, new Text("UNIT DESTROYED"), FloatieMessage.MessageNature.LocationDestroyed, true)));
      AttackDirector.AttackSequence attackSequence = this.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) { attackSequence.FlagAttackDestroyedAnyLocation(this.GUID); };
      UnitCustomInfo info = this.GetCustomInfo();
      foreach (MechComponent allComponent in this.allComponents) {
        if ((ChassisLocations)allComponent.Location == location) {
          if (allComponent.componentDef.Is<CustomComponents.Flags>(out var f) && f.IsSet("ignore_damage")) { continue; }
          allComponent.DamageComponent(hitInfo, ComponentDamageLevel.Destroyed, false);
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("====@@@ Component Destroyed: {0}", (object)allComponent.Name));
          if (attackSequence != null) {
            Weapon weapon = allComponent as Weapon;
            AmmunitionBox ammoBox = allComponent as AmmunitionBox;
            attackSequence.FlagAttackScoredCrit(this.GUID, weapon, ammoBox);
          }
        }
      }
      bool hasNotDestroyedLocations = false;
      foreach(ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(loc) == false) { hasNotDestroyedLocations = true; break; }
      }
      DeathMethod deathMethod = DeathMethod.NOT_SET;
      string reason = "";
      if (hasNotDestroyedLocations == false) {
        deathMethod = DeathMethod.HeadDestruction;
        reason = "Squad destroyed";
        if (damageType == DamageType.AmmoExplosion) {
          deathMethod = DeathMethod.AmmoExplosion;
          reason = "Ammo Explosion: " + location.ToString();
        } else if (damageType == DamageType.ComponentExplosion) {
          deathMethod = DeathMethod.ComponentExplosion;
          reason = "Component Explosion: " + location.ToString();
        }
      }
      if (deathMethod != DeathMethod.NOT_SET) {
        this.FlagForDeath(reason, deathMethod, damageType, (int)location, hitInfo.stackItemUID, hitInfo.attackerId, false);
      } else {
        Pilot pilot = this.GetPilot();
        if (pilot != null) { pilot.SetNeedsInjury(InjuryReason.HeadHit); }
      }
      if (this.squadReps.TryGetValue(location, out TrooperRepresentation trooperRep)) {
        trooperRep.HandleDeath(DeathMethod.Unknown, (int)ChassisLocations.CenterTorso);
      }
    }
    public Transform GetAttachTransform(MechRepresentation gameRep, ChassisLocations location) {
      if ((UnityEngine.Object)gameRep == (UnityEngine.Object)null)
        throw new ArgumentNullException("GetAttachTransform requires a valid GameRep!");
      switch (location) {
        case ChassisLocations.LeftArm:
        return gameRep.LeftArmAttach;
        case ChassisLocations.RightArm:
        return gameRep.RightArmAttach;
        case ChassisLocations.LeftLeg:
        return gameRep.LeftLegAttach;
        case ChassisLocations.RightLeg:
        return gameRep.RightLegAttach;
        default:
        return gameRep.TorsoAttach;
      }
    }
    public override float SummaryStructureCurrent {
      get {
        if (this.IsDead) { return 0.0f; }
        float result = 0f;
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          result += this.GetCurrentStructure(location);
        }
        return result;
      }
    }
    public override float SummaryArmorCurrent {
      get {
        float result = 0f;
        foreach (ArmorLocation location in TrooperSquad.armorLocations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(location));
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          result += this.GetCurrentArmor(location);
        }
        return result;
      }
    }
    public override bool IsAnyStructureExposed {
      get {
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          if (this.GetCurrentStructure(location) <= 0f) { return true; }
        }
        return false;
      }
    }
    public override bool IsDead {
      get {
        if (this.HasHandledDeath) { return true; }
        if (this.pilot.IsIncapacitated) { return true; }
        foreach (ChassisLocations location in TrooperSquad.locations) {
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.InternalStructure <= 1f) && (locDef.MaxArmor <= 0f)) { continue; }
          if (this.GetCurrentStructure(location) > 0f) { return false; }          
        }
        return true;
      }
    }
    public override float MaxWalkDistance {
      get {
        return this.WalkSpeed;
      }
    }
    public override float MaxSprintDistance {
      get {
        return this.RunSpeed;
      }
    }
    public override bool CanSprint {
      get {
        return !this.HasFiredThisRound;
      }
    }
    public override float MaxBackwardDistance {
      get {
        return this.WalkSpeed;
      }
    }
    public override void VentCoolant() {
    }
    public override bool IsProne {
      get { return false; }
      protected set { return; }
    }
    public bool isHasWorkingJumpjets() {
      //Log.TWL(0, "TrooperSquad.isHasWorkingJumpjets " + this.DisplayName);
      HashSet<ChassisLocations> workingJumpsLocations = new HashSet<ChassisLocations>();
      foreach (Jumpjet component in jumpjets) {
        if (component.IsFunctional == false) { continue; }
        if (this.IsLocationDestroyed(component.mechComponentRef.MountedLocation)) { continue; }
        workingJumpsLocations.Add(component.mechComponentRef.MountedLocation);
      }
      //Log.WL(1, "workingJumpsLocations:" + workingJumpsLocations.Count);
      //foreach (ChassisLocations loc in workingJumpsLocations) { Log.WL(2, loc.ToString()); }
      foreach(ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(loc)) { continue; }
        //Log.WL(1, loc.ToString() + " "+ workingJumpsLocations.Contains(loc));
        if (workingJumpsLocations.Contains(loc) == false) { return false; }
      }
      return workingJumpsLocations.Count > 0;
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      List<int> result = new List<int>();
      foreach(ArmorLocation alocation in TrooperSquad.locations) {
        ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(location)) { continue; }
        result.Add((int)alocation);
      }
      return result;
    }
    public static readonly int ToHitTableSumm = 100;
    public virtual Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      Dictionary<ArmorLocation, int> result = new Dictionary<ArmorLocation, int>();
      HashSet<ArmorLocation> alocations = new HashSet<ArmorLocation>();
      foreach (ArmorLocation alocation in TrooperSquad.locations) {
        ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (this.IsLocationDestroyed(location)) { continue; }
        alocations.Add(alocation);
      }
      foreach(ArmorLocation aloc in alocations) {
        result.Add(aloc, ToHitTableSumm / alocations.Count);
      }
      return result.Count > 0 ? result : null;
    }

    public static Dictionary<ArmorLocation, int> GetHitTable(Mech mech, bool isCluster, ArmorLocation aLoc, AttackDirection from) {
      TrooperSquad squad = mech as TrooperSquad;
      if (squad == null) { return null; };
      return squad.GetHitTable(from);
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(AttackDirection.FromFront);
      return (int)HitLocation.GetHitLocation<ArmorLocation>(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier);
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(AttackDirection.FromFront);
      if ((double)originalMultiplier > 1.00999999046326 || (double)adjacentMultiplier > 1.00999999046326) {
        Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
        foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
          if ((int)keyValuePair.Key == previousHitLocation) {
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)originalMultiplier));
          } else {
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)adjacentMultiplier));
          }
        }
        hitTable = dictionary;
      }
      return (int)HitLocation.GetHitLocation<ArmorLocation>(hitTable, randomRoll, ArmorLocation.None, 0.0f);
    }
    protected override float EvaluateExpectedArmorFromAttackDirection(AttackDirection attackDirection) {
      float num1 = 0.0f;
      Dictionary<ArmorLocation, int> mechHitTable = this.GetHitTable(attackDirection);
      if (mechHitTable != null) {
        float num2 = 0.0f;
        foreach (ArmorLocation key in mechHitTable.Keys) {
          int num3 = mechHitTable[key];
          num2 += (float)num3;
        }
        foreach (ArmorLocation key in mechHitTable.Keys) {
          int num3 = mechHitTable[key];
          float num4 = this.ArmorForLocation((int)key) * (float)num3 / num2;
          num1 += num4;
        }
      }
      return num1;
    }
    public static Text GetLongArmorLocation(Mech m, ArmorLocation location) {
      Log.TWL(0, "TrooperSquad.GetLongArmorLocation " + m.DisplayName+" "+location);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return Mech.GetLongArmorLocation(location); };
      if (BaySquadReadoutAligner.ARMOR_TO_SQUAD.TryGetValue(location, out int index)) {
        return new Text("UNIT {0}", BaySquadReadoutAligner.ARMOR_TO_SQUAD[location]);
      }
      return new Text("UNIT");
    }
    public static HashSet<ArmorLocation> GetDFASelfDamageLocations(Mech m) {
      Log.TWL(0, "TrooperSquad.GetDFASelfDamageLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return null; };
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach(ArmorLocation aloc in TrooperSquad.armorLocations) {
        LocationDef locDef = m.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(aloc));
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public static HashSet<ArmorLocation> GetLandmineDamageLocations(Mech m) {
      Log.TWL(0, "TrooperSquad.GetLandmineDamageLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return null; };
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach (ArmorLocation aloc in TrooperSquad.armorLocations) {
        LocationDef locDef = m.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(aloc));
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public static HashSet<ArmorLocation> GetBurnDamageLocations(Mech m) {
      Log.TWL(0, "TrooperSquad.GetBurnDamageLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return null; };
      HashSet<ArmorLocation> result = new HashSet<ArmorLocation>();
      foreach (ArmorLocation aloc in TrooperSquad.armorLocations) {
        LocationDef locDef = m.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(aloc));
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add(aloc);
      }
      return result;
    }
    public static Dictionary<int, float> GetAOESpreadLocations(Mech m) {
      Log.TWL(0, "TrooperSquad.GetAOESpreadLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return null; };
      if (CustomAmmoCategories.SquadHitLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); }
      return CustomAmmoCategories.SquadHitLocations;
    }
    public static List<int> GetAOEPossibleHitLocations(Mech m, Vector3 attackPos) {
      Log.TWL(0, "TrooperSquad.GetAOEPossibleHitLocations " + m.DisplayName);
      TrooperSquad squad = m as TrooperSquad;
      if (squad == null) { return null; };
      List<int> result = new List<int>();
      foreach (ArmorLocation aloc in TrooperSquad.armorLocations) {
        LocationDef locDef = m.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(aloc));
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add((int)aloc);
      }
      return result;
    }
    public static float GetSquadSizeToHitMod(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      Log.TWL(0, "TrooperSquad.GetSquadSizeToHitMod " + target.DisplayName);
      TrooperSquad squad = target as TrooperSquad;
      if (squad == null) { return 0f; };
      UnitCustomInfo info = squad.GetCustomInfo();
      if (info == null) { return 0f; };
      int deadUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        if (squad.IsLocationDestroyed(loc)) { ++deadUnitsCount; }
      }
      return Mathf.Round((info.SquadInfo.DeadUnitToHitMod/(info.SquadInfo.Troopers - 1))*deadUnitsCount);
    }
    public static string GetSquadSizeToHitModName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      Log.TWL(0, "TrooperSquad.GetSquadSizeToHitMod " + target.DisplayName);
      TrooperSquad squad = target as TrooperSquad;
      if (squad == null) { return string.Empty; };
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return string.Format("UNITS {0}/{1}", liveUnitsCount, allUnitsCount);
    }
    public static float SquadSizeDamageMod(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (weapon.WeaponCategoryValue.IsMelee == false) { return 1f; }
      TrooperSquad squad = weapon.parent as TrooperSquad;
      if (squad == null) { return 1f; };
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return (float)liveUnitsCount / (float)allUnitsCount;
    }
    public static string SquadSizeDamageModName(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (weapon.WeaponCategoryValue.IsMelee == false) { return string.Empty; }
      TrooperSquad squad = weapon.parent as TrooperSquad;
      if (squad == null) { return string.Empty; };
      int allUnitsCount = 0;
      int liveUnitsCount = 0;
      foreach (ChassisLocations loc in TrooperSquad.locations) {
        LocationDef locDef = squad.MechDef.Chassis.GetLocationDef(loc);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        ++allUnitsCount;
        if (squad.IsLocationDestroyed(loc) == false) { ++liveUnitsCount; }
      }
      return string.Format("UNITS {0}/{1}", liveUnitsCount, allUnitsCount);
    }
    public override Text GetActorInfoFromVisLevel(VisibilityLevel visLevel) {      
      if (visLevel == VisibilityLevel.LOSFull || visLevel == VisibilityLevel.BlipGhost)
        return new Text("{0} - {1}", new object[2]
        {
          this.Combat.NetworkGameInstance != null && this.Combat.NetworkGameInstance.IsNetworkGameActive() && this.Combat.HostilityMatrix.IsLocalPlayerEnemy(this.team.GUID) ? (object) this.UnitName : (object) this.Nickname,
          (object) this.VariantName
        });
      if (visLevel >= VisibilityLevel.Blip4Maximum)
        return new Text("SQUAD, {0}t", new object[1]
        {
          (object) (this.MechDef.Chassis.Tonnage)
        });
      if (visLevel == VisibilityLevel.Blip1Type)
        return new Text("UNKNOWN SQUAD", (object[])Array.Empty<object>());
      return new Text("?", (object[])Array.Empty<object>());
    }
    public override Vector3 GetImpactPosition(AbstractActor attacker, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation) {
      return this.Combat.LOS.GetImpactPosition(attacker, (ICombatant)this, attackPosition, weapon, ref hitLocation, ref attackDirection, ref secondaryTargetId, ref secondaryHitLocation);
    }
    private void CreateBlankPrefabs(MechRepresentation gameRep, List<string> usedPrefabNames, ChassisLocations location) {
      List<string> componentBlankNames = MechHardpointRules.GetComponentBlankNames(usedPrefabNames, this.MechDef, location);
      Transform attachTransform = this.GetAttachTransform(gameRep, location);
      for (int index = 0; index < componentBlankNames.Count; ++index) {
        WeaponRepresentation component = this.Combat.DataManager.PooledInstantiate(componentBlankNames[index], BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<WeaponRepresentation>();
        component.Init((ICombatant)this, attachTransform, true, this.LogDisplayName, (int)location);
        this.GameRep.weaponReps.Add(component);
      }
    }
    protected override void InitStats() {
      base.InitStats();
      this.pilot.StatCollection.GetStatistic("Health").SetValue<int>(info.SquadInfo.Troopers);
    }
    public override void CheckPilotStatusFromAttack(string sourceID, int sequenceID, int stackItemID) {
      if (!this.IsPilotable)
        return;
      Pilot pilot = this.GetPilot();
      DamageType damageType = DamageType.Unknown;
      if (!pilot.IsIncapacitated && pilot.NeedsInjury) {
        switch (pilot.InjuryReason) {
          case InjuryReason.HeadHit:
          damageType = DamageType.HeadShot;
          break;
          case InjuryReason.AmmoExplosion:
          case InjuryReason.ComponentExplosion:
          damageType = DamageType.AmmoExplosion;
          break;
          case InjuryReason.Knockdown:
          damageType = DamageType.Knockdown;
          break;
          case InjuryReason.SideTorsoDestroyed:
          damageType = DamageType.SideTorso;
          break;
          default:
          damageType = DamageType.Combat;
          break;
        }
        pilot.InjurePilot(sourceID, stackItemID, 1, damageType, (Weapon)null, this.Combat.FindActorByGUID(sourceID));
        if (!pilot.IsIncapacitated) {
          if (this.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "friendly_warrior_injured", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          else
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "enemy_warrior_injured", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          IStackSequence sequence;
          if (pilot.Injuries == 0) {
            sequence = (IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("{0}: INJURY IGNORED", (object)pilot.InjuryReasonDescription), FloatieMessage.MessageNature.PilotInjury, true);
          } else {
            sequence = (IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("{0}: SQUAD INJURED", (object)pilot.InjuryReasonDescription), FloatieMessage.MessageNature.PilotInjury, true);
            AudioEventManager.SetPilotVOSwitch<AudioSwitch_dialog_dark_light>(AudioSwitch_dialog_dark_light.dark, this);
            AudioEventManager.PlayPilotVO(VOEvents.Pilot_TakeDamage, this, (AkCallbackManager.EventCallback)null, (object)null, true);
          }
          this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage(sequence));
        }
        pilot.ClearNeedsInjury();
      }
      if (!pilot.IsIncapacitated)
        return;
      this.FlagForDeath("All Pilots Killed", DeathMethod.PilotKilled, damageType, 1, stackItemID, sourceID, false);
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)this, Strings.T("ALL PILOTS INCAPACITATED!"), FloatieMessage.MessageNature.PilotInjury, true)));
      this.HandleDeath(sourceID);
    }
    public override void InitGameRep(Transform parentTransform) {
      UnitCustomInfo info = this.GetCustomInfo();
      if (info == null) { base.InitGameRep(parentTransform); return; }
      if (info.SquadInfo.Troopers <= 1) { base.InitGameRep(parentTransform); return; }
      List<MechComponent> orig_allComponents = this.allComponents;
      List<Weapon> orig_weapons = this.weapons;
      List<MechComponent> orig_supportComponents = this.supportComponents;
      this.allComponents = new List<MechComponent>();
      this.weapons = new List<Weapon>();
      this.supportComponents = new List<MechComponent>();
      Dictionary<MechComponent, ChassisLocations> orig_Locations = new Dictionary<MechComponent, ChassisLocations>();
      Log.TWL(0, "TrooperSquad.InitGameRep");
      base.InitGameRep(parentTransform);
      var identifier = this.MechDef.ChassisID;
      Vector3 sizeMultiplier = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
      Log.WL(1, $"{identifier}: {sizeMultiplier}");
      var originalLOSSourcePositions = Traverse.Create(this).Field("originalLOSSourcePositions").GetValue<Vector3[]>();
      var originalLOSTargetPositions = Traverse.Create(this).Field("originalLOSTargetPositions").GetValue<Vector3[]>();
      var newSourcePositions = MechResizer.MechResizer.ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = MechResizer.MechResizer.ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      Traverse.Create(this).Field("originalLOSSourcePositions").SetValue(newSourcePositions);
      Traverse.Create(this).Field("originalLOSTargetPositions").SetValue(newTargetPositions);
      Transform transformToScale = this.GameRep.thisTransform;
      Transform j_Root = this.GameRep.gameObject.transform.FindRecursive("j_Root");
      if (j_Root != null) { transformToScale = j_Root; }
      transformToScale.localScale = sizeMultiplier;
      SkinnedMeshRenderer[] meshes = this.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>();
      GameObject go = new GameObject("Empty");
      Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
      go.AddComponent<MeshRenderer>();
      if (meshes != null) {
        foreach (SkinnedMeshRenderer mesh in meshes) {
          mesh.sharedMesh = emptyMesh;
        }
      }
      List<GameObject> headlightReps = Traverse.Create(this.GameRep).Field<List<GameObject>>("headlightReps").Value;
      foreach(GameObject light in headlightReps) {
        GameObject.Destroy(light);
      }
      List<JumpjetRepresentation> jumpjetReps = Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
      foreach (JumpjetRepresentation jumpjetRep in jumpjetReps) {
        GameObject.Destroy(jumpjetRep.gameObject);
      }
      Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value = new List<JumpjetRepresentation>();
      headlightReps.Clear();
      GameObject.Destroy(go);
      this.allComponents = orig_allComponents;
      this.weapons = orig_weapons;
      this.supportComponents = orig_supportComponents;
      Log.WL(1, "allComponents:" + allComponents.Count);
      foreach (MechComponent component in allComponents) {
        Log.WL(2, component.defId + ":"+component.mechComponentRef.MountedLocation+":"+component.baseComponentRef.prefabName);
      }
      Log.WL(1, "weapons:" + weapons.Count);
      foreach (MechComponent component in weapons) {
        Log.WL(2, component.defId + ":" + component.mechComponentRef.MountedLocation + ":" + component.baseComponentRef.prefabName);
      }
      Log.WL(1, "supportComponents:" + supportComponents.Count);
      foreach (MechComponent component in supportComponents) {
        Log.WL(2, component.defId + ":" + component.mechComponentRef.MountedLocation + ":" + component.baseComponentRef.prefabName);
      }
      string prefabIdentifier = this.MechDef.Chassis.PrefabIdentifier;
      Log.WL(0, "Initing squad members reps:" + prefabIdentifier);
      squadReps = new Dictionary<ChassisLocations, TrooperRepresentation>();
      //squadReps.Add(ChassisLocations.Head, this._gameRep as MechRepresentation);
      for (int ti = 0; ti < info.SquadInfo.Troopers; ++ti) {
        if (ti >= locations.Count) { break; }
        GameObject squadGO = this.Combat.DataManager.PooledInstantiate(prefabIdentifier, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        squadGO.name = prefabIdentifier + "_" + locations[ti].ToString();
        MechRepresentation squadRep = squadGO.GetComponent<MechRepresentation>();
        squadGO.GetComponent<Animator>().enabled = true;
        squadRep.Init(this, parentTransform, false);
        if ((UnityEngine.Object)parentTransform == (UnityEngine.Object)null) {
          squadGO.transform.position = this.currentPosition;
          squadGO.transform.rotation = this.currentRotation;
        }
        transformToScale = squadRep.thisTransform;
        j_Root = squadRep.gameObject.transform.FindRecursive("j_Root");
        if (j_Root != null) { transformToScale = j_Root; }
        transformToScale.localScale = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
        Vector3 pos = Vector3.zero;
        if (ti != 0) {
          pos.x += TrooperSquad.SquadRadius * Mathf.Cos(Mathf.Deg2Rad * positions[locations[ti]]);
          pos.z += TrooperSquad.SquadRadius * Mathf.Sin(Mathf.Deg2Rad * positions[locations[ti]]);
        }
        pos.y = 0;
        TrooperRepresentation trooperRep = new TrooperRepresentation(squadGO, pos, locations[ti]);
        this.Reps.Add(trooperRep);
        this.MechReps.Add(squadRep);
        squadReps.Add(locations[ti], trooperRep);
        pos += squadGO.transform.position;
        pos.y = this.Combat.MapMetaData.GetCellAt(pos).cachedHeight;
        //this.Combat.MapMetaData.GetLerpedHeightAt(pos);
        squadGO.transform.position = pos;
        squadGO.transform.SetParent(this.GameRep.gameObject.transform, true);
        jumpjetReps = Traverse.Create(this.GameRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
        foreach (JumpjetRepresentation jrep in jumpjetReps) {
          foreach (ParticleSystem psys in jrep.jumpjetParticles) {
            psys.RegisterRestoreScale();
            var main = psys.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            Log.LogWrite(" " + psys.name + ":" + psys.main.scalingMode + "\n");
          }
        }
      }
      foreach (var trooper in squadReps) {
        List<string> usedPrefabNames = new List<string>();
        foreach (MechComponent allComponent in this.allComponents) {
          if (allComponent.mechComponentRef.MountedLocation != trooper.Key) { continue; }
          if (allComponent.componentType != ComponentType.Weapon) {
            allComponent.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, allComponent.baseComponentRef, this.MechDef.Chassis.PrefabBase, ChassisLocations.CenterTorso.ToString().ToLower(), ref usedPrefabNames);
            allComponent.baseComponentRef.hasPrefabName = true;
            if (!string.IsNullOrEmpty(allComponent.baseComponentRef.prefabName)) {
              Transform attachTransform = this.GetAttachTransform(trooper.Value.GameRep.GetComponent<MechRepresentation>(), ChassisLocations.CenterTorso);
              allComponent.InitGameRep(allComponent.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
              trooper.Value.GameRep.GetComponent<MechRepresentation>().miscComponentReps.Add(allComponent.componentRep);
            }
          }
        }
        foreach (Weapon weapon in this.Weapons) {
          if (weapon.mechComponentRef.MountedLocation != trooper.Key) { continue; }
          Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = ChassisLocations.CenterTorso;
          if (info.SquadInfo.Hardpoints.ContainsKey(weapon.WeaponCategoryValue.Name)) {
            Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = info.SquadInfo.Hardpoints[weapon.WeaponCategoryValue.Name];
          }
          weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, this.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
          weapon.baseComponentRef.hasPrefabName = true;
          if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
            Transform attachTransform = this.GetAttachTransform(trooper.Value.GameRep.GetComponent<MechRepresentation>(), weapon.mechComponentRef.MountedLocation);
            weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
            trooper.Value.GameRep.GetComponent<MechRepresentation>().weaponReps.Add(weapon.weaponRep);
            string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(this.MechDef, weapon.mechComponentRef);
            if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
              WeaponRepresentation component = this.Combat.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<WeaponRepresentation>();
              component.Init((ICombatant)this, attachTransform, true, this.LogDisplayName, weapon.Location);
              trooper.Value.GameRep.GetComponent<MechRepresentation>().weaponReps.Add(component);
            }
          }
          Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = trooper.Key;
        }
        foreach (MechComponent supportComponent in this.supportComponents) {
          if (supportComponent.mechComponentRef.MountedLocation != trooper.Key) { continue; }
          Weapon weapon = supportComponent as Weapon;
          if (weapon != null) {
            Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = ChassisLocations.CenterTorso;
            if (info.SquadInfo.Hardpoints.ContainsKey(weapon.WeaponCategoryValue.Name)) {
              Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = info.SquadInfo.Hardpoints[weapon.WeaponCategoryValue.Name];
            }
            weapon.baseComponentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.MechDef.Chassis.HardpointDataDef, weapon.baseComponentRef, this.MechDef.Chassis.PrefabBase, weapon.mechComponentRef.MountedLocation.ToString().ToLower(), ref usedPrefabNames);
            weapon.baseComponentRef.hasPrefabName = true;
            if (!string.IsNullOrEmpty(weapon.baseComponentRef.prefabName)) {
              Transform attachTransform = this.GetAttachTransform(weapon.mechComponentRef.MountedLocation);
              weapon.InitGameRep(weapon.baseComponentRef.prefabName, attachTransform, this.LogDisplayName);
              this.GameRep.miscComponentReps.Add((ComponentRepresentation)weapon.weaponRep);
            }
            Traverse.Create(weapon.mechComponentRef).Property<ChassisLocations>("MountedLocation").Value = trooper.Key;
          }
        }
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.CenterTorso);
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.LeftTorso);
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.RightTorso);
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.LeftArm);
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.RightArm);
        this.CreateBlankPrefabs(trooper.Value.GameRep.GetComponent<MechRepresentation>(), usedPrefabNames, ChassisLocations.Head);
        bool flag1 = this.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
        bool flag2 = this.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
        if (flag1 | flag2) {
          SkinnedMeshRenderer[] componentsInChildren = trooper.Value.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
          for (int index = 0; index < componentsInChildren.Length; ++index) {
            if (flag1)
              componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderUnfinishedMaterial;
            if (flag2)
              componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderImpostorMaterial;
          }
        }
        trooper.Value.GameRep.GetComponent<MechRepresentation>().RefreshEdgeCache();
        trooper.Value.GameRep.GetComponent<MechRepresentation>().FadeIn(1f);
      }
      Log.WL(1, "result:");
      foreach (var trooper in squadReps) {
        Log.WL(1, trooper.Key.ToString()+":"+trooper.Value.GameRep.name);
      }
      //squadReps.Add(ChassisLocations.Head, this._gameRep as MechRepresentation);
    }
  }
  public class TrooperRepresentationSimGame: MonoBehaviour {
    public MechRepresentationSimGame simGameRep;
    public ChassisLocations location;
    public SquadRepresentationSimGame parent;
    public void LoadDamageState(bool isDestroyed) {
      foreach (ChassisLocations loc in Enum.GetValues(typeof(ChassisLocations))) {
        switch (loc) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All: continue;
          default: simGameRep.CollapseLocation((int)loc, isDestroyed); continue;
        }
      }
    }
    public void LoadWeapons() {
      Log.TWL(0, "TrooperRepresentationSimGame.LoadWeapons");
      List<string> usedPrefabNames = new List<string>();
      DataManager ___dataManager = Traverse.Create(simGameRep).Field<DataManager>("dataManager").Value;
      MechRepresentationSimGame __instance = this.simGameRep;
      VTOLBodyAnimation bodyAnimation = __instance.VTOLBodyAnim();
      MechTurretAnimation MechTurret = __instance.gameObject.GetComponentInChildren<MechTurretAnimation>(true);
      QuadBodyAnimation quadBody = __instance.gameObject.GetComponentInChildren<QuadBodyAnimation>(true);
      Log.WL(1, "bodyAnimation:" + (bodyAnimation == null ? "null" : "not null"));
      for (int index = 0; index < __instance.mechDef.Inventory.Length; ++index) {
        MechComponentRef componentRef = __instance.mechDef.Inventory[index];
        if (location != componentRef.MountedLocation) { continue; }
        ChassisLocations Location = ChassisLocations.CenterTorso;//componentRef.MountedLocation;\
        WeaponDef weaponDef = componentRef.Def as WeaponDef;
        if (weaponDef != null) {
          if(this.parent.info.SquadInfo.Hardpoints.TryGetValue(weaponDef.WeaponCategoryValue.Name, out ChassisLocations loc)) {
            Location = loc;
          }
        }
        string MountedLocation = Location.ToString();
        bool correctLocation = false;
        if (__instance.mechDef.IsChassisFake()) {
          switch (Location) {
            case ChassisLocations.LeftArm: MountedLocation = VehicleChassisLocations.Front.ToString(); correctLocation = true; break;
            case ChassisLocations.RightArm: MountedLocation = VehicleChassisLocations.Rear.ToString(); correctLocation = true; break;
            case ChassisLocations.LeftLeg: MountedLocation = VehicleChassisLocations.Left.ToString(); correctLocation = true; break;
            case ChassisLocations.RightLeg: MountedLocation = VehicleChassisLocations.Right.ToString(); correctLocation = true; break;
            case ChassisLocations.Head: MountedLocation = VehicleChassisLocations.Turret.ToString(); correctLocation = true; break;
          }
        } else {
          correctLocation = true;
        }
        Log.WL(1, "Component " + componentRef.Def.GetType().ToString() + ":" + componentRef.GetType().ToString() + " id:" + componentRef.Def.Description.Id + " loc:" + MountedLocation);
        if (correctLocation) {
          Log.WL(2, "GetComponentPrefabName " + __instance.mechDef.Chassis.HardpointDataDef.ID + " base:" + __instance.mechDef.Chassis.PrefabBase + " loc:" + MountedLocation + " currPrefabName:" + componentRef.prefabName + " hasPrefab:" + componentRef.hasPrefabName + " hardpointSlot:" + componentRef.HardpointSlot);
          if (weaponDef != null) {
            string desiredPrefabName = string.Format("chrPrfWeap_{0}_{1}_{2}{3}", __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), componentRef.Def.PrefabIdentifier.ToLower(), weaponDef.WeaponCategoryValue.HardpointPrefabText);
            Log.WL(3, "desiredPrefabName:" + desiredPrefabName);
          } else {
            Log.WL(3, "");
          }
          //if (componentRef.hasPrefabName == false) {
          componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(__instance.mechDef.Chassis.HardpointDataDef, (BaseComponentRef)componentRef, __instance.mechDef.Chassis.PrefabBase, MountedLocation.ToLower(), ref usedPrefabNames);
          componentRef.hasPrefabName = true;
          Log.WL(3, "effective prefab name:" + componentRef.prefabName);

          //}
        }
        if (!string.IsNullOrEmpty(componentRef.prefabName)) {
          HardpointAttachType attachType = HardpointAttachType.None;
          Log.WL(1, "component:" + componentRef.ComponentDefID + ":" + Location);
          Transform attachTransform = __instance.GetAttachTransform(Location);
          CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(componentRef.prefabName);
          GameObject prefab = null;
          string prefabName = componentRef.prefabName;
          if (customHardpoint != null) {
            attachType = customHardpoint.attachType;
            prefab = ___dataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            if (prefab == null) {
              prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
            } else {
              prefabName = customHardpoint.prefab;
            }
          } else {
            Log.WL(1, componentRef.prefabName + " have no custom hardpoint");
            prefab = ___dataManager.PooledInstantiate(componentRef.prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          }
          if (prefab != null) {
            ComponentRepresentation component1 = prefab.GetComponent<ComponentRepresentation>();
            if (component1 == null) {
              Log.WL(1, prefabName + " have no ComponentRepresentation");
              if (customHardpoint != null) {
                component1 = prefab.AddComponent<WeaponRepresentation>();
                Log.LogWrite(1, "reiniting vfxTransforms\n");
                List<Transform> transfroms = new List<Transform>();
                for (int i = 0; i < customHardpoint.emitters.Count; ++i) {
                  Transform[] trs = component1.GetComponentsInChildren<Transform>();
                  foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[i]) { transfroms.Add(tr); break; } }
                }
                Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
                for (int i = 0; i < transfroms.Count; ++i) {
                  Log.LogWrite(2, transfroms[i].name + ":" + transfroms[i].localPosition + "\n");
                }
                if (transfroms.Count == 0) { transfroms.Add(prefab.transform); };
                component1.vfxTransforms = transfroms.ToArray();
                if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
                  Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
                  GameObject shaderPrefab = ___dataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
                  if (shaderPrefab != null) {
                    Log.LogWrite(1, "shader prefab found\n");
                    Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
                    if (shaderComponent != null) {
                      Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
                      MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                      foreach (MeshRenderer renderer in renderers) {
                        for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                          if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
                            Log.LogWrite(2, "keep original shader: " + renderer.gameObject.transform.name + "\n");
                            continue;
                          }
                          Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
                          renderer.materials[mindex].shader = shaderComponent.material.shader;
                          renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
                        }
                      }
                    }
                    GameObject.Destroy(shaderPrefab);
                  }
                }
              } else {
                component1 = prefab.AddComponent<ComponentRepresentation>();
              }
            }
            if (bodyAnimation != null) {
              Log.WL(1, "found VTOL body animation and vehicle component ref. Location:" + MountedLocation + " type:" + attachType);
              if (attachType == HardpointAttachType.None) {
                if ((bodyAnimation.bodyAttach != null) && (MountedLocation != VehicleChassisLocations.Turret.ToString())) { attachTransform = bodyAnimation.bodyAttach; }
              } else {
                AttachInfo attachInfo = bodyAnimation.GetAttachInfo(MountedLocation, attachType);
                Log.WL(2, "attachInfo:" + (attachInfo == null ? "null" : "not null"));
                if ((attachInfo != null) && (attachInfo.attach != null) && (attachInfo.main != null)) {
                  Log.WL(2, "attachTransform:" + (attachInfo.attach == null ? "null" : attachInfo.attach.name));
                  Log.WL(2, "mainTransform:" + (attachInfo.main == null ? "null" : attachInfo.main.name));
                  attachTransform = attachInfo.attach;
                  attachInfo.bayComponents.Add(component1);
                }
              }
            } else if (MechTurret != null) {
              Log.WL(1, "found mech turret:" + MountedLocation + " type:" + attachType);
              if (attachType == HardpointAttachType.Turret) {
                if(string.IsNullOrEmpty(customHardpoint.attachOverride) == false) {
                  if (MechTurret.attachPointsNames.TryGetValue(customHardpoint.attachOverride, out MechTurretAttachPoint attachPoint)) {
                    attachTransform = attachPoint.attachTransform;
                  }
                } else {
                  if (MechTurret.attachPoints.TryGetValue(Location, out List<MechTurretAttachPoint> attachPoints)) {
                    if (attachPoints.Count > 0) {
                      attachTransform = attachPoints[0].attachTransform;
                    }
                  }
                }
              }
            }
            if (component1 != null) {
              component1.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
              component1.gameObject.SetActive(true);
              component1.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
              component1.gameObject.name = componentRef.prefabName;
              __instance.componentReps.Add(component1);
              Log.WL(3, "Component representation spawned and inited. GameObject name:" + component1.gameObject.name + " Active:" + component1.gameObject.activeInHierarchy + " parent transform:" + component1.transform.parent.name);
            }
          }
          string mountingPointPrefabName = MechHardpointRules.GetComponentMountingPointPrefabName(__instance.mechDef, componentRef);
          if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
            ComponentRepresentation component2 = ___dataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null).GetComponent<ComponentRepresentation>();
            component2.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
            component2.gameObject.SetActive(true);
            component2.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
            component2.gameObject.name = mountingPointPrefabName;
            __instance.componentReps.Add(component2);
          }
        }
      }
      if (bodyAnimation != null) { bodyAnimation.ResolveAttachPoints(); };
      if (quadBody != null) {
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.CenterTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightTorso);
        __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.Head);
      }
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.LeftArm);
      __instance.CreateBlankPrefabs(usedPrefabNames, ChassisLocations.RightArm);
    }
    public TrooperRepresentationSimGame Init(ChassisLocations loc, SquadRepresentationSimGame parent) {
      this.location = loc;
      this.parent = parent;
      simGameRep = this.GetComponent<MechRepresentationSimGame>();
      return this;
    }
  }
  public class SquadRepresentationSimGame: MonoBehaviour {
    public Dictionary<ChassisLocations, TrooperRepresentationSimGame> squadSimGameReps { get; private set; }
    public UnitCustomInfo info { get; private set; }
    public MechDef mechDef { get; private set; }
    public void LoadDamageState() {
      foreach(var simRep in squadSimGameReps) {
        LocationDef locDef = mechDef.GetChassisLocationDef(simRep.Key);
        bool isDestroyed = locDef.InternalStructure <= 0f;
        //if (simRep.Key == ChassisLocations.CenterTorso) { isDestroyed = true; };
        simRep.Value.LoadDamageState(isDestroyed);
      }
    }
    public SquadRepresentationSimGame() {
      squadSimGameReps = new Dictionary<ChassisLocations, TrooperRepresentationSimGame>();
    }
    public void Init(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      this.info = mechDef.GetCustomInfo();
      this.mechDef = mechDef;
      foreach(var simRep in squadSimGameReps) {
        simRep.Value.simGameRep.Init(dataManager, mechDef, this.transform, heraldryDef);
        Transform transformToScale = simRep.Value.simGameRep.rootTransform;
        transformToScale.localScale = new Vector3(info.SquadInfo.UnitSize, info.SquadInfo.UnitSize, info.SquadInfo.UnitSize);
        Vector3 pos = Vector3.zero;
        if (simRep.Key != ChassisLocations.Head) {
          pos.x += TrooperSquad.SquadRadius * Mathf.Cos(Mathf.Deg2Rad * TrooperSquad.positions[simRep.Key]);
          pos.z += TrooperSquad.SquadRadius * Mathf.Sin(Mathf.Deg2Rad * TrooperSquad.positions[simRep.Key]);
        }
        pos.y = 0;
        transformToScale.localPosition = pos;
      }
    }
    public void Instantine(MechDef mechDef, List<GameObject> squadTroopers) {
      MechRepresentationSimGame mechRep = this.gameObject.GetComponent<MechRepresentationSimGame>();
      SkinnedMeshRenderer[] meshes = this.GetComponentsInChildren<SkinnedMeshRenderer>();
      GameObject go = new GameObject("Empty");
      Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
      go.AddComponent<MeshRenderer>();
      if (meshes != null) {
        foreach (SkinnedMeshRenderer mesh in meshes) {
          mesh.sharedMesh = emptyMesh;
        }
      }
      GameObject.Destroy(go);
      UnitCustomInfo info = mechDef.GetCustomInfo();
      for (int index = 0; index < squadTroopers.Count; ++index) {
        ChassisLocations location = TrooperSquad.locations[index];
        MechRepresentationSimGame squadRep = squadTroopers[index].GetComponent<MechRepresentationSimGame>();
        squadSimGameReps.Add(location, squadRep.gameObject.AddComponent<TrooperRepresentationSimGame>().Init(location,this));
      }
    }
  }
}