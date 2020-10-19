using CustomAmmoCategoriesLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CustAmmoCategories {
  public class CACUIStringsDef {
    public Dictionary<string, string> UIStrings;
    public CACUIStringsDef() {
      UIStrings = new Dictionary<string, string>();
    }
  }
  public static class CACUiStringsHelper {
    private static Dictionary<string, string> UIStrings = new Dictionary<string, string>();
    public static string UI(this string key) {
      if(UIStrings.TryGetValue(key, out string val)) {
        return val;
      }
      return key;
    }
    private static void LoadFile(string filename) {
      try {
        CACUIStringsDef def = JsonConvert.DeserializeObject<CACUIStringsDef>(File.ReadAllText(filename));
        foreach (var UIStr in def.UIStrings) {
          if (CACUiStringsHelper.UIStrings.ContainsKey(UIStr.Key)) {
            CACUiStringsHelper.UIStrings[UIStr.Key] = UIStr.Value;
          } else {
            CACUiStringsHelper.UIStrings.Add(UIStr.Key, UIStr.Value);
          }
        }
      } catch(Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}