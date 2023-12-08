using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(CombatGameState), typeof(MechComponentRef), typeof(string) })]
  public static class MechComponent_Constructor_Mech {
    public static void Postfix(MechComponent __instance, Mech parent, CombatGameState combat, MechComponentRef mcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Vehicle), typeof(CombatGameState), typeof(VehicleComponentRef), typeof(string) })]
  public static class MechComponent_Constructor_Vehicle {
    public static void Postfix(MechComponent __instance, Vehicle parent, CombatGameState combat, VehicleComponentRef vcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechComponent))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Turret), typeof(CombatGameState), typeof(TurretComponentRef), typeof(string) })]
  public static class MechComponent_Constructor_Turret {
    public static void Postfix(MechComponent __instance, Turret parent, CombatGameState combat, TurretComponentRef tcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBox))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(MechComponentRef), typeof(string) })]
  public static class AmmunitionBox_Constructor_Mech {
    public static void Postfix(AmmunitionBox __instance, Mech parent, MechComponentRef mcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBox))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Vehicle), typeof(VehicleComponentRef), typeof(string) })]
  public static class AmmunitionBox_Constructor_Vehicle {
    public static void Postfix(AmmunitionBox __instance, Vehicle parent, VehicleComponentRef vcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AmmunitionBox))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Turret), typeof(TurretComponentRef), typeof(string) })]
  public static class AmmunitionBox_Constructor_Turret {
    public static void Postfix(AmmunitionBox __instance, Turret parent, TurretComponentRef tcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(Jumpjet))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(MechComponentRef), typeof(string) })]
  public static class Jumpjet_Constructor_Mech {
    public static void Postfix(Jumpjet __instance, Mech parent, MechComponentRef mcRef, string UID) {
      try {
        if(__instance.componentDef == null) { return; }
        if(__instance.componentDef.ComponentTags == null) { return; }
        if(__instance.componentDef.ComponentTags.Contains("move_to_none_location")) {
          __instance.locationDef = new LocationDef();
          __instance.location = 0;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
  }
}