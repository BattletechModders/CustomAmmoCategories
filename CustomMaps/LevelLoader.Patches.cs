using BattleTech;
using Harmony;
using HBS;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomMaps.Patches {
  [HarmonyPatch(typeof(LevelLoader))]
  [HarmonyPatch("Start")]
  [HarmonyPatch(MethodType.Normal)]
  public static class LevelLoader_Start {
    private static IEnumerator LoadCustom(this LevelLoader __instance) {
      LevelLoader levelLoader = __instance;
      CustomMapDef customMap = Core.findCustomMap(LevelLoader.LoaderTarget);
      if((customMap == null)&&(Core.currentCustomMap != null)) {
        if(Core.currentCustomMap.BasedOn == LevelLoader.LoaderTarget) {
          customMap = Core.currentCustomMap;
        }
      }
      levelLoader.EnablePhysics(false);
      levelLoader.EnableLogging(false);
      int targetFPS = Application.targetFrameRate;
      ThreadPriority bgLoadPriority = Application.backgroundLoadingPriority;
      Application.backgroundLoadingPriority = ThreadPriority.High;
      Application.targetFrameRate = 15;
      Stopwatch levelLoadTimer = Stopwatch.StartNew();
      bool useInterstitial = !string.IsNullOrEmpty(LevelLoader.interstitialTarget);
      if (!useInterstitial) {
        LevelLoader.interstitialTarget = PlayerPrefs.GetString("LoadingInterstitialName", (string)null);
        useInterstitial = !string.IsNullOrEmpty(LevelLoader.interstitialTarget);
      }
      PlayerPrefs.DeleteKey("LoadingInterstitialName");
      if (SceneManager.GetActiveScene().name != "Empty" && LevelLoader.loadTarget != "Empty" && LevelLoader.interstitialTarget != "Empty") {
        yield return (object)levelLoader.YieldForLoadOperation(SceneManager.LoadSceneAsync("Empty"));
        if (SceneManager.GetSceneByName(LevelLoader.activeScene).isLoaded) {
          LevelLoader.logger.Log((object)("Unloding Level " + LevelLoader.activeScene));
          yield return (object)SceneManager.UnloadSceneAsync(LevelLoader.activeScene);
          LevelLoader.activeScene = "";
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Empty"));
      }
      yield return (object)Resources.UnloadUnusedAssets();
      yield return (object)null;
      if (useInterstitial) {
        LevelLoader.logger.Log((object)("Loading level " + LevelLoader.interstitialTarget));
        yield return (object)levelLoader.YieldForLoadOperation(SceneManager.LoadSceneAsync(LevelLoader.interstitialTarget, LoadSceneMode.Additive));
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(LevelLoader.interstitialTarget));
        levelLoader.CleanupClearCameras();
        LevelLoader.loaderState = LevelLoader.LoadState.InterstitialActive;
        yield return (object)null;
      }
      LevelLoader.logger.Log((object)("Loading level " + LevelLoader.LoaderTarget));
      Log.M?.TWL(0, "Loading level "+ LevelLoader.LoaderTarget+" "+(customMap == null?"not custom":("custom based on "+ customMap.BasedOn)));
      yield return (object)levelLoader.YieldForLoadOperation(SceneManager.LoadSceneAsync(customMap == null?LevelLoader.LoaderTarget:customMap.BasedOn, LoadSceneMode.Single));
      if (SceneManager.GetSceneByName("Empty").isLoaded) {
        LevelLoader.logger.Log((object)"Unloading Empty");
        yield return (object)SceneManager.UnloadSceneAsync("Empty");
        yield return (object)null;
      }
      SceneManager.SetActiveScene(SceneManager.GetSceneByName(customMap == null ? LevelLoader.LoaderTarget : customMap.BasedOn));
      levelLoadTimer.Stop();
      LevelLoader.logger.Log((object)string.Format("Level {0} took {1}s to load", (object)LevelLoader.LoaderTarget, (object)levelLoadTimer.ElapsedSeconds));
      Log.M?.TWL(0, string.Format("Level {0} took {1}s to load", (object)LevelLoader.LoaderTarget, (object)levelLoadTimer.ElapsedSeconds));
      if (customMap != null) {
        Log.M?.TWL(0, "ActiveTerrain:" + Terrain.activeTerrain.name+" "+ Terrain.activeTerrain.terrainData.heightmapWidth+"x"+ Terrain.activeTerrain.terrainData.heightmapHeight);
        GameObject customTerrainGO = UnityGameInstance.BattleTechGame.DataManager.PooledInstantiate(customMap.Id, BattleTechResourceType.Prefab);
        if(customTerrainGO != null) {
          Log.M?.WL(1, "custom terrain spawned success");
          Terrain customTerrain = customTerrainGO.GetComponentInChildren<Terrain>(true);
          if (customTerrain != null) {
            Log.M?.WL(1, "custom terrain found "+customTerrain.name+" "+customTerrain.terrainData.heightmapWidth+"x"+customTerrain.terrainData.heightmapHeight);
            //Terrain.activeTerrain.gameObject.SetActive(false);
            //Terrain.activeTerrain.
            Terrain.activeTerrain.terrainData = customTerrain.terrainData;
            Terrain.activeTerrain.gameObject.SetActive(false);
            Terrain.activeTerrain.gameObject.SetActive(true);
            Transform[] customEnv = customTerrainGO.GetComponentsInChildren<Transform>();
            GameObject GAME = GameObject.Find("GAME");
            if (GAME != null) { GAME.SetActive(false); }
            foreach(Transform env in customEnv) {
              if (env.name == "ENVIRONMENT") {
                env.parent = Terrain.activeTerrain.gameObject.transform;
                break;
              }
            }
            //Terrain.activeTerrain.terrainData.SetHeights(0, 0, customTerrain.terrainData.GetHeights(0,0,customTerrain.terrainData.heightmapWidth,customTerrain.terrainData.heightmapHeight));
            //Terrain.activeTerrain.terrainData.treePrototypes = customTerrain.terrainData.treePrototypes;
            //Terrain.activeTerrain.terrainData.treeInstances = customTerrain.terrainData.treeInstances;
            //Terrain.activeTerrain.ma
          } else {
            Log.M?.WL(1, "custom terrain not found");
          }
          GameObject.DestroyImmediate(customTerrainGO);
        } else {
          Log.M?.WL(1, "custom terrain spawn fail");
        }
      }
      UnityGameInstance.BattleTechGame.MessageCenter.PublishMessage((MessageCenterMessage)new LevelLoadCompleteMessage(LevelLoader.LoaderTarget, LevelLoader.interstitialTarget));
      while (LevelLoader.loaderState == LevelLoader.LoadState.InterstitialActive)
        yield return (object)null;
      if (useInterstitial) {
        if (SceneManager.GetSceneByName(LevelLoader.interstitialTarget).isLoaded)
          yield return (object)SceneManager.UnloadSceneAsync(LevelLoader.interstitialTarget);
        if (LevelLoader.interstitialComplete != null) {
          LevelLoader.interstitialComplete();
          LevelLoader.interstitialComplete = (Action)null;
        }
      }
      if (SceneManager.GetSceneByName("Loading").isLoaded)
        yield return (object)SceneManager.UnloadSceneAsync("Loading");
      yield return (object)Resources.UnloadUnusedAssets();
      yield return (object)null;
      levelLoader.EnablePhysics(true);
      levelLoader.EnableLogging(true);
      if (!useInterstitial)
        levelLoader.CleanupClearCameras();
      Application.backgroundLoadingPriority = bgLoadPriority;
      Application.targetFrameRate = targetFPS;
      LevelLoader.loaderState = LevelLoader.LoadState.Loaded;
      UnityEngine.Object.Destroy((UnityEngine.Object)levelLoader.gameObject);
    }
    public static bool Prefix(LevelLoader __instance) {
      try {
        UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)__instance);
        UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)__instance.gameObject);
        __instance.StartCoroutine(__instance.LoadCustom());
        return false;
      } catch (Exception e) {
        Log.M_err?.TWL(0, e.ToString());
        return true;
      }
    }
  }

}