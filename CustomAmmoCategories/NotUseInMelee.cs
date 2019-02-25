using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using BattleTech.UI;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static WeaponCategory isWeaponUseInMelee(Weapon weapon) {
      if (weapon.Category != WeaponCategory.AntiPersonnel) { return weapon.Category; };
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.NotUseInMelee != TripleBoolean.True) { return weapon.Category; };
      switch (weapon.Type) {
        case WeaponType.AMS: return WeaponCategory.AMS;
        case WeaponType.Autocannon: return WeaponCategory.Ballistic;
        case WeaponType.Flamer: return WeaponCategory.Energy;
        case WeaponType.Gauss: return WeaponCategory.Ballistic;
        case WeaponType.Laser: return WeaponCategory.Energy;
        case WeaponType.LRM: return WeaponCategory.Missile;
        case WeaponType.MachineGun: return WeaponCategory.Ballistic;
        case WeaponType.Melee: return WeaponCategory.Melee;
        case WeaponType.NotSet: return WeaponCategory.NotSet;
        case WeaponType.PPC: return WeaponCategory.Energy;
        case WeaponType.SRM: return WeaponCategory.Missile;
        default: return WeaponCategory.NotSet;
      }
    }
  }
}

namespace CustomAmmoCategoriesPathes {
  [HarmonyPatch(typeof(AIUtil))]
  [HarmonyPatch("ExpectedDamageForMeleeAttackUsingUnitsBVs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(AbstractActor) })]
  public static class AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs Category "+weapon.Category+"->"+result+"\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrderForTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(AbstractActor) })]
  public static class AttackEvaluator_MakeAttackOrderForTarget_AP {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackEvaluator_MakeAttackOrderForTarget_AP), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get AttackEvaluator_MakeAttackOrderForTarget_AP Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("SetHitChance")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_SetHitChance {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_SetHitChance), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponSlot_SetHitChance Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(SelectionStateJump))]
  [HarmonyPatch("ProjectedHeatForState")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateJump_ProjectedHeatForState {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(SelectionStateJump_ProjectedHeatForState), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get SelectionStateJump_ProjectedHeatForState Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProjectedHeatForState")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMove_ProjectedHeatForState {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(SelectionStateMove_ProjectedHeatForState), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get SelectionStateMove_ProjectedHeatForState Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(HostileDamageFactor))]
  [HarmonyPatch("expectedDamageForMelee")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class HostileDamageFactor_expectedDamageForMelee {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(HostileDamageFactor_expectedDamageForMelee), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get HostileDamageFactor_expectedDamageForMelee Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayImpactAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(MeleeAttackType), typeof(float) })]
  public static class MechRepresentation_PlayImpactAnim {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MechRepresentation_PlayImpactAnim), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MechRepresentation_PlayImpactAnim Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(CombatTestFire))]
  [HarmonyPatch("MeleeAttackSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatTestFire_MeleeAttackSequence {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatTestFire_MeleeAttackSequence), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get CombatTestFire_MeleeAttackSequence Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  public static class MeleeWithHighestPriorityEnemyNode_Tick {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MeleeWithHighestPriorityEnemyNode_Tick), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MeleeWithHighestPriorityEnemyNode_Tick Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  public static class MoveTowardsHighestPriorityMoveCandidateNode_Tick {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "Category").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MoveTowardsHighestPriorityMoveCandidateNode_Tick), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategory Category(Weapon weapon) {
      WeaponCategory result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MoveTowardsHighestPriorityMoveCandidateNode_Tick Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
}
