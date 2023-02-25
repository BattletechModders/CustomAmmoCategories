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
using CustomAmmoCategoriesLog;
using HarmonyLib;
using HBS;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static Dictionary<string, List<ObjectSpawnDataSelf>> additinalImpactEffects = new Dictionary<string, List<ObjectSpawnDataSelf>>();
    public static string getCACGUID(this Weapon weapon) {
      string wGUID;
      if (weapon.StatCollection.ContainsStatistic(CustomAmmoCategories.GUIDStatisticName) == false) {
        wGUID = Guid.NewGuid().ToString();
        weapon.StatCollection.AddStatistic<string>(CustomAmmoCategories.GUIDStatisticName, wGUID);
      } else {
        wGUID = weapon.StatCollection.GetStatistic(CustomAmmoCategories.GUIDStatisticName).Value<string>();
      }
      return wGUID;
    }
    public static void clearImpactVFX(this Weapon weapon) {
      string wGUID = weapon.getCACGUID();
      if (CustomAmmoCategories.additinalImpactEffects.ContainsKey(wGUID)) {
        List<ObjectSpawnDataSelf> vfxs = CustomAmmoCategories.additinalImpactEffects[wGUID];
        foreach(var vfx in vfxs) {
          vfx.CleanupSelf();
        }
        CustomAmmoCategories.additinalImpactEffects.Remove(wGUID);
      }
    }
    public static string AdditionalImpactEffect(this Weapon weapon, out Vector3 scale) {
      WeaponMode mode = weapon.mode();
      if (string.IsNullOrEmpty(mode.AdditionalImpactVFX) == false) {
        scale = new Vector3(mode.AdditionalImpactVFXScaleX, mode.AdditionalImpactVFXScaleY, mode.AdditionalImpactVFXScaleZ);
        return mode.AdditionalImpactVFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (string.IsNullOrEmpty(ammo.AdditionalImpactVFX) == false) {
        scale = new Vector3(ammo.AdditionalImpactVFXScaleX, ammo.AdditionalImpactVFXScaleY, ammo.AdditionalImpactVFXScaleZ);
        return ammo.AdditionalImpactVFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      scale = new Vector3(extWeapon.AdditionalImpactVFXScaleX, extWeapon.AdditionalImpactVFXScaleY, extWeapon.AdditionalImpactVFXScaleZ);
      return extWeapon.AdditionalImpactVFX;
    }
    public static string AdditionalImpactSound(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (string.IsNullOrEmpty(mode.AdditionalAudioEffect) == false) {
        return mode.AdditionalAudioEffect;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (string.IsNullOrEmpty(ammo.AdditionalAudioEffect) == false) {
        return ammo.AdditionalAudioEffect;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.AdditionalAudioEffect;
    }
    public static string preFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.preFireSFX != null) {
        return mode.preFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.preFireSFX != null) {
        return ammo.preFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.preFireSFX;
    }
    public static string fireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.fireSFX != null) {
        return mode.fireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.fireSFX != null) {
        return ammo.fireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.fireSFX;
    }
    public static string lastFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.fireSFX != null) {
        return mode.lastFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.lastFireSFX != null) {
        return ammo.lastFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.lastFireSFX;
    }
    public static string firstFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.firstFireSFX != null) {
        return mode.firstFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.firstFireSFX != null) {
        return ammo.firstFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.firstFireSFX;
    }
    //public static string longPreFireSFX(this Weapon weapon) {
    //  WeaponMode mode = weapon.mode();
    //  if (mode.longPreFireSFX != null) {
    //    return mode.longPreFireSFX;
    //  }
    //  ExtAmmunitionDef ammo = weapon.ammo();
    //  if (ammo.longPreFireSFX != null) {
    //    return ammo.longPreFireSFX;
    //  }
    //  ExtWeaponDef extWeapon = weapon.exDef();
    //  return extWeapon.longPreFireSFX;
    //}
    public static string firstPreFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.firstPreFireSFX != null) {
        return mode.firstPreFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.firstPreFireSFX != null) {
        return ammo.preFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.firstPreFireSFX;
    }
    public static string preFireStartSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.preFireStartSFX != null) {
        return mode.preFireStartSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.preFireStartSFX != null) {
        return ammo.preFireStartSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.preFireStartSFX;
    }
    public static string preFireStopSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.preFireStopSFX != null) {
        return mode.preFireStopSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.preFireStopSFX != null) {
        return ammo.preFireStopSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.preFireStopSFX;
    }
    public static string pulseSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.delayedSFX != null) {
        return mode.delayedSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.delayedSFX != null) {
        return ammo.delayedSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.delayedSFX;
    }
    public static float pulseSFXdelay(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.delayedSFXDelay > CustomAmmoCategories.Epsilon) {
        return mode.delayedSFXDelay;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.delayedSFXDelay > CustomAmmoCategories.Epsilon) {
        return ammo.delayedSFXDelay;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.delayedSFXDelay;
    }
    public static string lastPreFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.lastPreFireSFX != null) {
        return mode.lastPreFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.lastPreFireSFX != null) {
        return ammo.lastPreFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.lastPreFireSFX;
    }
    public static string projectilePreFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.projectilePreFireSFX != null) {
        return mode.projectilePreFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.projectilePreFireSFX != null) {
        return ammo.projectilePreFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.projectilePreFireSFX;
    }
    public static string projectileFireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.projectileFireSFX != null) {
        return mode.projectileFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.projectileFireSFX != null) {
        return ammo.projectileFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.projectileFireSFX;
    }
    public static string projectileStopSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.projectileStopSFX != null) {
        return mode.projectileStopSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.projectileStopSFX != null) {
        return ammo.projectileStopSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.projectileStopSFX;
    }
    public static string firingStartSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.firingStartSFX != null) {
        return mode.firingStartSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.firingStartSFX != null) {
        return ammo.firingStartSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.firingStartSFX;
    }
    public static string firingStopSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.firingStopSFX != null) {
        return mode.firingStopSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.firingStopSFX != null) {
        return ammo.firingStopSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.firingStopSFX;
    }
    public static float prefireDuration(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.prefireDuration > CustomAmmoCategories.Epsilon) {
        return mode.prefireDuration;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.prefireDuration > CustomAmmoCategories.Epsilon) {
        return ammo.prefireDuration;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.prefireDuration;
    }
    public static float ProjectileSpeed(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.ProjectileSpeed > CustomAmmoCategories.Epsilon) {
        return mode.ProjectileSpeed;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.ProjectileSpeed > CustomAmmoCategories.Epsilon) {
        return ammo.ProjectileSpeed;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.ProjectileSpeed;
    }
    public static float shotDelay(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.shotDelay > CustomAmmoCategories.Epsilon) {
        return mode.shotDelay;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.shotDelay > CustomAmmoCategories.Epsilon) {
        return ammo.shotDelay;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.shotDelay;
    }
    public static void SpawnAdditionalImpactEffect(this Weapon weapon,Vector3 pos) {
      Vector3 scale;
      string VFXprefab = weapon.AdditionalImpactEffect(out scale);
      if (string.IsNullOrEmpty(VFXprefab)) { return; }
      string wGUID = weapon.getCACGUID();
      List<ObjectSpawnDataSelf> effects = null;
      if (CustomAmmoCategories.additinalImpactEffects.ContainsKey(wGUID)) {
        effects = CustomAmmoCategories.additinalImpactEffects[wGUID];
      } else {
        effects = new List<ObjectSpawnDataSelf>();
        CustomAmmoCategories.additinalImpactEffects.Add(wGUID, effects);
      }
      Log.LogWrite("SpawnAdditionalImpactEffect:"+VFXprefab+" "+scale+"\n");
      ObjectSpawnDataSelf vfx = new ObjectSpawnDataSelf(VFXprefab, pos, Quaternion.identity, scale, true, false);
      vfx.SpawnSelf(weapon.parent.Combat);
      effects.Add(vfx);
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayImpactAudio")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayImpactAudio{
    public static bool Prefix(WeaponEffect __instance) {
      Log.LogWrite("WeaponEffect_PlayImpactAudio.Postfix\n");
      if (__instance.weapon == null) { return true; }
      string snd = __instance.weapon.AdditionalImpactSound();
      if(string.IsNullOrEmpty(snd) == false) {
        //AkGameObj projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        Vector3 hitPos = __instance.hitInfo.hitPositions[__instance.hitIndex];
        float distanceToCamera = Vector3.Distance(Camera.main.transform.position, hitPos);
        CustomSoundHelper.SpawnAudioEmitter(snd, hitPos, false);
        Log.LogWrite(" additional sound found. Playing ... " + snd + ":"+distanceToCamera+"\n");

        //uint num = WwiseManager.PostEvent(snd, projectileAudioObject, null, null);
        //Log.LogWrite(" played "+num+"\n");
      } else {
        Log.LogWrite(" no additional impact sound\n");
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(WeaponEffect))]
  [HarmonyPatch("PlayPreFire")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class WeaponEffect_PlayPrefireSound {
    public static bool Prefix(WeaponEffect __instance, ref string __state) {
      Log.M.TWL(0,"WeaponEffect_PlayPrefireSound.Prefix");
      if (__instance.weapon == null) { return true; }
      string snd = __instance.weapon.preFireSFX();
      __state = __instance.preFireSFX;
      Log.M.WL(1, "current sound:"+ __state);
      if (string.IsNullOrEmpty(snd) == false) {
        Log.M.WL(1, "replacing:" + snd);
        __instance.preFireSFX = snd;
      }
      return true;
    }
    public static void Postfix(WeaponEffect __instance, ref string __state) {
      Log.M.TWL(0, "WeaponEffect_PlayPrefireSound.Postfix");
      Log.M.WL(1, "current sound:" + __instance.preFireSFX + "->"+__state);
      __instance.preFireSFX = __state;
    }
  }
  [HarmonyPatch(typeof(WwiseManager))]
  [HarmonyPatch("PostEventByName")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(string), typeof(AkGameObj), typeof(AkCallbackManager.EventCallback), typeof(object) })]
  public static class WwiseManager_PostEventByName {
    public static void Prefix(WwiseManager __instance, string eventName, AkGameObj sourceObject, AkCallbackManager.EventCallback callback, object in_pCookie) {
      //Log.M.TWL(0, $"WwiseManager.PostEventByName {eventName}");
      //Log.M?.WL(0,Environment.StackTrace);
    }
  }
}

