/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomMaps {
  public enum LogFileType {
    Main
  }
  public class LogFile {
    private string m_logfile;
    private Mutex mutex;
    private StringBuilder m_cache = null;
    private StreamWriter m_fs = null;
    public void printComponents(GameObject obj, int level) {
      Component[] components = obj.GetComponents<Component>();
      this.WL(level, "object:" + obj.name);
      this.WL(level, "components(" + components.Length + ")");
      foreach (Component component in components) {
        if (component == null) {
          this.WL(level + 1, "null");
        } else {
          this.WL(level + 1, component.name + ":" + component.GetType().ToString());
        }
      }
      this.WL(level, "childs(" + obj.transform.childCount + ")");
      for (int t = 0; t < obj.transform.childCount; ++t) {
        printComponents(obj.transform.GetChild(t).gameObject, level + 1);
      }
    }
    public LogFile(string name) {
      try {
        this.mutex = new Mutex();
        this.m_cache = new StringBuilder();
        this.m_logfile = Path.Combine(Log.BaseDirectory, name);
        File.Delete(this.m_logfile);
        this.m_fs = new StreamWriter(this.m_logfile);
        this.m_fs.AutoFlush = true;
      } catch (Exception) {

      }
    }
    public void flush() {
      if (this.mutex.WaitOne(1000)) {
        this.m_fs.Write(this.m_cache.ToString());
        this.m_fs.Flush();
        this.m_cache.Length = 0;
        this.mutex.ReleaseMutex();
      }
    }
    public void W(string line, bool isCritical = false) {
      if (this.mutex.WaitOne(1000)) {
        m_cache.Append(line);
        this.mutex.ReleaseMutex();
      }
      if ((m_logfile.Length > Log.flushBufferLength) || (isCritical)) { this.flush(); };
    }
    public void WL(string line, bool isCritical = false) {
      line += "\n"; this.W(line, isCritical);
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
    //private static string m_assemblyFile;
    public static string BaseDirectory;
    public static bool debugLog { get; set; } = false;
    public static readonly int flushBufferLength = 16 * 1024;
    public static bool flushThreadActive = true;
    public static Thread flushThread = new Thread(flushThreadProc);
    public static void flushThreadProc() {
      while (Log.flushThreadActive == true) {
        Thread.Sleep(30 * 1000);
        //Log.LogWrite("Log flushing\n");
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
    public static LogFile M { get { return Log.debugLog?Log.logs[LogFileType.Main]:null; } }
    public static LogFile M_err { get { return Log.logs[LogFileType.Main]; } }
    public static void InitLog() {
      Log.logs.Add(LogFileType.Main, new LogFile("main_log.txt"));
      Log.flushThread.Start();
    }
  }

}
