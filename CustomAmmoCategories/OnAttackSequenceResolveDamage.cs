using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;

namespace CustomAmmoCategoriesPatches
{
    [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
    [HarmonyPatch("OnAttackSequenceResolveDamage")]
    [HarmonyPatch(MethodType.Normal)]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class AttackSequence_OnAttackSequenceResolveDamage
    {
        public static bool Prefix(AttackDirector.AttackSequence __instance, MessageCenterMessage message)
        {
            CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.AttackSequence.OnAttackSequenceResolveDamage");
            try
            {
                AttackSequenceResolveDamageMessage resolveDamageMessage = (AttackSequenceResolveDamageMessage)message;
                WeaponHitInfo hitInfo = resolveDamageMessage.hitInfo;
                if (hitInfo.attackSequenceId != __instance.id) { return true; };
                MessageCoordinator messageCoordinator = (MessageCoordinator)typeof(AttackDirector.AttackSequence).GetField("messageCoordinator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                if (!messageCoordinator.CanProcessMessage(resolveDamageMessage))
                {
                    messageCoordinator.StoreMessage((MessageCenterMessage)resolveDamageMessage);
                }
                else
                {
                    if (AttackDirector.AttackSequence.logger.IsLogEnabled)
                        AttackDirector.AttackSequence.logger.Log((object)string.Format("[OnAttackSequenceResolveDamage]  ID {0}, Group {1}, Weapon {2}, AttackerId [{3}], TargetId [{4}]", (object)__instance.id, (object)hitInfo.attackGroupIndex, (object)hitInfo.attackWeaponIndex, (object)hitInfo.attackerId, (object)hitInfo.targetId));
                    Weapon weapon = __instance.GetWeapon(resolveDamageMessage.hitInfo.attackGroupIndex, resolveDamageMessage.hitInfo.attackWeaponIndex);
                    if (__instance.meleeAttackType == MeleeAttackType.DFA)
                    {
                        float damageAmount = __instance.attacker.StatCollection.GetValue<float>("DFASelfDamage");
                        __instance.attacker.TakeWeaponDamage(resolveDamageMessage.hitInfo, 64, weapon, damageAmount, 0, DamageType.DFASelf);
                        __instance.attacker.TakeWeaponDamage(resolveDamageMessage.hitInfo, 128, weapon, damageAmount, 0, DamageType.DFASelf);
                        if (AttackDirector.damageLogger.IsLogEnabled)
                            AttackDirector.damageLogger.Log((object)string.Format("@@@@@@@@ {0} takes {1} damage to its legs from the DFA attack!", (object)__instance.attacker.DisplayName, (object)damageAmount));
                    }
                    __instance.target.ResolveWeaponDamage(resolveDamageMessage.hitInfo);
                    AbstractActor target = __instance.target as AbstractActor;
                    int attackIndex = -1;
                    int num1 = 0;
                    int num2 = 65536;
                    for (int index = 0; index < resolveDamageMessage.hitInfo.hitLocations.Length; ++index)
                    {
                        int hitLocation = resolveDamageMessage.hitInfo.hitLocations[index];
                        if (hitLocation != num1 && hitLocation != num2 && attackIndex == -1)
                            attackIndex = index;
                    }
                    if (attackIndex > -1)
                    {
                        //typeof(AttackDirector.AttackSequence).GetProperty("attackCompletelyMissed", BindingFlags.NonPublic).SetValue(__instance, (object)false);
                        PropertyInfo property = typeof(AttackDirector.AttackSequence).GetProperty("attackCompletelyMissed");
                        property.DeclaringType.GetProperty("attackCompletelyMissed");
                        property.GetSetMethod(true).Invoke(__instance, new object[1] { (object)false });
                        //__instance.attackCompletelyMissed = false;
                    }
                    if (attackIndex > -1 && !__instance.target.IsDead && target != null)
                    {
                        foreach (EffectData statusEffect in CustomAmmoCategories.getWeaponStatusEffects(weapon))
                        {
                            if (statusEffect.targetingData.effectTriggerType == EffectTriggerType.OnHit)
                            {
                                string effectID = string.Format("OnHitEffect_{0}_{1}", (object)__instance.attacker.GUID, (object)resolveDamageMessage.hitInfo.attackSequenceId);
                                __instance.Director.Combat.EffectManager.CreateEffect(statusEffect, effectID, __instance.stackItemUID, (ICombatant)__instance.attacker, __instance.target, hitInfo, attackIndex, false);
                                if (__instance.target != null)
                                    __instance.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.target.GUID, __instance.target.GUID, statusEffect.Description.Name, FloatieMessage.MessageNature.Debuff));
                            }
                        }
                        if (target != null)
                        {
                            List<EffectData> effectsForTriggerType = target.GetComponentStatusEffectsForTriggerType(EffectTriggerType.OnDamaged);
                            for (int index = 0; index < effectsForTriggerType.Count; ++index)
                                __instance.Director.Combat.EffectManager.CreateEffect(effectsForTriggerType[index], string.Format("OnDamagedEffect_{0}_{1}", (object)target.GUID, (object)resolveDamageMessage.hitInfo.attackSequenceId), __instance.stackItemUID, __instance.target, (ICombatant)__instance.attacker, hitInfo, attackIndex, false);
                        }
                    }
                    __instance.attacker.HandleDeath(__instance.attacker.GUID);
                    __instance.attacker.HandleDeath(__instance.attacker.GUID);
                    messageCoordinator.MessageComplete((MessageCenterMessage)resolveDamageMessage);
                }
            }
            catch (Exception e)
            {
                CustomAmmoCategoriesLog.Log.LogWrite("Exception "+e.ToString()+"\nFallback to default");
                return true;
            }
            return false;
        }
    }
}
