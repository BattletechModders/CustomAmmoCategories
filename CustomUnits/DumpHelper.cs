using BattleTech;
using BattleTech.UI;
using IRBTModUtils;
using System;
using UnityEngine;

namespace CustomUnits {
  public class DumpListener: MonoBehaviour {
    public static void DumpDescription(DescriptionDef description, int i = 3) {
      Log.Dump?.WL(i, $"Id:{description.Id}");
      Log.Dump?.WL(i, $"Name:{description.Name}");
      Log.Dump?.WL(i, $"Icon:{description.Icon}");
      Log.Dump?.WL(i, $"Details:{description.Details}");
      Log.Dump?.WL(i, $"UIName:{description.UIName}");
      Log.Dump?.WL(i, $"Cost:{description.Cost}");
      Log.Dump?.WL(i, $"Manufacturer:{description.Manufacturer}");
      Log.Dump?.WL(i, $"Rarity:{description.Rarity}");
      Log.Dump?.WL(i, $"Model:{description.Model}");
      Log.Dump?.WL(i, $"Purchasable:{description.Purchasable}");
    }
    public static void DumpStats(StatCollection StatCollection, int i = 3) {
      foreach(var stat in StatCollection) {
        Log.Dump?.WL(i, $"{stat.Key}={stat.Value.CurrentValue.ToString()}:{stat.Value.ValueType().ToString()}");
      }
    }
    public static void DumpComponent(MechComponent component) {
      Log.Dump?.WL(2, $"{component.defId}:{component.GetType().ToString()}");
      if(component is Weapon weapon) {
        Log.Dump?.WL(3, $"WeaponState:{CustAmmoCategories.JammingRealizer.CantFireReason(weapon)}");
      }
      Log.Dump?.WL(3, $"DamageLevel:{component.DamageLevel}");
      Log.Dump?.WL(3, $"Location:{component.Location}");
      Log.Dump?.WL(3, $"Description:");
      DumpDescription(component.componentDef.Description,4);
      Log.Dump?.WL(3, $"StatCollection:");
      DumpStats(component.StatCollection, 4);
      Log.Dump?.WL(3, $"Effects:");
      foreach (var effect in component.parent.Combat.EffectManager.effects) {
        if (effect is StatisticEffect statEffect) {
          if (statEffect.statCollection == component.parent.StatCollection) {
            Log.Dump?.WL(4, $"ID:{statEffect.effectData.Description.Id} creatorGUID:{effect.creatorID} stat:{statEffect.effectData.statisticData.statName} operation:{statEffect.effectData.statisticData.operation} modValue:{statEffect.effectData.statisticData.modValue}");
          }
        }
      }

    }
    public static void DumpActor(AbstractActor actor) {
      Log.Dump?.WL(2, $"MaxWalkDistance:{actor.MaxWalkDistance}");
      Log.Dump?.WL(2, $"MaxSprintDistance:{actor.MaxSprintDistance}");
      Log.Dump?.WL(2, $"CanSprint:{actor.CanSprint}");
      Log.Dump?.WL(2, $"MaxBackwardDistance:{actor.MaxBackwardDistance}");
      Log.Dump?.WL(2, $"MaxMeleeEngageRangeDistance:{actor.MaxMeleeEngageRangeDistance}");
      Log.Dump?.WL(2, $"Radius:{actor.Radius}");
      Log.Dump?.WL(2, $"IsInterruptActor:{actor.IsInterruptActor}");
      Log.Dump?.WL(2, $"HasActivatedThisRound:{actor.HasActivatedThisRound}");
      Log.Dump?.WL(2, $"HasBegunActivation:{actor.HasBegunActivation}");
      Log.Dump?.WL(2, $"IsCompletingActivation:{actor.IsCompletingActivation}");
      Log.Dump?.WL(2, $"HasMovedThisRound:{actor.HasMovedThisRound}");
      Log.Dump?.WL(2, $"StoodUpThisRound:{actor.StoodUpThisRound}");
      Log.Dump?.WL(2, $"DistMovedThisRound:{actor.DistMovedThisRound}");
      Log.Dump?.WL(2, $"JumpedLastRound:{actor.JumpedLastRound}");
      Log.Dump?.WL(2, $"HasJumpedThisRound:{actor.HasJumpedThisRound}");
      Log.Dump?.WL(2, $"SprintedLastRound:{actor.SprintedLastRound}");
      Log.Dump?.WL(2, $"MeleeAttackedLastRound:{actor.MeleeAttackedLastRound}");
      Log.Dump?.WL(2, $"HasSprintedThisRound:{actor.HasSprintedThisRound}");
      Log.Dump?.WL(2, $"MovedLastRound:{actor.MovedLastRound}");
      Log.Dump?.WL(2, $"BracedLastRound:{actor.BracedLastRound}");
      Log.Dump?.WL(2, $"CrewLocationChassis:{CustAmmoCategories.UnitUnaffectionsActorStats.CrewLocationChassis(actor)}");
      Log.Dump?.WL(2, $"allComponents");
      foreach (var component in actor.allComponents) {
        DumpComponent(component);
      }
    }
    public static void DumpCombatant(ICombatant combatant) {
      Log.Dump?.WL(2, $"GUID:{combatant.GUID}");
      Log.Dump?.WL(2, $"Type:{combatant.GetType().ToString()}");
      Log.Dump?.WL(2, $"UnitType:{combatant.UnitType}");
      Log.Dump?.WL(2, $"DisplayName:{combatant.DisplayName}");
      Log.Dump?.WL(2, $"LogDisplayName:{combatant.LogDisplayName}");
      Log.Dump?.WL(2, $"team:{combatant.team?.DisplayName}:{combatant.team?.GUID} isPlayer:{combatant.team?.IsLocalPlayer}");
      if (combatant.GameRep != null) {
        Log.Dump?.WL(2, $"GameRep:{combatant.GameRep.name}");
      } else {
        Log.Dump?.WL(2, $"GameRep:null");
      }
      Log.Dump?.WL(2, $"CurrentPosition:{combatant.CurrentPosition}");
      Log.Dump?.WL(2, $"TargetPosition:{combatant.TargetPosition}");
      if(combatant.Description != null) {
        Log.Dump?.WL(2, $"Description:");
        DumpDescription(combatant.Description);
      } else {
        Log.Dump?.WL(2, $"Description:null");
      }
      Log.Dump?.WL(2, $"StartingArmor:{combatant.StartingArmor}");
      Log.Dump?.WL(2, $"CurrentArmor:{combatant.CurrentArmor}");
      Log.Dump?.WL(2, $"SummaryArmorMax:{combatant.SummaryArmorMax}");
      Log.Dump?.WL(2, $"SummaryArmorCurrent:{combatant.SummaryArmorCurrent}");
      Log.Dump?.WL(2, $"StartingStructure:{combatant.StartingStructure}");
      Log.Dump?.WL(2, $"SummaryStructureMax:{combatant.SummaryStructureMax}");
      Log.Dump?.WL(2, $"SummaryStructureCurrent:{combatant.SummaryStructureCurrent}");
      Log.Dump?.WL(2, $"IsAnyStructureExposed:{combatant.IsAnyStructureExposed}");
      Log.Dump?.WL(2, $"HealthAsRatio:{combatant.HealthAsRatio}");
      Log.Dump?.WL(2, $"IsDead:{combatant.IsDead}");
      Log.Dump?.WL(2, $"IsOperational:{combatant.IsOperational}");
      Log.Dump?.WL(2, $"IsShutDown:{combatant.IsShutDown}");
      Log.Dump?.WL(2, $"IsProne:{combatant.IsProne}");
      Log.Dump?.WL(2, $"CanMove:{combatant.CanMove}");
      if(combatant is AbstractActor actor) {
        DumpActor(actor);
      }
      if (combatant.StatCollection != null) {
        Log.Dump?.WL(2, $"StatCollection:");
        DumpStats(combatant.StatCollection);
        Log.Dump?.WL(2, $"Effects:");
        foreach (var effect in combatant.Combat.EffectManager.effects) {
          if(effect is StatisticEffect statEffect) {
            if(statEffect.statCollection == combatant.StatCollection) {
              Log.Dump?.WL(3, $"ID:{statEffect.effectData.Description.Id} creatorGUID:{effect.creatorID} stat:{statEffect.effectData.statisticData.statName} operation:{statEffect.effectData.statisticData.operation} modValue:{statEffect.effectData.statisticData.modValue}");
            }
          }
        }
      } else {
        Log.Dump?.WL(2, $"StatCollection:null");
      }
      Log.Dump?.WL(0, $"------------------------",true);
    }
    public static void Dump(DumpListener listener) {
      try {
        if(UnityGameInstance.BattleTechGame.Combat == null) {
          Log.Dump?.TWL(0, "Combat is null");
          return;
        }
        Log.Dump?.TWL(0, "Combat dump");
        Log.Dump?.WL(1, $"allCombatants:{UnityGameInstance.BattleTechGame.Combat.allCombatants.Count}");
        foreach (var combatant in UnityGameInstance.BattleTechGame.Combat.allCombatants) {
          if (combatant == null) { continue; }
          DumpCombatant(combatant);
        }
        listener.popup = null;
      } catch(Exception e) {
        Log.Dump?.TWL(0, e.ToString());
        CombatGameState.gameInfoLogger.LogException(e);
      }
    }
    public GenericPopup popup = null;
    public void Update() {
      if (this.popup == null) {
        if (Input.GetKey(KeyCode.B) && (Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
          this.popup = GenericPopupBuilder.Create(GenericPopupType.Info, "Dump will begin to be created once you press OK. Game UI might freeze for a while until dumping complete. Please be patient.\nThank you for choosing Roguetech!")
            .AddButton("OK", () => { DumpListener.Dump(this); }, true).AddFader().SetAlwaysOnTop().Render();
        }
      }
    }
    public static void Init(CombatHUD HUD) {
      DumpListener listener = HUD.gameObject.GetComponent<DumpListener>();
      if (listener == null) { listener = HUD.gameObject.AddComponent<DumpListener>(); }
    }
  }
}