using System;
using System.IO;
using CustomAmmoCategoriesLog;
using HarmonyLib;

namespace WeaponRealizer {
  public static class Logger {
    //private static string LogFilePath => $"{Core.ModDirectory}/{Core.ModName}.log";
    public static void Error(Exception ex) {
      //using (var writer = new StreamWriter(LogFilePath, true))
      //{
      //writer.WriteLine($"Message: {ex.Message}");
      Log.M.WL($"Message: {ex.Message}");
      Log.M.WL($"StackTrace: {ex.StackTrace}");
      //writer.WriteLine($"StackTrace: {ex.StackTrace}");
      WriteLogFooter();
    }
    public static void Debug(String line) {
      //if (!Core.ModSettings.debug) return;
      //using (var writer = new StreamWriter(LogFilePath, true)) {
      Log.M.WL(line);
      WriteLogFooter();
      //}
    }
    private static void WriteLogFooter() {
      Log.M.WL($"Date: {DateTime.Now}");
      Log.M.WL(new string(c: '-', count: 80));
    }
  }
}
