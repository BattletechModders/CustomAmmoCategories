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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using BattleTech.Rendering;
using CustAmmoCategories;
using CustomAmmoCategoriesLog;
using HarmonyLib;
using IRBTModUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static float StrayRange(this Weapon weapon) {
      return (weapon.exDef().SpreadRangeStat(weapon) + weapon.ammo().SpreadRange + weapon.mode().SpreadRange) * weapon.exDef().SpreadRangeMod(weapon);
    }
    public static bool AOECapable(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if (mode.AOECapable == TripleBoolean.True) { return true; }
      if (ammo.AOECapable == TripleBoolean.True) { return true; }
      if (extWeapon.AOECapable == TripleBoolean.True) { return true; }
      return false;
    }
    public static bool PhysicsAoE(this Weapon weapon) {
      if(CustomAmmoCategories.Settings.PhysicsAoE_Weapons == false) { return false; }
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if(mode.PhysicsAoE == TripleBoolean.True) { return true; }
      if(ammo.PhysicsAoE == TripleBoolean.True) { return true; }
      if(extWeapon.PhysicsAoE == TripleBoolean.True) { return true; }
      return false;
    }
    public static float PhysicsAoEHeight(this Weapon weapon) {
      if(CustomAmmoCategories.Settings.PhysicsAoE_Weapons == false) { return 0f; }
      ExtAmmunitionDef ammo = weapon.ammo();
      ExtWeaponDef extWeapon = weapon.exDef();
      WeaponMode mode = weapon.mode();
      if(mode.PhysicsAoE == TripleBoolean.True) { return mode.PhysicsAoE_Height; }
      if(ammo.PhysicsAoE == TripleBoolean.True) { return ammo.PhysicsAoE_Height; }
      if(extWeapon.PhysicsAoE == TripleBoolean.True) { return extWeapon.PhysicsAoE_Height; }
      return 0f;
    }
    public static string SpesialOfflineIFF = "_IFFOfflne";
    public static string IFFTransponderDef(this Weapon weapon) {
      string result = string.Empty;
      WeaponMode mode = weapon.mode();
      if (string.IsNullOrEmpty(mode.IFFDef) == false) { result = mode.IFFDef; } else {
        ExtAmmunitionDef ammo = weapon.ammo();
        if (string.IsNullOrEmpty(ammo.IFFDef) == false) { result = ammo.IFFDef; } else {
          ExtWeaponDef def = weapon.exDef();
          if (string.IsNullOrEmpty(def.IFFDef) == false) { result = def.IFFDef; };
        }
      }
      if (result == CustomAmmoCategories.SpesialOfflineIFF) { result = string.Empty; };
      return result;
    }
    public static bool isCombatantHaveIFFTransponder(ICombatant combatant, string IFFDefId) {
      AbstractActor actor = combatant as AbstractActor;
      if (actor == null) { return false; };
      foreach (MechComponent component in actor.allComponents) {
        if (component.IsFunctional == false) { continue; }
        if (component.defId == IFFDefId) { return true; }
      }
      return false;
    }
    private static bool HitLocationsInited = false;
    public static Dictionary<int, float> NormMechHitLocations = null;
    public static Dictionary<int, float> SquadHitLocations = null;
    public static Dictionary<int, float> FakeVehicleLocations = null;
    public static Dictionary<int, float> VehicleLocations = null;
    public static Dictionary<int, float> OtherLocations = null;
    public static readonly float AOEHitIndicator = -10f;
    public static void InitHitLocationsAOE() {
      if (HitLocationsInited) { return; }
      HitLocationsInited = true;
      CustomAmmoCategories.NormMechHitLocations = new Dictionary<int, float>();
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.CenterTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.CenterTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightTorso] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightTorsoRear] = 100f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftArm] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightArm] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.LeftLeg] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.RightLeg] = 50f;
      CustomAmmoCategories.NormMechHitLocations[(int)ArmorLocation.Head] = 0f;
      CustomAmmoCategories.FakeVehicleLocations = new Dictionary<int, float>();
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.CenterTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.CenterTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightTorso] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightTorsoRear] = 0f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftArm] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightArm] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.LeftLeg] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.RightLeg] = 100f;
      CustomAmmoCategories.FakeVehicleLocations[(int)ArmorLocation.Head] = 100f;
      CustomAmmoCategories.SquadHitLocations = new Dictionary<int, float>();
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.CenterTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.CenterTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightTorso] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightTorsoRear] = 0f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftArm] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightArm] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.LeftLeg] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.RightLeg] = 100f;
      CustomAmmoCategories.SquadHitLocations[(int)ArmorLocation.Head] = 100f;
      CustomAmmoCategories.VehicleLocations = new Dictionary<int, float>();
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Front] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Rear] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Left] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Right] = 100f;
      CustomAmmoCategories.VehicleLocations[(int)VehicleChassisLocations.Turret] = 80f;
      CustomAmmoCategories.OtherLocations = new Dictionary<int, float>();
      CustomAmmoCategories.OtherLocations[1] = 100f;
    }
  }


  namespace CustomAmmoCategoriesPatches {
    [HarmonyPatch(typeof(MessageCoordinator))]
    [HarmonyPatch("Initialize")]
    [HarmonyPatch(MethodType.Normal)]
    public static class MessageCoordinator_Debug {
      public static void Postfix(MessageCoordinator __instance, WeaponHitInfo?[][] allHitInfo) {
        Log.Combat?.WL(0,"----------------------EXPECTED MESSAGES---------------------");
        List<ExpectedMessage> expectedMessages = __instance.expectedMessages;
        AttackDirector.AttackSequence attackSequence = __instance.attackSequence;
        for (int index1 = 0; index1 < allHitInfo.Length; ++index1) {
          WeaponHitInfo?[] nullableArray = allHitInfo[index1];
          for (int index2 = 0; index2 < nullableArray.Length; ++index2) {
            WeaponHitInfo? nullable = nullableArray[index2];
            Log.Combat?.WL(0, string.Format("Initializing Group {0} Weapon {1}", (object)index1, (object)index2));
            if (!nullable.HasValue) {
              Log.Combat?.WL(0, string.Format("Group {0} Weapon {1} has no value", (object)index1, (object)index2));
            } else {
              int[] hitLocations = nullable.Value.hitLocations;
              Log.Combat?.WL(0, "weapon:" + index1+"-"+index2+" number of shots:"+nullable.Value.numberOfShots);
              for (int shot = 0; shot < hitLocations.Length; ++shot) {
                AdvWeaponHitInfoRec adv = nullable.Value.advRec(shot);
                Log.Combat?.WL(1, "hitIndex = " + shot + " hitLocation = " + hitLocations[shot] + " pos:" + nullable.Value.hitPositions[shot] + " " + (shot >= nullable.Value.numberOfShots) + " dr:" + nullable.Value.dodgeRolls[shot] + " adv:"+(adv==null?"false":"true"));
                if (adv == null) { continue; };
                Log.Combat?.WL(2, "aoe:" + adv.isAOE+ " aoeproc: "+adv.isAOEproc+" loc:"+adv.hitLocation+" trg:"+adv.target.DisplayName+"("+adv.target.GUID+") D/H/S:"+adv.Damage+"/"+adv.Heat+"/"+adv.Stability);
                Log.Combat?.WL(2, "frag: sep:" + adv.fragInfo.separated+" isPallet:"+adv.fragInfo.isFragPallet+" mainIdx:"+adv.fragInfo.fragMainHitIndex+" fragStartIdx:"+adv.fragInfo.fragStartHitIndex+" count:"+adv.fragInfo.fragsCount);
              }
            }
          }
        }
        for (int index = 0; index < expectedMessages.Count; ++index) {
          Log.Combat?.WL(0, expectedMessages[index].GetDebugString());
        }
      }
    }
    public class ImpactAOEState {
      public ICombatant target;
      public WeaponHitInfo hitInfo;
      public ImpactAOEState(ICombatant trg, WeaponHitInfo hInfo) {
        target = trg;
        hitInfo = hInfo;
      }
    }
    [HarmonyPatch(typeof(Mech))]
    [HarmonyPatch("DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    public static class Mech_DamageLocation {
      [HarmonyPriority(Priority.First)]
      public static void Prefix(ref bool __runOriginal,Mech __instance, int originalHitLoc, WeaponHitInfo hitInfo, ArmorLocation aLoc, Weapon weapon, float totalArmorDamage, float directStructureDamage, int hitIndex, AttackImpactQuality impactQuality, DamageType damageType) {
        try {
          if (!__runOriginal) { return; }
          ICustomMech custMech = __instance as ICustomMech;
          if (custMech != null) {
            float armor = ((aLoc == ArmorLocation.Invalid) || (aLoc == ArmorLocation.None))?-1f:__instance.ArmorForLocation((int)aLoc);
            float structure = ((aLoc == ArmorLocation.Invalid) || (aLoc == ArmorLocation.None)) ? -1f : __instance.StructureForLocation((int)aLoc);
            string name = aLoc.ToString();
            if ((aLoc != ArmorLocation.Invalid) && (aLoc != ArmorLocation.None)) {
              name = custMech.GetLongArmorLocation(aLoc).ToString();
            }
            Log.Combat?.TWL(0, "DamageLocation " + __instance.PilotableActorDef.ChassisID + " loc:" + name + " dmg:" + totalArmorDamage + " strDmg:" + directStructureDamage + " shot:" + hitIndex+" armor before hit:"+armor+" structure before hit:"+structure);
          }
        }catch(Exception e) {
          Log.Combat?.TWL(0,e.ToString(),true);
          AbstractActor.damageLogger.LogException(e);
        }
        return;
      }
    }
    [HarmonyPatch(typeof(WeaponEffect))]
    [HarmonyPatch("DestroyFlimsyObjects")]
    [HarmonyPatch(new Type[] { })]
    public static class WeaponEffect_DestroyFlimsyObjects {
      [HarmonyPriority(Priority.First)]
      public static void Prefix(ref bool __runOriginal, WeaponEffect __instance) {
        if (!__runOriginal) { return; }
        if (!__instance.shotsDestroyFlimsyObjects) {
          return;
        }
        if (__instance.weapon.AOECapable() == false) { return; };
        float AOERange = __instance.weapon.AOERange();
        if (AOERange < CustomAmmoCategories.Epsilon) { return; };
        Vector3 endPos = Traverse.Create(__instance).Field<Vector3>("endPos").Value;
        CombatGameState Combat = Traverse.Create(__instance).Field<CombatGameState>("Combat").Value;
        foreach (Collider collider in Physics.OverlapSphere(endPos, AOERange, -5, QueryTriggerInteraction.Ignore)) {
          DestructibleObject component = collider.gameObject.GetComponent<DestructibleObject>();
          if (component != null && component.isFlimsy) {
            Vector3 normalized = (collider.transform.position - endPos).normalized;
            float forceMagnitude = __instance.weapon.DamagePerShot + Combat.Constants.ResolutionConstants.FlimsyDestructionForceMultiplier;
            component.TakeDamage(endPos, normalized, forceMagnitude);
            component.Collapse(normalized, forceMagnitude);
          }
        }
        return;
      }
    }
    [HarmonyPatch(typeof(MissileEffect))]
    [HarmonyPatch("PlayImpact")]
    [HarmonyPatch(new Type[] { })]
    public static class MissileEffect_PlayImpactScorch {
      [HarmonyPriority(Priority.First)]
      public static void Postfix(WeaponEffect __instance) {
        if (__instance.weapon.AOECapable() == false) { return; };
        int hitIndex = __instance.hitIndex;
        float AOERange = __instance.weapon.AOERange();
        Vector3 endPos = Traverse.Create(__instance).Field<Vector3>("endPos").Value;
        CombatGameState Combat = Traverse.Create(__instance).Field<CombatGameState>("Combat").Value;
        float num3 = AOERange;
        FootstepManager.Instance.AddScorch(endPos, new Vector3(Random.Range(0.0f, 1f), 0.0f, Random.Range(0.0f, 1f)).normalized, new Vector3(num3, num3, num3), false);
        return;
      }
    }
  }
}