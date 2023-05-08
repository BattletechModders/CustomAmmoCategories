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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using BattleTech;
using HarmonyLib;
using BattleTech.UI;
using UnityEngine;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    //public static CustomAmmoCategory CustomAmmoCategory(this Weapon weapon) {
    //  if (weapon == null) { return null; }
    //  WeaponMode mode = weapon.mode();
    //  if(mode.AmmoCategory == null) {
    //    ExtWeaponDef extWeapon = weapon.exDef();
    //    if (extWeapon.AmmoCategory.BaseCategory.ID != weapon.AmmoCategoryValue.ID) {
    //      return CustomAmmoCategories.find(weapon.AmmoCategoryValue.Name);
    //    }
    //    return extWeapon.AmmoCategory;
    //  }
    //  return mode.AmmoCategory;
    //}
    //public static string getWeaponAmmoId(Weapon weapon) {
    //  if (weapon.CustomAmmoCategory() == CustomAmmoCategories.NotSetCustomAmmoCategoty) { return ""; }
    //  Statistic stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName);
    //  if (stat != null) { return stat.Value<string>(); }
    //  if (weapon.ammoBoxes.Count == 0) { return ""; };
    //  return weapon.ammoBoxes[0].ammoDef.Description.Id;
    //}
    public static bool isWeaponCanShootNoAmmo(this MechComponentRef weaponRef, List<BaseComponentRef> inventory) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weaponRef.ComponentDefID);
      WeaponDef weaponDef = weaponRef.Def as WeaponDef;
      if (weaponDef.AmmoCategoryValue.Is_NotSet) { return true; };
      List<WeaponMode> modes = weaponRef.WeaponModes(inventory);
      if (modes.Count <= 0) { return false; };
      foreach(var mode in modes) {
        if (mode.AmmoCategory == null) { continue; };
        if (mode.AmmoCategory.BaseCategory.Is_NotSet) { return true; };
      }
      return false;
    }
    //public static List<CustomAmmoCategory> getWeaponAmmoCategories(Weapon weapon) {
    //  List<CustomAmmoCategory> result = new List<CustomAmmoCategory>();
    //  ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
    //  if (weapon.AmmoCategoryValue.Is_NotSet == false) {
    //    if (extWeapon.AmmoCategory.BaseCategory.ID == weapon.AmmoCategoryValue.ID) {
    //      result.Add(extWeapon.AmmoCategory);
    //    } else {
    //      result.Add(CustomAmmoCategories.find(weapon.AmmoCategoryValue.Name));
    //    }
    //  }
    //  foreach (var mode in extWeapon.Modes) {
    //    if (mode.Value.AmmoCategory == null) { continue; };
    //    if (mode.Value.AmmoCategory.BaseCategory.Is_NotSet == false) { result.Add(mode.Value.AmmoCategory); };
    //  }
    //  return result;
    //}
    public static bool isWeaponCanUseAmmo(this BaseComponentRef weaponRef, List<BaseComponentRef> inventory, AmmunitionDef ammoDef) {
      Log.M?.WL(0,$"Cheching if weapon {weaponRef.ComponentDefID} SimGameUID:{weaponRef.SimGameUID} can use ammo {ammoDef.Description.Id}");
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoDef.Description.Id);
      CustomAmmoCategory ammoCategory = extAmmo.AmmoCategory;
      if (ammoCategory.BaseCategory.Is_NotSet) { ammoCategory = CustomAmmoCategories.find(ammoDef.AmmoCategoryValue.Name); };
      if (ammoCategory.BaseCategory.Is_NotSet) { Log.M?.WL(1, "ammo have bad category"); return false; };
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weaponRef.ComponentDefID);
      WeaponDef weaponDef = weaponRef.Def as WeaponDef;
      List<WeaponMode> modes = weaponRef.WeaponModes(inventory);
      if (modes.Count <= 0) { Log.M?.WL(1,"no modes"); return false; };
      if (extWeapon.AmmoCategory.BaseCategory.Is_NotSet == false) {
        if (extWeapon.AmmoCategory.Index == ammoCategory.Index) { return true; };
      }else
      if(weaponDef.AmmoCategoryValue.Is_NotSet == false) {
        CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.find(weaponDef.AmmoCategoryValue.Name);
        if ((weaponAmmoCategory.BaseCategory.Is_NotSet == false) && (weaponAmmoCategory.Index == ammoCategory.Index)) { return true; }
      }
      foreach (var mode in modes) {
        if (mode.AmmoCategory == null) { continue; };
        if ((mode.AmmoCategory.BaseCategory.Is_NotSet == false) &&(mode.AmmoCategory.Index == ammoCategory.Index)) { return true; };
      }
      return false;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeWeaponSetsForEvasive")]
  public static class AttackEvaluator_MakeWeaponSetsForEvasive {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackEvaluator_MakeWeaponSetsForEvasive), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(this Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("CalcAndSetAlphaStrikesRemaining")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_CalcAlphaStrikesRem_Patch {
    public static bool Prefix(AbstractActor __instance) {
      Log.M?.WL(0,"CalcAndSetAlphaStrikesRemaining "+__instance.DisplayName);
      if (__instance.ammoBoxes.Count < 1) {
        Log.M?.WL(1, "no ammo boxes");
        return true;
      };
      Dictionary<string, int> ammoUsed = new Dictionary<string, int>();
      for (int index1 = 0; index1 < __instance.Weapons.Count; ++index1) {
        Log.M?.WL(1, "weapon " + __instance.Weapons[index1].defId);
        string ammoId = __instance.Weapons[index1].info().ammo.Id;//CustomAmmoCategories.getWeaponAmmoId(__instance.Weapons[index1]);
        Log.M?.WL(2, "ammoId " + ammoId);
        if (string.IsNullOrEmpty(ammoId)) { continue; };
        if (ammoId == CustomAmmoCategories.DefaultAmmo.Id) { continue; };
        if (ammoUsed.ContainsKey(ammoId)) {
          ammoUsed[ammoId] += __instance.Weapons[index1].ShotsWhenFired;
        } else {
          ammoUsed[ammoId] = __instance.Weapons[index1].ShotsWhenFired;
        }
        Log.M?.WL(3, "ammoUsed[" + ammoId + "] = "+ ammoUsed[ammoId]);
      }
      Dictionary<string, int> ammoAvaible = new Dictionary<string, int>();
      for (int index1 = 0; index1 < __instance.ammoBoxes.Count; ++index1) {
        Log.M?.WL(1, "ammo box  " + __instance.ammoBoxes[index1].defId);
        string ammoId = __instance.ammoBoxes[index1].ammoDef.Description.Id;
        Log.M?.WL(2, "ammoId " + ammoId);
        if (ammoAvaible.ContainsKey(ammoId)) {
          ammoAvaible[ammoId] += __instance.ammoBoxes[index1].CurrentAmmo;
        } else {
          ammoAvaible[ammoId] = __instance.ammoBoxes[index1].CurrentAmmo;
        }
        Log.M?.WL(2, "ammoAvaible[" + ammoId + "] = " + ammoAvaible[ammoId]);
      }
      for (int index = 0; index < __instance.Weapons.Count; ++index) {
        Log.M?.WL(1, "weapon " + __instance.Weapons[index].defId);
        string ammoId = __instance.Weapons[index].info().ammo.Id;
        Log.M?.WL(2, "ammoId " + ammoId);
        if (string.IsNullOrEmpty(ammoId)) { continue; };
        if (ammoId == CustomAmmoCategories.DefaultAmmo.Id) { continue; };
        int ammoCountAvaible = 0;
        if (ammoAvaible.ContainsKey(ammoId)) { ammoCountAvaible = ammoAvaible[ammoId]; };
        int ammoCountUsed = 0;
        if (ammoUsed.ContainsKey(ammoId)) { ammoCountUsed = ammoUsed[ammoId]; };
        if (ammoCountUsed == 0) { continue; };
        __instance.Weapons[index].AlphaStrikesRemaining = (float)ammoCountAvaible/(float)ammoCountUsed + (float)__instance.Weapons[index].InternalAmmo/(float)__instance.Weapons[index].ShotsWhenFired;
      }
      Log.M?.WL(1, "done");
      return false;
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("ResolveSequenceAmmoDepletion")]
  public static class AttackDirector_ResolveSequenceAmmoDepletion {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackDirector_ResolveSequenceAmmoDepletion), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToMech")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToMech {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToMech), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToTurret")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToTurret {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToTurret), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToVehicle")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToVehicle {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToVehicle), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(int?), typeof(bool), typeof(bool) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon1 {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ShotsWhenFired").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon1),
          nameof(ShotsWhenFiredDisplayOverider));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static int ShotsWhenFiredDisplayOverider(Weapon weapon) {
      //Log.LogWrite("ShotsWhenFiredDisplayOverider "+weapon.UIName+"\n");
      int result = weapon.ShotsWhenFired;
      if (weapon.isImprovedBallistic() == false) {
        result = weapon.ShotsToHits(result);
      }
      return result;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(int?), typeof(bool), typeof(bool) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon2 {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon2), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("ShowTextColor")]
  [HarmonyPatch(new Type[] { typeof(Color), typeof(Color), typeof(Color), typeof(bool) })]
  public static class CombatHUDWeaponSlot_ShowTextColor {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_ShowTextColor), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("EnableWeapon")]
  public static class Weapon_EnableWeapon {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Weapon_EnableWeapon), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("HasAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  public static class Weapon_HasAmmo {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategoryValue").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Weapon_HasAmmo), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategoryValue AmmoCategory(Weapon weapon) {
      if (weapon == null) {
        throw new Exception("HasAmmo called for null weapon. This should not happen CustomAmmoCategories is just a victim here");
      }
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return weapon.info().effectiveAmmoCategory.BaseCategory;
    }
  }
}
