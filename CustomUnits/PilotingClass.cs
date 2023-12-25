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
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using CustAmmoCategories;
using HarmonyLib;
using HBS.Collections;
using IRBTModUtils;
using Newtonsoft.Json;
using SVGImporter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnits {
  public static class PilotingClassHelper {
    private static Dictionary<string, PilotingClassDef> pilotingClasses = new Dictionary<string, PilotingClassDef>();
    public static void Register(this PilotingClassDef def) {
      if (pilotingClasses.ContainsKey(def.Description.Id)) {
        pilotingClasses[def.Description.Id] = def;
      } else {
        pilotingClasses.Add(def.Description.Id, def);
      }
    }
    public static PilotingClassDef DEFAULT_MECH_PILOTING_CLASS { get; private set; } = null;
    public static PilotingClassDef DEFAULT_VEHICLE_PILOTING_CLASS { get; private set; } = null;
    public static void CreateDefault() {
      PilotingClassDef GenericVehicleClass = new PilotingClassDef();
      GenericVehicleClass.expertiseGenerationChance = Core.Settings.CanPilotVehicleProbability;
      GenericVehicleClass.expertiseGenerationMinCount = 1;
      GenericVehicleClass.Description.Id = PilotingClassDef.DEFAULT_VEHICLE_PILOTING_NAME;
      GenericVehicleClass.Description.Name = PilotingClassDef.DEFAULT_VEHICLE_PILOTING_UINAME;
      PilotingClassDef GenericMechClass = new PilotingClassDef();
      GenericMechClass.expertiseGenerationChance = Core.Settings.CanPilotAlsoMechProbability;
      GenericMechClass.expertiseGenerationMinCount = 1;
      GenericMechClass.Description.Id = PilotingClassDef.DEFAULT_MECH_PILOTING_NAME;
      GenericMechClass.Description.Name = PilotingClassDef.DEFAULT_MECH_PILOTING_UINAME;
      if (GenericMechClass.unitTags.Count == 0) { GenericMechClass.unitTags.Add(PilotingClassDef.DEFAULT_MECH_UNIT_TAG); }
      if (GenericMechClass.pilotTags.Count == 0) { GenericMechClass.pilotTags.Add(PilotingClassDef.DEFAULT_MECH_PILOTING_TAG); }
      if (GenericVehicleClass.unitTags.Count == 0) { GenericVehicleClass.unitTags.Add(PilotingClassDef.DEFAULT_VEHICLE_UNIT_TAG); }
      if (GenericVehicleClass.pilotTags.Count == 0) { GenericVehicleClass.pilotTags.Add(PilotingClassDef.DEFAULT_VEHICLE_PILOTING_TAG); }
      DEFAULT_MECH_PILOTING_CLASS = GenericMechClass;
      DEFAULT_VEHICLE_PILOTING_CLASS = GenericVehicleClass;
    }
    public static void Validate() {
      Log.M?.TWL(0, "PilotingClassHelper.Validate");
      if (pilotingClasses.TryGetValue(PilotingClassDef.DEFAULT_MECH_PILOTING_NAME, out PilotingClassDef GenericMechClass) == false) {
        GenericMechClass = new PilotingClassDef();
        GenericMechClass.expertiseGenerationChance = Core.Settings.CanPilotAlsoMechProbability;
        GenericMechClass.expertiseGenerationMinCount = 1;
        GenericMechClass.Description.Id = PilotingClassDef.DEFAULT_MECH_PILOTING_NAME;
        GenericMechClass.Description.Name = PilotingClassDef.DEFAULT_MECH_PILOTING_UINAME;
        pilotingClasses.Add(PilotingClassDef.DEFAULT_MECH_PILOTING_NAME, GenericMechClass);
      }
      if (pilotingClasses.TryGetValue(PilotingClassDef.DEFAULT_VEHICLE_PILOTING_NAME, out PilotingClassDef GenericVehicleClass) == false) {
        GenericVehicleClass = new PilotingClassDef();
        GenericVehicleClass.expertiseGenerationChance = Core.Settings.CanPilotVehicleProbability;
        GenericVehicleClass.expertiseGenerationMinCount = 1;
        GenericVehicleClass.Description.Id = PilotingClassDef.DEFAULT_VEHICLE_PILOTING_NAME;
        GenericVehicleClass.Description.Name = PilotingClassDef.DEFAULT_VEHICLE_PILOTING_UINAME;
        pilotingClasses.Add(PilotingClassDef.DEFAULT_VEHICLE_PILOTING_NAME, GenericVehicleClass);
      }
      if (GenericMechClass.unitTags.Count == 0) { GenericMechClass.unitTags.Add(PilotingClassDef.DEFAULT_MECH_UNIT_TAG); }
      if (GenericMechClass.pilotTags.Count == 0) { GenericMechClass.pilotTags.Add(PilotingClassDef.DEFAULT_MECH_PILOTING_TAG); }
      if (GenericVehicleClass.unitTags.Count == 0) { GenericVehicleClass.unitTags.Add(PilotingClassDef.DEFAULT_VEHICLE_UNIT_TAG); }
      if (GenericVehicleClass.pilotTags.Count == 0) { GenericVehicleClass.pilotTags.Add(PilotingClassDef.DEFAULT_VEHICLE_PILOTING_TAG); }
      DEFAULT_MECH_PILOTING_CLASS = GenericMechClass;
      DEFAULT_VEHICLE_PILOTING_CLASS = GenericVehicleClass;
      foreach (var pilotingClass0 in pilotingClasses) {
        if (pilotingClass0.Value.unitTags.IsEmpty) {
          throw new Exception("piloting class " + pilotingClass0.Key + " unit tags set is empty. Fix this");
        }
        if (pilotingClass0.Value.pilotTags.IsEmpty) {
          throw new Exception("piloting class " + pilotingClass0.Key + " pilot tags set is empty. Fix this");
        }
        pilotingClass0.Value.Description.Id = pilotingClass0.Key;
        foreach (string id in pilotingClass0.Value.ExcludeClasses) {
          if (pilotingClasses.TryGetValue(id, out PilotingClassDef exclude)) {
            pilotingClass0.Value.excludeClasses.Add(exclude);
          }
        }
        foreach(string id in pilotingClass0.Value.AdditionalClasses) {
          if(pilotingClasses.TryGetValue(id, out PilotingClassDef additional)) {
            pilotingClass0.Value.additionalClasses.Add(additional);
          }
        }
      }
      Log.M?.WL(1, "default mech class:"+ PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS==null?"null": PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.Description.Id);
      Log.M?.WL(1, "default vehicle class:" + PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS == null ? "null" : PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.Description.Id);

    }

    private static readonly string IS_IN_STARTING_PILOTS_GENERATING_FLAG = "STARTING_PILOTS_GENERATING";
    public static bool isInStartingPilotsGen() {
      return Thread.CurrentThread.isFlagSet(IS_IN_STARTING_PILOTS_GENERATING_FLAG);
    }
    public static void pushInStartingPilotsGen() {
      Thread.CurrentThread.SetFlag(IS_IN_STARTING_PILOTS_GENERATING_FLAG);
    }
    public static void popInStartingPilotsGen() {
      Thread.CurrentThread.ClearFlag(IS_IN_STARTING_PILOTS_GENERATING_FLAG);
    }
    private static ConcurrentDictionary<string, HashSet<PilotingClassDef>> pilotingClassesCache = new ConcurrentDictionary<string, HashSet<PilotingClassDef>>();
    public static bool CanBePilotedBy(this ChassisDef chassis, PilotDef pilot, out string title, out string message) {
      title = string.Empty;
      message = string.Empty;
      HashSet<PilotingClassDef> chassis_pclasses = chassis.GetPilotingClass();
      bool result = true;
      StringBuilder fail_classes = new StringBuilder();
      foreach (PilotingClassDef pclass in chassis_pclasses) {
        if (pclass.isMyPilotClass(pilot.PilotTags) == false) {
          if (result == false) { fail_classes.Append(", "); }
          fail_classes.Append(pclass.description.Name);
          result = false;
        }
      }
      if (result == false) {
        title = new Localize.Text("__/PILOT.CANT_PILOT/__",pilot.Description.Callsign, chassis.Description.Name).ToString();
        message = new Localize.Text("__/PILOT.CANT_PILOT_MESSAGE/__", chassis.Description.Name, pilot.Description.Callsign, fail_classes.ToString()).ToString();
      }
      return result;
    }
    public static void fallbackPilotingClass(this ChassisDef chassis) {
      UnitCustomInfo info = chassis.GetCustomInfo();
      if (info.FakeVehicle) {
        if (pilotingClasses.TryGetValue(PilotingClassDef.DEFAULT_VEHICLE_PILOTING_NAME, out PilotingClassDef pclass)) {
          chassis.ChassisTags.UnionWith(pclass.unitTags);
          if (pilotingClassesCache.TryGetValue(chassis.Description.Id, out HashSet<PilotingClassDef> result) == false) {
            result = new HashSet<PilotingClassDef>();
            pilotingClassesCache.TryAdd(chassis.Description.Id, result);
          }
          result.Add(pclass);
        }
      } else {
        if (pilotingClasses.TryGetValue(PilotingClassDef.DEFAULT_MECH_PILOTING_NAME, out PilotingClassDef pclass)) {
          chassis.ChassisTags.UnionWith(pclass.unitTags);
          if (pilotingClassesCache.TryGetValue(chassis.Description.Id, out HashSet<PilotingClassDef> result) == false) {
            result = new HashSet<PilotingClassDef>();
            pilotingClassesCache.TryAdd(chassis.Description.Id, result);
          }
          result.Add(pclass);
        }
      }
    }
    public static HashSet<PilotingClassDef> GetPilotingClass(this ChassisDef chassis, bool exception = true) {
      if (pilotingClassesCache.TryGetValue(chassis.Description.Id, out HashSet<PilotingClassDef> result)) { return result; }
      result = new HashSet<PilotingClassDef>();
      foreach (var pilotClass in pilotingClasses) {
        if (pilotClass.Value.isMyUnitClass(chassis.ChassisTags)) { result.Add(pilotClass.Value); }
      }
      pilotingClassesCache.TryAdd(chassis.Description.Id, result);
      return result;
    }
    public static HashSet<PilotingClassDef> GetPilotingClass(this VehicleChassisDef chassis) {
      if (pilotingClassesCache.TryGetValue(chassis.Description.Id, out HashSet<PilotingClassDef> result)) { return result; }
      result = new HashSet<PilotingClassDef>();
      foreach (var pilotClass in pilotingClasses) {
        if (pilotClass.Value.isMyUnitClass(chassis.ChassisTags())) { result.Add(pilotClass.Value); }
      }
      pilotingClassesCache.TryAdd(chassis.Description.Id, result);
      return result;
    }
    public static HashSet<PilotingClassDef> GetPilotingClass(this PilotDef pilot) {
      HashSet<PilotingClassDef> result = new HashSet<PilotingClassDef>();
      if (pilot.PilotTags == null) { Traverse.Create(pilot).Property<TagSet>("PilotTags").Value = new TagSet(); }
      foreach (var pilotingClass in pilotingClasses) {
        if (pilot.PilotTags.ContainsAll(pilotingClass.Value.pilotTags)) { result.Add(pilotingClass.Value); }
      }
      return result;
    }
    public static bool CanHaveExpertise(HashSet<PilotingClassDef> classes, PilotingClassDef pilot_class) {
      foreach (PilotingClassDef pclass in classes) {
        if (pclass.excludeClasses.Contains(pilot_class)) { return false; }
      }
      return true;
    }
    public static void AddPilotingClass(this PilotDef pilot, PilotingClassDef pclass) { pilot.PilotTags.UnionWith(pclass.pilotTags); }
    private static void RemoveReadyClasses(ref Dictionary<PilotingClassDef, int> classes_count) {
      HashSet<PilotingClassDef> to_del = new HashSet<PilotingClassDef>();
      foreach (var pclass in classes_count) {
        if (pclass.Key.expertiseGenerationMinCount <= pclass.Value) { to_del.Add(pclass.Key); }
      }
      foreach (PilotingClassDef pclass in to_del) { classes_count.Remove(pclass); }
    }
    private static List<PilotDef> gatherPilotsCanHaveClass(ref List<PilotDef> pilots, PilotingClassDef pclass) {
      List<PilotDef> result = new List<PilotDef>();
      foreach(PilotDef pilot in pilots) {
        if (CanHaveExpertise(pilot.GetPilotingClass(), pclass)) { result.Add(pilot); }
      }
      return result;
    }
    public static void GeneratePilotingExpertisesStart(this SimGameState simgame) {
      Log.M?.TWL(0, "GeneratePilotingExpertisesStart");
      HashSet<MechDef> activeMechs = new HashSet<MechDef>();
      HashSet<PilotDef> activePilots = new HashSet<PilotDef>();
      foreach(var mech in simgame.ActiveMechs) {
        activeMechs.Add(mech.Value);
        Log.M?.WL(1, "mech:" + mech.Value.ChassisID);
      }
      activePilots.Add(simgame.Commander.pilotDef);
      foreach (var pilot in simgame.PilotRoster) {
        activePilots.Add(pilot.pilotDef);
      }
      foreach (PilotDef pilot in activePilots) {
        Log.M?.WL(1, "pilot:" + pilot.Description.Id+"/"+pilot.Description.Callsign);
      }
      foreach (MechDef mech in activeMechs) {
        HashSet<PilotingClassDef> unit_classes = mech.Chassis.GetPilotingClass();
        Log.M?.W(1, "mech:"+mech.ChassisID);
        foreach (PilotingClassDef pcl in unit_classes) { Log.M?.W(1,pcl.Description.Id); }
        Log.M?.WL(0, "");
        List<PilotDef> pilots = gatherPilotCanApplyVehicleDriving(activePilots, unit_classes);
        if (pilots.Count == 0) { continue; }
        PilotDef pilot = pilots[UnityEngine.Random.Range(0, pilots.Count)];
        activePilots.Remove(pilot);
        foreach (PilotingClassDef unitClass in unit_classes) { pilot.AddPilotingClass(unitClass); }
      }
      List<PilotDef> rest_pilots = new List<PilotDef>();
      foreach (PilotDef pilot in activePilots) { rest_pilots.Add(pilot); }
      GeneratePilotingExpertises(ref rest_pilots, false);
      foreach (PilotDef pilot in rest_pilots) {
        HashSet<PilotingClassDef> pclasses = pilot.GetPilotingClass();
        pilot.fallbackPiloting();
      }
      activePilots.Clear();
      activePilots.Add(simgame.Commander.pilotDef);
      foreach (var pilot in simgame.PilotRoster) {
        activePilots.Add(pilot.pilotDef);
      }
      foreach (PilotDef pilot in activePilots) {
        Log.M?.W(1, "pilot:" + pilot.Description.Id + "/" + pilot.Description.Callsign);
        foreach (string tag in pilot.PilotTags) { Log.M?.W(1, tag); }
        Log.M?.WL(0, "");
      }
    }
    public static List<PilotDef> gatherPilotCanApplyVehicleDriving(HashSet<PilotDef> activePilots, HashSet<PilotingClassDef> unit_classes) {
      List<PilotDef> result = new List<PilotDef>();
      foreach (PilotDef pilot in activePilots) {
        bool can_drive_unit = true;
        HashSet<PilotingClassDef> pilotClasses = pilot.GetPilotingClass();
        foreach (PilotingClassDef unitClass in unit_classes) {
          if (pilotClasses.Contains(unitClass)) { continue; }
          if (CanHaveExpertise(pilotClasses, unitClass) == false) { can_drive_unit = false; break; }
        }
        if (can_drive_unit) { result.Add(pilot); }
      }
      return result;
    }
    public static void GenerateAdditionalExpertises(PilotDef pilot, PilotingClassDef mainClass, HashSet<PilotingClassDef> classes) {
      HashSet<PilotingClassDef> additionalClasses = new HashSet<PilotingClassDef>();
      List<PilotingClassDef> avaibleClasses = new List<PilotingClassDef>();
      float roll_border = 0f;
      foreach(var addclass in mainClass.additionalClasses) {
        if(classes.Contains(addclass)) { continue; }
        if(CanHaveExpertise(classes, addclass) == false) { continue; }
        avaibleClasses.Add(addclass);
        roll_border += addclass.expertiseGenerationChance;
      }
      avaibleClasses.Sort((a,b)=> { return a.expertiseGenerationChance.CompareTo(b.expertiseGenerationChance); });
      if(avaibleClasses.Count <= mainClass.additionalExpertisesCount) {
        foreach(var addclass in avaibleClasses) {
          if(CanHaveExpertise(classes, addclass) == false) { continue; }
          classes.Add(addclass);
          pilot.AddPilotingClass(addclass);
        }
        return;
      }
      for(int roll_index = 0; roll_index < mainClass.additionalExpertisesCount; ++roll_index) {
        float roll = UnityEngine.Random.Range(0f, roll_border);
        foreach(var addclass in avaibleClasses) {
          roll -= addclass.expertiseGenerationChance;
          if(roll > 0f) { continue; }
          classes.Add(addclass);
          pilot.AddPilotingClass(addclass);
          break;
        }
        avaibleClasses.Clear();
        roll_border = 0f;
        foreach(var addclass in mainClass.additionalClasses) {
          if(classes.Contains(addclass)) { continue; }
          if(CanHaveExpertise(classes, addclass) == false) { continue; }
          avaibleClasses.Add(addclass);
          roll_border += addclass.expertiseGenerationChance;
        }
      }
    }
    public static void GeneratePilotingExpertises(ref List<PilotDef> pilots, bool minCount = true) {
      foreach(PilotDef pilot in pilots) {
        HashSet<PilotingClassDef> classes = pilot.GetPilotingClass();
        foreach (var pilotClass in pilotingClasses) {
          if (classes.Contains(pilotClass.Value)) { continue; }
          if (CanHaveExpertise(classes, pilotClass.Value) == false) { continue; }
          float roll = UnityEngine.Random.Range(0f, 1f);
          if (roll < pilotClass.Value.expertiseGenerationChance) {
            pilot.AddPilotingClass(pilotClass.Value);
            classes.Add(pilotClass.Value);
            if(pilotClass.Value.additionalExpertisesCount > 0) {

            }
          }
        }
      }
      Dictionary<PilotingClassDef, int> classes_count = new Dictionary<PilotingClassDef, int>();
      foreach (var pilotClass in pilotingClasses) {
        if (pilotClass.Value.expertiseGenerationMinCount > 0) { classes_count.Add(pilotClass.Value, 0); }
      }
      foreach (PilotDef pilot in pilots) {
        HashSet<PilotingClassDef> classes = pilot.GetPilotingClass();
        foreach (PilotingClassDef pclass in classes) {
          if (pclass.expertiseGenerationMinCount == 0) { continue; }
          classes_count[pclass] += 1;
        }
      }
      if (minCount == false) { return; }
      RemoveReadyClasses(ref classes_count);
      int watchdog = 0;
      while(classes_count.Count > 0) {
        HashSet<PilotingClassDef> classes_count_keys = new HashSet<PilotingClassDef>();
        foreach (PilotingClassDef pclass in classes_count.Keys) { classes_count_keys.Add(pclass); }
        foreach (PilotingClassDef pclass in classes_count_keys) {
          List<PilotDef> readypilots = gatherPilotsCanHaveClass(ref pilots, pclass);
          if (readypilots.Count == 0) { classes_count[pclass] = pclass.expertiseGenerationMinCount; continue; }
          int index = UnityEngine.Random.Range(0, readypilots.Count);
          readypilots[index].AddPilotingClass(pclass);
          classes_count[pclass] += 1;
        }
        RemoveReadyClasses(ref classes_count);
        ++watchdog;
        if (watchdog > 100) { break; }
      }
      foreach (PilotDef pilot in pilots) {
        HashSet<PilotingClassDef> classes = pilot.GetPilotingClass();
        if (classes.Count == 0) { pilot.AddPilotingClass(PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS); }
      }
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotDef_FromJSON {
    public static void fallbackPiloting(this PilotDef __instance) {
      try {
        //Log.TWL(0, "PilotDef.fallbackPiloting " + __instance.Description.Id + ":" + __instance.Description.Callsign);
        if (__instance.PilotTags == null) { Traverse.Create(__instance).Property<TagSet>("PilotTags").Value = new TagSet(); }
        //Log.W(1, "tags:"); foreach (string tag in __instance.PilotTags) { Log.W(1, tag); }; Log.WL(0, "");
        //Log.WL(1,"default mech:"+(PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS==null?"null": PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.Description.Id));
        //if(PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags != null) {
          //Log.W(2, "tags:"); foreach (string tag in PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags) { Log.W(1, tag); }; Log.WL(0, "");
        //}
        //Log.WL(1, "default vehicle:" + (PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS == null ? "null" : PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.Description.Id));
        //if (PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.pilotTags != null) {
          //Log.W(2, "tags:"); foreach (string tag in PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.pilotTags) { Log.W(1, tag); }; Log.WL(0, "");
        //}
        HashSet<PilotingClassDef> classes = __instance.GetPilotingClass();
        if (classes.Count == 0) {
          if (__instance.PilotTags.Contains(Core.Settings.CannotPilotMechTag) == false) {
            __instance.PilotTags.UnionWith(PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags);
            //Log.W(2, "adding tags:"); foreach (string tag in PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags) { Log.W(1, tag); }; Log.WL(0, "");
          } else if (__instance.PilotTags.Contains(Core.Settings.CanPilotVehicleTag) == false) {
            __instance.PilotTags.UnionWith(PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags);
            //Log.W(2, "adding tags:"); foreach (string tag in PilotingClassHelper.DEFAULT_MECH_PILOTING_CLASS.pilotTags) { Log.W(1, tag); }; Log.WL(0, "");
          }
          if (__instance.PilotTags.Contains(Core.Settings.CanPilotVehicleTag)) {
            //Log.W(2, "adding tags:"); foreach (string tag in PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.pilotTags) { Log.W(1, tag); }; Log.WL(0, "");
            __instance.PilotTags.UnionWith(PilotingClassHelper.DEFAULT_VEHICLE_PILOTING_CLASS.pilotTags);
          }
        }
        //Log.W(1, "finall tags:"); foreach (string tag in __instance.PilotTags) { Log.W(1, tag); }; Log.WL(0, "");
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString());
        UnityGameInstance.BattleTechGame.DataManager?.logger.LogException(e);
      }
    }
    public static void Postfix(PilotDef __instance) {
      //Log.TWL(0, "PilotGenerator.FromJSON "+__instance.Description.Id);
      __instance.fallbackPiloting();
    }
  }
  [HarmonyPatch(typeof(PilotDef))]
  [HarmonyPatch("PostDeserialzation")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotDef_PostDeserialzation {
    public static void Postfix(PilotDef __instance) {
      //Log.TWL(0, "PilotGenerator.PostDeserialzation " + __instance.Description.Id);
      __instance.fallbackPiloting();
    }
  }
  [HarmonyPatch(typeof(SGCharacterCreationWidget))]
  [HarmonyPatch("CreatePilot")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SGCharacterCreationWidget_CreatePilot {
    public static void Postfix(SGCharacterCreationWidget __instance,ref Pilot __result) {
      Log.M?.TWL(0, "SGCharacterCreationWidget.CreatePilot " + __result.pilotDef.Description.Callsign);
      try {
        Log.M?.WL(1, "tags before:");
        foreach (string tag in __result.pilotDef.PilotTags) {
          Log.M?.WL(2, tag);
        }
        __result.pilotDef.fallbackPiloting();
        Log.M?.WL(1, "tags after:");
        foreach (string tag in __result.pilotDef.PilotTags) {
          Log.M?.WL(2, tag);
        }
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("FirstTimeInitializeDataFromDefs")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_FirstTimeInitializeDataFromDefs {
    public static void Prefix(SimGameState __instance) {
      PilotingClassHelper.pushInStartingPilotsGen();
    }
    public static void Postfix(SimGameState __instance) {
      PilotingClassHelper.popInStartingPilotsGen();
      __instance.GeneratePilotingExpertisesStart();
    }
  }
  [HarmonyPatch(typeof(SimGameState))]
  [HarmonyPatch("OnCareerModeCharacterCreationComplete")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SimGameState_OnCareerModeCharacterCreationComplete {
    public static void Prefix(SimGameState __instance,ref Pilot p) {
      HashSet<PilotingClassDef> pilotingClasses = __instance.Commander.pilotDef.GetPilotingClass();
      foreach (PilotingClassDef pilotingClass in pilotingClasses) { p.pilotDef.AddPilotingClass(pilotingClass); }
    }
  }
  [HarmonyPatch(typeof(PilotGenerator))]
  [HarmonyPatch("GeneratePilots")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotGenerator_GeneratePilots {
    public static void Postfix(PilotGenerator __instance, ref List<PilotDef> __result) {
      Log.M?.TWL(0, "PilotGenerator.GeneratePilots");
      if (__result == null) { return; }
      if (__result.Count == 0) { return; }
      if (PilotingClassHelper.isInStartingPilotsGen()) { return; }
      PilotingClassHelper.GeneratePilotingExpertises(ref __result);
    }
  }
  public class PilotingClassDef {
    public static readonly string DEFAULT_MECH_PILOTING_NAME = "GenericMechClass";
    public static readonly string DEFAULT_VEHICLE_PILOTING_NAME = "GenericVehicleClass";
    public static readonly string DEFAULT_MECH_PILOTING_UINAME = "__/PILOTING.MECH/__";
    public static readonly string DEFAULT_VEHICLE_PILOTING_UINAME = "__/PILOTING.VEHICLE/__";
    public static readonly string CAN_PILOT_TITLE = "__/PILOTING.CAN_PILOT/__";
    public static readonly string DEFAULT_MECH_UNIT_TAG = "unit_piloting_type_generic_mech";
    public static readonly string DEFAULT_VEHICLE_UNIT_TAG = "unit_piloting_type_generic_vehicle";
    public static readonly string DEFAULT_MECH_PILOTING_TAG = "can_pilot_generic_mech";
    public static readonly string DEFAULT_VEHICLE_PILOTING_TAG = "can_pilot_generic_vehicle";
    public TagSet unitTags { get; set; } = new TagSet();
    public List<string> UnitTags { set { unitTags = new TagSet(value); } }
    public TagSet pilotTags { get; set; } = new TagSet();
    public List<string> PilotTags { set { pilotTags = new TagSet(value); } }
    public DropDescriptionDef Description { get; set; } = new DropDescriptionDef();
    [JsonIgnore]
    private BaseDescriptionDef f_description = null;
    [JsonIgnore]
    public BaseDescriptionDef description {
      get {
        if (f_description == null) { f_description = new BaseDescriptionDef(Description.Id, Description.Name, Description.Details, Description.Icon); }
        return f_description;
      }
    }
    public float expertiseGenerationChance { get; set; }
    public int expertiseGenerationMinCount { get; set; }
    public int additionalExpertisesCount { get; set; }
    [JsonIgnore]
    public HashSet<PilotingClassDef> excludeClasses { get; private set; } = new HashSet<PilotingClassDef>();
    public List<string> ExcludeClasses { get; set; } = new List<string>();
    [JsonIgnore]
    public HashSet<PilotingClassDef> additionalClasses { get; private set; } = new HashSet<PilotingClassDef>();
    public List<string> AdditionalClasses { get; set; } = new List<string>();
    public bool isMyUnitClass(TagSet tags) { return tags.ContainsAll(this.unitTags); }
    public bool isMyPilotClass(TagSet tags) { return tags.ContainsAll(this.pilotTags); }
  }
}