using BattleTech;

namespace WeaponRealizer
{
    public static partial class Calculator
    {
        private static class HeatDamageModifier
        {
            public static bool IsApplicable(Weapon _weapon)
            {
                // TODO: need a mechanism for this to support multiplier on both sides of an attack
                return false;
            }
        }
    }
}