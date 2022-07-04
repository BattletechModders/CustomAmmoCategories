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
using Localize;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustAmmoCategories {
  public static class CustomMechHelper {
    public static List<Action<Mech, Transform>> InitGameRepPrefixes = new List<Action<Mech, Transform>>();
    public static List<Action<Mech, Transform>> InitGameRepPostfixes = new List<Action<Mech, Transform>>();
    public static void RegisterInitGameRepPrefix(Action<Mech, Transform> prefix) { InitGameRepPrefixes.Add(prefix); }
    public static void RegisterInitGameRepPostfix(Action<Mech, Transform> postfix) { InitGameRepPostfixes.Add(postfix); }

  }
}