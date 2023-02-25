using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using UnityEngine;
/*
namespace CharlesB {
  public static class AttackDirector__AttackSequence_OnAttackSequenceResolveDamage_Patch {
    public static bool Prepare() {
      return false;
    }
    public static bool Prefix(ref MessageCenterMessage message, AttackDirector.AttackSequence __instance) {
      Log.CB.TWL(0, "hit prefix");
      var attackSequenceResolveDamageMessage = (AttackSequenceResolveDamageMessage)message;

      var hitInfo = attackSequenceResolveDamageMessage.hitInfo;
      if (hitInfo.attackSequenceId != __instance.id) return true;

      var attackGroupIndex = attackSequenceResolveDamageMessage.hitInfo.attackGroupIndex;
      var attackWeaponIndex = attackSequenceResolveDamageMessage.hitInfo.attackWeaponIndex;
      var weapon = __instance.GetWeapon(attackGroupIndex, attackWeaponIndex);

      if (__instance.meleeAttackType != MeleeAttackType.DFA) return true;

      var attacker = __instance.attacker;
      var rawDFASelfDamageValue = attacker.StatCollection.GetValue<float>("DFASelfDamage");
      var dfaSelfDamageValue = rawDFASelfDamageValue;
      if (Core.ModSettings.PilotingSkillDFASelfDamageMitigation) {
        // TODO: hook up water physics
        //                var superAlphaWaterFactor = 1f;
        //                if ((attacker as Mech).occupiedDesignMask.Description.Id == "DesignMaskWater")
        //                    superAlphaWaterFactor = 2f;

        var mitigation = Calculator.PilotingMitigation(attacker);
        var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
        dfaSelfDamageValue = rawDFASelfDamageValue - (rawDFASelfDamageValue * mitigation);
        Log.CB.TWL(0, $"dfa miss numbers\n" +
                             $"pilotSkill: {attacker.SkillPiloting}\n" +
                             $"mitigation: {mitigation}\n" +
                             $"rawDFASelfDamageValue: {rawDFASelfDamageValue}\n" +
                             $"mitigationPercent: {mitigationPercent}\n" +
                             $"dfaSelfDamageValue: {dfaSelfDamageValue}");
        attacker.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(attacker, $"Pilot Check: Avoided {mitigationPercent}% DFA Self-Damage!", FloatieMessage.MessageNature.Neutral, true)));
      }

      attacker.TakeWeaponDamage(attackSequenceResolveDamageMessage.hitInfo, (int)ArmorLocation.LeftLeg, weapon, dfaSelfDamageValue, 0, 0, DamageType.DFASelf);
      attacker.TakeWeaponDamage(attackSequenceResolveDamageMessage.hitInfo, (int)ArmorLocation.RightLeg, weapon, dfaSelfDamageValue, 0, 0, DamageType.DFASelf);
      if (AttackDirector.damageLogger.IsLogEnabled) {
        AttackDirector.damageLogger.Log($"@@@@@@@@ {attacker.DisplayName} takes {dfaSelfDamageValue} damage to its legs from the DFA attack!");
      }

      return true;
    }


    // What does this bullshit do? Glad you asked. The following code was in the orig method, and
    // the Transpiler patch replaces "this.meleeAttackType" bit with a bogus value defined above
    // so this will always evaluate to false so we can deal with the damage elsewhere in a different 
    // patch.
    //
    //    if (this.meleeAttackType == MeleeAttackType.DFA) {
    //        float value = this.attacker.StatCollection.GetValue<float>("DFASelfDamage");
    //        this.attacker.TakeWeaponDamage(attackSequenceResolveDamageMessage.hitInfo, 64, weapon, value, 0);
    //        this.attacker.TakeWeaponDamage(attackSequenceResolveDamageMessage.hitInfo, 128, weapon, value, 0);
    //        if (AttackDirector.damageLogger.IsLogEnabled)
    //        {
    //            AttackDirector.damageLogger.Log(string.Format("@@@@@@@@ {0} takes {1} damage to its legs from the DFA attack!", this.attacker.DisplayName, value));
    //        }
    //    }
    //
    // P.S. This is way too clever gotdamn

    public static int BogusAssMeleeAttackType = 200_020_002;

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      List<CodeInstruction> instructionList = instructions.ToList();
      var attackTypeFieldInfo = AccessTools.Field(typeof(AttackDirector.AttackSequence), "meleeAttackType");
      var replacementFieldInfo = AccessTools.Field(typeof(AttackDirector__AttackSequence_OnAttackSequenceResolveDamage_Patch), "BogusAssMeleeAttackType");
      var startIndex = instructionList.FindIndex(instruction => instruction?.operand == attackTypeFieldInfo);
      instructionList[startIndex].operand = replacementFieldInfo;
      return instructionList;
    }
  }

  // this is the function that gets called after dfa hit/miss has been resolved but before the turn is over
  [HarmonyPatch(typeof(MechDFASequence), "OnMeleeComplete")]
  public static class MechDFASequence_OnMeleeComplete_Patch {
    static void Postfix(ref MessageCenterMessage message, MechMeleeSequence __instance) {
      if (Core.ModSettings.DfaMissInstability) {
        Log.CB.TWL(0, $"checking for miss: {(message as AttackCompleteMessage).attackSequence.attackCompletelyMissed}");
        var attackCompleteMessage = (AttackCompleteMessage)message;
        var attacker = __instance.OwningMech;
        if (attackCompleteMessage.attackSequence.attackCompletelyMissed) {
          Log.CB.TWL(0, "We missed!");
          Log.CB.TWL(0, $"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
          var rawInstabilityToAdd = attacker.IsLegged
              ? Core.ModSettings.DfaMissInstabilityLeggedPercent
              : Core.ModSettings.DfaMissInstabilityPercent;
          var instabilityToAdd = rawInstabilityToAdd;
          if (Core.ModSettings.PilotingSkillInstabilityMitigation) {
            var mitigation = Calculator.PilotingMitigation(attacker);
            var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
            instabilityToAdd = rawInstabilityToAdd - rawInstabilityToAdd * mitigation;
            Log.CB.TWL(0, $"dfa miss numbers\n" +
                         $"pilotSkill: {attacker.SkillPiloting}\n" +
                         $"mitigation: {mitigation}\n" +
                         $"rawInstabilityToAdd: {rawInstabilityToAdd}\n" +
                         $"mitigationPercent: {mitigationPercent}\n" +
                         $"instabilityToAdd: {instabilityToAdd}");
            attacker.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(attacker, $"Pilot Check: Avoided {mitigationPercent}% Instability!", FloatieMessage.MessageNature.Neutral, true)));
          }

          attacker.AddRelativeInstability(instabilityToAdd, StabilityChangeSource.DFA, attacker.GUID);
          attacker.NeedsInstabilityCheck = true;
          attacker.CheckForInstability();
          Log.CB.TWL(0, $"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
          if (Core.ModSettings.AllowSteadyToKnockdownForMelee) {
            attacker.NeedsInstabilityCheck = true;
            attacker.CheckForInstability();
            Log.CB.TWL(0, $"flagged for knockdown? {attacker.IsFlaggedForKnockdown}");
          }
        }

        HandleFall.Say(attacker);
      }
    }
  }

  // this is called just before relinquishing control in a dfa attack
  [HarmonyPatch(typeof(MechDFASequence), "CompleteOrders", new Type[] { })]
  public static class MechDFASequence_CompleteOrders_Patch {
    static bool Prefix(MechMeleeSequence __instance) {
      // allow DFA to knock down target in single hit
      if (Core.ModSettings.AllowSteadyToKnockdownForMelee) {
        if (!__instance.MeleeTarget.IsDead) {
          var mech = __instance.MeleeTarget as Mech;
          Log.CB.TWL(0, $"opponent stability: {mech.CurrentStability}");
          var target = __instance.MeleeTarget as AbstractActor;
          if (target != null) {
            Log.CB.TWL(0, $"found target before first check. unsteady? {mech.IsUnsteady} : prone? {mech.IsProne}");
            target.NeedsInstabilityCheck = true;
            target.CheckForInstability();
            Log.CB.TWL(0, $"found target before second check. unsteady? {mech.IsUnsteady} : prone? {mech.IsProne}");
            target.NeedsInstabilityCheck = true;
            target.CheckForInstability();
            Log.CB.TWL(0, $"found target after second  check. downed? {mech.IsUnsteady} : prone? {mech.IsProne}");
            var attacker = __instance.OwningMech;
            target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
          }
        }
      }

      return true;
    }
  }

  [HarmonyPatch(typeof(MechMeleeSequence), "OnMeleeComplete")]
  public static class MechMeleeSequence_OnMeleeComplete_Patch {
    static void Postfix(ref MessageCenterMessage message, MechMeleeSequence __instance) {
      Log.CB.TWL(0, $"checking for miss: {(message as AttackCompleteMessage).attackSequence.attackCompletelyMissed}");

      if (Core.ModSettings.AttackMissInstability) {
        // Deal with attacker missing
        var attackCompleteMessage = (AttackCompleteMessage)message;
        var attacker = __instance.OwningMech;
        if (attackCompleteMessage.attackSequence.attackCompletelyMissed) {
          Log.CB.TWL(0, $"melee pre-miss stability: {attacker.CurrentStability}");
          var rawInstabilityToAdd = attacker.IsLegged
              ? Core.ModSettings.AttackMissInstabilityLeggedPercent
              : Core.ModSettings.AttackMissInstabilityPercent;
          var instabilityToAdd = rawInstabilityToAdd;
          if (Core.ModSettings.pilotingSkillInstabilityMitigation) {
            var mitigation = Calculator.PilotingMitigation(attacker);
            var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
            instabilityToAdd = rawInstabilityToAdd - rawInstabilityToAdd * mitigation;
            Log.CB.TWL(0, $"melee miss numbers\n" +
                         $"pilotSkill: {attacker.SkillPiloting}\n" +
                         $"mitigation: {mitigation}\n" +
                         $"rawInstabilityToAdd: {rawInstabilityToAdd}\n" +
                         $"mitigationPercent: {mitigationPercent}\n" +
                         $"instabilityToAdd: {instabilityToAdd}");
            attacker.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(attacker, $"Pilot Check: Avoided {mitigationPercent}% Instability!", FloatieMessage.MessageNature.Neutral, true)));
          }

          attacker.AddRelativeInstability(instabilityToAdd, StabilityChangeSource.Attack, attacker.GUID);
          Log.CB.TWL(0, $"melee post-miss stability: {attacker.CurrentStability}");
          attacker.NeedsInstabilityCheck = true;
          if (Core.ModSettings.AllowSteadyToKnockdownForMelee) {
            attacker.CheckForInstability();
            attacker.NeedsInstabilityCheck = true;
          }
        }
      }

      // Deal with target needing additional checks if we want to be able to knock over
      // mechs in a single round from one attack.
      if (Core.ModSettings.AllowSteadyToKnockdownForMelee) {
        if (!__instance.MeleeTarget.IsDead) {
          var target = __instance.MeleeTarget as AbstractActor;
          if (target != null) {
            target.NeedsInstabilityCheck = true;
            target.CheckForInstability();
            var attacker = __instance.OwningMech;
            HandleFall.Say(attacker);
            target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
          }
        }
      }
    }
  }

  [HarmonyPatch(typeof(MechMeleeSequence), "CompleteOrders", new Type[] { })]
  public static class MechMeleeSequence_CompleteOrders_Patch {
    static bool Prefix(MechMeleeSequence __instance) {
      // attacker second instability check during melee whiff
      var attacker = __instance.OwningMech;
      attacker.CheckForInstability();
      HandleFall.Say(attacker);  // only one I couldn't get to trigger, but I believe is correctly placed
      attacker.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);

      // second target instability check during melee hit if can go to ground in one hit
      if (Core.ModSettings.AllowSteadyToKnockdownForMelee) {
        if (!__instance.MeleeTarget.IsDead) {
          var target = __instance.MeleeTarget as AbstractActor;
          if (target != null) {
            target.NeedsInstabilityCheck = true;
            target.CheckForInstability();
            target.HandleKnockdown(__instance.RootSequenceGUID, attacker.GUID, Vector2.one, null);
          }
        }
      }

      Log.CB.TWL(0, $"did we fall? {attacker.IsProne}");
      Log.CB.TWL(0, $"did they fall? {__instance.MeleeTarget.IsProne}");
      return true;
    }
  }

}
*/