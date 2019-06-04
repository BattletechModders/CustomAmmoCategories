using BattleTech;

namespace WeaponRealizer
{
    public static partial class Calculator
    {
        internal const float Epsilon = 0.0001f;

        public static float ApplyDamageModifiers(AbstractActor attacker, ICombatant target, Weapon weapon, float rawDamage)
        {
            return ApplyAllDamageModifiers(attacker, target, weapon, rawDamage, true);
        }

        internal static float ApplyAllDamageModifiers(AbstractActor attacker, ICombatant target, Weapon weapon, float rawDamage, bool calculateRandomComponent)
        {
            var damage = rawDamage;

            if (calculateRandomComponent && SimpleVariance.IsApplicable(weapon))
            {
                damage = SimpleVariance.Calculate(weapon, rawDamage);
            }

            if (DistanceBasedVariance.IsApplicable(weapon))
            {
                damage = DistanceBasedVariance.Calculate(attacker, target, weapon, damage, rawDamage);
            }

            if (ReverseDistanceBasedVariance.IsApplicable(weapon))
            {
                damage = ReverseDistanceBasedVariance.Calculate(attacker, target, weapon, damage, rawDamage);
            }

            if (OverheatMultiplier.IsApplicable(weapon))
            {
                damage = OverheatMultiplier.Calculate(attacker, target, weapon, damage);
            }

            if (HeatDamageModifier.IsApplicable(weapon))
            {
                // TODO: this can't work becuse the values don't get ingested from weapondef
                // damage = HeatDamageModifier.Calculate(weapon, damage);
            }

            if (HeatAsNormalDamage.IsApplicable(weapon))
            {
                damage = HeatAsNormalDamage.Calculate(target, weapon, damage, rawDamage);
            }

            return damage;
        }
    }
}