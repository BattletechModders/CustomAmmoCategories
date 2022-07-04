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
using CustomAmmoCategoriesLog;
using Localize;
using System.Collections.Generic;

namespace CustAmmoCategories {
  public static class BlockWeaponsHelpers {
    private static Dictionary<Weapon, bool> weaponBlockCache = new Dictionary<Weapon, bool>();
    public static bool isBlocked(this Weapon weapon) {
      if(weaponBlockCache.TryGetValue(weapon,out bool result)) { return result; };
      RecalculateBlocked(weapon.parent);
      if (weaponBlockCache.TryGetValue(weapon, out result)) { return result; };
      result = false;
      weaponBlockCache.Add(weapon, false);
      return result;
    }
    public static void RecalculateBlocked(this AbstractActor actor) {
      Log.M.TWL(0, "RecalculateBlocked:"+new Text(actor.DisplayName));
      HashSet<int> blockedLocations = new HashSet<int>();
      if(actor.UnitType != UnitType.Mech) {
        foreach (Weapon weapon in actor.Weapons) {
          if (weaponBlockCache.ContainsKey(weapon) == false) { weaponBlockCache.Add(weapon, false); } else { weaponBlockCache[weapon] = false; };
        }
        return;
      }
      foreach (Weapon weapon in actor.Weapons) {
        ExtWeaponDef def = weapon.exDef();
        if (weapon.IsFunctional == false) { continue; }
        if (def.blockWeaponsInInstalledLocation) { blockedLocations.Add(weapon.Location); }
        if (def.blockWeaponsInMechLocations.Count == 0) { continue; }
        foreach(ChassisLocations location in def.blockWeaponsInMechLocations) {
          blockedLocations.Add((int)location);
        }
      }
      Log.M.WL(1, "blockedLocations:" + blockedLocations.Count+" {");
      foreach (int location in blockedLocations) {
        Log.M.W(1, location.ToString());
      }
      Log.M.WL(0, "}");
      foreach (Weapon weapon in actor.Weapons) {
        ExtWeaponDef def = weapon.exDef();
        bool result = true;
        if (def.CanBeBlocked == false) { result = false; } else {
          result = blockedLocations.Contains(weapon.Location);
        }
        Log.M.WL(1, "weapon " + weapon.defId + " loc:"+weapon.Location+":"+result);
        if (weaponBlockCache.ContainsKey(weapon) == false) { weaponBlockCache.Add(weapon, result); } else { weaponBlockCache[weapon] = result; };
      }
    }
  }
}