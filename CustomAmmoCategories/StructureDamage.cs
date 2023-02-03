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
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  //[HarmonyPatch(typeof(AbstractActor))]
  //[HarmonyPatch("InitEffectStats")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  //public static class AbstractActor_InitStatsAP {
  //  public static void Postfix(AbstractActor __instance) {
  //    Log.LogWrite("AbstractActor.InitEffectStats " + __instance.DisplayName + ":" + __instance.GUID + "\n");
  //    __instance.InitExDamageStats();
  //  }
  //}
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("DamageLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
  public static class Mech_DamageLocation {
    public static bool Prefix(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, ref float totalArmorDamage, ref float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      ArmorLocation oaLoc = (ArmorLocation)originalHitLoc;
      Log.M.TWL(0,"Mech.DamageLocation " + __instance.MechDef.ChassisID + " origHitLoc:" + oaLoc + " dmgLoc:"+aLoc);
      if ((aLoc == ArmorLocation.Invalid) || (aLoc == ArmorLocation.None)) { return true; }
      if(oaLoc != aLoc) {
        Log.M.WL(1,"pass through location detected");
        if (CustomAmmoCategories.Settings.NullifyDestoryedLocationDamage) {
          Log.M.WL(2, "nullify damage");
          totalArmorDamage = 0f;
          directStructureDamage = 0f;
          return false;
        }
        if (CustomAmmoCategories.Settings.DestoryedLocationDamageTransferStructure) {
          Log.M.W(2, "transfer all damage direct to structure a:"+totalArmorDamage+" s:"+directStructureDamage);
          directStructureDamage += totalArmorDamage;
          totalArmorDamage = 0f;
          Log.M.WL(0, "->a:"+totalArmorDamage+" s:"+directStructureDamage);
        }
      }
      return true;
    }
  }
  public static class ExDamageHelper {
    public static readonly string APProtectionStatisticName = "CACAPProtection";
    public static readonly string APShardsStatisticName = "CACAPShardsMult";
    public static readonly string APMaxArmorThiknessStatisticName = "CACAPMaxThiknessMult";
    public static readonly string AoEDamageMultStatisticName = "CACAoEDamageMult";
    public static readonly string APDamageMultStatisticName = "CACAPDamageMult";
    public static readonly string IncomingHeatMultStatisticName = "CACIncomingHeatMult";
    public static readonly string IncomingStabilityMultStatisticName = "CACIncomingStabilityMult";
    public static readonly string CriticalHitChanceReceivedMultiplierStatName = "CriticalHitChanceReceivedMultiplier";
    public static readonly string DamageReductionMultiplierAllStatName = "DamageReductionMultiplierAll";
    public static float DamageReductionMultiplier(this ICombatant unit, string statName, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{statName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{statName}").Value<float>();
    }
    public static float DamageReductionMultiplierAll(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{DamageReductionMultiplierAllStatName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{DamageReductionMultiplierAllStatName}").Value<float>();
    }
    public static float AoEDamageMult(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{AoEDamageMultStatisticName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{AoEDamageMultStatisticName}").Value<float>();
    }
    public static float CriticalHitChanceReceivedMultiplier(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{CriticalHitChanceReceivedMultiplierStatName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{CriticalHitChanceReceivedMultiplierStatName}").Value<float>();
    }
    public static bool isAPProtected(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(APProtectionStatisticName) == false) { return false; }
      return unit.StatCollection.GetStatistic(APProtectionStatisticName).Value<bool>();
    }
    public static bool isAPProtected(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{APProtectionStatisticName}") == false) { return false; }
      return unit.StatCollection.GetStatistic($"{location}.{APProtectionStatisticName}").Value<bool>();
    }
    public static float AoEDamageMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(AoEDamageMultStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(AoEDamageMultStatisticName).Value<float>();
    }
    public static float APDamageMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(APDamageMultStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(APDamageMultStatisticName).Value<float>();
    }
    public static float APDamageMult(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{APDamageMultStatisticName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{APDamageMultStatisticName}").Value<float>();
    }
    public static float IncomingHeatMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(IncomingHeatMultStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(IncomingHeatMultStatisticName).Value<float>();
    }
    public static float IncomingStabilityMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(IncomingStabilityMultStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(IncomingStabilityMultStatisticName).Value<float>();
    }
    public static float APShardsMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(APShardsStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(APShardsStatisticName).Value<float>();
    }
    public static float APShardsMult(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{APShardsStatisticName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{APShardsStatisticName}").Value<float>();
    }
    public static float APMaxArmorThiknessMult(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(APMaxArmorThiknessStatisticName) == false) { return 1f; }
      return unit.StatCollection.GetStatistic(APMaxArmorThiknessStatisticName).Value<float>();
    }
    public static float APMaxArmorThiknessMult(this ICombatant unit, int location) {
      if (unit is Mech) { location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)location); }
      if (unit.StatCollection.ContainsStatistic($"{location}.{APMaxArmorThiknessStatisticName}") == false) { return 1f; }
      return unit.StatCollection.GetStatistic($"{location}.{APMaxArmorThiknessStatisticName}").Value<float>();
    }
    public static void InitExDamageStats(this ICombatant unit) {
      if (unit.StatCollection.ContainsStatistic(APProtectionStatisticName) == false) {
        unit.StatCollection.AddStatistic<bool>(APProtectionStatisticName, false);
      }
      if (unit.StatCollection.ContainsStatistic(APShardsStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(APShardsStatisticName, 1.0f);
      }
      if (unit.StatCollection.ContainsStatistic(APMaxArmorThiknessStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(APMaxArmorThiknessStatisticName, 1.0f);
      }
      if (unit.StatCollection.ContainsStatistic(AoEDamageMultStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(AoEDamageMultStatisticName, 1f);
      }
      if (unit.StatCollection.ContainsStatistic(APDamageMultStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(APDamageMultStatisticName, 1f);
      }
      if (unit.StatCollection.ContainsStatistic(IncomingHeatMultStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(IncomingHeatMultStatisticName, 1f);
      }
      if (unit.StatCollection.ContainsStatistic(IncomingStabilityMultStatisticName) == false) {
        unit.StatCollection.AddStatistic<float>(IncomingStabilityMultStatisticName, 1f);
      }
    }
    public static void ShowFloatie(this Mech mech,string sourceGuid, ArmorLocation location, FloatieMessage.MessageNature nature, string dmgText, float fontSize) {
      typeof(Mech).GetMethod("ShowFloatie", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mech, new object[5]{
        sourceGuid,location,nature,dmgText,fontSize
      });
    }
    public static void applyStructureStatDamage(this Vehicle vehicle,VehicleChassisLocations location, float damage, WeaponHitInfo hitInfo) {
      typeof(Vehicle).GetMethod("applyStructureStatDamage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(vehicle, new object[3]{ location,damage,hitInfo });
    }
    public static void ApplyStructureStatDamage(this Turret turret, BuildingLocation location, float damage, WeaponHitInfo hitInfo) {
      typeof(Turret).GetMethod("ApplyStructureStatDamage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(turret, new object[3] { location, damage, hitInfo });
    }
    public static void CACTakeWeaponDamage(this ICombatant combatant, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float totalDamage, float structureDamage, int hitIndex, DamageType damageType) {
      AttackImpactQuality hitQuality = hitInfo.hitQualities[hitIndex];
      Mech mech = combatant as Mech;
      if(mech != null) {
        ChassisLocations mechLocation = MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)hitLocation);
        if (mech.IsLocationDestroyed(mechLocation)) {
          if (CustomAmmoCategories.Settings.NullifyDestoryedLocationDamage) {
            mech.TakeWeaponDamage(hitInfo, hitLocation, weapon, 0, 0, hitIndex, damageType);
          } else {

          }
        }
      } else {
        combatant.TakeWeaponDamage(hitInfo, hitLocation, weapon, totalDamage, structureDamage, hitIndex, damageType);
      }
    }
    public static void TakeWeaponDamageStructure(this ICombatant combatant,WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
      AttackImpactQuality hitQuality = hitInfo.hitQualities[hitIndex];
      Mech mech = combatant as Mech;
      Vehicle vehicle = combatant as Vehicle;
      Turret turret = combatant as Turret;
      Building building = combatant as Building;
      if ((hitQuality == AttackImpactQuality.Solid) && (mech != null)) {
        mech.ReceivedSolidDamageSinceLastActivation = true;
      }
      if(mech != null) {
        mech.DamageLocationStructure(hitLocation, hitInfo, (ArmorLocation)hitLocation, weapon, damageAmount, hitIndex, hitQuality, damageType);
      }else if(vehicle != null) {
        vehicle.DamageLocationStructure(hitLocation, hitInfo, (VehicleChassisLocations)hitLocation, weapon, damageAmount, hitIndex, hitQuality, damageType);
      }else if(turret != null) { 
        turret.DamageLocationStructure(hitLocation, hitInfo, (BuildingLocation)hitLocation, weapon, damageAmount, hitIndex, hitQuality, damageType);
      } else {
        Log.LogWrite("Combatant "+combatant.DisplayName+":"+combatant.GUID+" can't receive trough armor damage\n",true);
      }
    }
    public static bool DamageLocationStructure(this Turret turret, int originalHitLoc, WeaponHitInfo hitInfo, BuildingLocation bLoc, Weapon weapon, float totalDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      if (bLoc == BuildingLocation.None || bLoc == BuildingLocation.Invalid) { return false; }
      AttackDirector.AttackSequence attackSequence = turret.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) { attackSequence.FlagAttackDidDamage(turret.GUID); };
      float a = totalDamage;
      float currentStructure = turret.GetCurrentStructure(bLoc);
      if ((double)currentStructure > 0.0) {
        float damage = Mathf.Min(a, currentStructure);
        turret.ApplyStructureStatDamage(bLoc, damage, hitInfo);
        float num = a - damage;
        if ((double)turret.GetCurrentStructure(bLoc) <= 0.0) {
          turret.FlagForDeath("Location Destroyed: " + bLoc.ToString(), DeathMethod.VehicleLocationDestroyed, DamageType.Weapon, (int)bLoc, hitInfo.stackItemUID, hitInfo.attackerId, false);
        }
      }
      turret.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.attackerId, turret.GUID, totalDamage, bLoc));
      return true;
    }
    public static bool DamageLocationStructure(this Vehicle vehicle, int originalHitLoc, WeaponHitInfo hitInfo, VehicleChassisLocations vLoc, Weapon weapon, float totalDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      if (vLoc == VehicleChassisLocations.None || vLoc == VehicleChassisLocations.Invalid)
        return false;
      AttackDirector.AttackSequence attackSequence = vehicle.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) { attackSequence.FlagAttackDidDamage(vehicle.GUID); };
      float a = totalDamage;
      float currentStructure = vehicle.GetCurrentStructure(vLoc);
      if ((double)currentStructure > 0.0) {
        float damage = Mathf.Min(a, currentStructure);
        vehicle.applyStructureStatDamage(vLoc, damage, hitInfo);
        float num = a - damage;
        if ((double)vehicle.GetCurrentStructure(vLoc) <= 0.0) {
          vehicle.FlagForDeath("Location Destroyed: " + vLoc.ToString(), DeathMethod.VehicleLocationDestroyed, DamageType.Weapon, (int)vLoc, hitInfo.stackItemUID, hitInfo.attackerId, false);
        }
      }
      vehicle.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.targetId, vehicle.GUID, totalDamage, vLoc));
      return true;
    }
    public static bool DamageLocationStructure(this Mech mech, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
      if (aLoc == ArmorLocation.None || aLoc == ArmorLocation.Invalid) { return false; }
      AttackDirector.AttackSequence attackSequence = mech.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (attackSequence != null) {
        attackSequence.FlagAttackDidDamage(mech.GUID);
        mech.Combat.MultiplayerGameVerification.RecordMechDamage(mech.GUID, originalHitLoc, hitInfo, aLoc, weapon, totalDamage, hitIndex, impactQuality);
      }
      float restDamage = totalDamage;
      ChassisLocations fromArmorLocation = MechStructureRules.GetChassisLocationFromArmorLocation(aLoc);
      Vector3 attackDirection = Vector3.one;
      if ((UnityEngine.Object)mech.GameRep != (UnityEngine.Object)null && (UnityEngine.Object)weapon.weaponRep != (UnityEngine.Object)null) {
        Vector3 position = weapon.weaponRep.vfxTransforms[0].position;
        attackDirection = mech.GameRep.GetVFXTransform((int)fromArmorLocation).position - position;
        attackDirection.Normalize();
        attackDirection.y = 0.5f;
        attackDirection *= totalDamage;
      }
      float currentStructure = mech.GetCurrentStructure(fromArmorLocation);
      if ((double)currentStructure > 0.0) {
        float damage = Mathf.Min(restDamage, currentStructure);
        mech.ApplyStructureStatDamage(fromArmorLocation, damage, hitInfo);
        restDamage -= damage;
        if (mech.IsLocationDestroyed(fromArmorLocation) && (double)damage > 0.00999999977648258) {
          mech.NukeStructureLocation(hitInfo, originalHitLoc, fromArmorLocation, attackDirection, damageType);
        }
      } else if (mech.IsDead && (double)restDamage > 0.0) {
        mech.ShowFloatie(hitInfo.attackerId, MechStructureRules.GetArmorFromChassisLocation(fromArmorLocation), FloatieMessage.MessageNature.StructureDamage, string.Format("{0}", (object)(int)Mathf.Max(1f, restDamage)), mech.Combat.Constants.CombatUIConstants.floatieSizeMedium);
      }
      mech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.attackerId, mech.GUID, totalDamage, aLoc));
      if ((double)restDamage <= 0.0) { return true; }
      ArmorLocation passthroughLocation = MechStructureRules.GetPassthroughLocation(aLoc, hitInfo.attackDirections[hitIndex]);
      return mech.DamageLocationStructure(originalHitLoc, hitInfo, passthroughLocation, weapon, restDamage, hitIndex, impactQuality, damageType);
    }
  }
}