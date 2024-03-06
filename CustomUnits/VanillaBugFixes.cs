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

}