/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustomAmmoCategoriesLog;
using Harmony;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CustAmmoCategories {
  [HarmonyPatch(typeof(AttackDirector.AttackSequence))]
  [HarmonyPatch("GetRandomNumber")]
  [HarmonyPatch(new Type[] { typeof(int), typeof(int) })]
  public static class AttackSequence_GetRandomNumberCycle {
    public static bool Prefix(AttackDirector.AttackSequence __instance, int groupIndex, int weaponIndex, ref int[][] ___randomCacheValuesUsed, ref float[][][] ___randomCache) {
      try {
        if (___randomCacheValuesUsed[groupIndex][weaponIndex] >= ___randomCache[groupIndex][weaponIndex].Length) { ___randomCacheValuesUsed[groupIndex][weaponIndex] = 0; }
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n", true);
      }
      return true;
    }
  }
  //[HarmonyPatch(typeof(MechComponent))]
  //[HarmonyPatch("InitStats")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { })]
  public static class MechComponent_InitStats {
    public static void Postfix(MechComponent __instance) {
      if(__instance.StatCollection.ContainsStatistic(CustomAmmoCategories.Settings.RemoveFromCritRollStatName) == false) {
        __instance.StatCollection.AddStatistic<bool>(CustomAmmoCategories.Settings.RemoveFromCritRollStatName, false);
      }       
    }
  }
  public static class AdvancedCriticalProcessor {
    public static readonly string FLAT_CRIT_CHANCE_STAT_NAME = "CAC_FlatCritChance";
    public static readonly string BASE_CRIT_CHANCE_STAT_NAME = "CAC_BaseCritChance";
    public static readonly string AP_CRIT_CHANCE_STAT_NAME = "CAC_APCritChance";
    public static string GetArmorLocationName(this ICombatant combatant, int aLoc) {
      Mech mech = combatant as Mech;
      Vehicle vehicle = combatant as Vehicle;
      Turret turret = combatant as Turret;
      Building building = combatant as Building;
      Thread.CurrentThread.pushActor(combatant as AbstractActor);
      Thread.CurrentThread.SetFlag("CHANGE_MECH_LOCATION_NAME");
      string result = "Unknown";
      if (mech != null) { result = mech.GetLongArmorLocation((ArmorLocation)aLoc).ToString(); } else
      if (vehicle != null) { result = vehicle.GetStringForArmorLocation((VehicleChassisLocations)aLoc); } else
      if (turret != null) { result = turret.GetStringForArmorLocation((BuildingLocation)aLoc); } else
      if (building != null) { result = "Structure"; }
      Thread.CurrentThread.ClearFlag("CHANGE_MECH_LOCATION_NAME");
      Thread.CurrentThread.clearActor();
      return result;
    }
    public static ChassisLocations GetCritTransferLocation(ChassisLocations location) {
      if (CustomAmmoCategories.Settings.CritLocationTransfer == false) { return ChassisLocations.None; }
      switch (location) {
        case ChassisLocations.LeftArm: return ChassisLocations.LeftTorso;
        case ChassisLocations.LeftTorso: return ChassisLocations.CenterTorso;
        case ChassisLocations.CenterTorso: return ChassisLocations.None;
        case ChassisLocations.RightArm: return ChassisLocations.RightTorso;
        case ChassisLocations.RightTorso: return ChassisLocations.CenterTorso;
        case ChassisLocations.LeftLeg: return ChassisLocations.LeftTorso;
        case ChassisLocations.RightLeg: return ChassisLocations.RightTorso;
        default: return ChassisLocations.None;
      }
    }
    public static bool ExcludedFromCritRoll(this MechComponent component) {
      if (string.IsNullOrEmpty(CustomAmmoCategories.Settings.RemoveFromCritRollStatName)) { return false; };
      if (component.StatCollection.ContainsStatistic(CustomAmmoCategories.Settings.RemoveFromCritRollStatName) == false) { return false; }
      return component.StatCollection.GetStatistic(CustomAmmoCategories.Settings.RemoveFromCritRollStatName).Value<bool>();
    }
    public static List<MechComponent> GetCritsComponentsForLocation(this AbstractActor unit, int location) {
      List<MechComponent> mechComponentList = new List<MechComponent>();
      foreach (MechComponent allComponent in unit.allComponents) {
        if (allComponent.Location == location) {
          if ((CustomAmmoCategories.Settings.DestroyedComponentsCritTrap == false) && (allComponent.IsFunctional == false)) { continue; }
          mechComponentList.Add(allComponent);
        };
      }
      return mechComponentList;
    }
    public static List<MechComponent> GetCriticalSlotsInLocation(this AbstractActor unit,ref int location) {
      List<MechComponent> result = new List<MechComponent>();
      List<MechComponent> components = unit.GetCritsComponentsForLocation(location);
      Mech mech = unit as Mech;
      if (mech != null) {
        LocationDamageLevel dmgLvl = mech.GetLocationDamageLevel((ChassisLocations)location);
        while ((components.Count == 0) && (location != 0)) {
          if (mech.NoCritTransfer()) { location = 0; } else {
            location = (int)AdvancedCriticalProcessor.GetCritTransferLocation((ChassisLocations)location);
          }
          if (location != 0) { components = mech.GetCritsComponentsForLocation(location); };
        }
      }
      foreach (MechComponent component in components) {
        for (int t = 0; t < component.inventorySize; ++t) {
          result.Add(component);
        }
      }
      return result;
    }
    public static float GetBaseCritChance(AbstractActor unit, AdvCritLocationInfo critInfo) {
      return (float)(1.0 - (double)critInfo.structureOnHit / (double)unit.MaxStructureForLocation(critInfo.armorLocation));
    }
    public static float GetBaseAPCritChance(AbstractActor unit, AdvCritLocationInfo critInfo) {
      float result = (float)(1.0 - (double)critInfo.structureOnHit / (double)unit.MaxStructureForLocation(critInfo.armorLocation));
      if (result < CustomAmmoCategories.Settings.APMinCritChance) { result = CustomAmmoCategories.Settings.APMinCritChance; };
      return result;
    }
    public static float GetCritChance(AbstractActor unit, AdvCritLocationInfo critInfo, Weapon weapon) {
      if (unit.StatCollection.GetValue<bool>("CriticalHitImmunity")) {
        Log.C.WL(1, "[GetCritChance] CriticalHitImmunity!\n");
        critInfo.critChance = 0f;
        return 0.0f;
      }
      float armor = critInfo.armorOnHit;
      if((critInfo.structureOnHit < CustomAmmoCategories.Epsilon)&&(CustomAmmoCategories.Settings.DestoryedLocationCriticalAllow == false)) {
        Log.C.WL(1, "structureOnHit: "+critInfo.structureOnHit+" and crits to destroyed location forbidden\n");
        critInfo.critChance = 0f;
        return 0.0f;
      }
      if (armor > CustomAmmoCategories.Epsilon) {
        float baseCritChance = AdvancedCriticalProcessor.GetBaseAPCritChance(unit, critInfo);
        float shardsCritChance = 1f; float shardsMod = weapon.APArmorShardsMod() * unit.APShardsMult() * unit.APShardsMult(critInfo.armorLocation);
        if (shardsMod > CustomAmmoCategories.Epsilon) {
          shardsCritChance += (1f - armor / unit.MaxArmorForLocation(critInfo.armorLocation)) * shardsMod;
        }
        float thicknessCritChance = 1f; float maxThickness = weapon.APMaxArmorThickness() * unit.APMaxArmorThiknessMult() * unit.APMaxArmorThiknessMult(critInfo.armorLocation);
        if (maxThickness > CustomAmmoCategories.Epsilon) {
          if (armor >= maxThickness) { thicknessCritChance = 0f; } else {
            thicknessCritChance = (1f - armor / maxThickness);
          }
        }
        float TAcritMultiplier = 1f;
        if (weapon.isAPCrit()) {
          TAcritMultiplier = weapon.APCriticalChanceMultiplier();
        }
        float result = baseCritChance * shardsCritChance * TAcritMultiplier * thicknessCritChance * unit.FlatCritChance() * unit.FlatCritChance(critInfo.armorLocation) * unit.APCritChance() * unit.APCritChance(critInfo.armorLocation);
        Log.C.WL(1, string.Format("[GetCritChance] base = {0}, shards mod = {1}, thickness mod = {2}, ap mod = {3}, flat={4}, ap = {5}, result = {6}!", baseCritChance, shardsCritChance, thicknessCritChance, TAcritMultiplier, unit.FlatCritChance() * unit.FlatCritChance(critInfo.armorLocation), unit.APCritChance() * unit.APCritChance(critInfo.armorLocation), result));
        critInfo.critChance = result;
        return result;
      } else {
        float a = AdvancedCriticalProcessor.GetBaseCritChance(unit, critInfo);
        float num = Mathf.Max(a, unit.Combat.Constants.ResolutionConstants.MinCritChance);
        float critMultiplier = unit.Combat.CritChance.GetCritMultiplier(unit, weapon, true) * unit.CriticalHitChanceReceivedMultiplier(critInfo.armorLocation);
        critInfo.critChance = num * critMultiplier * unit.FlatCritChance() * unit.FlatCritChance(critInfo.armorLocation) * unit.BaseCritChance() * unit.BaseCritChance(critInfo.armorLocation);
        Log.C.WL(1, string.Format("[GetCritChance] base = {0}, multiplier = {1}, flat={2}, basemod={3} result={4}!", num, critMultiplier, unit.FlatCritChance() * unit.FlatCritChance(critInfo.armorLocation), unit.BaseCritChance() * unit.BaseCritChance(critInfo.armorLocation), critInfo.critChance));
        return num * critMultiplier;
      }
    }
    public static void CritComponent(this MechComponent component, ref WeaponHitInfo hitInfo, Weapon weapon, bool forceDestroy = false) {
      try {
        Weapon weaponSelf = component as Weapon;
        AmmunitionBox ammoBox = component as AmmunitionBox;
        Jumpjet jumpjet = component as Jumpjet;
        HeatSinkDef heatSinkDef = component.componentDef as HeatSinkDef;
        bool isWeapon = weaponSelf != null;
        AttackDirector.AttackSequence attackSequence = component.parent.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        if (attackSequence != null) {
          attackSequence.FlagAttackScoredCrit(component.parent.GUID, weaponSelf, ammoBox);
        }
        ComponentDamageLevel startingDmgLevel = component.DamageLevel;
        ComponentDamageLevel pendingDmgLevel = ComponentDamageLevel.Destroyed;
        if (CACMain.Core.MechEngineerDetected) {
          pendingDmgLevel = ComponentDamageLevel.Penalized;
        } else {
          switch (startingDmgLevel) {
            case ComponentDamageLevel.Functional: {
              if (isWeapon) {
                pendingDmgLevel = ComponentDamageLevel.Penalized;
              } else {
                pendingDmgLevel = ComponentDamageLevel.Destroyed;
              }
            }; break;
            default: pendingDmgLevel = ComponentDamageLevel.Destroyed; break;
          }
        }
        if (forceDestroy) { pendingDmgLevel = ComponentDamageLevel.Destroyed; };
        Log.C.TWL(0, $"CritComponent SEQ:{hitInfo.attackSequenceId}: WEAP:{(weapon == null ? "null" : weapon.defId)} Loc:{component.Location} Component {component.defId} curState:{startingDmgLevel} pendingState{pendingDmgLevel} forceDestroy:{forceDestroy}");
        try {
          if ((CACMain.Core.MechEngineerDetected) && (forceDestroy)) {
            for (int t = 0; t < component.inventorySize; ++t) {
              component.DamageComponent(hitInfo, pendingDmgLevel, true);
              Log.C.WL(1, $"DamageComponent called state is {component.DamageLevel}");
              if (component.DamageLevel == ComponentDamageLevel.Destroyed) { break; }
            }
          } else {
            component.DamageComponent(hitInfo, pendingDmgLevel, true);
            Log.C.WL(1, $"DamageComponent called state is {component.DamageLevel}");
          }
        } catch (Exception e) {
          Log.C?.TWL(0, e.ToString(), true);
        }
        if ((component.parent != null) && (component.parent.GameRep != null) && (startingDmgLevel != component.DamageLevel) && (component.DamageLevel >= ComponentDamageLevel.Penalized)) {
          if ((weapon != null) && (weapon.weaponRep != null) && (weapon.weaponRep.HasWeaponEffect)) {
            WwiseManager.SetSwitch<AudioSwitch_weapon_type>(weapon.weaponRep.WeaponEffect.weaponImpactType, component.parent.GameRep.audioObject);
          } else {
            WwiseManager.SetSwitch<AudioSwitch_weapon_type>(AudioSwitch_weapon_type.laser_medium, component.parent.GameRep.audioObject);
          }
          WwiseManager.SetSwitch<AudioSwitch_surface_type>(AudioSwitch_surface_type.mech_critical_hit, component.parent.GameRep.audioObject);
          int num1 = (int)WwiseManager.PostEvent<AudioEventList_impact>(AudioEventList_impact.impact_weapon, component.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          int num2 = (int)WwiseManager.PostEvent<AudioEventList_explosion>(AudioEventList_explosion.explosion_small, component.parent.GameRep.audioObject, (AkCallbackManager.EventCallback)null, (object)null);
          if (component.parent.team.LocalPlayerControlsTeam)
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "critical_hit_friendly ", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          else if (!component.parent.team.IsFriendly(component.parent.Combat.LocalPlayerTeam))
            AudioEventManager.PlayAudioEvent("audioeventdef_musictriggers_combat", "critical_hit_enemy", (AkGameObj)null, (AkCallbackManager.EventCallback)null);
          if (jumpjet == null && heatSinkDef == null && (ammoBox == null && component.DamageLevel > ComponentDamageLevel.Functional)) {
            if (component.parent.GameRep is MechRepresentation mechRep) { mechRep.PlayComponentCritVFX(component.Location); };
          }
          if (ammoBox != null && component.DamageLevel == ComponentDamageLevel.Destroyed) {
            component.parent.GameRep.PlayVFX(component.Location, component.parent.Combat.Constants.VFXNames.componentDestruction_AmmoExplosion, true, Vector3.zero, true, -1f);
          }
        }
        if ((component.parent != null) && (startingDmgLevel != component.DamageLevel)) {
          if (!CustomAmmoCategories.Settings.NoCritFloatie()) {
            component.parent.Combat.MessageCenter.PublishMessage(
              new AddSequenceToStackMessage(
                new ShowActorInfoSequence(component.parent,
                new Text((component.DamageLevel == ComponentDamageLevel.Destroyed ? "__/CAC.DESTROYED/__" : "__/CAC.CRIT/__"), new object[1] { component.UIName })
            , (component.DamageLevel == ComponentDamageLevel.Destroyed ? FloatieMessage.MessageNature.ComponentDestroyed : FloatieMessage.MessageNature.CriticalHit), true)));
          }
        }
      }catch(Exception e) {
        Log.C?.TWL(0,e.ToString(),true);
      }
    }
    public static void CheckForCrit(this AbstractActor unit, ref WeaponHitInfo hitInfo, AdvCritLocationInfo critInfo, Weapon weapon) {
      int aLoc = critInfo.armorLocation;
      Log.C.TWL(0, "AdvancedCriticalProcessor.CheckForCrit " + unit.DisplayName + ":" + unit.GUID + " loc:" + unit.GetArmorLocationName(aLoc) + " AOH:" + critInfo.armorOnHit + " SOH:" + critInfo.structureOnHit + " weapon:" + weapon.defId);
      if (unit.StatCollection.GetValue<bool>("CriticalHitImmunity")) {
        Log.C.WL(1, "[GetCritChance] CriticalHitImmunity!");
        return;
      }
      if (weapon == null) { Log.C.TWL(1, "CheckForCrit had a null weapon!", true); return; };
      int location = aLoc;
      Mech mech = unit as Mech;
      if (mech != null) {
        location = (int)MechStructureRules.GetChassisLocationFromArmorLocation((ArmorLocation)aLoc);
        Log.C.WL(1, "this is mech armor location:" + unit.GetArmorLocationName(aLoc) + " -> chassis location:" + mech.GetStringForStructureLocation((ChassisLocations)location));
      };
      //AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} Loc:{2} Base crit chance: {3:P2}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)mech.Combat.CritChance.GetBaseCritChance(mech, location, true)));
      //AbstractActor.attackLogger.Log((object)string.Format("SEQ:{0}: WEAP:{1} Loc:{2} Modifiers : {3}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)mech.Combat.CritChance.GetCritMultiplierDescription((ICombatant)mech, weapon)));
      float critChance = AdvancedCriticalProcessor.GetCritChance(unit, critInfo, weapon);
      float[] randomFromCache = unit.Combat.AttackDirector.GetRandomFromCache(hitInfo, 2);
      Log.C.WL(1, string.Format("SEQ:{0}: WEAP:{1} Loc:{2} Final crit chance: {3:P2}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)critChance));
      Log.C.WL(1, string.Format("SEQ:{0}: WEAP:{1} Loc:{2} Crit roll: {3:P2}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)randomFromCache[0]));
      if ((double)randomFromCache[0] <= (double)critChance) {
        List<MechComponent> critComponents = unit.GetCriticalSlotsInLocation(ref location);
        if (critComponents.Count == 0) {
          Log.C.WL(1, "Can't find critical components in location:" + location + "\n", true);
          return;
        }
        int index = (int)((double)critComponents.Count * (double)randomFromCache[1]);
        MechComponent componentInSlot = critComponents[index];
        if (componentInSlot != null) {
          if (componentInSlot.DamageLevel != ComponentDamageLevel.Destroyed) {
            critInfo.isSuccess = true;
            critInfo.component = componentInSlot;
            critInfo.structureLocation = componentInSlot.Location;
            componentInSlot.CritComponent(ref hitInfo, weapon);
          } else {
            Log.C.WL(1, string.Format($"SEQ:{0}: WEAP:{1} Loc:{2} Critical Hit! Component already destroyed {3}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)componentInSlot.UIName));
          }
        } else {
          Log.C.WL(1, string.Format($"SEQ:{0}: WEAP:{1} Loc:{2} Critical Hit! No component in slot {3}", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString(), (object)index));
        }
      } else {
        Log.C.WL(1, string.Format($"SEQ:{0}: WEAP:{1} Loc:{2} No crit", (object)hitInfo.attackSequenceId, (object)hitInfo.attackWeaponIndex, (object)location.ToString()));
      }
    }
  }
}