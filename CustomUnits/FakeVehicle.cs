﻿using BattleTech;
using CustAmmoCategories;
using Harmony;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("WalkSpeed")]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_WalkSpeed {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (__instance is FakeVehicleMech vehicle) {
          __result = vehicle.CruiseSpeed;
          Log.TWL(0, "FakeVehicleMech.WalkSpeed " + __instance.MechDef.Description.Id+" "+__result);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch("RunSpeed")]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_RunSpeed {
    public static void Postfix(Mech __instance, ref float __result) {
      try {
        if (__instance is FakeVehicleMech vehicle) {
          __result = vehicle.FlankSpeed;
          Log.TWL(0, "FakeVehicleMech.RunSpeed " + __instance.MechDef.Description.Id + " " + __result);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class FakeVehicleMech: CustomMech, ICustomMech{
    public static List<ArmorLocation> locations = new List<ArmorLocation>() { ArmorLocation.Head, ArmorLocation.LeftLeg, ArmorLocation.RightLeg, ArmorLocation.LeftArm, ArmorLocation.RightArm };
    public FakeVehicleMech(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
      : base(mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef) {
    }
    public override bool _MoveMultiplierOverride { get { return true; } }
    public override float _MoveMultiplier { get { return 1f; } }
    public override bool IsDead {
      get {
        if (this.HasHandledDeath) { return true; }
        if (this.pilot.IsIncapacitated) { return true; }
        if (this.pilot.HasEjected) { return true; }
        foreach (ArmorLocation alocation in FakeVehicleMech.locations) {
          ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
          LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
          if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
          if (this.GetCurrentStructure(location) <= Core.Epsilon) { return true; }
        }
        return false;
      }
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      List<int> result = new List<int>();
      foreach (ArmorLocation alocation in FakeVehicleMech.locations) {
        ChassisLocations location = MechStructureRules.GetChassisLocationFromArmorLocation(alocation);
        LocationDef locDef = this.MechDef.Chassis.GetLocationDef(location);
        if ((locDef.MaxArmor <= 0f) && (locDef.InternalStructure <= 1f)) { continue; }
        result.Add((int)alocation);
      }
      return result;
    }
    private static Dictionary<AttackDirection, Dictionary<ArmorLocation, int>> GetHitTable_cache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
    public override Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      if (from == AttackDirection.ToProne) { from = AttackDirection.FromArtillery; };
      Dictionary<ArmorLocation, int> result = null;
      if (GetHitTable_cache.TryGetValue(from, out result)) { return result; }
      result = new Dictionary<ArmorLocation, int>();
      Dictionary<VehicleChassisLocations, int> vres = this.Combat.HitLocation.GetVehicleHitTable(from);
      foreach (var vloc in vres) {
        ArmorLocation aloc = vloc.Key.toFakeArmor();
        result.Add(aloc, vloc.Value);
      }
      GetHitTable_cache.Add(from,result);
      return result.Count > 0 ? result : null;
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      return (int)this.Combat.HitLocation.GetHitLocationFakeVehicle(attackPosition, this, hitLocationRoll, ((ArmorLocation)calledShotLocation).toFakeVehicleChassis(), bonusMultiplier).toFakeArmor();
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      return (int)this.Combat.HitLocation.GetAdjacentHitLocation(attackPosition, this, randomRoll, (ArmorLocation)previousHitLocation, originalMultiplier, adjacentMultiplier, ArmorLocation.None, 0.0f);
    }

    private static HashSet<ArmorLocation> DFASelfDamageLocations = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      return DFASelfDamageLocations;
    }
    private static HashSet<ArmorLocation> LandmineDamageArmorLocations = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetLandmineDamageArmorLocations() {
      return LandmineDamageArmorLocations;
    }
    private static HashSet<ArmorLocation> BurnDamageArmorLocations_Turret = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor(), VehicleChassisLocations.Turret.toFakeArmor() };
    private static HashSet<ArmorLocation> BurnDamageArmorLocations_TurretLess = new HashSet<ArmorLocation>() { VehicleChassisLocations.Front.toFakeArmor(), VehicleChassisLocations.Rear.toFakeArmor(), VehicleChassisLocations.Left.toFakeArmor(), VehicleChassisLocations.Right.toFakeArmor() };
    public override HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      LocationDef def = this.MechDef.Chassis.GetLocationDef(VehicleChassisLocations.Turret.toFakeChassis());
      if ((def.MaxArmor == 0f) && (def.InternalStructure <= 1f)) { return BurnDamageArmorLocations_TurretLess; }
      return BurnDamageArmorLocations_Turret;
    }
    public override Dictionary<int, float> GetAOESpreadArmorLocations() {
      return CustomAmmoCategories.FakeVehicleLocations;
    }
    public override List<int> GetAOEPossibleHitLocations(Vector3 attackPos) {
      Dictionary<ArmorLocation, int> vehicleHitTable = this.GetHitTable(this.Combat.HitLocation.GetAttackDirection(attackPos, (ICombatant)this));
      if (vehicleHitTable == null) return (List<int>)null;
      List<int> intList = new List<int>();
      foreach (ArmorLocation key in vehicleHitTable.Keys) {
        ChassisLocations chassisLoc = MechStructureRules.GetChassisLocationFromArmorLocation(key);
        LocationDef location = this.MechDef.Chassis.GetLocationDef(chassisLoc);
        if ((location.MaxArmor <= 0f) && (location.InternalStructure <= 1f)) { continue; }
        intList.Add((int)key);
      }
      return intList;
    }
    public override Text GetLongArmorLocation(ArmorLocation location) {
      return new Text(Vehicle.GetLongChassisLocation(location.toFakeVehicleChassis()));
    }
    private static Dictionary<ArmorLocation, ArmorLocation> GetAdjacentLocations_cache = new Dictionary<ArmorLocation, ArmorLocation>();
    public override ArmorLocation GetAdjacentLocations(ArmorLocation location) {
      if (GetAdjacentLocations_cache.TryGetValue(location, out ArmorLocation result)) { return result; }
      switch (location.toFakeVehicleChassis()) {
        case VehicleChassisLocations.Turret:
          result = VehicleChassisLocations.Rear.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Front.toFakeArmor();
        break;
        case VehicleChassisLocations.Front:
        case VehicleChassisLocations.Rear:
          result = VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Left.toFakeArmor() | VehicleChassisLocations.Right.toFakeArmor();
        break;
        case VehicleChassisLocations.Left:
        case VehicleChassisLocations.Right:
          result = VehicleChassisLocations.Turret.toFakeArmor() | VehicleChassisLocations.Front.toFakeArmor() | VehicleChassisLocations.Rear.toFakeArmor();
        break;
        default:
        return VehicleChassisLocations.None.toFakeArmor();
      }
      GetAdjacentLocations_cache.Add(location, result);
      return result;
    }
    private static Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> GetClusterTable_cache = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
    public override Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      if (GetClusterTable_cache.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      ArmorLocation adjacentLocations = this.GetAdjacentLocations(originalLocation);
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (originalLocation == keyValuePair.Key) {
          dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
        } else if ((adjacentLocations & keyValuePair.Key) == keyValuePair.Key) {
          dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
        } else { 
          dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceNonadjacentMultiplier));
        }
      }
      GetClusterTable_cache.Add(originalLocation, dictionary);
      return dictionary;
    }
    private static Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterHitTable_cache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public override Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      if (GetClusterHitTable_cache.TryGetValue(from, out Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        GetClusterHitTable_cache.Add(from, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      return result;
    }
    public override bool isSquad { get { return false; } }
    public override bool isVehicle { get { return true; } }
    public float CruiseSpeed => this.statCollection.GetValue<float>(nameof(CruiseSpeed));
    public float FlankSpeed => this.statCollection.GetValue<float>(nameof(FlankSpeed));
    public override void ApplyBraced() {}
    public override bool CanBeEntrenched { get { return false; } }
    public override string UnitTypeNameDefault { get { return "VEHICLE"; } }

    protected override void InitStats() {
      Log.TWL(0, "FakeVehicleMech.InitStats");
      this.statCollection.AddStatistic<float>("CruiseSpeed", this.MovementCaps.MaxWalkDistance);
      this.statCollection.AddStatistic<float>("FlankSpeed", this.MovementCaps.MaxSprintDistance);
      base.InitStats();
    }
  }
}