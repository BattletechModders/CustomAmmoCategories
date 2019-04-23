using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static HitGeneratorType getHitGenerator(Weapon weapon) {
      HitGeneratorType result = HitGeneratorType.NotSet;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      CustomAmmoCategoriesLog.Log.LogWrite("getHitGenerator " + weapon.defId + "\n");
      if (CustomAmmoCategories.isWeaponAOECapable(weapon)
        && (CustomAmmoCategories.getWeaponAOEDamage(weapon) < CustomAmmoCategories.Epsilon)
        && (CustomAmmoCategories.getWeaponAOEHeatDamage(weapon) < CustomAmmoCategories.Epsilon)
      ) {
        return HitGeneratorType.AOE;
      };
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots")) {
        result = HitGeneratorType.Individual;
        CustomAmmoCategoriesLog.Log.LogWrite(" contains wr-cluster\n");
      }
      result = extWeapon.HitGenerator;
      CustomAmmoCategoriesLog.Log.LogWrite(" per weapon def hit generator " + extWeapon.HitGenerator.ToString() + "\n");
      if (result == HitGeneratorType.NotSet) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            result = extWeapon.Modes[modeId].HitGenerator;
            CustomAmmoCategoriesLog.Log.LogWrite(" per mode hit generator " + extWeapon.Modes[modeId].HitGenerator.ToString() + "\n");
          }
        }
      }
      if (result == HitGeneratorType.NotSet) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
          string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
          result = CustomAmmoCategories.findExtAmmo(ammoId).HitGenerator;
          CustomAmmoCategoriesLog.Log.LogWrite(" per ammo hit generator " + result.ToString() + "\n");
        }
      }
      if (result == HitGeneratorType.NotSet) {
        switch (weapon.Type) {
          case WeaponType.Autocannon:
          case WeaponType.Gauss:
          case WeaponType.Laser:
          case WeaponType.PPC:
          case WeaponType.Flamer:
          case WeaponType.Melee:
            result = HitGeneratorType.Individual;
            break;
          case WeaponType.LRM:
            result = HitGeneratorType.Cluster;
            break;
          case WeaponType.SRM:
            result = HitGeneratorType.Individual;
            break;
          case WeaponType.MachineGun:
            result = HitGeneratorType.Individual;
            break;
          default:
            result = HitGeneratorType.Individual;
            break;
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" per weapon type hit generator " + result.ToString() + "\n");
      }
      return result;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("GenerateHitInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(float) })]
  public static class AttackSequence_GenerateHitInfo {
    private static void GetStreakHits(AttackDirector.AttackSequence instance, ref WeaponHitInfo hitInfo, int groupIdx, int weaponIdx, Weapon weapon, float toHitChance, float prevDodgedDamage) {
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
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        float corrRolls = (float)typeof(AttackDirector.AttackSequence).GetMethod("GetCorrectedRoll", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[2] { (object)hitInfo.toHitRolls[hitIndex], (object)team });
        //bool succeeded = (double)instance.GetCorrectedRoll(hitInfo.toHitRolls[hitIndex], team) <= (double)toHitChance;
        bool succeeded = (double)corrRolls <= (double)toHitChance;
        if (team != null) {
          team.ProcessRandomRoll(toHitChance, succeeded);
        }
        bool flag = false;
        if (target != null) {
          flag = target.CheckDodge(instance.attacker, weapon, hitInfo, hitIndex, instance.IsBreachingShot);
        }
        if (hitIndex == 0) {
          primeSucceeded = succeeded;
          primeFlag = flag;
          CustomAmmoCategoriesLog.Log.LogWrite("  prime success:" + primeSucceeded + " dodge:" + primeFlag + "\n");
        }
        if (primeSucceeded && primeFlag) {
          hitInfo.dodgeSuccesses[hitIndex] = true;
          instance.FlagAttackContainsDodge();
        } else {
          hitInfo.dodgeSuccesses[hitIndex] = false;
        }
        if (primeSucceeded && !primeFlag) {
          if (previousHitLocation == 0) {
            previousHitLocation = instance.target.GetHitLocation(instance.attacker, instance.attackPosition, hitInfo.locationRolls[hitIndex], instance.calledShotLocation, instance.attacker.CalledShotBonusMultiplier);
            hitInfo.hitLocations[hitIndex] = previousHitLocation;
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} Initial streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// INITIAL HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          } else {
            hitInfo.hitLocations[hitIndex] = instance.target.GetAdjacentHitLocation(instance.attackPosition, hitInfo.locationRolls[hitIndex], previousHitLocation, originalMultiplier, adjacentMultiplier);
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// STREAK HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          }
          hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, instance.target, instance.meleeAttackType, instance.IsBreachingShot);
          instance.FlagShotHit();
        } else {
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
    private static void GetAOEHits(AttackDirector.AttackSequence instance, ref WeaponHitInfo hitInfo, int groupIdx, int weaponIdx, Weapon weapon, float toHitChance, float prevDodgedDamage) {
      CustomAmmoCategoriesLog.Log.LogWrite("GetAOEHits\n");
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
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        float corrRolls = (float)typeof(AttackDirector.AttackSequence).GetMethod("GetCorrectedRoll", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[2] { (object)hitInfo.toHitRolls[hitIndex], (object)team });
        //bool succeeded = (double)instance.GetCorrectedRoll(hitInfo.toHitRolls[hitIndex], team) <= (double)toHitChance;
        bool succeeded = (double)corrRolls <= (double)toHitChance;
        if (team != null) {
          team.ProcessRandomRoll(toHitChance, succeeded);
        }
        bool flag = false;
        if (target != null) {
          flag = target.CheckDodge(instance.attacker, weapon, hitInfo, hitIndex, instance.IsBreachingShot);
        }
        if (hitIndex == 0) {
          primeSucceeded = false;
          primeFlag = true;
          CustomAmmoCategoriesLog.Log.LogWrite("  prime success:" + primeSucceeded + " dodge:" + primeFlag + "\n");
        }
        if (primeSucceeded && primeFlag) {
          hitInfo.dodgeSuccesses[hitIndex] = true;
          instance.FlagAttackContainsDodge();
        } else {
          hitInfo.dodgeSuccesses[hitIndex] = false;
        }
        if (primeSucceeded && !primeFlag) {
          if (previousHitLocation == 0) {
            previousHitLocation = instance.target.GetHitLocation(instance.attacker, instance.attackPosition, hitInfo.locationRolls[hitIndex], instance.calledShotLocation, instance.attacker.CalledShotBonusMultiplier);
            hitInfo.hitLocations[hitIndex] = previousHitLocation;
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} Initial streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// INITIAL HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          } else {
            hitInfo.hitLocations[hitIndex] = instance.target.GetAdjacentHitLocation(instance.attackPosition, hitInfo.locationRolls[hitIndex], previousHitLocation, originalMultiplier, adjacentMultiplier);
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// STREAK HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          }
          hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, instance.target, instance.meleeAttackType, instance.IsBreachingShot);
          instance.FlagShotHit();
        } else {
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

    public static void generateWeaponHitInfo(AttackDirector.AttackSequence instance, ICombatant target, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage, ref WeaponHitInfo hitInfo) {
      ICombatant originaltarget = instance.target;
      instance.target = target;
      CustomAmmoCategoriesLog.Log.LogWrite("generateWeaponHitInfo\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" altering target:" + originaltarget.GUID + "->" + target.GUID + "\n");
      float toHitChance = instance.Director.Combat.ToHit.GetToHitChance(instance.attacker, weapon, target, instance.attackPosition, target.CurrentPosition, instance.numTargets, instance.meleeAttackType, instance.isMoraleAttack);
      CustomAmmoCategoriesLog.Log.LogWrite(" filling to hit records " + target.DisplayName + " " + target.GUID + " weapon:" + weapon.defId + " shots:" + hitInfo.numberOfShots + " toHit:" + toHitChance + "\n");
      if (Mech.TEST_KNOCKDOWN)
        toHitChance = 1f;
      if (AttackDirector.hitLogger.IsLogEnabled)
        AttackDirector.hitLogger.Log((object)string.Format("======================================== HIT CHANCE: [[ {0:P2} ]]", (object)toHitChance));
      hitInfo.attackDirection = instance.Director.Combat.HitLocation.GetAttackDirection(instance.attackPosition, target);
      hitInfo.attackDirectionVector = instance.Director.Combat.HitLocation.GetAttackDirectionVector(instance.attackPosition, instance.target);
      object[] args = new object[6];
      HitGeneratorType hitGenType = CustomAmmoCategories.getHitGenerator(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite(" Hit generator:" + hitGenType + "\n");
      switch (hitGenType) {
        case HitGeneratorType.Individual:
          args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
          typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args);
          hitInfo = (WeaponHitInfo)args[0];
          break;
        case HitGeneratorType.Cluster:
          args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
          typeof(AttackDirector.AttackSequence).GetMethod("GetClusteredHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args);
          hitInfo = (WeaponHitInfo)args[0];
          break;
        case HitGeneratorType.Streak:
          AttackSequence_GenerateHitInfo.GetStreakHits(instance, ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
          break;
        case HitGeneratorType.AOE:
          AttackSequence_GenerateHitInfo.GetAOEHits(instance, ref hitInfo, groupIdx, weaponIdx, weapon, toHitChance, dodgedDamage);
          break;
        default:
          AttackDirector.attackLogger.LogError((object)string.Format("GenerateHitInfo found invalid weapon type: {0}, using basic hit info", (object)hitGenType));
          args[0] = hitInfo; args[1] = groupIdx; args[2] = weaponIdx; args[3] = weapon; args[4] = toHitChance; args[5] = dodgedDamage;
          typeof(AttackDirector.AttackSequence).GetMethod("GetIndividualHits", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args);
          hitInfo = (WeaponHitInfo)args[0];
          break;
      }
      if (hitInfo.numberOfShots != hitInfo.hitLocations.Length) {
        CustomAmmoCategoriesLog.Log.LogWrite(" strange behavior. NumberOfShots: " + hitInfo.numberOfShots + " but HitLocations length:" + hitInfo.hitLocations.Length + ". Must be equal\n", true);
        hitInfo.numberOfShots = hitInfo.hitLocations.Length;
      }
      if (instance.attacker.GUID == target.GUID) {
        Vector3 terrainPos = CustomAmmoCategories.getTerrinHitPosition(instance.stackItemUID);
        if (terrainPos != Vector3.zero) {
          CustomAmmoCategoriesLog.Log.LogWrite(" terrain attack detected to " + terrainPos + "\n");
          for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
            hitInfo.hitLocations[hitIndex] = 65536;
            hitInfo.hitPositions[hitIndex] = terrainPos + (hitInfo.hitPositions[hitIndex] - target.CurrentPosition);
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" result(" + hitInfo.numberOfShots + "):");
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        CustomAmmoCategoriesLog.Log.LogWrite(" " + hitInfo.hitLocations[hitIndex] + "/" + hitInfo.locationRolls[hitIndex]);
      }
      CustomAmmoCategoriesLog.Log.LogWrite("\n");
      instance.target = originaltarget;
    }

    public static bool Prefix(AttackDirector.AttackSequence __instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage, ref WeaponHitInfo __result) {
      CustomAmmoCategoriesLog.Log.LogWrite("Generating HitInfo " + weapon.defId + " grp:" + groupIdx + " id:" + weaponIdx + " shots:" + numberOfShots + " indirect:" + indirectFire + " " + dodgedDamage + "\n");
      try {
        WeaponHitInfo hitInfo = new WeaponHitInfo();
        //CustomAmmoCategories.
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
        CustomAmmoCategoriesLog.Log.LogWrite(" hit info created\n");
        if (AttackDirector.hitLogger.IsLogEnabled) {
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
        if (CustomAmmoCategories.getWeaponSpreadRange(weapon) > CustomAmmoCategories.Epsilon) {
          CustomAmmoCategoriesLog.Log.LogWrite(" Weapon has spread\n");
          List<SpreadHitInfo> spreadList = CustomAmmoCategories.prepareSpreadHitInfo(__instance, weapon, groupIdx, weaponIdx, numberOfShots, indirectFire, dodgedDamage);
          if (spreadList.Count > 1) {
            foreach (SpreadHitInfo spHitInfo in spreadList) {
              ICombatant spreadTarget = __instance.Director.Combat.FindCombatantByGUID(spHitInfo.targetGUID);
              if (spreadTarget == null) { continue; };
              generateWeaponHitInfo(__instance, spreadTarget, weapon, groupIdx, weaponIdx, spHitInfo.hitInfo.numberOfShots, indirectFire, dodgedDamage, ref spHitInfo.hitInfo);
            }
            bool consResult = CustomAmmoCategories.ConsolidateSpreadHitInfo(spreadList, ref hitInfo);
            if (consResult == false) {
              CustomAmmoCategoriesLog.Log.LogWrite("fallback to default\n", true);
              generateWeaponHitInfo(__instance, __instance.target, weapon, groupIdx, weaponIdx, numberOfShots, indirectFire, dodgedDamage, ref hitInfo);
            }
          } else {
            generateWeaponHitInfo(__instance, __instance.target, weapon, groupIdx, weaponIdx, numberOfShots, indirectFire, dodgedDamage, ref hitInfo);
          }
        } else {
          generateWeaponHitInfo(__instance, __instance.target, weapon, groupIdx, weaponIdx, numberOfShots, indirectFire, dodgedDamage, ref hitInfo);
        }
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        if (extWeapon.StreakEffect == true) {
          if (__instance.attacker.GUID != __instance.target.GUID) {
            CustomAmmoCategoriesLog.Log.LogWrite("Streak detected. Clearing missed.\n");
            WeaponHitInfo streakHitInfo = CustomAmmoCategories.getSuccessOnly(hitInfo);
            CustomAmmoCategories.ReturnNoFireHeat(weapon, hitInfo.stackItemUID, streakHitInfo.numberOfShots);
            CustomAmmoCategories.DecrementAmmo(weapon, streakHitInfo.stackItemUID, streakHitInfo.numberOfShots);
            hitInfo = streakHitInfo;
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("Streak detected. But terrain attack. No misses clearence performed.\n");
          }
        }
        //if (extWeapon.AMSImmune != TripleBoolean.True) {
        if (weapon.weaponRep != null) {
          MissileLauncherEffect missileLauncherEffect = weapon.weaponRep.WeaponEffect as MissileLauncherEffect;
          if (missileLauncherEffect != null) {
            CustomAmmoCategoriesLog.Log.LogWrite("Missile launcher detected. Pre generating trajectories\n");
            if (__instance.isMelee == false) {
              bool AMSImune = CustomAmmoCategories.getWeaponAMSImmune(weapon);
              bool IsUnguided = CustomAmmoCategories.getWeaponUnguided(weapon);
              for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
                CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.defId + " " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " " + hitIndex + " location:" + hitInfo.hitLocations[hitIndex] + "\n");
                CustomAmmoCategories.generateMissileCacheCurve(weapon, hitInfo, hitIndex, indirectFire, AMSImune, IsUnguided);
              }
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite("WARNING! " + weapon.defId + " is in melee attack. AMS can't intercept missiles in melee attack\n", true);
            }
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! " + weapon.defId + " has no weapon representation it is so sad ...\n", true);
        }
        //} else {
        //CustomAmmoCategoriesLog.Log.LogWrite("  "+weapon.defId + " is immune to AMS\n");
        //}
        if (CustomAmmoCategories.getWeaponHasShells(weapon)) {
          CustomAmmoCategoriesLog.Log.LogWrite("Shrapnel detected. Forsed early explode\n");
          bool IsUnguided = CustomAmmoCategories.getWeaponUnguided(weapon);
          for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
            CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.defId + " " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " " + hitIndex + " location:" + hitInfo.hitLocations[hitIndex] + "\n");
            CustomAmmoCategories.shrapnellEarlyExplode(__instance.Director.Combat, weapon, ref hitInfo, hitIndex, indirectFire, IsUnguided, __instance.target);
          }
        }
        __result = hitInfo;
        return false;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Generating HitInfo Exception:\n");
        CustomAmmoCategoriesLog.Log.LogWrite(e.ToString() + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("fallback to default\n");
        return true;
      }
    }
  }
}
