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
    public static CustomAudioSource AdditionalImpactSound(this Weapon weapon) {
      WeaponMode mode = weapon.mode();
      if (mode.AdditionalAudioEffect != null) {
        return mode.AdditionalAudioEffect;
      }
      ExtAmmunitionDef ammo = weapon.ammo();
      if (ammo.AdditionalAudioEffect != null) {
        return ammo.AdditionalAudioEffect;
      }
      ExtWeaponDef extWeapon = weapon.exDef();
      return extWeapon.AdditionalAudioEffect;
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
      CustomAudioSource snd = __instance.weapon.AdditionalImpactSound();
      if(snd != null) {
        Log.LogWrite(" additional sound found. Playing ... "+snd.id+"\n");
        uint testid = SceneSingletonBehavior<WwiseManager>.Instance.EnumValueToEventId<AudioEventList_explosion>(AudioEventList_explosion.explosion_large);
        Log.LogWrite(" additional sound found. Playing ... " + snd.id + " test id: "+testid+"\n");
        AkGameObj projectileAudioObject = (AkGameObj)typeof(WeaponEffect).GetField("projectileAudioObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        snd.play(projectileAudioObject);
        Log.LogWrite(" played\n");
      }
      return true;
    }
  }
}

