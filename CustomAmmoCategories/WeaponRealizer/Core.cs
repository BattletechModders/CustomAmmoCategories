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
      /*try
      {
          ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJson);
      }
      catch (Exception ex)
      {
          Logger.Error(ex);
          ModSettings = new Settings();
      }*/
      ModSettings = settings;
      Log.M.WL("WR settings:");
      Log.M.WL(JsonConvert.SerializeObject(ModSettings,Formatting.Indented));
      //HarmonyInstance.DEBUG = ModSettings.debug;
      //var harmony = HarmonyInstance.Create(ModId);
      //harmony.PatchAll(Assembly.GetExecutingAssembly());
      //Patches.Apply(harmony);
    }
  }
}