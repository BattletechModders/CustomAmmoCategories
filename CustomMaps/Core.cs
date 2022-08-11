using BattleTech;
using BattleTech.Data;
using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomMaps {
  public class CMSettings {
    public bool debugLog { get; set; } = false;
  }
  public static class Core {
    public static CMSettings Settings { get; set; } = new CMSettings();
    public static HarmonyInstance HarmonyInstance = null;
    private static Dictionary<string, CustomMapDef> m_customMaps = new Dictionary<string, CustomMapDef>();
    public static CustomMapDef currentCustomMap { get; private set; }
    public static void PushCustomMap(string mapid, string mappath, out string basedMapId, out string basedMapPath) {
      basedMapId = mapid;
      basedMapPath = mappath;
      currentCustomMap = findCustomMap(mapid);
      if(currentCustomMap != null) {
        Map_MDD basedMap = MetadataDatabase.Instance.GetMapByID(currentCustomMap.BasedOn);
        basedMapId = basedMap.MapID;
        basedMapPath = basedMap.MapPath;
      }
    }
    public static CustomMapDef findCustomMap(string mapid) {
      if(m_customMaps.TryGetValue(mapid,out var result)) {
        return result;
      }
      return null;
    }
    public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources) {
      Log.M?.TWL(0, "FinishedLoading", true);
      try {
        //List<Map_MDD> maps = BattleTech.Data.MetadataDatabase.Instance.GetAllMaps();
        //foreach(var map in maps) {
        //  Log.M?.W(1, "MapID:"+map.MapID+" tags:");
        //  foreach(var tag in map.TagSetEntry.TagSet) {
        //    Log.M?.W(1,"\"" + tag+"\",");
        //  }
        //  Log.M?.WL(0, "");
        //}
        foreach (var customResource in customResources) {
          Log.M?.TWL(0, "customResource:" + customResource.Key);
          if (customResource.Key == "CustomMapDef") {
            foreach (var custRes in customResource.Value) {
              try {
                Log.M?.WL(1, "Path:" + custRes.Value.FilePath);
                using (FileStream arg = new FileStream(custRes.Value.FilePath, FileMode.Open, FileAccess.Read)) {
                  StreamReader streamReader = new StreamReader((Stream)arg);
                  string content = streamReader.ReadToEnd();
                  CustomMapDef def = JsonConvert.DeserializeObject<CustomMapDef>(content);
                  Map_MDD basedOn = BattleTech.Data.MetadataDatabase.Instance.GetMapByID(def.BasedOn);
                  if (basedOn == null) {
                    Log.M_err?.WL(1,"map:"+ def.BasedOn+" is not exists");
                    continue;
                  }
                  if (m_customMaps.ContainsKey(def.Id)) { m_customMaps[def.Id] = def; } else { m_customMaps.Add(def.Id, def); }
                  Map_MDD newMap = BattleTech.Data.MetadataDatabase.Instance.GetMapByID(def.Id);
                  if (newMap != null) {
                    Log.M_err?.WL(1, "already exists");
                    continue;
                  }
                  newMap = BattleTech.Data.MetadataDatabase.Instance.GetOrCreateMap(custRes.Value.FilePath);

                  BattleTech.Data.MetadataDatabase.Instance.UpdateMap(
                    newMap.MapID, def.FriendlyName, (int)basedOn.Version, basedOn.BiomeSkinEntry.BiomeSkin, def.tags, custRes.Value.FilePath, basedOn.IncludeInBuild, basedOn.Weight
                  );

                  List<EncounterLayer_MDD> newEncounters = BattleTech.Data.MetadataDatabase.Instance.EncounterLayersForMap(newMap.MapID);
                  if(newEncounters != null) {
                    if(newEncounters.Count > 0) {
                      Log.M_err?.WL(1, "encounters already exists");
                      continue;
                    }
                  }
                  List<EncounterLayer_MDD> basedEncounters = BattleTech.Data.MetadataDatabase.Instance.EncounterLayersForMap(basedOn.MapID);
                  foreach(EncounterLayer_MDD baseEnc in basedEncounters) {
                    string encGUID = Guid.NewGuid().ToString();
                    EncounterLayer_MDD newEnc = BattleTech.Data.MetadataDatabase.Instance.GetOrCreateEncounterLayer(newMap.MapID, encGUID);
                    BattleTech.Data.MetadataDatabase.Instance.UpdateEncounterLayer(
                      newMap.MapID, encGUID, baseEnc.Name, baseEnc.FriendlyName, baseEnc.Description, (int)baseEnc.BattleValue
                      ,(int)baseEnc.ContractTypeID, baseEnc.TagSetEntry.TagSet, baseEnc.IncludeInBuild
                    );
                  }
                }
              } catch (Exception e) {
                Log.M?.TWL(0, custRes.Key, true);
                Log.M?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "CustomMapMetaData") {
            foreach (var custRes in customResource.Value) {
              try {
                Log.M?.WL(1, "Path:" + custRes.Value.FilePath);
              } catch (Exception e) {
                Log.M?.TWL(0, custRes.Key, true);
                Log.M?.WL(0, e.ToString(), true);
              }
            }
          }
        }
        List<Map_MDD> maps = BattleTech.Data.MetadataDatabase.Instance.GetAllMaps();
        foreach (var map in maps) {
          Log.M?.W(1, "MapID:" + map.MapID + " tags:");
          foreach (var tag in map.TagSetEntry.TagSet) {
            Log.M?.W(1, "\"" + tag + "\",");
          }
          Log.M?.WL(0, "");
        }
        BattleTech.Data.MetadataDatabase.SaveMDDToPath();
      } catch (Exception e) {
        Log.M_err.TWL(0,e.ToString(),true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Core.Settings = JsonConvert.DeserializeObject<CustomMaps.CMSettings>(settingsJson);
      Log.debugLog = Core.Settings.debugLog;
      Log.M_err?.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      try {
        HarmonyInstance = HarmonyInstance.Create("io.mission.custommaps");
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.M_err.TWL(0, e.ToString(), true);
      }
    }
  }
}
