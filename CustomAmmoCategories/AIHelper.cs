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
using HarmonyLib;
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
  public static class AIEnemyDistanceHelper {
    public static readonly string IS_UNIT_IN_FLEE = "CAC-UnitFlee";
    public static bool isInFlee(this AbstractActor unit) {
      return unit.StatCollection.GetOrCreateStatisic<bool>(IS_UNIT_IN_FLEE, false).Value<bool>();
    }
    public static void SetInFlee(this AbstractActor unit, bool val) {
      unit.StatCollection.GetOrCreateStatisic<bool>(IS_UNIT_IN_FLEE, false).SetValue<bool>(val);
    }
    public static float GetTonnage(this AbstractActor unit) {
      if (unit.PilotableActorDef is MechDef mechDef) {
        return mechDef.Chassis.Tonnage;
      } else if(unit.PilotableActorDef is VehicleDef vehicleDef) {
        return vehicleDef.Chassis.Tonnage;
      } else if(unit.PilotableActorDef is TurretDef turretDef) {
        return turretDef.Chassis.Tonnage;
      }
      return 0f;
    }
    public static void CheckInFlee(this AbstractActor unit) {
      if (unit.Combat.TurnDirector.IsInterleaved == false) { return; }
      if (unit.IsAttacking) { return; }
      float alliesTonnage = unit.GetTonnage();
      foreach(AbstractActor ally in unit.Combat.GetAllAlliesOf(unit)) {
        if (ally.GUID == unit.GUID) { continue; }
        if (ally.IsDead) { continue; }
        alliesTonnage += ally.GetTonnage();
      }
      float enemiesTonnage = 0f;
      foreach(AbstractActor enemy in unit.Combat.GetAllEnemiesOf(unit)) {
        if (enemy.IsDead) { continue; }
        enemiesTonnage += enemy.GetTonnage();
      }
      float chance = ((enemiesTonnage / alliesTonnage) - CustomAmmoCategories.Settings.FleeStartTonage) / (CustomAmmoCategories.Settings.FleeMaxTonage - CustomAmmoCategories.Settings.FleeStartTonage);
      float roll = UnityEngine.Random.Range(0f, 1f);
      if (roll < chance) {
        unit.Combat.MessageCenter.PublishMessage(new FloatieMessage(unit.GUID, unit.GUID, new Text("ABOUT TO FLEE"), FloatieMessage.MessageNature.Debuff));
        unit.SetInFlee(true);
      }
    }
    public class EnemyDistMoveCandidate {
      public float distance { get; set; }
      public MoveDestination destination { get; set; }
      public EnemyDistMoveCandidate(AbstractActor unit, MoveDestination dest) {
        this.destination = dest;
        this.distance = AIDistanceFromNearesEnemyCache.Get(unit, dest.PathNode.Position);
      }
    }
    public static void FleeCandidatesFilter(AbstractActor unit, ref List<MoveDestination> movementCandidateLocations) {
      if (movementCandidateLocations.Count == 0) { return; }
      List<EnemyDistMoveCandidate> candidates = new List<EnemyDistMoveCandidate>();
      foreach(var dest in movementCandidateLocations) {
        candidates.Add(new EnemyDistMoveCandidate(unit, dest));
      }
      candidates.Sort((a, b) => { return b.distance.CompareTo(a.distance); });
      movementCandidateLocations.Clear();
      float bareerDist = candidates[0].distance * 0.9f;
      Log.Combat?.TWL(0, $"FleeCandidatesFilter:{movementCandidateLocations.Count} limit:{bareerDist}");
      foreach (var cand in candidates) {
        if (cand.distance < bareerDist) { break; }
        movementCandidateLocations.Add(cand.destination);
        Log.Combat?.WL(1,$"{cand.destination.PathNode.Position}:{cand.distance}");
      }
    }
    public static void WeaponDistCandidatesFilter(AbstractActor unit, ref List<MoveDestination> movementCandidateLocations) {
      if (movementCandidateLocations.Count == 0) { return; }
      List<EnemyDistMoveCandidate> candidates = new List<EnemyDistMoveCandidate>();
      foreach (var dest in movementCandidateLocations) {
        candidates.Add(new EnemyDistMoveCandidate(unit, dest));
      }
      candidates.Sort((a, b) => { return a.distance.CompareTo(b.distance); });
      float bareerDist = AIMaxWeaponRangeCache.Get(unit) * 0.8f;
      if (candidates[0].distance > bareerDist) {
        bareerDist = candidates[0].distance * 0.8f;
      }
      Log.Combat?.TWL(0, $"WeaponDistCandidatesFilter:{movementCandidateLocations.Count} weapon max:{AIMaxWeaponRangeCache.Get(unit)} nearest:{candidates[0].distance}");
      movementCandidateLocations.Clear();
      foreach (var cand in candidates) {
        if (cand.distance > bareerDist) { break; }
        movementCandidateLocations.Add(cand.destination);
        Log.Combat?.WL(1, $"{cand.destination.PathNode.Position}:{cand.distance}");
      }
    }
  }
  public static class AIMaxWeaponRangeCache {
    public static int Round { get; set; } = 0;
    public static int Phase { get; set; } = 0;
    private static Dictionary<AbstractActor, float> cache = new Dictionary<AbstractActor, float>();
    public static void Clear() { cache.Clear(); }
    public static float Get(AbstractActor unit) {
      if (Round != unit.Combat.TurnDirector.CurrentRound) { Clear(); }
      if (Phase != unit.Combat.TurnDirector.CurrentPhase) { Clear(); }
      if (cache.TryGetValue(unit, out var result)) { return result; }
      result = 0f;
      foreach(Weapon weapon in unit.Weapons) {
        if (weapon.IsFunctional == false) { continue; }
        var curmode = weapon.getCurrentAmmoMode();
        var ammomodes = weapon.getAvaibleFiringMethods();
        foreach (var ammomode in ammomodes) {
          weapon.ApplyAmmoMode(ammomode);
          var maxrange = weapon.MaxRange;
          if (result < maxrange) { result = maxrange; }
        }
        weapon.ApplyAmmoMode(curmode);
      }
      cache.Add(unit, result);
      return result;
    }
  }
  public static class AIDistanceFromNearesEnemyCache {
    public static readonly float MAX_POSSIBLE_DISTANCE = 3000f;
    public static int round { get; private set; } = 0;
    public static int phase { get; private set; } = 0;
    private static Dictionary<AbstractActor, Dictionary<Vector3, float>> cache = new Dictionary<AbstractActor, Dictionary<Vector3, float>>();
    public static float Get(AbstractActor unit, Vector3 position) {
      if (unit.Combat.TurnDirector.CurrentRound != round) {
        round = unit.Combat.TurnDirector.CurrentRound;
        cache.Clear();
      }
      if(unit.Combat.TurnDirector.CurrentPhase != phase) {
        phase = unit.Combat.TurnDirector.CurrentPhase;
        cache.Clear();
      }
      if(cache.TryGetValue(unit, out var distances) == false) {
        distances = new Dictionary<Vector3, float>();
        cache.Add(unit,distances);
      }
      if(distances.TryGetValue(position, out float distance)) {
        return distance;
      }
      var enemies = unit.Combat.GetAllEnemiesOf(unit);
      distance = MAX_POSSIBLE_DISTANCE;
      foreach (var enemy in enemies) {
        if (enemy.IsDead) { continue; }
        var cur = Vector3.Distance(enemy.CurrentPosition, position);
        if (cur < distance) { distance = cur; }
      }
      distances.Add(position, distance);
      return distance;
    }
    public static void Clear() {
      cache.Clear();
    }
  }
  public class FlatPosition {
    private short x;
    private short y;
    public float X { get { return (float)x / 10f; } }
    public float Y { get { return (float)y / 10f; } }
    public FlatPosition(Vector3 pos) {
      this.x = (short)Mathf.RoundToInt(pos.x * 10f);
      this.y = (short)Mathf.RoundToInt(pos.z * 10f);
    }
    public FlatPosition() {
      this.x = 0;
      this.y = 0;
    }
    public override int GetHashCode() {
      var xb = BitConverter.GetBytes(this.x);
      var yb = BitConverter.GetBytes(this.y);
      byte[] rb = new byte[4];
      xb.CopyTo(rb, 0);
      yb.CopyTo(rb, 2);
      return BitConverter.ToInt32(rb, 0);
    }
    public override bool Equals(object obj) {
      if(obj is FlatPosition b) {
        return (this.x == b.x) && (this.y == b.y);
      }
      return false;
    }
  }

  public static class AIArtilleryStrikeHelper {
    public static UInt32 GetVectorHash(this Vector3 pos) {
      short x = (short)Mathf.RoundToInt(pos.x * 10f);
      short y = (short)Mathf.RoundToInt(pos.z * 10f);
      var xb = BitConverter.GetBytes(x);
      var yb = BitConverter.GetBytes(y);
      byte[] rb = new byte[4];
      xb.CopyTo(rb, 0);
      yb.CopyTo(rb, 2);
      return BitConverter.ToUInt32(rb, 0);
    }
    private static Dictionary<UInt32, float> artStrikeDistanceCache = new Dictionary<uint, float>();
    private static int artStrikeDistanceCacheRound = 0;
    private static int artStrikeDistanceCachePhase = 0;
    private class MoveDestinationElement {
      public MoveDestination dest;
      public float strikeDist;
      public MoveDestinationElement(MoveDestination d) {
        this.dest = d;
        var hash = d.PathNode.Position.GetVectorHash();
        if(artStrikeDistanceCache.TryGetValue(hash, out var dist)) {
          this.strikeDist = dist;
        } else {
          this.strikeDist = WeaponArtilleryHelper.GetDistFromArtStrikeUncached(d.PathNode.Position);
          artStrikeDistanceCache.Add(hash, this.strikeDist);
        }
      }
    }
    public static bool FilterMoveCandidates(AbstractActor unit, ref List<MoveDestination> movementCandidateLocations) {
      try {
        if((artStrikeDistanceCacheRound != unit.Combat.TurnDirector.CurrentRound) || (artStrikeDistanceCachePhase != unit.Combat.TurnDirector.CurrentPhase)) {
          artStrikeDistanceCacheRound = unit.Combat.TurnDirector.CurrentRound;
          artStrikeDistanceCachePhase = unit.Combat.TurnDirector.CurrentPhase;
          artStrikeDistanceCache.Clear();
        }
        List<MoveDestinationElement> moveElements = new List<MoveDestinationElement>();
        foreach(var md in movementCandidateLocations) {
          moveElements.Add(new MoveDestinationElement(md));
        }
        moveElements.Sort((a, b) => { return b.strikeDist.CompareTo(a.strikeDist); });
        Log.P?.TWL(0, $"AIArtilleryStrikeHelper.FilterMoveCandidates {unit.PilotableActorDef.ChassisID}:{unit.GUID} moveCandidates:{movementCandidateLocations.Count}");
        HashSet<MoveDestination> result = new HashSet<MoveDestination>();
        bool hasOutOfStrikeElements = false;
        Log.P?.WL(1, $"distances:");
        foreach(var md in moveElements) {
          Log.P?.WL(2, $"{md.dest.MoveType} {md.dest.PathNode.Position} dist:{md.strikeDist}");
          if(md.dest.MoveType == MoveType.None) { result.Add(md.dest); continue; }
          if(float.IsPositiveInfinity(md.strikeDist)) { result.Add(md.dest); hasOutOfStrikeElements = true; }
        }
        if(hasOutOfStrikeElements == false) {
          if(moveElements.Count > 0) {
            float farest = moveElements[0].strikeDist * 0.9f;
            foreach(var md in moveElements) {
              if(md.strikeDist > farest) { result.Add(md.dest); }
            }
          }
        }
        Log.P?.WL(1, $"result({result.Count}):");
        foreach(var md in result) {
          Log.P?.WL(2, $"{md.MoveType} {md.PathNode.Position}");
        }
        movementCandidateLocations.Clear();
        movementCandidateLocations.AddRange(result);
      }catch(Exception e) {
        Log.P?.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
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
        Log.Combat?.TWL(0, e.ToString(), true);

      }
      return true;
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
    public static bool Prefix(LeafBehaviorNode __instance, ref BehaviorTreeResults __result) {
      try {
        if (CustomAmmoCategories.Settings.extendedBraceBehavior == false) { return true; }
        Log.Combat?.TWL(0, "BraceNode.Tick()");
        Log.Combat?.WL(1, "name:" + __instance.name);
        Log.Combat?.WL(1, "unit:" + new Text(__instance.unit.DisplayName));
        Log.Combat?.WL(1, "type:" + __instance.unit.UnitType);
        Log.Combat?.WL(1, "HasFiredThisRound:" + __instance.unit.HasFiredThisRound+"/"+ __instance.unit.isBracedWithFireThisRound()+"("+ __instance.unit.Combat.TurnDirector.CurrentRound+")");
        if (__instance.unit.isBracedWithFireThisRound()) {
          __instance.unit.HasFiredThisRound = true;
          return true;
        };
        if (__instance.unit.HasFiredThisRound) { return true; };
        Mech mech = __instance.unit as Mech;
        if (mech != null) {
          Log.Combat?.WL(1, "");
          float heatLevelForMech = AIUtil.GetAcceptableHeatLevelForMech(mech);
          Log.Combat?.WL(1, "heat:" + mech.CurrentHeat + "/" + heatLevelForMech);
          if (mech.CurrentHeat > heatLevelForMech) { return true; }
          Log.Combat?.WL(1, "unsteady:" + mech.IsUnsteady);
          if (mech.IsUnsteady) { return true; }
        }
        Log.Combat?.WL(1, "HasAnyContactWithEnemy:" + __instance.unit.HasAnyContactWithEnemy);
        if (__instance.unit.HasAnyContactWithEnemy == false) { return true; }
        List<AbstractActor> VisibleEnemyUnits = __instance.unit.GetVisibleEnemyUnits();
        Log.Combat?.WL(1, "VisibleEnemyUnits:" + VisibleEnemyUnits.Count);
        if (VisibleEnemyUnits.Count > 0) { return true; }
        List<Weapon> AOEweapons = new List<Weapon>();
        List<Weapon> MFweapons = new List<Weapon>();
        List<AbstractActor> detectedTargets = __instance.unit.GetDetectedEnemyUnits();
        Log.Combat?.WL(1, "DetectedEnemyUnits:" + detectedTargets.Count);
        if (detectedTargets.Count == 0) { return true; }
        AbstractActor nearestEnemy = null;
        float nearestDistance = 0f;
        foreach(AbstractActor target in detectedTargets) {
          float distance = Vector3.Distance(target.CurrentPosition, __instance.unit.CurrentPosition);
          if ((nearestDistance == 0f) || (nearestDistance > distance)) { nearestDistance = distance; nearestEnemy = target; }
        }
        if (nearestEnemy == null) { return true; }

        foreach (Weapon weapon in __instance.unit.Weapons) {
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
        Log.Combat?.WL(1, "Capable weapons:"+weaponsList.Count);
        if (weaponsList.Count == 0) { return true; }
        AttackOrderInfo orderInfo = new AttackOrderInfo(__instance.unit, false, false);
        foreach (Weapon weapon in weaponsList) {
          AmmoModePair ammoMode = weapon.getCurrentAmmoMode();
          Log.Combat?.WL(2, weapon.defId+" mode:"+ ammoMode.modeId+" ammo:"+ammoMode.ammoId);
          orderInfo.AddWeapon(weapon);
        }
        string actorGUID = __instance.unit.GUID;
        Log.Combat?.WL(1, "Registering terrain attack to " + actorGUID);
        CustomAmmoCategories.addTerrainHitPosition(__instance.unit, nearestEnemy.CurrentPosition, true);
        __result = new BehaviorTreeResults(BehaviorNodeState.Success);
        __result.orderInfo = orderInfo;
        __result.debugOrderString = "CAC improved brace";
        __result.behaviorTrace = "CAC improved brace";
        __instance.unit.BracedWithFireThisRound();
        return false;
      }catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }

}
