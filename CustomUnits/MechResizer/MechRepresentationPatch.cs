using BattleTech;
using CustomUnits;
using HarmonyLib;
using UnityEngine;
using static MechResizer.MechResizer;

namespace MechResizer {
  public static class MechRepresentationInitHelper {
    public static void sizeMultiplier(this MechDef def, Transform repTransform) {
      Log.Combat?.TWL(0, "mech size initialization " + repTransform.name);
      var identifier = def.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(def.Chassis);
      Log.Combat?.TWL(0, $"{identifier}: {sizeMultiplier}");
      Transform transformToScale = repTransform;
      if (MechResizer.ModSettings.MechScaleJRoot) {
        Transform j_Root = repTransform.FindRecursive("j_Root");
        if (j_Root != null) { transformToScale = j_Root; }
      }
      transformToScale.localScale = sizeMultiplier;
    }
    public static void sizeMultiplier(this Mech mech) {
      Log.Combat?.TWL(0, "mech size initialization " + mech.GameRep.name+" chassis is fake: "+ mech.MechDef.ChassisID.IsInFakeChassis());
      var identifier = mech.MechDef.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(mech.MechDef);
      Log.Combat?.TWL(0, $"{identifier}: {sizeMultiplier}");
      if (mech is CustomMech custMech) {
        custMech.ApplyScale(sizeMultiplier);
        return;
      }
      var originalLOSSourcePositions = mech.originalLOSSourcePositions;
      var originalLOSTargetPositions = mech.originalLOSTargetPositions;
      var newSourcePositions = ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      mech.originalLOSSourcePositions=(newSourcePositions);
      mech.originalLOSTargetPositions=(newTargetPositions);
      Transform transformToScale = mech.GameRep.thisTransform;
      if (MechResizer.ModSettings.MechScaleJRoot) {
        Transform j_Root = mech.GameRep.gameObject.transform.FindTopLevelChild("j_Root");
        Transform VisibleObject = mech.GameRep.VisibleObject.transform;
        if (j_Root != null) {
          transformToScale = j_Root;
          if (VisibleObject != null) { VisibleObject.localScale = sizeMultiplier; }
        }
      }
      transformToScale.localScale = sizeMultiplier;
    }
  }
}