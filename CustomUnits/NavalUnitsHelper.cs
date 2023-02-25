/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustAmmoCategories;
using HarmonyLib;
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