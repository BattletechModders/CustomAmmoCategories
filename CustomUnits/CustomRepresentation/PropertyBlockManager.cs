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
using BattleTech.Rendering;
using HarmonyLib;
using HBS.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("UpdateCache")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class PropertyBlockManager_UpdateCache {
    public static bool Prefix(PropertyBlockManager __instance) {
      try {
        //CustomMechRepresentation custRep = __instance.rootObject.GetComponent<CustomMechRepresentation>();
        //if (custRep == null) { return true; }
        //Log.TWL(0, "PropertyBlockManager.UpdateCache "+custRep.gameObject.name+" mech:"+(custRep.parentMech == null?"null": custRep.parentMech.MechDef.ChassisID));
        //___skinnedRendererCache = custRep.skinnedMeshRenderersCache.ToArray();
        //___meshRendererCache = custRep.meshRenderersCache.ToArray();
        //foreach(MeshRenderer r in ___meshRendererCache) {
        //  Log.WL(1,"renderer:"+r.gameObject.name);
        //}
        //foreach (SkinnedMeshRenderer r in ___skinnedRendererCache) {
        //  Log.WL(1, "skinned renderer:" + r.gameObject.name);
        //}
        if(__instance is CustomPropertyBlockManager customBlock) {
          customBlock._UpdateCache();
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("UpdateProperties")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class PropertyBlockManager_UpdateProperties {
    public static bool Prefix(PropertyBlockManager __instance) {
      //try {
      if (__instance is CustomPropertyBlockManager customBlock) {
        customBlock._UpdateProperties();
        return false;
      }
      //} catch (Exception e) {
      //Log.TWL(0, e.ToString(), true);
      //}
      return true;
    }
  }
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("AddProperty")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PropertyBlockManager_AddProperty {
    public static bool Prefix(PropertyBlockManager __instance, ref PropertyBlockManager.PropertySetting newProperty) {
      try {
        if (__instance is CustomPropertyBlockManager customBlock) {
          customBlock._AddProperty(ref newProperty);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PropertyBlockManager))]
  [HarmonyPatch("RemoveProperty")]
  [HarmonyPatch(MethodType.Normal)]
  public static class PropertyBlockManager_RemoveProperty {
    public static bool Prefix(PropertyBlockManager __instance, ref PropertyBlockManager.PropertySetting oldProperty) {
      try {
        if (__instance is CustomPropertyBlockManager customBlock) {
          customBlock._RemoveProperty(ref oldProperty);
          return false;
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
  }
  public class CustomPropertyBlockManager : PropertyBlockManager {
    private static readonly bool DEBUG_LOG = false;
    private FastList<PropertyBlockManager.PropertySetting> main_properties = new FastList<PropertyBlockManager.PropertySetting>();
    protected Dictionary<Renderer, PropertyBlockManager.PropertySetting> paintSchemes { get; set; } = new Dictionary<Renderer, PropertyBlockManager.PropertySetting>();
    protected SkinnedMeshRenderer[] _skinnedRendererCache { get { return Traverse.Create(this).Field<SkinnedMeshRenderer[]>("skinnedRendererCache").Value; } set { Traverse.Create(this).Field<SkinnedMeshRenderer[]>("skinnedRendererCache").Value = value; } }
    protected MeshRenderer[] _meshRendererCache { get { return Traverse.Create(this).Field<MeshRenderer[]>("meshRendererCache").Value; } set { Traverse.Create(this).Field<MeshRenderer[]>("meshRendererCache").Value = value; } }
    protected Dictionary<Renderer, MaterialPropertyBlock> materialBlocks { get; set; } = new Dictionary<Renderer, MaterialPropertyBlock>();
    public void Copy(PropertyBlockManager src) {
      FastList<PropertyBlockManager.PropertySetting> properties = Traverse.Create(src).Field<FastList<PropertyBlockManager.PropertySetting>>("properties").Value;
      if (properties != null) {
        foreach (PropertyBlockManager.PropertySetting prop in properties) {
          if (prop == null) { continue; }
          this.main_properties.Add(prop);
        }
      }
      this._UpdateCache();
    }
    public void _UpdateProperties() {
      if(DEBUG_LOG) Log.TWL(0, "CustomPropertyBlockManager._UpdateProperties " + this.gameObject.name);
      foreach (var mblock in materialBlocks) { if (mblock.Key == null) { continue; }; mblock.Value.Clear(); }
      foreach (PropertyBlockManager.PropertySetting property in this.main_properties) {
        this.AddPropertyToBlock(property);
        if (DEBUG_LOG) Log.WL(1, property.PropertyName+":"+property.PropertyType+" common");
      }
      foreach (var paintTexture in this.paintSchemes) {
        if (paintTexture.Key == null) { continue; }
        if (materialBlocks.TryGetValue(paintTexture.Key, out MaterialPropertyBlock block)) {
          if (DEBUG_LOG) Log.WL(1, paintTexture.Value.PropertyName+":" + paintTexture.Value.PropertyType+" renderer:"+ paintTexture.Key.gameObject.name+" texture:"+paintTexture.Value.PropertyTexture.name);
          this.AddPropertyToBlock(block, paintTexture.Value);
        }
      }
      foreach (var mblock in materialBlocks) {
        if (mblock.Key == null) { continue; };
        mblock.Key.SetPropertyBlock(mblock.Value);
      }
    }
    public void _AddProperty(ref PropertyBlockManager.PropertySetting newProperty) {
      if (!this.main_properties.Contains(newProperty))
        this.main_properties.Add(newProperty);
      this._UpdateProperties();
    }
    public void _RemoveProperty(ref PropertyBlockManager.PropertySetting oldProperty) {
      if (this.main_properties.Contains(oldProperty)) { this.main_properties.RemoveFast(oldProperty); }
      this._UpdateProperties();
    }
    public void AddPaintScheme(Renderer renderer,ref PropertyBlockManager.PropertySetting paintSchemeProperty) {
      if (this.paintSchemes.ContainsKey(renderer)) {
        paintSchemes[renderer] = paintSchemeProperty;
      } else {
        paintSchemes.Add(renderer, paintSchemeProperty);
      }
    }
    //public void AddPropertyHeraldry(ref PropertyBlockManager.PropertySetting newProperty) {
    //  if (!this.heraldry_properties.Contains(newProperty)) { this.heraldry_properties.Add(newProperty); }
    //  if (this.main_properties.Contains(newProperty)) { this.main_properties.RemoveFast(newProperty); }
    //  this._UpdateProperties();
    //}
    //public void RemovePropertyHeraldry(ref PropertyBlockManager.PropertySetting oldProperty) {
    //  if (this.heraldry_properties.Contains(oldProperty)) { this.heraldry_properties.RemoveFast(oldProperty); }
    //  if (this.main_properties.Contains(oldProperty)) { this.main_properties.RemoveFast(oldProperty); }
    //  this._UpdateProperties();
    //}
    private void AddPropertyToBlock(PropertyBlockManager.PropertySetting property) {
      switch (property.PropertyType) {
        case PropertyBlockManager.PropertySetting.PropertyTypes.Color:
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetColor(property.PropertyName, property.PropertyColor); }
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Float:
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetFloat(property.PropertyName, property.PropertyFloat); }
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Matrix:
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetMatrix(property.PropertyName, property.PropertyMatrix); }
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Texture:
        if (property.PropertyTexture == null) { break; }
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetTexture(property.PropertyName, property.PropertyTexture); }
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Vector:
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetVector(property.PropertyName, property.PropertyVector); }
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.FloatArray:
        foreach (var mblock in this.materialBlocks) { mblock.Value.SetFloatArray(property.PropertyName, property.PropertyFloatArray); }
        break;
      }
    }
    private void AddPropertyToBlock(MaterialPropertyBlock block, PropertyBlockManager.PropertySetting property) {
      switch (property.PropertyType) {
        case PropertyBlockManager.PropertySetting.PropertyTypes.Color:
        block.SetColor(property.PropertyName, property.PropertyColor);
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Float:
        block.SetFloat(property.PropertyName, property.PropertyFloat);
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Matrix:
        block.SetMatrix(property.PropertyName, property.PropertyMatrix); 
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Texture:
        if (property.PropertyTexture == null) { break; }
        block.SetTexture(property.PropertyName, property.PropertyTexture);
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.Vector:
        block.SetVector(property.PropertyName, property.PropertyVector);
        break;
        case PropertyBlockManager.PropertySetting.PropertyTypes.FloatArray:
        block.SetFloatArray(property.PropertyName, property.PropertyFloatArray);
        break;
      }
    }
    public void _UpdateCache() {
      ICustomizationTarget custRep = this.rootObject.GetComponent<CustomMechRepresentation>() as ICustomizationTarget;
      if (custRep == null) { custRep = this.rootObject.GetComponent<CustomMechRepresentationSimGame>() as ICustomizationTarget; }
      if (custRep != null) {
        if (DEBUG_LOG) Log.TWL(0, "PropertyBlockManager.UpdateCache " + custRep._gameObject.name + " chassis:" + (custRep.chassisDef == null ? "null" : custRep.chassisDef.Description.Id));
        //this._skinnedRendererCache = 
        //this._meshRendererCache = 
        this._skinnedRendererCache = custRep.skinnedMeshRenderersCache.Keys.ToArray();
        this._meshRendererCache = custRep.meshRenderersCache.Keys.ToArray();
        this.paintSchemes.Clear();
        this.materialBlocks.Clear();
        foreach (SkinnedMeshRenderer renderer in _skinnedRendererCache) {
          CustomPaintPattern pattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
          if (pattern == null) { continue; }
          if (DEBUG_LOG) Log.WL(0,"renderer:"+ renderer.name+" "+ pattern.paintSchemeProperty.PropertyName);
          this.paintSchemes.Add(renderer, pattern.paintSchemeProperty);
          this.materialBlocks.Add(renderer, new MaterialPropertyBlock());
        }
        foreach (MeshRenderer renderer in _meshRendererCache) {
          CustomPaintPattern pattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
          if (pattern == null) { continue; }
          if (DEBUG_LOG) Log.WL(0, "renderer:" + renderer.name + " " + pattern.paintSchemeProperty.PropertyName);
          this.paintSchemes.Add(renderer, pattern.paintSchemeProperty);
          this.materialBlocks.Add(renderer, new MaterialPropertyBlock());
        }
      } else {
        throw new Exception("CustomPropertyBlockManager should not be used without CustomMechRepresentation");
      }
      this._UpdateProperties();
    }
    private void OnEnable() {
      if (this.rootObject == null) { this.rootObject = this.gameObject; }
      this.UpdateCache();
    }
    private void OnDisable() {
      if (this._skinnedRendererCache != null) {
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in this._skinnedRendererCache) {
          if (skinnedMeshRenderer != null) { skinnedMeshRenderer.SetPropertyBlock(null); }
        }
      }
      if (this._meshRendererCache == null) { return; }
      foreach (MeshRenderer meshRenderer in this._meshRendererCache) {
        if (meshRenderer != null) { meshRenderer.SetPropertyBlock(null); }
      }
    }
  }
}