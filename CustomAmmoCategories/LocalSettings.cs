using BattleTech;
using BattleTech.Save;
using BattleTech.Save.Core;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Harmony;
using HBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustAmmoCategories {
  public class LocalSettingsHelper {
    public static string ResetSettings() {
      return CustomAmmoCategories.GlobalSettings.SerializeLocal();
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