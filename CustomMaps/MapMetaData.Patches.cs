using BattleTech;
using BattleTech.Data;
using Harmony;
using System;

namespace CustomMaps.Patches {
  [HarmonyPatch(typeof(MapMetaData))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("LoadMapMetaData")]
  [HarmonyPatch(new Type[] { typeof(Contract), typeof(DataManager) })]
  public static class MapMetaData_LoadMapMetaData {
    public static void Prefix(Contract contract, ref string __state) {
      try {
        Log.M?.TWL(0, "MapMetaData.LoadMapMetaData",true);
        __state = string.Empty;
        CustomMapDef customMap = Core.findCustomMap(contract.mapName);
        if (customMap != null) {
          Log.M?.WL(1, "replacing map name from "+ contract.mapName+" to "+ customMap.BasedOn,true);
          __state = contract.mapName;
          contract.mapName = customMap.BasedOn;
        }
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
      }
    }
    public static void Postfix(Contract contract, ref string __state) {
      try {
        if (string.IsNullOrEmpty(__state) == false) {
          Log.M?.TWL(0, "MapMetaData.LoadMapMetaData restoring map name "+ __state);
          contract.mapName = __state;
        }
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
      }
    }
  }

}