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
using CustAmmoCategories;
using System;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

namespace CustomUnits {
  public static class LowVisibilityAPIHelper {
    public delegate void d_ObfuscateArmorAndStructText(AbstractActor target, TextMeshProUGUI armorHover, TextMeshProUGUI structHover);
    public static d_ObfuscateArmorAndStructText i_ObfuscateArmorAndStructText = null;
    public delegate void d_PilotableActorRepresentation_OnPlayerVisibilityChanged(PilotableActorRepresentation __instance, VisibilityLevel newLevel);
    public static d_PilotableActorRepresentation_OnPlayerVisibilityChanged i_PilotableActorRepresentation_OnPlayerVisibilityChanged = null;
    public static void Init() {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.GetName().Name.Contains("LowVisibility") == false) { continue; }
        Core.Settings.LowVisDetected = true;
        {
          Type type = assembly.GetType("LowVisibility.Patches.ArmorAndStructHelper");
          MethodInfo method = type.GetMethod("ObfuscateArmorAndStructText", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("LowVis_ObfuscateArmorAndStructText", null, new Type[] { typeof(AbstractActor), typeof(TextMeshProUGUI), typeof(TextMeshProUGUI) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Ldarg_2);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_ObfuscateArmorAndStructText = (d_ObfuscateArmorAndStructText)dm.CreateDelegate(typeof(d_ObfuscateArmorAndStructText));
        }
        {
          Type type = assembly.GetType("LowVisibility.Patch.PilotableActorRepresentation_OnPlayerVisibilityChanged");
          MethodInfo method = type.GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("LowVis_PilotableActorRepresentation_OnPlayerVisibilityChanged_Postfix", null, new Type[] { typeof(PilotableActorRepresentation), typeof(VisibilityLevel) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Ldarg_1);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_PilotableActorRepresentation_OnPlayerVisibilityChanged = (d_PilotableActorRepresentation_OnPlayerVisibilityChanged)dm.CreateDelegate(typeof(d_PilotableActorRepresentation_OnPlayerVisibilityChanged));
        }
      }
    }
    public static void SetArmorDisplayActive(CombatHUDTargetingComputer __instance, bool active) {
      CombatHUDTargetingComputerCustom computerCustom = __instance.gameObject.GetComponent<CombatHUDTargetingComputerCustom>();
      ICombatant DisplayedCombatant = __instance.ActivelyShownCombatant;
      //if (DisplayedCombatant == null) { DisplayedCombatant = __instance.MechArmorDisplay.DisplayedMech; }
      Log.Combat?.TWL(0, "LowVisibilityAPIHelper.SetArmorDisplayActive DisplayedCombatant:" + (DisplayedCombatant == null ? "null" : (DisplayedCombatant.PilotableActorDef.Description.Id + " fake:" + DisplayedCombatant.FakeVehicle())) + " computerCustom:" + (computerCustom == null ? "null" : "not null")+" active:"+active);
      if(active == false) {
        __instance.VehicleArmorDisplay.gameObject.SetActive(false);
        __instance.TurretArmorDisplay.gameObject.SetActive(false);
        __instance.BuildingArmorDisplay.gameObject.SetActive(false);
        __instance.MechArmorDisplay.gameObject.SetActive(false);
        if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
      } else {
        if (DisplayedCombatant is Mech mech) {
          if (mech.GetCustomInfo().TurretArmorReadout) {
            Log.Combat?.WL(1, "fake turret");
            __instance.MechArmorDisplay.gameObject.SetActive(false);
            __instance.VehicleArmorDisplay.gameObject.SetActive(false);
            __instance.TurretArmorDisplay.gameObject.SetActive(true);
            __instance.BuildingArmorDisplay.gameObject.SetActive(false);
            if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
          } else
          if (DisplayedCombatant.FakeVehicle() == false) {
            Log.Combat?.WL(1, "Mech");
            __instance.MechArmorDisplay.gameObject.SetActive(true);
            __instance.VehicleArmorDisplay.gameObject.SetActive(false);
            __instance.TurretArmorDisplay.gameObject.SetActive(false);
            __instance.BuildingArmorDisplay.gameObject.SetActive(false);
            if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
          } else {
            Log.Combat?.WL(1, "fake vehicle");
            __instance.MechArmorDisplay.gameObject.SetActive(false);
            __instance.VehicleArmorDisplay.gameObject.SetActive(false);
            __instance.TurretArmorDisplay.gameObject.SetActive(false);
            __instance.BuildingArmorDisplay.gameObject.SetActive(false);
            if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(true); }
          }
        }else
        if (DisplayedCombatant is Vehicle vehicle) {
          Log.Combat?.WL(1, "vehicle");
          __instance.MechArmorDisplay.gameObject.SetActive(false);
          __instance.VehicleArmorDisplay.gameObject.SetActive(true);
          __instance.TurretArmorDisplay.gameObject.SetActive(false);
          __instance.BuildingArmorDisplay.gameObject.SetActive(false);
          if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
        } else
        if (DisplayedCombatant is Turret turret) {
          Log.Combat?.WL(1, "turret");
          __instance.MechArmorDisplay.gameObject.SetActive(false);
          __instance.VehicleArmorDisplay.gameObject.SetActive(false);
          __instance.TurretArmorDisplay.gameObject.SetActive(true);
          __instance.BuildingArmorDisplay.gameObject.SetActive(false);
          if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
        } else
        if (DisplayedCombatant is BattleTech.Building building) {
          Log.Combat?.WL(1, "building");
          __instance.MechArmorDisplay.gameObject.SetActive(false);
          __instance.VehicleArmorDisplay.gameObject.SetActive(false);
          __instance.TurretArmorDisplay.gameObject.SetActive(false);
          __instance.BuildingArmorDisplay.gameObject.SetActive(true);
          if (computerCustom != null) { computerCustom.fakeVehicleReadout.gameObject.SetActive(false); }
        }
      }
      Log.Combat?.WL(1, "MechArmorDisplay:"+ __instance.MechArmorDisplay.gameObject.activeInHierarchy
        + " VehicleArmorDisplay:"+ __instance.VehicleArmorDisplay.gameObject.activeInHierarchy
        + " TurretArmorDisplay:"+ __instance.TurretArmorDisplay.gameObject.activeInHierarchy
        + " BuildingArmorDisplay:" + __instance.BuildingArmorDisplay.gameObject.activeInHierarchy
        + " fakeVehicleReadout:" + (computerCustom != null? computerCustom.fakeVehicleReadout.gameObject.activeInHierarchy:false)
      );
    }
    public static void LowVis_PilotableActorRepresentation_OnPlayerVisibilityChanged(PilotableActorRepresentation __instance, VisibilityLevel newLevel) {
      i_PilotableActorRepresentation_OnPlayerVisibilityChanged?.Invoke(__instance, newLevel);
    }
    public static void ObfuscateArmorAndStructText(AbstractActor target, TextMeshProUGUI armorHover, TextMeshProUGUI structHover) {
      i_ObfuscateArmorAndStructText?.Invoke(target, armorHover, structHover);
    }
  }
}