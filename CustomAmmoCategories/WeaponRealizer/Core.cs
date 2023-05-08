using System;
using System.Reflection;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Newtonsoft.Json;

namespace WeaponRealizer {
  public static partial class Core {
    public const string ModName = "WeaponRealizer";
    public const string ModId = "com.joelmeador.WeaponRealizer";

    public static Settings ModSettings = null;
    internal static string ModDirectory;

    public static void Init(string directory, Settings settings) {
      ModDirectory = directory;
      ModSettings = settings;
      Log.M.WL("WR settings:");
      Log.M.WL(JsonConvert.SerializeObject(ModSettings,Formatting.Indented));
    }
  }
}