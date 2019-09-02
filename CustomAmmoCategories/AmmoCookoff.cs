using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using BattleTech;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using Settings = CustAmmoCategories.Settings;
using CustAmmoCategories;
// original code by RealityMachina reworked for 

namespace AmmoCookOff {
  public class AmmoCookOff {
    [HarmonyPatch(typeof(Mech), "CheckForHeatDamage")]
    public static class Mech_CheckHeatDamage_Patch {
      public static void Postfix(Mech __instance, int stackID, string attackerID) {
        if (CustomAmmoCategories.Settings.AmmoCookoff.Enabled == false) { return; }
        if (__instance.IsDead || (__instance.IsFlaggedForDeath && __instance.HasHandledDeath) || !__instance.IsOverheated && !__instance.IsShutDown)
          return; //don't bother if they're dead or not overheating

        foreach (MechComponent mechComponent in __instance.allComponents) {
          if (mechComponent as AmmunitionBox != null) {
            AmmunitionBox ammoBox = mechComponent as AmmunitionBox;

            if (ammoBox != null) {
              int value = ammoBox.StatCollection.GetValue<int>("CurrentAmmo");
              int capacity = ammoBox.ammunitionBoxDef.Capacity;
              float num = value / (float)capacity;
              if (num < 0.5f && CustomAmmoCategories.Settings.AmmoCookoff.UseHBSMercySetting) {
                return;
              }
              var rng = (new System.Random()).Next(100);
              var rollToBeat = __instance.IsShutDown ? CustomAmmoCategories.Settings.AmmoCookoff.ShutdownHeatChance : CustomAmmoCategories.Settings.AmmoCookoff.OverheatChance; //if shut down, we use the Shutdown chance. Otherwise, the normal overheat chance.


              if (rng < rollToBeat) //things are exploding captain!
              {
                if (__instance.Combat.Constants.PilotingConstants.InjuryFromAmmoExplosion) {
                  Pilot pilot = __instance.GetPilot();
                  if (pilot != null) {
                    pilot.SetNeedsInjury(InjuryReason.AmmoExplosion);
                  }
                }
                string text = string.Format("__/CAC.EXPLOSION/__", ammoBox.Name);
                ammoBox.parent.Combat.MessageCenter.PublishMessage(new FloatieMessage(ammoBox.parent.GUID, ammoBox.parent.GUID, text, FloatieMessage.MessageNature.CriticalHit));
                //we make a fake hit info to apply the nuking
                WeaponHitInfo hitInfo = new WeaponHitInfo(stackID, -1, -1, -1, string.Empty, string.Empty, -1, null, null, null, null, null, null, null, new AttackDirection[1] { AttackDirection.None }, null, null, null);
                Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                __instance.NukeStructureLocation(hitInfo, ammoBox.Location, (ChassisLocations)ammoBox.Location, onUnitSphere, DamageType.Overheat);
                ChassisLocations dependentLocation = MechStructureRules.GetDependentLocation((ChassisLocations)ammoBox.Location);
                if (dependentLocation != ChassisLocations.None && !__instance.IsLocationDestroyed(dependentLocation)) {
                  __instance.NukeStructureLocation(hitInfo, ammoBox.Location, dependentLocation, onUnitSphere, DamageType.Overheat);
                }
              }
            }
          }
        }
      }
    }
  }
}
