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
using HarmonyLib;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using CustomAmmoCategoriesLog;

namespace CustAmmoCategories {
  public class AmmoModePair {
    public string ammoId { get; set; }
    public string modeId { get; set; }
    public override string ToString() {
      return modeId + ":" + ammoId;
    }
    public AmmoModePair() {
      ammoId = "";
      modeId = "";
    }
    public AmmoModePair(string ammo, string mode) {
      ammoId = ammo;
      modeId = mode;
    }
    public override bool Equals(object o) {
      if (o == null) { return false; }
      if (o is AmmoModePair) {
        return (this.ammoId == (o as AmmoModePair).ammoId) && (this.modeId == (o as AmmoModePair).modeId);
      }
      return false;
    }
    public override int GetHashCode() {
      return (this.ammoId + "/" + this.modeId).GetHashCode();
    }
    public static bool operator ==(AmmoModePair a, AmmoModePair b) {
      if (((object)a == null) && ((object)b == null)) { return true; };
      if ((object)a == null) { return false; };
      if ((object)b == null) { return false; };
      return (a.ammoId == b.ammoId) && (a.modeId == b.modeId);
    }
    public static bool operator !=(AmmoModePair a, AmmoModePair b) {
      if (((object)a == null) && ((object)b == null)) { return false; };
      if ((object)a == null) { return true; };
      if ((object)b == null) { return true; };
      return (a.ammoId != b.ammoId) || (a.modeId != b.modeId);
    }
  }
  public class DamagePredictRecord {
    public AmmoModePair Id { get; set; }
    public float HeatDamageCoeff { get; set; }
    public float PredictHeatDamage { get; set; }
    public float NormDamageCoeff { get; set; }
    public DamagePredictRecord() {
      Id = new AmmoModePair();
      HeatDamageCoeff = 0;
      NormDamageCoeff = 0;
      PredictHeatDamage = 0;
    }
    public DamagePredictRecord(string ammo, string mode) {
      Id = new AmmoModePair(ammo, mode);
      HeatDamageCoeff = 0;
      NormDamageCoeff = 0;
      PredictHeatDamage = 0;
    }
  }
  public static partial class CustomAmmoCategories {
    public static bool DisableInternalWeaponChoose = false;
    public static void applyWeaponAmmoMode(this Weapon weapon, string modeId, string ammoId) {
      WeaponExtendedInfo info = weapon.info();
      Log.Combat?.WL(0,"applyWeaponAmmoMode(" + weapon.defId + "," + modeId + "," + ammoId + ")");
      if (info.modes.ContainsKey(modeId)) {
        if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
        }
      } else {
        Log.Combat?.WL(0, "WARNING! " + weapon.defId + " has no mode " + modeId, true);
      }
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      }
      weapon.ClearAmmoModeCache();
    }
    public static HashSet<string> getWeaponAvaibleAmmoForMode(Weapon weapon, string modeId) {
      HashSet<string> result = new HashSet<string>();
      //CustomAmmoCategory ammoCategory = CustomAmmoCategories.find(weapon.AmmoCategoryValue.Name);
      WeaponExtendedInfo info = weapon.info();
      //if (extWeapon.AmmoCategory.BaseCategory.ID == weapon.AmmoCategoryValue.ID) { ammoCategory = extWeapon.AmmoCategory; }
      if (info.modes.Count < 1) {
        Log.Combat?.WL(0, "WARNING! " + weapon.defId + " has no modes. Even base mode. This means something is very very wrong", true);
        return result;
      }
      if (info.modes.ContainsKey(modeId) == false) {
        Log.Combat?.WL(0, "WARNING! " + weapon.defId + " has no mode " + modeId, true);
        return result;
      }
      WeaponMode weaponMode = info.modes[modeId];
      CustomAmmoCategory effectiveAmmoCategory = info.extDef.AmmoCategory;
      if (weaponMode.AmmoCategory != null) { effectiveAmmoCategory = weaponMode.AmmoCategory; };
      if (effectiveAmmoCategory.BaseCategory.Is_NotSet) { result.Add(""); return result; };
      List<ExtAmmunitionDef> allAmmo = info.getAvaibleAmmo(effectiveAmmoCategory);
      foreach(ExtAmmunitionDef ammo in allAmmo) {
        result.Add(ammo.Id);
      }
      return result;
    }
    public static List<DamagePredictRecord> getWeaponDamagePredict(AbstractActor unit, ICombatant target, Weapon weapon) {
      List<DamagePredictRecord> result = new List<DamagePredictRecord>();
      WeaponExtendedInfo info = weapon.info();
      List<WeaponMode> modes = weapon.AvaibleModes();
      if (info.modes.Count < 1) {
        //Log.LogWrite("WARNING! " + weapon.defId + " has no modes. Even base mode. This means something is very very wrong\n", true);
        return result;
      }
      if(modes.Count < 1) {
        //Log.LogWrite("Weapon has no mode to fire\n");
        return result;
      }
      string currentMode = info.mode.Id;
      string currentAmmo = info.ammo.Id;
      foreach (var mode in modes) {
        HashSet<string> ammos = CustomAmmoCategories.getWeaponAvaibleAmmoForMode(weapon, mode.Id);
        List<int> hitLocations = null;
        float AverageArmor = float.NaN;
        foreach (var ammo in ammos) {
          DamagePredictRecord record = new DamagePredictRecord(ammo, mode.Id);
          CustomAmmoCategories.fillWeaponPredictRecord(ref record, unit, target, weapon, ref hitLocations, ref AverageArmor);
          result.Add(record);
        }
      }
      CustomAmmoCategories.applyWeaponAmmoMode(weapon, currentMode, currentAmmo);
      return result;
    }
    public static int getWeaponPierceLocations(List<int> hitLocations, ICombatant target, float DamagePerShot) {
      int result = 0;
      Log.Combat?.WL(0, "getWeaponPierceLocations " + target.DisplayName + " : " + DamagePerShot);
      foreach (int hitLocation in hitLocations) {
        if (target.ArmorForLocation(hitLocation) <= DamagePerShot) {
          Log.Combat?.WL(1, " location " + hitLocation + " pierced");
          ++result;
        }
      }
      return result;
    }
    public static float getTargetAvarageArmor(List<int> hitLocations, ICombatant target) {
      Log.Combat?.WL(0, "getTargetAvarageArmor " + target.DisplayName);
      float result = 0.0f;
      foreach (int hitLocation in hitLocations) {
        Log.Combat?.WL(1, "location " + hitLocation + " : " + target.ArmorForLocation(hitLocation));
        result += target.ArmorForLocation(hitLocation);
      }
      if (hitLocations.Count > 0) {
        result /= (float)hitLocations.Count;
      } else {
        result = 1.0f;
      }
      if (result < Epsilon) { result = 1.0f; }
      return result;
    }

    //public static bool IsIHaveExposedLocations(this AbstractActor iam) {
    //  if (iam is Vehicle) {
    //    IEnumerator enumerator = Enum.GetValues(typeof(VehicleChassisLocations)).GetEnumerator();
    //    try {
    //      while (enumerator.MoveNext()) {
    //        VehicleChassisLocations current = (VehicleChassisLocations)enumerator.Current;
    //        switch (current) {
    //          case VehicleChassisLocations.None:
    //          case VehicleChassisLocations.MainBody:
    //          case VehicleChassisLocations.All:
    //          case VehicleChassisLocations.Invalid:
    //            continue;
    //          default:
    //            float locArmor = iam.ArmorForLocation((int)current);
    //            if (locArmor < 1.0f) { return true; }
    //            continue;
    //        }
    //      }
    //    } finally {
    //      IDisposable disposable;
    //      if ((disposable = enumerator as IDisposable) != null)
    //        disposable.Dispose();
    //    }
    //  }
    //  if (iam is Mech) {
    //    IEnumerator enumerator = Enum.GetValues(typeof(ArmorLocation)).GetEnumerator();
    //    try {
    //      while (enumerator.MoveNext()) {
    //        ArmorLocation current = (ArmorLocation)enumerator.Current;
    //        switch (current) {
    //          case ArmorLocation.None:
    //          case ArmorLocation.Invalid:
    //            continue;
    //          default:
    //            float locArmor = iam.ArmorForLocation((int)current);
    //            if (locArmor < 1.0f) { return true; }
    //            continue;
    //        }
    //      }
    //    } finally {
    //      IDisposable disposable;
    //      if ((disposable = enumerator as IDisposable) != null)
    //        disposable.Dispose();
    //    }
    //  }
    //  if (iam is Turret) {
    //    float locArmor = (iam as Turret).CurrentArmor;
    //    if (locArmor < 1.0f) { return true; }
    //  }
    //  return false;
    //}
    public static float CalcAMSAIDamageCoeff(this Weapon weapon) {
      if (weapon.parent == null) { return 0f; }
      Log.Combat?.WL(0, "CalcAMSAIDamageCoeff " + weapon.UIName+" " + weapon.parent.DisplayName + ":"+weapon.parent.GUID);
      float result = 0f;
      int missilesCount = 0;
      Log.Combat?.WL(1, "i've detected so far:");
      foreach (AbstractActor enemy in weapon.parent.GetDetectedEnemyUnits()) {
        CustomAmmoCategoriesLog.Log.LogWrite(" "+enemy.DisplayName+":"+enemy.GUID+"\n");
        foreach(Weapon eweapon in enemy.Weapons) {
          Log.LogWrite("  " + eweapon.UIName);
          if (eweapon.CanFire == false) { Log.Combat?.WL(1, "- can't fire"); continue; }
          if (eweapon.AMSImmune()) { Log.Combat?.WL(1, "- AMS imune"); continue; }
          MissileLauncherEffect eweffect = eweapon.getWeaponEffect() as MissileLauncherEffect;
          if (eweffect == null) { Log.Combat?.WL(1, "not missile launcher"); continue; }
          float distance = Vector3.Distance(enemy.CurrentPosition, weapon.parent.CurrentPosition);
          if(distance > eweapon.MaxRange) { Log.Combat?.WL(1, "- out of range"); continue; }
          float toHit = eweapon.GetToHitFromPosition(weapon.parent, 1, enemy.CurrentPosition, weapon.parent.CurrentPosition, true, weapon.parent.IsEvasive, false);
          float predictDamage = eweapon.ShotsWhenFired * toHit * (eweapon.DamagePerShot + eweapon.HeatDamagePerShot) * (weapon.AMSHitChance() + eweapon.AMSHitChance());
          missilesCount += eweapon.ShotsWhenFired;
          result += predictDamage;
          Log.Combat?.WL(1, "- " + predictDamage+"/"+result+"/"+missilesCount);
        }
      }
      if (missilesCount == 0) { return 0f; };
      if (weapon.isAAMS()) {
        foreach (AbstractActor friend in weapon.parent.Combat.GetAllAlliesOf(weapon.parent)) {
          float distance = Vector3.Distance(friend.CurrentPosition, weapon.parent.CurrentPosition);
          if (distance < weapon.MaxRange) {
            Log.Combat?.WL(1, "ally in AAMS range:" + friend.DisplayName+":"+friend.GUID);
            result *= (1f + CustomAmmoCategories.Settings.AAMSAICoeff);
          };
        }
      }
      int AMSCount = (weapon.ShotsWhenFired > missilesCount) ? missilesCount : weapon.ShotsWhenFired;
      result *= (float)weapon.ShotsWhenFired / (float)AMSCount;
      return result;
    }
    public static void fillWeaponPredictRecord(ref DamagePredictRecord record, AbstractActor unit, ICombatant target, Weapon weapon, ref List<int> hitLocations, ref float AverageArmor) {
      Log.Combat?.WL(0, "fillWeaponPredictRecord " + unit.DisplayName + " target " + target.DisplayName + " weapon " + weapon.defId);
      CustomAmmoCategories.applyWeaponAmmoMode(weapon, record.Id.modeId, record.Id.ammoId);
      AbstractActor targetActor = target as AbstractActor;
      if (hitLocations == null) {
        hitLocations = target.GetPossibleHitLocations(unit);
        foreach (int hitLocation in hitLocations) {
          Log.Combat?.WL(1, "Hit Location " + hitLocation);
        }
      }
      if (float.IsNaN(AverageArmor)) {
        AverageArmor = CustomAmmoCategories.getTargetAvarageArmor(hitLocations, target);
      }
      float toHit = 0f;
      if (weapon.WillFireAtTargetFromPosition(target, unit.CurrentPosition) == true) {
        toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
      }
      if (toHit < Epsilon) { record.HeatDamageCoeff = 0f; record.NormDamageCoeff = 0f; record.PredictHeatDamage = 0f; };
      float coolDownCoeff = 1.0f / (1.0f + weapon.Cooldown());
      float jammCoeff = (1.0f - weapon.FlatJammingChance(out string jdescr))/CustomAmmoCategories.Settings.JamAIAvoid;
      float damageJammCoeff = weapon.DamageOnJamming() ? (1.0f / CustomAmmoCategories.Settings.DamageJamAIAvoid) : 1.0f;
      jammCoeff *= damageJammCoeff;
      if (unit.IsAnyStructureExposed) {
        Log.Combat?.WL(1, "i have exposed locations. I will die soon. No fear of jamming or weapon explosion.");
        damageJammCoeff = 1.0f;
        jammCoeff = 1.0f;
      }
      if (weapon.isAMS()) {
        Log.Combat?.WL(1, "AMS detected. Altering calculations");
        record.HeatDamageCoeff = 0f;
        record.NormDamageCoeff = weapon.CalcAMSAIDamageCoeff() * jammCoeff * coolDownCoeff;
      } else {
        float damagePerShot = weapon.DamagePerShot;
        float heatPerShot = weapon.HeatDamagePerShot;
        float damageShotsCount = weapon.ShotsToHits(weapon.ShotsWhenFired);
        if (weapon.HasShells()) {
          damagePerShot /= (float)weapon.ProjectilesPerShot;
        }
        if (weapon.AOECapable()) {
          Log.Combat?.WL(1, "AOE weapon detected. Altering calculations");
          toHit = 1.0f;
          if (target is Mech) {
            int HeadIndex = hitLocations.IndexOf((int)ArmorLocation.Head);
            Log.Combat?.WL(2, "AOE can't hit Mech head:" + HeadIndex);
            if ((HeadIndex >= 0) && (HeadIndex < hitLocations.Count)) { hitLocations.RemoveAt(HeadIndex); };
          }
          damagePerShot *= damageShotsCount;
          Log.Combat?.WL(2, "Full damage " + damagePerShot);
          damagePerShot /= (float)hitLocations.Count;
          Log.Combat?.WL(2, "but spreaded by locations" + damagePerShot);
          damageShotsCount = hitLocations.Count;
          Log.Combat?.WL(2, "hits count " + damageShotsCount);
        }
        float piercedLocationsCount = (float)CustomAmmoCategories.getWeaponPierceLocations(hitLocations, target, damagePerShot);
        float hitLocationsCount = (hitLocations.Count > 0) ? (float)hitLocations.Count : 1.0f;
        float clusterCoeff = 1.0f + ((piercedLocationsCount / hitLocationsCount) * damageShotsCount) * CustomAmmoCategories.Settings.ClusterAIMult;
        float pierceCoeff = 1.0f;
        if (AverageArmor > damagePerShot) {
          pierceCoeff += (damagePerShot / AverageArmor) * CustomAmmoCategories.Settings.PenetrateAIMult;
        }
        record.NormDamageCoeff = damagePerShot * damageShotsCount * toHit * coolDownCoeff * jammCoeff * clusterCoeff * pierceCoeff;
        record.HeatDamageCoeff = heatPerShot * damageShotsCount * toHit * jammCoeff;
        record.PredictHeatDamage = heatPerShot * damageShotsCount * toHit;
        Log.Combat?.WL(1, "toHit = " + toHit);
        Log.Combat?.WL(1, "coolDownCoeff = " + coolDownCoeff);
        Log.Combat?.WL(1, "jammCoeff = " + jammCoeff);
        //CustomAmmoCategoriesLog.Log.LogWrite(" damageJammCoeff = " + damageJammCoeff + "\n");
        Log.Combat?.WL(1, "damageShotsCount = " + damageShotsCount);
        Log.Combat?.WL(1, "damagePerShot = " + damagePerShot);
        Log.Combat?.WL(1, "heatPerShot = " + heatPerShot);
        Log.Combat?.WL(1, "piercedLocationsCount = " + piercedLocationsCount);
        Log.Combat?.WL(1, "hitLocationsCount = " + hitLocationsCount);
        Log.Combat?.WL(1, "AverageArmor = " + AverageArmor);
        Log.Combat?.WL(1, "clusterCoeff = " + clusterCoeff);
        Log.Combat?.WL(1, "pierceCoeff = " + pierceCoeff);
      }
      Log.Combat?.WL(1, "NormDamageCoeff = " + record.NormDamageCoeff);
      Log.Combat?.WL(1, "HeatDamageCoeff = " + record.HeatDamageCoeff);
    }
    public static void ChooseBestWeaponForTarget(AbstractActor unit, ICombatant target, bool isStationary) {
      Stopwatch stopWatch = new Stopwatch();
      Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>();
      Dictionary<string, List<DamagePredictRecord>> damagePredict = new Dictionary<string, List<DamagePredictRecord>>();
      foreach (Weapon weapon in unit.Weapons) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        //if (extWeapon.IsAMS) {
        //  CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.defId + " is AMS exclude from ammo/mode choose\n");
        //  continue;
        //}
        weapons.Add(weapon.uid, weapon);
        damagePredict.Add(weapon.uid, CustomAmmoCategories.getWeaponDamagePredict(unit, target, weapon));
      }
      foreach (var weapon in weapons) {
        Log.Combat?.WL(1, "Weapon " + weapon.Key + " " + weapon.Value.defId);
        foreach (var fireType in damagePredict[weapon.Key]) {
          Log.Combat?.WL(2, "mode:" + fireType.Id.modeId + " ammo:" + fireType.Id.ammoId + " heat:" + fireType.HeatDamageCoeff + " dmg:" + fireType.NormDamageCoeff);
        }
      }
      Mech targetMech = target as Mech;
      if (targetMech != null) {
        Log.Combat?.WL(0, "Try overheat");
        float overallPredictHeatDamage = 0f;
        Dictionary<string, int> weaponsWithHeatFireMode = new Dictionary<string, int>();
        foreach (var weapon in weapons) {
          if (damagePredict.ContainsKey(weapon.Key) == false) {
            Log.Combat?.WL(0, "WARNING! " + weapon.Value.defId + " has no predict damage record something is very very wrong", true);
            continue;
          }
          if (damagePredict[weapon.Key].Count <= 0) {
            Log.Combat?.WL(0, "WARNING! " + weapon.Value.defId + " has empty predict damage record something is very very wrong", true);
            continue;
          }
          float HeatDamageCoeff = damagePredict[weapon.Key][0].HeatDamageCoeff;
          int heatDamageIndex = 0;
          bool haveDiffHeatMode = false;
          for (int index = 1; index < damagePredict[weapon.Key].Count; ++index) {
            DamagePredictRecord fireMode = damagePredict[weapon.Key][index];
            if (HeatDamageCoeff < fireMode.HeatDamageCoeff) { HeatDamageCoeff = fireMode.HeatDamageCoeff; heatDamageIndex = index; haveDiffHeatMode = true; };
          }
          overallPredictHeatDamage += damagePredict[weapon.Key][heatDamageIndex].PredictHeatDamage;
          if (haveDiffHeatMode) {
            weaponsWithHeatFireMode.Add(weapon.Key, heatDamageIndex);
          }
        }
        Log.Combat?.WL(1, "Current target heat:" + targetMech.CurrentHeat + " predicted:" + overallPredictHeatDamage);
        if ((targetMech.CurrentHeat + overallPredictHeatDamage) > targetMech.OverheatLevel) {
          Log.Combat?.WL(2, "worth it");
          foreach (var weapon in weaponsWithHeatFireMode) {
            CustomAmmoCategories.applyWeaponAmmoMode(weapons[weapon.Key], damagePredict[weapon.Key][weapon.Value].Id.modeId, damagePredict[weapon.Key][weapon.Value].Id.ammoId);
            weapons.Remove(weapon.Key);
            damagePredict.Remove(weapon.Key);
          }
        } else {
          Log.Combat?.WL(2, "not worth it");
        }
      }
      Log.Combat?.WL(0, "Normal damage");
      foreach (var weapon in weapons) {
        Log.Combat?.WL(0, "Weapon " + weapon.Key + " " + weapon.Value.defId);
        foreach (var fireType in damagePredict[weapon.Key]) {
          Log.Combat?.WL(1, "mode:" + fireType.Id.modeId + " ammo:" + fireType.Id.ammoId + " heat:" + fireType.HeatDamageCoeff + " dmg:" + fireType.NormDamageCoeff);
        }
      }
      foreach (var weapon in weapons) {
        if (damagePredict.ContainsKey(weapon.Key) == false) {
          Log.Combat?.WL(0, "WARNING! " + weapon.Value.defId + " has no predict damage record something is very very wrong", true);
          continue;
        }
        if (damagePredict[weapon.Key].Count <= 0) {
          Log.Combat?.WL(0, "WARNING! " + weapon.Value.defId + " has empty predict damage record something is very very wrong", true);
          continue;
        }
        float DamageCoeff = damagePredict[weapon.Key][0].NormDamageCoeff;
        int DamageIndex = 0;
        for (int index = 1; index < damagePredict[weapon.Key].Count; ++index) {
          DamagePredictRecord fireMode = damagePredict[weapon.Key][index];
          if (DamageCoeff < fireMode.NormDamageCoeff) { DamageCoeff = fireMode.NormDamageCoeff; DamageIndex = index; };
        }
        CustomAmmoCategories.applyWeaponAmmoMode(weapons[weapon.Key], damagePredict[weapon.Key][DamageIndex].Id.modeId, damagePredict[weapon.Key][DamageIndex].Id.ammoId);
      }
    }
    /*public static bool isWeaponHasDiffirentAmmoModes(Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (extWeapon.Modes.Count > 1) { return true; };
      if (weapon.ammoBoxes.Count == 0) { return false; }
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        if (string.IsNullOrEmpty(ammoId)) { ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id; continue; };
        if (ammoId != weapon.ammoBoxes[index].ammoDef.Description.Id) { return true; }
      }
      return false;
    }
    public static HashSet<string> getWeaponAvaibleAmmoForMode(Weapon weapon,string modeId) {
      CustomAmmoCategory ammoCategory = CustomAmmoCategories.find(weapon.AmmoCategory.ToString());
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if(extWeapon.AmmoCategory.BaseCategory == weapon.AmmoCategory) { ammoCategory = extWeapon.AmmoCategory; }
      WeaponMode weaponMode = CustomAmmoCategories.getWeaponMode(weapon);
      if (weaponMode.AmmoCategory.Index != ammoCategory.Index) { ammoCategory = weaponMode.AmmoCategory; };
      HashSet<string> result = new HashSet<string>();
      if (ammoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { result.Add(""); return result; };
      foreach(AmmunitionBox box in weapon.ammoBoxes) {
        if (box.IsFunctional == false) { continue; }
        if (box.CurrentAmmo <= 0) { continue; }
        CustomAmmoCategory boxAmmoCategory = CustomAmmoCategories.getAmmoAmmoCategory(box.ammoDef);
        if (boxAmmoCategory.Index == ammoCategory.Index) { result.Add(box.ammoDef.Description.Id); }
      }
      return result;
    }
    public static float getWeaponPredictionToHeatModifier(WeaponDef weaponDef,string ammoId, string modeId) {
      float result = 0;

      return result;
    }
    public static bool isWeaponHasDiffirentModes(Weapon weapon) {
      return CustomAmmoCategories.getExtWeaponDef(weapon.defId).Modes.Count > 1;
    }
    public static bool isWeaponHasHeatAmmoMode(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return false; }
      float HeatPerShot = 0;
      bool HeatSetted = false;
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (HeatSetted == false) { HeatSetted = true; HeatPerShot = extAmmo.HeatDamagePerShot; continue; };
        if (HeatPerShot != extAmmo.HeatDamagePerShot) { return true; }
      }
      return false;
    }
    public static bool isWeaponHasClusterAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return false; }
      int ClusterCount = 0;
      bool ClusterSetted = false;
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        int ammoShots = (weapon.weaponDef.ShotsWhenFired + extAmmo.ShotsWhenFired) * (weapon.weaponDef.ProjectilesPerShot + extAmmo.ProjectilesPerShot);
        if (ClusterSetted == false) { ClusterSetted = true; ClusterCount = ammoShots; continue; };
        if (ClusterCount != ammoShots) { return true; }
      }
      return false;
    }
    public static void swtichToBestToHitMode(Weapon weapon,AbstractActor unit, ICombatant target) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      AbstractActor targetActor = (target as AbstractActor);
      string modeId = extWeapon.baseModeId;
      float delta = 1;
      CustomAmmoCategoriesLog.Log.LogWrite("detecting best "+weapon.UIName+" mode to hit "+target.DisplayName+"\n");
      foreach (var mode in extWeapon.Modes) {
        if(CustomAmmoCategories.checkExistance(weapon.StatCollection,CustomAmmoCategories.WeaponModeStatisticName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.WeaponModeStatisticName, mode.Key);
        }else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, mode.Key);
        }
        float toHit = 0;
        if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
          toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
        }
        CustomAmmoCategoriesLog.Log.LogWrite(" Mode "+mode.Key+"\n");
        float curDelta = Math.Abs(toHit - mode.Value.AIHitChanceCap);
        CustomAmmoCategoriesLog.Log.LogWrite(" toHit: " + toHit + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" AIHitCap: " + mode.Value.AIHitChanceCap + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite(" delta: " + curDelta + "\n");
        if (curDelta < delta) {
          delta = curDelta;
          modeId = mode.Key;
        } else
        if(Math.Abs(curDelta-delta) < Epsilon) {
          if(mode.Value.isBaseMode == true) {
            delta = curDelta;
            modeId = mode.Key;
            CustomAmmoCategoriesLog.Log.LogWrite(" base mode\n");
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" Mode choosed\n");
      weapon.StatCollection.Set<string>(CustomAmmoCategories.WeaponModeStatisticName, modeId);
    }
    public static void switchToMostHeatAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return; }
      float HeatPerShot = 0;
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        if (string.IsNullOrEmpty(ammoId)) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          HeatPerShot = extAmmo.HeatDamagePerShot;
          continue;
        };
        if (HeatPerShot < extAmmo.HeatDamagePerShot) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          HeatPerShot = extAmmo.HeatDamagePerShot;
        }
      }
      if (string.IsNullOrEmpty(ammoId) == false) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        }
      }
    }
    public static void switchToMostClusterAmmo(Weapon weapon) {
      if (weapon.ammoBoxes.Count == 0) { return; }
      int ClusterShot = 0;
      string ammoId = "";
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(weapon.ammoBoxes[index].ammoDef.Description.Id);
        int ammoShots = (weapon.weaponDef.ShotsWhenFired + extAmmo.ShotsWhenFired) * (weapon.weaponDef.ProjectilesPerShot + extAmmo.ProjectilesPerShot);
        if (string.IsNullOrEmpty(ammoId)) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          ClusterShot = ammoShots;
          continue;
        };
        if (ClusterShot < ammoShots) {
          ammoId = weapon.ammoBoxes[index].ammoDef.Description.Id;
          ClusterShot = ammoShots;
        }
      }
      if (string.IsNullOrEmpty(ammoId) == false) {
        if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
          weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        } else {
          weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
        }
      }
    }
    public static float calcHeatCoeff(AbstractActor unit, ICombatant target) {
      AbstractActor targetActor = (target as AbstractActor);
      //if (targetActor == null) { return 0.0f; };
      float result = 0;
      foreach (Weapon weapon in unit.Weapons) {
        if (weapon.HeatDamagePerShot == 0) { continue; }
        float toHit = 0f;
        if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
          toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
        }
        result += (
            weapon.ShotsWhenFired
            * weapon.ProjectilesPerShot
            * weapon.HeatDamagePerShot
            * toHit
        );
      }
      return result;
    }
    public static bool hasHittableLocations(Weapon weapon, List<int> hitLocations, Mech target) {
      foreach (int location in hitLocations) {
        if (weapon.DamagePerShot * 2 >= target.ArmorForLocation(location)) {
          return true;
        }
      }
      return false;
    }
    public static List<string> getAvaibleEffectiveAmmo(Weapon weapon) {
      List<string> result = new List<string>();
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (weapon.ammoBoxes[index].CurrentAmmo <= 0) { continue; }
        if (weapon.ammoBoxes[index].IsFunctional == false) { continue; }
        if (result.IndexOf(weapon.ammoBoxes[index].ammoDef.Description.Id) < 0) {
          result.Add(weapon.ammoBoxes[index].ammoDef.Description.Id);
        }
      }
      return result;
    }
    public static void SetWeaponAmmo(Weapon weapon, string ammoId) {
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      } else {
        weapon.StatCollection.Set<string>(CustomAmmoCategories.AmmoIdStatName, ammoId);
      }
    }
    public static void ChooseBestWeaponForTarget(AbstractActor unit, ICombatant target, bool isStationary) {
      /*List<Weapon> ammoWeapons = new List<Weapon>();
      foreach (Weapon weapon in unit.Weapons) {
        if (CustomAmmoCategories.isWeaponHasDiffirentAmmo(weapon) == true) {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has ammo to choose\n");
          ammoWeapons.Add(weapon);
        }
      }
      if (ammoWeapons.Count > 0) {
        if (target is Mech) {
          CustomAmmoCategoriesLog.Log.LogWrite(" Target is mech\n");
          Mech targetMech = (target as Mech);
          List<Weapon> ammoHeatWeapons = new List<Weapon>();
          foreach (Weapon weapon in ammoWeapons) {
            if (CustomAmmoCategories.isWeaponHasHeatAmmo(weapon) == true) {
              CustomAmmoCategories.switchToMostHeatAmmo(weapon);
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has hit ammo\n");
              ammoHeatWeapons.Add(weapon);
            }
          }
          float expectedHeat = CustomAmmoCategories.calcHeatCoeff(unit, target);
          CustomAmmoCategoriesLog.Log.LogWrite(" Expected heat " + expectedHeat + "\n");
          if ((targetMech.CurrentHeat + expectedHeat) > targetMech.OverheatLevel) {
            foreach (Weapon weapon in ammoHeatWeapons) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " - ammo choosed\n");
              ammoWeapons.Remove(weapon);
            }
          }
          List<int> hitLocations = targetMech.GetPossibleHitLocations(unit);
          List<Weapon> ammoClusterWeapon = new List<Weapon>();
          foreach (Weapon weapon in ammoWeapons) {
            if (CustomAmmoCategories.isWeaponHasClusterAmmo(weapon)) {
              CustomAmmoCategories.switchToMostClusterAmmo(weapon);
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has cluster ammo\n");
              ammoClusterWeapon.Add(weapon);
            }
          }
          foreach (Weapon weapon in ammoClusterWeapon) {
            float toHit = 0f;
            if (weapon.parent.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
              toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, targetMech.IsEvasive, false);
            }
            if (toHit < 0.4f) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " cluster toHit is too low " + toHit + "\n");
              continue;
            }
            if (CustomAmmoCategories.hasHittableLocations(weapon, hitLocations, targetMech) == true) {
              CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " can crit one of locations\n");
              ammoWeapons.Remove(weapon);
            }
          }
        }
        AbstractActor targetActor = (target as AbstractActor);
        foreach (Weapon weapon in ammoWeapons) {
          List<string> avaibleAmmo = CustomAmmoCategories.getAvaibleEffectiveAmmo(weapon);
          if (avaibleAmmo.Count == 0) { continue; };
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " choose ammo default algorithm\n");
          string bestAmmo = "";
          float expectedDamage = 0;
          foreach (string ammoId in avaibleAmmo) {
            CustomAmmoCategories.SetWeaponAmmo(weapon, ammoId);
            float toHit = 0f;
            if (unit.HasLOFToTargetUnit(target, weapon.MaxRange, CustomAmmoCategories.getIndirectFireCapable(weapon))) {
              toHit = weapon.GetToHitFromPosition(target, 1, unit.CurrentPosition, target.CurrentPosition, true, (targetActor != null) ? targetActor.IsEvasive : false, false);
            }
            float nonClusterCoeff = 1f;
            int numberOfShots = weapon.ShotsWhenFired * weapon.ProjectilesPerShot;
            if ((toHit > 0.6f) && (numberOfShots == 1)) {
              nonClusterCoeff = 1.2f;
            }
            float tempExpectedDamage = numberOfShots * weapon.DamagePerShot * toHit * nonClusterCoeff;
            CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " toHit " + toHit + " expectedDamage:" + tempExpectedDamage + "\n");
            if (tempExpectedDamage > expectedDamage) {
              expectedDamage = tempExpectedDamage;
              bestAmmo = ammoId;
            }
          }
          if (string.IsNullOrEmpty(bestAmmo) == false) {
            CustomAmmoCategories.SetWeaponAmmo(weapon, bestAmmo);
            CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " best ammo choosed\n");
          }
        }
      }else {
        CustomAmmoCategoriesLog.Log.LogWrite(" No ammo to choose\n");
      }
      List<Weapon> modeWeapons = new List<Weapon>();
      foreach (Weapon weapon in unit.Weapons) {
        if (CustomAmmoCategories.isWeaponHasDiffirentModes(weapon) == true) {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + weapon.UIName + " has mode to choose\n");
          modeWeapons.Add(weapon);
        }
      }
      foreach (Weapon weapon in modeWeapons) {
        CustomAmmoCategories.swtichToBestToHitMode(weapon,unit,target);
      }
    }*/
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrderForTarget")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackEvaluator_MakeAttackOrderForTarget {
    public static bool Prefix(AbstractActor unit, ICombatant target, int enemyUnitIndex, bool isStationary) {
      if (CustomAmmoCategories.DisableInternalWeaponChoose) { return true; }
      Log.Combat?.WL(0, $"{unit.DisplayName} choosing best weapon for target {target.DisplayName}");
      try {
        CustomAmmoCategories.ChooseBestWeaponForTarget(unit, target, isStationary);
        return true;
      } catch (Exception e) {
        Log.Combat?.WL(0, $"Exception " + e.ToString() + "\nFallback to default");
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(AttackEvaluator))]
  [HarmonyPatch("MakeAttackOrder")]
  [HarmonyPatch(MethodType.Normal)]
  public static class AttackEvaluator_MakeAttackOrder {
    public static void Postfix(AbstractActor unit, bool isStationary, BehaviorTreeResults __result) {
      if (CustomAmmoCategories.DisableInternalWeaponChoose) { return; }
      Log.Combat?.WL(0, $"Choose result for " + unit.DisplayName);
      try {
        if (__result.nodeState == BehaviorNodeState.Failure) {
          Log.Combat?.WL(2, $"AI choosed not attack");
        } else
        if (__result.orderInfo is AttackOrderInfo) {
          Log.Combat?.WL(2, $"AI choosed to attack " + (__result.orderInfo as AttackOrderInfo).TargetUnit.DisplayName);
          CustomAmmoCategories.ChooseBestWeaponForTarget(unit, (__result.orderInfo as AttackOrderInfo).TargetUnit, isStationary);
        } else {
          Log.Combat?.WL(2, $"AI choosed something else beside attaking");
        }
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, $"Exception " + e.ToString() + "\nFallback to default\n");
        return;
      }
    }
  }
}
