using BattleTech;
using BattleTech.UI;
using System;
using UnityEngine;

namespace CustomUnits {
  public class DumpListener: MonoBehaviour {
    public static void DumpDescription(DescriptionDef description) {
      Log.Dump?.WL(3, $"Id:{description.Id}");
      Log.Dump?.WL(3, $"Name:{description.Name}");
      Log.Dump?.WL(3, $"Icon:{description.Icon}");
      Log.Dump?.WL(3, $"Details:{description.Details}");
      Log.Dump?.WL(3, $"UIName:{description.UIName}");
      Log.Dump?.WL(3, $"Cost:{description.Cost}");
      Log.Dump?.WL(3, $"Manufacturer:{description.Manufacturer}");
      Log.Dump?.WL(3, $"Rarity:{description.Rarity}");
      Log.Dump?.WL(3, $"Model:{description.Model}");
      Log.Dump?.WL(3, $"Purchasable:{description.Purchasable}");
    }
    public static void DumpStats(StatCollection StatCollection) {
      foreach(var stat in StatCollection) {
        Log.Dump?.WL(3, $"{stat.Key}={stat.Value.CurrentValue.ToString()}:{stat.Value.ValueType().ToString()}");
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