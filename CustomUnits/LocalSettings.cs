using CustAmmoCategories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomUnits {
  [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
  public class NextSortByGenerator : CustomSettings.NextSettingValue {
    public List<SortByTonnage.Settings> values = null;
    public override void Next(object settings) {
      Log.M?.TWL(0, $"NextSortByGenerator.Next {settings.GetType()}");
      if (settings is CustomUnits.CUSettings set) {
        int index = values.IndexOf(set.SortBy);
        Log.M?.WL(1, $"index:{index}");
        if (index < 0) { set.SortBy = values[0]; return; }
        index = (index + 1) % values.Count;
        set.SortBy = values[index];
        Log.M?.WL(1, $"next:{set.SortBy.ToString()}");
      }
    }
    public NextSortByGenerator() {
      values = new List<SortByTonnage.Settings>();
      values.Add(new SortByTonnage.Settings() { debug = false, orderByCbillValue = false, orderByNickname = false, orderByTonnage = false });
      values.Add(new SortByTonnage.Settings() { debug = false, orderByCbillValue = false, orderByNickname = true, orderByTonnage = false });
      values.Add(new SortByTonnage.Settings() { debug = false, orderByCbillValue = true, orderByNickname = false, orderByTonnage = false });
      values.Add(new SortByTonnage.Settings() { debug = false, orderByCbillValue = false, orderByNickname = false, orderByTonnage = true });
    }
  }
  public class LocalSettingsHelper {
    public static string ResetSettings() {
      return Core.GlobalSettings.SerializeLocal();
    }
    public static CUSettings DefaultSettings() {
      return Core.GlobalSettings;
    }
    public static CUSettings CurrentSettings() {
      return Core.Settings;
    }
    public static string SaveSettings(object settings) {
      CUSettings set = settings as CUSettings;
      if (set == null) { set = Core.GlobalSettings; }
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
        CUSettings local = JsonConvert.DeserializeObject<CUSettings>(json);
        Core.Settings.ApplyLocal(local);
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }

}