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
using Harmony;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class SpreadHitInfo {
    public string targetGUID;
    public float dogleDamage;
    public WeaponHitInfo hitInfo;
    public SpreadHitInfo(string GUID,WeaponHitInfo hInfo, float dd) {
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
    public List<AOEDamageRecord> damageList;
    public WeaponHitInfo hitInfo;
    public AOEHitInfo(AttackDirector.AttackSequence instance, ICombatant combatant, AbstractActor attacker, Vector3 attackPos, Weapon weapon, Dictionary<int, float> dmg, float heat, int groupIdx, int weaponIdx) {
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
      hitInfo.attackDirection = instance.Director.Combat.HitLocation.GetAttackDirection(attackPos, combatant);
      hitInfo.attackDirectionVector = instance.Director.Combat.HitLocation.GetAttackDirectionVector(attackPos, combatant);
      heatDamage = heat;
      int hitIndex = 0;
      CustomAmmoCategoriesLog.Log.LogWrite(" hitInfo created heatDamage:"+heatDamage+"\n");
      foreach (var dmgrec in dmg) {
        CustomAmmoCategoriesLog.Log.LogWrite("  creating hit record " + hitIndex + "\n");
        int Location = dmgrec.Key;
        Vector3 hitPosition = combatant.GetImpactPosition(attacker, attackPos, weapon, ref Location);
        CustomAmmoCategoriesLog.Log.LogWrite("  impact position generated\n");
        damageList.Add(new AOEDamageRecord(Location, dmgrec.Value, hitPosition));
        hitInfo.hitLocations[hitIndex] = Location;
        hitInfo.hitPositions[hitIndex] = hitPosition;
        hitInfo.dodgeRolls[hitIndex] = -10.0f;
        hitInfo.hitQualities[hitIndex] = instance.Director.Combat.ToHit.GetBlowQuality(attacker, attackPos, weapon, combatant, MeleeAttackType.NotSet, false);
        ++hitIndex;
      }
    }
  }
  public static partial class CustomAmmoCategories {
    //                   sequenceId       groupId      weaponIndex     hitIndex  Damage
    public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>> SpreadCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitRecord>>>>();
    /*public static void registerSpreadCache(AttackDirector.AttackSequence instance, int groupIdx, int weaponIdx, List<SpreadHitInfo> spreadList) {
      if (CustomAmmoCategories.SpreadCache.ContainsKey(instance.id) == false) {
        CustomAmmoCategories.SpreadCache.Add(instance.id, new Dictionary<int, Dictionary<int, Dictionary<int, SpreadHitInfo>>>());
      };
      if (CustomAmmoCategories.SpreadCache[instance.id].ContainsKey(groupIdx) == false) {
        CustomAmmoCategories.SpreadCache[instance.id].Add(groupIdx, new Dictionary<int, Dictionary<int, SpreadHitInfo>>());
      };
      if (CustomAmmoCategories.SpreadCache[instance.id][groupIdx].ContainsKey(weaponIdx) == false) {
        CustomAmmoCategories.SpreadCache[instance.id][groupIdx].Add(weaponIdx, new Dictionary<int, SpreadHitInfo>());
      };
      int hitIndex = 0;
      for(int ListIndex=0;ListIndex < spreadList.Count; ++ListIndex) {
        for (int spHitIndex = 0; spHitIndex < spreadList[ListIndex].hitInfo.numberOfShots; ++spHitIndex) {
          if (CustomAmmoCategories.SpreadCache[instance.id][groupIdx][weaponIdx].ContainsKey(hitIndex) == false) {
            CustomAmmoCategories.SpreadCache[instance.id][groupIdx][weaponIdx].Add(hitIndex, spreadList[ListIndex]);
          }
          ++hitIndex;
        }
      }
    }*/
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
      foreach(var spreadCacheRecord in CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex]) {
        //result.Add(spreadCacheRecord.Value);
        if(targets.Contains(spreadCacheRecord.Value.targetGUID) == false) {
          targets.Add(spreadCacheRecord.Value.targetGUID);
          result.Add(new SpreadHitInfo(spreadCacheRecord.Value.targetGUID, spreadCacheRecord.Value.hitInfo, spreadCacheRecord.Value.dogleDamage));
        }
      }
      return result;
    }
    public static List<SpreadHitInfo> prepareSpreadHitInfo(AttackDirector.AttackSequence instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage) {
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
      spreadCombatants.Add(instance.target);
      Dictionary<string, int> spreadCounts = new Dictionary<string, int>();
      spreadCounts[instance.target.GUID] = 0;
      foreach (ICombatant combatant in combatants) {
        if (combatant.IsDead) { continue; };
        if (combatant.GUID == instance.target.GUID) { continue; }
        float distance = Vector3.Distance(combatant.CurrentPosition,instance.target.CurrentPosition);
        if (distance < spreadDistance) {
          spreadRNDMax += (spreadDistance - distance);
          spreadBorders.Add(spreadRNDMax);
          spreadCombatants.Add(combatant);
          spreadCounts[combatant.GUID] = 0;
        };
      }
      for(int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
        float roll = Random.Range(0f, spreadRNDMax);
        int combatantIndex = 0;
        for(int targetIndex=0;targetIndex < spreadBorders.Count; ++targetIndex) {
          if(roll <= spreadBorders[targetIndex]) {
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
        CustomAmmoCategoriesLog.Log.LogWrite(" "+combatant.DisplayName+" "+combatant.GUID+" "+ spreadCount.Value + "\n");
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
        result.Add(new SpreadHitInfo(combatant.GUID,hitInfo, dodgedDamage));
      }
      if(spreadSumm != numberOfShots) {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! Spreaded count:"+spreadSumm+" not equal numbaer of shots:"+numberOfShots+"\n",true);
      }
      return result;
    }
    public static bool ConsolidateSpreadHitInfo(List<SpreadHitInfo> spreadHitInfos,ref WeaponHitInfo hitInfo) {
      CustomAmmoCategoriesLog.Log.LogWrite("Consolidating spread hit info:"+ spreadHitInfos.Count+ " "+hitInfo.numberOfShots+"\n");
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
            CustomAmmoCategoriesLog.Log.LogWrite(" local spread:" + spreadHitInfo.targetGUID +  " "+ spreadHitInfo.hitInfo.numberOfShots + " internal pos:"+internalPos+"\n");
            if (internalPos >= spreadHitInfo.hitInfo.numberOfShots) { continue; }
            CustomAmmoCategoriesLog.Log.LogWrite(" hitIndex = "+hitIndex+" "+ spreadHitInfo.targetGUID + " "+internalPos+" location:"+ spreadHitInfo.hitInfo.hitLocations[internalPos] + "\n");
            hitInfo.toHitRolls[hitIndex] = spreadHitInfo.hitInfo.toHitRolls[internalPos];
            hitInfo.locationRolls[hitIndex] = spreadHitInfo.hitInfo.locationRolls[internalPos];
            hitInfo.dodgeRolls[hitIndex] = spreadHitInfo.hitInfo.dodgeRolls[internalPos];
            hitInfo.dodgeSuccesses[hitIndex] = spreadHitInfo.hitInfo.dodgeSuccesses[internalPos];
            hitInfo.hitLocations[hitIndex] = spreadHitInfo.hitInfo.hitLocations[internalPos];
            hitInfo.hitPositions[hitIndex] = spreadHitInfo.hitInfo.hitPositions[internalPos];
            hitInfo.hitVariance[hitIndex] = spreadHitInfo.hitInfo.hitVariance[internalPos];
            hitInfo.hitQualities[hitIndex] = spreadHitInfo.hitInfo.hitQualities[internalPos];
            if (CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].ContainsKey(hitIndex) == false) {
              CustomAmmoCategories.SpreadCache[hitInfo.attackSequenceId][hitInfo.attackGroupIndex][hitInfo.attackWeaponIndex].Add(hitIndex, new SpreadHitRecord(spreadHitInfo.targetGUID, spreadHitInfo.hitInfo,internalPos, spreadHitInfo.dogleDamage));
            }
            ++hitIndex;
            copySuccess = true;
          }
          ++internalPos;
        } while ((copySuccess == true)&&(hitIndex < hitInfo.numberOfShots));
        return true;
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("ConsolidateSpreadHitInfo error copyPosition:"+ hitIndex + " "+e.ToString()+"\n",true);
        return false;
      }
    }
    public static float getWeaponSpreadRange(Weapon weapon) {
      float result = 0f;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result += extAmmoDef.SpreadRange;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        result += extWeapon.SpreadRange;
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          result += mode.SpreadRange;
        }
      }
      return result;
    }
    public static bool isWeaponAOECapable(Weapon weapon) {
      bool result = false;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if(extWeapon.AOECapable != TripleBoolean.NotSet) {
        return extWeapon.AOECapable == TripleBoolean.True;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        if (extAmmoDef.AOECapable != TripleBoolean.NotSet) {
          result = (extAmmoDef.AOECapable == TripleBoolean.True);
        }
      }
      return result;
    }
    public static float getWeaponAOERange(Weapon weapon) {
      float result = 0f;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.AOECapable != TripleBoolean.NotSet) {
        return extWeapon.AOERange;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result = extAmmoDef.AOERange;
      }
      return result;
    }
    public static float getWeaponAOEDamage(Weapon weapon) {
      float result = 0f;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.AOECapable != TripleBoolean.NotSet) {
        return extWeapon.AOEDamage;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result = extAmmoDef.AOEDamage;
      }
      return result;
    }

    public static float getWeaponAOEHeatDamage(Weapon weapon) {
      float result = 0f;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.AOECapable != TripleBoolean.NotSet) {
        return extWeapon.AOEHeatDamage;
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result = extAmmoDef.AOEHeatDamage;
      }
      return result;
    }

    public static string SpesialOfflineIFF = "_IFFOfflne";
    public static string getWeaponIFFTransponderDef(Weapon weapon) {
      string result = "";
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        result = extWeapon.IFFDef;
      }
      if (string.IsNullOrEmpty(result)) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
          string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
          if (extWeapon.Modes.ContainsKey(modeId)) {
            WeaponMode mode = extWeapon.Modes[modeId];
            result = mode.IFFDef;
          }
        }
      }
      if (string.IsNullOrEmpty(result)) {
        result = extWeapon.IFFDef;
      }
      if (result == CustomAmmoCategories.SpesialOfflineIFF) { result = ""; };
      return result;
    }

    public static bool isCombatantHaveIFFTransponder(ICombatant combatant, string IFFDefId) {
      AbstractActor actor = combatant as AbstractActor;
      if (actor == null) { return false; };
      foreach(MechComponent component in actor.allComponents) {
        if (component.IsFunctional == false) { continue; }
        if (component.defId == IFFDefId) { return true; }
      }
      return false;
    }

    public static Dictionary<int, float> MechHitLocations = null;
    public static Dictionary<int, float> VehicleLocations = null;
    public static Dictionary<int, float> OtherLocations = null;
    public static void InitHitLocationsAOE() {
      CustomAmmoCategories.MechHitLocations = new Dictionary<int, float>();
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.CenterTorso] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.CenterTorsoRear] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.LeftTorso] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.LeftTorsoRear] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.RightTorso] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.RightTorsoRear] = 100f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.LeftArm] = 50f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.RightArm] = 50f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.LeftLeg] = 50f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.RightLeg] = 50f;
      CustomAmmoCategories.MechHitLocations[(int)ArmorLocation.Head] = 0f;
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
    public static Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<AOEHitInfo>>>>> AOEDamageCache = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<AOEHitInfo>>>>>();
    public static void generateAOECache(AttackDirector.AttackSequence instance, ref WeaponHitInfo hitInfo, AbstractActor attacker, Weapon weapon, int groupIdx, int weaponIdx) {
      Dictionary<string, Dictionary<int, float>> targetsHitCache = new Dictionary<string, Dictionary<int, float>>();
      Dictionary<string, float> targetsHeatCache = new Dictionary<string, float>();
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
          float distance = Vector3.Distance(target.CurrentPosition, hitPosition);
          CustomAmmoCategoriesLog.Log.LogWrite(" testing combatant " + target.DisplayName + " " + target.GUID + " " + distance + " " + AOERange + "\n");
          if (distance > AOERange) { continue; }
          if (targetsHitCache.ContainsKey(target.GUID) == false) { targetsHitCache.Add(target.GUID, new Dictionary<int, float>()); }
          if (targetsHeatCache.ContainsKey(target.GUID) == false) { targetsHeatCache.Add(target.GUID, 0f); }
          Dictionary<int, float> targetHitCache = targetsHitCache[target.GUID];
          float DamagePerShot = CustomAmmoCategories.getWeaponAOEDamage(weapon);
          if (DamagePerShot < CustomAmmoCategories.Epsilon) { DamagePerShot = weapon.DamagePerShot; };
          float HeatDamagePerShot = CustomAmmoCategories.getWeaponAOEHeatDamage(weapon);
          if (HeatDamagePerShot < CustomAmmoCategories.Epsilon) { HeatDamagePerShot = weapon.HeatDamagePerShot; };
          float fullDamage = DamagePerShot * (AOERange - distance) / AOERange;
          float heatDamage = HeatDamagePerShot * (AOERange - distance) / AOERange;
          targetsHeatCache[target.GUID] += heatDamage;
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
          CustomAmmoCategoriesLog.Log.LogWrite(" full location damage coeff " + fullLocationDamage + "\n");
          foreach (int hitLocation in hitLocations) {
            float currentDamageCoeff = 100f;
            if (AOELocationDict.ContainsKey(hitLocation)) {
              currentDamageCoeff = AOELocationDict[hitLocation];
            }
            currentDamageCoeff /= fullLocationDamage;
            float CurrentLocationDamage = fullDamage * currentDamageCoeff;
            if (targetHitCache.ContainsKey(hitLocation)) {
              targetHitCache[hitLocation] += CurrentLocationDamage;
            } else {
              targetHitCache[hitLocation] = CurrentLocationDamage;
            }
            CustomAmmoCategoriesLog.Log.LogWrite("  location " + hitLocation + " damage " + targetHitCache[hitLocation] + "\n");
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
        CustomAmmoCategoriesLog.Log.LogWrite(" target:" + targetHitCache.Key + "\n");
        ICombatant combatant = attacker.Combat.FindCombatantByGUID(targetHitCache.Key);
        if (combatant == null) { continue; }
        if (AOEHitPosition.HasValue) {
          float heatDamage = 0f;
          if (targetsHeatCache.ContainsKey(combatant.GUID)) { heatDamage = targetsHeatCache[combatant.GUID]; };
          targetAOEHitInfo.Add(new AOEHitInfo(instance, combatant, attacker, AOEHitPosition.Value, weapon, targetHitCache.Value, heatDamage, groupIdx, weaponIdx));
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("No one projectile reaches target. So no AOE.\n");
        }
        foreach (var HitCache in targetHitCache.Value) {
          CustomAmmoCategoriesLog.Log.LogWrite(" Location:" + HitCache.Key + ":" + HitCache.Value + "\n");
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
      AttackImpactQuality[] oldhitQualities = hitInfo.hitQualities;

      hitInfo.toHitRolls = new float[oldtoHitRolls.Length];
      hitInfo.locationRolls = new float[AOEHitsCount];
      hitInfo.dodgeRolls = new float[AOEHitsCount];
      hitInfo.dodgeSuccesses = new bool[AOEHitsCount];
      hitInfo.hitLocations = new int[AOEHitsCount];
      hitInfo.hitPositions = new Vector3[AOEHitsCount];
      hitInfo.hitVariance = new int[AOEHitsCount];
      hitInfo.hitQualities = new AttackImpactQuality[AOEHitsCount];
      CustomAmmoCategoriesLog.Log.LogWrite(" new hits count:" + AOEHitsCount + "\n");
      oldtoHitRolls.CopyTo(hitInfo.toHitRolls, 0);
      oldlocationRolls.CopyTo(hitInfo.locationRolls, 0);
      olddodgeRolls.CopyTo(hitInfo.dodgeRolls, 0);
      olddodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, 0);
      oldhitLocations.CopyTo(hitInfo.hitLocations, 0);
      oldhitPositions.CopyTo(hitInfo.hitPositions, 0);
      oldhitVariance.CopyTo(hitInfo.hitVariance, 0);
      oldhitQualities.CopyTo(hitInfo.hitQualities, 0);
      AOEHitsCount = hitInfo.numberOfShots;
      foreach (AOEHitInfo AOEInfo in targetAOEHitInfo) {
        //AOEInfo.hitInfo.toHitRolls.CopyTo(hitInfo.toHitRolls, AOEHitsCount);
        AOEInfo.hitInfo.locationRolls.CopyTo(hitInfo.locationRolls, AOEHitsCount);
        AOEInfo.hitInfo.dodgeRolls.CopyTo(hitInfo.dodgeRolls, AOEHitsCount);
        AOEInfo.hitInfo.dodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, AOEHitsCount);
        AOEInfo.hitInfo.hitLocations.CopyTo(hitInfo.hitLocations, AOEHitsCount);
        AOEInfo.hitInfo.hitPositions.CopyTo(hitInfo.hitPositions, AOEHitsCount);
        AOEInfo.hitInfo.hitVariance.CopyTo(hitInfo.hitVariance, AOEHitsCount);
        AOEInfo.hitInfo.hitQualities.CopyTo(hitInfo.hitQualities, AOEHitsCount);
        AOEHitsCount += AOEInfo.hitInfo.toHitRolls.Length;
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
  }
}


namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MessageCoordinator))]
  [HarmonyPatch("Initialize")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MessageCoordinator_Debug {
    public static void Postfix(MessageCoordinator __instance, WeaponHitInfo?[][] allHitInfo) {
      CustomAmmoCategoriesLog.Log.LogWrite("----------------------EXPECTED MESSAGES---------------------\n");
      List<ExpectedMessage> expectedMessages = (List<ExpectedMessage>)typeof(MessageCoordinator).GetField("expectedMessages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      AttackDirector.AttackSequence attackSequence = (AttackDirector.AttackSequence)typeof(MessageCoordinator).GetField("attackSequence", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance); ;
      for (int index1 = 0; index1 < allHitInfo.Length; ++index1) {
        WeaponHitInfo?[] nullableArray = allHitInfo[index1];
        for (int index2 = 0; index2 < nullableArray.Length; ++index2) {
          WeaponHitInfo? nullable = nullableArray[index2];
          CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Initializing Group {0} Weapon {1}\n", (object)index1, (object)index2));
          if (!nullable.HasValue) {
            CustomAmmoCategoriesLog.Log.LogWrite(string.Format("Group {0} Weapon {1} has no value\n", (object)index1, (object)index2));
          } else {
            int[] hitLocations = nullable.Value.hitLocations;
            for (int shot = 0; shot < hitLocations.Length; ++shot) {
              CustomAmmoCategoriesLog.Log.LogWrite("  hitIndex = " + shot + " hitLocation = " + hitLocations[shot] + " AOE:" + (shot >= nullable.Value.numberOfShots) + "\n");
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
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return true; }
      __state = new ImpactAOEState(__instance.target, impactMessage.hitInfo);
      SpreadHitRecord spreadCache = CustomAmmoCategories.getSpreadCache(impactMessage.hitInfo, impactMessage.hitIndex);
      if(spreadCache != null) {
        CustomAmmoCategoriesLog.Log.LogWrite("Spread cache found\n");
        ICombatant SpreadTarget = __instance.Director.Combat.FindCombatantByGUID(spreadCache.targetGUID);
        if (SpreadTarget != null) {
          __instance.target = SpreadTarget;
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" Altering internal target "+ spreadCache.targetGUID + " found:" + (SpreadTarget != null)+"\n");
        CustomAmmoCategoriesLog.Log.LogWrite("  and position was:"+ impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] + "\n");
        impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] = spreadCache.hitInfo.hitPositions[spreadCache.internalIndex];
        CustomAmmoCategoriesLog.Log.LogWrite("  become:" + impactMessage.hitInfo.hitPositions[impactMessage.hitIndex] + "\n");
      } else
      if (impactMessage.hitIndex >= impactMessage.hitInfo.numberOfShots) {
        CustomAmmoCategoriesLog.Log.LogWrite("OnAttackSequenceImpact AOE damage detected");
        List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(impactMessage.hitInfo, impactMessage.hitInfo.numberOfShots - 1);
        if (AOEHitsInfo != null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" AOE Hit info found\n");
          int AOEHitIndex = impactMessage.hitIndex - impactMessage.hitInfo.numberOfShots;
          int AOEGroupIndex = 0;
          while (AOEHitIndex >= AOEHitsInfo[AOEGroupIndex].damageList.Count) {
            ++AOEGroupIndex;
            AOEHitIndex -= AOEHitsInfo[AOEGroupIndex].damageList.Count;
          }
          ICombatant AOETarget = __instance.Director.Combat.FindCombatantByGUID(AOEHitsInfo[AOEGroupIndex].targetGUID);
          if (AOETarget != null) {
            __instance.target = AOETarget;
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" Altering internal target " + AOEHitsInfo[AOEGroupIndex].targetGUID + " found:" + (AOETarget != null) + "\n");
          //impactMessage.hitIndex = AOEHitIndex;
          //impactMessage.hitInfo = AOEHitsInfo[AOEGroupIndex].hitInfo;
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("WARNING!Can't found AOE record\n", true);
        }
      }
      return true;
    }

    [HarmonyPriority(Priority.Last)]
    public static void Postfix(AttackDirector.AttackSequence __instance, ref ImpactAOEState __state, ref MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != __instance.id) { return; }
      if (__state != null) {
        CustomAmmoCategoriesLog.Log.LogWrite("OnAttackSequenceImpact restoring original target and hit info\n");
        __instance.target = __state.target;
        //impactMessage.hitInfo = __state.hitInfo;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("DamageLocation")]
  [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
  public static class Mech_DamageLocation {
    [HarmonyPriority(Priority.First)]
    public static bool Prefix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      CustomAmmoCategoriesLog.Log.LogWrite("DamageLocation " + __instance.DisplayName + " " + __instance.GUID + " loc:" + aLoc.ToString() + " dmg:" + totalDamage + " shot:" + hitIndex + "\n");
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
      if (CustomAmmoCategories.isWeaponAOECapable(__instance.weapon) == false) { return true; };
      float AOERange = CustomAmmoCategories.getWeaponAOERange(__instance.weapon);
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
      if (CustomAmmoCategories.isWeaponAOECapable(__instance.weapon) == false) { return; };
      int hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
      float AOERange = CustomAmmoCategories.getWeaponAOERange(__instance.weapon);
      Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      CombatGameState Combat = (CombatGameState)typeof(WeaponEffect).GetField("Combat", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      float num3 = AOERange;
      FootstepManager.Instance.AddScorch(endPos, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(num3, num3, num3), false);
      return;
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedAoE {
    public static bool Prefix(CombatGameState __instance) {
      if(CustomAmmoCategories.SpreadCache != null) CustomAmmoCategories.SpreadCache.Clear();
      if (CustomAmmoCategories.AOEDamageCache != null) CustomAmmoCategories.AOEDamageCache.Clear();
      if (CustomAmmoCategories.MissileCurveCache != null) CustomAmmoCategories.MissileCurveCache.Clear();
      return true;
    }
  }
}