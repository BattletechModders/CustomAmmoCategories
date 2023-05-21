using BattleTech;
using CustomUnits;
using HarmonyLib;
using UnityEngine;
using static MechResizer.MechResizer;

namespace MechResizer {
  [HarmonyPatch(typeof(VehicleRepresentation), "Init", new[] { typeof(Vehicle), typeof(Transform), typeof(bool) })]
  public static class GameRepresentationInitPatch {
    static void Postfix(
        Vehicle vehicle,
        Transform parentTransform,
        bool isParented,
        VehicleRepresentation __instance) {
      Log.Combat?.TWL(0, "vehicle size initialization ");
      if (vehicle == null) { return; }
      var identifier = vehicle.VehicleDef.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(vehicle.VehicleDef);
      Log.Combat?.TWL(0, $"{identifier}: {sizeMultiplier}");
      var originalLOSSourcePositions = vehicle.originalLOSSourcePositions;
      var originalLOSTargetPositions = vehicle.originalLOSTargetPositions;
      var newSourcePositions = ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      vehicle.originalLOSSourcePositions=(newSourcePositions);
      vehicle.originalLOSTargetPositions=(newTargetPositions);
      __instance.thisTransform.localScale=(sizeMultiplier);
    }
  }
}