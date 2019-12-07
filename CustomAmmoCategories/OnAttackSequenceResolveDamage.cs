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
using CustomAmmoCategoriesLog;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static void ResolveAoETargetHeat(AbstractActor attacker, int sequenceId, ICombatant combatant) {
      Mech target = combatant as Mech;
      if (target != null) {
        int tempHeat = target.TempHeat;
        CustomAmmoCategoriesLog.Log.LogWrite("  publish:" + target.DisplayName + " heat:" + tempHeat + "\n");
        if (tempHeat > 0) {
          attacker.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(attacker.GUID, target.GUID, new Text("__/CAC.HEAT/__", new object[1]
          {
              (object) tempHeat
          }), FloatieMessage.MessageNature.Debuff));
          if (attacker.GUID != combatant.GUID) {
            CustomAmmoCategoriesLog.Log.LogWrite("  damaging other target\n");
            target.GenerateAndPublishHeatSequence(sequenceId, false, false, attacker.GUID);
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite("  damaging self. so we need apply heatsinks.\n");
            target.GenerateAndPublishHeatSequence(sequenceId, true, false, attacker.GUID);
          }
        }
      }
    }
    public static bool StatusEffectsPerHit(this Weapon weapon) {
      return weapon.exDef().StatusEffectsPerHit == TripleBoolean.True;
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(WeaponHitInfo))]
  [HarmonyPatch("ConsolidateInstability")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(float), typeof(float), typeof(float), typeof(float) })]
  public static class WeaponHitInfo_ConsolidateInstability {
    public static void Postfix(ref WeaponHitInfo __instance, string targetGUID, float instability, float glancingMultiplier, float normalMultiplier, float solidMultiplier, ref float __result) {
      //Log.LogWrite("WeaponHitInfo.ConsolidateInstability\n");
      //__result = 0f;
    }
  }
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceResolveDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceResolveDamage {
    public static bool Prefix(AttackDirector.AttackSequence __instance, ref MessageCenterMessage message) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.AttackSequence.OnAttackSequenceResolveDamage\n");
      try {
        CharlesB.AttackDirector__AttackSequence_OnAttackSequenceResolveDamage_Patch.Prefix(ref message, __instance);
      } catch(Exception e) {
        Log.CB.TWL(0, e.ToString(),true);
      }
      AttackSequenceResolveDamageMessage resolveDamageMessage = (AttackSequenceResolveDamageMessage)message;
      WeaponHitInfo hitInfo = resolveDamageMessage.hitInfo;
      if (hitInfo.attackSequenceId != __instance.id) { return true; };
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { return true; }
      try {
        if (!__instance.messageCoordinator().CanProcessMessage(resolveDamageMessage)) {
          __instance.messageCoordinator().StoreMessage((MessageCenterMessage)resolveDamageMessage);
        } else {
          ResolveDamageHelper.ResolveWeaponDamageAdv(ref hitInfo);
          advInfo.Sequence.messageCoordinator().MessageComplete((MessageCenterMessage)resolveDamageMessage);
        }
      } catch (Exception e) {
        Log.LogWrite("Resolve weapon damage fail." + e.ToString() + "\nFallback\n");
        return true;
      }
      return false;
    }
  }
}
