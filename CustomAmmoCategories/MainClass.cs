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
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using HBS.Util;
using BattleTech;
using BattleTech.UI;
using HBS.Collections;
using Localize;
using BattleTech.Data;
using UnityEngine.EventSystems;
using CustAmmoCategories;
using UnityEngine;
using HBS;
using System.Threading;
using CustomAmmoCategoriesPathes;
using UIWidgets;
using InControl;
using BattleTech.Rendering;
using CustomAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using CustAmmoCategoriesPatches;
using System.Collections.Concurrent;
using MessagePack;
using CustomAmmoCategoriesHelper;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("MechCanUseAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AmmunitionBoxDef) })]
  public static class MechLabPanel_MechCanUseAmmo {
    public static void Postfix(MechLabPanel __instance, AmmunitionBoxDef ammoBoxDef, ref bool __result) {
      if (ammoBoxDef.Ammo.AmmoCategoryValue.IsFlamer) {
        if (CustomAmmoCategories.findExtAmmo(ammoBoxDef.Ammo.Description.Id).AmmoCategory.Id != "Flamer") {
          MechLabMechInfoWidget mechInfoWidget = __instance.mechInfoWidget;
          __result = mechInfoWidget.totalEnergyHardpoints > 0;
        }
      }
    }
  }
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("Cleanup")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AttackSequence_Cleanup {
    public static void Postfix(AttackDirector.AttackSequence __instance) {
      Log.Combat?.WL(0,"AttackDirector.AttackSequence.Cleanup");
      var sortedWeapons = __instance.sortedWeapons;
      foreach (List<Weapon> weapons in sortedWeapons) {
        foreach (Weapon weapon in weapons) {
          Log.Combat?.WL(2, "weapon " + weapon.Name);
          if (weapon.AmmoCategoryValue.Is_NotSet) { continue; }
          if (weapon.ammoBoxes.Count <= 0) { continue; }
          if (weapon.CurrentAmmo > 0) { continue; }
          CustomAmmoCategories.CycleAmmoBest(weapon);
        }
      }
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("RebuildAllLists")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_RebuildAllLists {
    public static void Postfix(CombatGameState __instance) {
      Log.Combat?.WL(0, "CombatGameState.RebuildAllLists");
      foreach (var unit in __instance.AllActors) {
        Log.Combat?.WL(2, unit.DisplayName);
        foreach (var Weapon in unit.Weapons) {
          CustomAmmoCategories.RegisterPlayerWeapon(Weapon);
        }
      }
    }
  }
  [HarmonyPatch(typeof(MechValidationRules))]
  [HarmonyPatch("ValidateMechHasAppropriateAmmo")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechValidationRules_ValidateMechHasAppropriateAmmo {
    public static bool isSameLocation(this MechComponentDef weapon) {
      if(string.IsNullOrEmpty(CustomAmmoCategories.Settings.WeaponUseAmmoInstalledLocationTag)) { return false; }
      return weapon.ComponentTags.Contains(CustomAmmoCategories.Settings.WeaponUseAmmoInstalledLocationTag);
    }
    public static bool Prefix(DataManager dataManager, MechDef mechDef, MechValidationLevel validationLevel, WorkOrderEntry_MechLab baseWorkOrder, ref Dictionary<MechValidationType, List<Text>> errorMessages) {
      List<MechComponentRef> weapons = new List<MechComponentRef>();
      Dictionary<ChassisLocations, Dictionary<string, AmmunitionDef>> ammos = new Dictionary<ChassisLocations, Dictionary<string, AmmunitionDef>>();
      List<BaseComponentRef> inventory = new List<BaseComponentRef>();
      Log.M?.TWL(0,"Start Mech Validation " + mechDef.ChassisID);
      bool same_location = false;
      bool adjacent_location = false;
      if(string.IsNullOrEmpty(CustomAmmoCategories.Settings.WeaponUseAmmoInstalledLocationTag) == false) {
        if(mechDef.Chassis.ChassisTags.Contains(CustomAmmoCategories.Settings.WeaponUseAmmoInstalledLocationTag)) {
          same_location = true;
        }
      }
      if(string.IsNullOrEmpty(CustomAmmoCategories.Settings.WeaponUseAmmoAdjacentLocationTag) == false) {
        if(mechDef.Chassis.ChassisTags.Contains(CustomAmmoCategories.Settings.WeaponUseAmmoAdjacentLocationTag)) {
          adjacent_location = true;
        }
      }
      for(int index = 0; index < mechDef.Inventory.Length; ++index) {
        MechComponentRef mechComponentRef = mechDef.Inventory[index];
        mechComponentRef.RefreshComponentDef();
        if (mechComponentRef.ComponentDefType == ComponentType.Weapon) {
          if (mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || mechComponentRef.DamageLevel == ComponentDamageLevel.NonFunctional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder)) {
            weapons.Add(mechComponentRef);
          }
        } else
        if (mechComponentRef.ComponentDefType == ComponentType.AmmunitionBox) {
          if ((mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder))) {
            AmmunitionBoxDef def = mechComponentRef.Def as AmmunitionBoxDef;
            def.refreshAmmo(dataManager);
            if(ammos.ContainsKey(mechComponentRef.MountedLocation) == false) { ammos[mechComponentRef.MountedLocation] = new Dictionary<string, AmmunitionDef>(); }
            if (ammos[mechComponentRef.MountedLocation].ContainsKey(def.Description.Id) == false) { ammos[mechComponentRef.MountedLocation][def.Description.Id] = def.Ammo; }
          }
        }
        if ((mechComponentRef.DamageLevel == ComponentDamageLevel.Functional || MechValidationRules.MechComponentUnderMaintenance(mechComponentRef, validationLevel, baseWorkOrder))) {
          inventory.Add(mechComponentRef);
        }
      }
      foreach (var weaponRef in weapons) {
        bool weaponHasAmmo = false;
        WeaponDef weaponDef = weaponRef.Def as WeaponDef;
        if (CustomAmmoCategories.isWeaponCanShootNoAmmo(weaponRef, inventory)) { continue; }
        if (weaponDef.StartingAmmoCapacity > 0) { continue; };
        ExtWeaponDef extDef = CustomAmmoCategories.getExtWeaponDef(weaponRef.ComponentDefID);
        if (extDef.isHaveInternalAmmo) { continue; }
        foreach (var ammoLocation in ammos) {
          if(same_location) { if(ammoLocation.Key != weaponRef.MountedLocation) { continue; } }
          if(weaponDef.isSameLocation()) { if(ammoLocation.Key != weaponRef.MountedLocation) { continue; } }
          foreach(var ammoDef in ammoLocation.Value) {
            if(CustomAmmoCategories.isWeaponCanUseAmmo(weaponRef, inventory, ammoDef.Value)) {
              Log.M?.WL(1, $"weapon:{weaponRef.ComponentDefID} SimGameUID:{weaponRef.SimGameUID} can use {ammoDef.Key}");
              weaponHasAmmo = true;
              break;
            }
          }
          if(weaponHasAmmo) { break; }
        }
        if (weaponHasAmmo == false) {
          Log.M?.WL(1, $"weapon:{weaponRef.ComponentDefID} SimGameUID:{weaponRef.SimGameUID} does not have ammo to use");
          string name = string.IsNullOrEmpty(weaponDef.Description.UIName) ? weaponDef.Description.Name : weaponDef.Description.UIName;
          MechValidationRules.AddErrorMessage(ref errorMessages, MechValidationType.AmmoMissing, new Text("__/CAC.MissingAmmo/__", new object[1] { (object)name }));
        }
      }
      foreach (var ammoLocation in ammos) {
        bool ammoIsUsed = false;
        foreach(var ammoDef in ammoLocation.Value) {
          foreach(var weaponRef in weapons) {
            if(same_location) { if(ammoLocation.Key != weaponRef.MountedLocation) { continue; } }
            if(weaponRef.Def.isSameLocation()) { if(ammoLocation.Key != weaponRef.MountedLocation) { continue; } }
            if(CustomAmmoCategories.isWeaponCanUseAmmo(weaponRef, inventory, ammoDef.Value)) {
              Log.M?.WL(1, $"weapon:{weaponRef.ComponentDefID} SimGameUID:{weaponRef.SimGameUID} can use {ammoDef.Key}");
              ammoIsUsed = true;
              break;
            }
          }
          if(ammoIsUsed == false) {
            Log.M?.WL(1, $"ammo:{ammoDef.Key} is not used by any weapon");
            string name = string.IsNullOrEmpty(ammoDef.Value.Description.UIName) ? ammoDef.Value.Description.Name : ammoDef.Value.Description.UIName;
            MechValidationRules.AddErrorMessage(ref errorMessages, MechValidationType.AmmoUnneeded, new Text("__/CAC.ExtraAmmo/__", new object[1] { (object)name }));
          }
        }
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("PlayWeaponEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo) })]
  public static class WeaponRepresentation_PlayWeaponEffect {
    public delegate void d_extendedFire(WeaponEffect weaponEffect, WeaponHitInfo hitInfo, int hitIndex, int emiter);
    public static d_extendedFire i_extendedFire = null;
    public static void Prefix(ref bool __runOriginal,WeaponRepresentation __instance, WeaponHitInfo hitInfo) {
      try {
        if (!__runOriginal) { return; }
        Log.Combat?.TWL(0, $"WeaponRepresentation.PlayWeaponEffect {__instance.transform.name} def:{__instance.weapon.defId} parent:{__instance.weapon.parent.PilotableActorDef.ChassisID}");
        if (__instance.weapon == null) { return; }
        __instance.weapon.clearImpactVFX();
        WeaponEffect currentEffect = CustomAmmoCategories.getWeaponEffect(__instance.weapon);
        if (currentEffect == null) { return; }
        ExtWeaponDef extWeaponDef = CustomAmmoCategories.getExtWeaponDef(__instance.weapon.Description.Id);
        if (hitInfo.numberOfShots == 0) {
          Log.Combat?.WL(1,"no success hits");
          currentEffect.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
          currentEffect.subEffect = false;
          currentEffect.hitInfo = hitInfo;
          currentEffect.FiringComplete(false);
          currentEffect.t(1.0f);
          currentEffect.OnComplete();
        } else {
          if (currentEffect.weapon != null) {
            if (i_extendedFire == null) {
              currentEffect.Fire(hitInfo, 0, 0);
            } else {
              i_extendedFire(currentEffect, hitInfo, 0, 0);
            }
          } else {
            Log.Combat?.TWL(0,"Very strange "+ currentEffect.GetType().ToString()+" have null weapon:"+ __instance.weapon.defId+" initiing.");
            try {
              currentEffect.Init(__instance.weapon);
              if (i_extendedFire == null) {
                currentEffect.Fire(hitInfo, 0, 0);
              } else {
                i_extendedFire(currentEffect, hitInfo, 0, 0);
              }
            } catch (Exception e) {
              Log.Combat?.TWL(0,"Exception:" + e.ToString() + "\nfallbak to no fire");
              Weapon.logger.LogException(e);
              currentEffect.currentState = WeaponEffect.WeaponEffectState.WaitingForImpact;
              currentEffect.subEffect = false;
              currentEffect.hitInfo = hitInfo;
              currentEffect.FiringComplete(false);
              currentEffect.t(1.0f);
              currentEffect.OnComplete();
            }
          }
        }
        Log.Combat?.WL(2,"fired");
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        Weapon.logger.LogException(e);
        return;
      }
    }
  }
  [HarmonyPatch(typeof(WeaponRepresentation))]
  [HarmonyPatch("ResetWeaponEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponRepresentation_ResetWeaponEffect {
    public static void Postfix(WeaponRepresentation __instance) {
      Log.Combat?.WL(0,"WeaponRepresentation.ResetWeaponEffect");
      if (__instance.weapon == null) { return; }
      Log.Combat?.WL(2, "weapon is set");
      Statistic stat = __instance.weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName);
      if (stat == null) { return; }
      Log.Combat?.WL(2, "weapon GUID is set");
      string wGUID = stat.Value<string>();
      CustomAmmoCategories.resetWeaponEffects(wGUID);
      return;
    }
  }
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("ShutdownCombatState")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatGameState_ShutdownCombatState {
    public static void Postfix(CombatGameState __instance) {
      Log.Combat?.WL(0, "CombatGameState.ShutdownCombatState");
      CustomAmmoCategories.clearAllWeaponEffects();
      return;
    }
  }
}

namespace CustAmmoCategories {
  [MessagePackObject]
  public class CustomVector {
    [IgnoreMember]
    public float x { get { return fx; } set { set = true; fx = value; } }
    [IgnoreMember]
    public float y { get { return fy; } set { set = true; fy = value; } }
    [IgnoreMember]
    public float z { get { return fz; } set { set = true; fz = value; } }
    [JsonIgnore, Key(0)]
    public bool set { get; set; }
    [JsonIgnore, Key(1)]
    public float fx;
    [JsonIgnore, Key(2)]
    public float fy;
    [JsonIgnore, Key(3)]
    public float fz;
    public CustomVector() {
      fx = 0f; fy = 0f; fz = 0f; set = false;
    }
    public CustomVector(bool one) {
      set = false;
      if (one) {
        fx = 1f; fy = 1f; fz = 1f;
      } else {
        fx = 0f; fy = 0f; fz = 0f;
      }
    }
    public CustomVector(float x, float y, float z) {
      this.fx = x; this.fy = y; this.fz = z; set = true;
    }
    public CustomVector(CustomVector b) {
      this.fx = b.fx; this.fy = b.fy; this.fz = b.fz; this.set = b.set;
    }
    public override string ToString() {
      return ("set " + this.set + " (" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")");
    }
    [JsonIgnore,IgnoreMember]
    public Vector3 vector { get { return new Vector3(this.x, this.y, this.z); } }
  }
  public class CustomAmmoCategoryRecord {
    public string Id { get; set; }
    public string BaseCategory { get; set; }
  }
  public class CustomAmmoCategory {
    public string Id { get; set; }
    public int Index { get; set; }
    private int BaseCategoryID;
    private static AmmoCategoryValue fallbackCategory = new AmmoCategoryValue();
    public AmmoCategoryValue BaseCategory {
      get {
        AmmoCategoryValue result = AmmoCategoryEnumeration.GetAmmoCategoryByID(BaseCategoryID);
        if (result == null) { return CustomAmmoCategory.fallbackCategory; };
        return result;
      }
      set {
        if (value == null) { BaseCategoryID = 0; return; };
        BaseCategoryID = value.ID;
      }
    }
    public override bool Equals(object o) {
      if (o == null) { return false; };
      if (o is CustomAmmoCategory) {
        return this.Index == (o as CustomAmmoCategory).Index;
      }
      return false;
    }
    public override int GetHashCode() {
      return this.Index;
    }
    public static bool operator ==(CustomAmmoCategory a, CustomAmmoCategory b) {
      if (((object)a == null) && ((object)b == null)) { return true; };
      if ((object)a == null) { return false; };
      if ((object)b == null) { return false; };
      return a.Index == b.Index;
    }
    public static bool operator !=(CustomAmmoCategory a, CustomAmmoCategory b) {
      if (((object)a == null) && ((object)b == null)) { return false; };
      if ((object)a == null) { return true; };
      if ((object)b == null) { return true; };
      return a.Index != b.Index;
    }
    public CustomAmmoCategory() {
      Index = 0;
      BaseCategoryID = 0;
      Id = "NotSet";
    }
    public CustomAmmoCategory(CustomAmmoCategoryRecord record) {
      AmmoCategoryValue val = AmmoCategoryEnumeration.GetAmmoCategoryByName(record.BaseCategory);
      if (val == null) {
        BaseCategoryID = 0;
        Id = "NotSet";
      } else { 
        this.BaseCategoryID = val.ID;
        Id = record.Id;
      }
    }
  }
  public enum HitGeneratorType {
    NotSet,
    Individual,
    Cluster,
    AOE,
    Streak
  }
  public enum TripleBoolean {
    NotSet,
    True,
    False
  }
  public static partial class CustomAmmoCategories {
    public static string AmmoIdStatName = "CurrentAmmoId";
    public static string GUIDStatisticName = "WeaponGUID";
    public static string StreakStatisticName = "Streak";
    public static string WeaponModeStatisticName = "CAC-WeaponMode";
    private static ConcurrentDictionary<string, CustomAmmoCategory> items;
    private static ConcurrentDictionary<string, ExtAmmunitionDef> ExtAmmunitionDef;
    private static ConcurrentDictionary<string, ExtWeaponDef> ExtWeaponDef;
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, WeaponEffect>> WeaponEffects;
    public static CustomAmmoCategory NotSetCustomAmmoCategoty;
    public static ExtAmmunitionDef DefaultAmmo;
    public static ExtWeaponDef DefaultWeapon;
    public static WeaponMode DefaultWeaponMode;
    public static Settings Settings;
    public static Settings GlobalSettings;
    public static Settings LocalSettings;
    private static HashSet<ArmorLocation> trueArmorLocations = new HashSet<ArmorLocation>();
    private static Dictionary<ArmorLocation, ChassisLocations> armorLocationToChassis = new Dictionary<ArmorLocation, ChassisLocations>();
    public static void InitLocationsSubsystem() {
      foreach(var aloc in Enum.GetValues(typeof(ArmorLocation)).Cast<ArmorLocation>()) {
        if(((int)aloc == 1)
          || ((int)aloc == 2)
          || ((int)aloc == 4)
          || ((int)aloc == 8)
          || ((int)aloc == 16)
          || ((int)aloc == 32)
          || ((int)aloc == 64)
          || ((int)aloc == 128)
          || ((int)aloc == 256)
          || ((int)aloc == 512)
          || ((int)aloc == 1024)
          || ((int)aloc == 2048)
          || ((int)aloc == 4096)
          || ((int)aloc == 8192)
        ) {
          trueArmorLocations.Add(aloc);
        }
      }
      foreach(var aloc in trueArmorLocations) {
        string name = aloc.ToString();
        name = name.Replace("Rear", "");
        if(Enum.TryParse<ChassisLocations>(name, out var chloc)) {
          armorLocationToChassis[aloc] = chloc;
        } else {
          throw new Exception($"Can't find ChassisLocation for ArmorLocation {aloc}");
        }
      }
    }
    public static HashSet<ChassisLocations> ConvertArmorToChassisLocations(ArmorLocation aloc) {
      HashSet<ChassisLocations> result = new HashSet<ChassisLocations>();
      foreach(var taloc in trueArmorLocations) {
        if((aloc & taloc) == 0) { continue; }
        if(armorLocationToChassis.TryGetValue(taloc, out var chloc)) { result.Add(chloc); }
      }
      return result;
    }
    public static WeaponHitInfo getSuccessOnly(WeaponHitInfo hitInfo) {
      int successShots = 0;
      for (int index = 0; index < hitInfo.numberOfShots; ++index) {
        if ((hitInfo.hitLocations[index] != 0) && (hitInfo.hitLocations[index] != 65536)) {
          ++successShots;
        }
      }
      WeaponHitInfo result = new WeaponHitInfo();
      result.attackerId = hitInfo.attackerId;
      result.targetId = hitInfo.targetId;
      result.numberOfShots = successShots;
      result.stackItemUID = hitInfo.stackItemUID;
      result.attackSequenceId = hitInfo.attackSequenceId;
      result.attackGroupIndex = hitInfo.attackGroupIndex;
      result.attackWeaponIndex = hitInfo.attackWeaponIndex;
      result.toHitRolls = new float[successShots];
      result.locationRolls = new float[successShots];
      result.dodgeRolls = new float[successShots];
      result.dodgeSuccesses = new bool[successShots];
      result.hitLocations = new int[successShots];
      result.hitPositions = new Vector3[successShots];
      result.hitVariance = new int[successShots];
      result.hitQualities = new AttackImpactQuality[successShots];
      result.attackDirections = new AttackDirection[successShots];
      result.secondaryHitLocations = new int[successShots];
      result.secondaryTargetIds = new string[successShots];
      successShots = 0;
      for (int index = 0; index < hitInfo.numberOfShots; ++index) {
        if ((hitInfo.hitLocations[index] != 0) && (hitInfo.hitLocations[index] != 65536)) {
          result.toHitRolls[successShots] = hitInfo.toHitRolls[index];
          result.locationRolls[successShots] = hitInfo.locationRolls[index];
          result.dodgeRolls[successShots] = hitInfo.dodgeRolls[index];
          result.dodgeSuccesses[successShots] = hitInfo.dodgeSuccesses[index];
          result.hitLocations[successShots] = hitInfo.hitLocations[index];
          result.hitPositions[successShots] = hitInfo.hitPositions[index];
          result.hitVariance[successShots] = hitInfo.hitVariance[index];
          result.hitQualities[successShots] = hitInfo.hitQualities[index];
          result.attackDirections[successShots] = hitInfo.attackDirections[index];
          result.secondaryHitLocations[successShots] = 0;
          result.secondaryTargetIds[successShots] = null;
          ++successShots;
        }
      }
      return result;
    }
    public static void registerExtWeaponDef(string defId, ExtWeaponDef def) {
      if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId) == false) {
        CustomAmmoCategories.ExtWeaponDef.AddOrUpdate(defId, def, (k,v) => { return def; });
      } else {
        CustomAmmoCategories.ExtWeaponDef[defId] = def;
      }
    }
    public static bool isRegistredWeapon(string defId) {
      return CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId);
    }
    public static ExtWeaponDef getExtWeaponDef(string defId) {
      if (CustomAmmoCategories.ExtWeaponDef.ContainsKey(defId)) {
        return ExtWeaponDef[defId];
      } else {
        Log.Combat?.WL(0,"WARNING!" + defId + " is not registed", true);
        return CustomAmmoCategories.DefaultWeapon;
      }
    }
    public static EffectData[] StatusEffects(this Weapon weapon) {
      List<EffectData> result = new List<EffectData>();
      result.AddRange(weapon.weaponDef.statusEffects);
      result.AddRange(weapon.ammo().statusEffects);
      result.AddRange(weapon.mode().statusEffects);
      return result.ToArray();
    }
    public static void clearAllWeaponEffects() {
      CustomAmmoCategories.WeaponEffects.Clear();
    }
    public static void resetWeaponEffects(string wGUID) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return; }
      foreach (var weaponEffect in CustomAmmoCategories.WeaponEffects[wGUID]) {
        if (weaponEffect.Value != (UnityEngine.Object)null) {
          weaponEffect.Value.Reset();
        }
      }
    }
    public static WeaponEffect getWeaponEffect(string wGUID, string weaponEffectId) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == false) { return null; }
      if (CustomAmmoCategories.WeaponEffects[wGUID].ContainsKey(weaponEffectId) == false) { return null; }
      return CustomAmmoCategories.WeaponEffects[wGUID][weaponEffectId];
    }
    public static void ClearWeaponEffects(string wGUID) {
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID)) { WeaponEffects.TryRemove(wGUID, out var val); };
    }
    public static void InitWeaponEffects(WeaponRepresentation weaponRepresentation, Weapon weapon) {
      Log.Combat?.WL(0,"InitWeaponEffects " + weapon.defId + ":" + weapon.parent.DisplayName);
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.GUIDStatisticName) == false) {
        Log.Combat?.WL(1, "no GUID");
        return;
      }
      string wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      if (CustomAmmoCategories.WeaponEffects.ContainsKey(wGUID) == true) {
        Log.Combat?.WL(1, "already contains GUID:" + wGUID);
        return;
      }
      WeaponEffects[wGUID] = new ConcurrentDictionary<string, WeaponEffect>();
      List<ExtAmmunitionDef> avaibleAmmo = CustomAmmoCategories.getAllAvaibleAmmo(weapon);
      foreach (ExtAmmunitionDef extAmmo in avaibleAmmo) {
        if (string.IsNullOrEmpty(extAmmo.WeaponEffectID)) {
          Log.Combat?.WL(2, extAmmo.Id + ".WeaponEffectID is empty");
          continue;
        }
        if (extAmmo.WeaponEffectID == weapon.weaponDef.WeaponEffectID) {
          Log.Combat?.WL(2, extAmmo.Id + ".WeaponEffectID " + extAmmo.WeaponEffectID + " same as per weapon def " + weapon.weaponDef.WeaponEffectID);
          continue;
        }
        if (WeaponEffects[wGUID].ContainsKey(extAmmo.WeaponEffectID) == true) {
          Log.Combat?.WL(2, extAmmo.Id + ".WeaponEffectID " + extAmmo.WeaponEffectID + " already inited");
          continue;
        }
        Log.Combat?.WL(2, "adding predefined weapon effect " + wGUID + "." + extAmmo.WeaponEffectID);
        WeaponEffects[wGUID][extAmmo.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, extAmmo.WeaponEffectID);
      }
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      foreach (var mode in extWeapon.Modes) {
        if (string.IsNullOrEmpty(mode.Value.WeaponEffectID)) {
          Log.Combat?.WL(2, "mode:" + mode.Key + ".WeaponEffectID is empty");
          continue;
        }
        if (mode.Value.WeaponEffectID == weapon.weaponDef.WeaponEffectID) {
          Log.LogWrite("  mode:" + mode.Key + ".WeaponEffectID " + mode.Value.WeaponEffectID + " same as per weapon def " + weapon.weaponDef.WeaponEffectID);
          continue;
        }
        if (WeaponEffects[wGUID].ContainsKey(mode.Value.WeaponEffectID) == true) {
          Log.Combat?.WL(2, "mode:" + mode.Key + ".WeaponEffectID " + mode.Value.WeaponEffectID + " already inited");
          continue;
        }
        Log.Combat?.WL(2, "adding predefined weapon effect " + wGUID + "." + mode.Value.WeaponEffectID);
        WeaponEffects[wGUID][mode.Value.WeaponEffectID] = CustomAmmoCategories.InitWeaponEffect(weaponRepresentation, weapon, mode.Value.WeaponEffectID);
      }
    }
    public static AmmunitionBox getAmmunitionBox(Weapon weapon, int aGUID) {
      if ((aGUID >= 0) && (aGUID < weapon.ammoBoxes.Count)) {
        return weapon.ammoBoxes[aGUID];
      }
      return null;
    }
    public static void RegisterExtAmmoDef(string defId, ExtAmmunitionDef extAmmoDef) {
      Log.M?.WL(0,"Registring extAmmoDef " + defId + " D/H/A " + extAmmoDef.ProjectilesPerShot + "/" + extAmmoDef.HeatDamagePerShot + "/" + extAmmoDef.AccuracyModifier);
      if (ExtAmmunitionDef.ContainsKey(defId) == false) {
        ExtAmmunitionDef.AddOrUpdate(defId, extAmmoDef, (k,v)=> { return extAmmoDef; });
      } else {
        ExtAmmunitionDef[defId] = extAmmoDef;
        Log.M?.WL("already registred");
      }
    }
    public static ExtAmmunitionDef extDef(this AmmunitionDef def) {
      if (string.IsNullOrEmpty(def.Description.Id)) { return CustomAmmoCategories.DefaultAmmo; };
      if (CustomAmmoCategories.ExtAmmunitionDef.ContainsKey(def.Description.Id)) {
        ExtAmmunitionDef result = CustomAmmoCategories.ExtAmmunitionDef[def.Description.Id];
        if (result == null) { return CustomAmmoCategories.DefaultAmmo; }
      }
      return CustomAmmoCategories.DefaultAmmo;
    }
    public static ExtAmmunitionDef findExtAmmo(string ammoDefId) {
      if (string.IsNullOrEmpty(ammoDefId)) { return CustomAmmoCategories.DefaultAmmo; };
      if (CustomAmmoCategories.ExtAmmunitionDef.ContainsKey(ammoDefId)) {
        return CustomAmmoCategories.ExtAmmunitionDef[ammoDefId];
      }
      return CustomAmmoCategories.DefaultAmmo;
    }
    public static void CycleAmmoBest(Weapon weapon) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
      }
      weapon.ClearAmmoModeCache();
    }
    public static List<ExtAmmunitionDef> getAvaibleAmmo(this Weapon weapon, CustomAmmoCategory category) {
      HashSet<ExtAmmunitionDef> result = new HashSet<ExtAmmunitionDef>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; };
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; };
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (ammo.AmmoCategory.Id != category.Id) { continue; };
        result.Add(ammo);
      }
      ExtWeaponDef def = weapon.exDef();
      foreach(var intAmmo in def.InternalAmmo) {
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(intAmmo.Key);
        if (ammo.AmmoCategory.Id != category.Id) { continue; };
        result.Add(ammo);
      }
      if (result.Count == 0) { result.Add(category.defaultAmmo()); }
      Log.M?.TWL(0, "getAvaibleAmmo "+weapon.defId+" category:"+category+" ammo count:"+result.Count);
      foreach (var res in result) {
        Log.M?.WL(1,res.Id);
      }
      return result.ToList();
    }
    public static List<ExtAmmunitionDef> getAllAvaibleAmmo(this Weapon weapon) {
      HashSet<ExtAmmunitionDef> result = new HashSet<ExtAmmunitionDef>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { continue; }
        result.Add(ammo);
      }
      ExtWeaponDef def = weapon.exDef();
      foreach (var intAmmo in def.InternalAmmo) {
        ExtAmmunitionDef ammo = CustomAmmoCategories.findExtAmmo(intAmmo.Key);
        if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { continue; }
        result.Add(ammo);
      }
      return result.ToList();
    }
    public static CustomAmmoCategory getAmmoAmmoCategory(AmmunitionDef ammoDef) {
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoDef.Description.Id);
      CustomAmmoCategory boxAmmoCategory = extAmmo.AmmoCategory;
      if (boxAmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { boxAmmoCategory = CustomAmmoCategories.find(ammoDef.AmmoCategoryValue.Name); }
      return boxAmmoCategory;
    }
    public static bool SyncAmmo(this Weapon trgWeapon, Weapon srcWeapon) {
      if (srcWeapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        return false;
      }
      string AmmoId = srcWeapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      trgWeapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, AmmoId);
      trgWeapon.ClearAmmoModeCache();
      return true;
    }
    public static bool CycleAmmo(Weapon weapon, bool direction = true) {
      Log.Combat?.WL(0,"Cycle Ammo");
      if ((weapon.ammoBoxes.Count == 0)&&(weapon.exDef().InternalAmmo.Count == 0)) {
        Log.Combat?.WL(1, "no ammo");
        return false;
      };
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        Log.Combat?.WL(1, "Current weapon is not set");
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        weapon.ClearAmmoModeCache();
        return true;
      }
      WeaponExtendedInfo info = weapon.info();
      ExtAmmunitionDef CurrentAmmo = info.ammo;
      CustomAmmoCategory weaponAmmoCategory = info.effectiveAmmoCategory;
      if (weaponAmmoCategory.BaseCategory.Is_NotSet) { return false; };
      List<ExtAmmunitionDef> AvaibleAmmo = CustomAmmoCategories.getAvaibleAmmo(weapon, weaponAmmoCategory);
      int CurrentAmmoIndex = AvaibleAmmo.IndexOf(CurrentAmmo);
      if (CurrentAmmoIndex < 0) {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        weapon.ClearAmmoModeCache();
        return true;
      }
      if (AvaibleAmmo.Count == 1) { return false; }
      int nextIndex = (CurrentAmmoIndex + (direction?1:-1)) % AvaibleAmmo.Count;
      if (nextIndex < 0) { nextIndex = AvaibleAmmo.Count - 1; }
      ExtAmmunitionDef tempAmmo = AvaibleAmmo[nextIndex];
      Log.Combat?.WL(3, "cycled to " + tempAmmo.Id);
      weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, tempAmmo.Id);
      weapon.ClearAmmoModeCache();
      return true;
    }
    public static bool SyncMode(this Weapon trgWeapon, Weapon srcWeapon) {
      if (srcWeapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
        return false;
      }
      string ModeId = srcWeapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      trgWeapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, ModeId);
      trgWeapon.ClearAmmoModeCache();
      return true;
    }
    public static bool CycleMode(Weapon weapon, bool direction = true, bool fromUI = true) {
      WeaponExtendedInfo info = weapon.info();
      if (info == null) { Log.Combat?.TWL(0,"!!!!THIS NEVER SHOULD HAPPEND!!!",true); return false; }
      if ((info.allowUIModSwitch == false) && (fromUI == false)) { return false; }
      Log.Combat?.TWL(0,"Cycling mode "+weapon.defId,true);
      if (info.modes.Count <= 1) {
        Log.Combat?.WL(1,"no weapon modes");
        return false;
      }
      Log.Combat?.WL(1, "checking "+ CustomAmmoCategories.WeaponModeStatisticName, true);
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, info.mode.Id);
        Log.Combat?.WL(1, "CustomAmmoCategories.CycleAmmoBest", true);
        CustomAmmoCategories.CycleAmmoBest(weapon);
        return true;
      }
      string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
      Log.Combat?.WL(1, "CustomAmmoCategories.CycleAmmoBest", true);
      CustomAmmoCategory oldWeaponAmmoCategory = info.effectiveAmmoCategory;
      List<WeaponMode> avaibleModes = info.avaibleModes();
      if (avaibleModes.Count == 0) { return false; };
      int nextIndex = direction?0:avaibleModes.Count-1;
      for (int t = 0; t < avaibleModes.Count; ++t) {
        if (avaibleModes[t].Id == modeId) {
          nextIndex = direction?(t + 1) % avaibleModes.Count:(t > 0? t-1:avaibleModes.Count-1);
          break;
        }
      }
      string oldModeId = modeId;
      modeId = avaibleModes[nextIndex].Id;
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
      weapon.ClearAmmoModeCache();
      info.Revalidate();
      CustomAmmoCategory newWeaponAmmoCategory = info.effectiveAmmoCategory;
      if (oldWeaponAmmoCategory.Index != newWeaponAmmoCategory.Index) {
        CustomAmmoCategories.CycleAmmoBest(weapon);
      } else {
        weapon.ClearAmmoModeCache();
      }
      return oldModeId != modeId;
    }
    public static void RegisterPlayerWeapon(Weapon weapon) {
      Log.Combat?.WL(0, "RegisterPlayerWeapon");
      if (weapon == null) {
        Log.Combat?.WL(2, "Weapon is NULL WTF?!");
        return;
      }
      if (weapon.weaponDef == null) {
        Log.Combat?.WL(2, "WeaponDef is NULL");
        return;
      }
      if (weapon.parent == null) {
        Log.Combat?.WL(2, "Parent is NULL");
        return;
      }
      WeaponExtendedInfo info = weapon.info();
      if (info.modes.Count > 0) {
        Log.Combat?.WL(1, "Weapon modes count " + info.modes.Count);
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, info.mode.Id);
          Log.Combat?.WL(1, "Add to weapon stat collection " + info.mode.Id);
          weapon.ClearAmmoModeCache();
        }
      }
      //if (weapon.ammoBoxes.Count > 0) {
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, CustomAmmoCategories.findBestAmmo(weapon));
        Log.Combat?.WL(1, "Add to weapon stat collection " + weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>());
        weapon.ClearAmmoModeCache();
      }
      //}
    }
    public static CustomAmmoCategory findByIndex(int index) {
      foreach (var item in CustomAmmoCategories.items) {
        if (item.Value.Index == index) {
          return item.Value;
        }
      }
      return NotSetCustomAmmoCategoty;
    }
    public static string findBestAmmo(Weapon weapon) {
      CustomAmmoCategory weaponAmmoCategory = CustomAmmoCategories.find(weapon.AmmoCategoryValue.Name);
      WeaponMode mode = weapon.info().mode;
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (mode.AmmoCategory != null) { weaponAmmoCategory = mode.AmmoCategory; } else
      if (extWeapon.AmmoCategory != CustomAmmoCategories.NotSetCustomAmmoCategoty) { weaponAmmoCategory = extWeapon.AmmoCategory; };
      if (weaponAmmoCategory == CustomAmmoCategories.NotSetCustomAmmoCategoty) { return ""; };
      List<ExtAmmunitionDef> avaibleAmmo = weapon.getAvaibleAmmo(weaponAmmoCategory);
      if (avaibleAmmo.Count > 0) { return avaibleAmmo[0].Id; } else {
        ExtAmmunitionDef ammo = weaponAmmoCategory.defaultAmmo();
        Log.Combat?.TWL(0,weapon.defId+" have no ammo for category "+weaponAmmoCategory.Id+" fallback to default "+ammo.Id);
        return ammo.Id;
      }
    }
    public static bool isInited { get; set; } = false;
    public static void CustomCategoriesInit() {
      if (isInited == true) { return; };
      isInited = true;
      CustomAmmoCategories.items = new ConcurrentDictionary<string, CustomAmmoCategory>();
      CustomAmmoCategories.WeaponEffects = new ConcurrentDictionary<string, ConcurrentDictionary<string, WeaponEffect>>();
      CustomAmmoCategories.ExtAmmunitionDef = new ConcurrentDictionary<string, ExtAmmunitionDef>();
      CustomAmmoCategories.ExtWeaponDef = new ConcurrentDictionary<string, ExtWeaponDef>();
      CustomAmmoCategories.amsWeapons = new Dictionary<string, Weapon>();
      foreach (var baseAmmoCat in AmmoCategoryEnumeration.AmmoCategoryList) {
        CustomAmmoCategory itm = new CustomAmmoCategory();
        itm.BaseCategory = baseAmmoCat;
        itm.Id = baseAmmoCat.Name;
        itm.Index = baseAmmoCat.ID;
        items[itm.Id] = itm;
        if (itm.Index == 0) { NotSetCustomAmmoCategoty = itm; };
      }
      CustomAmmoCategories.DefaultAmmo = new ExtAmmunitionDef();
      CustomAmmoCategories.DefaultWeaponMode = new WeaponMode();
      CustomAmmoCategories.DefaultWeapon = new ExtWeaponDef();
      CustomAmmoCategories.printItems();
    }
    public static void printItems() {
      Log.M?.WL(0,"Custom ammo categories:");
      foreach (var itm in items) {
        Log.M?.WL(1,"'" + itm.Key + "'= (" + itm.Value.Index + "/" + itm.Value.Id + "/" + itm.Value.BaseCategory.ToString() + ")");
      }
    }
    public static void printAmmo() {
      Log.M?.WL(0, "DB ammo categories:");
      foreach (var itm in AmmoCategoryEnumeration.AmmoCategoryList) {
        Log.M?.WL(1, "'" + itm.Name + "':"+itm.ID);
      }
    }
    public static bool contains(string id) { return CustomAmmoCategories.items.ContainsKey(id); }
    public static void add (AmmoCategoryValue val) {
      CustomAmmoCategory itm = new CustomAmmoCategory();
      itm.BaseCategory = val;
      itm.Id = val.Name;
      itm.Index = val.ID;
      items[itm.Id] = itm;
    }
    public static CustomAmmoCategory find(string id) {
      if (CustomAmmoCategories.items.ContainsKey(id)) {
        return CustomAmmoCategories.items[id];
      } else {
        return NotSetCustomAmmoCategoty;
      }
    }
  }
}

namespace CustAmmoCategories {
  [SelfDocumentedClass("Settings", "BurnedTreesSettings", "BurnedTreesSettings")]
  public class BurnedTreesSettings {
    public string Mesh { get; set; } = "envMdlTree_deadWood_polar_frozen_shapeA_LOD0";
    public string BumpMap { get; set; } = "envTxrTree_treesVaried_polar_frozen_nrm";
    public string MainTex { get; set; } = "envTxrTree_treesVaried_polar_frozen_alb";
    public string OcculusionMap { get; set; } = "envTxrTree_treesVaried_polar_frozen_amb";
    public string Transmission { get; set; } = "envTxrTree_treesVaried_polar_frozen_trs";
    public string MetallicGlossMap { get; set; } = "envTxrTree_treesVaried_polar_frozen_mtl";
    public float BurnedTreeScale { get; set; } = 2f;
    public float DecalScale { get; set; } = 40f;
    public string DecalTexture { get; set; } = "envTxrDecl_terrainDmgSmallBlack_alb";
  }
  [SelfDocumentedClass("Settings", "BloodSettings", "BloodSettings")]
  public class BloodSettings {
    [SelfDocumentationDefaultValue("empty"), SelfDocumentationTypeName("dictionary { \"<FlimsyDestructType enum>\":<float>}")]
    public Dictionary<FlimsyDestructType, float> DecalScales { get; set; } = new Dictionary<FlimsyDestructType, float>();
    public string DecalTexture { get; set; } = "envTxrDecl_terrainDmgSmallBlood_alb";
    public float DrawBloodChance { get; set; } = 0.7f;
  };
  [SelfDocumentedClass("Settings", "AmmoCookoffSettings", "AmmoCookoffSettings")]
  public class AmmoCookoffSettings {
    public bool Enabled { get; set; }
    public float OverheatChance { get; set; }
    public float ShutdownHeatChance { get; set; }
    public bool UseHBSMercySetting { get; set; }
    public AmmoCookoffSettings() {
      Enabled = false;
      OverheatChance = 10;
      ShutdownHeatChance = 25;
      UseHBSMercySetting = true;
    }
  }
  public static class CACConstants {
    public static readonly string DeployMechDefID = "mechdef_deploy_director";
    public static bool IsDeployDirector(this ICombatant unit) {
      Mech mech = unit as Mech;
      if (mech == null) { return false; }
      if (mech.MechDef.Description.Id == CACConstants.DeployMechDefID) { return true; }
      return false;
    }
  }
  public static class CACCombatState {
    public static bool IsInDeployManualState { get; set; } = false;
  }
  public static class UnitUnaffectionsActorStats {
    public static readonly string DesignMasksActorStat = "CUDesignMasksUnaffected";
    public static readonly string PathingActorStat = "CUPathingUnaffected";
    public static readonly string MoveCostActorStat = "CUMoveCost";
    public static readonly string MoveCostBiomeActorStat = "CUMoveCostBiomeUnaffected";
    public static readonly string FireActorStat = "CUFireActorStatUnaffected";
    public static readonly string LandminesActorStat = "CULandminesUnaffected";
    public static readonly string NoMoveAnimationActorStat = "CUNoMoveAnimation";
    public static readonly string NoRandomIdlesActorStat = "CUNoRandomIdleAnimations";
    public static readonly string NoDependLocaltionsActorStat = "CUNoDependLocations";
    public static readonly string NoDeathOnLegsActorStat = "CUNoDeathOnLegs";
    public static readonly string FlyingHeightActorStat = "CUAOEHeight";
    public static readonly string NoHeatActorStat = "CUNoHeat";
    public static readonly string NoStabilityActorStat = "CUNoStability";
    public static readonly string NoCritTransferActorStat = "CUNoCritTransfer";
    public static readonly string HasNoLegsActorStat = "CUHasNoLegs";
    public static readonly string AlternateRepresentationActorStat = "CUAlternateRepresentation";
    public static readonly string AlternateRepresentationIndexActorStat = "CUAlternateRepresentationIndex";
    public static readonly string BlockComponentsActivationActorStat = "CUBlockComponentsActivation";
    public static readonly string FiringArcActorStat = "CUFiringArc";
    public static readonly string AllowPartialMovementActorStat = "CUAllowPartialMovement";
    public static readonly string AllowPartialSprintActorStat = "CUAllowPartialSprint";
    public static readonly string AllowRotateWhileJumpActorStat = "CUAllowRotateWhileJump";
    public static readonly string FakeVehicleActorStat = "CUFakeVehicle";
    public static readonly string TrooperSquadActorStat = "CUTrooperSquad";
    public static readonly string NavalUnitActorStat = "CUNavalUnit";
    public static readonly string CrewLocationActorStat = "CUCrewLocation";
    public static readonly string InjurePilotOnCrewLocationHitActorStat = "CUInjurePilotOnCrewLocationHit";
    public static readonly string NukeCrewLocationOnEjectActorStat = "CUNukeCrewLocationOnEject";
    public static readonly string PartialMovementSpentActorStat = "CUPartialMovementSpent";
    public static readonly string LastMoveDistanceActorStat = "CULastMoveDistance";
    public static float LastMoveDistance(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(LastMoveDistanceActorStat) == false) { return 0f; };
      return unit.StatCollection.GetStatistic(LastMoveDistanceActorStat).Value<float>();
    }
    public static void LastMoveDistance(this ICombatant unit, float value) {
      if (unit.StatCollection.ContainsStatistic(LastMoveDistanceActorStat) == false) {
        unit.StatCollection.AddStatistic<float>(LastMoveDistanceActorStat, 0f);
      };
      unit.StatCollection.Set<float>(LastMoveDistanceActorStat, value);
    }
    public static float PartialMovementSpent(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(PartialMovementSpentActorStat) == false) { return 0f; };
      return unit.StatCollection.GetStatistic(PartialMovementSpentActorStat).Value<float>();
    }
    public static void PartialMovementSpent(this ICombatant unit, float value) {
      if (unit.StatCollection.ContainsStatistic(PartialMovementSpentActorStat) == false) {
        unit.StatCollection.AddStatistic<float>(PartialMovementSpentActorStat, 0f);
      };
      unit.StatCollection.Set<float>(PartialMovementSpentActorStat, value);
    }
    public static float FiringArc(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(FiringArcActorStat) == false) { return 0f; };
      return unit.StatCollection.GetStatistic(FiringArcActorStat).Value<float>();
    }
    public static void FiringArc(this ICombatant unit, float value) {
      if (unit.StatCollection.ContainsStatistic(FiringArcActorStat) == false) {
        unit.StatCollection.AddStatistic<float>(FiringArcActorStat, 0f);
      };
      unit.StatCollection.Set<float>(FiringArcActorStat,value);
    }
    public static bool NoHeat(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NoHeatActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoHeatActorStat).Value<bool>();
    }
    public static bool NoStability(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NoStabilityActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoStabilityActorStat).Value<bool>();
    }
    public static bool NoCritTransfer(this ICombatant unit) {
      if (unit.FakeVehicle()) { return true; }
      if (unit.StatCollection.ContainsStatistic(NoCritTransferActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoCritTransferActorStat).Value<bool>();
    }
    public static bool HasNoLegs(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(HasNoLegsActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(HasNoLegsActorStat).Value<bool>();
    }
    public static bool UnaffectedDesignMasks(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(DesignMasksActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(DesignMasksActorStat).Value<bool>();
    }
    public static bool BlockComponentsActivation(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(BlockComponentsActivationActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(BlockComponentsActivationActorStat).Value<bool>();
    }
    public static void BlockComponentsActivation(this ICombatant unit, bool value) {
      if (unit.StatCollection.ContainsStatistic(BlockComponentsActivationActorStat) == false) {
        unit.StatCollection.AddStatistic<bool>(BlockComponentsActivationActorStat, false);
      };
      unit.StatCollection.Set<bool>(BlockComponentsActivationActorStat,value);
    }
    public static bool FakeVehicle(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(FakeVehicleActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(FakeVehicleActorStat).Value<bool>();
    }
    public static bool TrooperSquad(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(TrooperSquadActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(TrooperSquadActorStat).Value<bool>();
    }
    public static void FakeVehicle(this ICombatant unit, bool value) {
      if (unit.StatCollection.ContainsStatistic(FakeVehicleActorStat) == false) {
        unit.StatCollection.AddStatistic<bool>(FakeVehicleActorStat, false);
      };
      unit.StatCollection.Set<bool>(FakeVehicleActorStat, value);
    }
    public static void TrooperSquad(this ICombatant unit, bool value) {
      if (unit.StatCollection.ContainsStatistic(TrooperSquadActorStat) == false) {
        unit.StatCollection.AddStatistic<bool>(TrooperSquadActorStat, false);
      };
      unit.StatCollection.Set<bool>(TrooperSquadActorStat, value);
    }
    public static bool NavalUnit(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NavalUnitActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NavalUnitActorStat).Value<bool>();
    }
    public static void NavalUnit(this ICombatant unit, bool value) {
      if (unit.StatCollection.ContainsStatistic(NavalUnitActorStat) == false) {
        unit.StatCollection.AddStatistic<bool>(NavalUnitActorStat, false);
      };
      unit.StatCollection.Set<bool>(NavalUnitActorStat, value);
    }
    public static bool UnaffectedPathing(this ICombatant unit) {
      //return false;
      try {
        if (unit.StatCollection.ContainsStatistic(PathingActorStat) == false) { return false; };
        return unit.StatCollection.GetStatistic(PathingActorStat).Value<bool>();
      } catch (Exception) {
        return false;
      }
    }
    public static bool UnaffectedFire(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(FireActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(FireActorStat).Value<bool>();
    }
    public static float MinefieldTriggerMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(DynamicMapHelper.MINEFIELD_TRIGGER_PROBABILITY_STATISTIC_NAME) == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic(DynamicMapHelper.MINEFIELD_TRIGGER_PROBABILITY_STATISTIC_NAME).Value<float>();
    }
    public static float FlatCritChance(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AdvancedCriticalProcessor.FLAT_CRIT_CHANCE_STAT_NAME) == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic(AdvancedCriticalProcessor.FLAT_CRIT_CHANCE_STAT_NAME).Value<float>();
    }
    public static float BaseCritChance(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AdvancedCriticalProcessor.BASE_CRIT_CHANCE_STAT_NAME) == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic(AdvancedCriticalProcessor.BASE_CRIT_CHANCE_STAT_NAME).Value<float>();
    }
    public static float APCritChance(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AdvancedCriticalProcessor.AP_CRIT_CHANCE_STAT_NAME) == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic(AdvancedCriticalProcessor.AP_CRIT_CHANCE_STAT_NAME).Value<float>();
    }
    public static float FlatCritChance(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{AdvancedCriticalProcessor.FLAT_CRIT_CHANCE_STAT_NAME}") == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic($"{location}.{AdvancedCriticalProcessor.FLAT_CRIT_CHANCE_STAT_NAME}").Value<float>();
    }
    public static float BaseCritChance(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{AdvancedCriticalProcessor.BASE_CRIT_CHANCE_STAT_NAME}") == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic($"{location}.{AdvancedCriticalProcessor.BASE_CRIT_CHANCE_STAT_NAME}").Value<float>();
    }
    public static float APCritChance(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{AdvancedCriticalProcessor.AP_CRIT_CHANCE_STAT_NAME}") == false) { return 1.0f; };
      return unit.StatCollection.GetStatistic($"{location}.{AdvancedCriticalProcessor.AP_CRIT_CHANCE_STAT_NAME}").Value<float>();
    }
    public static bool NoMoveAnimation(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NoMoveAnimationActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoMoveAnimationActorStat).Value<bool>();
    }
    public static bool NoDependentLocations(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NoDependLocaltionsActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoDependLocaltionsActorStat).Value<bool>();
    }
    public static bool UnaffectedLandmines(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(LandminesActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(LandminesActorStat).Value<bool>();
    }
    public static float FlyingHeight(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(FlyingHeightActorStat) == false) { return 0f; };
      return unit.StatCollection.GetStatistic(FlyingHeightActorStat).Value<float>();
    }
    public static void FlyingHeight(this ICombatant unit, float value) {
      if (unit.StatCollection.ContainsStatistic(FlyingHeightActorStat) == false) {
        unit.StatCollection.AddStatistic<float>(FlyingHeightActorStat, 0f);
      };
      unit.StatCollection.Set<float>(FlyingHeightActorStat, value);
    }
    public static string CustomMoveCostKey(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(MoveCostActorStat) == false) { return string.Empty; };
      return unit.StatCollection.GetStatistic(MoveCostActorStat).Value<string>();
    }
    public static bool UnaffectedMoveCostBiome(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(MoveCostBiomeActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(MoveCostBiomeActorStat).Value<bool>();
    }
    public static bool NoRandomIdles(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NoRandomIdlesActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(NoRandomIdlesActorStat).Value<bool>();
    }
    public static void NoRandomIdles(this ICombatant unit, bool value) {
      if (unit.StatCollection.ContainsStatistic(NoRandomIdlesActorStat) == false) {
        unit.StatCollection.AddStatistic<bool>(NoRandomIdlesActorStat, false);
      };
      unit.StatCollection.GetStatistic(NoRandomIdlesActorStat).SetValue<bool>(value);
    }
    public static bool AllowPartialMovement(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AllowPartialMovementActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(AllowPartialMovementActorStat).Value<bool>();
    }
    public static bool AllowPartialSprint(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AllowPartialSprintActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(AllowPartialSprintActorStat).Value<bool>();
    }
    public static bool AllowRotateWhileJump(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AllowRotateWhileJumpActorStat) == false) { return false; };
      return unit.StatCollection.GetStatistic(AllowRotateWhileJumpActorStat).Value<bool>();
    }
    public static bool InjurePilotOnCrewLocationHit(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(InjurePilotOnCrewLocationHitActorStat) == false) { return true; };
      return unit.StatCollection.GetStatistic(InjurePilotOnCrewLocationHitActorStat).Value<bool>();
    }
    public static bool NukeCrewLocationOnEject(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(NukeCrewLocationOnEjectActorStat) == false) { return true; };
      return unit.StatCollection.GetStatistic(NukeCrewLocationOnEjectActorStat).Value<bool>();
    }
    public class CrewLocationRecord {
      private ChassisLocations Chassis = ChassisLocations.Head;
      public HashSet<ArmorLocation> armor { get; set; } = new HashSet<ArmorLocation>();
      public ChassisLocations chassis { get { return this.Chassis; } }
      public void Set(string location) {
        if(Enum.TryParse<ChassisLocations>(location, out this.Chassis) == false) {
          if(Enum.TryParse<VehicleChassisLocations>(location, out var vchassis)) {
            Chassis = vchassis.toFakeChassis();
          } else {
            this.Chassis = ChassisLocations.Head;
          }
        }
        switch (this.Chassis) {
          case ChassisLocations.LeftTorso: this.armor = new HashSet<ArmorLocation>() { ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear }; break;
          case ChassisLocations.RightTorso: this.armor = new HashSet<ArmorLocation>() { ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear }; break;
          case ChassisLocations.CenterTorso: this.armor = new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear }; break;
          default: this.armor = new HashSet<ArmorLocation>() { (ArmorLocation)this.Chassis }; break;
        }
      }
    }
    private static Dictionary<StatCollection, CrewLocationRecord> crewLocation_cache = new Dictionary<StatCollection, CrewLocationRecord>();
    public static void Clear() {
      crewLocation_cache.Clear();
    }
    public static void ResetCrewLocationCache(StatCollection unit) {
      crewLocation_cache.Remove(unit);
    }
    public static ChassisLocations CrewLocationChassis(this ICombatant unit) {
      if(crewLocation_cache.TryGetValue(unit.StatCollection, out var cache)) {
        return cache.chassis;
      }
      string val = unit.StatCollection.GetOrCreateStatisic<string>(CrewLocationActorStat, "Head").Value<string>();
      cache = new CrewLocationRecord();
      cache.Set(val);
      crewLocation_cache.Add(unit.StatCollection, cache);
      return cache.chassis;
    }
    public static HashSet<ArmorLocation> CrewLocationArmor(this ICombatant unit) {
      if (crewLocation_cache.TryGetValue(unit.StatCollection, out var cache)) {
        return cache.armor;
      }
      string val = unit.StatCollection.GetOrCreateStatisic<string>(CrewLocationActorStat, "Head").Value<string>();
      cache = new CrewLocationRecord();
      cache.Set(val);
      crewLocation_cache.Add(unit.StatCollection, cache);
      return cache.armor;
    }
  }
  [SelfDocumentedClass("Settings", "AoEModifiers", "AoEModifiers")]
  public class AoEModifiers {
    public float Range { get; set; } = 1f;
    public float Damage { get; set; } = 1f;
  }
}

namespace CACMain {
  public static class Core {
    public static Harmony harmony { get; set; } = null;
    public static bool MechEngineerDetected { get; set; } = false;
    public static Type privateAssemblyCore { get; set; } = null;
    public static Dictionary<string, GameObject> AdditinalFXObjects { get; set; } = new Dictionary<string, GameObject>();
    public static Dictionary<string, Mesh> AdditinalMeshes = new Dictionary<string, Mesh>();
    public static Dictionary<string, Texture2D> AdditinalTextures = new Dictionary<string, Texture2D>();
    public static Dictionary<string, Material> AdditinalMaterials = new Dictionary<string, Material>();
    public static Dictionary<string, Shader> AdditinalShaders = new Dictionary<string, Shader>();
    public static Dictionary<string, AudioClip> AdditinalAudio = new Dictionary<string, AudioClip>();
    public static Sheepy.BattleTechMod.AttackImprovementMod.ModSettings AIMModSettings = null;
    private static HashSet<Action<MapMetaData>> MapMetadata_Load_Postfixes = new HashSet<Action<MapMetaData>>();
    public static void Add_MapMetadata_Load_Postfix(Action<MapMetaData> postfix) {
      MapMetadata_Load_Postfixes.Add(postfix);
    }
    public static void Call_MapMetadata_Load_Postfixes(MapMetaData metaData) {
      foreach (Action<MapMetaData> postfix in MapMetadata_Load_Postfixes) { postfix(metaData); }
    }
    public static void AIMShowBaseHitChance(CombatHUDWeaponSlot instance, ICombatant target) {
      if (Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowBaseHitChance == null) { return; }
      if (AIMModSettings == null) { return; }
      if (AIMModSettings.ShowBaseHitchance == false) { return; }
      Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowBaseHitChance(instance, target);
    }
    public static void AIMShowBaseMeleeChance(CombatHUDWeaponSlot instance, ICombatant target) {
      if (Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowBaseMeleeChance == null) { return; }
      if (AIMModSettings == null) { return; }
      if (AIMModSettings.ShowBaseHitchance == false) { return; }
      Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowBaseMeleeChance(instance, target);
    }
    public static void AIMShowNeutralRange(CombatHUDWeaponSlot instance, ICombatant target) {
      if (Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowNeutralRange == null) { return; }
      if (AIMModSettings == null) { return; }
      if (AIMModSettings.ShowNeutralRangeInBreakdown == false) { return; }
      Sheepy.BattleTechMod.AttackImprovementMod.APIReference.m_AIMShowNeutralRange(instance, target);
    }
    public static Mesh findMech(string name) {
      if (Core.AdditinalMeshes.ContainsKey(name)) { return Core.AdditinalMeshes[name]; };
      return null;
    }
    public static Texture2D findTexture(string name) {
      if (Core.AdditinalTextures.ContainsKey(name)) { return Core.AdditinalTextures[name]; };
      return null;
    }
    public static GameObject findPrefab(string name) {
      if (Core.AdditinalFXObjects.ContainsKey(name)) { return Core.AdditinalFXObjects[name]; };
      return null;
    }
    public static void DetectOtherMods() {
      foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        if (assembly.FullName.StartsWith("MechEngineer, Version=")) { Core.MechEngineerDetected = true; }
      }
    }
    public static void FinishedLoading(List<string> loadOrder) {
      Log.M?.TWL(0, "FinishedLoading", true);
      try {
        CustomAmmoCategories.CustomCategoriesInit();
        CustomSettings.ModsLocalSettingsHelper.RegisterLocalSettings("CustomAmmoCategoriesSettings","Custom Ammo Categories"
          , LocalSettingsHelper.ResetSettings
          , LocalSettingsHelper.ReadSettings
          , LocalSettingsHelper.DefaultSettings
          , LocalSettingsHelper.CurrentSettings
          , LocalSettingsHelper.SaveSettings
          );
        CustomPrewarm.Core.RegisterSerializator("CustomAmmoCategories", BattleTechResourceType.WeaponDef, CustomAmmoCategories.getExtWeaponDef);
        CustomPrewarm.Core.RegisterSerializator("CustomAmmoCategories", BattleTechResourceType.AmmunitionDef, CustomAmmoCategories.findExtAmmo);
        AccessTools.Method(privateAssemblyCore, "FinishedLoading").Invoke(null, new object[] { });
        LoadLegacyAssets(Log.BaseDirectory);
        BTCustomRenderer_DrawDecals._Prepare();
        UnitCombatStatisticHelper.Init();
        Core.DetectOtherMods();
        PersistentMapClientHelper.Init();
        WeaponDefModesCollectHelper.InitHelper(Core.harmony);
        SimGameState_RestoreMechPostCombat.Init();
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
    public static void LoadLegacyAssets(string directory) {
      try {
        string apath = Path.Combine(directory, "assets");
        Log.M?.WL(0,"additional assets:" + CustomAmmoCategories.Settings.AdditinalAssets.Count);
        foreach (string assetName in CustomAmmoCategories.Settings.AdditinalAssets) {
          string path = Path.Combine(apath, assetName);
          if (File.Exists(path)) {
            var assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle != null) {
              Log.M?.WL(0, "asset " + path + ":" + assetBundle.name + " loaded");
              UnityEngine.GameObject[] objects = assetBundle.LoadAllAssets<GameObject>();
              Log.M?.WL(0, "FX objects:");
              foreach (var obj in objects) {
                Log.M?.WL(1, obj.name);
                if (AdditinalFXObjects.ContainsKey(obj.name) == false) AdditinalFXObjects.Add(obj.name, obj);
              }
              UnityEngine.Texture2D[] textures = assetBundle.LoadAllAssets<Texture2D>();
              Log.M?.WL(0, "Textures:");
              foreach (var tex in textures) {
                Log.M?.WL(1, tex.name);
                if (AdditinalTextures.ContainsKey(tex.name) == false) AdditinalTextures.Add(tex.name, tex);
              }
              UnityEngine.Mesh[] meshes = assetBundle.LoadAllAssets<Mesh>();
              Log.M?.WL(0, "Meshes:");
              foreach (var msh in meshes) {
                Log.M?.WL(1, msh.name);
                if (AdditinalMeshes.ContainsKey(msh.name) == false) AdditinalMeshes.Add(msh.name, msh);
              }
              UnityEngine.Material[] materials = assetBundle.LoadAllAssets<Material>();
              Log.M?.WL(0, "Materials:");
              foreach (var mat in materials) {
                Log.M.WL(1, mat.name + "\n");
                if (AdditinalMaterials.ContainsKey(mat.name) == false) {
                  if (AdditinalMaterials.ContainsKey(mat.name) == false) AdditinalMaterials.Add(mat.name, mat);
                }
              }
              UnityEngine.Shader[] shaders = assetBundle.LoadAllAssets<Shader>();
              Log.M?.WL(0, "Shaders:");
              foreach (var shdr in shaders) {
                Log.M?.WL(1, shdr.name);
                if (AdditinalShaders.ContainsKey(shdr.name) == false) {
                  if (AdditinalShaders.ContainsKey(shdr.name) == false) AdditinalShaders.Add(shdr.name, shdr);
                }
              }
            } else {
              Log.M?.WL(0, "fail to load:" + path);
            }
          } else {
            Log.M?.WL(0, "not exists:" + path);
          }
        }
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
    public static void Init(string directory, string settingsJson) {
      CustomAmmoCategoriesLog.Log.BaseDirectory = directory;
      string settings_filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "CustomAmmoCategoriesSettings.json");
      JObject jsettings = JObject.Parse(File.ReadAllText(settings_filename));
      CustomAmmoCategories.Settings = new CustAmmoCategories.Settings {
        debugLog = (bool)jsettings["debugLog"]
      };
      Log.InitLog();
      Log.M?.TWL(0,"Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version + " debug:"+ CustomAmmoCategories.Settings.debugLog, true);
      Log.M?.TWL(0,"Reading settings");
      //foreach(string arg in Environment.GetCommandLineArgs()) {
      //  Log.M.WL(1, arg);
      //}
      string privateAssemblyPath = Path.Combine(directory,"CustomAmmoCategoriesPrivate.dll");
      Assembly privateAssembly = Assembly.LoadFile(privateAssemblyPath);
      privateAssemblyCore = privateAssembly.GetType("CustomAmmoCategoriesPrivate.Core");
      AccessTools.Method(privateAssemblyCore, "Init").Invoke(null, new object[] { });
      SelfDocumentationHelper.CreateSelfDocumentation(directory);
      CustomAmmoCategories.Settings = JsonConvert.DeserializeObject<CustAmmoCategories.Settings>(File.ReadAllText(settings_filename));
      CustomAmmoCategories.GlobalSettings = JsonConvert.DeserializeObject<CustAmmoCategories.Settings>(File.ReadAllText(settings_filename));
      CustomAmmoCategories.Settings.directory = directory;
      foreach (var dd in CustomAmmoCategories.Settings.DefaultAoEDamageMult) {
        Log.M.WL(1, dd.Key.ToString() + "={range:"+dd.Value.Range+" damage:"+dd.Value.Damage+"}");
      }
      ToHitModifiersHelper.Init();
      CustomAmmoCategories.InitHitLocationsAOE();
      CustomAmmoCategories.InitLocationsSubsystem();
      //try {
      //  Dictionary<string, uint> audioEvents = SceneSingletonBehavior<WwiseManager>.Instance.guidIdMap;
      //  Log.M?.WL(0,"audioEvents:", true);
      //  foreach (var aEvent in audioEvents) {
      //    Log.M?.WL(1, "'" + aEvent.Key+"':"+aEvent.Value);
      //  }
      //} catch(Exception e) {
      //  Log.M?.TWL(0, "Can't audioEvents " + e.ToString(),true);
      //}
      WeaponRealizer.Settings WRSettings = null;
      string WRSettingsFile = Path.Combine(directory, CustomAmmoCategories.Settings.WeaponRealizerSettings);
      if (File.Exists(WRSettingsFile) == false) {
        Log.M?.WL(0, "WeaponRealizer settings not exists");
        Log.flush();
        WRSettings = new WeaponRealizer.Settings();
      } else {
        try {
          Log.M?.WL(0, "Initing WR");
          string WRSettingsContent = File.ReadAllText(WRSettingsFile);
          WRSettings = JsonConvert.DeserializeObject<WeaponRealizer.Settings>(WRSettingsContent);
        } catch (Exception ex) {
          Log.M?.WL(0, ex.ToString());
          UnityGameInstance.logger.LogException(ex);
          WRSettings = new WeaponRealizer.Settings();
        }
      }
      WeaponRealizer.Core.Init(directory, WRSettings);
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.AIMStandalone) == false) {
        string AIMPath = Path.Combine(directory, CustomAmmoCategories.Settings.AIMStandalone);
        if (File.Exists(AIMPath) == false) {
          Log.M?.WL(0, "AttackImprovementMod.dll not exists. I will not load");
          Log.flush();
          throw new Exception("AttackImprovementMod.dll not exists. I will not load");
        } else {
          Log.M?.WL(0, AIMPath + " - exists. Loading assembly.");
          Assembly AIM = Assembly.LoadFile(AIMPath);
          Log.M?.WL(0, "Initing AIM");
          Type AIMMod = AIM.GetType("Sheepy.BattleTechMod.AttackImprovementMod.Mod");
          AIMMod.GetField("failToLoad",BindingFlags.Static|BindingFlags.Public).SetValue(null, false);
          AIMMod.GetMethod("Init", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[2] { directory, string.Empty});
          AIMModSettings = (Sheepy.BattleTechMod.AttackImprovementMod.ModSettings)AIMMod.GetField("AIMSettings").GetValue(null);
        }
      }
      try {
        Core.harmony = new Harmony("io.mission.modrepuation");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        InternalClassPathes.PatchInternalClasses(harmony);
        AccessTools.Field(typeof(FootstepManager), "maxDecals").SetValue(null, 1023);
        Thread thread = new Thread(ThreadWork.DoWork);
        thread.Start();
        CustAmmoCategories.Online.OnlineClientHelper.Init();
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
        UnityGameInstance.logger.LogException(e);
      }
    }
  }
}
