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
using BattleTech.UI;
using CACMain;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Harmony;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustAmmoCategories {
  public class ToHitModifier {
    public string id { get; set; }
    public string name { get; set; }
    public bool ranged { get; set; }
    public bool melee { get; set; }
    public Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier { get; set; }
    public Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname { get; set; }
    public Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, int, string> dname2 { get; set; }
    public ToHitModifier(string id,string name, bool ranged, bool melee, 
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, int, string> dname2
      ) {
      this.id = id;
      this.name = name;
      this.modifier = modifier;
      this.dname = dname;
      this.dname2 = dname2;
      this.ranged = ranged;
      this.melee = melee;
    }
  }
  public class ToHitNodeModifier {
    public string id { get; set; }
    public string name { get; set; }
    public bool ranged { get; set; }
    public bool melee { get; set; }
    public Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier { get; set; }
    public Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname { get; set; }
    public ToHitNodeModifier(string id, string name, bool ranged, bool melee,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      this.id = id;
      this.name = name;
      this.modifier = modifier;
      this.dname = dname;
      this.ranged = ranged;
      this.melee = melee;
    }
  }
  public class ToHitModifierNode {
    public string id { get; set; }
    public Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, object> prepare { get; set; }
    public Dictionary<string, ToHitNodeModifier> modifiers = null;
    public ToHitModifierNode(string id,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, object> prepare
    ) {
      this.id = id;
      this.prepare = prepare;
      this.modifiers = new Dictionary<string, ToHitNodeModifier>();
    }
    public void registerModifier(string id, string name, bool ranged, bool melee,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      Log.M.TWL(0, "registerModifier:" + id + "/" + name + " r:" + ranged + " m:" + melee, true);
      if (modifiers.TryGetValue(id, out ToHitNodeModifier mod)) {
        mod.name = name;
        mod.ranged = ranged;
        mod.melee = melee;
        mod.modifier = modifier;
        mod.dname = dname;
      } else {
        modifiers.Add(id, new ToHitNodeModifier(id, name, ranged, melee, modifier, dname));
      }
    }
  }
  public static class ToHitModifiersHelper {
    public static Dictionary<string, ToHitModifier> modifiers = new Dictionary<string, ToHitModifier>();
    public static Dictionary<string, ToHitModifierNode> mod_nodes = new Dictionary<string, ToHitModifierNode>();
    public static CombatHUD HUD = null;
    public static void InitHUD(CombatHUD HUD) {
      ToHitModifiersHelper.HUD = HUD;
    }
    public static void Clear() {
      HUD = null;
    }
    public static void registerNode(string id, Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, object> prepare) {
      Log.M.TWL(0, "registerNode:" + id, true);
      if(mod_nodes.TryGetValue(id, out ToHitModifierNode node)) {
        node.prepare = prepare;
      } else {
        mod_nodes.Add(id, new ToHitModifierNode(id, prepare));
      }
    }
    public static void registerNodeModifier(string nodeId, string id, string name, bool ranged, bool melee,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<object, ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      Log.M.TWL(0, "registerNodeModifier: node:" + nodeId + " id:"  + id + "/" + name + " r:" + ranged + " m:" + melee, true);
      if (mod_nodes.TryGetValue(nodeId, out ToHitModifierNode node)) {
        node.registerModifier(id,name,ranged,melee, modifier, dname);
      } else {
        Log.M.WL(1,"!Can't find node id:"+nodeId, true);
      }
    }
    public static void registerModifier(string id, string name, bool ranged, bool melee,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      Log.M.TWL(0, "registerModifier:" + id + "/" + name + " r:" + ranged + " m:" + melee, true);
      if (modifiers.TryGetValue(id, out ToHitModifier mod)) {
        mod.name = name;
        mod.ranged = ranged;
        mod.melee = melee;
        mod.modifier = modifier;
        mod.dname = dname;
        mod.dname2 = null;
      } else {
        modifiers.Add(id, new ToHitModifier(id, name, ranged, melee, modifier, dname, null));
      }
    }
    public static void registerModifier2(string id, string name, bool ranged, bool melee,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, int, string> dname2
      ) {
      Log.M.TWL(0, "registerModifier:" + id + "/" + name + " r:" + ranged + " m:" + melee, true);
      if (modifiers.TryGetValue(id, out ToHitModifier mod)) {
        mod.name = name;
        mod.ranged = ranged;
        mod.melee = melee;
        mod.modifier = modifier;
        mod.dname = null;
        mod.dname2 = dname2;
      } else {
        modifiers.Add(id, new ToHitModifier(id, name, ranged, melee, modifier, null, dname2));
      }
    }
    public static float GetRangeModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      float distance = Vector3.Distance(attackPosition, targetPosition);
      float distMod = 0f;
      float minRange = weapon.MinRange;
      float shortRange = weapon.ShortRange;
      float medRange = weapon.MediumRange;
      float longRange = weapon.LongRange;
      float maxRange = weapon.MaxRange;
      if (distance < minRange) {
        distMod = weapon.parent.MinRangeAccMod();
        //Log.LogWrite(" minRange "); 
      } else
      if (distance < shortRange) {
        distMod = weapon.parent.ShortRangeAccMod();
        //Log.LogWrite(" shortRange ");
      } else
      if (distance < medRange) {
        distMod = weapon.parent.MediumRangeAccMod();
        //Log.LogWrite(" medRange ");
      } else
      if (distance < longRange) {
        distMod = weapon.parent.LongRangeRangeAccMod();
        //Log.LogWrite(" longRange ");
      } else
      if (distance < maxRange) {
        distMod = weapon.parent.ExtraLongRangeAccMod();
        //Log.LogWrite(" extraRange ");
      };
      //return distMod;
      return distMod + instance.GetRangeModifier(weapon, attackPosition, targetPosition);
    }
    private static string SmartRange(float min, float range, float max) {
      if (min <= 0 || range - min > max - range)
        return " (<" + (int)max + "m)"; // Show next range boundery when no lower boundary or target is closer to next than lower.
      return " (>" + (int)min + "m)";
    }
    public static string GetRangeModifierName(ToHit instance, AbstractActor attacker, Weapon w, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      float range = Vector3.Distance(attackPosition, targetPosition);
      if (range < w.MinRange) return ("__/AIM.MIN_RANGE/__ (<" + w.MinRange + "m)");
      if (range < w.ShortRange) return ("__/AIM.SHORT_RANGE/__" + SmartRange(w.MinRange, range, w.ShortRange));
      if (range < w.MediumRange) return ("__/AIM.MED_RANGE/__" + SmartRange(w.ShortRange, range, w.MediumRange));
      if (range < w.LongRange) return ("__/AIM.LONG_RANGE/__" + SmartRange(w.MediumRange, range, w.LongRange));
      if (range < w.MaxRange) return ("__/AIM.MAX_RANGE/__" + SmartRange(w.LongRange, range, w.MaxRange));
      return "__/AIM.OUT_OF_RANGE/__ (>" + w.MaxRange + "m)";
    }
    public static float GetCoverModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetCoverModifier(attacker, target, lofLevel);
    }
    public static float GetSelfSpeedModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetSelfSpeedModifier(attacker);
    }
    public static float GetSelfSprintedModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetSelfSprintedModifier(attacker);
    }
    public static float GetSelfArmMountedModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetSelfArmMountedModifier(weapon);
    }
    public static float GetStoodUpModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetStoodUpModifier(attacker);
    }
    public static float GetHeightModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetHeightModifier(attackPosition.y, targetPosition.y);
    }
    public static float GetHeatModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetHeatModifier(attacker);
    }
    public static float GetTargetTerrainModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (target.UnaffectedDesignMasks()) { return 0f; }
      return instance.GetTargetTerrainModifier(target, targetPosition, false);
    }
    public static float GetSelfTerrainModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (attacker.UnaffectedDesignMasks()) { return 0f; };
      return instance.GetSelfTerrainModifier(attackPosition, false);
    }
    public static string GetTargetTerrainModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      AbstractActor unit = target as AbstractActor;
      if (unit != null) {
        if (unit.occupiedDesignMask != null) {
          return Strings.T("INTO {0}", (object)unit.occupiedDesignMask.Description.Name);
        }
      }
      return string.Empty;
    }
    public static string GetSelfTerrainModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (attacker != null) {
        DesignMaskDef dmask = attacker.Combat.MapMetaData.GetPriorityDesignMaskAtPos(attackPosition);
        if (dmask != null) {
          return Strings.T("FROM {0}", (object)dmask.Description.Name);
        }
      }
      return string.Empty;
    }
    public static float GetTargetSpeedModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetTargetSpeedModifier(target, weapon);
    }
    public static float GetSelfDamageModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      int location = weapon.Location;
      if ((meleeAttackType != MeleeAttackType.NotSet)&&(weapon.Type == WeaponType.Melee)) {
        location = GetMeleeWeaponLocation(attacker, meleeAttackType);
      } else {
        switch (attacker.UnitType) {
          case UnitType.Mech: location = weapon.mechComponentRef != null ? (int)weapon.mechComponentRef.MountedLocation : weapon.Location;
          break;
          case UnitType.Vehicle: location = weapon.vehicleComponentRef!=null?(int)weapon.vehicleComponentRef.MountedLocation:(int)VehicleChassisLocations.Front; break;
          case UnitType.Turret: location = (int)BuildingLocation.Structure; break;
        }
      }
      return location == 0 ? GetToHitModifier(attacker, weapon.Location): GetToHitModifier(attacker, location);
    }
    public static int GetMeleeWeaponLocation(AbstractActor unit, MeleeAttackType meleeAttackType) {
      switch (unit.UnitType) {
        case UnitType.Mech: return GetMeleeWeaponLocation(unit as Mech, meleeAttackType);
        case UnitType.Vehicle: return GetMeleeWeaponLocation(unit as Vehicle, meleeAttackType);
        case UnitType.Turret: return GetMeleeWeaponLocation(unit as Turret, meleeAttackType);
        default: return 0;
      }
    }
    public static int GetMeleeWeaponLocation(Mech mech, MeleeAttackType meleeAttackType) {
      if (mech.FakeVehicle()) { return (int)ChassisLocations.LeftArm; }
      if (mech == null) { return (int)ChassisLocations.CenterTorso; }
      switch (meleeAttackType) {
        case MeleeAttackType.Punch: return mech.MechDef.Chassis.PunchesWithLeftArm?(int)ChassisLocations.LeftArm: (int)ChassisLocations.RightArm;
        case MeleeAttackType.Kick: return mech.MechDef.Chassis.PunchesWithLeftArm ? (int)ChassisLocations.LeftLeg : (int)ChassisLocations.RightLeg;
        case MeleeAttackType.Stomp: return mech.MechDef.Chassis.PunchesWithLeftArm ? (int)ChassisLocations.LeftLeg : (int)ChassisLocations.RightLeg;
        default: return (int)ChassisLocations.CenterTorso;
      }
    }
    public static int GetMeleeWeaponLocation(Vehicle vehicle, MeleeAttackType meleeAttackType) {
      return (int)VehicleChassisLocations.Front;
    }
    public static int GetMeleeWeaponLocation(Turret turret, MeleeAttackType meleeAttackType) {
      return (int)BuildingLocation.Structure;
    }
    public static float GetToHitModifier(Vehicle unit, int location) {
      if (unit == null) { return 0f; }
      float num = 0.0f;
      VehicleChassisLocations loc = (VehicleChassisLocations)location;
      try {
        string locName = unit.GetStringForStructureDamageLevel(loc);
        if (string.IsNullOrEmpty(locName)) { loc = VehicleChassisLocations.Front; }
        switch (unit.GetLocationDamageLevel(loc)) {
          case LocationDamageLevel.Penalized:
            num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMinor;
            break;
          case LocationDamageLevel.NonFunctional:
            num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMajor;
            break;
        }
      }catch(Exception e) {
        Log.M.TWL(0, "unit:" + unit.DisplayName + " location:" + loc.ToString()+":"+location);
        Log.M.TWL(0,e.ToString(),true);
        return 0f;
      }
      return num;
    }
    public static float GetToHitModifier(Mech unit, int location) {
      if (unit == null) { return 0f; }
      float num = 0.0f;
      ChassisLocations loc = (ChassisLocations)location;
      string locName = unit.GetStringForStructureDamageLevel(loc);
      if (string.IsNullOrEmpty(locName)) { loc = ChassisLocations.CenterTorso; }
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMinor;
          break;
        case LocationDamageLevel.NonFunctional:
          num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMajor;
          break;
      }
      return num;
    }
    public static float GetToHitModifier(Turret unit, int location) {
      if (unit == null) { return 0f; }
      float num = 0.0f;
      BuildingLocation loc = (BuildingLocation)location;
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMinor;
          break;
        case LocationDamageLevel.NonFunctional:
          num = unit.Combat.Constants.ToHit.ToHitSelfWeaponLocationDamagedMajor;
          break;
      }
      return num;
    }
    public static string GetToHitModifierName(Vehicle unit, int location) {
      if (unit == null) { return string.Empty; }
      string num = string.Empty;
      VehicleChassisLocations loc = (VehicleChassisLocations)location;
      string locName = unit.GetStringForStructureDamageLevel(loc);
      if (string.IsNullOrEmpty(locName)) { loc = VehicleChassisLocations.Front; }
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = new Text("{0} DAMAGED", (object)GetAbbreviatedChassisLocation(loc)).ToString();
          break;
        case LocationDamageLevel.NonFunctional:
          num = new Text("{0} DESTROYED", (object)GetAbbreviatedChassisLocation(loc)).ToString();
          break;
      }
      return num;
    }
    public static string GetToHitModifierName(Mech unit, int location) {
      if (unit == null) { return string.Empty; }
      string num = string.Empty;
      ChassisLocations loc = (ChassisLocations)location;
      string locName = unit.GetStringForStructureDamageLevel(loc);
      if (string.IsNullOrEmpty(locName)) { loc = ChassisLocations.CenterTorso; }
      Thread.CurrentThread.pushActor(unit);
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = new Text("{0} DAMAGED", (object)Mech.GetAbbreviatedChassisLocation(loc)).ToString();
          break;
        case LocationDamageLevel.NonFunctional:
          num = new Text("{0} DESTROYED", (object)Mech.GetAbbreviatedChassisLocation(loc)).ToString();
          break;
      }
      Thread.CurrentThread.clearActor();
      return num;
    }
    public static string GetAbbreviatedChassisLocation(this AbstractActor unit, int location) {
      Mech mech = unit as Mech;
      if(mech != null) {
        Thread.CurrentThread.pushActor(unit);
        string result = Mech.GetAbbreviatedChassisLocation((ChassisLocations)location).ToString();
        Thread.CurrentThread.clearActor();
        return result;
      }
      Vehicle vehicle = unit as Vehicle;
      if (vehicle != null) {
        return GetAbbreviatedChassisLocation((VehicleChassisLocations)location).ToString();
      }
      Turret turret = unit as Turret;
      if (turret != null) { return "S"; };
      return "-";
    }
    public static string GetToHitModifierName(Turret unit, int location) {
      if (unit == null) { return string.Empty; }
      string num = string.Empty;
      BuildingLocation loc = (BuildingLocation)location;
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = new Text("STRUCT DAMAGED").ToString();
          break;
        case LocationDamageLevel.NonFunctional:
          num = new Text("STRUCT DESTROYED").ToString();
          break;
      }
      return num;
    }
    public static float GetToHitModifier(AbstractActor unit, int location) {
      switch (unit.UnitType) {
        case UnitType.Mech: return GetToHitModifier(unit as Mech, location);
        case UnitType.Vehicle: return GetToHitModifier(unit as Vehicle, location);
        case UnitType.Turret: return GetToHitModifier(unit as Turret, location);
        default: return 0f;
      }
    }
    public static string GetToHitModifierName(AbstractActor unit, int location) {
      switch (unit.UnitType) {
        case UnitType.Mech: return GetToHitModifierName(unit as Mech, location);
        case UnitType.Vehicle: return GetToHitModifierName(unit as Vehicle, location);
        case UnitType.Turret: return GetToHitModifierName(unit as Turret, location);
        default: return string.Empty;
      }
    }
    public static string GetAbbreviatedChassisLocation(this VehicleChassisLocations location) {
      switch (location) {
        case VehicleChassisLocations.Turret:
          return new Text("T", (object[])Array.Empty<object>()).ToString();
        case VehicleChassisLocations.Front:
          return new Text("F", (object[])Array.Empty<object>()).ToString();
        case VehicleChassisLocations.Rear:
          return new Text("R", (object[])Array.Empty<object>()).ToString();
        case VehicleChassisLocations.Left:
          return new Text("L", (object[])Array.Empty<object>()).ToString();
        case VehicleChassisLocations.Right:
          return new Text("R", (object[])Array.Empty<object>()).ToString();
        default:
          return string.Empty;
      }
    }
    public static string GetSelfDamageModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      int location = weapon.Location;
      if ((meleeAttackType != MeleeAttackType.NotSet) && (weapon.Type == WeaponType.Melee)) {
        location = GetMeleeWeaponLocation(attacker, meleeAttackType);
      } else {
        switch (attacker.UnitType) {
          case UnitType.Mech: location = weapon.mechComponentRef != null?(int)weapon.mechComponentRef.MountedLocation:weapon.Location; break;
          case UnitType.Vehicle: location = weapon.vehicleComponentRef != null ? (int)weapon.vehicleComponentRef.MountedLocation : (int)VehicleChassisLocations.Front; break;
          case UnitType.Turret: location = (int)BuildingLocation.Structure; break;
        }
      }
      return location == 0 ? GetToHitModifierName(attacker, weapon.Location) : GetToHitModifierName(attacker, location);
    }
    public static float GetTargetSizeModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetTargetSizeModifier(target);
    }
    public static float GetTargetShutdownModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetTargetShutdownModifier(target, meleeAttackType != MeleeAttackType.NotSet);
    }
    public static float GetTargetProneModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetTargetProneModifier(target, meleeAttackType != MeleeAttackType.NotSet);
    }
    public static float GetWeaponAccuracyModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetWeaponAccuracyModifier(attacker, weapon);
    }
    public static float GetAttackerAccuracyModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetAttackerAccuracyModifier(attacker);
    }
    public static string GetAttackerAccuracyModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (instance.GetAttackerAccuracyModifier(attacker) > 0f) {
        return "SENSORS IMPAIRED";
      } else {
        return "INSPIRED";
      }
    }
    public static float GetEnemyEffectModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetEnemyEffectModifier(target, weapon);
    }
    public static float GetRefireModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetRefireModifier(weapon);
    }
    public static float GetTargetDirectFireModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetTargetDirectFireModifier(target, lofLevel < LineOfFireLevel.LOFObstructed && weapon.IndirectFireCapable());
    }
    public static float GetIndirectModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetIndirectModifier(attacker, lofLevel < LineOfFireLevel.LOFObstructed && weapon.IndirectFireCapable());
    }
    public static float GetMoraleAttackModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetMoraleAttackModifier(attacker, isCalledShot);
    }
    public static string GetMoraleAttackModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return attacker.Combat.Constants.CombatUIConstants.MoraleAttackDescription.Name;
    }
    public static float GetToHitModifierWeaponDamage(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (weapon.DamageLevel == ComponentDamageLevel.Misaligned || weapon.DamageLevel == ComponentDamageLevel.Penalized)
        return attacker.Combat.Constants.ToHit.ToHitSelfWeaponDamaged;
      return 0.0f;
    }
    public static float GetDirectAttackModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return weapon.GetDirectFireModifier(lofLevel);
    }
    public static float GetJumpedModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      float movement = 0f;
      if (attacker.HasMovedThisRound && attacker.JumpedLastRound) {
        movement = attacker.DistMovedThisRound;
      }
      if (HUD != null) {
        if ((HUD.SelectedActor == attacker) && (HUD.SelectionHandler.ActiveState is SelectionStateJump)) {
          movement = 1f;
        }
      }
      if (movement <= 0f) return 0f;
      if (Core.AIMModSettings != null) { return Core.AIMModSettings.ToHitSelfJumped; }
      return CustAmmoCategories.CustomAmmoCategories.Settings.ToHitSelfJumped;
    }
    public static float GetDirectionModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (attacker.GUID == target.GUID) { return 0f; }
      AttackDirection dir = attacker.Combat.HitLocation.GetAttackDirection(attackPosition, target);
      if (target is Mech mech) {
        if (mech.IsProne) return 0f;
        if (dir == AttackDirection.FromFront) return Core.AIMModSettings != null?Core.AIMModSettings.ToHitMechFromFront:CustomAmmoCategories.Settings.ToHitMechFromFront;
        if (dir == AttackDirection.FromLeft || dir == AttackDirection.FromRight) return Core.AIMModSettings != null ? Core.AIMModSettings.ToHitMechFromSide : CustomAmmoCategories.Settings.ToHitMechFromSide;
        if (dir == AttackDirection.FromBack) return Core.AIMModSettings != null ? Core.AIMModSettings.ToHitMechFromRear : CustomAmmoCategories.Settings.ToHitMechFromRear;
      } else if (target is Vehicle vehicle) {
        if (dir == AttackDirection.FromFront) return Core.AIMModSettings != null ? Core.AIMModSettings.ToHitVehicleFromFront : CustomAmmoCategories.Settings.ToHitVehicleFromFront;
        if (dir == AttackDirection.FromLeft || dir == AttackDirection.FromRight) return Core.AIMModSettings != null ? Core.AIMModSettings.ToHitVehicleFromSide : CustomAmmoCategories.Settings.ToHitVehicleFromSide;
        if (dir == AttackDirection.FromBack) return Core.AIMModSettings != null ? Core.AIMModSettings.ToHitVehicleFromRear : CustomAmmoCategories.Settings.ToHitVehicleFromRear;
      }
      return 0f;
    }
    public static string GetDirectionModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (attacker.GUID == target.GUID) { return string.Empty; }
      AttackDirection dir = attacker.Combat.HitLocation.GetAttackDirection(attackPosition, target);
      if (target is Mech mech) {
        if (mech.IsProne) return string.Empty;
        if (dir == AttackDirection.FromFront) return new Text("__/AIM.FRONT_ATTACK/__").ToString();
        if (dir == AttackDirection.FromLeft || dir == AttackDirection.FromRight) return new Text("__/AIM.SIDE_ATTACK/__").ToString();
        if (dir == AttackDirection.FromBack) return new Text("__/AIM.REAR_ATTACK/__").ToString();
      } else if (target is Vehicle vehicle) {
        if (dir == AttackDirection.FromFront) return new Text("__/AIM.FRONT_ATTACK/__").ToString();
        if (dir == AttackDirection.FromLeft || dir == AttackDirection.FromRight) return new Text("__/AIM.SIDE_ATTACK/__").ToString();
        if (dir == AttackDirection.FromBack) return new Text("__/AIM.REAR_ATTACK/__").ToString();
      }
      return string.Empty;
    }
    public static float GetDFAModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetDFAModifier(meleeAttackType);
    }
    public static float GetMeleeChassisToHitModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return instance.GetMeleeChassisToHitModifier(attacker,meleeAttackType);
    }
    public static string GetMeleeChassisToHitModifierName(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (instance.GetMeleeChassisToHitModifier(attacker, meleeAttackType) > 0) {
        return "CHASSIS BONUS";
      };
      return "CHASSIS PENALTY";
    }
    public static float GetMeleeArmMount(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (meleeAttackType == MeleeAttackType.DFA || target is Vehicle || target.IsProne || !(attacker is Mech mech)) return 0f;
      if (mech.MechDef.Chassis.PunchesWithLeftArm) {
        if (mech.IsLocationDestroyed(ChassisLocations.LeftArm)) return 0f;
      } else if (mech.IsLocationDestroyed(ChassisLocations.RightArm)) return 0f;
      return attacker.Combat.Constants.ToHit.ToHitSelfArmMountedWeapon;
    }
    private static float HalfMaxMeleeVerticalOffset = 4f;
    public static float GetMeleeHeightMod(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (meleeAttackType == MeleeAttackType.DFA) { return instance.GetHeightModifier(attacker.CurrentPosition.y, target.CurrentPosition.y); }
      if(attacker.UnaffectedPathing() == false) {
        if (target.UnaffectedPathing()) {
          return 100f;
        }
      } else {
        return 0f;
      }
      float diff = attackPosition.y - target.CurrentPosition.y;
      if (Math.Abs(diff) < HalfMaxMeleeVerticalOffset || (diff < 0 && !attacker.Combat.Constants.ToHit.ToHitElevationApplyPenalties)) return 0f;
      float mod = attacker.Combat.Constants.ToHit.ToHitElevationModifierPerLevel;
      return (diff <= 0 ? mod : -mod);
    }
    public static float GetMeleeEvesionMod(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (!(target is AbstractActor actor)) return 0f;
      return instance.GetEvasivePipsModifier(actor.EvasivePipsCurrent, weapon);
    }
    public static float GetChassisTagsModifyer(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      return weapon.GetChassisTagsModifyer(target);
    }
    public static void Init() {
      //build in modifiers
      HashSet<string> modsToRemove = new HashSet<string>();
      foreach(string modid in CustomAmmoCategories.Settings.RemoveToHitModifiers) { modsToRemove.Add(modid); }
      if(modsToRemove.Contains("RANGE") == false) registerModifier("RANGE", "RANGE", true, false, GetRangeModifier, GetRangeModifierName);
      if (modsToRemove.Contains("OBSTRUCTED") == false) registerModifier("OBSTRUCTED", "OBSTRUCTED", true, false, GetCoverModifier, null);
      if (modsToRemove.Contains("ARM MOUNTED") == false) registerModifier("ARM MOUNTED", "ARM MOUNTED", true, false, GetSelfArmMountedModifier, null);
      if (modsToRemove.Contains("HEIGHT DIFF") == false) registerModifier("HEIGHT DIFF", "HEIGHT DIFF", true, false, GetHeightModifier, null);
      if (modsToRemove.Contains("FROM") == false) registerModifier("FROM", "FROM", true, false, GetSelfTerrainModifier, GetSelfTerrainModifierName);
      if (modsToRemove.Contains("INTO") == false) registerModifier("INTO", "INTO", true, false, GetTargetTerrainModifier, GetTargetTerrainModifierName);
      if (modsToRemove.Contains("TARGET MOVED") == false) registerModifier("TARGET MOVED", "TARGET MOVED", true, false, GetTargetSpeedModifier, null);
      if (modsToRemove.Contains("ATTACKER ACCURACY") == false) registerModifier("ATTACKER ACCURACY", "ATTACKER ACCURACY", true, false, GetAttackerAccuracyModifier, GetAttackerAccuracyModifierName);
      if (modsToRemove.Contains("REFIRE") == false) registerModifier("REFIRE", "REFIRE", true, false, GetRefireModifier, null);
      if (modsToRemove.Contains("SENSOR LOCK") == false) registerModifier("SENSOR LOCK", "SENSOR LOCK", true, false, GetTargetDirectFireModifier, null);
      if (modsToRemove.Contains("MORALE") == false) registerModifier("MORALE", "MORALE", true, false, GetMoraleAttackModifier, GetMoraleAttackModifierName);
      if (modsToRemove.Contains("INDIRECT FIRE") == false) registerModifier("INDIRECT FIRE", "INDIRECT FIRE", true, false, GetIndirectModifier, null);
      if (modsToRemove.Contains("DIRECT") == false) registerModifier("DIRECT", "__/AIM.DIRECT/__", true, false, GetDirectAttackModifier, null);
      if (modsToRemove.Contains("TARGET TYPE") == false) registerModifier("TARGET TYPE", "__/AIM.TARGET_TYPE/__", true, false, GetChassisTagsModifyer, null);

      if (modsToRemove.Contains("JUMPED") == false) registerModifier("JUMPED", "__/AIM.JUMPED/__", false, false, GetJumpedModifier, null);
      if (modsToRemove.Contains("MOVED SELF") == false) registerModifier("MOVED SELF", "MOVED SELF", false, false, GetSelfSpeedModifier, null);
      if (modsToRemove.Contains("HEAT") == false) registerModifier("HEAT", "HEAT", false, false, GetHeatModifier, null);
      if (modsToRemove.Contains("STOOD UP") == false) registerModifier("STOOD UP", "STOOD UP", false, false, GetStoodUpModifier, null);
      if (modsToRemove.Contains("WEAPON ACCURACY") == false) registerModifier("WEAPON ACCURACY", "WEAPON ACCURACY", false, false, GetWeaponAccuracyModifier, null);
      if (modsToRemove.Contains("SPRINTED") == false) registerModifier("SPRINTED", "SPRINTED", false, false, GetSelfSprintedModifier, null);
      if (modsToRemove.Contains("WEAPON DAMAGED") == false) registerModifier("WEAPON DAMAGED", "WEAPON DAMAGED", false, false, GetToHitModifierWeaponDamage, null);
      if (modsToRemove.Contains("DAMAGED") == false) registerModifier("DAMAGED", "DAMAGED", false, false, GetSelfDamageModifier, GetSelfDamageModifierName);

      if (modsToRemove.Contains("DIRECTION") == false) registerModifier("DIRECTION", "DIRECTION", true, true, GetDirectionModifier, GetDirectionModifierName);
      if (modsToRemove.Contains("TARGET SIZE") == false) registerModifier("TARGET SIZE", "TARGET SIZE", true, true, GetTargetSizeModifier, null);
      if (modsToRemove.Contains("TARGET SHUTDOWN") == false) registerModifier("TARGET SHUTDOWN", "TARGET SHUTDOWN", true, true, GetTargetShutdownModifier, null);
      if (modsToRemove.Contains("TARGET PRONE") == false) registerModifier("TARGET PRONE", "TARGET PRONE", true, true, GetTargetProneModifier, null);
      if (modsToRemove.Contains("ENEMY EFFECTS") == false) registerModifier("ENEMY EFFECTS", "ENEMY EFFECTS", true, true, GetEnemyEffectModifier, null);

      if (modsToRemove.Contains("DEATH FROM ABOVE") == false) registerModifier("DEATH FROM ABOVE", "DEATH FROM ABOVE", false, true, GetDFAModifier, null);
      if (modsToRemove.Contains("CHASSIS BONUS") == false) registerModifier("CHASSIS BONUS", "CHASSIS BONUS", false, true, GetMeleeChassisToHitModifier, GetMeleeChassisToHitModifierName);
      if (modsToRemove.Contains("TERRAIN") == false) registerModifier("TERRAIN", "TERRAIN", false, true, GetTargetTerrainModifier, null);
      if (modsToRemove.Contains("MELEE ARM MOUNTED") == false) registerModifier("MELEE ARM MOUNTED", "__/AIM.PUNCHING_ARM/__", false, true, GetMeleeArmMount, null);
      if (modsToRemove.Contains("MELEE HEIGHT DIFF") == false) registerModifier("MELEE HEIGHT DIFF", "HEIGHT DIFF", false, true, GetMeleeHeightMod, null);
      if (modsToRemove.Contains("MELEE RECOIL") == false) registerModifier("MELEE RECOIL", "RE ATTACK", false, true, GetRefireModifier, null);
      if (modsToRemove.Contains("MELEE TARGET MOVED") == false) registerModifier("MELEE TARGET MOVED", "TARGET MOVED", false, true, GetMeleeEvesionMod, null);
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("UpdateTooltipStrings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_UpdateTooltipStrings {
    public delegate void d_AddToolTipDetail(CombatHUDWeaponSlot slot, string description, int modifier);
    private static d_AddToolTipDetail i_AddToolTipDetail = null;
    public static bool Prepare() {
      {
        MethodInfo method = typeof(CombatHUDWeaponSlot).GetMethod("AddToolTipDetail", BindingFlags.NonPublic | BindingFlags.Instance);
        var dm = new DynamicMethod("CACAddToolTipDetail", null, new Type[] { typeof(CombatHUDWeaponSlot), typeof(string), typeof(int) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AddToolTipDetail = (d_AddToolTipDetail)dm.CreateDelegate(typeof(d_AddToolTipDetail));
      }

      return true;
    }
    public static void UpdateToolTipsTarget(this CombatHUDWeaponSlot slot, ICombatant target) {
      slot.ToolTipHoverElement.BasicString = new Text("SHOT MODIFIER ", (object[])Array.Empty<object>());
      slot.ToolTipHoverElement.UseModifier = true;
      if (slot.DisplayedWeapon.Type == WeaponType.Melee) {
        slot.UpdateToolTipsMelee(target);
      } else if (slot.DisplayedWeapon.WillFireAtTargetFromPosition(target, Traverse.Create(slot).Field<CombatHUD>("HUD").Value.SelectionHandler.ActiveState.PreviewPos)) {
        slot.UpdateToolTipsFiring(target);
      } else {
        slot.UpdateToolTipsSelf();
      }
    }
    public static bool contemplatingDFA(this CombatHUDWeaponSlot slot,ICombatant target) {
      if (target == null)
        return false;
      SelectionState activeState = Traverse.Create(slot).Field<CombatHUD>("HUD").Value.SelectionHandler.ActiveState;
      return activeState != null && activeState.SelectionType == SelectionType.Jump && (activeState.PotentialMeleeTarget == target || activeState.TargetedCombatant == target);
    }
    public static bool contemplatingMelee(this CombatHUDWeaponSlot slot,ICombatant target) {
      if (target == null)
        return false;
      SelectionState activeState = Traverse.Create(slot).Field<CombatHUD>("HUD").Value.SelectionHandler.ActiveState;
      return activeState != null && activeState.SelectionType == SelectionType.Move && (activeState.PotentialMeleeTarget == target || activeState.TargetedCombatant == target);
    }
    public static void AddToolTipDetail(this CombatHUDWeaponSlot slot, string description, int modifier) {
      if (i_AddToolTipDetail != null) { i_AddToolTipDetail(slot, description, modifier); }
    }
    public static void UpdateToolTipsMelee(this CombatHUDWeaponSlot slot, ICombatant target) {
      //slot.ToolTipHoverElement.BasicString = new Text(slot.DisplayedWeapon.Name, (object[])Array.Empty<object>());
      CombatGameState Combat = Traverse.Create(slot).Field<CombatGameState>("Combat").Value;
      CombatHUD HUD = Traverse.Create(slot).Field<CombatHUD>("HUD").Value;
      MeleeAttackType meleeAttackType = slot.contemplatingDFA(target) ? MeleeAttackType.DFA : MeleeAttackType.Punch;
      bool calledShot = HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.FireMorale;
      int all_modifiers = 0;
      Core.AIMShowBaseMeleeChance(slot, target);
      foreach (var mod in ToHitModifiersHelper.modifiers) {
        if ((mod.Value.melee == false) && (mod.Value.ranged == true)) { continue; }
        int modifier = (int)mod.Value.modifier(Combat.ToHit,
          HUD.SelectedActor,slot.DisplayedWeapon,
          target, 
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition,LineOfFireLevel.LOFClear,meleeAttackType, calledShot);
        string name = mod.Value.name;
        if(mod.Value.dname != null) {
          name = mod.Value.dname(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        }else if(mod.Value.dname2 != null) {
          name = mod.Value.dname2(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot, modifier);
        }
        if (mod.Value.dname2 != null) {
          if (string.IsNullOrEmpty(name) == false) {
            slot.AddToolTipDetail(name, modifier);
          }
        } else
        if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
        }
        all_modifiers += modifier;
      }
      foreach(var node in ToHitModifiersHelper.mod_nodes) {
        object state = node.Value.prepare(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        foreach(var mod in node.Value.modifiers) {
          if ((mod.Value.melee == false) && (mod.Value.ranged == true)) { continue; }
          int modifier = (int)mod.Value.modifier(state,Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            target,
            HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          string name = mod.Value.name;
          if (mod.Value.dname != null) {
            name = mod.Value.dname(state,Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            target,
            HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          }
          if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
            slot.AddToolTipDetail(name, modifier);
          }
          all_modifiers += modifier;
        }
      }
      if ((all_modifiers < 0) && (Combat.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) { all_modifiers = 0; };
      slot.ToolTipHoverElement.BasicModifierInt = all_modifiers;
    }
    public static void UpdateToolTipsFiring(this CombatHUDWeaponSlot slot, ICombatant target) {
      CombatGameState Combat = Traverse.Create(slot).Field<CombatGameState>("Combat").Value;
      CombatHUD HUD = Traverse.Create(slot).Field<CombatHUD>("HUD").Value;
      MeleeAttackType meleeAttackType = MeleeAttackType.NotSet;
      LineOfFireLevel lofLevel = HUD.SelectionHandler.ActiveState.FiringPreview.GetPreviewInfo(target).LOFLevel;
      bool calledShot = HUD.SelectionHandler.ActiveState.SelectionType == SelectionType.FireMorale;
      int all_modifiers = 0;
      Core.AIMShowBaseHitChance(slot,target);
      Core.AIMShowNeutralRange(slot, target);
      //float baseChance = RollModifier.StepHitChance(Combat.ToHit.GetBaseToHitChance(mech)) * 100;
      //__instance.ToolTipHoverElement.BuffStrings.Add(new Text("{0} {1} = " + BaseChanceFormat, Translate(Pilot.PILOTSTAT_GUNNERY), mech.SkillGunnery, baseChance));
      foreach (var mod in ToHitModifiersHelper.modifiers) {
        if ((mod.Value.melee == true) && (mod.Value.ranged == false)) { continue; }
        int modifier = (int)mod.Value.modifier(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, lofLevel, meleeAttackType, calledShot);
        string name = mod.Value.name;
        if (mod.Value.dname != null) {
          name = mod.Value.dname(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, lofLevel, meleeAttackType, calledShot);
        } else if (mod.Value.dname2 != null) {
          name = mod.Value.dname2(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot, modifier);
        }
        if(mod.Value.dname2 != null) {
          //Log.M?.TWL(0, "UpdateToolTipsFiring "+ slot.DisplayedWeapon.defId+" name:"+name+" modifier:"+modifier);
          if(string.IsNullOrEmpty(name) == false) {
            slot.AddToolTipDetail(name, modifier);
          }
        }else
        if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
        }
        all_modifiers += modifier;
      }
      foreach (var node in ToHitModifiersHelper.mod_nodes) {
        object state = node.Value.prepare(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        foreach (var mod in node.Value.modifiers) {
          if ((mod.Value.melee == true) && (mod.Value.ranged == false)) { continue; }
          int modifier = (int)mod.Value.modifier(state, Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            target,
            HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          string name = mod.Value.name;
          if (mod.Value.dname != null) {
            name = mod.Value.dname(state, Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            target,
            HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          }
          if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
            slot.AddToolTipDetail(name, modifier);
          }
          all_modifiers += modifier;
        }
      }
      if ((all_modifiers < 0) && (Combat.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) { all_modifiers = 0; };
      slot.ToolTipHoverElement.BasicModifierInt = all_modifiers;
    }
    public static void UpdateToolTipsSelf(this CombatHUDWeaponSlot slot) {
      slot.ToolTipHoverElement.BasicString = new Text(slot.DisplayedWeapon.Name, (object[])Array.Empty<object>());
      CombatGameState Combat = Traverse.Create(slot).Field<CombatGameState>("Combat").Value;
      CombatHUD HUD = Traverse.Create(slot).Field<CombatHUD>("HUD").Value;
      MeleeAttackType meleeAttackType = MeleeAttackType.NotSet;
      bool calledShot = false;
      foreach (var mod in ToHitModifiersHelper.modifiers) {
        if ((mod.Value.ranged != false) || (mod.Value.melee != false)) { continue; }
        if (slot.DisplayedWeapon == null) { continue; }
        if (HUD.SelectedActor == null) { continue; }
        int modifier = (int)mod.Value.modifier(Combat.ToHit,
          slot.DisplayedWeapon.parent, slot.DisplayedWeapon,
          HUD.SelectedTarget,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        string name = mod.Value.name;
        if (mod.Value.dname != null) {
          name = mod.Value.dname(Combat.ToHit,
          slot.DisplayedWeapon.parent, slot.DisplayedWeapon,
          HUD.SelectedTarget,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        } else if (mod.Value.dname2 != null) {
          name = mod.Value.dname2(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          HUD.SelectedTarget,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot, modifier);
        }
        if (mod.Value.dname2 != null) {
          //Log.M?.TWL(0, "UpdateToolTipsSelf " + slot.DisplayedWeapon.defId + " name:" + name + " modifier:" + modifier);
          if (string.IsNullOrEmpty(name) == false) {
            slot.AddToolTipDetail(name, modifier);
          }
        } else
        if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
        }
      }
      foreach (var node in ToHitModifiersHelper.mod_nodes) {
        if (slot.DisplayedWeapon == null) { continue; }
        if (HUD.SelectedActor == null) { continue; }
        object state = node.Value.prepare(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          HUD.SelectedTarget,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        foreach (var mod in node.Value.modifiers) {
          if ((mod.Value.ranged != false) || (mod.Value.melee != false)) { continue; }
          int modifier = (int)mod.Value.modifier(state, Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            HUD.SelectedTarget,
            HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          string name = mod.Value.name;
          if (mod.Value.dname != null) {
            name = mod.Value.dname(state, Combat.ToHit,
            HUD.SelectedActor, slot.DisplayedWeapon,
            HUD.SelectedTarget,
            HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
          }
          if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
            slot.AddToolTipDetail(name, modifier);
          }
        }
      }
      slot.ToolTipHoverElement.UseModifier = false;
    }
    public static bool Prefix(CombatHUDWeaponSlot __instance, ICombatant target) {
      __instance.ToolTipHoverElement.OnPointerExit((PointerEventData)null);
      __instance.ToolTipHoverElement.BuffStrings.Clear();
      __instance.ToolTipHoverElement.DebuffStrings.Clear();
      if (target != null) {
        __instance.UpdateToolTipsTarget(target);
      } else {
        __instance.UpdateToolTipsSelf();
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetAllModifiers")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
  public static class ToHit_GetAllModifiers {
    public static bool Prefix() {
      return false;
    }
    public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot, ref float __result) {
      __result = 0f;
      foreach(var mod in ToHitModifiersHelper.modifiers) {
        if ((mod.Value.ranged == false)&&(mod.Value.melee == true)) { continue; }
        __result += mod.Value.modifier(__instance,attacker,weapon,target,attackPosition,targetPosition,lofLevel,MeleeAttackType.NotSet,isCalledShot);
      }
      foreach (var node in ToHitModifiersHelper.mod_nodes) {
        object state = node.Value.prepare(__instance, attacker, weapon, target, attackPosition, targetPosition, lofLevel, MeleeAttackType.NotSet, isCalledShot);
        foreach (var mod in node.Value.modifiers) {
          if ((mod.Value.ranged == false) && (mod.Value.melee == true)) { continue; }
          __result += (int)mod.Value.modifier(state, __instance, attacker, weapon, target, attackPosition, targetPosition, lofLevel, MeleeAttackType.NotSet, isCalledShot);
        }
      }
      if ((__result < 0f) && (Traverse.Create(__instance).Field<CombatGameState>("combat").Value.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) {
        __result = 0f;
      }
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetAllMeleeModifiers")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(ICombatant), typeof(Vector3), typeof(MeleeAttackType) })]
  public static class ToHit_GetAllMeleeModifiers {
    public static bool Prefix() {
      return false;
    }
    public static void Postfix(ToHit __instance, Mech attacker, ICombatant target, Vector3 targetPosition, MeleeAttackType meleeAttackType, ref float __result) {
      __result = 0f;
      foreach (var mod in ToHitModifiersHelper.modifiers) {
        if ((mod.Value.ranged == true) && (mod.Value.melee == false)) { continue; }
        __result += mod.Value.modifier(__instance, attacker, attacker.MeleeWeapon, target, targetPosition, targetPosition, LineOfFireLevel.LOFClear, meleeAttackType, false);
      }
      foreach (var node in ToHitModifiersHelper.mod_nodes) {
        object state = node.Value.prepare(__instance, attacker, attacker.MeleeWeapon, target, targetPosition, targetPosition, LineOfFireLevel.LOFClear, MeleeAttackType.NotSet, false);
        foreach (var mod in node.Value.modifiers) {
          if ((mod.Value.ranged == true) && (mod.Value.melee == false)) { continue; }
          __result += (int)mod.Value.modifier(state, __instance, attacker, attacker.MeleeWeapon, target, targetPosition, targetPosition, LineOfFireLevel.LOFClear, MeleeAttackType.NotSet, false);
        }
      }
      if ((__result < 0f) && (Traverse.Create(__instance).Field<CombatGameState>("combat").Value.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) {
        __result = 0f;
      }
    }
  }
}