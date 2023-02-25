using BattleTech;
using CustomUnits;
using HarmonyLib;
using UnityEngine;
using static MechResizer.MechResizer;

namespace MechResizer {
  public static class MechRepresentationInitHelper {
    public static void sizeMultiplier(this MechDef def, Transform repTransform) {
      Log.TWL(0, "mech size initialization " + repTransform.name);
      var identifier = def.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(def.Chassis);
      Log.TWL(0, $"{identifier}: {sizeMultiplier}");
      Transform transformToScale = repTransform;
      if (MechResizer.ModSettings.MechScaleJRoot) {
        Transform j_Root = repTransform.FindRecursive("j_Root");
        if (j_Root != null) { transformToScale = j_Root; }
      }
      transformToScale.localScale = sizeMultiplier;
    }
    public static void sizeMultiplier(this Mech mech) {
      Log.TWL(0, "mech size initialization " + mech.GameRep.name+" chassis is fake: "+ mech.MechDef.ChassisID.IsInFakeChassis());
      var identifier = mech.MechDef.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(mech.MechDef);
      Log.TWL(0, $"{identifier}: {sizeMultiplier}");
      if (mech is CustomMech custMech) {
        custMech.ApplyScale(sizeMultiplier);
        return;
      }
      var originalLOSSourcePositions = Traverse.Create(mech).Field("originalLOSSourcePositions").GetValue<Vector3[]>();
      var originalLOSTargetPositions = Traverse.Create(mech).Field("originalLOSTargetPositions").GetValue<Vector3[]>();
      var newSourcePositions = ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      Traverse.Create(mech).Field("originalLOSSourcePositions").SetValue(newSourcePositions);
      Traverse.Create(mech).Field("originalLOSTargetPositions").SetValue(newTargetPositions);
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