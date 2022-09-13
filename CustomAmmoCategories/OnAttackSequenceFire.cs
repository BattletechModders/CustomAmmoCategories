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
using Harmony;
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
    private delegate WeaponHitInfo GenerateHitInfoDelegate(AttackDirector.AttackSequence instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage);
    private static GenerateHitInfoDelegate generateHitInfoInvoker = null;
    public static bool Prepare() {
      var method = typeof(AttackDirector.AttackSequence).GetMethod("GenerateHitInfo", BindingFlags.Instance|BindingFlags.NonPublic);
      var dm = new DynamicMethod("CACGenerateHitInfo", typeof(WeaponHitInfo), new Type[] { typeof(AttackDirector.AttackSequence), typeof(Weapon), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(float) }, typeof(AttackDirector.AttackSequence));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Ldarg_2);
      gen.Emit(OpCodes.Ldarg_3);
      gen.Emit(OpCodes.Ldarg_S, 4);
      gen.Emit(OpCodes.Ldarg_S, 5);
      gen.Emit(OpCodes.Ldarg_S, 6);
      gen.Emit(OpCodes.Call, method);
      gen.Emit(OpCodes.Ret);
      generateHitInfoInvoker = (GenerateHitInfoDelegate)dm.CreateDelegate(typeof(GenerateHitInfoDelegate));
      return true;
    }
    public static WeaponHitInfo GenerateHitInfo(this AttackDirector.AttackSequence instance, Weapon weapon, int groupIdx, int weaponIdx, int numberOfShots, bool indirectFire, float dodgedDamage) {
      return generateHitInfoInvoker(instance,weapon,groupIdx,weaponIdx,numberOfShots, indirectFire, dodgedDamage);
    }
    public static bool Prefix(AttackDirector.AttackSequence __instance,MessageCenterMessage message, List<List<Weapon>> ___sortedWeapons, int[][] ___numberOfShots, WeaponHitInfo?[][] ___weaponHitInfo) {
      Log.M.TWL(0, "AttackDirector.OnAttackSequenceFire");
      //Log.LogWrite("WeaponHitInfo.ConsolidateInstability\n");
      //__result = 0f;
      try {
        AttackSequenceFireMessage sequenceFireMessage = (AttackSequenceFireMessage)message;
        if (sequenceFireMessage.sequenceId != __instance.id) { return false; }
        int groupIdx = sequenceFireMessage.groupIdx;
        int weaponIdx = sequenceFireMessage.weaponIdx;
        Weapon weapon = ___sortedWeapons[groupIdx][weaponIdx];
        if (AttackDirector.attackLogger.IsDebugEnabled)
          AttackDirector.attackLogger.LogDebug((object)("MeleeType = " + __instance.meleeAttackType.ToString()));
        if (AttackDirector.AttackSequence.logger.IsLogEnabled)
          AttackDirector.AttackSequence.logger.Log((object)string.Format("[OnAttackSequenceFire] ID {0}, Group {1}, Weapon {2}, Weapon Name {3}", (object)__instance.id, (object)groupIdx, (object)weaponIdx, (object)weapon.Name));
        if (AttackDirector.hitminLogger.IsLogEnabled)
          AttackDirector.hitminLogger.Log((object)string.Format("============================================================= STARTING NEW ATTACK SEQUENCE FOR {0} (ID {1}):", (object)weapon.Name, (object)weaponIdx));
        int numberOfShots = ___numberOfShots[groupIdx][weaponIdx];
        if (numberOfShots == -1) {
          if (weapon.HasPreFired)
            AttackDirector.AttackSequence.logger.LogError((object)string.Format("[OnAttackSequenceFire] Weapon {0} should not have prefired if it wasn't going to fire!", (object)weapon.Name));
          AttackDirector.AttackSequence.logger.LogWarning((object)string.Format("Weapon {0} can't fire, skipping", (object)weapon.Description.Name));
          __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponPreFireCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
          __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
        } else {
          weapon.FireWeapon();
          if (numberOfShots == 0) {
            AttackDirector.AttackSequence.logger.LogError((object)string.Format("[OnAttackSequenceFire] Weapon {0} tried to fire 0 shots (this should not happen!), skipping", (object)weapon.Description.Name));
            __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponPreFireCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
            __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
          } else {
            WeaponHitInfo? nullable = ___weaponHitInfo[groupIdx][weaponIdx];
            WeaponHitInfo hitInfo;
            if (nullable.HasValue) {
              hitInfo = nullable.Value;
              __instance.AddAllAffectedTargets(hitInfo);
            } else {
              AttackDirector.AttackSequence.logger.LogError((object)"[OnAttackSequenceFire] had to generate hit info because pre-calculated hit info was not available!");
              hitInfo = __instance.GenerateHitInfo(weapon, groupIdx, weaponIdx, numberOfShots, __instance.indirectFire, 0.0f);
              __instance.AddAllAffectedTargets(hitInfo);
            }
            weapon.CompleteFiring();
            foreach (EffectData statusEffect in weapon.weaponDef.statusEffects) {
              if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnActivation) {
                string effectID = string.Format("{0}Effect_{1}_{2}", (object)statusEffect.targetingData.effectTriggerType.ToString(), (object)weapon.parent.GUID, (object)hitInfo.attackSequenceId);
                foreach (ICombatant target in __instance.Director.Combat.EffectManager.GetTargetCombatantForEffect(statusEffect, (ICombatant)weapon.parent, __instance.chosenTarget)) {
                  __instance.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, __instance.stackItemUID, (ICombatant)weapon.parent, target, hitInfo, weaponIdx, false);
                  if (!statusEffect.targetingData.hideApplicationFloatie)
                    __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(weapon.parent.GUID, weapon.parent.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Buff));
                  if (!statusEffect.targetingData.hideApplicationFloatie)
                    __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(weapon.parent.GUID, target.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Buff));
                }
              }
            }
            bool flag = (UnityEngine.Object)weapon.weaponRep != (UnityEngine.Object)null && weapon.weaponRep.HasWeaponEffect;
            if (DebugBridge.TestToolsEnabled)
              flag = flag && !DebugBridge.DisableWeaponEffectDrivenAttacks;
            if (flag) {
              weapon.weaponRep.PlayWeaponEffect(hitInfo);
            } else {
              Log.M?.TWL(0,$"Exception unit:{weapon.parent.PilotableActorDef.ChassisID} weapon:{weapon.weaponDef.Description.Id} weaponRep:"+(weapon.weaponRep==null?"null": (weapon.weaponRep.name+ " HasWeaponEffect:"+weapon.weaponRep.HasWeaponEffect)));
            //  if (DebugBridge.TestToolsEnabled || !DebugBridge.DisableWeaponEffectDrivenAttacks)
            //    AttackDirector.attackLogger.LogError((object)("NO WEAPONEFFECT for " + weapon.Description.Name + ", skipping straight to resolving damage."));
            //  __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponPreFireCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
            //  for (int hitIndex = 0; hitIndex < numberOfShots; ++hitIndex) {
            //    float hitDamage = weapon.DamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
            //    float structureDamage = weapon.StructureDamagePerShotAdjusted(weapon.parent.occupiedDesignMask);
            //    __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(hitInfo, hitIndex, hitDamage, structureDamage));
            //    AdvWeaponHitInfoRec advRec = hitInfo.advRec(hitIndex);
            //    if (advRec.isAOEproc) {
            //      Log.LogWrite("OnImpact AOE Hit info found:" + hitIndex + "\n");
            //      for (int aoeHitIndex = 0; aoeHitIndex < advRec.parent.hits.Count; ++aoeHitIndex) {
            //        AdvWeaponHitInfoRec aoeRec = advRec.parent.hits[aoeHitIndex];
            //        if (aoeRec.isAOE == false) { continue; }
            //        Log.LogWrite(" hitIndex = " + aoeHitIndex + " " + aoeRec.target.GUID + " " + aoeRec.Damage + "/" + aoeRec.Heat + "/" + aoeRec.Stability + "\n");
            //        __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceImpactMessage(hitInfo, aoeHitIndex, aoeRec.Damage, 0f));
            //      }
            //    }
            //  }
            //  __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceResolveDamageMessage(hitInfo));
            //  __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AttackSequenceWeaponCompleteMessage(__instance.stackItemUID, __instance.id, groupIdx, weaponIdx));
            }
          }
        }
        return false;
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
}