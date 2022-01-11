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
    public override bool IsDead {
      get {
        if (this.HasHandledDeath) { return true; }
        if (this.pilot.IsIncapacitated) { return true; }
        if (this.pilot.HasEjected) { return true; }
        if (this.HeadStructure < Core.Epsilon) { return true; }
        if (this.DestroyedLegsCount() > 2) { return true; }
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