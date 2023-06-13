using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CustomAmmoCategoriesPrivate {
  [HarmonyPatch(typeof(StackManager))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(new Type[] { })]
  [HarmonyWrapSafe]
  public static class StackManager_OnCombatGameDestroyed {
    public static void Postfix() {
      StackManager_Update.Clear();
    }
  }
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
      return $"{seq.GetType()}: state:{seq.state} track:{(seq.trackedSequence == null?"null":seq.trackedSequence.SequenceGUID.ToString())}";
    }
    public static string SeqToString(ActorMovementSequence seq) {
      if (seq == null) { return "null"; }
      string result = $"{seq.GetType()}: owner:{(seq.owningActor == null ? "null" : seq.owningActor.PilotableActorDef.ChassisID)} "
        + $"OrdersAreComplete:{seq.OrdersAreComplete} timeMoving:{seq.timeMoving} rep:{(seq.ActorRep == null ? "null" : seq.ActorRep.gameObject.name)} MoverTransform:{(seq.MoverTransform == null ? "null" : seq.MoverTransform.name)}"
        + $"\n  cameraSequence:{(seq.cameraSequence == null ? "null" : seq.cameraSequence.SeqToString())} childSequences:{seq.childSequences.Count}";
      foreach(var childseq in seq.childSequences) {
        result += $"\n  {childseq.SeqToString()} IsComplete:{childseq.IsComplete} IsPaused:{childseq.IsPaused}";
      }
      result += "\n ";
      return result;
    }
    public static string SeqToString(this IStackSequence seq) {
      if (seq is TeamActivationSequence tseq) { return SeqToString(tseq); }
      if (seq is CameraSequence cseq) { return SeqToString(cseq); }
      if (seq is ActorMovementSequence mseq) { return SeqToString(mseq); }
      return $"{seq.GetType()}";
    }
    public static readonly float OUTPUT_TIME = 5f;
    private static StringBuilder long_log = new StringBuilder();
    private static StringBuilder short_log = new StringBuilder();
    private static void clearShortLog() { short_log.Clear(); }
    private static void TWL(string line) {
      short_log.AppendLine("[" + DateTime.UtcNow.ToString("HH:mm:ss.fff") + "]"+line);
    }
    private static void WL(string line) {
      short_log.AppendLine(line);
    }
    private static void ToLongLog() {
      long_log.Append(short_log.ToString());
      short_log.Clear();
    }
    private static void Dump() {
      string path = Path.Combine(Log.BaseDirectory, "stack_sequence.txt");
      StringBuilder combined = new StringBuilder();
      combined.Append(long_log.ToString());
      combined.AppendLine("-------------------------------------------------");
      combined.Append(short_log.ToString());
      File.WriteAllText(path, combined.ToString());
      short_log.Clear();
    }
    public static void Clear() {
      short_log.Clear();
      long_log.Clear();
      TWL("combat end");
      Dump();
    }
    public static Exception Finalizer(StackManager __instance, Exception __exception) {
      try {
        if ((__instance.SequenceStack.Count != 0) || (__instance.ParallelStack.Count != 0)) {
          clearShortLog();
          HashSet<IStackSequence> deleteSequences = sequence_timeout.Keys.ToHashSet();
          bool seqAdded = false;
          bool seqRemoved = false;
          foreach (var seq in __instance.SequenceStack) {
            deleteSequences.Remove(seq);
            if(sequence_timeout.TryGetValue(seq, out var timer) == false) {
              timer = new SequenceTimer(seq, false);
              sequence_timeout[seq] = timer;
              seqAdded = true;
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
              seqAdded = true;
            } else {
              timer.time += Time.deltaTime;
            }
          }
          foreach (var seq in deleteSequences) { sequence_timeout.Remove(seq); seqRemoved = true; }
          foreach (var timer in sequence_timeout) {
            if(timer.Value.time > 30f) {
              if(timer.Key is CameraSequence camera) {
                TWL($"Possible stuck camera sequence: {timer.Key.SeqToString()}");
                camera.setState(CameraSequence.CamSequenceState.Finished);
                time_since_output = OUTPUT_TIME;
              }
            }
            if(timer.Key is ActorMovementSequence move) {
              if(move.OrdersAreComplete && (move.IsComplete == false)) {
                TWL($"Possible stuck movement sequence: {timer.Key.SeqToString()}");
                foreach (var childseq in move.childSequences) {
                  if (childseq is CameraSequence camera) { camera.state = CameraSequence.CamSequenceState.Finished;  }
                }
                move.OnUpdate();
                if(move.IsComplete == false) {
                  TWL($"Still stuck movement sequence: {timer.Key.SeqToString()}");
                } else {
                  TWL($"Sequence unstacked: {timer.Key.SeqToString()}");
                }
                ToLongLog();
                Dump();
              }
            }
          }
          if ((time_since_output >= OUTPUT_TIME) || (seqAdded == true) || (seqRemoved == true)) {
            time_since_output = 0f;
            TWL($"Currently running sequences: {sequence_timeout.Count}");
            foreach (var timer in sequence_timeout) {
              WL($" {timer.Key.SeqToString()} GUID:{timer.Key.SequenceGUID} executing:{timer.Value.time} is parallel:{timer.Value.parallel}");
            }
            if((seqAdded == true) || (seqRemoved == true)) {
              ToLongLog();
            }
            Dump();
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