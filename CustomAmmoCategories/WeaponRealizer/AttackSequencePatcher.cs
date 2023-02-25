using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BattleTech;
using HarmonyLib;

namespace WeaponRealizer
{
    /// <summary>
    /// So we have some code like this:
    ///   float hitDamage = impactMessage.hitDamage;
    ///   float qualityMultiplier = this.Director.Combat.ToHit.GetBlowQualityMultiplier(impactMessage.hitInfo.hitQualities[hitIndex]);
    ///   float num = hitDamage * qualityMultiplier;
    /// We want something like this happening at the end
    ///   num = rand(variance/weapondamage) * num
    /// </summary>
    /*[HarmonyPatch(typeof(AttackDirector.AttackSequence), "OnAttackSequenceImpact")]
    static class AttackSequencePatcher
    {
        // ReSharper disable once UnusedMember.Local
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            var instructionsToInsert = new List<CodeInstruction>();
            var attackerField = AccessTools.Field(typeof(AttackDirector.AttackSequence), "attacker");
            var targetField = AccessTools.Field(typeof(AttackDirector.AttackSequence), "target");
            var targetFieldIndex = instructionList.FindIndex(instruction =>
                instruction.opcode == OpCodes.Ldfld && instruction.operand == targetField 
            );
            var insertionIndex = targetFieldIndex - 1;
            var calculatorMethod = AccessTools.Method(typeof(Calculator), nameof(Calculator.ApplyDamageModifiers),
                new Type[] {typeof(AbstractActor), typeof(ICombatant), typeof(Weapon), typeof(float)});

            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));                    // this
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, attackerField));       // this.attacker
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));                    // this
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, targetField));         // this.target
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));                 // weapon
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 8));                 // num
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, calculatorMethod)); // call out to our calc and put result on stack
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, 8));                 // num = _
            instructionList.InsertRange(insertionIndex, instructionsToInsert);
                                                                                  
            return instructionList;
        }
    }*/
}