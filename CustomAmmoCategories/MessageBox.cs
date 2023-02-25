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

namespace CustAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatHUD))]
  [HarmonyPatch("OnActorSelected")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(AbstractActor) })]
  public static class CombatHUDActionButton_ExecuteClick {

    public static void Postfix(CombatHUD __instance, AbstractActor actor) {
      CustomAmmoCategoriesLog.Log.LogWrite("CombatHUD.OnActorSelected\n");
      if (actor is Mech) {
        CustomAmmoCategoriesLog.Log.LogWrite(" is mech\n");
        if (actor.team.GUID == __instance.Combat.LocalPlayerTeamGuid) {
          CustomAmmoCategoriesLog.Log.LogWrite(" is player\n");
          if (JokeMessageBox.Test(actor as Mech)) {
            CustomAmmoCategoriesLog.Log.LogWrite(" show message\n");
            JokeMessageBox.ShowMessage();
          }
        }
      }
    }
  }

}

namespace CustAmmoCategories {
  public static class JokeMessageBox{
    public static void GoToWiki() {
      string TSTPath = System.IO.Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "104.dat");
      File.AppendAllText(TSTPath, "Mischief managed");
      System.Diagnostics.Process.Start("https://roguetech.gamepedia.com/Rogue_Coins");
    }
    public static bool Test(Mech mech) {
      return false;/*
      string TSTPath = System.IO.Directory.GetParent(CustomAmmoCategoriesLog.Log.BaseDirectory).FullName;
      TSTPath = System.IO.Path.Combine(TSTPath, "RogueModuleTech");
      TSTPath = System.IO.Path.Combine(TSTPath, "mod.json");
      CustomAmmoCategoriesLog.Log.LogWrite("  path:"+TSTPath+" exists:"+ File.Exists(TSTPath) + "\n");
      if (File.Exists(TSTPath) == false) { return false; }
      TSTPath = System.IO.Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, "104.dat");
      CustomAmmoCategoriesLog.Log.LogWrite("  path:" + TSTPath + " exists:" + File.Exists(TSTPath) + "\n");
      if (File.Exists(TSTPath) == true) { return false; };
      DateTime localDate = DateTime.Now;
      if ((localDate.Month != 4)||(localDate.Day != 1)) {
        if(CustomAmmoCategories.Settings.Joke == true) {
          CustomAmmoCategoriesLog.Log.LogWrite("  not a 1st April. But Joke is true\n");
        } else {
          CustomAmmoCategoriesLog.Log.LogWrite("  not a 1st April.\n");
          return false;
        }
      };
      Dictionary<string, float> curCount = new Dictionary<string, float>();
      Dictionary<string, float> fullCount = new Dictionary<string, float>();
      foreach (var box in mech.ammoBoxes) {
        if (curCount.ContainsKey(box.ammoDef.Description.Id) == false) { curCount[box.ammoDef.Description.Id] = (float)box.CurrentAmmo; } else { curCount[box.ammoDef.Description.Id] += (float)box.CurrentAmmo; }
        if (fullCount.ContainsKey(box.ammoDef.Description.Id) == false) { fullCount[box.ammoDef.Description.Id] = (float)box.AmmoCapacity; } else { fullCount[box.ammoDef.Description.Id] += (float)box.AmmoCapacity; }
      }
      foreach (var ammo in fullCount) {
        float curAmmo = 0f;
        if (curCount.ContainsKey(ammo.Key)) { curAmmo = curCount[ammo.Key]; };
        float curLevel = curAmmo / ammo.Value;
        if (curLevel < 0.1f) { return true; };
      }
      return false;*/
    }
    public static void ShowMessage() {
      /*GenericPopupBuilder popup = GenericPopupBuilder.Create(GenericPopupType.Info, "RogueTech and CustomAmmoCategories presents:\nOut of Ammo? Tired of emenies overwhelming you?\nRogueTech and CustomAmmoCategories are starting special project just for you!\nDo you wish to know more?");
      popup.AddButton("No", (Action)null, true, (PlayerAction)null);
      popup.AddButton("Yes", new Action(GoToWiki), true, (PlayerAction)null);
      popup.IsNestedPopupWithBuiltInFader().CancelOnEscape().Render();*/
    }
  }
}