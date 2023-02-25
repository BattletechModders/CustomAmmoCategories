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
using BattleTech;
using BattleTech.Save;
using BattleTech.Save.Core;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using HBS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustAmmoCategories {
  public class LocalSettingsHelper {
    public static string ResetSettings() {
      return CustomAmmoCategories.GlobalSettings.SerializeLocal();
    }
    public static Settings DefaultSettings() {
      return CustomAmmoCategories.GlobalSettings;
    }
    public static Settings CurrentSettings() {
      return CustomAmmoCategories.Settings;
    }
    public static string SaveSettings(object settings) {
      Settings set = settings as Settings;
      if (set == null) { set = CustomAmmoCategories.GlobalSettings; }
      JObject jsettigns = JObject.FromObject(set);
      PropertyInfo[] props = set.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      Log.M.TWL(0, "LocalSettingsHelper.SaveSettings");
      foreach (PropertyInfo prop in props) {
        bool skip = true;
        object[] attrs = prop.GetCustomAttributes(true);
        foreach (object attr in attrs) { if ((attr as GameplaySafe) != null) { skip = false; break; } };
        if (skip == false) { continue; }
        jsettigns.Remove(prop.Name);
        Log.M.WL(1, "removing:" + prop.Name);
      }
      return jsettigns.ToString(Formatting.Indented);
    }
    public static void ReadSettings(string json) {
      try {
        Settings local = JsonConvert.DeserializeObject<Settings>(json);
        CustomAmmoCategories.Settings.ApplyLocal(local);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}