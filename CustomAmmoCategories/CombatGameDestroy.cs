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
using CustAmmoCategories;
using CustAmmoCategoriesPatches;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPathes;
using HarmonyLib;
using System;
using System.Threading;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyed {
    public static void Prefix(CombatGameState __instance) {
      Log.Combat?.TWL(0,"CombatGameState.OnCombatGameDestroyed");
      try {
        WeaponStatCacheHelper.Clear();
        TempAmmoCountHelper.Clear();
        foreach (var ae in CustomAmmoCategories.additinalImpactEffects) {
          foreach (var sp in ae.Value) {
            try { sp.CleanupSelf(); } finally { };
          }
        }
        CustomAmmoCategories.ClearJammingInfo();
        AIMaxWeaponRangeCache.Clear();
        CustomAmmoCategories.additinalImpactEffects.Clear();
        AMSWeaponEffectStaticHelper.Clear();
        DynamicMapHelper.ClearTerrain();
        MineFieldHelper.registredMovingDamage.Clear();
        BTCustomRenderer_DrawDecals.Clear();
        DynamicTreesHelper.Clean();
        CACDynamicTree.allCACTrees.Clear();
        CustomAmmoCategories.Settings.DontShowBurnedTreesTemporary = false;
        DynamicMapHelper.Combat = __instance;
        DynamicMapHelper.PoolDelayedGameObject();
        CombatHUDMechwarriorTray_InitAbilityButtons.attackGroundAbilities.Clear();
        CustomAmmoCategories.terrainHitPositions.Clear();
        DeferredEffectHelper.Clear();
        WeaponArtilleryHelper.Clear();
        PersistentFloatieHelper.Clear();
        Weapon_InternalAmmo.Clear();
        BraceNode_Tick.Clear();
        AreaOfEffectHelper.Clear();
        CombatHUDWeaponPanel_ResetSortedWeaponList.Clear();
        CombatHUDInfoSidePanelHelper.Clear();
        ExplosionAPIHelper.Clear();
        ToHitModifiersHelper.Clear();
        DamageModifiersCache.Clear();
        CombatHUDWeaponPanel_RefreshDisplayedWeapons.Clear();
        AdvWeaponHitInfo.ClearAttackLog();
        Weapon_ResetWeapon.Clear();
        AIMinefieldHelper.Clear();
        CombatHUDMiniMap.Clear();
        UnitCombatStatisticHelper.Clear();
        SpawnProtectionHelper.Clear();
        ToHit_GetAttackDirection.Clear();
        BlockWeaponsHelpers.Clear();
        AIDistanceFromNearesEnemyCache.Clear();
        Mech_AddExternalHeat.Clear();
        CombatHUD_Init.Clear();
        UnitUnaffectionsActorStats.Clear();
        CustomAmmoCategories.ClearTerrainPositions();
      } catch (Exception e) {
        Log.Combat?.TWL(0, e.ToString());
        AttackDirector.logger.LogException(e);
      }
    }
  }
}