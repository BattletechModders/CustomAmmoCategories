using BattleTech;
using CustAmmoCategories;
using CustomComponents;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CustomUnits {
#pragma warning disable CS0252
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("NukeStructureLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(ChassisLocations), typeof(Vector3), typeof(DamageType) })]
  public static class Mech_NukeStructureLocation {
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
      Log.TWL(0, "Mech.NukeStructureLocation Transpiler");
      MethodInfo targetMethod = typeof(MechStructureRules).GetMethod("GetDependentLocation", BindingFlags.Static | BindingFlags.Public);
      var replacementMethod = AccessTools.Method(typeof(Mech_NukeStructureLocation), nameof(GetDependentLocation));
      List<CodeInstruction> uInstructions = new List<CodeInstruction>();
      uInstructions.AddRange(instructions);
      int MethodPos = -1;
      for (int t = 0; t < uInstructions.Count; ++t) {
        if ((uInstructions[t].opcode == OpCodes.Call) && (uInstructions[t].operand == targetMethod)) {
          MethodPos = t;
          uInstructions[t].operand = replacementMethod;
          break;
        }
      }
      if (MethodPos < 0) {
        Log.WL(1, "can't find MechStructureRules.GetDependentLocation call");
        return uInstructions;
      }
      Log.WL(1, "found MechHardpointRules.GetComponentPrefabName call " + MethodPos.ToString("X"));
      uInstructions.Insert(MethodPos, new CodeInstruction(OpCodes.Ldarg_0));
      return uInstructions;
    }
    public static ChassisLocations GetDependentLocation(ChassisLocations location, Mech mech) {
      Log.TWL(0, "Mech_NukeStructureLocation.GetDependentLocation " + mech.DisplayName + " " + location + " noDeps:" + mech.NoDependentLocations());
      if (mech.NoDependentLocations() == false) { return MechStructureRules.GetDependentLocation(location); }
      return ChassisLocations.None;
    }
  }
#pragma warning restore CS0252
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("MoveMultiplier")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_MoveMultiplier {
    public static bool Prefix(Mech __instance, ref float __result) {
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        float num = 0.0f;
        if (__instance.IsOverheated) {
          num += __instance.Combat.Constants.MoveConstants.OverheatedMovePenalty;
        }
        List<ChassisLocations> legsDamageLevels = new List<ChassisLocations>();
        legsDamageLevels.Add(ChassisLocations.LeftLeg);
        legsDamageLevels.Add(ChassisLocations.RightLeg);
        if (info.ArmsCountedAsLegs) {
          legsDamageLevels.Add(ChassisLocations.LeftArm);
          legsDamageLevels.Add(ChassisLocations.RightArm);
        }
        float blackMod = info.LegDestroyedMovePenalty >= 0f ? info.LegDestroyedMovePenalty : __instance.Combat.Constants.MoveConstants.LegDestroyedPenalty;
        float redMod = info.LegDamageRedMovePenalty >= 0f ? info.LegDamageRedMovePenalty : __instance.Combat.Constants.MoveConstants.LegDamageRedPenalty;
        float yellowMod = info.LegDamageYellowMovePenalty >= 0f ? info.LegDamageYellowMovePenalty : __instance.Combat.Constants.MoveConstants.LegDamageYellowPenalty;
        foreach (ChassisLocations location in legsDamageLevels) {
          if (__instance.IsLocationDestroyed(location)) {
            num += blackMod;
          } else if (__instance.GetLocationDamageLevel(location) > LocationDamageLevel.Penalized) {
            num += redMod;
          } else {
            num += yellowMod;
          }
        }
        __result = Mathf.Max(__instance.Combat.Constants.MoveConstants.MinMoveSpeed, 1f - num);
        return false;
      }catch(Exception e) {
        Log.TWL(0,e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsLegged")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsLegged {
    public static int DestroyedLegsCount(this Mech mech) {
      int result = 0;
      if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg)) { ++result; }
      if (mech.IsLocationDestroyed(ChassisLocations.RightLeg)) { ++result; }
      UnitCustomInfo info = mech.GetCustomInfo();
      if (info != null) {
        if (info.ArmsCountedAsLegs) {
          if (mech.IsLocationDestroyed(ChassisLocations.LeftArm)) { ++result; }
          if (mech.IsLocationDestroyed(ChassisLocations.RightArm)) { ++result; }
        }
      }
      return result;
    }
    public static void Postfix(Mech __instance, ref bool __result) {
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return; }
        if (info.ArmsCountedAsLegs == false) { return; }
        __result = (__instance.DestroyedLegsCount() >= 2);
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("IsDead")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_IsDead {
    public static void Postfix(Mech __instance, ref bool __result) {
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return; }
        if (info.ArmsCountedAsLegs == false) { return; }
        if (__result == false) { return; }
        if (__instance.HasHandledDeath) { return; }
        if (__instance.pilot.IsIncapacitated) { return; }
        if (__instance.pilot.HasEjected) { return; }
        if (__instance.HeadStructure <= 0.0f) { return; }
        if (__instance.CenterTorsoStructure <= 0.0f) { return; }
        if (__instance.DestroyedLegsCount() < 4) { __result = false; }
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyLegStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplyLegStructureEffects {
    public static void ApplyLegStructureEffects(this Mech mech, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      float LegDamageRelativeInstability = mech.Combat.Constants.PilotingConstants.LegDamageRelativeInstability;
      float LegDestroyRelativeInstability = 1f;
      UnitCustomInfo info = mech.GetCustomInfo();
      if(info != null) {
        LegDamageRelativeInstability = info.LegDamageRelativeInstability >= 0.0f ? info.LegDamageRelativeInstability : mech.Combat.Constants.PilotingConstants.LegDamageRelativeInstability;
        LegDestroyRelativeInstability = info.LegDestroyRelativeInstability >= 0.0f ? info.LegDestroyRelativeInstability : 1f;
      }
      mech.AddRelativeInstability(LegDamageRelativeInstability, StabilityChangeSource.BodyPartDamaged, sourceID);
      if (newDamageLevel == oldDamageLevel) { return; }
      if (newDamageLevel == LocationDamageLevel.Destroyed) {
        mech.AddRelativeInstability(LegDestroyRelativeInstability, StabilityChangeSource.LegDestroyed, sourceID);
      }
      mech.ResetPathing(false);
      if (mech.GameRep != null) {
        mech.GameRep.UpdateLegDamageAnimFlags(mech.LeftLegDamageLevel, mech.RightLegDamageLevel);
      }
    }
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        __instance.ApplyLegStructureEffects(location, oldDamageLevel, newDamageLevel, sourceID, stackItemUID);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("ApplyArmStructureEffects")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(LocationDamageLevel), typeof(LocationDamageLevel), typeof(string), typeof(int) })]
  public static class Mech_ApplyArmStructureEffects {
    public static bool Prefix(Mech __instance, ChassisLocations location, LocationDamageLevel oldDamageLevel, LocationDamageLevel newDamageLevel, string sourceID, int stackItemUID) {
      try {
        UnitCustomInfo info = __instance.GetCustomInfo();
        if (info == null) { return true; }
        if (info.ArmsCountedAsLegs == false) { return true; }
        __instance.ApplyLegStructureEffects(location, oldDamageLevel, newDamageLevel, sourceID, stackItemUID);
        return false;
      } catch (Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("OnLocationDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(ChassisLocations), typeof(Vector3), typeof(WeaponHitInfo), typeof(DamageType) })]
  public static class Mech_OnLocationDestroyedRules {
    public static void UpdateMinStability(this Mech mech,WeaponHitInfo hitInfo) {
      float modValue = 0.0f;
      HashSet<ChassisLocations> legs = new HashSet<ChassisLocations>();
      legs.Add(ChassisLocations.LeftLeg);
      legs.Add(ChassisLocations.RightLeg);
      UnitCustomInfo info = mech.GetCustomInfo();
      float chassisModifier = 1f;
      if (info != null) {
        if (info.ArmsCountedAsLegs) {
          legs.Add(ChassisLocations.LeftArm);
          legs.Add(ChassisLocations.RightArm);
        }
        chassisModifier = info.LocDestroyedPermanentStabilityLossMod;
      }
      foreach (ChassisLocations location in Enum.GetValues(typeof(ChassisLocations))) {
        switch (location) {
          case ChassisLocations.None:
          case ChassisLocations.Torso:
          case ChassisLocations.Arms:
          case ChassisLocations.MainBody:
          case ChassisLocations.Legs:
          case ChassisLocations.All:
          continue;
          default: {
            if (mech.IsLocationDestroyed(location) == false) { continue; }
            if(mech.Combat.Constants.PilotingConstants.OnlyPermanentLossFromLegs == false) {
              modValue += mech.Combat.Constants.PilotingConstants.LocationDestroyedPermanentStabilityLoss * mech.MaxStability * chassisModifier;
            } else {
              if (legs.Contains(location)) {
                modValue += mech.Combat.Constants.PilotingConstants.LocationDestroyedPermanentStabilityLoss * mech.MaxStability * chassisModifier;
              }
            }
          }
          continue;
        }
      }
      mech.StatCollection.ModifyStat<float>(hitInfo.attackerId, hitInfo.stackItemUID, "MinStability", StatCollection.StatOperation.Set, modValue, -1, true);
      if (Traverse.Create(mech).Property<float>("_stability").Value < mech.MinStability) {
        Traverse.Create(mech).Property<float>("_stability").Value = mech.MinStability;
      }
      mech.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new StabilityChangedMessage(mech.GUID));
    }
    public static bool Prefix(Mech __instance, ChassisLocations location, Vector3 attackDirection, WeaponHitInfo hitInfo, DamageType damageType) {
      Log.TWL(0, "Mech.OnLocationDestroyed "+__instance.DisplayName+" location:"+location);
      try {
        if (location != ChassisLocations.Head && location != ChassisLocations.CenterTorso)
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, new Text("{0} DESTROYED", new object[1]
          {
          (object) Mech.GetAbbreviatedChassisLocation(location)
          }), FloatieMessage.MessageNature.LocationDestroyed, true)));
        AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        if (attackSequence != null) { attackSequence.FlagAttackDestroyedAnyLocation(__instance.GUID); };
        UnitCustomInfo info = __instance.GetCustomInfo();
        HashSet<ChassisLocations> legs = new HashSet<ChassisLocations>();
        bool armsAsLegs = false;
        legs.Add(ChassisLocations.RightLeg);
        legs.Add(ChassisLocations.LeftLeg);
        if (info != null) {
          if (info.ArmsCountedAsLegs) {
            legs.Add(ChassisLocations.RightArm);
            legs.Add(ChassisLocations.LeftArm);
            armsAsLegs = true;
          }
        }
        int destroyedLegsCount = __instance.DestroyedLegsCount();
        if (legs.Contains(location)) {
          if ((armsAsLegs == false) || (destroyedLegsCount >= 2)) {
            __instance.StatCollection.ModifyStat<float>(attackSequence == null ? "debug" : attackSequence.attacker.GUID, attackSequence == null ? -1 : attackSequence.attackSequenceIdx, "RunSpeed", StatCollection.StatOperation.Set, 0.0f, -1, true);
            __instance.FlagForKnockdown();
          }
          if (attackSequence != null) {
            if (__instance == attackSequence.attacker) {
              attackSequence.FlagAttackDestroyedAttackerLeg();
            } else if (attackSequence.allAffectedTargetIds.Contains(__instance.GUID)) {
              attackSequence.FlagAttackDestroyedLeg(__instance.GUID);
              if ((armsAsLegs == false) || (destroyedLegsCount >= 2)) attackSequence.FlagAttackCausedKnockdown(__instance.GUID);
            }
          }
        }
        foreach (MechComponent allComponent in __instance.allComponents) {
          if ((ChassisLocations)allComponent.Location == location) {
            if (allComponent.componentDef.Is<Flags>(out var f) && f.IsSet("ignore_damage")) { continue; }
            allComponent.DamageComponent(hitInfo, ComponentDamageLevel.Destroyed, false);
            if (AbstractActor.damageLogger.IsLogEnabled)
              AbstractActor.damageLogger.Log((object)string.Format("====@@@ Component Destroyed: {0}", (object)allComponent.Name));
            if (attackSequence != null) {
              Weapon weapon = allComponent as Weapon;
              AmmunitionBox ammoBox = allComponent as AmmunitionBox;
              attackSequence.FlagAttackScoredCrit(__instance.GUID, weapon, ammoBox);
            }
          }
        }
        __instance.UpdateMinStability(hitInfo);
        DeathMethod deathMethod = DeathMethod.NOT_SET;
        string reason = "";
        switch (location) {
          case ChassisLocations.Head:
            deathMethod = DeathMethod.HeadDestruction;
            reason = "Location Destroyed: " + location.ToString();
          break;
          case ChassisLocations.CenterTorso:
            deathMethod = DeathMethod.CenterTorsoDestruction;
            reason = "Location Destroyed: " + location.ToString();
          break;
          case ChassisLocations.LeftArm:
          case ChassisLocations.RightArm:
          case ChassisLocations.LeftLeg:
          case ChassisLocations.RightLeg: {
            if (legs.Contains(location) == false) { break; }
            if (((armsAsLegs == false)&&(destroyedLegsCount >= 2))||((armsAsLegs == true)&&(destroyedLegsCount >= 4))) {
              deathMethod = DeathMethod.LegDestruction;
              reason = "Location Destroyed: " + location.ToString();
              break;
            }
          }
          break;
        }
        if (damageType == DamageType.AmmoExplosion && (location == ChassisLocations.CenterTorso || location == ChassisLocations.Head)) {
          deathMethod = DeathMethod.AmmoExplosion;
          reason = "Ammo Explosion: " + location.ToString();
        } else if (damageType == DamageType.ComponentExplosion && (location == ChassisLocations.CenterTorso || location == ChassisLocations.Head)) {
          deathMethod = DeathMethod.ComponentExplosion;
          reason = "Component Explosion: " + location.ToString();
        }
        if (deathMethod != DeathMethod.NOT_SET)
          __instance.FlagForDeath(reason, deathMethod, damageType, (int)location, hitInfo.stackItemUID, hitInfo.attackerId, false);
        else if ((location == ChassisLocations.LeftTorso || location == ChassisLocations.RightTorso) && __instance.Combat.Constants.PilotingConstants.InjuryFromSideTorsoDestruction) {
          Pilot pilot = __instance.GetPilot();
          if (pilot != null)
            pilot.SetNeedsInjury(InjuryReason.SideTorsoDestroyed);
        }
        if (__instance.GameRep != null) {
          __instance.GameRep.PlayComponentDestroyedVFX((int)location, attackDirection);
        }
        return false;
      }catch(Exception e) {
        Log.TWL(0, e.ToString());
        return true;
      }
    }
  }

}