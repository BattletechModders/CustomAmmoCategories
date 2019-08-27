using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.UI;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using HBS.Math;
using CustomAmmoCategoriesLog;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static bool getIndirectFireCapable(Weapon weapon) {
      bool result = weapon.weaponDef.IndirectFireCapable;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        if (extAmmoDef.IndirectFireCapable != TripleBoolean.NotSet) {
          result = (extAmmoDef.IndirectFireCapable == TripleBoolean.True);
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          if (mode.IndirectFireCapable != TripleBoolean.NotSet) {
            result = (mode.IndirectFireCapable == TripleBoolean.True);
          }
        }
      }
      return result;
    }
    public static bool isIndirectFireCapable(this Weapon weapon) {return getIndirectFireCapable(weapon);}
    public static float getDirectFireModifier(Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.weaponDef.Description.Id);
      float result = extWeapon.DirectFireModifier;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName)) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        result += CustomAmmoCategories.findExtAmmo(ammoId).DirectFireModifier;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName)) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          result += extWeapon.Modes[modeId].DirectFireModifier;
        }
      }
      return result;
    }
    public static float getWeaponForbiddenRange(Weapon weapon) {
      if (CustomAmmoCategories.Settings.forbiddenRangeEnable == false) { return 0f; };
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      float result = 0f;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName)) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        result += CustomAmmoCategories.findExtAmmo(ammoId).ForbiddenRange;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName)) {
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          result += extWeapon.Modes[modeId].ForbiddenRange;
        }
      }
      return result;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AIUtil))]
  [HarmonyPatch("UnitHasLOFToTargetFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(CombatGameState), typeof(Vector3) })]
  public static class AIUtil_UnitHasLOFToTargetFromPosition {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AIUtil_UnitHasLOFToTargetFromPosition), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(AIUtil))]
  [HarmonyPatch("UnitHasLOFToUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AbstractActor), typeof(CombatGameState) })]
  public static class AIUtil_UnitHasLOFToUnit {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AIUtil_UnitHasLOFToUnit), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToUnit IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(AIRoleAssignment))]
  [HarmonyPatch("EvaluateSniper")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class AIRoleAssignment_EvaluateSniper {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AIRoleAssignment_EvaluateSniper), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIRoleAssignment_EvaluateSniper IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("GetLongestRangeWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool), typeof(bool) })]
  public static class AbstractActor_GetLongestRangeWeapon {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AbstractActor_GetLongestRangeWeapon), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_GetLongestRangeWeapon IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasIndirectLOFToTargetUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(bool) })]
  public static class AbstractActor_HasIndirectLOFToTargetUnit {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AbstractActor_HasIndirectLOFToTargetUnit), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_HasIndirectLOFToTargetUnit IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("HasLOFToTargetUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Weapon) })]
  public static class AbstractActor_HasLOFToTargetUnit {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AbstractActor_HasLOFToTargetUnit), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AbstractActor_HasLOFToTargetUnit IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(HostileDamageFactor))]
  [HarmonyPatch("expectedDamageForShooting")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool), typeof(bool) })]
  public static class HostileDamageFactor_expectedDamageForShooting {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(HostileDamageFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get HostileDamageFactor_expectedDamageForShooting IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(MultiAttack))]
  [HarmonyPatch("FindWeaponToHitTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(List<Weapon>), typeof(ICombatant) })]
  public static class MultiAttack_FindWeaponToHitTarget {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MultiAttack_FindWeaponToHitTarget), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_FindWeaponToHitTarget IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(MultiAttack))]
  [HarmonyPatch("GetExpectedDamageForMultiTargetWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(bool), typeof(Weapon), typeof(int) })]
  public static class MultiAttack_GetExpectedDamageForMultiTargetWeapon {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MultiAttack_GetExpectedDamageForMultiTargetWeapon), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_GetExpectedDamageForMultiTargetWeapon IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(AbstractActor attackerUnit, Vector3 attackPosition, ICombatant targetUnit, Vector3 targetPosition, bool targetIsEvasive, Weapon weapon, int numTargets,ref float __result) {
      if(__result > CustomAmmoCategories.Epsilon) {
        float forbiddenRange = CustomAmmoCategories.getWeaponForbiddenRange(weapon);
        if (forbiddenRange > (double)CustomAmmoCategories.Epsilon) {
          float ActualRange = Vector3.Distance(attackPosition, targetPosition);
          if (ActualRange < forbiddenRange) { __result = 0.0f; }
        }
      }
    }
  }
  [HarmonyPatch(typeof(MultiAttack))]
  [HarmonyPatch("PartitionWeaponListToKillTarget")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(List<Weapon>), typeof(ICombatant), typeof(ICombatant), typeof(float) })]
  public static class MultiAttack_PartitionWeaponListToKillTarget {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MultiAttack_PartitionWeaponListToKillTarget), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_PartitionWeaponListToKillTarget IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(MultiAttack))]
  [HarmonyPatch("ValidateMultiAttackOrder")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(MultiTargetAttackOrderInfo), typeof(AbstractActor) })]
  public static class MultiAttack_ValidateMultiAttackOrder {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MultiAttack_ValidateMultiAttackOrder), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get MultiAttack_ValidateMultiAttackOrder IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(PreferExposedAlonePositionalFactor))]
  [HarmonyPatch("InitEvaluationForPhaseForUnit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get PreferExposedAlonePositionalFactor_InitEvaluationForPhaseForUnit IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(PreferFiringSolutionWhenExposedAllyPositionalFactor))]
  [HarmonyPatch("EvaluateInfluenceMapFactorAtPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(float), typeof(MoveType), typeof(PathNode) })]
  public static class PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get PreferFiringSolutionWhenExposedAllyPositionalFactor_EvaluateInfluenceMapFactorAtPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(PreferLethalDamageToRearArcFromHostileFactor))]
  [HarmonyPatch("expectedDamageForShooting")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(bool) })]
  public static class PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get PreferLethalDamageToRearArcFromHostileFactor_expectedDamageForShooting IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(PreferNotLethalPositionFactor))]
  [HarmonyPatch("expectedDamageForShooting")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(MoveType) })]
  public static class PreferNotLethalPositionFactor_expectedDamageForShooting {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PreferNotLethalPositionFactor_expectedDamageForShooting), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    public static bool IndirectFireCapable(this Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get PreferNotLethalPositionFactor_expectedDamageForShooting IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetAllModifiers")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
  public static class ToHit_GetAllModifiers {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiers), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiers IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }

    public static HBS.Collections.TagSet Tags(this ICombatant target) {
      Mech mech = target as Mech;
      if (mech != null) { return mech.MechDef.MechTags; }
      Vehicle vehicle = target as Vehicle;
      if (vehicle != null) { return vehicle.VehicleDef.VehicleTags; }
      Turret turret = target as Turret;
      if (turret != null) { return turret.TurretDef.TurretTags; }
      return null;
    }
    public static float GetChassisTagsModifyer(this Weapon weapon, ICombatant target) {
      float result = 0f;
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.TagsAccuracyModifiers.Count == 0) { return 0f; }
      HBS.Collections.TagSet tags = target.Tags();
      if (tags == null) { return 0f; };
      foreach(string tag in tags) {
        if (ammo.TagsAccuracyModifiers.ContainsKey(tag)) { result += ammo.TagsAccuracyModifiers[tag]; };
      }
      return result;
    }
    public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot, ref float __result) {
      bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
      float num = __result;
      if (flag == false) {
        //float directFireModifier = CustomAmmoCategories.getDirectFireModifier(weapon);
        //CustomAmmoCategoriesLog.Log.LogWrite(attacker.DisplayName+" has LOS on "+target.DisplayName+ ". Apply DirectFireModifier "+directFireModifier+"\n");
        num += CustomAmmoCategories.getDirectFireModifier(weapon);
      }
      float distance = Vector3.Distance(attackPosition, targetPosition);
      float distMod = 0f;
      float minRange = weapon.MinRange;
      float shortRange = weapon.ShortRange;
      float medRange = weapon.MediumRange;
      float longRange = weapon.LongRange;
      float maxRange = weapon.MaxRange;
      //Log.LogWrite("ToHit " + weapon.defId + " distance: " + distance + " ranges:" + minRange + "/" + shortRange + "/" + medRange + "/" + longRange + "/" + maxRange + " mods:"
      //  + weapon.parent.MinRangeAccMod() + "/" + weapon.parent.ShortRangeAccMod() + "/" + weapon.parent.MediumRangeAccMod() + "/" + weapon.parent.LongRangeRangeAccMod() + "/" + weapon.parent.ExtraLongRangeAccMod());
      if (distance < minRange) { distMod = weapon.parent.MinRangeAccMod();
        //Log.LogWrite(" minRange "); 
      } else
      if (distance < shortRange) { distMod = weapon.parent.ShortRangeAccMod();
        //Log.LogWrite(" shortRange ");
      } else
      if (distance < medRange) { distMod = weapon.parent.MediumRangeAccMod();
        //Log.LogWrite(" medRange ");
      } else
      if (distance < longRange) { distMod = weapon.parent.LongRangeRangeAccMod();
        //Log.LogWrite(" longRange ");
      } else
      if (distance < maxRange) { distMod = weapon.parent.ExtraLongRangeAccMod();
        //Log.LogWrite(" extraRange ");
      };
      //Log.LogWrite(" effMod: " +distMod+"\n");
      num += distMod;
      num += weapon.GetChassisTagsModifyer(target);
      CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      if ((double)num < 0.0 && !combat.Constants.ResolutionConstants.AllowTotalNegativeModifier) {
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
  public static class ToHit_GetAllModifiersDescription {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(ToHit_GetAllModifiersDescription), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get ToHit_GetAllModifiersDescription IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
    public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot, ref string __result) {
      string str = string.Empty;
      bool flag = lofLevel < LineOfFireLevel.LOFObstructed && (CustomAmmoCategories.getIndirectFireCapable(weapon));
      float weaponDirectFireModifier = CustomAmmoCategories.getDirectFireModifier(weapon);
      if (flag == false) {
        //CustomAmmoCategoriesLog.Log.LogWrite(attacker.DisplayName + " has LOS on " + target.DisplayName + ". Apply DirectFireModifier " + weaponDirectFireModifier + "\n");
        if (!NvMath.FloatIsNearZero(weaponDirectFireModifier)) {
          __result = string.Format("{0}WEAPON-DIRECT-FIRE {1:+#;-#}; ", (object)__result, (object)(int)weaponDirectFireModifier);
        }
      }
      float distance = Vector3.Distance(attackPosition, targetPosition);
      float distMod = 0f;
      float minRange = weapon.MinRange;
      float shortRange = weapon.ShortRange;
      float medRange = weapon.MediumRange;
      float longRange = weapon.LongRange;
      float maxRange = weapon.MaxRange;
      if (distance < minRange) { distMod = weapon.parent.MinRangeAccMod(); } else
      if (distance < shortRange) { distMod = weapon.parent.ShortRangeAccMod(); } else
      if (distance < medRange) { distMod = weapon.parent.MediumRangeAccMod(); } else
      if (distance < longRange) { distMod = weapon.parent.LongRangeRangeAccMod(); } else
      if (distance < maxRange) { distMod = weapon.parent.ExtraLongRangeAccMod(); };
      if (!NvMath.FloatIsNearZero(distMod)) {
        __result = string.Format("{0}DISTANCE-MODIFIER {1:+#;-#}; ", (object)__result, (object)(int)distMod);
      }
      float tagsMod = weapon.GetChassisTagsModifyer(target);
      if (!NvMath.FloatIsNearZero(tagsMod)) {
        __result = string.Format("{0}TARGET-TYPE-MODIFIER {1:+#;-#}; ", (object)__result, (object)(int)tagsMod);
      }
      CombatGameState combat = (CombatGameState)typeof(ToHit).GetField("combat", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      return;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("UpdateToolTipsFiring")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_UpdateToolTipsFiring {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_UpdateToolTipsFiring), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponSlot_UpdateToolTipsFiring IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponTickMarks))]
  [HarmonyPatch("GetValidSlots")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(List<CombatHUDWeaponSlot>), typeof(float), typeof(bool) })]
  public static class CombatHUDWeaponTickMarks_GetValidSlots {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponTickMarks_GetValidSlots), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponTickMarks_GetValidSlots IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("WillFireAtTargetFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
  public static class Weapon_WillFireAtTargetFromPosition {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Weapon_WillFireAtTargetFromPosition), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get WillFireAtTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(Weapon __instance, ICombatant target, Vector3 position, Quaternion rotation,ref bool __result) {
      if (__result == true) {
        float forbiddenRange = CustomAmmoCategories.getWeaponForbiddenRange(__instance);
        if (forbiddenRange > (double)CustomAmmoCategories.Epsilon) {
          float ActualRange = Vector3.Distance(position, target.TargetPosition);
          if (ActualRange < forbiddenRange) { __result = false; }
        }
      }
    }
  }
  public static class DoAnyMovesYieldLOFToAnyHostileNode_Tick {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(DoAnyMovesYieldLOFToAnyHostileNode_Tick), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static bool IndirectFireCapable(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("get DoAnyMovesYieldLOFToAnyHostileNode_Tick IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }

  public static class HighestPriorityMoveCandidateIsAttackNode_Tick {
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(HighestPriorityMoveCandidateIsAttackNode_Tick), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static bool IndirectFireCapable(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("get HighestPriorityMoveCandidateIsAttackNode_Tick IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
  }


  [HarmonyPatch(typeof(LOFCache))]
  [HarmonyPatch("UnitHasLOFToTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(ICombatant), typeof(Weapon) })]
  public static class LOFCache_UnitHasLOFToTarget {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "IndirectFireCapable").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(LOFCache_UnitHasLOFToTarget), nameof(IndirectFireCapable));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static bool IndirectFireCapable(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get LOFCache_UnitHasLOFToTarget IndirectFireCapable\n");
      return CustomAmmoCategories.getIndirectFireCapable(weapon);
    }
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(LOFCache __instance, AbstractActor shootingUnit, ICombatant targetUnit, Weapon weapon,ref bool __result) {
      if (__result == true) {
        float forbiddenRange = CustomAmmoCategories.getWeaponForbiddenRange(weapon);
        if (forbiddenRange > (double)CustomAmmoCategories.Epsilon) {
          float ActualRange = Vector3.Distance(shootingUnit.CurrentPosition, targetUnit.CurrentPosition);
          if (ActualRange < forbiddenRange) { __result = false; }
        }
      }
    }
  }
}
