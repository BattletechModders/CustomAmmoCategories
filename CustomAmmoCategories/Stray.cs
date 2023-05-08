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
using BattleTech;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class WeaponStrayHelper {
    public static void MainStray(ref WeaponHitInfo hitInfo) {
      if (hitInfo.isAdvanced() == false) { return; };
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      Log.Combat?.WL(0,"MainStray " + advInfo.weapon.defId);
      if ((advInfo.weapon.StrayRange() < CustomAmmoCategories.Epsilon) || (advInfo.weapon.parent.isSpawnProtected())) {
        Log.Combat?.WL(1, "no stray needed");
        return;
      }
      Log.Combat?.WL(1, "stray range:" + advInfo.weapon.StrayRange());
      List<ICombatant> combatants = new List<ICombatant>();
      List<ICombatant> allCombatants = advInfo.Combat.GetAllCombatants();
      string IFFDef = advInfo.weapon.IFFTransponderDef();
      if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
        HashSet<string> combatantsGuids = new HashSet<string>();
        List<AbstractActor> enemies = advInfo.Combat.GetAllEnemiesOf(advInfo.weapon.parent);
        foreach (ICombatant combatant in enemies) {
          if (combatantsGuids.Contains(combatant.GUID) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
        foreach (ICombatant combatant in allCombatants) {
          if (combatant.GUID == advInfo.weapon.parent.GUID) { continue; }
          if (combatantsGuids.Contains(combatant.GUID) == true) { continue; }
          if (CustomAmmoCategories.isCombatantHaveIFFTransponder(combatant, IFFDef) == false) {
            combatants.Add(combatant);
            combatantsGuids.Add(combatant.GUID);
          }
        }
      }
      float spreadDistance = advInfo.weapon.StrayRange();
      float spreadRNDMax = 0f;
      List<float> spreadBorders = new List<float>();
      List<ICombatant> spreadCombatants = new List<ICombatant>();
      Dictionary<string, int> spreadCounts = new Dictionary<string, int>();
      Vector3 shootPosition = advInfo.Sequence.chosenTarget.CurrentPosition;
      if (advInfo.Sequence.chosenTarget.GUID == advInfo.weapon.parent.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(advInfo.Sequence.chosenTarget.GUID);
        if (terrainPos != null) { shootPosition = terrainPos.pos; };
      }
      foreach (ICombatant combatant in combatants) {
        if (combatant.IsDead) { continue; };
        if (combatant.isSpawnProtected()) { continue; }
        Vector3 CurrentPosition = combatant.CurrentPosition + Vector3.up * combatant.FlyingHeight();
        float distance = Vector3.Distance(combatant.CurrentPosition, shootPosition);
        if (distance <= spreadDistance) {
          spreadRNDMax += (spreadDistance - distance);
          spreadBorders.Add(spreadRNDMax);
          spreadCombatants.Add(combatant);
          spreadCounts[combatant.GUID] = 0;
        };
      }
      if (spreadCombatants.Count == 0) {
        Log.Combat?.WL(0, "No combatants in range? Strange. No stray needed.");
        return;
      }
      for (int hitIndex = 0; hitIndex < advInfo.hits.Count; ++hitIndex) {
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
      Log.Combat?.WL(1, "spread result");
      int spreadSumm = 0;
      List<WeaponHitInfo> result = new List<WeaponHitInfo>();
      advInfo.Sequence.ResetUsedRandomCache(hitInfo.attackGroupIndex, hitInfo.attackWeaponIndex);
      foreach (var spreadCount in spreadCounts) {
        ICombatant combatant = advInfo.Combat.FindCombatantByGUID(spreadCount.Key);
        if (combatant == null) { continue; }
        if (spreadCount.Value == 0) { continue; };
        Log.Combat?.WL(1, combatant.DisplayName + " " + combatant.GUID + " " + spreadCount.Value);
        spreadSumm += spreadCount.Value;
        WeaponHitInfo newHitInfo = new WeaponHitInfo();
        newHitInfo.numberOfShots = spreadCount.Value;
        newHitInfo.attackerId = advInfo.weapon.parent.GUID;
        newHitInfo.targetId = combatant.GUID;
        newHitInfo.stackItemUID = advInfo.Sequence.stackItemUID;
        newHitInfo.attackSequenceId = advInfo.Sequence.id;
        newHitInfo.attackGroupIndex = hitInfo.attackGroupIndex;
        newHitInfo.attackWeaponIndex = hitInfo.attackWeaponIndex;
        newHitInfo.toHitRolls = new float[newHitInfo.numberOfShots];
        newHitInfo.locationRolls = new float[newHitInfo.numberOfShots];
        newHitInfo.dodgeRolls = new float[newHitInfo.numberOfShots];
        newHitInfo.dodgeSuccesses = new bool[newHitInfo.numberOfShots];
        newHitInfo.hitLocations = new int[newHitInfo.numberOfShots];
        newHitInfo.hitPositions = new Vector3[newHitInfo.numberOfShots];
        newHitInfo.hitVariance = new int[newHitInfo.numberOfShots];
        newHitInfo.hitQualities = new AttackImpactQuality[newHitInfo.numberOfShots];
        newHitInfo.secondaryTargetIds = new string[newHitInfo.numberOfShots];
        newHitInfo.secondaryHitLocations = new int[newHitInfo.numberOfShots];
        newHitInfo.attackDirections = new AttackDirection[newHitInfo.numberOfShots];
        AttackSequence_GenerateHitInfo.generateWeaponHitInfo(advInfo.Sequence, combatant, advInfo.weapon, newHitInfo.attackGroupIndex, newHitInfo.attackWeaponIndex, newHitInfo.numberOfShots, advInfo.Sequence.indirectFire, 0f, ref newHitInfo, false, false);
        newHitInfo.stackItemUID = 0;
        result.Add(newHitInfo);
      }
      for (int hitIndex = 0; hitIndex < advInfo.hits.Count;) {
        for (int targetIndex = 0; targetIndex < result.Count; ++targetIndex) {
          if (result[targetIndex].stackItemUID >= result[targetIndex].numberOfShots) { continue; }
          hitInfo.toHitRolls[hitIndex] = result[targetIndex].toHitRolls[result[targetIndex].stackItemUID];
          hitInfo.locationRolls[hitIndex] = result[targetIndex].locationRolls[result[targetIndex].stackItemUID];
          hitInfo.dodgeRolls[hitIndex] = result[targetIndex].dodgeRolls[result[targetIndex].stackItemUID];
          hitInfo.dodgeSuccesses[hitIndex] = result[targetIndex].dodgeSuccesses[result[targetIndex].stackItemUID];
          hitInfo.hitPositions[hitIndex] = result[targetIndex].hitPositions[result[targetIndex].stackItemUID];
          hitInfo.hitVariance[hitIndex] = result[targetIndex].hitVariance[result[targetIndex].stackItemUID];
          hitInfo.hitQualities[hitIndex] = result[targetIndex].hitQualities[result[targetIndex].stackItemUID];
          hitInfo.attackDirections[hitIndex] = result[targetIndex].attackDirections[result[targetIndex].stackItemUID];
          advInfo.hits[hitIndex].hitPosition = hitInfo.hitPositions[hitIndex];
          if (result[targetIndex].targetId == hitInfo.targetId) {
            hitInfo.hitLocations[hitIndex] = result[targetIndex].hitLocations[result[targetIndex].stackItemUID];
            hitInfo.secondaryTargetIds[hitIndex] = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
            hitInfo.secondaryHitLocations[hitIndex] = result[targetIndex].secondaryHitLocations[result[targetIndex].stackItemUID];
            if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
              advInfo.hits[hitIndex].hitLocation = hitInfo.secondaryHitLocations[hitIndex];
              advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
              advInfo.hits[hitIndex].isStray = true;
            } else {
              advInfo.hits[hitIndex].hitLocation = hitInfo.hitLocations[hitIndex];
              advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(hitInfo.targetId);
              advInfo.hits[hitIndex].isStray = false;
            }
          } else {
            string secTarget = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
            if (string.IsNullOrEmpty(secTarget) == false) {
              hitInfo.hitLocations[hitIndex] = result[targetIndex].hitLocations[result[targetIndex].stackItemUID];
              hitInfo.secondaryTargetIds[hitIndex] = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
              hitInfo.secondaryHitLocations[hitIndex] = result[targetIndex].secondaryHitLocations[result[targetIndex].stackItemUID];
              advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(secTarget);
              advInfo.hits[hitIndex].isStray = true;
            } else {
              advInfo.hits[hitIndex].isStray = false;
              advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(result[targetIndex].targetId);
              hitInfo.hitLocations[hitIndex] = 65536;
              if (result[targetIndex].DidShotHitChosenTarget(result[targetIndex].stackItemUID)) {
                hitInfo.secondaryTargetIds[hitIndex] = result[targetIndex].targetId;
                hitInfo.secondaryHitLocations[hitIndex] = result[targetIndex].hitLocations[result[targetIndex].stackItemUID];
              } else
              if (result[targetIndex].DidShotHitAnything(result[targetIndex].stackItemUID)) { 
                hitInfo.secondaryTargetIds[hitIndex] = null;
                hitInfo.secondaryHitLocations[hitIndex] = 65536;
              }
            }
            advInfo.hits[hitIndex].hitLocation = hitInfo.secondaryHitLocations[hitIndex];
          }
          advInfo.hits[hitIndex].GenerateTrajectory();
          WeaponHitInfo nHitIfo = result[targetIndex];++nHitIfo.stackItemUID; result[targetIndex] = nHitIfo;
          ++hitIndex;
        }
      }
      Log.Combat?.WL(0, "stray result:");
      for(int hitIndex = 0;hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        Log.Combat?.W(1, hitIndex + " loc: pr " + hitInfo.hitLocations[hitIndex] + " sec " + hitInfo.secondaryHitLocations[hitIndex] + " adv " + advInfo.hits[hitIndex].hitLocation + " trg:");
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex])) {
          Log.Combat?.WL(0, "primary " + advInfo.hits[hitIndex].target.DisplayName + (advInfo.hits[hitIndex].target.GUID == hitInfo.targetId?" ok":" error"));
        } else {
          Log.Combat?.WL(0, "secondary " + advInfo.hits[hitIndex].target.DisplayName + (advInfo.hits[hitIndex].target.GUID == hitInfo.secondaryTargetIds[hitIndex] ? " ok" : " error"));
        }
      }
      if (spreadSumm != advInfo.hits.Count) {
        Log.Combat?.WL(0, "WARNING! Spreaded count:" + spreadSumm + " not equal numbaer of shots:" + advInfo.hits.Count, true);
      }
    }
  }
}