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
      Log.TWL(0, "LowVisibilityAPIHelper.SetArmorDisplayActive DisplayedCombatant:" + (DisplayedCombatant == null ? "null" : (DisplayedCombatant.PilotableActorDef.Description.Id + " fake:" + DisplayedCombatant.FakeVehicle())) + " computerCustom:" + (computerCustom == null ? "null" : "not null"));
      if (DisplayedCombatant is Mech mech) {
        if (computerCustom == null) { __instance.MechArmorDisplay.gameObject.SetActive(active); } else {
          if(active == true) {
            if (DisplayedCombatant.FakeVehicle() == false) {
              __instance.MechArmorDisplay.gameObject.SetActive(true);
              computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
            } else {
              __instance.MechArmorDisplay.gameObject.SetActive(false);
              computerCustom.fakeVehicleReadout.gameObject.SetActive(true);
            }
          } else { 
            __instance.MechArmorDisplay.gameObject.SetActive(false);
            computerCustom.fakeVehicleReadout.gameObject.SetActive(false);
          }
        }
      } else 
      if (DisplayedCombatant is Vehicle vehicle) { __instance.VehicleArmorDisplay.gameObject.SetActive(active); } else 
      if (DisplayedCombatant is Turret turret) { __instance.TurretArmorDisplay.gameObject.SetActive(active); } else 
      if (DisplayedCombatant is BattleTech.Building building) { __instance.BuildingArmorDisplay.gameObject.SetActive(active); }        
    }
    public static void LowVis_PilotableActorRepresentation_OnPlayerVisibilityChanged(PilotableActorRepresentation __instance, VisibilityLevel newLevel) {
      i_PilotableActorRepresentation_OnPlayerVisibilityChanged?.Invoke(__instance, newLevel);
    }
    public static void ObfuscateArmorAndStructText(AbstractActor target, TextMeshProUGUI armorHover, TextMeshProUGUI structHover) {
      i_ObfuscateArmorAndStructText?.Invoke(target, armorHover, structHover);
    }
  }
}