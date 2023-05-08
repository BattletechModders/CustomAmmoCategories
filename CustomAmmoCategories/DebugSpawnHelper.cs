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
using BattleTech.Data;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class VehicleSpawnerRecord {
    public VehicleDef vDef;
    public Team team;
    public Lance lance;
    public Vector3 pos;
    public PilotDef pilot;
    public string spawnerGUID;
    public int count;
    public VehicleSpawnerRecord(VehicleDef def,PilotDef pilot, string sg,Team t,Lance l,int c,Vector3 bp) {
      this.vDef = def; this.pilot = pilot; this.spawnerGUID = sg; this.team = t; this.lance = l;this.count = c;this.pos = bp;
    }
    public void OnDepLoaded() {
      Log.Combat?.WL(0,"Dependencies for "+vDef.Description.Id+" loaded. Spawning:"+count+" of "+vDef.Description.Id);
      for (int t = 0; t < count; ++t) {
        Vector3 spawnPos = SpawnVehicleDialogHelper.GetSpawnPosition(pos);
        if (spawnPos == Vector3.zero) {
          break;
        }
        Vehicle vehicle = ActorFactory.CreateVehicle(vDef, pilot, team.EncounterTags, SpawnVehicleDialogHelper.Dialog.Combat, Guid.NewGuid().ToString(), spawnerGUID, team.HeraldryDef);
        Log.Combat?.WL(1, "vehicle created:" + vehicle.DisplayName + ":" + vehicle.GUID);
        vehicle.Init(spawnPos, 0f, true);
        Log.Combat?.WL(1, "vehicle inited. Initing gameRep");
        try {
          vehicle.InitGameRep((Transform)null);
          team.SupportTeam.AddUnit(vehicle);
          lance.AddUnitGUID(vehicle.GUID);
          vehicle.AddToLance(lance);
          vehicle.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
          UnitSpawnedMessage unitSpawnedMessage = new UnitSpawnedMessage(spawnerGUID, vehicle.GUID);
          vehicle.OnPositionUpdate(spawnPos, Quaternion.identity, -1, true, (List<DesignMaskDef>)null, false);
          vehicle.BehaviorTree = BehaviorTreeFactory.MakeBehaviorTree(SpawnVehicleDialogHelper.Dialog.Combat.BattleTechGame, vehicle, BehaviorTreeIDEnum.CoreAITree);
          SpawnVehicleDialogHelper.Dialog.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new UnitSpawnedMessage("DEBUG_SPAWNER", vehicle.GUID));
          Log.Combat?.WL(0, "spawn success:" + vehicle.DisplayName + ":" + vehicle.GUID);
        } catch (Exception e) {
          Log.Combat?.WL(0, e.ToString());
          break;
        };
      }
    }
  }
  public static class SpawnVehicleDialogHelper {
    public static SpawnVehicleDialog Dialog = new SpawnVehicleDialog();
    public static Vector3 lastTerrainHitPosition = Vector3.zero;
    public static Vector3 GetSpawnPosition(Vector3 mainPos) {
      if (mainPos == Vector3.zero) { return Vector3.zero; };
      TerrainMaskFlags mask = TerrainMaskFlags.Impassable;
      int watchdog = 0;
      do {
        mainPos.x += Random.Range(-30f, 30f);
        mainPos.z += Random.Range(-30f, 30f);
        mainPos.y = Dialog.Combat.MapMetaData.GetLerpedHeightAt(mainPos);
        mask = Dialog.Combat.MapMetaData.GetPriorityTerrainMaskFlags(mainPos);
        ++watchdog;
        if (watchdog > 20) { return Vector3.zero; };
      } while ((mask == TerrainMaskFlags.Impassable)||(mask == TerrainMaskFlags.MapBoundary));
      return mainPos;
    }
    public static void SpawnSelected(int terrainAttackSeqId) {
      Team spawnTeam = null;
      Vector3 pos = SpawnVehicleDialogHelper.lastTerrainHitPosition;
      SpawnVehicleDialogHelper.lastTerrainHitPosition = Vector3.zero;
      if (Dialog.vehiclesCount.Count == 0) { return; };
      Log.Combat?.WL(0, "SpawnVehicleDialogHelper.SpawnSelected");
      string spawnerGUID = string.Empty;
      PilotDef pilot = null;
      foreach (Team team in Dialog.Combat.Teams) {
        Log.Combat?.WL(1, "team " + team.GUID + ":" + team.HeraldryDef.Description.Name + ":" + team.HeraldryDef.Description.Id);
        if (team.LocalPlayerControlsTeam == true) {
          Log.Combat?.WL(2, "palyer\n");
          continue;
        };
        if (team.unitCount == 0) {
          Log.Combat?.WL(2, "no units");
          continue;
        };
        Log.Combat?.WL(2, "units:");
        bool NoPilotable = true;
        foreach (AbstractActor unit in team.units) {
          Log.Combat?.WL(3, unit.DisplayName+":"+unit.GUID+":"+unit.spawnerGUID+" pilotable:"+unit.IsPilotable);
          if (unit.IsPilotable == false) {continue;}
          spawnerGUID = unit.spawnerGUID;
          pilot = unit.GetPilot().pilotDef;
          Log.Combat?.WL(3, "pilot:" + (pilot == null?"null":pilot.Description.Id));
          if (pilot != null) { NoPilotable = false; break; };
        }
        if (NoPilotable == false) {
          spawnTeam = team;
          break;
        } else {
          Log.Combat?.WL(2, "team not contains pilotable units");
        }
      }
      if (spawnTeam == null) { Log.LogWrite(" can't find team to spawn\n"); Dialog.vehiclesCount.Clear(); return; }
      Log.Combat?.WL(1, "spawn team found " + spawnTeam.GUID+":"+spawnTeam.HeraldryDef.Description.Name+":"+ spawnTeam.HeraldryDef.Description.Id + " lances:"+ spawnTeam.lances.Count);
      Lance spawnLance = null;
      foreach (Lance lance in spawnTeam.lances) {
        spawnLance = lance;
        break;
      }
      if (spawnLance == null) { Log.Combat?.WL(1, "can't find lance to spawn"); Dialog.vehiclesCount.Clear(); return; }
      Log.Combat?.WL(1, "spawn lance found " + spawnLance.GUID + ":" + spawnLance.DisplayName + ":" + spawnLance.Type);
      if (string.IsNullOrEmpty(spawnerGUID)) { Log.LogWrite(" can't find spawner GUID\n"); Dialog.vehiclesCount.Clear(); return; }
      Log.Combat?.WL(1, "spawner GUID " + spawnerGUID);
      if (pilot == null) { Log.Combat?.WL(1, "can't find spawn pilot"); Dialog.vehiclesCount.Clear(); return; }
      Log.Combat?.WL(1, "spawn pilot " + pilot.Description.Id);
      foreach (var spawnCount in Dialog.vehiclesCount) {
        if (spawnCount.Value == 0) { continue; }
        Log.Combat?.WL(1, "spawn position:" + pos);
        VehicleDef vDef = spawnCount.Key;
        if (vDef.DependenciesLoaded(1000U) == false) {
          DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(SpawnVehicleDialogHelper.Dialog.Combat.DataManager);
          vDef.GatherDependencies(SpawnVehicleDialogHelper.Dialog.Combat.DataManager, (DataManager.DependencyLoadRequest)dependencyLoad, 1000U);
          VehicleSpawnerRecord spawner = new VehicleSpawnerRecord(vDef, pilot, spawnerGUID, spawnTeam, spawnLance, spawnCount.Value, pos);
          dependencyLoad.RegisterLoadCompleteCallback(new Action(spawner.OnDepLoaded));
          SpawnVehicleDialogHelper.Dialog.Combat.DataManager.InjectDependencyLoader(dependencyLoad, 1000U);
        } else {
          VehicleSpawnerRecord spawner = new VehicleSpawnerRecord(vDef, pilot, spawnerGUID, spawnTeam, spawnLance, spawnCount.Value, pos);
          spawner.OnDepLoaded();
        }
      }
      Dialog.vehiclesCount.Clear();
    }
  }
  public class SpawnVehicleDialog {
    public static readonly int MaxLinesOnPage = 10;
    public GenericPopup popup;
    public List<VehicleDef> vehicles;
    public Dictionary<VehicleDef,int> vehiclesCount;
    public int SelectedVehicle;
    public CombatGameState Combat;
    public SpawnVehicleDialog() {
      this.Combat = null;
      SelectedVehicle = 0;
      vehicles = new List<VehicleDef>();
      vehiclesCount = new Dictionary<VehicleDef, int>();
      popup = null;
    }
    public void Clear() {
      popup = null;
      vehicles.Clear();
      vehiclesCount.Clear();
    }
    public void Render(CombatGameState Combat) {
      this.Combat = Combat;
      vehicles.Clear();
      vehiclesCount.Clear();
      foreach (var vh in Combat.DataManager.VehicleDefs) { vehicles.Add(vh.Value); }
      GenericPopupBuilder builder = GenericPopupBuilder.Create("Components", this.BuildText());
      builder.AddButton("X", new Action(this.Clear), true);
      builder.AddButton("-", new Action(this.Left), false);
      builder.AddButton("<-", new Action(this.Up), false);
      builder.AddButton("+", new Action(this.Right), false);
      builder.AddButton("->", new Action(this.Down), false);
      builder.AddButton("Ok", null, true);
      popup = builder.CancelOnEscape().Render();
    }
    public void Left() {
      if (vehiclesCount.ContainsKey(this.vehicles[this.SelectedVehicle])) {
        if(vehiclesCount[this.vehicles[this.SelectedVehicle]] > 0) {
          vehiclesCount[this.vehicles[this.SelectedVehicle]] -= 1;
          popup.TextContent = this.BuildText();
        }
      }
    }
    public void Right() {
      if (vehiclesCount.ContainsKey(this.vehicles[this.SelectedVehicle]) == false) {
        vehiclesCount.Add(this.vehicles[this.SelectedVehicle], 1);
        popup.TextContent = this.BuildText();
        return;
      } else 
      if (vehiclesCount[this.vehicles[this.SelectedVehicle]] < 4) {
        vehiclesCount[this.vehicles[this.SelectedVehicle]] += 1;
        popup.TextContent = this.BuildText();
        return;
      }
    }
    public string BuildText() {
      StringBuilder builder = new StringBuilder();
      int indexStart = SelectedVehicle - SelectedVehicle % MaxLinesOnPage;
      int indexEnd = indexStart + MaxLinesOnPage;
      if (indexEnd > vehicles.Count) { indexEnd = vehicles.Count; };
      int pagesCount = vehicles.Count / MaxLinesOnPage + ((vehicles.Count % MaxLinesOnPage) > 0 ? 1 : 0);
      int page = SelectedVehicle / MaxLinesOnPage;
      builder.Append("Page "+page+"/"+ pagesCount);
      for (int index = indexStart; index < indexEnd; ++index) {
        builder.Append("\n");
        if (index == SelectedVehicle) { builder.Append("->"); }
        VehicleDef vd = vehicles[index];
        builder.Append(vd.Description.Id);
        if (vehiclesCount.ContainsKey(vd)) {
          builder.Append(" "+ vehiclesCount[vd]);
        } else {
          builder.Append(" 0");
        }
      }
      return builder.ToString();
    }
    public void Up() {
      if (SelectedVehicle > 0) {
        SelectedVehicle -= 1;
      } else {
        SelectedVehicle = this.vehicles.Count - 1;
      }
      popup.TextContent = this.BuildText();
    }
    public void Down() {
      if (SelectedVehicle < this.vehicles.Count - 1) {
        SelectedVehicle += 1;
      } else {
        SelectedVehicle = 0;
      }
      popup.TextContent = this.BuildText();
    }
  }
}