using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.UI;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using HBS.Math;

namespace CustomAmmoCategoriesPatches
{
    [HarmonyPatch(typeof(AIUtil))]
    [HarmonyPatch("UnitHasLOFToTargetFromPosition")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(CombatGameState), typeof(Vector3) })]
    public static class AIUtil_UnitHasLOFToTargetFromPosition
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AIUtil_UnitHasLOFToTargetFromPosition), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(AIUtil))]
    [HarmonyPatch("UnitHasLOFToUnit")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor), typeof(CombatGameState) })]
    public static class AIUtil_UnitHasLOFToUnit
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AIUtil_UnitHasLOFToUnit), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToUnit IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(AIRoleAssignment))]
    [HarmonyPatch("EvaluateSniper")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class AIRoleAssignment_EvaluateSniper
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AIRoleAssignment_EvaluateSniper), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AIRoleAssignment_EvaluateSniper IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("GetLongestRangeWeapon")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
    public static class AbstractActor_GetLongestRangeWeapon
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AbstractActor_GetLongestRangeWeapon), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_GetLongestRangeWeapon IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("HasIndirectLOFToTargetUnit")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(bool) })]
    public static class AbstractActor_HasIndirectLOFToTargetUnit
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AbstractActor_HasIndirectLOFToTargetUnit), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_HasIndirectLOFToTargetUnit IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("HasLOFToTargetUnit")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Weapon) })]
    public static class AbstractActor_HasLOFToTargetUnit
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(AbstractActor_HasLOFToTargetUnit), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_HasLOFToTargetUnit IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(HostileDamageFactor))]
    [HarmonyPatch("expectedDamageForShooting")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool), typeof(bool) })]
    public static class HostileDamageFactor_expectedDamageForShooting
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(HostileDamageFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get HostileDamageFactor_expectedDamageForShooting IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(MultiAttack))]
    [HarmonyPatch("FindWeaponToHitTarget")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(List<Weapon>), typeof(ICombatant) })]
    public static class MultiAttack_FindWeaponToHitTarget
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(MultiAttack_FindWeaponToHitTarget), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_FindWeaponToHitTarget IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(MultiAttack))]
    [HarmonyPatch("GetExpectedDamageForMultiTargetWeapon")]
    [HarmonyPatch(MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(bool), typeof(Weapon), typeof(int) })]
    public static class MultiAttack_GetExpectedDamageForMultiTargetWeapon
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(MultiAttack_GetExpectedDamageForMultiTargetWeapon), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_GetExpectedDamageForMultiTargetWeapon IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(MultiAttack))]
    [HarmonyPatch("PartitionWeaponListToKillTarget")]
    [HarmonyPatch(MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(List<Weapon>), typeof(ICombatant), typeof(ICombatant), typeof(float) })]
    public static class MultiAttack_PartitionWeaponListToKillTarget
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(MultiAttack_PartitionWeaponListToKillTarget), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_PartitionWeaponListToKillTarget IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(MultiAttack))]
    [HarmonyPatch("ValidateMultiAttackOrder")]
    [HarmonyPatch(MethodType.Normal)]
    //[HarmonyPatch(new Type[] { typeof(MultiTargetAttackOrderInfo), typeof(AbstractActor) })]
    public static class MultiAttack_ValidateMultiAttackOrder
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(MultiAttack_ValidateMultiAttackOrder), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_ValidateMultiAttackOrder IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(PreferExposedAlonePositionalFactor))]
    [HarmonyPatch("InitEvaluationForPhaseForUnit")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
    public static class PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(PreferFiringSolutionWhenExposedAllyPositionalFactor))]
    [HarmonyPatch("EvaluateInfluenceMapFactorAtPosition")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(float), typeof(MoveType), typeof(PathNode) })]
    public static class PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(PreferLethalDamageToRearArcFromHostileFactor))]
    [HarmonyPatch("expectedDamageForShooting")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool) })]
    public static class PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(PreferNotLethalPositionFactor))]
    [HarmonyPatch("expectedDamageForShooting")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(MoveType) })]
    public static class PreferNotLethalPositionFactor_expectedDamageForShooting
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(PreferNotLethalPositionFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get PreferNotLethalPositionFactor_expectedDamageForShooting IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(ToHit))]
    [HarmonyPatch("GetAllModifiers")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
    public static class ToHit_GetAllModifiers
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiers), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiers IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }

        public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot,ref float __result)
        {
            bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
            float num = __result;
            if (flag == false)
            {
                float directFireModifier = CustomAmmoCategories.getDirectFireModifier(weapon);
                CustomAmmoCategoriesLog.Log.LogWrite(attacker.DisplayName+" has LOS on "+target.DisplayName+ ". Apply DirectFireModifier "+directFireModifier+"\n");
                num += CustomAmmoCategories.getDirectFireModifier(weapon);
            }
            CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if ((double)num < 0.0 && !combat.Constants.ResolutionConstants.AllowTotalNegativeModifier)
            {
                num = 0.0f;
            }
            __result = num;
            return;
        }
    }
    [HarmonyPatch(typeof(ToHit))]
    [HarmonyPatch("GetAllModifiersDescription")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
    public static class ToHit_GetAllModifiersDescription
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiersDescription), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiersDescription IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }

        public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot, ref string __result)
        {
            string str = string.Empty;
            bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
            float weaponDirectFireModifier = CustomAmmoCategories.getDirectFireModifier(weapon);
            if (flag == false)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(attacker.DisplayName + " has LOS on " + target.DisplayName + ". Apply DirectFireModifier " + weaponDirectFireModifier + "\n");
                if (!NvMath.FloatIsNearZero(weaponDirectFireModifier))
                {
                    __result = string.Format("{0}WEAPON-DIRECT-FIRE {1:+#;-#}; ", (object)__result, (object)(int)weaponDirectFireModifier);
                }
            }
            CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            return;
        }
    }
    [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
    [HarmonyPatch("UpdateToolTipsFiring")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(ICombatant) })]
    public static class CombatHUDWeaponSlot_UpdateToolTipsFiring
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_UpdateToolTipsFiring), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponSlot_UpdateToolTipsFiring IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(CombatHUDWeaponTickMarks))]
    [HarmonyPatch("GetValidSlots")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(List<CombatHUDWeaponSlot>),typeof(float),typeof(bool) })]
    public static class CombatHUDWeaponTickMarks_GetValidSlots
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponTickMarks_GetValidSlots), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponTickMarks_GetValidSlots IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("WillFireAtTargetFromPosition")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
    public static class Weapon_WillFireAtTargetFromPosition
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(Weapon_WillFireAtTargetFromPosition), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get WillFireAtTargetFromPosition IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
    [HarmonyPatch(typeof(LOFCache))]
    [HarmonyPatch("UnitHasLOFToTarget")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Weapon) })]
    public static class LOFCache_UnitHasLOFToTarget
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(LOFCache_UnitHasLOFToTarget), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get LOFCache_UnitHasLOFToTarget IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }
    }
}
