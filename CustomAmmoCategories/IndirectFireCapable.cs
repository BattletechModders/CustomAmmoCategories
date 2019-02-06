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
        /*static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiers), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiers IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }*/

        public static bool Prefix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot,ref float __result)
        {
            bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
            float num = __instance.GetRangeModifier(weapon, attackPosition, targetPosition) 
                + __instance.GetCoverModifier(attacker, target, lofLevel) 
                + __instance.GetSelfSpeedModifier(attacker) 
                + __instance.GetSelfSprintedModifier(attacker) 
                + __instance.GetSelfArmMountedModifier(weapon) 
                + __instance.GetStoodUpModifier(attacker) 
                + __instance.GetHeightModifier(attackPosition.y, targetPosition.y) 
                + __instance.GetHeatModifier(attacker)
                + __instance.GetTargetTerrainModifier(target, targetPosition, false) 
                + __instance.GetSelfTerrainModifier(attackPosition, false) 
                + __instance.GetTargetSpeedModifier(target, weapon)
                + __instance.GetSelfDamageModifier(attacker, weapon)
                + __instance.GetTargetSizeModifier(target)
                + __instance.GetTargetShutdownModifier(target, false)
                + __instance.GetTargetProneModifier(target, false)
                + __instance.GetWeaponAccuracyModifier(attacker, weapon)
                + __instance.GetAttackerAccuracyModifier(attacker)
                + __instance.GetEnemyEffectModifier(target)
                + __instance.GetRefireModifier(weapon)
                + __instance.GetTargetDirectFireModifier(target, flag)
                + __instance.GetIndirectModifier(attacker, flag) 
                + __instance.GetMoraleAttackModifier(target, isCalledShot);
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
            return false;
        }
    }
    [HarmonyPatch(typeof(ToHit))]
    [HarmonyPatch("GetAllModifiersDescription")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
    public static class ToHit_GetAllModifiersDescription
    {
        /*static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiersDescription), nameof(IndirectFireCapable));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static bool IndirectFireCapable(Weapon weapon)
        {
            //CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiersDescription IndirectFireCapable\n");
            return CustomAmmoCategories.getIndirectFireCapable(weapon);
        }*/

        public static bool Prefix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot, ref string __result)
        {
            string str = string.Empty;
            bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
            float rangeModifier = __instance.GetRangeModifier(weapon, attackPosition, targetPosition);
            float coverModifier = __instance.GetCoverModifier(attacker, target, lofLevel);
            float selfSpeedModifier = __instance.GetSelfSpeedModifier(attacker);
            float sprintedModifier = __instance.GetSelfSprintedModifier(attacker);
            float armMountedModifier = __instance.GetSelfArmMountedModifier(weapon);
            float stoodUpModifier = __instance.GetStoodUpModifier(attacker);
            float heightModifier = __instance.GetHeightModifier(attackPosition.y, targetPosition.y);
            float heatModifier = __instance.GetHeatModifier(attacker);
            float targetTerrainModifier = __instance.GetTargetTerrainModifier(target, targetPosition, false);
            float selfTerrainModifier = __instance.GetSelfTerrainModifier(attackPosition, false);
            float targetSpeedModifier = __instance.GetTargetSpeedModifier(target, weapon);
            float selfDamageModifier = __instance.GetSelfDamageModifier(attacker, weapon);
            float targetSizeModifier = __instance.GetTargetSizeModifier(target);
            float shutdownModifier = __instance.GetTargetShutdownModifier(target, false);
            float targetProneModifier = __instance.GetTargetProneModifier(target, false);
            float accuracyModifier1 = __instance.GetWeaponAccuracyModifier(attacker, weapon);
            float accuracyModifier2 = __instance.GetAttackerAccuracyModifier(attacker);
            float enemyEffectModifier = __instance.GetEnemyEffectModifier(target);
            float refireModifier = __instance.GetRefireModifier(weapon);
            float directFireModifier = __instance.GetTargetDirectFireModifier(target, flag);
            float indirectModifier = __instance.GetIndirectModifier(attacker, flag);
            float moraleAttackModifier = __instance.GetMoraleAttackModifier(target, isCalledShot);
            float weaponDirectFireModifier = CustomAmmoCategories.getDirectFireModifier(weapon);
            if (!NvMath.FloatIsNearZero(rangeModifier))
                str = string.Format("{0}RANGE {1:+#;-#}; ", (object)str, (object)(int)rangeModifier);
            if (!NvMath.FloatIsNearZero(coverModifier))
                str = string.Format("{0}COVER {1:+#;-#}; ", (object)str, (object)(int)rangeModifier);
            if (!NvMath.FloatIsNearZero(selfSpeedModifier))
                str = string.Format("{0}SELF-MOVED {1:+#;-#}; ", (object)str, (object)(int)selfSpeedModifier);
            if (!NvMath.FloatIsNearZero(sprintedModifier))
                str = string.Format("{0}SELF-SPRINTED {1:+#;-#}; ", (object)str, (object)(int)sprintedModifier);
            if (!NvMath.FloatIsNearZero(armMountedModifier))
                str = string.Format("{0}SELF-ARM MOUNTED {1:+#;-#}; ", (object)str, (object)(int)armMountedModifier);
            if (!NvMath.FloatIsNearZero(stoodUpModifier))
                str = string.Format("{0}STOOD UP {1:+#;-#}; ", (object)str, (object)(int)stoodUpModifier);
            if (!NvMath.FloatIsNearZero(heightModifier))
                str = string.Format("{0}HEIGHT {1:+#;-#}; ", (object)str, (object)(int)heightModifier);
            if (!NvMath.FloatIsNearZero(heatModifier))
                str = string.Format("{0}HEAT {1:+#;-#}; ", (object)str, (object)(int)heatModifier);
            if (!NvMath.FloatIsNearZero(targetTerrainModifier))
                str = string.Format("{0}TERRAIN {1:+#;-#}; ", (object)str, (object)(int)targetTerrainModifier);
            if (!NvMath.FloatIsNearZero(selfTerrainModifier))
                str = string.Format("{0}TERRAIN SELF {1:+#;-#}; ", (object)str, (object)(int)selfTerrainModifier);
            if (!NvMath.FloatIsNearZero(targetSpeedModifier))
                str = string.Format("{0}TARGET-SPEED {1:+#;-#}; ", (object)str, (object)(int)targetSpeedModifier);
            if (!NvMath.FloatIsNearZero(selfDamageModifier))
                str = string.Format("{0}SELF-DAMAGE {1:+#;-#}; ", (object)str, (object)(int)selfDamageModifier);
            if (!NvMath.FloatIsNearZero(targetSizeModifier))
                str = string.Format("{0}TARGET-SIZE {1:+#;-#}; ", (object)str, (object)(int)targetSizeModifier);
            if (!NvMath.FloatIsNearZero(shutdownModifier))
                str = string.Format("{0}TARGET-SHUTDOWN {1:+#;-#}; ", (object)str, (object)(int)shutdownModifier);
            if (!NvMath.FloatIsNearZero(targetProneModifier))
                str = string.Format("{0}TARGET-PRONE {1:+#;-#}; ", (object)str, (object)(int)targetProneModifier);
            if (!NvMath.FloatIsNearZero(accuracyModifier1))
                str = string.Format("{0}ATTACKER-EFFECTS {1:+#;-#}; ", (object)str, (object)(int)accuracyModifier1);
            if (!NvMath.FloatIsNearZero(accuracyModifier2))
                str = string.Format("{0}ATTACKER-SELF-EFFECTS {1:+#;-#}; ", (object)str, (object)(int)accuracyModifier2);
            if (!NvMath.FloatIsNearZero(enemyEffectModifier))
                str = string.Format("{0}ENEMY-EFFECTS {1:+#;-#}; ", (object)str, (object)(int)enemyEffectModifier);
            if (!NvMath.FloatIsNearZero(refireModifier))
                str = string.Format("{0}REFIRE {1:+#;-#}; ", (object)str, (object)(int)refireModifier);
            if (!NvMath.FloatIsNearZero(directFireModifier))
                str = string.Format("{0}DIRECT-FIRE {1:+#;-#}; ", (object)str, (object)(int)directFireModifier);
            if (!NvMath.FloatIsNearZero(indirectModifier))
                str = string.Format("{0}INDIRECT-FIRE {1:+#;-#}; ", (object)str, (object)(int)indirectModifier);
            if (!NvMath.FloatIsNearZero(moraleAttackModifier))
                str = string.Format("{0}CALLED-SHOT {1:+#;-#}; ", (object)str, (object)(int)moraleAttackModifier);
            float b = rangeModifier + coverModifier + selfSpeedModifier + sprintedModifier + armMountedModifier 
                + stoodUpModifier + heightModifier + heatModifier + targetTerrainModifier + selfTerrainModifier 
                + targetSpeedModifier + selfDamageModifier + targetSizeModifier + shutdownModifier + targetProneModifier 
                + accuracyModifier1 + accuracyModifier2 + enemyEffectModifier + refireModifier + directFireModifier 
                + indirectModifier + moraleAttackModifier;
            if (flag == false)
            {
                CustomAmmoCategoriesLog.Log.LogWrite(attacker.DisplayName + " has LOS on " + target.DisplayName + ". Apply DirectFireModifier " + weaponDirectFireModifier + "\n");
                if (!NvMath.FloatIsNearZero(weaponDirectFireModifier))
                {
                    str = string.Format("{0}WEAPON-DIRECT-FIRE {1:+#;-#}; ", (object)str, (object)(int)weaponDirectFireModifier);
                }
                b += weaponDirectFireModifier;
            }
            CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if ((double)b < 0.0 && !combat.Constants.ResolutionConstants.AllowTotalNegativeModifier)
                b = 0.0f;
            float allModifiers = __instance.GetAllModifiers(attacker, weapon, target, attackPosition, targetPosition, lofLevel, isCalledShot);
            if (!NvMath.FloatsAreEqual(allModifiers, b))
            {
                CustomAmmoCategoriesLog.Log.LogWrite("Strange behavior calced modifier "+b+" not equal geted "+allModifiers+"\n");
                AttackDirector.attackLogger.LogError((object)("ERROR!!! breakdown of Universal Modifier didn't match actual Universal Modifier. Check TargetingRules! current modifier: " + (object)b + ", doubleCheck modifier: " + (object)allModifiers));
            }
            __result = str;
            return false;
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
