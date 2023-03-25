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
using BattleTech.Rendering.MechCustomization;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomUnits {
  public class CustomPaintPattern: MonoBehaviour {
    public List<Texture2D> _paintPatterns = null;
    public int _currentIndex = -1;
    public bool is_custom { get; private set; } = false;
    public int currentIndex { get { return _currentIndex; } }
    public List<Texture2D> paintPatterns {
      get {
        if (_paintPatterns == null) { _paintPatterns = new List<Texture2D>(); }
        return _paintPatterns;
      }
    }
    public Renderer renderer { get; set; } = null;
    public PropertyBlockManager.PropertySetting paintSchemeProperty { get; set; } = new PropertyBlockManager.PropertySetting("_PaintScheme", Texture2D.blackTexture);
    //public PropertyBlockManager.PropertySetting paintEmblemProperty { get; set; } = new PropertyBlockManager.PropertySetting("_PaintScheme", Texture2D.blackTexture);
    public void Init(Renderer renderer, MeshRenderer paintSchemeHolder) {
      Log.TWL(0, "CustomPaintPattern.Init custom "+renderer.gameObject.name+" "+(paintSchemeHolder==null?"null": paintSchemeHolder.sharedMaterials.Length.ToString()));
      this.is_custom = true;
      this.renderer = renderer;
      if (this._paintPatterns == null) { this._paintPatterns = new List<Texture2D>(); }
      if (paintSchemeHolder != null) {
        foreach (Material mat in paintSchemeHolder.sharedMaterials) {
          this._paintPatterns.Add(mat.mainTexture as Texture2D);
          Log.WL(1, "add texture:" + (mat.mainTexture as Texture2D).name+":"+ this._paintPatterns.Count);
        }
      }
      if (this._paintPatterns.Count == 0) {
        this._paintPatterns.Add(Texture2D.blackTexture);
      }
      Log.WL(1, "paintPatterns:" + this._paintPatterns.Count);
      foreach(Texture2D tex in this._paintPatterns) {
        Log.WL(2,tex.name);
      }
    }
    public void ApplyPaintScheme(int index) {
      if (index < 0) { index = 0; }
      if (paintPatterns.Count == 0) return;
      paintSchemeProperty.PropertyTexture = this.paintPatterns[index % paintPatterns.Count];
      Log.TWL(0, "CustomPaintPattern.ApplyPaintScheme " + index + " renderer:" + this.renderer.gameObject.name+" texture:"+ paintSchemeProperty.PropertyTexture.name);
      this._currentIndex = index;
    }
    public void Init(Renderer renderer, CustomMechCustomization customization) {
      this.renderer = renderer;
      this.is_custom = false;
      Log.TWL(0, "CustomPaintPattern.Init default " + renderer.gameObject.name + " " + (customization == null ? "null" : customization.paintPatterns.Length.ToString()));
      if (this._paintPatterns == null) { this._paintPatterns = new List<Texture2D>(); }
      foreach (Texture2D tex in customization.paintPatterns) { if (tex == null) { continue; }; this._paintPatterns.Add(tex); }
    }
  }
  public class CustomMechCustomization : MonoBehaviour {
    public ICustomizationTarget parent { get; set; } = null;
    //private static Texture2D _defaultEmptyPaintPattern = null;
    public static Shader _mechShader;
    public static Shader _bakingShader;
    public Material _bakingMaterial;
    public MechPaintScheme _paintScheme;
    public MechEmblemScheme _emblemScheme;
    public RenderTexture bakingRT;
    public Texture2D baseAlbedoTex;
    public Texture2D emblemTex;
    public Texture2D emblemSDF;
    public CustomPropertyBlockManager _propertyBlock;
    public Texture2D[] paintPatterns;
    public Texture2D[] backerPatterns;
    public int patternIndex;
    public bool backerPattern;
    public PropertyBlockManager.PropertySetting emblemTexProp;
    //public PropertyBlockManager.PropertySetting paintSchemeTexProp;
    public PropertyBlockManager.PropertySetting paint0ColorProp;
    public PropertyBlockManager.PropertySetting paint1ColorProp;
    public PropertyBlockManager.PropertySetting paint2ColorProp;
    public PropertyBlockManager.PropertySetting paint0SmoothnessProp;
    public PropertyBlockManager.PropertySetting paint1SmoothnessProp;
    public PropertyBlockManager.PropertySetting paint2SmoothnessProp;
    public PropertyBlockManager.PropertySetting paint0MetallicProp;
    public PropertyBlockManager.PropertySetting paint1MetallicProp;
    public PropertyBlockManager.PropertySetting paint2MetallicProp;
    public CommandBuffer bakingCommandBuffer;
    public BTDecal decal0;
    public BTDecal decal1;
    public BTDecal decal2;
    public BTDecal decal3;
    public BTDecal decal4;
    public BTDecal decal5;
    public BattleTech.Rendering.MechCustomization.MechCustomization.EmblemSetting[] emblemSettings = new BattleTech.Rendering.MechCustomization.MechCustomization.EmblemSetting[6];
    public List<Mesh> meshList = new List<Mesh>();
    public List<Transform> transformList = new List<Transform>();
    //public static Texture2D defaultEmptyPaintPattern {
    //  get {
    //    if (_defaultEmptyPaintPattern == null) {
    //      _defaultEmptyPaintPattern = new Texture2D(2048, 2048);
    //      Color[] pixels = Enumerable.Repeat(Color.black, Screen.width * Screen.height).ToArray();
    //      _defaultEmptyPaintPattern.SetPixels(pixels);
    //      _defaultEmptyPaintPattern.Apply();
    //    }
    //    return _defaultEmptyPaintPattern;
    //  }
    //}
    public void Copy(MechCustomization source) {
      this._bakingMaterial = Traverse.Create(source).Property<Material>("bakingMaterial").Value;
      this._paintScheme = source.paintScheme;
      this._emblemScheme = source.emblemScheme;
      this.bakingRT = Traverse.Create(source).Field<RenderTexture>("bakingRT").Value;
      this.baseAlbedoTex = Traverse.Create(source).Field<Texture2D>("baseAlbedoTex").Value;
      this.emblemTex = Traverse.Create(source).Field<Texture2D>("emblemTex").Value;
      this.emblemSDF = Traverse.Create(source).Field<Texture2D>("emblemSDF").Value;
      this._propertyBlock = Traverse.Create(source).Property<PropertyBlockManager>("propertyBlock").Value as CustomPropertyBlockManager;
      this.paintPatterns = source.paintPatterns;
      this.backerPatterns = source.backerPatterns;
      this.patternIndex = Traverse.Create(source).Field<int>("patternIndex").Value;
      this.backerPattern = Traverse.Create(source).Field<bool>("backerPattern").Value;
      this.emblemTexProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("emblemTexProp").Value;
      //this.paintSchemeTexProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paintSchemeTexProp").Value;
      this.paint0ColorProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint0ColorProp").Value;
      this.paint1ColorProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint1ColorProp").Value;
      this.paint2ColorProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint2ColorProp").Value;
      this.paint0SmoothnessProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint0SmoothnessProp").Value;
      this.paint1SmoothnessProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint1SmoothnessProp").Value;
      this.paint2SmoothnessProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint2SmoothnessProp").Value;
      this.paint0MetallicProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint0MetallicProp").Value;
      this.paint1MetallicProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint1MetallicProp").Value;
      this.paint2MetallicProp = Traverse.Create(source).Field<PropertyBlockManager.PropertySetting>("paint2MetallicProp").Value;
      this.bakingCommandBuffer = Traverse.Create(source).Field<CommandBuffer>("bakingCommandBuffer").Value;
      this.decal0 = Traverse.Create(source).Field<BTDecal>("decal0").Value;
      this.decal1 = Traverse.Create(source).Field<BTDecal>("decal1").Value;
      this.decal2 = Traverse.Create(source).Field<BTDecal>("decal2").Value;
      this.decal3 = Traverse.Create(source).Field<BTDecal>("decal3").Value;
      this.decal4 = Traverse.Create(source).Field<BTDecal>("decal4").Value;
      this.decal5 = Traverse.Create(source).Field<BTDecal>("decal5").Value;
      this.emblemSettings = Traverse.Create(source).Field<BattleTech.Rendering.MechCustomization.MechCustomization.EmblemSetting[]>("emblemSettings").Value;
    }
    private static Shader mechShader {
      get {
        if ((UnityEngine.Object)CustomMechCustomization._mechShader == (UnityEngine.Object)null)
          CustomMechCustomization._mechShader = Shader.Find("BattleTech Mech Standard");
        return CustomMechCustomization._mechShader;
      }
    }
    private static Shader bakingShader {
      get {
        if ((UnityEngine.Object)CustomMechCustomization._bakingShader == (UnityEngine.Object)null)
          CustomMechCustomization._bakingShader = Shader.Find("Hidden/BT-DecalBake");
        return CustomMechCustomization._bakingShader;
      }
    }
    private Material bakingMaterial {
      get {
        if ((UnityEngine.Object)this._bakingMaterial == (UnityEngine.Object)null)
          this._bakingMaterial = new Material(CustomMechCustomization.bakingShader);
        return this._bakingMaterial;
      }
    }
    public MechPaintScheme paintScheme {
      get => this._paintScheme;
      set {
        this._paintScheme = value;
        this.SetPaintColors();
      }
    }
    public MechEmblemScheme emblemScheme {
      get => this._emblemScheme;
      set {
        this._emblemScheme = value;
        this.BakeEmblem();
      }
    }
    private CustomPropertyBlockManager propertyBlock {
      get {
        return this._propertyBlock;
      }
    }
    public int NumPatterns => this.paintPatterns == null ? 0 : this.paintPatterns.Length;
    public int NumBackerPatterns => this.backerPatterns == null ? 0 : this.backerPatterns.Length;
    private void SetParameter(ref PropertyBlockManager.PropertySetting prop, string name, Color value) {
      if (prop == null) {
        prop = new PropertyBlockManager.PropertySetting(name, value);
        this.propertyBlock._AddProperty(ref prop);
      } else
        prop.PropertyColor = value;
    }
    private void SetParameter(ref PropertyBlockManager.PropertySetting prop, string name, Texture2D tex) {
      if (prop == null) {
        prop = new PropertyBlockManager.PropertySetting(name, (Texture)tex);
        this.propertyBlock._AddProperty(ref prop);
      } else
        prop.PropertyTexture = (Texture)tex;
    }
    private void SetParameter(ref PropertyBlockManager.PropertySetting prop, string name, float value) {
      if (prop == null) {
        prop = new PropertyBlockManager.PropertySetting(name, value);
        this.propertyBlock._AddProperty(ref prop);
      } else
        prop.PropertyFloat = value;
    }
    public void UpdatePaintPatterns() {

    }
    private void ApplyPaintSchemeTexture() {
      CustomPaintPattern[] paintPatterns = this.parent._gameObject.GetComponentsInChildren<CustomPaintPattern>(true);
      foreach(CustomPaintPattern pattern in paintPatterns) {
        pattern.ApplyPaintScheme(this.patternIndex);
      }

      paintPatterns = this.parent._visibleObject.gameObject.GetComponentsInChildren<CustomPaintPattern>(true);
      foreach (CustomPaintPattern pattern in paintPatterns) {
        pattern.ApplyPaintScheme(this.patternIndex);
      }
    }
    private void SetPaintColors() {
      if (this.paintScheme == null) {
        this.LogError(nameof(SetPaintColors), "paintScheme is null");
      } else {
        if (this.paintScheme.paintLayer0 == null || this.paintScheme.paintLayer1 == null || this.paintScheme.paintLayer2 == null) { return; }
        if (this.paintScheme.paintSchemeTex == null) {
          this.LogError(nameof(SetPaintColors), "paintScheme.paintSchemeTex is null");
        } else {
          this.ApplyPaintSchemeTexture();
          //if (this.paintScheme.paintSchemeTex != null)
          //this.SetParameter(ref this.paintSchemeTexProp, "_PaintScheme", this.paintScheme.paintSchemeTex);
          if (this.paintScheme.paintLayer0.paintLayer != null) {
            this.SetParameter(ref this.paint0ColorProp, "_PaintColor1", this.paintScheme.paintLayer0.paintLayer.paintColor);
            this.SetParameter(ref this.paint0SmoothnessProp, "_PaintSmoothness1", this.paintScheme.paintLayer0.paintLayer.paintSmoothness);
            this.SetParameter(ref this.paint0MetallicProp, "_PaintMetal1", this.paintScheme.paintLayer0.paintLayer.paintMetallic);
          }
          if (this.paintScheme.paintLayer1.paintLayer != null) {
            this.SetParameter(ref this.paint1ColorProp, "_PaintColor2", this.paintScheme.paintLayer1.paintLayer.paintColor);
            this.SetParameter(ref this.paint1SmoothnessProp, "_PaintSmoothness2", this.paintScheme.paintLayer1.paintLayer.paintSmoothness);
            this.SetParameter(ref this.paint1MetallicProp, "_PaintMetal2", this.paintScheme.paintLayer1.paintLayer.paintMetallic);
          }
          if (this.paintScheme.paintLayer2.paintLayer != null) {
            this.SetParameter(ref this.paint2ColorProp, "_PaintColor3", this.paintScheme.paintLayer2.paintLayer.paintColor);
            this.SetParameter(ref this.paint2SmoothnessProp, "_PaintSmoothness3", this.paintScheme.paintLayer2.paintLayer.paintSmoothness);
            this.SetParameter(ref this.paint2MetallicProp, "_PaintMetal3", this.paintScheme.paintLayer2.paintLayer.paintMetallic);
          }
          this.propertyBlock._UpdateProperties();
        }
      }
    }
    private void BuildCommandBuffer() {
      if (this.bakingCommandBuffer == null) {
        this.bakingCommandBuffer = new CommandBuffer();
        this.bakingCommandBuffer.name = "Baking Command Buffer";
      }
      this.bakingCommandBuffer.Clear();
      if (this.emblemScheme == null && (UnityEngine.Object)this.emblemScheme.emblemTex0 == (UnityEngine.Object)null)
        return;
      this.bakingCommandBuffer.SetRenderTarget((RenderTargetIdentifier)(Texture)this.bakingRT);
      this.bakingCommandBuffer.ClearRenderTarget(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
      this.bakingMaterial.SetTexture("_MainTex", (Texture)this.emblemScheme.emblemTex0);
      BattleTech.Rendering.MechCustomization.MechCustomization.EmblemSetting emblemSetting = this.emblemSettings[(int)this.emblemScheme.slot];
      for (int index1 = 0; index1 < 6; ++index1) {
        bool flag = false;
        switch (index1) {
          case 0:
          if (emblemSetting.decal0 && (UnityEngine.Object)this.decal0 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal0.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
          case 1:
          if (emblemSetting.decal1 && (UnityEngine.Object)this.decal1 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal1.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
          case 2:
          if (emblemSetting.decal2 && (UnityEngine.Object)this.decal2 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal2.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
          case 3:
          if (emblemSetting.decal3 && (UnityEngine.Object)this.decal3 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal3.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
          case 4:
          if (emblemSetting.decal4 && (UnityEngine.Object)this.decal4 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal4.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
          case 5:
          if (emblemSetting.decal5 && (UnityEngine.Object)this.decal5 != (UnityEngine.Object)null) {
            this.bakingCommandBuffer.SetGlobalMatrix(Shader.PropertyToID("_WorldToDecal"), this.decal5.worldToLocalMatrix);
            flag = true;
            break;
          }
          break;
        }
        if (flag) {
          //Log.TWL(0, "CustomMechCustomization.BuildCommandBuffer");
          for (int index2 = 0; index2 < this.meshList.Count; ++index2) {
            Mesh mesh = this.meshList[index2];
            Transform transform = this.transformList[index2];
            Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            if ((UnityEngine.Object)mesh != (UnityEngine.Object)null)
              this.bakingCommandBuffer.DrawMesh(mesh, matrix, this.bakingMaterial, 0, 0, (MaterialPropertyBlock)null);
          }
        }
      }
    }
    public bool BakeEmblem() {
      this.RefreshCache();
      if ((UnityEngine.Object)this.baseAlbedoTex == (UnityEngine.Object)null || this.emblemScheme == null)
        return false;
      int width = this.baseAlbedoTex.width;
      int height1 = this.baseAlbedoTex.height;
      if (!((UnityEngine.Object)this.emblemTex == (UnityEngine.Object)null) && this.emblemTex.width == width) {
        int height2 = this.emblemTex.height;
      }
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.emblemTex);
      this.emblemTex = new Texture2D(width, height1, TextureFormat.ARGB32, true, false);
      this.emblemTex.name = this.baseAlbedoTex.name + "_emblem";
      RenderTexture renderTexture = new RenderTexture(width, height1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      renderTexture.name = "Emblem Bake RT: " + (object)width + "x" + (object)height1;
      renderTexture.useMipMap = true;
      renderTexture.autoGenerateMips = true;
      this.bakingRT = renderTexture;
      this.bakingRT.Create();
      this.BuildCommandBuffer();
      Graphics.ExecuteCommandBuffer(this.bakingCommandBuffer);
      RenderTexture.active = this.bakingRT;
      this.emblemTex.ReadPixels(new Rect(0.0f, 0.0f, (float)width, (float)height1), 0, 0);
      this.emblemTex.Apply(true, false);
      this.emblemTex.Compress(true);
      RenderTexture.active = (RenderTexture)null;
      this.bakingRT.Release();
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.bakingRT);
      this.bakingRT = (RenderTexture)null;
      if (this.emblemTexProp == null) {
        this.emblemTexProp = new PropertyBlockManager.PropertySetting("_EmblemMap", (Texture)this.emblemTex);
        this.propertyBlock.AddProperty(ref this.emblemTexProp);
      } else {
        this.emblemTexProp.PropertyTexture = (Texture)this.emblemTex;
        this.propertyBlock.UpdateProperties();
      }
      return true;
    }
    private void RefreshCache() {
      SkinnedMeshRenderer[] componentsInChildren = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      this.meshList = new List<Mesh>();
      this.transformList = new List<Transform>();
      foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren) {
        Material sharedMaterial = skinnedMeshRenderer.sharedMaterials[0];
        if (sharedMaterial != null && sharedMaterial.shader == CustomMechCustomization.mechShader && (skinnedMeshRenderer.gameObject.activeInHierarchy && !sharedMaterial.name.Contains("weapons"))) {
          if (this.baseAlbedoTex == null) {
            this.baseAlbedoTex = (Texture2D)skinnedMeshRenderer.sharedMaterials[0].GetTexture("_MainTex");
          }
          this.meshList.Add(skinnedMeshRenderer.sharedMesh);
          this.transformList.Add(skinnedMeshRenderer.transform);
        }
      }
    }
    public void SetPatternIndex(int index) {
      if (index < 0) {
        this.paintScheme.SetPatternTexture(Texture2D.blackTexture);
      } else {
        if (index >= this.NumPatterns) {
          this.LogError(nameof(SetPatternIndex), "Tried to set a pattern that doesn't exist");
          index = this.NumPatterns - 1;
        }
        this.paintScheme.SetPatternTexture(this.paintPatterns[index]);
      }
    }
        public void ApplyHeraldry(HeraldryDef heraldryDef, string paintTextureId = null)
        {
            Log.TWL(0, "CustomMechCustomization.ApplyHeraldry gameObject: " + this.gameObject.name + 
                "  heraldryDef.Desc.Id:" + (heraldryDef == null ? "null" : heraldryDef.Description.Id) + 
                "  paintTextureId:" + (paintTextureId == null ? "null" : paintTextureId));
            
            if (heraldryDef == null)
            {
                Log.TWL(0, "Tried to apply null heraldry to " + this.name);
                return;
            }
            if (this.paintPatterns == null)
            {
                Log.TWL(0, "This unit has null paintPatterns " + this.name);
                return;
            }
            if (this.paintPatterns.Length == 0)
            {
                Log.TWL(0, "This unit has a 0 length paintPatterns array " + this.name);
                return;
            }

            Texture2D camoMaskTexture = null;
            this.patternIndex = -1;
            for (int i = 0; i < this.paintPatterns.Length; i++)
            {
                Texture2D paintPattern = this.paintPatterns[i];
                if (paintPattern == null) 
                { 
                    Log.TWL(0, "Texture2d was null in paintPatterns for " + this.name); 
                }
                else if (paintPattern.name == paintTextureId)
                {
                    Log.TWL(0, $"Matched paintPattern: {paintPattern.name} to paintTextureId: {paintTextureId}");
                    camoMaskTexture = paintPattern;
                    this.patternIndex = i;
                    break;
                }
                else
                {
                    Log.TWL(0, $"PaintPattern: {paintPattern.name} did not match target: {paintTextureId}, skipping.");
                }
            }

            if (camoMaskTexture == null)
            {

                int targetPatternId = -1;
                try
                {
                    string paintTextureIdDigits = new String(paintTextureId.Where(Char.IsDigit).ToArray());
                    if (!String.IsNullOrEmpty(paintTextureIdDigits))
                    {
                        targetPatternId = int.Parse(paintTextureIdDigits) - 1; // String is human natural, need an index val
                        Log.TWL(0, $"Matched patternId {targetPatternId} from string.");
                    }
                }
                catch (Exception)
                {
                    Log.TWL(0, $"Failed to match by targetPatternId, falling back to random selection.");
                }

                if (this.paintPatterns != null && this.paintPatterns.Length != 0)
                {
                    if (targetPatternId > -1 && targetPatternId < this.paintPatterns.Length)
                    {
                        camoMaskTexture = this.paintPatterns[targetPatternId];
                        this.patternIndex = targetPatternId;
                        Log.TWL(0, $"camoMask was null, matched on string index: {camoMaskTexture}");
                    }
                    else
                    {
                        this.patternIndex = UnityEngine.Random.Range(0, this.paintPatterns.Length);
                        camoMaskTexture = this.paintPatterns[this.patternIndex];
                        Log.TWL(0, "camoMask was null, created new one randomly. patternIndex: " + this.patternIndex);
                    }
                }
                else
                {
                    Log.TWL(0, "Eck :) didn't think this could happen. Defaulting to paint tex that exists on prefab. I'm keeping original error message");
                    if (this.paintScheme != null && this.paintScheme.paintSchemeTex != null)
                        camoMaskTexture = this.paintScheme.paintSchemeTex;
                    else
                        Log.TWL(0, "No paint scheme on prefab.");
                }

                if (camoMaskTexture == null)
                {
                    Log.TWL(0, "Could not find a paint pattern, defaulting to a black texture.");
                    camoMaskTexture = Texture2D.blackTexture;
                    this.patternIndex = 0;
                }
            }

            Log.WL(1, "  camo mask texture.name:" + camoMaskTexture.name);
            this.paintScheme = new MechPaintScheme(camoMaskTexture, heraldryDef.PrimaryMechColor, heraldryDef.SecondaryMechColor, heraldryDef.TertiaryMechColor);
            this.emblemScheme = new MechEmblemScheme(heraldryDef.TextureLogo, MechEmblemScheme.EmblemSetting.first);
        }

    public void Init(CustomMechRepresentation parent) {
      this.parent = parent;
      this._propertyBlock = parent.customPropertyBlock;
      Log.TWL(0, "CustomMechCustomization.Init propertyBlock:"+(this.propertyBlock == null?"null":this.propertyBlock.gameObject.name));
      if (Application.isPlaying) {
        if ((bool)(UnityEngine.Object)this.decal0)
          this.decal0.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal1)
          this.decal1.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal2)
          this.decal2.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal3)
          this.decal3.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal4)
          this.decal4.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal5)
          this.decal5.gameObject.SetActive(false);
      }
      if (this.baseAlbedoTex == null || this.meshList == null || (this.meshList.Count == 0 || this.transformList == null) || (this.transformList.Count == 0 || this.meshList.Count != this.transformList.Count))
        this.RefreshCache();
      this.BakeEmblem();
      if (this.paintScheme == null)
        return;
      this.SetPaintColors();
    }
    public void Init(CustomMechRepresentationSimGame parent) {
      this.parent = parent;
      this._propertyBlock = parent.customPropertyBlock;
      Log.TWL(0, "CustomMechCustomization.Init propertyBlock:" + (this.propertyBlock == null ? "null" : this.propertyBlock.gameObject.name));
      if (Application.isPlaying) {
        if ((bool)(UnityEngine.Object)this.decal0)
          this.decal0.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal1)
          this.decal1.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal2)
          this.decal2.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal3)
          this.decal3.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal4)
          this.decal4.gameObject.SetActive(false);
        if ((bool)(UnityEngine.Object)this.decal5)
          this.decal5.gameObject.SetActive(false);
      }
      if (this.baseAlbedoTex == null || this.meshList == null || (this.meshList.Count == 0 || this.transformList == null) || (this.transformList.Count == 0 || this.meshList.Count != this.transformList.Count))
        this.RefreshCache();
      this.BakeEmblem();
      if (this.paintScheme == null)
        return;
      this.SetPaintColors();
    }
    private void OnDestroy() {
      if ((UnityEngine.Object)this._bakingMaterial != (UnityEngine.Object)null) {
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this._bakingMaterial);
        this._bakingMaterial = (Material)null;
      }
      if ((UnityEngine.Object)this.emblemTex != (UnityEngine.Object)null) {
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object)this.emblemTex);
        this.emblemTex = (Texture2D)null;
      }
      this.baseAlbedoTex = (Texture2D)null;
      this.emblemTex = (Texture2D)null;
      this.emblemSDF = (Texture2D)null;
      if (this.bakingCommandBuffer != null)
        this.bakingCommandBuffer.Clear();
      this.paintPatterns = (Texture2D[])null;
      this.decal0 = (BTDecal)null;
      this.decal1 = (BTDecal)null;
      this.decal2 = (BTDecal)null;
      this.decal3 = (BTDecal)null;
      this.decal4 = (BTDecal)null;
      this.decal5 = (BTDecal)null;
      this.meshList.Clear();
    }
    private void OnValidate() {
      if (Application.isPlaying)
        this.BakeEmblem();
      if (this.paintScheme == null)
        return;
      this.SetPaintColors();
    }
    public void LogError(string functionName, string text) => Debug.LogError((object)string.Format("Heraldry [{0}] - {1} - {2}", (object)this.name, (object)functionName, (object)text));
  }
}