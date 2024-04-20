using BattleTech;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS.Collections;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustAmmoCategories { 
  public enum GenderGeneratorType {
    Vanilla, ModernArmy, ModernArmy2, ModernArmy3, RussianArmy, AmericanArmy, ChineseArmy, MenOnly, WomenOnly, NonbinaryOnly, MatriarchyArmy, PatriarchyArmy
  }
  public class GenderGeneratorCarrier {
    public GenderGeneratorType val = GenderGeneratorType.Vanilla;
    public GenderGeneratorCarrier(GenderGeneratorType v) { this.val = v; }
    public override string ToString() {
      return val.ToString();
    }
  }
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextGenderGenerator : CustomSettings.NextSettingValue {
    public static List<GenderGeneratorType> generators = null;
    public override void Next(object settings) {
      Log.M?.TWL(0, $"NextGenderGenerator.Next {settings.GetType()}");
      if (settings is CustAmmoCategories.Settings cacsettings) {
        int index = generators.IndexOf(cacsettings.DefaultGenderGenerator.val);
        Log.M?.WL(1, $"index:{index}");
        if (index < 0) { cacsettings.DefaultGenderGenerator.val = GenderGeneratorType.ModernArmy; return; }
        index = (index + 1) % generators.Count;
        cacsettings.DefaultGenderGenerator.val = generators[index];
        Log.M?.WL(1, $"next:{cacsettings.DefaultGenderGenerator.val.ToString()}");
      }
    }
    public NextGenderGenerator() {
      if (generators == null) {
        generators = new List<GenderGeneratorType>();
        foreach (var v in Enum.GetValues(typeof(GenderGeneratorType))) {
          generators.Add((GenderGeneratorType)v);
        }
      }
    }
  }

  public static class GenderGeneratorHelper {
    public static readonly Gender[] Genders = new Gender[3] {
      Gender.Male,
      Gender.Female,
      Gender.NonBinary
    };
    private static TagSet f_GenderTags = null;
    public static readonly string GENDER_GENERATOR_TAG_PREFIX = "gender_generator_";
    public static TagSet GenderTags {
      get {
        if(f_GenderTags == null) {
          f_GenderTags = new TagSet();
          foreach (var n in Enum.GetNames(typeof(GenderGeneratorType))) {
            f_GenderTags.Add(GENDER_GENERATOR_TAG_PREFIX + n);
          }
        }
        return f_GenderTags;
      }
    }
    private static Dictionary<string, GenderGeneratorType> f_GenderGeneratorByTag = null;
    public static Dictionary<string, GenderGeneratorType> GenderGeneratorByTag {
      get {
        if(f_GenderGeneratorByTag == null) {
          f_GenderGeneratorByTag = new Dictionary<string, GenderGeneratorType>();
          foreach (var v in Enum.GetValues(typeof(GenderGeneratorType))) {
            f_GenderGeneratorByTag.Add(GENDER_GENERATOR_TAG_PREFIX + v.ToString(), (GenderGeneratorType)v);
          }
        }
        return f_GenderGeneratorByTag;
      }
    }
    public static Dictionary<GenderGeneratorType, List<int>> GenderWeights = new Dictionary<GenderGeneratorType, List<int>>() {
      { GenderGeneratorType.ModernArmy , new List<int> { 70, 25, 5} },
      { GenderGeneratorType.ModernArmy2 , new List<int> { 65, 25, 10} },
      { GenderGeneratorType.ModernArmy3 , new List<int> { 70, 19, 1} },
      { GenderGeneratorType.RussianArmy , new List<int> { 75, 25, 0} },
      { GenderGeneratorType.AmericanArmy , new List<int> { 82, 18, 1} },
      { GenderGeneratorType.ChineseArmy , new List<int> { 92, 8, 0} },
      { GenderGeneratorType.MenOnly , new List<int> { 100, 0, 0} },
      { GenderGeneratorType.WomenOnly , new List<int> { 0, 100, 0} },
      { GenderGeneratorType.NonbinaryOnly , new List<int> { 0, 0, 100} },
      { GenderGeneratorType.MatriarchyArmy , new List<int> { 5, 94, 1} },
      { GenderGeneratorType.PatriarchyArmy , new List<int> { 94, 5, 1} },
    };
    public static string description(Strings.Culture culture) {
      StringBuilder sb = new StringBuilder();
      switch (culture) {
        case Strings.Culture.CULTURE_RU_RU:
          sb.AppendLine("Возможные значения (Муж./Жен./Не бинар.):");
          sb.AppendLine(" Vanilla - не переопределять");
          break;
        default:
          sb.AppendLine("Possible values (Male/Female/Nonbinary):");
          sb.AppendLine(" Vanilla - do not override");
          break;
      }
      foreach (var weights in GenderWeights) {
        sb.AppendLine(string.Format("{0} ------ {1}/{2}/{3}", weights.Key, weights.Value[0], weights.Value[1], weights.Value[2]));
      }
      return sb.ToString();
    }
  }
  [HarmonyPatch(typeof(PilotGenerator))]
  [HarmonyPatch("GetNameAndGender")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PilotGenerator_GetNameAndGender {
    public static void Postfix(PilotGenerator __instance, ref string firstName, ref string lastName, ref Gender gender) {
      Log.M?.TWL(0, $"PilotGenerator.GetNameAndGender firstName:{firstName} lastName:{lastName} gender:{gender}");
      if (CustomAmmoCategories.Settings.DefaultGenderGenerator.val == GenderGeneratorType.Vanilla) { return; }
      GenderGeneratorType effectiveGenerator = CustomAmmoCategories.Settings.DefaultGenderGenerator.val;
      if (CustomAmmoCategories.Settings.DefaultGenderGeneratorOverride) {
        if (__instance.Sim != null) {
          if (__instance.Sim.CurSystem != null) {
            if (__instance.Sim.CurSystem.Tags.ContainsAny(GenderGeneratorHelper.GenderTags)) {
              foreach (var tag in __instance.Sim.CurSystem.Tags) {
                if (GenderGeneratorHelper.GenderGeneratorByTag.TryGetValue(tag, out var gg)) {
                  Log.M?.WL(1, $"system gender generator tag found {tag}");
                  effectiveGenerator = gg;
                  break;
                }
              }
            }
          }
        }
      }
      Log.M?.WL(1, $"effective generator {effectiveGenerator}");
      if (GenderGeneratorHelper.GenderWeights.TryGetValue(effectiveGenerator, out var weightedResults)) {
        int weightedResult = SimGameState.GetWeightedResult(weightedResults, __instance.Sim.NetworkRandom.Float());
        var newgender = GenderGeneratorHelper.Genders[weightedResult];
        if (newgender != gender) {
          gender = newgender;
          firstName = __instance.pilotNameGenerator.GetFirstName(gender);
          Log.M?.WL(1, $"gender changed: {newgender} name changed: {firstName}");
        }
      }
    }
  }
}