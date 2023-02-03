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
    public string FriendlyName { get; set; } = string.Empty;
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
      try {
        if (intelMinimaps.TryGetValue(contract.mapName, out Sprite result)) {
          return result;
        }
        Log.TWL(0,$"GetIntelMinimapSprite:{contract.mapName}");
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
        Log.WL(1,$"maxHeight:{maxHeight} minHeight:{minHeight}");
        bool noheightdiff = false;
        if ((maxHeight - minHeight) < Core.Epsilon) { noheightdiff = true; }
        bool hasForest = Traverse.Create(mapMetaData).Field<string>("forestDesignMaskName").Value.Contains("Forest");
        for (int x = 0; x < minimapXsize; ++x) {
          for (int y = 0; y < minimapYsize; ++y) {
            MapTerrainDataCell cell = mapMetaData.mapTerrainDataCells[x, y];
            float heightColor = 0.7f;
            if (noheightdiff == false) { heightColor = (cell.terrainHeight - minHeight) / (maxHeight - minHeight); };
            heightColor *= 0.8f;
            heightColor += 0.2f;
            Color color = new Color(heightColor, heightColor, heightColor, 1f);
            switch (MapMetaData.GetPriorityTerrainMaskFlags(cell.terrainMask)) {
              case TerrainMaskFlags.Impassable: { color.g = 0f; color.b = 0f; }; break;
              case TerrainMaskFlags.DeepWater: { color.r = 0f; color.g = 0f; }; break;
              case TerrainMaskFlags.Water: { color.r = 0f; color.g = 0f; }; break;
              case TerrainMaskFlags.Road: { color.b = 0f; color.g *= 0.5f; color.r *= 0.5f; }; break;
              case TerrainMaskFlags.Custom: { color.g = 0f; }; break;
              case TerrainMaskFlags.Forest: { color.r = 0f; color.b = 0f; }; break;
            }
            minimap.SetPixel(y, x, color);
          }
        }
        minimap.Apply();
        result = Sprite.Create(minimap, new UnityEngine.Rect(0.0f, 0.0f, (float)minimap.width, (float)minimap.height), new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect, Vector4.zero);
        intelMinimaps.Add(contract.mapName, result);
        return result;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return null;
      }
    }
    public LocalizableText moodDescription { get; set; } = null;
    public Image minimapBackground { get; set; } = null;
    public HBSTooltip weatherTooltip { get; set; } = null;
    public HBSTooltipStateData weatherTooltipData { get; set; } = null;
    public CustomWeatherEffectIntel customMood { get; set; } = null;
    public bool moodNameIsSet { get; set; } = false;
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
      if (moodNameIsSet == false) {
        moodDescription.SetText($"Weather: <color=#{ColorUtility.ToHtmlStringRGBA(UIManager.Instance.UIColorRefs.gold)}>{moodMask.Description.Name}</color>");
        moodNameIsSet = true;
      }
    }
    public void Init(LocalizableText ContractDescriptionField, Contract contract) {
      try {
        //LocalizableText ContractDescriptionField = Traverse.Create(parent).Field<LocalizableText>("ContractDescriptionField").Value;
        moodDescription = ContractDescriptionField.transform.parent.gameObject.FindComponent<LocalizableText>("txt_mood");
        if (moodDescription == null) {
          moodDescription = GameObject.Instantiate(ContractDescriptionField.gameObject).GetComponent<LocalizableText>();
          moodDescription.gameObject.transform.SetParent(ContractDescriptionField.transform.parent);
          moodDescription.gameObject.transform.localScale = Vector3.one;
          moodDescription.gameObject.name = "txt_mood";
        }
        Mood_MDD mood_MDD = MetadataDatabase.Instance.GetMood(contract.mapMood);
        string moodName = contract.mapMood;
        if (mood_MDD != null){ moodName = mood_MDD.FriendlyName; }
        weatherTooltip = moodDescription.gameObject.GetComponent<HBSTooltip>();
        if (weatherTooltip == null) { weatherTooltip = moodDescription.gameObject.AddComponent<HBSTooltip>(); }
        weatherTooltipData = new HBSTooltipStateData();
        weatherTooltipData.SetString("Nothing special");
        this.customMood = null;
        string designMask = Biome.GetDesignMaskNameFromBiomeSkin(contract.ContractBiome);
        this.moodNameIsSet = false;
        if (IntelHelper.moods.TryGetValue(contract.mapMood, out var custMood)) {
          this.customMood = custMood;
          if (string.IsNullOrEmpty(customMood.FriendlyName) == false) { moodName = customMood.FriendlyName; this.moodNameIsSet = true; }
          if (string.IsNullOrEmpty(customMood.designMask) == false) {
            if (UnityGameInstance.BattleTechGame.DataManager.ResourceLocator.EntryByID(customMood.designMask, BattleTechResourceType.DesignMaskDef) != null) {
              designMask = customMood.designMask;
            }
          }
        } else {
          this.moodNameIsSet = true;
        }
        if (UnityGameInstance.BattleTechGame.DataManager.Exists(BattleTechResourceType.DesignMaskDef, designMask)) {
          DesignMaskDef moodMask = UnityGameInstance.BattleTechGame.DataManager.DesignMaskDefs.Get(designMask);
          weatherTooltipData = new BaseDescriptionDef(moodMask.Description.Id, moodMask.Description.Name, moodMask.Description.Details, moodMask.Description.Icon).GetTooltipStateData();
          if (moodNameIsSet == false) { moodName = moodMask.Description.Name; this.moodNameIsSet = true; }
        } else {
          weatherTooltipData.SetString("Loading ...");
          weatherDesignMask = designMask;
          LoadRequest loadRequest = UnityGameInstance.BattleTechGame.DataManager.CreateLoadRequest(new Action<LoadRequest>(this.RequestDesignMaskComplete));
          loadRequest.AddBlindLoadRequest(BattleTechResourceType.DesignMaskDef, designMask);
          loadRequest.ProcessRequests(10U);
        }
        moodDescription.SetText($"Weather: <color=#{ColorUtility.ToHtmlStringRGBA(UIManager.Instance.UIColorRefs.gold)}>{moodName}</color>");
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
        minimapBackground.sprite = LanceContractIntelWidget.GetIntelMinimapSprite(contract);
        Log.TWL(0, $"Core.Settings.IntelShowMood:{Core.Settings.IntelShowMood} Core.Settings.IntelShowMiniMap:{Core.Settings.IntelShowMiniMap}");
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
            moodDescription.gameObject.SetActive(false);
          } else {
            Statistic stat = UnityGameInstance.BattleTechGame.Simulation.CompanyStats.GetStatistic(Core.Settings.IntelCompanyStatShowMood);
            if (stat == null) {
              moodDescription.gameObject.SetActive(false);
            } else if (stat.Value<bool>() == false) {
              moodDescription.gameObject.SetActive(false);
            } else {
              moodDescription.gameObject.SetActive(true);
            }
          }
        } else {
          moodDescription.gameObject.SetActive(true);
        }
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
    public static void Postfix(LanceContractDetailsWidget __instance, Contract contract, LocalizableText ___ContractDescriptionField) {
      try {
        LayoutElement layoutElement = ___ContractDescriptionField.gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null) { layoutElement = ___ContractDescriptionField.gameObject.AddComponent<LayoutElement>(); }
        LanceContractIntelWidget intel = __instance.gameObject.GetComponent<LanceContractIntelWidget>();
        if (intel == null) {
          intel = __instance.gameObject.AddComponent<LanceContractIntelWidget>();
        };
        intel.Init(___ContractDescriptionField, contract);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SGContractsWidget))]
  [HarmonyPatch("PopulateContract")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Contract), typeof(Action) })]
  public static class SGContractsWidget_PopulateContract {
    public static void Postfix(SGContractsWidget __instance, Contract contract, LocalizableText ___ContractDescriptionField) {
      try {
        LayoutElement layoutElement = ___ContractDescriptionField.gameObject.GetComponent<LayoutElement>();
        if (layoutElement == null) { layoutElement = ___ContractDescriptionField.gameObject.AddComponent<LayoutElement>(); }
        LanceContractIntelWidget intel = __instance.gameObject.GetComponent<LanceContractIntelWidget>();
        if (intel == null) {
          intel = __instance.gameObject.AddComponent<LanceContractIntelWidget>();
        };
        intel.Init(___ContractDescriptionField, contract);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}