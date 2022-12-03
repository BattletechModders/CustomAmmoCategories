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
using CleverGirlAIDamagePrediction;
using CustomAmmoCategoriesLog;
using Harmony;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustAmmoCategories {
  public static class AIMinefieldHelper {
    public static readonly string AI_AWARE_ALL_MINES_PILOT_TAG = "pilot_tag_ai_aware_all_mines";
    public static readonly string AI_MINEFIELD_DAMAGE_THRESHOLD_TAG = "pilot_tag_ai_minefield_damage_threshold_";
    public class MinefieldProjectedDamage {
      public float damage { get; set; } = 0f;
      public Dictionary<MineField, int> minefieldHits { get; set; } = new Dictionary<MineField, int>();
    }
    public class MineFieldFallbackCacheItem {
      public AbstractActor owner { get; set; } = null;
      public int round { get; set; } = -1;
      public int phase { get; set; } = -1;
      public float minefieldDamageTreshold { get; set; } = -1f;
      public float pilotDamageTreshold { get; set; } = 1f;
      public bool AwareAllMines { get; set; } = CustomAmmoCategories.Settings.AIMinefieldAwareAllMines;
      public ConcurrentDictionary<PathNode, MinefieldProjectedDamage> projectedDamage { get; set; } = new ConcurrentDictionary<PathNode, MinefieldProjectedDamage>();
      public HashSet<MoveDestination> checkedDestinations { get; set; } = new HashSet<MoveDestination>();
      public MineFieldFallbackCacheItem(AbstractActor unit) {
        owner = unit;
        round = unit.Combat.TurnDirector.CurrentRound;
        phase = unit.Combat.TurnDirector.CurrentPhase;
        unit.ClearAISafeMovePositions();
        if (unit.IsPilotable) {
          foreach(string tag in unit.GetPilot().pilotDef.PilotTags) {
            if (tag.StartsWith(AI_MINEFIELD_DAMAGE_THRESHOLD_TAG)) {
              if (int.TryParse(tag.Substring(AI_MINEFIELD_DAMAGE_THRESHOLD_TAG.Length), out int pilot_damage_treshhold)){
                pilotDamageTreshold = Mathf.Abs((float)pilot_damage_treshhold / 100f);
              };
            }
            if (tag == AI_AWARE_ALL_MINES_PILOT_TAG) { AwareAllMines = true; }
          }
        }
        ICustomMech custmech = unit as ICustomMech;
        if(custmech != null) {
          foreach (ArmorLocation aloc in custmech.GetLandmineDamageArmorLocations()) {
            float armor = unit.ArmorForLocation((int)aloc);
            if ((armor < CustomAmmoCategories.Epsilon)&&(CustomAmmoCategories.Settings.AIMinefieldAwareAllMinesCritical)) { AwareAllMines = true; }
            if ((minefieldDamageTreshold < 0f) || (minefieldDamageTreshold > armor)) { minefieldDamageTreshold = armor; }
          };
        }
        if (pilotDamageTreshold > CustomAmmoCategories.Epsilon) {
          minefieldDamageTreshold = minefieldDamageTreshold / pilotDamageTreshold;
        } else {
          minefieldDamageTreshold = 9999f;
        }
      }
      public bool isOutdated() {
        if (owner.Combat.TurnDirector.CurrentRound != round) { return true; }
        if (owner.Combat.TurnDirector.CurrentPhase != phase) { return true; }
        return false;
      }
    }
    public static Dictionary<AbstractActor, MineFieldFallbackCacheItem> minefieldFallbackCache = new Dictionary<AbstractActor, MineFieldFallbackCacheItem>();
    public static void Clear() { minefieldFallbackCache.Clear(); AIMinefieldSafeMovePositions.Clear(); }
    private static Dictionary<AbstractActor, HashSet<Vector3>> AIMinefieldSafeMovePositions = new Dictionary<AbstractActor, HashSet<Vector3>>();
    public static void AddAISafeMovePosition(this AbstractActor unit, Vector3 position) {
      if (AIMinefieldSafeMovePositions.TryGetValue(unit, out var positions) == false) {
        positions = new HashSet<Vector3>();
        AIMinefieldSafeMovePositions.Add(unit, positions);
      }
      positions.Add(position);
    }
    public static void ClearAISafeMovePositions(this AbstractActor unit) {
      if(AIMinefieldSafeMovePositions.TryGetValue(unit, out var positions)) {
        positions.Clear();
      }
    }
    public static int CountAISafeMovePositions(this AbstractActor unit) {
      if (AIMinefieldSafeMovePositions.TryGetValue(unit, out var positions)) {
        return positions.Count;
      }
      return 0;
    }
    public static bool CheckAISafeMovePositions(this AbstractActor unit, Vector3 position) {
      if (AIMinefieldSafeMovePositions.TryGetValue(unit, out var positions)) {
        foreach (Vector3 pos in positions) {
          if (Vector3.Distance(pos, position) < CustomAmmoCategories.Settings.AIMinefieldIgnorePosDelta) { return true; }
        }
      }
      return false;
    }
    public static MinefieldProjectedDamage GetProjectedMinefieldDamageMoving(MineFieldFallbackCacheItem cache, PathNode pathNode, AbstractActor unit) {
      if(cache.projectedDamage.TryGetValue(pathNode, out var result)) {
        return result;
      }
      MinefieldProjectedDamage parentDamage = null;
      if (pathNode.Parent != null) {
        parentDamage = GetProjectedMinefieldDamageMoving(cache, pathNode.Parent, unit);
      }
      result = new MinefieldProjectedDamage();
      result.damage = parentDamage == null ? 0f:parentDamage.damage;
      if(parentDamage != null) {
        foreach(var mdmg in parentDamage.minefieldHits) {
          result.minefieldHits.Add(mdmg.Key, mdmg.Value);
        }
      }
      MapTerrainDataCellEx cell = unit.Combat.MapMetaData.GetCellAt(pathNode.Position) as MapTerrainDataCellEx;
      if (cell == null) { goto add_result; }
      PathingCapabilitiesDef PathingCaps = unit.PathingCaps;
      float rollMod = 1f;
      if (CustomAmmoCategories.Settings.MineFieldPathingMods.ContainsKey(PathingCaps.Description.Id)) {
        rollMod = CustomAmmoCategories.Settings.MineFieldPathingMods[PathingCaps.Description.Id] * unit.MinefieldTriggerMult();
      }
      HashSet<MapTerrainHexCell> hexes = new HashSet<MapTerrainHexCell>();
      if((pathNode.Parent == null) && (Vector3.Distance(pathNode.Position, unit.CurrentPosition) > CustomAmmoCategories.Epsilon)) {
        List<MapPoint> mapPoints = MapPoint.calcMapCircle(cell.mapPoint(), CustomAmmoCategories.Settings.JumpLandingMineAttractRadius);
        foreach (MapPoint mapPoint in mapPoints) {
          MapTerrainDataCellEx ccell = unit.Combat.MapMetaData.mapTerrainDataCells[mapPoint.x, mapPoint.y] as MapTerrainDataCellEx;
          if (cell == null) { continue; };
          hexes.Add(cell.hexCell);
        }
      } else {
        hexes.Add(cell.hexCell);
      }
      foreach (MapTerrainHexCell hex in hexes) {
        foreach (MineField mineField in hex.MineFields) {
          if (mineField.count == 0) { continue; }
          if (mineField.getIFFLevel(unit)) { continue; }
          if (cache.AwareAllMines == false) {
            if (mineField.stealthLevel(unit.team) == MineFieldStealthLevel.Invisible) { continue; }
          }
          if (result.minefieldHits.ContainsKey(mineField)) {
            if (result.minefieldHits[mineField] >= mineField.count) { continue; }
            result.minefieldHits[mineField] += 1;
            result.damage += mineField.Def.Damage * mineField.Def.Chance * rollMod;
          } else {
            result.minefieldHits[mineField] = 1;
            result.damage += (mineField.Def.Damage * (mineField.Def.Chance + mineField.count * mineField.Def.DetonateAllMinesInStackChance) * rollMod);
          }
        }
      }
    add_result:
      cache.projectedDamage.AddOrUpdate(pathNode, result, (PathNode enode, MinefieldProjectedDamage edmg)=> { return result; });
      return result;
    }
    public static bool FilterMoveCandidates(AbstractActor unit, ref List<MoveDestination> movementCandidateLocations) {
      try {
        if (CustomAmmoCategories.Settings.AIMinefieldAware == false) { return false; }
        if (unit.UnaffectedLandmines()) { return false; }
        if(minefieldFallbackCache.TryGetValue(unit, out var mcacheItem) == false) {
          mcacheItem = new MineFieldFallbackCacheItem(unit);
          minefieldFallbackCache.Add(unit, mcacheItem);
        }
        if(mcacheItem.isOutdated()) {
          mcacheItem = new MineFieldFallbackCacheItem(unit);
          minefieldFallbackCache[unit] = mcacheItem;
        }
        Log.F?.TWL(0,"AIMinefieldHelper.FilterMoveCandidates "+unit.PilotableActorDef.ChassisID+ " movementCandidateLocations:"+ movementCandidateLocations.Count);
        HashSet<MoveDestination> result = new HashSet<MoveDestination>();
        movementCandidateLocations.Sort((a, b) => { return a.PathNode.CostToThisNode.CompareTo(b.PathNode.CostToThisNode); });
        foreach (MoveDestination moveDestination in movementCandidateLocations) {
          if (mcacheItem.checkedDestinations.Contains(moveDestination)) { result.Add(moveDestination); continue; }
          if (moveDestination.MoveType == MoveType.None) {
            result.Add(moveDestination); continue;
          }
          MinefieldProjectedDamage projectedDamage = GetProjectedMinefieldDamageMoving(mcacheItem, moveDestination.PathNode, unit);
          //Log.F?.WL(1, "moveCandidate: " +moveDestination.PathNode.Position + ":"+moveDestination.PathNode.CostToThisNode+":"+moveDestination.MoveType+" projectedDamage:"+ projectedDamage.damage+" threshold:"+(mcacheItem.minefieldDamageTreshold * CustomAmmoCategories.Settings.AIMinefieldDamageTreshold));
          if (projectedDamage.damage <= mcacheItem.minefieldDamageTreshold * CustomAmmoCategories.Settings.AIMinefieldDamageTreshold) {
            result.Add(moveDestination);
          } else {
            if ((moveDestination.MoveType == MoveType.Backward) || (moveDestination.MoveType == MoveType.Walking) || (moveDestination.MoveType == MoveType.Melee)) {
              if (moveDestination.PathNode.CostToThisNode <= CustomAmmoCategories.Settings.AIMinefieldIgnoreMoveDistance) {
                result.Add(moveDestination);
                unit.AddAISafeMovePosition(moveDestination.PathNode.Position);
              }
            }
          }
        }
        movementCandidateLocations.Clear();
        movementCandidateLocations.AddRange(result);
        Log.F?.WL(0, "filtered movementCandidateLocations:" + movementCandidateLocations.Count+" fallback positions:"+unit.CountAISafeMovePositions());
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
      return false;
    }
  }
  public static class BraceNode_Tick {
    private static Dictionary<AbstractActor, int> bracedWithFireRound = new Dictionary<AbstractActor, int>();
    private static bool isBracedWithFireThisRound(this AbstractActor unit) {
      if(bracedWithFireRound.TryGetValue(unit,out int round)) {
        return round == unit.Combat.TurnDirector.CurrentRound;
      } else {
        return false;
      }
    }
    private static void BracedWithFireThisRound(this AbstractActor unit) {
      if (bracedWithFireRound.ContainsKey(unit) == false) { bracedWithFireRound.Add(unit, unit.Combat.TurnDirector.CurrentRound); } else {
        bracedWithFireRound[unit] = unit.Combat.TurnDirector.CurrentRound;
      }
    }
    public static void Clear() { bracedWithFireRound.Clear(); }
    public static bool Prefix(LeafBehaviorNode __instance,ref string ___name,ref BehaviorTree ___tree, AbstractActor ___unit,ref BehaviorNodeState ___currentState, ref BehaviorTreeResults __result) {
      try {
        if (CustomAmmoCategories.Settings.extendedBraceBehavior == false) { return true; }
        Log.M.TWL(0, "BraceNode.Tick()");
        Log.M.WL(1, "name:" + ___name);
        Log.M.WL(1, "unit:" + new Text(___unit.DisplayName));
        Log.M.WL(1, "type:" + ___unit.UnitType);
        Log.M.WL(1, "HasFiredThisRound:" + ___unit.HasFiredThisRound+"/"+___unit.isBracedWithFireThisRound()+"("+___unit.Combat.TurnDirector.CurrentRound+")");
        if (___unit.isBracedWithFireThisRound()) {
          ___unit.HasFiredThisRound = true;
          return true;
        };
        if (___unit.HasFiredThisRound) { return true; };
        Mech mech = ___unit as Mech;
        if (mech != null) {
          Log.M.WL(1, "");
          float heatLevelForMech = AIUtil.GetAcceptableHeatLevelForMech(mech);
          Log.M.WL(1, "heat:" + mech.CurrentHeat + "/" + heatLevelForMech);
          if (mech.CurrentHeat > heatLevelForMech) { return true; }
          Log.M.WL(1, "unsteady:" + mech.IsUnsteady);
          if (mech.IsUnsteady) { return true; }
        }
        Log.M.WL(1, "HasAnyContactWithEnemy:" + ___unit.HasAnyContactWithEnemy);
        if (___unit.HasAnyContactWithEnemy == false) { return true; }
        List<AbstractActor> VisibleEnemyUnits = ___unit.GetVisibleEnemyUnits();
        Log.M.WL(1, "VisibleEnemyUnits:" + VisibleEnemyUnits.Count);
        if (VisibleEnemyUnits.Count > 0) { return true; }
        List<Weapon> AOEweapons = new List<Weapon>();
        List<Weapon> MFweapons = new List<Weapon>();
        List<AbstractActor> detectedTargets = ___unit.GetDetectedEnemyUnits();
        Log.M.WL(1, "DetectedEnemyUnits:" + detectedTargets.Count);
        if (detectedTargets.Count == 0) { return true; }
        AbstractActor nearestEnemy = null;
        float nearestDistance = 0f;
        foreach(AbstractActor target in detectedTargets) {
          float distance = Vector3.Distance(target.CurrentPosition, ___unit.CurrentPosition);
          if ((nearestDistance == 0f) || (nearestDistance > distance)) { nearestDistance = distance; nearestEnemy = target; }
        }
        if (nearestEnemy == null) { return true; }

        foreach (Weapon weapon in ___unit.Weapons) {
          if (weapon.IsFunctional == false) { continue; }
          if (weapon.IsJammed()) { continue; }
          if (weapon.IsCooldown() > 0) { continue; }
          //if (weapon.NoModeToFire()) { continue; };
          if (weapon.isBlocked()) { continue; };
          if (weapon.isWeaponBlockedStat()) { continue; }
          if (weapon.isCantNormalFire()) { continue; };
          List<AmmoModePair> FiringMethods = weapon.getAvaibleFiringMethods();
          if (FiringMethods.Count == 0) { continue; }
          foreach (AmmoModePair ammoMode in FiringMethods) {
            weapon.ApplyAmmoMode(ammoMode);
            if (weapon.CanFire == false) { continue; }
            if (CustomAmmoCategories.IndirectFireCapable(weapon) == false) { continue; }
            if (weapon.MaxRange < nearestDistance) { continue; }
            if (weapon.AOECapable() && (weapon.AOERange() > CustomAmmoCategories.Epsilon)) {
              AOEweapons.Add(weapon);
              break;
            }
            if (weapon.InstallMineField()) {
              MFweapons.Add(weapon);
              break;
            }
          }
        }
        List<Weapon> weaponsList = new List<Weapon>();
        weaponsList.AddRange(AOEweapons);
        weaponsList.AddRange(MFweapons);
        Log.M.WL(1, "Capable weapons:"+weaponsList.Count);
        if (weaponsList.Count == 0) { return true; }
        AttackOrderInfo orderInfo = new AttackOrderInfo(___unit, false, false);
        foreach (Weapon weapon in weaponsList) {
          AmmoModePair ammoMode = weapon.getCurrentAmmoMode();
          Log.M.WL(2, weapon.defId+" mode:"+ ammoMode.modeId+" ammo:"+ammoMode.ammoId);
          orderInfo.AddWeapon(weapon);
        }
        //MechRepresentation gameRep = ___unit.GameRep as MechRepresentation;
        //if ((UnityEngine.Object)gameRep != (UnityEngine.Object)null) {
        //Log.M.WL(1,"ToggleRandomIdles false");
        //gameRep.ToggleRandomIdles(false);
        //}
        string actorGUID = ___unit.GUID;
        //int seqId = ___unit.Combat.StackManager.NextStackUID;
        Log.M.WL(1, "Registering terrain attack to " + actorGUID);
        CustomAmmoCategories.addTerrainHitPosition(___unit, nearestEnemy.CurrentPosition, true);
        //AttackDirector.AttackSequence attackSequence = ___unit.Combat.AttackDirector.CreateAttackSequence(seqId, ___unit, ___unit, ___unit.CurrentPosition, ___unit.CurrentRotation, 0, weaponsList, MeleeAttackType.NotSet, 0, false);
        //attackSequence.indirectFire = true;
        //Log.M.WL(1, "attackSequence.indirectFire " + attackSequence.indirectFire);
        //___unit.Combat.AttackDirector.PerformAttack(attackSequence);
        __result = new BehaviorTreeResults(BehaviorNodeState.Success);
        __result.orderInfo = orderInfo;
        __result.debugOrderString = "CAC improved brace";
        __result.behaviorTrace = "CAC improved brace";
        ___unit.BracedWithFireThisRound();
        //___unit.HasFiredThisRound = true;
        return false;
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }

}
