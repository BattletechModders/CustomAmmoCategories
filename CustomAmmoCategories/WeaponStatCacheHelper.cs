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
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Mech), typeof(CombatGameState), typeof(MechComponentRef), typeof(string) })]
  public static class Weapon_Constructor_Mech {
    public static void Prefix(Mech parent, CombatGameState combat, MechComponentRef mcRef, string UID) {
      try {
        Log.M?.TWL(0, "Weapon.Constructor "+(parent == null?"null":parent.PilotableActorDef.ChassisID)+" "+ (mcRef == null ? "null" : (mcRef.ComponentDefID+":"+mcRef.ComponentDefType)));
        Log.M?.WL(1, "incoming ref definition:" + (mcRef.Def == null ? "null" : (mcRef.Def.GetType().Name + ":" + mcRef.Def.Description.Id)));
        if (mcRef.Def == null) {
          Log.M?.WL(1,"definition is null. Trying to fix");
          if (string.IsNullOrEmpty(mcRef.ComponentDefID) == false) {
            Log.M?.WL(1, "checking in data manager "+ mcRef.ComponentDefID);
            if (combat.DataManager.WeaponDefs.Exists(mcRef.ComponentDefID)) {
              Log.M?.WL(1, "exists");
              mcRef.DataManager = combat.DataManager;
              mcRef.SetComponentDef(combat.DataManager.WeaponDefs.Get(mcRef.ComponentDefID));
              mcRef.RefreshComponentDef();
              if(mcRef.Def != null) {
                Log.M?.WL(1, "fixed");
              }
            }
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(Weapon __instance, Mech parent, CombatGameState combat, MechComponentRef mcRef, string UID) {
      try {
        if (parent == null) {
          Log.M?.TWL(0,"weapon without parent\n"+Environment.StackTrace);
          return;
        }
        if (__instance.weaponDef == null) {
          Log.M?.TWL(0, "weapon without definition");
          Log.M?.WL(1, "ref:" + (__instance.baseComponentRef==null?"null":__instance.baseComponentRef.ComponentDefID));
          Log.M?.WL(1, "incoming ref:" + (mcRef == null ? "null" : mcRef.ComponentDefID));
          Log.M?.WL(1, "incoming ref definition:" + (mcRef.Def == null ? "null" : (mcRef.Def.GetType().Name + ":" + mcRef.Def.Description.Id)));
          Log.M?.WL(1, "uid:"+__instance.uid);
          return;
        }
        if (__instance.weaponDef.Description == null) {
          Log.M?.TWL(0, "weapon without description\n" + Environment.StackTrace);
          return;
        }
        Log.M?.TWL(0, "Weapon.Constructor " + parent.PilotableActorDef.Description.Id + " " + __instance.weaponDef.Description.Id);
        __instance.Register(new WeaponExtendedInfo(__instance, __instance.weaponDef));
      }catch(Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Vehicle), typeof(CombatGameState), typeof(VehicleComponentRef), typeof(string) })]
  public static class Weapon_Constructor_Vehicle {
    public static void Postfix(Weapon __instance, Vehicle parent, CombatGameState combat, VehicleComponentRef vcRef, string UID) {
      try {
        Log.M?.TWL(0, "Weapon.Constructor " + parent.PilotableActorDef.Description.Id + " " + __instance.weaponDef.Description.Id);
        __instance.Register(new WeaponExtendedInfo(__instance, __instance.weaponDef));
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(Turret), typeof(CombatGameState), typeof(TurretComponentRef), typeof(string) })]
  public static class Weapon_Constructor_Turret {
    public static void Postfix(Weapon __instance, Turret parent, CombatGameState combat, TurretComponentRef tcRef, string UID) {
      try {
        Log.M?.TWL(0, "Weapon.Constructor " + parent.PilotableActorDef.Description.Id + " " + __instance.weaponDef.Description.Id);
        __instance.Register(new WeaponExtendedInfo(__instance, __instance.weaponDef));
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("AssignAmmoToWeapons")]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_AssignAmmoToWeapons {
    public static void Postfix(AbstractActor __instance) {
      try {
        Log.M?.TWL(0, "AbstractActor.AssignAmmoToWeapons " + __instance.PilotableActorDef.Description.Id);
        foreach (Weapon weapon in __instance.Weapons) {
          weapon.info().isBoxesAssigned = true;
          weapon.Revalidate();
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch(MethodType.Setter)]
  [HarmonyPatch("DisplayedWeapon")]
  [HarmonyPatch(new Type[] { typeof(Weapon) })]
  public static class CombatHUDWeaponSlot_DisplayedWeapon {
    public static void Postfix(CombatHUDWeaponSlot __instance, Weapon value) {
      try {
        if (value == null) { return; }
        value.info().HUDSlot = __instance;
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("InitAbilities")]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class CombatHUDWeaponSlot_InitAbilities {
    public static void Postfix(AbstractActor __instance) {
      try {
        foreach(Weapon weapon in __instance.Weapons) {
          weapon.info().RefreshModeAvaibility();
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  //DEMO
  //[HarmonyPatch(typeof(Mech))]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch("Init")]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool) })]
  //public static class Mech_Init_Melee_Demo {
  //  public static void Postfix(Mech __instance, Vector3 position, float facing, bool checkEncounterCells) {
  //    try {
  //      Log.M?.TWL(0,"Mech init melee demo "+__instance.PilotableActorDef.Description.Id);
  //      WeaponExtendedInfo info = __instance.MeleeWeapon.info();
  //      WeaponMode modeToDel = info.mode;
  //      WeaponMode chargeMode = new WeaponMode();
  //      chargeMode.Id = "charge";
  //      chargeMode.Name = "CHARGE";
  //      chargeMode.UIName = "CHARGE";
  //      chargeMode.DamagePerShot = 10f;
  //      chargeMode.ShotsWhenFired = 5;
  //      chargeMode.isBaseMode = true;
  //      info.AddMode(chargeMode, true);
  //      WeaponMode kickMode = new WeaponMode();
  //      kickMode.Id = "kick";
  //      kickMode.Name = "KICK";
  //      kickMode.UIName = "KICK";
  //      kickMode.ShotsWhenFired = 0;
  //      kickMode.DamagePerShot = 15f;
  //      info.AddMode(kickMode, false);
  //      WeaponMode punchMode = new WeaponMode();
  //      punchMode.Id = "punch";
  //      punchMode.Name = "PUNCH";
  //      punchMode.UIName = "PUNCH";
  //      punchMode.ShotsWhenFired = 1;
  //      punchMode.DamagePerShot = 20f;
  //      info.AddMode(punchMode, false);
  //      info.RemoveMode(modeToDel);
  //    } catch (Exception e) {
  //      Log.M?.TWL(0, e.ToString(), true);
  //    }
  //  }
  //}
  public class WeaponExtendedInfo {
    public Weapon weapon { get; set; }
    public WeaponOrderDataElementDef sortingDef { get; set; } = null;
    public HashSet<string> restrictedModes { get; set; } = new HashSet<string>();
    public bool isBoxesAssigned { get; set; } = false;
    public WeaponMode mode { get; set; }
    public Dictionary<string, WeaponMode> modes { get; set; } = new Dictionary<string, WeaponMode>();
    public Dictionary<WeaponMode, MechComponent> modesSources { get; set; } = new Dictionary<WeaponMode, MechComponent>();
    public Dictionary<WeaponMode, HashSet<WeaponMode>> overridenModes { get; set; } = new Dictionary<WeaponMode, HashSet<WeaponMode>>();
    public ExtWeaponDef extDef { get; set; }
    public ExtAmmunitionDef ammo { get; set; }
    public bool HasAmmoVariants { get; set; }
    public bool NoValidAmmo { get; set; }
    public bool needRevalidate { get; set; }
    public bool allowUIModSwitch { get; set; } = true;
    public CustomAmmoCategory effectiveAmmoCategory { get { return mode.AmmoCategory == null ? extDef.AmmoCategory : mode.AmmoCategory; } }
    private CombatHUDWeaponSlot f_HUDSlot = null;
    public CombatHUDWeaponSlot HUDSlot {
      get {
        if (f_HUDSlot == null) { return null; }
        if (f_HUDSlot.DisplayedWeapon != this.weapon) { f_HUDSlot = null; }
        return f_HUDSlot;
      }
      set {
        f_HUDSlot = value;
      }
    }
    public void setSorting(WeaponOrderDataElementDef sorting) {
      this.sortingDef = sorting;
    }
    public bool setMode(string modeId) {
      if(this.modes.TryGetValue(modeId, out var Mode) == false) {
        return false;
      }
      if (this.restrictedModes.Contains(modeId)) { return false; }
      this.mode = Mode;
      this.needRevalidate = true;
      Statistic modeIdStat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName);
      if (modeIdStat == null) {
        modeIdStat = weapon.StatCollection.AddStatistic(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      } else {
        modeIdStat.SetValue<string>(modeId);
      }
      if (this.ammo.AmmoCategory != this.effectiveAmmoCategory) {
        this.ammo = this.findBestAmmo(this.effectiveAmmoCategory);
        Statistic ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
        if (ammoId == null) {
          ammoId = weapon.StatCollection.AddStatistic(CustomAmmoCategories.AmmoIdStatName, this.ammo.Id);
        } else {
          ammoId.SetValue<string>(this.ammo.Id);
        }
      }
      HUDSlot?.RefreshDisplayedWeapons();
      return true;
    }
    public void AddMode(WeaponMode mode, bool switchTo) {
      this.AddMode(mode, null, switchTo);
    }
    public void AddMode(WeaponMode mode, MechComponent src, bool switchTo) {
      if (mode == null) { return; }
      if (this.modes.ContainsKey(mode.Id)) {
        if (this.mode.Id == mode.Id) { switchTo = true; }
        WeaponMode oldMode = this.modes[mode.Id];
        string curid = mode.Id;
        if (mode.isFromJson) {
          mode = oldMode.merge(mode);
          int t = 0;
          string new_id = string.Format("{0}_{1}", mode.Id, t);
          while (this.modes.ContainsKey(new_id)) { ++t; new_id = string.Format("{0}_{1}", mode.Id, t); }
          mode.Id = new_id;
          if(this.overridenModes.TryGetValue(oldMode, out var ovrmodes) == false) {
            ovrmodes = new HashSet<WeaponMode>();
          }
          ovrmodes.Add(mode);
          this.overridenModes[oldMode] = ovrmodes;
        }
      }
      this.modes.Add(mode.Id, mode);
      if (switchTo) this.setMode(mode.Id);
      if (src != null)this.modesSources[mode] = src;
    }
    public MechComponent currentModeSource() {
      if (this.modesSources.TryGetValue(this.mode, out var result)) { return result; }
      return null;
    }
    public void RemoveMode(string id) {
      if(this.modes.TryGetValue(id, out var delmode)) {
        if(delmode != null)this.modesSources.Remove(delmode);
        this.modes.Remove(id);
      }      
    }
    public bool isModeAvailble(WeaponMode mode) {
      if(this.modesSources.TryGetValue(mode, out var src)) {
        if (src.IsFunctional == false) { return false; }
      }
      if (this.modes.ContainsKey(mode.Id) == false) { return false; }
      if(this.overridenModes.TryGetValue(mode, out var ovrmodes)) {
        foreach (var ovrmode in ovrmodes) {
          if (this.isModeAvailble(ovrmode)) { return false; }
        }
      }
      return mode.Lock.isAvaible(weapon);
    }
    public bool isCurrentModeAvailable() {
      return isModeAvailble(this.mode);
    }
    public void DisableMode(string id) {
      this.restrictedModes.Add(id);
      if (this.restrictedModes.Contains(id)) {
        CustomAmmoCategories.CycleMode(this.weapon, true, false);
        this.HUDSlot?.RefreshDisplayedWeapon();
      }
    }
    public void EnableMode(string id) {
      this.restrictedModes.Remove(id);
    }
    public void RemoveMode(WeaponMode m) {
      string id_to_remove = string.Empty;
      foreach(var mode in this.modes) {
        if (m == mode.Value) { id_to_remove = mode.Key; break; }
      }
      if (string.IsNullOrEmpty(id_to_remove) == false) { this.modes.Remove(id_to_remove); }
    }
    public List<WeaponMode> avaibleModes() {
      List<WeaponMode> result = new List<WeaponMode>();
      foreach (var mode in this.modes) {
        if (this.isModeAvailble(mode.Value) == false) { continue; };
        if (this.restrictedModes.Contains(mode.Key)) { continue; }
        result.Add(mode.Value);
      }
      return result;
    }
    public List<ExtAmmunitionDef> getAvaibleAmmo(CustomAmmoCategory category) {
      Log.M.TWL(0, "getAvaibleAmmo " + weapon.defId + " category:" + category.Id+ " ammoBoxes:" + weapon.ammoBoxes.Count);
      HashSet<ExtAmmunitionDef> result = new HashSet<ExtAmmunitionDef>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; };
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; };
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (ammo.AmmoCategory.Id != category.Id) { continue; };
        result.Add(ammo);
      }
      ExtWeaponDef def = this.extDef;
      foreach (var intAmmo in def.InternalAmmo) {
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(intAmmo.Key);
        if (ammo.AmmoCategory.Id != category.Id) { continue; };
        result.Add(ammo);
      }
      if (result.Count == 0) { result.Add(category.defaultAmmo()); }
      foreach (var res in result) {
        Log.M.WL(1, res.Id);
      }
      return result.ToList();
    }
    public ExtAmmunitionDef findBestAmmo(CustomAmmoCategory effectiveCategory) {
      List<ExtAmmunitionDef> avaibleAmmo = this.getAvaibleAmmo(effectiveCategory);
      if (avaibleAmmo.Count > 0) { this.NoValidAmmo = false; return avaibleAmmo[0]; }
      NoValidAmmo = true;
      return effectiveCategory.defaultAmmo();
    }
    public WeaponExtendedInfo(Weapon weapon, WeaponDef def) {
      this.weapon = weapon;
      if(weapon.weaponDef == null) {
        this.mode = CustomAmmoCategories.DefaultWeaponMode;
        this.needRevalidate = false;
        this.ammo = CustomAmmoCategories.DefaultAmmo;
        this.extDef = CustomAmmoCategories.DefaultWeapon;
        this.HasAmmoVariants = false;
        Log.M.TWL(0, "WeaponExtendedInfo parent:"+(weapon.parent == null?"null":weapon.parent.PilotableActorDef.Description.Id)+" has weapon without definition uid:"+weapon.uid + " mechRef:"+(weapon.baseComponentRef != null? weapon.baseComponentRef.ComponentDefID: "null"));
        return;
      }
      if (weapon.WeaponCategoryValue.IsMelee) { isBoxesAssigned = true; }
      this.extDef = def.exDef();
      foreach (var defMode in this.extDef.Modes) { this.modes.Add(defMode.Key, defMode.Value); }
      string modeId = extDef.baseModeId;
      if (this.modes.Count == 0) {
        modeId = CustomAmmoCategories.DefaultWeaponMode.Id;
        this.modes.Add(modeId, CustomAmmoCategories.DefaultWeaponMode);
      }
      if(this.modes.TryGetValue(modeId, out var Mode)) {
        this.mode = Mode;
      } else {
        this.modes.Add(modeId, CustomAmmoCategories.DefaultWeaponMode);
        this.mode = CustomAmmoCategories.DefaultWeaponMode;
      }
      Statistic modeIdStat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName);
      if (modeIdStat == null) {
        modeIdStat = weapon.StatCollection.AddStatistic(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      } else {
        modeIdStat.SetValue<string>(modeId);
      }
      Statistic ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
      if (ammoId == null) {
        ammoId = weapon.StatCollection.AddStatistic(CustomAmmoCategories.AmmoIdStatName,CustomAmmoCategories.DefaultAmmo.Id);
      }
      ammo = CustomAmmoCategories.DefaultAmmo;
      needRevalidate = true;
      NoValidAmmo = true;
      HasAmmoVariants = false;
    }
    public void RefreshModeAvaibility() {
      Log.M?.TWL(0, "RefreshModeAvaibility:" + this.weapon.defId);
      if (this.isCurrentModeAvailable() == false) {
        CustomAmmoCategories.CycleMode(this.weapon, true);
      }
      //Statistic modeIdStat = weapon.StatCollection.GetOrCreateStatisic(CustomAmmoCategories.WeaponModeStatisticName, extDef.baseModeId);
      //if(this.modes.TryGetValue(extDef.baseModeId, out WeaponMode baseMode)) {
      //  if (baseMode.Lock.isAvaible(this.weapon)) {
      //    Log.M?.WL(1, $"mode {extDef.baseModeId} is available");
      //    modeIdStat.SetValue<string>(extDef.baseModeId);
      //    this.mode = baseMode;
      //  } else {
      //    CustomAmmoCategories.CycleMode(this.weapon, true);
      //  }
      //}
    }
    public void Revalidate() {
      if (this.weapon.StatCollection == null) { return; }
      if (this.isBoxesAssigned == false) { return; }
      Log.M?.TWL(0,"Revalidate:"+this.weapon.defId);
      string modeId = extDef.baseModeId;
      Statistic modeIdStat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName);
      if (modeIdStat == null) {
        modeIdStat = weapon.StatCollection.AddStatistic(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      } else {
        modeId = modeIdStat.Value<string>();
      }
      Log.M?.WL(1, "Mode id:" + modeId);
      if (this.modes.TryGetValue(modeId, out WeaponMode Mode)) {
        Log.M?.WL(2, "mod found");
        this.mode = Mode;
      } else if(extDef.Modes.TryGetValue(modeId, out WeaponMode defMode)) {
        this.modes.Add(modeId, defMode);
        this.mode = defMode;
        Log.M?.WL(2, "mod found in ext def");
      } else if (this.modes.TryGetValue(extDef.baseModeId, out WeaponMode baseMode)) {
        this.mode = baseMode;
        modeIdStat.SetValue<string>(extDef.baseModeId);
        Log.M?.WL(2, "not found - fallback to "+ extDef.baseModeId);
      } else if (extDef.Modes.TryGetValue(extDef.baseModeId, out WeaponMode baseDefMode)) {
        this.modes.Add(extDef.baseModeId, defMode);
        this.mode = baseDefMode;
        modeIdStat.SetValue<string>(extDef.baseModeId);
        Log.M?.WL(2, "not found - fallback to ext def " + extDef.baseModeId);
      } else {
        this.modes.Add(CustomAmmoCategories.DefaultWeaponMode.Id, CustomAmmoCategories.DefaultWeaponMode);
        modeIdStat.SetValue<string>(CustomAmmoCategories.DefaultWeaponMode.Id);
        this.mode = CustomAmmoCategories.DefaultWeaponMode;
        Log.M?.WL(2, "not found - fallback to ext def " + CustomAmmoCategories.DefaultWeaponMode.Id);
      }
      CustomAmmoCategory effectiveCategory = extDef.AmmoCategory;
      if (mode.AmmoCategory != null) { effectiveCategory = mode.AmmoCategory; }
      Log.M?.WL(1, "effective ammo category "+effectiveAmmoCategory.Id);
      Statistic ammoIdStat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
      if (ammoIdStat == null) {
        ammoIdStat = weapon.StatCollection.AddStatistic(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.DefaultAmmo.Id);
      }
      string ammoId = ammoIdStat.Value<string>();
      ExtAmmunitionDef ammoDef = CustomAmmoCategories.findExtAmmo(ammoId);
      Log.M?.WL(1, "ammo id " + ammoId+" category:"+(ammoDef == null?"null": ammoDef.AmmoCategory.Id));
      if ((ammoDef == null) || (ammoDef.AmmoCategory.Id != effectiveCategory.Id)) {
        if(effectiveCategory.BaseCategory.Is_NotSet) {
          this.ammo = CustomAmmoCategories.DefaultAmmo;
          this.NoValidAmmo = false;
        } else {
          this.ammo = this.findBestAmmo(effectiveCategory);
        }
      } else {
        this.NoValidAmmo = false;
        this.ammo = ammoDef;
      }
      ammoIdStat.SetValue(this.ammo.Id);
      Log.M?.WL(1, "resulting ammo:"+ ammoIdStat.Value<string>());
      this.HasAmmoVariants = this.isWeaponHasAmmoVariantsNoCache();
      needRevalidate = false;
      HUDSlot?.RefreshDisplayedWeapons();
    }
    public bool isWeaponHasAmmoVariantsNoCache() {
      CustomAmmoCategory ammoCategory = extDef.AmmoCategory;
      if (this.mode.AmmoCategory != null) { ammoCategory = this.mode.AmmoCategory; }
      if (ammoCategory.BaseCategory.Is_NotSet) { return false; }
      List<ExtAmmunitionDef> ammos = this.getAvaibleAmmo(ammoCategory);
      return ammos.Count > 1;
    }

  }
  public static class WeaponStatCacheHelper {
    private static Dictionary<Weapon, WeaponExtendedInfo> weaponExtInfo = new Dictionary<Weapon, WeaponExtendedInfo>();
    private static Dictionary<StatCollection, Weapon> weaponsStatCollections = new Dictionary<StatCollection, Weapon>();
    public static void RegisterStatCollection(this Weapon weapon) {
      weaponsStatCollections[weapon.StatCollection] = weapon;
    }
    public static Weapon getWeapon(this StatCollection statCollection) {
      if (weaponsStatCollections.TryGetValue(statCollection, out var result)) { return result; }
      return null;
    }
    public static void RefreshDisplayedWeapons(this CombatHUDWeaponSlot slot) {
      Traverse.Create(slot).Field<CombatHUD>("HUD").Value.WeaponPanel.RefreshDisplayedWeapons();
    }
    public static WeaponExtendedInfo info(this Weapon weapon) {
      if (weaponExtInfo.TryGetValue(weapon, out var info)) {
        if (info.needRevalidate) { info.Revalidate(); };
      } else {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        info.Revalidate();
      }
      return info;
    }
    public static void Revalidate(this Weapon weapon) {
      if (weaponExtInfo.TryGetValue(weapon, out var info)) {
        if (info.needRevalidate) { info.Revalidate(); };
      } else {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        info.Revalidate();
      }
    }
    public static void SanitizeModeAmmo(this Weapon weapon) {
      CustomAmmoCategory effectiveAmmoCategory = weapon.mode().AmmoCategory;
      if (effectiveAmmoCategory.BaseCategory.Is_NotSet) {
        effectiveAmmoCategory = weapon.exDef().AmmoCategory;
      }
      if (effectiveAmmoCategory.BaseCategory.Is_NotSet) {
        weapon.forceAmmo(string.Empty);
      }
      
      // weapon.mode().AmmoCategory
    }
    public static void Register(this Weapon weapon, WeaponExtendedInfo info) {
      weaponExtInfo.Add(weapon, info);
    }
    //private static Dictionary<Weapon, ExtAmmunitionDef> weaponAmmo = new Dictionary<Weapon, ExtAmmunitionDef>();
    //private static Dictionary<Weapon, WeaponMode> weaponMode = new Dictionary<Weapon, WeaponMode>();
    //private static Dictionary<Weapon, ExtWeaponDef> weaponExte = new Dictionary<Weapon, ExtWeaponDef>();
    //private static Dictionary<Weapon, bool> weaponHasAmmoVariants = new Dictionary<Weapon, bool>();
    public static void Clear() {
      //weaponAmmo.Clear();
      //weaponMode.Clear();
      //weaponExte.Clear();
      //weaponHasAmmoVariants.Clear();
      weaponsStatCollections.Clear();
      weaponExtInfo.Clear();
    }
    public static void ClearAmmoModeCache(this Weapon weapon) {
      if(weaponExtInfo.TryGetValue(weapon, out var info)) {
        info.needRevalidate = true;
      } else {
        weapon.Register(new WeaponExtendedInfo(weapon, weapon.weaponDef));
      }
      weapon.ClearInternalAmmoCache();
    }
    public static bool isHasMode(this Weapon weapon, string modeId) {
      return weapon.exDef().Modes.ContainsKey(modeId);
    }
    public static void forceMode(this Weapon weapon, string modeId) {
      WeaponExtendedInfo info = weapon.info();
      CustomAmmoCategoriesLog.Log.LogWrite("applyWeaponMode(" + weapon.defId + "," + modeId + ")\n");
      if (info.modes.ContainsKey(modeId)) {
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
        }
      } else {
        CustomAmmoCategoriesLog.Log.LogWrite("WARNING! " + weapon.defId + " has no mode " + modeId + "\n", true);
      }
      weapon.ClearAmmoModeCache();
    }
    public static void forceAmmo(this Weapon weapon, string ammoId) {
      WeaponExtendedInfo info = weapon.info();
      CustomAmmoCategoriesLog.Log.LogWrite("applyWeaponAmmo(" + weapon.defId + "," + ammoId + ")\n");
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      }
      weapon.ClearAmmoModeCache();
    }
    public static bool isWeaponHasAmmoVariants(this Weapon weapon) {
      if (weaponExtInfo.TryGetValue(weapon, out var info)) {
        if (info.needRevalidate) { info.Revalidate(); };
      } else {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        info.Revalidate();
      }
      return info.HasAmmoVariants;
      //return weapon.CustomAmmoCategory().BaseCategory.Is_NotSet == false;
    }
    public static ExtAmmunitionDef ammo(this Weapon weapon) {
      if (weaponExtInfo.TryGetValue(weapon, out var info) == false) {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        weapon.Register(info);
      }
      if (info.needRevalidate) { info.Revalidate(); }
      return info.ammo;
    }
    public static ExtWeaponDef exDef(this Weapon weapon) {
      //Log.M.TWL(0, "ext def of:" + weapon.defId);
      if (weapon == null) { return null; }
      if (weapon == null) {
        return CustomAmmoCategories.DefaultWeapon;
        //throw new Exception("exDef() called for null weapon. This should not happen CustomAmmoCategories is just a victim here");
      }
      if (weapon.weaponDef == null) {
        return CustomAmmoCategories.DefaultWeapon;
        //throw new Exception("exDef() called for weapon with null definition. This should not happen CustomAmmoCategories is just a victim here. Weapon uid:" + weapon.uid);
      }
      if (weaponExtInfo.TryGetValue(weapon, out var info) == false) {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        weapon.Register(info);
      }
      if (info.needRevalidate) { info.Revalidate(); }
      return info.extDef;
    }
    public static ExtWeaponDef exDef(this WeaponDef weaponDef) {
      //Log.M.TWL(0, "ext def of:" + weapon.defId);
      ExtWeaponDef exDef = CustomAmmoCategories.getExtWeaponDef(weaponDef.Description.Id);
      return exDef;
    }
    public static WeaponMode mode(this Weapon weapon) {
      //Log.M.TWL(0, "mode of:" + weapon.defId);
      if (weapon == null) {
        throw new Exception("mode() called for null weapon. This should not happen CustomAmmoCategories is just a victim here");
      }
      if (weaponExtInfo.TryGetValue(weapon, out var info) == false) {
        info = new WeaponExtendedInfo(weapon, weapon.weaponDef);
        weapon.Register(info);
      }
      if (info.needRevalidate) { info.Revalidate(); }
      return info.mode;
    }
  }
}