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
      Log.M.TWL(0,"AttackDirector.AttackSequence.OnAttackSequenceResolveDamage "+message.messageIndex+":"+message.MessageType);
      AttackSequenceResolveDamageMessage resolveDamageMessage = (AttackSequenceResolveDamageMessage)message;
      ref WeaponHitInfo hitInfo = ref resolveDamageMessage.hitInfo;
      Log.M.WL(1, "grp:" + hitInfo.attackGroupIndex+ " weapon:"+hitInfo.attackWeaponIndex);
      if (hitInfo.attackSequenceId != __instance.id) { Log.M.WL(1, "sequence not mach:"+ hitInfo.attackSequenceId+" != "+__instance.id+" fallback"); return true; };
      AdvWeaponHitInfo advInfo = hitInfo.advInfo();
      if (advInfo == null) { Log.M.WL(1, "no advanced info. fallback"); return true; }
      Log.M?.WL(1,$"hits:{advInfo.hits.Count}");

      advInfo.resolveDamageMessage = resolveDamageMessage;
      advInfo.setVisualState();
      try {
        bool canProcessMessage = advInfo.advHitMessage == null ? false : advInfo.advHitMessage.CanBeApplied();
        if (!canProcessMessage) {
          Log.M.WL(1, "store message");
          if (advInfo.advHitMessage != null) { advInfo.advHitMessage.TryApplyPending(); };
        } else {
          Log.M.WL(1, "processing message");
          advInfo.advHitMessage.Apply(true);
        }
        AdvWeaponHitInfo.printApplyState(__instance.id);
      } catch (Exception e) {
        Log.M.TWL(0,"Resolve weapon damage fail." + e.ToString() + "\nFallback\n",true);
        return true;
      }
      return false;
    }
  }
}
