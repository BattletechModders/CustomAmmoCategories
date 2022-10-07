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

namespace CustAmmoCategoriesPatches {

}

namespace CustAmmoCategories {
  public static class FragWeaponHelper {
    public static bool strayFrags(ref WeaponHitInfo hitInfo,AttackDirector.AttackSequence sequence,Weapon weapon,ICombatant target) {
      float spreadDistance = weapon.ShellsRadius();
      Log.LogWrite("FragWeaponHelper.strayFrags:" + spreadDistance + "\n");
      if (spreadDistance < CustomAmmoCategories.Epsilon) { return false; }
      //List<ICombatant> combatants = new List<ICombatant>();
      List<ICombatant> combatants = weapon.parent.Combat.GetAllCombatants();
      //string IFFDef = CustomAmmoCategories.getWeaponIFFTransponderDef(advInfo.weapon);
      /*if (string.IsNullOrEmpty(IFFDef)) { combatants.AddRange(allCombatants); } else {
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
      }*/
      float spreadRNDMax = 0f;
      List<float> spreadBorders = new List<float>();
      List<ICombatant> spreadCombatants = new List<ICombatant>();
      Dictionary<string, int> spreadCounts = new Dictionary<string, int>();
      Vector3 shootPosition = target.CurrentPosition;
      if (weapon.parent.GUID == target.GUID) {
        TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(weapon.parent.GUID);
        if (terrainPos != null) { shootPosition = terrainPos.pos; };
      }
      foreach (ICombatant combatant in combatants) {
        if (combatant.IsDead) { continue; };
        if (combatant.isDropshipNotLanded()) { continue; };
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
        Log.LogWrite("No combatants in range? Strange. No stray needed.\n");
        return false;
      }
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
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
      Log.LogWrite(" spread result\n");
      int spreadSumm = 0;
      List<WeaponHitInfo> result = new List<WeaponHitInfo>();
      foreach (var spreadCount in spreadCounts) {
        ICombatant combatant = target.Combat.FindCombatantByGUID(spreadCount.Key);
        if (combatant == null) { continue; }
        if (spreadCount.Value == 0) { continue; };
        Log.LogWrite(" " + combatant.DisplayName + " " + combatant.GUID + " " + spreadCount.Value + "\n");
        spreadSumm += spreadCount.Value;
        WeaponHitInfo newHitInfo = new WeaponHitInfo();
        newHitInfo.numberOfShots = spreadCount.Value;
        newHitInfo.attackerId = weapon.parent.GUID;
        newHitInfo.targetId = combatant.GUID;
        newHitInfo.stackItemUID = sequence.stackItemUID;
        newHitInfo.attackSequenceId = sequence.id;
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
        AttackSequence_GenerateHitInfo.generateWeaponHitInfo(sequence, combatant, weapon, newHitInfo.attackGroupIndex, newHitInfo.attackWeaponIndex, newHitInfo.numberOfShots, false, 0f, ref newHitInfo, true, true);
        newHitInfo.stackItemUID = 0;
        result.Add(newHitInfo);
      }
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots;) {
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
          //advInfo.hits[hitIndex].hitPosition = hitInfo.hitPositions[hitIndex];
          if (result[targetIndex].targetId == hitInfo.targetId) {
            hitInfo.hitLocations[hitIndex] = result[targetIndex].hitLocations[result[targetIndex].stackItemUID];
            hitInfo.secondaryTargetIds[hitIndex] = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
            hitInfo.secondaryHitLocations[hitIndex] = result[targetIndex].secondaryHitLocations[result[targetIndex].stackItemUID];
            //if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex]) == false) {
            //  advInfo.hits[hitIndex].hitLocation = hitInfo.secondaryHitLocations[hitIndex];
            //  advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(hitInfo.secondaryTargetIds[hitIndex]);
            //} else {
            //  advInfo.hits[hitIndex].hitLocation = hitInfo.hitLocations[hitIndex];
            //  advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(hitInfo.targetId);
            //}
          } else {
            string secTarget = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
            if (string.IsNullOrEmpty(secTarget) == false) {
              hitInfo.hitLocations[hitIndex] = result[targetIndex].hitLocations[result[targetIndex].stackItemUID];
              hitInfo.secondaryTargetIds[hitIndex] = result[targetIndex].secondaryTargetIds[result[targetIndex].stackItemUID];
              hitInfo.secondaryHitLocations[hitIndex] = result[targetIndex].secondaryHitLocations[result[targetIndex].stackItemUID];
              //advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(secTarget);
            } else {
              //advInfo.hits[hitIndex].target = advInfo.Combat.FindCombatantByGUID(result[targetIndex].targetId);
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
            //advInfo.hits[hitIndex].hitLocation = hitInfo.secondaryHitLocations[hitIndex];
          }
          //advInfo.hits[hitIndex].GenerateTrajectory();
          WeaponHitInfo nHitIfo = result[targetIndex]; ++nHitIfo.stackItemUID; result[targetIndex] = nHitIfo;
          ++hitIndex;
        }
      }
      Log.LogWrite("stray result:\n");
      for (int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        Log.LogWrite(" " + hitIndex + " loc: pr " + hitInfo.hitLocations[hitIndex] + " sec " + hitInfo.secondaryHitLocations[hitIndex] + " trg:");
        if (string.IsNullOrEmpty(hitInfo.secondaryTargetIds[hitIndex])) {
          Log.LogWrite("primary " + hitInfo.targetId + "\n");
        } else {
          Log.LogWrite("secondary " + hitInfo.secondaryTargetIds[hitIndex] + "\n");
        }
      }
      if (spreadSumm != hitInfo.numberOfShots) {
        Log.LogWrite("WARNING! Spreaded count:" + spreadSumm + " not equal number of shots:" + hitInfo.numberOfShots + "\n", true);
      }
      return true;
    }
    public static float[] Resize(float[] old,int newSize) {float[] result = new float[newSize];old.CopyTo(result, 0);return result;}
    public static int[] Resize(int[] old, int newSize) { int[] result = new int[newSize]; old.CopyTo(result, 0); return result; }
    public static bool[] Resize(bool[] old, int newSize) { bool[] result = new bool[newSize]; old.CopyTo(result, 0); return result; }
    public static Vector3[] Resize(Vector3[] old, int newSize) { Vector3[] result = new Vector3[newSize]; old.CopyTo(result, 0); return result; }
    public static AttackImpactQuality[] Resize(AttackImpactQuality[] old, int newSize) { AttackImpactQuality[] result = new AttackImpactQuality[newSize]; old.CopyTo(result, 0); return result; }
    public static string[] Resize(string[] old, int newSize) { string[] result = new string[newSize]; old.CopyTo(result, 0); return result; }
    public static AttackDirection[] Resize(AttackDirection[] old, int newSize) { AttackDirection[] result = new AttackDirection[newSize]; old.CopyTo(result, 0); return result; }
    public static void Resize(ref WeaponHitInfo hitInfo,int newSize) {
      //hitInfo.numberOfShots = newSize;
      hitInfo.toHitRolls = FragWeaponHelper.Resize(hitInfo.toHitRolls, newSize);
      hitInfo.locationRolls = FragWeaponHelper.Resize(hitInfo.locationRolls, newSize);
      hitInfo.dodgeRolls = FragWeaponHelper.Resize(hitInfo.dodgeRolls, newSize);
      hitInfo.dodgeSuccesses = FragWeaponHelper.Resize(hitInfo.dodgeSuccesses, newSize);
      hitInfo.hitLocations = FragWeaponHelper.Resize(hitInfo.hitLocations, newSize);
      hitInfo.hitPositions = FragWeaponHelper.Resize(hitInfo.hitPositions, newSize);
      hitInfo.hitVariance = FragWeaponHelper.Resize(hitInfo.hitVariance, newSize);
      hitInfo.hitQualities = FragWeaponHelper.Resize(hitInfo.hitQualities, newSize);
      hitInfo.secondaryTargetIds = FragWeaponHelper.Resize(hitInfo.secondaryTargetIds, newSize);
      hitInfo.secondaryHitLocations = FragWeaponHelper.Resize(hitInfo.secondaryHitLocations, newSize);
      hitInfo.attackDirections = FragWeaponHelper.Resize(hitInfo.attackDirections, newSize);
    }
    public static void Append(ref WeaponHitInfo hitInfo,int index, WeaponHitInfo nHitInfo) {
      //if (hitInfo.numberOfShots <= (index + nHitInfo.numberOfShots)) { return; }
      nHitInfo.toHitRolls.CopyTo(hitInfo.toHitRolls, index);
      nHitInfo.locationRolls.CopyTo(hitInfo.locationRolls, index);
      nHitInfo.dodgeRolls.CopyTo(hitInfo.dodgeRolls, index);
      nHitInfo.dodgeSuccesses.CopyTo(hitInfo.dodgeSuccesses, index);
      nHitInfo.hitLocations.CopyTo(hitInfo.hitLocations, index);
      nHitInfo.hitPositions.CopyTo(hitInfo.hitPositions, index);
      nHitInfo.hitVariance.CopyTo(hitInfo.hitVariance, index);
      nHitInfo.hitQualities.CopyTo(hitInfo.hitQualities, index);
      nHitInfo.secondaryTargetIds.CopyTo(hitInfo.secondaryTargetIds, index);
      nHitInfo.secondaryHitLocations.CopyTo(hitInfo.secondaryHitLocations, index);
      nHitInfo.attackDirections.CopyTo(hitInfo.attackDirections, index);
    }
    public static WeaponHitInfo prepareFragHitInfo(AdvWeaponHitInfoRec advRec,ICombatant target, Weapon weapon,int numberOfShots,int groupIdx, int weaponIdx) {
      Log.LogWrite("prepareFragHitInfo "+weapon.defId+" "+target.DisplayName+":" +target.GUID+" "+numberOfShots+"\n");
      WeaponHitInfo hitInfo = new WeaponHitInfo();
      hitInfo.attackerId = advRec.parent.Sequence.attacker.GUID;
      hitInfo.targetId = target.GUID;
      hitInfo.numberOfShots = numberOfShots;
      hitInfo.stackItemUID = advRec.parent.Sequence.stackItemUID;
      hitInfo.attackSequenceId = advRec.parent.Sequence.id;
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
      int randomUsed = 0;
      int varianceUsed = 0;
      int randomUsedA = 0;
      int varianceUsedA = 0;
      advRec.parent.Sequence.GetUsedRandomCache(groupIdx, weaponIdx, out randomUsed, out varianceUsed);
      Log.LogWrite(" used random cache: rnd:"+randomUsed+" var:"+varianceUsed+"\n");
      AttackSequence_GenerateHitInfo.generateWeaponHitInfo(advRec.parent.Sequence, target, weapon, groupIdx, weaponIdx, numberOfShots, false, 0f, ref hitInfo, true, true);
      advRec.parent.Sequence.GetUsedRandomCache(groupIdx, weaponIdx, out randomUsedA, out varianceUsedA);
      Log.LogWrite(" used random cache: rnd:" + randomUsedA + " var:" + varianceUsedA + "\n");
      advRec.parent.Sequence.ResetUsedRandomCache(groupIdx, weaponIdx, randomUsed, varianceUsed);
      bool ret = strayFrags(ref hitInfo, advRec.parent.Sequence, weapon, target);
      Log.LogWrite(" stray generation result:"+ret+"\n");
      if (ret == false) {
        advRec.parent.Sequence.ResetUsedRandomCache(groupIdx, weaponIdx, randomUsedA, varianceUsedA);
      }
      return hitInfo;
    }
    public static void FragSeparation(ref WeaponHitInfo hitInfo) {
      Log.LogWrite("FragWeaponHelper.FragSeparation\n");
      if (hitInfo.isAdvanced() == false) {
        Log.LogWrite(" not advanced\n");
        return;
      }
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if ((advInfo.weapon.HasShells() == false)||(advInfo.weapon.parent.isSpawnProtected() == true)) {
        Log.LogWrite(" weapon "+advInfo.weapon.defId+" have no shells\n");
        return;
      }
      float sMin = advInfo.weapon.MinShellsDistance();
      float sMax = advInfo.weapon.MaxShellsDistance();
      float unsepDmbMod = advInfo.weapon.UnseparatedDamageMult();
      Dictionary<int, WeaponHitInfo> separatedFrags = new Dictionary<int, WeaponHitInfo>();
      for (int hitIndex = 0;hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        AdvWeaponHitInfoRec advRec = advInfo.hits[hitIndex];
        if (advRec.interceptInfo.Intercepted) { continue; }
        Vector3 targetPos = advRec.target.CurrentPosition;
        if(hitInfo.attackerId == advRec.target.GUID) {
          TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(hitInfo.attackerId);
          if (terrainPos != null) { targetPos = terrainPos.pos; };
        }
        Vector3 ePos = CustomAmmoCategories.interpolateSeparationPosition(advRec.trajectorySpline,advRec.startPosition,targetPos,sMin,sMax);
        if (advRec.target.isSpawnProtected()) { ePos = Vector3.zero; }
        if(ePos != Vector3.zero) {
          Log.LogWrite(" "+hitIndex+" separated\n");
          advRec.fragInfo.separated = true;
          hitInfo.hitPositions[hitIndex] = ePos;
          hitInfo.hitLocations[hitIndex] = 65536;
          hitInfo.secondaryTargetIds[hitIndex] = null;
          hitInfo.secondaryHitLocations[hitIndex] = 65536;
          advRec.hitLocation = 65536;
          advRec.hitPosition = ePos;
          advRec.GenerateTrajectory();
          separatedFrags.Add(hitIndex, prepareFragHitInfo(advRec,advRec.target, advInfo.weapon, advInfo.weapon.ProjectilesPerShot,advInfo.groupIdx,advInfo.weaponIdx));
        } else {
          Log.LogWrite(" " + hitIndex + " not separated\n");
          advRec.Damage *= unsepDmbMod;
          advRec.APDamage *= unsepDmbMod;
          advRec.Heat *= unsepDmbMod;
          advRec.Stability *= unsepDmbMod;
        }
      }
      int fullFragCount = hitInfo.numberOfShots;
      foreach (var fragInfo in separatedFrags) {
        fullFragCount += fragInfo.Value.numberOfShots;
      }
      Log.LogWrite("Extending hitInfo " + hitInfo.hitLocations.Length);
      FragWeaponHelper.Resize(ref hitInfo, fullFragCount);
      Log.LogWrite(" -> " + hitInfo.hitLocations.Length + "\n");
      int curFragIndex = hitInfo.numberOfShots;
      foreach (var fragInfo in separatedFrags) {
        FragWeaponHelper.Append(ref hitInfo, curFragIndex, fragInfo.Value);
        advInfo.AppendFrags(fragInfo.Key, fragInfo.Value);
        curFragIndex += fragInfo.Value.numberOfShots;
      }
    }
  }
}