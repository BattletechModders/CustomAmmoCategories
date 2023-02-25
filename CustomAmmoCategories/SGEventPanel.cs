using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategoriesPatches {
  public class SGEventPanelMutex : MonoBehaviour {
    public static Mutex mutex { get; set; } = new Mutex();
    public int fired { get; set; } = 0;
    public int dismissed { get; set; } = 0;
    public void Start() {
      Log.M?.TWL(0, "SGEventPanelMutex.Start");
      fired = 0;
      dismissed = 0;
    }
    public void OnDisable() {
      fired = 0;
      dismissed = 0;
      Log.M?.TWL(0, "SGEventPanelMutex.OnDisable");
    }
  }
  [HarmonyPatch(typeof(SGEventPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Init")]
  [HarmonyPatch(new Type[] { typeof(SimGameState) })]
  public static class SGEventPanel_Init {
    public static void Prefix(SGEventPanel __instance, SimGameState sim) {
      try {
        SGEventPanelMutex.mutex.WaitOne();
        SGEventPanelMutex mutex = __instance.gameObject.GetComponent<SGEventPanelMutex>();
        if (mutex == null) { mutex = __instance.gameObject.AddComponent<SGEventPanelMutex>(); }
        Log.M?.TWL(0, "SGEventPanel.Init");
        mutex.fired = 0;
        mutex.dismissed = 0;
        SGEventPanelMutex.mutex.ReleaseMutex();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(SGEventPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("OnOptionSelected")]
  [HarmonyPatch(new Type[] { typeof(SimGameEventOption) })]
  public static class SGEventPanel_OnOptionSelected {
    public static bool Prefix(SGEventPanel __instance, SimGameEventOption option) {
      bool result = true;
      try {
        SGEventPanelMutex.mutex.WaitOne();
        SGEventPanelMutex mutex = __instance.gameObject.GetComponent<SGEventPanelMutex>();
        if (mutex == null) { mutex = __instance.gameObject.AddComponent<SGEventPanelMutex>(); }
        result = (mutex.fired == 0);
        Log.M?.TWL(0, "SGEventPanel.OnOptionSelected fire counter " + mutex.fired + ". proceed " + result);
        ++mutex.fired;
        SGEventPanelMutex.mutex.ReleaseMutex();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
      return result;
    }
  }
  [HarmonyPatch(typeof(SGEventPanel))]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch("Dismiss")]
  [HarmonyPatch(new Type[] { })]
  public static class SGEventPanel_Dismiss {
    public static bool Prefix(SGEventPanel __instance) {
      bool result = true;
      try {
        SGEventPanelMutex.mutex.WaitOne();
        SGEventPanelMutex mutex = __instance.gameObject.GetComponent<SGEventPanelMutex>();
        if (mutex == null) { mutex = __instance.gameObject.AddComponent<SGEventPanelMutex>(); }
        result = (mutex.dismissed == 0);
        Log.M?.TWL(0, "SGEventPanel.Dismiss fire counter " + mutex.dismissed + ". proceed " + result);
        ++mutex.dismissed;
        SGEventPanelMutex.mutex.ReleaseMutex();
      } catch (Exception e) {
        Log.M?.TWL(0, e.ToString(), true);
      }
      return result;
    }
  }
}