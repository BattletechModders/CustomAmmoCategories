using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BattleTech;
using UnityEngine;

namespace WeaponRealizer {
  public static partial class Calculator {
    private static class DistanceBasedVariance {
      public static bool IsApplicable(Weapon weapon) {
        return Core.ModSettings.DistanceBasedVariance &&
               HasPositiveVarianceMultiplier(weapon);
      }
      private const double Pi2 = Math.PI / 2.0;
      private static readonly Dictionary<string, float> VarianceMultipliers = new Dictionary<string, float>();
      public static float Calculate(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage, bool log = true) {
        float varianceMultiplier;
        var distance = Vector3.Distance(attackPos, target.TargetPosition);
        var distanceDifference = weapon.MaxRange - distance;
        var distanceRatio = distanceDifference / weapon.MaxRange;
        var baseMultiplier = VarianceMultipliers[weapon.defId];
        var distanceBasedFunctionMultiplier = (float)Math.Atan(Pi2 * distanceRatio + baseMultiplier);
        if (distance < weapon.MaxRange) {
          varianceMultiplier = Mathf.Max(
              baseMultiplier,
              Mathf.Min(
                  1.0f,
                  distanceBasedFunctionMultiplier
              ));
        } else // out of range
          {
          return damage;
        }
        var computedDamage = damage * varianceMultiplier;// * adjustment;
        if (log) {
          Logger.Debug($"distanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
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
        }
        return computedDamage;
      }
      private static bool HasPositiveVarianceMultiplier(Weapon weapon) {
        if (!VarianceMultipliers.ContainsKey(weapon.defId))
          VarianceMultipliers[weapon.defId] = ParseBaseMultiplier(weapon);
        return VarianceMultipliers[weapon.defId] > Epsilon;
      }
      private const string DistanceVarianceTagPrefix = "wr-variance_by_distance";
      private static readonly char[] TagDelimiter = new char[] { '-' };
      private static float ParseBaseMultiplier(Weapon weapon) {
        if (!weapon.weaponDef.ComponentTags.Any(tag => tag.StartsWith(DistanceVarianceTagPrefix, StringComparison.InvariantCultureIgnoreCase)))
          return 0.0f;
        var rawTag = weapon.weaponDef.ComponentTags.First(tag => tag.StartsWith(DistanceVarianceTagPrefix, StringComparison.InvariantCultureIgnoreCase));
        var multiplier =
            rawTag == DistanceVarianceTagPrefix
                ? Core.ModSettings.DistanceBasedVarianceMaxRangeDamageMultiplier
                : float.Parse(rawTag.Split(TagDelimiter, 3).Last(), CultureInfo.InvariantCulture) / 100.0f;
        return multiplier;
      }
    }
    private static class ReverseDistanceBasedVariance {
      public static bool IsApplicable(Weapon weapon) {
        Logger.Debug($"reverse enabled? {Core.ModSettings.ReverseDistanceBasedVariance}\nvariance?{HasPositiveVarianceMultiplier(weapon)}");
        return Core.ModSettings.ReverseDistanceBasedVariance &&
               HasPositiveVarianceMultiplier(weapon);
      }

      private const double Pi2 = Math.PI / 2.0;
      private static readonly Dictionary<string, float> VarianceMultipliers = new Dictionary<string, float>();
      public static float Calculate(Vector3 attackPos, ICombatant target, Weapon weapon, float damage, float rawDamage, bool log = true) {
        //var damagePerShot = weapon.DamagePerShot;
        //var adjustment = damage / rawDamage;
        float varianceMultiplier;
        var distance = Vector3.Distance(attackPos, target.TargetPosition);
        var distanceDifference = weapon.MaxRange - distance;
        var distanceRatio = distanceDifference / weapon.MinRange;
        var baseMultiplier = VarianceMultipliers[weapon.defId];
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
        var computedDamage = damage * varianceMultiplier;// * adjustment;
        if (log) {
          Logger.Debug($"reverseDistanceBasedFunctionMultiplier: {distanceBasedFunctionMultiplier}\n" +
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
        }
        return computedDamage;
      }

      private static bool HasPositiveVarianceMultiplier(Weapon weapon) {
        if (!VarianceMultipliers.ContainsKey(weapon.defId))
          VarianceMultipliers[weapon.defId] = ParseBaseMultiplier(weapon);
        return VarianceMultipliers[weapon.defId] > Epsilon;
      }

      private const string ReverseDistanceVarianceTagPrefix = "wr-reverse_variance_by_distance";
      private static readonly char[] TagDelimiter = new char[] { '-' };
      private static float ParseBaseMultiplier(Weapon weapon) {
        if (!weapon.weaponDef.ComponentTags.Any(tag => tag.StartsWith(ReverseDistanceVarianceTagPrefix, StringComparison.InvariantCultureIgnoreCase)))
          return 0.0f;
        var rawTag = weapon.weaponDef.ComponentTags.First(tag => tag.StartsWith(ReverseDistanceVarianceTagPrefix, StringComparison.InvariantCultureIgnoreCase));
        var multiplier =
            rawTag == ReverseDistanceVarianceTagPrefix
                ? Core.ModSettings.ReverseDistanceBasedVarianceMinRangeDamageMultiplier
                : float.Parse(rawTag.Split(TagDelimiter, 3).Last()) / 100.0f;
        return multiplier;
      }
    }
  }
}