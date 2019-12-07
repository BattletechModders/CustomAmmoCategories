using BattleTech;
using Harmony;
using System;
using CustAmmoCategories;
using UnityEngine;
using System.Reflection;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static readonly string UsesHeatSinkCap = "CACUsedHEatSinks";
    public static void clearUsedHeatSinksCap(this Mech mech) {
      if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
        mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, 0);
      } else {
        mech.StatCollection.Set<int>(CustomAmmoCategories.UsesHeatSinkCap, 0);
      }
    }
    public static int UsedHeatSinksCap(this Mech mech) {
      if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
        mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, 0);
        return 0;
      } else {
        return mech.StatCollection.GetStatistic(CustomAmmoCategories.UsesHeatSinkCap).Value<int>();
      }
    }
    public static void CurrentHeat(this Mech mech,int heat) {
      if (mech.StatCollection.ContainsStatistic("CurrentHeat") == false) {
        mech.StatCollection.AddStatistic<int>("CurrentHeat", heat);
      } else {
        mech.StatCollection.Set<int>("CurrentHeat",heat);
      }
    }
    public static void UsedHeatSinksCap(this Mech mech, int heatSinks) {
      if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
        mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, heatSinks);
      } else {
        mech.StatCollection.Set<int>(CustomAmmoCategories.UsesHeatSinkCap, heatSinks);
      }
    }
    public static void addUsedHeatSinksCap(this Mech mech, int heatSinks) {
      if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
        mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, heatSinks);
      } else {
        int curValue = mech.UsedHeatSinksCap();
        mech.StatCollection.Set<int>(CustomAmmoCategories.UsesHeatSinkCap, curValue + heatSinks);
      }
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyHeatSinks")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class Mech_ApplyHeatSinks {
    public static bool Prefix(Mech __instance, int stackID) {
      CustomAmmoCategoriesLog.Log.LogWrite("Mech.ApplyHeatSinks "+__instance.DisplayName+":"+__instance.GUID+"\n");
      int heatsinkCapacity = __instance.AdjustedHeatsinkCapacity;
      int baseHeatSinkCap = __instance.HeatSinkCapacity;
      int currentHeat = __instance.CurrentHeat - heatsinkCapacity;
      CustomAmmoCategoriesLog.Log.LogWrite(" heatsinkCapacity = "+ heatsinkCapacity+"\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" baseHeatSinkCap = " + baseHeatSinkCap + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" currentHeat = " + currentHeat + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" usedHeatSinks = " + __instance.UsedHeatSinksCap() + "\n");
      int usedHeatSinks = 0;
      if (currentHeat > 0) { usedHeatSinks = baseHeatSinkCap; };
      if (currentHeat < 0) {
        usedHeatSinks = Mathf.RoundToInt(((float)heatsinkCapacity + (float)currentHeat)*(float)baseHeatSinkCap / (float)heatsinkCapacity);
        currentHeat = 0;
      }
      typeof(Mech).GetProperty("_heat", BindingFlags.Instance|BindingFlags.NonPublic).GetSetMethod(true).Invoke(__instance,new object[1] { (object) currentHeat });
      CustomAmmoCategoriesLog.Log.LogWrite(" new HeatValue = " + __instance.CurrentHeat + "\n");
      typeof(Mech).GetProperty("HasAppliedHeatSinks", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[1] { (object)true });
      __instance.addUsedHeatSinksCap(usedHeatSinks);
      __instance.ReconcileHeat(stackID, __instance.GUID);
      CustomAmmoCategoriesLog.Log.LogWrite(" new usedHeatSinks = " + __instance.UsedHeatSinksCap() + "\n");
      return false;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("HeatSinkCapacity")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_HeatSinkCapacity {
    public static void Postfix(Mech __instance, ref int __result) {
      __result -= __instance.UsedHeatSinksCap();
      if (__result < 0) { __result = 0; }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("OnActivationEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int)  })]
  public static class Mech_OnActivationEnd {
    public static void Postfix(Mech __instance, string sourceID, int stackItemID) {
      __instance.clearUsedHeatSinksCap();
    }
  }
}