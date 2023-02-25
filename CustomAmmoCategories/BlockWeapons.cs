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
using BattleTech.UI;
using CleverGirlAIDamagePrediction;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(StatCollection))]
  [HarmonyPatch("AddHistoryEvent")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(int), typeof(StatCollection.StatOperation), typeof(Variant), typeof(int) })]
  public static class StatCollection_AddHistoryEvent {
    public static void Postfix(StatCollection __instance, string statName, string sourceID, int stackItemUID, StatCollection.StatOperation operation, Variant modValue, int modIndex) {
      try {
        if (statName != CustomAmmoCategories.BlockedStatisticName) { return; }
        Weapon weapon = __instance.getWeapon();
        if (weapon == null) { return; }
        if (weapon.info().HUDSlot == null) { return; }
        Traverse.Create(weapon.info().HUDSlot).Field<CombatHUD>("HUD").Value.WeaponPanel.RefreshDisplayedWeapons();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(StatCollection))]
  [HarmonyPatch("RemoveHistoryEvent")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Statistic), typeof(int), typeof(bool) })]
  public static class StatCollection_RemoveHistoryEvent {
    public static void Postfix(StatCollection __instance, Statistic stat, int eventUID, bool skipLogging) {
      try {
        if (stat == null) { return; }
        if (stat.name != CustomAmmoCategories.BlockedStatisticName) { return; }
        Weapon weapon = __instance.getWeapon();
        if (weapon == null) { return; }
        if (weapon.info().HUDSlot == null) { return; }
        Traverse.Create(weapon.info().HUDSlot).Field<CombatHUD>("HUD").Value.WeaponPanel.RefreshDisplayedWeapons();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
    }
  }

  public static class BlockWeaponsHelpers {
    private static Dictionary<Weapon, bool> weaponBlockCache = new Dictionary<Weapon, bool>();
    public static void Clear() { weaponBlockCache.Clear(); }
    public static bool isBlocked(this Weapon weapon) {
      if(weaponBlockCache.TryGetValue(weapon,out bool result)) { return result; };
      RecalculateBlocked(weapon.parent);
      if (weaponBlockCache.TryGetValue(weapon, out result)) { return result; };
      result = false;
      weaponBlockCache.Add(weapon, false);
      return result;
    }
    public static bool CanBeEjected(this Weapon weapon) {
      return weapon.exDef().EjectWeapon;
    }
    public static bool isBlocking(this Weapon weapon) {
      var extDef = weapon.exDef();
      if (extDef.blockWeaponsInInstalledLocation) { return true; }
      if (extDef.blockWeaponsInMechLocations.Count > 0) { return true; }
      return false;
    }
    public static void EjectAIBlocking(this CombatGameState combat) {
      foreach(AbstractActor unit in combat.AllActors) {
        try {
          if (unit.IsDead) { continue; }
          if (unit.IsDeployDirector()) { continue; }
          if (unit.TeamId == combat.LocalPlayerTeamGuid) { continue; }
          if (unit.isSpawnProtected()) { continue; }
          unit.EjectBlocking();
        } catch (Exception e) {
          Log.M?.TWL(0,e.ToString(),true);
        }
      }
    }
    public static void EjectBlocking(this AbstractActor unit) {
      foreach(Weapon weapon in unit.Weapons) {
        try {
          if (weapon.IsFunctional == false) { continue; }
          if (weapon.CanBeEjected() == false) { continue; }
          AmmoModePair curAmmoMode = weapon.getCurrentAmmoMode();
          List<AmmoModePair> firingmethods = weapon.getAvaibleFiringMethods();
          bool eject = true;
          foreach (var ammomode in firingmethods) {
            weapon.ApplyAmmoMode(ammomode);
            if (weapon.CanFire) { eject = false; break; }
          }
          if (eject) { weapon.EjectWeaponForce(); }
          weapon.ApplyAmmoMode(curAmmoMode);
        }catch(Exception e) {
          Log.M?.TWL(0, e.ToString(), true);
        }
      }
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
      foreach (Weapon weapon in actor.Weapons) {
        if (weapon.info().HUDSlot == null) { continue; }
        Traverse.Create(weapon.info().HUDSlot).Field<CombatHUD>("HUD").Value.WeaponPanel.RefreshDisplayedWeapons();
      }
    }
  }
}