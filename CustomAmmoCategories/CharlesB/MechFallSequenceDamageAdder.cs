using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using UnityEngine;

/*
namespace CharlesB {
  [HarmonyPatch(typeof(MechFallSequence), "setState")]
  public static class MechFallSequenceDamageAdder {
    public static bool Prepare() { return false; }
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      if (!Core.ModSettings.FallingDamage) return instructions;

      var instructionList = instructions.ToList();
      var insertionIndex =
          instructionList.FindIndex(
              instruction => instruction.opcode == OpCodes.Ret                      // find early return
          ) + 2;
      var nopLabelReplaceIndex = insertionIndex - 1;
      instructionList[nopLabelReplaceIndex].opcode = OpCodes.Nop;                   // preserve jump label
      instructionList.Insert(insertionIndex, new CodeInstruction(OpCodes.Ldarg_0)); // replace code we just mangled

      var stateField = AccessTools.Field(typeof(MechFallSequence), "state");
      var owningMechGetter = AccessTools.Property(typeof(MechFallSequence), "OwningMech").GetGetMethod();
      var calculator = AccessTools.Method(
          typeof(MechFallSequenceDamageAdder), "ApplyFallingDamage", new Type[] { typeof(MechFallSequence), typeof(int), typeof(int) }
      );
      var damageMethodCalloutInstructions = new List<CodeInstruction>();
      damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));               // this
      damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));               // this
      damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldfld, stateField));     // this.state
      damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Ldarg_1));               // newState (Argument)
      damageMethodCalloutInstructions.Add(new CodeInstruction(OpCodes.Call, calculator));      // MechFallSequenceDamageAdder.DoDamage(this, this.state, newState)
      instructionList.InsertRange(insertionIndex, damageMethodCalloutInstructions);
      return instructionList;
    }
    private const int FinishedState = 3;

    private static readonly ArmorLocation[] possibleLocations = new[]
    {
            ArmorLocation.Head,
            ArmorLocation.CenterTorso,
            ArmorLocation.CenterTorsoRear,
            ArmorLocation.LeftTorso,
            ArmorLocation.LeftTorsoRear,
            ArmorLocation.RightTorso,
            ArmorLocation.RightTorsoRear,
            ArmorLocation.LeftArm,
            ArmorLocation.RightArm,
            ArmorLocation.RightLeg,
            ArmorLocation.LeftLeg
        };

    static void ApplyFallingDamage(MechFallSequence sequence, int oldState, int newState) {
      if (newState != FinishedState) return;
      var mech = sequence.OwningMech;
      if (mech.IsFlaggedForDeath || mech.IsDead) return; // TODO: maybe even the dead should take damage?
      var locationTakingDamage = possibleLocations[UnityEngine.Random.Range(0, possibleLocations.Length)];
      Log.CB.TWL(0, $"falling happened!\nlocation taking damage: {locationTakingDamage}");
      var rawFallingDamage = Core.ModSettings.FallingAmountDamagePerTon * mech.tonnage;
      var fallingDamageValue = rawFallingDamage;
      if (Core.ModSettings.PilotingSkillFallingDamageMitigation) {
        var mitigation = Calculator.PilotingMitigation(mech);
        var mitigationPercent = Mathf.RoundToInt(mitigation * 100);
        fallingDamageValue = rawFallingDamage - (mitigation * rawFallingDamage);
        Log.CB.TWL(0, $"falling damage numbers\n" +
                             $"pilotSkill: {mech.SkillPiloting}\n" +
                             $"mitigation: {mitigation}\n" +
                             $"rawFallingDamage: {rawFallingDamage}\n" +
                             $"mitigationPercent: {mitigationPercent}\n" +
                             $"fallingDamageValue: {fallingDamageValue}");
        mech.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(mech, $"Pilot Check: Avoided {mitigationPercent}% Falling Damage!", FloatieMessage.MessageNature.Neutral, true)));
      }
      mech.DEBUG_DamageLocation(locationTakingDamage, fallingDamageValue, mech, damageType: DamageType.Knockdown);
      if (AttackDirector.damageLogger.IsLogEnabled) {
        AttackDirector.damageLogger.Log($"@@@@@@@@ {mech.DisplayName} takes {fallingDamageValue} damage to its {Mech.GetLongArmorLocation(locationTakingDamage)} from falling!");
      }
    }
  }
}*/