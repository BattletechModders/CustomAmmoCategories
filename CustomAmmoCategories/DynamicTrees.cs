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
using BattleTech.Rendering.Trees;
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(QuadTreeData)), HarmonyPatch("Insert")]
  public static class QuadTreeData_Insert {
    public static void Postfix(QuadTreeData __instance, ref Matrix4x4 trs, ref Vector3 pos, Quaternion rot, ref Vector3 scale) {
      DynamicTreesHelper.OnInsert(__instance, ref trs, ref pos, rot, ref scale);
    }
  }
  [HarmonyPatch(typeof(QuadTreeData)), HarmonyPatch("SetupFullArray")]
  public static class QuadTreeData_SetupFullArray {
    public static void Prefix(QuadTreeData __instance) {
      DynamicTreesHelper.SetupFullArray();
    }
  }
  [HarmonyPatch(typeof(QuadTreeData)), HarmonyPatch("SetupComputeBuffer")]
  public static class QuadTreeData_SetupComputeBuffer {
    public static void Prefix(QuadTreeData __instance) {
      DynamicTreesHelper.SetupComputeBuffer();
    }
  }
  [HarmonyPatch(typeof(QuadTreeData)), HarmonyPatch("GenerateCombinedMesh")]
  public static class QuadTreeData_GenerateCombinedMesh {
    public static void Prefix(QuadTreeData __instance) {
      DynamicTreesHelper.GenerateCombinedMesh();
    }
  }
  [HarmonyPatch(typeof(QuadTreeData)), HarmonyPatch("GenerateMesh")]
  public static class QuadTreeData_GenerateMesh {
    public static void Prefix(QuadTreeData __instance) {
      DynamicTreesHelper.GenerateMesh();
    }
  }
  public class CACDynamicTree {
    public static CACDynamicTree currentBurningIniting = null;
    public static List<CACDynamicTree> allCACTrees = new List<CACDynamicTree>();
    public QuadTreeData quadTreeData;
    public QuadTreeData burnedTreeData { get; set; }
    public bool deleted;
    public int burnedTransformIndex;
    public int burnedMatrixIndex1;
    public int burnedMatrixIndex2;
    public int transformIndex;
    public int matrixIndex1;
    public int matrixIndex2;
    public QuadTreeTransform transform;
    public bool isTree;
    public CACDynamicTree(QuadTreeData qd,int ti,int mi1,int mi2, QuadTreeTransform tr,bool tree) {
      this.quadTreeData = qd;
      this.transformIndex = ti;
      this.matrixIndex1 = mi1;
      this.matrixIndex2 = mi2;
      this.deleted = false;
      this.transform = new QuadTreeTransform(tr.position,tr.rotation,tr.scale);
      this.isTree = tree;
      this.burnedTreeData = null;
      this.burnedTransformIndex = -1;
      this.burnedMatrixIndex1 = -1;
      this.burnedMatrixIndex2 = -1;
      CACDynamicTree.allCACTrees.Add(this);
    }
    public void InitBurned(QuadTreeData qd, int ti, int mi1, int mi2) {
      this.burnedTreeData = qd;
      this.burnedTransformIndex = ti;
      this.burnedMatrixIndex1 = mi1;
      this.burnedMatrixIndex2 = mi2;
    }
    public bool hideOriginal() {
      List<QuadTreeTransform> transforms = this.quadTreeData.transformList;
      var matrixLists = this.quadTreeData.matrixList;
      if (this.transformIndex >= transforms.Count) {
        Log.Combat?.WL(1,"warning transform out of bounds");
        return false;
      }
      if (this.matrixIndex1 >= matrixLists.Count) {
        Log.Combat?.WL(1, "warning matrixIndex1 out of bounds");
        return false;
      }
      List<Matrix4x4> matrixList = matrixLists[this.matrixIndex1].trsList;
      if (this.matrixIndex2 >= matrixList.Count) {
        Log.Combat?.WL(1, "warning matrixIndex2 out of bounds " + this.matrixIndex2 + " /" + matrixList.Count);
        return false;
      }
      transforms[this.transformIndex].scale.Set(0f, 0f, 0f);
      matrixList[matrixIndex2] = Matrix4x4.TRS(transforms[this.transformIndex].position, transforms[this.transformIndex].rotation, transforms[this.transformIndex].scale);
      return true;
    }
    public bool hideBurned() {
      if((this.burnedTreeData == null)||(this.burnedTransformIndex < 0)||(this.burnedMatrixIndex1 < 0)||(this.burnedMatrixIndex2 < 0)) {
        Log.Combat?.WL(1, "burned not inited");
        return false;
      }
      List<QuadTreeTransform> transforms = this.burnedTreeData.transformList;
      var matrixLists = this.burnedTreeData.matrixList;
      if (this.burnedTransformIndex >= transforms.Count) {
        Log.Combat?.WL(1, "warning burnedTransform out of bounds");
        return false;
      }
      if (this.burnedMatrixIndex1 >= matrixLists.Count) {
        Log.Combat?.WL(1, "warning burnedMatrixIndex1 out of bounds");
        return false;
      }
      List<Matrix4x4> matrixList = matrixLists[this.burnedMatrixIndex1].trsList;
      if (this.burnedMatrixIndex2 >= matrixList.Count) {
        Log.Combat?.WL(1, "warning burnedMatrixIndex2 out of bounds " + this.burnedMatrixIndex2 + " /" + matrixList.Count);
        return false;
      }
      transforms[this.burnedTransformIndex].scale.Set(0f, 0f, 0f);
      matrixList[burnedMatrixIndex2] = Matrix4x4.TRS(transforms[this.burnedTransformIndex].position, transforms[this.burnedTransformIndex].rotation, transforms[this.burnedTransformIndex].scale);
      return true;
    }
    public bool showBurned() {
      if ((this.burnedTreeData == null) || (this.burnedTransformIndex < 0) || (this.burnedMatrixIndex1 < 0) || (this.burnedMatrixIndex2 < 0)) {
        Log.Combat?.WL(1, "burned not inited\n");
        return false;
      }
      List<QuadTreeTransform> transforms = this.burnedTreeData.transformList;
      var matrixLists = this.burnedTreeData.matrixList;
      if (this.burnedTransformIndex >= transforms.Count) {
        Log.Combat?.WL(1, "warning burnedTransform out of bounds");
        return false;
      }
      if (this.burnedMatrixIndex1 >= matrixLists.Count) {
        Log.Combat?.WL(1, "warning burnedMatrixIndex1 out of bounds");
        return false;
      }
      List<Matrix4x4> matrixList = matrixLists[this.burnedMatrixIndex1].trsList;
      if (this.burnedMatrixIndex2 >= matrixList.Count) {
        Log.Combat?.WL(1, "warning burnedMatrixIndex2 out of bounds " + this.burnedMatrixIndex2 + " /" + matrixList.Count);
        return false;
      }
      float scaleFactor = CustomAmmoCategories.Settings.BurnedTrees.BurnedTreeScale;
      transforms[this.burnedTransformIndex].scale.Set(this.transform.scale.x*scaleFactor, this.transform.scale.y * scaleFactor, this.transform.scale.z * scaleFactor);
      Vector3 vector3 = new Vector3(transforms[this.burnedTransformIndex].scale.x * scaleFactor, transforms[this.burnedTransformIndex].scale.y * scaleFactor, transforms[this.burnedTransformIndex].scale.z * scaleFactor);
      matrixList[burnedMatrixIndex2] = Matrix4x4.TRS(transforms[this.burnedTransformIndex].position, transforms[this.burnedTransformIndex].rotation, transforms[this.burnedTransformIndex].scale);
      return true;
    }
    public List<QuadTreeData> delTree() {
      List<QuadTreeData> result = new List<QuadTreeData>();
      if (this.deleted) { return result; }
      QuadTreePrototype prototype = this.quadTreeData.prototype;
      if ((prototype.name.Contains("Tree") == false) && (prototype.name.Contains("Bush") == false)) { return result; };
      List<QuadTreeTransform> transforms = this.quadTreeData.transformList;
      var matrixLists = this.quadTreeData.matrixList;
      Log.Combat?.WL(1, "Removing tree " + prototype.name + " from ti:" + this.transformIndex + "/" + transforms.Count + " mi:" + this.matrixIndex1 + "/"+ matrixLists .Count+ " mi2:" + this.matrixIndex2);
      if (this.hideOriginal()) {
        result.Add(this.quadTreeData);
      }
      if (this.isTree 
        && (CustomAmmoCategories.Settings.DontShowBurnedTrees == false)
        &&(CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary == false)
        && (DynamicTreesHelper.RemoveTrees == false)
      ) {
        Log.Combat?.WL(1, "add tree same position as burned");
        if (this.showBurned()) {
          result.Add(this.burnedTreeData);
        }
      }
      this.deleted = true;
      return result;
    }
    public static void redrawTrees(HashSet<QuadTreeData> TreeDataHolders) {
      foreach(QuadTreeData quadTreeData in TreeDataHolders) {
        QuadTreePrototype prototype = quadTreeData.prototype;
        Log.Combat?.WL(0, "Redrawing tree data for:" + prototype.name);
        quadTreeData.Cleanup();
      }
    }
  }
  public static class DynamicTreesHelper {
    public static bool RemoveTrees { get; set; } = false;
    //public static FieldInfo transformListField = null;
    //public static FieldInfo matrixListField = null;
    //public static FieldInfo prototypeField = null;
    //public static MethodInfo matrixCountProperty = null;
    //public static MethodInfo trsListPropertyGet = null;
    //public static MethodInfo trsListPropertySet = null;
    //public static MethodInfo quadTreeDataCleanup = null;
    public static int counter = 0;
    public static readonly int maxCount = 1023;
    public static QuadTree _quadTree = null;
    public static GameObject burnedTreePrefab = null;
    public static QuadTreePrototype burnedTreePrototype = null;
    public static Dictionary<QuadTreePrototype, QuadTreePrototype> burnedTreePrototypes = new Dictionary<QuadTreePrototype, QuadTreePrototype>();
    public static bool NoNeedInsert = false;
    //public static object justAddedBurnedTreeData = null;
    public static void Clean() {
      DynamicTreesHelper.NoNeedInsert = false;
      DynamicTreesHelper._quadTree = null;
      DynamicTreesHelper.burnedTreePrefab = null;
      DynamicTreesHelper.burnedTreePrototypes.Clear();
    }
    public static void SetupFullArray() {
      Log.Combat?.WL(0,"QuadTreeData.SetupFullArray");
    }
    public static void SetupComputeBuffer() {
      Log.Combat?.WL(0, "QuadTreeData.SetupComputeBuffer");
    }
    public static void GenerateCombinedMesh() {
      Log.Combat?.WL(0, "QuadTreeData.GenerateCombinedMesh");
    }
    public static void GenerateMesh() {
      Log.Combat?.WL(0, "QuadTreeData.GenerateMesh");
    }
    public static void clearTrees() {
      Log.Combat?.WL(0, "DynamicTreesHelper.clearTrees");
      if (DynamicTreesHelper._quadTree == null) {
        Log.Combat?.WL(1, "_quadTree is null");
        return;
      }
      var quadTreeNodes = DynamicTreesHelper._quadTree.quadTreeNodes;
      Log.Combat?.WL(1, "quadTreeNodes.Count = " + quadTreeNodes.Count);
      //int treesCount = 0;
      for (int t = 0; t < quadTreeNodes.Count; ++t) {
        if (quadTreeNodes[t] == null) { continue; }
        var children = quadTreeNodes[t].children;
        var dataDictionary = quadTreeNodes[t].dataDictionary;
        Log.Combat?.WL(3, "children = " + ((children == null) ? "null" : children.Length.ToString()));
        Log.Combat?.WL(3, "dataDictionary = " + ((dataDictionary == null) ? "null" : dataDictionary.Count.ToString()));
        if (dataDictionary == null) { continue; }
        if (children == null) { continue; }
        if (dataDictionary.Count > 0) {
          foreach(var idataDictionary in dataDictionary) {
            var prototype = idataDictionary.Key;
            var quadTreeData = idataDictionary.Value;
            if (prototype == null) { continue; }
            if (quadTreeData == null) { continue; }
            List<QuadTreeTransform> transformList = quadTreeData.transformList;
            Log.Combat?.WL(4, prototype.name + ":" + transformList.Count);
            var matrixLists = quadTreeData.matrixList;
            Log.Combat?.WL(5, "matrix arrays:" + matrixLists.Count);
            for (int mi = 0; mi < matrixLists.Count; ++mi) {
              List<Matrix4x4> matrixList = matrixLists[mi].trsList;;
              Log.Combat?.WL(6, "matrix array[" + mi + "]:" + matrixList.Count);
            }
          };
        }
      }
    }
    public static void OnInsert(QuadTreeData __instance,ref Matrix4x4 trs, ref Vector3 pos, Quaternion rot, ref Vector3 scale) {
      if (DynamicMapHelper.mapMetaData == null) { return; }
      ++counter;
      List<QuadTreeTransform> transformList = null;
      List<QuadTreeArray> matrixList = null;
      int ti = -1;
      int mi1 = -1;
      int mi2 = -1;
      if (DynamicTreesHelper.NoNeedInsert == true) {
        if(CACDynamicTree.currentBurningIniting != null) {
          transformList = __instance.transformList;
          if (transformList == null) {
            Log.Combat?.WL(0, "can't get transform list");
          }
          ti = transformList.Count - 1;
          matrixList = __instance.matrixList;
          mi1 = matrixList.Count - 1;
          if (mi1 < 0) {
            Log.Combat?.WL(0, "QuadTreeData.OnInsert mi(" + mi1 + ") < 0");
            return;
          }
          mi2 = matrixList[mi1].count - 1;
          CACDynamicTree.currentBurningIniting.InitBurned(__instance, ti, mi1, mi2);
          CACDynamicTree.currentBurningIniting = null;
          return;
        }
        return;
      };
      QuadTreePrototype prototype = __instance.prototype;
      bool isTree = prototype.name.Contains("Tree");
      if ((isTree == false) && (prototype.name.Contains("Bush") == false)) { return; };
      int x = DynamicMapHelper.mapMetaData.GetXIndex(pos.x);
      int z = DynamicMapHelper.mapMetaData.GetZIndex(pos.z);
      if ((x < 0) || (x >= DynamicMapHelper.mapMetaData.GetXLimit()) || (z < 0) || (z >= DynamicMapHelper.mapMetaData.GetZLimit())) {
        return;
      }
      MapTerrainDataCellEx cell = DynamicMapHelper.mapMetaData.mapTerrainDataCells[z, x] as MapTerrainDataCellEx;
      if (cell == null) { return; }
      transformList = __instance.transformList;
      if (transformList == null) {
        Log.Combat?.WL(0, "can't get transform list");
      }
      ti = transformList.Count - 1;
      matrixList = __instance.matrixList;
      mi1 = matrixList.Count - 1;
      if (mi1 < 0) {
        Log.Combat?.WL(0, "QuadTreeData.OnInsert mi(" + mi1+") < 0");
        return;
      }
      mi2 = matrixList[mi1].count - 1;
      cell.trees.Add(new CACDynamicTree(__instance,ti,mi1,mi2, transformList[ti], isTree));
      return;
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(RenderTrees))]
  [HarmonyPatch("InitQuadTree")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class RenderTrees_InitQuadTree {
    private static void Postfix(RenderTrees __instance) {
      Log.Combat?.TWL(0,"RenderTrees.InitQuadTree postfix");
      DynamicTreesHelper.NoNeedInsert = true;
      try {
        QuadTreePrototype[] quadTreePrototypes = __instance.quadTreePrototypes;
        DynamicTreesHelper.burnedTreePrefab = CACMain.Core.findPrefab(CustomAmmoCategories.Settings.BurnedTrees.Mesh);
        if (DynamicTreesHelper.burnedTreePrefab == null) {
          Log.Combat?.WL(1, CustomAmmoCategories.Settings.BurnedTrees.Mesh + " - fail");
          DynamicTreesHelper.burnedTreePrototypes.Clear();
        } else {
          Log.Combat?.WL(1, CustomAmmoCategories.Settings.BurnedTrees.Mesh + " components:");
          foreach (var component in DynamicTreesHelper.burnedTreePrefab.GetComponents(typeof(Component))) {
            Log.Combat?.WL(2, component.GetType().ToString());
          }
          MeshFilter meshFilter = DynamicTreesHelper.burnedTreePrefab.GetComponentInChildren<MeshFilter>();
          Texture2D _BumpMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.BumpMap);
          Texture2D _MainTex = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.MainTex);
          Texture2D _OcculusionMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.OcculusionMap);
          Texture2D _Transmission = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.Transmission);
          Texture2D _MetallicGlossMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.MetallicGlossMap);

          if (meshFilter == null) {
            Log.Combat?.WL(1,"meshFilter is null");
          } else {
            Log.Combat?.WL(1, "meshFilter is not null " + meshFilter.name + ":" + meshFilter.sharedMesh.name);
            TreePrototype prototypeToCopy = null;
            Log.Combat?.WL(1, "active terrain unity tree prototypes:");
            foreach (TreePrototype prototype in Terrain.activeTerrain.terrainData.treePrototypes) {
              Log.Combat?.WL(2, prototype.prefab.name);
              if (prototype.prefab.name.Contains("Tree") == false) { continue; }
              prototypeToCopy = prototype; break;
            }
            if (prototypeToCopy != null) {
              QuadTreePrototype newPrototype = new QuadTreePrototype(prototypeToCopy);
              string protName = prototypeToCopy.prefab.name + "_burned";
              newPrototype.name = protName;
              Log.Combat?.WL(2, newPrototype.name + " lods count: " + newPrototype.meshes.Length);
              for (int lod = 0; lod < newPrototype.meshes.Length; ++lod) {
                newPrototype.meshes[lod] = meshFilter.sharedMesh;
                newPrototype.materials[lod] = UnityEngine.Object.Instantiate(newPrototype.materials[lod]);
                if (_BumpMap != null) newPrototype.materials[lod].SetTexture("_BumpMap", _BumpMap);
                if (_MainTex != null) newPrototype.materials[lod].SetTexture("_MainTex", _MainTex);
                if (_OcculusionMap != null) newPrototype.materials[lod].SetTexture("_OcculusionMap", _OcculusionMap);
                if (_Transmission != null) newPrototype.materials[lod].SetTexture("_Transmission", _Transmission);
                if (_MetallicGlossMap != null) newPrototype.materials[lod].SetTexture("_MetallicGlossMap", _MetallicGlossMap);
                newPrototype.materials[lod].color = Color.black;
                newPrototype.materialsNoInstance[lod] = UnityEngine.Object.Instantiate(newPrototype.materialsNoInstance[lod]);
                newPrototype.materialsNoInstance[lod].color = Color.black;
                newPrototype.combinedMaterial[lod] = UnityEngine.Object.Instantiate(newPrototype.combinedMaterial[lod]);
                newPrototype.combinedMaterial[lod].color = Color.black;
              }
              DynamicTreesHelper.burnedTreePrototype = newPrototype;
              Log.Combat?.WL(1, "initing burned success");
              foreach (QuadTreePrototype prototype in quadTreePrototypes) {
                if (DynamicTreesHelper.burnedTreePrototypes.ContainsKey(prototype) == false) { 
                  DynamicTreesHelper.burnedTreePrototypes.Add(prototype, newPrototype);
                } else {
                  DynamicTreesHelper.burnedTreePrototypes[prototype] = newPrototype;
                }
              }
              QuadTreePrototype[] newTreePrototypes = new QuadTreePrototype[quadTreePrototypes.Length + 1];
              quadTreePrototypes.CopyTo(newTreePrototypes, 0);
              newTreePrototypes[newTreePrototypes.Length - 1] = newPrototype;
              __instance.quadTreePrototypes = newTreePrototypes;
            } else {
              Log.Combat?.WL(1, "no trees on terrain");
            }
          }
        }
        QuadTree quadTree = __instance._quadTree;
        DynamicTreesHelper._quadTree = quadTree;
        quadTreePrototypes = __instance.quadTreePrototypes;
        if (quadTreePrototypes != null) {
          Log.Combat?.WL(0, "avaible tree prototypes (" + quadTreePrototypes.Length + "):");
          foreach (QuadTreePrototype prototype in quadTreePrototypes) {
            Log.Combat?.WL(1, prototype.name);
          }
          if ((DynamicTreesHelper.burnedTreePrototype != null) && (CustomAmmoCategories.Settings.DontShowBurnedTrees == false) && (CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary == false)) {
            Log.Combat?.WL(0, "Installing burned trees. All trees-like objects count: (" + CACDynamicTree.allCACTrees.Count + "):");
            int counter = 0;
            foreach (CACDynamicTree cacTree in CACDynamicTree.allCACTrees) {
              if (cacTree.isTree == false) { continue; };
              CACDynamicTree.currentBurningIniting = cacTree;
              Matrix4x4 trs = Matrix4x4.TRS(cacTree.transform.position, cacTree.transform.rotation, cacTree.transform.scale);
              DynamicTreesHelper._quadTree.Insert(DynamicTreesHelper.burnedTreePrototype, trs, cacTree.transform.position, cacTree.transform.rotation, cacTree.transform.scale);
              CACDynamicTree.currentBurningIniting = null;
              //cacTree.hideOriginal();
              cacTree.hideBurned();
              ++counter;
            }
            Log.Combat?.WL(0, "All trees count:" + counter);
          }
        } else {
          Log.Combat?.WL(0, "quadTreePrototypes array is null");
        }
        if (quadTree != null) {
          var quadTreeNodes = quadTree.quadTreeNodes;
          Log.Combat?.WL(1, "quadTreeNodes.Count = " + quadTreeNodes.Count);
          //int treesCount = 0;
          for (int t = 0; t < quadTreeNodes.Count; ++t) {
            var children = quadTreeNodes[t].children;
            var dataDictionary = quadTreeNodes[t].dataDictionary;
            Log.Combat?.WL(3, "children = " + ((children == null) ? "null" : children.Length.ToString()));
            Log.Combat?.WL(3, "dataDictionary = " + ((dataDictionary == null) ? "null" : dataDictionary.Count.ToString()));
            if (dataDictionary == null) { continue; }
            if (children == null) { continue; }
            if (dataDictionary.Count > 0) {
              foreach (var idataDictionary in dataDictionary) {
                var prototype = idataDictionary.Key;
                var quadTreeData = idataDictionary.Value;
                List<QuadTreeTransform> transformList = quadTreeData.transformList;
                Log.Combat?.WL(4, prototype.name + ":" + transformList.Count);
                var matrixLists = quadTreeData.matrixList;
                Log.Combat?.WL(5, "matrix arrays:" + matrixLists.Count);
                for (int mi = 0; mi < matrixLists.Count; ++mi) {
                  List<Matrix4x4> matrixList = matrixLists[mi].trsList;
                  Log.Combat?.WL(6, "matrix array[" + mi + "]:" + matrixList.Count);
                }
              };
            }
          }
        } else {
          Log.Combat?.WL(0, "quadTree is null");
        }
      } catch (Exception e) {
        Log.Combat?.WL(0, "Fail to setup burned trees. Burned trees will not be shown: " + e.ToString(),true);
        UIManager.logger.LogException(e);
        CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary = true;
      }
    }
  }
}
