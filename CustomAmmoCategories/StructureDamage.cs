using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitStatsAP {
    public static void Postfix(AbstractActor __instance) {
      Log.LogWrite("AbstractActor.InitEffectStats " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      __instance.isAPProtected(false);
    }
  }
  public static class StructureDamageHelper {
    public static readonly string APProtectionStatisticName = "CACAPProtection";
    public static bool isAPProtected(this ICombatant unit) {
      if (CustomAmmoCategories.checkExistance(unit.StatCollection, APProtectionStatisticName) == false) { return false; }
      return unit.StatCollection.GetStatistic(APProtectionStatisticName).Value<bool>();
    }
    public static void isAPProtected(this ICombatant unit,bool val) {
      if (CustomAmmoCategories.checkExistance(unit.StatCollection, APProtectionStatisticName) == false) {
        unit.StatCollection.AddStatistic<bool>(APProtectionStatisticName, val);
      } else {
        unit.StatCollection.Set<bool>(APProtectionStatisticName, val);
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
      float num1 = totalDamage;
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
        float damage = Mathf.Min(num1, currentStructure);
        mech.ApplyStructureStatDamage(fromArmorLocation, damage, hitInfo);
        num1 -= damage;
        if (mech.IsLocationDestroyed(fromArmorLocation) && (double)damage > 0.00999999977648258)
          mech.NukeStructureLocation(hitInfo, originalHitLoc, fromArmorLocation, attackDirection, damageType);
      } else if (mech.IsDead && (double)num1 > 0.0) {
        mech.ShowFloatie(hitInfo.attackerId, MechStructureRules.GetArmorFromChassisLocation(fromArmorLocation), FloatieMessage.MessageNature.StructureDamage, string.Format("{0}", (object)(int)Mathf.Max(1f, num1)), mech.Combat.Constants.CombatUIConstants.floatieSizeMedium);
      }
      mech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.attackerId, mech.GUID, totalDamage, aLoc));
      if ((double)num1 <= 0.0) { return true; }
      ArmorLocation passthroughLocation = MechStructureRules.GetPassthroughLocation(aLoc, hitInfo.attackDirections[hitIndex]);
      return mech.DamageLocationStructure(originalHitLoc, hitInfo, passthroughLocation, weapon, num1, hitIndex, impactQuality, damageType);
    }
  }
}