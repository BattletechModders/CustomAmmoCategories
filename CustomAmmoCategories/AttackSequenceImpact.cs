using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustAmmoCategories;
using BattleTech;
using Harmony;
using UnityEngine;
using System.Reflection;

//This part of code is modified code from original WeaponRealizer by Joel Meador under MIT LICENSE

namespace CustAmmoCategories {
  public struct VarianceBounds {
    public readonly float Min;
    public readonly float Max;
    public readonly float StandardDeviation;

    public VarianceBounds(float min, float max, float standardDeviation) {
      Min = min;
      Max = max;
      StandardDeviation = standardDeviation;
    }
  }

  public static class NormalDistribution {
    private const int IterationLimit = 10;
    public static float Random(VarianceBounds bounds, int step = -1) {
      // compute a random number that fits a gaussian function https://en.wikipedia.org/wiki/Gaussian_function
      // iterative w/ limit adapted from https://natedenlinger.com/php-random-number-generator-with-normal-distribution-bell-curve/
      var iterations = 0;
      float randomNumber;
      do {
        var rand1 = UnityEngine.Random.value;
        var rand2 = UnityEngine.Random.value;
        var gaussianNumber = Mathf.Sqrt(-2 * Mathf.Log(rand1)) * Mathf.Cos(2 * Mathf.PI * rand2);
        var mean = (bounds.Max + bounds.Min) / 2;
        randomNumber = (gaussianNumber * bounds.StandardDeviation) + mean;
        if (step > 0) randomNumber = Mathf.RoundToInt(randomNumber / step) * step;
        iterations++;
      } while ((randomNumber < bounds.Min || randomNumber > bounds.Max) && iterations < IterationLimit);

      if (iterations == IterationLimit) randomNumber = (bounds.Min + bounds.Max) / 2.0f;
      return randomNumber;
    }
  }

  public static partial class CustomAmmoCategories {
    public static WeaponRealizer.Settings getWRSettings() {
      return (WeaponRealizer.Settings)typeof(WeaponRealizer.Core).GetField("ModSettings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
    }
    public static float getWeaponDamageVariance(Weapon weapon) {
      float result = weapon.weaponDef.DamageVariance;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        result += extAmmo.DamageVariance;
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 0) {
        string modeId = "";
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        } else {
          modeId = extWeapon.baseModeId;
        }
        if (extWeapon.Modes.ContainsKey(modeId)) {
          result += extWeapon.Modes[modeId].DamageVariance;
        }
      }
      return result;
    }
    public static float getWeaponDistantVariance(Weapon weapon) {
      float result = 0;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
        result += extAmmo.DistantVariance;
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 0) {
        string modeId = "";
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        } else {
          modeId = extWeapon.baseModeId;
        }
        if (extWeapon.Modes.ContainsKey(modeId)) {
          result += extWeapon.Modes[modeId].DistantVariance;
        }
      }
      return result;
    }
    public static bool getWeaponDistantVarianceReversed(Weapon weapon) {
      TripleBoolean result = TripleBoolean.NotSet;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 0) {
        string modeId = "";
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        } else {
          modeId = extWeapon.baseModeId;
        }
        if (extWeapon.Modes.ContainsKey(modeId)) {
          result = extWeapon.Modes[modeId].DistantVarianceReversed;
        }
      }
      if (result == TripleBoolean.NotSet) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
          result = extAmmo.DistantVarianceReversed;
        }
      }
      return result == TripleBoolean.True;
    }
    private const double Pi2 = Math.PI / 2.0;
    public static float WeaponDamageDistance(ICombatant attacker, ICombatant target, Weapon weapon, float damage, float rawDamage) {
      var damagePerShot = weapon.DamagePerShot;
      var adjustment = rawDamage / damagePerShot;
      float varianceMultiplier;
      var distance = Vector3.Distance(attacker.TargetPosition, target.TargetPosition);
      var distanceDifference = weapon.MaxRange - distance;
      var distanceRatio = distanceDifference / weapon.MaxRange;
      var baseMultiplier = CustomAmmoCategories.getWeaponDistantVariance(weapon);
      var distanceBasedFunctionMultiplier = (float)Math.Atan(Pi2 * distanceRatio + baseMultiplier);
      if (distance < weapon.MaxRange) {
        varianceMultiplier = Mathf.Max(
            baseMultiplier,
            Mathf.Min(
                1.0f,
                distanceBasedFunctionMultiplier
            ));
      } else { //out of range
        return damage;
      }
      var computedDamage = damage * varianceMultiplier * adjustment;
      CustomAmmoCategoriesLog.Log.LogWrite($"distanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
                   $"defId: {weapon.defId}\n" +
                   $"varianceMultiplier: {varianceMultiplier}\n" +
                   $"adjustment: {adjustment}\n" +
                   $"damage: {damage}\n" +
                   $"distance: {distance}\n" +
                   $"max: {weapon.MaxRange}\n" +
                   $"distanceDifference {distanceDifference}\n" +
                   $"baseMultplier: {baseMultiplier}\n" +
                   $"distanceRatio: {distanceRatio}\n" +
                   $"computedDamage: {computedDamage}\n");
      return computedDamage;
    }
    public static float WeaponDamageRevDistance(ICombatant attacker, ICombatant target, Weapon weapon, float damage, float rawDamage) {
      var damagePerShot = weapon.DamagePerShot;
      var adjustment = rawDamage / damagePerShot;
      float varianceMultiplier;
      var distance = Vector3.Distance(attacker.TargetPosition, target.TargetPosition);
      var distanceDifference = weapon.MaxRange - distance;
      var distanceRatio = distanceDifference / weapon.MinRange;
      var baseMultiplier = CustomAmmoCategories.getWeaponDistantVariance(weapon);
      var distanceBasedFunctionMultiplier = (float)Math.Atan(1f / (Pi2 * distanceRatio + baseMultiplier));
      if (distance < weapon.MinRange) {
        varianceMultiplier = 0;
      } else if (distance <= weapon.MaxRange) {
        varianceMultiplier = Mathf.Max(
            baseMultiplier,
            Mathf.Min(
                1.0f,
                distanceBasedFunctionMultiplier
            ));
      } else // out of range ¯\_(ツ)_/¯
        {
        return damage;
      }
      var computedDamage = damage * varianceMultiplier * adjustment;
      CustomAmmoCategoriesLog.Log.LogWrite($"reverseDistanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
                   $"defId: {weapon.defId}\n" +
                   $"varianceMultiplier: {varianceMultiplier}\n" +
                   $"adjustment: {adjustment}\n" +
                   $"damage: {damage}\n" +
                   $"distance: {distance}\n" +
                   $"max: {weapon.MaxRange}\n" +
                   $"distanceDifference {distanceDifference}\n" +
                   $"baseMultplier: {baseMultiplier}\n" +
                   $"distanceRatio: {distanceRatio}\n" +
                   $"computedDamage: {computedDamage}\n");
      return computedDamage;
    }
    public static float WeaponDamageSimpleVariance(Weapon weapon, float rawDamage) {
      CustomAmmoCategoriesLog.Log.LogWrite("Simple damage variance for weapon "+weapon.UIName+"\n");
      var damagePerShot = weapon.DamagePerShot;
      var adjustment = rawDamage / damagePerShot;
      var variance = CustomAmmoCategories.getWeaponDamageVariance(weapon);
      var roll = NormalDistribution.Random(
          new VarianceBounds(
              damagePerShot - variance,
              damagePerShot + variance,
              CustomAmmoCategories.getWRSettings().StandardDeviationSimpleVarianceMultiplier * variance
          ));
      var variantDamage = roll * adjustment;

      var sb = new StringBuilder();
      sb.AppendLine($"roll: {roll}");
      sb.AppendLine($"damagePerShot: {damagePerShot}");
      sb.AppendLine($"variance: {variance}");
      sb.AppendLine($"adjustment: {adjustment}");
      sb.AppendLine($"variantDamage: {variantDamage}");
      CustomAmmoCategoriesLog.Log.LogWrite(sb.ToString()+"\n");
      return variantDamage;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpact {
    public static bool Prefix(AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return true; }
      int attackGroupIndex = impactMessage.hitInfo.attackGroupIndex;
      int attackWeaponIndex = impactMessage.hitInfo.attackWeaponIndex;
      Weapon weapon = __instance.GetWeapon(attackGroupIndex, attackWeaponIndex);
      float rawDamage = impactMessage.hitDamage;
      float realDamage = rawDamage;
      CustomAmmoCategoriesLog.Log.LogWrite("OnAttackSequenceImpact\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  attacker = "+__instance.attacker.DisplayName+"\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  target = "+__instance.target.DisplayName+"\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon = " + weapon.UIName + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  damage = " + rawDamage + "\n");
      if (realDamage >= 1.0f) {
        if (CustomAmmoCategories.getWeaponDamageVariance(weapon) > CustomAmmoCategories.Epsilon) {
          realDamage = CustomAmmoCategories.WeaponDamageSimpleVariance(weapon, rawDamage);
        }
        if (CustomAmmoCategories.getWeaponDistantVariance(weapon) > CustomAmmoCategories.Epsilon) {
          if (CustomAmmoCategories.getWeaponDistantVarianceReversed(weapon)) {
            realDamage = CustomAmmoCategories.WeaponDamageDistance(__instance.attacker, __instance.target, weapon, realDamage, rawDamage);
          } else {
            realDamage = CustomAmmoCategories.WeaponDamageRevDistance(__instance.attacker, __instance.target, weapon, realDamage, rawDamage);
          }
        }
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! raw damage is less than 1.0f. Variance calculation is forbidden with this damage value\n",true);
      }
      CustomAmmoCategoriesLog.Log.LogWrite("  real damage = " + realDamage + "\n");
      impactMessage.hitDamage = realDamage;
      return true;
    }
  }
}
