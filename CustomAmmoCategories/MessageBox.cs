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
using UnityEngine;
using System.Collections;
using BattleTech.UI;
using InControl;
using System;
using System.IO;
using BattleTech;
using HarmonyLib;
using System.Reflection;
using CustAmmoCategories;
using System.Collections.Generic;
using CustomAmmoCategoriesLog;

namespace CustAmmoCategoriesPatches {
  //[HarmonyPatch(typeof(CombatHUD))]
  //[HarmonyPatch("OnActorSelected")]
  //[HarmonyPatch(MethodType.Normal)]
  //[HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  //public static class CombatHUDActionButton_ExecuteClick {

  //  public static void Postfix(CombatHUD __instance, AbstractActor actor) {
  //    Log.Combat?.WL(0,"CombatHUD.OnActorSelected");
  //    if (actor is Mech) {
  //      Log.Combat?.WL(1, "is mech");
  //      if (actor.team.GUID == __instance.Combat.LocalPlayerTeamGuid) {
  //        Log.Combat?.WL(1, "is player");
  //        if (JokeMessageBox.Test(actor as Mech)) {
  //          Log.Combat?.WL(1, "show message");
  //          JokeMessageBox.ShowMessage();
  //        }
  //      }
  //    }
  //  }
  //}

}

namespace CustAmmoCategories {
  public static class JokeMessageBox{
    public static void GoToWiki() {
      string TSTPath = System.IO.Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "104.dat");
      File.AppendAllText(TSTPath, "Mischief managed");
      System.Diagnostics.Process.Start("https://roguetech.gamepedia.com/Rogue_Coins");
    }
    public static bool Test(Mech mech) {
      return false;
    }
    public static void ShowMessage() {
      /*GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "RogueTech and CustomAmmoCategories presents:\nOut of Ammo? Tired of emenies overwhelming you?\nRogueTech and CustomAmmoCategories are starting special project just for you!\nDo you wish to know more?");
      popup.AddButton("No", (Action)null, true, (PlayerAction)null);
      popup.AddButton("Yes", new Action(GoToWiki), true, (PlayerAction)null);
      popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();*/
    }
  }
}