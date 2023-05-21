using System;
using System.Collections.Generic;
using BattleTech;
using CustomUnits;
using HBS.Collections;
using UnityEngine;

namespace MechResizer {
  public class SizeMultiplier {
    public static Vector3 Get(ChassisDef def) {
      return Get(
          identifier: def.Description.Id,
          primary_tags: def.ChassisTags,
          secondary_tags: null,
          type: def.IsVehicle()?typeof(Vehicle):typeof(Mech),
          prefab: def.PrefabBase);
    }

    public static Vector3 Get(VehicleDef def) {
      return Get(
          identifier: def.Description.Id,
          primary_tags: def.VehicleTags,
          secondary_tags: null,
          type: typeof(Vehicle),
          prefab: def.Chassis.PrefabBase);
    }

    public static Vector3 Get(TurretDef def) {
      return Get(
          identifier: def.Description.Id,
          primary_tags: def.TurretTags,
          secondary_tags: null,
          type: typeof(Turret),
          prefab: def.Chassis.PrefabBase);
    }
    public static Vector3 Get(MechDef def) {
      return Get(
          identifier: def.Description.Id,
          primary_tags: def.MechTags,
          secondary_tags: def.Chassis != null?def.Chassis.ChassisTags:null,
          type: def.IsVehicle() ? typeof(Vehicle) : typeof(Mech),
          prefab: def.Chassis.PrefabBase);
    }

    public static Vector3 Get(WeaponDef def) {
      return Get(
          identifier: def.Description.Id,
          primary_tags: def.ComponentTags,
          secondary_tags: null,
          type: typeof(Weapon),
          prefab: def.PrefabIdentifier);
    }

    private static Vector3 Get(string identifier, TagSet primary_tags, TagSet secondary_tags, Type type, string prefab = null) {
      if (Cache.Get(identifier, out var mVector)) {
        Log.Combat?.TWL(0, $"{type.FullName}: Found Cached '{identifier}'");
        return mVector;
      }
      Vector3 returnValue;
      if (TagUnderstander.ReadSizeFromTags(primary_tags, out var tSize, out string rawTag)) {
        Log.Combat?.TWL(0, $"{type.FullName}: Found By Tag '{rawTag}'");
        returnValue = Cache.Set(identifier, tSize.Value);
        return returnValue;
      }
      if (secondary_tags != null) {
        if(TagUnderstander.ReadSizeFromTags(secondary_tags, out var tsSize, out string rawsTag)) {
          Log.Combat?.TWL(0, $"{type.FullName}: Found By Tag '{rawsTag}'");
          returnValue = Cache.Set(identifier, tsSize.Value);
          return returnValue;
        }
      }
      if (Cache.Get(identifier, out mVector, simple: true)) {
        Log.Combat?.TWL(0, $"{type.FullName}: Found Simple '{identifier}");
        returnValue = Cache.Set(identifier, mVector);
        return returnValue;
      }
      if (Cache.Get(identifier, out mVector, prefab: prefab)) {
        Log.Combat?.TWL(0, $"{type.FullName}: Found prefab '{identifier}' w/ prefab '{prefab}'");
        returnValue = Cache.Set(identifier, mVector);
        return returnValue;
      }
      Log.Combat?.TWL(0, $"{type.FullName}: Using Default Size for '{identifier}'");
      returnValue = Cache.SetDefault(identifier, type);

      return returnValue;
    }

    public static void AddSet(Dictionary<string, float> set) {
      foreach (var kv in set) {
        Cache.FloatMultipliers.Add(kv.Key, kv.Value);
      }
    }

    public static void AddSet(Dictionary<string, Vector3> set) {
      foreach (var kv in set) {
        Cache.VectorMultipliers.Add(kv.Key, kv.Value);
      }
    }

    public static void AddPrefabSet(Dictionary<string, Vector3> set) {
      foreach (var kv in set) {
        Cache.PrefabMultipliers.Add(kv.Key, kv.Value);
      }
    }

    public static void AddDefault(Type type, float value) {
      Cache.DefaultFloatMultipliers.Add(type.FullName, value);
    }

    private static class Cache {
      internal static readonly Dictionary<string, float> DefaultFloatMultipliers = new Dictionary<string, float>();
      internal static readonly Dictionary<string, float> FloatMultipliers = new Dictionary<string, float>();
      internal static readonly Dictionary<string, Vector3> VectorMultipliers = new Dictionary<string, Vector3>();
      internal static readonly Dictionary<string, Vector3> PrefabMultipliers = new Dictionary<string, Vector3>();

      private static bool Get(string identifier, out Vector3 p1) {
        return VectorMultipliers.TryGetValue(identifier, out p1);
      }

      public static bool Get(string identifier, out Vector3 p1, bool simple = false, string prefab = null) {
        if (!simple && string.IsNullOrEmpty(prefab)) return Get(identifier, out p1);
        if (simple && FloatMultipliers.TryGetValue(identifier, out float floatValue)) {
          p1 = new Vector3(floatValue, floatValue, floatValue);
          return true;
        }
        if (!string.IsNullOrEmpty(prefab) && PrefabMultipliers.TryGetValue(prefab, out var vectorValue)) {
          p1 = vectorValue;
          return true;
        }

        p1 = new Vector3();
        return false;
      }

      public static Vector3 Set(string identifier, Vector3 value) {
        Log.Combat?.TWL(0, $"Setting Cache: {identifier}: [{value.x},{value.y},{value.z}]");
        VectorMultipliers.Add(identifier, value);
        return value;
      }

      private static readonly Vector3 GameDefaultMultiplier = new Vector3(1.25f, 1.25f, 1.25f);
      public static Vector3 SetDefault(string identifier, Type type) {
        if (DefaultFloatMultipliers.TryGetValue(type.FullName, out var value)) {
          return Set(identifier, new Vector3(value, value, value));
        } else {
          return Set(identifier, GameDefaultMultiplier);
        }
      }
    }
  }
}