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

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static string EfectedThisRoundStatName = "CAC-AmmoEjected";
    public static Dictionary<string, bool> ActorsEjectedAmmo = new Dictionary<string, bool>();
    public static void EjectAmmo(Weapon weapon, CombatHUDWeaponSlot hudSlot) {
      CustomAmmoCategoriesLog.Log.LogWrite("EjectAmmo "+weapon.defId+"\n");
      string ammoId = "";
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no "+ CustomAmmoCategories.AmmoIdStatName + " in stat collection\n");
        return;
      }
      ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
      if (extAmmo.AmmoCategory.Index == CustomAmmoCategories.NotSetCustomAmmoCategoty.Index) {
        CustomAmmoCategoriesLog.Log.LogWrite(" has no ammo in stat collection\n");
        return;
      }
      if (weapon.parent == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no parent\n");
        return;
      }
      if (weapon.parent.DistMovedThisRound > 0.0f) {
        CustomAmmoCategoriesLog.Log.LogWrite(" moved this round "+ weapon.parent.DistMovedThisRound + "\n");
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
        weapon.parent.Combat.MessageCenter.PublishMessage(new AddSequenceToStackMessage(new ShowActorInfoSequence(weapon.parent, AmmoUIName + " AMMO JETTISONED", FloatieMessage.MessageNature.Buff, false)));
        CombatHUD HUD = (CombatHUD)typeof(CombatHUDWeaponSlot).GetField("HUD", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(hudSlot);
        if ((HUD.MechWarriorTray.SprintButton.IsActive) || (HUD.MechWarriorTray.JumpButton.IsActive)) {
          HUD.MechWarriorTray.MoveButton.OnClick();
        }
        HUD.MechWarriorTray.SprintButton.DisableButton();
        HUD.MechWarriorTray.JumpButton.DisableButton();
        if(CustomAmmoCategories.checkExistance(weapon.parent.StatCollection,CustomAmmoCategories.EfectedThisRoundStatName) == false) {
          weapon.parent.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EfectedThisRoundStatName, true);
        } else {
          weapon.parent.StatCollection.Set<bool>(CustomAmmoCategories.EfectedThisRoundStatName, true);
        }
      }
      CustomAmmoCategories.prosessExposion();
    }
    public static void ClearEjection(ICombatant target) {
      if (CustomAmmoCategories.ActorsEjectedAmmo.ContainsKey(target.GUID)) {
        CustomAmmoCategories.ActorsEjectedAmmo[target.GUID] = false;
      }
      if (CustomAmmoCategories.checkExistance(target.StatCollection, CustomAmmoCategories.EfectedThisRoundStatName) == false) {
        target.StatCollection.AddStatistic<bool>(CustomAmmoCategories.EfectedThisRoundStatName, false);
      } else {
        target.StatCollection.Set<bool>(CustomAmmoCategories.EfectedThisRoundStatName, false);
      }
    }
    public static bool isEjection(ICombatant target) {
      if (CustomAmmoCategories.ActorsEjectedAmmo.ContainsKey(target.GUID)) {
        return CustomAmmoCategories.ActorsEjectedAmmo[target.GUID];
      } else {
        if (CustomAmmoCategories.checkExistance(target.StatCollection, CustomAmmoCategories.EfectedThisRoundStatName) == false) {
          return false;
        } else {
          CustomAmmoCategories.ActorsEjectedAmmo[target.GUID] = target.StatCollection.GetStatistic(CustomAmmoCategories.EfectedThisRoundStatName).Value<bool>();
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