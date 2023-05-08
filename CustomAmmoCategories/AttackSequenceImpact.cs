/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using BattleTech;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using CustomAmmoCategoriesLog;
using WeaponRealizer;

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
      return WeaponRealizer.Core.ModSettings;
    }
    public static float ArmorDmgMult(this Weapon weapon) {
      return weapon.exDef().ArmorDamageModifier * weapon.ammo().ArmorDamageModifier * weapon.mode().ArmorDamageModifier;
    }
    public static float ISDmgMult(this Weapon weapon) {
      return weapon.exDef().ISDamageModifier * weapon.ammo().ISDamageModifier * weapon.mode().ISDamageModifier;
    }
    public static float DamageVariance(this Weapon weapon) {
      return weapon.weaponDef.DamageVariance + weapon.ammo().DamageVariance + weapon.mode().DamageVariance;
    }
    public static float DistantVariance(this Weapon weapon) {
      return weapon.exDef().DistantVariance + weapon.ammo().DistantVariance + weapon.mode().DistantVariance;
    }
    public static bool DistantVarianceReversed(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.DistantVarianceReversed != TripleBoolean.NotSet) { return mode.DistantVarianceReversed == TripleBoolean.True; }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.DistantVarianceReversed != TripleBoolean.NotSet) { return ammo.DistantVarianceReversed == TripleBoolean.True; }
      return weapon.exDef().DistantVarianceReversed == TripleBoolean.True;
    }
    private const double Pi2 = Math.PI / 2.0;
    public static float DistanceDamageMod(this Weapon weapon, Vector3 attackPos, ICombatant target, bool log = true) {
      float baseMultiplier = weapon.DistantVariance();
      float varianceMultiplier = 1f;
      float distance = Vector3.Distance(attackPos, target.TargetPosition);
      float middleRange = weapon.DamageFalloffStartDistance(); if (middleRange < CustomAmmoCategories.Epsilon) { middleRange = weapon.MediumRange; }
      float maxRange = weapon.DamageFalloffEndDistance(); if (maxRange < middleRange) { maxRange = weapon.MaxRange; };
      float ratio = 1f;
      if ((maxRange - middleRange) > CustomAmmoCategories.Epsilon) { ratio = (distance - middleRange) / (maxRange - middleRange); };
      if (baseMultiplier > 1f) {
        if (distance <= middleRange) { varianceMultiplier = baseMultiplier; } else {
          varianceMultiplier = baseMultiplier - weapon.RangedDmgFalloffType(ratio) * (baseMultiplier - 1f);
        }
      } else if (baseMultiplier < 1f) {
        if (distance <= middleRange) { varianceMultiplier = 1f; } else {
          varianceMultiplier = 1f - weapon.RangedDmgFalloffType(ratio) * (1f - baseMultiplier);
        }
      }
      var result =  varianceMultiplier;
      if (log) {
        Log.Combat?.TWL(0, $"defId: {weapon.defId}\n" +
                     $"baseMultiplier: {baseMultiplier}\n" +
                     $"distance: {distance}\n" +
                     $"start distance: {middleRange}\n" +
                     $"end distance {maxRange}\n" +
                     $"distanceRatio: {ratio}\n" +
                     $"result: {result}\n");
      }
      return result;
    }
    public static float WeaponDamageDistance(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage, bool log = true) {
      var varianceMultiplier = weapon.DistanceDamageMod(attackPos, target, log);
      var computedDamage = damage * varianceMultiplier;
      if (log) {
        Log.Combat?.TWL(0, $"varianceMultiplier: {varianceMultiplier}\n" +
                     $"damage: {damage}\n" +
                     $"computedDamage: {computedDamage}\n");
      }
      return computedDamage;
    }
    public static float RevDistanceDamageMod(this Weapon weapon, Vector3 attackPos, ICombatant target, bool log = true) {
      float baseMultiplier = weapon.DistantVariance();
      float varianceMultiplier = 1f;
      float distance = Vector3.Distance(attackPos, target.TargetPosition);
      float minRange = weapon.DamageFalloffStartDistance(); if (minRange < CustomAmmoCategories.Epsilon) { minRange = weapon.MinRange; }
      float middleRange = weapon.DamageFalloffEndDistance(); if (middleRange < minRange) { middleRange = weapon.MediumRange; };
      float ratio = 1f;
      if (distance < middleRange) {
        if ((middleRange - minRange) > CustomAmmoCategories.Epsilon) { ratio = (distance - minRange) / (middleRange - minRange); };
      }
      if (baseMultiplier > 1f) {
        if (distance <= minRange) { varianceMultiplier = 1f; } else {
          varianceMultiplier = 1f + weapon.RangedDmgFalloffType(ratio) * (baseMultiplier - 1f);
        }
      } else if (baseMultiplier < 1f) {
        if (distance <= minRange) { varianceMultiplier = baseMultiplier; } else {
          varianceMultiplier = baseMultiplier + weapon.RangedDmgFalloffType(ratio) * (1f - baseMultiplier);
        }
      }
      var result = varianceMultiplier;
      if (log) {
        Log.Combat?.TWL(0,$"defId: {weapon.defId}\n" +
                   $"baseMultiplier: {baseMultiplier}\n" +
                   $"distance: {distance}\n" +
                   $"start distance: {minRange}\n" +
                   $"end distance: {middleRange}\n" +
                   $"distanceRatio: {ratio}\n" +
                   $"result: {result}\n");
      }
      return result;
    }
    public static float WeaponDamageRevDistance(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage, bool log = true) {
      var varianceMultiplier = weapon.RevDistanceDamageMod(attackPos, target, log);
      var computedDamage = damage * varianceMultiplier;
      if (log) {
        Log.Combat?.TWL(0, $"varianceMultiplier: {varianceMultiplier}\n" +
                     $"damage: {damage}\n" +
                     $"computedDamage: {computedDamage}\n");
      }
      return computedDamage;
    }
    public static float TargetOverheatModifier(this Weapon weapon, ICombatant target, bool log = true) {
      if (weapon.IsOverheatModApplicable() == false) { return 1f; }
      var rawMultiplier = weapon.weaponDef.OverheatedDamageMultiplier;
      var effectActor = rawMultiplier < 0 ? weapon.parent : target;
      var multiplier = Mathf.Abs(rawMultiplier);
      var result = 1f;
      if (effectActor is Mech mech && mech.IsOverheated) {
        result = multiplier;
      }
      if (log) {
        var sb = new StringBuilder();
        sb.AppendLine($"OverheatedDamageMultiplier: {rawMultiplier}");
        sb.AppendLine(String.Format("effectActor: {0}", rawMultiplier < 0 ? "attacker" : "target"));
        sb.AppendLine($"multiplier: {multiplier}");
        Log.Combat?.TWL(0,sb.ToString());
      }
      return result;
    }
    public static float WeaponDamageTargetOverheat(ICombatant target, Weapon weapon, float rawDamage, bool log = true) {
      var multiplier = weapon.TargetOverheatModifier(target,log);
      var damage = rawDamage * multiplier;
      if (log) {
        var sb = new StringBuilder();
        sb.AppendLine($"rawDamage: {rawDamage}");
        sb.AppendLine($"damage: {damage}");
        Log.Combat?.TWL(0, sb.ToString());
      }
      return damage;
    }
    public static float WeaponDamageSimpleVariance(Weapon weapon, float rawDamage) {
      Log.LogWrite("Simple damage variance for weapon " + weapon.UIName + "\n");
      var damagePerShot = weapon.DamagePerShot;
      var adjustment = rawDamage / damagePerShot;
      var variance = weapon.DamageVariance();
      var roll = NormalDistribution.Random(
          new VarianceBounds(
              damagePerShot - variance,
              damagePerShot + variance,
              CustomAmmoCategories.getWRSettings().StandardDeviationSimpleVarianceMultiplier * variance
          ));
      var variantDamage = roll * adjustment;

      var sb = new StringBuilder();
      sb.AppendLine($" roll: {roll}");
      sb.AppendLine($" damagePerShot: {damagePerShot}");
      sb.AppendLine($" variance: {variance}");
      sb.AppendLine($" adjustment: {adjustment}");
      sb.AppendLine($" result: {variantDamage}");
      Log.Combat?.TWL(0, sb.ToString());
      return variantDamage;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpactDBG {
    [HarmonyPriority(Priority.Last)]
    public static void Prefix(ref bool __runOriginal, AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      if (!__runOriginal) { return; }
      try {
        AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
        if (impactMessage.hitInfo.attackSequenceId != __instance.id) {
          return;
        }
        Log.Combat?.WL(0, "Checking hitDamage last time:" + impactMessage.hitDamage);
        if (float.IsNaN(impactMessage.hitDamage)) {
          Log.Combat?.WL(1, "NaN - reparing\n");
          impactMessage.hitDamage = 0.1f;
        }
        if (float.IsInfinity(impactMessage.hitDamage)) {
          Log.Combat?.WL(1, "Infinity - reparing");
          impactMessage.hitDamage = 0.1f;
        }
        float qualityMultiplier = __instance.Director.Combat.ToHit.GetBlowQualityMultiplier(impactMessage.hitInfo.hitQualities[impactMessage.hitIndex]);
        Log.Combat?.WL(1, "checking blow quality multiplyer " + qualityMultiplier);
        if (float.IsNaN(qualityMultiplier)) {
          Log.Combat?.WL(1, "NaN - it can't be repaired, please check ResolutionConstants IneffectiveBlowDamageMultiplier/GlancingBlowDamageMultiplier/NormalBlowDamageMultiplier/SolidBlowDamageMultiplier");
        }
        if (float.IsInfinity(qualityMultiplier)) {
          Log.Combat?.WL(1, "Infinity - it can't be repaired, please check ResolutionConstants IneffectiveBlowDamageMultiplier/GlancingBlowDamageMultiplier/NormalBlowDamageMultiplier/SolidBlowDamageMultiplier");
        }
        __runOriginal = false;
        return;
      }catch(Exception e) {
        AttackDirector.logger.LogException(e);
      }
    }
  }

  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("GetAdjustedDamage")]
  [HarmonyPatch(new Type[] { typeof(float), typeof(WeaponCategoryValue), typeof(DesignMaskDef), typeof(LineOfFireLevel), typeof(bool) })]
  public static class AbstractActor_GetAdjustedDamage {
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(AbstractActor __instance, float incomingDamage, WeaponCategoryValue weaponCategoryValue, DesignMaskDef designMask, LineOfFireLevel lofLevel, bool doLogging, ref float __result) {
      //CustomAmmoCategoriesLog.Log.LogWrite("Checking GetAdjustedDamage incDmg:" + incomingDamage + "\n");
      if (float.IsNaN(__result)) {
        Log.Combat?.WL(0, "Checking GetAdjustedDamage incDmg:" + incomingDamage);
        Log.Combat?.WL(1, "but outdoing result NaN - reparing");
        __result = 0.1f;
      }
      if (float.IsInfinity(__result)) {
        Log.Combat?.WL(0, "Checking GetAdjustedDamage incDmg:" + incomingDamage);
        Log.Combat?.WL(1, "but outdoing result Infinity - reparing");
        __result = 0.1f;
      }
      return;
    }
  }

  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpact {
    public static bool isHasHeat(this ICombatant combatant) {
      Mech mech = combatant as Mech;
      if (mech == null) { return false; }
      if (combatant.FakeVehicle()) { return false; }
      if (combatant.NoHeat()) { return false; }
      try {
        foreach (string tag in CustomAmmoCategories.Settings.TransferHeatDamageToNormalTag) {
          if (mech.MechDef.Chassis.ChassisTags.Contains(tag)) { return false; }
        }
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static bool isHasStability(this ICombatant combatant) {
      Mech mech = combatant as Mech;
      if (mech == null) { return false; }
      if (combatant.FakeVehicle()) { return false; }
      if (combatant.NoStability()) { return false; }
      if(mech.IsShutDown) { return false; }
      try {
        foreach(string tag in CustomAmmoCategories.Settings.MechHasNoStabilityTag) {
          if (mech.MechDef.Chassis.ChassisTags.Contains(tag)) { return false; }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static float HeatDamage(this ICombatant combatant, float heat) {
      switch (combatant.UnitType) {
        case UnitType.Vehicle: return WeaponRealizer.Core.ModSettings.HeatDamageAppliesToVehicleAsNormalDamage ? heat * WeaponRealizer.Core.ModSettings.HeatDamageApplicationToVehicleMultiplier : 0f;
        case UnitType.Turret: return WeaponRealizer.Core.ModSettings.HeatDamageAppliesToTurretAsNormalDamage ? heat * WeaponRealizer.Core.ModSettings.HeatDamageApplicationToTurretMultiplier : 0f;
        case UnitType.Building: return WeaponRealizer.Core.ModSettings.HeatDamageAppliesToBuildingAsNormalDamage ? heat * WeaponRealizer.Core.ModSettings.HeatDamageApplicationToBuildingMultiplier : 0f;
        default: return heat;
      }
    }
    public static void Prefix(ref bool __runOriginal, AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      if (!__runOriginal) { return; }
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return; }
      int attackGroupIndex = impactMessage.hitInfo.attackGroupIndex;
      int attackWeaponIndex = impactMessage.hitInfo.attackWeaponIndex;
      AdvWeaponHitInfoRec advRec = impactMessage.hitInfo.advRec(impactMessage.hitIndex);
      Log.Combat?.TWL(0, "OnAttackSequenceImpact group:" + attackGroupIndex + " weapon:" + attackWeaponIndex + " shot:" + impactMessage.hitIndex + "/" + impactMessage.hitInfo.numberOfShots + "\n");
      if (advRec != null) {
        advRec.PlayImpact();
        __instance.OnAttackSequenceImpactAdv(message);
        __runOriginal = false;
        return;
      } else {
        Log.Combat?.TWL(0,"No advanced info found. This should not happend!",true);
        __runOriginal = false;
        return;
      }
    }
  }
}
