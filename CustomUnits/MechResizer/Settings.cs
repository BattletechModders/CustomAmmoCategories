using System;
using System.Collections.Generic;
using System.Text;
using BattleTech;
using CustomUnits;
using HarmonyLib;
using UnityEngine;

namespace MechResizer {
  // the properties in in here are to be a deserializer step from JSON format
  // into the C# variables used by the mod.
  public class Settings {
    public Settings() {
      losSourcePositions = new Dictionary<string, Vector3[]>();
      losTargetPositions = new Dictionary<string, Vector3[]>();
      MechScaleJRoot = false;
    }
    public bool MechScaleJRoot { get; set; }
    #region LOS settings
    public Dictionary<string, Vector3[]> losSourcePositions { private get; set; }

    public Vector3[] LOSSourcePositions(string identifier, Vector3[] originalSourcePositions, Vector3 scaleFactor) {
      if (losSourcePositions.ContainsKey(identifier)) {
        return losSourcePositions[identifier];
      }
      var newSourcePositions = new Vector3[originalSourcePositions.Length];
      StringBuilder foo = new StringBuilder();
      foo.AppendLine($"SourcePositions\nid: {identifier}");
      for (var i = 0; i < originalSourcePositions.Length; i++) {
        newSourcePositions[i] = Vector3.Scale(originalSourcePositions[i], scaleFactor);
        foo.AppendLine(
            $"{i} orig: [{originalSourcePositions[i].x},{originalSourcePositions[i].y},{originalSourcePositions[i].z}] | scaled: [{newSourcePositions[i].x},{newSourcePositions[i].y},{newSourcePositions[i].z}]");
      }
      Log.LogWrite(foo.ToString());
      losSourcePositions[identifier] = newSourcePositions;
      return losSourcePositions[identifier];
    }

    public Dictionary<string, Vector3[]> losTargetPositions { private get; set; }
    public Vector3[] LOSTargetPositions(string identifier, Vector3[] originalTargetPositions, Vector3 scaleFactor) {
      if (losTargetPositions.ContainsKey(identifier)) {
        return losTargetPositions[identifier];
      }
      StringBuilder foo = new StringBuilder();
      foo.AppendLine($"TargetPositions\nid: {identifier}");
      var newTargetPositions = new Vector3[originalTargetPositions.Length];
      for (var i = 0; i < originalTargetPositions.Length; i++) {
        newTargetPositions[i] = Vector3.Scale(originalTargetPositions[i], scaleFactor);
        foo.AppendLine(
            $"{i} orig: [{originalTargetPositions[i].x},{originalTargetPositions[i].y},{originalTargetPositions[i].z}] | scaled: [{newTargetPositions[i].x},{newTargetPositions[i].y},{newTargetPositions[i].z}]");
      }
      Log.LogWrite(foo.ToString());
      losTargetPositions[identifier] = newTargetPositions;
      return losTargetPositions[identifier];
    }
    #endregion

    #region mech settings
    public float defaultMechSizeMultiplier {
      set => SizeMultiplier.AddDefault(typeof(Mech), value);
    }
    public Dictionary<string, float> mechSizeMultipliers {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> mechSizeMultiplierVectors {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> mechSizePrefabMultipliers {
      set => SizeMultiplier.AddPrefabSet(value);
    }
    #endregion

    #region vehicle settings
    public float defaultVehicleSizeMultiplier {
      set => SizeMultiplier.AddDefault(typeof(Vehicle), value);
    }
    public Dictionary<string, float> vehicleSizeMultipliers {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> vehicleSizeMultiplierVectors {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> vehicleSizePrefabMultipliers {
      set => SizeMultiplier.AddPrefabSet(value);
    }
    #endregion

    #region turret settings
    public float defaultTurretSizeMultiplier {
      set => SizeMultiplier.AddDefault(typeof(Turret), value);
    }
    public Dictionary<string, float> turretSizeMultipliers {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> turretSizeMultiplierVectors {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> turretSizePrefabMultipliers {
      set => SizeMultiplier.AddPrefabSet(value);
    }
    #endregion

    #region projectile settings
    public float defaultProjectileSizeMultiplier {
      set => SizeMultiplier.AddDefault(typeof(Weapon), value);
    }
    public Dictionary<string, float> projectileSizeMultipliers {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> projectileSizeMultiplierVectors {
      set => SizeMultiplier.AddSet(value);
    }
    public Dictionary<string, Vector3> projectileSizePrefabMultipliers {
      set => SizeMultiplier.AddPrefabSet(value);
    }
    //        public float defaultProjectileSizeMultiplier = -1f;
    //        public float DefaultProjectileSizeMultiplier
    //        {
    //            set => defaultProjectileSizeMultiplier = value;
    //            get => defaultProjectileSizeMultiplier;
    //        }
    //
    //        public Dictionary<string, float> projectileSizeMultipliers { private get; set; }
    //        public Dictionary<string, Vector3> projectileSizeMultiplierVectors { private get; set; }
    //        public Vector3 ProjectileSizeMultiplier(string firingWeaponIdentifier)
    //        {
    //            return projectileSizeMultiplierVectors.TryGetValue(firingWeaponIdentifier, out var mVector) ? mVector :
    //                projectileSizeMultipliers.TryGetValue(firingWeaponIdentifier, out var mSimple) ? new Vector3(mSimple, mSimple, mSimple) :
    //                new Vector3(defaultProjectileSizeMultiplier, defaultProjectileSizeMultiplier, defaultProjectileSizeMultiplier);
    //        }
    #endregion

    #region debug settings
    public bool debug = false;
    #endregion
  }
}