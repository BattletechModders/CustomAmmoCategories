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
using CustomAmmoCategoriesLog;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public class DynamicWeaponHitInfo {
    public int numberOfShots;
    public List<string> secondaryTargetIds;
    public List<int> secondaryHitLocations;
    public List<float> toHitRolls;
    public List<float> locationRolls;
    public List<float> dodgeRolls;
    public List<bool> dodgeSuccesses;
    public List<int> hitLocations;
    public List<int> hitVariance;
    public List<AttackImpactQuality> hitQualities;
    public List<AttackDirection> attackDirections;
    public List<Vector3> hitPositions;
    public DynamicWeaponHitInfo() {
      numberOfShots = 0;
      secondaryTargetIds = new List<string>();
      secondaryHitLocations = new List<int>();
      toHitRolls = new List<float>();
      locationRolls = new List<float>();
      dodgeRolls = new List<float>();
      dodgeSuccesses = new List<bool>();
      hitLocations = new List<int>();
      hitVariance = new List<int>();
      hitQualities = new List<AttackImpactQuality>();
      attackDirections = new List<AttackDirection>();
      hitPositions = new List<Vector3>();
    }
    public void Add(ref WeaponHitInfo hitInfo, int hitIndex) {
      if (hitIndex < 0) { return; }
      if (hitIndex >= hitInfo.numberOfShots) { return; }
      if (hitIndex >= hitInfo.secondaryTargetIds.Length) { return; }
      if (hitIndex >= hitInfo.secondaryHitLocations.Length) { return; }
      if (hitIndex >= hitInfo.toHitRolls.Length) { return; }
      if (hitIndex >= hitInfo.locationRolls.Length) { return; }
      if (hitIndex >= hitInfo.dodgeRolls.Length) { return; }
      if (hitIndex >= hitInfo.dodgeSuccesses.Length) { return; }
      if (hitIndex >= hitInfo.hitLocations.Length) { return; }
      if (hitIndex >= hitInfo.hitVariance.Length) { return; }
      if (hitIndex >= hitInfo.hitQualities.Length) { return; }
      if (hitIndex >= hitInfo.attackDirections.Length) { return; }
      if (hitIndex >= hitInfo.hitPositions.Length) { return; }
      secondaryTargetIds.Add(hitInfo.secondaryTargetIds[hitIndex]);
      secondaryHitLocations.Add(hitInfo.secondaryHitLocations[hitIndex]);
      toHitRolls.Add(hitInfo.toHitRolls[hitIndex]);
      locationRolls.Add(hitInfo.locationRolls[hitIndex]);
      dodgeRolls.Add(hitInfo.dodgeRolls[hitIndex]);
      dodgeSuccesses.Add(hitInfo.dodgeSuccesses[hitIndex]);
      hitLocations.Add(hitInfo.hitLocations[hitIndex]);
      hitVariance.Add(hitInfo.hitVariance[hitIndex]);
      hitQualities.Add(hitInfo.hitQualities[hitIndex]);
      attackDirections.Add(hitInfo.attackDirections[hitIndex]);
      hitPositions.Add(hitInfo.hitPositions[hitIndex]);
      numberOfShots += 1;
    }
    public void ToHitInfo(ref WeaponHitInfo hitInfo) {
      hitInfo.numberOfShots = this.numberOfShots;
      hitInfo.secondaryTargetIds = secondaryTargetIds.ToArray();
      hitInfo.secondaryHitLocations = secondaryHitLocations.ToArray();
      hitInfo.toHitRolls = toHitRolls.ToArray();
      hitInfo.locationRolls = locationRolls.ToArray();
      hitInfo.dodgeRolls = dodgeRolls.ToArray();
      hitInfo.dodgeSuccesses = dodgeSuccesses.ToArray();
      hitInfo.hitLocations = hitLocations.ToArray();
      hitInfo.hitVariance = hitVariance.ToArray();
      hitInfo.hitQualities = hitQualities.ToArray();
      hitInfo.attackDirections = attackDirections.ToArray();
      hitInfo.hitPositions = hitPositions.ToArray();
    }
  }
  public static class TempAmmoCountHelper {
    private static Dictionary<Weapon, Dictionary<string, int>> tempWeaponAmmoCount = new Dictionary<Weapon, Dictionary<string, int>>();
    private static Dictionary<AmmunitionBox, int> tempAmmoBoxAmmoCount = new Dictionary<AmmunitionBox, int>();
    private static HashSet<Weapon> assaultTurretsWeapon = new HashSet<Weapon>();
    private static Dictionary<Weapon, AmmunitionBox> cachedAmmoBoxes = new Dictionary<Weapon, AmmunitionBox>();
    private static Dictionary<Weapon, List<AmmunitionBox>> sortedAmmoBoxes = new Dictionary<Weapon, List<AmmunitionBox>>();
    public static void Clear() {
      tempWeaponAmmoCount.Clear();
      tempAmmoBoxAmmoCount.Clear();
      assaultTurretsWeapon.Clear();
      cachedAmmoBoxes.Clear();
      sortedAmmoBoxes.Clear();
    }
    public static void FillAmmoBoxesCacheManual(this Weapon weapon) {
      sortedAmmoBoxes.Remove(weapon);
      List<AmmunitionBox> allboxes = new List<AmmunitionBox>();
      List<AmmunitionBox> sortedboxes = new List<AmmunitionBox>();
      allboxes.AddRange(weapon.ammoBoxes);
      List<AmmoBoxOderDataElementDef> sortOrder = weapon.info().sortingDef.ammoBoxesOrder;
      for (int i = 0; i < sortOrder.Count; ++i) {
        for(int ii=0; ii < allboxes.Count; ++ii) {
          if((sortOrder[i].ammoBoxDefId == allboxes[ii].defId) && ((int)sortOrder[i].mountLocation == allboxes[ii].Location)) {
            sortedboxes.Add(allboxes[ii]);
            allboxes.RemoveAt(ii);
            break;
          }
        }
      }
      sortedboxes.AddRange(allboxes);
      sortedAmmoBoxes.Add(weapon, sortedboxes);
    }
    public static void FillAmmoBoxesCache(this Weapon weapon) {
      if (weapon.info().sortingDef != null) {
        if (weapon.info().sortingDef.automaticAmmoBoxesOrder) {
          weapon.FillAmmoBoxesCacheManual();
          return;
        }
      }
      sortedAmmoBoxes.Remove(weapon);
      List<AmmunitionBox> boxes = new List<AmmunitionBox>();
      boxes.AddRange(weapon.ammoBoxes);
      Dictionary<AmmunitionBox, float> boxesArmor = new Dictionary<AmmunitionBox, float>();
      Mech mech = weapon.parent as Mech;
      Vehicle vehicle = weapon.parent as Vehicle;
      Turret turret = weapon.parent as Turret;
      foreach(AmmunitionBox box in boxes) {
        if (mech != null) {
          MechComponentRef mechRef = box.mechComponentRef;
          float armor = 0f;
          switch (mechRef.MountedLocation) {
            case ChassisLocations.CenterTorso: armor = Mathf.Min(mech.GetCurrentArmor(ArmorLocation.CenterTorso),mech.GetCurrentArmor(ArmorLocation.CenterTorsoRear)); break;
            case ChassisLocations.LeftTorso: armor = Mathf.Min(mech.GetCurrentArmor(ArmorLocation.LeftTorso), mech.GetCurrentArmor(ArmorLocation.LeftTorsoRear)); break;
            case ChassisLocations.RightTorso: armor = Mathf.Min(mech.GetCurrentArmor(ArmorLocation.RightTorso), mech.GetCurrentArmor(ArmorLocation.RightTorsoRear)); break;
            case ChassisLocations.LeftArm: armor = mech.GetCurrentArmor(ArmorLocation.LeftArm); break;
            case ChassisLocations.RightArm: armor = mech.GetCurrentArmor(ArmorLocation.RightArm); break;
            case ChassisLocations.LeftLeg: armor = mech.GetCurrentArmor(ArmorLocation.LeftLeg); break;
            case ChassisLocations.RightLeg: armor = mech.GetCurrentArmor(ArmorLocation.RightLeg); break;
          }
          boxesArmor.Add(box,armor);
        } else if(vehicle != null) {
          VehicleComponentRef vehicleRef = box.vehicleComponentRef;
          boxesArmor.Add(box, vehicle.GetCurrentArmor(vehicleRef.MountedLocation));
        } else if(turret != null) {
          TurretComponentRef turretRef = box.turretComponentRef;
          boxesArmor.Add(box, turret.GetCurrentArmor(BuildingLocation.Structure));
        } else {
          boxesArmor.Add(box, 0f);
        }
      }
      boxes.Sort((Comparison<AmmunitionBox>)((x, y) => (boxesArmor[x].CompareTo(boxesArmor[y]))));
      sortedAmmoBoxes.Add(weapon,boxes);
    }
    public static void DecrementAmmo(this Weapon weapon, ref WeaponHitInfo hitInfo, bool terrainAttack) {
      DynamicWeaponHitInfo dHitInfo = new DynamicWeaponHitInfo();
      double effectiveAmmo = 0.0;
      double ammoPerHit = 1.0 / (double)weapon.ShotsPerAmmo() / (double)weapon.ShotsToHits(1);
      //double epsilon = 0.001;
      bool ammoDepleded = false;
      bool streak = weapon.isStreak();
      for(int hitIndex = 0; hitIndex < hitInfo.numberOfShots; ++hitIndex) {
        if ((streak)&&(terrainAttack == false)&&((hitInfo.hitLocations[hitIndex] == 0)||(hitInfo.hitLocations[hitIndex] == 65536))) { continue; }
        effectiveAmmo += ammoPerHit;
        while(effectiveAmmo >= 1.0) {
          if (weapon.DecrementOneAmmo()) {
            effectiveAmmo -= 1.0;
          } else {
            ammoDepleded = true;
            break;
          }
        }
        if (ammoDepleded) { break; }
        dHitInfo.Add(ref hitInfo,hitIndex);
      }
      if((ammoDepleded == false)&&(effectiveAmmo > 0.1)) {
        weapon.DecrementOneAmmo();
      }
      dHitInfo.ToHitInfo(ref hitInfo);
    }
    public static void FlushAmmoCount(this Weapon weapon, int stackItemUID) {
      weapon.SetInternalAmmo(stackItemUID,weapon.tInternalAmmo());
      foreach(AmmunitionBox box in weapon.ammoBoxes) {
        if(box.CurrentAmmo != box.tCurrentAmmo()) {
          box.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, box.tCurrentAmmo(), -1, true);
          CustomAmmoCategories.AddToExposionCheck(box);
        }
      }
      sortedAmmoBoxes.Remove(weapon);
    }
    public static AmmunitionBox currentAmmoBox(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return null; }
      if (weapon.InternalAmmo > 0) { return null; }
      if (cachedAmmoBoxes.TryGetValue(weapon, out AmmunitionBox cBox)) {
        if ((cBox.IsFunctional) && (cBox.CurrentAmmo > 0) && (cBox.ammoDef.Description.Id == ammo.Id)) {
          return cBox;
        } else {
          cachedAmmoBoxes.Remove(weapon);
        }
      }
      if (sortedAmmoBoxes.TryGetValue(weapon, out List<AmmunitionBox> boxes) == false) {
        weapon.FillAmmoBoxesCache();
        if (sortedAmmoBoxes.TryGetValue(weapon, out boxes) == false) {
          boxes = weapon.ammoBoxes;
        }
      }
      for (int i = 0; i < boxes.Count; ++i) {
        AmmunitionBox box = boxes[i];
        if (box.IsFunctional == false) { continue; }
        if (box.CurrentAmmo < 1) { continue; }
        if (box.ammoDef.Description.Id != ammo.Id) { continue; }
        if (box.CurrentAmmo > 1) {
          if (cachedAmmoBoxes.ContainsKey(weapon) == false) {
            cachedAmmoBoxes.Add(weapon, box);
          } else {
            cachedAmmoBoxes[weapon] = box;
          }
        }
        return box;
      }
      return null;
    }
    public static List<AmmunitionBox> SortedAmmoBoxes(this Weapon weapon) {
      if (sortedAmmoBoxes.TryGetValue(weapon, out List<AmmunitionBox> boxes) == false) {
        weapon.FillAmmoBoxesCache();
        if (sortedAmmoBoxes.TryGetValue(weapon, out boxes) == false) {
          boxes = weapon.ammoBoxes;
        }
      }
      return boxes;
    }
    public static bool DecrementOneAmmo(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      Log.M.TWL(0, "Weapon.DecrementOneAmmo "+weapon.defId+" ammo:"+(ammo == null?"null":ammo.Id+" category:"+(ammo.AmmoCategory==null?"null":ammo.AmmoCategory.Id)));
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return true; }
      if (assaultTurretsWeapon.Contains(weapon)) { return true; }
      if (weapon.parent != null) {
        Turret turret = weapon.parent as Turret;
        if (turret != null) {
          if (turret.TurretDef.Description.Id.Contains("Assault")) {
            assaultTurretsWeapon.Add(weapon);
            return true;
          }
        }
      }
      if (weapon.tInternalAmmo() > 0) { weapon.tInternalAmmo(weapon.tInternalAmmo() - 1); return true; }
      if(cachedAmmoBoxes.TryGetValue(weapon,out AmmunitionBox cBox)) {
        if ((cBox.IsFunctional)&&(cBox.tCurrentAmmo() > 0)&&(cBox.ammoDef.Description.Id == ammo.Id)) {
          cBox.tCurrentAmmo(cBox.tCurrentAmmo() - 1);
          if (cBox.tCurrentAmmo() == 0) { cachedAmmoBoxes.Remove(weapon); }
          return true;
        } else {
          cachedAmmoBoxes.Remove(weapon);
        }
      }
      if (sortedAmmoBoxes.TryGetValue(weapon, out List<AmmunitionBox> boxes) == false) {
        weapon.FillAmmoBoxesCache();
        if (sortedAmmoBoxes.TryGetValue(weapon, out boxes) == false) {
          boxes = weapon.ammoBoxes;
        }
      }
      for (int i = 0; i < boxes.Count; ++i) {
        AmmunitionBox box = boxes[i];
        if (box.IsFunctional == false) { continue; }
        if (box.tCurrentAmmo() < 1) { continue; }
        if (box.ammoDef.Description.Id != ammo.Id) { continue; }
        if (box.tCurrentAmmo() > 1) {
          if (cachedAmmoBoxes.ContainsKey(weapon) == false) {
            cachedAmmoBoxes.Add(weapon, box);
          } else {
            cachedAmmoBoxes[weapon] = box;
          }
        }
        box.tCurrentAmmo(box.tCurrentAmmo() - 1);
        return true;
      }
      return false;
    }
    public static int tInternalAmmo(this Weapon weapon) {
      if (tempWeaponAmmoCount.TryGetValue(weapon, out Dictionary<string,int> iammos) == false) {
        iammos = new Dictionary<string, int>();
        tempWeaponAmmoCount.Add(weapon,iammos);
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return 0; }
      if (iammos.TryGetValue(ammo.Id, out int count) == false) {
        string statName = Weapon_InternalAmmo.InternalAmmoName + ammo.Id;
        Statistic intAmmo = weapon.StatCollection.GetStatistic(statName);
        if (intAmmo == null) {
          count = 0;
        } else {
          count = intAmmo.Value<int>();
        }
        iammos.Add(ammo.Id, count);
      }
      return count;
    }
    public static int tCurrentAmmo(this AmmunitionBox box) {
      if (tempAmmoBoxAmmoCount.ContainsKey(box) == false) { tempAmmoBoxAmmoCount.Add(box, box.CurrentAmmo); }
      return tempAmmoBoxAmmoCount[box];
    }
    public static int tCurrentAmmo(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return -1; }
      int result = weapon.tInternalAmmo();
      foreach (AmmunitionBox box in weapon.ammoBoxes) {
        if (box.IsFunctional == false) { continue; }
        if (box.ammoDef.Description.Id != ammo.Id) { continue; }
        result += box.tCurrentAmmo();
      }
      return result;
    }
    public static void tInternalAmmo(this Weapon weapon, int count) {
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return; }
      if (tempWeaponAmmoCount.TryGetValue(weapon, out Dictionary<string, int> iammos) == false) {
        iammos = new Dictionary<string, int>();
        tempWeaponAmmoCount.Add(weapon, iammos);
      }
      if (iammos.ContainsKey(ammo.Id) == false) {
        tempWeaponAmmoCount[weapon].Add(ammo.Id, count);
      } else {
        tempWeaponAmmoCount[weapon][ammo.Id] = count;
      }
    }
    public static void initInternalAmmo(this Weapon weapon,string ammoName, int count) {
      if (tempWeaponAmmoCount.ContainsKey(weapon) == false) {
        tempWeaponAmmoCount.Add(weapon, new Dictionary<string, int>());
      }
      if (tempWeaponAmmoCount[weapon].ContainsKey(ammoName) == false) {
        tempWeaponAmmoCount[weapon].Add(ammoName, count);
      } else {
        tempWeaponAmmoCount[weapon][ammoName] = count;
      }
    }
    public static void tCurrentAmmo(this AmmunitionBox box, int count) {
      if (tempAmmoBoxAmmoCount.ContainsKey(box) == false) { tempAmmoBoxAmmoCount.Add(box, box.CurrentAmmo); }
      tempAmmoBoxAmmoCount[box] = count;
    }
    public static void ResetTempAmmo(this Weapon weapon) {
      weapon.tInternalAmmo(weapon.InternalAmmo);
      foreach(AmmunitionBox box in weapon.ammoBoxes) { box.tCurrentAmmo(box.CurrentAmmo); };
    }
    public static int ShotsToHits(this Weapon weapon, int shoots) {
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (weapon.DisabledClustering() == false)) {
        shoots *= weapon.ProjectilesPerShot;
      }
      return shoots;
    }
    public static int HitsToShots(this Weapon weapon, int hits) {
      if (weapon.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (weapon.DisabledClustering() == false)) {
        hits /= weapon.ProjectilesPerShot;
      }
      return hits;
    }
    public static int ShootsToAmmo(this Weapon weapon, int shoots) {
      return Mathf.RoundToInt((float)shoots / weapon.ShotsPerAmmo());
    }
    public static int AmmoToShoots(this Weapon weapon, int ammo) {
      return Mathf.RoundToInt((float)ammo * weapon.ShotsPerAmmo());
    }
  }
  public static partial class CustomAmmoCategories {
    public static bool DisabledClustering(this Weapon weapon) {
      ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
      if (weapon.HasShells() == true) { return true; };
      if (weapon.DamagePerPallet() == true) { return false; }
      return extWeapon.DisableClustering == TripleBoolean.True;
    }
    public static bool isStreak(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.Streak != TripleBoolean.NotSet) { return mode.Streak == TripleBoolean.True; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.Streak != TripleBoolean.NotSet) { return ammo.Streak == TripleBoolean.True; };
      return weapon.exDef().Streak;
    }
    public static void ReturnNoFireHeat(Weapon weapon, int stackItemUID, int numNeedShots, int numSuccesHits) {
      CustomAmmoCategoriesLog.Log.LogWrite("Returining heat\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Needed shots " + numNeedShots + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" Success shots " + numSuccesHits + "\n");
      float returnHeat = 0.0f - ((float)(numNeedShots - numSuccesHits) * weapon.HeatGenerated) / (float)numNeedShots;
      CustomAmmoCategoriesLog.Log.LogWrite(" Heat to return " + (int)returnHeat + "\n");
      weapon.parent.AddWeaponHeat(weapon, (int)returnHeat);
    }
    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("DecrementAmmo")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(int) })]
    public static class Weapon_DecrementAmmo {
      public static bool Prefix(Weapon __instance, int stackItemUID, ref int __result) {
        Log.M.TWL(0, "Weapon.DecrementAmmo:" + __instance.defId);
        //__result = __instance.CountAmmoForShot(stackItemUID);
        //Log.M.W(1, "shots:" + __result);
        //__result = __instance.ShotsToHits(__result);
        //Log.M.WL(1, "hits:" + __result);
        if (__instance.CanFire == false) { __result = 0; return false; }
        __result = __instance.ShotsToHits(__instance.ShotsWhenFired);
        return false;
      }
    }
    public static int CountAmmoForShot(this Weapon weapon, int stackItemUID) {
      Log.M.TWL(0, "Weapon.CountAmmoForShot:" + weapon.defId);
      int ammoWhenFired =  weapon.ShootsToAmmo(weapon.ShotsWhenFired);
      Log.M.WL(1, "ammoWhenFired:"+ammoWhenFired);
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.BaseCategory.Is_NotSet) { return weapon.AmmoToShoots(ammoWhenFired); };
      if (weapon.parent != null) {
        Turret turret = weapon.parent as Turret;
        if (turret != null) {
          if (turret.TurretDef.Description.Id.Contains("Assault")) {
            Log.M.WL(1, "hardened turret detected");
            return weapon.AmmoToShoots(ammoWhenFired); ;
          }
        }
      }
      int modValue = 0;
      if (weapon.tInternalAmmo() >= ammoWhenFired) {
        weapon.tInternalAmmo(weapon.tInternalAmmo() - ammoWhenFired);
        return weapon.AmmoToShoots(ammoWhenFired);
      } else {
        modValue = ammoWhenFired - weapon.tInternalAmmo();
        weapon.tInternalAmmo(0);
      }
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (modValue == 0) { break; }
        AmmunitionBox ammoBox = weapon.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.tCurrentAmmo() <= 0) { continue; }
        if (ammoBox.ammoDef.Description.Id != ammo.Id) { continue; }
        if (ammoBox.tCurrentAmmo() >= modValue) {
          ammoBox.tCurrentAmmo(ammoBox.tCurrentAmmo() - modValue); modValue = 0; break;
        } else {
          modValue -= ammoBox.tCurrentAmmo();
          ammoBox.tCurrentAmmo(0);
        }
      }
      return weapon.AmmoToShoots(ammoWhenFired - modValue);
    }
    public static void RealDecrementAmmo(this Weapon weapon, int stackItemUID, int realShootsUsed) {
      Log.M.TWL(0,"RealDecrementAmmo:" + weapon.defId);
      int ammoWhenFired = weapon.ShootsToAmmo(realShootsUsed);
      WeaponExtendedInfo info = weapon.info();
      CustomAmmoCategory ammoCategory = info.effectiveAmmoCategory;
      if (ammoCategory.BaseCategory.Is_NotSet) { Log.M.WL(1, "not using ammo"); return; };
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AmmoCategory.Id != ammoCategory.Id) {
        ammo = ammoCategory.defaultAmmo();
      };
      if(ammo.AmmoCategory.BaseCategory.Is_NotSet) { Log.M.WL(1, "very strange behavior this ammo category have no ammo definitions"); return; };
      if (weapon.parent != null) {
        Turret turret = weapon.parent as Turret;
        if (turret != null) {
          if (turret.TurretDef.Description.Id.Contains("Assault")) {
            Log.M.WL(1,"Hardened turret detected"); return;
          }
        }
      }
      int modValue = 0;
      if (weapon.InternalAmmo >= ammoWhenFired) {
        weapon.DecInternalAmmo(stackItemUID, ammoWhenFired);
        //weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Subtract, ammoWhenFired, -1, true);
        weapon.tInternalAmmo(weapon.InternalAmmo);
        Log.M.WL(1, "new internal ammo:"+ weapon.InternalAmmo);
        return;
      } else {
        weapon.ZeroInternalAmmo(stackItemUID);
        //weapon.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, 0, -1, true);
        weapon.tInternalAmmo(0);
        modValue = ammoWhenFired - weapon.InternalAmmo;
      }
      for (int index = 0; index < weapon.ammoBoxes.Count; ++index) {
        if (modValue == 0) { break; }
        AmmunitionBox ammoBox = weapon.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.CurrentAmmo <= 0) { continue; }
        if (ammoBox.ammoDef.Description.Id != ammo.Id) { continue; }
        if (ammoBox.CurrentAmmo >= modValue) {
          ammoBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Subtract, modValue, -1, true);
          ammoBox.tCurrentAmmo(ammoBox.CurrentAmmo);
          modValue = 0;
          Log.M.WL(1, "new current ammo in "+ammoBox.defId+":" + ammoBox.CurrentAmmo);
          CustomAmmoCategories.AddToExposionCheck(ammoBox);
          break;
        } else {
          modValue -= ammoBox.CurrentAmmo;
          ammoBox.StatCollection.ModifyStat<int>(weapon.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, 0, -1, true);
          Log.M.WL(1, "new current ammo in " + ammoBox.defId + ":" + ammoBox.CurrentAmmo);
          ammoBox.tCurrentAmmo(0);
          CustomAmmoCategories.AddToExposionCheck(ammoBox);
        }
      }
      return;
    }
    /*public static int DecrementAmmo(this Weapon instance, int stackItemUID, int StreakHitCount, bool forceStreak = false) {
      int shotsWhenFired = instance.ShotsWhenFired;
      if (StreakHitCount != 0) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || (CustomAmmoCategories.getWeaponDisabledClustering(instance) == false)) {
          shotsWhenFired = StreakHitCount / instance.ProjectilesPerShot;
        } else {
          shotsWhenFired = StreakHitCount;
        }
      }
      bool streakEffect = instance.isStreak();
      if (forceStreak) { streakEffect = true; };
      if ((streakEffect == false) && (StreakHitCount == 0)) {
        StreakHitCount = shotsWhenFired;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Weapon.DecrementAmmo " + instance.UIName + " real fire count:" + StreakHitCount + "\n");
      int result = 0;
      bool noAmmoUsing = false;
      if (instance.CustomAmmoCategory().Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) { noAmmoUsing = true; };
      if (instance.parent != null) {
        if (instance.parent is Turret) {
          if (instance.parent.DisplayName.Contains("Hardened")) { //TODO: Check localization
            Log.LogWrite(" Hardened turret detected\n");
          } else {
            noAmmoUsing = true;
          }
        }
      }
      if (noAmmoUsing) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
          result = shotsWhenFired;
        } else {
          result = shotsWhenFired * instance.ProjectilesPerShot;
        }
        Log.LogWrite("  weapon has no ammo (energy or turret) " + instance.UIName + "\n");
        return result;
      }
      int modValue;
      if (instance.InternalAmmo >= shotsWhenFired) {
        if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Int_Subtract, shotsWhenFired, -1, true);
        modValue = 0;
      } else {
        modValue = shotsWhenFired - instance.InternalAmmo;
        if (StreakHitCount != 0) instance.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "InternalAmmo", StatCollection.StatOperation.Set, 0, -1, true);
      }
      string CurrentAmmoId = "";
      if (instance.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == true) {
        CurrentAmmoId = instance.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      } else {
        if (instance.ammoBoxes.Count > 0) {
          CurrentAmmoId = instance.ammoBoxes[0].ammoDef.Description.Id;
          CustomAmmoCategoriesLog.Log.LogWrite($"WARNING! strange behavior " + instance.UIName + " has no data in statistics. fallback to default ammo " + CurrentAmmoId + "\n");
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite($"WARNING! strange behavior " + instance.UIName + " not energy, parent no turret but no ammo boxes\n");
          if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
            result = shotsWhenFired;
          } else {
            result = shotsWhenFired * instance.ProjectilesPerShot;
          }
          return result;
        }
      }
      //ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
      //ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(__instance.weaponDef.Description.Id);
      if (modValue == 0) {
        if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
          result = shotsWhenFired;
        } else {
          result = shotsWhenFired * instance.ProjectilesPerShot;
        }
        CustomAmmoCategoriesLog.Log.LogWrite("  fire internal ammo. projectiles:" + result + "\n");
        return result;
      }
      for (int index = 0; index < instance.ammoBoxes.Count; ++index) {
        AmmunitionBox ammoBox = instance.ammoBoxes[index];
        if (ammoBox.IsFunctional == false) { continue; }
        if (ammoBox.CurrentAmmo <= 0) { continue; }
        if ((string.IsNullOrEmpty(CurrentAmmoId) == false) && (ammoBox.ammoDef.Description.Id != CurrentAmmoId)) { continue; }
        if (ammoBox.CurrentAmmo >= modValue) {
          if (StreakHitCount != 0) {
            ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Int_Subtract, modValue, -1, true);
            CustomAmmoCategories.AddToExposionCheck(ammoBox);
          }
          modValue = 0;
        } else {
          modValue -= ammoBox.CurrentAmmo;
          if (StreakHitCount != 0) {
            ammoBox.StatCollection.ModifyStat<int>(instance.uid, stackItemUID, "CurrentAmmo", StatCollection.StatOperation.Set, 0, -1, true);
            CustomAmmoCategories.AddToExposionCheck(ammoBox);
          }
        }
      }
      if (instance.weaponDef.ComponentTags.Contains("wr-clustered_shots") || CustomAmmoCategories.getWeaponDisabledClustering(instance)) {
        result = (instance.ShotsWhenFired - modValue);
      } else {
        result = (instance.ShotsWhenFired - modValue) * instance.ProjectilesPerShot;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("  fire external ammo. projectiles:" + result + "\n");
      return result;
    }*/
  }
}
