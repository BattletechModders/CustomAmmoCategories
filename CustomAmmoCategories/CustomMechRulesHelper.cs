using BattleTech;
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class HitTableHelper {
    private static Dictionary<Type, Func<Mech,bool, ArmorLocation, AttackDirection, Dictionary<ArmorLocation, int>>> GetHitTablesFunctions = new Dictionary<Type, Func<Mech,bool, ArmorLocation, AttackDirection, Dictionary<ArmorLocation, int>>>();
    public static void RegisterGetHitTableFunction(Type t, Func<Mech, bool, ArmorLocation, AttackDirection, Dictionary<ArmorLocation, int>> func) {
      GetHitTablesFunctions.Add(t, func);
    }
    public static Dictionary<ArmorLocation, int> GetHitTable(Mech mech, bool isCluster, ArmorLocation aLoc, AttackDirection from) {
      if(GetHitTablesFunctions.TryGetValue(mech.GetType(), out Func<Mech,bool, ArmorLocation, AttackDirection, Dictionary<ArmorLocation, int>> func)) {
        Dictionary<ArmorLocation, int> result = func(mech, isCluster, aLoc, from);
        if (result != null) { return result; }
      }
      return (aLoc == ArmorLocation.None || !isCluster) ? mech.Combat.HitLocation.GetMechHitTable(from) : mech.Combat.Constants.GetMechClusterTable(aLoc, from);
    }
  }
  public static class GetLongArmorLocationHelper {
    private static Dictionary<Type, Func<Mech, ArmorLocation, Text>> GetLongArmorLocationFunctions = new Dictionary<Type, Func<Mech, ArmorLocation, Text>>();
    public static void RegisterGetLongArmorLocationFunction(Type t, Func<Mech, ArmorLocation, Text> func) {
      GetLongArmorLocationFunctions.Add(t, func);
    }
    public static Text GetLongArmorLocation(this Mech mech, ArmorLocation location) {
      if (GetLongArmorLocationFunctions.TryGetValue(mech.GetType(), out Func<Mech, ArmorLocation, Text> func)) {
        Text result = func(mech, location);
        if (result != null) { return result; }
      }
      return Mech.GetLongArmorLocation(location);
    }
  }
  public static class GetDFASelfDamageLocationsHelper {
    private static Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>> GetDFASelfDamageLocationsFunctions = new Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>>();
    public static void RegisterDFASelfDamageLocationsFunction(Type t, Func<Mech, HashSet<ArmorLocation>> func) {
      GetDFASelfDamageLocationsFunctions.Add(t, func);
    }
    public static HashSet<ArmorLocation> GetDFASelfDamageLocations(this Mech mech) {
      if (GetDFASelfDamageLocationsFunctions.TryGetValue(mech.GetType(), out Func < Mech, HashSet < ArmorLocation >> func)) {
        HashSet<ArmorLocation> result = func(mech);
        if (result != null) { return result; }
      }
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }
  }
  public static class GetLandmineDamageLocationsHelper {
    private static Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>> GetLandmineDamageLocationsFunctions = new Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>>();
    public static void RegisterLandmineDamageLocationsFunction(Type t, Func<Mech, HashSet<ArmorLocation>> func) {
      GetLandmineDamageLocationsFunctions.Add(t, func);
    }
    public static HashSet<ArmorLocation> GetLandmineDamageArmorLocations(this Mech mech) {
      if (GetLandmineDamageLocationsFunctions.TryGetValue(mech.GetType(), out Func<Mech, HashSet<ArmorLocation>> func)) {
        HashSet<ArmorLocation> result = func(mech);
        if (result != null) { return result; }
      }
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }
  }
  public static class GetBurnDamageLocationsHelper {
    private static Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>> GetBurnDamageLocationsFunctions = new Dictionary<Type, Func<Mech, HashSet<ArmorLocation>>>();
    public static void RegisterBurnDamageLocationsFunction(Type t, Func<Mech, HashSet<ArmorLocation>> func) {
      GetBurnDamageLocationsFunctions.Add(t, func);
    }
    public static HashSet<ArmorLocation> GetBurnDamageArmorLocations(this Mech mech) {
      if (GetBurnDamageLocationsFunctions.TryGetValue(mech.GetType(), out Func<Mech, HashSet<ArmorLocation>> func)) {
        HashSet<ArmorLocation> result = func(mech);
        if (result != null) { return result; }
      }
      return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear,
        ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear, ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear,
        ArmorLocation.RightArm, ArmorLocation.LeftArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg
      };
    }
  }
  public static class GetAOESpreadLocationsHelper {
    private static Dictionary<Type, Func<Mech, Dictionary<int, float>>> GetAOELocationsFunctions = new Dictionary<Type, Func<Mech, Dictionary<int, float>>>();
    public static void RegisterAOELocationsFunction(Type t, Func<Mech, Dictionary<int, float>> func) {
      GetAOELocationsFunctions.Add(t, func);
    }
    public static Dictionary<int, float> GetAOESpreadArmorLocations(this Mech mech) {
      if (GetAOELocationsFunctions.TryGetValue(mech.GetType(), out Func<Mech, Dictionary<int, float>> func)) {
        Dictionary<int, float> result = func(mech);
        if (result != null) { return result; }
      }
      if (CustomAmmoCategories.NormMechHitLocations == null) { CustomAmmoCategories.InitHitLocationsAOE(); }
      return CustomAmmoCategories.NormMechHitLocations;
    }
  }
  public static class GetAOEPossibleHitLocationsHelper {
    private static Dictionary<Type, Func<Mech,Vector3, List<int>>> GetAOELocationsFunctions = new Dictionary<Type, Func<Mech, Vector3, List<int>>>();
    public static void RegisterAOELocationsFunction(Type t, Func<Mech, Vector3, List<int>> func) {
      GetAOELocationsFunctions.Add(t, func);
    }
    public static List<int> GetAOEPossibleHitLocations(this Mech mech, Vector3 attackPos) {
      if (GetAOELocationsFunctions.TryGetValue(mech.GetType(), out Func<Mech, Vector3, List<int>> func)) {
        List<int> result = func(mech, attackPos);
        if (result != null) { return result; }
      }
      return mech.Combat.HitLocation.GetPossibleHitLocations(attackPos, mech);
    }
  }
}