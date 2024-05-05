using BattleTech;
using CustAmmoCategories;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  public enum VehiclesEditState {
    Enabled,Disabled
  }
  public class CUSettings {
    public string CanPilotVehicleTag { get; set; }
    public string CannotPilotMechTag { get; set; }
    [GameplaySafe]
    public bool debugLog { get; set; }
    public float DeathHeight { get; set; }
    public bool fixWaterHeight { get; set; }
    public float maxWaterSteepness { get; set; }
    public float deepWaterSteepness { get; set; }
    public float deepWaterDepth { get; set; }
    public float waterFlatDepth { get; set; }
    [GameplaySafe]
    public List<string> LancesIcons { get; set; }
    public List<CustomLanceDef> Lances { get; set; }
    public int overallDeploySize { get; set; }
    public string EMPLOYER_LANCE_GUID { get; set; }
    public float CanPilotVehicleProbability { get; set; }
    public float CanPilotAlsoMechProbability { get; set; }
    public int MaxVehicleRandomPilots { get; set; }
    public string CanPilotVehicleDescription { get; set; }
    public string CanPilotBothDescription { get; set; }
    public bool GenerateExtraPilotingClassesForRonin { get; set; }
    public bool ShowVehicleBays { get; set; }
    public int ArgoBaysFix { get; set; }
    public bool BaysCountExternalControl { get; set; }
    public bool CountContractMaxUnits4AsUnlimited { get; set; }
    public string PlayerControlConvoyTag { get; set; }
    public string ConvoyRouteTag { get; set; }
    public int ConvoyUnitsCount { get; set; }
    public int PreferedLanceSize { get; set; } = 6;
    public string TransferTeamOnDeathPrefixTag { get; set; }
    public string ConvoyDenyMoveTag { get; set; }
    public float ConvoyMaxDistFromOther { get; set; }
    public float ConvoyMaxDistFromPlayer { get; set; }
    public float ConvoyMaxDistFromRoute { get; set; }
    public bool AllowVehiclesEdit { get; set; }
    public string MechBaySwitchIconMech { get; set; }
    public string MechBaySwitchIconVehicle { get; set; }
    public string MechBaySwitchIconUp { get; set; }
    public string MechBaySwitchIconDown { get; set; }
    public string ShowActiveAbilitiesIcon { get; set; }
    public string ShowPassiveAbilitiesIcon { get; set; }
    public string HideActiveAbilitiesIcon { get; set; }
    public string HidePassiveAbilitiesIcon { get; set; }
    public float DeploySpawnRadius { get; set; }
    public float DeployMaxDistanceFromOriginal { get; set; }
    public float DeployMinDistanceFromEnemy { get; set; }
    public float DeployMaxDistanceFromMates { get; set; }
    public float DeploySingleCircleRadius { get; set; }
    public float DeploySingleCircleRadiusTarget { get; set; }
    public float DeployLabelHeight { get; set; }
    public float DeployLabelFontSize { get; set; }
    public bool DeployManual { get; set; }
    public bool DeployManualSpawnProtection { get; set; } = true;
    public bool DeployAutoSpawnProtection { get; set; } = true;
    public bool OnUnitSpawnProtection { get; set; } = true;
    public bool AskForDeployManual { get; set; } = true;
    private HashSet<string> fManualDeployForbidContractTypes { get; set; }
    public List<string> ManualDeployForbidContractTypes {
      set {
        if (fManualDeployForbidContractTypes == null) { fManualDeployForbidContractTypes = new HashSet<string>(); }
        fManualDeployForbidContractTypes.Clear();
        foreach (string ct in value) {
          fManualDeployForbidContractTypes.Add(ct);
        }
      }
    }
    public HashSet<string> DeployForbidContractTypes { get { return fManualDeployForbidContractTypes; } }
    public bool DisableHotKeys { get; set; }
    public string SquadStructureIcon { get; set; }
    public string SquadArmorOutlineIcon { get; set; }
    public string SquadArmorIcon { get; set; }
    public string VehicleFrontStructureIcon { get; set; }
    public string VehicleFrontArmorOutlineIcon { get; set; }
    public string VehicleFrontArmorIcon { get; set; }
    public string VehicleRearStructureIcon { get; set; }
    public string VehicleRearArmorOutlineIcon { get; set; }
    public string VehicleRearArmorIcon { get; set; }
    public string VehicleLeftStructureIcon { get; set; }
    public string VehicleLeftArmorOutlineIcon { get; set; }
    public string VehicleLeftArmorIcon { get; set; }
    public string VehicleRightStructureIcon { get; set; }
    public string VehicleRightArmorOutlineIcon { get; set; }
    public string VehicleRightArmorIcon { get; set; }
    public string VehicleTurretStructureIcon { get; set; }
    public string VehicleTurretArmorOutlineIcon { get; set; }
    public string VehicleTurretArmorIcon { get; set; }
    public string ConvoyRouteBeaconVFX { get; set; }
    public CustomVector ConvoyRouteBeaconVFXScale { get; set; }
    public string CustomJumpJetsPrefabSrc { get; set; }
    public string CustomJetsStreamsPrefabSrc { get; set; } = "chrPrfVhcl_leopard";
    public string CustomJumpJetsComponentPrefab { get; set; }
    public string CustomJumpJetsPrefabSrcObjectName { get; set; }
    public string CustomHeadlightComponentPrefab { get; set; }
    public string CustomHeadlightPrefabSrcObjectName { get; set; }
    public string FullTransparentMaterialSource { get; set; }
    public float MaxHoveringHeightWithWorkingJets { get; set; }
    public float PartialMovementGuardDistance { get; set; }
    public bool PartialMovementOnlyWalkByDefault { get; set; }
    public bool AllowRotateWhileJumpByDefault { get; set; }
    public string MechIsVehicleTag { get; set; }
    public string DefaultMechBattleRepresentationPrefab { get; set; }
    public string DefaultMechSimgameRepresentationPrefab { get; set; }
    public CombatValueMultipliersDef? DefaultCombatValueMultipliers { get; set; }

    [JsonIgnore]
    public Dictionary<string, Dictionary<string, float>> weaponPrefabMappings { get; private set; }
    public ComponentPrefabMap[] WeaponPrefabMappings {
      set {
        this.weaponPrefabMappings = new Dictionary<string, Dictionary<string, float>>();
        foreach (var val in value) {
          foreach (var candidate in val.HardpointCandidates) {
            if (weaponPrefabMappings.TryGetValue(candidate.Key, out Dictionary<string, float> candidates) == false) {
              candidates = new Dictionary<string, float>();
              weaponPrefabMappings.Add(candidate.Key, candidates);
            }
            if (candidates.ContainsKey(val.PrefabIdentifier.ToLower())) {
              candidates[val.PrefabIdentifier] = candidate.Value;
            } else {
              candidates.Add(val.PrefabIdentifier.ToLower(), candidate.Value);
            }
          }
        }
      }
    }
    //public Features.HardpointFix.HardpointFixSettings HardpointFix { get; set; }
    [JsonIgnore]
    public bool LowVisDetected { get; set; }
    [JsonIgnore]
    public bool MechEngineerDetected { get; set; }
    [JsonIgnore]
    public bool CBTBEDetected { get; set; }
    public bool AllowPartialMove { get; set; }
    public bool AllowPartialSprint { get; set; }
    [GameplaySafe]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Sort by")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Сортировка по")]
    [CustomSettings.IsLocalSetting(false)]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, "Sort type for units in mechlab")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, "Тип сортировки для юнитов в мехлабе")]
    [NextSortByGenerator]
    public SortByTonnage.Settings SortBy { get; set; }
    public string PilotingIcon { get; set; }
    public string DefaultSkirmishDropLayout { get; set; }
    public string DefaultSimgameDropLayout { get; set; }
    public List<string> vehicleForcedTags { get; set; }
    public List<string> mechForcedTags { get; set; }
    public List<string> squadForcedTags { get; set; }
    public List<string> navalForcedTags { get; set; }
    [GameplaySafe]
    public int baysWidgetsCount { get; set; }
    public List<string> forcedLance { get; set; } = new List<string>();
    public string DefaultMeleeDefinition { get; set; } = "Weapon_MeleeAttack";
    public string DefaultDFADefinition { get; set; } = "Weapon_DFAAttack";
    public string DefaultAIImaginaryDefinition { get; set; } = "Weapon_Laser_AI_Imaginary";
    public HashSet<string> forceToBuildinShadersList { get; set; } = new HashSet<string>() { "Legacy Shaders/Particles/Alpha Blended Premultiply", "Mobile/Particles/Additive", "Particles/Alpha Blended" };
    public Dictionary<string, string> shadersReplacementList { get; set; } = new Dictionary<string, string>() { { "Mobile/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended Premultiply" } };
    [JsonIgnore]
    public Dictionary<string, Shader> forceToBuildinShaders { get; set; } = new Dictionary<string, Shader>();
    public string IntelCompanyStatShowMood { get; set; } = "Intel_Show_Mood";
    public string IntelCompanyStatShowMiniMap { get; set; } = "Intel_Show_Minimap";
    public bool IntelShowMood { get; set; } = false;
    public bool IntelShowMiniMap { get; set; } = false;
    public bool VehicleEquipmentIsFixed { get; set; } = true;
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_EN_US, "Vehicles editable")]
    [CustomSettings.LocalSettingName(Strings.Culture.CULTURE_RU_RU, "Танчики редактируемы")]
    [CustomSettings.IsLocalSetting(true)]
    [NextVehiclesEdit]
    [GameplaySafe]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_EN_US, typeof(ReducedComponentRefInfoHelper), "description")]
    [CustomSettings.LocalSettingDescription(Strings.Culture.CULTURE_RU_RU, typeof(ReducedComponentRefInfoHelper), "description")]
    [CustomSettings.LocalSettingValues(true, false)]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_EN_US, "Yes", "No")]
    [CustomSettings.LocalSettingValuesNames(Strings.Culture.CULTURE_RU_RU, "Да", "Нет")]
    public bool VehcilesPartialEditable { get; set; } = false;
    public HashSet<string> globalGameRepresenationAudioEventsSupress = new HashSet<string>();
    public Dictionary<string, TimerObjectiveAdvice> timerObjectiveChange { get; set; } = new Dictionary<string, TimerObjectiveAdvice>() { { "DefendBase", new TimerObjectiveAdvice(1, 2) } };
    public float CloseRangeFiringArc { get; set; } = 90f;
    public float CloseRangeFiringArcDistance { get; set; } = 40f;
    public string VehicleComponentForbiddenTag { get; set; } = "vehicle_forbidden";
    public string VehicleComponentOneAllowed { get; set; } = "vehicle_one_allowed";
    public string VehicleComponentOneAllowedLocation { get; set; } = "vehicle_one_allowed_location";
    public string VehicleComponentCategoryTagPrefix { get; set; } = "vehicle_component_category_";
    public bool PreserveJsonUnitCost { get; set; } = true;
    public bool RecalcUnitPartCost { get; set; } = true;
    public bool DEBUG_CanPilotEverything { get; set; } = false;
    public void ApplyLocal(CUSettings local) {
      PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      Log.M.TWL(0, "Settings.ApplyLocal");
      foreach (PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach (object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip) { continue; }
        Log.M.WL(1, "updating:" + prop.Name);
        prop.SetValue(Core.Settings, prop.GetValue(local));
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
    public CUSettings() {
      debugLog = false;
      DeathHeight = 1f;
      fixWaterHeight = true;
      maxWaterSteepness = 29f;
      deepWaterSteepness = 41f;
      deepWaterDepth = 10f;
      waterFlatDepth = 2f;
      LancesIcons = new List<string>();
      Lances = new List<CustomLanceDef>();
      overallDeploySize = 4;
      BaysCountExternalControl = false;
      ShowVehicleBays = false;
      ArgoBaysFix = 0;
      CanPilotVehicleTag = "pilot_vehicle_crew";
      CannotPilotMechTag = "pilot_nomech_crew";
      MaxVehicleRandomPilots = 0;
      CanPilotVehicleProbability = 0.0f;
      CanPilotAlsoMechProbability = 0.0f;
      CanPilotVehicleDescription = String.Format(HumanDescriptionDef.PilotGenDetailFormatting, "Piloting", "Can pilot only vehicles");
      CanPilotBothDescription = String.Format(HumanDescriptionDef.PilotGenDetailFormatting, "Piloting", "Can pilot both mechs and vehicles"); ;
      GenerateExtraPilotingClassesForRonin = true;
      EMPLOYER_LANCE_GUID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
      CountContractMaxUnits4AsUnlimited = false;
      PlayerControlConvoyTag = "convoy_player_control";
      TransferTeamOnDeathPrefixTag = "transfer-on-death:";
      ConvoyDenyMoveTag = "convoy_palyer_control_deny_move";
      ConvoyUnitsCount = 4;
      PreferedLanceSize = 6;
      ConvoyMaxDistFromOther = 200f;
      ConvoyMaxDistFromPlayer = 300f;
      ConvoyMaxDistFromRoute = 30f;
      ConvoyRouteTag = "escort_convoy";
      AllowVehiclesEdit = false;
      MechBaySwitchIconMech = "mech";
      MechBaySwitchIconVehicle = "vehicle";
      MechBaySwitchIconUp = "weapon_up";
      MechBaySwitchIconDown = "weapon_down";
      ShowActiveAbilitiesIcon = "";
      ShowPassiveAbilitiesIcon = "";
      HideActiveAbilitiesIcon = "";
      HidePassiveAbilitiesIcon = "";
      DeployManual = false;
      DeploySpawnRadius = 50f;
      DeployMaxDistanceFromOriginal = 30.0f;
      DeployMinDistanceFromEnemy = 300f;
      DeployMaxDistanceFromMates = 100f;
      DeploySingleCircleRadius = 5f;
      DeploySingleCircleRadiusTarget = 10f;
      DeployLabelHeight = 10f;
      DeployLabelFontSize = 32f;
      fManualDeployForbidContractTypes = new HashSet<string>();
      DisableHotKeys = false;
      SquadStructureIcon = "ba_structure";
      SquadArmorOutlineIcon = "ba_armor_outline";
      SquadArmorIcon = "ba_armor";
      VehicleFrontStructureIcon = "vehicle_readout_f_structure";
      VehicleFrontArmorOutlineIcon = "vehicle_readout_f_armor_outline";
      VehicleFrontArmorIcon = "vehicle_readout_f_armor";
      VehicleRearStructureIcon = "vehicle_readout_r_structure";
      VehicleRearArmorOutlineIcon = "vehicle_readout_r_armor_outline";
      VehicleRearArmorIcon = "vehicle_readout_r_armor";
      VehicleLeftStructureIcon = "vehicle_readout_l_structure";
      VehicleLeftArmorOutlineIcon = "vehicle_readout_l_armor_outline";
      VehicleLeftArmorIcon = "vehicle_readout_l_armor";
      VehicleRightStructureIcon = "vehicle_readout_ri_structure";
      VehicleRightArmorOutlineIcon = "vehicle_readout_ri_armor_outline";
      VehicleRightArmorIcon = "vehicle_readout_ri_armor";
      VehicleTurretStructureIcon = "vehicle_readout_t_structure";
      VehicleTurretArmorOutlineIcon = "vehicle_readout_t_armor_outline";
      VehicleTurretArmorIcon = "vehicle_readout_t_armor";
      ConvoyRouteBeaconVFX = "vfxPrfPrtl_artillerySmokeSignal_loop";
      ConvoyRouteBeaconVFXScale = new CustomVector(true);
      CustomJumpJetsPrefabSrc = "chrPrfMech_atlasBase-001";
      CustomJumpJetsPrefabSrcObjectName = "JumpJetPrefab (4)";
      CustomJumpJetsComponentPrefab = "chrPrfComp_atlas_leftleg_jumpjet";
      CustomHeadlightComponentPrefab = "chrPrfComp_atlas_leftshoulder_headlight";
      CustomHeadlightPrefabSrcObjectName = "PtLight - Torso (3)";
      FullTransparentMaterialSource = "full_transparent";
      DefaultMechBattleRepresentationPrefab = "chrPrfMech_atlasBase-001";
      DefaultMechSimgameRepresentationPrefab = "chrPrfComp_atlas_simgame";
      MaxHoveringHeightWithWorkingJets = 1f;
      PartialMovementGuardDistance = 15f;
      PartialMovementOnlyWalkByDefault = true;
      AllowRotateWhileJumpByDefault = true;
      MechIsVehicleTag = "unit_vehicle";
      WeaponPrefabMappings = new ComponentPrefabMap[] { };
      LowVisDetected = false;
      MechEngineerDetected = false;
      AllowPartialMove = true;
      AllowPartialSprint = true;
      SortBy = new SortByTonnage.Settings();
      PilotingIcon = "piloting";
      DefaultSkirmishDropLayout = "default_skirmish_layout";
      DefaultSimgameDropLayout = "default_simgame_layout";
      vehicleForcedTags = new List<string>() { "vehicle_bays" };
      //mechForcedTags = new List<string>() { "unit_drop_type_generic_mech", "unit_piloting_type_generic_mech" };
      mechForcedTags = new List<string>();
      squadForcedTags = new List<string>();
      navalForcedTags = new List<string>() { "unit_naval" };
      baysWidgetsCount = 4;
      //HardpointFix = new Features.HardpointFix.HardpointFixSettings();
    }
  }
}