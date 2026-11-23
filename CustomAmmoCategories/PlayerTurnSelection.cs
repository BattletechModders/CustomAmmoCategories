using BattleTech;
using BattleTech.UI;
using CustomAmmoCategoriesLog;
using HarmonyLib;

namespace CustAmmoCategories;

[HarmonyPatch(typeof(CombatSelectionHandler))]
internal static class PlayerTurnSelection
{
    [HarmonyPatch(nameof(CombatSelectionHandler.OnActivationComplete))]
    [HarmonyPostfix]
    private static void OnActivationComplete_Postfix(CombatSelectionHandler __instance)
    {
        if (__instance.Combat.LocalPlayerTeam.IsActive || __instance.SelectedActor == null)
        {
            return;
        }
        AbstractActor deselected = __instance.SelectedActor;
        __instance.DeselectActor(deselected);
        Log.Combat?.TWL(0, $"PlayerTurnSelection: deselected {deselected.DisplayName}, the player's turn has ended");
    }

    [HarmonyPatch(nameof(CombatSelectionHandler.TrySelectActor))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void TrySelectActor_Prefix(ref bool __runOriginal, CombatSelectionHandler __instance, AbstractActor actor, ref bool __result)
    {
        if (!__runOriginal || actor == null)
        {
            return;
        }
        if (IsAnotherPlayerUnitDuringActivation(__instance, actor))
        {
            __result = false;
            __runOriginal = false;
            Log.Combat?.TWL(0, $"PlayerTurnSelection: refused to select {actor.DisplayName}, another player unit is mid-activation");
            return;
        }
        if (CanUnitBeSelected(__instance, actor))
        {
            __result = TryReselectUnitDuringActivation(__instance, actor);
            __runOriginal = false;
        }
    }

    [HarmonyPatch(nameof(CombatSelectionHandler.AutoSelectActor))]
    [HarmonyPrefix]
    private static void AutoSelectActor_Prefix(ref bool __runOriginal, CombatSelectionHandler __instance)
    {
        if (!__runOriginal)
        {
            return;
        }
        AbstractActor unit = FindUnitDuringActivation(__instance);
        if (unit != null && TryReselectUnitDuringActivation(__instance, unit))
        {
            __runOriginal = false;
        }
    }

    private static bool IsAnotherPlayerUnitDuringActivation(CombatSelectionHandler handler, AbstractActor candidate)
    {
        CombatGameState combat = handler.Combat;
        if (combat == null || !combat.TurnDirector.IsInterleaved || handler.SelectedActor != null)
        {
            return false;
        }
        foreach (AbstractActor unit in combat.LocalPlayerTeam.units)
        {
            if (unit != candidate && IsDuringActivation(unit))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsDuringActivation(AbstractActor unit)
    {
        return !unit.IsDead && !unit.IsFlaggedForDeath && unit.HasBegunActivation && !unit.HasActivatedThisRound;
    }

    private static bool CanUnitBeSelected(CombatSelectionHandler handler, AbstractActor unit)
    {
        return IsPlayerUnitSelectionAllowed(handler) && IsSelectableDuringActivation(unit);
    }

    private static bool IsPlayerUnitSelectionAllowed(CombatSelectionHandler handler)
    {
        CombatGameState combat = handler.Combat;
        return combat != null && handler.SelectedActor == null && combat.LocalPlayerTeam.IsActive && !combat.StackManager.IsAnyOrderActive;
    }

    private static bool IsSelectableDuringActivation(AbstractActor unit)
    {
        return IsDuringActivation(unit) && unit.IsAvailableThisPhase;
    }

    private static bool TryReselectUnitDuringActivation(CombatSelectionHandler handler, AbstractActor unit)
    {
        unit.HasBegunActivation = false;
        bool selected;
        try
        {
            selected = handler.TrySelectActor(unit, false);
        }
        finally
        {
            unit.HasBegunActivation = true;
        }
        if (!selected)
        {
            Log.Combat?.TWL(0, $"PlayerTurnSelection: failed to reselect {unit.DisplayName} mid-activation");
            return false;
        }
        Log.Combat?.TWL(0, $"PlayerTurnSelection: reselected {unit.DisplayName} mid-activation");
        if (unit.HasMovedThisRound && handler.ActiveState is SelectionStateMoveBase)
        {
            handler.AddFireState(unit);
            Log.Combat?.TWL(0, $"PlayerTurnSelection: {unit.DisplayName} has moved, added its fire state");
        }
        return true;
    }

    private static AbstractActor FindUnitDuringActivation(CombatSelectionHandler handler)
    {
        if (!IsPlayerUnitSelectionAllowed(handler))
        {
            return null;
        }
        foreach (AbstractActor unit in handler.Combat.LocalPlayerTeam.units)
        {
            if (IsSelectableDuringActivation(unit))
            {
                return unit;
            }
        }
        return null;
    }
}
