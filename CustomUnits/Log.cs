using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CustomUnits {
  public class LogFile {
    private string m_logfile;
    private StringBuilder m_cache = new StringBuilder();
    private StreamWriter m_fs = null;
    public LogFile(string name) {
      this.m_logfile = Path.Combine(Log.BaseDirectory, $"CU_{name}_log.txt");
      string filename = Path.GetFileNameWithoutExtension(this.m_logfile);
      string ext = Path.GetExtension(this.m_logfile);
      foreach (var old_log in Directory.GetFiles(Log.BaseDirectory, $"{filename}.*{ext}", SearchOption.TopDirectoryOnly)) {
        File.Delete(old_log);
      }
      File.Delete(this.m_logfile);
      this.m_fs = new StreamWriter(this.m_logfile);
      this.m_fs.AutoFlush = true;
    }
    public void flush() {
      lock(this.m_fs) {
        lock (this.m_cache) {
          try {
            if (this.m_cache.Length > 0) {
              this.m_fs.Write(this.m_cache.ToString());
              this.m_fs.Flush();
              this.m_cache.Clear();
            }
          } catch (Exception) {

          }
        }
      }
    }
    public void skip() {
      lock (this.m_cache) {
        this.m_cache.Clear();
      }
    }
    public void LogWrite(int initiation, string line, bool eol = false, bool timestamp = false, bool isCritical = false) {
      string init = new string(' ', initiation);
      string prefix = String.Empty;
      if (timestamp) { prefix = DateTime.Now.ToString("[HH:mm:ss.fff]"); }
      if (initiation > 0) { prefix += init; };
      if (eol) {
        this.LogWrite(prefix + line + "\n", isCritical);
      } else {
        this.LogWrite(prefix + line, isCritical);
      }
    }
    public void LogWrite(string line, bool isCritical = false) {
      lock (m_cache) {
        m_cache.Append(line);
      }
      if (isCritical) { Log.flush(); };
      if (m_logfile.Length > Log.flushBufferLength) { Log.flush(); };
    }
    public void W(string line, bool isCritical = false) {
      this.LogWrite(line, isCritical);
    }
    public void WL(string line, bool isCritical = false) {
      line += "\n"; W(line, isCritical);
    }
    public void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; W(line, isCritical);
    }
    public void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; WL(line, isCritical);
    }
    public void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.UtcNow.ToString("HH:mm:ss.fff") + "]" + init + line;
      W(line, isCritical);
    }
    public void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.UtcNow.ToString("HH:mm:ss.fff") + "]" + init + line;
      WL(line, isCritical);
    }

  }
  public static class Log {
    public enum LogType { Main, Combat }
    public static string BaseDirectory;
    public static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    private static Dictionary<LogType, LogFile> files = new Dictionary<LogType, LogFile>();
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(10 * 1000);
        Log.flush();
      }
    }
    public static void InitLog() {
      Log.flushThread.Start();
      files.Add(LogType.Main, new LogFile("main"));
      files.Add(LogType.Combat, new LogFile("combat"));
    }
    public static void flush() {
      foreach(var log in files) { log.Value.flush(); }
    }
    public static LogFile M { get { return Core.Settings.debugLog?files[LogType.Main]:null; } }
    public static LogFile E { get { return files[LogType.Main]; } }
    public static LogFile Combat { get { return Core.Settings.debugLog ? files[LogType.Combat] : null; } }
    public static LogFile ECombat { get { return files[LogType.Combat]; } }
  }
}