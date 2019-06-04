using System;
using System.IO;
using Harmony;

namespace WeaponRealizer
{
    public static class Logger
    {
        private static string LogFilePath => $"{Core.ModDirectory}/{Core.ModName}.log";

        public static void Error(Exception ex)
        {
            using (var writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"StackTrace: {ex.StackTrace}");
                WriteLogFooter(writer);
            }
        }

        public static void Debug(String line)
        {
            if (!Core.ModSettings.debug) return;
            using (var writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine(line);
                WriteLogFooter(writer);
            }
        }

        private static void WriteLogFooter(StreamWriter writer)
        {
            writer.WriteLine($"Date: {DateTime.Now}");
            writer.WriteLine(new string(c: '-', count: 80));
        }
    }
}