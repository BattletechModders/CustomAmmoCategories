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
      Log.Combat?.TWL(0, "overrall sinked heat "+mech.DisplayName+" "+ heat.Value<float>()+"->"+ (heat.Value<float>() + value));
      heat.SetValue<float>(heat.Value<float>() + value);
    }
    public static void Prefix(ref bool __runOriginal, Mech __instance, int stackID) {
      if (__runOriginal == false) { return; }
      try {
        Log.HS?.TWL(0, "Mech.ApplyHeatSinks round:" + __instance.Combat.TurnDirector.CurrentRound + " phase: " + __instance.Combat.TurnDirector.CurrentPhase + " " + __instance.DisplayName + ":" + __instance.GUID);
        if (__instance.isHasHeat() == false) {
          __instance._heat = 0;
          Log.HS?.WL(1, "new HeatValue = " + __instance.CurrentHeat);
          __instance.HasAppliedHeatSinks = false;
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
          Log.HS.WL(1, "effective heatsinkCapacity = " + heatsinkCapacity);
          Log.HS.WL(1, "usedHeatSinkCap = " + (usedHeatSinkCap * mod));
          Log.HS.WL(1, "heat to sink = " + __instance.CurrentHeat);

          if (currentHeat >= 0) {
            __instance.AddSinkedHeat(heatsinkCapacity);
            __instance.UsedHeatSinksCap(Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() + (float)heatsinkCapacity / mod));
          };
          if (currentHeat < 0) {
            __instance.AddSinkedHeat(__instance.CurrentHeat);
            __instance.UsedHeatSinksCap(__instance.UsedHeatSinksCap() + Mathf.RoundToInt((float)__instance.CurrentHeat / mod));
            currentHeat = 0;
          }
          Log.HS.WL(1, "result heat = " + currentHeat);
          if (CustomAmmoCategories.Settings.ShowApplyHeatSinkMessage) {
            Text message = new Text(CustomAmmoCategories.Settings.ApplyHeatSinkMessageTemplate, __instance.CurrentHeat, currentHeat, Mathf.RoundToInt(usedHeatSinkCap * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod), Mathf.RoundToInt((float)__instance.UsedHeatSinksCap() * mod));
            __instance.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, message, FloatieMessage.MessageNature.Buff, false)));
          }
          __instance._heat = currentHeat;
          Log.HS?.WL(1,"new HeatValue = " + __instance.CurrentHeat);
          __instance.HasAppliedHeatSinks = false;
          __instance.ReconcileHeat(stackID, __instance.GUID);
          Log.HS?.WL(1, "new usedHeatSinks = " + __instance.UsedHeatSinksCap());
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        CombatGameState.gameInfoLogger.LogException(e);
      }
      __runOriginal = false;
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
      Log.HS?.TWL(0, "AbstractActor.OnActivationBegin round:" + __instance.Combat.TurnDirector.CurrentRound + " phase: " + __instance.Combat.TurnDirector.CurrentPhase + " " + __instance.DisplayName + ":" + __instance.GUID + " HasBegunActivation:" + __instance.HasBegunActivation);
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
    public static void Prefix(ref bool __runOriginal, Mech __instance) {
      if (__instance.isHasHeat() == false) { __runOriginal = false;  return; }
      return;
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddExternalHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
  public static class Mech_AddExternalHeat {
    private static Dictionary<Mech, int> extHeat = new Dictionary<Mech, int>();
    public static void Clear() { extHeat.Clear(); }
    public static void AddExtHeatCustom(this Mech mech, int amount) {
      if (extHeat.ContainsKey(mech)) { extHeat[mech] += amount; } else { extHeat[mech] = amount; }
    }
    public static int GetExtHeatCustom(this Mech mech) {
      if (extHeat.TryGetValue(mech, out int heat)) {
        extHeat[mech] = 0;
        return heat;
      }
      return 0;
    }
    public static void Prefix(ref bool __runOriginal, Mech __instance, string reason, int amt) {
      __runOriginal = false;
      if (__instance.isHasHeat() == false) { return; }
      if (Mech.heatLogger.IsLogEnabled)
      __instance.AddExtHeatCustom(amt);
      __instance._tempHeat += amt;
      Mech.heatLogger.Log($"Mech {__instance.MechDef.ChassisID} gains {amt} heat. Reason: {reason}. Temp heat:{__instance._tempHeat} ExternalHeat:{__instance.GetExtHeatCustom()}");
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddTempHeat")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_AddTempHeat {
    public static void Prefix(ref bool __runOriginal, Mech __instance) {
      if (__instance.isHasHeat() == false) { return; }
      int extHeat = __instance.GetExtHeatCustom();
      __instance._tempHeat -= extHeat;
      Log.HS?.TWL(0, $"Mech.AddTempHeat {__instance.PilotableActorDef.ChassisID} external heat:{extHeat} internal heat:{__instance._tempHeat}");
      if (extHeat == 0) { return; }
      if(CustomAmmoCategories.Settings.ExternalHeatLimit > 0) {
        int projectedHeat = extHeat + __instance._heat - __instance.AdjustedHeatsinkCapacity;
        if (projectedHeat > CustomAmmoCategories.Settings.ExternalHeatLimit) {
          extHeat = CustomAmmoCategories.Settings.ExternalHeatLimit - (__instance._heat - __instance.AdjustedHeatsinkCapacity);
        }
        if (extHeat < 0) { extHeat = 0; }
        Log.HS?.TWL(0,$"Limit external heat {__instance.PilotableActorDef.ChassisID} extheat:{extHeat}");
      }
      __instance._heat += extHeat;
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