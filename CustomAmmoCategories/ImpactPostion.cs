using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using IRBTModUtils;

namespace CustAmmoCategories {
  public static class ImpactPositionHelper {
    public static bool CheckRaySphereCollide(Vector3 startPoint, Vector3 endPoint, Vector3 sphereCenter, float circleRadius, out Vector3 collidePoint) {
      float cx = sphereCenter.x;
      float cy = sphereCenter.y;
      float cz = sphereCenter.z;
      float px = startPoint.x;
      float py = startPoint.y;
      float pz = startPoint.z;
      float vx = endPoint.x - px;
      float vy = endPoint.y - py;
      float vz = endPoint.z - pz;
      //Log.Combat?.TWL(0, $"CheckRaySphereCollide {px},{py},{pz}<->{vx},{vy},{vz} {cx},{cy},{cz}");

      float A = vx * vx + vy * vy + vz * vz;
      float B = 2.0f * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
      float C = px * px - 2 * px * cx + cx * cx + py * py - 2 * py * cy + cy * cy +
                 pz * pz - 2 * pz * cz + cz * cz - circleRadius * circleRadius;

      float D = B * B - 4 * A * C;
      //Log.Combat?.WL(1, $"A:{A} B:{B} C:{C} D:{D}");
      if (D < 0f) { collidePoint = Vector3.zero; return false; }
      float Ds = Mathf.Sqrt(D);
      float T1 = (0f - B + Ds) / (2.0f * A);
      float T2 = (0f - B - Ds) / (2.0f * A);
      if ((T1 < 0f) || (T2 < 0f)) { collidePoint = Vector3.zero; return false; }
      float T = Mathf.Min(T1, T2);
      if (T > 1.0f) { collidePoint = Vector3.zero; return false; }
      float rx = startPoint.x * (1f - T) + T * endPoint.x;
      float ry = startPoint.y * (1f - T) + T * endPoint.y;
      float rz = startPoint.z * (1f - T) + T * endPoint.z;
      //Log.Combat?.WL(1, $"T:{T} {rx},{ry},{rz}");
      collidePoint = new Vector3(rx,ry,rz);
      return true; 
    }
    public static Vector3 RandomInSphere(Vector3 center, float min, float max) {
      return center + UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(min, max);
    }
    public static Vector3 RandomInCone(Vector3 coneStart, Vector3 coneEnd, float min, float max) {
      return coneEnd + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, coneStart - coneEnd).normalized * UnityEngine.Random.Range(min, max);
    }
    public static Vector3 RandomOnCone(Vector3 coneStart, Vector3 coneEnd, float raduis) {
      return coneEnd + Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, coneStart - coneEnd).normalized * raduis;
    }
    public static Vector3 RandomOnSphere(Vector3 center, float raduis) {
      return center + UnityEngine.Random.onUnitSphere * raduis;
    }
    public static readonly float HIT_POSITION_RANDOM_RADIUS = 0.5f;
    public static Vector3 GetHitPosition(Vector3 center) {
      return RandomInSphere(center, 0f, HIT_POSITION_RANDOM_RADIUS);
    }
    public static Vector3 GetHitPosition(Vector3 start, Vector3 end) {
      return RandomInCone(start, end, 0f, HIT_POSITION_RANDOM_RADIUS);
    }
    public static Vector3 GetHitPositionFast_Combatant(this ICombatant combatant, Vector3 attackPos, int location, bool indirect) {
      if (combatant is AbstractActor actor) { return actor.GetHitPositionFast_AbstractActor(attackPos, location, indirect); };
      if (combatant is BattleTech.Building building) { return building.GetHitPositionFast_Building(attackPos, location, indirect); }
      return GetHitPosition(attackPos, combatant.TargetPosition);
    }
    public static Vector3 GetHitPositionFast_AbstractActor(this AbstractActor actor,Vector3 attackPos, int location, bool indirect) {
      if (actor.GameRep != null) { return GetHitPosition(attackPos, actor.GameRep.GetHitPosition(location)); }
      return GetHitPosition(attackPos, actor.TargetPosition);
    }
    private static Dictionary<string, BuildingPositionInfo> BUILDINGS_POSITIONS_CACHE = new Dictionary<string, BuildingPositionInfo>();
    public class BuildingPositionInfo {
      public HashSet<MapTerrainDataCellEx> innerCells = new HashSet<MapTerrainDataCellEx>();
      public HashSet<MapTerrainDataCellEx> outerCells = new HashSet<MapTerrainDataCellEx>();
      public int radiusCells = 0;
      public float radius = 0f;

    }
    public static void Clear() {
      BUILDINGS_POSITIONS_CACHE.Clear();
    }
    public static List<MapTerrainDataCellEx> midpointCircle(CombatGameState combat, Vector3 center, int r) {
      List<MapTerrainDataCellEx> result = new List<MapTerrainDataCellEx>();
      Point point = combat.MapMetaData.GetIndex(center);
      int zc = point.Z;
      int xc = point.X;
      void putpoint(int lz, int lx) {
        if (combat.MapMetaData.IsWithinBounds(lz, lx)) {
          if (combat.MapMetaData.mapTerrainDataCells[lz, lx] is MapTerrainDataCellEx cell) {
            result.Add(cell);
          }
        }
      }
      void putpoints(int lz, int lx) {
        putpoint(zc + lz, xc + lx);
        putpoint(zc - lz, xc + lx);
        putpoint(zc + lz, xc - lx);
        putpoint(zc - lz, xc - lx);
        putpoint(zc + lx, xc + lz);
        putpoint(zc - lx, xc + lz);
        putpoint(zc + lx, xc - lz);
        putpoint(zc - lx, xc - lz);
      }
      int z = 0, x = r;
      int d = 3 - 2 * r;
      putpoints(z, x);
      while (x >= z) {
        z++;
        if (d > 0) {
          x--; d = d + 4 * (z - x) + 10;
        } else {
          d = d + 4 * x + 6;
        }
        putpoints(z, x);
      }
      return result;
    } 
    public static BuildingPositionInfo GetBuildingCells(this BattleTech.Building building) {
      if (BUILDINGS_POSITIONS_CACHE.TryGetValue(building.GUID, out var result)) { return result; }
      result = new BuildingPositionInfo();
      int r = 1;
      MapTerrainDataCellEx centerCell = building.Combat.MapMetaData.GetCellAt(building.CurrentPosition) as MapTerrainDataCellEx;
      if (centerCell != null) {
        int exitCounter = 2;
        while (r < 100) {
          var cells = midpointCircle(building.Combat, building.CurrentPosition, r);
          bool exitCondition = true;
          foreach(var cell in cells) {
            if (cell.MapEncounterLayerDataCell.HasSpecifiedBuilding(building.GUID)) {
              exitCondition = false; result.innerCells.Add(cell);
            } else {
              result.outerCells.Add(cell);
            }
          }
          if (exitCondition) { --exitCounter; if (result.radiusCells == 0) { result.radiusCells = r; result.radius = r * 4f; } }
          if (exitCounter <= 0) { break; }
          ++r;
        }
      }
      BUILDINGS_POSITIONS_CACHE[building.GUID] = result;
      return result;
    }
    public static bool bresenhamBuildingTest(CombatGameState Combat,Point p0,float height0, Point p1, ref float height1, string targetedBuildingGuid, out Point collisionWorldPos) {
      collisionWorldPos = p1;
      if (p0.X == p1.X && p0.Z == p1.Z) { return true; }
      bool checkBuilding = !string.IsNullOrEmpty(targetedBuildingGuid);
      if (Combat.MapMetaData.IsWithinBounds(p0) == false) { return false; }
      if (Combat.MapMetaData.IsWithinBounds(p1) == false) { return false; }
      List<Point> pointList = BresenhamLineUtil.BresenhamLine(p0, p1);
      float delta = (height1 - height0) / (float)pointList.Count;
      float current = height0;
      MapMetaData mapMetaData = Combat.MapMetaData;
      EncounterLayerData encounterLayerData = Combat.EncounterLayerData;
      for (int index = 0; index < pointList.Count; ++index) {
        current += delta;
        Point point = pointList[index];
        if (checkBuilding && encounterLayerData.mapEncounterLayerDataCells[point.Z, point.X].HasSpecifiedBuilding(targetedBuildingGuid)) {
          collisionWorldPos = pointList[index];
          height1 = current;
          return true;
        }
        if (mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight > current) {
          //collisionWorldPos = pointList[index];
          delta = (mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight + 0.01f - height0) / (float)(index + 1);
          current = mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight + 0.01f;
          height1 = height0 + delta * (float)pointList.Count;
        }
      }
      return true;
    }
    public static bool bresenhamTerrainTest(CombatGameState Combat, Point p0, float height0, Point p1, float height1, out Point collisionPoint, out float collisionHeight) {
      collisionPoint = p1;
      collisionHeight = height1;
      if (p0.X == p1.X && p0.Z == p1.Z) { return false; }
      if (Combat.MapMetaData.IsWithinBounds(p0) == false) { collisionPoint = p0; collisionHeight = height0; return false; }
      List<Point> pointList = BresenhamLineUtil.BresenhamLine(p0, p1);
      float delta = (height1 - height0) / (float)pointList.Count;
      float current = height0;
      MapMetaData mapMetaData = Combat.MapMetaData;
      EncounterLayerData encounterLayerData = Combat.EncounterLayerData;
      for (int index = 0; index < pointList.Count; ++index) {
        current += delta;
        Point point = pointList[index];
        if (Combat.MapMetaData.IsWithinBounds(point) == false) {
          collisionPoint = index > 0 ? pointList[index - 1] : p0;
          collisionHeight = current;
          return false;
        }
        if (mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight > current) {
          collisionPoint = point;
          collisionHeight = mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight;
          return true;
        }
      }
      return false;
    }
    public static bool bresenhamBuildingCollide(CombatGameState Combat, Point p0, float height0, Point p1, float height1, out string BuildingGUID, out Point collisionPoint, out float collisionHeight) {
      collisionPoint = p1;
      collisionHeight = height1;
      BuildingGUID = string.Empty;
      if (p0.X == p1.X && p0.Z == p1.Z) { return false; }
      if (Combat.MapMetaData.IsWithinBounds(p0) == false) { collisionPoint = p0; collisionHeight = height0; return false; }
      List<Point> pointList = BresenhamLineUtil.BresenhamLine(p0, p1);
      float delta = (height1 - height0) / (float)pointList.Count;
      float current = height0;
      MapMetaData mapMetaData = Combat.MapMetaData;
      EncounterLayerData encounterLayerData = Combat.EncounterLayerData;
      for (int index = 0; index < pointList.Count; ++index) {
        current += delta;
        Point point = pointList[index];
        if (Combat.MapMetaData.IsWithinBounds(point) == false) {
          collisionPoint = index > 0 ? pointList[index - 1] : p0;
          collisionHeight = current;
          return false;
        }
        if (mapMetaData.mapTerrainDataCells[point.Z, point.X].cachedHeight > current) {
          collisionPoint = point;
          collisionHeight = current;
          if (mapMetaData.mapTerrainDataCells[point.Z, point.X].MapEncounterLayerDataCell.HasBuilding) {
            BuildingGUID = mapMetaData.mapTerrainDataCells[point.Z, point.X].MapEncounterLayerDataCell.GetTopmostBuilding().buildingGuid;
          }
          return true;
        }
      }
      return false;
    }
    public static Vector3 GetHitPositionFast_Building(this BattleTech.Building building, Vector3 attackPos, int location, bool indirect) {
      if (indirect) {
        List<MapTerrainDataCellEx> cells = building.GetBuildingCells().innerCells.ToList();
        if (cells.Count <= 1) { return GetHitPosition(building.TargetPosition); }
        int index = UnityEngine.Random.Range(0, cells.Count);
        return GetHitPosition(cells[index].WorldPos());
      } else {
        MapTerrainDataCellEx hitCell = building.Combat.MapMetaData.GetCellAt(building.TargetPosition) as MapTerrainDataCellEx;
        MapTerrainDataCellEx startCell = building.Combat.MapMetaData.GetCellAt(attackPos) as MapTerrainDataCellEx;
        if (hitCell == null) { return GetHitPosition(building.TargetPosition); }
        if (startCell == null) { return GetHitPosition(building.TargetPosition); }
        List<MapTerrainDataCellEx> cells = building.GetBuildingCells().innerCells.ToList();
        if (cells.Count > 1) {
          int index = UnityEngine.Random.Range(0, cells.Count);
          hitCell = cells[index];
        }
        Point p0 = startCell.GetPoint();
        Point p1 = hitCell.GetPoint();
        float h0 = attackPos.y;
        float min = Mathf.Min(hitCell.terrainHeight, hitCell.cachedHeight);
        float max = Mathf.Max(hitCell.terrainHeight, hitCell.cachedHeight);
        float h1 = UnityEngine.Random.Range(min, max);
        if (bresenhamBuildingTest(building.Combat, p0, h0, p1, ref h1, building.GUID, out Point collision) == false) {
          return GetHitPosition(building.TargetPosition);
        }
        //if (h1 != ht) { h1 = UnityEngine.Random.Range(h1, max); };
        Vector3 result = building.Combat.MapMetaData.getWorldPos(collision);
        result.y = h1;
        return result;
      }
    }
    public static MissBehavior MissBehavior(this Weapon weapon) {
      var ammo = weapon.ammo();
      var mode = weapon.mode();
      var def = weapon.exDef();
      if (mode.MissBehavior != CustAmmoCategories.MissBehavior.NotSet) { return mode.MissBehavior; }
      if (ammo.MissBehavior != CustAmmoCategories.MissBehavior.NotSet) { return ammo.MissBehavior; }
      if (def.MissBehavior != CustAmmoCategories.MissBehavior.NotSet) { return def.MissBehavior; }
      if (weapon.weaponDef == null) { return CustAmmoCategories.MissBehavior.Unguided; }
      if (weapon.weaponDef.WeaponCategoryValue.IsMissile) { return CustAmmoCategories.MissBehavior.Guided; }
      return CustAmmoCategories.MissBehavior.Unguided;
    }
    public static Vector3 GetMissPosition(this CombatGameState combat, Team team, ICombatant initalTarget, Vector3 attackPos, CustAmmoCategories.MissBehavior missBehavior, Vector3 targetPos, float weaponMaxRange, float missRadius, bool indirect, ref AttackDirection attackDirection, out bool hitAnything, out ICombatant secondaryCombatant, out int secondaryLocation) {
      if (indirect == false) {
        return GetMissPositionDirect(combat, team, initalTarget, attackPos, missBehavior, targetPos, weaponMaxRange, missRadius, ref attackDirection, out hitAnything, out secondaryCombatant, out secondaryLocation);
      } else {
        return GetMissPositionIndirect(combat, team, initalTarget, attackPos, missBehavior, targetPos, weaponMaxRange, missRadius, ref attackDirection, out hitAnything, out secondaryCombatant, out secondaryLocation);
      }
    }
    public static Vector3 GetTerrainCollision(this CombatGameState combat, Vector3 attackPos, Vector3 endPos, out bool hitAnything) {
      Point pc;
      float hc;
      Point p0 = new Point(combat.MapMetaData.GetXIndex(attackPos.x), combat.MapMetaData.GetZIndex(attackPos.z));
      Point p1 = new Point(combat.MapMetaData.GetXIndex(endPos.x), combat.MapMetaData.GetZIndex(endPos.z));
      hitAnything = bresenhamTerrainTest(combat, p0, attackPos.y, p1, endPos.y, out pc, out hc);
      Vector3 collizion = combat.MapMetaData.getWorldPos(pc);
      collizion.y = hc;
      return collizion;
    }
    public static Vector3 GetMissPositionIndirect(this CombatGameState combat, Team team, ICombatant initalTarget, Vector3 attackPos, CustAmmoCategories.MissBehavior missBehavior, Vector3 targetPos, float weaponMaxRange, float missRadius, ref AttackDirection attackDirection, out bool hitAnything, out ICombatant secondaryCombatant, out int secondaryLocation) {
      Vector3 endPos;
      if (initalTarget is BattleTech.Building building) {
        endPos = GetSafeMissBuildingPosition(building, attackPos, missRadius);
      } else {
        endPos = RandomOnSphere(targetPos, missRadius);
      }
      Vector3? groundPos = new Vector3?();
      if (combat.MapMetaData.IsWithinBounds(endPos) == false) {
        int r = 1;
        while (r < 100) {
          var endPoints = midpointCircle(combat, endPos, r);
          if (endPoints.Count > 0) {
            groundPos = endPoints[0].WorldPos();
            endPos.x = groundPos.Value.x;
            endPos.z = groundPos.Value.z;
            break;
          }
          ++r;
        }
        if (r >= 100) {
          endPos = initalTarget.CurrentPosition;
        }
      }
      if (groundPos.HasValue == false) { groundPos = combat.MapMetaData.getWorldPos(combat.MapMetaData.GetIndex(endPos)); }
      if (endPos.y < groundPos.Value.y) { endPos = groundPos.Value; };
      if (missBehavior == CustAmmoCategories.MissBehavior.Unguided) { endPos = groundPos.Value; }
      if (combat.Constants.ToHit.StrayShotsEnabled == false) {
        secondaryCombatant = null;
        hitAnything = true;
        secondaryLocation = 0;
        return endPos;
      }
      if (missBehavior == CustAmmoCategories.MissBehavior.Unguided) {
        if (groundPos.Value.y > endPos.y) { endPos = groundPos.Value; }
      }
      if (combat.Constants.ToHit.StrayShotsHitUnits == false) {
        secondaryCombatant = null;
        secondaryLocation = 0;
        hitAnything = true;
        if (groundPos.Value.y < endPos.y) { return endPos; }
        if (combat.MapMetaData.GetCellAt(endPos).MapEncounterLayerDataCell.HasBuilding) {
          secondaryCombatant = combat.FindCombatantByGUID(combat.MapMetaData.GetCellAt(endPos).MapEncounterLayerDataCell.GetTopmostBuilding().buildingGuid);
          secondaryLocation = 1;
        }
        return endPos;
      }
      secondaryCombatant = null;
      secondaryLocation = 0;
      foreach (var actor in combat.AllActors) {
        if (actor.team == null) { continue; }
        if (actor.IsDead) { continue; }
        if (actor == initalTarget) { continue; }
        if (actor is ICustomMech customMech) {
          if (customMech.carrier != null) { if (customMech.isMountedExternal == false) { continue; } }
        }
        switch (combat.Constants.ToHit.StrayShotValidTargets) {
          case StrayShotValidTargets.ENEMIES_ONLY: if (actor.team.IsEnemy(team) == false) { continue; }; break;
          case StrayShotValidTargets.ENEMIES_AND_NEUTRAL: if ((actor.team.IsEnemy(team) == false) && (actor.team.IsNeutral(team) == false)) { continue; }; break;
          default: break;
        }
        float distance = Vector3.Distance(endPos, actor.TargetPosition);
        if(distance > actor.GetRadius()) { continue; }
        Log.Combat?.WL(1, $"stray shot {actor.PilotableActorDef.ChassisID}");
        if (actor is ICustomMech cm) {
          Log.Combat?.WL(2, $"mounted:{(cm.carrier != null ? cm.carrier.PilotableActorDef.ChassisID : "null")} isMountedExternal:{cm.isMountedExternal}");
        }
        secondaryCombatant = actor;
        if (actor is Mech mech) {
          secondaryLocation = (int)combat.HitLocation.GetHitLocation(attackPos, mech, combat.NetworkRandom.Float(), 0, 1f);
        } else if (actor is Vehicle vehicle) {
          secondaryLocation = (int)combat.HitLocation.GetHitLocation(attackPos, vehicle, combat.NetworkRandom.Float(), 0, 1f);
        } else {
          secondaryLocation = 1;
        }
        hitAnything = true;
        attackDirection = combat.HitLocation.GetAttackDirection(attackPos, actor);
        return GetHitPositionFast_AbstractActor(actor, attackPos, secondaryLocation, false);
      }
      hitAnything = true;
      if (groundPos.Value.y < endPos.y) { return endPos; }
      if (combat.MapMetaData.GetCellAt(endPos).MapEncounterLayerDataCell.HasBuilding) {
        secondaryCombatant = combat.FindCombatantByGUID(combat.MapMetaData.GetCellAt(endPos).MapEncounterLayerDataCell.GetTopmostBuilding().buildingGuid);
        secondaryLocation = 1;
      }
      return endPos;
    }
    public static Vector3 GetMissPositionDirect(this CombatGameState combat, Team team, ICombatant initalTarget, Vector3 attackPos, CustAmmoCategories.MissBehavior missBehavior, Vector3 targetPos, float weaponMaxRange, float missRadius, ref AttackDirection attackDirection, out bool hitAnything, out ICombatant secondaryCombatant, out int secondaryLocation) {
      Vector3 endPos;
      if (initalTarget is BattleTech.Building building) {
        endPos = GetSafeMissBuildingPosition(building, attackPos, missRadius);
      } else {
        endPos = RandomOnCone(attackPos, targetPos, missRadius);
      }
      Point pc;
      float hc;
      Point p0;
      Point p1;
      Vector3 collizion;
      string buildingGUID = string.Empty;
      if (combat.Constants.ToHit.StrayShotsEnabled == false) {
        secondaryCombatant = null;
        secondaryLocation = 0;
        if (missBehavior == CustAmmoCategories.MissBehavior.Unguided) {
          endPos = attackPos + (endPos - attackPos).normalized * 2048.0f;
          Log.Combat?.WL(1, $"StrayShots disabled - setting end position out of map:{Vector3.Distance(endPos, attackPos)}");
        }
        return GetTerrainCollision(combat, attackPos, endPos, out hitAnything);
      }
      if (missBehavior == CustAmmoCategories.MissBehavior.Unguided) {
        endPos = attackPos + (endPos - attackPos).normalized * weaponMaxRange;
        Log.Combat?.WL(1, $"StrayShots enabled - setting end position max weapon range:{Vector3.Distance(endPos, attackPos)}");
      }
      if (combat.Constants.ToHit.StrayShotsHitUnits == false) {
        secondaryCombatant = null;
        secondaryLocation = 0;
        p0 = new Point(combat.MapMetaData.GetXIndex(attackPos.x), combat.MapMetaData.GetZIndex(attackPos.z));
        p1 = new Point(combat.MapMetaData.GetXIndex(endPos.x), combat.MapMetaData.GetZIndex(endPos.z));
        buildingGUID = string.Empty;
        hitAnything = bresenhamBuildingCollide(combat, p0, attackPos.y, p1, endPos.y, out buildingGUID, out pc, out hc);
        if(hitAnything) {
          if(string.IsNullOrEmpty(buildingGUID) == false) {
            secondaryCombatant = combat.FindCombatantByGUID(buildingGUID);
            secondaryLocation = (int)BuildingLocation.Structure;
          }
        } else if(missBehavior == CustAmmoCategories.MissBehavior.Unguided) {
          endPos = attackPos + (endPos - attackPos).normalized * 2048.0f;
          Log.Combat?.WL(1, $"Did not hit any building - setting end position out of map:{Vector3.Distance(endPos, attackPos)}");
          return GetTerrainCollision(combat, attackPos, endPos, out hitAnything);
        }
        collizion = combat.MapMetaData.getWorldPos(pc);
        collizion.y = hc;
        return collizion;
      }
      secondaryCombatant = null;
      secondaryLocation = 0;
      foreach (var actor in combat.AllActors) {
        if (actor.team == null) { continue; }
        if (actor.IsDead) { continue; }
        if (actor == initalTarget) { continue; }
        if (actor is ICustomMech customMech) {
          if(customMech.carrier != null) { if (customMech.isMountedExternal == false) { continue; } }
        }
        switch (combat.Constants.ToHit.StrayShotValidTargets) {
          case StrayShotValidTargets.ENEMIES_ONLY: if (actor.team.IsEnemy(team) == false) { continue; }; break;
          case StrayShotValidTargets.ENEMIES_AND_NEUTRAL: if ((actor.team.IsEnemy(team) == false)&&(actor.team.IsNeutral(team) == false)) { continue; }; break;
          default: break;
        }
        if(CheckRaySphereCollide(attackPos, endPos, actor.TargetPosition, actor.GetRadius(), out var collidePos) == false) { continue; }
        Log.Combat?.WL(1, $"stray shot {actor.PilotableActorDef.ChassisID}");
        if (actor is ICustomMech cm) {
          Log.Combat?.WL(2, $"mounted:{(cm.carrier != null?cm.carrier.PilotableActorDef.ChassisID:"null")} isMountedExternal:{cm.isMountedExternal}");
        }
        p0 = new Point(combat.MapMetaData.GetXIndex(attackPos.x), combat.MapMetaData.GetZIndex(attackPos.z));
        p1 = new Point(combat.MapMetaData.GetXIndex(collidePos.x), combat.MapMetaData.GetZIndex(collidePos.z));
        buildingGUID = string.Empty;
        hitAnything = bresenhamBuildingCollide(combat, p0, attackPos.y, p1, collidePos.y, out buildingGUID, out pc, out hc);
        if (hitAnything) {
          if (string.IsNullOrEmpty(buildingGUID) == false) {
            secondaryCombatant = combat.FindCombatantByGUID(buildingGUID);
            secondaryLocation = (int)BuildingLocation.Structure;
          }
          collizion = combat.MapMetaData.getWorldPos(pc);
          collizion.y = hc;
          return collizion;
        }
        secondaryCombatant = actor;
        if (actor is Mech mech) {
          secondaryLocation = (int)combat.HitLocation.GetHitLocation(attackPos, mech, combat.NetworkRandom.Float(), 0, 1f);
        }else if(actor is Vehicle vehicle) {
          secondaryLocation = (int)combat.HitLocation.GetHitLocation(attackPos, vehicle, combat.NetworkRandom.Float(), 0, 1f);
        } else {
          secondaryLocation = 1;
        }
        attackDirection = combat.HitLocation.GetAttackDirection(attackPos, actor);
        return GetHitPositionFast_AbstractActor(actor, attackPos, secondaryLocation, false);
      }
      Log.Combat?.WL(1, $"Did not hit any actor");
      secondaryCombatant = null;
      secondaryLocation = 0;
      p0 = new Point(combat.MapMetaData.GetXIndex(attackPos.x), combat.MapMetaData.GetZIndex(attackPos.z));
      p1 = new Point(combat.MapMetaData.GetXIndex(endPos.x), combat.MapMetaData.GetZIndex(endPos.z));
      buildingGUID = string.Empty;
      hitAnything = bresenhamBuildingCollide(combat, p0, attackPos.y, p1, endPos.y, out buildingGUID, out pc, out hc);
      if (hitAnything) {
        if (string.IsNullOrEmpty(buildingGUID) == false) {
          secondaryCombatant = combat.FindCombatantByGUID(buildingGUID);
          secondaryLocation = (int)BuildingLocation.Structure;
        }
      } else if (missBehavior == CustAmmoCategories.MissBehavior.Unguided) {
        endPos = attackPos + (endPos - attackPos).normalized * 2048.0f;
        Log.Combat?.WL(1, $"Did not hit any building - setting end position out of map:{Vector3.Distance(endPos, attackPos)}");
        return GetTerrainCollision(combat, attackPos, endPos, out hitAnything);
      }
      collizion = combat.MapMetaData.getWorldPos(pc);
      collizion.y = hc;
      return collizion;
    }
    public static float GetRadius(this ICombatant combatant) {
      if (combatant is AbstractActor actor) {
        float result = 4f;
        WeightClass weightClass = WeightClass.MEDIUM;
        if (actor is Mech mech) {
          result = mech.MechDef.Chassis.Radius;
          weightClass = mech.MechDef.Chassis.weightClass;
        } else
        if (actor is Vehicle vehcile) {
          result = vehcile.VehicleDef.Chassis.Radius;
          weightClass = vehcile.VehicleDef.Chassis.weightClass;
        } else
        if (actor is Turret turret) {
          result = turret.TurretDef.Chassis.Radius;
          weightClass = turret.TurretDef.Chassis.weightClass;
        }
        if (result < CustomAmmoCategories.Epsilon) {
          switch (weightClass) {
            case WeightClass.LIGHT: result = 4f; break;
            case WeightClass.MEDIUM: result = 6f; break;
            case WeightClass.HEAVY: result = 8f; break;
            case WeightClass.ASSAULT: result = 10f; break;
            default: result = 6f; break;
          }
        }
        return result;
      } else if(combatant is BattleTech.Building building) {
        return building.GetBuildingCells().radius;
      }
      return 10f;
    }
    
    public static Vector3 GetSafeMissBuildingPosition(BattleTech.Building building, Vector3 attackPos, float missRadius) {
      List<Vector2> safeCells = new List<Vector2>();
      Vector2 attackPos2D = new Vector2(attackPos.x, attackPos.z);
      Vector2 targetPos2D = new Vector2(building.TargetPosition.x, building.TargetPosition.z);
      float distance = Vector2.Distance(attackPos2D, targetPos2D);
      foreach (var cell in building.GetBuildingCells().outerCells) {
        Vector3 cellPos = cell.WorldPos();
        Vector2 cellPos2D = new Vector2(cellPos.x, cellPos.z);
        float currDist = Vector2.Distance(attackPos2D, cellPos2D);
        if (currDist < distance) { safeCells.Add(cellPos2D); }
      }
      if (safeCells.Count == 0) { return RandomOnCone(attackPos, building.TargetPosition, missRadius); }
      Vector2 rndCell = safeCells[0];
      if (safeCells.Count > 1) { rndCell = safeCells[UnityEngine.Random.Range(0, safeCells.Count)]; }
      Vector2 resultCell = targetPos2D + (rndCell - targetPos2D).normalized * missRadius;
      Vector3 result = new Vector3(resultCell.x, 0f, resultCell.y);
      result.y = building.Combat.MapMetaData.GetCellAt(result).cachedHeight;
      return result;
    }
    public static Vector3 GetImpactPosition(AbstractActor attacker, Vector3 attackPosition, float toHit, bool indirect, Weapon weapon, ICombatant initalTarget, Vector3 targetPosition, ref int location, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryLocation) {
      Vector3 attackPos = attackPosition + attacker.HighestLOSPosition;
      attackDirection = AttackDirection.FromFront;
      switch ((ArmorLocation)location) {
        case ArmorLocation.None:
        case ArmorLocation.Invalid:
          float minMissRadius = 0f;
          if (initalTarget != null) { minMissRadius = Mathf.Max(weapon.MinMissRadius(), initalTarget.GetRadius()); }
          if (minMissRadius < CustomAmmoCategories.Epsilon) { minMissRadius = 10f; }
          float maxMissRadius = weapon.MaxMissRadius();
          if (maxMissRadius < minMissRadius) { maxMissRadius = minMissRadius * 2f; }
          float missRadius = minMissRadius + (maxMissRadius - minMissRadius) * toHit;
          var missBehavior = weapon.MissBehavior();
          Log.Combat?.TWL(0, $"GetMissPosition {attacker.PilotableActorDef.ChassisID} weapon:{weapon.defId} missBehavior:{missBehavior} pos:{attackPos} indirect:{indirect} missRadius:{missRadius} location:{location}");
          if ((weapon.weaponDef != null) && (weapon.weaponDef.WeaponCategoryValue.IsMelee == true)) {
            attackDirection = attacker.Combat.HitLocation.GetAttackDirection(attackPosition, initalTarget);
            return RandomOnCone(attackPos, initalTarget.TargetPosition, minMissRadius);
          }
          Vector3 result = GetMissPosition(attacker.Combat, attacker.team, initalTarget, attackPos, weapon.MissBehavior(), 
            targetPosition, weapon.MaxRange, missRadius, indirect,ref attackDirection, out bool hitAnything, out var strayTarget, out var strayLocation);
          if (indirect) { location = (int)ArmorLocation.Invalid; }
          if (hitAnything) { location = (int)ArmorLocation.Invalid; }
          if (missBehavior == CustAmmoCategories.MissBehavior.Guided) { location = (int)ArmorLocation.Invalid; }
          Log.Combat?.WL(1, $"resultpos:{result}/{Vector3.Distance(targetPosition, result)} hitAnything:{hitAnything} location:{location}");
          if(strayTarget != null) {
            secondaryTargetId = strayTarget.GUID;
            secondaryLocation = strayLocation;
          } else {
            secondaryTargetId = string.Empty;
            secondaryLocation = 0;
          }
          return result;
        default:
          attackDirection = attacker.Combat.HitLocation.GetAttackDirection(attackPosition, initalTarget);
          return GetHitPositionFast_Combatant(initalTarget, attackPos, location, indirect);
      }
    }
    public static readonly string VISUAL_IMPACT_POSITION = "VISUAL_IMPACT_POSITION";
    public static readonly string IMPACT_POSITION_FLAG = "IMPACT_POSITION_INITED";
    public static Vector3 FineTuneTerrainHit(Vector3 startPos, Vector3 endPos) {
      Ray ray = new Ray(startPos, endPos - startPos);
      var hits = Physics.RaycastAll(ray, Vector3.Distance(endPos, startPos), LayerMask.GetMask("Terrain", "Obstruction"), QueryTriggerInteraction.Ignore);
      foreach(var hit in hits) {
        float distance = Vector3.Distance(endPos, hit.point);
        if (distance < 5f) { return hit.point; }
      }
      return endPos;
    }
    public static void PrepareVisuals(WeaponEffect instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      var advRec = hitInfo.advRec(hitIndex);
      if (advRec != null) {
        if (advRec.isHit == false) { advRec.hitPosition = FineTuneTerrainHit(advRec.startPosition, advRec.hitPosition); } else {
          if (advRec.target is BattleTech.Building) { advRec.hitPosition = FineTuneTerrainHit(advRec.startPosition, advRec.hitPosition); }
        }
        Thread.CurrentThread.pushToStack<Vector3>(VISUAL_IMPACT_POSITION, advRec.hitPosition);
        hitInfo.hitPositions[hitIndex] = advRec.hitPosition;
      } else {
        Thread.CurrentThread.pushToStack<Vector3>(VISUAL_IMPACT_POSITION, hitInfo.hitPositions[hitIndex]);
      }
      Thread.CurrentThread.SetFlag(IMPACT_POSITION_FLAG);
    }
    public static void DeprepareVisuals(WeaponEffect instance, ref WeaponHitInfo hitInfo, int hitIndex, int emitterIndex) {
      Thread.CurrentThread.ClearFlag(IMPACT_POSITION_FLAG);
      Thread.CurrentThread.popFromStack<Vector3>(VISUAL_IMPACT_POSITION);
    }
    //public static Vector3 GetImpactPosition(
    //  LineOfSight los,
    //  AbstractActor attacker,
    //  ICombatant initialTarget,
    //  Vector3 attackPosition,
    //  Weapon weapon,
    //  ref int hitLocation,
    //  ref AttackDirection attackDirection,
    //  ref string secondaryTargetId,
    //  ref int secondaryHitLocation
    //) {

    //}
  }
  [HarmonyPatch(typeof(LineOfSight))]
  [HarmonyPatch("GetImpactPosition")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(GameObject) })]
  public static class LineOfSight_GetImpactPosition {
    static void Prefix(ref bool __runOriginal, ref Vector3 __result, LineOfSight __instance, AbstractActor attacker, ICombatant initialTarget, Vector3 attackPosition, Weapon weapon, ref int hitLocation, ref AttackDirection attackDirection, ref string secondaryTargetId, ref int secondaryHitLocation) {
      //try {
      if (__runOriginal == false) { return; }
      if (Thread.CurrentThread.isFlagSet(ImpactPositionHelper.IMPACT_POSITION_FLAG)) {
        __runOriginal = false;
        __result = Thread.CurrentThread.peekFromStack<Vector3>(ImpactPositionHelper.VISUAL_IMPACT_POSITION);
        return;
      }
      throw new Exception("vanilla LineOfSight.GetImpactPosition should not be used");
        //if (attacker.team != attacker.Combat.LocalPlayerTeam) { return; }
        //GameObject targetGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //targetGO.name = "target_"+initialTarget.DisplayName;
        //targetGO.transform.position = initialTarget.TargetPosition;
        //targetGO.transform.localScale = Vector3.one * 10f;
        //var startPos = attackPosition + attacker.HighestLOSPosition;
        //var endPos = initialTarget.TargetPosition + UnityEngine.Random.insideUnitSphere * 5f;
        //GameObject debugLineGO = GameObject.Instantiate(WeaponRangeIndicators.Instance.LineTemplate.gameObject);
        //debugLineGO.transform.SetParent(WeaponRangeIndicators.Instance.transform);
        //debugLineGO.name = $"debugRaycast{weapon.parent.GUID}{initialTarget.GUID}";
        //debugLineGO.SetActive(true);
        //LineRenderer debugLine = debugLineGO.GetComponentInChildren<LineRenderer>(true);
        //debugLine.startWidth = 2.0f;
        //debugLine.endWidth = 2.0f;
        //debugLine.positionCount = 2;
        //debugLine.material = WeaponRangeIndicators.Instance.MaterialInRange;
        //debugLine.startColor = Color.red;
        //debugLine.endColor = Color.red;
        //debugLine.SetPosition(0, startPos);
        //debugLine.SetPosition(1, endPos);
        //if (ImpactPositionHelper.CheckRaySphereCollide(startPos, endPos, initialTarget.TargetPosition, 10f, out var collidePoint)) {
        //  GameObject collideGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //  collideGo.name = "collide_" + initialTarget.DisplayName;
        //  collideGo.transform.position = collidePoint;
        //}
        //} catch (Exception e) {
        //CombatGameState.gameInfoLogger.LogException(e);
      //}
    }
  }

}