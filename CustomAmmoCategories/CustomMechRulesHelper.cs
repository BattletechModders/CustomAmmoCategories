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
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategories {
  public static class CustomGetHitTableHelper {
    private static Dictionary<string,Func<ICustomMech, Weapon, int, Dictionary<ArmorLocation, int>, Dictionary<ArmorLocation, int>>> GetUnitHitTableFilters = new Dictionary<string,Func<ICustomMech, Weapon, int, Dictionary<ArmorLocation, int>, Dictionary<ArmorLocation, int>>>();
    public static void RegisterFilter(string id, Func<ICustomMech, Weapon, int, Dictionary<ArmorLocation, int>, Dictionary<ArmorLocation, int>> filter) {
      if(GetUnitHitTableFilters.ContainsKey(id) == false) {
        GetUnitHitTableFilters.Add(id,filter);
      } else {
        GetUnitHitTableFilters[id] = filter;
      }
    }
    public static Dictionary<ArmorLocation, int> InvokeFilters(Dictionary<ArmorLocation, int> inital) {
      if (GetUnitHitTableFilters.Count == 0) { return inital; }
      ICustomMech mech = Thread.CurrentThread.currentActor() as ICustomMech;
      Weapon weapon = Thread.CurrentThread.currentWeapon();
      int attackSeqId = Thread.CurrentThread.currentAttackSequence();
      Dictionary<ArmorLocation, int> result = new Dictionary<ArmorLocation, int>();
      foreach (var r in inital) { result.Add(r.Key, r.Value); }
      foreach (var filter in GetUnitHitTableFilters) {
        result = filter.Value(mech, weapon, attackSeqId, result);
      }
      return result;
    }
  }
  public static class FakeVehicleLocationConvertHelper {
    public static VehicleChassisLocations vehicleLocationFromMechLocation(ChassisLocations location) {
      switch (location) {
        case ChassisLocations.Head: return VehicleChassisLocations.Turret;
        case ChassisLocations.LeftArm: return VehicleChassisLocations.Front;
        case ChassisLocations.RightArm: return VehicleChassisLocations.Rear;
        case ChassisLocations.LeftLeg: return VehicleChassisLocations.Left;
        case ChassisLocations.RightLeg: return VehicleChassisLocations.Right;
        default: return VehicleChassisLocations.None;
      }
    }
    public static VehicleChassisLocations vehicleLocationFromArmorLocation(ArmorLocation location) {
      switch (location) {
        case BattleTech.ArmorLocation.Head: return VehicleChassisLocations.Turret;
        case BattleTech.ArmorLocation.LeftArm: return VehicleChassisLocations.Front;
        case BattleTech.ArmorLocation.RightArm: return VehicleChassisLocations.Rear;
        case BattleTech.ArmorLocation.LeftLeg: return VehicleChassisLocations.Left;
        case BattleTech.ArmorLocation.RightLeg: return VehicleChassisLocations.Right;
        default: return VehicleChassisLocations.None;
      }
    }
    public static ArmorLocation armorLocationFromVehicleLocation(VehicleChassisLocations location) {
      switch (location) {
        case VehicleChassisLocations.Turret: return BattleTech.ArmorLocation.Head;
        case VehicleChassisLocations.Front: return BattleTech.ArmorLocation.LeftArm;
        case VehicleChassisLocations.Rear: return BattleTech.ArmorLocation.RightArm;
        case VehicleChassisLocations.Left: return BattleTech.ArmorLocation.LeftLeg;
        case VehicleChassisLocations.Right: return BattleTech.ArmorLocation.RightLeg;
        default: return BattleTech.ArmorLocation.None;
      }
    }
    public static ChassisLocations chassisLocationFromVehicleLocation(VehicleChassisLocations location) {
      switch (location) {
        case VehicleChassisLocations.Turret: return ChassisLocations.Head;
        case VehicleChassisLocations.Front: return ChassisLocations.LeftArm;
        case VehicleChassisLocations.Rear: return ChassisLocations.RightArm;
        case VehicleChassisLocations.Left: return ChassisLocations.LeftLeg;
        case VehicleChassisLocations.Right: return ChassisLocations.RightLeg;
        default: return ChassisLocations.None;
      }
    }
    public static ArmorLocation toFakeArmor(this VehicleChassisLocations location) { return armorLocationFromVehicleLocation(location); }
    public static ChassisLocations toFakeChassis(this VehicleChassisLocations location) { return chassisLocationFromVehicleLocation(location); }
    public static VehicleChassisLocations toFakeVehicleChassis(this ChassisLocations location) { return vehicleLocationFromMechLocation(location); }
    public static VehicleChassisLocations toFakeVehicleChassis(this ArmorLocation location) { return vehicleLocationFromArmorLocation(location); }
  }
  public static class GetLongArmorLocationHelper {
    public static Text GetLongArmorLocation(this Mech mech, ArmorLocation location) {
      Thread.CurrentThread.pushActor(mech);
      Text result = Mech.GetLongArmorLocation(location);
      Thread.CurrentThread.clearActor();
      return result;
    }
  }
  public static class GetDFASelfDamageLocationsHelper {
    public static HashSet<ArmorLocation> GetDFASelfDamageLocations(this Mech mech) {
      ICustomMech customMech = mech as ICustomMech;
      if (customMech != null) { return customMech.GetDFASelfDamageLocations(); }
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }
  }
  public static class GetLandmineDamageLocationsHelper {
    public static HashSet<ArmorLocation> GetLandmineDamageArmorLocations(this Mech mech) {
      ICustomMech customMech = mech as ICustomMech;
      if (customMech != null) { return customMech.GetLandmineDamageArmorLocations(); }
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }
  }
  public static class GetBurnDamageLocationsHelper {
    private static Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>> GetBurnDamageLocationsFunctions = new Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>>();
    public static void RegisterBurnDamageLocationsFunction(Type t, Func<Mech, HashSet<ArmorLocation>> func) {
      GetBurnDamageLocationsFunctions.Add(t, func);
    }
    public static HashSet<ArmorLocation> GetBurnDamageArmorLocations(this Mech mech) {
      ICustomMech customMech = mech as ICustomMech;
      if (customMech != null) { return customMech.GetLandmineDamageArmorLocations(); }
      return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear,
          ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear, ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear,
          ArmorLocation.RightArm, ArmorLocation.LeftArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg
      };
    }
  }
  public static class GetAOESpreadLocationsHelper {
    public static Dictionary<int, float> GetAOESpreadArmorLocations(this Mech mech) {
      ICustomMech customMech = mech as ICustomMech;
      if (customMech != null) { return customMech.GetAOESpreadArmorLocations(); }
      return CustomAmmoCategories.NormMechHitLocations;
    }
  }
  public static class GetAOEPossibleHitLocationsHelper {
    public static List<int> GetAOEPossibleHitLocations(this Mech mech, Vector3 attackPos) {
      List<int> result = null;
      ICustomMech customMech = mech as ICustomMech;
      if (customMech != null) { return customMech.GetAOEPossibleHitLocations(attackPos); }
      Thread.CurrentThread.pushActor(mech);
      result = mech.Combat.HitLocation.GetPossibleHitLocations(attackPos, mech);
      Thread.CurrentThread.clearActor();
      return result;
    }
  }
}