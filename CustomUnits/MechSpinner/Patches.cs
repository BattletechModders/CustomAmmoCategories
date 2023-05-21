using BattleTech;
using BattleTech.UI;
using HarmonyLib;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace MechSpin.Patches {
  [HarmonyPatch(typeof(MechBayPanel), "Init", typeof(SimGameState))]
  public static class MechBayPanel_Init_Patch {
    public static void Postfix(MechBayPanel __instance) {
      Main.SetupUI(__instance.GetRepresentation().transform);
    }
  }
  [HarmonyPatch(typeof(MechRepresentationSimGame), "Init")]
  public static class MechRepresentationSimGame_Init_Patch {
    public static void Postfix(MechRepresentationSimGame __instance) {
      CustomUnits.Log.M?.TWL(0, "Main.SetupSpin "+__instance.transform.name);
      Main.SetupSpin(__instance.gameObject);
    }
  }
  [HarmonyPatch(typeof(MouseRotation), "Update")]
  public static class MouseRotation_Update {
    public static bool Prefix(MouseRotation __instance) {
      return false;
    }
  }
}