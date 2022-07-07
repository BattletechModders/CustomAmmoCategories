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
using System;
using System.Collections.Generic;
using BattleTech;
using CustAmmoCategories;
using Newtonsoft.Json;
using Unity;
using UnityEngine;

namespace CustomUnits {
  public class QuadVisualInfo {
    public bool UseQuadVisuals { get; set; }
    public string FLegsPrefab { get; set; }
    public string FLegsPrefabBase { get; set; }
    public string RLegsPrefab { get; set; }
    public string RLegsPrefabBase { get; set; }
    public string BodyPrefab { get; set; }
    public float BodyLength { get; set; }
    public string BodyShaderSource { get; set; }
    public List<string> SuppressRenderers { get; private set; }
    public List<string> NotSuppressRenderers { get; private set; }
    public List<string> JumpJets { get; private set; }
    public List<string> HeadLights { get; private set; }
    public List<AttachInfoRecord> WeaponsAttachPoints { get; private set; }
    public List<string> Animators { get; private set; }
    public List<string> TwistAnimators { get; private set; }
    public Dictionary<ChassisLocations, CustomDestructableDef> Destructables { get; private set; }
    public QuadVisualInfo() {
      FLegsPrefab = string.Empty;
      RLegsPrefab = string.Empty;
      UseQuadVisuals = false;
      SuppressRenderers = new List<string>();
      NotSuppressRenderers = new List<string>();
      JumpJets = new List<string>();
      HeadLights = new List<string>();
      Animators = new List<string>();
      TwistAnimators = new List<string>();
      BodyLength = 0f;
      BodyPrefab = string.Empty;
      FLegsPrefabBase = string.Empty;
      RLegsPrefabBase = string.Empty;
      BodyShaderSource = "chrPrfWeap_atlas_centertorso_laser_eh1";
      WeaponsAttachPoints = new List<AttachInfoRecord>();
      Destructables = new Dictionary<ChassisLocations, CustomDestructableDef>();
    }
  }
  public class CustomDestructableDef {
    public string Name { get; set; }
    public string wholeObj { get; set; }
    public string destroyedObj { get; set; }
  }
  public class CustomDestructionDef {
    public bool SuppressCombinedMesh { get; set; }
    public bool SuppressWeaponRepresentations { get; set; }
    public bool CollapseAllDestructables { get; set; }
    public CustomDestructionDef() {
      SuppressCombinedMesh = false;
      SuppressWeaponRepresentations = false;
      CollapseAllDestructables = false;
    }
  }
  public enum HardpointAttachType { Turret, Body, None };
  public class AttachInfoRecord {
    public string visuals { get; set; }
    public string animator { get; set; }
    public Dictionary<string, float> Animators { get; set; }
    public string Name { get; set; }
    public string attach { get; set; }
    public string location { get; set; }
    public HardpointAttachType type { get; set; }
    public bool hideIfEmpty { get; set; }
    public bool noRecoil { get; set; }
    public AttachInfoRecord() { visuals = string.Empty; animator = string.Empty; Animators = new Dictionary<string, float>(); attach = string.Empty; hideIfEmpty = false; noRecoil = false; }
  }
  public class CustomParticleSystemDef {
    public string object_name { get; set; }
    [JsonIgnore]
    public ChassisLocations Location { get; private set; }
    public string location {
      set {
        if (Enum.TryParse<ChassisLocations>(value, out ChassisLocations loc)) { Location = loc; } else
          if (Enum.TryParse<VehicleChassisLocations>(value, out VehicleChassisLocations vloc)) { Location = vloc.toFakeChassis(); } else {
          throw new Exception("invalid location value " + value);
        }
      }
    }
  }
  //public class LegGrounderDef {
  //  public enum LegGrounderType { Scorpion };
  //  public LegGrounderType type { get; set; } = LegGrounderType.Scorpion;
  //  public string parent { get; set; } = string.Empty;
  //  public string groundTransform { get; set; } = string.Empty;
  //  public void Apply(GameObject obj) {
  //    if (string.IsNullOrEmpty(this.parent)) { return; }
  //    Transform parent = obj.FindComponent<Transform>(this.parent);
  //    if (parent == null) { return; }
  //    ScorpionLegGrounder grounder = parent.gameObject.GetComponent<ScorpionLegGrounder>();
  //    if (grounder == null) { grounder = parent.gameObject.AddComponent<ScorpionLegGrounder>(); }
  //    if (string.IsNullOrEmpty(this.groundTransform)) { return; }
  //    grounder.groundTransform = obj.FindComponent<Transform>(this.groundTransform);
  //  }
  //}
  //public class LegSolverDef {
  //  public enum LegSolverType { Scorpion };
  //  public LegSolverType type { get; set; } = LegSolverType.Scorpion;
  //  public string parent { get; set; } = string.Empty;
  //  public string thigh { get; set; } = string.Empty;
  //  public string calf { get; set; } = string.Empty;
  //  public string foot { get; set; } = string.Empty;
  //  public string target { get; set; } = string.Empty;
  //  public void Apply(GameObject obj) {
  //    if (string.IsNullOrEmpty(this.parent)) { return; }
  //    Transform parent = obj.FindComponent<Transform>(this.parent);
  //    if (parent == null) { return; }
  //    ScorpionLegSolver solver = parent.gameObject.GetComponent<ScorpionLegSolver>();
  //    if (solver == null) { solver = parent.gameObject.AddComponent<ScorpionLegSolver>(); }
  //    if(string.IsNullOrEmpty(this.thigh) == false) {
  //      solver.Thigh = obj.gameObject.FindComponent<Transform>(this.thigh);
  //    }
  //    if (string.IsNullOrEmpty(this.calf) == false) {
  //      solver.Calf = obj.gameObject.FindComponent<Transform>(this.calf);
  //    }
  //    if (string.IsNullOrEmpty(this.foot) == false) {
  //      solver.Foot = obj.gameObject.FindComponent<Transform>(this.foot);
  //    }
  //    if (string.IsNullOrEmpty(this.target) == false) {
  //      solver.Target = obj.gameObject.FindComponent<Transform>(this.target);
  //    }
  //  }
  //}
  public class CustomAnimationEvent {
    public float time { get; set; } = 0f;
    public string functionName { get; set; } = string.Empty;
    private float m_floatParameter { get; set; } = 0f;
    private int m_intParameter { get; set; } = 0;
    private string m_stringParameter { get; set; } = string.Empty;
    public bool is_floatParameter { get; set; } = false;
    public bool is_intParameter { get; set; } = false;
    public bool is_stringParameter { get; set; } = false;
    public float floatParameter { get { return m_floatParameter; } set { is_floatParameter = true; m_floatParameter = value; } }
    public int intParameter { get { return m_intParameter; } set { is_intParameter = true; m_intParameter = value; } }
    public string stringParameter { get { return m_stringParameter; } set { is_stringParameter = true; m_stringParameter = value; } }
    public void Apply(AnimationClip clip) {
      if (string.IsNullOrEmpty(functionName)) { return; }
      AnimationEvent evt = new AnimationEvent();
      evt.functionName = this.functionName;
      evt.time = this.time;
      if (is_floatParameter) { evt.floatParameter = m_floatParameter; }
      if (is_intParameter) { evt.intParameter = m_intParameter; }
      if (is_stringParameter) { evt.stringParameter = m_stringParameter; }
      Log.WL(2,"AddEvent:"+ clip.name+" function:"+evt.functionName);
      clip.AddEvent(evt);
    }
  }
  public class CustomActorRepresentationDef {
    public enum RepresentationType { None, Mech, Vehicle, Turret }
    public enum RepresentationApplyType { MoveBone, None }
    public virtual RepresentationType RepType { get { return RepresentationType.None; } }
    public string Id { get; set; }
    public Dictionary<string, List<CustomAnimationEvent>> animationEvents { get; set; } = new Dictionary<string, List<CustomAnimationEvent>>();
    public void ApplyAnimationEvent(AnimationClip clip) {
      if(animationEvents.TryGetValue(clip.name, out var events)) {
        foreach (var evt in events) { evt.Apply(clip); }
      }
    }
    //public List<LegGrounderDef> legGrounders { get; set; } = new List<LegGrounderDef>();
    //public void ApplyGrounders(GameObject obj) { foreach (var g in legGrounders) { g.Apply(obj); }  }
    //public List<LegSolverDef> legSolvers { get; set; } = new List<LegSolverDef>();
    //public void ApplySolvers(GameObject obj) { foreach (var g in legSolvers) { g.Apply(obj); } }
    public CustomVector Scale { get; set; }
    public string PrefabBase { get; set; }
    public string SourcePrefabIdentifier { get; set; }
    public string SourcePrefabBase { get; set; }
    public string ShaderSource { get; set; }
    public string BlipSource { get; set; }
    public string BlipMeshSource { get; set; }
    public bool SupressAllMeshes { get; set; }
    public bool MoveSkeletalBones { get; set; } = false;
    public bool MoveAnimations { get; set; } = false;
    public RepresentationApplyType ApplyType { get; set; }
    public List<string> TwistAnimators { get; set; }
    public bool KeepRandomIdleAnimation { get; set; } = false;
    public List<string> HeadLights { get; set; }
    public List<string> JetStreamsAttaches { get; set; } = new List<string>();
    public CustomVector vfxScale { get; set; } = new CustomVector(true);
    public List<string> Animators { get; set; }
    public List<CustomParticleSystemDef> Particles { get; set; }
    public List<AttachInfoRecord> WeaponsAttachPoints { get; set; }
    public CustomDestructionDef OnDestroy { get; set; }
    public List<string> CustomMouseReceiver { get; set; }
    public string persistentAudioStart { get; set; }
    public string persistentAudioStop { get; set; }
    public QuadVisualInfo quadVisualInfo { get; set; }
    [JsonIgnore]
    private Dictionary<ChassisLocations, CustomDestructableDef> f_destructibles;
    [JsonIgnore]
    public Dictionary<ChassisLocations, CustomDestructableDef> destructibles {
      get {
        return f_destructibles;
      }
      set {
        f_destructibles = value;
      }
    }
    public Dictionary<string, CustomDestructableDef> Destructibles {
      set {
        if (f_destructibles == null) { f_destructibles = new Dictionary<ChassisLocations, CustomDestructableDef>(); }
        foreach (var val in value) {
          ChassisLocations res = ChassisLocations.None;
          if (Enum.TryParse<ChassisLocations>(val.Key, out res)) {
          } else if (Enum.TryParse<VehicleChassisLocations>(val.Key, out VehicleChassisLocations vres)) {
            res = vres.toFakeChassis();
          }
          if (res == ChassisLocations.None) { continue; }
          if (f_destructibles.ContainsKey(res) == false) {
            f_destructibles.Add(res, val.Value);
          } else {
            f_destructibles[res] = val.Value;
          }
        }
      }
    }
    public CustomActorRepresentationDef() {
      WeaponsAttachPoints = new List<AttachInfoRecord>();
      TwistAnimators = new List<string>();
      Animators = new List<string>();
      HeadLights = new List<string>();
      CustomMouseReceiver = new List<string>();
      OnDestroy = new CustomDestructionDef();
      persistentAudioStart = string.Empty;
      persistentAudioStop = string.Empty;
      f_destructibles = new Dictionary<ChassisLocations, CustomDestructableDef>();
      quadVisualInfo = new QuadVisualInfo();
      Particles = new List<CustomParticleSystemDef>();
      Scale = new CustomVector(true);
    }
  }
  public class CustomMechRepresentationDef : CustomActorRepresentationDef {
    public override RepresentationType RepType { get { return RepresentationType.Mech; } }
    public string LeftArmAttach { get; set; }
    public string RightArmAttach { get; set; }
    public string TorsoAttach { get; set; }
    public string LeftLegAttach { get; set; }
    public string RightLegAttach { get; set; }
    public Dictionary<string, string> Attaches {
      set {
        foreach (var attach in value) {
          if (attach.Key == "LeftArm") { LeftArmAttach = attach.Value; } else
          if (attach.Key == "Front") { LeftArmAttach = attach.Value; } else
          if (attach.Key == "RightArm") { RightArmAttach = attach.Value; } else
          if (attach.Key == "Rear") { RightArmAttach = attach.Value; } else
          if (attach.Key == "TorsoAttach") { TorsoAttach = attach.Value; } else
          if (attach.Key == "Turret") { TorsoAttach = attach.Value; } else
          if (attach.Key == "LeftLeg") { LeftLegAttach = attach.Value; } else
          if (attach.Key == "Left") { LeftLegAttach = attach.Value; } else
          if (attach.Key == "RightLeg") { RightLegAttach = attach.Value; } else
          if (attach.Key == "Right") { RightLegAttach = attach.Value; } else {
            throw new Exception(attach.Key + " is not valid attach");
          }
        }
      }
    }
    public Dictionary<string, string> vfxTransforms {
      set {
        foreach (var tr in value) {
          if (tr.Key == "CenterTorso") { vfxCenterTorsoTransform = tr.Value; } else
          if (tr.Key == "LeftTorso") { vfxLeftTorsoTransform = tr.Value; } else
          if (tr.Key == "RightTorso") { vfxRightTorsoTransform = tr.Value; } else
          if (tr.Key == "Head") { vfxHeadTransform = tr.Value; } else
          if (tr.Key == "Turret") {
            vfxHeadTransform = tr.Value; vfxCenterTorsoTransform = tr.Value;
            vfxLeftTorsoTransform = tr.Value; vfxRightTorsoTransform = tr.Value;
          } else
          if (tr.Key == "LeftArm") { vfxLeftArmTransform = tr.Value; } else
          if (tr.Key == "Front") { vfxLeftArmTransform = tr.Value; vfxLeftShoulderTransform = tr.Value; } else
          if (tr.Key == "RightArm") { vfxRightArmTransform = tr.Value; } else
          if (tr.Key == "Rear") { vfxRightArmTransform = tr.Value; vfxRightShoulderTransform = tr.Value; } else
          if (tr.Key == "LeftLeg") { vfxLeftLegTransform = tr.Value; } else
          if (tr.Key == "Left") { vfxLeftLegTransform = tr.Value; } else
          if (tr.Key == "RightLeg") { vfxRightLegTransform = tr.Value; } else
          if (tr.Key == "Right") { vfxRightLegTransform = tr.Value; } else
          if (tr.Key == "LeftShoulder") { vfxLeftShoulderTransform = tr.Value; } else
          if (tr.Key == "RightShoulder") { vfxRightShoulderTransform = tr.Value; } else {
            throw new Exception(tr.Key + " is not valid attach");
          }
        }
      }
    }
    public string vfxCenterTorsoTransform { get; set; }
    public string vfxLeftTorsoTransform { get; set; }
    public string vfxRightTorsoTransform { get; set; }
    public string vfxHeadTransform { get; set; }
    public string vfxLeftArmTransform { get; set; }
    public string vfxRightArmTransform { get; set; }
    public string vfxLeftLegTransform { get; set; }
    public string vfxRightLegTransform { get; set; }
    public string vfxLeftShoulderTransform { get; set; }
    public string vfxRightShoulderTransform { get; set; }
    public CustomMechRepresentationDef() {
      this.quadVisualInfo = new QuadVisualInfo();
    }
  }
}