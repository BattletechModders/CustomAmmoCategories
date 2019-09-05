using BattleTech;
using CustomAmmoCategoriesLog;

namespace WeaponRealizer {
  public static partial class Calculator {
    internal const float Epsilon = 0.0001f;

    public static float ApplyDamageModifiers(AbstractActor attacker, ICombatant target, Weapon weapon, float rawDamage) {
      return ApplyAllDamageModifiers(attacker, target, weapon, rawDamage, true);
    }

    internal static float ApplyAllDamageModifiers(AbstractActor attacker, ICombatant target, Weapon weapon, float rawDamage, bool calculateRandomComponent) {
      var damage = rawDamage;

      if (calculateRandomComponent && SimpleVariance.IsApplicable(weapon)) {
        Log.M.WL("WR simple variance:" + weapon.defId + " rawDamage:" + rawDamage );
        damage = SimpleVariance.Calculate(weapon, rawDamage);
      }

      if (DistanceBasedVariance.IsApplicable(weapon)) {
        Log.M.WL("WR distance variance:" + weapon.defId +" target:" +target.DisplayName+ " damage:" +damage+ " rawDamage:" + rawDamage );
        damage = DistanceBasedVariance.Calculate(attacker, target, weapon, damage, rawDamage);
      }

      if (ReverseDistanceBasedVariance.IsApplicable(weapon)) {
        Log.M.WL("WR reverse distance variance:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage);
        damage = ReverseDistanceBasedVariance.Calculate(attacker, target, weapon, damage, rawDamage);
      }

      if (OverheatMultiplier.IsApplicable(weapon)) {
        Log.M.WL("WR overheat muilt:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage);
        damage = OverheatMultiplier.Calculate(attacker, target, weapon, damage);
      }

      //if (HeatDamageModifier.IsApplicable(weapon))
      //{
      // TODO: this can't work becuse the values don't get ingested from weapondef
      // damage = HeatDamageModifier.Calculate(weapon, damage);
      //}

      //if (HeatAsNormalDamage.IsApplicable(weapon)) {
        //Log.M.WL("WR heat as normal damage:" + weapon.defId + " target:" + target.DisplayName + " damage:" + damage + " rawDamage:" + rawDamage + " heatdamage:" + heatDamage);
        //damage = HeatAsNormalDamage.Calculate(target, weapon, damage, rawDamage, heatDamage);
      //}

      return damage;
    }
  }
}