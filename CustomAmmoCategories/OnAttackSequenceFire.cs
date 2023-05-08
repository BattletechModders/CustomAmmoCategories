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
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("OnAttackSequenceFire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackDirector_OnAttackSequenceFire {
    public static void Prefix(ref bool __runOriginal, AttackDirector.AttackSequence __instance,MessageCenterMessage message) {
      if (!__runOriginal) { return; }
      Log.Combat?.TWL(0, "AttackDirector.OnAttackSequenceFire");
      //Log.LogWrite("WeaponHitInfo.ConsolidateInstability\n");
      //__result = 0f;
      try {
        AttackSequenceFireMessage sequenceFireMessage = (AttackSequenceFireMessage)message;
        if (sequenceFireMessage.sequenceId != __instance.id) { __runOriginal = false;  return; }
        int groupIdx = sequenceFireMessage.groupIdx;
        int weaponIdx = sequenceFireMessage.weaponIdx;
        Weapon weapon = __instance.sortedWeapons[groupIdx][weaponIdx];
        if (AttackDirector.attackLogger.IsDebugEnabled)
          AttackDirector.attackLogger.LogDebug(("MeleeType = " + __instance.meleeAttackType.ToString()));
        if (AttackDirector.AttackSequence.logger.IsLogEnabled)
          AttackDirector.AttackSequence.logger.Log(string.Format("[OnAttackSequenceFire] ID {0}, Group {1}, Weapon {2}, Weapon Name {3}", __instance.id, groupIdx, weaponIdx, weapon.Name));
        if (AttackDirector.hitminLogger.IsLogEnabled)
          AttackDirector.hitminLogger.Log(string.Format("============================================================= STARTING NEW ATTACK SEQUENCE FOR {0} (ID {1}):", weapon.Name, weaponIdx));
        int numberOfShots = __instance.numberOfShots[groupIdx][weaponIdx];
        if (numberOfShots == -1) {
          if (weapon.HasPreFired)
            AttackDirector.AttackSequence.logger.LogError(string.Format("[OnAttackSequenceFire] Weapon {0} should not have prefired if it wasn't going to fire!", weapon.Name));
          AttackDirector.AttackSequence.logger.LogWarning(string.Format("Weapon {0} can't fire, skipping", weapon.Description.Name));
          __instance.Director.Combat.MessageCenter.PublishMessage(new AttackSequenceWeaponPreFireCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
          __instance.Director.Combat.MessageCenter.PublishMessage(new AttackSequenceWeaponCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
        } else {
          weapon.FireWeapon();
          if (numberOfShots == 0) {
            AttackDirector.AttackSequence.logger.LogError(string.Format("[OnAttackSequenceFire] Weapon {0} tried to fire 0 shots (this should not happen!), skipping", weapon.Description.Name));
            __instance.Director.Combat.MessageCenter.PublishMessage(new AttackSequenceWeaponPreFireCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
            __instance.Director.Combat.MessageCenter.PublishMessage(new AttackSequenceWeaponCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
          } else {
            WeaponHitInfo? nullable = __instance.weaponHitInfo[groupIdx][weaponIdx];
            WeaponHitInfo hitInfo;
            if (nullable.HasValue) {
              hitInfo = nullable.Value;
              __instance.AddAllAffectedTargets(hitInfo);
            } else {
              AttackDirector.AttackSequence.logger.LogError("[OnAttackSequenceFire] had to generate hit info because pre-calculated hit info was not available!");
              hitInfo = __instance.GenerateHitInfo(weapon, groupIdx, weaponIdx, numberOfShots, __instance.indirectFire, 0.0f);
              __instance.AddAllAffectedTargets(hitInfo);
            }
            weapon.CompleteFiring();
            foreach (EffectData statusEffect in weapon.weaponDef.statusEffects) {
              if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnActivation) {
                string effectID = string.Format("{0}Effect_{1}_{2}", statusEffect.targetingData.effectTriggerType.ToString(), weapon.parent.GUID, hitInfo.attackSequenceId);
                foreach (ICombatant target in __instance.Director.Combat.EffectManager.GetTargetCombatantForEffect(statusEffect, weapon.parent, __instance.chosenTarget)) {
                  try {
                    __instance.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, __instance.stackItemUID, weapon.parent, target, hitInfo, weaponIdx, false);
                  }catch(Exception e) {
                    Log.Combat?.TWL(0, e.ToString(),true);
                    AttackDirector.AttackSequence.logger.LogException(e);
                  }
                  if (!statusEffect.targetingData.hideApplicationFloatie)
                    __instance.Director.Combat.MessageCenter.PublishMessage(new FloatieMessage(weapon.parent.GUID, weapon.parent.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Buff));
                  if (!statusEffect.targetingData.hideApplicationFloatie)
                    __instance.Director.Combat.MessageCenter.PublishMessage(new FloatieMessage(weapon.parent.GUID, target.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Buff));
                }
              }
            }
            bool flag = weapon.weaponRep != null && weapon.weaponRep.HasWeaponEffect;
            if (DebugBridge.TestToolsEnabled)
              flag = flag && !DebugBridge.DisableWeaponEffectDrivenAttacks;
            if (flag) {
              weapon.weaponRep.PlayWeaponEffect(hitInfo);
            } else {
              Log.Combat?.TWL(0,$"Exception unit:{weapon.parent.PilotableActorDef.ChassisID} weapon:{weapon.weaponDef.Description.Id} weaponRep:"+(weapon.weaponRep==null?"null": (weapon.weaponRep.name+ " HasWeaponEffect:"+weapon.weaponRep.HasWeaponEffect)));
            }
          }
        }
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        AttackDirector.AttackSequence.logger.LogException(e);
        return;
      }
    }
  }
}