using BattleTech;
using BattleTech.Data;
using BattleTech.Save;
using CustomAmmoCategoriesLog;
using CustomComponents;
using Dapper;
using Harmony;
using HBS.Data;
using HBS.Util;
using Mono.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(JSONSerializationUtility))]
  [HarmonyPatch("LogWarning")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class JSONSerializationUtility_JSONSerializationUtility {
    public static bool Prefix() { return false; }
  }
  [HarmonyPatch(typeof(UnityGameInstance))]
  [HarmonyPatch("Update")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class UnityGameInstance_Update {
    public static void Prefix(UnityGameInstance __instance) {
      FastDataLoadHelper.Update();
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("OnStateBitsChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] {  })]
  public static class SimGameState_OnStateBitsChanged {
    public enum InitStates {
      UNLOADED = 0,
      INITIALIZED = 1,
      DEFS_LOADED = 2,
      REQUEST_AUTO_HEADLESS_STATE_ON_READY = 3,
      HEADLESS_ON_READY_SUCCESS = 4,
      HEADLESS_STATE = 7,
      ATTACHED_UX_STATE = 8,
      UX_SYSTEMS_CREATED = 8,
      FROM_SAVE = 16, // 0x00000010
      UX_ATTACHED_PREVIOUSLY = 32, // 0x00000020
      ASYNC_ATTACHING_UX_STATE = 64, // 0x00000040
      ASYNC_LOADING_DEFS = 128, // 0x00000080
      REQUEST_ATTACH_UX_STATE = 256, // 0x00000100
      REQUEST_DEFS_LOAD = 512, // 0x00000200
      ACTIVELY_SAVING = 1024, // 0x00000400
      UPDATE_MILESTONE_ON_SAVE_LOADED = 2048, // 0x00000800
    }
    public static string InitStateToString(int state) {
      StringBuilder result = new StringBuilder();
      foreach(InitStates val in Enum.GetValues(typeof(InitStates))) {
        if ((state & (int)val) == (int)val) { result.Append(val.ToString() + ","); }
      }
      return result.ToString();
    }
    private static bool CallOriginalMethod = false;
    public static bool Prefix(SimGameState __instance) {
      if (CallOriginalMethod) { return true; }
      CallOriginalMethod = true;
      int state = (int)Traverse.Create(__instance).Field("initState").GetValue();
      int prevstate = (int)Traverse.Create(__instance).Field("previousInitState").GetValue();
      Log.P.TWL(0,"SimGameState.OnStateBitsChanged:"+(prevstate) +">" + state, true);
      Log.P.WL(1, InitStateToString(prevstate), true);
      Log.P.WL(1, InitStateToString(state), true);
      try {
        Traverse.Create(__instance).Method("OnStateBitsChanged").GetValue();
      }catch(Exception e) {
        Log.P.TWL(0, e.ToString(), true);
      }
      CallOriginalMethod = false;
      return false;
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("_OnInit")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SimGameDifficulty) })]
  public static class SimGameState__OnInit {
    public static void Prefix(SimGameState __instance, GameInstance game, SimGameDifficulty difficulty) {
      FastDataLoadHelper.Reset();
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("_OnHeadlessComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState__OnHeadlessComplete {
    public static void Postfix(SimGameState __instance, ref bool __result) {
      Log.P.TWL(0, "SimGameState._OnHeadlessComplete:" + __result, true);
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("HandleSaveHydrate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_HandleSaveHydrate {
    public static void SetInitStateBits(this SimGameState sim, int flag) {
      MethodInfo m_SetInitStateBits = typeof(SimGameState).GetMethod("SetInitStateBits", BindingFlags.Instance | BindingFlags.NonPublic);
      m_SetInitStateBits.Invoke(sim, new object[] { (object)flag });
    }
    public static bool Prefix(SimGameState __instance, ref bool __result,ref GameInstanceSave ___save) {
      Log.P.TWL(0, "SimGameState.HandleSaveHydrate", true);
      try {
        if (___save == null) { __result = true; return false; }
        try {
          if (DebugBridge.TestToolsEnabled && BattleTech.Save.SaveGameStructure.SaveGameStructure.ForceSimGameRehydrateFailure) {
            __result = false; return false;
          }
          __instance.Rehydrate(___save);
          if (___save.SimGameSave.PreviouslyAttachedHeadState) {
            //__instance.SetInitStateBits(SimGameState.InitStates.UX_ATTACHED_PREVIOUSLY);
            __instance.SetInitStateBits(32);
          }
          Log.P.TWL(0, "SimGameState.HandleSaveHydrate success", true);
          __result = true; return false;
        } catch (Exception e) {
          Log.P.TWL(0, e.ToString(), true);
        } finally {
          ___save = (GameInstanceSave)null;
        }
        __result = false;
        return false;
      } catch (Exception e) {
        Log.P.TWL(0,e.ToString(),true);
        __result = false;
        return false;
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RespondToDefsLoadComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LoadRequest) })]
  public static class SimGameState_RespondToDefsLoadComplete {
    public static void Prefix() {
      SimGameState_RequestDataManagerResources.timer.Stop();
      Log.P.TWL(0, "SimGameState.RespondToDefsLoadComplete:" + SimGameState_RequestDataManagerResources.timer.Elapsed.TotalSeconds, true);
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("RequestDataManagerResources")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class SimGameState_RequestDataManagerResources {
    public static Stopwatch timer = new Stopwatch();
    public class RespondToDefsLoadComplete_delegate {
      public SimGameState simgame { get; set; }
      public RespondToDefsLoadComplete_delegate(SimGameState simgame) { this.simgame = simgame; }
      public void Invoke(LoadRequest loadRequest) {
        Traverse.Create(simgame).Method("RespondToDefsLoadComplete").GetValue(loadRequest);
      }
    }
    public static bool Prefix(SimGameState __instance) {
      timer.Restart();
      if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return true; }
      //return true;
      try {
        Log.P.TWL(0, "SimGameState.RequestDataManagerResources", true);
        FastDataLoadHelper.Reset();
        FastDataLoadHelper.FastPrewarm(__instance);
        //LoadRequest loadRequest = __instance.DataManager.CreateLoadRequest(new Action<LoadRequest>(new RespondToDefsLoadComplete_delegate(__instance).Invoke), true);
        //loadRequest.ProcessRequests();
        return false;
      } catch (Exception e) {
        Log.P.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(GameInstanceSave))]
  [HarmonyPatch("RequestResourcesCustom")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager) })]
  public static class GameInstanceSave_RequestResourcesCustom {
    public static void Prefix(ref Stopwatch __state) {
      __state = new Stopwatch();
      __state.Start();
    }
    public static void Postfix(ref Stopwatch __state) {
      __state.Stop();
      Log.P.TWL(0, "GameInstanceSave.RequestResourcesCustom:" + __state.Elapsed.TotalSeconds, true);
    }
  }
  public enum FastLoadStage { Preload, Dependencies, RefreshMechs, Final, None  }
  public class MechDefCacheItem {
    public string id { get; set; }
    public string moddate { get; set; }
    public string filepath { get; set; }
  }
  public class originalData {
    public DateTime lastwriteTime { get; set; }
    public JObject content { get; set; }
  }
  public static class FixedMechDefHelper {
    private static ConcurrentDictionary<string, MechDefCacheItem> mechDefsCache = new ConcurrentDictionary<string, MechDefCacheItem>();
    private static ConcurrentDictionary<string, originalData> originalContent = new ConcurrentDictionary<string, originalData>();
    private static string fullMDDBPath;
    private static SqliteConnection connection;
    public static string DBFilePath { get; set; }
    public static string ConnectionURI => FixedMechDefHelper.CONNECTION_URI;
    private static string ActiveDbPath => FixedMechDefHelper.DBFilePath;
    private static string CONNECTION_URI => "URI=file:" + FixedMechDefHelper.ActiveDbPath;
    public static bool ConnectionOpen {
      get {
        if (!FixedMechDefHelper.DatabaseExists() || FixedMechDefHelper.connection == null)
          return false;
        return FixedMechDefHelper.connection.State != ConnectionState.Broken || (uint)FixedMechDefHelper.connection.State > 0U;
      }
    }
    public static bool DatabaseExists() {
      if (string.IsNullOrEmpty(FixedMechDefHelper.fullMDDBPath))
        FixedMechDefHelper.fullMDDBPath = FixedMechDefHelper.ActiveDbPath;
      return File.Exists(FixedMechDefHelper.fullMDDBPath);
    }
    public static void Open() {
      if (!FixedMechDefHelper.DatabaseExists()) {
        Log.P.TWL(0,"Database does not exist on disk at: " + FixedMechDefHelper.fullMDDBPath);
      } else {
        if (FixedMechDefHelper.ConnectionOpen)
          return;
        try {
          FixedMechDefHelper.connection = new SqliteConnection(FixedMechDefHelper.CONNECTION_URI);
          FixedMechDefHelper.connection.Open();
        } catch (Exception ex) {
          Log.P.TWL(0, ex.ToString());
          throw ex;
        }
      }
    }
    public static void Close(bool autoCommitTransaction = false) {
      if (ConnectionOpen == false) { return; }
      connection.Close();
      connection.Dispose();
      connection = null;
    }
    public static string GetFromCache(string id,string path) {
      if(mechDefsCache.TryGetValue(id,out MechDefCacheItem cacheItem) == false) {
        return path;
      }
      DateTime origDateTime = File.GetLastWriteTime(path);
      DateTime cacheDateTime = DateTime.ParseExact(cacheItem.moddate, FastDataLoadHelper.DATETIME_FORMAT, CultureInfo.InvariantCulture);
      TimeSpan delta = origDateTime - cacheDateTime;
      if (delta.TotalSeconds > 1.0) {
        cacheItem.filepath = path;
        cacheItem.moddate = origDateTime.ToString(FastDataLoadHelper.DATETIME_FORMAT);
      }
      //Log.P.TWL(0, "GetFromCache:" + id + " origDate:" + origDateTime.ToString(FastDataLoadHelper.DATETIME_FORMAT) + " cacheTime:" + cacheDateTime.ToString(FastDataLoadHelper.DATETIME_FORMAT) + "(" + cacheItem.moddate + ")\n" + cacheItem.filepath + "\n" + path);
      return cacheItem.filepath;
    }
    public static void StoreOriginalContent(string id, JObject json, DateTime lastWrite) {
      originalData item = new originalData() { content = json, lastwriteTime = lastWrite };
      originalContent.AddOrUpdate(id, item, (k,v)=> { return item; });
    }
    public static void Init() {
      if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return; }
      if (File.Exists(DBFilePath) == false) {
        SqliteConnection.CreateFile(DBFilePath);
      }
      FixedMechDefHelper.Open();
      string sql = string.Empty;
      if (FixedMechDefHelper.isTableExists("fixedmechdefs") == false) {
        sql = "create table fixedmechdefs (id varchar(246), moddate varchar(256), filepath varchar(4096))";
        SqliteCommand command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
      }
      sql = "SELECT * FROM fixedmechdefs";
      var cacheItems = connection.Query<MechDefCacheItem>(sql);
      foreach (MechDefCacheItem item in cacheItems) {
        mechDefsCache.AddOrUpdate(item.id, item, (k, v) => { return item; });
      }
      Log.P.TWL(0, "Files cache loaded. Records count:" + mechDefsCache.Count);
    }
    public static void Init(string directory) {
      FixedMechDefHelper.DBFilePath = Path.Combine(directory, "fixmechdefs.db");
      Init();
    }
    public static IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
      if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return null; }
      FixedMechDefHelper.Open();
      return SqlMapper.Query<T>(FixedMechDefHelper.connection, sql, param, transaction, buffered, commandTimeout, commandType);
    }
    public static void UpdateDatabase(string id, string path, DateTime lastwrite) {
      if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return; }
      if (mechDefsCache.TryGetValue(id, out MechDefCacheItem item)) {
        item.moddate = lastwrite.ToString(FastDataLoadHelper.DATETIME_FORMAT);
        item.filepath = path;
        string sql = "UPDATE fixedmechdefs SET moddate = @ins_moddate, filepath = @ins_filepath WHERE id = @ins_id";
        connection.Execute(sql, new { ins_id = id, ins_moddate = lastwrite.ToString(FastDataLoadHelper.DATETIME_FORMAT), ins_filepath = path });
      } else {
        string sql = "INSERT INTO fixedmechdefs(id, moddate, filepath) VALUES(@ins_id, @ins_moddate, @ins_filepath)";
        connection.Execute(sql, new { ins_id = id, ins_moddate = lastwrite.ToString(FastDataLoadHelper.DATETIME_FORMAT), ins_filepath = path });
      }
    }
    public static void AutoFixMechs(SimGameState simgame) {
      try {
        if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return; }
        while (FastDataLoadHelper.CheckUnitsChassis() == false) { };
        List<MechDef> list = simgame.DataManager.MechDefs.Select<KeyValuePair<string, MechDef>, MechDef>((Func<KeyValuePair<string, MechDef>, MechDef>)(pair => pair.Value)).ToList<MechDef>();
        int count = 0;
        foreach (MechDef def in list) {
          if (def.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG)) { continue; }
          if (def.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
          ++count;
        }
        Log.P.TWL(0, "MechDefs need autofix:"+count);
        AutoFixer.Shared.FixMechDef(list);
        string cacheDir = Path.Combine(Path.GetDirectoryName(FixedMechDefHelper.DBFilePath), "autofixcache");
        if (Directory.Exists(cacheDir) == false) { Directory.CreateDirectory(cacheDir); }
        count = 0;
        foreach (MechDef def in list) {
          if (def.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG)) { continue; }
          if (def.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG)) { continue; }
          def.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
          if (originalContent.TryGetValue(def.Description.Id, out originalData originalObj)) {
            JObject fixedObj = JObject.Parse(def.ToJSON());
            originalObj.content.Merge(fixedObj, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Ignore });
            if (originalObj.content["Chassis"] != null) {
              originalObj.content.Remove("Chassis");
            }
            string filePath = Path.Combine(cacheDir, def.Description.Id + ".json");
            File.WriteAllText(filePath, originalObj.content.ToString(Formatting.Indented));
            UpdateDatabase(def.Description.Id, filePath, originalObj.lastwriteTime);
            ++count;
          }          
        }
        Log.P.TWL(0, "AutoFixMechs saved to cache:" + count);
      }catch(Exception e) {
        Log.P.TWL(0,e.ToString(),true);
      }
    }
    public static bool isTableExists(string table) {
      if (CustomAmmoCategories.Settings.UseFastPreloading == false) { return false; }
      return FixedMechDefHelper.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName", (object)new { TableName = table }).Count<string>() == 1;
    }
    public static void ResetCache() {
      mechDefsCache.Clear();
      Close();
      if (File.Exists(FixedMechDefHelper.DBFilePath)) { File.Delete(FixedMechDefHelper.DBFilePath); }
      Init();
      string cacheDir = Path.Combine(Path.GetDirectoryName(FixedMechDefHelper.DBFilePath), "autofixcache");
      if (Directory.Exists(cacheDir)) {
        string[] files = Directory.GetFiles(cacheDir, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string fn in files) { File.Delete(fn); }
      }
    }
  }
  public static class FastDataLoadHelper {
    public static readonly string NOAUTOFIX_TAG = "noautofix";
    public static readonly string FAKE_VEHICLE_TAG = "fake_vehicle";
    public static readonly string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.ff";
    public static readonly int THREADS_COUNT = 16;
    private static FastLoadStage stage = FastLoadStage.None;
    private static Stopwatch stageWatcher = new Stopwatch();
    public static void Reset() { stage = FastLoadStage.None; stageWatcher.Stop(); stageWatcher.Reset(); }
    public static HashSet<BattleTechResourceType> prewarmTypes = new HashSet<BattleTechResourceType>() {
      BattleTechResourceType.AbilityDef,
      BattleTechResourceType.AmmunitionBoxDef,
      BattleTechResourceType.AmmunitionDef,
      BattleTechResourceType.BackgroundDef,
      //BattleTechResourceType.BackgroundQuestionDef,
      BattleTechResourceType.BaseDescriptionDef,
      //BattleTechResourceType.BuildingDef,
      BattleTechResourceType.CastDef,
      BattleTechResourceType.ChassisDef,
      //BattleTechResourceType.DesignMaskDef,
      //BattleTechResourceType.FactionDef,
      //BattleTechResourceType.FlashpointDef,
      BattleTechResourceType.HardpointDataDef,
      //BattleTechResourceType.HeatSinkDef,
      BattleTechResourceType.HeraldryDef,
      BattleTechResourceType.JumpJetDef,
      BattleTechResourceType.LanceDef,
      BattleTechResourceType.MechDef,
      BattleTechResourceType.MovementCapabilitiesDef,
      BattleTechResourceType.PathingCapabilitiesDef,
      //BattleTechResourceType.PilotDef,
      //BattleTechResourceType.RegionDef,
      BattleTechResourceType.StarSystemDef,
      //BattleTechResourceType.TurretChassisDef,
      //BattleTechResourceType.TurretDef,
      BattleTechResourceType.UpgradeDef,
      BattleTechResourceType.VehicleChassisDef,
      BattleTechResourceType.VehicleDef,
      BattleTechResourceType.WeaponDef
    };
    public class PrewarmItem {
      public string path { get; set; }
      public string id { get; set; }
      public BattleTechResourceType resType { get; set; }
      public PrewarmItem(string p, string ID, BattleTechResourceType resType) {
        this.path = p;
        this.id = ID;
        this.resType = resType;
      }
    }
    public static Queue<PrewarmItem> prewarmQueue = new Queue<PrewarmItem>();
    public class PrewarmDelegate {
      public int index { get; set; }
      public DataManager dataManager { get; set; }
      public PrewarmDelegate(DataManager dm, int i) {
        this.dataManager = dm; index = i;
      }
      public static Type GetTypeFromBattletechResource(BattleTechResourceType bt) {
        switch (bt) {
          case BattleTechResourceType.AbilityDef: return typeof(AbilityDef);
          case BattleTechResourceType.AmmunitionBoxDef: return typeof(AmmunitionBoxDef);
          case BattleTechResourceType.AmmunitionDef: return typeof(AmmunitionDef);
          case BattleTechResourceType.BackgroundDef: return typeof(BackgroundDef);
          case BattleTechResourceType.BackgroundQuestionDef: return typeof(BackgroundQuestionDef);
          case BattleTechResourceType.BaseDescriptionDef: return typeof(BaseDescriptionDef);
          case BattleTechResourceType.BuildingDef: return typeof(BuildingDef);
          case BattleTechResourceType.CastDef: return typeof(CastDef);
          case BattleTechResourceType.ChassisDef: return typeof(ChassisDef);
          case BattleTechResourceType.DesignMaskDef: return typeof(DesignMaskDef);
          case BattleTechResourceType.FactionDef: return typeof(FactionDef);
          case BattleTechResourceType.FlashpointDef: return typeof(FlashpointDef);
          case BattleTechResourceType.HardpointDataDef: return typeof(HardpointDataDef);
          case BattleTechResourceType.HeatSinkDef: return typeof(HeatSinkDef);
          case BattleTechResourceType.HeraldryDef: return typeof(HeraldryDef);
          case BattleTechResourceType.JumpJetDef: return typeof(JumpJetDef);
          case BattleTechResourceType.LanceDef: return typeof(LanceDef);
          case BattleTechResourceType.MechDef: return typeof(MechDef);
          case BattleTechResourceType.MovementCapabilitiesDef: return typeof(MovementCapabilitiesDef);
          case BattleTechResourceType.PathingCapabilitiesDef: return typeof(PathingCapabilitiesDef);
          case BattleTechResourceType.PilotDef: return typeof(PilotDef);
          case BattleTechResourceType.RegionDef: return typeof(RegionDef);
          case BattleTechResourceType.StarSystemDef: return typeof(StarSystemDef);
          case BattleTechResourceType.TurretChassisDef: return typeof(TurretChassisDef);
          case BattleTechResourceType.TurretDef: return typeof(TurretDef);
          case BattleTechResourceType.UpgradeDef: return typeof(UpgradeDef);
          case BattleTechResourceType.VehicleChassisDef: return typeof(VehicleChassisDef);
          case BattleTechResourceType.VehicleDef: return typeof(VehicleDef);
          case BattleTechResourceType.WeaponDef: return typeof(WeaponDef);
          default: throw new Exception("Unknown type " + bt);
        }
      }
      public static string GetStoreNameFromBattletechResource(BattleTechResourceType bt) {
        switch (bt) {
          case BattleTechResourceType.AbilityDef: return "abilityDefs";
          case BattleTechResourceType.AmmunitionBoxDef: return "ammoBoxDefs";
          case BattleTechResourceType.AmmunitionDef: return "ammoDefs";
          case BattleTechResourceType.BackgroundDef: return "backgroundDefs";
          case BattleTechResourceType.BackgroundQuestionDef: return "backgroundQuestionDefs";
          case BattleTechResourceType.BaseDescriptionDef: return "baseDescriptionDefs";
          case BattleTechResourceType.BuildingDef: return "buildingDefs";
          case BattleTechResourceType.CastDef: return "castDefs";
          case BattleTechResourceType.ChassisDef: return "chassisDefs";
          case BattleTechResourceType.DesignMaskDef: return "designMaskDefs";
          case BattleTechResourceType.FactionDef: return "factions";
          case BattleTechResourceType.FlashpointDef: return "flashpointDefs";
          case BattleTechResourceType.HardpointDataDef: return "hardpointDataDefs";
          case BattleTechResourceType.HeatSinkDef: return "heatSinkDefs";
          case BattleTechResourceType.HeraldryDef: return "heraldries";
          case BattleTechResourceType.JumpJetDef: return "jumpJetDefs";
          case BattleTechResourceType.LanceDef: return "lanceDefs";
          case BattleTechResourceType.MechDef: return "mechDefs";
          case BattleTechResourceType.MovementCapabilitiesDef: return "movementCapDefs";
          case BattleTechResourceType.PathingCapabilitiesDef: return "pathingCapDefs";
          case BattleTechResourceType.PilotDef: return "pilotDefs";
          case BattleTechResourceType.RegionDef: return "regions";
          case BattleTechResourceType.StarSystemDef: return "systemDefs";
          case BattleTechResourceType.TurretChassisDef: return "turretChassisDefs";
          case BattleTechResourceType.TurretDef: return "turretDefs";
          case BattleTechResourceType.UpgradeDef: return "upgradeDefs";
          case BattleTechResourceType.VehicleChassisDef: return "vehicleChassisDefs";
          case BattleTechResourceType.VehicleDef: return "vehicleDefs";
          case BattleTechResourceType.WeaponDef: return "weaponDefs";
          default: throw new Exception("Unknown type " + bt);
        }
      }
      private static ConcurrentDictionary<object, MethodInfo> addMethods_cache = new ConcurrentDictionary<object, MethodInfo>();
      private static ConcurrentDictionary<BattleTechResourceType, FieldInfo> Field_cache = new ConcurrentDictionary<BattleTechResourceType, FieldInfo>();
      private static SpinLock cacheLock = new SpinLock();
      public static void ParceJson(ref IJsonTemplated jsonTemplate, BattleTechResourceType resType, string json) {
        switch (resType) {
          case BattleTechResourceType.AbilityDef: JSONSerializationUtility.FromJSON<AbilityDef>(jsonTemplate as AbilityDef, json); break;
          case BattleTechResourceType.AmmunitionBoxDef: JSONSerializationUtility.FromJSON<AmmunitionBoxDef>(jsonTemplate as AmmunitionBoxDef, json); break;
          case BattleTechResourceType.AmmunitionDef: JSONSerializationUtility.FromJSON<AmmunitionDef>(jsonTemplate as AmmunitionDef, json); break;
          case BattleTechResourceType.BackgroundDef: JSONSerializationUtility.FromJSON<BackgroundDef>(jsonTemplate as BackgroundDef, json); break;
          case BattleTechResourceType.BackgroundQuestionDef: JSONSerializationUtility.FromJSON<BackgroundQuestionDef>(jsonTemplate as BackgroundQuestionDef, json); break;
          case BattleTechResourceType.BaseDescriptionDef: JSONSerializationUtility.FromJSON<BaseDescriptionDef>(jsonTemplate as BaseDescriptionDef, json); break;
          case BattleTechResourceType.BuildingDef: JSONSerializationUtility.FromJSON<BuildingDef>(jsonTemplate as BuildingDef, json); break;
          case BattleTechResourceType.CastDef: JSONSerializationUtility.FromJSON<CastDef>(jsonTemplate as CastDef, json); break;
          case BattleTechResourceType.ChassisDef: JSONSerializationUtility.FromJSON<ChassisDef>(jsonTemplate as ChassisDef, json); break;
          case BattleTechResourceType.DesignMaskDef: JSONSerializationUtility.FromJSON<DesignMaskDef>(jsonTemplate as DesignMaskDef, json); break;
          case BattleTechResourceType.FactionDef: JSONSerializationUtility.FromJSON<FactionDef>(jsonTemplate as FactionDef, json); break;
          case BattleTechResourceType.FlashpointDef: JSONSerializationUtility.FromJSON<FlashpointDef>(jsonTemplate as FlashpointDef, json); break;
          case BattleTechResourceType.HardpointDataDef: JSONSerializationUtility.FromJSON<HardpointDataDef>(jsonTemplate as HardpointDataDef, json); break;
          case BattleTechResourceType.HeatSinkDef: JSONSerializationUtility.FromJSON<HeatSinkDef>(jsonTemplate as HeatSinkDef, json); break;
          case BattleTechResourceType.HeraldryDef: JSONSerializationUtility.FromJSON<HeraldryDef>(jsonTemplate as HeraldryDef, json); break;
          case BattleTechResourceType.JumpJetDef: JSONSerializationUtility.FromJSON<JumpJetDef>(jsonTemplate as JumpJetDef, json); break;
          case BattleTechResourceType.LanceDef: JSONSerializationUtility.FromJSON<LanceDef>(jsonTemplate as LanceDef, json); break;
          case BattleTechResourceType.MechDef: JSONSerializationUtility.FromJSON<MechDef>(jsonTemplate as MechDef, json); break;
          case BattleTechResourceType.MovementCapabilitiesDef: JSONSerializationUtility.FromJSON<MovementCapabilitiesDef>(jsonTemplate as MovementCapabilitiesDef, json); break;
          case BattleTechResourceType.PathingCapabilitiesDef: JSONSerializationUtility.FromJSON<PathingCapabilitiesDef>(jsonTemplate as PathingCapabilitiesDef, json); break;
          case BattleTechResourceType.PilotDef: JSONSerializationUtility.FromJSON<PilotDef>(jsonTemplate as PilotDef, json); break;
          case BattleTechResourceType.RegionDef: JSONSerializationUtility.FromJSON<RegionDef>(jsonTemplate as RegionDef, json); break;
          case BattleTechResourceType.StarSystemDef: JSONSerializationUtility.FromJSON<StarSystemDef>(jsonTemplate as StarSystemDef, json); break;
          case BattleTechResourceType.TurretChassisDef: JSONSerializationUtility.FromJSON<TurretChassisDef>(jsonTemplate as TurretChassisDef, json); break;
          case BattleTechResourceType.TurretDef: JSONSerializationUtility.FromJSON<TurretDef>(jsonTemplate as TurretDef, json); break;
          case BattleTechResourceType.UpgradeDef: JSONSerializationUtility.FromJSON<UpgradeDef>(jsonTemplate as UpgradeDef, json); break;
          case BattleTechResourceType.VehicleChassisDef: JSONSerializationUtility.FromJSON<VehicleChassisDef>(jsonTemplate as VehicleChassisDef, json); break;
          case BattleTechResourceType.VehicleDef: JSONSerializationUtility.FromJSON<VehicleDef>(jsonTemplate as VehicleDef, json); break;
          case BattleTechResourceType.WeaponDef: JSONSerializationUtility.FromJSON<WeaponDef>(jsonTemplate as WeaponDef, json); break;
          default: throw new Exception("Unknown type " + resType);
        }
      }
      public void Load() {
        do {
          PrewarmItem item = null;
          try {
            item = FastDataLoadHelper.prewarmQueue.Dequeue();
          } catch (InvalidOperationException) {
            break;
          }
          string content = string.Empty;
          try {
            if (dataManager.Exists(item.resType, item.id)) { continue; }
            //Log.M.TWL(0, "PrewarmThread[" + index + "]:" + item.resType + ":" + item.path);
            object definition = Activator.CreateInstance(GetTypeFromBattletechResource(item.resType));
            IJsonTemplated jsonTemplate = definition as IJsonTemplated;
            if (jsonTemplate == null) { continue; }
            MechDef mechDef = jsonTemplate as MechDef;
            if (mechDef == null) {
              using (var reader = new StreamReader(item.path)) {
                content = reader.ReadToEnd();
                jsonTemplate.FromJSON(content);
              }
            } else {
              string path = FixedMechDefHelper.GetFromCache(item.id, item.path);
              using (var reader = new StreamReader(path)) {
                content = reader.ReadToEnd();
                mechDef.FromJSON(content);
              }
              if (path == item.path) {
                if ((mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) && (mechDef.MechTags.Contains(FastDataLoadHelper.FAKE_VEHICLE_TAG) == false)) {
                  FixedMechDefHelper.StoreOriginalContent(item.id, JObject.Parse(content), File.GetLastWriteTime(item.path));
                  Log.P.TWL(0, item.id + " original");
                }
              } else {
                if (mechDef.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) {
                  Log.P.TWL(0, item.id + " cached. without "+ FastDataLoadHelper.NOAUTOFIX_TAG+"?");
                }
              }
            }
            //Log.M.TWL(0, "PrewarmThread[" + index + "]:parsing success");
            //ParceJson(ref jsonTemplate, item.resType, content);
            object store = null;
            if (Field_cache.TryGetValue(item.resType, out FieldInfo storeField) == false) {
              storeField = this.dataManager.GetType().GetField(GetStoreNameFromBattletechResource(item.resType), BindingFlags.Instance | BindingFlags.NonPublic);
              Field_cache.AddOrUpdate(item.resType, storeField, (k,v)=> { return storeField; });
            }
            store = storeField.GetValue(this.dataManager);
            if (store == null) {
              Log.P.TWL(0, "STORAGE: " + GetStoreNameFromBattletechResource(item.resType) + " is null");
              continue;
            }
            MethodInfo addMethod = null;
            if (addMethods_cache.TryGetValue(store, out addMethod) == false) {
              addMethod = store.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
              if (addMethod == null) {
                Log.P.TWL(0, "STORAGE: " + GetStoreNameFromBattletechResource(item.resType) + " has no Add method");
              } else {
                addMethods_cache.AddOrUpdate(store, addMethod, (k,v)=> { return addMethod; });
              }
            }
            bool locked = false;
            try {
              //Log.M.TWL(0, "PrewarmThread[" + index + "]: waiting lock");
              cacheLock.Enter(ref locked);
              //Log.M.TWL(0, "PrewarmThread[" + index + "]: lock getted");
              if (addMethod != null) addMethod.Invoke(store, new object[] { item.id, definition });
            } catch (Exception e) {
              if (locked) {
                cacheLock.Exit(); locked = false;
                //Log.M.TWL(0, "PrewarmThread[" + index + "]: lock released");
              }
              Log.P.TWL(0, "PrewarmThread[" + index + "]:" + item.id + ":" + item.path + "\n" + e.ToString(), true);
              Log.P.TWL(0, content == null ? "null" : JsonConvert.SerializeObject(definition, Formatting.Indented));
            }
            if (locked) {
              cacheLock.Exit(); locked = false;
              //Log.P.TWL(0, "PrewarmThread[" + index + "]: lock released");
            }
          } catch (Exception e) {
            Log.P.TWL(0, "PrewarmThread[" + index + "]:" + item.id + ":" + item.path + "\n" + e.ToString(), true);
            Log.P.TWL(0, content);
            //break;
          }
        } while (true);
        Log.P.TWL(0, "PrewarmThread[" + index + "]: exit", true);
      }
    }
    public static void PrewarmDoWorkDelegate(object param) {
      PrewarmDelegate threadItem = param as PrewarmDelegate;
      if (threadItem != null) { threadItem.Load(); }
    }
    public static void RespondToDefsLoadComplete(LoadRequest loadRequest) {
      Log.P.TWL(0, "RespondToDefsLoadComplete success:" + stageWatcher.Elapsed.TotalSeconds);
      try {
        Log.P.TWL(0, "Autofixing mechs");
        FixedMechDefHelper.AutoFixMechs(simgame);
        Log.P.TWL(0, "Autofixed mechs:"+ stageWatcher.Elapsed.TotalSeconds);
        Reset();
        Traverse.Create(simgame).Method("RespondToDefsLoadComplete", new Type[] { typeof(LoadRequest) }, new object[] { loadRequest }).GetValue();
      }catch(Exception e) {
        Log.P.TWL(0,e.ToString(), true);
      }
    }
    public static void AddAllOfTypeBlindLoadRequestPrewarm(this LoadRequest loadRequest,BattleTechResourceType resourceType, bool? filterByOwnerShip = false) {
      if (prewarmTypes.Contains(resourceType) == false) {
        loadRequest.AddAllOfTypeBlindLoadRequest(resourceType, filterByOwnerShip);
        return;
      }
      DataManager dataManager = Traverse.Create(loadRequest).Field<DataManager>("dataManager").Value;
      foreach (VersionManifestEntry versionManifestEntry in dataManager.ResourceLocator.AllEntriesOfResource(resourceType)) {
        if (versionManifestEntry.IsTemplate) { continue; }
        if (dataManager.Exists(resourceType, versionManifestEntry.Id)) { continue; }
        loadRequest.AddBlindLoadRequest(resourceType, versionManifestEntry.Id, filterByOwnerShip);
      }
    }
    public static void Final() {
      Log.P.TWL(0, "FastDataLoadHelper.Final:" + stageWatcher.Elapsed.TotalSeconds);
      stage = FastLoadStage.Final;
      LoadRequest loadRequest = simgame.DataManager.CreateLoadRequest(new Action<LoadRequest>(RespondToDefsLoadComplete), true);
      try {
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameEventDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.StarSystemDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.ContractOverride, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameStringList);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.LifepathNodeDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.ShopDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.FactionDef, new bool?(true));
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.FlashpointDef, new bool?(true));
        foreach (string startingMechWarrior in simgame.Constants.Story.StartingMechWarriors)
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, startingMechWarrior);
        foreach (string mechWarriorPortrait in simgame.Constants.Story.StartingMechWarriorPortraits)
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.PortraitSettings, mechWarriorPortrait);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, simgame.Constants.Story.DefaultCommanderID);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.MechDef, new bool?(true));
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_inOrbit");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_roomArgo");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrIcon_atlas");
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.HeraldryDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameConversations);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.CastDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameSpeakers);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.BackgroundDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameMilestoneDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameStatDescDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.AbilityDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.ShipModuleUpgrade);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.SimGameSubstitutionListDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.PortraitSettings);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.AudioEventDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.WeaponDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.AmmunitionDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.AmmunitionBoxDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.HeatSinkDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.UpgradeDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.JumpJetDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.MechDef);
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.VehicleDef);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimGameDifficultySettingList, "DifficultySettings");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimGameDifficultySettingList, "CareerDifficultySettings");
        foreach (SimGameCrew simGameCrew in Enum.GetValues(typeof(SimGameCrew))) {
          string resourceId = string.Format("{0}{1}{2}", (object)"castDef_", (object)simGameCrew.ToString().Substring("Crew_".Length), (object)"Default");
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.CastDef, resourceId);
        }
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, simgame.Constants.Story.StartingPlayerMech);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, simgame.Constants.Pilot.PilotPortraits);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, simgame.Constants.Pilot.PilotVoices);
        //loadRequest.AddBlindLoadRequest(BattleTechResourceType.MechDef, simgame.Constants.Story.StartingPlayerMech);
        //loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, simgame.Constants.Pilot.PilotPortraits);
        //loadRequest.AddBlindLoadRequest(BattleTechResourceType.GenderedOptionsListDef, simgame.Constants.Pilot.PilotVoices);
        foreach (PilotDef_MDD roninPilotDef in MetadataDatabase.Instance.GetRoninPilotDefs(true))
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.PilotDef, roninPilotDef.PilotDefID);
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_Ronin");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_KSBacker");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_mwrank_Commander");
        for (int index = 0; index < 5; ++index)
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, string.Format("{0}{1}{2}", (object)"uixSvgIcon_mwrank_", (object)"Rank", (object)(index + 1)));
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SVGAsset, "uixSvgIcon_generic_MechPart");
        foreach (VersionManifestEntry versionManifestEntry in simgame.DataManager.ResourceLocator.AllEntriesOfResourceFromAddendum(BattleTechResourceType.Texture2D, simgame.DataManager.ResourceLocator.GetAddendumByName(Traverse.Create(simgame).Field<string>("CONVERSATION_TEXTURE_ADDENDUM").Value)))
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Texture2D, versionManifestEntry.Id);
        foreach (VersionManifestEntry versionManifestEntry in simgame.DataManager.ResourceLocator.AllEntriesOfResourceFromAddendum(BattleTechResourceType.Sprite, simgame.DataManager.ResourceLocator.GetAddendumByName(Traverse.Create(simgame).Field<string>("PLAYER_CREST_ADDENDUM").Value)))
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, versionManifestEntry.Id);
        simgame.Player1sMercUnitHeraldryDef = simgame.Constants.Player1sMercUnitHeraldryDef;
        //MethodInfo m_RequestResources = simgame.Player1sMercUnitHeraldryDef.GetType().GetMethod("RequestResources");
        Traverse.Create(simgame.Player1sMercUnitHeraldryDef).Method("RequestResources",new Type[] { typeof(DataManager), typeof(Action) }, new object[] { simgame.DataManager, null }).GetValue();
        //__instance.Player1sMercUnitHeraldryDef.RequestResources(__instance.DataManager);
        foreach (SimGameState.SimGameCharacterType gameCharacterType in Enum.GetValues(typeof(SimGameState.SimGameCharacterType))) {
          if (gameCharacterType != SimGameState.SimGameCharacterType.UNSET) {
            string str = "TooltipSimGameCharacter" + gameCharacterType.ToString();
            if (simgame.DataManager.ResourceLocator.EntryByID(str, BattleTechResourceType.BackgroundDef) != null)
              loadRequest.AddBlindLoadRequest(BattleTechResourceType.BaseDescriptionDef, str);
          }
        }
        loadRequest.AddAllOfTypeBlindLoadRequestPrewarm(BattleTechResourceType.ItemCollectionDef, new bool?(true));
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllLightChassis");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllMediumChassis");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllHeavyChassis");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.SimpleText, "careerModeAllAssaultChassis");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrSpot_flashpointExample");
        loadRequest.AddBlindLoadRequest(BattleTechResourceType.Sprite, "uixTxrSpot_StarmapV2-Example");
      }catch(Exception e) {
        Log.P.TWL(0, e.ToString(), true);
      }
      Log.P.TWL(0, "FastDataLoadHelper.Final:" + loadRequest.GetRequestCount());
      loadRequest.ProcessRequests();
    }
    public static Queue<PilotableActorDef> validateUnitsDefs = new Queue<PilotableActorDef>();
    public static void ValidateMechDefWork() {
      do {
        PilotableActorDef item = null;
        try {
          item = FastDataLoadHelper.validateUnitsDefs.Dequeue();
        } catch (InvalidOperationException) {
          break;
        }
        try {
          if (item == null) { continue; }
          if (item.DataManager == null) { item.DataManager = simgame.DataManager; }
          MechDef mech = item as MechDef;
          if(mech != null) {
            mech.Refresh();
            if (mech.Chassis == null) {
              Log.P.TWL(0, item.Description.Id + " still has null chassis:" + item.ChassisID+" datamanager:"+item.DataManager.Exists(BattleTechResourceType.ChassisDef, item.ChassisID));
            }
            continue;
          }
          VehicleDef vehicle = item as VehicleDef;
          if (vehicle != null) {
            vehicle.Refresh();
            if (vehicle.Chassis == null) {
              Log.P.TWL(0, item.Description.Id + " still has null chassis:" + item.ChassisID + " datamanager:" + item.DataManager.Exists(BattleTechResourceType.VehicleChassisDef, item.ChassisID));
            }
            continue;
          }
        } catch(Exception e) {
          Log.P.TWL(0,e.ToString(),true);
        }
        if (FastDataLoadHelper.validateUnitsDefs.Count == 0) { break; };
      } while (true);
    }
    public static bool CheckUnitsChassis() {
      Log.P.TWL(0, "FastDataLoadHelper.CheckUnitsChassis start",true);
      HashSet<MechDef> toDeleteMech = new HashSet<MechDef>();
      HashSet<VehicleDef> toDeleteVehicle = new HashSet<VehicleDef>();
      bool result = true;
      foreach (var mech in simgame.DataManager.MechDefs) {
        if (mech.Value.Chassis == null) {
          Log.P.WL(1,mech.Key+" has no chassis "+mech.Value.ChassisID+" chassis exists:"+ simgame.DataManager.ChassisDefs.Exists(mech.Value.ChassisID));
          if (simgame.DataManager.ChassisDefs.Exists(mech.Value.ChassisID)) {
            mech.Value.Refresh();
            Log.P.WL(2, (mech.Value.Chassis == null ? "null" : mech.Value.Chassis.Description.Id));
            if (mech.Value.Chassis == null) { toDeleteMech.Add(mech.Value); result = false; }
          }
        }
      }
      foreach (var vehicle in simgame.DataManager.VehicleDefs) {
        if (vehicle.Value.Chassis == null) {
          Log.P.WL(1, vehicle.Key + " has no chassis " + vehicle.Value.ChassisID + " vehicle chassis exists:" + simgame.DataManager.VehicleChassisDefs.Exists(vehicle.Value.ChassisID));
          if (simgame.DataManager.VehicleChassisDefs.Exists(vehicle.Value.ChassisID)) {
            vehicle.Value.Refresh();
            Log.P.WL(2, (vehicle.Value.Chassis == null?"null": vehicle.Value.Chassis.Description.Id));
            if (vehicle.Value.Chassis == null) { toDeleteVehicle.Add(vehicle.Value); result = false; }
          }
        }
      }
      foreach (MechDef def in toDeleteMech) {
        Traverse.Create(simgame.DataManager).Field<DictionaryStore<MechDef>>("mechDefs").Value.Remove(def.Description.Id);
      }
      foreach (VehicleDef def in toDeleteVehicle) {
        Traverse.Create(simgame.DataManager).Field<DictionaryStore<VehicleDef>>("vehicleDefs").Value.Remove(def.Description.Id);
      }
      Log.P.TWL(0, "FastDataLoadHelper.CheckUnitsChassis end", true);
      return result;
    }
    public static void ValidateMechDefs() {
      try {
        foreach (var mech in simgame.DataManager.MechDefs) {
          if (mech.Value.Chassis == null) { validateUnitsDefs.Enqueue(mech.Value); }
          //if (mech.Value.MechTags.Contains(FastDataLoadHelper.NOAUTOFIX_TAG) == false) {
          //  mech.Value.MechTags.Add(FastDataLoadHelper.NOAUTOFIX_TAG);
          //}
        }
        foreach (var vehicle in simgame.DataManager.VehicleDefs) {
          if (vehicle.Value.Chassis == null) { validateUnitsDefs.Enqueue(vehicle.Value); }
        }
        //HashSet<Thread> threads = new HashSet<Thread>();
        //Log.P.TWL(0, "DataManager.ValidateMechs: " + validateUnitsDefs.Count, true);
        //for (int t = 0; t < 16; ++t) {
        //  Thread thread = new Thread(ValidateMechDefWork);
        //  thread.Start();
        //  threads.Add(thread);
        //}
        Log.P.TWL(0,"units validation start:"+stageWatcher.Elapsed.TotalSeconds,true);
        ValidateMechDefWork();
        Log.P.TWL(0, "units validation end:" + stageWatcher.Elapsed.TotalSeconds, true);
        while (CheckUnitsChassis() == false) { };
        stage = FastLoadStage.RefreshMechs;
      } catch (Exception e) {
        Log.P.TWL(0, e.ToString(), true);
      }
    }
    public static void PrewarmAllCompleeteDelegate() {
      //stageWatcher.Stop();
      Log.P.TWL(0, "FastDataLoadHelper.Deps success:" + stageWatcher.Elapsed.TotalSeconds);
      //ValidateMechDefs();
      Final();
      //stageWatcher.Start();
    }
    public static void PrewarmAllFailDelegate() {
      //stageWatcher.Stop();
      Log.P.TWL(0, "FastDataLoadHelper.Deps fail:" + stageWatcher.Elapsed.TotalSeconds);
      //ValidateMechDefs();
      Final();
      //stageWatcher.Start();
    }
    private static SimGameState simgame = null;
    private static Stopwatch preloadTimer = new Stopwatch();
    public static void Update() {
      if (stage == FastLoadStage.None) { return; }
      if (simgame == null) { return; }
      if (stage == FastLoadStage.Preload) {
        if (preloadTimer.IsRunning == false) { preloadTimer.Restart(); }
        if (preloadTimer.Elapsed.TotalSeconds > 10.0) {
          Log.P.TWL(0, "FastDataLoadHelper.Prewarming: " + prewarmQueue.Count+" time:"+stageWatcher.Elapsed.TotalSeconds);
          preloadTimer.Restart();
        }
        if (prewarmQueue.Count > 0) { return; }
        preloadTimer.Stop(); preloadTimer.Reset();
        ValidateMechDefs();
      }else if(stage == FastLoadStage.RefreshMechs) {
        if (preloadTimer.IsRunning == false) { preloadTimer.Restart(); }
        if (preloadTimer.Elapsed.TotalSeconds > 2.0) {
          Log.P.TWL(0, "FastDataLoadHelper.RefreshMechs: " + validateUnitsDefs.Count + " time:" + stageWatcher.Elapsed.TotalSeconds);
          preloadTimer.Restart();
        }
        if (validateUnitsDefs.Count > 0) { return; }
        preloadTimer.Stop(); preloadTimer.Reset();
        GatherDependencies();
      }
    }
    public static void PrepareDataBase() {

    }
    public static void GatherDependencies() {
      stage = FastLoadStage.Dependencies;
      Log.P.TWL(0, "FastDataLoadHelper.Prewarm complete:" + stageWatcher.Elapsed.TotalSeconds);
      DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(simgame.DataManager);
      foreach (BattleTechResourceType resType in prewarmTypes) {
        VersionManifestEntry[] manifest = simgame.DataManager.ResourceLocator.AllEntriesOfResource(resType, false);
        foreach (VersionManifestEntry entry in manifest) {
          if (entry.IsAssetBundled) { continue; }
          if (entry.IsFileAsset == false) { continue; }
          if (entry.IsTemplate) { continue; }
          DataManager.ILoadDependencies item = simgame.DataManager.Get(resType, entry.Id) as DataManager.ILoadDependencies;
          if (item == null) { continue; }
          try {
            if (item.DependenciesLoaded(10u)) { continue; }
          } catch (Exception) {
            //Log.M.TWL(0,entry.Id+":"+resType);
            //Log.M.TWL(0, e.StackTrace, true);
          }
          item.GatherDependencies(simgame.DataManager, dependencyLoad, 10u);
        }
      }
      Log.M.TWL(0, "DataManager.ProcessPrewarmRequests Dependencies:" + dependencyLoad.DependencyCount());
      if (dependencyLoad.DependencyCount() > 0) {
        dependencyLoad.RegisterLoadCompleteCallback(new Action(PrewarmAllCompleeteDelegate));
        dependencyLoad.RegisterLoadFailedCallback(new Action(PrewarmAllFailDelegate));
        simgame.DataManager.InjectDependencyLoader(dependencyLoad, 10U);
      }
    }
    public static void FastPrewarm(SimGameState sg) {
      try {
        Log.P.TWL(0, "DataManager.FastPrewarm", true);
        FastDataLoadHelper.simgame = sg;
        stage = FastLoadStage.Preload;
        stageWatcher.Start();
        foreach (BattleTechResourceType resType in prewarmTypes) {
          VersionManifestEntry[] manifest = simgame.DataManager.ResourceLocator.AllEntriesOfResource(resType, false);
          foreach (VersionManifestEntry entry in manifest) {
            if (entry.IsAssetBundled) { continue; }
            if (entry.IsFileAsset == false) { continue; }
            if (entry.IsTemplate) { continue; }
            if (simgame.DataManager.Exists(resType, entry.Id)) { continue; }
            prewarmQueue.Enqueue(new PrewarmItem(entry.FilePath, entry.Id, resType));
          }
        }
        HashSet<Thread> threads = new HashSet<Thread>();
        Log.P.TWL(0, "DataManager.FastPrewarm: "+prewarmQueue.Count, true);
        for (int t = 0; t < THREADS_COUNT; ++t) {
          Thread thread = new Thread(PrewarmDoWorkDelegate);
          thread.Start(new PrewarmDelegate(simgame.DataManager, t));
          threads.Add(thread);
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}