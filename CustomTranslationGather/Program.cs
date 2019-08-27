using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomTranslationGather {
  public enum Culture {
    CULTURE_EN_US = 0,
    CULTURE_DE_DE = 1,
    CULTURE_ZH_CN = 2,
    CULTURE_ES_ES = 3,
    CULTURE_FR_FR = 4,
    CULTURE_IT_IT = 5,
    CULTURE_RU_RU = 6,
    CULTURE_PT_PT = 7,
    CULTURE_PT_BR = 8
  }
  public class TranslateRecord {
    public string Name { get; set; }
    public string FileName { get; set; }
    public Dictionary<Culture, string> Localization { get; set; }
    public TranslateRecord() {
      Localization = new Dictionary<Culture, string>();
    }
    public TranslateRecord(string key, string filename, string original) {
      this.Name = key;
      this.FileName = filename;
      this.Localization = new Dictionary<Culture, string>();
      this.Localization.Add(Culture.CULTURE_EN_US, original);
      this.Localization.Add(Culture.CULTURE_DE_DE, original);
      this.Localization.Add(Culture.CULTURE_RU_RU, original);
    }
  }
  class Program {
    public static Regex locRegEx = new Regex("[_]{2}[/]{1}([a-zA-Z0-9\\._\\-\\,]*)[/]{1}[_]{2}");
    public static void GetAllJsons(string path,ref List<string> jsons, int initiation) {
      try {
        string init = new string(' ', initiation);
        foreach (string d in Directory.GetDirectories(path)) {
          Console.WriteLine(init+d);
          foreach (string f in Directory.GetFiles(d)) {
            Console.WriteLine(init + " " + f+":"+ Path.GetExtension(f).ToUpper());
            if (Path.GetExtension(f).ToUpper() == ".JSON") {
              jsons.Add(f);
            }
          }
          GetAllJsons(d,ref jsons, initiation+1);
        }
      } catch (System.Exception excpt) {
        Console.WriteLine(excpt.Message);
      }
    }
    static Dictionary<string, TranslateRecord> GatherLocalization(string basePath) {
      string locfile = Path.Combine(basePath, "Localization.json");
      Console.Write("File:" + locfile + "\n");
      Dictionary<string, TranslateRecord> locs = new Dictionary<string, TranslateRecord>();
      if (File.Exists(locfile)) {
        string content = File.ReadAllText(locfile);
        locs = JsonConvert.DeserializeObject<Dictionary<string, TranslateRecord>>(content);
      }
      return locs;
    }
    static void Main(string[] args) {
      string modsDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),"..");
      string consolidatedFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Localization.json");
      Dictionary<string, TranslateRecord> full_translation = new Dictionary<string, TranslateRecord>();
      File.Delete(consolidatedFile);
      //string modsDir = "c:\\Games\\steamapps\\common\\BATTLETECH\\Mods\\CustomTranslation";
      Console.WriteLine(modsDir);
      foreach (string basePath in Directory.GetDirectories(modsDir)){
        string filename = Path.GetFileName(basePath);
        if (filename == ".modtek") { continue; }
        Console.WriteLine(" "+basePath);
        List<string> jsons = new List<string>();
        Dictionary<string, TranslateRecord> translation = null;
        try {
          translation = GatherLocalization(basePath);
        } catch (Exception e) {
          Console.WriteLine(basePath + " Localization.json" + e.ToString());
          return;
        }
        GetAllJsons(basePath, ref jsons,3);
        foreach (string jsonPath in jsons) {
          Console.WriteLine("  json:" + jsonPath);
          string jsonContent = File.ReadAllText(jsonPath);
          try {
            JObject json = JObject.Parse(jsonContent);
            string fileName = jsonPath.Substring(basePath.Length);
            string sPath = jsonPath.Substring(basePath.Length);
            bool changed = false;
            if (json["Description"] != null) {
              string content = (string)json["Description"]["Details"];
              if ((string.IsNullOrEmpty(content) == false)&&(locRegEx.Matches(content).Count == 0)) {
                string key = json["Description"]["Id"] + ".Details";
                TranslateRecord rec = new TranslateRecord(key, sPath, content);
                if (translation.ContainsKey(rec.Name) == false) { translation.Add(rec.Name, rec); full_translation.Add(rec.Name, rec); }
                json["Description"]["Details"] = "__/" + key + "/__";
                changed = true;
              }
              content = (string)json["Description"]["UIName"];
              if ((string.IsNullOrEmpty(content) == false) && (locRegEx.Matches(content).Count == 0)) {
                string key = json["Description"]["Id"] + ".UIName";
                TranslateRecord rec = new TranslateRecord(key, sPath, content);
                if (translation.ContainsKey(rec.Name) == false) { translation.Add(rec.Name, rec); full_translation.Add(rec.Name, rec); }
                json["Description"]["UIName"] = "__/" + key + "/__";
                changed = true;
              }
              if (json["StockRole"] != null) {
                content = (string)json["StockRole"];
                if ((string.IsNullOrEmpty(content) == false) && (locRegEx.Matches(content).Count == 0)) {
                  string key = json["Description"]["Id"] + ".StockRole";
                  TranslateRecord rec = new TranslateRecord(key, sPath, content);
                  if (translation.ContainsKey(rec.Name) == false) { translation.Add(rec.Name, rec); full_translation.Add(rec.Name, rec); }
                  json["StockRole"] = "__/" + key + "/__";
                  changed = true;
                }
              }
              if (json["YangsThoughts"] != null) {
                content = (string)json["YangsThoughts"];
                if ((string.IsNullOrEmpty(content) == false) && (locRegEx.Matches(content).Count == 0)) {
                  string key = json["Description"]["Id"] + ".YangsThoughts";
                  TranslateRecord rec = new TranslateRecord(key, sPath, content);
                  if (translation.ContainsKey(rec.Name) == false) { translation.Add(rec.Name, rec); full_translation.Add(rec.Name, rec); }
                  json["YangsThoughts"] = "__/" + key + "/__";
                  changed = true;
                }
              }
              if (changed) {
                File.WriteAllText(jsonPath, json.ToString(Formatting.Indented));
              }
            }
          } catch(Exception e) {
            Console.WriteLine(jsonPath);
            Console.WriteLine(e.ToString());
          }
        }
        if (translation.Count > 0) {
          string locContent = JsonConvert.SerializeObject(translation, Formatting.Indented);
          string locPath = Path.Combine(basePath, "Localization.json");
          File.WriteAllText(locPath, locContent);
        }
      }
      string fullContent = JsonConvert.SerializeObject(full_translation, Formatting.Indented);
      File.WriteAllText(consolidatedFile, fullContent);
      Console.WriteLine("Press any key to continue ...");
      Console.ReadKey();
    }
  }
}
