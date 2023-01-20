/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using BattleTech.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Reflection;
using Harmony;
using CustAmmoCategories;
using FluffyUnderware.Curvy;
using Localize;
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using CustAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public class AMSRecord {
    public Weapon weapon;
    public int ShootsRemains;
    public int ShootsCount;
    public float AMSHitChance;
    public float Range;
    public bool isLaser;
    public string weaponEffectId;
    public HashSet<int> weaponCountred;
    public bool isAAMS;
    //public bool amsShooted;
    public AMSRecord(Weapon weapon, ExtWeaponDef extWeapon, bool AAMS) {
      this.weapon = weapon;
      AMSHitChance = weapon.AMSHitChance();
      ShootsRemains = 0;
      ShootsCount = 0;
      if (weapon.CanFire == false) {
        ShootsRemains = 0;
      } else {
        if (weapon.AMSActivationsPerTurn() <= 0) {
          if (weapon.AMSShootsEveryAttack() == false) {
            ShootsRemains = weapon.ShotsWhenFired - weapon.AMSShootsCount();
          } else {
            ShootsRemains = weapon.ShotsWhenFired;
          }
        } else  if (weapon.AMSActivationsPerTurn() > weapon.AMSActivationsCount()) {
          ShootsRemains = weapon.ShotsWhenFired;
        } else {
          ShootsRemains = 0;
        }        
      }
      if (ShootsRemains < 0) { ShootsRemains = 0; };
      Range = weapon.MaxRange;
      weaponCountred = new HashSet<int>();
      LaserEffect laserEffect = CustomAmmoCategories.getWeaponEffect(weapon) as LaserEffect;
      isLaser = laserEffect != null;
      weaponEffectId = CustomAmmoCategories.getWeaponEffectId(weapon);
      isAAMS = AAMS;
      //amsShooted = false;
    }
  }
  public class AMSShoot {
    public int shootIdx;
    public float t;
    public Weapon AMS;
    public AMSShoot(int shootIdx, float relPos, Weapon ams) {
      this.shootIdx = shootIdx;
      this.t = relPos;
      AMS = ams;
    }
  }
  /*public class CachedMissileCurve {
    public Vector3 startPos;
    public Vector3 endPos;
    public Vector3[] spline;
    public Weapon InterceptedAMS;
    public Weapon weapon;
    public float missileProjectileSpeed;
    public GameObject splineObject;
    public CurvySpline UnitySpline;
    public bool Intercepted;
    public bool isIndirect;
    public float InterceptedT;
    public List<AMSShoot> AMSShoots;
    public float AMSHitChance;
    public ICombatant target;
    public MissileLauncherEffect launcherEffect;
    public CombatGameState Combat;
    public int hitLocation;
    public int hitIndex;
    public int weaponIndex;
    public int groupIndex;
    public int AMSShootIndex;
    public bool AMSImunne;
    public void regenerateMissilepath(bool isDirect,bool isIndirect) {
      if (isDirect) {
        this.spline = CustomAmmoCategories.GenerateDirectMissilePath(
          this.launcherEffect.missileCurveStrength,
          this.launcherEffect.missileCurveFrequency,
          this.launcherEffect.isSRM,
          this.hitLocation, this.startPos, this.endPos, this.Combat);
      } else {
        if (isIndirect) {
          this.spline = CustomAmmoCategories.GenerateIndirectMissilePath(
            this.launcherEffect.missileCurveStrength,
            this.launcherEffect.missileCurveFrequency,
            this.launcherEffect.isSRM,
            this.hitLocation, this.startPos, this.endPos, this.Combat);
        } else {
          this.spline = CustomAmmoCategories.GenerateMissilePath(
            this.launcherEffect.missileCurveStrength,
            this.launcherEffect.missileCurveFrequency,
            this.launcherEffect.isSRM,
            this.hitLocation, this.startPos, this.endPos, this.Combat);
        }
      }
      this.UnitySpline.Interpolation = CurvyInterpolation.Bezier;
      this.UnitySpline.Clear();
      this.UnitySpline.Closed = false;
      this.UnitySpline.Add(this.spline);
      this.UnitySpline.Refresh();
    }
    public CachedMissileCurve() {
      startPos = new Vector3();
      endPos = new Vector3();
      spline = new Vector3[0] { };
      InterceptedAMS = null;
      weapon = null;
      splineObject = null;
      UnitySpline = null;
      missileProjectileSpeed = 0.0f;
      Intercepted = false;
      target = null;
      InterceptedT = 2.0f;
      isIndirect = false;
      launcherEffect = null;
      Combat = null;
      hitLocation = 0;
      hitIndex = 0;
      weaponIndex = 0;
      AMSShootIndex = 0;
      groupIndex = 0;
      AMSHitChance = 0f;
      AMSShoots = new List<AMSShoot>();
      AMSImunne = false;
    }
    public float getAMSShootT() {
      if ((AMSShootIndex >= 0) && (AMSShootIndex < AMSShoots.Count)) {
        return 0.0f - (2.0f + AMSShoots[AMSShootIndex].t);
      }
      return Intercepted ? -1.0f : 0.0f;
    }
    public AMSShoot getAMSShoot() {
      if ((AMSShootIndex >= 0) && (AMSShootIndex < AMSShoots.Count)) {
        return AMSShoots[AMSShootIndex];
      }
      return null;
    }
    public void nextAMSShoot() {
      ++AMSShootIndex;
    }
    public void clearSpline() {
      GameObject.Destroy(UnitySpline);
      GameObject.Destroy(splineObject);
      UnitySpline = null;
      splineObject = null;
    }
  }*/
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, Weapon> amsWeapons = new Dictionary<string, Weapon>();
    //public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, CachedMissileCurve>>>> MissileCurveCache = null;
    public static string AMSShootsCountStatName = "CAC-AMShootsCount";
    public static string AMSActivationsCountStatName = "CAC-AMSActivationsCount";
    public static string AMSJammingAttemptStatName = "CAC-AMSJammingAttempt";
    public static bool Unguided(this Weapon weapon) {
      if (weapon.ammo().Unguided == TripleBoolean.True) { return true; }
      if (weapon.mode().Unguided == TripleBoolean.True) { return true; }
      if (weapon.exDef().Unguided == TripleBoolean.True) { return true; }
      return false;
    }
    public static float AMSHitChance(this Weapon weapon) {
      return weapon.exDef().AMSHitChance + weapon.ammo().AMSHitChance + weapon.mode().AMSHitChance;
    }
    public static int AMSShootsCount(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AMSShootsCountStatName);
      if (stat == null) { return 0; }
      return stat.Value<int>();
    }
    public static int AMSActivationsCount(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AMSActivationsCountStatName);
      if (stat == null) { return 0; }
      return stat.Value<int>();
    }
    public static bool AMSJammingAttempt(Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AMSJammingAttemptStatName);
      if (stat == null) { return false; }
      return stat.Value<bool>();
    }
    public static void AMSShootsCount(this Weapon weapon, int shoots) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AMSShootsCountStatName) == false) {
        weapon.StatCollection.AddStatistic<int>(CustomAmmoCategories.AMSShootsCountStatName, shoots);
      } else {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.AMSShootsCountStatName, shoots);
      }
    }
    public static void AMSActivationsCount(this Weapon weapon, int activations) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AMSActivationsCountStatName) == false) {
        weapon.StatCollection.AddStatistic<int>(CustomAmmoCategories.AMSActivationsCountStatName, activations);
      } else {
        weapon.StatCollection.Set<int>(CustomAmmoCategories.AMSActivationsCountStatName, activations);
      }
    }
    public static void AMSJammingAttempt(this Weapon weapon, bool isShooted) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AMSJammingAttemptStatName) == false) {
        weapon.StatCollection.AddStatistic<bool>(CustomAmmoCategories.AMSShootsCountStatName, isShooted);
      } else {
        weapon.StatCollection.Set<bool>(CustomAmmoCategories.AMSShootsCountStatName, isShooted);
      }
    }
    /*public static float calculateInterceptCorrection(WeaponEffect amsEffect, float curPath, float pathLenth, float distance, float missileProjectileSpeed) {
      BallisticEffect ballisticEffect = amsEffect as BallisticEffect;
      if (ballisticEffect == null) { return curPath / pathLenth; }
      float amsProjectileSpeed = amsEffect.projectileSpeed;
      float timeToIntercept = distance / amsProjectileSpeed;
      float missileDistanceToIntecept = missileProjectileSpeed * timeToIntercept;
      if (curPath <= missileDistanceToIntecept) { return 0.1f; };
      return (curPath - missileDistanceToIntecept) / pathLenth;
    }*/
    public static string getGUIDFromHitInfo(WeaponHitInfo hitInfo) {
      return hitInfo.attackerId + "_" + hitInfo.targetId + "_" + hitInfo.attackWeaponIndex;
    }
    public static string getWeaponEffectId(this Weapon weapon) {
      string result = "";
      if (string.IsNullOrEmpty(result)) {
        result = weapon.mode().WeaponEffectID;
      }
      if (string.IsNullOrEmpty(result)) {
        result = weapon.ammo().WeaponEffectID;
      }
      if (string.IsNullOrEmpty(result)) {
        result = weapon.weaponDef.WeaponEffectID;
      }
      return result;
    }
    public static WeaponEffect getWeaponEffect(this Weapon weapon) {
      Log.LogWrite(weapon.defId+".getWeaponEffect\n");
      if ((UnityEngine.Object)weapon.weaponRep == (UnityEngine.Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Weapon "+weapon.defId+" "+weapon.UIName+" on "+weapon.parent.DisplayName+":"+weapon.parent.GUID+" has no representation! It no visuals will be played\n",true);
        return null;
      };
      Statistic wGUIDstat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (wGUIDstat == null) { return weapon.weaponRep.WeaponEffect; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon ammoId is set\n");
      string wGUID = wGUIDstat.Value<string>();
      WeaponMode weaponMode = weapon.mode();
      string weaponEffectId = weaponMode.WeaponEffectID;
      Log.LogWrite(" weaponMode.WeaponEffectID = "+ weaponMode.WeaponEffectID + "\n");
      WeaponEffect currentEffect = (WeaponEffect)null;
      if (string.IsNullOrEmpty(weaponEffectId)) {
        Log.LogWrite(" null or empty\n");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.ammo().WeaponEffectID;
        Log.LogWrite(" ammo.WeaponEffectID = "+ weaponEffectId + "\n");
        if (string.IsNullOrEmpty(weaponEffectId)) {
          weaponEffectId = weapon.weaponDef.WeaponEffectID;
        }
      }
      if (weaponEffectId == weapon.weaponDef.WeaponEffectID) {
        Log.LogWrite(" same as per weapon\n");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.weaponDef.WeaponEffectID;
      };
      if (currentEffect == (WeaponEffect)null) {
        Log.LogWrite(" getting weapon effect "+wGUID+"."+weaponEffectId+"\n");
        currentEffect = CustomAmmoCategories.getWeaponEffect(wGUID, weaponEffectId);
      }
      if (currentEffect == (WeaponEffect)null) {
        currentEffect = weapon.weaponRep.WeaponEffect;
      }
      return currentEffect;
    }
    public static string getWeaponEffectID(this Weapon weapon) {
      Log.LogWrite(weapon.defId + ".getWeaponEffectID\n");
      if ((UnityEngine.Object)weapon.weaponRep == (UnityEngine.Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Weapon " + weapon.defId + " " + weapon.UIName + " on " + weapon.parent.DisplayName + ":" + weapon.parent.GUID + " has no representation! It no visuals will be played\n", true);
        return string.Empty;
      };
      Statistic wGUIDstat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (wGUIDstat == null) { return weapon.weaponDef.WeaponEffectID; }
      CustomAmmoCategoriesLog.Log.LogWrite("  weapon GUID is set\n");
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      WeaponMode weaponMode = weapon.mode();
      string weaponEffectId = weaponMode.WeaponEffectID;
      Log.LogWrite(" weaponMode.WeaponEffectID = " + weaponMode.WeaponEffectID + "\n");
      WeaponEffect currentEffect = (WeaponEffect)null;
      if (string.IsNullOrEmpty(weaponEffectId)) {
        Log.LogWrite(" null or empty\n");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.mode().WeaponEffectID;
        Log.LogWrite(" ammo.WeaponEffectID = " + weaponEffectId + "\n");
        if (string.IsNullOrEmpty(weaponEffectId)) {
          weaponEffectId = weapon.weaponDef.WeaponEffectID;
        }
      }
      if (weaponEffectId == weapon.weaponDef.WeaponEffectID) {
        Log.LogWrite(" same as per weapon\n");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.weaponDef.WeaponEffectID;
        return weapon.weaponDef.WeaponEffectID;
      };
      if (currentEffect == (WeaponEffect)null) {
        Log.LogWrite(" getting weapon effect " + wGUID + "." + weaponEffectId + "\n");
        currentEffect = CustomAmmoCategories.getWeaponEffect(wGUID, weaponEffectId);
      }
      if (currentEffect == (WeaponEffect)null) {
        currentEffect = weapon.weaponRep.WeaponEffect;
        return weapon.weaponDef.WeaponEffectID;
      } 
      return weaponEffectId;
    }
    public static Vector3[] GenerateMissilePath(float missileCurveStrength, int missileCurveFrequency, bool isSRM, int hitLocation, Vector3 startPos, Vector3 endPos, CombatGameState Combat) {
      float max = missileCurveStrength;
      int index1 = Random.Range(2, missileCurveFrequency);
      if ((double)max < 0.1 || missileCurveFrequency < 1) {
        max = 0.0f;
        index1 = 2;
      }
      if (isSRM && (hitLocation == 0 || hitLocation == 65536)) {
        max += Random.Range(5f, 12f);
        index1 += Random.Range(3, 9);
      }
      float num1 = 1f / (float)index1;
      Vector3 up = Vector3.up;
      Vector3 axis = endPos - startPos;
      Vector3[] vector3Array = new Vector3[index1 + 1];
      vector3Array[0] = startPos;
      float num2 = !isSRM ? 25f : 0.0f;
      for (int index2 = 1; index2 < index1; ++index2) {
        Vector3 vector3_1 = Vector3.Lerp(startPos, endPos, num1 * (float)index2);
        Vector3 vector3_2 = Vector3.up * Random.Range(-max, max);
        vector3_2 = Quaternion.AngleAxis((float)Random.Range(0, 360), axis) * vector3_2;
        if ((double)vector3_2.y < 0.0)
          vector3_2.y = 0.0f;
        Vector3 worldPos = vector3_1 + vector3_2;
        float lerpedHeightAt = Combat.MapMetaData.GetLerpedHeightAt(worldPos);
        if ((double)worldPos.y < (double)lerpedHeightAt)
          worldPos.y = lerpedHeightAt + num2;
        vector3Array[index2] = worldPos;
      }
      vector3Array[index1] = endPos;
      return vector3Array;
    }
    public static Vector3[] GenerateDirectMissilePath(float missileCurveStrength, int missileCurveFrequency, bool isSRM, int hitLocation, Vector3 startPos, Vector3 endPos, CombatGameState Combat) {
      Vector3[] vector3Array = new Vector3[2];
      vector3Array[0] = startPos;
      vector3Array[1] = endPos;
      return vector3Array;
    }
    public static Vector3[] GenerateIndirectMissilePath(float missileCurveStrength, int missileCurveFrequency, bool isSRM, int hitLocation, Vector3 startPos, Vector3 endPos, CombatGameState Combat) {
      float max = missileCurveStrength;
      int num1 = Random.Range(2, missileCurveFrequency);
      if ((double)max < 0.1 || missileCurveFrequency < 1) {
        max = 0.0f;
        num1 = 2;
      }
      Vector3 up = Vector3.up;
      Vector3 axis = endPos - startPos;
      int length = 9;
      if (num1 > length)
        length = num1;
      float num2 = (float)((double)endPos.y - (double)startPos.y + 15.0);
      Vector3[] vector3Array = new Vector3[length];
      Vector3 vector3_1 = endPos - startPos;
      float num3 = (float)(((double)Mathf.Max(endPos.y, startPos.y) - (double)Mathf.Min(endPos.y, startPos.y)) * 0.5) + num2;
      vector3Array[0] = startPos;
      for (int index = 1; index < length - 1; ++index) {
        float num4 = (float)index / (float)length;
        float num5 = (float)(1.0 - (double)Mathf.Abs(num4 - 0.5f) / 0.5);
        float num6 = (float)(1.0 - (1.0 - (double)num5) * (1.0 - (double)num5));
        Vector3 worldPos = vector3_1 * num4;
        float lerpedHeightAt = Combat.MapMetaData.GetLerpedHeightAt(worldPos);
        if ((double)num3 < (double)lerpedHeightAt)
          num3 = lerpedHeightAt + 5f;
        worldPos.y += num6 * num3;
        worldPos += startPos;
        Vector3 vector3_2 = Vector3.up * Random.Range(-max, max);
        vector3_2 = Quaternion.AngleAxis((float)Random.Range(0, 360), axis) * vector3_2;
        if ((double)vector3_2.y < 0.0)
          vector3_2.y = 0.0f;
        worldPos += vector3_2;
        if ((double)worldPos.y < (double)lerpedHeightAt)
          worldPos.y = lerpedHeightAt + 5f;
        vector3Array[index] = worldPos;
      }
      vector3Array[length - 1] = endPos;
      return vector3Array;
    }
    public static bool AMSImmune(this Weapon weapon) {
      return getWeaponAMSImmune(weapon);
    }
    public static float AMSDamage(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return (mode.AMSDamage + extAmmo.AMSDamage + weapon.GetStatisticFloat("AMSDamage")) * (weapon.GetStatisticMod("AMSDamage")); //+ extWeapon.AMSDamage;
    }
    public static float MissileHealth(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return (mode.MissileHealth + extAmmo.MissileHealth + weapon.GetStatisticFloat("MissileHealth")) * (weapon.GetStatisticMod("MissileHealth")); //+ extWeapon.MissileHealth;
    }
    public static bool getWeaponAMSImmune(Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.AMSImmune != TripleBoolean.NotSet) { return mode.AMSImmune == TripleBoolean.True; }
      ExtAmmunitionDef extAmmo = weapon.ammo();
      if (extAmmo.AMSImmune != TripleBoolean.NotSet) { return extAmmo.AMSImmune == TripleBoolean.True; }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return extWeapon.AMSImmune == TripleBoolean.True;
    }
    /*public static void generateMissileCacheCurve(Weapon weapon, WeaponHitInfo hitInfo, int hitIndex, bool indirectFire, bool AMSImmune, bool DirectStrike) {
      if (CustomAmmoCategories.MissileCurveCache == null) {
        CustomAmmoCategories.MissileCurveCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, CachedMissileCurve>>>>();
      }
      if (CustomAmmoCategories.MissileCurveCache.ContainsKey(hitInfo.attackSequenceId) == true) {
        if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == true) {
          if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == true) {
            if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == true) {
              CustomAmmoCategoriesLog.Log.LogWrite("WARNING! i have " + hitInfo.attackSequenceId + " " + hitInfo.attackGroupIndex + " " + hitInfo.attackWeaponIndex + " " + hitIndex + " in pregenerated curves. That is strange", true);
              return;
            }
          }
        }
      }
      MissileLauncherEffect currentEffect = CustomAmmoCategories.getWeaponEffect(weapon) as MissileLauncherEffect;
      if (currentEffect == null) { return; }
      Log.LogWrite("effect is not null\n");
      int numberOfEmitters = (int)typeof(WeaponEffect).GetField("numberOfEmitters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currentEffect);
      int emitterIndex = hitIndex % numberOfEmitters;
      Log.LogWrite("number of emmiters getted\n");
      Transform startingTransform = weapon.weaponRep.vfxTransforms[emitterIndex];
      CachedMissileCurve cachedCurve = new CachedMissileCurve();
      cachedCurve.weapon = weapon;
      Log.LogWrite("transform getted.\n");
      CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(currentEffect);
      cachedCurve.startPos = startingTransform.position;
      SpreadHitRecord spreadCacheRecord = CustomAmmoCategories.getSpreadCache(hitInfo, hitIndex);
      string targetGUID = hitInfo.targetId;
      if (spreadCacheRecord != null) { targetGUID = spreadCacheRecord.targetGUID; };
      ICombatant combatantByGuid = null;
      if (targetGUID == hitInfo.attackerId) {
        CustomAmmoCategoriesLog.Log.LogWrite("generateMissileCacheCurve terrain hit detected. No need to recalculate hit position\n");
      } else {
        combatantByGuid = Combat.FindCombatantByGUID(targetGUID);
      }
      Log.LogWrite("408\n");
      cachedCurve.target = combatantByGuid;
      int hitLocation = hitInfo.hitLocations[hitIndex];
      if (combatantByGuid != null) {
        Log.LogWrite("combatantByGuid is not null. Getting impact position\n");
        if (hitInfo.attackDirections == null) {
          Log.LogWrite("WARNING!!attackDirections is null this never should happend. Please report to author!\n",true);
        }
        string secondaryTargetId = (string)null;
        int secondaryHitLocation = 0;
        cachedCurve.endPos = combatantByGuid.GetImpactPosition(weapon.parent, cachedCurve.startPos, weapon, ref hitLocation, ref hitInfo.attackDirections[hitIndex], ref secondaryTargetId, ref secondaryHitLocation);
        hitInfo.hitPositions[hitIndex] = cachedCurve.endPos;
      } else {
        Log.LogWrite("combatantByGuid is null. No need impact position change\n");
        cachedCurve.endPos = hitInfo.hitPositions[hitIndex];
      }
      Log.LogWrite("impact position getted\n");
      if (spreadCacheRecord != null) {
        Log.LogWrite("found spread hit record\n");
        spreadCacheRecord.hitInfo.hitPositions[spreadCacheRecord.internalIndex] = cachedCurve.endPos;
        Log.LogWrite("Altering spread position. target " + combatantByGuid.DisplayName + " " + combatantByGuid.GUID + " " + spreadCacheRecord.hitInfo.hitPositions[spreadCacheRecord.internalIndex] + "\n");
      }
      Log.LogWrite("generating missile curvy\n");
      cachedCurve.missileProjectileSpeed = currentEffect.projectileSpeed;
      float distance = Vector3.Distance(cachedCurve.startPos, cachedCurve.endPos);
      if (cachedCurve.missileProjectileSpeed < CustomAmmoCategories.Epsilon) {
        cachedCurve.missileProjectileSpeed = distance / 1.5f;
      }
      if ((cachedCurve.missileProjectileSpeed * 4.0f) < distance) {
        cachedCurve.missileProjectileSpeed = distance / 4.0f;
      }
      float max = cachedCurve.missileProjectileSpeed * 0.1f;
      cachedCurve.missileProjectileSpeed += Random.Range(-max, max);
      if (weapon.isImprovedBallistic()) {
        Log.LogWrite(" altering missile speed " + cachedCurve.missileProjectileSpeed + " -> ");
        cachedCurve.missileProjectileSpeed *= weapon.ProjectileSpeedMultiplier();
        Log.LogWrite(" " + cachedCurve.missileProjectileSpeed + "\n");
      }
      //if (DirectStrike) { cachedCurve.missileProjectileSpeed *= 2.0f; }
      if (CustomAmmoCategories.getWeaponAlwaysIndirectVisuals(weapon)) { indirectFire = true; };
      cachedCurve.isIndirect = indirectFire;
      cachedCurve.launcherEffect = currentEffect;
      cachedCurve.Combat = Combat;
      cachedCurve.hitLocation = hitLocation;
      cachedCurve.hitIndex = hitIndex;
      cachedCurve.weaponIndex = hitInfo.attackWeaponIndex;
      cachedCurve.groupIndex = hitInfo.attackGroupIndex;
      cachedCurve.AMSHitChance = CustomAmmoCategories.getWeaponAMSHitChance(weapon);
      cachedCurve.AMSImunne = AMSImmune;
      Log.LogWrite("generating spline\n");
      if (DirectStrike) {
        Log.LogWrite("direct\n");
        cachedCurve.spline = CustomAmmoCategories.GenerateDirectMissilePath(
          currentEffect.missileCurveStrength, currentEffect.missileCurveFrequency, currentEffect.isSRM, hitLocation, cachedCurve.startPos, cachedCurve.endPos, Combat);
      } else {
        if (indirectFire) {
          Log.LogWrite("indirect\n");
          cachedCurve.spline = CustomAmmoCategories.GenerateIndirectMissilePath(
            currentEffect.missileCurveStrength, currentEffect.missileCurveFrequency, currentEffect.isSRM, hitLocation, cachedCurve.startPos, cachedCurve.endPos, Combat);
        } else {
          Log.LogWrite("normal\n");
          cachedCurve.spline = CustomAmmoCategories.GenerateMissilePath(
            currentEffect.missileCurveStrength, currentEffect.missileCurveFrequency, currentEffect.isSRM, hitLocation, cachedCurve.startPos, cachedCurve.endPos, Combat);
        }
      }
      Log.LogWrite("generatng game object\n");
      cachedCurve.splineObject = new GameObject();
      cachedCurve.UnitySpline = cachedCurve.splineObject.AddComponent<CurvySpline>();
      cachedCurve.UnitySpline.Interpolation = CurvyInterpolation.Bezier;
      cachedCurve.UnitySpline.Clear();
      cachedCurve.UnitySpline.Closed = false;
      cachedCurve.UnitySpline.Add(cachedCurve.spline);
      cachedCurve.UnitySpline.Refresh();
      Log.LogWrite("saving info\n");
      if (CustomAmmoCategories.MissileCurveCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        CustomAmmoCategories.MissileCurveCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, CachedMissileCurve>>>());
      }
      if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, CachedMissileCurve>>());
      };
      if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, CachedMissileCurve>());
      };
      CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, cachedCurve);
    }*/

    public static void genreateAMSInterceptionInfo(AttackDirector.AttackSequence instance) {
      Log.M.TWL(0, "genreateAMSInterceptionInfo");
      /*if (CustomAmmoCategories.MissileCurveCache == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Curve cache is not inited. No missiles since battle start.\n");
        return;
      };
      if (CustomAmmoCategories.MissileCurveCache.ContainsKey(instance.id) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("Curve cache is not contain " + instance.id + " attack sequence id. No missiles in sequence.\n");
        return;
      };*/
      List<AdvWeaponHitInfoRec> missiles = instance.Interceptables();
      Dictionary<Weapon, AMSJammInfoMessage> amsJammingInfo = new Dictionary<Weapon, AMSJammInfoMessage>();
      Log.LogWrite("missiles:" + missiles.Count + "\n");
      List<AMSRecord> ams = new List<AMSRecord>();
      HashSet<string> targetsGUIDs = new HashSet<string>();
      List<ICombatant> targetsList = new List<ICombatant>();
      foreach (AdvWeaponHitInfoRec missile in missiles) {
        if (missile.target == null) { continue; };
        if (targetsGUIDs.Contains(missile.target.GUID) == false) {
          targetsList.Add(missile.target);
          targetsGUIDs.Add(missile.target.GUID);
        }
      }
      if (targetsGUIDs.Contains(instance.chosenTarget.GUID) == false) {
        targetsList.Add(instance.chosenTarget);
        targetsGUIDs.Add(instance.chosenTarget.GUID);
      }
      Log.LogWrite("AMS list:\n");
      foreach (ICombatant target in targetsList) {
        Log.LogWrite(" actor:" + target.DisplayName + ":" + target.GUID + "\n");
        AbstractActor targetActor = target as AbstractActor;
        if (targetActor == null) { continue; };
        if (targetActor.GUID == instance.attacker.GUID) {
          CustomAmmoCategoriesLog.Log.LogWrite("i will not fire my own missiles.\n");
          continue;
        }
        if (targetActor.IsShutDown) {
          Log.LogWrite("  shutdown\n");
          continue;
        };
        if (targetActor.IsDead) {
          Log.LogWrite("  dead\n");
          continue;
        };
        foreach (Weapon weapon in targetActor.Weapons) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
          Log.M.WL(2, weapon.defId + " mode:" + weapon.mode().Id + " ammo:" + weapon.ammo().Id + " AMS:" + weapon.isAMS() + " AAMS:" + weapon.isAAMS() + " have weaponRep:" + (weapon.weaponRep == null ? "false" : "true"));
          if ((weapon.isAMS()) && (weapon.isAAMS() == false) && (weapon.weaponRep != null)) {
            Log.LogWrite("  AMS " + weapon.UIName + "\n");
            ams.Add(new AMSRecord(weapon, extWeapon, false));
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Searching advanced AMS in battle.\n");
      CombatGameState combat = instance.attacker.Combat;
      List<AbstractActor> atackerEnemies = combat.GetAllEnemiesOf(instance.attacker);
      Log.LogWrite("AAMS list:\n");
      foreach (AbstractActor enemy in atackerEnemies) {
        Log.LogWrite(" actor:" + enemy.DisplayName + ":" + enemy.GUID + "\n");
        if (enemy.IsShutDown) {
          Log.LogWrite("  shutdown\n");
          continue;
        };
        if (enemy.IsDead) {
          Log.LogWrite("  dead\n");
          continue;
        };
        foreach (Weapon weapon in enemy.Weapons) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
          Log.M.WL(2, weapon.defId + " mode:" + weapon.mode().Id + " ammo:" + weapon.ammo().Id + " AMS:" + weapon.isAMS() + " AAMS:" + weapon.isAAMS() + " have weaponRep:" + (weapon.weaponRep == null ? "false" : "true"));
          if (weapon.isAAMS() && (weapon.weaponRep != null)) {
            Log.LogWrite("  AAMS " + weapon.UIName + "\n");
            ams.Add(new AMSRecord(weapon, extWeapon, true));
          }
        }
      }
      if (ams.Count <= 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("No one AMS found.\n");
        return;
      };
      bool NoActiveAMS = true;
      Log.M.TWL(0, "FIELD AMS LIST. ROUND:" + combat.TurnDirector.CurrentRound + " PHASE:" + combat.TurnDirector.CurrentPhase + " seqId:" + instance.id);
      if (missiles.Count > 0) {
        foreach (AMSRecord amsrec in ams) {
          Log.M.WL(2, $"{amsrec.weapon.parent.DisplayName}.{amsrec.weapon.defId} ShootsRemains:{amsrec.ShootsRemains} maxShoots:{amsrec.weapon.ShotsWhenFired} shotsPerformed:{amsrec.weapon.AMSShootsCount()} maxActiv:{amsrec.weapon.AMSActivationsPerTurn()} curActiv:{amsrec.weapon.AMSActivationsCount()} CanFire:{amsrec.weapon.CanFire} AMSShootsEveryAttack:{amsrec.weapon.AMSShootsEveryAttack()} mode:{amsrec.weapon.mode().Id} ammo:{amsrec.weapon.ammo().Id} AMS:{amsrec.weapon.isAMS()} AAMS:{amsrec.weapon.isAAMS()} have weaponRep:{(amsrec.weapon.weaponRep == null ? "false" : "true")}");
          if (amsrec.weapon.CanFire == false) {
            if (CustomAmmoCategories.Settings.AMSCantFireFloatie) {
              combat.MessageCenter.PublishMessage(new FloatieMessage(amsrec.weapon.parent.GUID, amsrec.weapon.parent.GUID, new Text("AMS: {0} CAN'T FIRE {1}", amsrec.weapon.UIName, amsrec.weapon.CantFireReason()), FloatieMessage.MessageNature.Debuff));
            }
          }
        }
      }
      foreach (AMSRecord amsrec in ams) {
        if (amsrec.ShootsRemains > 0) { NoActiveAMS = false; break; };
      }
      if (NoActiveAMS) {
        CustomAmmoCategoriesLog.Log.LogWrite("No one active AMS found.\n");
        return;
      };
      //foreach (AbstractActor trgAlly in combat.GetAllAlliesOf)
      float longestPath = 0.0f;
      HashSet<AdvWeaponHitInfoRec> trackMissiles = new HashSet<AdvWeaponHitInfoRec>();
      foreach (AdvWeaponHitInfoRec missile in missiles) {
        if (missile.interceptInfo.AMSImunne) {
          CustomAmmoCategoriesLog.Log.LogWrite("  missile immune to AMS " + missile.parent.weapon.defId + " " + missile.hitIndex + "\n");
          continue;
        }
        if (missile.trajectorySpline.Length > longestPath) { longestPath = missile.trajectorySpline.Length; }
        trackMissiles.Add(missile);
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Longest path:" + longestPath + "\n");
      for (float path = 0.0f; path < longestPath; path += 10.0f) {
        //CustomAmmoCategoriesLog.Log.LogWrite(" path:" + path + "\n");
        bool missilesInCharge = false;
        foreach (AdvWeaponHitInfoRec missile in missiles) {
          if (trackMissiles.Contains(missile) == false) { continue; }
          if (missile.trajectorySpline.Length <= path) {
            CustomAmmoCategoriesLog.Log.LogWrite("  missile done " + missile.parent.weapon.defId + " " + missile.hitIndex + "\n");
            trackMissiles.Remove(missile);
          }
          if (missile.interceptInfo.Intercepted == true) {
            CustomAmmoCategoriesLog.Log.LogWrite("  missile intercepted " + missile.parent.weapon.defId + " " + missile.hitIndex + "\n");
            trackMissiles.Remove(missile);
          }

        }
        foreach (AdvWeaponHitInfoRec missile in trackMissiles) {
          missilesInCharge = true;
          Vector3 pos = missile.trajectorySpline.InterpolateByDistance(path);
          bool AMSInCharge = false;
          CustomAmmoCategoriesLog.Log.M.WL(2, "missile interception " + missile.parent.weapon.defId + " hitIndex:" + missile.hitIndex + " weaponId:" + missile.parent.weaponIdx + " hp:" + missile.interceptInfo.missileHealth);
          foreach (AMSRecord amsrec in ams) {
            if (amsrec.ShootsRemains <= 0) { continue; };
            if ((amsrec.isAAMS == false)) {
              if (amsrec.weapon.parent != null) {
                if (missile.target != null) {
                  if (amsrec.weapon.parent.GUID != missile.target.GUID) { continue; };
                }
              }
            }
            AMSInCharge = true;
            float distance = Vector3.Distance(pos, amsrec.weapon.parent.CurrentPosition);
            CustomAmmoCategoriesLog.Log.LogWrite(distance + "\n");
            if (distance > amsrec.Range) { continue; };
            CustomAmmoCategoriesLog.Log.LogWrite("   AMS ready to fire " + amsrec.weapon.defId + " dmg:" + amsrec.weapon.AMSDamage() + " shoot remains:" + amsrec.ShootsRemains + "\n");
            if (amsJammingInfo.ContainsKey(amsrec.weapon) == false) {
              amsJammingInfo.Add(amsrec.weapon, new AMSJammInfoMessage(amsrec.weapon));
            }
            if (amsrec.weapon.DecrementOneAmmo() == false) {
              CustomAmmoCategoriesLog.Log.LogWrite("   AMS ammo depleted " + amsrec.weapon.tCurrentAmmo() + "\n");
              amsrec.ShootsRemains = 0;
              continue;
            }
            --amsrec.ShootsRemains;
            ++amsrec.ShootsCount;
            float interceptRoll = Random.Range(0f, 1f);
            float effectiveHitChance = amsrec.AMSHitChance + missile.interceptInfo.AMSHitChance;
            CustomAmmoCategoriesLog.Log.LogWrite("   roll:" + interceptRoll + " chance:" + effectiveHitChance + "\n");
            AMSShoot amsShoot = null;
            int AMSShootIdx = amsrec.weapon.AMS().AddHitPosition(pos);
            if (interceptRoll < effectiveHitChance) {
              missile.interceptInfo.missileHealth -= amsrec.weapon.AMSDamage();
              CustomAmmoCategoriesLog.Log.M.WL(3, "hit. New missile health:" + missile.interceptInfo.missileHealth);
            }
            if (missile.interceptInfo.missileHealth < CustomAmmoCategories.Epsilon) {
              CustomAmmoCategoriesLog.Log.M.WL(3, "missile down");
              missile.interceptInfo.Intercepted = true;
              missile.hitPosition = pos;
              missile.hitLocation = 0;
              missile.GenerateTrajectory();
              missile.interceptInfo.InterceptedT = amsrec.weapon.AMS().calculateInterceptCorrection(AMSShootIdx, path, missile.trajectorySpline.Length, distance, missile.projectileSpeed);
              //  CustomAmmoCategories.calculateInterceptCorrection(CustomAmmoCategories.getWeaponEffect(amsrec.weapon), path, missile.UnitySpline.Length, distance, missile.missileProjectileSpeed);
              missile.interceptInfo.InterceptedAMS = amsrec.weapon;
              float t = missile.interceptInfo.InterceptedT;
              if (t > 0.9f) { t = Random.Range(0.85f, 0.95f); };
              missile.interceptInfo.InterceptedT = t;
              CustomAmmoCategoriesLog.Log.M.WL(3, "hit " + t + "\n");
              amsShoot = new AMSShoot(AMSShootIdx, t, missile.interceptInfo.InterceptedAMS);
            } else {
              float t = path / missile.trajectorySpline.Length;
              CustomAmmoCategoriesLog.Log.M.WL(3, "still flying " + t + "\n");
              amsShoot = new AMSShoot(AMSShootIdx, t, amsrec.weapon);
            }
            if (amsShoot != null) {
              CustomAmmoCategoriesLog.Log.LogWrite("Add AMShoot " + missile.parent.weaponIdx + " " + missile.hitIndex + " t:" + amsShoot.t + "\n");
              missile.interceptInfo.AMSShoots.Add(amsShoot);
            }
            if (missile.interceptInfo.Intercepted) { break; }
          }
          if (AMSInCharge == false) {
            CustomAmmoCategoriesLog.Log.LogWrite("  no AMS against " + instance.attacker.DisplayName + "\n");
            break;
          }
        }
        if (missilesInCharge == false) {
          CustomAmmoCategoriesLog.Log.LogWrite(" no more missiles\n");
          break;
        }
      }
      foreach (var jummInfoRec in amsJammingInfo) { 
        CustomAmmoCategories.jammAMSQueue.Enqueue(jummInfoRec.Value);
      }
      foreach (AMSRecord amsrec in ams) {
        CustomAmmoCategoriesLog.Log.LogWrite("AMS " + amsrec.weapon.defId + " shoots:" + amsrec.ShootsCount + "\n");
        if (amsrec.ShootsCount > 0) {
          amsrec.weapon.FlushAmmoCount(-1);
          float HeatGenerated = amsrec.weapon.HeatGenerated * (float)amsrec.ShootsCount / (float)amsrec.weapon.ShotsWhenFired;
          if (amsrec.weapon.parent != null) {
            amsrec.weapon.parent.AddWeaponHeat(amsrec.weapon, (int)HeatGenerated);
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("WARNING! missile launcher has no parent. That is very odd\n", true);
          }
          amsrec.weapon.AMSShootsCount(amsrec.weapon.AMSShootsCount() + amsrec.ShootsCount);
          amsrec.weapon.AMSActivationsCount(amsrec.weapon.AMSActivationsCount() + 1);
        }
      }
      WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
      //CustomAmmoCategoriesLog.Log.LogWrite("Applying missile interception\n");
      foreach (AdvWeaponHitInfoRec missile in missiles) {
        //missile.clearSpline();
        CustomAmmoCategoriesLog.Log.LogWrite(" " + missile.parent.groupIdx + " " + missile.parent.weaponIdx + " " + missile.hitIndex + " " + missile.interceptInfo.Intercepted + "\n");
        weaponHitInfo[missile.parent.groupIdx][missile.parent.weaponIdx].Value.hitPositions[missile.hitIndex] = missile.hitPosition;
        //SpreadHitRecord spreadCache = CustomAmmoCategories.getSpreadCache(weaponHitInfo[missile.groupIndex][missile.weaponIndex].Value, missile.hitIndex);
        //if (spreadCache != null) {
          //spreadCache.hitInfo.hitPositions[spreadCache.internalIndex] = missile.endPos;
        //}
        if (missile.interceptInfo.Intercepted) {
          // if (spreadCache != null) {
          //   spreadCache.hitInfo.hitLocations[spreadCache.internalIndex] = 0;
          // }
          weaponHitInfo[missile.parent.groupIdx][missile.parent.weaponIdx].Value.hitLocations[missile.hitIndex] = 0;
        }
      }
      typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, weaponHitInfo);
    }

    /*public static CachedMissileCurve getCachedMissileCurve(WeaponHitInfo hitInfo, int hitIndex) {
      if (CustomAmmoCategories.MissileCurveCache == null) { return null; };
      if (CustomAmmoCategories.MissileCurveCache.ContainsKey(hitInfo.attackSequenceId) == false) { return null; }
      if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return null; }
      if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return null; }
      if (CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) { return null; }
      return CustomAmmoCategories.MissileCurveCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitIndex];
    }*/

    /*public static void registerAMSCounterMeasure(WeaponHitInfo hitInfo, Weapon amsWeapon) {
      string key = CustomAmmoCategories.getGUIDFromHitInfo(hitInfo);
      if (CustomAmmoCategories.amsWeapons.ContainsKey(key) == false) { CustomAmmoCategories.amsWeapons.Add(key, amsWeapon); };
    }
    public static void unregisterAMSCounterMeasure(WeaponHitInfo hitInfo) {
      string key = CustomAmmoCategories.getGUIDFromHitInfo(hitInfo);
      if (CustomAmmoCategories.amsWeapons.ContainsKey(key) == true) { CustomAmmoCategories.amsWeapons.Remove(key); };
    }
    public static Weapon getAMSCounterMeasure(WeaponHitInfo hitInfo) {
      string key = CustomAmmoCategories.getGUIDFromHitInfo(hitInfo);
      if (CustomAmmoCategories.amsWeapons.ContainsKey(key) == true) { return CustomAmmoCategories.amsWeapons[key]; };
      return null;
    }*/
    /*public static void AMSFire(WeaponEffect weaponEffect, Vector3 target) {
      WeaponHitInfo amsHit = new WeaponHitInfo(-1, -1, -1, -1, string.Empty, string.Empty, -1, new float[0] { }, null, null, null, null, null, null, AttackDirection.None, Vector2.zero
          , new Vector3[1] { target });
      BallisticEffect AMSBallistic = weaponEffect as BallisticEffect;
      LaserEffect AMSLaserEffect = weaponEffect as LaserEffect;
      if (AMSBallistic != null) {
        if ((AMSBallistic.currentState == WeaponEffect.WeaponEffectState.Complete) || (AMSBallistic.currentState == WeaponEffect.WeaponEffectState.NotStarted)) {
          CustomAmmoCategoriesLog.Log.LogWrite("AMS first bullet\n");
          AMSBallistic.Fire(amsHit, -1, 0);
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("AMS next bullet\n");
          CustomAmmoCategories.AddBallisticEffectBullet(AMSBallistic);
          CustomAmmoCategories.FireNextAMSBullet(AMSBallistic, amsHit);
        }
      } else
      if (AMSLaserEffect != null) {
        CustomAmmoCategoriesLog.Log.LogWrite("AMS laser fire x:" + target.x + " y:" + target.y + " x:" + target.z + "\n");
        AMSLaserEffect.Fire(amsHit, -1, 0);
      }
    }*/
    /*public static void FireNextAMSBullet(BallisticEffect instance, WeaponHitInfo amsHit) {
      List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
      int currentBullet = (int)typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
      CustomAmmoCategoriesLog.Log.LogWrite("fire AMS bullet " + currentBullet + "\n");
      if (currentBullet < 0 || currentBullet >= bullets.Count) {
        return;
      }
      BulletEffect bullet = bullets[currentBullet];
      bullet.bulletIdx = currentBullet;
      bullet.Fire(amsHit, -1, 0);
      typeof(WeaponEffect).GetMethod("PlayMuzzleFlash", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, new object[0] { });
      string empty = string.Empty;
      string eventName = currentBullet != 0 ? (currentBullet != instance.weapon.ProjectilesPerShot - 1 ? instance.middleShotSFX : instance.lastShotSFX) : instance.firstShotSFX;
      AkGameObj parentAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("parentAudioObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
      if (!string.IsNullOrEmpty(eventName)) {
        int num = (int)WwiseManager.PostEvent(eventName, parentAudioObject, (AkCallbackManager.EventCallback)null, (object)null);
      }
      ++currentBullet;
      typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, (object)currentBullet);
      typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, (object)0.0f);
      instance.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
    }
    public static void AddBallisticEffectBullet(BallisticEffect instance) {
      CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
      GameObject gameObject = Combat.DataManager.PooledInstantiate(instance.bulletPrefab.name, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
      if ((Object)gameObject == (Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Error instantiating BulletObject " + instance.bulletPrefab.name + "\n", true);
        return;
      }
      gameObject.transform.parent = (Transform)null;
      BulletEffect component = gameObject.GetComponent<BulletEffect>();
      if ((Object)component == (Object)null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Error finding BulletEffect on GO " + instance.bulletPrefab.name + "\n", true);
        return;
      }
      List<BulletEffect> bullets = (List<BulletEffect>)typeof(BallisticEffect).GetField("bullets", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
      bullets.Add(component);
      component.Init(instance.weapon, instance);
    }
  }*/
}


  namespace CustomAmmoCategoriesPatches {
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("Fire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class WeaponEffect_Fire {
      public static bool Prefix(WeaponEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, ref Vector3 __state) {
        try {
          Log.LogWrite("WeaponEffect.Fire ");
          __state = hitInfo.hitPositions[hitIndex];
          Log.LogWrite(" save HitPosition " + __state + " ");
          Log.LogWrite(__instance.weapon.defId + " " + hitInfo.attackWeaponIndex + ":" + hitIndex + "\n");
        } catch (Exception e) {
          Log.LogWrite(e.ToString() + "\n");
        }
        return true;
      }
      public static void Postfix(WeaponEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, ref Vector3 __state) {
        Log.LogWrite("WeaponEffect.Fire " + __instance.weapon.UIName + " " + hitInfo.attackWeaponIndex + ":" + hitIndex + " restore HitPosition " + __state + "\n");
        hitInfo.hitPositions[hitIndex] = __state;
        __instance.hitInfo.hitPositions[hitIndex] = __state;
        typeof(WeaponEffect).GetField("endPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, hitInfo.hitPositions[hitIndex]);
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayPreFire")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class WeaponEffect_PlayPreFire {
      public static bool Prefix(WeaponEffect __instance) {
        try {
          Log.M.TWL(0,__instance.GetType().ToString()+ ".PlayPreFire sfx:"+ __instance.preFireSFX);
        } catch (Exception e) {
          Log.LogWrite(e.ToString() + "\n");
        }
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayImpact")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_PlayImpact {
      public static bool Prefix(WeaponEffect __instance) {
        int hitIndex = __instance.HitIndex();
        if (hitIndex >= 0) {
          if ((__instance.hitInfo.hitLocations[hitIndex] == 0) || (__instance.hitInfo.hitLocations[hitIndex] == 65536)) { return true; };
          AbstractActor actor = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as AbstractActor;
          if (actor != null) {
            Mech mech = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Mech;
            if (mech != null) {
              string strLocation = mech.GetStringForArmorLocation((ArmorLocation)__instance.hitInfo.hitLocations[hitIndex]);
              if (string.IsNullOrEmpty(strLocation)) {
                CustomAmmoCategoriesLog.Log.LogWrite("WARNING! bad location for mech:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
                __instance.hitInfo.hitLocations[hitIndex] = 0;
              }
            } else {
              Vehicle vehicle = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Vehicle;
              if (vehicle != null) {
                string strLocation = vehicle.GetStringForArmorLocation((VehicleChassisLocations)__instance.hitInfo.hitLocations[hitIndex]);
                if (string.IsNullOrEmpty(strLocation)) {
                  CustomAmmoCategoriesLog.Log.LogWrite("WARNING! bad location for vehicle:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
                  __instance.hitInfo.hitLocations[hitIndex] = 0;
                }
              }
            }
          }
        };
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PlayImpactAudio")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_PlayImpactAudio {
      public static bool Prefix(WeaponEffect __instance) {
        int hitIndex = __instance.HitIndex();
        if (hitIndex < 0) { return false; };
        if ((__instance.hitInfo.hitLocations[hitIndex] == 0) || (__instance.hitInfo.hitLocations[hitIndex] == 65536)) { return true; };
        AbstractActor actor = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as AbstractActor;
        if (actor != null) {
          Mech mech = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Mech;
          if (mech != null) {
            string strLocation = mech.GetStringForArmorLocation((ArmorLocation)__instance.hitInfo.hitLocations[hitIndex]);
            if (string.IsNullOrEmpty(strLocation)) {
              CustomAmmoCategoriesLog.Log.LogWrite("WARNING! bad location for mech:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
              __instance.hitInfo.hitLocations[hitIndex] = 0;
            }
          } else {
            Vehicle vehicle = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Vehicle;
            if (vehicle != null) {
              string strLocation = vehicle.GetStringForArmorLocation((VehicleChassisLocations)__instance.hitInfo.hitLocations[hitIndex]);
              if (string.IsNullOrEmpty(strLocation)) {
                CustomAmmoCategoriesLog.Log.LogWrite("WARNING! bad location for vehicle:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
                __instance.hitInfo.hitLocations[hitIndex] = 0;
              }
            }
          }
        }
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("OnImpact")]
    [HarmonyPatch(MethodType.Normal)]
#if BT1_8
    [HarmonyPatch(new Type[] { typeof(float), typeof(float) })]
#else
    [HarmonyPatch(new Type[] { typeof(float) })]
#endif
    public static class WeaponEffect_OnImpact {
#if BT1_8
      public static bool Prefix(WeaponEffect __instance, ref float hitDamage,ref float structureDamage, int ___hitIndex) {
#else
      public static bool Prefix(WeaponEffect __instance, ref float hitDamage, int ___hitIndex) {
#endif
        //Log.LogWrite("OnImpact hitIndex:" + ___hitIndex + "/"+__instance.hitInfo.numberOfShots+"\n");
        return true;
      }
#if BT1_8
      public static void Postfix(WeaponEffect __instance, ref float hitDamage, ref float structureDamage, int ___hitIndex) {
#else
      public static void Postfix(WeaponEffect __instance, ref float hitDamage, int ___hitIndex) {
#endif
        Log.LogWrite("OnImpact hitIndex:" + ___hitIndex + "/" + __instance.hitInfo.numberOfShots + "\n");
        AdvWeaponHitInfoRec advRec = __instance.hitInfo.advRec(___hitIndex);
        if (advRec == null) {
          Log.LogWrite(" no advanced record.");
          return;
        }
        if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
          Log.LogWrite(" frag projectile separated.");
          FragWeaponEffect fWe = __instance.fragEffect();
          if (fWe != null) {
            Log.LogWrite(" frag weapon effect found " + advRec.fragInfo.fragStartHitIndex + "\n");
            __instance.hitInfo.printHitPositions();
            Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            fWe.Fire(endPos, __instance.hitInfo, advRec.fragInfo.fragStartHitIndex);
            //__instance.unregisterFragEffect();
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" frag weapon effect not found\n");
            for (int shHitIndex = 0; shHitIndex < advRec.fragInfo.fragsCount; ++shHitIndex) {
              int shrapnelHitIndex = (shHitIndex + advRec.fragInfo.fragStartHitIndex);
              Log.LogWrite("  shellsHitIndex = " + shrapnelHitIndex + " dmg:" + advRec.Damage + "\n");
              advRec.parent.weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, shrapnelHitIndex, advRec.Damage, advRec.APDamage));
            }
          }
        }
        if (advRec.isAOEproc) {
          Log.LogWrite("OnImpact AOE Hit info found:" + ___hitIndex + "\n");
          for (int aoeHitIndex = 0; aoeHitIndex < advRec.parent.hits.Count; ++aoeHitIndex) {
            AdvWeaponHitInfoRec aoeRec = advRec.parent.hits[aoeHitIndex];
            if (aoeRec.isAOE == false) { continue; }
            Log.LogWrite(" hitIndex = " + aoeHitIndex + " " + aoeRec.target.GUID + " " + aoeRec.Damage + "/" + aoeRec.Heat + "/" + aoeRec.Stability + "\n");
            __instance.weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, aoeHitIndex, aoeRec.Damage, 0f));
          }
        }
      }
    }
    /*[HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("OnComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_OnComplete {
      public static bool Prefix(WeaponEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return true; };
        if (__instance.currentState == WeaponEffect.WeaponEffectState.Complete) {
          return false;
        }
        __instance.currentState = WeaponEffect.WeaponEffectState.Complete;
        typeof(WeaponEffect).GetMethod("PublishNextWeaponMessage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[0] { });
        __instance.PublishWeaponCompleteMessage();
        if (!((UnityEngine.Object)__instance.projectilePrefab != (UnityEngine.Object)null)) {
          return false;
        }
        AutoPoolObject autoPoolObject = __instance.projectile.GetComponent<AutoPoolObject>();
        if ((UnityEngine.Object)autoPoolObject == (UnityEngine.Object)null) {
          autoPoolObject = __instance.projectile.AddComponent<AutoPoolObject>();
        }
        string activeProjectileName = (string)typeof(WeaponEffect).GetField("activeProjectileName", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        autoPoolObject.Init(__instance.weapon.parent.Combat.DataManager, activeProjectileName, 4f);
        __instance.projectile = (GameObject)null;
        return false;
      }
    }*/
    /*[HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PublishNextWeaponMessage")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_PublishNextWeaponMessage {
      public static bool Prefix(WeaponEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return true; };
        typeof(WeaponEffect).GetField("attackSequenceNextDelayTimer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)-1f);
        typeof(WeaponEffect).GetField("hasSentNextWeaponMessage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)true);
        return false;
      }
    }*/
    /*[HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("PublishWeaponCompleteMessage")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_PublishWeaponCompleteMessage {
      public static bool Prefix(WeaponEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return true; };
        PropertyInfo property = typeof(WeaponEffect).GetProperty("FiringComplete");
        property.DeclaringType.GetProperty("FiringComplete");
        property.GetSetMethod(true).Invoke(__instance, new object[1] { (object)true });
        return false;
      }
    }*/
    /*[HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("OnBulletImpact")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(BulletEffect) })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class BallisticEffect_OnBulletImpact {
      public static bool Prefix(BallisticEffect __instance, BulletEffect bullet) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex < 0) { return false; };
        //typeof(WeaponEffect).GetMethod("OnImpact", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[1] { (object)0.0f });
        return true;
      }
    }*/
    /*[HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("SetupBullets")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class BallisticEffect_SetupBullets {
      public static bool Prefix(BallisticEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return true; };
        CustomAmmoCategoriesLog.Log.LogWrite("SetupBullets AMS\n");
        typeof(BallisticEffect).GetField("currentBullet", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)0);
        if ((double)__instance.shotDelay <= 0.0) {
          __instance.shotDelay = 0.5f;
        }
        typeof(WeaponEffect).GetField("rate", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)(1f / __instance.shotDelay));
        typeof(BallisticEffect).GetMethod("ClearBullets", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[0] { });
        CustomAmmoCategories.AddBallisticEffectBullet(__instance);
        return false;
      }
    }*/
    /*[HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("FireNextBullet")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class BallisticEffect_FireNextBullet {
      public static bool Prefix(BallisticEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return true; };
        CustomAmmoCategoriesLog.Log.LogWrite("FireNextBullet AMS\n");
        CustomAmmoCategories.FireNextAMSBullet(__instance, __instance.hitInfo);
        return false;
      }
    }*/
    /*[HarmonyPatch(typeof(BallisticEffect))]
    [HarmonyPatch("OnComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class BallisticEffect_OnComplete {
      public static void Postfix(WeaponEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return; };
        CustomAmmoCategoriesLog.Log.LogWrite("AMS ballistic complete\n");
        __instance.Reset();
        return;
      }
    }
    [HarmonyPatch(typeof(BulletEffect))]
    [HarmonyPatch("OnComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class BulletEffect_OnComplete {
      public static void Postfix(WeaponEffect __instance) {
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) { return; };
        __instance.Reset();
        CustomAmmoCategoriesLog.Log.LogWrite("AMS bullet complete\n");
        return;
      }
    }*/
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("PlayProjectile")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_PlayProjectileAMS {
      public static bool Prefix(MissileEffect __instance) {
        int hitIndex = __instance.HitIndex();
        AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
        if (cachedCurve == null) { return true; };
        CustomAmmoCategoriesLog.Log.LogWrite("Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("Altering start/end positions\n");
        typeof(WeaponEffect).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, (object)cachedCurve.startPosition);
        typeof(MissileEffect).GetField("preFireEndPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, (object)cachedCurve.startPosition);
        typeof(MissileEffect).GetField("endPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, (object)cachedCurve.hitPosition);
        CustomAmmoCategoriesLog.Log.LogWrite("Intercept " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + " T:" + cachedCurve.interceptInfo.getAMSShootT() + " " + cachedCurve.interceptInfo.AMSShootIndex + "/" + cachedCurve.interceptInfo.AMSShoots.Count + "\n");
        __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.getAMSShootT();
        return true;
      }
      public static void Postfix(MissileEffect __instance, int ___hitIndex) {
        AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(___hitIndex);
        if (cachedCurve == null) {
          if (__instance.weapon.isImprovedBallistic() == false) { return; };
          Log.LogWrite("Altering missile speed (rate) " + __instance.rate() + " -> ");
          __instance.rate(__instance.rate() * __instance.weapon.ProjectileSpeedMultiplier());
          Log.LogWrite(" " + __instance.rate() + "\n");
        } else {
          float distance = cachedCurve.trajectorySpline.Length;
          Log.LogWrite("Altering missile speed (rate) precached " + __instance.rate() + " -> ");
          __instance.rate(cachedCurve.projectileSpeed / distance);
          Log.LogWrite(" " + __instance.rate() + "\n");
        }
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("SpawnImpactExplosion")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(string) })]
    public static class MissileEffect_SpawnImpactExplosion {
      public static bool Prefix(MissileEffect __instance, string explosionName) {
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        CustomAmmoCategoriesLog.Log.LogWrite("SpawnImpactExplosion(" + explosionName + ")\n");
        CustomAmmoCategoriesLog.Log.LogWrite("EndPos " + endPos.x + "," + endPos.y + "," + endPos.z + "\n");
        return true;
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("GenerateMissilePath")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_GenerateMissilePath {
      public static bool Prefix(MissileEffect __instance) {
        int hitIndex = __instance.HitIndex();
        AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
        if (cachedCurve == null) { return true; };
        CustomAmmoCategoriesLog.Log.LogWrite("Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("Altering trajectory\n");
        CurvySpline spline = (CurvySpline)typeof(MissileEffect).GetField("spline", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        spline.Interpolation = CurvyInterpolation.Bezier;
        spline.Clear();
        spline.Closed = false;
        spline.Add(cachedCurve.trajectory);
        return false;
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("GenerateIndirectMissilePath")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_GenerateIndirectMissilePath {
      public static bool Prefix(MissileEffect __instance) {
        int hitIndex = __instance.HitIndex();
        AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
        if (cachedCurve == null) { return true; };
        CustomAmmoCategoriesLog.Log.LogWrite("Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("Altering trajectory\n");
        CurvySpline spline = (CurvySpline)typeof(MissileEffect).GetField("spline", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        spline.Interpolation = CurvyInterpolation.Bezier;
        spline.Clear();
        spline.Closed = false;
        spline.Add(cachedCurve.trajectory);
        return false;
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("Update")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_Update {
      public static void Postfix(MissileEffect __instance) {
        float t = (float)typeof(WeaponEffect).GetField("t", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        if (__instance.currentState == WeaponEffect.WeaponEffectState.Firing) {
          int hitIndex = __instance.HitIndex();
          if (__instance.hitInfo.dodgeRolls[hitIndex] <= -2.0f) {
            float AMSShootT = (0.0f - __instance.hitInfo.dodgeRolls[hitIndex]) - 2.0f;
            if (t >= AMSShootT) {
              CustomAmmoCategoriesLog.Log.LogWrite(" Update missile " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " t:" + t + " " + AMSShootT + "\n");
              AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
              __instance.hitInfo.dodgeRolls[hitIndex] = 0f;
              if (cachedCurve == null) { return; };
              AMSShoot amsShoot = cachedCurve.interceptInfo.getAMSShoot();
              if (amsShoot == null) {
                __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
                //__instance.hitInfo.hitLocations[hitIndex] = cachedCurve.Intercepted ? 65536 : __instance.hitInfo.hitLocations[hitIndex];
                return;
              };
              if (amsShoot.AMS == null) {
                __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
                //__instance.hitInfo.hitLocations[hitIndex] = cachedCurve.Intercepted ? 65536 : __instance.hitInfo.hitLocations[hitIndex];
                return;
              };
              CustomAmmoCategoriesLog.Log.LogWrite(" firing AMS " + amsShoot.AMS.UIName + " sootIdx:" + amsShoot.shootIdx + "\n");
              amsShoot.AMS.AMS().Fire(amsShoot.shootIdx);
              /*BallisticEffect amsBEffect = CustomAmmoCategories.getWeaponEffect(amsShoot.AMS) as BallisticEffect;
              LaserEffect amsLEffect = amsShoot.LAMSEffect;
              if (amsBEffect != null) {
                CustomAmmoCategories.AMSFire(amsBEffect, amsShoot.target);
              } else
              if (amsLEffect != null) {
                CustomAmmoCategories.AMSFire(amsLEffect, amsShoot.target);
              } else {
                __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.Intercepted ? -1.0f : 0f;
                return;
              }*/
              cachedCurve.interceptInfo.nextAMSShoot();
              amsShoot = cachedCurve.interceptInfo.getAMSShoot();
              if (amsShoot == null) {
                __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
                //__instance.hitInfo.hitLocations[hitIndex] = cachedCurve.Intercepted ? 65536 : __instance.hitInfo.hitLocations[hitIndex];
              } else {
                __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.getAMSShootT();
              }
            }
          }
        }
      }
    }
    [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
    [HarmonyPatch("GenerateToHitInfo")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { })]
    public static class AttackSequence_GenerateHitInfoAMS {
      public static void Postfix(AttackDirector.AttackSequence __instance) {
        Log.M.TWL(0, "AttackDirector.AttackSequence.GenerateToHitInfo");
        try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          Log.LogWrite("Main stray generator\n", true);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {

            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
                Log.M.TWL(0,"WeaponHitInfo at grp:"+groupIndex+" index:"+ weaponIndex + " is null. How?!",true);
                continue;
              }
              WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
              WeaponStrayHelper.MainStray(ref hitInfo);
              weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
            }
          }
          typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, weaponHitInfo);
        } catch (Exception e) {
          Log.LogWrite("WARNING! Stray generation FAIL\n" + e.ToString() + "\n", true);
        }
        try {
          CustomAmmoCategories.genreateAMSInterceptionInfo(__instance);
        } catch (Exception e) {
          Log.LogWrite("WARNING! AMS generation FAIL\n" + e.ToString() + "\n", true);
        }
        try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          Log.LogWrite("Frag generator\n", true);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
                Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
                continue;
              }
              WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
              FragWeaponHelper.FragSeparation(ref hitInfo);
              weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
            }
          }
          typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, weaponHitInfo);
        } catch (Exception e) {
          Log.LogWrite("WARNING! Frag generation FAIL\n" + e.ToString() + "\nFallback\n", true);
        }
        try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          Log.LogWrite("AoE generator\n", true);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
                Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
                continue;
              }
              WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
              AreaOfEffectHelper.AoEProcessing(ref hitInfo);
              weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
            }
          }
          typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, weaponHitInfo);
        } catch (Exception e) {
          Log.LogWrite("WARNING! AoE generation FAIL\n" + e.ToString() + "\n", true);
        }
        try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
                Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
                continue;
              }
              AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
              advInfo.FillResolveHitInfo();
            }
          }
        } catch (Exception e) {
          Log.LogWrite("WARNING! Hits ordering FAIL\n" + e.ToString() + "\n", true);
        }
        try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          DamageModifiersCache.ClearComulativeDamage();
          HashSet<ICombatant> affectedCombatants = new HashSet<ICombatant>();
          Log.M.TWL(0, "Start damage variance:"+__instance.id);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
                Log.M.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
                continue;
              }
              AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
              if (advInfo == null) { continue; }
              advInfo.weapon.ClearDamageCache();
              Log.M.TWL(0, "Damage variance grp:"+groupIndex+" index:"+weaponIndex+" wpn:"+advInfo.weapon.defId+" ammo:"+advInfo.weapon.ammo().Id+" mode:"+advInfo.weapon.mode().Id);
              foreach (AdvWeaponHitInfoRec advRec in advInfo.hits) {
                affectedCombatants.Add(advRec.target);
                advRec.ApplyVariance(null);
              }
              advInfo.FillResolveHitInfo();
              advInfo.ApplyHitEffects();
              advInfo.ApplyMantanceStats();

              AdvWeaponHitInfo.AddToExpected(advInfo);
            }
          }
          DamageModifiersCache.ClearComulativeDamage();
          AdvWeaponHitInfo.printExpectedMessages(__instance.id);
          Log.M.TWL(0, "Combatants affected by attack:" + affectedCombatants.Count);
          foreach(ICombatant trg in affectedCombatants) {
            Log.M.WL(1, trg.DisplayName + ":" + trg.GUID);
            foreach(var stat in trg.StatCollection) {
              Log.M.WL(2, stat.Key +"="+stat.Value.CurrentValue.type+":"+stat.Value.CurrentValue.ToString());
            }
          }
        } catch (Exception e) {
          Log.LogWrite("WARNING! Hits ordering FAIL\n" + e.ToString() + "\n", true);
        }
        /*try {
          WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
          for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
            for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
              Weapon weapon = __instance.GetWeapon(groupIndex, weaponIndex);
              //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
              if (weapon.HasShells()) {
                WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
                Dictionary<string, SpreadHitInfo> shrapnelInfo = CustomAmmoCategories.prepareShrapnelHitInfo(__instance, ref hitInfo, weapon, groupIndex, weaponIndex, hitInfo.numberOfShots, 1f);
                foreach (var shrapnelInf in shrapnelInfo) {
                  ICombatant combatant = __instance.Director.Combat.FindCombatantByGUID(shrapnelInf.Key);
                  WeaponHitInfo shrapnellHitInfo = shrapnelInf.Value.hitInfo;
                  if (combatant != null) {
                    AttackSequence_GenerateHitInfo.generateWeaponHitInfo(__instance, combatant, weapon, groupIndex, weaponIndex, shrapnelInf.Value.hitInfo.numberOfShots, __instance.indirectFire, 1f, ref shrapnellHitInfo, true,true);
                  }
                  shrapnelInf.Value.hitInfo = shrapnellHitInfo;
                }
                CustomAmmoCategories.consolidateShrapnelHitInfo(weapon, ref hitInfo, shrapnelInfo, weapon.ProjectilesPerShot, 1f);
                weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
              }
            }
          }
          typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, weaponHitInfo);
        } catch (Exception e) {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Shrapnel generation FAIL\n" + e.ToString() + "\nFallback\n", true);
        }*/
      }
    }
    public static partial class AdvWeaponHitInfoHelper {
      private class AmsHitsInfo {
        public int intercepted;
        public int all;
        public AmsHitsInfo() { intercepted = 0; all = 0; }
      }
      public static void ResolveAMSprocessing(int sequenceId) {
        if (AdvWeaponHitInfo.advancedWeaponHitInfo.ContainsKey(sequenceId) == false) { return; };
        Dictionary<int, Dictionary<int, AdvWeaponHitInfo>> seqAdvInfo = AdvWeaponHitInfo.advancedWeaponHitInfo[sequenceId];
        ICombatant attacker = null;
        //AdvWeaponHitInfo.advancedWeaponHitInfo.Remove(sequenceId);
        List<AdvWeaponHitInfo> advInfos = new List<AdvWeaponHitInfo>();
        foreach(var group in seqAdvInfo) {foreach(var wpn in group.Value) {advInfos.Add(wpn.Value);}};
        Dictionary<ICombatant, AmsHitsInfo> amsHitsInfo = new Dictionary<ICombatant, AmsHitsInfo>();
        HashSet<Weapon> amsShooted = new HashSet<Weapon>();
        bool isAMSShoots = false;
        foreach(AdvWeaponHitInfo advInfo in advInfos) {
          foreach(AdvWeaponHitInfoRec advRec in advInfo.hits) {
            attacker = advRec.parent.Sequence.attacker;
            if (advRec.interceptInfo.AMSImunne) { continue; }
            if (advRec.interceptInfo.AMSShoots.Count > 0) { isAMSShoots = true; }
            if (amsHitsInfo.ContainsKey(advRec.target) == false) { amsHitsInfo.Add(advRec.target, new AmsHitsInfo()); };
            amsHitsInfo[advRec.target].all += 1;
            if (advRec.interceptInfo.Intercepted) { amsHitsInfo[advRec.target].intercepted += 1; }
            foreach(AMSShoot amsShoot in advRec.interceptInfo.AMSShoots) {
              amsShooted.Add(amsShoot.AMS);
            }
          }
        }
        foreach(Weapon ams in amsShooted) {
          ams.AMS().Clear();
        }
        if (isAMSShoots) {
          foreach (var amsHI in amsHitsInfo) {
            if (attacker != null) { if (amsHI.Key.GUID == attacker.GUID) { continue; }; }
            if (amsHI.Value.all > 0) {
              string message_string = amsHI.Value.intercepted + " FROM " + amsHI.Value.all + " HIT BY AMS";
              amsHI.Key.Combat.MessageCenter.PublishMessage(
                                  new AddSequenceToStackMessage(
                                      new ShowActorInfoSequence(amsHI.Key, new Text("__/CAC.FROMHITBYAMS/__", amsHI.Value.intercepted, amsHI.Value.all), FloatieMessage.MessageNature.Buff, true)));

            }
          }
        }
        //AMSWeaponEffectStaticHelper.Clear(false);
      }
    }

    [HarmonyPatch(typeof(AttackDirector))]
    [HarmonyPatch("OnAttackComplete")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class AttackDirector_OnAttackComplete {
      public static bool Prefix(AttackDirector __instance, MessageCenterMessage message) {
        AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
        int sequenceId = attackCompleteMessage.sequenceId;
        try {
          AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
          if (attackSequence == null) {
            return true;
          }
          AdvWeaponHitInfoHelper.ResolveAMSprocessing(sequenceId);
          AdvWeaponHitInfo.Sanitize(sequenceId);
          AdvWeaponHitInfo.FlushInfo(attackSequence);
          AdvWeaponHitInfo.Clear(sequenceId);
          __instance.Combat.HandleSanitize();
          //ICombatant actor = attackSequence.chosenTarget;
          //HashSet<Weapon> AMSs = new HashSet<Weapon>();
          /*if (CustomAmmoCategories.MissileCurveCache != null) {
            if (CustomAmmoCategories.MissileCurveCache.ContainsKey(sequenceId)) {
              List<CachedMissileCurve> missiles = new List<CachedMissileCurve>();
              int allMissiles = 0;
              int interceptedMissiles = 0;
              var launchers_groups = CustomAmmoCategories.MissileCurveCache[sequenceId];
              foreach (var launchers in launchers_groups) {
                foreach (var launcher in launchers.Value) {
                  foreach (var launche in launcher.Value) {
                    ++allMissiles;
                    if (launche.Value.Intercepted) { ++interceptedMissiles; };
                    foreach(var amshoot in launche.Value.AMSShoots){
                      AMSs.Add(amshoot.AMS);
                    }
                  }
                }
              }
              foreach(var ams in AMSs) {ams.AMS().Clear();}
              CustomAmmoCategories.MissileCurveCache.Remove(sequenceId);
              if ((allMissiles > 0) && (interceptedMissiles > 0)) {
                string message_string = interceptedMissiles + " FROM " + allMissiles + " HIT BY AMS";
                actor.Combat.MessageCenter.PublishMessage(
                                    new AddSequenceToStackMessage(
                                        new ShowActorInfoSequence(actor, message_string, FloatieMessage.MessageNature.Buff, true)));
              }
            }
          }*/
        } catch (Exception e) {
          Log.LogWrite("WARNING! Advanced info clearing FAIL\n" + e.ToString() + "\nFallback\n", true);
        }
        return true;
      }
    }
  }
}