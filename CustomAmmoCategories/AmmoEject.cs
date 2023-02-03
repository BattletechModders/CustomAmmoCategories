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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using BattleTech.UI;
using InControl;
using Localize;
using CustomAmmoCategoriesLog;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static string EjectedThisRoundStatName = "CAC-AmmoEjected";
    public static string EjectingNowStatName = "CAC-AmmoEjecting";
    public static string EjectingRestoreStatName = "CAC-WeaponRestoreState";
    public static Dictionary<string, bool> ActorsEjectedAmmo = new Dictionary<string, bool>();
    public static bool isAmmoEjecting(this AbstractActor actor) {
      if(actor.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectingNowStatName) == false) { return false; }
      return actor.StatCollection.GetStatistic(CustomAmmoCategories.EjectingNowStatName).Value<bool>();
    }
    public static void isAmmoEjecting(this AbstractActor actor,bool value) {
      if (actor.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectingNowStatName) == false) {
        actor.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EjectingNowStatName, false);
      }
      actor.StatCollection.Set<bool>(CustomAmmoCategories.EjectingNowStatName, value);
    }
    public static void EjectWeaponForce(this Weapon weapon) {
      if (weapon.IsFunctional == false) { return; }
      weapon.StatCollection.GetOrCreateStatisic<ComponentDamageLevel>(EjectingRestoreStatName, ComponentDamageLevel.Functional).SetValue<ComponentDamageLevel>(weapon.DamageLevel);
      weapon.StatCollection.Set<ComponentDamageLevel>("DamageLevel", ComponentDamageLevel.Destroyed);
      weapon.CancelCreatedEffects();
      BlockWeaponsHelpers.RecalculateBlocked(weapon.parent);
    }
    public static void EjectWeapon(Weapon weapon, CombatHUDWeaponSlot hudSlot) {
      CustomAmmoCategoriesLog.Log.LogWrite("EjectWeapon " + weapon.defId + "\n");
      if (weapon.IsFunctional == false) { return; }
      CustomAmmoCategoriesLog.Log.LogWrite(" HasActivatedThisRound " + weapon.parent.HasActivatedThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" HasFiredThisRound " + weapon.parent.HasFiredThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" HasMovedThisRound " + weapon.parent.HasMovedThisRound + "\n");
      if (weapon.parent.HasFiredThisRound || weapon.parent.HasMovedThisRound) {
        CustomAmmoCategoriesLog.Log.LogWrite(" moved or fired this round " + weapon.parent.DistMovedThisRound + "\n");
        GenericPopupBuilder popup = GenericPopupBuilder.Create("__/CAC.AMMOEJECTIONERROR/__", "You can't drop weapon after moving or firing this round");
        popup.AddButton("OK", (Action)null, true, (PlayerAction)null);
        popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        return;
      }
      weapon.StatCollection.GetOrCreateStatisic<ComponentDamageLevel>(EjectingRestoreStatName, ComponentDamageLevel.Functional).SetValue<ComponentDamageLevel>(weapon.DamageLevel);
      weapon.StatCollection.Set<ComponentDamageLevel>("DamageLevel", ComponentDamageLevel.Destroyed);
      weapon.CancelCreatedEffects();
      CustomAmmoCategories.ActorsEjectedAmmo[weapon.parent.GUID] = true;
      weapon.parent.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(weapon.parent, new Text("WEAPON DROPPED"), FloatieMessage.MessageNature.Buff, false)));
      CombatHUD HUD = (CombatHUD)typeof(CombatHUDWeaponSlot).GetField("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hudSlot);
      if ((HUD.MechWarriorTray.SprintButton.IsActive) || (HUD.MechWarriorTray.JumpButton.IsActive)) {
        //weapon.parent.isAmmoEjecting(true);
        typeof(CombatHUDActionButton).GetMethod("ExecuteClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(HUD.MechWarriorTray.MoveButton, new object[0] { });
        //HUD.MechWarriorTray.MoveButton.ExecuteClick();
        //weapon.parent.isAmmoEjecting(false);
      }
      HUD.MechWarriorTray.SprintButton.DisableButton();
      HUD.MechWarriorTray.JumpButton.DisableButton();
      if (weapon.parent.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectedThisRoundStatName) == false) {
        weapon.parent.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EjectedThisRoundStatName, true);
      } else {
        weapon.parent.StatCollection.Set<bool>(CustomAmmoCategories.EjectedThisRoundStatName, true);
      }
      BlockWeaponsHelpers.RecalculateBlocked(weapon.parent);
    }
    public static void EjectAmmo(Weapon weapon, CombatHUDWeaponSlot hudSlot) {
      CustomAmmoCategoriesLog.Log.LogWrite("EjectAmmo "+weapon.defId+"\n");
      string ammoId = "";
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no "+ CustomAmmoCategories.AmmoIdStatName + " in stat collection\n");
        return;
      }
      ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
      if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
        CustomAmmoCategoriesLog.Log.LogWrite(" has no ammo in stat collection\n");
        GenericPopupBuilder popup = GenericPopupBuilder.Create("__/CAC.AMMOEJECTIONERROR/__", "__/CAC.AMMOEJECTIONERRORNOAMMO/__");
        popup.AddButton("OK", (Action)null, true, (PlayerAction)null);
        popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        return;
      }
      if (weapon.parent == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no parent\n");
        return;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" HasActivatedThisRound " + weapon.parent.HasActivatedThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" HasFiredThisRound " + weapon.parent.HasFiredThisRound + "\n");
      CustomAmmoCategoriesLog.Log.LogWrite(" HasMovedThisRound " + weapon.parent.HasMovedThisRound + "\n");
      if (weapon.parent.HasFiredThisRound || weapon.parent.HasMovedThisRound) {
        CustomAmmoCategoriesLog.Log.LogWrite(" moved or fired this round "+ weapon.parent.DistMovedThisRound + "\n");
        GenericPopupBuilder popup = GenericPopupBuilder.Create("__/CAC.AMMOEJECTIONERROR/__", "You can't eject ammo after moving or firing this round");
        popup.AddButton("OK", (Action)null, true, (PlayerAction)null);
        popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();
        return;
      }
      int ejectedCount = 0;
      string AmmoUIName = "";
      CustomAmmoCategoriesLog.Log.LogWrite(" ejecting\n");
      foreach (AmmunitionBox box in weapon.ammoBoxes) {
        if (box.ammoDef.Description.Id == ammoId) {
          ejectedCount = box.CurrentAmmo;
          box.StatCollection.Set<int>("CurrentAmmo",0);
          AmmoUIName = extAmmo.AmmoCategory.Id+"(" +box.ammoDef.Description.UIName+")";
          CustomAmmoCategoriesLog.Log.LogWrite(" add to exposion check "+box.defId+"\n");
          CustomAmmoCategories.AddToExposionCheck(box);
        }
      }
      if (ejectedCount > 0) {
        CustomAmmoCategories.ActorsEjectedAmmo[weapon.parent.GUID] = true;
        weapon.parent.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(weapon.parent,new Text("__/CAC.AMMOJETTISONED/__", AmmoUIName), FloatieMessage.MessageNature.Buff, false)));
        CombatHUD HUD = (CombatHUD)typeof(CombatHUDWeaponSlot).GetField("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hudSlot);
        if ((HUD.MechWarriorTray.SprintButton.IsActive) || (HUD.MechWarriorTray.JumpButton.IsActive)) {
          //weapon.parent.isAmmoEjecting(true);
          typeof(CombatHUDActionButton).GetMethod("ExecuteClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(HUD.MechWarriorTray.MoveButton, new object[0] { });
          //HUD.MechWarriorTray.MoveButton.ExecuteClick();
          //weapon.parent.isAmmoEjecting(false);
        }
        HUD.MechWarriorTray.SprintButton.DisableButton();
        HUD.MechWarriorTray.JumpButton.DisableButton();
        if(weapon.parent.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectedThisRoundStatName) == false) {
          weapon.parent.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EjectedThisRoundStatName, true);
        } else {
          weapon.parent.StatCollection.Set<bool>(CustomAmmoCategories.EjectedThisRoundStatName, true);
        }
      }
      CustomAmmoCategories.prosessExposion();
    }
    public static void ClearEjection(ICombatant target) {
      if (CustomAmmoCategories.ActorsEjectedAmmo.ContainsKey(target.GUID)) {
        CustomAmmoCategories.ActorsEjectedAmmo[target.GUID] = false;
      }
      if (target.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectedThisRoundStatName) == false) {
        target.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EjectedThisRoundStatName, false);
      } else {
        target.StatCollection.Set<bool>(CustomAmmoCategories.EjectedThisRoundStatName, false);
      }
    }
    public static bool isEjection(ICombatant target) {
      if (CustomAmmoCategories.ActorsEjectedAmmo.ContainsKey(target.GUID)) {
        return CustomAmmoCategories.ActorsEjectedAmmo[target.GUID];
      } else {
        if (target.StatCollection.ContainsStatistic(CustomAmmoCategories.EjectedThisRoundStatName) == false) {
          return false;
        } else {
          CustomAmmoCategories.ActorsEjectedAmmo[target.GUID] = target.StatCollection.GetStatistic(CustomAmmoCategories.EjectedThisRoundStatName).Value<bool>();
          return CustomAmmoCategories.ActorsEjectedAmmo[target.GUID];
        }
      }
    }

  }
}


namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("CanSprint")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class Mech_CanSprint {
    public static void Postfix(Mech __instance, ref bool __result) {
      if (CustomAmmoCategories.isEjection(__instance)) {
        __result = false;
      }
    }
  }
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("HandleMissionComplete")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatHUD_HandleMissionComplete {
    public static void Prefix(CombatHUD __instance) {
      try {
        if (CustomAmmoCategories.Settings.RestoreEjectedWeapons == false) { return; }
        Log.M?.TWL(0,$"Restoring ejected weapons");
        foreach(var unit in __instance.Combat.AllActors) {
          Log.M?.WL(1, $"{unit.PilotableActorDef.ChassisID}:{unit.PilotableActorDef.GUID}");
          foreach (var weapon in unit.Weapons) {
            var stat = weapon.StatCollection.GetStatistic(CustomAmmoCategories.EjectingRestoreStatName);
            if (stat == null) { continue; }
            Log.M?.WL(2, $"{weapon.defId}:{weapon.DamageLevel}->{stat.Value<ComponentDamageLevel>()}");
            if (stat.Value<ComponentDamageLevel>() != weapon.DamageLevel) {
              weapon.StatCollection.Set<ComponentDamageLevel>("DamageLevel", stat.Value<ComponentDamageLevel>());
            }
          }
        }
      }catch(Exception e) {
        Log.M?.TWL(0,e.ToString(),true);
      }
    }
  }
  [HarmonyPatch(typeof(AbstractActor))]
  [HarmonyPatch("WorkingJumpjets")]
  [HarmonyPatch(MethodType.Getter)]
  [HarmonyPatch(new Type[] { })]
  public static class AbstractActor_WorkingJumpjets {
    public static void Postfix(AbstractActor __instance, ref int __result) {
      if (CustomAmmoCategories.isEjection(__instance)) {
        __result = 0;
      }
    }
  }
}