using System.Text;
using BattleTech;

namespace WeaponRealizer {
  public static partial class Calculator {
    private static class SimpleVariance {
      public static bool IsApplicable(Weapon weapon) {
        return Core.ModSettings.SimpleVariance &&
               weapon.weaponDef.DamageVariance > 0;
      }

      public static float Calculate(Weapon weapon, float rawDamage, bool log = true) {
        var damagePerShot = weapon.DamagePerShot;
        var adjustment = rawDamage / damagePerShot;
        var variance = weapon.weaponDef.DamageVariance;
        var roll = NormalDistribution.Random(
            new VarianceBounds(
                damagePerShot - variance,
                damagePerShot + variance,
                Core.ModSettings.StandardDeviationSimpleVarianceMultiplier * variance
            ));
        var variantDamage = roll * adjustment;
        if (log) {
          var sb = new StringBuilder();
          sb.AppendLine($"roll: {roll}");
          sb.AppendLine($"damagePerShot: {damagePerShot}");
          sb.AppendLine($"variance: {variance}");
          sb.AppendLine($"adjustment: {adjustment}");
          sb.AppendLine($"variantDamage: {variantDamage}");
          Logger.Debug(sb.ToString());
        }
        return variantDamage;
      }
    }
  }
}