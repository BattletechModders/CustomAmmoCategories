/*  
 *  This file is part of CustomUnits.
 *  CustomUnits is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using BattleTech;
using CustAmmoCategories;
using HarmonyLib;
using IRBTModUtils;
using System;

namespace CustomUnits {
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyedMap {
    public static bool Prefix(CombatGameState __instance) {
      try {
        //HardpointAnimatorHelper.Clear();
        UnitsAnimatedPartsHelper.Clear();
        CustomMechRepresentation.Clear();
        //ActorMovementSequence_InitDistanceClamp.Clear();
        VTOLBodyAnimationHelper.Clear();
        CombatHUDMechwarriorTray_RefreshTeam.Clear();
        //ContractObjectiveGameLogic_Update.Clear();
        //ObjectiveGameLogic_Update.Clear();
        //AlternateRepresentationHelper.Clear();
        FakeHeightController.Clear();
        BossAppearManager.Clear();
        VehicleCustomInfoHelper.Clear();
        DeployManualHelper.Clean();
        TargetingCirclesHelper.Clear();
        MoveClampHelper.Clear();
        SelectionStateJump_GetAllDFATargets.Clear();
        PathingHelper.Clear();
        CustomDeploy.Core.ClearFallbackTracked();
        //StackDataHelper.Clear();
      } catch (Exception e) {
        Log.LogWrite(e.ToString() + "\n");
      }
      return true;
    }
  }
}