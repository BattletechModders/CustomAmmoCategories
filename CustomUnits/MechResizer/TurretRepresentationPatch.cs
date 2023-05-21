using BattleTech;
using CustomUnits;
using HarmonyLib;
using UnityEngine;
using static MechResizer.MechResizer;

namespace MechResizer {
  [HarmonyPatch(typeof(TurretRepresentation), "Init", new[] { typeof(Turret), typeof(Transform), typeof(bool) })]
  public static class TurretRepresentationInitPatch {
    static void Postfix(
        Turret turret,
        Transform parentTransform,
        bool isParented,
        TurretRepresentation __instance) {
      Log.Combat?.TWL(0, "turret size initialization");
      var identifier = turret.TurretDef.ChassisID;
      var sizeMultiplier = SizeMultiplier.Get(turret.TurretDef);
      Log.Combat?.TWL(0, $"{identifier}: {sizeMultiplier}");
      var originalLOSSourcePositions = turret.originalLOSSourcePositions;
      var originalLOSTargetPositions = turret.originalLOSTargetPositions;
      var newSourcePositions = ModSettings.LOSSourcePositions(identifier, originalLOSSourcePositions, sizeMultiplier);
      var newTargetPositions = ModSettings.LOSTargetPositions(identifier, originalLOSTargetPositions, sizeMultiplier);
      turret.originalLOSSourcePositions=(newSourcePositions);
      turret.originalLOSTargetPositions=(newTargetPositions);
      __instance.thisTransform.localScale=(sizeMultiplier);
    }
  }
}