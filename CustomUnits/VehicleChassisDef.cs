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
using BattleTech.Rendering.UrbanWarfare;
using BattleTech.UI;
using CustAmmoCategories;
using Harmony;
using Localize;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEngine;
using CustomComponents;
using CustomAmmoCategoriesPatches;
using HBS.Collections;
using System.Collections.Concurrent;
using MessagePack;
using CustAmmoCategoriesPatches;
using IRBTModUtils;

namespace CustomUnits {
  [MessagePackObject]
  public class CustomTransform {
    [Key(0)]
    public CustomVector offset { get; set; }
    [Key(1)]
    public CustomVector scale { get; set; }
    [Key(2)]
    public CustomVector rotate { get; set; }
    public CustomTransform() {
      offset = new CustomVector(false);
      scale = new CustomVector(true);
      rotate = new CustomVector(false);
    }
    public void debugLog(int initiation) {
      string init = new String(' ', initiation);
      Log.LogWrite(init + "offset:" + offset + "\n");
      Log.LogWrite(init + "scale:" + scale + "\n");
      Log.LogWrite(init + "rotate:" + rotate + "\n");
    }
  }
  [MessagePackObject]
  public class CustomMaterialInfo {
    [Key(0)]
    public string shader { get; set; }
    [Key(1)]
    public List<string> shaderKeyWords { get; set; }
    [Key(2)]
    public Dictionary<string, string> materialTextures { get; set; }
    public CustomMaterialInfo() {
      shader = string.Empty;
      shaderKeyWords = new List<string>();
      materialTextures = new Dictionary<string, string>();
    }
    public void debugLog(int initiation) {
      string init = new String(' ', initiation);
      Log.LogWrite(init + "shader:" + shader + "\n");
      Log.LogWrite(init + "shaderKeyWords:"); foreach (string shaderKeyword in shaderKeyWords) { Log.LogWrite("'" + shaderKeyword + "' "); }; Log.LogWrite("\n");
      Log.LogWrite(init + "materialTextures:\n");
      foreach (var materialTexture in materialTextures) {
        Log.LogWrite(init + " " + materialTexture.Key + ":" + materialTexture.Value + "\n");
      };
    }
  }

  [MessagePackObject]
  public class RequiredComponent {
    [Key(0)]
    public string CategoryId { get; set; }
    [Key(1)]
    public string DefId { get; set; }
    [JsonIgnore, Key(2)]
    public HashSet<int> SearchLocations { get; set; }
    [IgnoreMember]
    public List<ChassisLocations> MechSearchLocations { set {
        foreach(ChassisLocations loc in value){ SearchLocations.Add((int)loc); }
      } }
    [IgnoreMember]
    public List<VehicleChassisLocations> VehicleSearchLocations {
      set {
        foreach (VehicleChassisLocations loc in value) { SearchLocations.Add((int)loc); }
      }
    }
    public RequiredComponent() { CategoryId = string.Empty; DefId = string.Empty; SearchLocations = new HashSet<int>(); }
    public bool Test(MechComponent component) {
      if(string.IsNullOrEmpty(DefId) == false) {
        if (component.defId != DefId) {
          Log.WL(5,"bad defId " + component.defId + " != " + DefId);
          return false;
        }
      }
      if(SearchLocations.Count != 0) {
        if (SearchLocations.Contains(component.Location) == false) {
          Log.WL(5,"bad location");
          return false;
        }
      }
      if(string.IsNullOrEmpty(CategoryId) == false) {
        Category category = component.componentDef.GetComponent<Category>();
        if (category == null) { return false; }
        if (category.CategoryID != CategoryId) { return false; }
      }
      return true;
    }
    public bool Test(MechComponentRef component) {
      if (string.IsNullOrEmpty(DefId) == false) {
        if (component.ComponentDefID != DefId) {
          Log.WL(5, "bad defId " + component.ComponentDefID + " != " + DefId);
          return false;
        }
      }
      if (SearchLocations.Count != 0) {
        if (SearchLocations.Contains((int)component.MountedLocation) == false) {
          Log.WL(5, "bad location");
          return false;
        }
      }
      if (string.IsNullOrEmpty(CategoryId) == false) {
        Category category = component.Def.GetComponent<Category>();
        if (category == null) { return false; }
        if (category.CategoryID != CategoryId) { return false; }
      }
      return true;
    }
    public bool Test(VehicleComponentRef component) {
      if (string.IsNullOrEmpty(DefId) == false) {
        if (component.ComponentDefID != DefId) {
          Log.WL(5, "bad defId " + component.ComponentDefID + " != " + DefId);
          return false;
        }
      }
      if (SearchLocations.Count != 0) {
        if (SearchLocations.Contains((int)component.MountedLocation) == false) {
          Log.WL(5, "bad location");
          return false;
        }
      }
      if (string.IsNullOrEmpty(CategoryId) == false) {
        Category category = component.Def.GetComponent<Category>();
        if (category == null) { return false; }
        if (category.CategoryID != CategoryId) { return false; }
      }
      return true;
    }
  }
  [MessagePackObject]
  public class CustomPart {
    [Key(0)]
    public string prefab { get; set; }
    [Key(1)]
    public string boneName { get; set; }
    [Key(2)]
    public Dictionary<string, CustomMaterialInfo> MaterialInfo { get; set; }
    [Key(3)]
    public CustomTransform prefabTransform { get; set; }
    [Key(4)]
    public VehicleChassisLocations VehicleChassisLocation { get; set; }
    [Key(5)]
    public ChassisLocations MechChassisLocation { get; set; }
    [Key(6)]
    public List<RequiredComponent> RequiredComponents { get; set; }
    [Key(7)]
    public List<string> RequiredUpgrades {
      get {
        return RequiredUpgradesSet.ToList();
      }
      set {
        RequiredUpgradesSet = value.ToHashSet();
      }
    }
    [JsonIgnore,IgnoreMember]
    public HashSet<string> RequiredUpgradesSet { get; private set; }
    [Key(8)]
    public string AnimationType { get; set; }
    [IgnoreMember]
    public JObject AnimationData {
      set {
        this.Data = value.ToString(Formatting.None);
      }
    }
    [JsonIgnore, Key(9)]
    public string Data { get; set; }
    public CustomPart() {
      prefab = string.Empty;
      boneName = string.Empty;
      VehicleChassisLocation = VehicleChassisLocations.Front;
      MechChassisLocation = ChassisLocations.CenterTorso;
      prefabTransform = new CustomTransform();
      MaterialInfo = new Dictionary<string, CustomMaterialInfo>();
      RequiredUpgradesSet = new HashSet<string>();
      RequiredComponents = new List<RequiredComponent>();
    }
    public void debugLog(int initiation) {
      string init = new String(' ', initiation);
      Log.LogWrite(init + "prefab:" + prefab + "\n");
      Log.LogWrite(init + "MaterialInfo:\n");
      foreach (var mi in MaterialInfo) {
        Log.LogWrite(init + " " + mi.Key + ":\n");
        mi.Value.debugLog(initiation + 2);
      }
      Log.LogWrite(init + "VehicleChassisLocation:" + VehicleChassisLocation + "\n");
      Log.LogWrite(init + "MechChassisLocation:" + MechChassisLocation + "\n");
      Log.LogWrite(init + "prefabTransform:\n");
      prefabTransform.debugLog(initiation + 1);
      Log.LogWrite(init + "AnimationType:" + AnimationType + "\n");
      Log.LogWrite(init + "AnimationData:" + Data + "\n");
    }
    public CustomMaterialInfo findMaterialInfo(string materialName) {
      foreach (var mi in this.MaterialInfo) {
        if (materialName.StartsWith(mi.Key)) {
          return mi.Value;
        }
      }
      return null;
    }
  }
  [MessagePackObject]
  public class UnitUnaffection {
    private static readonly float MinMoveClamp = 0.2f;
    private static readonly float MaxMoveClamp = 0.5f;
    [JsonIgnore,IgnoreMember]
    private float FMoveClamp;
    [Key(0)]
    public bool DesignMasks { get; set; }
    [Key(1)]
    public bool Pathing { get; set; }
    [Key(2)]
    public bool MoveCostBiome { get; set; }
    [Key(3)]
    public bool Fire { get; set; }
    [Key(4)]
    public bool Landmines { get; set; }
    [Key(5)]
    public float MinJumpDistance { get; set; }
    [Key(6)]
    public bool AllowPartialMovement { get; set; }
    [Key(7)]
    public bool AllowPartialSprint { get; set; }
    [Key(8)]
    public bool AllowRotateWhileJump { get; set; }
    [Key(9)]
    public float MoveClamp {
      get {
        if (Pathing && (FMoveClamp < MinMoveClamp)) { return MinMoveClamp; }
        if (Pathing && (FMoveClamp > MaxMoveClamp)) { return MaxMoveClamp; }
        return FMoveClamp;
      }
      set {
        FMoveClamp = value;
      }
    }
    public UnitUnaffection() {
      DesignMasks = false;
      Pathing = false;
      Fire = false;
      Landmines = false;
      MoveCostBiome = false;
      FMoveClamp = 0f;
      AllowPartialMovement = true;
      AllowPartialSprint = Core.Settings.PartialMovementOnlyWalkByDefault == false;
      AllowRotateWhileJump = Core.Settings.AllowRotateWhileJumpByDefault;
    }
    public void debugLog(int initiation) {
      Log.LogWrite(initiation, "DesignMasks:" + DesignMasks, true);
      Log.LogWrite(initiation, "Pathing:" + Pathing, true);
      Log.LogWrite(initiation, "Fire:" + Fire, true);
      Log.LogWrite(initiation, "Landmines:" + Landmines, true);
      Log.LogWrite(initiation, "MoveCostBiome:" + MoveCostBiome, true);
    }
  }
  [MessagePackObject]
  public class HangarLocationTransforms {
    [Key(0)]
    public CustomTransform TurretAttach { get; set; }
    [Key(1)]
    public CustomTransform BodyAttach { get; set; }
    [Key(2)]
    public CustomTransform TurretLOS { get; set; }
    [Key(3)]
    public CustomTransform LeftSideLOS { get; set; }
    [Key(4)]
    public CustomTransform RightSideLOS { get; set; }
    [Key(5)]
    public CustomTransform leftVFXTransform { get; set; }
    [Key(6)]
    public CustomTransform rightVFXTransform { get; set; }
    [Key(7)]
    public CustomTransform rearVFXTransform { get; set; }
    [Key(8)]
    public List<CustomTransform> lightsTransforms { get; set; }
    [Key(9)]
    public CustomTransform thisTransform { get; set; }
    public HangarLocationTransforms() {
      TurretAttach = new CustomTransform();
      BodyAttach = new CustomTransform();
      TurretLOS = new CustomTransform();
      LeftSideLOS = new CustomTransform();
      RightSideLOS = new CustomTransform();
      leftVFXTransform = new CustomTransform();
      rightVFXTransform = new CustomTransform();
      rearVFXTransform = new CustomTransform();
      thisTransform = new CustomTransform();
      lightsTransforms = new List<CustomTransform>();
    }
  }
  [MessagePackObject]
  public class MeleeWeaponOverrideDef {
    [Key(0)]
    public string DefaultWeapon { get; set; }
    [Key(1)]
    public Dictionary<string, string> Components { get; set; }
    public MeleeWeaponOverrideDef() {
      DefaultWeapon = "Weapon_MeleeAttack";
      Components = new Dictionary<string, string>();
    }
  }
  public static class ChassisAdditinalInfoHelper {
    private static Dictionary<string, ChassisAdditionalInfo> f_ChassisAdditionalInfos = new Dictionary<string, ChassisAdditionalInfo>();
    public static ChassisAdditionalInfo ChassisInfo(this VehicleChassisDef chassis) {
      if(f_ChassisAdditionalInfos.TryGetValue(chassis.Description.Id, out ChassisAdditionalInfo info)) {
        return info;
      } else {
        info = new ChassisAdditionalInfo(chassis.Description.Id, SpawnType.Undefined);
        info.SpawnAs = SpawnType.AsVehicle;
        f_ChassisAdditionalInfos.Add(chassis.Description.Id, info);
        return info;
      }
    }
    public static ChassisAdditionalInfo ChassisInfo(this ChassisDef chassis) {
      if (f_ChassisAdditionalInfos.TryGetValue(chassis.Description.Id, out ChassisAdditionalInfo info)) {
        return info;
      } else {
        info = new ChassisAdditionalInfo(chassis.Description.Id, SpawnType.Undefined);
        f_ChassisAdditionalInfos.Add(chassis.Description.Id, info);
        return info;
      }
    }
  }
  public enum SpawnType { Undefined, AsMech, AsVehicle };
  [MessagePackObject]
  public class ChassisAdditionalInfo {
    [Key(0)]
    public string Id { get; set; }
    [Key(1)]
    public SpawnType SpawnAs { get; set; }
    public ChassisAdditionalInfo(string id, SpawnType spawn) {
      SpawnAs = spawn;
      Id = id;
    }
  }
  [MessagePackObject]
  public class UnitCustomInfo {
    [Key(0)]
    public List<AlternateRepresentationDef> AlternateRepresentations { get; set; } = new List<AlternateRepresentationDef>();
    [Key(1)]
    public TrooperSquadDef SquadInfo { get; set; } = new TrooperSquadDef();
    [Key(2)]
    public bool NullifyBodyMesh { get; set; } = false;
    [Key(3)]
    public float FlyingHeight { get; set; } = 0f;
    [IgnoreMember]
    public float AOEHeight { get { return FlyingHeight; } set { FlyingHeight = value; } }
    [Key(4)]
    public bool TieToGroundOnDeath { get; set; } = false;
    [IgnoreMember, JsonIgnore]
    public List<ChassisLocations> lethalLocations { get; private set; } = new List<ChassisLocations>();
    [IgnoreMember, JsonIgnore]
    private List<string> f_lethalLocations = new List<string>();
    [Key(5)]
    public List<string> LethalLocations {
      get { return f_lethalLocations; }
      set {
        f_lethalLocations = value == null ? new List<string>() : value;
        lethalLocations = new List<ChassisLocations>();
        foreach (string loc in value) {
          if (Enum.TryParse<ChassisLocations>(loc, out ChassisLocations cloc)) { lethalLocations.Add(cloc); } else
            if (Enum.TryParse<VehicleChassisLocations>(loc, out VehicleChassisLocations vloc)) { lethalLocations.Add(vloc.toFakeChassis()); } else {
            throw new Exception(loc + " is not valid mech or vehicle location");
          }
        }
      }
    }
    [Key(6)]
    public float FiringArc { get; set; } = 0f;
    [Key(7)]
    public bool NoIdleAnimations { get; set; } = false;
    [Key(8)]
    public bool NoMoveAnimations { get; set; } = false;
    [Key(9)]
    public bool ArmsCountedAsLegs { get; set; } = false;
    [Key(10)]
    public float LegDestroyedMovePenalty { get; set; } = -1f;
    [Key(11)]
    public float LegDamageRedMovePenalty { get; set; } = -1f;
    [Key(12)]
    public float LegDamageYellowMovePenalty { get; set; } = -1f;
    [Key(13)]
    public float LegDamageRelativeInstability { get; set; } = -1f;
    [Key(14)]
    public float LegDestroyRelativeInstability { get; set; } = 1f;
    [Key(15)]
    public float LocDestroyedPermanentStabilityLossMod { get; set; } = 1f;
    [Key(16)]
    public CustomVector HighestLOSPosition { get; set; } = new CustomVector(false);
    [Key(17)]
    public UnitUnaffection Unaffected { get; set; } = new UnitUnaffection();
    [Key(18)]
    public CustomTransform TurretAttach { get; set; } = new CustomTransform();
    [Key(19)]
    public CustomTransform BodyAttach { get; set; } = new CustomTransform();
    [Key(20)]
    public CustomTransform TurretLOS { get; set; } = new CustomTransform();
    [Key(21)]
    public CustomTransform LeftSideLOS { get; set; } = new CustomTransform();
    [Key(22)]
    public CustomTransform RightSideLOS { get; set; } = new CustomTransform();
    [Key(23)]
    public CustomTransform leftVFXTransform { get; set; } = new CustomTransform();
    [Key(24)]
    public CustomTransform rightVFXTransform { get; set; } = new CustomTransform();
    [Key(25)]
    public CustomTransform rearVFXTransform { get; set; } = new CustomTransform();
    [Key(26)]
    public List<CustomTransform> lightsTransforms { get; set; } = new List<CustomTransform>();
    [Key(27)]
    public CustomTransform thisTransform { get; set; } = new CustomTransform();
    [Key(28)]
    public List<CustomPart> CustomParts { get; set; } = new List<CustomPart>();
    [Key(29)]
    public HangarLocationTransforms HangarTransforms { get; set; } = new HangarLocationTransforms();
    [Key(30)]
    public string MoveCost { get; set; } = string.Empty;
    [Key(31)]
    public string SourcePrefabIdentifier { get; set; } = string.Empty;
    [Key(32)]
    public string SourcePrefabBase { get; set; } = string.Empty;
    [Key(33)]
    public Dictionary<string, float> MoveCostModPerBiome { get; set; } = new Dictionary<string, float>();
    [Key(34)]
    public MeleeWeaponOverrideDef MeleeWeaponOverride { get; set; } = new MeleeWeaponOverrideDef();
    [Key(35)]
    public bool FakeVehicle { get; set; } = false;
    [Key(36)]
    public bool Naval { get; set; } = false;
    [Key(37)]
    public VehicleMovementType FakeVehicleMovementType { get; set; } = VehicleMovementType.Tracked;
    [Key(38)]
    public DesignMaskMoveCostInfo defaultMoveCost { get; set; } = new DesignMaskMoveCostInfo();
    [IgnoreMember, JsonIgnore]
    public ChassisLocations MechVehicleCrewLocation {
      get {
        return f_FakeVehicleCrewLocation;
      }
      set {
        f_FakeVehicleCrewLocation = value;
      }
    }
    [IgnoreMember,JsonIgnore]
    private ChassisLocations f_FakeVehicleCrewLocation = ChassisLocations.Head;
    [Key(39)]
    public string CrewLocation {
      get {
        return f_FakeVehicleCrewLocation.ToString();
      }
      set {
        if(Enum.TryParse<ChassisLocations>(value, out ChassisLocations mloc)) {
          f_FakeVehicleCrewLocation = mloc;
        } else if (Enum.TryParse<VehicleChassisLocations>(value, out VehicleChassisLocations vloc)) {
          f_FakeVehicleCrewLocation = vloc.toFakeChassis();
        } else {
          throw new Exception(value + " is not valid mech or vehicle location");
        }
      }
    }
    [Key(40)]
    public bool TurretArmorReadout { get; set; } = false;
    [Key(41)]
    public string UnitTypeName { get; set; } = string.Empty;
    [Key(42)]
    public bool BossAppearAnimation { get; set; } = false;
    public UnitCustomInfo() {
      //FlyingHeight = 0f;
      //Naval = false;
      //HighestLOSPosition = new CustomVector(false);
      //TurretAttach = new CustomTransform();
      //BodyAttach = new CustomTransform();
      //TurretLOS = new CustomTransform();
      //LeftSideLOS = new CustomTransform();
      //RightSideLOS = new CustomTransform();
      //leftVFXTransform = new CustomTransform();
      //rightVFXTransform = new CustomTransform();
      //rearVFXTransform = new CustomTransform();
      //thisTransform = new CustomTransform();
      //CustomParts = new List<CustomPart>();
      //Unaffected = new UnitUnaffection();
      //lightsTransforms = new List<CustomTransform>();
      //MoveCostModPerBiome = new Dictionary<string, float>();
      //MoveCost = string.Empty;
      //FiringArc = 0f;
      //TieToGroundOnDeath = false;
      //NoIdleAnimations = false;
      //NullifyBodyMesh = false;
      //HangarTransforms = new HangarLocationTransforms();
      //NoMoveAnimations = false;
      //ArmsCountedAsLegs = false;
      //LegDestroyedMovePenalty = -1f;
      //LegDamageRedMovePenalty = -1f;
      //LegDamageYellowMovePenalty = -1f;
      //LegDamageRelativeInstability = -1f;
      //LegDestroyRelativeInstability = 1f;
      //LocDestroyedPermanentStabilityLossMod = 1f;
      //SquadInfo = new TrooperSquadDef();
      //MeleeWeaponOverride = new MeleeWeaponOverrideDef();
      ////quadVisualInfo = new QuadVisualInfo();
      //AlternateRepresentations = new List<AlternateRepresentationDef>();
      //SourcePrefabIdentifier = string.Empty;
      //SourcePrefabBase = string.Empty;
      //lethalLocations = new List<ChassisLocations>();
      //FakeVehicle = false;
      //FakeVehicleMovementType = VehicleMovementType.Tracked;
      //defaultMoveCost = new DesignMaskMoveCostInfo();
    }
    public void debugLog(int initiation) {
      string init = new String(' ', initiation);
      Log.LogWrite(init + "AOEHeight: " + AOEHeight.ToString() + "\n");
      Log.LogWrite(init + "heightFix: " + HighestLOSPosition.ToString() + "\n");
      Log.LogWrite(init + "FiringArc: " + FiringArc.ToString() + "\n");
      Log.LogWrite(init + "MoveCost: " + MoveCost + "\n");
      Log.LogWrite(init + "MoveCostModPerBiome:\n");
      foreach (var mc in MoveCostModPerBiome) {
        Log.LogWrite(init + " " + mc.Key + ":" + mc.Value + "\n");
      }
      Log.LogWrite(init + "Unaffected:\n");
      Unaffected.debugLog(initiation + 1);
      Log.LogWrite(init + "TurretAttach:\n");
      TurretAttach.debugLog(initiation + 1);
      Log.LogWrite(init + "BodyAttach:\n");
      BodyAttach.debugLog(initiation + 1);
      Log.LogWrite(init + "TurretLOS:\n");
      TurretLOS.debugLog(initiation + 1);
      Log.LogWrite(init + "LeftSideLOS:\n");
      LeftSideLOS.debugLog(initiation + 1);
      Log.LogWrite(init + "RightSideLOS:\n");
      RightSideLOS.debugLog(initiation + 1);
      Log.LogWrite(init + "leftVFXTransform:\n");
      leftVFXTransform.debugLog(initiation + 1);
      Log.LogWrite(init + "rightVFXTransform:\n");
      rightVFXTransform.debugLog(initiation + 1);
      Log.LogWrite(init + "rearVFXTransform:\n");
      rearVFXTransform.debugLog(initiation + 1);
      Log.LogWrite(init + "thisTransform:\n");
      thisTransform.debugLog(initiation + 1);
      Log.LogWrite(init + "lightsTransforms:\n");
      for (int t = 0; t < lightsTransforms.Count; ++t) {
        Log.LogWrite(init + " [" + t + "]:\n");
        lightsTransforms[t].debugLog(initiation + 2);
      }
      Log.LogWrite(init + "CustomParts: " + CustomParts.Count + "\n");
      for (int t = 0; t < CustomParts.Count; ++t) {
        Log.LogWrite(init + " [" + t + "]:\n");
        CustomParts[t].debugLog(initiation + 2);
      }
    }
  }
  public static class VehicleCustomInfoHelper {
    public static ConcurrentDictionary<string, UnitCustomInfo> vehicleChasissInfosDb = new ConcurrentDictionary<string, UnitCustomInfo>();
    public static Dictionary<int, AbstractActor> unityInstanceIdActor = new Dictionary<int, AbstractActor>();
    public static Dictionary<string, string> CustomMechRepresentationsPrefabs = new Dictionary<string, string>();
    public static Dictionary<string, string> CustomVehicleRepresentationsPrefabs = new Dictionary<string, string>();
    public static Dictionary<string, string> CustomMechBayRepresentationsPrefabs = new Dictionary<string, string>();
    public static Dictionary<string, string> CustomVehicleBayRepresentationsPrefabs = new Dictionary<string, string>();
    private static Dictionary<AbstractActor, UnitCustomInfo> actorsCustomInfos = new Dictionary<AbstractActor, UnitCustomInfo>();
    public static void Clear() { actorsCustomInfos.Clear(); }
    public static UnitCustomInfo GetInfoByChassisId(string id) {
      if(vehicleChasissInfosDb.TryGetValue(id, out UnitCustomInfo result) == false) {
        result = new UnitCustomInfo();
        vehicleChasissInfosDb.AddOrUpdate(id, result, (k,v)=>{ return result; });
      }
      return result;
    }
    public static UnitCustomInfo GetCustomInfo(this VehicleChassisDef chassis) {
      //if (vehicleChasissInfosDb.TryGetValue(chassis.Description.Id, out UnitCustomInfo result)) {
        //return result;
      //}
      return GetInfoByChassisId(chassis.Description.Id);
    }
    public static UnitCustomInfo GetCustomInfo(this ChassisDef chassis) {
      //if (vehicleChasissInfosDb.TryGetValue(chassis.Description.Id, out UnitCustomInfo result)) {
        //return result;
      //}
      return GetInfoByChassisId(chassis.Description.Id);
    }
    public static UnitCustomInfo GetCustomInfo(this MechDef mechDef) {
      //if (vehicleChasissInfosDb.TryGetValue(mechDef.ChassisID, out UnitCustomInfo result)) {
      //  return result;
      //}
      return GetInfoByChassisId(mechDef.ChassisID);
    }
    public static UnitCustomInfo GetCustomInfo(this AbstractActor actor) {
      if (actor == null) { return new UnitCustomInfo(); }
      if (actorsCustomInfos.TryGetValue(actor, out var info) == false) {
        info = GetInfoByChassisId(actor.PilotableActorDef.ChassisID);
        actorsCustomInfos.Add(actor, info);
      }
      return info;
    }
  }
  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_VehicleChassisDef_fromJSON_Patch {
    private static Dictionary<string, TagSet> VehicleChassisDef_ChassisTags = new Dictionary<string, TagSet>();
    public static TagSet ChassisTags(this VehicleChassisDef chassis) {
      if (VehicleChassisDef_ChassisTags.TryGetValue(chassis.Description.Id, out TagSet tags) == false) {
        tags = new TagSet(); VehicleChassisDef_ChassisTags.Add(chassis.Description.Id, tags);
      }
      return tags;
    }
    public static CustomPrewarm.Serialize.TagSet serializeChassisTags(string id) {
      if (VehicleChassisDef_ChassisTags.TryGetValue(id, out TagSet tags) == false) {
        tags = new TagSet(); VehicleChassisDef_ChassisTags.Add(id, tags);
      }
      return new CustomPrewarm.Serialize.TagSet(tags);
    }
    public static bool Prefix(VehicleChassisDef __instance, ref string json) {
      //Log.TW(0,"VehicleChassisDef.FromJSON"); //vehiclechassisdef_WARRIOR_VTOL
      UnitCustomInfo info = null;
      try {
        if (__instance.Description != null) {
          CustomPrewarm.Serialize.TagSet ChassisTags = CustomPrewarm.Core.getDeserializedObject(BattleTechResourceType.VehicleChassisDef, __instance.Description.Id, "CustomUnitsChassisTags") as CustomPrewarm.Serialize.TagSet;
          UnitCustomInfo dinfo = CustomPrewarm.Core.getDeserializedObject(BattleTechResourceType.VehicleChassisDef, __instance.Description.Id, "CustomUnits") as UnitCustomInfo;
          if ((dinfo != null)&&(ChassisTags != null)) {
            VehicleCustomInfoHelper.vehicleChasissInfosDb.AddOrUpdate(__instance.Description.Id, dinfo, (k, v) => { return dinfo; });
            if (VehicleChassisDef_ChassisTags.ContainsKey(__instance.Description.Id) == false) {
              VehicleChassisDef_ChassisTags.Add(__instance.Description.Id, ChassisTags.toBT());
            } else {
              VehicleChassisDef_ChassisTags[__instance.Description.Id] = ChassisTags.toBT();
            }
            return true;
          }
          Log.TWL(0, "ChassisDef:" + __instance.Description.Id + " has no deserialized UnitCustomInfo or ChassisTags. Should not happend");
        }
        JObject definition = JObject.Parse(json);
        string id = (string)definition["Description"]["Id"];
        Log.WL(1,id);
        if (definition["CustomParts"] != null) {
          info = definition["CustomParts"].ToObject<UnitCustomInfo>();
          definition.Remove("CustomParts");
        } else {
          info = new UnitCustomInfo();
        }
        if (definition["ChassisTags"] != null) {
          string ChassisTags = definition["ChassisTags"].ToString();
          if(VehicleChassisDef_ChassisTags.TryGetValue(id, out TagSet tags) == false) {
            tags = new TagSet(); VehicleChassisDef_ChassisTags.Add(id, tags);
          }
          tags.FromJSON(ChassisTags);
          definition.Remove("ChassisTags");
        }
        if (definition["MeleeDamage"] != null) {
          definition.Remove("MeleeDamage");
        }
        if (definition["MeleeToHitModifier"] != null) {
          definition.Remove("MeleeToHitModifier");
        }
        if (definition["DFADamage"] != null) {
          definition.Remove("DFADamage");
        }
        if (definition["DFAToHitModifier"] != null) {
          definition.Remove("DFAToHitModifier");
        }
        if (definition["DFASelfDamage"] != null) {
          definition.Remove("DFASelfDamage");
        }
        VehicleCustomInfoHelper.vehicleChasissInfosDb.AddOrUpdate(id, info, (k,v)=> { return info; } );
        //if (VehicleCustomInfoHelper.vehicleChasissInfosDb.ContainsKey(id) == false) {
        //  VehicleCustomInfoHelper.vehicleChasissInfosDb.Add(id, __state);
        //} else {
        //  VehicleCustomInfoHelper.vehicleChasissInfosDb[id] = __state;
        //}
        //info.debugLog(1);
        json = definition.ToString();
      } catch (Exception e) {
        Log.TWL(0,e.ToString(), true);
      }
      return true;
    }
    public static void Postfix(VehicleChassisDef __instance) {
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        throw new Exception(__instance.Description.Id + " has no unit custom info this should not happend");
      }
      if (info != null) {
        info.FakeVehicle = true;
        info.FakeVehicleMovementType = __instance.movementType;
        //Log.TW(0, "VehicleChassisDef.FromJSON " + __instance.Description.Id + " FiringArc:" + info.FiringArc);
        if (info.FiringArc == 0f) {
          if (__instance.HasTurret) { info.FiringArc = 360f; } else { info.FiringArc = 90f; };
        }
        //Log.WL(0, "=>"+ info.FiringArc);
        //Log.WL(1, " FakeVehicle:" + __instance.GetCustomInfo().FakeVehicle+ " HasTurret:"+ __instance.HasTurret);
      }
      __instance.ChassisInfo().SpawnAs = SpawnType.AsMech;
      //CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(__instance.PrefabIdentifier);
      //if (custRepDef != null) {
      //  if (custRepDef.SupressAllMeshes) {
      //    __instance.ChassisInfo().SpawnAs = SpawnType.AsMech;
      //    Log.W(1, "CustomActorRepresentationDef found on " + __instance.PrefabIdentifier);
      //  }
      //}
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_ChassisDef_fromJSON_Patch {
    public static bool isVehiclePaperDoll(this ChassisDef def) {
      UnitCustomInfo info = def.GetCustomInfo();
      if (info == null) { return false; }
      return info.FakeVehicle;
    }
    public static bool isVehiclePaperDoll(this MechDef def) {
      UnitCustomInfo info = def.GetCustomInfo();
      if (info == null) { return false; }
      return info.FakeVehicle;
    }
    public static bool Prefix(ChassisDef __instance, ref string json) {
      //Log.TW(0,"ChassisDef.FromJSON");
      try {
        if (__instance.Description != null) {
          UnitCustomInfo dinfo = CustomPrewarm.Core.getDeserializedObject(BattleTechResourceType.ChassisDef, __instance.Description.Id, "CustomUnits") as UnitCustomInfo;
          if (dinfo != null) {
            VehicleCustomInfoHelper.vehicleChasissInfosDb.AddOrUpdate(__instance.Description.Id, dinfo, (k, v) => { return dinfo; });
            return true;
          }
          Log.TWL(0,"ChassisDef:"+ __instance.Description.Id+" has no deserialized UnitCustomInfo. Should not happend");
        }
        JObject definition = JObject.Parse(json);
        string id = (string)definition["Description"]["Id"];
        Log.W(1,id);
        bool isFake = id.IsInFakeChassis();
        Log.WL(1, " isFake:" + isFake);
        UnitCustomInfo info = null;
        if (definition["CustomParts"] != null) {
          info = definition["CustomParts"].ToObject<UnitCustomInfo>();
          definition.Remove("CustomParts");
          json = definition.ToString();
        } else {
          info = new UnitCustomInfo();
        }
        VehicleCustomInfoHelper.vehicleChasissInfosDb.AddOrUpdate(id, info, (k, v) => { return info; });
        //if (VehicleCustomInfoHelper.vehicleChasissInfosDb.ContainsKey(id) == false) {
        //  VehicleCustomInfoHelper.vehicleChasissInfosDb.Add(id, info);
        //} else {
        //  info = VehicleCustomInfoHelper.vehicleChasissInfosDb[id];
        //}
        if (isFake) {
          info.FakeVehicle = true;
          if (Enum.TryParse<VehicleMovementType>((string)definition["movementType"],out VehicleMovementType movementType)) {
            info.FakeVehicleMovementType = movementType;
          } else {
            info.FakeVehicleMovementType = VehicleMovementType.Tracked;
          }
          json = json.ConstructMechFakeVehicle();
        }
        //info.debugLog(1);
      } catch (Exception e) {
        Log.TWL(0, json, true);
        Log.TWL(0,e.ToString(), true);
      }
      return true;
    }
    public static void Postfix(ChassisDef __instance) {
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        throw new Exception(__instance.Description.Id+" has no unit custom info this should not happend");
      }
      if((info.FakeVehicle == false)&&(info.SquadInfo.Troopers <= 1)&&(info.Naval == false)) {
        __instance.ChassisTags.UnionWith(Core.Settings.mechForcedTags);
      }else if (info.SquadInfo.Troopers > 1) {
        __instance.ChassisTags.UnionWith(Core.Settings.squadForcedTags);
      }else if (info.FakeVehicle) {
        __instance.ChassisTags.UnionWith(Core.Settings.vehicleForcedTags);
      }
      if (info.Naval) {
        __instance.ChassisTags.UnionWith(Core.Settings.navalForcedTags);
      }
      HashSet<PilotingClassDef> pilotingClasses = __instance.GetPilotingClass(false);
      if (pilotingClasses.Count == 0) {
        __instance.fallbackPilotingClass();
      }
      HashSet<DropClassDef> dropClasses = __instance.GetDropClass(false);
      if (dropClasses.Count == 0) {
        __instance.fallbackDropClass();
      }
      //if (__instance.ChassisInfo().SpawnAs == SpawnType.Undefined) {
      //__instance.ChassisInfo().SpawnAs = SpawnType.AsMech;
      //if (__instance.Description.Id.IsInFakeChassis()) {
      //UnitCustomInfo info = __instance.GetCustomInfo();
      //if (info != null) { info.FakeVehicle = true; }
      //__instance.ChassisInfo().SpawnAs = SpawnType.AsVehicle;
      //CustomActorRepresentationDef custRepDef = CustomActorRepresentationHelper.Find(__instance.PrefabIdentifier);
      //if(custRepDef != null) { if (custRepDef.SupressAllMeshes) { __instance.ChassisInfo().SpawnAs = SpawnType.AsMech; } }
      //};
      //}
    }
  }
  //[HarmonyPatch(typeof(MechDef))]
  //[HarmonyPatch("FromJSON")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(string) })]
  //public static class MechDef_FromJSON {
  //  public static void Postfix(MechDef __instance, ref string json) {
  //    Log.TWL(0, "MechDef.FromJSON "+ __instance.Description.Id);
  //    try {
  //      __instance.RegisterMech(__instance.Description.Id,__instance.ChassisID);
  //      if (__instance.IsVehicle()) { __instance.AddToFake(); }
  //    } catch (Exception e) {
  //      Log.LogWrite(json, true);
  //      Log.LogWrite(e.ToString() + "\n", true);
  //    }
  //  }
  //}
#pragma warning disable CS0252
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetAllModifiers")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
  public static class ToHit_GetSelfTerrainModifier {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.LogWrite("ToHit.GetAllModifiers transpliter\n", true);
      List<CodeInstruction> result = instructions.ToList();
      MethodInfo GetSelfTerrainModifier = AccessTools.Method(typeof(ToHit), "GetSelfTerrainModifier");
      if (GetSelfTerrainModifier != null) {
        Log.LogWrite(1, "source method found", true);
      } else {
        return result;
      }
      MethodInfo replacementMethod = AccessTools.Method(typeof(ToHit_GetSelfTerrainModifier), nameof(GetSelfTerrainModifier));
      if (replacementMethod != null) {
        Log.LogWrite(1, "target method found", true);
      } else {
        return result;
      }
      int methodCallIndex = result.FindIndex(instruction => instruction.opcode == OpCodes.Call && instruction.operand == GetSelfTerrainModifier);
      if (methodCallIndex >= 0) {
        Log.LogWrite(1, "methodCallIndex found " + methodCallIndex, true);
      }
      result[methodCallIndex].operand = replacementMethod;
      for (int thisIndex = methodCallIndex - 1; thisIndex > 0; --thisIndex) {
        Log.LogWrite(1, " result[" + thisIndex + "].opcode = " + result[thisIndex].opcode, true);
        if (result[thisIndex].opcode == OpCodes.Ldarg_0) {
          result[thisIndex].opcode = OpCodes.Ldarg_1;
          Log.LogWrite(1, "this opcode found changing to attacker", true);
          break;
        }
      }
      return result;
    }
    public static float GetSelfTerrainModifier(this AbstractActor actor, Vector3 attackPosition, bool isMelee) {
      //Log.LogWrite("AbstractActor.GetSelfTerrainModifier(" + actor.DisplayName + ":" + actor.GUID + ")\n");
      if (actor.UnaffectedDesignMasks()) {
        //Log.LogWrite(1, "unaffected", true);
        return 0f;
      }
      /*VehicleCustomInfo info = actor.GetCustomInfo();
      if (info != null) {
        if (info.Unaffected.DesignMasks) {
          Log.LogWrite(1, "unaffected", true);
          return 0f;
        }
      }*/
      return actor.Combat.ToHit.GetSelfTerrainModifier(attackPosition, isMelee);
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetAllModifiersDescription")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(LineOfFireLevel), typeof(bool) })]
  public static class ToHit_GetAllModifiersDescription {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.LogWrite("ToHit.GetAllModifiers transpliter\n", true);
      List<CodeInstruction> result = instructions.ToList();
      MethodInfo GetSelfTerrainModifier = AccessTools.Method(typeof(ToHit), "GetSelfTerrainModifier");
      if (GetSelfTerrainModifier != null) {
        Log.LogWrite(1, "source method found", true);
      } else {
        return result;
      }
      MethodInfo replacementMethod = AccessTools.Method(typeof(ToHit_GetSelfTerrainModifier), nameof(GetSelfTerrainModifier));
      if (replacementMethod != null) {
        Log.LogWrite(1, "target method found", true);
      } else {
        return result;
      }
      int methodCallIndex = result.FindIndex(instruction => instruction.opcode == OpCodes.Call && instruction.operand == GetSelfTerrainModifier);
      if (methodCallIndex >= 0) {
        Log.LogWrite(1, "methodCallIndex found " + methodCallIndex, true);
      } else {
        Log.LogWrite(1, "methodCallIndex not found ", true);
        return result;
      }
      result[methodCallIndex].operand = replacementMethod;
      for (int thisIndex = methodCallIndex - 1; thisIndex > 0; --thisIndex) {
        Log.LogWrite(1, " result[" + thisIndex + "].opcode = " + result[thisIndex].opcode, true);
        if (result[thisIndex].opcode == OpCodes.Ldarg_0) {
          result[thisIndex].opcode = OpCodes.Ldarg_1;
          Log.LogWrite(1, "this opcode found changing to attacker", true);
          break;
        }
      }
      return result;
    }
  }
#pragma warning restore CS0252
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ApplyDesignMaskStickyEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DesignMaskDef), typeof(int) })]
  public static class AbstractActor_ApplyDesignMaskStickyEffect {
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("SetOccupiedDesignMask")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DesignMaskDef), typeof(int), typeof(List<DesignMaskDef>) })]
  public static class AbstractActor_SetOccupiedDesignMask {
    private static MethodInfo occupiedDesignMaskSet = null;
    private static FieldInfo opuDesignMask = null;
    private delegate void occupiedDesignMaskSetDelegate(AbstractActor actor, DesignMaskDef mask);
    private static occupiedDesignMaskSetDelegate occupiedDesignMaskSetInkover = null;
    public static bool Prepare() {
      try {
        occupiedDesignMaskSet = typeof(AbstractActor).GetProperty("occupiedDesignMask", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
        opuDesignMask = typeof(AbstractActor).GetField("opuDesignMask", BindingFlags.Instance | BindingFlags.NonPublic);
        if (opuDesignMask == null) {
          Log.LogWrite(0, "Can't find opuDesignMask", true);
          return false;
        }
        if (occupiedDesignMaskSet == null) {
          Log.LogWrite(0, "Can't find occupiedDesignMaskSet", true);
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(0, e.ToString(), true);
        return false;
      }
      var dm = new DynamicMethod("CUoccupiedDesignMaskSet", null, new Type[] { typeof(AbstractActor), typeof(DesignMaskDef) }, typeof(AbstractActor));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Call, occupiedDesignMaskSet);
      gen.Emit(OpCodes.Ret);
      occupiedDesignMaskSetInkover = (occupiedDesignMaskSetDelegate)dm.CreateDelegate(typeof(occupiedDesignMaskSetDelegate));
      return true;
    }
    public static bool Prefix(AbstractActor __instance,ref DesignMaskDef mask, int stackItemUID, ref List<DesignMaskDef> approvedMasks) {
      Log.TWL(0, "AbstractActor.SetOccupiedDesignMask prefx " + __instance.DisplayName + ":" + __instance.GUID);
      try {
        if (__instance.UnaffectedDesignMasks()) {
          Log.LogWrite(1, "unaffected", true);
          mask = null;
          //__instance.occupiedDesignMask = null;
          //occupiedDesignMaskSetInkover(__instance, mask);
          //opuDesignMask.SetValue(__instance, null);
          if (approvedMasks != null) { approvedMasks.Clear(); };
          return true;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("OnPositionUpdate")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Quaternion), typeof(int), typeof(bool), typeof(List<DesignMaskDef>), typeof(bool) })]
  public static class AbstractActor_OnPositionUpdate {
    public static bool Prefix(AbstractActor __instance, Vector3 position, Quaternion heading, int stackItemUID,ref bool updateDesignMask, List<DesignMaskDef> approvedMasks, bool skipAbilityLogging) {
      if (__instance.UnaffectedDesignMasks()) {
        updateDesignMask = false;
        Thread.CurrentThread.SetFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
      return true;
    }
    public static void Postfix(AbstractActor __instance, Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> approvedMasks, bool skipAbilityLogging, ref DesignMaskDef ___opuDesignMask) {
      if (__instance.UnaffectedDesignMasks()) {
        Traverse.Create(__instance).Property<DesignMaskDef>("occupiedDesignMask").Value = null;
        ___opuDesignMask = null;
        Thread.CurrentThread.ClearFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("DamagePerShotPredicted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DesignMaskDef), typeof(AttackImpactQuality), typeof(ICombatant), typeof(LineOfFireLevel) })]
  public static class Weapon_DamagePerShotFromPosition {
    public static bool Prefix(Weapon __instance, ref DesignMaskDef designMask, AttackImpactQuality blowQuality, ICombatant target, LineOfFireLevel lofLevel) {
      //Log.LogWrite("Weapon.DamagePerShotPredicted prefix\n");
      if (__instance.parent.UnaffectedDesignMasks()) {
        //Log.LogWrite(1, "unaffected. Tie designMask to null", true);
        designMask = null;
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("FindBlockerBetween")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3) })]
  public static class PathNodeGrid_FindBlockerBetween {
    private static FieldInfo FowningActor = null;
    public static bool Prepare() {
      try {
        FowningActor = typeof(PathNodeGrid).GetField("owningActor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (FowningActor == null) {
          Log.LogWrite(0, "PathNodeGrid.FindBlockerBetween Can't find owningActor", true);
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(0, e.ToString(), true);
        return false;
      }
      return true;
    }
    public static void Postfix(PathNodeGrid __instance, Vector3 from, Vector3 to, ref bool __result) {
      try {
        AbstractActor owningActor = (AbstractActor)FowningActor.GetValue(__instance);
        if (owningActor.UnaffectedPathing()) {
          __result = false;
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("FindBlockerReciprocal")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3) })]
  public static class PathNodeGrid_FindBlockerReciprocal {
    //private static FieldInfo FowningActor = null;
    //public static bool Prepare() {
    //  try {
    //    FowningActor = typeof(PathNodeGrid).GetField("owningActor", BindingFlags.Instance | BindingFlags.NonPublic);
    //    if (FowningActor == null) {
    //      Log.LogWrite(0, "Can't find owningActor", true);
    //      return false;
    //    }
    //  } catch (Exception e) {
    //    Log.LogWrite(0, e.ToString(), true);
    //    return false;
    //  }
    //  return true;
    //}
    public static bool Prefix(PathNodeGrid __instance, AbstractActor ___owningActor, Vector3 from, Vector3 to, ref bool __result) {
      try {
        if (__instance.FindBlockerBetween(from, to) == true) { __result = true; return false; };
        if (__instance.FindBlockerBetween(to, from) == true) { __result = true; return false; };
        __result = false;
      } 
      catch (System.IndexOutOfRangeException) {
        __result = true;
      }
      catch(Exception e){
        Log.TWL(0, "Actor:" + (___owningActor==null?"null": ___owningActor.PilotableActorDef.Description.Id)+" from:"+from+" to:"+to, true);
        Log.TWL(0, e.ToString(), true);
        __result = true;
      }
      return false;
    }
  }
  /*[HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform), typeof(bool) })]
  public static class PilotableActorRepresentation_Init {
    public static void Postfix(PilotableActorRepresentation __instance, AbstractActor unit, Transform parentTransform, bool isParented) {
      Log.LogWrite("PilotableActorRepresentation.Init postfix " + unit.DisplayName + ":" + unit.GUID + "\n");
      BTLight[] lights = __instance.GetComponentsInChildren<BTLight>(true);
      for (int t = 0; t < lights.Length; ++t) {
        lights[t].lastPosition = lights[t].lightTransform.position;
        lights[t].RefreshLightSettings(true);
        Log.LogWrite("  light[" + t + "] - " + lights[t].gameObject.name + ":" + lights[t].gameObject.GetInstanceID()
          + " pos:" + lights[t].lightTransform.position
          + " last pos:" + lights[t].lastPosition
          + " rot:" + lights[t].lightTransform.rotation.eulerAngles + "\n");
      }
    }
  }*/
  [HarmonyPatch(typeof(VehicleRepresentation))]
  [HarmonyPatch("GetDestructibleObject")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int) })]
  public static class VehicleRepresentation_GetDestructibleObject {
    public static void Postfix(VehicleRepresentation __instance, ref MechDestructibleObject __result) {
      try {
        UnitCustomInfo info = __instance.parentActor.GetCustomInfo();
        if (info == null) { return; }
        if (info.NullifyBodyMesh) { __result = null; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
  public class CustomPartsDirector : MonoBehaviour {

  }
  public static class PilotableActorRepresentation_Init_vehicle {
    private static Dictionary<int, VehicleChassisDef> registredVehiclesChassis = new Dictionary<int, VehicleChassisDef>();
    public static void RegisterChassis(this PilotableActorRepresentation rep, VehicleChassisDef def) {
      if (registredVehiclesChassis.ContainsKey(rep.GetInstanceID())) {
        registredVehiclesChassis[rep.GetInstanceID()] = def;
      } else {
        registredVehiclesChassis.Add(rep.GetInstanceID(), def);
      }
    }
    public static VehicleChassisDef RegistredChassis(this PilotableActorRepresentation rep) {
      if(registredVehiclesChassis.TryGetValue(rep.GetInstanceID(),out VehicleChassisDef chassis)) {
        return chassis;
      }
      return null;
    }
    public static void MovePosition(Transform transform, string name, CustomVector pos, CustomVector rotation) {
      Log.LogWrite(" " + name + " pos:" + transform.position + " rot:" + transform.rotation.eulerAngles);
      if (rotation.set) transform.rotation = Quaternion.Euler(rotation.vector);
      if (pos.set) transform.position = pos.vector;
      Log.LogWrite(" -> " + transform.position + " rot: " + transform.rotation.eulerAngles + "\n");
    }
    public static bool Prefix(PilotableActorRepresentation __instance, AbstractActor unit, Transform parentTransform, bool isParented) {
      Log.LogWrite("PilotableActorRepresentation.Init prefix "+__instance.gameObject.name);
      if (unit != null) {
        Log.WL(1,new Text(unit.DisplayName).ToString());
      } else if(__instance.RegistredChassis() != null) {
        Log.WL(1, __instance.RegistredChassis().Description.Id);
      } else {
        Log.WL("representation have no unit and chassiss");
        return true;
      }
      //__instance.gameObject.printComponents(1);
      try {
        VehicleRepresentation vRep = __instance as VehicleRepresentation;
        Vehicle vehicle = unit as Vehicle;
        if (vRep == null) {
          Log.LogWrite(" not a vehicle\n");
          return true;
        }
        UnitCustomInfo info = null;
        if (vehicle != null) { info = vehicle.GetCustomInfo(); } else if(__instance.RegistredChassis() != null) {
          info = __instance.RegistredChassis().GetCustomInfo();
        }
        if (info == null) {
          Log.LogWrite(" no custom info\n");
          return true;
        }
        if (info.NullifyBodyMesh) {
          //Transform j_Root = vRep.transform.FindRecursive("j_Root");
          SkinnedMeshRenderer[] meshes = vRep.GetComponentsInChildren<SkinnedMeshRenderer>();

          GameObject go = new GameObject("Empty");
          //j_Null.transform.SetParent(vRep.transform);
          //j_Null.transform.localPosition = Vector3.down * 1000f;
          //j_Null.transform.localScale = Vector3.zero;
          Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
          go.AddComponent<MeshRenderer>();
          if (meshes != null) {
            foreach (SkinnedMeshRenderer mesh in meshes) {
              mesh.sharedMesh = emptyMesh;
              //mesh.transform.localScale = Vector3.zero;
              //mesh.rootBone = j_Null.transform;
            }
          }
          GameObject.Destroy(go);
        }
        if (unit != null) {
          MovePosition(vRep.TurretAttach, "vRep.TurretAttach", info.TurretAttach.offset, info.TurretAttach.rotate);
          MovePosition(vRep.BodyAttach, "vRep.BodyAttach", info.BodyAttach.offset, info.BodyAttach.rotate);
          MovePosition(vRep.TurretLOS, "vRep.TurretLOS", info.TurretLOS.offset, info.TurretLOS.rotate);
          MovePosition(vRep.LeftSideLOS, "vRep.LeftSideLOS", info.LeftSideLOS.offset, info.LeftSideLOS.rotate);
          MovePosition(vRep.RightSideLOS, "vRep.RightSideLOS", info.RightSideLOS.offset, info.RightSideLOS.rotate);
          MovePosition(vRep.leftVFXTransform, "vRep.leftVFXTransform", info.leftVFXTransform.offset, info.leftVFXTransform.rotate);
          MovePosition(vRep.rightVFXTransform, "vRep.rightVFXTransform", info.rightVFXTransform.offset, info.rightVFXTransform.rotate);
          MovePosition(vRep.rearVFXTransform, "vRep.rearVFXTransform", info.rearVFXTransform.offset, info.rearVFXTransform.rotate);
          MovePosition(vRep.thisTransform, "vRep.thisTransform", info.thisTransform.offset, info.thisTransform.rotate);
        } else {
          MovePosition(vRep.TurretAttach, "vRep.TurretAttach", info.HangarTransforms.TurretAttach.offset, info.HangarTransforms.TurretAttach.rotate);
          MovePosition(vRep.BodyAttach, "vRep.BodyAttach", info.HangarTransforms.BodyAttach.offset, info.HangarTransforms.BodyAttach.rotate);
          MovePosition(vRep.TurretLOS, "vRep.TurretLOS", info.HangarTransforms.TurretLOS.offset, info.HangarTransforms.TurretLOS.rotate);
          MovePosition(vRep.LeftSideLOS, "vRep.LeftSideLOS", info.HangarTransforms.LeftSideLOS.offset, info.HangarTransforms.LeftSideLOS.rotate);
          MovePosition(vRep.RightSideLOS, "vRep.RightSideLOS", info.HangarTransforms.RightSideLOS.offset, info.HangarTransforms.RightSideLOS.rotate);
          MovePosition(vRep.leftVFXTransform, "vRep.leftVFXTransform", info.HangarTransforms.leftVFXTransform.offset, info.HangarTransforms.leftVFXTransform.rotate);
          MovePosition(vRep.rightVFXTransform, "vRep.rightVFXTransform", info.HangarTransforms.rightVFXTransform.offset, info.HangarTransforms.rightVFXTransform.rotate);
          MovePosition(vRep.rearVFXTransform, "vRep.rearVFXTransform", info.HangarTransforms.rearVFXTransform.offset, info.HangarTransforms.rearVFXTransform.rotate);
          MovePosition(vRep.thisTransform, "vRep.thisTransform", info.HangarTransforms.thisTransform.offset, info.HangarTransforms.thisTransform.rotate);
        }
        BTLight[] lights = __instance.GetComponentsInChildren<BTLight>(true);
        List<CustomTransform> lightsTransforms = unit != null ? info.lightsTransforms : info.HangarTransforms.lightsTransforms;
        if (lights != null) {
          for (int t = 0; t < lights.Length; ++t) {
            if (t >= lightsTransforms.Count) { break; }
            MovePosition(lights[t].lightTransform, "lights[" + t + "].lightTransform", lightsTransforms[t].offset, lightsTransforms[t].rotate);
            lights[t].lastPosition = lights[t].lightTransform.position;
            lights[t].lastRotation = lights[t].lightTransform.rotation;
            lights[t].RefreshLightSettings(true);
            Log.LogWrite("  light[" + t + "] - " + lights[t].gameObject.name + ":" + lights[t].gameObject.GetInstanceID()
              + " pos:" + lights[t].lastPosition
              + " rot:" + lights[t].lastRotation.eulerAngles + "\n");
          }
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
    public static VehicleChassisLocations toVehicleLocation(this ChassisLocations loc) {
      switch (loc) {
        case ChassisLocations.CenterTorso: return VehicleChassisLocations.Turret;
        case ChassisLocations.RightTorso: return VehicleChassisLocations.Turret;
        case ChassisLocations.LeftTorso: return VehicleChassisLocations.Turret;
        case ChassisLocations.LeftArm: return VehicleChassisLocations.Front;
        case ChassisLocations.RightArm: return VehicleChassisLocations.Rear;
        case ChassisLocations.LeftLeg: return VehicleChassisLocations.Left;
        case ChassisLocations.RightLeg: return VehicleChassisLocations.Right;
        default: return VehicleChassisLocations.Turret;
      }
    }

    public static void SpawnCustomParts(this MechDef mechDef, MechRepresentationSimGame rep) {
      Log.TWL(0, "mechDef.SpawnCustomParts " + mechDef.ChassisID);
      UnitCustomInfo info = mechDef.Chassis.GetCustomInfo();
      if (info == null) {
        Log.WL(1, "no custom info");
        return;
      }
      try {
        //if (rep.gameObject.GetComponent<CustomPartsDirector>() != null) {
          //Log.WL(1, "Custom parts already spawned");
          //return;
        //}
        //rep.gameObject.AddComponent<CustomPartsDirector>();
        foreach (var AnimPart in info.CustomParts) {
          int location = 1;
          Log.WL(1, AnimPart.prefab + " req components: " + AnimPart.RequiredComponents.Count);
          if (AnimPart.RequiredComponents.Count > 0) {
            bool suitable_component_found = false;
            foreach (RequiredComponent rcomp in AnimPart.RequiredComponents) {
              suitable_component_found = false;
              Log.WL(2, "condition def:" + rcomp.DefId + " cat:" + rcomp.CategoryId);
              foreach (MechComponentRef component in mechDef.Inventory) {
                Log.WL(3, "component " + component.ComponentDefID + " loc:" + component.MountedLocation);
                if (rcomp.Test(component)) {
                  Log.WL(4, "success");
                  suitable_component_found = true; break;
                } else {
                  Log.WL(4, "fail");
                }
              }
              if (suitable_component_found) { break; }
            }
            if (suitable_component_found == false) { continue; }
          };
          if (rep != null) {
            if (AnimPart.RequiredUpgradesSet.Count > 0) {
              bool found = false;
              foreach (MechComponentRef component in mechDef.Inventory) {
                if (mechDef.IsVehicle()) {
                  if (AnimPart.VehicleChassisLocation != component.MountedLocation.toVehicleLocation()) { continue; }
                } else {
                  if (AnimPart.MechChassisLocation != component.MountedLocation) { continue; }
                }
                if (AnimPart.RequiredUpgradesSet.Contains(component.ComponentDefID)) { found = true; break; }
              }
              if (found == false) { continue; }
            }
            location = mechDef.IsVehicle()?(int)AnimPart.VehicleChassisLocation:(int)AnimPart.MechChassisLocation;
          }
          UnitsAnimatedPartsHelper.SpawnAnimatedPart(mechDef,rep,AnimPart,location);
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
    public static void Postfix(PilotableActorRepresentation __instance, AbstractActor unit, Transform parentTransform, bool isParented) {
      if (unit == null) { return; };
      Log.TWL(0, "PilotableActorRepresentation.Init postfix " + new Text(unit.DisplayName).ToString() + ":" + unit.GUID);
      try {
        QuadLegsRepresentation quadLegs = __instance.gameObject.GetComponent<QuadLegsRepresentation>();
        if (quadLegs != null) { return; }
        AlternateMechRepresentations altReps = __instance.gameObject.GetComponent<AlternateMechRepresentations>();
        if (altReps != null) { return; }
        int instanceId = unit.GameRep.gameObject.GetInstanceID();
        if (VehicleCustomInfoHelper.unityInstanceIdActor.ContainsKey(instanceId) == false) {
          VehicleCustomInfoHelper.unityInstanceIdActor.Add(instanceId, unit);
        } else {
          VehicleCustomInfoHelper.unityInstanceIdActor[instanceId] = unit;
        }
        UnitCustomInfo info = unit.GetCustomInfo();
        if (info == null) {
          Log.WL(1,"no custom info");
          return;
        }
        Vehicle vehicle = unit as Vehicle;
        Mech mech = unit as Mech;
        foreach (var AnimPart in info.CustomParts) {
          int location = 1;
          Log.WL(1,AnimPart.prefab+" req components: "+ AnimPart.RequiredComponents.Count);
          if (AnimPart.RequiredComponents.Count > 0) {
            bool suitable_component_found = false;
            foreach (RequiredComponent rcomp in AnimPart.RequiredComponents) {
              suitable_component_found = false;
              Log.WL(2,"condition def:" + rcomp.DefId + " cat:" + rcomp.CategoryId);
              foreach (MechComponent component in unit.allComponents) {
                Log.WL(3,"component "+component.defId+" loc:"+component.Location);
                if (rcomp.Test(component)) {
                  Log.WL(4,"success");
                  suitable_component_found = true; break;
                } else {
                  Log.WL(4,"fail");
                }
              }
              if (suitable_component_found) { break; }
            }
            if (suitable_component_found == false) { continue; }
          };
          if (mech != null) {
            if(AnimPart.RequiredUpgradesSet.Count > 0) {
              List<MechComponent> components = mech.GetComponentsForLocation(AnimPart.MechChassisLocation, ComponentType.Upgrade);
              bool found = false;
              foreach(MechComponent component in components) {
                if (AnimPart.RequiredUpgradesSet.Contains(component.defId)) { found = true; break; }
              }
              if (found == false) { continue; }
            }
            location = (int)AnimPart.MechChassisLocation;
          } else if (vehicle != null) {
            if (AnimPart.RequiredUpgradesSet.Count > 0) {
              List<MechComponent> components = vehicle.allComponents;
              bool found = false;
              foreach (MechComponent component in components) {
                if (AnimPart.RequiredUpgradesSet.Contains(component.defId)) { found = true; break; }
              }
              if (found == false) { continue; }
            }
            location = (int)AnimPart.VehicleChassisLocation;
          };
          UnitsAnimatedPartsHelper.SpawnAnimatedPart(unit, AnimPart, location);
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitEffectStats {
    public static void Postfix(AbstractActor __instance) {
      Log.LogWrite("AbstractActor.InitEffectStats " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        Log.LogWrite(" no custom info\n");
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowPartialMovementActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowPartialMovementActorStat, true);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowPartialSprintActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowPartialSprintActorStat, Core.Settings.PartialMovementOnlyWalkByDefault == false);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowRotateWhileJumpActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowRotateWhileJumpActorStat, Core.Settings.AllowRotateWhileJumpByDefault);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FakeVehicleActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.FakeVehicleActorStat, false);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NavalUnitActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NavalUnitActorStat, false);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.PathingActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.PathingActorStat, false);
        }
        return;
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.DesignMasksActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.DesignMasksActorStat, info.Unaffected.DesignMasks);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowPartialMovementActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowPartialMovementActorStat, info.Unaffected.AllowPartialMovement);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowPartialSprintActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowPartialSprintActorStat, info.Unaffected.AllowPartialSprint);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AllowRotateWhileJumpActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.AllowRotateWhileJumpActorStat, info.Unaffected.AllowRotateWhileJump);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.PathingActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.PathingActorStat, info.Unaffected.Pathing);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FireActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.FireActorStat, info.Unaffected.Fire);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.LandminesActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.LandminesActorStat, info.Unaffected.Landmines);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FlyingHeightActorStat) == false) {
        __instance.StatCollection.AddStatistic<float>(UnitUnaffectionsActorStats.FlyingHeightActorStat, info.FlyingHeight);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.MoveCostActorStat) == false) {
        __instance.StatCollection.AddStatistic<string>(UnitUnaffectionsActorStats.MoveCostActorStat, info.MoveCost);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NoMoveAnimationActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoMoveAnimationActorStat, info.NoMoveAnimations);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FiringArcActorStat) == false) {
        __instance.StatCollection.AddStatistic<float>(UnitUnaffectionsActorStats.FiringArcActorStat, info.FiringArc);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.MoveCostBiomeActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.MoveCostBiomeActorStat, info.Unaffected.MoveCostBiome);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.ArmsCountedAsLegsActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.ArmsCountedAsLegsActorStat, info.ArmsCountedAsLegs);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NoHeatActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoHeatActorStat, (info.SquadInfo.Troopers > 1));
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.BlockComponentsActivationActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.BlockComponentsActivationActorStat, false);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NoStabilityActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoStabilityActorStat, (info.SquadInfo.Troopers > 1));
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NoCritTransferActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoCritTransferActorStat, (info.SquadInfo.Troopers > 1));
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.HasNoLegsActorStat) == false) {
        Vehicle vehcile = __instance as Vehicle;
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.HasNoLegsActorStat, (info.SquadInfo.Troopers > 1) || (vehcile != null));
      }
      if (info.SquadInfo.Troopers > 1) {
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FakeVehicleActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.FakeVehicleActorStat, false);
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.TrooperSquadActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.TrooperSquadActorStat, true);
        }
      } else {
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.FakeVehicleActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.FakeVehicleActorStat, (info.FakeVehicle));
        }
        if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.TrooperSquadActorStat) == false) {
          __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.TrooperSquadActorStat, false);
        }
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NavalUnitActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NavalUnitActorStat, (info.Naval));
      }
      Log.LogWrite(1, "UnaffectedDesignMasks " + __instance.UnaffectedDesignMasks(), true);
      Log.LogWrite(1, "UnaffectedPathing " + __instance.UnaffectedPathing(), true);
      Log.LogWrite(1, "UnaffectedFire " + __instance.UnaffectedFire(), true);
      Log.LogWrite(1, "UnaffectedLandmines " + __instance.UnaffectedLandmines(), true);
      Log.LogWrite(1, "UnaffectedMoveCostBiome " + __instance.UnaffectedMoveCostBiome(), true);
      Log.LogWrite(1, "AoEHeightFix " + __instance.FlyingHeight(), true);
      Log.LogWrite(1, "MoveCost " + __instance.CustomMoveCostKey(), true);
      Log.LogWrite(1, "FakeVehicle " + __instance.FakeVehicle(), true);
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetTargetTerrainModifier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(bool) })]
  public static class CombatGameState_GetTargetTerrainModifier {
    public static void Postfix(ToHit __instance, ICombatant target, Vector3 targetPosition, bool isMelee, ref float __result) {
      if (__result > CustomAmmoCategories.Epsilon) {
        if (target.UnaffectedDesignMasks()) { __result = 0f; };
      }
    }
  }
  [HarmonyPatch(typeof(DestructibleUrbanFlimsy))]
  [HarmonyPatch("OnTriggerEnter")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Collider) })]
  public static class DestructibleUrbanFlimsy_OnTriggerEnter {
    public static bool Prefix(DestructibleUrbanFlimsy __instance, Collider other) {
      int instanceId = other.gameObject.GetInstanceID();
      Log.LogWrite("DestructibleUrbanFlimsy.OnTriggerEnter Prefix " + other.gameObject.name + ":" + instanceId + "\n");
      if (VehicleCustomInfoHelper.unityInstanceIdActor.ContainsKey(instanceId)) {
        AbstractActor actor = VehicleCustomInfoHelper.unityInstanceIdActor[instanceId];
        Log.LogWrite(1, "actor found:" + actor.DisplayName + ":" + actor.GUID, true);
        if (actor.UnaffectedPathing()) {
          Log.LogWrite(1, "ignore pathing", true);
          return false;
        }
      } else {
        Log.LogWrite(1, "actor not found", true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(DestructibleUrbanFlimsy))]
  [HarmonyPatch("OnCollisionEnter")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Collision) })]
  public static class DestructibleUrbanFlimsy_OnCollisionEnter {
    public static bool Prefix(DestructibleUrbanFlimsy __instance, Collision other) {
      //Log.LogWrite("DestructibleUrbanFlimsy.OnCollisionEnter Prefix " + other.collider.gameObject.name + ":" + other.collider.gameObject.GetInstanceID() + "\n");
      //return false;
      return true;
    }
  }
  [HarmonyPatch(typeof(DestructibleUrbanFlimsy))]
  [HarmonyPatch("OnParticleCollision")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(GameObject) })]
  public static class DestructibleUrbanFlimsy_OnParticleCollision {
    public static bool Prefix(DestructibleUrbanFlimsy __instance, GameObject other) {
      //Log.LogWrite("DestructibleUrbanFlimsy.OnParticleCollision Prefix  " + other.name + ":" + other.GetInstanceID() + "\n");
      return true;
    }
  }
  [HarmonyPatch(typeof(Vehicle))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(float), typeof(bool) })]
  public static class Vehicle_Init {
    public static void Postfix(Vehicle __instance, Vector3 position, float facing, bool checkEncounterCells) {
      Log.LogWrite("Vehicle.Init " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        Log.LogWrite(" no custom info\n");
        return;
      }
      Log.LogWrite(" Vehicle.HighestLOSPosition " + __instance.HighestLOSPosition);
      if (info.HighestLOSPosition.set) { __instance.HighestLOSPosition = info.HighestLOSPosition.vector; };
      Log.LogWrite(" -> " + __instance.HighestLOSPosition + "\n");
    }
  }
  [HarmonyPatch(typeof(PathingUtil))]
  [HarmonyPatch("DoesMovementLineCollide")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(List<AbstractActor>), typeof(Vector3), typeof(Vector3), typeof(AbstractActor), typeof(float) })]
  public static class PathingUtil_DoesMovementLineCollide {
    public static bool Prefix(AbstractActor thisActor, ref List<AbstractActor> actors) {
      try {
        int index = 0;
        if (actors == null) { return true; }
        if (thisActor == null) { return true; }
        bool mePathingUnaffected = thisActor.UnaffectedPathing();
        if (mePathingUnaffected == false) {
          while (index < actors.Count) {
            if (actors[index].UnaffectedPathing()) {
              actors.RemoveAt(index);
            } else {
              ++index;
            }
          }
        } else {
          while (index < actors.Count) {
            if (actors[index].UnaffectedPathing() == false) {
              actors.RemoveAt(index);
            } else {
              ++index;
            }
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static void Postfix(AbstractActor thisActor, ref AbstractActor collision, ref bool __result) {
      /*if (thisActor.UnaffectedPathing()) {
        collision = null;
        __result = false;
      }*/
    }
  }
  [HarmonyPatch(typeof(PathNode))]
  [HarmonyPatch("HasCollisionAt")]
  [HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(Vector3), typeof(AbstractActor), typeof(List<AbstractActor>),  })]
  public static class PathNode_HasCollisionAt {
    public static bool Prefix(AbstractActor unit, ref List<AbstractActor> allActors) {
      try {
        int index = 0;
        //occupyingActor = (AbstractActor)null;
        if (allActors == null) { return true; }
        if (unit == null) { return true; }
        bool mePathingUnaffected = unit.UnaffectedPathing();
        if (mePathingUnaffected == false) {
          while (index < allActors.Count) {
            if (allActors[index].UnaffectedPathing()) {
              allActors.RemoveAt(index);
            } else {
              ++index;
            }
          }
        } else {
          while (index < allActors.Count) {
            if (allActors[index].UnaffectedPathing() == false) {
              allActors.RemoveAt(index);
            } else {
              ++index;
            }
          }
        }
      }catch(Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public static void Postfix(AbstractActor unit, ref bool __result) {
      //if (unit.UnaffectedPathing()) {
      //  __result = false;
      //}
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetGradeModifier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float) })]
  public static class PathNodeGrid_GetGradeModifier {
    public static void Postfix(PathNodeGrid __instance, float grade, AbstractActor ___owningActor, ref float __result) {
      if (___owningActor.UnaffectedPathing()) {
        __result = 1f;
      }
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("GetSteepnessMultiplier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float), typeof(float) })]
  public static class PathNodeGrid_GetSteepnessMultiplier {
    public static void Postfix(PathNodeGrid __instance, float steepness, float grade, AbstractActor ___owningActor, ref float __result) {
      //Log.LogWrite("PathNodeGrid.GetSteepnessMultiplier postfix " + ___owningActor.DisplayName + ":" + ___owningActor.GUID + " "+steepness+"," + grade + "-> " + __result + "\n");
      if (___owningActor.UnaffectedPathing()) {
        __result = 1f;
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("WillFireAtTarget")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class Weapon_WillFireAtTarget {
    public static void Postfix(Weapon __instance, ICombatant target, ref bool __result) {
      if (__result == true) {
        if (((__instance.Type == WeaponType.Melee) && (__instance.WeaponSubType == WeaponSubType.Melee)) || (__instance.CantHitUnaffectedByPathing() == true)) {
          if (target.UnaffectedPathing()) {
            __result = false;
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("WillFireAtTargetFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(Vector3), typeof(Quaternion) })]
  public static class Weapon_WillFireAtTargetFromPosition {
    public static void Postfix(Weapon __instance, ICombatant target, Vector3 position, Quaternion rotation, ref bool __result) {
      try {
        if (__result == true) {
          if (((__instance.Type == WeaponType.Melee) && (__instance.WeaponSubType == WeaponSubType.Melee)) || (__instance.CantHitUnaffectedByPathing() == true)) {
            if (target.UnaffectedPathing()) {
              __result = false;
            }
          }
        }
      }catch(Exception e) {
        Log.TWL(0,e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDFireButton))]
  [HarmonyPatch("OnClick")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUDFireButton_OnClick {
    public static bool Prefix(CombatHUDFireButton __instance) {
      CombatHUD HUD = (CombatHUD)typeof(CombatHUDFireButton).GetProperty("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance, null);
      CombatHUDFireButton.FireMode fireMode = (CombatHUDFireButton.FireMode)typeof(CombatHUDFireButton).GetField("currentFireMode", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      if ((fireMode == CombatHUDFireButton.FireMode.Engage)|| (fireMode == CombatHUDFireButton.FireMode.DFA)) {
        Log.LogWrite("CombatHUDFireButton.OnClick: " + HUD.SelectedActor.DisplayName + " fire mode:" + fireMode + "\n");
        if (HUD.SelectedTarget.UnaffectedPathing()) {
          if (HUD.SelectedActor.UnaffectedPathing() == false) {
            GenericPopupBuilder popupBuilder = GenericPopupBuilder.Create("FORBIDEN", "You can't select this unit as melee target");
            popupBuilder.Render();
            return false;
          }
        }
      }
      if (fireMode == CombatHUDFireButton.FireMode.DFA) {
        if (HUD.SelectedTarget.UnaffectedPathing()) {
          HashSet<Weapon> forbiddenWeapon = new HashSet<Weapon>();
          foreach (Weapon weapon in HUD.SelectedActor.Weapons) {
            Log.LogWrite(" weapon:" + weapon.defId + " enabled:" + weapon.IsEnabled + "\n");
            if (weapon.IsEnabled == false) { continue; }
#if BT1_8
            //if (weapon.isWeaponUseInMelee().CanUseInMelee == false) { continue; }
            if (weapon.WeaponCategoryValue.CanUseInMelee == false) { continue; }
#else
            if (weapon.isWeaponUseInMelee() != WeaponCategory.AntiPersonnel) { continue; }
#endif
            if (weapon.CantHitUnaffectedByPathing() == false) { continue; };
            forbiddenWeapon.Add(weapon);
          }
          if(forbiddenWeapon.Count > 0) {
            StringBuilder body = new StringBuilder();
            body.Append("You can't use this weapons to perform DFA attack to target:");
            foreach(Weapon weapon in forbiddenWeapon) {
              body.Append("\n" + weapon.UIName);
            }
            GenericPopupBuilder popupBuilder = GenericPopupBuilder.Create("FORBIDEN", body.ToString());
            popupBuilder.Render();
            return false;
          }
        }
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(AIUtil))]
  [HarmonyPatch("ExpectedDamageForAttack")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(AIUtil.AttackType), typeof(List<Weapon>), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(AbstractActor) })]
  public static class AIUtil_ExpectedDamageForAttack {
    public static void Postfix(AbstractActor unit, AIUtil.AttackType attackType, ICombatant target, ref float __result) {
      if ((attackType != AIUtil.AttackType.Melee) && (attackType != AIUtil.AttackType.DeathFromAbove)) { return; }
      if (target.UnaffectedPathing() == false) { return; }
      if (unit.UnaffectedPathing()) { return; }
      __result = 0f;
    }
  }
  [HarmonyPatch(typeof(HostileDamageFactor))]
  [HarmonyPatch("expectedDamageForMelee")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class HostileDamageFactor_expectedDamageForMelee {
    public static void Postfix(AbstractActor attackingUnit, ICombatant targetUnit, ref float __result) {
      if (targetUnit.UnaffectedPathing() == false) { return; }
      if (attackingUnit.UnaffectedPathing()) { return; }
      __result = 0f;
    }
  }
  [HarmonyPatch(typeof(HostileDamageFactor))]
  [HarmonyPatch("expectedDamageForDFA")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(Quaternion), typeof(ICombatant), typeof(Vector3), typeof(Quaternion), typeof(bool) })]
  public static class HostileDamageFactor_expectedDamageForDFA {
    public static void Postfix(AbstractActor attackingUnit, ICombatant targetUnit, ref float __result) {
      if (targetUnit.UnaffectedPathing() == false) { return; }
      if (attackingUnit.UnaffectedPathing()) { return; }
      __result = 0f;
    }
  }
  [HarmonyPatch(typeof(CombatHUDWeaponSlot))]
  [HarmonyPatch("contemplatingMelee")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant) })]
  public static class CombatHUDWeaponSlot_contemplatingMelee {
    public static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target, ref bool __result) {
      /*if (__result == true) {
        if (target.UnaffectedPathing()) {
          __result = false;
        }
      }*/
    }
  }
  [HarmonyPatch(typeof(Weapon))]
  [HarmonyPatch("GetToHitFromPosition")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ICombatant), typeof(int), typeof(Vector3), typeof(Vector3), typeof(bool), typeof(bool), typeof(bool) })]
  public static class Weapon_GetToHitFromPosition {
    public static void Postfix(Weapon __instance, ICombatant target, int numTargets, Vector3 attackPosition, Vector3 targetPosition, bool bakeInEvasiveModifier, bool targetIsEvasive, bool isMoraleAttack, ref float __result) {
      if (__result > 0f) {
        if ((__instance.Type != WeaponType.Melee) || (__instance.WeaponSubType != WeaponSubType.Melee)) { return; }
        if (target.UnaffectedPathing() == false) { return; }
        if (__instance.CantHitUnaffectedByPathing() == false) { return; }
        if (__instance.parent.UnaffectedPathing() == true) { return; }
        __result = 0f;
      }
    }
  }
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetToHitChance")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Weapon), typeof(ICombatant), typeof(Vector3), typeof(Vector3), typeof(int), typeof(MeleeAttackType), typeof(bool) })]
  public static class ToHit_GetToHitChance {
    public static void Postfix(ToHit __instance, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, int numTargets, MeleeAttackType meleeAttackType, bool isMoraleAttack, ref float __result) {
      if (__result > 0f) {
        if ((weapon.Type != WeaponType.Melee) || (weapon.WeaponSubType != WeaponSubType.Melee)) { return; }
        if (target.UnaffectedPathing() == false) { return; }
        if (weapon.CantHitUnaffectedByPathing() == false) { return; }
        if (attacker.UnaffectedPathing() == true) { return; }
        __result = 0f;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("AddInstability")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(float), typeof(StabilityChangeSource), typeof(string) })]
  public static class Mech_AddInstability {
    public static bool Prefix(Mech __instance, float amt, StabilityChangeSource source, string sourceGuid) {
      if (__instance.isHasStability() == false) { return false; }
      return true;
    }
  }
  [HarmonyPatch(typeof(MechDef))]
  [HarmonyPatch("RefreshChassis")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MechDef_RefreshChassis {
    public static void Postfix(MechDef __instance) {
      try {
        if (__instance.Chassis == null) { return; }
        //Log.TWL(0, "MechDef.RefreshChassis " + __instance.Description.Id);
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return; }
        if (info.MeleeWeaponOverride == null) { return; }
        string meleeDef = info.MeleeWeaponOverride.DefaultWeapon;
        if (info.MeleeWeaponOverride.Components != null) {
          foreach (BaseComponentRef cref in __instance.Inventory) {
            if (cref == null) { continue; }
            if (string.IsNullOrEmpty(cref.ComponentDefID)) { continue; }
            if (info.MeleeWeaponOverride.Components.ContainsKey(cref.ComponentDefID)) {
              meleeDef = info.MeleeWeaponOverride.Components[cref.ComponentDefID];
              break;
            }
          }
        }
        if (string.IsNullOrEmpty(meleeDef)) { meleeDef = "Weapon_MeleeAttack"; }
        //Log.WL(1, "meleeDef " + meleeDef);
        __instance.meleeWeaponRef = new MechComponentRef(meleeDef, "", ComponentType.Weapon, ChassisLocations.CenterTorso, -1, ComponentDamageLevel.Functional, false);
      } catch (Exception) {
        //Log.TWL(0, e.ToString(), true);
      }
    }
  }
}