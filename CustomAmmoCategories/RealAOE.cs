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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using BattleTech.Rendering;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using IRBTModUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  /*public class SpreadHitInfo {
    public string targetGUID;
    public float dogleDamage;
    public WeaponHitInfo hitInfo;
    public SpreadHitInfo(string GUID, WeaponHitInfo hInfo, float dd) {
      targetGUID = GUID;
      hitInfo = hInfo;
      dogleDamage = dd;
    }
  }
  public class SpreadHitRecord {
    public string targetGUID;
    public WeaponHitInfo hitInfo;
    public int internalIndex;
    public float dogleDamage;
    public SpreadHitRecord(string GUID, WeaponHitInfo hInfo, int intIndex, float dd) {
      targetGUID = GUID;
      hitInfo = hInfo;
      internalIndex = intIndex;
      dogleDamage = dd;
    }

  }
  public class AOEDamageRecord {
    public int hitLocation;
    public float damage;
    public Vector3 hitPosition;
    public AOEDamageRecord(int location, float dmg, Vector3 pos) {
      hitLocation = location;
      damage = dmg;
      hitPosition = pos;
    }
  }
  public class AOEHitInfo {
    public string targetGUID;
    public float heatDamage;
    public float stableDamage;
    public List<AOEDamageRecord> damageList;
    public int RealHitIndex;
    public WeaponHitInfo hitInfo;
    public AOEHitInfo(AttackDirector.AttackSequence instance, ICombatant combatant, AbstractActor attacker, Vector3 attackPos, Weapon weapon, Dictionary<int, float> dmg, float heat, float stbl, int groupIdx, int weaponIdx) {
      RealHitIndex = -1;
      damageList = new List<AOEDamageRecord>();
      hitInfo = new WeaponHitInfo();
      //CustomAmmoCategories.
      targetGUID = combatant.GUID;
      CustomAmmoCategoriesLog.Log.LogWrite("Creating AOE Hit Group " + dmg.Count + "\n");
      hitInfo.attackerId = attacker.GUID;
      hitInfo.targetId = combatant.GUID;
      hitInfo.numberOfShots = dmg.Count;
      hitInfo.stackItemUID = instance.stackItemUID;
      hitInfo.attackSequenceId = instance.id;
      hitInfo.attackGroupIndex = groupIdx;
      hitInfo.attackWeaponIndex = weaponIdx;
      hitInfo.toHitRolls = new float[dmg.Count];
      hitInfo.locationRolls = new float[dmg.Count];
      hitInfo.dodgeRolls = new float[dmg.Count];
      hitInfo.dodgeSuccesses = new bool[dmg.Count];
      hitInfo.hitLocations = new int[dmg.Count];
      hitInfo.hitPositions = new Vector3[dmg.Count];
      hitInfo.hitVariance = new int[dmg.Count];
      hitInfo.hitQualities = new AttackImpactQuality[dmg.Count];
      hitInfo.secondaryTargetIds = new string[dmg.Count];
      hitInfo.secondaryHitLocations = new int[dmg.Count];
      hitInfo.attackDirections = new AttackDirection[dmg.Count];
      heatDamage = heat;
      this.stableDamage = stbl;
      int hitIndex = 0;
      CustomAmmoCategoriesLog.Log.LogWrite(" hitInfo created heatDamage:" + heatDamage + "\n");
      foreach (var dmgrec in dmg) {
        CustomAmmoCategoriesLog.Log.LogWrite("  creating hit record " + hitIndex + "\n");
        int Location = dmgrec.Key;
        string secTarget = string.Empty;
        int secLocation = 0;
        Vector3 hitPosition = combatant.GetImpactPosition(attacker, attackPos, weapon, ref Location, ref hitInfo.attackDirections[hitIndex], ref secTarget, ref secLocation);
        CustomAmmoCategoriesLog.Log.LogWrite("  impact position generated\n");
        damageList.Add(new AOEDamageRecord(Location, dmgrec.Value, hitPosition));
        hitInfo.hitLocations[hitIndex] = Location;
        hitInfo.hitPositions[hitIndex] = hitPosition;
        hitInfo.dodgeRolls[hitIndex] = CustomAmmoCategories.AOEHitIndicator;
        hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(attacker, attackPos, weapon, combatant, MeleeAttackType.NotSet, false);
        hitInfo.secondaryTargetIds[hitIndex] = null;
        hitInfo.secondaryHitLocations[hitIndex] = 0;
        ++hitIndex;
      }
    }
  }*/
  public static partial class CustomAmmoCategories {
    //                   sequenceId       groupId      weaponIndex     hitIndex  Damage
    /*public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>> SpreadCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>>();
    public static SpreadHitRecord getSpreadCache(WeaponHitInfo hitInfo, int hitIndex) {
      if (CustomAmmoCategories.SpreadCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        return null;
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        return null;
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        return null;
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
        return null;
      };
      return CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitIndex];
    }
    public static List<SpreadHitInfo> getSpreadCache(WeaponHitInfo hitInfo) {
      List<SpreadHitInfo> result = new List<SpreadHitInfo>();
      if (CustomAmmoCategories.SpreadCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        return result;
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        return result;
      };
      if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        return result;
      };
      HashSet<string> targets = new HashSet<string>();
      HashSet<string> targetsFrag = new HashSet<string>();
      foreach (var spreadCacheRecord in CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex]) {
        //result.Add(spreadCacheRecord.Value);
        if (spreadCacheRecord.Key < hitInfo.numberOfShots) {
          if (targets.Contains(spreadCacheRecord.Value.targetGUID) == false) {
            targets.Add(spreadCacheRecord.Value.targetGUID);
            result.Add(new SpreadHitInfo(spreadCacheRecord.Value.targetGUID, spreadCacheRecord.Value.hitInfo, spreadCacheRecord.Value.dogleDamage));
          }
        } else {
          if (targetsFrag.Contains(spreadCacheRecord.Value.targetGUID) == false) {
            targetsFrag.Add(spreadCacheRecord.Value.targetGUID);
            result.Add(new SpreadHitInfo(spreadCacheRecord.Value.targetGUID, spreadCacheRecord.Value.hitInfo, spreadCacheRecord.Value.dogleDamage));
          }
        }
      }
      return result;
    }*/
    /*public static List<SpreadHitInfo> prepareSpreadHitInfo(AttackDirector.AttackSequence instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage) {
      CustomAmmoCategoriesLog.Log.LogWrite("prepareSpreadHitInfo\n");
      List<SpreadHitInfo> result = new List<SpreadHitInfo>();
      List<ICombatant> combatants = new List<ICombatant>();
      List<ICombatant> allCombatants = instance.Director.Combat.GetAllCombatants();
      string IFFDef = CustomAmmoCategories.getWeaponIFFTransponderDef(weapon);
      if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
        HashSet<string> combatantsGuids = new HashSet<string>();
        List<AbstractActor> enemies = instance.Director.Combat.GetAllEnemiesOf(instance.attacker);
        foreach (ICombatant combatant in enemies) {
          if (combatantsGuids.Contains(combatant.GUID) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
        foreach (ICombatant combatant in allCombatants) {
          if (combatant.GUID == instance.attacker.GUID) { continue; }
          if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
          if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant, IFFDef) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
      }
      float spreadDistance = CustomAmmoCategories.getWeaponSpreadRange(weapon);
      float spreadRNDMax = spreadDistance;
      List<float> spreadBorders = new List<float>();
      List<ICombatant> spreadCombatants = new List<ICombatant>();
      spreadBorders.Add(spreadRNDMax);
      spreadCombatants.Add(instance.chosenTarget);
      Dictionary<string, int> spreadCounts = new Dictionary<string, int>();
      spreadCounts[instance.chosenTarget.GUID] = 0;
      foreach (ICombatant combatant in combatants) {
        if (combatant.IsDead) { continue; };
        if (combatant.GUID == instance.chosenTarget.GUID) { continue; }
        Vector3 CurrentPosition = combatant.CurrentPosition + Vector3.up * combatant.AoEHeightFix();
        float distance = Vector3.Distance(combatant.CurrentPosition, instance.chosenTarget.CurrentPosition);
        if (distance < spreadDistance) {
          spreadRNDMax += (spreadDistance - distance);
          spreadBorders.Add(spreadRNDMax);
          spreadCombatants.Add(combatant);
          spreadCounts[combatant.GUID] = 0;
        };
      }
      for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
        float roll = Random.Range(0f, spreadRNDMax);
        int combatantIndex = 0;
        for (int targetIndex = 0; targetIndex < spreadBorders.Count; ++targetIndex) {
          if (roll <= spreadBorders[targetIndex]) {
            combatantIndex = targetIndex;
            break;
          }
        }
        spreadCounts[spreadCombatants[combatantIndex].GUID] += 1;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" spread result\n");
      int spreadSumm = 0;
      foreach (var spreadCount in spreadCounts) {
        ICombatant combatant = instance.Director.Combat.FindCombatantByGUID(spreadCount.Key);
        if (combatant == null) { continue; }
        if (spreadCount.Value == 0) { continue; };
        CustomAmmoCategoriesLog.Log.LogWrite(" " + combatant.DisplayName + " " + combatant.GUID + " " + spreadCount.Value + "\n");
        spreadSumm += spreadCount.Value;
        WeaponHitInfo hitInfo = new WeaponHitInfo();
        hitInfo.numberOfShots = spreadCount.Value;
        hitInfo.attackerId = instance.attacker.GUID;
        hitInfo.targetId = combatant.GUID;
        hitInfo.stackItemUID = instance.stackItemUID;
        hitInfo.attackSequenceId = instance.id;
        hitInfo.attackGroupIndex = groupIdx;
        hitInfo.attackWeaponIndex = weaponIdx;
        hitInfo.toHitRolls = new float[hitInfo.numberOfShots];
        hitInfo.locationRolls = new float[hitInfo.numberOfShots];
        hitInfo.dodgeRolls = new float[hitInfo.numberOfShots];
        hitInfo.dodgeSuccesses = new bool[hitInfo.numberOfShots];
        hitInfo.hitLocations = new int[hitInfo.numberOfShots];
        hitInfo.hitPositions = new Vector3[hitInfo.numberOfShots];
        hitInfo.hitVariance = new int[hitInfo.numberOfShots];
        hitInfo.hitQualities = new AttackImpactQuality[hitInfo.numberOfShots];
        hitInfo.secondaryTargetIds = new string[hitInfo.numberOfShots];
        hitInfo.secondaryHitLocations = new int[hitInfo.numberOfShots];
        hitInfo.attackDirections = new AttackDirection[hitInfo.numberOfShots];
        result.Add(new SpreadHitInfo(combatant.GUID, hitInfo, dodgedDamage));
      }
      if (spreadSumm != numberOfShots) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Spreaded count:" + spreadSumm + " not equal numbaer of shots:" + numberOfShots + "\n", true);
      }
      return result;
    }*/

    /*public static bool isHasStray(this ref WeaponHitInfo hitInfo) {
      for (int hitIndex = 0; hitIndex < hitInfo.secondaryTargetIds.Length; ++hitIndex) {
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex])) { continue; }
        if (hitInfo.secondaryTargetIds[hitIndex] != hitInfo.targetId) { return true; }
      }
      return false;
    }
    public static HashSet<string> targetsIds(this ref WeaponHitInfo hitInfo) {
      HashSet<string> result = new HashSet<string>();
      result.Add(hitInfo.targetId);
      for (int hitIndex = 0; hitIndex < hitInfo.secondaryTargetIds.Length; ++hitIndex) {
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex])) { continue; }
        if (result.Contains(hitInfo.secondaryTargetIds[hitIndex]) == false) {
          result.Add(hitInfo.secondaryTargetIds[hitIndex]);
        }
      }
      return result;
    }
    public static bool prepareStrayHitInfo(ref WeaponHitInfo hitInfo, float dodgedDamage) {
      Log.LogWrite("prepareStrayHitInfo\n");
      try {
        Dictionary<string, int> strayHitCounts = new Dictionary<string, int>();
        Dictionary<string, int> strayHitIndexes = new Dictionary<string, int>();
        Dictionary<string, int> strayHitIndexesAdd = new Dictionary<string, int>();
        for (int hitIndex = 0; hitIndex < hitInfo.secondaryTargetIds.Length; ++hitIndex) {
          string targetId = hitInfo.targetId;
          if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) { targetId = hitInfo.secondaryTargetIds[hitIndex]; };
          Log.LogWrite(" hi:" + hitIndex + " loc:" + hitInfo.hitLocations[hitIndex] + " st:" + (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) ? "null" : hitInfo.secondaryTargetIds[hitIndex]) + " secLoc:" + hitInfo.secondaryHitLocations[hitIndex] + "\n");
          if (strayHitCounts.ContainsKey(targetId) == false) { strayHitCounts[targetId] = 1; strayHitIndexes[targetId] = 0; strayHitIndexesAdd[targetId] = 0; } else { ++strayHitCounts[targetId]; };
        }
        Dictionary<string, SpreadHitInfo> strayHitInfos = new Dictionary<string, SpreadHitInfo>();
        foreach (var strayHitCount in strayHitCounts) {
          WeaponHitInfo stayHitInfo = new WeaponHitInfo();
          stayHitInfo.numberOfShots = strayHitCount.Value;
          stayHitInfo.attackerId = hitInfo.attackerId;
          stayHitInfo.targetId = strayHitCount.Key;
          stayHitInfo.stackItemUID = hitInfo.stackItemUID;
          stayHitInfo.attackSequenceId = hitInfo.attackSequenceId;
          stayHitInfo.attackGroupIndex = hitInfo.attackGroupIndex;
          stayHitInfo.attackWeaponIndex = hitInfo.attackWeaponIndex;
          stayHitInfo.toHitRolls = new float[stayHitInfo.numberOfShots];
          stayHitInfo.locationRolls = new float[stayHitInfo.numberOfShots];
          stayHitInfo.dodgeRolls = new float[stayHitInfo.numberOfShots];
          stayHitInfo.dodgeSuccesses = new bool[stayHitInfo.numberOfShots];
          stayHitInfo.hitLocations = new int[stayHitInfo.numberOfShots];
          stayHitInfo.hitPositions = new Vector3[stayHitInfo.numberOfShots];
          stayHitInfo.hitVariance = new int[stayHitInfo.numberOfShots];
          stayHitInfo.hitQualities = new AttackImpactQuality[stayHitInfo.numberOfShots];
          stayHitInfo.secondaryTargetIds = new string[stayHitInfo.numberOfShots];
          stayHitInfo.secondaryHitLocations = new int[stayHitInfo.numberOfShots];
          stayHitInfo.attackDirections = new AttackDirection[stayHitInfo.numberOfShots];
          strayHitInfos.Add(strayHitCount.Key, new SpreadHitInfo(stayHitInfo.targetId, stayHitInfo, dodgedDamage));
        }
        if (CustomAmmoCategories.SpreadCache.ContainsKey(hitInfo.attackSequenceId) == false) {
          CustomAmmoCategories.SpreadCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>());
        };
        if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
          CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, SpreadHitRecord>>());
        };
        if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
          CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, SpreadHitRecord>());
        };
        for (int hitIndex = 0; hitIndex < hitInfo.secondaryTargetIds.Length; ++hitIndex) {
          string targetId = hitInfo.targetId;
          if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) { targetId = hitInfo.secondaryTargetIds[hitIndex]; };
          if (strayHitInfos.ContainsKey(targetId) == false) { continue; }
          if (strayHitIndexes.ContainsKey(targetId) == false) { continue; }
          SpreadHitInfo spreadHitInfo = strayHitInfos[targetId];
          int strayHitIndex = strayHitIndexes[targetId];
          spreadHitInfo.hitInfo.toHitRolls[strayHitIndex] = hitInfo.toHitRolls[hitIndex];
          spreadHitInfo.hitInfo.locationRolls[strayHitIndex] = hitInfo.locationRolls[hitIndex];
          spreadHitInfo.hitInfo.dodgeRolls[strayHitIndex] = hitInfo.dodgeRolls[hitIndex];
          spreadHitInfo.hitInfo.dodgeSuccesses[strayHitIndex] = hitInfo.dodgeSuccesses[hitIndex];
          if (hitInfo.targetId != targetId) {
            spreadHitInfo.hitInfo.hitLocations[strayHitIndex] = hitInfo.secondaryHitLocations[hitIndex];
          } else {
            spreadHitInfo.hitInfo.hitLocations[strayHitIndex] = hitInfo.hitLocations[hitIndex];
          }
          spreadHitInfo.hitInfo.hitPositions[strayHitIndex] = hitInfo.hitPositions[hitIndex];
          spreadHitInfo.hitInfo.hitVariance[strayHitIndex] = hitInfo.hitVariance[hitIndex];
          spreadHitInfo.hitInfo.hitQualities[strayHitIndex] = hitInfo.hitQualities[hitIndex];
          spreadHitInfo.hitInfo.secondaryTargetIds[strayHitIndex] = null;
          spreadHitInfo.hitInfo.secondaryHitLocations[strayHitIndex] = 0;
          spreadHitInfo.hitInfo.attackDirections[strayHitIndex] = hitInfo.attackDirections[hitIndex];
          ++strayHitIndexes[targetId];
        }
        for (int hitIndex = 0; hitIndex < hitInfo.secondaryTargetIds.Length; ++hitIndex) {
          string targetId = hitInfo.targetId;
          if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) { targetId = hitInfo.secondaryTargetIds[hitIndex]; };
          if (strayHitInfos.ContainsKey(targetId) == false) { continue; }
          if (strayHitIndexesAdd.ContainsKey(targetId) == false) { continue; }
          SpreadHitInfo spreadHitInfo = strayHitInfos[targetId];
          int strayHitIndex = strayHitIndexesAdd[targetId];
          hitInfo.secondaryTargetIds[hitIndex] = null;
          hitInfo.secondaryHitLocations[hitIndex] = 0;
          hitInfo.hitLocations[hitIndex] = spreadHitInfo.hitInfo.hitLocations[strayHitIndex];
          if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
            CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, new SpreadHitRecord(spreadHitInfo.targetGUID, spreadHitInfo.hitInfo, strayHitIndex, spreadHitInfo.dogleDamage));
          }
          ++strayHitIndexesAdd[targetId];
        }
        Log.LogWrite("Counts:\n");
        foreach (var i in strayHitCounts) {
          Log.LogWrite(" " + i.Key + ":" + i.Value + "\n");
        }
        Log.LogWrite("Indexes:\n");
        foreach (var i in strayHitIndexes) {
          Log.LogWrite(" " + i.Key + ":" + i.Value + "\n");
        }
        Log.LogWrite("Indexes add:\n");
        foreach (var i in strayHitIndexesAdd) {
          Log.LogWrite(" " + i.Key + ":" + i.Value + "\n");
        }
        Log.LogWrite("Indexes add:\n");
        foreach (var i in strayHitInfos) {
          Log.LogWrite(" " + i.Key + ":" + i.Value.hitInfo.numberOfShots + "\n");
          for (int hitIndex = 0; hitIndex < i.Value.hitInfo.numberOfShots; ++hitIndex) {
            Log.LogWrite("  loc:" + i.Value.hitInfo.hitLocations[hitIndex] + "\n");
          }
        }
        return true;
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
        return false;
      }
    }*/

    /*public static bool ConsolidateSpreadHitInfo(List<SpreadHitInfo> spreadHitInfos, ref WeaponHitInfo hitInfo) {
      CustomAmmoCategoriesLog.Log.LogWrite("Consolidating spread hit info:" + spreadHitInfos.Count + " " + hitInfo.numberOfShots + "\n");
      int hitIndex = 0;
      try {
        if (CustomAmmoCategories.SpreadCache.ContainsKey(hitInfo.attackSequenceId) == false) {
          CustomAmmoCategories.SpreadCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>());
        };
        if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
          CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, SpreadHitRecord>>());
        };
        if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
          CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, SpreadHitRecord>());
        };
        int internalPos = 0;
        bool copySuccess = false;
        do {
          copySuccess = false;
          foreach (SpreadHitInfo spreadHitInfo in spreadHitInfos) {
            CustomAmmoCategoriesLog.Log.LogWrite(" local spread:" + spreadHitInfo.targetGUID + " " + spreadHitInfo.hitInfo.numberOfShots + " internal pos:" + internalPos + "\n");
            if (internalPos >= spreadHitInfo.hitInfo.numberOfShots) { continue; }
            CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex = " + hitIndex + " " + spreadHitInfo.targetGUID + " " + internalPos + " location:" + spreadHitInfo.hitInfo.hitLocations[internalPos] + "\n");
            hitInfo.toHitRolls[hitIndex] = spreadHitInfo.hitInfo.toHitRolls[internalPos];
            hitInfo.locationRolls[hitIndex] = spreadHitInfo.hitInfo.locationRolls[internalPos];
            hitInfo.dodgeRolls[hitIndex] = spreadHitInfo.hitInfo.dodgeRolls[internalPos];
            hitInfo.dodgeSuccesses[hitIndex] = spreadHitInfo.hitInfo.dodgeSuccesses[internalPos];
            hitInfo.hitLocations[hitIndex] = spreadHitInfo.hitInfo.hitLocations[internalPos];
            hitInfo.hitPositions[hitIndex] = spreadHitInfo.hitInfo.hitPositions[internalPos];
            hitInfo.hitVariance[hitIndex] = spreadHitInfo.hitInfo.hitVariance[internalPos];
            hitInfo.hitQualities[hitIndex] = spreadHitInfo.hitInfo.hitQualities[internalPos];
            hitInfo.secondaryTargetIds[hitIndex] = null;
            hitInfo.secondaryHitLocations[hitIndex] = 0;
            hitInfo.attackDirections[hitIndex] = spreadHitInfo.hitInfo.attackDirections[internalPos];
            if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
              CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, new SpreadHitRecord(spreadHitInfo.targetGUID, spreadHitInfo.hitInfo, internalPos, spreadHitInfo.dogleDamage));
            }
            ++hitIndex;
            copySuccess = true;
          }
          ++internalPos;
        } while ((copySuccess == true) && (hitIndex < hitInfo.numberOfShots));
        return true;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("ConsolidateSpreadHitInfo error copyPosition:" + hitIndex + " " + e.ToString() + "\n", true);
        return false;
      }
    }*/
    public static float StrayRange(this Weapon weapon) {
      return (weapon.exDef().SpreadRangeStat(weapon) + weapon.ammo().SpreadRange + weapon.mode().SpreadRange) * weapon.exDef().SpreadRangeMod(weapon);
    }
    public static bool AOECapable(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (mode.AOECapable == TripleBoolean.True) { return true; }
      if (ammo.AOECapable == TripleBoolean.True) { return true; }
      if (extWeapon.AOECapable == TripleBoolean.True) { return true; }
      return false;
    }
    public static string SpesialOfflineIFF = "_IFFOfflne";
    public static string IFFTransponderDef(this Weapon weapon) {
      string result = string.Empty;
      WeaponMode mode = weapon.mode();
      if (string.IsNullOrEmpty(mode.IFFDef) == false) { result = mode.IFFDef; } else {
        ExtAmmunitionDef ammo = weapon.ammo();
        if (string.IsNullOrEmpty(ammo.IFFDef) == false) { result = ammo.IFFDef; } else {
          ExtWeaponDef def = weapon.exDef();
          if (string.IsNullOrEmpty(def.IFFDef) == false) { result = def.IFFDef; };
        }
      }
      if (result == CustomAmmoCategories.SpesialOfflineIFF) { result = string.Empty; };
      return result;
    }
    public static bool isCombatantHaveIFFTransponder(ICombatant combatant, string IFFDefId) {
      AbstractActor actor = combatant as AbstractActor;
      if (actor == null) { return false; };
      foreach (MechComponent component in actor.allComponents) {
        if (component.IsFunctional == false) { continue; }
        if (component.defId == IFFDefId) { return true; }
      }
      return false;
    }
    private static bool HitLocationsInited = false;
    public static Dictionary<int, float> NormMechHitLocations = null;
    public static Dictionary<int, float> SquadHitLocations = null;
    public static Dictionary<int, float> FakeVehicleLocations = null;
    public static Dictionary<int, float> VehicleLocations = null;
    public static Dictionary<int, float> OtherLocations = null;
    public static readonly float AOEHitIndicator = -10f;
    public static void InitHitLocationsAOE() {
      if (HitLocationsInited) { return; }
      HitLocationsInited = true;
      CustomAmmoCategories.NormMechHitLocations = new Dictionary<int, float>();
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.CenterTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.CenterTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftArm] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightArm] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftLeg] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightLeg] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.Head] = 0f;
      CustomAmmoCategories.FakeVehicleLocations = new Dictionary<int, float>();
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.CenterTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.CenterTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftArm] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightArm] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftLeg] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightLeg] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.Head] = 100f;
      CustomAmmoCategories.SquadHitLocations = new Dictionary<int, float>();
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.CenterTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.CenterTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftArm] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightArm] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftLeg] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightLeg] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.Head] = 100f;
      CustomAmmoCategories.VehicleLocations = new Dictionary<int, float>();
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Front] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Rear] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Left] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Right] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Turret] = 80f;
      CustomAmmoCategories.OtherLocations = new Dictionary<int, float>();
      CustomAmmoCategories.OtherLocations[1] = 100f;
    }
    //                   sequenceId       groupId      weaponIndex     hitIndex  Damage
    //public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<AOEHitInfo>>>>> AOEDamageCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<AOEHitInfo>>>>>();
    /*public static void generateAOECache(AttackDirector.AttackSequence instance, ref WeaponHitInfo hitInfo, AbstractActor attacker, Weapon weapon, int groupIdx, int weaponIdx) {
      Dictionary<ICombatant, Dictionary<int, float>> targetsHitCache = new Dictionary<ICombatant, Dictionary<int, float>>();
      Dictionary<ICombatant, float> targetsHeatCache = new Dictionary<ICombatant, float>();
      Dictionary<ICombatant, float> targetsStabCache = new Dictionary<ICombatant, float>();
      float AOERange = CustomAmmoCategories.getWeaponAOERange(weapon);
      CustomAmmoCategoriesLog.Log.LogWrite("AOE generation started " + attacker.DisplayName + " " + weapon.defId + " grp:" + hitInfo.attackGroupIndex + " index:" + hitInfo.attackWeaponIndex + " shots:" + hitInfo.numberOfShots + "\n");
      if (hitInfo.numberOfShots == 0) { return; };
      Vector3? AOEHitPosition = null;
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        CachedMissileCurve cachedCurve = CustomAmmoCategories.getCachedMissileCurve(hitInfo, hitIndex);
        if (cachedCurve != null) {
          if (cachedCurve.Intercepted) {
            CustomAmmoCategoriesLog.Log.LogWrite(" intercepted missiles not generating AOE\n");
            continue;
          }
        }
        Vector3 hitPosition = hitInfo.hitPositions[hitIndex];
        if (AOEHitPosition.HasValue == false) { AOEHitPosition = hitPosition; };
        List<ICombatant> combatants = new List<ICombatant>();
        List<ICombatant> allCombatants = attacker.Combat.GetAllCombatants();
        string IFFDef = CustomAmmoCategories.getWeaponIFFTransponderDef(weapon);
        if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
          HashSet<string> combatantsGuids = new HashSet<string>();
          List<AbstractActor> enemies = attacker.Combat.GetAllEnemiesOf(attacker);
          foreach (ICombatant combatant in enemies) {
            if(combatantsGuids.Contains(combatant.GUID) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
          foreach (ICombatant combatant in allCombatants) {
            if (combatant.GUID == instance.attacker.GUID) { continue; }
            if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
            if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant,IFFDef) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
        }
        foreach (ICombatant target in combatants) {
          if (target.IsDead) { continue; };
          Vector3 CurrentPosition = target.CurrentPosition + Vector3.up * target.AoEHeightFix();
          float distance = Vector3.Distance(CurrentPosition, hitPosition);
          CustomAmmoCategoriesLog.Log.LogWrite(" testing combatant " + target.DisplayName + " " + target.GUID + " " + distance + " " + AOERange + "\n");
          if (distance > AOERange) { continue; }
          if (targetsHitCache.ContainsKey(target) == false) { targetsHitCache.Add(target, new Dictionary<int, float>()); }
          if (targetsHeatCache.ContainsKey(target) == false) { targetsHeatCache.Add(target, 0f); }
          if (targetsStabCache.ContainsKey(target) == false) { targetsStabCache.Add(target, 0f); }
          //Dictionary<int, float> targetHitCache = targetsHitCache[target];
          float DamagePerShot = CustomAmmoCategories.getWeaponAOEDamage(weapon);
          if (DamagePerShot < CustomAmmoCategories.Epsilon) { DamagePerShot = weapon.DamagePerShot; };
          float HeatDamagePerShot = CustomAmmoCategories.getWeaponAOEHeatDamage(weapon);
          if (HeatDamagePerShot < CustomAmmoCategories.Epsilon) { HeatDamagePerShot = weapon.HeatDamagePerShot; };
          float fullDamage = DamagePerShot * (AOERange - distance) / AOERange;
          float heatDamage = HeatDamagePerShot * (AOERange - distance) / AOERange;
          float stabDamage = weapon.AOEInstability() * (AOERange - distance) / AOERange;
          targetsHeatCache[target] += heatDamage;
          targetsStabCache[target] += stabDamage;
          CustomAmmoCategoriesLog.Log.LogWrite(" full damage " + fullDamage + "\n");
          List<int> hitLocations = null;
          Dictionary<int, float> AOELocationDict = null;
          if (target is Mech) {
            hitLocations = attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Mech);
            if (CustomAmmoCategories.MechHitLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.MechHitLocations;
            int HeadIndex = hitLocations.IndexOf((int)ArmorLocation.Head);
            if ((HeadIndex >= 0) && (HeadIndex < hitLocations.Count)) { hitLocations.RemoveAt(HeadIndex); };
          } else
          if (target is Vehicle) {
            hitLocations = attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Vehicle);
            if (CustomAmmoCategories.VehicleLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.VehicleLocations;
          } else {
            hitLocations = new List<int>() { 1 };
            if (CustomAmmoCategories.OtherLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.OtherLocations;
          }
          float fullLocationDamage = 0.0f;
          foreach (int hitLocation in hitLocations) {
            if (AOELocationDict.ContainsKey(hitLocation)) {
              fullLocationDamage += AOELocationDict[hitLocation];
            } else {
              fullLocationDamage += 100f;
            }
          }
          Log.LogWrite(" hitLocations: ");
          foreach (int hitLocation in hitLocations) {
            Log.LogWrite(" "+hitLocation);
          }
          Log.LogWrite("\n");
          Log.LogWrite(" full location damage coeff " + fullLocationDamage + "\n");
          foreach (int hitLocation in hitLocations) {
            float currentDamageCoeff = 100f;
            if (AOELocationDict.ContainsKey(hitLocation)) {
              currentDamageCoeff = AOELocationDict[hitLocation];
            }
            currentDamageCoeff /= fullLocationDamage;
            float CurrentLocationDamage = fullDamage * currentDamageCoeff;
            if (targetsHitCache[target].ContainsKey(hitLocation)) {
              targetsHitCache[target][hitLocation] += CurrentLocationDamage;
            } else {
              targetsHitCache[target][hitLocation] = CurrentLocationDamage;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("  location " + hitLocation + " damage " + targetsHitCache[target][hitLocation] + "\n");
          }
        }
      }
      if (CustomAmmoCategories.AOEDamageCache.ContainsKey(hitInfo.attackSequenceId) == false) {
        CustomAmmoCategories.AOEDamageCache.Add(hitInfo.attackSequenceId, new Dictionary<int, Dictionary<int, Dictionary<int, List<AOEHitInfo>>>>());
      };
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) {
        CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId].Add(hitInfo.attackGroupIndex, new Dictionary<int, Dictionary<int, List<AOEHitInfo>>>());
      };
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) {
        CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].Add(hitInfo.attackWeaponIndex, new Dictionary<int, List<AOEHitInfo>>());
      };
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitInfo.numberOfShots - 1) == false) {
        CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitInfo.numberOfShots - 1, new List<AOEHitInfo>());
      };
      List<AOEHitInfo> targetAOEHitInfo = CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitInfo.numberOfShots - 1];
      CustomAmmoCategoriesLog.Log.LogWrite("AOE generation result\n");
      int AOEHitsCount = hitInfo.numberOfShots;
      foreach (var targetHitCache in targetsHitCache) {
        CustomAmmoCategoriesLog.Log.LogWrite(" target:" + targetHitCache.Key.DisplayName + ":"+targetHitCache.Key.GUID+"\n");
        //ICombatant combatant = attacker.Combat.FindCombatantByGUID(targetHitCache.Key);
        //if (combatant == null) { continue; }
        if (AOEHitPosition.HasValue) {
          float heatDamage = 0f;
          if (targetsHeatCache.ContainsKey(targetHitCache.Key)) { heatDamage = targetsHeatCache[targetHitCache.Key]; };
          float stabDamage = 0f;
          if (targetsStabCache.ContainsKey(targetHitCache.Key)) { stabDamage = targetsStabCache[targetHitCache.Key]; };
          targetAOEHitInfo.Add(new AOEHitInfo(instance, targetHitCache.Key, attacker, AOEHitPosition.Value, weapon, targetHitCache.Value, heatDamage, stabDamage, groupIdx, weaponIdx));
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("No one projectile reaches target. So no AOE.\n");
        }
        foreach (var HitCache in targetHitCache.Value) {
          CustomAmmoCategoriesLog.Log.LogWrite("  Location:" + HitCache.Key + ":" + HitCache.Value + "\n");
        }
        AOEHitsCount += targetAOEHitInfo[targetAOEHitInfo.Count - 1].hitInfo.hitLocations.Length;
      }
      float[] oldtoHitRolls = hitInfo.toHitRolls;
      float[] oldlocationRolls = hitInfo.locationRolls;
      float[] olddodgeRolls = hitInfo.dodgeRolls;
      bool[] olddodgeSuccesses = hitInfo.dodgeSuccesses;
      int[] oldhitLocations = hitInfo.hitLocations;
      Vector3[] oldhitPositions = hitInfo.hitPositions;
      int[] oldhitVariance = hitInfo.hitVariance;
      string[] oldsecondaryTargetIds = new string[AOEHitsCount];
      int[] oldsecondaryHitLocations = new int[AOEHitsCount];
      AttackDirection[] oldattackDirections = new AttackDirection[AOEHitsCount];
      AttackImpactQuality[] oldhitQualities = hitInfo.hitQualities;

      hitInfo.toHitRolls = new float[oldtoHitRolls.Length];
      hitInfo.locationRolls = new float[AOEHitsCount];
      hitInfo.dodgeRolls = new float[AOEHitsCount];
      hitInfo.dodgeSuccesses = new bool[AOEHitsCount];
      hitInfo.hitLocations = new int[AOEHitsCount];
      hitInfo.hitPositions = new Vector3[AOEHitsCount];
      hitInfo.hitVariance = new int[AOEHitsCount];
      hitInfo.hitQualities = new AttackImpactQuality[AOEHitsCount];
      hitInfo.secondaryTargetIds = new string[AOEHitsCount];
      hitInfo.secondaryHitLocations = new int[AOEHitsCount];
      hitInfo.attackDirections = new AttackDirection[AOEHitsCount];
      CustomAmmoCategoriesLog.Log.LogWrite(" new hits count:" + AOEHitsCount + "\n");
      oldtoHitRolls.CopyTo(hitInfo.toHitRolls, 0);
      oldlocationRolls.CopyTo(hitInfo.locationRolls, 0);
      olddodgeRolls.CopyTo(hitInfo.dodgeRolls, 0);
      olddodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, 0);
      oldhitLocations.CopyTo(hitInfo.hitLocations, 0);
      oldhitPositions.CopyTo(hitInfo.hitPositions, 0);
      oldhitVariance.CopyTo(hitInfo.hitVariance, 0);
      oldhitQualities.CopyTo(hitInfo.hitQualities, 0);
      oldsecondaryTargetIds.CopyTo(hitInfo.secondaryTargetIds, 0);
      oldsecondaryHitLocations.CopyTo(hitInfo.secondaryHitLocations, 0);
      oldattackDirections.CopyTo(hitInfo.attackDirections, 0);
      AOEHitsCount = hitInfo.numberOfShots;
      for (int index = 0; index < targetAOEHitInfo.Count; ++index) {
        //foreach (AOEHitInfo AOEInfo in targetAOEHitInfo) {
        targetAOEHitInfo[index].RealHitIndex = AOEHitsCount;
        //AOEInfo.hitInfo.toHitRolls.CopyTo(hitInfo.toHitRolls, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.locationRolls.CopyTo(hitInfo.locationRolls, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.dodgeRolls.CopyTo(hitInfo.dodgeRolls, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.dodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.hitLocations.CopyTo(hitInfo.hitLocations, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.hitPositions.CopyTo(hitInfo.hitPositions, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.hitVariance.CopyTo(hitInfo.hitVariance, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.hitQualities.CopyTo(hitInfo.hitQualities, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.secondaryTargetIds.CopyTo(hitInfo.secondaryTargetIds, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.secondaryHitLocations.CopyTo(hitInfo.secondaryHitLocations, AOEHitsCount);
        targetAOEHitInfo[index].hitInfo.attackDirections.CopyTo(hitInfo.attackDirections, AOEHitsCount);
        AOEHitsCount += targetAOEHitInfo[index].hitInfo.toHitRolls.Length;
      }
    }
    public static List<AOEHitInfo> getAOEHitInfo(WeaponHitInfo hitInfo, int hitIndex) {
      if (CustomAmmoCategories.AOEDamageCache.ContainsKey(hitInfo.attackSequenceId) == false) { return null; }
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return null; }
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return null; }
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) { return null; }
      return CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex][hitIndex];
    }
    public static List<AOEHitInfo> getAOEHitInfo(WeaponHitInfo hitInfo, string targetGUID) {
      if (CustomAmmoCategories.AOEDamageCache.ContainsKey(hitInfo.attackSequenceId) == false) { return null; }
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId].ContainsKey(hitInfo.attackGroupIndex) == false) { return null; }
      if (CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex].ContainsKey(hitInfo.attackWeaponIndex) == false) { return null; }
      Dictionary<int, List<AOEHitInfo>> AOEHitInfos = CustomAmmoCategories.AOEDamageCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex];
      List<AOEHitInfo> result = new List<AOEHitInfo>();
      foreach (var AOEHInfo in AOEHitInfos) {
        foreach (var AOEHRec in AOEHInfo.Value) {
          if (AOEHRec.targetGUID == targetGUID) { result.Add(AOEHRec); };
        }
      }
      return result;
    }
  }*/
  }


  namespace CustomAmmoCategoriesPatches {
    [HarmonyPatch(typeof(MessageCoordinator))]
    [HarmonyPatch("Initialize")]
    [HarmonyPatch(MethodType.Normal)]
    public static class MessageCoordinator_Debug {
      public static void Postfix(MessageCoordinator __instance, WeaponHitInfo?[][] allHitInfo) {
        Log.LogWrite("----------------------EXPECTED MESSAGES---------------------\n");
        List<ExpectedMessage> expectedMessages = (List<ExpectedMessage>)typeof(MessageCoordinator).GetField("expectedMessages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        AttackDirector.AttackSequence attackSequence = (AttackDirector.AttackSequence)typeof(MessageCoordinator).GetField("attackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance); ;
        for (int index1 = 0; index1 < allHitInfo.Length; ++index1) {
          WeaponHitInfo?[] nullableArray = allHitInfo[index1];
          for (int index2 = 0; index2 < nullableArray.Length; ++index2) {
            WeaponHitInfo? nullable = nullableArray[index2];
            Log.LogWrite(string.Format("Initializing Group {0} Weapon {1}\n", (object)index1, (object)index2));
            if (!nullable.HasValue) {
              Log.LogWrite(string.Format("Group {0} Weapon {1} has no value\n", (object)index1, (object)index2));
            } else {
              int[] hitLocations = nullable.Value.hitLocations;
              Log.LogWrite("weapon:"+index1+"-"+index2+" number of shots:"+nullable.Value.numberOfShots+"\n");
              for (int shot = 0; shot < hitLocations.Length; ++shot) {
                AdvWeaponHitInfoRec adv = nullable.Value.advRec(shot);
                Log.LogWrite(" hitIndex = " + shot + " hitLocation = " + hitLocations[shot] + " pos:" + nullable.Value.hitPositions[shot] + " " + (shot >= nullable.Value.numberOfShots) + " dr:" + nullable.Value.dodgeRolls[shot] + " adv:"+(adv==null?"false":"true")+"\n");
                if (adv == null) { continue; };
                Log.LogWrite("  aoe:" +adv.isAOE+ " aoeproc: "+adv.isAOEproc+" loc:"+adv.hitLocation+" trg:"+adv.target.DisplayName+"("+adv.target.GUID+") D/H/S:"+adv.Damage+"/"+adv.Heat+"/"+adv.Stability+"\n");
                Log.LogWrite("  frag: sep:"+adv.fragInfo.separated+" isPallet:"+adv.fragInfo.isFragPallet+" mainIdx:"+adv.fragInfo.fragMainHitIndex+" fragStartIdx:"+adv.fragInfo.fragStartHitIndex+" count:"+adv.fragInfo.fragsCount+"\n");
                /*if (shot == (hitLocations.Length - 1)) {
                  List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(nullable.Value, shot);
                  if (AOEHitsInfo != null) {
                    CustomAmmoCategoriesLog.Log.LogWrite("  AOE Hit info found \n");
                    int AOEHitIndex = hitLocations.Length;
                    for (int aHitGroupIndex = 0; aHitGroupIndex < AOEHitsInfo.Count;++aHitGroupIndex) {
                      for (int aHitIndex = 0; aHitIndex < AOEHitsInfo[aHitGroupIndex].damageList.Count; ++aHitIndex) {
                        CustomAmmoCategoriesLog.Log.LogWrite("   aHitIndex = " + AOEHitIndex + " "+ AOEHitsInfo[aHitGroupIndex].targetGUID + " "+ AOEHitsInfo[aHitGroupIndex].damageList[aHitIndex] + "\n");
                        expectedMessages.Add((ExpectedMessage)new ExpectedImpact(attackSequence, AOEHitsInfo[aHitGroupIndex].hitInfo, aHitIndex));
                        ++AOEHitIndex;
                      }
                    }
                  }
                }*/
              }
            }
          }
        }
        for (int index = 0; index < expectedMessages.Count; ++index) {
          CustomAmmoCategoriesLog.Log.LogWrite(expectedMessages[index].GetDebugString() + "\n");
        }
      }
    }
    public class ImpactAOEState {
      public ICombatant target;
      public WeaponHitInfo hitInfo;
      public ImpactAOEState(ICombatant trg, WeaponHitInfo hInfo) {
        target = trg;
        hitInfo = hInfo;
      }
    }
    [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
    [HarmonyPatch("OnAttackSequenceImpact")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class AttackSequence_OnAttackSequenceImpactAOE {
      [HarmonyPriority(Priority.First)]
      public static bool Prefix(AttackDirector.AttackSequence __instance, ref ImpactAOEState __state, ref MessageCenterMessage message) {
        /*AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
        if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return true; }
        __state = new ImpactAOEState(__instance.chosenTarget, impactMessage.hitInfo);
        SpreadHitRecord spreadCache = CustomAmmoCategories.getSpreadCache(impactMessage.hitInfo, impactMessage.hitIndex);
        if (spreadCache != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("Spread cache found\n");
          ICombatant SpreadTarget = __instance.Director.Combat.FindCombatantByGUID(spreadCache.targetGUID);
          if (SpreadTarget != null) {
            __instance.chosenTarget = SpreadTarget;
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" Altering internal target spread " + spreadCache.targetGUID + " found:" + ((SpreadTarget != null) ? SpreadTarget.DisplayName : "false") + "\n");
          if (impactMessage.hitInfo.secondaryTargetIds[impactMessage.hitIndex] == spreadCache.targetGUID) {
            Log.LogWrite(" assume buildin stray. keep hit position. alter hit location:" + impactMessage.hitInfo.hitLocations[impactMessage.hitIndex] + " -> " + spreadCache.hitInfo.hitLocations[spreadCache.internalIndex] + "\n");
            impactMessage.hitInfo.hitLocations[impactMessage.hitIndex] = spreadCache.hitInfo.hitLocations[spreadCache.internalIndex];
          } else {
            Log.LogWrite(" no buildin stray\n");
            Log.LogWrite("  and position was:" + impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] + "\n");
            impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] = spreadCache.hitInfo.hitPositions[spreadCache.internalIndex];
            CustomAmmoCategoriesLog.Log.LogWrite("  become:" + impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] + "\n");
          }
        } else
        if (impactMessage.hitIndex >= impactMessage.hitInfo.numberOfShots) {
          Log.LogWrite("OnAttackSequenceImpact AOE damage detected");
          /*List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(impactMessage.hitInfo, impactMessage.hitInfo.numberOfShots - 1);
          if (AOEHitsInfo != null) {
            Log.LogWrite(" AOE Hit info found. Searching record...\n");
            AOEHitInfo curAoEHitInfo = null;
            for (int aoeGroup = 0; aoeGroup < AOEHitsInfo.Count; ++aoeGroup) {
              int startHitIndex = AOEHitsInfo[aoeGroup].RealHitIndex;
              int endHitIndex = startHitIndex + AOEHitsInfo[aoeGroup].hitInfo.hitLocations.Length;
              Log.LogWrite("  Group:" + aoeGroup + ":" + AOEHitsInfo[aoeGroup].targetGUID + ":" + startHitIndex + "-" + (endHitIndex - 1) + "\n");
              if ((impactMessage.hitIndex >= startHitIndex) && (impactMessage.hitIndex < endHitIndex)) {
                int localHitIndex = impactMessage.hitIndex - AOEHitsInfo[aoeGroup].RealHitIndex;
                curAoEHitInfo = AOEHitsInfo[aoeGroup];
                Log.LogWrite("   found. AoE location:(" + localHitIndex + ")" + curAoEHitInfo.hitInfo.hitLocations[localHitIndex] + " Cons location:" + impactMessage.hitInfo.hitLocations[impactMessage.hitIndex] + "\n");
                break;
              }
            }
            if (curAoEHitInfo == null) {
              Log.LogWrite("WARNING!AoE record for hitIndex:" + impactMessage.hitIndex + " not found\n", true);
            } else {
              ICombatant AOETarget = __instance.Director.Combat.FindCombatantByGUID(curAoEHitInfo.targetGUID);
              if (AOETarget != null) {
                __instance.chosenTarget = AOETarget;
              }
              Log.LogWrite(" Altering internal target " + curAoEHitInfo.targetGUID + " found:" + ((AOETarget != null) ? AOETarget.DisplayName : "false") + "\n");
            }
          } else {
            Log.LogWrite("WARNING!Can't found AOE record\n", true);
          }
        }*/
        return true;
      }

      [HarmonyPriority(Priority.Last)]
      public static void Postfix(AttackDirector.AttackSequence __instance, ref ImpactAOEState __state, ref MessageCenterMessage message) {
        /*AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
        if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return; }
        if (__state != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("OnAttackSequenceImpact restoring original target and hit info " + __state.target.DisplayName + ":" + __state.target.GUID + "\n");
          __instance.chosenTarget = __state.target;
          //impactMessage.hitInfo = __state.hitInfo;
        }*/
      }
    }
    [HarmonyPatch(typeof(Mech))]
    [HarmonyPatch("DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    public static class Mech_DamageLocation {
      [HarmonyPriority(Priority.First)]
      public static bool Prefix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalArmorDamage, float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
        try {
          ICustomMech custMech = __instance as ICustomMech;
          if (custMech != null) {
            float armor = ((aLoc == ArmorLocation.Invalid) || (aLoc == ArmorLocation.None))?-1f:__instance.ArmorForLocation((int)aLoc);
            float structure = ((aLoc == ArmorLocation.Invalid) || (aLoc == ArmorLocation.None)) ? -1f : __instance.StructureForLocation((int)aLoc);
            string name = aLoc.ToString();
            if ((aLoc != ArmorLocation.Invalid) && (aLoc != ArmorLocation.None)) {
              name = custMech.GetLongArmorLocation(aLoc).ToString();
            }
            Log.M.TWL(0, "DamageLocation " + __instance.PilotableActorDef.ChassisID + " loc:" + name + " dmg:" + totalArmorDamage + " strDmg:" + directStructureDamage + " shot:" + hitIndex+" armor before hit:"+armor+" structure before hit:"+structure);
          }
        }catch(Exception e) {
          Log.M.TWL(0,e.ToString(),true);
        }
        return true;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("DestroyFlimsyObjects")]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_DestroyFlimsyObjects {
      [HarmonyPriority(Priority.First)]
      public static bool Prefix(WeaponEffect __instance) {
        if (!__instance.shotsDestroyFlimsyObjects) {
          return true;
        }
        if (__instance.weapon.AOECapable() == false) { return true; };
        float AOERange = __instance.weapon.AOERange();
        if (AOERange < CustomAmmoCategories.Epsilon) { return true; };
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        foreach (Collider collider in Physics.OverlapSphere(endPos, AOERange, -5, QueryTriggerInteraction.Ignore)) {
          DestructibleObject component = collider.gameObject.GetComponent<DestructibleObject>();
          if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.isFlimsy) {
            Vector3 normalized = (collider.transform.position - endPos).normalized;
            float forceMagnitude = __instance.weapon.DamagePerShot + Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
            component.TakeDamage(endPos, normalized, forceMagnitude);
            component.Collapse(normalized, forceMagnitude);
          }
        }
        return true;
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("PlayImpact")]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_PlayImpactScorch {
      [HarmonyPriority(Priority.First)]
      public static void Postfix(WeaponEffect __instance) {
        if (__instance.weapon.AOECapable() == false) { return; };
        int hitIndex = __instance.HitIndex();
        float AOERange = __instance.weapon.AOERange();
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        float num3 = AOERange;
        FootstepManager.Instance.AddScorch(endPos, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(num3, num3, num3), false);
        return;
      }
    }
  }
}