using BattleTech;
using BattleTech.UI;
using CustomLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace CustomProfiler {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUD_Init {
    public static bool Prefix(CombatHUD __instance) {
      Profiler.framesDrawn += 1f;
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("ProcessInput")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatSelectionHandler_ProcessInput {
    public static bool Prefix(object __instance) {
      Profiler.Start("CombatSelectionHandler.ProcessInput");
      return true;
    }
    public static void Postfix(object __instance) {
      Profiler.Stop("CombatSelectionHandler.ProcessInput");
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("RemoveCompletedItems")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatSelectionHandler_RemoveCompletedItems {
    public static bool Prefix(object __instance) {
      Profiler.Start("CombatSelectionHandler.RemoveCompletedItems");
      return true;
    }
    public static void Postfix(object __instance) {
      Profiler.Stop("CombatSelectionHandler.RemoveCompletedItems");
    }
  }
  [HarmonyPatch(typeof(CombatSelectionHandler))]
  [HarmonyPatch("ProcessMousePos")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatSelectionHandler_ProcessMousePos {
    public static bool Prefix(object __instance) {
      Profiler.Start("CombatSelectionHandler.ProcessMousePos");
      return true;
    }
    public static void Postfix(object __instance) {
      Profiler.Stop("CombatSelectionHandler.ProcessMousePos");
    }
  }
  [HarmonyPatch(typeof(AuraCache))]
  [HarmonyPatch("PreviewAurasFromActorAffectingMe")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AuraCache_PreviewAurasFromActorAffectingMe {
    public static bool Prefix(object __instance) {
      Profiler.Start("CombatSelectionHandler.ProcessMousePos");
      return true;
    }
    public static void Postfix(object __instance) {
      Profiler.Stop("CombatSelectionHandler.ProcessMousePos");
    }
  }
  public class ProfilerUI:  MonoBehaviour {
    public bool keyState = false;
    public void Awake() {
      Log.M.TWL(0, "Awake", true);
    }
    public void Update() {
      bool alt = Input.GetKey(KeyCode.LeftAlt);
      bool ctrl = Input.GetKey(KeyCode.LeftControl);
      if (alt && ctrl) {
        //Log.M.TWL(0, "Update alt:" + alt + " ctrl:" + ctrl, true);
        bool key = Input.GetKey(KeyCode.P);
        if(key != keyState) {
          keyState = key;
          if (keyState) {
            Log.M.TWL(0, "Key pressed",true);
          }
        }
      }
    }
  }
  public class ProfilerRecord {
    public Stopwatch stopWatch;
    public string name;
    public TimeSpan workingTime;
    public int CallCount;
    public ProfilerRecord(string name) {
      stopWatch = new Stopwatch();
      workingTime = TimeSpan.Zero;
      this.name = name;
      CallCount = 0;
    }
    public void Start() {
      stopWatch.Start();
    }
    public void Stop() {
      CallCount += 1;
      stopWatch.Stop();
      workingTime += stopWatch.Elapsed;
      stopWatch.Reset();
    }
  }
  public static class Profiler {
    private static Stopwatch stopwatch = new Stopwatch();
    private static Dictionary<string, ProfilerRecord> records = new Dictionary<string, ProfilerRecord>();
    private static Mutex mutex = new Mutex();
    public static Thread flushThread = new Thread(flushThreadProc);
    public static bool ProfilingEnabled = false;
    public static GameObject UI = null;
    public static ProfilerUI pUI = null;
    //private static bool keyState = false;
    public static float framesDrawn = 0f;
    public static void flushThreadProc() {
      Log.M.TWL(0, "profiler flush thread started");
      while (true) {
        Thread.Sleep(10);
        bool ctrl = Input.GetKey(KeyCode.LeftControl);
        bool shift = Input.GetKey(KeyCode.LeftShift);
        bool key = Input.GetKey(KeyCode.Z);
        //Log.M.TWL(0, "Update ctrl:" + ctrl + " shift:" + shift + " key:" + key+" profiling:"+ ProfilingEnabled, true);
        if (shift && ctrl && key) {
          if(ProfilingEnabled == false) {
            Log.M.TWL(0, "Start profiling", true);
            StartProfiling();
          }
        }else
        if (ProfilingEnabled) {
          StopProfiling();
        }
        //Log.M.TWL(0, "Profiling log flushing----------------------------------------------------------------------------------------");
        //flush();
        //Log.M.TWL(0, "Profiling log flushing----------------------------------------------------------------------------------------", true);
      }
    }
    public static void flush() {
      if (mutex.WaitOne(1000)) {
        stopwatch.Stop();
        Log.M.WL(1, "overall time:" + stopwatch.Elapsed.TotalMilliseconds);
        List<ProfilerRecord> profiles = records.Values.ToList();
        profiles.Sort((x, y) => y.workingTime.CompareTo(x.workingTime));
        foreach (ProfilerRecord rec in profiles) {
          Log.M.WL(1, rec.name + " calls: " + rec.CallCount + " time:" + rec.workingTime.TotalMilliseconds);
          rec.workingTime = TimeSpan.Zero;
          rec.CallCount = 0;
        }
        stopwatch.Restart();
        mutex.ReleaseMutex();
      }
    }
    public static void PrefixUpdate(object __instance) {
      string name = __instance.GetType().ToString() + ".Update";
      Start(name);
    }
    public static void PostfixUpdate(object __instance) {
      string name = __instance.GetType().ToString() + ".Update";
      Stop(name);
    }
    public static void PrefixProcessMousePos(object __instance) {
      string name = __instance.GetType().ToString() + ".ProcessMousePos";
      CombatSelectionHandler csh = __instance as CombatSelectionHandler;
      if(csh != null) {
        if(csh.ActiveState != null) {
          Start(name + "." + csh.ActiveState.GetType().ToString()+ ".ProcessMousePos");
        }
      }
      Start(name);
    }
    public static void PostfixProcessMousePos(object __instance) {
      string name = __instance.GetType().ToString() + ".ProcessMousePos";
      CombatSelectionHandler csh = __instance as CombatSelectionHandler;
      if (csh != null) {
        if (csh.ActiveState != null) {
          Stop(name + "." + csh.ActiveState.GetType().ToString() + ".ProcessMousePos");
        }
      }
      Stop(name);
    }
    public static void StartProfiling() {
      if (mutex.WaitOne(1000)) {
        stopwatch.Stop();
        stopwatch.Reset();
        framesDrawn = 0f;
        foreach (var rec in records) {
          rec.Value.stopWatch.Stop();
          rec.Value.stopWatch.Reset();
          rec.Value.workingTime = TimeSpan.Zero;
          rec.Value.CallCount = 0;
        }
        ProfilingEnabled = true;
        mutex.ReleaseMutex();
      }
    }
    public static void StopProfiling() {
      if (mutex.WaitOne(1000)) {
        stopwatch.Stop();
        Log.M.TWL(0, "Profiling log flushing----------------------------------------------------------------------------------------");
        Log.M.WL(1, "overall time:" + stopwatch.Elapsed.TotalMilliseconds+ " total frames:"+framesDrawn+" average FPS:"+(framesDrawn*1000.0f / stopwatch.Elapsed.TotalMilliseconds));
        List<ProfilerRecord> profiles = records.Values.ToList();
        profiles.Sort((x, y) => y.workingTime.CompareTo(x.workingTime));
        foreach (ProfilerRecord rec in profiles) {
          Log.M.WL(1, rec.name + " calls: " + rec.CallCount +" time:" + rec.workingTime.TotalMilliseconds);
          rec.workingTime = TimeSpan.Zero;
          rec.CallCount = 0;
          rec.stopWatch.Stop();
          rec.stopWatch.Reset();
        }
        ProfilingEnabled = false;
        Log.M.TWL(0, "Profiling log flushing----------------------------------------------------------------------------------------",true);
        mutex.ReleaseMutex();
      }
    }

    public static void Start(string name) {
      if (ProfilingEnabled == false) { return; }
      if (mutex.WaitOne(1000)) {
        if (stopwatch.IsRunning == false) { stopwatch.Start(); }
        if (records.ContainsKey(name) == false) {
          records.Add(name, new ProfilerRecord(name));
        }
        records[name].Start();
        mutex.ReleaseMutex();
      }
    }
    public static void Stop(string name) {
      if (ProfilingEnabled == false) { return; }
      if (mutex.WaitOne(1000)) {
        if (records.ContainsKey(name) == false) { return; }
        records[name].Stop();
        mutex.ReleaseMutex();
      }
    }
    public static void Init() {
      var harmony = HarmonyInstance.Create("io.mission.profiler");
      Type[] types = typeof(WeaponEffect).Assembly.GetTypes();
      foreach (Type type in types) {
        //Log.M.WL(0,type.ToString());
        MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (MethodInfo method in methods) {
          //if ((method.Name != "Update")||(method.Name != "ProcessMousePos")) { continue; }
          //Log.M.WL(1, method.Name + ":"+method.IsPublic);
          string name = type.ToString() + "." + method.Name;
          //Log.M.W(name + " ");
          try {
            if (method.Name == "Update") {
              harmony.Patch(method,
                new HarmonyMethod(typeof(Profiler).GetMethod(nameof(PrefixUpdate), BindingFlags.Static | BindingFlags.Public)),
                new HarmonyMethod(typeof(Profiler).GetMethod(nameof(PostfixUpdate), BindingFlags.Static | BindingFlags.Public))
              );
            }/* else
            if (method.Name == "ProcessMousePos") { 
              harmony.Patch(method,
                new HarmonyMethod(typeof(Profiler).GetMethod(nameof(PrefixProcessMousePos), BindingFlags.Static | BindingFlags.Public)),
                new HarmonyMethod(typeof(Profiler).GetMethod(nameof(PostfixProcessMousePos), BindingFlags.Static | BindingFlags.Public))
              );
            }*/
          } catch (Exception) {
            //Log.M.WL("fail:" + e.ToString());
            continue;
          }
          //Log.M.WL("success");
        }
      }
      harmony.PatchAll();
      //UI = new GameObject();
      //pUI = UI.AddComponent<ProfilerUI>();
      //UI.SetActive(true);
      flushThread.Start();
    }
  }
}