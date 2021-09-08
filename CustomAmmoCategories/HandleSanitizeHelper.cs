﻿using BattleTech;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class HandleSanitiseHelper {
    public static void HandleSanitize(this CombatGameState combat, bool checkForStability = false, bool checkForPilot = false){
      List<AbstractActor> allActors = combat.AllActors;
      Log.M.TWL(0, "HandleSanitize:"+allActors.Count);
      foreach(AbstractActor actor in allActors) {
        Log.M.WL(1, actor.DisplayName+":"+actor.GUID+ " IsDead:"+actor.IsDead+ " HasHandledDeath:"+ actor.HasHandledDeath+ " isHasStability:"+actor.isHasStability());
        if ((actor.HasHandledDeath == false)&&(actor.IsDead == true)) {
          
          if(actor is Mech mech) {
            Log.M.WL(2, "pilot.IsIncapacitated:"+ mech.pilot.IsIncapacitated);
            Log.M.WL(2, "pilot.HasEjected:" + mech.pilot.HasEjected);
            Log.M.WL(2, "HeadStructure:" + mech.HeadStructure);
            Log.M.WL(2, "CenterTorsoStructure:" + mech.CenterTorsoStructure);
            Log.M.WL(2, "LeftLegStructure:" + mech.LeftLegStructure);
            Log.M.WL(2, "RightLegStructure:" + mech.RightLegStructure);
          }
          int deathLocation = 0;
          switch (actor.UnitType) {
            case UnitType.Building: deathLocation = (int)BuildingLocation.Structure; break;
            case UnitType.Mech: deathLocation = (int)ChassisLocations.CenterTorso; break;
            case UnitType.Vehicle: deathLocation = (int)VehicleChassisLocations.Front; break;
          }
          actor.FlagForDeath("DEATH", DeathMethod.VitalComponentDestroyed, DamageType.Enemy, deathLocation, -1, string.Empty, false);
          actor.HandleDeath("DEZOMBIFICATOR");
        }
        if (actor.IsDead == false) {
          if (checkForStability) {
            Mech mech = actor as Mech;
            if (mech != null) {
              if (mech.isHasStability()) { mech.CheckForInstability(); mech.HandleKnockdown(-1, "DEZOMBIFICATOR", Vector2.zero, null); }
            }
          }
          if (checkForPilot) {
            actor.CheckPilotStatusFromAttack("DEZOMBIFICATOR", -1, -1);
            actor.HandleDeath("DEZOMBIFICATOR");
          }
        }
      }
    }
  }
}