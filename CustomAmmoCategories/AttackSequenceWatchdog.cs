using BattleTech;
using BattleTech.AttackDirectorHelpers;
using BattleTech.UI;
using CustAmmoCategories;
using Harmony;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static BattleTech.AttackDirector;

namespace CustAmmoCategories {
  public class AttackSequenceWatchdog {
    public Stopwatch timer;
    public int attackSequenceId;
    public AttackSequenceWatchdog(int id) {
      this.attackSequenceId = id;
      timer = new Stopwatch();
      timer.Start();
    }
  }
  public static class ASWatchdog {
    public static Dictionary<int, AttackSequenceWatchdog> WatchDogInfo = new Dictionary<int, AttackSequenceWatchdog>();
    public static Mutex mutex = new Mutex();
    public static AttackDirector AttackDirector = null;
    public static Thread watchdogThread = new Thread(watchdogProc);
    public static bool watchdogTerminating = false;
    public static void Woof(int sequenceId) {
      if (mutex.WaitOne(1000)) {
        if (ASWatchdog.WatchDogInfo.ContainsKey(sequenceId) == true) {
          AttackSequenceWatchdog wd = ASWatchdog.WatchDogInfo[sequenceId];
          wd.timer.Stop();
          CustomAmmoCategoriesLog.Log.LogWrite("Watchdog " + sequenceId + ": "+wd.timer.ElapsedMilliseconds+"ms Woof! Woof!\n");
          wd.timer.Reset();
          wd.timer.Start();
        }
        mutex.ReleaseMutex();
      }
    }
    public static void StartWatchDogThread() {
      CustomAmmoCategoriesLog.Log.LogWrite("StartWatchDogThread. Watchdog thread state:" + ASWatchdog.watchdogThread.ThreadState + "\n");
      if ((ASWatchdog.watchdogThread.ThreadState == System.Threading.ThreadState.Stopped)
        || (ASWatchdog.watchdogThread.ThreadState == System.Threading.ThreadState.Unstarted)) 
      {
        ASWatchdog.watchdogTerminating = false;
        CustomAmmoCategoriesLog.Log.LogWrite(" starting\n");
        ASWatchdog.watchdogThread.Start();
      }
    }
    public static void EndWatchDogThread() {
      CustomAmmoCategoriesLog.Log.LogWrite("StartWatchDogThread. Watchdog thread state:" + ASWatchdog.watchdogThread.ThreadState + "\n");
      if (ASWatchdog.watchdogThread.ThreadState == System.Threading.ThreadState.Running) {
        ASWatchdog.watchdogTerminating = true;
        ASWatchdog.watchdogThread.Join();
      }
    }
    public static void watchdogProc() {
      int counter = 0;
      CustomAmmoCategoriesLog.Log.LogWrite("Watchdog thread started\n");
      while (ASWatchdog.watchdogTerminating == false) {
        Thread.Sleep(1000);
        if(counter > 10) {
          counter = 0;
          ASWatchdog.testWatchdogs();
        }
        ++counter;
      }
      CustomAmmoCategoriesLog.Log.LogWrite("Watchdog thread ended\n");
    }
    public static void addAttackSequenceToWatch(int attackSequnceId) {
      if (mutex.WaitOne()) {
        if (ASWatchdog.WatchDogInfo.ContainsKey(attackSequnceId) == false) {
          ASWatchdog.WatchDogInfo.Add(attackSequnceId,new AttackSequenceWatchdog(attackSequnceId));
          CustomAmmoCategoriesLog.Log.LogWrite("added watchdog sequence:"+attackSequnceId+"\n");
        }
        mutex.ReleaseMutex();
      }
    }
    public static void delAttackSequenceToWatch(int attackSequnceId) {
      if (mutex.WaitOne()) {
        if (ASWatchdog.WatchDogInfo.ContainsKey(attackSequnceId) == true) {
          ASWatchdog.WatchDogInfo.Remove(attackSequnceId);
          CustomAmmoCategoriesLog.Log.LogWrite("deleted watchdog sequence:" + attackSequnceId + "\n");
        }
        mutex.ReleaseMutex();
      }
    }
    public static void testWatchdogs() {
      if (mutex.WaitOne(1000)) {
        CustomAmmoCategoriesLog.Log.LogWrite("attack sequence watchdog:\n");
        HashSet<int> delSequence = new HashSet<int>();
        foreach (var wd in ASWatchdog.WatchDogInfo) {
          wd.Value.timer.Stop();
          CustomAmmoCategoriesLog.Log.LogWrite(" "+wd.Value.attackSequenceId+":"+wd.Value.timer.ElapsedMilliseconds+"ms");
          if(wd.Value.timer.ElapsedMilliseconds > CustomAmmoCategories.Settings.AttackSequenceMaxLength) {
            CustomAmmoCategoriesLog.Log.LogWrite(" elapced\n");
            delSequence.Add(wd.Key);
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" normal\n");
          }
          wd.Value.timer.Start();
        }
        foreach(int sequenceId in delSequence) {
          CustomAmmoCategoriesLog.Log.LogWrite("force finish sequence:"+sequenceId+"\n");
          if (ASWatchdog.AttackDirector == null) { continue; };
          AttackSequence attackSequence = ASWatchdog.AttackDirector.GetAttackSequence(sequenceId);
          if (attackSequence == null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" can't find sequence\n");
            ASWatchdog.WatchDogInfo.Remove(sequenceId);
            continue;
          }
          CustomAmmoCategoriesLog.Log.LogWrite(" force sequence to end\n");
          MessageCoordinator messageCoordinator = (MessageCoordinator)typeof(AttackSequence).GetField("messageCoordinator", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(attackSequence);
          typeof(AttackSequence).GetProperty("CoordinatedMesssagesSuccessful", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(attackSequence, new object[1] { (object)messageCoordinator.VerifyAllMessagesComplete() });
          AttackSequenceEndMessage sequenceEndMessage = new AttackSequenceEndMessage(attackSequence.stackItemUID, attackSequence.id);
          attackSequence.chosenTarget.ResolveAttackSequence(attackSequence.attacker.GUID, attackSequence.id, attackSequence.stackItemUID, attackSequence.Director.Combat.HitLocation.GetAttackDirection(attackSequence.attackPosition, attackSequence.chosenTarget));
          attackSequence.Director.Combat.MessageCenter.PublishMessage((MessageCenterMessage)sequenceEndMessage);
          messageCoordinator.VerifyAllMessagesComplete();
          ASWatchdog.WatchDogInfo.Remove(sequenceId);
        }
        mutex.ReleaseMutex();
      }
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(AttackSequence))]
  [HarmonyPatch(MethodType.Constructor)]
  [HarmonyPatch(new Type[] { typeof(AttackDirector), typeof(AbstractActor), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(int), typeof(List<Weapon>), typeof(MeleeAttackType), typeof(int), typeof(bool), typeof(int), typeof(int)})]
  public static class AttackSequence_Constructor {
    private static void Postfix(AttackSequence __instance, AttackDirector director, AbstractActor attacker, ICombatant target, Vector3 attackPosition, Quaternion attackRotation, int attackSequenceIdx, List<Weapon> selectedWeapons, MeleeAttackType meleeAttackType, int calledShotLocation, bool isMoraleAttack, int id, int stackItemUID) {
      CustomAmmoCategoriesLog.Log.LogWrite("AttackSequence.Constructor add watchdog\n");
      ASWatchdog.AttackDirector = director;
      ASWatchdog.addAttackSequenceToWatch(__instance.id);
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
      ASWatchdog.delAttackSequenceToWatch(sequenceId);
    }
  }
  [HarmonyPatch(typeof(AttackSequence))]
  [HarmonyPatch("OnAttackSequenceImpact")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
  public static class AttackSequence_OnAttackSequenceImpactWD {
    private static bool Prefix(AttackSequence __instance, MessageCenterMessage message) {
      ASWatchdog.Woof(__instance.id);
      return true;
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
  public static class CombatHUD_Init {
    public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
      ASWatchdog.StartWatchDogThread();
    }
  }

}