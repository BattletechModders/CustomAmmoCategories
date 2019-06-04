// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming

namespace WeaponRealizer
{
    public class Settings
    {
        public bool simpleVariance = true;
        public bool SimpleVariance => simpleVariance;

        public float standardDeviationPercentOfSimpleVariance = 75.0f;
        public float StandardDeviationSimpleVarianceMultiplier => standardDeviationPercentOfSimpleVariance / 100.0f;

        public bool distanceBasedVariance = true;
        public bool DistanceBasedVariance => distanceBasedVariance;

        public float distanceBasedVarianceMaxRangeDamagePercent = 10.0f;
        public float DistanceBasedVarianceMaxRangeDamageMultiplier => distanceBasedVarianceMaxRangeDamagePercent / 100.0f;

        public bool reverseDistanceBasedVariance = true;
        public bool ReverseDistanceBasedVariance => reverseDistanceBasedVariance;

        public float reverseDistanceBasedVarianceMinRangeDamagePercent = 10.0f;
        public float ReverseDistanceBasedVarianceMinRangeDamageMultiplier => reverseDistanceBasedVarianceMinRangeDamagePercent / 100.0f;
 
        public bool overheatModifier = true;
        public bool OverheatModifier => overheatModifier;

        public bool heatDamageModifier = false;
        public bool HeatDamageModifier => heatDamageModifier;

        public bool heatDamageAppliesToVehicleAsNormalDamage = true;
        public bool HeatDamageAppliesToVehicleAsNormalDamage => heatDamageAppliesToVehicleAsNormalDamage;

        public float heatDamagePercentApplicationToVehicle = 50f;
        public float HeatDamageApplicationToVehicleMultiplier => heatDamagePercentApplicationToVehicle / 100f;

        public bool heatDamageAppliesToTurretAsNormalDamage = true;
        public bool HeatDamageAppliesToTurretAsNormalDamage => heatDamageAppliesToTurretAsNormalDamage;

        public float heatDamagePercentApplicationToTurret = 75f;
        public float HeatDamageApplicationToTurretMultiplier => heatDamagePercentApplicationToTurret / 100f;

        public bool heatDamageAppliesToBuildingAsNormalDamage = true;
        public bool HeatDamageAppliesToBuildingAsNormalDamage => heatDamageAppliesToBuildingAsNormalDamage;

        public float heatDamagePercentApplicationToBuilding = 150f;
        public float HeatDamageApplicationToBuildingMultiplier => heatDamagePercentApplicationToBuilding / 100f;

        public bool ballisticNumberOfShots = true;
        public bool BallisticNumberOfShots => ballisticNumberOfShots;

        public bool clusteredBallistics = true;
        public bool ClusteredBallistics => clusteredBallistics;

        public bool laserNumberOfShots = true;
        public bool LaserNumberOfShots => laserNumberOfShots;

        public bool damageAltersWeaponRefireModifier = true;
        public bool DamageAltersWeaponRefireModifier => damageAltersWeaponRefireModifier;

        public float damagedWeaponRefireModifierMultiplier = 1.5f;
        public float DamagedWeaponRefireModifierMultiplier => damagedWeaponRefireModifierMultiplier;

        public bool jamming = true;
        public bool Jamming => jamming;

        public float jamChanceMultiplier = 1;
        public float JamChanceMultiplier => jamChanceMultiplier;

        public bool debug = false;
    }
}