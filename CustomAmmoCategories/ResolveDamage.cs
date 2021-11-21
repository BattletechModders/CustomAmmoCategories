﻿using BattleTech;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  public static class ResolveDamageHelper {
    public static void ResolveWeaponDamageAdv(ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      advInfo.isResolved = true;
      if (advInfo.Sequence.meleeAttackType == MeleeAttackType.DFA) {
        float damageAmount = advInfo.Sequence.attacker.StatCollection.GetValue<float>("DFASelfDamage");
        Mech mech = advInfo.Sequence.attacker as Mech;
        if (mech != null) {
          HashSet<ArmorLocation> DFALocs = mech.GetDFASelfDamageLocations();
          Log.M.TWL(0,"DFA self damage "+mech.DisplayName);
          foreach (ArmorLocation aloc in DFALocs) {
            Log.M.WL(1, aloc.ToString()+":"+ damageAmount);
            advInfo.Sequence.attacker.TakeWeaponDamage(hitInfo, (int)aloc, advInfo.weapon, damageAmount, 0f, 0, DamageType.DFASelf);
          }
        }
        if (AttackDirector.damageLogger.IsLogEnabled)
          AttackDirector.damageLogger.Log((object)string.Format("@@@@@@@@ {0} takes {1} damage to its legs from the DFA attack!", (object)advInfo.Sequence.attacker.DisplayName, (object)damageAmount));
      }
      Log.C.TWL(0, "Resolving weapon's critical damage. Sequence id:"+advInfo.Sequence.id+" weapon:"+advInfo.weapon.UIName);
      foreach (var target in advInfo.resolveInfo) {
        Log.C.WL(1, "target: "+target.Key.DisplayName+":"+target.Key.GUID);
        AbstractActor unit = target.Key as AbstractActor;
        if (unit == null) { continue; };
        foreach(var critInfo in target.Value.Crits) {
          Log.C.WL(2, unit.GetArmorLocationName(critInfo.armorLocation)+" armor-on-hit:"+critInfo.armorOnHit+" structure-on-hit:"+critInfo.structureOnHit);
        }
      }
      foreach (var target in advInfo.resolveInfo) {
        target.Key.ResolveTargetWeaponDamageEffects(ref hitInfo);
        target.Key.ResolveTargetWeaponCrits(ref hitInfo);
        target.Key.ResolveTargetWeaponHeat(ref hitInfo);
        target.Key.ResolveTargetWeaponInstability(ref hitInfo);
      }
      advInfo.Sequence.attacker.ResolveJamming(ref hitInfo);
      advInfo.Sequence.attacker.HandleDeath(advInfo.Sequence.attacker.GUID);
    }
    public static void ResolveTargetWeaponDamageEffects(this ICombatant target, ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      AdvWeaponResolveInfo advRes = advInfo.resolve(target);
      Weapon weapon = advInfo.weapon;
      bool effectPerHit = weapon.StatusEffectsPerHit();
      Log.M.TWL(0, "ResolveTargetWeaponDamageEffects:" + advInfo.weapon.defId+" "+target.DisplayName);
      foreach(string msgtext in advRes.floatieMessages) {
        advInfo.Sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(advInfo.Sequence.attacker.GUID, target.GUID, msgtext, FloatieMessage.MessageNature.Debuff));
      }
      /*if ((advRes.hitLocations.Count > 0) && (!target.IsDead) && (target != null)) {
        EffectData[] effects = weapon.StatusEffects();
        foreach (EffectData statusEffect in effects) {
          if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnHit) {
            string effectID = string.Format("OnHitEffect_{0}_{1}", (object)advInfo.Sequence.attacker.GUID, (object)hitInfo.attackSequenceId);
            if (statusEffect.Description == null || statusEffect.Description.Id == null || statusEffect.Description.Name == null) {
              CustomAmmoCategoriesLog.Log.LogWrite($"WARNING: EffectID:{effectID} has broken effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}! SKIPPING\n", true);
            } else {
              int effectsApply = 0;
              foreach (EffectsLocaltion HitLocation in advRes.hitLocations) {
                float roll = Random.Range(0f, 1f);
                if(roll > HitLocation.effectsMod) {
                  Log.LogWrite($"Roll fail\n");
                  continue;
                }
                ++effectsApply;
                Log.LogWrite($"Applying effectID:{effectID} with effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}\n");
                advInfo.Sequence.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, advInfo.Sequence.stackItemUID, (ICombatant)advInfo.Sequence.attacker, target, hitInfo, HitLocation.location, false);
                if (effectPerHit == false) { break; }
              }
              if (target != null) {
                if (effectsApply > 0) {
                  if (effectPerHit == false) {
                    advInfo.Sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(advInfo.Sequence.attacker.GUID, target.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
                  } else {
                    advInfo.Sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(advInfo.Sequence.attacker.GUID, target.GUID, statusEffect.Description.Name + " x " + effectsApply.ToString(), FloatieMessage.MessageNature.Debuff));
                  }
                }
              }
            }
          }
        }
        AbstractActor targetActor = target as AbstractActor;
        if (targetActor != null) {
          List<EffectData> effectsForTriggerType = targetActor.GetComponentStatusEffectsForTriggerType(EffectTriggerType.OnDamaged);
          for (int index = 0; index < effectsForTriggerType.Count; ++index) {
            targetActor.Combat.EffectManager.CreateEffect(effectsForTriggerType[index], string.Format("OnDamagedEffect_{0}_{1}", (object)target.GUID, (object)hitInfo.attackSequenceId), advInfo.Sequence.stackItemUID, target, (ICombatant)advInfo.Sequence.attacker, hitInfo, advRes.hitLocations[0].location, false);
          }
        }
      }*/
    }
    public static void CheckForCrit(this ICombatant target,  ref WeaponHitInfo hitInfo, AdvCritLocationInfo critInfo, Weapon weapon){
      if (CustomAmmoCategories.Settings.AdvancedCirtProcessing) {
        AbstractActor actor = target as AbstractActor;
        if (actor != null) { AdvancedCriticalProcessor.CheckForCrit(actor, ref hitInfo, critInfo, weapon); };
      } else {
        Mech mech = target as Mech;
        if (mech != null) {
          ChassisLocations fromArmorLocation = MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)critInfo.armorLocation);
          typeof(Mech).GetMethod("CheckForCrit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(mech, new object[3] { hitInfo, fromArmorLocation, weapon });
        }
      }
    }
    public static void ResolveTargetWeaponCrits(this ICombatant target, ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      AdvWeaponResolveInfo advRes = advInfo.resolve(target);
      foreach(AdvCritLocationInfo crit in advRes.Crits) {
        target.CheckForCrit(ref hitInfo, crit, advInfo.weapon);
      }
    }
    public static void ResolveTargetWeaponHeat(this ICombatant target, ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      AdvWeaponResolveInfo advRes = advInfo.resolve(target);
      Mech mech = target as Mech;
      Vehicle vehicle = target as Vehicle;
      Turret turret = target as Turret;
      if (advRes.Heat > CustomAmmoCategories.Epsilon) {
        if ((mech != null)&&(mech.isHasHeat())) {
          mech.AddExternalHeat(string.Format("Heat Damage from {0}", (object)advInfo.weapon.Description.Name), (int)advRes.Heat);
          advInfo.Sequence.FlagAttackDidHeatDamage(target.GUID);
        }
      }
    }
    public static void ResolveTargetWeaponInstability(this ICombatant target, ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      AdvWeaponResolveInfo advRes = advInfo.resolve(target);
      Mech mech = target as Mech;
      Vehicle vehicle = target as Vehicle;
      Turret turret = target as Turret;
      if (advRes.Stability > CustomAmmoCategories.Epsilon) {
        if ((mech != null)&&(mech.isHasStability())) {
          mech.AddAbsoluteInstability(advRes.Stability * target.StatCollection.GetValue<float>("ReceivedInstabilityMultiplier") * mech.EntrenchedMultiplier, StabilityChangeSource.Attack, hitInfo.attackerId);
        }
      }
    }
    public static void ResolveJamming(this ICombatant attacker, ref WeaponHitInfo hitInfo) {
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return; }
      advInfo.weapon.jammWeapon(advInfo.jammInfo);
    }
  }
}