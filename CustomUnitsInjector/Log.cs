using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CustomUnitsInjector {
  public enum LogFileType {
    Main
  }
  public class LogFile {
    private string m_logfile;
    private SpinLock spinlock;
    private StringBuilder m_cache = null;
    //private StreamWriter m_fs = null;
    public LogFile(string name) {
      try {
        this.spinlock = new SpinLock();
        this.m_cache = new StringBuilder();
        this.m_logfile = Path.Combine(Log.BaseDirectory, name);
        File.Delete(this.m_logfile);
        //this.m_fs = new StreamWriter(this.m_logfile);
        //this.m_fs.AutoFlush = true;
      } catch (Exception) {

      }
    }
    public void flush() {
      bool locked = false;
      try {
        if (spinlock.IsHeldByCurrentThread == false) { spinlock.Enter(ref locked); }
        if (this.m_cache.Length > 0) {
          //this.m_fs.Write(this.m_cache.ToString());
          //this.m_fs.Flush();
          this.m_cache.Length = 0;
        }
      } finally {
        if (locked) { spinlock.Exit(); }
      }
    }
    public void W(string line, bool isCritical = false) {
      //bool locked = false;
      try {
        Console.Write(line);
        //if (spinlock.IsHeldByCurrentThread == false) { spinlock.Enter(ref locked); }
        //m_cache.Append(line);
      } finally {
        //if (locked) { spinlock.Exit(); }
      }
      //if (isCritical) { this.flush(); };
      //if (m_logfile.Length > Log.flushBufferLength) { this.flush(); };
    }
    public void WL(string line, bool isCritical = false) {
      Console.WriteLine(line);
      //line += "\n"; this.W(line, isCritical);
    }
    public void W(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; this.W(line, isCritical);
    }
    public void WL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = init + line; this.WL(line, isCritical);
    }
    public void TW(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      this.W(line, isCritical);
    }
    public void TWL(int initiation, string line, bool isCritical = false) {
      string init = new string(' ', initiation);
      line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + init + line;
      this.WL(line, isCritical);
    }
  }
  public static class Log {
    private static Dictionary<LogFileType, LogFile> logs = new Dictionary<LogFileType, LogFile>();
    public static bool enabled = true;
    //private static string m_assemblyFile;
    public static string BaseDirectory;
    public static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(30 * 1000);
        Log.flush();
      }
    }
    public static void flush() {
      foreach (var log in Log.logs) { log.Value.flush(); }
    }
    public static void LogWrite(string line, bool isCritical = false) {
      if (Log.logs.ContainsKey(LogFileType.Main) == false) { return; }
      Log.logs[LogFileType.Main].W(line, isCritical);
    }
    public static LogFile M { get { return Injector.settings.debugLog ? Log.logs[LogFileType.Main] : null; } }
    public static LogFile Err { get { return Log.logs[LogFileType.Main]; } }
    public static void InitLog() {
      Log.logs.Add(LogFileType.Main, new LogFile("StatisticEffectDataInjector_main_log.txt"));
      //Log.flushThread.Start();
    }
  }

}