/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Designed;
using BattleTech.Framework;
using BattleTech.UI;
using CustAmmoCategories;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(LanceSpawnerGameLogic))]
  [HarmonyPatch("OnUnitSpawnComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class LanceSpawnerGameLogic_OnUnitSpawnComplete {
    public static void Postfix(LanceSpawnerGameLogic __instance) {
      bool flag = false;
      UnitSpawnPointGameLogic[] pointGameLogicList = __instance.unitSpawnPointGameLogicList;
      for (int index = 0; index < pointGameLogicList.Length; ++index) {
        if (pointGameLogicList[index].IsSpawning || pointGameLogicList[index].unitSpawnInProgress) {
          flag = true;
          break;
        }
      }
      if (flag) { return; }
      Log.TWL(0, "LanceSpawnerGameLogic.OnUnitSpawnComplete " + __instance.Name);
      CombatHUD HUD = __instance.HUD();
      if (HUD == null) { return; }
      HUD.MechWarriorTray.RefreshTeam(HUD.Combat.LocalPlayerTeam);
    }
  }
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch("GetRealMissionObjectiveResults")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(TeamController) })]
  public static class LanceSpawnerGameLogic_GetRealMissionObjectiveResults {
    public static void Postfix(Contract __instance, TeamController teamController, ref List<MissionObjectiveResult> __result) {
      Log.TWL(0, "Contract.GetRealMissionObjectiveResults");
      foreach (MissionObjectiveResult result in __result) {
        Log.WL(1, result.guid);
        Log.WL(2, result.title);
        Log.WL(2, result.status.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(HostilityMatrix))]
  [HarmonyPatch("IsFriendly")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Team), typeof(Team) })]
  public static class HostilityMatrix_IsFriendly {
    public static bool Prefix(HostilityMatrix __instance, BattleTech.Team teamOne, BattleTech.Team teamTwo, ref bool __result) {
      if (teamOne == null) { __result = false; return false; }
      if (teamTwo == null) { __result = false; return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(SelectionStateJump))]
  [HarmonyPatch("ProcessLeftClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateJump_ProcessLeftClick {
    public static bool Prefix(SelectionStateMove __instance, Vector3 worldPos, ref bool __result) {
      if (__instance.HasDestination == true) { return true; }
      if (__instance.SelectedActor.IsValidEscortPosition(worldPos, out string message) == false) {
        GenericPopupBuilder.Create(GenericPopupType.Warning, message).IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        __result = false;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(SelectionStateMove))]
  [HarmonyPatch("ProcessLeftClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3) })]
  public static class SelectionStateMove_ProcessLeftClick {
    public static float GetRouteDistance(this CombatGameState combat, Vector3 pos) {
      List<ConvoyRoutePoint> route = combat.getConvoyRoutePoints();
      float result = 0f;
      for(int t = 0; t < (route.Count - 1); ++t) {
        Vector3 point = route[t].point;
        Vector3 pointNext = route[t+1].point;
        float routeDist = Vector3.Distance(pos, point) + Vector3.Distance(pos, pointNext) - Vector3.Distance(point, pointNext);
        if ((result == 0f) || (result > routeDist)) { result = routeDist; }
      }
      return result;
    }
    public static float GetNearestPlayerDistance(this CombatGameState combat, Vector3 pos) {
      List<AbstractActor> allActors = combat.AllActors;
      float result = 0f;
      foreach (AbstractActor actor in allActors) {
        if (actor.IsDead) { continue; }
        if (actor.TeamId != combat.LocalPlayerTeamGuid) { continue; }
        if (actor.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag)) { continue; }
        if (actor.IsDeployDirector()) { continue; }
        float distance = Vector3.Distance(pos, actor.CurrentIntendedPosition);
        if ((result == 0f) || (result > distance)) { result = distance; }
      }
      return result;
    }
    public static float GetFarestOtherDistance(this CombatGameState combat, Vector3 pos) {
      List<AbstractActor> allActors = combat.AllActors;
      float result = 0f;
      foreach (AbstractActor actor in allActors) {
        if (actor.IsDead) { continue; }
        if (actor.TeamId != combat.LocalPlayerTeamGuid) { continue; }
        if (actor.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag) == false) { continue; }
        if (actor.IsDeployDirector()) { continue; }
        float distance = Vector3.Distance(pos, actor.CurrentIntendedPosition);
        if ((result == 0f) || (result < distance)) { result = distance; }
      }
      return result;
    }
    public static bool IsValidEscortPosition(this AbstractActor unit, Vector3 worldPos, out string message) {
      message = string.Empty;
      if (unit.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag)) {
        message = Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU? "Стой на месте и жди эвакуации, падла!" : "You should wait at this point for an extraction";
        return false;
      } else if (unit.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag)) {
        bool result = true;
        float routeDistance = unit.Combat.GetRouteDistance(worldPos);
        float curRouteDistance = unit.Combat.GetRouteDistance(unit.CurrentIntendedPosition);
        float routeDistanceLimit = curRouteDistance > Core.Settings.ConvoyMaxDistFromRoute ? curRouteDistance : Core.Settings.ConvoyMaxDistFromRoute;
        StringBuilder content = new StringBuilder();
        float playerDistnace = unit.Combat.GetNearestPlayerDistance(worldPos);
        float otherDistnace = unit.Combat.GetFarestOtherDistance(worldPos);
        if (routeDistance > routeDistanceLimit) { content.Append("<color=red>"); result = false; } else { content.Append("<color=green>"); };
        content.AppendLine($"{(Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU ? "Отклонение от маршрута" : "Distance from convoy route")}:{routeDistance}/{routeDistanceLimit}</color>");
        if (playerDistnace > Core.Settings.ConvoyMaxDistFromPlayer) { content.Append("<color=red>"); result = false; } else { content.Append("<color=green>"); };
        content.AppendLine($"{(Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU ? "Расстояние от ваших боевых единиц" : "Distance from your units")}: {playerDistnace}/{Core.Settings.ConvoyMaxDistFromPlayer}</color>");
        if (otherDistnace > Core.Settings.ConvoyMaxDistFromOther) { content.Append("<color=red>"); result = false; } else { content.Append("<color=green>"); };
        content.AppendLine($"{(Strings.CurrentCulture == Strings.Culture.CULTURE_RU_RU ? "Расстояние от других машин конвоя" : "Distance from other convoy units")}: {otherDistnace}/{Core.Settings.ConvoyMaxDistFromOther}</color>");
        message = content.ToString();
        return result;
      }
      return true;
    }
    public static bool isConvoyUnit(this AbstractActor unit) {
      return unit.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag) || unit.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag);
    }
    public static bool Prefix(SelectionStateMove __instance, Vector3 worldPos, ref bool __result) {
      if (__instance.SelectedActor == null) { return true; }
      if (__instance.SelectedActor.isConvoyUnit() == false) { return true; }
      if (__instance.HasTarget) {
        GenericPopupBuilder.Create(GenericPopupType.Warning, "CONVOY UNITS CAN'T MELEE").IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        __result = false;
        return false;
      }
      if (__instance.HasDestination == true) { return true; }
      if (__instance.SelectedActor.IsValidEscortPosition(__instance.SelectedActor.Pathing.ResultDestination, out string message) == false) {
        GenericPopupBuilder.Create(GenericPopupType.Warning, message).IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        __result = false;
        return false;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("DespawnActor")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AbstractActor_FlagForDeath {
    public static void Postfix(AbstractActor __instance, MessageCenterMessage message, ref string ____teamId, ref Team ____team) {
      try {
        DespawnActorMessage despawnActorMessage = message as DespawnActorMessage;
        if (despawnActorMessage == null) { return; }
        if (!(despawnActorMessage.affectedObjectGuid == __instance.GUID)) { return; }
        Log.TWL(0, "AbstractActor.DespawnActor " + __instance.DisplayName + ":" + despawnActorMessage.deathMethod);
        if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
        string transferTeamId = string.Empty;
        foreach (string tag in __instance.EncounterTags) {
          if (tag.StartsWith(Core.Settings.TransferTeamOnDeathPrefixTag)) {
            transferTeamId = tag.Substring(Core.Settings.TransferTeamOnDeathPrefixTag.Length);
            break;
          }
        }
        if (string.IsNullOrEmpty(transferTeamId)) { return; };
        Team transferTeam = __instance.Combat.TurnDirector.GetTurnActorByUniqueId(transferTeamId) as Team;
        if (transferTeam == null) { return; }
        __instance.Combat.LocalPlayerTeam.RemoveUnit(__instance);
        transferTeam.AddUnit(__instance);
        ____teamId = transferTeamId;
        ____team = transferTeam;
        __instance.HUD().MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("HandleDeath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class Mech_HandleDeath {
    public static void TransferLance(this CombatGameState combat, Lance lance, Team team) {
      if (lance == null) { return; }
      if (combat == null) { return; }
      if (team == null) { return; }
      bool flag = true;
      foreach (string guid in lance.unitGuids) {
        AbstractActor unit = combat.FindActorByGUID(guid);
        if (unit == null) { continue; }
        if (unit.TeamId != team.GUID) { flag = false; break; }
      }
      if (flag) { lance.team = team; }
    }
    public static void Postfix(Mech __instance, string attackerGUID, ref string ____teamId, ref Team ____team) {
      try {
        if (__instance.IsDead == false) { return; }
        if (__instance.WasDespawned) { return; }
        if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
        Log.TWL(0, "Mech.HandleDeath " + __instance.DisplayName + ":" + attackerGUID);
        string transferTeamId = string.Empty;
        foreach (string tag in __instance.EncounterTags) {
          if (tag.StartsWith(Core.Settings.TransferTeamOnDeathPrefixTag)) {
            transferTeamId = tag.Substring(Core.Settings.TransferTeamOnDeathPrefixTag.Length);
            break;
          }
        }
        if (string.IsNullOrEmpty(transferTeamId)) { return; };
        Team transferTeam = __instance.Combat.TurnDirector.GetTurnActorByUniqueId(transferTeamId) as Team;
        if (transferTeam == null) { return; }
        __instance.Combat.LocalPlayerTeam.RemoveUnit(__instance);
        transferTeam.AddUnit(__instance);
        ____teamId = transferTeamId;
        ____team = transferTeam;
        __instance.Combat.TransferLance(__instance.lance, transferTeam);
        __instance.HUD().MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("HandleDeath")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class Vehicle_HandleDeath {
    public static void Postfix(Vehicle __instance, string attackerGUID, ref string ____teamId, ref Team ____team) {
      try {
        if (__instance.IsDead == false) { return; }
        if (__instance.WasDespawned) { return; }
        if (__instance.TeamId != __instance.Combat.LocalPlayerTeamGuid) { return; };
        Log.TWL(0, "Vehicle.HandleDeath " + __instance.DisplayName + ":" + attackerGUID);
        string transferTeamId = string.Empty;
        foreach (string tag in __instance.EncounterTags) {
          if (tag.StartsWith(Core.Settings.TransferTeamOnDeathPrefixTag)) {
            transferTeamId = tag.Substring(Core.Settings.TransferTeamOnDeathPrefixTag.Length);
            break;
          }
        }
        if (string.IsNullOrEmpty(transferTeamId)) { return; };
        Team transferTeam = __instance.Combat.TurnDirector.GetTurnActorByUniqueId(transferTeamId) as Team;
        if (transferTeam == null) { return; }
        __instance.Combat.LocalPlayerTeam.RemoveUnit(__instance);
        transferTeam.AddUnit(__instance);
        ____teamId = transferTeamId;
        ____team = transferTeam;
        __instance.Combat.TransferLance(__instance.lance, transferTeam);
        __instance.HUD().MechWarriorTray.RefreshTeam(__instance.Combat.LocalPlayerTeam);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(OccupyRegionObjective))]
  [HarmonyPatch("UpdateCounts")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class OccupyRegionObjective_UpdateCounts {
    public static void Postfix(OccupyRegionObjective __instance) {
      try {
        List<ICombatant> targetUnits = __instance.GetTargetUnits();
        for (int index = 0; index < targetUnits.Count; ++index) {
          ICombatant combatant = targetUnits[index];
          if (combatant.IsDead) { continue; }
          AbstractActor unit = combatant as AbstractActor;
          if (unit == null) { continue; }
          if (unit.TeamId != unit.Combat.LocalPlayerTeamGuid) { continue; }
          if (unit.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag) == false) { continue; }
          if (combatant.IsInRegion(__instance.occupyTargetRegion.EncounterObjectGuid) == false) {
            if (unit.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag)) { unit.EncounterTags.Remove(Core.Settings.ConvoyDenyMoveTag); }
          } else {
            if (unit.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag) == false) { unit.EncounterTags.Add(Core.Settings.ConvoyDenyMoveTag); }
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(ObjectiveGameLogic))]
  [HarmonyPatch("ActivateObjective")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ObjectiveGameLogic_ActivateObjectiveConvoy {
    private static string escortTargetRegion = string.Empty;
    public static void Prefix(ObjectiveGameLogic __instance) {
      try {
        Log.TWL(0, "ObjectiveGameLogic.ActivateObjective "+ __instance.GetType().ToString()+ " IsComplete:" + __instance.IsComplete);
        if (__instance.IsComplete) { return; }
        EscortXUnitsObjective escortXUnitsObjective = __instance as EscortXUnitsObjective;
        if (escortXUnitsObjective == null) { return; }
        __instance.Combat.FillRoutePoints();
        List<ConvoyRoutePoint> route = __instance.Combat.getConvoyRoutePoints();
        Log.WL(1, "route:" + route.Count);
        foreach (ConvoyRoutePoint point in route) {
          MapTerrainDataCellEx cell = escortXUnitsObjective.Combat.MapMetaData.GetCellAt(point.point) as MapTerrainDataCellEx;
          Log.WL(2, "point:" + point.point+" cell:"+(cell==null?"null":"not null"));
          //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
          //sphere.transform.SetParent(null);
          //sphere.transform.position = point.point;
          //sphere.transform.localScale = Vector3.one * 10f;
          if (cell == null) { continue; }
          cell.hexCell.addTempTerrainVFX(escortXUnitsObjective.Combat, Core.Settings.ConvoyRouteBeaconVFX, 999, Core.Settings.ConvoyRouteBeaconVFXScale.vector);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class ConvoyRoutePoint {
    public Vector3 point { get; set; }
    public float distToEnd { get; set; }
    private ConvoyRoutePoint _next;
    private ConvoyRoutePoint _prev;
    public ConvoyRoutePoint next { get { return _next; } set { _next = value; distanceNext = _next == null ? 0f : Vector3.Distance(point, _next.point); } }
    public ConvoyRoutePoint prev { get { return _prev; } set { _prev = value; distancePrev = _prev == null ? 0f : Vector3.Distance(point, _prev.point); } }
    public float distanceNext { get; set; }
    public float distancePrev { get; set; }
    public ConvoyRoutePoint(Vector3 p,Vector3 endPos) {
      point = p;
      distanceNext = 0f;
      distancePrev = 0f;
      _next = null;
      _prev = null;
      distToEnd = Vector3.Distance(p,endPos);
    }
  }
  public class ConvoyRoutePointInfoCache {
    public ConvoyRoutePoint nearest { get; set; }
    public float elipticDistance { get; set; }
    public ConvoyRoutePointInfoCache(Vector3 pos) {
      nearest = pos.getNearestRoute();
      float eDistNext = Vector3.Distance(pos, nearest.point);
      float eDistPrev = eDistNext;
      if (nearest.next != null) eDistNext += Vector3.Distance(pos, nearest.next.point);
      if (nearest.prev != null) eDistPrev += Vector3.Distance(pos, nearest.prev.point);
      eDistNext -= nearest.distanceNext;
      eDistPrev -= nearest.distancePrev;
      elipticDistance = Mathf.Min(eDistNext, eDistPrev);
    }
  }
  public class ConvoyActorPathingLimitCache {
    public AbstractActor unit { get; set; }
    public int round { get; set; }
    public List<AbstractActor> others { get; set; }
    public List<AbstractActor> player { get; set; }
    public AbstractActor FarestOther { get; private set; }
    public AbstractActor NearestPlayer { get; private set; }
    public Dictionary<Vector3, float> distanceFormOthersCache { get; set; }
    public Dictionary<Vector3, float> distanceFormPlayerCache { get; set; }
    public void UpdatePositionValues(int round, bool forced) {
      if ((this.round == round) && (forced == false)) { return; }
      distanceFormOthersCache.Clear();
      distanceFormPlayerCache.Clear();
      this.round = round;
      float farestOtherDist = 0f;
      FarestOther = unit;
      foreach (AbstractActor other in others) {
        float distance = Vector3.Distance(unit.CurrentIntendedPosition, other.CurrentIntendedPosition);
        if (other.IsDead) { continue; }
        if (farestOtherDist < distance) { farestOtherDist = distance; FarestOther = other; }
      }
      NearestPlayer = unit;
      float nearestPlayerDist = 0f;
      foreach (AbstractActor punit in player) {
        float distance = Vector3.Distance(unit.CurrentIntendedPosition, punit.CurrentIntendedPosition);
        if (punit.IsDead) { continue; }
        if ((nearestPlayerDist > distance) || (nearestPlayerDist == 0f)) { nearestPlayerDist = distance; NearestPlayer = punit; }
      }
    }
    public float getDistanceFromOthers(Vector3 pos) {
      if (distanceFormOthersCache.TryGetValue(pos, out float result) == false) {
        result = 0f;
        foreach (AbstractActor other in others) {
          if (other.IsDead) { continue; }
          float dist = Vector3.Distance(pos, other.CurrentIntendedPosition);
          if (dist > result) { result = dist; FarestOther = other; }
        }
        distanceFormOthersCache.Add(pos, result);
        Log.TWL(0, "getDistanceFromOthers round:" + round + " pos:" + pos + " farest:" + FarestOther.DisplayName + " result:" + result + "/" + Core.Settings.ConvoyMaxDistFromOther);
      }
      return result;
    }
    public float getDistanceFromPlayer(Vector3 pos) {
      if (distanceFormPlayerCache.TryGetValue(pos, out float result) == false) {
        result = 0f;
        foreach (AbstractActor other in player) {
          if (other.IsDead) { continue; }
          float dist = Vector3.Distance(pos, other.CurrentIntendedPosition);
          if ((dist < result) || (result == 0f)) { result = dist; NearestPlayer = other; }
        }
        distanceFormPlayerCache.Add(pos, result);
        Log.TWL(0, "getDistanceFromPlayer round:" + round + " pos:" + pos + " nearest:" + NearestPlayer.DisplayName + " result:" + result + "/" + Core.Settings.ConvoyMaxDistFromPlayer);
      }
      return result;
    }
    public ConvoyActorPathingLimitCache(AbstractActor owner) {
      unit = owner;
      others = new List<AbstractActor>();
      player = new List<AbstractActor>();
      distanceFormOthersCache = new Dictionary<Vector3, float>();
      distanceFormPlayerCache = new Dictionary<Vector3, float>();
      foreach (AbstractActor actor in unit.Combat.AllActors) {
        if (actor.GUID == unit.GUID) { continue; }
        if (actor.IsDead) { continue; }
        if (actor.TeamId != actor.Combat.LocalPlayerTeamGuid) { continue; }
        if (actor.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag) == false) {
          player.Add(actor);
        } else {
          others.Add(actor);
        }
      }
      this.round = -1;
      this.UpdatePositionValues(unit.Combat.TurnDirector.CurrentRound, true);
    }
  }
  public class ConvoyPositionPathingLimitCache {
    public float MaxDistanceFromOthers { get; private set; }
    public float MinDistanceFromOthers { get; private set; }
    public float MaxDistanceFromRoute { get; private set; }
    public float MinDistanceFromRoute { get; private set; }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetTerrainModifiedCost")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(PathNode), typeof(PathNode), typeof(float) })]
  public static class PathNodeGrid_GetTerrainModifiedCost {
    private static List<ConvoyRoutePoint> routePoints = new List<ConvoyRoutePoint>();
    public static List<ConvoyRoutePoint> getConvoyRoutePoints(this CombatGameState combat) { return routePoints; }
    //private static bool routePointsFilled = false;
    private static Dictionary<AbstractActor, ConvoyActorPathingLimitCache> convoyPathing = new Dictionary<AbstractActor, ConvoyActorPathingLimitCache>();
    private static Dictionary<Vector3, ConvoyRoutePointInfoCache> distanceFromRouteCache = new Dictionary<Vector3, ConvoyRoutePointInfoCache>();
    private static Dictionary<Vector3, Dictionary<Vector3, float>> actorFromRouteCache = new Dictionary<Vector3, Dictionary<Vector3, float>>();
    public static void Clear() {
      routePoints.Clear();
      convoyPathing.Clear();
      distanceFromRouteCache.Clear();
      actorFromRouteCache.Clear();
      //routePointsFilled = false;
    }
    public static void FillRoutePoints(this CombatGameState combat) {
      //if (routePointsFilled) { return; }
      //routePointsFilled = true;
      routePoints.Clear();
      EscortChunkGameLogic escortLogic = combat.EncounterLayerData.gameObject.GetComponentInChildren<EscortChunkGameLogic>(true);
      if(escortLogic == null) {
        Log.TWL(0, "!Exception!: no EscortChunkGameLogic in EncounterLayerData");
        return;
      }
      RegionPointGameLogic[] regions = escortLogic.GetComponentsInChildren<RegionPointGameLogic>();
      float x = 0f;
      float y = 0f;
      float z = 0f;
      float i = 0f;
      UnitSpawnPointGameLogic[] spoints = escortLogic.GetComponentsInChildren<UnitSpawnPointGameLogic>();
      foreach (UnitSpawnPointGameLogic spoint in spoints) {
        x += spoint.hexPosition.x;
        y += spoint.hexPosition.y;
        z += spoint.hexPosition.z;
        i += 1f;
      }
      Vector3 startPos = new Vector3(x / i, y / i, z / i);
      startPos.y = combat.MapMetaData.GetCellAt(startPos).cachedHeight;
      x = 0f;
      y = 0f;
      z = 0f;
      i = 0f;
      foreach (RegionPointGameLogic rpoint in regions) {
        x += rpoint.hexPosition.x;
        y += rpoint.hexPosition.y;
        z += rpoint.hexPosition.z;
        i += 1f;
      }
      Vector3 endPos = new Vector3(x / i, y / i, z / i);
      endPos.y = combat.MapMetaData.GetCellAt(endPos).cachedHeight;
      if (escortLogic == null) { return; }
      RouteGameLogic route = escortLogic.GetComponentInChildren<RouteGameLogic>();
      if (route == null) { return; }
      RoutePointGameLogic[] rPoints = route.routePointList;
      //if (rPoints.Length > 0) {
      //  float fpToEndDist = Vector3.Distance(rPoints[0].hexPosition,endPos);
      //  float stToEndDist = Vector3.Distance(startPos, endPos);
      //  if (stToEndDist > fpToEndDist) {
      //    routePoints.Add(new ConvoyRoutePoint(startPos, endPos));
      //  }
      //} else {
      routePoints.Add(new ConvoyRoutePoint(startPos, endPos));
      //}
      for (int t = 0; t < rPoints.Length; ++t) {
        routePoints.Add(new ConvoyRoutePoint(rPoints[t].hexPosition, endPos));
      }
      routePoints.Add(new ConvoyRoutePoint(endPos, endPos));
      //routePoints.Sort((a, b) => a.distToEnd.CompareTo(b.distToEnd));
      Log.TWL(0, "FillRoutePoints:" + routePoints.Count);
      for (int t = 0; t < routePoints.Count; ++t) {
        //Log.WL(1,"point:"+ routePoints[t].point+" end dist:"+ routePoints[t].distToEnd);
        if (t < (routePoints.Count - 1)) { routePoints[t].next = routePoints[t + 1]; }
        if (t > 0) { routePoints[t].prev = routePoints[t - 1]; }
      }
    }
    public static ConvoyRoutePoint getNearestRoute(this Vector3 pos) {
      ConvoyRoutePoint result = routePoints[0];
      float near = Vector3.Distance(pos, result.point);
      foreach (ConvoyRoutePoint route in routePoints) {
        float distance = Vector3.Distance(pos, route.point);
        if ((near > distance)) { near = distance; result = route; }
      }
      return result;
    }
    /*public static float DistanceFromRoute(Vector3 pos) {
      float result = -1f;
      foreach (Vector3 route in routePoints) {
        float distance = Vector3.Distance(pos, route);
        if ((result == -1f) || (result > distance)) { result = distance; }
      }
      Log.TWL(0, "DistanceFromRoute:" + pos.ToString() + " = " + result);
      return result;
    }*/
    public static void UpdateConvoyPathingCache(this AbstractActor actor) {
      if (convoyPathing.TryGetValue(actor, out ConvoyActorPathingLimitCache convoyActorPathing)) {
        convoyActorPathing.UpdatePositionValues(actor.Combat.TurnDirector.CurrentRound, true);
      }
    }
    public static bool isValidConvoyPosition(this AbstractActor actor, Vector3 pos) {
      if (actor.EncounterTags.Contains(Core.Settings.ConvoyDenyMoveTag)) {
        return false;
      }
      if (actor.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag)) {
        //FillRoutePoints(actor.Combat);
        //Log.TWL(0, "isValidConvoyPosition:" + actor.DisplayName + ":"+actor.Combat.TurnDirector.CurrentRound+":" + pos);
        if (distanceFromRouteCache.TryGetValue(pos, out ConvoyRoutePointInfoCache posDistCache) == false) {
          posDistCache = new ConvoyRoutePointInfoCache(pos);
          distanceFromRouteCache.Add(pos, posDistCache);
        }
        //Log.WL(1, "pos nearest:" + posDistCache.nearest.point + " ellipticDist:" + posDistCache.elipticDistance + "/" + Core.Settings.ConvoyMaxDistFromRoute);
        if (posDistCache.elipticDistance > Core.Settings.ConvoyMaxDistFromRoute) {
          if (actorFromRouteCache.TryGetValue(actor.CurrentPosition, out Dictionary<Vector3, float> actorRouteCache) == false) {
            actorRouteCache = new Dictionary<Vector3, float>();
            actorFromRouteCache.Add(actor.CurrentPosition, actorRouteCache);
          }
          if (actorRouteCache.TryGetValue(pos, out float actorRouteEllpticDist) == false) {
            if (distanceFromRouteCache.TryGetValue(actor.CurrentPosition, out ConvoyRoutePointInfoCache actorDistCache) == false) {
              actorDistCache = new ConvoyRoutePointInfoCache(actor.CurrentPosition);
              distanceFromRouteCache.Add(actor.CurrentPosition, actorDistCache);
            }
            actorRouteEllpticDist = Vector3.Distance(actor.CurrentPosition, pos);
            actorRouteEllpticDist += Vector3.Distance(actorDistCache.nearest.point, pos);
            actorRouteEllpticDist -= Vector3.Distance(actor.CurrentPosition, actorDistCache.nearest.point);
            actorRouteCache.Add(pos, actorRouteEllpticDist);
          }
          //Log.WL(1, "actorPos:" + actor.CurrentPosition.ToString() + " ellipticDist:" + actorRouteEllpticDist + "/" + Core.Settings.ConvoyMaxDistFromRoute);
          if (actorRouteEllpticDist > Core.Settings.ConvoyMaxDistFromRoute) {
            return false;
          }
        }
        if (convoyPathing.TryGetValue(actor, out ConvoyActorPathingLimitCache convoyActorPathing) == false) {
          convoyActorPathing = new ConvoyActorPathingLimitCache(actor);
          convoyPathing.Add(actor, convoyActorPathing);
        }
        convoyActorPathing.UpdatePositionValues(actor.Combat.TurnDirector.CurrentRound, false);
        float distanceFromOthers = convoyActorPathing.getDistanceFromOthers(pos);
        //Log.WL(1, "distanceFromOthers:" + distanceFromOthers + " / " + Core.Settings.ConvoyMaxDistFromOther);
        if (distanceFromOthers > Core.Settings.ConvoyMaxDistFromOther) { return false; }
        float distanceFromPlayer = convoyActorPathing.getDistanceFromPlayer(pos);
        //Log.WL(1, "distanceFromPlayer:" + distanceFromPlayer + " / " + Core.Settings.ConvoyMaxDistFromPlayer);
        if (distanceFromPlayer > Core.Settings.ConvoyMaxDistFromPlayer) { return false; }
      }
      return true;
    }
    public static void Postfix(PathNodeGrid __instance, PathNode from, PathNode to, float distanceAvailable, AbstractActor ___owningActor, CombatGameState ___combat, ref float __result) {
      //try {
      //if (__result > distanceAvailable) { return; }
      //if (___owningActor.isValidConvoyPosition(to.Position) == false) {
      //__result = 9999.9f;
      //return;
      //}
      //} catch (Exception e) {
      //Log.TWL(0, e.ToString(), true);
      //}
    }
  }
  [HarmonyPatch(typeof(JumpPathing))]
  [HarmonyPatch("IsValidLandingSpot")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(List<AbstractActor>) })]
  public static class JumpPathing_IsValidLandingSpot {
    public static void Postfix(JumpPathing __instance, Vector3 worldPos, List<AbstractActor> allActors, ref bool __result) {
      //if (__result == false) { return; }
      //try {
      //if (Traverse.Create(__instance).Property<Mech>("Mech").Value.isValidConvoyPosition(worldPos) == false) { __result = false; return; }
      //}catch(Exception e) {
      //Log.TWL(0,e.ToString(),true);
      //}
    }
  }
  //[HarmonyPatch(typeof(EncounterChunkGameLogic))]
  //[HarmonyPatch("EncounterStart")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class EncounterChunkGameLogic_EncounterStart {
  //  public static void Postfix(EncounterChunkGameLogic __instance) {
  //    EscortChunkGameLogic escortChunkGameLogic = __instance as EscortChunkGameLogic;
  //    if (escortChunkGameLogic == null) { return; }
  //    Log.TWL(0, "EscortChunkGameLogic.EncounterStart");
  //    __instance.Combat.FillRoutePoints();
  //  }
  //}
  [HarmonyPatch(typeof(MechJumpSequence))]
  [HarmonyPatch("OnAdded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechJumpSequence_OnAdded {
    public static void Postfix(ActorMovementSequence __instance) {
      //try {
      //  if (__instance.OwningActor.TeamId != __instance.OwningActor.Combat.LocalPlayerTeamGuid) { return; }
      //  foreach (AbstractActor unit in __instance.OwningActor.Combat.AllActors) {
      //    if (unit.IsDead) { continue; }
      //    if (unit.TeamId != __instance.OwningActor.Combat.LocalPlayerTeamGuid) { continue; }
      //    if (unit.EncounterTags.Contains(Core.Settings.PlayerControlConvoyTag) == false) { continue; }
      //    if (unit.HasMovedThisRound) { continue; }
      //    unit.ResetPathing();
      //    unit.UpdateConvoyPathingCache();
      //  }
      //} catch (Exception e) {
      //  Log.TWL(0, e.ToString(), true);
      //}
    }
  }
  [HarmonyPatch(typeof(EncounterLayerParent))]
  [HarmonyPatch("InitializeContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class CombatGameState_InitializeContract {
    public static string ContentToString(this HBS.Collections.TagSet tags) {
      StringBuilder result = new StringBuilder();
      foreach (var val in tags) {
        result.Append(val); result.Append(";");
      }
      return result.ToString();
    }
    public static void Postfix(EncounterLayerParent __instance, MessageCenterMessage message) {
      try {
        InitializeContractMessage initializeContractMessage = message as InitializeContractMessage;
        CombatGameState combat = initializeContractMessage.combat;
        Contract activeContract = combat.ActiveContract;
        Log.TWL(0, "LanceSpawnerList:" + Traverse.Create(combat.EncounterLayerData).Property("LanceSpawnerList").GetValue<LanceSpawnerGameLogic[]>().Length);
        foreach (LanceSpawnerGameLogic lanceSpawner in Traverse.Create(combat.EncounterLayerData).Property("LanceSpawnerList").GetValue<LanceSpawnerGameLogic[]>()) {
          Log.WL(1, "Name:" + lanceSpawner.Name + "/" + lanceSpawner.DisplayName);
          Log.WL(2, "Lance:" + lanceSpawner.LanceGuid + "/" + lanceSpawner.spawnEffectTags.ContentToString());
          Log.WL(2, "encounterObjectName:" + lanceSpawner.encounterObjectName);
          Team oldTeam = combat.TurnDirector.GetTurnActorByUniqueId(lanceSpawner.teamDefinitionGuid) as Team;
          Team playerTeam = combat.LocalPlayerTeam;
          Lance lance = null;
          if (lanceSpawner.spawnEffectTags.Contains(Core.Settings.PlayerControlConvoyTag) == false) {
            Log.WL(2, "spawnEffectTags not contains:" + Core.Settings.PlayerControlConvoyTag);
            continue;
          };
          lanceSpawner.spawnEffectTags.Remove(Core.Settings.PlayerControlConvoyTag);
          if ((oldTeam == null) || (playerTeam == null)) {
            Log.WL(2, "oldTeam:" + (oldTeam == null ? "null" : oldTeam.DisplayName) + " new team:" + (playerTeam == null ? "null" : playerTeam.DisplayName));
            continue;
          }
          if (!string.IsNullOrEmpty(lanceSpawner.LanceGuid)) {
            lance = combat.ItemRegistry.GetItemByGUID<Lance>(lanceSpawner.LanceGuid);
          }
          if (lance == null) {
            Log.WL(2, "lance is null");
            continue;
          }
          lanceSpawner.teamDefinitionGuid = combat.LocalPlayerTeamGuid;
          lance.team = playerTeam;
          typeof(Lance).GetMethod("InitVisibilityCache", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(lance, new object[] { });
          oldTeam.lances.Remove(lance);
          playerTeam.lances.Add(lance);
          //lanceSpawner.EncounterTags.Remove(oldTeam.Name);
          //lanceSpawner.EncounterTags.Add(playerTeam.Name);
          Log.WL(2, "EncounterTags:" + lanceSpawner.EncounterTags.ContentToString());
          Log.WL(2, "Position:" + lanceSpawner.Position);
          Log.WL(2, "Team:" + lanceSpawner.teamDefinitionGuid);
          Log.WL(2, "unitSpawnPointGameLogicList:" + lanceSpawner.unitSpawnPointGameLogicList.Length);
          foreach (UnitSpawnPointGameLogic unitSpawner in lanceSpawner.unitSpawnPointGameLogicList) {
            Log.WL(3, "Name:" + unitSpawner.Name + "/" + unitSpawner.DisplayName);
            unitSpawner.SetTeamDefinitionGuid(combat.LocalPlayerTeamGuid);
            unitSpawner.spawnEffectTags.Remove(Core.Settings.PlayerControlConvoyTag);
            unitSpawner.EncounterTags.Remove(oldTeam.Name);
            unitSpawner.EncounterTags.Add(playerTeam.Name);
            unitSpawner.EncounterTags.Add(Core.Settings.PlayerControlConvoyTag);
            unitSpawner.EncounterTags.Add(Core.Settings.TransferTeamOnDeathPrefixTag + oldTeam.GUID);
            Log.WL(4, "unitTagSet:" + unitSpawner.unitTagSet.ContentToString());
            Log.WL(4, "UnitDefId:" + unitSpawner.UnitDefId);
            Log.WL(4, "encounterObjectName:" + unitSpawner.encounterObjectName);
            Log.WL(4, "EncounterTags:" + unitSpawner.EncounterTags.ContentToString());
            Log.WL(4, "Position:" + unitSpawner.Position);
            Log.WL(4, "Team:" + unitSpawner.team);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}