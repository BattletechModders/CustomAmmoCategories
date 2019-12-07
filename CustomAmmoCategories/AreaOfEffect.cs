using BattleTech;
using CustomAmmoCategoriesLog;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class AreaOfEffectHelper {
    public static WeaponHitInfo generateAoEWeaponHitInfo(ICombatant combatant,AbstractActor attacker, Weapon weapon,Dictionary<int, float> dmgInfo) {
      WeaponHitInfo hitInfo = new WeaponHitInfo();
      hitInfo.attackerId = attacker.GUID;
      hitInfo.targetId = combatant.GUID;
      hitInfo.numberOfShots = dmgInfo.Count;
      hitInfo.stackItemUID = -1;
      hitInfo.attackSequenceId = -1;
      hitInfo.attackGroupIndex = -1;
      hitInfo.attackWeaponIndex = -1;
      hitInfo.toHitRolls = new float[dmgInfo.Count];
      hitInfo.locationRolls = new float[dmgInfo.Count];
      hitInfo.dodgeRolls = new float[dmgInfo.Count];
      hitInfo.dodgeSuccesses = new bool[dmgInfo.Count];
      hitInfo.hitLocations = new int[dmgInfo.Count];
      hitInfo.hitPositions = new Vector3[dmgInfo.Count];
      hitInfo.hitVariance = new int[dmgInfo.Count];
      hitInfo.hitQualities = new AttackImpactQuality[dmgInfo.Count];
      hitInfo.secondaryTargetIds = new string[dmgInfo.Count];
      hitInfo.secondaryHitLocations = new int[dmgInfo.Count];
      hitInfo.attackDirections = new AttackDirection[dmgInfo.Count];
      int hitIndex = 0;
      foreach(var dmg in dmgInfo) {
        hitInfo.toHitRolls[hitIndex] = dmg.Value;
        hitInfo.dodgeRolls[hitIndex] = 0f;
        hitInfo.dodgeSuccesses[hitIndex] = false;
        hitInfo.hitLocations[hitIndex] = dmg.Key;
        int Location = hitInfo.hitLocations[hitIndex];
        string secTarget = string.Empty;
        int secLocation = 0;
        hitInfo.hitPositions[hitIndex] = combatant.GetImpactPosition(attacker, attacker.CurrentPosition, weapon, ref Location, ref hitInfo.attackDirections[hitIndex], ref secTarget, ref secLocation);
        hitInfo.hitVariance[hitIndex] = 0;
        hitInfo.hitQualities[hitIndex] = AttackImpactQuality.Solid;
        hitInfo.secondaryTargetIds[hitIndex] = string.Empty;
        hitInfo.secondaryHitLocations[hitIndex] = 0;
        ++hitIndex;
      }
      return hitInfo;
    }
    public static void AoEProcessing(ref WeaponHitInfo hitInfo) {
      if (hitInfo.isAdvanced() == false) {
        Log.LogWrite(" not advanced\n");
        return;
      }
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      Weapon weapon = advInfo.weapon;
      float AOERange = weapon.AOERange();
      Log.LogWrite("AOE generation started " + advInfo.Sequence.attacker.DisplayName + " " + weapon.defId + " grp:" + hitInfo.attackGroupIndex + " index:" + hitInfo.attackWeaponIndex + " shots:" + advInfo.hits.Count + "\n");
      if (advInfo.hits.Count == 0) { return; };
      if (advInfo.hits.Count != hitInfo.hitLocations.Length) {
        Log.LogWrite("WARNING! advInfo count " + advInfo.hits.Count + " is not equal hitInfo length:" + hitInfo.hitLocations.Length + ". Any processing should be avoided\n", true);
        return;
      }
      bool HasShells = weapon.HasShells();
      //bool DamagePerPallet = weapon.DamagePerPallet();
      float AoEDamage = weapon.AOEDamage();
      if (AoEDamage < CustomAmmoCategories.Epsilon) { AoEDamage = weapon.DamagePerShot; };
      if (AoEDamage < CustomAmmoCategories.Epsilon) { AoEDamage = 1f; };
      float AoEHeat = weapon.AOEHeatDamage();
      float AoEStability = weapon.AOEInstability();
      float FullAoEDamage = AoEDamage * advInfo.hits.Count;
      Dictionary<ICombatant, Dictionary<int, float>> targetsHitCache = new Dictionary<ICombatant, Dictionary<int, float>>();
      Dictionary<ICombatant, float> targetsHeatCache = new Dictionary<ICombatant, float>();
      Dictionary<ICombatant, float> targetsStabCache = new Dictionary<ICombatant, float>();
      for (int hitIndex = 0; hitIndex < advInfo.hits.Count; ++hitIndex) {
        AdvWeaponHitInfoRec advRec = advInfo.hits[hitIndex];
        if (advRec == null) { continue; }
        if (advRec.interceptInfo.Intercepted) {
          Log.LogWrite(" intercepted missiles not generating AOE\n");
          continue;
        }
        if (advRec.fragInfo.separated) {
          Log.LogWrite(" separated frags not generating AOE\n");
          continue;
        }
        Vector3 hitPosition = advRec.hitPosition;
        List<ICombatant> combatants = new List<ICombatant>();
        List<ICombatant> allCombatants = advInfo.Sequence.attacker.Combat.GetAllCombatants();
        string IFFDef = weapon.IFFTransponderDef();
        if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
          HashSet<string> combatantsGuids = new HashSet<string>();
          List<AbstractActor> enemies = advInfo.Sequence.attacker.Combat.GetAllEnemiesOf(advInfo.Sequence.attacker);
          foreach (ICombatant combatant in enemies) {
            if (combatantsGuids.Contains(combatant.GUID) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
          foreach (ICombatant combatant in allCombatants) {
            if (combatant.GUID == advInfo.Sequence.attacker.GUID) { continue; }
            if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
            if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant, IFFDef) == false) {
              combatants.Add(combatant);
              combatantsGuids.Add(combatant.GUID);
            }
          }
        }
        foreach (ICombatant target in combatants) {
          if (target.IsDead) { continue; };
          Vector3 CurrentPosition = target.CurrentPosition + Vector3.up * target.AoEHeightFix();
          float distance = Vector3.Distance(CurrentPosition, hitPosition);
          Log.LogWrite(" testing combatant " + target.DisplayName + " " + target.GUID + " " + distance + " " + AOERange + "\n");
          if (distance > AOERange) { continue; }
          if (targetsHitCache.ContainsKey(target) == false) { targetsHitCache.Add(target, new Dictionary<int, float>()); }
          if (targetsHeatCache.ContainsKey(target) == false) { targetsHeatCache.Add(target, 0f); }
          if (targetsStabCache.ContainsKey(target) == false) { targetsStabCache.Add(target, 0f); }
          //Dictionary<int, float> targetHitCache = targetsHitCache[target];
          float DamagePerShot = AoEDamage;
          float HeatDamagePerShot = AoEHeat;
          if (HeatDamagePerShot < CustomAmmoCategories.Epsilon) { HeatDamagePerShot = weapon.HeatDamagePerShot; };
          float fullDamage = DamagePerShot * (AOERange - distance) / AOERange;
          float heatDamage = HeatDamagePerShot * (AOERange - distance) / AOERange;
          float stabDamage = AoEStability * (AOERange - distance) / AOERange;
          targetsHeatCache[target] += heatDamage;
          targetsStabCache[target] += stabDamage;
          Log.LogWrite(" full damage " + fullDamage + "\n");
          List<int> hitLocations = null;
          Dictionary<int, float> AOELocationDict = null;
          if (target is Mech) {
            hitLocations = advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Mech);
            if (CustomAmmoCategories.MechHitLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.MechHitLocations;
            int HeadIndex = hitLocations.IndexOf((int)ArmorLocation.Head);
            if ((HeadIndex >= 0) && (HeadIndex < hitLocations.Count)) { hitLocations.RemoveAt(HeadIndex); };
          } else
          if (target is Vehicle) {
            hitLocations = advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Vehicle);
            if (CustomAmmoCategories.VehicleLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.VehicleLocations;
          } else {
            hitLocations = new List<int>() { 1 };
            if (CustomAmmoCategories.OtherLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            AOELocationDict = CustomAmmoCategories.OtherLocations;
          }
          float fullLocationDamage = 0.0f;
          HashSet<int> badLocations = new HashSet<int>();
          foreach (int hitLocation in hitLocations) {
            if (AOELocationDict.ContainsKey(hitLocation)) {
              fullLocationDamage += AOELocationDict[hitLocation];
            } else {
              badLocations.Add(hitLocation);
            }
          }
          foreach (int hitLocation in badLocations) {Log.LogWrite(" bad location detected: "+hitLocation+" removing.");hitLocations.Remove(hitLocation);};
          Log.LogWrite(" hitLocations: ");
          foreach (int hitLocation in hitLocations) {
            Log.LogWrite(" " + hitLocation);
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
            Log.LogWrite("  location " + hitLocation + " damage " + targetsHitCache[target][hitLocation] + "\n");
          }
        }
      }
      Log.LogWrite(" consolidated AoE damage:\n");
      foreach (var targetHitCache in targetsHitCache) {
        Log.LogWrite("  "+targetHitCache.Key.DisplayName+":"+targetHitCache.Key.GUID+"\n");
        foreach (var targetHit in targetHitCache.Value) {
          Log.LogWrite("  location:" +targetHit.Key+ ":"+targetHit.Value+"\n");
        }
      }
      int AOEHitsCount = advInfo.hits.Count;
      Dictionary<ICombatant, WeaponHitInfo> AoEHitInfos = new Dictionary<ICombatant, WeaponHitInfo>();
      foreach (var targetHitCache in targetsHitCache) {
        WeaponHitInfo aHitInfo = AreaOfEffectHelper.generateAoEWeaponHitInfo(targetHitCache.Key, advInfo.Sequence.attacker, advInfo.weapon, targetHitCache.Value);
        AoEHitInfos.Add(targetHitCache.Key, aHitInfo);
        AOEHitsCount += aHitInfo.numberOfShots;
      }
      FragWeaponHelper.Resize(ref hitInfo, AOEHitsCount);
      int hIndex = advInfo.hits.Count;
      int aoeStartIndex = advInfo.hits.Count - 1;
      foreach (var aoeHitInfo in AoEHitInfos) {
        for(int hi = 0; hi < aoeHitInfo.Value.numberOfShots; ++hi) {
          AdvWeaponHitInfoRec advRec = new AdvWeaponHitInfoRec(advInfo);
          hitInfo.toHitRolls[hIndex] = 1f;
          hitInfo.locationRolls[hIndex] = 1f;
          hitInfo.dodgeRolls[hIndex] = 1f;
          hitInfo.dodgeSuccesses[hIndex] = false;
          hitInfo.hitPositions[hIndex] = aoeHitInfo.Value.hitPositions[hi];
          hitInfo.hitVariance[hIndex] = aoeHitInfo.Value.hitVariance[hi];
          hitInfo.hitQualities[hIndex] = aoeHitInfo.Value.hitQualities[hi];
          hitInfo.attackDirections[hIndex] = aoeHitInfo.Value.attackDirections[hi];
          int hitLocation = aoeHitInfo.Value.hitLocations[hi];
          if (aoeHitInfo.Key.GUID == hitInfo.targetId) {
            hitInfo.hitLocations[hIndex] = aoeHitInfo.Value.hitLocations[hi];
            hitInfo.secondaryTargetIds[hIndex] = aoeHitInfo.Value.secondaryTargetIds[hi];
            hitInfo.secondaryHitLocations[hIndex] = aoeHitInfo.Value.secondaryHitLocations[hi];
          } else {
            hitInfo.hitLocations[hIndex] = 65536;
            hitInfo.secondaryTargetIds[hIndex] = aoeHitInfo.Key.GUID;
            hitInfo.secondaryHitLocations[hIndex] = aoeHitInfo.Value.hitLocations[hi];
          }
          float Damage = 0f;
          float Heat = 0f;
          float Stability = 0f;
          if (targetsHitCache.ContainsKey(aoeHitInfo.Key)) {
            if (targetsHitCache[aoeHitInfo.Key].ContainsKey(hitLocation)) {
              Damage = targetsHitCache[aoeHitInfo.Key][hitLocation];
            }
          }
          if(hi == 0) {
            if (targetsHeatCache.ContainsKey(aoeHitInfo.Key)) {
              Heat = targetsHeatCache[aoeHitInfo.Key];
            }
            if (targetsHeatCache.ContainsKey(aoeHitInfo.Key)) {
              Stability = targetsStabCache[aoeHitInfo.Key];
            }
          }
          advInfo.AppendAoEHit(aoeStartIndex, FullAoEDamage, Damage, Heat, Stability, aoeHitInfo.Key, aoeHitInfo.Value.hitPositions[hi], hitLocation);
          ++hIndex;
        }
      }
    }
  }
}