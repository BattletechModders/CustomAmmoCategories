using BattleTech;
using BattleTech.UI;
using CACMain;
using CustomAmmoCategoriesPatches;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
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
    public ToHitModifier(string id,string name, bool ranged, bool melee, 
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      this.id = id;
      this.name = name;
      this.modifier = modifier;
      this.dname = dname;
      this.ranged = ranged;
      this.melee = melee;
    }
  }
  public static class ToHitModifiersHelper {
    public static Dictionary<string, ToHitModifier> modifiers = new Dictionary<string, ToHitModifier>();
    public static CombatHUD HUD = null;
    public static void InitHUD(CombatHUD HUD) {
      ToHitModifiersHelper.HUD = HUD;
    }
    public static void Clear() {
      HUD = null;
    }
    public static void registerModifier(string id, string name, bool ranged, bool melee,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, float> modifier,
      Func<ToHit, AbstractActor, Weapon, ICombatant, Vector3, Vector3, LineOfFireLevel, MeleeAttackType, bool, string> dname
      ) {
      if (modifiers.TryGetValue(id, out ToHitModifier mod)) {
        mod.name = name;
        mod.ranged = ranged;
        mod.melee = melee;
        mod.modifier = modifier;
        mod.dname = dname;
      } else {
        modifiers.Add(id, new ToHitModifier(id, name, ranged, melee, modifier, dname));
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
      return instance.GetTargetTerrainModifier(target, targetPosition, false);
    }
    public static float GetSelfTerrainModifier(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
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
      };
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
    public static float GetToHitModifier(Mech unit, int location) {
      if (unit == null) { return 0f; }
      float num = 0.0f;
      ChassisLocations loc = (ChassisLocations)location;
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
      switch (unit.GetLocationDamageLevel(loc)) {
        case LocationDamageLevel.Penalized:
          num = new Text("{0} DAMAGED", (object)Mech.GetAbbreviatedChassisLocation(loc)).ToString();
          break;
        case LocationDamageLevel.NonFunctional:
          num = new Text("{0} DESTROYED", (object)Mech.GetAbbreviatedChassisLocation(loc)).ToString();
          break;
      }
      return num;
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
      };
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
        if (dir == AttackDirection.FromLeft || dir == AttackDirection.FromRight) return new Text("__ /AIM.SIDE_ATTACK/__").ToString();
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
      if (meleeAttackType == MeleeAttackType.DFA)
        return instance.GetHeightModifier(attacker.CurrentPosition.y, target.CurrentPosition.y);
      float diff = attackPosition.y - target.CurrentPosition.y;
      if (Math.Abs(diff) < HalfMaxMeleeVerticalOffset || (diff < 0 && !attacker.Combat.Constants.ToHit.ToHitElevationApplyPenalties)) return 0f;
      float mod = attacker.Combat.Constants.ToHit.ToHitElevationModifierPerLevel;
      return (diff <= 0 ? mod : -mod);
    }
    public static float GetMeleeEvesionMod(ToHit instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, MeleeAttackType meleeAttackType, bool isCalledShot) {
      if (!(target is AbstractActor actor)) return 0f;
      return instance.GetEvasivePipsModifier(actor.EvasivePipsCurrent, weapon);
    }
    public static void Init() {
      //build in modifiers
      registerModifier("RANGE", "RANGE", true, false, GetRangeModifier, GetRangeModifierName);
      registerModifier("OBSTRUCTED", "OBSTRUCTED", true, false, GetCoverModifier, null);
      registerModifier("ARM MOUNTED", "ARM MOUNTED", true, false, GetSelfArmMountedModifier, null);
      registerModifier("HEIGHT DIFF", "HEIGHT DIFF", true, false, GetHeightModifier, null);
      registerModifier("FROM", "FROM", true, false, GetSelfTerrainModifier, GetSelfTerrainModifierName);
      registerModifier("INTO", "INTO", true, false, GetTargetTerrainModifier, GetTargetTerrainModifierName);
      registerModifier("TARGET MOVED", "TARGET MOVED", true, false, GetTargetSpeedModifier, null);
      registerModifier("TARGET SIZE", "TARGET SIZE", true, false, GetTargetSizeModifier, null);
      registerModifier("ATTACKER ACCURACY", "ATTACKER ACCURACY", true, false, GetAttackerAccuracyModifier, GetAttackerAccuracyModifierName);
      registerModifier("REFIRE", "REFIRE", true, false, GetRefireModifier, null);
      registerModifier("SENSOR LOCK", "SENSOR LOCK", true, false, GetTargetDirectFireModifier, null);
      registerModifier("INDIRECT FIRE", "INDIRECT FIRE", true, false, GetIndirectModifier, null);
      registerModifier("MORALE", "MORALE", true, false, GetMoraleAttackModifier, GetMoraleAttackModifierName);
      registerModifier("INDIRECT FIRE", "INDIRECT FIRE", true, false, GetIndirectModifier, null);
      registerModifier("DIRECT", "__/AIM.DIRECT/__", true, false, GetDirectAttackModifier, null);

      registerModifier("JUMPED", "__/AIM.JUMPED/__", false, false, GetJumpedModifier, null);
      registerModifier("MOVED SELF", "MOVED SELF", false, false, GetSelfSpeedModifier, null);
      registerModifier("HEAT", "HEAT", false, false, GetHeatModifier, null);
      registerModifier("STOOD UP", "STOOD UP", false, false, GetStoodUpModifier, null);
      registerModifier("WEAPON ACCURACY", "WEAPON ACCURACY", false, false, GetWeaponAccuracyModifier, null);
      registerModifier("SPRINTED", "SPRINTED", false, false, GetSelfSprintedModifier, null);
      registerModifier("WEAPON DAMAGED", "WEAPON DAMAGED", false, false, GetToHitModifierWeaponDamage, null);
      registerModifier("DAMAGED", "DAMAGED", false, false, GetSelfDamageModifier, GetSelfDamageModifierName);

      registerModifier("DIRECTION", "DIRECTION", true, true, GetDirectionModifier, GetDirectionModifierName);
      registerModifier("TARGET SIZE", "TARGET SIZE", true, true, GetTargetSizeModifier, null);
      registerModifier("TARGET SHUTDOWN", "TARGET SHUTDOWN", true, true, GetTargetShutdownModifier, null);
      registerModifier("TARGET PRONE", "TARGET PRONE", true, true, GetTargetProneModifier, null);
      registerModifier("ENEMY EFFECTS", "ENEMY EFFECTS", true, true, GetEnemyEffectModifier, null);

      registerModifier("DEATH FROM ABOVE", "DEATH FROM ABOVE", false, true, GetDFAModifier, null);
      registerModifier("CHASSIS BONUS", "CHASSIS BONUS", false, true, GetMeleeChassisToHitModifier, GetMeleeChassisToHitModifierName);
      registerModifier("TERRAIN", "TERRAIN", false, true, GetTargetTerrainModifier, null);
      registerModifier("MELEE ARM MOUNTED", "__/AIM.PUNCHING_ARM/__", false, true, GetMeleeArmMount, null);
      registerModifier("MELEE HEIGHT DIFF", "HEIGHT DIFF", false, true, GetMeleeHeightMod, null);
      registerModifier("MELEE RECOIL", "RE ATTACK", false, true, GetRefireModifier, null);
      registerModifier("MELEE TARGET MOVED", "TARGET MOVED", false, true, GetMeleeEvesionMod, null);
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init {
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      ToHitModifiersHelper.InitHUD(__instance);
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("UpdateTooltipStrings")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.Last)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_UpdateTooltipStrings {
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
      if (modifier < 0) {
        slot.ToolTipHoverElement.BuffStrings.Add(new Text("{0} {1:+0;-#}", new object[2]
        {
          (object) description,
          (object) modifier
        }));
      } else {
        if (modifier <= 0)
          return;
        slot.ToolTipHoverElement.DebuffStrings.Add(new Text("{0} {1:+0;-#}", new object[2]
        {
          (object) description,
          (object) modifier
        }));
      }
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
        }
        if((modifier!=0)&&(string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
        }
        all_modifiers += modifier;
      }
      if ((all_modifiers < 0) && (Combat.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) { all_modifiers = 0; };
      slot.ToolTipHoverElement.BasicModifierInt = all_modifiers;
    }
    public static void UpdateToolTipsFiring(this CombatHUDWeaponSlot slot, ICombatant target) {
      CombatGameState Combat = Traverse.Create(slot).Field<CombatGameState>("Combat").Value;
      CombatHUD HUD = Traverse.Create(slot).Field<CombatHUD>("HUD").Value;
      MeleeAttackType meleeAttackType = MeleeAttackType.NotSet;
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
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        string name = mod.Value.name;
        if (mod.Value.dname != null) {
          name = mod.Value.dname(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          target,
          HUD.SelectionHandler.ActiveState.PreviewPos, target.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        }
        if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
        }
        all_modifiers += modifier;
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
        int modifier = (int)mod.Value.modifier(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          HUD.SelectedActor,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        string name = mod.Value.name;
        if (mod.Value.dname != null) {
          name = mod.Value.dname(Combat.ToHit,
          HUD.SelectedActor, slot.DisplayedWeapon,
          HUD.SelectedActor,
          HUD.SelectionHandler.ActiveState.PreviewPos, HUD.SelectedActor.TargetPosition, LineOfFireLevel.LOFClear, meleeAttackType, calledShot);
        }
        if ((modifier != 0) && (string.IsNullOrEmpty(name) == false)) {
          slot.AddToolTipDetail(name, modifier);
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
      if((__result < 0f) && (Traverse.Create(__instance).Field<CombatGameState>("combat").Value.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) {
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
      if ((__result < 0f) && (Traverse.Create(__instance).Field<CombatGameState>("combat").Value.Constants.ResolutionConstants.AllowTotalNegativeModifier == false)) {
        __result = 0f;
      }
    }
  }
}