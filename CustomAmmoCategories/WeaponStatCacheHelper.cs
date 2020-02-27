using BattleTech;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CustAmmoCategories {
  public static class WeaponStatCacheHelper {
    private static Dictionary<Weapon, ExtAmmunitionDef> weaponAmmo = new Dictionary<Weapon, ExtAmmunitionDef>();
    private static Dictionary<Weapon, WeaponMode> weaponMode = new Dictionary<Weapon, WeaponMode>();
    private static Dictionary<Weapon, ExtWeaponDef> weaponExte = new Dictionary<Weapon, ExtWeaponDef>();
    private static Dictionary<Weapon, bool> weaponHasAmmoVariants = new Dictionary<Weapon, bool>();
    public static void Clear() {
      weaponAmmo.Clear();
      weaponMode.Clear();
      weaponExte.Clear();
      weaponHasAmmoVariants.Clear();
    }
    public static void ClearAmmoModeCache(this Weapon weapon) {
      weaponAmmo.Remove(weapon);
      weaponMode.Remove(weapon);
      weaponHasAmmoVariants.Remove(weapon);
      weapon.ClearInternalAmmoCache();
    }
    public static bool isWeaponHasAmmoVariants(this Weapon weapon) {
      if (weaponHasAmmoVariants.TryGetValue(weapon, out bool result)) { return result; } else {
        result = weapon.isWeaponHasAmmoVariantsNoCache();
        weaponHasAmmoVariants.Add(weapon, result);
        return result;
      }
      //return weapon.CustomAmmoCategory().BaseCategory.Is_NotSet == false;
    }
    private static bool isWeaponHasAmmoVariantsNoCache(this Weapon weapon) {
      CustomAmmoCategory ammoCategory = weapon.CustomAmmoCategory();
      if (ammoCategory.BaseCategory.Is_NotSet) { return false; }
      List<ExtAmmunitionDef> ammos = weapon.getAvaibleAmmo(ammoCategory);
      return ammos.Count > 1;
    }
    public static ExtAmmunitionDef ammo(this Weapon weapon) {
      //Log.M.TWL(0, "ammo of:" + weapon.defId);
      if (weaponAmmo.TryGetValue(weapon, out ExtAmmunitionDef ammo)) { return ammo; }
      Statistic ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
      if (ammoId == null) { weaponAmmo.Add(weapon, CustomAmmoCategories.DefaultAmmo); return CustomAmmoCategories.DefaultAmmo; }
      ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(ammoId.Value<string>());
      weaponAmmo.Add(weapon, extAmmoDef); return extAmmoDef;
    }
    public static ExtWeaponDef exDef(this Weapon weapon) {
      //Log.M.TWL(0, "ext def of:" + weapon.defId);
      if (weaponExte.TryGetValue(weapon, out ExtWeaponDef eDef)) { return eDef; }
      ExtWeaponDef exDef = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      weaponExte.Add(weapon, exDef);
      return exDef;
    }
    public static WeaponMode mode(this Weapon weapon) {
      //Log.M.TWL(0, "mode of:" + weapon.defId);
      if (weaponMode.TryGetValue(weapon, out WeaponMode mode)) { return mode; }
      ExtWeaponDef extWeapon = weapon.exDef();
      string modeId = extWeapon.baseModeId;
      Statistic modeIdStat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName);
      if (modeIdStat != null) { modeId = modeIdStat.Value<string>(); }
      if (extWeapon.Modes.ContainsKey(modeId)) {
        WeaponMode wmode = extWeapon.Modes[modeId];
        weaponMode.Add(weapon, wmode);
        return wmode;
      }
      weaponMode.Add(weapon, CustomAmmoCategories.DefaultWeaponMode);
      return CustomAmmoCategories.DefaultWeaponMode;
    }
  }
}