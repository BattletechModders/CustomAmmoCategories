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
using Harmony;
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
      if (string.IsNullOrEmpty(mode.preFireSFX) == false) {
        return mode.preFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (string.IsNullOrEmpty(ammo.preFireSFX) == false) {
        return ammo.preFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.preFireSFX;
    }
    public static string fireSFX(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (string.IsNullOrEmpty(mode.fireSFX) == false) {
        return mode.preFireSFX;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (string.IsNullOrEmpty(ammo.fireSFX) == false) {
        return ammo.preFireSFX;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.fireSFX;
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
}

