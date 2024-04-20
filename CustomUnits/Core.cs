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
using HarmonyLib;
using HarmonyLib.Tools;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomUnits {
  public static class PatchingDebug {
    public static MethodInfo GetOriginalMethod_Target() {
      return AccessTools.Method(typeof(Harmony).Assembly.GetType("HarmonyLib.PatchTools"), "GetOriginalMethod");
    }
    public static Exception GetOriginalMethod_Finalizer(Exception __exception, HarmonyMethod attr) {
      try {
        if (__exception != null) {
          Log.M?.TWL(0, $"HarmonyLib.PatchTools.GetOriginalMethod type:{attr.methodType.SafeToString()} {attr.declaringType.Name}{attr.methodName}");
          Log.M?.WL(0, __exception.ToString(),true);
        }
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UnityGameInstance.logger.LogException(e);
      }
      return __exception;
    }
    public static HarmonyMethod GetOriginalMethod_Finalizer_H() {
      var result = new HarmonyMethod(AccessTools.Method(typeof(PatchingDebug), nameof(GetOriginalMethod_Finalizer)));
      return result;
    }
  }
  public class ComponentPrefabMap {
    public string PrefabIdentifier { get; set; }
    public Dictionary<string,float> HardpointCandidates { get; set; }
    public ComponentPrefabMap() { PrefabIdentifier = string.Empty; HardpointCandidates = new Dictionary<string, float>(); }
  }
  public class TimerObjectiveAdvice {
    public int manualDeployAdvice { get; set; } = 0;
    public int autoDeployAdvice { get; set; } = 0;
    public TimerObjectiveAdvice() { }
    public TimerObjectiveAdvice(int a, int m) { manualDeployAdvice = m; autoDeployAdvice = a; }
  }
  public static partial class Core {
    public static readonly float Epsilon = 0.001f;
    public static CUSettings Settings;
    public static CUSettings GlobalSettings;
    public static Assembly MechEngineerAssembly = null;
    private static Type EngineHeatSinkDef = null;
    private static Type CoolingDef = null;
    private static Type EngineCoreDef = null;
    private static Type EngineHeatBlockDef = null;
    public delegate bool d_CustomComponents_Is(MechComponentDef def);
    private static MethodInfo is_EngineHeatSinkDef = null;
    private static MethodInfo is_CoolingDef = null;
    private static MethodInfo is_EngineCoreDef = null;
    private static MethodInfo is_EngineHeatBlockDef = null;
    private static d_CustomComponents_Is i_is_EngineHeatSinkDef = null;
    private static d_CustomComponents_Is i_is_CoolingDef = null;
    private static d_CustomComponents_Is i_is_EngineCoreDef = null;
    private static d_CustomComponents_Is i_is_EngineHeatBlockDef = null;
    public static bool Is_EngineHeatSinkDef(this MechComponentDef component) {
      if(i_is_EngineHeatSinkDef == null) { return false; }
      return i_is_EngineHeatSinkDef(component);
    }
    public static bool Is_CoolingDef(this MechComponentDef component) {
      if(i_is_CoolingDef == null) { return false; }
      return i_is_CoolingDef(component);
    }
    public static bool Is_EngineCoreDef(this MechComponentDef component) {
      if(i_is_EngineCoreDef == null) { return false; }
      return i_is_EngineCoreDef(component);
    }
    public static bool Is_EngineHeatBlockDef(this MechComponentDef component) {
      if(i_is_EngineHeatBlockDef == null) { return false; }
      return i_is_EngineHeatBlockDef(component);
    }
    public static void InitMechEngineer_API() {
      Log.M?.TWL(0, "Core.InitMechEngineer_API");
      try {
        Core.EngineHeatSinkDef = Core.MechEngineerAssembly.GetType("MechEngineer.Features.Engines.EngineHeatSinkDef");
        if(Core.EngineHeatSinkDef == null) { Log.M?.WL(1, $"can't find MechEngineer.Features.Engines.EngineHeatSinkDef"); return; }
        Core.CoolingDef = Core.MechEngineerAssembly.GetType("MechEngineer.Features.Engines.CoolingDef");
        if(Core.CoolingDef == null) { Log.M?.WL(1, $"can't find MechEngineer.Features.Engines.CoolingDef"); return; }
        Core.EngineCoreDef = Core.MechEngineerAssembly.GetType("MechEngineer.Features.Engines.EngineCoreDef");
        if(Core.EngineCoreDef == null) { Log.M?.WL(1, $"can't find MechEngineer.Features.Engines.EngineCoreDef"); return; }
        Core.EngineHeatBlockDef = Core.MechEngineerAssembly.GetType("MechEngineer.Features.Engines.EngineHeatBlockDef");
        if(Core.EngineHeatBlockDef == null) { Log.M?.WL(1, $"can't find MechEngineer.Features.Engines.EngineHeatBlockDef"); return; }
        MethodInfo MechComponentDefExtensions_Is = typeof(CustomComponents.MechComponentDefExtensions).GetMethod("Is", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(MechComponentDef) }, null);
        if(MechComponentDefExtensions_Is == null) { Log.M?.WL(1, $"can't find CustomComponents.MechComponentDefExtensions.Is"); return; }
        Core.is_EngineHeatSinkDef = MechComponentDefExtensions_Is.MakeGenericMethod(new Type[] { Core.EngineHeatSinkDef });
        Core.is_CoolingDef = MechComponentDefExtensions_Is.MakeGenericMethod(new Type[] { Core.CoolingDef });
        Core.is_EngineCoreDef = MechComponentDefExtensions_Is.MakeGenericMethod(new Type[] { Core.EngineCoreDef });
        Core.is_EngineHeatBlockDef = MechComponentDefExtensions_Is.MakeGenericMethod(new Type[] { Core.EngineHeatBlockDef });
        {
          var dm = new DynamicMethod("is_EngineHeatSinkDef", typeof(bool), new Type[] { typeof(MechComponentDef) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, is_EngineHeatSinkDef);
          gen.Emit(OpCodes.Ret);
          i_is_EngineHeatSinkDef = (d_CustomComponents_Is)dm.CreateDelegate(typeof(d_CustomComponents_Is));
        }
        {
          var dm = new DynamicMethod("is_CoolingDef", typeof(bool), new Type[] { typeof(MechComponentDef) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, is_CoolingDef);
          gen.Emit(OpCodes.Ret);
          i_is_CoolingDef = (d_CustomComponents_Is)dm.CreateDelegate(typeof(d_CustomComponents_Is));
        }
        {
          var dm = new DynamicMethod("is_EngineCoreDef", typeof(bool), new Type[] { typeof(MechComponentDef) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, is_EngineCoreDef);
          gen.Emit(OpCodes.Ret);
          i_is_EngineCoreDef = (d_CustomComponents_Is)dm.CreateDelegate(typeof(d_CustomComponents_Is));
        }
        {
          var dm = new DynamicMethod("is_EngineHeatBlockDef", typeof(bool), new Type[] { typeof(MechComponentDef) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, is_EngineHeatBlockDef);
          gen.Emit(OpCodes.Ret);
          i_is_EngineHeatBlockDef = (d_CustomComponents_Is)dm.CreateDelegate(typeof(d_CustomComponents_Is));
        }
      } catch(Exception e) {
        Log.M?.TWL(0, e.ToString());
        UnityGameInstance.logger.LogException(e);
      }
      //.MakeGenericMethod(new Type[] { Core.EngineHeatSinkDef } );
    }
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
      Log.M?.TWL(0, "FinishedLoading", true);
      CustomSettings.ModsLocalSettingsHelper.RegisterLocalSettings("CustomUnits", "Custom Units"
        , LocalSettingsHelper.ResetSettings
        , LocalSettingsHelper.ReadSettings
        , LocalSettingsHelper.DefaultSettings
        , LocalSettingsHelper.CurrentSettings
        , LocalSettingsHelper.SaveSettings
        );
      CustAmmoCategories.UnitDefTypesAPI.Register_isVehcileDelegateMechDef(FakeDatabase.IsVehicle);
      CustAmmoCategories.UnitDefTypesAPI.Register_isVehcileDelegateChassisDef(FakeDatabase.IsVehicle);
      CustAmmoCategories.UnitDefTypesAPI.Register_isQuadDelegateMechDef(FakeDatabase.IsQuad_Delegate);
      CustAmmoCategories.UnitDefTypesAPI.Register_isQuadDelegateChassisDef(FakeDatabase.IsQuad_Delegate);
      CustAmmoCategories.UnitDefTypesAPI.Register_isSquadDelegateMechDef(VehicleCustomInfoHelper.IsSquad);
      CustAmmoCategories.UnitDefTypesAPI.Register_isSquadDelegateChassisDef(FakeDatabase.IsSquad_Delegate);
      CustAmmoCategories.UnitDefTypesAPI.Register_isDestroyedDelegate(FakeDatabase.IsDestroyed_Delegate);
      CustAmmoCategories.UnitDefTypesAPI.Register_GetAbbreviatedChassisLocationDelegate(VehicleCustomInfoHelper.GetAbbreviatedChassisLocationDelegate);
      IRBTModUtils.Feature.MovementFeature.RegisterMoveDistanceModifier("CustomUnits", 10, Mech_MaxWalkDistance.MaxWalkDistanceMod, Mech_MaxWalkDistance.MaxSprintDistanceMod);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.ChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.VehicleChassisDef, VehicleCustomInfoHelper.GetInfoByChassisId);
      CustomPrewarm.Core.RegisterSerializator("CustomUnits", BattleTechResourceType.HardpointDataDef, CustomHardPointsHelper.CustomHardpoints);
      CustomPrewarm.Core.RegisterSerializator("CustomUnitsChassisTags", BattleTechResourceType.VehicleChassisDef, BattleTech_VehicleChassisDef_fromJSON_Patch.serializeChassisTags);
      InfluenceMapPositionFactorPatch.PatchAll(Core.HarmonyInstance);
      try {
        MechLabPanelFillAs.InitMechLabInventoryAccess();
        foreach (string name in loadOrder) { if (name == "Mission Control") { CustomLanceHelper.MissionControlDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "MechEngineer") { Core.Settings.MechEngineerDetected = true; break; }; }
        foreach (string name in loadOrder) { if (name == "LowVisibility") { LowVisibilityAPIHelper.Init(); break; }; }
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
          Log.M?.WL(1, "Assembly:"+ assembly.FullName);
          if (assembly.FullName.StartsWith("MechEngineer")) { Core.MechEngineerAssembly = assembly; }
        }
        if(Core.MechEngineerAssembly != null) {
          Type meStatHelper = Core.MechEngineerAssembly.GetType("MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics");
          if(meStatHelper != null) {
            Log.M?.WL(1, "MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics found " + meStatHelper.Name);
            InitMechEngineer_API();
            MethodInfo meStatHelper_GetJumpCapacity = meStatHelper.GetMethod("GetJumpCapacity", BindingFlags.NonPublic | BindingFlags.Instance);
            if(meStatHelper_GetJumpCapacity != null) {
              Log.M?.WL(2, "MechEngineer.Features.OverrideStatTooltips.Helper.MechDefMovementStatistics GetJumpCapacity found");
              Core.HarmonyInstance.Patch(meStatHelper_GetJumpCapacity, null, new HarmonyMethod(typeof(Core).GetMethod(nameof(MechDefMovementStatistics_GetJumpCapacity))));
            }
          }
        }
        foreach (var customResource in customResources) {
          Log.M?.TWL(0, $"customResource:{customResource.Key}");
          if (customResource.Key == "CustomMechRepresentationDef") {
            foreach (var res in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{res.Value.FilePath}",true);
                CustomMechRepresentationDef mechRepDef = JsonConvert.DeserializeObject<CustomMechRepresentationDef>(File.ReadAllText(res.Value.FilePath));
                mechRepDef.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, res.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "CustomPrefabDef") {
            foreach (var res in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{res.Value.FilePath}", true);
                CustomPrefabDef mechRepDef = JsonConvert.DeserializeObject<CustomPrefabDef>(File.ReadAllText(res.Value.FilePath));
                mechRepDef.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, res.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}",true);
                DropSlotDef def = JsonConvert.DeserializeObject<DropSlotDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, $"id:{def.Description.Id}");
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropLanceDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}",true);
                DropLanceDef def = JsonConvert.DeserializeObject<DropLanceDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, $"id:{def.Description.Id}");
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotsDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}",true);
                DropSlotsDef def = JsonConvert.DeserializeObject<DropSlotsDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, "id:" + def.Description.Id);
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropSlotDecorationDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
                DropSlotDecorationDef def = JsonConvert.DeserializeObject<DropSlotDecorationDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, "id:" + def.Description.Id);
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "DropClassDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
                DropClassDef def = JsonConvert.DeserializeObject<DropClassDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, "id:" + def.Description.Id);
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "PilotingClassDef") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
                PilotingClassDef def = JsonConvert.DeserializeObject<PilotingClassDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, "id:" + def.Description.Id);
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == nameof(CustomHangarDef)) {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
                CustomHangarDef def = JsonConvert.DeserializeObject<CustomHangarDef>(File.ReadAllText(custItem.Value.FilePath));
                Log.M?.WL(1, "id:" + def.Description.Id);
                Log.M?.WL(1, JsonConvert.SerializeObject(def, Formatting.Indented));
                def.Register();
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == "CustomWeatherEffect") {
            foreach (var custItem in customResource.Value) {
              try {
                Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
                IntelHelper.AddMood(custItem.Key, custItem.Value);
              } catch (Exception e) {
                Log.E?.TWL(0, custItem.Key, true);
                Log.E?.WL(0, e.ToString(), true);
              }
            }
          } else if (customResource.Key == nameof(CustomStructureDef)) {
            foreach (var custItem in customResource.Value) {
              Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
              CustomStructureDef.Register(custItem.Value.FilePath);
            }
          } else if (customResource.Key == nameof(CustomHitTableDef)) {
            foreach (var custItem in customResource.Value) {
              Log.M?.WL(1, $"Path:{custItem.Value.FilePath}", true);
              CustomHitTableDef.Register(custItem.Value.FilePath);
            }
          } else {
            throw new Exception("Unknown resource "+ customResource.Key);
          }
        }
        CustomHitTableDef.Resolve();
        CustAmmoCategories.ToHitModifiersHelper.registerModifier("SQUAD SIZE", "SQUAD SIZE", true, false, TrooperSquad.GetSquadSizeToHitMod, TrooperSquad.GetSquadSizeToHitModName);
        CustAmmoCategories.DamageModifiersCache.RegisterDamageModifier("SQUAD SIZE", "SQUAD SIZE", true, true, true, true, true, TrooperSquad.SquadSizeDamageMod, TrooperSquad.SquadSizeDamageModName);
        DamageModifiersCache.RegisterDamageModifier("TYPEMOD", "TYPEMOD", false, true, true, true, true, TypeDmgCACModifier, TypeDmgCACModifierName);
        CustAmmoCategories.DeferredEffectHelper.RegisterCallback("HOTDROP",HotDropManager.DeferredHotDrop);
        PilotingClassHelper.Validate();
        //DropClassDef.Validate();
        DropSystemHelper.Validate();
        Core.HarmonyInstance.Patch(typeof(Mech).GetMethod("InitGameRep", BindingFlags.Public | BindingFlags.Instance),new HarmonyMethod(typeof(CustomMech).GetMethod(nameof(CustomMech.InitGameRepStatic), BindingFlags.Static | BindingFlags.Public)));
        //Log.TWL(0, "Harmony log Mech.InitGameRep");
        //Patches patches = Core.HarmonyInstance.GetPatchInfo(typeof(Mech).GetMethod("InitGameRep", BindingFlags.Public | BindingFlags.Instance));
        //Log.WL(1, "Prefixes:");
        //foreach (var patch in patches.Prefixes) {
        //  Log.WL(2, patch.owner+" index:"+patch.index+" method:"+patch.patch.Name);
        //}
        //Log.WL(1, "Postfixes:");
        //foreach (var patch in patches.Postfixes) {
        //  Log.WL(2, patch.owner + " index:" + patch.index + " method:" + patch.patch.Name);
        //}
        Core.HarmonyInstance.Patch(typeof(Mech).GetMethod("DamageLocation", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(CustomMech).GetMethod(nameof(CustomMech.DamageLocation_Override), BindingFlags.Static | BindingFlags.Public)));
        Core.HarmonyInstance.Patch(TrooperSquad.Pilot_IsIncapacitated_get(), null, TrooperSquad.Pilot_IsIncapacitated_get_patch());
        ChassisHandler_MakeMech.Prepare();
        LewdableTanks_Patches_Contract_CompleteContract_Patch.Prepare();
        MechEngineer_Features_CustomCapacities_CustomCapacitiesFeature.Prepare();
        //Core.HarmonyInstance.Patch(Contract_BeginRequestResources_Intel.TargetMethod(), Contract_BeginRequestResources_Intel.Patch());
        CustomDeploy.Core.FinishLoading();
      } catch (Exception e) {
        Log.E?.TWL(0, e.ToString(), true);
      }
    }
    public static Harmony HarmonyInstance = null;
    public static void LoadBuildinShaders() {
      Log.M?.TWL(0,"Loading buildin shaders");
      foreach (string shaderName in Core.Settings.forceToBuildinShadersList) {
        Shader shader = Shader.Find(shaderName);
        if (shader != null) {
          Core.Settings.forceToBuildinShaders.Add(shaderName, shader);
          Log.M?.WL(1, $"shader found {shader.name}:{shader.GetInstanceID()}");
        } else {
          Log.M?.WL(1, $"shader not found {shaderName}");
        }
      }
      foreach(var repShader in Core.Settings.shadersReplacementList) {
        if (Core.Settings.forceToBuildinShaders.ContainsKey(repShader.Key)) { continue; }
        if(Core.Settings.forceToBuildinShaders.TryGetValue(repShader.Value,out var shader)) {
          Log.M?.WL(1, $"shader replaced {repShader.Key} -> {shader.name}:{shader.GetInstanceID()}");
          Core.Settings.forceToBuildinShaders.Add(repShader.Key, shader);
        }
      }
    }
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Core.BaseDir = directory;
      Core.GlobalSettings = JsonConvert.DeserializeObject<CustomUnits.CUSettings>(settingsJson);
      Core.Settings = JsonConvert.DeserializeObject<CustomUnits.CUSettings>(settingsJson);
      //PilotingClass.Validate();
      Log.M?.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      //Log.WL(1, $"Harmony.FileLog.logPath:{Harmony.FileLog.logPath}");
      //HarmonyInstance.DEBUG = true;
      LoadBuildinShaders();
      Vector3 pos = new Vector3(-7.6f, 96.5f, 0f);
      Vector3 look = new Vector3(-1.9f, 96.5f, 0f);
      Vector3 diff = look - pos;
      Log.M?.WL(1,$"look:{look} pos:{pos} forward:{diff} backrot:{Quaternion.LookRotation(diff,Vector3.back).eulerAngles} fwdrot:{Quaternion.LookRotation(diff, Vector3.forward).eulerAngles} uprot:{Quaternion.LookRotation(diff, Vector3.up).eulerAngles} downrot:{Quaternion.LookRotation(diff, Vector3.down).eulerAngles}");
      Log.M?.WL(1,"Core.Settings.weaponPrefabMappings");
      foreach (var mapping in Core.Settings.weaponPrefabMappings) {
        Log.M?.W(2, mapping.Key);
        foreach (var candidate in mapping.Value) {
          Log.M?.W(1, candidate.Key + ":" + candidate.Value);
        };
        Log.M?.WL(0, "");
      }
      //InitLancesLoadoutDefault();
      //CustomLanceHelper.BaysCount(3+(Core.Settings.BaysCountExternalControl?0:Core.Settings.ArgoBaysFix));
      CustomComponents.Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
      MechResizer.MechResizer.Init(directory, settingsJson);
      SortByTonnage.SortByTonnage.Init(directory, Core.Settings.SortBy);
      PilotingClassHelper.CreateDefault();
      try {
        HarmonyInstance = new Harmony("io.mission.customunits");
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
        Log.M?.TWL(0,"Helper assembly "+CUHelperAssembly.FullName);
        HarmonyFileLog.Enabled = true;
        HarmonyInstance.Patch(PatchingDebug.GetOriginalMethod_Target(), null, null, null, PatchingDebug.GetOriginalMethod_Finalizer_H(), null);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        PathingInfoHelper.RegisterMaxMoveDeligate(PathingHelper.MaxMoveDistance);
        //WeightedFactorHelper.PatchInfluenceMapPositionFactor(HarmonyInstance);
        WeaponRepresentation_PlayWeaponEffect.i_extendedFire = extendedFireHelper.extendedFire;
        //Debug.unityLogger.logEnabled = false;
      } catch (Exception e) {
        //HarmonyInstance.DEBUG = false;

        Log.E?.TWL(0,e.ToString(),true);
      }
    }
  }
}
