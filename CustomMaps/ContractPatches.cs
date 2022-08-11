using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using Harmony;
using System;
using System.Linq;

namespace CustomMaps.Patches {
  [HarmonyPatch(typeof(Contract))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string ), typeof(string), typeof(ContractTypeValue), typeof(GameInstance), typeof(ContractOverride), typeof(GameContext), typeof(bool), typeof(int), typeof(int), typeof(int?) })]
  public static class Contract_Constructor {
    public static void Prefix(ref string mapName, ref string mapPath, ref string encounterLayerGuid) {
      try {
        Core.PushCustomMap(mapName, mapPath, out string basedName, out string basedPath);
        EncounterLayer_MDD customLayerMdd = MetadataDatabase.Instance.Query<EncounterLayer_MDD>("SELECT * FROM EncounterLayer WHERE MapID=@MapID AND EncounterLayerGUID=@EncounterLayerGUID", (object)new {
          MapID = mapName,
          EncounterLayerGUID = encounterLayerGuid
        }).FirstOrDefault<EncounterLayer_MDD>();
        if (customLayerMdd == null) {
          Log.M_err?.TWL(0,"Can't find encounterLevel with map:"+mapName+" and layer GUID:"+ encounterLayerGuid+". This is very wrong");
          return;
        }
        EncounterLayer_MDD basedLayerMdd = MetadataDatabase.Instance.Query<EncounterLayer_MDD>("SELECT * FROM EncounterLayer WHERE MapID=@MapID AND ContractTypeID=@ContractTypeID", (object)new {
          MapID = basedName,
          customLayerMdd.ContractTypeID
        }).FirstOrDefault<EncounterLayer_MDD>();
        if (basedLayerMdd == null) {
          Log.M_err?.TWL(0, "Can't find encounterLevel with map:" + basedName + " and contract type:" + customLayerMdd.ContractTypeID + ". This is very wrong");
          return;
        }
        if (customLayerMdd.EncounterLayerGUID != basedLayerMdd.EncounterLayerGUID) {
          Log.M?.TWL(0, "Custom map detected on contract constructor. encounterLayerGuid "+ encounterLayerGuid+"->"+ basedLayerMdd.EncounterLayerGUID);
          encounterLayerGuid = basedLayerMdd.EncounterLayerGUID;
        }
        //if (Core.currentCustomMap != null) {
        //mapName = basedName;
        //mapPath = basedPath;
        //}
      } catch (Exception e) {
        Log.M_err?.TWL(0,e.ToString());
      }
    }
  }
}