using BattleTech;
using BattleTech.UI;
using HarmonyLib;
using System;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(SimGameCameraController))]
  [HarmonyPatch("GetMechBounds")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(GameObject) })]
  public static class SimGameCameraController_GetMechBounds {
    static void Prefix(SimGameCameraController __instance, ref GameObject mech) {
      try {
        if (mech == null) {
          UIManager.logger.LogError("SimGameCameraController.GetMechBounds called with null mech. I'm NOT, repeat NOT aware of reason!");
          mech = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        } else {
          Renderer[] componentsInChildren = mech.GetComponentsInChildren<Renderer>();
          if (componentsInChildren.Length == 0) {
            UIManager.logger.LogError("SimGameCameraController.GetMechBounds called with null mech that does not have any Renderer on it. I'm NOT, repeat NOT aware of reason!");
            GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.SetParent(mech.transform);
          }
        }
      } catch (Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(SGContractsWidget))]
  [HarmonyPatch("OnContractAccepted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class SGContractsWidget_OnContractAccepted {
    private static float Timer = 0f;
    private static readonly float TIMER_MAX = 3f;
    public class OnContractAcceptedTimer : MonoBehaviour {
      public void Update() {
        if (Timer > Core.Epsilon) {
          Timer -= Time.deltaTime;
        } else if(Timer != 0f) { 
          Timer = 0f;
        }
      }
    }
    static void Prefix(ref bool __runOriginal, SGContractsWidget __instance) {
      try {
        if (SGContractsWidget_OnContractAccepted.Timer > 0f) { __runOriginal = false; return; }
        Timer = TIMER_MAX;
        if (__instance.gameObject.GetComponent<OnContractAcceptedTimer>() == null) {
          __instance.gameObject.AddComponent<OnContractAcceptedTimer>();
        }
      } catch (Exception e) {
        UIManager.logger.LogException(e);
      }
    }
  }

}