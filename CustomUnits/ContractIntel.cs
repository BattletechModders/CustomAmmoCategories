using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using HBS.Collections;
using IRBTModUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnits {
  public class CustomWeatherEffectIntel {
    public string ID { get; set; } = string.Empty;
    public string designMask { get; set; } = string.Empty;
  }
  public static class IntelHelper {
    public static Dictionary<string, CustomWeatherEffectIntel> moods { get; set; } = new Dictionary<string, CustomWeatherEffectIntel>();
    public static void AddMood(string id,VersionManifestEntry entry) {
      CustomWeatherEffectIntel def = JsonConvert.DeserializeObject<CustomWeatherEffectIntel>(File.ReadAllText(entry.FilePath));
      Log.WL(1, "id:" + def.ID);
      if (moods.ContainsKey(def.ID)) { moods[def.ID] = def; } else { moods.Add(def.ID, def); }
    }
  }
  public class LanceContractIntelWidget: MonoBehaviour {
    public static Dictionary<string, Sprite> intelMinimaps = new Dictionary<string, Sprite>();
    public static Sprite GetIntelMinimapSprite(Contract contract) {
      if(intelMinimaps.TryGetValue(contract.mapName, out Sprite result)) {
        return result;
      }
      MapMetaData mapMetaData = MapMetaData.LoadMapMetaData(contract, contract.DataManager);
      int minimapXsize = mapMetaData.mapTerrainDataCells.GetLength(0);
      int minimapYsize = mapMetaData.mapTerrainDataCells.GetLength(1);
      Texture2D minimap = new Texture2D(minimapYsize, minimapXsize, TextureFormat.ARGB32, false);
      float maxHeight = mapMetaData.mapTerrainDataCells[0, 0].terrainHeight;
      float minHeight = mapMetaData.mapTerrainDataCells[0, 0].terrainHeight;
      for (int x = 0; x < minimapXsize; ++x) {
        for (int y = 0; y < minimapYsize; ++y) {
          MapTerrainDataCell cell = mapMetaData.mapTerrainDataCells[x, y];
          if (cell.terrainHeight > maxHeight) { maxHeight = cell.terrainHeight; }
          if (cell.terrainHeight < minHeight) { minHeight = cell.terrainHeight; }
        }
      }
      bool hasForest = Traverse.Create(mapMetaData).Field<string>("forestDesignMaskName").Value.Contains("Forest");
      for (int x = 0; x < minimapXsize; ++x) {
        for (int y = 0; y < minimapYsize; ++y) {
          MapTerrainDataCell cell = mapMetaData.mapTerrainDataCells[x, y];
          float heightColor = (cell.terrainHeight - minHeight) / (maxHeight - minHeight);
          heightColor *= 0.8f;
          heightColor += 0.2f;
          Color color = new Color(heightColor, heightColor, heightColor, 1f);
          if (SplatMapInfo.IsWater(cell.terrainMask) || SplatMapInfo.IsDeepWater(cell.terrainMask)) {
            color.r = 0f; color.g = 0f;
          } else if(hasForest && SplatMapInfo.IsForest(cell.terrainMask)) {
            color.r = 0f; color.b = 0f;
          }
          minimap.SetPixel(y, x, color);
        }
      }
      minimap.Apply();
      result = Sprite.Create(minimap, new UnityEngine.Rect(0.0f, 0.0f, (float)minimap.width, (float)minimap.height), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect, Vector4.zero);
      intelMinimaps.Add(contract.mapName, result);
      return result;
    }
    public LocalizableText moodDesctiption { get; set; } = null;
    public Image minimapBackground { get; set; } = null;
    public LanceContractDetailsWidget parent { get; set; } = null;
    public HBSTooltip weatherTooltip { get; set; } = null;
    public HBSTooltipStateData weatherTooltipData { get; set; } = null;
    private string weatherDesignMask { get; set; } = string.Empty;
    public void Update() {
      if(minimapBackground != null) {
        if (minimapBackground.color != Color.white) { minimapBackground.color = Color.white; }
      }
    }
    public void RequestDesignMaskComplete(LoadRequest loadRequest) {
      if (string.IsNullOrEmpty(weatherDesignMask)) { return; }
      if (UnityGameInstance.BattleTechGame.DataManager.DesignMaskDefs.Exists(weatherDesignMask) == false) { return; }
      DesignMaskDef moodMask = UnityGameInstance.BattleTechGame.DataManager.DesignMaskDefs.Get(weatherDesignMask);
      weatherTooltipData = new BaseDescriptionDef(moodMask.Description.Id, moodMask.Description.Name, moodMask.Description.Details, moodMask.Description.Icon).GetTooltipStateData();
      weatherTooltip.SetDefaultStateData(weatherTooltipData);
      //moodDesctiption.SetText($"Weather: {moodMask.Description.Name}");
    }
    public void Init(LanceContractDetailsWidget details) {
      try {
        parent = details;
        LocalizableText ContractDescriptionField = Traverse.Create(parent).Field<LocalizableText>("ContractDescriptionField").Value;
        moodDesctiption = ContractDescriptionField.transform.parent.gameObject.FindComponent<LocalizableText>("txt_mood");
        if (moodDesctiption == null) {
          moodDesctiption = GameObject.Instantiate(ContractDescriptionField.gameObject).GetComponent<LocalizableText>();
          moodDesctiption.gameObject.transform.SetParent(ContractDescriptionField.transform.parent);
          moodDesctiption.gameObject.transform.localScale = Vector3.one;
          moodDesctiption.gameObject.name = "txt_mood";
        }
        Mood_MDD mood_MDD = MetadataDatabase.Instance.GetMood(parent.SelectedContract.mapMood);
        string moodName = parent.SelectedContract.mapMood;
        if (mood_MDD != null){ moodName = mood_MDD.FriendlyName; }
        moodDesctiption.SetText($"Weather: {moodName}");
        weatherTooltip = moodDesctiption.gameObject.GetComponent<HBSTooltip>();
        if (weatherTooltip == null) { weatherTooltip = moodDesctiption.gameObject.AddComponent<HBSTooltip>(); }
        weatherTooltipData = new HBSTooltipStateData();
        weatherTooltipData.SetString("Nothing special");
        if (IntelHelper.moods.TryGetValue(parent.SelectedContract.mapMood, out var custMood)) {
          if (string.IsNullOrEmpty(custMood.designMask) == false) {
            if (UnityGameInstance.BattleTechGame.DataManager.Exists(BattleTechResourceType.DesignMaskDef, custMood.designMask)) {
              DesignMaskDef moodMask = UnityGameInstance.BattleTechGame.DataManager.DesignMaskDefs.Get(custMood.designMask);
              weatherTooltipData = new BaseDescriptionDef(moodMask.Description.Id, moodMask.Description.Name, moodMask.Description.Details, moodMask.Description.Icon).GetTooltipStateData();
            } else if (UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.EntryByID(custMood.designMask, BattleTechResourceType.DesignMaskDef) != null) {
              weatherTooltipData.SetString("Loading ...");
              weatherDesignMask = custMood.designMask;
              LoadRequest loadRequest = UnityGameInstance.BattleTechGame.DataManager.CreateLoadRequest(new Action<LoadRequest>(this.RequestDesignMaskComplete));
              loadRequest.AddBlindLoadRequest(BattleTechResourceType.DesignMaskDef, custMood.designMask);
              loadRequest.ProcessRequests(10U);
            }
          }
        }
        weatherTooltip.SetDefaultStateData(weatherTooltipData);
        minimapBackground = ContractDescriptionField.transform.parent.gameObject.FindComponent<Image>("img_minimap_back");
        if (minimapBackground == null) {
          RectTransform layout_detail = this.transform.gameObject.FindComponent<RectTransform>("layout_detail");
          if (layout_detail != null) {
            Image bg_fill = layout_detail.gameObject.FindComponent<Image>("bg-fill");
            if (bg_fill) {
              minimapBackground = GameObject.Instantiate(bg_fill.gameObject).GetComponent<Image>();
              LayoutElement layoutElement = minimapBackground.gameObject.GetComponent<LayoutElement>();
              if (layoutElement == null) { layoutElement = minimapBackground.gameObject.AddComponent<LayoutElement>(); }
              minimapBackground.gameObject.transform.SetParent(ContractDescriptionField.transform.parent);
              minimapBackground.gameObject.transform.localScale = Vector3.one;
              minimapBackground.gameObject.name = "img_minimap_back";
              minimapBackground.rectTransform.sizeDelta = new Vector2(200f, 200f);
              minimapBackground.color = Color.white;
            }
          }
        }
        minimapBackground.sprite = LanceContractIntelWidget.GetIntelMinimapSprite(parent.SelectedContract);
        if (Core.Settings.IntelShowMiniMap == false) {
          if(UnityGameInstance.BattleTechGame.Simulation == null) {
            minimapBackground.gameObject.SetActive(false);
          } else {
            Statistic stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetStatistic(Core.Settings.IntelCompanyStatShowMiniMap);
            if (stat == null) {
              minimapBackground.gameObject.SetActive(false);
            } else if (stat.Value<bool>() == false) {
              minimapBackground.gameObject.SetActive(false);
            } else {
              minimapBackground.gameObject.SetActive(true);
            }
          }
        } else {
          minimapBackground.gameObject.SetActive(true);
        }
        if (Core.Settings.IntelShowMood == false) {
          if (UnityGameInstance.BattleTechGame.Simulation == null) {
            moodDesctiption.gameObject.SetActive(false);
          } else {
            Statistic stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetStatistic(Core.Settings.IntelCompanyStatShowMood);
            if (stat == null) {
              moodDesctiption.gameObject.SetActive(false);
            } else if (stat.Value<bool>() == false) {
              moodDesctiption.gameObject.SetActive(false);
            } else {
              moodDesctiption.gameObject.SetActive(true);
            }
          }
        } else {
          moodDesctiption.gameObject.SetActive(true);
        }
        //Thread.CurrentThread.SetFlag(Contract_BeginRequestResources_Intel.STOP_Contract_BeginRequestResources);
        //parent.SelectedContract.Resume();
        //Thread.CurrentThread.ClearFlag(Contract_BeginRequestResources_Intel.STOP_Contract_BeginRequestResources);
        //parent.SelectedContract.Override.SetupContract(parent.SelectedContract);
        //parent.SelectedContract.Override.GenerateUnits(UnityGameInstance.BattleTechGame.DataManager, Traverse.Create(parent.SelectedContract).Method("GetSimGameCurrentDate").GetValue<DateTime?>(), Traverse.Create(parent.SelectedContract).Method("GetCompanyTags").GetValue<TagSet>());
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(LanceContractDetailsWidget))]
  [HarmonyPatch("PopulateContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(LanceConfiguratorPanel), typeof(Contract) })]
  public static class LanceContractDetailsWidget_PopulateContract {
    public static void Postfix(LanceContractDetailsWidget __instance, LanceConfiguratorPanel LC, Contract contract, LocalizableText ___ContractDescriptionField) {
      try {
        LayoutElement layoutElement = ___ContractDescriptionField.gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null) { layoutElement = ___ContractDescriptionField.gameObject.AddComponent<LayoutElement>(); }
        LanceContractIntelWidget intel = __instance.gameObject.GetComponent<LanceContractIntelWidget>();
        if (intel == null) {
          intel = __instance.gameObject.AddComponent<LanceContractIntelWidget>();
        };
        intel.Init(__instance);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  //[HarmonyPatch(typeof(ContractOverride))]
  //[HarmonyPatch("GenerateTeam")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(MetadataDatabase), typeof(TeamOverride), typeof(DateTime?), typeof(TagSet) })]
  //public static class ContractOverride_GenerateTeam {
  //  public static void Prefix(ContractOverride __instance, MetadataDatabase mdd, TeamOverride teamOverride, DateTime? currentDate, TagSet companyTags) {
  //    try {
  //      Log.TWL(0, $"ContractOverride.GenerateTeam prefix {teamOverride.teamName}:{teamOverride.teamGuid} lanceOverrideList:{teamOverride.lanceOverrideList.Count}");
  //      foreach (var lanceOverride in teamOverride.lanceOverrideList) {
  //        LanceDef loadedLanceDef = Traverse.Create(lanceOverride).Field<LanceDef>("loadedLanceDef").Value;
  //        Log.WL(1,$"{lanceOverride.name}:{lanceOverride.GUID} {lanceOverride.unitSpawnPointOverrideList.Count} lanceDef:{lanceOverride.lanceDefId}:{lanceOverride.selectedLanceDefId}");
  //        foreach(var unit in lanceOverride.unitSpawnPointOverrideList) {
  //          Log.WL(2,$"{unit.selectedPilotDefId}:{unit.selectedUnitDefId}:{unit.unitType}");
  //        }
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //  public static void Postfix(ContractOverride __instance, MetadataDatabase mdd, TeamOverride teamOverride, DateTime? currentDate, TagSet companyTags) {
  //    try {
  //      //Log.TWL(0, $"ContractOverride.GenerateTeam postfix {teamOverride.teamName}:{teamOverride.teamGuid} lanceOverrideList:{teamOverride.lanceOverrideList.Count}");
  //      //foreach (var lanceOverride in teamOverride.lanceOverrideList) {
  //      //  LanceDef loadedLanceDef = Traverse.Create(lanceOverride).Field<LanceDef>("loadedLanceDef").Value;
  //      //  Log.WL(1, $"{lanceOverride.name}:{lanceOverride.GUID} {lanceOverride.unitSpawnPointOverrideList.Count} lanceDef:{lanceOverride.lanceDefId}:{lanceOverride.selectedLanceDefId}");
  //      //  foreach (var unit in lanceOverride.unitSpawnPointOverrideList) {
  //      //    Log.WL(2, $"{unit.selectedPilotDefId}:{unit.selectedUnitDefId}:{unit.unitType}");
  //      //  }
  //      //}
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  public static class Contract_BeginRequestResources_Intel {
    public static readonly string STOP_Contract_BeginRequestResources = "STOP_Contract_BeginRequestResources";
    public static MethodInfo TargetMethod() { return AccessTools.Method(typeof(Contract), "BeginRequestResources"); }
    public static HarmonyMethod Patch() { return new HarmonyMethod(AccessTools.Method(typeof(Contract_BeginRequestResources_Intel), nameof(Prefix))); }
    public static bool Prefix(Contract __instance, bool generateUnits) {
      return Thread.CurrentThread.isFlagSet(STOP_Contract_BeginRequestResources) == false;
    }
  }
  //[HarmonyPatch(typeof(Contract))]
  //[HarmonyPatch(MethodType.Constructor)]
  //[HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(string), typeof(ContractTypeValue), typeof(GameInstance), typeof(ContractOverride), typeof(GameContext), typeof(bool), typeof(int), typeof(int), typeof(int?) })]
  //public static class Contract_Constructor {
  //  public static void Postfix(Contract __instance, DataManager ___dataManager) {
  //    try {
  //      Log.TWL(0, $"Contract.Constructor");
  //      //__instance.Override.SetupContract(__instance);
  //      //__instance.Override.GenerateUnits(___dataManager, Traverse.Create(__instance).Method("GetSimGameCurrentDate").GetValue<DateTime?>(), Traverse.Create(__instance).Method("GetCompanyTags").GetValue<TagSet>());
  //      //Log.TWL(0, $"generated");
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
}