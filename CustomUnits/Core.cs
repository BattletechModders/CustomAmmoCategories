using BattleTech;
using BattleTech.Data;
using CustomAmmoCategoriesPatches;
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

namespace CustomUnits{
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
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomUnits.log");
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
    public static void LogWrite(int initiation,string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
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
      if ((Core.Settings.debugLog) || (isCritical)) {
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
  public class CUSettings {
    public string CanPilotVehicleTag { get; set; }
    public string CannotPilotMechTag { get; set; }
    public bool debugLog { get; set; }
    public float DeathHeight { get; set; }
    public bool fixWaterHeight { get; set; }
    public float maxWaterSteepness { get; set; }
    public float deepWaterSteepness { get; set; }
    public float deepWaterDepth { get; set; }
    public float waterFlatDepth { get; set; }
    public List<string> LancesIcons { get; set; }
    public List<CustomLanceDef> Lances { get; set; }
    public int overallDeploySize { get; set; }
    public string EMPLOYER_LANCE_GUID { get; set; }
    public float CanPilotVehicleProbability { get; set; }
    public float CanPilotAlsoMechProbability { get; set; }
    public int MaxVehicleRandomPilots { get; set; }
    public string CanPilotVehicleDescription { get; set; }
    public string CanPilotBothDescription { get; set; }
    public bool ShowVehicleBays { get; set; }
    public int ArgoBaysFix { get; set; }
    public bool BaysCountExternalControl { get; set; }
    public bool CountContractMaxUnits4AsUnlimited { get; set; }
    public string PlayerControlConvoyTag { get; set; }
    public string ConvoyRouteTag { get; set; }
    public int ConvoyUnitsCount { get; set; }
    public string TransferTeamOnDeathPrefixTag { get; set; }
    public string ConvoyDenyMoveTag { get; set; }
    public float ConvoyMaxDistFromOther { get; set; }
    public float ConvoyMaxDistFromPlayer { get; set; }
    public float ConvoyMaxDistFromRoute { get; set; }
    public bool AllowVehiclesEdit { get; set; }
    public string MechBaySwitchIconMech { get; set; }
    public string MechBaySwitchIconVehicle { get; set; }
    public string MechBaySwitchIconUp { get; set; }
    public string MechBaySwitchIconDown { get; set; }
    public string ShowActiveAbilitiesIcon { get; set; }
    public string ShowPassiveAbilitiesIcon { get; set; }
    public string HideActiveAbilitiesIcon { get; set; }
    public string HidePassiveAbilitiesIcon { get; set; }
    public float DeploySpawnRadius { get; set; }
    public float DeployMaxDistanceFromOriginal { get; set; }
    public float DeployMinDistanceFromEnemy { get; set; }
    public bool DeployManual { get; set; }
    private HashSet<string> fManualDeployForbidContractTypes { get; set; }
    public List<string> ManualDeployForbidContractTypes { set {
        if (fManualDeployForbidContractTypes == null) { fManualDeployForbidContractTypes = new HashSet<string>(); }
        fManualDeployForbidContractTypes.Clear();
        foreach (string ct in value) {
          fManualDeployForbidContractTypes.Add(ct);
        }
    } }
    public HashSet<string> DeployForbidContractTypes { get { return fManualDeployForbidContractTypes; } }
    public bool DisableHotKeys { get; set; }
    public CUSettings() {
      debugLog = false;
      DeathHeight = 1f;
      fixWaterHeight = true;
      maxWaterSteepness = 29f;
      deepWaterSteepness = 41f;
      deepWaterDepth = 10f;
      waterFlatDepth = 2f;
      LancesIcons = new List<string>();
      Lances = new List<CustomLanceDef>();
      overallDeploySize = 4;
      BaysCountExternalControl = false;
      ShowVehicleBays = false;
      ArgoBaysFix = 0;
      CanPilotVehicleTag = "pilot_vehicle_crew";
      CannotPilotMechTag = "pilot_nomech_crew";
      MaxVehicleRandomPilots = 0;
      CanPilotVehicleProbability = 0.0f;
      CanPilotAlsoMechProbability = 0.0f;
      CanPilotVehicleDescription = String.Format(HumanDescriptionDef.PilotGenDetailFormatting, "Piloting", "Can pilot only vehicles");
      CanPilotBothDescription = String.Format(HumanDescriptionDef.PilotGenDetailFormatting, "Piloting", "Can pilot both mechs and vehicles"); ;
      EMPLOYER_LANCE_GUID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
      CountContractMaxUnits4AsUnlimited = true;
      PlayerControlConvoyTag = "convoy_player_control";
      TransferTeamOnDeathPrefixTag = "transfer-on-death:";
      ConvoyDenyMoveTag = "convoy_palyer_control_deny_move";
      ConvoyUnitsCount = 4;
      ConvoyMaxDistFromOther = 200f;
      ConvoyMaxDistFromPlayer = 300f;
      ConvoyMaxDistFromRoute = 30f;
      ConvoyRouteTag = "escort_convoy";
      AllowVehiclesEdit = false;
      MechBaySwitchIconMech = "mech";
      MechBaySwitchIconVehicle = "vehicle";
      MechBaySwitchIconUp = "weapon_up";
      MechBaySwitchIconDown = "weapon_down";
      ShowActiveAbilitiesIcon = "";
      ShowPassiveAbilitiesIcon = "";
      HideActiveAbilitiesIcon = "";
      HidePassiveAbilitiesIcon = "";
      DeployManual = true;
      DeploySpawnRadius = 50f;
      DeployMaxDistanceFromOriginal = 30.0f;
      DeployMinDistanceFromEnemy = 300f;
      fManualDeployForbidContractTypes = new HashSet<string>();
      DisableHotKeys = false;
    }
  }
  public static partial class Core{
    public static readonly float Epsilon = 0.001f;
    public static CUSettings Settings;
    public static void FinishedLoading(List<string> loadOrder) {
      Log.TWL(0, "FinishedLoading", true);
      try {
        foreach(string name in loadOrder) { if (name == "Mission Control") { CustomLanceHelper.MissionControlDetected(); break; }; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void InitLancesLoadoutDefault() {
      if (Core.Settings.Lances.Count > 0) {
        CustomLanceHelper.setLancesCount(Core.Settings.Lances.Count);
        for (int lanceid = 0; lanceid < Core.Settings.Lances.Count; ++lanceid) {
          CustomLanceHelper.setLanceData(lanceid, Core.Settings.Lances[lanceid].size, Core.Settings.Lances[lanceid].allow, Core.Settings.Lances[lanceid].is_vehicle);
        }
        CustomLanceHelper.setOverallDeployCount(Core.Settings.overallDeploySize);
      } else {
        CustomLanceHelper.setLancesCount(1);
        CustomLanceHelper.setLanceData(0, 4, 4, false);
        CustomLanceHelper.setOverallDeployCount(4);
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Core.Settings = JsonConvert.DeserializeObject<CustomUnits.CUSettings>(settingsJson);
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      InitLancesLoadoutDefault();
      CustomLanceHelper.BaysCount(3+(Core.Settings.BaysCountExternalControl?0:Core.Settings.ArgoBaysFix));
      MechResizer.MechResizer.Init(directory, settingsJson);
      try {
        var harmony = HarmonyInstance.Create("io.mission.customunits");
        /*Type AssetBundleTracker = typeof(WeaponEffect).Assembly.GetType("BattleTech.Assetbundles.AssetBundleTracker");
        if (AssetBundleTracker != null) {
          MethodInfo BuildObjectMap = AssetBundleTracker.GetMethod("BuildObjectMap", BindingFlags.Instance | BindingFlags.NonPublic);
          if (BuildObjectMap != null) {
            MethodInfo patched = typeof(AssetBundleTracker_BuildObjectMap).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
            if(patched != null) { 
              harmony.Patch(BuildObjectMap,null,new HarmonyMethod(patched));
              Log.TWL(0, "BattleTech.Assetbundles.AssetBundleTracker.BuildObjectMap patched");
            }
          } else {
            Log.TWL(0, "can't find BattleTech.Assetbundles.AssetBundleTracker.BuildObjectMap");
          }
        } else {
          Log.TWL(0, "can't find BattleTech.Assetbundles.AssetBundleTracker");
        }
        Type PrefabLoadRequest = typeof(DataManager).GetNestedType("PrefabLoadRequest", BindingFlags.NonPublic);
        if (AssetBundleTracker != null) {
          MethodInfo Load = PrefabLoadRequest.GetMethod("Load");
          MethodInfo patched = typeof(PrefabLoadRequest_Load).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
          if ((patched != null)&&(Load != null)) {
            harmony.Patch(Load, null, new HarmonyMethod(patched));
            Log.TWL(0, "PrefabLoadRequest.Load patched");
          }
        } else {
          Log.TWL(0, "can't find DataManager.PrefabLoadRequest");
        }*/
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        WeightedFactorHelper.PatchInfluenceMapPositionFactor(harmony);
        WeaponRepresentation_PlayWeaponEffect.i_extendedFire = extendedFireHelper.extendedFire;
      } catch (Exception e) {
        Log.LogWrite(e.ToString()+"\n");
      }
    }
  }
}
