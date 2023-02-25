using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BattleTech;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using BattleTech.UI;
/*
namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
#if BT1_8
    public static Dictionary<int, WeaponCategoryValue> fakeCantUseInMeleeWeaponCategories = new Dictionary<int, WeaponCategoryValue>();
    public static WeaponCategoryValue Copy(this WeaponCategoryValue value) {
      WeaponCategoryValue result = new WeaponCategoryValue();
      result.ID = value.ID;
      result.Name = value.Name;
      result.FriendlyName = value.FriendlyName;
      typeof(EnumValue).GetProperty("Description").GetSetMethod(true).Invoke(result, new object[1] { value.Description });
      typeof(WeaponCategoryValue).GetProperty("IsBallistic").GetSetMethod(true).Invoke(result, new object[1] { value.IsBallistic });
      typeof(WeaponCategoryValue).GetProperty("IsMissile").GetSetMethod(true).Invoke(result, new object[1] { value.IsMissile });
      typeof(WeaponCategoryValue).GetProperty("IsEnergy").GetSetMethod(true).Invoke(result, new object[1] { value.IsEnergy });
      typeof(WeaponCategoryValue).GetProperty("IsSupport").GetSetMethod(true).Invoke(result, new object[1] { value.IsSupport });
      typeof(WeaponCategoryValue).GetProperty("IsMelee").GetSetMethod(true).Invoke(result, new object[1] { value.IsMelee });
      typeof(WeaponCategoryValue).GetProperty("CanUseInMelee").GetSetMethod(true).Invoke(result, new object[1] { value.CanUseInMelee });
      typeof(WeaponCategoryValue).GetProperty("IsAffectedByEvasive").GetSetMethod(true).Invoke(result, new object[1] { value.IsAffectedByEvasive });
      typeof(WeaponCategoryValue).GetProperty("ForceLightHitReact").GetSetMethod(true).Invoke(result, new object[1] { value.ForceLightHitReact });
      typeof(WeaponCategoryValue).GetProperty("DamageReductionMultiplierStat").GetSetMethod(true).Invoke(result, new object[1] { value.DamageReductionMultiplierStat });
      typeof(WeaponCategoryValue).GetProperty("ToBeHitStat").GetSetMethod(true).Invoke(result, new object[1] { value.ToBeHitStat });
      typeof(WeaponCategoryValue).GetProperty("DesignMaskString").GetSetMethod(true).Invoke(result, new object[1] { value.DesignMaskString });
      typeof(WeaponCategoryValue).GetProperty("TurretDamageMultiplier").GetSetMethod(true).Invoke(result, new object[1] { value.TurretDamageMultiplier });
      typeof(WeaponCategoryValue).GetProperty("VehicleDamageMultiplier").GetSetMethod(true).Invoke(result, new object[1] { value.VehicleDamageMultiplier });
      typeof(WeaponCategoryValue).GetProperty("MinHorizontalAngle").GetSetMethod(true).Invoke(result, new object[1] { value.MinHorizontalAngle });
      typeof(WeaponCategoryValue).GetProperty("MaxHorizontalAngle").GetSetMethod(true).Invoke(result, new object[1] { value.MaxHorizontalAngle });
      typeof(WeaponCategoryValue).GetProperty("MinVerticalAngle").GetSetMethod(true).Invoke(result, new object[1] { value.MinVerticalAngle });
      typeof(WeaponCategoryValue).GetProperty("MaxVerticalAngle").GetSetMethod(true).Invoke(result, new object[1] { value.MaxVerticalAngle });
      typeof(WeaponCategoryValue).GetProperty("UIColorRef").GetSetMethod(true).Invoke(result, new object[1] { value.UIColorRef });
      typeof(WeaponCategoryValue).GetProperty("FallbackUIColor").GetSetMethod(true).Invoke(result, new object[1] { value.FallbackUIColor });
      typeof(WeaponCategoryValue).GetProperty("Icon").GetSetMethod(true).Invoke(result, new object[1] { value.Icon });
      typeof(WeaponCategoryValue).GetProperty("HardpointPrefabText").GetSetMethod(true).Invoke(result, new object[1] { value.HardpointPrefabText });
      typeof(WeaponCategoryValue).GetProperty("UseHardpointPrefabTextAsSuffix").GetSetMethod(true).Invoke(result, new object[1] { value.UseHardpointPrefabTextAsSuffix });
      return result;
    }
    public static WeaponCategoryValue isWeaponUseInMelee(this Weapon weapon) {
      if (weapon.WeaponCategoryValue.CanUseInMelee == false) { return weapon.WeaponCategoryValue; };
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.NotUseInMelee != TripleBoolean.True) { return weapon.WeaponCategoryValue; };
      if (fakeCantUseInMeleeWeaponCategories.ContainsKey(weapon.WeaponCategoryValue.ID) == false) { return weapon.WeaponCategoryValue; }
      return fakeCantUseInMeleeWeaponCategories[weapon.WeaponCategoryValue.ID];
    }
  }
#else
    public static WeaponCategory isWeaponUseInMelee(this Weapon weapon) {
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
#endif
}

namespace CustomAmmoCategoriesPathes {
#if BT1_8
  [HarmonyPatch(typeof(WeaponCategoryEnumeration))]
  [HarmonyPatch("RefreshStaticData")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class WeaponCategoryEnumeration_RefreshStaticData {
    private static void Postfix(WeaponCategoryEnumeration __instance) {
      List<WeaponCategoryValue> weaponCategoryList = (List<WeaponCategoryValue>)typeof(WeaponCategoryEnumeration).GetField("weaponCategoryList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
      CustomAmmoCategories.fakeCantUseInMeleeWeaponCategories.Clear();
      foreach (WeaponCategoryValue weaponCategory in weaponCategoryList) {
        WeaponCategoryValue fake = weaponCategory.Copy();
        typeof(WeaponCategoryValue).GetProperty("CanUseInMelee").GetSetMethod(true).Invoke(fake, new object[1] { false });
        CustomAmmoCategories.fakeCantUseInMeleeWeaponCategories.Add(weaponCategory.ID, fake);
      }
    }
  }
  [HarmonyPatch(typeof(AIUtil))]
  [HarmonyPatch("ExpectedDamageForMeleeAttackUsingUnitsBVs")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(AbstractActor) })]
  public static class AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrderForTarget")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackEvaluator_MakeAttackOrderForTarget_AP {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackEvaluator_MakeAttackOrderForTarget_AP), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get AttackEvaluator_MakeAttackOrderForTarget_AP Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("SetHitChance")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_SetHitChance {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_SetHitChance), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get CombatHUDWeaponSlot_SetHitChance Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(SelectionStateJump))]
  [HarmonyPatch("ProjectedHeatForState")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateJump_ProjectedHeatForState {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(SelectionStateJump_ProjectedHeatForState), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get SelectionStateJump_ProjectedHeatForState Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProjectedHeatForState")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class SelectionStateMove_ProjectedHeatForState {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(SelectionStateMove_ProjectedHeatForState), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get SelectionStateMove_ProjectedHeatForState Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(HostileDamageFactor))]
  [HarmonyPatch("expectedDamageForMelee")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class HostileDamageFactor_expectedDamageForMelee {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(HostileDamageFactor_expectedDamageForMelee), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get HostileDamageFactor_expectedDamageForMelee Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(MechRepresentation))]
  [HarmonyPatch("PlayImpactAnim")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(MeleeAttackType), typeof(float) })]
  public static class MechRepresentation_PlayImpactAnim {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MechRepresentation_PlayImpactAnim), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MechRepresentation_PlayImpactAnim Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(CombatTestFire))]
  [HarmonyPatch("MeleeAttackSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatTestFire_MeleeAttackSequence {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatTestFire_MeleeAttackSequence), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get CombatTestFire_MeleeAttackSequence Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  public static class MeleeWithHighestPriorityEnemyNode_Tick {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MeleeWithHighestPriorityEnemyNode_Tick), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MeleeWithHighestPriorityEnemyNode_Tick Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
  public static class MoveTowardsHighestPriorityMoveCandidateNode_Tick {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "WeaponCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(MoveTowardsHighestPriorityMoveCandidateNode_Tick), nameof(Category));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static WeaponCategoryValue Category(Weapon weapon) {
      WeaponCategoryValue result = CustomAmmoCategories.isWeaponUseInMelee(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("get MoveTowardsHighestPriorityMoveCandidateNode_Tick Category " + weapon.WeaponCategoryValue.CanUseInMelee + "->" + result.CanUseInMelee + "\n");
      return result;
    }
  }
#else
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
      CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_ExpectedDamageForMeleeAttackUsingUnitsBVs Category " + weapon.Category + "->" + result + "\n");
      return result;
    }
  }
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrderForTarget")]
  [HarmonyPatch(MethodType.Normal)]
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
#endif
}
*/