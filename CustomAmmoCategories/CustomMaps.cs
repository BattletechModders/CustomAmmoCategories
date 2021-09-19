using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustAmmoCategories {
  public class CustomMapDef {
    public string BaseMap { get; set; }
    public string ID { get; set; }
    public string FriendlyName { get; set; }
    public string TerrainPrefab { get; set; }
  }
  public static class CustomMapHelper {
    private static Dictionary<string, CustomMapDef> customMaps = new Dictionary<string, CustomMapDef>();
    public static void Register() {

    }
  }
}