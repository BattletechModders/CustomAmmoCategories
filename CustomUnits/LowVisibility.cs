using BattleTech;
using BattleTech.UI;
using CustAmmoCategories;
using System;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

namespace CustomUnits {
  public static class CBTBehaviorsEnhancedAPIHelper {
    public delegate void d_AddDistanceMod(Func<Mech, float, float> postfix);
    private static d_AddDistanceMod i_AddMaxWalkDistanceMod = null;
    private static d_AddDistanceMod i_AddMaxBackwardDistanceMod = null;
    private static d_AddDistanceMod i_AddMaxSprintDistanceMod = null;
    private static d_AddDistanceMod i_AddMaxMeleeEngageRangeDistanceMod = null;
    public delegate float d_FinalSpeed(Mech mech);
    private static d_FinalSpeed i_FinalWalkSpeed = null;
    private static d_FinalSpeed i_FinalRunSpeed = null;
    public static void Init() {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.GetName().Name.Contains("CBTBehaviorsEnhanced") == false) { continue; }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("AddMaxWalkDistanceMod", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_AddMaxWalkDistanceMod", null, new Type[] { typeof(Func<Mech, float, float>) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_AddMaxWalkDistanceMod = (d_AddDistanceMod)dm.CreateDelegate(typeof(d_AddDistanceMod));
          i_AddMaxWalkDistanceMod.Invoke(Mech_MaxWalkDistance.MaxWalkDistanceMod);
        }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("AddMaxBackwardDistanceMod", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_AddMaxBackwardDistanceMod", null, new Type[] { typeof(Func<Mech, float, float>) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_AddMaxBackwardDistanceMod = (d_AddDistanceMod)dm.CreateDelegate(typeof(d_AddDistanceMod));
          i_AddMaxBackwardDistanceMod.Invoke(Mech_MaxWalkDistance.MaxBackwardDistanceMod);
        }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("AddMaxSprintDistanceMod", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_AddMaxSprintDistanceMod", null, new Type[] { typeof(Func<Mech, float, float>) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_AddMaxSprintDistanceMod = (d_AddDistanceMod)dm.CreateDelegate(typeof(d_AddDistanceMod));
          i_AddMaxSprintDistanceMod.Invoke(Mech_MaxWalkDistance.MaxSprintDistanceMod);
        }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("AddMaxMeleeEngageRangeDistanceMod", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_AddMaxMeleeEngageRangeDistanceMod", null, new Type[] { typeof(Func<Mech, float, float>) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_AddMaxMeleeEngageRangeDistanceMod = (d_AddDistanceMod)dm.CreateDelegate(typeof(d_AddDistanceMod));
          i_AddMaxMeleeEngageRangeDistanceMod.Invoke(Mech_MaxWalkDistance.MaxMeleeEngageRangeDistanceMod);
        }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("FinalWalkSpeed", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_FinalWalkSpeed", typeof(float), new Type[] { typeof(Mech) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_FinalWalkSpeed = (d_FinalSpeed)dm.CreateDelegate(typeof(d_FinalSpeed));
        }
        {
          Type type = assembly.GetType("CBTBehaviorsEnhanced.MechHelper");
          MethodInfo method = type.GetMethod("FinalRunSpeed", BindingFlags.Public | BindingFlags.Static);
          var dm = new DynamicMethod("CBTBE_FinalRunSpeed", typeof(float), new Type[] { typeof(Mech) });
          var gen = dm.GetILGenerator();
          gen.Emit(OpCodes.Ldarg_0);
          gen.Emit(OpCodes.Call, method);
          gen.Emit(OpCodes.Ret);
          i_FinalRunSpeed = (d_FinalSpeed)dm.CreateDelegate(typeof(d_FinalSpeed));
        }
        Core.Settings.CBTBEDetected = true;
      }
    }
    public static float FinalWalkSpeed(Mech mech) {
      if (i_FinalWalkSpeed == null) { return 0f; }
      return i_FinalWalkSpeed(mech);
    }
    public static float FinalRunSpeed(Mech mech) {
      if (i_FinalRunSpeed == null) { return 0f; }
      return i_FinalRunSpeed(mech);
    }
    public static void AddMaxWalkDistanceMod(Func<Mech, float, float> postfix) {
      i_AddMaxWalkDistanceMod?.Invoke(postfix);
    }
    public static void AddMaxBackwardDistanceMod(Func<Mech, float, float> postfix) {
      i_AddMaxBackwardDistanceMod?.Invoke(postfix);
    }
    public static void AddMaxSprintDistanceMod(Func<Mech, float, float> postfix) {
      i_AddMaxSprintDistanceMod?.Invoke(postfix);
    }
    public static void AddMaxMeleeEngageRangeDistanceMod(Func<Mech, float, float> postfix) {
      i_AddMaxMeleeEngageRangeDistanceMod?.Invoke(postfix);
    }
  }
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
          if (DisplayedCombatant.FakeVehicle() == false) { __instance.MechArmorDisplay.gameObject.SetActive(active); } 
          else { computerCustom.fakeVehicleReadout.gameObject.SetActive(active); }
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