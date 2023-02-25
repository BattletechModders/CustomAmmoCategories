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
using BattleTech.Rendering;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  //public class QuadLegsRepresentationSimGame: MonoBehaviour {
  //  public MechRepresentationSimGame LegsRepresentation { get; set; }
  //  public void Init(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
  //    LegsRepresentation.Init(dataManager, mechDef, parentTransform, heraldryDef);
  //  }
  //}
  //public class QuadRepresentationSimGame: MonoBehaviour {
  //  public QuadLegsRepresentationSimGame FLegsRepresentation { get; set; }
  //  public MechRepresentationSimGame RLegsRepresentation { get; set; }
  //  public void Instantine(MechRepresentationSimGame RLegs, DataManager dataManager, MechDef mechDef) {
  //    RLegsRepresentation = RLegs;
  //    UnitCustomInfo info = mechDef.GetCustomInfo();
  //    if (info == null) { return; }
  //    if (info.quadVisualInfo.UseQuadVisuals == false) { return; }
  //    string arg = info.quadVisualInfo.FLegsPrefabBase;
  //    string mechPrefabName = string.Format("chrPrfComp_{0}_simgame", arg);
  //    GameObject fLegsGO = dataManager.PooledInstantiate(mechPrefabName, BattleTechResourceType.Prefab);
  //    FLegsRepresentation = fLegsGO.AddComponent<QuadLegsRepresentationSimGame>();
  //    FLegsRepresentation.LegsRepresentation = fLegsGO.GetComponent<MechRepresentationSimGame>();
  //    Transform j_Root = RLegs.transform.FindRecursive("j_Root");
  //    if(j_Root != null) {
  //      j_Root.localPosition = new Vector3(0f, 0f, 0f - info.quadVisualInfo.BodyLength / 2f);
  //    }
  //    fLegsGO.transform.SetParent(RLegs.transform);
  //    fLegsGO.transform.localPosition = new Vector3(0f, 0f, info.quadVisualInfo.BodyLength / 2f);
  //    SkinnedMeshRenderer[] meshes = RLegsRepresentation.GetComponentsInChildren<SkinnedMeshRenderer>();
  //    Log.WL(1, "MechRepresentation.Init rear legs nullify SkinnedMeshRenderers:" + meshes.Length);
  //    foreach (SkinnedMeshRenderer mesh in meshes) {
  //      foreach (string keyword in info.quadVisualInfo.SuppressRenderers) { if (mesh.name.Contains(keyword)) { mesh.gameObject.SetActive(false); } }
  //      if (info.quadVisualInfo.NotSuppressRenderers.Count > 0) {
  //        bool foundNotSuppress = false;
  //        foreach (string keyword in info.quadVisualInfo.NotSuppressRenderers) { if (mesh.name.Contains(keyword)) { foundNotSuppress = true; break; } }
  //        if (foundNotSuppress == false) { mesh.gameObject.SetActive(false); }
  //      }
  //      Log.WL(2, mesh.gameObject.name + ":" + (mesh.gameObject.activeInHierarchy ? "show" : "hide"));
  //    }
  //    meshes = FLegsRepresentation.GetComponentsInChildren<SkinnedMeshRenderer>();
  //    Log.WL(1, "MechRepresentation.Init front legs nullify SkinnedMeshRenderers:" + meshes.Length);
  //    foreach (SkinnedMeshRenderer mesh in meshes) {
  //      foreach (string keyword in info.quadVisualInfo.SuppressRenderers) { if (mesh.name.Contains(keyword)) { mesh.gameObject.SetActive(false); } }
  //      if (info.quadVisualInfo.NotSuppressRenderers.Count > 0) {
  //        bool foundNotSuppress = false;
  //        foreach (string keyword in info.quadVisualInfo.NotSuppressRenderers) { if (mesh.name.Contains(keyword)) { foundNotSuppress = true; break; } }
  //        if (foundNotSuppress == false) { mesh.gameObject.SetActive(false); }
  //      }
  //      Log.WL(2, mesh.gameObject.name + ":" + (mesh.gameObject.activeInHierarchy ? "show" : "hide"));
  //    }
  //    RLegsRepresentation.LeftArmAttach = FLegsRepresentation.LegsRepresentation.LeftLegAttach;
  //    RLegsRepresentation.RightArmAttach = FLegsRepresentation.LegsRepresentation.RightLegAttach;
  //    RLegsRepresentation.vfxLeftArmTransform = FLegsRepresentation.LegsRepresentation.vfxLeftLegTransform;
  //    RLegsRepresentation.vfxRightArmTransform = FLegsRepresentation.LegsRepresentation.vfxRightLegTransform;
  //    RLegsRepresentation.vfxLeftShoulderTransform = FLegsRepresentation.LegsRepresentation.vfxLeftLegTransform;
  //    RLegsRepresentation.vfxRightShoulderTransform = FLegsRepresentation.LegsRepresentation.vfxRightLegTransform;
  //    RLegsRepresentation.leftArmDestructible = FLegsRepresentation.LegsRepresentation.leftLegDestructible;
  //    RLegsRepresentation.rightArmDestructible = FLegsRepresentation.LegsRepresentation.rightLegDestructible;
  //    RLegsRepresentation.centerTorsoDestructible = null;
  //    RLegsRepresentation.rightTorsoDestructible = null;
  //    RLegsRepresentation.leftTorsoDestructible = null;
  //    FLegsRepresentation.LegsRepresentation.headDestructible = null;
  //    FLegsRepresentation.LegsRepresentation.centerTorsoDestructible = null;
  //    FLegsRepresentation.LegsRepresentation.rightTorsoDestructible = null;
  //    FLegsRepresentation.LegsRepresentation.leftTorsoDestructible = null;
  //    FLegsRepresentation.LegsRepresentation.headDestructible = null;
  //  }
  //  public void Init(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
  //    FLegsRepresentation.Init(dataManager,mechDef,parentTransform,heraldryDef);
  //  }
  //}
  //public class QuadRepresentation: MonoBehaviour {
  //  public MechRepresentation mechRep { get; set; }
  //  public QuadLegsRepresentation fLegsRep { get; set; }
  //  public string prefabBase { get; set; }
  //  //public QuadLegsRepresentation rLegsRep { get; set; }
  //  public float TurnParam { set { fLegsRep.TurnParam = value; } }
  //  public float ForwardParam { set { fLegsRep.ForwardParam = value; } }
  //  public bool IsMovingParam { set { fLegsRep.IsMovingParam = value; } }
  //  public bool BeginMovementParam { set { fLegsRep.BeginMovementParam = value; } }
  //  public void InitPaintScheme(HeraldryDef heraldryDef, string teamGUID) {
  //    fLegsRep.InitPaintScheme(heraldryDef, teamGUID);
  //  }
  //  public float DamageParam {
  //    set {
  //      if (mechRep.parentMech.LeftArmDamageLevel == LocationDamageLevel.Destroyed) { fLegsRep.DamageParam = -1f; } else
  //      if (mechRep.parentMech.RightArmDamageLevel == LocationDamageLevel.Destroyed) { fLegsRep.DamageParam = 1f; } else {
  //        fLegsRep.DamageParam = 0f;
  //      }
  //    }
  //  }
  //}
  public class QuadLegsRepresentation: MonoBehaviour {
    public MechRepresentation LegsRep { get; set; }
    public Animator ThisAnimator { get; set; }
    public int ForwardHash { get; set; }
    public int TurnHash { get; set; }
    public int IsMovingHash { get; set; }
    public int BeginMovementHash { get; set; }
    public int DamageHash { get; set; }
    public bool HasForwardParam { get; set; }
    public bool HasTurnParam { get; set; }
    public bool HasIsMovingParam { get; set; }
    public bool HasBeginMovementParam { get; set; }
    public bool HasDamageParam { get; set; }
    public float TurnParam {
      set {
        if (!this.HasTurnParam)
          return;
        this.ThisAnimator.SetFloat(this.TurnHash, value);
      }
    }
    public float ForwardParam {
      set {
        if (!this.HasForwardParam)
          return;
        this.ThisAnimator.SetFloat(this.ForwardHash, value);
      }
    }
    public bool IsMovingParam {
      set {
        if (!this.HasIsMovingParam)
          return;
        this.ThisAnimator.SetBool(this.IsMovingHash, value);
      }
    }
    public bool BeginMovementParam {
      set {
        if (!this.HasBeginMovementParam)
          return;
        this.ThisAnimator.SetTrigger(this.BeginMovementHash);
      }
    }
    public float DamageParam {
      set {
        if (!this.HasDamageParam)
          return;
        this.ThisAnimator.SetFloat(this.DamageHash, value);
      }
    }
    public QuadLegsRepresentation() {

    }
    public void InitPaintScheme(HeraldryDef heraldryDef, string teamGUID) {
      LegsRep.InitPaintScheme(heraldryDef, teamGUID);
    }
    public void Awake() {
      LegsRep = this.GetComponent<MechRepresentation>();
      this.ThisAnimator = this.GetComponent<Animator>();
      ThisAnimator.enabled = true;
      this.ForwardHash = Animator.StringToHash("Forward");
      this.TurnHash = Animator.StringToHash("Turn");
      this.IsMovingHash = Animator.StringToHash("IsMoving");
      this.BeginMovementHash = Animator.StringToHash("BeginMovement");
      this.DamageHash = Animator.StringToHash("Damage");
      AnimatorControllerParameter[] parameters = this.ThisAnimator.parameters;
      for (int index = 0; index < parameters.Length; ++index) {
        if (parameters[index].nameHash == this.ForwardHash)
          this.HasForwardParam = true;
        if (parameters[index].nameHash == this.TurnHash)
          this.HasTurnParam = true;
        if (parameters[index].nameHash == this.IsMovingHash)
          this.HasIsMovingParam = true;
        if (parameters[index].nameHash == this.BeginMovementHash)
          this.HasBeginMovementParam = true;
        if (parameters[index].nameHash == this.DamageHash)
          this.HasDamageParam = true;
      }
    }
  }
#if BT_PUBLIC_ASSEMBLY
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("BuildCombinedMesh")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_BuildCombinedMesh {
    public static bool Prefix(MechMeshMerge __instance, ref Dictionary<SkinnedMeshRenderer, MechMeshMerge.CombineInfo> ___meshBoneDict,ref Mesh ___combinedMesh0, ref Mesh ___combinedMesh1,ref int ___materialCount,ref List<Matrix4x4> ___bindPoses,ref float ___bottom, ref float ___size) {
      Log.TWL(0, "MechMeshMerge.BuildCombinedMesh");
      try {
        if (___meshBoneDict == null || ___meshBoneDict.Count == 0) {
          //__instance.GenerateCache();
          typeof(MechMeshMerge).GetMethod("GenerateCache", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { });
        }
        int num1 = 0;
        //Dictionary<SkinnedMeshRenderer, object> meshBoneDict = new Dictionary<SkinnedMeshRenderer, object>();
        //foreach(DictionaryEntry bone in ___meshBoneDict) {
        //  meshBoneDict.Add((SkinnedMeshRenderer)bone.Key, bone.Value);
        //}
        List<Vector3> combineVerts = Traverse.Create(__instance).Field<List<Vector3>>("combineVerts").Value;
        List<Vector3> combineNormals = Traverse.Create(__instance).Field<List<Vector3>>("combineNormals").Value;
        List<Vector4> combineTangents = Traverse.Create(__instance).Field<List<Vector4>>("combineTangents").Value;
        List<Vector2> combineUVs = Traverse.Create(__instance).Field<List<Vector2>>("combineUVs").Value;
        List<BoneWeight> combineWeights = Traverse.Create(__instance).Field<List<BoneWeight>>("combineWeights").Value;
        List<int> combineIndicies = Traverse.Create(__instance).Field<List<int>>("combineIndicies").Value;
        List<int> combineIndicies1 = Traverse.Create(__instance).Field<List<int>>("combineIndicies1").Value;
        List<Color> combineColors = Traverse.Create(__instance).Field<List<Color>>("combineColors").Value;
        combineVerts.Clear();
        combineNormals.Clear();
        combineTangents.Clear();
        combineUVs.Clear();
        combineWeights.Clear();
        combineIndicies.Clear();
        combineIndicies1.Clear();
        combineColors.Clear();
        int index1;
        SkinnedMeshRenderer[] childrenRenderers = Traverse.Create(__instance).Property<SkinnedMeshRenderer[]>("childrenRenderers").Value;
        foreach (SkinnedMeshRenderer renderer in childrenRenderers) {
          Log.WL(1, "childrenRenderer:"+renderer.name);
        }
        for (index1 = 0; index1 < childrenRenderers.Length / 2; ++index1) {
          MechMeshMerge.CombineInfo combineInfo;
          if (childrenRenderers[index1].gameObject.activeInHierarchy && ___meshBoneDict.TryGetValue(childrenRenderers[index1], out combineInfo)) {
            Log.WL(1,"mesh0:"+ combineInfo.originalMesh.name);
            combineVerts.AddRange((IEnumerable<Vector3>)Traverse.Create(combineInfo).Field<Vector3[]>("verts").Value);
            combineNormals.AddRange((IEnumerable<Vector3>)Traverse.Create(combineInfo).Field<Vector3[]>("normals").Value);
            combineTangents.AddRange((IEnumerable<Vector4>)Traverse.Create(combineInfo).Field<Vector4[]>("tangents").Value);
            combineUVs.AddRange((IEnumerable<Vector2>)Traverse.Create(combineInfo).Field<Vector2[]>("uvs").Value);
            combineWeights.AddRange((IEnumerable<BoneWeight>)Traverse.Create(combineInfo).Field<BoneWeight[]>("boneWeights").Value);
            combineColors.AddRange((IEnumerable<Color>)Traverse.Create(combineInfo).Field<Color[]>("colors").Value);
            if (Traverse.Create(combineInfo).Field<int>("materialIndex").Value == 0) {
              for (int index2 = 0; index2 < Traverse.Create(combineInfo).Field<int[]>("triangles").Value.Length; ++index2)
                combineIndicies.Add(Traverse.Create(combineInfo).Field<int[]>("triangles").Value[index2] + num1);
            } else {
              for (int index2 = 0; index2 < Traverse.Create(combineInfo).Field<int[]>("triangles").Value.Length; ++index2)
                combineIndicies1.Add(Traverse.Create(combineInfo).Field<int[]>("triangles").Value[index2] + num1);
            }
            num1 += Traverse.Create(combineInfo).Field<Mesh>("newMesh").Value.vertexCount;
          }
        }
        if ((UnityEngine.Object)___combinedMesh0 != (UnityEngine.Object)null) {
          UnityEngine.Object.DestroyImmediate((UnityEngine.Object)___combinedMesh0);
          ___combinedMesh0 = (Mesh)null;
        }
        if (combineVerts.Count > 0) {
          ___combinedMesh0 = new Mesh();
          ___combinedMesh0.subMeshCount = ___materialCount;
          ___combinedMesh0.SetVertices(combineVerts);
          ___combinedMesh0.SetTangents(combineTangents);
          ___combinedMesh0.SetUVs(0, combineUVs);
          ___combinedMesh0.boneWeights = combineWeights.ToArray();
          ___combinedMesh0.SetTriangles(combineIndicies, 0, true);
          if (combineIndicies1.Count > 0)
            ___combinedMesh0.SetTriangles(combineIndicies1, 1, true);
          ___combinedMesh0.SetNormals(combineNormals);
          ___combinedMesh0.SetColors(combineColors);
          ___combinedMesh0.bindposes = ___bindPoses.ToArray();
        }
        combineVerts.Clear();
        combineNormals.Clear();
        combineTangents.Clear();
        combineUVs.Clear();
        combineWeights.Clear();
        combineIndicies.Clear();
        combineIndicies1.Clear();
        combineColors.Clear();
        int num2 = 0;
        for (; index1 < childrenRenderers.Length; ++index1) {
          MechMeshMerge.CombineInfo combineInfo;
          if (childrenRenderers[index1].gameObject.activeInHierarchy && ___meshBoneDict.TryGetValue(childrenRenderers[index1], out combineInfo)) {
            Log.WL(1, "mesh1:" + combineInfo.originalMesh.name);
            combineVerts.AddRange((IEnumerable<Vector3>)Traverse.Create(combineInfo).Field<Vector3[]>("verts").Value);
            combineNormals.AddRange((IEnumerable<Vector3>)Traverse.Create(combineInfo).Field<Vector3[]>("normals").Value);
            combineTangents.AddRange((IEnumerable<Vector4>)Traverse.Create(combineInfo).Field<Vector4[]>("tangents").Value);
            combineUVs.AddRange((IEnumerable<Vector2>)Traverse.Create(combineInfo).Field<Vector2[]>("uvs").Value);
            combineWeights.AddRange((IEnumerable<BoneWeight>)Traverse.Create(combineInfo).Field<BoneWeight[]>("boneWeights").Value);
            combineColors.AddRange((IEnumerable<Color>)Traverse.Create(combineInfo).Field<Color[]>("colors").Value);
            if (Traverse.Create(combineInfo).Field<int>("materialIndex").Value == 0) {
              for (int index2 = 0; index2 < Traverse.Create(combineInfo).Field<int[]>("triangles").Value.Length; ++index2)
                combineIndicies.Add(Traverse.Create(combineInfo).Field<int[]>("triangles").Value[index2] + num2);
            } else {
              for (int index2 = 0; index2 < Traverse.Create(combineInfo).Field<int[]>("triangles").Value.Length; ++index2)
                combineIndicies1.Add(Traverse.Create(combineInfo).Field<int[]>("triangles").Value[index2] + num2);
            }
            num2 += Traverse.Create(combineInfo).Field<Mesh>("newMesh").Value.vertexCount;
          }
        }
        if ((UnityEngine.Object)___combinedMesh1 != (UnityEngine.Object)null) {
          UnityEngine.Object.DestroyImmediate((UnityEngine.Object)___combinedMesh1);
          ___combinedMesh1 = (Mesh)null;
        }
        if (combineVerts.Count != 0) {
          ___combinedMesh1 = new Mesh();
          ___combinedMesh1.subMeshCount = ___materialCount;
          ___combinedMesh1.SetVertices(combineVerts);
          ___combinedMesh1.SetTangents(combineTangents);
          ___combinedMesh1.SetUVs(0, combineUVs);
          ___combinedMesh1.boneWeights = combineWeights.ToArray();
          ___combinedMesh1.SetTriangles(combineIndicies, 0, true);
          if (combineIndicies1.Count > 0)
            ___combinedMesh1.SetTriangles(combineIndicies1, 1, true);
          ___combinedMesh1.SetNormals(combineNormals);
          ___combinedMesh1.SetColors(combineColors);
          ___combinedMesh1.bindposes = ___bindPoses.ToArray();
        }
        if ((double)___bottom < 0.0 && (double)___size < 0.0) {
          Bounds bounds = ___combinedMesh0.bounds;
          if ((UnityEngine.Object)___combinedMesh1 != (UnityEngine.Object)null)
            bounds.Encapsulate(___combinedMesh1.bounds);
          ___bottom = bounds.center.y - bounds.extents.y;
          ___size = bounds.size.y;
          foreach (KeyValuePair<SkinnedMeshRenderer, MechMeshMerge.CombineInfo> keyValuePair in ___meshBoneDict)
            Traverse.Create(keyValuePair.Value).Method("SetColorForHeight", new Type[] { typeof(float), typeof(float) }).GetValue(new object[] { ___bottom, ___size });
            //keyValuePair.Value .SetColorForHeight(___bottom, ___size);
          ___combinedMesh0.GetVertices(combineVerts);
          ___combinedMesh0.GetColors(combineColors);
          for (int index2 = 0; index2 < combineVerts.Count; ++index2) {
            float num3 = (combineVerts[index2].y - ___bottom) / ___size;
            Color combineColor = combineColors[index2];
            combineColor.g = num3;
            combineColors[index2] = combineColor;
          }
          ___combinedMesh0.SetColors(combineColors);
          ___combinedMesh0.UploadMeshData(true);
          if ((UnityEngine.Object)___combinedMesh1 != (UnityEngine.Object)null) {
            ___combinedMesh1.GetVertices(combineVerts);
            ___combinedMesh1.GetColors(combineColors);
            for (int index2 = 0; index2 < combineVerts.Count; ++index2) {
              float num3 = (combineVerts[index2].y - ___bottom) / ___size;
              Color combineColor = combineColors[index2];
              combineColor.g = num3;
              combineColors[index2] = combineColor;
            }
            ___combinedMesh1.SetColors(combineColors);
            ___combinedMesh1.UploadMeshData(true);
          }
        } else {
          ___combinedMesh0.UploadMeshData(true);
          ___combinedMesh1.UploadMeshData(true);
        }
        combineVerts.Clear();
        combineNormals.Clear();
        combineTangents.Clear();
        combineUVs.Clear();
        combineWeights.Clear();
        combineIndicies.Clear();
        combineIndicies1.Clear();
        combineColors.Clear();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("OnEnable")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechMeshMerge_OnEnable {
    public static void Postfix(MechMeshMerge __instance) {
      try {
        string exportPath0 = Path.Combine(Core.BaseDir, __instance.gameObject.name + ".mesh0.enable.fbx");
        string exportPath1 = Path.Combine(Core.BaseDir, __instance.gameObject.name + ".mesh1.enable.fbx");
        FBXExporter.ExportGameObjToFBX(__instance.newMeshRoot0, exportPath0, false, false);
        FBXExporter.ExportGameObjToFBX(__instance.newMeshRoot1, exportPath1, false, false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(MechMeshMerge))]
  [HarmonyPatch("RefreshCombinedMesh")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class MechMeshMerge_RefreshCombinedMesh {
    public static void Postfix(MechMeshMerge __instance) {
      try {
        string exportPath0 = Path.Combine(Core.BaseDir, __instance.gameObject.name + ".mesh0.refresh.fbx");
        string exportPath1 = Path.Combine(Core.BaseDir, __instance.gameObject.name + ".mesh1.refresh.fbx");
        FBXExporter.ExportGameObjToFBX(__instance.newMeshRoot0, exportPath0, false, false);
        FBXExporter.ExportGameObjToFBX(__instance.newMeshRoot1, exportPath1, false, false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
#endif
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("Init")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Mech), typeof(Transform), typeof(bool) })]
  //public static class MechRepresentation_Init {
  //  public static void InitHeadLights(this QuadLegsRepresentation quadLegs, Mech mech) {
  //    UnitCustomInfo info = mech.GetCustomInfo();
  //    if (info == null) { return; }
  //    List<GameObject> headlightReps = Traverse.Create(quadLegs.LegsRep).Field<List<GameObject>>("headlightReps").Value;
  //    foreach (GameObject headLight in headlightReps) { GameObject.Destroy(headLight); }
  //    headlightReps.Clear();
  //    string PrefabBase = info.quadVisualInfo.FLegsPrefabBase;
  //    string id1 = string.Format("chrPrfComp_{0}_leftleg_headlight", (object)PrefabBase);
  //    string id2 = string.Format("chrPrfComp_{0}_rightleg_headlight", (object)PrefabBase);
  //    GameObject gameObject1 = mech.Combat.DataManager.PooledInstantiate(id1, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    if ((UnityEngine.Object)gameObject1 != (UnityEngine.Object)null) {
  //      gameObject1.transform.parent = quadLegs.LegsRep.vfxLeftLegTransform;
  //      gameObject1.transform.localPosition = Vector3.zero;
  //      gameObject1.transform.localRotation = Quaternion.identity;
  //      gameObject1.transform.localScale = Vector3.one;
  //      headlightReps.Add(gameObject1);
  //    }
  //    GameObject gameObject2 = mech.Combat.DataManager.PooledInstantiate(id2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    if ((UnityEngine.Object)gameObject2 != (UnityEngine.Object)null) {
  //      gameObject2.transform.parent = quadLegs.LegsRep.vfxRightLegTransform;
  //      gameObject2.transform.localPosition = Vector3.zero;
  //      gameObject2.transform.localRotation = Quaternion.identity;
  //      gameObject2.transform.localScale = Vector3.one;
  //      headlightReps.Add(gameObject2);
  //    }
  //  }
  //  public static void Prefix(MechRepresentation __instance, Mech mech, Transform parentTransform, bool isParented) {
  //    try {
  //      Log.TWL(0, "MechRepresentation.Init prefix "+mech.PilotableActorDef.Description.Id, true);
  //      UnitCustomInfo info = mech.GetCustomInfo();
  //      if (info == null) { return; }
  //      if(info.AlternateRepresentations.Count > 0) {
  //        MechRepresentation_InitAlternate.Prefix(__instance, mech, parentTransform, isParented, info);
  //      }
  //      QuadLegsRepresentation quadLegs = __instance.gameObject.GetComponent<QuadLegsRepresentation>();
  //      if (quadLegs == null) {
  //        if (info.quadVisualInfo.UseQuadVisuals) {
  //          QuadRepresentation quadRepresentation = __instance.gameObject.GetComponent<QuadRepresentation>();
  //          if (quadRepresentation == null) {
  //            quadRepresentation = __instance.gameObject.AddComponent<QuadRepresentation>();
  //            quadRepresentation.mechRep = __instance;
  //            quadRepresentation.prefabBase = info.quadVisualInfo.FLegsPrefabBase;
  //            Transform j_Root = __instance.transform.FindRecursive("j_Root");
  //            if (j_Root != null) { j_Root.localPosition = new Vector3(0f, 0f, 0f - info.quadVisualInfo.BodyLength / 2f); }
  //            SkinnedMeshRenderer[] meshes = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
  //            Log.WL(1, "MechRepresentation.Init rear legs nullify SkinnedMeshRenderers:" + meshes.Length);
  //            foreach (SkinnedMeshRenderer mesh in meshes) {
  //              foreach (string keyword in info.quadVisualInfo.SuppressRenderers) { if (mesh.name.Contains(keyword)) { mesh.gameObject.SetActive(false); } }
  //              if (info.quadVisualInfo.NotSuppressRenderers.Count > 0) {
  //                bool foundNotSuppress = false;
  //                foreach (string keyword in info.quadVisualInfo.NotSuppressRenderers) { if (mesh.name.Contains(keyword)) { foundNotSuppress = true; break; } }
  //                if (foundNotSuppress == false) { mesh.gameObject.SetActive(false); }
  //              }
  //              Log.WL(2, mesh.gameObject.name + ":" + (mesh.gameObject.activeInHierarchy ? "show" : "hide"));
  //            }
  //          }
  //        }
  //      }
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return;
  //  }
  //  public static void Postfix(MechRepresentation __instance, Mech mech, Transform parentTransform, bool isParented) {
  //    try {
  //      Log.TWL(0, "MechRepresentation.Init postfix " + mech.PilotableActorDef.Description.Id, true);
  //      UnitCustomInfo info = mech.GetCustomInfo();
  //      if (info == null) { return; }
  //      QuadLegsRepresentation quadLegs = __instance.gameObject.GetComponent<QuadLegsRepresentation>();
  //      if (quadLegs == null) {
  //        if (info.quadVisualInfo.UseQuadVisuals) {
  //          bool quadRepInited = false;
  //          QuadRepresentation quadRepresentation = __instance.gameObject.GetComponent<QuadRepresentation>();
  //          if (quadRepresentation != null) {
  //            if (quadRepresentation.fLegsRep != null) { quadRepInited = true; }
  //          }
  //          if (quadRepInited == false) {
  //            if (string.IsNullOrEmpty(info.quadVisualInfo.FLegsPrefab) == false) {
  //              GameObject forwardLegsGO = mech.Combat.DataManager.PooledInstantiate(info.quadVisualInfo.FLegsPrefab, BattleTechResourceType.Prefab);
  //              if (forwardLegsGO != null) {
  //                quadLegs = forwardLegsGO.GetComponent<QuadLegsRepresentation>();
  //                if (quadLegs == null) { quadLegs = forwardLegsGO.AddComponent<QuadLegsRepresentation>(); }
  //                quadRepresentation.fLegsRep = quadLegs;
  //                quadLegs.LegsRep = forwardLegsGO.GetComponent<MechRepresentation>();
  //                if (quadRepresentation.fLegsRep.LegsRep != null) {
  //                  SkinnedMeshRenderer[] meshes = quadRepresentation.fLegsRep.GetComponentsInChildren<SkinnedMeshRenderer>();
  //                  Log.WL(1, "MechRepresentation.Init forward legs nullify SkinnedMeshRenderers:" + meshes.Length);
  //                  foreach (SkinnedMeshRenderer mesh in meshes) {
  //                    foreach (string keyword in info.quadVisualInfo.SuppressRenderers) { if (mesh.name.Contains(keyword)) { mesh.gameObject.SetActive(false); } }
  //                    if (info.quadVisualInfo.NotSuppressRenderers.Count > 0) {
  //                      bool foundNotSuppress = false;
  //                      foreach (string keyword in info.quadVisualInfo.NotSuppressRenderers) { if (mesh.name.Contains(keyword)) { foundNotSuppress = true; break; } }
  //                      if (foundNotSuppress == false) { mesh.gameObject.SetActive(false); }
  //                    }
  //                    Log.WL(2, mesh.gameObject.name + ":" + (mesh.gameObject.activeInHierarchy ? "show" : "hide"));
  //                  }
  //                  quadRepresentation.fLegsRep.LegsRep.Init(mech, parentTransform, isParented);
  //                  quadRepresentation.fLegsRep.InitHeadLights(mech);
  //                  quadRepresentation.fLegsRep.LegsRep.transform.SetParent(__instance.transform);
  //                  quadRepresentation.fLegsRep.LegsRep.transform.localPosition = new Vector3(0f, 0f, info.quadVisualInfo.BodyLength / 2f);
  //                  __instance.LeftArmAttach = quadRepresentation.fLegsRep.LegsRep.LeftLegAttach;
  //                  __instance.RightArmAttach = quadRepresentation.fLegsRep.LegsRep.RightLegAttach;
  //                  __instance.vfxLeftArmTransform = quadRepresentation.fLegsRep.LegsRep.vfxLeftLegTransform;
  //                  __instance.vfxRightArmTransform = quadRepresentation.fLegsRep.LegsRep.vfxRightLegTransform;
  //                  __instance.vfxLeftShoulderTransform = quadRepresentation.fLegsRep.LegsRep.vfxLeftLegTransform;
  //                  __instance.vfxRightShoulderTransform = quadRepresentation.fLegsRep.LegsRep.vfxRightLegTransform;
  //                  __instance.leftArmDestructible = quadRepresentation.fLegsRep.LegsRep.leftLegDestructible;
  //                  __instance.rightArmDestructible = quadRepresentation.fLegsRep.LegsRep.rightLegDestructible;
  //                }
  //              }
  //            }
  //          }
  //        }
  //      }
  //      MechRepresentation_InitAlternate.Postfix(__instance,mech,parentTransform,isParented, info);
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return;
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentation))]
  //[HarmonyPatch("SetupJumpJets")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Mech), typeof(MechDef), typeof(DataManager) })]
  //public static class MechRepresentation_SetupJumpJets {
  //  public static void SetupJumpJets(this QuadLegsRepresentation quadLegsRepresentation, Mech mech, MechDef mechDef, DataManager dataManager) {
  //    UnitCustomInfo info = mech.GetCustomInfo();
  //    if (info == null) { return; }
  //    string PrefabBase = info.quadVisualInfo.FLegsPrefabBase;
  //    string id1 = string.Format("chrPrfComp_{0}_leftleg_jumpjet", (object)PrefabBase);
  //    string id2 = string.Format("chrPrfComp_{0}_rightleg_jumpjet", (object)PrefabBase);
  //    Log.TWL(0, "QuadLegsRepresentation.SetupJumpJets");
  //    List<JumpjetRepresentation> jumpjetReps = Traverse.Create(quadLegsRepresentation.LegsRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
  //    jumpjetReps.Clear();
  //    GameObject gameObject1 = dataManager.PooledInstantiate(id1, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    GameObject gameObject2 = dataManager.PooledInstantiate(id2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    Log.WL(1, id1+":"+(gameObject1==null?"null":"not null"));
  //    Log.WL(1, id2 + ":" + (gameObject2 == null ? "null" : "not null"));
  //    if (gameObject1 != null) {
  //      JumpjetRepresentation component = gameObject1.GetComponent<JumpjetRepresentation>();
  //      if (mech != null)
  //        component.Init((ICombatant)mech, quadLegsRepresentation.LegsRep.LeftLegAttach, true, false, quadLegsRepresentation.LegsRep.name);
  //      jumpjetReps.Add(component);
  //    }
  //    if (gameObject2 != null) {
  //      JumpjetRepresentation component1 = gameObject2.GetComponent<JumpjetRepresentation>();
  //      if (mech != null)
  //        component1.Init((ICombatant)mech, quadLegsRepresentation.LegsRep.RightLegAttach, true, false, quadLegsRepresentation.LegsRep.name);
  //      jumpjetReps.Add(component1);
  //    }
  //  }
  //  public static void SetupJumpJets(this QuadRepresentation quadRepresentation, Mech mech, MechDef mechDef, DataManager dataManager) {
  //    UnitCustomInfo info = mech.GetCustomInfo();
  //    if (info == null) { return; }
  //    string PrefabBase = info.quadVisualInfo.RLegsPrefabBase;
  //    string id1 = string.Format("chrPrfComp_{0}_leftleg_jumpjet", (object)PrefabBase);
  //    string id2 = string.Format("chrPrfComp_{0}_rightleg_jumpjet", (object)PrefabBase);
  //    Log.TWL(0, "QuadRepresentation.SetupJumpJets");
  //    List<JumpjetRepresentation> jumpjetReps = Traverse.Create(quadRepresentation.mechRep).Field<List<JumpjetRepresentation>>("jumpjetReps").Value;
  //    jumpjetReps.Clear();
  //    GameObject gameObject1 = dataManager.PooledInstantiate(id1, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    GameObject gameObject2 = dataManager.PooledInstantiate(id2, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
  //    Log.WL(1, id1 + ":" + (gameObject1 == null ? "null" : "not null"));
  //    Log.WL(1, id2 + ":" + (gameObject2 == null ? "null" : "not null"));
  //    if (gameObject1 != null) {
  //      JumpjetRepresentation component = gameObject1.GetComponent<JumpjetRepresentation>();
  //      if (mech != null)
  //        component.Init((ICombatant)mech, quadRepresentation.mechRep.LeftLegAttach, true, false, quadRepresentation.mechRep.name);
  //      jumpjetReps.Add(component);
  //    }
  //    if (gameObject2 != null) {
  //      JumpjetRepresentation component1 = gameObject2.GetComponent<JumpjetRepresentation>();
  //      if (mech != null)
  //        component1.Init((ICombatant)mech, quadRepresentation.mechRep.RightLegAttach, true, false, quadRepresentation.mechRep.name);
  //      jumpjetReps.Add(component1);
  //    }
  //  }
  //  public static bool Prefix(MechRepresentation __instance, Mech mech, MechDef mechDef, DataManager dataManager) {
  //    try {
  //      QuadLegsRepresentation quadLegsRepresentation = __instance.GetComponent<QuadLegsRepresentation>();
  //      if (quadLegsRepresentation != null) {
  //        quadLegsRepresentation.SetupJumpJets(mech, mechDef, dataManager);
  //        return false;
  //      }
  //      QuadRepresentation quadRepresentation = __instance.GetComponent<QuadRepresentation>();
  //      if (quadRepresentation != null) {
  //        quadRepresentation.SetupJumpJets(mech, mechDef, dataManager);
  //        return false;
  //      }
  //      return true;
  //    } catch (Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //      return true;
  //    }
  //  }
  //}
  //[HarmonyPatch(typeof(MechRepresentationSimGame))]
  //[HarmonyPatch("CollapseLocation")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(int), typeof(bool) })]
  //public static class MechRepresentationSimGame_CollapseLocation {
  //  public static bool Prefix(MechRepresentationSimGame __instance, int location, bool isDestroyed) {
  //    try {
  //      QuadRepresentation quadBody = __instance.GetComponentInChildren<QuadRepresentation>(true);
  //      ChassisLocations chassisLocations = (ChassisLocations)location;
  //      if (quadBody != null) {
  //        MechDestructibleObject destructibleObject = null;
  //        switch (chassisLocations) {
  //          case ChassisLocations.RightTorso: return false;
  //          case ChassisLocations.LeftTorso: return false;
  //          case ChassisLocations.CenterTorso: return false;
  //          case ChassisLocations.Head: return false;
  //          case ChassisLocations.LeftArm:
  //            destructibleObject = __instance.leftArmDestructible;
  //            if ((UnityEngine.Object)destructibleObject != (UnityEngine.Object)null)
  //              destructibleObject.CollapseSwap(isDestroyed);
  //          return false;
  //          case ChassisLocations.RightArm:
  //            destructibleObject = __instance.rightArmDestructible;
  //            if ((UnityEngine.Object)destructibleObject != (UnityEngine.Object)null)
  //              destructibleObject.CollapseSwap(isDestroyed);
  //          break;
  //        }
  //      }
  //    } catch(Exception e) {
  //      Log.TWL(0, e.ToString(), true);
  //    }
  //    return true;
  //  }
  //}
}