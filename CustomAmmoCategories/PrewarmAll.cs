using BattleTech;
using BattleTech.Data;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(JSONSerializationUtility))]
  [HarmonyPatch("LogWarning")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class JSONSerializationUtility_JSONSerializationUtility {
    public static bool Prefix() { return false; }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("ProcessPrewarmRequests")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(IEnumerable<PrewarmRequest>) })]
  public static class DataManager_ProcessPrewarmRequests {
    public static HashSet<BattleTechResourceType> prewarmTypes = new HashSet<BattleTechResourceType>() {
      BattleTechResourceType.AbilityDef,
      BattleTechResourceType.AmmunitionBoxDef,
      BattleTechResourceType.AmmunitionDef,
      BattleTechResourceType.BackgroundDef,
      BattleTechResourceType.BackgroundQuestionDef,
      BattleTechResourceType.BaseDescriptionDef,
      BattleTechResourceType.BuildingDef,
      BattleTechResourceType.CastDef,
      BattleTechResourceType.ChassisDef,
      BattleTechResourceType.DesignMaskDef,
      BattleTechResourceType.FactionDef,
      BattleTechResourceType.FlashpointDef,
      BattleTechResourceType.HardpointDataDef,
      BattleTechResourceType.HeatSinkDef,
      BattleTechResourceType.HeraldryDef,
      BattleTechResourceType.JumpJetDef,
      BattleTechResourceType.LanceDef,
      BattleTechResourceType.MechDef,
      BattleTechResourceType.MovementCapabilitiesDef,
      BattleTechResourceType.PathingCapabilitiesDef,
      BattleTechResourceType.PilotDef,
      BattleTechResourceType.RegionDef,
      BattleTechResourceType.StarSystemDef,
      BattleTechResourceType.TurretChassisDef,
      BattleTechResourceType.TurretDef,
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
          default: throw new Exception("Unknown type "+bt);
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
      private static Dictionary<object, MethodInfo> addMethods_cache = new Dictionary<object, MethodInfo>();
      private static Dictionary<object, FieldInfo> Field_cache = new Dictionary<object, FieldInfo>();
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
            item = DataManager_ProcessPrewarmRequests.prewarmQueue.Dequeue();
          }catch(InvalidOperationException) {
            break;
          }
          string content = string.Empty;
          try {
            if (dataManager.Exists(item.resType, item.id)) { continue; }
            Log.M.TWL(0, "PrewarmThread[" + index + "]:" + item.resType + ":" + item.path);
            object definition = Activator.CreateInstance(GetTypeFromBattletechResource(item.resType));
            IJsonTemplated jsonTemplate = definition as IJsonTemplated;
            if (jsonTemplate == null) { continue; }
            content = File.ReadAllText(item.path);
            //ParceJson(ref jsonTemplate, item.resType, content);
            bool lockTacken = false;
            try {
              cacheLock.Enter(ref lockTacken);
              jsonTemplate.FromJSON(content);
            }catch(Exception e) {
              Log.M.TWL(0,e.ToString(),true);
            }
            if (lockTacken) { cacheLock.Exit(); }
            lockTacken = false;
            object store = null;
            try {
              cacheLock.Enter(ref lockTacken);
              if (Field_cache.TryGetValue(item.resType, out FieldInfo storeField) == false) {
                storeField = this.dataManager.GetType().GetField(GetStoreNameFromBattletechResource(item.resType), BindingFlags.Instance | BindingFlags.NonPublic);
                Field_cache.Add(item.resType, storeField);
              }
              store = storeField.GetValue(this.dataManager);
            } finally {
              if (lockTacken) { cacheLock.Exit(); }
            }
            if (store == null) {
              Log.M.TWL(0,"STORAGE: "+ GetStoreNameFromBattletechResource(item.resType) + " is null");
              continue;
            }
            lockTacken = false;
            MethodInfo addMethod = null;
            try {
              cacheLock.Enter(ref lockTacken);
              if (addMethods_cache.TryGetValue(store, out addMethod) == false) {
                addMethod = store.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                if (addMethod == null) {
                  Log.M.TWL(0, "STORAGE: " + GetStoreNameFromBattletechResource(item.resType) + " has no Add method");
                } else {
                  addMethods_cache.Add(store, addMethod);
                }
              }
              try {
                if (addMethod != null) addMethod.Invoke(store, new object[] { item.id, definition });
              }catch(Exception e) {
                Log.M.TWL(0, "PrewarmThread[" + index + "]:" + item.id + ":" + item.path + "\n" + e.ToString(), true);
                Log.M.TWL(0, content == null ? "null":JsonConvert.SerializeObject(definition, Formatting.Indented));
              }
            } finally {
              if (lockTacken) { cacheLock.Exit(); }
            }
          } catch (Exception e) {
            Log.M.TWL(0, "PrewarmThread[" + index + "]:" + item.id+":"+item.path+"\n"+ e.ToString(),true);
            Log.M.TWL(0, content);
            //break;
          }
        } while (true);
        Log.M.TWL(0, "PrewarmThread[" + index + "]: exit", true);
      }
    }
    public static void PrewarmDoWorkDelegate(object param) {
      PrewarmDelegate threadItem = param as PrewarmDelegate;
      if (threadItem != null) { threadItem.Load(); }
    }
    public static void Prefix(DataManager __instance, IEnumerable<PrewarmRequest> toPrewarm) {
      try {
        //Log.M.TWL(0, "DataManager.ProcessPrewarmRequests", true);
        //foreach (BattleTechResourceType resType in prewarmTypes) {
        //  VersionManifestEntry[] manifest = __instance.ResourceLocator.AllEntriesOfResource(resType, true);
        //  foreach (VersionManifestEntry entry in manifest) {
        //    if (entry.IsAssetBundled) { continue; }
        //    if (entry.IsFileAsset == false) { continue; }
        //    prewarmQueue.Enqueue(new PrewarmItem(entry.FilePath, entry.Id, resType));
        //  }
        //}
        //HashSet<Thread> threads = new HashSet<Thread>();
        //for (int t = 0; t < 16; ++t) {
        //  Thread thread = new Thread(PrewarmDoWorkDelegate);
        //  thread.Start(new PrewarmDelegate(__instance, t));
        //  threads.Add(thread);
        //}
        //while (prewarmQueue.Count > 0) {
        //  Thread.Sleep(2000);
        //  Log.M.TWL(0, "DataManager.ProcessPrewarmRequests:"+ prewarmQueue.Count);
        //}
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}