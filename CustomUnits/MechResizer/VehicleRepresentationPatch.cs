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
      Log.TWL(0, "vehicle size initialization ");
      if (vehicle == null) { return; }
      var identifier = vehicle.VehicleDef.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(vehicle.VehicleDef);
      Log.TWL(0, $"{identifier}: {sizeMultiplier}");
      var originalLOSSourcePositions = Traverse.Create(vehicle).Field("originalLOSSourcePositions").GetValue<Vector3[]>();
      var originalLOSTargetPositions = Traverse.Create(vehicle).Field("originalLOSTargetPositions").GetValue<Vector3[]>();
      var newSourcePositions = ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      Traverse.Create(vehicle).Field("originalLOSSourcePositions").SetValue(newSourcePositions);
      Traverse.Create(vehicle).Field("originalLOSTargetPositions").SetValue(newTargetPositions);
      Traverse.Create(__instance.thisTransform).Property("localScale").SetValue(sizeMultiplier);
    }
  }
}