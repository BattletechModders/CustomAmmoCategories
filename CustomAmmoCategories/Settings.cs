/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustomAmmoCategoriesLog;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomSettings;
using Log = CustomAmmoCategoriesLog.Log;

namespace CustAmmoCategories {
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public class GameplaySafe : System.Attribute {
    public GameplaySafe() { }
  }
  [SelfDocumentedClass("Settings", "CustomAmmoCategoriesSettings", "Settings")]
  public class Settings {
    [SkipDocumentation, GameplaySafe]
    public bool debugLog { get; set; }
    public bool forbiddenRangeEnable { get; set; } = true;
    public bool AmmoCanBeExhausted { get; set; } = true;
    public bool Joke { get; set; } = false;
    public float ClusterAIMult { get; set; } = 0.2f;
    public float PenetrateAIMult { get; set; } = 0.4f;
    public float JamAIAvoid { get; set; } = 1.0f;
    public float DamageJamAIAvoid { get; set; } = 2.0f;
    public bool modHTTPServer { get; set; } = true;
    public string modHTTPListen { get; set; } = "http://localhost:65080/";
    public string WeaponRealizerStandalone { get; set; } = "WeaponRealizer.dll";
    public string AIMStandalone { get; set; } = "AttackImprovementMod.dll";
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> DynamicDesignMasksDefs { get; set; } = new List<string>();
    public string BurningTerrainDesignMask { get; set; } = "DesignMaskBurningTerrain";
    public string BurningForestDesignMask { get; set; } = "DesignMaskBurningForest";
    [GameplaySafe]
    public string BurningFX { get; set; } = "vfxPrfPrtl_fireTerrain_lrgLoop";
    [GameplaySafe]
    public string BurnedFX { get; set; } = "vfxPrfPrtl_miningSmokePlume_lrg_loop";
    [GameplaySafe]
    public float BurningScaleX { get; set; } = 1f;
    [GameplaySafe]
    public float BurningScaleY { get; set; } = 1f;
    [GameplaySafe]
    public float BurningScaleZ { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleX { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleY { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedScaleZ { get; set; } = 1f;
    [GameplaySafe]
    public float BurnedOffsetX { get; set; } = 0f;
    [GameplaySafe]
    public float BurnedOffsetY { get; set; } = 0f;
    [GameplaySafe]
    public float BurnedOffsetZ { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetX { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetY { get; set; } = 0f;
    [GameplaySafe]
    public float BurningOffsetZ { get; set; } = 0f;
    public string BurnedForestDesignMask { get; set; } = "DesignMaskBurnedForest";
    public int BurningForestCellRadius { get; set; } = 3;
    public int BurningForestTurns { get; set; } = 3;
    public int BurningForestStrength { get; set; } = 5;
    public float BurningForestBaseExpandChance { get; set; } = 0.5f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> AdditinalAssets { get; set; } = new List<string>();
    [GameplaySafe]
    public bool DontShowNotDangerouceJammMessages { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> NoForestBiomes { get; set; } = new List<string>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> ForestBurningDurationBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> WeaponBurningDurationBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> ForestBurningStrengthBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> WeaponBurningStrengthBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> LitFireChanceBiomeMult { get; set; } = new Dictionary<string, float>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<string>\":<float>}")]
    public Dictionary<string, float> MineFieldPathingMods { get; set; } = new Dictionary<string, float>();
    public int JumpLandingMineAttractRadius { get; set; } = 2;
    public int AttackSequenceMaxLength { get; set; } = 15000;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("BurnedTreesSettings structure")]
    [GameplaySafe]
    public BurnedTreesSettings BurnedTrees { get; set; } = new BurnedTreesSettings();
    [GameplaySafe]
    public bool DontShowBurnedTrees { get; set; } = false;
    [GameplaySafe]
    public bool DontShowBurnedTreesTemporary { get; set; } = false;
    [GameplaySafe]
    public bool DontShowScorchTerrain { get; set; } = false;
    public float AAMSAICoeff { get; set; } = 0.2f;
    public bool AIPeerToPeerNodeEnabled { get; set; } = false;
    public bool AIPeerToPeerFirewallPierceThrough { get; set; } = false;
    public string WeaponRealizerSettings { get; set; } = "WeaponRealizerSettings.json";
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("AmmoCookoffSettings structure")]
    public AmmoCookoffSettings AmmoCookoff { get; set; } = new AmmoCookoffSettings();
    //public bool WaterHeightFix { get; set; }
    public float TerrainFiendlyFireRadius { get; set; } = 10f;
    public bool AdvancedCirtProcessing { get; set; } = true;
    public bool DestroyedComponentsCritTrap { get; set; } = true;
    public bool CritLocationTransfer { get; set; } = true;
    public float APMinCritChance { get; set; } = 0.1f;
    public string RemoveFromCritRollStatName { get; set; } = "IgnoreDamage";
    public bool SpawnMenuEnabled { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("boolean")]
    public bool NoCritFloatieMessage { get { return FNoCritFloatieMessage == TripleBoolean.True; } set { FNoCritFloatieMessage = value ? TripleBoolean.True : TripleBoolean.False; } }
    [JsonIgnore, SkipDocumentation]
    public TripleBoolean FNoCritFloatieMessage { get; private set; } = TripleBoolean.NotSet;
    [JsonIgnore, SkipDocumentation]
    public TripleBoolean MechEngineerDetected { get; set; } = TripleBoolean.NotSet;
    public void MechEngineerDetect() {
      //Log.M.WL(0, "Detecting MechEngineer:");
      if (MechEngineerDetected != TripleBoolean.NotSet) { return; }
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        Log.M.WL(1, assembly.GetName().Name);
        if (assembly.GetName().Name == "MechEngineer") { MechEngineerDetected = TripleBoolean.True; return; }
      }
      MechEngineerDetected = TripleBoolean.False;
    }
    public bool NoCritFloatie() {
      if (FNoCritFloatieMessage != TripleBoolean.NotSet) { return FNoCritFloatieMessage != TripleBoolean.False; }
      MechEngineerDetect();
      return MechEngineerDetected != TripleBoolean.False;
    }
    [SkipDocumentation, JsonIgnore]
    public HashSet<Strings.Culture> patchWeaponSlotsOverflowCultures { get; private set; } = new HashSet<Strings.Culture>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of Strings.Culture")]
    public List<Strings.Culture> PatchWeaponSlotsOverflowCultures { get { return patchWeaponSlotsOverflowCultures.ToList(); } set { patchWeaponSlotsOverflowCultures = value.ToHashSet(); } }
    [GameplaySafe]
    public int FiringPreviewRecalcTrottle { get; set; } = 500;
    [GameplaySafe]
    public int SelectionStateMoveBaseProcessMousePosTrottle { get; set; } = 4;
    [GameplaySafe]
    public int UpdateReticleTrottle { get; set; } = 8;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("BloodSettings structure")]
    [GameplaySafe]
    public BloodSettings bloodSettings { get; set; } = new BloodSettings();
    public bool fixPrewarmRequests { get; set; } = true;
    [SkipDocumentation, JsonIgnore]
    public string directory { get; set; }
    [GameplaySafe]
    public ShowMissBehavior showMissBehavior { get; set; } = ShowMissBehavior.Default;
    public bool extendedBraceBehavior { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<string>\": { AoEModifiers structure } }")]
    public Dictionary<string, AoEModifiers> TagAoEDamageMult { get; set; } = new Dictionary<string, AoEModifiers>();
    [JsonIgnore]
    private Dictionary<UnitType, AoEModifiers> FDefaultAoEDamageMult = new Dictionary<UnitType, AoEModifiers>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<UnitType enum>\": { AoEModifiers structure } }")]
    public Dictionary<UnitType, AoEModifiers> DefaultAoEDamageMult {
      get { return FDefaultAoEDamageMult; }
      set {
        //Log.M.TWL(0, "set DefaultAoEDamageMult");
        foreach (var val in value) {
          //Log.M.WL(1, val.Key.ToString() + " = {range:" + val.Value.Range + " damage:" + val.Value.Damage + "}");
          FDefaultAoEDamageMult[val.Key] = val.Value;
        }
      }
    }
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    [GameplaySafe]
    public List<string> screamsIds { get; set; } = new List<string>();
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    [GameplaySafe]
    public List<string> uiIcons { get; set; } = new List<string>();
    public bool NullifyDestoryedLocationDamage { get; set; } = true;
    public bool DestoryedLocationDamageTransferStructure { get; set; } = true;
    public bool DestoryedLocationCriticalAllow { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> TransferHeatDamageToNormalTag { get; set; } = new List<string>();
    [GameplaySafe]
    public float WeaponPanelBackWidthScale { get; set; } = 1.1f;
    [GameplaySafe]
    public float WeaponPanelHeightScale { get; set; } = 1f;
    [GameplaySafe]
    public float WeaponPanelWidthScale { get; set; } = 1f;
    [GameplaySafe]
    public float OrderButtonWidthScale { get; set; } = 0.5f;
    [GameplaySafe]
    public float OrderButtonPaddingScale { get; set; } = 0.3f;
    public float AttackSequenceTimeout { get; set; } = 60f;
    [GameplaySafe]
    public bool SidePanelInfoSelfExternal { get; set; } = false;
    [GameplaySafe]
    public bool SidePanelInfoTargetExternal { get; set; } = false;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> MechHasNoStabilityTag { get; set; } = new List<string>();
    [GameplaySafe]
    public bool InfoPanelDefaultState { get; set; } = false;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "XLSX attack log")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Логи аттак в XLSX")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Saves attacks info to XLSX files. Location Mods/Core/CustomAmmoCategories/AttacksLogs/")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Сохраняет информацию об атаках в XLSX файлах. Путь Mods/Core/CustomAmmoCategories/AttacksLogs/")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool AttackLogWrite { get; set; } = false;
    public bool ShowAttackGroundButton { get; set; } = false;
    public bool ShowWeaponOrderButtons { get; set; } = false;
    public float ToHitSelfJumped { get; set; } = 2f;
    public float ToHitMechFromFront { get; set; } = 0f;
    public float ToHitMechFromSide { get; set; } = -1f;
    public float ToHitMechFromRear { get; set; } = -2f;
    public float ToHitVehicleFromFront { get; set; } = 0f;
    public float ToHitVehicleFromSide { get; set; } = -1f;
    public float ToHitVehicleFromRear { get; set; } = -2f;
    public string MinefieldDetectorStatName { get; set; } = "MinefieldDetection";
    public string MinefieldIFFStatName { get; set; } = "MinefieldIFF";
    [GameplaySafe]
    public bool AmmoNameInSidePanel { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show apply heatsinks message")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Показывать сообщение о радиаторах")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show apply heatsinks message")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывать ли сообщение о применении радиаторов")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool ShowApplyHeatSinkMessage { get; set; } = true;
    [GameplaySafe]
    public string ApplyHeatSinkMessageTemplate { get; set; } = "APPLY HEAT SINKS:{0}=>{1} HCAP:{1} USED:{2}=>{3}";
    [GameplaySafe]
    public string ResetHeatSinkMessageTemplate { get; set; } = "USED HEAT SINKS:{0}=>{1}";
    public string ApplyHeatSinkActorStat { get; set; } = "CACOverrallHeatSinked";
    public string OverrallShootsCountWeaponStat { get; set; } = "CACOverallShoots";
    public bool AmmoGenericBoxUINameAsName { get; set; } = true;
    [GameplaySafe]
    public bool NoSVGCacheClear { get; set; } = true;
    [GameplaySafe]
    public bool AMSCantFireFloatie { get; set; } = false;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show jamm chance formula")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Показывать формулу клина")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show jamm chance formula")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывать ли точный расчет шанса оружия к заклиниванию")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool ShowJammChance { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show evasive as number")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Уклонение -> число")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show evasive as number instead of pips")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывать ли токиены уклонения в виде числа, полезно при большом уклонении, что бы не считать иконки")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool ShowEvasiveAsNumber { get; set; } = true;
    [GameplaySafe]
    public float EvasiveNumberFontSize { get; set; } = 24f;
    [GameplaySafe]
    public float EvasiveNumberWidth { get; set; } = 20f;
    [GameplaySafe]
    public float EvasiveNumberHeight { get; set; } = 25f;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("list of strings")]
    public List<string> RemoveToHitModifiers { get; set; } = new List<string>();
    public bool ImprovedBallisticByDefault { get; set; } = true;
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary of { \"<string>\": { DesignMaskMoveCostInfo structure } }")]
    public Dictionary<string, DesignMaskMoveCostInfo> DefaultMoveCosts { get; set; } = new Dictionary<string, DesignMaskMoveCostInfo>();
    public bool DestroyedLocationsCritTransfer { get; set; } = false;
    public string OnlineServerHost { get; set; } = "192.168.78.162";
    public int OnlineServerServicePort { get; set; } = 143;
    public int OnlineServerDataPort { get; set; } = 443;
    public bool AoECanCrit { get; set; } = false;
    public void ApplyLocal(Settings local) {
      PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      Log.M.TWL(0, "Settings.ApplyLocal");
      foreach(PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach(object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip) { continue; }
        Log.M.WL(1, "updating:"+prop.Name);
        prop.SetValue(CustomAmmoCategories.Settings, prop.GetValue(local));
      }
    }
    public string SerializeLocal() {
      Log.M.TWL(0, "Settings.SerializeLocal");
      JObject json = JObject.FromObject(this);
      PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach (object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip) {
          if (json[prop.Name] != null) {
            json.Remove(prop.Name);
          }
        }
      }
      return json.ToString(Formatting.Indented);
    }
    [GameplaySafe]
    public bool AIPathingOptimization { get; set; } = true;
    [GameplaySafe]
    public bool AIPathingMultithread { get { return false; } set { } }
    public int AIPathingSamplesLimit { get; set; } = 120;
    public int AIPathingSamplesTreshold { get; set; } = 200;
    public float AIMinefieldDamageTreshold { get; set; } = 0.1f;
    public float AIMinefieldIgnoreMoveDistance { get; set; } = 36f;
    public float AIMinefieldIgnorePosDelta { get; set; } = 5f;
    public bool AIMinefieldAware { get; set; } = true;
    public bool AIMinefieldAwareAllMines { get; set; } = false;
    public bool AIMinefieldAwareAllMinesCritical { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Objectives black background")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Затемнение фона приказов")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Makes objectives background black on mouse hover")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "В бою затемняет фон приказов при наведении курсора. Послезно при сильном тумане")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool ObjectiveBlackBackgroundOnEnter { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Enable minimap")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Миникарта")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Enable minimap")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывает миникарту")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool EnableMinimap { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show regions on minimap")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Показывать регионы на миникарте")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show regions on minimap. Evacuate zones, enemies drop zones, events trigger zones etc")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывает области на миникарте. Зоны эвакуации, зоны приземления противиников, зоны которые приказано занять и т.д.")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool MinimapShowRegions { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show objectives on minimap")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Показывать цели на миникарте")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show objectives on minimap")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывать цели на миникарте")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool MinimapShowObjectives { get; set; } = true;
    [GameplaySafe]
    public string MinimapBurnigTerrainColor { get; set; } = "#FF9700FF";
    [GameplaySafe]
    public string MinimapBurnedTerrainColor { get; set; } = "#FFFFFFFF";
    [GameplaySafe]
    public Dictionary<TerrainMaskFlags, string> MinimapTerrainColors { get; set; } = new Dictionary<TerrainMaskFlags, string>() {
            { TerrainMaskFlags.Water, "#24D3D6FF" },
            { TerrainMaskFlags.DeepWater, "#2475d6FF" },
            { TerrainMaskFlags.Forest, "#73a573FF" },
            { TerrainMaskFlags.Road, "#636363FF" },
            { TerrainMaskFlags.Rough, "#da8923FF" },
            { TerrainMaskFlags.Custom, "#6b06a5FF" },
            { TerrainMaskFlags.Impassable, "#920000FF" },
            { TerrainMaskFlags.DropshipLandingZone, "#920000FF" },
            { TerrainMaskFlags.DangerousLocation, "#920000FF" },
            { TerrainMaskFlags.DropPodLandingZone, "#920000FF" },
            { TerrainMaskFlags.None, "#877931FF" },
            { TerrainMaskFlags.DestroyedBuilding, "#877931FF" },
            { TerrainMaskFlags.UseTerrain, "#877931FF" }
    };
    public bool PlayerAlwaysHit { get; set; } = false;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Result screen statistic")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Доп. статистика боя")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Show additional battle statistic on result screen. Hover \"Kills\" text")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывает дополнительную статистику на экране результатов боя. Статистика показывается при наведении на надпись показывающую кол-во убийств совершенных данной боевой единицой")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool StatisticOnResultScreenEnabled { get; set; } = true;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Show real kill icons")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Показывать реальные иконки убитых")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Instead of generic icons for killed units on battle result screen their real icons will be shown")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Показывать реальные иконки уничтоженой техники на экране результатов боя")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool StatisticOnResultScreenRealIcons { get; set; } = false;
    [GameplaySafe]
    public float StatisticOnResultScreenRealIconsScale { get; set; } = 1.5f;
    [GameplaySafe]
    public string StatisticOnResultScreenTurretSprite { get; set; } = "turret_unit_result_stat";
    [GameplaySafe]
    public string StatisticOnResultScreenBattleArmorSprite { get; set; } = "battle_armor_unit_result_stat";
    public bool SpawnProtectionAffectsAOE { get; set; } = true;
    public bool SpawnProtectionAffectsMinelayers { get; set; } = true;
    public bool SpawnProtectionAffectsDesignMasks { get; set; } = true;
    public bool SpawnProtectionAffectsBurningTerrain { get; set; } = true;
    public bool SpawnProtectionAffectsCanFire { get; set; } = true;
    [SelfDocumentationDefaultValue("{\"regionDef_HostileDropZone\"}"), SelfDocumentationTypeName("list of strings")]
    public HashSet<string> hidableRegionsList { get; set; } = new HashSet<string>() { "regionDef_HostileDropZone" };
    public bool PlayerAlwaysCalledShot { get; set; } = false;
    public AttackDirection PlayerAlwaysCalledShotDirection { get; set; } = AttackDirection.None;
    public float FleeStartTonage { get; set; } = 2f;
    public float FleeMaxTonage { get; set; } = 3f;
    public float HexSizeForMods { get; set; } = 30f;
    public bool RestoreEjectedWeapons { get; set; } = true;
    public string MapOnlineClientLink { get; set; } = "http://www.roguewar.org/playerlut?cId={0}";
    public bool MapOnlineClientDrawWidget { get; set; } = true;
    public string BuildinBurningFX { get; set; } = "vfxPrfPrtl_fireTerrain_lrgLoop";
    public float BuildinBurningScaleX { get; set; } = 1.2f;
    public float BuildinBurningScaleY { get; set; } = 1.2f;
    public float BuildinBurningScaleZ { get; set; } = 1.2f;
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Remove smoke on fire VFX")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Удалить дым от подгорания")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Remove smoke from build-in fire VFX, used only in build-in fire VFX is forced")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Удалить дым с визуального эффекта подгорания. Применяется только при принудительном втроенном эффекте подгорания")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool BuildinBurningFXDisableSmoke { get; set; } = false;
    public HashSet<string> BuildinBurningFXSmokeObjects { get; set; } = new HashSet<string>() { "smoke" };
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Force build-in fire VFX")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Принудительно встроенный эффект подгорания")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Make build-in fire VFX be forced regardles other settings (in launcher)")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Принудительно использовать встроенный визуальный эффект подгорания, вне зависимости от других настроек (например в лаунчере)")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool ForceBuildinBurningFX { get; set; } = false;
    public float AIUnsafeJamChance { get; set; } = 1f;
    public float ScaleWeaponHeat { get; set; } = 150f;
    public int ExternalHeatLimit { get; set; } = 149;
    public bool AMSUseAttractiveness { get; set; } = true;
    public int AMSDefaultInterceptedTrace { get; set; } = 2;
    public bool PhysicsAoE_Weapons { get; set; } = true;
    public bool PhysicsAoE_Deffered { get; set; } = true;
    public bool PhysicsAoE_Minefield { get; set; } = true;
    public bool PhysicsAoE_API { get; set; } = true;
    public float PhysicsAoE_API_Height { get; set; } = 10f;
    public bool AIAwareArtillery { get; set; } = true;
    public bool AIAwareMinefields { get; set; } = true;
    public bool AIFastUnitsOptimization { get; set; } = true;
    public string WeaponUseAmmoInstalledLocationTag { get; set; } = "ammo_installed_location_only";
    public string WeaponUseAmmoAdjacentLocationTag { get; set; } = "ammo_adjacent_location_only";
    public Settings() {
      //directory = string.Empty;
      //debugLog = true;
      //modHTTPServer = true;
      //forbiddenRangeEnable = true;
      //Joke = false;
      //AmmoCanBeExhausted = true;
      //ClusterAIMult = 0.2f;
      //PenetrateAIMult = 0.4f;
      //JamAIAvoid = 1.0f;
      //DamageJamAIAvoid = 2.0f;
      //WeaponRealizerStandalone = "";
      //OnlineServerHost = "192.168.78.162";
      //OnlineServerServicePort = 143;
      //OnlineServerDataPort = 443;
      //modHTTPListen = "http://localhost:65080";
      //DynamicDesignMasksDefs = new List<string>();
      //BurningForestDesignMask = "DesignMaskBurningForest";
      //BurnedForestDesignMask = "DesignMaskBurnedForest";
      //BurningTerrainDesignMask = "DesignMaskBurningTerrain";
      //BurningForestCellRadius = 3;
      //BurningForestTurns = 3;
      //BurningForestStrength = 5;
      //BurningForestBaseExpandChance = 0.5f;
      //BurningFX = "vfxPrfPrtl_fireTerrain_lrgLoop";
      //BurnedFX = "vfxPrfPrtl_miningSmokePlume_lrg_loop";
      //BurningScaleX = 1f;
      //BurningScaleY = 1f;
      //BurningScaleZ = 1f;
      //BurnedScaleX = 1f;
      //BurnedScaleY = 1f;
      //BurnedScaleZ = 1f;
      //BurnedOffsetX = 0f;
      //BurnedOffsetY = 0f;
      //BurnedOffsetZ = 0f;
      //BurningOffsetX = 0f;
      //BurningOffsetY = 0f;
      //BurningOffsetZ = 0f;
      //AttackSequenceMaxLength = 15000;
      //AdditinalAssets = new List<string>();
      //DontShowNotDangerouceJammMessages = false;
      //NoForestBiomes = new List<string>();
      //ForestBurningDurationBiomeMult = new Dictionary<string, float>();
      //WeaponBurningDurationBiomeMult = new Dictionary<string, float>();
      //ForestBurningStrengthBiomeMult = new Dictionary<string, float>();
      //WeaponBurningStrengthBiomeMult = new Dictionary<string, float>();
      //LitFireChanceBiomeMult = new Dictionary<string, float>();
      //MineFieldPathingMods = new Dictionary<string, float>();
      //JumpLandingMineAttractRadius = 2;
      //BurnedTrees = new BurnedTreesSettings();
      //DontShowBurnedTrees = false;
      //DontShowScorchTerrain = false;
      //AIPeerToPeerNodeEnabled = false;
      //AIPeerToPeerFirewallPierceThrough = false;
      //AAMSAICoeff = 0.2f;
      //WeaponRealizerSettings = "WeaponRealizerSettings.json";
      //WeaponRealizerStandalone = "WeaponRealizer.dll";
      //AIMStandalone = "AttackImprovementMod.dll";
      //AmmoCookoff = new AmmoCookoffSettings();
      //DontShowBurnedTreesTemporary = false;
      ////WaterHeightFix = true;
      //TerrainFiendlyFireRadius = 10f;
      //AdvancedCirtProcessing = true;
      //DestroyedComponentsCritTrap = true;
      //CritLocationTransfer = true;
      //APMinCritChance = 0.1f;
      //RemoveFromCritRollStatName = "IgnoreDamage";
      //FNoCritFloatieMessage = TripleBoolean.NotSet;
      //MechEngineerDetected = TripleBoolean.NotSet;
      //SpawnMenuEnabled = false;
      //patchWeaponSlotsOverflowCultures = new HashSet<Strings.Culture>();
      //FiringPreviewRecalcTrottle = 500;
      //SelectionStateMoveBaseProcessMousePosTrottle = 4;
      //UpdateReticleTrottle = 8;
      //bloodSettings = new BloodSettings();
      //fixPrewarmRequests = true;
      //showMissBehavior = ShowMissBehavior.Default;
      //extendedBraceBehavior = true;
      FDefaultAoEDamageMult = new Dictionary<UnitType, AoEModifiers>();
      foreach (UnitType t in Enum.GetValues(typeof(UnitType))) {
        FDefaultAoEDamageMult[t] = new AoEModifiers();
      }
      FDefaultAoEDamageMult[UnitType.Building].Range = 1.5f;
      FDefaultAoEDamageMult[UnitType.Building].Damage = 5f;
      //screamsIds = new List<string>();
      //TagAoEDamageMult = new Dictionary<string, AoEModifiers>();
      //uiIcons = new List<string>();
      //NullifyDestoryedLocationDamage = true;
      //DestoryedLocationDamageTransferStructure = true;
      //DestoryedLocationCriticalAllow = true;
      //TransferHeatDamageToNormalTag = new List<string>();
      //WeaponPanelBackWidthScale = 1.1f;
      //OrderButtonWidthScale = 0.5f;
      //OrderButtonPaddingScale = 0.3f;
      //AttackSequenceTimeout = 60f;
      //SidePanelInfoSelfExternal = false;
      //MechHasNoStabilityTag = new List<string>();
      //InfoPanelDefaultState = false;
      //AttackLogWrite = false;
      //ShowAttackGroundButton = false;
      //ShowWeaponOrderButtons = false;
      //ToHitSelfJumped = 2f;
      //ToHitMechFromFront = 0f;
      //ToHitMechFromSide = -1f;
      //ToHitMechFromRear = -2f;
      //ToHitVehicleFromFront = 0f;
      //ToHitVehicleFromSide = -1f;
      //ToHitVehicleFromRear = -2f;
      //WeaponPanelHeightScale = 1f;
      //MinefieldDetectorStatName = "MinefieldDetection";
      //MinefieldIFFStatName = "MinefieldIFF";
      //AmmoNameInSidePanel = true;
      //ShowApplyHeatSinkMessage = true;
      //ResetHeatSinkMessageTemplate = "USED HEAT SINKS:{0}=>{1}";
      //ApplyHeatSinkMessageTemplate = "APPLY HEAT SINKS:{0}=>{1} HCAP:{1} USED:{2}=>{3}";
      //ApplyHeatSinkActorStat = "CACOverrallHeatSinked";
      //OverrallShootsCountWeaponStat = "CACOverallShoots";
      //AmmoGenericBoxUINameAsName = true;
      //NoSVGCacheClear = true;
      //AMSCantFireFloatie = false;
      //ShowJammChance = true;
      //ShowEvasiveAsNumber = true;
      //EvasiveNumberFontSize = 24f;
      //EvasiveNumberWidth = 20f;
      //EvasiveNumberHeight = 25f;
      //RemoveToHitModifiers = new List<string>();
      //ImprovedBallisticByDefault = true;
      //DefaultMoveCosts = new Dictionary<string, DesignMaskMoveCostInfo>();
      //DestroyedLocationsCritTransfer = false;
    }
  }
}