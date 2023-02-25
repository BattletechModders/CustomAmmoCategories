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
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using HarmonyLib;
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
    private static FieldInfo f_messageCoordinator = typeof(AttackDirector.AttackSequence).GetField("messageCoordinator", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MessageCoordinator messageCoordinator(this AttackDirector.AttackSequence sequence) {
      return (MessageCoordinator)f_messageCoordinator.GetValue(sequence);
    }
    public static void OnAttackSequenceImpactAdv(this AttackDirector.AttackSequence sequence, MessageCenterMessage message) {
      AttackSequenceImpactMessage impactMessage = (AttackSequenceImpactMessage)message;
      if (impactMessage.hitInfo.attackSequenceId != sequence.id)
        return;
      AdvWeaponHitInfoRec advRec = impactMessage.hitInfo.advRec(impactMessage.hitIndex);
      if (advRec == null) { return; }
      advRec.impactMessage = impactMessage;
      advRec.setVisualsState();
      int hitIndex = impactMessage.hitIndex;
      Weapon weapon = advRec.parent.weapon;
      Log.LogWrite("OnAttackSequenceImpactAdv:" + weapon.defId + " "+impactMessage.hitInfo.attackSequenceId+" hi/wi/gi:" + hitIndex + "/"+impactMessage.hitInfo.attackWeaponIndex+"/"+impactMessage.hitInfo.attackGroupIndex+" trg:" + new Text(advRec.target.DisplayName).ToString() + ":" + advRec.hitLocation + " impact:"+impactMessage.hasPlayedImpact+"\n");
      Vector3 hitPosition = impactMessage.hitInfo.hitPositions[hitIndex];
      float damage = advRec.Damage;
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
      float apdmg = advRec.APDamage;// * (damage / advRec.Damage);
      if ((double)apdmg <= 0.0) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing <= 0 damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)apdmg));
        apdmg = 0.0f;
      }
      if (float.IsInfinity(apdmg)) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing inf damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)apdmg));
        apdmg = 0.0f;
      }
      if (float.IsNaN(apdmg)) {
        AttackDirector.attackLogger.LogWarning((object)string.Format("OnAttackSequenceImpact is dealing NaN damage: base dmg: {0}, total: {1}", (object)impactMessage.hitDamage, (object)apdmg));
        apdmg = 0.0f;
      }
      float locArmor = 0f;
      float doggleRoll = impactMessage.hitInfo.dodgeRolls[impactMessage.hitIndex];
      float hitRoll = impactMessage.hitInfo.toHitRolls[impactMessage.hitIndex];
      if (advRec.isHit) { locArmor = advRec.target.ArmorForLocation(advRec.hitLocation); };
      if (!impactMessage.hasPlayedImpact) {
        Log.LogWrite(" impact not played\n");
        //advRec.ApplyTargetResistance();
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
      bool canProcessMessage = advRec.advHitMessage == null ? false : advRec.advHitMessage.CanBeApplied();
      if (!canProcessMessage) {
        Log.M.WL(1, "Can not process message!");
        if (advRec.advHitMessage != null) { advRec.advHitMessage.TryApplyPending(); };
      } else {
         advRec.advHitMessage.Apply(true);
      }
      AdvWeaponHitInfo.printApplyState(sequence.id);
    }
  }
}