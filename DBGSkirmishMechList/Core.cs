using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DBGSkirmishMechList {
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
      Log.m_logfile = Path.Combine(BaseDirectory, "log.txt");
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
      if ((Core.settings.debugLog) || (isCritical)) {
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
  public class Settings {
    public bool debugLog { get; set; }
    public List<string> skirmishMeches { get; set; }
    public List<string> skirmishMechesMods { get; set; }
    public string fallbackMech { get; set; }
    public Settings() {
      debugLog = false;
      skirmishMeches = new List<string>();
      skirmishMechesMods = new List<string>();
      fallbackMech = "mechdef_cicada_CDA-2A";
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta))]
  [HarmonyPatch("LoadLanceConfiguratorData")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SkirmishSettings_Beta_LoadLanceConfiguratorData {
    private static FieldInfo f_allLCPilots = null;
    private static FieldInfo f_allPilots = null;
    private static FieldInfo f_allLances = null;
    private static FieldInfo f_uiManager = null;
    private static FieldInfo f_stockMechs = null;
    private static HashSet<string> avaibleMeches = new HashSet<string>();
    public static bool Prepare() {
      f_allLCPilots = typeof(SkirmishSettings_Beta).GetField("allLCPilots", BindingFlags.Instance | BindingFlags.NonPublic);
      if (f_allLCPilots == null) { Log.TWL(0, "Can't find SkirmishSettings_Beta.allLCPilots", true); return false; }
      f_allPilots = typeof(SkirmishSettings_Beta).GetField("allPilots", BindingFlags.Instance | BindingFlags.NonPublic);
      if (f_allPilots == null) { Log.TWL(0, "Can't find SkirmishSettings_Beta.allPilots", true); return false; }
      f_allLances = typeof(SkirmishSettings_Beta).GetField("allLances", BindingFlags.Instance | BindingFlags.NonPublic);
      if (f_allLances == null) { Log.TWL(0, "Can't find SkirmishSettings_Beta.allLances", true); return false; }
      f_stockMechs = typeof(SkirmishSettings_Beta).GetField("stockMechs", BindingFlags.Instance | BindingFlags.NonPublic);
      if (f_stockMechs == null) { Log.TWL(0, "Can't find SkirmishSettings_Beta.stockMechs", true); return false; }
      f_uiManager = typeof(UIModule).GetField("uiManager", BindingFlags.Instance | BindingFlags.NonPublic);
      if (f_uiManager == null) { Log.TWL(0, "Can't find UIModule.n", true); return false; }
      return true;
    }
    public static List<Pilot> allLCPilots(this SkirmishSettings_Beta set) {
      return (List<Pilot>)f_allLCPilots.GetValue(set);
    }
    public static void allLCPilots(this SkirmishSettings_Beta set, List<Pilot> val) {
      f_allLCPilots.SetValue(set,val);
    }
    public static List<PilotDef> allPilots(this SkirmishSettings_Beta set) {
      return (List<PilotDef>)f_allPilots.GetValue(set);
    }
    public static void allPilots(this SkirmishSettings_Beta set, List<PilotDef> val) {
      f_allPilots.SetValue(set, val);
    }
    public static List<MechDef> stockMechs(this SkirmishSettings_Beta set) {
      return (List<MechDef>)f_stockMechs.GetValue(set);
    }
    public static void stockMechs(this SkirmishSettings_Beta set, List<MechDef> val) {
      f_stockMechs.SetValue(set, val);
    }
    public static List<LanceDef> allLances(this SkirmishSettings_Beta set) {
      return (List<LanceDef>)f_allLances.GetValue(set);
    }
    public static void allLances(this SkirmishSettings_Beta set, List<LanceDef> val) {
      f_allLances.SetValue(set, val);
    }
    public static UIManager uiManager(this UIModule set) {
      return (UIManager)f_uiManager.GetValue(set);
    }
    private static SkirmishSettings_Beta instance = null;
    private static void OnLoadCompleteStage1(LoadRequest request) {
      Log.TWL(0, "SkirmishSettings_Beta.OnLoadCompleteStage1");
      if (instance == null) { return; }
      List<LanceDef> allLances = instance.allLances();
      LoadRequest loadRequest = instance.uiManager().dataManager.CreateLoadRequest(new Action<LoadRequest>(SkirmishSettings_Beta_LoadLanceConfiguratorData.OnLoadCompleteStage2), false);
      instance.stockMechs(new List<MechDef>());
      foreach (LanceDef lance in allLances) {
        foreach(LanceDef.Unit unit in lance.LanceUnits) {
          Log.WL(0, "AddLoadRequest:"+ unit.unitId);
          loadRequest.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef,unit.unitId, (Action<string, MechDef>)((id, mechDef) => {
            MechDef def = new MechDef(mechDef, (string)null, true);
            def.Refresh();
            if (!MechValidationRules.MechIsValidForSkirmish(def, false) || def.MechTags.Contains("unit_unlocked"))
              return;
            instance.stockMechs().Add(def);
            avaibleMeches.Add(def.Description.Id);
          }), false);
        }
      }
      foreach(string addMech in Core.settings.skirmishMeches) {
        loadRequest.AddLoadRequest<MechDef>(BattleTechResourceType.MechDef, addMech, (Action<string, MechDef>)((id, mechDef) => {
          MechDef def = new MechDef(mechDef, (string)null, true);
          def.Refresh();
          instance.stockMechs().Add(def);
        }), false);
      }
      loadRequest.ProcessRequests(10U);
    }
    private static void OnLoadCompleteStage2(LoadRequest request) {
      Log.TWL(0, "SkirmishSettings_Beta.OnLoadCompleteStage2");
      List<LanceDef> allLances = instance.allLances();
      foreach(LanceDef lance in allLances) {
        for(int index =0;index < lance.LanceUnits.Length; ++index) {
          if (avaibleMeches.Contains(lance.LanceUnits[index].unitId) == false) {
            Log.WL(1,"Mech not found:" + lance.LanceUnits[index].unitId + " fallback to " + Core.settings.fallbackMech);
            lance.LanceUnits[index].unitId = Core.settings.fallbackMech;
          }
        }
      }
      typeof(SkirmishSettings_Beta).GetMethod("OnLoadComplete", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, new object[] { request });
      instance = null;
    }
    public static bool Prefix(SkirmishSettings_Beta __instance) {
      Log.TWL(0, "SkirmishSettings_Beta.LoadLanceConfiguratorData");
      try {
        SkirmishSettings_Beta_LoadLanceConfiguratorData.instance = __instance;
        LoadRequest loadRequest = __instance.uiManager().dataManager.CreateLoadRequest(new Action<LoadRequest>(SkirmishSettings_Beta_LoadLanceConfiguratorData.OnLoadCompleteStage1), false);
        __instance.allLCPilots(new List<Pilot>());
        __instance.allPilots(new List<PilotDef>());
        loadRequest.AddAllOfTypeLoadRequest<PilotDef>(BattleTechResourceType.PilotDef, (Action<string, PilotDef>)((id, def) => {
          if (!MechValidationRules.PilotIsValidForSkirmish(def))
            return;
          __instance.allPilots().Add(def);
          __instance.allLCPilots().Add(new Pilot(def, string.Format("Pilot_{0}", (object)id), true));
        }), new bool?(true));
        __instance.allLances(new List<LanceDef>());
        loadRequest.AddAllOfTypeLoadRequest<LanceDef>(BattleTechResourceType.LanceDef, (Action<string, LanceDef>)((id, def) => {
          if (!MechValidationRules.LanceIsValidForSkirmish(def, false, false))
            return;
          __instance.allLances().Add(def);
        }), new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.BaseDescriptionDef, new bool?(false));
        loadRequest.ProcessRequests(10U);
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
      return false;
    }
  }
  public static partial class Core {
    public static Settings settings;
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.settings = JsonConvert.DeserializeObject<DBGSkirmishMechList.Settings>(settingsJson);
      Log.InitLog();
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      Log.LogWrite("settings:"+JsonConvert.SerializeObject(Core.settings,Formatting.Indented)+"\n", true);
      try {
        var harmony = HarmonyInstance.Create("io.mission.customunits");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.TWL(0,"FinishedLoading", true);
      HashSet<string> mechNames = new HashSet<string>();
      foreach(string mech in Core.settings.skirmishMeches) {if (mechNames.Contains(mech) == false) { mechNames.Add(mech); }; }
      Log.WL(1, "searching mods:" + Core.settings.skirmishMechesMods.Count);
      foreach (string modName in Core.settings.skirmishMechesMods) {
        Log.WL(1, "searching mod:"+modName);
        if (ModTek.ModTek.allModDefs.ContainsKey(modName) == false) { Log.WL(2, "not exists"); continue; }
        ModTek.ModDefEx mod = ModTek.ModTek.allModDefs[modName];
        if (mod.LoadFail == true) { Log.WL(2, "failed to load"); continue; }
        if (mod.Enabled == false) { Log.WL(2, "disabled"); continue; }
        foreach (ModTek.ModEntry entry in mod.Manifest) {
          Log.WL(2, "entry:"+entry.Id);
          if (entry.Type != "MechDef") { Log.WL(3, "not mech"); continue; }
          if (mechNames.Contains(entry.Id) == false) {
            Log.WL(1, "Add mech id from mod " + entry.Id);
            mechNames.Add(entry.Id);
          };
        }
      }
      Log.flush();
      Core.settings.skirmishMeches = mechNames.ToList();
    }
  }
}
