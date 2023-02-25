using System;
using System.Reflection;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Newtonsoft.Json;
/*
namespace CharlesB {
  public class Core {
    public const string ModName = "CharlesB";
    public const string ModId = "com.joelmeador.CharlesB";

    internal static Settings ModSettings = new Settings();
    internal static string ModDirectory;

    public static void Init(string directory, string settingsJSON) {
      ModDirectory = directory;
      try {
        Log.CB.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
        ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
      } catch (Exception ex) {
        Log.CB.TWL(0,ex.ToString(), true);
        ModSettings = new Settings();
      }
    }
  }
}*/