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