using System;
using System.Text;
using BattleTech;
using UnityEngine;

namespace WeaponRealizer {
  public static partial class Calculator {
    private static class OverheatMultiplier {
      public static bool IsApplicable(Weapon weapon) {
        return Core.ModSettings.OverheatModifier &&
               Mathf.Abs(weapon.weaponDef.OverheatedDamageMultiplier) > Epsilon;
      }

      public static float Calculate(AbstractActor attacker, ICombatant target, Weapon weapon, float rawDamage, bool log = true) {
        var rawMultiplier = weapon.weaponDef.OverheatedDamageMultiplier;
        var effectActor = rawMultiplier < 0 ? attacker : target;
        var multiplier = Mathf.Abs(rawMultiplier);
        var damage = rawDamage;
        if (effectActor is Mech mech && mech.IsOverheated) {
          damage = rawDamage * multiplier;
        }
        if (log) {
          var sb = new StringBuilder();
          sb.AppendLine($"rawMultiplier: {rawMultiplier}");
          sb.AppendLine(String.Format("effectActor: {0}", rawMultiplier < 0 ? "attacker" : "target"));
          sb.AppendLine($"multiplier: {multiplier}");
          sb.AppendLine($"rawDamage: {rawDamage}");
          sb.AppendLine($"damage: {damage}");
          Logger.Debug(sb.ToString());
        }
        return damage;
      }
    }
  }
}