using BattleTech;
using BattleTech.Data;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using Harmony;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomUnits{
  public static class Log {
    //private static string m_assemblyFile;
    private static string m_logfile;
    private static readonly Mutex mutex = new Mutex();
    public static string BaseDirectory;
    private static StringBuilder m_cache = new StringBuilder();
    private static StreamWriter m_fs = null;
    private static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        Log.LogWrite("flush\n");
        Log.flush();
      }
    }
    public static void InitLog() {
      Log.m_logfile = Path.Combine(BaseDirectory, "CustomUnits.log");
      File.Delete(Log.m_logfile);
      Log.m_fs = new StreamWriter(Log.m_logfile);
      Log.m_fs.AutoFlush = true;
      Log.flushThread.Start();
    }
    public static void flush() {
      if (Log.mutex.WaitOne(1000)) {
        Log.m_fs.Write(Log.m_cache.ToString());
        Log.m_fs.Flush();
        Log.m_cache.Length = 0;
        Log.mutex.ReleaseMutex();
      }
    }
    public static void LogWrite(int initiation,string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n", isCritical);
      } else {
        LogWrite(prefix + line, isCritical);
      }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      //try {
      if ((Core.Settings.debugLog) || (isCritical)) {
        if (Log.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          //File.AppendAllText(Log.m_logfile, line);
          Log.mutex.ReleaseMutex();
        }
        if (isCritical) { Log.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
      }
      //} catch (Exception) {
      //i'm sertanly don't know what to do
      //}
    }
    public static void W(string line, bool isCritical = false) {
      LogWrite(line, isCritical);
    }
    public static void WL(string line, bool isCritical = false) {
      line += "\n"; W(line, isCritical);
    }
    public static void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; W(line, isCritical);
    }
    public static void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; WL(line, isCritical);
    }
    public static void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line, isCritical);
    }
    public static void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line, isCritical);
    }
  }
  public class ComponentPrefabMap {
    public string PrefabIdentifier { get; set; }
    public Dictionary<string,float> HardpointCandidates { get; set; }
    public ComponentPrefabMap() { PrefabIdentifier = string.Empty; HardpointCandidates = new Dictionary<string, float>(); }
  }
  public class CUSettings {
    public string CanPilotVehicleTag { get; set; }
    public string CannotPilotMechTag { get; set; }
    public bool debugLog { get; set; }
    public float DeathHeight { get; set; }
    public bool fixWaterHeight { get; set; }
    public float maxWaterSteepness { get; set; }
    public float deepWaterSteepness { get; set; }
    public float deepWaterDepth { get; set; }
    public float waterFlatDepth { get; set; }
    public List<string> LancesIcons { get; set; }
    public List<CustomLanceDef> Lances { get; set; }
    public int overallDeploySize { get; set; }
    public string EMPLOYER_LANCE_GUID { get; set; }
    public float CanPilotVehicleProbability { get; set; }
    public float CanPilotAlsoMechProbability { get; set; }
    public int MaxVehicleRandomPilots { get; set; }
    public string CanPilotVehicleDescription { get; set; }
    public string CanPilotBothDescription { get; set; }
    public bool ShowVehicleBays { get; set; }
    public int ArgoBaysFix { get; set; }
    public bool BaysCountExternalControl { get; set; }
    public bool CountContractMaxUnits4AsUnlimited { get; set; }
    public string PlayerControlConvoyTag { get; set; }
    public string ConvoyRouteTag { get; set; }
    public int ConvoyUnitsCount { get; set; }
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
    private HashSet<string> fManualDeployForbidContractTypes { get; set; }
    public List<string> ManualDeployForbidContractTypes { set {
        if (fManualDeployForbidContractTypes == null) { fManualDeployForbidContractTypes = new HashSet<string>(); }
        fManualDeployForbidContractTypes.Clear();
        foreach (string ct in value) {
          fManualDeployForbidContractTypes.Add(ct);
        }
      } }
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
    public SortByTonnage.Settings SortBy { get; set; }
    public string PilotingIcon { get; set; }
    public string DefaultSkirmishDropLayout { get; set; }
    public string DefaultSimgameDropLayout { get; set; }
    public List<string> vehicleForcedTags { get; set; }
    public List<string> mechForcedTags { get; set; }
    public List<string> squadForcedTags { get; set; }
    public List<string> navalForcedTags { get; set; }
    public int baysWidgetsCount { get; set; }
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
      EMPLOYER_LANCE_GUID = "ecc8d4f2-74b4-465d-adf6-84445e5dfc230";
      CountContractMaxUnits4AsUnlimited = false;
      PlayerControlConvoyTag = "convoy_player_control";
      TransferTeamOnDeathPrefixTag = "transfer-on-death:";
      ConvoyDenyMoveTag = "convoy_palyer_control_deny_move";
      ConvoyUnitsCount = 4;
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
      vehicleForcedTags = new List<string>() { "unit_drop_type_generic_vehicle", "unit_piloting_type_generic_vehicle", "vehicle_bays" };
      mechForcedTags = new List<string>() { "unit_drop_type_generic_mech", "unit_piloting_type_generic_mech" };
      squadForcedTags = new List<string>() { "battle_armor_bays" };
      navalForcedTags = new List<string>() { "unit_naval" };
      baysWidgetsCount = 4;
      //HardpointFix = new Features.HardpointFix.HardpointFixSettings();
    }
  }
  public static partial class Core {
    public static readonly float Epsilon = 0.001f;
    public static CUSettings Settings;
    public static string BaseDir { get; private set; }
    public static float TypeDmgCACModifier(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      ExtWeaponDef def = weapon.exDef();
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      if(target is BattleTech.Building) {
        return def.BuildingsDamageModifier * ammo.BuildingsDamageModifier * mode.BuildingsDamageModifier;
      }
      if (target is Turret) {
        return def.TurretDamageModifier * ammo.TurretDamageModifier * mode.TurretDamageModifier;
      }
      if (target is Vehicle vehicle) {
        UnitCustomInfo info = vehicle.GetCustomInfo();
        if (info != null) {
          if ((info.AOEHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) && (vehicle.UnaffectedPathing())) {
            return def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier * def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier;
          }
        }
        return def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier;
      }
      if (target is TrooperSquad squad) {
        return def.TrooperSquadDamageModifier * ammo.TrooperSquadDamageModifier * mode.TrooperSquadDamageModifier;
      }
      if (target.GameRep != null) {
        AlternateMechRepresentations altReps = target.GameRep.GetComponent<AlternateMechRepresentations>();
        if(altReps != null) {
          if (altReps.isHovering) {
            return def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier * def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
          }
        }
        QuadRepresentation quadRep = target.GameRep.GetComponent<QuadRepresentation>();
        if (quadRep != null) {
          return def.QuadDamageModifier * ammo.QuadDamageModifier * mode.QuadDamageModifier * def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
        }
      }
      if(target is Mech mech) {
        return def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
      }
      return 1f;
    }
    public static string TypeDmgCACModifierName(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      if (target is BattleTech.Building) {
        return "Building (x"+Math.Round(TypeDmgCACModifier(weapon,attackPosition,target,IsBreachingShot,location,dmg,ap,heat,stab), 1)+")";
      }
      if (target is Turret) {
        return "Turret (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")";
      }
      if (target is Vehicle vehicle) {
        UnitCustomInfo info = vehicle.GetCustomInfo();
        if (info != null) {
          if ((info.AOEHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) && (vehicle.UnaffectedPathing())) {
            return "VTOL; Vehicle (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
          }
        }
        return "Vehicle (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      }
      if (target is TrooperSquad squad) {
        return "Trooper squad (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      }
      if (target.GameRep != null) {
        AlternateMechRepresentations altReps = target.GameRep.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) {
          if (altReps.isHovering) {
            return "AirMech; Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
          }
        }
        QuadRepresentation quadRep = target.GameRep.GetComponent<QuadRepresentation>();
        if (quadRep != null) {
          return "Quad; Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
        }
      }
      if (target is Mech mech) {
        return "Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      }
      return "Target type (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
    }
    public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources) {
      Log.TWL(0, "FinishedLoading", true);
      IRBTModUtils.Extension.MechExtensions.RegisterMoveDistanceModifier("CustomUnits", 10, Mech_MaxWalkDistance.MaxWalkDistanceMod, Mech_MaxWalkDistance.MaxSprintDistanceMod);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.ChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.VehicleChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.HardpointDataDef, CustomHardPointsHelper.CustomHardpoints);
      CustomPrewarm.Core.RegisterSerializator("CustomUnitsChassisTags", BattleTechResourceType.VehicleChassisDef, BattleTech_VehicleChassisDef_fromJSON_Patch.serializeChassisTags);
      try {
        foreach(string name in loadOrder) { if (name == "Mission Control") { CustomLanceHelper.MissionControlDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "MechEngineer") { Core.Settings.MechEngineerDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "LowVisibility") { LowVisibilityAPIHelper.Init(); break; }; }
        foreach (var customResource in customResources) {
          Log.TWL(0, "customResource:"+ customResource.Key);
          if (customResource.Key == "CustomMechRepresentationDef") {
            foreach (var custMechRep in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custMechRep.Value.FilePath);
                CustomMechRepresentationDef mechRepDef = JsonConvert.DeserializeObject<CustomMechRepresentationDef>(File.ReadAllText(custMechRep.Value.FilePath));
                mechRepDef.Register();
              } catch (Exception e) {
                Log.TWL(0, custMechRep.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "CustomPrefabDef") {
            foreach (var custMechRep in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custMechRep.Value.FilePath);
                CustomPrefabDef mechRepDef = JsonConvert.DeserializeObject<CustomPrefabDef>(File.ReadAllText(custMechRep.Value.FilePath));
                mechRepDef.Register();
              } catch (Exception e) {
                Log.TWL(0, custMechRep.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                DropSlotDef def = JsonConvert.DeserializeObject<DropSlotDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropLanceDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                DropLanceDef def = JsonConvert.DeserializeObject<DropLanceDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotsDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                DropSlotsDef def = JsonConvert.DeserializeObject<DropSlotsDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotDecorationDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                DropSlotDecorationDef def = JsonConvert.DeserializeObject<DropSlotDecorationDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropClassDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                DropClassDef def = JsonConvert.DeserializeObject<DropClassDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "PilotingClassDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                PilotingClassDef def = JsonConvert.DeserializeObject<PilotingClassDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == nameof(CustomHangarDef)) {
            foreach (var custItem in customResource.Value) {
              try {
                Log.WL(1, "Path:" + custItem.Value.FilePath);
                CustomHangarDef def = JsonConvert.DeserializeObject<CustomHangarDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.WL(1, "id:" + def.Description.Id);
                Log.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.TWL(0, custItem.Key, true);
                Log.WL(0, e.ToString(), true);
              }
            }
          } else {
            throw new Exception("Unknown resource "+ customResource.Key);
          }
        }
        CustAmmoCategories.ToHitModifiersHelper.registerModifier("SQUAD SIZE", "SQUAD SIZE", true, false, TrooperSquad.GetSquadSizeToHitMod, TrooperSquad.GetSquadSizeToHitModName);
        CustAmmoCategories.DamageModifiersCache.RegisterDamageModifier("SQUAD SIZE", "SQUAD SIZE", true, true, true, true, true, TrooperSquad.SquadSizeDamageMod, TrooperSquad.SquadSizeDamageModName);
        DamageModifiersCache.RegisterDamageModifier("TYPEMOD", "TYPEMOD", false, true, true, true, true, TypeDmgCACModifier, TypeDmgCACModifierName);
        CustAmmoCategories.DeferredEffectHelper.RegisterCallback("HOTDROP",HotDropManager.DefferedHotDrop);
        PilotingClassHelper.Validate();
        //DropClassDef.Validate();
        DropSystemHelper.Validate();
        Core.HarmonyInstance.Patch(typeof(Mech).GetMethod("InitGameRep", BindingFlags.Public | BindingFlags.Instance),new HarmonyMethod(typeof(CustomMech).GetMethod(nameof(CustomMech.InitGameRepStatic), BindingFlags.Static | BindingFlags.Public)));
        Log.TWL(0, "Harmony log Mech.InitGameRep");
        Patches patches = Core.HarmonyInstance.GetPatchInfo(typeof(Mech).GetMethod("InitGameRep", BindingFlags.Public | BindingFlags.Instance));
        Log.WL(1, "Prefixes:");
        foreach (var patch in patches.Prefixes) {
          Log.WL(2, patch.owner+" index:"+patch.index+" method:"+patch.patch.Name);
        }
        Log.WL(1, "Postfixes:");
        foreach (var patch in patches.Postfixes) {
          Log.WL(2, patch.owner + " index:" + patch.index + " method:" + patch.patch.Name);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static HarmonyInstance HarmonyInstance = null;
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Core.BaseDir = directory;
      Core.Settings = JsonConvert.DeserializeObject<CustomUnits.CUSettings>(settingsJson);
      //PilotingClass.Validate();
      Log.LogWrite("Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      //InitLancesLoadoutDefault();
      //CustomLanceHelper.BaysCount(3+(Core.Settings.BaysCountExternalControl?0:Core.Settings.ArgoBaysFix));
      MechResizer.MechResizer.Init(directory, settingsJson);
      SortByTonnage.SortByTonnage.Init(directory, Core.Settings.SortBy);

      try {
        HarmonyInstance = HarmonyInstance.Create("io.mission.customunits");
        HitLocation_GetMechHitTableCustom.i_GetMechHitTable = HitLocation_GetMechHitTable.Get;
        /*Type AssetBundleTracker = typeof(WeaponEffect).Assembly.GetType("BattleTech.Assetbundles.AssetBundleTracker");
        if (AssetBundleTracker != null) {
          MethodInfo BuildObjectMap = AssetBundleTracker.GetMethod("BuildObjectMap", BindingFlags.Instance | BindingFlags.NonPublic);
          if (BuildObjectMap != null) {
            MethodInfo patched = typeof(AssetBundleTracker_BuildObjectMap).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
            if(patched != null) {
              harmony.Patch(BuildObjectMap,null,new HarmonyMethod(patched));
              Log.TWL(0, "BattleTech.Assetbundles.AssetBundleTracker.BuildObjectMap patched");
            }
          } else {
            Log.TWL(0, "can't find BattleTech.Assetbundles.AssetBundleTracker.BuildObjectMap");
          }
        } else {
          Log.TWL(0, "can't find BattleTech.Assetbundles.AssetBundleTracker");
        }
        Type PrefabLoadRequest = typeof(DataManager).GetNestedType("PrefabLoadRequest", BindingFlags.NonPublic);
        if (AssetBundleTracker != null) {
          MethodInfo Load = PrefabLoadRequest.GetMethod("Load");
          MethodInfo patched = typeof(PrefabLoadRequest_Load).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
          if ((patched != null)&&(Load != null)) {
            harmony.Patch(Load, null, new HarmonyMethod(patched));
            Log.TWL(0, "PrefabLoadRequest.Load patched");
          }
        } else {
          Log.TWL(0, "can't find DataManager.PrefabLoadRequest");
        }*/
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        PathingInfoHelper.RegisterMaxMoveDeligate(PathingHelper.MaxMoveDistance);
        WeightedFactorHelper.PatchInfluenceMapPositionFactor(HarmonyInstance);
        WeaponRepresentation_PlayWeaponEffect.i_extendedFire = extendedFireHelper.extendedFire;
      } catch (Exception e) {
        Log.LogWrite(e.ToString()+"\n");
      }
    }
  }
}
