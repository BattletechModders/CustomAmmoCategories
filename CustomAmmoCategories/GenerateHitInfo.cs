using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;

namespace CustomAmmoCategoriesPatches
{
    [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
    [HarmonyPatch("GenerateHitInfo")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(Weapon), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(float) })]
    public static class AttackSequence_GenerateHitInfo
    {
        private static void GetStreakHits(AttackDirector.AttackSequence instance,ref WeaponHitInfo hitInfo, int groupIdx, int weaponIdx, Weapon weapon, float toHitChance, float prevDodgedDamage)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("GetStreakHits\n");
            if (hitInfo.numberOfShots == 0) { return; };
            if (AttackDirector.hitLogger.IsLogEnabled)
                AttackDirector.hitLogger.Log((object)string.Format("???????? RANDOM HIT ROLLS (GetStreakHits): Weapon Group: {0} // Weapon: {1}", (object)groupIdx, (object)weaponIdx));
            hitInfo.toHitRolls = instance.GetRandomNumbers(groupIdx, weaponIdx, hitInfo.numberOfShots);
            if (AttackDirector.hitLogger.IsLogEnabled)
                AttackDirector.hitLogger.Log((object)string.Format("???????? RANDOM LOCATION ROLLS (GetStreakHits): Weapon Group: {0} // Weapon: {1}", (object)groupIdx, (object)weaponIdx));
            hitInfo.locationRolls = instance.GetRandomNumbers(groupIdx, weaponIdx, hitInfo.numberOfShots);
            if (AttackDirector.hitLogger.IsLogEnabled)
                AttackDirector.hitLogger.Log((object)string.Format("???????? DODGE ROLLS (GetStreakHits): Weapon Group: {0} // Weapon: {1}", (object)groupIdx, (object)weaponIdx));
            hitInfo.dodgeRolls = instance.GetRandomNumbers(groupIdx, weaponIdx, hitInfo.numberOfShots);
            hitInfo.hitVariance = instance.GetVarianceSums(groupIdx, weaponIdx, hitInfo.numberOfShots, weapon);
            int previousHitLocation = 0;
            float originalMultiplier = 1f;
            float adjacentMultiplier = 1f;
            AbstractActor target = instance.target as AbstractActor;
            Team team = weapon == null || weapon.parent == null || weapon.parent.team == null ? (Team)null : weapon.parent.team;
            bool primeSucceeded = false;
            bool primeFlag = false;
            for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex)
            {
                float corrRolls = (float)typeof(AttackDirector.AttackSequence).GetMethod("GetCorrectedRoll", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance,new object[2] { (object)hitInfo.toHitRolls[hitIndex], (object)team });
                //bool succeeded = (double)instance.GetCorrectedRoll(hitInfo.toHitRolls[hitIndex], team) <= (double)toHitChance;
                bool succeeded = (double)corrRolls <= (double)toHitChance;
                if (team != null)
                {
                    team.ProcessRandomRoll(toHitChance, succeeded);
                }
                bool flag = false;
                if (target != null)
                {
                    flag = target.CheckDodge(instance.attacker, weapon, hitInfo, hitIndex, instance.IsBreachingShot);
                }
                if(hitIndex == 0)
                {
                    primeSucceeded = succeeded;
                    primeFlag = flag;
                    CustomAmmoCategoriesLog.Log.LogWrite("  prime success:"+primeSucceeded+" dodge:"+primeFlag+"\n");
                }
                if (primeSucceeded && primeFlag)
                {
                    hitInfo.dodgeSuccesses[hitIndex] = true;
                    instance.FlagAttackContainsDodge();
                }else{
                    hitInfo.dodgeSuccesses[hitIndex] = false;
                }
                if (primeSucceeded && !primeFlag)
                {
                    if (previousHitLocation == 0)
                    {
                        previousHitLocation = instance.target.GetHitLocation(instance.attacker, instance.attackPosition, hitInfo.locationRolls[hitIndex], instance.calledShotLocation, instance.attacker.CalledShotBonusMultiplier);
                        hitInfo.hitLocations[hitIndex] = previousHitLocation;
                        CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
                        if (AttackDirector.attackLogger.IsLogEnabled)
                            AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} Initial streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
                        if (AttackDirector.hitminLogger.IsLogEnabled)
                            AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// INITIAL HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
                    }
                    else
                    {
                        hitInfo.hitLocations[hitIndex] = instance.target.GetAdjacentHitLocation(instance.attackPosition, hitInfo.locationRolls[hitIndex], previousHitLocation, originalMultiplier, adjacentMultiplier);
                        CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
                        if (AttackDirector.attackLogger.IsLogEnabled)
                            AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
                        if (AttackDirector.hitminLogger.IsLogEnabled)
                            AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// STREAK HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
                    }
                    hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, instance.target, instance.meleeAttackType, instance.IsBreachingShot);
                    instance.FlagShotHit();
                }
                else
                {
                    hitInfo.hitLocations[hitIndex] = 0;
                    CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
                    if (AttackDirector.attackLogger.IsLogEnabled)
                        AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} Miss!", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
                    if (AttackDirector.hitminLogger.IsLogEnabled)
                        AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Misses!", (object)weapon.Name, (object)hitIndex));
                    instance.FlagShotMissed();
                }
                hitInfo.hitPositions[hitIndex] = instance.target.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref hitInfo.hitLocations[hitIndex]);
            }
        }
        public static bool Prefix(AttackDirector.AttackSequence __instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage,ref WeaponHitInfo __result)
        {
            WeaponHitInfo hitInfo = new WeaponHitInfo();
            hitInfo.attackerId = __instance.attacker.GUID;
            hitInfo.targetId = __instance.target.GUID;
            hitInfo.numberOfShots = numberOfShots;
            hitInfo.stackItemUID = __instance.stackItemUID;
            hitInfo.attackSequenceId = __instance.id;
            hitInfo.attackGroupIndex = groupIdx;
            hitInfo.attackWeaponIndex = weaponIdx;
            hitInfo.toHitRolls = new float[numberOfShots];
            hitInfo.locationRolls = new float[numberOfShots];
            hitInfo.dodgeRolls = new float[numberOfShots];
            hitInfo.dodgeSuccesses = new bool[numberOfShots];
            hitInfo.hitLocations = new int[numberOfShots];
            hitInfo.hitPositions = new Vector3[numberOfShots];
            hitInfo.hitVariance = new int[numberOfShots];
            hitInfo.hitQualities = new AttackImpactQuality[numberOfShots];
            if (AttackDirector.hitLogger.IsLogEnabled)
            {
                Vector3 collisionWorldPos;
                LineOfFireLevel lineOfFire = __instance.Director.Combat.LOS.GetLineOfFire(__instance.attacker, __instance.attackPosition, __instance.target, __instance.target.CurrentPosition, __instance.target.CurrentRotation, out collisionWorldPos);
                float allModifiers = __instance.Director.Combat.ToHit.GetAllModifiers(__instance.attacker, weapon, __instance.target, __instance.attackPosition + __instance.attacker.HighestLOSPosition, __instance.target.TargetPosition, lineOfFire, __instance.isMoraleAttack);
                string modifiersDescription = __instance.Director.Combat.ToHit.GetAllModifiersDescription(__instance.attacker, weapon, __instance.target, __instance.attackPosition + __instance.attacker.HighestLOSPosition, __instance.target.TargetPosition, lineOfFire, __instance.isMoraleAttack);
                Pilot pilot = __instance.attacker.GetPilot();
                AttackDirector.hitLogger.Log((object)string.Format("======================================== Unit Firing: {0} | Weapon: {1} | Shots: {2}", (object)__instance.attacker.DisplayName, (object)weapon.Name, (object)numberOfShots));
                AttackDirector.hitLogger.Log((object)string.Format("======================================== Hit Info: GROUP {0} | ID {1}", (object)groupIdx, (object)weaponIdx));
                AttackDirector.hitLogger.Log((object)string.Format("======================================== MODIFIERS: {0}... FINAL: [[ {1} ]] ", (object)modifiersDescription, (object)allModifiers));
                if (pilot != null)
                    AttackDirector.hitLogger.Log((object)__instance.Director.Combat.ToHit.GetBaseToHitChanceDesc(__instance.attacker));
                else
                    AttackDirector.hitLogger.Log((object)string.Format("======================================== Gunnery Check: NO PILOT"));
            }
            float toHitChance = __instance.Director.Combat.ToHit.GetToHitChance(__instance.attacker, weapon, __instance.target, __instance.attackPosition, __instance.target.CurrentPosition, __instance.numTargets, __instance.meleeAttackType, __instance.isMoraleAttack);
            if (Mech.TEST_KNOCKDOWN)
                toHitChance = 1f;
            if (AttackDirector.hitLogger.IsLogEnabled)
                AttackDirector.hitLogger.Log((object)string.Format("======================================== HIT CHANCE: [[ {0:P2} ]]", (object)toHitChance));
            hitInfo.attackDirection = __instance.Director.Combat.HitLocation.GetAttackDirection(__instance.attackPosition, __instance.target);
            hitInfo.attackDirectionVector = __instance.Director.Combat.HitLocation.GetAttackDirectionVector(__instance.attackPosition, __instance.target);
            object[] args = new object[6];
            HitGeneratorType hitGenType = HitGeneratorType.NotSet;
            if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots"))
            {
                hitGenType = HitGeneratorType.Individual;
            }
            if (hitGenType == HitGeneratorType.NotSet)
            {
                if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true)
                {
                    string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
                    ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
                    hitGenType = extAmmo.HitGenerator;
                }
                if (hitGenType == HitGeneratorType.NotSet)
                {
                    ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.weaponDef.Description.Id);
                    hitGenType = extWeapon.HitGenerator;
                }
            }
            if (hitGenType != HitGeneratorType.NotSet)
            {
                switch (hitGenType)
                {
                    case HitGeneratorType.Individual:
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        break;
                    case HitGeneratorType.Cluster:
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetClusteredHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetClusteredHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    case HitGeneratorType.Streak:
                        //args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        //typeof(AttackDirector.AttackSequence).GetMethod("GetClusteredHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        //hitInfo = (WeaponHitInfo)args[0];
                        AttackSequence_GenerateHitInfo.GetStreakHits(__instance,ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        //__instance.GetClusteredHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    default:
                        AttackDirector.attackLogger.LogError((object)string.Format("GenerateHitInfo found invalid weapon type: {0}, using basic hit info", (object)hitGenType));
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetIndividualHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                }
            }
            else
            {
                switch (weapon.Type)
                {
                    case WeaponType.Autocannon:
                    case WeaponType.Gauss:
                    case WeaponType.Laser:
                    case WeaponType.PPC:
                    case WeaponType.Flamer:
                    case WeaponType.Melee:
                        args[0] = hitInfo;args[1] = groupIdx;args[2] = weaponIdx;args[3] = weapon;args[4] = toHitChance;args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetIndividualHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    case WeaponType.LRM:
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetClusteredHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetClusteredHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    case WeaponType.SRM:
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetIndividualHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    case WeaponType.MachineGun:
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetIndividualHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                    default:
                        AttackDirector.attackLogger.LogError((object)string.Format("GenerateHitInfo found invalid weapon type: {0}, using basic hit info", (object)weapon.Type));
                        args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
                        typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, args);
                        hitInfo = (WeaponHitInfo)args[0];
                        //__instance.GetIndividualHits(ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
                        break;
                }
            }
            __result = hitInfo;
            return false;
            //return hitInfo;
        }
    }
}
