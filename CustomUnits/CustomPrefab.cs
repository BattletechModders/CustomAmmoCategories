/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using BattleTech.Data;
using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUnits {
  public class CustomShaderReplacer: MonoBehaviour {

  }
  public class CustomShaderSource {
    public string prefab;
    public string material;
    public string direct;
  }
  public class CustomPrefabDef {
    public string Id { get; set; }
    public Dictionary<string, CustomShaderSource> MaterialShaderReplacement { get; set; }
    public Dictionary<string, CustomShaderSource> MaterialReplacement { get; set; }
    public CustomPrefabDef() {
      MaterialShaderReplacement = new Dictionary<string, CustomShaderSource>();
      MaterialReplacement = new Dictionary<string, CustomShaderSource>();
    }
  }
  public static class CustomPrefabHelper {
    private static Dictionary<string, CustomPrefabDef> customPrefabs = new Dictionary<string, CustomPrefabDef>();
    public static void Register(this CustomPrefabDef def) {
      if (customPrefabs.ContainsKey(def.Id)) { customPrefabs[def.Id] = def; } else { customPrefabs.Add(def.Id, def); }
    }
    public static bool TryGet(string id, out CustomPrefabDef def) {
      if (customPrefabs.TryGetValue(id, out def)) { return true; }
      def = null;
      return false;
    }
    public static void RequestResource(DataManager.DependencyLoadRequest __instance, string id) {
      if (customPrefabs.TryGetValue(id, out CustomPrefabDef def)) {
        foreach(var shaderRep in def.MaterialShaderReplacement) {
          if (string.IsNullOrEmpty(shaderRep.Value.prefab) == false) {
            __instance.RequestResource(BattleTechResourceType.Prefab, shaderRep.Value.prefab);
          }
        }
        foreach (var matRep in def.MaterialReplacement) {
          if (string.IsNullOrEmpty(matRep.Value.prefab) == false) {
            __instance.RequestResource(BattleTechResourceType.Prefab, matRep.Value.prefab);
          }
        }
      }
    }
    public static bool Exists(DataManager dataManager, string id) {
      if (customPrefabs.TryGetValue(id, out CustomPrefabDef def)) {
        foreach (var shaderRep in def.MaterialShaderReplacement) {
          if (string.IsNullOrEmpty(shaderRep.Value.prefab) == false) {
            if (dataManager.Exists(BattleTechResourceType.Prefab, shaderRep.Value.prefab) == false) { return false; }
          }
        }
        foreach (var matRep in def.MaterialReplacement) {
          if (string.IsNullOrEmpty(matRep.Value.prefab) == false) {
            if (dataManager.Exists(BattleTechResourceType.Prefab, matRep.Value.prefab) == false) { return false; }
          }
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(DataManager))]
  [HarmonyPatch("PooledInstantiate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(BattleTechResourceType), typeof(Vector3?), typeof(Quaternion?), typeof(Transform) })]
  public static class DataManager_PooledInstantiate {
    public static Shader SearchForShader(this GameObject go, string materialName) {
      Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
      foreach(Renderer r in renderers) {
        foreach(Material m in r.sharedMaterials) {
          if (m.name.StartsWith(materialName) == false) { continue; }
          return m.shader;
        }
      }
      return null;
    }
    public static Material SearchForMaterial(this GameObject go, string materialName) {
      Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
      foreach (Renderer r in renderers) {
        foreach (Material m in r.sharedMaterials) {
          if (m.name.StartsWith(materialName) == false) { continue; }
          return m;
        }
      }
      return null;
    }
    public static void ReplaceShaders(DataManager __instance, string id, ref GameObject __result) {
      try {
        CustomShaderReplacer replacer = __result.GetComponent<CustomShaderReplacer>();
        if (replacer != null) { return; }
        __result.AddComponent<CustomShaderReplacer>();
        Log.TWL(0, "CustomPrefabHelper.Searching " + id);
        Renderer[] renderers = null;
        if (CustomPrefabHelper.TryGet(id, out CustomPrefabDef def)) {
          Log.WL(1, "found");
          renderers = __result.GetComponentsInChildren<Renderer>(true);
          Dictionary<string, GameObject> shaderSources = new Dictionary<string, GameObject>();
          Dictionary<string, GameObject> materialSources = new Dictionary<string, GameObject>();
          foreach (Renderer r in renderers) {
            Material[] sharedMaterials = r.sharedMaterials;
            for (int matIndex = 0; matIndex < sharedMaterials.Length; ++matIndex) {
              if (sharedMaterials[matIndex] == null) { continue; }
              Log.WL(2, "material:" + sharedMaterials[matIndex].name);
              foreach (var mn in def.MaterialShaderReplacement) {
                if (sharedMaterials[matIndex].name.StartsWith(mn.Key)) {
                  if (string.IsNullOrEmpty(mn.Value.prefab) == false) {
                    if (shaderSources.TryGetValue(mn.Value.prefab, out GameObject shaderSource) == false) {
                      shaderSource = __instance.PooledInstantiate(mn.Value.prefab, BattleTechResourceType.Prefab);
                      shaderSources.Add(mn.Value.prefab, shaderSource);
                    }
                    if (shaderSource == null) { continue; }
                    Shader sh = shaderSource.SearchForShader(mn.Value.material);
                    if (sh != null) { sharedMaterials[matIndex].shader = sh; }
                  } else if (string.IsNullOrEmpty(mn.Value.direct) == false) {
                    Shader shader = Shader.Find(mn.Value.direct);
                    if (shader != null) { sharedMaterials[matIndex].shader = shader; }
                  }
                }
              }
              foreach (var mn in def.MaterialReplacement) {
                if (sharedMaterials[matIndex].name.StartsWith(mn.Key)) {
                  Log.WL(3, "replacement:" + mn.Value.prefab + "." + mn.Value.material + " direct:" + mn.Value.direct);
                  if (string.IsNullOrEmpty(mn.Value.prefab) == false) {
                    if (materialSources.TryGetValue(mn.Value.prefab, out GameObject materialSource) == false) {
                      materialSource = __instance.PooledInstantiate(mn.Value.prefab, BattleTechResourceType.Prefab);
                      materialSources.Add(mn.Value.prefab, materialSource);
                    }
                    if (materialSource == null) { continue; }
                    Material mat = materialSource.SearchForMaterial(mn.Value.material);
                    if (mat != null) { sharedMaterials[matIndex] = mat; }
                  } else if (string.IsNullOrEmpty(mn.Value.direct) == false) {
                    Material[] mats = Material.FindObjectsOfType<Material>();
                    foreach (Material mat in mats) {
                      if (mat.name.StartsWith(mn.Value.direct)) {
                        sharedMaterials[matIndex] = mat;
                        Log.WL(4, "target material found " + r.name + ".sharedMaterials[" + matIndex + "] = " + sharedMaterials[matIndex].name + "/" + mat.name);
                        break;
                      }
                    }
                  }
                }
              }

            }
            r.sharedMaterials = sharedMaterials;
          }
          foreach (var shaderSource in shaderSources) {
            __instance.PoolGameObject(shaderSource.Key, shaderSource.Value);
          }
          foreach (var matSource in materialSources) {
            __instance.PoolGameObject(matSource.Key, matSource.Value);
          }
        }
        renderers = __result.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers) {
          Material[] sharedMaterials = r.sharedMaterials;
          for (int matIndex = 0; matIndex < sharedMaterials.Length; ++matIndex) {
            if (sharedMaterials[matIndex] == null) { continue; }
            if(Core.Settings.forceToBuildinShaders.TryGetValue(sharedMaterials[matIndex].shader.name, out var shaderToReplace)) {
              if(sharedMaterials[matIndex].shader.GetInstanceID() != shaderToReplace.GetInstanceID()) {
                Log.WL(1,$"replacing shader {sharedMaterials[matIndex].shader.name}:{sharedMaterials[matIndex].shader.GetInstanceID()} -> {shaderToReplace.name}:{shaderToReplace.GetInstanceID()}");
                sharedMaterials[matIndex].shader = shaderToReplace;
              }
            }
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public static void Postfix(DataManager __instance, string id, BattleTechResourceType resourceType, ref GameObject __result) {
      try {
        if (resourceType != BattleTechResourceType.Prefab) { return; }
        if (__result == null) { return; }
        ReplaceShaders(__instance, id, ref __result);
        //VehicleRepresentation vehicleRep = __result.GetComponent<VehicleRepresentation>();
        //if (vehicleRep != null) {  }
      } catch (Exception e) { Log.TWL(0, e.ToString(), true); }
    }
  }
}