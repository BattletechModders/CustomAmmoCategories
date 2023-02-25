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
using HarmonyLib;
using System;
using UnityEngine;
using CustAmmoCategories;
using Localize;
using System.Collections;

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(Mech))]
  [HarmonyPatch("NukeStructureLocation")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(ChassisLocations), typeof(Vector3), typeof(DamageType) })]
  public static class Mech_NukeStructureLocationZombie {
    private static bool Prefix(Mech __instance, WeaponHitInfo hitInfo, int hitLoc, ChassisLocations location, Vector3 attackDirection, DamageType damageType) {
      CustomAmmoCategoriesLog.Log.LogWrite("Mech.NukeStructureLocation("+__instance.DisplayName+":"+__instance.GUID+":"+ __instance.GetStringForStructureLocation(location)+"\n");
      if (__instance.pilot == null) {
        CustomAmmoCategoriesLog.Log.LogWrite(" no pilot\n");
        return true;
      }
      if (__instance.pilot.pilotDef.PilotTags.Contains("pilot_zombie") == false) {
        CustomAmmoCategoriesLog.Log.LogWrite(" pilot is not zombie\n");
        return true;
      }
      CustomAmmoCategoriesLog.Log.LogWrite(" pilot is zombie\n");
      __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(location), 1f);
      if(location == ChassisLocations.CenterTorso) {
        AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(hitInfo.attackSequenceId);
        __instance.FlagForKnockdown();
        if (attackSequence != null) {
          attackSequence.FlagAttackCausedKnockdown(__instance.GUID);
        }
        if (__instance.pilot.Injuries < __instance.pilot.Health) {
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, new Text("EMERGENCY REPAIRS COMMENCING"), FloatieMessage.MessageNature.LocationDestroyed, true)));
          CustomAmmoCategoriesLog.Log.LogWrite(" zombie regeneration\n");
          float maxVal = 0f; ArmorLocation aloc = ArmorLocation.None;
          aloc = ArmorLocation.Head; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.CenterTorso; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.CenterTorsoRear; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.RightTorso; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.RightTorsoRear; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.LeftTorso; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.LeftTorsoRear; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.LeftLeg; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.RightLeg; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.LeftArm; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          aloc = ArmorLocation.RightArm; maxVal = __instance.GetMaxArmor(aloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForArmorLocation(aloc), maxVal); }
          ChassisLocations cloc = ChassisLocations.None;
          cloc = ChassisLocations.Head; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.CenterTorso; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.RightTorso; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.LeftTorso; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.RightArm; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.LeftArm; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.RightLeg; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          cloc = ChassisLocations.LeftLeg; maxVal = __instance.GetMaxStructure(cloc); if (maxVal > CustomAmmoCategories.Epsilon) { __instance.StatCollection.Set<float>(__instance.GetStringForStructureLocation(cloc), maxVal); }
          foreach (MechComponent component in __instance.allComponents) {
            if (component.IsFunctional == false) {
              CustomAmmoCategoriesLog.Log.LogWrite(" regenerating component:" + component.Description.Id + "\n");
              component.StatCollection.Set<ComponentDamageLevel>("DamageLevel", ComponentDamageLevel.Functional);
              component.InitPassiveSelfEffects();
            }
          }
        } else {
          __instance.Combat.MessageCenter.PublishMessage((MessageCenterMessage)new AddSequenceToStackMessage((IStackSequence)new ShowActorInfoSequence((ICombatant)__instance, new Text("BALLISTIC SHOCK"), FloatieMessage.MessageNature.LocationDestroyed, true)));
          __instance.FlagForDeath("Core destroyed", DeathMethod.PilotKilled, damageType, (int)location, hitInfo.stackItemUID, hitInfo.attackerId, false);
        }
      }
      return false;
    }
  }
}