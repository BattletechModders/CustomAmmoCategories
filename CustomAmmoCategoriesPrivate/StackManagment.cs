using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;

namespace CustomAmmoCategoriesPrivate {
  [HarmonyPatch(typeof(StackManager))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Update")]
  [HarmonyPatch(new Type[] { })]
  public static class StackManager_Update {
    private static bool call_original = false;
    public static bool Prepare() { return false; }
    public static bool Prefix(StackManager __instance) {
      if (call_original) { return true; }
      call_original = true;
      try {
        if ((__instance.SequenceStack.Count != 0) || (__instance.ParallelStack.Count != 0)) {
          Log.P?.TWL(0, "StackManager.Update");
          foreach (var seq in __instance.SequenceStack) {
            MultiSequence mseq = seq as MultiSequence;
            if (mseq == null) {
              Log.P?.WL(1, $"stack {seq.GetType()}:{seq.SequenceGUID} IsPaused:{seq.IsPaused} IsComplete:{seq.IsComplete}");
            } else {
              Log.P?.WL(1, $"stack {mseq.GetType()}:{mseq.SequenceGUID} IsPaused:{mseq.IsPaused} IsComplete:{mseq.IsComplete} camera:{(mseq.cameraSequence==null?"null":mseq.cameraSequence.IsFinished.ToString())}");
              if (mseq.cameraSequence != null) Log.P?.WL(2, $"{mseq.cameraSequence.GetType()}:{mseq.cameraSequence.state}");
            }
          }
          foreach (var seq in __instance.ParallelStack) {
            Log.P?.WL(1, $"parallel {seq.GetType()}:{seq.SequenceGUID} IsPaused:{seq.IsPaused} IsComplete:{seq.IsComplete}");
          }
        }
        __instance.Update();
      } catch (Exception e) {
        Log.P?.TWL(0, e.ToString(),true);
      }
      call_original = false;
      return false;
    }
  }
}