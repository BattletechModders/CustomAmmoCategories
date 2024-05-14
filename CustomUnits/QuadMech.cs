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
using CustAmmoCategories;
using CustomUnits;
using HBS.Collections;
using IRBTModUtils;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits { 
  public class QuadMech : CustomMech, ICustomMech {
    public static List<ArmorLocation> locations = new List<ArmorLocation>() { ArmorLocation.Head, ArmorLocation.LeftLeg, ArmorLocation.RightLeg, ArmorLocation.LeftArm, ArmorLocation.RightArm };
    public QuadMech(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
      : base(mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef) {

    }
    public override bool _MoveMultiplierOverride { get { return true; } }
    public override float _MoveMultiplier { get { return base._MoveMultiplier; } }
    public override bool isReallyDead {
      get {
        if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.TWL(0, $"QuadMech.IsReallyDead {this.PilotableActorDef.ChassisID}"); }
        if (this.HasHandledDeath) { if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, "HasHandledDeath"); }; return true; }
        if (this.pilot.IsIncapacitated) { if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, "Pilot.IsIncapacitated"); }; return true; }
        if (this.pilot.HasEjected) { if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, "Pilot.HasEjected"); }; return true; }
        if (this.CenterTorsoStructure <= Core.Epsilon) { if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, "CT Destruction"); }; return true; }
        int DestroyedLegsCount = this.DestroyedLegsCount();
        if (DestroyedLegsCount > 2) {
          if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, $"legs destroyed: {DestroyedLegsCount}"); }
          return true;
        }
        UnitCustomInfo info = this.GetCustomInfo();
        if (info != null) {
          foreach (ChassisLocations location in info.lethalLocations) {
            if (this.GetCurrentStructure(location) <= Core.Epsilon) {
              if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, $"vital location {location} destroyed"); }
              return true;
            }
          }
        }
        ChassisLocations crewLocation = this.CrewLocationChassis();
        if (crewLocation != ChassisLocations.None) {
          if (this.IsLocationDestroyed(crewLocation)) {
            if (ICustomMechDebug.IS_DEAD_DEBUG) { Log.Combat?.WL(1, $"crew location destroyed: {crewLocation}"); }
            return true;
          }
        }
        return false;
      }
    }
    private static HashSet<ArmorLocation> DFASelfDamageLocations = new HashSet<ArmorLocation>() { ArmorLocation.LeftArm, ArmorLocation.RightArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    public override HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      return DFASelfDamageLocations;
    }
    private static HashSet<ArmorLocation> LandmineDamageArmorLocations = new HashSet<ArmorLocation>() { ArmorLocation.LeftArm, ArmorLocation.RightArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    public override HashSet<ArmorLocation> GetLandmineDamageArmorLocations() {
      return LandmineDamageArmorLocations;
    }
    public override HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear,
          ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear, ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear,
          ArmorLocation.RightArm, ArmorLocation.LeftArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg
      };
    }
    public override string UnitTypeNameDefault { get { return "QUAD"; } }

    public override bool isQuad { get { return true; } }
  }
}