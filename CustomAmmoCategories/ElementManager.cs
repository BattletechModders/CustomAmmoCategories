/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech.Rendering;
using BattleTech.Rendering.UI;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace CustAmmoCategoriesPatches {
  public static class ElementManager_Uniforms {
    internal static readonly int _BT_Temp01 = Shader.PropertyToID(nameof(_BT_Temp01));
    internal static readonly int _BT_ElementUI = Shader.PropertyToID(nameof(_BT_ElementUI));
  }
  [HarmonyPatch(typeof(ElementManager))]
  [HarmonyPatch("RefreshCommandBufferInt")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ElementManager_RefreshCommandBufferInt {
    private static Color32 clearColor = new Color32((byte)0, (byte)0, (byte)0, (byte)0);
    public static void Clear() {
    }
    public static void AddScorch(Vector3 position, Vector3 forward, Vector3 scale) {
    }
    public static void AddBlood(Vector3 position, Vector3 forward, Vector3 scale) {
    }
    public static void Prefix(ref bool __runOriginal, ElementManager __instance) {
      __runOriginal = false;
      Shader.SetGlobalTexture(ElementManager_Uniforms._BT_ElementUI, (Texture)__instance.gameUIRT);
      __instance._uiCommandBuffer.Clear();
      __instance._uiCommandBuffer.GetTemporaryRT(ElementManager_Uniforms._BT_Temp01, -1, -1, 24, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      __instance._uiCommandBuffer.SetRenderTarget((RenderTargetIdentifier)ElementManager_Uniforms._BT_Temp01);
      __instance._uiCommandBuffer.ClearRenderTarget(true, true, (Color)clearColor, 1f);
      for (int index1 = 0; index1 < __instance.creepContainer.Count; ++index1) {
        UICreep uiCreep = __instance.creepContainer[index1];
        for (int index2 = 0; index2 < uiCreep.renderCache.Count; ++index2) {
          UICreep.RenderCacheObject renderCacheObject = uiCreep.renderCache[index2];
          if (!((Object)renderCacheObject.renderer == (Object)null) && !((Object)renderCacheObject.renderer.sharedMaterial == (Object)null) && !renderCacheObject.renderer.sharedMaterial.IsKeywordEnabled("_STIPPLE_FADE")) {
            bool flag = (Object)renderCacheObject.renderer != (Object)null;
            if (!uiCreep.isBuilding ? ((flag ? 1 : 0) & (!renderCacheObject.renderer.gameObject.activeInHierarchy ? 0 : (renderCacheObject.renderer.enabled ? 1 : 0))) != 0 : flag & uiCreep.isSelected) {
              if (renderCacheObject.material == null) {
                if (renderCacheObject.renderer.sharedMaterial == null) {
                  if (__instance.log100Count < 100) {
                    Log.Combat?.TWL(0, "!!!WARNING!!! " + renderCacheObject.renderer.gameObject.name + " has no material");
                    ++__instance.log100Count;
                    renderCacheObject.renderer.enabled = false;
                  }
                } else {
                  renderCacheObject.material = renderCacheObject.renderer.sharedMaterial;
                }
                continue;
              }
              for (int submeshIndex = 0; submeshIndex < renderCacheObject.numSubmesh; ++submeshIndex) {
                __instance._uiCommandBuffer.DrawRenderer(renderCacheObject.renderer, renderCacheObject.material, submeshIndex, renderCacheObject.pass);
              }
            }
          }
        }
      }
      __instance._uiCommandBuffer.SetRenderTarget((RenderTargetIdentifier)((Texture)__instance.gameUIRT));
      __instance._uiCommandBuffer.ClearRenderTarget(true, true, (Color)clearColor, 1f);
      for (int index = 0; index < __instance.sweepDecalContainer.Count; ++index) {
        BTUIDecal btuiDecal = __instance.sweepDecalContainer[index];
        if ((Object)btuiDecal.DecalMaterial == (Object)null) {
          if (__instance.log100Count < 100) {
            Debug.LogError((object)string.Format("Null material found for sweep: {0}", (object)btuiDecal.name), (Object)btuiDecal.gameObject);
            ++__instance.log100Count;
          }
        } else
          __instance._uiCommandBuffer.DrawMesh(BTDecal.DecalMesh.DecalMeshFull, btuiDecal.transform.localToWorldMatrix, btuiDecal.DecalMaterial, 0, 0, btuiDecal.DecalPropertyBlock);
      }
      for (int index = 0; index < __instance.bleepContainer.Count; ++index) {
        UIBleep uiBleep = __instance.bleepContainer[index];
        if (uiBleep != null && uiBleep.bleepRenderer != null && uiBleep.bleepMaterial != null)
          __instance._uiCommandBuffer.DrawRenderer(uiBleep.bleepRenderer, uiBleep.bleepMaterial, 0, -1);
      }
      int count1 = 0;
      int count2 = 0;
      int count3 = 0;
      int count4 = 0;
      int count5 = 0;
      int count6 = 0;
      int count7 = 0;
      for (int index = 0; index < __instance.dotContainer.Count; ++index) {
        UIMovementDot uiMovementDot = __instance.dotContainer[index];
        Transform transform = uiMovementDot.transform;
        Matrix4x4 matrix4x4 = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        if (uiMovementDot.type == MovementDotMgr.DotType.Normal) {
          if (count1 < __instance.normalTrs.Length) {
            __instance.normalTrs[count1] = matrix4x4;
            ++count1;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Water) {
          if (count2 < __instance.waterTrs.Length) {
            __instance.waterTrs[count2] = matrix4x4;
            ++count2;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Forest) {
          if (count3 < __instance.forestTrs.Length) {
            __instance.forestTrs[count3] = matrix4x4;
            ++count3;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Rough) {
          if (count4 < __instance.roughTrs.Length) {
            __instance.roughTrs[count4] = matrix4x4;
            ++count4;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Road) {
          if (count5 < __instance.roadTrs.Length) {
            __instance.roadTrs[count5] = matrix4x4;
            ++count5;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Special) {
          if (count6 < __instance.specialTrs.Length) {
            __instance.specialTrs[count6] = matrix4x4;
            ++count6;
          }
        } else if (uiMovementDot.type == MovementDotMgr.DotType.Dangerous) {
          if (count7 < __instance.dangerousTrs.Length) {
            __instance.dangerousTrs[count7] = matrix4x4;
            ++count7;
          }
        }
      }
      if (count1 > 0) {
        Material material = UIMovementDot.GetMaterial(0);
        Mesh mesh = UIMovementDot.GetMesh(0);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.normalTrs, count1);
      }
      if (count3 > 0) {
        Material material = UIMovementDot.GetMaterial(1);
        Mesh mesh = UIMovementDot.GetMesh(1);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.forestTrs, count3);
      }
      if (count2 > 0) {
        Material material = UIMovementDot.GetMaterial(2);
        Mesh mesh = UIMovementDot.GetMesh(2);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.waterTrs, count2);
      }
      if (count4 > 0) {
        Material material = UIMovementDot.GetMaterial(3);
        Mesh mesh = UIMovementDot.GetMesh(3);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.roughTrs, count4);
      }
      if (count5 > 0) {
        Material material = UIMovementDot.GetMaterial(4);
        Mesh mesh = UIMovementDot.GetMesh(4);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.roadTrs, count5);
      }
      if (count6 > 0) {
        Material material = UIMovementDot.GetMaterial(5);
        Mesh mesh = UIMovementDot.GetMesh(5);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.specialTrs, count6);
      }
      if (count7 > 0) {
        Material material = UIMovementDot.GetMaterial(6);
        Mesh mesh = UIMovementDot.GetMesh(6);
        if ((Object)material != (Object)null && (Object)mesh != (Object)null)
          __instance._uiCommandBuffer.DrawMeshInstanced(mesh, 0, material, 0, __instance.dangerousTrs, count7);
      }
      for (int index = 0; index < __instance.sweepContainer.Count; ++index) {
        UISweep uiSweep = __instance.sweepContainer[index];
        if (uiSweep.sweepRenderer.enabled)
          __instance._uiCommandBuffer.DrawRenderer(uiSweep.sweepRenderer, uiSweep.sweepRenderer.sharedMaterial, 0, -1);
      }
      __instance._uiCommandBuffer.Blit((RenderTargetIdentifier)ElementManager_Uniforms._BT_Temp01, (RenderTargetIdentifier)((Texture)__instance.gameUIRT), __instance.sobelMaterial, 0);
    }
  }

}