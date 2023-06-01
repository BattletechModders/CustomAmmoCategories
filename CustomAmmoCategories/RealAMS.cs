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
using HarmonyLib;
using CustAmmoCategories;
using FluffyUnderware.Curvy;
using Localize;
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesHelper;

namespace CustAmmoCategories {
  public class AMSRecord {
    public Weapon weapon;
    public int ShootsRemains;
    public int ShootsCount;
    public int InterceptedTrace;
    public int InterceptedTraceInital;
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
        } else if (weapon.AMSActivationsPerTurn() > weapon.AMSActivationsCount()) {
          ShootsRemains = weapon.ShotsWhenFired;
        } else {
          ShootsRemains = 0;
        }
      }
      if (ShootsRemains < 0) { ShootsRemains = 0; };
      Range = weapon.MaxRange;
      InterceptedTrace = 0;
      InterceptedTraceInital = weapon.AMSInterceptedTrace();
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
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, Weapon> amsWeapons = new Dictionary<string, Weapon>();
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
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return (mode.AMSHitChance + extAmmo.AMSHitChance + weapon.GetStatisticFloat("AMSHitChance")) * (weapon.GetStatisticMod("AMSHitChance")) * extAmmo.AMSHitChanceMod * mode.AMSHitChanceMod; //+ extWeapon.AMSHitChance;
    }
    public static float AMSHitChanceMult(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return (mode.AMSHitChanceMult + extAmmo.AMSHitChanceMult + weapon.GetStatisticFloat("AMSHitChanceMult")) * (weapon.GetStatisticMod("AMSHitChanceMult"));
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
      Log.Combat?.WL(0, weapon.defId + ".getWeaponEffect");
      if (weapon.weaponRep == null) {
        Log.Combat?.WL(0, "WARNING! Weapon " + weapon.defId + " " + weapon.UIName + " on " + weapon.parent.DisplayName + ":" + weapon.parent.GUID + " has no representation! It no visuals will be played", true);
        return null;
      };
      Statistic wGUIDstat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (wGUIDstat == null) { return weapon.weaponRep.WeaponEffect; }
      Log.Combat?.WL(2, "weapon GUID is set");
      Log.Combat?.WL(2, "weapon ammoId is set");
      string wGUID = wGUIDstat.Value<string>();
      WeaponMode weaponMode = weapon.mode();
      string weaponEffectId = weaponMode.WeaponEffectID;
      Log.Combat?.WL(1, "weaponMode.WeaponEffectID = " + weaponMode.WeaponEffectID);
      WeaponEffect currentEffect = (WeaponEffect)null;
      if (string.IsNullOrEmpty(weaponEffectId)) {
        Log.Combat?.WL(1, "null or empty");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.ammo().WeaponEffectID;
        Log.Combat?.WL(1, "ammo.WeaponEffectID = " + weaponEffectId);
        if (string.IsNullOrEmpty(weaponEffectId)) {
          weaponEffectId = weapon.weaponDef.WeaponEffectID;
        }
      }
      if (weaponEffectId == weapon.weaponDef.WeaponEffectID) {
        Log.Combat?.WL(1, "same as per weapon");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.weaponDef.WeaponEffectID;
      };
      if (currentEffect == (WeaponEffect)null) {
        Log.Combat?.WL(1, "getting weapon effect " + wGUID + "." + weaponEffectId);
        currentEffect = CustomAmmoCategories.getWeaponEffect(wGUID, weaponEffectId);
      }
      if (currentEffect == (WeaponEffect)null) {
        currentEffect = weapon.weaponRep.WeaponEffect;
      }
      return currentEffect;
    }
    public static string getWeaponEffectID(this Weapon weapon) {
      Log.Combat?.WL(0, weapon.defId + ".getWeaponEffectID");
      if (weapon.weaponRep == null) {
        Log.Combat?.WL(0, "WARNING! Weapon " + weapon.defId + " " + weapon.UIName + " on " + weapon.parent.DisplayName + ":" + weapon.parent.GUID + " has no representation! It no visuals will be played\n", true);
        return string.Empty;
      };
      Statistic wGUIDstat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (wGUIDstat == null) { return weapon.weaponDef.WeaponEffectID; }
      Log.Combat?.WL(2, "weapon GUID is set");
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      WeaponMode weaponMode = weapon.mode();
      string weaponEffectId = weaponMode.WeaponEffectID;
      Log.Combat?.WL(1, "weaponMode.WeaponEffectID = " + weaponMode.WeaponEffectID);
      WeaponEffect currentEffect = (WeaponEffect)null;
      if (string.IsNullOrEmpty(weaponEffectId)) {
        Log.Combat?.WL(1, "null or empty");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.mode().WeaponEffectID;
        Log.Combat?.WL(1, "ammo.WeaponEffectID = " + weaponEffectId);
        if (string.IsNullOrEmpty(weaponEffectId)) {
          weaponEffectId = weapon.weaponDef.WeaponEffectID;
        }
      }
      if (weaponEffectId == weapon.weaponDef.WeaponEffectID) {
        Log.Combat?.WL(1, "same as per weapon");
        currentEffect = weapon.weaponRep.WeaponEffect;
        weaponEffectId = weapon.weaponDef.WeaponEffectID;
        return weapon.weaponDef.WeaponEffectID;
      };
      if (currentEffect == (WeaponEffect)null) {
        Log.Combat?.WL(1, "getting weapon effect " + wGUID + "." + weaponEffectId);
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
    public static int AMSInterceptedTrace(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return mode.AMSInterceptedTrace + extAmmo.AMSInterceptedTrace + extWeapon.AMSInterceptedTrace > 0? extWeapon.AMSInterceptedTrace:CustomAmmoCategories.Settings.AMSDefaultInterceptedTrace;
    }
    public static float AMSAttractiveness(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      ExtAmmunitionDef extAmmo = weapon.ammo();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return (mode.AMSAttractiveness + extAmmo.AMSAttractiveness + weapon.GetStatisticFloat("AMSAttractiveness")) * (weapon.GetStatisticMod("AMSAttractiveness")); //+ extWeapon.MissileHealth;
    }
    public static bool getWeaponAMSImmune(Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.AMSImmune != TripleBoolean.NotSet) { return mode.AMSImmune == TripleBoolean.True; }
      ExtAmmunitionDef extAmmo = weapon.ammo();
      if (extAmmo.AMSImmune != TripleBoolean.NotSet) { return extAmmo.AMSImmune == TripleBoolean.True; }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      return extWeapon.AMSImmune == TripleBoolean.True;
    }
    public static void genreateAMSInterceptionInfo(AttackDirector.AttackSequence instance) {
      Log.Combat?.TWL(0, "genreateAMSInterceptionInfo");
      List<AdvWeaponHitInfoRec> missiles = instance.Interceptables();
      Dictionary<Weapon, AMSJammInfoMessage> amsJammingInfo = new Dictionary<Weapon, AMSJammInfoMessage>();
      Log.Combat?.WL(0, "missiles:" + missiles.Count);
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
      Log.Combat?.WL(0, "AMS list:");
      foreach (ICombatant target in targetsList) {
        Log.Combat?.WL(1, "actor:" + target.DisplayName + ":" + target.GUID);
        AbstractActor targetActor = target as AbstractActor;
        if (targetActor == null) { continue; };
        if (targetActor.GUID == instance.attacker.GUID) {
          Log.Combat?.WL(0, "i will not fire my own missiles.");
          continue;
        }
        if (targetActor.IsShutDown) {
          Log.Combat?.WL(2, "shutdown");
          continue;
        };
        if (targetActor.IsDead) {
          Log.Combat?.WL(2, "dead");
          continue;
        };
        foreach (Weapon weapon in targetActor.Weapons) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
          Log.Combat?.WL(2, weapon.defId + " mode:" + weapon.mode().Id + " ammo:" + weapon.ammo().Id + " AMS:" + weapon.isAMS() + " AAMS:" + weapon.isAAMS() + " have weaponRep:" + (weapon.weaponRep == null ? "false" : "true"));
          if ((weapon.isAMS()) && (weapon.isAAMS() == false) && (weapon.weaponRep != null)) {
            Log.Combat?.WL(2, "AMS " + weapon.UIName);
            ams.Add(new AMSRecord(weapon, extWeapon, false));
          }
        }
      }
      Log.Combat?.WL(0, "Searching advanced AMS in battle.");
      CombatGameState combat = instance.attacker.Combat;
      List<AbstractActor> atackerEnemies = combat.GetAllEnemiesOf(instance.attacker);
      Log.Combat?.WL(0, "AAMS list:\n");
      foreach (AbstractActor enemy in atackerEnemies) {
        Log.Combat?.WL(1, "actor:" + enemy.DisplayName + ":" + enemy.GUID);
        if (enemy.IsShutDown) {
          Log.Combat?.WL(2, "shutdown");
          continue;
        };
        if (enemy.IsDead) {
          Log.Combat?.WL(2, "dead");
          continue;
        };
        foreach (Weapon weapon in enemy.Weapons) {
          ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
          Log.Combat?.WL(2, weapon.defId + " mode:" + weapon.mode().Id + " ammo:" + weapon.ammo().Id + " AMS:" + weapon.isAMS() + " AAMS:" + weapon.isAAMS() + " have weaponRep:" + (weapon.weaponRep == null ? "false" : "true"));
          if (weapon.isAAMS() && (weapon.weaponRep != null)) {
            Log.Combat?.WL(2, "AAMS " + weapon.UIName);
            ams.Add(new AMSRecord(weapon, extWeapon, true));
          }
        }
      }
      if (ams.Count <= 0) {
        Log.Combat?.WL(0, "No one AMS found.");
        return;
      };
      bool NoActiveAMS = true;
      Log.Combat?.TWL(0, "FIELD AMS LIST. ROUND:" + combat.TurnDirector.CurrentRound + " PHASE:" + combat.TurnDirector.CurrentPhase + " seqId:" + instance.id);
      if (missiles.Count > 0) {
        foreach (AMSRecord amsrec in ams) {
          Log.Combat?.WL(2, $"{amsrec.weapon.parent.DisplayName}.{amsrec.weapon.defId} ShootsRemains:{amsrec.ShootsRemains} maxShoots:{amsrec.weapon.ShotsWhenFired} shotsPerformed:{amsrec.weapon.AMSShootsCount()} maxActiv:{amsrec.weapon.AMSActivationsPerTurn()} curActiv:{amsrec.weapon.AMSActivationsCount()} CanFire:{amsrec.weapon.CanFire} AMSShootsEveryAttack:{amsrec.weapon.AMSShootsEveryAttack()} mode:{amsrec.weapon.mode().Id} ammo:{amsrec.weapon.ammo().Id} AMS:{amsrec.weapon.isAMS()} AAMS:{amsrec.weapon.isAAMS()} have weaponRep:{(amsrec.weapon.weaponRep == null ? "false" : "true")}");
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
        Log.Combat?.WL(0, "No one active AMS found.");
        return;
      };
      //foreach (AbstractActor trgAlly in combat.GetAllAlliesOf)
      if (CustomAmmoCategories.Settings.AMSUseAttractiveness) {
        List<AdvWeaponHitInfoRec> trackMissiles = new List<AdvWeaponHitInfoRec>();
        foreach (AdvWeaponHitInfoRec missile in missiles) {
          if (missile.interceptInfo.AMSImunne) {
            Log.Combat?.WL(2, $"missile immune to AMS {missile.parent.weapon.defId} {missile.hitIndex}");
            continue;
          }
          trackMissiles.Add(missile);
        }
        trackMissiles.Sort((a, b) => {
          if (a.interceptInfo.AMSAttractiveness.Equals(b.interceptInfo.AMSAttractiveness) == false) { return b.interceptInfo.AMSAttractiveness.CompareTo(a.interceptInfo.AMSAttractiveness); }
          if (a.interceptInfo.missileHealth.Equals(b.interceptInfo.missileHealth) == false) { return b.interceptInfo.missileHealth.CompareTo(a.interceptInfo.missileHealth); }
          return b.interceptInfo.AMSHitChance.CompareTo(a.interceptInfo.AMSHitChance);
        });
        Log.Combat?.WL(2, $"missiles:{trackMissiles.Count}");
        foreach (AdvWeaponHitInfoRec missile in trackMissiles) {
          float trajectoryLength = missile.trajectorySpline.Length;
          bool AMSInCharge = false;
          for (float path = 0f; path < trajectoryLength; path += 10f) {
            Vector3 pos = missile.trajectorySpline.InterpolateByDistance(path);
            AMSInCharge = false;
            foreach (AMSRecord amsrec in ams) { amsrec.InterceptedTrace = amsrec.InterceptedTraceInital; }
            foreach (AMSRecord amsrec in ams) {
              if (amsrec.ShootsRemains <= 0) { continue; };
              AMSInCharge = true;
              if ((amsrec.isAAMS == false)) {
                if (amsrec.weapon.parent != null) {
                  if (missile.target != null) { if (amsrec.weapon.parent.GUID != missile.target.GUID) { continue; }; }
                }
              }
              float distance = Vector3.Distance(pos, amsrec.weapon.parent.CurrentPosition);
              Log.Combat?.WL(0, distance.ToString());
              if (distance > amsrec.Range) { continue; };
              Log.Combat?.WL(3, $"AMS ready to fire {amsrec.weapon.defId}.{amsrec.weapon.GUID} dmg:{amsrec.weapon.AMSDamage()} shoot remains:{amsrec.ShootsRemains}");
              if (amsJammingInfo.ContainsKey(amsrec.weapon) == false) {
                amsJammingInfo.Add(amsrec.weapon, new AMSJammInfoMessage(amsrec.weapon));
              }
              if (amsrec.weapon.DecrementOneAmmo() == false) {
                Log.Combat?.WL(4, "AMS ammo depleted " + amsrec.weapon.tCurrentAmmo());
                amsrec.ShootsRemains = 0;
                continue;
              }
              --amsrec.ShootsRemains;
              ++amsrec.ShootsCount;
              int AMSShootIdx = amsrec.weapon.AMS().AddHitPosition(pos);
              AMSShoot amsShoot = null;
              float interceptRoll = Random.Range(0f, 1f);
              float effectiveHitChance = (amsrec.AMSHitChance + missile.interceptInfo.AMSHitChance) * missile.interceptInfo.AMSHitChanceMult;
              Log.Combat?.WL(4, $"roll:{interceptRoll} chance:{effectiveHitChance} AMS.HitChance:{amsrec.AMSHitChance} Missile.HitChance:{missile.interceptInfo.AMSHitChance} Missile.Mod:{missile.interceptInfo.AMSHitChanceMult}");
              
              if (interceptRoll < effectiveHitChance) {
                missile.interceptInfo.missileHealth -= amsrec.weapon.AMSDamage();
                Log.Combat?.WL(3, $"hit. New missile health:" + missile.interceptInfo.missileHealth);
              }
              
              if (missile.interceptInfo.missileHealth < CustomAmmoCategories.Epsilon) {
                Log.Combat?.WL(3, "missile down");
                missile.interceptInfo.Intercepted = true;
                missile.hitPosition = pos;
                missile.hitLocation = 0;
                missile.GenerateTrajectory();
                missile.interceptInfo.InterceptedT = amsrec.weapon.AMS().calculateInterceptCorrection(AMSShootIdx, path, missile.trajectorySpline.Length, distance, missile.projectileSpeed);
                missile.interceptInfo.InterceptedAMS = amsrec.weapon;
                float t = missile.interceptInfo.InterceptedT;
                if (t > 0.9f) { t = Random.Range(0.85f, 0.95f); };
                missile.interceptInfo.InterceptedT = t;
                Log.Combat?.WL(3, $"hit at {t}");
                amsShoot = new AMSShoot(AMSShootIdx, t, missile.interceptInfo.InterceptedAMS);
              } else {
                float t = path / missile.trajectorySpline.Length;
                Log.Combat?.WL(3, "still flying " + t);
                amsShoot = new AMSShoot(AMSShootIdx, t, amsrec.weapon);
              }
            //add_ams_shoot:
              if (amsShoot != null) {
                Log.Combat?.WL(0, "Add AMShoot " + missile.parent.weaponIdx + " " + missile.hitIndex + " t:" + amsShoot.t);
                missile.interceptInfo.AMSShoots.Add(amsShoot);
              }
              if (missile.interceptInfo.Intercepted) { break; }
            }
            if (AMSInCharge == false) { Log.Combat?.WL(2, "no AMS or all of them reach shooting limit"); break; }
            if (missile.interceptInfo.Intercepted) {
              foreach (AMSRecord amsrec in ams) {
                float distance = Vector3.Distance(missile.hitPosition, amsrec.weapon.parent.CurrentPosition);
                if (distance > amsrec.Range) { continue; };
                while ((amsrec.InterceptedTrace > 0)&&(amsrec.ShootsRemains > 0)) {
                  if (amsJammingInfo.ContainsKey(amsrec.weapon) == false) {
                    amsJammingInfo.Add(amsrec.weapon, new AMSJammInfoMessage(amsrec.weapon));
                  }
                  Log.Combat?.WL(3, $"AMS ready to fire {amsrec.weapon.defId}.{amsrec.weapon.GUID} dmg:{amsrec.weapon.AMSDamage()} shoot remains:{amsrec.ShootsRemains}");
                  if (amsrec.weapon.DecrementOneAmmo() == false) {
                    Log.Combat?.WL(4, "AMS ammo depleted " + amsrec.weapon.tCurrentAmmo());
                    amsrec.ShootsRemains = 0;
                    continue;
                  }
                  --amsrec.ShootsRemains;
                  ++amsrec.ShootsCount;
                  int AMSShootIdx = amsrec.weapon.AMS().AddHitPosition(pos);
                  AMSShoot amsShoot = new AMSShoot(AMSShootIdx, missile.interceptInfo.InterceptedT, amsrec.weapon);
                  Log.Combat?.WL(4, "shoot intercepted. rest trace " + amsrec.InterceptedTrace);
                  --amsrec.InterceptedTrace;
                }
              }
              break;
            }
          }
        }
      } else {
        float longestPath = 0.0f;
        HashSet<AdvWeaponHitInfoRec> trackMissiles = new HashSet<AdvWeaponHitInfoRec>();
        for (float path = 0.0f; path < longestPath; path += 10.0f) {
          Log.Combat?.WL(1,"path:" + path);
          bool missilesInCharge = false;
          foreach (AdvWeaponHitInfoRec missile in missiles) {
            if (trackMissiles.Contains(missile) == false) { continue; }
            if (missile.trajectorySpline.Length <= path) {
              Log.Combat?.WL(2, "missile done " + missile.parent.weapon.defId + " " + missile.hitIndex);
              trackMissiles.Remove(missile);
            }
            if (missile.interceptInfo.Intercepted == true) {
              Log.Combat?.WL(2, "missile intercepted " + missile.parent.weapon.defId + " " + missile.hitIndex);
              trackMissiles.Remove(missile);
            }
          }
          foreach (AdvWeaponHitInfoRec missile in trackMissiles) {
            missilesInCharge = true;
            Vector3 pos = missile.trajectorySpline.InterpolateByDistance(path);
            bool AMSInCharge = false;
            Log.Combat?.WL(2, "missile interception " + missile.parent.weapon.defId + " hitIndex:" + missile.hitIndex + " weaponId:" + missile.parent.weaponIdx + " hp:" + missile.interceptInfo.missileHealth);
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
              Log.Combat?.WL(0, distance.ToString());
              if (distance > amsrec.Range) { continue; };
              Log.Combat?.WL(3, "AMS ready to fire " + amsrec.weapon.defId + " dmg:" + amsrec.weapon.AMSDamage() + " shoot remains:" + amsrec.ShootsRemains + "\n");
              if (amsJammingInfo.ContainsKey(amsrec.weapon) == false) {
                amsJammingInfo.Add(amsrec.weapon, new AMSJammInfoMessage(amsrec.weapon));
              }
              if (amsrec.weapon.DecrementOneAmmo() == false) {
                Log.Combat?.WL(4, "AMS ammo depleted " + amsrec.weapon.tCurrentAmmo());
                amsrec.ShootsRemains = 0;
                continue;
              }
              --amsrec.ShootsRemains;
              ++amsrec.ShootsCount;
              float interceptRoll = Random.Range(0f, 1f);
              float effectiveHitChance = amsrec.AMSHitChance + missile.interceptInfo.AMSHitChance;
              Log.Combat?.WL(4, "roll:" + interceptRoll + " chance:" + effectiveHitChance);
              AMSShoot amsShoot = null;
              int AMSShootIdx = amsrec.weapon.AMS().AddHitPosition(pos);
              if (interceptRoll < effectiveHitChance) {
                missile.interceptInfo.missileHealth -= amsrec.weapon.AMSDamage();
                Log.Combat?.WL(3, "hit. New missile health:" + missile.interceptInfo.missileHealth);
              }
              if (missile.interceptInfo.missileHealth < CustomAmmoCategories.Epsilon) {
                Log.Combat?.WL(3, "missile down");
                missile.interceptInfo.Intercepted = true;
                missile.hitPosition = pos;
                missile.hitLocation = 0;
                missile.GenerateTrajectory();
                missile.interceptInfo.InterceptedT = amsrec.weapon.AMS().calculateInterceptCorrection(AMSShootIdx, path, missile.trajectorySpline.Length, distance, missile.projectileSpeed);
                missile.interceptInfo.InterceptedAMS = amsrec.weapon;
                float t = missile.interceptInfo.InterceptedT;
                if (t > 0.9f) { t = Random.Range(0.85f, 0.95f); };
                missile.interceptInfo.InterceptedT = t;
                Log.Combat?.WL(3, "hit " + t);
                amsShoot = new AMSShoot(AMSShootIdx, t, missile.interceptInfo.InterceptedAMS);
              } else {
                float t = path / missile.trajectorySpline.Length;
                Log.Combat?.WL(3, "still flying " + t);
                amsShoot = new AMSShoot(AMSShootIdx, t, amsrec.weapon);
              }
              if (amsShoot != null) {
                Log.Combat?.WL(0, "Add AMShoot " + missile.parent.weaponIdx + " " + missile.hitIndex + " t:" + amsShoot.t);
                missile.interceptInfo.AMSShoots.Add(amsShoot);
              }
              if (missile.interceptInfo.Intercepted) { break; }
            }
            if (AMSInCharge == false) {
              Log.Combat?.WL(2, "no AMS against " + instance.attacker.DisplayName);
              break;
            }
          }
          if (missilesInCharge == false) {
            Log.Combat?.WL(1, "no more missiles");
            break;
          }
        }
      }
      foreach (var jummInfoRec in amsJammingInfo) {
        CustomAmmoCategories.jammAMSQueue.Enqueue(jummInfoRec.Value);
      }
      foreach (AMSRecord amsrec in ams) {
        Log.Combat?.WL(0, "AMS " + amsrec.weapon.defId + " shoots:" + amsrec.ShootsCount);
        if (amsrec.ShootsCount > 0) {
          amsrec.weapon.FlushAmmoCount(-1);
          float HeatGenerated = amsrec.weapon.HeatGenerated * (float)amsrec.ShootsCount / (float)amsrec.weapon.ShotsWhenFired;
          if (amsrec.weapon.parent != null) {
            amsrec.weapon.parent.AddWeaponHeat(amsrec.weapon, (int)HeatGenerated);
          } else {
            Log.Combat?.WL(0, "WARNING! missile launcher has no parent. That is very odd", true);
          }
          amsrec.weapon.AMSShootsCount(amsrec.weapon.AMSShootsCount() + amsrec.ShootsCount);
          amsrec.weapon.AMSActivationsCount(amsrec.weapon.AMSActivationsCount() + 1);
        }
      }
      WeaponHitInfo?[][] weaponHitInfo = instance.weaponHitInfo;
      foreach (AdvWeaponHitInfoRec missile in missiles) {
        Log.Combat?.WL(0,$"missile {missile.parent.groupIdx}.{missile.parent.weaponIdx} hitIndex:{missile.hitIndex} intercepted:{missile.interceptInfo.Intercepted}");
        weaponHitInfo[missile.parent.groupIdx][missile.parent.weaponIdx].Value.hitPositions[missile.hitIndex] = missile.hitPosition;
        if (missile.interceptInfo.Intercepted) {
          weaponHitInfo[missile.parent.groupIdx][missile.parent.weaponIdx].Value.hitLocations[missile.hitIndex] = 0;
        }
      }
      instance.weaponHitInfo = weaponHitInfo;
    }

  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("Fire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(int) })]
  [HarmonyPriority(Priority.HigherThanNormal)]
  public static class WeaponEffect_Fire {
    public static void Prefix(WeaponEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, ref Vector3 __state) {
      try {
        Log.Combat?.W(0,"WeaponEffect.Fire");
        __state = hitInfo.hitPositions[hitIndex];
        Log.Combat?.W(1, "save HitPosition " + __state);
        Log.Combat?.WL(1, __instance.weapon.defId + " " + hitInfo.attackWeaponIndex + ":" + hitIndex);
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        Weapon.logger.LogException(e);
      }
    }
    public static void Postfix(WeaponEffect __instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex, ref Vector3 __state) {
      Log.Combat?.WL(0,"WeaponEffect.Fire " + __instance.weapon.UIName + " " + hitInfo.attackWeaponIndex + ":" + hitIndex + " restore HitPosition " + __state);
      hitInfo.hitPositions[hitIndex] = __state;
      __instance.hitInfo.hitPositions[hitIndex] = __state;
      __instance.endPos(hitInfo.hitPositions[hitIndex]);
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayPreFire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  [HarmonyPriority(Priority.HigherThanNormal)]
  public static class WeaponEffect_PlayPreFire {
    public static void Prefix(WeaponEffect __instance) {
      try {
        Log.Combat?.TWL(0, __instance.GetType().ToString() + ".PlayPreFire sfx:" + __instance.preFireSFX);
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString());
        Weapon.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayImpact {
    public static void Prefix(ref bool __runOriginal, WeaponEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      if (hitIndex >= 0) {
        if ((__instance.hitInfo.hitLocations[hitIndex] == 0) || (__instance.hitInfo.hitLocations[hitIndex] == 65536)) { return; };
        AbstractActor actor = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as AbstractActor;
        if (actor != null) {
          Mech mech = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Mech;
          if (mech != null) {
            string strLocation = mech.GetStringForArmorLocation((ArmorLocation)__instance.hitInfo.hitLocations[hitIndex]);
            if (string.IsNullOrEmpty(strLocation)) {
              Log.Combat?.WL(0,"WARNING! bad location for mech:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting", true);
              __instance.hitInfo.hitLocations[hitIndex] = 0;
            }
          } else {
            Vehicle vehicle = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Vehicle;
            if (vehicle != null) {
              string strLocation = vehicle.GetStringForArmorLocation((VehicleChassisLocations)__instance.hitInfo.hitLocations[hitIndex]);
              if (string.IsNullOrEmpty(strLocation)) {
                Log.Combat?.WL(0, "WARNING! bad location for vehicle:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting", true);
                __instance.hitInfo.hitLocations[hitIndex] = 0;
              }
            }
          }
        }
      };
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayImpactAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayImpactAudio {
    public static void Prefix(ref bool __runOriginal, WeaponEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      if (hitIndex < 0) { __runOriginal = false; return; };
      if ((__instance.hitInfo.hitLocations[hitIndex] == 0) || (__instance.hitInfo.hitLocations[hitIndex] == 65536)) { return; };
      AbstractActor actor = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as AbstractActor;
      if (actor != null) {
        Mech mech = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Mech;
        if (mech != null) {
          string strLocation = mech.GetStringForArmorLocation((ArmorLocation)__instance.hitInfo.hitLocations[hitIndex]);
          if (string.IsNullOrEmpty(strLocation)) {
            Log.Combat?.WL(0, "WARNING! bad location for mech:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
            __instance.hitInfo.hitLocations[hitIndex] = 0;
          }
        } else {
          Vehicle vehicle = __instance.weapon.parent.Combat.AttackDirector.GetHitInfoTarget(__instance.hitInfo) as Vehicle;
          if (vehicle != null) {
            string strLocation = vehicle.GetStringForArmorLocation((VehicleChassisLocations)__instance.hitInfo.hitLocations[hitIndex]);
            if (string.IsNullOrEmpty(strLocation)) {
              Log.Combat?.WL(0, "WARNING! bad location for vehicle:" + __instance.hitInfo.hitLocations[hitIndex] + ".Correcting\n", true);
              __instance.hitInfo.hitLocations[hitIndex] = 0;
            }
          }
        }
      }
      return;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("OnImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float), typeof(float) })]
  public static class WeaponEffect_OnImpact {
    public static void Postfix(WeaponEffect __instance, ref float hitDamage, ref float structureDamage) {
      Log.Combat?.WL(0,"OnImpact hitIndex:" + __instance.hitIndex + "/" + __instance.hitInfo.numberOfShots);
      AdvWeaponHitInfoRec advRec = __instance.hitInfo.advRec(__instance.hitIndex);
      if (advRec == null) {
        Log.LogWrite(" no advanced record.");
        return;
      }
      if (advRec.fragInfo.separated && (advRec.fragInfo.fragStartHitIndex >= 0) && (advRec.fragInfo.fragsCount > 0)) {
        Log.Combat?.WL(1, "frag projectile separated.");
        FragWeaponEffect fWe = __instance.fragEffect();
        if (fWe != null) {
          Log.Combat?.WL(1, "frag weapon effect found " + advRec.fragInfo.fragStartHitIndex);
          __instance.hitInfo.printHitPositions();
          Vector3 endPos = __instance.endPos();
          fWe.Fire(endPos, __instance.hitInfo, advRec.fragInfo.fragStartHitIndex);
        } else {
          Log.Combat?.WL(1, "frag weapon effect not found");
          for (int shHitIndex = 0; shHitIndex < advRec.fragInfo.fragsCount; ++shHitIndex) {
            int shrapnelHitIndex = (shHitIndex + advRec.fragInfo.fragStartHitIndex);
            Log.Combat?.WL(2, "shellsHitIndex = " + shrapnelHitIndex + " dmg:" + advRec.Damage);
            advRec.parent.weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, shrapnelHitIndex, advRec.Damage, advRec.APDamage));
          }
        }
      }
      if (advRec.isAOEproc) {
        Log.Combat?.WL(0, "OnImpact AOE Hit info found:" + __instance.hitIndex);
        for (int aoeHitIndex = 0; aoeHitIndex < advRec.parent.hits.Count; ++aoeHitIndex) {
          AdvWeaponHitInfoRec aoeRec = advRec.parent.hits[aoeHitIndex];
          if (aoeRec.isAOE == false) { continue; }
          Log.Combat?.WL(1, "hitIndex = " + aoeHitIndex + " " + aoeRec.target.GUID + " " + aoeRec.Damage + "/" + aoeRec.Heat + "/" + aoeRec.Stability);
          __instance.weapon.parent.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(__instance.hitInfo, aoeHitIndex, aoeRec.Damage, 0f));
        }
      }
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_PlayProjectileAMS {
    public static void Prefix(ref bool __runOriginal, MissileEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
      if (cachedCurve == null) { return; };
      Log.Combat?.WL(0, "Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex);
      Log.Combat?.WL(0, "Altering start/end positions");
      __instance.startPos(cachedCurve.startPosition);
      __instance.preFireEndPos = cachedCurve.startPosition;
      __instance.endPos(cachedCurve.hitPosition);
      Log.Combat?.WL(0, "Intercept " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex + " T:" + cachedCurve.interceptInfo.getAMSShootT() + " " + cachedCurve.interceptInfo.AMSShootIndex + "/" + cachedCurve.interceptInfo.AMSShoots.Count);
      __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.getAMSShootT();
      return;
    }
    public static void Postfix(MissileEffect __instance) {
      AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(__instance.hitIndex);
      if (cachedCurve == null) {
        if (__instance.weapon.isImprovedBallistic() == false) { return; };
        Log.Combat?.W(0, "Altering missile speed (rate) " + __instance.rate() + " -> ");
        __instance.rate(__instance.rate() * __instance.weapon.ProjectileSpeedMultiplier());
        Log.Combat?.WL(1, __instance.rate().ToString());
      } else {
        float distance = cachedCurve.trajectorySpline.Length;
        Log.Combat?.W(0, "Altering missile speed (rate) precached " + __instance.rate() + " -> ");
        __instance.rate(cachedCurve.projectileSpeed / distance);
        Log.Combat?.WL(1, __instance.rate().ToString());
      }
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("SpawnImpactExplosion")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class MissileEffect_SpawnImpactExplosion {
    public static void Prefix(ref bool __runOriginal, MissileEffect __instance, string explosionName) {
      if (!__runOriginal) { return; }
      Vector3 endPos = __instance.endPos();
      Log.Combat?.WL(0, "SpawnImpactExplosion(" + explosionName + ")");
      Log.Combat?.WL(0, "EndPos " + endPos.x + "," + endPos.y + "," + endPos.z);
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("GenerateMissilePath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_GenerateMissilePath {
    public static void Prefix(ref bool __runOriginal, MissileEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
      if (cachedCurve == null) { return; };
      Log.Combat?.WL(0, "Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex);
      Log.Combat?.WL(0, "Altering trajectory");
      CurvySpline spline = __instance.spline;
      spline.Interpolation = CurvyInterpolation.Bezier;
      spline.Clear();
      spline.Closed = false;
      spline.Add(cachedCurve.trajectory);
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("GenerateIndirectMissilePath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_GenerateIndirectMissilePath {
    public static void Prefix(ref bool __runOriginal, MissileEffect __instance) {
      if (!__runOriginal) { return; }
      int hitIndex = __instance.hitIndex;
      AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
      if (cachedCurve == null) { return; };
      Log.Combat?.WL(0, "Cached missile path found " + __instance.weapon.defId + " " + __instance.hitInfo.attackSequenceId + " " + __instance.hitInfo.attackWeaponIndex + " " + hitIndex);
      Log.Combat?.WL(0, "Altering trajectory");
      CurvySpline spline = __instance.spline;
      spline.Interpolation = CurvyInterpolation.Bezier;
      spline.Clear();
      spline.Closed = false;
      spline.Add(cachedCurve.trajectory);
      __runOriginal = false;
      return;
    }
  }
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_Update {
    public static void Postfix(MissileEffect __instance) {
      float t = __instance.t();
      if (__instance.currentState == WeaponEffect.WeaponEffectState.Firing) {
        int hitIndex = __instance.hitIndex;
        if (__instance.hitInfo.dodgeRolls[hitIndex] <= -2.0f) {
          float AMSShootT = (0.0f - __instance.hitInfo.dodgeRolls[hitIndex]) - 2.0f;
          if (t >= AMSShootT) {
            Log.Combat?.WL(0, "Update missile " + __instance.hitInfo.attackWeaponIndex + ":" + hitIndex + " t:" + t + " " + AMSShootT);
            AdvWeaponHitInfoRec cachedCurve = __instance.hitInfo.advRec(hitIndex);
            __instance.hitInfo.dodgeRolls[hitIndex] = 0f;
            if (cachedCurve == null) { return; };
            AMSShoot amsShoot = cachedCurve.interceptInfo.getAMSShoot();
            if (amsShoot == null) {
              __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
              return;
            };
            if (amsShoot.AMS == null) {
              __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
              return;
            };
            Log.Combat?.WL(1, "firing AMS " + amsShoot.AMS.UIName + " sootIdx:" + amsShoot.shootIdx);
            amsShoot.AMS.AMS().Fire(amsShoot.shootIdx);
            cachedCurve.interceptInfo.nextAMSShoot();
            amsShoot = cachedCurve.interceptInfo.getAMSShoot();
            if (amsShoot == null) {
              __instance.hitInfo.dodgeRolls[hitIndex] = cachedCurve.interceptInfo.Intercepted ? -1.0f : 0f;
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
      Log.Combat?.TWL(0, "AttackDirector.AttackSequence.GenerateToHitInfo");
      try {
        WeaponHitInfo?[][] weaponHitInfo = __instance.weaponHitInfo;
        Log.Combat?.WL(0,"Main stray generator", true);
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {

          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.Combat?.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponIndex + " is null. How?!", true);
              continue;
            }
            WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
            WeaponStrayHelper.MainStray(ref hitInfo);
            weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
          }
        }
        __instance.weaponHitInfo = weaponHitInfo;
      } catch (Exception e) {
        Log.Combat?.WL(0,"WARNING! Stray generation FAIL\n" + e.ToString(), true);
      }
      try {
        CustomAmmoCategories.genreateAMSInterceptionInfo(__instance);
      } catch (Exception e) {
        Log.Combat?.WL(0, "WARNING! AMS generation FAIL\n" + e.ToString(), true);
        Weapon.logger.LogException(e);
      }
      try {
        WeaponHitInfo?[][] weaponHitInfo = __instance.weaponHitInfo;
        Log.Combat?.WL(0, "Frag generator", true);
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.Combat?.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
              continue;
            }
            WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
            FragWeaponHelper.FragSeparation(ref hitInfo);
            weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
          }
        }
        __instance.weaponHitInfo = weaponHitInfo;
      } catch (Exception e) {
        Log.Combat?.WL(0, "WARNING! Frag generation FAIL\n" + e.ToString() + "\nFallback", true);
        Weapon.logger.LogException(e);
      }
      try {
        WeaponHitInfo?[][] weaponHitInfo = __instance.weaponHitInfo;
        Log.Combat?.WL(0, "AoE generator", true);
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.Combat?.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
              continue;
            }
            WeaponHitInfo hitInfo = weaponHitInfo[groupIndex][weaponIndex].Value;
            AreaOfEffectHelper.AoEProcessing(ref hitInfo);
            weaponHitInfo[groupIndex][weaponIndex] = hitInfo;
          }
        }
        __instance.weaponHitInfo = weaponHitInfo;
      } catch (Exception e) {
        Log.Combat?.WL(0, "WARNING! AoE generation FAIL\n" + e.ToString(), true);
        Weapon.logger.LogException(e);
      }
      try {
        WeaponHitInfo?[][] weaponHitInfo = __instance.weaponHitInfo; 
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.Combat?.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
              continue;
            }
            AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
            advInfo.FillResolveHitInfo();
          }
        }
      } catch (Exception e) {
        Log.Combat?.WL(0, "WARNING! Hits ordering FAIL\n" + e.ToString(), true);
        Weapon.logger.LogException(e);
      }
      try {
        WeaponHitInfo?[][] weaponHitInfo = __instance.weaponHitInfo; 
        DamageModifiersCache.ClearComulativeDamage();
        HashSet<ICombatant> affectedCombatants = new HashSet<ICombatant>();
        Log.Combat?.TWL(0, "Start damage variance:" + __instance.id);
        for (int groupIndex = 0; groupIndex < weaponHitInfo.Length; ++groupIndex) {
          for (int weaponIndex = 0; weaponIndex < weaponHitInfo[groupIndex].Length; ++weaponIndex) {
            if (weaponHitInfo[groupIndex][weaponIndex].HasValue == false) {
              Log.Combat?.TWL(0, "WeaponHitInfo at grp:" + groupIndex + " index:" + weaponHitInfo + " is null. How?! Again weapon without representation?! You realy should stop it!", true);
              continue;
            }
            AdvWeaponHitInfo advInfo = weaponHitInfo[groupIndex][weaponIndex].Value.advInfo();
            if (advInfo == null) { continue; }
            advInfo.weapon.ClearDamageCache();
            Log.Combat?.TWL(0, "Damage variance grp:" + groupIndex + " index:" + weaponIndex + " wpn:" + advInfo.weapon.defId + " ammo:" + advInfo.weapon.ammo().Id + " mode:" + advInfo.weapon.mode().Id);
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
        Log.Combat?.TWL(0, "Combatants affected by attack:" + affectedCombatants.Count);
        foreach (ICombatant trg in affectedCombatants) {
          Log.Combat?.WL(1, trg.DisplayName + ":" + trg.GUID);
          foreach (var stat in trg.StatCollection) {
            Log.Combat?.WL(2, stat.Key + "=" + stat.Value.CurrentValue.type + ":" + stat.Value.CurrentValue.ToString());
          }
        }
      } catch (Exception e) {
        Log.Combat?.WL(0,"WARNING! Hits ordering FAIL\n" + e.ToString(), true);
        Weapon.logger.LogException(e);
      }
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
      foreach (var group in seqAdvInfo) { foreach (var wpn in group.Value) { advInfos.Add(wpn.Value); } };
      Dictionary<ICombatant, AmsHitsInfo> amsHitsInfo = new Dictionary<ICombatant, AmsHitsInfo>();
      HashSet<Weapon> amsShooted = new HashSet<Weapon>();
      bool isAMSShoots = false;
      foreach (AdvWeaponHitInfo advInfo in advInfos) {
        foreach (AdvWeaponHitInfoRec advRec in advInfo.hits) {
          attacker = advRec.parent.Sequence.attacker;
          if (advRec.interceptInfo.AMSImunne) { continue; }
          if (advRec.interceptInfo.AMSShoots.Count > 0) { isAMSShoots = true; }
          if (amsHitsInfo.ContainsKey(advRec.target) == false) { amsHitsInfo.Add(advRec.target, new AmsHitsInfo()); };
          amsHitsInfo[advRec.target].all += 1;
          if (advRec.interceptInfo.Intercepted) { amsHitsInfo[advRec.target].intercepted += 1; }
          foreach (AMSShoot amsShoot in advRec.interceptInfo.AMSShoots) {
            amsShooted.Add(amsShoot.AMS);
          }
        }
      }
      foreach (Weapon ams in amsShooted) {
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
    public static void Prefix(ref bool __runOriginal, AttackDirector __instance, MessageCenterMessage message) {
      if (!__runOriginal) { return; }
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      int sequenceId = attackCompleteMessage.sequenceId;
      try {
        AttackDirector.AttackSequence attackSequence = __instance.GetAttackSequence(sequenceId);
        if (attackSequence == null) {
          return;
        }
        AdvWeaponHitInfoHelper.ResolveAMSprocessing(sequenceId);
        AdvWeaponHitInfo.Sanitize(sequenceId);
        AdvWeaponHitInfo.FlushInfo(attackSequence);
        AdvWeaponHitInfo.Clear(sequenceId);
        __instance.Combat.HandleSanitize();
      } catch (Exception e) {
        Log.Combat?.WL(0,"WARNING! Advanced info clearing FAIL\n" + e.ToString() + "\nFallback", true);
        AttackDirector.attackLogger.LogException(e);
      }
      return;
    }
  }
}
