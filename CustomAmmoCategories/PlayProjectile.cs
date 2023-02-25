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
    public static bool Prefix(MissileEffect __instance) {
      bool isIndirect = (bool)typeof(MissileEffect).GetField("isIndirect", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      CustomAmmoCategoriesLog.Log.LogWrite("MissileEffect.PlayProjectile "+__instance.weapon.UIName+" real isIndirect = " + isIndirect+"\n");
      if (__instance.weapon.AlwaysIndirectVisuals() == true) {
        CustomAmmoCategoriesLog.Log.LogWrite(" always indirect\n");
        typeof(MissileEffect).GetField("isIndirect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)true);
      }
      return true;
    }
  }
  public static class WeaponEffect_PlayProjectile {
    public static bool Prefix(WeaponEffect __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayProjectile recoil\n");
      try {
        //__instance.t = 0.0f;
        typeof(WeaponEffect).GetField("t", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, 0.0f);
        __instance.currentState = WeaponEffect.WeaponEffectState.Firing;
        GameObject projectileMeshObject = (GameObject)typeof(WeaponEffect).GetField("projectileMeshObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if ((UnityEngine.Object)projectileMeshObject != (UnityEngine.Object)null) {
          projectileMeshObject.SetActive(true);
        }
        GameObject projectileLightObject = (GameObject)typeof(WeaponEffect).GetField("projectileLightObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if ((UnityEngine.Object)projectileLightObject != (UnityEngine.Object)null) {
          projectileLightObject.SetActive(true);
        }
        ParticleSystem projectileParticles = (ParticleSystem)typeof(WeaponEffect).GetField("projectileParticles", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if ((UnityEngine.Object)projectileParticles != (UnityEngine.Object)null) {
          projectileParticles.Stop(true);
          projectileParticles.Clear(true);
        }
        Transform projectileTransform = (Transform)typeof(WeaponEffect).GetField("projectileTransform", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        Transform startingTransform = (Transform)typeof(WeaponEffect).GetField("startingTransform", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        projectileTransform.position = startingTransform.position;
        Vector3 endPos = (Vector3)typeof(WeaponEffect).GetField("endPos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        projectileTransform.LookAt(endPos);
        typeof(WeaponEffect).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, startingTransform.position);
        //__instance.startPos = __instance.startingTransform.position;
        if ((UnityEngine.Object)projectileParticles != (UnityEngine.Object)null) {
          BTCustomRenderer.SetVFXMultiplier(projectileParticles);
          projectileParticles.Play(true);
          BTLightAnimator componentInChildren = projectileParticles.GetComponentInChildren<BTLightAnimator>(true);
          if ((UnityEngine.Object)componentInChildren != (UnityEngine.Object)null) {
            componentInChildren.StopAnimation();
            componentInChildren.PlayAnimation();
          }
        }
        if ((UnityEngine.Object)__instance.weapon.parent.GameRep != (UnityEngine.Object)null) {
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
        int hitIndex = __instance.HitIndex();
        if (hitIndex >= 0) {
          if (!__instance.AllowMissSkipping || __instance.hitInfo.hitLocations[hitIndex] != 0 && __instance.hitInfo.hitLocations[hitIndex] != 65536) {
            return false;
          }
        }
        __instance.PublishWeaponCompleteMessage();
      } catch (Exception e) {
        CustomAmmoCategoriesLog.Log.LogWrite("Exception " + e.ToString() + "\nFallback to default\n");
        return true;
      }
      return false;
    }
  }
}

