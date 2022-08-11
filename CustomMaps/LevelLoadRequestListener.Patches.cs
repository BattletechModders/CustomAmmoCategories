using BattleTech;
using BattleTech.Data;
using Harmony;
using System;
using System.Collections;
using UnityEngine;

namespace CustomMaps.Patches {
  [HarmonyPatch(typeof(LevelLoadRequestListener))]
  [HarmonyPatch("BundlesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LevelLoadRequestListener_BundlesLoaded {
    public static void Prefix(LevelLoadRequestListener __instance, LoadRequest request) {
      try {
        __instance.dataManager.MessageCenter.AddSubscriber(MessageCenterMessageType.LevelLoadComplete, new ReceiveMessageCenterMessage(__instance.LevelLoadedFromAssetbundles));
        __instance.StartCoroutine(__instance.AttemptLoadSceneCustom(false)); //we will clean request at LevelLoadedFromAssetbundles invocation
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
      }
    }
  }

  [HarmonyPatch(typeof(LevelLoadRequestListener))]
  [HarmonyPatch("OnRequestLevelLoad")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LevelLoadRequestListener_OnRequestLevelLoad {
    public static IEnumerator AttemptLoadSceneCustom(this LevelLoadRequestListener __instance, bool nullifyRequest) {
      while (!LevelLoader.CanLoadScene())
        yield return (object)null;
      LevelLoader.LoadScene(__instance.request.MapName, __instance.request.Interstitial);
      if(nullifyRequest) __instance.request = (LevelLoadRequestMessage)null;
    }
    public static void BundlesLoadedCustom(this LevelLoadRequestListener __instance) {
      try {
        __instance.dataManager.MessageCenter.AddSubscriber(MessageCenterMessageType.LevelLoadComplete, new ReceiveMessageCenterMessage(__instance.LevelLoadedFromAssetbundles));
        __instance.StartCoroutine(__instance.AttemptLoadSceneCustom(false)); //we will clean request at LevelLoadedFromAssetbundles invocation
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
      }
    }
    public static bool Prefix(LevelLoadRequestListener __instance, MessageCenterMessage message) {
      try {
        if (__instance.request != null) {
          Log.M_err?.TWL(0, string.Format("Received LevelLoadRequest {0}, While one is already active {1}", (object)(message as LevelLoadRequestMessage).MapName, (object)__instance.request.MapName));
        } else {
          __instance.request = message as LevelLoadRequestMessage;
          if (message == null) { return true; }
          string baseMapName = __instance.request.MapName;
          CustomMapDef customMap = Core.findCustomMap(baseMapName);
          if (customMap != null) {
            Log.M?.TWL(0, baseMapName+" is custom based on "+ customMap.BasedOn);
            baseMapName = customMap.BasedOn;
          }
          if((Core.currentCustomMap != null) && (Core.currentCustomMap.BasedOn == baseMapName)) {
            customMap = Core.currentCustomMap;
            Log.M?.TWL(0, "detected custom map loading " + customMap.Id+" based on "+ customMap.BasedOn);
          }
          VersionManifestEntry mapManifest = __instance.dataManager.ResourceLocator.EntryByID(baseMapName, BattleTechResourceType.AssetBundle);
          VersionManifestEntry brifingManifest = __instance.dataManager.ResourceLocator.EntryByID(__instance.request.Interstitial, BattleTechResourceType.AssetBundle);
          VersionManifestEntry customMapManifest = null;
          if (customMap != null) {
            customMapManifest = __instance.dataManager.ResourceLocator.EntryByID(customMap.Id, BattleTechResourceType.Prefab);
          }
          if (mapManifest != null || mapManifest != null || customMapManifest != null) {
            //LoadRequest loadRequest = __instance.dataManager.CreateLoadRequest(new Action<LoadRequest>(__instance.BundlesLoaded), true);
            DataManager.InjectedDependencyLoadRequest dependencyLoad = new DataManager.InjectedDependencyLoadRequest(__instance.dataManager);
            if (mapManifest != null) {
              dependencyLoad.RequestResource(BattleTechResourceType.AssetBundle, mapManifest.Id);
              //loadRequest.AddBlindLoadRequest(BattleTechResourceType.AssetBundle, mapManifest.Id);
            }
            if (brifingManifest != null) {
              dependencyLoad.RequestResource(BattleTechResourceType.AssetBundle, brifingManifest.Id);
              //loadRequest.AddBlindLoadRequest(BattleTechResourceType.AssetBundle, brifingManifest.Id);
            }
            if (customMapManifest != null) {
              dependencyLoad.RequestResource(BattleTechResourceType.Prefab, customMapManifest.Id);
              //loadRequest.AddBlindLoadRequest(BattleTechResourceType.Prefab, customMapManifest.Id);
            }
            dependencyLoad.RegisterLoadCompleteCallback(new Action(__instance.BundlesLoadedCustom));
            __instance.dataManager.InjectDependencyLoader(dependencyLoad, 1000U);
            //loadRequest.ProcessRequests();
          } else
            __instance.StartCoroutine(__instance.AttemptLoadSceneCustom(true));
        }
        return false;
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
        return true;
      }
    }
  }
}