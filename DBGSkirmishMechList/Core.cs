using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
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
using UnityEngine.Events;

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
  [HarmonyPatch(typeof(SkirmishMechBayPanel), "RequestResources")]
  public static class SkirmishMechBayPanel_RequestResources {
    public class d_LanceConfiguratorDataLoaded {
      public SkirmishMechBayPanel __instance { get; set; }
      public d_LanceConfiguratorDataLoaded(SkirmishMechBayPanel i) { __instance = i; }
      public void LanceConfiguratorDataLoaded(LoadRequest loadRequest) {
        try {
          Log.TWL(0, "SkirmishMechBayPanel.LanceConfiguratorDataLoaded");
          foreach (var mech in __instance.dataManager.MechDefs) {
            Traverse.Create(__instance).Field<List<MechDef>>("stockMechs").Value.Add(mech.Value);
            Log.WL(1, mech.Key);
          }
          Traverse.Create(__instance).Method("LanceConfiguratorDataLoaded", new Type[] { typeof(LoadRequest) }, new object[] { loadRequest }).GetValue();
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
    }
    public static bool Prefix(SkirmishMechBayPanel __instance, ref List<MechDef> ___customMechs, ref List<MechDef> ___stockMechs, ref List<LanceDef> ___stockLances, ref List<LanceDef> ___customLances, ref MechBayMechStorageWidget ___mechStorageWidget) {
      try {
        Log.TWL(0, "SkirmishMechBayPanel.RequestResources");
        LoadRequest loadRequest = __instance.dataManager.CreateLoadRequest(new Action<LoadRequest>(new d_LanceConfiguratorDataLoaded(__instance).LanceConfiguratorDataLoaded));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.HeatSinkDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.UpgradeDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.WeaponDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AmmunitionBoxDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.JumpJetDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.ChassisDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.BaseDescriptionDef, new bool?(true));
        __instance.allPilots = new List<Pilot>();
        __instance.allPilotDefs = new List<PilotDef>();
        ___stockMechs = new List<MechDef>();
        foreach (string mechid in Core.settings.skirmishMeches) {
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, mechid);
        }
        ___stockLances = new List<LanceDef>();
        loadRequest.ProcessRequests();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(SkirmishMechBayPanel), "RefreshMechList")]
  public static class SkirmishMechBayPanel_RefreshMechList {
    public static bool Prefix(SkirmishMechBayPanel __instance, bool resetFilters, ref List<MechDef> ___customMechs,ref List<MechDef> ___stockMechs,ref List<LanceDef> ___stockLances,ref List<LanceDef> ___customLances,ref MechBayMechStorageWidget ___mechStorageWidget) {
      try {
        Log.TWL(0, "SkirmishMechBayPanel.RefreshMechList");
        Dictionary<string, Localize.Text> invalidMechErrors = new Dictionary<string, Localize.Text>();
        List<MechDef> validatedMechs = ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.GetValidatedMechs(__instance.dataManager, out invalidMechErrors);
        if (invalidMechErrors != null)
          Traverse.Create(__instance).Method("DisplayErrorPopup", new Type[] { typeof(Dictionary<string, Localize.Text>) }, new object[] { invalidMechErrors }).GetValue();
        ___customMechs.Clear();
        foreach (MechDef mechDef in validatedMechs) {
          mechDef.DataManager = __instance.dataManager;
          mechDef.Refresh();
          ___customMechs.Add(mechDef);
        }
        __instance.allMechs = new List<MechDef>();
        HashSet<string> allowMechDefs = new HashSet<string>();
        List<LanceDef> customLances = ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.GetValidLances();
        foreach (LanceDef lance in customLances) {
          Log.WL(1, "customLance:"+ lance.Description.Id+":"+lance.Description.Name);
          foreach (LanceDef.Unit unit in lance.LanceUnits) {
            Log.WL(2, "unit:" + unit.unitId);
            if (string.IsNullOrEmpty(unit.unitId) == false) { allowMechDefs.Add(unit.unitId); }
          }
        }
        foreach (LanceDef lance in ___stockLances) {
          Log.WL(1, "stockLance:" + lance.Description.Id + ":" + lance.Description.Name);
          foreach (LanceDef.Unit unit in lance.LanceUnits) {
            Log.WL(2, "unit:" + unit.unitId);
            if (string.IsNullOrEmpty(unit.unitId) == false) { allowMechDefs.Add(unit.unitId); }
          }
        }
        foreach (string id in Core.settings.skirmishMeches) {

          allowMechDefs.Add(id);
        }
        foreach (MechDef mechDef in ___stockMechs) {
          if (allowMechDefs.Contains(mechDef.Description.Id)) { __instance.allMechs.Add(mechDef); }
        }
        //__instance.allMechs.AddRange((IEnumerable<MechDef>)___stockMechs);
        __instance.allMechs.AddRange((IEnumerable<MechDef>)___customMechs);
        Log.WL(1, "allMechs:"+ __instance.allMechs.Count);
        ___mechStorageWidget.InitInventory(__instance.allMechs, resetFilters);
        foreach (MechBayMechUnitElement bayMechUnitElement in ___mechStorageWidget.inventory) {
          bool shouldShow = !bayMechUnitElement.MechDef.MechTags.Contains("unit_custom");
          bayMechUnitElement.SetFrameColor(shouldShow ? UIColor.StockMech : UIColor.White);
          bayMechUnitElement.ShowStockIcon(shouldShow);
          bayMechUnitElement.ShowLCNotch(false);
        }
        return false;
      }catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(SkirmishSettings_Beta), "InitializeLanceModules")]
  public static class SkirmishSettings_Beta_InitializeLanceModules {
    private static string CostToUID(int cost) { return "SP-" + (object)cost; }
    public class d_OnLanceUpdated {
      public SkirmishSettings_Beta __instance { get; set; }
      public d_OnLanceUpdated(SkirmishSettings_Beta i) { __instance = i; }
      public void OnLanceUpdated(LanceDef lance) {
        try {
          Traverse.Create(__instance).Method("OnLanceUpdated", new Type[] { typeof(LanceDef) }, new object[] { lance }).GetValue();
        }catch(Exception e) {
          Log.TWL(0,e.ToString(),true);
        }
      }
    }
    public static bool Prefix(SkirmishSettings_Beta __instance, int battleValueIdx,CloudUserSettings ___playerSettings, UIManager ___uiManager, LancePreviewPanel ___playerLancePreview, LancePreviewPanel ___opponentLancePreview, Dictionary<int, List<LanceDef>> ___lancesByBV, List<PilotDef> ___allPilots, List<LanceDef> ___allLances, List<MechDef> ___stockMechs) {
      try {
        Log.TWL(0, "SkirmishSettings_Beta.InitializeLanceModules");
        if (Traverse.Create(__instance).Property<bool>("DataLoaded").Value == false) { return false; }
        ___playerSettings.LastUsedLances.RequestAndRefresh(___uiManager.dataManager, (Action)(() =>
        {
          Log.TWL(0, "SkirmishSettings_Beta.RequestAndRefresh end");
          Traverse.Create(__instance).Field<int>("maxCBills").Value = MultiplayerMenuOptions.LanceSizeDefDefaultDefinitions[battleValueIdx].MaxCBills;
          int maxCBills = Traverse.Create(__instance).Field<int>("maxCBills").Value;
          List<int> intList = new List<int>();
          if (battleValueIdx >= MultiplayerMenuOptions.LanceSizeDefDefaultDefinitions.Count - 1) {
            intList.Add(MultiplayerMenuOptions.LanceSizeDefDefaultDefinitions[battleValueIdx].MaxCBills);
          } else {
            for (int index = battleValueIdx; index >= 0; --index)
              intList.Add(MultiplayerMenuOptions.LanceSizeDefDefaultDefinitions[index].MaxCBills);
          }
          List<LanceDef> stockLances = new List<LanceDef>();
          foreach (int key in intList) {
            stockLances.AddRange((IEnumerable<LanceDef>)___lancesByBV[key]);
          }
          List<MechDef> stockMechs = new List<MechDef>();
          HashSet<string> allowMechDefs = new HashSet<string>();
          List<LanceDef> customLances = ActiveOrDefaultSettings.CloudSettings.CustomUnitsAndLances.GetValidLances();
          foreach (LanceDef lance in customLances) {
            Log.WL(1, "customLance:" + lance.Description.Id + ":" + lance.Description.Name);
            foreach (LanceDef.Unit unit in lance.LanceUnits) {
              Log.WL(2, "unit:" + unit.unitId);
              if (string.IsNullOrEmpty(unit.unitId) == false) { allowMechDefs.Add(unit.unitId); }
            }
          }
          foreach (LanceDef lance in ___allLances) {
            Log.WL(1, "stockLance:" + lance.Description.Id + ":" + lance.Description.Name);
            foreach (LanceDef.Unit unit in lance.LanceUnits) {
              Log.WL(2, "unit:" + unit.unitId);
              if (string.IsNullOrEmpty(unit.unitId) == false) { allowMechDefs.Add(unit.unitId); }
            }
          }
          foreach (string id in Core.settings.skirmishMeches) { allowMechDefs.Add(id); }
          Log.WL(1, "stockMechs:" + ___stockMechs.Count);

          foreach (MechDef mechDef in ___stockMechs) {
            Log.WL(2, mechDef.Description.Id+ " allow:" + allowMechDefs.Contains(mechDef.Description.Id));
            if (allowMechDefs.Contains(mechDef.Description.Id)) { stockMechs.Add(mechDef); }
          }
          Log.WL(1, "stockMechs:"+ stockMechs.Count);
          ___playerLancePreview.SetData(
            "Player Lance", "Random Lance", ___uiManager.dataManager, "bf40fd39-ccf9-47c4-94a6-061809681140", 
            HeraldryDef.HeraldyrDef_SinglePlayerSkirmishPlayer1, stockLances,
            stockMechs, ___allPilots, 
            ___playerSettings.LastUsedLances[CostToUID(maxCBills)], 
            4, maxCBills, true, false, 
            new UnityAction<LanceDef>(new d_OnLanceUpdated(__instance).OnLanceUpdated), 
            ___playerLancePreview.lastLanceId);
          ___opponentLancePreview.SetData("Opposing Force", "Unknown Enemy Forces", ___uiManager.dataManager, 
            "757173dd-b4e1-4bb5-9bee-d78e623cc867", HeraldryDef.HeraldyrDef_SinglePlayerSkirmishPlayer2, 
            stockLances, stockMechs, ___allPilots, 
            ___playerSettings.LastUsedLances[CostToUID(maxCBills)], 4, maxCBills, true, false, 
            new UnityAction<LanceDef>(new d_OnLanceUpdated(__instance).OnLanceUpdated), 
            ___opponentLancePreview.lastLanceId);
          LoadingCurtain.Hide();
        }));
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(BattleTechResourceLocator), "RefreshTypedEntries")]
  public static class BattleTechResourceLocator_RefreshTypedEntries {
    public static BattleTechResourceLocator battleTechResourceLocator { get; set; } = null;
    public static void Postfix(BattleTechResourceLocator __instance) {
      try {
        battleTechResourceLocator = __instance;
        VersionManifest manifest = Traverse.Create(__instance).Property<VersionManifest>("manifest").Value;
        Log.TWL(0, $"BattleTechResourceLocator.RefreshTypedEntries manifest:{manifest.Count}");
        HashSet<string> mechNames = new HashSet<string>();
        foreach (string mech in Core.settings.skirmishMeches) { if (mechNames.Contains(mech) == false) { mechNames.Add(mech); }; }
        Log.WL(1, "searching mods:" + Core.settings.skirmishMechesMods.Count);
        HashSet<string> foldersToSearch = new HashSet<string>();
        foreach (string modName in Core.settings.skirmishMechesMods) {
          Log.WL(1, "searching mod:" + modName);
          if (ModTek.ModTek.allModDefs.ContainsKey(modName) == false) { Log.WL(2, "not exists"); continue; }
          var mod = ModTek.ModTek.allModDefs[modName];
          if (mod.LoadFail == true) { Log.WL(2, "failed to load"); continue; }
          if (mod.Enabled == false) { Log.WL(2, "disabled"); continue; }
          foreach (var entry in mod.Manifest) {
            Log.WL(2, $"entry:{entry.Id}:{entry.Type}:{entry.Path}");
            if ((entry.Type != "MechDef") && (entry.Type != "VehicleDef")) { Log.WL(3, "wrong type"); continue; }
            if (entry.IsDirectory) {
              string folder = Path.Combine(mod.Directory, entry.Path);
              foldersToSearch.Add(folder);
              Log.WL(3, $"folder {folder}");
              continue;
            }
            if (mechNames.Contains(entry.Id) == false) {
              Log.WL(1, "Add mech id from mod " + entry.Id);
              mechNames.Add(entry.Id);
            };
          }
        }
        foreach (var entry in __instance.AllEntriesOfResource(BattleTechResourceType.MechDef)) {
          if (mechNames.Contains(entry.Id)) { continue; }
          foreach (var folder in foldersToSearch) {
            if (entry.FilePath.StartsWith(folder)) {
              Log.WL(2, $"found {entry.Id}:{entry.FilePath}");
              mechNames.Add(entry.Id);
            }
          }
        }
        foreach (var entry in __instance.AllEntriesOfResource(BattleTechResourceType.VehicleDef)) {
          if (mechNames.Contains(entry.Id)) { continue; }
          foreach (var folder in foldersToSearch) {
            if (entry.FilePath.StartsWith(folder)) {
              Log.WL(2, $"found {entry.Id}:{entry.FilePath}");
              mechNames.Add(entry.Id);
            }
          }
        }
        Log.flush();
        Core.settings.skirmishMeches = mechNames.ToList();
        Log.TWL(0, "skirmishMeches");
        foreach (string id in Core.settings.skirmishMeches) {
          Log.WL(1, id);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public static partial class Core {
    public static Settings settings;
    public static HarmonyInstance harmony = null;
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Core.settings = JsonConvert.DeserializeObject<DBGSkirmishMechList.Settings>(settingsJson);
      Log.InitLog();
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      Log.LogWrite("settings:"+JsonConvert.SerializeObject(Core.settings,Formatting.Indented)+"\n", true);
      try {
        Core.harmony = HarmonyInstance.Create("io.mission.customunits");
        Core.harmony.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
    public static bool getPrefabId(object __instance, ref string __result) {
      if (Traverse.Create(__instance).Field<DataManager>("dataManager").Value == null) {
        __result = "NO_DATA_MANAGER_JAMIE_FIX_THIS";
        Log.TWL(0, "PilotAffinityManager.getPrefabId no data manager");
        return false;
      }
      return true;
    }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.TWL(0,"FinishedLoading", true);
      try {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies) {
          if (assembly.FullName.Contains("MechAffinity")) {
            Type PilotAffinityManager = assembly.GetType("MechAffinity.PilotAffinityManager");
            Type EIdType = assembly.GetType("MechAffinity.Data.EIdType");
            Log.WL(1, "PilotAffinityManager:" + (PilotAffinityManager == null ? "null" : "not null"));
            Log.WL(1, "EIdType:" + (EIdType == null ? "null" : "not null"));
            if (PilotAffinityManager != null) {
              MethodInfo getPrefabId = PilotAffinityManager.GetMethod("getPrefabId", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(AbstractActor), EIdType }, null);
              Log.WL(1, "getPrefabId:" + (getPrefabId == null ? "null" : "not null"));
              Core.harmony.Patch(getPrefabId, new HarmonyMethod(typeof(Core).GetMethod(nameof(Core.getPrefabId), BindingFlags.Static | BindingFlags.Public)));
            }
            break;
          }
        }
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}
