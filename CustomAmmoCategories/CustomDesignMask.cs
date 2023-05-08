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
using BattleTech.UI;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using IRBTModUtils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace CustAmmoCategories {
  [SelfDocumentedClass("Settings", "DesignMaskMoveCostInfo", "DesignMaskMoveCostInfo")]
  public class DesignMaskMoveCostInfo {
    public float moveCost { get; set; } = 1f;
    public float SprintMultiplier { get; set; } = 1f;
    public DesignMaskMoveCostInfo() { }
    public DesignMaskMoveCostInfo(float mc,float sm) {
      moveCost = mc;
      SprintMultiplier = sm;
    }
    public DesignMaskMoveCostInfo(DesignMaskMoveCostInfo b) {
      this.moveCost = b.moveCost;
      this.SprintMultiplier = b.SprintMultiplier;
    }
    public void debugLog(int i) {
      string init = new string(' ', i);
      Log.M?.WL(0,init + "moveCost:"+moveCost);
      Log.M?.WL(0,init + "SprintMultiplier:" + SprintMultiplier);
    }
  }
  public class CustomDesignMaskInfo {
    public Dictionary<string, DesignMaskMoveCostInfo> CustomMoveCost { get; set; } = new Dictionary<string, DesignMaskMoveCostInfo>();
    public void AppendDefault() {
      foreach(var costInfo in CustomAmmoCategories.Settings.DefaultMoveCosts) {
        if (this.CustomMoveCost.ContainsKey(costInfo.Key)) { continue; }
        this.CustomMoveCost.Add(costInfo.Key,costInfo.Value);
      }
    }
    public CustomDesignMaskInfo() {
      CustomMoveCost = new Dictionary<string, DesignMaskMoveCostInfo>();
    }
    public CustomDesignMaskInfo(CustomDesignMaskInfo b) {
      CustomMoveCost = new Dictionary<string, DesignMaskMoveCostInfo>();
      if (b != null) {
        foreach (var ci in b.CustomMoveCost) {
          CustomMoveCost.Add(ci.Key, new DesignMaskMoveCostInfo(ci.Value));
        }
      }
    }
    public void Merge(CustomDesignMaskInfo b) {
      if (b == null) { return; }
      foreach(var ci in b.CustomMoveCost) {
        if (this.CustomMoveCost.ContainsKey(ci.Key)) {
          this.CustomMoveCost[ci.Key].moveCost += (ci.Value.moveCost - 1f);
          this.CustomMoveCost[ci.Key].SprintMultiplier += (ci.Value.SprintMultiplier - 1f);
        } else {
          this.CustomMoveCost.Add(ci.Key, new DesignMaskMoveCostInfo(ci.Value));
        }
      }
    }
    public void debugLog(int i) {
      string init = new string(' ', i);
      Log.M?.WL(0,init + "CustomMoveCost:");
      foreach(var ci in CustomMoveCost) {
        Log.M?.WL(init + " "+ci.Key+":");
        ci.Value.debugLog(i + 2);
      }
    }
  }
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, DesignMaskDef> tempDesignMasksDefs = new Dictionary<string, DesignMaskDef>();
    public static ConcurrentDictionary<string, CustomDesignMaskInfo> customDesignMaskInfo = new ConcurrentDictionary<string, CustomDesignMaskInfo>();
    public static Dictionary<string, List<EffectData>> tempDesignMasksStickyEffects = new Dictionary<string, List<EffectData>>();
    public static CustomDesignMaskInfo GetCustomDesignMaskInfo(this DesignMaskDef mask) {
      if (customDesignMaskInfo.ContainsKey(mask.Description.Id) == false) { return null; }
      return customDesignMaskInfo[mask.Description.Id];
    }
    public static string DesignMaskId(this List<string> id) {
      string result = "";
      foreach(string str in id) {
        if (string.IsNullOrEmpty(result) == false) { result += "_"; };
        result += str;
      }
      return result;
    }
    public static DesignMaskDef createDesignMask(List<string> parentID, DesignMaskDef parentMask, DesignMaskDef newMask) {
      DesignMaskDef result = new DesignMaskDef();
      List<string> resultId = new List<string>(parentID);
      if (resultId.Contains(newMask.Id)) { return parentMask; };
      resultId.Add(newMask.Id);
      resultId.Sort();
      string newDesignMaskId = resultId.DesignMaskId();
      if (CustomAmmoCategories.tempDesignMasksDefs.ContainsKey(newDesignMaskId)) {
        return CustomAmmoCategories.tempDesignMasksDefs[newDesignMaskId];
      }
      if (parentMask == null) {
        CustomAmmoCategories.tempDesignMasksDefs.Add(newDesignMaskId, newMask);
        return newMask;
      }
      string id = newDesignMaskId;
      string name = parentMask.Description.Name + " " + newMask.Description.Name;
      string details = parentMask.Description.Details + "\n" + newMask.Description.Details;
      typeof(BaseDescriptionDef).GetProperty("Id",BindingFlags.Instance|BindingFlags.Public).GetSetMethod(true).Invoke(result.Description,new object[1] { (object)id });
      typeof(BaseDescriptionDef).GetProperty("Name", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(result.Description, new object[1] { (object)name });
      typeof(BaseDescriptionDef).GetProperty("Details", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(result.Description, new object[1] { (object)details });
      typeof(BaseDescriptionDef).GetProperty("Icon", BindingFlags.Instance | BindingFlags.Public).GetSetMethod(true).Invoke(result.Description, new object[1] { (object)newMask.Description.Icon });
      result.hideInUI = newMask.hideInUI;
      result.moveCostMechLight = parentMask.moveCostMechLight + newMask.moveCostMechLight - 1f;
      result.moveCostMechMedium = parentMask.moveCostMechMedium + newMask.moveCostMechMedium - 1f; 
      result.moveCostMechHeavy = parentMask.moveCostMechHeavy + newMask.moveCostMechHeavy - 1f; 
      result.moveCostMechAssault = parentMask.moveCostMechAssault + newMask.moveCostMechAssault - 1f; 
      result.moveCostTrackedLight = parentMask.moveCostTrackedLight + newMask.moveCostTrackedLight - 1f; 
      result.moveCostTrackedMedium = parentMask.moveCostTrackedMedium + newMask.moveCostTrackedMedium - 1f; 
      result.moveCostTrackedHeavy = parentMask.moveCostTrackedHeavy + newMask.moveCostTrackedHeavy - 1f; 
      result.moveCostTrackedAssault = parentMask.moveCostTrackedAssault + newMask.moveCostTrackedAssault - 1f; 
      result.moveCostWheeledLight = parentMask.moveCostWheeledLight + newMask.moveCostWheeledLight - 1f; 
      result.moveCostWheeledMedium = parentMask.moveCostWheeledMedium + newMask.moveCostWheeledMedium - 1f; 
      result.moveCostWheeledHeavy = parentMask.moveCostWheeledHeavy + newMask.moveCostWheeledHeavy - 1f; 
      result.moveCostWheeledAssault = parentMask.moveCostWheeledAssault + newMask.moveCostWheeledAssault - 1f;
      result.moveCostSprintMultiplier = parentMask.moveCostSprintMultiplier + newMask.moveCostSprintMultiplier - 1f;
      result.stabilityDamageMultiplier = parentMask.stabilityDamageMultiplier + newMask.stabilityDamageMultiplier - 1f;
      result.visibilityMultiplier = parentMask.visibilityMultiplier + newMask.visibilityMultiplier - 1f;
      result.visibilityHeight = parentMask.visibilityHeight + newMask.visibilityHeight;
      result.signatureMultiplier = parentMask.signatureMultiplier + newMask.signatureMultiplier - 1f;
      result.sensorRangeMultiplier = parentMask.sensorRangeMultiplier + newMask.sensorRangeMultiplier - 1f;
      result.targetabilityModifier = parentMask.targetabilityModifier + newMask.targetabilityModifier;
      result.meleeTargetabilityModifier = parentMask.meleeTargetabilityModifier + newMask.meleeTargetabilityModifier - 1f;
      result.grantsGuarded = newMask.grantsGuarded;
      result.grantsEvasive = newMask.grantsEvasive;
      result.toHitFromModifier = parentMask.toHitFromModifier + newMask.toHitFromModifier;
      result.heatSinkMultiplier = parentMask.heatSinkMultiplier + newMask.heatSinkMultiplier - 1f;
      result.heatPerTurn = parentMask.heatPerTurn + newMask.heatPerTurn;
      result.legStructureDamageMin = parentMask.legStructureDamageMin + newMask.legStructureDamageMin;
      result.legStructureDamageMax = parentMask.legStructureDamageMax + newMask.legStructureDamageMax;
      result.canBurn = newMask.canBurn;
      result.canExplode = newMask.canExplode;
      result.allDamageDealtMultiplier = parentMask.allDamageDealtMultiplier + newMask.allDamageDealtMultiplier - 1f;
      result.allDamageTakenMultiplier = parentMask.allDamageTakenMultiplier + newMask.allDamageTakenMultiplier - 1f;
      result.antipersonnelDamageDealtMultiplier = parentMask.antipersonnelDamageDealtMultiplier + newMask.antipersonnelDamageDealtMultiplier - 1f;
      result.antipersonnelDamageTakenMultiplier = parentMask.antipersonnelDamageTakenMultiplier + newMask.antipersonnelDamageTakenMultiplier - 1f;
      result.energyDamageDealtMultiplier = parentMask.energyDamageDealtMultiplier + newMask.energyDamageDealtMultiplier - 1f;
      result.energyDamageTakenMultiplier = parentMask.energyDamageTakenMultiplier + newMask.energyDamageTakenMultiplier - 1f;
      result.ballisticDamageDealtMultiplier = parentMask.ballisticDamageDealtMultiplier + newMask.ballisticDamageDealtMultiplier - 1f;
      result.ballisticDamageTakenMultiplier = parentMask.ballisticDamageTakenMultiplier + newMask.ballisticDamageTakenMultiplier - 1f;
      result.missileDamageDealtMultiplier = parentMask.missileDamageDealtMultiplier + newMask.missileDamageDealtMultiplier - 1f;
      result.missileDamageTakenMultiplier = parentMask.missileDamageTakenMultiplier + newMask.missileDamageTakenMultiplier - 1f;
      result.audioSwitchSurfaceType = parentMask.audioSwitchSurfaceType;
      result.audioSwitchRainingSurfaceType = parentMask.audioSwitchRainingSurfaceType;
      result.customBiomeAudioSurfaceType = parentMask.customBiomeAudioSurfaceType;
      if (parentMask.stickyEffect == null || parentMask.stickyEffect.effectType == EffectType.NotSet) {
        result.stickyEffect = newMask.stickyEffect;
      } else {
        result.stickyEffect = parentMask.stickyEffect;
        if (newMask.stickyEffect == null || newMask.stickyEffect.effectType == EffectType.NotSet) {
        } else {
          if (CustomAmmoCategories.tempDesignMasksStickyEffects.ContainsKey(newDesignMaskId) == false) {
            CustomAmmoCategories.tempDesignMasksStickyEffects.Add(newDesignMaskId, new List<EffectData>());
          }
          CustomAmmoCategories.tempDesignMasksStickyEffects[newDesignMaskId].Add(newMask.stickyEffect);
        }
      }
      CustomDesignMaskInfo parent_customDesignMaskInfo = parentMask.GetCustomDesignMaskInfo();
      CustomDesignMaskInfo new_customDesignMaskInfo = new CustomDesignMaskInfo(parent_customDesignMaskInfo);
      new_customDesignMaskInfo.Merge(newMask.GetCustomDesignMaskInfo());
      CustomAmmoCategories.tempDesignMasksDefs.Add(newDesignMaskId, result);
      customDesignMaskInfo.AddOrUpdate(result.Description.Id, new_customDesignMaskInfo, (k, v) => { return new_customDesignMaskInfo; });
      return result;
    }
  }
}

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(ActorMovementSequence))]
  [HarmonyPatch("UpdateSticky")]
  [HarmonyPatch(MethodType.Normal)]
  public static class ActorMovementSequence_UpdateSticky {
    public static string HIDE_DESIGN_MASK_FLAG = "HIDE_DESIGN_MASK";
    public static void Prefix(ActorMovementSequence __instance) {
      if (__instance.owningActor.UnaffectedDesignMasks()) {
        Log.Combat?.TWL(0, "ActorMovementSequence.UpdateSticky "+ __instance.owningActor.PilotableActorDef.ChassisID+" is unaffected by design mask");
        Thread.CurrentThread.SetFlag(HIDE_DESIGN_MASK_FLAG);
      }
    }
    public static void Postfix(ActorMovementSequence __instance) {
      if (__instance.owningActor.UnaffectedDesignMasks()) Thread.CurrentThread.ClearFlag(HIDE_DESIGN_MASK_FLAG);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("Init")]
  [HarmonyPatch(MethodType.Normal)]
  public static class ActorMovementSequence_Init {
    public static string NO_APPLY_DESIGN_MASK_AT_INIT = "NO_APPLY_DESIGN_MASK_AT_INIT";
    public static void Prefix(AbstractActor __instance) {
      Thread.CurrentThread.SetFlag(NO_APPLY_DESIGN_MASK_AT_INIT);
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ResetPathing")]
  [HarmonyPatch(MethodType.Normal)]
  public static class ActorMovementSequence_ResetPathing {
    public static void Prefix(AbstractActor __instance) {
      if (Thread.CurrentThread.isFlagSet(ActorMovementSequence_Init.NO_APPLY_DESIGN_MASK_AT_INIT)) {
        Log.Combat?.TWL(0, "Late ReapplyDesignMasks " + __instance.PilotableActorDef.ChassisID);
        Thread.CurrentThread.ClearFlag(ActorMovementSequence_Init.NO_APPLY_DESIGN_MASK_AT_INIT);
        __instance.ReapplyDesignMasks();
      };
    }
  }
  [HarmonyPatch(typeof(Turret))]
  [HarmonyPatch("InitStats")]
  [HarmonyPatch(MethodType.Normal)]
  public static class Turret_InitStats {
    public static void Prefix(Turret __instance) {
      if (Thread.CurrentThread.isFlagSet(ActorMovementSequence_Init.NO_APPLY_DESIGN_MASK_AT_INIT)) {
        Log.Combat?.TWL(0, "Late ReapplyDesignMasks "+__instance.PilotableActorDef.ChassisID);
        Thread.CurrentThread.ClearFlag(ActorMovementSequence_Init.NO_APPLY_DESIGN_MASK_AT_INIT);
        __instance.ReapplyDesignMasks();
      };
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ReapplyDesignMasks")]
  [HarmonyPatch(MethodType.Normal)]
  public static class ActorMovementSequence_ReapplyDesignMasks {
    public static string NO_APPLY_DESIGN_MASK_AT_INIT = "NO_APPLY_DESIGN_MASK_AT_INIT";
    public static void Prefix(ref bool __runOriginal,AbstractActor __instance) {
      if (Thread.CurrentThread.isFlagSet(NO_APPLY_DESIGN_MASK_AT_INIT) == false) { return; }
      __runOriginal = false;
    }
  }
  [HarmonyPatch(typeof(CombatHUDStatusPanel))]
  [HarmonyPatch("ShowPreviewMoveIndicators")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDStatusPanel_ShowPreviewMoveIndicators {
    public static void Prefix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType) {
      if (actor.UnaffectedDesignMasks()) {
        Thread.CurrentThread.SetFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
    }
    public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType) {
      if (actor.UnaffectedDesignMasks()) {
        Thread.CurrentThread.ClearFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUDStatusPanel))]
  [HarmonyPatch("ShowPreviewStatuses")]
  [HarmonyPatch(MethodType.Normal)]
  public static class CombatHUDStatusPanel_ShowPreviewStatuses {
    public static void Prefix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType) {
      if (actor.UnaffectedDesignMasks()) {
        Thread.CurrentThread.SetFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
    }
    public static void Postfix(CombatHUDStatusPanel __instance, AbstractActor actor, MoveType moveType) {
      if (actor.UnaffectedDesignMasks()) {
        Thread.CurrentThread.ClearFlag(ActorMovementSequence_UpdateSticky.HIDE_DESIGN_MASK_FLAG);
      }
    }
  }
  [HarmonyPatch(typeof(StatisticEffect))]
  [HarmonyPatch("OnEffectBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class StatisticEffect_OnEffectBegin {
    public static void Prefix(StatisticEffect __instance, ref bool __state) {
      try {
        __state = __instance.Target.UnaffectedDesignMasks();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
    public static void Postfix(StatisticEffect __instance, ref bool __state) {
      try {
        bool cur_state = __instance.Target.UnaffectedDesignMasks();
        if (__state != cur_state) {
          if (cur_state) {
            Log.Combat?.TWL(0,"Removing all design masks from "+ __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RemoveAllDesignMaskEffects();
          } else {
            Log.Combat?.TWL(0, "Applying design mask to " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RestoreDesignMaskEffect();
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(StatisticEffect))]
  [HarmonyPatch("OnEffectPhaseBegin")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class StatisticEffect_OnEffectPhaseBegin {
    public static void Prefix(StatisticEffect __instance, ref bool __state) {
      try {
        __state = __instance.Target.UnaffectedDesignMasks();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
    public static void Postfix(StatisticEffect __instance, ref bool __state) {
      try {
        bool cur_state = __instance.Target.UnaffectedDesignMasks();
        if (__state != cur_state) {
          if (cur_state) {
            Log.Combat?.TWL(0, "Removing all design masks from " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RemoveAllDesignMaskEffects();
          } else {
            Log.Combat?.TWL(0, "Applying design mask to " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RestoreDesignMaskEffect();
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(StatisticEffect))]
  [HarmonyPatch("OnEffectTakeDamage")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class StatisticEffect_OnEffectTakeDamage {
    public static void Prefix(StatisticEffect __instance, ref bool __state) {
      try {
        __state = __instance.Target.UnaffectedDesignMasks();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
    public static void Postfix(StatisticEffect __instance, ref bool __state) {
      try {
        bool cur_state = __instance.Target.UnaffectedDesignMasks();
        if (__state != cur_state) {
          if (cur_state) {
            Log.Combat?.TWL(0, "Removing all design masks from " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RemoveAllDesignMaskEffects();
          } else {
            Log.Combat?.TWL(0, "Applying design mask to " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RestoreDesignMaskEffect();
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(StatisticEffect))]
  [HarmonyPatch("OnEffectEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class StatisticEffect_OnEffectEnd {
    public static void Prefix(StatisticEffect __instance, ref bool __state) {
      try {
        __state = __instance.Target.UnaffectedDesignMasks();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
    public static void Postfix(StatisticEffect __instance, ref bool __state) {
      try {
        bool cur_state = __instance.Target.UnaffectedDesignMasks();
        if (__state != cur_state) {
          if (cur_state) {
            Log.Combat?.TWL(0, "Removing all design masks from " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RemoveAllDesignMaskEffects();
          } else {
            Log.Combat?.TWL(0, "Applying design mask to " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RestoreDesignMaskEffect();
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(StatisticEffect))]
  [HarmonyPatch("OnEffectActivationEnd")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPriority(Priority.First)]
  public static class StatisticEffect_OnEffectActivationEnd {
    public static void Prefix(StatisticEffect __instance, ref bool __state) {
      try {
        __state = __instance.Target.UnaffectedDesignMasks();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
    public static void Postfix(StatisticEffect __instance, ref bool __state) {
      try {
        bool cur_state = __instance.Target.UnaffectedDesignMasks();
        if (__state != cur_state) {
          if (cur_state) {
            Log.Combat?.TWL(0, "Removing all design masks from " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RemoveAllDesignMaskEffects();
          } else {
            Log.Combat?.TWL(0, "Applying design mask to " + __instance.Target.PilotableActorDef.ChassisID);
            __instance.Target.RestoreDesignMaskEffect();
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        StatisticEffect.logger.LogException(e);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("ApplyDesignMaskStickyEffect")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(DesignMaskDef),typeof(int) })]
  public static class AbstractActor_ApplyDesignMaskStickyEffect {
    public static string GetDesignMaskEffectId(this AbstractActor unit) {
      return "DesignMask_" + unit.GUID;
    }
    public static void RemoveAllDesignMaskEffects(this AbstractActor unit) {
      try {
        List<Effect> defignMaskEffects = unit.Combat.EffectManager.GetAllEffectsWithID(unit.GetDesignMaskEffectId());
        foreach (Effect effect in defignMaskEffects) {
          unit.Combat.EffectManager.CancelEffect(effect);
        }
        unit.SetOccupiedDesignMask(null, -1, null);
        if (CombatHUD_Init.HUD() != null) {
          if(CombatHUD_Init.HUD().MechTray.DisplayedActor == unit) {
            CombatHUD_Init.HUD().MechTray.DisplayedActor = unit;
          }
        }
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
    }
    public static void RestoreDesignMaskEffect(this AbstractActor unit) {
      try {
        DesignMaskDef designMask = unit.Combat.MapMetaData.GetPriorityDesignMaskAtPos(unit.CurrentPosition);
        if (designMask != null) {
          unit.SetOccupiedDesignMask(designMask, -1, null);
        }
      }catch(Exception e) {
        Log.Combat?.TWL(0,e.ToString(),true);
        AbstractActor.logger.LogException(e);
      }
    }
    public static void Prefix(ref bool __runOriginal,AbstractActor __instance, ref DesignMaskDef mask, int stackItemUID) {
      if (__runOriginal == false) { return; }
      Log.Combat?.TWL(0, "AbstractActor.ApplyDesignMaskStickyEffect Prefix " + __instance.DisplayName + ":" + __instance.GUID + " UnaffectedDesignMasks:"+ __instance.UnaffectedDesignMasks(), true);
      //Log.M?.WL(0,Environment.StackTrace);
      try {
        if (__instance.UnaffectedDesignMasks()) {
          mask = null;
        }
        if (mask == null || mask.stickyEffect == null || mask.stickyEffect.effectType == EffectType.NotSet) {
          __runOriginal = false; return;
        }
        bool ActuallyApplied = false;
        if (__instance.CreateEffect(mask.stickyEffect, (Ability)null, __instance.GetDesignMaskEffectId(), stackItemUID, __instance, false)) {
          FloatieMessage.MessageNature nature = mask.stickyEffect.nature == EffectNature.Buff ? FloatieMessage.MessageNature.Buff : FloatieMessage.MessageNature.Debuff;
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, mask.stickyEffect.Description.Name, nature));
          ActuallyApplied = true;
        }
        bool InstabilityMultiplier = mask.stickyEffect.statisticData != null && mask.stickyEffect.statisticData.statName == "ReceivedInstabilityMultiplier";
        Log.Combat?.TWL(0, string.Format("[ApplyDesignMaskStickyEffect] Actor {0} applying {1}. Is InstabilityMultiplier? {2} Actually applied? {3}", (object)__instance.GUID, (object)mask.stickyEffect.effectType, (object)InstabilityMultiplier, (object)ActuallyApplied));
        __runOriginal = false; return;
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString(), true);
        AbstractActor.logger.LogException(e);
      }
      return;
    }
    private static void Postfix(AbstractActor __instance, DesignMaskDef mask, int stackItemUID) {
      if (mask == null) { return; };
      Log.Combat?.WL(0, "AbstractActor.ApplyDesignMaskStickyEffect:"+mask.Id);
      if (CustomAmmoCategories.tempDesignMasksStickyEffects.ContainsKey(mask.Id) == false) {
        Log.Combat?.WL(1, "no additional sticky effects\n");
        return;
      }
      foreach (EffectData stickyEffect in CustomAmmoCategories.tempDesignMasksStickyEffects[mask.Id]) {
        if (stickyEffect == null || stickyEffect.effectType == EffectType.NotSet) { continue; }
        Log.Combat?.WL(1, "additional sticky effect:" + stickyEffect.Description.Name);
        if (__instance.CreateEffect(stickyEffect, (Ability)null, __instance.GetDesignMaskEffectId(), stackItemUID, __instance, false)) {
          FloatieMessage.MessageNature nature = stickyEffect.nature != EffectNature.Buff ? FloatieMessage.MessageNature.Debuff : FloatieMessage.MessageNature.Buff;
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new FloatieMessage(__instance.GUID, __instance.GUID, stickyEffect.Description.Name, nature));
        }
      }
    }
  }
  [HarmonyPatch(typeof(DesignMaskDef))]
  [HarmonyPatch("FromJSON")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string) })]
  public static class DesignMaskDef_fromJSON {
    public static void Prefix(ref bool __runOriginal,VehicleChassisDef __instance, ref string json) {
      if (!__runOriginal) { return; }
      Log.M?.WL(0,"DesignMaskDef.FromJSON");
      try {
        JObject definition = JObject.Parse(json);
        string id = (string)definition["Description"]["Id"];
        Log.M?.WL(id);
        if (definition["Custom"] != null) {
          CustomDesignMaskInfo info = definition["Custom"].ToObject<CustomDesignMaskInfo>();
          info.AppendDefault();
          CustomAmmoCategories.customDesignMaskInfo.AddOrUpdate(id, info, (k,v)=> { return info; });
          info.debugLog(1);
          definition.Remove("Custom");
        }
        json = definition.ToString();
      } catch (Exception e) {
        Log.M?.TWL(0,e.ToString(), true);
        UnityGameInstance.BattleTechGame.DataManager.logger.LogException(e);
      }
    }
  }
}