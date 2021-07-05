using BattleTech;
using CustAmmoCategories;
using Harmony;
using System;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool) })]
  public static class Mech_InitNaval {
    public static MapTerrainHexCell findNearestWaterHex(this CombatGameState combat,Vector3 position) {
      float tempDist = 99999.9f;
      MapTerrainHexCell result = null;
      foreach (var hex in DynamicMapHelper.hexGrid) {
        if (hex.Value.centerCell == null) { continue; }
        if (SplatMapInfo.IsMapBoundary(hex.Value.centerCell.terrainMask)) { continue; }
        if (combat.EncounterLayerData.IsInEncounterBounds(hex.Key) == false) { continue; }
        if (hex.Value.centerCell.HasWater() == false) { continue; }
        float dist = Vector3.Distance(position, hex.Key);
        if ((result == null) || (tempDist > dist)) { tempDist = dist; result = hex.Value; }
      }
      return result;
    }
    public static void Prefix(Mech __instance,ref Vector3 position, float facing, bool checkEncounterCells) {
      try {
        UnitCustomInfo info = __instance.MechDef.Chassis.GetCustomInfo();
        Log.TWL(0, "Mech.Init Naval:" + __instance.Description.Id + " info:" + (info == null?"null": info.Naval.ToString()));
        if (info == null) { return; }
        if (info.Naval == false) { return; }
        MapTerrainHexCell hex = __instance.Combat.findNearestWaterHex(position);
        if(hex != null) {
          position = hex.center;
          position.y = __instance.Combat.MapMetaData.GetLerpedHeightAt(position);
          Log.WL(1, "water hex found:" + position);
        }
      } catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}