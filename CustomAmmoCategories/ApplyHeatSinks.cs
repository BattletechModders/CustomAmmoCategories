using BattleTech;
using Harmony;
using System;
using CustAmmoCategories;
using UnityEngine;
using System.Reflection;
using CustomAmmoCategoriesLog;
using Localize;
using System.Collections.Generic;

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
    public static void UsedHeatSinksCap(this Mech mech, int heatSinksCap) {
      if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
        mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, 0);
      }
      mech.StatCollection.Set<int>(CustomAmmoCategories.UsesHeatSinkCap, heatSinksCap);
      Log.HS.TWL(0, mech.DisplayName+ "UsedHeatSinksCap=>"+ heatSinksCap);
    }
    //public static void addUsedHeatSinksCap(this Mech mech, int heatSinks) {
    //  if (mech.StatCollection.ContainsStatistic(CustomAmmoCategories.UsesHeatSinkCap) == false) {
    //    mech.StatCollection.AddStatistic<int>(CustomAmmoCategories.UsesHeatSinkCap, heatSinks);
    //  } else {
    //    int curValue = mech.UsedHeatSinksCap();
    //    mech.StatCollection.Set<int>(CustomAmmoCategories.UsesHeatSinkCap, curValue + heatSinks);
    //  }
    //}
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyHeatSinks")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class Mech_ApplyHeatSinks {
    public static void AddSinkedHeat(this Mech mech,float value) {
      Statistic heat = mech.StatCollection.GetOrCreateStatisic<float>(CustomAmmoCategories.Settings.ApplyHeatSinkActorStat, 0f);
      Log.M.TWL(0, "overrall sinked heat "+mech.DisplayName+" "+ heat.Value<float>()+"->"+ (heat.Value<float>() + value));
      heat.SetValue<float>(heat.Value<float>() + value);
    }
    public static bool Prefix(Mech __instance, int stackID) {
      Log.HS.TWL(0,"Mech.ApplyHeatSinks round:"+ __instance.Combat.TurnDirector.CurrentRound + " phase: "+ __instance.Combat.TurnDirector.CurrentPhase + " " + __instance.DisplayName + ":" + __instance.GUID);
      float mod = 1f;
      if(__instance.occupiedDesignMask != null && !Mathf.Approximately(__instance.occupiedDesignMask.heatSinkMultiplier, 1f)) {
        mod *= __instance.occupiedDesignMask.heatSinkMultiplier;
      }
      if(__instance.Combat.MapMetaData.biomeDesignMask != null && !Mathf.Approximately(__instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier, 1f)) {
        mod *= __instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier;
      }
      mod *= __instance.Combat.Constants.Heat.GlobalHeatSinkMultiplier;

      int heatsinkCapacity = Mathf.RoundToInt(((float)__instance.HeatSinkCapacity - (float)__instance.UsedHeatSinksCap()) * mod);
      if (heatsinkCapacity < 0) { heatsinkCapacity = 0; }
      int currentHeat = __instance.CurrentHeat - heatsinkCapacity;
      float usedHeatSinkCap = __instance.UsedHeatSinksCap();
      Log.HS.WL(1, "effective heatsinkCapacity = " + heatsinkCapacity + "\n");
      Log.HS.WL(1, "usedHeatSinkCap = " + (usedHeatSinkCap * mod) + "\n");
      Log.HS.WL(1, "heat to sink = " + __instance.CurrentHeat + "\n");

      if(currentHeat >= 0) {
        __instance.AddSinkedHeat(heatsinkCapacity);
        __instance.UsedHeatSinksCap(Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() + (float)heatsinkCapacity / mod));
      };
      if(currentHeat < 0) {
        __instance.AddSinkedHeat(__instance.CurrentHeat);
        __instance.UsedHeatSinksCap(__instance.UsedHeatSinksCap() + Mathf.RoundToInt((float)__instance.CurrentHeat / mod));
        currentHeat = 0;
      }
      Log.HS.WL(1, "result heat = " + currentHeat + "\n");
      if (CustomAmmoCategories.Settings.ShowApplyHeatSinkMessage) { 
        Text message = new Text(CustomAmmoCategories.Settings.ApplyHeatSinkMessageTemplate, __instance.CurrentHeat, currentHeat, Mathf.RoundToInt(usedHeatSinkCap * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod));
        __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, message, FloatieMessage.MessageNature.Buff, false)));
      }
      typeof(Mech).GetProperty("_heat", BindingFlags.Instance|BindingFlags.NonPublic).GetSetMethod(true).Invoke(__instance,new object[1] { (object) currentHeat });
      CustomAmmoCategoriesLog.Log.LogWrite(" new HeatValue = " + __instance.CurrentHeat + "\n");
      typeof(Mech).GetProperty("HasAppliedHeatSinks", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
      __instance.ReconcileHeat(stackID, __instance.GUID);
      Log.HS.WL(1, "new usedHeatSinks = " + __instance.UsedHeatSinksCap() + "\n");
      return false;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AdjustedHeatsinkCapacity")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_HeatSinkCapacity {
    public static void Postfix(Mech __instance, ref int __result) {
      float num = 1f;
      if(__instance.occupiedDesignMask != null && !Mathf.Approximately(__instance.occupiedDesignMask.heatSinkMultiplier, 1f))
        num *= __instance.occupiedDesignMask.heatSinkMultiplier;
      if(__instance.Combat.MapMetaData.biomeDesignMask != null && !Mathf.Approximately(__instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier, 1f))
        num *= __instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier;
      if (__instance.isUsedHeatSinkReseted()) {
        __result = (int)((double)(__instance.HeatSinkCapacity - __instance.UsedHeatSinksCap()) * (double)(num * __instance.Combat.Constants.Heat.GlobalHeatSinkMultiplier));
      } else {
        __result = (int)((double)(__instance.HeatSinkCapacity) * (double)(num * __instance.Combat.Constants.Heat.GlobalHeatSinkMultiplier));
      }
      if (__result < 0) { __result = 0; }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnActivationBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class AbstractActor_OnActivationBegin {
    private static HashSet<Mech> heatSinksReseted = new HashSet<Mech>();
    public static void SetUsedHeatSinkReseted(this Mech mech) { heatSinksReseted.Add(mech); }
    public static void ClearUsedHeatSinkReseted(this Mech mech) { heatSinksReseted.Remove(mech); }
    public static bool isUsedHeatSinkReseted(this Mech mech) { return heatSinksReseted.Contains(mech); }
    public static void Prefix(AbstractActor __instance, string sourceID, int stackItemID) {
      Log.HS.TWL(0, "AbstractActor.OnActivationBegin round:" + __instance.Combat.TurnDirector.CurrentRound + " phase: " + __instance.Combat.TurnDirector.CurrentPhase + " " + __instance.DisplayName + ":" + __instance.GUID + " HasBegunActivation:" + __instance.HasBegunActivation);
      if(__instance.HasBegunActivation == false) {
        Mech mech = __instance as Mech;
        if (mech != null) {
          if (CustomAmmoCategories.Settings.ShowApplyHeatSinkMessage) {
            Text message = new Text(CustomAmmoCategories.Settings.ResetHeatSinkMessageTemplate, mech.UsedHeatSinksCap(), 0);
            __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, message, FloatieMessage.MessageNature.Buff, false)));
          }
          Log.HS.WL(1, "Reseting used heat sinks");
          mech.UsedHeatSinksCap(0);
          mech.SetUsedHeatSinkReseted();
        }
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("OnNewRound")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class Mech_OnNewRound {
    public static void Postfix(Mech __instance, int round) {
      Log.HS.TWL(0, "Mech.OnNewRound round:" + __instance.Combat.TurnDirector.CurrentRound + " " + __instance.DisplayName+" GUID:"+__instance.GUID);
      Log.HS.WL(1, "set mech can reset used heatsinks");
      __instance.ClearUsedHeatSinkReseted();
    }
  }
  //[HarmonyPatch(typeof(AbstractActor))]
  //[HarmonyPatch("OnPhaseBegin")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
  //public static class Mech_OnPhaseBegin{
  //  public static void Postfix(Mech __instance, int round, int phase){
  //    Log.M.TWL(0, "Mech.OnPhaseBegin " + __instance.DisplayName + " IsInterleaved:" + __instance.Combat.TurnDirector.IsInterleaved+ " IsAvailableOnPhase:" + __instance.IsAvailableOnPhase(phase));
  //    if(__instance.Combat.TurnDirector.IsInterleaved) {
  //      if(__instance.IsAvailableOnPhase(phase)) {
  //        __instance.UsedHeatSinksCap(0);
  //        Log.M.WL(1, "Reseting used heatsinks");
  //      }
  //    }
  //  }
  //}
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("GetHeatSequence")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(bool), typeof(bool), typeof(string) })]
  public static class Mech_GetHeatSequence {
    public static void Prefix(Mech __instance, int rootSequenceGUID, ref bool performHeatSinkStep, bool applyStartupHeatSinks, string instigatorID) {
      if (applyStartupHeatSinks == false) { performHeatSinkStep = true; }
      Log.HS.TWL(0, "Mech.GetHeatSequence "+__instance.DisplayName+ " tie performHeatSinkStep to true");
    }
  }
}