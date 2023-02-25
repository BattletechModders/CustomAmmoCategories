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
using BattleTech.Rendering.MechCustomization;
using BattleTech.UI;
using HarmonyLib;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomUnits {
  public class MechPaintPatternSelectorWidgetCust: MonoBehaviour {
    public CustomMechCustomization ActiveCustomization { get; set; }
    public void Init(CustomMechCustomization ActiveCustomization) {
      this.ActiveCustomization = ActiveCustomization;
    }
  }
  [HarmonyPatch(typeof(MechPaintPatternSelectorWidget))]
  [HarmonyPatch("TryShow")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(MechRepresentationSimGame) })]
  public static class MechPaintPatternSelectorWidget_TryShow {
    public static void TryShow(this MechPaintPatternSelectorWidget __instance, CustomMechCustomization customization, MechDef def) {
      if (customization == null || def == null) {
        __instance.gameObject.SetActive(false);
      } else {
        if (def == Traverse.Create(__instance).Field<MechDef>("ActiveDef").Value) { return; }
        Traverse.Create(__instance).Field<MechCustomization>("ActiveCustomization").Value = null;
        MechPaintPatternSelectorWidgetCust custWidget = __instance.gameObject.GetComponent<MechPaintPatternSelectorWidgetCust>();
        if (custWidget == null) { custWidget = __instance.gameObject.AddComponent<MechPaintPatternSelectorWidgetCust>(); }
        custWidget.Init(customization);
        Traverse.Create(__instance).Field<MechDef>("ActiveDef").Value = def;
        List<string> stringList1 = new List<string>();
        List<string> stringList2 = new List<string>();
        int length = customization.paintPatterns.Length;
        for (int index = 0; index < length; ++index) {
          stringList1.Add(customization.paintPatterns[index].name);
          stringList2.Add(Strings.T("Pattern {0}", (object)(index + 1)));
        }
        int num = stringList1.IndexOf(customization.paintScheme.paintSchemeTex.name);
        Traverse.Create(__instance).Field<HorizontalScrollSelectorText>("ScrollSelector").Value.ClearOptions();
        Traverse.Create(__instance).Field<HorizontalScrollSelectorText>("ScrollSelector").Value.AddOptions(stringList2.ToArray());
        Traverse.Create(__instance).Field<HorizontalScrollSelectorText>("ScrollSelector").Value.selectionIdx = num;
        __instance.gameObject.SetActive(customization.paintPatterns.Length > 1);
      }
    }

    public static bool Prefix(MechPaintPatternSelectorWidget __instance, MechRepresentationSimGame mechRep) {
      try {
        CustomMechRepresentationSimGame simGame = mechRep as CustomMechRepresentationSimGame;
        if (simGame == null) {
          MechPaintPatternSelectorWidgetCust custWidget = __instance.gameObject.GetComponent<MechPaintPatternSelectorWidgetCust>();
          if (custWidget == null) { custWidget = __instance.gameObject.AddComponent<MechPaintPatternSelectorWidgetCust>(); }
          custWidget.Init(null);
          return true;
        }
        __instance.TryShow(simGame.defaultMechCustomization, simGame._mechDef);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechPaintPatternSelectorWidget))]
  [HarmonyPatch("OnValueChanged")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechPaintPatternSelectorWidget_OnValueChanged {
    public static bool Prefix(MechPaintPatternSelectorWidget __instance) {
      try {
        if (Traverse.Create(__instance).Field<MechCustomization>("ActiveCustomization").Value != null) { return true; }
        if (Traverse.Create(__instance).Field<MechDef>("ActiveDef").Value == null) { return true; }
        if (Traverse.Create(__instance).Field<bool>("ableToChange").Value == false) { return true; }
        if (Traverse.Create(__instance).Field<bool>("skipOnValueChanged").Value == true) { return true; }
        MechPaintPatternSelectorWidgetCust custWidget = __instance.gameObject.GetComponent<MechPaintPatternSelectorWidgetCust>();
        if (custWidget == null) { return true; }
        int length = custWidget.ActiveCustomization.paintPatterns.Length;
        int selectionIdx = Traverse.Create(__instance).Field<HorizontalScrollSelectorText>("ScrollSelector").Value.selectionIdx;
        if (selectionIdx > length) { return false; }
        string name = custWidget.ActiveCustomization.paintPatterns[selectionIdx].name;
        if (name == custWidget.ActiveCustomization.paintScheme.paintSchemeTex.name) { return false; }
        Traverse.Create(__instance).Field<MechDef>("ActiveDef").Value.UpdatePaintTextureId(name);
        __instance.SetAllowInput(false);
        __instance.RefreshPaintSelector();
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(MechLabPanel))]
  [HarmonyPatch("RefreshPaintSelector")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechLabPanel_RefreshPaintSelector {
    public static bool Prefix(MechLabPanel __instance, MechPaintPatternSelectorWidget ___paintSelector) {
      try {
        if (__instance.Sim == null || __instance.Sim.RoomManager.MechBayRoom.LoadedMechObject == null) { return true; }
        CustomMechRepresentationSimGame component = __instance.Sim.RoomManager.MechBayRoom.LoadedMechObject.GetComponent<CustomMechRepresentationSimGame>();
        if (component == null) { return true; }
        ___paintSelector.SetAllowInput(true);
        ___paintSelector.TryShow(component);
        component.loadCustomization(__instance.Sim.Player1sMercUnitHeraldryDef);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
        return true;
      }
    }
  }

  public class ComponentRepresentationSimGameInfo {
    public MechComponentRef componentRef { get; private set; }
    public ComponentRepresentation componentRep { get; set; }
    public ChassisLocations prefabLocation { get; private set; }
    public ChassisLocations attachLocation { get; private set; }
    public ComponentRepresentationSimGameInfo(MechComponentRef componentRef, ChassisLocations prefabLocation, ChassisLocations attachLocation) {
      this.componentRef = componentRef;
      this.prefabLocation = prefabLocation;
      this.attachLocation = attachLocation;
      this.componentRep = null;
    }
  }
  public interface ICustomizationTarget {
    ChassisDef chassisDef { get; }
    GameObject _gameObject { get; }
    GameObject _visibleObject { get; }
    Dictionary<MeshRenderer, bool> meshRenderersCache { get; }
    Dictionary<SkinnedMeshRenderer, bool> skinnedMeshRenderersCache { get; }
    void UnregisterRenderers(GameObject src);
    void RegisterRenderersComponentRepresentation(ComponentRepresentation src);
    void RegisterRenderersCustomHeraldry(GameObject src, MeshRenderer paintSchemePlaceholder);
    void RegisterRenderersMainHeraldry(GameObject src);
    void RegisterColliders(GameObject src);
  }
  public class QuadRepresentationSimGame : CustomMechRepresentationSimGame {
    public virtual CustomMechRepresentationSimGame ForwardLegs { get; set; }
    public virtual CustomMechRepresentationSimGame RearLegs { get; set; }
    public virtual QuadVisualInfo quadVisualInfo { get; set; }
    public virtual QuadBodyKinematic quadBodyKinematic { get; set; }

    public virtual GameObject QuadBody { get; set; }
    public static void SuppressVisuals(CustomMechRepresentationSimGame rep, GameObject VisibleObject, List<string> sr, List<string> nsr) {
      HashSet<string> suppress = new HashSet<string>();
      HashSet<string> notSuppress = new HashSet<string>();
      foreach (string name in sr) { suppress.Add(name); }
      foreach (string name in nsr) { notSuppress.Add(name); }
      HashSet<GameObject> deleteObjects = new HashSet<GameObject>();
      SkinnedMeshRenderer[] skinnedRenderers = VisibleObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer renderer in skinnedRenderers) {
        if (renderer == null) { continue; }
        foreach (string name in suppress) {
          if (renderer.gameObject.name.Contains(name)) { deleteObjects.Add(renderer.gameObject); }
        }
        if (notSuppress.Count != 0) {
          bool notDel = false;
          foreach (string name in notSuppress) {
            if (renderer.gameObject.name.Contains(name)) { notDel = true; break; }
          }
          if (notDel == false) { deleteObjects.Add(renderer.gameObject); }
        }
      }
      MeshRenderer[] meshRenderers = VisibleObject.GetComponentsInChildren<MeshRenderer>(true);
      foreach (MeshRenderer renderer in meshRenderers) {
        if (renderer == null) { continue; }
        foreach (string name in suppress) {
          if (renderer.gameObject.name.Contains(name)) { deleteObjects.Add(renderer.gameObject); }
        }
        if (notSuppress.Count != 0) {
          bool notDel = false;
          foreach (string name in notSuppress) {
            if (renderer.gameObject.name.Contains(name)) { notDel = true; break; }
          }
          if (notDel == false) { deleteObjects.Add(renderer.gameObject); }
        }
      }
      foreach (GameObject go in deleteObjects) {
        if (go == null) { continue; }
        rep.UnregisterRenderers(go);
        GameObject.DestroyImmediate(go);
      };
    }
    public virtual void AddForwardLegs(CustomMechRepresentationSimGame flegs) {
      Log.TWL(0, "QuadRepresentationSimGame.AddForwardLegs " + this.gameObject.name + " " + (flegs == null ? "null" : flegs.name));
      this.ForwardLegs = flegs;
      flegs.transform.SetParent(j_Root);
      //frontLegs.HardpointData = dataManager.GetObjectOfType<HardpointDataDef>(customInfo.quadVisualInfo.);
      flegs.VisibleObject.name = "front_legs";
      flegs.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      Log.WL(1, "SuppressVisuals " + (flegs.VisibleObject == null ? "null" : flegs.VisibleObject.name) + " quadVisualInfo:" + (quadVisualInfo == null ? "null" : "not null"));
      QuadRepresentationSimGame.SuppressVisuals(flegs, flegs.VisibleObject, quadVisualInfo.SuppressRenderers, quadVisualInfo.NotSuppressRenderers);
      this.VisualObjects.Add(flegs.VisibleObject);
      flegs.parentRepresentation = this;
      if (quadBodyKinematic != null) {
        quadBodyKinematic.FrontLegsAttach = this.ForwardLegs.vfxCenterTorsoTransform;
      }
      this.vfxLeftArmTransform = this.ForwardLegs.vfxLeftLegTransform;
      this.vfxRightArmTransform = this.ForwardLegs.vfxRightLegTransform;
      this.vfxLeftShoulderTransform = this.ForwardLegs.vfxLeftLegTransform;
      this.vfxRightShoulderTransform = this.ForwardLegs.vfxRightLegTransform;
      this.LeftArmAttach = this.ForwardLegs.LeftLegAttach;
      this.RightArmAttach = this.ForwardLegs.RightLegAttach;
      this.leftArmDestructible = this.ForwardLegs.leftLegDestructible;
      this.rightArmDestructible = this.ForwardLegs.rightLegDestructible;
    }
    public virtual void AddRearLegs(CustomMechRepresentationSimGame rlegs) {
      Log.TWL(0, "QuadRepresentationSimGame.AddRearLegs " + this.gameObject.name + " " + (rlegs == null ? "null" : rlegs.name));
      this.RearLegs = rlegs;
      rlegs.transform.SetParent(j_Root);
      //frontLegs.HardpointData = dataManager.GetObjectOfType<HardpointDataDef>(customInfo.quadVisualInfo.);
      rlegs.VisibleObject.name = "rear_legs";
      rlegs.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      QuadRepresentationSimGame.SuppressVisuals(rlegs, rlegs.VisibleObject, quadVisualInfo.SuppressRenderers, quadVisualInfo.NotSuppressRenderers);
      this.VisualObjects.Add(rlegs.VisibleObject);
      rlegs.parentRepresentation = this;
      if (quadBodyKinematic != null) {
        quadBodyKinematic.RearLegsAttach = this.RearLegs.vfxCenterTorsoTransform;
      }
      this.vfxLeftLegTransform = this.RearLegs.vfxLeftLegTransform;
      this.vfxRightLegTransform = this.RearLegs.vfxRightLegTransform;
      this.LeftLegAttach = this.RearLegs.LeftLegAttach;
      this.RightLegAttach = this.RearLegs.RightLegAttach;
      this.leftLegDestructible = this.RearLegs.leftLegDestructible;
      this.rightLegDestructible = this.RearLegs.rightLegDestructible;
    }

    public virtual void InitDestructable(Transform bodyRoot) {
      if (this.quadVisualInfo == null) { return; }
      foreach (var destr in this.quadVisualInfo.Destructables) {
        Transform obj = bodyRoot.FindRecursive(destr.Value.Name);
        Transform wholeObj = bodyRoot.FindRecursive(destr.Value.wholeObj);
        Transform destroyedObj = bodyRoot.FindRecursive(destr.Value.destroyedObj);
        if (obj == null) { continue; }
        if (wholeObj == null) { continue; }
        if (destroyedObj == null) { continue; }
        MechDestructibleObject dObj = obj.gameObject.AddComponent<MechDestructibleObject>();
        dObj.destroyedObj = destroyedObj.gameObject;
        dObj.wholeObj = wholeObj.gameObject;
        switch (destr.Key) {
          case ChassisLocations.Head: this.headDestructible = dObj; break;
          case ChassisLocations.CenterTorso: this.centerTorsoDestructible = dObj; break;
          case ChassisLocations.LeftTorso: this.leftTorsoDestructible = dObj; break;
          case ChassisLocations.RightTorso: this.rightTorsoDestructible = dObj; break;
        }
      }
    }

    public virtual void AddBody(GameObject bodyGo, DataManager dataManager) {
      Log.TWL(0, "QuadRepresentationSimGame.AddBody "+bodyGo.name);
      Transform bodyRoot = bodyGo.transform.FindRecursive("j_Root");
      Transform bodyMesh = bodyGo.transform.FindTopLevelChild("mesh");
      Transform camoholderGo = bodyGo.transform.FindTopLevelChild("camoholder");
      MeshRenderer camoholder = null;
      if (camoholderGo != null) {
        Log.WL(1, "camoholderGo found");
        camoholder = camoholderGo.gameObject.GetComponent<MeshRenderer>();
      }
      if (camoholder != null) {
        Log.WL(1, "camoholder found sharedMaterials:" + camoholder.sharedMaterials.Length);
      }

      if (bodyRoot != null) {
        bodyRoot.name = "j_QuadSkeleton";
        bodyRoot.SetParent(this.j_Root);
        quadBodyKinematic = bodyRoot.gameObject.AddComponent<QuadBodyKinematic>();
        quadBodyKinematic.BodyTransform = bodyRoot;//.FindRecursive("j_QuadBody");
        this.vfxCenterTorsoTransform = bodyRoot.FindRecursive("CT_vfx_transform");
        this.vfxLeftTorsoTransform = bodyRoot.FindRecursive("LT_vfx_transform");
        this.vfxRightTorsoTransform = bodyRoot.FindRecursive("RT_vfx_transform");
        this.vfxHeadTransform = bodyRoot.FindRecursive("HEAD_vfx_transform");
        this.TorsoAttach = bodyRoot.FindRecursive("CT_vfx_transform");
        this.headDestructible = null;
        this.centerTorsoDestructible = null;
        this.leftTorsoDestructible = null;
        this.rightTorsoDestructible = null;
        this.InitDestructable(bodyRoot);
      }
      if (bodyMesh != null) {
        bodyMesh.gameObject.name = "quad_body";
        if (camoholderGo != null) {
          camoholder = camoholderGo.gameObject.GetComponent<MeshRenderer>();
          camoholderGo.transform.SetParent(bodyMesh);
        }
        bodyMesh.SetParent(this.VisibleObject.transform);
        if (string.IsNullOrEmpty(quadVisualInfo.BodyShaderSource) == false) {
          GameObject shaderSource = dataManager.PooledInstantiate(quadVisualInfo.BodyShaderSource, BattleTechResourceType.Prefab);
          if (shaderSource != null) {
            Log.WL(1, "shader prefab found");
            Renderer shaderComponent = shaderSource.GetComponentInChildren<Renderer>();
            Renderer[] shaderTargets = bodyMesh.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in shaderTargets) {
              if (renderer.gameObject.name.StartsWith("camoholder")) { continue; }
              Log.WL(2, "renderer:" + renderer.name);
              for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                Log.WL(3, "material:" + renderer.materials[mindex].name + " <- " + shaderComponent.material.shader.name);
                renderer.materials[mindex].shader = shaderComponent.material.shader;
                renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
              }
            }
            dataManager.PoolGameObject(quadVisualInfo.BodyShaderSource, shaderSource);
          }
        }
        this.VisualObjects.Add(bodyMesh.gameObject);
        //bodyMesh.gameObject.InitBindPoses();
        if ((this.quadVisualInfo != null) && (this.customRep != null)) {
          foreach (string bodyAnim in this.quadVisualInfo.Animators) { this.customRep.AddBodyAnimator(this.transform, bodyAnim); }
          //foreach (string twistAnim in this.quadVisualInfo.TwistAnimators) { this.customRep.AddTwistAnimator(this.transform, twistAnim); }
          foreach (AttachInfoRecord wAttach in this.quadVisualInfo.WeaponsAttachPoints) { this.customRep.AddWeaponAttachPoint(this.gameObject, wAttach); }
          this.customRep.InBattle = false;
        }
        this.RegisterRenderersCustomHeraldry(bodyMesh.gameObject, camoholder);
        this.customPropertyBlock.UpdateCache();
      }
    }
    public override void InitSlaves(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      if (this.ForwardLegs != null) {
        this.ForwardLegs.DataManager = dataManager;
        this.ForwardLegs._mechDef = mechDef;
        this.ForwardLegs.rootTransform.parent = parentTransform;
        this.ForwardLegs.rootTransform.localPosition = new Vector3(0f, 0f, quadVisualInfo.BodyLength / 2f); ;
        this.ForwardLegs.rootTransform.localScale = Vector3.one;
        this.ForwardLegs.rootTransform.localRotation = Quaternion.identity;
      }
      if (this.RearLegs != null) {
        this.RearLegs.DataManager = dataManager;
        this.RearLegs._mechDef = mechDef;
        this.RearLegs.transform.localPosition = new Vector3(0f, 0f, quadVisualInfo.BodyLength / 2f);
        this.RearLegs.rootTransform.parent = parentTransform;
        this.RearLegs.rootTransform.localPosition = new Vector3(0f, 0f, -quadVisualInfo.BodyLength / 2f); ;
        this.RearLegs.rootTransform.localScale = Vector3.one;
        this.RearLegs.rootTransform.localRotation = Quaternion.identity;
      }
    }
    public override void InitSimGameRepresentation(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      this.DataManager = dataManager;
      this._mechDef = mechDef;
      //this.prefabName = string.Format("chrPrfComp_{0}_simgame", string.IsNullOrEmpty(mechDef.prefabOverride) ? (object)mechDef.Chassis.PrefabBase : (object)mechDef.prefabOverride.Replace("chrPrfMech_", ""));
      this.prefabName = mechDef.GetCustomSimGamePrefabName();
      this.rootTransform.parent = parentTransform;
      this.rootTransform.localPosition = Vector3.zero;
      this.rootTransform.localScale = Vector3.one;
      this.rootTransform.localRotation = Quaternion.identity;
      this.InitSlaves(dataManager, mechDef, this.j_Root, heraldryDef);
      this.ClearComponentReps();
      this.LoadWeapons();
      this.loadCustomization(heraldryDef);
      this.loadDamageState();
      this.gameObject.AddComponent<ShadowTracker>();
      this.mouseRotation = this.GetComponent<MouseRotation>();
      if (this.mouseRotation == null) { this.mouseRotation = this.gameObject.AddComponent<MouseRotation>(); }
      MechSpin.Patches.MechRepresentationSimGame_Init_Patch.Postfix(this);
    }
    public override void ClearComponentReps() {
      base.ClearComponentReps();
      if (this.ForwardLegs != null) { this.ForwardLegs.ClearComponentReps(); }
      if (this.RearLegs != null) { this.RearLegs.ClearComponentReps(); }
    }

    public override void loadCustomization(HeraldryDef heraldryDef) 
    {
        Log.TWL(0, $"Applying heraldry for unit: {this._mechDef?.Description?.Name}");
        base.loadCustomization(heraldryDef);
        if (this.ForwardLegs != null) 
        {
            Log.TWL(0, $"Applying ForwardLegs heraldry: {this.ForwardLegs._mechDef?.Description?.Name}");
            this.ForwardLegs.loadCustomization(heraldryDef); 
        }
        if (this.RearLegs != null)
        {
            Log.TWL(0, $"Applying RearLegs heraldry: {this.RearLegs._mechDef?.Description?.Name}");
            this.RearLegs.loadCustomization(heraldryDef);
        }
  
    }

    public override void InitWeapons(List<ComponentRepresentationSimGameInfo> compInfo, string parentDisplayName) {
      base.InitWeapons(compInfo, parentDisplayName);
    }
  }
  public class SquadRepresentationSimGame : CustomMechRepresentationSimGame {
    public Dictionary<ChassisLocations, CustomMechRepresentationSimGame> squad { get; set; } = new Dictionary<ChassisLocations, CustomMechRepresentationSimGame>();
    public virtual void AddUnit(CustomMechRepresentationSimGame unit) {
      int index = squad.Count;
      ChassisLocations location = TrooperSquad.locations[index];
      //this.slaveRepresentations.Add(unit);
      this.squad.Add(location, unit);
      unit.parentRepresentation = this;
      this.VisualObjects.Add(unit.VisibleObject);
      unit.VisibleObject.name = "unit_" + index;
      unit.VisibleObject.transform.SetParent(this.VisibleObject.transform);
      switch (location) {
        case ChassisLocations.CenterTorso: this.vfxCenterTorsoTransform = unit.vfxCenterTorsoTransform; this.TorsoAttach = unit.TorsoAttach; break;
        case ChassisLocations.LeftTorso: this.vfxLeftTorsoTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.RightTorso: this.vfxRightTorsoTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.Head: this.vfxHeadTransform = unit.vfxCenterTorsoTransform; break;
        case ChassisLocations.LeftArm: this.vfxLeftArmTransform = unit.vfxCenterTorsoTransform; this.vfxLeftShoulderTransform = unit.vfxCenterTorsoTransform; this.LeftArmAttach = unit.TorsoAttach; break;
        case ChassisLocations.RightArm: this.vfxRightArmTransform = unit.vfxCenterTorsoTransform; this.vfxRightShoulderTransform = unit.vfxCenterTorsoTransform; this.RightArmAttach = unit.TorsoAttach; break;
        case ChassisLocations.LeftLeg: this.vfxRightLegTransform = unit.vfxCenterTorsoTransform; this.RightLegAttach = unit.TorsoAttach; break;
        case ChassisLocations.RightLeg: this.vfxRightLegTransform = unit.vfxCenterTorsoTransform; this.RightLegAttach = unit.TorsoAttach; break;
      }
    }
    public override void LoadWeapons() {
      foreach (var unit in squad) {
        List<ComponentRepresentationSimGameInfo> compInfo = new List<ComponentRepresentationSimGameInfo>();
        foreach (MechComponentRef compRef in this._mechDef.Inventory) {
          if (compRef.MountedLocation != unit.Key) { continue; }
          ChassisLocations location = ChassisLocations.CenterTorso;
          if(compRef.Def is WeaponDef weapon) {
            if (this.chassisDef.GetCustomInfo().SquadInfo.Hardpoints.TryGetValue(weapon.WeaponCategoryValue.Name, out location)) {

            }
          }
          ComponentRepresentationSimGameInfo cmpInfo = new ComponentRepresentationSimGameInfo(compRef, location, location);
          compInfo.Add(cmpInfo);
        }
        unit.Value.InitWeapons(compInfo, this._mechDef.Description.Id+"_"+unit.Key);
      }
    }
    public override void collapseLocation(int location, bool isDestroyed) {
      if(this.squad.TryGetValue((ChassisLocations)location, out CustomMechRepresentationSimGame unit)) {
        unit.collapseAll(isDestroyed);
      }
    }
    public override void ClearComponentReps() {
      foreach(var unit in this.squad) { unit.Value.ClearComponentReps(); }
    }
    public override void loadCustomization(HeraldryDef heraldryDef) {
      foreach (var unit in this.squad) { unit.Value.loadCustomization(heraldryDef); }
    }
    public override void loadDamageState() {
      foreach (ChassisLocations loc in Enum.GetValues(typeof(ChassisLocations))) {
        switch (loc) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default:
          this.collapseLocation((int)loc, (double)this._mechDef.GetLocationLoadoutDef(loc).CurrentInternalStructure <= 0.0);
          continue;
        }
      }
    }
    public override void InitSlaves(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      foreach (var unit in this.squad) {
        unit.Value.DataManager = dataManager;
        unit.Value._mechDef = mechDef;
        unit.Value.rootTransform.parent = parentTransform;
        unit.Value.rootTransform.localRotation = Quaternion.identity;
        this.rootTransform.localScale = Vector3.one;
        float xd = 0f;
        float yd = 0f;
        if (unit.Key != ChassisLocations.Head) {
          xd = Mathf.Cos(Mathf.Deg2Rad * TrooperSquad.positions[unit.Key]) * TrooperSquad.SquadRadius;
          yd = Mathf.Sin(Mathf.Deg2Rad * TrooperSquad.positions[unit.Key]) * TrooperSquad.SquadRadius;
        }
        Vector3 unitPos = new Vector3(xd, 0f, yd);
        unit.Value.rootTransform.localPosition = unitPos;
        unit.Value.InitSlaves(dataManager, mechDef, unit.Value.j_Root, heraldryDef);
      }
    }
    public override void InitSimGameRepresentation(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      this.DataManager = dataManager;
      this._mechDef = mechDef;
      //this.prefabName = string.Format("chrPrfComp_{0}_simgame", string.IsNullOrEmpty(mechDef.prefabOverride) ? (object)mechDef.Chassis.PrefabBase : (object)mechDef.prefabOverride.Replace("chrPrfMech_", ""));
      this.prefabName = mechDef.GetCustomSimGamePrefabName();
      this.rootTransform.parent = parentTransform;
      this.rootTransform.localPosition = Vector3.zero;
      this.rootTransform.localScale = Vector3.one;
      this.rootTransform.localRotation = Quaternion.identity;
      this.InitSlaves(dataManager, mechDef, this.j_Root, heraldryDef);
      this.ClearComponentReps();
      this.LoadWeapons();
      this.loadCustomization(heraldryDef);
      this.loadDamageState();
      foreach (var unit in this.squad) { unit.Value.gameObject.AddComponent<ShadowTracker>(); }
      this.mouseRotation = this.GetComponent<MouseRotation>();
      if (this.mouseRotation == null) { this.mouseRotation = this.gameObject.AddComponent<MouseRotation>(); }
      MechSpin.Patches.MechRepresentationSimGame_Init_Patch.Postfix(this);
    }
  }
  public class CustomMechRepresentationSimGame : MechRepresentationSimGame, ICustomizationTarget {
    public Transform f_j_Root = null;
    public Transform j_Root {
      get { if (f_j_Root == null) { f_j_Root = this.transform.FindRecursive("j_Root"); }; return f_j_Root; }
    }
    public HashSet<CustomParticleSystemRep> CustomParticleSystemReps { get; set; } = new HashSet<CustomParticleSystemRep>();
    public CustomMechRepresentationSimGame parentRepresentation { get; set; } = null;
    public HashSet<GameObject> VisualObjects { get; set; } = new HashSet<GameObject>();
    public GameObject _gameObject { get { return this.gameObject; } }
    public GameObject _visibleObject { get { return this.VisibleObject; } }
    public string PrefabBase { get; set; }
    public ChassisDef chassisDef { get; set; } = null;
    public HardpointDataDef HardpointData { get; set; }
    public PropertyBlockManager propertyBlock { get; set; }
    public CustomPropertyBlockManager customPropertyBlock { get { return propertyBlock as CustomPropertyBlockManager; } }
    public GameObject VisibleObject { get; set; }
    public CustomMechCustomization defaultMechCustomization { get; set; }
    public virtual HashSet<Collider> selfColliders { get; set; } = new HashSet<Collider>();
    public virtual Dictionary<MeshRenderer, bool> meshRenderersCache { get; set; } = new Dictionary<MeshRenderer, bool>();
    public virtual Dictionary<SkinnedMeshRenderer, bool> skinnedMeshRenderersCache { get; set; } = new Dictionary<SkinnedMeshRenderer, bool>();
    public virtual CustomRepresentation customRep { get; set; }
    public new virtual Dictionary<ChassisLocations, HashSet<ComponentRepresentation>> componentReps { get; set; } = new Dictionary<ChassisLocations, HashSet<ComponentRepresentation>>();
    public virtual void componentReps_Add(ChassisLocations location, ComponentRepresentation rep) {
      if(componentReps.TryGetValue(location, out HashSet<ComponentRepresentation> reps) == false) {
        reps = new HashSet<ComponentRepresentation>();
        componentReps.Add(location, reps);
      }
      reps.Add(rep);
    }
    public DataManager DataManager {
      get {
        return Traverse.Create(this).Field<DataManager>("dataManager").Value;
      }
      set {
        Traverse.Create(this).Field<DataManager>("dataManager").Value = value;
      }
    }
    private MethodInfo set_mechDef = typeof(MechRepresentationSimGame).GetProperty("mechDef", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
    public MechDef _mechDef { get; set; }
    //public MechDef MechDef {
    //  get {
    //    return this.mechDef;
    //  }
    //  set {
    //    set_mechDef.Invoke(this,new object[1] { value } );
    //  }
    //}
    //public virtual void CreateBlankPrefabs(List<string> usedPrefabNames, ChassisLocations location) {
    //  List<string> componentBlankNames = MechHardpointRules.GetComponentBlankNames(usedPrefabNames, this.mechDef, location);
    //  Transform attachTransform = this.GetAttachTransform(location);
    //  for (int index = 0; index < componentBlankNames.Count; ++index) {
    //    ComponentRepresentation component = this.DataManager.PooledInstantiate(componentBlankNames[index], BattleTechResourceType.Prefab).GetComponent<ComponentRepresentation>();
    //    component.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
    //    component.gameObject.SetActive(true);
    //    component.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
    //    component.gameObject.name = componentBlankNames[index];
    //    this.componentReps_Add(location, component);
    //  }
    //}
    public virtual void InitSlaves(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {

    }
    public virtual void InitSimGameRepresentation(DataManager dataManager, MechDef mechDef, Transform parentTransform, HeraldryDef heraldryDef) {
      Log.TWL(0, "CustomMechRepresentationSimGame.InitSimGameRepresentation def:"+mechDef.Description.Id+" object:"+this.gameObject.name+ " heraldry:"+(heraldryDef == null?"null":(heraldryDef.Description.Id+" c1:"+heraldryDef.PrimaryMechColor.paintLayer.paintColor.ToString())));
      this.DataManager = dataManager;
      this._mechDef = mechDef;
      //this.prefabName = string.Format("chrPrfComp_{0}_simgame", string.IsNullOrEmpty(mechDef.prefabOverride) ? (object)mechDef.Chassis.PrefabBase : (object)mechDef.prefabOverride.Replace("chrPrfMech_", ""));
      this.prefabName = mechDef.GetCustomSimGamePrefabName();
      this.rootTransform.parent = parentTransform;
      this.rootTransform.localPosition = Vector3.zero;
      this.rootTransform.localScale = Vector3.one;
      this.rootTransform.localRotation = Quaternion.identity;
      this.InitSlaves(dataManager, mechDef, this.j_Root, heraldryDef);
      this.ClearComponentReps();
      this.LoadWeapons();
      this.loadCustomization(heraldryDef);
      this.loadDamageState();
      this.gameObject.AddComponent<ShadowTracker>();
      this.mouseRotation = this.GetComponent<MouseRotation>();
      if (this.mouseRotation == null) { this.mouseRotation = this.gameObject.AddComponent<MouseRotation>(); }
      MechSpin.Patches.MechRepresentationSimGame_Init_Patch.Postfix(this);
    }

    public virtual void loadCustomization(HeraldryDef heraldryDef) {
      Log.TWL(0, "CustomMechRepresentationSimGame.loadCustomization "
        + " customization:"+ (this.defaultMechCustomization==null?"null": this.defaultMechCustomization.gameObject.name)
        + " heraldryDef:" + (heraldryDef == null ? "null" : heraldryDef.Description.Id)
        + " mechDef:" + (this._mechDef == null ? "null" : this._mechDef.Description.Id)
      );

      try {
        if (this.defaultMechCustomization != null && heraldryDef != null && this._mechDef != null) {
          string paintTextureId = this._mechDef.PaintTextureID;
          SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
          
          if (string.IsNullOrEmpty(paintTextureId) && simulation != null) {
            if (!simulation.pilotableActorPaintDiscardPile.ContainsKey(this._mechDef.Description.Id))
              simulation.pilotableActorPaintDiscardPile.Add(this._mechDef.Description.Id, new List<string>());
            List<string> stringList1 = simulation.pilotableActorPaintDiscardPile[this._mechDef.Description.Id];
            List<string> stringList2 = new List<string>();
            foreach (Texture2D paintPattern in this.defaultMechCustomization.paintPatterns) {
              if (!stringList1.Contains(paintPattern.name))
                stringList2.Add(paintPattern.name);
            }
            if (stringList1.Count >= stringList2.Count)
              stringList1.Clear();
            if (stringList2.Count > 0) {
              string textureId = stringList2[UnityEngine.Random.Range(0, stringList2.Count - 1)];
              Log.TWL(0, $"Applying paint scheme textureId: {textureId}");
              this._mechDef.UpdatePaintTextureId(textureId);
              stringList1.Add(textureId);
            }
          }

          this.defaultMechCustomization.ApplyHeraldry(heraldryDef, this._mechDef.PaintTextureID);
        } else {
          ColorSwatch layer0 = Resources.Load("Swatches/Player1_0", typeof(ColorSwatch)) as ColorSwatch;
          ColorSwatch layer1 = Resources.Load("Swatches/Player1_1", typeof(ColorSwatch)) as ColorSwatch;
          ColorSwatch layer2 = Resources.Load("Swatches/Player1_2", typeof(ColorSwatch)) as ColorSwatch;
          if (this.defaultMechCustomization != null && this.defaultMechCustomization.paintPatterns != null && this.defaultMechCustomization.paintPatterns.Length != 0)
            this.defaultMechCustomization.paintScheme = new MechPaintScheme(this.defaultMechCustomization.paintPatterns[UnityEngine.Random.Range(0, this.defaultMechCustomization.paintPatterns.Length)], layer0, layer1, layer2);
          else
            Log.TWL(0, this.chassisDef.Description.Id + " problem initializing custom paint scheme");
        }
      } catch(Exception e) {
        Log.TWL(0,e.ToString(),true);
      }
            Log.TWL(0, "CustomMechRepresentationSimGame.loadCustomization DONE");
    }


    public virtual void ClearComponentReps() {
      HashSet<ComponentRepresentation> toPool = new HashSet<ComponentRepresentation>();
      foreach (var reps in this.componentReps) {
        foreach (ComponentRepresentation rep in reps.Value) {
          toPool.Add(rep);
        }
      }
      this.componentReps.Clear();
      foreach (ComponentRepresentation rep in toPool) {
        this.DataManager.PoolGameObject(rep.gameObject.name, rep.gameObject);
      }
    }
    public virtual void LoadWeapons() {
      List<ComponentRepresentationSimGameInfo> compInfo = new List<ComponentRepresentationSimGameInfo>();      
      foreach (MechComponentRef compRef in this._mechDef.Inventory) {
        ComponentRepresentationSimGameInfo cmpInfo = new ComponentRepresentationSimGameInfo(compRef, compRef.MountedLocation, compRef.MountedLocation);
        compInfo.Add(cmpInfo);
      }
      this.InitWeapons(compInfo, this._mechDef.Description.Id);
    }
    public void CopyFrom(CustomMechRepresentationSimGame parent) {
      this.CopyFrom((MechRepresentationSimGame)parent);
      this.PrefabBase = parent.PrefabBase;
      this.chassisDef = chassisDef;
      this.HardpointData = parent.HardpointData;
      this.propertyBlock = parent.propertyBlock;
      this.VisibleObject = parent.VisibleObject;
      this.defaultMechCustomization = parent.defaultMechCustomization;
      this.selfColliders = parent.selfColliders;
      this.meshRenderersCache = parent.meshRenderersCache;
      this.skinnedMeshRenderersCache = parent.skinnedMeshRenderersCache;
      this.customRep = parent.customRep;
    }
    public void CopyFrom(MechRepresentationSimGame parent) {
      this.DataManager = Traverse.Create(parent).Field<DataManager>("dataManager").Value;
      this.prefabName = parent.prefabName;
      this.rootTransform = parent.rootTransform;
      this.thisAnimator = parent.thisAnimator;
      this.mechCustomization = parent.mechCustomization;
      this.LeftArmAttach = parent.LeftArmAttach;
      this.RightArmAttach = parent.RightArmAttach;
      this.TorsoAttach = parent.TorsoAttach;
      this.LeftLegAttach = parent.LeftLegAttach;
      this.RightLegAttach = parent.RightLegAttach;
      this.vfxCenterTorsoTransform = parent.vfxCenterTorsoTransform;
      this.vfxLeftTorsoTransform = parent.vfxLeftTorsoTransform;
      this.vfxRightTorsoTransform = parent.vfxRightTorsoTransform;
      this.vfxHeadTransform = parent.vfxHeadTransform;
      this.vfxLeftArmTransform = parent.vfxLeftArmTransform;
      this.vfxRightArmTransform = parent.vfxRightArmTransform;
      this.vfxLeftLegTransform = parent.vfxLeftLegTransform;
      this.vfxRightLegTransform = parent.vfxRightLegTransform;
      this.vfxLeftShoulderTransform = parent.vfxLeftShoulderTransform;
      this.vfxRightShoulderTransform = parent.vfxRightShoulderTransform;
      this.headDestructible = parent.headDestructible;
      this.centerTorsoDestructible = parent.centerTorsoDestructible;
      this.leftTorsoDestructible = parent.leftTorsoDestructible;
      this.rightTorsoDestructible = parent.rightTorsoDestructible;
      this.leftArmDestructible = parent.leftArmDestructible;
      this.rightArmDestructible = parent.rightArmDestructible;
      this.leftLegDestructible = parent.leftLegDestructible;
      this.rightLegDestructible = parent.rightLegDestructible;
      typeof(MechRepresentationSimGame).GetMethod("ClearComponentReps", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(parent, new object[] { });
      //this.componentReps = parent.componentReps;
      this.mouseRotation = parent.mouseRotation;
      Transform visibleObject = this.transform.FindRecursive("mesh");
      if (visibleObject != null) { this.VisibleObject = visibleObject.gameObject; } else { this.VisibleObject = this.gameObject; }
    }
    public virtual void RegisterColliders(GameObject src) {
      Log.TWL(0, "CustomMechRepresentation.RegisterColliders " + src.name);
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        Log.WL(1, collider.gameObject.name);
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersMainHeraldry(GameObject src) {
      Log.TW(0, "CustomMechRepresentationSimGame.RegisterRenderersMainHeraldry: " + this.gameObject.name + " " + src.name);
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      Log.WL(1, "MeshRenderers:" + mRenderer.Length + " SkinnedMeshRenderer:" + sRenderer.Length);
      Dictionary<Renderer, MeshRenderer> customCamoHolders = new Dictionary<Renderer, MeshRenderer>();
      Transform[] trs = src.GetComponentsInChildren<Transform>();
      foreach (Transform tr in trs) {
        if (tr.name.StartsWith("camoholder") == false) { continue; }
        MeshRenderer camoholder = tr.gameObject.GetComponent<MeshRenderer>();
        if (camoholder == null) { continue; }
        SkinnedMeshRenderer parentSkinnedRenderer = tr.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
        MeshRenderer parentMeshRenderer = tr.parent.gameObject.GetComponent<MeshRenderer>();
        Renderer parentRenderer = parentSkinnedRenderer;
        if (parentRenderer == null) { parentRenderer = parentMeshRenderer; }
        if (parentRenderer == null) { continue; }
        if (customCamoHolders.ContainsKey(parentRenderer)) { continue; }
        customCamoHolders.Add(parentRenderer, camoholder);
      }
      foreach (MeshRenderer renderer in mRenderer) {
        if (renderer.gameObject.GetComponent<BTDecal>() != null) { continue; }
        if (meshRenderersCache.ContainsKey(renderer)) { continue; }
        Log.WL(1, "renderer:" + renderer.gameObject.name + " heraldry:" + true);
        this.meshRenderersCache.Add(renderer, true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, this.defaultMechCustomization);
        }
      }
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        if (skinnedMeshRenderersCache.ContainsKey(renderer)) { continue; };
        this.skinnedMeshRenderersCache.Add(renderer, true);
        Log.WL(1, "skinned renderer:" + renderer.gameObject.name + " heraldry:" + true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, this.defaultMechCustomization);
        }
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersCustomHeraldry(GameObject src, MeshRenderer paintSchemePlaceholder) {
      Log.TW(0, "CustomMechRepresentationSimGame.RegisterRenderersCustomHeraldry: " + this.gameObject.name + " " + src.name + " paintSchemePlaceholder:" + (paintSchemePlaceholder == null ? "null" : paintSchemePlaceholder.gameObject.name));
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      Log.WL(1, "MeshRenderers:" + mRenderer.Length + " SkinnedMeshRenderer:" + sRenderer.Length);
      Dictionary<Renderer, MeshRenderer> customCamoHolders = new Dictionary<Renderer, MeshRenderer>();
      Transform[] trs = src.GetComponentsInChildren<Transform>();
      foreach (Transform tr in trs) {
        if (tr.name.StartsWith("camoholder") == false) { continue; }
        MeshRenderer camoholder = tr.gameObject.GetComponent<MeshRenderer>();
        if (camoholder == null) { continue; }
        SkinnedMeshRenderer parentSkinnedRenderer = tr.parent.gameObject.GetComponent<SkinnedMeshRenderer>();
        MeshRenderer parentMeshRenderer = tr.parent.gameObject.GetComponent<MeshRenderer>();
        Renderer parentRenderer = parentSkinnedRenderer;
        if (parentRenderer == null) { parentRenderer = parentMeshRenderer; }
        if (parentRenderer == null) { continue; }
        if (customCamoHolders.ContainsKey(parentRenderer)) { continue; }
        customCamoHolders.Add(parentRenderer, camoholder);
      }
      foreach (MeshRenderer renderer in mRenderer) {
        if (renderer.gameObject.GetComponent<BTDecal>() != null) { continue; }
        if (renderer.name.StartsWith("camoholder")) { continue; }
        if (meshRenderersCache.ContainsKey(renderer)) { continue; }
        Log.WL(1, "renderer:" + renderer.gameObject.name + " heraldry:" + true);
        this.meshRenderersCache.Add(renderer, true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, paintSchemePlaceholder);
        }
      }
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        if (skinnedMeshRenderersCache.ContainsKey(renderer)) { continue; };
        if (renderer.name.StartsWith("camoholder")) { continue; }
        this.skinnedMeshRenderersCache.Add(renderer, true);
        Log.WL(1, "skinned renderer:" + renderer.gameObject.name + " heraldry:" + true);
        CustomPaintPattern paintPattern = renderer.gameObject.GetComponent<CustomPaintPattern>();
        if (paintPattern == null) { paintPattern = renderer.gameObject.AddComponent<CustomPaintPattern>(); }
        if (customCamoHolders.TryGetValue(renderer, out MeshRenderer localPaintHolder)) {
          paintPattern.Init(renderer, localPaintHolder);
        } else {
          paintPattern.Init(renderer, paintSchemePlaceholder);
        }
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Add(collider);
      }
    }
    public virtual void RegisterRenderersComponentRepresentation(ComponentRepresentation src) {
      CustomHardpointDef hardpointDef = null;
      if (src.mechComponent != null) {
        hardpointDef = CustomHardPointsHelper.Find(src.mechComponent.mechComponentRef.prefabName);
      }
      if (hardpointDef == null) {
        RegisterRenderersMainHeraldry(src.gameObject);
      } else {
        Transform[] trs = src.gameObject.GetComponentsInChildren<Transform>(true);
        MeshRenderer paintSchemeHolder = null;
        foreach (Transform tr in trs) {
          if (tr.name.Contains(hardpointDef.paintSchemePlaceholder)) {
            paintSchemeHolder = tr.gameObject.GetComponent<MeshRenderer>();
            if (paintSchemeHolder != null) { break; }
          }
        }
        this.RegisterRenderersCustomHeraldry(src.gameObject, paintSchemeHolder);
      }
    }
    public virtual void UnregisterRenderers(GameObject src) {
      MeshRenderer[] mRenderer = src.GetComponentsInChildren<MeshRenderer>(true);
      foreach (MeshRenderer renderer in mRenderer) {
        meshRenderersCache.Remove(renderer);
      }
      SkinnedMeshRenderer[] sRenderer = src.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer renderer in sRenderer) {
        skinnedMeshRenderersCache.Remove(renderer);
      }
      Collider[] colliders = src.GetComponentsInChildren<Collider>(true);
      foreach (Collider collider in colliders) {
        this.selfColliders.Remove(collider);
      }
    }
    public virtual void ClearRenderers() {
      meshRenderersCache.Clear();
      skinnedMeshRenderersCache.Clear();
    }
    public virtual string GetComponentMountingPointPrefabName(HardpointDataDef hardpointData, string prefabName, ChassisLocations MountedLocation) {
      string[] strArray = prefabName.Split('_');
      string str = strArray[strArray.Length - 1];
      string lower = MountedLocation.ToString().ToLower();
      for (int index1 = 0; index1 < hardpointData.HardpointData.Length; ++index1) {
        if (hardpointData.HardpointData[index1].location == lower) {
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].mountingPoints.Length; ++index2) {
            string mountingPoint = hardpointData.HardpointData[index1].mountingPoints[index2];
            if (mountingPoint.EndsWith(str))
              return mountingPoint;
          }
        }
      }
      return "";
    }
    public virtual ComponentRepresentation AddComponentRepresentationSimGame(GameObject componentGO, CustomHardpointDef customHardpoint) {
      ComponentRepresentation result = null;
      if(customHardpoint == null) { result = componentGO.AddComponent<ComponentRepresentation>(); return result; }
      WeaponRepresentation weaponRep = componentGO.AddComponent<WeaponRepresentation>();
      Log.LogWrite(1, "reiniting vfxTransforms\n");
      List<Transform> transfroms = new List<Transform>();
      for (int index = 0; index < customHardpoint.emitters.Count; ++index) {
        Transform[] trs = componentGO.GetComponentsInChildren<Transform>();
        foreach (Transform tr in trs) { if (tr.name == customHardpoint.emitters[index]) { transfroms.Add(tr); break; } }
      }
      Log.LogWrite(1, "result(" + transfroms.Count + "):\n");
      for (int index = 0; index < transfroms.Count; ++index) {
        Log.LogWrite(2, transfroms[index].name + ":" + transfroms[index].localPosition + "\n");
      }
      if (transfroms.Count == 0) { transfroms.Add(componentGO.transform); };
      weaponRep.vfxTransforms = transfroms.ToArray();
      if (string.IsNullOrEmpty(customHardpoint.shaderSrc) == false) {
        Log.LogWrite(1, "updating shader:" + customHardpoint.shaderSrc + "\n");
        GameObject shaderPrefab = this.DataManager.PooledInstantiate(customHardpoint.shaderSrc, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        if (shaderPrefab != null) {
          Log.LogWrite(1, "shader prefab found\n");
          Renderer shaderComponent = shaderPrefab.GetComponentInChildren<Renderer>();
          if (shaderComponent != null) {
            Log.LogWrite(1, "shader renderer found:" + shaderComponent.name + " material: " + shaderComponent.material.name + " shader:" + shaderComponent.material.shader.name + "\n");
            Renderer[] renderers = componentGO.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
              for (int mindex = 0; mindex < renderer.materials.Length; ++mindex) {
                if (customHardpoint.keepShaderIn.Contains(renderer.gameObject.transform.name)) {
                  Log.LogWrite(2, "keep original shader: " + renderer.gameObject.transform.name + "\n");
                  continue;
                }
                Log.LogWrite(2, "seting shader :" + renderer.name + " material: " + renderer.materials[mindex] + " -> " + shaderComponent.material.shader.name + "\n");
                renderer.materials[mindex].shader = shaderComponent.material.shader;
                renderer.materials[mindex].shaderKeywords = shaderComponent.material.shaderKeywords;
              }
            }
            this.DataManager.PoolGameObject(customHardpoint.shaderSrc, shaderPrefab);
          }
        }
      }
      return weaponRep;
    }
    public virtual void InitWeapons(List<ComponentRepresentationSimGameInfo> compInfo, string parentDisplayName) {
      Log.TWL(0, "CustomMechRepresentationSimGame.InitWeapons " + (this._mechDef == null?"null":this._mechDef.ChassisID) + " HardpointData:" + (this.HardpointData==null?"null":this.HardpointData.ID));
      List<HardpointCalculator.Element> calcData = new List<HardpointCalculator.Element>();
      foreach (ComponentRepresentationSimGameInfo info in compInfo) { calcData.Add(new HardpointCalculator.Element() { location = info.prefabLocation, componentRef = info.componentRef }); }
      HardpointCalculator calculator = new HardpointCalculator();
      calculator.Init(calcData, this.HardpointData);
      List<string> usedPrefabNames = calculator.usedPrefabs.ToList();
      foreach (var cmp in compInfo) {
        Log.WL(1, "GetComponentPrefabName "+cmp.componentRef.ComponentDefID+ " PrefabBase:"+ this.PrefabBase+" location:"+ cmp.prefabLocation.ToString().ToLower());
        //cmp.componentRef.prefabName = MechHardpointRules.GetComponentPrefabName(this.HardpointData, cmp.componentRef, this.PrefabBase, cmp.prefabLocation.ToString().ToLower(), ref usedPrefabNames);
        cmp.componentRef.prefabName = calculator.GetComponentPrefabName(cmp.componentRef);
        cmp.componentRef.hasPrefabName = true;
        if (string.IsNullOrEmpty(cmp.componentRef.prefabName)) { continue; }
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(cmp.componentRef.prefabName);
        string prefabName = cmp.componentRef.prefabName;
        GameObject componentGO = null;
        if (customHardpoint != null) {
          componentGO = this.DataManager.PooledInstantiate(customHardpoint.prefab, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          if (componentGO == null) {
            componentGO = this.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
          } else {
            prefabName = customHardpoint.prefab;
          }
        } else {
          Log.LogWrite(1, prefabName + " have no custom hardpoint\n", true);
          componentGO = this.DataManager.PooledInstantiate(prefabName, BattleTechResourceType.Prefab, new Vector3?(), new Quaternion?(), (Transform)null);
        }
        Log.WL(1, cmp.componentRef.ComponentDefID + ":" + prefabName + " gameObject:"+(componentGO == null?"null": componentGO.name));
        if (componentGO == null) { continue; }
        MechSimGameComponentRepresentation simgameRep = componentGO.GetComponent<MechSimGameComponentRepresentation>();
        if (simgameRep == null) { simgameRep = componentGO.AddComponent<MechSimGameComponentRepresentation>(); }
        simgameRep.HardpointDefId = this.HardpointData.ID;
        simgameRep.OriginalPrefabId = cmp.componentRef.prefabName;
        Transform attachTransform = this.GetAttachTransform(cmp.attachLocation);
        cmp.componentRep = componentGO.GetComponent<ComponentRepresentation>();
        if (cmp.componentRep == null) { cmp.componentRep = this.AddComponentRepresentationSimGame(componentGO, customHardpoint); };
        if (cmp.componentRep != null) {
          cmp.componentRep.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
          cmp.componentRep.gameObject.SetActive(true);
          cmp.componentRep.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
          cmp.componentRep.gameObject.name = cmp.componentRef.prefabName;
          this.componentReps_Add(cmp.attachLocation,cmp.componentRep);
          if (cmp.componentRep != null) { this.RegisterRenderersComponentRepresentation(cmp.componentRep); }
          string originalPrefabName = calculator.GetComponentPrefabNameNoAlias(cmp.componentRef);
          if (string.IsNullOrEmpty(originalPrefabName) == false) {
            string mountingPointPrefabName = this.GetComponentMountingPointPrefabName(this.HardpointData, originalPrefabName, cmp.attachLocation);
            Log.WL(1, cmp.componentRef.ComponentDefID + " mount point:" + mountingPointPrefabName);
            if (!string.IsNullOrEmpty(mountingPointPrefabName)) {
              GameObject mountingPointGO = this.DataManager.PooledInstantiate(mountingPointPrefabName, BattleTechResourceType.Prefab);
              if (mountingPointGO != null) {
                ComponentRepresentation mountingPoint = mountingPointGO.GetComponent<ComponentRepresentation>();
                mountingPoint.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
                mountingPoint.gameObject.SetActive(true);
                mountingPoint.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
                mountingPoint.gameObject.name = mountingPointPrefabName;
                this.componentReps_Add(cmp.attachLocation, mountingPoint);
              }
            }
          }
        }
      }
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.CenterTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.LeftTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.RightTorso, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.LeftArm, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.RightArm, parentDisplayName);
      this.CreateBlankPrefabs(usedPrefabNames, this.HardpointData, ChassisLocations.Head, parentDisplayName);
      if (this.customRep != null) { this.customRep.AttachWeaponsSimGame(); }
      this.propertyBlock.UpdateCache();
    }
    public virtual void CreateBlankPrefabs(List<string> usedPrefabNames, HardpointDataDef hardpointData, ChassisLocations location, string parentDisplayName) {
      Log.TWL(0, "CustomMechRepresentation.CreateBlankPrefabs " + hardpointData.ID + " location:" + location);
      List<string> componentBlankNames = this.GetComponentBlankNames(usedPrefabNames, hardpointData, location);
      Transform attachTransform = this.GetAttachTransform(location);
      for (int index = 0; index < componentBlankNames.Count; ++index) {
        string blankName = componentBlankNames[index];
        CustomHardpointDef customHardpoint = CustomHardPointsHelper.Find(blankName);
        Log.WL(1, "blankName:" + blankName + (customHardpoint == null ? "" : ("->" + customHardpoint.prefab)));
        if (customHardpoint != null) { blankName = customHardpoint.prefab; }
        GameObject blankGO = this.DataManager.PooledInstantiate(blankName, BattleTechResourceType.Prefab);
        if (blankGO != null) {
          this.RegisterRenderersMainHeraldry(blankGO);
          ComponentRepresentation component = blankGO.GetComponent<ComponentRepresentation>();
          if (component == null) { component = blankGO.AddComponent<ComponentRepresentation>(); }
          component.Init((ICombatant)null, attachTransform, true, false, "MechRepSimGame");
          component.gameObject.SetActive(true);
          component.OnPlayerVisibilityChanged(VisibilityLevel.LOSFull);
          component.gameObject.name = componentBlankNames[index];
          this.RegisterRenderersComponentRepresentation(component);
          this.componentReps_Add(location, component);
        }
      }
    }
    public List<string> GetComponentBlankNames(List<string> usedPrefabNames, HardpointDataDef hardpointData, ChassisLocations location) {
      List<string> stringList1 = new List<string>();
      string lower = location.ToString().ToLower();
      List<string> stringList2 = new List<string>();
      for (int index1 = 0; index1 < hardpointData.HardpointData.Length; ++index1) {
        if (hardpointData.HardpointData[index1].location == lower || location == ChassisLocations.All) {
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].weapons.Length; ++index2) {
            List<string> stringList3 = new List<string>();
            bool flag = false;
            for (int index3 = 0; index3 < hardpointData.HardpointData[index1].weapons[index2].Length; ++index3) {
              string str = hardpointData.HardpointData[index1].weapons[index2][index3];
              stringList3.Add(str);
              if (usedPrefabNames.Contains(str))
                flag = true;
            }
            if (flag)
              stringList2.AddRange((IEnumerable<string>)stringList3);
          }
          for (int index2 = 0; index2 < hardpointData.HardpointData[index1].blanks.Length; ++index2) {
            string blank = hardpointData.HardpointData[index1].blanks[index2];
            stringList1.Add(blank);
          }
        }
      }
      if (location == ChassisLocations.All || stringList1.Count < 1)
        return stringList1;
      foreach (string str in stringList2) {
        char[] chArray = new char[1] { '_' };
        string[] strArray = str.Split(chArray);
        string prefabLocation = strArray[2];
        string prefabHardpoint = strArray[strArray.Length - 1];
        stringList1.RemoveAll((Predicate<string>)(x => x.Contains(prefabLocation) && x.EndsWith(prefabHardpoint)));
      }
      return stringList1;
    }
    public virtual void collapseAll(bool isDestroyed) {
      foreach (ChassisLocations loc in Enum.GetValues(typeof(ChassisLocations))) {
        switch (loc) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default:
          this.CollapseLocation((int)loc, isDestroyed);
          continue;
        }
      }
    }
    public virtual void loadDamageState() {
      foreach (ChassisLocations loc in Enum.GetValues(typeof(ChassisLocations))) {
        switch (loc) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default:
          this.collapseLocation((int)loc, (double)this._mechDef.GetLocationLoadoutDef(loc).CurrentInternalStructure <= 0.0);
          continue;
        }
      }
    }
    public virtual void collapseLocation(int location, bool isDestroyed) {
      MechDestructibleObject destructibleObject = this.GetDestructibleObject(location);
      if ((UnityEngine.Object)destructibleObject != (UnityEngine.Object)null)
        destructibleObject.CollapseSwap(isDestroyed);
      ChassisLocations chassisLocations = (ChassisLocations)location;
      switch (chassisLocations) {
        case ChassisLocations.LeftArm:
        case ChassisLocations.RightArm:
        if (this.componentReps.TryGetValue(chassisLocations, out HashSet<ComponentRepresentation> reps)) {
          foreach (ComponentRepresentation component in reps) {
            component.OnPlayerVisibilityChanged(isDestroyed ? VisibilityLevel.None : VisibilityLevel.LOSFull);
          }
        }
        break;
      }
    }
    public virtual MechDestructibleObject GetDestructibleObject(int location) {
      MechDestructibleObject destructibleObject = (MechDestructibleObject)null;
      switch ((ChassisLocations)location) {
        case ChassisLocations.Head:
        destructibleObject = this.headDestructible;
        break;
        case ChassisLocations.LeftArm:
        destructibleObject = this.leftArmDestructible;
        break;
        case ChassisLocations.LeftTorso:
        destructibleObject = this.leftTorsoDestructible;
        break;
        case ChassisLocations.CenterTorso:
        destructibleObject = this.centerTorsoDestructible;
        break;
        case ChassisLocations.RightTorso:
        destructibleObject = this.rightTorsoDestructible;
        break;
        case ChassisLocations.RightArm:
        destructibleObject = this.rightArmDestructible;
        break;
        case ChassisLocations.LeftLeg:
        destructibleObject = this.leftLegDestructible;
        break;
        case ChassisLocations.RightLeg:
        destructibleObject = this.rightLegDestructible;
        break;
        default:
        Debug.LogError((object)("Location out of bounds for GetDestructibleObject " + (object)location), (UnityEngine.Object)this);
        break;
      }
      return destructibleObject;
    }
    public void InitCustomParticles(GameObject source, CustomActorRepresentationDef custRepDef) {
      try {
        Log.TWL(0, "CustomMechRepresentation.InitCustomParticles " + source.transform.name);
        Dictionary<string, CustomParticleSystemDef> definitions = new Dictionary<string, CustomParticleSystemDef>();
        foreach (CustomParticleSystemDef customParticleSystemDef in custRepDef.Particles) {
          definitions.Add(customParticleSystemDef.object_name, customParticleSystemDef);
        }
        ParticleSystem[] particles = source.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in particles) {
          Log.W(1, ps.transform.name);
          if (definitions.TryGetValue(ps.transform.name, out CustomParticleSystemDef def)) {
            Log.WL(1, ":" + def.Location);
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, def.Location));
          } else {
            CustomParticleSystemReps.Add(new CustomParticleSystemRep(ps, ChassisLocations.None));
            Log.WL(1, ":" + ChassisLocations.None);
          }
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public void StopCustomParticlesInLocation(ChassisLocations location) {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        if (psRep.location == location) { psRep.ps.Stop(true); }
      }
    }
    public void StopCustomParticles() {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        psRep.ps.Stop(true);
      }
    }
    public void StartCustomParticles() {
      foreach (CustomParticleSystemRep psRep in CustomParticleSystemReps) {
        psRep.ps.Play(true);
      }
    }

  }
}