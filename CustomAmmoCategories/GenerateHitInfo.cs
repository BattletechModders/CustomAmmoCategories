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
using CustomAmmoCategoriesLog;
using Localize;
using System.Reflection.Emit;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static HitGeneratorType HitGenerator(this Weapon weapon) {
      HitGeneratorType result = HitGeneratorType.NotSet;
      CustomAmmoCategoriesLog.Log.LogWrite("getHitGenerator " + weapon.defId + "\n");
      if (weapon.AOECapable()
        && (weapon.AOEDamage() < CustomAmmoCategories.Epsilon)
        && (weapon.AOEHeatDamage() < CustomAmmoCategories.Epsilon)
      ) {
        return HitGeneratorType.AOE;
      };
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots")) {
        result = HitGeneratorType.Individual;
        CustomAmmoCategoriesLog.Log.LogWrite(" contains wr-cluster\n");
        return result;
      }
      result = HitGeneratorType.NotSet;
      WeaponMode mode = weapon.mode();
      if (mode.HitGenerator != HitGeneratorType.NotSet) {
        CustomAmmoCategoriesLog.Log.LogWrite(" per mode hit generator " + mode.HitGenerator.ToString() + "\n");
        result = mode.HitGenerator;
      } else {
        ExtAmmunitionDef ammo = weapon.ammo();
        if (ammo.HitGenerator != HitGeneratorType.NotSet) {
          result = ammo.HitGenerator;
          CustomAmmoCategoriesLog.Log.LogWrite(" per ammo hit generator " + result.ToString() + "\n");
        } else {
          ExtWeaponDef def = weapon.exDef();
          if(def.HitGenerator != HitGeneratorType.NotSet) {
            result = def.HitGenerator;
            CustomAmmoCategoriesLog.Log.LogWrite(" per weapon def hit generator " + result.ToString() + "\n");
          }
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
  /*[HarmonyPatch(typeof(LineOfSight))]
  [HarmonyPatch("GetImpactPosition")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Weapon), typeof(int), typeof(AttackDirection), typeof(string), typeof(int) })]
  public static class LineOfSight_GetImpactPosition {
    public static void Postfix(AbstractActor attacker, ICombatant initialTarget, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation, ref Vector3 __result) {
      Log.LogWrite("LineOfSight.GetImpactPosition " + initialTarget.DisplayName+":"+ initialTarget.GUID+" attaker:"+attacker.GUID+"\n");
      if (initialTarget.GUID == attacker.GUID) { return; };
      Vehicle vehicle = initialTarget as Vehicle;
      if (vehicle == null) {
        Log.LogWrite(" not vehicle\n");
        return;
      }
      string chassisName = string.Empty;
      if (String.IsNullOrEmpty(secondaryTargetId)) {
        chassisName = vehicle.VehicleDef.Chassis.Description.Id;
      } else {
        AbstractActor secondaryTrg = vehicle.Combat.FindActorByGUID(secondaryTargetId);
        if (secondaryTargetId == null) { return; }
        vehicle = secondaryTrg as Vehicle;
        if (vehicle == null) { return; }
        chassisName = vehicle.VehicleDef.Chassis.Description.Id;
      }
      if (string.IsNullOrEmpty(chassisName)) { return; };
      Log.LogWrite(" chassisName:"+chassisName+"\n");
      if (CustomAmmoCategories.Settings.VTOLHeightFix.ContainsKey(chassisName) == false) { return; };
      Log.LogWrite(" old pos:"+__result);
      __result.y += CustomAmmoCategories.Settings.VTOLHeightFix[chassisName];
      Log.LogWrite(" new pos:" + __result+"\n");
    }
  }*/
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("GenerateHitInfo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(float) })]
  public static class AttackSequence_GenerateHitInfo {
    private delegate float GetCorrectedRollDelegate(AttackDirector.AttackSequence seq, float roll, Team team);
    private static GetCorrectedRollDelegate GetCorrectedRollInvoke = null;
    public static bool Prepare() {
      MethodInfo GetCorrectedRoll = typeof(AttackDirector.AttackSequence).GetMethod("GetCorrectedRoll", BindingFlags.NonPublic | BindingFlags.Instance);
      if (GetCorrectedRoll == null) {
        Log.LogWrite("Can't find AttackDirector.AttackSequence.GetCorrectedRoll\n");
        return false;
      }
      var dm = new DynamicMethod("CACGetCorrectedRoll", typeof(float), new Type[] { typeof(AttackDirector.AttackSequence), typeof(float), typeof(Team) }, typeof(AttackDirector.AttackSequence));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Ldarg_2);
      gen.Emit(OpCodes.Call, GetCorrectedRoll);
      gen.Emit(OpCodes.Ret);
      GetCorrectedRollInvoke = (GetCorrectedRollDelegate)dm.CreateDelegate(typeof(GetCorrectedRollDelegate));
      return true;
    }
    public static float GetCorrectedRoll(this AttackDirector.AttackSequence seq, float roll, Team team) {
      return GetCorrectedRollInvoke(seq, roll, team);
    }
    public static Vector3 getMissInCircleToPosition(this CombatGameState combat, AttackDirector.AttackSequence seq, Vector3 centerPos, Weapon weapon, float toHitRoll) {
      Log.M.TWL(0, "getMissInCircleToPosition: weapon "+weapon.defId+" pos:"+centerPos+" roll:"+toHitRoll);
      Vector3 position = centerPos;
      float minradius = 5f;
      Team team = weapon == null || weapon.parent == null || weapon.parent.team == null ? (Team)null : weapon.parent.team;
      float correctedRolls = seq.GetCorrectedRoll(toHitRoll, team);
      float hitMargin = correctedRolls;
      if (hitMargin < 0f) { hitMargin = 0f; };
      minradius = Mathf.Max(minradius, weapon.MinMissRadius());
      float maxradius = weapon.MaxMissRadius();
      if ((maxradius - minradius) < CustomAmmoCategories.Epsilon) { maxradius = minradius * 3f; }
      float radius = Mathf.Lerp(minradius, maxradius, hitMargin);
      float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
      //radius *= UnityEngine.Random.Range(combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
      position.x += Mathf.Sin(direction)*radius;
      position.z += Mathf.Cos(direction)*radius;
      position.y = combat.MapMetaData.GetLerpedHeightAt(position);
      Log.M.WL(1, "radius:"+radius+" realdistance:"+Vector3.Distance(centerPos,position));
      return position;
    }
    public static Vector3 getMissPositionRadius(this GameRepresentation targetRep, AttackDirector.AttackSequence seq, Weapon weapon, float toHitChance, float toHitRoll) {
      TurretRepresentation tRep = targetRep as TurretRepresentation;
      MechRepresentation mRep = targetRep as MechRepresentation;
      VehicleRepresentation vRep = targetRep as VehicleRepresentation;
      Vector3 position = targetRep.thisTransform.position;
      Team team = weapon == null || weapon.parent == null || weapon.parent.team == null ? (Team)null : weapon.parent.team;
      float minradius = 5f;
      if (mRep != null) {
        position = mRep.vfxCenterTorsoTransform.position;
        position.y = mRep.vfxCenterTorsoTransform.position.y;
        minradius = mRep.parentMech.MechDef.Chassis.Radius;
        //radius = mRep.parentMech.MechDef.Chassis.Radius * UnityEngine.Random.Range(mRep.Constants.ResolutionConstants.MissOffsetHorizontalMin, mRep.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      } else
      if (tRep != null) {
        position = tRep.TurretLOS.position;
      } else
      if (vRep != null) {
        position = vRep.TurretLOS.position;
      }
      Log.M.TWL(0, "getMissPositionRadius: weapon " + weapon.defId + " pos:" + position + " chance:" + toHitChance + " roll:" + toHitRoll);
      float correctedRolls = seq.GetCorrectedRoll(toHitRoll, team);
      float hitMargin = (correctedRolls - toHitChance) / (1 - toHitChance);
      if (hitMargin < 0f) { hitMargin = 0f; };
      minradius = Mathf.Max(minradius, weapon.MinMissRadius());
      float maxradius = weapon.MaxMissRadius();
      if ((maxradius - minradius) < CustomAmmoCategories.Epsilon) { maxradius = minradius * 3f; }
      float radius = Mathf.Lerp(minradius, maxradius, hitMargin);
      float direction = Mathf.Deg2Rad * UnityEngine.Random.Range(0f, 360f);
      //radius *= UnityEngine.Random.Range(targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMin, targetRep.parentCombatant.Combat.Constants.ResolutionConstants.MissOffsetHorizontalMax);
      //Vector2 vector2 = UnityEngine.Random.insideUnitCircle.normalized * radius;
      Vector3 centerPosition = position;
      position.x += Mathf.Sin(direction) * radius;
      position.z += Mathf.Cos(direction) * radius;
      Log.M.WL(1, "radius:" + radius + " realdistance:" + Vector3.Distance(centerPosition, position));
      return position;
    }
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
      int primeHitLocation = 0;
      float originalMultiplier = 1f;
      float adjacentMultiplier = 1f;
      AbstractActor target = instance.chosenTarget as AbstractActor;
      Team team = weapon == null || weapon.parent == null || weapon.parent.team == null ? (Team)null : weapon.parent.team;
      ICombatant primeTarget = instance.chosenTarget;
      Log.LogWrite("Prime hit Generation\n");
      bool primeSuccess = false;
      {
        float corrRolls = (float)typeof(AttackDirector.AttackSequence).GetMethod("GetCorrectedRoll", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[2] { (object)hitInfo.toHitRolls[0], (object)team });
        bool succeeded = (double)corrRolls <= (double)toHitChance;
        bool targetDoggle = false;
        if (target != null) {
          targetDoggle = target.CheckDodge(instance.attacker, weapon, hitInfo, 0, instance.IsBreachingShot);
        }
        if (succeeded && targetDoggle) {
          hitInfo.dodgeSuccesses[0] = true;
          instance.FlagAttackContainsDodge(target.GUID);
        } else {
          hitInfo.dodgeSuccesses[0] = false;
        }
        if (succeeded && !targetDoggle) {
          hitInfo.hitLocations[0] = instance.chosenTarget.GetHitLocation(instance.attacker, instance.attackPosition, hitInfo.locationRolls[0], instance.calledShotLocation, instance.attacker.CalledShotBonusMultiplier);
          primeSuccess = true;
          primeHitLocation = hitInfo.hitLocations[0];
          instance.FlagShotHit();
        } else {
          hitInfo.hitLocations[0] = 0;
          instance.FlagShotMissed();
        }
        hitInfo.hitQualities[0] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, instance.chosenTarget, instance.meleeAttackType, instance.IsBreachingShot);
        hitInfo.hitPositions[0] = instance.chosenTarget.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref hitInfo.hitLocations[0], ref hitInfo.attackDirections[0], ref hitInfo.secondaryTargetIds[0], ref hitInfo.secondaryHitLocations[0]);
      }
      ICombatant chosenTarget = instance.chosenTarget;
      bool primeHitsSecondary = false;
      if (primeSuccess == false) {
        Log.LogWrite("prime miss\n");
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[0]) == false) {
          Log.LogWrite("but hit something stray\n");
          
          chosenTarget = instance.Director.Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[0]);
          if (chosenTarget == null) {
            Log.LogWrite("can't find combatant " + hitInfo.secondaryTargetIds[0] + "\n");
            chosenTarget = instance.chosenTarget;
          } else {
            Log.LogWrite("secondary combatant found " + chosenTarget.DisplayName + "\n");
            primeHitsSecondary = true;
            if ((hitInfo.secondaryHitLocations[0] != 0) && (hitInfo.secondaryHitLocations[0] != 65536)) {
              primeSuccess = true;
              primeHitLocation = hitInfo.secondaryHitLocations[0];
            }
          }
        }
      }
      Log.LogWrite("followers hit generator. primeSuccess:" + primeSuccess + " primeHitLocation:" + primeHitLocation + " primeHitsSecondary:" + primeHitsSecondary + "\n");
      for (int hitIndex = 1; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        Log.LogWrite(" hitIndex:" + hitIndex + "\n");
        hitInfo.dodgeSuccesses[hitIndex] = hitInfo.dodgeSuccesses[0];
        if (primeSuccess) {
          Log.LogWrite("  prime success\n");
          int HitLocation = chosenTarget.GetAdjacentHitLocation(instance.attackPosition, hitInfo.locationRolls[hitIndex], primeHitLocation, originalMultiplier, adjacentMultiplier);
          Log.LogWrite("  hitLocation:" + HitLocation + "\n");
          if (primeHitsSecondary) {
            Log.LogWrite("  hit to secondary target\n");
            string secondaryTargetId = (string)null;
            int secondaryHitLocation = 0;
            hitInfo.hitPositions[hitIndex] = instance.chosenTarget.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref HitLocation, ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
            hitInfo.hitLocations[hitIndex] = 0;
            hitInfo.secondaryHitLocations[hitIndex] = HitLocation;
            hitInfo.secondaryTargetIds[hitIndex] = chosenTarget.GUID;
          } else {
            Log.LogWrite("  hit to primary target\n");
            string secondaryTargetId = (string)null;
            int secondaryHitLocation = 0;
            hitInfo.hitPositions[hitIndex] = instance.chosenTarget.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref HitLocation, ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
            hitInfo.hitLocations[hitIndex] = HitLocation;
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.secondaryTargetIds[hitIndex] = null;
          }
          hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, chosenTarget, instance.meleeAttackType, instance.IsBreachingShot);
          instance.FlagShotHit();
        } else {
          Log.LogWrite("  prime fail\n");
          hitInfo.hitLocations[hitIndex] = 0;
          string secondaryTargetId = (string)null;
          int secondaryHitLocation = 0;
          hitInfo.hitPositions[hitIndex] = instance.chosenTarget.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref hitInfo.hitLocations[hitIndex], ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
          CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
          hitInfo.secondaryHitLocations[hitIndex] = 0;
          hitInfo.secondaryTargetIds[hitIndex] = null;
          instance.FlagShotMissed();
        }
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
      AbstractActor target = instance.chosenTarget as AbstractActor;
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
          instance.FlagAttackContainsDodge(instance.chosenTarget.GUID);
        } else {
          hitInfo.dodgeSuccesses[hitIndex] = false;
        }
        if (primeSucceeded && !primeFlag) {
          if (previousHitLocation == 0) {
            previousHitLocation = instance.chosenTarget.GetHitLocation(instance.attacker, instance.attackPosition, hitInfo.locationRolls[hitIndex], instance.calledShotLocation, instance.attacker.CalledShotBonusMultiplier);
            hitInfo.hitLocations[hitIndex] = previousHitLocation;
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} Initial streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// INITIAL HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          } else {
            hitInfo.hitLocations[hitIndex] = instance.chosenTarget.GetAdjacentHitLocation(instance.attackPosition, hitInfo.locationRolls[hitIndex], previousHitLocation, originalMultiplier, adjacentMultiplier);
            CustomAmmoCategoriesLog.Log.LogWrite("  hitLocation:" + hitInfo.hitLocations[hitIndex] + "\n");
            if (AttackDirector.attackLogger.IsLogEnabled)
              AttackDirector.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} SHOT:{2} streak hit! Location: {3}", (object)instance.id, (object)weaponIdx, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
            if (AttackDirector.hitminLogger.IsLogEnabled)
              AttackDirector.hitminLogger.Log((object)string.Format("WEAPON: {0} - SHOT: {1} Hits! ////// STREAK HIT - HEX VAL {2}", (object)weapon.Name, (object)hitIndex, (object)hitInfo.hitLocations[hitIndex]));
          }
          hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(instance.attacker, instance.attackPosition, weapon, instance.chosenTarget, instance.meleeAttackType, instance.IsBreachingShot);
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
        hitInfo.hitPositions[hitIndex] = instance.chosenTarget.GetImpactPosition(instance.attacker, instance.attackPosition, weapon, ref hitInfo.hitLocations[hitIndex], ref hitInfo.attackDirections[hitIndex], ref hitInfo.secondaryTargetIds[hitIndex], ref hitInfo.secondaryHitLocations[hitIndex]);
      }
    }
    public static float generateWeaponHitInfo(AttackDirector.AttackSequence instance, ICombatant target, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage, ref WeaponHitInfo hitInfo, bool missInCircle, bool fragHits) {
      ICombatant originaltarget = instance.chosenTarget;
      instance.chosenTarget = target;
      if (weapon.AlwaysIndirectVisuals()) { indirectFire = true; };
      CustomAmmoCategoriesLog.Log.M.TWL(0,"generateWeaponHitInfo indirect:"+indirectFire+" missInSircle:"+missInCircle);
      CustomAmmoCategoriesLog.Log.LogWrite(" altering target:" + originaltarget.GUID + "->" + target.GUID + "\n");
      float toHitChance = instance.Director.Combat.ToHit.GetToHitChance(instance.attacker, weapon, target, instance.attackPosition, target.CurrentPosition, instance.numTargets, instance.meleeAttackType, instance.isMoraleAttack);
      CustomAmmoCategoriesLog.Log.LogWrite(" filling to hit records " + target.DisplayName + " " + target.GUID + " weapon:" + weapon.defId + " shots:" + hitInfo.numberOfShots + " toHit:" + toHitChance + "\n");
      if (Mech.TEST_KNOCKDOWN)
        toHitChance = 1f;
      if (AttackDirector.hitLogger.IsLogEnabled)
        AttackDirector.hitLogger.Log((object)string.Format("======================================== HIT CHANCE: [[ {0:P2} ]]", (object)toHitChance));
      object[] args = new object[6];
      HitGeneratorType hitGenType = (fragHits ? HitGeneratorType.Cluster : weapon.HitGenerator());
      if (fragHits) { Log.LogWrite(" shells - tie to cluster\n"); }
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
      //disabling buildin spread
      if (instance.attacker.GUID == target.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(weapon.parent.GUID);
        if (terrainPos != null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" terrain attack detected to " + terrainPos.pos + ". removing buildin stray\n");
          for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.secondaryTargetIds[hitIndex] = null;
          }
        }
      }
      /*if ((weapon.SpreadRange() > CustomAmmoCategories.Epsilon) || (fragHits)) {
        Log.LogWrite(" spread range non zero " + weapon.SpreadRange() + " or frag hit generation " + fragHits + ". disabling buildin stray\n");
        for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
          hitInfo.secondaryTargetIds[hitIndex] = null;
          hitInfo.secondaryHitLocations[hitIndex] = 0;
        }
      } else {
        Log.LogWrite(" spread range zero. testing buildin stray\n");
        if (hitInfo.isHasStray()) {
          Log.LogWrite(" weapon has stray.\n");
          CustomAmmoCategories.prepareStrayHitInfo(ref hitInfo, dodgedDamage);
        } else {
          Log.LogWrite(" weapon has no stray.\n");
        }
      }*/
      if (hitInfo.numberOfShots != hitInfo.hitLocations.Length) {
        Log.LogWrite(" strange behavior. NumberOfShots: " + hitInfo.numberOfShots + " but HitLocations length:" + hitInfo.hitLocations.Length + ". Must be equal\n", true);
        hitInfo.numberOfShots = hitInfo.hitLocations.Length;
      }
      if (indirectFire && (missInCircle == false)) { missInCircle = true;  };
      if ((missInCircle)&&(instance.attacker.GUID != target.GUID)) {
        Log.LogWrite(" miss in circle\n");
        for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
          int hitLocation = hitInfo.hitLocations[hitIndex];
          if ((hitLocation == 0) || (hitLocation == 65536)) {
            Log.LogWrite("  hi:" + hitIndex + " was " + hitInfo.hitPositions[hitIndex]);
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.secondaryTargetIds[hitIndex] = null;
            hitInfo.hitPositions[hitIndex] = target.GameRep.getMissPositionRadius(instance,weapon,toHitChance,hitInfo.toHitRolls[hitIndex]);
            Log.LogWrite("  become: " + hitInfo.hitPositions[hitIndex] + "\n");
          }
        }
      }else
      if (instance.attacker.GUID == target.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(instance.attacker.GUID);
        if (terrainPos != null) {
          instance.attackCompletelyMissed(false);
          CustomAmmoCategoriesLog.Log.LogWrite(" terrain attack detected to " + terrainPos.pos + ". target position: "+target.CurrentPosition+" distance:"+Vector3.Distance(terrainPos.pos, target.CurrentPosition) +"\n");
          CustomAmmoCategoriesLog.Log.LogWrite(" recalculating hit positions and removing buildin stray\n");
          for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
            hitInfo.hitLocations[hitIndex] = 65536;
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.secondaryTargetIds[hitIndex] = null;
            Vector3 oldPos = hitInfo.hitPositions[hitIndex];
            hitInfo.hitPositions[hitIndex] = target.Combat.getMissInCircleToPosition(instance, terrainPos.pos, weapon, hitInfo.toHitRolls[hitIndex]);
            Log.LogWrite("  hi:" + hitIndex + " was " + oldPos + " become: " + hitInfo.hitPositions[hitIndex] + " distance "+Vector3.Distance(oldPos, hitInfo.hitPositions[hitIndex]) +"\n");
          }
          /*for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
            hitInfo.hitLocations[hitIndex] = 65536;
            Log.LogWrite("  hi:" + hitIndex + " was " + hitInfo.hitPositions[hitIndex]);
            hitInfo.hitPositions[hitIndex] = terrainPos.pos + (hitInfo.hitPositions[hitIndex] - target.CurrentPosition);
            Log.LogWrite("  become: " + hitInfo.hitPositions[hitIndex] + "\n");
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.secondaryTargetIds[hitIndex] = null;
          }*/
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" result(" + hitInfo.numberOfShots + "):");
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        Log.M.WL(2, "hit:" + hitIndex);
        Log.M.WL(2, "loc:" + hitInfo.hitLocations[hitIndex]);
        Log.M.WL(2, "roll:" + hitInfo.locationRolls[hitIndex]);
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          ICombatant strg = target.Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
          if (strg != null) {
            Log.M.WL(2, "sloc:" + hitInfo.secondaryHitLocations[hitIndex]);
            Log.M.WL(2, "starget:" + strg.DisplayName);
          }
        }
      }
      instance.chosenTarget = originaltarget;
      return toHitChance;
    }

    public static bool Prefix(AttackDirector.AttackSequence __instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, ref bool indirectFire, float dodgedDamage, ref WeaponHitInfo __result) {
      Log.LogWrite("Generating HitInfo " + weapon.defId + " grp:" + groupIdx + " id:" + weaponIdx + " shots:" + numberOfShots + " indirect:" + indirectFire + " " + dodgedDamage + "\n");
      if (__instance.attacker.GUID == __instance.chosenTarget.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(__instance.attacker.GUID);
        if (terrainPos != null) {
          Log.LogWrite(" Terrain attack info found. Overriding indirectFire "+indirectFire + "->");
          indirectFire = terrainPos.indirect;
          __instance.indirectFire = indirectFire;
          Log.LogWrite(" " + indirectFire + "\n");
        }
      }
      try {
        WeaponHitInfo hitInfo = new WeaponHitInfo();
        hitInfo.attackerId = __instance.attacker.GUID;
        hitInfo.targetId = __instance.chosenTarget.GUID;
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
        hitInfo.secondaryTargetIds = new string[numberOfShots];
        hitInfo.secondaryHitLocations = new int[numberOfShots];
        hitInfo.attackDirections = new AttackDirection[numberOfShots];

        CustomAmmoCategoriesLog.Log.LogWrite(" hit info created\n");
        if (AttackDirector.hitLogger.IsLogEnabled) {
          Vector3 collisionWorldPos;
          LineOfFireLevel lineOfFire = __instance.Director.Combat.LOS.GetLineOfFire(__instance.attacker, __instance.attackPosition, __instance.chosenTarget, __instance.chosenTarget.CurrentPosition, __instance.chosenTarget.CurrentRotation, out collisionWorldPos);
          float allModifiers = __instance.Director.Combat.ToHit.GetAllModifiers(__instance.attacker, weapon, __instance.chosenTarget, __instance.attackPosition + __instance.attacker.HighestLOSPosition, __instance.chosenTarget.TargetPosition, lineOfFire, __instance.isMoraleAttack);
          string modifiersDescription = __instance.Director.Combat.ToHit.GetAllModifiersDescription(__instance.attacker, weapon, __instance.chosenTarget, __instance.attackPosition + __instance.attacker.HighestLOSPosition, __instance.chosenTarget.TargetPosition, lineOfFire, __instance.isMoraleAttack);
          Pilot pilot = __instance.attacker.GetPilot();
          AttackDirector.hitLogger.Log((object)string.Format("======================================== Unit Firing: {0} | Weapon: {1} | Shots: {2}", (object)__instance.attacker.DisplayName, (object)weapon.Name, (object)numberOfShots));
          AttackDirector.hitLogger.Log((object)string.Format("======================================== Hit Info: GROUP {0} | ID {1}", (object)groupIdx, (object)weaponIdx));
          AttackDirector.hitLogger.Log((object)string.Format("======================================== MODIFIERS: {0}... FINAL: [[ {1} ]] ", (object)modifiersDescription, (object)allModifiers));
          if (pilot != null)
            AttackDirector.hitLogger.Log((object)__instance.Director.Combat.ToHit.GetBaseToHitChanceDesc(__instance.attacker));
          else
            AttackDirector.hitLogger.Log((object)string.Format("======================================== Gunnery Check: NO PILOT"));
        }
        float toHitChance = generateWeaponHitInfo(__instance, __instance.chosenTarget, weapon, groupIdx, weaponIdx, numberOfShots, indirectFire, dodgedDamage, ref hitInfo, false, false);
        weapon.DecrementAmmo(ref hitInfo, __instance.attacker.GUID == __instance.chosenTarget.GUID);
        weapon.FlushAmmoCount(hitInfo.stackItemUID);
        if(numberOfShots > hitInfo.numberOfShots) {
          CustomAmmoCategories.ReturnNoFireHeat(weapon, hitInfo.stackItemUID, numberOfShots, hitInfo.numberOfShots);
        }
        //int realAmmoUsedCount = hitInfo.numberOfShots;
        /*if (weapon.isStreak()) {
          if (__instance.attacker.GUID != __instance.chosenTarget.GUID) {
            CustomAmmoCategoriesLog.Log.LogWrite("Streak detected. Clearing missed.\n");
            WeaponHitInfo streakHitInfo = CustomAmmoCategories.getSuccessOnly(hitInfo);
            CustomAmmoCategories.ReturnNoFireHeat(weapon, hitInfo.stackItemUID, hitInfo.numberOfShots, streakHitInfo.numberOfShots);
            hitInfo = streakHitInfo;
            realAmmoUsedCount = streakHitInfo.numberOfShots;
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("Streak detected. But terrain attack. No misses clearence performed.\n");
          }
        }*/
        //weapon.RealDecrementAmmo(hitInfo.stackItemUID, weapon.HitsToShots(realAmmoUsedCount));
        hitInfo.initGenericAdvInfo(toHitChance, __instance, __instance.Director.Combat);
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
