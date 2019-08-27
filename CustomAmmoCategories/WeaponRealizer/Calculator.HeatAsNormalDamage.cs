using BattleTech;

namespace WeaponRealizer
{
    public static partial class Calculator
    {
        private static class HeatAsNormalDamage
        {
            public static bool IsApplicable(Weapon weapon)
            {
                return (Core.ModSettings.HeatDamageAppliesToBuildingAsNormalDamage ||
                        Core.ModSettings.HeatDamageAppliesToVehicleAsNormalDamage ||
                        Core.ModSettings.HeatDamageAppliesToTurretAsNormalDamage) &&
                       weapon.HeatDamagePerShot > Epsilon;
            }

            public static float Calculate(ICombatant target, Weapon weapon, float currentDamage, float rawDamage, float heatDamage)
            {
                var damage = currentDamage;
                if (Core.ModSettings.HeatDamageAppliesToBuildingAsNormalDamage && target is BattleTech.Building)
                {
                    var damagePerShot = weapon.DamagePerShot;
                    var adjustment = rawDamage / damagePerShot * Core.ModSettings.HeatDamageApplicationToBuildingMultiplier;
                    damage = currentDamage + (adjustment * heatDamage);
                }
                else if (Core.ModSettings.HeatDamageAppliesToVehicleAsNormalDamage && target is Vehicle)
                {
                    var damagePerShot = weapon.DamagePerShot;
                    var adjustment = rawDamage / damagePerShot * Core.ModSettings.HeatDamageApplicationToVehicleMultiplier;
                    damage = currentDamage + (adjustment * heatDamage);
                }
                else if (Core.ModSettings.HeatDamageAppliesToTurretAsNormalDamage && target is Turret)
                {
                    var damagePerShot = weapon.DamagePerShot;
                    var adjustment = rawDamage / damagePerShot * Core.ModSettings.HeatDamageApplicationToTurretMultiplier;
                    damage = currentDamage + (adjustment * heatDamage);
                }
                return damage;
            }
        }
    }
}