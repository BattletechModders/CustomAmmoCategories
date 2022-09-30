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
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
  public static class TerraiWaterHelper {
    public static bool HasWater(this MapTerrainDataCell cell) {
      if (cell == null) { return false; };
      if (cell.MapEncounterLayerDataCell != null) {
        if (cell.MapEncounterLayerDataCell.HasBuilding) { return false; }
      }
      //TerrainMaskFlags terrainMaskFlags = MapMetaData.GetPriorityTerrainMaskFlags(cell);
      return SplatMapInfo.IsDeepWater(cell.terrainMask) || SplatMapInfo.IsWater(cell.terrainMask);
    }
    //public static void UpdateWaterHeightSelf(this MapTerrainDataCell cell) {
    //  if (cell.HasWater() == false) { return; }
    //  MapTerrainDataCellEx ecell = cell as MapTerrainDataCellEx;
    //  if (ecell == null) { return; }
    //  if (ecell.waterLevelCached) { return; }
    //  ecell.UpdateWaterHeightRay();
    //}
    //public static void UpdateWaterHeightRay(this MapTerrainDataCellEx ecell) {
    //  Vector3 pos = ecell.WorldPos();
    //  Ray ray = new Ray(new Vector3(pos.x, 1000f, pos.z), Vector3.down);
    //  int layerMask = 1 << LayerMask.NameToLayer("Water");
    //  RaycastHit[] hits = Physics.RaycastAll(ray, 2000f, layerMask, QueryTriggerInteraction.Collide);
    //  //Log.LogWrite(0, "UpdateWaterHeightRay:" + pos + "\n");
    //  //Log.LogWrite(1, "hits count:" + hits.Length + "\n");
    //  float waterLevel = float.NaN;
    //  foreach (RaycastHit hit in hits) {
    //    if (float.IsNaN(waterLevel) || (hit.point.y > waterLevel)) {
    //      //Log.LogWrite(2, "hit pos:" + hit.point + " " + hit.collider.gameObject.name + " layer:" + LayerMask.LayerToName(hit.collider.gameObject.layer) + "\n");
    //      waterLevel = hit.point.y;
    //    }
    //  }
    //  if (float.IsNaN(waterLevel) == false) {
    //    waterLevel -= Core.Settings.waterFlatDepth;
    //    if (Mathf.Abs(ecell.terrainHeight - waterLevel) > Core.Settings.waterFlatDepth) {
    //      ecell.realTerrainHeight = ecell.terrainHeight;
    //      ecell.terrainHeight = waterLevel;
    //      ecell.cachedHeight = waterLevel;
    //      //Log.LogWrite(1, "terrain height:" + ecell.realTerrainHeight + " water surface height:" + ecell.terrainHeight + "\n");
    //    }
    //    ecell.waterLevelCached = true;
    //    if (ecell.cachedSteepness > Core.Settings.maxWaterSteepness) {
    //      //Log.LogWrite(1, "steppiness too high faltering:" + ecell.cachedSteepness + "\n");
    //      if (ecell.cachedSteepness > Core.Settings.deepWaterSteepness) {
    //        //Log.LogWrite(1, "steppiness too high mark as deep water:" + ecell.cachedSteepness + "\n");
    //        ecell.AddTerrainMask(TerrainMaskFlags.DeepWater);
    //      }
    //      ecell.cachedSteepness = Core.Settings.maxWaterSteepness;
    //      ecell.terrainSteepness = Core.Settings.maxWaterSteepness;
    //    }
    //    if (Mathf.Abs(ecell.realTerrainHeight - waterLevel) > Core.Settings.deepWaterDepth) {
    //      //Log.LogWrite(1, "real depth too high tie to deep water:" + Mathf.Abs(ecell.realTerrainHeight - waterLevel) + "\n");
    //      ecell.AddTerrainMask(TerrainMaskFlags.DeepWater);
    //    }
    //  }
    //}
    //public static void UpdateWaterHeightParent(this MapTerrainDataCell cell, float waterLevel) {
    //  if (cell.HasWater() == false) { return; }
    //  MapTerrainDataCellEx ecell = cell as MapTerrainDataCellEx;
    //  if (ecell == null) { return; }
    //  if (ecell.waterLevelCached) { return; }
    //  if (float.IsNaN(waterLevel)) {
    //    ecell.terrainHeight = waterLevel;
    //    ecell.cachedHeight = waterLevel;
    //    ecell.waterLevelCached = true;
    //  }
    //}
    public static int MAX_LEVEL = 3;
    //public static void UpdateWaterHeight(this MapTerrainDataCell cell, int level = 0) {
    //  if (Core.Settings.fixWaterHeight == false) { return; }
    //  if (level > MAX_LEVEL) { return; }
    //  if (cell.HasWater() == false) { return; }
    //  MapTerrainDataCellEx ecell = cell as MapTerrainDataCellEx;
    //  if (ecell == null) { return; }
    //  if (ecell.waterLevelCached) { return; }
    //  ecell.UpdateWaterHeightRay();
    //  int x = ecell.x;
    //  int y = ecell.y;
    //  int mx = ecell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1;
    //  int my = ecell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1;
    //  if ((x > 0) && (y > 0)) { ecell.mapMetaData.mapTerrainDataCells[x - 1, y - 1].UpdateWaterHeight(level + 1); }
    //  if (x > 0) { ecell.mapMetaData.mapTerrainDataCells[x - 1, y].UpdateWaterHeight(level + 1); }
    //  if ((x < mx) && (y < my)) { ecell.mapMetaData.mapTerrainDataCells[x + 1, y + 1].UpdateWaterHeight(level + 1); }
    //  if (y < my) { ecell.mapMetaData.mapTerrainDataCells[x, y + 1].UpdateWaterHeight(level + 1); }
    //  if (x < mx) { ecell.mapMetaData.mapTerrainDataCells[x + 1, y].UpdateWaterHeight(level + 1); }
    //  if ((x < mx) && (y > 0)) { ecell.mapMetaData.mapTerrainDataCells[x + 1, y - 1].UpdateWaterHeight(level + 1); }
    //  if ((x > 0) && (y < my)) { ecell.mapMetaData.mapTerrainDataCells[x - 1, y + 1].UpdateWaterHeight(level + 1); }
    //}
  }

  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetTerrainCost")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MapTerrainDataCell), typeof(AbstractActor), typeof(MoveType) })]
  public static class PathNodeGrid_GetTerrainCost {
    private static FieldInfo FgtcScratchGrid = null;
    private static FieldInfo FgtcScratchMask = null;
    private static FieldInfo FgtcMovingVehicle = null;
    private static FieldInfo FgtcMovingMech = null;
    public static bool Prepare() {
      try {
        FgtcScratchGrid = typeof(PathNodeGrid).GetField("gtcScratchGrid", BindingFlags.Static | BindingFlags.NonPublic);
        if (FgtcScratchGrid == null) {
          Log.LogWrite(0, "Can't find gtcScratchGrid", true);
          return false;
        }
        FgtcScratchMask = typeof(PathNodeGrid).GetField("gtcScratchMask", BindingFlags.Static | BindingFlags.NonPublic);
        if (FgtcScratchGrid == null) {
          Log.LogWrite(0, "Can't find gtcScratchMask", true);
          return false;
        }
        FgtcMovingMech = typeof(PathNodeGrid).GetField("gtcMovingMech", BindingFlags.Static | BindingFlags.NonPublic);
        if (FgtcScratchGrid == null) {
          Log.LogWrite(0, "Can't find gtcMovingMech", true);
          return false;
        }
        FgtcMovingVehicle = typeof(PathNodeGrid).GetField("gtcMovingVehicle", BindingFlags.Static | BindingFlags.NonPublic);
        if (FgtcScratchGrid == null) {
          Log.LogWrite(0, "Can't find gtcMovingVehicle", true);
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(0, e.ToString(), true);
        return false;
      }
      return true;
    }
    public static float MoveCostModPerBiome(this AbstractActor unit) {
      UnitCustomInfo info = unit.GetCustomInfo();
      if (info == null) {
        //Log.LogWrite(" no custom info\n");
        return 1f;
      };
      if (unit.UnaffectedMoveCostBiome()) {
        return 1f;
      }
      string biome = DynamicMapHelper.CurrentBiome;
      //Log.LogWrite(" biome:"+biome+"\n");
      if (info.MoveCostModPerBiome.ContainsKey(biome) == false) {
        //Log.LogWrite(" no biome override\n");
        return 1f;
      };
      //Log.LogWrite(" override mod:"+ info.MoveCostModPerBiome[biome] + "\n");
      return info.MoveCostModPerBiome[biome];
    }
    public static DesignMaskMoveCostInfo MoveCostByTerrain(this AbstractActor unit, DesignMaskDef mask, float defValue) {
      if (mask == null) { return new DesignMaskMoveCostInfo(defValue, 1f); };
      CustomDesignMaskInfo maskInfo = mask.GetCustomDesignMaskInfo();
      string MoveCostKey = unit.CustomMoveCostKey();
      if (string.IsNullOrEmpty(MoveCostKey) == false) {
        //Log.WL(1, "MoveCostKey:" + MoveCostKey);
      };
      if ((maskInfo != null) && (string.IsNullOrEmpty(MoveCostKey) == false)) {
        if (maskInfo.CustomMoveCost.ContainsKey(MoveCostKey)) {
          DesignMaskMoveCostInfo result = maskInfo.CustomMoveCost[MoveCostKey];
          //Log.LogWrite(1, "result:" + result.moveCost + "," + result.SprintMultiplier);
          return result;
        } else {
          //Log.LogWrite(1, "move cost not found");
        }
      }
      Mech mech = unit as Mech;
      Vehicle vehicle = unit as Vehicle;
      float moveCost = defValue;
      if (mech != null) {
        bool FakeVehicle = false;
        UnitCustomInfo info = unit.GetCustomInfo();
        if (info != null) { if (info.FakeVehicle) { FakeVehicle = true; } }
        if (FakeVehicle == false) {
          switch (mech.weightClass) {
            case WeightClass.LIGHT:
            moveCost = mask.moveCostMechLight;
            break;
            case WeightClass.MEDIUM:
            moveCost = mask.moveCostMechMedium;
            break;
            case WeightClass.HEAVY:
            moveCost = mask.moveCostMechHeavy;
            break;
            case WeightClass.ASSAULT:
            moveCost = mask.moveCostMechAssault;
            break;
          }
        } else {
          switch (info.FakeVehicleMovementType) {
            case VehicleMovementType.Wheeled:
            switch (mech.weightClass) {
              case WeightClass.LIGHT:
              moveCost = mask.moveCostWheeledLight;
              break;
              case WeightClass.MEDIUM:
              moveCost = mask.moveCostWheeledMedium;
              break;
              case WeightClass.HEAVY:
              moveCost = mask.moveCostWheeledHeavy;
              break;
              case WeightClass.ASSAULT:
              moveCost = mask.moveCostWheeledAssault;
              break;
            };
            break;
            case VehicleMovementType.Tracked:
            switch (mech.weightClass) {
              case WeightClass.LIGHT:
              moveCost = mask.moveCostTrackedLight;
              break;
              case WeightClass.MEDIUM:
              moveCost = mask.moveCostTrackedMedium;
              break;
              case WeightClass.HEAVY:
              moveCost = mask.moveCostTrackedHeavy;
              break;
              case WeightClass.ASSAULT:
              moveCost = mask.moveCostTrackedAssault;
              break;
            };
            break;
          }
        }
      } else
      if (vehicle != null) {
        switch (vehicle.movementType) {
          case VehicleMovementType.Wheeled:
            switch (vehicle.weightClass) {
              case WeightClass.LIGHT:
                moveCost = mask.moveCostWheeledLight;
                break;
              case WeightClass.MEDIUM:
                moveCost = mask.moveCostWheeledMedium;
                break;
              case WeightClass.HEAVY:
                moveCost = mask.moveCostWheeledHeavy;
                break;
              case WeightClass.ASSAULT:
                moveCost = mask.moveCostWheeledAssault;
                break;
            };
            break;
          case VehicleMovementType.Tracked:
            switch (vehicle.weightClass) {
              case WeightClass.LIGHT:
                moveCost = mask.moveCostTrackedLight;
                break;
              case WeightClass.MEDIUM:
                moveCost = mask.moveCostTrackedMedium;
                break;
              case WeightClass.HEAVY:
                moveCost = mask.moveCostTrackedHeavy;
                break;
              case WeightClass.ASSAULT:
                moveCost = mask.moveCostTrackedAssault;
                break;
            };
            break;
        }
      }
      return new DesignMaskMoveCostInfo(moveCost, mask.moveCostSprintMultiplier);
    }
    public static float GetTerrainCost(MapTerrainDataCell cell, AbstractActor unit, MoveType moveType) {
      //PathNodeGrid.gtcScratchGrid = unit.Pathing.getGrid(moveType);
      PathNodeGrid gtcScratchGrid = unit.Pathing.getGrid(moveType);
      float num = gtcScratchGrid.Capabilities.MoveCostNormal;
      FgtcScratchGrid.SetValue(null, gtcScratchGrid);
      if (cell == null) { return num * unit.MoveCostModPerBiome(); };
      if (unit.NavalUnit()) {
        if (cell.HasWater() == false) { return 99999.9f; }
      }
      bool UnaffectedPathing = unit.UnaffectedPathing();
      if (UnaffectedPathing) { return num * unit.MoveCostModPerBiome(); }
      if (SplatMapInfo.IsImpassable(cell.terrainMask) && (UnaffectedPathing == false)) { return 99999.9f; };
      if (SplatMapInfo.IsMapBoundary(cell.terrainMask)) { return 99999.9f; };
      DesignMaskDef gtcScratchMask = unit.Combat.MapMetaData.GetPriorityDesignMask(cell); FgtcScratchMask.SetValue(null, gtcScratchMask); //PathNodeGrid.gtcScratchMask = unit.Combat.MapMetaData.GetPriorityDesignMask(cell);
      if (gtcScratchMask == null) { return num * unit.MoveCostModPerBiome(); };
      Mech gtcMovingMech = unit as Mech; FgtcMovingMech.SetValue(null, gtcMovingMech); //PathNodeGrid.gtcMovingMech = unit as Mech;
      Vehicle gtcMovingVehicle = unit as Vehicle; FgtcMovingVehicle.SetValue(null, gtcMovingVehicle); //PathNodeGrid.gtcMovingVehicle = unit as Vehicle;
      DesignMaskMoveCostInfo designMaskMoveCostInfo = unit.MoveCostByTerrain(gtcScratchMask, num);
      if (designMaskMoveCostInfo == null) { return num * unit.MoveCostModPerBiome(); }
      if (moveType == MoveType.Sprinting) {
        return designMaskMoveCostInfo.moveCost * designMaskMoveCostInfo.SprintMultiplier * unit.MoveCostModPerBiome();
      } else {
        return designMaskMoveCostInfo.moveCost * unit.MoveCostModPerBiome();
      }
    }
    public static bool Prefix(MapTerrainDataCell cell, AbstractActor unit, MoveType moveType, ref float __result) {
      try {
        __result = GetTerrainCost(cell, unit, moveType);
        //tbone movecost modifier for minefields
        if (moveType != MoveType.Jumping && !unit.UnaffectedLandmines())
        { 
          if (cell is MapTerrainDataCellEx cellEx) {
            if (cellEx.hexCell.MineFields.Count > 0) {
              float modifier = 1f;
              foreach (MineField mineField in cellEx.hexCell.MineFields) {
                if (mineField.count > 0) modifier += (mineField.Def.MoveCostFactor * mineField.count);
              } 
              //CustomAmmoCategoriesLog.Log.F.TWL(0,$"{unit.DisplayName} movecost {__result} to be multiplied by {modifier} due to mines");//spammy log, not needed
              __result *= modifier;
            }
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetPathNode")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(PathNode), typeof(List<AbstractActor>) })]
  public static class PathNodeGrid_GetPathNode {
    public static MethodInfo mIsOutOfGrid = null;
    private delegate bool IsOutOfGridDelegate(PathNodeGrid node, int x, int z);
    private static IsOutOfGridDelegate IsOutOfGridInkover = null;
    public static bool Prepare() {
      mIsOutOfGrid = typeof(PathNodeGrid).GetMethod("IsOutOfGrid", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[2] { typeof(int), typeof(int) }, null);
      if (mIsOutOfGrid == null) {
        Log.LogWrite("Can't find PathNodeGrid.IsOutOfGrid\n");
        return false;
      }
      var dm = new DynamicMethod("CUIsOutOfGrid", typeof(bool), new Type[] { typeof(PathNodeGrid),typeof(int),typeof(int) }, typeof(PathNodeGrid));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Ldarg_2);
      gen.Emit(OpCodes.Call, mIsOutOfGrid);
      gen.Emit(OpCodes.Ret);
      IsOutOfGridInkover = (IsOutOfGridDelegate)dm.CreateDelegate(typeof(IsOutOfGridDelegate));
      return true;
    }
    public static bool IsOutOfGrid(this PathNodeGrid node, int x, int z) {
      if (IsOutOfGridInkover == null) { return false; }
      return IsOutOfGridInkover(node,x,z);
    }
    public static bool Prefix(PathNodeGrid __instance, int x, int z, int angle, PathNode from, List<AbstractActor> collisionTestActors, ref PathNode __result, ref PathNode[,] ___pathNodes, ref MapTerrainDataCell ___gpnCell, ref CombatGameState ___combat, ref AbstractActor ___owningActor) {
      try {
        if (__instance.IsOutOfGrid(x, z)) {
          __result = null;
          return false;
        }
        if (___pathNodes[x, z] == null) {
          Vector3 posFromIndex = __instance.GetPosFromIndex(x, z);
          ___gpnCell = ___combat.MapMetaData.GetCellAt(posFromIndex);
          if (___gpnCell == null) {
            __result = (PathNode)null;
            return false;
          }
          //___gpnCell.UpdateWaterHeight();
          posFromIndex.y = ___gpnCell.cachedHeight;
          ___pathNodes[x, z] = new PathNode(from, x, z, posFromIndex, angle, ___gpnCell, collisionTestActors, ___owningActor);
        }
        __result = ___pathNodes[x, z];
        return false;
      } catch (Exception e) {
        Log.TWL(0,e.ToString());
        return true;
      }
    }
  }
  //[HarmonyPatch(typeof(PathNode))]
  //[HarmonyPatch(MethodType.Constructor)]
  //[HarmonyPatch(new Type[] { typeof(PathNode), typeof(int), typeof(int), typeof(Vector3), typeof(int), typeof(MapTerrainDataCell), typeof(List<AbstractActor>), typeof(AbstractActor) })]
  //public static class PathNode_Constructor {
  //  private static MethodInfo FPosition = null;
  //  private delegate void PositionSetDelegate(PathNode node, Vector3 pos);
  //  private static PositionSetDelegate PositionSetInkover = null;
  //  public static bool Prepare() {
  //    FPosition = typeof(PathNode).GetProperty("Position", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
  //    if (FPosition == null) {
  //      Log.LogWrite("Can't find PathNode.Position");
  //      return false;
  //    }
  //    var dm = new DynamicMethod("CUPositionSet", null, new Type[] { typeof(PathNode), typeof(Vector3) }, typeof(PathNode));
  //    var gen = dm.GetILGenerator();
  //    gen.Emit(OpCodes.Ldarg_0);
  //    gen.Emit(OpCodes.Ldarg_1);
  //    gen.Emit(OpCodes.Call, FPosition);
  //    gen.Emit(OpCodes.Ret);
  //    PositionSetInkover = (PositionSetDelegate)dm.CreateDelegate(typeof(PositionSetDelegate));
  //    return true;
  //  }
  //  public static void Position(this PathNode node, Vector3 pos) {
  //    if (PositionSetInkover == null) { return; }
  //    PositionSetInkover(node, pos);
  //  }
  //  /*public static void UpdateWaterHeight(this MapTerrainDataCellEx ecell) {
  //    if (float.IsNaN(ecell.WaterHeight)) {
  //      Vector3 pos = ecell.WorldPos();
  //      Ray ray = new Ray(new Vector3(pos.x, 1000f, pos.z), Vector3.down);
  //      int layerMask = 1 << LayerMask.NameToLayer("Water");
  //      RaycastHit[] hits = Physics.RaycastAll(ray, 2000f, layerMask, QueryTriggerInteraction.Collide);
  //      Log.LogWrite(2, "hits count:" + hits.Length + "\n");
  //      foreach (RaycastHit hit in hits) {
  //        Log.LogWrite(3, "hit pos:" + hit.point + " " + hit.collider.gameObject.name + " layer:" + LayerMask.LayerToName(hit.collider.gameObject.layer) + "\n");
  //        ecell.WaterHeight = hit.point.y;
  //        return;
  //      }
  //      ecell.WaterHeight = ecell.cachedHeight;
  //    }
  //  }*/
  //  public static void Postfix(PathNode __instance, PathNode from, int x, int z, Vector3 pos, int angle, MapTerrainDataCell cell, List<AbstractActor> collisionTestActors, AbstractActor owningActor) {
  //    //try {
  //    //  MapTerrainDataCellEx ecell = cell as MapTerrainDataCellEx;
  //    //  if (ecell == null) { return; }
  //    //  if ((ecell.cachedHeight - ecell.realTerrainHeight) > 5f) {
  //    //    //Log.LogWrite(1, "detected lifted up cell. " + pos + " cell height:" + cell.cachedHeight + "/" + ecell.realTerrainHeight + "\n");
  //    //  }
  //    //  if (__instance.HasCollision || (__instance.IsValidDestination == false)) {
  //    //    //Log.LogWrite(1, "X:" + ecell.x + " Y:" + ecell.y, true);
  //    //    //Log.LogWrite(1, "HasCollision:" + __instance.HasCollision, true);
  //    //    //Log.LogWrite(1, "IsValidDestination:" + __instance.IsValidDestination, true);
  //    //    //Log.LogWrite(1, "Priority Terrain Flag:" + MapMetaData.GetPriorityTerrainMaskFlags(ecell) + "(" + ((int)ecell.terrainMask) + ")", true);
  //    //    if (owningActor.UnaffectedPathingF()) {
  //    //      if (__instance.HasCollision) {
  //    //      } else {
  //    //        if (SplatMapInfo.IsMapBoundary(cell.terrainMask) == false) {
  //    //          //Log.LogWrite(1, "Unaffected by pathing", true);
  //    //          __instance.IsValidDestination = true;
  //    //        }
  //    //      }
  //    //    }
  //    //  }
  //    //}catch(Exception e) {
  //    //  Log.TWL(0, e.ToString(), true);
  //    //}
  //  }
  //}
  //[HarmonyPatch(typeof(PathNodeGrid))]
  //[HarmonyPatch("GetPathTo")]
  //[HarmonyPatch(MethodType.Normal)]
  ////[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3 lookTarget, typeof(float maxCost, typeof(AbstractActor meleeTarget, out float costLeft, out Vector3 resultPos, out float resultAngle, bool preservePosition, float lockedAngle, float lockedCostLeft, float mouseInfluence, bool calledFromUI, bool overrideAngle) })]
  //public static class PathNodeGrid_GetPathTo {
  //  private static FieldInfo FowningActor = null;
  //  public static bool Prepare() {
  //    try {
  //      FowningActor = typeof(PathNodeGrid).GetField("owningActor", BindingFlags.Instance | BindingFlags.NonPublic);
  //      if (FowningActor == null) {
  //        Log.LogWrite(0, "PathNodeGrid.GetPathTo Can't find owningActor", true);
  //        return false;
  //      }
  //    } catch (Exception e) {
  //      Log.LogWrite(0, e.ToString(), true);
  //      return false;
  //    }
  //    return true;
  //  }
  //  public static void Postfix(PathNodeGrid __instance, Vector3 pos, Vector3 lookTarget, float maxCost, AbstractActor meleeTarget, ref float costLeft, ref Vector3 resultPos, ref float resultAngle, bool preservePosition, float lockedAngle, float lockedCostLeft, float mouseInfluence, bool calledFromUI, bool overrideAngle, ref List<PathNode> __result) {
  //    AbstractActor owningActor = (AbstractActor)FowningActor.GetValue(__instance);
  //    try {
  //      if (__result == null) { return; }
  //      //Log.LogWrite("PathNodeGrid.GetPathTo " + pos + " result:" + __result.Count + "\n");
  //      for (int index = 0; index < __result.Count; ++index) {
  //        //bool water = false;
  //        PathNode node = __result[index];
  //        //MapTerrainDataCellEx ecell = node.MapTerrainDataCell as MapTerrainDataCellEx;
  //        //if (ecell == null) { continue; }
  //        if (node.MapTerrainDataCell == null) { return; }
  //        node.MapTerrainDataCell.UpdateWaterHeight();
  //        //Log.LogWrite(1, node.Position + " cell height:" + node.MapTerrainDataCell.cachedHeight + "/" + (ecell == null ? "null" : ecell.realTerrainHeight.ToString()) + " water:" + water, true);
  //      }
  //    }catch(Exception e) {
  //      Log.TWL(0,e.ToString(),true);
  //    }
  //  }
  //}
  /*[HarmonyPatch(typeof(JumpPathing))]
  [HarmonyPatch("IsValidLandingSpot")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3 lookTarget, typeof(float maxCost, typeof(AbstractActor meleeTarget, out float costLeft, out Vector3 resultPos, out float resultAngle, bool preservePosition, float lockedAngle, float lockedCostLeft, float mouseInfluence, bool calledFromUI, bool overrideAngle) })]
  public static class JumpPathing_IsValidLandingSpot {
    private static PropertyInfo pMech = null;
    public static bool Prepare() {
      try {
        pMech = typeof(JumpPathing).GetProperty("Mech", BindingFlags.Instance | BindingFlags.NonPublic);
        if (pMech == null) {
          Log.LogWrite(0, "Can't find JumpPathing.Mech", true);
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(0, e.ToString(), true);
        return false;
      }
      return true;
    }
    public static Mech Mech(this JumpPathing pathing) {
      return (Mech)pMech.GetValue(pathing, null);
    }
    public static void Postfix(JumpPathing __instance, Vector3 worldPos, List<AbstractActor> allActors, ref bool __result) {
      try {
        if (__result == false) { return; };
        MapTerrainDataCellEx ecell = __instance.Mech().Combat.MapMetaData.GetCellAt(worldPos) as MapTerrainDataCellEx;
        if (ecell == null) { return; };
        int sx = ecell.x > 0 ? ecell.x - 1 : 0;
        int ex = ecell.x < (ecell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1) ? ecell.x + 1 : ecell.mapMetaData.mapTerrainDataCells.GetLength(0) - 1;
        int sy = ecell.y > 0 ? ecell.y - 1 : 0;
        int ey = ecell.y < (ecell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1) ? ecell.y + 1 : ecell.mapMetaData.mapTerrainDataCells.GetLength(1) - 1;
        for(int x = sx; x <= ex; ++x) {
          for (int y = sy; y <= ey; ++y) {
            MapTerrainDataCellEx tcell = ecell.mapMetaData.mapTerrainDataCells[x, y] as MapTerrainDataCellEx;
            if (tcell == null) { continue; }
            TerrainMaskFlags flag = MapMetaData.GetPriorityTerrainMaskFlags(tcell.terrainMask);
            if (SplatMapInfo.IsDeepWater(flag)) {
              __result = false;
              return;
            }
          }
        }
        } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
    }
  }*/
  //[HarmonyPatch(typeof(MapMetaData))]
  //[HarmonyPatch("GetLerpedHeightAt")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(bool) })]
  //public static class MapMetaData_GetLerpedHeightAt {
  //  public static bool Prefix(MapMetaData __instance, Vector3 worldPos, bool terrainOnly) {
  //    try {
  //      MapTerrainDataCell cell = __instance.GetCellAt(worldPos);
  //      cell.UpdateWaterHeight();
  //    } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
  //    return true;
  //  }
  //}
  //[HarmonyPatch(typeof(MapMetaData))]
  //[HarmonyPatch("GetCellAt")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3) })]
  //public static class MapMetaData_GetCellAtVector {
  //  public static void Postfix(MapMetaData __instance, ref MapTerrainDataCell __result) {
  //    try {
  //      __result.UpdateWaterHeight();
  //    } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
  //    return;
  //  }
  //}
  //[HarmonyPatch(typeof(MapMetaData))]
  //[HarmonyPatch("GetCellAt")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Point) })]
  //public static class MapMetaData_GetCellAtPoint {
  //  public static void Postfix(MapMetaData __instance, ref MapTerrainDataCell __result) {
  //    try {
  //      __result.UpdateWaterHeight();
  //    } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
  //    return;
  //  }
  //}
  //[HarmonyPatch(typeof(MapMetaData))]
  //[HarmonyPatch("GetCellAt")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int),typeof(int) })]
  //public static class MapMetaData_GetCellAtIndex {
  //  public static void Postfix(MapMetaData __instance, ref MapTerrainDataCell __result) {
  //    try {
  //      __result.UpdateWaterHeight();
  //    } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
  //    return;
  //  }
  //}
  //[HarmonyPatch(typeof(MapMetaData))]
  //[HarmonyPatch("SafeGetCellAt")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
  //public static class MapMetaData_SafeGetCellAt {
  //  public static void Postfix(MapMetaData __instance, ref MapTerrainDataCell __result) {
  //    try {
  //      __result.UpdateWaterHeight();
  //    } catch (Exception e) { Log.LogWrite(e.ToString() + "\n", true); }
  //    return;
  //  }
  //}
//  [HarmonyPatch(typeof(ActorMovementSequence))]
//  [HarmonyPatch("UpdateSpline")]
//  [HarmonyPatch(MethodType.Normal)]
//  [HarmonyPatch(new Type[] { })]
//  public static class ActorMovementSequence_UpdateSpline {
//    //private static FieldInfo FowningActor = null;
//    public static bool Prepare() {
//      try {
//        //FowningActor = typeof(OrderSequence).GetField("owningActor", BindingFlags.Instance | BindingFlags.NonPublic);
//        //if (FowningActor == null) {
//          //Log.LogWrite(0, "ActorMovementSequence.UpdateSpline Can't find owningActor", true);
//          //return false;
//        //}
//      } catch (Exception e) {
//        Log.LogWrite(0, e.ToString(), true);
//        return false;
//      }
//      return true;
//    }
//#pragma warning disable CS0252
//    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
//      Log.LogWrite("ActorMovementSequence.UpdateSpline transpliter\n", true);
//      List<CodeInstruction> result = instructions.ToList();
//      MethodInfo GetSelfTerrainModifier = AccessTools.Method(typeof(ToHit), "GetSelfTerrainModifier");
//      if (GetSelfTerrainModifier != null) {
//        Log.LogWrite(1, "source method found", true);
//      } else {
//        return result;
//      }
//      MethodInfo replacementMethod = AccessTools.Method(typeof(ActorMovementSequence_UpdateSpline), nameof(GetLerpedHeightAt));
//      if (replacementMethod != null) {
//        Log.LogWrite(1, "target method found", true);
//      } else {
//        return result;
//      }
//      int methodCallIndex = result.FindIndex(instruction => instruction.opcode == OpCodes.Callvirt && instruction.operand == GetSelfTerrainModifier);
//      return result;
//    }
//    public static float GetLerpedHeightAt(Vector3 worldPos, AbstractActor owningActor) {
//      return owningActor.Combat.MapMetaData.GetLerpedHeightAt(worldPos, false);
//    }
//  }
//#pragma warning restore CS0252
}