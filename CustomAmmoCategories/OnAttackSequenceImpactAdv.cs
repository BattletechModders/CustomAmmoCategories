using BattleTech;
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomAmmoCategoriesPatches {
  /*[HarmonyPatch(typeof(MessageCoordinator))]
  [HarmonyPatch("CanProcessMessage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AttackSequenceImpactMessage) })]
  public static class MessageCoordinator_CanProcessMessage {
    private delegate bool IsImpactMessageAHitDelegate(MessageCoordinator coordinator, AttackSequenceImpactMessage message);
    private static IsImpactMessageAHitDelegate IsImpactMessageAHitIvoker = null;
    private delegate bool MessageMatchesNextExpectedMesssageDelegate(MessageCoordinator coordinator, AttackSequenceImpactMessage message);
    private static IsImpactMessageAHitDelegate MessageMatchesNextExpectedMesssageIvoker = null;
    private static FieldInfo expectedMessages
    public static bool Prepare() {
      {
        MethodInfo IsImpactMessageAHit = typeof(MessageCoordinator).GetMethod("IsImpactMessageAHit", BindingFlags.Instance | BindingFlags.NonPublic);
        var dm = new DynamicMethod("CACIsImpactMessageAHit", typeof(bool), new Type[] { typeof(MessageCoordinator), typeof(AttackSequenceImpactMessage) }, typeof(MessageCoordinator));
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Call, IsImpactMessageAHit);
        gen.Emit(OpCodes.Ret);
        IsImpactMessageAHitIvoker = (IsImpactMessageAHitDelegate)dm.CreateDelegate(typeof(IsImpactMessageAHitDelegate));
      }
      return true;
    }
    public static bool IsImpactMessageAHit(this MessageCoordinator coordinator, AttackSequenceImpactMessage message) {
      return IsImpactMessageAHitIvoker(coordinator, message);
    }
    public static bool 
    public static bool Prefix(MessageCoordinator __instance, AttackSequenceImpactMessage impactMessage, List<ExpectedMessage> ___expectedMessages, int ___expectedMessageIndex, ref bool __result) {
      if (!__instance.IsImpactMessageAHit(impactMessage)) {
        if (MessageCoordinator.logger.IsDebugEnabled)
          MessageCoordinator.logger.LogDebug((object)string.Format("{0} is a miss. trigger it now", (object)__instance.MessageToString((MessageCenterMessage)impactMessage)));
        return true;
      }
      bool flag = __instance.MessageMatchesNextExpectedMesssage((MessageCenterMessage)impactMessage);
      if (MessageCoordinator.logger.IsDebugEnabled) {
        if (flag)
          MessageCoordinator.logger.LogDebug((object)string.Format("Message {0} matches next message. trigger it now.", (object)__instance.MessageToString((MessageCenterMessage)impactMessage)));
        else
          MessageCoordinator.logger.LogDebug((object)string.Format("Message {0} must be stored.", (object)__instance.MessageToString((MessageCenterMessage)impactMessage)));
      }
      if (flag)
        __instance.GetNextExpectedMessage().ClearForProcessing();
      return flag;

    }
  }*/

}

namespace CustAmmoCategories {
  public enum ShowMissBehavior { None,Vanilla,Default,All }
  public static class OnAttackSeuenceImpactHelper {
    public static MessageCoordinator messageCoordinator(this AttackDirector.AttackSequence sequence) {
      return (MessageCoordinator)typeof(AttackDirector.AttackSequence).GetField("messageCoordinator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequence);
    }
    public static void OnAttackSequenceImpactAdv(this AttackDirector.AttackSequence sequence, MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != sequence.id)
        return;
      AdvWeaponHitInfoRec advRec = impactMessage.hitInfo.advRec(impactMessage.hitIndex);
      //int attackGroupIndex = impactMessage.hitInfo.attackGroupIndex;
      //int attackWeaponIndex = impactMessage.hitInfo.attackWeaponIndex;
      int hitIndex = impactMessage.hitIndex;
      Weapon weapon = advRec.parent.weapon;
      Log.LogWrite("OnAttackSequenceImpactAdv:" + weapon.defId + " "+impactMessage.hitInfo.attackSequenceId+" hi/wi/gi:" + hitIndex + "/"+impactMessage.hitInfo.attackWeaponIndex+"/"+impactMessage.hitInfo.attackGroupIndex+" trg:" + new Text(advRec.target.DisplayName).ToString() + ":" + advRec.hitLocation + " impact:"+impactMessage.hasPlayedImpact+"\n");
      //int num1 = impactMessage.hitInfo.ShotHitLocation(hitIndex);
      Vector3 hitPosition = impactMessage.hitInfo.hitPositions[hitIndex];
      //float hitDamage = impactMessage.hitDamage;
      float qualityMultiplier = sequence.Director.Combat.ToHit.GetBlowQualityMultiplier(impactMessage.hitInfo.hitQualities[hitIndex]);
      float damage = advRec.Damage * qualityMultiplier;
      AbstractActor actorTarget = advRec.target as AbstractActor;
      if (actorTarget != null) {
        LineOfFireLevel lineOfFireLevel = sequence.attacker.VisibilityCache.VisibilityToTarget((ICombatant)actorTarget).LineOfFireLevel;
#if BT1_8
        float adjustedDamage = actorTarget.GetAdjustedDamage(damage, weapon.WeaponCategoryValue, actorTarget.occupiedDesignMask, lineOfFireLevel, true);
        damage = actorTarget.GetAdjustedDamageForMelee(adjustedDamage, weapon.WeaponCategoryValue);
#else
        float adjustedDamage = actorTarget.GetAdjustedDamage(damage, weapon.Category, actorTarget.occupiedDesignMask, lineOfFireLevel, true);
        damage = actorTarget.GetAdjustedDamageForMelee(adjustedDamage, weapon.Category);
#endif
      }
      if ((double)damage <= 0.0) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing <= 0 damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)damage));
        damage = 0.0f;
      }
      if (float.IsInfinity(damage)) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing inf damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)damage));
        damage = 0.0f;
      }
      if (float.IsNaN(damage)) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing NaN damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)damage));
        damage = 0.0f;
      }
      float apdmg = advRec.APDamage * (damage / advRec.Damage);
      bool canProcessMessage = sequence.messageCoordinator().CanProcessMessage(impactMessage);
      //bool flag3 = impactMessage.hitInfo.DidShotHitChosenTarget(hitIndex);
      float locArmor = 0f;
      float doggleRoll = impactMessage.hitInfo.dodgeRolls[impactMessage.hitIndex];
      float hitRoll = impactMessage.hitInfo.toHitRolls[impactMessage.hitIndex];
      if (advRec.target.isAPProtected()) { apdmg = 0f; };
      if (advRec.isHit) { locArmor = advRec.target.ArmorForLocation(advRec.hitLocation); };
      if (!impactMessage.hasPlayedImpact) {
        Log.LogWrite(" impact not played\n");
        //if (AttackDirector.AttackSequence.logger.IsDebugEnabled)
        //AttackDirector.AttackSequence.logger.LogDebug((object)string.Format("[OnAttackSequenceImpact] playing impact \"visuals\" for ID {0}, Group {1}, Weapon {2}, Hit {3}. Will process during this call? {4}", (object)sequence.id, (object)attackGroupIndex, (object)attackWeaponIndex, (object)hitIndex, (object)flag2));
        impactMessage.hasPlayedImpact = true;
        if ((UnityEngine.Object)advRec.target.GameRep != (UnityEngine.Object)null) {
          Log.LogWrite(" gameRep exists\n");
          if (advRec.isHit) {
            Log.LogWrite(" hit.\n");
            advRec.target.GameRep.PlayImpactAnim(impactMessage.hitInfo, hitIndex, weapon, sequence.meleeAttackType, advRec.parent.resolve(advRec.target).cumulativeDamage);
            Vector3 fPrimPos = hitPosition;
            Vector3 fSecPos = hitPosition + Vector3.up * 0.1f;
            float arDamage = damage;
            float isDamage = apdmg;
            if (locArmor < arDamage) { arDamage = locArmor; isDamage += (damage - locArmor); };
            Log.LogWrite(" damage:" + arDamage + ":" + isDamage + "\n");
            if (arDamage > 0f) {
              Log.LogWrite(" armor floatie\n");
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID,
              new Text("{0}", new object[1] { (object)(int)Mathf.Max(1f, arDamage) })
              , sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.ArmorDamage, fPrimPos.x, fPrimPos.y, fPrimPos.z));
              fPrimPos = fSecPos;
            }
            if (isDamage > 0f) {
              //advRec.parent.resolve(advRec.target).AddCrit(advRec.hitLocation);
              Log.LogWrite(" is floatie\n");
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID,
                new Text("{0}", new object[1] { (object)(int)Mathf.Max(1f, isDamage) })
              , sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.StructureDamage, fPrimPos.x, fPrimPos.y, fPrimPos.z));
            }
          } else {
            Vector3 missMsgPos = advRec.target.CurrentPosition + UnityEngine.Random.insideUnitSphere * 5f;
            TerrainHitInfo terrainPos = null;
            if (impactMessage.hitInfo.targetId == impactMessage.hitInfo.attackerId) {
              terrainPos = CustomAmmoCategories.getTerrinHitPosition(impactMessage.hitInfo.attackerId);
            }
            if (terrainPos != null) { missMsgPos = terrainPos.pos + UnityEngine.Random.insideUnitSphere * 5f; };
            if (impactMessage.hitInfo.dodgeSuccesses[hitIndex]) {
              Log.LogWrite(" dodgeSuccesses\n");
              advRec.target.GameRep.PlayImpactAnim(impactMessage.hitInfo, hitIndex, weapon, sequence.meleeAttackType, advRec.parent.resolve(advRec.target).cumulativeDamage);
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, new Text("__/CAC.EVADE/__", new object[0]), sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.MeleeMiss, missMsgPos.x, missMsgPos.y, missMsgPos.z));
            } else if (sequence.meleeAttackType != MeleeAttackType.NotSet) {
              Log.LogWrite(" melee\n");
              advRec.target.GameRep.PlayImpactAnim(impactMessage.hitInfo, hitIndex, weapon, sequence.meleeAttackType, advRec.parent.resolve(advRec.target).cumulativeDamage);
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, new Text("__/CAC.MISS/__", new object[1] { (hitRoll - advRec.parent.hitChance) * 100f }), sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.MeleeMiss, missMsgPos.x, missMsgPos.y, missMsgPos.z));
            } else {
              FloatieMessage.MessageNature nature = weapon.ShotsWhenFired <= 1 ? FloatieMessage.MessageNature.MeleeMiss : FloatieMessage.MessageNature.Miss;
              Text text = null;
              if (advRec.interceptInfo.Intercepted) {
                missMsgPos = advRec.hitPosition;
                text = new Text("__/CAC.INTERCEPTED/__");
              } else {
                text = new Text("__/CAC.MISS/__", new object[1] { (hitRoll - advRec.parent.hitChance) * 100f });
              }
              if (CustomAmmoCategories.Settings.showMissBehavior == ShowMissBehavior.All) { nature = FloatieMessage.MessageNature.ArmorDamage; };
              if (CustomAmmoCategories.Settings.showMissBehavior != ShowMissBehavior.None) {
                Log.LogWrite(" normal '" + text.ToString() + "': " + impactMessage.hitInfo.attackerId + " " + new Text(advRec.target.DisplayName).ToString() + ":" + advRec.target.GUID + " pos:" + missMsgPos + " " + advRec.target.CurrentPosition + "\n");
                sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, text, sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeLarge, nature, missMsgPos.x, missMsgPos.y, missMsgPos.z));
              }
            }
          }
        }
        if (weapon.Type != WeaponType.Laser && weapon.Type != WeaponType.Flamer)
          CameraControl.Instance.AddCameraShake(damage * sequence.Director.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageRelativeMod + sequence.Director.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageAbsoluteMod, 1f, hitPosition);
      }
      if (!canProcessMessage) {
        Log.M.WL(1, "Can not process message!");
        sequence.messageCoordinator().StoreMessage((MessageCenterMessage)impactMessage);
      } else {
        if (advRec.isHit) {
          sequence.FlagAttackDidDamage(advRec.target.GUID);
          sequence.attackCompletelyMissed(false);
          //if (AttackDirector.AttackSequence.logger.IsLogEnabled) {
          //  AttackDirector.AttackSequence.logger.Log((object)string.Format("[OnAttackSequenceImpact]  ID {0}, Group {1}, Weapon {2}, Hit {3}.", (object)sequence.id, (object)attackGroupIndex, (object)attackWeaponIndex, (object)hitIndex));
          //  if (AttackDirector.AttackSequence.logger.IsDebugEnabled)
          //    AttackDirector.AttackSequence.logger.LogDebug((object)string.Format("\t WeaponName {0}, MeleeType {1}, HitLocation {2}", (object)weapon.Name, (object)sequence.meleeAttackType.ToString(), (object)num1.ToString()));
          //}
          sequence.cumulativeDamage += (damage + apdmg);
          advRec.parent.resolve(advRec.target).cumulativeDamage += (damage + apdmg);
          float critAPchance = weapon.isAPCrit() ? advRec.parent.weapon.APCriticalChanceMultiplier() : float.NaN;
          Log.LogWrite(" crit testing - damage/armor " + damage + "/" + locArmor + " ap dmg:" + apdmg + " ap crit chance:" + critAPchance + "\n");
          if (damage > locArmor) {
            Log.LogWrite("  crit to location armor breach:" + advRec.hitLocation + "\n");
            advRec.parent.resolve(advRec.target).AddCrit(advRec.hitLocation,advRec.target);
          } else if (advRec.isAOE == false) {
            if (advRec.target.isAPProtected() == false) {
              if ((apdmg > CustomAmmoCategories.Epsilon) || weapon.isAPCrit()) {
                Log.LogWrite("  crit to location armor pierce:" + advRec.hitLocation + "\n");
                advRec.parent.resolve(advRec.target).AddCrit(advRec.hitLocation, advRec.target);
              } else {
                Log.LogWrite("  ap damage is zero and weapon not cause AP crits:" + advRec.hitLocation + "\n");
              }
            } else {
              Log.LogWrite("  target is AP crit protected:" + advRec.hitLocation + "\n");
            }
          } else {
            Log.LogWrite("  AoE can't inflict armor pierce crits:" + advRec.hitLocation + "\n");
          }

          Log.LogWrite(" resolve damage. arm: " + locArmor + " dmg:" + damage + " ap:" + apdmg + "\n");
#if BT1_8
          advRec.target.TakeWeaponDamage(impactMessage.hitInfo, advRec.hitLocation, weapon, damage, apdmg, hitIndex, DamageType.Weapon);
#else
          if (locArmor <= damage) {
            Log.LogWrite(" " + (damage + apdmg) + " all damage processed normally\n");
            advRec.target.TakeWeaponDamage(impactMessage.hitInfo, advRec.hitLocation, weapon, damage+apdmg, hitIndex, DamageType.Weapon);
          } else {
            Log.LogWrite(" " + damage + " damage to armor\n");
            advRec.target.TakeWeaponDamage(impactMessage.hitInfo, advRec.hitLocation, weapon, damage, hitIndex, DamageType.Weapon);
            Log.LogWrite(" " + apdmg + " damage to structure\n");
            if (apdmg > CustomAmmoCategories.Epsilon) {
              advRec.target.TakeWeaponDamageStructure(impactMessage.hitInfo, advRec.hitLocation, weapon, apdmg, hitIndex, DamageType.Weapon);
            }
          }
#endif
          advRec.parent.resolve(advRec.target).AddHit(advRec.hitLocation,advRec.EffectsMod,advRec.isAOE);
          advRec.parent.resolve(advRec.target).AddHeat(advRec.Heat);
          Log.LogWrite(" Added heat:"+advRec.Heat+" overall: "+ advRec.parent.resolve(advRec.target).Heat+ "\n");
          advRec.parent.resolve(advRec.target).AddInstability(advRec.Stability);
          Log.LogWrite(" Added instability:" + advRec.Stability + " overall: " + advRec.parent.resolve(advRec.target).Stability + "\n");
          advRec.target.HandleDeath(sequence.attacker.GUID);
        }
        sequence.messageCoordinator().MessageComplete((MessageCenterMessage)impactMessage);
      }
    }
  }
}