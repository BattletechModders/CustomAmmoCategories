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
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public class CACDynamicTree {
    public static CACDynamicTree currentBurningIniting = null;
    public static List<CACDynamicTree> allCACTrees = new List<CACDynamicTree>();
    public object quadTreeData;
    public object burnedTreeData;
    public bool deleted;
    public int burnedTransformIndex;
    public int burnedMatrixIndex1;
    public int burnedMatrixIndex2;
    public int transformIndex;
    public int matrixIndex1;
    public int matrixIndex2;
    public QuadTreeTransform transform;
    public bool isTree;
    public CACDynamicTree(object qd,int ti,int mi1,int mi2, QuadTreeTransform tr,bool tree) {
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
    public void InitBurned(object qd, int ti, int mi1, int mi2) {
      this.burnedTreeData = qd;
      this.burnedTransformIndex = ti;
      this.burnedMatrixIndex1 = mi1;
      this.burnedMatrixIndex2 = mi2;
    }
    public bool hideOriginal() {
      List<QuadTreeTransform> transforms = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(this.quadTreeData);
      IList matrixLists = (IList)DynamicTreesHelper.matrixListField.GetValue(this.quadTreeData);
      if (this.transformIndex >= transforms.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning transform out of bounds\n");
        return false;
      }
      if (this.matrixIndex1 >= matrixLists.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning matrixIndex1 out of bounds\n");
        return false;
      }
      List<Matrix4x4> matrixList = (List<Matrix4x4>)DynamicTreesHelper.trsListPropertyGet.Invoke(matrixLists[this.matrixIndex1], new object[0] { });
      if (this.matrixIndex2 >= matrixList.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning matrixIndex2 out of bounds " + this.matrixIndex2 + " /" + matrixList.Count + "\n");
        return false;
      }
      transforms[this.transformIndex].scale.Set(0f, 0f, 0f);
      matrixList[matrixIndex2] = Matrix4x4.TRS(transforms[this.transformIndex].position, transforms[this.transformIndex].rotation, transforms[this.transformIndex].scale);
      return true;
    }
    public bool hideBurned() {
      if((this.burnedTreeData == null)||(this.burnedTransformIndex < 0)||(this.burnedMatrixIndex1 < 0)||(this.burnedMatrixIndex2 < 0)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" burned not inited\n");
        return false;
      }
      List<QuadTreeTransform> transforms = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(this.burnedTreeData);
      IList matrixLists = (IList)DynamicTreesHelper.matrixListField.GetValue(this.burnedTreeData);
      if (this.burnedTransformIndex >= transforms.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedTransform out of bounds\n");
        return false;
      }
      if (this.burnedMatrixIndex1 >= matrixLists.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedMatrixIndex1 out of bounds\n");
        return false;
      }
      List<Matrix4x4> matrixList = (List<Matrix4x4>)DynamicTreesHelper.trsListPropertyGet.Invoke(matrixLists[this.burnedMatrixIndex1], new object[0] { });
      if (this.burnedMatrixIndex2 >= matrixList.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedMatrixIndex2 out of bounds " + this.burnedMatrixIndex2 + " /" + matrixList.Count + "\n");
        return false;
      }
      transforms[this.burnedTransformIndex].scale.Set(0f, 0f, 0f);
      matrixList[burnedMatrixIndex2] = Matrix4x4.TRS(transforms[this.burnedTransformIndex].position, transforms[this.burnedTransformIndex].rotation, transforms[this.burnedTransformIndex].scale);
      return true;
    }
    public bool showBurned() {
      if ((this.burnedTreeData == null) || (this.burnedTransformIndex < 0) || (this.burnedMatrixIndex1 < 0) || (this.burnedMatrixIndex2 < 0)) {
        CustomAmmoCategoriesLog.Log.LogWrite(" burned not inited\n");
        return false;
      }
      List<QuadTreeTransform> transforms = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(this.burnedTreeData);
      IList matrixLists = (IList)DynamicTreesHelper.matrixListField.GetValue(this.burnedTreeData);
      if (this.burnedTransformIndex >= transforms.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedTransform out of bounds\n");
        return false;
      }
      if (this.burnedMatrixIndex1 >= matrixLists.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedMatrixIndex1 out of bounds\n");
        return false;
      }
      List<Matrix4x4> matrixList = (List<Matrix4x4>)DynamicTreesHelper.trsListPropertyGet.Invoke(matrixLists[this.burnedMatrixIndex1], new object[0] { });
      if (this.burnedMatrixIndex2 >= matrixList.Count) {
        CustomAmmoCategoriesLog.Log.LogWrite(" warning burnedMatrixIndex2 out of bounds " + this.burnedMatrixIndex2 + " /" + matrixList.Count + "\n");
        return false;
      }
      float scaleFactor = CustomAmmoCategories.Settings.BurnedTrees.BurnedTreeScale;
      transforms[this.burnedTransformIndex].scale.Set(this.transform.scale.x*scaleFactor, this.transform.scale.y * scaleFactor, this.transform.scale.z * scaleFactor);
      Vector3 vector3 = new Vector3(transforms[this.burnedTransformIndex].scale.x * scaleFactor, transforms[this.burnedTransformIndex].scale.y * scaleFactor, transforms[this.burnedTransformIndex].scale.z * scaleFactor);
      matrixList[burnedMatrixIndex2] = Matrix4x4.TRS(transforms[this.burnedTransformIndex].position, transforms[this.burnedTransformIndex].rotation, transforms[this.burnedTransformIndex].scale);
      return true;
    }
    public List<object> delTree() {
      List<object> result = new List<object>();
      if (this.deleted) { return result; }
      QuadTreePrototype prototype = (QuadTreePrototype)DynamicTreesHelper.prototypeField.GetValue(this.quadTreeData);
      if ((prototype.name.Contains("Tree") == false) && (prototype.name.Contains("Bush") == false)) { return result; };
      List<QuadTreeTransform> transforms = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(this.quadTreeData);
      IList matrixLists = (IList)DynamicTreesHelper.matrixListField.GetValue(this.quadTreeData);
      CustomAmmoCategoriesLog.Log.LogWrite("Removing tree " + prototype.name + " from ti:" + this.transformIndex + "/" + transforms.Count + " mi:" + this.matrixIndex1 + "/"+ matrixLists .Count+ " mi2:" + this.matrixIndex2 + "\n");
      if (this.hideOriginal()) {
        result.Add(this.quadTreeData);
      }
      if (this.isTree 
        && (CustomAmmoCategories.Settings.DontShowBurnedTrees == false)
        &&(CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary == false)
        && (DynamicTreesHelper.RemoveTrees == false)
      ) {
        CustomAmmoCategoriesLog.Log.LogWrite(" add tree same position as burned\n");
        if (this.showBurned()) {
          result.Add(this.burnedTreeData);
        }
      }
      this.deleted = true;
      return result;
    }
    public static void redrawTrees(HashSet<object> TreeDataHolders) {
      foreach(object quadTreeData in TreeDataHolders) {
        QuadTreePrototype prototype = (QuadTreePrototype)DynamicTreesHelper.prototypeField.GetValue(quadTreeData);
        CustomAmmoCategoriesLog.Log.LogWrite("Redrawing tree data for:"+prototype.name+"\n");
        DynamicTreesHelper.quadTreeDataCleanup.Invoke(quadTreeData, new object[0] { });
      }
    }
  }
  public static class DynamicTreesHelper {
    public static bool RemoveTrees { get; set; } = false;
    public static FieldInfo transformListField = null;
    public static FieldInfo matrixListField = null;
    public static FieldInfo prototypeField = null;
    public static MethodInfo matrixCountProperty = null;
    public static MethodInfo trsListPropertyGet = null;
    public static MethodInfo trsListPropertySet = null;
    public static MethodInfo quadTreeDataCleanup = null;
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
    public static bool SetupFullArray() {
      CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.SetupFullArray\n");
      return true;
    }
    public static bool SetupComputeBuffer() {
      CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.SetupComputeBuffer\n");
      return true;
    }
    public static bool GenerateCombinedMesh() {
      CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.GenerateCombinedMesh\n");
      return true;
    }
    public static bool GenerateMesh() {
      CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.GenerateMesh\n");
      return true;
    }
    public static void clearTrees() {
      CustomAmmoCategoriesLog.Log.LogWrite("DynamicTreesHelper.clearTrees\n");
      if (DynamicTreesHelper._quadTree == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" _quadTree is null\n");
        return;
      }
      IList quadTreeNodes = (IList)typeof(QuadTree).GetField("quadTreeNodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DynamicTreesHelper._quadTree);
      CustomAmmoCategoriesLog.Log.LogWrite(" quadTreeNodes.Count = " + quadTreeNodes.Count + "\n");
      Type QuadTreeNode = quadTreeNodes[0].GetType();
      FieldInfo childrenField = QuadTreeNode.GetField("children", BindingFlags.Instance | BindingFlags.NonPublic);
      FieldInfo dataDictionaryField = QuadTreeNode.GetField("dataDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
      //int treesCount = 0;
      for (int t = 0; t < quadTreeNodes.Count; ++t) {
        if (quadTreeNodes[t] == null) { continue; }
        CustomAmmoCategoriesLog.Log.LogWrite("  [" + t + "]=" + quadTreeNodes[t].GetType().ToString() + "\n");
        IList children = (IList)childrenField.GetValue(quadTreeNodes[t]);
        IDictionary dataDictionary = (IDictionary)dataDictionaryField.GetValue(quadTreeNodes[t]);
        CustomAmmoCategoriesLog.Log.LogWrite("   children = " + ((children == null) ? "null" : children.Count.ToString()) + "\n");
        CustomAmmoCategoriesLog.Log.LogWrite("   dataDictionary = " + ((dataDictionary == null) ? "null" : dataDictionary.Count.ToString()) + "\n");
        if (dataDictionary == null) { continue; }
        if (children == null) { continue; }
        if (dataDictionary.Count > 0) {
          IDictionaryEnumerator idataDictionary = dataDictionary.GetEnumerator();
          while(idataDictionary.MoveNext()) {
            QuadTreePrototype prototype = (QuadTreePrototype)idataDictionary.Key;
            object quadTreeData = idataDictionary.Value;
            if (prototype == null) { continue; }
            if (quadTreeData == null) { continue; }
            List<QuadTreeTransform> transformList = (List<QuadTreeTransform>)transformListField.GetValue(quadTreeData);
            CustomAmmoCategoriesLog.Log.LogWrite("    " + prototype.name + ":" + transformList.Count + "\n");
            IList matrixLists = (IList)matrixListField.GetValue(quadTreeData);
            CustomAmmoCategoriesLog.Log.LogWrite("     matrix arrays:" + matrixLists.Count + "\n");
            for (int mi = 0; mi < matrixLists.Count; ++mi) {
              List<Matrix4x4> matrixList = (List<Matrix4x4>)trsListPropertyGet.Invoke(matrixLists[mi], new object[0] { });
              CustomAmmoCategoriesLog.Log.LogWrite("      matrix array[" + mi + "]:" + matrixList.Count + "\n");
            }
          };
        }
      }
    }
    public static void OnInsert(object __instance,ref Matrix4x4 trs, ref Vector3 pos, Quaternion rot, ref Vector3 scale) {
      if (DynamicMapHelper.mapMetaData == null) { return; }
      ++counter;
      if (DynamicTreesHelper.transformListField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting field info\n");
        DynamicTreesHelper.transformListField = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetField("transformList", BindingFlags.NonPublic | BindingFlags.Instance);
        if (DynamicTreesHelper.transformListField == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.matrixListField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting matrixList\n");
        DynamicTreesHelper.matrixListField = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetField("matrixList", BindingFlags.NonPublic | BindingFlags.Instance);
        if (DynamicTreesHelper.matrixListField == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.matrixCountProperty == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting matrixCountProperty\n");
        DynamicTreesHelper.matrixCountProperty = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeArray").GetProperty("count", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        if (DynamicTreesHelper.matrixCountProperty == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.trsListPropertyGet == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting trsListPropertyGet\n");
        DynamicTreesHelper.trsListPropertyGet = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeArray").GetProperty("trsList", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        if (DynamicTreesHelper.trsListPropertyGet == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.trsListPropertySet == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting trsListPropertySet\n");
        DynamicTreesHelper.trsListPropertySet = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeArray").GetProperty("trsList", BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
        if (DynamicTreesHelper.trsListPropertySet == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.quadTreeDataCleanup == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting quadTreeDataCleanup\n");
        DynamicTreesHelper.quadTreeDataCleanup = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetMethod("Cleanup", BindingFlags.Public | BindingFlags.Instance);
        if (DynamicTreesHelper.quadTreeDataCleanup == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      if (DynamicTreesHelper.prototypeField == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("getting prototypeField\n");
        DynamicTreesHelper.prototypeField = typeof(QuadTreeTransform).Assembly.GetType("BattleTech.Rendering.Trees.QuadTreeData").GetField("prototype", BindingFlags.NonPublic | BindingFlags.Instance);
        if (DynamicTreesHelper.prototypeField == null) {
          CustomAmmoCategoriesLog.Log.LogWrite("can't get field info\n");
        };
      }
      List<QuadTreeTransform> transformList = null;
      IList matrixList = null;
      int ti = -1;
      int mi1 = -1;
      int mi2 = -1;
      if (DynamicTreesHelper.NoNeedInsert == true) {
        if(CACDynamicTree.currentBurningIniting != null) {
          transformList = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(__instance);
          if (transformList == null) {
            CustomAmmoCategoriesLog.Log.LogWrite("can't get transform list\n");
          }
          ti = transformList.Count - 1;
          matrixList = (IList)DynamicTreesHelper.matrixListField.GetValue(__instance);
          mi1 = matrixList.Count - 1;
          if (mi1 < 0) {
            CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.OnInsert mi(" + mi1 + ") < 0\n");
            return;
          }
          mi2 = (int)DynamicTreesHelper.matrixCountProperty.Invoke(matrixList[mi1], new object[0] { }) - 1;
          CACDynamicTree.currentBurningIniting.InitBurned(__instance, ti, mi1, mi2);
          CACDynamicTree.currentBurningIniting = null;
          return;
        }
        return;
      };
      QuadTreePrototype prototype = (QuadTreePrototype)DynamicTreesHelper.prototypeField.GetValue(__instance);
      bool isTree = prototype.name.Contains("Tree");
      if ((isTree == false) && (prototype.name.Contains("Bush") == false)) { return; };
      int x = DynamicMapHelper.mapMetaData.GetXIndex(pos.x);
      int z = DynamicMapHelper.mapMetaData.GetZIndex(pos.z);
      if ((x < 0) || (x >= DynamicMapHelper.mapMetaData.GetXLimit()) || (z < 0) || (z >= DynamicMapHelper.mapMetaData.GetZLimit())) {
        return;
      }
      //CustomAmmoCategoriesLog.Log.LogWrite(prototype.name+":"+z+","+x+"\n");
      MapTerrainDataCellEx cell = DynamicMapHelper.mapMetaData.mapTerrainDataCells[z, x] as MapTerrainDataCellEx;
      if (cell == null) { return; }
      transformList = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(__instance);
      if (transformList == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("can't get transform list\n");
      }
      ti = transformList.Count - 1;
      matrixList = (IList)DynamicTreesHelper.matrixListField.GetValue(__instance);
      mi1 = matrixList.Count - 1;
      if (mi1 < 0) {
        CustomAmmoCategoriesLog.Log.LogWrite("QuadTreeData.OnInsert mi("+mi1+") < 0\n");
        return;
      }
      mi2 = (int)DynamicTreesHelper.matrixCountProperty.Invoke(matrixList[mi1],new object[0] { }) - 1;
      cell.trees.Add(new CACDynamicTree(__instance,ti,mi1,mi2, transformList[ti], isTree));
      //};
      return;
      //MapTerrainDataCellEx cell = DynamicMapHelper.mapMetaData.GetCellAt(pos) as MapTerrainDataCellEx;
      //if (cell == null) {
        //int xindex = DynamicMapHelper.mapMetaData.GetXIndex(pos.x);
        //int zindex = DynamicMapHelper.mapMetaData.GetZIndex(pos.z);
        //CustomAmmoCategoriesLog.Log.LogWrite("cell "+xindex+":"+zindex+"\n");
        /*return;
      //}
      List<QuadTreeTransform> transformList = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(__instance);
      if (transformList == null) {
        CustomAmmoCategoriesLog.Log.LogWrite("can't get transform list\n");
      }
      cell.trees.Add(new CACDynamicTree(__instance, transformList.Last()));*/
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
      Log.M?.TWL(0,"RenderTrees.InitQuadTree postfix");
      DynamicTreesHelper.NoNeedInsert = true;
      try {
        QuadTreePrototype[] quadTreePrototypes = (QuadTreePrototype[])typeof(RenderTrees).GetField("quadTreePrototypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        DynamicTreesHelper.burnedTreePrefab = CACMain.Core.findPrefab(CustomAmmoCategories.Settings.BurnedTrees.Mesh);
        if (DynamicTreesHelper.burnedTreePrefab == null) {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + CustomAmmoCategories.Settings.BurnedTrees.Mesh + " - fail\n");
          DynamicTreesHelper.burnedTreePrototypes.Clear();
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite(" " + CustomAmmoCategories.Settings.BurnedTrees.Mesh + " components:\n");
          foreach (var component in DynamicTreesHelper.burnedTreePrefab.GetComponents(typeof(Component))) {
            CustomAmmoCategoriesLog.Log.LogWrite("  " + component.GetType().ToString() + "\n");
          }
          MeshFilter meshFilter = DynamicTreesHelper.burnedTreePrefab.GetComponentInChildren<MeshFilter>();
          Texture2D _BumpMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.BumpMap);
          Texture2D _MainTex = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.MainTex);
          Texture2D _OcculusionMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.OcculusionMap);
          Texture2D _Transmission = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.Transmission);
          Texture2D _MetallicGlossMap = CACMain.Core.findTexture(CustomAmmoCategories.Settings.BurnedTrees.MetallicGlossMap);

          if (meshFilter == null) {
            CustomAmmoCategoriesLog.Log.LogWrite(" meshFilter is null\n");
          } else {
            CustomAmmoCategoriesLog.Log.LogWrite(" meshFilter is not null " + meshFilter.name + ":" + meshFilter.sharedMesh.name + "\n");
            TreePrototype prototypeToCopy = null;
            CustomAmmoCategoriesLog.Log.LogWrite(" active terrain unity tree prototypes:\n");
            foreach (TreePrototype prototype in Terrain.activeTerrain.terrainData.treePrototypes) {
              CustomAmmoCategoriesLog.Log.LogWrite("  " + prototype.prefab.name + "\n");
              if (prototype.prefab.name.Contains("Tree") == false) { continue; }
              prototypeToCopy = prototype; break;
            }
            if (prototypeToCopy != null) {
              QuadTreePrototype newPrototype = new QuadTreePrototype(prototypeToCopy);
              string protName = prototypeToCopy.prefab.name + "_burned";
              typeof(QuadTreePrototype).GetProperty("name", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(newPrototype, new object[1] { (object)protName });
              CustomAmmoCategoriesLog.Log.LogWrite(" " + protName + " lods count: " + newPrototype.meshes.Length + "\n");
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
              CustomAmmoCategoriesLog.Log.LogWrite(" initing burned success\n");
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
              typeof(RenderTrees).GetField("quadTreePrototypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, newTreePrototypes);
            } else {
              CustomAmmoCategoriesLog.Log.LogWrite(" no trees on terrain\n");
            }
          }
        }
        QuadTree quadTree = (QuadTree)typeof(RenderTrees).GetField("_quadTree", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        DynamicTreesHelper._quadTree = quadTree;
        quadTreePrototypes = (QuadTreePrototype[])typeof(RenderTrees).GetField("quadTreePrototypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
        if (quadTreePrototypes != null) {
          CustomAmmoCategoriesLog.Log.LogWrite("avaible tree prototypes (" + quadTreePrototypes.Length + "):\n");
          foreach (QuadTreePrototype prototype in quadTreePrototypes) {
            CustomAmmoCategoriesLog.Log.LogWrite(" " + prototype.name + "\n");
          }
          if ((DynamicTreesHelper.burnedTreePrototype != null) && (CustomAmmoCategories.Settings.DontShowBurnedTrees == false) && (CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary == false)) {
            CustomAmmoCategoriesLog.Log.LogWrite("Installing burned trees. All trees-like objects count: (" + CACDynamicTree.allCACTrees.Count + "):\n");
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
            CustomAmmoCategoriesLog.Log.LogWrite("All trees count:" + counter + "\n");
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("quadTreePrototypes array is null\n");
        }
        if (quadTree != null) {
          IList quadTreeNodes = (IList)typeof(QuadTree).GetField("quadTreeNodes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(quadTree);
          CustomAmmoCategoriesLog.Log.LogWrite(" quadTreeNodes.Count = " + quadTreeNodes.Count + "\n");
          Type QuadTreeNode = quadTreeNodes[0].GetType();
          FieldInfo childrenField = QuadTreeNode.GetField("children", BindingFlags.Instance | BindingFlags.NonPublic);
          FieldInfo dataDictionaryField = QuadTreeNode.GetField("dataDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
          //int treesCount = 0;
          for (int t = 0; t < quadTreeNodes.Count; ++t) {
            CustomAmmoCategoriesLog.Log.LogWrite("  [" + t + "]=" + quadTreeNodes[t].GetType().ToString() + "\n");
            IList children = (IList)childrenField.GetValue(quadTreeNodes[t]);
            IDictionary dataDictionary = (IDictionary)dataDictionaryField.GetValue(quadTreeNodes[t]);
            CustomAmmoCategoriesLog.Log.LogWrite("   children = " + ((children == null) ? "null" : children.Count.ToString()) + "\n");
            CustomAmmoCategoriesLog.Log.LogWrite("   dataDictionary = " + ((dataDictionary == null) ? "null" : dataDictionary.Count.ToString()) + "\n");
            if (dataDictionary == null) { continue; }
            if (children == null) { continue; }
            if (dataDictionary.Count > 0) {
              IDictionaryEnumerator idataDictionary = dataDictionary.GetEnumerator();
              while (idataDictionary.MoveNext()) {
                QuadTreePrototype prototype = (QuadTreePrototype)idataDictionary.Key;
                object quadTreeData = idataDictionary.Value;
                List<QuadTreeTransform> transformList = (List<QuadTreeTransform>)DynamicTreesHelper.transformListField.GetValue(quadTreeData);
                CustomAmmoCategoriesLog.Log.LogWrite("    " + prototype.name + ":" + transformList.Count + "\n");
                IList matrixLists = (IList)DynamicTreesHelper.matrixListField.GetValue(quadTreeData);
                CustomAmmoCategoriesLog.Log.LogWrite("     matrix arrays:" + matrixLists.Count + "\n");
                for (int mi = 0; mi < matrixLists.Count; ++mi) {
                  List<Matrix4x4> matrixList = (List<Matrix4x4>)DynamicTreesHelper.trsListPropertyGet.Invoke(matrixLists[mi], new object[0] { });
                  CustomAmmoCategoriesLog.Log.LogWrite("      matrix array[" + mi + "]:" + matrixList.Count + "\n");
                }
              };
            }
          }
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("quadTree is null\n");
        }
      } catch (Exception e) {
        Log.LogWrite("Fail to setup burned trees. Burned trees will not be shown: "+e.ToString()+"\n",true);
        CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary = true;
      }
    }
  }
}
