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
  public class CustomActorRepresentationDef {
    public enum RepresentationType { None, Mech, Vehicle, Turret }
    public enum RepresentationApplyType { MoveBone, None }
    public virtual RepresentationType RepType { get { return RepresentationType.None; } }
    public string Id { get; set; }
    public CustomVector Scale { get; set; }
    public string PrefabBase { get; set; }
    public string SourcePrefabIdentifier { get; set; }
    public string SourcePrefabBase { get; set; }
    public string ShaderSource { get; set; }
    public string BlipSource { get; set; }
    public string BlipMeshSource { get; set; }
    public bool SupressAllMeshes { get; set; }
    public RepresentationApplyType ApplyType { get; set; }
    public List<string> TwistAnimators { get; set; }
    public List<string> HeadLights { get; set; }
    public List<string> JetStreamsAttaches { get; set; } = new List<string>();
    public CustomVector vfxScale { get; set; } = new CustomVector(true);
    public List<string> Animators { get; set; }
    public List<CustomParticleSystemDef> Particles { get; set; }
    public List<AttachInfoRecord> WeaponAttachPoints { get; set; }
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
      WeaponAttachPoints = new List<AttachInfoRecord>();
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