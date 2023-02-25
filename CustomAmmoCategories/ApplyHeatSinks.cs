/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using HarmonyLib;
using System;
using CustAmmoCategories;
using UnityEngine;
using System.Reflection;
using CustomAmmoCategoriesLog;
using Localize;
using System.Collections.Generic;
using CustomAmmoCategoriesPatches;

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
      if (__instance.isHasHeat() == false) {
        typeof(Mech).GetProperty("_heat", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true).Invoke(__instance, new object[1] { (object)0 });
        CustomAmmoCategoriesLog.Log.LogWrite(" new HeatValue = " + __instance.CurrentHeat + "\n");
        typeof(Mech).GetProperty("HasAppliedHeatSinks", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
        __instance.ReconcileHeat(stackID, __instance.GUID);
      } else {
        float mod = 1f;
        if (__instance.occupiedDesignMask != null && !Mathf.Approximately(__instance.occupiedDesignMask.heatSinkMultiplier, 1f)) {
          mod *= __instance.occupiedDesignMask.heatSinkMultiplier;
        }
        if (__instance.Combat.MapMetaData.biomeDesignMask != null && !Mathf.Approximately(__instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier, 1f)) {
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

        if (currentHeat >= 0) {
          __instance.AddSinkedHeat(heatsinkCapacity);
          __instance.UsedHeatSinksCap(Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() + (float)heatsinkCapacity / mod));
        };
        if (currentHeat < 0) {
          __instance.AddSinkedHeat(__instance.CurrentHeat);
          __instance.UsedHeatSinksCap(__instance.UsedHeatSinksCap() + Mathf.RoundToInt((float)__instance.CurrentHeat / mod));
          currentHeat = 0;
        }
        Log.HS.WL(1, "result heat = " + currentHeat + "\n");
        if (CustomAmmoCategories.Settings.ShowApplyHeatSinkMessage) {
          Text message = new Text(CustomAmmoCategories.Settings.ApplyHeatSinkMessageTemplate, __instance.CurrentHeat, currentHeat, Mathf.RoundToInt(usedHeatSinkCap * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod));
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, message, FloatieMessage.MessageNature.Buff, false)));
        }
        typeof(Mech).GetProperty("_heat", BindingFlags.Instance | BindingFlags.NonPublic).GetSetMethod(true).Invoke(__instance, new object[1] { (object)currentHeat });
        CustomAmmoCategoriesLog.Log.LogWrite(" new HeatValue = " + __instance.CurrentHeat + "\n");
        typeof(Mech).GetProperty("HasAppliedHeatSinks", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
        __instance.ReconcileHeat(stackID, __instance.GUID);
        Log.HS.WL(1, "new usedHeatSinks = " + __instance.UsedHeatSinksCap() + "\n");
      }
      return false;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AdjustedHeatsinkCapacity")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] {  })]
  public static class Mech_HeatSinkCapacity {
    public static void Postfix(Mech __instance, ref int __result) {
      if (__instance.isHasHeat() == false) { __result = 99999; return; }
      float num = 1f;
      if(__instance.occupiedDesignMask != null && !Mathf.Approximately(__instance.occupiedDesignMask.heatSinkMultiplier, 1f))
        num *= __instance.occupiedDesignMask.heatSinkMultiplier;
      if(__instance.Combat.MapMetaData.biomeDesignMask != null && !Mathf.Approximately(__instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier, 1f))
        num *= __instance.Combat.MapMetaData.biomeDesignMask.heatSinkMultiplier;

      float HeatSinkCapacityMult = __instance.StatCollection.GetOrCreateStatisic<float>(CustomAmmoCategories.HeatSinkCapacityMultActorStat, 1f).Value<float>();
      if (Mathf.Approximately(HeatSinkCapacityMult, 1f) == false) {
        num *= HeatSinkCapacityMult;
      }

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
          if (mech.isHasHeat()) {
            if (CustomAmmoCategories.Settings.ShowApplyHeatSinkMessage) {
              Text message = new Text(CustomAmmoCategories.Settings.ResetHeatSinkMessageTemplate, mech.UsedHeatSinksCap(), 0);
              __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, message, FloatieMessage.MessageNature.Buff, false)));
            }
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
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddWeaponHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Weapon), typeof(int) })]
  public static class Mech_AddWeaponHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddExternalHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class Mech_AddExternalHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddWalkHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddWalkHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddSprintHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddSprintHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddJumpHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class Mech_AddJumpHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddEngineDamageHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddEngineDamageHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddEnvironmentHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddEnvironmentHeat {
    public static bool Prefix(Mech __instance) {
      if (__instance.isHasHeat() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(OrderSequence))]
  [HarmonyPatch("OnUpdate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class OrderSequence_OnUpdate {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(TurnDirector), "IsInterleaved").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(OrderSequence_OnUpdate), nameof(IsInterleaved));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    public static bool IsInterleaved(this TurnDirector turnDirector) {
      Log.M.TWL(0, "OrderSequence.OnUpdate.TurnDirector.IsInterleaved");
      return true;
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