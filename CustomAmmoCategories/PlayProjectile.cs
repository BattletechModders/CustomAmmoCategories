using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using BattleTech.Rendering;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static int getWeaponAttackRecoil(Weapon weapon) {
      CustomAmmoCategoriesLog.Log.LogWrite("getWeaponAttackRecoil " + weapon.UIName + "\n");
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == false) {
        CustomAmmoCategoriesLog.Log.LogWrite("  weapon has no ammo\n");
        return weapon.weaponDef.AttackRecoil;
      }
      string ammoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
      ExtAmmunitionDef extAmmo = CustomAmmoCategories.findExtAmmo(ammoId);
      CustomAmmoCategoriesLog.Log.LogWrite("  modified AttackRecoil\n");
      return weapon.weaponDef.AttackRecoil + extAmmo.AttackRecoil;
    }
    public static bool getWeaponAlwaysIndirectVisuals(Weapon weapon) {
      bool result = false;
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.AmmoIdStatName) == true) {
        string CurrentAmmoId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.AmmoIdStatName).Value<string>();
        ExtAmmunitionDef extAmmoDef = CustomAmmoCategories.findExtAmmo(CurrentAmmoId);
        if (extAmmoDef.AlwaysIndirectVisuals != TripleBoolean.NotSet) {
          result = (extAmmoDef.AlwaysIndirectVisuals == TripleBoolean.True);
        }
      }
      if (CustomAmmoCategories.checkExistance(weapon.StatCollection, CustomAmmoCategories.WeaponModeStatisticName) == true) {
        ExtWeaponDef extWeapon = CustomAmmoCategories.getExtWeaponDef(weapon.defId);
        string modeId = weapon.StatCollection.GetStatistic(CustomAmmoCategories.WeaponModeStatisticName).Value<string>();
        if (extWeapon.Modes.ContainsKey(modeId)) {
          WeaponMode mode = extWeapon.Modes[modeId];
          if (mode.AlwaysIndirectVisuals != TripleBoolean.NotSet) {
            result = (mode.AlwaysIndirectVisuals == TripleBoolean.True);
          }
        }
      }
      return result;
    }
  }
}


namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(MissileEffect))]
  [HarmonyPatch("PlayProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class MissileEffect_Fire {
    public static bool Prefix(MissileEffect __instance) {
      bool isIndirect = (bool)typeof(MissileEffect).GetField("isIndirect", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
      CustomAmmoCategoriesLog.Log.LogWrite("MissileEffect.PlayProjectile "+__instance.weapon.UIName+" real isIndirect = " + isIndirect+"\n");
      if (CustomAmmoCategories.getWeaponAlwaysIndirectVisuals(__instance.weapon) == true) {
        CustomAmmoCategoriesLog.Log.LogWrite(" always indirect\n");
        typeof(MissileEffect).GetField("isIndirect", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (object)true);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayProjectile")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayProjectile {
    public static bool Prefix(WeaponEffect __instance) {
      CustomAmmoCategoriesLog.Log.LogWrite("WeaponEffect.PlayProjectile");
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
          __instance.weapon.parent.GameRep.PlayFireAnim((AttackSourceLimb)num, CustomAmmoCategories.getWeaponAttackRecoil(__instance.weapon));
        }
        int hitIndex = (int)typeof(WeaponEffect).GetField("hitIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if (!__instance.AllowMissSkipping || __instance.hitInfo.hitLocations[hitIndex] != 0 && __instance.hitInfo.hitLocations[hitIndex] != 65536) {
          return false;
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

