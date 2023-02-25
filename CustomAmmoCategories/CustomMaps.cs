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
using BattleTech.Data;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
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