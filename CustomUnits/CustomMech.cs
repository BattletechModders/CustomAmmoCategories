using BattleTech;
using BattleTech.Data;
using CustAmmoCategories;
using Harmony;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MoveMultiplier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MoveMultiplier {
    public static bool Prefix(Mech __instance, ref float __result) {
      try {
        //if (__instance.FakeVehicle()) { __result = 1f; return false; }
        if (__instance is CustomMech custMech) { return false; }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
    public static void Postfix(Mech __instance, ref float __result) {
      if (__instance is CustomMech custMech) { __result = custMech._MoveMultiplierOverride ? custMech._MoveMultiplier : __result; }
    }
  }

  public class CustomMech : Mech, ICustomMech {
    public delegate void d_AbstractActor_EjectPilot(AbstractActor unit, string sourceID, int stackItemID, DeathMethod deathMethod, bool isSilent);
    private static d_AbstractActor_EjectPilot i_AbstractActor_EjectPilot = null;
    public delegate void d_AbstractActor_Init(AbstractActor unit, Vector3 position, float facing, bool checkEncounterCells);
    private static d_AbstractActor_Init i_AbstractActor_Init = null;
    public delegate void d_AbstractActor_ApplyBraced(AbstractActor unit);
    private static d_AbstractActor_ApplyBraced i_AbstractActor_ApplyBraced = null;
    public static void AbstractActor_Init(AbstractActor unit, Vector3 position, float facing, bool checkEncounterCells) {
      if (i_AbstractActor_Init == null) {
        MethodInfo method = typeof(AbstractActor).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
        var dm = new DynamicMethod("CACAbstractActor_Init", null, new Type[] { typeof(AbstractActor), typeof(Vector3), typeof(float), typeof(bool) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AbstractActor_Init = (d_AbstractActor_Init)dm.CreateDelegate(typeof(d_AbstractActor_Init));
      }
      if (i_AbstractActor_Init == null) { return; }
      i_AbstractActor_Init(unit, position, facing, checkEncounterCells);
    }
    public static void AbstractActor_ApplyBraced(AbstractActor unit) {
      if (i_AbstractActor_ApplyBraced == null) {
        MethodInfo method = typeof(AbstractActor).GetMethod("ApplyBraced", BindingFlags.Public | BindingFlags.Instance);
        var dm = new DynamicMethod("CACAbstractActor_ApplyBraced", null, new Type[] { typeof(AbstractActor) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AbstractActor_ApplyBraced = (d_AbstractActor_ApplyBraced)dm.CreateDelegate(typeof(d_AbstractActor_ApplyBraced));
      }
      if (i_AbstractActor_ApplyBraced == null) { return; }
      i_AbstractActor_ApplyBraced(unit);
    }
    public static void AbstractActor_EjectPilot(AbstractActor unit, string sourceID, int stackItemID, DeathMethod deathMethod, bool isSilent) {
      if (i_AbstractActor_EjectPilot == null) {
        MethodInfo method = typeof(AbstractActor).GetMethod("EjectPilot", BindingFlags.Public | BindingFlags.Instance);
        var dm = new DynamicMethod("CACAbstractActor_EjectPilot", null, new Type[] { typeof(AbstractActor), typeof(string), typeof(int), typeof(DeathMethod), typeof(bool) });
        var gen = dm.GetILGenerator();
        gen.Emit(OpCodes.Ldarg_0);
        gen.Emit(OpCodes.Ldarg_1);
        gen.Emit(OpCodes.Ldarg_2);
        gen.Emit(OpCodes.Ldarg_3);
        gen.Emit(OpCodes.Ldarg_S, 4);
        gen.Emit(OpCodes.Call, method);
        gen.Emit(OpCodes.Ret);
        i_AbstractActor_EjectPilot = (d_AbstractActor_EjectPilot)dm.CreateDelegate(typeof(d_AbstractActor_EjectPilot));
      }
      if (i_AbstractActor_EjectPilot == null) { return; }
      i_AbstractActor_EjectPilot(unit, sourceID, stackItemID, deathMethod, isSilent);
    }
    public CustomMechRepresentation custGameRep { get { return this.GameRep as CustomMechRepresentation; } }
    public CustomMech(MechDef mDef, PilotDef pilotDef, TagSet additionalTags, string UID, CombatGameState combat, string spawnerId, HeraldryDef customHeraldryDef)
      : base(mDef, pilotDef, additionalTags, UID, combat, spawnerId, customHeraldryDef) {

    }
    public virtual bool _MoveMultiplierOverride { get { return true; } }
    public virtual float _MoveMultiplier {
      get {
        float num = 0.0f;
        if (this.IsOverheated) {
          num += this.Combat.Constants.MoveConstants.OverheatedMovePenalty;
        }
        List<ChassisLocations> legsDamageLevels = new List<ChassisLocations>();
        legsDamageLevels.Add(ChassisLocations.LeftLeg);
        legsDamageLevels.Add(ChassisLocations.RightLeg);
        float blackMod = this.Combat.Constants.MoveConstants.LegDestroyedPenalty;
        float redMod = this.Combat.Constants.MoveConstants.LegDestroyedPenalty;
        float yellowMod = this.Combat.Constants.MoveConstants.LegDestroyedPenalty;
        UnitCustomInfo info = this.GetCustomInfo();
        if (info != null) {
          if (info.ArmsCountedAsLegs) {
            legsDamageLevels.Add(ChassisLocations.LeftArm);
            legsDamageLevels.Add(ChassisLocations.RightArm);
          }
          blackMod = info.LegDestroyedMovePenalty >= 0f ? info.LegDestroyedMovePenalty : blackMod;
          redMod = info.LegDamageRedMovePenalty >= 0f ? info.LegDamageRedMovePenalty : redMod;
          yellowMod = info.LegDamageYellowMovePenalty >= 0f ? info.LegDamageYellowMovePenalty : yellowMod;
        }
        foreach (ChassisLocations location in legsDamageLevels) {
          if (this.IsLocationDestroyed(location)) {
            num += blackMod;
          } else if (this.GetLocationDamageLevel(location) > LocationDamageLevel.Penalized) {
            num += redMod;
          } else {
            num += yellowMod;
          }
        }
        return Mathf.Max(this.Combat.Constants.MoveConstants.MinMoveSpeed, 1f - num);
      }
    }
    public override void ApplyBraced() {
      if (this.IsDead || this.IsOrWillBeProne || this.IsShutDown || this.isVehicle || this.isSquad) { return; };
      AbstractActor_ApplyBraced(this);
      this.BracedLastRound = true;
      this.IsEntrenched = true;
      this.ApplyInstabilityReduction(StabilityChangeSource.Bracing);
      if (this.GameRep != null) { return; }
      this.GameRep.TriggerMeleeTransition(true);
    }
    public override void EjectPilot(string sourceID, int stackItemID, DeathMethod deathMethod, bool isSilent) {
      AbstractActor_EjectPilot(this, sourceID, stackItemID, deathMethod, isSilent);
      WeaponHitInfo hitInfo = new WeaponHitInfo(0, 0, 0, 0, "EJECTED", this.GUID, 1, (float[])null, (float[])null, (float[])null, (bool[])null, (int[])null, (int[])null, (AttackImpactQuality[])null, new AttackDirection[1]
      {
        AttackDirection.FromFront
      }, (Vector3[])null, (string[])null, (int[])null);
      ArmorLocation crewArmor = ArmorLocation.Head;
      ChassisLocations crewLocation = ChassisLocations.Head;
      UnitCustomInfo info = this.GetCustomInfo();
      if (info != null) {
        crewArmor = (ArmorLocation)info.MechVehicleCrewLocation;
        crewLocation = info.MechVehicleCrewLocation;
      }
      this.statCollection.ModifyStat<float>(sourceID, stackItemID, this.GetStringForArmorLocation(crewArmor), StatCollection.StatOperation.Set, 0.0f);
      this.statCollection.ModifyStat<float>(sourceID, stackItemID, this.GetStringForStructureLocation(crewLocation), StatCollection.StatOperation.Set, 0.0f);
      foreach (MechComponent allComponent in this.allComponents) {
        if (allComponent.Location == (int)crewLocation) {
          allComponent.DamageComponent(hitInfo, ComponentDamageLevel.Destroyed, false);
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("====@@@ Component Destroyed: {0}", (object)allComponent.Name));
        }
      }
      this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(sourceID, this.GUID, "COCKPIT DESTROYED", FloatieMessage.MessageNature.ComponentDestroyed));
    }
    public virtual void _InitGameRep(Transform parentTransform) {
      Log.TWL(0, "CustomMech._InitGameRep:" + this.MechDef.Chassis.PrefabIdentifier);
      try {
        string prefabIdentifier = this.MechDef.Chassis.PrefabIdentifier;
        if (AbstractActor.initLogger.IsLogEnabled) { AbstractActor.initLogger.Log((object)("InitGameRep Loading this -" + prefabIdentifier)); }
        GameObject gameObject = this.Combat.DataManager.PooledInstantiate_CustomMechRep_Battle(prefabIdentifier, this.MechDef.Chassis, true, true, true);
        if (gameObject == null) {
          throw new Exception(prefabIdentifier + " fail to load. Chassis:" + this.MechDef.ChassisID);
        }
        MechRepresentation mechRep = gameObject.GetComponent<MechRepresentation>();
        if (mechRep == null) {
          throw new Exception(prefabIdentifier + " is not a mech prefab. Chassis:" + this.MechDef.ChassisID);
        }
        CustomMechRepresentation custMechRepLoc = gameObject.GetComponent<CustomMechRepresentation>();
        if (custMechRepLoc == null) {
          throw new Exception(prefabIdentifier + " CustomMech can only operate CustomMechRepresentation");
        }
        Log.WL(1, "current game representation:" + (mechRep == null ? "null" : mechRep.name));
        this._gameRep = (GameRepresentation)mechRep;
        this.custGameRep.Init(this, parentTransform, false);
        if (parentTransform == null) {
          this.custGameRep.gameObject.transform.position = this.currentPosition;
          this.custGameRep.gameObject.transform.rotation = this.currentRotation;
        }
        this.InitWeapons();
        bool flag1 = this.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
        bool flag2 = this.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
        if (flag1 | flag2) {
          SkinnedMeshRenderer[] componentsInChildren = this.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
          for (int index = 0; index < componentsInChildren.Length; ++index) {
            if (flag1)
              componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderUnfinishedMaterial;
            if (flag2)
              componentsInChildren[index].sharedMaterial = Traverse.Create(this.Combat.DataManager).Property<TextureManager>("TextureManager").Value.PlaceholderImpostorMaterial;
          }
        }
        this.custGameRep.GatherColliders();
        //this.custGameRep.CustomPostInit();
        this.GameRep.RefreshEdgeCache();
        this.GameRep.FadeIn(1f);
        if (this.IsDead || !this.Combat.IsLoadingFromSave) { return; }
        if (this.AuraComponents != null) {
          foreach (MechComponent auraComponent in this.AuraComponents) {
            for (int index = 0; index < auraComponent.componentDef.statusEffects.Length; ++index) {
              if (auraComponent.componentDef.statusEffects[index].targetingData.auraEffectType == AuraEffectType.ECM_GHOST) {
                this.GameRep.PlayVFXAt(this.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECM_loop", true, Vector3.zero, false, -1f);
                this.GameRep.PlayVFXAt(this.GameRep.thisTransform, Vector3.zero, "vfxPrfPrtl_ECMcarrierAura_loop", true, Vector3.zero, false, -1f);
                return;
              }
            }
          }
        }
        if (this.VFXDataFromLoad != null) {
          foreach (VFXEffect.StoredVFXEffectData storedVfxEffectData in this.VFXDataFromLoad)
            this.GameRep.PlayVFXAt(this.GameRep.GetVFXTransform(storedVfxEffectData.hitLocation), storedVfxEffectData.hitPos, storedVfxEffectData.vfxName, storedVfxEffectData.isAttached, storedVfxEffectData.lookatPos, storedVfxEffectData.isOneShot, storedVfxEffectData.duration);
        }
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
    }
    public virtual void InitWeapons() {
      List<ComponentRepresentationInfo> componentsToInit = new List<ComponentRepresentationInfo>();
      foreach (MechComponent allComponent in this.allComponents) {
        if (allComponent.componentType != ComponentType.Weapon) {
          componentsToInit.Add(new ComponentRepresentationInfo(allComponent, allComponent.mechComponentRef.MountedLocation, allComponent.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Component));
        }
      }
      foreach (Weapon weapon in this.Weapons) {
        componentsToInit.Add(new ComponentRepresentationInfo(weapon, weapon.mechComponentRef.MountedLocation, weapon.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Weapon));
      }
      foreach (MechComponent supportComponent in this.supportComponents) {
        if (supportComponent is Weapon weapon) {
          componentsToInit.Add(new ComponentRepresentationInfo(weapon, weapon.mechComponentRef.MountedLocation, weapon.mechComponentRef.MountedLocation, ComponentRepresentationInfo.RepType.Support));
        }
      }
      this.custGameRep.InitWeapons(componentsToInit, this.LogDisplayName);
      if (!this.MeleeWeapon.baseComponentRef.hasPrefabName) {
        this.MeleeWeapon.baseComponentRef.prefabName = "chrPrfWeap_generic_melee";
        this.MeleeWeapon.baseComponentRef.hasPrefabName = true;
      }
      this.MeleeWeapon.InitGameRep(this.MeleeWeapon.baseComponentRef.prefabName, this.GetAttachTransform(this.MeleeWeapon.mechComponentRef.MountedLocation), this.LogDisplayName);
      if (!this.DFAWeapon.mechComponentRef.hasPrefabName) {
        this.DFAWeapon.mechComponentRef.prefabName = "chrPrfWeap_generic_melee";
        this.DFAWeapon.mechComponentRef.hasPrefabName = true;
      }
      this.DFAWeapon.InitGameRep(this.DFAWeapon.mechComponentRef.prefabName, this.GetAttachTransform(this.DFAWeapon.mechComponentRef.MountedLocation), this.LogDisplayName);
    }
    private static MethodInfo Mech_InitGameRep = null;
    private static Patches Mech_InitGameRep_patches = null;
    public virtual void MechInitGameRep_prefixes(Transform parentTransform) {
      Log.TWL(0, "Mech.InitGameRep.prefixes");
      if (Mech_InitGameRep == null) { Mech_InitGameRep = typeof(Mech).GetMethod("InitGameRep"); }
      if (Mech_InitGameRep == null) {
        Log.WL(1, "Can't find Mech.InitGameRep");
        return;
      }
      if (Mech_InitGameRep_patches == null) {
        Mech_InitGameRep_patches = Core.HarmonyInstance.GetPatchInfo(Mech_InitGameRep);
      }
      if (Mech_InitGameRep_patches == null) {
        Log.WL(1, "Mech.InitGameRep has no patches");
        return;
      }
      foreach (Patch patch in Mech_InitGameRep_patches.Prefixes) {
        Log.WL(1, patch.owner + ":" + patch.patch.Name);
        try {
          List<object> methodParams = new List<object>();
          foreach (var param in patch.patch.GetParameters()) {
            if (param.Name == "__instance") { methodParams.Add(this); }
            if (param.Name == "parentTransform") { methodParams.Add(parentTransform); }
            if (param.Name.StartsWith("___")) { methodParams.Add(Traverse.Create(this).Field(param.Name.Substring(3)).GetValue()); }
            Log.WL(2, param.Name + " is ref:" + param.GetType().IsByRef);
          }
          patch.patch.Invoke(null, methodParams.ToArray());
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
    }
    public virtual void MechInitGameRep_postfixes(Transform parentTransform) {
      Log.TWL(0, "Mech.InitGameRep.postfixes");
      if (Mech_InitGameRep == null) { Mech_InitGameRep = typeof(Mech).GetMethod("InitGameRep"); }
      if (Mech_InitGameRep == null) {
        Log.WL(1, "Can't find Mech.InitGameRep");
        return;
      }
      if (Mech_InitGameRep_patches == null) {
        Mech_InitGameRep_patches = Core.HarmonyInstance.GetPatchInfo(Mech_InitGameRep);
      }
      if (Mech_InitGameRep_patches == null) {
        Log.WL(1, "Mech.InitGameRep has no patches");
        return;
      }
      foreach (Patch patch in Mech_InitGameRep_patches.Postfixes) {
        Log.WL(1, patch.owner + ":" + patch.patch.Name);
        try {
          List<object> methodParams = new List<object>();
          foreach (var param in patch.patch.GetParameters()) {
            if (param.Name == "__instance") { methodParams.Add(this); }
            if (param.Name == "parentTransform") { methodParams.Add(parentTransform); }
            if (param.Name.StartsWith("___")) { methodParams.Add(Traverse.Create(this).Field(param.Name.Substring(3)).GetValue()); }
            Log.WL(2, param.Name + " is ref:" + param.GetType().IsByRef);
          }
          patch.patch.Invoke(null, methodParams.ToArray());
        } catch (Exception e) {
          Log.TWL(0, e.ToString(), true);
        }
      }
    }
    public static bool InitGameRepStatic(Mech __instance, Transform parentTransform) {
      Log.TWL(0, "CustomMech.InitGameRepStatic " + __instance.MechDef.Description.Id + " " + __instance.GetType().Name);
      try {
        if (__instance is CustomMech custMech) {
          custMech._InitGameRep(parentTransform);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.TWL(0, e.ToString(), true);
      }
      return true;
    }
    public override void InitGameRep(Transform parentTransform) {
      base.InitGameRep(parentTransform);
      //this.MechInitGameRep_prefixes(parentTransform);
      //this._InitGameRep(parentTransform);
      this.custGameRep.HeightController.ForceHeight(this.FlyingHeight());
      //this.MechInitGameRep_postfixes(parentTransform);
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attackPosition, this));
      Thread.CurrentThread.pushActor(this);
      int result = (int)(hitTable != null ? HitLocation.GetHitLocation(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier) : ArmorLocation.None);
      Thread.CurrentThread.clearActor();
      return result;
    }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      List<int> result = this.Combat.HitLocation.GetPossibleHitLocations(attacker, this);
      if (this.CanBeHeadShot == false) { result.Remove((int)ArmorLocation.Head); }
      return result;
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      return (int)this.Combat.HitLocation.GetAdjacentHitLocation(attackPosition, this, randomRoll, (ArmorLocation)previousHitLocation, originalMultiplier, adjacentMultiplier, ArmorLocation.None, 0.0f);
    }

    public virtual HashSet<ArmorLocation> GetDFASelfDamageLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }

    public virtual HashSet<ArmorLocation> GetLandmineDamageArmorLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.LeftLeg, ArmorLocation.RightLeg };
    }

    public virtual HashSet<ArmorLocation> GetBurnDamageArmorLocations() {
      return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear,
          ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear, ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear,
          ArmorLocation.RightArm, ArmorLocation.LeftArm, ArmorLocation.LeftLeg, ArmorLocation.RightLeg
      };
    }

    public virtual Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      Thread.CurrentThread.pushActor(this);
      Thread.CurrentThread.SetFlag("CallOriginal_GetMechHitTable");
      Dictionary<ArmorLocation, int> result = new Dictionary<ArmorLocation, int>(this.Combat.HitLocation.GetMechHitTable(from));
      Thread.CurrentThread.ClearFlag("CallOriginal_GetMechHitTable");
      Thread.CurrentThread.clearActor();
      if (!this.CanBeHeadShot && result.ContainsKey(ArmorLocation.Head)) result.Remove(ArmorLocation.Head);
      return result;
    }

    public virtual Dictionary<int, float> GetAOESpreadArmorLocations() {
      return CustomAmmoCategories.NormMechHitLocations;
    }

    public virtual List<int> GetAOEPossibleHitLocations(Vector3 attackPos) {
      return this.Combat.HitLocation.GetPossibleHitLocations(attackPos, this);
    }
    public new virtual Text GetLongArmorLocation(ArmorLocation location) {
      Thread.CurrentThread.SetFlag("GetLongArmorLocation_CallNative");
      Text result = Mech.GetLongArmorLocation(location);
      Thread.CurrentThread.ClearFlag("GetLongArmorLocation_CallNative");
      return result;
    }
    public virtual ArmorLocation GetAdjacentLocations(ArmorLocation location) {
      return MechStructureRules.GetAdjacentLocations(location);
    }
    private static Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> GetClusterTable_cache = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
    public virtual Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      if (GetClusterTable_cache.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      ArmorLocation adjacentLocations = this.GetAdjacentLocations(originalLocation);
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (keyValuePair.Key != ArmorLocation.Head || !this.Combat.Constants.ToHit.ClusterChanceNeverClusterHead || originalLocation == ArmorLocation.Head) {
          if (keyValuePair.Key == ArmorLocation.Head && this.Combat.Constants.ToHit.ClusterChanceNeverMultiplyHead)
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
          else if (originalLocation == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
          else if ((adjacentLocations & keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceNonadjacentMultiplier));
        }
      }
      GetClusterTable_cache.Add(originalLocation, dictionary);
      return dictionary;
    }
    private static Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>> GetClusterHitTable_cache = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
    public virtual Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      if (GetClusterHitTable_cache.TryGetValue(from, out Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        GetClusterHitTable_cache.Add(from, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      return result;
    }
    public virtual bool isSquad { get { return false; } }
    public virtual bool isVehicle { get { return false; } }
    public virtual bool isQuad { get { return false; } }
  }

}