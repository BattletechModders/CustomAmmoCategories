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
using BattleTech.Data;
using CustAmmoCategories;
using CustomAmmoCategoriesPatches;
using Harmony;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomUnits{
  public static class DLog {
    private static StringBuilder m_cache = new StringBuilder();
    public static void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        LogWrite(prefix + line + "\n");
      } else {
        LogWrite(prefix + line);
      }
    }
    public static void LogWrite(string line) {
      m_cache.Append(line);
    }
    public static void W(string line) {
      LogWrite(line);
    }
    public static void WL(string line) {
      line += "\n"; W(line);
    }
    public static void W(int initiation, string line) {
      string init = new string(' ', initiation);
      line = init + line; W(line);
    }
    public static void WL(int initiation, string line) {
      string init = new string(' ', initiation);
      line = init + line; WL(line);
    }
    public static void TW(int initiation, string line) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line);
    }
    public static void TWL(int initiation, string line) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line);
    }
    public static void Flush() {
      if (m_cache.Length > 0) { Log.LogWrite(m_cache.ToString(), false); m_cache.Clear(); }
    }
    public static void Skip() {
      m_cache.Clear();
    }
  }
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
      if ((Core.Settings.debugLog) || (isCritical)) {
        if (Log.mutex.WaitOne(1000)) {
          m_cache.Append(line);
          //File.AppendAllText(Log.m_logfile, line);
          Log.mutex.ReleaseMutex();
        }
        if (isCritical) { Log.flush(); };
        if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
      }
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
    public static Assembly MechEngineerAssembly = null;
    public static void MechDefMovementStatistics_GetJumpCapacity(object __instance,ref MechDef ___mechDef, ref float __result) {
      //Log.TWL(0, "MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics.GetJumpCapacity " + ___mechDef.Description.Id, true);
      UnitCustomInfo info = ___mechDef.GetCustomInfo();
      if (info != null) {
        if(info.SquadInfo.Troopers > 1) {
          HashSet<ChassisLocations> locations = new HashSet<ChassisLocations>();
          foreach(ChassisLocations location in TrooperSquad.locations) {
            LocationDef locationDef = ___mechDef.GetChassisLocationDef(location);
            if((locationDef.MaxArmor == 0f)&&(locationDef.InternalStructure <= 1f)) { continue; }

          }
          __result /= info.SquadInfo.Troopers;
        }
      }
    }
    public static string BaseDir { get; private set; }
    public static float TypeDmgCACModifier(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      ExtWeaponDef def = weapon.exDef();
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      float result = 1f;
      if(target is BattleTech.Building) {
        result *= (def.BuildingsDamageModifier * ammo.BuildingsDamageModifier * mode.BuildingsDamageModifier);
      }else
      if ((target is Turret)) {
        result *= def.TurretDamageModifier * ammo.TurretDamageModifier * mode.TurretDamageModifier;
      }else
      if (target is Vehicle vehicle) {
        UnitCustomInfo info = vehicle.GetCustomInfo();
        if (info != null) {
          if ((info.AOEHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) && (vehicle.UnaffectedPathing())) {
            result *= def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier;
          }
        }
        result *= def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier;
      }else
      if(target is CustomMech custMech) {
        if (custMech.isSquad) {
          result *= (def.TrooperSquadDamageModifier * ammo.TrooperSquadDamageModifier * mode.TrooperSquadDamageModifier);
        }else
        if (custMech.isVehicle) {
          result *= (def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier);
          if(custMech.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            result *= def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier;
          }
        } else {
          result *= (def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier);
          if (custMech.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            result *= def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier;
          }
        }
        if (custMech.isQuad) {
          result *= (def.QuadDamageModifier * ammo.QuadDamageModifier * mode.QuadDamageModifier);
        }
      }
      //if (target is TrooperSquad squad) {
      //  return def.TrooperSquadDamageModifier * ammo.TrooperSquadDamageModifier * mode.TrooperSquadDamageModifier;
      //}
      //if (target.GameRep != null) {
      //  AlternateMechRepresentations altReps = target.GameRep.GetComponent<AlternateMechRepresentations>();
      //  if(altReps != null) {
      //    if (altReps.isHovering) {
      //      return def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier * def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
      //    }
      //  }
      //  QuadRepresentation quadRep = target.GameRep.GetComponent<QuadRepresentation>();
      //  if (quadRep != null) {
      //    return def.QuadDamageModifier * ammo.QuadDamageModifier * mode.QuadDamageModifier * def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
      //  }
      //}
      //if(target is Mech mech) {
      //  return def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier;
      //}
      return result;
    }
    public static string TypeDmgCACModifierName(Weapon weapon, Vector3 attackPosition, ICombatant target, bool IsBreachingShot, int location, float dmg, float ap, float heat, float stab) {
      ExtWeaponDef def = weapon.exDef();
      ExtAmmunitionDef ammo = weapon.ammo();
      WeaponMode mode = weapon.mode();
      StringBuilder result = new StringBuilder();
      if (target is BattleTech.Building) {
        result.Append("Building:" + Math.Round(def.BuildingsDamageModifier * ammo.BuildingsDamageModifier * mode.BuildingsDamageModifier, 1)+";");
      } else
      if ((target is Turret)) {
        result.Append("Turret:" + Math.Round(def.TurretDamageModifier * ammo.TurretDamageModifier * mode.TurretDamageModifier, 1) + ";");
      } else
      if (target is Vehicle vehicle) {
        UnitCustomInfo info = vehicle.GetCustomInfo();
        if (info != null) {
          if ((info.AOEHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) && (vehicle.UnaffectedPathing())) {
            result.Append("VTOL:" + Math.Round(def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier, 1) + ";");
            //result *= def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier;
          }
        }
        result.Append("Vehicle:" + Math.Round(def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier, 1) + ";");
        //result *= def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier;
      } else
      if (target is CustomMech custMech) {
        if (custMech.isSquad) {
          //result *= (def.TrooperSquadDamageModifier * ammo.TrooperSquadDamageModifier * mode.TrooperSquadDamageModifier);
          result.Append("Squad:" + Math.Round(def.TrooperSquadDamageModifier * ammo.TrooperSquadDamageModifier * mode.TrooperSquadDamageModifier, 1) + ";");
        } else
        if (custMech.isVehicle) {
          //result *= (def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier);
          result.Append("Vehicle:" + Math.Round(def.VehicleDamageModifier * ammo.VehicleDamageModifier * mode.VehicleDamageModifier, 1) + ";");
          if (custMech.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            //result *= def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier;
            result.Append("VTOL:" + Math.Round(def.VTOLDamageModifier * ammo.VTOLDamageModifier * mode.VTOLDamageModifier, 1) + ";");
          }
        } else {
          //result *= (def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier);
          result.Append("Mech:" + Math.Round(def.MechDamageModifier * ammo.MechDamageModifier * mode.MechDamageModifier, 1) + ";");
          if (custMech.FlyingHeight() > Core.Settings.MaxHoveringHeightWithWorkingJets) {
            //result *= def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier;
            result.Append("Flying:" + Math.Round(def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier, 1) + ";");
          }
        }
        if (custMech.isQuad) {
          //result *= (def.QuadDamageModifier * ammo.QuadDamageModifier * mode.QuadDamageModifier);
          result.Append("Quad:" + Math.Round(def.AirMechDamageModifier * ammo.AirMechDamageModifier * mode.AirMechDamageModifier, 1) + ";");
        }
      }
      return result.ToString();
      //if (target is BattleTech.Building) {
      //  return "Building (x"+Math.Round(TypeDmgCACModifier(weapon,attackPosition,target,IsBreachingShot,location,dmg,ap,heat,stab), 1)+")";
      //}
      //if (target is Turret) {
      //  return "Turret (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")";
      //}
      //if (target is Vehicle vehicle) {
      //  UnitCustomInfo info = vehicle.GetCustomInfo();
      //  if (info != null) {
      //    if ((info.AOEHeight > Core.Settings.MaxHoveringHeightWithWorkingJets) && (vehicle.UnaffectedPathing())) {
      //      return "VTOL; Vehicle (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //    }
      //  }
      //  return "Vehicle (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //}
      //if (target is TrooperSquad squad) {
      //  return "Trooper squad (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //}
      //if (target.GameRep != null) {
      //  AlternateMechRepresentations altReps = target.GameRep.GetComponent<AlternateMechRepresentations>();
      //  if (altReps != null) {
      //    if (altReps.isHovering) {
      //      return "AirMech; Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //    }
      //  }
      //  QuadRepresentation quadRep = target.GameRep.GetComponent<QuadRepresentation>();
      //  if (quadRep != null) {
      //    return "Quad; Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //  }
      //}
      //if (target is Mech mech) {
      //  return "Mech (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
      //}
      //return "Target type (x" + Math.Round(TypeDmgCACModifier(weapon, attackPosition, target, IsBreachingShot, location, dmg, ap, heat, stab), 1) + ")"; ;
    }
    public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources) {
      Log.TWL(0, "FinishedLoading", true);
      IRBTModUtils.Feature.MovementFeature.RegisterMoveDistanceModifier("CustomUnits", 10, Mech_MaxWalkDistance.MaxWalkDistanceMod, Mech_MaxWalkDistance.MaxSprintDistanceMod);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.ChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.VehicleChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.HardpointDataDef, CustomHardPointsHelper.CustomHardpoints);
      CustomPrewarm.Core.RegisterSerializator("CustomUnitsChassisTags", BattleTechResourceType.VehicleChassisDef, BattleTech_VehicleChassisDef_fromJSON_Patch.serializeChassisTags);
      InfluenceMapPositionFactorPatch.PatchAll(Core.HarmonyInstance);
      try {
        foreach(string name in loadOrder) { if (name == "Mission Control") { CustomLanceHelper.MissionControlDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "MechEngineer") { Core.Settings.MechEngineerDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "LowVisibility") { LowVisibilityAPIHelper.Init(); break; }; }
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          Log.WL(1, "Assembly:"+ assembly.FullName);
          if (assembly.FullName.StartsWith("MechEngineer")) { Core.MechEngineerAssembly = assembly; }
        }
        if(Core.MechEngineerAssembly != null) {
          Type meStatHelper = Core.MechEngineerAssembly.GetType("MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics");
          if(meStatHelper != null) {
            Log.WL(1, "MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics found " + meStatHelper.Name);
            MethodInfo meStatHelper_GetJumpCapacity = meStatHelper.GetMethod("GetJumpCapacity", BindingFlags.NonPublic | BindingFlags.Instance);
            if(meStatHelper_GetJumpCapacity != null) {
              Log.WL(2, "MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics GetJumpCapacity found");
              Core.HarmonyInstance.Patch(meStatHelper_GetJumpCapacity, null, new HarmonyMethod(typeof(Core).GetMethod(nameof(MechDefMovementStatistics_GetJumpCapacity))));
            }
          }
        }
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
        Core.HarmonyInstance.Patch(typeof(Mech).GetMethod("DamageLocation", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(CustomMech).GetMethod(nameof(CustomMech.DamageLocation_Override), BindingFlags.Static | BindingFlags.Public)));
        CustomDeploy.Core.FinishLoading();
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
      Log.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + "\n", true);
      Log.WL(1,"Core.Settings.weaponPrefabMappings");
      foreach (var mapping in Core.Settings.weaponPrefabMappings) {
        Log.W(2, mapping.Key);
        foreach (var candidate in mapping.Value) {
          Log.W(1, candidate.Key + ":" + candidate.Value);
        };
        Log.WL(0, "");
      }
      //InitLancesLoadoutDefault();
      //CustomLanceHelper.BaysCount(3+(Core.Settings.BaysCountExternalControl?0:Core.Settings.ArgoBaysFix));
      MechResizer.MechResizer.Init(directory, settingsJson);
      SortByTonnage.SortByTonnage.Init(directory, Core.Settings.SortBy);
      PilotingClassHelper.CreateDefault();
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
        string DeployHelperAssemblyPath = Path.Combine(directory, "CustomDeploy.dll");
        Assembly CustomDeployAssembly = Assembly.LoadFile(DeployHelperAssemblyPath);
        CustomDeploy.Core.Init(directory, Core.Settings.debugLog);

        string CUHelperAssemblyPath = Path.Combine(directory, "CustomUnitsHelper.dll");
        Assembly CUHelperAssembly = Assembly.LoadFile(CUHelperAssemblyPath);
        Log.TWL(0,"Helper assembly "+CUHelperAssembly.FullName);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        PathingInfoHelper.RegisterMaxMoveDeligate(PathingHelper.MaxMoveDistance);
        //WeightedFactorHelper.PatchInfluenceMapPositionFactor(HarmonyInstance);
        WeaponRepresentation_PlayWeaponEffect.i_extendedFire = extendedFireHelper.extendedFire;
        //Debug.unityLogger.logEnabled = false;
      } catch (Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
    }
  }
}
