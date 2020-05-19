using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using BattleTech;
using Harmony;
using UnityEngine;
using System.Reflection;
using CustomAmmoCategoriesLog;

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
    public static float WeaponDamageDistance(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage) {
      //var damagePerShot = weapon.DamagePerShot;
      //var adjustment = rawDamage / damagePerShot;
      float varianceMultiplier;
      var distance = Vector3.Distance(attackPos, target.TargetPosition);
      var distanceDifference = weapon.MaxRange - distance;
      var distanceRatio = distanceDifference / weapon.MaxRange;
      var baseMultiplier = weapon.DistantVariance();
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
      var computedDamage = damage * varianceMultiplier; //* adjustment;
      CustomAmmoCategoriesLog.Log.LogWrite($"distanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
                   $"defId: {weapon.defId}\n" +
                   $"varianceMultiplier: {varianceMultiplier}\n" +
                   //$"adjustment: {adjustment}\n" +
                   $"damage: {damage}\n" +
                   $"distance: {distance}\n" +
                   $"max: {weapon.MaxRange}\n" +
                   $"distanceDifference {distanceDifference}\n" +
                   $"baseMultplier: {baseMultiplier}\n" +
                   $"distanceRatio: {distanceRatio}\n" +
                   $"computedDamage: {computedDamage}\n");
      return computedDamage;
    }
    public static float WeaponDamageRevDistance(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage) {
      //var damagePerShot = weapon.DamagePerShot;
      //var adjustment = rawDamage / damagePerShot;
      float varianceMultiplier;
      var distance = Vector3.Distance(attackPos, target.TargetPosition);
      var distanceDifference = weapon.MaxRange - distance;
      var distanceRatio = distanceDifference / weapon.MinRange;
      var baseMultiplier = weapon.DistantVariance();
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
      var computedDamage = damage * varianceMultiplier; //* adjustment;
      CustomAmmoCategoriesLog.Log.LogWrite($"reverseDistanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
                   $"defId: {weapon.defId}\n" +
                   $"varianceMultiplier: {varianceMultiplier}\n" +
                   //$"adjustment: {adjustment}\n" +
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
      Log.LogWrite(sb.ToString() + "\n");
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
    public static bool Prefix(AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) {
        return true;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Checking hitDamage last time:" + impactMessage.hitDamage + "\n");
      if (float.IsNaN(impactMessage.hitDamage)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" NaN - reparing\n");
        impactMessage.hitDamage = 0.1f;
      }
      if (float.IsInfinity(impactMessage.hitDamage)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Infinity - reparing\n");
        impactMessage.hitDamage = 0.1f;
      }
      float qualityMultiplier = __instance.Director.Combat.ToHit.GetBlowQualityMultiplier(impactMessage.hitInfo.hitQualities[impactMessage.hitIndex]);
      CustomAmmoCategoriesLog.Log.LogWrite(" checking blow quality multiplyer " + qualityMultiplier + "\n");
      if (float.IsNaN(qualityMultiplier)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" NaN - it can't be repaired, please check ResolutionConstants IneffectiveBlowDamageMultiplier/GlancingBlowDamageMultiplier/NormalBlowDamageMultiplier/SolidBlowDamageMultiplier\n");
      }
      if (float.IsInfinity(qualityMultiplier)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" Infinity - it can't be repaired, please check ResolutionConstants IneffectiveBlowDamageMultiplier/GlancingBlowDamageMultiplier/NormalBlowDamageMultiplier/SolidBlowDamageMultiplier\n");
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("GetAdjustedDamage")]
#if BT1_8
  [HarmonyPatch(new Type[] { typeof(float), typeof(WeaponCategoryValue), typeof(DesignMaskDef), typeof(LineOfFireLevel), typeof(bool) })]
#else
  [HarmonyPatch(new Type[] { typeof(float), typeof(WeaponCategory), typeof(DesignMaskDef), typeof(LineOfFireLevel), typeof(bool) })]
#endif
  public static class AbstractActor_GetAdjustedDamage {
    [HarmonyPriority(Priority.Last)]
#if BT1_8
    public static void Postfix(AbstractActor __instance, float incomingDamage, WeaponCategoryValue weaponCategoryValue, DesignMaskDef designMask, LineOfFireLevel lofLevel, bool doLogging, ref float __result) {
#else
    public static void Postfix(AbstractActor __instance, float incomingDamage, WeaponCategory category, DesignMaskDef designMask, LineOfFireLevel lofLevel, bool doLogging, ref float __result) {
#endif
      //CustomAmmoCategoriesLog.Log.LogWrite("Checking GetAdjustedDamage incDmg:" + incomingDamage + "\n");
      if (float.IsNaN(__result)) {
        CustomAmmoCategoriesLog.Log.LogWrite("Checking GetAdjustedDamage incDmg:" + incomingDamage + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" but outdoing result NaN - reparing\n");
        __result = 0.1f;
      }
      if (float.IsInfinity(__result)) {
        CustomAmmoCategoriesLog.Log.LogWrite("Checking GetAdjustedDamage incDmg:" + incomingDamage + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" but outdoing result Infinity - reparing\n");
        __result = 0.1f;
      }
      return;
    }
  }

  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpact {
    private static MethodInfo mAttackCompletelyMissed = null;
    public static bool Prepare() {
      mAttackCompletelyMissed = typeof(AttackDirector.AttackSequence).GetProperty("attackCompletelyMissed", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);
      if(mAttackCompletelyMissed == null) {
        Log.M.TWL(0,"WARNING! can't find AttackDirector.AttackSequence.attackCompletelyMissed setter",true);
        return false;
      }
      return true;
    }
    public static void attackCompletelyMissed(this AttackDirector.AttackSequence sequence, bool value) {
      if (mAttackCompletelyMissed == null) { return; }
      mAttackCompletelyMissed.Invoke(sequence, new object[1] { value });
    }
    public static bool isHasHeat(this ICombatant combatant) {
      Mech mech = combatant as Mech;
      if (mech == null) { return false; }
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

    public static bool Prefix(AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return true; }
      int attackGroupIndex = impactMessage.hitInfo.attackGroupIndex;
      int attackWeaponIndex = impactMessage.hitInfo.attackWeaponIndex;
      Weapon weapon = __instance.GetWeapon(attackGroupIndex, attackWeaponIndex);
      float rawDamage = impactMessage.hitDamage;
      float realDamage = rawDamage;
      float rawHeat = weapon.HeatDamagePerShot;
      //float heatToNormalPercantage = 1f;
      int hitLocation = impactMessage.hitInfo.hitLocations[impactMessage.hitIndex];
      ICombatant target = __instance.chosenTarget;
      AdvWeaponHitInfoRec advRec = impactMessage.hitInfo.advRec(impactMessage.hitIndex);
      bool isAOE = false;
      bool isFragMain = false;
      Log.LogWrite("OnAttackSequenceImpact group:" + attackGroupIndex + " weapon:" + attackWeaponIndex + " shot:" + impactMessage.hitIndex + "/" + impactMessage.hitInfo.numberOfShots + "\n");
      if (advRec != null) {
        rawDamage = advRec.Damage;
        realDamage = advRec.Damage;
        rawHeat = advRec.Heat;
        hitLocation = advRec.hitLocation;
        target = advRec.target;
        isAOE = advRec.isAOE;
        isFragMain = advRec.fragInfo.separated && (advRec.fragInfo.isFragPallet == false);
      } else {
        Log.LogWrite("No advanced info info.\n");
        if((impactMessage.hitInfo.DidShotHitChosenTarget(impactMessage.hitIndex) == false) && (impactMessage.hitInfo.DidShotHitAnything(impactMessage.hitIndex))) {
          target = __instance.Director.Combat.FindCombatantByGUID(impactMessage.hitInfo.secondaryTargetIds[impactMessage.hitIndex]);
          if (target == null) { target = __instance.chosenTarget; } else {
            hitLocation = impactMessage.hitInfo.secondaryHitLocations[impactMessage.hitIndex];
          }
        }
      }
      //if (hitLocation != 0) {
      //DynamicMapHelper.applyImpactMapChange(weapon, __instance.target.CurrentPosition);
      //}
      //CustomAmmoCategories.unregisterAMSCounterMeasure(impactMessage.hitInfo);
      CustomAmmoCategoriesLog.Log.LogWrite("  ");
      for (int t = 0; t < impactMessage.hitInfo.hitLocations.Length; ++t) {
        CustomAmmoCategoriesLog.Log.LogWrite("H:" + t + " L:" + impactMessage.hitInfo.hitLocations[t] + "/"+impactMessage.hitInfo.secondaryHitLocations[t]+" ");
      }
      CustomAmmoCategoriesLog.Log.LogWrite("\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  attacker = " + __instance.attacker.DisplayName + "\n");
      string trgInfo = "error"; if (target.GUID == impactMessage.hitInfo.targetId) { trgInfo = "primary"; } else if (target.GUID == impactMessage.hitInfo.secondaryTargetIds[impactMessage.hitIndex]) { trgInfo = "secondary"; }
      CustomAmmoCategoriesLog.Log.LogWrite("  target = " + target.DisplayName + ":"+ trgInfo + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  location = " + hitLocation + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon = " + weapon.UIName + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  damage = " + rawDamage + "/"+realDamage+"\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  heat = " + rawHeat + "\n");
      if (advRec != null) {
        if(advRec.target.isHasStability() == false) {
          advRec.Stability = 0f;
          CustomAmmoCategoriesLog.Log.LogWrite("  target has no stability\n");
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  stability = " + advRec.Stability + "\n");
      }
      CustomAmmoCategoriesLog.Log.LogWrite("  isAOE = " + (isAOE) + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  isFragMain = " + (isAOE) + "\n");
      if ((isAOE == true)||(isFragMain == true)) {
        Log.LogWrite("this is AOE/Frag greanade - no variance/impact processing for them\n");
      } else {
        //AdvWeaponHitInfoRec missile = impactMessage.hitInfo.advRec(impactMessage.hitIndex);
        bool intercepted = false;
        if (advRec != null) { if (advRec.interceptInfo.Intercepted) { intercepted = true; }; };
        if (intercepted == false) {
          weapon.SpawnAdditionalImpactEffect(impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
          if (hitLocation == 65536) {
            DynamicMapHelper.applyMineField(weapon, impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
            DynamicMapHelper.applyImpactBurn(weapon, impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
            DynamicMapHelper.applyImpactTempMask(weapon, impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
            DynamicMapHelper.applyCleanMinefield(weapon, impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
            weapon.CreateDifferedEffect(impactMessage.hitInfo.hitPositions[impactMessage.hitIndex]);
          } else
          if (hitLocation != 0) {
            if (weapon.FireOnSuccessHit()) { 
              DynamicMapHelper.applyImpactBurn(weapon, target.CurrentPosition);
              DynamicMapHelper.applyImpactTempMask(weapon, target.CurrentPosition);
              DynamicMapHelper.applyCleanMinefield(weapon, target.CurrentPosition);
            }
            weapon.CreateDifferedEffect(target);
          }
          //DynamicTreesHelper.clearTrees();
        } else {
          Log.LogWrite("Missile intercepted. No additional impact. No minefield.\n");
        }
        if (realDamage >= 1.0f) { 
          if (weapon.DamageVariance() > CustomAmmoCategories.Epsilon) {
            realDamage = CustomAmmoCategories.WeaponDamageSimpleVariance(weapon, rawDamage);
          } else {
            Log.M.WL("no simple variance defined");
          }
          if (weapon.DistantVariance() > CustomAmmoCategories.Epsilon) {
            if (weapon.DistantVarianceReversed() == false) {
              realDamage = CustomAmmoCategories.WeaponDamageDistance(__instance.attacker.TargetPosition, target, weapon, realDamage, rawDamage);
            } else {
              realDamage = CustomAmmoCategories.WeaponDamageRevDistance(__instance.attacker.TargetPosition, target, weapon, realDamage, rawDamage);
            }
          } else {
            Log.M.WL("no distance variance defined");
          }
          if ((hitLocation != 0) && (hitLocation != 65536)) {
            float CurArmor = target.ArmorForLocation(hitLocation);
            CustomAmmoCategoriesLog.Log.LogWrite("  location armor = " + CurArmor + "\n");
            float ArmorDmgMuil = weapon.ArmorDmgMult();
            if (CurArmor / ArmorDmgMuil > realDamage) {
              realDamage *= ArmorDmgMuil;
              CustomAmmoCategoriesLog.Log.LogWrite("  all damage to armor = " + realDamage + "\n");
            } else {
              float ISDdamagePart = (realDamage - CurArmor / ArmorDmgMuil) * weapon.ISDmgMult();
              CustomAmmoCategoriesLog.Log.LogWrite("  damage to armor = " + CurArmor + "\n");
              CustomAmmoCategoriesLog.Log.LogWrite("  part of damage to IS = " + ISDdamagePart + "\n");
              realDamage = CurArmor + ISDdamagePart;
            }
          }
          if (realDamage >= 1.0f) {
            Log.LogWrite("Applying WeaponRealizer variance. Current damage: " + realDamage + "\n");
            realDamage = WeaponRealizer.Calculator.ApplyDamageModifiers(__instance.attacker.TargetPosition, target, weapon, realDamage);
            Log.LogWrite("damage after WeaponRealizer variance: " + realDamage + "\n");
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! raw damage is less than 1.0f. Variance calculation is forbidden with this damage value\n", true);
        }
      }
      if (float.IsNaN(realDamage)) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! real damage is NaN. That is sad. Rounding to 0.1\n", true);
        realDamage = 0.1f;
      }
      if (float.IsInfinity(realDamage)) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! real damage is positive infinity. That is sad. Rounding to 0.1\n", true);
        realDamage = 0.1f;
      }
      if (realDamage < CustomAmmoCategories.Epsilon) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! real damage is less than epsilon. May be negative. That is sad. Rounding to 0.1\n", true);
        realDamage = 0.1f;
      }
      if (impactMessage.hitInfo.dodgeRolls[impactMessage.hitIndex] == -1.0f) {
        Log.LogWrite("AMS - no damage\n");
        realDamage = 0.1f;
        //impactMessage.hitInfo.hitLocations[impactMessage.hitIndex] = 65536;
      }
      if (weapon.isDamageVariation()) { impactMessage.hitDamage = realDamage; } else {
        Log.LogWrite(" damge variation forbidden by weapon's settings\n");
      }
      if (advRec != null) {
        if (weapon.isHeatVariation()) {
          advRec.Heat *= advRec.Damage > CustomAmmoCategories.Epsilon? (realDamage / advRec.Damage) : 0f;
          rawHeat = advRec.Heat;
        } else {
          Log.LogWrite(" heat variation forbidden by weapon's settings\n");
        }
        if (weapon.isStabilityVariation()) {
          advRec.Stability *= advRec.Damage > CustomAmmoCategories.Epsilon ? realDamage / advRec.Damage : 0f;
        } else {
          Log.LogWrite(" stability variation forbidden by weapon's settings\n");
        }
        if (weapon.isDamageVariation()) { advRec.Damage = realDamage; } else {
          Log.LogWrite(" damage variation forbidden by weapon's settings\n");
        }
        Log.LogWrite("  real damage = " + advRec.Damage + "\n");
        Log.LogWrite("  real heat = " + advRec.Heat + "\n");
        Log.LogWrite("  real stability = " + advRec.Stability + "\n");
      } else {
        Log.LogWrite("  real damage = " + impactMessage.hitDamage + "\n");
        if (weapon.isHeatVariation()) {
          rawHeat *= rawDamage > CustomAmmoCategories.Epsilon ? (realDamage / rawDamage) : 0f;
        } else {
          Log.LogWrite("  heat variation forbidden by weapon's settings\n");
        }
        Log.LogWrite("  real heat = " + rawHeat + "\n");
      }
      if ((target.isHasHeat() == false) && (rawHeat >= 0.5f)) {
        Log.M.WL("  heat damage exists, but target can't be heated");
        float heatAsNormal = target.HeatDamage(rawHeat);
        Log.M.WL("  heat transfered to normal damage:" + heatAsNormal);
        if (advRec != null) {
          advRec.Damage += heatAsNormal;
          advRec.Heat = 0f;
          Log.LogWrite("  real damage = " + advRec.Damage + "\n");
          Log.LogWrite("  real heat = " + advRec.Heat + "\n");
        } else {
          impactMessage.hitDamage += heatAsNormal;
          Log.LogWrite("  real damage = " + impactMessage.hitDamage + "\n");
        }
      }
      if (advRec == null) { return true; }
      __instance.OnAttackSequenceImpactAdv(message);
      return false;
    }
  }
}
