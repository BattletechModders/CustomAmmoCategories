using BattleTech;
using BattleTech.Rendering.UI;
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.UI;
using Harmony;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomDeploy{
  public static class Log {
    //private static string m_assemblyFile;
    private static string m_logfile;
    private static readonly Mutex mutex = new Mutex();
    public static string BaseDirectory;
    private static StringBuilder m_cache = new StringBuilder();
    private static StreamWriter m_fs = null;
    private static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        Log.LogWrite("flush\n");
        Log.flush();
      }
    }
    public static void InitLog() {
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomDeploy.log");
      File.Delete(Log.m_logfile);
      Log.m_fs = new StreamWriter(Log.m_logfile);
      Log.m_fs.AutoFlush = true;
      Log.flushThread.Start();
    }
    public static void flush() {
      if (Log.mutex.WaitOne(1000)) {
        Log.m_fs.Write(Log.m_cache.ToString());
        Log.m_fs.Flush();
        Log.m_cache.Length = 0;
        Log.mutex.ReleaseMutex();
      }
    }
    public static void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n", isCritical);
      } else {
        LogWrite(prefix + line, isCritical);
      }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      //try {
      if ((Core.debugLog) || (isCritical)) {
        if (Log.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          //File.AppendAllText(Log.m_logfile, line);
          Log.mutex.ReleaseMutex();
        }
        if (isCritical) { Log.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
      }
      //} catch (Exception) {
      //i'm sertanly don't know what to do
      //}
    }
    public static void W(string line, bool isCritical = false) {
      LogWrite(line, isCritical);
    }
    public static void WL(string line, bool isCritical = false) {
      line += "\n"; W(line, isCritical);
    }
    public static void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; W(line, isCritical);
    }
    public static void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; WL(line, isCritical);
    }
    public static void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line, isCritical);
    }
    public static void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line, isCritical);
    }
  }
  [HarmonyPatch(typeof(Briefing))]
  [HarmonyPatch("BeginPlaying")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Briefing_BeginPlaying {
    public static bool Prefix(Briefing __instance) {
      try {
        if (__instance.loadingState != Briefing.LoadingState.Complete)
          return false;
        LevelLoader.SetInterstitialComplete();
        __instance.messageCenter.PublishMessage((MessageCenterMessage)new EncounterBeginMessage());
        int num = (int)WwiseManager.PostEvent<AudioEventList_ui>(AudioEventList_ui.ui_generic_confirm, WwiseManager.GlobalAudioObject);
        if (__instance.loadingCameraGo != null) {
          UnityEngine.Object.Destroy(__instance.loadingCameraGo);
          __instance.loadingCameraGo = null;
        }
        CombatGameState combat = UnityGameInstance.BattleTechGame.Combat;
        if (combat != null && combat.ActiveContract != null) {
          combat.ActiveContract.StartProgress();
        }
        if (UICameraRenderer.HasInstance)
          UICameraRenderer.Instance.EnableRenderTexture();
        if (CameraFadeManager.HasInstance)
          CameraFadeManager.Instance.RefreshCamera();
        if (LazySingletonBehavior<VFXCullingController>.HasInstance)
          LazySingletonBehavior<VFXCullingController>.Instance.RefreshCamera();
        if (combat != null && combat.WasFromSave)
          combat.SetAudioFromSaveState();
        __instance.Pool();
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }

  public static class Core{
    public static string BaseDir { get; set; }
    public static bool debugLog { get; set; }
    public static HarmonyInstance HarmonyInstance = null;
    public static void Init(string directory,bool debugLog) {
      Log.BaseDirectory = directory;
      Core.debugLog = debugLog;
      Log.InitLog();
      Core.BaseDir = directory;
      Log.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      try {
        HarmonyInstance = HarmonyInstance.Create("io.mission.customdeploy");
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}
 