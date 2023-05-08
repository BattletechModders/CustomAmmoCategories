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
using HarmonyLib;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using BattleTech.Rendering;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesHelper;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static int AttackRecoil(this Weapon weapon) {
      return weapon.weaponDef.AttackRecoil + weapon.ammo().AttackRecoil;
    }
    public static bool AlwaysIndirectVisuals(this Weapon weapon) {
      ExtAmmunitionDef ammo = weapon.ammo();
      if(ammo.AlwaysIndirectVisuals != TripleBoolean.NotSet) { return ammo.AlwaysIndirectVisuals == TripleBoolean.True; }
      WeaponMode mode = weapon.mode();
      if (mode.AlwaysIndirectVisuals != TripleBoolean.NotSet) { return mode.AlwaysIndirectVisuals == TripleBoolean.True; }
      return weapon.exDef().AlwaysIndirectVisuals == TripleBoolean.True;
    }
  }
}
namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_PlayProjectile {
    public static void Prefix(MissileEffect __instance) {
      bool isIndirect = (bool)typeof(MissileEffect).GetField("isIndirect", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      Log.Combat?.WL(0,"MissileEffect.PlayProjectile "+__instance.weapon.UIName+" real isIndirect = " + isIndirect);
      if (__instance.weapon.AlwaysIndirectVisuals() == true) {
        Log.Combat?.WL(1, "always indirect");
        __instance.isIndirect = true;
      }
    }
  }
  public static class WeaponEffect_PlayProjectile {
    public static void Prefix(ref bool __runOriginal, WeaponEffect __instance) {
      if (!__runOriginal) { return; }
      Log.Combat?.WL(0, "WeaponEffect.PlayProjectile recoil");
      try {
        //__instance.t = 0.0f;\
        __instance.t(0f);
        __instance.currentState = WeaponEffect.WeaponEffectState.Firing;
        GameObject projectileMeshObject = __instance.projectileMeshObject();
        if (projectileMeshObject != null) {
          projectileMeshObject.SetActive(true);
        }
        GameObject projectileLightObject = __instance.projectileLightObject();
        if (projectileLightObject != null) {
          projectileLightObject.SetActive(true);
        }
        ParticleSystem projectileParticles = __instance.projectileParticles();
        if (projectileParticles != null) {
          projectileParticles.Stop(true);
          projectileParticles.Clear(true);
        }
        Transform projectileTransform = __instance.projectileTransform();
        Transform startingTransform = __instance.startingTransform();
        projectileTransform.position = startingTransform.position;
        Vector3 endPos = __instance.endPos();
        projectileTransform.LookAt(endPos);
        __instance.startPos(startingTransform.position);
        if (projectileParticles != null) {
          BTCustomRenderer.SetVFXMultiplier(projectileParticles);
          projectileParticles.Play(true);
          BTLightAnimator componentInChildren = projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
          if (componentInChildren != null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
        }
        if (__instance.weapon.parent.GameRep != null) {
          int num;
          switch ((ChassisLocations)__instance.weapon.Location) {
            case ChassisLocations.LeftArm:
            num = 1;
            break;
            case ChassisLocations.RightArm:
            num = 2;
            break;
            default:
            num = 0;
            break;
          }
          __instance.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, __instance.weapon.AttackRecoil());
        }
        int hitIndex = __instance.hitIndex;
        if (hitIndex >= 0) {
          if (!__instance.AllowMissSkipping || __instance.hitInfo.hitLocations[hitIndex] != 0 && __instance.hitInfo.hitLocations[hitIndex] != 65536) {
            __runOriginal = false; return;
          }
        }
        __instance.PublishWeaponCompleteMessage();
      } catch (Exception e) {
        Log.Combat?.TWL(0,"Exception " + e.ToString() + "\nFallback to default");
        Weapon.logger.LogException(e);
      }
      __runOriginal = false; return;
    }
  }
}

