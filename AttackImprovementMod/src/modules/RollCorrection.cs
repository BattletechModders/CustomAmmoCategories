using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using CustomAmmoCategoriesLog;

namespace Sheepy.BattleTechMod.AttackImprovementMod {
  using static Mod;
  using static System.Reflection.BindingFlags;

  public class RollCorrection : BattleModModule {

    private static bool NoRollCorrection = false;
    private static bool TrueRNG = false;
    private static Dictionary<float, float> correctionCache;
    internal static string WeaponHitChanceFormat = "{0:0}%";

    private static float RollCorrectionStrength, MissStreakBreakerThreshold, MissStreakBreakerDivider;

    public override void CombatStartsOnce() {
      if (HasMod("Battletech.realitymachina.NoCorrections", "NoCorrectedRoll.InitClass")) {
        BattleMod.BTML_LOG.Warn(Mod.Name + " detected realitymachina's True RNG (NoCorrections) mod, roll correction and streak breaker disabled.");
        TrueRNG = true;
      }
      if (HasMod("aa.battletech.realhitchance", "RealHitChance.Loader")) {
        BattleMod.BTML_LOG.Warn(Mod.Name + " detected casualmods's Real Hit Chance mod, which should be REMOVED because it does not support AIM's features. Just remember to set AIM's ShowCorrectedHitChance to true!");
        AIMSettings.ShowCorrectedHitChance = true;
      }

      RollCorrectionStrength = (float)AIMSettings.RollCorrectionStrength;
      MissStreakBreakerThreshold = (float)AIMSettings.MissStreakBreakerThreshold;
      MissStreakBreakerDivider = (float)AIMSettings.MissStreakBreakerDivider;
      NoRollCorrection = RollCorrectionStrength == 0;

      if (!NoRollCorrection && !TrueRNG) {
        if (RollCorrectionStrength != 1)
          Patch(typeof(AttackDirector.AttackSequence), "GetCorrectedRoll", new Type[] { typeof(float), typeof(Team) }, "OverrideRollCorrection", null);
        if (AIMSettings.ShowCorrectedHitChance) {
          correctionCache = new Dictionary<float, float>(20);
          Patch(typeof(CombatHUDWeaponSlot), "SetHitChance", typeof(float), "ShowCorrectedHitChance", null);
        }
      } else if (AIMSettings.ShowCorrectedHitChance) {
        Info("ShowCorrectedHitChance auto-disabled because roll Correction is disabled.");
        AIMSettings.ShowCorrectedHitChance = false;
      }

      if ((MissStreakBreakerThreshold != 0.5f || MissStreakBreakerDivider != 5) && !TrueRNG) {
        if (MissStreakBreakerThreshold == 1 || MissStreakBreakerDivider == 0)
          Patch(typeof(Team), "ProcessRandomRoll", new Type[] { typeof(float), typeof(bool) }, "BypassMissStreakBreaker", null);
        else
          Patch(typeof(Team), "ProcessRandomRoll", new Type[] { typeof(float), typeof(bool) }, "OverrideMissStreakBreaker", null);
      }

      if (AIMSettings.HitChanceFormat != null)
        WeaponHitChanceFormat = AIMSettings.HitChanceFormat;
      else if (AIMSettings.HitChanceStep == 0 && !AIMSettings.DiminishingHitChanceModifier)
        WeaponHitChanceFormat = "{0:0.#}%";

      bool HitChanceFormatChanged = AIMSettings.HitChanceFormat != null || (AIMSettings.HitChanceStep == 0 && AIMSettings.HitChanceFormat != "{0:0}%");
      if (HitChanceFormatChanged || AIMSettings.ShowCorrectedHitChance || AIMSettings.MinFinalHitChance < 0.05m || AIMSettings.MaxFinalHitChance > 0.95m) {
        HitChance = typeof(CombatHUDWeaponSlot).GetMethod("set_HitChance", Instance | NonPublic);
        Refresh = typeof(CombatHUDWeaponSlot).GetMethod("RefreshNonHighlighted", Instance | NonPublic);
        Patch(typeof(CombatHUDWeaponSlot), "SetHitChance", typeof(float), "OverrideDisplayedHitChance", null);
      }

      if (NoRollCorrection)
        UseWeightedHitNumbersProp = typeof(AttackDirector.AttackSequence).GetField("UseWeightedHitNumbers", Static | NonPublic);
    }

    FieldInfo UseWeightedHitNumbersProp;

    public override void CombatStarts() {
      if (NoRollCorrection) {
        if (UseWeightedHitNumbersProp != null)
          UseWeightedHitNumbersProp.SetValue(null, false);
        else
          Warn("Cannot find AttackDirector.AttackSequence.UseWeightedHitNumbers. Roll correction not disabled.");
      } else if (correctionCache != null)
        Info("Combat starts with {0} reverse roll correction cached from previous battles.", correctionCache.Count);
    }

    // ============ UTILS ============

    private static float CorrectRoll(float roll, float strength) {
      strength /= 2;
      return (float)((Math.Pow(1.6 * roll - 0.8, 3) + 0.5) * strength + roll * (1 - strength));
    }

    // A reverse algorithm of AttackDirector.GetCorrectedRoll
    internal static float ReverseRollCorrection(float target, float strength) {
      if (strength == 0.0f) return target;
      // Solving r for target = ((1.6r-0.8)^3+0.5)*(s/2)+r*(1-s/2)
      double t = target, t2 = t * t, s = strength, s2 = s * s, s3 = s2 * s,
             a = 125 * Math.Sqrt((13824 * t2 * s - 13824 * t * s - 125 * s3 + 750 * s2 + 1956 * s + 1000) / s),
             b = a / (4096 * Math.Pow(6, 3d / 2d) * s) + (250 * t - 125) / (1024 * s),
             c = Math.Pow(b, 1d / 3d);
      return c == 0 ? target : (float)(c + (125 * s - 250) / (1536 * s * c) + 0.5);
    }

    // ============ Fixes ============

    [HarmonyPriority(Priority.Low)]
    public static bool OverrideRealHitChance() { return false; }

    [HarmonyPriority(Priority.Low)]
    public static bool OverrideRollCorrection(ref float __result, float roll, Team team) {
      try {
        roll = CorrectRoll(roll, RollCorrectionStrength);
        if (team != null)
          roll -= team.StreakBreakingValue;
        __result = roll;
        return false;
      } catch (Exception ex) { return Error(ex); }
    }

    [HarmonyPriority(Priority.Low)]
    public static bool BypassMissStreakBreaker() {
      return false;
    }

    [HarmonyPriority(Priority.Low)]
    public static bool OverrideMissStreakBreaker(Team __instance, float targetValue, bool succeeded, ref float ___streakBreakingValue) {
      try {
        if (succeeded) {
          ___streakBreakingValue = 0f;

        } else if (targetValue > MissStreakBreakerThreshold) {
          float mod;
          if (MissStreakBreakerDivider > 0)
            mod = (targetValue - MissStreakBreakerThreshold) / MissStreakBreakerDivider;
          else
            mod = -MissStreakBreakerDivider;
          ___streakBreakingValue += mod;
        }
        return false;
      } catch (Exception ex) { return Error(ex); }
    }

    [HarmonyPriority(Priority.HigherThanNormal)] // Above alexanderabramov's Real Hit Chance mod
    public static void ShowCorrectedHitChance(CombatHUDWeaponSlot __instance,ref float chance) {
      try {
         chance = Mathf.Clamp(chance, 0f, 1f);
        if (!correctionCache.TryGetValue(chance, out float corrected))
          correctionCache.Add(chance, corrected = ReverseRollCorrection(chance, RollCorrectionStrength));
        chance = corrected;
        CustomAmmoCategoriesLog.Log.M.TWL(0, "ShowCorrectedHitChance:" + __instance.weaponSlotType+":"+chance);
      } catch (Exception ex) { Error(ex); }
    }

    private static MethodInfo HitChance, Refresh;

    // Override the original code to remove accuracy cap on display, since correction or other settings can push it above 95%.
    [HarmonyPriority(Priority.HigherThanNormal)] // Above alexanderabramov's Real Hit Chance mod
    public static bool OverrideDisplayedHitChance(CombatHUDWeaponSlot __instance, float chance) {
      try {
        HitChance.Invoke(__instance, new object[] { chance });
        __instance.HitChanceText.text = string.Format(WeaponHitChanceFormat, Mathf.Clamp(chance * 100f, 0f, 100f));
        Refresh.Invoke(__instance, null);
        return false;
      } catch (Exception ex) { return Error(ex); }
    }
  }
}