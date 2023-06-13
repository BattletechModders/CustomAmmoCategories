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
using CustAmmoCategories;
using CustomDeploy;
using HarmonyLib;
using HBS.Collections;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomUnits {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MoveMultiplier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MoveMultiplier {
    public static void Prefix(ref bool __runOriginal, Mech __instance, ref float __result) {
      try {
        if (!__runOriginal) { return; }
        //if (__instance.FakeVehicle()) { __result = 1f; return false; }
        if (__instance is CustomMech custMech) { __runOriginal = false; return; }
        //Log.TWL(0, "MoveMultiplier:"+__instance.PilotableActorDef.Description.Id+" not a custom mech");
        return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        return;
      }
    }
    public static void Postfix(Mech __instance, ref float __result) {
      if (__instance is CustomMech custMech) { __result = custMech._MoveMultiplierOverride ? custMech._MoveMultiplier : __result; }
    }
  }
  [HarmonyPatch(typeof(Pilot))]
  [HarmonyPatch("LogMechKillInflicted")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(string) })]
  public static class Pilot_LogMechKillInflicted {
    public static void Prefix(ref bool __runOriginal, Pilot __instance, int stackID, string sourceID) {
      try {
        if (!__runOriginal) { return; }
        ICustomMech custMech = Thread.CurrentThread.currentActor() as ICustomMech;
        if (custMech == null) { return; }
        if (custMech.isVehicle) {
          Log.Combat?.TWL(0, "Pilot.LogMechKillInflicted fake vehicle");
          __instance.LogOtherKillInflicted(stackID, sourceID);
          __runOriginal = false;
          return;
        } else {
          return;
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        Pilot.pilotErrorLog.LogException(e);
        return;
      }
    }
  }
  public class CustomLOSData {
    public Vector3[] sourcePositions { get; set; }
    public Vector3[] targetPositions { get; set; }
    public Vector3 highest { get; set; }
    public CustomLOSData(CustomMech custMech) {
      float HeightFix = 0f;
      Log.Combat?.TWL(0, "CustomLOSData "+custMech.MechDef.ChassisID);
      UnitCustomInfo info = custMech.MechDef.GetCustomInfo();
      if (info != null) {
        HeightFix = info.FlyingHeight;
      }
      sourcePositions = new Vector3[custMech.MechDef.Chassis.LOSSourcePositions.Length];
      targetPositions = new Vector3[custMech.MechDef.Chassis.LOSTargetPositions.Length];
      Vector3? h = null;
      float mechScaleMultiplier = custMech.Combat.Constants.CombatValueMultipliers.TEST_MechScaleMultiplier;
      for (int t = 0; t < custMech.MechDef.Chassis.LOSSourcePositions.Length; ++t) {
        FakeVector3 srcPos = custMech.MechDef.Chassis.LOSSourcePositions[t];
        Log.Combat?.WL(1, "");
        Vector3 pos = new Vector3(srcPos.x * mechScaleMultiplier, srcPos.y * mechScaleMultiplier, srcPos.z * mechScaleMultiplier);
        if (HeightFix > Core.Epsilon) {
          if (Mathf.Abs(pos.y - HeightFix) < 5f) { pos.y -= HeightFix; }
        }
        sourcePositions[t] = pos;
        Log.Combat?.WL(1, "sourcePositions["+t+"] = " + sourcePositions[t]);
      }
      for (int t = 0; t < custMech.MechDef.Chassis.LOSTargetPositions.Length; ++t) {
        FakeVector3 srcPos = custMech.MechDef.Chassis.LOSTargetPositions[t];
        Vector3 pos = new Vector3(srcPos.x * mechScaleMultiplier, srcPos.y * mechScaleMultiplier, srcPos.z * mechScaleMultiplier);
        if (HeightFix > Core.Epsilon) {
          if (Mathf.Abs(pos.y - HeightFix) < 5f) { pos.y -= HeightFix; }
        }
        targetPositions[t] = pos;
        Log.Combat?.WL(1, "targetPositions[" + t + "] = " + targetPositions[t]);
      }
      for (int t = 0; t < sourcePositions.Length; ++t) {
        if (h.HasValue == false) { h = sourcePositions[t]; continue; }
        if (h.Value.y < sourcePositions[t].y) { h = sourcePositions[t]; }
      }
      highest = h.HasValue ? h.Value : Vector3.zero;
      Log.Combat?.WL(1, "highest = " + highest);
    }
    public void ApplyScale(Vector3 scale) {
      Log.Combat?.TWL(0, "CustomLOSData.ApplyScale");
      for (int t = 0; t < sourcePositions.Length; ++t) {
        sourcePositions[t] = Vector3.Scale(sourcePositions[t], scale);
        Log.Combat?.WL(1, "sourcePositions[" + t + "] = " + sourcePositions[t]);
      }
      for (int t = 0; t < targetPositions.Length; ++t) {
        targetPositions[t] = Vector3.Scale(targetPositions[t], scale);
        Log.Combat?.WL(1, "targetPositions[" + t + "] = " + targetPositions[t]);
      }
      highest = Vector3.Scale(highest, scale);
      Log.Combat?.WL(1, "highest = " + highest);
    }
  }
  public class CustomMech : Mech, ICustomMech {
    public delegate void d_AbstractActor_EjectPilot(AbstractActor unit, string sourceID, int stackItemID, DeathMethod deathMethod, bool isSilent);
    private static d_AbstractActor_EjectPilot i_AbstractActor_EjectPilot = null;
    public delegate void d_AbstractActor_Init(AbstractActor unit, Vector3 position, float facing, bool checkEncounterCells);
    private static d_AbstractActor_Init i_AbstractActor_Init = null;
    public delegate void d_AbstractActor_ApplyBraced(AbstractActor unit);
    private static d_AbstractActor_ApplyBraced i_AbstractActor_ApplyBraced = null;
    public CustomLOSData custLosData { get; set; }
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
    public void ShowFloatie(string sourceGuid, ArmorLocation location, FloatieMessage.MessageNature nature, string dmgText, float fontSize) {
      if (this.GameRep == null) { return; }
      Vector3 vector3 = this.GameRep.GetHitPosition((int)location) + UnityEngine.Random.insideUnitSphere * 5f;
      this.Combat.MessageCenter.PublishMessage(new FloatieMessage(sourceGuid, this.GUID, dmgText, fontSize, nature, vector3.x, vector3.y, vector3.z));
    }

    protected override void InitStats() {
      custLosData = new CustomLOSData(this);
      custLosData.ApplyScale(MechResizer.SizeMultiplier.Get(this.MechDef));
      base.InitStats();
      UpdateLOSHeight(this.FlyingHeight());
    }
    public override void AddToTeam(Team team) {
      try {
        base.AddToTeam(team);
        string no_biome_tag = "NoBiome_" + this.Combat.ActiveContract.ContractBiome.ToString();
        bool immobilize = false;
        if (this.Combat.LocalPlayerTeam != team) {
          immobilize = this.MechDef.MechTags.Contains(no_biome_tag);
        }
        if (immobilize == false) {
          foreach (MechComponent component in this.allComponents) {
            if (component == null) { continue; }
            if (component.componentDef == null) { continue; }
            if (component.componentDef.ComponentTags.Contains(no_biome_tag)) { immobilize = true; break; }
          }
        }
        if (immobilize) {
          this.FlyingHeight(0f);
          UpdateLOSHeight(this.FlyingHeight());
          if (this.custGameRep != null) {
            this.custGameRep.SetVisualHeight(0f);
            if (this.custGameRep.customRep != null) {
              this.custGameRep.customRep.InBattle = false;
            }
          }
          Statistic irbtmu_immobile_unit = this.StatCollection.GetStatistic("irbtmu_immobile_unit");
          if (irbtmu_immobile_unit == null) {
            irbtmu_immobile_unit = this.StatCollection.AddStatistic("irbtmu_immobile_unit",false);
          }
          irbtmu_immobile_unit.SetValue(true);
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.logger.LogException(e);
      }
    }
    public virtual void UpdateLOSHeight(float height) {
      if (custLosData == null) { return; }
      Log.Combat?.TWL(0, "CustomMech.UpdateLOSHeight "+this.PilotableActorDef.ChassisID+" height:"+height);
      for(int t = 0; t < this.originalLOSSourcePositions.Length; ++t) {
        if (t >= custLosData.sourcePositions.Length) { break; }
        this.originalLOSSourcePositions[t] = custLosData.sourcePositions[t] + Vector3.up * height;
        Log.Combat?.WL(1, "originalLOSSourcePositions["+t+"]"+ custLosData.sourcePositions[t]+"=>"+ this.originalLOSSourcePositions[t]);
      }
      for (int t = 0; t < this.originalLOSTargetPositions.Length; ++t) {
        if (t >= custLosData.targetPositions.Length) { break; }
        this.originalLOSTargetPositions[t] = custLosData.targetPositions[t] + Vector3.up * height;
        Log.Combat?.WL(1, "originalLOSTargetPositions[" + t + "]" + custLosData.targetPositions[t] + "=>" + this.originalLOSTargetPositions[t]);
      }
      this.HighestLOSPosition = custLosData.highest + Vector3.up * height;
      Log.Combat?.WL(1, "HighestLOSPosition" + custLosData.highest + "=>" + this.HighestLOSPosition);
      this.UpdateLOSPositions();
    }
    public virtual bool _MoveMultiplierOverride { get { return true; } }
    public virtual float _MoveMultiplier {
      get {
        float num = 0.0f;
        StringBuilder log = new StringBuilder();
        if (this.IsOverheated) {
          num += this.Combat.Constants.MoveConstants.OverheatedMovePenalty;
          log.Append(" IsOverheated:" + this.IsOverheated+" penalty:"+ num);
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
            //log.Append(" location:" + location + " destroyed.penalty:" + num);
          } else if (this.GetLocationDamageLevel(location) > LocationDamageLevel.Penalized) {
            num += redMod;
            //log.Append(" location:" + location + " damaged.penalty:" + num);
          } else if(this.GetLocationDamageLevel(location) > LocationDamageLevel.Functional) { 
            num += yellowMod;
            //log.Append(" location:" + location + " light damage.penalty:" + num);
          }
        }
        float result = Mathf.Max(this.Combat.Constants.MoveConstants.MinMoveSpeed, 1f - num);
        //Log.TWL(0, "MoveMultiplier:"+this.PilotableActorDef.Description.Id+" "+result+" "+log.ToString());
        return result;
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
      if (this.NukeCrewLocationOnEject() == false) { return; }
      var crewArmors = this.CrewLocationArmor();
      ChassisLocations crewLocation = this.CrewLocationChassis();
      try {
        foreach (var crewArmor in crewArmors) {
          string armorStatName = this.GetStringForArmorLocation(crewArmor);
          if (string.IsNullOrEmpty(armorStatName) == false) {
            Statistic stat = this.statCollection.GetStatistic(armorStatName);
            if (stat != null) {
              this.statCollection.ModifyStat<float>(sourceID, stackItemID, armorStatName, StatCollection.StatOperation.Set, 0.0f);
            }
          }
        }
        string statName = this.GetStringForStructureLocation(crewLocation);
        if (string.IsNullOrEmpty(statName) == false) {
          Statistic stat = this.statCollection.GetStatistic(statName);
          if (stat != null) {
            this.statCollection.ModifyStat<float>(sourceID, stackItemID, statName, StatCollection.StatOperation.Set, 0.0f);
          }
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.logger.LogException(e);
      }
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
      Log.Combat?.TWL(0, "CustomMech._InitGameRep:" + this.MechDef.Chassis.PrefabIdentifier);
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
        Log.Combat?.WL(1, "current game representation:" + (mechRep == null ? "null" : mechRep.name));
        this._gameRep = (GameRepresentation)mechRep;
        this.custGameRep.Init(this, parentTransform, false);
        if (parentTransform == null) {
          this.custGameRep.gameObject.transform.position = this.currentPosition;
          this.custGameRep.gameObject.transform.rotation = this.currentRotation;
        }
        this.InitWeapons();
        if (this.custGameRep.customRep != null) {
          this.custGameRep.customRep.InBattle = true;
        }
        bool flag1 = this.MechDef.MechTags.Contains("PlaceholderUnfinishedMaterial");
        bool flag2 = this.MechDef.MechTags.Contains("PlaceholderImpostorMaterial");
        if (flag1 | flag2) {
          SkinnedMeshRenderer[] componentsInChildren = this.GameRep.GetComponentsInChildren<SkinnedMeshRenderer>(true);
          for (int index = 0; index < componentsInChildren.Length; ++index) {
            if (flag1)
              componentsInChildren[index].sharedMaterial = this.Combat.DataManager.TextureManager.PlaceholderUnfinishedMaterial;
            if (flag2)
              componentsInChildren[index].sharedMaterial = this.Combat.DataManager.TextureManager.PlaceholderImpostorMaterial;
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
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.initLogger.LogException(e);
      }
    }
    public virtual void InitWeapons() {
      try {
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
      }catch(Exception e) {
        Log.Combat?.TWL(0, e.ToString(),true);
        AbstractActor.initLogger.LogException(e);
      }
    }
    public virtual void _NukeStructureLocation(WeaponHitInfo hitInfo, int hitLoc, ChassisLocations location, Vector3 attackDirection, DamageType damageType) {
      Log.Combat?.WL(0, $"CustomMech.NukeStructureLocation {this.PilotableActorDef.ChassisID} location:{location} hitLoc:{(ArmorLocation)hitLoc}");
      try {
        if (AbstractActor.attackLogger.IsLogEnabled)
          AbstractActor.attackLogger.Log($"{this.PilotableActorDef.ChassisID} SEQ:{hitInfo.stackItemUID}: WEAP:{hitInfo.attackWeaponIndex} HITLOC: {hitLoc} ({location}) Location destroyed!");
        if (AbstractActor.damageLogger.IsLogEnabled)
          AbstractActor.damageLogger.Log($"==== Location Destroyed: {this.PilotableActorDef.ChassisID} {location}");
        this.ApplyStructureStatDamage(location, this.GetCurrentStructure(location), hitInfo);
        try {
          this.OnLocationDestroyed_private(location, attackDirection, hitInfo, damageType);
        }catch(Exception e) {
          Log.ECombat?.TWL(0, e.ToString(), true);
          AbstractActor.damageLogger.LogException(e);
        }
        ArmorLocation fromChassisLocation = MechStructureRules.GetArmorFromChassisLocation(location);
        foreach (ArmorLocation location1 in Enum.GetValues(typeof(ArmorLocation))) {
          if (location1 > ArmorLocation.None && location1 < ArmorLocation.Invalid && (location1 & fromChassisLocation) != ArmorLocation.None)
            this.ApplyArmorStatDamage(location1, this.GetCurrentArmor(location1), hitInfo);
        }
        ChassisLocations dependentLocation = MechStructureRules.GetDependentLocation(location);
        Log.Combat?.WL(1, $"dependentLocation:{dependentLocation} isDestroyed:{(dependentLocation == ChassisLocations.None?"None":this.IsLocationDestroyed(dependentLocation).ToString())}");
        if (dependentLocation == ChassisLocations.None || this.IsLocationDestroyed(dependentLocation))
          return;
        this.NukeStructureLocation(hitInfo, 0, dependentLocation, Vector3.one, damageType);
      } catch (Exception e) {
        Log.ECombat?.TWL(0,e.ToString(),true);
        AbstractActor.damageLogger.LogException(e);
      }
    }
    //private static MethodInfo Mech_InitGameRep = null;
    //private static Patches Mech_InitGameRep_patches = null;
    //public virtual void MechInitGameRep_prefixes(Transform parentTransform) {
    //  Log.TWL(0, "Mech.InitGameRep.prefixes");
    //  if (Mech_InitGameRep == null) { Mech_InitGameRep = typeof(Mech).GetMethod("InitGameRep"); }
    //  if (Mech_InitGameRep == null) {
    //    Log.WL(1, "Can't find Mech.InitGameRep");
    //    return;
    //  }
    //  if (Mech_InitGameRep_patches == null) {
    //    Mech_InitGameRep_patches = Core.HarmonyInstance.GetPatchInfo(Mech_InitGameRep);
    //  }
    //  if (Mech_InitGameRep_patches == null) {
    //    Log.WL(1, "Mech.InitGameRep has no patches");
    //    return;
    //  }
    //  foreach (Patch patch in Mech_InitGameRep_patches.Prefixes) {
    //    Log.WL(1, patch.owner + ":" + patch.patch.Name);
    //    try {
    //      List<object> methodParams = new List<object>();
    //      foreach (var param in patch.patch.GetParameters()) {
    //        if (param.Name == "__instance") { methodParams.Add(this); }
    //        if (param.Name == "parentTransform") { methodParams.Add(parentTransform); }
    //        if (param.Name.StartsWith("___")) { methodParams.Add(Traverse.Create(this).Field(param.Name.Substring(3)).GetValue()); }
    //        Log.WL(2, param.Name + " is ref:" + param.GetType().IsByRef);
    //      }
    //      patch.patch.Invoke(null, methodParams.ToArray());
    //    } catch (Exception e) {
    //      Log.TWL(0, e.ToString(), true);
    //    }
    //  }
    //}
    //public virtual void MechInitGameRep_postfixes(Transform parentTransform) {
    //  Log.TWL(0, "Mech.InitGameRep.postfixes");
    //  if (Mech_InitGameRep == null) { Mech_InitGameRep = typeof(Mech).GetMethod("InitGameRep"); }
    //  if (Mech_InitGameRep == null) {
    //    Log.WL(1, "Can't find Mech.InitGameRep");
    //    return;
    //  }
    //  if (Mech_InitGameRep_patches == null) {
    //    Mech_InitGameRep_patches = Core.HarmonyInstance.GetPatchInfo(Mech_InitGameRep);
    //  }
    //  if (Mech_InitGameRep_patches == null) {
    //    Log.WL(1, "Mech.InitGameRep has no patches");
    //    return;
    //  }
    //  foreach (Patch patch in Mech_InitGameRep_patches.Postfixes) {
    //    Log.WL(1, patch.owner + ":" + patch.patch.Name);
    //    try {
    //      List<object> methodParams = new List<object>();
    //      foreach (var param in patch.patch.GetParameters()) {
    //        if (param.Name == "__instance") { methodParams.Add(this); }
    //        if (param.Name == "parentTransform") { methodParams.Add(parentTransform); }
    //        if (param.Name.StartsWith("___")) { methodParams.Add(Traverse.Create(this).Field(param.Name.Substring(3)).GetValue()); }
    //        Log.WL(2, param.Name + " is ref:" + param.GetType().IsByRef);
    //      }
    //      patch.patch.Invoke(null, methodParams.ToArray());
    //    } catch (Exception e) {
    //      Log.TWL(0, e.ToString(), true);
    //    }
    //  }
    //}
    public static HashSet<ArmorLocation> GetArmorFromChassisLocation(ChassisLocations location) {
      switch (location) {
        case ChassisLocations.LeftTorso: return new HashSet<ArmorLocation>() { ArmorLocation.LeftTorso, ArmorLocation.LeftTorsoRear };
        case ChassisLocations.CenterTorso: return new HashSet<ArmorLocation>() { ArmorLocation.CenterTorso, ArmorLocation.CenterTorsoRear };
        case ChassisLocations.RightTorso: return new HashSet<ArmorLocation>() { ArmorLocation.RightTorso, ArmorLocation.RightTorsoRear };
        default: return new HashSet<ArmorLocation>() { (ArmorLocation)location };
      }
    }
    public virtual void _ApplyArmorStatDamage(ArmorLocation location, float damage, WeaponHitInfo hitInfo) {
      this.statCollection.ModifyStat<float>(hitInfo.attackerId, hitInfo.stackItemUID, this.GetStringForArmorLocation(location), StatCollection.StatOperation.Float_Subtract, damage);
      this.OnArmorDamaged((int)location, hitInfo, damage);
      var specialLocations = this.CrewLocationArmor();
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(this.DefaultStructureDef); }
      //if ((info != null) && (info.customStructure.is_empty == false)) { specialLocations = new HashSet<ArmorLocation>() { info.customStructure.ClusterSpecialLocation }; }
      if (specialLocations.Contains(location)) {
        if(this.InjurePilotOnCrewLocationHit()) this.pilot.SetNeedsInjury(InjuryReason.HeadHit);
      }
    }
    public static bool InitGameRepStatic(Mech __instance, Transform parentTransform) {
      try {
        Log.Combat?.TWL(0, "CustomMech.InitGameRepStatic " + __instance.MechDef.Description.Id + " " + __instance.GetType().Name);
        if (__instance is CustomMech custMech) {
          custMech._InitGameRep(parentTransform);
          return false;
        }
        __instance.InitGameRepLocal(parentTransform);
        return false;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.initLogger.LogException(e);
      }
      return true;
    }
    public override void InitGameRep(Transform parentTransform) {
      try {
        foreach (var prefix in CustomMechHelper.InitGameRepPrefixes) { prefix(this,parentTransform); }
        this._InitGameRep(parentTransform);
        foreach (var postfix in CustomMechHelper.InitGameRepPostfixes) { postfix(this, parentTransform); }
        foreach(MonoBehaviour component in this.GameRep.gameObject.GetComponentsInChildren<MonoBehaviour>(true)) {
          if (component is IOnRepresentationInit onRepInit) { onRepInit.Init(this.GameRep.gameObject); }
        }
        this.custGameRep.SetVisualHeight(this.FlyingHeight());
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.initLogger.LogException(e);
      }
    }
    public override int GetHitLocation(AbstractActor attacker, Vector3 attackPosition, float hitLocationRoll, int calledShotLocation, float bonusMultiplier) {
      Dictionary<ArmorLocation, int> hitTable = new Dictionary<ArmorLocation, int>(this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attackPosition, this)));
      ArmorLocation specialLocation = ArmorLocation.Head;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(this.DefaultStructureDef); }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      if ((this.CanBeHeadShot == false) && (hitTable.ContainsKey(specialLocation))) { hitTable.Remove(specialLocation); }
      Thread.CurrentThread.pushActor(this);
      int result = (int)((hitTable.Count > 0) ? HitLocation.GetHitLocation(hitTable, hitLocationRoll, (ArmorLocation)calledShotLocation, bonusMultiplier) : ArmorLocation.None);
      Thread.CurrentThread.clearActor();
      Log.Combat?.TW(0, "CustomMech.GetHitLocation " + this.PilotableActorDef.ChassisID + " attacker:" + attacker.PilotableActorDef.ChassisID + " hitTable:");
      foreach (var ht in hitTable) {
        Log.Combat?.W(1, ht.Key + "=" + ht.Value);
      }
      Log.Combat?.WL(1, "result:" + ((ArmorLocation)result));
      return result;
    }
    public virtual string DefaultStructureDef { get { return "mech"; } }
    public override List<int> GetPossibleHitLocations(AbstractActor attacker) {
      Dictionary<ArmorLocation, int> unitHitTable = this.GetHitTable(this.IsProne ? AttackDirection.ToProne : this.Combat.HitLocation.GetAttackDirection(attacker.CurrentPosition, (ICombatant)this));
      if (unitHitTable == null)
        return (List<int>)null;
      HashSet<int> result = new HashSet<int>();
      foreach (var loc in unitHitTable) {
        if (loc.Value > 0)
          result.Add((int)loc.Key);
      }
      ArmorLocation specialLocation = ArmorLocation.Head;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) { info.customStructure = CustomStructureDef.Search(DefaultStructureDef); }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      if ((this.CanBeHeadShot == false) && (result.Contains((int)specialLocation))) { result.Remove((int)specialLocation); }
      return result.ToList();
    }
    public override int GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, int previousHitLocation, float originalMultiplier = 1f, float adjacentMultiplier = 1f) {
      return (int)this.GetAdjacentHitLocation(attackPosition, randomRoll, (ArmorLocation)previousHitLocation, originalMultiplier, adjacentMultiplier, ArmorLocation.None, 0.0f);
    }
    public virtual ArmorLocation GetAdjacentHitLocation(Vector3 attackPosition, float randomRoll, ArmorLocation previousHitLocation, float originalMultiplier, float adjacentMultiplier, ArmorLocation bonusLocation, float bonusChanceMultiplier) {
      AttackDirection attackDirection = this.Combat.HitLocation.GetAttackDirection(attackPosition, (ICombatant)this);
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTableCluster(attackDirection, previousHitLocation);
      if (hitTable == null)
        return ArmorLocation.None;
      if (originalMultiplier > 1.01f || adjacentMultiplier > 1.01f) {
        Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
        ArmorLocation adjacentLocations = this.GetAdjacentLocations(previousHitLocation);
        foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
          if (keyValuePair.Key == previousHitLocation)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)originalMultiplier));
          else if ((adjacentLocations | keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)adjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }
        hitTable = dictionary;
      }
      Thread.CurrentThread.pushActor(this);
      ArmorLocation result = HitLocation.GetHitLocation(hitTable, randomRoll, bonusLocation, bonusChanceMultiplier);
      Thread.CurrentThread.clearActor();
      Log.Combat?.TW(0, "CustomMech.GetAdjacentHitLocation " + this.PilotableActorDef.ChassisID + " hitTable:");
      foreach (var ht in hitTable) {
        Log.Combat?.W(1, ht.Key + "=" + ht.Value);
      }
      Log.Combat?.WL(1, "result:" + ((ArmorLocation)result));
      return result;
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
    protected Dictionary<string, Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>> GetHitTable_cache = new Dictionary<string, Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>>();
    public virtual Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from) {
      string specialHitTable = Thread.CurrentThread.peekFromStack<string>(CustomAmmoCategories.SPECIAL_HIT_TABLE_NAME);
      if (string.IsNullOrEmpty(specialHitTable)) { specialHitTable = "default"; }
      if(GetHitTable_cache.TryGetValue(specialHitTable, out var GetHitTable_cache_sp) == false) {
        GetHitTable_cache_sp = new Dictionary<AttackDirection, Dictionary<ArmorLocation, int>>();
        GetHitTable_cache.Add(specialHitTable, GetHitTable_cache_sp);
      }
      if (GetHitTable_cache_sp.TryGetValue(from, out var result)) { return result; }
      Log.Combat?.TWL(0,$"CustomMech.GetHitTable {this.PilotableActorDef.ChassisID} table name:{specialHitTable} attack direction:{from}");
      UnitCustomInfo info = this.GetCustomInfo();
      result = new Dictionary<ArmorLocation, int>();
      Dictionary<ArmorLocation, int> hittable = null;
      if (info == null) { goto call_native; }
      if (info.customStructure.is_empty) { info.customStructure = CustomStructureDef.Search(this.DefaultStructureDef); }
      if (info.customStructure.is_empty) { goto call_native; }
      CustomHitTableDef hittabledef = null;
      Log.Combat?.WL(1,$"searching hittable:{specialHitTable} in {info.customStructure.Id}");
      if (info.customStructure.tables.TryGetValue(specialHitTable, out hittabledef) == false) {
        hittabledef = null;
        var fallbackStructure = CustomStructureDef.Search(this.DefaultStructureDef);
        Log.Combat?.WL(1, $"searching hittable:{specialHitTable} in {fallbackStructure.Id}");
        if (fallbackStructure.tables.TryGetValue(specialHitTable, out hittabledef)) {
          Log.Combat?.WL(2, "found");
          goto process_hittable;
        }
        Log.Combat?.WL(1, $"searching hittable:default in {info.customStructure.Id}");
        if (info.customStructure.tables.TryGetValue("default", out hittabledef) == false) {
          hittabledef = null;
          Log.Combat?.WL(1, $"searching hittable:default in {fallbackStructure.Id}");
          if (fallbackStructure.tables.TryGetValue("default", out hittabledef) == false) {
            hittabledef = null;
            goto call_native;
          } else {
            Log.Combat?.WL(2, "found");
          }
        } else {
          Log.Combat?.WL(2, "found");
        }
      } else {
        Log.Combat?.WL(2, "found");
      }
    process_hittable:
      if (hittabledef == null) { goto call_native; }
      if(hittabledef.HitTable.TryGetValue(from, out hittable) == false) {
        goto call_native;
      }
      if (hittable == null) { goto call_native; }
      goto return_result;
    call_native:      
      Thread.CurrentThread.pushActor(this);
      Thread.CurrentThread.SetFlag("CallOriginal_GetMechHitTable");
      Log.Combat?.WL(1, $"fallback");
      hittable = this.Combat.HitLocation.GetMechHitTable(from);
      Thread.CurrentThread.ClearFlag("CallOriginal_GetMechHitTable");
      Thread.CurrentThread.clearActor();
    return_result:
      Log.Combat?.W(1, $"result: ");
      foreach (var loc in hittable) {
        LocationDef locationDef = this.MechDef.Chassis.GetLocationDef(MechStructureRules.GetChassisLocationFromArmorLocation(loc.Key));
        if ((locationDef.MaxArmor <= 0f) && (locationDef.InternalStructure <= 1f)) { continue; }
        result.Add(loc.Key, loc.Value);
        Log.Combat?.W(1, $"{loc.Key}:{loc.Value}");
      }
      Log.Combat?.WL(0,"");
      GetHitTable_cache_sp.Add(from, result);
      return result;
    }
    public static bool DamageLocation_Override(Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalArmorDamage, float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType, ref bool __result) {
      try {
        if (__instance is CustomMech custMech) {
          __result = custMech.DamageLocationCustom(originalHitLoc, hitInfo, aLoc, weapon, totalArmorDamage, directStructureDamage, hitIndex, impactQuality, damageType);
          return false;
        }
        return true;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.damageLogger.LogException(e);
      }
      return true;
    }
    public virtual bool DamageLocationCustom(int originalHitLoc,WeaponHitInfo hitInfo,ArmorLocation aLoc,Weapon weapon,float totalArmorDamage,float directStructureDamage,int hitIndex,AttackImpactQuality impactQuality,DamageType damageType) {
      try {
        Log.Combat?.TWL(0, "CustomMech.DamageLocationCustom "+this.MechDef.ChassisID+" location:"+aLoc);
        if (aLoc == ArmorLocation.None || aLoc == ArmorLocation.Invalid)
          return false;
        if (Mech.attackSequenceLogger.IsDebugEnabled)
          Mech.attackSequenceLogger.LogDebug((object)string.Format("[Mech.DamageLocation] GUID {4}, Group {3}, Weapon {0}, Hit Index {5}, Location {1}, Total Damage {2}", (object)hitInfo.attackWeaponIndex, (object)aLoc.ToString(), (object)totalArmorDamage, (object)hitInfo.attackGroupIndex, (object)this.GUID, (object)hitIndex));
        AttackDirector.AttackSequence attackSequence = this.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        if (AbstractActor.damageLogger.IsLogEnabled)
          AbstractActor.damageLogger.Log((object)string.Format("{0} takes {1} Damage to its {2} from {3} (ID {4})", (object)this.Description.Name, (object)totalArmorDamage, (object)aLoc.ToString(), (object)weapon.Name, (object)hitInfo.attackWeaponIndex));
        if (attackSequence != null) {
          attackSequence.FlagAttackDidDamage(this.GUID);
          this.Combat.MultiplayerGameVerification.RecordMechDamage(this.GUID, originalHitLoc, hitInfo, aLoc, weapon, totalArmorDamage, hitIndex, impactQuality);
        }
        float num1 = totalArmorDamage;
        float num2 = directStructureDamage;
        float currentArmor = this.GetCurrentArmor(aLoc);
        if ((double)currentArmor > 0.0) {
          float damage = Mathf.Min(totalArmorDamage, currentArmor);
          if (AbstractActor.attackLogger.IsLogEnabled)
            AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} HITLOC: {2} ({3}) Armor damage: {4}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)originalHitLoc, (object)aLoc.ToString(), (object)damage));
          if (AbstractActor.damageLogger.IsLogEnabled) {
            float num3 = currentArmor - damage;
            AbstractActor.damageLogger.Log((object)string.Format("==== Armor Damage: {0} / {1} || Now: {2}", (object)damage, (object)currentArmor, (object)num3));
          }
          this.ApplyArmorStatDamage(aLoc, damage, hitInfo);
          num1 = totalArmorDamage - damage;
        }
        if ((double)num1 <= 0.0 && (double)num2 <= 0.0) {
          this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.attackerId, this.GUID, totalArmorDamage, aLoc));
          return true;
        }
        ChassisLocations fromArmorLocation = MechStructureRules.GetChassisLocationFromArmorLocation(aLoc);
        Vector3 attackDirection = Vector3.one;
        if (this.GameRep != null && weapon.weaponRep != null) {
          Vector3 position = weapon.weaponRep.vfxTransforms[0].position;
          Vector3 vector3 = this.GameRep.GetVFXTransform((int)fromArmorLocation).position - position;
          vector3.Normalize();
          vector3.y = 0.5f;
          attackDirection = vector3 * totalArmorDamage;
        }
        float currentStructure = this.GetCurrentStructure(fromArmorLocation);
        if ((double)currentStructure > 0.0) {
          float damage1 = Mathf.Min(num1, currentStructure);
          float val2 = currentStructure - damage1;
          if (AbstractActor.attackLogger.IsLogEnabled)
            AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} HITLOC: {2} ({3}) Structure damage: {4}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)originalHitLoc, (object)fromArmorLocation.ToString(), (object)damage1));
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("==== Structure Damage: {0} / {1} || Now: {2}", (object)damage1, (object)currentStructure, (object)val2));
          this.ApplyStructureStatDamage(fromArmorLocation, damage1, hitInfo);
          num1 -= damage1;
          float damage2 = Math.Min(num2, val2);
          if (AbstractActor.attackLogger.IsLogEnabled)
            AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} HITLOC: {2} ({3}) Structure damage: {4}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)originalHitLoc, (object)fromArmorLocation.ToString(), (object)damage2));
          if (AbstractActor.damageLogger.IsLogEnabled)
            AbstractActor.damageLogger.Log((object)string.Format("==== Structure Damage: {0} / {1} || Now: {2}", (object)damage2, (object)val2, (object)(float)((double)val2 - (double)damage2)));
          this.ApplyStructureStatDamage(fromArmorLocation, damage2, hitInfo);
          num2 -= damage2;
          float num3 = val2 - damage2;
          if (this.IsLocationDestroyed(fromArmorLocation) && (double)num3 < (double)currentStructure)
            this.NukeStructureLocation(hitInfo, originalHitLoc, fromArmorLocation, attackDirection, damageType);
        } else if (this.IsDead && (double)num1 > 0.0 || (double)num2 > 0.0)
          this.ShowFloatie(hitInfo.attackerId, MechStructureRules.GetArmorFromChassisLocation(fromArmorLocation), FloatieMessage.MessageNature.StructureDamage, string.Format("{0}", (object)(int)Mathf.Max(1f, num1 + num2)), this.Combat.Constants.CombatUIConstants.floatieSizeMedium);
        this.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new TakeDamageMessage(hitInfo.attackerId, this.GUID, totalArmorDamage, aLoc));
        if ((double)num1 <= 0.0 && (double)num2 <= 0.0) { return true; }
        Thread.CurrentThread.pushActor(this);
        ArmorLocation passthroughLocation = MechStructureRules.GetPassthroughLocation(aLoc, hitInfo.attackDirections[hitIndex]);
        Thread.CurrentThread.clearActor();
        if (AbstractActor.attackLogger.IsLogEnabled)
          AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} HITLOC: {2} ({3}) Passing {4} damage through to {5}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)originalHitLoc, (object)fromArmorLocation.ToString(), (object)num1, (object)passthroughLocation.ToString()));
        if (AbstractActor.damageLogger.IsLogEnabled)
          AbstractActor.damageLogger.Log((object)string.Format("==== {0} Armor Destroyed: {1} Damage applied to {2}", (object)fromArmorLocation.ToString(), (object)num1, (object)passthroughLocation.ToString()));
        if(passthroughLocation == ArmorLocation.None || passthroughLocation == ArmorLocation.Invalid) {
          return false;
        }
        return this.DamageLocation_private(originalHitLoc, hitInfo, passthroughLocation, weapon, num1, num2, hitIndex, impactQuality, damageType);
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.damageLogger.LogException(e);
      }
      return false;
    }
    public virtual Dictionary<int, float> GetAOESpreadArmorLocations() {
      return CustomAmmoCategories.NormMechHitLocations;
    }
    public override void FlagForDeath(string reason, DeathMethod deathMethod,DamageType damageType,int location,int stackItemID,string attackerID,bool isSilent) {
      if (this._flaggedForDeath) { return; }
      Log.Combat?.TWL(0, $"CustomMech.FlagForDeath {this.PilotableActorDef.ChassisID} {reason} method:{deathMethod} dmgType:{damageType} location:{location}");
      if(deathMethod == DeathMethod.DespawnedEscaped) {
        Log.Combat?.WL(0, Environment.StackTrace);
      }
      Thread.CurrentThread.pushActor(this);
      base.FlagForDeath(reason, deathMethod, damageType, location, stackItemID, attackerID, isSilent);
      Thread.CurrentThread.clearActor();
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
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)){
        info.customStructure = CustomStructureDef.Search(this.DefaultStructureDef);
      }
      if ((info != null) && (info.customStructure.is_empty == false)) {
        if(info.customStructure.AdjacentLocations.TryGetValue(location, out var result)) {
          return result;
        }
      }
      return MechStructureRules.GetAdjacentLocations(location);
    }
    //private static Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>> GetClusterTable_cache = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
    public virtual Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable) {
      //if (GetClusterTable_cache.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) { return result; }
      ArmorLocation specialLocation = ArmorLocation.Head;
      UnitCustomInfo info = this.GetCustomInfo();
      if ((info != null) && (info.customStructure.is_empty)) {
        info.customStructure = CustomStructureDef.Search(this.DefaultStructureDef);
      }
      if ((info != null) && (info.customStructure.is_empty == false)) { specialLocation = info.customStructure.ClusterSpecialLocation; }
      ArmorLocation adjacentLocations = this.GetAdjacentLocations(originalLocation);
      Dictionary<ArmorLocation, int> dictionary = new Dictionary<ArmorLocation, int>();
      foreach (KeyValuePair<ArmorLocation, int> keyValuePair in hitTable) {
        if (keyValuePair.Key != specialLocation || !this.Combat.Constants.ToHit.ClusterChanceNeverClusterHead || originalLocation == specialLocation) {
          if (keyValuePair.Key == specialLocation && this.Combat.Constants.ToHit.ClusterChanceNeverMultiplyHead)
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
          else if (originalLocation == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceOriginalLocationMultiplier));
          else if ((adjacentLocations & keyValuePair.Key) == keyValuePair.Key)
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceAdjacentMultiplier));
          else
            dictionary.Add(keyValuePair.Key, (int)((double)keyValuePair.Value * (double)this.Combat.Constants.ToHit.ClusterChanceNonadjacentMultiplier));
        }
      }
      //GetClusterTable_cache.Add(originalLocation, dictionary);
      return dictionary;
    }
    protected Dictionary<string, Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>> GetClusterHitTable_cache = new Dictionary<string, Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>>();
    public virtual Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation) {
      string specialHitTable = Thread.CurrentThread.peekFromStack<string>(CustomAmmoCategories.SPECIAL_HIT_TABLE_NAME);
      if (string.IsNullOrEmpty(specialHitTable)) { specialHitTable = "default"; }
      if (GetClusterHitTable_cache.TryGetValue(specialHitTable, out var GetClusterHitTable_cache_sp) == false) {
        GetClusterHitTable_cache_sp = new Dictionary<AttackDirection, Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>>();
        GetClusterHitTable_cache.Add(specialHitTable, GetClusterHitTable_cache_sp);
      }
      if (GetClusterHitTable_cache_sp.TryGetValue(from, out var clusterTables) == false) {
        clusterTables = new Dictionary<ArmorLocation, Dictionary<ArmorLocation, int>>();
        //GetClusterHitTable_cache.Add(from, clusterTables);
      }
      if (clusterTables.TryGetValue(originalLocation, out Dictionary<ArmorLocation, int> result)) {
        return result;
      }
      Dictionary<ArmorLocation, int> hitTable = this.GetHitTable(from);
      CustomAmmoCategoriesLog.Log.AIM?.TW(0,$"Generating cluster {specialHitTable} table {from} location:{originalLocation} based on:");
      foreach (var hit in hitTable) { CustomAmmoCategoriesLog.Log.AIM?.W(1, $"{hit.Key}:{hit.Value}"); }
      CustomAmmoCategoriesLog.Log.AIM?.WL(0, "");
      result = GetClusterTable(originalLocation, hitTable);
      clusterTables.Add(originalLocation, result);
      if (GetClusterHitTable_cache_sp.ContainsKey(from) == false) {
        CustomAmmoCategoriesLog.Log.AIM?.WL(1, $"adding to cache as {from}");
        GetClusterHitTable_cache_sp.Add(from, clusterTables);
      }
      this.DumpClusterTableCache(CustomAmmoCategoriesLog.Log.AIM);
      return result;
    }
    public virtual void DumpClusterTableCache(CustomAmmoCategoriesLog.LogFile logFile) {
      foreach(var clusterTable in GetClusterHitTable_cache) {
        logFile?.WL(0,$"ClusterTable {clusterTable.Key}");
        foreach (var table in clusterTable.Value) {
          logFile?.W(1,$"{table.Key}");
          foreach (var hit in table.Value) { logFile?.W(1,$"{hit.Key}:{hit.Value}"); }; logFile?.WL(0, "");
        } 
      }
    }
    public virtual bool isSquad { get { return false; } }
    public virtual bool isVehicle { get { return false; } }
    public virtual bool isQuad { get { return false; } }
    public virtual bool isTurret {
      get {
        UnitCustomInfo info = this.GetCustomInfo();
        if (info == null) { return false; }
        return info.TurretArmorReadout;
      }
    }
    public virtual string UnitTypeName {
      get {
        UnitCustomInfo info = this.GetCustomInfo();
        if (info == null) { return this.UnitTypeNameDefault; }
        if(string.IsNullOrEmpty(info.UnitTypeName)) { return this.UnitTypeNameDefault; }
        return info.UnitTypeName;
      }
    }
    public override Text GetActorInfoFromVisLevel(VisibilityLevel visLevel) {
      if (Core.Settings.LowVisDetected) { return base.GetActorInfoFromVisLevel(visLevel); }
      if (visLevel == VisibilityLevel.LOSFull || visLevel == VisibilityLevel.BlipGhost) {
        Text result = null;
        bool displayNick = this.Combat.NetworkGameInstance != null && this.Combat.NetworkGameInstance.IsNetworkGameActive() && this.Combat.HostilityMatrix.IsLocalPlayerEnemy(this.team.GUID);
        if (this.isVehicle || this.FakeVehicle()) {
          result = new Text("{0}", displayNick ? this.Nickname : this.DisplayName);
        }else if (this.isSquad) {
          result = new Text("{0}", displayNick ? this.Nickname : this.DisplayName);
        } else {
          result = new Text("{0} - {1}", new object[2] { displayNick ? (object) this.UnitName : (object) this.Nickname, (object) this.VariantName });
        }
        return result == null?new Text("???"):result;
      } else
      if (visLevel >= VisibilityLevel.Blip4Maximum) {
        return new Text("{0}, {1}t", new object[]
        {
          this.UnitTypeName,
          (this.MechDef.Chassis.Tonnage)
        });
      } else
      if (visLevel == VisibilityLevel.Blip1Type) {
        return new Text("UNKNOWN {0}", this.UnitTypeName);
      }
      return new Text("?", (object[])Array.Empty<object>());
    }

    public virtual string UnitTypeNameDefault { get { return "MECH"; } }
    public virtual void ApplyScale(Vector3 scale) {
      this.custGameRep.ApplyScale(scale);
    }
    public virtual bool CanBeBossSeen { get; set; } = false;
    public override void TeleportActor(Vector3 newPosition) {
      if (this.GetCustomInfo().BossAppearAnimation == false) { this.TeleportActorCustom(newPosition); return; }
      if (this.CanBeBossSeen) { this.TeleportActorCustom(newPosition); return; }
    }
    public virtual void TeleportActorCustom(Vector3 newPosition) {
      Log.Combat?.TWL(0, "TeleportActorCustom "+this.PilotableActorDef.ChassisID+" pos:"+newPosition);
      base.TeleportActor(newPosition);
      foreach (LinkedActor link in this.linkedActors) {
        Log.Combat?.WL(1, "link teleport " + link.actor.PilotableActorDef.ChassisID + " pos:" + (newPosition + link.relativePosition));
        link.actor.TeleportActor(newPosition+link.relativePosition);
        link.actor.CurrentPosition = newPosition + link.relativePosition;
        link.actor.GameRep.transform.position = newPosition + link.relativePosition;
        link.actor.IsTeleportedOffScreen = true;
      }
    }
    public virtual void CheckMeleeSystemWeapons() {
      Log.Combat?.TWL(0, "CustomMech.CheckMeleeSystemWeapons "+this.MechDef.ChassisID);
      if (this.MechDef.meleeWeaponRef == null) {
        Log.Combat?.WL(1, "meleeWeaponRef is null should not happend");
        this.MechDef.meleeWeaponRef = new MechComponentRef(Core.Settings.DefaultMeleeDefinition, "", ComponentType.Weapon, ChassisLocations.CenterTorso);
      }
      if (this.Combat.DataManager.WeaponDefs.Exists(this.MechDef.meleeWeaponRef.ComponentDefID) == false) {
        Log.Combat?.WL(1, "!!!!WARNING!!!! "+ this.MechDef.meleeWeaponRef.ComponentDefID + " does not exists in DataManager fix this");
      } else {
        Log.Combat?.WL(1, this.MechDef.meleeWeaponRef.ComponentDefID+" exists in dataManager");
      }
      if (this.MechDef.dfaWeaponRef == null) {
        Log.Combat?.WL(1, "meleeWeaponRef is null should not happend");
        this.MechDef.dfaWeaponRef = new MechComponentRef(Core.Settings.DefaultDFADefinition, "", ComponentType.Weapon, ChassisLocations.CenterTorso);
      }
      if (this.Combat.DataManager.WeaponDefs.Exists(this.MechDef.dfaWeaponRef.ComponentDefID) == false) {
        Log.Combat?.WL(1, "!!!!WARNING!!!! " + this.MechDef.dfaWeaponRef.ComponentDefID + " does not exists in DataManager fix this");
      } else {
        Log.Combat?.WL(1, this.MechDef.dfaWeaponRef.ComponentDefID + " exists in dataManager");
      }
      if (this.MechDef.imaginaryLaserWeaponRef == null) {
        Log.Combat?.WL(1, "meleeWeaponRef is null should not happend");
        this.MechDef.imaginaryLaserWeaponRef = new MechComponentRef(Core.Settings.DefaultAIImaginaryDefinition, "", ComponentType.Weapon, ChassisLocations.CenterTorso);
      }
      if (this.Combat.DataManager.WeaponDefs.Exists(this.MechDef.imaginaryLaserWeaponRef.ComponentDefID) == false) {
        Log.Combat?.WL(1, "!!!!WARNING!!!! " + this.MechDef.imaginaryLaserWeaponRef.ComponentDefID + " does not exists in DataManager fix this");
      } else {
        Log.Combat?.WL(1, this.MechDef.imaginaryLaserWeaponRef.ComponentDefID + " exists in dataManager");
      }
    }
    public override void Init(Vector3 position, float facing, bool checkEncounterCells) {
      if (this.GetCustomInfo().BossAppearAnimation) {
        BossAppearManager.CreateBossBeacon(this, position);
        position = this.Combat.LocalPlayerTeam.OffScreenPosition;
      }
      this.CheckMeleeSystemWeapons();
      base.Init(position, facing, checkEncounterCells);
      if (this.GetCustomInfo().BossAppearAnimation) { this.IsTeleportedOffScreen = true; }
      if (this.IsTeleportedOffScreen) {
        foreach (LinkedActor link in linkedActors) {
          link.actor.CurrentPosition = this.Combat.LocalPlayerTeam.OffScreenPosition;
          link.actor.GameRep.transform.position = this.Combat.LocalPlayerTeam.OffScreenPosition;
          link.actor.IsTeleportedOffScreen = true;
        }
      }
    }
    public class LinkedActor {
      public AbstractActor actor { get; set; }
      public CustomMech customMech { get; set; }
      public Transform rootHeightTransform { get; set; }
      public Vector3 relativePosition { get; set; }
      public bool keepPosition { get; set; }
      public LinkedActor(CustomMech parent, AbstractActor linked, Vector3 relativePosition, bool keepPos) {
        this.actor = linked;
        this.customMech = linked as CustomMech;
        if (this.customMech != null) { this.rootHeightTransform = this.customMech.custGameRep.j_Root; } else {
          this.rootHeightTransform = this.actor.GameRep?.transform.FindRecursive("j_Root");
        }
        this.relativePosition = relativePosition;
        this.keepPosition = keepPos;
      }
    }
    public virtual HashSet<LinkedActor> linkedActors { get; set; } = new HashSet<LinkedActor>();
    public virtual void AddLinkedActor(AbstractActor actor, Vector3 relativePosition, bool keepPosition) {
      linkedActors.Add(new LinkedActor(this, actor, relativePosition, keepPosition));
    }
    public override void OnPositionUpdate(Vector3 position, Quaternion heading, int stackItemUID, bool updateDesignMask, List<DesignMaskDef> remainingMasks, bool skipLogging = false) {
      base.OnPositionUpdate(position, heading, stackItemUID, updateDesignMask, remainingMasks, skipLogging);
      foreach(LinkedActor link in this.linkedActors) {
        if (link.keepPosition == true) { continue; }
        Log.Combat?.TWL(0, "OnPositionUpdate link " + link.actor.PilotableActorDef.ChassisID + " pos:" + (position + link.relativePosition));
        link.actor.OnPositionUpdate(position + link.relativePosition, link.actor.CurrentRotation, stackItemUID, updateDesignMask, remainingMasks, skipLogging);
      }
    }
    public virtual bool ForcedVisible { get; set; } = false;
    public virtual void BossAppearAnimation() {
      ForcedVisible = true;
      this.custGameRep?.BossAppearAnimation();
    }
    internal class DropOffDelegate {
      public float height { get; set; } = 0f;
      public CustomMech parent { get; set; } = null;
      public Action OnLand { get; set; } = null;
      public Action OnRestoreHeight { get; set; } = null;
      public DropOffDelegate(CustomMech parent,float h, Action land, Action restore) {
        this.height = h;
        this.OnLand = land;
        this.OnRestoreHeight = restore;
        this.parent = parent;
      }
      public void OnAnimationCompleete() {
        this.OnLand?.Invoke();
        this.parent.custGameRep.RegisterHeightChangeCompleteEvent(this.OnRestoreInt);
        this.parent.custGameRep.PendVisualHeight(this.height);
      }
      public void OnLandInt() {
        this.parent.custGameRep.ClearHeightChangeCompleteEvent();
        if(this.parent.custGameRep.customRep != null) {
          this.parent.custGameRep.customRep.DropOffAnimation(this.OnAnimationCompleete);
        } else {
          this.OnAnimationCompleete();
        }
      }
      public void OnRestoreInt() {
        this.OnRestoreHeight?.Invoke();
      }
    }
    public virtual void DropOffAnimation(Action OnLand = null, Action OnRestoreHeight = null) {
      float current_height = this.custGameRep.GetVisualHeight();
      if (current_height < Core.Epsilon) {
        OnLand?.Invoke(); OnRestoreHeight?.Invoke();
        return;
      }
      this.custGameRep.ClearHeightChangeCompleteEvent();
      this.custGameRep.RegisterHeightChangeCompleteEvent(new DropOffDelegate(this, current_height, OnLand, OnRestoreHeight).OnLandInt);
      this.custGameRep.PendVisualHeight(0f);
    }
  }
  public static class AttachExampleHelper {
    internal class AttachExampleDelegate {
      public TrooperSquad squad { get; set; }
      public CustomMech attachTarget { get; set; }
      public AttachExampleDelegate(TrooperSquad squad, CustomMech target) {
        this.squad = squad;
        this.attachTarget = target;
      }
      public void OnLand() {
        //HIDE SQUAD REPRESENTATION
      }
    }
    public static void AttachToExample(this TrooperSquad squad, AbstractActor attachTarget) {
      if (attachTarget is CustomMech custMech) {
        if (custMech.FlyingHeight() > 1.5f) { //Check if actually flying unit
          //CALL ATTACH CODE BUT WITHOUT SQUAD REPRESENTATION HIDING 
          custMech.DropOffAnimation(new AttachExampleDelegate(squad, custMech).OnLand);
        } else {
          //CALL DEFAULT ATTACH CODE
        }
      } else {
        //CALL DEFAULT ATTACH CODE
      }
    }
  }
}