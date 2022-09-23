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
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;
using Building = BattleTech.Building;

namespace CustAmmoCategories {
  public static class AreaOfEffectHelper {
    private static Dictionary<ICombatant, bool> isDropshipCache = new Dictionary<ICombatant, bool>();
    public static void Clear() { isDropshipCache.Clear(); }
    public static void TagAoEModifiers(this ICombatant target, out float range, out float damage) {
      range = 1f;
      damage = 1f;
      HBS.Collections.TagSet tags = target.Tags();
      foreach (var tag in tags) {
        if(CustomAmmoCategories.Settings.TagAoEDamageMult.TryGetValue(tag,out AoEModifiers mods)) {
          range *= mods.Range; damage *= mods.Damage;
        }
      }
    }
    public static bool isDropshipNotLanded(this ICombatant target) {
      try {
        if (isDropshipCache.TryGetValue(target, out bool result)) { return result; }
        Log.M.TWL(0, "AreaOfEffectHelper.isDropshipNotLanded "+new Text(target.DisplayName));
        BattleTech.Building building = target as BattleTech.Building;
        if (building == null) {
          Log.M.WL(1, "not building");
          isDropshipCache.Add(target, false); return false;
        }
        ObstructionGameLogic logic = ObstructionGameLogic.GetObstructionFromBuilding(building, target.Combat.ItemRegistry);
        if (logic.IsDropship() == false) {
          Log.M.WL(1, "not dropship");
          isDropshipCache.Add(target, false); return false;
        }
        DropshipGameLogic dropLogic = logic as DropshipGameLogic;
        if (dropLogic == null) {
          Log.M.WL(1, "no dropship logic");
          isDropshipCache.Add(target, logic.IsDropship()); return logic.IsDropship();
        }
        Log.M.WL(1, "drop ship current animation state:"+ dropLogic.currentAnimationState);
        return dropLogic.currentAnimationState != DropshipAnimationState.Landed;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return false;
      }
    }
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
      if (AOERange < CustomAmmoCategories.Epsilon) { return; };
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
          if (target.isDropshipNotLanded()) { continue; };
          if (CustomAmmoCategories.Settings.SpawnProtectionAffectsAOE) {
            if (target.isSpawnProtected()) { continue; }
            if (weapon.parent.isSpawnProtected()) { continue; }
          }
          Vector3 CurrentPosition = target.CurrentPosition;
          if(advRec.isHit == false) { CurrentPosition += Vector3.up* target.FlyingHeight(); } else {
            if (advRec.target.GUID != target.GUID) { CurrentPosition += Vector3.up * target.FlyingHeight(); }
          }
          float distance = Vector3.Distance(CurrentPosition, hitPosition);
          Log.LogWrite(" testing combatant " + target.DisplayName + " " + target.GUID + " " + distance + "("+CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range+")/" + AOERange + "\n");
          if (CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range < CustomAmmoCategories.Epsilon) { CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range = 1f; }
          distance /= CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Range;
          target.TagAoEModifiers(out float tagAoEModRange, out float tagAoEDamage);
          if (tagAoEModRange < CustomAmmoCategories.Epsilon) { tagAoEModRange = 1f; }
          if (tagAoEDamage < CustomAmmoCategories.Epsilon) { tagAoEDamage = 1f; }
          distance /= tagAoEModRange;
          if (distance > AOERange) { continue; }
          if (targetsHitCache.ContainsKey(target) == false) { targetsHitCache.Add(target, new Dictionary<int, float>()); }
          if (targetsHeatCache.ContainsKey(target) == false) { targetsHeatCache.Add(target, 0f); }
          if (targetsStabCache.ContainsKey(target) == false) { targetsStabCache.Add(target, 0f); }
          //Dictionary<int, float> targetHitCache = targetsHitCache[target];
          float DamagePerShot = AoEDamage * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          float HeatDamagePerShot = AoEHeat * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          float StabilityPerShot = AoEStability * CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage * tagAoEDamage;
          if (HeatDamagePerShot < CustomAmmoCategories.Epsilon) { HeatDamagePerShot = weapon.HeatDamagePerShot; };
          float distanceRatio = weapon.AoEDmgFalloffType((AOERange - distance) / AOERange);
          float targetAoEMult = target.AoEDamageMult();
          float targetHeatMult = target.IncomingHeatMult();
          float targetStabMult = target.IncomingStabilityMult();
          float fullDamage = DamagePerShot * distanceRatio * targetAoEMult;
          float heatDamage = HeatDamagePerShot * distanceRatio * targetAoEMult * targetHeatMult;
          float stabDamage = StabilityPerShot * distanceRatio * targetAoEMult * targetStabMult;
          if (target.isHasHeat() == false) { fullDamage += heatDamage; heatDamage = 0f; }
          if (target.isHasStability() == false) { stabDamage = 0f; }
          targetsHeatCache[target] += heatDamage;
          targetsStabCache[target] += stabDamage;
          Log.LogWrite(" full damage " + fullDamage + " AoEDamage: "+ AoEDamage + " byTypeMod:"+ CustomAmmoCategories.Settings.DefaultAoEDamageMult[target.UnitType].Damage + " tagAoEDamage:"+ tagAoEDamage + " distanceRatio:" + distanceRatio+ " targetAoEMult:"+ targetAoEMult + "\n");
          if (fullDamage < 0f) { fullDamage = 0f; };
          HashSet<int> reachableLocations = new HashSet<int>();
          Dictionary<int, float> SpreadLocations = null;
          Mech mech = target as Mech;
          Vehicle vehicle = target as Vehicle;
          ICustomMech custMech = target as ICustomMech;
          if(custMech != null) {
            List<int> hitLocations = custMech.GetAOEPossibleHitLocations(hitPosition);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            SpreadLocations = custMech.GetAOESpreadArmorLocations();
          } else
          if (mech != null) {
            List<int> hitLocations = mech.GetAOEPossibleHitLocations(hitPosition);//advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, target as Mech);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            SpreadLocations = mech.GetAOESpreadArmorLocations();
          } else
          if (vehicle != null) {
            List<int> hitLocations = advInfo.Sequence.attacker.Combat.HitLocation.GetPossibleHitLocations(hitPosition, vehicle);
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            if (CustomAmmoCategories.VehicleLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            SpreadLocations = CustomAmmoCategories.VehicleLocations;
          } else {
            List<int> hitLocations = new List<int>() { 1 };
            foreach (int loc in hitLocations) { reachableLocations.Add(loc); }
            if (CustomAmmoCategories.OtherLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); };
            SpreadLocations = CustomAmmoCategories.OtherLocations;
          }
          Log.M.TWL(0, "SpreadLocations:"+(SpreadLocations==null?"null":"not null"));
          float locationsCoeff = 0f;
          foreach (var sLoc in SpreadLocations) {
            if (reachableLocations.Contains(sLoc.Key)) { locationsCoeff += sLoc.Value; }
          }
          Dictionary<int, float> AOELocationDamage = new Dictionary<int, float>();
          Log.M.W(2,"Location spread:");
          foreach (var sLoc in SpreadLocations) {
            if (reachableLocations.Contains(sLoc.Key) == false) { continue; }
            if (sLoc.Value < CustomAmmoCategories.Epsilon) { continue; }
            AOELocationDamage.Add(sLoc.Key, sLoc.Value / locationsCoeff);
            string lname = sLoc.Key.ToString();
            if (mech != null) { lname = ((ArmorLocation)sLoc.Key).ToString(); } else
            if (vehicle != null) { lname = ((VehicleChassisLocations)sLoc.Key).ToString(); } else
              lname = ((BuildingLocation)sLoc.Key).ToString();
            Log.M.W(1, lname+":"+ sLoc.Value / locationsCoeff);
          }
          Log.M.WL(0,"");
          foreach (var hitLocation in AOELocationDamage) {
            float CurrentLocationDamage = fullDamage * hitLocation.Value;
            if (targetsHitCache[target].ContainsKey(hitLocation.Key)) {
              targetsHitCache[target][hitLocation.Key] += CurrentLocationDamage;
            } else {
              targetsHitCache[target].Add(hitLocation.Key, CurrentLocationDamage);
            }
            Log.LogWrite("  location " + hitLocation + " damage " + targetsHitCache[target][hitLocation.Key] + "\n");
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
          //AdvWeaponHitInfoRec advRec = new AdvWeaponHitInfoRec(advInfo);
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