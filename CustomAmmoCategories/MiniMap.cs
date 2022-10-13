using BattleTech;
using BattleTech.Framework;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(MainMenu))]
  [HarmonyPatch("OnAddedToHierarchy")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MainMenu_OnAddedToHierarchy {
    public static T FindObject<T>(this GameObject go, string name, bool toplevel) where T : Component {
      T[] arr = go.GetComponentsInChildren<T>(true);
      foreach (T component in arr) {
        if (toplevel) { if (component.gameObject.transform.parent != go.transform) { continue; } }
        if (component.gameObject.transform.name == name) { return component; }
      }
      return null;
    }
    public static void Postfix(MainMenu __instance) {
      try {
        HBSDOTweenButton closeButton = __instance.gameObject.FindObject<HBSDOTweenButton>("uixPrfBttn_BASE_cancelButton", false);
        if (closeButton != null) {
          Log.M?.TWL(0, "MainMenu.OnAddedToHierarchy minimap close button found");
          GameObject closeButton_go = GameObject.Instantiate(closeButton.gameObject);
          closeButton = closeButton_go.GetComponent<HBSDOTweenButton>();
          HBSTooltip tooltip = closeButton_go.GetComponent<HBSTooltip>();
          HBSTooltipStateData defaultStateData = new HBSTooltipStateData();
          defaultStateData.SetString("Close minimap");
          if (tooltip != null) { tooltip.SetDefaultStateData(defaultStateData); }
          closeButton.OnClicked = new UnityEngine.Events.UnityEvent();
          GameObject.DontDestroyOnLoad(closeButton_go);
          closeButton_go.name = "uixPrfBttn_BASE_minimapClosebutton_source";
          closeButton_go.SetActive(false);
          CombatHUDMiniMap.closeButtonSource = closeButton_go;
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_Init {
    public static void Postfix(CombatHUDMechTray __instance, MessageCenter messageCenter, CombatHUD HUD) {
      try {
        //if (CustomAmmoCategories.Settings.EnableMinimap) {
          CombatHUDMiniMap.Create(HUD);
        //}
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDMechTray))]
  [HarmonyPatch("refreshMechInfo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDMechTray_refreshMechInfo {
    public static void Postfix(CombatHUDMechTray __instance, AbstractActor ___displayedActor) {
      try {
        if (CustomAmmoCategories.Settings.EnableMinimap && (CombatHUDMiniMap.instance != null) && (___displayedActor != null)) {
          CombatHUDMiniMap.instance.MapJammedState = CombatHUDMiniMap.isMinimapJammed(___displayedActor);
          CombatHUDMiniMap.instance.UnitsJammedState = CombatHUDMiniMap.isMinimapUnitsJammed(___displayedActor);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(EncounterLayerData))]
  [HarmonyPatch("GetEncounterBoundaryTexture")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class EncounterLayerData_GetEncounterBoundaryTexture {
    public static void Postfix(EncounterLayerData __instance, ref Texture2D __result) {
      CombatHUDMiniMap.InitMinimap(__instance.Combat);
    }
  }
  public class CombatHUDMiniMapEnabler: EventTrigger {
    public CombatHUDMiniMap minimap { get; set; } = null;
    public override void OnPointerEnter(PointerEventData data) {
    }
    public override void OnPointerExit(PointerEventData data) {
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.M?.TWL(0, "CombatHUDMiniMapEnabler.OnPointerClick called." + data.position + " clickCount:" + data.clickCount,true);
      if (data.button == PointerEventData.InputButton.Left) {
        if (minimap != null) {
          this.minimap.gameObject.SetActive(true);
          this.minimap.MechTray_CompanyLogo?.SetActive(false);
          this.minimap.MechTray_GreebleWedge?.SetActive(false);
          this.minimap.transform.SetAsLastSibling();
          CustomAmmoCategories.Settings.EnableMinimap = true;
        }
      }
    }
  }
  public class CombatHUDMiniMap : EventTrigger {
    public static readonly string MINIMAP_JAMMED = "MINIMAP_JAMMED";
    public static readonly string MINIMAP_UNITS_JAMMED = "MINIMAP_UNITS_JAMMED";
    public static GameObject closeButtonSource { get; set; } = null;
    public static void InitMinimapStatistic(AbstractActor unit) {
      unit.StatCollection.AddStatistic<float>(MINIMAP_JAMMED, 0.0f);
      unit.StatCollection.AddStatistic<float>(MINIMAP_UNITS_JAMMED, 0.0f);
    }
    public static bool isMinimapJammed(AbstractActor unit) {
      return unit.StatCollection.GetOrCreateStatisic<float>(MINIMAP_JAMMED, 0f).Value<float>() > 0f;
    }
    public static bool isMinimapUnitsJammed(AbstractActor unit) {
      return unit.StatCollection.GetOrCreateStatisic<float>(MINIMAP_UNITS_JAMMED, 0f).Value<float>() > 0f;
    }
    public static void InitMinimap(CombatGameState combat) {
      try {
        Log.M?.TWL(0, $"InitMinimap rects:{combat.EncounterLayerData.encounterBoundaryChunk.encounterBoundaryRectList.Count}");
        foreach (var r in combat.EncounterLayerData.encounterBoundaryChunk.encounterBoundaryRectList) {
          Log.M?.WL(1, r.rect.ToString());
        }
        if (combat.EncounterLayerData.encounterBoundaryChunk.encounterBoundaryRectList.Count != 1) {
          CombatHUDMiniMap.minimapContent = null;
          return;
        }
        CombatHUDMiniMap.initColors();
        RectHolder rect = combat.EncounterLayerData.encounterBoundaryChunk.encounterBoundaryRectList[0];
        CombatHUDMiniMap.minimapXstart = -1024.0f - MapMetaDataExporter.cellSize / 2f;//rect.rect.x;
        CombatHUDMiniMap.minimapZstart = -1024.0f - MapMetaDataExporter.cellSize / 2f;//rect.rect.y;
        //if (CombatHUDMiniMap.minimapXstart < rect.rect.x) { CombatHUDMiniMap.minimapXstart = Mathf.Round(rect.rect.x / 2f) * 2f; }
        //if (CombatHUDMiniMap.minimapZstart < rect.rect.y) { CombatHUDMiniMap.minimapZstart = Mathf.Round(rect.rect.y / 2f) * 2f; }
        //if ((CombatHUDMiniMap.minimapXstart + 1024.0f) < SplatMapInfo.mapBoundaryWidth) { CombatHUDMiniMap.minimapXstart = SplatMapInfo.mapBoundaryWidth - 1024.0f; }
        //if ((CombatHUDMiniMap.minimapZstart + 1024.0f) < SplatMapInfo.mapBoundaryWidth) { CombatHUDMiniMap.minimapZstart = SplatMapInfo.mapBoundaryWidth - 1024.0f; }

        //if (CombatHUDMiniMap.minimapXend > (rect.rect.x + rect.rect.width)) { CombatHUDMiniMap.minimapXend = Mathf.Round(((rect.rect.x + rect.rect.width)) / 2f) * 2f; };
        //if (CombatHUDMiniMap.minimapZend > (rect.rect.y + rect.rect.height)) { CombatHUDMiniMap.minimapZend = Mathf.Round(((rect.rect.y + rect.rect.height)) / 2f) * 2f; };

        CombatHUDMiniMap.cellXsize = 2048.0f / combat.MapMetaData.mapTerrainDataCells.GetLength(0);
        CombatHUDMiniMap.cellZsize = 2048.0f / combat.MapMetaData.mapTerrainDataCells.GetLength(1);

        CombatHUDMiniMap.minimapXend = 1024.0f - MapMetaDataExporter.cellSize / 2f - CombatHUDMiniMap.cellXsize;//rect.rect.x + rect.rect.width;
        CombatHUDMiniMap.minimapZend = 1024.0f - MapMetaDataExporter.cellSize / 2f - CombatHUDMiniMap.cellZsize;//rect.rect.y + rect.rect.height;

        //MapTerrainDataCellEx startCell = __instance.Combat.MapMetaData.GetCellAt(new Vector3(CombatHUDMiniMap.minimapXstart, 0f, CombatHUDMiniMap.minimapZstart)) as MapTerrainDataCellEx;
        //MapTerrainDataCellEx endCell = __instance.Combat.MapMetaData.GetCellAt(new Vector3(CombatHUDMiniMap.minimapXend, 0f, CombatHUDMiniMap.minimapZend)) as MapTerrainDataCellEx;
        //MapTerrainDataCellEx startCell = __instance.Combat.MapMetaData.mapTerrainDataCells[0, 0] as MapTerrainDataCellEx;
        //MapTerrainDataCellEx endCell = __instance.Combat.MapMetaData.mapTerrainDataCells[__instance.Combat.MapMetaData.mapTerrainDataCells.GetLength(0)-1, __instance.Combat.MapMetaData.mapTerrainDataCells.GetLength(1)-1] as MapTerrainDataCellEx;
        Point startCell = combat.MapMetaData.GetIndex(new Vector3(CombatHUDMiniMap.minimapXstart, 0f, CombatHUDMiniMap.minimapZstart));
        Point endCell = combat.MapMetaData.GetIndex(new Vector3(CombatHUDMiniMap.minimapXend, 0f, CombatHUDMiniMap.minimapZend));

        int minimapXsize = endCell.X - startCell.X + 1;
        int minimapYsize = endCell.Z - startCell.Z + 1;

        CombatHUDMiniMap.minimapXstart = combat.MapMetaData.getWorldPos(startCell).x;
        CombatHUDMiniMap.minimapZstart = combat.MapMetaData.getWorldPos(startCell).z;

        CombatHUDMiniMap.minimapXend = combat.MapMetaData.getWorldPos(endCell).x;
        CombatHUDMiniMap.minimapZend = combat.MapMetaData.getWorldPos(endCell).z;
        CombatHUDMiniMap.minimapXsize = CombatHUDMiniMap.minimapXend - CombatHUDMiniMap.minimapXstart;
        CombatHUDMiniMap.minimapZsize = CombatHUDMiniMap.minimapZend - CombatHUDMiniMap.minimapZstart;

        Log.M?.WL(1, $"minimap world start:{CombatHUDMiniMap.minimapXstart},{CombatHUDMiniMap.minimapZstart} end:{CombatHUDMiniMap.minimapXend},{CombatHUDMiniMap.minimapZend} size:{CombatHUDMiniMap.minimapXsize},{CombatHUDMiniMap.minimapZsize}");
        Log.M?.WL(1, $"minimap cells start:{startCell.X},{startCell.Z} end:{endCell.X},{endCell.Z} size:{minimapXsize},{minimapYsize}");

        CombatHUDMiniMap.minimapContent = new Texture2D(minimapYsize, minimapXsize, TextureFormat.ARGB32, false);
        CombatHUDMiniMap.minimapJammedContent = new Texture2D(minimapYsize, minimapXsize, TextureFormat.ARGB32, false);
        Texture2D heightMap = new Texture2D(minimapYsize, minimapXsize, TextureFormat.ARGB32, false);
        CombatHUDMiniMap.minimapCameraTexture = new Texture2D(65, 65, TextureFormat.ARGB32, false);
        for (int x = 0; x < 65; ++x) {
          for (int y = 0; y < 65; ++y) {
            CombatHUDMiniMap.minimapCameraTexture.SetPixel(x, y, Color.clear);
          }
        }
        for (int x = 0; x < 33; ++x) {
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 + x, x, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 - x, x, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 + x, x + 1, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 - x, x + 1, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 + x, x + 2, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 - x, x + 2, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 + x, x + 3, Color.white);
          CombatHUDMiniMap.minimapCameraTexture.SetPixel(32 - x, x + 3, Color.white);
        }
        int size = 5;
        for (int x = (32 - size); x < (32 + size); ++x) {
          for (int y = 0; y < size * 2; ++y) {
            CombatHUDMiniMap.minimapCameraTexture.SetPixel(x, y, Color.yellow);
          }
        }
        CombatHUDMiniMap.minimapCameraTexture.Apply();

        Color[] pixels = CombatHUDMiniMap.minimapContent.GetPixels();
        for (int index = 0; index < pixels.Length; ++index) { pixels[index] = Color.gray; }
        CombatHUDMiniMap.minimapContent.SetPixels(pixels);
        Vector3 mapPos_0_0_border = new Vector3(rect.rect.x, 0f, rect.rect.y);
        Point cell_0x0 = combat.MapMetaData.GetIndex(new Vector3(rect.rect.x, 0f, rect.rect.y));
        if (cell_0x0.X < 0) { cell_0x0.X = 0; }
        if (cell_0x0.X >= combat.MapMetaData.mapTerrainDataCells.GetLength(0)) { cell_0x0.X = combat.MapMetaData.mapTerrainDataCells.GetLength(0) - 1; }
        if (cell_0x0.Z < 0) { cell_0x0.Z = 0; }
        if (cell_0x0.Z >= combat.MapMetaData.mapTerrainDataCells.GetLength(1)) { cell_0x0.Z = combat.MapMetaData.mapTerrainDataCells.GetLength(1) - 1; }
        Vector3 mapPos_0_0_cell = combat.MapMetaData.getWorldPos(cell_0x0);
        Log.M?.WL(1, $"border:{mapPos_0_0_border} cell:{mapPos_0_0_cell} meta:{cell_0x0.X},{cell_0x0.Z}");
        mapPos_0_0_cell.y = combat.MapMetaData.GetLerpedHeightAt(mapPos_0_0_cell) + 10f;
        GameObject marker_0x0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker_0x0.SetActive(false);
        marker_0x0.name = "marker_0x0";
        marker_0x0.transform.position = mapPos_0_0_cell;
        marker_0x0.transform.localScale = Vector3.one * 10f;

        Vector3 mapPos_1_1_border = new Vector3(rect.rect.x + rect.rect.width, 0f, rect.rect.y + rect.rect.height);
        Point cell_1x1 = combat.MapMetaData.GetIndex(new Vector3(rect.rect.x + rect.rect.width, 0f, rect.rect.y + rect.rect.height));
        if (cell_1x1.X < 0) { cell_1x1.X = 0; }
        if (cell_1x1.X >= combat.MapMetaData.mapTerrainDataCells.GetLength(0)) { cell_1x1.X = combat.MapMetaData.mapTerrainDataCells.GetLength(0) - 1; }
        if (cell_1x1.Z < 0) { cell_1x1.Z = 0; }
        if (cell_1x1.Z >= combat.MapMetaData.mapTerrainDataCells.GetLength(1)) { cell_1x1.Z = combat.MapMetaData.mapTerrainDataCells.GetLength(1) - 1; }
        Vector3 mapPos_1_1_cell = combat.MapMetaData.getWorldPos(cell_1x1);
        Log.M?.WL(1, $"border:{mapPos_1_1_border} cell:{mapPos_1_1_cell} meta:{cell_1x1.X},{cell_1x1.Z}");
        mapPos_1_1_cell.y = combat.MapMetaData.GetLerpedHeightAt(mapPos_1_1_cell) + 10f;
        GameObject marker_1x1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker_1x1.name = "marker_1x1";
        marker_1x1.SetActive(false);
        marker_1x1.transform.position = mapPos_1_1_cell;
        marker_1x1.transform.localScale = Vector3.one * 20f;
        float maxHeight = combat.MapMetaData.mapTerrainDataCells[0, 0].terrainHeight;
        float minHeight = combat.MapMetaData.mapTerrainDataCells[0, 0].terrainHeight;
        for (int x = 0; x < minimapXsize; ++x) {
          for (int y = 0; y < minimapYsize; ++y) {
            float jummedColor = UnityEngine.Random.Range(0.1f, 0.7f);
            minimapJammedContent.SetPixel(y, x, new Color(jummedColor, jummedColor, jummedColor, 1f));
            MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[x + startCell.X, y + startCell.Z] as MapTerrainDataCellEx;
            if (cell.terrainHeight > maxHeight) { maxHeight = cell.terrainHeight; }
            if (cell.terrainHeight < minHeight) { minHeight = cell.terrainHeight; }
            if (cell == null) { CombatHUDMiniMap.minimapContent.SetPixel(y, x, Color.black); continue; }
            if (CombatHUDMiniMap.terrainColors.TryGetValue(MapMetaData.GetPriorityTerrainMaskFlags(cell.terrainMask), out var color)) {
              CombatHUDMiniMap.minimapContent.SetPixel(y, x, color);
            } else {
              CombatHUDMiniMap.minimapContent.SetPixel(y, x, Color.magenta);
            }
          }
        }
        if (CustomAmmoCategories.Settings.MinimapShowRegions) {
          RegionGameLogic[] regions = combat.EncounterLayerData.gameObject.GetComponentsInChildren<RegionGameLogic>();
          foreach (RegionGameLogic region in regions) {
            List<MapEncounterLayerDataCell> mapEncounterLayerDataCellList = Traverse.Create(region).Field<List<MapEncounterLayerDataCell>>("mapEncounterLayerDataCellList").Value;
            //Log.M?.WL(1, $"minimap region:{region.name}:{region.encounterObjectGuid} cells:{mapEncounterLayerDataCellList.Count}");
            HashSet<MapTerrainDataCellEx> regionCells = new HashSet<MapTerrainDataCellEx>();
            foreach (MapEncounterLayerDataCell ecell in mapEncounterLayerDataCellList) {
              MapTerrainDataCellEx cell = ecell.relatedTerrainCell as MapTerrainDataCellEx;
              if (cell == null) { continue; }
              regionCells.Add(cell);
            }
            foreach (MapTerrainDataCellEx cell in regionCells) {
              HashSet<MapTerrainDataCellEx> testCells = new HashSet<MapTerrainDataCellEx>();
              bool neiborsNotInRegion = false;
              for (int x = (cell.x - 4); x < (cell.x + 4); ++x) {
                if (x < 0) { continue; }
                if (x >= minimapXsize) { break; }
                for (int y = (cell.y - 4); y < (cell.y + 4); ++y) {
                  if (y < 0) { continue; }
                  if (y >= minimapYsize) { break; }
                  MapTerrainDataCellEx tcell = combat.MapMetaData.mapTerrainDataCells[x, y] as MapTerrainDataCellEx;
                  if (regionCells.Contains(tcell) == false) { neiborsNotInRegion = true; break; }
                }
                if (neiborsNotInRegion) { break; }
              }
              //Log.M?.WL(2, $"cell:{cell.x},{cell.y} neiborsNotInRegion:{neiborsNotInRegion}");
              if (neiborsNotInRegion) {
                CombatHUDMiniMap.minimapContent.SetPixel(cell.y, cell.x, UIManager.Instance.UIColorRefs.orange);
              } else {
                //CombatHUDMiniMap.minimapContent.SetPixel(cell.y, cell.x, UIManager.Instance.UIColorRefs.blue);
              }
            }
          }
        }

        //for (int x = 0; x < minimapXsize; ++x) {
        //  for (int y = 0; y < minimapYsize; ++y) {
        //    MapTerrainDataCellEx cell = combat.MapMetaData.mapTerrainDataCells[x + startCell.X, y + startCell.Z] as MapTerrainDataCellEx;
        //    float height = (cell.terrainHeight - minHeight) / (maxHeight - minHeight);
        //    heightMap.SetPixel(y, x, new Color(height, height, height, 1f));
        //  }
        //}
        CombatHUDMiniMap.minimapContent.Apply();
        CombatHUDMiniMap.minimapJammedContent.Apply();
        //heightMap.Apply();
        //string path = Path.Combine(Log.BaseDirectory, "minimap.png");
        //byte[] _bytes = heightMap.EncodeToPNG();
        //System.IO.File.WriteAllBytes(path, _bytes);
        //Log.M?.WL(1,_bytes.Length / 1024 + "Kb was saved as: " + path);

      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString());
        CombatHUDMiniMap.minimapContent = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        CombatHUDMiniMap.minimapContent.Apply();
      }
    }
    public CombatHUD HUD;
    public static Dictionary<TerrainMaskFlags, Color> terrainColors = new Dictionary<TerrainMaskFlags, Color>();
    public static CombatHUDMiniMap instance { get; set; } = null;
    public static void Clear() { if (instance != null) { GameObject.Destroy(instance.gameObject); instance = null; }; minimapContent = null; }
    public static Color minimapBurningTerrainColor { get; set; } = Color.magenta;
    public static Color minimapBurnedTerrainColor { get; set; } = Color.black;
    public static void initColors() {
      terrainColors.Clear();
      foreach (var tcol in CustomAmmoCategories.Settings.MinimapTerrainColors) {
        if (ColorUtility.TryParseHtmlString(tcol.Value, out var color)) {
          terrainColors.Add(tcol.Key, color);
        }
      }
      if (ColorUtility.TryParseHtmlString(CustomAmmoCategories.Settings.MinimapBurnedTerrainColor, out var bcol)) {
        minimapBurnedTerrainColor = bcol;
      }
      if (ColorUtility.TryParseHtmlString(CustomAmmoCategories.Settings.MinimapBurnigTerrainColor, out bcol)) {
        minimapBurningTerrainColor = bcol;
      }

      //if (ColorUtility.TryParseHtmlString("#24D3D6FF",out var color)) {
      //  terrainColors.Add(TerrainMaskFlags.Water, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#2475d6FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.DeepWater, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#73a573FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.Forest, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#636363FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.Road, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#da8923FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.Rough, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#6b06a5FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.Custom, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#920000FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.Impassable, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#920000FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.DropshipLandingZone, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#920000FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.DangerousLocation, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#920000FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.DropPodLandingZone, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#877931FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.None, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#877931FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.DestroyedBuilding, color);
      //}
      //if (ColorUtility.TryParseHtmlString("#877931FF", out color)) {
      //  terrainColors.Add(TerrainMaskFlags.UseTerrain, color);
      //}
    }
    public static Texture2D minimapContent { get; set; } = null;
    public static Texture2D minimapJammedContent { get; set; } = null;
    public static Texture2D minimapCameraTexture { get; set; } = null;
    public static float minimapXstart { get; set; } = 0;
    public static float minimapZstart { get; set; } = 0;
    public static float minimapXend { get; set; } = 0;
    public static float minimapZend { get; set; } = 0;
    public static float minimapXsize { get; set; } = 0;
    public static float minimapZsize { get; set; } = 0;
    public static float cellXsize { get; set; } = 0f;
    public static float cellZsize { get; set; } = 0f;
    public RawImage minimap { get; set; } = null;
    public RawImage camera { get; set; } = null;
    public Vector2 cameraPosition = Vector3.zero;
    public Vector2 cameraLook = Vector3.zero;
    public Vector3 cameraRot = Vector3.zero;
    public RectTransform rectTransform { get; set; } = null;
    public Dictionary<AbstractActor, UnitPositionMark> actorsPoints = new Dictionary<AbstractActor, UnitPositionMark>();
    public Dictionary<ObjectiveGameLogic, UnitPositionMark> objectivePoints = new Dictionary<ObjectiveGameLogic, UnitPositionMark>();
    public bool Hovered { get; private set; } = false;
    public override void OnPointerEnter(PointerEventData data) {
      this.rectTransform.localScale = new Vector3(2f, 2f, 1f);
      this.rectTransform.pivot = new Vector2(0f, 0.5f);
      this.rectTransform.SetAsLastSibling();
      Hovered = true;
      HUD.RefreshSidePanelInfo();
    }
    public override void OnPointerExit(PointerEventData data) {
      if (sizeToggled == false) {
        this.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        this.rectTransform.pivot = new Vector2(0f, 1f);
        Hovered = false;
        HUD.RefreshSidePanelInfo();
      }
    }
    public bool sizeToggled { get; set; } = false;
    public CameraSequence cameraSequence { get; set; } = null;
    public void OnDoubleClick(Vector2 position) {
      Vector3[] corners = new Vector3[4];
      this.rectTransform.GetWorldCorners(corners);
      float left = corners[0].x;
      float right = corners[2].x;
      float top = corners[0].y;
      float bottom = corners[2].y;
      Camera mainUiCamera = LazySingletonBehavior<UIManager>.Instance.UICamera;
      Vector3 worldClickPos = mainUiCamera.ScreenToWorldPoint(position);
      float localX = (worldClickPos.x - left) * this.rectTransform.sizeDelta.x / (right - left);
      float localY = (worldClickPos.y - top) * this.rectTransform.sizeDelta.y / (bottom - top);
      Vector3 newCameraPos = Camera.main.gameObject.transform.parent.position;
      newCameraPos.x = this.fromLocalX(localX);
      newCameraPos.z = this.fromLocalY(localY);
      Log.M?.WL(1, $"local double click {localX},{localY} camera:{newCameraPos} canControl:{CameraControl.Instance.CanControl}");
      if (CameraControl.Instance.CanControl) {
        CameraControl.Instance.SetMovingToGroundPos(newCameraPos);
      }
    }
    public void OnClose() {
      this.gameObject.SetActive(false);
      this.MechTray_CompanyLogo?.SetActive(true);
      this.MechTray_GreebleWedge?.SetActive(true);
      CustomAmmoCategories.Settings.EnableMinimap = false;
    }
    public override void OnPointerClick(PointerEventData data) {
      Log.M?.TWL(0, "CombatHUDMiniMap.OnPointerClick called." + data.position + " clickCount:" + data.clickCount);
      if (data.clickCount == 2) { OnDoubleClick(data.position); }
      sizeToggled = !sizeToggled;
    }
    public void Start() {
      rectTransform = this.GetComponent<RectTransform>();
      if (camera == null) {
        GameObject camera_go = GameObject.Instantiate(HUD.MechTray.logoDisplay.gameObject);
        CombatHUDTeamLogoDisplay teamDisplay = camera_go.GetComponent<CombatHUDTeamLogoDisplay>();
        if (teamDisplay != null) { GameObject.Destroy(teamDisplay); }
        camera_go.name = "minimapCamera";
        camera_go.transform.SetParent(this.transform);
        camera_go.transform.localScale = Vector3.one;
        camera_go.SetActive(true);
        camera = camera_go.GetComponent<RawImage>();
        camera.enabled = true;
        RectTransform camera_rt = camera_go.GetComponent<RectTransform>();
        camera_rt.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x / 5f, this.rectTransform.sizeDelta.y / 5f);
        camera_rt.anchorMin = new Vector2(0f, 0f);
        camera_rt.anchorMax = new Vector2(0f, 0f);
        camera_rt.pivot = new Vector2(0.5f, 0f);
        camera.texture = CombatHUDMiniMap.minimapCameraTexture;
        camera_rt.localPosition = Vector3.zero;
      }
    }
    public class UnitPositionMark {
      public RawImage mark { get; set; } = null;
      public Vector3 position = Vector3.zero;
      public Color detectedColor { get; set; } = Color.white;
      public VisibilityLevel lastVisibility { get; set; } = VisibilityLevel.None;
      public bool isDead { get; set; } = false;
      public bool isFriendly { get; set; } = false;
      public UnitPositionMark(RawImage m) {
        this.mark = m;
        position = Vector3.zero;
      }
    }
    public VisibilityLevel GetVisibilityLevel(AbstractActor unit) {
      if (unit.team == unit.Combat.LocalPlayerTeam) { return VisibilityLevel.LOSFull; }
      if (unit.Combat.LocalPlayerTeam.IsFriendly(unit.team)) { return VisibilityLevel.LOSFull; }
      return unit.Combat.LocalPlayerTeam.VisibilityToTarget(unit);
    }
    public static float GetAngle(float cx, float cy, float lx, float ly) {
      Vector2 d = new Vector2(lx - cx, ly - cy);
      if (d.magnitude < CustomAmmoCategories.Epsilon) { return 0f; }
      float sin = Mathf.Abs(d.y) / d.magnitude;
      float result = Mathf.Asin(sin) * Mathf.Rad2Deg;
      if ((d.x >= 0f) && (d.y >= 0f)) { result = result - 90f; } else
      if ((d.x >= 0f) && (d.y <= 0f)) { result = -90f - result; } else
      if ((d.x <= 0f) && (d.y >= 0f)) { result = 90f - result; } else
      if ((d.x <= 0f) && (d.y <= 0f)) { result = 90f + result; }
      return result;
    }
    public float toLocalX(float x) {
      return (x - CombatHUDMiniMap.minimapXstart) * rectTransform.sizeDelta.x / CombatHUDMiniMap.minimapXsize;
    }
    public float toLocalY(float z) {
      return (z - CombatHUDMiniMap.minimapZstart) * rectTransform.sizeDelta.y / CombatHUDMiniMap.minimapZsize;
    }
    public int toTextureX(float x) {
      return (int)((x - CombatHUDMiniMap.minimapXstart) / CombatHUDMiniMap.cellXsize);
    }
    public int toTextureY(float z) {
      return (int)((z - CombatHUDMiniMap.minimapZstart) / CombatHUDMiniMap.cellZsize);
    }
    public float fromLocalX(float x) {
      return (x / rectTransform.sizeDelta.x * CombatHUDMiniMap.minimapXsize) + CombatHUDMiniMap.minimapXstart;
    }
    public float fromLocalY(float z) {
      return (z / rectTransform.sizeDelta.y * CombatHUDMiniMap.minimapZsize) + CombatHUDMiniMap.minimapZstart;
    }
    public class MinimapChangeRequest {
      public int x { get; set; } = 0;
      public int y { get; set; } = 0;
      public Color color { get; set; } = Color.magenta;
      public MinimapChangeRequest(int x, int y, Color c) {
        this.x = x;
        this.y = y;
        this.color = c;
      }
    }
    public float minimap_change_t = 0f;
    public bool minimap_changed = false;
    public static float MINIMAP_FLUSH_INTERVAL = 0.5f;
    public Queue<MinimapChangeRequest> minimapChangeQueue { get; set; } = new Queue<MinimapChangeRequest>();
    public void AddBurning(MapTerrainDataCellEx cell) {
      try {
        if (this.minimap == null) { return; }
        if (this.minimap.texture == null) { return; }
        Vector3 position = cell.WorldPos();
        int x = toTextureX(position.x);
        int y = toTextureY(position.z);
        if ((x >= 0) && (x < this.minimap.texture.width) && (y >= 0) && (y <= this.minimap.texture.width)) {
          minimapChangeQueue.Enqueue(new MinimapChangeRequest(x, y, CombatHUDMiniMap.minimapBurningTerrainColor));
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void AddBurned(MapTerrainDataCellEx cell) {
      try {
        if (this.minimap == null) { return; }
        if (this.minimap.texture == null) { return; }
        Vector3 position = cell.WorldPos();
        int x = toTextureX(position.x);
        int y = toTextureY(position.z);
        if ((x >= 0) && (x < this.minimap.texture.width) && (y >= 0) && (y <= this.minimap.texture.width)) {
          minimapChangeQueue.Enqueue(new MinimapChangeRequest(x, y, CombatHUDMiniMap.minimapBurnedTerrainColor));
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void AddRestore(MapTerrainDataCellEx cell) {
      try {
        if (this.minimap == null) { return; }
        if (this.minimap.texture == null) { return; }
        Vector3 position = cell.WorldPos();
        int x = toTextureX(position.x);
        int y = toTextureY(position.z);
        if ((x >= 0) && (x < (this.minimap.texture.width - 1)) && (y >= 0) && (y < (this.minimap.texture.height - 1))) {
          Color restoreColor = Color.magenta;
          if (CombatHUDMiniMap.terrainColors.TryGetValue(MapMetaData.GetPriorityTerrainMaskFlags(cell), out restoreColor)) {
          }
          minimapChangeQueue.Enqueue(new MinimapChangeRequest(x, y, restoreColor));
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void minimapTextureUpdate(float deltaTime) {
      try {
        if (minimapChangeQueue.Count == 0) {
          if (minimap_changed) {
            if (minimap_change_t > 0f) { minimap_change_t -= deltaTime; } else {
              minimap_change_t = 0f;
              minimap_changed = false;
              if (CombatHUDMiniMap.minimapContent != null) { CombatHUDMiniMap.minimapContent.Apply(); }
            }
          }
        } else {
          minimap_changed = true;
          minimap_change_t = MINIMAP_FLUSH_INTERVAL;
          while (minimapChangeQueue.Count != 0) {
            MinimapChangeRequest chReq = minimapChangeQueue.Dequeue();
            CombatHUDMiniMap.minimapContent.SetPixel(chReq.x + 1, chReq.y + 1, chReq.color);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public bool LastMapJammedState { get; set; } = false;
    public bool LastUnitsJammedState { get; set; } = false;
    public bool MapJammedState { get; set; } = false;
    public bool UnitsJammedState { get; set; } = false;
    public GameObject MechTray_GreebleWedge { get; set; } = null;
    public GameObject MechTray_CompanyLogo { get; set; } = null;
    private float objectiveUpdate_t = 0f;
    public void Update() {
      if (HUD.MechTray.logoDisplay.gameObject.activeSelf) { HUD.MechTray.logoDisplay.gameObject.SetActive(false); }
      if (this.minimap == null) {
        minimap = this.gameObject.GetComponent<RawImage>();
        if (minimap == null) { minimap = this.gameObject.AddComponent<RawImage>(); }
        minimap.color = Color.black;
      }
      if (minimap != null) {
        if (CombatHUDMiniMap.minimapContent == null) { CombatHUDMiniMap.InitMinimap(HUD.Combat); }
        if ((minimap.texture == null) && (CombatHUDMiniMap.minimapContent != null)) { minimap.texture = CombatHUDMiniMap.minimapContent; minimap.color = Color.white; }
        if((minimap.texture != null) && (CombatHUDMiniMap.minimapContent != null) && (CombatHUDMiniMap.minimapJammedContent != null)) {
          if(LastMapJammedState != MapJammedState) {
            LastMapJammedState = MapJammedState;
            minimap.texture = LastMapJammedState ? CombatHUDMiniMap.minimapJammedContent : CombatHUDMiniMap.minimapContent;
            minimap.color = Color.white;
          }
        }
      }
      if (camera != null) {
        if ((camera.texture == null) && (CombatHUDMiniMap.minimapCameraTexture != null)) { camera.texture = CombatHUDMiniMap.minimapCameraTexture; }
      }
      try {
        this.minimapTextureUpdate(Time.deltaTime);
        if (objectiveUpdate_t <= 0f) {
          objectiveUpdate_t = 1f;
          Log.M?.TWL(0,$"minimap objectives update");
          foreach (var objective in objectivePoints) {
            Log.M?.WL(1, $"state:{objective.Key.CurrentObjectiveStatus} markUnitsWith:{objective.Key.markUnitsWith} useBeacon:{objective.Key.useBeacon}");
            if (objective.Key.CurrentObjectiveStatus != ObjectiveStatus.Active) {
              objective.Value.mark.enabled = false; continue;
            }
            Vector3? position = new Vector3?();
            if (objective.Key.markUnitsWith != ObjectiveMark.None) {
              position = objective.Key.GetAverageUnitPosition(true);
            } else {
              if (objective.Key.useBeacon) {
                position = objective.Key.GetBeaconPosition();
              }
            }
            if (position.HasValue) {
              objective.Value.position.x = this.toLocalX(position.Value.x);
              objective.Value.position.y = this.toLocalY(position.Value.z);
              objective.Value.mark.rectTransform.anchoredPosition = objective.Value.position;
              objective.Value.mark.color = objective.Value.detectedColor;
              objective.Value.mark.enabled = true;
            } else {
              objective.Value.mark.enabled = false;
            }
          }
        } else {
          objectiveUpdate_t -= Time.deltaTime;
        }
        HashSet<AbstractActor> allActors = HUD.Combat.AllActors.ToHashSet();
        foreach (AbstractActor unit in allActors) {
          if (unit.IsDeployDirector()) { continue; }
          if (actorsPoints.TryGetValue(unit, out var mark) == false) {
            GameObject mark_go = GameObject.Instantiate(HUD.MechTray.logoDisplay.gameObject);
            CombatHUDTeamLogoDisplay teamDisplay = mark_go.GetComponent<CombatHUDTeamLogoDisplay>();
            if (teamDisplay != null) { GameObject.Destroy(teamDisplay); }
            mark_go.name = unit.PilotableActorDef.ChassisID + "_minimapMark";
            mark_go.transform.SetParent(this.transform);
            mark_go.transform.localScale = Vector3.one;
            mark_go.SetActive(true);
            RawImage img = mark_go.GetComponent<RawImage>();
            mark = new UnitPositionMark(img);
            RectTransform mark_rt = mark_go.GetComponent<RectTransform>();
            mark_rt.sizeDelta = new Vector2(3f, 3f);
            mark_rt.anchorMin = new Vector2(0f, 0f);
            mark_rt.anchorMax = new Vector2(0f, 0f);
            img.texture = null;
            img.color = Color.white;
            if (unit.Combat.LocalPlayerTeam == unit.team) { mark.detectedColor = Color.green; mark.isFriendly = true; } else
            if (unit.Combat.LocalPlayerTeam.IsEnemy(unit.team)) { mark.detectedColor = Color.red; mark.isFriendly = false; } else
            if (unit.Combat.LocalPlayerTeam.IsFriendly(unit.team)) { mark.detectedColor = Color.blue; mark.isFriendly = true; }
            img.enabled = false;
            mark_rt.pivot = new Vector2(0.5f, 0.5f);
            mark_rt.localPosition = Vector3.zero;
            actorsPoints.Add(unit, mark);
            if (camera != null) { camera.transform.SetAsLastSibling(); }
          }
          mark.position.x = this.toLocalX(unit.CurrentPosition.x);
          mark.position.y = this.toLocalY(unit.CurrentPosition.z);
          mark.mark.rectTransform.anchoredPosition = mark.position;
          if ((UnitsJammedState == false)||(mark.isFriendly)) {
            if (mark.isDead != unit.IsDead) {
              mark.isDead = unit.IsDead;
              if (mark.isDead) {
                mark.mark.enabled = ((unit.DeathMethod == DeathMethod.DespawnedEscaped)||(unit.DeathMethod == DeathMethod.DespawnedNoMessage)?false:true); mark.mark.color = Color.black;
              }
            }
            if (mark.isDead == false) {
              VisibilityLevel newVisibility = this.GetVisibilityLevel(unit);
              if (mark.lastVisibility != newVisibility) {
                mark.lastVisibility = newVisibility;
                switch (mark.lastVisibility) {
                  case VisibilityLevel.None: mark.mark.enabled = false; break;
                  case VisibilityLevel.LOSFull: mark.mark.enabled = true; mark.mark.color = mark.detectedColor; break;
                  case VisibilityLevel.Blip4Maximum: mark.mark.enabled = true; mark.mark.color = mark.detectedColor; break;
                  default: mark.mark.enabled = true; mark.mark.color = Color.white; break;
                }
              }
            }
          } else {
            mark.mark.enabled = false;
          }
        }
        if (camera != null) {
          Vector3 cameraPos = CameraControl.Instance.CameraPos;
          Vector3 cameraLookPos = cameraPos + Camera.main.gameObject.transform.forward * 100f;
          cameraPosition.x = this.toLocalX(cameraPos.x);
          cameraPosition.y = this.toLocalY(cameraPos.z);
          cameraLook.x = this.toLocalX(cameraLookPos.x);
          cameraLook.y = this.toLocalY(cameraLookPos.z);
          cameraRot.z = CombatHUDMiniMap.GetAngle(cameraPosition.x, cameraPosition.y, cameraLook.x, cameraLook.y);
          cameraPosition.x = Mathf.Clamp(cameraPosition.x, 0f, this.rectTransform.sizeDelta.x);
          cameraPosition.y = Mathf.Clamp(cameraPosition.y, 0f, this.rectTransform.sizeDelta.y);
          camera.rectTransform.anchoredPosition = cameraPosition;
          camera.transform.localRotation = Quaternion.Euler(cameraRot);
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public void Init(CombatHUD HUD) {
      CombatHUDMiniMap.instance = this;
      this.HUD = HUD;
      HUD.MechTray.TrayPosUp = new Vector3(HUD.MechTray.TrayPosDown.x, 10f, HUD.MechTray.TrayPosDown.z);
      Image img = this.gameObject.GetComponent<Image>();
      if (img != null) {
        GameObject.DestroyImmediate(img);
      }
      minimap = this.gameObject.GetComponent<RawImage>();
      if (minimap != null) { minimap.texture = CombatHUDMiniMap.minimapContent; }
      if (CustomAmmoCategories.Settings.EnableMinimap) {
        if (this.MechTray_GreebleWedge != null) { MechTray_GreebleWedge.SetActive(false); }
        if (this.MechTray_CompanyLogo != null) { MechTray_CompanyLogo.SetActive(false); }
        this.gameObject.SetActive(true);
      } else {
        if (this.MechTray_GreebleWedge != null) { MechTray_GreebleWedge.SetActive(true); }
        if (this.MechTray_CompanyLogo != null) { MechTray_CompanyLogo.SetActive(true); }
        this.gameObject.SetActive(false);
      }
      if (MechTray_GreebleWedge != null) {
        CombatHUDMiniMapEnabler enabler = MechTray_GreebleWedge.GetComponent<CombatHUDMiniMapEnabler>();
        if (enabler == null) {
          enabler = MechTray_GreebleWedge.AddComponent<CombatHUDMiniMapEnabler>();
          CanvasGroup canvasGroup = MechTray_GreebleWedge.GetComponent<CanvasGroup>();
          if(canvasGroup != null) {
            canvasGroup.blocksRaycasts = true;
          }          
        }
        enabler.minimap = this;
      }
      foreach (var objective in this.objectivePoints) {
        GameObject.DestroyImmediate(objective.Value.mark.gameObject);
      }
      if (CustomAmmoCategories.Settings.MinimapShowObjectives) {
        ObjectiveGameLogic[] objectives = HUD.Combat.EncounterLayerData.gameObject.GetComponentsInChildren<ObjectiveGameLogic>();
        foreach (var objective in objectives) {
          GameObject mark_go = GameObject.Instantiate(HUD.MechTray.logoDisplay.gameObject);
          CombatHUDTeamLogoDisplay teamDisplay = mark_go.GetComponent<CombatHUDTeamLogoDisplay>();
          if (teamDisplay != null) { GameObject.Destroy(teamDisplay); }
          mark_go.name = objective.name + "_minimapMark";
          mark_go.transform.SetParent(this.transform);
          mark_go.transform.localScale = Vector3.one;
          mark_go.SetActive(true);
          RawImage mark_img = mark_go.GetComponent<RawImage>();
          UnitPositionMark mark = new UnitPositionMark(mark_img);
          RectTransform mark_rt = mark_go.GetComponent<RectTransform>();
          mark_rt.sizeDelta = new Vector2(3f, 3f);
          mark_rt.anchorMin = new Vector2(0f, 0f);
          mark_rt.anchorMax = new Vector2(0f, 0f);
          mark_img.texture = null;
          mark_img.color = Color.white;
          mark.detectedColor = UIManager.Instance.UIColorRefs.orange;
          mark.isFriendly = true;
          mark_img.enabled = false;
          mark_rt.pivot = new Vector2(0.5f, 0.5f);
          mark_rt.localPosition = Vector3.zero;
          this.objectivePoints.Add(objective, mark);
        }
      }
    }
    public static void Create(CombatHUD HUD) {
      try {
        Log.M?.TWL(0, "CombatHUDMiniMap.Create", true);
        CombatHUDMiniMap minimap = HUD.MechTray.gameObject.GetComponentInChildren<CombatHUDMiniMap>(true);
        if (minimap != null) {
          minimap.Init(HUD);
          return;
        }
        Transform MechTrayBGImage = HUD.MechTray.gameObject.transform.Find("MechTrayBGImage");
        if (MechTrayBGImage == null) { Log.M?.TWL(0, "can't find MechTrayBGImage"); return; }
        RectTransform MechTray_MechNameBackground = HUD.MechTray.gameObject.transform.Find("MechTray_MechNameBackground") as RectTransform;

        GameObject MechTrayBGImage_go = GameObject.Instantiate(MechTrayBGImage.gameObject);
        HashSet<Transform> childs = new HashSet<Transform>();
        foreach (Transform tr in MechTrayBGImage_go.GetComponentsInChildren<Transform>(true)) {
          if (tr.parent != MechTrayBGImage_go.transform) { continue; }
          childs.Add(tr);
        }
        foreach (Transform tr in childs) { GameObject.Destroy(tr.gameObject); }
        MechTrayBGImage_go.name = "MechTrayMiniMap";
        MechTrayBGImage_go.transform.SetParent(MechTrayBGImage.transform.parent);
        MechTrayBGImage_go.transform.SetAsLastSibling();
        MechTrayBGImage_go.transform.localScale = Vector3.one;
        MechTrayBGImage_go.transform.localPosition = Vector3.zero;
        RectTransform MechTrayMiniMap = MechTrayBGImage_go.GetComponent<RectTransform>();
        MechTrayMiniMap.sizeDelta = new Vector2(MechTray_MechNameBackground.sizeDelta.x, MechTray_MechNameBackground.sizeDelta.x);
        MechTrayMiniMap.pivot = new Vector2(0f, 1f);
        MechTrayMiniMap.anchorMin = Vector2.zero;
        MechTrayMiniMap.anchorMax = Vector2.zero;
        MechTrayMiniMap.anchoredPosition = new Vector2(MechTray_MechNameBackground.anchoredPosition.x, MechTray_MechNameBackground.sizeDelta.x);
        minimap = MechTrayBGImage_go.AddComponent<CombatHUDMiniMap>();
        RectTransform MechTray_GreebleWedge = HUD.MechTray.gameObject.transform.Find("MechTray_GreebleWedge") as RectTransform;
        if (MechTray_GreebleWedge != null) {
          minimap.MechTray_GreebleWedge = MechTray_GreebleWedge.gameObject;
        }
        RectTransform MechTray_CompanyLogo = HUD.MechTray.gameObject.transform.Find("MechTray_CompanyLogo") as RectTransform;
        if (MechTray_CompanyLogo != null) { minimap.MechTray_CompanyLogo = MechTray_CompanyLogo.gameObject; }
        if (CombatHUDMiniMap.closeButtonSource != null) {
          GameObject minimapCloseBtn = GameObject.Instantiate(CombatHUDMiniMap.closeButtonSource);
          minimapCloseBtn.transform.SetParent(MechTrayBGImage_go.transform);
          minimapCloseBtn.transform.localScale = Vector3.one * 0.5f;
          minimapCloseBtn.transform.localPosition = Vector3.zero;
          minimapCloseBtn.SetActive(true);
          HBSDOTweenButton closeBtn = minimapCloseBtn.GetComponent<HBSDOTweenButton>();
          closeBtn.OnClicked.AddListener(new UnityEngine.Events.UnityAction(minimap.OnClose));
        }
        minimap.Init(HUD);
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
}