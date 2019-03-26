using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustAmmoCategories;
using BattleTech;
using Harmony;
using BattleTech.UI;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static CustomAmmoCategory getWeaponCustomAmmoCategory(Weapon weapon) {
      WeaponMode mode = CustomAmmoCategories.getWeaponMode(weapon);
      if(mode.AmmoCategory == null) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        if (extWeapon.AmmoCategory.BaseCategory != weapon.AmmoCategory) {
          return CustomAmmoCategories.find(weapon.AmmoCategory.ToString());
        }
        return extWeapon.AmmoCategory;
      }
      return mode.AmmoCategory;
    }
    public static string getWeaponAmmoId(Weapon weapon) {
      if (CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon) == CustomAmmoCategories.NotSetCustomAmmoCategoty) { return ""; }
      if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.AmmoIdStatName) == true) {
        return weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      }
      if (weapon.ammoBoxes.Count == 0) { return ""; };
      return weapon.ammoBoxes[0].ammoDef.Description.Id;
    }
    public static bool isWeaponCanShootNoAmmo(WeaponDef weaponDef) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weaponDef.Description.Id);
      if (weaponDef.AmmoCategory == AmmoCategory.NotSet) { return true; };
      if (extWeapon.Modes.Count <= 0) { return false; };
      foreach(var mode in extWeapon.Modes) {
        if (mode.Value.AmmoCategory == null) { continue; };
        if (mode.Value.AmmoCategory.BaseCategory == AmmoCategory.NotSet) { return true; };
      }
      return false;
    }
    public static List<CustomAmmoCategory> getWeaponAmmoCategories(Weapon weapon) {
      List<CustomAmmoCategory> result = new List<CustomAmmoCategory>();
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (weapon.AmmoCategory != AmmoCategory.NotSet) {
        if (extWeapon.AmmoCategory.BaseCategory == weapon.AmmoCategory) {
          result.Add(extWeapon.AmmoCategory);
        } else {
          result.Add(CustomAmmoCategories.find(weapon.AmmoCategory.ToString()));
        }
      }
      foreach (var mode in extWeapon.Modes) {
        if (mode.Value.AmmoCategory == null) { continue; };
        if (mode.Value.AmmoCategory.BaseCategory != AmmoCategory.NotSet) { result.Add(mode.Value.AmmoCategory); };
      }
      return result;
    }
    public static bool isWeaponCanUseAmmo(WeaponDef weaponDef,AmmunitionDef ammoDef) {
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoDef.Description.Id);
      CustomAmmoCategory ammoCategory = extAmmo.AmmoCategory;
      if (ammoCategory.BaseCategory == AmmoCategory.NotSet) { ammoCategory = CustomAmmoCategories.find(ammoDef.Category.ToString()); };
      if (ammoCategory.BaseCategory == AmmoCategory.NotSet) { return false; };
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weaponDef.Description.Id);
      if(extWeapon.AmmoCategory.BaseCategory != AmmoCategory.NotSet) {
        if (extWeapon.AmmoCategory.Index == ammoCategory.Index) { return true; };
      }else
      if(weaponDef.AmmoCategory != AmmoCategory.NotSet) {
        CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.find(weaponDef.AmmoCategory.ToString());
        if ((weaponAmmoCategory.BaseCategory != AmmoCategory.NotSet) && (weaponAmmoCategory.Index == ammoCategory.Index)) { return true; }
      }
      foreach (var mode in extWeapon.Modes) {
        if (mode.Value.AmmoCategory == null) { continue; };
        if ((mode.Value.AmmoCategory.BaseCategory != AmmoCategory.NotSet)&&(mode.Value.AmmoCategory.Index == ammoCategory.Index)) { return true; };
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
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackEvaluator_MakeWeaponSetsForEvasive), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("CalcAndSetAlphaStrikesRemaining")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_CalcAlphaStrikesRem_Patch {
    public static bool Prefix(AbstractActor __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("CalcAndSetAlphaStrikesRemaining "+__instance.DisplayName+"\n");
      if (__instance.ammoBoxes.Count < 1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no ammo boxes\n");
        return true;
      };
      Dictionary<string, int> ammoUsed = new Dictionary<string, int>();
      for (int index1 = 0; index1 < __instance.Weapons.Count; ++index1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" weapon "+ __instance.Weapons[index1].defId+ "\n");
        string ammoId = CustomAmmoCategories.getWeaponAmmoId(__instance.Weapons[index1]);
        CustomAmmoCategoriesLog.Log.LogWrite("  ammoId " + ammoId + "\n");
        if (string.IsNullOrEmpty(ammoId)) { continue; };
        if (ammoUsed.ContainsKey(ammoId)) {
          ammoUsed[ammoId] += __instance.Weapons[index1].ShotsWhenFired;
        } else {
          ammoUsed[ammoId] = __instance.Weapons[index1].ShotsWhenFired;
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  ammoUsed[" + ammoId + "] = "+ ammoUsed[ammoId] + "\n");
      }
      Dictionary<string, int> ammoAvaible = new Dictionary<string, int>();
      for (int index1 = 0; index1 < __instance.ammoBoxes.Count; ++index1) {
        CustomAmmoCategoriesLog.Log.LogWrite(" ammo box  " + __instance.ammoBoxes[index1].defId + "\n");
        string ammoId = __instance.ammoBoxes[index1].ammoDef.Description.Id;
        CustomAmmoCategoriesLog.Log.LogWrite("  ammoId " + ammoId + "\n");
        if (ammoAvaible.ContainsKey(ammoId)) {
          ammoAvaible[ammoId] += __instance.ammoBoxes[index1].CurrentAmmo;
        } else {
          ammoAvaible[ammoId] = __instance.ammoBoxes[index1].CurrentAmmo;
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  ammoAvaible[" + ammoId + "] = " + ammoAvaible[ammoId] + "\n");
      }
      for (int index = 0; index < __instance.Weapons.Count; ++index) {
        CustomAmmoCategoriesLog.Log.LogWrite(" weapon " + __instance.Weapons[index].defId + "\n");
        string ammoId = CustomAmmoCategories.getWeaponAmmoId(__instance.Weapons[index]);
        CustomAmmoCategoriesLog.Log.LogWrite("  ammoId " + ammoId + "\n");
        if (string.IsNullOrEmpty(ammoId)) { continue; };
        int ammoCountAvaible = 0;
        if (ammoAvaible.ContainsKey(ammoId)) { ammoCountAvaible = ammoAvaible[ammoId]; };
        int ammoCountUsed = 0;
        if (ammoUsed.ContainsKey(ammoId)) { ammoCountUsed = ammoUsed[ammoId]; };
        if (ammoCountUsed == 0) { continue; };
        __instance.Weapons[index].AlphaStrikesRemaining = (float)ammoCountAvaible/(float)ammoCountUsed + (float)__instance.Weapons[index].InternalAmmo/(float)__instance.Weapons[index].ShotsWhenFired;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" done\n");
      return false;
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("ResolveSequenceAmmoDepletion")]
  public static class AttackDirector_ResolveSequenceAmmoDepletion {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(AttackDirector_ResolveSequenceAmmoDepletion), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToMech")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToMech {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToMech), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToTurret")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToTurret {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToTurret), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(PoorlyMaintainedEffect))]
  [HarmonyPatch("ApplyEffectsToVehicle")]
  public static class PoorlyMaintainedEffect_ApplyEffectsToVehicle {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(PoorlyMaintainedEffect_ApplyEffectsToVehicle), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon1 {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "ShotsWhenFired").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon1),
          nameof(ShotsWhenFiredDisplayOverider));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static int ShotsWhenFiredDisplayOverider(Weapon weapon) {
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (CustomAmmoCategories.getWeaponDisabledClustering(weapon) == false)) {
        return weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
      } else {
        return weapon.ShotsWhenFired;
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("RefreshDisplayedWeapon")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_RefreshDisplayedWeapon2 {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_RefreshDisplayedWeapon2), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("ShowTextColor")]
  [HarmonyPatch(new Type[] { typeof(Color), typeof(Color), typeof(bool) })]
  public static class CombatHUDWeaponSlot_ShowTextColor {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(CombatHUDWeaponSlot_ShowTextColor), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("EnableWeapon")]
  public static class Weapon_EnableWeapon {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Weapon_EnableWeapon), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("HasAmmo")]
  [HarmonyPatch(MethodType.Getter)]
  public static class Weapon_HasAmmo {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "AmmoCategory").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Weapon_HasAmmo), nameof(AmmoCategory));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }
    private static AmmoCategory AmmoCategory(Weapon weapon) {
      //CustomAmmoCategoriesLog.Log.LogWrite("get AIUtil_UnitHasLOFToTargetFromPosition IndirectFireCapable\n");
      return CustomAmmoCategories.getWeaponCustomAmmoCategory(weapon).BaseCategory;
    }
  }
}
