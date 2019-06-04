using System;
using System.Collections.Generic;
using BattleTech;
using Harmony;
using UnityEngine;

namespace WeaponRealizer
{
    [HarmonyPatch(typeof(AttackDirector.AttackSequence), "GenerateRandomCache")]
    static class ClusteredShotRandomCacheEnabler
    {
        static bool Prepare()
        {
            return Core.ModSettings.ClusteredBallistics;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ShotsWhenFired").GetGetMethod();
            var replacementMethod = AccessTools.Method(typeof(ClusteredShotRandomCacheEnabler),
                nameof(ShotsWhenFiredRandomizerOverider));
            return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
        }

        private static int ShotsWhenFiredRandomizerOverider(Weapon weapon)
        {
            if (!IsClustered(weapon)) return weapon.ShotsWhenFired;
            return weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
        }

        private static readonly Dictionary<string, bool> _isClustered = new Dictionary<string, bool>();
        private static bool IsClustered(Weapon weapon)
        {
            var weaponId = weapon.defId;
            if (!_isClustered.ContainsKey(weaponId))
            {
                _isClustered[weaponId] =
                    Core.ModSettings.ClusteredBallistics &&
                    weapon.weaponDef.ComponentTags.Contains(ClusteredShotEnabler.CLUSTER_TAG);
            }
            return _isClustered[weaponId];
        }
    }

    [HarmonyPatch(typeof(AttackDirector.AttackSequence), "GetIndividualHits")]
    static class ClusteredShotEnabler
    {
        internal const string CLUSTER_TAG = "wr-clustered_shots";
        private static FastInvokeHandler AttackSequenceGetClusteredHits;

        static bool Prepare()
        {
            if (!Core.ModSettings.ClusteredBallistics) return false;
            BuildAttackSequenceGetClusteredHits();
            return true;
        }

        private static void BuildAttackSequenceGetClusteredHits()
        {
            var mi = AccessTools.Method(typeof(AttackDirector.AttackSequence), "GetClusteredHits");
            AttackSequenceGetClusteredHits = MethodInvoker.GetHandler(mi);
        }

        static bool Prefix(ref WeaponHitInfo hitInfo, int groupIdx, int weaponIdx, Weapon weapon, float toHitChance,
            float prevDodgedDamage, AttackDirector.AttackSequence __instance)
        {
            if (!weapon.weaponDef.ComponentTags.Contains(CLUSTER_TAG)) return true;
            Logger.Debug("had the cluster tag");
            var newNumberOfShots = weapon.ProjectilesPerShot * hitInfo.numberOfShots;
            var originalNumberOfShots = hitInfo.numberOfShots;
            hitInfo.numberOfShots = newNumberOfShots;
            hitInfo.toHitRolls = new float[newNumberOfShots];
            hitInfo.locationRolls = new float[newNumberOfShots];
            hitInfo.dodgeRolls = new float[newNumberOfShots];
            hitInfo.dodgeSuccesses = new bool[newNumberOfShots];
            hitInfo.hitLocations = new int[newNumberOfShots];
            hitInfo.hitPositions = new Vector3[newNumberOfShots];
            hitInfo.hitVariance = new int[newNumberOfShots];
            hitInfo.hitQualities = new AttackImpactQuality[newNumberOfShots];
            AttackSequenceGetClusteredHits.Invoke(
                __instance,
                new object[] {hitInfo, groupIdx, weaponIdx, weapon, toHitChance, prevDodgedDamage}
            );

            PrintHitLocations(hitInfo);
            hitInfo.numberOfShots = originalNumberOfShots;
            return false;
        }

        private static void PrintHitLocations(WeaponHitInfo hitInfo)
        {
            if (!Core.ModSettings.debug) return;
            try
            {
                var output = "";
                output += $"clustered hits: {hitInfo.hitLocations.Length}\n";
                for (int i = 0; i < hitInfo.hitLocations.Length; i++)
                {
                    int location = hitInfo.hitLocations[i];
                    var chassisLocationFromArmorLocation =
                        MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation) location);

                    if (location == 0 || location == 65536)
                    {
                        output += $"hitLocation {i}: NONE/INVALID\n";
                    }
                    else
                    {
                        output += $"hitLocation {i}: {chassisLocationFromArmorLocation} ({location})\n";
                    }
                }
                Logger.Debug(output);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }
}