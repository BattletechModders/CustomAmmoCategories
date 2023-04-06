using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAmmoCategoriesPrivate {
  [HarmonyPatch(typeof(StackManager))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Update")]
  [HarmonyPatch(new Type[] { })]
  [HarmonyWrapSafe]
  public static class StackManager_Update {
    internal class SequenceTimer {
      public float time = 0f;
      public bool parallel = false;
      public IStackSequence parent;
      public SequenceTimer(IStackSequence p, bool par) {
        this.parent = p;
        time = 0f;
        parallel = par;
      }
    }
    private static Dictionary<IStackSequence, SequenceTimer> sequence_timeout = new Dictionary<IStackSequence, SequenceTimer>();
    private static float time_since_output = 0f;
    public static string SeqToString(TeamActivationSequence seq) {
      if (seq == null) { return "null"; }
      if (seq.team == null) { return "Team: null ActivationSequence"; }
      if (seq.team.IsLocalPlayer) { return "Team: player ActivationSequence"; }
      return $"Team: {seq.team.Name} ActivationSequence";
    }
    public static string SeqToString(CameraSequence seq) {
      if (seq == null) { return "null"; }
      return $"{seq.GetType()}: track:{(seq.trackedSequence == null?"null":seq.trackedSequence.SequenceGUID.ToString())}";
    }
    public static string SeqToString(this IStackSequence seq) {
      if (seq is TeamActivationSequence tseq) { return SeqToString(tseq); }
      return $"{seq.GetType()}";
    }
    public static readonly float OUTPUT_TIME = 20f;
    public static Exception Finalizer(StackManager __instance, Exception __exception) {
      try {
        if ((__instance.SequenceStack.Count != 0) || (__instance.ParallelStack.Count != 0)) {
          HashSet<IStackSequence> deleteSequences = sequence_timeout.Keys.ToHashSet();
          foreach (var seq in __instance.SequenceStack) {
            deleteSequences.Remove(seq);
            if(sequence_timeout.TryGetValue(seq, out var timer) == false) {
              timer = new SequenceTimer(seq, false);
              sequence_timeout[seq] = timer;
            } else {
              timer.time += Time.deltaTime;
            }
            MultiSequence mseq = seq as MultiSequence;
          }
          foreach (var seq in __instance.ParallelStack) {
            deleteSequences.Remove(seq);
            if (sequence_timeout.TryGetValue(seq, out var timer) == false) {
              timer = new SequenceTimer(seq, true);
              sequence_timeout[seq] = timer;
            } else {
              timer.time += Time.deltaTime;
            }
          }
          foreach (var seq in deleteSequences) { sequence_timeout.Remove(seq); }
          foreach (var timer in sequence_timeout) {
            if(timer.Value.time > 30f) {
              if(timer.Key is CameraSequence camera) {
                Log.P?.TWL(0, $"Possible stuck camera sequence: {timer.Key.SeqToString()}");
                camera.setState(CameraSequence.CamSequenceState.Finished);
                time_since_output = OUTPUT_TIME;
              }
            }
          }
          if (time_since_output >= OUTPUT_TIME) {
            time_since_output = 0f;
            Log.P?.TWL(0,$"Currently running sequences: {sequence_timeout.Count}");
            foreach (var timer in sequence_timeout) {
              Log.P?.WL(1,$"{timer.Key.SeqToString()} GUID:{timer.Key.SequenceGUID} executing:{timer.Value.time} is parallel:{timer.Value.parallel}");
            }
          } else {
            time_since_output += Time.deltaTime;
          }
        } else {
          time_since_output = 0f;
          sequence_timeout.Clear();
        }
      } catch (Exception e) {
        StackManager.logger.LogException(e);
      }
      if (__exception != null) { StackManager.logger.LogException(__exception); }
      return null;
    }
  }
}