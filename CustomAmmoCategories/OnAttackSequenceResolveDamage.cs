using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using Localize;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static void ResolveAoETargetHeat(AbstractActor attacker,int sequenceId,ICombatant combatant) {
      Mech target = combatant as Mech;
      if (target != null) {
        int tempHeat = target.TempHeat;
        CustomAmmoCategoriesLog.Log.LogWrite("  publish:" + target.DisplayName + " heat:"+tempHeat+"\n");
        if (tempHeat > 0) {
          attacker.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(attacker.GUID, target.GUID, new Text("+{0} HEAT", new object[1]
          {
              (object) tempHeat
          }), FloatieMessage.MessageNature.Debuff));
          target.GenerateAndPublishHeatSequence(sequenceId, false, false, attacker.GUID);
        }
      }
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("ResolveSequenceTookDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(AttackDirector.AttackSequence) })]
  public static class AttackDirector_ResolveSequenceTookDamage {

    public static void Postfix(AttackDirector __instance, string VOQueueId, AttackDirector.AttackSequence sequence) {
      WeaponHitInfo?[][] weaponHitInfo = (WeaponHitInfo?[][])typeof(AttackDirector.AttackSequence).GetField("weaponHitInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequence);
      HashSet<string> targetsGUIDs = new HashSet<string>();
      CustomAmmoCategoriesLog.Log.LogWrite("ResolveSequenceTookDamage. Main target: "+ sequence .target.GUID+ " "+sequence.target.DisplayName+"\n");
      if(sequence.target is Mech) {
        CustomAmmoCategoriesLog.Log.LogWrite(" main target resolve heat: " + (sequence.target as Mech).TempHeat+"\n");
      }
      for (int index1 = 0; index1 < weaponHitInfo.Length; ++index1) {
        WeaponHitInfo?[] nullableArray = weaponHitInfo[index1];
        for (int index2 = 0; index2 < nullableArray.Length; ++index2) {
          WeaponHitInfo? nullable = nullableArray[index2];
          if (nullable.HasValue) {
            List<SpreadHitInfo> spreadHitList = CustomAmmoCategories.getSpreadCache(nullable.Value);
            if (spreadHitList.Count > 1) {
              foreach (SpreadHitInfo spreadHitRecord in spreadHitList) {
                if (spreadHitRecord.targetGUID == sequence.target.GUID) { continue; }
                if (targetsGUIDs.Contains(spreadHitRecord.targetGUID) == false) { targetsGUIDs.Add(spreadHitRecord.targetGUID); };
              }
            }
            List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(nullable.Value, nullable.Value.numberOfShots - 1);
            if (AOEHitsInfo != null) {
              foreach (AOEHitInfo AOEHInfo in AOEHitsInfo) {
                if (AOEHInfo.targetGUID == sequence.target.GUID) { continue; }
                if (targetsGUIDs.Contains(AOEHInfo.targetGUID) == false) { targetsGUIDs.Add(AOEHInfo.targetGUID); };
              }
            }
          }
        }
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" additional targets:"+targetsGUIDs.Count+"\n");
      foreach (string targetGUID in targetsGUIDs) {
        CustomAmmoCategoriesLog.Log.LogWrite("  "+ targetGUID + "\n");
        ICombatant combatant = __instance.Combat.FindCombatantByGUID(targetGUID);
        if(combatant != null) {
          CustomAmmoCategories.ResolveAoETargetHeat(sequence.attacker,sequence.id,combatant);
        }
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ResolveWeaponDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(Weapon), typeof(MeleeAttackType) })]
  public static class Mech_ResolveWeaponDamage {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      var targetPropertyGetter = AccessTools.Property(typeof(Weapon), "HeatDamagePerShot").GetGetMethod();
      var replacementMethod = AccessTools.Method(typeof(Mech_ResolveWeaponDamage), nameof(HeatDamagePerShot));
      return Transpilers.MethodReplacer(instructions, targetPropertyGetter, replacementMethod);
    }

    private static float HeatDamagePerShot(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("get ResolveWeaponDamage HeatDamagePerShot\n");
      //if (CustomAmmoCategories.isWeaponAOECapable(weapon)) {
      //  CustomAmmoCategoriesLog.Log.LogWrite(": AoE\n");
        return 0f;
      //} else {
      //  CustomAmmoCategoriesLog.Log.LogWrite(": normal\n");
      //  return weapon.HeatDamagePerShot;
      //}
    }

    public static bool isAOEHitInfo(WeaponHitInfo hitInfo) {
      for(int hitIndex =0;hitIndex < hitInfo.dodgeRolls.Length; ++hitIndex) {
        if (hitInfo.dodgeRolls[hitIndex] != -10f) { return false; };
      }
      return true;
    }

    public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, MeleeAttackType meleeAttackType) {
      CustomAmmoCategoriesLog.Log.LogWrite("get ResolveWeaponDamage Heat" + __instance.GUID + " " + __instance.DisplayName + "\n");
      AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
      if (isAOEHitInfo(hitInfo) == false) {
        bool heatIsGenerated = false;
        CustomAmmoCategoriesLog.Log.LogWrite("  normal to heat damage\n");
        for (int index = 0; index < hitInfo.numberOfShots; ++index) {
          if (hitInfo.hitLocations[index] != 0 && hitInfo.hitLocations[index] != 65536) {
            int extHeat = (int)weapon.HeatDamagePerShotAdjusted(hitInfo.hitQualities[index]);
            if (extHeat > 0) { heatIsGenerated = true; };
            CustomAmmoCategoriesLog.Log.LogWrite("  adding external normal heat:" + extHeat + "\n");
            __instance.AddExternalHeat(string.Format("Heat Damage from {0}", (object)weapon.Description.Name), (int)extHeat);
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" traget result heat: " + __instance.TempHeat + "\n");
        }
        if (attackSequence != null) {
          if (heatIsGenerated) { attackSequence.FlagAttackDidHeatDamage(); } ;
        }
      } else {
        List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(hitInfo, __instance.GUID);
        if (AOEHitsInfo != null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" Resolve Damage AOE Hit info found\n");
          foreach (AOEHitInfo AOEHInfo in AOEHitsInfo) {
            if (AOEHInfo.targetGUID != __instance.GUID) { continue; }
            CustomAmmoCategoriesLog.Log.LogWrite(" Self AoE heat record found:" + AOEHInfo.heatDamage + "\n");
            if (AOEHInfo.heatDamage > CustomAmmoCategories.Epsilon) {
              __instance.AddExternalHeat(string.Format("Heat Damage from {0}", (object)weapon.Description.Name), (int)AOEHInfo.heatDamage);
              CustomAmmoCategoriesLog.Log.LogWrite(" Target:" + __instance.GUID + " " + __instance.DisplayName + " result heat:" + __instance.TempHeat + "\n");
              if (attackSequence != null) {
                attackSequence.FlagAttackDidHeatDamage();
              }
            }
            break;
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("  Resolve Damage AOE Hit info not found\n");
        }
      }
    }
  }
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceResolveDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceResolveDamage {
    public static void ResolveDamage(ICombatant targetCombatant, WeaponHitInfo hitInfo, AttackDirector.AttackSequence instance, AttackSequenceResolveDamageMessage resolveDamageMessage, Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite(" resolving damage for target " + targetCombatant.DisplayName + " " + targetCombatant.DisplayName + "\n");
      targetCombatant.ResolveWeaponDamage(hitInfo);
      AbstractActor target = targetCombatant as AbstractActor;
      int attackIndex = -1;
      int num1 = 0;
      int num2 = 65536;
      for (int index = 0; index < hitInfo.numberOfShots; ++index) {
        int hitLocation = hitInfo.hitLocations[index];
        if (hitLocation != num1 && hitLocation != num2 && attackIndex == -1)
          attackIndex = index;
      }
      if (attackIndex > -1) {
        //typeof(AttackDirector.AttackSequence).GetProperty("attackCompletelyMissed", BindingFlags.NonPublic).SetValue(__instance, (object)false);  
        PropertyInfo property = typeof(AttackDirector.AttackSequence).GetProperty("attackCompletelyMissed");
        property.DeclaringType.GetProperty("attackCompletelyMissed");
        property.GetSetMethod(true).Invoke(instance, new object[1] { (object)false });
        //__instance.attackCompletelyMissed = false;
      }
      if (attackIndex > -1 && !targetCombatant.IsDead && target != null) {
        foreach (EffectData statusEffect in CustomAmmoCategories.getWeaponStatusEffects(weapon)) {
          if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnHit) {
            string effectID = string.Format("OnHitEffect_{0}_{1}", (object)instance.attacker.GUID, (object)hitInfo.attackSequenceId);
            if (statusEffect.Description == null || statusEffect.Description.Id == null || statusEffect.Description.Name == null) {
              CustomAmmoCategoriesLog.Log.LogWrite($"WARNING: EffectID:{effectID} has broken effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}! SKIPPING\n", true);
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite($"Applying effectID:{effectID} with effectDescId:{statusEffect?.Description.Id} effectDescName:{statusEffect?.Description.Name}\n");
              instance.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, instance.stackItemUID, (ICombatant)instance.attacker, targetCombatant, hitInfo, attackIndex, false);
              if (targetCombatant != null) {
                instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(targetCombatant.GUID, targetCombatant.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
              }
            }
          }
        }
        if (target != null) {
          List<EffectData> effectsForTriggerType = target.GetComponentStatusEffectsForTriggerType(EffectTriggerType.OnDamaged);
          for (int index = 0; index < effectsForTriggerType.Count; ++index) {
            instance.Director.Combat.EffectManager.CreateEffect(effectsForTriggerType[index], string.Format("OnDamagedEffect_{0}_{1}", (object)target.GUID, (object)hitInfo.attackSequenceId), instance.stackItemUID, targetCombatant, (ICombatant)instance.attacker, hitInfo, attackIndex, false);
          }
        }
      }
    }
    public static bool Prefix(AttackDirector.AttackSequence __instance, MessageCenterMessage message) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.AttackSequence.OnAttackSequenceResolveDamage");
      try {
        AttackSequenceResolveDamageMessage resolveDamageMessage = (AttackSequenceResolveDamageMessage)message;
        WeaponHitInfo hitInfo = resolveDamageMessage.hitInfo;
        //resolveDamageMessage.
        if (hitInfo.attackSequenceId != __instance.id) { return true; };
        MessageCoordinator messageCoordinator = (MessageCoordinator)typeof(AttackDirector.AttackSequence).GetField("messageCoordinator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if (!messageCoordinator.CanProcessMessage(resolveDamageMessage)) {
          messageCoordinator.StoreMessage((MessageCenterMessage)resolveDamageMessage);
        } else {
          if (AttackDirector.AttackSequence.logger.IsLogEnabled)
            AttackDirector.AttackSequence.logger.Log((object)string.Format("[OnAttackSequenceResolveDamage]  ID {0}, Group {1}, Weapon {2}, AttackerId [{3}], TargetId [{4}]", (object)__instance.id, (object)hitInfo.attackGroupIndex, (object)hitInfo.attackWeaponIndex, (object)hitInfo.attackerId, (object)hitInfo.targetId));
          Weapon weapon = __instance.GetWeapon(resolveDamageMessage.hitInfo.attackGroupIndex, resolveDamageMessage.hitInfo.attackWeaponIndex);
          if (__instance.meleeAttackType == MeleeAttackType.DFA) {
            float damageAmount = __instance.attacker.StatCollection.GetValue<float>("DFASelfDamage");
            __instance.attacker.TakeWeaponDamage(resolveDamageMessage.hitInfo, 64, weapon, damageAmount, 0, DamageType.DFASelf);
            __instance.attacker.TakeWeaponDamage(resolveDamageMessage.hitInfo, 128, weapon, damageAmount, 0, DamageType.DFASelf);
            if (AttackDirector.damageLogger.IsLogEnabled)
              AttackDirector.damageLogger.Log((object)string.Format("@@@@@@@@ {0} takes {1} damage to its legs from the DFA attack!", (object)__instance.attacker.DisplayName, (object)damageAmount));
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" resolving damage\n");
          List<SpreadHitInfo> spreadHitList = CustomAmmoCategories.getSpreadCache(hitInfo);
          List<AOEHitInfo> AOEHitsInfo = CustomAmmoCategories.getAOEHitInfo(hitInfo, hitInfo.numberOfShots - 1);
          if (spreadHitList.Count > 1) {
            CustomAmmoCategoriesLog.Log.LogWrite(" spreading\n");
            foreach (SpreadHitInfo spreadHitRecord in spreadHitList) {
              ICombatant SpreadTarget = __instance.Director.Combat.FindCombatantByGUID(spreadHitRecord.targetGUID);
              if (SpreadTarget != null) {
                //if (AOEHitsInfo == null) {
                  //CustomAmmoCategoriesLog.Log.LogWrite(" No AOE Hit info found. So normal resolve is needed.\n");
                AttackSequence_OnAttackSequenceResolveDamage.ResolveDamage(SpreadTarget, spreadHitRecord.hitInfo, __instance, resolveDamageMessage, weapon);
                //} else {
                  //CustomAmmoCategoriesLog.Log.LogWrite(" Resolve Damage AOE Hit info found. Normal resolve always missed, so not needed.\n");
                //}
              } else {
                CustomAmmoCategoriesLog.Log.LogWrite("WARNING!Can't find AOE target " + spreadHitRecord.targetGUID + "\n", true);
              }
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" no spreading. Normal resolving damage\n");
            AttackSequence_OnAttackSequenceResolveDamage.ResolveDamage(__instance.target, hitInfo, __instance, resolveDamageMessage, weapon);
          }
          if (AOEHitsInfo != null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" Resolving AoE damage.\n");
            foreach (AOEHitInfo AOEHInfo in AOEHitsInfo) {
              ICombatant AOETarget = __instance.Director.Combat.FindCombatantByGUID(AOEHInfo.targetGUID);
              if (AOETarget != null) {
                AttackSequence_OnAttackSequenceResolveDamage.ResolveDamage(AOETarget, AOEHInfo.hitInfo, __instance, resolveDamageMessage, weapon);
              } else {
                CustomAmmoCategoriesLog.Log.LogWrite("WARNING!Can't find AOE target " + AOEHInfo.targetGUID + "\n", true);
              }
            }
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("  Resolve Damage AOE Hit info not found. So AoE resolve damage not needed.\n");
          }
          __instance.attacker.HandleDeath(__instance.attacker.GUID);
          __instance.attacker.HandleDeath(__instance.attacker.GUID);
          messageCoordinator.MessageComplete((MessageCenterMessage)resolveDamageMessage);
        }
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default");
        return true;
      }
      return false;
    }
  }
}
