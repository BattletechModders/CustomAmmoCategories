using BattleTech;
using BattleTech.AttackDirectorHelpers;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;
using Localize;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
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
      Log.LogWrite("OnAttackSequenceImpactAdv:" + weapon.defId + " " + hitIndex + " trg:" + advRec.target.DisplayName + ":" + advRec.target.GUID + ":" + advRec.hitLocation + "\n");
      //int num1 = impactMessage.hitInfo.ShotHitLocation(hitIndex);
      Vector3 hitPosition = impactMessage.hitInfo.hitPositions[hitIndex];
      //float hitDamage = impactMessage.hitDamage;
      float qualityMultiplier = sequence.Director.Combat.ToHit.GetBlowQualityMultiplier(impactMessage.hitInfo.hitQualities[hitIndex]);
      float damage = advRec.Damage * qualityMultiplier;
      AbstractActor actorTarget = advRec.target as AbstractActor;
      if (actorTarget != null) {
        LineOfFireLevel lineOfFireLevel = sequence.attacker.VisibilityCache.VisibilityToTarget((ICombatant)actorTarget).LineOfFireLevel;
        float adjustedDamage = actorTarget.GetAdjustedDamage(damage, weapon.Category, actorTarget.occupiedDesignMask, lineOfFireLevel, true);
        damage = actorTarget.GetAdjustedDamageForMelee(adjustedDamage, weapon.Category);
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
      bool canPorcessMessage = sequence.messageCoordinator().CanProcessMessage(impactMessage);
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
            Log.LogWrite(" miss\n");
            Vector3 missMsgPos = advRec.target.CurrentPosition + UnityEngine.Random.insideUnitSphere * 5f;
            TerrainHitInfo terrainPos = CustomAmmoCategories.getTerrinHitPosition(impactMessage.hitInfo.stackItemUID);
            if (terrainPos != null) { missMsgPos = terrainPos.pos + UnityEngine.Random.insideUnitSphere * 5f; };
            if (impactMessage.hitInfo.dodgeSuccesses[hitIndex]) {
              advRec.target.GameRep.PlayImpactAnim(impactMessage.hitInfo, hitIndex, weapon, sequence.meleeAttackType, advRec.parent.resolve(advRec.target).cumulativeDamage);
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, new Text("__/CAC.EVADE/__", new object[0]), sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.Dodge, missMsgPos.x, missMsgPos.y, missMsgPos.z));
            } else if (sequence.meleeAttackType != MeleeAttackType.NotSet) {
              advRec.target.GameRep.PlayImpactAnim(impactMessage.hitInfo, hitIndex, weapon, sequence.meleeAttackType, advRec.parent.resolve(advRec.target).cumulativeDamage);
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, new Text("__/CAC.MISS/__", new object[1] { (hitRoll - advRec.parent.hitChance) * 100f }), sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, FloatieMessage.MessageNature.MeleeMiss, missMsgPos.x, missMsgPos.y, missMsgPos.z));
            } else {
              FloatieMessage.MessageNature nature = weapon.ShotsWhenFired <= 1 ? FloatieMessage.MessageNature.MeleeMiss : FloatieMessage.MessageNature.Miss;
              sequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(impactMessage.hitInfo.attackerId, advRec.target.GUID, new Text("__/CAC.MISS/__", new object[1] { (hitRoll - advRec.parent.hitChance) * 100f }), sequence.Director.Combat.Constants.CombatUIConstants.floatieSizeMedium, nature, missMsgPos.x, missMsgPos.y, missMsgPos.z));
            }
          }
        }
        if (weapon.Type != WeaponType.Laser && weapon.Type != WeaponType.Flamer)
          CameraControl.Instance.AddCameraShake(damage * sequence.Director.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageRelativeMod + sequence.Director.Combat.Constants.CombatUIConstants.ScreenShakeRangedDamageAbsoluteMod, 1f, hitPosition);
      }
      if (!canPorcessMessage) {
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
          Log.LogWrite(" crit testing - damage/armor" + damage + "/" + locArmor + " ap dmg:" + apdmg + " ap crit chance:" + critAPchance + "\n");
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
          if (locArmor <= damage) {
            Log.LogWrite(" " + (damage + apdmg) + " all damage processed normally\n");
            advRec.target.TakeWeaponDamage(impactMessage.hitInfo, advRec.hitLocation, weapon, (damage + apdmg), hitIndex, DamageType.Weapon);
          } else {
            Log.LogWrite(" " + damage + " damage to armor\n");
            advRec.target.TakeWeaponDamage(impactMessage.hitInfo, advRec.hitLocation, weapon, damage, hitIndex, DamageType.Weapon);
            Log.LogWrite(" " + apdmg + " damage to structure\n");
            if (apdmg > CustomAmmoCategories.Epsilon) {
              advRec.target.TakeWeaponDamageStructure(impactMessage.hitInfo, advRec.hitLocation, weapon, apdmg, hitIndex, DamageType.Weapon);
            }
          }
          advRec.parent.resolve(advRec.target).AddHit(advRec.hitLocation);
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