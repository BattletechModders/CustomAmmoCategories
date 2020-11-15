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

namespace CustomUnits {
  public class CustomTransform {
    public CustomVector offset { get; set; }
    public CustomVector scale { get; set; }
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
  public class CustomMaterialInfo {
    public string shader { get; set; }
    public List<string> shaderKeyWords { get; set; }
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

  public class RequiredComponent {
    public string CategoryId { get; set; }
    public string DefId { get; set; }
    [JsonIgnore]
    public HashSet<int> SearchLocations { get; set; }
    public List<ChassisLocations> MechSearchLocations { set {
        foreach(ChassisLocations loc in value){ SearchLocations.Add((int)loc); }
      } }
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
  public class CustomPart {
    public string prefab { get; set; }
    public string boneName { get; set; }
    public Dictionary<string, CustomMaterialInfo> MaterialInfo { get; set; }
    public CustomTransform prefabTransform { get; set; }
    public VehicleChassisLocations VehicleChassisLocation { get; set; }
    public ChassisLocations MechChassisLocation { get; set; }
    public List<RequiredComponent> RequiredComponents { get; set; }
    public List<string> RequiredUpgrades { set {
        foreach(string def in value) { this.RequiredUpgradesSet.Add(def);}
      }
    }
    [JsonIgnore]
    public HashSet<string> RequiredUpgradesSet { get; private set; }
    public string AnimationType { get; set; }
    public JObject AnimationData {
      set {
        this.Data = value.ToString(Formatting.None);
      }
    }
    [JsonIgnore]
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
  public class UnitUnaffection {
    private static readonly float MinMoveClamp = 0.2f;
    private static readonly float MaxMoveClamp = 0.5f;
    [JsonIgnore]
    private float FMoveClamp;
    public bool DesignMasks { get; set; }
    public bool Pathing { get; set; }
    public bool MoveCostBiome { get; set; }
    public bool Fire { get; set; }
    public bool Landmines { get; set; }
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
    }
    public void debugLog(int initiation) {
      Log.LogWrite(initiation, "DesignMasks:" + DesignMasks, true);
      Log.LogWrite(initiation, "Pathing:" + Pathing, true);
      Log.LogWrite(initiation, "Fire:" + Fire, true);
      Log.LogWrite(initiation, "Landmines:" + Landmines, true);
      Log.LogWrite(initiation, "MoveCostBiome:" + MoveCostBiome, true);
    }
  }
  public class HangarLocationTransforms {
    public CustomTransform TurretAttach { get; set; }
    public CustomTransform BodyAttach { get; set; }
    public CustomTransform TurretLOS { get; set; }
    public CustomTransform LeftSideLOS { get; set; }
    public CustomTransform RightSideLOS { get; set; }
    public CustomTransform leftVFXTransform { get; set; }
    public CustomTransform rightVFXTransform { get; set; }
    public CustomTransform rearVFXTransform { get; set; }
    public List<CustomTransform> lightsTransforms { get; set; }
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
  public class MeleeWeaponOverrideDef {
    public string DefaultWeapon { get; set; }
    public Dictionary<string, string> Components { get; set; }
    public MeleeWeaponOverrideDef() {
      DefaultWeapon = "Weapon_MeleeAttack";
      Components = new Dictionary<string, string>();
    }
  }
  public class UnitCustomInfo {
    public TrooperSquadDef SquadInfo { get; set; }
    public bool NullifyBodyMesh { get; set; }
    public float AOEHeight { get; set; }
    public bool TieToGroundOnDeath { get; set; }
    public float FiringArc { get; set; }
    public bool NoIdleAnimations { get; set; }
    public bool NoMoveAnimations { get; set; }
    public bool ArmsCountedAsLegs { get; set; }
    public float LegDestroyedMovePenalty { get; set; }
    public float LegDamageRedMovePenalty { get; set; }
    public float LegDamageYellowMovePenalty { get; set; }
    public float LegDamageRelativeInstability { get; set; }
    public float LegDestroyRelativeInstability { get; set; }
    public float LocDestroyedPermanentStabilityLossMod { get; set; }
    public CustomVector HighestLOSPosition { get; set; }
    public UnitUnaffection Unaffected { get; set; }
    public CustomTransform TurretAttach { get; set; }
    public CustomTransform BodyAttach { get; set; }
    public CustomTransform TurretLOS { get; set; }
    public CustomTransform LeftSideLOS { get; set; }
    public CustomTransform RightSideLOS { get; set; }
    public CustomTransform leftVFXTransform { get; set; }
    public CustomTransform rightVFXTransform { get; set; }
    public CustomTransform rearVFXTransform { get; set; }
    public List<CustomTransform> lightsTransforms { get; set; }
    public CustomTransform thisTransform { get; set; }
    public List<CustomPart> CustomParts { get; set; }
    public HangarLocationTransforms HangarTransforms { get; set; }
    public string MoveCost { get; set; }
    public Dictionary<string, float> MoveCostModPerBiome { get; set; }
    public MeleeWeaponOverrideDef MeleeWeaponOverride { get; set; }
    public UnitCustomInfo() {
      AOEHeight = 0f;
      HighestLOSPosition = new CustomVector(false);
      TurretAttach = new CustomTransform();
      BodyAttach = new CustomTransform();
      TurretLOS = new CustomTransform();
      LeftSideLOS = new CustomTransform();
      RightSideLOS = new CustomTransform();
      leftVFXTransform = new CustomTransform();
      rightVFXTransform = new CustomTransform();
      rearVFXTransform = new CustomTransform();
      thisTransform = new CustomTransform();
      CustomParts = new List<CustomPart>();
      Unaffected = new UnitUnaffection();
      lightsTransforms = new List<CustomTransform>();
      MoveCostModPerBiome = new Dictionary<string, float>();
      MoveCost = string.Empty;
      FiringArc = 0f;
      TieToGroundOnDeath = false;
      NoIdleAnimations = false;
      NullifyBodyMesh = false;
      HangarTransforms = new HangarLocationTransforms();
      NoMoveAnimations = false;
      ArmsCountedAsLegs = false;
      LegDestroyedMovePenalty = -1f;
      LegDamageRedMovePenalty = -1f;
      LegDamageYellowMovePenalty = -1f;
      LegDamageRelativeInstability = -1f;
      LegDestroyRelativeInstability = 1f;
      LocDestroyedPermanentStabilityLossMod = 1f;
      SquadInfo = new TrooperSquadDef();
      MeleeWeaponOverride = new MeleeWeaponOverrideDef();
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
    public static Dictionary<string, UnitCustomInfo> vehicleChasissInfosDb = new Dictionary<string, UnitCustomInfo>();
    public static Dictionary<int, AbstractActor> unityInstanceIdActor = new Dictionary<int, AbstractActor>();
    public static UnitCustomInfo GetCustomInfo(this VehicleChassisDef chassis) {
      if (vehicleChasissInfosDb.ContainsKey(chassis.Description.Id) == false) { return null; };
      return vehicleChasissInfosDb[chassis.Description.Id];
    }
    public static UnitCustomInfo GetCustomInfo(this ChassisDef chassis) {
      if (vehicleChasissInfosDb.ContainsKey(chassis.Description.Id) == false) { return null; };
      return vehicleChasissInfosDb[chassis.Description.Id];
    }
    public static UnitCustomInfo GetCustomInfo(this MechDef mechDef) {
      if (vehicleChasissInfosDb.ContainsKey(mechDef.ChassisID) == false) { return null; };
      return vehicleChasissInfosDb[mechDef.ChassisID];
    }
    public static UnitCustomInfo GetCustomInfo(this AbstractActor actor) {
      Mech mech = actor as Mech;
      if (mech != null) { return mech.MechDef.Chassis.GetCustomInfo(); };
      Vehicle vehicle = actor as Vehicle;
      if (vehicle != null) { return vehicle.VehicleDef.Chassis.GetCustomInfo(); };
      return null;
    }
  }
  [HarmonyPatch(typeof(VehicleChassisDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_VehicleChassisDef_fromJSON_Patch {
    public static bool Prefix(VehicleChassisDef __instance, ref string json) {
      Log.LogWrite("VehicleChassisDef.FromJSON\n");
      try {
        JObject definition = JObject.Parse(json);
        string id = (string)definition["Description"]["Id"];
        Log.LogWrite(id + "\n");
        if (definition["CustomParts"] != null) {
          UnitCustomInfo info = definition["CustomParts"].ToObject<UnitCustomInfo>();
          if (VehicleCustomInfoHelper.vehicleChasissInfosDb.ContainsKey(id) == false) {
            VehicleCustomInfoHelper.vehicleChasissInfosDb.Add(id, info);
          } else {
            VehicleCustomInfoHelper.vehicleChasissInfosDb[id] = info;
          }
          info.debugLog(1);
          definition.Remove("CustomParts");
        }
        json = definition.ToString();
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(ChassisDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class BattleTech_ChassisDef_fromJSON_Patch {
    public static bool Prefix(ChassisDef __instance, ref string json) {
      Log.LogWrite("ChassisDef.FromJSON\n");
      try {
        JObject definition = JObject.Parse(json);
        string id = (string)definition["Description"]["Id"];
        Log.LogWrite(id + "\n");
        if (definition["CustomParts"] != null) {
          UnitCustomInfo info = definition["CustomParts"].ToObject<UnitCustomInfo>();
          if (VehicleCustomInfoHelper.vehicleChasissInfosDb.ContainsKey(id) == false) {
            VehicleCustomInfoHelper.vehicleChasissInfosDb.Add(id, info);
          } else {
            VehicleCustomInfoHelper.vehicleChasissInfosDb[id] = info;
          }
          info.debugLog(1);
          definition.Remove("CustomParts");
        }
        json = definition.ToString();
      } catch (Exception e) {
        Log.LogWrite(json, true);
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
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
    public static bool Prefix(AbstractActor __instance,ref DesignMaskDef mask, int stackItemUID) {
      Log.LogWrite(0, "AbstractActor.ApplyDesignMaskStickyEffect prefx " + __instance.DisplayName + ":" + __instance.GUID, true);
      try {
        if (__instance.UnaffectedDesignMasks()) {
          mask = null;
        }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
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
      Log.LogWrite(0, "AbstractActor.SetOccupiedDesignMask prefx " + __instance.DisplayName + ":" + __instance.GUID);
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
      //Log.LogWrite("Weapon.DamagePerShotPredicted prefix\n");
      //Log.LogWrite(0, "AbstractActor.OnPositionUpdate prefx " + __instance.DisplayName + ":" + __instance.GUID);
      if (__instance.UnaffectedDesignMasks()) {
        //Log.LogWrite(1, "unaffected. Tie designMask to null", true);
        updateDesignMask = false;
      }
      return true;
    }
    public static void Postfix(AbstractActor __instance, Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> approvedMasks, bool skipAbilityLogging, ref DesignMaskDef ___opuDesignMask) {
      if (__instance.UnaffectedDesignMasks()) {
        //Log.LogWrite(1, "unaffected. Tie designMask to null", true);
        Traverse.Create(__instance).Property<DesignMaskDef>("occupiedDesignMask").Value = null;
        ___opuDesignMask = null;
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
      AbstractActor owningActor = (AbstractActor)FowningActor.GetValue(__instance);
      if (owningActor.UnaffectedPathing()) {
        //Log.LogWrite("PathNodeGrid.FindBlockerBetween postfix " + owningActor.DisplayName + ":" + owningActor.GUID + " from " + from + " to " + to + " result: " + __result + "\n");
        //Log.LogWrite(1, "can't be blocked", true);
        __result = false;
      }
    }
  }
  [HarmonyPatch(typeof(PathNodeGrid))]
  [HarmonyPatch("FindBlockerReciprocal")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(Vector3), typeof(Vector3) })]
  public static class PathNodeGrid_FindBlockerReciprocal {
    private static FieldInfo FowningActor = null;
    public static bool Prepare() {
      try {
        FowningActor = typeof(PathNodeGrid).GetField("owningActor", BindingFlags.Instance | BindingFlags.NonPublic);
        if (FowningActor == null) {
          Log.LogWrite(0, "Can't find owningActor", true);
          return false;
        }
      } catch (Exception e) {
        Log.LogWrite(0, e.ToString(), true);
        return false;
      }
      return true;
    }
    public static bool Prefix(PathNodeGrid __instance, Vector3 from, Vector3 to, ref bool __result) {
      try {
        if (__instance.FindBlockerBetween(from, to) == true) { __result = true; return false; };
        if (__instance.FindBlockerBetween(to, from) == true) { __result = true; return false; };
        __result = false;
      } catch (Exception) {
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
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor), typeof(Transform), typeof(bool) })]
  public static class PilotableActorRepresentation_Init {
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
          SkinnedMeshRenderer[] meshes = vRep.GetComponentsInChildren<SkinnedMeshRenderer>();
          GameObject go = new GameObject("Empty");
          Mesh emptyMesh = go.AddComponent<MeshFilter>().mesh;
          go.AddComponent<MeshRenderer>();
          if (meshes != null) {
            foreach (SkinnedMeshRenderer mesh in meshes) {
              mesh.sharedMesh = emptyMesh;
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
                if (mechDef.IsChassisFake()) {
                  if (AnimPart.VehicleChassisLocation != component.MountedLocation.toVehicleLocation()) { continue; }
                } else {
                  if (AnimPart.MechChassisLocation != component.MountedLocation) { continue; }
                }
                if (AnimPart.RequiredUpgradesSet.Contains(component.ComponentDefID)) { found = true; break; }
              }
              if (found == false) { continue; }
            }
            location = mechDef.IsChassisFake()?(int)AnimPart.VehicleChassisLocation:(int)AnimPart.MechChassisLocation;
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
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("UpdateRotation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_UpdateRotation {
    private static MethodInfo mOwningVehicleSet;
    private delegate void OwningVehicleSetDelegate(ActorMovementSequence seq, Vehicle v);
    private static OwningVehicleSetDelegate OwningVehicleSetInvoker = null;
    public static bool Prepare() {
      mOwningVehicleSet = null;
      try {
        mOwningVehicleSet = typeof(ActorMovementSequence).GetProperty("OwningVehicle", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true);
        if (mOwningVehicleSet == null) { return false; }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
        return false;
      }
      var dm = new DynamicMethod("CUOwningVehicleSet", null, new Type[] { typeof(ActorMovementSequence), typeof(Vehicle) }, typeof(ActorMovementSequence));
      var gen = dm.GetILGenerator();
      gen.Emit(OpCodes.Ldarg_0);
      gen.Emit(OpCodes.Ldarg_1);
      gen.Emit(OpCodes.Call, mOwningVehicleSet);
      gen.Emit(OpCodes.Ret);
      OwningVehicleSetInvoker = (OwningVehicleSetDelegate)dm.CreateDelegate(typeof(OwningVehicleSetDelegate));
      return true;
    }
    public static void OwningVehicle(this ActorMovementSequence seq, Vehicle v) {
      OwningVehicleSetInvoker(seq, v);
    }
    public static bool Prefix(ActorMovementSequence __instance, ref Vehicle __state) {
      __state = null;
      if (__instance.OwningVehicle != null) {
        if (__instance.OwningVehicle.UnaffectedPathing() == false) { return true; };
        __state = __instance.OwningVehicle;
        __instance.OwningVehicle(null);
      }
      return true;
    }
    public static void Postfix(ActorMovementSequence __instance, ref Vehicle __state) {
      if (__state != null) {
        __instance.OwningVehicle(__state);
      }
    }
  }
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("CompleteMove")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class ActorMovementSequence_CompleteMove {
    public static bool Prepare() {
      return true;
    }
    public static bool Prefix(ActorMovementSequence __instance, ref Vehicle __state) {
      __state = null;
      if (__instance.OwningVehicle != null) {
        if (__instance.OwningVehicle.UnaffectedPathing() == false) { return true; };
        __state = __instance.OwningVehicle;
        __instance.OwningVehicle(null);
      }
      return true;
    }
    public static void Postfix(ActorMovementSequence __instance, ref Vehicle __state) {
      if (__state != null) {
        __instance.OwningVehicle(__state);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("InitEffectStats")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_InitStats {
    public static void Postfix(AbstractActor __instance) {
      Log.LogWrite("AbstractActor.InitEffectStats " + __instance.DisplayName + ":" + __instance.GUID + "\n");
      UnitCustomInfo info = __instance.GetCustomInfo();
      if (info == null) {
        Log.LogWrite(" no custom info\n");
        return;
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.DesignMasksActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.DesignMasksActorStat, info.Unaffected.DesignMasks);
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
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.AOEHeightActorStat) == false) {
        __instance.StatCollection.AddStatistic<float>(UnitUnaffectionsActorStats.AOEHeightActorStat, info.AOEHeight);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.MoveCostActorStat) == false) {
        __instance.StatCollection.AddStatistic<string>(UnitUnaffectionsActorStats.MoveCostActorStat, info.MoveCost);
      }
      if (__instance.StatCollection.ContainsStatistic(UnitUnaffectionsActorStats.NoMoveAnimationActorStat) == false) {
        __instance.StatCollection.AddStatistic<bool>(UnitUnaffectionsActorStats.NoMoveAnimationActorStat, info.NoMoveAnimations);
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
      Log.LogWrite(1, "UnaffectedDesignMasks " + __instance.UnaffectedDesignMasks(), true);
      Log.LogWrite(1, "UnaffectedPathing " + __instance.UnaffectedPathing(), true);
      Log.LogWrite(1, "UnaffectedFire " + __instance.UnaffectedFire(), true);
      Log.LogWrite(1, "UnaffectedLandmines " + __instance.UnaffectedLandmines(), true);
      Log.LogWrite(1, "UnaffectedMoveCostBiome " + __instance.UnaffectedMoveCostBiome(), true);
      Log.LogWrite(1, "AoEHeightFix " + __instance.AoEHeightFix(), true);
      Log.LogWrite(1, "MoveCost " + __instance.CustomMoveCostKey(), true);
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
  [HarmonyPatch(typeof(PilotableActorRepresentation))]
  [HarmonyPatch("RefreshSurfaceType")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(bool) })]
  public static class PilotableActorRepresentation_RefreshSurfaceType {
    public static bool Prefix(PilotableActorRepresentation __instance, bool forceUpdate, ref bool __result) {
      Log.LogWrite("PilotableActorRepresentation.RefreshSurfaceType Prefix\n");
      if (__instance.parentCombatant == null) { return false; }
      if (__instance.parentCombatant.UnaffectedDesignMasks()) {
        Log.LogWrite(" unaffected\n");
        __result = true;
        return false;
      }
      return true;
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
      //Log.LogWrite("PathNodeGrid.GetGradeModifier postfix " + ___owningActor.DisplayName + ":" + ___owningActor.GUID + " " + grade + "->" + __result + "\n");
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
      if (__result == true) {
        if (((__instance.Type == WeaponType.Melee) && (__instance.WeaponSubType == WeaponSubType.Melee)) || (__instance.CantHitUnaffectedByPathing() == true)) {
          if (target.UnaffectedPathing()) {
            __result = false;
          }
        }
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
      if (fireMode == CombatHUDFireButton.FireMode.Engage) {
        Log.LogWrite("CombatHUDFireButton.OnClick: " + HUD.SelectedActor.DisplayName + " fire mode:" + fireMode + "\n");
        if (HUD.SelectedTarget.UnaffectedPathing()) {
          GenericPopupBuilder popupBuilder = GenericPopupBuilder.Create("FORBIDEN", "You can't select this unit as melee target");
          popupBuilder.Render();
          return false;
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
        //Log.LogWrite("ToWeaponHit.GetToHitFromPosition " + __instance.defId + " " + target.DisplayName + " tohit:" + __result + "\n");
        if (((__instance.Type == WeaponType.Melee) && (__instance.WeaponSubType == WeaponSubType.Melee)) || (__instance.CantHitUnaffectedByPathing() == true)) {
          //Log.LogWrite(" melee\n");
          if (target.UnaffectedPathing()) {
            //Log.LogWrite(" unaffected\n");
            __result = 0f;
          }
        }
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
        //Log.LogWrite("ToHit.GetToHitChance "+weapon.defId+" "+target.DisplayName+" tohit:"+__result+"\n");
        if (((weapon.Type == WeaponType.Melee) && (weapon.WeaponSubType == WeaponSubType.Melee)) || (weapon.CantHitUnaffectedByPathing() == true)) {
          //Log.LogWrite(" melee\n");
          if (target.UnaffectedPathing()) {
            //Log.LogWrite(" unaffected\n");
            __result = 0f;
          }
        }
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
        Log.TWL(0, "MechDef.RefreshChassis " + __instance.Description.Id);
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { Log.WL(1, "info is null"); return; }
        if (info.MeleeWeaponOverride == null) { return; }
        string meleeDef = info.MeleeWeaponOverride.DefaultWeapon;
        foreach(MechComponentRef cref in __instance.Inventory) {
          if (cref == null) { continue; }
          if (string.IsNullOrEmpty(cref.ComponentDefID)) { continue; }
          if (info.MeleeWeaponOverride.Components.ContainsKey(cref.ComponentDefID)) {
            meleeDef = info.MeleeWeaponOverride.Components[cref.ComponentDefID];
            break;
          }
        }
        if (string.IsNullOrEmpty(meleeDef)) { meleeDef = "Weapon_MeleeAttack"; }
        Log.WL(1, "meleeDef " + meleeDef);
        __instance.meleeWeaponRef = new MechComponentRef(meleeDef, "", ComponentType.Weapon, ChassisLocations.CenterTorso, -1, ComponentDamageLevel.Functional, false);
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
  }
}