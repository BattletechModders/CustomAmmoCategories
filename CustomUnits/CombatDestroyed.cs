using BattleTech;
using CustAmmoCategories;
using Harmony;
using System;

namespace CustomUnits {
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedMap {
    public static bool Prefix(CombatGameState __instance) {
      try {
        HardpointAnimatorHelper.Clear();
        UnitsAnimatedPartsHelper.Clear();
        //ActorMovementSequence_InitDistanceClamp.Clear();
        VTOLBodyAnimationHelper.Clear();
        CombatHUDMechwarriorTray_RefreshTeam.Clear();
        //AlternateRepresentationHelper.Clear();
        TargetingCirclesHelper.Clear();
        MoveClampHelper.Clear();
        SelectionStateJump_GetAllDFATargets.Clear();
        PathingHelper.Clear();
        StackDataHelper.Clear();
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
      return true;
    }
  }
}