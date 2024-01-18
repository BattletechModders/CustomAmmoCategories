using BattleTech;
using System;
using System.Collections.Generic;

namespace CustAmmoCategories {
  public static class UnitDefTypesAPI {
    public static List<Func<MechDef, bool>> isVehicleDelegateMechDef = new List<Func<MechDef, bool>>();
    public static List<Func<MechDef, bool>> isQuadDelegateMechDef = new List<Func<MechDef, bool>>();
    public static List<Func<MechDef, bool>> isSquadDelegateMechDef = new List<Func<MechDef, bool>>();

    public static List<Func<ChassisDef, bool>> isVehicleDelegateChassisDef = new List<Func<ChassisDef, bool>>();
    public static List<Func<ChassisDef, bool>> isQuadDelegateChassisDef = new List<Func<ChassisDef, bool>>();
    public static List<Func<ChassisDef, bool>> isSquadDelegateChassisDef = new List<Func<ChassisDef, bool>>();

    public static List<Func<MechDef, bool>> isDestroyedDelegate = new List<Func<MechDef, bool>>();

    public static Func<ChassisDef, ChassisLocations, string> GetAbbreviatedChassisLocationDelegate = null;
    public static void Register_isVehcileDelegateMechDef(Func<MechDef, bool> Delegate) {
      isVehicleDelegateMechDef.Add(Delegate);
    }
    public static void Register_isQuadDelegateMechDef(Func<MechDef, bool> Delegate) {
      isQuadDelegateMechDef.Add(Delegate);
    }
    public static void Register_isSquadDelegateMechDef(Func<MechDef, bool> Delegate) {
      isSquadDelegateMechDef.Add(Delegate);
    }
    public static void Register_isVehcileDelegateChassisDef(Func<ChassisDef, bool> Delegate) {
      isVehicleDelegateChassisDef.Add(Delegate);
    }
    public static void Register_isQuadDelegateChassisDef(Func<ChassisDef, bool> Delegate) {
      isQuadDelegateChassisDef.Add(Delegate);
    }
    public static void Register_isSquadDelegateChassisDef(Func<ChassisDef, bool> Delegate) {
      isSquadDelegateChassisDef.Add(Delegate);
    }
    public static void Register_isDestroyedDelegate(Func<MechDef, bool> Delegate) {
      isDestroyedDelegate.Add(Delegate);
    }
    public static void Register_GetAbbreviatedChassisLocationDelegate(Func<ChassisDef, ChassisLocations, string> Delegate) {
      GetAbbreviatedChassisLocationDelegate = Delegate;
    }
    public static bool IsVehicle(this MechDef def) {
      foreach(var d in isVehicleDelegateMechDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsQuad(this MechDef def) {
      foreach(var d in isQuadDelegateMechDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsSquad(this MechDef def) {
      foreach(var d in isSquadDelegateMechDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsVehicle(this ChassisDef def) {
      foreach(var d in isVehicleDelegateChassisDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsQuad(this ChassisDef def) {
      foreach(var d in isQuadDelegateChassisDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsSquad(this ChassisDef def) {
      foreach(var d in isSquadDelegateChassisDef) { if(d.Invoke(def)) { return true; } }
      return false;
    }
    public static bool IsDestroyed(this MechDef def) {
      foreach(var d in isDestroyedDelegate) { if(d.Invoke(def)) { return true; } }
      return def.IsDestroyed;
    }
    public static string GetAbbreviatedChassisLocation(this ChassisDef def, ChassisLocations location) {
      if(GetAbbreviatedChassisLocationDelegate != null) { return GetAbbreviatedChassisLocationDelegate(def, location); }
      return Mech.GetAbbreviatedChassisLocation(location).ToString();
    }
  }
}