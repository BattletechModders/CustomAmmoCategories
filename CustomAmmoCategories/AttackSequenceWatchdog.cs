using BattleTech;
using BattleTech.AttackDirectorHelpers;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using HBS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static BattleTech.AttackDirector;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace CustAmmoCategories {
  public class ASWatchdog: MonoBehaviour {
    public static ASWatchdog instance = null;
    public Dictionary<int, float> WatchDogInfo = new Dictionary<int, float>();
    public AttackDirector AttackDirector = null;
    private float t;
    public void Init(CombatHUD HUD) {
      t = 0f;
      AttackDirector = HUD.Combat.AttackDirector;
      WatchDogInfo = new Dictionary<int, float>();
      ASWatchdog.instance = this;
    }
    public void Woof(int sequenceId) {
      if (WatchDogInfo.ContainsKey(sequenceId) == true) {
        WatchDogInfo[sequenceId] = 0f;
      }
    }
    public void add(int attackSequnceId) {
      if (WatchDogInfo.ContainsKey(attackSequnceId) == false) {
        WatchDogInfo.Add(attackSequnceId, 0f);
      } else {
        WatchDogInfo[attackSequnceId] = 0f;
      }
    }
    public void del(int attackSequnceId) {
      WatchDogInfo.Remove(attackSequnceId);
    }
    public void Update() {
      if (AttackDirector == null) { return; }
      t += Time.deltaTime;
      if (t < 5f) { return; }
      HashSet<int> seq = WatchDogInfo.Keys.ToHashSet();
      foreach (int seqId in seq) {
        WatchDogInfo[seqId] += t;
      }
      t = 0f;
      Log.M.TWL(0, "ASWatchdog.Update", true);
      foreach (int seqId in seq) {
        Log.M.WL(1, seqId.ToString() + " = "+ WatchDogInfo[seqId]);
        if (WatchDogInfo[seqId] > CustomAmmoCategories.Settings.AttackSequenceTimeout) {
          Log.M.WL(2, "timeout");
          AttackSequence attackSequence = AttackDirector.GetAttackSequence(seqId);
          if (attackSequence == null) {
            Log.M.WL(2, "can't find sequence");
          } else {
            Log.M.WL(2, "end sequence by timeout");
            WatchDogInfo.Remove(seqId);
            MessageCoordinator messageCoordinator = (MessageCoordinator)typeof(AttackSequence).GetField("messageCoordinator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(attackSequence);
            typeof(AttackSequence).GetProperty("CoordinatedMesssagesSuccessful", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(attackSequence, new object[1] { (object)messageCoordinator.VerifyAllMessagesComplete() });
            AttackSequenceEndMessage sequenceEndMessage = new AttackSequenceEndMessage(attackSequence.stackItemUID, attackSequence.id);
            attackSequence.chosenTarget.ResolveAttackSequence(attackSequence.attacker.GUID, attackSequence.id, attackSequence.stackItemUID, attackSequence.Director.Combat.HitLocation.GetAttackDirection(attackSequence.attackPosition, attackSequence.chosenTarget));
            attackSequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)sequenceEndMessage);
            messageCoordinator.VerifyAllMessagesComplete();
            continue;
          }
        }
      }
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnAttackSequenceBegin")]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackDirector_OnAttackSequenceBeginWD {
    private static void Postfix(AttackDirector __instance, MessageCenterMessage message) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.OnAttackSequenceBegin add watchdog\n");
      int sequenceId = ((AttackSequenceBeginMessage)message).sequenceId;
      if (ASWatchdog.instance != null) {
        ASWatchdog.instance.add(sequenceId);
      }
    }
  }
  [HarmonyPatch(typeof(AttackDirector))]
  [HarmonyPatch("OnAttackComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackDirector_OnAttackCompleteWD {
    private static void Postfix(AttackDirector __instance, MessageCenterMessage message) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackDirector.OnAttackComplete del watchdog\n");
      AttackCompleteMessage attackCompleteMessage = (AttackCompleteMessage)message;
      int sequenceId = attackCompleteMessage.sequenceId;
      if (ASWatchdog.instance != null) {
        ASWatchdog.instance.del(sequenceId);
      }
    }
  }
  [HarmonyPatch(typeof(AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpactWD {
    private static bool Prefix(AttackSequence __instance, MessageCenterMessage message) {
      if (ASWatchdog.instance != null) {
        ASWatchdog.instance.Woof(__instance.id);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init {
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      ASWatchdog wd = __instance.gameObject.GetComponent<ASWatchdog>();
      if(wd == null) {
        wd = __instance.gameObject.AddComponent<ASWatchdog>();
      }
      if(wd != null) {
        wd.Init(__instance);
      }
      ExplosionAPIHelper.Init(Combat);
    }
  }
}