using BattleTech;
using BattleTech.Data;
using CustomComponents;
using HarmonyLib;
using System;

namespace CustomUnits {
  [CustomComponent("AnimatorReplacer")]
  public class AnimatorReplacer :SimpleCustomComponent {
    public string AnimationSource { get; set; } = string.Empty;
  }
  [HarmonyPatch(typeof(MechComponentDef))]
  [HarmonyPatch("DependenciesLoaded")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(uint) })]
  public static class MechComponentDef_DependenciesLoaded {
    public static void Postfix(MechComponentDef __instance, uint loadWeight, ref bool __result) {
      try {
        if(__result == false) { return; }
        if(loadWeight <= 10U) { return; }
        AnimatorReplacer animReplacer = __instance.GetComponent<AnimatorReplacer>();
        if(animReplacer == null) { return; }
        Log.Combat?.TWL(0, "MechComponentDef.DependenciesLoaded " + __instance.Description.Id);
        if(__instance.dataManager.ResourceEntryExists(BattleTechResourceType.Prefab, animReplacer.AnimationSource) == false) {
          Log.Combat?.WL(1, $"resource not exists in manifest:{animReplacer.AnimationSource}");
          return;
        }
        if(__instance.dataManager.Exists(BattleTechResourceType.Prefab, animReplacer.AnimationSource) == false) {
          Log.Combat?.WL(1, $"resource not exists in datamanager:{animReplacer.AnimationSource}");
          __result = false;
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UnityGameInstance.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(MechComponentDef))]
  [HarmonyPatch("GatherDependencies")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DataManager), typeof(DataManager.DependencyLoadRequest), typeof(uint) })]
  public static class MechComponentDef_GatherDependencies {
    public static void Postfix(MechComponentDef __instance, DataManager dataManager, DataManager.DependencyLoadRequest dependencyLoad, uint activeRequestWeight) {
      try {
        if(activeRequestWeight <= 10U) { return; }
        AnimatorReplacer animReplacer = __instance.GetComponent<AnimatorReplacer>();
        if(animReplacer == null) { return; }
        Log.Combat?.TWL(0, "MechComponentDef.GatherDependencies " + __instance.Description.Id);
        if(__instance.dataManager.ResourceEntryExists(BattleTechResourceType.Prefab, animReplacer.AnimationSource) == false) {
          Log.Combat?.WL(1, $"resource not exists in manifest:{animReplacer.AnimationSource}");
          return;
        }
        if(__instance.dataManager.Exists(BattleTechResourceType.Prefab, animReplacer.AnimationSource) == false) {
          Log.Combat?.WL(1, $"request resource:{animReplacer.AnimationSource}");
          dependencyLoad.RequestResource(BattleTechResourceType.Prefab, animReplacer.AnimationSource);
        }
      } catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        UnityGameInstance.logger.LogException(e);
      }
    }
  }

}