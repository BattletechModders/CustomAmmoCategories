﻿using UnityEngine;
using BattleTech;
using BattleTech.Rendering;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using CustAmmoCategories;
using BattleTech.Data;
using System.Reflection;
using System.Linq;
using BattleTech.Rendering.UI;
using Object = UnityEngine.Object;

namespace CustomUnits {
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("GenerateCache")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_GenerateCache {
    public static bool Prefix(MechMeshMerge __instance) { return false; }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("OnDestroy")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_OnDestroy {
    public static bool Prefix(MechMeshMerge __instance) { return false; }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("BuildCombinedMesh")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_BuildCombinedMesh {
    public static bool Prefix(MechMeshMerge __instance) { return false; }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("RefreshCombinedMesh")]
  [HarmonyPatch(MethodType.Normal)]
  public static class MechMeshMerge_RefreshCombinedMesh {
    public static bool Prefix(MechMeshMerge __instance) { return false; }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("OnEnable")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_OnEnable {
    public static bool Prefix(MechMeshMerge __instance) { return false; }
  }
  public class CustomMechMeshMerge : MonoBehaviour {
    private Dictionary<SkinnedMeshRenderer, CustomMechMeshMerge.CombineInfo> meshBoneDict;
    private List<Transform> boneList;
    private List<Matrix4x4> bindPoses;
    private int materialCount;
    private Material[] mechMaterial = new Material[2];
    private Material[] explodeMechMaterial = new Material[2];
    private Material explodeWeaponMaterial;
    public List<Mesh> meshList;
    private bool SingleMesh = false;
    private bool? NeedRebuildDamaged { get; set; } = null;
    private SkinnedMeshRenderer mainSkinRenderer0 { get; set; } = null;
    private SkinnedMeshRenderer mainSkinRenderer1 { get; set; } = null;
    private Mesh combinedMesh0 { get; set; } = null;
    private Mesh combinedMesh1 { get; set; } = null;
    public GameObject newMeshRoot0 { get; set; } = null;
    public GameObject newMeshRoot1 { get; set; } = null;
    public CustomMechRepresentation parentRepresentation;
    private GameObject visibleObject;
    private float bottom = -1f;
    private float size = -1f;
    private static List<Vector3> combineVerts = new List<Vector3>();
    private static List<Vector3> combineNormals = new List<Vector3>();
    private static List<Vector4> combineTangents = new List<Vector4>();
    private static List<Vector2> combineUVs = new List<Vector2>();
    private static List<BoneWeight> combineWeights = new List<BoneWeight>();
    private static List<int> combineIndicies = new List<int>();
    private static List<int> combineIndicies1 = new List<int>();
    private static List<Color> combineColors = new List<Color>();
    private List<SkinnedMeshRenderer> _childrenRenderers;
    private UICreep uiCreep;
    private PropertyBlockManager blockManager;
    private static Texture2D _damageAlbedo;
    private static Texture2D _damageNormal;

    private List<SkinnedMeshRenderer> childrenRenderers {
      get {
        if (_childrenRenderers != null) { return this._childrenRenderers; }
        this._childrenRenderers = new List<SkinnedMeshRenderer>();
        HashSet<SkinnedMeshRenderer> nomerge = new HashSet<SkinnedMeshRenderer>();
        Transform[] trs = this.visibleObject.GetComponentsInChildren<Transform>(true);
        foreach(Transform tr in trs) {
          if (tr.name.StartsWith("nomerge") == false) { continue; };
          if (tr.transform.parent == null) { continue; }
          SkinnedMeshRenderer nomerge_renderer = tr.transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
          if (nomerge_renderer == null) { continue; }
          nomerge.Add(nomerge_renderer);
        }
        SkinnedMeshRenderer[] skinnedRenderers = this.visibleObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        Log.TWL(0, "CustomMechMeshMerge.childrenRenderers");
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers) {
          if (renderer.gameObject.name.Contains("camoholder")) { continue; }
          if (nomerge.Contains(renderer)) { continue; };
          if (renderer.gameObject.GetComponent<Rigidbody>() != null) { continue; }
          Log.WL(0, renderer.gameObject.name);
          this._childrenRenderers.Add(renderer);
        }
        return this._childrenRenderers;
      }
    }

    public static Texture2D damageAlbedo {
      get {
        if ((Object)CustomMechMeshMerge._damageAlbedo == (Object)null)
          CustomMechMeshMerge._damageAlbedo = Resources.Load<Texture2D>("Textures/mech_tiling_dmg_alb");
        return CustomMechMeshMerge._damageAlbedo;
      }
    }

    public static Texture2D damageNormal {
      get {
        if ((Object)CustomMechMeshMerge._damageNormal == (Object)null)
          CustomMechMeshMerge._damageNormal = Resources.Load<Texture2D>("Textures/mech_tiling_dmg_nrm");
        return CustomMechMeshMerge._damageNormal;
      }
    }

    private void GenerateCache() {
      if (this.visibleObject == null) { return; }
      this.meshBoneDict = new Dictionary<SkinnedMeshRenderer, CustomMechMeshMerge.CombineInfo>();
      this.boneList = new List<Transform>();
      Matrix4x4 worldToLocalMatrix = this.visibleObject.transform.worldToLocalMatrix;
      this.meshList = new List<Mesh>();
      this.bindPoses = new List<Matrix4x4>();
      for (int index1 = 0; index1 < this.childrenRenderers.Count; ++index1) {
        if (this.childrenRenderers[index1].sharedMaterials.Length <= 1 && !((Object)this.childrenRenderers[index1].sharedMesh == (Object)null)) {
          Material sharedMaterial = this.childrenRenderers[index1].sharedMaterial;
          if (!((Object)sharedMaterial == (Object)null) && !sharedMaterial.name.Contains("weapons")) {
            int matIndex = sharedMaterial.name.Contains("base") ? 0 : 1;
            this.materialCount = Mathf.Max(this.materialCount, matIndex + 1);
            if (!((Object)this.childrenRenderers[index1].transform.parent.GetComponent<Rigidbody>() != (Object)null)) {
              if ((Object)this.mechMaterial[matIndex] == (Object)null) {
                this.mechMaterial[matIndex] = Object.Instantiate<Material>(sharedMaterial);
                this.mechMaterial[matIndex].SetTexture(MechMeshMerge.Uniforms._DamageAlbedoMap, (Texture)MechMeshMerge.damageAlbedo);
                this.mechMaterial[matIndex].SetTextureScale("_DamageAlbedoMap", new Vector2(6f, 6f));
                this.mechMaterial[matIndex].SetTexture(MechMeshMerge.Uniforms._DamageNormalMap, (Texture)MechMeshMerge.damageNormal);
              }
              this.childrenRenderers[index1].sharedMaterial = this.mechMaterial[matIndex];
              Transform transform = this.childrenRenderers[index1].rootBone.gameObject.transform;
              int index2 = this.boneList.IndexOf(transform);
              if (index2 == -1) {
                this.boneList.Add(transform);
                index2 = this.boneList.IndexOf(transform);
                this.bindPoses.Add(this.childrenRenderers[index1].sharedMesh.bindposes[0] * this.childrenRenderers[index1].transform.worldToLocalMatrix);
              }
              CustomMechMeshMerge.CombineInfo combineInfo = new CustomMechMeshMerge.CombineInfo(this.childrenRenderers[index1].sharedMesh, index2, this.childrenRenderers[index1].transform.localToWorldMatrix, matIndex);
              this.meshList.Add(combineInfo.newMesh);
              this.meshBoneDict.Add(this.childrenRenderers[index1], combineInfo);
              this.childrenRenderers[index1].enabled = false;
            }
          }
        }
      }
      for (int index = 0; index < this.mechMaterial.Length; ++index) {
        if ((Object)this.mechMaterial[index] != (Object)null) {
          this.explodeMechMaterial[index] = Object.Instantiate<Material>(this.mechMaterial[index]);
          this.explodeMechMaterial[index].EnableKeyword("_DAMAGED");
          this.explodeMechMaterial[index].EnableKeyword("_DAMAGEDFULL");
        }
      }
      Transform[] trs = this.visibleObject.GetComponentsInChildren<Transform>(true);
      HashSet<Renderer> nomerge = new HashSet<Renderer>();
      foreach (Transform tr in trs) {
        if (tr.name.StartsWith("nomerge") == false) { continue; };
        if (tr.parent == null) { continue; }
        Renderer r = tr.parent.gameObject.GetComponent<Renderer>();
        if (r == null) { continue; }
        nomerge.Add(r);
      }
      foreach (MeshRenderer componentsInChild in this.visibleObject.GetComponentsInChildren<MeshRenderer>(true)) {
        if (componentsInChild.name.Contains("camoholder")) { continue; }
        if (nomerge.Contains(componentsInChild)) { continue; }
        Log.WL(0, "componentsInChild.name:" + componentsInChild.name);
        Material[] sharedMaterials = componentsInChild.sharedMaterials;
        Material[] materialArray = new Material[sharedMaterials.Length];
        for (int index1 = 0; index1 < materialArray.Length; ++index1) {
          if (sharedMaterials[index1].name.Contains("weapons")) {
            if (this.explodeWeaponMaterial == null) {
              this.explodeWeaponMaterial = Object.Instantiate<Material>(sharedMaterials[index1]);
              this.explodeWeaponMaterial.SetTexture(MechMeshMerge.Uniforms._DamageAlbedoMap, (Texture)MechMeshMerge.damageAlbedo);
              this.explodeWeaponMaterial.SetTextureScale("_DamageAlbedoMap", new Vector2(6f, 6f));
              this.explodeWeaponMaterial.SetTexture(MechMeshMerge.Uniforms._DamageNormalMap, (Texture)MechMeshMerge.damageNormal);
              this.explodeWeaponMaterial.EnableKeyword("_DAMAGED");
              this.explodeWeaponMaterial.EnableKeyword("_DAMAGEDFULL");
            }
            materialArray[index1] = this.explodeWeaponMaterial;
          } else {
            int index2 = sharedMaterials[index1].name.Contains("base") ? 0 : 1;
            materialArray[index1] = this.explodeMechMaterial[index2];
          }
        }
        componentsInChild.sharedMaterials = materialArray;
      }
    }
    private void BuildCombinedMeshClassic() {
      if (this.meshBoneDict == null || this.meshBoneDict.Count == 0)
        this.GenerateCache();
      int num1 = 0;
      CustomMechMeshMerge.combineVerts.Clear();
      CustomMechMeshMerge.combineNormals.Clear();
      CustomMechMeshMerge.combineTangents.Clear();
      CustomMechMeshMerge.combineUVs.Clear();
      CustomMechMeshMerge.combineWeights.Clear();
      CustomMechMeshMerge.combineIndicies.Clear();
      CustomMechMeshMerge.combineIndicies1.Clear();
      CustomMechMeshMerge.combineColors.Clear();
      int index1;
      for (index1 = 0; index1 < this.childrenRenderers.Count / 2; ++index1) {
        CustomMechMeshMerge.CombineInfo combineInfo;
        if (this.childrenRenderers[index1].gameObject.activeInHierarchy && this.meshBoneDict.TryGetValue(this.childrenRenderers[index1], out combineInfo)) {
          CustomMechMeshMerge.combineVerts.AddRange((IEnumerable<Vector3>)combineInfo.verts);
          CustomMechMeshMerge.combineNormals.AddRange((IEnumerable<Vector3>)combineInfo.normals);
          CustomMechMeshMerge.combineTangents.AddRange((IEnumerable<Vector4>)combineInfo.tangents);
          CustomMechMeshMerge.combineUVs.AddRange((IEnumerable<Vector2>)combineInfo.uvs);
          CustomMechMeshMerge.combineWeights.AddRange((IEnumerable<BoneWeight>)combineInfo.boneWeights);
          CustomMechMeshMerge.combineColors.AddRange((IEnumerable<Color>)combineInfo.colors);
          if (combineInfo.materialIndex == 0) {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies.Add(combineInfo.triangles[index2] + num1);
          } else {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies1.Add(combineInfo.triangles[index2] + num1);
          }
          num1 += combineInfo.newMesh.vertexCount;
        }
      }
      if ((Object)this.combinedMesh0 != (Object)null) {
        Object.DestroyImmediate((Object)this.combinedMesh0);
        this.combinedMesh0 = (Mesh)null;
      }
      if (CustomMechMeshMerge.combineVerts.Count > 0) {
        this.combinedMesh0 = new Mesh();
        this.combinedMesh0.subMeshCount = this.materialCount;
        this.combinedMesh0.SetVertices(CustomMechMeshMerge.combineVerts);
        this.combinedMesh0.SetTangents(CustomMechMeshMerge.combineTangents);
        this.combinedMesh0.SetUVs(0, CustomMechMeshMerge.combineUVs);
        this.combinedMesh0.boneWeights = CustomMechMeshMerge.combineWeights.ToArray();
        this.combinedMesh0.SetTriangles(CustomMechMeshMerge.combineIndicies, 0, true);
        if (CustomMechMeshMerge.combineIndicies1.Count > 0)
          this.combinedMesh0.SetTriangles(CustomMechMeshMerge.combineIndicies1, 1, true);
        this.combinedMesh0.SetNormals(CustomMechMeshMerge.combineNormals);
        this.combinedMesh0.SetColors(CustomMechMeshMerge.combineColors);
        this.combinedMesh0.bindposes = this.bindPoses.ToArray();
      }
      CustomMechMeshMerge.combineVerts.Clear();
      CustomMechMeshMerge.combineNormals.Clear();
      CustomMechMeshMerge.combineTangents.Clear();
      CustomMechMeshMerge.combineUVs.Clear();
      CustomMechMeshMerge.combineWeights.Clear();
      CustomMechMeshMerge.combineIndicies.Clear();
      CustomMechMeshMerge.combineIndicies1.Clear();
      CustomMechMeshMerge.combineColors.Clear();
      int num2 = 0;
      for (; index1 < this.childrenRenderers.Count; ++index1) {
        CustomMechMeshMerge.CombineInfo combineInfo;
        if (this.childrenRenderers[index1].gameObject.activeInHierarchy && this.meshBoneDict.TryGetValue(this.childrenRenderers[index1], out combineInfo)) {
          CustomMechMeshMerge.combineVerts.AddRange((IEnumerable<Vector3>)combineInfo.verts);
          CustomMechMeshMerge.combineNormals.AddRange((IEnumerable<Vector3>)combineInfo.normals);
          CustomMechMeshMerge.combineTangents.AddRange((IEnumerable<Vector4>)combineInfo.tangents);
          CustomMechMeshMerge.combineUVs.AddRange((IEnumerable<Vector2>)combineInfo.uvs);
          CustomMechMeshMerge.combineWeights.AddRange((IEnumerable<BoneWeight>)combineInfo.boneWeights);
          CustomMechMeshMerge.combineColors.AddRange((IEnumerable<Color>)combineInfo.colors);
          if (combineInfo.materialIndex == 0) {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies.Add(combineInfo.triangles[index2] + num2);
          } else {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies1.Add(combineInfo.triangles[index2] + num2);
          }
          num2 += combineInfo.newMesh.vertexCount;
        }
      }
      if ((Object)this.combinedMesh1 != (Object)null) {
        Object.DestroyImmediate((Object)this.combinedMesh1);
        this.combinedMesh1 = (Mesh)null;
      }
      if (CustomMechMeshMerge.combineVerts.Count != 0) {
        this.combinedMesh1 = new Mesh();
        this.combinedMesh1.subMeshCount = this.materialCount;
        this.combinedMesh1.SetVertices(CustomMechMeshMerge.combineVerts);
        this.combinedMesh1.SetTangents(CustomMechMeshMerge.combineTangents);
        this.combinedMesh1.SetUVs(0, CustomMechMeshMerge.combineUVs);
        this.combinedMesh1.boneWeights = CustomMechMeshMerge.combineWeights.ToArray();
        this.combinedMesh1.SetTriangles(CustomMechMeshMerge.combineIndicies, 0, true);
        if (CustomMechMeshMerge.combineIndicies1.Count > 0)
          this.combinedMesh1.SetTriangles(CustomMechMeshMerge.combineIndicies1, 1, true);
        this.combinedMesh1.SetNormals(CustomMechMeshMerge.combineNormals);
        this.combinedMesh1.SetColors(CustomMechMeshMerge.combineColors);
        this.combinedMesh1.bindposes = this.bindPoses.ToArray();
      }
      if ((double)this.bottom < 0.0 && (double)this.size < 0.0) {
        Bounds bounds = this.combinedMesh0.bounds;
        if ((Object)this.combinedMesh1 != (Object)null)
          bounds.Encapsulate(this.combinedMesh1.bounds);
        this.bottom = bounds.center.y - bounds.extents.y;
        this.size = bounds.size.y;
        foreach (KeyValuePair<SkinnedMeshRenderer, CustomMechMeshMerge.CombineInfo> keyValuePair in this.meshBoneDict)
          keyValuePair.Value.SetColorForHeight(this.bottom, this.size);
        this.combinedMesh0.GetVertices(CustomMechMeshMerge.combineVerts);
        this.combinedMesh0.GetColors(CustomMechMeshMerge.combineColors);
        for (int index2 = 0; index2 < CustomMechMeshMerge.combineVerts.Count; ++index2) {
          float num3 = (CustomMechMeshMerge.combineVerts[index2].y - this.bottom) / this.size;
          Color combineColor = CustomMechMeshMerge.combineColors[index2];
          combineColor.g = num3;
          CustomMechMeshMerge.combineColors[index2] = combineColor;
        }
        this.combinedMesh0.SetColors(CustomMechMeshMerge.combineColors);
        this.combinedMesh0.UploadMeshData(true);
        if ((Object)this.combinedMesh1 != (Object)null) {
          this.combinedMesh1.GetVertices(CustomMechMeshMerge.combineVerts);
          this.combinedMesh1.GetColors(CustomMechMeshMerge.combineColors);
          for (int index2 = 0; index2 < CustomMechMeshMerge.combineVerts.Count; ++index2) {
            float num3 = (CustomMechMeshMerge.combineVerts[index2].y - this.bottom) / this.size;
            Color combineColor = CustomMechMeshMerge.combineColors[index2];
            combineColor.g = num3;
            CustomMechMeshMerge.combineColors[index2] = combineColor;
          }
          this.combinedMesh1.SetColors(CustomMechMeshMerge.combineColors);
          this.combinedMesh1.UploadMeshData(true);
        }
      } else {
        this.combinedMesh0?.UploadMeshData(true);
        this.combinedMesh1?.UploadMeshData(true);
      }
      CustomMechMeshMerge.combineVerts.Clear();
      CustomMechMeshMerge.combineNormals.Clear();
      CustomMechMeshMerge.combineTangents.Clear();
      CustomMechMeshMerge.combineUVs.Clear();
      CustomMechMeshMerge.combineWeights.Clear();
      CustomMechMeshMerge.combineIndicies.Clear();
      CustomMechMeshMerge.combineIndicies1.Clear();
      CustomMechMeshMerge.combineColors.Clear();
    }

    private void BuildCombinedMeshSingle() {
      if (this.visibleObject == null) { return; }
      Log.TWL(0, "CustomMechMeshMerge.BuildCombinedMesh "+this.gameObject.transform.parent.name);
      if (this.meshBoneDict == null || this.meshBoneDict.Count == 0) { this.GenerateCache(); }
      int num1 = 0;
      Log.WL(1, "childrenRenderers:"+childrenRenderers.Count);
      foreach (SkinnedMeshRenderer rnd in childrenRenderers) {
        Log.WL(2, rnd.transform.name+":"+rnd.gameObject.activeInHierarchy);
      }
      Log.WL(1, "meshBoneDict:" + meshBoneDict.Count);
      foreach (var bone in meshBoneDict) {
        Log.WL(2, bone.Key.transform.name+" triangles:"+bone.Value.triangles.Length);
      }
      CustomMechMeshMerge.combineVerts.Clear();
      CustomMechMeshMerge.combineNormals.Clear();
      CustomMechMeshMerge.combineTangents.Clear();
      CustomMechMeshMerge.combineUVs.Clear();
      CustomMechMeshMerge.combineWeights.Clear();
      CustomMechMeshMerge.combineIndicies.Clear();
      CustomMechMeshMerge.combineIndicies1.Clear();
      CustomMechMeshMerge.combineColors.Clear();
      for (int index1 = 0; index1 < this.childrenRenderers.Count; ++index1) {
        CustomMechMeshMerge.CombineInfo combineInfo;
        if (this.childrenRenderers[index1].gameObject.activeInHierarchy && this.meshBoneDict.TryGetValue(this.childrenRenderers[index1], out combineInfo)) {
          CustomMechMeshMerge.combineVerts.AddRange((IEnumerable<Vector3>)combineInfo.verts);
          CustomMechMeshMerge.combineNormals.AddRange((IEnumerable<Vector3>)combineInfo.normals);
          CustomMechMeshMerge.combineTangents.AddRange((IEnumerable<Vector4>)combineInfo.tangents);
          CustomMechMeshMerge.combineUVs.AddRange((IEnumerable<Vector2>)combineInfo.uvs);
          CustomMechMeshMerge.combineWeights.AddRange((IEnumerable<BoneWeight>)combineInfo.boneWeights);
          CustomMechMeshMerge.combineColors.AddRange((IEnumerable<Color>)combineInfo.colors);
          if (combineInfo.materialIndex == 0) {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies.Add(combineInfo.triangles[index2] + num1);
          } else {
            for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
              CustomMechMeshMerge.combineIndicies1.Add(combineInfo.triangles[index2] + num1);
          }
          num1 += combineInfo.newMesh.vertexCount;
        }
      }
      if (this.combinedMesh0 != null) {
        Object.DestroyImmediate(this.combinedMesh0);
        this.combinedMesh0 = null;
      }
      if (CustomMechMeshMerge.combineVerts.Count > 0) {
        this.combinedMesh0 = new Mesh();
        this.combinedMesh0.subMeshCount = this.materialCount;
        this.combinedMesh0.SetVertices(CustomMechMeshMerge.combineVerts);
        this.combinedMesh0.SetTangents(CustomMechMeshMerge.combineTangents);
        this.combinedMesh0.SetUVs(0, CustomMechMeshMerge.combineUVs);
        this.combinedMesh0.boneWeights = CustomMechMeshMerge.combineWeights.ToArray();
        this.combinedMesh0.SetTriangles(CustomMechMeshMerge.combineIndicies, 0, true);
        if (CustomMechMeshMerge.combineIndicies1.Count > 0)
          this.combinedMesh0.SetTriangles(CustomMechMeshMerge.combineIndicies1, 1, true);
        this.combinedMesh0.SetNormals(CustomMechMeshMerge.combineNormals);
        this.combinedMesh0.SetColors(CustomMechMeshMerge.combineColors);
        this.combinedMesh0.bindposes = this.bindPoses.ToArray();
      }
      CustomMechMeshMerge.combineVerts.Clear();
      CustomMechMeshMerge.combineNormals.Clear();
      CustomMechMeshMerge.combineTangents.Clear();
      CustomMechMeshMerge.combineUVs.Clear();
      CustomMechMeshMerge.combineWeights.Clear();
      CustomMechMeshMerge.combineIndicies.Clear();
      CustomMechMeshMerge.combineIndicies1.Clear();
      CustomMechMeshMerge.combineColors.Clear();
      //int num2 = 0;
      //for (; index1 < this.childrenRenderers.Count; ++index1) {
      //  CustomMechMeshMerge.CombineInfo combineInfo;
      //  if (this.childrenRenderers[index1].gameObject.activeInHierarchy && this.meshBoneDict.TryGetValue(this.childrenRenderers[index1], out combineInfo)) {
      //    CustomMechMeshMerge.combineVerts.AddRange((IEnumerable<Vector3>)combineInfo.verts);
      //    CustomMechMeshMerge.combineNormals.AddRange((IEnumerable<Vector3>)combineInfo.normals);
      //    CustomMechMeshMerge.combineTangents.AddRange((IEnumerable<Vector4>)combineInfo.tangents);
      //    CustomMechMeshMerge.combineUVs.AddRange((IEnumerable<Vector2>)combineInfo.uvs);
      //    CustomMechMeshMerge.combineWeights.AddRange((IEnumerable<BoneWeight>)combineInfo.boneWeights);
      //    CustomMechMeshMerge.combineColors.AddRange((IEnumerable<Color>)combineInfo.colors);
      //    if (combineInfo.materialIndex == 0) {
      //      for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
      //        CustomMechMeshMerge.combineIndicies.Add(combineInfo.triangles[index2] + num2);
      //    } else {
      //      for (int index2 = 0; index2 < combineInfo.triangles.Length; ++index2)
      //        CustomMechMeshMerge.combineIndicies1.Add(combineInfo.triangles[index2] + num2);
      //    }
      //    num2 += combineInfo.newMesh.vertexCount;
      //  }
      //}
      //if ((Object)this.combinedMesh1 != (Object)null) {
      //  Object.DestroyImmediate((Object)this.combinedMesh1);
      //  this.combinedMesh1 = (Mesh)null;
      //}
      //if (CustomMechMeshMerge.combineVerts.Count != 0) {
      //  this.combinedMesh1 = new Mesh();
      //  this.combinedMesh1.subMeshCount = this.materialCount;
      //  this.combinedMesh1.SetVertices(CustomMechMeshMerge.combineVerts);
      //  this.combinedMesh1.SetTangents(CustomMechMeshMerge.combineTangents);
      //  this.combinedMesh1.SetUVs(0, CustomMechMeshMerge.combineUVs);
      //  this.combinedMesh1.boneWeights = CustomMechMeshMerge.combineWeights.ToArray();
      //  this.combinedMesh1.SetTriangles(CustomMechMeshMerge.combineIndicies, 0, true);
      //  if (CustomMechMeshMerge.combineIndicies1.Count > 0)
      //    this.combinedMesh1.SetTriangles(CustomMechMeshMerge.combineIndicies1, 1, true);
      //  this.combinedMesh1.SetNormals(CustomMechMeshMerge.combineNormals);
      //  this.combinedMesh1.SetColors(CustomMechMeshMerge.combineColors);
      //  this.combinedMesh1.bindposes = this.bindPoses.ToArray();
      //}
      if (this.combinedMesh0 != null) {
        if ((double)this.bottom < 0.0 && (double)this.size < 0.0) {
          Bounds bounds = this.combinedMesh0.bounds;
          //if ((Object)this.combinedMesh1 != (Object)null) bounds.Encapsulate(this.combinedMesh1.bounds);
          this.bottom = bounds.center.y - bounds.extents.y;
          this.size = bounds.size.y;
          foreach (KeyValuePair<SkinnedMeshRenderer, CustomMechMeshMerge.CombineInfo> keyValuePair in this.meshBoneDict)
            keyValuePair.Value.SetColorForHeight(this.bottom, this.size);
          this.combinedMesh0.GetVertices(CustomMechMeshMerge.combineVerts);
          this.combinedMesh0.GetColors(CustomMechMeshMerge.combineColors);
          for (int index2 = 0; index2 < CustomMechMeshMerge.combineVerts.Count; ++index2) {
            float num3 = (CustomMechMeshMerge.combineVerts[index2].y - this.bottom) / this.size;
            Color combineColor = CustomMechMeshMerge.combineColors[index2];
            combineColor.g = num3;
            CustomMechMeshMerge.combineColors[index2] = combineColor;
          }
          this.combinedMesh0.SetColors(CustomMechMeshMerge.combineColors);
          this.combinedMesh0.UploadMeshData(true);
          //if ((Object)this.combinedMesh1 != (Object)null) {
          //  this.combinedMesh1.GetVertices(CustomMechMeshMerge.combineVerts);
          //  this.combinedMesh1.GetColors(CustomMechMeshMerge.combineColors);
          //  for (int index2 = 0; index2 < CustomMechMeshMerge.combineVerts.Count; ++index2) {
          //    float num3 = (CustomMechMeshMerge.combineVerts[index2].y - this.bottom) / this.size;
          //    Color combineColor = CustomMechMeshMerge.combineColors[index2];
          //    combineColor.g = num3;
          //    CustomMechMeshMerge.combineColors[index2] = combineColor;
          //  }
          //  this.combinedMesh1.SetColors(CustomMechMeshMerge.combineColors);
          //  this.combinedMesh1.UploadMeshData(true);
          //}
        } else {
          this.combinedMesh0.UploadMeshData(true);
          //this.combinedMesh1.UploadMeshData(true);
        }
      }
      //CustomMechMeshMerge.combineVerts.Clear();
      //CustomMechMeshMerge.combineNormals.Clear();
      //CustomMechMeshMerge.combineTangents.Clear();
      //CustomMechMeshMerge.combineUVs.Clear();
      //CustomMechMeshMerge.combineWeights.Clear();
      //CustomMechMeshMerge.combineIndicies.Clear();
      //CustomMechMeshMerge.combineIndicies1.Clear();
      //CustomMechMeshMerge.combineColors.Clear();
    }

    private void BuildCombinedMesh() {
      if (this.SingleMesh) { this.BuildCombinedMeshSingle(); } else { this.BuildCombinedMeshClassic(); }
    }
    public void Update() {
      if (this.NeedRebuildDamaged.HasValue == false) { return; }
      if (this.visibleObject == null) { return; }
      if (this.visibleObject.activeInHierarchy == false) { return; }
      RefreshCombinedMesh(this.NeedRebuildDamaged.Value);
    }
    public void RefreshCombinedMesh(bool damaged) {
      if (this.visibleObject == null) { return; }
      if (this.visibleObject.activeInHierarchy == false) { this.NeedRebuildDamaged = damaged; return; }
      this.BuildCombinedMesh();
      this.NeedRebuildDamaged = null;
      if (this.combinedMesh0 != null) {
        this.mainSkinRenderer0.sharedMesh = this.combinedMesh0;
        this.mainSkinRenderer0.enabled = true;
        this.mainSkinRenderer0.bones = this.boneList.ToArray();
      } else {
        this.mainSkinRenderer0.enabled = false;
      }
      if (this.SingleMesh == false) {
        if ((Object)this.combinedMesh1 != (Object)null) {
          this.mainSkinRenderer1.sharedMesh = this.combinedMesh1;
          this.mainSkinRenderer1.enabled = true;
          this.mainSkinRenderer1.bones = this.boneList.ToArray();
        } else {
          this.mainSkinRenderer1.enabled = false;
        }
      }
      for (int index = 0; index < this.mechMaterial.Length; ++index) {
        if ((Object)this.mechMaterial[index] != (Object)null) {
          this.mechMaterial[index].SetTexture(MechMeshMerge.Uniforms._DamageAlbedoMap, (Texture)MechMeshMerge.damageAlbedo);
          this.mechMaterial[index].SetTextureScale("_DamageAlbedoMap", new Vector2(6f, 6f));
          this.mechMaterial[index].SetTexture(MechMeshMerge.Uniforms._DamageNormalMap, (Texture)MechMeshMerge.damageNormal);
        }
      }
      if (this.uiCreep != null) { this.uiCreep.RefreshCache(); }
      if (this.blockManager != null) { this.blockManager.UpdateCache(); }
      if (!damaged) { return; }
      for (int index = 0; index < this.mechMaterial.Length; ++index) {
        if ((Object)this.mechMaterial[index] != (Object)null)
          this.mechMaterial[index].EnableKeyword("_DAMAGED");
      }
    }

    //public void ForceEnable() => this.OnEnable();

    //public void Init(GameObject visibleObject, UICreep uiCreep, PropertyBlockManager blockManager) {
    //  Log.TWL(0, "CustomMechMeshMerge.Init " + this.name);
    //  this.visibleObject = visibleObject;
    //  this.uiCreep = uiCreep;
    //  this.blockManager = blockManager;
    //  if (OnEnableCalled) { this.OnEnable(); }
    //}
    //private bool OnEnableCalled = false;
    public void Init(CustomMechRepresentation parent, GameObject visibleObject, UICreep uiCreep, PropertyBlockManager blockManager,string prefix = null, MeshRenderer customHeraldry = null) {
      this.visibleObject = visibleObject;
      this.SingleMesh = this.childrenRenderers.Count < 10;
      if (this.visibleObject == null) { this.visibleObject = parent.VisibleObject; }
      this.parentRepresentation = parent;
      this.uiCreep = uiCreep;
      this.blockManager = blockManager;
      Log.TWL(0, "CustomMechMeshMerge.Init " + this.name + " visibleObject:"+(visibleObject == null?"null": visibleObject.name)+" uiCreep:"+(uiCreep == null?"null":uiCreep.name)+ " blockManager:"+(blockManager == null?"null": blockManager.name)+ " childrenRenderers.Count:"+ childrenRenderers.Count+" SingleMesh:"+this.SingleMesh);
      //OnEnableCalled = true;
      if (visibleObject == null) { return; }
      if (uiCreep == null) { return; }
      if (blockManager == null) { return; }
      //MechRepresentation component = this.GetComponent<MechRepresentation>();
      this.BuildCombinedMesh();
      if ((Object)this.newMeshRoot0 == (Object)null) {
        this.newMeshRoot0 = new GameObject();
        this.newMeshRoot0.name = (prefix == null?"":prefix) + "mesh_combined_0";
        this.newMeshRoot0.transform.parent = this.visibleObject.transform;
        this.mainSkinRenderer0 = this.newMeshRoot0.AddComponent<SkinnedMeshRenderer>();
        this.mainSkinRenderer0.bones = this.boneList.ToArray();
        if (this.materialCount == 1)
          this.mainSkinRenderer0.sharedMaterial = this.mechMaterial[0];
        else
          this.mainSkinRenderer0.sharedMaterials = this.mechMaterial;
        if ((Object)this.combinedMesh0 != (Object)null)
          this.mainSkinRenderer0.sharedMesh = this.combinedMesh0;
        else
          this.mainSkinRenderer0.enabled = false;
      }
      if (this.SingleMesh == false) {
        if ((Object)this.newMeshRoot1 == (Object)null) {
          this.newMeshRoot1 = new GameObject();
          this.newMeshRoot1.name = (prefix == null ? "" : prefix) + "mesh_combined_1";
          this.newMeshRoot1.transform.parent = this.visibleObject.transform;
          this.mainSkinRenderer1 = this.newMeshRoot1.AddComponent<SkinnedMeshRenderer>();
          this.mainSkinRenderer1.bones = this.boneList.ToArray();
          if (this.materialCount == 1)
            this.mainSkinRenderer1.sharedMaterial = this.mechMaterial[0];
          else
            this.mainSkinRenderer1.sharedMaterials = this.mechMaterial;
          if ((Object)this.combinedMesh1 != (Object)null)
            this.mainSkinRenderer1.sharedMesh = this.combinedMesh1;
          else
            this.mainSkinRenderer1.enabled = false;
        }
      }
      if (customHeraldry == null) {
        this.parentRepresentation.RegisterRenderersMainHeraldry(this.newMeshRoot0);
        if (this.SingleMesh == false) this.parentRepresentation.RegisterRenderersMainHeraldry(this.newMeshRoot1);
      } else {
        this.parentRepresentation.RegisterRenderersCustomHeraldry(this.newMeshRoot0, customHeraldry);
        if (this.SingleMesh == false) this.parentRepresentation.RegisterRenderersCustomHeraldry(this.newMeshRoot1, customHeraldry);
      }
      if ((Object)this.uiCreep != (Object)null)
        this.uiCreep.RefreshCache();
      if (!((Object)this.blockManager != (Object)null))
        return;
      this.blockManager.UpdateCache();
    }

    private void OnDestroy() {
      if ((Object)this.combinedMesh0 != (Object)null) {
        Object.DestroyImmediate((Object)this.combinedMesh0);
        this.combinedMesh0 = (Mesh)null;
      }
      if (this.SingleMesh == false) { 
        if ((Object)this.combinedMesh1 != (Object)null) {
          Object.DestroyImmediate((Object)this.combinedMesh1);
          this.combinedMesh1 = (Mesh)null;
        }
      }
      if (this.meshBoneDict != null) {
        foreach (KeyValuePair<SkinnedMeshRenderer, CustomMechMeshMerge.CombineInfo> keyValuePair in this.meshBoneDict) {
          keyValuePair.Value.originalMesh = (Mesh)null;
          Object.DestroyImmediate((Object)keyValuePair.Value.newMesh);
        }
        this.meshBoneDict.Clear();
      }
      for (int index = 0; index < this.mechMaterial.Length; ++index) {
        if (this.mechMaterial != null) {
          Object.DestroyImmediate((Object)this.mechMaterial[index]);
          this.mechMaterial[index] = (Material)null;
        }
      }
      for (int index = 0; index < this.explodeMechMaterial.Length; ++index) {
        if (this.explodeMechMaterial != null) {
          Object.DestroyImmediate((Object)this.explodeMechMaterial[index]);
          this.explodeMechMaterial[index] = (Material)null;
        }
      }
      if ((Object)this.explodeWeaponMaterial != (Object)null) {
        Object.DestroyImmediate((Object)this.explodeWeaponMaterial);
        this.explodeWeaponMaterial = (Material)null;
      }
      this.newMeshRoot0 = (GameObject)null;
      if (this.SingleMesh == false) this.newMeshRoot1 = (GameObject)null;
      this.visibleObject = (GameObject)null;
    }

    public static class Uniforms {
      public static readonly int _DamageAlbedoMap = Shader.PropertyToID(nameof(_DamageAlbedoMap));
      public static readonly int _DamageNormalMap = Shader.PropertyToID(nameof(_DamageNormalMap));
    }

    internal class CombineInfo {
      internal int boneIndex;
      internal Mesh originalMesh;
      internal Mesh newMesh;
      internal Matrix4x4 localToWorldMatrix;
      internal int materialIndex;
      private static List<Vector3> tempVerts = new List<Vector3>();
      private static List<Vector3> tempNormals = new List<Vector3>();
      private static List<Vector4> tempTangents = new List<Vector4>();
      private static List<Color> tempColor = new List<Color>();
      private static List<Vector3> origVerts = new List<Vector3>();
      private static List<Vector3> origNormals = new List<Vector3>();
      private static List<Vector4> origTangents = new List<Vector4>();
      private static List<Vector2> origUV = new List<Vector2>();
      private static List<int> origTris = new List<int>();
      internal Vector3[] verts;
      internal Vector3[] normals;
      internal Vector4[] tangents;
      internal Vector2[] uvs;
      internal BoneWeight[] boneWeights;
      internal Color[] colors;
      internal int[] triangles;

      internal CombineInfo(Mesh mesh, int index, Matrix4x4 trs, int matIndex) {
        this.originalMesh = mesh;
        this.boneIndex = index;
        this.materialIndex = matIndex;
        BoneWeight[] boneWeightArray = new BoneWeight[this.originalMesh.vertexCount];
        CustomMechMeshMerge.CombineInfo.tempVerts.Clear();
        CustomMechMeshMerge.CombineInfo.tempNormals.Clear();
        CustomMechMeshMerge.CombineInfo.tempTangents.Clear();
        CustomMechMeshMerge.CombineInfo.tempColor.Clear();
        this.originalMesh.GetVertices(CustomMechMeshMerge.CombineInfo.origVerts);
        this.originalMesh.GetNormals(CustomMechMeshMerge.CombineInfo.origNormals);
        this.originalMesh.GetTangents(CustomMechMeshMerge.CombineInfo.origTangents);
        this.originalMesh.GetUVs(0, CustomMechMeshMerge.CombineInfo.origUV);
        this.originalMesh.GetTriangles(CustomMechMeshMerge.CombineInfo.origTris, 0);
        bool flag = mesh.name.ToLower().Contains("dmg");
        for (int index1 = 0; index1 < this.originalMesh.vertexCount; ++index1) {
          boneWeightArray[index1].boneIndex0 = index;
          boneWeightArray[index1].weight0 = 1f;
          CustomMechMeshMerge.CombineInfo.tempVerts.Add(trs.MultiplyPoint3x4(CustomMechMeshMerge.CombineInfo.origVerts[index1]));
          CustomMechMeshMerge.CombineInfo.tempNormals.Add(trs.MultiplyVector(CustomMechMeshMerge.CombineInfo.origNormals[index1]));
          Vector3 vector3 = trs.MultiplyVector((Vector3)CustomMechMeshMerge.CombineInfo.origTangents[index1]);
          CustomMechMeshMerge.CombineInfo.tempTangents.Add(new Vector4(vector3.x, vector3.y, vector3.z, CustomMechMeshMerge.CombineInfo.origTangents[index1].w));
          CustomMechMeshMerge.CombineInfo.tempColor.Add(flag ? new Color(0.0f, 0.0f, 0.0f, 0.0f) : new Color(1f, 1f, 1f, 1f));
        }
        this.newMesh = new Mesh();
        this.newMesh.SetVertices(CustomMechMeshMerge.CombineInfo.tempVerts);
        this.newMesh.SetNormals(CustomMechMeshMerge.CombineInfo.tempNormals);
        this.newMesh.SetTangents(CustomMechMeshMerge.CombineInfo.tempTangents);
        this.newMesh.boneWeights = boneWeightArray;
        this.newMesh.SetColors(CustomMechMeshMerge.CombineInfo.tempColor);
        this.newMesh.SetUVs(0, CustomMechMeshMerge.CombineInfo.origUV);
        this.localToWorldMatrix = trs;
        this.newMesh.SetTriangles(CustomMechMeshMerge.CombineInfo.origTris, 0, true);
        this.newMesh.name = this.originalMesh.name + "_combinePrep";
        this.verts = CustomMechMeshMerge.CombineInfo.tempVerts.ToArray();
        this.normals = CustomMechMeshMerge.CombineInfo.tempNormals.ToArray();
        this.tangents = CustomMechMeshMerge.CombineInfo.tempTangents.ToArray();
        this.boneWeights = boneWeightArray;
        this.colors = CustomMechMeshMerge.CombineInfo.tempColor.ToArray();
        this.uvs = CustomMechMeshMerge.CombineInfo.origUV.ToArray();
        this.triangles = CustomMechMeshMerge.CombineInfo.origTris.ToArray();
      }

      internal void SetColorForHeight(float bottom, float size) {
        for (int index = 0; index < this.colors.Length; ++index) {
          float num = (this.verts[index].y - bottom) / size;
          Color color = this.colors[index];
          color.g = num;
          this.colors[index] = color;
        }
        this.newMesh.colors = this.colors;
      }
    }
  }
}