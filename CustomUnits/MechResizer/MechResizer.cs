using System;
using System.Reflection;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Newtonsoft.Json;

namespace MechResizer {
  public class MechResizer {
    public static Settings ModSettings = new Settings();
    internal static string ModDirectory;

    public static void Init(string directory, string settingsJSON) {
      ModDirectory = directory;
      try {
        ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
      } catch (Exception ex) {
        Log.LogWrite(ex.ToString(), true);
        ModSettings = new Settings();
      }
    }
  }
}